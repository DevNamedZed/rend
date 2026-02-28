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
    }
}
