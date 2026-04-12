using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarfBuzzSharp;
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
    ///   2. Advance callback: SkFont::getWidth(glyph) -> SkiaScalarToHarfBuzzPosition(width)
    ///      = ClampTo(width * 65536)  [16.16 fixed-point, truncation not rounding]
    ///      Source: skia_text_metrics.cc, SkFontGetGlyphWidthForHarfBuzz()
    ///   3. When subpixel=true (our case), raw float advance is used directly (no rounding)
    ///   4. Output positions divided by 65536 to convert back to pixels
    ///
    /// Key insight: Chrome uses 16.16 fixed-point (1 shl 16 = 65536), NOT 26.6 (1 shl 6 = 64).
    /// </summary>
    internal sealed class SkiaTextShaper : ITextShaper, IDisposable
    {
        private bool _disposed;
        private readonly object _cacheLock = new object();
        private readonly Dictionary<int, CachedShapingFont> _fontCache = new();
        private readonly SkiaFontMapper _mapper;

        // Instance-level cache for fallback font data extracted from system typefaces.
        private readonly Dictionary<string, byte[]> _fontDataCache = new();
        private readonly object _fontDataLock = new();

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
            if (fontMapper == null)
            {
                throw new ArgumentNullException(nameof(fontMapper));
            }

            _mapper = fontMapper;

            // Create shared font functions with our custom advance callback.
            _fontFunctions = new FontFunctions();
            _fontFunctions.SetHorizontalGlyphAdvanceDelegate(GetHorizontalGlyphAdvance, null);
            _fontFunctions.MakeImmutable();
        }

        /// <summary>
        /// HarfBuzz callback: returns glyph horizontal advance from Skia/DirectWrite,
        /// converted to 16.16 fixed-point hb_position_t.
        /// Matches Chrome's SkFontGetGlyphWidthForHarfBuzz() -> SkiaScalarToHarfBuzzPosition().
        /// Source: third_party/blink/renderer/platform/fonts/skia/skia_text_metrics.cc
        /// </summary>
        private int GetHorizontalGlyphAdvance(Font font, object? fontData, uint glyph)
        {
            var skFont = _activeSkFont;
            if (skFont == null)
            {
                return 0;
            }

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
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (fontData == null)
            {
                throw new ArgumentNullException(nameof(fontData));
            }
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SkiaTextShaper));
            }

            if (text.Length == 0)
            {
                return new ShapedTextRun(Array.Empty<ShapedGlyph>(), text, fontSize, fontData);
            }

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
                    {
                        buffer.Language = new Language(language);
                    }

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

                    // Check for .notdef glyphs (glyph ID 0) that need font fallback.
                    bool hasNotdef = false;
                    float notdefAdvance = 0;
                    for (int i = 0; i < count; i++)
                    {
                        if (glyphs[i].GlyphId == 0)
                        {
                            if (!hasNotdef)
                            {
                                notdefAdvance = glyphs[i].XAdvance;
                            }
                            hasNotdef = true;
                            break;
                        }
                    }

                    if (!hasNotdef)
                    {
                        return new ShapedTextRun(glyphs, text, fontSize, fontData);
                    }

                    // Font fallback: find alternative fonts for .notdef glyphs.
                    var fontOverrides = new byte[]?[count];
                    int i2 = 0;
                    while (i2 < count)
                    {
                        if (glyphs[i2].GlyphId != 0)
                        {
                            i2++;
                            continue;
                        }

                        // Find the character for this .notdef glyph.
                        int charIndex = (int)glyphs[i2].Cluster;
                        if (charIndex >= text.Length)
                        {
                            i2++;
                            continue;
                        }

                        int codepoint = char.IsHighSurrogate(text[charIndex]) && charIndex + 1 < text.Length
                            ? char.ConvertToUtf32(text[charIndex], text[charIndex + 1])
                            : text[charIndex];

                        // Find a fallback typeface for this character.
                        using var fallbackTypeface = SKFontManager.Default.MatchCharacter(codepoint);
                        if (fallbackTypeface == null)
                        {
                            i2++;
                            continue;
                        }

                        // Get font data from the fallback typeface.
                        byte[]? fallbackData = GetFontDataFromTypeface(fallbackTypeface);
                        if (fallbackData == null)
                        {
                            i2++;
                            continue;
                        }

                        // Find the contiguous range of .notdef glyphs that this fallback covers.
                        // Only group characters that the SAME fallback font can handle.
                        int rangeStart = i2;
                        int rangeEnd = i2 + 1;
                        while (rangeEnd < count && glyphs[rangeEnd].GlyphId == 0)
                        {
                            int nextCharIdx = (int)glyphs[rangeEnd].Cluster;
                            if (nextCharIdx < text.Length)
                            {
                                int nextCp = char.IsHighSurrogate(text[nextCharIdx]) && nextCharIdx + 1 < text.Length
                                    ? char.ConvertToUtf32(text[nextCharIdx], text[nextCharIdx + 1])
                                    : text[nextCharIdx];
                                using var probe = SKFontManager.Default.MatchCharacter(nextCp);
                                if (probe == null || probe.FamilyName != fallbackTypeface.FamilyName)
                                {
                                    break;
                                }
                            }
                            rangeEnd++;
                        }

                        // Extract the text substring for this range.
                        // For bidi text, cluster values may not be monotonically increasing
                        // (RTL segments have descending clusters), so we must handle both orders.
                        int textStart = (int)glyphs[rangeStart].Cluster;
                        int textEnd = rangeEnd < count
                            ? (int)glyphs[rangeEnd].Cluster
                            : text.Length;
                        if (textEnd < textStart)
                        {
                            int tmp = textStart;
                            textStart = textEnd;
                            textEnd = tmp;
                        }
                        textStart = Math.Max(0, Math.Min(textStart, text.Length));
                        textEnd = Math.Max(textStart, Math.Min(textEnd, text.Length));
                        string subText = text.Substring(textStart, textEnd - textStart);

                        // Shape the substring with the fallback font.
                        var fallbackCached = GetOrCreateFont(fallbackData, fontSize);
                        _activeSkFont = fallbackCached.SkFont;

                        using var fbBuffer = new HarfBuzzSharp.Buffer();
                        fbBuffer.AddUtf16(subText);
                        fbBuffer.GuessSegmentProperties();
                        fallbackCached.HbFont.Shape(fbBuffer);

                        var fbInfos = fbBuffer.GlyphInfos;
                        var fbPositions = fbBuffer.GlyphPositions;

                        // Replace .notdef glyphs with fallback glyphs.
                        // The fallback may produce a different number of glyphs.
                        if (fbInfos.Length == rangeEnd - rangeStart)
                        {
                            // Same glyph count -- simple replacement.
                            for (int j = 0; j < fbInfos.Length; j++)
                            {
                                int idx = rangeStart + j;
                                int ci = (int)glyphs[idx].Cluster;
                                float advance = fbPositions[j].XAdvance / scale;
                                // [CSS-TEXT-3 §4.1.3] Space separators (Zs) use primary font's
                                // advance, not fallback. Chrome shapes all chars with the
                                // declared font; space width must match for correct wrapping.
                                if (ci < text.Length && IsUnicodeSpaceSeparator(text[ci]))
                                {
                                    advance = notdefAdvance;
                                }
                                glyphs[idx] = new ShapedGlyph(
                                    glyphId: fbInfos[j].Codepoint,
                                    cluster: glyphs[idx].Cluster,
                                    xAdvance: advance,
                                    yAdvance: fbPositions[j].YAdvance / scale,
                                    xOffset: fbPositions[j].XOffset / scale,
                                    yOffset: fbPositions[j].YOffset / scale
                                );
                                fontOverrides[idx] = fallbackData;
                            }
                        }
                        else
                        {
                            // Different glyph count -- rebuild arrays.
                            int newCount = count - (rangeEnd - rangeStart) + fbInfos.Length;
                            var newGlyphs = new ShapedGlyph[newCount];
                            var newOverrides = new byte[]?[newCount];

                            // Copy glyphs before the range.
                            Array.Copy(glyphs, 0, newGlyphs, 0, rangeStart);
                            Array.Copy(fontOverrides, 0, newOverrides, 0, rangeStart);

                            // Insert fallback glyphs.
                            for (int j = 0; j < fbInfos.Length; j++)
                            {
                                uint cluster = (uint)(textStart + (int)fbInfos[j].Cluster);
                                float advance = fbPositions[j].XAdvance / scale;
                                int ci = (int)cluster;
                                if (ci < text.Length && IsUnicodeSpaceSeparator(text[ci]))
                                {
                                    advance = notdefAdvance;
                                }
                                newGlyphs[rangeStart + j] = new ShapedGlyph(
                                    glyphId: fbInfos[j].Codepoint,
                                    cluster: cluster,
                                    xAdvance: advance,
                                    yAdvance: fbPositions[j].YAdvance / scale,
                                    xOffset: fbPositions[j].XOffset / scale,
                                    yOffset: fbPositions[j].YOffset / scale
                                );
                                newOverrides[rangeStart + j] = fallbackData;
                            }

                            // Copy glyphs after the range.
                            int afterCount = count - rangeEnd;
                            if (afterCount > 0)
                            {
                                Array.Copy(glyphs, rangeEnd, newGlyphs, rangeStart + fbInfos.Length, afterCount);
                                Array.Copy(fontOverrides, rangeEnd, newOverrides, rangeStart + fbInfos.Length, afterCount);
                            }

                            glyphs = newGlyphs;
                            fontOverrides = newOverrides;
                            count = newCount;
                        }

                        // Restore the primary font as active.
                        _activeSkFont = cached.SkFont;
                        i2 = rangeStart + fbInfos.Length;
                    }

                    // Check if any overrides were actually set.
                    bool hasOverrides = false;
                    for (int k = 0; k < fontOverrides.Length; k++)
                    {
                        if (fontOverrides[k] != null)
                        {
                            hasOverrides = true;
                            break;
                        }
                    }

                    return new ShapedTextRun(glyphs, text, fontSize, fontData,
                        hasOverrides ? fontOverrides : null);
                }
                finally
                {
                    _activeSkFont = null;
                }
            }
        }

        private byte[]? GetFontDataFromTypeface(SKTypeface typeface)
        {
            string key = typeface.FamilyName + "|" + (int)typeface.FontStyle.Weight + "|" + (int)typeface.FontStyle.Slant;
            lock (_fontDataLock)
            {
                if (_fontDataCache.TryGetValue(key, out var cached))
                {
                    return cached;
                }
            }

            using var stream = typeface.OpenStream(out _);
            if (stream == null)
            {
                return null;
            }

            var data = new byte[stream.Length];
            int totalRead = 0;
            while (totalRead < data.Length)
            {
                int bytesRead = stream.Read(data, data.Length - totalRead);
                if (bytesRead <= 0)
                {
                    break;
                }
                totalRead += bytesRead;
            }
            if (totalRead < data.Length)
            {
                return null;
            }

            lock (_fontDataLock)
            {
                // Double-check: another thread may have added it.
                if (_fontDataCache.TryGetValue(key, out var existing))
                {
                    return existing;
                }
                if (_fontDataCache.Count >= 30)
                {
                    _fontDataCache.Clear();
                }
                _fontDataCache[key] = data;
            }
            return data;
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
                    cached.SkFont.LinearMetrics = true;
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

                // Use the mapper's instance-level typeface cache to ensure deterministic rendering.
                typeface = _mapper.GetOrCreateTypeface(fontData);

                skFont = new SKFont(typeface, fontSize);
                skFont.Subpixel = true;
                skFont.LinearMetrics = true;
                // Chrome's InitSkiaFont() sets both setSubpixel(true) and setLinearMetrics(true).
                // LinearMetrics prevents Skia from rounding advances at the font level.

                var entry = new CachedShapingFont(handle, blob, face, parentFont, hbFont, typeface, skFont, fontScale);

                // Cap cache size to prevent unbounded native memory growth.
                // Each entry holds HarfBuzz blob+face+font + SKFont (~5-20MB native).
                if (_fontCache.Count >= 10)
                {
                    // Evict ALL entries and start fresh. Simple and prevents any leak.
                    foreach (var kvp in _fontCache)
                    {
                        kvp.Value.Dispose();
                    }
                    _fontCache.Clear();
                }

                _fontCache[key] = entry;
                return entry;
            }
            catch
            {
                skFont?.Dispose();
                hbFont?.Dispose();
                parentFont?.Dispose();
                face?.Dispose();
                blob?.Dispose();
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
                throw;
            }
        }

        private static bool IsUnicodeSpaceSeparator(char ch)
        {
            return ch == '\u0020' || ch == '\u00A0' || ch == '\u1680' ||
                   (ch >= '\u2000' && ch <= '\u200A') ||
                   ch == '\u202F' || ch == '\u205F' || ch == '\u3000';
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            lock (_cacheLock)
            {
                foreach (var kvp in _fontCache)
                {
                    kvp.Value.Dispose();
                }
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
                // Do NOT dispose Typeface -- it's owned by the SkiaFontMapper's typeface cache.
                HbFont.Dispose();
                _parentFont.Dispose();
                _face.Dispose();
                _blob.Dispose();
                if (_handle.IsAllocated)
                {
                    _handle.Free();
                }
            }
        }
    }
}
