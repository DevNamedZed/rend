using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
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
        /// <summary>
        /// Lazily-initialized hyphenation dictionary for auto-hyphenation (en-US patterns).
        /// </summary>
        private static HyphenationDictionary? s_hyphenationDict;
        private static readonly object s_hyphenLock = new object();

        private static HyphenationDictionary GetHyphenationDictionary()
        {
            if (s_hyphenationDict != null) return s_hyphenationDict;
            lock (s_hyphenLock)
            {
                if (s_hyphenationDict != null) return s_hyphenationDict;
                var dict = new HyphenationDictionary();
                dict.LoadPatterns(HyphenationPatterns.GetEnglishPatterns());
                s_hyphenationDict = dict;
                return dict;
            }
        }

        public static void Layout(LayoutBox parent, LayoutContext context)
        {
            var styledElement = parent.StyledNode as StyledElement;
            if (styledElement == null) return;

            bool vertical = BlockFormattingContext.IsVerticalWritingMode(styledElement.Style);

            if (vertical)
            {
                // Vertical writing mode: fall back to horizontal layout for now
                // (full vertical-rl/vertical-lr is a future enhancement)
            }

            float containingWidth = parent.ContentRect.Width;
            float startX = parent.ContentRect.X;
            float cursorX = startX;
            float cursorY = parent.ContentRect.Y;

            // Adjust for floats if a float context is available
            var floatCtx = context.FloatContext;
            if (floatCtx != null)
            {
                float leftEdge = floatCtx.GetLeftEdge(cursorY, 0);
                float rightEdge = floatCtx.GetRightEdge(cursorY, 0);
                if (leftEdge > startX) startX = leftEdge;
                float availWidth = rightEdge - startX;
                if (availWidth < containingWidth) containingWidth = availWidth;
                cursorX = startX;
            }

            var lineBoxes = new List<LineBox>();
            var currentLine = new LineBox { X = startX, Y = cursorY, Width = containingWidth };
            float maxLineHeight = 0;
            float lineBaseline = 0;

            // ::first-letter tracking
            bool firstLetterProcessed = styledElement.FirstLetterStyle == null;

            // Text indent for first line
            float textIndent = styledElement.Style.TextIndent;
            if (!float.IsNaN(textIndent) && textIndent != 0)
                cursorX += textIndent;

            // list-style-position: inside — reserve space on the first line for the marker
            if (parent.BoxType == BoxType.ListItem &&
                styledElement.Style.ListStylePosition == CssListStylePosition.Inside &&
                styledElement.Style.ListStyleType != CssListStyleType.None)
            {
                // Approximate marker width: bullet + gap
                float markerReserve;
                var lstType = styledElement.Style.ListStyleType;
                bool isSummary = styledElement.TagName == "summary";
                if (isSummary)
                {
                    // Disclosure triangle is ~0.5em wide + small gap
                    markerReserve = styledElement.Style.FontSize * 0.5f + 4f;
                }
                else if (lstType == CssListStyleType.Disc || lstType == CssListStyleType.Circle ||
                    lstType == CssListStyleType.Square)
                {
                    // Chrome uses ~1.375em marker box for inside disc/circle/square
                    markerReserve = styledElement.Style.FontSize * 1.375f;
                }
                else
                {
                    // Counter text: estimate width
                    markerReserve = styledElement.Style.FontSize * 1.2f;
                }
                cursorX += markerReserve;
            }

            for (int i = 0; i < styledElement.Children.Count; i++)
            {
                var child = styledElement.Children[i];

                if (child.IsText)
                {
                    var textNode = (StyledText)child;

                    // ::first-letter: split off the first letter of the first text run
                    if (!firstLetterProcessed)
                    {
                        firstLetterProcessed = true;
                        string text = textNode.Text;
                        string trimmed = text.TrimStart();
                        if (trimmed.Length > 0)
                        {
                            // Find the first letter (skip leading punctuation per CSS spec)
                            int letterIdx = 0;
                            while (letterIdx < trimmed.Length && char.IsPunctuation(trimmed[letterIdx]))
                                letterIdx++;

                            if (letterIdx < trimmed.Length)
                            {
                                // Include the letter (and any leading punctuation)
                                int endIdx = letterIdx + 1;
                                // Handle surrogate pairs
                                if (char.IsHighSurrogate(trimmed[letterIdx]) && endIdx < trimmed.Length)
                                    endIdx++;

                                string firstLetter = trimmed.Substring(0, endIdx);
                                string remainder = trimmed.Substring(endIdx);

                                var flStyle = styledElement.FirstLetterStyle!;

                                // Check if ::first-letter is floated (common drop-cap pattern)
                                if (flStyle.Float != CssFloat.None && floatCtx != null && context.TextMeasurer != null)
                                {
                                    // Build font descriptor for the first-letter style
                                    var flFontDesc = new Fonts.FontDescriptor(
                                        flStyle.FontFamily ?? "serif",
                                        flStyle.FontWeight,
                                        flStyle.FontStyle,
                                        Fonts.FontDescriptor.StretchToPercentage(flStyle.FontStretch));
                                    float flFontSize = flStyle.FontSize;
                                    float flLineHeight = flStyle.LineHeight;
                                    bool flNormalLH = float.IsNaN(flLineHeight) || flLineHeight == 0;
                                    if (flLineHeight < 0) flLineHeight = -flLineHeight * flFontSize;
                                    else if (flNormalLH) flLineHeight = flFontSize * 1.2f;
                                    if (flNormalLH)
                                    {
                                        float mlh = context.TextMeasurer.GetNormalLineHeight(flFontDesc, flFontSize);
                                        if (!float.IsNaN(mlh) && mlh > 0) flLineHeight = mlh;
                                    }
                                    float flAscent = context.TextMeasurer.GetAscent(flFontDesc, flFontSize);
                                    float flDescent = context.TextMeasurer.GetDescent(flFontDesc, flFontSize);
                                    float flContentArea = flAscent + flDescent;
                                    float flBaseline = flAscent + (flLineHeight - flContentArea) / 2f;

                                    var flShaped = context.TextMeasurer.Shape(firstLetter, flFontDesc, flFontSize);
                                    float flTextWidth = flShaped.TotalWidth;

                                    // Box model from first-letter style
                                    float flMarginLeft = float.IsNaN(flStyle.MarginLeft) ? 0 : flStyle.MarginLeft;
                                    float flMarginRight = float.IsNaN(flStyle.MarginRight) ? 0 : flStyle.MarginRight;
                                    float flMarginTop = float.IsNaN(flStyle.MarginTop) ? 0 : flStyle.MarginTop;
                                    float flMarginBottom = float.IsNaN(flStyle.MarginBottom) ? 0 : flStyle.MarginBottom;
                                    float flPadLeft = flStyle.PaddingLeft;
                                    float flPadRight = flStyle.PaddingRight;
                                    float flPadTop = flStyle.PaddingTop;
                                    float flPadBottom = flStyle.PaddingBottom;

                                    float flContentW = flTextWidth + flPadLeft + flPadRight;
                                    float flContentH = flLineHeight;
                                    float flTotalW = flContentW + flMarginLeft + flMarginRight;
                                    float flTotalH = flContentH + flPadTop + flPadBottom + flMarginTop + flMarginBottom;

                                    // Create a float box
                                    var flBox = new LayoutBox(styledElement, BoxType.Block);
                                    flBox.MarginLeft = flMarginLeft;
                                    flBox.MarginRight = flMarginRight;
                                    flBox.MarginTop = flMarginTop;
                                    flBox.MarginBottom = flMarginBottom;
                                    flBox.PaddingLeft = flPadLeft;
                                    flBox.PaddingRight = flPadRight;
                                    flBox.PaddingTop = flPadTop;
                                    flBox.PaddingBottom = flPadBottom;

                                    float flY = cursorY;
                                    float flX;
                                    if (flStyle.Float == CssFloat.Left)
                                    {
                                        flX = floatCtx.GetLeftEdge(flY, flTotalH) + flMarginLeft;
                                        floatCtx.AddLeftFloat(new Core.Values.RectF(flX - flMarginLeft, flY, flTotalW, flTotalH));
                                    }
                                    else
                                    {
                                        flX = floatCtx.GetRightEdge(flY, flTotalH) - flTotalW + flMarginLeft;
                                        floatCtx.AddRightFloat(new Core.Values.RectF(flX - flMarginLeft, flY, flTotalW, flTotalH));
                                    }

                                    flBox.ContentRect = new Core.Values.RectF(
                                        flX + flPadLeft, flY + flMarginTop + flPadTop,
                                        flTextWidth, flContentH);

                                    // Add a line box with the letter fragment for painting
                                    var flLine = new LineBox { X = flBox.ContentRect.X, Y = flBox.ContentRect.Y, Width = flTextWidth };
                                    flLine.AddFragment(new LineFragment
                                    {
                                        X = 0,
                                        Width = flTextWidth,
                                        Height = flLineHeight,
                                        ContentHeight = flContentArea,
                                        Baseline = flBaseline,
                                        Text = firstLetter,
                                        ShapedRun = flShaped,
                                        StyleOverride = flStyle
                                    });
                                    flLine.Height = flLineHeight;
                                    flLine.Baseline = flBaseline;
                                    flBox.LineBoxes = new List<LineBox> { flLine };
                                    parent.AddChild(flBox);

                                    // Adjust inline cursor for float exclusion
                                    float newLeft = floatCtx.GetLeftEdge(cursorY, 0);
                                    float newRight = floatCtx.GetRightEdge(cursorY, 0);
                                    if (newLeft > startX) startX = newLeft;
                                    containingWidth = newRight - startX;
                                    cursorX = startX;
                                    currentLine.X = startX;
                                    currentLine.Width = containingWidth;
                                }
                                else
                                {
                                    // Non-floated first letter: layout inline
                                    var firstLetterText = new StyledText(firstLetter, flStyle);
                                    LayoutTextRun(firstLetterText, context, ref cursorX, ref cursorY, ref startX,
                                                  ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent,
                                                  styleOverride: flStyle);
                                }

                                // Layout the remainder with normal style
                                if (remainder.Length > 0)
                                {
                                    var remainderText = new StyledText(remainder, textNode.Style);
                                    LayoutTextRun(remainderText, context, ref cursorX, ref cursorY, ref startX,
                                                  ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);
                                }
                                continue;
                            }
                        }
                    }

                    LayoutTextRun(textNode, context, ref cursorX, ref cursorY, ref startX,
                                  ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);
                }
                else if (child is StyledPseudoElement pseudo)
                {
                    // Pseudo-element: render as inline text with its own style.
                    // Pass the pseudo's style as styleOverride so the painter uses
                    // its color/font properties instead of falling through to the parent.
                    var pseudoText = new StyledText(pseudo.Content, pseudo.Style);
                    LayoutTextRun(pseudoText, context, ref cursorX, ref cursorY, ref startX,
                                  ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent,
                                  styleOverride: pseudo.Style);
                }
                else
                {
                    var childElement = (StyledElement)child;
                    if (childElement.Style.Display == CssDisplay.None) continue;

                    // display:contents — unwrap children into inline context
                    if (childElement.Style.Display == CssDisplay.Contents)
                    {
                        LayoutInlineElement(childElement, context, ref cursorX, ref cursorY, ref startX,
                                           ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);
                        continue;
                    }

                    // Handle <br> as a forced line break
                    if (childElement.TagName == "br")
                    {
                        StartNewLine(parent, ref cursorX, ref cursorY, ref startX, ref containingWidth,
                                     ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, context);
                        continue;
                    }

                    // Handle <wbr> as a soft break opportunity (break if overflowing)
                    if (childElement.TagName == "wbr")
                    {
                        if (cursorX > startX + containingWidth)
                        {
                            StartNewLine(parent, ref cursorX, ref cursorY, ref startX, ref containingWidth,
                                         ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, context);
                        }
                        continue;
                    }

                    // Ruby container: render base text with annotation above/below
                    if (childElement.Style.Display == CssDisplay.Ruby)
                    {
                        LayoutRubyContainer(childElement, context, ref cursorX, ref cursorY, ref startX,
                                           ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);
                        continue;
                    }

                    // Ruby sub-elements outside a ruby container: treat as inline
                    if (childElement.Style.Display == CssDisplay.RubyText ||
                        childElement.Style.Display == CssDisplay.RubyBase ||
                        childElement.Style.Display == CssDisplay.RubyTextContainer)
                    {
                        LayoutInlineElement(childElement, context, ref cursorX, ref cursorY, ref startX,
                                           ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);
                        continue;
                    }

                    if (childElement.Style.Display == CssDisplay.InlineBlock ||
                        childElement.Style.Display == CssDisplay.InlineFlex ||
                        childElement.Style.Display == CssDisplay.InlineGrid ||
                        ReplacedElementLayout.IsReplaced(childElement))
                    {
                        LayoutInlineBlock(childElement, context, ref cursorX, ref cursorY, startX,
                                          containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);
                    }
                    else
                    {
                        // Inline element: process as text-like
                        LayoutInlineElement(childElement, context, ref cursorX, ref cursorY, ref startX,
                                           ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);
                    }
                }
            }

            // Finalize last line
            if (currentLine.Fragments.Count > 0)
            {
                currentLine.IsLastLine = true;
                FinalizeLineBox(currentLine, maxLineHeight, lineBaseline, styledElement.Style.TextAlign,
                                styledElement.Style.TextAlignLast, styledElement.Style.Direction);
                lineBoxes.Add(currentLine);
            }

            // Apply ::first-line style override to all fragments on the first line
            if (styledElement.FirstLineStyle != null && lineBoxes.Count > 0)
            {
                var firstLine = lineBoxes[0];
                for (int fi = 0; fi < firstLine.Fragments.Count; fi++)
                {
                    var frag = firstLine.Fragments[fi];
                    if (frag.StyleOverride == null) // don't override ::first-letter
                        frag.StyleOverride = styledElement.FirstLineStyle;
                }
            }

            // Apply text-overflow: ellipsis when overflow is hidden
            if (styledElement.Style.TextOverflow == CssTextOverflow.Ellipsis &&
                (styledElement.Style.OverflowX == CssOverflow.Hidden ||
                 styledElement.Style.OverflowX == CssOverflow.Scroll ||
                 styledElement.Style.OverflowX == CssOverflow.Auto))
            {
                ApplyEllipsis(lineBoxes, startX, containingWidth, context, styledElement.Style);
            }

            // Apply text-wrap: balance — equalize line widths for short blocks (≤ 6 lines)
            if (styledElement.Style.TextWrap == CssTextWrap.Balance &&
                lineBoxes.Count >= 2 && lineBoxes.Count <= 6)
            {
                ApplyTextWrapBalance(lineBoxes, startX, containingWidth, styledElement.Style.TextAlign,
                                     styledElement.Style.TextAlignLast, styledElement.Style.Direction);
            }

            // Apply hanging-punctuation: shift punctuation outside the margin
            if (styledElement.Style.HangingPunctuation != CssHangingPunctuation.None && lineBoxes.Count > 0)
            {
                ApplyHangingPunctuation(lineBoxes, styledElement.Style.HangingPunctuation);
            }

            // Reconcile inline-block box positions with fragment vertical-align offsets.
            // After FinalizeLineBox applies vertical-align (frag.Y), update the actual
            // box ContentRect to reflect the final vertical position within the line.
            for (int li = 0; li < lineBoxes.Count; li++)
            {
                var line = lineBoxes[li];
                for (int fi = 0; fi < line.Fragments.Count; fi++)
                {
                    var frag = line.Fragments[fi];
                    if (frag.Box != null)
                    {
                        float newY = line.Y + frag.Y + frag.Box.MarginTop + frag.Box.BorderTopWidth + frag.Box.PaddingTop;
                        float dy = newY - frag.Box.ContentRect.Y;
                        if (Math.Abs(dy) > 0.01f)
                        {
                            frag.Box.ContentRect = new RectF(frag.Box.ContentRect.X, newY,
                                frag.Box.ContentRect.Width, frag.Box.ContentRect.Height);
                            // Also offset children positioned during inline-block layout
                            for (int ci = 0; ci < frag.Box.Children.Count; ci++)
                                OffsetChildBoxes(frag.Box.Children[ci], dy);
                            if (frag.Box.LineBoxes != null)
                            {
                                for (int lbi = 0; lbi < frag.Box.LineBoxes.Count; lbi++)
                                {
                                    frag.Box.LineBoxes[lbi].Y += dy;
                                }
                            }
                        }
                    }
                }
            }

            parent.LineBoxes = lineBoxes;
        }

        private static void OffsetChildBoxes(LayoutBox box, float dy)
        {
            box.ContentRect = new RectF(box.ContentRect.X, box.ContentRect.Y + dy,
                                        box.ContentRect.Width, box.ContentRect.Height);
            for (int i = 0; i < box.Children.Count; i++)
                OffsetChildBoxes(box.Children[i], dy);
            if (box.LineBoxes != null)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                    box.LineBoxes[i].Y += dy;
            }
        }

        /// <summary>
        /// Layout inline content in a vertical writing mode context.
        /// Lines run top-to-bottom (the inline direction), and new lines
        /// advance horizontally (the block direction).
        /// </summary>
        private static void LayoutVertical(LayoutBox parent, LayoutContext context, StyledElement styledElement)
        {
            // In vertical writing mode:
            // - The inline direction is top-to-bottom
            // - The "line length" is the container height (available inline space)
            // - Line breaks create new columns advancing horizontally
            float containingHeight = parent.ContentRect.Height;
            if (containingHeight <= 0)
            {
                // If no definite height, use the container width as a fallback
                // (this handles the case where height is auto)
                containingHeight = parent.ContentRect.Width;
            }
            if (containingHeight <= 0)
                containingHeight = 600f; // ultimate fallback

            float startY = parent.ContentRect.Y;
            float cursorY = startY;
            // Block cursor: for vertical-rl, columns advance right-to-left;
            // for vertical-lr, columns advance left-to-right.
            // We start at the left edge and let the caller/parent handle overall direction.
            float cursorX = parent.ContentRect.X;

            var lineBoxes = new List<LineBox>();
            var currentLine = new LineBox { X = cursorX, Y = startY, Width = containingHeight, IsVertical = true };
            float maxColumnWidth = 0; // "line height" in vertical mode = column width

            for (int i = 0; i < styledElement.Children.Count; i++)
            {
                var child = styledElement.Children[i];

                if (child.IsText)
                {
                    var textNode = (StyledText)child;
                    LayoutVerticalTextRun(textNode, context, ref cursorX, ref cursorY, startY,
                                          containingHeight, currentLine, lineBoxes, ref maxColumnWidth, parent);
                }
                else if (child is StyledPseudoElement pseudo)
                {
                    var pseudoText = new StyledText(pseudo.Content, pseudo.Style);
                    LayoutVerticalTextRun(pseudoText, context, ref cursorX, ref cursorY, startY,
                                          containingHeight, currentLine, lineBoxes, ref maxColumnWidth, parent);
                }
                else
                {
                    var childElement = (StyledElement)child;
                    if (childElement.Style.Display == CssDisplay.None) continue;

                    if (childElement.TagName == "br")
                    {
                        StartNewVerticalLine(parent, ref cursorX, ref cursorY, startY, containingHeight,
                                              ref currentLine, lineBoxes, ref maxColumnWidth);
                        continue;
                    }

                    // For other inline elements, recurse into their children
                    for (int j = 0; j < childElement.Children.Count; j++)
                    {
                        var grandchild = childElement.Children[j];
                        if (grandchild.IsText)
                        {
                            LayoutVerticalTextRun((StyledText)grandchild, context, ref cursorX, ref cursorY, startY,
                                                  containingHeight, currentLine, lineBoxes, ref maxColumnWidth, parent,
                                                  inlineAncestor: childElement);
                        }
                    }
                }
            }

            // Finalize last line
            if (currentLine.Fragments.Count > 0)
            {
                currentLine.IsLastLine = true;
                FinalizeVerticalLineBox(currentLine, maxColumnWidth);
                lineBoxes.Add(currentLine);
            }

            parent.LineBoxes = lineBoxes;
        }

        /// <summary>
        /// Layout a text run within a vertical writing mode inline context.
        /// Characters advance top-to-bottom. When the inline extent (height)
        /// overflows, a new column is started.
        /// </summary>
        private static void LayoutVerticalTextRun(
            StyledText textNode, LayoutContext context,
            ref float cursorX, ref float cursorY, float startY, float containingHeight,
            LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxColumnWidth, LayoutBox parent,
            StyledElement? inlineAncestor = null)
        {
            var style = textNode.Style;
            string text = textNode.Text;

            text = WhitespaceCollapser.Collapse(text, style.WhiteSpace);
            if (string.IsNullOrEmpty(text)) return;

            text = TextTransformer.Transform(text, style.TextTransform);

            float fontSize = style.FontSize;
            float lineHeight = style.LineHeight;
            // Negative = unitless multiplier, positive = pixels, NaN = normal
            if (lineHeight < 0)
                lineHeight = -lineHeight * fontSize;
            else if (float.IsNaN(lineHeight) || lineHeight == 0)
                lineHeight = fontSize * 1.2f;

            if (context.TextMeasurer != null)
            {
                var fontDesc = new FontDescriptor(
                    style.FontFamily ?? "serif",
                    style.FontWeight,
                    style.FontStyle,
                    FontDescriptor.StretchToPercentage(style.FontStretch));

                // In vertical mode, each character or word-segment occupies a vertical slot.
                // For the pragmatic approach (sideways text), we shape the entire run and
                // treat its measured width as the vertical extent.
                var shaped = context.TextMeasurer.Shape(text, fontDesc, fontSize);
                float textWidth = shaped.TotalWidth + CalculateSpacingExtra(text, style);

                if (cursorY + textWidth <= startY + containingHeight)
                {
                    // Fits in the current column
                    var fragment = new LineFragment
                    {
                        X = cursorX - currentLine.X, // offset within line box
                        Y = cursorY - startY,
                        Width = lineHeight, // each text run is one "line height" wide (column width)
                        Height = textWidth,
                        Baseline = fontSize * 0.8f,
                        Text = text,
                        ShapedRun = shaped,
                        InlineElement = inlineAncestor
                    };
                    currentLine.AddFragment(fragment);
                    cursorY += textWidth;
                    if (lineHeight > maxColumnWidth) maxColumnWidth = lineHeight;
                }
                else
                {
                    // Overflow: start a new column
                    StartNewVerticalLine(parent, ref cursorX, ref cursorY, startY, containingHeight,
                                          ref currentLine, lineBoxes, ref maxColumnWidth);

                    var fragment = new LineFragment
                    {
                        X = cursorX - currentLine.X,
                        Y = 0,
                        Width = lineHeight,
                        Height = textWidth,
                        Baseline = fontSize * 0.8f,
                        Text = text,
                        ShapedRun = shaped,
                        InlineElement = inlineAncestor
                    };
                    currentLine.AddFragment(fragment);
                    cursorY += textWidth;
                    if (lineHeight > maxColumnWidth) maxColumnWidth = lineHeight;
                }
            }
            else
            {
                if (float.IsNaN(lineHeight) || lineHeight <= 0)
                    lineHeight = fontSize * 1.2f;

                // Fallback: estimate text extent
                float charWidth = fontSize * 0.6f;
                float textExtent = text.Length * charWidth;

                if (cursorY + textExtent > startY + containingHeight)
                {
                    StartNewVerticalLine(parent, ref cursorX, ref cursorY, startY, containingHeight,
                                          ref currentLine, lineBoxes, ref maxColumnWidth);
                }

                var fragment = new LineFragment
                {
                    X = cursorX - currentLine.X,
                    Y = cursorY - startY,
                    Width = lineHeight,
                    Height = textExtent,
                    Baseline = fontSize * 0.8f,
                    Text = text,
                    InlineElement = inlineAncestor
                };
                currentLine.AddFragment(fragment);
                cursorY += textExtent;
                if (lineHeight > maxColumnWidth) maxColumnWidth = lineHeight;
            }
        }

        /// <summary>
        /// Start a new vertical line (column) when the current column overflows
        /// in the inline direction.
        /// </summary>
        private static void StartNewVerticalLine(LayoutBox parent, ref float cursorX, ref float cursorY,
            float startY, float containingHeight,
            ref LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxColumnWidth)
        {
            FinalizeVerticalLineBox(currentLine, maxColumnWidth);
            lineBoxes.Add(currentLine);

            // Advance in the block direction (horizontal)
            cursorX += maxColumnWidth;
            cursorY = startY;
            currentLine = new LineBox { X = cursorX, Y = startY, Width = containingHeight, IsVertical = true };
            maxColumnWidth = 0;
        }

        /// <summary>
        /// Finalize a vertical line box by setting its dimensions.
        /// </summary>
        private static void FinalizeVerticalLineBox(LineBox line, float columnWidth)
        {
            // In vertical mode, "Height" is the extent in the inline direction (vertical),
            // and "Width" is the column width (one line-height).
            if (columnWidth > 0) line.Height = columnWidth;
            if (line.Height <= 0) line.Height = 16f;

            // Calculate actual vertical content extent
            float maxBottom = 0;
            for (int i = 0; i < line.Fragments.Count; i++)
            {
                var frag = line.Fragments[i];
                float bottom = frag.Y + frag.Height;
                if (bottom > maxBottom) maxBottom = bottom;
            }
            // The Width of a vertical line box is the column width
            line.Width = columnWidth > 0 ? columnWidth : 16f;
            // The Height is the max extent in the inline (vertical) direction
            if (maxBottom > 0) line.Height = maxBottom;
        }

        private static void LayoutTextRun(
            StyledText textNode, LayoutContext context,
            ref float cursorX, ref float cursorY, ref float startX, ref float containingWidth,
            ref LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline, LayoutBox parent,
            StyledElement? inlineAncestor = null, ComputedStyle? styleOverride = null)
        {
            var style = textNode.Style;
            string text = textNode.Text;

            // Apply white-space processing
            text = WhitespaceCollapser.Collapse(text, style.WhiteSpace);
            if (string.IsNullOrEmpty(text)) return;

            // Expand tab characters in pre/pre-wrap modes using tab-size property.
            if (text.IndexOf('\t') >= 0 &&
                (style.WhiteSpace == CssWhiteSpace.Pre ||
                 style.WhiteSpace == CssWhiteSpace.PreWrap ||
                 style.WhiteSpace == CssWhiteSpace.BreakSpaces))
            {
                int tabSize = (int)style.TabSize;
                if (tabSize <= 0) tabSize = 8;
                text = text.Replace("\t", new string(' ', tabSize));
            }

            // Apply text-transform
            text = TextTransformer.Transform(text, style.TextTransform);

            // Apply font-variant: small-caps by converting to uppercase.
            // This is the standard fallback when the font doesn't have small-caps glyphs.
            if (style.FontVariant == CssFontVariant.SmallCaps)
            {
                text = text.ToUpperInvariant();
            }

            // Apply hyphens: none — strip soft hyphens to prevent break opportunities.
            if (style.Hyphens == CssHyphens.None && text.IndexOf('\u00AD') >= 0)
            {
                text = text.Replace("\u00AD", string.Empty);
                if (string.IsNullOrEmpty(text)) return;
            }

            // For white-space modes that preserve newlines, split on \n and process
            // each segment separately with forced line breaks between them.
            bool preservesNewlines = style.WhiteSpace == CssWhiteSpace.Pre ||
                                     style.WhiteSpace == CssWhiteSpace.PreWrap ||
                                     style.WhiteSpace == CssWhiteSpace.PreLine ||
                                     style.WhiteSpace == CssWhiteSpace.BreakSpaces;

            if (preservesNewlines && text.IndexOf('\n') >= 0)
            {
                string[] segments = text.Split('\n');
                for (int seg = 0; seg < segments.Length; seg++)
                {
                    if (segments[seg].Length > 0)
                    {
                        var segText = new StyledText(segments[seg], style);
                        LayoutTextRunSegment(segText, context, ref cursorX, ref cursorY, ref startX, ref containingWidth,
                                             ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent,
                                             inlineAncestor, styleOverride);
                    }

                    // Force a line break after each segment except the last
                    if (seg < segments.Length - 1)
                    {
                        StartNewLine(parent, ref cursorX, ref cursorY, ref startX, ref containingWidth,
                                     ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, context);
                    }
                }
                return;
            }

            // Create a processed text node (whitespace collapsed, text-transform applied)
            var processedNode = new StyledText(text, style);
            LayoutTextRunSegment(processedNode, context, ref cursorX, ref cursorY, ref startX, ref containingWidth,
                                 ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent,
                                 inlineAncestor, styleOverride);
        }

        /// <summary>
        /// Lays out a single segment of text (no embedded newlines) within an inline formatting context.
        /// </summary>
        private static void LayoutTextRunSegment(
            StyledText textNode, LayoutContext context,
            ref float cursorX, ref float cursorY, ref float startX, ref float containingWidth,
            ref LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline, LayoutBox parent,
            StyledElement? inlineAncestor = null, ComputedStyle? styleOverride = null)
        {
            var style = textNode.Style;
            string text = textNode.Text;

            // CSS Text Level 3 §4.1.1: strip leading whitespace at line start
            // and collapse adjacent spaces across inline element boundaries.
            bool isNormalWs = style.WhiteSpace == CssWhiteSpace.Normal ||
                              style.WhiteSpace == CssWhiteSpace.Nowrap;
            if (isNormalWs && text.Length > 0 && text[0] == ' ')
            {
                if (Math.Abs(cursorX - startX) < 0.01f)
                {
                    // At start of line — strip leading space
                    text = text.TrimStart(' ');
                    if (text.Length == 0) return;
                }
                else if (currentLine.Fragments.Count > 0)
                {
                    // Collapse space across inline boundaries
                    var lastFrag = currentLine.Fragments[currentLine.Fragments.Count - 1];
                    if (lastFrag.Text != null && lastFrag.Text.Length > 0 &&
                        lastFrag.Text[lastFrag.Text.Length - 1] == ' ')
                    {
                        text = text.TrimStart(' ');
                        if (text.Length == 0) return;
                    }
                }
            }

            // Rebuild textNode with cleaned text
            if (text != textNode.Text)
                textNode = new StyledText(text, style);

            float fontSize = style.FontSize;
            float lineHeight = style.LineHeight;
            float ascent = fontSize * 0.8f;
            bool isNormalLineHeight = float.IsNaN(lineHeight) || lineHeight == 0;

            // Negative = unitless multiplier, positive = pixels, NaN = normal
            if (lineHeight < 0)
                lineHeight = -lineHeight * fontSize;
            else if (isNormalLineHeight)
                lineHeight = fontSize * 1.2f;

            bool noWrap = style.WhiteSpace == CssWhiteSpace.Nowrap ||
                          style.WhiteSpace == CssWhiteSpace.Pre;

            if (context.TextMeasurer != null)
            {
                var fontDesc = new FontDescriptor(
                    style.FontFamily ?? "serif",
                    style.FontWeight,
                    style.FontStyle,
                    FontDescriptor.StretchToPercentage(style.FontStretch));

                ascent = context.TextMeasurer.GetAscent(fontDesc, fontSize);

                // Use actual font metrics for "normal" line-height
                if (isNormalLineHeight)
                {
                    float metricsLineHeight = context.TextMeasurer.GetNormalLineHeight(fontDesc, fontSize);
                    if (!float.IsNaN(metricsLineHeight) && metricsLineHeight > 0)
                        lineHeight = metricsLineHeight;
                }

                // CSS half-leading: the baseline position from the top of the line box
                // includes half the leading (space distributed equally above and below text).
                // Content area = ascent + descent (from font metrics).
                // leading = lineHeight - contentArea; halfLeading = leading / 2
                float descent = context.TextMeasurer.GetDescent(fontDesc, fontSize);
                float contentArea = ascent + descent;
                ascent += (lineHeight - contentArea) / 2f;

                // Strip soft hyphens from display text (invisible unless at a break point)
                string displayText = text;
                if (style.Hyphens != CssHyphens.None && text.IndexOf('\u00AD') >= 0)
                    displayText = text.Replace("\u00AD", string.Empty);

                // Shape and measure
                var shaped = context.TextMeasurer.Shape(displayText, fontDesc, fontSize);

                // Add extra width for letter-spacing and word-spacing
                float adjustedWidth = shaped.TotalWidth + CalculateSpacingExtra(displayText, style);

                if (cursorX + adjustedWidth <= startX + containingWidth || noWrap)
                {
                    // Fits on current line (or no-wrap mode)
                    AddTextFragment(currentLine, displayText, shaped, cursorX, adjustedWidth, lineHeight, ascent, inlineAncestor, styleOverride, contentArea);
                    cursorX += adjustedWidth;
                    UpdateLineMetrics(ref maxLineHeight, ref lineBaseline, lineHeight, ascent);
                }
                else
                {
                    // Need to wrap: split text at word boundaries
                    WrapText(text, fontDesc, fontSize, context, ref cursorX, ref cursorY, ref startX,
                             ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline,
                             lineHeight, ascent, parent, inlineAncestor, style.LetterSpacing, style.WordSpacing,
                             style.WordBreak, style.OverflowWrap, style.Hyphens, style.WhiteSpace);
                }
            }
            else
            {
                // Fallback line-height when no font metrics available
                if (float.IsNaN(lineHeight) || lineHeight <= 0)
                    lineHeight = fontSize * 1.2f;

                // CSS half-leading for fallback path
                ascent += (lineHeight - fontSize) / 2f;

                // Fallback: estimate
                float charWidth = fontSize * 0.6f;
                float textWidth = text.Length * charWidth;

                if (cursorX + textWidth > startX + containingWidth && !noWrap)
                {
                    // Wrap at word boundaries
                    WrapTextSimple(text, charWidth, ref cursorX, ref cursorY, startX, containingWidth,
                                   ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline,
                                   lineHeight, ascent, parent, inlineAncestor);
                }
                else
                {
                    var fragment = new LineFragment
                    {
                        X = cursorX - currentLine.X,
                        Width = textWidth,
                        Height = lineHeight,
                        ContentHeight = fontSize,
                        Baseline = ascent,
                        Text = text,
                        InlineElement = inlineAncestor,
                        StyleOverride = styleOverride
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
            ref float cursorX, ref float cursorY, ref float startX, ref float containingWidth,
            ref LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline,
            float lineHeight, float ascent, LayoutBox parent,
            StyledElement? inlineAncestor = null, float letterSpacing = 0f, float wordSpacing = 0f,
            CssWordBreak wordBreak = CssWordBreak.Normal,
            CssOverflowWrap overflowWrap = CssOverflowWrap.Normal,
            CssHyphens hyphens = CssHyphens.Manual,
            CssWhiteSpace whiteSpace = CssWhiteSpace.Normal)
        {
            // For break-all, every character boundary is a break opportunity
            if (wordBreak == CssWordBreak.BreakAll)
            {
                WrapTextBreakAll(text, fontDesc, fontSize, context, ref cursorX, ref cursorY, ref startX,
                                 ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline,
                                 lineHeight, ascent, parent, inlineAncestor, letterSpacing, wordSpacing);
                return;
            }

            // Find break opportunities
            var breaker = new LineBreaker();
            var breaks = breaker.FindBreaks(text);

            // keep-all: suppress CJK break opportunities
            if (wordBreak == CssWordBreak.KeepAll)
            {
                for (int k = 0; k < breaks.Length; k++)
                {
                    if (breaks[k] == LineBreakOpportunity.Allowed)
                    {
                        char c = text[k];
                        char n = text[k + 1];
                        if (IsCjk(c) || IsCjk(n))
                            breaks[k] = LineBreakOpportunity.Forbidden;
                    }
                }
            }

            int wordStart = 0;
            bool hasSoftHyphens = hyphens != CssHyphens.None && text.IndexOf('\u00AD') >= 0;

            // Accumulate consecutive words into a single fragment per line.
            // This prevents word spacing mismatch between HarfBuzz layout and Skia rendering
            // by letting Skia handle intra-line glyph spacing as one continuous string.
            int lineTextStart = 0;       // Start index of accumulated text for current line fragment
            float lineFragStartX = cursorX; // X position where the accumulated fragment starts
            float accumulatedWidth = 0;  // Total width of accumulated words (HarfBuzz-measured)

            for (int j = 0; j < text.Length; j++)
            {
                if (j < breaks.Length && breaks[j] == LineBreakOpportunity.Allowed || j == text.Length - 1)
                {
                    int end = j == text.Length - 1 ? text.Length : j + 1;
                    string word = text.Substring(wordStart, end - wordStart);

                    // Strip soft hyphens from display text (they're invisible unless at a break point)
                    string displayWord = hasSoftHyphens ? word.Replace("\u00AD", string.Empty) : word;
                    var shaped = context.TextMeasurer!.Shape(displayWord, fontDesc, fontSize);
                    float wordWidth = shaped.TotalWidth + CalculateSpacingExtraRaw(displayWord, letterSpacing, wordSpacing);

                    // Compute the full-line width by reshaping accumulated text + this word as
                    // one string.  This avoids accumulated rounding error from summing individual
                    // word widths (HarfBuzz can produce different advances for isolated words vs.
                    // words shaped in context, and float accumulation magnifies the error).
                    float candidateWidth;
                    {
                        string lineCandidate = text.Substring(lineTextStart, end - lineTextStart);
                        if (hasSoftHyphens) lineCandidate = lineCandidate.Replace("\u00AD", string.Empty);
                        if (lineCandidate.Length > 0)
                        {
                            // CSS Text Level 3 §4.1.3: In pre-wrap, trailing whitespace hangs
                            // and does not contribute to the line width for wrapping decisions.
                            string measureCandidate = lineCandidate;
                            bool hangTrailingWs = whiteSpace == CssWhiteSpace.PreWrap;
                            if (hangTrailingWs)
                                measureCandidate = lineCandidate.TrimEnd(' ');
                            if (measureCandidate.Length > 0)
                            {
                                var candShape = context.TextMeasurer!.Shape(measureCandidate, fontDesc, fontSize);
                                candidateWidth = candShape.TotalWidth + CalculateSpacingExtraRaw(measureCandidate, letterSpacing, wordSpacing);
                            }
                            else
                            {
                                candidateWidth = 0; // all whitespace — hangs
                            }
                        }
                        else
                        {
                            candidateWidth = accumulatedWidth + wordWidth;
                        }
                    }

                    bool wordHandled = false;
                    if (lineFragStartX + candidateWidth > startX + containingWidth && (currentLine.Fragments.Count > 0 || accumulatedWidth > 0))
                    {
                        // Try auto-hyphenation when hyphens: auto is set
                        if (hyphens == CssHyphens.Auto && displayWord.Length >= 4)
                        {
                            // First flush accumulated text before hyphenation
                            if (lineTextStart < wordStart)
                            {
                                FlushAccumulatedText(text, lineTextStart, wordStart, hasSoftHyphens, fontDesc, fontSize,
                                    context, currentLine, lineFragStartX, accumulatedWidth, lineHeight, ascent,
                                    inlineAncestor, letterSpacing, wordSpacing);
                                cursorX = lineFragStartX + accumulatedWidth;
                                lineTextStart = wordStart;
                                lineFragStartX = cursorX;
                                accumulatedWidth = 0;
                            }

                            wordHandled = TryAutoHyphenate(displayWord, fontDesc, fontSize, context,
                                ref cursorX, ref cursorY, ref startX, ref containingWidth,
                                ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline,
                                lineHeight, ascent, parent, inlineAncestor, letterSpacing, wordSpacing);
                            if (wordHandled)
                            {
                                lineTextStart = end;
                                lineFragStartX = cursorX;
                                accumulatedWidth = 0;
                            }
                        }

                        if (!wordHandled)
                        {
                            // Flush accumulated text as a single fragment before line break
                            if (lineTextStart < wordStart)
                            {
                                FlushAccumulatedText(text, lineTextStart, wordStart, hasSoftHyphens, fontDesc, fontSize,
                                    context, currentLine, lineFragStartX, accumulatedWidth, lineHeight, ascent,
                                    inlineAncestor, letterSpacing, wordSpacing);
                                UpdateLineMetrics(ref maxLineHeight, ref lineBaseline, lineHeight, ascent);
                            }

                            // Start new line
                            StartNewLine(parent, ref cursorX, ref cursorY, ref startX, ref containingWidth,
                                         ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, context);

                            lineTextStart = wordStart;
                            lineFragStartX = cursorX;
                            accumulatedWidth = 0;

                            // Recompute candidateWidth for the new line — the old value
                            // included text from the previous line (e.g. "underlined blue"
                            // instead of just "blue") causing a desynchronized cursor.
                            {
                                string newLineCandidate = text.Substring(lineTextStart, end - lineTextStart);
                                if (hasSoftHyphens) newLineCandidate = newLineCandidate.Replace("\u00AD", string.Empty);
                                if (newLineCandidate.Length > 0)
                                {
                                    string nlMeasure = newLineCandidate;
                                    if (whiteSpace == CssWhiteSpace.PreWrap)
                                        nlMeasure = newLineCandidate.TrimEnd(' ');
                                    if (nlMeasure.Length > 0)
                                    {
                                        var nlShape = context.TextMeasurer!.Shape(nlMeasure, fontDesc, fontSize);
                                        candidateWidth = nlShape.TotalWidth + CalculateSpacingExtraRaw(nlMeasure, letterSpacing, wordSpacing);
                                    }
                                    else
                                    {
                                        candidateWidth = 0;
                                    }
                                }
                                else
                                {
                                    candidateWidth = wordWidth;
                                }
                            }
                        }
                    }

                    if (!wordHandled)
                    {
                        // break-word fallback: if the word still doesn't fit on an empty line, break it character by character
                        bool allowCharBreak = wordBreak == CssWordBreak.BreakWord ||
                                              overflowWrap == CssOverflowWrap.BreakWord ||
                                              overflowWrap == CssOverflowWrap.Anywhere;
                        if (allowCharBreak &&
                            lineFragStartX + candidateWidth > startX + containingWidth && currentLine.Fragments.Count == 0 && accumulatedWidth == 0)
                        {
                            WrapTextBreakAll(displayWord, fontDesc, fontSize, context, ref cursorX, ref cursorY, ref startX,
                                             ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline,
                                             lineHeight, ascent, parent, inlineAncestor, letterSpacing, wordSpacing);
                            lineTextStart = end;
                            lineFragStartX = cursorX;
                            accumulatedWidth = 0;
                        }
                        else
                        {
                            // Accumulate this word — use the full-line candidateWidth for accuracy
                            accumulatedWidth = candidateWidth;
                            cursorX = lineFragStartX + accumulatedWidth;
                            UpdateLineMetrics(ref maxLineHeight, ref lineBaseline, lineHeight, ascent);
                        }
                    }
                    wordStart = end;
                }
            }

            // Flush any remaining accumulated text as a single fragment
            if (lineTextStart < text.Length && accumulatedWidth > 0)
            {
                FlushAccumulatedText(text, lineTextStart, text.Length, hasSoftHyphens, fontDesc, fontSize,
                    context, currentLine, lineFragStartX, accumulatedWidth, lineHeight, ascent,
                    inlineAncestor, letterSpacing, wordSpacing);
            }
        }

        /// <summary>
        /// Emits accumulated text [textStart..textEnd) as a single shaped fragment.
        /// By shaping the entire line segment as one string, we ensure Skia's DrawText
        /// handles inter-word spacing consistently with its own glyph metrics.
        /// </summary>
        private static void FlushAccumulatedText(
            string fullText, int textStart, int textEnd, bool hasSoftHyphens,
            FontDescriptor fontDesc, float fontSize, LayoutContext context,
            LineBox currentLine, float fragX, float totalWidth,
            float lineHeight, float ascent, StyledElement? inlineAncestor,
            float letterSpacing, float wordSpacing)
        {
            string segment = fullText.Substring(textStart, textEnd - textStart);
            if (hasSoftHyphens)
                segment = segment.Replace("\u00AD", string.Empty);
            if (segment.Length == 0) return;

            var shaped = context.TextMeasurer!.Shape(segment, fontDesc, fontSize);
            // Use the shaped width for the combined segment (more accurate than sum of word widths)
            float segmentWidth = shaped.TotalWidth + CalculateSpacingExtraRaw(segment, letterSpacing, wordSpacing);

            AddTextFragment(currentLine, segment, shaped, fragX, segmentWidth, lineHeight, ascent, inlineAncestor);
        }

        /// <summary>
        /// Attempts to break a word using dictionary-based auto-hyphenation.
        /// If successful, the first part (with trailing "-") is placed on the current line
        /// and the remainder is placed on a new line.
        /// Returns true if hyphenation was applied.
        /// </summary>
        private static bool TryAutoHyphenate(
            string word, FontDescriptor fontDesc, float fontSize,
            LayoutContext context,
            ref float cursorX, ref float cursorY, ref float startX, ref float containingWidth,
            ref LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline,
            float lineHeight, float ascent, LayoutBox parent,
            StyledElement? inlineAncestor, float letterSpacing, float wordSpacing)
        {
            // Extract only the alphabetic portion for dictionary lookup (strip leading/trailing punctuation/spaces)
            int alphaStart = 0;
            int alphaEnd = word.Length;
            while (alphaStart < word.Length && !char.IsLetter(word[alphaStart]))
                alphaStart++;
            while (alphaEnd > alphaStart && !char.IsLetter(word[alphaEnd - 1]))
                alphaEnd--;

            if (alphaEnd - alphaStart < 4)
                return false; // Too short to hyphenate meaningfully

            string alphaWord = word.Substring(alphaStart, alphaEnd - alphaStart);
            var dict = GetHyphenationDictionary();
            var hyphenPoints = dict.FindHyphenPoints(alphaWord);

            if (hyphenPoints.Length == 0)
                return false;

            // Measure the hyphen character width
            float hyphenCharWidth = context.TextMeasurer!.MeasureWidth("-", fontDesc, fontSize);
            float availableWidth = startX + containingWidth - cursorX;

            // Find the best (rightmost) hyphen point that fits on the current line.
            // hyphenPoints[i] means we can split after alphaWord[i], so the prefix is alphaWord[0..i+1]
            int bestSplit = -1;
            for (int i = hyphenPoints.Length - 1; i >= 0; i--)
            {
                if (!hyphenPoints[i]) continue;

                // The split in the original word: alphaStart + i + 1 chars of alpha portion
                int splitInWord = alphaStart + i + 1;
                string prefix = word.Substring(0, splitInWord) + "-";
                float prefixWidth = context.TextMeasurer.MeasureWidth(prefix, fontDesc, fontSize)
                    + CalculateSpacingExtraRaw(prefix, letterSpacing, wordSpacing);

                if (prefixWidth <= availableWidth)
                {
                    bestSplit = splitInWord;
                    break;
                }
            }

            if (bestSplit <= 0)
                return false;

            // Place the hyphenated prefix on the current line
            string firstPart = word.Substring(0, bestSplit) + "-";
            var firstShaped = context.TextMeasurer.Shape(firstPart, fontDesc, fontSize);
            float firstWidth = firstShaped.TotalWidth + CalculateSpacingExtraRaw(firstPart, letterSpacing, wordSpacing);
            AddTextFragment(currentLine, firstPart, firstShaped, cursorX, firstWidth, lineHeight, ascent, inlineAncestor);
            cursorX += firstWidth;
            UpdateLineMetrics(ref maxLineHeight, ref lineBaseline, lineHeight, ascent);

            // Start a new line for the remainder
            StartNewLine(parent, ref cursorX, ref cursorY, ref startX, ref containingWidth,
                         ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, context);

            // Place the remainder on the new line
            string secondPart = word.Substring(bestSplit);
            if (secondPart.Length > 0)
            {
                var secondShaped = context.TextMeasurer.Shape(secondPart, fontDesc, fontSize);
                float secondWidth = secondShaped.TotalWidth + CalculateSpacingExtraRaw(secondPart, letterSpacing, wordSpacing);
                AddTextFragment(currentLine, secondPart, secondShaped, cursorX, secondWidth, lineHeight, ascent, inlineAncestor);
                cursorX += secondWidth;
                UpdateLineMetrics(ref maxLineHeight, ref lineBaseline, lineHeight, ascent);
            }

            return true;
        }

        private static void WrapTextBreakAll(
            string text, FontDescriptor fontDesc, float fontSize,
            LayoutContext context,
            ref float cursorX, ref float cursorY, ref float startX, ref float containingWidth,
            ref LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline,
            float lineHeight, float ascent, LayoutBox parent,
            StyledElement? inlineAncestor, float letterSpacing, float wordSpacing)
        {
            // Batch consecutive characters that fit on the same line into a single
            // text fragment to preserve proper kerning and avoid per-character spacing artifacts.
            int batchStart = 0;
            float batchWidth = 0;
            float batchX = cursorX;

            for (int i = 0; i < text.Length; i++)
            {
                int charLen = char.IsHighSurrogate(text[i]) && i + 1 < text.Length ? 2 : 1;
                string ch = text.Substring(i, charLen);
                var charShaped = context.TextMeasurer!.Shape(ch, fontDesc, fontSize);
                float charWidth = charShaped.TotalWidth;
                if (letterSpacing != 0 && i > 0) charWidth += letterSpacing;
                if (wordSpacing != 0 && ch == " ") charWidth += wordSpacing;

                // Check if adding this char would overflow the line.
                // Allow wrap if there's already content (fragments or pending batch).
                bool hasContent = currentLine.Fragments.Count > 0 || i > batchStart;
                if (cursorX + charWidth > startX + containingWidth && hasContent)
                {
                    // Flush accumulated batch before wrapping
                    if (i > batchStart)
                    {
                        FlushBreakAllBatch(text, batchStart, i, fontDesc, fontSize, context,
                            batchX, batchWidth, lineHeight, ascent, currentLine, inlineAncestor);
                    }
                    StartNewLine(parent, ref cursorX, ref cursorY, ref startX, ref containingWidth,
                                 ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, context);
                    batchStart = i;
                    batchWidth = 0;
                    batchX = cursorX;
                }

                batchWidth += charWidth;
                cursorX += charWidth;
                UpdateLineMetrics(ref maxLineHeight, ref lineBaseline, lineHeight, ascent);

                if (charLen == 2) i++; // skip second surrogate
            }

            // Flush remaining batch
            if (text.Length > batchStart)
            {
                FlushBreakAllBatch(text, batchStart, text.Length, fontDesc, fontSize, context,
                    batchX, batchWidth, lineHeight, ascent, currentLine, inlineAncestor);
            }
        }

        private static void FlushBreakAllBatch(
            string text, int start, int end, FontDescriptor fontDesc, float fontSize,
            LayoutContext context, float x, float width,
            float lineHeight, float ascent, LineBox currentLine, StyledElement? inlineAncestor)
        {
            string batchText = text.Substring(start, end - start);
            var shaped = context.TextMeasurer!.Shape(batchText, fontDesc, fontSize);
            AddTextFragment(currentLine, batchText, shaped, x, width, lineHeight, ascent, inlineAncestor);
        }

        private static void StartNewLine(LayoutBox parent, ref float cursorX, ref float cursorY,
            ref float startX, ref float containingWidth,
            ref LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline,
            LayoutContext? context = null)
        {
            var parentStyle = parent.StyledNode as StyledElement;
            var align = parentStyle?.Style.TextAlign ?? CssTextAlign.Left;
            var dir = parentStyle?.Style.Direction ?? CssDirection.Ltr;
            FinalizeLineBox(currentLine, maxLineHeight, lineBaseline, align, CssTextAlign.Auto, dir);
            lineBoxes.Add(currentLine);
            cursorY += maxLineHeight;

            // Re-query float context for the new line's Y position
            var floatCtx = context?.FloatContext;
            if (floatCtx != null)
            {
                float baseX = parent.ContentRect.X;
                float baseWidth = parent.ContentRect.Width;
                float leftEdge = floatCtx.GetLeftEdge(cursorY, 0);
                float rightEdge = floatCtx.GetRightEdge(cursorY, 0);
                startX = Math.Max(baseX, leftEdge);
                // Clamp right edge to the parent's content area to avoid
                // using the float context's wider containing block width.
                float rightLimit = baseX + baseWidth;
                containingWidth = Math.Min(rightEdge, rightLimit) - startX;
            }

            currentLine = new LineBox { X = startX, Y = cursorY, Width = containingWidth };
            cursorX = startX;
            maxLineHeight = 0;
            lineBaseline = 0;
        }

        private static void WrapTextSimple(
            string text, float charWidth,
            ref float cursorX, ref float cursorY, float startX, float containingWidth,
            ref LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline,
            float lineHeight, float ascent, LayoutBox parent,
            StyledElement? inlineAncestor = null)
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
                    var dir = parentStyle?.Style.Direction ?? CssDirection.Ltr;
                    FinalizeLineBox(currentLine, maxLineHeight, lineBaseline, align, CssTextAlign.Auto, dir);
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
                    X = cursorX - currentLine.X,
                    Width = wordWidth,
                    Height = lineHeight,
                    Baseline = ascent,
                    Text = word,
                    InlineElement = inlineAncestor
                };
                currentLine.AddFragment(fragment);
                cursorX += wordWidth;
                UpdateLineMetrics(ref maxLineHeight, ref lineBaseline, lineHeight, ascent);
            }
        }

        private static void LayoutInlineBlock(
            StyledElement element, LayoutContext context,
            ref float cursorX, ref float cursorY, float startX, float containingWidth,
            ref LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline, LayoutBox parent)
        {
            var box = new LayoutBox(element, BoxType.InlineBlock);
            BoxModelCalculator.ApplyBoxModel(box, element.Style, containingWidth);

            float contentWidth;
            float contentHeight = 0;

            if (ReplacedElementLayout.IsReplaced(element))
            {
                // Replaced elements (img, svg, form controls, etc.): use intrinsic/attribute dimensions
                float intrinsicW = 0;
                float intrinsicH = 0;
                string? attrW = element.GetAttribute("width");
                string? attrH = element.GetAttribute("height");
                if (attrW != null && float.TryParse(attrW, out float aw)) intrinsicW = aw;
                if (attrH != null && float.TryParse(attrH, out float ah)) intrinsicH = ah;
                // Form controls: apply default intrinsic dimensions if no attributes set
                if (ReplacedElementLayout.IsFormControl(element))
                {
                    if (intrinsicW <= 0) intrinsicW = ReplacedElementLayout.GetFormControlIntrinsicWidth(element, context.TextMeasurer);
                    if (intrinsicH <= 0) intrinsicH = ReplacedElementLayout.GetFormControlIntrinsicHeight(element);
                }

                // MathML: measure content for intrinsic dimensions
                if (element.TagName == "math" && intrinsicW <= 0)
                {
                    var mathSize = Rendering.Internal.MathmlRenderer.MeasureElement(
                        element.Element, 16f);
                    if (mathSize.Width > 0) intrinsicW = mathSize.Width + 4f;
                    if (mathSize.Height > 0) intrinsicH = mathSize.Height;
                }

                // Resolve CSS width (handles percentages encoded as negative fractions)
                float specW = element.Style.Width;
                if (specW < 0 && specW > -1.01f)
                    contentWidth = DimensionResolver.ResolveWidth(element.Style, containingWidth, box);
                else if (float.IsNaN(specW))
                    contentWidth = intrinsicW;
                else
                    contentWidth = specW;

                // Resolve CSS height
                float specH = element.Style.Height;
                float tempH;
                if (specH < 0 && specH > -1.01f)
                    tempH = DimensionResolver.ResolveHeight(element.Style, float.NaN, box);
                else if (float.IsNaN(specH))
                    tempH = intrinsicH;
                else
                    tempH = specH;
                if (float.IsNaN(tempH)) tempH = intrinsicH;

                box.ContentRect = new RectF(0, 0, contentWidth, tempH);
                ReplacedElementLayout.ResolveDimensions(box, element.Style, containingWidth, intrinsicW, intrinsicH);
                contentWidth = box.ContentRect.Width;
                contentHeight = box.ContentRect.Height;
            }
            else if (SizingKeyword.IsSizingKeyword(element.Style.Width))
            {
                contentWidth = BlockFormattingContext.MeasureIntrinsicWidth(element, element.Style.Width, containingWidth, context);
            }
            else
            {
                contentWidth = DimensionResolver.ResolveWidth(element.Style, containingWidth, box);
            }

            // Inline-level boxes with auto width should shrink-to-fit
            bool needsShrinkToFit = float.IsNaN(element.Style.Width) &&
                !ReplacedElementLayout.IsReplaced(element) &&
                (element.Style.Display == CssDisplay.InlineBlock ||
                 element.Style.Display == CssDisplay.InlineFlex ||
                 element.Style.Display == CssDisplay.InlineGrid);

            // For shrink-to-fit boxes, do a preliminary layout to measure actual content width
            // BEFORE the line wrapping check. This ensures inline-flex/inline-grid containers
            // use their true shrink-to-fit width for line break decisions.
            if (needsShrinkToFit)
            {
                var measureBox = new LayoutBox(element, BoxType.InlineBlock);
                BoxModelCalculator.ApplyBoxModel(measureBox, element.Style, containingWidth);
                measureBox.ContentRect = new RectF(0, 0, contentWidth, 0);
                var prevFloatM = context.FloatContext;
                context.FloatContext = null;
                BlockFormattingContext.LayoutChildren(measureBox, context);
                context.FloatContext = prevFloatM;
                float measuredWidth = MeasureContentWidth(measureBox);
                if (measuredWidth > 0 && measuredWidth < contentWidth)
                    contentWidth = measuredWidth;
            }

            float totalWidth = contentWidth + box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth;

            if (cursorX + totalWidth > startX + containingWidth && currentLine.Fragments.Count > 0)
            {
                var parentStyle = parent.StyledNode as StyledElement;
                var align = parentStyle?.Style.TextAlign ?? CssTextAlign.Left;
                var dir = parentStyle?.Style.Direction ?? CssDirection.Ltr;
                FinalizeLineBox(currentLine, maxLineHeight, lineBaseline, align, CssTextAlign.Auto, dir);
                lineBoxes.Add(currentLine);
                cursorY += maxLineHeight;
                currentLine = new LineBox { X = startX, Y = cursorY, Width = containingWidth };
                cursorX = startX;
                maxLineHeight = 0;
                lineBaseline = 0;
            }

            if (ReplacedElementLayout.IsReplaced(element))
            {
                box.ContentRect = new RectF(cursorX + box.MarginLeft + box.BorderLeftWidth + box.PaddingLeft,
                                            cursorY, contentWidth, contentHeight);
            }
            else
            {
                box.ContentRect = new RectF(cursorX + box.MarginLeft + box.BorderLeftWidth + box.PaddingLeft,
                                            cursorY, contentWidth, 0);

                // Layout contents (dispatch based on display type: flex, grid, table, etc.)
                BlockFormattingContext.LayoutChildren(box, context);
                contentHeight = DimensionResolver.ResolveHeight(element.Style, float.NaN, box);
                if (float.IsNaN(contentHeight))
                    contentHeight = CalculateContentHeight(box);

                box.ContentRect = new RectF(box.ContentRect.X, cursorY, contentWidth, contentHeight);
            }

            float totalHeight = contentHeight + box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth;

            // Compute baseline for inline-block: use last line box baseline if available,
            // otherwise fall back to bottom margin edge (CSS 2.1 §10.8.1)
            float fragmentBaseline = totalHeight;
            var overflow = element.Style.OverflowY;
            if (overflow == CssOverflow.Visible || overflow == CssOverflow.Auto)
            {
                float? lastLineBaseline = FindLastLineBaseline(box);
                if (lastLineBaseline.HasValue)
                    fragmentBaseline = lastLineBaseline.Value + box.PaddingTop + box.BorderTopWidth + box.MarginTop;
            }

            var fragment = new LineFragment
            {
                X = cursorX - currentLine.X,
                Width = totalWidth + box.MarginLeft + box.MarginRight,
                Height = totalHeight + box.MarginTop + box.MarginBottom,
                Baseline = fragmentBaseline,
                Box = box
            };
            currentLine.AddFragment(fragment);
            parent.AddChild(box);

            cursorX += fragment.Width;
            UpdateLineMetrics(ref maxLineHeight, ref lineBaseline, fragment.Height, fragment.Baseline);
        }

        /// <summary>
        /// Lay out a ruby container element (display: ruby).
        /// Extracts base text and annotation text from children, lays out base text
        /// as inline content, and attaches ruby annotation to the fragments.
        /// </summary>
        private static void LayoutRubyContainer(
            StyledElement rubyElement, LayoutContext context,
            ref float cursorX, ref float cursorY, ref float startX, ref float containingWidth,
            ref LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline, LayoutBox parent)
        {
            // Collect base text and annotation text from ruby children
            string baseText = "";
            string annotationText = "";
            ComputedStyle? annotationStyle = null;
            bool rubyBelow = rubyElement.Style.RubyPosition == CssRubyPosition.Under;

            for (int i = 0; i < rubyElement.Children.Count; i++)
            {
                var child = rubyElement.Children[i];

                if (child.IsText)
                {
                    // Direct text children are base text
                    var text = ((StyledText)child).Text;
                    if (!string.IsNullOrWhiteSpace(text))
                        baseText += text.Trim();
                }
                else if (child is StyledElement childEl)
                {
                    if (childEl.Style.Display == CssDisplay.None) continue;

                    if (childEl.Style.Display == CssDisplay.RubyText)
                    {
                        // Collect annotation text from <rt> children
                        annotationStyle = childEl.Style;
                        annotationText = ExtractTextContent(childEl);
                    }
                    else if (childEl.Style.Display == CssDisplay.RubyBase)
                    {
                        // Collect base text from <rb> children
                        baseText += ExtractTextContent(childEl);
                    }
                    // RubyTextContainer is handled via its rt children
                    else if (childEl.Style.Display == CssDisplay.RubyTextContainer)
                    {
                        for (int j = 0; j < childEl.Children.Count; j++)
                        {
                            if (childEl.Children[j] is StyledElement rtChild &&
                                rtChild.Style.Display == CssDisplay.RubyText)
                            {
                                annotationStyle = rtChild.Style;
                                annotationText = ExtractTextContent(rtChild);
                            }
                        }
                    }
                    // rp elements have display:none in UA stylesheet, but handle fallback
                }
            }

            if (string.IsNullOrEmpty(baseText))
            {
                // No base text found — nothing to lay out
                return;
            }

            // Record the fragment index before layout so we can attach ruby annotation
            int fragmentCountBefore = currentLine.Fragments.Count;

            // Lay out the base text as a normal text run using the ruby container's style
            var baseTextNode = new StyledText(baseText, rubyElement.Style);
            LayoutTextRun(baseTextNode, context, ref cursorX, ref cursorY, ref startX,
                          ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);

            // Attach ruby annotation to the first base fragment that was just added
            if (!string.IsNullOrEmpty(annotationText))
            {
                // The annotation height adds space above the line — account for it in line metrics
                float annotationFontSize = annotationStyle != null ? annotationStyle.FontSize : rubyElement.Style.FontSize * 0.5f;
                float annotationHeight = annotationFontSize * 1.2f; // approximate line height

                // Attach to all fragments created for this ruby container
                int newCount = currentLine.Fragments.Count;
                for (int fi = fragmentCountBefore; fi < newCount; fi++)
                {
                    var frag = currentLine.Fragments[fi];
                    frag.RubyText = annotationText;
                    frag.RubyStyle = annotationStyle;
                    frag.RubyBelow = rubyBelow;
                }

                // Also check fragments that may have been pushed to previous line boxes
                // (if text wrapped, the first fragments are in the finalized lines)
                for (int li = lineBoxes.Count - 1; li >= 0; li--)
                {
                    var line = lineBoxes[li];
                    for (int fi = line.Fragments.Count - 1; fi >= 0; fi--)
                    {
                        var frag = line.Fragments[fi];
                        if (frag.RubyText == null && frag.Text == baseText)
                        {
                            frag.RubyText = annotationText;
                            frag.RubyStyle = annotationStyle;
                            frag.RubyBelow = rubyBelow;
                        }
                    }
                }

                // Increase line height to accommodate the annotation
                float totalHeight = maxLineHeight + annotationHeight;
                if (totalHeight > maxLineHeight)
                    maxLineHeight = totalHeight;
            }
        }

        /// <summary>
        /// Recursively extract all text content from a styled element and its children.
        /// </summary>
        private static string ExtractTextContent(StyledElement element)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < element.Children.Count; i++)
            {
                var child = element.Children[i];
                if (child.IsText)
                {
                    sb.Append(((StyledText)child).Text);
                }
                else if (child is StyledElement childEl && childEl.Style.Display != CssDisplay.None)
                {
                    sb.Append(ExtractTextContent(childEl));
                }
            }
            return sb.ToString().Trim();
        }

        private static void LayoutInlineElement(
            StyledElement element, LayoutContext context,
            ref float cursorX, ref float cursorY, ref float startX, ref float containingWidth,
            ref LineBox currentLine, List<LineBox> lineBoxes,
            ref float maxLineHeight, ref float lineBaseline, LayoutBox parent)
        {
            // For inline elements, process their children as if they're part of this inline context.
            // Pass the inline element reference so fragments can be linked back to it
            // (e.g., for detecting <a> elements to generate link annotations).

            // Inline box model: advance cursor for left padding/border/margin
            var inlineStyle = element.Style;
            float inlineML = float.IsNaN(inlineStyle.MarginLeft) ? 0 : inlineStyle.MarginLeft;
            float inlineBL = inlineStyle.BorderLeftStyle != CssBorderStyle.None
                ? (float.IsNaN(inlineStyle.BorderLeftWidth) ? 0 : inlineStyle.BorderLeftWidth) : 0;
            float inlinePL = float.IsNaN(inlineStyle.PaddingLeft) ? 0 : inlineStyle.PaddingLeft;
            cursorX += inlineML + inlineBL + inlinePL;

            for (int i = 0; i < element.Children.Count; i++)
            {
                var child = element.Children[i];
                if (child.IsText)
                {
                    LayoutTextRun((StyledText)child, context, ref cursorX, ref cursorY, ref startX,
                                  ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent,
                                  inlineAncestor: element);
                }
                else if (child is StyledPseudoElement pseudo)
                {
                    var pseudoText = new StyledText(pseudo.Content, pseudo.Style);
                    LayoutTextRun(pseudoText, context, ref cursorX, ref cursorY, ref startX,
                                  ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent,
                                  inlineAncestor: element, styleOverride: pseudo.Style);
                }
                else if (child is StyledElement childEl)
                {
                    if (childEl.Style.Display == CssDisplay.None) continue;

                    if (childEl.Style.Display == CssDisplay.InlineBlock ||
                        childEl.Style.Display == CssDisplay.InlineFlex ||
                        childEl.Style.Display == CssDisplay.InlineGrid)
                    {
                        LayoutInlineBlock(childEl, context, ref cursorX, ref cursorY, startX,
                                          containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);
                    }
                    else
                    {
                        // Recurse for nested inline elements (<span><strong>text</strong></span>)
                        LayoutInlineElement(childEl, context, ref cursorX, ref cursorY, ref startX,
                                           ref containingWidth, ref currentLine, lineBoxes, ref maxLineHeight, ref lineBaseline, parent);
                    }
                }
            }

            // Inline box model: advance cursor for right padding/border/margin
            float inlinePR = float.IsNaN(inlineStyle.PaddingRight) ? 0 : inlineStyle.PaddingRight;
            float inlineBR = inlineStyle.BorderRightStyle != CssBorderStyle.None
                ? (float.IsNaN(inlineStyle.BorderRightWidth) ? 0 : inlineStyle.BorderRightWidth) : 0;
            float inlineMR = float.IsNaN(inlineStyle.MarginRight) ? 0 : inlineStyle.MarginRight;
            cursorX += inlinePR + inlineBR + inlineMR;
        }

        private static void AddTextFragment(LineBox line, string text, ShapedTextRun? shaped,
                                              float x, float width, float height, float baseline,
                                              StyledElement? inlineAncestor = null, ComputedStyle? styleOverride = null,
                                              float contentHeight = 0)
        {
            var fragment = new LineFragment
            {
                X = x - line.X,
                Width = width,
                Height = height,
                ContentHeight = contentHeight > 0 ? contentHeight : height,
                Baseline = baseline,
                Text = text,
                ShapedRun = shaped,
                InlineElement = inlineAncestor,
                StyleOverride = styleOverride
            };
            line.AddFragment(fragment);
        }

        private static void UpdateLineMetrics(ref float maxLineHeight, ref float lineBaseline,
                                               float height, float baseline)
        {
            if (height > maxLineHeight) maxLineHeight = height;
            if (baseline > lineBaseline) lineBaseline = baseline;
        }

        private static void FinalizeLineBox(LineBox line, float height, float baseline, CssTextAlign textAlign,
            CssTextAlign textAlignLast = CssTextAlign.Auto, CssDirection direction = CssDirection.Ltr)
        {
            float h = height > 0 ? height : 16f;
            // Chrome uses LayoutUnit internally (1/64th pixel precision, truncated).
            // Match that instead of rounding to integer pixels, which causes
            // accumulating errors across multiple lines/rows.
            line.Height = (float)((int)(h * 64f)) / 64f;
            line.Baseline = baseline;

            // Apply vertical-align to each fragment
            for (int i = 0; i < line.Fragments.Count; i++)
            {
                var frag = line.Fragments[i];
                var va = GetFragmentVerticalAlign(frag);

                switch (va)
                {
                    case CssVerticalAlign.Baseline:
                        // Align fragment baseline with line baseline
                        frag.Y = baseline - frag.Baseline;
                        break;
                    case CssVerticalAlign.Top:
                        frag.Y = 0;
                        break;
                    case CssVerticalAlign.Bottom:
                        frag.Y = line.Height - frag.Height;
                        break;
                    case CssVerticalAlign.Middle:
                        // Align midpoint of fragment with baseline + half x-height
                        frag.Y = baseline - frag.Height / 2;
                        break;
                    case CssVerticalAlign.TextTop:
                        frag.Y = 0;
                        break;
                    case CssVerticalAlign.TextBottom:
                        frag.Y = line.Height - frag.Height;
                        break;
                    case CssVerticalAlign.Sub:
                        frag.Y = baseline - frag.Baseline + frag.Height * 0.2f;
                        break;
                    case CssVerticalAlign.Super:
                        frag.Y = baseline - frag.Baseline - frag.Height * 0.3f;
                        break;
                }
            }

            // Calculate actual content width
            float contentWidth = 0;
            for (int i = 0; i < line.Fragments.Count; i++)
            {
                var frag = line.Fragments[i];
                float fragRight = frag.X + frag.Width;
                // For the last fragment from an inline element, include trailing padding/border/margin
                if (i == line.Fragments.Count - 1 && frag.InlineElement != null)
                {
                    var ils = frag.InlineElement.Style;
                    fragRight += (float.IsNaN(ils.PaddingRight) ? 0 : ils.PaddingRight)
                               + (ils.BorderRightStyle != CssBorderStyle.None && !float.IsNaN(ils.BorderRightWidth) ? ils.BorderRightWidth : 0)
                               + (float.IsNaN(ils.MarginRight) ? 0 : ils.MarginRight);
                }
                contentWidth = Math.Max(contentWidth, fragRight);
            }
            line.NaturalContentWidth = contentWidth;

            // Apply text-align (for last lines, use text-align-last if set)
            CssTextAlign effectiveAlign = textAlign;
            if (line.IsLastLine && textAlignLast != CssTextAlign.Auto)
            {
                effectiveAlign = textAlignLast;
            }

            float freeSpace = line.Width - contentWidth;
            if (freeSpace <= 0) return;

            // Resolve direction-dependent Start/End to physical Left/Right
            CssTextAlign resolved = effectiveAlign;
            if (resolved == CssTextAlign.Start)
                resolved = direction == CssDirection.Rtl ? CssTextAlign.Right : CssTextAlign.Left;
            else if (resolved == CssTextAlign.End)
                resolved = direction == CssDirection.Rtl ? CssTextAlign.Left : CssTextAlign.Right;

            float offset = 0;
            switch (resolved)
            {
                case CssTextAlign.Center:
                    offset = freeSpace / 2;
                    break;
                case CssTextAlign.Right:
                    offset = freeSpace;
                    break;
                case CssTextAlign.Justify:
                    // Distribute space across word gaps (only non-last lines)
                    if (!line.IsLastLine)
                    {
                        // Count total word gaps (spaces) across all text fragments
                        int totalGaps = 0;
                        for (int i = 0; i < line.Fragments.Count; i++)
                        {
                            var ft = line.Fragments[i].Text;
                            if (ft != null)
                            {
                                for (int c = 0; c < ft.Length; c++)
                                    if (ft[c] == ' ') totalGaps++;
                            }
                        }
                        if (totalGaps > 0)
                        {
                            float extraPerGap = freeSpace / totalGaps;
                            // Apply extra word spacing to each text fragment
                            float cumulativeShift = 0;
                            for (int i = 0; i < line.Fragments.Count; i++)
                            {
                                var frag = line.Fragments[i];
                                frag.X += cumulativeShift;
                                if (frag.Text != null)
                                {
                                    int gapsInFrag = 0;
                                    for (int c = 0; c < frag.Text.Length; c++)
                                        if (frag.Text[c] == ' ') gapsInFrag++;
                                    if (gapsInFrag > 0)
                                    {
                                        frag.JustifyWordSpacing = extraPerGap;
                                        float shift = gapsInFrag * extraPerGap;
                                        frag.Width += shift;
                                        cumulativeShift += shift;
                                    }
                                }
                            }
                        }
                        else if (line.Fragments.Count > 1)
                        {
                            // No word gaps — distribute between fragments
                            float gap = freeSpace / (line.Fragments.Count - 1);
                            for (int i = 1; i < line.Fragments.Count; i++)
                                line.Fragments[i].X += gap * i;
                        }
                    }
                    return;
                case CssTextAlign.Left:
                default:
                    break;
            }

            if (offset > 0)
            {
                for (int i = 0; i < line.Fragments.Count; i++)
                {
                    line.Fragments[i].X += offset;
                }
            }
        }

        /// <summary>
        /// Applies text-wrap: balance by narrowing the effective line width to the maximum
        /// content width across all lines, then re-applying text alignment offsets.
        /// This produces visually balanced line lengths for short text blocks.
        /// </summary>
        private static void ApplyTextWrapBalance(List<LineBox> lineBoxes, float startX, float containingWidth,
            CssTextAlign textAlign, CssTextAlign textAlignLast, CssDirection direction)
        {
            // Find the maximum natural content width across all lines
            float maxContentWidth = 0;
            for (int i = 0; i < lineBoxes.Count; i++)
            {
                float cw = lineBoxes[i].NaturalContentWidth - lineBoxes[i].X;
                if (cw > maxContentWidth) maxContentWidth = cw;
            }

            // If content already fills the container (within 5%), no rebalancing needed
            if (maxContentWidth >= containingWidth * 0.95f) return;

            // Compute balanced width: center the narrower effective width within the container
            float balancedOffset = (containingWidth - maxContentWidth) / 2f;

            // Shift all fragments so lines are centered within the container
            for (int li = 0; li < lineBoxes.Count; li++)
            {
                var line = lineBoxes[li];
                // Reset any existing text-align offset by computing content's current start
                float minFragX = float.MaxValue;
                for (int fi = 0; fi < line.Fragments.Count; fi++)
                {
                    if (line.Fragments[fi].X < minFragX)
                        minFragX = line.Fragments[fi].X;
                }

                // Move fragments so they start at startX + balancedOffset
                float shift = startX + balancedOffset - minFragX;
                for (int fi = 0; fi < line.Fragments.Count; fi++)
                {
                    line.Fragments[fi].X += shift;
                }
            }
        }

        /// <summary>
        /// Applies hanging-punctuation by shifting leading/trailing punctuation
        /// outside the line box margin.
        /// </summary>
        private static void ApplyHangingPunctuation(List<LineBox> lineBoxes, CssHangingPunctuation hp)
        {
            bool hangFirst = hp == CssHangingPunctuation.First;
            bool hangLast = hp == CssHangingPunctuation.Last;
            bool hangForceEnd = hp == CssHangingPunctuation.ForceEnd;
            bool hangAllowEnd = hp == CssHangingPunctuation.AllowEnd;

            for (int li = 0; li < lineBoxes.Count; li++)
            {
                var line = lineBoxes[li];
                if (line.Fragments.Count == 0) continue;

                // Hang first: shift leading opening punctuation outside the start margin
                if (hangFirst && li == 0)
                {
                    var firstFrag = line.Fragments[0];
                    if (!string.IsNullOrEmpty(firstFrag.Text) && IsOpeningPunctuation(firstFrag.Text![0]))
                    {
                        // Approximate the width of the first character
                        float charWidth = firstFrag.Width / Math.Max(1, firstFrag.Text!.Length);
                        for (int fi = 0; fi < line.Fragments.Count; fi++)
                            line.Fragments[fi].X -= charWidth;
                    }
                }

                // Hang last / force-end / allow-end: shift trailing punctuation outside the end
                if (hangLast && line.IsLastLine || hangForceEnd || hangAllowEnd)
                {
                    var lastFrag = line.Fragments[line.Fragments.Count - 1];
                    if (!string.IsNullOrEmpty(lastFrag.Text))
                    {
                        char lastChar = lastFrag.Text![lastFrag.Text!.Length - 1];
                        if (IsClosingPunctuation(lastChar))
                        {
                            // No position shift needed — the trailing punctuation naturally
                            // extends past the line box width, which is acceptable
                        }
                    }
                }
            }
        }

        private static bool IsOpeningPunctuation(char c)
        {
            return c == '\u201C' || c == '\u2018' || c == '(' || c == '[' || c == '{' ||
                   c == '\u00AB' || c == '\u2039'; // «, ‹
        }

        private static bool IsClosingPunctuation(char c)
        {
            return c == '.' || c == ',' || c == ';' || c == ':' || c == '!' || c == '?' ||
                   c == '\u201D' || c == '\u2019' || c == ')' || c == ']' || c == '}' ||
                   c == '\u00BB' || c == '\u203A'; // », ›
        }

        private static void ApplyEllipsis(List<LineBox> lineBoxes, float startX, float containingWidth,
            LayoutContext context, ComputedStyle containerStyle)
        {
            const string ellipsis = "\u2026"; // "…"

            for (int li = 0; li < lineBoxes.Count; li++)
            {
                var line = lineBoxes[li];
                float rightEdge = startX + containingWidth;

                // Check if any fragment overflows (fragment.X is relative to line.X)
                bool overflows = false;
                float lineRelativeRight = rightEdge - line.X;
                for (int fi = 0; fi < line.Fragments.Count; fi++)
                {
                    if (line.Fragments[fi].X + line.Fragments[fi].Width > lineRelativeRight + 0.01f)
                    {
                        overflows = true;
                        break;
                    }
                }

                if (!overflows) continue;

                // Measure the ellipsis width using the container's font properties
                float ellipsisWidth = 0;
                if (context.TextMeasurer != null)
                {
                    // Use container style for font info; fall back to first fragment's style
                    ComputedStyle? fragStyle = containerStyle;
                    if (fragStyle == null && line.Fragments.Count > 0)
                    {
                        var firstTextFrag = line.Fragments[0];
                        fragStyle = firstTextFrag.StyleOverride ?? firstTextFrag.InlineElement?.Style;
                    }
                    float fontSize = fragStyle?.FontSize ?? 14f;
                    var fontDesc = fragStyle != null
                        ? new FontDescriptor(
                            fragStyle.FontFamily ?? "serif",
                            fragStyle.FontWeight,
                            fragStyle.FontStyle,
                            FontDescriptor.StretchToPercentage(fragStyle.FontStretch))
                        : new FontDescriptor("serif", 400, CssFontStyle.Normal, 100f);
                    var shapedEllipsis = context.TextMeasurer.Shape(ellipsis, fontDesc, fontSize);
                    ellipsisWidth = shapedEllipsis.TotalWidth;
                }
                if (ellipsisWidth <= 0) ellipsisWidth = 10f; // fallback

                float cutoff = lineRelativeRight - ellipsisWidth;

                // Find the fragment that contains the cutoff point
                int cutFragIdx = -1;
                for (int fi = 0; fi < line.Fragments.Count; fi++)
                {
                    var frag = line.Fragments[fi];
                    if (frag.X + frag.Width > cutoff)
                    {
                        cutFragIdx = fi;
                        break;
                    }
                }

                if (cutFragIdx < 0) continue;

                // Remove fragments after the cut fragment
                line.TruncateFragmentsAfter(cutFragIdx + 1);

                // Truncate the cut fragment's text and append ellipsis
                var cutFrag = line.Fragments[cutFragIdx];
                if (cutFrag.Text != null)
                {
                    float availableWidth = cutoff - cutFrag.X;

                    string truncated = TruncateTextShaped(cutFrag.Text, availableWidth, cutFrag.Width, cutFrag.ShapedRun);
                    cutFrag.Text = truncated + ellipsis;
                    cutFrag.Width = availableWidth + ellipsisWidth;
                    cutFrag.ShapedRun = null; // invalidate shaped data since text changed
                }

                break; // only process the first overflowing line (typically the only one with nowrap)
            }
        }

        private static string TruncateTextShaped(string text, float availableWidth, float totalWidth,
            Text.ShapedTextRun? shapedRun)
        {
            if (totalWidth <= 0 || text.Length == 0) return "";

            // If we have a shaped run, use per-glyph advances for accurate truncation.
            // The shaped run may cover more text than this fragment if the fragment
            // was split; in that case, scale the available width proportionally.
            if (shapedRun?.Glyphs != null && shapedRun.Glyphs.Length > 0)
            {
                // If the shaped run's total width differs from the fragment width,
                // scale availableWidth to match the shaped coordinate space
                float scale = shapedRun.TotalWidth > 0 ? totalWidth / shapedRun.TotalWidth : 1f;
                float scaledAvailable = scale > 0 ? availableWidth / scale : availableWidth;

                float accumulated = 0;
                int charCount = 0;
                for (int i = 0; i < shapedRun.Glyphs.Length; i++)
                {
                    var glyph = shapedRun.Glyphs[i];
                    if (accumulated + glyph.XAdvance > scaledAvailable + 0.5f)
                        break;
                    accumulated += glyph.XAdvance;
                    charCount = (int)glyph.Cluster + 1;
                }
                charCount = Math.Min(charCount, text.Length);
                return text.Substring(0, charCount);
            }

            // Fallback: proportional estimation
            float ratio = availableWidth / totalWidth;
            int estimatedChars = (int)(text.Length * ratio);
            estimatedChars = Math.Max(0, Math.Min(estimatedChars, text.Length));

            while (estimatedChars < text.Length)
            {
                float nextRatio = (estimatedChars + 1.0f) / text.Length;
                float nextWidth = totalWidth * nextRatio;
                if (nextWidth > availableWidth) break;
                estimatedChars++;
            }

            return text.Substring(0, estimatedChars);
        }

        private static float CalculateSpacingExtra(string text, ComputedStyle style)
        {
            return CalculateSpacingExtraRaw(text, style.LetterSpacing, style.WordSpacing);
        }

        private static float CalculateSpacingExtraRaw(string text, float letterSpacing, float wordSpacing)
        {
            float extra = 0;
            if (letterSpacing != 0 && text.Length > 0)
            {
                // Chrome applies letter-spacing to every character including the last
                extra += letterSpacing * text.Length;
            }
            if (wordSpacing != 0)
            {
                int spaceCount = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == ' ') spaceCount++;
                }
                extra += wordSpacing * spaceCount;
            }
            return extra;
        }

        private static CssVerticalAlign GetFragmentVerticalAlign(LineFragment frag)
        {
            if (frag.InlineElement != null)
                return frag.InlineElement.Style.VerticalAlign;
            if (frag.Box?.StyledNode is StyledElement el)
                return el.Style.VerticalAlign;
            return CssVerticalAlign.Baseline;
        }

        /// <summary>
        /// Find the baseline of the last line box inside a box (recursing into children).
        /// Returns the baseline relative to the box's content rect top.
        /// </summary>
        private static float? FindLastLineBaseline(LayoutBox box)
        {
            // Check the box's own line boxes first
            if (box.LineBoxes != null && box.LineBoxes.Count > 0)
            {
                var lastLine = box.LineBoxes[box.LineBoxes.Count - 1];
                return (lastLine.Y - box.ContentRect.Y) + lastLine.Baseline;
            }

            // Recurse into last child with line boxes
            for (int i = box.Children.Count - 1; i >= 0; i--)
            {
                var childBaseline = FindLastLineBaseline(box.Children[i]);
                if (childBaseline.HasValue)
                {
                    return (box.Children[i].ContentRect.Y - box.ContentRect.Y)
                         + box.Children[i].PaddingTop + box.Children[i].BorderTopWidth
                         + childBaseline.Value;
                }
            }

            return null;
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

        /// <summary>
        /// Measure the actual content width of a box (for shrink-to-fit inline-level boxes).
        /// </summary>
        private static float MeasureContentWidth(LayoutBox box)
        {
            float right = 0;
            float left = box.ContentRect.X;
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                float childRight = child.ContentRect.X + child.ContentRect.Width
                                  + child.PaddingRight + child.BorderRightWidth + child.MarginRight - left;
                if (childRight > right) right = childRight;
            }
            if (box.LineBoxes != null)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    var line = box.LineBoxes[i];
                    for (int f = 0; f < line.Fragments.Count; f++)
                    {
                        float fragRight = line.Fragments[f].X + line.Fragments[f].Width;
                        if (fragRight > right) right = fragRight;
                    }
                }
            }
            return right;
        }

        private static bool IsCjk(char ch)
        {
            return (ch >= 0x3400 && ch <= 0x4DBF) ||
                   (ch >= 0x4E00 && ch <= 0x9FFF) ||
                   (ch >= 0xF900 && ch <= 0xFAFF) ||
                   (ch >= 0x3000 && ch <= 0x303F) ||
                   (ch >= 0x3040 && ch <= 0x309F) ||
                   (ch >= 0x30A0 && ch <= 0x30FF) ||
                   (ch >= 0xFF00 && ch <= 0xFFEF);
        }
    }
}
