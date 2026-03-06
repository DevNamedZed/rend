using System;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Css.Resolution.Internal;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Resolves margin, padding, and border values from a ComputedStyle
    /// and applies them to a LayoutBox.
    /// </summary>
    internal static class BoxModelCalculator
    {
        public static void ApplyBoxModel(LayoutBox box, ComputedStyle style, float containingBlockWidth)
        {
            // Padding (percentages resolve against containing block width)
            box.PaddingTop = ResolveLength(style.PaddingTop, containingBlockWidth, style, PropertyId.PaddingTop);
            box.PaddingRight = ResolveLength(style.PaddingRight, containingBlockWidth, style, PropertyId.PaddingRight);
            box.PaddingBottom = ResolveLength(style.PaddingBottom, containingBlockWidth, style, PropertyId.PaddingBottom);
            box.PaddingLeft = ResolveLength(style.PaddingLeft, containingBlockWidth, style, PropertyId.PaddingLeft);

            // Border
            box.BorderTopWidth = style.BorderTopStyle != CssBorderStyle.None ? style.BorderTopWidth : 0;
            box.BorderRightWidth = style.BorderRightStyle != CssBorderStyle.None ? style.BorderRightWidth : 0;
            box.BorderBottomWidth = style.BorderBottomStyle != CssBorderStyle.None ? style.BorderBottomWidth : 0;
            box.BorderLeftWidth = style.BorderLeftStyle != CssBorderStyle.None ? style.BorderLeftWidth : 0;

            // Margins (auto is represented as float.NaN in ComputedStyle)
            box.MarginTop = ResolveMargin(style.MarginTop, containingBlockWidth, style, PropertyId.MarginTop);
            box.MarginRight = ResolveMargin(style.MarginRight, containingBlockWidth, style, PropertyId.MarginRight);
            box.MarginBottom = ResolveMargin(style.MarginBottom, containingBlockWidth, style, PropertyId.MarginBottom);
            box.MarginLeft = ResolveMargin(style.MarginLeft, containingBlockWidth, style, PropertyId.MarginLeft);
        }

        private static float ResolveLength(float value, float containingBlockWidth, ComputedStyle style, int propertyId)
        {
            if (float.IsNaN(value)) return 0;
            if (float.IsNegativeInfinity(value))
            {
                var refVal = style.GetRefValue(propertyId);
                if (refVal is CssFunctionValue calcFn)
                    return Math.Max(0, ValueResolver.EvaluateDeferredCalc(calcFn, containingBlockWidth));
                return 0;
            }
            // Negative values encode deferred percentages (e.g., -0.05 = 5% of containing block width)
            if (value < 0 && value > -1.01f)
                return Math.Max(0, -value * containingBlockWidth);
            return Math.Max(0, value);
        }

        private static float ResolveMargin(float value, float containingBlockWidth, ComputedStyle style, int propertyId)
        {
            if (float.IsNaN(value)) return 0; // auto margins handled separately
            if (float.IsNegativeInfinity(value))
            {
                var refVal = style.GetRefValue(propertyId);
                if (refVal is CssFunctionValue calcFn)
                    return ValueResolver.EvaluateDeferredCalc(calcFn, containingBlockWidth);
                return 0;
            }
            // Deferred percentage encoding: small negative fractions in (-1.01, 0) represent percentages.
            // CSS 2.1 §8.3: All percentage margins resolve against the containing block width.
            if (value < 0 && value > -1.01f)
                return -value * containingBlockWidth;
            return value;
        }

        /// <summary>
        /// Returns the total horizontal spacing (padding + border + margin).
        /// </summary>
        public static float GetHorizontalSpacing(LayoutBox box)
        {
            return box.MarginLeft + box.BorderLeftWidth + box.PaddingLeft
                 + box.PaddingRight + box.BorderRightWidth + box.MarginRight;
        }

        /// <summary>
        /// Returns the total vertical spacing (padding + border + margin).
        /// </summary>
        public static float GetVerticalSpacing(LayoutBox box)
        {
            return box.MarginTop + box.BorderTopWidth + box.PaddingTop
                 + box.PaddingBottom + box.BorderBottomWidth + box.MarginBottom;
        }
    }
}
