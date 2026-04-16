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

            // [CSS-WRITING-MODES-3 §6.2] In vertical writing modes the inline axis is
            // physically vertical and the block axis is physically horizontal. Grid's
            // internal arithmetic is written as if the inline axis were "width" and the
            // block axis were "height"; by swapping the physical width/height into those
            // locals on entry we let every subsequent computation stay in logical
            // (inline, block) coordinates without rewriting the track-sizing math.
            CssWritingMode writingMode = style.WritingMode;
            bool isVerticalWM = writingMode == CssWritingMode.VerticalRl
                                || writingMode == CssWritingMode.VerticalLr;
            float containerPhysicalWidth = parent.ContentRect.Width;
            float containerPhysicalHeight = parent.ContentRect.Height;
            float containerPhysicalX = parent.ContentRect.X;
            float containerPhysicalY = parent.ContentRect.Y;

            float containerWidth = isVerticalWM ? containerPhysicalHeight : containerPhysicalWidth;
            float containerHeight = isVerticalWM ? containerPhysicalWidth : containerPhysicalHeight;
            // [CSS-SIZING-3 §5.2] "Definite" height means a height resolved without
            // reference to content. Only definite heights drive row-track stretching
            // and align-content free-space distribution. Max-height on an auto-height
            // container is an upper bound, not a definite height.
            bool containerHeightIsDefinite = !float.IsNaN(containerHeight) && containerHeight > 0;
            // Container height may not be resolved yet (BFC sets it to 0 before LayoutChildren).
            // Resolve from explicit CSS height so fr row tracks work correctly.
            if (!containerHeightIsDefinite)
            {
                float explicitH = DimensionResolver.ResolveHeight(style, float.NaN, parent);
                if (!float.IsNaN(explicitH) && explicitH > 0)
                {
                    containerHeight = explicitH;
                    containerHeightIsDefinite = true;
                }
            }
            if (!containerHeightIsDefinite)
            {
                // [CSS-SIZING §5.2] When height is auto, use max-height as an upper bound
                // for percentage resolution on abspos children (e.g., height: 100%).
                // This is NOT a definite container height — the container still
                // auto-sizes from its row tracks — so `containerHeightIsDefinite`
                // stays false and stretching/align-content logic below is skipped.
                float maxH = style.MaxHeight;
                if (!float.IsNaN(maxH) && maxH > 0)
                {
                    if (style.BoxSizing == CssBoxSizing.BorderBox)
                    {
                        maxH -= parent.PaddingTop + parent.PaddingBottom
                              + parent.BorderTopWidth + parent.BorderBottomWidth;
                        if (maxH < 0) { maxH = 0; }
                    }
                    containerHeight = maxH;
                }
                else
                {
                    containerHeight = 0f; // fr rows will resolve to 0; content sizing handles them
                }
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
                // [CSS-GRID §12.4] Even with no items, explicit tracks define the grid
                // container's intrinsic block size. Compute and set it so BFC/IFC can use it.
                // In vertical writing mode the block axis is physically horizontal so the
                // computed total goes into physical width, not height.
                bool emptyHasNoBlockExtent = isVerticalWM
                    ? containerPhysicalWidth <= 0
                    : containerPhysicalHeight <= 0;
                if (emptyHasNoBlockExtent)
                {
                    var earlyRowTracks = ResolveTrackList(rowRaw, containerHeight, rowGap, rowLineNames);
                    if (earlyRowTracks != null)
                    {
                        float totalEmptyRowH = 0;
                        for (int r = 0; r < earlyRowTracks.Length; r++)
                        {
                            totalEmptyRowH += earlyRowTracks[r];
                            if (r < earlyRowTracks.Length - 1)
                            {
                                totalEmptyRowH += rowGap;
                            }
                        }
                        if (totalEmptyRowH > 0)
                        {
                            if (isVerticalWM)
                            {
                                parent.ContentRect = new RectF(containerPhysicalX, containerPhysicalY,
                                    totalEmptyRowH, containerPhysicalHeight);
                            }
                            else
                            {
                                parent.ContentRect = new RectF(containerPhysicalX, containerPhysicalY,
                                    containerPhysicalWidth, totalEmptyRowH);
                            }
                        }
                    }
                }
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

            // Detect auto-fit for column/row collapsing after placement
            bool isAutoFitCols = HasAutoFit(colRaw);
            bool isAutoFitRows = HasAutoFit(rowRaw);

            // Read auto-flow direction
            var autoFlow = style.GridAutoFlow;
            bool flowColumn = autoFlow == CssGridAutoFlow.Column || autoFlow == CssGridAutoFlow.ColumnDense;
            bool dense = autoFlow == CssGridAutoFlow.RowDense || autoFlow == CssGridAutoFlow.ColumnDense;

            // Parse grid-template-areas if present
            Dictionary<string, (int rowStart, int colStart, int rowSpan, int colSpan)>? namedAreas = null;
            var areasRaw = style.GetRefValue(PropertyId.GridTemplateAreas);
            if (areasRaw != null)
            {
                namedAreas = ParseGridTemplateAreas(areasRaw);

                // [CSS-GRID §7.3] Generate implicit line names from named areas.
                // Each area "foo" creates lines "foo-start" and "foo-end" for both axes.
                if (namedAreas != null)
                {
                    foreach (var kvp in namedAreas)
                    {
                        string startName = kvp.Key + "-start";
                        string endName = kvp.Key + "-end";

                        if (!colLineNames.ContainsKey(startName))
                        {
                            colLineNames[startName] = new List<int>();
                        }
                        if (!colLineNames[startName].Contains(kvp.Value.colStart))
                        {
                            colLineNames[startName].Add(kvp.Value.colStart);
                        }

                        if (!colLineNames.ContainsKey(endName))
                        {
                            colLineNames[endName] = new List<int>();
                        }
                        int colEnd = kvp.Value.colStart + kvp.Value.colSpan;
                        if (!colLineNames[endName].Contains(colEnd))
                        {
                            colLineNames[endName].Add(colEnd);
                        }

                        if (!rowLineNames.ContainsKey(startName))
                        {
                            rowLineNames[startName] = new List<int>();
                        }
                        if (!rowLineNames[startName].Contains(kvp.Value.rowStart))
                        {
                            rowLineNames[startName].Add(kvp.Value.rowStart);
                        }

                        if (!rowLineNames.ContainsKey(endName))
                        {
                            rowLineNames[endName] = new List<int>();
                        }
                        int rowEnd = kvp.Value.rowStart + kvp.Value.rowSpan;
                        if (!rowLineNames[endName].Contains(rowEnd))
                        {
                            rowLineNames[endName].Add(rowEnd);
                        }
                    }
                }
            }

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
            bool[]? collapsedCols = null;
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
                collapsedCols = new bool[explicitColTracks.Length];
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
                        collapsedCols[c] = true;
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
            bool[] isImplicitAutoCol;
            if (isSubgridCols && subgridColTracks != null && subgridColTracks.Length >= finalCols)
            {
                colWidths = new float[finalCols];
                Array.Copy(subgridColTracks, colWidths, finalCols);
                isImplicitAutoCol = new bool[finalCols];
            }
            else
            {
                colWidths = BuildTrackSizes(autoFitColTracks, finalCols, containerWidth,
                    colGap, style.GetRefValue(PropertyId.GridAutoColumns), containerWidth,
                    out isImplicitAutoCol);
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
                // [CSS-GRID §11.5] Track sizing pass 1: non-spanning items
                float[] intrinsicWidths = new float[finalCols];
                float[] minContentWidths = new float[finalCols];
                // Track which columns were intrinsic sentinels (for spanning distribution)
                bool[] wasIntrinsic = new bool[finalCols];
                for (int c = 0; c < finalCols; c++)
                {
                    if (colWidths[c] >= -3.5f && colWidths[c] < 0)
                    {
                        wasIntrinsic[c] = true;
                    }
                }

                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item.ColSpan != 1 || item.ColStart < 0 || item.ColStart >= finalCols)
                    {
                        continue;
                    }
                    if (colWidths[item.ColStart] >= 0)
                    {
                        continue; // not an intrinsic track
                    }

                    if (item.StyledElement == null) { continue; }

                    bool isMinContent = colWidths[item.ColStart] == -1;
                    float keyword = isMinContent ? SizingKeyword.MinContent : SizingKeyword.MaxContent;

                    // [CSS-GRID-1 §11.5.1] Intrinsic size contributions: for the max-content
                    // contribution, if the item has a definite used inline-size that resolves
                    // against the grid container's inline size, use that resolved size instead
                    // of the item's content-based max-content. This preserves the behaviour of
                    // 'width: 100%' items (common for items wrapping text inside fixed-width
                    // grids) — they should contribute 100% of the container, not the entire
                    // natural max-content width of the text they contain. The result is still
                    // floored by min-content below.
                    float measured;
                    float itemStyleWidth = item.StyledElement.Style.Width;
                    float resolvedItemWidth = float.NaN;
                    if (!isMinContent
                        && !float.IsNaN(itemStyleWidth)
                        && !SizingKeyword.IsSizingKeyword(itemStyleWidth))
                    {
                        if (DeferredPercent.IsEncoded(itemStyleWidth))
                        {
                            resolvedItemWidth = DeferredPercent.Resolve(itemStyleWidth, containerWidth);
                        }
                        else if (itemStyleWidth >= 0)
                        {
                            resolvedItemWidth = itemStyleWidth;
                        }
                    }
                    if (!float.IsNaN(resolvedItemWidth) && resolvedItemWidth >= 0)
                    {
                        measured = resolvedItemWidth;
                        // Floor by min-content so nothing shrinks below an unbreakable content piece.
                        float minContent = BlockFormattingContext.MeasureIntrinsicWidth(
                            item.StyledElement, SizingKeyword.MinContent, containerWidth, context);
                        if (minContent > measured)
                        {
                            measured = minContent;
                        }
                    }
                    else
                    {
                        measured = BlockFormattingContext.MeasureIntrinsicWidth(
                            item.StyledElement, keyword, containerWidth, context);
                    }
                    // Add horizontal box model spacing
                    var tempBox = new LayoutBox(item.StyledElement, BoxType.Block);
                    BoxModelCalculator.ApplyBoxModel(tempBox, item.StyledElement.Style, containerWidth);
                    float boxSpacing = tempBox.PaddingLeft + tempBox.PaddingRight
                                     + tempBox.BorderLeftWidth + tempBox.BorderRightWidth
                                     + tempBox.MarginLeft + tempBox.MarginRight;
                    measured += boxSpacing;
                    if (measured > intrinsicWidths[item.ColStart])
                    {
                        intrinsicWidths[item.ColStart] = measured;
                    }

                    // [CSS-GRID §7.2.4.1] fit-content needs min-content as auto minimum floor.
                    // [CSS-GRID-1 §11.4] Implicit auto tracks also need min-content as the
                    // base size so the §11.4 maximize step can cap their growth without
                    // shrinking past the unbreakable piece of content.
                    bool isFitContent = colWidths[item.ColStart] <= -2.5f
                                     && colWidths[item.ColStart] > -3.5f;
                    bool needsMinContentFloor = (isFitContent || isImplicitAutoCol[item.ColStart])
                                             && !isMinContent;
                    if (needsMinContentFloor)
                    {
                        float minMeasured = BlockFormattingContext.MeasureIntrinsicWidth(
                            item.StyledElement, SizingKeyword.MinContent, containerWidth, context);
                        minMeasured += boxSpacing;
                        if (minMeasured > minContentWidths[item.ColStart])
                        {
                            minContentWidths[item.ColStart] = minMeasured;
                        }
                    }
                }

                // Replace intrinsic sentinels with measured widths
                for (int c = 0; c < finalCols; c++)
                {
                    if (colWidths[c] >= -3.5f && colWidths[c] < 0)
                    {
                        float measured = intrinsicWidths[c];
                        // [CSS-GRID §7.2.4.1] fit-content: max(auto_min, min(max_content, limit))
                        if (colWidths[c] <= -2.5f && colWidths[c] > -3.5f
                            && fitContentColLimits != null && c < fitContentColLimits.Length
                            && fitContentColLimits[c] >= 0)
                        {
                            measured = Math.Max(minContentWidths[c],
                                Math.Min(measured, fitContentColLimits[c]));
                        }
                        colWidths[c] = measured;
                    }
                }

                // [CSS-GRID-1 §11.4 Maximize Tracks] Implicit auto tracks default to
                // minmax(min-content, max-content). The base size is min-content and
                // the growth limit is max-content. When the sum of max-content sizes
                // exceeds the container's available inline space, cap each implicit
                // auto track so the total fits, but never shrink past min-content —
                // an unbreakable piece of content still has to appear.
                // https://drafts.csswg.org/css-grid-1/#algo-grow-tracks
                if (containerWidth > 0 && finalCols > 0)
                {
                    int implicitTrackCount = 0;
                    float nonImplicitTrackSum = 0;
                    for (int c = 0; c < finalCols; c++)
                    {
                        if (isImplicitAutoCol[c])
                        {
                            implicitTrackCount++;
                        }
                        else
                        {
                            nonImplicitTrackSum += colWidths[c];
                        }
                    }
                    if (implicitTrackCount > 0)
                    {
                        float gapSpace = finalCols > 1 ? (finalCols - 1) * colGap : 0;
                        float availableForImplicit = containerWidth - nonImplicitTrackSum - gapSpace;
                        float currentImplicitSum = 0;
                        for (int c = 0; c < finalCols; c++)
                        {
                            if (isImplicitAutoCol[c])
                            {
                                currentImplicitSum += colWidths[c];
                            }
                        }
                        if (currentImplicitSum > availableForImplicit && availableForImplicit > 0)
                        {
                            float perTrackCap = availableForImplicit / implicitTrackCount;
                            for (int c = 0; c < finalCols; c++)
                            {
                                if (!isImplicitAutoCol[c]) { continue; }
                                float minFloor = minContentWidths[c];
                                float capped = Math.Min(colWidths[c], perTrackCap);
                                if (capped < minFloor)
                                {
                                    capped = minFloor;
                                }
                                colWidths[c] = capped;
                            }
                        }
                    }
                }

                // [CSS-GRID §11.5] Track sizing pass 2: spanning items
                // Distribute extra width across spanned intrinsic columns.
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item.ColSpan <= 1 || item.ColStart < 0)
                    {
                        continue;
                    }
                    if (item.StyledElement == null) { continue; }

                    // Check if any spanned column was intrinsic
                    bool hasIntrinsicSpan = false;
                    for (int c = item.ColStart; c < item.ColStart + item.ColSpan && c < finalCols; c++)
                    {
                        if (wasIntrinsic[c]) { hasIntrinsicSpan = true; break; }
                    }
                    if (!hasIntrinsicSpan) { continue; }

                    float itemWidth = BlockFormattingContext.MeasureIntrinsicWidth(
                        item.StyledElement, SizingKeyword.MaxContent, containerWidth, context);
                    var spanBox = new LayoutBox(item.StyledElement, BoxType.Block);
                    BoxModelCalculator.ApplyBoxModel(spanBox, item.StyledElement.Style, containerWidth);
                    itemWidth += spanBox.PaddingLeft + spanBox.PaddingRight
                               + spanBox.BorderLeftWidth + spanBox.BorderRightWidth
                               + spanBox.MarginLeft + spanBox.MarginRight;

                    // Sum existing track widths + gaps
                    float existingWidth = 0;
                    int intrinsicCount = 0;
                    int spannedCount = 0;
                    for (int c = item.ColStart; c < item.ColStart + item.ColSpan && c < finalCols; c++)
                    {
                        existingWidth += colWidths[c];
                        if (wasIntrinsic[c]) { intrinsicCount++; }
                        spannedCount++;
                    }
                    if (spannedCount > 1)
                    {
                        existingWidth += (spannedCount - 1) * colGap;
                    }

                    if (itemWidth > existingWidth && intrinsicCount > 0)
                    {
                        float extra = itemWidth - existingWidth;
                        float perCol = extra / intrinsicCount;
                        for (int c = item.ColStart; c < item.ColStart + item.ColSpan && c < finalCols; c++)
                        {
                            if (!wasIntrinsic[c]) { continue; }
                            float growth = perCol;
                            // [CSS-GRID §7.2.4.1] Respect fit-content limit on growth
                            bool isFitContent = fitContentColLimits != null
                                             && c < fitContentColLimits.Length
                                             && fitContentColLimits[c] >= 0;
                            if (isFitContent && fitContentColLimits != null)
                            {
                                float maxSize = Math.Max(minContentWidths[c], fitContentColLimits![c]);
                                float room = maxSize - colWidths[c];
                                if (room < growth) { growth = Math.Max(0, room); }
                            }
                            colWidths[c] += growth;
                        }
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

            // [CSS-GRID-1 §11.8] Stretch auto tracks: when the grid container has a
            // definite inline size larger than the sum of the content-sized implicit
            // auto tracks, distribute the remaining free space equally among those
            // tracks so they fill the container. Only fires when justify-content is
            // 'normal' (which behaves as 'stretch' for grid per CSS-ALIGN-3 §6.1)
            // or explicit 'stretch'. Explicit start/end/center/space-* values leave
            // the tracks at their content-based sizes and instead position them
            // via justify-content-offset below. The parser maps both unset and the
            // 'normal' keyword to CssJustifyContent.FlexStart, so that sentinel is
            // what we treat as the grid default here.
            {
                var jcForStretch = style.JustifyContent;
                bool stretchByDefault = jcForStretch == CssJustifyContent.FlexStart
                                      || jcForStretch == CssJustifyContent.Stretch;
                if (stretchByDefault)
                {
                    int stretchableCount = 0;
                    for (int c = 0; c < finalCols; c++)
                    {
                        if (isImplicitAutoCol[c])
                        {
                            stretchableCount++;
                        }
                    }
                    if (stretchableCount > 0)
                    {
                        float usedWidth = 0;
                        for (int c = 0; c < finalCols; c++)
                        {
                            usedWidth += colWidths[c];
                        }
                        float gapSpace = finalCols > 1 ? (finalCols - 1) * colGap : 0;
                        float freeSpace = containerWidth - usedWidth - gapSpace;
                        if (freeSpace > 0)
                        {
                            float stretchPerTrack = freeSpace / stretchableCount;
                            for (int c = 0; c < finalCols; c++)
                            {
                                if (isImplicitAutoCol[c])
                                {
                                    colWidths[c] += stretchPerTrack;
                                }
                            }
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
                        else if (widthAutoForGrid && heightAutoForGrid && itemAr > 0)
                        {
                            // [CSS-GRID §6.4] Grid items with aspect-ratio and both axes
                            // auto stretch to fill the track's inline dimension by default.
                            // The stretched track width takes precedence over the
                            // max-height→max-width transfer that ResolveWidth would apply
                            // (CSS Sizing 4 §5.2.1: stretched auto sizes are not clamped
                            // by transferred max-* sizes from the other axis in Chrome).
                            //
                            // This branch only runs when height is also auto; if height is
                            // explicit, ResolveWidth's aspect-ratio transfer path already
                            // returns the correct preferred size (height × ratio), so we
                            // must fall through to ResolveWidth in that case.
                            CssAlignItems justifySelfInline = item.StyledElement.Style.JustifySelf;
                            if (justifySelfInline == CssAlignItems.Normal || (int)justifySelfInline > (int)CssAlignItems.Normal)
                            {
                                justifySelfInline = style.JustifyItems;
                            }
                            bool gridItemIsStretched = IsStretch(justifySelfInline);
                            if (gridItemIsStretched)
                            {
                                contentWidth = cellWidth - item.Box.PaddingLeft - item.Box.PaddingRight
                                               - item.Box.BorderLeftWidth - item.Box.BorderRightWidth
                                               - item.Box.MarginLeft - item.Box.MarginRight;
                            }
                            else
                            {
                                contentWidth = DimensionResolver.ResolveWidth(item.StyledElement.Style, cellWidth, item.Box);
                            }
                        }
                        else if (widthAutoForGrid)
                        {
                            // [CSS-GRID §11.1] [CSS-ALIGN-3 §9.3] A grid item's
                            // automatic inline size fills its track only when the
                            // effective justify-self is stretch (or normal).
                            // Otherwise the item is sized to its max-content
                            // contribution, clamped to the available track width.
                            // Baseline falls back to start when the item does not
                            // participate in a shared baseline context, so it also
                            // uses intrinsic sizing here.
                            CssAlignItems effectiveJustifySelf = item.StyledElement.Style.JustifySelf;
                            if (effectiveJustifySelf == CssAlignItems.Normal || (int)effectiveJustifySelf > (int)CssAlignItems.Normal)
                            {
                                effectiveJustifySelf = style.JustifyItems;
                            }
                            if (IsStretch(effectiveJustifySelf))
                            {
                                contentWidth = DimensionResolver.ResolveWidth(item.StyledElement.Style, cellWidth, item.Box);
                            }
                            else
                            {
                                float availableWidth = cellWidth - item.Box.PaddingLeft - item.Box.PaddingRight
                                                       - item.Box.BorderLeftWidth - item.Box.BorderRightWidth
                                                       - item.Box.MarginLeft - item.Box.MarginRight;
                                float maxContentWidth = BlockFormattingContext.MeasureIntrinsicWidth(
                                    item.StyledElement, SizingKeyword.MaxContent, cellWidth, context);
                                contentWidth = Math.Min(maxContentWidth, Math.Max(0, availableWidth));
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
                    // [CSS-WRITING-MODES-3 §6.2] In vertical writing mode the inline axis is
                    // physically vertical so the child BFC must read its inline size from
                    // the box's physical Height. Swap the (logical inline, logical block)
                    // values into (physical W=block, physical H=inline) before laying out.
                    if (isVerticalWM)
                    {
                        item.Box.ContentRect = new RectF(0, 0, preHeight, contentWidth);
                    }
                    else
                    {
                        item.Box.ContentRect = new RectF(0, 0, contentWidth, preHeight);
                    }

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
                    {
                        contentHeight = CalculateAutoHeight(item.Box);

                        // [CSS-GRID §6.5] Automatic minimum size clamping: when the item
                        // has min-height:auto and the track has a definite max sizing
                        // function (fixed track), clamp the auto height to the track size.
                        if (float.IsNaN(item.StyledElement.Style.MinHeight) && explicitRowTracks != null
                            && item.StyledElement.Style.OverflowY == CssOverflow.Visible)
                        {
                            float trackMaxH = 0;
                            for (int r = item.RowStart; r < item.RowStart + item.RowSpan && r < explicitRowTracks.Length; r++)
                            {
                                trackMaxH += explicitRowTracks[r];
                            }
                            if (trackMaxH > 0 && contentHeight > trackMaxH)
                            {
                                contentHeight = trackMaxH;
                            }
                        }
                    }

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

            // [CSS-GRID-2 §8] Subgrid per-row contributions to parent rows.
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.Box.SubgridRowHeights == null || item.RowSpan <= 1)
                {
                    continue;
                }
                float[] subH = item.Box.SubgridRowHeights;
                for (int sr = 0; sr < subH.Length && sr < item.RowSpan; sr++)
                {
                    int parentRow = item.RowStart + sr;
                    if (parentRow >= 0 && parentRow < finalRows && subH[sr] > rowHeights[parentRow])
                    {
                        rowHeights[parentRow] = subH[sr];
                    }
                }
            }

            // [CSS-GRID §7.2.4.1] Extract fit-content row limits early for auto minimum save.
            float[]? fitContentRowLimits = ExtractFitContentLimits(
                rowRaw, finalRows, containerHeight, rowGap);
            // Save auto minimum (non-spanning content heights) for fit-content floor.
            // Must be saved BEFORE spanning distribution inflates row heights.
            float[]? autoMinRowHeights = null;
            if (fitContentRowLimits != null)
            {
                autoMinRowHeights = new float[finalRows];
                Array.Copy(rowHeights, autoMinRowHeights, finalRows);
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
                // [CSS-GRID-2 §8] Store content-based row heights BEFORE applying
                // inherited tracks. Parent uses these for its own row sizing.
                parent.SubgridRowHeights = new float[finalRows];
                Array.Copy(rowHeights, parent.SubgridRowHeights, finalRows);

                // Subgridded rows: use inherited sizes, but keep content heights
                // when they exceed inherited (parent auto rows start at 0).
                for (int r = 0; r < Math.Min(subgridRowTracks.Length, finalRows); r++)
                {
                    rowHeights[r] = Math.Max(rowHeights[r], subgridRowTracks[r]);
                }
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

            // [CSS-GRID §7.2.4.1] Apply fit-content row limits.
            // fit-content(limit) = max(auto_min, min(limit, max_content))
            // auto_min = content height from non-spanning items (saved before spanning distribution).
            if (fitContentRowLimits != null && autoMinRowHeights != null)
            {
                for (int r = 0; r < finalRows; r++)
                {
                    if (fitContentRowLimits[r] < 0) { continue; }
                    // auto_min floor from non-spanning content; limit caps spanning growth.
                    float autoMin = autoMinRowHeights[r];
                    float capped = Math.Min(rowHeights[r], fitContentRowLimits[r]);
                    rowHeights[r] = Math.Max(autoMin, capped);
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
            // natural size and are offset instead. Only definite container heights
            // trigger stretching — max-height fallback (auto-height container) must not.
            if (containerHeightIsDefinite && finalRows > 0)
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

            // [CSS-GRID-1 §7.2.3.1] auto-fit rows: collapse empty explicit rows to 0.
            bool[]? collapsedRows = null;
            if (isAutoFitRows && explicitRowTracks != null && explicitRowTracks.Length > 0)
            {
                var occupiedRows = new bool[explicitRowTracks.Length];
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item.RowStart >= 0)
                    {
                        for (int rs = 0; rs < item.RowSpan; rs++)
                        {
                            int rowIdx = item.RowStart + rs;
                            if (rowIdx < occupiedRows.Length)
                            {
                                occupiedRows[rowIdx] = true;
                            }
                        }
                    }
                }
                collapsedRows = new bool[explicitRowTracks.Length];
                for (int r = 0; r < explicitRowTracks.Length && r < finalRows; r++)
                {
                    if (!occupiedRows[r])
                    {
                        collapsedRows[r] = true;
                        rowHeights[r] = 0;
                    }
                }
            }

            // Read container-level alignment defaults
            CssAlignItems containerAlignItems = style.AlignItems;
            CssAlignItems containerJustifyItems = style.JustifyItems;

            // [CSS-SIZING-3 §5.2.2] Percent heights on grid items resolve against
            // the grid area size. The first-pass item layout used explicitRowTracks
            // as a pre-estimate, but auto rows in an inline-grid with no definite
            // container height still report 0 at that point. Now that rowHeights[]
            // reflects sibling content contributions, re-resolve any unresolved
            // percent heights and re-layout the inner tree so descendants see the
            // final definite block size.
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.StyledElement == null)
                {
                    continue;
                }
                if (!DeferredPercent.IsEncoded(item.StyledElement.Style.Height))
                {
                    continue;
                }

                float spanRowHeight = 0;
                for (int r = item.RowStart; r < item.RowStart + item.RowSpan && r < finalRows; r++)
                {
                    spanRowHeight += rowHeights[r];
                }
                if (item.RowSpan > 1)
                {
                    spanRowHeight += (item.RowSpan - 1) * rowGap;
                }
                if (spanRowHeight <= 0)
                {
                    continue;
                }

                float cbHeight = spanRowHeight
                    - item.Box.PaddingTop - item.Box.PaddingBottom
                    - item.Box.BorderTopWidth - item.Box.BorderBottomWidth;
                if (cbHeight < 0)
                {
                    cbHeight = 0;
                }

                float resolvedHeight = DimensionResolver.ResolveHeight(
                    item.StyledElement.Style, cbHeight, item.Box);
                if (float.IsNaN(resolvedHeight))
                {
                    continue;
                }
                if (Math.Abs(resolvedHeight - item.ContentHeight) < 0.01f)
                {
                    continue;
                }

                item.ContentHeight = resolvedHeight;
                item.Box.ContentRect = new RectF(0, 0, item.ContentWidth, resolvedHeight);
                item.Box.ClearChildren();
                item.Box.LineBoxes = null;
                var savedFloatCtx = context.FloatContext;
                context.FloatContext = new FloatContext(0, item.ContentWidth);
                BlockFormattingContext.LayoutChildren(item.Box, context);
                context.FloatContext = savedFloatCtx;
            }

            // Update item box dimensions to final resolved sizes before baseline
            // computation. First-pass layout leaves ContentRect at auto height;
            // the resolved contentWidth/contentHeight may differ (explicit CSS height).
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Box.ContentRect = new RectF(0, 0, items[i].ContentWidth, items[i].ContentHeight);
            }

            // [CSS-GRID §10.1] [CSS-ALIGN-3 §9.4.4] Compute per-row baseline sharing
            // groups for baseline alignment. Items with align-self:baseline in the
            // same row share a baseline group.
            //
            // A row may contain up to two groups:
            // - "Start" group: items whose block-flow direction matches the grid's
            //   (parallel-same and horizontal-tb grids' orthogonal items). These
            //   align to a common baseline anchored at the row's block-start edge.
            // - "End" group: items with the opposite block-flow direction of the
            //   grid (vertical-lr item in vertical-rl grid, or vice versa). These
            //   align to a common baseline anchored at the row's block-end edge.
            //
            // The row's block-axis size is grown to fit the larger of the two
            // group extents; the groups are independent and may overlap in the
            // middle of the row when the row is oversized.
            //
            // [CSS-WRITING-MODES-3 §6.2] The block-axis extent per-item is collected
            // via the logical block-side accessors so the same loop handles
            // horizontal-tb, vertical-lr and vertical-rl grids without physical
            // top/bottom assumptions leaking in.
            float[]? rowMaxBaselineStart = null;
            float[]? rowMaxBaselineEnd = null;
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
                    rowMaxBaselineStart = new float[finalRows];
                    float[] rowMaxDescentStart = new float[finalRows];
                    rowMaxBaselineEnd = new float[finalRows];
                    float[] rowMaxAscentEnd = new float[finalRows];

                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        if (item.RowSpan != 1) { continue; }
                        int row = item.RowStart;
                        if (row < 0 || row >= finalRows) { continue; }

                        CssAlignItems itemAlign = ResolveItemBlockAlignment(item, containerAlignItems);
                        if (itemAlign != CssAlignItems.Baseline) { continue; }

                        // [CSS-ALIGN-3 §9.3] Replaced elements and orthogonal items
                        // in a vertical-WM grid do not join the row sharing group
                        // (they fall back to start alignment); orthogonal items
                        // in a horizontal-tb grid still join via Scope 1b border-end
                        // synthesis, matching Chrome's inline-block emulation.
                        if (!ItemParticipatesInRowBaselineSharingGroup(item, writingMode))
                        {
                            continue;
                        }

                        float baseline = ComputeItemFirstBaselineInBlockAxis(item, writingMode);
                        float outerBlockSize = ComputeItemOuterBlockSize(item, writingMode);

                        if (IsItemOppositeBlockDirection(item, writingMode))
                        {
                            // [CSS-ALIGN-3 §9.4.4] Opposing-direction items anchor
                            // their (projected) first baseline against the row's
                            // block-end edge. `baselineFromEnd` is the distance
                            // from the item's margin-box block-end to its baseline;
                            // `baseline` (from block-start) becomes the "ascent"
                            // contribution for sizing the end-anchored group.
                            float baselineFromEnd = outerBlockSize - baseline;
                            if (baselineFromEnd > rowMaxBaselineEnd[row])
                            {
                                rowMaxBaselineEnd[row] = baselineFromEnd;
                            }
                            if (baseline > rowMaxAscentEnd[row])
                            {
                                rowMaxAscentEnd[row] = baseline;
                            }
                        }
                        else
                        {
                            float descent = outerBlockSize - baseline;
                            if (baseline > rowMaxBaselineStart[row])
                            {
                                rowMaxBaselineStart[row] = baseline;
                            }
                            if (descent > rowMaxDescentStart[row])
                            {
                                rowMaxDescentStart[row] = descent;
                            }
                        }
                    }

                    for (int r = 0; r < finalRows; r++)
                    {
                        float neededStart = rowMaxBaselineStart[r] + rowMaxDescentStart[r];
                        float neededEnd = rowMaxBaselineEnd[r] + rowMaxAscentEnd[r];
                        float needed = Math.Max(neededStart, neededEnd);
                        if (needed > rowHeights[r])
                        {
                            rowHeights[r] = needed;
                        }
                    }
                }
            }

            // [CSS-GRID §10.1] [CSS-ALIGN-3 §9.3] Compute per-column baseline groups for
            // inline-axis baseline alignment (justify-self / justify-items: baseline).
            // Items with justify-self:baseline in the same column share a baseline group
            // anchored to a common X position; the column width may grow to accommodate
            // baseline-shifted items. Parallel items (same writing mode as the grid)
            // synthesize at the inline-start edge, so only orthogonal items participate
            // in a non-degenerate way; the math still runs uniformly to let the track
            // grow when an orthogonal item's descent pushes past the end of its column.
            float[]? colMaxBaselines = null;
            {
                bool hasColumnBaselineAlignment = !isVerticalWM
                    && containerJustifyItems == CssAlignItems.Baseline;
                if (!hasColumnBaselineAlignment && !isVerticalWM)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        var styledEl = items[i].StyledElement;
                        if (styledEl != null
                            && styledEl.Style.JustifySelf == CssAlignItems.Baseline)
                        {
                            hasColumnBaselineAlignment = true;
                            break;
                        }
                    }
                }

                if (hasColumnBaselineAlignment)
                {
                    colMaxBaselines = new float[finalCols];
                    float[] colMaxDescents = new float[finalCols];

                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        if (item.ColSpan != 1) { continue; }
                        int col = item.ColStart;
                        if (col < 0 || col >= finalCols) { continue; }

                        CssAlignItems itemJustify = ResolveItemInlineAlignment(item, containerJustifyItems);
                        if (itemJustify != CssAlignItems.Baseline) { continue; }

                        // [CSS-ALIGN-3 §9.3] Replaced elements fall back to start,
                        // matching the row-baseline treatment above.
                        if (HasSynthesizedBaselineOnly(item)) { continue; }

                        float baseline = ComputeItemBaselineFromMarginLeft(item, isVerticalWM);
                        float outerWidth = item.ContentWidth
                            + item.Box.PaddingLeft + item.Box.PaddingRight
                            + item.Box.BorderLeftWidth + item.Box.BorderRightWidth
                            + item.Box.MarginLeft + item.Box.MarginRight;
                        float descent = outerWidth - baseline;

                        if (baseline > colMaxBaselines[col])
                        {
                            colMaxBaselines[col] = baseline;
                        }
                        if (descent > colMaxDescents[col])
                        {
                            colMaxDescents[col] = descent;
                        }
                    }

                    for (int c = 0; c < finalCols; c++)
                    {
                        float needed = colMaxBaselines[c] + colMaxDescents[c];
                        if (needed > colWidths[c])
                        {
                            colWidths[c] = needed;
                        }
                    }
                }
            }

            // Compute justify-content offset and gap adjustment (horizontal track alignment)
            float justifyContentOffset = 0;
            float effectiveColGap = colGap;
            {
                // [CSS-GRID §7.2.3.1] Collapsed auto-fit tracks have their gutters
                // collapsed too. Count only non-collapsed tracks for gap math.
                float totalColW = 0;
                int nonCollapsedCols = 0;
                for (int c = 0; c < finalCols; c++)
                {
                    totalColW += colWidths[c];
                    if (collapsedCols == null || c >= collapsedCols.Length || !collapsedCols[c])
                    {
                        nonCollapsedCols++;
                    }
                }
                int gapCols = nonCollapsedCols > 0 ? nonCollapsedCols : finalCols;
                totalColW += Math.Max(0, gapCols - 1) * colGap;
                float freeInline = containerWidth - totalColW;
                int distributionCols = nonCollapsedCols > 0 ? nonCollapsedCols : finalCols;
                if (freeInline > 1f)
                {
                    var jc = style.JustifyContent;
                    if (jc == CssJustifyContent.Stretch && distributionCols > 0)
                    {
                        // [CSS-ALIGN §5.3.4] Distribute free space equally to non-collapsed columns
                        float perCol = freeInline / distributionCols;
                        for (int c = 0; c < finalCols; c++)
                        {
                            if (collapsedCols == null || c >= collapsedCols.Length || !collapsedCols[c])
                            {
                                colWidths[c] += perCol;
                            }
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
                    else if (jc == CssJustifyContent.SpaceBetween && distributionCols > 1)
                    {
                        effectiveColGap = colGap + freeInline / (distributionCols - 1);
                    }
                    else if (jc == CssJustifyContent.SpaceAround && distributionCols > 0)
                    {
                        float perCol = freeInline / distributionCols;
                        justifyContentOffset = perCol / 2f;
                        effectiveColGap = colGap + perCol;
                    }
                    else if (jc == CssJustifyContent.SpaceEvenly && distributionCols > 0)
                    {
                        float slot = freeInline / (distributionCols + 1);
                        justifyContentOffset = slot;
                        effectiveColGap = colGap + slot;
                    }
                }
            }

            // Compute align-content offset and gap adjustment (vertical track alignment)
            // Only runs when the container has a definite block size — max-height
            // fallback doesn't create free space for alignment distribution.
            float alignContentOffset = 0;
            float effectiveRowGap = rowGap;
            if (containerHeightIsDefinite)
            {
                // [CSS-GRID §7.2.3.1] Count non-collapsed rows for gap calculation.
                float totalRowH = 0;
                int nonCollapsedRows = 0;
                for (int r = 0; r < finalRows; r++)
                {
                    totalRowH += rowHeights[r];
                    if (collapsedRows == null || r >= collapsedRows.Length || !collapsedRows[r])
                    {
                        nonCollapsedRows++;
                    }
                }
                int gapRows = nonCollapsedRows > 0 ? nonCollapsedRows : finalRows;
                totalRowH += Math.Max(0, gapRows - 1) * rowGap;
                int distributionRows = nonCollapsedRows > 0 ? nonCollapsedRows : finalRows;
                float freeBlock = containerHeight - totalRowH;
                if (freeBlock > 0)
                {
                    var ac = style.AlignContent;
                    if (ac == CssAlignItems.Center)
                        alignContentOffset = freeBlock / 2f;
                    else if (ac == CssAlignItems.End || ac == CssAlignItems.FlexEnd)
                        alignContentOffset = freeBlock;
                    else if (ac == CssAlignItems.SpaceBetween && distributionRows > 1)
                        effectiveRowGap = rowGap + freeBlock / (distributionRows - 1);
                    else if (ac == CssAlignItems.SpaceAround && distributionRows > 0)
                    {
                        float perRow = freeBlock / distributionRows;
                        alignContentOffset = perRow / 2f;
                        effectiveRowGap = rowGap + perRow;
                    }
                    else if (ac == CssAlignItems.SpaceEvenly && distributionRows > 0)
                    {
                        float slot = freeBlock / (distributionRows + 1);
                        alignContentOffset = slot;
                        effectiveRowGap = rowGap + slot;
                    }
                }
            }

            // Second pass: position items
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                // Logical inline-axis cumulative start of the cell within the container's
                // content box (no physical origin baked in — that is added at the final
                // logical→physical mapping below).
                float x = justifyContentOffset;
                for (int c = 0; c < item.ColStart && c < finalCols; c++)
                {
                    x += colWidths[c];
                    // [CSS-GRID §7.2.3.1] Collapsed track gutters are also collapsed.
                    bool colCollapsed = collapsedCols != null && c < collapsedCols.Length && collapsedCols[c];
                    if (!colCollapsed)
                    {
                        x += effectiveColGap;
                    }
                }

                // Logical block-axis cumulative start of the cell within the container's
                // content box.
                float y = alignContentOffset;
                for (int r = 0; r < item.RowStart && r < finalRows; r++)
                {
                    y += rowHeights[r];
                    bool rowCollapsed = collapsedRows != null && r < collapsedRows.Length && collapsedRows[r];
                    if (!rowCollapsed)
                    {
                        y += effectiveRowGap;
                    }
                }

                // For spanning items, calculate the actual cell area
                float spanWidth = 0;
                for (int c = item.ColStart; c < item.ColStart + item.ColSpan && c < finalCols; c++)
                {
                    spanWidth += colWidths[c];
                    if (c > item.ColStart)
                    {
                        bool colColl = collapsedCols != null && c < collapsedCols.Length && collapsedCols[c];
                        bool prevColl = collapsedCols != null && (c - 1) < collapsedCols.Length && collapsedCols[c - 1];
                        if (!colColl && !prevColl)
                        {
                            spanWidth += effectiveColGap;
                        }
                    }
                }

                float spanHeight = 0;
                for (int r = item.RowStart; r < item.RowStart + item.RowSpan && r < finalRows; r++)
                {
                    spanHeight += rowHeights[r];
                    if (r > item.RowStart)
                    {
                        bool rowColl = collapsedRows != null && r < collapsedRows.Length && collapsedRows[r];
                        bool prevRowColl = collapsedRows != null && (r - 1) < collapsedRows.Length && collapsedRows[r - 1];
                        if (!rowColl && !prevRowColl)
                        {
                            spanHeight += effectiveRowGap;
                        }
                    }
                }

                float finalWidth = item.ContentWidth;
                float finalHeight = item.ContentHeight;

                // [CSS-WRITING-MODES-3 §6.2] Compute outer (margin-box) size on each logical
                // axis using the writing-mode-aware accessors. In horizontal-tb the inline
                // axis is physically X (left/right margin/border/padding), in vertical-lr/rl
                // it is physically Y (top/bottom margin/border/padding).
                float outerInlineExtra = LogicalPaddingInlineStart(item.Box, writingMode)
                    + LogicalPaddingInlineEnd(item.Box, writingMode)
                    + LogicalBorderInlineStart(item.Box, writingMode)
                    + LogicalBorderInlineEnd(item.Box, writingMode)
                    + LogicalMarginInlineStart(item.Box, writingMode)
                    + LogicalMarginInlineEnd(item.Box, writingMode);
                float outerBlockExtra = LogicalPaddingBlockStart(item.Box, writingMode)
                    + LogicalPaddingBlockEnd(item.Box, writingMode)
                    + LogicalBorderBlockStart(item.Box, writingMode)
                    + LogicalBorderBlockEnd(item.Box, writingMode)
                    + LogicalMarginBlockStart(item.Box, writingMode)
                    + LogicalMarginBlockEnd(item.Box, writingMode);
                float outerWidth = finalWidth + outerInlineExtra;
                float outerHeight = finalHeight + outerBlockExtra;

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
                // The auto check operates on the LOGICAL inline-start/end and block-start/end
                // sides so `margin-inline-start: auto` works regardless of writing mode.
                if (item.StyledElement != null)
                {
                    ResolveAutoMargins(item.Box, item.StyledElement.Style, writingMode,
                        spanWidth - outerWidth, spanHeight - outerHeight);
                    // Recompute outer extras: auto-margin resolution may have written values
                    // into previously-NaN sides, changing the inline/block outer sizes.
                    outerInlineExtra = LogicalPaddingInlineStart(item.Box, writingMode)
                        + LogicalPaddingInlineEnd(item.Box, writingMode)
                        + LogicalBorderInlineStart(item.Box, writingMode)
                        + LogicalBorderInlineEnd(item.Box, writingMode)
                        + LogicalMarginInlineStart(item.Box, writingMode)
                        + LogicalMarginInlineEnd(item.Box, writingMode);
                    outerBlockExtra = LogicalPaddingBlockStart(item.Box, writingMode)
                        + LogicalPaddingBlockEnd(item.Box, writingMode)
                        + LogicalBorderBlockStart(item.Box, writingMode)
                        + LogicalBorderBlockEnd(item.Box, writingMode)
                        + LogicalMarginBlockStart(item.Box, writingMode)
                        + LogicalMarginBlockEnd(item.Box, writingMode);
                    outerWidth = finalWidth + outerInlineExtra;
                    outerHeight = finalHeight + outerBlockExtra;
                }

                // [CSS-GRID §10.1] Apply inline-axis alignment offset (justify-self / justify-items).
                // Baseline-aligned items use the per-column baseline group offset computed
                // above; others fall back to the positional alignment via AlignOffset.
                // [CSS-ALIGN-3 §9.3] Replaced elements fall back to start alignment.
                float xOffset;
                bool useColBaselineAlignment = alignInline == CssAlignItems.Baseline
                    && colMaxBaselines != null
                    && item.ColStart >= 0 && item.ColStart < finalCols
                    && item.ColSpan == 1
                    && !HasSynthesizedBaselineOnly(item)
                    && !isVerticalWM;
                if (useColBaselineAlignment)
                {
                    float itemBaselineFromCell = ComputeItemBaselineFromMarginLeft(item, isVerticalWM);
                    xOffset = colMaxBaselines![item.ColStart] - itemBaselineFromCell;
                }
                else
                {
                    CssAlignItems effectiveInline = alignInline == CssAlignItems.Baseline
                        ? CssAlignItems.Start : alignInline;
                    xOffset = AlignOffset(effectiveInline, spanWidth, outerWidth);
                }

                // [CSS-GRID §10.1] Apply block-axis alignment offset (align-self / align-items).
                // Baseline-aligned items use per-row baseline group offset.
                // [CSS-ALIGN-3 §9.3] Replaced elements and orthogonal items in
                // vertical-WM grids fall back to start alignment instead of joining
                // the row sharing group (see ItemParticipatesInRowBaselineSharingGroup).
                // [CSS-ALIGN-3 §9.4.4] Items with block-flow opposite to the grid's
                // join the end-anchored group: their yOffset is measured back from
                // the row's block-end edge.
                float yOffset;
                bool useBaselineAlignment = alignBlock == CssAlignItems.Baseline
                    && rowMaxBaselineStart != null
                    && item.RowStart >= 0 && item.RowStart < finalRows
                    && item.RowSpan == 1
                    && ItemParticipatesInRowBaselineSharingGroup(item, writingMode);
                if (useBaselineAlignment)
                {
                    float itemBaselineFromCell = ComputeItemFirstBaselineInBlockAxis(item, writingMode);
                    if (IsItemOppositeBlockDirection(item, writingMode))
                    {
                        yOffset = spanHeight
                            - rowMaxBaselineEnd![item.RowStart]
                            - itemBaselineFromCell;
                    }
                    else
                    {
                        yOffset = rowMaxBaselineStart![item.RowStart] - itemBaselineFromCell;
                    }
                }
                else
                {
                    CssAlignItems effectiveBlock = alignBlock == CssAlignItems.Baseline
                        ? CssAlignItems.Start : alignBlock;
                    yOffset = AlignOffset(effectiveBlock, spanHeight, outerHeight);
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

                // Compute logical content-box origin: cell-start + alignment offset + the
                // start-side margin/border/padding on each axis. Stays in logical space until
                // the LogicalToPhysicalRect mapping below.
                float inlineContentStart = x + xOffset
                    + LogicalMarginInlineStart(item.Box, writingMode)
                    + LogicalBorderInlineStart(item.Box, writingMode)
                    + LogicalPaddingInlineStart(item.Box, writingMode);
                float blockContentStart = y + yOffset
                    + LogicalMarginBlockStart(item.Box, writingMode)
                    + LogicalBorderBlockStart(item.Box, writingMode)
                    + LogicalPaddingBlockStart(item.Box, writingMode);

                RectF physicalContentRect = LogicalToPhysicalRect(
                    inlineContentStart, blockContentStart, finalWidth, finalHeight,
                    writingMode,
                    containerPhysicalX, containerPhysicalY, containerPhysicalWidth);

                // Offset all descendants (children + line boxes) from first-pass (0,0)
                // to the actual grid cell physical position.
                float dx = physicalContentRect.X - item.Box.ContentRect.X;
                float dy = physicalContentRect.Y - item.Box.ContentRect.Y;
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

                item.Box.ContentRect = physicalContentRect;

                parent.AddChild(item.Box);
            }

            // [CSS-GRID §12.4] Set grid container auto block-size from row tracks so
            // BFC can use it (instead of CalculateAutoHeight which misses tracks).
            // In vertical writing mode the block axis is physically horizontal, so the
            // accumulated track total goes into physical width, not height.
            bool needsBlockExtent = isVerticalWM
                ? containerPhysicalWidth <= 0
                : parent.ContentRect.Height <= 0;
            if (needsBlockExtent)
            {
                float totalRowHeight = 0;
                for (int r = 0; r < finalRows; r++)
                {
                    totalRowHeight += rowHeights[r];
                    if (r < finalRows - 1)
                    {
                        totalRowHeight += effectiveRowGap;
                    }
                }
                if (isVerticalWM)
                {
                    parent.ContentRect = new RectF(containerPhysicalX, containerPhysicalY,
                        totalRowHeight, parent.ContentRect.Height);
                }
                else
                {
                    parent.ContentRect = new RectF(parent.ContentRect.X, parent.ContentRect.Y,
                        parent.ContentRect.Width, totalRowHeight);
                }
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
                context.FloatContext = new FloatContext(gridArea.X, areaWidth);

                // [CSS2 §10.3.7] Resolve width for abspos items, accounting for
                // left/right constraints against the grid area containing block.
                var itemStyle = item.StyledElement.Style;
                float posWidth;
                bool widthIsAuto = float.IsNaN(itemStyle.Width);
                if (widthIsAuto && !float.IsNaN(itemStyle.Left) && !float.IsNaN(itemStyle.Right))
                {
                    // Both left and right specified with auto width: width = area - left - right - box model
                    float resolvedLeft = DeferredPercent.IsEncoded(itemStyle.Left)
                        ? DeferredPercent.Resolve(itemStyle.Left, areaWidth) : itemStyle.Left;
                    float resolvedRight = DeferredPercent.IsEncoded(itemStyle.Right)
                        ? DeferredPercent.Resolve(itemStyle.Right, areaWidth) : itemStyle.Right;
                    posWidth = areaWidth - resolvedLeft - resolvedRight
                             - posBox.MarginLeft - posBox.MarginRight
                             - posBox.BorderLeftWidth - posBox.BorderRightWidth
                             - posBox.PaddingLeft - posBox.PaddingRight;
                    if (posWidth < 0)
                    {
                        posWidth = 0;
                    }
                }
                else if (widthIsAuto)
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
                        itemStyle, areaWidth, posBox);
                }

                // [CSS2 §10.6.4] Pre-resolve height from top/bottom constraints so
                // children can use the definite height during layout.
                float preHeight = 0;
                if (float.IsNaN(itemStyle.Height) && !float.IsNaN(itemStyle.Top) && !float.IsNaN(itemStyle.Bottom))
                {
                    float resolvedTop = DeferredPercent.IsEncoded(itemStyle.Top)
                        ? DeferredPercent.Resolve(itemStyle.Top, areaHeight) : itemStyle.Top;
                    float resolvedBottom = DeferredPercent.IsEncoded(itemStyle.Bottom)
                        ? DeferredPercent.Resolve(itemStyle.Bottom, areaHeight) : itemStyle.Bottom;
                    preHeight = areaHeight - resolvedTop - resolvedBottom
                              - posBox.MarginTop - posBox.MarginBottom
                              - posBox.BorderTopWidth - posBox.BorderBottomWidth
                              - posBox.PaddingTop - posBox.PaddingBottom;
                    if (preHeight < 0)
                    {
                        preHeight = 0;
                    }
                }

                // Layout children at the resolved static position and width.
                posBox.ContentRect = new RectF(gridArea.X, gridArea.Y, posWidth, preHeight);
                BlockFormattingContext.LayoutChildren(posBox, context);

                float posHeight = DimensionResolver.ResolveHeight(
                    itemStyle, areaHeight, posBox);
                if (float.IsNaN(posHeight))
                {
                    posHeight = preHeight > 0 ? preHeight : BlockFormattingContext.CalculateAutoHeight(posBox);
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
            // [CSS-GRID §9] Resolve negative start line (e.g., -1 = last explicit line).
            if (item.RawColStart != 0)
            {
                int resolved = ResolveNegativeLine(item.RawColStart, Math.Max(1, explicitCols));
                colStartLine = resolved >= 0 ? resolved : -1;
            }
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
            // [CSS-GRID §9] Resolve negative start line.
            if (item.RawRowStart != 0)
            {
                int resolved = ResolveNegativeLine(item.RawRowStart, Math.Max(1, explicitRows));
                rowStartLine = resolved >= 0 ? resolved : -1;
            }
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

        // Build the initial track size array, filling in explicit tracks then padding
        // any remaining implicit tracks using grid-auto-columns/grid-auto-rows (if set)
        // or the auto default (max-content sentinel with stretch-to-fill) otherwise.
        // The isImplicitAuto out parameter marks the tracks that came from the auto
        // default branch so the caller can stretch them to absorb free space after
        // the intrinsic sizing pass resolves their base sizes from item max-content.
        // [CSS-GRID-1 §7.2.3] https://drafts.csswg.org/css-grid-1/#auto-tracks
        // [CSS-GRID-1 §11.8] https://drafts.csswg.org/css-grid-1/#algo-stretch
        private static float[] BuildTrackSizes(float[]? explicitTracks, int count, float containerSize,
            float gap, object? autoTrackRaw, float defaultSize, out bool[] isImplicitAuto)
        {
            var sizes = new float[count];
            isImplicitAuto = new bool[count];

            if (explicitTracks != null)
            {
                for (int i = 0; i < Math.Min(explicitTracks.Length, count); i++)
                {
                    sizes[i] = explicitTracks[i];
                }
            }

            float autoSize = 0;
            if (autoTrackRaw != null)
            {
                var autoTracks = ResolveTrackList(autoTrackRaw, containerSize);
                if (autoTracks != null && autoTracks.Length > 0)
                {
                    autoSize = autoTracks[0];
                }
            }

            int explicitCount = explicitTracks?.Length ?? 0;
            if (explicitCount < count)
            {
                if (autoSize > 0)
                {
                    for (int i = explicitCount; i < count; i++)
                    {
                        sizes[i] = autoSize;
                    }
                }
                else
                {
                    // [CSS-GRID-1 §7.2.3] Default implicit track sizing = auto, which
                    // behaves as minmax(min-content, max-content). Emit the max-content
                    // sentinel (-2) so the item-measurement pass at BuildAllTracks time
                    // sizes each track to its contents. The post-intrinsic stretch pass
                    // then absorbs any remaining free space per §11.7.
                    for (int i = explicitCount; i < count; i++)
                    {
                        sizes[i] = -2;
                        isImplicitAuto[i] = true;
                    }
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
            // [CSS-GRID §9] Detect negative start line (e.g., -1) vs auto (both parse to -1).
            if (item.RowStart < 0 && IsNegativeLineNumber(rowStartRaw))
            {
                item.RawRowStart = item.RowStart;
            }

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

            var colStartRaw = style.GetRefValue(PropertyId.GridColumnStart);
            item.ColStart = ParseLineValue(colStartRaw, out int colSpan, colLineNames);
            item.ColSpan = colSpan;
            // [CSS-GRID §9] Detect negative start line vs auto.
            if (item.ColStart < 0 && IsNegativeLineNumber(colStartRaw))
            {
                item.RawColStart = item.ColStart;
            }

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
            if (raw is CssKeywordValue kw && (kw.Keyword == "none" || kw.Keyword == "subgrid"))
                return null;
            // [CSS-GRID §7.2] 'auto' as a standalone keyword = one auto-sized track.
            // Don't confuse with 'none' (no explicit tracks). ParseTrackValue handles
            // 'auto' as a track sizing function (≈ 1fr for now).
            // Falls through to normal track parsing below.

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
                    int tracksPerRepeat = 0;
                    for (int j = 1; j < fn.Arguments.Count; j++)
                    {
                        var arg = fn.Arguments[j];
                        // [CSS-GRID §7.2.3.1] Repeat arguments may be a space-separated
                        // list of multiple track values (e.g., repeat(auto-fill, 50px 50px)).
                        if (arg is CssListValue argList && argList.Separator == ' ')
                        {
                            for (int k = 0; k < argList.Values.Count; k++)
                            {
                                var innerArg = argList.Values[k];
                                // Skip line name brackets (all-keyword lists)
                                if (innerArg is CssListValue bracket)
                                {
                                    bool allKw = true;
                                    for (int bi = 0; bi < bracket.Values.Count; bi++)
                                    {
                                        if (!(bracket.Values[bi] is CssKeywordValue))
                                        {
                                            allKw = false;
                                            break;
                                        }
                                    }
                                    if (allKw) { continue; }
                                }
                                var innerParsed = ParseTrackValue(innerArg, containerSize);
                                trackMinSize += innerParsed.isFr ? 0 : Math.Max(0, innerParsed.value);
                                tracksPerRepeat++;
                            }
                        }
                        else if (arg is CssFunctionValue minmaxFn && minmaxFn.Name == "minmax" && minmaxFn.Arguments.Count >= 2)
                        {
                            var minParsed = ParseTrackValue(minmaxFn.Arguments[0], containerSize);
                            trackMinSize += minParsed.isFr ? 0 : minParsed.value;
                            tracksPerRepeat++;
                        }
                        else
                        {
                            var parsed = ParseTrackValue(arg, containerSize);
                            trackMinSize += parsed.isFr ? 0 : Math.Max(0, parsed.value);
                            tracksPerRepeat++;
                        }
                    }
                    if (tracksPerRepeat < 1) { tracksPerRepeat = 1; }
                    if (trackMinSize > 0)
                    {
                        // [CSS-GRID §7.2.3.1] With K tracks per repetition, each
                        // repetition adds K-1 inter-track gaps plus 1 inter-group gap.
                        // N * trackMinSize + (N*K - 1) * gap <= containerSize
                        // → N <= (containerSize + gap) / (trackMinSize + K * gap)
                        float denominator = trackMinSize + tracksPerRepeat * gap;
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
                    // [CSS-GRID §7.2.4] minmax(auto, fixed): auto min = min-content,
                    // clamped by the max. Use the max value as track size.
                    // Full iterative min-size-auto resolution not yet implemented.
                    if (minVal.isFr)
                    {
                        return (maxVal.value, false);
                    }
                    // Both fixed: use max as the track size,
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

        /// <summary>
        /// [CSS-GRID-2 §8] Pre-measures a row-subgrid item's children to contribute
        /// per-row heights to the parent grid's row sizing. This runs BEFORE the main
        /// layout pass so parent auto rows get sized from subgrid content.
        /// </summary>
        private static void PreMeasureSubgridRowContributions(
            GridItem subgridItem, float[] parentRowHeights, int parentRows,
            float[] parentColWidths, int parentCols,
            float colGap, float rowGap,
            Dictionary<string, List<int>> colLineNames,
            Dictionary<string, List<int>> rowLineNames,
            LayoutContext context)
        {
            var element = subgridItem.StyledElement!;
            var children = BlockFormattingContext.FlattenContents(element);
            if (children.Count == 0)
            {
                return;
            }

            int subRows = subgridItem.RowSpan;

            // Build subgrid column widths from parent's columns
            float subgridWidth = 0;
            int subCols = subgridItem.ColSpan;
            for (int c = subgridItem.ColStart; c < subgridItem.ColStart + subCols && c < parentCols; c++)
            {
                subgridWidth += parentColWidths[c];
            }
            if (subCols > 1)
            {
                subgridWidth += (subCols - 1) * colGap;
            }

            // Collect and place subgrid children in their rows
            int autoRow = 0;
            for (int ci = 0; ci < children.Count; ci++)
            {
                var child = children[ci];
                if (child.IsText)
                {
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

                // Determine which subgrid row this child lands in
                int childRow = -1;
                int childRowSpan = 1;
                var childRowStartRaw = childEl.Style.GetRefValue(PropertyId.GridRowStart);
                if (childRowStartRaw is CssNumberValue rowNum && rowNum.Value > 0)
                {
                    childRow = (int)rowNum.Value - 1; // 1-based to 0-based
                }
                else if (childRowStartRaw is CssDimensionValue rowDim && rowDim.Value > 0)
                {
                    childRow = (int)rowDim.Value - 1;
                }

                if (childRow < 0)
                {
                    // Auto-placement: use next available row
                    childRow = autoRow;
                }
                if (childRow >= subRows)
                {
                    continue; // outside subgrid span
                }
                autoRow = childRow + childRowSpan;

                // Measure this child's height contribution
                var tempBox = new LayoutBox(childEl, BoxType.Block);
                BoxModelCalculator.ApplyBoxModel(tempBox, childEl.Style, subgridWidth);

                float contentHeight;
                float explicitH = DimensionResolver.ResolveHeight(childEl.Style, float.NaN, tempBox);
                if (!float.IsNaN(explicitH) && explicitH >= 0)
                {
                    contentHeight = explicitH;
                }
                else
                {
                    // Measure content: lay out at available width to get auto height
                    float availW = subgridWidth - tempBox.PaddingLeft - tempBox.PaddingRight
                                 - tempBox.BorderLeftWidth - tempBox.BorderRightWidth;
                    if (availW < 0) { availW = 0; }
                    tempBox.ContentRect = new RectF(0, 0, availW, 0);
                    var savedFloat = context.FloatContext;
                    var savedParent = context.ParentGridContext;
                    context.FloatContext = new FloatContext(0, availW);
                    context.ParentGridContext = null;
                    BlockFormattingContext.LayoutChildren(tempBox, context);
                    context.ParentGridContext = savedParent;
                    context.FloatContext = savedFloat;
                    contentHeight = CalculateAutoHeight(tempBox);
                }

                float totalHeight = contentHeight
                    + tempBox.PaddingTop + tempBox.PaddingBottom
                    + tempBox.BorderTopWidth + tempBox.BorderBottomWidth
                    + tempBox.MarginTop + tempBox.MarginBottom;

                int parentRow = subgridItem.RowStart + childRow;
                if (parentRow >= 0 && parentRow < parentRows
                    && totalHeight > parentRowHeights[parentRow])
                {
                    parentRowHeights[parentRow] = totalHeight;
                }
            }
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
        /// [CSS-ALIGN-3 §9.1] Get the first baseline of a grid item from its first
        /// line box. Falls back to the bottom edge of the item's content area if no
        /// line boxes are found (synthesized baseline).
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
        /// [CSS-ALIGN-3 §9.3] Returns true if this grid item has only a synthesized
        /// baseline and should fall back to start alignment instead of joining the
        /// baseline-sharing group. Replaced elements (canvas, img, video, form
        /// controls, etc.) never synthesize a usable baseline — Chrome treats them
        /// as "has no baseline" and falls them back to start alignment.
        /// </summary>
        private static bool HasSynthesizedBaselineOnly(GridItem item)
        {
            if (item.StyledElement == null)
            {
                return false;
            }
            return ReplacedElementLayout.IsReplaced(item.StyledElement);
        }

        /// <summary>
        /// [CSS-WRITING-MODES-3 §7.1] Returns true when the grid item's writing
        /// mode is orthogonal to the grid container's writing mode. An orthogonal
        /// item has no natural first baseline along the sharing-group axis, so
        /// per [CSS-ALIGN-3 §9.1] its alignment baseline is synthesized from the
        /// end edge of its border box on the group's alignment axis.
        /// </summary>
        private static bool IsItemOrthogonalToGrid(GridItem item, bool gridIsVerticalWritingMode)
        {
            if (item.StyledElement == null)
            {
                return false;
            }
            bool itemIsVerticalWritingMode = BlockFormattingContext.IsVerticalWritingMode(
                item.StyledElement.Style);
            return itemIsVerticalWritingMode != gridIsVerticalWritingMode;
        }

        /// <summary>
        /// [CSS-ALIGN-3 §9.3] Decides whether a grid item participates in the
        /// row-like Baseline Sharing Group — i.e. whether its first baseline in
        /// the grid's block axis should be collected and used to shift the item.
        ///
        /// Cases:
        /// - Replaced elements (canvas, img, form controls): never participate;
        ///   they fall back to start alignment instead of being pulled to the
        ///   bottom margin edge of the sharing group.
        /// - Parallel items (same writing-mode horizontality as the grid): always
        ///   participate via their real text baseline or their box-model-start
        ///   synthesis.
        /// - Orthogonal items in a horizontal-tb grid: participate via border-end
        ///   synthesis, matching Chrome's `inline-block + margin-bottom:0`
        ///   emulation used by the horiz-002/003 WPT reference files.
        /// - Orthogonal items in a vertical-lr/rl grid: do NOT participate. The
        ///   vertical-lr/rl-003 WPT reference files emulate the expected
        ///   rendering with `float: left` rather than `inline-block`, which is
        ///   out-of-flow and pins items at the grid's block-start edge. Pulling
        ///   them into the sharing group here would drag smaller items toward
        ///   the block-end edge of the row, which is what the pre-fix Scope 3
        ///   code did and what produced the lr-003 regression.
        /// </summary>
        private static bool ItemParticipatesInRowBaselineSharingGroup(
            GridItem item, CssWritingMode writingMode)
        {
            if (HasSynthesizedBaselineOnly(item))
            {
                return false;
            }
            bool gridIsVerticalWritingMode = writingMode == CssWritingMode.VerticalLr
                || writingMode == CssWritingMode.VerticalRl;
            if (gridIsVerticalWritingMode
                && IsItemOrthogonalToGrid(item, gridIsVerticalWritingMode))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// [CSS-ALIGN-3 §9.4.4] Returns true when a grid item's block-flow
        /// direction is the exact reverse of the grid container's along the
        /// same physical axis. Only vertical-lr ↔ vertical-rl pairs qualify —
        /// horizontal-tb never has an opposing counterpart because it is the
        /// only horizontal block-flow mode supported.
        ///
        /// Opposing-direction items form a second ("last") baseline sharing
        /// group anchored at the row's block-end edge, separate from the
        /// start-anchored group used for same-direction items.
        /// </summary>
        private static bool IsItemOppositeBlockDirection(GridItem item, CssWritingMode gridWm)
        {
            if (item.StyledElement == null)
            {
                return false;
            }
            CssWritingMode itemWm = item.StyledElement.Style.WritingMode;
            if (gridWm == CssWritingMode.VerticalLr && itemWm == CssWritingMode.VerticalRl)
            {
                return true;
            }
            if (gridWm == CssWritingMode.VerticalRl && itemWm == CssWritingMode.VerticalLr)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// [CSS-WRITING-MODES-3 §6.2] Compute a grid item's outer (margin-box)
        /// extent along the grid container's block axis.
        ///
        /// GridItem stores its sizes in LOGICAL terms: ContentWidth is the
        /// logical inline-axis extent and ContentHeight is the logical
        /// block-axis extent, regardless of the grid's writing mode. The
        /// grid's first-pass layout places the logical-block track value in
        /// ContentHeight for both horizontal-tb and vertical-lr/rl grids,
        /// so the block-axis outer size always uses ContentHeight plus the
        /// block-side margin/border/padding via the logical accessors.
        /// </summary>
        private static float ComputeItemOuterBlockSize(GridItem item, CssWritingMode writingMode)
        {
            return item.ContentHeight
                 + LogicalPaddingBlockStart(item.Box, writingMode)
                 + LogicalPaddingBlockEnd(item.Box, writingMode)
                 + LogicalBorderBlockStart(item.Box, writingMode)
                 + LogicalBorderBlockEnd(item.Box, writingMode)
                 + LogicalMarginBlockStart(item.Box, writingMode)
                 + LogicalMarginBlockEnd(item.Box, writingMode);
        }

        /// <summary>
        /// [CSS-ALIGN-3 §9.1] Compute a grid item's first-baseline offset measured
        /// from the block-start edge of its margin box, for participation in a
        /// row-like (block-axis) baseline sharing group.
        ///
        /// Four shapes are handled:
        /// - Parallel item in a horizontal-tb grid: returns the item's inner text
        ///   baseline via GetItemBaseline, offset by MarginTop to move from the
        ///   item's border box into its margin box.
        /// - Parallel item in a vertical-lr/rl grid: [CSS-WRITING-MODES-3 §5.1.1]
        ///   text-orientation:mixed rotates Latin glyphs 90° CW, placing their
        ///   alphabetic baseline at `descent` distance from the content block-start
        ///   edge (not `ascent`). Rend's IFC falls through to horizontal layout,
        ///   so the first horizontal line box's `Height - Baseline` (descent)
        ///   gives the correct block-axis baseline offset for the rotated glyph.
        /// - Orthogonal item in either grid writing mode: has no natural first
        ///   baseline along the sharing-group axis. Per §9.1 it is synthesized
        ///   from the block-end edge of the border box, matching Chrome's
        ///   `inline-block + margin-bottom:0` emulation used by the WPT reference
        ///   files (grid-self-baseline-horiz-002-ref.html etc.).
        /// </summary>
        private static float ComputeItemFirstBaselineInBlockAxis(GridItem item, CssWritingMode writingMode)
        {
            bool gridIsVerticalWritingMode = writingMode == CssWritingMode.VerticalLr
                || writingMode == CssWritingMode.VerticalRl;

            if (IsItemOrthogonalToGrid(item, gridIsVerticalWritingMode))
            {
                return LogicalMarginBlockStart(item.Box, writingMode)
                     + LogicalBorderBlockStart(item.Box, writingMode)
                     + LogicalPaddingBlockStart(item.Box, writingMode)
                     + item.ContentHeight
                     + LogicalPaddingBlockEnd(item.Box, writingMode)
                     + LogicalBorderBlockEnd(item.Box, writingMode);
            }

            if (!gridIsVerticalWritingMode)
            {
                return GetItemBaseline(item.Box) + item.Box.MarginTop;
            }

            float blockStartToContent = LogicalMarginBlockStart(item.Box, writingMode)
                + LogicalBorderBlockStart(item.Box, writingMode)
                + LogicalPaddingBlockStart(item.Box, writingMode);
            var firstLine = FindFirstHorizontalLineBox(item.Box);
            if (firstLine != null && firstLine.Height > 0)
            {
                float rotatedBaselineDistance = firstLine.Height - firstLine.Baseline;
                if (rotatedBaselineDistance < 0)
                {
                    rotatedBaselineDistance = 0;
                }
                return blockStartToContent + rotatedBaselineDistance;
            }
            return blockStartToContent
                 + item.ContentHeight
                 + LogicalPaddingBlockEnd(item.Box, writingMode)
                 + LogicalBorderBlockEnd(item.Box, writingMode);
        }

        /// <summary>
        /// [CSS-ALIGN-3 §9.1] Compute a grid item's first-baseline offset measured
        /// from the inline-start edge (left in horizontal-tb) of its margin box,
        /// for participation in a column-like baseline sharing group.
        /// Parallel items (same writing mode as grid) have no natural baseline
        /// along the column axis; per §9.1 their alignment-baseline is synthesized
        /// from the inline-start edge of the margin box (returns 0).
        /// Orthogonal items (vertical-WM in a horizontal-tb grid) would normally
        /// use the first-line ascent projected into the parent coordinate space.
        /// Because Rend's inline formatting context currently falls through to
        /// horizontal layout for vertical-WM containers (see
        /// InlineFormattingContext.Layout), the first line of an orthogonal item
        /// is horizontal, not vertical — so there is no real line-box X coordinate
        /// to read. The synthetic baseline X for a sideways-rotated glyph is the
        /// descent of the horizontal first line (= line.Height - line.Baseline),
        /// which equals `lineWidth - ascent` in the conceptual rotated layout.
        /// </summary>
        private static float ComputeItemBaselineFromMarginLeft(GridItem item, bool gridIsVerticalWritingMode)
        {
            if (gridIsVerticalWritingMode)
            {
                return 0;
            }
            var itemBox = item.Box;
            if (!IsItemOrthogonalToGrid(item, gridIsVerticalWritingMode))
            {
                return 0;
            }
            var firstLine = FindFirstHorizontalLineBox(itemBox);
            if (firstLine != null && firstLine.Height > 0)
            {
                float descent = firstLine.Height - firstLine.Baseline;
                if (descent < 0)
                {
                    descent = 0;
                }
                return itemBox.MarginLeft + itemBox.BorderLeftWidth + itemBox.PaddingLeft + descent;
            }
            return itemBox.MarginLeft + itemBox.BorderLeftWidth + itemBox.PaddingLeft;
        }

        /// <summary>
        /// Recursively walk a LayoutBox subtree and return the first horizontal
        /// line box found. Used by ComputeItemBaselineFromMarginLeft to source
        /// metrics for the synthesized column-axis baseline of an orthogonal item.
        /// </summary>
        private static LineBox? FindFirstHorizontalLineBox(LayoutBox box)
        {
            if (box.LineBoxes != null)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    if (!box.LineBoxes[i].IsVertical)
                    {
                        return box.LineBoxes[i];
                    }
                }
            }
            for (int i = 0; i < box.Children.Count; i++)
            {
                var fromChild = FindFirstHorizontalLineBox(box.Children[i]);
                if (fromChild != null)
                {
                    return fromChild;
                }
            }
            return null;
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

        /// <summary>
        /// [CSS-GRID §10.1] Resolve the effective inline-axis alignment for a grid
        /// item, considering justify-self override vs container justify-items default.
        /// </summary>
        private static CssAlignItems ResolveItemInlineAlignment(GridItem item, CssAlignItems containerDefault)
        {
            if (item.StyledElement == null)
            {
                return containerDefault;
            }
            var selfJustify = item.StyledElement.Style.JustifySelf;
            if (selfJustify != CssAlignItems.Normal && (int)selfJustify <= (int)CssAlignItems.Normal)
            {
                return selfJustify;
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

        // [CSS-WRITING-MODES-3 §6.2] Logical box-model side accessors. Grid arithmetic is
        // expressed in logical (inline / block) coordinates so it can stay agnostic to the
        // container's writing mode. Each of these maps a logical side onto the physical
        // margin/border/padding side that holds its value in the current writing mode.
        // horizontal-tb: inline-start = left,  block-start = top
        // vertical-lr:   inline-start = top,   block-start = left
        // vertical-rl:   inline-start = top,   block-start = right (block axis grows left)

        private static float LogicalMarginInlineStart(LayoutBox box, CssWritingMode wm)
        {
            return wm == CssWritingMode.HorizontalTb ? box.MarginLeft : box.MarginTop;
        }

        private static float LogicalMarginInlineEnd(LayoutBox box, CssWritingMode wm)
        {
            return wm == CssWritingMode.HorizontalTb ? box.MarginRight : box.MarginBottom;
        }

        private static float LogicalMarginBlockStart(LayoutBox box, CssWritingMode wm)
        {
            if (wm == CssWritingMode.HorizontalTb) { return box.MarginTop; }
            if (wm == CssWritingMode.VerticalLr) { return box.MarginLeft; }
            return box.MarginRight;
        }

        private static float LogicalMarginBlockEnd(LayoutBox box, CssWritingMode wm)
        {
            if (wm == CssWritingMode.HorizontalTb) { return box.MarginBottom; }
            if (wm == CssWritingMode.VerticalLr) { return box.MarginRight; }
            return box.MarginLeft;
        }

        private static float LogicalBorderInlineStart(LayoutBox box, CssWritingMode wm)
        {
            return wm == CssWritingMode.HorizontalTb ? box.BorderLeftWidth : box.BorderTopWidth;
        }

        private static float LogicalBorderInlineEnd(LayoutBox box, CssWritingMode wm)
        {
            return wm == CssWritingMode.HorizontalTb ? box.BorderRightWidth : box.BorderBottomWidth;
        }

        private static float LogicalBorderBlockStart(LayoutBox box, CssWritingMode wm)
        {
            if (wm == CssWritingMode.HorizontalTb) { return box.BorderTopWidth; }
            if (wm == CssWritingMode.VerticalLr) { return box.BorderLeftWidth; }
            return box.BorderRightWidth;
        }

        private static float LogicalBorderBlockEnd(LayoutBox box, CssWritingMode wm)
        {
            if (wm == CssWritingMode.HorizontalTb) { return box.BorderBottomWidth; }
            if (wm == CssWritingMode.VerticalLr) { return box.BorderRightWidth; }
            return box.BorderLeftWidth;
        }

        private static float LogicalPaddingInlineStart(LayoutBox box, CssWritingMode wm)
        {
            return wm == CssWritingMode.HorizontalTb ? box.PaddingLeft : box.PaddingTop;
        }

        private static float LogicalPaddingInlineEnd(LayoutBox box, CssWritingMode wm)
        {
            return wm == CssWritingMode.HorizontalTb ? box.PaddingRight : box.PaddingBottom;
        }

        private static float LogicalPaddingBlockStart(LayoutBox box, CssWritingMode wm)
        {
            if (wm == CssWritingMode.HorizontalTb) { return box.PaddingTop; }
            if (wm == CssWritingMode.VerticalLr) { return box.PaddingLeft; }
            return box.PaddingRight;
        }

        private static float LogicalPaddingBlockEnd(LayoutBox box, CssWritingMode wm)
        {
            if (wm == CssWritingMode.HorizontalTb) { return box.PaddingBottom; }
            if (wm == CssWritingMode.VerticalLr) { return box.PaddingRight; }
            return box.PaddingLeft;
        }

        // [CSS-GRID §10.3] Resolve auto margins on a grid item. Auto margins on a logical
        // axis absorb any free space along that axis, overriding alignment. The check uses
        // the LOGICAL inline-start/end and block-start/end so `margin-inline-start: auto`
        // works regardless of the container's writing mode.
        private static void ResolveAutoMargins(LayoutBox box, ComputedStyle itemStyle,
            CssWritingMode wm, float freeInline, float freeBlock)
        {
            bool autoInlineStart = wm == CssWritingMode.HorizontalTb
                ? float.IsNaN(itemStyle.MarginLeft) : float.IsNaN(itemStyle.MarginTop);
            bool autoInlineEnd = wm == CssWritingMode.HorizontalTb
                ? float.IsNaN(itemStyle.MarginRight) : float.IsNaN(itemStyle.MarginBottom);
            if ((autoInlineStart || autoInlineEnd) && freeInline > 0)
            {
                if (autoInlineStart && autoInlineEnd)
                {
                    SetLogicalMarginInlineStart(box, wm, freeInline / 2f);
                    SetLogicalMarginInlineEnd(box, wm, freeInline / 2f);
                }
                else if (autoInlineStart)
                {
                    SetLogicalMarginInlineStart(box, wm, freeInline);
                }
                else
                {
                    SetLogicalMarginInlineEnd(box, wm, freeInline);
                }
            }

            bool autoBlockStart;
            bool autoBlockEnd;
            if (wm == CssWritingMode.HorizontalTb)
            {
                autoBlockStart = float.IsNaN(itemStyle.MarginTop);
                autoBlockEnd = float.IsNaN(itemStyle.MarginBottom);
            }
            else if (wm == CssWritingMode.VerticalLr)
            {
                autoBlockStart = float.IsNaN(itemStyle.MarginLeft);
                autoBlockEnd = float.IsNaN(itemStyle.MarginRight);
            }
            else
            {
                autoBlockStart = float.IsNaN(itemStyle.MarginRight);
                autoBlockEnd = float.IsNaN(itemStyle.MarginLeft);
            }
            if ((autoBlockStart || autoBlockEnd) && freeBlock > 0)
            {
                if (autoBlockStart && autoBlockEnd)
                {
                    SetLogicalMarginBlockStart(box, wm, freeBlock / 2f);
                    SetLogicalMarginBlockEnd(box, wm, freeBlock / 2f);
                }
                else if (autoBlockStart)
                {
                    SetLogicalMarginBlockStart(box, wm, freeBlock);
                }
                else
                {
                    SetLogicalMarginBlockEnd(box, wm, freeBlock);
                }
            }
        }

        private static void SetLogicalMarginInlineStart(LayoutBox box, CssWritingMode wm, float value)
        {
            if (wm == CssWritingMode.HorizontalTb) { box.MarginLeft = value; }
            else { box.MarginTop = value; }
        }

        private static void SetLogicalMarginInlineEnd(LayoutBox box, CssWritingMode wm, float value)
        {
            if (wm == CssWritingMode.HorizontalTb) { box.MarginRight = value; }
            else { box.MarginBottom = value; }
        }

        private static void SetLogicalMarginBlockStart(LayoutBox box, CssWritingMode wm, float value)
        {
            if (wm == CssWritingMode.HorizontalTb) { box.MarginTop = value; return; }
            if (wm == CssWritingMode.VerticalLr) { box.MarginLeft = value; return; }
            box.MarginRight = value;
        }

        private static void SetLogicalMarginBlockEnd(LayoutBox box, CssWritingMode wm, float value)
        {
            if (wm == CssWritingMode.HorizontalTb) { box.MarginBottom = value; return; }
            if (wm == CssWritingMode.VerticalLr) { box.MarginRight = value; return; }
            box.MarginLeft = value;
        }

        // [CSS-WRITING-MODES-3 §7.1] Maps a logical rect (inline-start, block-start,
        // inline-size, block-size) within a container's content box origin into the
        // physical RectF that drawing/hit-testing actually use. The container's physical
        // origin and physical width are needed because vertical-rl flips the block axis
        // along physical X.
        private static RectF LogicalToPhysicalRect(
            float inlineStart, float blockStart, float inlineSize, float blockSize,
            CssWritingMode wm,
            float containerPhysicalX, float containerPhysicalY, float containerPhysicalWidth)
        {
            if (wm == CssWritingMode.HorizontalTb)
            {
                return new RectF(
                    containerPhysicalX + inlineStart,
                    containerPhysicalY + blockStart,
                    inlineSize, blockSize);
            }
            if (wm == CssWritingMode.VerticalLr)
            {
                return new RectF(
                    containerPhysicalX + blockStart,
                    containerPhysicalY + inlineStart,
                    blockSize, inlineSize);
            }
            // VerticalRl: block axis grows from physical right edge toward left.
            return new RectF(
                containerPhysicalX + containerPhysicalWidth - blockStart - blockSize,
                containerPhysicalY + inlineStart,
                blockSize, inlineSize);
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
        /// For horizontal-tb grids this sums column (inline axis) tracks; for
        /// vertical-lr/vertical-rl grids it sums row (block axis) tracks, because
        /// the physical width of a vertical-WM grid is its block size, not its
        /// inline size.
        ///
        /// Per-track sizing rules:
        ///   - Explicit pixel/percent tracks: declared size
        ///   - Auto tracks: max-content contribution of items in that track
        ///   - fr tracks: 0 contribution (flexible, not intrinsic)
        ///   - auto-fill/auto-fit: 1 repetition for intrinsic sizing
        /// </summary>
        internal static float ComputeIntrinsicWidth(
            StyledElement element, float keyword, float containingWidth, LayoutContext context,
            bool forceColumnAxis = false)
        {
            var style = element.Style;
            // [CSS-WRITING-MODES-3 §6.2] For vertical writing modes the grid's
            // physical width is the block axis size (sum of row tracks), not the
            // inline axis size (sum of column tracks). Placement still uses
            // logical column/row semantics from the CSS properties; only the
            // final per-track sum switches axis.
            //
            // <paramref name="forceColumnAxis"/> bypasses this writing-mode fork
            // and always walks the column-axis (inline) path. Used by
            // FloatLayout to pre-compute the inline size (physical Height) of a
            // vertical-WM floated grid so Layout receives a definite inline
            // size on entry instead of the default zero.
            bool sizeBlockAxis = !forceColumnAxis && BlockFormattingContext.IsVerticalWritingMode(style);

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
                    // [CSS-GRID §6] Text directly inside a grid container is wrapped in
                    // an anonymous grid item. For intrinsic sizing we wrap it in a
                    // synthesized StyledElement so its text contribution is measured.
                    var textNode = (StyledText)child;
                    if (string.IsNullOrWhiteSpace(textNode.Text))
                    {
                        continue;
                    }
                    var anonTextStyle = CloneStyleAsBlock(element.Style);
                    var anonTextDoc = element.Element.OwnerDocument;
                    var anonTextElement = anonTextDoc!.CreateElement("div");
                    var anonTextChildren = new List<StyledNode> { new StyledText(textNode.Text, anonTextStyle) };
                    var anonTextStyled = new StyledElement(anonTextElement, anonTextStyle, anonTextChildren);
                    items.Add(new GridItem
                    {
                        StyledElement = anonTextStyled,
                        Box = new LayoutBox(anonTextStyled, BoxType.Block),
                        OriginalIndex = items.Count
                    });
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

            if (sizeBlockAxis)
            {
                // [CSS-WRITING-MODES-3 §6.2] For vertical writing modes, the
                // grid's physical width equals its block size — the sum of row
                // tracks, not column tracks. Placement above used CSS-logical
                // col/row semantics unchanged; here we compute the per-row
                // primary-axis sum using the same sizing algorithm but walking
                // the row axis instead of the column axis.
                return ComputeRowAxisPrimarySum(
                    style, items, maxRow, keyword, containingWidth, context);
            }

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
            var minContentWidths = new float[finalCols];

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

                // [CSS-GRID §7.2.4.1] fit-content needs min-content as auto minimum floor
                bool isFitContent = columnWidths[item.ColStart] <= -2.5f
                                 && columnWidths[item.ColStart] > -3.5f;
                if (isFitContent && !isMinContent)
                {
                    float minWidth = MeasureGridItemOuterWidth(item, true, containingWidth, context);
                    if (minWidth > minContentWidths[item.ColStart])
                    {
                        minContentWidths[item.ColStart] = minWidth;
                    }
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
                // [CSS-GRID §7.2.4.1] fit-content: max(auto_min, min(max_content, limit))
                if (columnWidths[c] <= -2.5f && columnWidths[c] > -3.5f
                    && fitContentLimits != null && c < fitContentLimits.Length
                    && fitContentLimits[c] >= 0)
                {
                    measured = Math.Max(minContentWidths[c],
                        Math.Min(measured, fitContentLimits[c]));
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
        /// [CSS-WRITING-MODES-3 §6.2] Computes the row-axis primary-track sum for
        /// a vertical writing-mode grid container. This is the grid's physical
        /// width (= block size) for vertical-lr / vertical-rl. Mirrors the
        /// column-axis path in <see cref="ComputeIntrinsicWidth"/> but walks
        /// rows, reads <see cref="PropertyId.GridTemplateRows"/> /
        /// <see cref="PropertyId.GridAutoRows"/>, and uses
        /// <see cref="ComputedStyle.RowGap"/>.
        ///
        /// Each row's block size is the max physical-X extent of its items —
        /// <see cref="MeasureGridItemOuterWidth"/> returns the physical X
        /// dimension, which for a vertical-WM grid item is its block size.
        /// </summary>
        private static float ComputeRowAxisPrimarySum(
            ComputedStyle style,
            List<GridItem> items,
            int maxRow,
            float keyword,
            float containingWidth,
            LayoutContext context)
        {
            float rowGap = style.RowGap;
            if (DeferredPercent.IsEncoded(rowGap))
            {
                rowGap = 0;
            }
            if (float.IsNaN(rowGap) || rowGap < 0)
            {
                rowGap = 0;
            }

            var rowRaw = style.GetRefValue(PropertyId.GridTemplateRows);
            var flatRowValues = new List<object>();
            if (rowRaw != null && !IsSubgrid(rowRaw))
            {
                if (rowRaw is CssKeywordValue rowKw
                    && (rowKw.Keyword == "none" || rowKw.Keyword == "auto"))
                {
                    // No explicit rows
                }
                else if (rowRaw is CssListValue rowList)
                {
                    for (int i = 0; i < rowList.Values.Count; i++)
                    {
                        FlattenTrackValueForIntrinsic(rowList.Values[i], flatRowValues, containingWidth);
                    }
                }
                else
                {
                    FlattenTrackValueForIntrinsic(rowRaw, flatRowValues, containingWidth);
                }
            }

            int explicitRows = flatRowValues.Count;
            var rowTrackSizes = new float[explicitRows];
            for (int i = 0; i < explicitRows; i++)
            {
                var trackVal = flatRowValues[i];
                if (trackVal is CssKeywordValue autoKw && autoKw.Keyword == "auto")
                {
                    rowTrackSizes[i] = -2;
                    continue;
                }

                var parsed = ParseTrackValue(trackVal, containingWidth);
                if (parsed.isFr)
                {
                    float minFloor = GetMinmaxFloorForIntrinsic(trackVal, containingWidth);
                    if (minFloor > 0)
                    {
                        rowTrackSizes[i] = minFloor;
                    }
                    else
                    {
                        rowTrackSizes[i] = -2;
                    }
                }
                else if (parsed.value < 0)
                {
                    rowTrackSizes[i] = parsed.value;
                }
                else
                {
                    rowTrackSizes[i] = parsed.value;
                }
            }

            float autoRowSize = 0;
            object? autoRowRaw = style.GetRefValue(PropertyId.GridAutoRows);
            if (autoRowRaw != null)
            {
                var autoRowTracks = ResolveTrackList(autoRowRaw, containingWidth);
                if (autoRowTracks != null && autoRowTracks.Length > 0 && autoRowTracks[0] > 0)
                {
                    autoRowSize = autoRowTracks[0];
                }
            }

            var rowWidths = new float[maxRow];
            for (int r = 0; r < maxRow; r++)
            {
                if (r < rowTrackSizes.Length)
                {
                    rowWidths[r] = rowTrackSizes[r];
                }
                else if (autoRowSize > 0)
                {
                    rowWidths[r] = autoRowSize;
                }
                else
                {
                    rowWidths[r] = -2;
                }
            }

            float[]? fitContentLimits = ExtractFitContentLimits(rowRaw, maxRow, containingWidth, rowGap);

            bool isMinContent = keyword == SizingKeyword.MinContent;
            var measuredWidths = new float[maxRow];
            var minContentWidths = new float[maxRow];

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.RowStart < 0 || item.RowStart >= maxRow)
                {
                    continue;
                }
                if (item.RowSpan != 1)
                {
                    continue;
                }
                if (rowWidths[item.RowStart] >= 0)
                {
                    continue;
                }

                float itemOuterWidth = MeasureGridItemOuterWidth(item, isMinContent, containingWidth, context);
                if (itemOuterWidth > measuredWidths[item.RowStart])
                {
                    measuredWidths[item.RowStart] = itemOuterWidth;
                }

                bool isFitContent = rowWidths[item.RowStart] <= -2.5f
                                 && rowWidths[item.RowStart] > -3.5f;
                if (isFitContent && !isMinContent)
                {
                    float minWidth = MeasureGridItemOuterWidth(item, true, containingWidth, context);
                    if (minWidth > minContentWidths[item.RowStart])
                    {
                        minContentWidths[item.RowStart] = minWidth;
                    }
                }
            }

            for (int r = 0; r < maxRow; r++)
            {
                if (rowWidths[r] >= 0)
                {
                    continue;
                }
                if (rowWidths[r] <= -999f)
                {
                    rowWidths[r] = 0;
                    continue;
                }
                float measured = measuredWidths[r];
                if (rowWidths[r] <= -2.5f && rowWidths[r] > -3.5f
                    && fitContentLimits != null && r < fitContentLimits.Length
                    && fitContentLimits[r] >= 0)
                {
                    measured = Math.Max(minContentWidths[r],
                        Math.Min(measured, fitContentLimits[r]));
                }
                rowWidths[r] = measured;
            }

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.RowSpan <= 1 || item.RowStart < 0)
                {
                    continue;
                }

                float itemOuterWidth = MeasureGridItemOuterWidth(item, isMinContent, containingWidth, context);
                float existingWidth = 0;
                int spannedCount = 0;
                for (int r = item.RowStart; r < item.RowStart + item.RowSpan && r < maxRow; r++)
                {
                    existingWidth += rowWidths[r];
                    spannedCount++;
                }
                if (spannedCount > 1)
                {
                    existingWidth += (spannedCount - 1) * rowGap;
                }
                if (itemOuterWidth > existingWidth && spannedCount > 0)
                {
                    float extra = itemOuterWidth - existingWidth;
                    float perRow = extra / spannedCount;
                    for (int r = item.RowStart; r < item.RowStart + item.RowSpan && r < maxRow; r++)
                    {
                        rowWidths[r] += perRow;
                    }
                }
            }

            return SumTrackWidthsAndGaps(rowWidths, rowGap);
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
            /// <summary>Raw negative start line (e.g., -1 for last line). 0 = not set.</summary>
            public int RawRowStart { get; set; }
            /// <summary>Raw negative start line (e.g., -1 for last line). 0 = not set.</summary>
            public int RawColStart { get; set; }
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
                || RawColStart != 0 || RawRowStart != 0
                || RawColEnd != 0 || RawRowEnd != 0
                || ExplicitColEnd >= 0 || ExplicitRowEnd >= 0;
        }
    }
}
