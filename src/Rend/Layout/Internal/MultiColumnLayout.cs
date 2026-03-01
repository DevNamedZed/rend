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

            // Calculate target column height (balanced)
            float targetHeight = Math.Max(totalHeight / columnCount, style.FontSize * 2);

            // Second pass: layout content constrained to column width
            var columnBox = CreateTempBox(box, styledElement, columnWidth);
            if (HasBlockChildren(styledElement))
                BlockFormattingContext.Layout(columnBox, context);
            else
                InlineFormattingContext.Layout(columnBox, context);

            float contentHeight = CalculateContentHeight(columnBox);

            // Distribute content across columns
            // For simplicity, we treat the column layout as a single tall column
            // and clip/translate children into column positions
            float columnHeight = Math.Max(targetHeight, contentHeight / columnCount);
            if (columnHeight < 1) columnHeight = contentHeight;

            // Create column boxes
            float startX = box.ContentRect.X;
            float startY = box.ContentRect.Y;

            for (int col = 0; col < columnCount; col++)
            {
                float colX = startX + col * (columnWidth + columnGap);
                float colStartY = col * columnHeight;
                float colEndY = colStartY + columnHeight;

                // Create a column wrapper box
                var colBox = new LayoutBox(null, BoxType.Block);
                colBox.ContentRect = new RectF(colX, startY, columnWidth, columnHeight);

                // Copy children that fall within this column's Y range
                foreach (var child in columnBox.Children)
                {
                    float childTop = child.BorderRect.Top - columnBox.ContentRect.Y;
                    float childBottom = child.BorderRect.Bottom - columnBox.ContentRect.Y;

                    if (childBottom > colStartY && childTop < colEndY)
                    {
                        // Offset child into column position
                        float offsetY = startY - colStartY;
                        var offsetChild = OffsetBox(child, colX - child.ContentRect.X + child.PaddingLeft + child.BorderLeftWidth, offsetY);
                        colBox.AddChild(offsetChild);
                    }
                }

                // Copy line boxes that fall within this column
                if (columnBox.LineBoxes != null)
                {
                    var colLines = new List<LineBox>();
                    foreach (var line in columnBox.LineBoxes)
                    {
                        float lineY = line.Y - columnBox.ContentRect.Y;
                        if (lineY + line.Height > colStartY && lineY < colEndY)
                        {
                            var offsetLine = new LineBox
                            {
                                X = colX,
                                Y = line.Y - colStartY + startY,
                                Width = columnWidth,
                                Height = line.Height,
                                Baseline = line.Baseline
                            };

                            foreach (var frag in line.Fragments)
                            {
                                var newFrag = new LineFragment
                                {
                                    X = frag.X - columnBox.ContentRect.X + colX,
                                    Y = frag.Y,
                                    Width = frag.Width,
                                    Height = frag.Height,
                                    Baseline = frag.Baseline,
                                    Text = frag.Text,
                                    ShapedRun = frag.ShapedRun,
                                    Box = frag.Box,
                                    InlineElement = frag.InlineElement,
                                    StyleOverride = frag.StyleOverride
                                };
                                offsetLine.AddFragment(newFrag);
                            }
                            colLines.Add(offsetLine);
                        }
                    }
                    colBox.LineBoxes = colLines;
                }

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
                                    Height = columnHeight,
                                    Width = ruleWidth,
                                    Style = ruleStyle,
                                    Color = ruleColor
                                });
                            }
                        }
                    }
                }
            }

            // Set the box height to the column height
            box.ContentRect = new RectF(
                box.ContentRect.X, box.ContentRect.Y,
                box.ContentRect.Width, columnHeight);
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
            // Create a wrapper element with just these children for BFC layout
            var parentElement = parent.StyledNode as StyledElement;
            if (parentElement == null) return startY;

            var tempBox = new LayoutBox(parentElement, BoxType.Block);
            tempBox.ContentRect = new RectF(startX, startY, columnWidth, 0);

            // Lay out as block context to measure height
            BlockFormattingContext.Layout(tempBox, context);

            float totalHeight = CalculateContentHeight(tempBox);
            if (totalHeight <= 0) return startY;

            float targetHeight = Math.Max(totalHeight / columnCount, 1);

            // Distribute content across columns
            for (int col = 0; col < columnCount; col++)
            {
                float colX = startX + col * (columnWidth + columnGap);
                float colStartY = col * targetHeight;
                float colEndY = colStartY + targetHeight;

                var colBox = new LayoutBox(null, BoxType.Block);
                colBox.ContentRect = new RectF(colX, startY, columnWidth, targetHeight);

                foreach (var child in tempBox.Children)
                {
                    float childTop = child.BorderRect.Top - tempBox.ContentRect.Y;
                    float childBottom = child.BorderRect.Bottom - tempBox.ContentRect.Y;

                    if (childBottom > colStartY && childTop < colEndY)
                    {
                        float offsetY = startY - colStartY;
                        var offsetChild = OffsetBox(child,
                            colX - child.ContentRect.X + child.PaddingLeft + child.BorderLeftWidth,
                            offsetY);
                        colBox.AddChild(offsetChild);
                    }
                }

                if (tempBox.LineBoxes != null)
                {
                    var colLines = new List<LineBox>();
                    foreach (var line in tempBox.LineBoxes)
                    {
                        float lineY = line.Y - tempBox.ContentRect.Y;
                        if (lineY + line.Height > colStartY && lineY < colEndY)
                        {
                            var newLine = new LineBox
                            {
                                X = colX,
                                Y = line.Y - colStartY + startY,
                                Width = columnWidth,
                                Height = line.Height,
                                Baseline = line.Baseline
                            };
                            foreach (var frag in line.Fragments)
                            {
                                newLine.AddFragment(new LineFragment
                                {
                                    X = frag.X - tempBox.ContentRect.X + colX,
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
                        }
                    }
                    colBox.LineBoxes = colLines;
                }

                parent.AddChild(colBox);
            }

            return startY + targetHeight;
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
                            X = frag.X + offsetX,
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
