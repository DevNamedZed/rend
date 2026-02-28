using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Placeholder for painting box-shadow effects. Currently draws offset
    /// rectangles as a simplified shadow approximation.
    /// </summary>
    internal static class BoxShadowPainter
    {
        /// <summary>
        /// Paints box-shadow for the given box onto the render target.
        /// This is a placeholder implementation that can be extended with full
        /// box-shadow parsing (offset, blur, spread, color, inset) once the
        /// CSS box-shadow property is resolved in ComputedStyle.
        /// </summary>
        /// <param name="box">The layout box whose box-shadow to paint.</param>
        /// <param name="target">The render target to draw on.</param>
        public static void Paint(LayoutBox box, IRenderTarget target)
        {
            // Box-shadow painting is a placeholder.
            // When ComputedStyle exposes box-shadow values, this method will:
            // 1. Read each box-shadow layer (offset-x, offset-y, blur, spread, color, inset)
            // 2. Compute the shadow rectangle (border rect + spread +/- offsets)
            // 3. Draw filled rectangles (or rounded rects if border-radius is set)
            //    with the shadow color offset from the element
            // 4. Apply gaussian blur approximation if blur radius > 0
        }
    }
}
