using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
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

            // Container height may not be resolved yet (BFC sets it to 0 before LayoutChildren).
            // Resolve from explicit CSS height so column main-size and cross-axis alignment work.
            if (float.IsNaN(containerHeight) || containerHeight <= 0)
            {
                float explicitH = DimensionResolver.ResolveHeight(style, float.NaN, parent);
                if (!float.IsNaN(explicitH) && explicitH > 0)
                {
                    containerHeight = explicitH;
                }
            }

            // Note: min-height on flex container does NOT establish definite cross-size
            // for stretch. It only provides a floor for the final container height.
            // Percentage children need the ACTUAL resolved height, not min-height.

            // [CSS-FLEXBOX §9.4] Apply max-height to containerHeight so cross-axis
            // stretch doesn't exceed the container's max constraint.
            float containerMaxHeight = style.MaxHeight;
            if (!float.IsNaN(containerMaxHeight) && containerMaxHeight >= 0
                && !DeferredPercent.IsEncoded(containerMaxHeight))
            {
                if (style.BoxSizing == CssBoxSizing.BorderBox)
                {
                    containerMaxHeight -= parent.PaddingTop + parent.PaddingBottom
                        + parent.BorderTopWidth + parent.BorderBottomWidth;
                    if (containerMaxHeight < 0) { containerMaxHeight = 0; }
                }
                if (containerHeight > containerMaxHeight || float.IsNaN(containerHeight) || containerHeight <= 0)
                {
                    containerHeight = containerMaxHeight;
                }
            }

            // [CSS-FLEXBOX §3] In vertical writing modes, row↔column axes are swapped.
            // Row flex in vertical-rl/lr has vertical main axis (like column in horizontal).
            bool isVerticalWM = BlockFormattingContext.IsVerticalWritingMode(style);
            bool isColumn = style.FlexDirection == CssFlexDirection.Column ||
                            style.FlexDirection == CssFlexDirection.ColumnReverse;
            if (isVerticalWM)
            {
                isColumn = !isColumn;
            }
            bool isReverse = style.FlexDirection == CssFlexDirection.RowReverse ||
                             style.FlexDirection == CssFlexDirection.ColumnReverse;
            bool isWrap = style.FlexWrap != CssFlexWrap.Nowrap;

            // [CSS-FLEXBOX §9.4] For ROW flex, min-height provides a definite cross size
            // floor so stretch and percentage resolution work. Only for cross axis (height
            // in row flex), NOT main axis (height in column flex).
            if (!isColumn)
            {
                float containerMinHeight = style.MinHeight;
                if (!float.IsNaN(containerMinHeight) && containerMinHeight > 0
                    && !DeferredPercent.IsEncoded(containerMinHeight))
                {
                    if (style.BoxSizing == CssBoxSizing.BorderBox)
                    {
                        containerMinHeight -= parent.PaddingTop + parent.PaddingBottom
                            + parent.BorderTopWidth + parent.BorderBottomWidth;
                        if (containerMinHeight < 0) { containerMinHeight = 0; }
                    }
                    if (float.IsNaN(containerHeight) || containerHeight < containerMinHeight)
                    {
                        containerHeight = containerMinHeight;
                    }
                }
            }

            float mainSize = isColumn ? containerHeight : containerWidth;
            bool isAutoMainSize = false;
            if (float.IsNaN(mainSize) || mainSize <= 0)
            {
                mainSize = isColumn ? 10000f : containerWidth;
                if (isColumn) { isAutoMainSize = true; }
            }
            // Note: column flex percentage height indefiniteness is complex.
            // The container height may come from stretch, min-height, or abs-pos insets,
            // all of which count as definite. Only the top-level auto-height case is handled
            // via isAutoMainSize above.

            float gap = isColumn ? style.RowGap : style.ColumnGap;
            if (DeferredPercent.IsEncoded(gap))
            {
                gap = DeferredPercent.Resolve(gap, isColumn ? containerHeight : containerWidth);
            }
            if (float.IsNaN(gap) || gap < 0) { gap = 0; }
            float crossGap = isColumn ? style.ColumnGap : style.RowGap;
            if (DeferredPercent.IsEncoded(crossGap))
            {
                crossGap = DeferredPercent.Resolve(crossGap, isColumn ? containerWidth : containerHeight);
            }
            if (float.IsNaN(crossGap) || crossGap < 0) { crossGap = 0; }

            // Collect flex items
            var items = new List<FlexItem>();
            var children = BlockFormattingContext.FlattenContents(styledElement);
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child.IsText)
                {
                    // CSS Flexbox §4: Text directly inside a flex container is wrapped in
                    // an anonymous flex item. Whitespace-only text is not rendered.
                    var textNode = (StyledText)child;
                    if (string.IsNullOrWhiteSpace(textNode.Text)) continue;

                    // Create anonymous block with display:block (not flex!) to avoid recursion
                    var blockStyle = CloneStyleAsBlock(styledElement.Style);
                    var doc = styledElement.Element.OwnerDocument;
                    var anonElement = doc!.CreateElement("div");
                    var anonChildren = new List<StyledNode> { new StyledText(textNode.Text, blockStyle) };
                    var anonStyled = new StyledElement(anonElement, blockStyle, anonChildren);

                    var textBox = new LayoutBox(anonStyled, BoxType.Block);
                    textBox.ContentRect = new RectF(0, 0, containerWidth, 0);
                    var savedFloatCtx2 = context.FloatContext;
                    context.FloatContext = null;
                    InlineFormattingContext.Layout(textBox, context);
                    context.FloatContext = savedFloatCtx2;
                    float textHeight = 0;
                    if (textBox.LineBoxes != null && textBox.LineBoxes.Count > 0)
                    {
                        var lastLine = textBox.LineBoxes[textBox.LineBoxes.Count - 1];
                        textHeight = lastLine.Y + lastLine.Height - textBox.ContentRect.Y;
                    }
                    // Measure actual text width from line fragments
                    float textWidth = 0;
                    if (textBox.LineBoxes != null && textBox.LineBoxes.Count > 0)
                    {
                        for (int lb = 0; lb < textBox.LineBoxes.Count; lb++)
                        {
                            float lw = 0;
                            var line = textBox.LineBoxes[lb];
                            for (int f = 0; f < line.Fragments.Count; f++)
                                lw += line.Fragments[f].Width;
                            if (lw > textWidth) textWidth = lw;
                        }
                    }
                    textBox.ContentRect = new RectF(0, 0, textWidth, textHeight);

                    items.Add(new FlexItem
                    {
                        Box = textBox,
                        Style = blockStyle,
                        FlexGrow = 0,
                        FlexShrink = 1,
                        BaseSize = isColumn ? textHeight : textWidth,
                        Order = 0
                    });
                    continue;
                }

                if (child is StyledPseudoElement pseudo)
                {
                    // CSS Flexbox §4: Pseudo-elements (::before, ::after) on a flex
                    // container are flex items. Create an anonymous StyledElement with
                    // the pseudo's style so box model (margin, padding, border,
                    // explicit width/height) and child layout work correctly.
                    var pseudoStyle = pseudo.Style;
                    var doc = styledElement.Element.OwnerDocument;
                    var pseudoElement = doc!.CreateElement("div");
                    var pseudoChildren = new List<StyledNode>();
                    if (!string.IsNullOrEmpty(pseudo.Content))
                    {
                        pseudoChildren.Add(new StyledText(pseudo.Content, pseudoStyle));
                    }
                    var pseudoStyled = new StyledElement(pseudoElement, pseudoStyle, pseudoChildren);

                    var pseudoBox = new LayoutBox(pseudoStyled, BoxType.Block);
                    BoxModelCalculator.ApplyBoxModel(pseudoBox, pseudoStyle, containerWidth);

                    float pseudoBaseSize = ResolveFlexBasis(pseudoStyle, isColumn, containerWidth, containerHeight, pseudoBox, pseudoStyled, context, isAutoMainSize);
                    items.Add(new FlexItem
                    {
                        Box = pseudoBox,
                        Style = pseudoStyle,
                        FlexGrow = Math.Max(0, pseudoStyle.FlexGrow),
                        FlexShrink = Math.Max(0, pseudoStyle.FlexShrink),
                        BaseSize = pseudoBaseSize,
                        Order = pseudoStyle.Order
                    });
                    continue;
                }

                var childElement = (StyledElement)child;
                if (childElement.Style.Display == CssDisplay.None) continue;

                // Absolutely/fixed positioned items are out of flow
                if (childElement.Style.Position == CssPosition.Absolute ||
                    childElement.Style.Position == CssPosition.Fixed)
                {
                    var posBox = new LayoutBox(childElement, BoxType.Block);
                    BoxModelCalculator.ApplyBoxModel(posBox, childElement.Style, containerWidth);
                    float posWidth = DimensionResolver.ResolveWidth(childElement.Style, containerWidth, posBox);
                    // Pre-resolve explicit height so flex/grid children can center.
                    float posHeight = DimensionResolver.ResolveHeight(childElement.Style, containerHeight, posBox);
                    if (float.IsNaN(posHeight)) posHeight = 0;
                    posBox.ContentRect = new RectF(parent.ContentRect.X, parent.ContentRect.Y, posWidth, posHeight);
                    var savedFc = context.FloatContext;
                    context.FloatContext = null;
                    BlockFormattingContext.LayoutChildren(posBox, context);
                    context.FloatContext = savedFc;
                    if (posHeight <= 0) posHeight = CalculateAutoHeight(posBox);

                    // [CSS-FLEXBOX §9.3] Static position for abspos flex children:
                    // positioned as if the sole flex item, honoring justify-content
                    // and align-items for the static position offset.
                    float staticX = parent.ContentRect.X;
                    float staticY = parent.ContentRect.Y;
                    float outerW = posWidth + posBox.MarginLeft + posBox.MarginRight
                                 + posBox.PaddingLeft + posBox.PaddingRight
                                 + posBox.BorderLeftWidth + posBox.BorderRightWidth;
                    float outerH = posHeight + posBox.MarginTop + posBox.MarginBottom
                                 + posBox.PaddingTop + posBox.PaddingBottom
                                 + posBox.BorderTopWidth + posBox.BorderBottomWidth;
                    // Main-axis: justify-content
                    float mainFree = (isColumn ? containerHeight : containerWidth) - (isColumn ? outerH : outerW);
                    if (mainFree > 0)
                    {
                        var justify = style.JustifyContent;
                        if (justify == CssJustifyContent.Center)
                        {
                            if (isColumn) { staticY += mainFree / 2; }
                            else { staticX += mainFree / 2; }
                        }
                        else if (justify == CssJustifyContent.FlexEnd || justify == CssJustifyContent.End)
                        {
                            if (isColumn) { staticY += mainFree; }
                            else { staticX += mainFree; }
                        }
                    }
                    // Cross-axis: align-items / align-self
                    float crossFree = (isColumn ? containerWidth : containerHeight) - (isColumn ? outerW : outerH);
                    if (crossFree > 0)
                    {
                        var alignSelf = childElement.Style.AlignSelf;
                        if ((int)alignSelf == 255 || (int)alignSelf == 0)
                        {
                            alignSelf = style.AlignItems;
                        }
                        if (alignSelf == CssAlignItems.Center)
                        {
                            if (isColumn) { staticX += crossFree / 2; }
                            else { staticY += crossFree / 2; }
                        }
                        else if (alignSelf == CssAlignItems.FlexEnd)
                        {
                            if (isColumn) { staticX += crossFree; }
                            else { staticY += crossFree; }
                        }
                    }

                    posBox.ContentRect = new RectF(staticX, staticY, posWidth, posHeight);
                    parent.AddChild(posBox);
                    continue;
                }

                var box = new LayoutBox(childElement, BoxType.Block);
                BoxModelCalculator.ApplyBoxModel(box, childElement.Style, containerWidth);

                float baseSize = ResolveFlexBasis(childElement.Style, isColumn, containerWidth, containerHeight, box, childElement, context, isAutoMainSize);
                items.Add(new FlexItem
                {
                    Box = box,
                    Style = childElement.Style,
                    FlexGrow = Math.Max(0, childElement.Style.FlexGrow),
                    FlexShrink = Math.Max(0, childElement.Style.FlexShrink),
                    BaseSize = baseSize,
                    Order = childElement.Style.Order,
                    ContainerWidth = containerWidth,
                    ContainerHeight = containerHeight
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
                // CSS Flexbox §9.3: hypothetical main size = flex base size clamped by min/max.
                float minMain = GetFlexItemMinMain(item, isColumn);

                // CSS Flexbox §4.5: automatic minimum size for flex items.
                // When min-width/height is auto and overflow is visible,
                // the automatic minimum = content-based minimum size.
                if (isColumn)
                {
                    // [CSS-FLEXBOX §4.5] Auto min-height for column flex items.
                    // When min-height is auto (NaN) and overflow-y is visible,
                    // auto min = min(content size suggestion, specified size suggestion).
                    // Content size suggestion = min-content height, clamped by max-height.
                    // Specified size suggestion = computed height if definite (NOT flex-basis).
                    if (minMain <= 0 && item.Style.OverflowY == CssOverflow.Visible
                        && float.IsNaN(item.Style.MinHeight))
                    {
                        // [CSS-FLEXBOX §4.5] When the container has definite height
                        // and the item has a definite height, the specified size suggestion
                        // will clamp the result. If the item's height would make auto min 0
                        // (because content-size ≤ specified-size), we can skip measurement.
                        // But when the container has auto height OR the item has no definite
                        // height, we must measure content.
                        bool shouldMeasure = isAutoMainSize
                            || float.IsNaN(item.Style.Height)
                            || item.Style.Height <= 0;
                        if (shouldMeasure)
                        {
                        float contentMin = ComputeContentMinHeight(item, containerWidth, context);

                        // [CSS-FLEXBOX §4.5] Clamp content size suggestion by max-height
                        float itemMaxH = item.Style.MaxHeight;
                        if (DeferredPercent.IsEncoded(itemMaxH))
                        {
                            itemMaxH = DeferredPercent.Resolve(itemMaxH, containerHeight);
                        }
                        if (!float.IsNaN(itemMaxH) && itemMaxH >= 0 && contentMin > itemMaxH)
                        {
                            contentMin = itemMaxH;
                        }

                        // [CSS-FLEXBOX §4.5] Specified size suggestion = computed height
                        // (not flex-basis) if definite. auto min = min(content, specified).
                        float specHeight = item.Style.Height;
                        if (DeferredPercent.IsEncoded(specHeight))
                        {
                            specHeight = DeferredPercent.Resolve(specHeight, containerHeight);
                        }
                        if (!float.IsNaN(specHeight) && specHeight >= 0)
                        {
                            contentMin = Math.Min(contentMin, specHeight);
                        }

                        if (contentMin > minMain)
                        {
                            minMain = contentMin;
                            item.AutoMinMain = contentMin;
                        }
                        }
                    }
                }
                else
                {
                    // [CSS-FLEXBOX §4.5] Auto min-width for row flex items.
                    // When min-width is auto (NaN) and overflow is visible,
                    // min = min(specified-width or content-width, content-based min).
                    if (minMain <= 0 && item.Style.OverflowX == CssOverflow.Visible
                        && float.IsNaN(item.Style.MinWidth))
                    {
                        float contentMin = ComputeContentMinWidth(item, containerWidth, context);

                        // [CSS-FLEXBOX §4.5] Clamp content size suggestion by max-width
                        float itemMaxW = item.Style.MaxWidth;
                        if (DeferredPercent.IsEncoded(itemMaxW))
                        {
                            itemMaxW = DeferredPercent.Resolve(itemMaxW, containerWidth);
                        }
                        if (!float.IsNaN(itemMaxW) && itemMaxW >= 0 && contentMin > itemMaxW)
                        {
                            contentMin = itemMaxW;
                        }

                        // [CSS-FLEXBOX §4.5] Specified size suggestion = computed width
                        float specWidth = item.Style.Width;
                        if (!float.IsNaN(specWidth) && specWidth >= 0
                            && !DeferredPercent.IsEncoded(specWidth))
                        {
                            contentMin = Math.Min(contentMin, specWidth);
                        }
                        if (contentMin > minMain)
                        {
                            minMain = contentMin;
                            item.AutoMinMain = contentMin;
                        }
                    }
                }

                // [CSS-SIZING §5.2] Resolve sizing keywords for min-width/min-height
                float minMainRaw = isColumn ? item.Style.MinHeight : item.Style.MinWidth;
                if (SizingKeyword.IsSizingKeyword(minMainRaw))
                {
                    float resolvedKeywordMin;
                    if (isColumn)
                    {
                        // Column flex: min-height keyword → measure content height
                        resolvedKeywordMin = ComputeContentMinHeight(item, containerWidth, context);
                    }
                    else if (item.Box.StyledNode is StyledElement minKeywordEl)
                    {
                        // Row flex: min-width keyword → use MeasureIntrinsicWidth with the actual keyword
                        resolvedKeywordMin = BlockFormattingContext.MeasureIntrinsicWidth(
                            minKeywordEl, minMainRaw, containerWidth, context);
                    }
                    else
                    {
                        resolvedKeywordMin = 0;
                    }
                    item.SizingKeywordMinMain = resolvedKeywordMin;
                    if (resolvedKeywordMin > minMain)
                    {
                        minMain = resolvedKeywordMin;
                    }
                }

                float maxMain = isColumn ? item.Style.MaxHeight : item.Style.MaxWidth;
                if (DeferredPercent.IsEncoded(maxMain))
                {
                    maxMain = DeferredPercent.Resolve(maxMain, isColumn ? containerHeight : containerWidth);
                }
                // [CSS-SIZING §5.2] border-box: max-width/height includes padding+border
                if (!float.IsNaN(maxMain) && maxMain >= 0
                    && item.Style.BoxSizing == CssBoxSizing.BorderBox)
                {
                    if (isColumn)
                    {
                        maxMain -= item.Box.PaddingTop + item.Box.PaddingBottom
                                 + item.Box.BorderTopWidth + item.Box.BorderBottomWidth;
                    }
                    else
                    {
                        maxMain -= item.Box.PaddingLeft + item.Box.PaddingRight
                                 + item.Box.BorderLeftWidth + item.Box.BorderRightWidth;
                    }
                    if (maxMain < 0) { maxMain = 0; }
                }
                float clampedBase = Math.Max(item.BaseSize, minMain);
                if (!float.IsNaN(maxMain) && maxMain >= 0)
                {
                    clampedBase = Math.Min(clampedBase, maxMain);
                }
                float itemMain = clampedBase + GetItemMainMargins(item, isColumn);
                // Include the gap that would precede this item on the current line.
                float neededMain = itemMain + (currentLine.Items.Count > 0 ? gap : 0);

                if (isWrap && usedMain + neededMain > mainSize && currentLine.Items.Count > 0)
                {
                    lines.Add(currentLine);
                    currentLine = new FlexLine();
                    usedMain = 0;
                    neededMain = itemMain; // first item on new line: no gap
                }

                currentLine.Items.Add(item);
                usedMain += neededMain;
            }
            if (currentLine.Items.Count > 0)
                lines.Add(currentLine);

            // flex-wrap: wrap-reverse reverses cross-axis line order
            if (style.FlexWrap == CssFlexWrap.WrapReverse && lines.Count > 1)
                lines.Reverse();

            // Resolve flexible lengths and position items
            float crossCursor = isColumn ? parent.ContentRect.X : parent.ContentRect.Y;

            for (int li = 0; li < lines.Count; li++)
            {
                var line = lines[li];
                float totalGaps = (line.Items.Count - 1) * gap;

                ResolveFlexibleLengths(line, mainSize, totalGaps, isAutoMainSize, isColumn);

                // Distribute auto margins on the main axis (overrides justify-content)
                bool hasAutoMargins = DistributeAutoMargins(line, mainSize, totalGaps, isColumn);

                // Position items on main axis
                float mainCursor = isColumn ? parent.ContentRect.Y : parent.ContentRect.X;

                // For auto-sized column containers (no definite height), justify-content
                // has no effect since there's no definite free space.
                var effectiveJustify = style.JustifyContent;
                if (isAutoMainSize)
                {
                    effectiveJustify = CssJustifyContent.FlexStart;
                }
                // [CSS-FLEXBOX §8.2] For reverse directions, swap flex-start ↔ flex-end.
                // Only applies to flex-relative keywords, NOT physical ones.
                else if (isReverse)
                {
                    if (effectiveJustify == CssJustifyContent.FlexStart)
                    {
                        effectiveJustify = CssJustifyContent.FlexEnd;
                    }
                    else if (effectiveJustify == CssJustifyContent.FlexEnd)
                    {
                        effectiveJustify = CssJustifyContent.FlexStart;
                    }
                }
                // [CSS-ALIGN §6.1] Physical/logical keywords resolve AFTER flex-relative swap.
                // start/end: writing-mode start/end. For LTR: start=left, end=right.
                // left/right: physical. For row: left=start, right=end.
                // For column: left/right/start/end map to flex-start (main axis is vertical,
                // horizontal directions don't apply).
                if (effectiveJustify == CssJustifyContent.Start
                    || effectiveJustify == CssJustifyContent.Left)
                {
                    // LTR start/left: for row = physical left = flex-start.
                    // For column = no horizontal axis → flex-start (top).
                    effectiveJustify = CssJustifyContent.FlexStart;
                }
                else if (effectiveJustify == CssJustifyContent.End
                         || effectiveJustify == CssJustifyContent.Right)
                {
                    // LTR end/right: for row = physical right = flex-end.
                    // For column = no horizontal axis → flex-start (per spec: maps to start).
                    effectiveJustify = isColumn
                        ? CssJustifyContent.FlexStart
                        : CssJustifyContent.FlexEnd;
                }

                // Compute remaining free space for justify-content
                float justifyFreeSpace = mainSize - totalGaps;
                for (int i = 0; i < line.Items.Count; i++)
                {
                    justifyFreeSpace -= line.Items[i].ResolvedMainSize + GetItemMainMargins(line.Items[i], isColumn);
                }

                var (startOffset, justifyGap) = hasAutoMargins
                    ? (0f, gap)
                    : ApplyJustifyContent(effectiveJustify, justifyFreeSpace, line.Items.Count, gap);
                mainCursor += startOffset;

                float maxCross = 0;
                for (int i = 0; i < line.Items.Count; i++)
                {
                    if (i > 0) mainCursor += justifyGap;
                    var item = line.Items[i];
                    var box = item.Box;

                    float contentMain = item.ResolvedMainSize;
                    // Apply min/max main size — for auto-height containers, the flex
                    // resolution may give 0 but min-height should still constrain.
                    float itemMinMain = GetFlexItemMinMain(item, isColumn);
                    if (contentMain < itemMinMain)
                    {
                        contentMain = itemMinMain;
                    }
                    float itemMaxMain = isColumn ? item.Style.MaxHeight : item.Style.MaxWidth;
                    if (DeferredPercent.IsEncoded(itemMaxMain))
                    {
                        itemMaxMain = DeferredPercent.Resolve(itemMaxMain, isColumn ? containerHeight : containerWidth);
                    }
                    if (!float.IsNaN(itemMaxMain) && itemMaxMain >= 0 && contentMain > itemMaxMain)
                    {
                        contentMain = itemMaxMain;
                    }
                    float contentCross;

                    if (isColumn)
                    {
                        // Check alignment: non-stretch items use fit-content width
                        var itemAlign = item.Style.AlignSelf;
                        if ((int)itemAlign == 255)
                        {
                            itemAlign = style.AlignItems;
                        }
                        // CSS Flexbox §8.1: auto margins on the cross axis override
                        // align-self:stretch — the item uses fit-content width instead.
                        bool hasCrossAutoMargin = float.IsNaN(item.Style.MarginLeft)
                                                || float.IsNaN(item.Style.MarginRight);
                        bool shouldStretch = (itemAlign == CssAlignItems.Stretch || (int)itemAlign == 0)
                                           && float.IsNaN(item.Style.Width)
                                           && !hasCrossAutoMargin;

                        if (shouldStretch)
                        {
                            contentCross = DimensionResolver.ResolveWidth(item.Style, containerWidth, box);
                        }
                        else if (!float.IsNaN(item.Style.Width))
                        {
                            contentCross = DimensionResolver.ResolveWidth(item.Style, containerWidth, box);
                        }
                        else
                        {
                            // align-self: start/end/center/baseline with auto width
                            // → fit-content (shrink-to-fit) width
                            var measureBox = new LayoutBox(box.StyledNode, BoxType.Block);
                            BoxModelCalculator.ApplyBoxModel(measureBox, item.Style, containerWidth);
                            float availW = containerWidth - BoxModelCalculator.GetHorizontalSpacing(measureBox);

                            // Apply max-width constraint BEFORE measuring so children
                            // lay out within the constrained width.
                            float crossMax = item.Style.MaxWidth;
                            if (DeferredPercent.IsEncoded(crossMax))
                            {
                                crossMax = DeferredPercent.Resolve(crossMax, containerWidth);
                            }
                            // [CSS-SIZING §5.2] Resolve sizing keywords for max-width
                            if (SizingKeyword.IsSizingKeyword(crossMax)
                                && item.Box.StyledNode is StyledElement maxWEl)
                            {
                                crossMax = BlockFormattingContext.MeasureIntrinsicWidth(
                                    maxWEl, crossMax, containerWidth, context);
                            }
                            if (!float.IsNaN(crossMax) && crossMax >= 0)
                            {
                                if (item.Style.BoxSizing == CssBoxSizing.BorderBox)
                                {
                                    crossMax -= box.PaddingLeft + box.PaddingRight
                                              + box.BorderLeftWidth + box.BorderRightWidth;
                                    if (crossMax < 0) { crossMax = 0; }
                                }
                                if (availW > crossMax) { availW = crossMax; }
                            }

                            measureBox.ContentRect = new RectF(0, 0, availW, contentMain);
                            var savedCtx = context.FloatContext;
                            context.FloatContext = null;
                            BlockFormattingContext.LayoutChildren(measureBox, context);
                            context.FloatContext = savedCtx;
                            contentCross = BlockFormattingContext.GetContentExtent(measureBox);
                            if (contentCross > availW)
                            {
                                contentCross = availW;
                            }
                        }

                        // CSS Sizing 4: if width is auto and aspect-ratio is set,
                        // derive width from the resolved main size (height).
                        if (float.IsNaN(item.Style.Width) && contentMain > 0)
                        {
                            float aspectRatio = DimensionResolver.GetAspectRatio(item.Style);
                            if (aspectRatio > 0)
                            {
                                float arWidth = contentMain * aspectRatio;
                                if (item.Style.BoxSizing == CssBoxSizing.BorderBox)
                                {
                                    arWidth -= (box.PaddingLeft + box.PaddingRight
                                              + box.BorderLeftWidth + box.BorderRightWidth);
                                }
                                if (arWidth > 0)
                                {
                                    contentCross = arWidth;
                                }
                            }
                        }

                        // [CSS-SIZING §5.2] Apply cross-axis min/max constraints
                        float crossMinW = DimensionResolver.ResolvePercentWidth(item.Style.MinWidth, containerWidth);
                        float crossMaxW = DimensionResolver.ResolvePercentWidth(item.Style.MaxWidth, containerWidth);
                        // [CSS-SIZING §5.2] Resolve sizing keywords for min/max-width
                        if (SizingKeyword.IsSizingKeyword(item.Style.MaxWidth)
                            && item.Box.StyledNode is StyledElement crossMaxEl)
                        {
                            crossMaxW = BlockFormattingContext.MeasureIntrinsicWidth(
                                crossMaxEl, item.Style.MaxWidth, containerWidth, context);
                        }
                        if (SizingKeyword.IsSizingKeyword(item.Style.MinWidth)
                            && item.Box.StyledNode is StyledElement crossMinEl)
                        {
                            crossMinW = BlockFormattingContext.MeasureIntrinsicWidth(
                                crossMinEl, item.Style.MinWidth, containerWidth, context);
                        }
                        if (item.Style.BoxSizing == CssBoxSizing.BorderBox)
                        {
                            float hExtra = box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth;
                            if (!float.IsNaN(crossMinW) && crossMinW >= 0) { crossMinW = Math.Max(0, crossMinW - hExtra); }
                            if (!float.IsNaN(crossMaxW) && crossMaxW >= 0) { crossMaxW = Math.Max(0, crossMaxW - hExtra); }
                        }
                        if (!float.IsNaN(crossMaxW) && crossMaxW >= 0 && contentCross > crossMaxW)
                        {
                            contentCross = crossMaxW;
                        }
                        if (!float.IsNaN(crossMinW) && crossMinW >= 0 && contentCross < crossMinW)
                        {
                            contentCross = crossMinW;
                        }

                        box.ContentRect = new RectF(
                            crossCursor + box.MarginLeft + box.BorderLeftWidth + box.PaddingLeft,
                            mainCursor + box.MarginTop + box.BorderTopWidth + box.PaddingTop,
                            contentCross, contentMain);
                    }
                    else
                    {
                        // Set content width on box before resolving height so
                        // aspect-ratio can derive height from width.
                        box.ContentRect = new RectF(0, 0, contentMain, 0);
                        float specHeight = DimensionResolver.ResolveHeight(item.Style, containerHeight, box);
                        // [CSS-SIZING-4 §5.1] If height resolves to 0 (unset = auto) and
                        // aspect-ratio is set, derive height from the main size.
                        if ((float.IsNaN(specHeight) || specHeight <= 0) && contentMain > 0)
                        {
                            float itemAspectRatio = DimensionResolver.GetAspectRatio(item.Style);
                            if (itemAspectRatio > 0)
                            {
                                if (item.Style.BoxSizing == CssBoxSizing.BorderBox)
                                {
                                    float borderBoxWidth = contentMain
                                        + box.PaddingLeft + box.PaddingRight
                                        + box.BorderLeftWidth + box.BorderRightWidth;
                                    float borderBoxHeight = borderBoxWidth / itemAspectRatio;
                                    specHeight = borderBoxHeight
                                        - box.PaddingTop - box.PaddingBottom
                                        - box.BorderTopWidth - box.BorderBottomWidth;
                                    if (specHeight < 0) { specHeight = 0; }
                                }
                                else
                                {
                                    specHeight = contentMain / itemAspectRatio;
                                }
                            }
                        }
                        contentCross = float.IsNaN(specHeight) ? 0 : specHeight;

                        // Pre-stretch: when item has auto height and will be stretched,
                        // set cross size to the container's definite cross dimension BEFORE
                        // calling LayoutChildren. This ensures nested column flex containers
                        // receive the correct main-axis size instead of falling back to 10000f.
                        if (contentCross <= 0 && !float.IsNaN(containerHeight) && containerHeight > 0)
                        {
                            var preAlign = item.Style.AlignSelf;
                            if ((int)preAlign == 255) preAlign = style.AlignItems;
                            // CSS Flexbox §8.1: auto margins on cross axis override stretch
                            bool hasRowCrossAutoMargin = float.IsNaN(item.Style.MarginTop)
                                                       || float.IsNaN(item.Style.MarginBottom);
                            if ((preAlign == CssAlignItems.Stretch || (int)preAlign == 0)
                                && !hasRowCrossAutoMargin)
                            {
                                contentCross = containerHeight
                                    - box.PaddingTop - box.PaddingBottom
                                    - box.BorderTopWidth - box.BorderBottomWidth
                                    - box.MarginTop - box.MarginBottom;
                                if (contentCross < 0) { contentCross = 0; }
                                box.HasDefiniteCrossSize = true;
                            }
                        }

                        // CSS Flexbox §9.8: Flex items have definite cross sizes after
                        // flexing. For row flex items with auto height, apply min-height
                        // as a floor before laying out children so that percentage-height
                        // children (e.g., height:100%) resolve against the min-height
                        // rather than 0.
                        if (contentCross <= 0)
                        {
                            float itemMinHeight = DimensionResolver.ResolvePercentHeight(
                                item.Style.MinHeight, containerHeight);
                            if (!float.IsNaN(itemMinHeight) && itemMinHeight > 0)
                            {
                                contentCross = itemMinHeight;
                            }
                        }

                        box.ContentRect = new RectF(
                            mainCursor + box.MarginLeft + box.BorderLeftWidth + box.PaddingLeft,
                            crossCursor + box.MarginTop + box.BorderTopWidth + box.PaddingTop,
                            contentMain, contentCross);
                    }

                    // Set parent reference before layout so margin collapsing
                    // can detect that this box is a flex item (establishes BFC).
                    box.Parent = parent;

                    // Layout item contents.
                    // For anonymous text items (which already have line boxes from initial
                    // inline layout), skip re-layout but offset line boxes to match the
                    // positioned ContentRect. Re-running IFC would use ContentRect.X as
                    // startX, double-counting the flex offset.
                    // However, if flex-shrink reduced the main size, text must re-wrap.
                    if (box.LineBoxes != null && box.LineBoxes.Count > 0)
                    {
                        // Check if the resolved width differs from the original layout width —
                        // if so, text needs to re-wrap at the new constrained width.
                        bool needsRelayout = false;
                        if (!isColumn)
                        {
                            float origWidth = 0;
                            for (int lb = 0; lb < box.LineBoxes.Count; lb++)
                            {
                                float lw = 0;
                                for (int f = 0; f < box.LineBoxes[lb].Fragments.Count; f++)
                                {
                                    lw += box.LineBoxes[lb].Fragments[f].Width;
                                }
                                if (lw > origWidth) origWidth = lw;
                            }
                            if (contentMain < origWidth - 0.5f)
                            {
                                needsRelayout = true;
                            }
                        }

                        if (needsRelayout)
                        {
                            // Clear line boxes and re-layout with constrained width.
                            // Save ContentRect position, then reset for IFC layout.
                            float savedX = box.ContentRect.X;
                            float savedY = box.ContentRect.Y;
                            box.LineBoxes.Clear();
                            box.ClearChildren();
                            box.ContentRect = new RectF(0, 0, contentMain, 0);
                            var savedFloatCtx2 = context.FloatContext;
                            context.FloatContext = null;
                            InlineFormattingContext.Layout(box, context);
                            context.FloatContext = savedFloatCtx2;
                            // Recalculate height from re-wrapped line boxes
                            float newHeight = 0;
                            if (box.LineBoxes != null && box.LineBoxes.Count > 0)
                            {
                                var lastLb = box.LineBoxes[box.LineBoxes.Count - 1];
                                newHeight = lastLb.Y + lastLb.Height;
                            }
                            box.ContentRect = new RectF(savedX, savedY, contentMain, newHeight);
                            // Offset line boxes to final position
                            if (savedX != 0 || savedY != 0)
                            {
                                for (int lbi = 0; lbi < box.LineBoxes!.Count; lbi++)
                                {
                                    box.LineBoxes[lbi].X += savedX;
                                    box.LineBoxes[lbi].Y += savedY;
                                }
                            }
                        }
                        else
                        {
                            // Offset existing line boxes to match the new ContentRect position
                            float lbDx = box.ContentRect.X;
                            float lbDy = box.ContentRect.Y;
                            if (lbDx != 0 || lbDy != 0)
                            {
                                for (int lbi = 0; lbi < box.LineBoxes.Count; lbi++)
                                {
                                    box.LineBoxes[lbi].X += lbDx;
                                    box.LineBoxes[lbi].Y += lbDy;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Flex items establish independent formatting contexts — clear the
                        // parent float context so it doesn't leak absolute coordinates.
                        var savedFloatCtx = context.FloatContext;
                        context.FloatContext = null;
                        BlockFormattingContext.LayoutChildren(box, context);
                        context.FloatContext = savedFloatCtx;
                    }

                    // Resolve auto cross size
                    if (isColumn)
                    {
                        // Width already resolved
                    }
                    else if (float.IsNaN(item.Style.Height))
                    {
                        // [CSS-SIZING-4 §5.1] If aspect-ratio already resolved a definite
                        // cross size, keep it — don't override with content-based height.
                        if (DimensionResolver.GetAspectRatio(item.Style) <= 0 || contentCross <= 0)
                        {
                            contentCross = CalculateAutoHeight(box);
                            // Replaced elements (form controls, images) have intrinsic cross size
                            if (contentCross <= 0 && box.StyledNode is StyledElement stEl
                                && ReplacedElementLayout.IsReplaced(stEl))
                            {
                                contentCross = ReplacedElementLayout.GetFormControlIntrinsicHeight(stEl);
                            }
                        }
                        // [CSS2 §10.7] Apply cross-axis min/max constraints for row flex
                        float crossMinH = DimensionResolver.ResolvePercentHeight(item.Style.MinHeight, containerHeight);
                        float crossMaxH = DimensionResolver.ResolvePercentHeight(item.Style.MaxHeight, containerHeight);
                        if (!float.IsNaN(crossMaxH) && crossMaxH >= 0 && contentCross > crossMaxH)
                        {
                            contentCross = crossMaxH;
                        }
                        if (!float.IsNaN(crossMinH) && crossMinH >= 0 && contentCross < crossMinH)
                        {
                            contentCross = crossMinH;
                        }
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

                // For single-line flex containers with definite cross size,
                // the line's cross size equals the container's inner cross size.
                if (!isWrap || lines.Count == 1)
                {
                    float containerCross = isColumn ? containerWidth : containerHeight;
                    if (!float.IsNaN(containerCross) && containerCross > 0 && containerCross > maxCross)
                    {
                        maxCross = containerCross;
                    }
                }

                // [CSS-FLEXBOX §8.3] Apply cross-axis alignment (align-items / align-self)
                AlignCrossAxis(line, style, maxCross, isColumn, containerWidth, containerHeight, context);

                crossCursor += maxCross;
                if (li < lines.Count - 1)
                    crossCursor += crossGap;
            }

            // [CSS-FLEXBOX §9.4] Apply align-content for multi-line flex containers
            if (isWrap && lines.Count > 1)
            {
                ApplyAlignContent(lines, style, isColumn, containerWidth, containerHeight, crossGap);
            }

            // [CSS-FLEXBOX §9.4] flex-wrap: wrap-reverse swaps cross-start/end.
            // For single-line containers, shift items to the cross-end.
            if (style.FlexWrap == CssFlexWrap.WrapReverse)
            {
                float containerCross = isColumn ? containerWidth : containerHeight;
                if (!float.IsNaN(containerCross) && containerCross > 0)
                {
                    float totalCrossUsed = 0;
                    for (int li = 0; li < lines.Count; li++)
                    {
                        totalCrossUsed += lines[li].CrossSize;
                        if (li < lines.Count - 1) { totalCrossUsed += crossGap; }
                    }
                    float shift = containerCross - totalCrossUsed;
                    if (shift > 0.5f)
                    {
                        for (int li = 0; li < lines.Count; li++)
                        {
                            for (int ii = 0; ii < lines[li].Items.Count; ii++)
                            {
                                if (isColumn)
                                {
                                    OffsetBoxInPlace(lines[li].Items[ii].Box, shift, 0);
                                }
                                else
                                {
                                    OffsetBoxInPlace(lines[li].Items[ii].Box, 0, shift);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// CSS Flexbox §9.9.1: Compute the intrinsic width of a flex container.
        /// For max-content: row flex sums all items' max-content contributions + gaps;
        ///                   column flex takes the max of all items' max-content widths.
        /// For min-content: row flex takes the largest item's min-content contribution;
        ///                  column flex takes the max of all items' min-content widths.
        /// </summary>
        internal static float ComputeIntrinsicWidth(StyledElement styledElement, float keyword,
                                                     float containingWidth, LayoutContext context)
        {
            var style = styledElement.Style;
            bool isColumn = style.FlexDirection == CssFlexDirection.Column ||
                            style.FlexDirection == CssFlexDirection.ColumnReverse;
            bool isMinContent = keyword == SizingKeyword.MinContent;

            float mainAxisGap = isColumn ? style.RowGap : style.ColumnGap;
            if (float.IsNaN(mainAxisGap))
            {
                mainAxisGap = 0;
            }

            var children = BlockFormattingContext.FlattenContents(styledElement);
            float totalItemWidth = 0;
            float maxItemWidth = 0;
            int itemCount = 0;

            for (int childIndex = 0; childIndex < children.Count; childIndex++)
            {
                var child = children[childIndex];

                if (child.IsText)
                {
                    var textNode = (StyledText)child;
                    if (string.IsNullOrWhiteSpace(textNode.Text))
                    {
                        continue;
                    }

                    float textWidth = MeasureTextItemWidth(styledElement, textNode, isMinContent, containingWidth, context);
                    totalItemWidth += textWidth;
                    if (textWidth > maxItemWidth)
                    {
                        maxItemWidth = textWidth;
                    }
                    itemCount++;
                    continue;
                }

                if (child is StyledPseudoElement pseudo)
                {
                    float pseudoWidth = MeasurePseudoItemWidth(pseudo, containingWidth, context);
                    totalItemWidth += pseudoWidth;
                    if (pseudoWidth > maxItemWidth)
                    {
                        maxItemWidth = pseudoWidth;
                    }
                    itemCount++;
                    continue;
                }

                var childElement = (StyledElement)child;
                if (childElement.Style.Display == CssDisplay.None)
                {
                    continue;
                }

                if (childElement.Style.Position == CssPosition.Absolute ||
                    childElement.Style.Position == CssPosition.Fixed)
                {
                    continue;
                }

                float itemWidth = MeasureFlexItemIntrinsicWidth(childElement, isColumn, isMinContent, containingWidth, context);
                totalItemWidth += itemWidth;
                if (itemWidth > maxItemWidth)
                {
                    maxItemWidth = itemWidth;
                }
                itemCount++;
            }

            if (itemCount == 0)
            {
                return 0;
            }

            float totalGaps = (itemCount - 1) * mainAxisGap;

            if (isColumn)
            {
                // [CSS-FLEXBOX §9.9.1] For column wrap with definite height,
                // max-content width = sum of column widths needed to fit all items.
                bool isWrap = style.FlexWrap != CssFlexWrap.Nowrap;
                float containerHeight = 0;
                if (!float.IsNaN(style.Height) && style.Height > 0
                    && !DeferredPercent.IsEncoded(style.Height))
                {
                    containerHeight = style.Height;
                }
                // [CSS-FLEXBOX §9.9.1] max-height also constrains column wrap
                if (containerHeight <= 0)
                {
                    float maxH = style.MaxHeight;
                    if (!float.IsNaN(maxH) && maxH > 0 && !DeferredPercent.IsEncoded(maxH))
                    {
                        containerHeight = maxH;
                    }
                }
                if (isWrap && containerHeight > 0 && !isMinContent)
                {
                    // [CSS-FLEXBOX §5.4] Sort items by order for wrapping calculation
                    var orderedChildren = new System.Collections.Generic.List<StyledElement>();
                    for (int oi = 0; oi < children.Count; oi++)
                    {
                        if (children[oi] is StyledElement oe && !oe.IsText
                            && oe.Style.Display != CssDisplay.None
                            && oe.Style.Position != CssPosition.Absolute
                            && oe.Style.Position != CssPosition.Fixed)
                        {
                            orderedChildren.Add(oe);
                        }
                    }
                    orderedChildren.Sort((a, b) => a.Style.Order.CompareTo(b.Style.Order));

                    float columnWidth = 0;
                    float totalWidth = 0;
                    float usedHeight = 0;
                    int colItemIndex = 0;
                    for (int ci = 0; ci < orderedChildren.Count; ci++)
                    {
                        var ce = orderedChildren[ci];

                        float itemHeight = ResolveFlexBasis(ce.Style, true, containingWidth, containerHeight,
                            new LayoutBox(ce, BoxType.Block), ce, context);
                        float itemWidth = MeasureFlexItemIntrinsicWidth(ce, true, false, containingWidth, context);

                        if (colItemIndex > 0 && usedHeight + itemHeight > containerHeight)
                        {
                            totalWidth += columnWidth;
                            columnWidth = 0;
                            usedHeight = 0;
                        }
                        if (itemWidth > columnWidth)
                        {
                            columnWidth = itemWidth;
                        }
                        usedHeight += itemHeight;
                        colItemIndex++;
                    }
                    totalWidth += columnWidth;
                    return totalWidth;
                }
                return maxItemWidth;
            }
            else
            {
                // [CSS-FLEXBOX §9.9.1] Row flex intrinsic main size.
                // For wrapping containers, min-content = widest single item.
                // For single-line containers, min-content = sum of all items' min-content + gaps
                // (all items are forced onto one line).
                bool isWrap = style.FlexWrap != CssFlexWrap.Nowrap;
                if (isMinContent && isWrap)
                {
                    return maxItemWidth;
                }
                return totalItemWidth + totalGaps;
            }
        }

        /// <summary>
        /// Measures the intrinsic width contribution of an anonymous text flex item.
        /// </summary>
        private static float MeasureTextItemWidth(StyledElement parentElement, StyledText textNode,
                                                   bool isMinContent, float containingWidth, LayoutContext context)
        {
            var blockStyle = CloneStyleAsBlock(parentElement.Style);
            var doc = parentElement.Element.OwnerDocument;
            var anonElement = doc!.CreateElement("div");
            var anonChildren = new List<StyledNode> { new StyledText(textNode.Text, blockStyle) };
            var anonStyled = new StyledElement(anonElement, blockStyle, anonChildren);

            float measureWidth = isMinContent ? 1f : 10000f;
            var textBox = new LayoutBox(anonStyled, BoxType.Block);
            textBox.ContentRect = new RectF(0, 0, measureWidth, 0);
            var savedFloatCtx = context.FloatContext;
            context.FloatContext = null;
            InlineFormattingContext.Layout(textBox, context);
            context.FloatContext = savedFloatCtx;

            float textWidth = 0;
            if (textBox.LineBoxes != null)
            {
                for (int lineIndex = 0; lineIndex < textBox.LineBoxes.Count; lineIndex++)
                {
                    float lineWidth = textBox.LineBoxes[lineIndex].NaturalContentWidth;
                    if (lineWidth > textWidth)
                    {
                        textWidth = lineWidth;
                    }
                }
            }
            return textWidth;
        }

        /// <summary>
        /// Measures the intrinsic width contribution of a pseudo-element flex item.
        /// Accounts for explicit CSS width, margin, padding, and border on the pseudo.
        /// </summary>
        private static float MeasurePseudoItemWidth(StyledPseudoElement pseudo, float containingWidth, LayoutContext context)
        {
            var pseudoStyle = pseudo.Style;
            var measureBox = new LayoutBox(pseudo, BoxType.Block);
            BoxModelCalculator.ApplyBoxModel(measureBox, pseudoStyle, containingWidth);

            float contentWidth;
            float specifiedWidth = pseudoStyle.Width;
            if (!float.IsNaN(specifiedWidth) && specifiedWidth >= 0)
            {
                contentWidth = specifiedWidth;
                if (pseudoStyle.BoxSizing == CssBoxSizing.BorderBox)
                {
                    contentWidth -= measureBox.PaddingLeft + measureBox.PaddingRight
                                  + measureBox.BorderLeftWidth + measureBox.BorderRightWidth;
                    if (contentWidth < 0)
                    {
                        contentWidth = 0;
                    }
                }
            }
            else
            {
                float fontSize = pseudoStyle.FontSize;
                if (context.TextMeasurer != null && !string.IsNullOrEmpty(pseudo.Content))
                {
                    var fontDescriptor = new Fonts.FontDescriptor(pseudoStyle.FontFamilies,
                        pseudoStyle.FontWeight, pseudoStyle.FontStyle);
                    var shaped = context.TextMeasurer.Shape(pseudo.Content, fontDescriptor, fontSize);
                    contentWidth = shaped.TotalWidth;
                }
                else
                {
                    contentWidth = (pseudo.Content?.Length ?? 0) * (pseudoStyle.FontSize) * 0.6f;
                }
            }

            return contentWidth + measureBox.PaddingLeft + measureBox.PaddingRight
                 + measureBox.BorderLeftWidth + measureBox.BorderRightWidth
                 + measureBox.MarginLeft + measureBox.MarginRight;
        }

        /// <summary>
        /// Measures the intrinsic width contribution of a regular flex item (non-text, non-pseudo).
        /// For row flex: returns the item's outer width (content + padding + border + margin).
        /// For column flex: returns the item's max-content or min-content width.
        /// </summary>
        private static float MeasureFlexItemIntrinsicWidth(StyledElement childElement, bool isColumn,
                                                            bool isMinContent, float containingWidth,
                                                            LayoutContext context)
        {
            var childStyle = childElement.Style;
            var measureBox = new LayoutBox(childElement, BoxType.Block);
            BoxModelCalculator.ApplyBoxModel(measureBox, childStyle, containingWidth);
            float horizontalExtra = measureBox.PaddingLeft + measureBox.PaddingRight
                                  + measureBox.BorderLeftWidth + measureBox.BorderRightWidth
                                  + measureBox.MarginLeft + measureBox.MarginRight;

            if (isColumn)
            {
                // [CSS-FLEXBOX §9.9.1] For column flex, the item's cross-axis (width)
                // contribution uses explicit width if set, else content measurement.
                float specWidth = childStyle.Width;
                if (!float.IsNaN(specWidth) && specWidth >= 0 && !SizingKeyword.IsSizingKeyword(specWidth)
                    && !DeferredPercent.IsEncoded(specWidth))
                {
                    if (childStyle.BoxSizing == CssBoxSizing.BorderBox)
                    {
                        specWidth -= measureBox.PaddingLeft + measureBox.PaddingRight
                                   + measureBox.BorderLeftWidth + measureBox.BorderRightWidth;
                        if (specWidth < 0) { specWidth = 0; }
                    }
                    return specWidth + horizontalExtra;
                }
                float itemContentWidth = MeasureChildIntrinsicContentWidth(childElement, childStyle,
                    measureBox, isMinContent, containingWidth, context);
                return itemContentWidth + horizontalExtra;
            }

            float flexBasis = childStyle.FlexBasis;
            bool hasBasis = !float.IsNaN(flexBasis) && flexBasis >= 0;

            if (hasBasis)
            {
                if (childStyle.BoxSizing == CssBoxSizing.BorderBox)
                {
                    flexBasis -= (measureBox.PaddingLeft + measureBox.PaddingRight
                                + measureBox.BorderLeftWidth + measureBox.BorderRightWidth);
                    if (flexBasis < 0)
                    {
                        flexBasis = 0;
                    }
                }

                // [CSS-FLEXBOX §9.9.1] Max-content contribution is clamped by auto min-width.
                // When min-width is auto and overflow is visible, the content-based minimum
                // provides a floor for the intrinsic contribution.
                float minW = childStyle.MinWidth;
                bool autoMinWidth = float.IsNaN(minW) || minW < 0;
                if (autoMinWidth && childStyle.OverflowX == CssOverflow.Visible && !isMinContent)
                {
                    float contentMin = MeasureChildIntrinsicContentWidth(childElement, childStyle,
                        measureBox, true, containingWidth, context);
                    if (contentMin > flexBasis)
                    {
                        flexBasis = contentMin;
                    }
                }

                return flexBasis + horizontalExtra;
            }

            float specifiedWidth = childStyle.Width;
            if (!float.IsNaN(specifiedWidth) && specifiedWidth >= 0
                && !SizingKeyword.IsSizingKeyword(specifiedWidth)
                && !DeferredPercent.IsEncoded(specifiedWidth))
            {
                float itemWidth = specifiedWidth;
                if (childStyle.BoxSizing == CssBoxSizing.BorderBox)
                {
                    itemWidth -= (measureBox.PaddingLeft + measureBox.PaddingRight
                                + measureBox.BorderLeftWidth + measureBox.BorderRightWidth);
                    if (itemWidth < 0)
                    {
                        itemWidth = 0;
                    }
                }
                return itemWidth + horizontalExtra;
            }

            float contentWidth = MeasureChildIntrinsicContentWidth(childElement, childStyle,
                measureBox, isMinContent, containingWidth, context);
            return contentWidth + horizontalExtra;
        }

        /// <summary>
        /// Measures a flex item child's intrinsic content width via trial layout.
        /// </summary>
        private static float MeasureChildIntrinsicContentWidth(StyledElement childElement,
                                                                ComputedStyle childStyle,
                                                                LayoutBox measureBox,
                                                                bool isMinContent,
                                                                float containingWidth,
                                                                LayoutContext context)
        {
            float measureWidth = isMinContent ? 1f : 10000f;
            measureBox.ContentRect = new RectF(0, 0, measureWidth, 0);
            var savedFloatCtx = context.FloatContext;
            context.FloatContext = null;
            BlockFormattingContext.LayoutChildren(measureBox, context);
            context.FloatContext = savedFloatCtx;
            float contentWidth = BlockFormattingContext.GetContentExtent(measureBox);
            // For max-content, cap at the measure width to avoid overflow artifacts.
            // For min-content, don't cap — content can exceed the 1px measure width.
            if (!isMinContent && contentWidth > measureWidth)
            {
                contentWidth = measureWidth;
            }
            return contentWidth;
        }

        private static float ResolveFlexBasis(ComputedStyle style, bool isColumn, float containerWidth,
            float containerHeight, LayoutBox box, StyledElement element, LayoutContext context,
            bool isAutoMainSize = false)
        {
            float basis = style.FlexBasis;
            if (!float.IsNaN(basis) && basis >= 0)
            {
                // When box-sizing is border-box, flex-basis includes padding+border.
                // Convert to content-box since flex algorithm works with content sizes.
                if (style.BoxSizing == CssBoxSizing.BorderBox)
                {
                    if (isColumn)
                    {
                        basis -= (box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth);
                    }
                    else
                    {
                        basis -= (box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth);
                    }
                    if (basis < 0)
                    {
                        basis = 0;
                    }
                }
                return basis;
            }
            // Resolve deferred calc() or percentage flex-basis against the flex container's main size.
            bool indefinitePercentBasis = false;
            if (float.IsNegativeInfinity(basis))
            {
                // Deferred calc() — resolve against container main size
                var calcRef = style.GetRefValue(Css.Properties.Internal.PropertyId.FlexBasis);
                if (calcRef is CssFunctionValue calcFn)
                {
                    float refSize = isColumn ? containerHeight : containerWidth;
                    if (!float.IsNaN(refSize) && refSize > 0)
                    {
                        float resolved = Css.Resolution.Internal.ValueResolver.EvaluateDeferredCalc(calcFn, refSize);
                        if (style.BoxSizing == CssBoxSizing.BorderBox)
                        {
                            if (isColumn)
                            {
                                resolved -= (box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth);
                            }
                            else
                            {
                                resolved -= (box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth);
                            }
                            if (resolved < 0) { resolved = 0; }
                        }
                        return resolved;
                    }
                    indefinitePercentBasis = true;
                }
            }
            // CSS Flexbox §9.2 step 3E: if the percentage resolves against an indefinite
            // size, treat the flex basis as content — skip the width/height fallback.
            if (DeferredPercent.IsEncoded(basis))
            {
                float refSize = isColumn ? containerHeight : containerWidth;
                if (!float.IsNaN(refSize) && refSize > 0)
                {
                    float resolved = DeferredPercent.Resolve(basis, refSize);
                    if (style.BoxSizing == CssBoxSizing.BorderBox)
                    {
                        if (isColumn)
                        {
                            resolved -= (box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth);
                        }
                        else
                        {
                            resolved -= (box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth);
                        }
                        if (resolved < 0)
                        {
                            resolved = 0;
                        }
                    }
                    return resolved;
                }
                // Indefinite reference size — skip width/height, use content measurement
                indefinitePercentBasis = true;
            }

            // [CSS-FLEXBOX §7.1.1] flex-basis: content → use max-content size directly.
            // Unlike auto basis, this uses intrinsic sizing (not container-constrained).
            if (SizingKeyword.IsSizingKeyword(basis))
            {
                if (element != null && !isColumn)
                {
                    return BlockFormattingContext.MeasureIntrinsicWidth(
                        element, SizingKeyword.MaxContent, containerWidth, context);
                }
                indefinitePercentBasis = true;
            }

            // Use width/height as fallback (resolve deferred percentages and calc)
            // CSS Flexbox §9.2: when percentage flex-basis resolves against indefinite
            // size, treat as content — do NOT fall back to width/height property.
            if (!indefinitePercentBasis)
            {
                float size = isColumn ? style.Height : style.Width;
                if (!float.IsNaN(size))
                {
                    // Deferred calc() with percentage
                    if (float.IsNegativeInfinity(size))
                    {
                        int propId = isColumn
                            ? Css.Properties.Internal.PropertyId.Height
                            : Css.Properties.Internal.PropertyId.Width;
                        float cbDim = isColumn ? containerHeight : containerWidth;
                        var refVal = style.GetRefValue(propId);
                        if (refVal is CssFunctionValue calcFn)
                        {
                            float calcSize = Css.Resolution.Internal.ValueResolver.EvaluateDeferredCalc(calcFn, cbDim);
                            // box-sizing: border-box → subtract padding+border
                            if (style.BoxSizing == CssBoxSizing.BorderBox)
                            {
                                if (isColumn)
                                {
                                    calcSize -= (box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth);
                                }
                                else
                                {
                                    calcSize -= (box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth);
                                }
                                if (calcSize < 0)
                                {
                                    calcSize = 0;
                                }
                            }
                            return calcSize;
                        }
                    }
                    // Resolve deferred percentage (sentinel offset encoding)
                    if (DeferredPercent.IsEncoded(size))
                    {
                        // [CSS-FLEXBOX §9.8] Percentage main sizes in column flex with
                        // indefinite container height resolve to auto (content-based).
                        if (isAutoMainSize && isColumn)
                        {
                            size = float.NaN;
                        }
                        else
                        {
                            size = DeferredPercent.Resolve(size, isColumn ? containerHeight : containerWidth);
                        }
                    }
                    if (size >= 0)
                    {
                        // When box-sizing is border-box, the CSS width/height includes
                        // padding and border. Flex base size must be content-box.
                        if (style.BoxSizing == CssBoxSizing.BorderBox)
                        {
                            if (isColumn)
                            {
                                size -= (box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth);
                            }
                            else
                            {
                                size -= (box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth);
                            }
                            if (size < 0)
                            {
                                size = 0;
                            }
                        }
                        return size;
                    }
                }
            }

            // [CSS-SIZING-4 §5.1] When flex-basis is auto and main dimension is auto,
            // but cross dimension is definite and aspect-ratio is set, derive main size
            // from cross dimension and ratio.
            float arRatio = DimensionResolver.GetAspectRatio(style);
            if (arRatio > 0)
            {
                if (isColumn)
                {
                    // Column: main=height, cross=width. If width is definite, height = width / ratio.
                    float crossWidth = DimensionResolver.ResolveWidth(style, containerWidth, box);

                    // [CSS-FLEXBOX §9.2 step E] When cross-axis (width) is auto with
                    // aspect-ratio, measure max-content width from content, then
                    // apply min-width/max-width constraints.
                    if (float.IsNaN(crossWidth) && element != null)
                    {
                        crossWidth = BlockFormattingContext.MeasureIntrinsicWidth(
                            element, SizingKeyword.MaxContent, containerWidth, context);
                        float minW = style.MinWidth;
                        if (!float.IsNaN(minW) && minW > 0 && !DeferredPercent.IsEncoded(minW)
                            && !SizingKeyword.IsSizingKeyword(minW))
                        {
                            if (crossWidth < minW) { crossWidth = minW; }
                        }
                        float maxW = style.MaxWidth;
                        if (!float.IsNaN(maxW) && maxW >= 0 && !DeferredPercent.IsEncoded(maxW)
                            && !SizingKeyword.IsSizingKeyword(maxW))
                        {
                            if (crossWidth > maxW) { crossWidth = maxW; }
                        }
                    }

                    if (!float.IsNaN(crossWidth) && crossWidth > 0)
                    {
                        if (style.BoxSizing == CssBoxSizing.BorderBox)
                        {
                            float borderBoxWidth = crossWidth
                                + box.PaddingLeft + box.PaddingRight
                                + box.BorderLeftWidth + box.BorderRightWidth;
                            float borderBoxHeight = borderBoxWidth / arRatio;
                            float contentHeight = borderBoxHeight
                                - box.PaddingTop - box.PaddingBottom
                                - box.BorderTopWidth - box.BorderBottomWidth;
                            return Math.Max(0, contentHeight);
                        }
                        return crossWidth / arRatio;
                    }
                }
                else
                {
                    // Row: main=width, cross=height. If height is definite, width = height * ratio.
                    float crossHeight = DimensionResolver.ResolveHeight(style, containerHeight, box);

                    // [CSS-FLEXBOX §9.2 + CSS-SIZING-4 §5.1] If cross size is auto but the item
                    // will be stretched to the container's definite cross size, that counts as
                    // a definite cross size for aspect-ratio purposes.
                    if (float.IsNaN(crossHeight) && !float.IsNaN(containerHeight) && containerHeight > 0)
                    {
                        // [CSS-FLEXBOX §9.2] align-self: auto (255) inherits parent's align-items.
                        // Default align-items is stretch, so auto also means stretch.
                        // [CSS-FLEXBOX §8.1] Auto margins on cross axis prevent stretching.
                        var alignSelf = style.AlignSelf;
                        bool willStretch = alignSelf == CssAlignItems.Stretch
                            || (int)alignSelf == 0
                            || (int)alignSelf == 255;
                        bool hasAutoCrossMargin = float.IsNaN(style.MarginTop) || float.IsNaN(style.MarginBottom);
                        if (willStretch && !hasAutoCrossMargin)
                        {
                            crossHeight = containerHeight
                                - box.PaddingTop - box.PaddingBottom
                                - box.BorderTopWidth - box.BorderBottomWidth
                                - box.MarginTop - box.MarginBottom;
                        }
                    }

                    if (!float.IsNaN(crossHeight) && crossHeight > 0)
                    {
                        if (style.BoxSizing == CssBoxSizing.BorderBox)
                        {
                            float borderBoxHeight = crossHeight
                                + box.PaddingTop + box.PaddingBottom
                                + box.BorderTopWidth + box.BorderBottomWidth;
                            float borderBoxWidth = borderBoxHeight * arRatio;
                            float contentWidth = borderBoxWidth
                                - box.PaddingLeft - box.PaddingRight
                                - box.BorderLeftWidth - box.BorderRightWidth;
                            return Math.Max(0, contentWidth);
                        }
                        return crossHeight * arRatio;
                    }
                }
            }

            // Replaced elements (img, input, select, textarea, etc.) have intrinsic dimensions.
            // Use those instead of trial layout.
            if (element != null && ReplacedElementLayout.IsReplaced(element))
            {
                float intrW = ReplacedElementLayout.GetFormControlIntrinsicWidth(element);
                float intrH = ReplacedElementLayout.GetFormControlIntrinsicHeight(element);
                if (element.TagName == "img" || element.TagName == "svg")
                {
                    // For images, check data URI or attributes
                    if (ReplacedElementLayout.TryGetDataUriDimensions(element, out float dw, out float dh))
                    {
                        intrW = dw;
                        intrH = dh;
                    }
                    else
                    {
                        string widthAttr = element.GetAttribute("width") ?? "";
                        string heightAttr = element.GetAttribute("height") ?? "";
                        if (float.TryParse(widthAttr, out float aw)) { intrW = aw; }
                        if (float.TryParse(heightAttr, out float ah)) { intrH = ah; }
                    }
                    // [CSS-IMAGES-3 §2.2] SVG with viewBox but no width/height:
                    // has intrinsic RATIO but no intrinsic dimensions.
                    // Don't set intrinsic W/H from viewBox — let ResolveDimensions
                    // handle it through the ratio.
                }
                return isColumn ? intrH : intrW;
            }

            // Auto: measure content size via trial layout
            var measureBox = new LayoutBox(element, BoxType.Block);
            BoxModelCalculator.ApplyBoxModel(measureBox, style, containerWidth);

            if (isColumn)
            {
                // Column: main axis is height, measure content height.
                // Width (cross axis) defaults to available width when auto.
                float w = DimensionResolver.ResolveWidth(style, containerWidth, measureBox);
                if (float.IsNaN(w))
                {
                    w = containerWidth - BoxModelCalculator.GetHorizontalSpacing(measureBox);
                }
                // [CSS-SIZING §5.2] Apply max-width before measuring column flex cross axis
                float colMaxW = style.MaxWidth;
                if (!float.IsNaN(colMaxW) && colMaxW >= 0 && !DeferredPercent.IsEncoded(colMaxW) && !SizingKeyword.IsSizingKeyword(colMaxW))
                {
                    if (style.BoxSizing == CssBoxSizing.BorderBox)
                    {
                        colMaxW -= measureBox.PaddingLeft + measureBox.PaddingRight + measureBox.BorderLeftWidth + measureBox.BorderRightWidth;
                        if (colMaxW < 0) { colMaxW = 0; }
                    }
                    if (w > colMaxW) { w = colMaxW; }
                }
                measureBox.ContentRect = new RectF(0, 0, w, 0);
                BlockFormattingContext.LayoutChildren(measureBox, context);
                return CalculateAutoHeight(measureBox);
            }
            else
            {
                // Row: main axis is width, use shrink-to-fit heuristic
                // Lay out with full available width, then measure actual content extent.
                // Use GetContentExtent which recursively measures auto-width block children
                // and uses NaturalContentWidth for lines (pre-alignment, ignoring text-align).
                float availWidth = containerWidth - BoxModelCalculator.GetHorizontalSpacing(measureBox);
                measureBox.ContentRect = new RectF(0, 0, availWidth, 0);
                BlockFormattingContext.LayoutChildren(measureBox, context);
                float contentWidth = BlockFormattingContext.GetContentExtent(measureBox);
                return Math.Min(contentWidth, availWidth);
            }
        }

        // MeasureContentWidth removed — now using BlockFormattingContext.GetContentExtent
        // which handles text-align correctly (via NaturalContentWidth) and recursively
        // measures auto-width block children.

        /// <summary>
        /// CSS Flexbox §4.5: Compute content-based minimum height for a column flex item.
        /// Used when min-height is auto to prevent items from shrinking below content.
        /// </summary>
        private static float ComputeContentMinHeight(FlexItem item, float containerWidth, LayoutContext context)
        {
            if (item.Box.StyledNode is not StyledElement element)
            {
                return 0;
            }
            var measureBox = new LayoutBox(element, BoxType.Block);
            BoxModelCalculator.ApplyBoxModel(measureBox, item.Style, containerWidth);
            float w = DimensionResolver.ResolveWidth(item.Style, containerWidth, measureBox);
            if (float.IsNaN(w))
            {
                w = containerWidth - BoxModelCalculator.GetHorizontalSpacing(measureBox);
            }
            measureBox.ContentRect = new RectF(0, 0, w, 0);
            var savedFloatCtx = context.FloatContext;
            var savedCbw = context.ContainingBlockWidth;
            var savedCbh = context.ContainingBlockHeight;
            context.FloatContext = null;
            BlockFormattingContext.LayoutChildren(measureBox, context);
            context.FloatContext = savedFloatCtx;
            context.ContainingBlockWidth = savedCbw;
            context.ContainingBlockHeight = savedCbh;
            return CalculateAutoHeight(measureBox);
        }

        /// <summary>
        /// CSS Flexbox §4.5: Compute content-based minimum width for a row flex item.
        /// Used when min-width is auto to prevent items from shrinking below content.
        /// </summary>
        private static float ComputeContentMinWidth(FlexItem item, float containerWidth, LayoutContext context)
        {
            if (item.Box.StyledNode is not StyledElement element)
            {
                return 0;
            }
            // [CSS-FLEXBOX §4.5] The content size suggestion is the MIN-CONTENT
            // width of the item. Use MeasureIntrinsicWidth which correctly handles
            // flex containers, grid containers, and regular blocks.
            var savedFloatCtx = context.FloatContext;
            var savedCbw = context.ContainingBlockWidth;
            var savedCbh = context.ContainingBlockHeight;
            context.FloatContext = null;
            float contentWidth = BlockFormattingContext.MeasureIntrinsicWidth(
                element, SizingKeyword.MinContent, containerWidth, context);
            context.FloatContext = savedFloatCtx;
            context.ContainingBlockWidth = savedCbw;
            context.ContainingBlockHeight = savedCbh;
            return contentWidth;
        }

        /// <summary>
        /// [CSS-FLEXBOX §8.3] Apply cross-axis alignment for all items in a flex line.
        /// Handles align-items, align-self, auto cross margins, and stretch re-layout.
        /// </summary>
        private static void AlignCrossAxis(FlexLine line, ComputedStyle containerStyle, float maxCross,
            bool isColumn, float containerWidth, float containerHeight, LayoutContext context)
        {
            for (int i = 0; i < line.Items.Count; i++)
            {
                var item = line.Items[i];
                var box = item.Box;

                var align = item.Style.AlignSelf;
                if ((int)align == 255)
                {
                    align = containerStyle.AlignItems;
                }

                float itemCross;
                if (isColumn)
                {
                    itemCross = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                              + box.BorderLeftWidth + box.BorderRightWidth
                              + box.MarginLeft + box.MarginRight;
                }
                else
                {
                    itemCross = box.ContentRect.Height + box.PaddingTop + box.PaddingBottom
                              + box.BorderTopWidth + box.BorderBottomWidth
                              + box.MarginTop + box.MarginBottom;
                }

                float freeCross = maxCross - itemCross;
                if (freeCross <= 0)
                {
                    continue;
                }

                // Auto margins on cross axis absorb free space
                if (TryApplyCrossAutoMargins(item, box, freeCross, isColumn))
                {
                    continue;
                }

                float crossOffset = 0;
                switch (align)
                {
                    case CssAlignItems.FlexStart:
                    case CssAlignItems.Start:
                        crossOffset = 0;
                        break;
                    case CssAlignItems.Baseline:
                        crossOffset = GetBaselineOffset(box, line, isColumn);
                        break;
                    case CssAlignItems.FlexEnd:
                    case CssAlignItems.End:
                        crossOffset = freeCross;
                        break;
                    case CssAlignItems.Center:
                        crossOffset = freeCross / 2;
                        break;
                    case CssAlignItems.Stretch:
                    default:
                        ApplyStretch(item, box, freeCross, isColumn, containerWidth, containerHeight, context);
                        crossOffset = 0;
                        break;
                }

                if (crossOffset > 0)
                {
                    if (isColumn)
                    {
                        OffsetBoxInPlace(box, crossOffset, 0);
                    }
                    else
                    {
                        OffsetBoxInPlace(box, 0, crossOffset);
                    }
                }
            }
        }

        private static bool TryApplyCrossAutoMargins(FlexItem item, LayoutBox box, float freeCross, bool isColumn)
        {
            if (isColumn)
            {
                bool autoLeft = float.IsNaN(item.Style.MarginLeft);
                bool autoRight = float.IsNaN(item.Style.MarginRight);
                if (autoLeft || autoRight)
                {
                    float perMargin = (autoLeft && autoRight) ? freeCross / 2 : freeCross;
                    float offset = autoLeft ? perMargin : 0;
                    box.ContentRect = new Core.Values.RectF(
                        box.ContentRect.X + offset, box.ContentRect.Y,
                        box.ContentRect.Width, box.ContentRect.Height);
                    return true;
                }
            }
            else
            {
                bool autoTop = float.IsNaN(item.Style.MarginTop);
                bool autoBottom = float.IsNaN(item.Style.MarginBottom);
                if (autoTop || autoBottom)
                {
                    float perMargin = (autoTop && autoBottom) ? freeCross / 2 : freeCross;
                    float offset = autoTop ? perMargin : 0;
                    box.ContentRect = new Core.Values.RectF(
                        box.ContentRect.X, box.ContentRect.Y + offset,
                        box.ContentRect.Width, box.ContentRect.Height);
                    return true;
                }
            }
            return false;
        }

        private static void ApplyStretch(FlexItem item, LayoutBox box, float freeCross,
            bool isColumn, float containerWidth, float containerHeight, LayoutContext context)
        {
            if (isColumn)
            {
                if (float.IsNaN(item.Style.Width))
                {
                    float newWidth = box.ContentRect.Width + freeCross;
                    float minW = DimensionResolver.ResolvePercentWidth(item.Style.MinWidth, containerWidth);
                    float maxW = DimensionResolver.ResolvePercentWidth(item.Style.MaxWidth, containerWidth);
                    if (!float.IsNaN(minW) && minW >= 0) { newWidth = Math.Max(newWidth, minW); }
                    if (!float.IsNaN(maxW) && maxW >= 0) { newWidth = Math.Min(newWidth, maxW); }
                    box.ContentRect = new RectF(box.ContentRect.X, box.ContentRect.Y,
                                                newWidth, box.ContentRect.Height);
                }
            }
            else
            {
                if (float.IsNaN(item.Style.Height))
                {
                    float newHeight = box.ContentRect.Height + freeCross;
                    float minH = DimensionResolver.ResolvePercentHeight(item.Style.MinHeight, containerHeight);
                    float maxH = DimensionResolver.ResolvePercentHeight(item.Style.MaxHeight, containerHeight);
                    if (!float.IsNaN(minH) && minH >= 0) { newHeight = Math.Max(newHeight, minH); }
                    if (!float.IsNaN(maxH) && maxH >= 0) { newHeight = Math.Min(newHeight, maxH); }
                    float oldHeight = box.ContentRect.Height;
                    box.ContentRect = new RectF(box.ContentRect.X, box.ContentRect.Y,
                                                box.ContentRect.Width, newHeight);

                    // [CSS-FLEXBOX §9.8] After stretch, the item's cross size is definite.
                    box.HasDefiniteCrossSize = true;

                    // Re-layout children so percentage heights resolve against the
                    // stretched size, not the initial content-based size.
                    if (newHeight > oldHeight + 0.01f && box.Children.Count > 0)
                    {
                        box.ClearChildren();
                        box.LineBoxes?.Clear();
                        var savedFc = context.FloatContext;
                        context.FloatContext = null;
                        BlockFormattingContext.LayoutChildren(box, context);
                        context.FloatContext = savedFc;
                    }
                }
            }
        }

        /// <summary>
        /// [CSS-FLEXBOX §9.4] Apply align-content to distribute cross-axis space among flex lines.
        /// </summary>
        private static void ApplyAlignContent(List<FlexLine> lines, ComputedStyle style,
            bool isColumn, float containerWidth, float containerHeight, float crossGap)
        {
            float totalLineCross = 0;
            for (int li = 0; li < lines.Count; li++)
            {
                totalLineCross += lines[li].CrossSize;
            }
            totalLineCross += crossGap * (lines.Count - 1);

            float crossSpace = isColumn ? containerWidth : containerHeight;
            if (float.IsNaN(crossSpace) || crossSpace <= 0)
            {
                return;
            }

            float freeCrossSpace = crossSpace - totalLineCross;
            // [CSS-FLEXBOX §9.4] align-content applies even when content overflows
            // (freeCrossSpace < 0). Only skip for distribution modes with no free space.
            var alignContent = style.AlignContent;
            float lineOffset = 0;
            float lineGap = 0;

            switch (alignContent)
            {
                case CssAlignItems.Center:
                    lineOffset = freeCrossSpace / 2;
                    break;
                case CssAlignItems.FlexEnd:
                case CssAlignItems.End:
                    lineOffset = freeCrossSpace;
                    break;
                // Distribution modes fall back to flex-start when no positive free space
                // [CSS-ALIGN §5.3] If free space is negative, behave as start alignment
                case CssAlignItems.SpaceBetween:
                    if (freeCrossSpace > 0 && lines.Count > 1)
                    {
                        lineGap = freeCrossSpace / (lines.Count - 1);
                    }
                    break;
                case CssAlignItems.SpaceAround:
                    if (freeCrossSpace > 0 && lines.Count > 0)
                    {
                        float halfGap = freeCrossSpace / (lines.Count * 2);
                        lineOffset = halfGap;
                        lineGap = halfGap * 2;
                    }
                    else if (freeCrossSpace < 0)
                    {
                        // [CSS-ALIGN §5.3] Fallback to center when negative
                        lineOffset = freeCrossSpace / 2;
                    }
                    break;
                case CssAlignItems.SpaceEvenly:
                    if (freeCrossSpace > 0 && lines.Count > 0)
                    {
                        float evenGap = freeCrossSpace / (lines.Count + 1);
                        lineOffset = evenGap;
                        lineGap = evenGap;
                    }
                    else if (freeCrossSpace < 0)
                    {
                        lineOffset = freeCrossSpace / 2;
                    }
                    break;
                case CssAlignItems.Stretch:
                default:
                    if (freeCrossSpace <= 0)
                    {
                        return;
                    }
                    ApplyAlignContentStretch(lines, style, freeCrossSpace, isColumn);
                    return;
                case CssAlignItems.FlexStart:
                case CssAlignItems.Start:
                case CssAlignItems.Baseline:
                    break;
            }

            if (lineOffset != 0 || lineGap != 0)
            {
                float cumOffset = lineOffset;
                for (int li = 0; li < lines.Count; li++)
                {
                    if (cumOffset != 0)
                    {
                        for (int i = 0; i < lines[li].Items.Count; i++)
                        {
                            if (isColumn)
                            {
                                OffsetBoxInPlace(lines[li].Items[i].Box, cumOffset, 0);
                            }
                            else
                            {
                                OffsetBoxInPlace(lines[li].Items[i].Box, 0, cumOffset);
                            }
                        }
                    }
                    cumOffset += lineGap;
                }
            }
        }

        private static void ApplyAlignContentStretch(List<FlexLine> lines, ComputedStyle style,
            float freeCrossSpace, bool isColumn)
        {
            float stretchPerLine = freeCrossSpace / lines.Count;
            float stretchCumOffset = 0;
            for (int li = 0; li < lines.Count; li++)
            {
                var stretchLine = lines[li];
                float newLineCross = stretchLine.CrossSize + stretchPerLine;

                if (stretchCumOffset > 0)
                {
                    for (int i = 0; i < stretchLine.Items.Count; i++)
                    {
                        if (isColumn)
                        {
                            OffsetBoxInPlace(stretchLine.Items[i].Box, stretchCumOffset, 0);
                        }
                        else
                        {
                            OffsetBoxInPlace(stretchLine.Items[i].Box, 0, stretchCumOffset);
                        }
                    }
                }

                for (int i = 0; i < stretchLine.Items.Count; i++)
                {
                    var stretchItem = stretchLine.Items[i];
                    var stretchBox = stretchItem.Box;
                    var stretchAlign = stretchItem.Style.AlignSelf;
                    if ((int)stretchAlign == 255)
                    {
                        stretchAlign = style.AlignItems;
                    }
                    if (stretchAlign != CssAlignItems.Stretch)
                    {
                        continue;
                    }

                    if (isColumn)
                    {
                        if (float.IsNaN(stretchItem.Style.Width))
                        {
                            float itemMarginCross = stretchBox.MarginLeft + stretchBox.MarginRight
                                + stretchBox.PaddingLeft + stretchBox.PaddingRight
                                + stretchBox.BorderLeftWidth + stretchBox.BorderRightWidth;
                            float newWidth = newLineCross - itemMarginCross;
                            if (newWidth > stretchBox.ContentRect.Width)
                            {
                                stretchBox.ContentRect = new RectF(
                                    stretchBox.ContentRect.X, stretchBox.ContentRect.Y,
                                    newWidth, stretchBox.ContentRect.Height);
                            }
                        }
                    }
                    else
                    {
                        if (float.IsNaN(stretchItem.Style.Height))
                        {
                            float itemMarginCross = stretchBox.MarginTop + stretchBox.MarginBottom
                                + stretchBox.PaddingTop + stretchBox.PaddingBottom
                                + stretchBox.BorderTopWidth + stretchBox.BorderBottomWidth;
                            float newHeight = newLineCross - itemMarginCross;
                            if (newHeight > stretchBox.ContentRect.Height)
                            {
                                stretchBox.ContentRect = new RectF(
                                    stretchBox.ContentRect.X, stretchBox.ContentRect.Y,
                                    stretchBox.ContentRect.Width, newHeight);
                            }
                        }
                    }
                }

                stretchLine.CrossSize = newLineCross;
                stretchCumOffset += stretchPerLine;
            }
        }

        /// <summary>
        /// [CSS-FLEXBOX §9.5] Distribute auto margins on the main axis.
        /// Auto margins absorb free space, overriding justify-content.
        /// Returns true if any auto margins were distributed.
        /// </summary>
        private static bool DistributeAutoMargins(FlexLine line, float mainSize, float totalGaps, bool isColumn)
        {
            float resolvedFreeSpace = mainSize - totalGaps;
            for (int i = 0; i < line.Items.Count; i++)
            {
                resolvedFreeSpace -= line.Items[i].ResolvedMainSize + GetItemMainMargins(line.Items[i], isColumn);
            }

            int autoMarginCount = 0;
            for (int i = 0; i < line.Items.Count; i++)
            {
                if (line.Items[i].Style == null)
                {
                    continue;
                }
                if (isColumn)
                {
                    if (float.IsNaN(line.Items[i].Style.MarginTop))
                    {
                        autoMarginCount++;
                    }
                    if (float.IsNaN(line.Items[i].Style.MarginBottom))
                    {
                        autoMarginCount++;
                    }
                }
                else
                {
                    if (float.IsNaN(line.Items[i].Style.MarginLeft))
                    {
                        autoMarginCount++;
                    }
                    if (float.IsNaN(line.Items[i].Style.MarginRight))
                    {
                        autoMarginCount++;
                    }
                }
            }

            if (autoMarginCount <= 0 || resolvedFreeSpace <= 0)
            {
                return false;
            }

            float perAutoMargin = resolvedFreeSpace / autoMarginCount;
            for (int i = 0; i < line.Items.Count; i++)
            {
                var item = line.Items[i];
                if (item.Style == null)
                {
                    continue;
                }
                if (isColumn)
                {
                    if (float.IsNaN(item.Style.MarginTop))
                    {
                        item.Box.MarginTop = perAutoMargin;
                    }
                    if (float.IsNaN(item.Style.MarginBottom))
                    {
                        item.Box.MarginBottom = perAutoMargin;
                    }
                }
                else
                {
                    if (float.IsNaN(item.Style.MarginLeft))
                    {
                        item.Box.MarginLeft = perAutoMargin;
                    }
                    if (float.IsNaN(item.Style.MarginRight))
                    {
                        item.Box.MarginRight = perAutoMargin;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// [CSS-FLEXBOX §9.7] Resolve flexible lengths for a single flex line.
        /// Implements the freeze-redistribute loop that distributes free space
        /// among flex items based on flex-grow/flex-shrink factors.
        /// </summary>
        private static void ResolveFlexibleLengths(FlexLine line, float mainSize, float totalGaps,
            bool isAutoMainSize, bool isColumn)
        {
            float totalBase = 0;
            for (int i = 0; i < line.Items.Count; i++)
            {
                totalBase += line.Items[i].BaseSize + GetItemMainMargins(line.Items[i], isColumn);
            }

            float initialFreeSpace = isAutoMainSize ? 0 : mainSize - totalBase - totalGaps;
            bool isGrowing = initialFreeSpace > 0;
            var frozen = new bool[line.Items.Count];

            // Phase 1: Freeze inflexible items and clamp to min/max
            for (int i = 0; i < line.Items.Count; i++)
            {
                var item = line.Items[i];
                item.ResolvedMainSize = item.BaseSize;

                if ((isGrowing && item.FlexGrow == 0) || (!isGrowing && item.FlexShrink == 0))
                {
                    frozen[i] = true;
                    if (item.Style != null)
                    {
                        float minMain = GetFlexItemMinMain(item, isColumn);
                        float maxMain = isColumn ? item.Style.MaxHeight : item.Style.MaxWidth;
                        if (DeferredPercent.IsEncoded(maxMain))
                        {
                            maxMain = DeferredPercent.Resolve(maxMain, isColumn ? item.ContainerHeight : item.ContainerWidth);
                        }
                        if (!float.IsNaN(maxMain) && maxMain >= 0 && item.Style.BoxSizing == CssBoxSizing.BorderBox)
                        {
                            float mainExtra = isColumn
                                ? item.Box.PaddingTop + item.Box.PaddingBottom + item.Box.BorderTopWidth + item.Box.BorderBottomWidth
                                : item.Box.PaddingLeft + item.Box.PaddingRight + item.Box.BorderLeftWidth + item.Box.BorderRightWidth;
                            maxMain = Math.Max(0, maxMain - mainExtra);
                        }
                        if (minMain > 0 && item.ResolvedMainSize < minMain)
                        {
                            item.ResolvedMainSize = minMain;
                        }
                        if (!float.IsNaN(maxMain) && maxMain >= 0 && item.ResolvedMainSize > maxMain)
                        {
                            item.ResolvedMainSize = maxMain;
                        }
                    }
                }

                if (item.Style != null && item.Style.Visibility == CssVisibility.Collapse)
                {
                    item.ResolvedMainSize = 0;
                    frozen[i] = true;
                }
            }

            // Phase 2: Iteratively distribute free space among unfrozen items
            for (int iteration = 0; iteration < line.Items.Count + 1; iteration++)
            {
                float frozenSpace = totalGaps;
                float unfrozenBaseTotal = 0;
                float activeTotalGrow = 0;
                float totalScaledShrink = 0;
                for (int i = 0; i < line.Items.Count; i++)
                {
                    frozenSpace += GetItemMainMargins(line.Items[i], isColumn);
                    if (frozen[i])
                    {
                        frozenSpace += line.Items[i].ResolvedMainSize;
                    }
                    else
                    {
                        unfrozenBaseTotal += line.Items[i].BaseSize;
                        activeTotalGrow += line.Items[i].FlexGrow;
                        totalScaledShrink += line.Items[i].FlexShrink * line.Items[i].BaseSize;
                    }
                }

                float remainingSpace = isAutoMainSize ? 0 : mainSize - frozenSpace - unfrozenBaseTotal;

                // [CSS-FLEXBOX §9.7 step 4c] Fractional flex factors
                if (remainingSpace > 0 && activeTotalGrow > 0 && activeTotalGrow < 1)
                {
                    float scaledFreeSpace = initialFreeSpace * activeTotalGrow;
                    if (Math.Abs(scaledFreeSpace) < Math.Abs(remainingSpace))
                    {
                        remainingSpace = scaledFreeSpace;
                    }
                }
                else if (remainingSpace < 0 && totalScaledShrink > 0)
                {
                    float totalUnscaledShrink = 0;
                    for (int i = 0; i < line.Items.Count; i++)
                    {
                        if (!frozen[i])
                        {
                            totalUnscaledShrink += line.Items[i].FlexShrink;
                        }
                    }
                    if (totalUnscaledShrink < 1)
                    {
                        float scaledFreeSpace = initialFreeSpace * totalUnscaledShrink;
                        if (Math.Abs(scaledFreeSpace) < Math.Abs(remainingSpace))
                        {
                            remainingSpace = scaledFreeSpace;
                        }
                    }
                }

                bool anyNewlyFrozen = false;

                // Cumulative fractions for sequential consumption (matching Chrome's line_flexer.cc)
                float runningGrow = 0;
                float runningScaledShrink = 0;
                float[] fractions = new float[line.Items.Count];
                for (int i = 0; i < line.Items.Count; i++)
                {
                    if (frozen[i])
                    {
                        continue;
                    }
                    if (remainingSpace > 0 && activeTotalGrow > 0)
                    {
                        runningGrow += line.Items[i].FlexGrow;
                        fractions[i] = line.Items[i].FlexGrow / runningGrow;
                    }
                    else if (remainingSpace < 0 && totalScaledShrink > 0)
                    {
                        float ws = line.Items[i].FlexShrink * line.Items[i].BaseSize;
                        runningScaledShrink += ws;
                        fractions[i] = ws / runningScaledShrink;
                    }
                }

                // Distribute in reverse (last item absorbs rounding remainder)
                float freeSpace = remainingSpace;
                for (int i = line.Items.Count - 1; i >= 0; i--)
                {
                    if (frozen[i])
                    {
                        continue;
                    }
                    var item = line.Items[i];

                    float extraSize;
                    if (fractions[i] >= 1.0f)
                    {
                        extraSize = freeSpace;
                    }
                    else
                    {
                        double extra = (double)freeSpace * fractions[i];
                        extraSize = (float)(Math.Round(extra * 64.0, MidpointRounding.AwayFromZero) / 64.0);
                    }
                    freeSpace -= extraSize;

                    float resolved = item.BaseSize + extraSize;
                    resolved = Math.Max(0, resolved);

                    if (item.Style != null)
                    {
                        float minMain = GetFlexItemMinMain(item, isColumn);
                        float maxMain = isColumn ? item.Style.MaxHeight : item.Style.MaxWidth;
                        if (DeferredPercent.IsEncoded(maxMain))
                        {
                            maxMain = DeferredPercent.Resolve(maxMain, isColumn ? item.ContainerHeight : item.ContainerWidth);
                        }
                        if (!float.IsNaN(maxMain) && maxMain >= 0 && item.Style.BoxSizing == CssBoxSizing.BorderBox)
                        {
                            float mainExtra = isColumn
                                ? item.Box.PaddingTop + item.Box.PaddingBottom + item.Box.BorderTopWidth + item.Box.BorderBottomWidth
                                : item.Box.PaddingLeft + item.Box.PaddingRight + item.Box.BorderLeftWidth + item.Box.BorderRightWidth;
                            maxMain = Math.Max(0, maxMain - mainExtra);
                        }
                        if (minMain > 0 && resolved < minMain)
                        {
                            resolved = minMain;
                            frozen[i] = true;
                            anyNewlyFrozen = true;
                        }
                        if (!float.IsNaN(maxMain) && maxMain >= 0 && resolved > maxMain)
                        {
                            resolved = maxMain;
                            frozen[i] = true;
                            anyNewlyFrozen = true;
                        }
                    }

                    item.ResolvedMainSize = resolved;
                }

                if (!anyNewlyFrozen)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Returns the effective min-main-size for a flex item.
        /// Uses the explicit min-width/min-height if set, then falls back to
        /// the CSS §4.5 automatic minimum (content-based) if computed.
        /// </summary>
        private static float GetFlexItemMinMain(FlexItem item, bool isColumn)
        {
            float explicitMin = isColumn ? item.Style.MinHeight : item.Style.MinWidth;
            if (DeferredPercent.IsEncoded(explicitMin))
            {
                explicitMin = DeferredPercent.Resolve(explicitMin,
                    isColumn ? item.ContainerHeight : item.ContainerWidth);
            }
            // [CSS-SIZING §5.2] Resolve sizing keywords (min-content, max-content, fit-content)
            // for min-width/min-height to actual pixel values.
            if (SizingKeyword.IsSizingKeyword(explicitMin))
            {
                if (item.SizingKeywordMinMain > 0)
                {
                    return item.SizingKeywordMinMain;
                }
                return 0;
            }
            if (!float.IsNaN(explicitMin) && explicitMin > 0)
            {
                return explicitMin;
            }
            if (item.AutoMinMain > 0)
            {
                return item.AutoMinMain;
            }
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

        private static (float startOffset, float gap) ApplyJustifyContent(
            CssJustifyContent justify, float freeSpace, int itemCount, float defaultGap)
        {
            if (freeSpace <= 0)
                return (0, defaultGap);

            switch (justify)
            {
                case CssJustifyContent.Center:
                    return (freeSpace / 2, defaultGap);
                case CssJustifyContent.FlexEnd:
                    return (freeSpace, defaultGap);
                case CssJustifyContent.SpaceBetween:
                    if (itemCount <= 1) return (0, defaultGap);
                    // freeSpace already has defaultGap subtracted, so add it back per gap slot
                    return (0, defaultGap + freeSpace / (itemCount - 1));
                case CssJustifyContent.SpaceAround:
                {
                    float perItem = freeSpace / itemCount;
                    return (perItem / 2, defaultGap + perItem);
                }
                case CssJustifyContent.SpaceEvenly:
                {
                    float slot = freeSpace / (itemCount + 1);
                    return (slot, defaultGap + slot);
                }
                default:
                    return (0, defaultGap);
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

        /// <summary>
        /// Get the baseline offset for a flex item for baseline alignment.
        /// Returns how much to offset the item so its baseline aligns with the line's max baseline.
        /// </summary>
        private static float GetBaselineOffset(LayoutBox box, FlexLine line, bool isColumn)
        {
            if (isColumn) return 0; // Baseline alignment only applies to row flex

            float itemBaseline = GetItemBaseline(box);
            float maxBaseline = 0;
            for (int i = 0; i < line.Items.Count; i++)
            {
                float b = GetItemBaseline(line.Items[i].Box);
                if (b > maxBaseline) maxBaseline = b;
            }
            return maxBaseline - itemBaseline;
        }

        /// <summary>
        /// Get the first baseline of a flex item from its first line box,
        /// or approximate from font size.
        /// </summary>
        private static float GetItemBaseline(LayoutBox box)
        {
            // Check for line boxes (inline content)
            if (box.LineBoxes != null && box.LineBoxes.Count > 0)
                return box.LineBoxes[0].Baseline + (box.LineBoxes[0].Y - box.ContentRect.Y)
                     + box.PaddingTop + box.BorderTopWidth;

            // Check first child with line boxes
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                if (child.LineBoxes != null && child.LineBoxes.Count > 0)
                    return child.LineBoxes[0].Baseline + (child.LineBoxes[0].Y - box.ContentRect.Y)
                         + box.PaddingTop + box.BorderTopWidth;
            }

            // Fallback: use bottom edge of content as baseline
            return box.ContentRect.Height + box.PaddingTop + box.BorderTopWidth;
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
                    var lb = box.LineBoxes[i];
                    lb.X += dx;
                    lb.Y += dy;
                }
            }
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
            public float AutoMinMain { get; set; }
            public float SizingKeywordMinMain { get; set; }
            public float ContainerWidth { get; set; }
            public float ContainerHeight { get; set; }
        }

        private sealed class FlexLine
        {
            public List<FlexItem> Items { get; } = new List<FlexItem>();
            public float CrossSize { get; set; }
        }

        /// <summary>
        /// Clone a computed style but override display to block.
        /// Prevents infinite recursion when anonymous flex text items get relaid.
        /// </summary>
        private static ComputedStyle CloneStyleAsBlock(ComputedStyle source)
        {
            var values = (PropertyValue[])source.GetValues().Clone();
            values[PropertyId.Display] = PropertyValue.FromInt((int)CssDisplay.Block);
            // Anonymous flex items must not inherit the parent's explicit sizing properties.
            // CSS anonymous boxes have auto width/height — inheriting the flex container's
            // dimensions causes the anonymous item to fill the container, defeating centering.
            var autoVal = PropertyValue.FromLength(float.NaN);
            values[PropertyId.Width] = autoVal;
            values[PropertyId.Height] = autoVal;
            values[PropertyId.MinWidth] = autoVal;
            values[PropertyId.MinHeight] = autoVal;
            values[PropertyId.MaxWidth] = autoVal;
            values[PropertyId.MaxHeight] = autoVal;

            // Anonymous boxes must not inherit the parent's visual decoration properties.
            // CSS 2.1 §9.2.1.1: anonymous boxes inherit inheritable properties from their
            // enclosing non-anonymous box, but non-inheritable decorations (box-shadow,
            // border, background, etc.) must not be duplicated onto the wrapper.
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
            // Anonymous boxes are always in normal flow — don't inherit positioned status.
            values[PropertyId.Position] = PropertyValue.FromInt((int)CssPosition.Static);
            // Anonymous flex text items must not inherit text-align from the parent.
            // Flex layout's justify-content handles horizontal centering; text-align
            // would double-center text against the initial (oversized) line width.
            values[PropertyId.TextAlign] = PropertyValue.FromInt((int)CssTextAlign.Left);
            return new ComputedStyle(values, refValues);
        }
    }
}
