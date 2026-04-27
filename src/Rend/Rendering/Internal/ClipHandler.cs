using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Values;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Handles CSS overflow clipping by pushing clip rectangles onto the render
    /// target when overflow is hidden, scroll, auto, or clip.
    /// </summary>
    internal static class ClipHandler
    {
        /// <summary>
        /// If the box has overflow hidden, scroll, auto, or clip on either axis,
        /// pushes a clip rectangle (or rounded rect path) matching the padding rect.
        /// </summary>
        /// <param name="box">The layout box whose overflow to handle.</param>
        /// <param name="target">The render target.</param>
        /// <returns><c>true</c> if a clip was pushed and <see cref="Restore"/> must be called; otherwise <c>false</c>.</returns>
        public static bool Apply(LayoutBox box, IRenderTarget target)
        {
            if (box.StyledNode?.Style == null)
            {
                return false;
            }

            ComputedStyle style = box.StyledNode.Style;
            CssOverflow overflowX = style.OverflowX;
            CssOverflow overflowY = style.OverflowY;

            // contain: paint, content, or strict also establishes clipping
            CssContain contain = style.Contain;
            bool styleEstablishesContainment = contain == CssContain.Paint
                || contain == CssContain.Layout
                || contain == CssContain.Content
                || contain == CssContain.Strict;

            // [CSS2 §11.1.1] The root element's overflow propagates to the viewport,
            // not to the element itself. Skip clipping for the root html element
            // unless it has its own containment (contain: paint/etc.) which forces clipping.
            if (box.StyledNode is Style.StyledElement rootElem
                && rootElem.TagName == "html"
                && box.Parent == null
                && !styleEstablishesContainment)
            {
                return false;
            }

            // [CSS-OVERFLOW-3 §3.5] When the root element (html) has overflow:visible
            // (initial value), the body element's overflow propagates to the viewport.
            // The body itself behaves as if it has overflow:visible, and the canvas/viewport
            // applies the body's original overflow value. Skip clipping for body in this case.
            //
            // Propagation is suppressed when body is its own scroll container (e.g. has
            // contain: paint/layout/strict/content, transform, position: fixed/sticky, etc.).
            if (box.StyledNode is Style.StyledElement bodyElem
                && bodyElem.TagName == "body"
                && box.Parent != null
                && box.Parent.StyledNode is Style.StyledElement parentHtml
                && parentHtml.TagName == "html")
            {
                CssOverflow rootOverflowX = parentHtml.Style.OverflowX;
                CssOverflow rootOverflowY = parentHtml.Style.OverflowY;
                if (rootOverflowX == CssOverflow.Visible
                    && rootOverflowY == CssOverflow.Visible
                    && !styleEstablishesContainment)
                {
                    return false;
                }
            }

            bool needsClip = NeedsClipping(overflowX) || NeedsClipping(overflowY)
                || contain == CssContain.Paint || contain == CssContain.Content || contain == CssContain.Strict;
            if (!needsClip)
            {
                return false;
            }

            // [CSS-OVERFLOW-3 §3.4] overflow-clip-margin applies only when the clip
            // is established by overflow:clip or by paint containment (with overflow:visible).
            // It does NOT apply when overflow is hidden/scroll/auto — those establish
            // their own non-expandable clip regardless of containment.
            bool hasNonClipOverflow = overflowX == CssOverflow.Hidden || overflowX == CssOverflow.Scroll
                || overflowX == CssOverflow.Auto || overflowY == CssOverflow.Hidden
                || overflowY == CssOverflow.Scroll || overflowY == CssOverflow.Auto;
            bool clipMarginApplies = !hasNonClipOverflow
                && (overflowX == CssOverflow.Clip || overflowY == CssOverflow.Clip
                    || contain == CssContain.Paint || contain == CssContain.Content
                    || contain == CssContain.Strict);
            OverflowClipMarginInfo? clipMarginInfo = clipMarginApplies ? style.OverflowClipMargin : null;
            float clipMargin = clipMarginInfo?.Margin ?? 0f;

            // Determine the base clip rect from the reference box.
            RectF baseClipRect = GetReferenceBox(box, clipMarginInfo);

            // Use rounded clip path when border-radius is set.
            // [CSS-OVERFLOW §5.1] When one axis is clip and the other is visible,
            // the clipping region is NOT rounded (no border-radius applied).
            var radii = BorderRadiusResolver.Resolve(style, box.BorderRect);
            bool mixedClipVisible = (overflowX == CssOverflow.Clip && overflowY == CssOverflow.Visible)
                || (overflowX == CssOverflow.Visible && overflowY == CssOverflow.Clip);

            // [CSS-OVERFLOW-3 §3.4] Expand the clip rect outward by the clip margin.
            RectF clipRect = ExpandRect(baseClipRect, clipMargin);

            // [CSS-OVERFLOW-3 §3] When one axis is clip and the other is visible,
            // extend the clip rect to allow overflow on the visible axis.
            if (mixedClipVisible)
            {
                const float largeExtent = 100000f;
                if (overflowX == CssOverflow.Visible)
                {
                    clipRect = new RectF(
                        -largeExtent, clipRect.Y,
                        largeExtent * 2, clipRect.Height);
                }
                else
                {
                    clipRect = new RectF(
                        clipRect.X, -largeExtent,
                        clipRect.Width, largeExtent * 2);
                }
            }

            if (radii.HasRadius && !mixedClipVisible)
            {
                // [CSS-OVERFLOW-3 §3.4] When overflow-clip-margin expands the clip,
                // border-radius corners are also expanded outward by the margin.
                BorderRadii expandedRadii = ExpandRadii(radii, clipMargin);
                var path = new PathData();
                expandedRadii.AddToPath(path, clipRect);
                target.PushClipPath(path);
            }
            else
            {
                target.PushClipRect(clipRect);
            }

            return true;
        }

        /// <summary>
        /// Pops the clip rectangle that was previously pushed by <see cref="Apply"/>.
        /// This should only be called when <see cref="Apply"/> returned <c>true</c>.
        /// </summary>
        /// <param name="target">The render target.</param>
        public static void Restore(IRenderTarget target)
        {
            target.PopClip();
        }

        /// <summary>
        /// [CSS-OVERFLOW-3 §3.4] Returns the reference box rect for the clip region.
        /// </summary>
        private static RectF GetReferenceBox(LayoutBox box, OverflowClipMarginInfo? clipMarginInfo)
        {
            if (clipMarginInfo == null)
            {
                return box.PaddingRect;
            }

            switch (clipMarginInfo.ReferenceBox)
            {
                case CssVisualBox.ContentBox:
                    return box.ContentRect;
                case CssVisualBox.BorderBox:
                    return box.BorderRect;
                case CssVisualBox.PaddingBox:
                default:
                    return box.PaddingRect;
            }
        }

        /// <summary>Expands a rectangle outward by the given amount on all sides.</summary>
        private static RectF ExpandRect(RectF rect, float amount)
        {
            if (amount == 0f)
            {
                return rect;
            }
            return new RectF(
                rect.X - amount,
                rect.Y - amount,
                rect.Width + amount * 2f,
                rect.Height + amount * 2f);
        }

        /// <summary>
        /// [CSS-OVERFLOW-3 §3.4] Expands border-radius values outward by the clip margin.
        /// Each radius is increased by the margin amount (minimum 0).
        /// </summary>
        private static BorderRadii ExpandRadii(BorderRadii radii, float margin)
        {
            if (margin == 0f)
            {
                return radii;
            }
            return new BorderRadii
            {
                TlRx = Math.Max(0f, radii.TlRx + margin),
                TlRy = Math.Max(0f, radii.TlRy + margin),
                TrRx = Math.Max(0f, radii.TrRx + margin),
                TrRy = Math.Max(0f, radii.TrRy + margin),
                BrRx = Math.Max(0f, radii.BrRx + margin),
                BrRy = Math.Max(0f, radii.BrRy + margin),
                BlRx = Math.Max(0f, radii.BlRx + margin),
                BlRy = Math.Max(0f, radii.BlRy + margin)
            };
        }

        private static bool NeedsClipping(CssOverflow overflow)
        {
            return overflow == CssOverflow.Hidden
                || overflow == CssOverflow.Scroll
                || overflow == CssOverflow.Auto
                || overflow == CssOverflow.Clip;
        }
    }
}
