namespace Rend.Text
{
    /// <summary>
    /// Shapes text using a given font, producing positioned glyph runs.
    /// </summary>
    public interface ITextShaper
    {
        /// <summary>
        /// Shapes the specified text using the provided font data and size.
        /// </summary>
        /// <param name="text">The text to shape.</param>
        /// <param name="fontData">Raw font file bytes (TrueType or OpenType).</param>
        /// <param name="fontSize">The font size in points/pixels.</param>
        /// <param name="language">Optional BCP 47 language tag.</param>
        /// <param name="script">Optional ISO 15924 script tag.</param>
        /// <returns>A shaped text run containing positioned glyphs.</returns>
        ShapedTextRun Shape(string text, byte[] fontData, float fontSize, string? language = null, string? script = null);
    }
}
