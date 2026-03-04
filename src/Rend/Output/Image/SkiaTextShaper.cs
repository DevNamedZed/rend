using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rend.Fonts;
using Rend.Output.Image.Internal;
using Rend.Text;
using SkiaSharp;

namespace Rend.Output.Image
{
    /// <summary>
    /// Text shaper that uses Skia's text measurement to produce widths that exactly
    /// match Skia's DrawText rendering. This eliminates layout/rendering mismatch
    /// where HarfBuzz and Skia disagree on glyph advances.
    /// HarfBuzz is still used for glyph decomposition (needed for PDF glyph embedding).
    /// </summary>
    internal sealed class SkiaTextShaper : ITextShaper, IDisposable
    {
        private readonly HarfBuzzTextShaper _harfBuzz = new HarfBuzzTextShaper();
        private readonly SkiaFontMapper _fontMapper;
        private readonly object _paintLock = new object();
        private readonly SKPaint _measurePaint;
        private bool _disposed;

        public SkiaTextShaper(SkiaFontMapper fontMapper)
        {
            _fontMapper = fontMapper ?? throw new ArgumentNullException(nameof(fontMapper));
            _measurePaint = new SKPaint
            {
                IsAntialias = true,
                SubpixelText = true,
                HintingLevel = SKPaintHinting.Slight,
                Style = SKPaintStyle.Fill
            };
        }

        public ShapedTextRun Shape(string text, byte[] fontData, float fontSize,
                                     string? language = null, string? script = null)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (fontData == null) throw new ArgumentNullException(nameof(fontData));
            if (_disposed) throw new ObjectDisposedException(nameof(SkiaTextShaper));

            // Get HarfBuzz shaping for glyph data
            var hbRun = _harfBuzz.Shape(text, fontData, fontSize, language, script);

            if (text.Length == 0 || hbRun.Glyphs.Length == 0)
                return hbRun;

            // Measure with Skia to get the actual rendered width
            float skiaWidth;
            lock (_paintLock)
            {
                _measurePaint.TextSize = fontSize;

                // Resolve typeface from font data
                var descriptor = new FontDescriptor("_measure", 400, Css.CssFontStyle.Normal);
                SKTypeface typeface = ResolveTypeface(fontData);
                _measurePaint.Typeface = typeface;

                skiaWidth = _measurePaint.MeasureText(text);
            }

            // Create a new ShapedTextRun with Skia's width but HarfBuzz glyph data.
            // Scale each glyph advance proportionally so they sum to skiaWidth.
            float hbWidth = hbRun.TotalWidth;
            if (hbWidth > 0 && Math.Abs(hbWidth - skiaWidth) > 0.01f)
            {
                float scale = skiaWidth / hbWidth;
                var scaledGlyphs = new ShapedGlyph[hbRun.Glyphs.Length];
                for (int i = 0; i < hbRun.Glyphs.Length; i++)
                {
                    var g = hbRun.Glyphs[i];
                    scaledGlyphs[i] = new ShapedGlyph(
                        g.GlyphId, g.Cluster,
                        g.XAdvance * scale, g.YAdvance,
                        g.XOffset * scale, g.YOffset);
                }
                return new ShapedTextRun(scaledGlyphs, text, fontSize, fontData);
            }

            return hbRun;
        }

        // Cache typefaces by font data identity (same approach as HarfBuzzTextShaper)
        private readonly Dictionary<int, SKTypeface> _typefaceCache = new Dictionary<int, SKTypeface>();

        private SKTypeface ResolveTypeface(byte[] fontData)
        {
            int key = RuntimeHelpers.GetHashCode(fontData);
            if (_typefaceCache.TryGetValue(key, out var cached))
                return cached;

            SKTypeface? tf = null;
            try
            {
                // Load typeface directly from font data bytes to ensure measurement
                // uses the exact same font as layout (HarfBuzz) and rendering.
                using var skData = SKData.CreateCopy(fontData);
                tf = SKTypeface.FromData(skData);
            }
            catch { }

            if (tf == null)
                tf = SKTypeface.Default;

            _typefaceCache[key] = tf;
            return tf;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _harfBuzz.Dispose();
            _measurePaint.Dispose();
            foreach (var kvp in _typefaceCache)
            {
                if (kvp.Value != SKTypeface.Default)
                    kvp.Value.Dispose();
            }
            _typefaceCache.Clear();
        }
    }
}
