using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Css.Resolution.Internal;
using Rend.Style;

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
                    ApplyRelative(box, style, containingBlock);
                    break;
                case CssPosition.Absolute:
                    // [CSS2 §10.1] If no positioned ancestor exists, the containing block
                    // is the initial containing block (viewport). When the containing block
                    // is the root html element (position:static), use the viewport box so
                    // percentage/calc offsets and dimensions resolve against viewport size.
                    var absContaining = containingBlock;
                    if (rootBox != null
                        && containingBlock.StyledNode is StyledElement cbElem
                        && cbElem.TagName == "html"
                        && containingBlock.StyledNode.Style.Position == CssPosition.Static)
                    {
                        absContaining = rootBox;
                    }
                    ApplyAbsolute(box, style, absContaining);
                    break;
                case CssPosition.Fixed:
                    ApplyFixed(box, style, rootBox ?? containingBlock);
                    break;
                case CssPosition.Sticky:
                    // Sticky acts as relative for static rendering
                    ApplyRelative(box, style, containingBlock);
                    break;
            }
        }

        private static void ApplyRelative(LayoutBox box, ComputedStyle style, LayoutBox containingBlock)
        {
            float dx = 0, dy = 0;

            // [CSS2 §10.1] For relative/static elements, the containing block is the
            // content edge of the nearest block container ancestor, NOT the nearest
            // positioned ancestor. Use the parent's dimensions for percentage resolution.
            var cb = box.Parent ?? containingBlock;
            float cbWidth = cb.ContentRect.Width;

            // [CSS2 §10.6] Percentage top/bottom resolve against the containing block's
            // height. Anonymous block wrappers are transparent for this purpose — walk
            // up to the real block container that has a definite height.
            var heightCb = cb;
            while (heightCb.IsAnonymousBlock && heightCb.Parent != null)
            {
                heightCb = heightCb.Parent;
            }
            float cbStyleHeight = heightCb.StyledNode?.Style.Height ?? float.NaN;
            bool hasDefiniteHeight = !float.IsNaN(cbStyleHeight) || heightCb.HasDefiniteCrossSize;
            float cbHeight = hasDefiniteHeight ? heightCb.ContentRect.Height : 0;

            float top = ResolvePositionValueWithCalc(style.Top, cbHeight, style, PropertyId.Top);
            float left = ResolvePositionValueWithCalc(style.Left, cbWidth, style, PropertyId.Left);
            float bottom = ResolvePositionValueWithCalc(style.Bottom, cbHeight, style, PropertyId.Bottom);
            float right = ResolvePositionValueWithCalc(style.Right, cbWidth, style, PropertyId.Right);

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
                box.RelativeOffsetX = dx;
                box.RelativeOffsetY = dy;
            }
        }

        private static void ApplyAbsolute(LayoutBox box, ComputedStyle style, LayoutBox containingBlock)
        {
            // [CSS-GRID §9] Abspos grid items with grid placement use the grid area
            // as their containing block instead of the grid container's padding box.
            var cb = box.GridAreaContainingBlock ?? containingBlock.PaddingRect;

            float top = ResolvePositionValueWithCalc(style.Top, cb.Height, style, PropertyId.Top);
            float left = ResolvePositionValueWithCalc(style.Left, cb.Width, style, PropertyId.Left);
            float bottom = ResolvePositionValueWithCalc(style.Bottom, cb.Height, style, PropertyId.Bottom);
            float right = ResolvePositionValueWithCalc(style.Right, cb.Width, style, PropertyId.Right);


            float x = box.ContentRect.X;
            float y = box.ContentRect.Y;
            float w = box.ContentRect.Width;
            float h = box.ContentRect.Height;

            // [CSS2 §10.5] Re-resolve percentage height against the containing block's
            // now-known height. During initial layout, the CB height may have been 0/NaN.
            if (h <= 0 && DeferredPercent.IsEncoded(style.Height))
            {
                float cbHeight = cb.Height;
                if (cbHeight > 0)
                {
                    h = DeferredPercent.Resolve(style.Height, cbHeight);
                    if (style.BoxSizing == CssBoxSizing.BorderBox)
                    {
                        h -= box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth;
                        if (h < 0) { h = 0; }
                    }
                }
            }
            // [CSS2 §10.5] Re-resolve deferred calc() height (NegativeInfinity sentinel)
            // against the containing block's now-known height.
            else if (h <= 0 && float.IsNegativeInfinity(style.Height))
            {
                float cbHeight = cb.Height;
                if (cbHeight > 0)
                {
                    var refVal = style.GetRefValue(PropertyId.Height);
                    if (refVal is CssFunctionValue calcFn)
                    {
                        h = ValueResolver.EvaluateDeferredCalc(calcFn, cbHeight);
                        if (style.BoxSizing == CssBoxSizing.BorderBox)
                        {
                            h -= box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth;
                            if (h < 0) { h = 0; }
                        }
                    }
                }
            }

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

                    // [CSS-SIZING-4 §5.1] Transfer max-height/min-height → width
                    // through aspect-ratio when width is auto (computed from left+right).
                    float arTransfer = DimensionResolver.GetAspectRatio(style);
                    if (arTransfer > 0)
                    {
                        float trMaxH = style.MaxHeight;
                        if (!float.IsNaN(trMaxH) && trMaxH >= 0
                            && !DeferredPercent.IsEncoded(trMaxH))
                        {
                            float transferredMaxW = trMaxH * arTransfer;
                            if (w > transferredMaxW) { w = transferredMaxW; }
                        }
                        float trMinH = style.MinHeight;
                        if (!float.IsNaN(trMinH) && trMinH > 0
                            && !DeferredPercent.IsEncoded(trMinH))
                        {
                            float transferredMinW = trMinH * arTransfer;
                            if (w < transferredMinW) { w = transferredMinW; }
                        }
                    }
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
            // [CSS-SIZING-4 §5.1] When aspect-ratio is set and height was derived from
            // the ratio in BFC layout (h > 0), treat as if height is explicit so the
            // top+bottom offset path doesn't override the ratio-derived height.
            if (!hasHeight && h > 0)
            {
                float arRatio = DimensionResolver.GetAspectRatio(style);
                if (arRatio > 0)
                {
                    hasHeight = true;
                }
            }
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
        /// <summary>
        /// Resolves a position value (top/right/bottom/left) that may be a deferred percentage.
        /// Deferred percentages are encoded via DeferredPercent sentinel offset.
        /// </summary>
        private static float ResolvePositionValue(float value, float containingDimension)
        {
            if (float.IsNaN(value))
            {
                return float.NaN;
            }
            // Deferred percentage encoded with sentinel offset
            if (DeferredPercent.IsEncoded(value))
            {
                return DeferredPercent.Resolve(value, containingDimension);
            }
            return value;
        }

        /// <summary>
        /// Resolves a position value that may have a deferred calc() expression stored as a ref value.
        /// </summary>
        private static float ResolvePositionValueWithCalc(
            float value, float containingDimension, ComputedStyle style, int propertyId)
        {
            if (float.IsNaN(value))
            {
                return float.NaN;
            }
            if (DeferredPercent.IsEncoded(value))
            {
                return DeferredPercent.Resolve(value, containingDimension);
            }
            if (float.IsNegativeInfinity(value))
            {
                var refVal = style.GetRefValue(propertyId);
                if (refVal is CssFunctionValue calcFn)
                {
                    return ValueResolver.EvaluateDeferredCalc(calcFn, containingDimension);
                }
                return 0;
            }
            return value;
        }
    }
}
