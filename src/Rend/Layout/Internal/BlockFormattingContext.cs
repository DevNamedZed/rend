using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
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
        /// Layout block-level children within a containing block.
        /// </summary>
        public static void Layout(LayoutBox parent, LayoutContext context)
        {
            var style = parent.StyledNode?.Style;
            float containingWidth = parent.ContentRect.Width;
            float cursorY = parent.ContentRect.Y;
            float prevMarginBottom = 0;

            context.ContainingBlockWidth = containingWidth;

            var styledElement = parent.StyledNode as StyledElement;
            if (styledElement == null) return;

            for (int i = 0; i < styledElement.Children.Count; i++)
            {
                var child = styledElement.Children[i];

                if (child.IsText)
                {
                    // Text in block context: create anonymous inline box
                    var textNode = (StyledText)child;
                    if (string.IsNullOrWhiteSpace(textNode.Text)) continue;

                    var inlineBox = CreateInlineBox(textNode, context, containingWidth, cursorY);
                    parent.AddChild(inlineBox);
                    cursorY = inlineBox.MarginRect.Bottom;
                    prevMarginBottom = 0;
                    continue;
                }

                var childElement = (StyledElement)child;
                var childStyle = childElement.Style;

                // Skip display:none
                if (childStyle.Display == CssDisplay.None) continue;

                var childBox = CreateLayoutBox(childElement);

                // Apply box model
                BoxModelCalculator.ApplyBoxModel(childBox, childStyle, containingWidth);

                // Resolve content width
                float contentWidth = DimensionResolver.ResolveWidth(childStyle, containingWidth, childBox);

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

                // Position the child
                float x = parent.ContentRect.X + childBox.MarginLeft + childBox.BorderLeftWidth + childBox.PaddingLeft;
                float y = cursorY + collapsedMargin + childBox.BorderTopWidth + childBox.PaddingTop;

                childBox.ContentRect = new RectF(x, y, contentWidth, 0);

                // Layout children recursively
                LayoutChildren(childBox, context);

                // Resolve content height
                float contentHeight = DimensionResolver.ResolveHeight(childStyle, float.NaN, childBox);
                if (float.IsNaN(contentHeight))
                {
                    // Auto height: determined by content
                    contentHeight = CalculateAutoHeight(childBox);
                }

                childBox.ContentRect = new RectF(x, y, contentWidth, contentHeight);

                parent.AddChild(childBox);

                cursorY = childBox.ContentRect.Y + contentHeight + childBox.PaddingBottom + childBox.BorderBottomWidth;
                prevMarginBottom = childBox.MarginBottom;
            }

            // Handle parent-last-child margin collapsing
            if (styledElement.Children.Count > 0 && MarginCollapsing.ShouldCollapseWithLastChild(parent))
            {
                parent.MarginBottom = MarginCollapsing.Collapse(parent.MarginBottom, prevMarginBottom);
            }
        }

        private static LayoutBox CreateLayoutBox(StyledElement element)
        {
            var display = element.Style.Display;
            BoxType boxType;

            switch (display)
            {
                case CssDisplay.Flex:
                    boxType = BoxType.Flex;
                    break;
                case CssDisplay.Grid:
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

        private static void LayoutChildren(LayoutBox box, LayoutContext context)
        {
            var styledElement = box.StyledNode as StyledElement;
            if (styledElement == null || styledElement.Children.Count == 0) return;

            var display = styledElement.Style.Display;

            switch (display)
            {
                case CssDisplay.Flex:
                    FlexLayout.Layout(box, context);
                    break;
                case CssDisplay.Grid:
                    GridLayout.Layout(box, context);
                    break;
                case CssDisplay.Table:
                    TableLayout.Layout(box, context);
                    break;
                default:
                    // Determine if this is a block or inline formatting context
                    if (HasBlockChildren(styledElement))
                        Layout(box, context);
                    else
                        InlineFormattingContext.Layout(box, context);
                    break;
            }
        }

        private static bool HasBlockChildren(StyledElement element)
        {
            for (int i = 0; i < element.Children.Count; i++)
            {
                var child = element.Children[i];
                if (child.IsText) continue;
                var childElement = (StyledElement)child;
                var display = childElement.Style.Display;
                if (display == CssDisplay.Block || display == CssDisplay.Flex ||
                    display == CssDisplay.Grid || display == CssDisplay.Table ||
                    display == CssDisplay.ListItem)
                    return true;
            }
            return false;
        }

        private static float CalculateAutoHeight(LayoutBox box)
        {
            if (box.Children.Count == 0)
            {
                // Check for line boxes
                if (box.LineBoxes != null && box.LineBoxes.Count > 0)
                {
                    var lastLine = box.LineBoxes[box.LineBoxes.Count - 1];
                    return (lastLine.Y + lastLine.Height) - box.ContentRect.Y;
                }
                return 0;
            }

            float bottom = box.ContentRect.Y;
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

        private static LayoutBox CreateInlineBox(StyledText textNode, LayoutContext context, float containingWidth, float cursorY)
        {
            var box = new LayoutText(textNode);
            box.ContentRect = new RectF(0, cursorY, containingWidth, 0);

            // Measure text
            float fontSize = textNode.Style.FontSize;
            float lineHeight = textNode.Style.LineHeight;
            if (float.IsNaN(lineHeight) || lineHeight <= 0)
                lineHeight = fontSize * 1.2f;

            if (context.TextMeasurer != null)
            {
                var fontDesc = new Fonts.FontDescriptor(
                    textNode.Style.FontFamily ?? "serif",
                    textNode.Style.FontWeight,
                    textNode.Style.FontStyle,
                    100f);

                var shaped = context.TextMeasurer.Shape(textNode.Text, fontDesc, fontSize);
                box.ShapedRun = shaped;
                box.TextX = 0;
                box.TextY = cursorY + lineHeight;
                box.ContentRect = new RectF(0, cursorY, Math.Min(shaped.TotalWidth, containingWidth), lineHeight);
            }
            else
            {
                // Fallback: estimate text width
                float estimatedWidth = textNode.Text.Length * fontSize * 0.6f;
                float numLines = (float)Math.Ceiling(estimatedWidth / containingWidth);
                box.ContentRect = new RectF(0, cursorY, containingWidth, numLines * lineHeight);
            }

            return box;
        }
    }
}
