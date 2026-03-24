using Rend.Core.Values;
using Rend.Css;
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

            // [CSS2 §11.1.1] The root element's overflow propagates to the viewport,
            // not to the element itself. Skip clipping for the root html element.
            if (box.StyledNode is Style.StyledElement rootElem
                && rootElem.TagName == "html"
                && box.Parent == null)
            {
                return false;
            }

            // contain: paint, content, or strict also establishes clipping
            CssContain contain = style.Contain;
            bool needsClip = NeedsClipping(overflowX) || NeedsClipping(overflowY)
                || contain == CssContain.Paint || contain == CssContain.Content || contain == CssContain.Strict;
            if (!needsClip)
            {
                return false;
            }




            // Use rounded clip path when border-radius is set.
            // [CSS-OVERFLOW §5.1] When one axis is clip and the other is visible,
            // the clipping region is NOT rounded (no border-radius applied).
            var radii = BorderRadiusResolver.Resolve(style, box.BorderRect);
            bool mixedClipVisible = (overflowX == CssOverflow.Clip && overflowY == CssOverflow.Visible)
                || (overflowX == CssOverflow.Visible && overflowY == CssOverflow.Clip);

            if (radii.HasRadius && !mixedClipVisible)
            {
                var path = new PathData();
                radii.AddToPath(path, box.PaddingRect);
                target.PushClipPath(path);
            }
            else
            {
                target.PushClipRect(box.PaddingRect);
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

        private static bool NeedsClipping(CssOverflow overflow)
        {
            return overflow == CssOverflow.Hidden
                || overflow == CssOverflow.Scroll
                || overflow == CssOverflow.Auto
                || overflow == CssOverflow.Clip;
        }
    }
}
