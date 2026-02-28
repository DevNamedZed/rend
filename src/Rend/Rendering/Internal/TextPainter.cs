using Rend.Core.Values;
using Rend.Css;
using Rend.Fonts;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints text content from line fragments, including text decorations
    /// such as underline, overline, and line-through.
    /// </summary>
    internal static class TextPainter
    {
        /// <summary>
        /// Paints a single line fragment onto the render target.
        /// </summary>
        /// <param name="fragment">The line fragment to paint.</param>
        /// <param name="lineX">The X position of the parent line box.</param>
        /// <param name="lineY">The Y position of the parent line box.</param>
        /// <param name="lineBaseline">The baseline offset from the top of the line box.</param>
        /// <param name="target">The render target to draw on.</param>
        /// <param name="style">The computed style for the text.</param>
        public static void Paint(LineFragment fragment, float lineX, float lineY,
                                 float lineBaseline, IRenderTarget target, ComputedStyle style)
        {
            float drawX = lineX + fragment.X;
            float drawY = lineY + fragment.Y + fragment.Baseline;

            CssColor color = style.Color;
            float fontSize = style.FontSize;
            string fontFamily = style.FontFamily;
            CssFontStyle fontStyle = style.FontStyle;
            float fontWeight = style.FontWeight;

            if (fragment.ShapedRun != null)
            {
                var fontDesc = new FontDescriptor(fontFamily, fontWeight, fontStyle);
                target.DrawGlyphs(fragment.ShapedRun, drawX, drawY, color, fontDesc);
            }
            else if (fragment.Text != null)
            {
                var textStyle = new TextStyle
                {
                    Font = new FontDescriptor(fontFamily, fontWeight, fontStyle),
                    FontSize = fontSize,
                    Color = color,
                    Bold = fontWeight >= 700f,
                    Italic = fontStyle == CssFontStyle.Italic || fontStyle == CssFontStyle.Oblique
                };
                target.DrawText(fragment.Text, drawX, drawY, textStyle);
            }

            // Paint text decorations.
            PaintDecorations(fragment, lineX, lineY, lineBaseline, target, style);
        }

        private static void PaintDecorations(LineFragment fragment, float lineX, float lineY,
                                              float lineBaseline, IRenderTarget target, ComputedStyle style)
        {
            CssTextDecorationLine decoration = style.TextDecorationLine;
            if (decoration == CssTextDecorationLine.None)
            {
                return;
            }

            CssColor color = style.Color;
            if (color.A == 0)
            {
                return;
            }

            float fontSize = style.FontSize;
            // Decoration stroke width is typically 1/16th of font size, at minimum 1px.
            float strokeWidth = fontSize > 16f ? fontSize / 16f : 1f;
            var pen = new PenInfo(color, strokeWidth);

            float startX = lineX + fragment.X;
            float endX = startX + fragment.Width;

            if (decoration == CssTextDecorationLine.Underline)
            {
                // Underline sits slightly below the baseline.
                float underlineY = lineY + fragment.Y + fragment.Baseline + fontSize * 0.15f;
                DrawLine(target, pen, startX, underlineY, endX, underlineY);
            }
            else if (decoration == CssTextDecorationLine.Overline)
            {
                // Overline sits at the top of the text.
                float overlineY = lineY + fragment.Y;
                DrawLine(target, pen, startX, overlineY, endX, overlineY);
            }
            else if (decoration == CssTextDecorationLine.LineThrough)
            {
                // Line-through goes through the middle of the text.
                float strikeY = lineY + fragment.Y + fragment.Height * 0.5f;
                DrawLine(target, pen, startX, strikeY, endX, strikeY);
            }
        }

        private static void DrawLine(IRenderTarget target, PenInfo pen, float x1, float y1, float x2, float y2)
        {
            var path = new PathData();
            path.MoveTo(x1, y1);
            path.LineTo(x2, y2);
            target.StrokePath(path, pen);
        }
    }
}
