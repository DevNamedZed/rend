using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Handles CSS transform application by converting style transforms
    /// to a <see cref="Rend.Core.Values.Matrix3x2"/> and applying them to the render target.
    /// </summary>
    internal static class TransformHandler
    {
        /// <summary>
        /// Checks whether the box has a CSS transform and, if so, saves the
        /// render target state and applies the transform matrix.
        /// </summary>
        /// <param name="box">The layout box whose transform to apply.</param>
        /// <param name="target">The render target.</param>
        /// <returns><c>true</c> if a transform was applied and the state needs to be restored; otherwise <c>false</c>.</returns>
        public static bool Apply(LayoutBox box, IRenderTarget target)
        {
            // CSS transforms are not yet exposed in ComputedStyle.
            // When they are, this method will:
            // 1. Read the transform function list (translate, rotate, scale, skew, matrix)
            // 2. Compute the transform origin (default 50% 50% of border box)
            // 3. Build a Matrix3x2: translate to origin -> apply transform -> translate back
            // 4. Call target.Save() and target.SetTransform(matrix)
            //
            // For now, this is a no-op that returns false.

            // Placeholder: check for positioned elements with non-zero offset.
            // Positioned elements (relative) use translation offsets from layout,
            // but those are already reflected in the content rect. Actual CSS
            // transform property support will be added here.

            _ = box;
            _ = target;

            return false;
        }

        /// <summary>
        /// Restores the render target state if a transform was previously applied.
        /// This should only be called when <see cref="Apply"/> returned <c>true</c>.
        /// </summary>
        /// <param name="target">The render target.</param>
        public static void Restore(IRenderTarget target)
        {
            target.Restore();
        }
    }
}
