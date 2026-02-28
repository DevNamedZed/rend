using Rend.Core.Values;
using Rend.Style;
using Rend.Text;

namespace Rend.Layout
{
    /// <summary>
    /// A text content box with positioned glyphs from text shaping.
    /// </summary>
    public sealed class LayoutText : LayoutBox
    {
        public LayoutText(StyledText styledText)
            : base(styledText, BoxType.Inline)
        {
            Text = styledText.Text;
        }

        /// <summary>The original text content.</summary>
        public string Text { get; set; }

        /// <summary>The shaped text run with glyph positions (set during layout).</summary>
        public ShapedTextRun? ShapedRun { get; set; }

        /// <summary>X position where the text is drawn.</summary>
        public float TextX { get; set; }

        /// <summary>Y position where the text baseline sits.</summary>
        public float TextY { get; set; }
    }
}
