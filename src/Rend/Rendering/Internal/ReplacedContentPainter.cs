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
                float textY = rect.Y + (rect.Height - FormFontSize) / 2f;
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
            // White background
            target.FillRect(rect, BrushInfo.Solid(CssColor.White));

            // 1px border
            target.StrokeRect(rect, new PenInfo(BorderColor, 1f));

            // Draw checkmark if checked
            bool isChecked = element.GetAttribute("checked") != null;
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

                target.StrokePath(path, new PenInfo(CheckmarkColor, 1.5f));
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

            // Outer circle (white fill + border)
            var outerCircle = BuildCirclePath(cx, cy, radius);
            target.FillPath(outerCircle, BrushInfo.Solid(CssColor.White));
            target.StrokePath(outerCircle, new PenInfo(BorderColor, 1f));

            // Inner filled circle if checked
            bool isChecked = element.GetAttribute("checked") != null;
            if (isChecked)
            {
                float innerRadius = radius * 0.45f;
                var innerCircle = BuildCirclePath(cx, cy, innerRadius);
                target.FillPath(innerCircle, BrushInfo.Solid(CheckmarkColor));
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
            float textY = rect.Y + (rect.Height - FormFontSize) / 2f;

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

                float textY = rect.Y + (rect.Height - FormFontSize) / 2f;
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
    }

    /// <summary>
    /// A delegate that resolves a source URL to an <see cref="ImageData"/> instance,
    /// or returns null if the image could not be loaded.
    /// </summary>
    /// <param name="src">The image source URL or data URI.</param>
    /// <returns>The resolved image data, or null.</returns>
    internal delegate ImageData? ImageResolverDelegate(string src);
}
