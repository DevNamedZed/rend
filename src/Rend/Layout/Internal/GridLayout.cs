using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Style;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// CSS Grid layout: track sizing, explicit/auto placement, spanning, and item positioning.
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

            // Collect grid items with placement info
            var items = new List<GridItem>();
            var children = BlockFormattingContext.FlattenContents(styledElement);
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child.IsText) continue;

                if (child is StyledPseudoElement pseudo)
                {
                    var pseudoText = new StyledText(pseudo.Content, pseudo.Style);
                    var pseudoBox = new LayoutText(pseudoText);
                    float fontSize = pseudo.Style.FontSize;
                    float lineHeight = pseudo.Style.LineHeight;
                    if (float.IsNaN(lineHeight) || lineHeight <= 0)
                        lineHeight = fontSize * 1.2f;
                    float estimatedWidth = pseudo.Content.Length * fontSize * 0.6f;
                    pseudoBox.ContentRect = new RectF(0, 0, estimatedWidth, lineHeight);
                    items.Add(new GridItem { Box = pseudoBox });
                    continue;
                }

                var childEl = (StyledElement)child;
                if (childEl.Style.Display == CssDisplay.None) continue;

                // Absolutely/fixed positioned items are out of flow
                if (childEl.Style.Position == CssPosition.Absolute ||
                    childEl.Style.Position == CssPosition.Fixed)
                {
                    float containingWidth = parent.ContentRect.Width;
                    var posBox = new LayoutBox(childEl, BoxType.Block);
                    BoxModelCalculator.ApplyBoxModel(posBox, childEl.Style, containingWidth);
                    float posWidth = DimensionResolver.ResolveWidth(childEl.Style, containingWidth, posBox);
                    posBox.ContentRect = new RectF(parent.ContentRect.X, parent.ContentRect.Y, posWidth, 0);
                    BlockFormattingContext.LayoutChildren(posBox, context);
                    float posHeight = DimensionResolver.ResolveHeight(childEl.Style, float.NaN, posBox);
                    if (float.IsNaN(posHeight)) posHeight = 0;
                    posBox.ContentRect = new RectF(posBox.ContentRect.X, posBox.ContentRect.Y, posWidth, posHeight);
                    parent.AddChild(posBox);
                    continue;
                }

                var item = new GridItem
                {
                    StyledElement = childEl,
                    Box = new LayoutBox(childEl, BoxType.Block),
                    Order = childEl.Style.Order,
                    OriginalIndex = items.Count
                };
                ParsePlacement(childEl.Style, item);
                items.Add(item);
            }

            if (items.Count == 0) return;

            // Sort by CSS order (stable: use original index as tiebreaker)
            items.Sort((a, b) =>
            {
                int cmp = a.Order.CompareTo(b.Order);
                return cmp != 0 ? cmp : a.OriginalIndex.CompareTo(b.OriginalIndex);
            });

            // Resolve explicit tracks
            var explicitColTracks = ResolveTrackList(
                style.GetRefValue(PropertyId.GridTemplateColumns), containerWidth);
            var explicitRowTracks = ResolveTrackList(
                style.GetRefValue(PropertyId.GridTemplateRows), containerHeight);

            int explicitCols = explicitColTracks?.Length ?? 0;
            int explicitRows = explicitRowTracks?.Length ?? 0;

            // Read auto-flow direction
            var autoFlow = style.GridAutoFlow;
            bool flowColumn = autoFlow == CssGridAutoFlow.Column || autoFlow == CssGridAutoFlow.ColumnDense;
            bool dense = autoFlow == CssGridAutoFlow.RowDense || autoFlow == CssGridAutoFlow.ColumnDense;

            // Parse grid-template-areas if present
            Dictionary<string, (int rowStart, int colStart, int rowSpan, int colSpan)>? namedAreas = null;
            var areasRaw = style.GetRefValue(PropertyId.GridTemplateAreas);
            if (areasRaw != null)
                namedAreas = ParseGridTemplateAreas(areasRaw);

            // Resolve named-area placement for items using grid-area with a name
            if (namedAreas != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item.AreaName != null && namedAreas.TryGetValue(item.AreaName, out var area))
                    {
                        item.RowStart = area.rowStart;
                        item.ColStart = area.colStart;
                        item.RowSpan = area.rowSpan;
                        item.ColSpan = area.colSpan;
                    }
                }
            }

            // Determine grid dimensions by scanning explicit placements
            int gridCols = Math.Max(1, explicitCols);
            int gridRows = Math.Max(1, explicitRows);

            // Expand grid from named areas
            if (namedAreas != null)
            {
                foreach (var area in namedAreas.Values)
                {
                    if (area.rowStart + area.rowSpan > gridRows)
                        gridRows = area.rowStart + area.rowSpan;
                    if (area.colStart + area.colSpan > gridCols)
                        gridCols = area.colStart + area.colSpan;
                }
            }

            // First pass: determine minimum grid size from explicit placements
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                int colEnd = item.ColStart >= 0 ? item.ColStart + item.ColSpan : 0;
                int rowEnd = item.RowStart >= 0 ? item.RowStart + item.RowSpan : 0;
                if (colEnd > gridCols) gridCols = colEnd;
                if (rowEnd > gridRows) gridRows = rowEnd;
            }

            // Resolve negative line numbers now that we know grid dimensions
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.RowStart < -1)
                {
                    item.RowStart = Math.Max(0, ResolveNegativeLine(item.RowStart, gridRows));
                }
                if (item.ColStart < -1)
                {
                    item.ColStart = Math.Max(0, ResolveNegativeLine(item.ColStart, gridCols));
                }
            }

            // If no explicit columns, determine from item count
            if (explicitCols == 0 && !HasAnyExplicitPlacement(items))
            {
                if (flowColumn)
                {
                    gridRows = Math.Max(1, explicitRows > 0 ? explicitRows : (int)Math.Ceiling(Math.Sqrt(items.Count)));
                    gridCols = (int)Math.Ceiling((float)items.Count / gridRows);
                }
                else
                {
                    gridCols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(items.Count)));
                    gridRows = (int)Math.Ceiling((float)items.Count / gridCols);
                }
            }

            // Place items on the grid using a placement matrix
            var occupied = new bool[gridRows * gridCols * 4]; // oversized to handle growth
            int maxRow = gridRows;
            int maxCol = gridCols;

            // Phase 1: Place items with definite row AND column
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.RowStart >= 0 && item.ColStart >= 0)
                {
                    EnsureGridSize(ref occupied, ref maxRow, ref maxCol,
                        item.RowStart + item.RowSpan, item.ColStart + item.ColSpan);
                    MarkOccupied(occupied, maxCol, item.RowStart, item.ColStart, item.RowSpan, item.ColSpan);
                    item.Placed = true;
                }
            }

            // Phase 2: Place items with definite row only
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.Placed) continue;
                if (item.RowStart >= 0)
                {
                    EnsureGridSize(ref occupied, ref maxRow, ref maxCol,
                        item.RowStart + item.RowSpan, maxCol);
                    int col = FindFreeColumn(occupied, maxCol, item.RowStart, item.ColSpan, 0);
                    if (col < 0)
                    {
                        col = maxCol;
                        EnsureGridSize(ref occupied, ref maxRow, ref maxCol,
                            maxRow, col + item.ColSpan);
                    }
                    item.ColStart = col;
                    MarkOccupied(occupied, maxCol, item.RowStart, item.ColStart, item.RowSpan, item.ColSpan);
                    item.Placed = true;
                }
            }

            // Phase 3: Place items with definite column only
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.Placed) continue;
                if (item.ColStart >= 0)
                {
                    EnsureGridSize(ref occupied, ref maxRow, ref maxCol,
                        maxRow, item.ColStart + item.ColSpan);
                    int row = FindFreeRow(occupied, maxCol, maxRow, item.ColStart, item.RowSpan, 0);
                    if (row < 0)
                    {
                        row = maxRow;
                        EnsureGridSize(ref occupied, ref maxRow, ref maxCol,
                            row + item.RowSpan, maxCol);
                    }
                    item.RowStart = row;
                    MarkOccupied(occupied, maxCol, item.RowStart, item.ColStart, item.RowSpan, item.ColSpan);
                    item.Placed = true;
                }
            }

            // Phase 4: Auto-place remaining items
            int autoRow = 0, autoCol = 0;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.Placed) continue;

                if (dense)
                {
                    autoRow = 0;
                    autoCol = 0;
                }

                bool found = false;
                if (flowColumn)
                {
                    // Column-major auto-placement
                    for (int c = autoCol; !found; c++)
                    {
                        int startRow = (c == autoCol) ? autoRow : 0;
                        for (int r = startRow; r <= maxRow; r++)
                        {
                            EnsureGridSize(ref occupied, ref maxRow, ref maxCol,
                                r + item.RowSpan, c + item.ColSpan);
                            if (IsFree(occupied, maxCol, r, c, item.RowSpan, item.ColSpan))
                            {
                                item.RowStart = r;
                                item.ColStart = c;
                                MarkOccupied(occupied, maxCol, r, c, item.RowSpan, item.ColSpan);
                                item.Placed = true;
                                autoRow = r;
                                autoCol = c;
                                found = true;
                                break;
                            }
                        }
                        if (c > maxCol + items.Count) break; // safety
                    }
                }
                else
                {
                    // Row-major auto-placement (default)
                    for (int r = autoRow; !found; r++)
                    {
                        int startCol = (r == autoRow) ? autoCol : 0;
                        for (int c = startCol; c <= maxCol; c++)
                        {
                            EnsureGridSize(ref occupied, ref maxRow, ref maxCol,
                                r + item.RowSpan, c + item.ColSpan);
                            if (IsFree(occupied, maxCol, r, c, item.RowSpan, item.ColSpan))
                            {
                                item.RowStart = r;
                                item.ColStart = c;
                                MarkOccupied(occupied, maxCol, r, c, item.RowSpan, item.ColSpan);
                                item.Placed = true;
                                autoRow = r;
                                autoCol = c;
                                found = true;
                                break;
                            }
                        }
                        if (r > maxRow + items.Count) break; // safety
                    }
                }

                if (!item.Placed)
                {
                    // Fallback: place at end
                    EnsureGridSize(ref occupied, ref maxRow, ref maxCol,
                        maxRow + item.RowSpan, maxCol);
                    item.RowStart = maxRow - item.RowSpan;
                    item.ColStart = 0;
                    item.Placed = true;
                }
            }

            // Build final column and row sizes
            int finalCols = maxCol;
            int finalRows = maxRow;

            float[] colWidths = BuildTrackSizes(explicitColTracks, finalCols, containerWidth,
                colGap, style.GetRefValue(PropertyId.GridAutoColumns), containerWidth);
            float[] rowHeights = new float[finalRows];

            // First pass: layout each item to determine content size and row heights
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                // Calculate cell width across spanned columns
                float cellWidth = 0;
                for (int c = item.ColStart; c < item.ColStart + item.ColSpan && c < finalCols; c++)
                    cellWidth += colWidths[c];
                if (item.ColSpan > 1)
                    cellWidth += (item.ColSpan - 1) * colGap;

                if (item.StyledElement == null)
                {
                    // Pseudo-element: already sized
                    item.ContentWidth = Math.Min(item.Box.ContentRect.Width, cellWidth);
                    item.ContentHeight = item.Box.ContentRect.Height;
                }
                else
                {
                    BoxModelCalculator.ApplyBoxModel(item.Box, item.StyledElement.Style, cellWidth);
                    float contentWidth;
                    if (SizingKeyword.IsSizingKeyword(item.StyledElement.Style.Width))
                    {
                        contentWidth = BlockFormattingContext.MeasureIntrinsicWidth(
                            item.StyledElement, item.StyledElement.Style.Width, cellWidth, context);
                    }
                    else
                    {
                        contentWidth = DimensionResolver.ResolveWidth(item.StyledElement.Style, cellWidth, item.Box);
                    }
                    if (float.IsNaN(contentWidth))
                    {
                        contentWidth = cellWidth - item.Box.PaddingLeft - item.Box.PaddingRight
                                       - item.Box.BorderLeftWidth - item.Box.BorderRightWidth
                                       - item.Box.MarginLeft - item.Box.MarginRight;
                    }
                    contentWidth = Math.Max(0, contentWidth);

                    item.Box.ContentRect = new RectF(0, 0, contentWidth, 0);
                    BlockFormattingContext.Layout(item.Box, context);

                    float contentHeight = DimensionResolver.ResolveHeight(item.StyledElement.Style, float.NaN, item.Box);
                    if (float.IsNaN(contentHeight))
                        contentHeight = CalculateAutoHeight(item.Box);

                    item.ContentWidth = contentWidth;
                    item.ContentHeight = contentHeight;

                    // visibility: collapse → zero contribution to row height
                    if (item.StyledElement.Style.Visibility == CssVisibility.Collapse)
                    {
                        item.ContentHeight = 0;
                        item.ContentWidth = 0;
                    }
                }

                // Distribute height across spanned rows
                float totalHeight = item.ContentHeight;
                if (item.StyledElement != null)
                {
                    totalHeight += item.Box.PaddingTop + item.Box.PaddingBottom
                                 + item.Box.BorderTopWidth + item.Box.BorderBottomWidth
                                 + item.Box.MarginTop + item.Box.MarginBottom;
                }

                if (item.RowSpan == 1)
                {
                    int r = item.RowStart;
                    if (r < finalRows && totalHeight > rowHeights[r])
                        rowHeights[r] = totalHeight;
                }
                else
                {
                    // For spanning items, distribute height evenly across rows
                    float perRow = totalHeight / item.RowSpan;
                    for (int r = item.RowStart; r < item.RowStart + item.RowSpan && r < finalRows; r++)
                    {
                        if (perRow > rowHeights[r])
                            rowHeights[r] = perRow;
                    }
                }
            }

            // Apply explicit row heights
            if (explicitRowTracks != null)
            {
                for (int r = 0; r < Math.Min(explicitRowTracks.Length, finalRows); r++)
                {
                    if (explicitRowTracks[r] > rowHeights[r])
                        rowHeights[r] = explicitRowTracks[r];
                }
            }

            // Apply grid-auto-rows to implicit rows (beyond explicit template)
            object? autoRowRaw = style.GetRefValue(PropertyId.GridAutoRows);
            if (autoRowRaw != null)
            {
                var autoRowTracks = ResolveTrackList(autoRowRaw, containerHeight);
                if (autoRowTracks != null && autoRowTracks.Length > 0)
                {
                    float autoRowSize = autoRowTracks[0];
                    int implicitStart = explicitRows;
                    for (int r = implicitStart; r < finalRows; r++)
                    {
                        if (autoRowSize > rowHeights[r])
                            rowHeights[r] = autoRowSize;
                    }
                }
            }

            // Read container-level alignment defaults
            CssAlignItems containerAlignItems = style.AlignItems;
            CssAlignItems containerJustifyItems = style.JustifyItems;

            // Second pass: position items
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                float x = parent.ContentRect.X;
                for (int c = 0; c < item.ColStart && c < finalCols; c++)
                    x += colWidths[c] + colGap;

                float y = parent.ContentRect.Y;
                for (int r = 0; r < item.RowStart && r < finalRows; r++)
                    y += rowHeights[r] + rowGap;

                // For spanning items, calculate the actual cell area
                float spanWidth = 0;
                for (int c = item.ColStart; c < item.ColStart + item.ColSpan && c < finalCols; c++)
                    spanWidth += colWidths[c];
                if (item.ColSpan > 1)
                    spanWidth += (item.ColSpan - 1) * colGap;

                float spanHeight = 0;
                for (int r = item.RowStart; r < item.RowStart + item.RowSpan && r < finalRows; r++)
                    spanHeight += rowHeights[r];
                if (item.RowSpan > 1)
                    spanHeight += (item.RowSpan - 1) * rowGap;

                float finalWidth = item.ContentWidth;
                float finalHeight = item.ContentHeight;

                // Calculate total item outer size (content + padding + border + margin)
                float outerWidth = finalWidth + item.Box.PaddingLeft + item.Box.PaddingRight
                    + item.Box.BorderLeftWidth + item.Box.BorderRightWidth
                    + item.Box.MarginLeft + item.Box.MarginRight;
                float outerHeight = finalHeight + item.Box.PaddingTop + item.Box.PaddingBottom
                    + item.Box.BorderTopWidth + item.Box.BorderBottomWidth
                    + item.Box.MarginTop + item.Box.MarginBottom;

                // Resolve alignment: item's self overrides container default
                CssAlignItems alignBlock = containerAlignItems;
                CssAlignItems alignInline = containerJustifyItems;
                if (item.StyledElement != null)
                {
                    var itemStyle = item.StyledElement.Style;
                    CssAlignItems selfBlock = itemStyle.AlignSelf;
                    CssAlignItems selfInline = itemStyle.JustifySelf;
                    if (selfBlock != CssAlignItems.Normal) alignBlock = selfBlock;
                    if (selfInline != CssAlignItems.Normal) alignInline = selfInline;
                }

                // Apply inline (horizontal) alignment offset
                float xOffset = AlignOffset(alignInline, spanWidth, outerWidth);

                // Apply block (vertical) alignment offset
                float yOffset = AlignOffset(alignBlock, spanHeight, outerHeight);

                // Stretch: expand content to fill cell (default grid behavior)
                if (IsStretch(alignInline) && outerWidth < spanWidth)
                    finalWidth = spanWidth - (outerWidth - finalWidth);
                if (IsStretch(alignBlock) && outerHeight < spanHeight)
                    finalHeight = spanHeight - (outerHeight - finalHeight);

                item.Box.ContentRect = new RectF(
                    x + xOffset + item.Box.MarginLeft + item.Box.BorderLeftWidth + item.Box.PaddingLeft,
                    y + yOffset + item.Box.MarginTop + item.Box.BorderTopWidth + item.Box.PaddingTop,
                    finalWidth, finalHeight);

                parent.AddChild(item.Box);
            }
        }

        private static float[] BuildTrackSizes(float[]? explicitTracks, int count, float containerSize,
            float gap, object? autoTrackRaw, float defaultSize)
        {
            var sizes = new float[count];

            // Apply explicit tracks
            if (explicitTracks != null)
            {
                for (int i = 0; i < Math.Min(explicitTracks.Length, count); i++)
                    sizes[i] = explicitTracks[i];
            }

            // Determine auto track size
            float autoSize = 0;
            if (autoTrackRaw != null)
            {
                var autoTracks = ResolveTrackList(autoTrackRaw, containerSize);
                if (autoTracks != null && autoTracks.Length > 0)
                    autoSize = autoTracks[0];
            }

            // Fill remaining (implicit) tracks
            int explicitCount = explicitTracks?.Length ?? 0;
            if (explicitCount < count)
            {
                if (autoSize > 0)
                {
                    for (int i = explicitCount; i < count; i++)
                        sizes[i] = autoSize;
                }
                else
                {
                    // Distribute remaining space equally among implicit tracks
                    float usedWidth = 0;
                    for (int i = 0; i < explicitCount; i++)
                        usedWidth += sizes[i];
                    float gapSpace = (count - 1) * gap;
                    float remaining = Math.Max(0, containerSize - usedWidth - gapSpace);
                    int implicitCount = count - explicitCount;
                    float implicitSize = remaining / implicitCount;
                    for (int i = explicitCount; i < count; i++)
                        sizes[i] = implicitSize;
                }
            }

            return sizes;
        }

        private static void ParsePlacement(ComputedStyle style, GridItem item)
        {
            // Check if grid-row-start is a plain identifier (named area from grid-area shorthand)
            var rowStartRaw = style.GetRefValue(PropertyId.GridRowStart);
            if (rowStartRaw is CssKeywordValue areaKw && areaKw.Keyword != "auto" && areaKw.Keyword != "span")
            {
                // This is a named area reference (e.g., grid-area: header)
                item.AreaName = areaKw.Keyword;
                return;
            }

            item.RowStart = ParseLineValue(rowStartRaw, out int rowSpan);
            item.RowSpan = rowSpan;

            int endRowSpan;
            int rowEnd = ParseLineValue(style.GetRefValue(PropertyId.GridRowEnd), out endRowSpan);
            if (rowEnd >= 0 && item.RowStart >= 0 && rowEnd > item.RowStart)
                item.RowSpan = rowEnd - item.RowStart;
            else if (endRowSpan > 1 && item.RowStart >= 0)
                item.RowSpan = endRowSpan;

            item.ColStart = ParseLineValue(style.GetRefValue(PropertyId.GridColumnStart), out int colSpan);
            item.ColSpan = colSpan;

            int endColSpan;
            int colEnd = ParseLineValue(style.GetRefValue(PropertyId.GridColumnEnd), out endColSpan);
            if (colEnd >= 0 && item.ColStart >= 0 && colEnd > item.ColStart)
                item.ColSpan = colEnd - item.ColStart;
            else if (endColSpan > 1 && item.ColStart >= 0)
                item.ColSpan = endColSpan;
        }

        /// <summary>
        /// Parse a grid line value (e.g., "2", "-1", "span 2", "auto").
        /// Returns -1 for auto, 0-based line number otherwise.
        /// Negative line numbers are stored as negative values (resolved later with grid size).
        /// Sets span to > 1 when "span N" is used.
        /// </summary>
        private static int ParseLineValue(object? raw, out int span)
        {
            span = 1;
            if (raw == null) return -1;

            if (raw is CssKeywordValue kw)
            {
                if (kw.Keyword == "auto") return -1;
                if (kw.Keyword == "span") return -1;
            }

            if (raw is CssNumberValue num)
            {
                int val = (int)num.Value;
                if (val > 0) return val - 1;
                if (val < 0) return val; // negative: resolve later
                return -1;
            }

            if (raw is CssDimensionValue dim)
            {
                int val = (int)dim.Value;
                if (val > 0) return val - 1;
                if (val < 0) return val;
                return -1;
            }

            if (raw is CssListValue list)
            {
                bool isSpan = false;
                int lineNum = -1;
                bool hasLineNum = false;

                for (int i = 0; i < list.Values.Count; i++)
                {
                    var v = list.Values[i];
                    if (v is CssKeywordValue spanKw && spanKw.Keyword == "span")
                    {
                        isSpan = true;
                    }
                    else if (v is CssNumberValue n)
                    {
                        lineNum = (int)n.Value;
                        hasLineNum = true;
                    }
                    else if (v is CssDimensionValue d)
                    {
                        lineNum = (int)d.Value;
                        hasLineNum = true;
                    }
                }

                if (isSpan && hasLineNum && lineNum > 0)
                {
                    span = lineNum;
                    return -1;
                }
                else if (hasLineNum && lineNum > 0)
                {
                    return lineNum - 1;
                }
                else if (hasLineNum && lineNum < 0)
                {
                    return lineNum;
                }
            }

            return -1;
        }

        /// <summary>
        /// Resolve negative line numbers to 0-based indices using the grid dimension.
        /// CSS: line -1 = last line = gridSize, line -2 = gridSize-1, etc.
        /// </summary>
        private static int ResolveNegativeLine(int line, int gridSize)
        {
            if (line >= 0) return line;
            // CSS: -1 = gridSize (after last track), -2 = gridSize-1, etc.
            return gridSize + line + 1;
        }

        private static bool HasAnyExplicitPlacement(List<GridItem> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].RowStart >= 0 || items[i].ColStart >= 0)
                    return true;
            }
            return false;
        }

        private static void EnsureGridSize(ref bool[] occupied, ref int maxRow, ref int maxCol,
            int needRows, int needCols)
        {
            int newMaxRow = Math.Max(maxRow, needRows);
            int newMaxCol = Math.Max(maxCol, needCols);
            if (newMaxRow == maxRow && newMaxCol == maxCol) return;

            int newSize = newMaxRow * newMaxCol;
            if (newSize > occupied.Length)
            {
                var newOcc = new bool[newSize * 2]; // double for headroom
                // Copy old data
                for (int r = 0; r < maxRow; r++)
                {
                    for (int c = 0; c < maxCol; c++)
                    {
                        if (occupied[r * maxCol + c])
                            newOcc[r * newMaxCol + c] = true;
                    }
                }
                occupied = newOcc;
            }
            else if (newMaxCol != maxCol)
            {
                // Re-layout existing data in-place if column count changed
                var newOcc = new bool[newSize * 2];
                for (int r = 0; r < maxRow; r++)
                {
                    for (int c = 0; c < maxCol; c++)
                    {
                        if (occupied[r * maxCol + c])
                            newOcc[r * newMaxCol + c] = true;
                    }
                }
                occupied = newOcc;
            }

            maxRow = newMaxRow;
            maxCol = newMaxCol;
        }

        private static bool IsFree(bool[] occupied, int cols, int row, int col, int rowSpan, int colSpan)
        {
            for (int r = row; r < row + rowSpan; r++)
            {
                for (int c = col; c < col + colSpan; c++)
                {
                    int idx = r * cols + c;
                    if (idx >= occupied.Length) return true; // beyond current grid = free
                    if (occupied[idx]) return false;
                }
            }
            return true;
        }

        private static void MarkOccupied(bool[] occupied, int cols, int row, int col, int rowSpan, int colSpan)
        {
            for (int r = row; r < row + rowSpan; r++)
            {
                for (int c = col; c < col + colSpan; c++)
                {
                    int idx = r * cols + c;
                    if (idx < occupied.Length)
                        occupied[idx] = true;
                }
            }
        }

        private static int FindFreeColumn(bool[] occupied, int cols, int row, int colSpan, int startCol)
        {
            for (int c = startCol; c + colSpan <= cols; c++)
            {
                if (IsFree(occupied, cols, row, c, 1, colSpan))
                    return c;
            }
            return -1;
        }

        private static int FindFreeRow(bool[] occupied, int cols, int rows, int col, int rowSpan, int startRow)
        {
            for (int r = startRow; r + rowSpan <= rows; r++)
            {
                if (IsFree(occupied, cols, r, col, rowSpan, 1))
                    return r;
            }
            return -1;
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

        /// <summary>
        /// Parses a grid-template-columns/rows raw CssValue into track sizes in px.
        /// Supports: px values, percentages, fr units, repeat(count, size).
        /// Returns null for "none" or missing values.
        /// </summary>
        internal static float[]? ResolveTrackList(object? raw, float containerSize)
        {
            if (raw == null) return null;
            if (raw is CssKeywordValue kw && (kw.Keyword == "none" || kw.Keyword == "auto"))
                return null;

            // Flatten into a list of individual track values (expanding repeat())
            var flatValues = new List<object>();
            if (raw is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count; i++)
                    FlattenTrackValue(list.Values[i], flatValues, containerSize);
            }
            else
            {
                FlattenTrackValue(raw, flatValues, containerSize);
            }

            if (flatValues.Count == 0) return null;

            // Two-pass: collect sizes, resolve fr units
            var sizes = new List<(float value, bool isFr)>();
            var minFloors = new float[flatValues.Count];
            float totalFixed = 0;
            float totalFr = 0;

            for (int i = 0; i < flatValues.Count; i++)
            {
                var parsed = ParseTrackValue(flatValues[i], containerSize);
                sizes.Add(parsed);
                minFloors[i] = GetMinmaxFloor(flatValues[i], containerSize);
                if (parsed.isFr)
                    totalFr += parsed.value;
                else
                    totalFixed += parsed.value;
            }

            float remaining = Math.Max(0, containerSize - totalFixed);
            float frSize = totalFr > 0 ? remaining / totalFr : 0;

            var tracks = new float[sizes.Count];
            for (int i = 0; i < sizes.Count; i++)
            {
                float resolved = sizes[i].isFr ? sizes[i].value * frSize : sizes[i].value;
                // Enforce minmax() minimum constraint
                if (minFloors[i] > 0 && resolved < minFloors[i])
                    resolved = minFloors[i];
                tracks[i] = resolved;
            }

            return tracks;
        }

        private static void FlattenTrackValue(object val, List<object> output, float containerSize)
        {
            if (val is CssFunctionValue fn && fn.Name == "repeat" && fn.Arguments.Count >= 2)
            {
                var first = fn.Arguments[0];
                bool isAutoFill = first is CssKeywordValue kw1 &&
                    (kw1.Keyword == "auto-fill" || kw1.Keyword == "auto-fit");

                int count;
                if (isAutoFill)
                {
                    // auto-fill/auto-fit: calculate count from container size and track min size
                    float trackMinSize = 0;
                    for (int j = 1; j < fn.Arguments.Count; j++)
                    {
                        var arg = fn.Arguments[j];
                        if (arg is CssFunctionValue minmaxFn && minmaxFn.Name == "minmax" && minmaxFn.Arguments.Count >= 2)
                        {
                            var minParsed = ParseTrackValue(minmaxFn.Arguments[0], containerSize);
                            trackMinSize += minParsed.isFr ? 0 : minParsed.value;
                        }
                        else
                        {
                            var parsed = ParseTrackValue(arg, containerSize);
                            trackMinSize += parsed.isFr ? 0 : parsed.value;
                        }
                    }
                    count = trackMinSize > 0
                        ? Math.Max(1, (int)Math.Floor(containerSize / trackMinSize))
                        : 1;
                    count = Math.Min(count, 100); // safety cap
                }
                else if (first is CssNumberValue num)
                {
                    count = Math.Max(1, Math.Min((int)num.Value, 100));
                }
                else if (first is CssDimensionValue dim)
                {
                    count = Math.Max(1, Math.Min((int)dim.Value, 100));
                }
                else
                {
                    count = 1;
                }

                // Remaining arguments are track values to repeat
                for (int rep = 0; rep < count; rep++)
                {
                    for (int j = 1; j < fn.Arguments.Count; j++)
                    {
                        var arg = fn.Arguments[j];
                        if (arg is CssListValue innerList)
                        {
                            for (int k = 0; k < innerList.Values.Count; k++)
                                output.Add(innerList.Values[k]);
                        }
                        else
                        {
                            output.Add(arg);
                        }
                    }
                }
            }
            else
            {
                output.Add(val);
            }
        }

        private static (float value, bool isFr) ParseTrackValue(object val, float containerSize)
        {
            if (val is CssDimensionValue dim)
            {
                if (dim.Unit == "fr")
                    return (dim.Value, true);
                return (dim.Value, false);
            }
            if (val is CssNumberValue num)
                return (num.Value, false);
            if (val is CssPercentageValue pct)
                return (pct.Value / 100f * containerSize, false);
            if (val is CssKeywordValue kwVal)
            {
                if (kwVal.Keyword == "auto")
                    return (0, true); // auto acts like 1fr
                if (kwVal.Keyword == "min-content")
                    return (0, false);
                if (kwVal.Keyword == "max-content")
                    return (0, true);
            }
            if (val is CssFunctionValue fn)
            {
                if (fn.Name == "minmax" && fn.Arguments.Count >= 2)
                {
                    // minmax(min, max): use max if it's fr, otherwise use min as a floor
                    var maxVal = ParseTrackValue(fn.Arguments[fn.Arguments.Count - 1], containerSize);
                    if (maxVal.isFr)
                    {
                        // fr-based max: report as fr so it gets flexible space,
                        // minimum will be enforced in ResolveTrackList via minmax tracking
                        return maxVal;
                    }
                    var minVal = ParseTrackValue(fn.Arguments[0], containerSize);
                    // Both fixed: use maximum of the two
                    return (Math.Max(minVal.value, maxVal.value), false);
                }
                if (fn.Name == "fit-content" && fn.Arguments.Count >= 1)
                {
                    // fit-content(limit): starts at min-content, grows up to limit
                    var limit = ParseTrackValue(fn.Arguments[0], containerSize);
                    return (limit.value, false);
                }
            }
            return (0, false);
        }

        /// <summary>
        /// Extract minmax minimum constraint for a track value, or 0 if none.
        /// </summary>
        private static float GetMinmaxFloor(object val, float containerSize)
        {
            if (val is CssFunctionValue fn && fn.Name == "minmax" && fn.Arguments.Count >= 2)
            {
                var minVal = ParseTrackValue(fn.Arguments[0], containerSize);
                return minVal.value;
            }
            return 0;
        }

        private static float AlignOffset(CssAlignItems align, float cellSize, float itemSize)
        {
            float space = cellSize - itemSize;
            if (space <= 0) return 0;
            switch (align)
            {
                case CssAlignItems.Center: return space * 0.5f;
                case CssAlignItems.End:
                case CssAlignItems.FlexEnd: return space;
                case CssAlignItems.Start:
                case CssAlignItems.FlexStart:
                case CssAlignItems.Baseline: return 0;
                default: return 0; // Stretch, Normal → 0 offset (stretch handled separately)
            }
        }

        private static bool IsStretch(CssAlignItems align)
        {
            return align == CssAlignItems.Stretch || align == CssAlignItems.Normal;
        }

        /// <summary>
        /// Parse grid-template-areas value into named area regions.
        /// Input: list of CssStringValue like "header header" "sidebar main" "footer footer"
        /// Output: dictionary mapping area name to (rowStart, colStart, rowSpan, colSpan).
        /// </summary>
        private static Dictionary<string, (int rowStart, int colStart, int rowSpan, int colSpan)>? ParseGridTemplateAreas(object? raw)
        {
            if (raw == null) return null;
            if (raw is CssKeywordValue kw && kw.Keyword == "none") return null;

            var rows = new List<string[]>();

            void AddRow(string rowStr)
            {
                var cells = rowStr.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (cells.Length > 0) rows.Add(cells);
            }

            if (raw is CssStringValue sv)
            {
                AddRow(sv.Value);
            }
            else if (raw is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (list.Values[i] is CssStringValue s)
                        AddRow(s.Value);
                }
            }
            else
            {
                return null;
            }

            if (rows.Count == 0) return null;

            var areas = new Dictionary<string, (int rowStart, int colStart, int rowSpan, int colSpan)>();
            for (int r = 0; r < rows.Count; r++)
            {
                for (int c = 0; c < rows[r].Length; c++)
                {
                    string name = rows[r][c];
                    if (name == ".") continue; // unnamed cell

                    if (areas.ContainsKey(name))
                    {
                        // Expand existing area
                        var a = areas[name];
                        int newRowEnd = Math.Max(a.rowStart + a.rowSpan, r + 1);
                        int newColEnd = Math.Max(a.colStart + a.colSpan, c + 1);
                        int newRowStart = Math.Min(a.rowStart, r);
                        int newColStart = Math.Min(a.colStart, c);
                        areas[name] = (newRowStart, newColStart, newRowEnd - newRowStart, newColEnd - newColStart);
                    }
                    else
                    {
                        areas[name] = (r, c, 1, 1);
                    }
                }
            }

            return areas.Count > 0 ? areas : null;
        }

        private sealed class GridItem
        {
            public StyledElement? StyledElement { get; set; }
            public LayoutBox Box { get; set; } = null!;
            public float ContentWidth { get; set; }
            public float ContentHeight { get; set; }
            public int RowStart { get; set; } = -1; // -1 = auto
            public int ColStart { get; set; } = -1;
            public int RowSpan { get; set; } = 1;
            public int ColSpan { get; set; } = 1;
            public bool Placed { get; set; }
            public int Order { get; set; }
            public int OriginalIndex { get; set; }
            public string? AreaName { get; set; }
        }
    }
}
