using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Style;
using Rend.Text;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Block formatting context: stack children vertically with margin collapsing.
    /// CSS 2.1 §9.4.1
    /// </summary>
    internal static class BlockFormattingContext
    {
        /// <summary>
        /// Returns true if the style specifies a vertical writing mode
        /// (vertical-rl or vertical-lr).
        /// </summary>
        internal static bool IsVerticalWritingMode(ComputedStyle? style)
        {
            if (style == null) return false;
            return style.WritingMode == CssWritingMode.VerticalRl ||
                   style.WritingMode == CssWritingMode.VerticalLr;
        }

        /// <summary>
        /// Layout block-level children within a containing block.
        /// </summary>
        public static void Layout(LayoutBox parent, LayoutContext context)
        {
            var style = parent.StyledNode?.Style;
            bool vertical = IsVerticalWritingMode(style);

            // In vertical writing mode the inline dimension is the container's height,
            // and blocks stack horizontally. Use the height as the "containing width"
            // for inline sizing when the container has a definite height.
            float containingWidth = vertical
                ? (style != null && !float.IsNaN(style.Height) ? style.Height : parent.ContentRect.Height)
                : parent.ContentRect.Width;
            if (containingWidth <= 0 && vertical)
                containingWidth = parent.ContentRect.Width; // fallback

            float cursorY = parent.ContentRect.Y;
            float cursorX = parent.ContentRect.X;
            float prevMarginBottom = 0;

            context.ContainingBlockWidth = vertical ? parent.ContentRect.Width : containingWidth;

            // Determine the parent's definite content height for percentage height resolution.
            // If the parent has an explicit CSS height, use it; otherwise NaN (auto).
            // [CSS-FLEXBOX §9.8] Stretched flex items have definite cross size.
            float parentContentHeight = parent.ContentRect.Height;
            if (float.IsNaN(parentContentHeight) || parentContentHeight <= 0)
            {
                var parentStyled = parent.StyledNode as StyledElement;
                if (parentStyled != null)
                {
                    float h = parentStyled.Style.Height;
                    if (!float.IsNaN(h) && h > 0) { parentContentHeight = h; }
                }
            }
            if ((float.IsNaN(parentContentHeight) || parentContentHeight <= 0) && parent.HasDefiniteCrossSize)
            {
                parentContentHeight = parent.ContentRect.Height;
            }

            // [CSS-SIZING-3 §5.4] Propagate the containing block height so that nested
            // formatting contexts (IFC replaced elements, nested BFC) can resolve
            // percentage heights against it.
            var savedContainingBlockHeight = context.ContainingBlockHeight;
            context.ContainingBlockHeight = parentContentHeight;

            var styledElement = parent.StyledNode as StyledElement;
            if (styledElement == null)
            {
                context.ContainingBlockHeight = savedContainingBlockHeight;
                return;
            }

            var floatCtx = new FloatContext(parent.ContentRect.X, parent.ContentRect.Width);
            var prevFloatCtx = context.FloatContext;
            context.FloatContext = floatCtx;

            // Flatten display:contents children into effective child list
            var effectiveChildren = FlattenContents(styledElement);

            // <details> without open attribute: only show the first <summary> child
            bool isClosedDetails = styledElement.TagName == "details"
                                   && styledElement.GetAttribute("open") == null;

            // <fieldset>: track whether we have a <legend> for special positioning
            bool isFieldset = styledElement.TagName == "fieldset";
            bool legendHandled = false;

            bool foundSummary = false;
            bool isFirstInFlowChild = true;

            for (int i = 0; i < effectiveChildren.Count; i++)
            {
                var child = effectiveChildren[i];

                // For closed <details>, skip everything except the first <summary>
                if (isClosedDetails)
                {
                    if (child.IsText) continue; // skip text nodes in closed details
                    if (child is StyledPseudoElement) continue;
                    var detailChild = child as StyledElement;
                    if (detailChild != null && detailChild.TagName == "summary" && !foundSummary)
                    {
                        foundSummary = true;
                        // Allow this child to be laid out (fall through)
                    }
                    else
                    {
                        continue; // Skip all other children
                    }
                }

                if (child.IsText)
                {
                    // [CSS2 §9.2.1.1] Text among block-level siblings is wrapped in an
                    // anonymous block that runs InlineFormattingContext. Consecutive
                    // inline-level siblings (inline pseudo-elements, inline/inline-block
                    // elements) are absorbed into the same anonymous block so they share
                    // a line box, matching Chrome's anonymous block coalescing.
                    var textNode = (StyledText)child;
                    if (string.IsNullOrWhiteSpace(textNode.Text))
                    {
                        continue;
                    }

                    if (vertical)
                    {
                        var inlineBox = CreateInlineBox(textNode, context, containingWidth, cursorY, vertical);
                        inlineBox.ContentRect = new RectF(cursorX, parent.ContentRect.Y, 0, containingWidth);
                        parent.AddChild(inlineBox);
                        cursorX += inlineBox.ContentRect.Width > 0 ? inlineBox.ContentRect.Width : inlineBox.MarginRect.Width;
                    }
                    else
                    {
                        var inlineRun = new List<StyledNode>();
                        inlineRun.Add(textNode);
                        CollectAdjacentInlineRun(effectiveChildren, ref i, inlineRun);

                        float textAnonY = cursorY + MarginCollapsing.Collapse(prevMarginBottom, 0);
                        var anonBox = CreateAnonymousBlockForInlineRun(inlineRun, styledElement, parent, textAnonY, containingWidth, context);
                        parent.AddChild(anonBox);
                        cursorY = anonBox.ContentRect.Y + anonBox.ContentRect.Height;
                    }
                    prevMarginBottom = 0;
                    isFirstInFlowChild = false;
                    continue;
                }

                // [CSS2 §9.7] Absolutely/fixed positioned or floated pseudo-elements
                // get blockified display. Convert to a StyledElement so the normal
                // element layout path handles abspos/float positioning correctly.
                if (child is StyledPseudoElement pseudo
                    && (pseudo.Style.Position == CssPosition.Absolute
                        || pseudo.Style.Position == CssPosition.Fixed
                        || pseudo.Style.Float != CssFloat.None))
                {
                    child = ConvertPseudoToElement(pseudo, styledElement);
                    // Fall through to the StyledElement handling below
                }

                if (child is StyledPseudoElement inlinePseudo)
                {
                    var pseudoDisplay = inlinePseudo.Style.Display;
                    // [CSS2 §12.1] Pseudo-elements with block-level display (flex, grid,
                    // block, table) create proper layout boxes, not inline text.
                    if (pseudoDisplay == CssDisplay.Flex || pseudoDisplay == CssDisplay.InlineFlex
                        || pseudoDisplay == CssDisplay.Grid || pseudoDisplay == CssDisplay.InlineGrid
                        || pseudoDisplay == CssDisplay.Block || pseudoDisplay == CssDisplay.Table)
                    {
                        var doc = styledElement.Element.OwnerDocument;
                        var pseudoEl = doc!.CreateElement("div");
                        var pseudoChildren = new List<StyledNode>();
                        if (!string.IsNullOrEmpty(inlinePseudo.Content))
                        {
                            pseudoChildren.Add(new StyledText(inlinePseudo.Content, inlinePseudo.Style));
                        }
                        var pseudoStyled = new StyledElement(pseudoEl, inlinePseudo.Style, pseudoChildren);
                        var pseudoBox = CreateLayoutBox(pseudoStyled);
                        BoxModelCalculator.ApplyBoxModel(pseudoBox, inlinePseudo.Style, containingWidth);
                        float pseudoW = DimensionResolver.ResolveWidth(inlinePseudo.Style, containingWidth, pseudoBox);
                        float pseudoH = DimensionResolver.ResolveHeight(inlinePseudo.Style, float.NaN, pseudoBox);
                        if (float.IsNaN(pseudoH)) { pseudoH = 0; }
                        float pseudoY = cursorY + MarginCollapsing.Collapse(prevMarginBottom, pseudoBox.MarginTop)
                                      + pseudoBox.BorderTopWidth + pseudoBox.PaddingTop;
                        pseudoBox.ContentRect = new RectF(
                            parent.ContentRect.X + pseudoBox.MarginLeft + pseudoBox.BorderLeftWidth + pseudoBox.PaddingLeft,
                            pseudoY, pseudoW, pseudoH);
                        LayoutChildren(pseudoBox, context);
                        if (pseudoH <= 0) { pseudoH = CalculateAutoHeight(pseudoBox); }
                        pseudoBox.ContentRect = new RectF(pseudoBox.ContentRect.X, pseudoBox.ContentRect.Y, pseudoW, pseudoH);
                        parent.AddChild(pseudoBox);
                        cursorY = pseudoBox.ContentRect.Y + pseudoH + pseudoBox.PaddingBottom + pseudoBox.BorderBottomWidth;
                        prevMarginBottom = pseudoBox.MarginBottom;
                        isFirstInFlowChild = false;
                        continue;
                    }

                    // Inline pseudo-element: render as inline text, coalescing with
                    // any consecutive inline siblings into one anonymous block.
                    if (vertical)
                    {
                        var pseudoText = new StyledText(inlinePseudo.Content, inlinePseudo.Style);
                        var inlineBox = CreateInlineBox(pseudoText, context, containingWidth, cursorY, vertical);
                        inlineBox.ContentRect = new RectF(cursorX, parent.ContentRect.Y, 0, containingWidth);
                        parent.AddChild(inlineBox);
                        cursorX += inlineBox.ContentRect.Width > 0 ? inlineBox.ContentRect.Width : inlineBox.MarginRect.Width;
                    }
                    else
                    {
                        var inlineRun = new List<StyledNode>();
                        inlineRun.Add(inlinePseudo);
                        CollectAdjacentInlineRun(effectiveChildren, ref i, inlineRun);

                        float pseudoAnonY = cursorY + MarginCollapsing.Collapse(prevMarginBottom, 0);
                        var anonBox = CreateAnonymousBlockForInlineRun(inlineRun, styledElement, parent, pseudoAnonY, containingWidth, context);
                        parent.AddChild(anonBox);
                        cursorY = anonBox.ContentRect.Y + anonBox.ContentRect.Height;
                    }
                    prevMarginBottom = 0;
                    isFirstInFlowChild = false;
                    continue;
                }

                var childElement = (StyledElement)child;
                var childStyle = childElement.Style;

                // Skip display:none
                if (childStyle.Display == CssDisplay.None) continue;

                // [CSS2 §9.2.1.1] Inline-level elements among block-level siblings must
                // be wrapped in anonymous block boxes so they can be laid out via
                // InlineFormattingContext. This covers inline-block/flex/grid directly,
                // and plain display:inline elements that became BFC children via
                // display:contents flattening (but only when those inline elements
                // contain no block-level descendants — an inline span with block
                // children needs the block layout path for HTML5 IB-split semantics).
                // Inline-level replaced elements (e.g. inline-block &lt;input&gt;, &lt;img&gt;)
                // participate in the inline run just like non-replaced inline content.
                if (!vertical &&
                    ShouldWrapInlineElementInAnonBlock(childElement) &&
                    childStyle.Position != CssPosition.Absolute &&
                    childStyle.Position != CssPosition.Fixed &&
                    childStyle.Float == CssFloat.None)
                {
                    var inlineRun = new List<StyledNode>();
                    inlineRun.Add(childElement);
                    CollectAdjacentInlineRun(effectiveChildren, ref i, inlineRun);

                    float inlineAnonY = cursorY + MarginCollapsing.Collapse(prevMarginBottom, 0);
                    var anonBox = CreateAnonymousBlockForInlineRun(inlineRun, styledElement, parent, inlineAnonY, containingWidth, context);
                    parent.AddChild(anonBox);
                    cursorY = anonBox.ContentRect.Y + anonBox.ContentRect.Height;
                    prevMarginBottom = 0;
                    continue;
                }

                // <dialog> without open attribute is hidden
                if (childElement.TagName == "dialog" && childElement.GetAttribute("open") == null)
                    continue;

                // Absolutely/fixed positioned elements are out of normal flow.
                // Still create the box and add as child (for positioning later),
                // but don't advance cursorY or participate in margin collapsing.
                if (childStyle.Position == CssPosition.Absolute || childStyle.Position == CssPosition.Fixed)
                {
                    var posBox = CreateLayoutBox(childElement);
                    BoxModelCalculator.ApplyBoxModel(posBox, childStyle, containingWidth);
                    float posWidth;
                    if (SizingKeyword.IsSizingKeyword(childStyle.Width))
                    {
                        // Intrinsic sizing keywords (fit-content, min-content, max-content):
                        // measure content width, don't fill available space
                        posWidth = MeasureIntrinsicWidth(childElement, childStyle.Width, containingWidth, context);
                    }
                    else
                    {
                        posWidth = DimensionResolver.ResolveWidth(childStyle, containingWidth, posBox);
                        // CSS 2.1 §10.3.7: Absolutely positioned elements with auto width
                        // use shrink-to-fit (= fit-content), not fill available space.
                        // When left/right is set, constrain available width accordingly.
                        if (float.IsNaN(childStyle.Width) && !DeferredPercent.IsEncoded(childStyle.Width))
                        {
                            float leftVal = childStyle.Left;
                            float rightVal = childStyle.Right;

                            // CSS 2.1 §10.3.7: when both left and right are set with
                            // auto width, width = containing - left - right - margins/border/padding.
                            if (!float.IsNaN(leftVal) && !float.IsNaN(rightVal))
                            {
                                float resolvedLeft = DeferredPercent.IsEncoded(leftVal)
                                    ? DeferredPercent.Resolve(leftVal, containingWidth) : leftVal;
                                float resolvedRight = DeferredPercent.IsEncoded(rightVal)
                                    ? DeferredPercent.Resolve(rightVal, containingWidth) : rightVal;
                                posWidth = containingWidth - resolvedLeft - resolvedRight
                                         - posBox.MarginLeft - posBox.MarginRight
                                         - posBox.BorderLeftWidth - posBox.BorderRightWidth
                                         - posBox.PaddingLeft - posBox.PaddingRight;
                                if (posWidth < 0)
                                {
                                    posWidth = 0;
                                }
                            }
                            else
                            {
                                // Only one of left/right set (or neither): shrink-to-fit
                                // [CSS-TABLES §abspos] Available width for abspos shrink-to-fit
                                // should never exceed the containing block width.
                                float shrinkAvail = containingWidth;
                                if (!float.IsNaN(leftVal))
                                {
                                    float resolvedLeft = DeferredPercent.IsEncoded(leftVal)
                                        ? DeferredPercent.Resolve(leftVal, containingWidth) : leftVal;
                                    shrinkAvail = Math.Min(containingWidth, containingWidth - resolvedLeft);
                                }
                                else if (!float.IsNaN(rightVal))
                                {
                                    float resolvedRight = DeferredPercent.IsEncoded(rightVal)
                                        ? DeferredPercent.Resolve(rightVal, containingWidth) : rightVal;
                                    shrinkAvail = Math.Min(containingWidth, containingWidth - resolvedRight);
                                }
                                if (shrinkAvail < 0) { shrinkAvail = 0; }
                                posWidth = MeasureIntrinsicWidth(childElement, SizingKeyword.FitContent, shrinkAvail, context);
                            }
                        }
                    }
                    // [CSS-SIZING-4 §5.1] Abspos with auto width: if the element has
                    // an explicit height and aspect-ratio, derive width from height * ratio.
                    if (float.IsNaN(childStyle.Width) && !DeferredPercent.IsEncoded(childStyle.Width))
                    {
                        float absAspectRatio = DimensionResolver.GetAspectRatio(childStyle);
                        float absSpecHeight = childStyle.Height;
                        if (absAspectRatio > 0 && !float.IsNaN(absSpecHeight) && absSpecHeight > 0
                            && !DeferredPercent.IsEncoded(absSpecHeight)
                            && !float.IsNegativeInfinity(absSpecHeight))
                        {
                            if (childStyle.BoxSizing == CssBoxSizing.BorderBox)
                            {
                                float widthBorderBox = absSpecHeight * absAspectRatio;
                                posWidth = widthBorderBox
                                         - posBox.PaddingLeft - posBox.PaddingRight
                                         - posBox.BorderLeftWidth - posBox.BorderRightWidth;
                                if (posWidth < 0)
                                {
                                    posWidth = 0;
                                }
                            }
                            else
                            {
                                posWidth = absSpecHeight * absAspectRatio;
                            }
                        }
                    }
                    // [CSS-SIZING-4 §5.1] Transfer max-height → max-width through
                    // aspect-ratio for abspos elements. When an element has aspect-ratio
                    // and max-height, the effective max-width = max-height * ratio.
                    {
                        float absAr = DimensionResolver.GetAspectRatio(childStyle);
                        if (absAr > 0)
                        {
                            float maxH = childStyle.MaxHeight;
                            if (!float.IsNaN(maxH) && maxH >= 0 && !DeferredPercent.IsEncoded(maxH))
                            {
                                float transferredMaxW = maxH * absAr;
                                if (childStyle.BoxSizing == CssBoxSizing.BorderBox)
                                {
                                    transferredMaxW -= posBox.PaddingLeft + posBox.PaddingRight
                                                    + posBox.BorderLeftWidth + posBox.BorderRightWidth;
                                    if (transferredMaxW < 0) { transferredMaxW = 0; }
                                }
                                if (posWidth > transferredMaxW)
                                {
                                    posWidth = transferredMaxW;
                                }
                            }
                            float minH = childStyle.MinHeight;
                            if (!float.IsNaN(minH) && minH > 0 && !DeferredPercent.IsEncoded(minH))
                            {
                                float transferredMinW = minH * absAr;
                                if (childStyle.BoxSizing == CssBoxSizing.BorderBox)
                                {
                                    transferredMinW -= posBox.PaddingLeft + posBox.PaddingRight
                                                    + posBox.BorderLeftWidth + posBox.BorderRightWidth;
                                    if (transferredMinW < 0) { transferredMinW = 0; }
                                }
                                if (posWidth < transferredMinW)
                                {
                                    posWidth = transferredMinW;
                                }
                            }
                        }
                    }

                    // [CSS2 §10.4] Apply min/max-width to abspos shrink-to-fit
                    float absMinW = DimensionResolver.ResolvePercentWidth(childStyle.MinWidth, containingWidth, childStyle, PropertyId.MinWidth);
                    float absMaxW = DimensionResolver.ResolvePercentWidth(childStyle.MaxWidth, containingWidth, childStyle, PropertyId.MaxWidth);
                    // [CSS-SIZING-3 §5.1] Resolve sizing keywords for min/max-width
                    if (SizingKeyword.IsSizingKeyword(childStyle.MaxWidth))
                    {
                        absMaxW = MeasureIntrinsicWidth(childElement, childStyle.MaxWidth, containingWidth, context);
                    }
                    if (SizingKeyword.IsSizingKeyword(childStyle.MinWidth))
                    {
                        absMinW = MeasureIntrinsicWidth(childElement, childStyle.MinWidth, containingWidth, context);
                    }
                    if (!float.IsNaN(absMaxW) && absMaxW >= 0 && posWidth > absMaxW)
                    {
                        posWidth = absMaxW;
                    }
                    if (!float.IsNaN(absMinW) && absMinW >= 0 && posWidth < absMinW)
                    {
                        posWidth = absMinW;
                    }
                    // Static position Y: where the element's content edge would be in normal flow.
                    // Include the collapsed margin gap from the previous sibling, plus
                    // the element's own border and padding (since this is the content rect Y).
                    float staticY = cursorY + MarginCollapsing.Collapse(prevMarginBottom, posBox.MarginTop)
                                  + posBox.BorderTopWidth + posBox.PaddingTop;
                    // Pre-resolve height for abspos elements. If height is auto but
                    // both top and bottom are set, compute height from the containing
                    // block (CSS 2.1 §10.6.4) so flex/grid children get a definite size.
                    // Set width first so aspect-ratio can derive height from width.
                    posBox.ContentRect = new RectF(0, 0, posWidth, 0);
                    // [CSS2 §10.1] For abspos elements, percentage heights resolve against the
                    // containing block. When parentContentHeight is 0 (no positioned ancestor with
                    // known height), try viewport height for deferred calc expressions.
                    float absContainingHeight = parentContentHeight;
                    if ((absContainingHeight <= 0 || float.IsNaN(absContainingHeight))
                        && float.IsNegativeInfinity(childStyle.Height))
                    {
                        float vpH = Css.Resolution.Internal.ValueResolver.ViewportHeightHint;
                        if (vpH > 0)
                        {
                            absContainingHeight = vpH;
                        }
                    }
                    float preHeight = DimensionResolver.ResolveHeight(childStyle, absContainingHeight, posBox);
                    if (float.IsNaN(preHeight) && parentContentHeight > 0)
                    {
                        float topVal = childStyle.Top;
                        float bottomVal = childStyle.Bottom;
                        if (!float.IsNaN(topVal) && !float.IsNaN(bottomVal))
                        {
                            float resolvedTop = DeferredPercent.IsEncoded(topVal)
                                ? DeferredPercent.Resolve(topVal, parentContentHeight) : topVal;
                            float resolvedBottom = DeferredPercent.IsEncoded(bottomVal)
                                ? DeferredPercent.Resolve(bottomVal, parentContentHeight) : bottomVal;
                            preHeight = parentContentHeight - resolvedTop - resolvedBottom
                                      - posBox.MarginTop - posBox.MarginBottom
                                      - posBox.BorderTopWidth - posBox.BorderBottomWidth
                                      - posBox.PaddingTop - posBox.PaddingBottom;
                            if (preHeight < 0)
                            {
                                preHeight = 0;
                            }
                        }
                    }
                    if (float.IsNaN(preHeight))
                    {
                        preHeight = 0;
                    }
                    // [CSS2 §10.3.7] Static position X for abspos elements.
                    // For inline-level elements, the static position is where a hypothetical
                    // inline box would be placed — respecting float exclusions and text-align.
                    // For block-level elements, the margin edge aligns with the parent content edge.
                    float staticX;
                    bool isInlineLevel = childStyle.Display == CssDisplay.Inline
                                      || childStyle.Display == CssDisplay.InlineBlock
                                      || childStyle.Display == CssDisplay.InlineFlex
                                      || childStyle.Display == CssDisplay.InlineGrid;
                    if (isInlineLevel)
                    {
                        staticX = ComputeInlineStaticX(styledElement, floatCtx,
                            cursorY, prevMarginBottom, posBox, posWidth);
                    }
                    else
                    {
                        staticX = parent.ContentRect.X + posBox.MarginLeft
                                + posBox.BorderLeftWidth + posBox.PaddingLeft;
                    }
                    posBox.ContentRect = new RectF(staticX, staticY, posWidth, preHeight);
                    LayoutChildren(posBox, context);
                    float posHeight = preHeight > 0 ? preHeight : CalculateAutoHeight(posBox);
                    posBox.ContentRect = new RectF(posBox.ContentRect.X, posBox.ContentRect.Y, posWidth, posHeight);
                    parent.AddChild(posBox);
                    continue;
                }

                // Handle floated elements
                if (childStyle.Float != CssFloat.None)
                {
                    var floatBox = CreateLayoutBox(childElement);
                    // [CSS2 §9.5.1] Float's outer top must not be higher than the outer
                    // top of any earlier block or float. Account for previous sibling's
                    // bottom margin (float margins don't collapse, so use the full margin).
                    floatCtx.CurrentY = cursorY + prevMarginBottom;
                    FloatLayout.PlaceFloat(floatBox, floatCtx, parent, context);
                    parent.AddChild(floatBox);
                    continue;
                }

                var childBox = CreateLayoutBox(childElement);

                // Wire up the parent pointer before LayoutChildren runs so that
                // inner layout passes (e.g. list-item marker reserve computation
                // in InlineFormattingContext) can walk back to the container.
                // parent.AddChild later re-assigns this to the same value.
                childBox.Parent = parent;

                // Apply box model
                BoxModelCalculator.ApplyBoxModel(childBox, childStyle, containingWidth);

                // [CSS 2.1 §17.6.2] In collapsed-border mode the table element's
                // border participates in the collapse algorithm and does not inset
                // the content area or get painted separately. Zero the border widths
                // on the box so ContentRect positioning and BorderPainter skip them;
                // the style object retains the originals for CollapseBorders.
                if (childStyle.Display == CssDisplay.Table
                    && childStyle.BorderCollapse == CssBorderCollapse.Collapse)
                {
                    childBox.BorderTopWidth = 0;
                    childBox.BorderRightWidth = 0;
                    childBox.BorderBottomWidth = 0;
                    childBox.BorderLeftWidth = 0;
                }

                // Resolve content width
                float contentWidth;
                bool isReplaced = ReplacedElementLayout.IsReplaced(childElement);

                if (isReplaced && (float.IsNaN(childStyle.Width) || SizingKeyword.IsSizingKeyword(childStyle.Width)))
                {
                    // [CSS-SIZING-3 §5.1] Replaced element with auto or intrinsic sizing keyword width:
                    // use HTML attribute, form control defaults, or fallback.
                    // min-content/max-content/fit-content resolve to the intrinsic size for replaced elements.
                    float intrinsicW = 0;
                    string? attrW = childElement.GetAttribute("width");
                    if (attrW != null && float.TryParse(attrW, out float aw)) intrinsicW = aw;
                    if (intrinsicW <= 0 && ReplacedElementLayout.IsFormControl(childElement))
                        intrinsicW = ReplacedElementLayout.GetFormControlIntrinsicWidth(childElement, context.TextMeasurer);
                    if (intrinsicW <= 0 && childElement.TagName == "math")
                    {
                        var mathSize = Rendering.Internal.MathmlRenderer.MeasureElement(
                            childElement.Element, 16f, context.TextMeasurer);
                        intrinsicW = mathSize.Width + 4f;
                    }
                    if (intrinsicW <= 0 &&
                        ReplacedElementLayout.TryGetDataUriDimensions(childElement, out float duW0, out _))
                    {
                        intrinsicW = duW0;
                    }
                    // [CSS-IMAGES-3 §2.2] SVG with viewBox: derive width from ratio × height
                    if (intrinsicW <= 0 && childElement.TagName == "svg")
                    {
                        float svgRatio = ReplacedElementLayout.GetIntrinsicRatio(childElement);
                        if (svgRatio > 0 && !float.IsNaN(childStyle.Height) && childStyle.Height > 0)
                        {
                            intrinsicW = childStyle.Height * svgRatio;
                        }
                    }
                    contentWidth = intrinsicW > 0 ? intrinsicW : 300;
                }
                else if (SizingKeyword.IsSizingKeyword(childStyle.Width))
                {
                    // Intrinsic sizing keyword: measure content
                    contentWidth = MeasureIntrinsicWidth(childElement, childStyle.Width, containingWidth, context);

                    // [CSS-SIZING-4 §5.2] Transfer max-height → max-width through aspect-ratio
                    // for sizing keywords (max-content, min-content, fit-content).
                    float arRatio = DimensionResolver.GetAspectRatio(childStyle);
                    if (arRatio > 0 && float.IsNaN(childStyle.Height))
                    {
                        float maxH = DimensionResolver.ResolvePercentHeight(childStyle.MaxHeight, parentContentHeight);
                        if (!float.IsNaN(maxH) && maxH >= 0)
                        {
                            float transferredMaxW = maxH * arRatio;
                            if (contentWidth > transferredMaxW)
                            {
                                contentWidth = transferredMaxW;
                            }
                        }
                        float minH = DimensionResolver.ResolvePercentHeight(childStyle.MinHeight, parentContentHeight);
                        if (!float.IsNaN(minH) && minH >= 0)
                        {
                            float transferredMinW = minH * arRatio;
                            if (contentWidth < transferredMinW)
                            {
                                contentWidth = transferredMinW;
                            }
                        }
                    }
                }
                else if (childStyle.Display == CssDisplay.Table
                    && childElement.TagName != "table"
                    && (float.IsNaN(childStyle.Width) || childStyle.Width == 0))
                {
                    // [CSS-TABLES §17.5.2] Non-table elements with display:table and auto
                    // width use shrink-to-fit: min(max-content, available).
                    // Real <table> elements handle shrink-to-fit via TableLayout + post-layout
                    // adjustment at the contentRect update below.
                    float availableForTable = DimensionResolver.ResolveWidth(
                        childStyle, containingWidth, childBox, parentContentHeight);
                    float maxContentWidth = MeasureIntrinsicWidth(
                        childElement, SizingKeyword.MaxContent, containingWidth, context);
                    contentWidth = Math.Min(maxContentWidth, availableForTable);
                }
                else
                {
                    contentWidth = DimensionResolver.ResolveWidth(childStyle, containingWidth, childBox, parentContentHeight);
                }

                // [CSS-SIZING §5.2] Apply min-width/max-width constraints.
                // [CSS-UI §3.2] When box-sizing: border-box, min-width/max-width
                // apply to the border box, so subtract horizontal padding+border
                // before clamping the content width.
                float cwMinW = DimensionResolver.ResolvePercentWidth(childStyle.MinWidth, containingWidth, childStyle, PropertyId.MinWidth);
                float cwMaxW = DimensionResolver.ResolvePercentWidth(childStyle.MaxWidth, containingWidth, childStyle, PropertyId.MaxWidth);
                if (childStyle.BoxSizing == CssBoxSizing.BorderBox)
                {
                    float horizontalExtra = childBox.PaddingLeft + childBox.PaddingRight
                                          + childBox.BorderLeftWidth + childBox.BorderRightWidth;
                    if (!float.IsNaN(cwMinW) && cwMinW >= 0)
                    {
                        cwMinW = Math.Max(0, cwMinW - horizontalExtra);
                    }
                    if (!float.IsNaN(cwMaxW) && cwMaxW >= 0)
                    {
                        cwMaxW = Math.Max(0, cwMaxW - horizontalExtra);
                    }
                }
                if (!float.IsNaN(cwMaxW) && cwMaxW >= 0 && contentWidth > cwMaxW)
                {
                    contentWidth = cwMaxW;
                }
                if (!float.IsNaN(cwMinW) && cwMinW >= 0 && contentWidth < cwMinW)
                {
                    contentWidth = cwMinW;
                }

                // Resolve auto margins
                var tempRect = new RectF(0, 0, contentWidth, 0);
                childBox.ContentRect = tempRect;
                DimensionResolver.ResolveAutoMargins(childStyle, childBox, containingWidth);

                // Margin collapsing
                float marginTop = childBox.MarginTop;
                float collapsedMargin;
                bool wasFirstInFlow = isFirstInFlowChild;
                isFirstInFlowChild = false;

                // Apply clear property (CSS 2.1 §9.5.2)
                // Must be after marginTop is known to compute hypothetical border position.
                bool hasClearance = false;
                if (childStyle.Clear != CssClear.None)
                {
                    float clearY = floatCtx.GetClearY(childStyle.Clear);
                    // Compute hypothetical collapsed margin (as if clear:none)
                    float hypotheticalMargin;
                    if (wasFirstInFlow && MarginCollapsing.ShouldCollapseWithFirstChild(parent))
                        hypotheticalMargin = 0;
                    else
                        hypotheticalMargin = MarginCollapsing.Collapse(prevMarginBottom, marginTop);
                    float hypotheticalBorderEdge = cursorY + hypotheticalMargin;
                    if (clearY > hypotheticalBorderEdge)
                    {
                        // Clearance needed: place border edge at clearY
                        cursorY = clearY;
                        hasClearance = true;
                    }
                }

                if (hasClearance)
                {
                    // CSS 2.1 §9.5.2: clearance positions the border edge at the
                    // float bottom. The margin-top is above the clearance, so we
                    // set collapsedMargin=0 — cursorY already equals clearY.
                    collapsedMargin = 0;
                }
                else if (wasFirstInFlow && MarginCollapsing.ShouldCollapseWithFirstChild(parent))
                {
                    collapsedMargin = MarginCollapsing.Collapse(parent.MarginTop, marginTop);
                    parent.MarginTop = collapsedMargin;
                    collapsedMargin = 0;
                }
                else
                {
                    collapsedMargin = MarginCollapsing.Collapse(prevMarginBottom, marginTop);
                }

                if (vertical)
                {
                    // [CSS-WRITING-MODES-3 §3.1, §7.1] Vertical writing mode: blocks
                    // stack horizontally. CSS width = block-size (physical width),
                    // CSS height = inline-size (physical height). Re-resolve both
                    // dimensions with correct logical-to-physical mapping.
                    float parentBlockSize = parent.ContentRect.Width;

                    float x = cursorX + childBox.MarginLeft + childBox.BorderLeftWidth + childBox.PaddingLeft;
                    float y = parent.ContentRect.Y + childBox.MarginTop + childBox.BorderTopWidth + childBox.PaddingTop;

                    float childWidth;
                    float childHeight;

                    if (isReplaced)
                    {
                        float intrinsicW = 0, intrinsicH = 0;
                        string? attrW = childElement.GetAttribute("width");
                        string? attrH = childElement.GetAttribute("height");
                        if (attrW != null && float.TryParse(attrW, out float aw)) { intrinsicW = aw; }
                        if (attrH != null && float.TryParse(attrH, out float ah)) { intrinsicH = ah; }
                        if (ReplacedElementLayout.IsFormControl(childElement))
                        {
                            if (intrinsicW <= 0) { intrinsicW = ReplacedElementLayout.GetFormControlIntrinsicWidth(childElement, context.TextMeasurer); }
                            if (intrinsicH <= 0) { intrinsicH = ReplacedElementLayout.GetFormControlIntrinsicHeight(childElement); }
                        }
                        if (childElement.TagName == "math" && (intrinsicW <= 0 || intrinsicH <= 0))
                        {
                            var mathSize = Rendering.Internal.MathmlRenderer.MeasureElement(childElement.Element, 16f, context.TextMeasurer);
                            if (intrinsicW <= 0) { intrinsicW = mathSize.Width + 4f; }
                            if (intrinsicH <= 0) { intrinsicH = mathSize.Height; }
                        }
                        if ((intrinsicW <= 0 || intrinsicH <= 0) &&
                            ReplacedElementLayout.TryGetDataUriDimensions(childElement, out float duW, out float duH))
                        {
                            if (intrinsicW <= 0) { intrinsicW = duW; }
                            if (intrinsicH <= 0) { intrinsicH = duH; }
                        }
                        if ((intrinsicW <= 0 || intrinsicH <= 0) && childElement.TagName == "svg")
                        {
                            string? viewBox = childElement.GetAttribute("viewbox");
                            if (viewBox != null)
                            {
                                var vbParts = viewBox.Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                                if (vbParts.Length >= 4
                                    && float.TryParse(vbParts[2], System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture, out float vbW)
                                    && float.TryParse(vbParts[3], System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture, out float vbH))
                                {
                                    if (intrinsicW <= 0) { intrinsicW = vbW; }
                                    if (intrinsicH <= 0) { intrinsicH = vbH; }
                                }
                            }
                        }

                        // [CSS-SIZING-4 §3] contain:size overrides intrinsic dimensions
                        var containValueVwm = childStyle.Contain;
                        if (containValueVwm == CssContain.Size || containValueVwm == CssContain.Strict)
                        {
                            float ciW = childStyle.GetValues()[PropertyId.ContainIntrinsicWidth].FloatValue;
                            float ciH = childStyle.GetValues()[PropertyId.ContainIntrinsicHeight].FloatValue;
                            if (!float.IsNaN(ciW) && ciW > 0) { intrinsicW = ciW; }
                            if (!float.IsNaN(ciH) && ciH > 0) { intrinsicH = ciH; }
                        }

                        ReplacedElementLayout.ResolveDimensions(childBox, childStyle, containingWidth, parentContentHeight, intrinsicW, intrinsicH);
                        childWidth = childBox.ContentRect.Width;
                        childHeight = childBox.ContentRect.Height;
                    }
                    else
                    {
                        bool childVertical = IsVerticalWritingMode(childStyle);

                        if (!childVertical)
                        {
                            // [CSS-WRITING-MODES-3 §7.3.1] Orthogonal flow: horizontal-tb
                            // child inside vertical parent. The child's available inline-size
                            // (width) comes from parent's block-size (physical width).
                            childWidth = DimensionResolver.ResolveWidth(
                                childStyle, parentBlockSize, childBox, containingWidth);
                            childHeight = DimensionResolver.ResolveHeight(
                                childStyle, containingWidth, childBox);
                            bool autoHeight = float.IsNaN(childHeight);
                            if (autoHeight)
                            {
                                childHeight = 0;
                            }

                            childBox.ContentRect = new RectF(x, y, childWidth, childHeight);
                            LayoutChildren(childBox, context);

                            if (autoHeight)
                            {
                                childHeight = CalculateAutoHeight(childBox);
                            }
                        }
                        else
                        {
                            // [CSS-WRITING-MODES-3 §7.1] Same vertical writing mode.
                            // CSS width = block-size, CSS height = inline-size.
                            // Auto inline-size fills containing inline dimension.
                            float resolvedInlineSize = DimensionResolver.ResolveHeight(
                                childStyle, containingWidth, childBox);
                            if (float.IsNaN(resolvedInlineSize))
                            {
                                childHeight = containingWidth - BoxModelCalculator.GetVerticalSpacing(childBox);
                            }
                            else
                            {
                                childHeight = resolvedInlineSize;
                            }

                            // [CSS-WRITING-MODES-3 §7.1] Auto block-size (physical width)
                            // shrinks to content (like auto height in horizontal mode).
                            bool autoBlockSize = float.IsNaN(childStyle.Width)
                                || SizingKeyword.IsSizingKeyword(childStyle.Width);
                            if (!autoBlockSize)
                            {
                                childWidth = DimensionResolver.ResolveWidth(
                                    childStyle, parentBlockSize, childBox, containingWidth);
                            }
                            else
                            {
                                childWidth = 0;
                            }

                            childBox.ContentRect = new RectF(x, y, childWidth, childHeight);
                            LayoutChildren(childBox, context);

                            if (autoBlockSize)
                            {
                                childWidth = CalculateAutoWidth(childBox);
                            }
                        }

                        // [CSS2 §10.4] Apply min/max-width constraints
                        float minBlock = DimensionResolver.ResolvePercentWidth(
                            childStyle.MinWidth, parentBlockSize, childStyle, PropertyId.MinWidth);
                        float maxBlock = DimensionResolver.ResolvePercentWidth(
                            childStyle.MaxWidth, parentBlockSize, childStyle, PropertyId.MaxWidth);
                        if (childStyle.BoxSizing == CssBoxSizing.BorderBox)
                        {
                            float horizontalExtra = childBox.PaddingLeft + childBox.PaddingRight
                                + childBox.BorderLeftWidth + childBox.BorderRightWidth;
                            if (!float.IsNaN(minBlock) && minBlock >= 0)
                            {
                                minBlock = Math.Max(0, minBlock - horizontalExtra);
                            }
                            if (!float.IsNaN(maxBlock) && maxBlock >= 0)
                            {
                                maxBlock = Math.Max(0, maxBlock - horizontalExtra);
                            }
                        }
                        if (!float.IsNaN(maxBlock) && maxBlock >= 0 && childWidth > maxBlock)
                        {
                            childWidth = maxBlock;
                        }
                        if (!float.IsNaN(minBlock) && minBlock >= 0 && childWidth < minBlock)
                        {
                            childWidth = minBlock;
                        }

                        // Apply min/max-height constraints
                        float minInline = DimensionResolver.ResolvePercentHeight(
                            childStyle.MinHeight, containingWidth);
                        float maxInline = DimensionResolver.ResolvePercentHeight(
                            childStyle.MaxHeight, containingWidth);
                        if (childStyle.BoxSizing == CssBoxSizing.BorderBox)
                        {
                            float verticalExtra = childBox.PaddingTop + childBox.PaddingBottom
                                + childBox.BorderTopWidth + childBox.BorderBottomWidth;
                            if (!float.IsNaN(minInline) && minInline >= 0)
                            {
                                minInline = Math.Max(0, minInline - verticalExtra);
                            }
                            if (!float.IsNaN(maxInline) && maxInline >= 0)
                            {
                                maxInline = Math.Max(0, maxInline - verticalExtra);
                            }
                        }
                        if (!float.IsNaN(maxInline) && maxInline >= 0 && childHeight > maxInline)
                        {
                            childHeight = maxInline;
                        }
                        if (!float.IsNaN(minInline) && minInline >= 0 && childHeight < minInline)
                        {
                            childHeight = minInline;
                        }
                    }

                    childBox.ContentRect = new RectF(x, y, childWidth, childHeight);
                    parent.AddChild(childBox);

                    // Advance cursor in the block direction (horizontal)
                    cursorX = x + childWidth + childBox.PaddingRight + childBox.BorderRightWidth + childBox.MarginRight;
                    prevMarginBottom = 0;
                }
                else
                {
                    // Position the child.
                    // [CSS2 §10.3.3] In RTL containing blocks, over-constrained blocks
                    // (explicit width, no auto margins) align to the right edge.
                    // The containing block's direction determines this, not the element's.
                    float effectiveMarginLeft = childBox.MarginLeft;
                    var parentStyle = parent.StyledNode?.Style;
                    if (parentStyle != null && parentStyle.Direction == CssDirection.Rtl
                        && !float.IsNaN(childStyle.Width) && !SizingKeyword.IsSizingKeyword(childStyle.Width)
                        && !DeferredPercent.IsEncoded(childStyle.Width)
                        && !float.IsNaN(childStyle.MarginLeft)
                        && !float.IsNaN(childStyle.MarginRight))
                    {
                        float totalUsed = childBox.MarginLeft + childBox.BorderLeftWidth + childBox.PaddingLeft
                            + contentWidth + childBox.PaddingRight + childBox.BorderRightWidth + childBox.MarginRight;
                        float remaining = containingWidth - totalUsed;
                        if (remaining > 0)
                        {
                            effectiveMarginLeft = childBox.MarginLeft + remaining;
                        }
                    }
                    float x = parent.ContentRect.X + effectiveMarginLeft + childBox.BorderLeftWidth + childBox.PaddingLeft;
                    float y = cursorY + collapsedMargin + childBox.BorderTopWidth + childBox.PaddingTop;

                    // CSS 2.1 §9.5: The border box of an element that establishes a
                    // new block formatting context must not overlap the margin box of
                    // any floats in the same BFC. If it has a specified width that
                    // doesn't fit, move below. If auto width, shrink to fit beside.
                    if (EstablishesNewBfc(childStyle) && !IsBodyOverflowPropagated(childBox))
                    {
                        float borderBoxY = y - childBox.PaddingTop - childBox.BorderTopWidth;
                        // [CSS2 §9.5] Use actual element height for float edge queries,
                        // not hardcoded 1. This ensures tall BFC elements correctly
                        // detect float overlaps across their full height.
                        float queryHeight = 1;
                        float specH = childStyle.Height;
                        if (!float.IsNaN(specH) && specH > 0 && !DeferredPercent.IsEncoded(specH))
                        {
                            queryHeight = specH + childBox.PaddingTop + childBox.PaddingBottom
                                        + childBox.BorderTopWidth + childBox.BorderBottomWidth;
                        }
                        float floatLeftEdge = floatCtx.GetLeftEdge(borderBoxY, queryHeight);
                        float floatRightEdge = floatCtx.GetRightEdge(borderBoxY, queryHeight);
                        float normalBorderBoxX = parent.ContentRect.X + childBox.MarginLeft;
                        if (floatLeftEdge > normalBorderBoxX)
                        {
                            float availableWidth = floatRightEdge - floatLeftEdge;
                            float horizontalSpacing = childBox.BorderLeftWidth + childBox.PaddingLeft
                                + childBox.PaddingRight + childBox.BorderRightWidth;
                            bool hasSpecifiedWidth = !float.IsNaN(childStyle.Width)
                                && !SizingKeyword.IsSizingKeyword(childStyle.Width);
                            float marginBoxWidth = childBox.MarginLeft + horizontalSpacing
                                + contentWidth + childBox.MarginRight;
                            // [CSS2 §9.5] Push BFC element below floats when it doesn't fit.
                            // Also push below when floats overlap (availableWidth <= 0) —
                            // there's no gap for ANY element, even zero-width ones.
                            if (availableWidth <= 0
                                || (hasSpecifiedWidth && marginBoxWidth > 0 && marginBoxWidth > availableWidth))
                            {
                                // BFC float avoidance uses only LOCAL floats (not descendant clear Y)
                                float clearY = floatCtx.GetClearY(Css.CssClear.Both, includeDescendants: false);
                                if (clearY > borderBoxY)
                                {
                                    y = clearY + childBox.BorderTopWidth + childBox.PaddingTop;
                                    cursorY = clearY - collapsedMargin;
                                }
                            }
                            else
                            {
                                x = floatLeftEdge + childBox.BorderLeftWidth + childBox.PaddingLeft;
                                float shrunkContent = availableWidth - horizontalSpacing
                                    - childBox.MarginLeft - childBox.MarginRight;
                                if (shrunkContent < contentWidth && shrunkContent > 0)
                                {
                                    contentWidth = shrunkContent;
                                }
                            }
                        }
                    }

                    childBox.ContentRect = new RectF(x, y, contentWidth, 0);

                    float contentHeight;

                    if (isReplaced)
                    {
                        // Replaced element: resolve dimensions from intrinsic/attribute sizes
                        float intrinsicW = 0, intrinsicH = 0;
                        string? attrW = childElement.GetAttribute("width");
                        string? attrH = childElement.GetAttribute("height");
                        if (attrW != null && float.TryParse(attrW, out float aw)) intrinsicW = aw;
                        if (attrH != null && float.TryParse(attrH, out float ah)) intrinsicH = ah;
                        // Form controls: apply default intrinsic dimensions if no attributes set
                        if (ReplacedElementLayout.IsFormControl(childElement))
                        {
                            if (intrinsicW <= 0) intrinsicW = ReplacedElementLayout.GetFormControlIntrinsicWidth(childElement, context.TextMeasurer);
                            if (intrinsicH <= 0) intrinsicH = ReplacedElementLayout.GetFormControlIntrinsicHeight(childElement);
                        }
                        if (childElement.TagName == "math" && (intrinsicW <= 0 || intrinsicH <= 0))
                        {
                            var mathSize = Rendering.Internal.MathmlRenderer.MeasureElement(childElement.Element, 16f, context.TextMeasurer);
                            if (intrinsicW <= 0) intrinsicW = mathSize.Width + 4f;
                            if (intrinsicH <= 0) intrinsicH = mathSize.Height;
                        }
                        // Fallback: extract dimensions from data: URI for images
                        if ((intrinsicW <= 0 || intrinsicH <= 0) &&
                            ReplacedElementLayout.TryGetDataUriDimensions(childElement, out float duW, out float duH))
                        {
                            if (intrinsicW <= 0) intrinsicW = duW;
                            if (intrinsicH <= 0) intrinsicH = duH;
                        }
                        // [CSS-IMAGES-3 §2.2] SVG with viewBox but no width/height attrs:
                        // use viewBox dimensions as intrinsic size for ratio calculation.
                        if ((intrinsicW <= 0 || intrinsicH <= 0) && childElement.TagName == "svg")
                        {
                            string? viewBox = childElement.GetAttribute("viewbox");
                            if (viewBox != null)
                            {
                                var vbParts = viewBox.Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                                if (vbParts.Length >= 4
                                    && float.TryParse(vbParts[2], System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture, out float vbW)
                                    && float.TryParse(vbParts[3], System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture, out float vbH))
                                {
                                    if (intrinsicW <= 0) intrinsicW = vbW;
                                    if (intrinsicH <= 0) intrinsicH = vbH;
                                }
                            }
                        }
                        // [CSS-SIZING-4 §3] contain:size overrides intrinsic dimensions
                        var containValue = childStyle.Contain;
                        if (containValue == CssContain.Size || containValue == CssContain.Strict)
                        {
                            float ciW = childStyle.GetValues()[PropertyId.ContainIntrinsicWidth].FloatValue;
                            float ciH = childStyle.GetValues()[PropertyId.ContainIntrinsicHeight].FloatValue;
                            if (!float.IsNaN(ciW) && ciW > 0) { intrinsicW = ciW; }
                            if (!float.IsNaN(ciH) && ciH > 0) { intrinsicH = ciH; }
                        }
                        ReplacedElementLayout.ResolveDimensions(childBox, childStyle, containingWidth, parentContentHeight, intrinsicW, intrinsicH);
                        contentWidth = childBox.ContentRect.Width;
                        contentHeight = childBox.ContentRect.Height;
                    }
                    else
                    {
                        // Pre-resolve the child's height if definite so nested percentage children
                        // can resolve against it during LayoutChildren.
                        float preHeight = DimensionResolver.ResolveHeight(childStyle, parentContentHeight, childBox);
                        if (!float.IsNaN(preHeight) && preHeight > 0)
                            childBox.ContentRect = new RectF(childBox.ContentRect.X, y, contentWidth, preHeight);

                        // Layout children recursively
                        float marginTopBefore = childBox.MarginTop;
                        LayoutChildren(childBox, context);

                        // If LayoutChildren updated MarginTop (first-child collapsing),
                        // recompute position with the new margin.
                        if (childBox.MarginTop != marginTopBefore)
                        {
                            float newMarginTop = childBox.MarginTop;
                            float newCollapsedMargin;
                            if (wasFirstInFlow && MarginCollapsing.ShouldCollapseWithFirstChild(parent))
                            {
                                newCollapsedMargin = MarginCollapsing.Collapse(parent.MarginTop, newMarginTop);
                                parent.MarginTop = newCollapsedMargin;
                                newCollapsedMargin = 0;
                            }
                            else
                            {
                                newCollapsedMargin = MarginCollapsing.Collapse(prevMarginBottom, newMarginTop);
                            }
                            y = cursorY + newCollapsedMargin + childBox.BorderTopWidth + childBox.PaddingTop;
                            // Shift all children by the delta
                            float deltaY = y - childBox.ContentRect.Y;
                            if (Math.Abs(deltaY) > 0.01f)
                            {
                                childBox.ContentRect = new RectF(x, y, contentWidth, 0);
                                ShiftDescendants(childBox, deltaY);
                            }
                        }

                        // Resolve content height
                        contentHeight = DimensionResolver.ResolveHeight(childStyle, parentContentHeight, childBox);

                        // [CSS-SIZING-4 §5.1] When aspect-ratio gives a definite height but
                        // the element has auto height, use max(ratio-height, content-height)
                        // so that content can push the element taller than the ratio suggests.
                        // Then apply max-height/min-height constraints.
                        if (!float.IsNaN(contentHeight) && float.IsNaN(childStyle.Height)
                            && DimensionResolver.GetAspectRatio(childStyle) > 0
                            && childStyle.OverflowY == CssOverflow.Visible)
                        {
                            float autoH = CalculateAutoHeight(childBox);
                            if (autoH > contentHeight)
                            {
                                contentHeight = autoH;
                            }
                            // Apply max-height/min-height to the resolved height
                            float arMaxH = DimensionResolver.ResolvePercentHeight(childStyle.MaxHeight, parentContentHeight);
                            float arMinH = DimensionResolver.ResolvePercentHeight(childStyle.MinHeight, parentContentHeight);
                            if (!float.IsNaN(arMaxH) && arMaxH >= 0 && contentHeight > arMaxH)
                            {
                                contentHeight = arMaxH;
                            }
                            if (!float.IsNaN(arMinH) && arMinH >= 0 && contentHeight < arMinH)
                            {
                                contentHeight = arMinH;
                            }
                        }

                        if (float.IsNaN(contentHeight))
                        {
                            // contain: size or contain: strict → use contain-intrinsic-height or 0
                            var contain = childStyle.Contain;
                            if (contain == CssContain.Size || contain == CssContain.Strict)
                            {
                                float ciHeight = childStyle.GetValues()[PropertyId.ContainIntrinsicHeight].FloatValue;
                                contentHeight = (!float.IsNaN(ciHeight) && ciHeight > 0) ? ciHeight : 0;
                            }
                            else if ((childStyle.Display == CssDisplay.Table
                                     || childStyle.Display == CssDisplay.Grid
                                     || childStyle.Display == CssDisplay.InlineGrid)
                                    && childBox.ContentRect.Height > 0)
                                contentHeight = childBox.ContentRect.Height;
                            else
                                contentHeight = CalculateAutoHeight(childBox);

                            // Apply min-height / max-height to auto height
                            float minH = DimensionResolver.ResolvePercentHeight(childStyle.MinHeight, parentContentHeight);
                            float maxH = DimensionResolver.ResolvePercentHeight(childStyle.MaxHeight, parentContentHeight);
                            // box-sizing: border-box → min/max-height includes padding+border
                            if (childStyle.BoxSizing == CssBoxSizing.BorderBox)
                            {
                                float vExtra = childBox.PaddingTop + childBox.PaddingBottom
                                             + childBox.BorderTopWidth + childBox.BorderBottomWidth;
                                if (!float.IsNaN(minH) && minH >= 0)
                                {
                                    minH = Math.Max(0, minH - vExtra);
                                }
                                if (!float.IsNaN(maxH) && maxH >= 0)
                                {
                                    maxH = Math.Max(0, maxH - vExtra);
                                }
                            }
                            if (!float.IsNaN(maxH) && maxH >= 0 && contentHeight > maxH)
                                contentHeight = maxH;
                            if (!float.IsNaN(minH) && minH >= 0 && contentHeight < minH)
                                contentHeight = minH;
                        }
                    }


                    // For auto-width tables, LayoutChildren (TableLayout) may shrink-wrap
                    // the content rect. Preserve that width instead of overwriting.
                    // [CSS-TABLES §17.5.2] Empty tables use 0 content width (just padding/border).
                    float finalWidth = contentWidth;
                    if (childStyle.Display == CssDisplay.Table && float.IsNaN(childStyle.Width))
                    {
                        if (childBox.ContentRect.Width < contentWidth)
                        {
                            finalWidth = childBox.ContentRect.Width;
                        }
                        else if (childBox.Children.Count == 0)
                        {
                            finalWidth = 0;
                        }
                    }
                    childBox.ContentRect = new RectF(x, y, finalWidth, contentHeight);

                    // Margin collapse through: empty elements (no height, padding, or border)
                    // have their top and bottom margins collapse into a single margin.
                    // CSS 2.1 §8.3.1: the element takes no space; its combined margin
                    // participates in adjacent collapsing.
                    if (contentHeight == 0
                        && childBox.PaddingTop == 0 && childBox.PaddingBottom == 0
                        && childBox.BorderTopWidth == 0 && childBox.BorderBottomWidth == 0
                        && !isReplaced
                        && childBox.Children.Count == 0)
                    {
                        // Collapse the element's bottom margin with the already-collapsed top margin
                        float throughMargin = MarginCollapsing.Collapse(collapsedMargin, childBox.MarginBottom);
                        // This combined margin becomes the prevMarginBottom for the next sibling
                        prevMarginBottom = throughMargin;
                        // Don't advance cursorY — the element is effectively invisible
                        parent.AddChild(childBox);
                        continue;
                    }

                    // <legend> inside <fieldset>: position at fieldset border-box top
                    if (isFieldset && !legendHandled && childElement.TagName == "legend")
                    {
                        legendHandled = true;

                        // WHATWG rendering: legend uses shrink-to-fit width (like fit-content),
                        // not the full fieldset content width. Measure from line boxes or children.
                        float legendFitWidth = contentWidth;
                        if (float.IsNaN(childStyle.Width))
                        {
                            // auto width → shrink-to-fit
                            float maxLineW = GetContentExtent(childBox);
                            if (maxLineW > 0)
                            {
                                legendFitWidth = Math.Min(maxLineW, contentWidth);
                            }
                        }
                        contentWidth = legendFitWidth;

                        float legendBorderBoxH = childBox.BorderTopWidth + childBox.PaddingTop
                            + contentHeight + childBox.PaddingBottom + childBox.BorderBottomWidth;
                        // Chrome positions the legend's top edge at the fieldset's border-box top
                        float legendContentY = parent.BorderRect.Top + childBox.BorderTopWidth + childBox.PaddingTop;
                        float deltaY = legendContentY - childBox.ContentRect.Y;
                        childBox.ContentRect = new RectF(x, legendContentY, contentWidth, contentHeight);
                        ShiftDescendants(childBox, deltaY);
                        parent.AddChild(childBox);
                        // Content after legend starts at max(legendBottom, borderInnerTop) + paddingTop
                        float legendBottom = parent.BorderRect.Top + legendBorderBoxH;
                        float borderInnerTop = parent.BorderRect.Top + parent.BorderTopWidth;
                        cursorY = Math.Max(legendBottom, borderInnerTop) + parent.PaddingTop;
                        prevMarginBottom = 0;
                        continue;
                    }

                    parent.AddChild(childBox);

                    cursorY = childBox.ContentRect.Y + contentHeight + childBox.PaddingBottom + childBox.BorderBottomWidth;
                    prevMarginBottom = childBox.MarginBottom;
                }
            }

            // Handle parent-last-child margin collapsing
            // CSS 2.1 §8.3.1: When the parent has no bottom padding/border and auto height,
            // the last child's bottom margin collapses with the parent's bottom margin.
            // The collapsed margin does NOT contribute to the parent's auto height.
            if (effectiveChildren.Count > 0 && MarginCollapsing.ShouldCollapseWithLastChild(parent))
            {
                parent.MarginBottom = MarginCollapsing.Collapse(parent.MarginBottom, prevMarginBottom);
                // Zero the last child's MarginBottom so CalculateAutoHeight excludes it.
                // The margin is now represented on the parent instead.
                if (parent.Children.Count > 0)
                {
                    var lastChild = parent.Children[parent.Children.Count - 1];
                    lastChild.MarginBottom = 0;
                }
            }

            // [CSS2 §9.5] Propagate clear Y from non-BFC blocks to parent.
            // Only affects GetClearY (for clear:both), not GetLeftEdge/GetRightEdge
            // (for BFC float avoidance). This avoids the knock-on effects of full
            // float propagation while still allowing clear to work across siblings.
            if (prevFloatCtx != null && floatCtx.HasFloats
                && !EstablishesNewBfc(styledElement.Style)
                && styledElement.Style.Float == CssFloat.None
                && styledElement.TagName != "html"
                && styledElement.TagName != "body")
            {
                prevFloatCtx.PropagateClearY(floatCtx);
            }

            // Restore previous float context and containing block height
            context.FloatContext = prevFloatCtx;
            context.ContainingBlockHeight = savedContainingBlockHeight;
        }

        internal static StyledElement ConvertPseudoToElement(StyledPseudoElement pseudo,
            StyledElement ownerElement)
        {
            var doc = ownerElement.Element.OwnerDocument;
            var pseudoEl = doc!.CreateElement("div");
            var pseudoChildren = new List<StyledNode>();
            if (!string.IsNullOrEmpty(pseudo.Content))
            {
                pseudoChildren.Add(new StyledText(pseudo.Content, pseudo.Style));
            }
            return new StyledElement(pseudoEl, pseudo.Style, pseudoChildren);
        }

        internal static LayoutBox CreateLayoutBox(StyledElement element)
        {
            var display = element.Style.Display;
            BoxType boxType;

            switch (display)
            {
                case CssDisplay.Flex:
                case CssDisplay.InlineFlex:
                    boxType = BoxType.Flex;
                    break;
                case CssDisplay.Grid:
                case CssDisplay.InlineGrid:
                    boxType = BoxType.Grid;
                    break;
                case CssDisplay.Table:
                    boxType = BoxType.Table;
                    break;
                case CssDisplay.TableRow:
                    boxType = BoxType.TableRow;
                    break;
                case CssDisplay.TableCell:
                    boxType = BoxType.TableCell;
                    break;
                case CssDisplay.TableCaption:
                    boxType = BoxType.TableCaption;
                    break;
                case CssDisplay.InlineBlock:
                    boxType = BoxType.InlineBlock;
                    break;
                case CssDisplay.ListItem:
                    boxType = BoxType.ListItem;
                    break;
                case CssDisplay.Inline:
                case CssDisplay.Ruby:
                case CssDisplay.RubyText:
                case CssDisplay.RubyBase:
                case CssDisplay.RubyTextContainer:
                    boxType = BoxType.Inline;
                    break;
                default:
                    boxType = BoxType.Block;
                    break;
            }

            return new LayoutBox(element, boxType);
        }

        internal static void LayoutChildren(LayoutBox box, LayoutContext context)
        {
            var styledElement = box.StyledNode as StyledElement;
            if (styledElement == null || styledElement.Children.Count == 0) return;

            var display = styledElement.Style.Display;

            switch (display)
            {
                case CssDisplay.Flex:
                case CssDisplay.InlineFlex:
                    FlexLayout.Layout(box, context);
                    break;
                case CssDisplay.Grid:
                case CssDisplay.InlineGrid:
                    GridLayout.Layout(box, context);
                    break;
                case CssDisplay.Table:
                    TableLayout.Layout(box, context);
                    // [CSS-TABLES §4] If table layout produced no children (no table
                    // rows/cells found — e.g., div with display:table and non-table children),
                    // fall back to block layout so content is still rendered.
                    // Only apply when no child has table-related display.
                    if (box.Children.Count == 0 && styledElement.Children.Count > 0
                        && !HasTableChildren(styledElement)
                        && styledElement.TagName != "table")
                    {
                        if (HasBlockChildren(styledElement))
                        {
                            Layout(box, context);
                        }
                        else
                        {
                            // [CSS2 §9.5] Scope float context to this box's content rect.
                            var prevFloatCtx = context.FloatContext;
                            context.FloatContext = new FloatContext(box.ContentRect.X, box.ContentRect.Width);
                            var savedCbh = context.ContainingBlockHeight;
                            context.ContainingBlockHeight = ComputeBoxContainingHeight(box, styledElement);
                            InlineFormattingContext.Layout(box, context);
                            context.FloatContext = prevFloatCtx;
                            context.ContainingBlockHeight = savedCbh;
                        }
                    }
                    break;
                default:
                    // Check for multi-column layout
                    float colCount = styledElement.Style.ColumnCount;
                    float colWidth = styledElement.Style.ColumnWidth;
                    bool isMultiColumn = (!float.IsNaN(colCount) && colCount > 1) ||
                                         (!float.IsNaN(colWidth) && colWidth > 0);

                    if (isMultiColumn)
                    {
                        MultiColumnLayout.Layout(box, context);
                    }
                    else if (HasBlockChildren(styledElement))
                    {
                        Layout(box, context);
                    }
                    else
                    {
                        // [CSS2 §9.5] When the container has only inline-level content
                        // (possibly including floats), we still need a float context scoped
                        // to this container's content rect so floated children are placed
                        // relative to this element's edges (not the inherited outer BFC).
                        // Mirrors the FloatContext creation in Layout() above.
                        var prevFloatCtx = context.FloatContext;
                        context.FloatContext = new FloatContext(box.ContentRect.X, box.ContentRect.Width);
                        // [CSS-SIZING-3 §5.4] Scope containing block height to this box.
                        // If this box has a definite height, children resolve percentages
                        // against it; otherwise height is indefinite (NaN).
                        var savedCbh = context.ContainingBlockHeight;
                        context.ContainingBlockHeight = ComputeBoxContainingHeight(box, styledElement);
                        InlineFormattingContext.Layout(box, context);
                        context.FloatContext = prevFloatCtx;
                        context.ContainingBlockHeight = savedCbh;
                    }
                    break;
            }
        }

        /// <summary>
        /// [CSS-SIZING-3 §5.4] Compute the definite containing block height for a box.
        /// Returns the box's content height if it has a definite CSS height or is a
        /// stretched flex/grid item; otherwise returns NaN (indefinite).
        /// </summary>
        private static float ComputeBoxContainingHeight(LayoutBox box, StyledElement element)
        {
            float cssHeight = element.Style.Height;
            if (!float.IsNaN(cssHeight) && cssHeight > 0)
            {
                return cssHeight;
            }
            if (DeferredPercent.IsEncoded(cssHeight))
            {
                // Percentage height that was already resolved on the box
                if (box.ContentRect.Height > 0)
                {
                    return box.ContentRect.Height;
                }
                return float.NaN;
            }
            if (box.HasDefiniteCrossSize && box.ContentRect.Height > 0)
            {
                return box.ContentRect.Height;
            }
            return float.NaN;
        }

        private static bool HasTableChildren(StyledElement element)
        {
            for (int i = 0; i < element.Children.Count; i++)
            {
                if (element.Children[i] is StyledElement child)
                {
                    var d = child.Style.Display;
                    if (d == CssDisplay.TableRow || d == CssDisplay.TableRowGroup ||
                        d == CssDisplay.TableHeaderGroup || d == CssDisplay.TableFooterGroup ||
                        d == CssDisplay.TableCell || d == CssDisplay.TableCaption ||
                        d == CssDisplay.TableColumn || d == CssDisplay.TableColumnGroup)
                    {
                        return true;
                    }
                    // Also check for actual <tr>, <td>, <th>, <caption> HTML elements
                    string tag = child.TagName;
                    if (tag == "tr" || tag == "td" || tag == "th" || tag == "caption" ||
                        tag == "thead" || tag == "tbody" || tag == "tfoot" ||
                        tag == "col" || tag == "colgroup")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool HasBlockChildren(StyledElement element)
        {
            for (int i = 0; i < element.Children.Count; i++)
            {
                var child = element.Children[i];
                if (child.IsText)
                {
                    continue;
                }
                // [CSS2 §12.1] Pseudo-elements with block-level display count as block children.
                if (child is StyledPseudoElement pseudoChild)
                {
                    var pd = pseudoChild.Style.Display;
                    if (pd == CssDisplay.Block || pd == CssDisplay.Flex || pd == CssDisplay.InlineFlex
                        || pd == CssDisplay.Grid || pd == CssDisplay.InlineGrid || pd == CssDisplay.Table)
                    {
                        return true;
                    }
                    continue;
                }
                var childElement = (StyledElement)child;
                var childStyle = childElement.Style;
                // [CSS2 §9.5] Floated elements are out of flow and do not affect whether
                // the container establishes a block formatting context. Only in-flow
                // block-level children force BFC vs IFC. Abspos children still count here
                // because IFC has no generic abspos handling path; BFC picks them up instead.
                if (childStyle.Float != CssFloat.None)
                {
                    continue;
                }
                var display = childStyle.Display;
                // display:contents — look through its children
                if (display == CssDisplay.Contents)
                {
                    if (HasBlockChildren(childElement))
                    {
                        return true;
                    }
                    continue;
                }
                if (display == CssDisplay.Block || display == CssDisplay.FlowRoot ||
                    display == CssDisplay.Flex ||
                    display == CssDisplay.Grid ||
                    display == CssDisplay.Table ||
                    display == CssDisplay.ListItem)
                {
                    return true;
                }
                // [CSS2 §9.2.1.1] An inline element with block-level descendants
                // triggers block-in-inline splitting — the parent must use BFC.
                if (display == CssDisplay.Inline && HasBlockChildren(childElement))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// CSS 2.1 §9.5: Determines whether a style establishes a new block
        /// formatting context. Such elements must not overlap floats.
        /// </summary>
        private static bool EstablishesNewBfc(ComputedStyle style)
        {
            if (style.Display == CssDisplay.Flex || style.Display == CssDisplay.InlineFlex ||
                style.Display == CssDisplay.Grid || style.Display == CssDisplay.InlineGrid ||
                style.Display == CssDisplay.Table ||
                style.Display == CssDisplay.InlineBlock ||
                style.Display == CssDisplay.FlowRoot)
            {
                return true;
            }
            // [CSS-OVERFLOW-3 §3.1] overflow: hidden/scroll/auto establish a BFC.
            // overflow: clip does NOT establish a BFC — it clips visually but
            // doesn't affect layout (no float avoidance, no margin isolation).
            if ((style.OverflowX != CssOverflow.Visible && style.OverflowX != CssOverflow.Clip) ||
                (style.OverflowY != CssOverflow.Visible && style.OverflowY != CssOverflow.Clip))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// [CSS2 §11.1.1] Returns true if the layout box is a body element whose
        /// overflow propagates to the viewport (html parent has overflow:visible).
        /// </summary>
        internal static bool IsBodyOverflowPropagated(LayoutBox box)
        {
            if (box.StyledNode is Style.StyledElement bodyElem
                && bodyElem.TagName == "body"
                && box.Parent?.StyledNode is Style.StyledElement htmlElem
                && htmlElem.TagName == "html"
                && htmlElem.Style.OverflowX == CssOverflow.Visible
                && htmlElem.Style.OverflowY == CssOverflow.Visible)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// [CSS2 §10.6.7] Calculates auto height from children, line boxes, and floats.
        /// For elements establishing a BFC, the height includes any floated descendants
        /// whose bottom margin edge extends below the last child.
        /// </summary>
        internal static float CalculateAutoHeight(LayoutBox box)
        {
            float bottom = box.ContentRect.Y;

            // Check line boxes (from InlineFormattingContext)
            if (box.LineBoxes != null && box.LineBoxes.Count > 0)
            {
                var lastLine = box.LineBoxes[box.LineBoxes.Count - 1];
                float lineBottom = lastLine.Y + lastLine.Height;
                if (lineBottom > bottom)
                {
                    bottom = lineBottom;
                }
            }

            // Check children (from BlockFormattingContext)
            // [CSS2 §10.6.3] Abspos/fixed children don't contribute to auto height.
            // [CSS2 §10.6.7] Floated children only contribute for BFC-establishing elements.
            // [CSS2 §10.6.7] Floated children only contribute to auto height for BFC roots.
            // Check the box itself — if it establishes a new BFC, it contains floats.
            // Root boxes (html/body) implicitly establish BFC.
            bool boxEstablishesBfc = false;
            if (box.StyledNode is Style.StyledElement boxStyled)
            {
                boxEstablishesBfc = EstablishesNewBfc(boxStyled.Style)
                    || boxStyled.TagName == "html" || boxStyled.TagName == "body";
            }
            else
            {
                boxEstablishesBfc = true; // anonymous boxes → BFC
            }
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                var childStyled = child.StyledNode as Style.StyledElement;

                if (childStyled != null &&
                    (childStyled.Style.Position == Css.CssPosition.Absolute ||
                     childStyled.Style.Position == Css.CssPosition.Fixed))
                {
                    continue;
                }

                if (childStyled != null && childStyled.Style.Float != Css.CssFloat.None
                    && !boxEstablishesBfc)
                {
                    continue;
                }

                float childBottom = child.ContentRect.Y + child.ContentRect.Height
                                  + child.PaddingBottom + child.BorderBottomWidth + child.MarginBottom;
                if (childBottom > bottom)
                {
                    bottom = childBottom;
                }
            }

            return bottom - box.ContentRect.Y;
        }

        /// <summary>
        /// [CSS-WRITING-MODES-3 §7.1] Calculate auto block-size (physical width) for
        /// vertical writing mode containers. Finds the rightmost content edge of
        /// in-flow children, analogous to CalculateAutoHeight for horizontal mode.
        /// </summary>
        internal static float CalculateAutoWidth(LayoutBox box)
        {
            float right = box.ContentRect.X;

            if (box.LineBoxes != null && box.LineBoxes.Count > 0)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    var lineBox = box.LineBoxes[i];
                    float lineRight = lineBox.X + lineBox.Width;
                    if (lineRight > right)
                    {
                        right = lineRight;
                    }
                }
            }

            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                var childStyled = child.StyledNode as Style.StyledElement;

                if (childStyled != null &&
                    (childStyled.Style.Position == Css.CssPosition.Absolute ||
                     childStyled.Style.Position == Css.CssPosition.Fixed))
                {
                    continue;
                }

                float childRight = child.ContentRect.X + child.ContentRect.Width
                                 + child.PaddingRight + child.BorderRightWidth + child.MarginRight;
                if (childRight > right)
                {
                    right = childRight;
                }
            }

            return right - box.ContentRect.X;
        }

        /// <summary>
        /// Returns true if a CSS display value is an inline-level block container
        /// (inline-block, inline-flex, inline-grid). These need anonymous block wrapping
        /// when they appear among block-level siblings so they get shrink-to-fit sizing
        /// via InlineFormattingContext. Plain display:inline elements are NOT included
        /// because they don't establish their own BFC.
        /// </summary>
        private static bool IsInlineLevelBlockDisplay(CssDisplay display)
        {
            return display == CssDisplay.InlineBlock ||
                   display == CssDisplay.InlineFlex ||
                   display == CssDisplay.InlineGrid;
        }

        /// <summary>
        /// [CSS2 §10.3.7] Computes the static-position content-box X for an inline-level
        /// absolutely positioned element. The hypothetical inline box is placed respecting
        /// float exclusions and text-align. In LTR, the left margin edge is positioned;
        /// in RTL, the right margin edge is positioned.
        /// </summary>
        private static float ComputeInlineStaticX(StyledElement parent, FloatContext floatCtx,
            float cursorY, float prevMarginBottom, LayoutBox posBox, float posWidth)
        {
            float hypotheticalY = cursorY + MarginCollapsing.Collapse(prevMarginBottom, posBox.MarginTop);
            float leftEdge = floatCtx.GetLeftEdge(hypotheticalY, 0);
            float rightEdge = floatCtx.GetRightEdge(hypotheticalY, 0);

            var parentTextAlign = parent.Style.TextAlign;
            var parentDirection = parent.Style.Direction;

            // [CSS2 §10.3.7] Resolve start/end/justify to physical left/right
            CssTextAlign resolved = parentTextAlign;
            if (resolved == CssTextAlign.Start || resolved == CssTextAlign.Justify)
            {
                resolved = parentDirection == CssDirection.Rtl
                    ? CssTextAlign.Right : CssTextAlign.Left;
            }
            else if (resolved == CssTextAlign.End)
            {
                resolved = parentDirection == CssDirection.Rtl
                    ? CssTextAlign.Left : CssTextAlign.Right;
            }

            // Position a zero-width hypothetical inline within the available area
            float hypotheticalEdge;
            switch (resolved)
            {
                case CssTextAlign.Center:
                    hypotheticalEdge = (leftEdge + rightEdge) / 2;
                    break;
                case CssTextAlign.Right:
                    hypotheticalEdge = rightEdge;
                    break;
                default:
                    hypotheticalEdge = leftEdge;
                    break;
            }

            // [CSS2 §10.3.7/§10.3.8] In LTR the static position is the left margin edge;
            // in RTL the static position is the right margin edge. Convert to content-box X.
            if (parentDirection == CssDirection.Rtl)
            {
                return hypotheticalEdge - posBox.MarginRight - posBox.BorderRightWidth
                     - posBox.PaddingRight - posWidth;
            }

            return hypotheticalEdge + posBox.MarginLeft + posBox.BorderLeftWidth + posBox.PaddingLeft;
        }

        /// <summary>
        /// [CSS2 §9.2.1.1] Returns true if an element that appears among block-level
        /// siblings in a block formatting context should be wrapped in an anonymous
        /// block and laid out via InlineFormattingContext. Covers:
        /// (a) inline-block, inline-flex, inline-grid (always wrap — shrink-to-fit),
        /// including inline-level replaced form controls like &lt;input&gt;;
        /// (b) plain display:inline non-replaced elements with no block-level DOM
        /// descendants (typical case is display:contents flattening bringing an
        /// inline span to a BFC). Inline elements that contain block-level
        /// descendants need the block layout path so HTML5 IB-split semantics can
        /// run recursively. Replaced elements with the UA default display:inline
        /// (e.g. &lt;svg&gt;) are excluded from the wrapping path so their intrinsic
        /// sizing (attribute-based width/height, viewBox aspect ratio) is handled
        /// by the block-layout replaced element code instead of IFC.
        /// </summary>
        private static bool ShouldWrapInlineElementInAnonBlock(StyledElement element)
        {
            var display = element.Style.Display;
            if (IsInlineLevelBlockDisplay(display))
            {
                return true;
            }
            if (display != CssDisplay.Inline)
            {
                return false;
            }
            if (ReplacedElementLayout.IsReplaced(element))
            {
                return false;
            }
            return !HasBlockChildren(element);
        }

        /// <summary>
        /// [CSS2 §9.2.1.1] Starting after the item already at <paramref name="index"/>,
        /// walks forward and adds each consecutive inline-level sibling (text nodes,
        /// inline pseudo-elements, non-replaced inline-level elements) to
        /// <paramref name="run"/>. Advances <paramref name="index"/> past every item
        /// absorbed. Block-level, floated, out-of-flow, or inline elements with
        /// block-level descendants terminate the run. The caller is responsible for
        /// seeding <paramref name="run"/> with the item at the starting index.
        /// </summary>
        private static void CollectAdjacentInlineRun(IReadOnlyList<StyledNode> effectiveChildren,
            ref int index, List<StyledNode> run)
        {
            while (index + 1 < effectiveChildren.Count)
            {
                var next = effectiveChildren[index + 1];
                if (next.IsText)
                {
                    run.Add(next);
                    index++;
                    continue;
                }
                if (next is StyledPseudoElement nextPseudo)
                {
                    var pd = nextPseudo.Style.Display;
                    if (pd == CssDisplay.Block || pd == CssDisplay.Flex || pd == CssDisplay.InlineFlex
                        || pd == CssDisplay.Grid || pd == CssDisplay.InlineGrid || pd == CssDisplay.Table)
                    {
                        break;
                    }
                    run.Add(next);
                    index++;
                    continue;
                }
                if (next is StyledElement nextElement &&
                    ShouldWrapInlineElementInAnonBlock(nextElement) &&
                    nextElement.Style.Position != CssPosition.Absolute &&
                    nextElement.Style.Position != CssPosition.Fixed &&
                    nextElement.Style.Float == CssFloat.None &&
                    nextElement.Style.Display != CssDisplay.None)
                {
                    run.Add(next);
                    index++;
                    continue;
                }
                break;
            }
        }

        /// <summary>
        /// Creates an anonymous block box wrapping a run of inline-level content
        /// (text nodes, pseudo-elements, and inline-level elements) in a block
        /// formatting context with mixed block/inline children.
        /// </summary>
        private static LayoutBox CreateAnonymousBlockForInlineRun(List<StyledNode> inlineRun,
            StyledElement parentStyled, LayoutBox parent, float cursorY, float containingWidth,
            LayoutContext context)
        {
            var blockStyle = CloneStyleAsBlock(parentStyled.Style);
            var doc = parentStyled.Element.OwnerDocument;
            var anonElement = doc!.CreateElement("div");

            // Build child list: text nodes get the block style, elements keep their own style
            var anonChildren = new List<StyledNode>();
            for (int r = 0; r < inlineRun.Count; r++)
            {
                var node = inlineRun[r];
                if (node.IsText)
                {
                    anonChildren.Add(new StyledText(((StyledText)node).Text, blockStyle));
                }
                else if (node is StyledPseudoElement pseudo)
                {
                    anonChildren.Add(new StyledText(pseudo.Content, pseudo.Style));
                }
                else
                {
                    // Inline-level element (inline-block, inline-flex, etc.) — keep as-is
                    anonChildren.Add(node);
                }
            }

            var anonStyled = new StyledElement(anonElement, blockStyle, anonChildren);
            var box = new LayoutBox(anonStyled, BoxType.Block);
            box.IsAnonymousBlock = true;
            box.ContentRect = new RectF(parent.ContentRect.X, cursorY, containingWidth, 0);

            InlineFormattingContext.Layout(box, context);

            float height = CalculateAutoHeight(box);
            box.ContentRect = new RectF(box.ContentRect.X, box.ContentRect.Y, box.ContentRect.Width, height);

            return box;
        }

        /// <summary>
        /// Clone a computed style but override display to block.
        /// Prevents issues when anonymous text wrappers inherit non-block display.
        /// </summary>
        // BUG-062: Match FlexLayout.CloneStyleAsBlock — clear visual decoration on anonymous wrappers.
        private static ComputedStyle CloneStyleAsBlock(ComputedStyle source)
        {
            var values = (PropertyValue[])source.GetValues().Clone();
            values[PropertyId.Display] = PropertyValue.FromInt((int)CssDisplay.Block);
            var autoVal = PropertyValue.FromLength(float.NaN);
            values[PropertyId.Width] = autoVal;
            values[PropertyId.Height] = autoVal;

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
            values[PropertyId.PaddingTop] = zero;
            values[PropertyId.PaddingRight] = zero;
            values[PropertyId.PaddingBottom] = zero;
            values[PropertyId.PaddingLeft] = zero;
            return new ComputedStyle(values, refValues);
        }

        private static LayoutBox CreateInlineBox(StyledText textNode, LayoutContext context, float containingWidth, float cursorY, bool vertical = false)
        {
            var box = new LayoutText(textNode);
            box.ContentRect = new RectF(0, cursorY, containingWidth, 0);

            // Measure text
            float fontSize = textNode.Style.FontSize;
            float lineHeight = textNode.Style.LineHeight;
            bool isNormalLineHeight = float.IsNaN(lineHeight) || lineHeight == 0;
            // Negative = unitless multiplier, positive = pixels, NaN = normal
            if (lineHeight < 0)
                lineHeight = -lineHeight * fontSize;
            else if (isNormalLineHeight)
                lineHeight = fontSize * 1.2f;

            if (context.TextMeasurer != null)
            {
                var fontDesc = new Fonts.FontDescriptor(
                    textNode.Style.FontFamilies,
                    textNode.Style.FontWeight,
                    textNode.Style.FontStyle,
                    Fonts.FontDescriptor.StretchToPercentage(textNode.Style.FontStretch));

                // Use actual font metrics for "normal" line-height
                if (isNormalLineHeight)
                {
                    float metricsLineHeight = context.TextMeasurer.GetNormalLineHeight(fontDesc, fontSize);
                    if (!float.IsNaN(metricsLineHeight) && metricsLineHeight > 0)
                        lineHeight = metricsLineHeight;
                }

                string? fontFeatures = FontVariantFeatureMapper.BuildFeatureString(textNode.Style);
                var shaped = context.TextMeasurer.Shape(textNode.Text, fontDesc, fontSize, fontFeatures);
                box.ShapedRun = shaped;
                box.TextX = 0;
                box.TextY = cursorY + lineHeight;

                if (vertical)
                {
                    // In vertical mode, text runs top-to-bottom; the "width" of the
                    // anonymous inline box is one line height, and "height" is the
                    // measured text width (which becomes the inline extent).
                    box.ContentRect = new RectF(0, cursorY, lineHeight, Math.Min(shaped.TotalWidth, containingWidth));
                }
                else
                {
                    box.ContentRect = new RectF(0, cursorY, Math.Min(shaped.TotalWidth, containingWidth), lineHeight);
                }
            }
            else
            {
                // Fallback: estimate text width
                float estimatedWidth = textNode.Text.Length * fontSize * 0.6f;

                if (vertical)
                {
                    float numCols = (float)Math.Ceiling(estimatedWidth / containingWidth);
                    box.ContentRect = new RectF(0, cursorY, numCols * lineHeight, containingWidth);
                }
                else
                {
                    float numLines = (float)Math.Ceiling(estimatedWidth / containingWidth);
                    box.ContentRect = new RectF(0, cursorY, containingWidth, numLines * lineHeight);
                }
            }

            return box;
        }

        /// <summary>
        /// Measure the intrinsic width for min-content, max-content, or fit-content sizing.
        /// </summary>
        internal static float MeasureIntrinsicWidth(StyledElement element, float keyword,
                                                    float containingWidth, LayoutContext context)
        {
            // CSS Flexbox §9.9.1: Flex containers have dedicated intrinsic sizing that
            // sums (row) or maxes (column) their items' contributions, rather than using
            // the generic layout+measure approach which doesn't account for flex semantics.
            var display = element.Style.Display;
            if (display == CssDisplay.Flex || display == CssDisplay.InlineFlex)
            {
                if (keyword == SizingKeyword.FitContent)
                {
                    float minContentWidth = FlexLayout.ComputeIntrinsicWidth(element, SizingKeyword.MinContent, containingWidth, context);
                    float maxContentWidth = FlexLayout.ComputeIntrinsicWidth(element, SizingKeyword.MaxContent, containingWidth, context);
                    var fitBox = new LayoutBox(element, BoxType.Block);
                    BoxModelCalculator.ApplyBoxModel(fitBox, element.Style, containingWidth);
                    float available = containingWidth - BoxModelCalculator.GetHorizontalSpacing(fitBox);
                    return Math.Max(minContentWidth, Math.Min(maxContentWidth, available));
                }
                return FlexLayout.ComputeIntrinsicWidth(element, keyword, containingWidth, context);
            }

            // [CSS-SIZING-4 §3] Size containment: use contain-intrinsic-width
            // instead of measuring content for intrinsic sizing.
            // Must check BEFORE grid/flex intrinsic sizing — containment overrides all.
            var containVal = element.Style.Contain;
            if (containVal == CssContain.Size || containVal == CssContain.Strict)
            {
                float ciWidth = element.Style.GetValues()[PropertyId.ContainIntrinsicWidth].FloatValue;
                return (!float.IsNaN(ciWidth) && ciWidth > 0) ? ciWidth : 0;
            }

            // [CSS-GRID §12.1] Grid containers have dedicated intrinsic sizing that
            // sums per-column contributions, rather than using the generic layout+measure
            // approach which would expand auto tracks to fill available width.
            if (display == CssDisplay.Grid || display == CssDisplay.InlineGrid)
            {
                if (keyword == SizingKeyword.FitContent)
                {
                    float minContentWidth = GridLayout.ComputeIntrinsicWidth(element, SizingKeyword.MinContent, containingWidth, context);
                    float maxContentWidth = GridLayout.ComputeIntrinsicWidth(element, SizingKeyword.MaxContent, containingWidth, context);
                    var fitBox = new LayoutBox(element, BoxType.Block);
                    BoxModelCalculator.ApplyBoxModel(fitBox, element.Style, containingWidth);
                    float available = containingWidth - BoxModelCalculator.GetHorizontalSpacing(fitBox);
                    return Math.Max(minContentWidth, Math.Min(maxContentWidth, available));
                }
                return GridLayout.ComputeIntrinsicWidth(element, keyword, containingWidth, context);
            }

            // min-content: lay out with very narrow width to find the minimum
            // max-content: lay out with very wide width to find the maximum
            float measureWidth;
            if (keyword == SizingKeyword.MinContent)
                measureWidth = 1f;
            else if (keyword == SizingKeyword.MaxContent)
                measureWidth = 10000f;
            else // fit-content
                measureWidth = containingWidth;

            var box = new LayoutBox(element, BoxType.Block);
            BoxModelCalculator.ApplyBoxModel(box, element.Style, measureWidth);
            float contentWidth = measureWidth - box.PaddingLeft - box.PaddingRight
                               - box.BorderLeftWidth - box.BorderRightWidth;
            contentWidth = Math.Max(0, contentWidth);
            box.ContentRect = new RectF(0, 0, contentWidth, 0);
            LayoutChildren(box, context);

            // Measure actual content extent.
            // For block children with auto width, they fill the available space
            // but their actual content may be narrower. Use recursive measurement.
            float maxRight = GetContentExtent(box);

            // [CSS-TEXT-3 §4.1.2 / CSS-SIZING-3 §4] For max-content with pre-wrap,
            // trailing preserved spaces contribute to intrinsic max-content size.
            // FinalizeLineBox subtracts trailing whitespace for hanging (correct for
            // alignment and wrapping) but max-content needs it included.
            if (keyword == SizingKeyword.MaxContent)
            {
                maxRight = AddTrailingWhitespaceForMaxContent(box, maxRight);
            }

            // Return content width (not including parent's padding/border — the caller
            // uses this as ContentRect.Width which is the content area).
            float measured = maxRight;

            if (keyword == SizingKeyword.FitContent)
            {
                // fit-content = clamp(min-content, stretch, max-content)
                // fit-content(X) = clamp(min-content, X, max-content)
                float minW = MeasureIntrinsicWidth(element, SizingKeyword.MinContent, containingWidth, context);
                float maxW = MeasureIntrinsicWidth(element, SizingKeyword.MaxContent, containingWidth, context);
                float available = containingWidth - BoxModelCalculator.GetHorizontalSpacing(box);

                // [CSS-SIZING-3 §5.1] Check for fit-content(<length-percentage>) function argument
                var fitRef = element.Style.GetRefValue(PropertyId.Width);
                if (fitRef is CssFunctionValue fitFn && fitFn.Name == "fit-content" && fitFn.Arguments.Count >= 1)
                {
                    var arg = fitFn.Arguments[0];
                    if (arg is CssPercentageValue pct)
                    {
                        available = containingWidth * pct.Value / 100f;
                    }
                    else if (arg is CssDimensionValue dim)
                    {
                        available = dim.Value;
                    }
                }

                return Math.Max(minW, Math.Min(maxW, available));
            }

            // [CSS-SIZING-4 §5.1] If the element itself has aspect-ratio + definite height,
            // its intrinsic width is at least height * ratio (even with no children).
            float arRatio = DimensionResolver.GetAspectRatio(element.Style);
            if (arRatio > 0 && !float.IsNaN(element.Style.Height) && element.Style.Height > 0
                && !DeferredPercent.IsEncoded(element.Style.Height))
            {
                // [CSS2 §10.7] Clamp height by min/max before deriving width
                float arHeight = element.Style.Height;
                float arMaxH = DimensionResolver.ResolvePercentHeight(element.Style.MaxHeight, 0);
                float arMinH = DimensionResolver.ResolvePercentHeight(element.Style.MinHeight, 0);
                if (!float.IsNaN(arMaxH) && arMaxH >= 0 && arHeight > arMaxH) { arHeight = arMaxH; }
                if (!float.IsNaN(arMinH) && arMinH >= 0 && arHeight < arMinH) { arHeight = arMinH; }
                float arWidth = arHeight * arRatio;
                if (element.Style.BoxSizing == CssBoxSizing.BorderBox)
                {
                    arWidth -= box.PaddingLeft + box.PaddingRight
                             + box.BorderLeftWidth + box.BorderRightWidth;
                    if (arWidth < 0) { arWidth = 0; }
                }
                if (arWidth > measured)
                {
                    measured = arWidth;
                }
            }

            return measured;
        }

        /// <summary>
        /// Recursively measures the actual content extent of a laid-out box.
        /// For inline content, measures the widest line fragment extent.
        /// For block children with auto width, recursively measures their content
        /// instead of using the filled available width.
        /// Returns extent relative to box.ContentRect.X.
        /// </summary>
        private static bool IsGridOrFlexContainer(LayoutBox box)
        {
            if (box.StyledNode is StyledElement se)
            {
                var d = se.Style.Display;
                return d == CssDisplay.Grid || d == CssDisplay.InlineGrid
                    || d == CssDisplay.Flex || d == CssDisplay.InlineFlex;
            }
            return false;
        }

        internal static float GetContentExtent(LayoutBox box)
        {
            float maxExtent = 0;
            float contentLeft = box.ContentRect.X;

            // Inline content: use NaturalContentWidth which captures the
            // pre-alignment (pre-centering) extent of each line.
            if (box.LineBoxes != null)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    float lineWidth = box.LineBoxes[i].NaturalContentWidth;
                    if (lineWidth > maxExtent)
                    {
                        maxExtent = lineWidth;
                    }
                }
            }

            // Block children
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                var childStyle = child.StyledNode?.Style;
                // [CSS-SIZING-4 §5.1] Auto-width blocks with aspect-ratio + definite height
                // derive their width from the ratio — treat as fixed-width, not recursive.
                // Only for block-level children, not grid/flex items (those use grid/flex sizing).
                bool hasAspectRatioWidth = childStyle != null
                    && float.IsNaN(childStyle.Width)
                    && DimensionResolver.GetAspectRatio(childStyle) > 0
                    && !float.IsNaN(childStyle.Height) && childStyle.Height > 0
                    && !DeferredPercent.IsEncoded(childStyle.Height)
                    && !IsGridOrFlexContainer(box);
                // [CSS-SIZING-4 §3] contain:size uses contain-intrinsic-width for extent
                bool hasSizeContainment = childStyle != null
                    && (childStyle.Contain == CssContain.Size || childStyle.Contain == CssContain.Strict);
                bool isAutoWidthBlock = childStyle != null
                    && float.IsNaN(childStyle.Width)
                    && !SizingKeyword.IsSizingKeyword(childStyle.Width)
                    && !DeferredPercent.IsEncoded(childStyle.Width)
                    && !hasAspectRatioWidth
                    && !hasSizeContainment;

                float extent;
                if (hasSizeContainment)
                {
                    float ciW = childStyle!.GetValues()[PropertyId.ContainIntrinsicWidth].FloatValue;
                    extent = (!float.IsNaN(ciW) && ciW > 0) ? ciW : 0;
                    extent += (child.ContentRect.X - contentLeft)
                            + child.PaddingRight + child.BorderRightWidth + child.MarginRight;
                }
                else if (isAutoWidthBlock)
                {
                    // Auto-width block fills available space but actual content is narrower.
                    // Recursively measure the content extent.
                    float innerExtent = GetContentExtent(child);
                    extent = (child.ContentRect.X - contentLeft) + innerExtent
                           + child.PaddingRight + child.BorderRightWidth + child.MarginRight;
                }
                else
                {
                    extent = (child.ContentRect.X - contentLeft) + child.ContentRect.Width
                           + child.PaddingRight + child.BorderRightWidth + child.MarginRight;
                }

                if (extent > maxExtent)
                {
                    maxExtent = extent;
                }
            }

            return maxExtent;
        }

        /// <summary>
        /// [CSS-TEXT-3 §4.1.2] For max-content intrinsic sizing, trailing preserved
        /// whitespace in pre-wrap lines contributes to the line's max-content size.
        /// Walks the box tree and adds back any trailing whitespace that was subtracted
        /// during FinalizeLineBox for hanging purposes.
        /// </summary>
        private static float AddTrailingWhitespaceForMaxContent(LayoutBox box, float baseExtent)
        {
            float maxExtent = baseExtent;
            float contentLeft = box.ContentRect.X;

            if (box.LineBoxes != null)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    var line = box.LineBoxes[i];
                    float lineWidth = line.NaturalContentWidth + line.TrailingWhitespaceWidth;
                    if (lineWidth > maxExtent)
                    {
                        maxExtent = lineWidth;
                    }
                }
            }

            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                var childStyle = child.StyledNode?.Style;
                bool isAutoWidthBlock = childStyle != null
                    && float.IsNaN(childStyle.Width)
                    && !SizingKeyword.IsSizingKeyword(childStyle.Width)
                    && !DeferredPercent.IsEncoded(childStyle.Width);
                if (isAutoWidthBlock)
                {
                    float innerExtent = AddTrailingWhitespaceForMaxContent(child, GetContentExtent(child));
                    float extent = (child.ContentRect.X - contentLeft) + innerExtent
                                 + child.PaddingRight + child.BorderRightWidth + child.MarginRight;
                    if (extent > maxExtent)
                    {
                        maxExtent = extent;
                    }
                }
            }

            return maxExtent;
        }

        /// <summary>
        /// Shifts all descendant boxes and line boxes by a vertical delta.
        /// Used when repositioning a box (e.g. fieldset legend) after its children have been laid out.
        /// </summary>
        private static void ShiftDescendants(LayoutBox box, float deltaY)
        {
            // Shift line boxes (inline content)
            if (box.LineBoxes != null)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    box.LineBoxes[i].Y += deltaY;
                }
            }

            // Shift child layout boxes
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                var cr = child.ContentRect;
                child.ContentRect = new RectF(cr.X, cr.Y + deltaY, cr.Width, cr.Height);

                // [CSS-GRID §9] Keep GridAreaContainingBlock in sync with shifted coordinates.
                if (child.GridAreaContainingBlock != null)
                {
                    var ga = child.GridAreaContainingBlock.Value;
                    child.GridAreaContainingBlock = new RectF(ga.X, ga.Y + deltaY, ga.Width, ga.Height);
                }

                ShiftDescendants(child, deltaY);
            }
        }

        /// <summary>
        /// Flatten display:contents children recursively. Elements with display:contents
        /// are replaced by their children in the effective child list.
        /// </summary>
        internal static IReadOnlyList<StyledNode> FlattenContents(StyledElement element)
        {
            bool hasContents = false;
            for (int i = 0; i < element.Children.Count; i++)
            {
                var child = element.Children[i];
                if (!child.IsText && !(child is StyledPseudoElement) &&
                    child is StyledElement ce && ce.Style.Display == CssDisplay.Contents)
                {
                    hasContents = true;
                    break;
                }
            }

            if (!hasContents) return element.Children;

            var result = new List<StyledNode>();
            FlattenContentsRecursive(element, result);
            return result;
        }

        private static void FlattenContentsRecursive(StyledElement element, List<StyledNode> result)
        {
            for (int i = 0; i < element.Children.Count; i++)
            {
                var child = element.Children[i];
                if (!child.IsText && !(child is StyledPseudoElement) &&
                    child is StyledElement ce && ce.Style.Display == CssDisplay.Contents)
                {
                    FlattenContentsRecursive(ce, result);
                }
                else
                {
                    result.Add(child);
                }
            }
        }
    }
}
