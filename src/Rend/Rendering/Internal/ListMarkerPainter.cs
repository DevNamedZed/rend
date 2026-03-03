using System;
using System.Globalization;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Fonts;
using Rend.Layout;
using Rend.Style;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints list markers (bullets, counters, and images) for list-item boxes.
    /// </summary>
    internal static class ListMarkerPainter
    {
        private const float MarkerOffset = 8f;

        /// <summary>
        /// Paints the list marker for a list-item box. If list-style-image is set,
        /// draws the image. Otherwise, for disc, circle, and square types, draws a
        /// graphical marker. For numeric and alpha types, draws counter text.
        /// </summary>
        public static void Paint(LayoutBox box, IRenderTarget target, int itemIndex,
                                 ImageResolverDelegate? imageResolver = null)
        {
            if (box.BoxType != BoxType.ListItem)
            {
                return;
            }

            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
            {
                return;
            }

            // Check for <summary> element — render disclosure triangle instead of normal marker
            if (box.StyledNode is StyledElement styledEl && styledEl.TagName == "summary")
            {
                bool isOpen = false;
                // Check if parent <details> has 'open' attribute
                var parentEl = styledEl.Element.Parent as Rend.Html.Element;
                if (parentEl != null && parentEl.TagName == "details")
                {
                    isOpen = parentEl.GetAttribute("open") != null;
                }
                PaintDisclosureTriangle(target, box, style, isOpen);
                return;
            }

            CssListStyleType listType = style.ListStyleType;

            RectF contentRect = box.ContentRect;
            CssColor color = style.Color;
            float fontSize = style.FontSize;
            bool isInside = style.ListStylePosition == CssListStylePosition.Inside;

            // Try list-style-image first
            if (imageResolver != null && TryPaintImageMarker(box, target, style, imageResolver, contentRect, fontSize, isInside))
            {
                return;
            }

            if (listType == CssListStyleType.None)
            {
                return;
            }

            // Chrome sizes bullets at ~0.3em diameter
            float bulletRadius = fontSize * 0.15f;

            // Compute actual pixel line-height for the first line
            float rawLh = style.LineHeight;
            float pixelLineHeight;
            if (rawLh < 0) // Negative = unitless multiplier (e.g., -1.4 for line-height: 1.4)
                pixelLineHeight = Math.Abs(rawLh) * fontSize;
            else if (rawLh == 0) // normal
                pixelLineHeight = fontSize * 1.2f;
            else
                pixelLineHeight = rawLh; // Already in pixels

            // Chrome centers list bullets vertically on the first line
            float markerCenterY = contentRect.Y + pixelLineHeight * 0.5f;
            // Outside: marker drawn to the left of content area.
            // Inside: marker drawn at the start of content area (text is indented to make room).
            float markerX = isInside
                ? contentRect.X + bulletRadius + 2f
                : contentRect.X - MarkerOffset;

            switch (listType)
            {
                case CssListStyleType.Disc:
                    PaintDisc(target, markerX, markerCenterY, bulletRadius, color);
                    break;

                case CssListStyleType.Circle:
                    PaintCircle(target, markerX, markerCenterY, bulletRadius, color);
                    break;

                case CssListStyleType.Square:
                    PaintSquare(target, markerX, markerCenterY, bulletRadius, color);
                    break;

                default:
                    string? markerText = GetMarkerText(listType, itemIndex);
                    if (markerText != null)
                    {
                        PaintCounterText(target, markerText, contentRect, color, fontSize, style, isInside);
                    }
                    break;
            }
        }

        private static bool TryPaintImageMarker(LayoutBox box, IRenderTarget target,
            ComputedStyle style, ImageResolverDelegate imageResolver,
            RectF contentRect, float fontSize, bool isInside)
        {
            object? imageRef = style.GetRefValue(PropertyId.ListStyleImage);
            if (imageRef == null) return false;

            string? imageUrl = null;
            if (imageRef is string s && s != "none")
            {
                imageUrl = s;
            }
            else if (imageRef is CssUrlValue urlVal)
            {
                imageUrl = urlVal.Url;
            }

            if (imageUrl == null) return false;

            ImageData? imageData = imageResolver(imageUrl);
            if (imageData == null) return false;

            // Size marker image to fontSize x fontSize
            float size = fontSize;
            float markerY = contentRect.Y;
            float markerX;
            if (isInside)
            {
                markerX = contentRect.X;
            }
            else
            {
                markerX = contentRect.X - size - 4f;
            }

            target.DrawImage(imageData, new RectF(markerX, markerY, size, size));
            return true;
        }

        private static void PaintDisc(IRenderTarget target, float cx, float cy, float radius, CssColor color)
        {
            // Approximate a filled circle with a small rounded rectangle.
            var rect = new RectF(cx - radius, cy - radius, radius * 2f, radius * 2f);
            var path = new PathData();
            path.AddRoundedRectangle(rect, radius, radius, radius, radius);
            target.FillPath(path, BrushInfo.Solid(color));
        }

        private static void PaintCircle(IRenderTarget target, float cx, float cy, float radius, CssColor color)
        {
            // Approximate a stroked circle with a rounded rectangle outline.
            var rect = new RectF(cx - radius, cy - radius, radius * 2f, radius * 2f);
            var path = new PathData();
            path.AddRoundedRectangle(rect, radius, radius, radius, radius);
            var pen = new PenInfo(color, 0.8f);
            target.StrokePath(path, pen);
        }

        private static void PaintSquare(IRenderTarget target, float cx, float cy, float radius, CssColor color)
        {
            float size = radius * 1.6f;
            var rect = new RectF(cx - size * 0.5f, cy - size * 0.5f, size, size);
            target.FillRect(rect, BrushInfo.Solid(color));
        }

        private static void PaintCounterText(IRenderTarget target, string text, RectF contentRect,
                                               CssColor color, float fontSize, ComputedStyle style,
                                               bool isInside)
        {
            string fontFamily = style.FontFamily;
            float fontWeight = style.FontWeight;
            CssFontStyle fontStyle = style.FontStyle;

            var textStyle = new TextStyle
            {
                Font = new FontDescriptor(fontFamily, fontWeight, fontStyle, FontDescriptor.StretchToPercentage(style.FontStretch)),
                FontSize = fontSize,
                Color = color,
                Bold = fontWeight >= 700f,
                Italic = fontStyle == CssFontStyle.Italic || fontStyle == CssFontStyle.Oblique
            };

            // Compute pixel line height for vertical centering
            float rawLh = style.LineHeight;
            float pixelLineHeight;
            if (rawLh < 0) pixelLineHeight = Math.Abs(rawLh) * fontSize;
            else if (rawLh == 0) pixelLineHeight = fontSize * 1.2f;
            else pixelLineHeight = rawLh;

            // Position at baseline using CSS half-leading model:
            // halfLeading = (lineHeight - contentArea) / 2
            // baseline Y = top + halfLeading + ascent
            // With typical ascent ratio of 0.8:
            float y = contentRect.Y + (pixelLineHeight - fontSize) / 2f + fontSize * 0.8f;

            if (isInside)
            {
                // Inside: draw at the start of the content area.
                float x = contentRect.X;
                target.DrawText(text, x, y, textStyle);
            }
            else
            {
                // Outside: draw to the left of the content area.
                float estimatedWidth = text.Length * fontSize * 0.6f;
                float x = contentRect.X - estimatedWidth - 4f;
                target.DrawText(text, x, y, textStyle);
            }
        }

        private static string? GetMarkerText(CssListStyleType listType, int index)
        {
            switch (listType)
            {
                case CssListStyleType.Decimal:
                    return index.ToString(CultureInfo.InvariantCulture) + ".";

                case CssListStyleType.DecimalLeadingZero:
                    return index.ToString("D2", CultureInfo.InvariantCulture) + ".";

                case CssListStyleType.LowerAlpha:
                case CssListStyleType.LowerLatin:
                    return ToAlpha(index, lowercase: true) + ".";

                case CssListStyleType.UpperAlpha:
                case CssListStyleType.UpperLatin:
                    return ToAlpha(index, lowercase: false) + ".";

                case CssListStyleType.LowerRoman:
                    return ToRoman(index).ToLowerInvariant() + ".";

                case CssListStyleType.UpperRoman:
                    return ToRoman(index) + ".";

                default:
                    return null;
            }
        }

        private static string ToAlpha(int index, bool lowercase)
        {
            if (index <= 0)
            {
                return index.ToString(CultureInfo.InvariantCulture);
            }

            // Convert 1-based index to a-z, aa-az, etc.
            char[] buffer = new char[8];
            int pos = buffer.Length;
            int val = index - 1;

            do
            {
                pos--;
                int remainder = val % 26;
                buffer[pos] = (char)((lowercase ? 'a' : 'A') + remainder);
                val = val / 26 - 1;
            }
            while (val >= 0 && pos > 0);

            return new string(buffer, pos, buffer.Length - pos);
        }

        private static string ToRoman(int number)
        {
            if (number <= 0 || number > 3999)
            {
                return number.ToString(CultureInfo.InvariantCulture);
            }

            // Standard roman numeral conversion.
            int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            string[] numerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

            var result = new System.Text.StringBuilder(15);
            int remaining = number;

            for (int i = 0; i < values.Length; i++)
            {
                while (remaining >= values[i])
                {
                    result.Append(numerals[i]);
                    remaining -= values[i];
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Paint a disclosure triangle for a &lt;summary&gt; element.
        /// ▼ for open, ▶ for closed.
        /// </summary>
        private static void PaintDisclosureTriangle(IRenderTarget target, LayoutBox box,
            ComputedStyle style, bool isOpen)
        {
            RectF contentRect = box.ContentRect;
            CssColor color = style.Color;
            float fontSize = style.FontSize;

            // Triangle size: roughly 0.4em
            float size = fontSize * 0.4f;

            // Compute pixel line height for vertical centering
            float rawLh = style.LineHeight;
            float pixelLineHeight;
            if (rawLh < 0) pixelLineHeight = Math.Abs(rawLh) * fontSize;
            else if (rawLh == 0) pixelLineHeight = fontSize * 1.2f;
            else pixelLineHeight = rawLh;

            float centerY = contentRect.Y + pixelLineHeight * 0.5f;
            float centerX = contentRect.X + size * 0.5f + 2f;

            var path = new PathData();
            if (isOpen)
            {
                // Downward pointing triangle ▼
                float halfW = size * 0.5f;
                float halfH = size * 0.45f;
                path.MoveTo(centerX - halfW, centerY - halfH);
                path.LineTo(centerX + halfW, centerY - halfH);
                path.LineTo(centerX, centerY + halfH);
                path.Close();
            }
            else
            {
                // Right pointing triangle ▶
                float halfW = size * 0.45f;
                float halfH = size * 0.5f;
                path.MoveTo(centerX - halfW, centerY - halfH);
                path.LineTo(centerX + halfW, centerY);
                path.LineTo(centerX - halfW, centerY + halfH);
                path.Close();
            }

            target.FillPath(path, BrushInfo.Solid(color));
        }
    }
}
