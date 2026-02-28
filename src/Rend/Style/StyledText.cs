using Rend.Css;

namespace Rend.Style
{
    /// <summary>
    /// A text node in the styled tree: contains text content with the inherited style
    /// from the parent element.
    /// </summary>
    public sealed class StyledText : StyledNode
    {
        public StyledText(string text, ComputedStyle style)
            : base(style)
        {
            Text = text;
        }

        public override bool IsText => true;

        /// <summary>The text content.</summary>
        public string Text { get; }
    }
}
