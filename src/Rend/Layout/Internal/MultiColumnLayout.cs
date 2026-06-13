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

            // [CSS-BREAK-3 §5] Track multicol nesting so an inner multicol can
            // detect that it is being fragmented by an outer fragmentation root.
            // The captured value is the depth observed *before* this multicol was
            // pushed onto the stack — non-zero means at least one ancestor multicol
            // is currently fragmenting our box.
            bool isNestedInMulticol = context.MultiColumnNestingDepth > 0;
            context.MultiColumnNestingDepth++;
            try
            {
                LayoutCore(box, styledElement, context, isNestedInMulticol);
            }
            finally
            {
                context.MultiColumnNestingDepth--;
            }
        }

        private static void LayoutCore(LayoutBox box, StyledElement styledElement,
            LayoutContext context, bool isNestedInMulticol)
        {
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

            // [CSS-MULTICOL §7.2] column-span: all applies to the nearest multicol
            // ancestor, even when nested inside in-flow wrappers. Hoistable spanners
            // are gathered into a flat segment list; wrappers with content around a
            // spanner are split into pre/post fragments.
            if (columnCount > 1 && HasHoistableSpanner(styledElement))
            {
                var segmentList = BuildHoistedSegmentList(styledElement);
                LayoutWithSpanners(box, segmentList, context, columnCount, columnWidth, columnGap, isNestedInMulticol);
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

            // [CSS-MULTICOL §7.1] The multicol container's specified/resolved
            // block size constrains the column height. Prefer box.ContentRect.Height
            // (already clamped by parent BFC through ResolveHeight, so min-height/
            // max-height and box-sizing are honoured) over the raw style.Height.
            float specifiedHeight = box.ContentRect.Height;
            if (specifiedHeight <= 0 && !float.IsNaN(style.Height))
            {
                specifiedHeight = style.Height;
            }

            // [CSS-BREAK-3 §5.1] Forced column breaks on direct children split the
            // flow into segments. When more segments exist than columns, excess
            // segments become "virtual columns" per Chrome's implementation
            // (see chromium issue 385595003): they are not painted and their
            // height does not contribute to the balanced column height.
            //
            // The wrapper-fragmentation path handles multicols that contain a
            // single fragmentable in-flow wrapper whose grandchildren carry the
            // forced breaks — the wrapper is split into one fragment per
            // segment so its background is replicated in each column.
            var wrapperFragments = TryFragmentWrapperOnForcedBreaks(columnBox);
            if (wrapperFragments != null)
            {
                columnBox.ClearChildren();
                for (int i = 0; i < wrapperFragments.Count; i++)
                {
                    columnBox.AddChild(wrapperFragments[i]);
                }
            }

            var forcedSegments = BuildForcedBreakSegments(columnBox.Children);
            if (wrapperFragments != null)
            {
                // Each wrapper fragment is already its own segment even though
                // there is no break-before/after on them — the wrapper
                // fragmentation pass created them across forced-break seams on
                // grandchildren.
                forcedSegments = new List<List<LayoutBox>>();
                for (int i = 0; i < wrapperFragments.Count; i++)
                {
                    forcedSegments.Add(new List<LayoutBox> { wrapperFragments[i] });
                }
            }
            bool hasForcedBreaks = forcedSegments.Count > 1;

            // Calculate target column height
            float targetHeight;
            if (hasForcedBreaks && style.ColumnFill == CssColumnFill.Balance)
            {
                // [CSS-MULTICOL §3.3] Balance with forced breaks: the used column
                // height is the tallest segment that lands in a real column.
                // Segments assigned to virtual columns (index >= columnCount) do
                // not contribute.
                float forcedBalancedHeight = MeasureRealSegmentMaxHeight(forcedSegments, columnCount);
                if (specifiedHeight > 0)
                {
                    // Balance with specified height: used col height may be less
                    // than the container's specified block size.
                    targetHeight = Math.Min(forcedBalancedHeight, specifiedHeight);
                }
                else
                {
                    targetHeight = forcedBalancedHeight;
                }
            }
            else if (specifiedHeight > 0)
            {
                // [CSS-MULTICOL §7.1] When the multicol element has a specified
                // block size, the column height is the specified height. Both
                // column-fill:auto and column-fill:balance respect this — the
                // fill mode only affects how content flows between columns, not
                // the column height itself.
                targetHeight = specifiedHeight;
            }
            else if (style.ColumnFill == CssColumnFill.Auto)
            {
                // Auto without specified height: use measured content height.
                targetHeight = totalHeight;
            }
            else
            {
                // [CSS-MULTICOL §3.3] Balance: Chrome's ColumnBalancer starts with
                // contentHeight / columnCount and validates it. Binary search only
                // when the even-distribution estimate doesn't fit (e.g., break-avoid
                // groups or unbreakable blocks larger than the estimate).
                float initialEstimate = contentHeight / columnCount;
                var groups = BuildBreakGroups(columnBox.Children);
                if (FitsInColumnsWithSplitting(columnBox, columnCount, initialEstimate, groups))
                {
                    targetHeight = initialEstimate;
                }
                else
                {
                    targetHeight = BinarySearchColumnHeight(columnBox, columnCount, contentHeight);
                }
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

            if (hasForcedBreaks)
            {
                // [CSS-BREAK-3 §5.1] Segment-based column assignment. Each
                // forced-break segment is assigned to its own column in order.
                // Excess segments (index >= columnCount) are placed in virtual
                // columns that are not painted.
                for (int segIdx = 0; segIdx < forcedSegments.Count; segIdx++)
                {
                    if (segIdx >= columnCount)
                    {
                        break;
                    }
                    var segment = forcedSegments[segIdx];
                    for (int i = 0; i < segment.Count; i++)
                    {
                        colChildren[segIdx].Add(segment[i]);
                    }
                }

                // [CSS-MULTICOL §3.3] Wrapper fragments across real columns
                // stretch to the balanced column height so the wrapper's
                // background replicates across every column it occupies. The
                // grandchildren carried by the fragment stay anchored at the
                // fragment's top — only the fragment's block size grows.
                if (wrapperFragments != null)
                {
                    int stretchCount = Math.Min(wrapperFragments.Count, columnCount);
                    for (int i = 0; i < stretchCount; i++)
                    {
                        var fragment = wrapperFragments[i];
                        fragment.ContentRect = new RectF(
                            fragment.ContentRect.X,
                            fragment.ContentRect.Y,
                            fragment.ContentRect.Width,
                            columnHeight);
                    }
                }
                goto afterMainFill;
            }

            // Position-based column filling: use BFC-computed positions (which already
            // account for margin collapsing) to determine column breaks. Track the Y
            // position where the current column started rather than accumulating heights.
            // Pending list allows a child to be replaced by its continuation fragment
            // after atomic/line splits for N-way splitting across columns.
            // [CSS-BREAK-3 §5] break-avoid linkage between siblings is soft and easily
            // violated when it would leave usable space unused, so this loop does not
            // coalesce siblings into groups — avoid linkage only influences the column
            // height chosen by BinarySearchColumnHeight.
            {
            var pendingChildren = new List<LayoutBox>(columnBox.Children);
            int currentCol = 0;
            float colStartY = contentOriginY;
            bool colHasContent = false;
            int childIdx = 0;
            while (childIdx < pendingChildren.Count)
            {
                var child = pendingChildren[childIdx];
                float childBottomY = child.BorderRect.Bottom;
                float heightInCol = childBottomY - colStartY;
                bool overflows = heightInCol > columnHeight && currentCol < columnCount - 1;

                if (overflows)
                {
                    // [CSS-BREAK-3 §5] break-inside: avoid is a soft preference. When the
                    // element is taller than the column itself, there is no way to honour
                    // it — allow the break anyway. Otherwise, honour it by moving the
                    // whole element to the next column instead of splitting.
                    float childHeight = child.BorderRect.Height;
                    bool breakInsideAvoid = HasBreakInsideAvoid(child) && childHeight <= columnHeight;

                    // [CSS-FRAGMENTATION §4] Atomic block with no internal fragmentation
                    // points (no children, no line boxes). Split the background/padding box
                    // at the column boundary so the remaining content flows into the next
                    // column. Atomic split makes progress even on the first child of a
                    // column because the split position is strictly greater than colStartY.
                    if (!breakInsideAvoid
                        && (child.Children == null || child.Children.Count == 0)
                        && (child.LineBoxes == null || child.LineBoxes.Count == 0))
                    {
                        float splitY = colStartY + columnHeight;
                        var atomicSplit = SplitAtomicBoxAtY(child, splitY);
                        if (atomicSplit.IsValid)
                        {
                            colChildren[currentCol].Add(atomicSplit.First!);
                            currentCol++;
                            colStartY = splitY;
                            colHasContent = false;
                            pendingChildren[childIdx] = atomicSplit.Second!;
                            continue;
                        }
                    }

                    // CSS Fragmentation Level 3: split block at line boundaries when it
                    // overflows the remaining column space, for balanced column layout.
                    if (!breakInsideAvoid
                        && colHasContent
                        && child.LineBoxes != null
                        && child.LineBoxes.Count > 1)
                    {
                        float availableForLines = colStartY + columnHeight - child.ContentRect.Y;
                        if (availableForLines > 0)
                        {
                            float contentStartY = child.ContentRect.Y;
                            int splitAfter = -1;
                            for (int li = 0; li < child.LineBoxes.Count; li++)
                            {
                                float lineBottom = child.LineBoxes[li].Y + child.LineBoxes[li].Height - contentStartY;
                                if (lineBottom <= availableForLines)
                                {
                                    splitAfter = li;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (splitAfter >= 0 && splitAfter < child.LineBoxes.Count - 1)
                            {
                                var split = SplitBoxAtLine(child, splitAfter);
                                if (split.IsValid)
                                {
                                    colChildren[currentCol].Add(split.First!);
                                    currentCol++;
                                    colStartY = child.LineBoxes[splitAfter + 1].Y;
                                    colHasContent = true;
                                    colChildren[currentCol].Add(split.Second!);
                                    childIdx++;
                                    continue;
                                }
                            }
                        }
                    }

                    // [CSS-BREAK-3 §5.1] Block container class-C break points.
                    // When a tall child has block children (not line boxes) and
                    // is not atomic — typically a nested multicol or a plain
                    // block wrapper — fragment at the column boundary by
                    // partitioning its block children. Straddling children are
                    // recursively split via SplitBlockBoxAtY. First half stays
                    // in the current column; second half is re-queued so the
                    // next iteration can split it again if it is still taller
                    // than the remaining column space. Monolithic boxes
                    // (scrolling containers, atomic inlines, contain:size,
                    // break-inside:avoid) fall through to the whole-child move
                    // path instead. Only split when the child itself is taller
                    // than a full column — otherwise the move-whole fallback
                    // keeps the box intact in the next column, which matches
                    // Chrome's preference to avoid unnecessary fragmentation.
                    if (!breakInsideAvoid
                        && !IsMonolithic(child)
                        && childHeight >= columnHeight
                        && child.Children != null
                        && child.Children.Count > 0
                        && (child.LineBoxes == null || child.LineBoxes.Count == 0))
                    {
                        float splitY = colStartY + columnHeight;
                        var blockSplit = SplitBlockBoxAtY(child, splitY);
                        if (blockSplit.IsValid)
                        {
                            colChildren[currentCol].Add(blockSplit.First!);
                            currentCol++;
                            colStartY = splitY;
                            colHasContent = false;
                            pendingChildren[childIdx] = blockSplit.Second!;
                            continue;
                        }
                    }

                    // No split possible — move whole child to next column
                    if (colHasContent)
                    {
                        currentCol++;
                        colStartY = child.BorderRect.Y;
                        colHasContent = false;
                    }
                }

                colChildren[currentCol].Add(child);
                colHasContent = true;
                childIdx++;
            }
            }

            afterMainFill:

            // Assign line boxes to columns (for inline content).
            // Use position-based tracking (matching block child assignment)
            // to avoid accumulated rounding errors from summing heights.
            if (columnBox.LineBoxes != null && columnBox.LineBoxes.Count > 0)
            {
                int lineCol = 0;
                float lineColStartY = columnBox.LineBoxes[0].Y;
                bool lineColHasContent = false;
                foreach (var line in columnBox.LineBoxes)
                {
                    float lineBottomY = line.Y + line.Height;
                    float heightInCol = lineBottomY - lineColStartY;
                    if (lineColHasContent && heightInCol > columnHeight
                        && lineCol < columnCount - 1)
                    {
                        lineCol++;
                        lineColStartY = line.Y;
                    }
                    colLineBoxes[lineCol].Add(line);
                    lineColHasContent = true;
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
                                StyleOverride = frag.StyleOverride,
                                JustifyWordSpacing = frag.JustifyWordSpacing
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

                // Update column box height to reflect actual content extent
                colBox.ContentRect = new RectF(colBox.ContentRect.X, colBox.ContentRect.Y,
                    colBox.ContentRect.Width, colActualHeight);

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
                                float ruleX = (float)Math.Round(colX - columnGap / 2);
                                // [CSS-MULTICOL §4.4] Column rules span the multicol
                                // container's content height (= balanced column height).
                                float ruleHeight = columnHeight;
                                box.ColumnRules.Add(new ColumnRuleInfo
                                {
                                    X = ruleX,
                                    Y = startY,
                                    Height = ruleHeight,
                                    Width = ruleWidth,
                                    Style = ruleStyle,
                                    Color = ruleColor
                                });
                            }
                        }
                    }
                }
            }

            // [CSS-MULTICOL §7.1] The multicol box keeps its specified block size
            // when one is set. With forced breaks present, the used column height
            // can be less than the specified height (balance mode shrinks columns
            // to the tallest real segment), but the multicol container itself
            // still occupies the specified block size — Chrome paints the area
            // between the column strip bottom and the container bottom using the
            // multicol's own background.
            // [CSS-MULTICOL §3.3] The multicol container's block size equals:
            // - specified height when set with forced breaks,
            // - the balanced column height when balancing (Chrome's ColumnBalancer),
            // - the tallest column's extent otherwise (column-fill: auto).
            float finalHeight;
            if (specifiedHeight > 0 && hasForcedBreaks)
            {
                finalHeight = specifiedHeight;
            }
            else if (style.ColumnFill == CssColumnFill.Auto || specifiedHeight > 0)
            {
                finalHeight = tallestColumn > 0 ? tallestColumn : columnHeight;
            }
            else
            {
                finalHeight = columnHeight;
            }
            box.ContentRect = new RectF(
                box.ContentRect.X, box.ContentRect.Y,
                box.ContentRect.Width, finalHeight);
        }

        /// <summary>
        /// Handles multi-column layout when column-span: all elements are present.
        /// Consumes a pre-flattened list where hoistable nested spanners appear at
        /// the top level interleaved with block content fragments. See
        /// <see cref="BuildHoistedSegmentList"/>.
        /// </summary>
        private static void LayoutWithSpanners(LayoutBox box, List<StyledNode> segmentList,
            LayoutContext context, int columnCount, float columnWidth, float columnGap,
            bool isNestedInMulticol)
        {
            float startX = box.ContentRect.X;
            float cursorY = box.ContentRect.Y;
            float availableWidth = box.ContentRect.Width;
            float prevMarginBottom = 0;

            // [CSS-MULTICOL §7.1] The multicol container's definite height is the
            // containing block height for percentage resolution of all direct children
            // (both column content and spanners).
            float containerHeight = box.ContentRect.Height;
            if (float.IsNaN(containerHeight) || containerHeight <= 0)
            {
                var parentStyle = (box.StyledNode as StyledElement)?.Style;
                if (parentStyle != null && !float.IsNaN(parentStyle.Height) && parentStyle.Height > 0)
                {
                    containerHeight = parentStyle.Height;
                }
            }
            var currentSegment = new List<StyledNode>();

            for (int i = 0; i < segmentList.Count; i++)
            {
                var child = segmentList[i];
                bool isSpanner = false;

                if (!child.IsText && !(child is StyledPseudoElement) && child is StyledElement childEl)
                {
                    if (childEl.Style.ColumnSpan == CssColumnSpan.All)
                    {
                        isSpanner = true;
                    }
                }

                if (isSpanner)
                {
                    // Layout the accumulated segment as multi-column
                    if (currentSegment.Count > 0)
                    {
                        // [CSS-MULTICOL §7.1] Constrain segment column height to remaining
                        // available space in a fixed-height multicol container.
                        float remainingHeight = float.NaN;
                        if (!float.IsNaN(containerHeight) && containerHeight > 0)
                        {
                            remainingHeight = containerHeight - (cursorY - box.ContentRect.Y);
                            if (remainingHeight < 0) { remainingHeight = 0; }
                        }

                        float segTrailingMargin;
                        cursorY = LayoutSegmentAsColumns(box, currentSegment, context,
                            columnCount, columnWidth, columnGap, startX, cursorY, out segTrailingMargin,
                            isNestedInMulticol, remainingHeight);
                        prevMarginBottom = segTrailingMargin;
                        currentSegment.Clear();
                    }

                    // Layout the spanning element at full width
                    var spanEl = (StyledElement)child;
                    var spanBox = new LayoutBox(spanEl, BoxType.Block);
                    BoxModelCalculator.ApplyBoxModel(spanBox, spanEl.Style, availableWidth);
                    float spanWidth = DimensionResolver.ResolveWidth(spanEl.Style, availableWidth, spanBox);
                    // Collapse margins between previous segment/spanner and this spanner
                    float collapsedMargin = MarginCollapsing.Collapse(prevMarginBottom, spanBox.MarginTop);
                    float spanX = startX + spanBox.MarginLeft + spanBox.BorderLeftWidth + spanBox.PaddingLeft;
                    float spanY = cursorY + collapsedMargin + spanBox.BorderTopWidth + spanBox.PaddingTop;
                    spanBox.ContentRect = new RectF(spanX, spanY, spanWidth, 0);

                    // Layout the spanner's children
                    if (spanEl.Children.Count > 0)
                    {
                        if (HasBlockChildren(spanEl))
                            BlockFormattingContext.Layout(spanBox, context);
                        else
                            InlineFormattingContext.Layout(spanBox, context);
                    }

                    float spanHeight = DimensionResolver.ResolveHeight(spanEl.Style, containerHeight, spanBox);
                    if (float.IsNaN(spanHeight))
                    {
                        spanHeight = CalculateContentHeight(spanBox);
                    }
                    spanBox.ContentRect = new RectF(spanX, spanY, spanWidth, spanHeight);
                    box.AddChild(spanBox);

                    cursorY = spanY + spanHeight + spanBox.PaddingBottom + spanBox.BorderBottomWidth;
                    prevMarginBottom = spanBox.MarginBottom;
                }
                else
                {
                    currentSegment.Add(child);
                }
            }

            // Layout any remaining segment
            if (currentSegment.Count > 0)
            {
                cursorY += prevMarginBottom;
                float remainingHeight = float.NaN;
                if (!float.IsNaN(containerHeight) && containerHeight > 0)
                {
                    remainingHeight = containerHeight - (cursorY - box.ContentRect.Y);
                    if (remainingHeight < 0) { remainingHeight = 0; }
                }

                float trailingMargin;
                cursorY = LayoutSegmentAsColumns(box, currentSegment, context,
                    columnCount, columnWidth, columnGap, startX, cursorY, out trailingMargin,
                    isNestedInMulticol, remainingHeight);
            }

            box.ContentRect = new RectF(
                box.ContentRect.X, box.ContentRect.Y,
                box.ContentRect.Width, cursorY - box.ContentRect.Y);
        }

        /// <summary>
        /// Lays out a segment of children in multi-column format, returning the Y position after layout.
        /// When <paramref name="isNestedInMulticol"/> is true the segment is inside an outer
        /// fragmentation root and is allowed to use last-resort atomic balancing
        /// (CSS-BREAK-3 §5.4) for content-empty single-block segments so neither inner
        /// column is left empty at the seam between outer fragments.
        /// </summary>
        private static float LayoutSegmentAsColumns(LayoutBox parent, List<StyledNode> children,
            LayoutContext context, int columnCount, float columnWidth, float columnGap,
            float startX, float startY, out float trailingMargin, bool isNestedInMulticol,
            float maxColumnHeight = float.NaN)
        {
            trailingMargin = 0;
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
            if (totalHeight <= 0)
            {
                return startY;
            }

            // [CSS-MULTICOL §3.3] Balance columns so each is as short as possible.
            // BinarySearchColumnHeight also respects atomic blocks — for a segment
            // with a single unbreakable item, the target equals its height rather
            // than `totalHeight / columnCount`, which would leave content overflowing.
            float targetHeight = BinarySearchColumnHeight(tempBox, columnCount, totalHeight);
            if (targetHeight < 1)
            {
                targetHeight = totalHeight;
            }

            // [CSS-MULTICOL §7.1] In a fixed-height multicol with spanners, constrain
            // the column height to the remaining available space. Overflow content
            // spills into additional (virtual) columns.
            if (!float.IsNaN(maxColumnHeight) && maxColumnHeight >= 0 && targetHeight > maxColumnHeight)
            {
                targetHeight = Math.Max(1, maxColumnHeight);
            }

            // Sequential content-aware fragmentation with line-level splitting
            var segColChildren = new List<LayoutBox>[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                segColChildren[i] = new List<LayoutBox>();
            }

            // [CSS-BREAK-3 §5.4] Last-resort atomic balance: when this segment is
            // a single content-empty non-monolithic block and the inner multicol
            // is being fragmented by an outer multicol, balance the block evenly
            // across our columns by atomic-splitting it. Without this, the block
            // sits in column 0 and column 1+ stays empty — the outer column then
            // overflows because the inner multicol's effective capacity is half
            // of what it should be at the seam. Chrome reaches the same end
            // state by treating content-empty atomic blocks as having last-resort
            // breaks per CSS-BREAK-3 §5.4.
            bool atomicBalanced = false;
            if (isNestedInMulticol
                && columnCount >= 2
                && tempBox.Children.Count == 1
                && (tempBox.LineBoxes == null || tempBox.LineBoxes.Count == 0)
                && IsContentEmptyAtomicNonMonolithic(tempBox.Children[0]))
            {
                var sole = tempBox.Children[0];
                float blockTop = sole.BorderRect.Y;
                float blockHeight = sole.BorderRect.Height;
                float perColHeight = blockHeight / columnCount;

                if (perColHeight > 0 && perColHeight < blockHeight)
                {
                    var current = sole;
                    bool sliceFailed = false;
                    for (int slice = 0; slice < columnCount - 1; slice++)
                    {
                        float sliceY = blockTop + perColHeight * (slice + 1);
                        var atomicSplit = SplitAtomicBoxAtY(current, sliceY);
                        if (!atomicSplit.IsValid)
                        {
                            sliceFailed = true;
                            break;
                        }
                        segColChildren[slice].Add(atomicSplit.First!);
                        current = atomicSplit.Second!;
                    }
                    if (sliceFailed)
                    {
                        for (int c = 0; c < columnCount; c++)
                        {
                            segColChildren[c].Clear();
                        }
                    }
                    else
                    {
                        segColChildren[columnCount - 1].Add(current);
                        targetHeight = perColHeight;
                        atomicBalanced = true;
                    }
                }
            }

            if (!atomicBalanced)
            {
                // Position-based column filling using BFC-computed positions
                float segContentOriginY = tempBox.ContentRect.Y;
                int curCol = 0;
                float segColStartY = segContentOriginY;
                bool segColHasContent = false;
                foreach (var child in tempBox.Children)
                {
                    float childBottomY = child.BorderRect.Bottom;
                    float heightInCol = childBottomY - segColStartY;

                    if (segColHasContent && heightInCol > targetHeight && curCol < columnCount - 1)
                    {
                        // [CSS-BREAK-3 §5.1] Try line-level splitting
                        if (child.LineBoxes != null && child.LineBoxes.Count > 1)
                        {
                            float contentStartY = child.ContentRect.Y;
                            float availableForLines = segColStartY + targetHeight - contentStartY;
                            if (availableForLines > 0)
                            {
                                int splitAfter = -1;
                                for (int li = 0; li < child.LineBoxes.Count; li++)
                                {
                                    float lineBottom = child.LineBoxes[li].Y + child.LineBoxes[li].Height - contentStartY;
                                    if (lineBottom <= availableForLines)
                                    {
                                        splitAfter = li;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                if (splitAfter >= 0 && splitAfter < child.LineBoxes.Count - 1)
                                {
                                    var split = SplitBoxAtLine(child, splitAfter);
                                    if (split.IsValid)
                                    {
                                        segColChildren[curCol].Add(split.First!);
                                        curCol++;
                                        segColStartY = child.LineBoxes[splitAfter + 1].Y;
                                        segColHasContent = true;
                                        segColChildren[curCol].Add(split.Second!);
                                        continue;
                                    }
                                }
                            }
                        }

                        curCol++;
                        segColStartY = child.BorderRect.Y;
                        segColHasContent = false;
                    }
                    // [CSS-BREAK-3 §5.4] Non-monolithic block that overflows
                    // the column as the first (and only) child — split it
                    // atomically so the binary-search balanced height works.
                    // Only for blocks that can't be line-split (fixed-height
                    // blocks with 0-1 line boxes). Blocks with multiple lines
                    // are left unsplit in the first column (matching Chrome).
                    else if (!segColHasContent && heightInCol > targetHeight
                        && curCol < columnCount - 1 && !IsMonolithic(child)
                        && (child.LineBoxes == null || child.LineBoxes.Count <= 1))
                    {
                        float splitY = segColStartY + targetHeight;
                        BoxSplit blockSplit = SplitAtomicBoxAtY(child, splitY);
                        if (blockSplit.IsValid)
                        {
                            segColChildren[curCol].Add(blockSplit.First!);
                            curCol++;
                            segColStartY = splitY;
                            segColHasContent = true;
                            segColChildren[curCol].Add(blockSplit.Second!);
                            continue;
                        }
                    }
                    segColChildren[curCol].Add(child);
                    segColHasContent = true;
                }
            }

            float tallest = 0;
            float tallestColTrailingMargin = 0;
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
                float lastChildMarginBottom = 0;
                foreach (var child in segColChildren[col])
                {
                    var offsetChild = OffsetBox(child, xOffset, yOffset);
                    colBox.AddChild(offsetChild);
                    // Compute bottom WITHOUT margin-bottom (visual bottom only)
                    float childVisualBottom = offsetChild.ContentRect.Y + offsetChild.ContentRect.Height
                                            + offsetChild.PaddingBottom + offsetChild.BorderBottomWidth;
                    float childFullBottom = childVisualBottom + offsetChild.MarginBottom;
                    if (childFullBottom > colBottom)
                    {
                        colBottom = childFullBottom;
                        lastChildMarginBottom = offsetChild.MarginBottom;
                    }
                }

                float colH = colBottom - startY;
                if (colH > tallest)
                {
                    tallest = colH;
                    tallestColTrailingMargin = lastChildMarginBottom;
                }

                parent.AddChild(colBox);
            }

            // Exclude trailing margin from height — it will be collapsed with the next element
            trailingMargin = tallestColTrailingMargin;
            float segmentHeight = tallest - tallestColTrailingMargin;
            return startY + (segmentHeight > 0 ? segmentHeight : targetHeight);
        }

        /// <summary>
        /// Binary search for the minimum column height that allows all content
        /// (block children and/or line boxes) to fit within the given number of columns.
        /// Supports line-level fragmentation: blocks with multiple line boxes can be
        /// split at line boundaries for better column balance (CSS Fragmentation Level 3).
        /// Break groups (<see cref="BuildBreakGroups"/>) are treated as atomic units so
        /// <c>break-inside: avoid</c> and <c>break-*: avoid</c> siblings contribute their
        /// full combined height to the minimum column height.
        /// </summary>
        private static float BinarySearchColumnHeight(LayoutBox columnBox, int columnCount, float contentHeight)
        {
            if (contentHeight <= 0 || columnCount <= 1)
            {
                return contentHeight;
            }

            var groups = BuildBreakGroups(columnBox.Children);

            float minHeight = ComputeMinColumnHeight(groups, columnBox.LineBoxes);

            if (minHeight <= 0)
            {
                return contentHeight;
            }

            float lo = minHeight;
            float hi = contentHeight;

            // Binary search: find minimum height where content fits with line-level splitting
            for (int iter = 0; iter < 40 && hi - lo > 0.01f; iter++)
            {
                float mid = (lo + hi) * 0.5f;
                if (FitsInColumnsWithSplitting(columnBox, columnCount, mid, groups))
                {
                    hi = mid;
                }
                else
                {
                    lo = mid;
                }
            }

            // Add minimal epsilon to avoid edge-case rounding issues
            return hi + 0.01f;
        }

        /// <summary>
        /// Computes the minimum column height required to accommodate the
        /// tallest non-splittable unit among the break groups and any direct
        /// line boxes. Single-child groups whose child is splittable (has
        /// multiple line boxes and no <c>break-inside: avoid</c>) contribute
        /// only their tallest line plus top overhead; all other groups
        /// contribute their full combined height.
        /// </summary>
        private static float ComputeMinColumnHeight(List<BreakGroup> groups, List<LineBox>? directLineBoxes)
        {
            float minHeight = 0;
            foreach (var group in groups)
            {
                bool lineSplittable = group.Children.Count == 1
                    && !HasBreakInsideAvoid(group.Children[0])
                    && group.Children[0].LineBoxes != null
                    && group.Children[0].LineBoxes!.Count > 1;

                if (lineSplittable)
                {
                    // [CSS-BREAK-3 §5.1] Block with multiple lines: minimum is
                    // the tallest single line plus top box-model overhead.
                    var child = group.Children[0];
                    float topOverhead = child.PaddingTop + child.BorderTopWidth + child.MarginTop;
                    foreach (var line in child.LineBoxes!)
                    {
                        float lineHeight = line.Height + topOverhead;
                        if (lineHeight > minHeight)
                        {
                            minHeight = lineHeight;
                        }
                        topOverhead = 0;
                    }
                }
                else if (group.Children.Count == 1 && !IsMonolithic(group.Children[0]))
                {
                    // [CSS-BREAK-3 §5.4] Non-monolithic block that can be
                    // fragmented at the column boundary. Minimum is just the
                    // box-model overhead — the content itself can be split.
                    var child = group.Children[0];
                    float overhead = child.PaddingTop + child.BorderTopWidth + child.MarginTop
                                   + child.PaddingBottom + child.BorderBottomWidth + child.MarginBottom;
                    if (overhead < 1)
                    {
                        overhead = 1;
                    }
                    if (overhead > minHeight)
                    {
                        minHeight = overhead;
                    }
                }
                else
                {
                    float groupHeight = MeasureGroupMinHeight(group);
                    if (groupHeight > minHeight)
                    {
                        minHeight = groupHeight;
                    }
                }
            }

            if (directLineBoxes != null)
            {
                foreach (var line in directLineBoxes)
                {
                    if (line.Height > minHeight)
                    {
                        minHeight = line.Height;
                    }
                }
            }

            return minHeight;
        }

        /// <summary>
        /// Returns the height a break group would consume if placed in a fresh
        /// column, measured as the extent from the first child's border-box top
        /// to the last child's border-box bottom (plus the outer margins that
        /// would be present as unsplittable top/bottom overhead).
        /// </summary>
        private static float MeasureGroupMinHeight(BreakGroup group)
        {
            var first = group.Children[0];
            var last = group.Children[group.Children.Count - 1];
            float top = first.BorderRect.Y;
            float bottom = last.BorderRect.Bottom;
            return (bottom - top) + first.MarginTop + last.MarginBottom;
        }

        /// <summary>
        /// Check if content fits within the given number of columns at the specified height,
        /// using BFC-computed positions (which already account for margin collapsing) to
        /// determine column breaks. Honors break groups (break-avoid linkage) and allows
        /// splittable single-child groups to split at line boundaries.
        /// </summary>
        private static bool FitsInColumnsWithSplitting(LayoutBox columnBox, int columnCount,
            float columnHeight, List<BreakGroup> groups)
        {
            float contentOriginY = columnBox.ContentRect.Y;
            int col = 0;
            float colStartY = contentOriginY;
            bool colHasContent = false;

            foreach (var group in groups)
            {
                var firstChild = group.Children[0];
                var lastChild = group.Children[group.Children.Count - 1];

                float groupBottomY = lastChild.BorderRect.Bottom;
                float heightInCol = groupBottomY - colStartY;

                if (colHasContent && heightInCol > columnHeight)
                {
                    if (group.Children.Count == 1
                        && !HasBreakInsideAvoid(firstChild)
                        && firstChild.LineBoxes != null
                        && firstChild.LineBoxes.Count > 1)
                    {
                        // [CSS-BREAK-3 §5.1] Try line-level splitting
                        float contentStartY = firstChild.ContentRect.Y;
                        float availableForLines = colStartY + columnHeight - contentStartY;
                        if (availableForLines > 0)
                        {
                            int splitAfter = -1;
                            for (int i = 0; i < firstChild.LineBoxes.Count; i++)
                            {
                                float lineBottom = firstChild.LineBoxes[i].Y + firstChild.LineBoxes[i].Height - contentStartY;
                                if (lineBottom <= availableForLines)
                                {
                                    splitAfter = i;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (splitAfter >= 0 && splitAfter < firstChild.LineBoxes.Count - 1)
                            {
                                col++;
                                if (col >= columnCount)
                                {
                                    return false;
                                }
                                colStartY = firstChild.LineBoxes[splitAfter + 1].Y;
                                colHasContent = true;
                                continue;
                            }
                        }
                    }

                    col++;
                    if (col >= columnCount)
                    {
                        return false;
                    }
                    colStartY = firstChild.BorderRect.Y;
                    colHasContent = false;
                }
                // [CSS-BREAK-3 §5] Multi-child break groups (break-avoid
                // linked siblings) that exceed the column height cannot be
                // split — the column height is insufficient.
                else if (!colHasContent && heightInCol > columnHeight
                    && group.Children.Count > 1)
                {
                    return false;
                }
                // [CSS-BREAK-3 §5.4] Non-monolithic blocks that overflow
                // the column height and aren't line-splittable can be
                // fragmented at the column boundary.
                else if (!colHasContent && heightInCol > columnHeight
                    && group.Children.Count == 1
                    && !IsMonolithic(firstChild)
                    && (firstChild.LineBoxes == null || firstChild.LineBoxes.Count <= 1))
                {
                    float remaining = heightInCol;
                    float splitPoint = colStartY + columnHeight;
                    while (remaining > columnHeight)
                    {
                        col++;
                        if (col >= columnCount)
                        {
                            return false;
                        }
                        colStartY = splitPoint;
                        remaining -= columnHeight;
                        splitPoint += columnHeight;
                    }
                    colHasContent = true;
                    continue;
                }

                colHasContent = true;
            }

            // Handle direct line boxes (inline content)
            if (columnBox.LineBoxes != null)
            {
                foreach (var line in columnBox.LineBoxes)
                {
                    float lineBottom = line.Y + line.Height;
                    float heightInCol = lineBottom - colStartY;
                    if (colHasContent && heightInCol > columnHeight)
                    {
                        col++;
                        if (col >= columnCount)
                        {
                            return false;
                        }
                        colStartY = line.Y;
                        colHasContent = false;
                    }
                    colHasContent = true;
                }
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
                if (display == CssDisplay.Block || display == CssDisplay.FlowRoot ||
                    display == CssDisplay.Flex || display == CssDisplay.Grid ||
                    display == CssDisplay.Table || display == CssDisplay.ListItem)
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

        /// <summary>
        /// Splits a block box at the given line index so that lines 0..splitAfter go
        /// in the first part and lines splitAfter+1..end go in the second part.
        /// Returns (first, second) partial boxes.
        /// </summary>
        private static BoxSplit SplitBoxAtLine(LayoutBox box, int splitAfter)
        {
            if (IsMonolithic(box))
            {
                return BoxSplit.None;
            }

            if (box.LineBoxes == null || box.LineBoxes.Count < 2)
            {
                return BoxSplit.None;
            }

            if (splitAfter < 0 || splitAfter >= box.LineBoxes.Count - 1)
            {
                return BoxSplit.None;
            }

            float contentStartY = box.ContentRect.Y;

            // Create first part: lines 0..splitAfter
            var firstBox = new LayoutBox(box.StyledNode, box.BoxType);
            firstBox.PaddingTop = box.PaddingTop;
            firstBox.PaddingRight = box.PaddingRight;
            firstBox.PaddingBottom = 0; // No bottom padding on first fragment
            firstBox.PaddingLeft = box.PaddingLeft;
            firstBox.BorderTopWidth = box.BorderTopWidth;
            firstBox.BorderRightWidth = box.BorderRightWidth;
            firstBox.BorderBottomWidth = 0; // No bottom border on first fragment
            firstBox.BorderLeftWidth = box.BorderLeftWidth;
            firstBox.MarginTop = box.MarginTop;
            firstBox.MarginRight = box.MarginRight;
            firstBox.MarginBottom = 0;
            firstBox.MarginLeft = box.MarginLeft;

            var firstLines = new List<LineBox>();
            for (int i = 0; i <= splitAfter; i++)
            {
                firstLines.Add(box.LineBoxes[i]);
            }
            firstBox.LineBoxes = firstLines;

            float firstContentHeight = firstLines[firstLines.Count - 1].Y
                                     + firstLines[firstLines.Count - 1].Height - contentStartY;
            firstBox.ContentRect = new RectF(
                box.ContentRect.X, box.ContentRect.Y,
                box.ContentRect.Width, firstContentHeight);

            // Create second part: lines splitAfter+1..end
            var secondBox = new LayoutBox(box.StyledNode, box.BoxType);
            secondBox.PaddingTop = 0; // No top padding on continuation
            secondBox.PaddingRight = box.PaddingRight;
            secondBox.PaddingBottom = box.PaddingBottom;
            secondBox.PaddingLeft = box.PaddingLeft;
            secondBox.BorderTopWidth = 0;
            secondBox.BorderRightWidth = box.BorderRightWidth;
            secondBox.BorderBottomWidth = box.BorderBottomWidth;
            secondBox.BorderLeftWidth = box.BorderLeftWidth;
            secondBox.MarginTop = 0;
            secondBox.MarginRight = box.MarginRight;
            secondBox.MarginBottom = box.MarginBottom;
            secondBox.MarginLeft = box.MarginLeft;

            var secondLines = new List<LineBox>();
            float secondStartY = box.LineBoxes[splitAfter + 1].Y;
            for (int i = splitAfter + 1; i < box.LineBoxes.Count; i++)
            {
                secondLines.Add(box.LineBoxes[i]);
            }
            secondBox.LineBoxes = secondLines;

            float secondContentHeight = box.ContentRect.Y + box.ContentRect.Height - secondStartY;
            secondBox.ContentRect = new RectF(
                box.ContentRect.X, secondStartY,
                box.ContentRect.Width, secondContentHeight);

            return BoxSplit.Create(firstBox, secondBox);
        }

        /// <summary>
        /// [CSS-FRAGMENTATION §4] Splits an atomic block with no inner fragmentation
        /// points (no child boxes, no line boxes) at the given Y coordinate. The first
        /// fragment receives top-side padding/border/margin, the second receives the
        /// bottom side, and each carries its share of the content height so painting
        /// draws the background on both halves.
        /// </summary>
        private static BoxSplit SplitAtomicBoxAtY(LayoutBox box, float splitY)
        {
            if (IsMonolithic(box))
            {
                return BoxSplit.None;
            }

            float boxTop = box.ContentRect.Y;
            float boxBottom = box.ContentRect.Y + box.ContentRect.Height;
            if (splitY <= boxTop || splitY >= boxBottom)
            {
                return BoxSplit.None;
            }

            float firstHeight = splitY - boxTop;
            float secondHeight = boxBottom - splitY;
            if (firstHeight <= 0 || secondHeight <= 0)
            {
                return BoxSplit.None;
            }

            var firstBox = new LayoutBox(box.StyledNode, box.BoxType);
            firstBox.PaddingTop = box.PaddingTop;
            firstBox.PaddingRight = box.PaddingRight;
            firstBox.PaddingBottom = 0;
            firstBox.PaddingLeft = box.PaddingLeft;
            firstBox.BorderTopWidth = box.BorderTopWidth;
            firstBox.BorderRightWidth = box.BorderRightWidth;
            firstBox.BorderBottomWidth = 0;
            firstBox.BorderLeftWidth = box.BorderLeftWidth;
            firstBox.MarginTop = box.MarginTop;
            firstBox.MarginRight = box.MarginRight;
            firstBox.MarginBottom = 0;
            firstBox.MarginLeft = box.MarginLeft;
            firstBox.ContentRect = new RectF(
                box.ContentRect.X, boxTop,
                box.ContentRect.Width, firstHeight);

            var secondBox = new LayoutBox(box.StyledNode, box.BoxType);
            secondBox.PaddingTop = 0;
            secondBox.PaddingRight = box.PaddingRight;
            secondBox.PaddingBottom = box.PaddingBottom;
            secondBox.PaddingLeft = box.PaddingLeft;
            secondBox.BorderTopWidth = 0;
            secondBox.BorderRightWidth = box.BorderRightWidth;
            secondBox.BorderBottomWidth = box.BorderBottomWidth;
            secondBox.BorderLeftWidth = box.BorderLeftWidth;
            secondBox.MarginTop = 0;
            secondBox.MarginRight = box.MarginRight;
            secondBox.MarginBottom = box.MarginBottom;
            secondBox.MarginLeft = box.MarginLeft;
            secondBox.ContentRect = new RectF(
                box.ContentRect.X, splitY,
                box.ContentRect.Width, secondHeight);

            return BoxSplit.Create(firstBox, secondBox);
        }

        /// <summary>
        /// [CSS-BREAK-3 §5.1] Splits a block container at the given Y coordinate
        /// by partitioning its block children. Children whose border-box bottom
        /// sits at or above <paramref name="splitY"/> go to the first fragment;
        /// children whose border-box top sits at or below it go to the second;
        /// any child that straddles the line is recursively split using the
        /// appropriate helper (atomic, line-boxed, or block). Returns
        /// <see cref="BoxSplit.None"/> if no legal partition exists — the
        /// caller must fall back to a whole-child move.
        /// </summary>
        private static BoxSplit SplitBlockBoxAtY(LayoutBox box, float splitY)
        {
            if (IsMonolithic(box))
            {
                return BoxSplit.None;
            }
            if (box.LineBoxes != null && box.LineBoxes.Count > 0)
            {
                return BoxSplit.None;
            }
            if (box.Children == null || box.Children.Count == 0)
            {
                return BoxSplit.None;
            }

            float boxTop = box.ContentRect.Y;
            float boxBottom = box.ContentRect.Y + box.ContentRect.Height;
            if (splitY <= boxTop || splitY >= boxBottom)
            {
                return BoxSplit.None;
            }

            var firstChildren = new List<LayoutBox>();
            var secondChildren = new List<LayoutBox>();

            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                float childTop = child.BorderRect.Y;
                float childBottom = child.BorderRect.Bottom;

                if (childBottom <= splitY)
                {
                    firstChildren.Add(child);
                    continue;
                }
                if (childTop >= splitY)
                {
                    secondChildren.Add(child);
                    continue;
                }

                BoxSplit childSplit = BoxSplit.None;
                if (child.LineBoxes != null && child.LineBoxes.Count > 1)
                {
                    float contentStartY = child.ContentRect.Y;
                    int splitAfter = -1;
                    for (int li = 0; li < child.LineBoxes.Count; li++)
                    {
                        float lineBottom = child.LineBoxes[li].Y + child.LineBoxes[li].Height;
                        if (lineBottom <= splitY)
                        {
                            splitAfter = li;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (splitAfter >= 0 && splitAfter < child.LineBoxes.Count - 1)
                    {
                        childSplit = SplitBoxAtLine(child, splitAfter);
                    }
                }
                else if (child.Children != null && child.Children.Count > 0)
                {
                    childSplit = SplitBlockBoxAtY(child, splitY);
                }
                else
                {
                    childSplit = SplitAtomicBoxAtY(child, splitY);
                }

                if (!childSplit.IsValid)
                {
                    return BoxSplit.None;
                }

                firstChildren.Add(childSplit.First!);
                secondChildren.Add(childSplit.Second!);
            }

            if (firstChildren.Count == 0 || secondChildren.Count == 0)
            {
                return BoxSplit.None;
            }

            var firstBox = new LayoutBox(box.StyledNode, box.BoxType);
            firstBox.PaddingTop = box.PaddingTop;
            firstBox.PaddingRight = box.PaddingRight;
            firstBox.PaddingBottom = 0;
            firstBox.PaddingLeft = box.PaddingLeft;
            firstBox.BorderTopWidth = box.BorderTopWidth;
            firstBox.BorderRightWidth = box.BorderRightWidth;
            firstBox.BorderBottomWidth = 0;
            firstBox.BorderLeftWidth = box.BorderLeftWidth;
            firstBox.MarginTop = box.MarginTop;
            firstBox.MarginRight = box.MarginRight;
            firstBox.MarginBottom = 0;
            firstBox.MarginLeft = box.MarginLeft;
            firstBox.ContentRect = new RectF(
                box.ContentRect.X, boxTop,
                box.ContentRect.Width, splitY - boxTop);
            for (int i = 0; i < firstChildren.Count; i++)
            {
                firstBox.AddChild(firstChildren[i]);
            }

            var secondBox = new LayoutBox(box.StyledNode, box.BoxType);
            secondBox.PaddingTop = 0;
            secondBox.PaddingRight = box.PaddingRight;
            secondBox.PaddingBottom = box.PaddingBottom;
            secondBox.PaddingLeft = box.PaddingLeft;
            secondBox.BorderTopWidth = 0;
            secondBox.BorderRightWidth = box.BorderRightWidth;
            secondBox.BorderBottomWidth = box.BorderBottomWidth;
            secondBox.BorderLeftWidth = box.BorderLeftWidth;
            secondBox.MarginTop = 0;
            secondBox.MarginRight = box.MarginRight;
            secondBox.MarginBottom = box.MarginBottom;
            secondBox.MarginLeft = box.MarginLeft;
            secondBox.ContentRect = new RectF(
                box.ContentRect.X, splitY,
                box.ContentRect.Width, boxBottom - splitY);
            for (int i = 0; i < secondChildren.Count; i++)
            {
                secondBox.AddChild(secondChildren[i]);
            }

            return BoxSplit.Create(firstBox, secondBox);
        }

        /// <summary>
        /// [CSS-BREAK-3 §5] Walks the direct children of a fragmentation root
        /// and coalesces them into break groups. Adjacent siblings connected by
        /// <c>break-after: avoid</c>/<c>break-before: avoid</c> (or the legacy
        /// <c>page-break-*: avoid</c> aliases) join the same group so the
        /// column filling pass can treat them as a single unbreakable unit.
        /// Forced breaks (<c>break-*: column</c>/<c>always</c>) terminate the
        /// current group and start a new one; the top-level
        /// <see cref="BuildForcedBreakSegments"/> pass then drives column
        /// assignment off the forced-break boundaries.
        /// </summary>
        private static List<BreakGroup> BuildBreakGroups(IReadOnlyList<LayoutBox> children)
        {
            var groups = new List<BreakGroup>();
            if (children == null || children.Count == 0)
            {
                return groups;
            }

            var current = new BreakGroup();
            current.Children.Add(children[0]);

            for (int i = 1; i < children.Count; i++)
            {
                var previous = children[i - 1];
                var child = children[i];
                bool forced = HasBreakAfterForced(previous) || HasBreakBeforeForced(child);
                bool avoid = HasBreakAfterAvoid(previous) || HasBreakBeforeAvoid(child);

                if (!forced && avoid)
                {
                    current.Children.Add(child);
                    continue;
                }

                groups.Add(current);
                current = new BreakGroup();
                current.Children.Add(child);
            }

            groups.Add(current);

            return groups;
        }

        /// <summary>
        /// [CSS-BREAK-3 §5.1] Walks the direct children of a fragmentation root
        /// and splits them on forced column breaks (<c>break-before/after: column</c>,
        /// <c>always</c>, <c>page</c>, <c>left</c>, <c>right</c>, plus the legacy
        /// <c>page-break-*</c> aliases). A forced break between two siblings closes
        /// the current segment and starts a new one; a forced break on the very
        /// first child is a no-op because there is no earlier content to break
        /// away from. The resulting list never contains empty segments.
        /// </summary>
        private static List<List<LayoutBox>> BuildForcedBreakSegments(IReadOnlyList<LayoutBox> children)
        {
            var segments = new List<List<LayoutBox>>();
            if (children == null || children.Count == 0)
            {
                return segments;
            }

            var current = new List<LayoutBox>();
            current.Add(children[0]);

            for (int i = 1; i < children.Count; i++)
            {
                var previous = children[i - 1];
                var child = children[i];
                bool forced = HasBreakAfterForced(previous) || HasBreakBeforeForced(child);

                if (forced)
                {
                    segments.Add(current);
                    current = new List<LayoutBox>();
                }
                current.Add(child);
            }

            segments.Add(current);
            return segments;
        }

        /// <summary>
        /// [CSS-BREAK-3 §5.1] When the multicol has a single in-flow wrapper
        /// child whose grandchildren carry forced column breaks, split the
        /// wrapper into one fragment per forced-break segment. Each fragment
        /// reuses the wrapper's styled node so its background/border paints in
        /// every column it lands in. Returns null when no wrapper
        /// fragmentation applies (either because the multicol has direct
        /// forced breaks, has multiple direct children, the single child is
        /// not fragmentable, or the grandchildren have no forced breaks).
        /// Fragmentation is only performed for wrappers with no borders, no
        /// padding, and no specified block size — non-trivial wrappers need
        /// border joining across fragments which is not yet implemented.
        /// </summary>
        private static List<LayoutBox>? TryFragmentWrapperOnForcedBreaks(LayoutBox columnBox)
        {
            if (columnBox.Children == null || columnBox.Children.Count != 1)
            {
                return null;
            }
            var wrapper = columnBox.Children[0];
            if (wrapper.Children == null || wrapper.Children.Count < 2)
            {
                return null;
            }

            if (wrapper.BorderTopWidth > 0 || wrapper.BorderBottomWidth > 0
                || wrapper.BorderLeftWidth > 0 || wrapper.BorderRightWidth > 0
                || wrapper.PaddingTop > 0 || wrapper.PaddingBottom > 0)
            {
                return null;
            }
            var wrapperStyle = wrapper.StyledNode?.Style;
            if (wrapperStyle != null && !float.IsNaN(wrapperStyle.Height))
            {
                return null;
            }

            bool hasForcedBreak = false;
            for (int i = 1; i < wrapper.Children.Count; i++)
            {
                if (HasBreakBeforeForced(wrapper.Children[i])
                    || HasBreakAfterForced(wrapper.Children[i - 1]))
                {
                    hasForcedBreak = true;
                    break;
                }
            }
            if (!hasForcedBreak)
            {
                return null;
            }

            var innerSegments = BuildForcedBreakSegments(wrapper.Children);
            if (innerSegments.Count < 2)
            {
                return null;
            }

            var fragments = new List<LayoutBox>();
            for (int segIdx = 0; segIdx < innerSegments.Count; segIdx++)
            {
                var seg = innerSegments[segIdx];
                var fragment = new LayoutBox(wrapper.StyledNode, wrapper.BoxType);
                fragment.PaddingTop = 0;
                fragment.PaddingRight = wrapper.PaddingRight;
                fragment.PaddingBottom = 0;
                fragment.PaddingLeft = wrapper.PaddingLeft;
                fragment.BorderTopWidth = 0;
                fragment.BorderRightWidth = wrapper.BorderRightWidth;
                fragment.BorderBottomWidth = 0;
                fragment.BorderLeftWidth = wrapper.BorderLeftWidth;
                fragment.MarginTop = segIdx == 0 ? wrapper.MarginTop : 0;
                fragment.MarginRight = wrapper.MarginRight;
                fragment.MarginBottom = segIdx == innerSegments.Count - 1 ? wrapper.MarginBottom : 0;
                fragment.MarginLeft = wrapper.MarginLeft;

                var segFirst = seg[0];
                var segLast = seg[seg.Count - 1];
                float segTop = segFirst.BorderRect.Y;
                float segBottom = segLast.BorderRect.Bottom;
                fragment.ContentRect = new RectF(
                    wrapper.ContentRect.X, segTop,
                    wrapper.ContentRect.Width, segBottom - segTop);

                for (int i = 0; i < seg.Count; i++)
                {
                    fragment.AddChild(seg[i]);
                }

                fragments.Add(fragment);
            }

            return fragments;
        }

        /// <summary>
        /// [CSS-MULTICOL §3.3] Computes the tallest segment height for the
        /// segments that land in real columns (segment index &lt; <paramref name="columnCount"/>).
        /// Chrome excludes virtual-column segments from the balanced height
        /// calculation — see chromium issue 385595003. Each segment's height is
        /// measured as the distance from the first child's border-box top to the
        /// last child's border-box bottom.
        /// </summary>
        private static float MeasureRealSegmentMaxHeight(List<List<LayoutBox>> segments, int columnCount)
        {
            int realCount = Math.Min(segments.Count, columnCount);
            float maxHeight = 0;
            for (int i = 0; i < realCount; i++)
            {
                var segment = segments[i];
                if (segment.Count == 0)
                {
                    continue;
                }
                var first = segment[0];
                var last = segment[segment.Count - 1];
                float height = last.BorderRect.Bottom - first.BorderRect.Y;
                if (height > maxHeight)
                {
                    maxHeight = height;
                }
            }
            return maxHeight;
        }

        /// <summary>
        /// Returns true when the box's style requests that fragmentation inside
        /// the box be avoided for column or generic fragmentation contexts.
        /// Honors both the modern <c>break-inside</c> property and the legacy
        /// <c>page-break-inside</c> alias.
        /// </summary>
        private static bool HasBreakInsideAvoid(LayoutBox box)
        {
            var style = box.StyledNode?.Style;
            if (style == null)
            {
                return false;
            }
            var value = style.BreakInside;
            if (value == CssBreakValue.Avoid
                || value == CssBreakValue.AvoidColumn
                || value == CssBreakValue.AvoidPage)
            {
                return true;
            }
            return style.PageBreakInside == CssPageBreak.Avoid;
        }

        /// <summary>
        /// [CSS-BREAK-3 §3.2, §5] Returns true when a box is a monolithic
        /// fragmentation unit that must not be split across columns. Scrolling
        /// containers (any non-visible overflow), atomic inline-level boxes
        /// (<c>inline-block</c>, <c>inline-flex</c>, <c>inline-grid</c>),
        /// containment roots (<c>contain: size|strict|content</c>), and boxes
        /// requesting <c>break-inside: avoid</c> all qualify. Split helpers
        /// refuse to operate on monolithic boxes so the caller falls back to
        /// placing the box whole in the next column.
        /// <spec>CSS-BREAK-3 §3.2 https://drafts.csswg.org/css-break-3/#possible-breaks</spec>
        /// </summary>
        private static bool IsMonolithic(LayoutBox box)
        {
            var style = box.StyledNode?.Style;
            if (style == null)
            {
                return false;
            }

            if (style.OverflowX != CssOverflow.Visible || style.OverflowY != CssOverflow.Visible)
            {
                return true;
            }

            if (HasBreakInsideAvoid(box))
            {
                return true;
            }

            var display = style.Display;
            if (display == CssDisplay.InlineBlock
                || display == CssDisplay.InlineFlex
                || display == CssDisplay.InlineGrid)
            {
                return true;
            }

            var contain = style.Contain;
            if (contain == CssContain.Size
                || contain == CssContain.Strict
                || contain == CssContain.Content)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// [CSS-BREAK-3 §5.4] A "content-empty atomic" block — one that has no
        /// child boxes and no line content of its own — qualifies for last-resort
        /// atomic-balance treatment when it sits alone in an inner-multicol
        /// segment that is being fragmented by an outer multicol. Such a block
        /// renders only its own background/borders, so the visual result of
        /// slicing it across columns is identical to letting it fill those
        /// columns naturally; this lets us recover the otherwise wasted
        /// inner-column space at the seam between outer columns. Monolithic
        /// boxes are excluded because they refuse to be split.
        /// <spec>CSS-BREAK-3 §5.4 https://drafts.csswg.org/css-break-3/#unforced-breaks</spec>
        /// </summary>
        private static bool IsContentEmptyAtomicNonMonolithic(LayoutBox box)
        {
            if (IsMonolithic(box))
            {
                return false;
            }
            if (box.Children != null && box.Children.Count > 0)
            {
                return false;
            }
            if (box.LineBoxes != null && box.LineBoxes.Count > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true when the box has a forced column break before it
        /// (<c>break-before: column</c>/<c>always</c>/<c>page</c>/left/right or
        /// the legacy <c>page-break-before: always</c>/left/right). Per
        /// CSS-BREAK-3 §4.1.2, forced break values on the first in-flow child
        /// propagate to the parent, so a break-before on a descendant that sits
        /// at the start of its enclosing box becomes a break-before on that box.
        /// <spec>CSS-BREAK-3 §4.1.2 https://drafts.csswg.org/css-break-3/#propagation-of-forced-breaks</spec>
        /// </summary>
        private static bool HasBreakBeforeForced(LayoutBox box)
        {
            if (HasOwnBreakBeforeForced(box))
            {
                return true;
            }
            if (box.Children != null && box.Children.Count > 0)
            {
                var firstChild = box.Children[0];
                if (HasBreakBeforeForced(firstChild))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasOwnBreakBeforeForced(LayoutBox box)
        {
            var style = box.StyledNode?.Style;
            if (style == null)
            {
                return false;
            }
            var value = style.BreakBefore;
            if (value == CssBreakValue.Column
                || value == CssBreakValue.Always
                || value == CssBreakValue.Page
                || value == CssBreakValue.Left
                || value == CssBreakValue.Right)
            {
                return true;
            }
            var legacy = style.PageBreakBefore;
            return legacy == CssPageBreak.Always
                || legacy == CssPageBreak.Left
                || legacy == CssPageBreak.Right;
        }

        /// <summary>
        /// Returns true when the box has a forced column break after it
        /// (<c>break-after: column</c>/<c>always</c>/<c>page</c>/left/right or
        /// the legacy <c>page-break-after: always</c>/left/right). Per
        /// CSS-BREAK-3 §4.1.2, forced break values on the last in-flow child
        /// propagate to the parent, so a break-after on a descendant that sits
        /// at the end of its enclosing box becomes a break-after on that box.
        /// <spec>CSS-BREAK-3 §4.1.2 https://drafts.csswg.org/css-break-3/#propagation-of-forced-breaks</spec>
        /// </summary>
        private static bool HasBreakAfterForced(LayoutBox box)
        {
            if (HasOwnBreakAfterForced(box))
            {
                return true;
            }
            if (box.Children != null && box.Children.Count > 0)
            {
                var lastChild = box.Children[box.Children.Count - 1];
                if (HasBreakAfterForced(lastChild))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasOwnBreakAfterForced(LayoutBox box)
        {
            var style = box.StyledNode?.Style;
            if (style == null)
            {
                return false;
            }
            var value = style.BreakAfter;
            if (value == CssBreakValue.Column
                || value == CssBreakValue.Always
                || value == CssBreakValue.Page
                || value == CssBreakValue.Left
                || value == CssBreakValue.Right)
            {
                return true;
            }
            var legacy = style.PageBreakAfter;
            return legacy == CssPageBreak.Always
                || legacy == CssPageBreak.Left
                || legacy == CssPageBreak.Right;
        }

        /// <summary>
        /// Returns true when the box requests that a break immediately before
        /// it be avoided (<c>break-before: avoid</c>/<c>avoid-column</c>/
        /// <c>avoid-page</c> or legacy <c>page-break-before: avoid</c>).
        /// </summary>
        private static bool HasBreakBeforeAvoid(LayoutBox box)
        {
            var style = box.StyledNode?.Style;
            if (style == null)
            {
                return false;
            }
            var value = style.BreakBefore;
            if (value == CssBreakValue.Avoid
                || value == CssBreakValue.AvoidColumn
                || value == CssBreakValue.AvoidPage)
            {
                return true;
            }
            return style.PageBreakBefore == CssPageBreak.Avoid;
        }

        /// <summary>
        /// Returns true when the box requests that a break immediately after
        /// it be avoided (<c>break-after: avoid</c>/<c>avoid-column</c>/
        /// <c>avoid-page</c> or legacy <c>page-break-after: avoid</c>).
        /// </summary>
        private static bool HasBreakAfterAvoid(LayoutBox box)
        {
            var style = box.StyledNode?.Style;
            if (style == null)
            {
                return false;
            }
            var value = style.BreakAfter;
            if (value == CssBreakValue.Avoid
                || value == CssBreakValue.AvoidColumn
                || value == CssBreakValue.AvoidPage)
            {
                return true;
            }
            return style.PageBreakAfter == CssPageBreak.Avoid;
        }

        /// <summary>
        /// [CSS-MULTICOL §7.2] Walks the multicol subtree looking for an element
        /// with column-span: all that can reach the multicol root without crossing
        /// an element that establishes a new formatting context. A wrapper with a
        /// visible box (border/padding/background/fixed dimensions) also stops the
        /// walk because fragmenting it is not yet implemented — see
        /// <see cref="IsTrivialWrapperBox"/>.
        /// </summary>
        private static bool HasHoistableSpanner(StyledElement element)
        {
            for (int i = 0; i < element.Children.Count; i++)
            {
                if (!(element.Children[i] is StyledElement childElement))
                {
                    continue;
                }
                if (childElement.Style.ColumnSpan == CssColumnSpan.All)
                {
                    return true;
                }
                if (EstablishesNewFormattingContext(childElement.Style))
                {
                    continue;
                }
                if (!IsTrivialWrapperBox(childElement.Style))
                {
                    continue;
                }
                if (HasHoistableSpanner(childElement))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Flattens the multicol root's children into a linear list where hoistable
        /// nested spanners are promoted to the top level. Wrappers that contain a
        /// spanner are split into pre- and post-spanner clones so the surrounding
        /// content flows correctly inside columns.
        /// </summary>
        private static List<StyledNode> BuildHoistedSegmentList(StyledElement multicolRoot)
        {
            var output = new List<StyledNode>();
            FlattenChildrenForHoisting(multicolRoot, output);
            return output;
        }

        private static void FlattenChildrenForHoisting(StyledElement parent, List<StyledNode> output)
        {
            for (int i = 0; i < parent.Children.Count; i++)
            {
                var child = parent.Children[i];
                if (child is StyledElement childElement)
                {
                    if (childElement.Style.ColumnSpan == CssColumnSpan.All)
                    {
                        output.Add(child);
                        continue;
                    }
                    if (!EstablishesNewFormattingContext(childElement.Style)
                        && IsTrivialWrapperBox(childElement.Style)
                        && HasHoistableSpanner(childElement))
                    {
                        FragmentWrapperAroundSpanners(childElement, output);
                        continue;
                    }
                }
                output.Add(child);
            }
        }

        /// <summary>
        /// [CSS-MULTICOL §7.2] Fragmentation of a wrapper around a spanner requires
        /// splitting the wrapper's borders and backgrounds across the fragments. We
        /// only support this when the wrapper has no visible box (no border, padding,
        /// explicit dimensions, or painted background). Wrappers with visible styling
        /// are left intact and their spanners are not hoisted — this is incorrect per
        /// spec but avoids catastrophic regressions in cases we cannot render yet.
        /// </summary>
        private static bool IsTrivialWrapperBox(ComputedStyle style)
        {
            if (!float.IsNaN(style.Width) || !float.IsNaN(style.Height))
            {
                return false;
            }
            if (!float.IsNaN(style.MinHeight) && style.MinHeight > 0)
            {
                return false;
            }
            if (!float.IsNaN(style.MaxHeight))
            {
                return false;
            }
            if (style.BorderTopWidth > 0 || style.BorderBottomWidth > 0
                || style.BorderLeftWidth > 0 || style.BorderRightWidth > 0)
            {
                return false;
            }
            if (style.PaddingTop > 0 || style.PaddingBottom > 0
                || style.PaddingLeft > 0 || style.PaddingRight > 0)
            {
                return false;
            }
            if (style.BackgroundColor.A > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Fragments a wrapper element around any hoistable spanners in its subtree,
        /// emitting pre-spanner clones (with the wrapper's style), then the spanner
        /// itself, then any remaining content as a post-spanner clone. Multiple
        /// spanners in the same wrapper produce multiple fragments.
        /// </summary>
        private static void FragmentWrapperAroundSpanners(StyledElement wrapper, List<StyledNode> output)
        {
            var pending = new List<StyledNode>();
            for (int i = 0; i < wrapper.Children.Count; i++)
            {
                var child = wrapper.Children[i];
                if (child is StyledElement childElement)
                {
                    if (childElement.Style.ColumnSpan == CssColumnSpan.All)
                    {
                        EmitWrapperFragment(wrapper, pending, output);
                        output.Add(childElement);
                        continue;
                    }
                    if (!EstablishesNewFormattingContext(childElement.Style)
                        && IsTrivialWrapperBox(childElement.Style)
                        && HasHoistableSpanner(childElement))
                    {
                        EmitWrapperFragment(wrapper, pending, output);
                        FragmentWrapperAroundSpanners(childElement, output);
                        continue;
                    }
                }
                pending.Add(child);
            }
            EmitWrapperFragment(wrapper, pending, output);
        }

        /// <summary>
        /// Emits a fragment of the wrapper containing the pending children as a new
        /// StyledElement sharing the wrapper's underlying HTML element and style.
        /// Whitespace-only pending content is dropped — it would render as an empty
        /// line otherwise, which does not match Chrome's hoisting behavior.
        /// </summary>
        private static void EmitWrapperFragment(StyledElement wrapper, List<StyledNode> pending,
            List<StyledNode> output)
        {
            if (pending.Count == 0)
            {
                return;
            }
            if (IsWhitespaceOnlyContent(pending))
            {
                pending.Clear();
                return;
            }
            var fragment = new StyledElement(wrapper.Element, wrapper.Style,
                new List<StyledNode>(pending));
            output.Add(fragment);
            pending.Clear();
        }

        private static bool IsWhitespaceOnlyContent(List<StyledNode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] is StyledText textNode)
                {
                    if (!string.IsNullOrWhiteSpace(textNode.Text))
                    {
                        return false;
                    }
                    continue;
                }
                return false;
            }
            return true;
        }

        private static bool IsActiveRawValue(object? rawValue)
        {
            if (rawValue == null)
            {
                return false;
            }
            if (rawValue is CssKeywordValue keyword && keyword.Keyword == "none")
            {
                return false;
            }
            if (rawValue is string text && text == "none")
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// [CSS-MULTICOL §7.2] Per-spec list of conditions that stop a spanner from
        /// being hoisted out of a wrapper: new formatting contexts, positioning
        /// containing blocks, CSS containment, transforms, filters, or a nested
        /// multicol ancestor (which becomes the spanner's new nearest multicol).
        /// </summary>
        private static bool EstablishesNewFormattingContext(ComputedStyle style)
        {
            var display = style.Display;
            if (display == CssDisplay.InlineBlock || display == CssDisplay.Table
                || display == CssDisplay.TableCell || display == CssDisplay.TableCaption
                || display == CssDisplay.Flex || display == CssDisplay.InlineFlex
                || display == CssDisplay.Grid || display == CssDisplay.InlineGrid
                || display == CssDisplay.FlowRoot)
            {
                return true;
            }

            var position = style.Position;
            if (position == CssPosition.Absolute || position == CssPosition.Fixed)
            {
                return true;
            }

            if (style.OverflowX != CssOverflow.Visible || style.OverflowY != CssOverflow.Visible)
            {
                return true;
            }

            if (IsActiveRawValue(style.GetRefValue(Css.Properties.Internal.PropertyId.Transform)))
            {
                return true;
            }

            if (IsActiveRawValue(style.GetRefValue(Css.Properties.Internal.PropertyId.Filter)))
            {
                return true;
            }

            var contain = style.Contain;
            if (contain == CssContain.Layout || contain == CssContain.Paint
                || contain == CssContain.Content || contain == CssContain.Strict)
            {
                return true;
            }

            float columnCount = style.ColumnCount;
            float columnWidth = style.ColumnWidth;
            if ((!float.IsNaN(columnCount) && columnCount > 1)
                || (!float.IsNaN(columnWidth) && columnWidth > 0))
            {
                return true;
            }

            return false;
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
                            StyleOverride = frag.StyleOverride,
                            JustifyWordSpacing = frag.JustifyWordSpacing
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
