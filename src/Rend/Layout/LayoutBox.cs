using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Style;

namespace Rend.Layout
{
    /// <summary>
    /// A positioned box in the layout tree. Contains content, padding, border, and margin rectangles,
    /// plus references to the styled node and child boxes.
    /// </summary>
    public class LayoutBox
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

        /// <summary>Parent layout box.</summary>
        public LayoutBox? Parent { get; set; }

        /// <summary>Line boxes for inline formatting contexts.</summary>
        public List<LineBox>? LineBoxes { get; set; }

        /// <summary>Whether this box establishes a new stacking context.</summary>
        public bool EstablishesStackingContext { get; set; }

        /// <summary>Z-index for stacking order.</summary>
        public float ZIndex { get; set; }

        /// <summary>Column rules to paint between multi-column columns.</summary>
        internal List<ColumnRuleInfo>? ColumnRules { get; set; }
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
