using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarfBuzzSharp;
using Rend.Output.Image.Internal;
using Rend.Text;
using SkiaSharp;

namespace Rend.Output.Image
{
    /// <summary>
    /// Text shaper that uses HarfBuzz with custom font callbacks that query Skia/DirectWrite
    /// for glyph advances, matching Chrome's approach exactly.
    ///
    /// Chrome's pipeline (from Chromium source):
    ///   1. Font scale = SkiaScalarToHarfBuzzPosition(fontSize) = (int)(fontSize * 65536)
    ///      Source: harfbuzz_face.cc, GetScaledFont()
    ///   2. Advance callback: SkFont::getWidth(glyph) → SkiaScalarToHarfBuzzPosition(width)
    ///      = ClampTo(width * 65536)  [16.16 fixed-point, truncation not rounding]
    ///      Source: skia_text_metrics.cc, SkFontGetGlyphWidthForHarfBuzz()
    ///   3. When subpixel=true (our case), raw float advance is used directly (no rounding)
    ///   4. Output positions divided by 65536 to convert back to pixels
    ///
    /// Key insight: Chrome uses 16.16 fixed-point (1 shl 16 = 65536), NOT 26.6 (1 shl 6 = 64).
    /// </summary>
    public sealed class SkiaTextShaper : ITextShaper, IDisposable
    {
        private bool _disposed;
        private readonly object _cacheLock = new object();
        private readonly Dictionary<int, CachedShapingFont> _fontCache = new();

        // Per-shape-call state used by the HarfBuzz callback (set before Shape, read during callback).
        // Safe because Shape holds _cacheLock during the entire shape call.
        private SKFont? _activeSkFont;

        // Shared FontFunctions instance (immutable, reusable across all fonts).
        private readonly FontFunctions _fontFunctions;

        // Pre-allocated single-element arrays to avoid allocation in the hot callback path.
        [ThreadStatic] private static ushort[]? _singleGlyphId;
        [ThreadStatic] private static float[]? _singleAdvance;

        public SkiaTextShaper(SkiaFontMapper fontMapper)
        {
            if (fontMapper == null) throw new ArgumentNullException(nameof(fontMapper));

            // Create shared font functions with our custom advance callback.
            _fontFunctions = new FontFunctions();
            _fontFunctions.SetHorizontalGlyphAdvanceDelegate(GetHorizontalGlyphAdvance, null);
            _fontFunctions.MakeImmutable();
        }

        /// <summary>
        /// HarfBuzz callback: returns glyph horizontal advance from Skia/DirectWrite,
        /// converted to 16.16 fixed-point hb_position_t.
        /// Matches Chrome's SkFontGetGlyphWidthForHarfBuzz() → SkiaScalarToHarfBuzzPosition().
        /// Source: third_party/blink/renderer/platform/fonts/skia/skia_text_metrics.cc
        /// </summary>
        private int GetHorizontalGlyphAdvance(Font font, object? fontData, uint glyph)
        {
            var skFont = _activeSkFont;
            if (skFont == null)
                return 0;

            // Reuse thread-static arrays to avoid allocation per callback.
            var glyphId = _singleGlyphId ??= new ushort[1];
            var advance = _singleAdvance ??= new float[1];

            glyphId[0] = (ushort)glyph;
            skFont.GetGlyphWidths(glyphId, advance, null);

            // Chrome's SkiaScalarToHarfBuzzPosition(): ClampTo<int>(value * (1 << 16))
            // This is 16.16 fixed-point truncation (NOT rounding, NOT * 64).
            // When font.isSubpixel() is true (our case), the raw float advance is used
            // directly without SkScalarRoundToInt pre-processing.
            return (int)(advance[0] * 65536.0);
        }

        public ShapedTextRun Shape(string text, byte[] fontData, float fontSize,
                                     string? language = null, string? script = null)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (fontData == null) throw new ArgumentNullException(nameof(fontData));
            if (_disposed) throw new ObjectDisposedException(nameof(SkiaTextShaper));

            if (text.Length == 0)
                return new ShapedTextRun(Array.Empty<ShapedGlyph>(), text, fontSize, fontData);

            lock (_cacheLock)
            {
                var cached = GetOrCreateFont(fontData, fontSize);

                // Set the active SKFont for the callback to use during shaping.
                _activeSkFont = cached.SkFont;

                try
                {
                    using var buffer = new HarfBuzzSharp.Buffer();
                    buffer.AddUtf16(text);

                    if (language != null)
                        buffer.Language = new Language(language);

                    buffer.GuessSegmentProperties();

                    cached.HbFont.Shape(buffer);

                    var glyphInfos = buffer.GlyphInfos;
                    var glyphPositions = buffer.GlyphPositions;
                    int count = glyphInfos.Length;
                    var glyphs = new ShapedGlyph[count];

                    // HarfBuzz positions are in 16.16 fixed-point (fontSize * 65536).
                    // Divide by 65536 to convert to pixels.
                    const float scale = 65536f;

                    for (int i = 0; i < count; i++)
                    {
                        var info = glyphInfos[i];
                        var pos = glyphPositions[i];

                        glyphs[i] = new ShapedGlyph(
                            glyphId: info.Codepoint,
                            cluster: info.Cluster,
                            xAdvance: pos.XAdvance / scale,
                            yAdvance: pos.YAdvance / scale,
                            xOffset: pos.XOffset / scale,
                            yOffset: pos.YOffset / scale
                        );
                    }

                    return new ShapedTextRun(glyphs, text, fontSize, fontData);
                }
                finally
                {
                    _activeSkFont = null;
                }
            }
        }

        private CachedShapingFont GetOrCreateFont(byte[] fontData, float fontSize)
        {
            int key = RuntimeHelpers.GetHashCode(fontData);

            if (_fontCache.TryGetValue(key, out var cached))
            {
                // Update scale and SKFont size if fontSize changed.
                // Chrome: SkiaScalarToHarfBuzzPosition(fontSize) = (int)(fontSize * 65536)
                int newScale = (int)(fontSize * 65536f);
                if (cached.Scale != newScale)
                {
                    cached.HbFont.SetScale(newScale, newScale);
                    cached.Scale = newScale;

                    // Recreate SKFont at new size.
                    cached.SkFont.Dispose();
                    cached.SkFont = new SKFont(cached.Typeface, fontSize);
                    cached.SkFont.Subpixel = true;
                }
                return cached;
            }

            // Create HarfBuzz face/font.
            var handle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
            Blob? blob = null;
            Face? face = null;
            Font? hbFont = null;
            SKTypeface? typeface = null;
            SKFont? skFont = null;

            Font? parentFont = null;
            try
            {
                blob = new Blob(handle.AddrOfPinnedObject(), fontData.Length, MemoryMode.ReadOnly);
                face = new Face(blob, 0);

                // Create parent font with full OT functions (cmap, hmtx, GPOS, etc).
                parentFont = new Font(face);
                // Chrome: SkiaScalarToHarfBuzzPosition(fontSize) = (int)(fontSize * 65536)
                // This is 16.16 fixed-point scale, NOT 26.6.
                int fontScale = (int)(fontSize * 65536f);
                parentFont.SetScale(fontScale, fontScale);

                // Create sub-font that inherits parent's OT functions but overrides
                // the horizontal advance callback with our Skia/DirectWrite version.
                // This matches Chrome's approach: custom hb_font_funcs for advances,
                // all other functions fall back to the parent (OT) font.
                hbFont = new Font(parentFont);
                hbFont.SetScale(fontScale, fontScale);
                hbFont.SetFontFunctions(_fontFunctions, null);

                // Create Skia typeface/font for advance queries.
                using var skData = SKData.CreateCopy(fontData);
                typeface = SKTypeface.FromData(skData) ?? SKTypeface.Default;

                skFont = new SKFont(typeface, fontSize);
                skFont.Subpixel = true;
                // Chrome's InitSkiaFont() does NOT set hinting — uses Skia default (Normal)
                // Do NOT set hinting explicitly to match Chrome's behavior.

                var entry = new CachedShapingFont(handle, blob, face, parentFont, hbFont, typeface, skFont, fontScale);
                _fontCache[key] = entry;
                return entry;
            }
            catch
            {
                skFont?.Dispose();
                typeface?.Dispose();
                hbFont?.Dispose();
                parentFont?.Dispose();
                face?.Dispose();
                blob?.Dispose();
                if (handle.IsAllocated) handle.Free();
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            lock (_cacheLock)
            {
                foreach (var kvp in _fontCache)
                    kvp.Value.Dispose();
                _fontCache.Clear();
            }

            _fontFunctions.Dispose();
        }

        private sealed class CachedShapingFont : IDisposable
        {
            private readonly GCHandle _handle;
            private readonly Blob _blob;
            private readonly Face _face;
            private readonly Font _parentFont;

            public Font HbFont { get; }
            public SKTypeface Typeface { get; }
            public SKFont SkFont { get; set; }
            public int Scale { get; set; }

            public CachedShapingFont(GCHandle handle, Blob blob, Face face, Font parentFont,
                                     Font hbFont, SKTypeface typeface, SKFont skFont, int scale)
            {
                _handle = handle;
                _blob = blob;
                _face = face;
                _parentFont = parentFont;
                HbFont = hbFont;
                Typeface = typeface;
                SkFont = skFont;
                Scale = scale;
            }

            public void Dispose()
            {
                SkFont.Dispose();
                if (Typeface != SKTypeface.Default)
                    Typeface.Dispose();
                HbFont.Dispose();
                _parentFont.Dispose();
                _face.Dispose();
                _blob.Dispose();
                if (_handle.IsAllocated) _handle.Free();
            }
        }
    }
}
