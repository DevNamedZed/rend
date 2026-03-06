using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Style;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// CSS Table layout: fixed and automatic table layout algorithms.
    /// CSS 2.1 §17
    /// </summary>
    internal static class TableLayout
    {
        internal static bool _debugTable = false;
        public static void Layout(LayoutBox parent, LayoutContext context)
        {
            var styledElement = parent.StyledNode as StyledElement;
            if (styledElement == null) return;

            var style = styledElement.Style;
            float containerWidth = parent.ContentRect.Width;
            bool isFixed = style.TableLayout == CssTableLayout.Fixed;

            // Build table context: collect rows and cells
            var tableCtx = new TableContext();
            CollectTableStructure(styledElement, tableCtx);

            if (tableCtx.Rows.Count == 0) return;

            int numCols = tableCtx.GetColumnCount();
            if (numCols == 0) return;

            // Layout cells — use CSS border-spacing (defaults to 0 if collapsed)
            bool collapsed = style.BorderCollapse == CssBorderCollapse.Collapse;
            float borderSpacingH = collapsed ? 0 : Math.Max(style.BorderSpacing, 0);
            float borderSpacingV = collapsed ? 0 : Math.Max(style.BorderSpacingV, 0);

            // Calculate column widths
            // Chrome's undistributable space: border-spacing + table border/padding
            // For collapsed tables, the outer halves of collapsed borders are undistributable.
            float[] colWidths;
            float totalSpacing = (numCols + 1) * borderSpacingH;

            // Pre-compute collapsed outer border halves from CSS values (before full layout)
            float preCollapseOuterLeft = 0, preCollapseOuterRight = 0;
            if (collapsed)
            {
                // Scan cells to find max border on left and right table edges
                float maxLeftBorder = 0, maxRightBorder = 0;
                for (int r = 0; r < tableCtx.Rows.Count; r++)
                {
                    var row = tableCtx.Rows[r];
                    if (row.Cells.Count > 0)
                    {
                        // First cell's left border (only if border-style is not none)
                        var firstCell = row.Cells[0];
                        if (firstCell.StyledElement != null)
                        {
                            var fs = firstCell.StyledElement.Style;
                            float lb = fs.BorderLeftStyle != CssBorderStyle.None ? fs.BorderLeftWidth : 0;
                            if (lb > maxLeftBorder) maxLeftBorder = lb;
                        }
                        // Last cell's right border (only if border-style is not none)
                        var lastCell = row.Cells[row.Cells.Count - 1];
                        if (lastCell.StyledElement != null)
                        {
                            var ls = lastCell.StyledElement.Style;
                            float rb = ls.BorderRightStyle != CssBorderStyle.None ? ls.BorderRightWidth : 0;
                            if (rb > maxRightBorder) maxRightBorder = rb;
                        }
                    }
                }
                // Also check table element's own borders (only if border-style is not none)
                float tableBorderLeft = style.BorderLeftStyle != CssBorderStyle.None ? style.BorderLeftWidth : 0;
                float tableBorderRight = style.BorderRightStyle != CssBorderStyle.None ? style.BorderRightWidth : 0;
                maxLeftBorder = Math.Max(maxLeftBorder, tableBorderLeft);
                maxRightBorder = Math.Max(maxRightBorder, tableBorderRight);

                preCollapseOuterLeft = maxLeftBorder / 2f;
                preCollapseOuterRight = maxRightBorder / 2f;
                if (_debugTable) Console.WriteLine($"[TABLE] preCollapse: maxL={maxLeftBorder}, maxR={maxRightBorder}, tableL={style.BorderLeftWidth}, tableR={style.BorderRightWidth}, outerL={preCollapseOuterLeft}, outerR={preCollapseOuterRight}, totalSpacing={totalSpacing}");
            }

            // Chrome: assignable = used_table_size - undistributable_space
            // undistributable_space = border_padding + (numCols+1)*spacing
            // For collapsed: border_padding includes outer border halves
            float undistributable = totalSpacing + preCollapseOuterLeft + preCollapseOuterRight;
            float colAvailWidth = containerWidth - undistributable;
            if (isFixed)
                colWidths = CalculateFixedWidths(tableCtx, numCols, ref colAvailWidth);
            else
                colWidths = CalculateAutoWidths(tableCtx, numCols, colAvailWidth, context,
                    hasExplicitWidth: !float.IsNaN(style.Width), collapsed: collapsed);

            if (_debugTable)
            {
                Console.WriteLine($"[TABLE] containerWidth={containerWidth}, colAvailWidth={colAvailWidth}, numCols={numCols}, hasExplicit={!float.IsNaN(style.Width)}, collapsed={collapsed}");
                for (int c = 0; c < numCols; c++)
                    Console.WriteLine($"[TABLE] Col {c}: width={colWidths[c]:F4}");
            }

            int numRows = tableCtx.Rows.Count;

            // Build occupied grid for rowspan/colspan tracking
            var occupied = new bool[numRows, numCols];
            float[] rowHeights = new float[numRows];
            var rowBoxes = new LayoutBox[numRows];

            // Track cells with rowspan > 1 for height distribution
            var rowspanCells = new List<RowspanCellInfo>();

            // First pass: lay out all cells, determine row heights for rowspan=1 cells
            for (int r = 0; r < numRows; r++)
            {
                var row = tableCtx.Rows[r];
                var rowBox = new LayoutBox(row.StyledElement, BoxType.TableRow);
                rowBoxes[r] = rowBox;

                // visibility: collapse — row takes no space but structure is preserved
                if (row.StyledElement?.Style.Visibility == CssVisibility.Collapse)
                {
                    rowHeights[r] = 0;
                    continue;
                }

                int cellIdx = 0;
                for (int c = 0; c < numCols && cellIdx < row.Cells.Count; c++)
                {
                    if (occupied[r, c]) continue;

                    var cell = row.Cells[cellIdx++];
                    if (cell == null) continue;

                    // Mark occupied cells
                    int effectiveRowSpan = Math.Min(cell.RowSpan, numRows - r);
                    int effectiveColSpan = Math.Min(cell.ColSpan, numCols - c);
                    for (int rs = 0; rs < effectiveRowSpan; rs++)
                        for (int cs = 0; cs < effectiveColSpan; cs++)
                            occupied[r + rs, c + cs] = true;

                    // Calculate cell width (with colspan)
                    float cellWidth = colWidths[c];
                    for (int cs = 1; cs < effectiveColSpan; cs++)
                        cellWidth += colWidths[c + cs] + borderSpacingH;

                    var cellBox = new LayoutBox(cell.StyledElement, BoxType.TableCell);
                    if (cell.StyledElement != null)
                        BoxModelCalculator.ApplyBoxModel(cellBox, cell.StyledElement.Style, cellWidth);

                    float contentWidth = cellWidth - cellBox.PaddingLeft - cellBox.PaddingRight
                                       - cellBox.BorderLeftWidth - cellBox.BorderRightWidth;
                    contentWidth = Math.Max(0, contentWidth);

                    // Temporary Y — will be repositioned in second pass
                    // Table cells establish their own BFC, so isolate from outer float context
                    cellBox.ContentRect = new RectF(0, 0, contentWidth, 0);
                    var prevFloat = context.FloatContext;
                    context.FloatContext = null;
                    BlockFormattingContext.LayoutChildren(cellBox, context);
                    context.FloatContext = prevFloat;
                    float cellContentHeight = CalculateAutoHeight(cellBox);

                    // Respect explicit height on the cell (e.g. <td style="height:30px">)
                    if (cell.StyledElement != null)
                    {
                        float explicitH = cell.StyledElement.Style.Height;
                        if (!float.IsNaN(explicitH) && explicitH > 0)
                        {
                            // height on table cells is content-box by default (CSS 2.1 §17.5.3)
                            float explicitContentH = explicitH;
                            if (cell.StyledElement.Style.BoxSizing == CssBoxSizing.BorderBox)
                            {
                                explicitContentH -= cellBox.PaddingTop + cellBox.PaddingBottom
                                                  + cellBox.BorderTopWidth + cellBox.BorderBottomWidth;
                            }
                            if (explicitContentH > cellContentHeight)
                                cellContentHeight = explicitContentH;
                        }
                    }

                    // Truncate to 1/64th pixel precision (matching Chrome's LayoutUnit).
                    cellContentHeight = (int)(cellContentHeight * 64f) / 64f;
                    cellBox.ContentRect = new RectF(0, 0, contentWidth, cellContentHeight);

                    float totalCellHeight = cellContentHeight + cellBox.PaddingTop + cellBox.PaddingBottom
                                          + cellBox.BorderTopWidth + cellBox.BorderBottomWidth;
                    totalCellHeight = (int)(totalCellHeight * 64f) / 64f;

                    if (effectiveRowSpan == 1)
                    {
                        if (totalCellHeight > rowHeights[r]) rowHeights[r] = totalCellHeight;
                    }
                    else
                    {
                        rowspanCells.Add(new RowspanCellInfo
                        {
                            CellBox = cellBox,
                            StartRow = r,
                            RowSpan = effectiveRowSpan,
                            Col = c,
                            ColSpan = effectiveColSpan,
                            TotalHeight = totalCellHeight
                        });
                    }

                    rowBox.AddChild(cellBox);

                    // Skip columns occupied by colspan
                    c += effectiveColSpan - 1;
                }

                // Enforce explicit row height from CSS (e.g., <tr style="height:80px">)
                float rowSpecHeight = row.StyledElement?.Style.Height ?? float.NaN;
                if (!float.IsNaN(rowSpecHeight) && rowSpecHeight > 0)
                {
                    // Resolve deferred percentage height
                    rowSpecHeight = DimensionResolver.ResolvePercentHeight(rowSpecHeight, float.NaN);
                    if (!float.IsNaN(rowSpecHeight) && rowSpecHeight > rowHeights[r])
                        rowHeights[r] = rowSpecHeight;
                }
            }

            // Distribute rowspan cell heights across spanned rows
            for (int s = 0; s < rowspanCells.Count; s++)
            {
                var rsc = rowspanCells[s];
                float spannedHeight = 0;
                for (int rs = 0; rs < rsc.RowSpan; rs++)
                    spannedHeight += rowHeights[rsc.StartRow + rs] + (rs > 0 ? borderSpacingV : 0);

                if (rsc.TotalHeight > spannedHeight)
                {
                    float extra = rsc.TotalHeight - spannedHeight;
                    // Distribute extra height proportionally to existing row heights
                    float totalRowHeight = 0;
                    for (int rs = 0; rs < rsc.RowSpan; rs++)
                        totalRowHeight += rowHeights[rsc.StartRow + rs];

                    if (totalRowHeight > 0)
                    {
                        for (int rs = 0; rs < rsc.RowSpan; rs++)
                            rowHeights[rsc.StartRow + rs] += extra * (rowHeights[rsc.StartRow + rs] / totalRowHeight);
                    }
                    else
                    {
                        // All spanned rows have zero height: distribute equally
                        float perRow = extra / rsc.RowSpan;
                        for (int rs = 0; rs < rsc.RowSpan; rs++)
                            rowHeights[rsc.StartRow + rs] += perRow;
                    }
                }
            }

            // Build column X positions (border-spacing applies before first column too)
            float[] colXPositions = new float[numCols];
            float cx = parent.ContentRect.X + borderSpacingH;
            for (int c = 0; c < numCols; c++)
            {
                colXPositions[c] = cx;
                cx += colWidths[c] + borderSpacingH;
            }

            // Build cell-to-column mapping by replaying the grid walk
            var cellColumns = new Dictionary<LayoutBox, int>();
            var occupied2 = new bool[numRows, numCols];
            for (int r = 0; r < numRows; r++)
            {
                var row2 = tableCtx.Rows[r];
                int cellIdx2 = 0;
                for (int c = 0; c < numCols && cellIdx2 < row2.Cells.Count; c++)
                {
                    if (occupied2[r, c]) continue;
                    var cell2 = row2.Cells[cellIdx2++];
                    int rs2 = Math.Min(cell2.RowSpan, numRows - r);
                    int cs2 = Math.Min(cell2.ColSpan, numCols - c);
                    for (int ri = 0; ri < rs2; ri++)
                        for (int ci = 0; ci < cs2; ci++)
                            occupied2[r + ri, c + ci] = true;

                    // Map the corresponding child box in rowBoxes[r]
                    int childIdx = cellIdx2 - 1;
                    if (childIdx < rowBoxes[r].Children.Count)
                        cellColumns[rowBoxes[r].Children[childIdx]] = c;

                    c += cs2 - 1;
                }
            }

            // Apply border collapsing BEFORE positioning so collapsed border widths
            // are used when computing cell positions (avoids gaps from zeroed borders).
            float collapseOuterTop = 0, collapseOuterBottom = 0;
            float collapseOuterLeft = 0, collapseOuterRight = 0;
            if (collapsed)
            {
                CollapseBorders(rowBoxes, numRows, numCols, occupied2, cellColumns,
                    out collapseOuterTop, out collapseOuterBottom,
                    out collapseOuterLeft, out collapseOuterRight);

                // Rebuild column X positions with outer-half offset.
                // Column widths were already distributed to the correct assignable size
                // (container width minus outer border halves), so no proportional scaling needed.
                {
                    float cx2 = parent.ContentRect.X + collapseOuterLeft;
                    for (int c = 0; c < numCols; c++)
                    {
                        colXPositions[c] = cx2;
                        cx2 += colWidths[c];
                    }
                }

                // Recalculate cell content widths now that borders have been collapsed,
                // then re-layout children so they use the correct available width.
                for (int r = 0; r < numRows; r++)
                {
                    for (int ci = 0; ci < rowBoxes[r].Children.Count; ci++)
                    {
                        var cellBox = rowBoxes[r].Children[ci];
                        int cellCol = cellColumns.ContainsKey(cellBox) ? cellColumns[cellBox] : 0;
                        float cellWidth = colWidths[cellCol];
                        int cs = 1;
                        var cellEl = cellBox.StyledNode as StyledElement;
                        if (cellEl != null)
                        {
                            // Find colspan from the table context
                            var rowCtx = tableCtx.Rows[r];
                            for (int cIdx = 0; cIdx < rowCtx.Cells.Count; cIdx++)
                            {
                                if (rowCtx.Cells[cIdx].StyledElement == cellEl)
                                {
                                    cs = Math.Min(rowCtx.Cells[cIdx].ColSpan, numCols - cellCol);
                                    break;
                                }
                            }
                        }
                        for (int s = 1; s < cs; s++)
                            cellWidth += colWidths[cellCol + s] + borderSpacingH;

                        float newContentW = cellWidth - cellBox.PaddingLeft - cellBox.PaddingRight
                                          - cellBox.BorderLeftWidth - cellBox.BorderRightWidth;
                        newContentW = Math.Max(0, newContentW);

                        // Re-layout children with corrected content width
                        // Table cells establish their own BFC, so isolate from outer float context
                        cellBox.ClearChildren();
                        cellBox.LineBoxes = null;
                        cellBox.ContentRect = new RectF(0, 0, newContentW, 0);
                        var prevFloat2 = context.FloatContext;
                        context.FloatContext = null;
                        BlockFormattingContext.LayoutChildren(cellBox, context);
                        context.FloatContext = prevFloat2;
                        float cellContentH = (int)(CalculateAutoHeight(cellBox) * 64f) / 64f;

                        // Respect explicit height on the cell
                        var cellStyledEl = cellBox.StyledNode as StyledElement;
                        if (cellStyledEl != null)
                        {
                            float explicitH = cellStyledEl.Style.Height;
                            if (!float.IsNaN(explicitH) && explicitH > 0)
                            {
                                float explicitContentH = explicitH;
                                if (cellStyledEl.Style.BoxSizing == CssBoxSizing.BorderBox)
                                {
                                    explicitContentH -= cellBox.PaddingTop + cellBox.PaddingBottom
                                                      + cellBox.BorderTopWidth + cellBox.BorderBottomWidth;
                                }
                                if (explicitContentH > cellContentH)
                                    cellContentH = explicitContentH;
                            }
                        }

                        cellBox.ContentRect = new RectF(0, 0, newContentW, cellContentH);
                    }
                }

                // Recalculate row heights now that borders have been collapsed.
                // The first pass used original border widths; collapsed borders
                // reduce the total cell height and thus the row height.
                var rowspanBoxes = new HashSet<LayoutBox>();
                for (int s = 0; s < rowspanCells.Count; s++)
                    rowspanBoxes.Add(rowspanCells[s].CellBox);

                for (int r = 0; r < numRows; r++)
                {
                    float maxH = 0;
                    for (int ci = 0; ci < rowBoxes[r].Children.Count; ci++)
                    {
                        var cellBox = rowBoxes[r].Children[ci];
                        if (rowspanBoxes.Contains(cellBox)) continue;
                        float h = cellBox.ContentRect.Height + cellBox.PaddingTop + cellBox.PaddingBottom
                                 + cellBox.BorderTopWidth + cellBox.BorderBottomWidth;
                        if (ci == 0 && _debugTable) Console.WriteLine($"[TABLE] Row {r}: contentH={cellBox.ContentRect.Height} padT={cellBox.PaddingTop} padB={cellBox.PaddingBottom} borT={cellBox.BorderTopWidth} borB={cellBox.BorderBottomWidth} totalH={h}");
                        if (h > maxH) maxH = h;
                    }
                    // Enforce explicit row height from CSS
                    var row = tableCtx.Rows[r];
                    float rowSpecH = row.StyledElement?.Style.Height ?? float.NaN;
                    if (!float.IsNaN(rowSpecH) && rowSpecH > 0)
                    {
                        rowSpecH = DimensionResolver.ResolvePercentHeight(rowSpecH, float.NaN);
                        if (!float.IsNaN(rowSpecH) && rowSpecH > maxH)
                            maxH = rowSpecH;
                    }
                    if (maxH > 0) rowHeights[r] = maxH;
                }
            }

            // Layout top captions (caption-side: top or default)
            float cursorY = parent.ContentRect.Y;
            for (int cap = 0; cap < tableCtx.Captions.Count; cap++)
            {
                var captionEl = tableCtx.Captions[cap];
                if (captionEl.Style.CaptionSide == CssCaptionSide.Bottom) continue;
                cursorY = LayoutCaption(captionEl, parent, context, containerWidth, cursorY);
            }

            // Second pass: position cells with final Y coordinates
            // border-spacing applies before the first row too.
            // In collapsed mode, offset by the outer half of the top border
            // so cell positions start at the border midpoint (matching Chrome).
            if (_debugTable) Console.WriteLine($"[TABLE] cursorY before rows = {cursorY}, borderSpacingV={borderSpacingV}, collapseOuterTop={collapseOuterTop}");
            cursorY += borderSpacingV + collapseOuterTop;
            float[] rowYPositions = new float[numRows];
            for (int r = 0; r < numRows; r++)
            {
                rowYPositions[r] = cursorY;
                if (_debugTable) Console.WriteLine($"[TABLE] Row {r}: Y={cursorY}, height={rowHeights[r]}");
                cursorY += rowHeights[r] + borderSpacingV;

            }

            for (int r = 0; r < numRows; r++)
            {
                var rowBox = rowBoxes[r];

                for (int ci = 0; ci < rowBox.Children.Count; ci++)
                {
                    var cellBox = rowBox.Children[ci];
                    int cellCol = cellColumns.ContainsKey(cellBox) ? cellColumns[cellBox] : 0;
                    float cellX = colXPositions[cellCol];

                    // Calculate cell height (may span multiple rows)
                    float cellHeight = rowHeights[r];
                    int cellRowSpan = GetCellRowSpan(cellBox, rowspanCells, r);
                    if (cellRowSpan > 1)
                    {
                        cellHeight = 0;
                        for (int rs = 0; rs < cellRowSpan && r + rs < numRows; rs++)
                            cellHeight += rowHeights[r + rs] + (rs > 0 ? borderSpacingV : 0);
                    }

                    // Position the cell — content rect spans the full row height
                    // so cell backgrounds fill the entire row.
                    float cellContentHeight = cellBox.ContentRect.Height;
                    float fullCellContentH = cellHeight - cellBox.PaddingTop - cellBox.PaddingBottom
                                           - cellBox.BorderTopWidth - cellBox.BorderBottomWidth;
                    if (fullCellContentH < cellContentHeight) fullCellContentH = cellContentHeight;

                    cellBox.ContentRect = new RectF(
                        cellX + cellBox.MarginLeft + cellBox.BorderLeftWidth + cellBox.PaddingLeft,
                        rowYPositions[r] + cellBox.MarginTop + cellBox.BorderTopWidth + cellBox.PaddingTop,
                        cellBox.ContentRect.Width, fullCellContentH);

                    // Reposition children from (0,0) to actual cell position
                    float dx = cellBox.ContentRect.X;
                    float dy = cellBox.ContentRect.Y;
                    for (int cci = 0; cci < cellBox.Children.Count; cci++)
                        OffsetBoxInPlace(cellBox.Children[cci], dx, dy);
                    if (cellBox.LineBoxes != null)
                    {
                        for (int li = 0; li < cellBox.LineBoxes.Count; li++)
                        {
                            if (_debugTable && li == 0 && ci == 0)
                                Console.WriteLine($"[TABLE] Cell r={r} c=0: lineBox.X={cellBox.LineBoxes[li].X:F2} lineBox.Y={cellBox.LineBoxes[li].Y:F2} dx={dx:F2} dy={dy:F2}");
                            cellBox.LineBoxes[li].X += dx;
                            cellBox.LineBoxes[li].Y += dy;
                            if (_debugTable && li == 0 && ci == 0)
                                Console.WriteLine($"[TABLE] After offset: lineBox.X={cellBox.LineBoxes[li].X:F2} lineBox.Y={cellBox.LineBoxes[li].Y:F2}");
                        }
                    }

                    // Apply vertical-align: only offset children within the cell,
                    // NOT the cell's ContentRect (background must span the full row).
                    float freeSpace = fullCellContentH - cellContentHeight;
                    if (freeSpace > 0)
                    {
                        var valign = CssVerticalAlign.Baseline;
                        if (cellBox.StyledNode is StyledElement cellEl)
                            valign = cellEl.Style.VerticalAlign;

                        float offsetY = 0;
                        switch (valign)
                        {
                            case CssVerticalAlign.Middle:
                                offsetY = freeSpace / 2;
                                break;
                            case CssVerticalAlign.Bottom:
                            case CssVerticalAlign.TextBottom:
                                offsetY = freeSpace;
                                break;
                        }

                        if (offsetY > 0)
                        {
                            // Only move children and line boxes, not the cell box itself
                            for (int cci = 0; cci < cellBox.Children.Count; cci++)
                                OffsetBoxInPlace(cellBox.Children[cci], 0, offsetY);
                            if (cellBox.LineBoxes != null)
                            {
                                for (int li = 0; li < cellBox.LineBoxes.Count; li++)
                                    cellBox.LineBoxes[li].Y += offsetY;
                            }
                        }
                    }
                }

                rowBox.ContentRect = new RectF(parent.ContentRect.X, rowYPositions[r],
                                               containerWidth, rowHeights[r]);
                parent.AddChild(rowBox);
            }

            // In collapsed mode, account for the outer half of the bottom border
            cursorY += collapseOuterBottom;

            // Layout bottom captions
            for (int cap = 0; cap < tableCtx.Captions.Count; cap++)
            {
                var captionEl = tableCtx.Captions[cap];
                if (captionEl.Style.CaptionSide != CssCaptionSide.Bottom) continue;
                cursorY = LayoutCaption(captionEl, parent, context, containerWidth, cursorY);
            }

            // For auto-width tables, shrink-wrap the parent to actual content width
            if (float.IsNaN(style.Width))
            {
                float totalColWidth = 0;
                for (int i = 0; i < numCols; i++)
                    totalColWidth += colWidths[i];
                float actualWidth = totalColWidth + (numCols + 1) * borderSpacingH
                                  + collapseOuterLeft + collapseOuterRight;
                if (actualWidth < containerWidth)
                {
                    parent.ContentRect = new RectF(
                        parent.ContentRect.X, parent.ContentRect.Y,
                        actualWidth, parent.ContentRect.Height);
                }
            }
        }

        private static void CollapseBorders(LayoutBox[] rowBoxes, int numRows, int numCols,
            bool[,] occupied, Dictionary<LayoutBox, int> cellColumns,
            out float outerHalfTop, out float outerHalfBottom,
            out float outerHalfLeft, out float outerHalfRight)
        {
            // Build a grid mapping (row, col) -> cellBox for quick adjacency lookups
            var cellGrid = new LayoutBox[numRows, numCols];
            for (int r = 0; r < numRows; r++)
            {
                var rowBox = rowBoxes[r];
                for (int ci = 0; ci < rowBox.Children.Count; ci++)
                {
                    var cellBox = rowBox.Children[ci];
                    int col = cellColumns.ContainsKey(cellBox) ? cellColumns[cellBox] : 0;
                    cellGrid[r, col] = cellBox;
                    cellBox.CollapsedBorderCell = true;
                }
            }

            // Collapse adjacent horizontal borders: cell's right border vs right neighbor's left border
            // Per CSS 2.1 §17.6.2, the collapsed border width is shared equally between cells.
            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < numCols - 1; c++)
                {
                    var left = cellGrid[r, c];
                    var right = cellGrid[r, c + 1];
                    if (left == null || right == null) continue;
                    if (ReferenceEquals(left, right)) continue; // same cell (colspan)

                    var leftStyle = left.StyledNode?.Style;
                    var rightStyle = right.StyledNode?.Style;
                    float collapsedWidth = GetCollapsedBorderWidth(
                        left.BorderRightWidth, leftStyle?.BorderRightStyle ?? CssBorderStyle.None,
                        right.BorderLeftWidth, rightStyle?.BorderLeftStyle ?? CssBorderStyle.None);
                    // Each cell gets half the collapsed border for layout purposes
                    left.BorderRightWidth = collapsedWidth / 2f;
                    right.BorderLeftWidth = collapsedWidth / 2f;
                }
            }

            // Collapse adjacent vertical borders: cell's bottom border vs bottom neighbor's top border
            for (int r = 0; r < numRows - 1; r++)
            {
                for (int c = 0; c < numCols; c++)
                {
                    var top = cellGrid[r, c];
                    var bottom = cellGrid[r + 1, c];
                    if (top == null || bottom == null) continue;
                    if (ReferenceEquals(top, bottom)) continue; // same cell (rowspan)

                    var topStyle = top.StyledNode?.Style;
                    var bottomStyle = bottom.StyledNode?.Style;
                    float collapsedWidth = GetCollapsedBorderWidth(
                        top.BorderBottomWidth, topStyle?.BorderBottomStyle ?? CssBorderStyle.None,
                        bottom.BorderTopWidth, bottomStyle?.BorderTopStyle ?? CssBorderStyle.None);
                    // Each cell gets half the collapsed border for layout purposes
                    top.BorderBottomWidth = collapsedWidth / 2f;
                    bottom.BorderTopWidth = collapsedWidth / 2f;
                }
            }

            // Collapse outer edges: per CSS 2.1 §17.6.2, the outer border of the table
            // participates in the collapse as well. For layout purposes, the outer cells
            // get half the collapsed border width on their outer edges.
            // Top row outer edge
            for (int c = 0; c < numCols; c++)
            {
                var cell = cellGrid[0, c];
                if (cell != null) cell.BorderTopWidth /= 2f;
            }
            // Bottom row outer edge
            for (int c = 0; c < numCols; c++)
            {
                var cell = cellGrid[numRows - 1, c];
                if (cell != null) cell.BorderBottomWidth /= 2f;
            }
            // Left column outer edge
            for (int r = 0; r < numRows; r++)
            {
                var cell = cellGrid[r, 0];
                if (cell != null) cell.BorderLeftWidth /= 2f;
            }
            // Right column outer edge
            for (int r = 0; r < numRows; r++)
            {
                var cell = cellGrid[r, numCols - 1];
                if (cell != null) cell.BorderRightWidth /= 2f;
            }

            // Compute the outer half of each table edge (the portion extending outside cells).
            // After the /= 2f above, each outer-edge cell's border stores the inner half.
            // The outer half is the same value (borderWidth / 2).
            outerHalfTop = 0;
            for (int c = 0; c < numCols; c++)
            {
                var cell = cellGrid[0, c];
                if (cell != null) outerHalfTop = Math.Max(outerHalfTop, cell.BorderTopWidth);
            }
            outerHalfBottom = 0;
            for (int c = 0; c < numCols; c++)
            {
                var cell = cellGrid[numRows - 1, c];
                if (cell != null) outerHalfBottom = Math.Max(outerHalfBottom, cell.BorderBottomWidth);
            }
            outerHalfLeft = 0;
            for (int r = 0; r < numRows; r++)
            {
                var cell = cellGrid[r, 0];
                if (cell != null) outerHalfLeft = Math.Max(outerHalfLeft, cell.BorderLeftWidth);
            }
            outerHalfRight = 0;
            for (int r = 0; r < numRows; r++)
            {
                var cell = cellGrid[r, numCols - 1];
                if (cell != null) outerHalfRight = Math.Max(outerHalfRight, cell.BorderRightWidth);
            }
        }

        /// <summary>
        /// Computes the collapsed border width from two adjacent borders per CSS 2.1 §17.6.2.
        /// Hidden suppresses the border. Otherwise the wider border wins.
        /// </summary>
        private static float GetCollapsedBorderWidth(float widthA, CssBorderStyle styleA,
                                                     float widthB, CssBorderStyle styleB)
        {
            if (styleA == CssBorderStyle.Hidden || styleB == CssBorderStyle.Hidden)
                return 0;
            if (styleA == CssBorderStyle.None && styleB == CssBorderStyle.None)
                return 0;
            return Math.Max(widthA, widthB);
        }

        /// <summary>
        /// Determines if border A wins over border B per CSS 2.1 §17.6.2.
        /// hidden always wins (both zeroed). Then wider wins. Then style priority.
        /// </summary>
        private static bool BorderWins(float widthA, CssBorderStyle styleA,
                                       float widthB, CssBorderStyle styleB)
        {
            // hidden always wins — both borders get suppressed, but the hidden side "wins"
            if (styleA == CssBorderStyle.Hidden) return true;
            if (styleB == CssBorderStyle.Hidden) return false;

            // none always loses
            if (styleA == CssBorderStyle.None && styleB != CssBorderStyle.None) return false;
            if (styleB == CssBorderStyle.None && styleA != CssBorderStyle.None) return true;

            // Wider border wins
            if (widthA > widthB) return true;
            if (widthB > widthA) return false;

            // Equal width: style priority (CSS 2.1 §17.6.2)
            return StylePriority(styleA) >= StylePriority(styleB);
        }

        private static int StylePriority(CssBorderStyle style)
        {
            switch (style)
            {
                case CssBorderStyle.Double: return 8;
                case CssBorderStyle.Solid: return 7;
                case CssBorderStyle.Dashed: return 6;
                case CssBorderStyle.Dotted: return 5;
                case CssBorderStyle.Ridge: return 4;
                case CssBorderStyle.Outset: return 3;
                case CssBorderStyle.Groove: return 2;
                case CssBorderStyle.Inset: return 1;
                default: return 0; // none
            }
        }

        private static int GetCellRowSpan(LayoutBox cellBox, List<RowspanCellInfo> rowspanCells, int row)
        {
            for (int i = 0; i < rowspanCells.Count; i++)
            {
                if (rowspanCells[i].CellBox == cellBox && rowspanCells[i].StartRow == row)
                    return rowspanCells[i].RowSpan;
            }
            return 1;
        }

        /// <summary>
        /// Matches Chrome's LayoutUnit::MulDiv: converts to 1/64px integer representation,
        /// performs (value * multiplier) / divisor in integer math, then converts back.
        /// This avoids floating-point precision loss that causes column width mismatches.
        /// </summary>
        private static float LayoutUnitMulDiv(float value, float multiplier, float divisor)
        {
            long vRaw = (long)(value * 64f);
            long mRaw = (long)(multiplier * 64f);
            long dRaw = (long)(divisor * 64f);
            if (dRaw == 0) return 0;
            long result = (vRaw * mRaw) / dRaw;
            return result / 64f;
        }

        private struct RowspanCellInfo
        {
            public LayoutBox CellBox;
            public int StartRow;
            public int RowSpan;
            public int Col;
            public int ColSpan;
            public float TotalHeight;
        }

        private static float LayoutCaption(StyledElement captionEl, LayoutBox parent,
            LayoutContext context, float containerWidth, float cursorY)
        {
            var captionBox = new LayoutBox(captionEl, BoxType.TableCaption);
            BoxModelCalculator.ApplyBoxModel(captionBox, captionEl.Style, containerWidth);

            float contentWidth = containerWidth - captionBox.PaddingLeft - captionBox.PaddingRight
                               - captionBox.BorderLeftWidth - captionBox.BorderRightWidth
                               - captionBox.MarginLeft - captionBox.MarginRight;
            contentWidth = Math.Max(0, contentWidth);

            float x = parent.ContentRect.X + captionBox.MarginLeft + captionBox.BorderLeftWidth + captionBox.PaddingLeft;
            float y = cursorY + captionBox.MarginTop + captionBox.BorderTopWidth + captionBox.PaddingTop;

            captionBox.ContentRect = new RectF(x, y, contentWidth, 0);
            BlockFormattingContext.LayoutChildren(captionBox, context);

            float contentHeight = CalculateAutoHeight(captionBox);
            captionBox.ContentRect = new RectF(x, y, contentWidth, contentHeight);

            parent.AddChild(captionBox);

            return y + contentHeight + captionBox.PaddingBottom + captionBox.BorderBottomWidth + captionBox.MarginBottom;
        }

        private static void CollectTableStructure(StyledElement table, TableContext ctx)
        {
            for (int i = 0; i < table.Children.Count; i++)
            {
                var child = table.Children[i];
                if (child.IsText || child is StyledPseudoElement) continue;
                var childEl = (StyledElement)child;
                var display = childEl.Style.Display;

                if (display == CssDisplay.TableCaption || childEl.TagName == "caption")
                {
                    ctx.Captions.Add(childEl);
                }
                else if (display == CssDisplay.TableRow)
                {
                    ctx.Rows.Add(BuildRow(childEl));
                }
                else if (display == CssDisplay.TableRowGroup ||
                         childEl.TagName == "thead" || childEl.TagName == "tbody" || childEl.TagName == "tfoot")
                {
                    for (int j = 0; j < childEl.Children.Count; j++)
                    {
                        var rowChild = childEl.Children[j];
                        if (!rowChild.IsText && !(rowChild is StyledPseudoElement))
                        {
                            var rowEl = (StyledElement)rowChild;
                            if (rowEl.Style.Display == CssDisplay.TableRow || rowEl.TagName == "tr")
                                ctx.Rows.Add(BuildRow(rowEl));
                        }
                    }
                }
            }
        }

        private static TableRow BuildRow(StyledElement rowElement)
        {
            var row = new TableRow { StyledElement = rowElement };
            for (int i = 0; i < rowElement.Children.Count; i++)
            {
                var child = rowElement.Children[i];
                if (child.IsText || child is StyledPseudoElement) continue;
                var cellEl = (StyledElement)child;
                if (cellEl.Style.Display == CssDisplay.TableCell ||
                    cellEl.TagName == "td" || cellEl.TagName == "th")
                {
                    int colspan = 1;
                    var csVal = cellEl.GetAttribute("colspan");
                    if (csVal != null && int.TryParse(csVal, out int cs) && cs > 0)
                        colspan = cs;

                    int rowspan = 1;
                    var rsVal = cellEl.GetAttribute("rowspan");
                    if (rsVal != null && int.TryParse(rsVal, out int rs) && rs > 0)
                        rowspan = rs;

                    row.Cells.Add(new TableCell
                    {
                        StyledElement = cellEl,
                        ColSpan = colspan,
                        RowSpan = rowspan
                    });
                }
            }
            return row;
        }

        private static float[] CalculateFixedWidths(TableContext ctx, int numCols, ref float containerWidth)
        {
            float[] widths = new float[numCols];
            float equalWidth = containerWidth / numCols;

            // First row determines widths in fixed layout.
            // CSS 2.1 §17.5.2.1: cell 'width' is the content width.
            // Column slot width = content width + padding + borders.
            if (ctx.Rows.Count > 0)
            {
                var firstRow = ctx.Rows[0];
                int col = 0;
                for (int c = 0; c < firstRow.Cells.Count && col < numCols; c++)
                {
                    var cell = firstRow.Cells[c];
                    float specWidth = DimensionResolver.ResolvePercentWidth(
                        cell.StyledElement?.Style.Width ?? float.NaN, containerWidth);

                    float w;
                    if (!float.IsNaN(specWidth) && specWidth > 0 && cell.StyledElement != null)
                    {
                        // Specified width is content width; add padding and border for column slot
                        var cs = cell.StyledElement.Style;
                        float padH = (float.IsNaN(cs.PaddingLeft) ? 0 : cs.PaddingLeft)
                                   + (float.IsNaN(cs.PaddingRight) ? 0 : cs.PaddingRight);
                        float borderH = (cs.BorderLeftStyle != CssBorderStyle.None ? cs.BorderLeftWidth : 0)
                                      + (cs.BorderRightStyle != CssBorderStyle.None ? cs.BorderRightWidth : 0);
                        w = specWidth + padH + borderH;
                    }
                    else
                    {
                        w = equalWidth;
                    }

                    for (int s = 0; s < cell.ColSpan && col < numCols; s++)
                    {
                        widths[col] = w / cell.ColSpan;
                        col++;
                    }
                }
                // Fill remaining
                for (; col < numCols; col++)
                    widths[col] = equalWidth;

                // If total column widths exceed container, expand the table (CSS 2.1 §17.5.2.1)
                float total = 0;
                for (int i = 0; i < numCols; i++) total += widths[i];
                if (total > containerWidth)
                    containerWidth = total;
            }
            else
            {
                for (int i = 0; i < numCols; i++)
                    widths[i] = equalWidth;
            }

            return widths;
        }

        private static float[] CalculateAutoWidths(TableContext ctx, int numCols, float containerWidth,
                                                    LayoutContext context, bool hasExplicitWidth = false,
                                                    bool collapsed = false)
        {
            float[] minWidths = new float[numCols];
            float[] maxWidths = new float[numCols];

            // Measure each cell's min-content and max-content width
            for (int r = 0; r < ctx.Rows.Count; r++)
            {
                int col = 0;
                for (int c = 0; c < ctx.Rows[r].Cells.Count && col < numCols; c++)
                {
                    var cell = ctx.Rows[r].Cells[c];
                    if (cell.StyledElement == null) { col += cell.ColSpan; continue; }

                    // Check for specified width (resolve deferred percentage)
                    float specWidth = DimensionResolver.ResolvePercentWidth(
                        cell.StyledElement.Style.Width, containerWidth);
                    if (!float.IsNaN(specWidth) && specWidth > 0 && cell.ColSpan == 1)
                    {
                        if (specWidth > minWidths[col]) minWidths[col] = specWidth;
                        if (specWidth > maxWidths[col]) maxWidths[col] = specWidth;
                        col += cell.ColSpan;
                        continue;
                    }

                    // Measure min-content width (very narrow container)
                    float minW = MeasureCellWidth(cell.StyledElement, 1f, context, collapsed);
                    // Measure max-content width — use containerWidth as upper bound
                    // to prevent percentage-width children from inflating.
                    // CSS Sizing L3: percentage widths behave as auto for intrinsic sizing,
                    // so capping at containerWidth approximates this behavior.
                    float maxMeasureWidth = Math.Min(10000f, containerWidth > 0 ? containerWidth : 10000f);
                    float maxW = MeasureCellWidth(cell.StyledElement, maxMeasureWidth, context, collapsed);

                    if (cell.ColSpan == 1)
                    {
                        if (minW > minWidths[col]) minWidths[col] = minW;
                        if (maxW > maxWidths[col]) maxWidths[col] = maxW;
                    }
                    else
                    {
                        // Distribute spanned widths equally for now
                        float perCol = minW / cell.ColSpan;
                        float perColMax = maxW / cell.ColSpan;
                        for (int s = 0; s < cell.ColSpan && col + s < numCols; s++)
                        {
                            if (perCol > minWidths[col + s]) minWidths[col + s] = perCol;
                            if (perColMax > maxWidths[col + s]) maxWidths[col + s] = perColMax;
                        }
                    }
                    col += cell.ColSpan;
                }
            }

            // Ensure min <= max for each column
            for (int i = 0; i < numCols; i++)
                if (maxWidths[i] < minWidths[i]) maxWidths[i] = minWidths[i];

            float totalMin = 0, totalMax = 0;
            for (int i = 0; i < numCols; i++)
            {
                totalMin += minWidths[i];
                totalMax += maxWidths[i];
            }

            if (_debugTable)
            {
                for (int i = 0; i < numCols; i++)
                    Console.WriteLine($"[TABLE] AutoWidth Col {i}: min={minWidths[i]:F4}, max={maxWidths[i]:F4}");
                Console.WriteLine($"[TABLE] totalMin={totalMin:F4}, totalMax={totalMax:F4}, containerWidth={containerWidth:F4}");
            }

            float[] widths = new float[numCols];

            // Chrome's width distribution algorithm (CSS Tables 3 §width-distribution-algorithm)
            // Uses LayoutUnit (1/64px) integer arithmetic with remainder tracking on last column.
            // All columns are "auto" unless they have explicit width (is_constrained).
            // Distribution follows the "guess" system: kMinGuess < kPercentageGuess < kSpecifiedGuess < kMaxGuess < kAboveMax
            if (totalMax <= containerWidth)
            {
                if (hasExplicitWidth)
                {
                    // kAboveMax: table has explicit width, target exceeds all max-content widths.
                    // Distribute excess proportionally to max_inline_size (for auto columns).
                    // Chrome: delta = distributable.MulDiv(col_max, total_auto_max)
                    float distributable = containerWidth - totalMax;
                    if (distributable == 0)
                    {
                        // Exact match — use max widths directly to avoid rounding errors
                        for (int i = 0; i < numCols; i++)
                            widths[i] = maxWidths[i];
                    }
                    else if (totalMax > 0)
                    {
                        // Use LayoutUnit-style integer MulDiv for each column,
                        // track remainder and add to last column
                        float remainingDeficit = distributable;
                        int lastCol = -1;
                        for (int i = 0; i < numCols; i++)
                        {
                            lastCol = i;
                            // Chrome: MulDiv = (raw * mul_raw) / div_raw in 1/64px integer math
                            float delta = LayoutUnitMulDiv(distributable, maxWidths[i], totalMax);
                            remainingDeficit -= delta;
                            widths[i] = maxWidths[i] + delta;
                        }
                        // Add remainder to last column (matches Chrome's rounding behavior)
                        if (lastCol >= 0 && Math.Abs(remainingDeficit) > 0.001f)
                            widths[lastCol] += remainingDeficit;
                    }
                    else
                    {
                        float perCol = containerWidth / numCols;
                        for (int i = 0; i < numCols; i++)
                            widths[i] = perCol;
                    }
                }
                else
                {
                    // Table width is auto and columns fit: use max-content widths
                    for (int i = 0; i < numCols; i++)
                        widths[i] = maxWidths[i];
                }
            }
            else if (totalMin >= containerWidth)
            {
                // kMinGuess: use min-content widths
                for (int i = 0; i < numCols; i++)
                    widths[i] = minWidths[i];
            }
            else
            {
                // kMaxGuess: interpolate between min and max
                // Chrome: auto columns grow proportionally to (max - min)
                float autoIncrease = totalMax - totalMin;
                float distributable = containerWidth - totalMin;
                float remainingDeficit = distributable;
                int lastCol = -1;
                for (int i = 0; i < numCols; i++)
                {
                    lastCol = i;
                    float colIncrease = maxWidths[i] - minWidths[i];
                    float delta;
                    if (autoIncrease > 0)
                        delta = LayoutUnitMulDiv(distributable, colIncrease, autoIncrease);
                    else
                        delta = distributable / numCols;
                    remainingDeficit -= delta;
                    widths[i] = minWidths[i] + delta;
                }
                if (lastCol >= 0 && Math.Abs(remainingDeficit) > 0.001f)
                    widths[lastCol] += remainingDeficit;
            }

            return widths;
        }

        private static float MeasureCellWidth(StyledElement cellElement, float availWidth,
            LayoutContext context, bool collapsed = false)
        {
            var box = new LayoutBox(cellElement, BoxType.TableCell);
            BoxModelCalculator.ApplyBoxModel(box, cellElement.Style, availWidth);

            // In border-collapse mode, cell borders are shared with neighbors.
            // For intrinsic sizing, use half the declared border widths (approximation
            // matching Chrome's behavior where collapsed borders are pre-computed).
            if (collapsed)
            {
                box.BorderLeftWidth /= 2f;
                box.BorderRightWidth /= 2f;
                box.BorderTopWidth /= 2f;
                box.BorderBottomWidth /= 2f;
            }

            float contentWidth = availWidth - box.PaddingLeft - box.PaddingRight
                               - box.BorderLeftWidth - box.BorderRightWidth;
            contentWidth = Math.Max(0, contentWidth);
            box.ContentRect = new RectF(0, 0, contentWidth, 0);
            var prevFloat = context.FloatContext;
            context.FloatContext = null;
            BlockFormattingContext.LayoutChildren(box, context);
            context.FloatContext = prevFloat;

            // Measure actual content extent
            float maxRight = 0;
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                float childWidth = child.ContentRect.Width;

                // CSS Sizing L3: percentage widths resolve to auto for intrinsic sizing.
                // For table children with percentage width, measure their actual content
                // extent instead of the percentage-resolved width.
                if (child.BoxType == BoxType.Table && child.StyledNode is StyledElement childEl
                    && !float.IsNaN(childEl.Style.Width) && childEl.Style.Width < 0)
                {
                    // Measure the nested table's content width from its laid-out line boxes
                    // and children, not from its assigned percentage width.
                    float nestedContentW = MeasureBoxContentWidth(child);
                    if (nestedContentW > 0 && nestedContentW < childWidth)
                        childWidth = nestedContentW;
                }

                float right = child.ContentRect.X + childWidth
                            + child.PaddingRight + child.BorderRightWidth + child.MarginRight;
                if (right > maxRight) maxRight = right;
            }
            if (box.LineBoxes != null)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    var line = box.LineBoxes[i];
                    // Use NaturalContentWidth which captures the pre-centering extent
                    // including inline element padding/border/margin.
                    float lineContentWidth = line.NaturalContentWidth;
                    if (lineContentWidth <= 0 && line.Fragments.Count > 0)
                    {
                        // Fallback: measure from fragment positions
                        float firstFragX = line.Fragments[0].X;
                        float lastFragRight = firstFragX;
                        for (int f = 0; f < line.Fragments.Count; f++)
                        {
                            float fragRight = line.Fragments[f].X + line.Fragments[f].Width;
                            if (fragRight > lastFragRight) lastFragRight = fragRight;
                        }
                        lineContentWidth = lastFragRight - firstFragX;
                    }
                    if (lineContentWidth > maxRight) maxRight = lineContentWidth;
                }
            }

            float result = maxRight + box.PaddingLeft + box.PaddingRight
                 + box.BorderLeftWidth + box.BorderRightWidth;
            return result;
        }

        /// <summary>
        /// Measures the actual content width of a laid-out box by examining its children's
        /// text content and line boxes, rather than using the assigned ContentRect width.
        /// Used for intrinsic sizing of percentage-width children.
        /// </summary>
        private static float MeasureBoxContentWidth(LayoutBox box)
        {
            float maxRight = 0;
            // For tables, look at cell contents within rows
            for (int ri = 0; ri < box.Children.Count; ri++)
            {
                var row = box.Children[ri];
                float rowRight = 0;
                for (int ci = 0; ci < row.Children.Count; ci++)
                {
                    var cell = row.Children[ci];
                    // Measure cell's actual text/content width
                    float cellContentRight = 0;
                    if (cell.LineBoxes != null)
                    {
                        for (int li = 0; li < cell.LineBoxes.Count; li++)
                        {
                            float lcw = cell.LineBoxes[li].NaturalContentWidth;
                            if (lcw > cellContentRight) cellContentRight = lcw;
                        }
                    }
                    for (int cci = 0; cci < cell.Children.Count; cci++)
                    {
                        var cc = cell.Children[cci];
                        float ccRight = cc.ContentRect.X + cc.ContentRect.Width
                                      + cc.PaddingRight + cc.BorderRightWidth
                                      - cell.ContentRect.X;
                        if (ccRight > cellContentRight) cellContentRight = ccRight;
                    }
                    // Add cell's box model
                    float cellTotal = cellContentRight + cell.PaddingLeft + cell.PaddingRight
                                    + cell.BorderLeftWidth + cell.BorderRightWidth;
                    rowRight += cellTotal;
                }
                if (rowRight > maxRight) maxRight = rowRight;
            }
            // Add table's own box model
            return maxRight + box.PaddingLeft + box.PaddingRight
                 + box.BorderLeftWidth + box.BorderRightWidth;
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
            return Math.Max(height, 0);
        }

        private static void OffsetBoxInPlace(LayoutBox box, float dx, float dy)
        {
            box.ContentRect = new RectF(box.ContentRect.X + dx, box.ContentRect.Y + dy,
                                        box.ContentRect.Width, box.ContentRect.Height);
            for (int i = 0; i < box.Children.Count; i++)
                OffsetBoxInPlace(box.Children[i], dx, dy);
            if (box.LineBoxes != null)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    box.LineBoxes[i].X += dx;
                    box.LineBoxes[i].Y += dy;
                }
            }
        }
    }

    internal sealed class TableContext
    {
        public List<TableRow> Rows { get; } = new List<TableRow>();
        public List<StyledElement> Captions { get; } = new List<StyledElement>();

        public int GetColumnCount()
        {
            int max = 0;
            for (int r = 0; r < Rows.Count; r++)
            {
                int count = 0;
                for (int c = 0; c < Rows[r].Cells.Count; c++)
                    count += Rows[r].Cells[c].ColSpan;
                if (count > max) max = count;
            }
            return max;
        }
    }

    internal sealed class TableRow
    {
        public StyledElement? StyledElement { get; set; }
        public List<TableCell> Cells { get; } = new List<TableCell>();
    }

    internal sealed class TableCell
    {
        public StyledElement? StyledElement { get; set; }
        public int ColSpan { get; set; } = 1;
        public int RowSpan { get; set; } = 1;
    }
}
