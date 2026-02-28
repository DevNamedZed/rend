using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Style;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// CSS Grid layout: track sizing, auto-placement, and item positioning.
    /// CSS Grid Layout Module Level 1.
    /// </summary>
    internal static class GridLayout
    {
        public static void Layout(LayoutBox parent, LayoutContext context)
        {
            var styledElement = parent.StyledNode as StyledElement;
            if (styledElement == null) return;

            var style = styledElement.Style;
            float containerWidth = parent.ContentRect.Width;
            float containerHeight = parent.ContentRect.Height;
            if (float.IsNaN(containerHeight)) containerHeight = 10000f;

            float rowGap = float.IsNaN(style.RowGap) ? 0 : style.RowGap;
            float colGap = float.IsNaN(style.ColumnGap) ? 0 : style.ColumnGap;

            // Collect grid items
            var items = new List<GridItem>();
            for (int i = 0; i < styledElement.Children.Count; i++)
            {
                var child = styledElement.Children[i];
                if (child.IsText) continue;
                var childEl = (StyledElement)child;
                if (childEl.Style.Display == CssDisplay.None) continue;

                items.Add(new GridItem
                {
                    StyledElement = childEl,
                    Box = new LayoutBox(childEl, BoxType.Block)
                });
            }

            if (items.Count == 0) return;

            // Auto-placement: arrange items in a grid
            int cols = (int)Math.Max(1, Math.Ceiling(Math.Sqrt(items.Count)));
            int rows = (int)Math.Ceiling((float)items.Count / cols);

            float colWidth = (containerWidth - (cols - 1) * colGap) / cols;

            // First pass: layout each item to determine row heights
            float[] rowHeights = new float[rows];
            for (int i = 0; i < items.Count; i++)
            {
                int row = i / cols;
                int col = i % cols;
                var item = items[i];

                BoxModelCalculator.ApplyBoxModel(item.Box, item.StyledElement.Style, colWidth);
                float contentWidth = colWidth - item.Box.PaddingLeft - item.Box.PaddingRight
                                   - item.Box.BorderLeftWidth - item.Box.BorderRightWidth
                                   - item.Box.MarginLeft - item.Box.MarginRight;
                contentWidth = Math.Max(0, contentWidth);

                item.Box.ContentRect = new RectF(0, 0, contentWidth, 0);
                BlockFormattingContext.Layout(item.Box, context);

                float contentHeight = DimensionResolver.ResolveHeight(item.StyledElement.Style, float.NaN, item.Box);
                if (float.IsNaN(contentHeight))
                    contentHeight = CalculateAutoHeight(item.Box);

                float totalHeight = contentHeight + item.Box.PaddingTop + item.Box.PaddingBottom
                                  + item.Box.BorderTopWidth + item.Box.BorderBottomWidth
                                  + item.Box.MarginTop + item.Box.MarginBottom;
                if (totalHeight > rowHeights[row])
                    rowHeights[row] = totalHeight;

                item.ContentHeight = contentHeight;
                item.ContentWidth = contentWidth;
            }

            // Second pass: position items
            float cursorY = parent.ContentRect.Y;
            for (int i = 0; i < items.Count; i++)
            {
                int row = i / cols;
                int col = i % cols;
                var item = items[i];

                float x = parent.ContentRect.X + col * (colWidth + colGap);
                float y = parent.ContentRect.Y;
                for (int r = 0; r < row; r++)
                    y += rowHeights[r] + rowGap;

                item.Box.ContentRect = new RectF(
                    x + item.Box.MarginLeft + item.Box.BorderLeftWidth + item.Box.PaddingLeft,
                    y + item.Box.MarginTop + item.Box.BorderTopWidth + item.Box.PaddingTop,
                    item.ContentWidth, item.ContentHeight);

                parent.AddChild(item.Box);
            }
        }

        private static float CalculateAutoHeight(LayoutBox box)
        {
            float height = 0;
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                float childBottom = child.ContentRect.Y + child.ContentRect.Height
                                  + child.PaddingBottom + child.BorderBottomWidth
                                  - box.ContentRect.Y;
                if (childBottom > height) height = childBottom;
            }
            if (box.LineBoxes != null)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    float lb = box.LineBoxes[i].Y + box.LineBoxes[i].Height - box.ContentRect.Y;
                    if (lb > height) height = lb;
                }
            }
            return height;
        }

        private sealed class GridItem
        {
            public StyledElement StyledElement { get; set; } = null!;
            public LayoutBox Box { get; set; } = null!;
            public float ContentWidth { get; set; }
            public float ContentHeight { get; set; }
        }
    }
}
