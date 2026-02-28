using System;
using System.Globalization;
using Rend.Core.Values;
using Rend.Css;
using Rend.Fonts;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints list markers (bullets and counters) for list-item boxes.
    /// </summary>
    internal static class ListMarkerPainter
    {
        private const float MarkerOffset = 8f;
        private const float BulletRadius = 3f;

        /// <summary>
        /// Paints the list marker for a list-item box. For disc, circle, and square
        /// types, draws a graphical marker. For numeric and alpha types, draws
        /// counter text to the left of the content area.
        /// </summary>
        /// <param name="box">The layout box to paint a marker for.</param>
        /// <param name="target">The render target to draw on.</param>
        /// <param name="itemIndex">The 1-based ordinal index for numeric/alpha markers.</param>
        public static void Paint(LayoutBox box, IRenderTarget target, int itemIndex)
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

            CssListStyleType listType = style.ListStyleType;
            if (listType == CssListStyleType.None)
            {
                return;
            }

            RectF contentRect = box.ContentRect;
            CssColor color = style.Color;
            float fontSize = style.FontSize;

            // Vertical center of the first line (approximate as top + fontSize * 0.5).
            float markerCenterY = contentRect.Y + fontSize * 0.5f;
            float markerX = contentRect.X - MarkerOffset;

            switch (listType)
            {
                case CssListStyleType.Disc:
                    PaintDisc(target, markerX, markerCenterY, color);
                    break;

                case CssListStyleType.Circle:
                    PaintCircle(target, markerX, markerCenterY, color);
                    break;

                case CssListStyleType.Square:
                    PaintSquare(target, markerX, markerCenterY, color);
                    break;

                default:
                    string? markerText = GetMarkerText(listType, itemIndex);
                    if (markerText != null)
                    {
                        PaintCounterText(target, markerText, contentRect, color, fontSize, style);
                    }
                    break;
            }
        }

        private static void PaintDisc(IRenderTarget target, float cx, float cy, CssColor color)
        {
            // Approximate a filled circle with a small rounded rectangle.
            var rect = new RectF(cx - BulletRadius, cy - BulletRadius, BulletRadius * 2f, BulletRadius * 2f);
            var path = new PathData();
            path.AddRoundedRectangle(rect, BulletRadius, BulletRadius, BulletRadius, BulletRadius);
            target.FillPath(path, BrushInfo.Solid(color));
        }

        private static void PaintCircle(IRenderTarget target, float cx, float cy, CssColor color)
        {
            // Approximate a stroked circle with a rounded rectangle outline.
            var rect = new RectF(cx - BulletRadius, cy - BulletRadius, BulletRadius * 2f, BulletRadius * 2f);
            var path = new PathData();
            path.AddRoundedRectangle(rect, BulletRadius, BulletRadius, BulletRadius, BulletRadius);
            var pen = new PenInfo(color, 1f);
            target.StrokePath(path, pen);
        }

        private static void PaintSquare(IRenderTarget target, float cx, float cy, CssColor color)
        {
            float size = BulletRadius * 1.6f;
            var rect = new RectF(cx - size * 0.5f, cy - size * 0.5f, size, size);
            target.FillRect(rect, BrushInfo.Solid(color));
        }

        private static void PaintCounterText(IRenderTarget target, string text, RectF contentRect,
                                               CssColor color, float fontSize, ComputedStyle style)
        {
            string fontFamily = style.FontFamily;
            float fontWeight = style.FontWeight;
            CssFontStyle fontStyle = style.FontStyle;

            var textStyle = new TextStyle
            {
                Font = new FontDescriptor(fontFamily, fontWeight, fontStyle),
                FontSize = fontSize,
                Color = color,
                Bold = fontWeight >= 700f,
                Italic = fontStyle == CssFontStyle.Italic || fontStyle == CssFontStyle.Oblique
            };

            // Draw the counter text to the left of the content area.
            // Estimate width: approximate at 0.6 * fontSize per character.
            float estimatedWidth = text.Length * fontSize * 0.6f;
            float x = contentRect.X - estimatedWidth - 4f;
            float y = contentRect.Y + fontSize;

            target.DrawText(text, x, y, textStyle);
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
    }
}
