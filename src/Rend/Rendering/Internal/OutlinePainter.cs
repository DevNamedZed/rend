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
                    float dashLen, gapLen;
                    if (width >= 3f)
                    {
                        dashLen = width * 2f;
                        gapLen = width;
                    }
                    else
                    {
                        dashLen = width * 3f;
                        gapLen = width * 2f;
                    }
                    dashLen = Math.Max(dashLen, 1f);
                    gapLen = Math.Max(gapLen, 1f);
                    StrokeOutlinePerSide(target, outlineRect, color, width, dashLen, gapLen);
                    break;
                }

                case CssBorderStyle.Dotted:
                {
                    float dotLen = Math.Max(width, 1f);
                    StrokeOutlinePerSide(target, outlineRect, color, width, dotLen, dotLen);
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

        private static void StrokeOutlinePerSide(IRenderTarget target, RectF rect,
            CssColor color, float width, float dashLen, float gapLen)
        {
            float halfW = width * 0.5f;
            float left = rect.X;
            float top = rect.Y;
            float right = rect.X + rect.Width;
            float bottom = rect.Y + rect.Height;

            float horizontalLength = rect.Width + width;
            float verticalLength = rect.Height + width;

            StrokeOneSide(target, color, width, dashLen, gapLen, horizontalLength,
                left - halfW, top, right + halfW, top);
            StrokeOneSide(target, color, width, dashLen, gapLen, verticalLength,
                right, top - halfW, right, bottom + halfW);
            StrokeOneSide(target, color, width, dashLen, gapLen, horizontalLength,
                right + halfW, bottom, left - halfW, bottom);
            StrokeOneSide(target, color, width, dashLen, gapLen, verticalLength,
                left, bottom + halfW, left, top - halfW);
        }

        private static void StrokeOneSide(IRenderTarget target, CssColor color, float width,
            float dashLen, float gapLen, float sideLength,
            float x1, float y1, float x2, float y2)
        {
            if (sideLength <= 0f)
            {
                return;
            }

            float adjustedGap = Math.Max(SelectBestDashGap(sideLength, dashLen, gapLen), 1f);
            float[] dashPattern = new[] { dashLen, adjustedGap };
            var pen = new PenInfo(color, width, dashPattern);

            var path = new PathData();
            path.MoveTo(x1, y1);
            path.LineTo(x2, y2);
            target.StrokePath(path, pen);
        }

        private static float SelectBestDashGap(float strokeLength, float dashLength, float gapLength)
        {
            float availableLength = strokeLength + gapLength;
            float minNumDashes = (float)Math.Floor(availableLength / (dashLength + gapLength));
            float maxNumDashes = minNumDashes + 1;
            float minNumGaps = minNumDashes - 1;
            float maxNumGaps = maxNumDashes - 1;
            if (minNumGaps <= 0 || maxNumGaps <= 0)
            {
                return gapLength;
            }
            float minGap = (strokeLength - minNumDashes * dashLength) / minNumGaps;
            float maxGap = (strokeLength - maxNumDashes * dashLength) / maxNumGaps;
            return (maxGap <= 0) || (Math.Abs(minGap - gapLength) < Math.Abs(maxGap - gapLength))
                ? minGap
                : maxGap;
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
