using System;
using Rend.Css;
using Rend.Css.Properties.Internal;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Implements CSS 2.1 §8.3.1 adjacent margin collapsing for block-level elements.
    /// </summary>
    internal static class MarginCollapsing
    {
        /// <summary>
        /// Compute the collapsed margin between two adjacent vertical margins.
        /// Per CSS spec: if both positive, use the larger. If both negative, use the more negative.
        /// If one positive and one negative, sum them.
        /// </summary>
        public static float Collapse(float marginA, float marginB)
        {
            if (marginA >= 0 && marginB >= 0)
                return Math.Max(marginA, marginB);

            if (marginA < 0 && marginB < 0)
                return Math.Min(marginA, marginB);

            return marginA + marginB;
        }

        /// <summary>
        /// Returns true if margin collapsing should occur between parent and first child.
        /// Margins collapse unless separated by padding, border, or inline content.
        /// </summary>
        public static bool ShouldCollapseWithFirstChild(LayoutBox parent)
        {
            if (parent.PaddingTop != 0 || parent.BorderTopWidth != 0)
                return false;

            // Elements that establish a new BFC do not collapse margins with children.
            if (EstablishesBfc(parent))
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if margin collapsing should occur between parent and last child.
        /// </summary>
        public static bool ShouldCollapseWithLastChild(LayoutBox parent)
        {
            if (parent.PaddingBottom != 0 || parent.BorderBottomWidth != 0)
                return false;

            if (!float.IsNaN(parent.StyledNode?.Style.Height ?? float.NaN))
                return false;

            if (EstablishesBfc(parent))
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if the box establishes a new block formatting context,
        /// which prevents margin collapsing through its boundary.
        /// </summary>
        private static bool EstablishesBfc(LayoutBox box)
        {
            var style = box.StyledNode?.Style;
            if (style == null) return false;

            // overflow != visible establishes a BFC
            if (style.OverflowX != CssOverflow.Visible || style.OverflowY != CssOverflow.Visible)
                return true;

            // Floated elements establish a BFC
            if (style.Float != CssFloat.None)
                return true;

            // Absolutely/fixed positioned elements establish a BFC
            if (style.Position == CssPosition.Absolute || style.Position == CssPosition.Fixed)
                return true;

            // display: inline-block, flex, grid establish a BFC
            if (style.Display == CssDisplay.InlineBlock ||
                style.Display == CssDisplay.Flex ||
                style.Display == CssDisplay.Grid)
                return true;

            // contain: layout, content, or strict establish a BFC
            var contain = style.Contain;
            if (contain == CssContain.Layout || contain == CssContain.Content || contain == CssContain.Strict)
                return true;

            return false;
        }
    }
}
