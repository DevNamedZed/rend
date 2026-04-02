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
            // Container height may not be resolved yet (BFC sets it to 0 before LayoutChildren).
            // Resolve from explicit CSS height so fr row tracks work correctly.
            if (float.IsNaN(containerHeight) || containerHeight <= 0)
            {
                float explicitH = DimensionResolver.ResolveHeight(style, float.NaN, parent);
                if (!float.IsNaN(explicitH) && explicitH > 0)
                    containerHeight = explicitH;
            }
            if (float.IsNaN(containerHeight) || containerHeight <= 0)
            {
                containerHeight = 0f; // fr rows will resolve to 0; content sizing handles them
            }

            float rowGap = style.RowGap;
            if (DeferredPercent.IsEncoded(rowGap))
            {
                rowGap = DeferredPercent.Resolve(rowGap, containerHeight > 0 ? containerHeight : containerWidth);
            }
            if (float.IsNaN(rowGap) || rowGap < 0) { rowGap = 0; }
            float colGap = style.ColumnGap;
            if (DeferredPercent.IsEncoded(colGap))
            {
                colGap = DeferredPercent.Resolve(colGap, containerWidth);
            }
            if (float.IsNaN(colGap) || colGap < 0) { colGap = 0; }

            // Read grid-template raw values and extract line names early (before item loop).
            var colRaw = style.GetRefValue(PropertyId.GridTemplateColumns);
            var rowRaw = style.GetRefValue(PropertyId.GridTemplateRows);
            var colLineNames = new Dictionary<string, List<int>>();
            var rowLineNames = new Dictionary<string, List<int>>();
            ExtractLineNames(colRaw, colLineNames);
            ExtractLineNames(rowRaw, rowLineNames);

            // Collect grid items with placement info.
            // [CSS-GRID §9] Abspos items with grid placement are collected separately;
            // they must NOT participate in track sizing or create implicit tracks.
            var items = new List<GridItem>();
            var absposGridItems = new List<GridItem>();
            var children = BlockFormattingContext.FlattenContents(styledElement);
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child.IsText)
                {
                    // CSS Grid §4: Text directly inside a grid container is wrapped in
                    // an anonymous grid item. Whitespace-only text is not rendered.
                    var textNode = (StyledText)child;
                    if (string.IsNullOrWhiteSpace(textNode.Text)) continue;

                    var blockStyle = CloneStyleAsBlock(styledElement.Style);
                    var doc = styledElement.Element.OwnerDocument;
                    var anonElement = doc!.CreateElement("div");
                    var anonChildren = new List<StyledNode> { new StyledText(textNode.Text, blockStyle) };
                    var anonStyled = new StyledElement(anonElement, blockStyle, anonChildren);

                    var textBox = new LayoutBox(anonStyled, BoxType.Block);
                    textBox.ContentRect = new RectF(0, 0, parent.ContentRect.Width, 0);
                    var savedFloatCtx = context.FloatContext;
                    context.FloatContext = null;
                    InlineFormattingContext.Layout(textBox, context);
                    context.FloatContext = savedFloatCtx;
                    float textHeight = 0;
                    if (textBox.LineBoxes != null && textBox.LineBoxes.Count > 0)
                    {
                        var lastLine = textBox.LineBoxes[textBox.LineBoxes.Count - 1];
                        textHeight = lastLine.Y + lastLine.Height - textBox.ContentRect.Y;
                    }
                    textBox.ContentRect = new RectF(0, 0, parent.ContentRect.Width, textHeight);
                    items.Add(new GridItem { Box = textBox });
                    continue;
                }

                if (child is StyledPseudoElement pseudo)
                {
                    var pseudoText = new StyledText(pseudo.Content, pseudo.Style);
                    var pseudoBox = new LayoutText(pseudoText);
                    float fontSize = pseudo.Style.FontSize;
                    float lineHeight = pseudo.Style.LineHeight;
                    if (lineHeight < 0)
                        lineHeight = -lineHeight * fontSize;
                    else if (float.IsNaN(lineHeight) || lineHeight == 0)
                        lineHeight = fontSize * 1.2f;
                    float measuredWidth;
                    if (context.TextMeasurer != null)
                    {
                        var fontDesc = new Fonts.FontDescriptor(pseudo.Style.FontFamilies,
                            pseudo.Style.FontWeight, pseudo.Style.FontStyle);
                        var shaped = context.TextMeasurer.Shape(pseudo.Content, fontDesc, fontSize);
                        measuredWidth = shaped.TotalWidth;
                    }
                    else
                    {
                        measuredWidth = pseudo.Content.Length * fontSize * 0.6f;
                    }
                    pseudoBox.ContentRect = new RectF(0, 0, measuredWidth, lineHeight);
                    items.Add(new GridItem { Box = pseudoBox });
                    continue;
                }

                var childEl = (StyledElement)child;
                if (childEl.Style.Display == CssDisplay.None) continue;

                // Absolutely/fixed positioned items are out of flow
                if (childEl.Style.Position == CssPosition.Absolute ||
                    childEl.Style.Position == CssPosition.Fixed)
                {
                    // [CSS-GRID §9] Check if this abspos item has grid placement.
                    // If so, defer until after track sizing so we can use grid area
                    // as the containing block. Abspos items must NOT participate in
                    // track sizing or create implicit tracks.
                    var absposItem = new GridItem
                    {
                        StyledElement = childEl,
                        Box = new LayoutBox(childEl, BoxType.Block),
                        Order = childEl.Style.Order,
                        OriginalIndex = items.Count
                    };
                    ParsePlacement(childEl.Style, absposItem);

                    if (absposItem.HasGridPlacement &&
                        childEl.Style.Position == CssPosition.Absolute)
                    {
                        // Deferred: will be positioned after track sizing completes.
                        absposGridItems.Add(absposItem);
                        continue;
                    }

                    // No grid placement (or fixed position): use grid padding box
                    // as containing block (current behavior).
                    bool isFixed = childEl.Style.Position == CssPosition.Fixed;
                    float cbWidth = isFixed ? context.ViewportWidth : parent.ContentRect.Width;
                    float cbHeight;
                    if (isFixed)
                    {
                        cbHeight = context.ViewportHeight;
                    }
                    else
                    {
                        cbHeight = containerHeight;
                        if (cbHeight <= 0)
                        {
                            cbHeight = DimensionResolver.ResolveHeight(style, float.NaN, parent);
                            if (float.IsNaN(cbHeight) || cbHeight <= 0)
                            {
                                cbHeight = 0;
                            }
                        }
                    }

                    var posBox = absposItem.Box;
                    BoxModelCalculator.ApplyBoxModel(posBox, childEl.Style, cbWidth);
                    float posWidth;
                    bool widthIsAutoAbspos = float.IsNaN(childEl.Style.Width);
                    if (widthIsAutoAbspos)
                    {
                        // [CSS2 §10.3.7] Auto width on abspos → shrink-to-fit
                        posBox.ContentRect = new RectF(0, 0, cbWidth, 0);
                        BlockFormattingContext.LayoutChildren(posBox, context);
                        posWidth = BlockFormattingContext.GetContentExtent(posBox);
                        if (posWidth > cbWidth)
                        {
                            posWidth = cbWidth;
                        }
                        posBox.ClearChildren();
                    }
                    else
                    {
                        posWidth = DimensionResolver.ResolveWidth(childEl.Style, cbWidth, posBox);
                    }

                    posBox.ContentRect = new RectF(parent.ContentRect.X, parent.ContentRect.Y, posWidth, 0);
                    BlockFormattingContext.LayoutChildren(posBox, context);
                    float posHeight = DimensionResolver.ResolveHeight(childEl.Style, cbHeight, posBox);
                    if (float.IsNaN(posHeight))
                    {
                        posHeight = BlockFormattingContext.CalculateAutoHeight(posBox);
                    }

                    // [CSS-ALIGN §6.5] Static position of abspos in grid respects alignment.
                    float absStaticX = parent.ContentRect.X;
                    float absStaticY = parent.ContentRect.Y;
                    if (!isFixed)
                    {
                        float absOuterW = posWidth + posBox.PaddingLeft + posBox.PaddingRight
                            + posBox.BorderLeftWidth + posBox.BorderRightWidth
                            + posBox.MarginLeft + posBox.MarginRight;
                        float absOuterH = posHeight + posBox.PaddingTop + posBox.PaddingBottom
                            + posBox.BorderTopWidth + posBox.BorderBottomWidth
                            + posBox.MarginTop + posBox.MarginBottom;

                        CssAlignItems absAlignSelf = childEl.Style.AlignSelf;
                        if (absAlignSelf == CssAlignItems.Normal || (int)absAlignSelf > (int)CssAlignItems.Normal)
                        {
                            absAlignSelf = style.AlignItems;
                        }
                        float absFreeV = cbHeight - absOuterH;
                        if (absFreeV > 0)
                        {
                            if (absAlignSelf == CssAlignItems.Center)
                            {
                                absStaticY += absFreeV / 2f;
                            }
                            else if (absAlignSelf == CssAlignItems.End || absAlignSelf == CssAlignItems.FlexEnd)
                            {
                                absStaticY += absFreeV;
                            }
                        }

                        CssAlignItems absJustifySelf = childEl.Style.JustifySelf;
                        if (absJustifySelf == CssAlignItems.Normal || (int)absJustifySelf > (int)CssAlignItems.Normal)
                        {
                            absJustifySelf = style.JustifyItems;
                        }
                        float absFreeH = cbWidth - absOuterW;
                        if (absFreeH > 0)
                        {
                            if (absJustifySelf == CssAlignItems.Center)
                            {
                                absStaticX += absFreeH / 2f;
                            }
                            else if (absJustifySelf == CssAlignItems.End || absJustifySelf == CssAlignItems.FlexEnd)
                            {
                                absStaticX += absFreeH;
                            }
                        }
                    }

                    posBox.ContentRect = new RectF(absStaticX, absStaticY, posWidth, posHeight);
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
                ParsePlacement(childEl.Style, item, colLineNames, rowLineNames);
                items.Add(item);
            }

            if (items.Count == 0 && absposGridItems.Count == 0)
            {
                return;
            }

            // Sort by CSS order (stable: use original index as tiebreaker)
            items.Sort((a, b) =>
            {
                int cmp = a.Order.CompareTo(b.Order);
                return cmp != 0 ? cmp : a.OriginalIndex.CompareTo(b.OriginalIndex);
            });

            // Detect CSS Subgrid: if grid-template-columns or grid-template-rows is "subgrid",
            // inherit track sizes from the parent grid for the lines this item spans.
            bool isSubgridCols = IsSubgrid(colRaw);
            bool isSubgridRows = IsSubgrid(rowRaw);

            float[]? subgridColTracks = null;
            float[]? subgridRowTracks = null;

            var parentGridCtx = context.ParentGridContext;
            if (isSubgridCols && parentGridCtx != null)
            {
                // [CSS-GRID-2 §8.1] Subgrid inherits parent gap when its own gap is 'normal'.
                // An explicit gap: 0 is different from the initial 'normal' value.
                float parentColGap = parentGridCtx.ColumnGap;
                if (!style.IsColumnGapExplicit)
                {
                    colGap = parentColGap;
                }
                subgridColTracks = GetSubgridTracks(
                    parentGridCtx.ColumnWidths, parentGridCtx.ItemColStart, parentGridCtx.ItemColSpan,
                    parentColGap, colGap);
            }
            if (isSubgridRows && parentGridCtx != null)
            {
                float parentRowGap = parentGridCtx.RowGap;
                if (!style.IsRowGapExplicit)
                {
                    rowGap = parentRowGap;
                }
                subgridRowTracks = GetSubgridTracks(
                    parentGridCtx.RowHeights, parentGridCtx.ItemRowStart, parentGridCtx.ItemRowSpan,
                    parentRowGap, rowGap);
            }

            // Resolve explicit tracks (use subgrid tracks if the axis is subgridded)
            var explicitColTracks = isSubgridCols && subgridColTracks != null
                ? subgridColTracks
                : ResolveTrackList(colRaw, containerWidth, colGap, colLineNames);
            var explicitRowTracks = isSubgridRows && subgridRowTracks != null
                ? subgridRowTracks
                : ResolveTrackList(rowRaw, containerHeight, rowGap, rowLineNames);

            int explicitCols = explicitColTracks?.Length ?? 0;
            int explicitRows = explicitRowTracks?.Length ?? 0;

            // Detect auto-fit for column collapsing after placement
            bool isAutoFitCols = HasAutoFit(colRaw);

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

                // [CSS-GRID §9] Also resolve named areas for deferred abspos items.
                for (int i = 0; i < absposGridItems.Count; i++)
                {
                    var item = absposGridItems[i];
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

                // Resolve negative end lines into spans
                if (item.RawColEnd != 0)
                {
                    int resolvedEnd = Math.Max(0, ResolveNegativeLine(item.RawColEnd, gridCols));
                    int start = item.ColStart >= 0 ? item.ColStart : 0;
                    if (resolvedEnd > start)
                    {
                        item.ColSpan = resolvedEnd - start;
                    }
                }
                if (item.RawRowEnd != 0)
                {
                    int resolvedEnd = Math.Max(0, ResolveNegativeLine(item.RawRowEnd, gridRows));
                    int start = item.RowStart >= 0 ? item.RowStart : 0;
                    if (resolvedEnd > start)
                    {
                        item.RowSpan = resolvedEnd - start;
                    }
                }
            }

            // [CSS-GRID §9] Resolve negative line numbers for abspos items.
            // Use explicit grid dimensions (abspos items don't create implicit tracks).
            // If a resolved line falls outside the explicit grid (< 0 after resolution),
            // it is treated as auto (padding edge) per CSS Grid §9.
            for (int i = 0; i < absposGridItems.Count; i++)
            {
                var item = absposGridItems[i];
                int absposGridRowCount = Math.Max(1, explicitRows);
                int absposGridColCount = Math.Max(1, explicitCols);

                if (item.RowStart < -1)
                {
                    int resolved = ResolveNegativeLine(item.RowStart, absposGridRowCount);
                    item.RowStart = resolved >= 0 ? resolved : -1;
                }
                if (item.ColStart < -1)
                {
                    int resolved = ResolveNegativeLine(item.ColStart, absposGridColCount);
                    item.ColStart = resolved >= 0 ? resolved : -1;
                }

                if (item.RawColEnd != 0)
                {
                    int resolvedEnd = ResolveNegativeLine(item.RawColEnd, absposGridColCount);
                    if (resolvedEnd >= 0)
                    {
                        int start = item.ColStart >= 0 ? item.ColStart : 0;
                        if (resolvedEnd > start)
                        {
                            item.ColSpan = resolvedEnd - start;
                        }
                    }
                    // If resolvedEnd < 0, the line is out of range → end becomes auto.
                    // RawColEnd stays set but will be ignored since resolved is invalid.
                }
                if (item.RawRowEnd != 0)
                {
                    int resolvedEnd = ResolveNegativeLine(item.RawRowEnd, absposGridRowCount);
                    if (resolvedEnd >= 0)
                    {
                        int start = item.RowStart >= 0 ? item.RowStart : 0;
                        if (resolvedEnd > start)
                        {
                            item.RowSpan = resolvedEnd - start;
                        }
                    }
                }

                // Also resolve explicit end lines stored when start is auto
                if (item.ExplicitColEnd < -1)
                {
                    int resolved = ResolveNegativeLine(item.ExplicitColEnd, absposGridColCount);
                    item.ExplicitColEnd = resolved >= 0 ? resolved : -1;
                }
                if (item.ExplicitRowEnd < -1)
                {
                    int resolved = ResolveNegativeLine(item.ExplicitRowEnd, absposGridRowCount);
                    item.ExplicitRowEnd = resolved >= 0 ? resolved : -1;
                }
            }

            // If no explicit columns, determine from item count
            if (explicitCols == 0 && !HasAnyExplicitPlacement(items))
            {
                if (explicitRows > 0)
                {
                    // Explicit rows but no explicit columns: default to 1 column,
                    // items fill rows sequentially (standard CSS Grid behavior).
                    gridRows = explicitRows;
                    gridCols = Math.Max(1, (int)Math.Ceiling((float)items.Count / gridRows));
                }
                else if (flowColumn)
                {
                    // Column flow: items fill columns vertically, then create new columns.
                    // Without explicit rows, each item gets its own column.
                    gridCols = items.Count;
                    gridRows = 1;
                }
                else
                {
                    // [CSS-GRID §7.1] With no explicit template and row auto-flow,
                    // default to 1 column — items stack vertically in auto rows.
                    gridCols = 1;
                    gridRows = items.Count;
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
                    int col = FindFreeColumn(occupied, maxCol, item.RowStart, item.ColSpan, item.RowSpan, 0);
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

            // Phase 3+4: Place items with definite column only AND fully auto items
            // in source order (CSS Grid Level 1 §8.5).
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

                if (item.ColStart >= 0)
                {
                    // Definite column, auto row: find first free row in that column
                    // Per spec, cursor row is used as starting point (not always 0)
                    int searchRow = dense ? 0 : autoRow;
                    EnsureGridSize(ref occupied, ref maxRow, ref maxCol,
                        maxRow, item.ColStart + item.ColSpan);
                    int row = FindFreeRow(occupied, maxCol, maxRow, item.ColStart, item.RowSpan, item.ColSpan, searchRow);
                    if (row < 0)
                    {
                        row = maxRow;
                        EnsureGridSize(ref occupied, ref maxRow, ref maxCol,
                            row + item.RowSpan, maxCol);
                    }
                    item.RowStart = row;
                    MarkOccupied(occupied, maxCol, item.RowStart, item.ColStart, item.RowSpan, item.ColSpan);
                    item.Placed = true;
                    autoRow = item.RowStart;
                    autoCol = item.ColStart;
                }
                else if (flowColumn)
                {
                    // Column-major auto-placement
                    // BUG-063: Clamp rowLimit to prevent near-infinite loop when RowSpan > gridRows
                    int rowLimit = Math.Max(1, gridRows - item.RowSpan + 1);
                    for (int c = autoCol; !found; c++)
                    {
                        int startRow = (c == autoCol) ? autoRow : 0;
                        for (int r = startRow; r < rowLimit; r++)
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
                    // BUG-063: Clamp colLimit to prevent near-infinite loop when ColSpan > gridCols
                    int colLimit = Math.Max(1, gridCols - item.ColSpan + 1);
                    for (int r = autoRow; !found; r++)
                    {
                        int startCol = (r == autoRow) ? autoCol : 0;
                        for (int c = startCol; c < colLimit; c++)
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

            // [CSS-GRID-1 §7.2.3.1] auto-fit: collapse empty explicit tracks to 0
            // and redistribute freed space to occupied tracks via fr resolution.
            float[]? autoFitColTracks = explicitColTracks;
            if (isAutoFitCols && explicitColTracks != null && explicitColTracks.Length > 0)
            {
                var occupiedCols = new bool[explicitColTracks.Length];
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item.ColStart >= 0)
                    {
                        for (int cs = 0; cs < item.ColSpan; cs++)
                        {
                            int colIdx = item.ColStart + cs;
                            if (colIdx < occupiedCols.Length)
                            {
                                occupiedCols[colIdx] = true;
                            }
                        }
                    }
                }

                // Collapse empty tracks: set to 0 and count freed space
                autoFitColTracks = new float[explicitColTracks.Length];
                int emptyCount = 0;
                for (int c = 0; c < explicitColTracks.Length; c++)
                {
                    if (occupiedCols[c])
                    {
                        autoFitColTracks[c] = explicitColTracks[c];
                    }
                    else
                    {
                        autoFitColTracks[c] = 0;
                        emptyCount++;
                    }
                }

                // Redistribute freed space only if tracks have flexible (fr) max.
                // Fixed tracks (e.g., repeat(auto-fit, 100px)) keep their size.
                if (emptyCount > 0 && HasAutoFitFr(colRaw))
                {
                    int occupiedCount = explicitColTracks.Length - emptyCount;
                    if (occupiedCount > 0)
                    {
                        float totalGaps = (occupiedCount - 1) * colGap;
                        float availableForTracks = containerWidth - totalGaps;
                        float perTrack = availableForTracks / occupiedCount;
                        for (int c = 0; c < autoFitColTracks.Length; c++)
                        {
                            if (occupiedCols[c])
                            {
                                autoFitColTracks[c] = perTrack;
                            }
                        }
                    }
                }
            }

            // For subgridded axes, use the inherited tracks directly and do not run BuildTrackSizes
            // which would redistribute implicit space. The subgrid columns/rows are fixed from the parent.
            float[] colWidths;
            if (isSubgridCols && subgridColTracks != null && subgridColTracks.Length >= finalCols)
            {
                colWidths = new float[finalCols];
                Array.Copy(subgridColTracks, colWidths, finalCols);
            }
            else
            {
                colWidths = BuildTrackSizes(autoFitColTracks, finalCols, containerWidth,
                    colGap, style.GetRefValue(PropertyId.GridAutoColumns), containerWidth);
            }
            float[] rowHeights = new float[finalRows];

            // [CSS-GRID §7.2.4.1] Extract fit-content limits from raw track definitions.
            // fit-content(limit) = minmax(auto, min(max-content, limit))
            float[]? fitContentColLimits = ExtractFitContentLimits(colRaw, finalCols, containerWidth, colGap);

            // Resolve intrinsic (min-content / max-content / fit-content) column tracks by measuring items.
            // Sentinel values: -1 = min-content, -2 = max-content, -3 = fit-content.
            bool hasIntrinsicCols = false;
            for (int c = 0; c < finalCols; c++)
            {
                if (colWidths[c] < 0) { hasIntrinsicCols = true; break; }
            }
            if (hasIntrinsicCols)
            {
                float[] intrinsicWidths = new float[finalCols];
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item.ColSpan != 1 || item.ColStart < 0 || item.ColStart >= finalCols)
                        continue;
                    if (colWidths[item.ColStart] >= 0)
                        continue; // not an intrinsic track

                    if (item.StyledElement == null) continue;

                    // fit-content uses max-content measurement (clamped later)
                    bool isMinContent = colWidths[item.ColStart] == -1;
                    float keyword = isMinContent ? SizingKeyword.MinContent : SizingKeyword.MaxContent;
                    float measured = BlockFormattingContext.MeasureIntrinsicWidth(
                        item.StyledElement, keyword, containerWidth, context);
                    // Add horizontal box model spacing
                    var tempBox = new LayoutBox(item.StyledElement, BoxType.Block);
                    BoxModelCalculator.ApplyBoxModel(tempBox, item.StyledElement.Style, containerWidth);
                    measured += tempBox.PaddingLeft + tempBox.PaddingRight
                              + tempBox.BorderLeftWidth + tempBox.BorderRightWidth
                              + tempBox.MarginLeft + tempBox.MarginRight;
                    if (measured > intrinsicWidths[item.ColStart])
                        intrinsicWidths[item.ColStart] = measured;
                }

                // Replace intrinsic sentinels with measured widths
                for (int c = 0; c < finalCols; c++)
                {
                    if (colWidths[c] >= -3.5f && colWidths[c] < 0)
                    {
                        float measured = intrinsicWidths[c];
                        // [CSS-GRID §7.2.4.1] fit-content clamp: min(max-content, limit)
                        if (colWidths[c] <= -2.5f && colWidths[c] > -3.5f
                            && fitContentColLimits != null && c < fitContentColLimits.Length
                            && fitContentColLimits[c] >= 0)
                        {
                            measured = Math.Min(measured, fitContentColLimits[c]);
                        }
                        colWidths[c] = measured;
                    }
                }
            }

            // Resolve deferred fr tracks now that intrinsic sizes are known.
            // Fr sentinels are encoded as -(1000 + frValue).
            {
                float totalFr = 0;
                float totalNonFr = 0;
                bool hasFrSentinel = false;
                for (int c = 0; c < finalCols; c++)
                {
                    if (colWidths[c] <= -999f)
                    {
                        totalFr += -(colWidths[c] + 1000f);
                        hasFrSentinel = true;
                    }
                    else
                    {
                        totalNonFr += colWidths[c];
                    }
                }
                if (hasFrSentinel && totalFr > 0)
                {
                    float gapSpace = finalCols > 1 ? (finalCols - 1) * colGap : 0;
                    float remaining = Math.Max(0, containerWidth - totalNonFr - gapSpace);
                    // Use LayoutUnit integer arithmetic for fr distribution (matching Chrome)
                    int remainingRaw = (int)(remaining * 64f);
                    int totalFrI = (int)totalFr;
                    int frSizeRaw = totalFrI > 0 ? remainingRaw / totalFrI : 0;
                    int frRem = totalFrI > 0 ? remainingRaw % totalFrI : 0;
                    int frIdx = 0;
                    for (int c = 0; c < finalCols; c++)
                    {
                        if (colWidths[c] <= -999f)
                        {
                            float frVal = -(colWidths[c] + 1000f);
                            int trackFrI = (int)frVal;
                            if (trackFrI > 0 && totalFrI > 0)
                            {
                                int trackRaw = frSizeRaw * trackFrI;
                                if (frIdx < frRem) trackRaw += 1;
                                colWidths[c] = trackRaw / 64f;
                            }
                            else
                            {
                                colWidths[c] = frVal * (remaining / totalFr);
                            }
                            frIdx++;
                        }
                    }
                }
            }

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

                    // [CSS-SIZING-4 §5.1] When aspect-ratio is set, both axes are auto,
                    // and stretch applies in the block axis with a definite row height,
                    // derive width from the row height × ratio instead of column width.
                    float contentWidth = float.NaN;
                    float itemAr = DimensionResolver.GetAspectRatio(item.StyledElement.Style);
                    bool widthAutoForGrid = float.IsNaN(item.StyledElement.Style.Width);
                    bool heightAutoForGrid = float.IsNaN(item.StyledElement.Style.Height);
                    if (itemAr > 0 && widthAutoForGrid && heightAutoForGrid && explicitRowTracks != null)
                    {
                        CssAlignItems itemAlignBlock = style.AlignItems;
                        CssAlignItems selfBlock = item.StyledElement.Style.AlignSelf;
                        if (selfBlock != CssAlignItems.Normal && (int)selfBlock <= (int)CssAlignItems.Normal)
                        {
                            itemAlignBlock = selfBlock;
                        }
                        if (IsStretch(itemAlignBlock))
                        {
                            float rowH = 0;
                            for (int r = item.RowStart; r < item.RowStart + item.RowSpan && r < explicitRowTracks.Length; r++)
                            {
                                rowH += explicitRowTracks[r];
                            }
                            if (rowH > 0)
                            {
                                float stretchedHeight = rowH
                                    - item.Box.PaddingTop - item.Box.PaddingBottom
                                    - item.Box.BorderTopWidth - item.Box.BorderBottomWidth
                                    - item.Box.MarginTop - item.Box.MarginBottom;
                                if (stretchedHeight > 0)
                                {
                                    contentWidth = stretchedHeight * itemAr;
                                }
                            }
                        }
                    }

                    if (float.IsNaN(contentWidth))
                    {
                        if (SizingKeyword.IsSizingKeyword(item.StyledElement.Style.Width))
                        {
                            contentWidth = BlockFormattingContext.MeasureIntrinsicWidth(
                                item.StyledElement, item.StyledElement.Style.Width, cellWidth, context);

                            // [CSS-SIZING-4 §5.2] Transfer max-height → max-width for grid items
                            float itemArRatio = DimensionResolver.GetAspectRatio(item.StyledElement.Style);
                            if (itemArRatio > 0 && float.IsNaN(item.StyledElement.Style.Height))
                            {
                                float itemMaxH = DimensionResolver.ResolvePercentHeight(
                                    item.StyledElement.Style.MaxHeight, 0);
                                if (!float.IsNaN(itemMaxH) && itemMaxH >= 0 && contentWidth > itemMaxH * itemArRatio)
                                {
                                    contentWidth = itemMaxH * itemArRatio;
                                }
                            }
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
                    }
                    contentWidth = Math.Max(0, contentWidth);

                    // Pre-set height from explicit row track ONLY when the item has
                    // a percentage height that needs a definite containing block.
                    float preHeight = 0;
                    bool hasPercentHeight = DeferredPercent.IsEncoded(item.StyledElement!.Style.Height)
                        || DeferredPercent.IsEncoded(item.StyledElement.Style.MinHeight);
                    if (hasPercentHeight && explicitRowTracks != null)
                    {
                        for (int r = item.RowStart; r < item.RowStart + item.RowSpan && r < explicitRowTracks.Length; r++)
                        {
                            preHeight += explicitRowTracks[r];
                        }
                        if (preHeight > 0)
                        {
                            preHeight -= item.Box.PaddingTop + item.Box.PaddingBottom
                                       + item.Box.BorderTopWidth + item.Box.BorderBottomWidth;
                            if (preHeight < 0) { preHeight = 0; }
                        }
                    }
                    item.Box.ContentRect = new RectF(0, 0, contentWidth, preHeight);

                    // Set parent reference before layout so margin collapsing
                    // can detect that this box is a grid item (establishes BFC).
                    item.Box.Parent = parent;

                    // Grid items establish their own BFC (CSS Grid §6). Isolate the
                    // float context so child IFC doesn't pick up the parent's floats
                    // and misalign line boxes.
                    var savedFloatCtx = context.FloatContext;
                    context.FloatContext = new FloatContext(
                        item.Box.ContentRect.X, item.Box.ContentRect.Width);

                    // Propagate parent grid context so nested subgrids can inherit tracks.
                    // Save and restore to avoid leaking context to sibling items.
                    var savedParentGridCtx = context.ParentGridContext;
                    context.ParentGridContext = new ParentGridContext
                    {
                        ColumnWidths = colWidths,
                        RowHeights = rowHeights, // will be partially filled; subgrid rows uses this
                        ColumnGap = colGap,
                        RowGap = rowGap,
                        ItemColStart = item.ColStart,
                        ItemColSpan = item.ColSpan,
                        ItemRowStart = item.RowStart,
                        ItemRowSpan = item.RowSpan
                    };
                    BlockFormattingContext.LayoutChildren(item.Box, context);
                    context.ParentGridContext = savedParentGridCtx;
                    context.FloatContext = savedFloatCtx;

                    // Use explicit row track height as containing block for percentage height resolution
                    float rowTrackHeight = preHeight > 0 ? preHeight : float.NaN;
                    float contentHeight = DimensionResolver.ResolveHeight(item.StyledElement.Style, rowTrackHeight, item.Box);
                    if (float.IsNaN(contentHeight))
                        contentHeight = CalculateAutoHeight(item.Box);

                    // Apply min-height / max-height (same as BlockFormattingContext)
                    float gridMinH = DimensionResolver.ResolvePercentHeight(item.StyledElement.Style.MinHeight, containerHeight);
                    float gridMaxH = DimensionResolver.ResolvePercentHeight(item.StyledElement.Style.MaxHeight, containerHeight);
                    // box-sizing: border-box → min/max-height includes padding+border,
                    // but contentHeight is content-box only, so subtract padding+border.
                    if (item.StyledElement.Style.BoxSizing == CssBoxSizing.BorderBox)
                    {
                        float vExtra = item.Box.PaddingTop + item.Box.PaddingBottom
                                     + item.Box.BorderTopWidth + item.Box.BorderBottomWidth;
                        if (!float.IsNaN(gridMinH) && gridMinH >= 0)
                        {
                            gridMinH = Math.Max(0, gridMinH - vExtra);
                        }
                        if (!float.IsNaN(gridMaxH) && gridMaxH >= 0)
                        {
                            gridMaxH = Math.Max(0, gridMaxH - vExtra);
                        }
                    }
                    if (!float.IsNaN(gridMaxH) && gridMaxH >= 0 && contentHeight > gridMaxH)
                        contentHeight = gridMaxH;
                    if (!float.IsNaN(gridMinH) && gridMinH >= 0 && contentHeight < gridMinH)
                        contentHeight = gridMinH;

                    // If the final content height differs from what was used during layout
                    // (initially 0), flex containers need re-layout so cross-axis alignment
                    // (align-items: center/flex-end) works with the actual height.
                    float layoutHeight = item.Box.ContentRect.Height;
                    if (Math.Abs(contentHeight - layoutHeight) > 0.01f)
                    {
                        var itemDisplay = item.StyledElement.Style.Display;
                        if (itemDisplay == CssDisplay.Flex || itemDisplay == CssDisplay.InlineFlex)
                        {
                            item.Box.ClearChildren();
                            item.Box.LineBoxes = null;
                            item.Box.ContentRect = new RectF(0, 0, contentWidth, contentHeight);
                            var savedFloatCtx2 = context.FloatContext;
                            context.FloatContext = new FloatContext(0, contentWidth);
                            BlockFormattingContext.LayoutChildren(item.Box, context);
                            context.FloatContext = savedFloatCtx2;
                        }
                    }

                    item.ContentWidth = contentWidth;
                    item.ContentHeight = contentHeight;

                    // visibility: collapse → zero contribution to row height
                    if (item.StyledElement.Style.Visibility == CssVisibility.Collapse)
                    {
                        item.ContentHeight = 0;
                        item.ContentWidth = 0;
                    }
                }

            }

            // CSS Grid §11.5: Size rows in two passes — non-spanning first, then spanning.
            // Pass 1: Set row heights from non-spanning items
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.RowSpan != 1)
                {
                    continue;
                }

                float totalHeight = item.ContentHeight;
                if (item.StyledElement != null)
                {
                    totalHeight += item.Box.PaddingTop + item.Box.PaddingBottom
                                 + item.Box.BorderTopWidth + item.Box.BorderBottomWidth
                                 + item.Box.MarginTop + item.Box.MarginBottom;
                }

                int r = item.RowStart;
                if (r < finalRows && totalHeight > rowHeights[r])
                {
                    rowHeights[r] = totalHeight;
                }
            }

            // Pass 2: Distribute extra space for spanning items
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.RowSpan <= 1)
                {
                    continue;
                }

                float totalHeight = item.ContentHeight;
                if (item.StyledElement != null)
                {
                    totalHeight += item.Box.PaddingTop + item.Box.PaddingBottom
                                 + item.Box.BorderTopWidth + item.Box.BorderBottomWidth
                                 + item.Box.MarginTop + item.Box.MarginBottom;
                }

                // Only distribute extra space beyond what existing rows + gaps already provide
                float existing = (item.RowSpan - 1) * rowGap;
                for (int r = item.RowStart; r < item.RowStart + item.RowSpan && r < finalRows; r++)
                {
                    existing += rowHeights[r];
                }
                if (totalHeight > existing)
                {
                    float extra = totalHeight - existing;
                    float perRow = extra / item.RowSpan;
                    for (int r = item.RowStart; r < item.RowStart + item.RowSpan && r < finalRows; r++)
                    {
                        rowHeights[r] += perRow;
                    }
                }
            }

            // [CSS-GRID §11.5] Apply explicit row heights (or subgrid row heights).
            // Fixed track sizes (e.g. "25px") set the exact row height — content overflows.
            // Intrinsic sentinels (negative values) keep the content-sized height from above.
            if (isSubgridRows && subgridRowTracks != null)
            {
                // Subgridded rows: use the parent's row sizes directly
                for (int r = 0; r < Math.Min(subgridRowTracks.Length, finalRows); r++)
                    rowHeights[r] = subgridRowTracks[r];
            }
            else if (explicitRowTracks != null)
            {
                for (int r = 0; r < Math.Min(explicitRowTracks.Length, finalRows); r++)
                {
                    if (explicitRowTracks[r] > 0)
                    {
                        // Fixed track: exact size per CSS Grid spec
                        rowHeights[r] = explicitRowTracks[r];
                    }
                }
            }

            // Apply explicit row track minimums from minmax(min, auto) or fixed tracks
            if (explicitRowTracks != null)
            {
                for (int r = 0; r < Math.Min(finalRows, explicitRowTracks.Length); r++)
                {
                    if (explicitRowTracks[r] > rowHeights[r])
                    {
                        rowHeights[r] = explicitRowTracks[r];
                    }
                }
            }

            // [CSS-GRID §7.2.4.1] Apply fit-content row limits — cap content height at limit.
            float[]? fitContentRowLimits = ExtractFitContentLimits(
                rowRaw, finalRows, containerHeight, rowGap);
            if (fitContentRowLimits != null)
            {
                for (int r = 0; r < finalRows; r++)
                {
                    if (fitContentRowLimits[r] >= 0 && rowHeights[r] > fitContentRowLimits[r])
                    {
                        rowHeights[r] = fitContentRowLimits[r];
                    }
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

            // Distribute extra container height to row tracks when the grid has
            // an explicit height and align-content is stretch (default) or normal.
            // For other align-content values (center, end, etc.), the tracks keep their
            // natural size and are offset instead.
            if (containerHeight > 0 && finalRows > 0)
            {
                var alignContent = style.AlignContent;
                bool stretchRows = alignContent == CssAlignItems.Stretch
                                || alignContent == CssAlignItems.Normal;
                if (stretchRows)
                {
                    float totalRowH = 0;
                    for (int r = 0; r < finalRows; r++)
                    {
                        totalRowH += rowHeights[r];
                    }
                    totalRowH += (finalRows - 1) * rowGap;

                    float extraH = containerHeight - totalRowH;
                    if (extraH > 1f)
                    {
                        // [CSS-GRID §10.4] align-content: normal stretches only
                        // auto-sized rows. Explicit 'stretch' stretches all rows.
                        bool stretchAll = alignContent == CssAlignItems.Stretch;
                        if (stretchAll)
                        {
                            float perRow = extraH / finalRows;
                            for (int r = 0; r < finalRows; r++)
                            {
                                rowHeights[r] += perRow;
                            }
                        }
                        else
                        {
                            int autoRowCount = 0;
                            for (int r = 0; r < finalRows; r++)
                            {
                                bool isExplicitRow = explicitRowTracks != null
                                    && r < explicitRowTracks.Length
                                    && explicitRowTracks[r] > 0;
                                if (!isExplicitRow)
                                {
                                    autoRowCount++;
                                }
                            }
                            if (autoRowCount > 0)
                            {
                                float perRow = extraH / autoRowCount;
                                for (int r = 0; r < finalRows; r++)
                                {
                                    bool isExplicitRow = explicitRowTracks != null
                                        && r < explicitRowTracks.Length
                                        && explicitRowTracks[r] > 0;
                                    if (!isExplicitRow)
                                    {
                                        rowHeights[r] += perRow;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Read container-level alignment defaults
            CssAlignItems containerAlignItems = style.AlignItems;
            CssAlignItems containerJustifyItems = style.JustifyItems;

            // Update item box dimensions to final resolved sizes before baseline
            // computation. First-pass layout leaves ContentRect at auto height;
            // the resolved contentWidth/contentHeight may differ (explicit CSS height).
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Box.ContentRect = new RectF(0, 0, items[i].ContentWidth, items[i].ContentHeight);
            }

            // [CSS-GRID §10.1] Compute per-row baseline groups for baseline alignment.
            // Items with align-self:baseline in the same row share a baseline group.
            // The row height may grow to accommodate baseline-shifted items.
            float[]? rowMaxBaselines = null;
            {
                bool hasBaselineAlignment = containerAlignItems == CssAlignItems.Baseline;
                if (!hasBaselineAlignment)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        var styledEl = items[i].StyledElement;
                        if (styledEl != null
                            && styledEl.Style.AlignSelf == CssAlignItems.Baseline)
                        {
                            hasBaselineAlignment = true;
                            break;
                        }
                    }
                }

                if (hasBaselineAlignment)
                {
                    rowMaxBaselines = new float[finalRows];
                    float[] rowMaxDescents = new float[finalRows];

                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        if (item.RowSpan != 1) { continue; }
                        int row = item.RowStart;
                        if (row < 0 || row >= finalRows) { continue; }

                        CssAlignItems itemAlign = ResolveItemBlockAlignment(item, containerAlignItems);
                        if (itemAlign != CssAlignItems.Baseline) { continue; }

                        float baseline = GetItemBaseline(item.Box) + item.Box.MarginTop;
                        float outerHeight = item.ContentHeight
                            + item.Box.PaddingTop + item.Box.PaddingBottom
                            + item.Box.BorderTopWidth + item.Box.BorderBottomWidth
                            + item.Box.MarginTop + item.Box.MarginBottom;
                        float descent = outerHeight - baseline;

                        if (baseline > rowMaxBaselines[row])
                        {
                            rowMaxBaselines[row] = baseline;
                        }
                        if (descent > rowMaxDescents[row])
                        {
                            rowMaxDescents[row] = descent;
                        }
                    }

                    for (int r = 0; r < finalRows; r++)
                    {
                        float needed = rowMaxBaselines[r] + rowMaxDescents[r];
                        if (needed > rowHeights[r])
                        {
                            rowHeights[r] = needed;
                        }
                    }
                }
            }

            // Compute justify-content offset and gap adjustment (horizontal track alignment)
            float justifyContentOffset = 0;
            float effectiveColGap = colGap;
            {
                float totalColW = 0;
                for (int c = 0; c < finalCols; c++)
                    totalColW += colWidths[c];
                totalColW += Math.Max(0, finalCols - 1) * colGap;
                float freeInline = containerWidth - totalColW;
                if (freeInline > 1f)
                {
                    var jc = style.JustifyContent;
                    if (jc == CssJustifyContent.Stretch && finalCols > 0)
                    {
                        // [CSS-ALIGN §5.3.4] Distribute free space equally to all columns
                        float perCol = freeInline / finalCols;
                        for (int c = 0; c < finalCols; c++)
                        {
                            colWidths[c] += perCol;
                        }
                    }
                    else if (jc == CssJustifyContent.Center)
                    {
                        justifyContentOffset = freeInline / 2f;
                    }
                    else if (jc == CssJustifyContent.FlexEnd || jc == CssJustifyContent.End)
                    {
                        justifyContentOffset = freeInline;
                    }
                    else if (jc == CssJustifyContent.SpaceBetween && finalCols > 1)
                    {
                        effectiveColGap = colGap + freeInline / (finalCols - 1);
                    }
                    else if (jc == CssJustifyContent.SpaceAround && finalCols > 0)
                    {
                        float perCol = freeInline / finalCols;
                        justifyContentOffset = perCol / 2f;
                        effectiveColGap = colGap + perCol;
                    }
                    else if (jc == CssJustifyContent.SpaceEvenly && finalCols > 0)
                    {
                        float slot = freeInline / (finalCols + 1);
                        justifyContentOffset = slot;
                        effectiveColGap = colGap + slot;
                    }
                }
            }

            // Compute align-content offset and gap adjustment (vertical track alignment)
            float alignContentOffset = 0;
            float effectiveRowGap = rowGap;
            if (containerHeight > 0)
            {
                float totalRowH = 0;
                for (int r = 0; r < finalRows; r++)
                    totalRowH += rowHeights[r];
                totalRowH += Math.Max(0, finalRows - 1) * rowGap;
                float freeBlock = containerHeight - totalRowH;
                if (freeBlock > 0)
                {
                    var ac = style.AlignContent;
                    if (ac == CssAlignItems.Center)
                        alignContentOffset = freeBlock / 2f;
                    else if (ac == CssAlignItems.End || ac == CssAlignItems.FlexEnd)
                        alignContentOffset = freeBlock;
                    else if (ac == CssAlignItems.SpaceBetween && finalRows > 1)
                        effectiveRowGap = rowGap + freeBlock / (finalRows - 1);
                    else if (ac == CssAlignItems.SpaceAround && finalRows > 0)
                    {
                        float perRow = freeBlock / finalRows;
                        alignContentOffset = perRow / 2f;
                        effectiveRowGap = rowGap + perRow;
                    }
                    else if (ac == CssAlignItems.SpaceEvenly && finalRows > 0)
                    {
                        float slot = freeBlock / (finalRows + 1);
                        alignContentOffset = slot;
                        effectiveRowGap = rowGap + slot;
                    }
                }
            }

            // Second pass: position items
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                float x = parent.ContentRect.X + justifyContentOffset;
                for (int c = 0; c < item.ColStart && c < finalCols; c++)
                    x += colWidths[c] + effectiveColGap;

                float y = parent.ContentRect.Y + alignContentOffset;
                for (int r = 0; r < item.RowStart && r < finalRows; r++)
                    y += rowHeights[r] + effectiveRowGap;

                // For spanning items, calculate the actual cell area
                float spanWidth = 0;
                for (int c = item.ColStart; c < item.ColStart + item.ColSpan && c < finalCols; c++)
                    spanWidth += colWidths[c];
                if (item.ColSpan > 1)
                    spanWidth += (item.ColSpan - 1) * effectiveColGap;

                float spanHeight = 0;
                for (int r = item.RowStart; r < item.RowStart + item.RowSpan && r < finalRows; r++)
                    spanHeight += rowHeights[r];
                if (item.RowSpan > 1)
                    spanHeight += (item.RowSpan - 1) * effectiveRowGap;

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
                    // Only override if explicitly set (valid enum range and not Normal/auto)
                    if (selfBlock != CssAlignItems.Normal && (int)selfBlock <= (int)CssAlignItems.Normal)
                        alignBlock = selfBlock;
                    if (selfInline != CssAlignItems.Normal && (int)selfInline <= (int)CssAlignItems.Normal)
                        alignInline = selfInline;
                }

                // [CSS-GRID §10.3] Auto margins absorb free space, overriding alignment.
                if (item.StyledElement != null)
                {
                    var itemStyle = item.StyledElement.Style;
                    bool autoML = float.IsNaN(itemStyle.MarginLeft);
                    bool autoMR = float.IsNaN(itemStyle.MarginRight);
                    if (autoML || autoMR)
                    {
                        float freeH = spanWidth - outerWidth;
                        if (freeH > 0)
                        {
                            if (autoML && autoMR)
                            {
                                item.Box.MarginLeft = freeH / 2;
                                item.Box.MarginRight = freeH / 2;
                            }
                            else if (autoML)
                            {
                                item.Box.MarginLeft = freeH;
                            }
                            else
                            {
                                item.Box.MarginRight = freeH;
                            }
                        }
                    }

                    bool autoMT = float.IsNaN(itemStyle.MarginTop);
                    bool autoMB = float.IsNaN(itemStyle.MarginBottom);
                    if (autoMT || autoMB)
                    {
                        float freeV = spanHeight - outerHeight;
                        if (freeV > 0)
                        {
                            if (autoMT && autoMB)
                            {
                                item.Box.MarginTop = freeV / 2;
                                item.Box.MarginBottom = freeV / 2;
                            }
                            else if (autoMT)
                            {
                                item.Box.MarginTop = freeV;
                            }
                            else
                            {
                                item.Box.MarginBottom = freeV;
                            }
                        }
                    }
                }

                // Apply inline (horizontal) alignment offset
                float xOffset = AlignOffset(alignInline, spanWidth,
                    finalWidth + item.Box.PaddingLeft + item.Box.PaddingRight
                    + item.Box.BorderLeftWidth + item.Box.BorderRightWidth
                    + item.Box.MarginLeft + item.Box.MarginRight);

                // [CSS-GRID §10.1] Apply block (vertical) alignment offset.
                // Baseline-aligned items use per-row baseline group offset.
                float yOffset;
                if (alignBlock == CssAlignItems.Baseline && rowMaxBaselines != null
                    && item.RowStart >= 0 && item.RowStart < finalRows && item.RowSpan == 1)
                {
                    float itemBaselineFromCell = GetItemBaseline(item.Box) + item.Box.MarginTop;
                    yOffset = rowMaxBaselines[item.RowStart] - itemBaselineFromCell;
                }
                else
                {
                    yOffset = AlignOffset(alignBlock, spanHeight,
                        finalHeight + item.Box.PaddingTop + item.Box.PaddingBottom
                        + item.Box.BorderTopWidth + item.Box.BorderBottomWidth
                        + item.Box.MarginTop + item.Box.MarginBottom);
                }

                // Stretch: expand content to fill cell (default grid behavior)
                // Per CSS Grid spec, stretch only applies when the item's size is 'auto' in that axis.
                bool widthIsAuto = item.StyledElement == null || float.IsNaN(item.StyledElement.Style.Width);
                bool heightIsAuto = item.StyledElement == null || float.IsNaN(item.StyledElement.Style.Height);
                float itemAspectRatio = item.StyledElement != null
                    ? DimensionResolver.GetAspectRatio(item.StyledElement.Style) : 0;

                // [CSS-SIZING-4 §5.1] When aspect-ratio is set, stretch in block axis
                // is preferred. Inline axis derives from ratio, not from stretch.
                if (itemAspectRatio > 0 && widthIsAuto && heightIsAuto)
                {
                    // Stretch height to row track, derive width from ratio
                    if (IsStretch(alignBlock) && outerHeight < spanHeight)
                    {
                        finalHeight = spanHeight - (outerHeight - finalHeight);
                        finalWidth = finalHeight * itemAspectRatio;
                    }
                }
                else
                {
                    if (IsStretch(alignInline) && outerWidth < spanWidth && widthIsAuto)
                    {
                        finalWidth = spanWidth - (outerWidth - finalWidth);
                    }
                    if (heightIsAuto && itemAspectRatio > 0 && !widthIsAuto
                        && !IsStretch(alignBlock))
                    {
                        // [CSS-SIZING-4 §5.1] Width is definite + aspect-ratio → derive height
                        // Only when NOT stretched — stretch overrides aspect-ratio.
                        finalHeight = finalWidth / itemAspectRatio;
                    }
                    else if (IsStretch(alignBlock) && outerHeight < spanHeight && heightIsAuto)
                    {
                        finalHeight = spanHeight - (outerHeight - finalHeight);
                    }
                }

                // [CSS-GRID §6.6] Apply min/max constraints after stretch
                if (item.StyledElement != null)
                {
                    var itemStyle = item.StyledElement.Style;
                    float maxW = itemStyle.MaxWidth;
                    float maxH = itemStyle.MaxHeight;
                    float minW = itemStyle.MinWidth;
                    float minH = itemStyle.MinHeight;
                    if (DeferredPercent.IsEncoded(maxW)) { maxW = DeferredPercent.Resolve(maxW, spanWidth); }
                    if (DeferredPercent.IsEncoded(maxH)) { maxH = DeferredPercent.Resolve(maxH, spanHeight); }
                    if (DeferredPercent.IsEncoded(minW)) { minW = DeferredPercent.Resolve(minW, spanWidth); }
                    if (DeferredPercent.IsEncoded(minH)) { minH = DeferredPercent.Resolve(minH, spanHeight); }
                    if (itemStyle.BoxSizing == CssBoxSizing.BorderBox)
                    {
                        float hExtra = item.Box.PaddingLeft + item.Box.PaddingRight + item.Box.BorderLeftWidth + item.Box.BorderRightWidth;
                        float vExtra = item.Box.PaddingTop + item.Box.PaddingBottom + item.Box.BorderTopWidth + item.Box.BorderBottomWidth;
                        if (!float.IsNaN(maxW) && maxW >= 0) { maxW = Math.Max(0, maxW - hExtra); }
                        if (!float.IsNaN(maxH) && maxH >= 0) { maxH = Math.Max(0, maxH - vExtra); }
                        if (!float.IsNaN(minW) && minW >= 0) { minW = Math.Max(0, minW - hExtra); }
                        if (!float.IsNaN(minH) && minH >= 0) { minH = Math.Max(0, minH - vExtra); }
                    }
                    if (!float.IsNaN(maxW) && maxW >= 0 && finalWidth > maxW) { finalWidth = maxW; }
                    if (!float.IsNaN(maxH) && maxH >= 0 && finalHeight > maxH) { finalHeight = maxH; }
                    if (!float.IsNaN(minW) && minW >= 0 && finalWidth < minW) { finalWidth = minW; }
                    if (!float.IsNaN(minH) && minH >= 0 && finalHeight < minH) { finalHeight = minH; }
                }

                // If stretched dimensions differ from original, re-layout children
                // for items whose layout depends on container size (grid/flex with fr tracks).
                bool widthChanged = Math.Abs(finalWidth - item.ContentWidth) > 0.01f;
                bool heightChanged = Math.Abs(finalHeight - item.ContentHeight) > 0.01f;
                if ((widthChanged || heightChanged) && item.StyledElement != null)
                {
                    var itemDisplay = item.StyledElement.Style.Display;
                    if (itemDisplay == CssDisplay.Grid || itemDisplay == CssDisplay.InlineGrid)
                    {
                        // Re-layout inner grid with stretched dimensions
                        item.Box.ClearChildren();
                        item.Box.LineBoxes = null;
                        item.Box.ContentRect = new RectF(0, 0, finalWidth, finalHeight);
                        var savedFloatCtx2 = context.FloatContext;
                        context.FloatContext = new FloatContext(0, finalWidth);
                        var savedCtx = context.ParentGridContext;
                        context.ParentGridContext = new ParentGridContext
                        {
                            ColumnWidths = colWidths,
                            RowHeights = rowHeights,
                            ColumnGap = colGap,
                            RowGap = rowGap,
                            ItemColStart = item.ColStart,
                            ItemColSpan = item.ColSpan,
                            ItemRowStart = item.RowStart,
                            ItemRowSpan = item.RowSpan
                        };
                        BlockFormattingContext.LayoutChildren(item.Box, context);
                        context.ParentGridContext = savedCtx;
                        context.FloatContext = savedFloatCtx2;
                    }
                    else if (itemDisplay == CssDisplay.Flex || itemDisplay == CssDisplay.InlineFlex)
                    {
                        // Re-layout flex container with stretched dimensions so that
                        // cross-axis alignment (align-items: center) works correctly.
                        item.Box.ClearChildren();
                        item.Box.LineBoxes = null;
                        item.Box.ContentRect = new RectF(0, 0, finalWidth, finalHeight);
                        var savedFloatCtx3 = context.FloatContext;
                        context.FloatContext = new FloatContext(0, finalWidth);
                        BlockFormattingContext.LayoutChildren(item.Box, context);
                        context.FloatContext = savedFloatCtx3;
                    }
                }

                float newX = x + xOffset + item.Box.MarginLeft + item.Box.BorderLeftWidth + item.Box.PaddingLeft;
                float newY = y + yOffset + item.Box.MarginTop + item.Box.BorderTopWidth + item.Box.PaddingTop;

                // Offset all descendants (children + line boxes) from first-pass (0,0)
                // to the actual grid cell position.
                float dx = newX - item.Box.ContentRect.X;
                float dy = newY - item.Box.ContentRect.Y;
                if (dx != 0 || dy != 0)
                {
                    for (int ci = 0; ci < item.Box.Children.Count; ci++)
                        OffsetBoxInPlace(item.Box.Children[ci], dx, dy);
                    if (item.Box.LineBoxes != null)
                    {
                        for (int li = 0; li < item.Box.LineBoxes.Count; li++)
                        {
                            item.Box.LineBoxes[li].X += dx;
                            item.Box.LineBoxes[li].Y += dy;
                        }
                    }
                }

                item.Box.ContentRect = new RectF(newX, newY, finalWidth, finalHeight);

                parent.AddChild(item.Box);
            }

            // [CSS-GRID §9] Position abspos items with grid placement within their
            // grid areas. These were deferred until track sizing completed because
            // they need grid area coordinates but must not participate in track sizing.
            PositionAbsposGridItems(absposGridItems, parent, context, style,
                colWidths, rowHeights, finalCols, finalRows,
                effectiveColGap, effectiveRowGap,
                justifyContentOffset, alignContentOffset,
                containerWidth, containerHeight, explicitCols, explicitRows);
        }
        /// <summary>
        /// [CSS-GRID §9] Position absolutely positioned grid items with explicit grid
        /// placement within their grid areas. The grid area serves as the containing
        /// block for CSS positioning (top/left/right/bottom offsets).
        /// </summary>
        private static void PositionAbsposGridItems(
            List<GridItem> absposGridItems, LayoutBox parent, LayoutContext context,
            ComputedStyle containerStyle,
            float[] colWidths, float[] rowHeights, int finalCols, int finalRows,
            float effectiveColGap, float effectiveRowGap,
            float justifyContentOffset, float alignContentOffset,
            float containerWidth, float containerHeight,
            int explicitCols, int explicitRows)
        {
            if (absposGridItems.Count == 0)
            {
                return;
            }

            for (int i = 0; i < absposGridItems.Count; i++)
            {
                var item = absposGridItems[i];
                if (item.StyledElement == null)
                {
                    continue;
                }

                // Compute the grid area rectangle for this abspos item.
                // Per CSS Grid §9, each axis independently:
                //   - If start line is explicit → area starts at that grid line
                //   - If start line is auto → area starts at padding edge (line 0)
                //   - If end line is explicit → area ends at that grid line
                //   - If end line is auto → area ends at padding edge (last line)
                var gridArea = ComputeAbsposGridArea(item, parent,
                    colWidths, rowHeights, finalCols, finalRows,
                    effectiveColGap, effectiveRowGap,
                    justifyContentOffset, alignContentOffset,
                    containerWidth, containerHeight, explicitCols, explicitRows);

                float areaWidth = gridArea.Width;
                float areaHeight = gridArea.Height;

                // Lay out the abspos item using the grid area as containing block.
                var posBox = item.Box;
                BoxModelCalculator.ApplyBoxModel(posBox, item.StyledElement.Style, areaWidth);

                // Isolate float context for abspos grid item layout.
                var savedFloatCtx = context.FloatContext;
                context.FloatContext = new FloatContext(0, areaWidth);

                float posWidth;
                bool widthIsAuto = float.IsNaN(item.StyledElement.Style.Width);
                if (widthIsAuto)
                {
                    // [CSS2 §10.3.7] Auto width on abspos → shrink-to-fit (= fit-content).
                    posWidth = BlockFormattingContext.MeasureIntrinsicWidth(
                        item.StyledElement, SizingKeyword.FitContent, areaWidth, context);
                    if (posWidth > areaWidth)
                    {
                        posWidth = areaWidth;
                    }
                }
                else
                {
                    posWidth = DimensionResolver.ResolveWidth(
                        item.StyledElement.Style, areaWidth, posBox);
                }

                // Layout children at the resolved static position and width.
                posBox.ContentRect = new RectF(gridArea.X, gridArea.Y, posWidth, 0);
                BlockFormattingContext.LayoutChildren(posBox, context);

                float posHeight = DimensionResolver.ResolveHeight(
                    item.StyledElement.Style, areaHeight, posBox);
                if (float.IsNaN(posHeight))
                {
                    posHeight = BlockFormattingContext.CalculateAutoHeight(posBox);
                }

                // [CSS-GRID §9] Apply alignment to the abspos item's static position.
                // The static position respects align-items/align-self and justify-items/justify-self.
                float outerWidth = posWidth + posBox.PaddingLeft + posBox.PaddingRight
                    + posBox.BorderLeftWidth + posBox.BorderRightWidth
                    + posBox.MarginLeft + posBox.MarginRight;
                float outerHeight = posHeight + posBox.PaddingTop + posBox.PaddingBottom
                    + posBox.BorderTopWidth + posBox.BorderBottomWidth
                    + posBox.MarginTop + posBox.MarginBottom;

                float staticX = gridArea.X;
                float staticY = gridArea.Y;

                CssAlignItems justifySelf = item.StyledElement.Style.JustifySelf;
                if (justifySelf == CssAlignItems.Normal || (int)justifySelf > (int)CssAlignItems.Normal)
                {
                    justifySelf = containerStyle.JustifyItems;
                }
                float freeH = areaWidth - outerWidth;
                if (freeH > 0)
                {
                    if (justifySelf == CssAlignItems.Center)
                    {
                        staticX += freeH / 2f;
                    }
                    else if (justifySelf == CssAlignItems.End || justifySelf == CssAlignItems.FlexEnd)
                    {
                        staticX += freeH;
                    }
                }

                CssAlignItems alignSelf = item.StyledElement.Style.AlignSelf;
                if (alignSelf == CssAlignItems.Normal || (int)alignSelf > (int)CssAlignItems.Normal)
                {
                    alignSelf = containerStyle.AlignItems;
                }
                float freeV = areaHeight - outerHeight;
                if (freeV > 0)
                {
                    if (alignSelf == CssAlignItems.Center)
                    {
                        staticY += freeV / 2f;
                    }
                    else if (alignSelf == CssAlignItems.End || alignSelf == CssAlignItems.FlexEnd
                             )
                    {
                        staticY += freeV;
                    }
                }

                posBox.ContentRect = new RectF(staticX, staticY, posWidth, posHeight);

                // Restore float context after layout.
                context.FloatContext = savedFloatCtx;

                // Store the grid area as the containing block for PositionedLayout.
                // ApplyAbsolute will use this instead of the grid container's padding rect.
                posBox.GridAreaContainingBlock = gridArea;

                parent.AddChild(posBox);
            }
        }

        /// <summary>
        /// [CSS-GRID §9] Compute the grid area rectangle for an abspos item.
        /// Auto lines map to the grid container's padding edge (not content edge).
        /// Explicit lines map to the grid line position within the content area.
        /// </summary>
        private static RectF ComputeAbsposGridArea(
            GridItem item, LayoutBox parent,
            float[] colWidths, float[] rowHeights, int finalCols, int finalRows,
            float effectiveColGap, float effectiveRowGap,
            float justifyContentOffset, float alignContentOffset,
            float containerWidth, float containerHeight,
            int explicitCols, int explicitRows)
        {
            float contentX = parent.ContentRect.X;
            float contentY = parent.ContentRect.Y;

            // [CSS-GRID §9] Auto lines resolve to the padding edge of the grid
            // container. The padding edge is the boundary between the grid's
            // padding and its border — i.e., the outer edge of the padding area.
            float paddingEdgeLeft = contentX - parent.PaddingLeft;
            float paddingEdgeTop = contentY - parent.PaddingTop;
            float paddingEdgeRight = contentX + containerWidth + parent.PaddingRight;
            float paddingEdgeBottom = contentY + containerHeight + parent.PaddingBottom;

            // Determine column start/end lines for the containing block.
            // -1 means "auto" → maps to padding edge of grid container.
            // [CSS-GRID §9.2] For abspos items, a bare span or auto on the end side
            // is treated as auto (padding edge). Only explicit line numbers count.
            int colStartLine = item.ColStart;
            int colEndLine;

            if (colStartLine >= 0 && item.IsColEndExplicitLine)
            {
                // Both start and end are explicit lines
                colEndLine = colStartLine + item.ColSpan;
            }
            else if (colStartLine >= 0)
            {
                // Explicit start, but end is span or auto → end is auto (padding edge)
                colEndLine = -1;
            }
            else if (item.ExplicitColEnd >= 0)
            {
                // Auto start, explicit end
                colEndLine = item.ExplicitColEnd;
            }
            else if (item.RawColEnd != 0)
            {
                // Resolve negative end line. If out of range (< 0), treat as auto.
                int resolved = ResolveNegativeLine(item.RawColEnd, Math.Max(1, explicitCols));
                colEndLine = resolved >= 0 ? resolved : -1;
            }
            else
            {
                // Both auto: padding edge to padding edge
                colStartLine = -1;
                colEndLine = -1;
            }

            // Determine row start/end lines
            int rowStartLine = item.RowStart;
            int rowEndLine;

            if (rowStartLine >= 0 && item.IsRowEndExplicitLine)
            {
                rowEndLine = rowStartLine + item.RowSpan;
            }
            else if (rowStartLine >= 0)
            {
                rowEndLine = -1;
            }
            else if (item.ExplicitRowEnd >= 0)
            {
                rowEndLine = item.ExplicitRowEnd;
            }
            else if (item.RawRowEnd != 0)
            {
                int resolved = ResolveNegativeLine(item.RawRowEnd, Math.Max(1, explicitRows));
                rowEndLine = resolved >= 0 ? resolved : -1;
            }
            else
            {
                rowStartLine = -1;
                rowEndLine = -1;
            }

            // [CSS-GRID §9] If a grid line is beyond the implicit grid,
            // treat it as auto (padding edge). Grid lines range from 0 to
            // finalCols/finalRows (inclusive). Line finalCols is the right/bottom
            // edge of the last track.
            if (colStartLine > finalCols)
            {
                colStartLine = -1;
            }
            if (colEndLine > finalCols)
            {
                colEndLine = -1;
            }
            if (rowStartLine > finalRows)
            {
                rowStartLine = -1;
            }
            if (rowEndLine > finalRows)
            {
                rowEndLine = -1;
            }

            // Compute X coordinates
            float areaX;
            float areaWidth;

            if (colStartLine < 0 && colEndLine < 0)
            {
                // Both auto: padding edge to padding edge
                areaX = paddingEdgeLeft;
                areaWidth = paddingEdgeRight - paddingEdgeLeft;
            }
            else
            {
                // Compute start X: position of the start grid line
                if (colStartLine >= 0)
                {
                    areaX = contentX + justifyContentOffset;
                    for (int c = 0; c < colStartLine && c < finalCols; c++)
                    {
                        areaX += colWidths[c] + effectiveColGap;
                    }
                }
                else
                {
                    // Auto start: padding edge
                    areaX = paddingEdgeLeft;
                }

                // Compute end X: position of the end grid line
                float areaEndX;
                if (colEndLine >= 0)
                {
                    areaEndX = contentX + justifyContentOffset;
                    int clampedEnd = Math.Min(colEndLine, finalCols);
                    for (int c = 0; c < clampedEnd; c++)
                    {
                        areaEndX += colWidths[c];
                        if (c < clampedEnd - 1)
                        {
                            areaEndX += effectiveColGap;
                        }
                    }
                }
                else
                {
                    // Auto end: padding edge
                    areaEndX = paddingEdgeRight;
                }

                areaWidth = Math.Max(0, areaEndX - areaX);
            }

            // Compute Y coordinates
            float areaY;
            float areaHeight;

            if (rowStartLine < 0 && rowEndLine < 0)
            {
                areaY = paddingEdgeTop;
                areaHeight = paddingEdgeBottom - paddingEdgeTop;
            }
            else
            {
                if (rowStartLine >= 0)
                {
                    areaY = contentY + alignContentOffset;
                    for (int r = 0; r < rowStartLine && r < finalRows; r++)
                    {
                        areaY += rowHeights[r] + effectiveRowGap;
                    }
                }
                else
                {
                    areaY = paddingEdgeTop;
                }

                float areaEndY;
                if (rowEndLine >= 0)
                {
                    areaEndY = contentY + alignContentOffset;
                    int clampedEnd = Math.Min(rowEndLine, finalRows);
                    for (int r = 0; r < clampedEnd; r++)
                    {
                        areaEndY += rowHeights[r];
                        if (r < clampedEnd - 1)
                        {
                            areaEndY += effectiveRowGap;
                        }
                    }
                }
                else
                {
                    areaEndY = paddingEdgeBottom;
                }

                areaHeight = Math.Max(0, areaEndY - areaY);
            }

            return new RectF(areaX, areaY, areaWidth, areaHeight);
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

        private static void ParsePlacement(ComputedStyle style, GridItem item,
            Dictionary<string, List<int>>? colLineNames = null,
            Dictionary<string, List<int>>? rowLineNames = null)
        {
            // Check if grid-row-start is a plain identifier (named area from grid-area shorthand)
            var rowStartRaw = style.GetRefValue(PropertyId.GridRowStart);
            if (rowStartRaw is CssKeywordValue areaKw && areaKw.Keyword != "auto" && areaKw.Keyword != "span")
            {
                // Check if it's a named LINE (not area) by looking in line name maps
                bool isNamedLine = (rowLineNames != null && rowLineNames.ContainsKey(areaKw.Keyword))
                    || (colLineNames != null && colLineNames.ContainsKey(areaKw.Keyword));
                if (!isNamedLine)
                {
                    // This is a named area reference (e.g., grid-area: header)
                    item.AreaName = areaKw.Keyword;
                    return;
                }
            }

            item.RowStart = ParseLineValue(rowStartRaw, out int rowSpan, rowLineNames);
            item.RowSpan = rowSpan;

            int endRowSpan;
            var rowEndRaw = style.GetRefValue(PropertyId.GridRowEnd);
            int rowEnd = ParseLineValue(rowEndRaw, out endRowSpan, rowLineNames);
            bool rowEndIsNegativeLine = rowEnd < 0 && IsNegativeLineNumber(rowEndRaw);
            if (rowEnd >= 0 && item.RowStart >= 0 && rowEnd > item.RowStart)
            {
                item.RowSpan = rowEnd - item.RowStart;
                item.IsRowEndExplicitLine = true;
            }
            else if (rowEndIsNegativeLine)
            {
                item.RawRowEnd = rowEnd; // Store for deferred resolution
                item.IsRowEndExplicitLine = true;
            }
            else if (endRowSpan > 1 && item.RowStart >= 0)
            {
                item.RowSpan = endRowSpan;
                // Span, not an explicit line — IsRowEndExplicitLine stays false
            }
            else if (rowEnd >= 0)
            {
                // Explicit positive end line but start was auto or end <= start
                item.IsRowEndExplicitLine = true;
            }
            // [CSS-GRID §9] Store explicit end line even when start is auto,
            // needed for abspos containing block with partial placement.
            if (rowEnd >= 0 && item.RowStart < 0)
            {
                item.ExplicitRowEnd = rowEnd;
            }

            item.ColStart = ParseLineValue(style.GetRefValue(PropertyId.GridColumnStart), out int colSpan, colLineNames);
            item.ColSpan = colSpan;

            int endColSpan;
            var colEndRaw = style.GetRefValue(PropertyId.GridColumnEnd);
            int colEnd = ParseLineValue(colEndRaw, out endColSpan, colLineNames);
            bool colEndIsNegativeLine = colEnd < 0 && IsNegativeLineNumber(colEndRaw);
            if (colEnd >= 0 && item.ColStart >= 0 && colEnd > item.ColStart)
            {
                item.ColSpan = colEnd - item.ColStart;
                item.IsColEndExplicitLine = true;
            }
            else if (colEndIsNegativeLine)
            {
                item.RawColEnd = colEnd; // Store for deferred resolution
                item.IsColEndExplicitLine = true;
            }
            else if (endColSpan > 1 && item.ColStart >= 0)
            {
                item.ColSpan = endColSpan;
                // Span, not an explicit line
            }
            else if (colEnd >= 0)
            {
                item.IsColEndExplicitLine = true;
            }
            // [CSS-GRID §9] Store explicit end line even when start is auto.
            if (colEnd >= 0 && item.ColStart < 0)
            {
                item.ExplicitColEnd = colEnd;
            }
        }

        /// <summary>
        /// Parse a grid line value (e.g., "2", "-1", "span 2", "auto").
        /// Returns -1 for auto, 0-based line number otherwise.
        /// Negative line numbers are stored as negative values (resolved later with grid size).
        /// Sets span to > 1 when "span N" is used.
        /// </summary>
        private static int ParseLineValue(object? raw, out int span,
            Dictionary<string, List<int>>? lineNames = null)
        {
            span = 1;
            if (raw == null) return -1;

            if (raw is CssKeywordValue kw)
            {
                if (kw.Keyword == "auto") return -1;
                if (kw.Keyword == "span") return -1;
                // [CSS-GRID §8.3] Named line reference: grid-column: header-start
                if (lineNames != null && lineNames.TryGetValue(kw.Keyword, out var indices) && indices.Count > 0)
                {
                    return indices[0]; // use first matching line
                }
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
        /// Returns true if the raw CSS value is a negative integer line number
        /// (not auto, not null, not a keyword). Distinguishes CSS line -1 from auto.
        /// </summary>
        private static bool IsNegativeLineNumber(object? raw)
        {
            if (raw is CssNumberValue num && num.Value < 0)
            {
                return true;
            }
            if (raw is CssDimensionValue dim && dim.Value < 0)
            {
                return true;
            }
            if (raw is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (list.Values[i] is CssNumberValue n && n.Value < 0)
                    {
                        return true;
                    }
                    if (list.Values[i] is CssDimensionValue d && d.Value < 0)
                    {
                        return true;
                    }
                }
            }
            return false;
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

        private static int FindFreeColumn(bool[] occupied, int cols, int row, int colSpan, int rowSpan, int startCol)
        {
            for (int c = startCol; c + colSpan - 1 < cols; c++)
            {
                if (IsFree(occupied, cols, row, c, rowSpan, colSpan))
                {
                    return c;
                }
            }
            return -1;
        }

        private static int FindFreeRow(bool[] occupied, int cols, int rows, int col, int rowSpan, int colSpan, int startRow)
        {
            for (int r = startRow; r + rowSpan - 1 < rows; r++)
            {
                if (IsFree(occupied, cols, r, col, rowSpan, colSpan))
                {
                    return r;
                }
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
        internal static float[]? ResolveTrackList(object? raw, float containerSize, float gap = 0,
            Dictionary<string, List<int>>? lineNames = null)
        {
            if (raw == null) return null;
            if (raw is CssKeywordValue kw && (kw.Keyword == "none" || kw.Keyword == "auto" || kw.Keyword == "subgrid"))
                return null;

            // Flatten into a list of individual track values (expanding repeat())
            // Line name brackets [name] are extracted and mapped to line indices.
            var flatValues = new List<object>();
            if (raw is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    var val = list.Values[i];
                    // [CSS-GRID §7.1] Line name brackets: [name1 name2]
                    if (val is CssListValue bracket && bracket.Separator == ' '
                        && bracket.Values.Count > 0 && bracket.Values[0] is CssKeywordValue)
                    {
                        // This might be a line name bracket — check if ALL items are keywords
                        bool allKeywords = true;
                        for (int bi = 0; bi < bracket.Values.Count; bi++)
                        {
                            if (!(bracket.Values[bi] is CssKeywordValue)) { allKeywords = false; break; }
                        }
                        if (allKeywords && lineNames != null)
                        {
                            int lineIdx = flatValues.Count; // line before next track
                            for (int bi = 0; bi < bracket.Values.Count; bi++)
                            {
                                string name = ((CssKeywordValue)bracket.Values[bi]).Keyword;
                                if (!lineNames.ContainsKey(name))
                                    lineNames[name] = new List<int>();
                                lineNames[name].Add(lineIdx);
                            }
                            continue; // don't add to flatValues — it's a line name, not a track
                        }
                    }
                    FlattenTrackValue(val, flatValues, containerSize, gap);
                }
            }
            else
            {
                FlattenTrackValue(raw, flatValues, containerSize, gap);
            }

            if (flatValues.Count == 0) return null;

            // Two-pass: collect sizes, resolve fr units
            var sizes = new List<(float value, bool isFr)>();
            var minFloors = new float[flatValues.Count];
            float totalFixed = 0;
            float totalFr = 0;
            bool hasIntrinsic = false;

            for (int i = 0; i < flatValues.Count; i++)
            {
                var parsed = ParseTrackValue(flatValues[i], containerSize);
                sizes.Add(parsed);
                minFloors[i] = GetMinmaxFloor(flatValues[i], containerSize);
                if (parsed.value < 0)
                {
                    hasIntrinsic = true;
                    continue; // intrinsic sentinel — don't count toward fixed or fr
                }
                if (parsed.isFr)
                    totalFr += parsed.value;
                else
                    totalFixed += parsed.value;
            }

            // Subtract gap space: N tracks have (N-1) gaps
            float totalGapSpace = flatValues.Count > 1 ? (flatValues.Count - 1) * gap : 0;
            float remaining = Math.Max(0, containerSize - totalFixed - totalGapSpace);

            // Chrome resolves fr tracks using LayoutUnit (1/64px) integer arithmetic.
            // This truncates frSize to 1/64px and distributes the remainder (1/64px units)
            // to the first fr tracks. This avoids sub-pixel accumulation differences.
            int remainingRaw = (int)(remaining * 64f);
            int totalFrInt = (int)totalFr; // fr values are typically integers (1fr, 2fr)
            int frSizeRaw = totalFrInt > 0 ? remainingRaw / totalFrInt : 0;
            int frRemainder = totalFrInt > 0 ? remainingRaw % totalFrInt : 0;
            float frSize = totalFr > 0 ? remaining / totalFr : 0;

            // Count total fr tracks for remainder distribution from end
            int totalFrTracks = 0;
            for (int i = 0; i < sizes.Count; i++)
            {
                if (sizes[i].isFr && sizes[i].value > 0 && !hasIntrinsic)
                {
                    totalFrTracks++;
                }
            }

            var tracks = new float[sizes.Count];
            int frIndex = 0;
            for (int i = 0; i < sizes.Count; i++)
            {
                if (sizes[i].value < 0)
                {
                    // Preserve sentinel for intrinsic sizing (-1 = min-content, -2 = max-content)
                    tracks[i] = sizes[i].value;
                    continue;
                }
                if (sizes[i].isFr)
                {
                    if (hasIntrinsic)
                    {
                        // Defer fr resolution: encode as sentinel -(1000 + frValue)
                        // Will be resolved after intrinsic tracks are measured
                        tracks[i] = -(1000f + sizes[i].value);
                        continue;
                    }
                    // Use LayoutUnit integer arithmetic for fr distribution
                    int trackFr = (int)sizes[i].value;
                    if (trackFr > 0 && totalFrInt > 0)
                    {
                        int trackRaw = frSizeRaw * trackFr;
                        // Chrome distributes remainder to LAST tracks, not first.
                        // frIndex counts from 0; remainder goes to the last frRemainder tracks.
                        int distanceFromEnd = totalFrTracks - 1 - frIndex;
                        if (distanceFromEnd < frRemainder)
                        {
                            trackRaw += 1;
                        }
                        float resolved = trackRaw / 64f;
                        if (minFloors[i] > 0 && resolved < minFloors[i])
                        {
                            resolved = minFloors[i];
                        }
                        tracks[i] = resolved;
                    }
                    else
                    {
                        // Non-integer fr: fall back to float division
                        float resolved = sizes[i].value * frSize;
                        if (minFloors[i] > 0 && resolved < minFloors[i])
                        {
                            resolved = minFloors[i];
                        }
                        tracks[i] = resolved;
                    }
                    frIndex++;
                }
                else
                {
                    float resolved = sizes[i].value;
                    if (minFloors[i] > 0 && resolved < minFloors[i])
                        resolved = minFloors[i];
                    tracks[i] = resolved;
                }
            }

            return tracks;
        }

        /// <summary>
        /// [CSS-GRID-1 §7.2.3.1] Checks if auto-fit repeat includes a flexible (fr) track max.
        /// </summary>
        /// <summary>
        /// Extracts named line mappings from grid-template-columns/rows raw values.
        /// Named lines appear as [name] brackets between track sizes.
        /// The CSS parser represents [name] as a CssListValue inside the track list.
        /// </summary>
        private static void ExtractLineNames(object? raw, Dictionary<string, List<int>> lineNames)
        {
            if (raw == null) return;
            if (!(raw is CssListValue list)) return;

            int trackIndex = 0;
            for (int i = 0; i < list.Values.Count; i++)
            {
                var val = list.Values[i];

                // Track-sizing values increment the track index
                if (val is CssDimensionValue || val is CssNumberValue ||
                    val is CssPercentageValue || val is CssFunctionValue)
                {
                    trackIndex++;
                    continue;
                }

                // Keywords that are track sizes
                if (val is CssKeywordValue kw)
                {
                    if (kw.Keyword == "auto" || kw.Keyword == "min-content" ||
                        kw.Keyword == "max-content" || kw.Keyword == "fit-content")
                    {
                        trackIndex++;
                        continue;
                    }
                    // Skip non-track keywords (none, subgrid, etc.)
                    continue;
                }

                // Space-separated sub-list might be line names or a bracketed group
                if (val is CssListValue subList && subList.Separator == ' ')
                {
                    // Check if all items are keywords (likely line names)
                    bool allKw = true;
                    for (int j = 0; j < subList.Values.Count; j++)
                    {
                        if (!(subList.Values[j] is CssKeywordValue)) { allKw = false; break; }
                    }
                    if (allKw)
                    {
                        for (int j = 0; j < subList.Values.Count; j++)
                        {
                            string name = ((CssKeywordValue)subList.Values[j]).Keyword;
                            if (!lineNames.ContainsKey(name))
                                lineNames[name] = new List<int>();
                            lineNames[name].Add(trackIndex);
                        }
                    }
                    else
                    {
                        // Mixed list — probably contains track values, count them
                        for (int j = 0; j < subList.Values.Count; j++)
                        {
                            if (subList.Values[j] is CssDimensionValue || subList.Values[j] is CssFunctionValue)
                                trackIndex++;
                        }
                    }
                }
            }
        }

        private static bool HasAutoFitFr(object? raw)
        {
            if (raw is CssFunctionValue fn && fn.Name == "repeat" && fn.Arguments.Count >= 2)
            {
                if (fn.Arguments[0] is CssKeywordValue kw && kw.Keyword == "auto-fit")
                {
                    for (int j = 1; j < fn.Arguments.Count; j++)
                    {
                        if (fn.Arguments[j] is CssFunctionValue mmFn && mmFn.Name == "minmax" && mmFn.Arguments.Count >= 2)
                        {
                            var maxArg = mmFn.Arguments[mmFn.Arguments.Count - 1];
                            if (maxArg is CssDimensionValue dim && dim.Unit == "fr")
                            {
                                return true;
                            }
                        }
                        else
                        {
                            var parsed = ParseTrackValue(fn.Arguments[j], 0);
                            if (parsed.isFr) { return true; }
                        }
                    }
                }
            }
            if (raw is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (HasAutoFitFr(list.Values[i])) { return true; }
                }
            }
            return false;
        }

        /// <summary>
        /// [CSS-GRID-1 §7.2.3.1] Checks if a grid-template value contains repeat(auto-fit, ...).
        /// </summary>
        private static bool HasAutoFit(object? raw)
        {
            if (raw is CssFunctionValue fn && fn.Name == "repeat" && fn.Arguments.Count >= 2)
            {
                return fn.Arguments[0] is CssKeywordValue kw && kw.Keyword == "auto-fit";
            }
            if (raw is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (HasAutoFit(list.Values[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static void FlattenTrackValue(object val, List<object> output, float containerSize, float gap = 0)
        {
            if (val is CssFunctionValue fn && fn.Name == "repeat" && fn.Arguments.Count >= 2)
            {
                var first = fn.Arguments[0];
                bool isAutoFill = first is CssKeywordValue kw1 &&
                    (kw1.Keyword == "auto-fill" || kw1.Keyword == "auto-fit");

                int count;
                if (isAutoFill)
                {
                    // [CSS-GRID-1 §7.2.3.1] auto-fill/auto-fit: count from container
                    // size, track min size, and gap.
                    // count = floor((containerSize + gap) / (trackMinSize + gap))
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
                    if (trackMinSize > 0)
                    {
                        float denominator = trackMinSize + gap;
                        count = Math.Max(1, (int)Math.Floor((containerSize + gap) / denominator));
                    }
                    else
                    {
                        count = 1;
                    }
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
                    return (1, true); // [CSS-GRID §7.2.1] auto acts like minmax(auto, auto) ≈ 1fr
                if (kwVal.Keyword == "min-content")
                    return (-1, false); // sentinel: resolved by content measurement
                if (kwVal.Keyword == "max-content")
                    return (-2, false); // sentinel: resolved by content measurement
            }
            if (val is CssFunctionValue fn)
            {
                if (fn.Name == "minmax" && fn.Arguments.Count >= 2)
                {
                    // minmax(min, max): use max if it's fr, otherwise clamp between min and max
                    var maxVal = ParseTrackValue(fn.Arguments[fn.Arguments.Count - 1], containerSize);
                    if (maxVal.isFr)
                    {
                        // fr-based max: report as fr so it gets flexible space,
                        // minimum will be enforced in ResolveTrackList via minmax tracking
                        return maxVal;
                    }
                    var minVal = ParseTrackValue(fn.Arguments[0], containerSize);
                    // [CSS-GRID §7.2.4] Both fixed: use max as the track size,
                    // but clamp to container when the container has a definite size
                    // and the min is a definite non-intrinsic value.
                    float trackSize = Math.Max(minVal.value, maxVal.value);
                    if (!minVal.isFr && !maxVal.isFr && containerSize > 0
                        && minVal.value >= 0 && trackSize > containerSize)
                    {
                        trackSize = Math.Max(minVal.value, containerSize);
                    }
                    return (trackSize, false);
                }
                if (fn.Name == "fit-content" && fn.Arguments.Count >= 1)
                {
                    // [CSS-GRID §7.2.4.1] fit-content(limit) = minmax(auto, min(max-content, limit))
                    // Use sentinel -3 to trigger intrinsic sizing with max-content clamped at limit.
                    // The limit value is stored via FitContentLimits dictionary at the call site.
                    return (-3, false);
                }
                // [CSS-VALUES §8] calc/min/max/clamp math functions
                if (fn.Name == "calc" || fn.Name == "min" || fn.Name == "max" || fn.Name == "clamp")
                {
                    var ctx = new Core.Values.CssResolutionContext(16f, 16f, containerSize, containerSize, containerSize);
                    float result = Css.Resolution.Internal.ValueResolver.EvaluateDeferredCalc(fn, containerSize);
                    return (result, false);
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

        /// <summary>
        /// Extracts fit-content() limit values from raw track definitions.
        /// Returns a float array indexed by track position, with the limit for fit-content tracks
        /// and -1 for non-fit-content tracks. Returns null if no fit-content tracks exist.
        /// </summary>
        private static float[]? ExtractFitContentLimits(
            object? raw, int trackCount, float containerSize, float gap)
        {
            if (raw == null) return null;

            var flatValues = new List<object>();
            if (raw is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    FlattenTrackValue(list.Values[i], flatValues, containerSize, gap);
                }
            }
            else
            {
                FlattenTrackValue(raw, flatValues, containerSize, gap);
            }

            float[]? limits = null;
            for (int i = 0; i < Math.Min(flatValues.Count, trackCount); i++)
            {
                if (flatValues[i] is CssFunctionValue fn
                    && fn.Name == "fit-content" && fn.Arguments.Count >= 1)
                {
                    if (limits == null)
                    {
                        limits = new float[trackCount];
                        for (int j = 0; j < trackCount; j++)
                        {
                            limits[j] = -1f;
                        }
                    }
                    var limitVal = ParseTrackValue(fn.Arguments[0], containerSize);
                    limits[i] = limitVal.value;
                }
            }
            return limits;
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

        /// <summary>
        /// [CSS-GRID §10.1] Get the first baseline of a grid item from its first line box,
        /// or fall back to its bottom border edge.
        /// </summary>
        private static float GetItemBaseline(LayoutBox box)
        {
            if (box.LineBoxes != null && box.LineBoxes.Count > 0)
            {
                return box.LineBoxes[0].Baseline + (box.LineBoxes[0].Y - box.ContentRect.Y)
                     + box.PaddingTop + box.BorderTopWidth;
            }

            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                if (child.LineBoxes != null && child.LineBoxes.Count > 0)
                {
                    return child.LineBoxes[0].Baseline + (child.LineBoxes[0].Y - box.ContentRect.Y)
                         + box.PaddingTop + box.BorderTopWidth;
                }
            }

            return box.ContentRect.Height + box.PaddingTop + box.BorderTopWidth;
        }

        /// <summary>
        /// [CSS-GRID §10.1] Resolve the effective block-axis alignment for a grid item,
        /// considering align-self override vs container align-items default.
        /// </summary>
        private static CssAlignItems ResolveItemBlockAlignment(GridItem item, CssAlignItems containerDefault)
        {
            if (item.StyledElement == null)
            {
                return containerDefault;
            }
            var selfAlign = item.StyledElement.Style.AlignSelf;
            if (selfAlign != CssAlignItems.Normal && (int)selfAlign <= (int)CssAlignItems.Normal)
            {
                return selfAlign;
            }
            return containerDefault;
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

        /// <summary>
        /// Returns true if the given raw CSS value for grid-template-columns/rows is the "subgrid" keyword.
        /// </summary>
        internal static bool IsSubgrid(object? raw)
        {
            if (raw is CssKeywordValue kw && kw.Keyword == "subgrid")
                return true;
            return false;
        }

        /// <summary>
        /// Extract track sizes from the parent grid context for a subgridded axis.
        /// Returns the parent tracks corresponding to the lines this item spans,
        /// or null if no parent grid context is available.
        /// </summary>
        /// <summary>
        /// [CSS-GRID-2 §8.1] Extract subgrid tracks from parent, adjusting for gap delta.
        /// When the subgrid's gap differs from the parent's gap, track sizes are adjusted
        /// so the total space (tracks + gaps) matches the parent's allocation.
        /// </summary>
        private static float[]? GetSubgridTracks(float[] parentTracks, int itemStart, int itemSpan,
            float parentGap = 0, float subgridGap = 0)
        {
            if (parentTracks == null || parentTracks.Length == 0)
            {
                return null;
            }

            int count = itemSpan;
            int start = Math.Max(0, itemStart);
            if (start + count > parentTracks.Length)
            {
                count = parentTracks.Length - start;
            }
            if (count <= 0)
            {
                return null;
            }

            var tracks = new float[count];
            float parentTrackSum = 0;
            for (int i = 0; i < count; i++)
            {
                tracks[i] = parentTracks[start + i];
                parentTrackSum += tracks[i];
            }

            // Adjust track sizes for gap delta:
            // Parent allocated: parentTrackSum + (count-1)*parentGap
            // Subgrid needs:    adjustedSum + (count-1)*subgridGap
            // adjustedSum = parentTrackSum + (count-1)*(parentGap - subgridGap)
            float gapDelta = parentGap - subgridGap;
            if (count > 1 && Math.Abs(gapDelta) > 0.001f)
            {
                float totalGapDelta = (count - 1) * gapDelta;
                if (parentTrackSum > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        tracks[i] += totalGapDelta * (tracks[i] / parentTrackSum);
                        if (tracks[i] < 0)
                        {
                            tracks[i] = 0;
                        }
                    }
                }
                else
                {
                    float perTrack = totalGapDelta / count;
                    for (int i = 0; i < count; i++)
                    {
                        tracks[i] = Math.Max(0, perTrack);
                    }
                }
            }

            return tracks;
        }

        // BUG-062: Match FlexLayout.CloneStyleAsBlock — clear visual decoration on anonymous wrappers.
        private static ComputedStyle CloneStyleAsBlock(ComputedStyle source)
        {
            var values = (PropertyValue[])source.GetValues().Clone();
            values[PropertyId.Display] = PropertyValue.FromInt((int)CssDisplay.Block);
            var autoVal = PropertyValue.FromLength(float.NaN);
            values[PropertyId.Width] = autoVal;
            values[PropertyId.Height] = autoVal;
            values[PropertyId.MinWidth] = autoVal;
            values[PropertyId.MinHeight] = autoVal;
            values[PropertyId.MaxWidth] = autoVal;
            values[PropertyId.MaxHeight] = autoVal;

            var refValues = (object?[])source.GetRefValues().Clone();
            refValues[PropertyId.BoxShadow] = null;
            refValues[PropertyId.BackgroundImage] = null;
            var zero = PropertyValue.FromLength(0);
            var transparent = PropertyValue.FromColor(new CssColor(0, 0, 0, 0));
            values[PropertyId.BackgroundColor] = transparent;
            values[PropertyId.BorderTopWidth] = zero;
            values[PropertyId.BorderRightWidth] = zero;
            values[PropertyId.BorderBottomWidth] = zero;
            values[PropertyId.BorderLeftWidth] = zero;
            values[PropertyId.BorderTopLeftRadius] = zero;
            values[PropertyId.BorderTopRightRadius] = zero;
            values[PropertyId.BorderBottomRightRadius] = zero;
            values[PropertyId.BorderBottomLeftRadius] = zero;
            values[PropertyId.PaddingTop] = zero;
            values[PropertyId.PaddingRight] = zero;
            values[PropertyId.PaddingBottom] = zero;
            values[PropertyId.PaddingLeft] = zero;
            values[PropertyId.MarginTop] = zero;
            values[PropertyId.MarginRight] = zero;
            values[PropertyId.MarginBottom] = zero;
            values[PropertyId.MarginLeft] = zero;
            values[PropertyId.Position] = PropertyValue.FromInt((int)CssPosition.Static);
            return new ComputedStyle(values, refValues);
        }

        /// <summary>
        /// [CSS-GRID §12.1] Compute intrinsic width for a grid container.
        /// Collects grid items, parses grid-template-columns, places items,
        /// then sums per-column contributions:
        ///   - Explicit pixel/percent tracks: declared size
        ///   - Auto tracks: max-content width of items in that column
        ///   - fr tracks: 0 contribution (flexible, not intrinsic)
        ///   - auto-fill/auto-fit: 1 repetition for intrinsic sizing
        /// </summary>
        internal static float ComputeIntrinsicWidth(
            StyledElement element, float keyword, float containingWidth, LayoutContext context)
        {
            var style = element.Style;
            float colGap = style.ColumnGap;
            if (DeferredPercent.IsEncoded(colGap))
            {
                colGap = 0;
            }
            if (float.IsNaN(colGap) || colGap < 0)
            {
                colGap = 0;
            }

            var colRaw = style.GetRefValue(PropertyId.GridTemplateColumns);
            var colLineNames = new Dictionary<string, List<int>>();
            var rowLineNames = new Dictionary<string, List<int>>();
            ExtractLineNames(colRaw, colLineNames);

            // [CSS-GRID §12.1] For intrinsic sizing, auto-fill/auto-fit produce 1 repetition.
            // Flatten track definitions with intrinsic-mode flag.
            var flatTrackValues = new List<object>();
            if (colRaw != null && !IsSubgrid(colRaw))
            {
                if (colRaw is CssKeywordValue kw && (kw.Keyword == "none" || kw.Keyword == "auto"))
                {
                    // No explicit columns
                }
                else if (colRaw is CssListValue colList)
                {
                    for (int i = 0; i < colList.Values.Count; i++)
                    {
                        FlattenTrackValueForIntrinsic(colList.Values[i], flatTrackValues, containingWidth);
                    }
                }
                else
                {
                    FlattenTrackValueForIntrinsic(colRaw, flatTrackValues, containingWidth);
                }
            }

            int explicitCols = flatTrackValues.Count;

            // Classify each explicit track for intrinsic sizing
            var trackSizes = new float[explicitCols];
            for (int i = 0; i < explicitCols; i++)
            {
                var trackVal = flatTrackValues[i];

                // [CSS-GRID §7.2.1] auto keyword = minmax(auto, auto) ≈ minmax(min-content, max-content)
                // For intrinsic sizing, auto tracks use content measurement, not fr distribution.
                if (trackVal is CssKeywordValue autoKw && autoKw.Keyword == "auto")
                {
                    trackSizes[i] = -2; // max-content sentinel
                    continue;
                }

                var parsed = ParseTrackValue(trackVal, containingWidth);
                if (parsed.isFr)
                {
                    // [CSS-GRID §12.1] fr tracks for intrinsic sizing: the track's
                    // intrinsic size is max(minmax-floor, items' max-content contribution).
                    // Use -2 sentinel (max-content) so item measurement runs.
                    // The minmax floor is applied as a minimum after measurement.
                    float minFloor = GetMinmaxFloorForIntrinsic(trackVal, containingWidth);
                    if (minFloor > 0)
                    {
                        trackSizes[i] = minFloor; // definite minimum, keep as-is
                    }
                    else
                    {
                        trackSizes[i] = -2; // max-content sentinel: measure items
                    }
                }
                else if (parsed.value < 0)
                {
                    // Intrinsic sentinel (-1=min-content, -2=max-content, -3=fit-content)
                    // Mark as needing content measurement
                    trackSizes[i] = parsed.value;
                }
                else
                {
                    trackSizes[i] = parsed.value;
                }
            }

            // Collect grid items (same logic as Layout, but lightweight — no layout)
            var children = BlockFormattingContext.FlattenContents(element);
            var items = new List<GridItem>();
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child.IsText)
                {
                    var textNode = (StyledText)child;
                    if (string.IsNullOrWhiteSpace(textNode.Text))
                    {
                        continue;
                    }
                    items.Add(new GridItem { OriginalIndex = items.Count });
                    continue;
                }
                if (child is StyledPseudoElement)
                {
                    items.Add(new GridItem { OriginalIndex = items.Count });
                    continue;
                }

                var childEl = (StyledElement)child;
                if (childEl.Style.Display == CssDisplay.None)
                {
                    continue;
                }
                if (childEl.Style.Position == CssPosition.Absolute ||
                    childEl.Style.Position == CssPosition.Fixed)
                {
                    continue;
                }

                var item = new GridItem
                {
                    StyledElement = childEl,
                    Box = new LayoutBox(childEl, BoxType.Block),
                    Order = childEl.Style.Order,
                    OriginalIndex = items.Count
                };
                ParsePlacement(childEl.Style, item, colLineNames, rowLineNames);
                items.Add(item);
            }

            if (items.Count == 0)
            {
                // No items: sum explicit track sizes + gaps
                if (explicitCols > 0)
                {
                    return SumTrackWidthsAndGaps(trackSizes, colGap);
                }
                return 0;
            }

            // Sort by CSS order
            items.Sort((a, b) =>
            {
                int cmp = a.Order.CompareTo(b.Order);
                return cmp != 0 ? cmp : a.OriginalIndex.CompareTo(b.OriginalIndex);
            });

            // Determine grid column count
            int gridCols = Math.Max(1, explicitCols);
            int gridRows = 1;

            // Parse grid-template-areas to get named areas
            Dictionary<string, (int rowStart, int colStart, int rowSpan, int colSpan)>? namedAreas = null;
            var areasRaw = style.GetRefValue(PropertyId.GridTemplateAreas);
            if (areasRaw != null)
            {
                namedAreas = ParseGridTemplateAreas(areasRaw);
            }
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
                foreach (var area in namedAreas.Values)
                {
                    if (area.colStart + area.colSpan > gridCols)
                    {
                        gridCols = area.colStart + area.colSpan;
                    }
                    if (area.rowStart + area.rowSpan > gridRows)
                    {
                        gridRows = area.rowStart + area.rowSpan;
                    }
                }
            }

            // Expand grid from explicit placements
            for (int i = 0; i < items.Count; i++)
            {
                int colEnd = items[i].ColStart >= 0 ? items[i].ColStart + items[i].ColSpan : 0;
                int rowEnd = items[i].RowStart >= 0 ? items[i].RowStart + items[i].RowSpan : 0;
                if (colEnd > gridCols) { gridCols = colEnd; }
                if (rowEnd > gridRows) { gridRows = rowEnd; }
            }

            // Resolve negative line numbers
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
                if (item.RawColEnd != 0)
                {
                    int resolvedEnd = Math.Max(0, ResolveNegativeLine(item.RawColEnd, gridCols));
                    int start = item.ColStart >= 0 ? item.ColStart : 0;
                    if (resolvedEnd > start)
                    {
                        item.ColSpan = resolvedEnd - start;
                    }
                }
                if (item.RawRowEnd != 0)
                {
                    int resolvedEnd = Math.Max(0, ResolveNegativeLine(item.RawRowEnd, gridRows));
                    int start = item.RowStart >= 0 ? item.RowStart : 0;
                    if (resolvedEnd > start)
                    {
                        item.RowSpan = resolvedEnd - start;
                    }
                }
            }

            // If no explicit columns and no explicit placements, determine grid dimensions
            if (explicitCols == 0 && !HasAnyExplicitPlacement(items))
            {
                var rowRaw = style.GetRefValue(PropertyId.GridTemplateRows);
                var explicitRowTracks = ResolveTrackList(rowRaw, 0);
                int explicitRows = explicitRowTracks?.Length ?? 0;
                bool flowColumn = style.GridAutoFlow == CssGridAutoFlow.Column ||
                                  style.GridAutoFlow == CssGridAutoFlow.ColumnDense;
                if (explicitRows > 0)
                {
                    gridCols = Math.Max(1, (int)Math.Ceiling((float)items.Count / explicitRows));
                }
                else if (flowColumn)
                {
                    // Column flow without explicit rows: each item gets its own column
                    gridCols = items.Count;
                    gridRows = 1;
                }
                else
                {
                    gridCols = 1;
                    gridRows = items.Count;
                }
            }

            // Simple auto-placement: place items without definite positions
            var occupied = new bool[gridRows * gridCols * 4];
            int maxRow = gridRows;
            int maxCol = gridCols;

            // Phase 1: definite row+col
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

            // Phase 2: definite row only
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.Placed) { continue; }
                if (item.RowStart >= 0)
                {
                    EnsureGridSize(ref occupied, ref maxRow, ref maxCol, item.RowStart + item.RowSpan, maxCol);
                    int col = FindFreeColumn(occupied, maxCol, item.RowStart, item.ColSpan, item.RowSpan, 0);
                    if (col < 0)
                    {
                        col = maxCol;
                        EnsureGridSize(ref occupied, ref maxRow, ref maxCol, maxRow, col + item.ColSpan);
                    }
                    item.ColStart = col;
                    MarkOccupied(occupied, maxCol, item.RowStart, item.ColStart, item.RowSpan, item.ColSpan);
                    item.Placed = true;
                }
            }

            // Phase 3+4: auto placement (row-major)
            int autoRow = 0;
            int autoCol = 0;
            bool dense = style.GridAutoFlow == CssGridAutoFlow.RowDense ||
                         style.GridAutoFlow == CssGridAutoFlow.ColumnDense;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.Placed) { continue; }
                if (dense)
                {
                    autoRow = 0;
                    autoCol = 0;
                }

                bool found = false;
                if (item.ColStart >= 0)
                {
                    int searchRow = dense ? 0 : autoRow;
                    EnsureGridSize(ref occupied, ref maxRow, ref maxCol, maxRow, item.ColStart + item.ColSpan);
                    int row = FindFreeRow(occupied, maxCol, maxRow, item.ColStart, item.RowSpan, item.ColSpan, searchRow);
                    if (row < 0)
                    {
                        row = maxRow;
                        EnsureGridSize(ref occupied, ref maxRow, ref maxCol, row + item.RowSpan, maxCol);
                    }
                    item.RowStart = row;
                    MarkOccupied(occupied, maxCol, item.RowStart, item.ColStart, item.RowSpan, item.ColSpan);
                    item.Placed = true;
                    autoRow = item.RowStart;
                    autoCol = item.ColStart;
                }
                else
                {
                    int colLimit = Math.Max(1, gridCols - item.ColSpan + 1);
                    for (int r = autoRow; !found; r++)
                    {
                        int startCol = (r == autoRow) ? autoCol : 0;
                        for (int c = startCol; c < colLimit; c++)
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
                        if (r > maxRow + items.Count) { break; }
                    }
                }

                if (!item.Placed)
                {
                    EnsureGridSize(ref occupied, ref maxRow, ref maxCol, maxRow + item.RowSpan, maxCol);
                    item.RowStart = maxRow - item.RowSpan;
                    item.ColStart = 0;
                    item.Placed = true;
                }
            }

            int finalCols = maxCol;

            // Resolve grid-auto-columns for implicit tracks
            float autoColumnSize = 0;
            object? autoColRaw = style.GetRefValue(PropertyId.GridAutoColumns);
            if (autoColRaw != null)
            {
                var autoColTracks = ResolveTrackList(autoColRaw, containingWidth);
                if (autoColTracks != null && autoColTracks.Length > 0 && autoColTracks[0] > 0)
                {
                    autoColumnSize = autoColTracks[0];
                }
            }

            // Build per-column widths array, expanding if needed
            var columnWidths = new float[finalCols];
            for (int c = 0; c < finalCols; c++)
            {
                if (c < trackSizes.Length)
                {
                    columnWidths[c] = trackSizes[c];
                }
                else if (autoColumnSize > 0)
                {
                    // [CSS-GRID §7.2.3] Implicit tracks use grid-auto-columns size
                    columnWidths[c] = autoColumnSize;
                }
                else
                {
                    // No grid-auto-columns: implicit track needs content measurement
                    columnWidths[c] = -2; // max-content sentinel
                }
            }

            // [CSS-GRID §7.2.4.1] Extract fit-content limits
            float[]? fitContentLimits = ExtractFitContentLimits(colRaw, finalCols, containingWidth, colGap);

            // Measure intrinsic column widths from items
            bool isMinContent = keyword == SizingKeyword.MinContent;
            var measuredWidths = new float[finalCols];

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.ColStart < 0 || item.ColStart >= finalCols)
                {
                    continue;
                }

                // Only handle non-spanning items for per-column sizing
                if (item.ColSpan != 1)
                {
                    continue;
                }

                // Only measure for columns that need content measurement
                if (columnWidths[item.ColStart] >= 0)
                {
                    continue;
                }

                float itemOuterWidth = MeasureGridItemOuterWidth(item, isMinContent, containingWidth, context);
                if (itemOuterWidth > measuredWidths[item.ColStart])
                {
                    measuredWidths[item.ColStart] = itemOuterWidth;
                }
            }

            // Replace intrinsic sentinels with measured widths
            for (int c = 0; c < finalCols; c++)
            {
                if (columnWidths[c] >= 0)
                {
                    continue;
                }
                // fr sentinel (deferred): contributes 0
                if (columnWidths[c] <= -999f)
                {
                    columnWidths[c] = 0;
                    continue;
                }
                float measured = measuredWidths[c];
                // [CSS-GRID §7.2.4.1] fit-content clamp
                if (columnWidths[c] <= -2.5f && columnWidths[c] > -3.5f
                    && fitContentLimits != null && c < fitContentLimits.Length
                    && fitContentLimits[c] >= 0)
                {
                    measured = Math.Min(measured, fitContentLimits[c]);
                }
                columnWidths[c] = measured;
            }

            // Handle spanning items: distribute extra width across spanned columns
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.ColSpan <= 1 || item.ColStart < 0)
                {
                    continue;
                }

                float itemOuterWidth = MeasureGridItemOuterWidth(item, isMinContent, containingWidth, context);
                float existingWidth = 0;
                int spannedCount = 0;
                for (int c = item.ColStart; c < item.ColStart + item.ColSpan && c < finalCols; c++)
                {
                    existingWidth += columnWidths[c];
                    spannedCount++;
                }
                if (spannedCount > 1)
                {
                    existingWidth += (spannedCount - 1) * colGap;
                }
                if (itemOuterWidth > existingWidth && spannedCount > 0)
                {
                    float extra = itemOuterWidth - existingWidth;
                    float perCol = extra / spannedCount;
                    for (int c = item.ColStart; c < item.ColStart + item.ColSpan && c < finalCols; c++)
                    {
                        columnWidths[c] += perCol;
                    }
                }
            }

            return SumTrackWidthsAndGaps(columnWidths, colGap);
        }

        /// <summary>
        /// Measures the outer width (content + padding + border + margin) of a grid item
        /// for intrinsic sizing purposes.
        /// </summary>
        private static float MeasureGridItemOuterWidth(
            GridItem item, bool isMinContent, float containingWidth, LayoutContext context)
        {
            if (item.StyledElement == null)
            {
                // Text or pseudo-element item without StyledElement
                return 0;
            }

            var childStyle = item.StyledElement.Style;
            float childWidth = childStyle.Width;

            // Explicit non-auto, non-percentage, non-keyword width
            if (!float.IsNaN(childWidth) && childWidth > 0
                && !DeferredPercent.IsEncoded(childWidth)
                && !SizingKeyword.IsSizingKeyword(childWidth))
            {
                var tempBox = new LayoutBox(item.StyledElement, BoxType.Block);
                BoxModelCalculator.ApplyBoxModel(tempBox, childStyle, containingWidth);
                float outerWidth = childWidth + tempBox.PaddingLeft + tempBox.PaddingRight
                                 + tempBox.BorderLeftWidth + tempBox.BorderRightWidth
                                 + tempBox.MarginLeft + tempBox.MarginRight;
                if (childStyle.BoxSizing == CssBoxSizing.BorderBox)
                {
                    // Width already includes padding+border
                    outerWidth = childWidth + tempBox.MarginLeft + tempBox.MarginRight;
                }
                return outerWidth;
            }

            // Use BFC's MeasureIntrinsicWidth for content measurement
            float sizingKeyword = isMinContent ? SizingKeyword.MinContent : SizingKeyword.MaxContent;
            float contentWidth = BlockFormattingContext.MeasureIntrinsicWidth(
                item.StyledElement, sizingKeyword, containingWidth, context);

            var measureBox = new LayoutBox(item.StyledElement, BoxType.Block);
            BoxModelCalculator.ApplyBoxModel(measureBox, childStyle, containingWidth);
            float outerMeasured = contentWidth + measureBox.PaddingLeft + measureBox.PaddingRight
                                + measureBox.BorderLeftWidth + measureBox.BorderRightWidth
                                + measureBox.MarginLeft + measureBox.MarginRight;
            return outerMeasured;
        }

        /// <summary>
        /// Sums track widths and inter-track gaps.
        /// </summary>
        private static float SumTrackWidthsAndGaps(float[] trackWidths, float gap)
        {
            float total = 0;
            int trackCount = 0;
            for (int i = 0; i < trackWidths.Length; i++)
            {
                if (trackWidths[i] > 0)
                {
                    total += trackWidths[i];
                }
                trackCount++;
            }
            if (trackCount > 1)
            {
                total += (trackCount - 1) * gap;
            }
            return total;
        }

        /// <summary>
        /// [CSS-GRID §12.1] Flattens track values for intrinsic sizing.
        /// Same as FlattenTrackValue but auto-fill/auto-fit always produce 1 repetition
        /// since there is no definite container width to compute repeat count from.
        /// </summary>
        private static void FlattenTrackValueForIntrinsic(object val, List<object> output, float containingWidth)
        {
            if (val is CssFunctionValue fn && fn.Name == "repeat" && fn.Arguments.Count >= 2)
            {
                var first = fn.Arguments[0];
                bool isAutoRepeat = first is CssKeywordValue autoKw &&
                    (autoKw.Keyword == "auto-fill" || autoKw.Keyword == "auto-fit");

                int count;
                if (isAutoRepeat)
                {
                    // [CSS-GRID §12.1] For intrinsic sizing, auto-fill/auto-fit
                    // produce exactly 1 repetition.
                    count = 1;
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

                for (int rep = 0; rep < count; rep++)
                {
                    for (int j = 1; j < fn.Arguments.Count; j++)
                    {
                        var arg = fn.Arguments[j];
                        if (arg is CssListValue innerList)
                        {
                            for (int k = 0; k < innerList.Values.Count; k++)
                            {
                                output.Add(innerList.Values[k]);
                            }
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

        /// <summary>
        /// [CSS-GRID §12.1] Get the minmax minimum for intrinsic sizing.
        /// For minmax(definite, fr): returns the definite minimum.
        /// For minmax(auto/min-content/max-content, fr): returns sentinel for content measurement.
        /// For bare fr: returns 0.
        /// </summary>
        private static float GetMinmaxFloorForIntrinsic(object val, float containerSize)
        {
            if (val is CssFunctionValue fn && fn.Name == "minmax" && fn.Arguments.Count >= 2)
            {
                var minArg = fn.Arguments[0];
                // Check if the minimum is a content-based keyword
                if (minArg is CssKeywordValue minKw)
                {
                    if (minKw.Keyword == "auto" || minKw.Keyword == "min-content")
                    {
                        return -1; // min-content sentinel
                    }
                    if (minKw.Keyword == "max-content")
                    {
                        return -2; // max-content sentinel
                    }
                }
                // Definite minimum (px, %, etc.)
                var minVal = ParseTrackValue(minArg, containerSize);
                if (!minVal.isFr && minVal.value >= 0)
                {
                    return minVal.value;
                }
                return 0;
            }
            // Bare fr track (not inside minmax): contributes 0
            return 0;
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
            /// <summary>Raw negative end line (e.g., -1 for last line). 0 = not set.</summary>
            public int RawRowEnd { get; set; }
            /// <summary>Raw negative end line (e.g., -1 for last line). 0 = not set.</summary>
            public int RawColEnd { get; set; }
            /// <summary>Positive end line (0-based) when start is auto. -1 = not set.</summary>
            public int ExplicitColEnd { get; set; } = -1;
            /// <summary>Positive end line (0-based) when start is auto. -1 = not set.</summary>
            public int ExplicitRowEnd { get; set; } = -1;
            /// <summary>
            /// [CSS-GRID §9] True when the column end is an explicit line number
            /// (not auto or span). Needed for abspos items where spans are auto.
            /// </summary>
            public bool IsColEndExplicitLine { get; set; }
            /// <summary>
            /// [CSS-GRID §9] True when the row end is an explicit line number.
            /// </summary>
            public bool IsRowEndExplicitLine { get; set; }
            public bool Placed { get; set; }
            public int Order { get; set; }
            public int OriginalIndex { get; set; }
            public string? AreaName { get; set; }

            /// <summary>
            /// [CSS-GRID §9] Whether this item has any explicit grid placement
            /// (grid-column or grid-row set to something other than auto).
            /// </summary>
            public bool HasGridPlacement =>
                ColStart >= 0 || RowStart >= 0 || AreaName != null
                || RawColEnd != 0 || RawRowEnd != 0
                || ExplicitColEnd >= 0 || ExplicitRowEnd >= 0;
        }
    }
}
