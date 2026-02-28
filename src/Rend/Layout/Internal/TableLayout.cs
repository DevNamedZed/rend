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

            // Calculate column widths
            float[] colWidths;
            if (isFixed)
                colWidths = CalculateFixedWidths(tableCtx, numCols, containerWidth);
            else
                colWidths = CalculateAutoWidths(tableCtx, numCols, containerWidth, context);

            // Layout cells
            float borderSpacing = style.BorderCollapse == CssBorderCollapse.Collapse ? 0 : 2f;
            float cursorY = parent.ContentRect.Y;

            for (int r = 0; r < tableCtx.Rows.Count; r++)
            {
                var row = tableCtx.Rows[r];
                float rowHeight = 0;
                float cursorX = parent.ContentRect.X;

                // Create row box
                var rowBox = new LayoutBox(row.StyledElement, BoxType.TableRow);

                for (int c = 0; c < row.Cells.Count && c < numCols; c++)
                {
                    var cell = row.Cells[c];
                    if (cell == null) continue;

                    float cellWidth = colWidths[c];
                    // Handle colspan
                    for (int cs = 1; cs < cell.ColSpan && c + cs < numCols; cs++)
                        cellWidth += colWidths[c + cs] + borderSpacing;

                    var cellBox = new LayoutBox(cell.StyledElement, BoxType.TableCell);
                    if (cell.StyledElement != null)
                        BoxModelCalculator.ApplyBoxModel(cellBox, cell.StyledElement.Style, cellWidth);

                    float contentWidth = cellWidth - cellBox.PaddingLeft - cellBox.PaddingRight
                                       - cellBox.BorderLeftWidth - cellBox.BorderRightWidth;
                    contentWidth = Math.Max(0, contentWidth);

                    cellBox.ContentRect = new RectF(
                        cursorX + cellBox.MarginLeft + cellBox.BorderLeftWidth + cellBox.PaddingLeft,
                        cursorY + cellBox.MarginTop + cellBox.BorderTopWidth + cellBox.PaddingTop,
                        contentWidth, 0);

                    // Layout cell contents
                    BlockFormattingContext.Layout(cellBox, context);
                    float cellContentHeight = CalculateAutoHeight(cellBox);
                    cellBox.ContentRect = new RectF(cellBox.ContentRect.X, cellBox.ContentRect.Y,
                                                    contentWidth, cellContentHeight);

                    float totalCellHeight = cellContentHeight + cellBox.PaddingTop + cellBox.PaddingBottom
                                          + cellBox.BorderTopWidth + cellBox.BorderBottomWidth;
                    if (totalCellHeight > rowHeight) rowHeight = totalCellHeight;

                    rowBox.AddChild(cellBox);
                    cursorX += cellWidth + borderSpacing;
                }

                rowBox.ContentRect = new RectF(parent.ContentRect.X, cursorY,
                                               containerWidth, rowHeight);
                parent.AddChild(rowBox);
                cursorY += rowHeight + borderSpacing;
            }
        }

        private static void CollectTableStructure(StyledElement table, TableContext ctx)
        {
            for (int i = 0; i < table.Children.Count; i++)
            {
                var child = table.Children[i];
                if (child.IsText) continue;
                var childEl = (StyledElement)child;
                var display = childEl.Style.Display;

                if (display == CssDisplay.TableRow)
                {
                    ctx.Rows.Add(BuildRow(childEl));
                }
                else if (display == CssDisplay.TableRowGroup ||
                         childEl.TagName == "thead" || childEl.TagName == "tbody" || childEl.TagName == "tfoot")
                {
                    for (int j = 0; j < childEl.Children.Count; j++)
                    {
                        var rowChild = childEl.Children[j];
                        if (!rowChild.IsText)
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
                if (child.IsText) continue;
                var cellEl = (StyledElement)child;
                if (cellEl.Style.Display == CssDisplay.TableCell ||
                    cellEl.TagName == "td" || cellEl.TagName == "th")
                {
                    int colspan = 1;
                    var csVal = cellEl.GetAttribute("colspan");
                    if (csVal != null && int.TryParse(csVal, out int cs) && cs > 0)
                        colspan = cs;

                    row.Cells.Add(new TableCell
                    {
                        StyledElement = cellEl,
                        ColSpan = colspan
                    });
                }
            }
            return row;
        }

        private static float[] CalculateFixedWidths(TableContext ctx, int numCols, float containerWidth)
        {
            float[] widths = new float[numCols];
            float equalWidth = containerWidth / numCols;

            // First row determines widths in fixed layout
            if (ctx.Rows.Count > 0)
            {
                var firstRow = ctx.Rows[0];
                int col = 0;
                for (int c = 0; c < firstRow.Cells.Count && col < numCols; c++)
                {
                    var cell = firstRow.Cells[c];
                    float specWidth = cell.StyledElement?.Style.Width ?? float.NaN;
                    float w = float.IsNaN(specWidth) ? equalWidth : specWidth;
                    for (int s = 0; s < cell.ColSpan && col < numCols; s++)
                    {
                        widths[col] = w / cell.ColSpan;
                        col++;
                    }
                }
                // Fill remaining
                for (; col < numCols; col++)
                    widths[col] = equalWidth;
            }
            else
            {
                for (int i = 0; i < numCols; i++)
                    widths[i] = equalWidth;
            }

            return widths;
        }

        private static float[] CalculateAutoWidths(TableContext ctx, int numCols, float containerWidth,
                                                    LayoutContext context)
        {
            float[] widths = new float[numCols];
            float equalWidth = containerWidth / numCols;
            for (int i = 0; i < numCols; i++)
                widths[i] = equalWidth;
            return widths;
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
    }

    internal sealed class TableContext
    {
        public List<TableRow> Rows { get; } = new List<TableRow>();

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
