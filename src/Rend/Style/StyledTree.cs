using Rend.Core.Values;

namespace Rend.Style
{
    /// <summary>
    /// The root of a styled tree: a DOM tree with resolved computed styles for every node.
    /// Produced by <see cref="StyleTreeBuilder"/> and consumed by the layout engine.
    /// </summary>
    public sealed class StyledTree
    {
        public StyledTree(StyledElement root, PageStyleInfo pageStyle)
        {
            Root = root;
            PageStyle = pageStyle;
        }

        /// <summary>The root styled element (typically the &lt;html&gt; element).</summary>
        public StyledElement Root { get; }

        /// <summary>Page style information extracted from @page rules.</summary>
        public PageStyleInfo PageStyle { get; }
    }

    /// <summary>
    /// Page style information extracted from @page CSS rules.
    /// </summary>
    public sealed class PageStyleInfo
    {
        /// <summary>Page size in points. Defaults to A4.</summary>
        public SizeF PageSize { get; set; } = Rend.Core.Values.PageSize.A4;

        /// <summary>Page margins in points (top, right, bottom, left).</summary>
        public float MarginTop { get; set; } = 72f;
        public float MarginRight { get; set; } = 72f;
        public float MarginBottom { get; set; } = 72f;
        public float MarginLeft { get; set; } = 72f;
    }
}
