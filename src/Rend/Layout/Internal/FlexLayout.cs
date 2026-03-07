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
        internal static bool _debugFlex;

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
                    containerHeight = explicitH;
            }

            bool isColumn = style.FlexDirection == CssFlexDirection.Column ||
                            style.FlexDirection == CssFlexDirection.ColumnReverse;
            bool isReverse = style.FlexDirection == CssFlexDirection.RowReverse ||
                             style.FlexDirection == CssFlexDirection.ColumnReverse;
            bool isWrap = style.FlexWrap != CssFlexWrap.Nowrap;

            float mainSize = isColumn ? containerHeight : containerWidth;
            bool isAutoMainSize = false;
            if (float.IsNaN(mainSize) || mainSize <= 0)
            {
                mainSize = isColumn ? 10000f : containerWidth;
                if (isColumn) isAutoMainSize = true;
            }

            float gap = isColumn ? style.RowGap : style.ColumnGap;
            if (float.IsNaN(gap)) gap = 0;
            float crossGap = isColumn ? style.ColumnGap : style.RowGap;
            if (float.IsNaN(crossGap)) crossGap = 0;

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
                    InlineFormattingContext.Layout(textBox, context);
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
                    // Pseudo-elements become flex items containing their generated text
                    var pseudoText = new StyledText(pseudo.Content, pseudo.Style);
                    var pseudoBox = new LayoutText(pseudoText);
                    float fontSize = pseudo.Style.FontSize;
                    float lineHeight = pseudo.Style.LineHeight;
                    if (lineHeight < 0)
                        lineHeight = -lineHeight * fontSize;
                    else if (float.IsNaN(lineHeight) || lineHeight == 0)
                        lineHeight = fontSize * 1.2f;
                    float estimatedWidth = pseudo.Content.Length * fontSize * 0.6f;
                    pseudoBox.ContentRect = new RectF(0, 0, estimatedWidth, lineHeight);
                    items.Add(new FlexItem
                    {
                        Box = pseudoBox,
                        Style = pseudo.Style,
                        FlexGrow = 0,
                        FlexShrink = 1,
                        BaseSize = isColumn ? lineHeight : estimatedWidth,
                        Order = 0
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
                    posBox.ContentRect = new RectF(parent.ContentRect.X, parent.ContentRect.Y, posWidth, 0);
                    BlockFormattingContext.LayoutChildren(posBox, context);
                    float posHeight = DimensionResolver.ResolveHeight(childElement.Style, float.NaN, posBox);
                    if (float.IsNaN(posHeight)) posHeight = 0;
                    posBox.ContentRect = new RectF(posBox.ContentRect.X, posBox.ContentRect.Y, posWidth, posHeight);
                    parent.AddChild(posBox);
                    continue;
                }

                var box = new LayoutBox(childElement, BoxType.Block);
                BoxModelCalculator.ApplyBoxModel(box, childElement.Style, containerWidth);

                float baseSize = ResolveFlexBasis(childElement.Style, isColumn, containerWidth, containerHeight, box, childElement, context);
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
                float totalBase = 0;
                float totalGaps = (line.Items.Count - 1) * gap;
                for (int i = 0; i < line.Items.Count; i++)
                    totalBase += line.Items[i].BaseSize + GetItemMainMargins(line.Items[i], isColumn);

                // CSS Flexbox spec §9.7: Resolve flexible lengths with freeze-redistribute loop.
                float initialFreeSpace = mainSize - totalBase - totalGaps;
                bool isGrowing = initialFreeSpace > 0;
                var frozen = new bool[line.Items.Count];

                // Phase 1: Freeze inflexible items (grow=0 when growing, shrink=0 when shrinking)
                // and clamp them to min/max. Do NOT freeze flexible items even if base violates min/max.
                for (int i = 0; i < line.Items.Count; i++)
                {
                    var item = line.Items[i];
                    item.ResolvedMainSize = item.BaseSize;

                    if ((isGrowing && item.FlexGrow == 0) || (!isGrowing && item.FlexShrink == 0))
                    {
                        frozen[i] = true;
                        // Clamp inflexible items to min/max
                        if (item.Style != null)
                        {
                            float minMain = GetFlexItemMinMain(item, isColumn);
                            float maxMain = isColumn ? item.Style.MaxHeight : item.Style.MaxWidth;
                            if (minMain > 0 && item.ResolvedMainSize < minMain)
                                item.ResolvedMainSize = minMain;
                            if (!float.IsNaN(maxMain) && maxMain > 0 && item.ResolvedMainSize > maxMain)
                                item.ResolvedMainSize = maxMain;
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
                    float totalScaledShrink = 0; // CSS spec: sum(flex-shrink * base-size)
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

                    float remainingSpace = mainSize - frozenSpace - unfrozenBaseTotal;
                    bool anyNewlyFrozen = false;

                    // Chrome's sequential consumption approach (line_flexer.cc):
                    // Compute cumulative fractions, iterate in reverse, each item
                    // consumes freeSpace * fraction then subtracts from freeSpace.
                    // This ensures the last item absorbs any rounding remainder.

                    // Step 1: Compute cumulative fractions (matching Chrome's FreezeItems)
                    float runningGrow = 0;
                    float runningScaledShrink = 0;
                    float[] fractions = new float[line.Items.Count];
                    for (int i = 0; i < line.Items.Count; i++)
                    {
                        if (frozen[i]) continue;
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

                    // Step 2: Distribute in reverse (matching Chrome's ResolveFlexibleLengths)
                    float freeSpace = remainingSpace;
                    for (int i = line.Items.Count - 1; i >= 0; i--)
                    {
                        if (frozen[i]) continue;
                        var item = line.Items[i];

                        float extraSize;
                        if (fractions[i] >= 1.0f)
                            extraSize = freeSpace;
                        else
                        {
                            double extra = (double)freeSpace * fractions[i];
                            // Round to 1/64px (Chrome's LayoutUnit precision)
                            extraSize = (float)(Math.Round(extra * 64.0, MidpointRounding.AwayFromZero) / 64.0);
                        }
                        freeSpace -= extraSize;

                        float resolved = item.BaseSize + extraSize;
                        resolved = Math.Max(0, resolved);

                        // Check min/max — freeze if clamped
                        if (item.Style != null)
                        {
                            float minMain = GetFlexItemMinMain(item, isColumn);
                            float maxMain = isColumn ? item.Style.MaxHeight : item.Style.MaxWidth;
                            if (minMain > 0 && resolved < minMain)
                            {
                                resolved = minMain;
                                frozen[i] = true;
                                anyNewlyFrozen = true;
                            }
                            if (!float.IsNaN(maxMain) && maxMain > 0 && resolved > maxMain)
                            {
                                resolved = maxMain;
                                frozen[i] = true;
                                anyNewlyFrozen = true;
                            }
                        }

                        item.ResolvedMainSize = resolved;
                    }

                    if (!anyNewlyFrozen) break;
                }

                // Distribute auto margins on the main axis (overrides justify-content)
                float resolvedFreeSpace = mainSize - totalGaps;
                for (int i = 0; i < line.Items.Count; i++)
                    resolvedFreeSpace -= line.Items[i].ResolvedMainSize + GetItemMainMargins(line.Items[i], isColumn);

                int autoMarginCount = 0;
                for (int i = 0; i < line.Items.Count; i++)
                {
                    if (line.Items[i].Style == null) continue;
                    if (isColumn)
                    {
                        if (float.IsNaN(line.Items[i].Style.MarginTop)) autoMarginCount++;
                        if (float.IsNaN(line.Items[i].Style.MarginBottom)) autoMarginCount++;
                    }
                    else
                    {
                        if (float.IsNaN(line.Items[i].Style.MarginLeft)) autoMarginCount++;
                        if (float.IsNaN(line.Items[i].Style.MarginRight)) autoMarginCount++;
                    }
                }

                bool hasAutoMargins = autoMarginCount > 0 && resolvedFreeSpace > 0;
                if (hasAutoMargins)
                {
                    float perAutoMargin = resolvedFreeSpace / autoMarginCount;
                    for (int i = 0; i < line.Items.Count; i++)
                    {
                        var item = line.Items[i];
                        if (item.Style == null) continue;
                        if (isColumn)
                        {
                            if (float.IsNaN(item.Style.MarginTop)) item.Box.MarginTop = perAutoMargin;
                            if (float.IsNaN(item.Style.MarginBottom)) item.Box.MarginBottom = perAutoMargin;
                        }
                        else
                        {
                            if (float.IsNaN(item.Style.MarginLeft)) item.Box.MarginLeft = perAutoMargin;
                            if (float.IsNaN(item.Style.MarginRight)) item.Box.MarginRight = perAutoMargin;
                        }
                    }
                }

                // Position items on main axis
                float mainCursor = isColumn ? parent.ContentRect.Y : parent.ContentRect.X;

                // For auto-sized column containers (no definite height), justify-content
                // has no effect since there's no definite free space.
                var effectiveJustify = style.JustifyContent;
                if (isAutoMainSize)
                    effectiveJustify = CssJustifyContent.FlexStart;
                // For reverse directions, flex-start means the physical end (bottom/right),
                // so swap flex-start ↔ flex-end for justify-content.
                else if (isReverse)
                {
                    if (effectiveJustify == CssJustifyContent.FlexStart)
                        effectiveJustify = CssJustifyContent.FlexEnd;
                    else if (effectiveJustify == CssJustifyContent.FlexEnd)
                        effectiveJustify = CssJustifyContent.FlexStart;
                }

                float finalTotalGrow = 0;
                for (int i = 0; i < line.Items.Count; i++)
                    finalTotalGrow += line.Items[i].FlexGrow;

                var (startOffset, justifyGap) = hasAutoMargins
                    ? (0f, gap)
                    : ApplyJustifyContent(effectiveJustify, resolvedFreeSpace, line.Items.Count, finalTotalGrow > 0, gap);
                mainCursor += startOffset;

                float maxCross = 0;
                for (int i = 0; i < line.Items.Count; i++)
                {
                    if (i > 0) mainCursor += justifyGap;
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

                        // Pre-stretch: when item has auto height and will be stretched,
                        // set cross size to the container's definite cross dimension BEFORE
                        // calling LayoutChildren. This ensures nested column flex containers
                        // receive the correct main-axis size instead of falling back to 10000f.
                        if (contentCross <= 0 && !float.IsNaN(containerHeight) && containerHeight > 0)
                        {
                            var preAlign = item.Style.AlignSelf;
                            if ((int)preAlign == 255) preAlign = style.AlignItems;
                            if (preAlign == CssAlignItems.Stretch || (int)preAlign == 0)
                            {
                                contentCross = containerHeight
                                    - box.PaddingTop - box.PaddingBottom
                                    - box.BorderTopWidth - box.BorderBottomWidth
                                    - box.MarginTop - box.MarginBottom;
                                if (contentCross < 0) contentCross = 0;
                            }
                        }

                        box.ContentRect = new RectF(
                            mainCursor + box.MarginLeft + box.BorderLeftWidth + box.PaddingLeft,
                            crossCursor + box.MarginTop + box.BorderTopWidth + box.PaddingTop,
                            contentMain, contentCross);
                    }

                    // Layout item contents
                    BlockFormattingContext.LayoutChildren(box, context);

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

                // For single-line flex containers with definite cross size,
                // the line's cross size equals the container's inner cross size.
                if (!isWrap || lines.Count == 1)
                {
                    float containerCross = isColumn ? containerWidth : containerHeight;
                    if (!float.IsNaN(containerCross) && containerCross > 0 && containerCross > maxCross)
                        maxCross = containerCross;
                }

                // Apply cross-axis alignment (align-items / align-self)
                for (int i = 0; i < line.Items.Count; i++)
                {
                    var item = line.Items[i];
                    var box = item.Box;

                    // Determine alignment: align-self overrides align-items (255 = auto)
                    var align = item.Style.AlignSelf;
                    if ((int)align == 255)
                        align = style.AlignItems;

                    // Calculate item's total cross size
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
                    if (freeCross <= 0) continue;

                    // Auto margins on the cross axis absorb free space (override align-items/align-self)
                    bool hasAutoMarginCross = false;
                    if (isColumn)
                    {
                        bool autoLeft = float.IsNaN(item.Style.MarginLeft);
                        bool autoRight = float.IsNaN(item.Style.MarginRight);
                        if (autoLeft || autoRight)
                        {
                            hasAutoMarginCross = true;
                            float perMargin = (autoLeft && autoRight) ? freeCross / 2 : freeCross;
                            float offset = autoLeft ? perMargin : 0;
                            box.ContentRect = new Core.Values.RectF(
                                box.ContentRect.X + offset, box.ContentRect.Y,
                                box.ContentRect.Width, box.ContentRect.Height);
                        }
                    }
                    else
                    {
                        bool autoTop = float.IsNaN(item.Style.MarginTop);
                        bool autoBottom = float.IsNaN(item.Style.MarginBottom);
                        if (autoTop || autoBottom)
                        {
                            hasAutoMarginCross = true;
                            float perMargin = (autoTop && autoBottom) ? freeCross / 2 : freeCross;
                            float offset = autoTop ? perMargin : 0;
                            box.ContentRect = new Core.Values.RectF(
                                box.ContentRect.X, box.ContentRect.Y + offset,
                                box.ContentRect.Width, box.ContentRect.Height);
                        }
                    }
                    if (hasAutoMarginCross) continue;

                    float crossOffset = 0;
                    switch (align)
                    {
                        case CssAlignItems.FlexStart:
                        case CssAlignItems.Start:
                            crossOffset = 0;
                            break;
                        case CssAlignItems.Baseline:
                            // Approximate baseline alignment using first line box baseline
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
                            // Stretch: expand cross dimension if auto, clamped to min/max
                            if (isColumn)
                            {
                                float rawW = item.Style.Width;
                                if (float.IsNaN(rawW))
                                {
                                    float newWidth = box.ContentRect.Width + freeCross;
                                    float minW = DimensionResolver.ResolvePercentWidth(item.Style.MinWidth, containerWidth);
                                    float maxW = DimensionResolver.ResolvePercentWidth(item.Style.MaxWidth, containerWidth);
                                    if (!float.IsNaN(minW) && minW >= 0) newWidth = Math.Max(newWidth, minW);
                                    if (!float.IsNaN(maxW) && maxW >= 0) newWidth = Math.Min(newWidth, maxW);
                                    box.ContentRect = new RectF(box.ContentRect.X, box.ContentRect.Y,
                                                                newWidth, box.ContentRect.Height);
                                }
                            }
                            else
                            {
                                float rawH = item.Style.Height;
                                if (float.IsNaN(rawH))
                                {
                                    float newHeight = box.ContentRect.Height + freeCross;
                                    float minH = DimensionResolver.ResolvePercentHeight(item.Style.MinHeight, containerHeight);
                                    float maxH = DimensionResolver.ResolvePercentHeight(item.Style.MaxHeight, containerHeight);
                                    if (!float.IsNaN(minH) && minH >= 0) newHeight = Math.Max(newHeight, minH);
                                    if (!float.IsNaN(maxH) && maxH >= 0) newHeight = Math.Min(newHeight, maxH);
                                    float oldHeight = box.ContentRect.Height;
                                    box.ContentRect = new RectF(box.ContentRect.X, box.ContentRect.Y,
                                                                box.ContentRect.Width, newHeight);

                                    // If the item is a column flex or grid container and height changed,
                                    // re-layout to distribute the new height among tracks/items.
                                    if (newHeight > oldHeight + 0.01f)
                                    {
                                        bool needsRelayout = false;
                                        if (item.Style.Display == CssDisplay.Flex)
                                        {
                                            var itemDir = item.Style.FlexDirection;
                                            if (itemDir == CssFlexDirection.Column || itemDir == CssFlexDirection.ColumnReverse)
                                                needsRelayout = true;
                                        }
                                        else if (item.Style.Display == CssDisplay.Grid)
                                        {
                                            needsRelayout = true;
                                        }
                                        if (needsRelayout)
                                        {
                                            box.ClearChildren();
                                            box.LineBoxes?.Clear();
                                            BlockFormattingContext.LayoutChildren(box, context);
                                        }
                                    }
                                }
                            }
                            crossOffset = 0;
                            break;
                    }

                    if (crossOffset > 0)
                    {
                        if (isColumn)
                            OffsetBoxInPlace(box, crossOffset, 0);
                        else
                            OffsetBoxInPlace(box, 0, crossOffset);
                    }
                }

                crossCursor += maxCross;
                if (li < lines.Count - 1)
                    crossCursor += crossGap;
            }

            // Apply align-content for multi-line flex containers
            if (isWrap && lines.Count > 1)
            {
                float totalLineCross = 0;
                for (int li = 0; li < lines.Count; li++)
                    totalLineCross += lines[li].CrossSize;
                totalLineCross += crossGap * (lines.Count - 1);

                float crossSpace = (isColumn ? containerWidth : containerHeight);
                if (!float.IsNaN(crossSpace) && crossSpace > totalLineCross)
                {
                    float freeCrossSpace = crossSpace - totalLineCross;
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
                        case CssAlignItems.SpaceBetween:
                            if (lines.Count > 1)
                                lineGap = freeCrossSpace / (lines.Count - 1);
                            break;
                        case CssAlignItems.SpaceAround:
                            if (lines.Count > 0)
                            {
                                float halfGap = freeCrossSpace / (lines.Count * 2);
                                lineOffset = halfGap;
                                lineGap = halfGap * 2;
                            }
                            break;
                        case CssAlignItems.SpaceEvenly:
                            if (lines.Count > 0)
                            {
                                float evenGap = freeCrossSpace / (lines.Count + 1);
                                lineOffset = evenGap;
                                lineGap = evenGap;
                            }
                            break;
                        case CssAlignItems.Stretch:
                        default:
                            // Stretch distributes extra space equally among lines
                            lineGap = freeCrossSpace / lines.Count;
                            break;
                        case CssAlignItems.FlexStart:
                        case CssAlignItems.Start:
                        case CssAlignItems.Baseline:
                            // No offset
                            break;
                    }

                    if (lineOffset > 0 || lineGap > 0)
                    {
                        float cumOffset = lineOffset;
                        for (int li = 0; li < lines.Count; li++)
                        {
                            if (cumOffset > 0)
                            {
                                for (int i = 0; i < lines[li].Items.Count; i++)
                                {
                                    if (isColumn)
                                        OffsetBoxInPlace(lines[li].Items[i].Box, cumOffset, 0);
                                    else
                                        OffsetBoxInPlace(lines[li].Items[i].Box, 0, cumOffset);
                                }
                            }
                            cumOffset += lineGap;
                        }
                    }
                }
            }
        }

        private static float ResolveFlexBasis(ComputedStyle style, bool isColumn, float containerWidth,
            float containerHeight, LayoutBox box, StyledElement element, LayoutContext context)
        {
            float basis = style.FlexBasis;
            if (!float.IsNaN(basis) && basis >= 0)
                return basis;
            // Resolve deferred percentage flex-basis against the flex container's main size
            if (!float.IsNaN(basis) && basis < 0 && basis > -1.01f)
                return -basis * (isColumn ? containerHeight : containerWidth);

            // Use width/height as fallback (resolve deferred percentages and calc)
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
                        return Css.Resolution.Internal.ValueResolver.EvaluateDeferredCalc(calcFn, cbDim);
                }
                // Resolve deferred percentage (negative fraction encoding)
                if (size < 0 && size > -1.01f)
                    size = -size * (isColumn ? containerHeight : containerWidth);
                if (size >= 0)
                    return size;
            }

            // Auto: measure content size via trial layout
            var measureBox = new LayoutBox(element, BoxType.Block);
            BoxModelCalculator.ApplyBoxModel(measureBox, style, containerWidth);

            if (isColumn)
            {
                // Column: main axis is height, measure content height
                float w = DimensionResolver.ResolveWidth(style, containerWidth, measureBox);
                measureBox.ContentRect = new RectF(0, 0, w, 0);
                BlockFormattingContext.LayoutChildren(measureBox, context);
                return CalculateAutoHeight(measureBox);
            }
            else
            {
                // Row: main axis is width, use shrink-to-fit heuristic
                // Lay out with full available width, then measure actual content extent
                float availWidth = containerWidth - BoxModelCalculator.GetHorizontalSpacing(measureBox);
                measureBox.ContentRect = new RectF(0, 0, availWidth, 0);
                BlockFormattingContext.LayoutChildren(measureBox, context);
                float contentWidth = MeasureContentWidth(measureBox);
                return Math.Min(contentWidth, availWidth);
            }
        }

        private static float MeasureContentWidth(LayoutBox box)
        {
            float maxRight = 0;
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                float right = child.ContentRect.X + child.ContentRect.Width
                            + child.PaddingRight + child.BorderRightWidth + child.MarginRight
                            - box.ContentRect.X;
                if (right > maxRight) maxRight = right;
            }
            if (box.LineBoxes != null)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    var lb = box.LineBoxes[i];
                    // Measure actual content extent from fragments, not LineBox.Width
                    // (which holds the available/containing width, not content width)
                    float contentRight = 0;
                    for (int f = 0; f < lb.Fragments.Count; f++)
                    {
                        float fragRight = lb.Fragments[f].X + lb.Fragments[f].Width;
                        if (fragRight > contentRight) contentRight = fragRight;
                    }
                    if (contentRight > maxRight) maxRight = contentRight;
                }
            }
            return maxRight;
        }

        /// <summary>
        /// Returns the effective min-main-size for a flex item.
        /// Uses the explicit min-width/min-height if set, otherwise 0.
        /// CSS spec §4.5: min-width:auto = min(content_min, specified_size),
        /// but computing content_min is expensive; 0 is a safe approximation
        /// that allows normal shrinking while still respecting explicit minimums.
        /// </summary>
        private static float GetFlexItemMinMain(FlexItem item, bool isColumn)
        {
            float explicitMin = isColumn ? item.Style.MinHeight : item.Style.MinWidth;
            if (!float.IsNaN(explicitMin) && explicitMin > 0)
                return explicitMin;
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
            CssJustifyContent justify, float freeSpace, int itemCount, bool hasGrow, float defaultGap)
        {
            if (hasGrow || freeSpace <= 0)
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
            var refValues = (object?[])source.GetRefValues().Clone();
            return new ComputedStyle(values, refValues);
        }
    }
}
