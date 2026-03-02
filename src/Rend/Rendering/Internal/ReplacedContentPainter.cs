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

        private const float FormFontSize = 11f;
        private const float FormTextPadding = 3f;
        // Approximate ascent ratio for sans-serif fonts (baseline sits ~80% below top of em square)
        private const float FormFontAscent = FormFontSize * 0.8f;

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
                    PaintTextInput(element, rect, target, inputType);
                    break;
            }
        }

        /// <summary>
        /// Paints a text/password input: white background, 1px border, value or placeholder text.
        /// </summary>
        private static void PaintTextInput(StyledElement element, RectF rect, IRenderTarget target, string inputType)
        {
            // White background
            target.FillRect(rect, BrushInfo.Solid(CssColor.White));

            // 1px border
            target.StrokeRect(rect, new PenInfo(BorderColor, 1f));

            // Determine text to show
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
                // DrawText uses Y as baseline; center by placing baseline at vertical midpoint + half cap height
                float textY = rect.Y + rect.Height / 2f + FormFontAscent * 0.4f;
                target.DrawText(displayText!, rect.X + FormTextPadding, textY,
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
            bool isChecked = element.GetAttribute("checked") != null;
            var accent = GetAccentColor(element);
            bool hasAccent = accent.A > 0 && isChecked;

            // Background: accent color when checked with accent-color set, otherwise white
            target.FillRect(rect, BrushInfo.Solid(hasAccent ? accent : CssColor.White));

            // 1px border
            target.StrokeRect(rect, new PenInfo(hasAccent ? accent : BorderColor, 1f));

            // Draw checkmark if checked
            if (isChecked)
            {
                float cx = rect.X;
                float cy = rect.Y;
                float w = rect.Width;
                float h = rect.Height;

                var path = new PathData();
                // Checkmark path: two lines forming a check
                path.MoveTo(cx + w * 0.2f, cy + h * 0.5f);
                path.LineTo(cx + w * 0.4f, cy + h * 0.75f);
                path.LineTo(cx + w * 0.8f, cy + h * 0.25f);

                // White checkmark on accent background, black otherwise
                target.StrokePath(path, new PenInfo(hasAccent ? CssColor.White : CheckmarkColor, 1.5f));
            }
        }

        /// <summary>
        /// Paints a radio button: 13x13 circle with optional filled inner circle.
        /// </summary>
        private static void PaintRadio(StyledElement element, RectF rect, IRenderTarget target)
        {
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            float radius = Math.Min(rect.Width, rect.Height) / 2f;
            bool isChecked = element.GetAttribute("checked") != null;
            var accent = GetAccentColor(element);
            bool hasAccent = accent.A > 0 && isChecked;

            // Outer circle (white fill + border)
            var outerCircle = BuildCirclePath(cx, cy, radius);
            target.FillPath(outerCircle, BrushInfo.Solid(hasAccent ? accent : CssColor.White));
            target.StrokePath(outerCircle, new PenInfo(hasAccent ? accent : BorderColor, 1f));

            // Inner filled circle if checked
            if (isChecked)
            {
                float innerRadius = radius * 0.45f;
                var innerCircle = BuildCirclePath(cx, cy, innerRadius);
                target.FillPath(innerCircle, BrushInfo.Solid(hasAccent ? CssColor.White : CheckmarkColor));
            }
        }

        /// <summary>
        /// Paints a submit/button/reset input: gray background, outset border, centered text.
        /// </summary>
        private static void PaintButtonInput(StyledElement element, RectF rect, IRenderTarget target, string inputType)
        {
            // Gray background
            target.FillRect(rect, BrushInfo.Solid(ButtonBackground));

            // Outset border: light top-left, dark bottom-right
            float bw = 1f;

            // Top edge (light)
            target.FillRect(new RectF(rect.X, rect.Y, rect.Width, bw), BrushInfo.Solid(ButtonBorderLight));
            // Left edge (light)
            target.FillRect(new RectF(rect.X, rect.Y, bw, rect.Height), BrushInfo.Solid(ButtonBorderLight));
            // Bottom edge (dark)
            target.FillRect(new RectF(rect.X, rect.Y + rect.Height - bw, rect.Width, bw), BrushInfo.Solid(ButtonBorderDark));
            // Right edge (dark)
            target.FillRect(new RectF(rect.X + rect.Width - bw, rect.Y, bw, rect.Height), BrushInfo.Solid(ButtonBorderDark));

            // Button label text
            string? label = element.GetAttribute("value");
            if (string.IsNullOrEmpty(label))
            {
                label = inputType == "submit" ? "Submit"
                      : inputType == "reset" ? "Reset"
                      : "Button";
            }

            // Center text in the button
            float estimatedTextWidth = label!.Length * FormFontSize * 0.55f;
            float textX = rect.X + (rect.Width - estimatedTextWidth) / 2f;
            float textY = rect.Y + rect.Height / 2f + FormFontAscent * 0.4f;

            target.DrawText(label, textX, textY,
                new TextStyle
                {
                    Font = new FontDescriptor("sans-serif", 400f),
                    FontSize = FormFontSize,
                    Color = CssColor.Black
                });
        }

        /// <summary>
        /// Paints a &lt;select&gt; element: white background, border, first option text, down arrow.
        /// </summary>
        private static void PaintSelect(StyledElement element, LayoutBox box, IRenderTarget target)
        {
            RectF rect = box.ContentRect;

            // White background
            target.FillRect(rect, BrushInfo.Solid(CssColor.White));

            // 1px border
            target.StrokeRect(rect, new PenInfo(BorderColor, 1f));

            // Find first <option> text
            string displayText = "";
            var child = element.Element.FirstChild;
            while (child != null)
            {
                if (child is Html.Element optionElement && optionElement.TagName == "option")
                {
                    displayText = optionElement.TextContent?.Trim() ?? "";
                    break;
                }
                child = child.NextSibling;
            }

            // Draw text (clip to content minus arrow area)
            float arrowAreaWidth = 16f;
            if (!string.IsNullOrEmpty(displayText))
            {
                var textClip = new RectF(rect.X, rect.Y, rect.Width - arrowAreaWidth, rect.Height);
                target.PushClipRect(textClip);

                float textY = rect.Y + rect.Height / 2f + FormFontAscent * 0.4f;
                target.DrawText(displayText, rect.X + FormTextPadding, textY,
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

            // White background
            target.FillRect(rect, BrushInfo.Solid(CssColor.White));

            // 1px border
            target.StrokeRect(rect, new PenInfo(BorderColor, 1f));

            // Draw text content
            string content = element.Element.TextContent?.Trim() ?? "";
            if (!string.IsNullOrEmpty(content))
            {
                target.PushClipRect(rect);

                float lineHeight = FormFontSize * 1.4f;
                float textX = rect.X + FormTextPadding;
                float textY = rect.Y + FormTextPadding;

                // Split content into lines and draw each
                string[] lines = content.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (textY + FormFontSize > rect.Y + rect.Height)
                        break; // Stop if text overflows

                    string line = lines[i].TrimEnd('\r');
                    if (line.Length > 0)
                    {
                        target.DrawText(line, textX, textY,
                            new TextStyle
                            {
                                Font = new FontDescriptor("monospace", 400f),
                                FontSize = FormFontSize,
                                Color = CssColor.Black
                            });
                    }
                    textY += lineHeight;
                }

                target.PopClip();
            }
        }

        /// <summary>
        /// Builds a circle path approximated with cubic bezier curves.
        /// </summary>
        private static PathData BuildCirclePath(float cx, float cy, float radius)
        {
            const float kappa = 0.5522847498f;
            float k = radius * kappa;

            var path = new PathData();
            path.MoveTo(cx + radius, cy);
            path.CubicBezierTo(cx + radius, cy + k, cx + k, cy + radius, cx, cy + radius);
            path.CubicBezierTo(cx - k, cy + radius, cx - radius, cy + k, cx - radius, cy);
            path.CubicBezierTo(cx - radius, cy - k, cx - k, cy - radius, cx, cy - radius);
            path.CubicBezierTo(cx + k, cy - radius, cx + radius, cy - k, cx + radius, cy);
            path.Close();
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

        private static CssColor GetAccentColor(StyledElement element)
        {
            return element.Style.AccentColor;
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

            // Determine color based on value relative to low/high/optimum
            CssColor barColor;
            CssColor accentColor = GetAccentColor(element);
            if (accentColor.R != 0 || accentColor.G != 0 || accentColor.B != 0 || accentColor.A != 0)
            {
                barColor = accentColor;
            }
            else if (value < low)
            {
                // Below low threshold — use red/warning
                barColor = optimum >= high
                    ? new CssColor(220, 50, 50) // danger (optimum is high, value is low)
                    : new CssColor(200, 180, 0); // caution
            }
            else if (value > high)
            {
                // Above high threshold
                barColor = optimum <= low
                    ? new CssColor(220, 50, 50) // danger
                    : new CssColor(200, 180, 0); // caution
            }
            else
            {
                // In optimal range — green
                barColor = new CssColor(50, 180, 50);
            }

            // Draw background track (gray)
            var trackColor = new CssColor(220, 220, 220);
            target.FillRect(rect, BrushInfo.Solid(trackColor));

            // Draw filled bar
            float barWidth = rect.Width * fraction;
            if (barWidth > 0)
            {
                var barRect = new RectF(rect.X, rect.Y, barWidth, rect.Height);
                target.FillRect(barRect, BrushInfo.Solid(barColor));
            }

            // Draw 1px border
            target.StrokeRect(rect, new PenInfo(BorderColor, 1f));
        }

        private static void PaintProgress(StyledElement element, LayoutBox box, IRenderTarget target)
        {
            RectF rect = box.ContentRect;

            // Parse attributes
            float max = ParseFloat(element.GetAttribute("max"), 1f);
            string? valueAttr = element.GetAttribute("value");

            // Draw background track
            var trackColor = new CssColor(220, 220, 220);
            target.FillRect(rect, BrushInfo.Solid(trackColor));

            if (valueAttr != null)
            {
                // Determinate progress bar
                float value = ParseFloat(valueAttr, 0f);
                float fraction = max > 0 ? value / max : 0f;
                fraction = Math.Max(0f, Math.Min(1f, fraction));

                // Determine bar color
                CssColor barColor;
                CssColor accentColor = GetAccentColor(element);
                if (accentColor.R != 0 || accentColor.G != 0 || accentColor.B != 0 || accentColor.A != 0)
                {
                    barColor = accentColor;
                }
                else
                {
                    barColor = new CssColor(50, 120, 220); // Blue
                }

                float barWidth = rect.Width * fraction;
                if (barWidth > 0)
                {
                    var barRect = new RectF(rect.X, rect.Y, barWidth, rect.Height);
                    target.FillRect(barRect, BrushInfo.Solid(barColor));
                }
            }
            else
            {
                // Indeterminate: draw striped pattern
                CssColor stripeColor = new CssColor(50, 120, 220);
                float stripeWidth = 20f;
                float x = rect.X;
                while (x < rect.X + rect.Width)
                {
                    float w = Math.Min(stripeWidth, rect.X + rect.Width - x);
                    var stripeRect = new RectF(x, rect.Y, w, rect.Height);
                    target.FillRect(stripeRect, BrushInfo.Solid(stripeColor));
                    x += stripeWidth * 2; // skip one stripe width for gap
                }
            }

            // Draw 1px border
            target.StrokeRect(rect, new PenInfo(BorderColor, 1f));
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
