using Rend.Style;

namespace Rend.Layout
{
    /// <summary>
    /// An inline-level box: a text run fragment or an inline element.
    /// </summary>
    public sealed class LayoutInline : LayoutBox
    {
        public LayoutInline(StyledNode? styledNode)
            : base(styledNode, BoxType.Inline)
        {
        }
    }
}
