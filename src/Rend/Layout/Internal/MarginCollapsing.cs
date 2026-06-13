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
            {
                return false;
            }

            // Elements that establish a new BFC do not collapse margins with children.
            if (EstablishesBfc(parent))
            {
                return false;
            }

            // BUG-058: Margins don't collapse if there's inline content (text, replaced elements)
            // before the first block child. Check if the first child is inline content.
            if (parent.Children.Count > 0)
            {
                var firstChild = parent.Children[0];
                if (firstChild.BoxType == BoxType.Inline || firstChild is LayoutText)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if margin collapsing should occur between parent and last child.
        /// </summary>
        public static bool ShouldCollapseWithLastChild(LayoutBox parent)
        {
            if (parent.PaddingBottom != 0 || parent.BorderBottomWidth != 0)
            {
                return false;
            }

            // [CSS-SIZING-3 §5.1] Margin collapse requires height to "behave as auto".
            // This includes: auto (NaN), intrinsic keywords (min/max/fit-content),
            // deferred percentages (cyclic), and deferred calc with percentages.
            if (!HeightBehavesAsAuto(parent.StyledNode?.Style.Height ?? float.NaN))
            {
                return false;
            }

            if (EstablishesBfc(parent))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// [CSS-SIZING-3 §5.1] Returns true if the height value "behaves as auto"
        /// for margin collapsing purposes.
        /// </summary>
        private static bool HeightBehavesAsAuto(float height)
        {
            if (float.IsNaN(height))
            {
                return true;
            }
            if (SizingKeyword.IsSizingKeyword(height))
            {
                return true;
            }
            if (DeferredPercent.IsEncoded(height))
            {
                return true;
            }
            if (float.IsNegativeInfinity(height))
            {
                return true;
            }
            return false;
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
            {
                // [CSS2 §11.1.1] When html has overflow:visible, body's overflow
                // propagates to the viewport. Body itself behaves as overflow:visible
                // and does NOT establish a BFC.
                if (!IsBodyOverflowPropagated(box))
                {
                    return true;
                }
            }

            // Floated elements establish a BFC
            if (style.Float != CssFloat.None)
                return true;

            // Absolutely/fixed positioned elements establish a BFC
            if (style.Position == CssPosition.Absolute || style.Position == CssPosition.Fixed)
                return true;

            // display: inline-block, flow-root, flex, grid establish a BFC
            if (style.Display == CssDisplay.InlineBlock ||
                style.Display == CssDisplay.FlowRoot ||
                style.Display == CssDisplay.Flex ||
                style.Display == CssDisplay.Grid)
            {
                return true;
            }

            // Flex items and grid items establish an independent BFC
            // (CSS Flexbox §4, CSS Grid §6) — child margins don't collapse through
            if (box.Parent != null &&
                (box.Parent.BoxType == BoxType.Flex || box.Parent.BoxType == BoxType.Grid))
            {
                return true;
            }

            // contain: layout, content, or strict establish a BFC
            var contain = style.Contain;
            if (contain == CssContain.Layout || contain == CssContain.Content || contain == CssContain.Strict)
                return true;

            return false;
        }

        /// <summary>
        /// [CSS2 §11.1.1] Delegates to BFC for body overflow propagation check.
        /// </summary>
        private static bool IsBodyOverflowPropagated(LayoutBox box)
        {
            return BlockFormattingContext.IsBodyOverflowPropagated(box);
        }
    }
}
