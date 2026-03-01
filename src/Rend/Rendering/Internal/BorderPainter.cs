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
            float tlr = style.BorderTopLeftRadius;
            float trr = style.BorderTopRightRadius;
            float brr = style.BorderBottomRightRadius;
            float blr = style.BorderBottomLeftRadius;
            bool hasRadius = tlr > 0f || trr > 0f || brr > 0f || blr > 0f;

            if (hasRadius)
            {
                PaintWithRadius(box, target, style, topW, rightW, bottomW, leftW,
                                topStyle, rightStyle, bottomStyle, leftStyle,
                                tlr, trr, brr, blr);
                return;
            }

            RectF borderRect = box.BorderRect;

            float outerLeft = borderRect.Left;
            float outerTop = borderRect.Top;
            float outerRight = borderRect.Right;
            float outerBottom = borderRect.Bottom;

            float innerLeft = outerLeft + leftW;
            float innerTop = outerTop + topW;
            float innerRight = outerRight - rightW;
            float innerBottom = outerBottom - bottomW;

            // Top border
            if (topW > 0f && topStyle != CssBorderStyle.None && topStyle != CssBorderStyle.Hidden)
            {
                CssColor color = style.BorderTopColor;

                // Fieldset + legend: split the top border around the legend gap
                var legendGap = GetFieldsetLegendGap(box);
                if (legendGap.HasValue)
                {
                    float gapLeft = legendGap.Value.Left;
                    float gapRight = legendGap.Value.Right;

                    // Left segment: outerLeft to gapLeft
                    if (gapLeft > outerLeft)
                    {
                        float segInnerLeft = innerLeft;
                        float segInnerRight = gapLeft;
                        PaintSide(target, color, topStyle, topW,
                                  outerLeft, outerTop, gapLeft, outerTop,
                                  segInnerRight, innerTop, segInnerLeft, innerTop);
                    }

                    // Right segment: gapRight to outerRight
                    if (gapRight < outerRight)
                    {
                        float segInnerLeft = gapRight;
                        float segInnerRight = innerRight;
                        PaintSide(target, color, topStyle, topW,
                                  gapRight, outerTop, outerRight, outerTop,
                                  segInnerRight, innerTop, segInnerLeft, innerTop);
                    }
                }
                else
                {
                    PaintSide(target, color, topStyle, topW,
                              outerLeft, outerTop, outerRight, outerTop,
                              innerRight, innerTop, innerLeft, innerTop);
                }
            }

            // Right border
            if (rightW > 0f && rightStyle != CssBorderStyle.None && rightStyle != CssBorderStyle.Hidden)
            {
                CssColor color = style.BorderRightColor;
                PaintSide(target, color, rightStyle, rightW,
                          outerRight, outerTop, outerRight, outerBottom,
                          innerRight, innerBottom, innerRight, innerTop);
            }

            // Bottom border
            if (bottomW > 0f && bottomStyle != CssBorderStyle.None && bottomStyle != CssBorderStyle.Hidden)
            {
                CssColor color = style.BorderBottomColor;
                PaintSide(target, color, bottomStyle, bottomW,
                          outerRight, outerBottom, outerLeft, outerBottom,
                          innerLeft, innerBottom, innerRight, innerBottom);
            }

            // Left border
            if (leftW > 0f && leftStyle != CssBorderStyle.None && leftStyle != CssBorderStyle.Hidden)
            {
                CssColor color = style.BorderLeftColor;
                PaintSide(target, color, leftStyle, leftW,
                          outerLeft, outerBottom, outerLeft, outerTop,
                          innerLeft, innerTop, innerLeft, innerBottom);
            }
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
                    // The gap spans the legend's border box horizontally
                    RectF legendBorder = child.BorderRect;
                    float gapLeft = legendBorder.Left - child.PaddingLeft;
                    float gapRight = legendBorder.Right + child.PaddingRight;
                    return (gapLeft, gapRight);
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

        private static void FillTrapezoid(
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

        private static void StrokeDashed(
            IRenderTarget target, CssColor color, float width,
            float outerX1, float outerY1, float outerX2, float outerY2,
            float innerX1, float innerY1, float innerX2, float innerY2)
        {
            // Draw as a line through the midpoint of the border side with a dash pattern.
            float midX1 = (outerX1 + innerX1) * 0.5f;
            float midY1 = (outerY1 + innerY1) * 0.5f;
            float midX2 = (outerX2 + innerX2) * 0.5f;
            float midY2 = (outerY2 + innerY2) * 0.5f;

            float dashLen = Math.Max(width * 3f, 1f);
            float[] dashPattern = new[] { dashLen, dashLen };
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
            // Draw as a line through the midpoint with dot pattern (width, width).
            float midX1 = (outerX1 + innerX1) * 0.5f;
            float midY1 = (outerY1 + innerY1) * 0.5f;
            float midX2 = (outerX2 + innerX2) * 0.5f;
            float midY2 = (outerY2 + innerY2) * 0.5f;

            float dotLen = Math.Max(width, 1f);
            float[] dashPattern = new[] { dotLen, dotLen };
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

        private static CssColor DarkenColor(CssColor c)
        {
            return new CssColor(
                (byte)(c.R * 2 / 3),
                (byte)(c.G * 2 / 3),
                (byte)(c.B * 2 / 3),
                c.A);
        }

        private static CssColor LightenColor(CssColor c)
        {
            return new CssColor(
                (byte)Math.Min(255, c.R + (255 - c.R) / 3),
                (byte)Math.Min(255, c.G + (255 - c.G) / 3),
                (byte)Math.Min(255, c.B + (255 - c.B) / 3),
                c.A);
        }

        private static void PaintWithRadius(
            LayoutBox box, IRenderTarget target, ComputedStyle style,
            float topW, float rightW, float bottomW, float leftW,
            CssBorderStyle topStyle, CssBorderStyle rightStyle,
            CssBorderStyle bottomStyle, CssBorderStyle leftStyle,
            float tlr, float trr, float brr, float blr)
        {
            RectF borderRect = box.BorderRect;

            // For rounded borders, create the outer rounded rect path,
            // clip to it, and then fill each side within the clip.
            var outerPath = new PathData();
            outerPath.AddRoundedRectangle(borderRect, tlr, trr, brr, blr);

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
