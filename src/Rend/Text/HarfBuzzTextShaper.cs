using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarfBuzzSharp;

namespace Rend.Text
{
    /// <summary>
    /// Text shaper implementation using HarfBuzzSharp for OpenType text shaping.
    /// Caches HarfBuzz Face and Font objects by font data reference to avoid re-parsing.
    /// </summary>
    public sealed class HarfBuzzTextShaper : ITextShaper, IDisposable
    {
        private readonly object _cacheLock = new object();
        private readonly Dictionary<int, CachedFont> _fontCache = new Dictionary<int, CachedFont>();
        private bool _disposed;

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

            var cached = GetOrCreateFont(fontData, fontSize);

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

                cached.Font.Shape(buffer);

                var glyphInfos = buffer.GlyphInfos;
                var glyphPositions = buffer.GlyphPositions;

                int count = glyphInfos.Length;
                var glyphs = new ShapedGlyph[count];

                // HarfBuzz positions are in 26.6 fixed-point format (multiplied by 64)
                // because we set the font scale to fontSize * 64.
                // We divide by 64 to convert back to pixels.
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

                return new ShapedTextRun(glyphs, text, fontSize, fontData);
            }
        }

        private CachedFont GetOrCreateFont(byte[] fontData, float fontSize)
        {
            lock (_cacheLock)
            {
                // Use object identity hash code as cache key. Same byte[] instance maps
                // to the same cached HarfBuzz Face/Font. Different arrays with the same
                // content will create separate cache entries, which is acceptable.
                int key = RuntimeHelpers.GetHashCode(fontData);

                if (_fontCache.TryGetValue(key, out var cached))
                {
                    int scale = (int)(fontSize * 64f);
                    if (cached.Scale != scale)
                    {
                        cached.Font.SetScale(scale, scale);
                        cached.Scale = scale;
                    }
                    return cached;
                }

                var handle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
                Blob? blob = null;
                Face? face = null;
                Font? font = null;

                try
                {
                    blob = new Blob(handle.AddrOfPinnedObject(), fontData.Length, MemoryMode.ReadOnly);
                    face = new Face(blob, 0);
                    font = new Font(face);

                    int fontScale = (int)(fontSize * 64f);
                    font.SetScale(fontScale, fontScale);

                    var entry = new CachedFont(handle, blob, face, font, fontScale);
                    _fontCache[key] = entry;
                    return entry;
                }
                catch
                {
                    font?.Dispose();
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

            // Build a four-character tag from the script string (ISO 15924).
            char c0 = script[0];
            char c1 = script[1];
            char c2 = script[2];
            char c3 = script[3];
            uint tag = (uint)((c0 << 24) | (c1 << 16) | (c2 << 8) | c3);
            return (Script)tag;
        }

        /// <summary>
        /// Releases all cached HarfBuzz resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            lock (_cacheLock)
            {
                foreach (var kvp in _fontCache)
                {
                    kvp.Value.Dispose();
                }
                _fontCache.Clear();
            }
        }

        private sealed class CachedFont : IDisposable
        {
            private readonly GCHandle _handle;
            private readonly Blob _blob;
            private readonly Face _face;

            public Font Font { get; }
            public int Scale { get; set; }

            public CachedFont(GCHandle handle, Blob blob, Face face, Font font, int scale)
            {
                _handle = handle;
                _blob = blob;
                _face = face;
                Font = font;
                Scale = scale;
            }

            public void Dispose()
            {
                Font.Dispose();
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
