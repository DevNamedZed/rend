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
        /// pushes a clip rectangle matching the padding rect.
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

            CssOverflow overflowX = box.StyledNode.Style.OverflowX;
            CssOverflow overflowY = box.StyledNode.Style.OverflowY;

            bool needsClip = NeedsClipping(overflowX) || NeedsClipping(overflowY);
            if (!needsClip)
            {
                return false;
            }

            target.PushClipRect(box.PaddingRect);
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
