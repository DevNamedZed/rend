using System;

namespace Rend.Text
{
    /// <summary>
    /// Contains the result of text shaping: an array of positioned glyphs
    /// along with aggregate metrics.
    /// </summary>
    public sealed class ShapedTextRun
    {
        /// <summary>Gets the shaped glyphs in visual order.</summary>
        public ShapedGlyph[] Glyphs { get; }

        /// <summary>Gets the total advance width of all glyphs in pixels.</summary>
        public float TotalWidth { get; }

        /// <summary>Gets the original input text that was shaped.</summary>
        public string OriginalText { get; }

        /// <summary>Gets the font size used for shaping.</summary>
        public float FontSize { get; }

        /// <summary>Gets the raw font data bytes used for shaping, or null if unavailable.</summary>
        public byte[]? FontData { get; }

        /// <summary>
        /// Per-glyph font data overrides for font fallback. Null if all glyphs use the primary font.
        /// When set, GlyphFontOverrides[i] is the font data for glyph i (null means primary font).
        /// </summary>
        public byte[]?[]? GlyphFontOverrides { get; }

        /// <summary>
        /// Creates a new <see cref="ShapedTextRun"/>.
        /// </summary>
        /// <param name="glyphs">The shaped glyphs.</param>
        /// <param name="originalText">The original input text.</param>
        /// <param name="fontSize">The font size used.</param>
        /// <param name="fontData">The raw font data bytes used for shaping.</param>
        /// <param name="glyphFontOverrides">Per-glyph font data overrides for fallback glyphs.</param>
        public ShapedTextRun(ShapedGlyph[] glyphs, string originalText, float fontSize, byte[]? fontData = null, byte[]?[]? glyphFontOverrides = null)
        {
            Glyphs = glyphs ?? throw new ArgumentNullException(nameof(glyphs));
            OriginalText = originalText ?? throw new ArgumentNullException(nameof(originalText));
            FontSize = fontSize;
            FontData = fontData;
            GlyphFontOverrides = glyphFontOverrides;

            float total = 0f;
            for (int i = 0; i < glyphs.Length; i++)
            {
                total += glyphs[i].XAdvance;
            }
            TotalWidth = total;
        }

        /// <summary>
        /// [CSS-TEXT-3 §8] Splits this shaped run at a character boundary.
        /// Returns two runs: glyphs for characters [0, charIndex) and [charIndex, end).
        /// Used for cross-element shaping where text is shaped as one unit then split
        /// at element boundaries (e.g. ::first-letter).
        /// </summary>
        public (ShapedTextRun first, ShapedTextRun second) SplitAtCharIndex(int charIndex)
        {
            if (charIndex <= 0)
            {
                var empty = new ShapedTextRun(System.Array.Empty<ShapedGlyph>(), "", FontSize, FontData);
                return (empty, this);
            }
            if (charIndex >= OriginalText.Length)
            {
                var empty = new ShapedTextRun(System.Array.Empty<ShapedGlyph>(), "", FontSize, FontData);
                return (this, empty);
            }

            // Find the split point in the glyph array based on cluster indices.
            // Glyphs with cluster < charIndex go to the first run.
            int splitGlyphIdx = Glyphs.Length;
            for (int i = 0; i < Glyphs.Length; i++)
            {
                if ((int)Glyphs[i].Cluster >= charIndex)
                {
                    splitGlyphIdx = i;
                    break;
                }
            }

            // For RTL text, clusters may be in reverse order.
            // Check if clusters are descending (RTL).
            bool isRtl = Glyphs.Length >= 2 && Glyphs[0].Cluster > Glyphs[Glyphs.Length - 1].Cluster;
            if (isRtl)
            {
                // RTL: glyphs with cluster >= charIndex come FIRST in the array
                splitGlyphIdx = 0;
                for (int i = Glyphs.Length - 1; i >= 0; i--)
                {
                    if ((int)Glyphs[i].Cluster >= charIndex)
                    {
                        splitGlyphIdx = i;
                        break;
                    }
                }
                // For RTL, first run = glyphs from splitGlyphIdx to end (high clusters)
                // second run = glyphs from 0 to splitGlyphIdx (low clusters)
                var firstGlyphs = new ShapedGlyph[Glyphs.Length - splitGlyphIdx];
                var secondGlyphs = new ShapedGlyph[splitGlyphIdx];
                System.Array.Copy(Glyphs, splitGlyphIdx, firstGlyphs, 0, firstGlyphs.Length);
                System.Array.Copy(Glyphs, 0, secondGlyphs, 0, secondGlyphs.Length);
                var firstRun = new ShapedTextRun(firstGlyphs, OriginalText.Substring(0, charIndex), FontSize, FontData);
                var secondRun = new ShapedTextRun(secondGlyphs, OriginalText.Substring(charIndex), FontSize, FontData);
                return (firstRun, secondRun);
            }

            // LTR: straightforward split
            var firstG = new ShapedGlyph[splitGlyphIdx];
            var secondG = new ShapedGlyph[Glyphs.Length - splitGlyphIdx];
            System.Array.Copy(Glyphs, 0, firstG, 0, splitGlyphIdx);
            System.Array.Copy(Glyphs, splitGlyphIdx, secondG, 0, secondG.Length);

            // Adjust cluster indices for the second run
            for (int i = 0; i < secondG.Length; i++)
            {
                secondG[i] = new ShapedGlyph(
                    secondG[i].GlyphId,
                    secondG[i].Cluster - (uint)charIndex,
                    secondG[i].XAdvance,
                    secondG[i].YAdvance,
                    secondG[i].XOffset,
                    secondG[i].YOffset);
            }

            var first = new ShapedTextRun(firstG, OriginalText.Substring(0, charIndex), FontSize, FontData);
            var second = new ShapedTextRun(secondG, OriginalText.Substring(charIndex), FontSize, FontData);
            return (first, second);
        }
    }
}
