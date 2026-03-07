using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Layout;
using Rend.Style;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints the borders of a layout box, handling each side independently
    /// with support for different styles, colors, and border-radius.
    /// </summary>
    internal static class BorderPainter
    {
        internal static bool _debugCollapse = false;
        /// <summary>
        /// Paints all four borders of the given box onto the render target.
        /// Each side is drawn only if it has a non-none style and a width greater than zero.
        /// </summary>
        /// <param name="box">The layout box whose borders to paint.</param>
        /// <param name="target">The render target to draw on.</param>
        public static void Paint(LayoutBox box, IRenderTarget target)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
            {
                return;
            }

            float topW = box.BorderTopWidth;
            float rightW = box.BorderRightWidth;
            float bottomW = box.BorderBottomWidth;
            float leftW = box.BorderLeftWidth;

            CssBorderStyle topStyle = style.BorderTopStyle;
            CssBorderStyle rightStyle = style.BorderRightStyle;
            CssBorderStyle bottomStyle = style.BorderBottomStyle;
            CssBorderStyle leftStyle = style.BorderLeftStyle;

            // Check for border-radius.
            var radii = BorderRadiusResolver.Resolve(style, box.BorderRect);
            bool hasRadius = radii.HasRadius;

            if (hasRadius)
            {
                PaintWithRadius(box, target, style, topW, rightW, bottomW, leftW,
                                topStyle, rightStyle, bottomStyle, leftStyle,
                                radii);
                return;
            }

            RectF borderRect = box.BorderRect;

            float outerLeft = borderRect.Left;
            float outerTop = borderRect.Top;
            float outerRight = borderRect.Right;
            float outerBottom = borderRect.Bottom;

            // In border-collapse mode, layout stores half-widths for positioning.
            // The borderRect edges are at the grid lines between cells.
            // Expand outward by half the full border width on each side so the
            // collapsed border is centered on the grid line (matching Chrome).
            if (box.CollapsedBorderCell)
            {
                // Grid lines are at the current borderRect edges (before expansion).
                float gridTop = outerTop;
                float gridBottom = outerBottom;
                float gridLeft = outerLeft;
                float gridRight = outerRight;

                topW *= 2f;
                rightW *= 2f;
                bottomW *= 2f;
                leftW *= 2f;

                // Center the full border on each grid line:
                // expand outward by half the full border width.
                float halfTop = topW / 2f;
                float halfBottom = bottomW / 2f;
                float halfLeft = leftW / 2f;
                float halfRight = rightW / 2f;

                outerLeft = (float)Math.Round(gridLeft - halfLeft);
                outerTop = (float)Math.Round(gridTop - halfTop);
                outerRight = (float)Math.Round(gridRight + halfRight);
                outerBottom = (float)Math.Round(gridBottom + halfBottom);

                if (_debugCollapse)
                    Console.Error.WriteLine($"[BORDER] grid=({gridLeft:F2},{gridTop:F2})-({gridRight:F2},{gridBottom:F2}) outer=({outerLeft:F1},{outerTop:F1})-({outerRight:F1},{outerBottom:F1}) widths=L{leftW} T{topW} R{rightW} B{bottomW}");
            }

            float innerLeft = outerLeft + leftW;
            float innerTop = outerTop + topW;
            float innerRight = outerRight - rightW;
            float innerBottom = outerBottom - bottomW;

            // Use resolved collapsed border colors when available
            CssColor topColor = box.CollapsedBorderTopColor ?? style.BorderTopColor;
            CssColor rightColor = box.CollapsedBorderRightColor ?? style.BorderRightColor;
            CssColor bottomColor = box.CollapsedBorderBottomColor ?? style.BorderBottomColor;
            CssColor leftColor = box.CollapsedBorderLeftColor ?? style.BorderLeftColor;

            // Chrome draws rectangular borders as rectangles (not trapezoids):
            // Top/bottom span full width (owning the corners), left/right fill between them.
            // When adjacent borders have DIFFERENT colors, use diagonal (trapezoid) corner joins.
            bool topLeftDiag = NeedsDiagonalCorner(topW, topStyle, topColor, leftW, leftStyle, leftColor);
            bool topRightDiag = NeedsDiagonalCorner(topW, topStyle, topColor, rightW, rightStyle, rightColor);
            bool bottomLeftDiag = NeedsDiagonalCorner(bottomW, bottomStyle, bottomColor, leftW, leftStyle, leftColor);
            bool bottomRightDiag = NeedsDiagonalCorner(bottomW, bottomStyle, bottomColor, rightW, rightStyle, rightColor);

            // Top border
            if (topW > 0f && topStyle != CssBorderStyle.None && topStyle != CssBorderStyle.Hidden)
            {
                CssColor color = topColor;

                // Fieldset + legend: split the top border around the legend gap
                var legendGap = GetFieldsetLegendGap(box);
                if (legendGap.HasValue)
                {
                    float gapLeft = legendGap.Value.Left;
                    float gapRight = legendGap.Value.Right;

                    if (gapLeft > outerLeft)
                    {
                        PaintSide(target, color, topStyle, topW,
                                  outerLeft, outerTop, gapLeft, outerTop,
                                  gapLeft, innerTop, topLeftDiag ? innerLeft : outerLeft, innerTop);
                    }

                    if (gapRight < outerRight)
                    {
                        PaintSide(target, color, topStyle, topW,
                                  gapRight, outerTop, outerRight, outerTop,
                                  topRightDiag ? innerRight : outerRight, innerTop, gapRight, innerTop);
                    }
                }
                else
                {
                    // Use rectangle corners where adjacent borders share the same color
                    float il = topLeftDiag ? innerLeft : outerLeft;
                    float ir = topRightDiag ? innerRight : outerRight;
                    PaintSide(target, color, topStyle, topW,
                              outerLeft, outerTop, outerRight, outerTop,
                              ir, innerTop, il, innerTop);
                }
            }

            // Bottom border
            if (bottomW > 0f && bottomStyle != CssBorderStyle.None && bottomStyle != CssBorderStyle.Hidden)
            {
                CssColor color = bottomColor;
                float il = bottomLeftDiag ? innerLeft : outerLeft;
                float ir = bottomRightDiag ? innerRight : outerRight;
                PaintSide(target, color, bottomStyle, bottomW,
                          outerRight, outerBottom, outerLeft, outerBottom,
                          il, innerBottom, ir, innerBottom);
            }

            // Left border
            if (leftW > 0f && leftStyle != CssBorderStyle.None && leftStyle != CssBorderStyle.Hidden)
            {
                CssColor color = leftColor;
                // Left border: if no diagonal at top-left, start from innerTop; else outerTop
                float ot = topLeftDiag ? outerTop : innerTop;
                float ob = bottomLeftDiag ? outerBottom : innerBottom;
                PaintSide(target, color, leftStyle, leftW,
                          outerLeft, ob, outerLeft, ot,
                          innerLeft, topLeftDiag ? innerTop : ot, innerLeft, bottomLeftDiag ? innerBottom : ob);
            }

            // Right border
            if (rightW > 0f && rightStyle != CssBorderStyle.None && rightStyle != CssBorderStyle.Hidden)
            {
                CssColor color = rightColor;
                float ot = topRightDiag ? outerTop : innerTop;
                float ob = bottomRightDiag ? outerBottom : innerBottom;
                PaintSide(target, color, rightStyle, rightW,
                          outerRight, ot, outerRight, ob,
                          innerRight, bottomRightDiag ? innerBottom : ob, innerRight, topRightDiag ? innerTop : ot);
            }
        }

        /// <summary>
        /// Returns true if two adjacent borders need a diagonal (trapezoid) corner join
        /// because they have different colors. When same color, use rectangular overlap
        /// (top/bottom border covers the corner, left/right is shorter).
        /// </summary>
        private static bool NeedsDiagonalCorner(float widthA, CssBorderStyle styleA, CssColor colorA,
                                                 float widthB, CssBorderStyle styleB, CssColor colorB)
        {
            // No diagonal if either side is invisible
            if (widthA <= 0 || styleA == CssBorderStyle.None || styleA == CssBorderStyle.Hidden)
                return false;
            if (widthB <= 0 || styleB == CssBorderStyle.None || styleB == CssBorderStyle.Hidden)
                return false;
            // Diagonal needed only when colors differ
            return colorA.R != colorB.R || colorA.G != colorB.G || colorA.B != colorB.B || colorA.A != colorB.A;
        }

        /// <summary>
        /// For a fieldset box with a legend child, returns the horizontal gap
        /// (left x, right x) where the top border should be interrupted.
        /// Returns null if this is not a fieldset or has no legend child.
        /// </summary>
        private static (float Left, float Right)? GetFieldsetLegendGap(LayoutBox box)
        {
            if (box.StyledNode is not StyledElement elem || elem.TagName != "fieldset")
                return null;

            // Find the first legend child in the layout children
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                if (child.StyledNode is StyledElement childElem && childElem.TagName == "legend")
                {
                    // The gap spans the legend's margin box horizontally
                    RectF legendMargin = child.MarginRect;
                    return (legendMargin.Left, legendMargin.Right);
                }
            }

            return null;
        }

        private static void PaintSide(
            IRenderTarget target,
            CssColor color,
            CssBorderStyle borderStyle,
            float width,
            float outerX1, float outerY1,
            float outerX2, float outerY2,
            float innerX2, float innerY2,
            float innerX1, float innerY1)
        {
            if (color.A == 0)
            {
                return;
            }

            switch (borderStyle)
            {
                case CssBorderStyle.Solid:
                    FillTrapezoid(target, color, outerX1, outerY1, outerX2, outerY2, innerX2, innerY2, innerX1, innerY1);
                    break;

                case CssBorderStyle.Dashed:
                    StrokeDashed(target, color, width, outerX1, outerY1, outerX2, outerY2, innerX1, innerY1, innerX2, innerY2);
                    break;

                case CssBorderStyle.Dotted:
                    StrokeDotted(target, color, width, outerX1, outerY1, outerX2, outerY2, innerX1, innerY1, innerX2, innerY2);
                    break;

                case CssBorderStyle.Double:
                    PaintDouble(target, color, width, outerX1, outerY1, outerX2, outerY2, innerX2, innerY2, innerX1, innerY1);
                    break;

                case CssBorderStyle.Groove:
                    PaintGrooveRidge(target, color, width, true,
                                    outerX1, outerY1, outerX2, outerY2,
                                    innerX2, innerY2, innerX1, innerY1);
                    break;

                case CssBorderStyle.Ridge:
                    PaintGrooveRidge(target, color, width, false,
                                    outerX1, outerY1, outerX2, outerY2,
                                    innerX2, innerY2, innerX1, innerY1);
                    break;

                case CssBorderStyle.Inset:
                case CssBorderStyle.Outset:
                    PaintInsetOutset(target, color, borderStyle,
                                    outerX1, outerY1, outerX2, outerY2,
                                    innerX2, innerY2, innerX1, innerY1);
                    break;

                default:
                    FillTrapezoid(target, color, outerX1, outerY1, outerX2, outerY2, innerX2, innerY2, innerX1, innerY1);
                    break;
            }
        }

        internal static void FillTrapezoid(
            IRenderTarget target, CssColor color,
            float x1, float y1, float x2, float y2,
            float x3, float y3, float x4, float y4)
        {
            var path = new PathData();
            path.MoveTo(x1, y1);
            path.LineTo(x2, y2);
            path.LineTo(x3, y3);
            path.LineTo(x4, y4);
            path.Close();
            target.FillPath(path, BrushInfo.Solid(color));
        }

        /// <summary>
        /// Chrome's SelectBestDashGap: adjusts gap to distribute dashes evenly along the path.
        /// </summary>
        private static float SelectBestDashGap(float strokeLength, float dashLength, float gapLength)
        {
            float availableLength = strokeLength + gapLength; // open path
            float minNumDashes = (float)Math.Floor(availableLength / (dashLength + gapLength));
            float maxNumDashes = minNumDashes + 1;
            float minNumGaps = minNumDashes - 1;
            float maxNumGaps = maxNumDashes - 1;
            if (minNumGaps <= 0) return gapLength;
            float minGap = (strokeLength - minNumDashes * dashLength) / minNumGaps;
            float maxGap = maxNumGaps > 0 ? (strokeLength - maxNumDashes * dashLength) / maxNumGaps : -1;
            return (maxGap <= 0) || (Math.Abs(minGap - gapLength) < Math.Abs(maxGap - gapLength))
                ? minGap
                : maxGap;
        }

        private static void StrokeDashed(
            IRenderTarget target, CssColor color, float width,
            float outerX1, float outerY1, float outerX2, float outerY2,
            float innerX1, float innerY1, float innerX2, float innerY2)
        {
            float midX1 = (outerX1 + innerX1) * 0.5f;
            float midY1 = (outerY1 + innerY1) * 0.5f;
            float midX2 = (outerX2 + innerX2) * 0.5f;
            float midY2 = (outerY2 + innerY2) * 0.5f;

            // Chrome's StyledStrokeData: thickness >= 3 → dash=2*w, gap=1*w; < 3 → dash=3*w, gap=2*w
            float dashLen, gapLen;
            if (width >= 3f)
            {
                dashLen = width * 2f;
                gapLen = width * 1f;
            }
            else
            {
                dashLen = width * 3f;
                gapLen = width * 2f;
            }
            dashLen = Math.Max(dashLen, 1f);
            gapLen = Math.Max(gapLen, 1f);

            // Adjust gap to distribute dashes evenly (Chrome's SelectBestDashGap)
            float dx = midX2 - midX1, dy = midY2 - midY1;
            float strokeLength = (float)Math.Sqrt(dx * dx + dy * dy);
            if (strokeLength > 0)
                gapLen = Math.Max(SelectBestDashGap(strokeLength, dashLen, gapLen), 1f);

            float[] dashPattern = new[] { dashLen, gapLen };
            var pen = new PenInfo(color, width, dashPattern);

            var path = new PathData();
            path.MoveTo(midX1, midY1);
            path.LineTo(midX2, midY2);
            target.StrokePath(path, pen);
        }

        private static void StrokeDotted(
            IRenderTarget target, CssColor color, float width,
            float outerX1, float outerY1, float outerX2, float outerY2,
            float innerX1, float innerY1, float innerX2, float innerY2)
        {
            float midX1 = (outerX1 + innerX1) * 0.5f;
            float midY1 = (outerY1 + innerY1) * 0.5f;
            float midX2 = (outerX2 + innerX2) * 0.5f;
            float midY2 = (outerY2 + innerY2) * 0.5f;

            // Chrome: dash=width, gap adjusted via SelectBestDashGap
            float dotLen = Math.Max(width, 1f);
            float gapLen = dotLen;

            float dx = midX2 - midX1, dy = midY2 - midY1;
            float strokeLength = (float)Math.Sqrt(dx * dx + dy * dy);
            if (strokeLength > 0)
                gapLen = Math.Max(SelectBestDashGap(strokeLength, dotLen, gapLen), 1f);

            float[] dashPattern = new[] { dotLen, gapLen };
            var pen = new PenInfo(color, width, dashPattern);

            var path = new PathData();
            path.MoveTo(midX1, midY1);
            path.LineTo(midX2, midY2);
            target.StrokePath(path, pen);
        }

        private static void PaintDouble(
            IRenderTarget target, CssColor color, float width,
            float outerX1, float outerY1, float outerX2, float outerY2,
            float innerX2, float innerY2, float innerX1, float innerY1)
        {
            if (width < 3f)
            {
                // If too thin for double, just draw solid.
                FillTrapezoid(target, color, outerX1, outerY1, outerX2, outerY2, innerX2, innerY2, innerX1, innerY1);
                return;
            }

            // Double border: draw the outer third and inner third, leaving the middle third empty.
            float third = width / 3f;

            // Compute direction: the inward normal, normalized by the width.
            float dx = innerX1 - outerX1;
            float dy = innerY1 - outerY1;
            float dx2 = innerX2 - outerX2;
            float dy2 = innerY2 - outerY2;

            float ratioOuter = third / width;
            float ratioInner = (width - third) / width;

            // Outer stripe
            float oMidX1 = outerX1 + dx * ratioOuter;
            float oMidY1 = outerY1 + dy * ratioOuter;
            float oMidX2 = outerX2 + dx2 * ratioOuter;
            float oMidY2 = outerY2 + dy2 * ratioOuter;
            FillTrapezoid(target, color, outerX1, outerY1, outerX2, outerY2, oMidX2, oMidY2, oMidX1, oMidY1);

            // Inner stripe
            float iMidX1 = outerX1 + dx * ratioInner;
            float iMidY1 = outerY1 + dy * ratioInner;
            float iMidX2 = outerX2 + dx2 * ratioInner;
            float iMidY2 = outerY2 + dy2 * ratioInner;
            FillTrapezoid(target, color, iMidX1, iMidY1, iMidX2, iMidY2, innerX2, innerY2, innerX1, innerY1);
        }

        private static void PaintGrooveRidge(
            IRenderTarget target, CssColor color, float width, bool isGroove,
            float outerX1, float outerY1, float outerX2, float outerY2,
            float innerX2, float innerY2, float innerX1, float innerY1)
        {
            // Groove: outer half darker, inner half lighter.
            // Ridge: outer half lighter, inner half darker.
            CssColor dark = DarkenColor(color);
            CssColor light = LightenColor(color);

            float half = width / 2f;
            float dx = innerX1 - outerX1;
            float dy = innerY1 - outerY1;
            float dx2 = innerX2 - outerX2;
            float dy2 = innerY2 - outerY2;
            float ratio = half / width;

            float midX1 = outerX1 + dx * ratio;
            float midY1 = outerY1 + dy * ratio;
            float midX2 = outerX2 + dx2 * ratio;
            float midY2 = outerY2 + dy2 * ratio;

            CssColor outerColor = isGroove ? dark : light;
            CssColor innerColor = isGroove ? light : dark;

            FillTrapezoid(target, outerColor, outerX1, outerY1, outerX2, outerY2, midX2, midY2, midX1, midY1);
            FillTrapezoid(target, innerColor, midX1, midY1, midX2, midY2, innerX2, innerY2, innerX1, innerY1);
        }

        private static void PaintInsetOutset(
            IRenderTarget target, CssColor color, CssBorderStyle style,
            float outerX1, float outerY1, float outerX2, float outerY2,
            float innerX2, float innerY2, float innerX1, float innerY1)
        {
            // Determine if this side should be dark or light.
            // For inset: top/left are dark, bottom/right are light.
            // For outset: top/left are light, bottom/right are dark.
            // We detect the side by examining the direction of the outer edge.
            float edgeDx = outerX2 - outerX1;
            float edgeDy = outerY2 - outerY1;

            bool isTopOrLeft;
            if (Math.Abs(edgeDx) > Math.Abs(edgeDy))
                isTopOrLeft = edgeDy == 0 && outerY1 <= (outerY1 + innerY1) * 0.5f; // horizontal → top if y is smaller
            else
                isTopOrLeft = edgeDx == 0 && outerX1 <= (outerX1 + innerX1) * 0.5f; // vertical → left if x is smaller

            bool useDark = style == CssBorderStyle.Inset ? isTopOrLeft : !isTopOrLeft;
            CssColor sideColor = useDark ? DarkenColor(color) : LightenColor(color);

            FillTrapezoid(target, sideColor, outerX1, outerY1, outerX2, outerY2, innerX2, innerY2, innerX1, innerY1);
        }

        // Chrome's Color::Dark() — darken based on max component brightness
        // Source: third_party/blink/renderer/platform/graphics/color.cc
        internal static CssColor DarkenColor(CssColor c)
        {
            float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f;
            float v = Math.Max(r, Math.Max(g, b));
            if (v == 0f)
                return new CssColor(64, 64, 64, c.A); // 0.25 * 256 ≈ 64
            float multiplier = Math.Max(0f, (v - 0.33f) / v);
            const float scale = 255.998f; // nextafterf(256.0f, 0.0f)
            return new CssColor(
                (byte)(r * multiplier * scale),
                (byte)(g * multiplier * scale),
                (byte)(b * multiplier * scale),
                c.A);
        }

        // Chrome's Color::Light() — lighten based on max component brightness
        internal static CssColor LightenColor(CssColor c)
        {
            float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f;
            float v = Math.Max(r, Math.Max(g, b));
            if (v == 0f)
                return new CssColor(64, 64, 64, c.A);
            float multiplier = Math.Min(1f, (v + 0.33f) / v);
            const float scale = 255.998f;
            return new CssColor(
                (byte)(r * multiplier * scale),
                (byte)(g * multiplier * scale),
                (byte)(b * multiplier * scale),
                c.A);
        }

        private static void PaintWithRadius(
            LayoutBox box, IRenderTarget target, ComputedStyle style,
            float topW, float rightW, float bottomW, float leftW,
            CssBorderStyle topStyle, CssBorderStyle rightStyle,
            CssBorderStyle bottomStyle, CssBorderStyle leftStyle,
            BorderRadii radii)
        {
            RectF borderRect = box.BorderRect;

            // For rounded borders, create the outer rounded rect path,
            // clip to it, and then fill each side within the clip.
            var outerPath = new PathData();
            radii.AddToPath(outerPath, borderRect);

            target.Save();
            target.PushClipPath(outerPath);

            // Paint each side as a rectangle covering that side, clipped by the outer rounded rect.
            // Top
            if (topW > 0f && topStyle != CssBorderStyle.None && topStyle != CssBorderStyle.Hidden)
            {
                CssColor color = style.BorderTopColor;
                if (color.A > 0)
                {
                    var legendGap = GetFieldsetLegendGap(box);
                    if (legendGap.HasValue)
                    {
                        float gapLeft = legendGap.Value.Left;
                        float gapRight = legendGap.Value.Right;
                        // Left segment
                        if (gapLeft > borderRect.Left)
                        {
                            var leftRect = new RectF(borderRect.Left, borderRect.Top,
                                                     gapLeft - borderRect.Left, topW);
                            target.FillRect(leftRect, BrushInfo.Solid(color));
                        }
                        // Right segment
                        if (gapRight < borderRect.Right)
                        {
                            var rightRect = new RectF(gapRight, borderRect.Top,
                                                      borderRect.Right - gapRight, topW);
                            target.FillRect(rightRect, BrushInfo.Solid(color));
                        }
                    }
                    else
                    {
                        var rect = new RectF(borderRect.Left, borderRect.Top, borderRect.Width, topW);
                        target.FillRect(rect, BrushInfo.Solid(color));
                    }
                }
            }

            // Right
            if (rightW > 0f && rightStyle != CssBorderStyle.None && rightStyle != CssBorderStyle.Hidden)
            {
                CssColor color = style.BorderRightColor;
                if (color.A > 0)
                {
                    var rect = new RectF(borderRect.Right - rightW, borderRect.Top, rightW, borderRect.Height);
                    target.FillRect(rect, BrushInfo.Solid(color));
                }
            }

            // Bottom
            if (bottomW > 0f && bottomStyle != CssBorderStyle.None && bottomStyle != CssBorderStyle.Hidden)
            {
                CssColor color = style.BorderBottomColor;
                if (color.A > 0)
                {
                    var rect = new RectF(borderRect.Left, borderRect.Bottom - bottomW, borderRect.Width, bottomW);
                    target.FillRect(rect, BrushInfo.Solid(color));
                }
            }

            // Left
            if (leftW > 0f && leftStyle != CssBorderStyle.None && leftStyle != CssBorderStyle.Hidden)
            {
                CssColor color = style.BorderLeftColor;
                if (color.A > 0)
                {
                    var rect = new RectF(borderRect.Left, borderRect.Top, leftW, borderRect.Height);
                    target.FillRect(rect, BrushInfo.Solid(color));
                }
            }

            target.PopClip();
            target.Restore();
        }
    }
}
