using System;

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
            return parent.PaddingTop == 0
                && parent.BorderTopWidth == 0;
        }

        /// <summary>
        /// Returns true if margin collapsing should occur between parent and last child.
        /// </summary>
        public static bool ShouldCollapseWithLastChild(LayoutBox parent)
        {
            return parent.PaddingBottom == 0
                && parent.BorderBottomWidth == 0
                && float.IsNaN(parent.StyledNode?.Style.Height ?? float.NaN);
        }
    }
}
