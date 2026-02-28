using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Style;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// CSS Flexbox layout algorithm per CSS Flexible Box Layout Module Level 1.
    /// Handles flex-direction, flex-wrap, flex-grow/shrink, alignment.
    /// </summary>
    internal static class FlexLayout
    {
        public static void Layout(LayoutBox parent, LayoutContext context)
        {
            var styledElement = parent.StyledNode as StyledElement;
            if (styledElement == null) return;

            var style = styledElement.Style;
            float containerWidth = parent.ContentRect.Width;
            float containerHeight = parent.ContentRect.Height;
            bool isColumn = style.FlexDirection == CssFlexDirection.Column ||
                            style.FlexDirection == CssFlexDirection.ColumnReverse;
            bool isReverse = style.FlexDirection == CssFlexDirection.RowReverse ||
                             style.FlexDirection == CssFlexDirection.ColumnReverse;
            bool isWrap = style.FlexWrap != CssFlexWrap.Nowrap;

            float mainSize = isColumn ? containerHeight : containerWidth;
            if (float.IsNaN(mainSize) || mainSize <= 0)
                mainSize = isColumn ? 10000f : containerWidth;

            float gap = isColumn ? style.RowGap : style.ColumnGap;
            if (float.IsNaN(gap)) gap = 0;

            // Collect flex items
            var items = new List<FlexItem>();
            for (int i = 0; i < styledElement.Children.Count; i++)
            {
                var child = styledElement.Children[i];
                if (child.IsText) continue;
                var childElement = (StyledElement)child;
                if (childElement.Style.Display == CssDisplay.None) continue;

                var box = new LayoutBox(childElement, BoxType.Block);
                BoxModelCalculator.ApplyBoxModel(box, childElement.Style, containerWidth);

                float baseSize = ResolveFlexBasis(childElement.Style, isColumn, containerWidth, box);
                items.Add(new FlexItem
                {
                    Box = box,
                    Style = childElement.Style,
                    FlexGrow = Math.Max(0, childElement.Style.FlexGrow),
                    FlexShrink = Math.Max(0, childElement.Style.FlexShrink),
                    BaseSize = baseSize,
                    Order = childElement.Style.Order
                });
            }

            // Sort by order
            items.Sort((a, b) => a.Order.CompareTo(b.Order));
            if (isReverse) items.Reverse();

            // Distribute into flex lines
            var lines = new List<FlexLine>();
            var currentLine = new FlexLine();
            float usedMain = 0;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                float itemMain = item.BaseSize + GetItemMainMargins(item, isColumn);

                if (isWrap && usedMain + itemMain > mainSize && currentLine.Items.Count > 0)
                {
                    lines.Add(currentLine);
                    currentLine = new FlexLine();
                    usedMain = 0;
                }

                currentLine.Items.Add(item);
                usedMain += itemMain + (currentLine.Items.Count > 1 ? gap : 0);
            }
            if (currentLine.Items.Count > 0)
                lines.Add(currentLine);

            // Resolve flexible lengths and position items
            float crossCursor = isColumn ? parent.ContentRect.X : parent.ContentRect.Y;

            for (int li = 0; li < lines.Count; li++)
            {
                var line = lines[li];
                float totalBase = 0;
                float totalGaps = (line.Items.Count - 1) * gap;
                for (int i = 0; i < line.Items.Count; i++)
                    totalBase += line.Items[i].BaseSize + GetItemMainMargins(line.Items[i], isColumn);

                float freeSpace = mainSize - totalBase - totalGaps;
                float totalGrow = 0, totalShrink = 0;
                for (int i = 0; i < line.Items.Count; i++)
                {
                    totalGrow += line.Items[i].FlexGrow;
                    totalShrink += line.Items[i].FlexShrink;
                }

                // Resolve sizes
                for (int i = 0; i < line.Items.Count; i++)
                {
                    var item = line.Items[i];
                    float resolved = item.BaseSize;

                    if (freeSpace > 0 && totalGrow > 0)
                        resolved += freeSpace * (item.FlexGrow / totalGrow);
                    else if (freeSpace < 0 && totalShrink > 0)
                        resolved += freeSpace * (item.FlexShrink / totalShrink);

                    item.ResolvedMainSize = Math.Max(0, resolved);
                }

                // Position items on main axis
                float mainCursor = isColumn ? parent.ContentRect.Y : parent.ContentRect.X;
                mainCursor = ApplyJustifyContent(style.JustifyContent, mainCursor, freeSpace, line.Items.Count, totalGrow > 0);

                float maxCross = 0;
                for (int i = 0; i < line.Items.Count; i++)
                {
                    if (i > 0) mainCursor += gap;
                    var item = line.Items[i];
                    var box = item.Box;

                    float contentMain = item.ResolvedMainSize;
                    float contentCross;

                    if (isColumn)
                    {
                        contentCross = DimensionResolver.ResolveWidth(item.Style, containerWidth, box);
                        box.ContentRect = new RectF(
                            crossCursor + box.MarginLeft + box.BorderLeftWidth + box.PaddingLeft,
                            mainCursor + box.MarginTop + box.BorderTopWidth + box.PaddingTop,
                            contentCross, contentMain);
                    }
                    else
                    {
                        float specHeight = DimensionResolver.ResolveHeight(item.Style, containerHeight, box);
                        contentCross = float.IsNaN(specHeight) ? 0 : specHeight;
                        box.ContentRect = new RectF(
                            mainCursor + box.MarginLeft + box.BorderLeftWidth + box.PaddingLeft,
                            crossCursor + box.MarginTop + box.BorderTopWidth + box.PaddingTop,
                            contentMain, contentCross);
                    }

                    // Layout item contents
                    BlockFormattingContext.Layout(box, context);

                    // Resolve auto cross size
                    if (isColumn)
                    {
                        // Width already resolved
                    }
                    else if (float.IsNaN(item.Style.Height))
                    {
                        contentCross = CalculateAutoHeight(box);
                        box.ContentRect = new RectF(box.ContentRect.X, box.ContentRect.Y,
                                                    box.ContentRect.Width, contentCross);
                    }

                    parent.AddChild(box);

                    float totalCross = contentCross + box.PaddingTop + box.PaddingBottom
                                     + box.BorderTopWidth + box.BorderBottomWidth
                                     + box.MarginTop + box.MarginBottom;
                    if (totalCross > maxCross) maxCross = totalCross;

                    float totalMain = contentMain + (isColumn
                        ? box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth + box.MarginTop + box.MarginBottom
                        : box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth + box.MarginLeft + box.MarginRight);
                    mainCursor += totalMain;
                }

                line.CrossSize = maxCross;
                crossCursor += maxCross;
            }
        }

        private static float ResolveFlexBasis(ComputedStyle style, bool isColumn, float containerWidth, LayoutBox box)
        {
            float basis = style.FlexBasis;
            if (!float.IsNaN(basis) && basis >= 0)
                return basis;

            // Use width/height as fallback
            float size = isColumn ? style.Height : style.Width;
            if (!float.IsNaN(size) && size >= 0)
                return size;

            // Auto: use content size (estimated)
            return 0;
        }

        private static float GetItemMainMargins(FlexItem item, bool isColumn)
        {
            var box = item.Box;
            if (isColumn)
                return box.MarginTop + box.MarginBottom + box.PaddingTop + box.PaddingBottom
                     + box.BorderTopWidth + box.BorderBottomWidth;
            return box.MarginLeft + box.MarginRight + box.PaddingLeft + box.PaddingRight
                 + box.BorderLeftWidth + box.BorderRightWidth;
        }

        private static float ApplyJustifyContent(CssJustifyContent justify, float start, float freeSpace,
                                                   int itemCount, bool hasGrow)
        {
            if (hasGrow || freeSpace <= 0) return start;

            switch (justify)
            {
                case CssJustifyContent.Center:
                    return start + freeSpace / 2;
                case CssJustifyContent.FlexEnd:
                    return start + freeSpace;
                case CssJustifyContent.SpaceBetween:
                    return start; // gaps handled separately
                case CssJustifyContent.SpaceAround:
                    return start + freeSpace / (itemCount * 2);
                case CssJustifyContent.SpaceEvenly:
                    return start + freeSpace / (itemCount + 1);
                default:
                    return start;
            }
        }

        private static float CalculateAutoHeight(LayoutBox box)
        {
            float height = 0;
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                float childBottom = child.ContentRect.Y + child.ContentRect.Height
                                  + child.PaddingBottom + child.BorderBottomWidth + child.MarginBottom
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

        private sealed class FlexItem
        {
            public LayoutBox Box { get; set; } = null!;
            public ComputedStyle Style { get; set; } = null!;
            public float FlexGrow { get; set; }
            public float FlexShrink { get; set; } = 1;
            public float BaseSize { get; set; }
            public float ResolvedMainSize { get; set; }
            public int Order { get; set; }
        }

        private sealed class FlexLine
        {
            public List<FlexItem> Items { get; } = new List<FlexItem>();
            public float CrossSize { get; set; }
        }
    }
}
