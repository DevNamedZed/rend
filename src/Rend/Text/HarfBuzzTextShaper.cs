using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarfBuzzSharp;
using SkiaSharp;

namespace Rend.Text
{
    /// <summary>
    /// Text shaper implementation using HarfBuzzSharp for OpenType text shaping.
    /// Caches HarfBuzz Face and Font objects by font data reference to avoid re-parsing.
    /// Includes font fallback: when a glyph is missing (.notdef, glyph ID 0), uses
    /// SKFontManager to find a system font that contains the character.
    /// </summary>
    internal sealed class HarfBuzzTextShaper : ITextShaper, IDisposable
    {
        private readonly object _cacheLock = new object();
        private readonly Dictionary<int, CachedFace> _faceCache = new Dictionary<int, CachedFace>();
        private bool _disposed;

        // Static cache for fallback font data extracted from system typefaces.
        private static readonly Dictionary<string, byte[]> s_fallbackFontDataCache = new();
        private static readonly object s_fallbackLock = new();

        /// <summary>
        /// Optional font provider used as secondary fallback when SKFontManager cannot
        /// find a system font for a missing character (e.g., in WASM environments).
        /// </summary>
        internal Fonts.IFontProvider? FallbackFontProvider { get; set; }

        /// <inheritdoc />
        public ShapedTextRun Shape(string text, byte[] fontData, float fontSize, string? language = null, string? script = null)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (fontData == null) throw new ArgumentNullException(nameof(fontData));
            if (_disposed) throw new ObjectDisposedException(nameof(HarfBuzzTextShaper));

            if (text.Length == 0)
            {
                return new ShapedTextRun(Array.Empty<ShapedGlyph>(), text, fontSize, fontData);
            }

            ShapedGlyph[] glyphs;
            int count;

            Face face = GetOrCreateFace(fontData);

            using (var font = new Font(face))
            {
                int scale = (int)(fontSize * 64f);
                font.SetScale(scale, scale);

                using (var buffer = new HarfBuzzSharp.Buffer())
                {
                    buffer.AddUtf16(text);

                    if (script != null)
                    {
                        buffer.Script = ParseScript(script);
                    }
                    if (language != null)
                    {
                        buffer.Language = new Language(language);
                    }

                    buffer.GuessSegmentProperties();
                    font.Shape(buffer);

                    var glyphInfos = buffer.GlyphInfos;
                    var glyphPositions = buffer.GlyphPositions;
                    count = glyphInfos.Length;
                    glyphs = new ShapedGlyph[count];

                    const float fixedPointScale = 64f;

                    for (int i = 0; i < count; i++)
                    {
                        var info = glyphInfos[i];
                        var pos = glyphPositions[i];

                        glyphs[i] = new ShapedGlyph(
                            glyphId: info.Codepoint,
                            cluster: info.Cluster,
                            xAdvance: pos.XAdvance / fixedPointScale,
                            yAdvance: pos.YAdvance / fixedPointScale,
                            xOffset: pos.XOffset / fixedPointScale,
                            yOffset: pos.YOffset / fixedPointScale
                        );
                    }
                }
            }

            // Check for .notdef glyphs (glyph ID 0) that need font fallback.
            bool hasNotdef = false;
            for (int i = 0; i < count; i++)
            {
                if (glyphs[i].GlyphId == 0)
                {
                    hasNotdef = true;
                    break;
                }
            }

            if (!hasNotdef)
                return new ShapedTextRun(glyphs, text, fontSize, fontData);

            // Font fallback: find alternative fonts for .notdef glyphs.
            // Guard with try-catch — SKFontManager may not work in all environments (e.g., WASM).
            try
            {
                return ShapeWithFallback(glyphs, count, text, fontSize, fontData);
            }
            catch
            {
                // Fallback failed (e.g., no system font manager in WASM).
                // Return the run as-is with .notdef glyphs.
                return new ShapedTextRun(glyphs, text, fontSize, fontData);
            }
        }

        private ShapedTextRun ShapeWithFallback(ShapedGlyph[] glyphs, int count, string text, float fontSize, byte[] fontData)
        {
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
                // Try system font manager first, then registered font collection.
                byte[]? fallbackData = null;
                using (var fallbackTypeface = SKFontManager.Default.MatchCharacter(codepoint))
                {
                    if (fallbackTypeface != null)
                    {
                        fallbackData = GetFontDataFromTypeface(fallbackTypeface);
                    }
                }
                if (fallbackData == null && FallbackFontProvider != null)
                {
                    fallbackData = FallbackFontProvider.FindFontDataForCharacter(codepoint);
                }
                if (fallbackData == null)
                {
                    i2++;
                    continue;
                }

                // Find the contiguous range of .notdef glyphs that this fallback covers.
                int rangeStart = i2;
                int rangeEnd = i2;
                while (rangeEnd < count && glyphs[rangeEnd].GlyphId == 0)
                    rangeEnd++;

                // Extract the text substring for this range.
                int textStart = (int)glyphs[rangeStart].Cluster;
                int textEnd = rangeEnd < count
                    ? (int)glyphs[rangeEnd].Cluster
                    : text.Length;
                string subText = text.Substring(textStart, textEnd - textStart);

                // Shape the substring with the fallback font.
                {
                    Face fallbackFace = GetOrCreateFace(fallbackData);

                    using var fallbackFont = new Font(fallbackFace);
                    int fallbackScale = (int)(fontSize * 64f);
                    fallbackFont.SetScale(fallbackScale, fallbackScale);

                    using var fbBuffer = new HarfBuzzSharp.Buffer();
                    fbBuffer.AddUtf16(subText);
                    fbBuffer.GuessSegmentProperties();
                    fallbackFont.Shape(fbBuffer);

                    var fbInfos = fbBuffer.GlyphInfos;
                    var fbPositions = fbBuffer.GlyphPositions;

                    const float fixedPointScale = 64f;

                    // Replace .notdef glyphs with fallback glyphs.
                    if (fbInfos.Length == rangeEnd - rangeStart)
                    {
                        // Same glyph count — simple replacement.
                        for (int j = 0; j < fbInfos.Length; j++)
                        {
                            int idx = rangeStart + j;
                            glyphs[idx] = new ShapedGlyph(
                                glyphId: fbInfos[j].Codepoint,
                                cluster: glyphs[idx].Cluster,
                                xAdvance: fbPositions[j].XAdvance / fixedPointScale,
                                yAdvance: fbPositions[j].YAdvance / fixedPointScale,
                                xOffset: fbPositions[j].XOffset / fixedPointScale,
                                yOffset: fbPositions[j].YOffset / fixedPointScale
                            );
                            fontOverrides[idx] = fallbackData;
                        }
                        i2 = rangeEnd;
                    }
                    else
                    {
                        // Different glyph count — rebuild arrays.
                        int newCount = count - (rangeEnd - rangeStart) + fbInfos.Length;
                        var newGlyphs = new ShapedGlyph[newCount];
                        var newOverrides = new byte[]?[newCount];

                        Array.Copy(glyphs, 0, newGlyphs, 0, rangeStart);
                        Array.Copy(fontOverrides, 0, newOverrides, 0, rangeStart);

                        for (int j = 0; j < fbInfos.Length; j++)
                        {
                            uint cluster = (uint)(textStart + (int)fbInfos[j].Cluster);
                            newGlyphs[rangeStart + j] = new ShapedGlyph(
                                glyphId: fbInfos[j].Codepoint,
                                cluster: cluster,
                                xAdvance: fbPositions[j].XAdvance / fixedPointScale,
                                yAdvance: fbPositions[j].YAdvance / fixedPointScale,
                                xOffset: fbPositions[j].XOffset / fixedPointScale,
                                yOffset: fbPositions[j].YOffset / fixedPointScale
                            );
                            newOverrides[rangeStart + j] = fallbackData;
                        }

                        int afterCount = count - rangeEnd;
                        if (afterCount > 0)
                        {
                            Array.Copy(glyphs, rangeEnd, newGlyphs, rangeStart + fbInfos.Length, afterCount);
                            Array.Copy(fontOverrides, rangeEnd, newOverrides, rangeStart + fbInfos.Length, afterCount);
                        }

                        glyphs = newGlyphs;
                        fontOverrides = newOverrides;
                        count = newCount;
                        i2 = rangeStart + fbInfos.Length;
                    }
                }
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

        private static byte[]? GetFontDataFromTypeface(SKTypeface typeface)
        {
            string key = typeface.FamilyName + "|" + (int)typeface.FontStyle.Weight + "|" + (int)typeface.FontStyle.Slant;
            lock (s_fallbackLock)
            {
                if (s_fallbackFontDataCache.TryGetValue(key, out var cached))
                    return cached;
            }

            using var stream = typeface.OpenStream(out _);
            if (stream == null) return null;

            var data = new byte[stream.Length];
            int totalRead = 0;
            while (totalRead < data.Length)
            {
                int bytesRead = stream.Read(data, data.Length - totalRead);
                if (bytesRead <= 0) break;
                totalRead += bytesRead;
            }
            if (totalRead < data.Length) return null;

            lock (s_fallbackLock)
            {
                if (s_fallbackFontDataCache.TryGetValue(key, out var existing))
                    return existing;
                s_fallbackFontDataCache[key] = data;
            }
            return data;
        }

        private Face GetOrCreateFace(byte[] fontData)
        {
            // Use object identity hash code as cache key. Same byte[] instance maps
            // to the same cached HarfBuzz Face. Different arrays with the same
            // content will create separate cache entries, which is acceptable.
            int key = RuntimeHelpers.GetHashCode(fontData);

            lock (_cacheLock)
            {
                if (_faceCache.TryGetValue(key, out var cached))
                {
                    return cached.Face;
                }

                var handle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
                Blob? blob = null;
                Face? face = null;

                try
                {
                    blob = new Blob(handle.AddrOfPinnedObject(), fontData.Length, MemoryMode.ReadOnly);
                    face = new Face(blob, 0);

                    var entry = new CachedFace(handle, blob, face);
                    _faceCache[key] = entry;
                    return face;
                }
                catch
                {
                    face?.Dispose();
                    blob?.Dispose();
                    if (handle.IsAllocated)
                    {
                        handle.Free();
                    }
                    throw;
                }
            }
        }

        private static Script ParseScript(string script)
        {
            if (script == null || script.Length < 4)
            {
                return Script.Common;
            }

            char c0 = script[0];
            char c1 = script[1];
            char c2 = script[2];
            char c3 = script[3];
            uint tag = (uint)((c0 << 24) | (c1 << 16) | (c2 << 8) | c3);
            return (Script)tag;
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
                foreach (var kvp in _faceCache)
                {
                    kvp.Value.Dispose();
                }
                _faceCache.Clear();
            }
        }

        private sealed class CachedFace : IDisposable
        {
            private readonly GCHandle _handle;
            private readonly Blob _blob;

            public Face Face { get; }

            public CachedFace(GCHandle handle, Blob blob, Face face)
            {
                _handle = handle;
                _blob = blob;
                Face = face;
            }

            public void Dispose()
            {
                Face.Dispose();
                _blob.Dispose();
                if (_handle.IsAllocated)
                {
                    _handle.Free();
                }
            }
        }
    }
}
