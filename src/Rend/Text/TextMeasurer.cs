using System;
using Rend.Fonts;

namespace Rend.Text
{
    /// <summary>
    /// Provides text measurement capabilities by combining font resolution with text shaping.
    /// </summary>
    public sealed class TextMeasurer
    {
        private readonly IFontProvider _fontProvider;
        private readonly ITextShaper _textShaper;

        /// <summary>
        /// Creates a new <see cref="TextMeasurer"/>.
        /// </summary>
        /// <param name="fontProvider">The font provider for resolving font descriptors to font data.</param>
        /// <param name="textShaper">The text shaper for producing glyph runs.</param>
        public TextMeasurer(IFontProvider fontProvider, ITextShaper textShaper)
        {
            _fontProvider = fontProvider ?? throw new ArgumentNullException(nameof(fontProvider));
            _textShaper = textShaper ?? throw new ArgumentNullException(nameof(textShaper));
        }

        /// <summary>
        /// Measures the total advance width of the given text in pixels.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="font">The font descriptor.</param>
        /// <param name="fontSize">The font size in pixels.</param>
        /// <returns>The total width in pixels.</returns>
        public float MeasureWidth(string text, FontDescriptor font, float fontSize)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            if (text.Length == 0)
            {
                return 0f;
            }

            var run = Shape(text, font, fontSize);
            return run.TotalWidth;
        }

        /// <summary>
        /// Measures the advance width of each character in the text.
        /// For characters that map to multiple glyphs (or ligatures that combine characters),
        /// the width is distributed evenly across the cluster.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="font">The font descriptor.</param>
        /// <param name="fontSize">The font size in pixels.</param>
        /// <returns>An array of widths, one per character in the input text.</returns>
        public float[] MeasureCharacterWidths(string text, FontDescriptor font, float fontSize)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var widths = new float[text.Length];

            if (text.Length == 0)
            {
                return widths;
            }

            var run = Shape(text, font, fontSize);
            var glyphs = run.Glyphs;

            // Distribute glyph advances across their character clusters.
            // Multiple glyphs may belong to the same cluster (complex scripts),
            // or one glyph may span multiple characters (ligatures).
            for (int i = 0; i < glyphs.Length; i++)
            {
                uint cluster = glyphs[i].Cluster;
                float advance = glyphs[i].XAdvance;

                // Determine the cluster span: find the next different cluster value.
                uint nextCluster = (uint)text.Length;
                for (int j = i + 1; j < glyphs.Length; j++)
                {
                    if (glyphs[j].Cluster != cluster)
                    {
                        nextCluster = glyphs[j].Cluster;
                        break;
                    }
                }

                // Sum advances from all glyphs in this cluster.
                float clusterAdvance = advance;
                while (i + 1 < glyphs.Length && glyphs[i + 1].Cluster == cluster)
                {
                    i++;
                    clusterAdvance += glyphs[i].XAdvance;
                }

                // Determine char range for this cluster.
                uint clusterStart = cluster;
                uint clusterEnd = nextCluster;

                // Ensure cluster indices are within bounds.
                if (clusterStart >= (uint)text.Length)
                {
                    continue;
                }

                if (clusterEnd > (uint)text.Length)
                {
                    clusterEnd = (uint)text.Length;
                }

                int charCount = (int)(clusterEnd - clusterStart);
                if (charCount <= 0)
                {
                    charCount = 1;
                }

                float perChar = clusterAdvance / charCount;
                for (int c = (int)clusterStart; c < (int)clusterEnd && c < text.Length; c++)
                {
                    widths[c] = perChar;
                }
            }

            return widths;
        }

        /// <summary>
        /// Gets the normal line height for the given font and size, computed from actual font metrics.
        /// Returns NaN if the font cannot be resolved.
        /// </summary>
        public float GetNormalLineHeight(FontDescriptor font, float fontSize)
        {
            var metrics = _fontProvider.GetMetrics(font);
            float lh = metrics.GetLineHeight(fontSize);
            return lh > 0 ? lh : float.NaN;
        }

        /// <summary>
        /// Gets the typographic ascent for the given font and size.
        /// Returns fontSize * 0.8 as fallback if the font cannot be resolved.
        /// </summary>
        public float GetAscent(FontDescriptor font, float fontSize)
        {
            var metrics = _fontProvider.GetMetrics(font);
            float a = metrics.GetAscent(fontSize);
            return a > 0 ? a : fontSize * 0.8f;
        }

        /// <summary>
        /// Shapes the text using the resolved font and returns the full shaped run.
        /// </summary>
        /// <param name="text">The text to shape.</param>
        /// <param name="font">The font descriptor.</param>
        /// <param name="fontSize">The font size in pixels.</param>
        /// <returns>The shaped text run.</returns>
        public ShapedTextRun Shape(string text, FontDescriptor font, float fontSize)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var entry = _fontProvider.ResolveFont(font);
            if (entry == null)
            {
                // If no font is resolved, return an empty run.
                return new ShapedTextRun(Array.Empty<ShapedGlyph>(), text, fontSize);
            }

            return _textShaper.Shape(text, entry.FontData, fontSize);
        }
    }
}
