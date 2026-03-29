using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Style;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Float placement: positions floated elements and tracks exclusion areas.
    /// CSS 2.1 §9.5
    /// </summary>
    internal static class FloatLayout
    {
        public static void PlaceFloat(LayoutBox floatBox, FloatContext floatContext,
                                       LayoutBox parent, LayoutContext context)
        {
            var style = floatBox.StyledNode?.Style;
            if (style == null) return;

            float containingWidth = parent.ContentRect.Width;
            BoxModelCalculator.ApplyBoxModel(floatBox, style, containingWidth);

            float contentWidth;
            bool isFloatReplaced = floatBox.StyledNode is StyledElement floatStyledEl
                && ReplacedElementLayout.IsReplaced(floatStyledEl);
            if (isFloatReplaced && (SizingKeyword.IsSizingKeyword(style.Width) || float.IsNaN(style.Width)))
            {
                // [CSS-SIZING-3 §5.1] Replaced elements: intrinsic sizing keywords
                // resolve to the intrinsic size. Use auto-sizing algorithm.
                var replacedEl = (StyledElement)floatBox.StyledNode!;
                float intrinsicW = 0;
                string? attrW = replacedEl.GetAttribute("width");
                if (attrW != null && float.TryParse(attrW, out float aw))
                {
                    intrinsicW = aw;
                }
                if (intrinsicW <= 0 && ReplacedElementLayout.TryGetDataUriDimensions(replacedEl, out float duW, out _))
                {
                    intrinsicW = duW;
                }
                contentWidth = intrinsicW > 0 ? intrinsicW : 300;
            }
            else if (SizingKeyword.IsSizingKeyword(style.Width) && floatBox.StyledNode is StyledElement floatEl)
            {
                contentWidth = BlockFormattingContext.MeasureIntrinsicWidth(floatEl, style.Width, containingWidth, context);
            }
            else if (float.IsNaN(style.Width) && !DeferredPercent.IsEncoded(style.Width)
                     && floatBox.StyledNode is StyledElement floatStyledElement)
            {
                // CSS 2.1 §10.3.5: Floated, non-replaced elements with auto width
                // use shrink-to-fit (fit-content) width.
                contentWidth = BlockFormattingContext.MeasureIntrinsicWidth(
                    floatStyledElement, SizingKeyword.FitContent, containingWidth, context);
            }
            else
            {
                contentWidth = DimensionResolver.ResolveWidth(style, containingWidth, floatBox);
            }
            // [CSS-SIZING-4 §5.2] Transfer min-height through aspect-ratio for floats
            float arRatio = DimensionResolver.GetAspectRatio(style);
            if (arRatio > 0 && float.IsNaN(style.Width))
            {
                float arMinH = style.MinHeight;
                if (!float.IsNaN(arMinH) && arMinH > 0 && !DeferredPercent.IsEncoded(arMinH))
                {
                    float transferredW = arMinH * arRatio;
                    if (contentWidth < transferredW)
                    {
                        contentWidth = transferredW;
                    }
                }
            }

            // [CSS2 §10.4] Apply min-width/max-width to float content width
            float minW = DimensionResolver.ResolvePercentWidth(style.MinWidth, containingWidth);
            float maxW = DimensionResolver.ResolvePercentWidth(style.MaxWidth, containingWidth);
            if (style.BoxSizing == CssBoxSizing.BorderBox)
            {
                float hExtra = floatBox.PaddingLeft + floatBox.PaddingRight + floatBox.BorderLeftWidth + floatBox.BorderRightWidth;
                if (!float.IsNaN(minW) && minW >= 0) { minW = Math.Max(0, minW - hExtra); }
                if (!float.IsNaN(maxW) && maxW >= 0) { maxW = Math.Max(0, maxW - hExtra); }
            }
            if (!float.IsNaN(maxW) && maxW >= 0 && contentWidth > maxW) { contentWidth = maxW; }
            if (!float.IsNaN(minW) && minW >= 0 && contentWidth < minW) { contentWidth = minW; }

            float totalWidth = contentWidth + floatBox.PaddingLeft + floatBox.PaddingRight
                             + floatBox.BorderLeftWidth + floatBox.BorderRightWidth
                             + floatBox.MarginLeft + floatBox.MarginRight;

            // Layout contents to get height
            floatBox.ContentRect = new RectF(0, 0, contentWidth, 0);
            BlockFormattingContext.Layout(floatBox, context);

            float contentHeight = DimensionResolver.ResolveHeight(style, float.NaN, floatBox);
            if (float.IsNaN(contentHeight))
            {
                contentHeight = CalculateAutoHeight(floatBox);
            }

            // [CSS2 §10.7] Apply min-height/max-height
            float minH = DimensionResolver.ResolvePercentHeight(style.MinHeight, float.NaN);
            float maxH = DimensionResolver.ResolvePercentHeight(style.MaxHeight, float.NaN);
            if (!float.IsNaN(maxH) && maxH >= 0 && contentHeight > maxH) { contentHeight = maxH; }
            if (!float.IsNaN(minH) && minH >= 0 && contentHeight < minH) { contentHeight = minH; }

            float totalHeight = contentHeight + floatBox.PaddingTop + floatBox.PaddingBottom
                              + floatBox.BorderTopWidth + floatBox.BorderBottomWidth
                              + floatBox.MarginTop + floatBox.MarginBottom;

            // Find position
            float y = floatContext.CurrentY;
            float x;

            if (style.Float == CssFloat.Left)
            {
                x = floatContext.GetLeftEdge(y, totalHeight) + floatBox.MarginLeft;
                floatContext.AddLeftFloat(new RectF(x - floatBox.MarginLeft, y, totalWidth, totalHeight));
            }
            else
            {
                x = floatContext.GetRightEdge(y, totalHeight) - totalWidth + floatBox.MarginLeft;
                floatContext.AddRightFloat(new RectF(x - floatBox.MarginLeft, y, totalWidth, totalHeight));
            }

            floatBox.ContentRect = new RectF(
                x + floatBox.BorderLeftWidth + floatBox.PaddingLeft,
                y + floatBox.MarginTop + floatBox.BorderTopWidth + floatBox.PaddingTop,
                contentWidth, contentHeight);
        }

        private static float CalculateAutoHeight(LayoutBox box)
        {
            float height = 0;
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                float childBottom = child.ContentRect.Y + child.ContentRect.Height
                                  + child.PaddingBottom + child.BorderBottomWidth + child.MarginBottom;
                float childHeight = childBottom - box.ContentRect.Y;
                if (childHeight > height) height = childHeight;
            }
            return height;
        }
    }
}
