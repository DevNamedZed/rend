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
        /// Creates a new <see cref="ShapedTextRun"/>.
        /// </summary>
        /// <param name="glyphs">The shaped glyphs.</param>
        /// <param name="originalText">The original input text.</param>
        /// <param name="fontSize">The font size used.</param>
        /// <param name="fontData">The raw font data bytes used for shaping.</param>
        public ShapedTextRun(ShapedGlyph[] glyphs, string originalText, float fontSize, byte[]? fontData = null)
        {
            Glyphs = glyphs ?? throw new ArgumentNullException(nameof(glyphs));
            OriginalText = originalText ?? throw new ArgumentNullException(nameof(originalText));
            FontSize = fontSize;
            FontData = fontData;

            float total = 0f;
            for (int i = 0; i < glyphs.Length; i++)
            {
                total += glyphs[i].XAdvance;
            }
            TotalWidth = total;
        }
    }
}
