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

        // Instance-level cache for fallback font data extracted from system typefaces.
        private readonly Dictionary<string, byte[]> _fallbackFontDataCache = new();
        private readonly object _fallbackFontDataLock = new();

        /// <summary>
        /// Optional font provider used as secondary fallback when SKFontManager cannot
        /// find a system font for a missing character (e.g., in WASM environments).
        /// </summary>
        internal Fonts.IFontProvider? FallbackFontProvider { get; set; }

        /// <summary>
        /// Optional sink for render diagnostics. When font fallback for a missing glyph
        /// fails (e.g. no system font manager in WASM), a warning is reported here instead
        /// of the failure being swallowed.
        /// </summary>
        internal Action<RenderDiagnostic>? OnDiagnostic { get; set; }

        /// <inheritdoc />
        public ShapedTextRun Shape(string text, byte[] fontData, float fontSize,
            string? language = null, string? script = null, string? fontFeatures = null)
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

                    var features = ParseFontFeatures(fontFeatures);
                    if (features != null)
                    {
                        font.Shape(buffer, features);
                    }
                    else
                    {
                        font.Shape(buffer);
                    }

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
                return new ShapedTextRun(glyphs, text, fontSize, fontData);

            // Font fallback: find alternative fonts for .notdef glyphs.
            // SKFontManager may not work in all environments (e.g., WASM). If fallback
            // fails, surface a warning and degrade to the unshaped run with .notdef
            // glyphs — never swallow the failure silently.
            try
            {
                return ShapeWithFallback(glyphs, count, text, fontSize, fontData, notdefAdvance);
            }
            catch (Exception fallbackException)
            {
                OnDiagnostic?.Invoke(new RenderDiagnostic(
                    RenderDiagnosticSeverity.Warning,
                    $"Font fallback failed for missing glyph(s) in text \"{TruncateForDiagnostic(text)}\": " +
                    $"{fallbackException.GetType().Name}: {fallbackException.Message}. " +
                    "Rendering with .notdef glyphs."));
                return new ShapedTextRun(glyphs, text, fontSize, fontData);
            }
        }

        private static string TruncateForDiagnostic(string text)
        {
            const int maxLength = 40;
            if (text.Length <= maxLength)
            {
                return text;
            }
            return text.Substring(0, maxLength) + "…";
        }

        private ShapedTextRun ShapeWithFallback(ShapedGlyph[] glyphs, int count, string text, float fontSize, byte[] fontData, float notdefAdvance)
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
                            int ci = (int)glyphs[idx].Cluster;
                            float advance = fbPositions[j].XAdvance / fixedPointScale;
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
                            float advance = fbPositions[j].XAdvance / fixedPointScale;
                            int ci = (int)cluster;
                            if (ci < text.Length && IsUnicodeSpaceSeparator(text[ci]))
                            {
                                advance = notdefAdvance;
                            }
                            newGlyphs[rangeStart + j] = new ShapedGlyph(
                                glyphId: fbInfos[j].Codepoint,
                                cluster: cluster,
                                xAdvance: advance,
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

        private byte[]? GetFontDataFromTypeface(SKTypeface typeface)
        {
            string key = typeface.FamilyName + "|" + (int)typeface.FontStyle.Weight + "|" + (int)typeface.FontStyle.Slant;
            lock (_fallbackFontDataLock)
            {
                if (_fallbackFontDataCache.TryGetValue(key, out var cached))
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

            lock (_fallbackFontDataLock)
            {
                if (_fallbackFontDataCache.TryGetValue(key, out var existing))
                {
                    return existing;
                }
                // Cap fallback font data cache — each entry is 200KB-2MB
                if (_fallbackFontDataCache.Count >= 30)
                {
                    _fallbackFontDataCache.Clear();
                }
                _fallbackFontDataCache[key] = data;
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

                // Evict oldest entries when cache exceeds limit.
                // Each entry pins a font byte[] (200KB-2MB) preventing GC compaction.
                const int maxCacheEntries = 20;
                if (_faceCache.Count >= maxCacheEntries)
                {
                    var firstKey = default(int);
                    foreach (var k in _faceCache.Keys)
                    {
                        firstKey = k;
                        break;
                    }
                    if (_faceCache.TryGetValue(firstKey, out var evicted))
                    {
                        evicted.Dispose();
                        _faceCache.Remove(firstKey);
                    }
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

        internal static Feature[]? ParseFontFeatures(string? fontFeatures)
        {
            if (string.IsNullOrWhiteSpace(fontFeatures) || fontFeatures == "normal")
            {
                return null;
            }

            var featureList = new List<Feature>();
            var entries = fontFeatures!.Split(',');

            for (int i = 0; i < entries.Length; i++)
            {
                string entry = entries[i].Trim();
                if (entry.Length == 0)
                {
                    continue;
                }

                // CSS font-feature-settings format: "tag" [value]
                // e.g., "liga" 1, "smcp", "kern" 0
                // Tag is 4 chars in quotes; value defaults to 1 if omitted.
                string tag;
                uint value = 1;

                // Extract quoted tag.
                int quoteStart = entry.IndexOf('"');
                if (quoteStart < 0)
                {
                    quoteStart = entry.IndexOf('\'');
                }
                if (quoteStart >= 0)
                {
                    char quoteChar = entry[quoteStart];
                    int quoteEnd = entry.IndexOf(quoteChar, quoteStart + 1);
                    if (quoteEnd < 0 || quoteEnd - quoteStart - 1 != 4)
                    {
                        continue;
                    }
                    tag = entry.Substring(quoteStart + 1, 4);

                    // Parse optional value after the closing quote.
                    string remainder = entry.Substring(quoteEnd + 1).Trim();
                    if (remainder.Length > 0)
                    {
                        if (remainder == "on")
                        {
                            value = 1;
                        }
                        else if (remainder == "off")
                        {
                            value = 0;
                        }
                        else if (uint.TryParse(remainder, out uint parsed))
                        {
                            value = parsed;
                        }
                    }
                }
                else
                {
                    // No quotes — try bare tag (e.g., from font-variant mapping).
                    var parts = entry.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0 || parts[0].Length != 4)
                    {
                        continue;
                    }
                    tag = parts[0];
                    if (parts.Length > 1)
                    {
                        if (parts[1] == "on")
                        {
                            value = 1;
                        }
                        else if (parts[1] == "off")
                        {
                            value = 0;
                        }
                        else if (uint.TryParse(parts[1], out uint parsed))
                        {
                            value = parsed;
                        }
                    }
                }

                if (tag.Length != 4)
                {
                    continue;
                }

                // Encode 4-char tag as uint32 (big-endian, per OpenType spec).
                uint tagValue = (uint)((tag[0] << 24) | (tag[1] << 16) | (tag[2] << 8) | tag[3]);

                featureList.Add(new Feature
                {
                    Tag = tagValue,
                    Value = value,
                    Start = 0,
                    End = uint.MaxValue
                });
            }

            return featureList.Count > 0 ? featureList.ToArray() : null;
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
