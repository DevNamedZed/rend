using Rend.Css;

namespace Rend.Style
{
    /// <summary>
    /// Abstract base for a node in the styled tree.
    /// Each node is either a styled element or a styled text run.
    /// </summary>
    public abstract class StyledNode
    {
        /// <summary>Whether this is a text node.</summary>
        public abstract bool IsText { get; }

        /// <summary>The inherited computed style for this node.</summary>
        public ComputedStyle Style { get; }

        protected StyledNode(ComputedStyle style)
        {
            Style = style;
        }
    }
}
