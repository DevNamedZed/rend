using Rend.Core.Values;
using Rend.Fonts;

namespace Rend.Rendering
{
    /// <summary>
    /// Describes the visual style for rendering text, including font, size, color, and emphasis.
    /// </summary>
    public sealed class TextStyle
    {
        /// <summary>Gets or sets the font descriptor.</summary>
        public FontDescriptor Font { get; set; }

        /// <summary>Gets or sets the font size in pixels.</summary>
        public float FontSize { get; set; }

        /// <summary>Gets or sets the text color.</summary>
        public CssColor Color { get; set; }

        /// <summary>Gets or sets a value indicating whether the text is bold.</summary>
        public bool Bold { get; set; }

        /// <summary>Gets or sets a value indicating whether the text is italic.</summary>
        public bool Italic { get; set; }

        /// <summary>Gets or sets the extra spacing between characters in pixels (CSS letter-spacing).</summary>
        public float LetterSpacing { get; set; }

        /// <summary>Gets or sets the extra spacing between words in pixels (CSS word-spacing).</summary>
        public float WordSpacing { get; set; }

        /// <summary>Gets or sets the raw font data bytes for exact font matching, or null to use OS font lookup.</summary>
        public byte[]? FontData { get; set; }
    }
}
