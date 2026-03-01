using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints CSS outlines around layout boxes. Unlike borders, outlines don't
    /// affect layout and are drawn outside the border edge (plus outline-offset).
    /// </summary>
    internal static class OutlinePainter
    {
        public static void Paint(LayoutBox box, IRenderTarget target)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
                return;

            CssBorderStyle outlineStyle = style.OutlineStyle;
            if (outlineStyle == CssBorderStyle.None || outlineStyle == CssBorderStyle.Hidden)
                return;

            float width = style.OutlineWidth;
            if (width <= 0f)
                return;

            CssColor color = style.OutlineColor;
            if (color.A == 0)
                return;

            float offset = style.OutlineOffset;
            RectF borderRect = box.BorderRect;

            // Expand the border rect by outline-offset + half the outline width
            // so the stroke is centered on the outline position.
            float expand = offset + width * 0.5f;
            var outlineRect = new RectF(
                borderRect.X - expand,
                borderRect.Y - expand,
                borderRect.Width + expand * 2f,
                borderRect.Height + expand * 2f);

            switch (outlineStyle)
            {
                case CssBorderStyle.Dashed:
                {
                    float dashLen = Math.Max(width * 3f, 1f);
                    var pen = new PenInfo(color, width, new[] { dashLen, dashLen });
                    target.StrokeRect(outlineRect, pen);
                    break;
                }

                case CssBorderStyle.Dotted:
                {
                    float dotLen = Math.Max(width, 1f);
                    var pen = new PenInfo(color, width, new[] { dotLen, dotLen });
                    target.StrokeRect(outlineRect, pen);
                    break;
                }

                case CssBorderStyle.Double:
                {
                    if (width < 3f)
                    {
                        // Too thin for double — draw solid.
                        target.StrokeRect(outlineRect, new PenInfo(color, width));
                    }
                    else
                    {
                        float third = width / 3f;
                        // Outer stroke
                        float outerExpand = offset + width - third * 0.5f;
                        var outerRect = new RectF(
                            borderRect.X - outerExpand,
                            borderRect.Y - outerExpand,
                            borderRect.Width + outerExpand * 2f,
                            borderRect.Height + outerExpand * 2f);
                        target.StrokeRect(outerRect, new PenInfo(color, third));

                        // Inner stroke
                        float innerExpand = offset + third * 0.5f;
                        var innerRect = new RectF(
                            borderRect.X - innerExpand,
                            borderRect.Y - innerExpand,
                            borderRect.Width + innerExpand * 2f,
                            borderRect.Height + innerExpand * 2f);
                        target.StrokeRect(innerRect, new PenInfo(color, third));
                    }
                    break;
                }

                case CssBorderStyle.Groove:
                {
                    PaintGrooveRidgeOutline(target, outlineRect, borderRect, offset, width, color, isGroove: true);
                    break;
                }

                case CssBorderStyle.Ridge:
                {
                    PaintGrooveRidgeOutline(target, outlineRect, borderRect, offset, width, color, isGroove: false);
                    break;
                }

                case CssBorderStyle.Inset:
                {
                    PaintInsetOutsetOutline(target, outlineRect, borderRect, offset, width, color, isInset: true);
                    break;
                }

                case CssBorderStyle.Outset:
                {
                    PaintInsetOutsetOutline(target, outlineRect, borderRect, offset, width, color, isInset: false);
                    break;
                }

                default:
                {
                    // Solid and all other styles fallback to solid.
                    target.StrokeRect(outlineRect, new PenInfo(color, width));
                    break;
                }
            }
        }
        private static void PaintGrooveRidgeOutline(IRenderTarget target, RectF outlineRect,
            RectF borderRect, float offset, float width, CssColor color, bool isGroove)
        {
            if (width < 2f)
            {
                target.StrokeRect(outlineRect, new PenInfo(color, width));
                return;
            }

            float half = width * 0.5f;
            var dark = Darken(color, 0.5f);
            var light = Lighten(color, 0.5f);

            // Outer half
            float outerExpand = offset + width - half * 0.5f;
            var outerRect = new RectF(
                borderRect.X - outerExpand,
                borderRect.Y - outerExpand,
                borderRect.Width + outerExpand * 2f,
                borderRect.Height + outerExpand * 2f);
            target.StrokeRect(outerRect, new PenInfo(isGroove ? dark : light, half));

            // Inner half
            float innerExpand = offset + half * 0.5f;
            var innerRect = new RectF(
                borderRect.X - innerExpand,
                borderRect.Y - innerExpand,
                borderRect.Width + innerExpand * 2f,
                borderRect.Height + innerExpand * 2f);
            target.StrokeRect(innerRect, new PenInfo(isGroove ? light : dark, half));
        }

        private static void PaintInsetOutsetOutline(IRenderTarget target, RectF outlineRect,
            RectF borderRect, float offset, float width, CssColor color, bool isInset)
        {
            var drawColor = isInset ? Darken(color, 0.4f) : Lighten(color, 0.4f);
            target.StrokeRect(outlineRect, new PenInfo(drawColor, width));
        }

        private static CssColor Darken(CssColor c, float factor)
        {
            return new CssColor(
                (byte)(c.R * (1f - factor)),
                (byte)(c.G * (1f - factor)),
                (byte)(c.B * (1f - factor)),
                c.A);
        }

        private static CssColor Lighten(CssColor c, float factor)
        {
            return new CssColor(
                (byte)Math.Min(255, c.R + (255 - c.R) * factor),
                (byte)Math.Min(255, c.G + (255 - c.G) * factor),
                (byte)Math.Min(255, c.B + (255 - c.B) * factor),
                c.A);
        }
    }
}
