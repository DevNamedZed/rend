using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Style;

namespace Rend.Layout
{
    /// <summary>
    /// A positioned box in the layout tree. Contains content, padding, border, and margin rectangles,
    /// plus references to the styled node and child boxes.
    /// </summary>
    internal class LayoutBox
    {
        private readonly List<LayoutBox> _children = new List<LayoutBox>();

        public LayoutBox(StyledNode? styledNode, BoxType boxType)
        {
            StyledNode = styledNode;
            BoxType = boxType;
        }

        /// <summary>The styled node that generated this box, or null for anonymous boxes.</summary>
        public StyledNode? StyledNode { get; }

        /// <summary>The box type / formatting context.</summary>
        public BoxType BoxType { get; set; }

        /// <summary>The content rectangle (position and size of the content area).</summary>
        public RectF ContentRect { get; set; }

        /// <summary>Padding edges (top, right, bottom, left).</summary>
        public float PaddingTop { get; set; }
        public float PaddingRight { get; set; }
        public float PaddingBottom { get; set; }
        public float PaddingLeft { get; set; }

        /// <summary>Border widths.</summary>
        public float BorderTopWidth { get; set; }
        public float BorderRightWidth { get; set; }
        public float BorderBottomWidth { get; set; }
        public float BorderLeftWidth { get; set; }

        /// <summary>Margin edges.</summary>
        public float MarginTop { get; set; }
        public float MarginRight { get; set; }
        public float MarginBottom { get; set; }
        public float MarginLeft { get; set; }

        /// <summary>The padding box rectangle.</summary>
        public RectF PaddingRect => new RectF(
            ContentRect.X - PaddingLeft,
            ContentRect.Y - PaddingTop,
            ContentRect.Width + PaddingLeft + PaddingRight,
            ContentRect.Height + PaddingTop + PaddingBottom);

        /// <summary>The border box rectangle.</summary>
        public RectF BorderRect => new RectF(
            ContentRect.X - PaddingLeft - BorderLeftWidth,
            ContentRect.Y - PaddingTop - BorderTopWidth,
            ContentRect.Width + PaddingLeft + PaddingRight + BorderLeftWidth + BorderRightWidth,
            ContentRect.Height + PaddingTop + PaddingBottom + BorderTopWidth + BorderBottomWidth);

        /// <summary>The margin box rectangle.</summary>
        public RectF MarginRect => new RectF(
            ContentRect.X - PaddingLeft - BorderLeftWidth - MarginLeft,
            ContentRect.Y - PaddingTop - BorderTopWidth - MarginTop,
            ContentRect.Width + PaddingLeft + PaddingRight + BorderLeftWidth + BorderRightWidth + MarginLeft + MarginRight,
            ContentRect.Height + PaddingTop + PaddingBottom + BorderTopWidth + BorderBottomWidth + MarginTop + MarginBottom);

        /// <summary>Child layout boxes.</summary>
        public IReadOnlyList<LayoutBox> Children => _children;

        /// <summary>Add a child layout box.</summary>
        public void AddChild(LayoutBox child)
        {
            child.Parent = this;
            _children.Add(child);
        }

        /// <summary>Remove all child layout boxes.</summary>
        public void ClearChildren()
        {
            _children.Clear();
        }

        /// <summary>Parent layout box.</summary>
        public LayoutBox? Parent { get; set; }

        /// <summary>Line boxes for inline formatting contexts.</summary>
        public List<LineBox>? LineBoxes { get; set; }

        /// <summary>
        /// Per-row heights from a subgrid layout. When this box is a grid item
        /// with grid-template-rows: subgrid, the subgrid's row heights are stored
        /// here so the parent grid can use them for track sizing (CSS Grid §8).
        /// </summary>
        internal float[]? SubgridRowHeights { get; set; }

        /// <summary>
        /// Y offset to apply to line boxes during painting. Used by the paginator
        /// when line boxes are shared from the original (unpaginated) layout —
        /// the offset shifts line box coordinates into page-local space.
        /// </summary>
        internal float LineBoxOffsetY { get; set; }

        /// <summary>Whether this box establishes a new stacking context.</summary>
        public bool EstablishesStackingContext { get; set; }

        /// <summary>Z-index for stacking order.</summary>
        public float ZIndex { get; set; }

        /// <summary>
        /// [CSS-FLEXBOX §9.8] Set when this flex item's cross size becomes definite
        /// after stretch. Children can resolve percentage heights against ContentRect.Height.
        /// </summary>
        public bool HasDefiniteCrossSize { get; set; }

        /// <summary>
        /// [CSS2 §9.2.1.1] True when this box is an anonymous block wrapper generated
        /// by the layout engine (not present in the source document). Anonymous blocks
        /// are transparent for containing-block height resolution.
        /// </summary>
        internal bool IsAnonymousBlock { get; set; }

        /// <summary>Column rules to paint between multi-column columns.</summary>
        internal List<ColumnRuleInfo>? ColumnRules { get; set; }

        /// <summary>
        /// When true, this cell is in a border-collapse table. Border widths store the
        /// layout half-widths; the painter should double them for rendering.
        /// </summary>
        internal bool CollapsedBorderCell { get; set; }

        /// <summary>
        /// [CSS-GRID §9] For absolutely positioned grid items with grid placement,
        /// the containing block is the grid area (not the grid container padding box).
        /// When set, PositionedLayout.ApplyAbsolute uses this rectangle instead of
        /// the containing block's PaddingRect.
        /// </summary>
        internal RectF? GridAreaContainingBlock { get; set; }

        /// <summary>
        /// [CSS-TRANSFORM2 §5] When true, the element's back face is toward the viewer
        /// and backface-visibility is hidden, so painting should be skipped.
        /// </summary>
        internal bool BackfaceHidden { get; set; }

        /// <summary>
        /// Resolved collapsed border colors (CSS 2.1 §17.6.2 priority).
        /// When set, the painter uses these instead of the cell's own border colors.
        /// </summary>
        internal CssColor? CollapsedBorderTopColor { get; set; }
        internal CssColor? CollapsedBorderRightColor { get; set; }
        internal CssColor? CollapsedBorderBottomColor { get; set; }
        internal CssColor? CollapsedBorderLeftColor { get; set; }

        /// <summary>
        /// [CSS-POSITION-3 §2.1] The (dx, dy) offset applied by position:relative.
        /// Stored so the painter can un-shift backgrounds for table internal elements
        /// (row-group, row) where Chrome paints the background at the original grid
        /// position, not the shifted position.
        /// </summary>
        internal float RelativeOffsetX { get; set; }
        internal float RelativeOffsetY { get; set; }
    }

    /// <summary>
    /// Describes a column rule line to be painted between multi-column columns.
    /// </summary>
    internal struct ColumnRuleInfo
    {
        public float X;
        public float Y;
        public float Height;
        public float Width;
        public Css.CssBorderStyle Style;
        public Core.Values.CssColor Color;
    }
}
