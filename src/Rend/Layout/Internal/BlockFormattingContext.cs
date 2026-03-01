using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Style;

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
            float parentContentHeight = parent.ContentRect.Height;
            if (float.IsNaN(parentContentHeight) || parentContentHeight <= 0)
            {
                var parentStyled = parent.StyledNode as StyledElement;
                if (parentStyled != null)
                {
                    float h = parentStyled.Style.Height;
                    if (!float.IsNaN(h) && h > 0) parentContentHeight = h;
                }
            }

            var styledElement = parent.StyledNode as StyledElement;
            if (styledElement == null) return;

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

            bool foundSummary = false;

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
                    // Text in block context: create anonymous inline box
                    var textNode = (StyledText)child;
                    if (string.IsNullOrWhiteSpace(textNode.Text)) continue;

                    if (vertical)
                    {
                        var inlineBox = CreateInlineBox(textNode, context, containingWidth, cursorY, vertical);
                        inlineBox.ContentRect = new RectF(cursorX, parent.ContentRect.Y, 0, containingWidth);
                        parent.AddChild(inlineBox);
                        cursorX += inlineBox.ContentRect.Width > 0 ? inlineBox.ContentRect.Width : inlineBox.MarginRect.Width;
                    }
                    else
                    {
                        var inlineBox = CreateInlineBox(textNode, context, containingWidth, cursorY);
                        parent.AddChild(inlineBox);
                        cursorY = inlineBox.MarginRect.Bottom;
                    }
                    prevMarginBottom = 0;
                    continue;
                }

                if (child is StyledPseudoElement pseudo)
                {
                    // Pseudo-element: render as inline text with its own style
                    var pseudoText = new StyledText(pseudo.Content, pseudo.Style);
                    if (vertical)
                    {
                        var inlineBox = CreateInlineBox(pseudoText, context, containingWidth, cursorY, vertical);
                        inlineBox.ContentRect = new RectF(cursorX, parent.ContentRect.Y, 0, containingWidth);
                        parent.AddChild(inlineBox);
                        cursorX += inlineBox.ContentRect.Width > 0 ? inlineBox.ContentRect.Width : inlineBox.MarginRect.Width;
                    }
                    else
                    {
                        var inlineBox = CreateInlineBox(pseudoText, context, containingWidth, cursorY);
                        parent.AddChild(inlineBox);
                        cursorY = inlineBox.MarginRect.Bottom;
                    }
                    prevMarginBottom = 0;
                    continue;
                }

                var childElement = (StyledElement)child;
                var childStyle = childElement.Style;

                // Skip display:none
                if (childStyle.Display == CssDisplay.None) continue;

                // Absolutely/fixed positioned elements are out of normal flow.
                // Still create the box and add as child (for positioning later),
                // but don't advance cursorY or participate in margin collapsing.
                if (childStyle.Position == CssPosition.Absolute || childStyle.Position == CssPosition.Fixed)
                {
                    var posBox = CreateLayoutBox(childElement);
                    BoxModelCalculator.ApplyBoxModel(posBox, childStyle, containingWidth);
                    float posWidth = DimensionResolver.ResolveWidth(childStyle, containingWidth, posBox);
                    posBox.ContentRect = new RectF(parent.ContentRect.X, cursorY, posWidth, 0);
                    LayoutChildren(posBox, context);
                    float posHeight = DimensionResolver.ResolveHeight(childStyle, parentContentHeight, posBox);
                    if (float.IsNaN(posHeight)) posHeight = CalculateAutoHeight(posBox);
                    posBox.ContentRect = new RectF(posBox.ContentRect.X, posBox.ContentRect.Y, posWidth, posHeight);
                    parent.AddChild(posBox);
                    continue;
                }

                // Handle floated elements
                if (childStyle.Float != CssFloat.None)
                {
                    var floatBox = CreateLayoutBox(childElement);
                    floatCtx.CurrentY = cursorY;
                    FloatLayout.PlaceFloat(floatBox, floatCtx, parent, context);
                    parent.AddChild(floatBox);
                    continue;
                }

                // Apply clear property
                if (childStyle.Clear != CssClear.None)
                {
                    float clearY = floatCtx.GetClearY(childStyle.Clear);
                    if (clearY > cursorY) cursorY = clearY;
                }

                var childBox = CreateLayoutBox(childElement);

                // Apply box model
                BoxModelCalculator.ApplyBoxModel(childBox, childStyle, containingWidth);

                // Resolve content width
                float contentWidth;
                bool isReplaced = ReplacedElementLayout.IsReplaced(childElement);

                if (isReplaced && float.IsNaN(childStyle.Width))
                {
                    // Replaced element with auto width: use HTML attribute, form control defaults, or fallback
                    float intrinsicW = 0;
                    string? attrW = childElement.GetAttribute("width");
                    if (attrW != null && float.TryParse(attrW, out float aw)) intrinsicW = aw;
                    if (intrinsicW <= 0 && ReplacedElementLayout.IsFormControl(childElement))
                        intrinsicW = ReplacedElementLayout.GetFormControlIntrinsicWidth(childElement);
                    contentWidth = intrinsicW > 0 ? intrinsicW : 300;
                }
                else if (SizingKeyword.IsSizingKeyword(childStyle.Width))
                {
                    // Intrinsic sizing keyword: measure content
                    contentWidth = MeasureIntrinsicWidth(childElement, childStyle.Width, containingWidth, context);
                }
                else
                {
                    contentWidth = DimensionResolver.ResolveWidth(childStyle, containingWidth, childBox);
                }

                // Resolve auto margins
                var tempRect = new RectF(0, 0, contentWidth, 0);
                childBox.ContentRect = tempRect;
                DimensionResolver.ResolveAutoMargins(childStyle, childBox, containingWidth);

                // Margin collapsing
                float marginTop = childBox.MarginTop;
                float collapsedMargin;
                if (i == 0 && MarginCollapsing.ShouldCollapseWithFirstChild(parent))
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
                    // Vertical writing mode: blocks stack horizontally.
                    // The inline dimension is the container's height (containingWidth above).
                    // Each child block fills the inline dimension (height = containingWidth)
                    // and is positioned along the block axis (horizontal).
                    float x = cursorX + childBox.MarginLeft + childBox.BorderLeftWidth + childBox.PaddingLeft;
                    float y = parent.ContentRect.Y + childBox.MarginTop + childBox.BorderTopWidth + childBox.PaddingTop;

                    // In vertical mode, the child's "width" is its block size and
                    // "height" is its inline size (= containingWidth for a block-level child).
                    float childInlineSize = containingWidth;
                    childBox.ContentRect = new RectF(x, y, contentWidth, childInlineSize);

                    float contentHeight;
                    if (isReplaced)
                    {
                        float intrinsicW = 0, intrinsicH = 0;
                        string? attrW = childElement.GetAttribute("width");
                        string? attrH = childElement.GetAttribute("height");
                        if (attrW != null && float.TryParse(attrW, out float aw)) intrinsicW = aw;
                        if (attrH != null && float.TryParse(attrH, out float ah)) intrinsicH = ah;
                        if (ReplacedElementLayout.IsFormControl(childElement))
                        {
                            if (intrinsicW <= 0) intrinsicW = ReplacedElementLayout.GetFormControlIntrinsicWidth(childElement);
                            if (intrinsicH <= 0) intrinsicH = ReplacedElementLayout.GetFormControlIntrinsicHeight(childElement);
                        }
                        ReplacedElementLayout.ResolveDimensions(childBox, childStyle, containingWidth, intrinsicW, intrinsicH);
                        contentWidth = childBox.ContentRect.Width;
                        contentHeight = childBox.ContentRect.Height;
                    }
                    else
                    {
                        LayoutChildren(childBox, context);
                        contentHeight = DimensionResolver.ResolveHeight(childStyle, parentContentHeight, childBox);
                        if (float.IsNaN(contentHeight))
                        {
                            var contain = childStyle.Contain;
                            if (contain == CssContain.Size || contain == CssContain.Strict)
                                contentHeight = 0;
                            else
                                contentHeight = CalculateAutoHeight(childBox);

                            // Apply min-height / max-height to auto height
                            float minH = DimensionResolver.ResolvePercentHeight(childStyle.MinHeight, parentContentHeight);
                            float maxH = DimensionResolver.ResolvePercentHeight(childStyle.MaxHeight, parentContentHeight);
                            if (!float.IsNaN(maxH) && maxH >= 0 && contentHeight > maxH)
                                contentHeight = maxH;
                            if (!float.IsNaN(minH) && minH >= 0 && contentHeight < minH)
                                contentHeight = minH;
                        }
                    }

                    childBox.ContentRect = new RectF(x, y, contentWidth, contentHeight);
                    parent.AddChild(childBox);

                    // Advance the cursor in the block direction (horizontal)
                    cursorX = x + contentWidth + childBox.PaddingRight + childBox.BorderRightWidth + childBox.MarginRight;
                    prevMarginBottom = 0; // No vertical margin collapsing in vertical mode
                }
                else
                {
                    // Position the child
                    float x = parent.ContentRect.X + childBox.MarginLeft + childBox.BorderLeftWidth + childBox.PaddingLeft;
                    float y = cursorY + collapsedMargin + childBox.BorderTopWidth + childBox.PaddingTop;

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
                            if (intrinsicW <= 0) intrinsicW = ReplacedElementLayout.GetFormControlIntrinsicWidth(childElement);
                            if (intrinsicH <= 0) intrinsicH = ReplacedElementLayout.GetFormControlIntrinsicHeight(childElement);
                        }
                        ReplacedElementLayout.ResolveDimensions(childBox, childStyle, containingWidth, intrinsicW, intrinsicH);
                        contentWidth = childBox.ContentRect.Width;
                        contentHeight = childBox.ContentRect.Height;
                    }
                    else
                    {
                        // Layout children recursively
                        LayoutChildren(childBox, context);

                        // Resolve content height
                        contentHeight = DimensionResolver.ResolveHeight(childStyle, parentContentHeight, childBox);
                        if (float.IsNaN(contentHeight))
                        {
                            // contain: size or contain: strict → treat auto height as 0
                            var contain = childStyle.Contain;
                            if (contain == CssContain.Size || contain == CssContain.Strict)
                                contentHeight = 0;
                            else
                                contentHeight = CalculateAutoHeight(childBox);

                            // Apply min-height / max-height to auto height
                            float minH = DimensionResolver.ResolvePercentHeight(childStyle.MinHeight, parentContentHeight);
                            float maxH = DimensionResolver.ResolvePercentHeight(childStyle.MaxHeight, parentContentHeight);
                            if (!float.IsNaN(maxH) && maxH >= 0 && contentHeight > maxH)
                                contentHeight = maxH;
                            if (!float.IsNaN(minH) && minH >= 0 && contentHeight < minH)
                                contentHeight = minH;
                        }
                    }

                    childBox.ContentRect = new RectF(x, y, contentWidth, contentHeight);

                    // <legend> inside <fieldset>: shift up to center on the top border
                    if (isFieldset && i == 0 && childElement.TagName == "legend")
                    {
                        float legendHeight = contentHeight + childBox.PaddingTop + childBox.PaddingBottom;
                        float offsetUp = legendHeight * 0.5f;
                        childBox.ContentRect = new RectF(x, y - offsetUp, contentWidth, contentHeight);
                    }

                    parent.AddChild(childBox);

                    cursorY = childBox.ContentRect.Y + contentHeight + childBox.PaddingBottom + childBox.BorderBottomWidth;
                    prevMarginBottom = childBox.MarginBottom;
                }
            }

            // Handle parent-last-child margin collapsing
            if (effectiveChildren.Count > 0 && MarginCollapsing.ShouldCollapseWithLastChild(parent))
            {
                parent.MarginBottom = MarginCollapsing.Collapse(parent.MarginBottom, prevMarginBottom);
            }

            // Restore previous float context
            context.FloatContext = prevFloatCtx;
        }

        private static LayoutBox CreateLayoutBox(StyledElement element)
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
                        InlineFormattingContext.Layout(box, context);
                    }
                    break;
            }
        }

        private static bool HasBlockChildren(StyledElement element)
        {
            for (int i = 0; i < element.Children.Count; i++)
            {
                var child = element.Children[i];
                if (child.IsText || child is StyledPseudoElement) continue;
                var childElement = (StyledElement)child;
                var display = childElement.Style.Display;
                // display:contents — look through its children
                if (display == CssDisplay.Contents)
                {
                    if (HasBlockChildren(childElement)) return true;
                    continue;
                }
                if (display == CssDisplay.Block || display == CssDisplay.Flex ||
                    display == CssDisplay.InlineFlex || display == CssDisplay.Grid ||
                    display == CssDisplay.InlineGrid || display == CssDisplay.Table ||
                    display == CssDisplay.ListItem)
                    return true;
            }
            return false;
        }

        private static float CalculateAutoHeight(LayoutBox box)
        {
            float bottom = box.ContentRect.Y;

            // Check line boxes (from InlineFormattingContext)
            if (box.LineBoxes != null && box.LineBoxes.Count > 0)
            {
                var lastLine = box.LineBoxes[box.LineBoxes.Count - 1];
                float lineBottom = lastLine.Y + lastLine.Height;
                if (lineBottom > bottom) bottom = lineBottom;
            }

            // Check children (from BlockFormattingContext)
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                float childBottom = child.ContentRect.Y + child.ContentRect.Height
                                  + child.PaddingBottom + child.BorderBottomWidth + child.MarginBottom;
                if (childBottom > bottom)
                    bottom = childBottom;
            }

            return bottom - box.ContentRect.Y;
        }

        private static LayoutBox CreateInlineBox(StyledText textNode, LayoutContext context, float containingWidth, float cursorY, bool vertical = false)
        {
            var box = new LayoutText(textNode);
            box.ContentRect = new RectF(0, cursorY, containingWidth, 0);

            // Measure text
            float fontSize = textNode.Style.FontSize;
            float lineHeight = textNode.Style.LineHeight;
            // Negative = unitless multiplier, positive = pixels, NaN = normal
            if (lineHeight < 0)
                lineHeight = -lineHeight * fontSize;
            else if (float.IsNaN(lineHeight) || lineHeight == 0)
                lineHeight = fontSize * 1.2f;

            if (context.TextMeasurer != null)
            {
                var fontDesc = new Fonts.FontDescriptor(
                    textNode.Style.FontFamily ?? "serif",
                    textNode.Style.FontWeight,
                    textNode.Style.FontStyle,
                    Fonts.FontDescriptor.StretchToPercentage(textNode.Style.FontStretch));

                var shaped = context.TextMeasurer.Shape(textNode.Text, fontDesc, fontSize);
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

            // Measure actual content extent
            float maxRight = 0;
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                float right = child.ContentRect.X + child.ContentRect.Width
                            + child.PaddingRight + child.BorderRightWidth + child.MarginRight;
                if (right > maxRight) maxRight = right;
            }
            if (box.LineBoxes != null)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    float right = box.LineBoxes[i].X + box.LineBoxes[i].Width;
                    if (right > maxRight) maxRight = right;
                }
            }

            float measured = maxRight + box.PaddingLeft + box.PaddingRight
                           + box.BorderLeftWidth + box.BorderRightWidth;

            if (keyword == SizingKeyword.FitContent)
            {
                // fit-content = clamp(min-content, available, max-content)
                float minW = MeasureIntrinsicWidth(element, SizingKeyword.MinContent, containingWidth, context);
                float maxW = MeasureIntrinsicWidth(element, SizingKeyword.MaxContent, containingWidth, context);
                float available = containingWidth - BoxModelCalculator.GetHorizontalSpacing(box);
                return Math.Max(minW, Math.Min(maxW, available));
            }

            return measured;
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
