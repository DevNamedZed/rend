using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Handles CSS opacity by saving state and setting the target opacity
    /// when a box has opacity less than 1.
    /// </summary>
    internal static class OpacityCompositor
    {
        /// <summary>
        /// If the box's computed opacity is less than 1, saves the render target
        /// state and sets the opacity.
        /// </summary>
        /// <param name="box">The layout box whose opacity to apply.</param>
        /// <param name="target">The render target.</param>
        /// <returns><c>true</c> if opacity was applied and <see cref="Restore"/> must be called; otherwise <c>false</c>.</returns>
        public static bool Apply(LayoutBox box, IRenderTarget target)
        {
            float opacity = GetOpacity(box);
            if (opacity >= 1f)
            {
                return false;
            }

            target.Save();
            target.SetOpacity(opacity);
            return true;
        }

        /// <summary>
        /// Restores the render target state after opacity was applied.
        /// This should only be called when <see cref="Apply"/> returned <c>true</c>.
        /// </summary>
        /// <param name="target">The render target.</param>
        public static void Restore(IRenderTarget target)
        {
            target.Restore();
        }

        private static float GetOpacity(LayoutBox box)
        {
            if (box.StyledNode?.Style == null)
            {
                return 1f;
            }

            float opacity = box.StyledNode.Style.Opacity;

            // Clamp to valid range.
            if (opacity < 0f)
            {
                return 0f;
            }

            if (opacity > 1f)
            {
                return 1f;
            }

            return opacity;
        }
    }
}
