using System;
using Rend.Css;
using Rend.Css.Properties.Internal;

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

            // Deferred percentage width (encoded as negative fraction, e.g. -0.5 = 50%)
            if (specifiedWidth < 0 && specifiedWidth > -1.01f)
            {
                width = -specifiedWidth * containingBlockWidth;
                if (style.BoxSizing == CssBoxSizing.BorderBox)
                {
                    width -= (box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth);
                }
            }
            else if (float.IsNaN(specifiedWidth))
            {
                // auto: fill containing block minus margins/padding/border
                width = containingBlockWidth - BoxModelCalculator.GetHorizontalSpacing(box);
            }
            else if (SizingKeyword.IsSizingKeyword(specifiedWidth))
            {
                // Intrinsic sizing keyword: treat as auto (will be resolved during layout)
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

            // Apply min/max constraints (resolve deferred percentages)
            float minW = ResolvePercentWidth(style.MinWidth, containingBlockWidth);
            float maxW = ResolvePercentWidth(style.MaxWidth, containingBlockWidth);
            if (!SizingKeyword.IsSizingKeyword(style.MinWidth)) width = ApplyMinMax(width, minW, float.NaN);
            if (!SizingKeyword.IsSizingKeyword(style.MaxWidth)) width = ApplyMinMax(width, float.NaN, maxW);

            return Math.Max(0, width);
        }

        /// <summary>
        /// Resolve a width value that may be a deferred percentage (negative fraction).
        /// </summary>
        public static float ResolvePercentWidth(float value, float containingBlockWidth)
        {
            if (value < 0 && value > -1.01f)
                return -value * containingBlockWidth;
            return value;
        }

        /// <summary>
        /// Resolve the content height for an element.
        /// Returns NaN if height is auto (to be determined by content).
        /// </summary>
        public static float ResolveHeight(ComputedStyle style, float containingBlockHeight, LayoutBox box)
        {
            float specifiedHeight = style.Height;

            // Negative values encode deferred percentage heights (e.g., -0.5 = 50%).
            // Resolve against the containing block height, or treat as auto if unknown.
            if (specifiedHeight < 0 && specifiedHeight > -1.01f)
            {
                if (float.IsNaN(containingBlockHeight) || containingBlockHeight <= 0)
                    specifiedHeight = float.NaN; // treat as auto
                else
                    specifiedHeight = -specifiedHeight * containingBlockHeight;
            }

            if (float.IsNaN(specifiedHeight))
            {
                // Check for aspect-ratio: if set and width is known, compute height from ratio.
                float ratio = ParseAspectRatio(style);
                if (ratio > 0 && box.ContentRect.Width > 0)
                {
                    float arHeight = box.ContentRect.Width / ratio;
                    arHeight = ApplyMinMax(arHeight,
                        ResolveMinMaxH(style.MinHeight, containingBlockHeight),
                        ResolveMinMaxH(style.MaxHeight, containingBlockHeight));
                    return Math.Max(0, arHeight);
                }
                return float.NaN; // auto: determined by content
            }

            float height = specifiedHeight;

            if (style.BoxSizing == CssBoxSizing.BorderBox)
            {
                height -= (box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth);
            }

            height = ApplyMinMax(height,
                ResolveMinMaxH(style.MinHeight, containingBlockHeight),
                ResolveMinMaxH(style.MaxHeight, containingBlockHeight));

            return Math.Max(0, height);
        }

        /// <summary>
        /// Resolve a min/max height value, handling deferred percentage encoding.
        /// </summary>
        /// <summary>
        /// Resolve a height value that may be a deferred percentage (negative fraction).
        /// </summary>
        public static float ResolvePercentHeight(float value, float containingBlockHeight)
        {
            return ResolveMinMaxH(value, containingBlockHeight);
        }

        private static float ResolveMinMaxH(float value, float containingBlockHeight)
        {
            if (value < 0 && value > -1.01f)
            {
                if (float.IsNaN(containingBlockHeight) || containingBlockHeight <= 0)
                    return float.NaN;
                return -value * containingBlockHeight;
            }
            return value;
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

        /// <summary>
        /// Parses the aspect-ratio CSS value. Returns the ratio (width/height) or 0 if auto/unset.
        /// Supports formats: "auto", "16/9", "1.5", etc.
        /// </summary>
        private static float ParseAspectRatio(ComputedStyle style)
        {
            object? ratioRef = style.GetRefValue(PropertyId.AspectRatio);
            if (ratioRef == null) return 0;

            if (ratioRef is CssKeywordValue kw && kw.Keyword == "auto") return 0;

            if (ratioRef is CssNumberValue num) return num.Value;

            if (ratioRef is CssListValue list && list.Separator == ' ' && list.Values.Count >= 3)
            {
                // "16 / 9" parsed as space-separated list: [16, /, 9]
                float w = GetNumericValue(list.Values[0]);
                float h = GetNumericValue(list.Values[2]);
                if (w > 0 && h > 0) return w / h;
            }

            if (ratioRef is CssDimensionValue dim) return dim.Value;

            return 0;
        }

        private static float GetNumericValue(CssValue value)
        {
            if (value is CssNumberValue n) return n.Value;
            if (value is CssDimensionValue d) return d.Value;
            return 0;
        }

        private static float ApplyMinMax(float value, float min, float max)
        {
            if (!float.IsNaN(min) && min >= 0)
                value = Math.Max(value, min);
            if (!float.IsNaN(max) && max >= 0)
                value = Math.Min(value, max);
            return value;
        }
    }
}
