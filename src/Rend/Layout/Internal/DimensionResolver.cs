using System;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Css.Resolution.Internal;

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
        public static float ResolveWidth(ComputedStyle style, float containingBlockWidth, LayoutBox box,
            float containingBlockHeight = float.NaN)
        {
            float specifiedWidth = style.Width;
            float width;

            // Deferred calc() with percentage — resolve at layout time
            if (float.IsNegativeInfinity(specifiedWidth))
            {
                width = ResolveDeferredCalc(style, PropertyId.Width, containingBlockWidth);
                if (style.BoxSizing == CssBoxSizing.BorderBox)
                {
                    width -= (box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth);
                }
            }
            // Deferred percentage width (encoded with sentinel offset)
            else if (DeferredPercent.IsEncoded(specifiedWidth))
            {
                width = DeferredPercent.Resolve(specifiedWidth, containingBlockWidth);
                if (style.BoxSizing == CssBoxSizing.BorderBox)
                {
                    width -= (box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth);
                }
            }
            else if (float.IsNaN(specifiedWidth))
            {
                // [CSS-SIZING-4 §5.1] If width is auto and element has aspect-ratio
                // with a definite height, compute width from the ratio.
                // Ratio applies to the box determined by box-sizing: border-box
                // maps borderBoxWidth = borderBoxHeight * ratio, then subtract
                // horizontal padding+border for content width.
                float ratio = ParseAspectRatio(style);
                float specHeight = style.Height;
                if (ratio > 0 && !float.IsNaN(specHeight) && specHeight > 0
                    && !DeferredPercent.IsEncoded(specHeight)
                    && !float.IsNegativeInfinity(specHeight))
                {
                    // [CSS2 §10.7] Apply max-height/min-height BEFORE deriving width,
                    // so the width reflects the clamped height, not the raw specified height.
                    float clampMaxH = ResolvePercentHeight(style.MaxHeight, containingBlockHeight);
                    float clampMinH = ResolvePercentHeight(style.MinHeight, containingBlockHeight);
                    if (!float.IsNaN(clampMaxH) && clampMaxH >= 0 && specHeight > clampMaxH)
                    {
                        specHeight = clampMaxH;
                    }
                    if (!float.IsNaN(clampMinH) && clampMinH >= 0 && specHeight < clampMinH)
                    {
                        specHeight = clampMinH;
                    }

                    // [CSS-SIZING-4 §5.1] Ratio maps between the box determined by
                    // box-sizing. For border-box, specHeight IS the border-box height,
                    // so apply ratio to get border-box width, then subtract horizontal
                    // padding+border for content width.
                    width = specHeight * ratio;
                    if (style.BoxSizing == CssBoxSizing.BorderBox)
                    {
                        width -= (box.PaddingLeft + box.PaddingRight
                                + box.BorderLeftWidth + box.BorderRightWidth);
                        if (width < 0)
                        {
                            width = 0;
                        }
                    }
                }
                else
                {
                    // auto: fill containing block minus margins/padding/border
                    width = containingBlockWidth - BoxModelCalculator.GetHorizontalSpacing(box);
                }
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

            // [CSS-SIZING-4 §5.2] Transfer max-height/min-height constraints through
            // aspect-ratio to max-width/min-width. Only when width is AUTO and
            // determined by the aspect-ratio (not explicitly set).
            {
                float arRatio = ParseAspectRatio(style);
                // [CSS-SIZING-4 §5.2] Transfer min/max-height through aspect-ratio
                // even when containing block height is indefinite, as long as
                // min/max-height values are absolute (not percentage).
                // Height is auto, or a percentage that can't resolve (indefinite CB)
                bool heightIsIndefinite = float.IsNaN(style.Height)
                    || (DeferredPercent.IsEncoded(style.Height) && float.IsNaN(containingBlockHeight));
                if (arRatio > 0 && float.IsNaN(style.Width) && heightIsIndefinite)
                {
                    float maxH = ResolvePercentHeight(style.MaxHeight, containingBlockHeight);
                    if (!float.IsNaN(maxH) && maxH >= 0)
                    {
                        float transferredMaxW = maxH * arRatio;
                        if (float.IsNaN(maxW) || maxW < 0 || transferredMaxW < maxW)
                        {
                            maxW = transferredMaxW;
                        }
                    }
                    float minH = ResolvePercentHeight(style.MinHeight, containingBlockHeight);
                    if (!float.IsNaN(minH) && minH >= 0)
                    {
                        float transferredMinW = minH * arRatio;
                        // [CSS-SIZING-4 §5.2] Transferred min must not override explicit max
                        if (!float.IsNaN(maxW) && maxW >= 0 && transferredMinW > maxW)
                        {
                            transferredMinW = maxW;
                        }
                        if (float.IsNaN(minW) || minW < 0 || transferredMinW > minW)
                        {
                            minW = transferredMinW;
                        }
                    }
                }
            }
            if (style.BoxSizing == CssBoxSizing.BorderBox)
            {
                float hExtra = box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth;
                if (!float.IsNaN(minW) && minW >= 0)
                {
                    minW = Math.Max(0, minW - hExtra);
                }
                if (!float.IsNaN(maxW) && maxW >= 0)
                {
                    maxW = Math.Max(0, maxW - hExtra);
                }
            }
            // [CSS2 §10.4] Apply min-width and max-width together.
            // If min > max, min wins (CSS spec: min-width overrides max-width).
            float effectiveMin = !SizingKeyword.IsSizingKeyword(style.MinWidth) ? minW : float.NaN;
            float effectiveMax = !SizingKeyword.IsSizingKeyword(style.MaxWidth) ? maxW : float.NaN;
            width = ApplyMinMax(width, effectiveMin, effectiveMax);

            return Math.Max(0, width);
        }

        /// <summary>
        /// Resolve a width value that may be a deferred percentage (sentinel offset encoding).
        /// </summary>
        public static float ResolvePercentWidth(float value, float containingBlockWidth)
        {
            if (float.IsNegativeInfinity(value))
            {
                return value; // deferred calc — needs style context, handle at call site
            }
            if (DeferredPercent.IsEncoded(value))
            {
                return DeferredPercent.Resolve(value, containingBlockWidth);
            }
            return value;
        }

        /// <summary>
        /// Resolve the content height for an element.
        /// Returns NaN if height is auto (to be determined by content).
        /// </summary>
        public static float ResolveHeight(ComputedStyle style, float containingBlockHeight, LayoutBox box)
        {
            float specifiedHeight = style.Height;

            // Deferred calc() with percentage
            if (float.IsNegativeInfinity(specifiedHeight))
            {
                specifiedHeight = ResolveDeferredCalc(style, PropertyId.Height, containingBlockHeight);
            }
            // Deferred percentage heights (encoded with sentinel offset).
            // Resolve against the containing block height, or treat as auto if unknown.
            else if (DeferredPercent.IsEncoded(specifiedHeight))
            {
                if (float.IsNaN(containingBlockHeight) || containingBlockHeight <= 0)
                {
                    specifiedHeight = float.NaN; // treat as auto
                }
                else
                {
                    specifiedHeight = DeferredPercent.Resolve(specifiedHeight, containingBlockHeight);
                }
            }

            // Sizing keywords (fit-content, min-content, max-content) → treat as auto for height
            if (SizingKeyword.IsSizingKeyword(specifiedHeight))
                specifiedHeight = float.NaN;

            if (float.IsNaN(specifiedHeight))
            {
                // [CSS-SIZING-4 §5.1] Aspect-ratio applies to the box as determined
                // by box-sizing. For border-box, ratio maps border-box width → height,
                // then subtract padding/border for content height.
                float ratio = ParseAspectRatio(style);
                if (ratio > 0 && box.ContentRect.Width > 0)
                {
                    float verticalExtra = 0;
                    float ratioWidth = box.ContentRect.Width;
                    if (style.BoxSizing == CssBoxSizing.BorderBox)
                    {
                        float horizontalExtra = box.PaddingLeft + box.PaddingRight
                                              + box.BorderLeftWidth + box.BorderRightWidth;
                        verticalExtra = box.PaddingTop + box.PaddingBottom
                                      + box.BorderTopWidth + box.BorderBottomWidth;
                        ratioWidth += horizontalExtra;
                    }

                    float arHeight = ratioWidth / ratio;

                    float arMinH = ResolveMinMaxH(style.MinHeight, containingBlockHeight);
                    float arMaxH = ResolveMinMaxH(style.MaxHeight, containingBlockHeight);
                    arHeight = ApplyMinMax(arHeight, arMinH, arMaxH);

                    // Convert from border-box height to content-box height
                    if (verticalExtra > 0)
                    {
                        arHeight -= verticalExtra;
                    }

                    return Math.Max(0, arHeight);
                }
                return float.NaN; // auto: determined by content
            }

            float height = specifiedHeight;

            float vExtra = 0;
            if (style.BoxSizing == CssBoxSizing.BorderBox)
            {
                vExtra = box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth;
                height -= vExtra;
            }

            float minH = ResolveMinMaxH(style.MinHeight, containingBlockHeight);
            float maxH = ResolveMinMaxH(style.MaxHeight, containingBlockHeight);
            if (vExtra > 0)
            {
                if (!float.IsNaN(minH) && minH >= 0)
                {
                    minH = Math.Max(0, minH - vExtra);
                }
                if (!float.IsNaN(maxH) && maxH >= 0)
                {
                    maxH = Math.Max(0, maxH - vExtra);
                }
            }
            height = ApplyMinMax(height, minH, maxH);

            return Math.Max(0, height);
        }

        /// <summary>
        /// Resolve a min/max height value, handling deferred percentage encoding.
        /// </summary>
        /// <summary>
        /// Resolve a height value that may be a deferred percentage (sentinel offset encoding).
        /// </summary>
        public static float ResolvePercentHeight(float value, float containingBlockHeight)
        {
            return ResolveMinMaxH(value, containingBlockHeight);
        }

        private static float ResolveMinMaxH(float value, float containingBlockHeight)
        {
            if (DeferredPercent.IsEncoded(value))
            {
                if (float.IsNaN(containingBlockHeight) || containingBlockHeight <= 0)
                {
                    return float.NaN;
                }
                return DeferredPercent.Resolve(value, containingBlockHeight);
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
        /// <summary>
        /// Returns the aspect ratio (width/height) for public callers. 0 if auto/unset.
        /// </summary>
        public static float GetAspectRatio(ComputedStyle style)
        {
            return ParseAspectRatio(style);
        }

        /// <summary>
        /// [CSS-SIZING-4 §5.1] Parses the aspect-ratio CSS value.
        /// Supports: "auto", "16/9", "1.5", "auto 16/9" (auto + ratio).
        /// Returns the ratio (width/height) or 0 if auto-only/unset.
        /// </summary>
        private static float ParseAspectRatio(ComputedStyle style)
        {
            object? ratioRef = style.GetRefValue(PropertyId.AspectRatio);
            if (ratioRef == null)
            {
                return 0;
            }

            if (ratioRef is CssKeywordValue kw && kw.Keyword == "auto")
            {
                return 0;
            }

            if (ratioRef is CssNumberValue num)
            {
                return num.Value;
            }

            if (ratioRef is CssListValue list && list.Separator == ' ')
            {
                // Find the ratio part — skip "auto" keyword if present.
                // Formats: "16 / 9", "auto 16 / 9", "auto 1.5"
                int startIdx = 0;
                if (list.Values.Count > 0 && list.Values[0] is CssKeywordValue autoKw
                    && autoKw.Keyword == "auto")
                {
                    startIdx = 1;
                }

                if (list.Values.Count >= startIdx + 3)
                {
                    float w = GetNumericValue(list.Values[startIdx]);
                    float h = GetNumericValue(list.Values[startIdx + 2]);
                    if (w > 0 && h > 0)
                    {
                        return w / h;
                    }
                }
                else if (list.Values.Count >= startIdx + 1)
                {
                    float v = GetNumericValue(list.Values[startIdx]);
                    if (v > 0)
                    {
                        return v;
                    }
                }
            }

            if (ratioRef is CssDimensionValue dim)
            {
                return dim.Value;
            }

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
            // [CSS2 §10.4] Apply max first, then min. This ensures min wins if min > max.
            if (!float.IsNaN(max) && max >= 0)
            {
                value = Math.Min(value, max);
            }
            if (!float.IsNaN(min) && min >= 0)
            {
                value = Math.Max(value, min);
            }
            return value;
        }

        /// <summary>
        /// Evaluates a deferred calc() expression stored in the style's ref values.
        /// Used when calc() contains percentages that must resolve at layout time.
        /// </summary>
        private static float ResolveDeferredCalc(ComputedStyle style, int propertyId, float containingBlockDimension)
        {
            var refVal = style.GetRefValue(propertyId);
            if (refVal is CssFunctionValue calcFn)
            {
                return ValueResolver.EvaluateDeferredCalc(calcFn, containingBlockDimension);
            }
            // Fallback: treat as 0
            return 0;
        }
    }
}
