using Rend.Css;

namespace Rend.Fonts
{
    /// <summary>
    /// Represents a source of font data with associated metadata.
    /// </summary>
    public interface IFontSource
    {
        /// <summary>
        /// Gets the font family name.
        /// </summary>
        string FamilyName { get; }

        /// <summary>
        /// Gets the font weight (e.g., 400 for normal, 700 for bold).
        /// </summary>
        float Weight { get; }

        /// <summary>
        /// Gets the font style (Normal, Italic, or Oblique).
        /// </summary>
        CssFontStyle Style { get; }

        /// <summary>
        /// Returns the raw font data bytes.
        /// </summary>
        byte[] GetFontData();
    }
}
