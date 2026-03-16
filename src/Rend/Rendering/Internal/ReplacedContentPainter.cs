using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Fonts;
using Rend.Layout;
using Rend.Layout.Internal;
using Rend.Style;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints replaced element content (such as images and form controls) by drawing
    /// the associated <see cref="ImageData"/> into the content rect.
    /// Respects the CSS object-fit property.
    /// </summary>
    internal static class ReplacedContentPainter
    {
        // Common colors for form control rendering
        private static readonly CssColor BorderColor = new CssColor(118, 118, 118);       // #767676
        private static readonly CssColor PlaceholderColor = new CssColor(169, 169, 169);   // #a9a9a9
        private static readonly CssColor ButtonBackground = new CssColor(224, 224, 224);    // #e0e0e0
        private static readonly CssColor ButtonBorderLight = new CssColor(240, 240, 240);   // #f0f0f0
        private static readonly CssColor ButtonBorderDark = new CssColor(160, 160, 160);    // #a0a0a0
        private static readonly CssColor CheckmarkColor = new CssColor(0, 0, 0);            // black
        private static readonly CssColor ArrowColor = new CssColor(80, 80, 80);             // #505050

        private const float FormFontSize = 13.333f;  // Chrome default form font = 10pt = 13.333px
        private const float FormTextPadding = 1f;  // Chrome default input padding: 1px
        private const float FormFontAscent = 12f;  // Arial WinAscent: round(1854/2048 * 13.333) = 12
        private const float FormFontDescent = 3f;  // Arial WinDescent: round(434/2048 * 13.333) = 3
        private const float FormLineHeight = FormFontAscent + FormFontDescent;  // 15px

        /// <summary>
        /// If the layout box represents a replaced element (e.g. &lt;img&gt; or form control),
        /// draws the content into the box's content rectangle.
        /// </summary>
        public static void Paint(LayoutBox box, IRenderTarget target, ImageResolverDelegate? imageResolver)
        {
            if (box.StyledNode == null || box.StyledNode.IsText)
            {
                return;
            }

            StyledElement? element = box.StyledNode as StyledElement;
            if (element == null)
            {
                return;
            }

            string tagName = element.TagName;

            // SVG elements: render inline
            if (tagName == "svg")
            {
                SvgRenderer.Render(element.Element, target, box.ContentRect);
                return;
            }

            // MathML elements: render math notation
            if (tagName == "math")
            {
                MathmlRenderer.Render(element.Element, target, box.ContentRect);
                return;
            }

            // Form controls: paint visual appearance
            if (tagName == "input")
            {
                PaintInput(element, box, target);
                return;
            }

            if (tagName == "select")
            {
                PaintSelect(element, box, target);
                return;
            }

            if (tagName == "textarea")
            {
                PaintTextarea(element, box, target);
                return;
            }



            if (tagName == "meter")
            {
                PaintMeter(element, box, target);
                return;
            }

            if (tagName == "progress")
            {
                PaintProgress(element, box, target);
                return;
            }

            if (tagName == "video")
            {
                PaintVideoPlaceholder(element, box, target, imageResolver);
                return;
            }

            if (tagName == "audio")
            {
                PaintAudioPlaceholder(element, box, target);
                return;
            }

            if (tagName == "canvas")
            {
                PaintCanvasPlaceholder(element, box, target);
                return;
            }

            if (tagName == "iframe")
            {
                PaintIframePlaceholder(element, box, target);
                return;
            }

            if (tagName != "img")
            {
                return;
            }

            string? src = element.GetAttribute("src");
            if (src == null)
            {
                return;
            }

            if (imageResolver == null)
            {
                return;
            }

            ImageData? imageData = imageResolver(src);
            if (imageData == null)
            {
                return;
            }

            RectF contentRect = box.ContentRect;
            CssObjectFit objectFit = element.Style.ObjectFit;
            var (posX, posY) = ParseObjectPosition(element.Style);
            RectF destRect = ComputeObjectFitRect(contentRect, imageData.Width, imageData.Height, objectFit, posX, posY);

            // Clip to content rect when the image may overflow (cover, none, scale-down)
            bool needsClip = objectFit == CssObjectFit.Cover || objectFit == CssObjectFit.None || objectFit == CssObjectFit.ScaleDown;
            if (needsClip)
            {
                target.PushClipRect(contentRect);
            }

            // Apply image-rendering hint
            var imageRendering = element.Style.ImageRendering;
            if (imageRendering != CssImageRendering.Auto)
                target.SetImageRendering(imageRendering);

            target.DrawImage(imageData, destRect);

            // Restore image-rendering
            if (imageRendering != CssImageRendering.Auto)
                target.SetImageRendering(CssImageRendering.Auto);

            if (needsClip)
            {
                target.PopClip();
            }
        }

        // ----- Form control painting methods -----

        /// <summary>
        /// Paints an &lt;input&gt; element based on its type attribute.
        /// </summary>
        private static void PaintInput(StyledElement element, LayoutBox box, IRenderTarget target)
        {
            string inputType = element.GetAttribute("type")?.ToLowerInvariant() ?? "text";
            RectF rect = box.ContentRect;

            switch (inputType)
            {
                case "checkbox":
                    PaintCheckbox(element, rect, target);
                    break;
                case "radio":
                    PaintRadio(element, rect, target);
                    break;
                case "submit":
                case "button":
                case "reset":
                    PaintButtonInput(element, rect, target, inputType);
                    break;
                default:
                    // text, password, email, url, search, tel, number, etc.
                    PaintTextInput(element, box, target, inputType);
                    break;
            }
        }

        /// <summary>
        /// Paints a text/password input matching Chrome's native theme (native_theme_base.cc PaintTextField):
        /// White background fill, then 1px solid #767676 stroke at 0.5px inset.
        /// CSS transparent border provides layout sizing (2px each side + 1px padding).
        /// </summary>
        private static void PaintTextInput(StyledElement element, LayoutBox box, IRenderTarget target, string inputType)
        {
            // Chrome's native theme draws on the border rect (the full control area)
            RectF borderRect = box.BorderRect;

            // Use CSS background-color if author-specified, else white (native theme default).
            // BackgroundPainter may have already painted it, but for form controls the native
            // theme paints on top — so we need to use the correct color here.
            CssColor bgColor = CssColor.White;
            if (box.StyledNode?.Style != null)
            {
                CssColor cssBg = box.StyledNode.Style.BackgroundColor;
                if (cssBg.A > 0)
                {
                    bgColor = cssBg;
                }
            }
            target.FillRect(borderRect, BrushInfo.Solid(bgColor));

            // 1px stroke border at 0.5px inset (matches Chrome native_theme_base.cc)
            // skrect.inset(0.5f, 0.5f); canvas->drawRect(skrect, stroke_flags);
            var strokeRect = new RectF(borderRect.X + 0.5f, borderRect.Y + 0.5f,
                                        borderRect.Width - 1f, borderRect.Height - 1f);
            target.StrokeRect(strokeRect, new PenInfo(BorderColor, 1f));

            // Draw text content in the content rect
            RectF rect = box.ContentRect;
            string? displayText = element.GetAttribute("value");
            bool isPlaceholder = false;
            if (string.IsNullOrEmpty(displayText))
            {
                displayText = element.GetAttribute("placeholder");
                isPlaceholder = true;
            }

            if (!string.IsNullOrEmpty(displayText))
            {
                // Mask password fields
                if (inputType == "password" && !isPlaceholder)
                {
                    displayText = new string('\u2022', displayText!.Length); // bullet characters
                }

                // Clip text to content area
                target.PushClipRect(rect);

                CssColor textColor = isPlaceholder ? PlaceholderColor : CssColor.Black;
                float textY = rect.Y + (rect.Height - FormLineHeight) / 2f + FormFontAscent;
                target.DrawText(displayText!, rect.X, textY,
                    new TextStyle
                    {
                        Font = new FontDescriptor("sans-serif", 400f),
                        FontSize = FormFontSize,
                        Color = textColor
                    });

                target.PopClip();
            }
        }

        /// <summary>
        /// Paints a checkbox: 13x13 box with optional checkmark.
        /// </summary>
        private static void PaintCheckbox(StyledElement element, RectF rect, IRenderTarget target)
        {
            // Chrome's native theme paints on pixel-aligned rects (PixelSnappedIntRect).
            rect = PixelSnapControlRect(rect);

            bool isChecked = element.GetAttribute("checked") != null;
            var accent = GetAccentColor(element);
            bool hasAccent = accent.A > 0 && isChecked;

            // Chrome native_theme_base.cc: border_radius = 2.0f
            float radius = 2f;
            float w = rect.Width;
            float h = rect.Height;

            if (isChecked)
            {
                // Chrome: checked checkbox has accent fill, NO separate border stroke.
                // Background inset by kBorderWidth * 0.2 = 0.2px
                var bgRect = new RectF(rect.X + 0.2f, rect.Y + 0.2f, w - 0.4f, h - 0.4f);
                var bgPath = new PathData();
                bgPath.AddRoundedRectangle(bgRect, radius, radius, radius, radius);
                target.FillPath(bgPath, BrushInfo.Solid(hasAccent ? accent : CssColor.White));

                if (!hasAccent)
                {
                    // If no accent, draw border
                    var strokeRect = new RectF(rect.X + 0.5f, rect.Y + 0.5f, w - 1f, h - 1f);
                    var strokePath = new PathData();
                    strokePath.AddRoundedRectangle(strokeRect, radius, radius, radius, radius);
                    target.StrokePath(strokePath, new PenInfo(BorderColor, 1f));
                }

                // Chrome checkmark path (native_theme_base.cc PaintCheckbox):
                //   moveTo(skrect.x + width * 0.2, skrect.centerY)
                //   rLineTo(width * 0.2, height * 0.2)
                //   lineTo(skrect.right - width * 0.2, skrect.y + height * 0.2)
                // Stroke width: height * 0.16
                var path = new PathData();
                float startX = rect.X + w * 0.2f;
                float startY = rect.Y + h * 0.5f;
                path.MoveTo(startX, startY);
                path.LineTo(startX + w * 0.2f, startY + h * 0.2f);
                path.LineTo(rect.X + w - w * 0.2f, rect.Y + h * 0.2f);

                // Chrome: stroke width = height * 0.16, with kRound_Cap
                float strokeW = h * 0.16f;
                target.StrokePath(path, new PenInfo(hasAccent ? CssColor.White : CheckmarkColor, strokeW, cap: StrokeCap.Round));
            }
            else
            {
                // Chrome: unchecked checkbox — white fill + 1px border
                // Background inset by kBorderWidth * 0.2 = 0.2px
                var bgRect = new RectF(rect.X + 0.2f, rect.Y + 0.2f, w - 0.4f, h - 0.4f);
                var bgPath = new PathData();
                bgPath.AddRoundedRectangle(bgRect, radius, radius, radius, radius);
                target.FillPath(bgPath, BrushInfo.Solid(CssColor.White));

                // Border: inset by kBorderWidth/2 = 0.5px, stroke 1px
                var strokeRect = new RectF(rect.X + 0.5f, rect.Y + 0.5f, w - 1f, h - 1f);
                var strokePath = new PathData();
                strokePath.AddRoundedRectangle(strokeRect, radius, radius, radius, radius);
                target.StrokePath(strokePath, new PenInfo(BorderColor, 1f));
            }
        }

        /// <summary>
        /// Paints a radio button: 13x13 circle with optional filled inner circle.
        /// </summary>
        private static void PaintRadio(StyledElement element, RectF rect, IRenderTarget target)
        {
            // Chrome's native theme paints on pixel-aligned rects (PixelSnappedIntRect).
            rect = PixelSnapControlRect(rect);

            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            float w = rect.Width;
            float h = rect.Height;
            float radius = Math.Max(w, h) * 0.5f;
            bool isChecked = element.GetAttribute("checked") != null;
            var accent = GetAccentColor(element);
            bool hasAccent = accent.A > 0 && isChecked;

            // Chrome: PaintCheckboxRadioCommon always draws background and border for radio
            // Background inset by kBorderWidth * 0.2 = 0.2px
            var bgRect = new RectF(rect.X + 0.2f, rect.Y + 0.2f, w - 0.4f, h - 0.4f);
            float bgRadius = Math.Max(bgRect.Width, bgRect.Height) * 0.5f;

            if (isChecked)
            {
                // Chrome: checked radio — white background + accent border + accent inner dot
                // See native_theme_base.cc PaintRadio: white fill, then accent stroke, then accent dot
                var bgPath = new PathData();
                bgPath.AddRoundedRectangle(bgRect, bgRadius, bgRadius, bgRadius, bgRadius);
                target.FillPath(bgPath, BrushInfo.Solid(CssColor.White));

                // Accent-colored border outline
                var strokeCircle = BuildCirclePath(cx, cy, radius - 0.5f);
                target.StrokePath(strokeCircle, new PenInfo(hasAccent ? accent : BorderColor, 1f));

                // Chrome refreshed form controls: kCheckboxRadioIndicatorDotScale = 0.4
                // dot_size = min(bgRect.Width, bgRect.Height) * 0.4 = 12.6 * 0.4 = 5.04
                // dot_radius = 5.04 / 2 = 2.52
                float dotSize = Math.Min(bgRect.Width, bgRect.Height) * 0.4f;
                float innerRadius = dotSize / 2f;
                var innerCircle = BuildCirclePath(cx, cy, innerRadius);
                target.FillPath(innerCircle, BrushInfo.Solid(hasAccent ? accent : CheckmarkColor));
            }
            else
            {
                // Chrome: unchecked radio — white fill + border
                var bgPath = new PathData();
                bgPath.AddRoundedRectangle(bgRect, bgRadius, bgRadius, bgRadius, bgRadius);
                target.FillPath(bgPath, BrushInfo.Solid(CssColor.White));

                var strokeCircle = BuildCirclePath(cx, cy, radius - 0.5f);
                target.StrokePath(strokeCircle, new PenInfo(BorderColor, 1f));
            }
        }

        /// <summary>
        /// Paints a submit/button/reset input matching Chrome's default button appearance:
        /// #efefef background, 2px outset #767676 border, 2px border-radius, 1px 6px padding.
        /// </summary>
        private static void PaintButtonInput(StyledElement element, RectF rect, IRenderTarget target, string inputType)
        {
            // Chrome native_theme_base.cc PaintButton:
            // - Fill: #efefef rounded rect with radius 2
            // - Border: 1px stroke #767676 at 0.5px inset
            var bgColor = new CssColor(239, 239, 239); // #efefef
            float radius = 2f;

            // Fill background with rounded corners
            var bgPath = new PathData();
            bgPath.AddRoundedRectangle(rect, radius, radius, radius, radius);
            target.FillPath(bgPath, BrushInfo.Solid(bgColor));

            // 1px border at 0.5px inset (matching Chrome's stroke at border_width/2)
            var strokeRect = new RectF(rect.X + 0.5f, rect.Y + 0.5f,
                                        rect.Width - 1f, rect.Height - 1f);
            var strokePath = new PathData();
            strokePath.AddRoundedRectangle(strokeRect, radius, radius, radius, radius);
            target.StrokePath(strokePath, new PenInfo(BorderColor, 1f));

            // Button label text
            string? label = element.GetAttribute("value");
            if (string.IsNullOrEmpty(label))
            {
                label = inputType == "submit" ? "Submit"
                      : inputType == "reset" ? "Reset"
                      : "Button";
            }

            // Center text in the button (inside border+padding area)
            var textStyle = new TextStyle
            {
                Font = new FontDescriptor("sans-serif", 400f),
                FontSize = FormFontSize,
                Color = CssColor.Black
            };
            float measuredWidth = target.MeasureText(label!, textStyle);
            float textWidth = measuredWidth > 0 ? measuredWidth : label!.Length * FormFontSize * 0.55f;
            float textX = rect.X + (rect.Width - textWidth) / 2f;
            float textY = rect.Y + (rect.Height - FormLineHeight) / 2f + FormFontAscent;

            target.DrawText(label!, textX, textY, textStyle);
        }

        /// <summary>
        /// Paints a &lt;select&gt; element: white background, border, first option text, down arrow.
        /// </summary>
        private static void PaintSelect(StyledElement element, LayoutBox box, IRenderTarget target)
        {
            RectF rect = box.ContentRect;
            RectF borderRect = box.BorderRect;

            // Use CSS background-color if author-specified, else white (native theme default).
            CssColor selectBg = CssColor.White;
            if (box.StyledNode?.Style != null)
            {
                CssColor cssBg = box.StyledNode.Style.BackgroundColor;
                if (cssBg.A > 0)
                {
                    selectBg = cssBg;
                }
            }
            target.FillRect(borderRect, BrushInfo.Solid(selectBg));
            var strokeRect = new RectF(borderRect.X + 0.5f, borderRect.Y + 0.5f,
                                        borderRect.Width - 1f, borderRect.Height - 1f);
            target.StrokeRect(strokeRect, new PenInfo(BorderColor, 1f));

            // Find selected <option> text (or first option as fallback)
            string displayText = "";
            string firstOptionText = "";
            var child = element.Element.FirstChild;
            while (child != null)
            {
                if (child is Html.Element optionElement && optionElement.TagName == "option")
                {
                    string optText = optionElement.TextContent?.Trim() ?? "";
                    if (string.IsNullOrEmpty(firstOptionText))
                    {
                        firstOptionText = optText;
                    }
                    if (optionElement.GetAttribute("selected") != null)
                    {
                        displayText = optText;
                        break;
                    }
                }
                child = child.NextSibling;
            }
            if (string.IsNullOrEmpty(displayText))
            {
                displayText = firstOptionText;
            }

            // Draw text (clip to content minus arrow area)
            // Chrome internal padding: 4px start, ~16px end (arrow area)
            float internalPadStart = 4f;
            float arrowAreaWidth = 16f;
            if (!string.IsNullOrEmpty(displayText))
            {
                var textClip = new RectF(rect.X, rect.Y, rect.Width - arrowAreaWidth, rect.Height);
                target.PushClipRect(textClip);

                // Chrome: align-items:center centers the line-height (14px) in the content box
                // baseline = top + (contentHeight - lineHeight)/2 + ascent
                float lineHeight = 14f;  // font metrics height for Arial 13.333px
                float ascent = 11f;
                float textY = rect.Y + (rect.Height - lineHeight) / 2f + ascent;
                float textX = rect.X + internalPadStart;

                target.DrawText(displayText, textX, textY,
                    new TextStyle
                    {
                        Font = new FontDescriptor("sans-serif", 400f),
                        FontSize = FormFontSize,
                        Color = CssColor.Black
                    });

                target.PopClip();
            }

            // Draw down-arrow indicator on right side
            float arrowX = rect.X + rect.Width - arrowAreaWidth / 2f;
            float arrowY = rect.Y + rect.Height / 2f;
            float arrowSize = 4f;

            var arrowPath = new PathData();
            arrowPath.MoveTo(arrowX - arrowSize, arrowY - arrowSize * 0.5f);
            arrowPath.LineTo(arrowX + arrowSize, arrowY - arrowSize * 0.5f);
            arrowPath.LineTo(arrowX, arrowY + arrowSize * 0.5f);
            arrowPath.Close();

            target.FillPath(arrowPath, BrushInfo.Solid(ArrowColor));
        }

        /// <summary>
        /// Paints a &lt;textarea&gt; element: white background, border, text content.
        /// </summary>
        private static void PaintTextarea(StyledElement element, LayoutBox box, IRenderTarget target)
        {
            RectF rect = box.ContentRect;
            RectF borderRect = box.BorderRect;

            // Use CSS background-color if author-specified, else white (native theme default).
            CssColor textareaBg = CssColor.White;
            if (box.StyledNode?.Style != null)
            {
                CssColor cssBg = box.StyledNode.Style.BackgroundColor;
                if (cssBg.A > 0)
                {
                    textareaBg = cssBg;
                }
            }
            target.FillRect(borderRect, BrushInfo.Solid(textareaBg));
            var strokeRect = new RectF(borderRect.X + 0.5f, borderRect.Y + 0.5f,
                                        borderRect.Width - 1f, borderRect.Height - 1f);
            target.StrokeRect(strokeRect, new PenInfo(BorderColor, 1f));

            // Draw text content
            string content = element.Element.TextContent?.Trim() ?? "";
            if (!string.IsNullOrEmpty(content))
            {
                target.PushClipRect(rect);

                // Read font properties from the element's computed style
                float fontSize = FormFontSize;
                string[] fontFamilies = new[] { "monospace" };
                float fontWeight = 400f;
                CssFontStyle fontStyle = CssFontStyle.Normal;
                CssColor textColor = CssColor.Black;

                if (element.Style != null)
                {
                    if (element.Style.FontSize > 0)
                    {
                        fontSize = element.Style.FontSize;
                    }
                    if (element.Style.FontFamilies != null && element.Style.FontFamilies.Length > 0)
                    {
                        fontFamilies = element.Style.FontFamilies;
                    }
                    if (element.Style.FontWeight > 0)
                    {
                        fontWeight = element.Style.FontWeight;
                    }
                    fontStyle = element.Style.FontStyle;
                    textColor = element.Style.Color;
                }

                var fontDesc = new FontDescriptor(fontFamilies, fontWeight, fontStyle);
                var textStyle = new TextStyle
                {
                    Font = fontDesc,
                    FontSize = fontSize,
                    Color = textColor
                };

                // Compute line height and ascent.
                // For Arial/sans-serif at 13.333px: ascent=12, lineHeight=15 (same as FormFont constants).
                // For other sizes, scale proportionally using Arial's WinAscent/WinDescent ratios.
                float ascent = (float)Math.Round(fontSize * 1854.0 / 2048.0, MidpointRounding.AwayFromZero);
                float descent = (float)Math.Round(fontSize * 434.0 / 2048.0, MidpointRounding.AwayFromZero);
                float lineHeight = ascent + descent;

                float textX = rect.X;
                float textY = rect.Y + ascent;
                float contentWidth = rect.Width;

                string[] hardLines = content.Split('\n');

                for (int i = 0; i < hardLines.Length; i++)
                {
                    string remaining = hardLines[i].TrimEnd('\r');
                    // Wrap long lines at word boundaries using actual text measurement
                    while (remaining.Length > 0)
                    {
                        if (textY + fontSize > rect.Y + rect.Height)
                        {
                            break;
                        }

                        string segment;
                        float measuredWidth = target.MeasureText(remaining, textStyle);

                        if (measuredWidth >= 0 && measuredWidth <= contentWidth)
                        {
                            segment = remaining;
                            remaining = "";
                        }
                        else
                        {
                            // Find last word boundary that fits within content width
                            int breakAt = -1;
                            int searchFrom = 0;
                            while (true)
                            {
                                int spaceIdx = remaining.IndexOf(' ', searchFrom);
                                if (spaceIdx < 0)
                                {
                                    break;
                                }
                                float candidateWidth = target.MeasureText(remaining.Substring(0, spaceIdx), textStyle);
                                if (candidateWidth >= 0 && candidateWidth <= contentWidth)
                                {
                                    breakAt = spaceIdx;
                                    searchFrom = spaceIdx + 1;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (breakAt <= 0)
                            {
                                // No word boundary fits — break at character level
                                breakAt = 1;
                                for (int c = 2; c <= remaining.Length; c++)
                                {
                                    float charMeasure = target.MeasureText(remaining.Substring(0, c), textStyle);
                                    if (charMeasure >= 0 && charMeasure > contentWidth)
                                    {
                                        break;
                                    }
                                    breakAt = c;
                                }
                            }
                            segment = remaining.Substring(0, breakAt);
                            remaining = remaining.Substring(breakAt).TrimStart(' ');
                        }

                        if (segment.Length > 0)
                        {
                            target.DrawText(segment, textX, textY, textStyle);
                        }
                        textY += lineHeight;
                    }

                    // Empty hard line still advances
                    if (hardLines[i].TrimEnd('\r').Length == 0)
                    {
                        textY += lineHeight;
                    }
                }

                target.PopClip();
            }

            // Draw resize grip (Chrome's ScrollableAreaPainter)
            PaintResizeGrip(target, borderRect);
        }

        /// <summary>
        /// Paints a resize grip in the bottom-right corner of a textarea,
        /// matching Chrome's diagonal line pattern.
        /// </summary>
        private static void PaintResizeGrip(IRenderTarget target, RectF borderRect)
        {
            // Chrome draws two sets of diagonal lines: dark (rgba(0,0,0,0.6)) and
            // light (rgba(255,255,255,0.6)), offset by 1px for a 3D shadow effect.
            float right = borderRect.X + borderRect.Width - 1f;
            float bottom = borderRect.Y + borderRect.Height - 1f;

            var darkColor = new CssColor(102, 102, 102);   // ~rgba(0,0,0,0.6) on white
            var lightColor = new CssColor(255, 255, 255);   // light lines on the left of each pair

            // Three pairs of diagonal lines from bottom-right corner
            for (int i = 0; i < 3; i++)
            {
                float offset = i * 4f;
                float x1 = right - 1f - offset;
                float y1 = bottom;
                float x2 = right;
                float y2 = bottom - 1f - offset;

                // Light line (offset -1)
                var lightPath = new PathData();
                lightPath.MoveTo(x1 - 1f, y1);
                lightPath.LineTo(x2, y2 - 1f);
                target.StrokePath(lightPath, new PenInfo(lightColor, 1f));

                // Dark line
                var darkPath = new PathData();
                darkPath.MoveTo(x1, y1);
                darkPath.LineTo(x2, y2);
                target.StrokePath(darkPath, new PenInfo(darkColor, 1f));
            }
        }

        /// <summary>
        /// Builds a circle path approximated with cubic bezier curves.
        /// </summary>
        private static PathData BuildCirclePath(float cx, float cy, float radius)
        {
            // Use AddRoundedRectangle with equal radii to create a circle.
            // This triggers native SKRoundRect (type=kOval) matching Chrome's Skia drawOval.
            var path = new PathData();
            var rect = new RectF(cx - radius, cy - radius, radius * 2f, radius * 2f);
            path.AddRoundedRectangle(rect, radius, radius, radius, radius);
            return path;
        }

        /// <summary>
        /// Parse the object-position property from a computed style.
        /// Returns normalized position as fractions (0.0 = left/top, 1.0 = right/bottom).
        /// Default: (0.5, 0.5) = center.
        /// </summary>
        private static (float x, float y) ParseObjectPosition(ComputedStyle style)
        {
            var raw = style.GetRefValue(Css.Properties.Internal.PropertyId.ObjectPosition);
            if (raw == null) return (0.5f, 0.5f);

            if (raw is CssListValue list && list.Values.Count >= 2)
            {
                float x = ResolvePositionComponent(list.Values[0]);
                float y = ResolvePositionComponent(list.Values[1]);
                return (x, y);
            }

            if (raw is CssPercentageValue pct)
                return (pct.Value / 100f, 0.5f);

            if (raw is CssKeywordValue kw)
            {
                float v = KeywordToFraction(kw.Keyword);
                return (v, 0.5f);
            }

            return (0.5f, 0.5f);
        }

        private static float ResolvePositionComponent(CssValue val)
        {
            if (val is CssPercentageValue pct) return pct.Value / 100f;
            if (val is CssKeywordValue kw) return KeywordToFraction(kw.Keyword);
            if (val is CssDimensionValue dim) return 0.5f; // px values need container size
            if (val is CssNumberValue num) return num.Value == 0 ? 0f : 0.5f;
            return 0.5f;
        }

        private static float KeywordToFraction(string keyword)
        {
            switch (keyword)
            {
                case "left":
                case "top": return 0f;
                case "center": return 0.5f;
                case "right":
                case "bottom": return 1f;
                default: return 0.5f;
            }
        }

        private static RectF ComputeObjectFitRect(RectF contentRect, float imgW, float imgH,
            CssObjectFit fit, float posX = 0.5f, float posY = 0.5f)
        {
            if (imgW <= 0 || imgH <= 0)
            {
                return contentRect;
            }

            switch (fit)
            {
                case CssObjectFit.Contain:
                {
                    float ratioW = contentRect.Width / imgW;
                    float ratioH = contentRect.Height / imgH;
                    float ratio = Math.Min(ratioW, ratioH);
                    float scaledW = imgW * ratio;
                    float scaledH = imgH * ratio;
                    float x = contentRect.X + (contentRect.Width - scaledW) * posX;
                    float y = contentRect.Y + (contentRect.Height - scaledH) * posY;
                    return new RectF(x, y, scaledW, scaledH);
                }

                case CssObjectFit.Cover:
                {
                    float ratioW = contentRect.Width / imgW;
                    float ratioH = contentRect.Height / imgH;
                    float ratio = Math.Max(ratioW, ratioH);
                    float scaledW = imgW * ratio;
                    float scaledH = imgH * ratio;
                    float x = contentRect.X + (contentRect.Width - scaledW) * posX;
                    float y = contentRect.Y + (contentRect.Height - scaledH) * posY;
                    return new RectF(x, y, scaledW, scaledH);
                }

                case CssObjectFit.None:
                {
                    float x = contentRect.X + (contentRect.Width - imgW) * posX;
                    float y = contentRect.Y + (contentRect.Height - imgH) * posY;
                    return new RectF(x, y, imgW, imgH);
                }

                case CssObjectFit.ScaleDown:
                {
                    if (imgW <= contentRect.Width && imgH <= contentRect.Height)
                    {
                        float x = contentRect.X + (contentRect.Width - imgW) * posX;
                        float y = contentRect.Y + (contentRect.Height - imgH) * posY;
                        return new RectF(x, y, imgW, imgH);
                    }
                    else
                    {
                        float ratioW = contentRect.Width / imgW;
                        float ratioH = contentRect.Height / imgH;
                        float ratio = Math.Min(ratioW, ratioH);
                        float scaledW = imgW * ratio;
                        float scaledH = imgH * ratio;
                        float x = contentRect.X + (contentRect.Width - scaledW) * posX;
                        float y = contentRect.Y + (contentRect.Height - scaledH) * posY;
                        return new RectF(x, y, scaledW, scaledH);
                    }
                }

                default: // Fill
                    return contentRect;
            }
        }

        /// <summary>
        /// Snap a form control rect to integer pixel boundaries, matching Chrome's
        /// PixelSnappedIntRect for native theme painting. Each edge rounds independently
        /// using AwayFromZero (matching Chrome's roundf).
        /// </summary>
        private static RectF PixelSnapControlRect(RectF rect)
        {
            float left = (float)Math.Round(rect.X, MidpointRounding.AwayFromZero);
            float top = (float)Math.Round(rect.Y, MidpointRounding.AwayFromZero);
            float right = (float)Math.Round(rect.X + rect.Width, MidpointRounding.AwayFromZero);
            float bottom = (float)Math.Round(rect.Y + rect.Height, MidpointRounding.AwayFromZero);
            return new RectF(left, top, right - left, bottom - top);
        }

        // Chrome 116 default accent color for checkboxes/radios
        private static readonly CssColor DefaultAccentBlue = new CssColor(0, 117, 255);

        private static CssColor GetAccentColor(StyledElement element)
        {
            var accent = element.Style.AccentColor;
            // If no accent-color set (transparent/zero), use Chrome's default blue
            if (accent.A == 0 && accent.R == 0 && accent.G == 0 && accent.B == 0)
                return DefaultAccentBlue;
            return accent;
        }

        /// <summary>
        /// Returns true if the element has an explicit accent-color CSS property set.
        /// </summary>
        private static bool HasExplicitAccentColor(StyledElement element)
        {
            var accent = element.Style.AccentColor;
            return accent.A != 0 || accent.R != 0 || accent.G != 0 || accent.B != 0;
        }

        /// <summary>
        /// Chrome's GetGaugeRegion() logic mapped to colors from UA stylesheet.
        /// </summary>
        private static CssColor GetMeterColor(float value, float low, float high, float optimum)
        {
            // Chrome 116 UA: optimum=#107c10, suboptimal=#ffb900, even-less-good=#d83b01
            var colorOptimum = new CssColor(0x10, 0x7C, 0x10);
            var colorSuboptimal = new CssColor(0xFF, 0xB9, 0x00);
            var colorEvenLessGood = new CssColor(0xD8, 0x3B, 0x01);

            if (optimum < low)
            {
                if (value <= low) return colorOptimum;
                if (value <= high) return colorSuboptimal;
                return colorEvenLessGood;
            }

            if (high < optimum)
            {
                if (high <= value) return colorOptimum;
                if (low <= value) return colorSuboptimal;
                return colorEvenLessGood;
            }

            // Optimum between low and high
            if (low <= value && value <= high) return colorOptimum;
            return colorSuboptimal;
        }

        private static void PaintMeter(StyledElement element, LayoutBox box, IRenderTarget target)
        {
            RectF rect = box.ContentRect;

            // Parse attributes
            float min = ParseFloat(element.GetAttribute("min"), 0f);
            float max = ParseFloat(element.GetAttribute("max"), 1f);
            float value = ParseFloat(element.GetAttribute("value"), 0f);
            float low = ParseFloat(element.GetAttribute("low"), min);
            float high = ParseFloat(element.GetAttribute("high"), max);
            float optimum = ParseFloat(element.GetAttribute("optimum"), (min + max) * 0.5f);

            // Normalize value to 0-1 range
            float range = max - min;
            if (range <= 0) range = 1;
            float fraction = (value - min) / range;
            fraction = Math.Max(0f, Math.Min(1f, fraction));

            // Determine gauge region per Chrome's GetGaugeRegion() logic
            // Colors: optimum=#107c10, suboptimal=#ffb900, even-less-good=#d83b01
            CssColor barColor;
            if (HasExplicitAccentColor(element))
            {
                barColor = element.Style.AccentColor;
            }
            else
            {
                barColor = GetMeterColor(value, low, high, optimum);
            }

            // Chrome 116 meter uses shadow DOM with grid: 1fr [line1] 2fr [line2] 1fr
            // The bar track occupies the center 50% of the element height
            // border-radius: 20px, background: #efefef, border: thin solid rgba(118,118,118,0.3)
            float trackHeight = (float)Math.Round(rect.Height * 0.5, MidpointRounding.AwayFromZero);
            float insetY = (float)Math.Round((rect.Height - trackHeight) * 0.5, MidpointRounding.AwayFromZero);
            var barAreaRect = new RectF(rect.X, rect.Y + insetY, rect.Width, trackHeight);
            float radius = 20f; // Chrome UA: border-radius: 20px

            // Chrome meter uses CSS: border on bar element, value inside with overflow:hidden
            // So: 1) fill track, 2) clip to content area (inside border), 3) fill value, 4) stroke border
            float bw = 1f; // "thin" = 1px border
            var trackColor = new CssColor(0xEF, 0xEF, 0xEF);
            var trackPath = new PathData();
            trackPath.AddRoundedRectangle(barAreaRect, radius, radius, radius, radius);
            target.FillPath(trackPath, BrushInfo.Solid(trackColor));

            // Value bar clipped to content area (inside border)
            float barWidth = barAreaRect.Width * fraction;
            if (barWidth > 0)
            {
                var innerRect = new RectF(barAreaRect.X + bw, barAreaRect.Y + bw,
                                          barAreaRect.Width - bw * 2, barAreaRect.Height - bw * 2);
                var innerPath = new PathData();
                innerPath.AddRoundedRectangle(innerRect, radius, radius, radius, radius);
                target.Save();
                target.PushClipPath(innerPath);
                var barRect = new RectF(barAreaRect.X, barAreaRect.Y, barWidth, trackHeight);
                target.FillRect(barRect, BrushInfo.Solid(barColor));
                target.PopClip();
                target.Restore();
            }

            // Border: thin solid rgba(118,118,118,0.3)
            // Stroke centered on track edge, inset by borderWidth/2
            var borderColor = new CssColor(118, 118, 118, (byte)(255 * 0.3f));
            var borderRect = new RectF(barAreaRect.X + bw * 0.5f, barAreaRect.Y + bw * 0.5f,
                                       barAreaRect.Width - bw, barAreaRect.Height - bw);
            var borderPath = new PathData();
            borderPath.AddRoundedRectangle(borderRect, radius, radius, radius, radius);
            target.StrokePath(borderPath, new PenInfo(borderColor, bw));
        }

        private static void PaintProgress(StyledElement element, LayoutBox box, IRenderTarget target)
        {
            RectF rect = box.ContentRect;

            // Parse attributes
            float max = ParseFloat(element.GetAttribute("max"), 1f);
            string? valueAttr = element.GetAttribute("value");
            if (valueAttr == null)
            {
                var attrs = element.Element.Attributes;
                for (int i = 0; i < attrs.Count; i++)
                {
                    var attr = attrs[i];
                    if (string.Equals(attr.Name, "value", StringComparison.OrdinalIgnoreCase))
                    {
                        valueAttr = attr.Value;
                        break;
                    }
                }
            }

            // Chrome 116 progress bar (from native_theme_base.cc):
            // kSliderTrackHeight = 8px, border_radius = 40px (full pill), kBorderWidth = 1px
            // Track centered vertically via AlignSliderTrack.
            float trackHeight = 8f;
            float insetY = (float)Math.Round((rect.Height - trackHeight) * 0.5, MidpointRounding.AwayFromZero);
            var barAreaRect = new RectF(rect.X, rect.Y + insetY, rect.Width, trackHeight);
            float radius = 40f; // kGetBorderRadiusForPart returns 40 for progress bar

            // ControlsFillColorForState(kNormal) = 0xEF,0xEF,0xEF
            var trackColor = new CssColor(0xEF, 0xEF, 0xEF);
            var trackPath = new PathData();
            trackPath.AddRoundedRectangle(barAreaRect, radius, radius, radius, radius);
            target.FillPath(trackPath, BrushInfo.Solid(trackColor));

            // ControlsAccentColorForState(kNormal) = 0x00,0x75,0xFF
            CssColor barColor;
            CssColor accentColor = GetAccentColor(element);
            if (accentColor.R != 0 || accentColor.G != 0 || accentColor.B != 0 || accentColor.A != 0)
                barColor = accentColor;
            else
                barColor = new CssColor(0x00, 0x75, 0xFF);

            if (valueAttr != null)
            {
                // Determinate: value bar clipped to track pill shape
                float value = ParseFloat(valueAttr, 0f);
                float fraction = max > 0 ? value / max : 0f;
                fraction = Math.Max(0f, Math.Min(1f, fraction));

                float barWidth = Math.Max(barAreaRect.Width * fraction, 2f); // kMinimumProgressInlineValue = 2
                target.Save();
                target.PushClipPath(trackPath);
                var barRect = new RectF(barAreaRect.X, barAreaRect.Y, barWidth, trackHeight);
                target.FillRect(barRect, BrushInfo.Solid(barColor));
                target.PopClip();
                target.Restore();
            }
            else
            {
                // Indeterminate: animated block — static snapshot at ~33%
                // Chrome uses drawRoundRect for indeterminate (not plain rect)
                target.Save();
                target.PushClipPath(trackPath);
                float blockWidth = barAreaRect.Width * 0.33f;
                var blockRect = new RectF(barAreaRect.X, barAreaRect.Y, blockWidth, trackHeight);
                var blockPath = new PathData();
                blockPath.AddRoundedRectangle(blockRect, radius, radius, radius, radius);
                target.FillPath(blockPath, BrushInfo.Solid(barColor));
                target.PopClip();
                target.Restore();
            }

            // ControlsBorderColorForState(kNormal) = 0x76,0x76,0x76 with 0x80 alpha
            // Chrome insets the border rect by borderWidth/2 so the stroke stays inside the track
            var borderColor = new CssColor(0x76, 0x76, 0x76, 0x80);
            float bw = 1f;
            var borderRect = new RectF(barAreaRect.X + bw * 0.5f, barAreaRect.Y + bw * 0.5f,
                                       barAreaRect.Width - bw, barAreaRect.Height - bw);
            var borderPath = new PathData();
            borderPath.AddRoundedRectangle(borderRect, radius, radius, radius, radius);
            target.StrokePath(borderPath, new PenInfo(borderColor, bw));
        }

        private static void PaintVideoPlaceholder(StyledElement element, LayoutBox box, IRenderTarget target, ImageResolverDelegate? imageResolver)
        {
            RectF rect = box.ContentRect;

            // Try to render poster image
            string? poster = element.GetAttribute("poster");
            if (poster != null && imageResolver != null)
            {
                ImageData? posterImage = imageResolver(poster);
                if (posterImage != null)
                {
                    CssObjectFit objectFit = element.Style.ObjectFit;
                    var (posX, posY) = ParseObjectPosition(element.Style);
                    RectF destRect = ComputeObjectFitRect(rect, posterImage.Width, posterImage.Height, objectFit, posX, posY);
                    target.DrawImage(posterImage, destRect);
                    return;
                }
            }

            // Fallback: gray placeholder with play button triangle
            var bgColor = new CssColor(40, 40, 40);
            target.FillRect(rect, BrushInfo.Solid(bgColor));

            // Draw play triangle in center
            float triSize = Math.Min(rect.Width, rect.Height) * 0.3f;
            if (triSize > 5f)
            {
                float cx = rect.X + rect.Width * 0.5f;
                float cy = rect.Y + rect.Height * 0.5f;

                var path = new PathData();
                path.MoveTo(cx - triSize * 0.4f, cy - triSize * 0.5f);
                path.LineTo(cx + triSize * 0.5f, cy);
                path.LineTo(cx - triSize * 0.4f, cy + triSize * 0.5f);
                path.Close();

                target.FillPath(path, BrushInfo.Solid(new CssColor(200, 200, 200)));
            }
        }

        private static void PaintAudioPlaceholder(StyledElement element, LayoutBox box, IRenderTarget target)
        {
            RectF rect = box.ContentRect;

            // Light gray background
            var bgColor = new CssColor(240, 240, 240);
            target.FillRect(rect, BrushInfo.Solid(bgColor));

            // Border
            target.StrokeRect(rect, new PenInfo(BorderColor, 1f));

            // Draw play triangle on the left
            float triSize = Math.Min(rect.Width * 0.15f, rect.Height * 0.6f);
            if (triSize > 3f)
            {
                float tx = rect.X + rect.Height * 0.5f;
                float ty = rect.Y + rect.Height * 0.5f;

                var path = new PathData();
                path.MoveTo(tx - triSize * 0.3f, ty - triSize * 0.5f);
                path.LineTo(tx + triSize * 0.4f, ty);
                path.LineTo(tx - triSize * 0.3f, ty + triSize * 0.5f);
                path.Close();

                target.FillPath(path, BrushInfo.Solid(new CssColor(80, 80, 80)));
            }

            // Draw a simple progress track line
            float trackY = rect.Y + rect.Height * 0.5f;
            float trackLeft = rect.X + rect.Height + 4f;
            float trackRight = rect.X + rect.Width - 8f;
            if (trackRight > trackLeft)
            {
                var trackPath = new PathData();
                trackPath.MoveTo(trackLeft, trackY);
                trackPath.LineTo(trackRight, trackY);
                target.StrokePath(trackPath, new PenInfo(new CssColor(180, 180, 180), 2f));
            }
        }

        private static void PaintIframePlaceholder(StyledElement element, LayoutBox box, IRenderTarget target)
        {
            RectF rect = box.ContentRect;

            // White background
            target.FillRect(rect, BrushInfo.Solid(CssColor.White));

            // 1px border
            target.StrokeRect(rect, new PenInfo(BorderColor, 1f));

            // Show srcdoc content if available — just render as text
            string? srcdoc = element.GetAttribute("srcdoc");
            if (srcdoc != null && srcdoc.Length > 0)
            {
                // Strip HTML tags for simple text display
                string text = StripHtmlTags(srcdoc).Trim();
                if (text.Length > 0)
                {
                    target.PushClipRect(rect);
                    float textX = rect.X + 4f;
                    float textY = rect.Y + 4f;
                    target.DrawText(text, textX, textY, new TextStyle
                    {
                        Font = new FontDescriptor("sans-serif", 400f),
                        FontSize = 12f,
                        Color = CssColor.Black
                    });
                    target.PopClip();
                }
            }
            else
            {
                // No srcdoc: show URL or empty placeholder
                string? src = element.GetAttribute("src");
                if (src != null)
                {
                    target.PushClipRect(rect);
                    target.DrawText(src, rect.X + 4f, rect.Y + 4f, new TextStyle
                    {
                        Font = new FontDescriptor("sans-serif", 400f),
                        FontSize = 10f,
                        Color = new CssColor(128, 128, 128)
                    });
                    target.PopClip();
                }
            }
        }

        private static string StripHtmlTags(string html)
        {
            var sb = new System.Text.StringBuilder(html.Length);
            bool inTag = false;
            for (int i = 0; i < html.Length; i++)
            {
                char c = html[i];
                if (c == '<') { inTag = true; continue; }
                if (c == '>') { inTag = false; continue; }
                if (!inTag) sb.Append(c);
            }
            return sb.ToString();
        }

        private static void PaintCanvasPlaceholder(StyledElement element, LayoutBox box, IRenderTarget target)
        {
            RectF rect = box.ContentRect;

            // Canvas without JS: transparent/white background (per spec)
            target.FillRect(rect, BrushInfo.Solid(CssColor.White));
        }

        private static float ParseFloat(string? value, float defaultValue)
        {
            if (value != null && float.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }
            return defaultValue;
        }
    }

    /// <summary>
    /// A delegate that resolves a source URL to an <see cref="ImageData"/> instance,
    /// or returns null if the image could not be loaded.
    /// </summary>
    /// <param name="src">The image source URL or data URI.</param>
    /// <returns>The resolved image data, or null.</returns>
    internal delegate ImageData? ImageResolverDelegate(string src);
}
