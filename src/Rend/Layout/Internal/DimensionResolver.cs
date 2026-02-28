using System;
using Rend.Css;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Resolves width/height/min/max dimensions from ComputedStyle,
    /// handling auto values (NaN), percentages, and constraints.
    /// </summary>
    internal static class DimensionResolver
    {
        /// <summary>
        /// Resolve the content width for a block-level element.
        /// </summary>
        public static float ResolveWidth(ComputedStyle style, float containingBlockWidth, LayoutBox box)
        {
            float specifiedWidth = style.Width;
            float width;

            if (float.IsNaN(specifiedWidth))
            {
                // auto: fill containing block minus margins/padding/border
                width = containingBlockWidth - BoxModelCalculator.GetHorizontalSpacing(box);
            }
            else
            {
                width = specifiedWidth;

                // box-sizing: border-box → subtract padding and border from width
                if (style.BoxSizing == CssBoxSizing.BorderBox)
                {
                    width -= (box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth);
                }
            }

            // Apply min/max constraints
            width = ApplyMinMax(width, style.MinWidth, style.MaxWidth);

            return Math.Max(0, width);
        }

        /// <summary>
        /// Resolve the content height for an element.
        /// Returns NaN if height is auto (to be determined by content).
        /// </summary>
        public static float ResolveHeight(ComputedStyle style, float containingBlockHeight, LayoutBox box)
        {
            float specifiedHeight = style.Height;

            if (float.IsNaN(specifiedHeight))
                return float.NaN; // auto: determined by content

            float height = specifiedHeight;

            if (style.BoxSizing == CssBoxSizing.BorderBox)
            {
                height -= (box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth);
            }

            height = ApplyMinMax(height, style.MinHeight, style.MaxHeight);

            return Math.Max(0, height);
        }

        /// <summary>
        /// Resolve auto margins for block-level boxes (centering).
        /// </summary>
        public static void ResolveAutoMargins(ComputedStyle style, LayoutBox box, float containingBlockWidth)
        {
            bool marginLeftAuto = float.IsNaN(style.MarginLeft);
            bool marginRightAuto = float.IsNaN(style.MarginRight);

            if (marginLeftAuto && marginRightAuto)
            {
                // Center the element
                float usedWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                                + box.BorderLeftWidth + box.BorderRightWidth;
                float remaining = containingBlockWidth - usedWidth;
                float margin = Math.Max(0, remaining / 2);
                box.MarginLeft = margin;
                box.MarginRight = margin;
            }
            else if (marginLeftAuto)
            {
                float usedWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                                + box.BorderLeftWidth + box.BorderRightWidth + box.MarginRight;
                box.MarginLeft = Math.Max(0, containingBlockWidth - usedWidth);
            }
            else if (marginRightAuto)
            {
                float usedWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                                + box.BorderLeftWidth + box.BorderRightWidth + box.MarginLeft;
                box.MarginRight = Math.Max(0, containingBlockWidth - usedWidth);
            }
        }

        private static float ApplyMinMax(float value, float min, float max)
        {
            if (!float.IsNaN(min) && min > 0)
                value = Math.Max(value, min);
            if (!float.IsNaN(max) && max > 0)
                value = Math.Min(value, max);
            return value;
        }
    }
}
