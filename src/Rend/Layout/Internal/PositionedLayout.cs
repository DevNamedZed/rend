using System;
using Rend.Core.Values;
using Rend.Css;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Handles positioned elements: relative, absolute, fixed, and sticky positioning.
    /// CSS 2.1 §9.3
    /// </summary>
    internal static class PositionedLayout
    {
        /// <summary>
        /// Apply positioning offsets to a layout box after normal flow layout.
        /// </summary>
        public static void ApplyPositioning(LayoutBox box, LayoutBox containingBlock, LayoutBox? rootBox = null)
        {
            var style = box.StyledNode?.Style;
            if (style == null) return;

            switch (style.Position)
            {
                case CssPosition.Relative:
                    ApplyRelative(box, style);
                    break;
                case CssPosition.Absolute:
                    ApplyAbsolute(box, style, containingBlock);
                    break;
                case CssPosition.Fixed:
                    ApplyFixed(box, style, rootBox ?? containingBlock);
                    break;
                case CssPosition.Sticky:
                    // Sticky acts as relative for static rendering
                    ApplyRelative(box, style);
                    break;
            }
        }

        private static void ApplyRelative(LayoutBox box, ComputedStyle style)
        {
            float dx = 0, dy = 0;

            float top = style.Top;
            float left = style.Left;
            float bottom = style.Bottom;
            float right = style.Right;

            if (!float.IsNaN(top)) dy = top;
            else if (!float.IsNaN(bottom)) dy = -bottom;

            if (!float.IsNaN(left)) dx = left;
            else if (!float.IsNaN(right)) dx = -right;

            if (dx != 0 || dy != 0)
            {
                box.ContentRect = new RectF(
                    box.ContentRect.X + dx,
                    box.ContentRect.Y + dy,
                    box.ContentRect.Width,
                    box.ContentRect.Height);
            }
        }

        private static void ApplyAbsolute(LayoutBox box, ComputedStyle style, LayoutBox containingBlock)
        {
            var cb = containingBlock.PaddingRect;

            float top = style.Top;
            float left = style.Left;
            float bottom = style.Bottom;
            float right = style.Right;

            float x = box.ContentRect.X;
            float y = box.ContentRect.Y;
            float w = box.ContentRect.Width;
            float h = box.ContentRect.Height;

            // Horizontal: CSS 2.1 §10.3.7
            bool hasWidth = !float.IsNaN(style.Width);
            if (!float.IsNaN(left) && !float.IsNaN(right))
            {
                if (!hasWidth)
                {
                    // Width is auto: compute from left+right constraints
                    x = cb.X + left + box.MarginLeft + box.BorderLeftWidth + box.PaddingLeft;
                    w = cb.Width - left - right - box.MarginLeft - box.MarginRight
                      - box.BorderLeftWidth - box.BorderRightWidth - box.PaddingLeft - box.PaddingRight;
                    w = Math.Max(0, w);
                }
                else
                {
                    // Over-constrained: left+right+width all specified.
                    // Distribute available space to auto margins.
                    float available = cb.Width - left - right - w
                                    - box.BorderLeftWidth - box.BorderRightWidth
                                    - box.PaddingLeft - box.PaddingRight;
                    bool mlAuto = float.IsNaN(style.MarginLeft);
                    bool mrAuto = float.IsNaN(style.MarginRight);
                    if (mlAuto && mrAuto)
                    {
                        float each = Math.Max(0, available) * 0.5f;
                        box.MarginLeft = each;
                        box.MarginRight = each;
                    }
                    else if (mlAuto)
                    {
                        box.MarginLeft = Math.Max(0, available - box.MarginRight);
                    }
                    else if (mrAuto)
                    {
                        box.MarginRight = Math.Max(0, available - box.MarginLeft);
                    }
                    x = cb.X + left + box.MarginLeft + box.BorderLeftWidth + box.PaddingLeft;
                }
            }
            else if (!float.IsNaN(left))
            {
                x = cb.X + left + box.MarginLeft + box.BorderLeftWidth + box.PaddingLeft;
            }
            else if (!float.IsNaN(right))
            {
                x = cb.Right - right - box.MarginRight - box.BorderRightWidth - box.PaddingRight - w;
            }

            // Vertical: CSS 2.1 §10.6.4
            bool hasHeight = !float.IsNaN(style.Height);
            if (!float.IsNaN(top) && !float.IsNaN(bottom))
            {
                if (!hasHeight)
                {
                    // Height is auto: compute from top+bottom constraints
                    y = cb.Y + top + box.MarginTop + box.BorderTopWidth + box.PaddingTop;
                    h = cb.Height - top - bottom - box.MarginTop - box.MarginBottom
                      - box.BorderTopWidth - box.BorderBottomWidth - box.PaddingTop - box.PaddingBottom;
                    h = Math.Max(0, h);
                }
                else
                {
                    // Over-constrained: distribute to auto margins
                    float available = cb.Height - top - bottom - h
                                    - box.BorderTopWidth - box.BorderBottomWidth
                                    - box.PaddingTop - box.PaddingBottom;
                    bool mtAuto = float.IsNaN(style.MarginTop);
                    bool mbAuto = float.IsNaN(style.MarginBottom);
                    if (mtAuto && mbAuto)
                    {
                        float each = Math.Max(0, available) * 0.5f;
                        box.MarginTop = each;
                        box.MarginBottom = each;
                    }
                    else if (mtAuto)
                    {
                        box.MarginTop = Math.Max(0, available - box.MarginBottom);
                    }
                    else if (mbAuto)
                    {
                        box.MarginBottom = Math.Max(0, available - box.MarginTop);
                    }
                    y = cb.Y + top + box.MarginTop + box.BorderTopWidth + box.PaddingTop;
                }
            }
            else if (!float.IsNaN(top))
            {
                y = cb.Y + top + box.MarginTop + box.BorderTopWidth + box.PaddingTop;
            }
            else if (!float.IsNaN(bottom))
            {
                y = cb.Bottom - bottom - box.MarginBottom - box.BorderBottomWidth - box.PaddingBottom - h;
            }

            box.ContentRect = new RectF(x, y, w, h);
        }

        private static void ApplyFixed(LayoutBox box, ComputedStyle style, LayoutBox containingBlock)
        {
            // Fixed positioning is similar to absolute but relative to viewport
            // For PDF/image output, treat as absolute relative to page
            ApplyAbsolute(box, style, containingBlock);
        }
    }
}
