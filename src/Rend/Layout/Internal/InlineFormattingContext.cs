using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Fonts;
using Rend.Style;
using Rend.Text;
using Rend.Text.Internal;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Inline formatting context: constructs line boxes from inline-level content,
    /// handles text wrapping, white-space processing, and vertical alignment.
    /// CSS 2.1 §9.4.2
    /// </summary>
    internal static class InlineFormattingContext
    {
        public static void Layout(LayoutBox parent, LayoutContext context)
        {
            var styledElement = parent.StyledNode as StyledElement;
            if (styledElement == null) return;

            float containingWidth = parent.ContentRect.Width;
            float startX = parent.ContentRect.X;
            float cursorX = startX;
            float cursorY = parent.ContentRect.Y;

            var lineBoxes = new List<LineBox>();
            var currentLine = new LineBox { X = startX, Y = cursorY, Width = containingWidth };
            float maxLineHeight = 0;
            float lineBaseline = 0;

            // Text indent for first line
            float textIndent = styledElement.Style.TextIndent;
            if (!float.IsNaN(textIndent) && textIndent != 0)
                cursorX += textIndent;

            for (int i = 0; i < styledElement.Children.Count; i++)
            {
                var child = styledElement.Children[i];

                if (child.IsText)
                {
                    var textNode = (StyledText)child;
                    LayoutTextRun(textNode, context, ref cursorX, ref cursorY, startX,
                                  containingWidth, currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);
                }
                else
                {
                    var childElement = (StyledElement)child;
                    if (childElement.Style.Display == CssDisplay.None) continue;

                    if (childElement.Style.Display == CssDisplay.InlineBlock)
                    {
                        LayoutInlineBlock(childElement, context, ref cursorX, ref cursorY, startX,
                                          containingWidth, currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);
                    }
                    else
                    {
                        // Inline element: process as text-like
                        LayoutInlineElement(childElement, context, ref cursorX, ref cursorY, startX,
                                           containingWidth, currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);
                    }
                }
            }

            // Finalize last line
            if (currentLine.Fragments.Count > 0)
            {
                FinalizeLineBox(currentLine, maxLineHeight, lineBaseline, styledElement.Style.TextAlign);
                lineBoxes.Add(currentLine);
            }

            parent.LineBoxes = lineBoxes;
        }

        private static void LayoutTextRun(
            StyledText textNode, LayoutContext context,
            ref float cursorX, ref float cursorY, float startX, float containingWidth,
            LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline, LayoutBox parent)
        {
            var style = textNode.Style;
            string text = textNode.Text;

            // Apply white-space processing
            text = WhitespaceCollapser.Collapse(text, style.WhiteSpace);
            if (string.IsNullOrEmpty(text)) return;

            // Apply text-transform
            text = TextTransformer.Transform(text, style.TextTransform);

            float fontSize = style.FontSize;
            float lineHeight = style.LineHeight;
            if (float.IsNaN(lineHeight) || lineHeight <= 0)
                lineHeight = fontSize * 1.2f;

            float ascent = fontSize * 0.8f;
            float descent = fontSize * 0.2f;

            if (context.TextMeasurer != null)
            {
                var fontDesc = new FontDescriptor(
                    style.FontFamily ?? "serif",
                    style.FontWeight,
                    style.FontStyle,
                    100f);

                // Shape and measure
                var shaped = context.TextMeasurer.Shape(text, fontDesc, fontSize);

                if (cursorX + shaped.TotalWidth <= startX + containingWidth ||
                    style.WhiteSpace == CssWhiteSpace.Nowrap ||
                    style.WhiteSpace == CssWhiteSpace.Pre)
                {
                    // Fits on current line
                    AddTextFragment(currentLine, text, shaped, cursorX, shaped.TotalWidth, lineHeight, ascent);
                    cursorX += shaped.TotalWidth;
                    UpdateLineMetrics(ref maxLineHeight, ref lineBaseline, lineHeight, ascent);
                }
                else
                {
                    // Need to wrap: split text at word boundaries
                    WrapText(text, fontDesc, fontSize, context, ref cursorX, ref cursorY, startX,
                             containingWidth, currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline,
                             lineHeight, ascent, parent);
                }
            }
            else
            {
                // Fallback: estimate
                float charWidth = fontSize * 0.6f;
                float textWidth = text.Length * charWidth;

                if (cursorX + textWidth > startX + containingWidth &&
                    style.WhiteSpace != CssWhiteSpace.Nowrap)
                {
                    // Wrap at word boundaries
                    WrapTextSimple(text, charWidth, ref cursorX, ref cursorY, startX, containingWidth,
                                   currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline,
                                   lineHeight, ascent, parent);
                }
                else
                {
                    var fragment = new LineFragment
                    {
                        X = cursorX,
                        Width = textWidth,
                        Height = lineHeight,
                        Baseline = ascent,
                        Text = text
                    };
                    currentLine.AddFragment(fragment);
                    cursorX += textWidth;
                    UpdateLineMetrics(ref maxLineHeight, ref lineBaseline, lineHeight, ascent);
                }
            }
        }

        private static void WrapText(
            string text, FontDescriptor fontDesc, float fontSize,
            LayoutContext context,
            ref float cursorX, ref float cursorY, float startX, float containingWidth,
            LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline,
            float lineHeight, float ascent, LayoutBox parent)
        {
            // Find break opportunities
            var breaker = new LineBreaker();
            var breaks = breaker.FindBreaks(text);
            int wordStart = 0;

            for (int j = 0; j < text.Length; j++)
            {
                if (j < breaks.Length && breaks[j] == LineBreakOpportunity.Allowed || j == text.Length - 1)
                {
                    int end = j == text.Length - 1 ? text.Length : j + 1;
                    string word = text.Substring(wordStart, end - wordStart);
                    var shaped = context.TextMeasurer!.Shape(word, fontDesc, fontSize);

                    if (cursorX + shaped.TotalWidth > startX + containingWidth && currentLine.Fragments.Count > 0)
                    {
                        // Start new line
                        var parentStyle = parent.StyledNode as StyledElement;
                        var align = parentStyle?.Style.TextAlign ?? CssTextAlign.Left;
                        FinalizeLineBox(currentLine, maxLineHeight, lineBaseline, align);
                        lineBoxes.Add(currentLine);
                        cursorY += maxLineHeight;
                        currentLine = new LineBox { X = startX, Y = cursorY, Width = containingWidth };
                        cursorX = startX;
                        maxLineHeight = 0;
                        lineBaseline = 0;
                    }

                    AddTextFragment(currentLine, word, shaped, cursorX, shaped.TotalWidth, lineHeight, ascent);
                    cursorX += shaped.TotalWidth;
                    UpdateLineMetrics(ref maxLineHeight, ref lineBaseline, lineHeight, ascent);
                    wordStart = end;
                }
            }
        }

        private static void WrapTextSimple(
            string text, float charWidth,
            ref float cursorX, ref float cursorY, float startX, float containingWidth,
            LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline,
            float lineHeight, float ascent, LayoutBox parent)
        {
            string[] words = text.Split(' ');
            for (int w = 0; w < words.Length; w++)
            {
                string word = words[w];
                if (w > 0) word = " " + word;
                float wordWidth = word.Length * charWidth;

                if (cursorX + wordWidth > startX + containingWidth && currentLine.Fragments.Count > 0)
                {
                    var parentStyle = parent.StyledNode as StyledElement;
                    var align = parentStyle?.Style.TextAlign ?? CssTextAlign.Left;
                    FinalizeLineBox(currentLine, maxLineHeight, lineBaseline, align);
                    lineBoxes.Add(currentLine);
                    cursorY += maxLineHeight;
                    currentLine = new LineBox { X = startX, Y = cursorY, Width = containingWidth };
                    cursorX = startX;
                    maxLineHeight = 0;
                    lineBaseline = 0;
                    word = word.TrimStart();
                    wordWidth = word.Length * charWidth;
                }

                var fragment = new LineFragment
                {
                    X = cursorX,
                    Width = wordWidth,
                    Height = lineHeight,
                    Baseline = ascent,
                    Text = word
                };
                currentLine.AddFragment(fragment);
                cursorX += wordWidth;
                UpdateLineMetrics(ref maxLineHeight, ref lineBaseline, lineHeight, ascent);
            }
        }

        private static void LayoutInlineBlock(
            StyledElement element, LayoutContext context,
            ref float cursorX, ref float cursorY, float startX, float containingWidth,
            LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline, LayoutBox parent)
        {
            var box = new LayoutBox(element, BoxType.InlineBlock);
            BoxModelCalculator.ApplyBoxModel(box, element.Style, containingWidth);
            float contentWidth = DimensionResolver.ResolveWidth(element.Style, containingWidth, box);
            float totalWidth = contentWidth + box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth;

            if (cursorX + totalWidth > startX + containingWidth && currentLine.Fragments.Count > 0)
            {
                var parentStyle = parent.StyledNode as StyledElement;
                var align = parentStyle?.Style.TextAlign ?? CssTextAlign.Left;
                FinalizeLineBox(currentLine, maxLineHeight, lineBaseline, align);
                lineBoxes.Add(currentLine);
                cursorY += maxLineHeight;
                currentLine = new LineBox { X = startX, Y = cursorY, Width = containingWidth };
                cursorX = startX;
                maxLineHeight = 0;
                lineBaseline = 0;
            }

            box.ContentRect = new RectF(cursorX + box.MarginLeft + box.BorderLeftWidth + box.PaddingLeft,
                                        cursorY, contentWidth, 0);

            // Layout contents
            BlockFormattingContext.Layout(box, context);
            float contentHeight = DimensionResolver.ResolveHeight(element.Style, float.NaN, box);
            if (float.IsNaN(contentHeight))
                contentHeight = CalculateContentHeight(box);
            box.ContentRect = new RectF(box.ContentRect.X, cursorY, contentWidth, contentHeight);

            float totalHeight = contentHeight + box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth;

            var fragment = new LineFragment
            {
                X = cursorX,
                Width = totalWidth + box.MarginLeft + box.MarginRight,
                Height = totalHeight + box.MarginTop + box.MarginBottom,
                Baseline = totalHeight,
                Box = box
            };
            currentLine.AddFragment(fragment);
            parent.AddChild(box);

            cursorX += fragment.Width;
            UpdateLineMetrics(ref maxLineHeight, ref lineBaseline, fragment.Height, fragment.Baseline);
        }

        private static void LayoutInlineElement(
            StyledElement element, LayoutContext context,
            ref float cursorX, ref float cursorY, float startX, float containingWidth,
            LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline, LayoutBox parent)
        {
            // For inline elements, process their children as if they're part of this inline context
            for (int i = 0; i < element.Children.Count; i++)
            {
                var child = element.Children[i];
                if (child.IsText)
                {
                    LayoutTextRun((StyledText)child, context, ref cursorX, ref cursorY, startX,
                                  containingWidth, currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);
                }
            }
        }

        private static void AddTextFragment(LineBox line, string text, ShapedTextRun? shaped,
                                              float x, float width, float height, float baseline)
        {
            var fragment = new LineFragment
            {
                X = x,
                Width = width,
                Height = height,
                Baseline = baseline,
                Text = text,
                ShapedRun = shaped
            };
            line.AddFragment(fragment);
        }

        private static void UpdateLineMetrics(ref float maxLineHeight, ref float lineBaseline,
                                               float height, float baseline)
        {
            if (height > maxLineHeight) maxLineHeight = height;
            if (baseline > lineBaseline) lineBaseline = baseline;
        }

        private static void FinalizeLineBox(LineBox line, float height, float baseline, CssTextAlign textAlign)
        {
            line.Height = height > 0 ? height : 16f; // minimum line height
            line.Baseline = baseline;

            // Calculate actual content width
            float contentWidth = 0;
            for (int i = 0; i < line.Fragments.Count; i++)
            {
                var frag = line.Fragments[i];
                contentWidth = Math.Max(contentWidth, frag.X + frag.Width - line.X);
            }

            // Apply text-align
            float freeSpace = line.Width - contentWidth;
            if (freeSpace <= 0) return;

            float offset = 0;
            switch (textAlign)
            {
                case CssTextAlign.Center:
                    offset = freeSpace / 2;
                    break;
                case CssTextAlign.Right:
                case CssTextAlign.End:
                    offset = freeSpace;
                    break;
                case CssTextAlign.Justify:
                    // Distribute space between fragments
                    if (line.Fragments.Count > 1)
                    {
                        float gap = freeSpace / (line.Fragments.Count - 1);
                        for (int i = 1; i < line.Fragments.Count; i++)
                        {
                            var frag = line.Fragments[i];
                            frag.X += gap * i;
                        }
                    }
                    return;
            }

            if (offset > 0)
            {
                for (int i = 0; i < line.Fragments.Count; i++)
                {
                    line.Fragments[i].X += offset;
                }
            }
        }

        private static float CalculateContentHeight(LayoutBox box)
        {
            float height = 0;
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                float childBottom = child.ContentRect.Y + child.ContentRect.Height
                                  + child.PaddingBottom + child.BorderBottomWidth + child.MarginBottom;
                float childHeight = childBottom - box.ContentRect.Y;
                if (childHeight > height) height = childHeight;
            }
            if (box.LineBoxes != null)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    var line = box.LineBoxes[i];
                    float lineBottom = line.Y + line.Height - box.ContentRect.Y;
                    if (lineBottom > height) height = lineBottom;
                }
            }
            return height;
        }
    }
}
