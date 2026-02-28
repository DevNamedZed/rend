using Rend.Core.Values;

namespace Rend.Output.Pdf.Internal
{
    /// <summary>
    /// Provides coordinate conversion between CSS (top-left origin, Y-down)
    /// and PDF (bottom-left origin, Y-up) coordinate systems.
    /// </summary>
    internal static class PdfCoordinateMapper
    {
        /// <summary>
        /// Flips a CSS Y coordinate to PDF Y coordinate for the given page height.
        /// </summary>
        /// <param name="y">The CSS Y coordinate (top-down).</param>
        /// <param name="pageHeight">The page height in points.</param>
        /// <returns>The PDF Y coordinate (bottom-up).</returns>
        internal static float FlipY(float y, float pageHeight)
        {
            return pageHeight - y;
        }

        /// <summary>
        /// Returns a Matrix3x2 that transforms from CSS coordinates (top-left origin)
        /// to PDF coordinates (bottom-left origin) for the given page height.
        /// This is equivalent to: scale(1, -1) then translate(0, pageHeight).
        /// In PDF matrix form: [1, 0, 0, -1, 0, pageHeight].
        /// </summary>
        /// <param name="pageHeight">The page height in points.</param>
        /// <returns>A transformation matrix for the coordinate flip.</returns>
        internal static Matrix3x2 GetPageFlipTransform(float pageHeight)
        {
            return new Matrix3x2(1f, 0f, 0f, -1f, 0f, pageHeight);
        }
    }
}
