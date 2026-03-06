using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Style;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// CSS Multi-Column Layout (CSS Multi-column Layout Module Level 1).
    /// Splits block content into multiple columns with configurable count, width, gap, and rules.
    /// </summary>
    internal static class MultiColumnLayout
    {
        public static void Layout(LayoutBox box, LayoutContext context)
        {
            var styledElement = box.StyledNode as StyledElement;
            if (styledElement == null) return;

            var style = styledElement.Style;
            float availableWidth = box.ContentRect.Width;

            // Resolve column parameters
            float columnGap = style.ColumnGap;
            if (float.IsNaN(columnGap) || columnGap < 0)
                columnGap = style.FontSize; // CSS spec: 'normal' = 1em

            int columnCount = ResolveColumnCount(style, availableWidth, columnGap);
            if (columnCount < 1) columnCount = 1;

            float totalGaps = (columnCount - 1) * columnGap;
            float columnWidth = (availableWidth - totalGaps) / columnCount;
            if (columnWidth < 1) columnWidth = availableWidth;

            // Check for column-span: all elements among direct children.
            // If found, split content into segments around spanning elements.
            bool hasSpanners = false;
            for (int i = 0; i < styledElement.Children.Count; i++)
            {
                var child = styledElement.Children[i];
                if (!child.IsText && !(child is StyledPseudoElement) && child is StyledElement childEl)
                {
                    if (childEl.Style.ColumnSpan == CssColumnSpan.All)
                    {
                        hasSpanners = true;
                        break;
                    }
                }
            }

            if (hasSpanners && columnCount > 1)
            {
                LayoutWithSpanners(box, styledElement, context, columnCount, columnWidth, columnGap);
                return;
            }

            // First pass: layout all children as a single column to measure total height
            var tempBox = CreateTempBox(box, styledElement, availableWidth);
            if (HasBlockChildren(styledElement))
                BlockFormattingContext.Layout(tempBox, context);
            else
                InlineFormattingContext.Layout(tempBox, context);

            float totalHeight = CalculateContentHeight(tempBox);

            // If content fits in one column, just do normal layout
            if (columnCount <= 1)
            {
                CopyLayoutResult(tempBox, box);
                return;
            }

            // Second pass: layout content constrained to column width
            var columnBox = CreateTempBox(box, styledElement, columnWidth);
            if (HasBlockChildren(styledElement))
                BlockFormattingContext.Layout(columnBox, context);
            else
                InlineFormattingContext.Layout(columnBox, context);

            float contentHeight = CalculateContentHeight(columnBox);

            // Calculate target column height
            float targetHeight;
            if (style.ColumnFill == CssColumnFill.Auto)
            {
                // Auto: fill columns sequentially; use explicit height if set, else total
                float explicitHeight = style.Height;
                targetHeight = float.IsNaN(explicitHeight) ? totalHeight : explicitHeight;
            }
            else
            {
                // Balance: binary search for the minimum column height that fits all
                // content into the available number of columns
                targetHeight = BinarySearchColumnHeight(columnBox, columnCount, contentHeight);
            }

            float columnHeight = targetHeight;
            if (columnHeight < 1) columnHeight = contentHeight;

            // Sequential content-aware fragmentation: assign children to columns
            // based on accumulated height, breaking between elements.
            float startX = box.ContentRect.X;
            float startY = box.ContentRect.Y;
            float contentOriginY = columnBox.ContentRect.Y;

            // Build lists of children and line boxes assigned to each column
            var colChildren = new List<LayoutBox>[columnCount];
            var colLineBoxes = new List<LineBox>[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                colChildren[i] = new List<LayoutBox>();
                colLineBoxes[i] = new List<LineBox>();
            }

            // Assign block children to columns using content-aware fragmentation.
            // When checking overflow, only count margin-top + border-box height (not margin-bottom)
            // because trailing margin doesn't affect the visual bottom of the last item in a column.
            int currentCol = 0;
            float currentColHeight = 0;
            foreach (var child in columnBox.Children)
            {
                float childVisualHeight = child.BorderRect.Height + child.MarginTop;
                float childFullHeight = childVisualHeight + child.MarginBottom;

                // Move to next column if this child would overflow and there's room
                if (currentColHeight > 0 && currentColHeight + childVisualHeight > columnHeight
                    && currentCol < columnCount - 1)
                {
                    currentCol++;
                    currentColHeight = 0;
                }

                colChildren[currentCol].Add(child);
                currentColHeight += childFullHeight;
            }

            // Assign line boxes to columns (for inline content)
            if (columnBox.LineBoxes != null)
            {
                int lineCol = 0;
                float lineColHeight = 0;
                foreach (var line in columnBox.LineBoxes)
                {
                    float lineHeight = line.Height;
                    if (lineColHeight > 0 && lineColHeight + lineHeight > columnHeight
                        && lineCol < columnCount - 1)
                    {
                        lineCol++;
                        lineColHeight = 0;
                    }
                    colLineBoxes[lineCol].Add(line);
                    lineColHeight += lineHeight;
                }
            }

            // Build column layout boxes — shift all children in each column by a uniform offset
            // so the first child's margin-box-top aligns with startY, preserving relative spacing.
            float tallestColumn = 0;
            for (int col = 0; col < columnCount; col++)
            {
                float colX = startX + col * (columnWidth + columnGap);
                float xOffset = colX - columnBox.ContentRect.X;

                var colBox = new LayoutBox(null, BoxType.Block);
                colBox.ContentRect = new RectF(colX, startY, columnWidth, columnHeight);

                // Compute Y offset: shift first child's margin-box top to startY
                float yOffset = 0;
                if (colChildren[col].Count > 0)
                {
                    var first = colChildren[col][0];
                    float firstMarginBoxTop = first.ContentRect.Y - first.PaddingTop
                                            - first.BorderTopWidth - first.MarginTop;
                    yOffset = startY - firstMarginBoxTop;
                }

                float colBottom = startY;
                foreach (var child in colChildren[col])
                {
                    var offsetChild = OffsetBox(child, xOffset, yOffset);
                    colBox.AddChild(offsetChild);

                    float childBottomY = offsetChild.ContentRect.Y + offsetChild.ContentRect.Height
                                       + offsetChild.PaddingBottom + offsetChild.BorderBottomWidth
                                       + offsetChild.MarginBottom;
                    if (childBottomY > colBottom) colBottom = childBottomY;
                }

                // Handle line boxes similarly
                if (colLineBoxes[col].Count > 0)
                {
                    float lineYOffset = 0;
                    if (colLineBoxes[col].Count > 0)
                    {
                        lineYOffset = startY - colLineBoxes[col][0].Y;
                    }

                    var colLines = new List<LineBox>();
                    foreach (var line in colLineBoxes[col])
                    {
                        var newLine = new LineBox
                        {
                            X = colX,
                            Y = line.Y + lineYOffset,
                            Width = columnWidth,
                            Height = line.Height,
                            Baseline = line.Baseline
                        };
                        foreach (var frag in line.Fragments)
                        {
                            newLine.AddFragment(new LineFragment
                            {
                                X = frag.X,
                                Y = frag.Y,
                                Width = frag.Width,
                                Height = frag.Height,
                                Baseline = frag.Baseline,
                                Text = frag.Text,
                                ShapedRun = frag.ShapedRun,
                                Box = frag.Box,
                                InlineElement = frag.InlineElement,
                                StyleOverride = frag.StyleOverride
                            });
                        }
                        colLines.Add(newLine);

                        float lineBottom = line.Y + lineYOffset + line.Height;
                        if (lineBottom > colBottom) colBottom = lineBottom;
                    }
                    colBox.LineBoxes = colLines;
                }

                float colActualHeight = colBottom - startY;
                if (colActualHeight > tallestColumn)
                    tallestColumn = colActualHeight;

                box.AddChild(colBox);

                // Add column rule between columns (except before first)
                if (col > 0)
                {
                    var ruleStyle = style.ColumnRuleStyle;
                    if (ruleStyle != CssBorderStyle.None && ruleStyle != CssBorderStyle.Hidden)
                    {
                        float ruleWidth = style.ColumnRuleWidth;
                        if (ruleWidth > 0)
                        {
                            var ruleColor = style.ColumnRuleColor;
                            if (ruleColor.A > 0)
                            {
                                if (box.ColumnRules == null)
                                    box.ColumnRules = new List<ColumnRuleInfo>();
                                float ruleX = colX - columnGap / 2;
                                box.ColumnRules.Add(new ColumnRuleInfo
                                {
                                    X = ruleX,
                                    Y = startY,
                                    Height = tallestColumn > 0 ? tallestColumn : columnHeight,
                                    Width = ruleWidth,
                                    Style = ruleStyle,
                                    Color = ruleColor
                                });
                            }
                        }
                    }
                }
            }

            // Set the box height to the tallest column
            float finalHeight = tallestColumn > 0 ? tallestColumn : columnHeight;
            box.ContentRect = new RectF(
                box.ContentRect.X, box.ContentRect.Y,
                box.ContentRect.Width, finalHeight);
        }

        /// <summary>
        /// Handles multi-column layout when column-span: all elements are present.
        /// Content is split into segments: multi-column sections interleaved with
        /// full-width spanning elements.
        /// </summary>
        private static void LayoutWithSpanners(LayoutBox box, StyledElement styledElement,
            LayoutContext context, int columnCount, float columnWidth, float columnGap)
        {
            float startX = box.ContentRect.X;
            float cursorY = box.ContentRect.Y;
            float availableWidth = box.ContentRect.Width;

            // Collect segments: groups of non-spanning children, separated by spanning elements
            var currentSegment = new List<StyledNode>();

            for (int i = 0; i < styledElement.Children.Count; i++)
            {
                var child = styledElement.Children[i];
                bool isSpanner = false;

                if (!child.IsText && !(child is StyledPseudoElement) && child is StyledElement childEl)
                {
                    if (childEl.Style.ColumnSpan == CssColumnSpan.All)
                        isSpanner = true;
                }

                if (isSpanner)
                {
                    // Layout the accumulated segment as multi-column
                    if (currentSegment.Count > 0)
                    {
                        cursorY = LayoutSegmentAsColumns(box, currentSegment, context,
                            columnCount, columnWidth, columnGap, startX, cursorY);
                        currentSegment.Clear();
                    }

                    // Layout the spanning element at full width
                    var spanEl = (StyledElement)child;
                    var spanBox = new LayoutBox(spanEl, BoxType.Block);
                    BoxModelCalculator.ApplyBoxModel(spanBox, spanEl.Style, availableWidth);
                    float spanWidth = DimensionResolver.ResolveWidth(spanEl.Style, availableWidth, spanBox);
                    float spanX = startX + spanBox.MarginLeft + spanBox.BorderLeftWidth + spanBox.PaddingLeft;
                    float spanY = cursorY + spanBox.MarginTop + spanBox.BorderTopWidth + spanBox.PaddingTop;
                    spanBox.ContentRect = new RectF(spanX, spanY, spanWidth, 0);

                    // Layout the spanner's children
                    if (spanEl.Children.Count > 0)
                    {
                        if (HasBlockChildren(spanEl))
                            BlockFormattingContext.Layout(spanBox, context);
                        else
                            InlineFormattingContext.Layout(spanBox, context);
                    }

                    float spanHeight = DimensionResolver.ResolveHeight(spanEl.Style, float.NaN, spanBox);
                    if (float.IsNaN(spanHeight))
                    {
                        spanHeight = CalculateContentHeight(spanBox);
                    }
                    spanBox.ContentRect = new RectF(spanX, spanY, spanWidth, spanHeight);
                    box.AddChild(spanBox);

                    cursorY = spanY + spanHeight + spanBox.PaddingBottom + spanBox.BorderBottomWidth + spanBox.MarginBottom;
                }
                else
                {
                    currentSegment.Add(child);
                }
            }

            // Layout any remaining segment
            if (currentSegment.Count > 0)
            {
                cursorY = LayoutSegmentAsColumns(box, currentSegment, context,
                    columnCount, columnWidth, columnGap, startX, cursorY);
            }

            box.ContentRect = new RectF(
                box.ContentRect.X, box.ContentRect.Y,
                box.ContentRect.Width, cursorY - box.ContentRect.Y);
        }

        /// <summary>
        /// Lays out a segment of children in multi-column format, returning the Y position after layout.
        /// </summary>
        private static float LayoutSegmentAsColumns(LayoutBox parent, List<StyledNode> children,
            LayoutContext context, int columnCount, float columnWidth, float columnGap,
            float startX, float startY)
        {
            // Create a wrapper element with ONLY the segment children for BFC layout.
            // Using the parent element directly would lay out ALL children (including spanners
            // and other segments), causing content duplication.
            var parentElement = parent.StyledNode as StyledElement;
            if (parentElement == null) return startY;

            var segmentWrapper = new StyledElement(parentElement.Element, parentElement.Style,
                new List<StyledNode>(children));
            var tempBox = new LayoutBox(segmentWrapper, BoxType.Block);
            tempBox.ContentRect = new RectF(startX, startY, columnWidth, 0);

            // Lay out as block context to measure height
            BlockFormattingContext.Layout(tempBox, context);

            float totalHeight = CalculateContentHeight(tempBox);
            if (totalHeight <= 0) return startY;

            float targetHeight = Math.Max(totalHeight / columnCount, 1);

            // Sequential content-aware fragmentation
            var segColChildren = new List<LayoutBox>[columnCount];
            for (int i = 0; i < columnCount; i++)
                segColChildren[i] = new List<LayoutBox>();

            int curCol = 0;
            float curColH = 0;
            foreach (var child in tempBox.Children)
            {
                float childVisH = child.BorderRect.Height + child.MarginTop;
                float childFullH = childVisH + child.MarginBottom;
                if (curColH > 0 && curColH + childVisH > targetHeight && curCol < columnCount - 1)
                {
                    curCol++;
                    curColH = 0;
                }
                segColChildren[curCol].Add(child);
                curColH += childFullH;
            }

            float tallest = 0;
            for (int col = 0; col < columnCount; col++)
            {
                float colX = startX + col * (columnWidth + columnGap);
                float xOffset = colX - tempBox.ContentRect.X;

                var colBox = new LayoutBox(null, BoxType.Block);
                colBox.ContentRect = new RectF(colX, startY, columnWidth, targetHeight);

                float yOffset = 0;
                if (segColChildren[col].Count > 0)
                {
                    var first = segColChildren[col][0];
                    float firstMarginBoxTop = first.ContentRect.Y - first.PaddingTop
                                            - first.BorderTopWidth - first.MarginTop;
                    yOffset = startY - firstMarginBoxTop;
                }

                float colBottom = startY;
                foreach (var child in segColChildren[col])
                {
                    var offsetChild = OffsetBox(child, xOffset, yOffset);
                    colBox.AddChild(offsetChild);
                    float childBottomY = offsetChild.ContentRect.Y + offsetChild.ContentRect.Height
                                       + offsetChild.PaddingBottom + offsetChild.BorderBottomWidth
                                       + offsetChild.MarginBottom;
                    if (childBottomY > colBottom) colBottom = childBottomY;
                }

                float colH = colBottom - startY;
                if (colH > tallest) tallest = colH;

                parent.AddChild(colBox);
            }

            return startY + (tallest > 0 ? tallest : targetHeight);
        }

        /// <summary>
        /// Binary search for the minimum column height that allows all content
        /// (block children and/or line boxes) to fit within the given number of columns.
        /// </summary>
        private static float BinarySearchColumnHeight(LayoutBox columnBox, int columnCount, float contentHeight)
        {
            if (contentHeight <= 0 || columnCount <= 1)
                return contentHeight;

            // Collect item heights for fragmentation simulation
            var itemHeights = new List<float>();
            foreach (var child in columnBox.Children)
            {
                float h = child.BorderRect.Height + child.MarginTop + child.MarginBottom;
                itemHeights.Add(Math.Max(h, 0));
            }
            if (columnBox.LineBoxes != null)
            {
                foreach (var line in columnBox.LineBoxes)
                    itemHeights.Add(line.Height);
            }

            if (itemHeights.Count == 0)
                return contentHeight;

            // Find the tallest single item (minimum possible column height)
            float maxItem = 0;
            for (int i = 0; i < itemHeights.Count; i++)
                if (itemHeights[i] > maxItem) maxItem = itemHeights[i];

            float lo = maxItem;
            float hi = contentHeight;

            // Binary search: find minimum height where items fit in columnCount columns
            for (int iter = 0; iter < 20 && hi - lo > 0.5f; iter++)
            {
                float mid = (lo + hi) * 0.5f;
                if (FitsInColumns(itemHeights, columnCount, mid))
                    hi = mid;
                else
                    lo = mid;
            }

            // Add a small epsilon to avoid edge-case rounding issues
            return hi + 0.5f;
        }

        /// <summary>
        /// Check if items fit within the given number of columns at the specified height.
        /// </summary>
        private static bool FitsInColumns(List<float> itemHeights, int columnCount, float columnHeight)
        {
            int col = 0;
            float currentHeight = 0;
            for (int i = 0; i < itemHeights.Count; i++)
            {
                float h = itemHeights[i];
                if (currentHeight > 0 && currentHeight + h > columnHeight)
                {
                    col++;
                    if (col >= columnCount) return false;
                    currentHeight = 0;
                }
                currentHeight += h;
            }
            return true;
        }

        private static int ResolveColumnCount(ComputedStyle style, float availableWidth, float gap)
        {
            float specCount = style.ColumnCount;
            float specWidth = style.ColumnWidth;

            bool hasCount = !float.IsNaN(specCount) && specCount >= 1;
            bool hasWidth = !float.IsNaN(specWidth) && specWidth > 0;

            if (hasCount && hasWidth)
            {
                // Both specified: column-count is the maximum
                int maxByWidth = Math.Max(1, (int)Math.Floor((availableWidth + gap) / (specWidth + gap)));
                return Math.Min((int)specCount, maxByWidth);
            }

            if (hasCount)
                return (int)specCount;

            if (hasWidth)
                return Math.Max(1, (int)Math.Floor((availableWidth + gap) / (specWidth + gap)));

            return 1;
        }

        private static LayoutBox CreateTempBox(LayoutBox original, StyledElement element, float width)
        {
            var temp = new LayoutBox(element, BoxType.Block);
            temp.ContentRect = new RectF(
                original.ContentRect.X,
                original.ContentRect.Y,
                width,
                0);
            temp.PaddingTop = 0;
            temp.PaddingRight = 0;
            temp.PaddingBottom = 0;
            temp.PaddingLeft = 0;
            temp.BorderTopWidth = 0;
            temp.BorderRightWidth = 0;
            temp.BorderBottomWidth = 0;
            temp.BorderLeftWidth = 0;
            return temp;
        }

        private static float CalculateContentHeight(LayoutBox box)
        {
            float height = 0;
            foreach (var child in box.Children)
            {
                float childBottom = child.BorderRect.Bottom - box.ContentRect.Y;
                if (childBottom > height) height = childBottom;
            }
            if (box.LineBoxes != null)
            {
                foreach (var line in box.LineBoxes)
                {
                    float lineBottom = line.Y + line.Height - box.ContentRect.Y;
                    if (lineBottom > height) height = lineBottom;
                }
            }
            return height;
        }

        private static bool HasBlockChildren(StyledElement element)
        {
            for (int i = 0; i < element.Children.Count; i++)
            {
                var child = element.Children[i];
                if (child.IsText || child is StyledPseudoElement) continue;
                var childElement = (StyledElement)child;
                var display = childElement.Style.Display;
                if (display == CssDisplay.Block || display == CssDisplay.Flex ||
                    display == CssDisplay.Grid || display == CssDisplay.Table ||
                    display == CssDisplay.ListItem)
                    return true;
            }
            return false;
        }

        private static void CopyLayoutResult(LayoutBox source, LayoutBox target)
        {
            foreach (var child in source.Children)
                target.AddChild(child);
            target.LineBoxes = source.LineBoxes;
        }

        private static LayoutBox OffsetBox(LayoutBox original, float offsetX, float offsetY)
        {
            var box = new LayoutBox(original.StyledNode, original.BoxType);
            box.ContentRect = new RectF(
                original.ContentRect.X + offsetX,
                original.ContentRect.Y + offsetY,
                original.ContentRect.Width,
                original.ContentRect.Height);
            box.PaddingTop = original.PaddingTop;
            box.PaddingRight = original.PaddingRight;
            box.PaddingBottom = original.PaddingBottom;
            box.PaddingLeft = original.PaddingLeft;
            box.BorderTopWidth = original.BorderTopWidth;
            box.BorderRightWidth = original.BorderRightWidth;
            box.BorderBottomWidth = original.BorderBottomWidth;
            box.BorderLeftWidth = original.BorderLeftWidth;
            box.MarginTop = original.MarginTop;
            box.MarginRight = original.MarginRight;
            box.MarginBottom = original.MarginBottom;
            box.MarginLeft = original.MarginLeft;

            // Offset children
            foreach (var child in original.Children)
                box.AddChild(OffsetBox(child, offsetX, offsetY));

            // Offset line boxes
            if (original.LineBoxes != null)
            {
                var lines = new List<LineBox>();
                foreach (var line in original.LineBoxes)
                {
                    var newLine = new LineBox
                    {
                        X = line.X + offsetX,
                        Y = line.Y + offsetY,
                        Width = line.Width,
                        Height = line.Height,
                        Baseline = line.Baseline
                    };
                    foreach (var frag in line.Fragments)
                    {
                        newLine.AddFragment(new LineFragment
                        {
                            X = frag.X,
                            Y = frag.Y,
                            Width = frag.Width,
                            Height = frag.Height,
                            Baseline = frag.Baseline,
                            Text = frag.Text,
                            ShapedRun = frag.ShapedRun,
                            Box = frag.Box,
                            InlineElement = frag.InlineElement,
                            StyleOverride = frag.StyleOverride
                        });
                    }
                    lines.Add(newLine);
                }
                box.LineBoxes = lines;
            }

            return box;
        }
    }
}
