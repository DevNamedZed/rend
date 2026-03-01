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
            if (SizingKeyword.IsSizingKeyword(style.Width) && floatBox.StyledNode is StyledElement floatEl)
            {
                contentWidth = BlockFormattingContext.MeasureIntrinsicWidth(floatEl, style.Width, containingWidth, context);
            }
            else
            {
                contentWidth = DimensionResolver.ResolveWidth(style, containingWidth, floatBox);
            }
            float totalWidth = contentWidth + floatBox.PaddingLeft + floatBox.PaddingRight
                             + floatBox.BorderLeftWidth + floatBox.BorderRightWidth
                             + floatBox.MarginLeft + floatBox.MarginRight;

            // Layout contents to get height
            floatBox.ContentRect = new RectF(0, 0, contentWidth, 0);
            BlockFormattingContext.Layout(floatBox, context);

            float contentHeight = DimensionResolver.ResolveHeight(style, float.NaN, floatBox);
            if (float.IsNaN(contentHeight))
                contentHeight = CalculateAutoHeight(floatBox);

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
