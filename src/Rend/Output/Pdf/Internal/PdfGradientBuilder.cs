using System;
using Rend.Pdf;
using Rend.Rendering;

namespace Rend.Output.Pdf.Internal
{
    /// <summary>
    /// Converts CSS gradient definitions to PDF shading patterns.
    /// </summary>
    internal static class PdfGradientBuilder
    {
        /// <summary>
        /// Determines whether the given gradient info can be rendered as a PDF shading pattern.
        /// </summary>
        internal static bool IsSupported(GradientInfo gradient)
        {
            if (gradient == null || gradient.Stops == null || gradient.Stops.Length < 2)
                return false;

            return gradient.Type == GradientType.Linear || gradient.Type == GradientType.Radial
                || gradient.Type == GradientType.Conic;
        }

        /// <summary>
        /// Applies a gradient shading to the current PDF content stream.
        /// </summary>
        internal static void Apply(GradientInfo gradient, PdfContentStream content,
                                    float x, float y, float width, float height)
        {
            if (gradient == null || content == null) return;

            if (gradient.Type == GradientType.Linear)
            {
                var linear = new PdfLinearGradient();

                // Convert angle to coordinates within the bounding rect.
                // CSS angle: 0deg = bottom to top, 90deg = left to right, 180deg = top to bottom
                float angleRad = gradient.Angle * (float)(Math.PI / 180.0);
                float cos = (float)Math.Cos(angleRad);
                float sin = (float)Math.Sin(angleRad);

                float cx = x + width / 2;
                float cy = y + height / 2;
                float halfDiag = (Math.Abs(sin) * width + Math.Abs(cos) * height) / 2;

                linear.X0 = cx - sin * halfDiag;
                linear.Y0 = cy + cos * halfDiag;
                linear.X1 = cx + sin * halfDiag;
                linear.Y1 = cy - cos * halfDiag;

                linear.Stops = ConvertStops(gradient.Stops);
                content.ApplyLinearGradient(linear);
            }
            else if (gradient.Type == GradientType.Radial)
            {
                var radial = new PdfRadialGradient();

                float cx = gradient.Center.X;
                float cy = gradient.Center.Y;
                radial.X0 = cx;
                radial.Y0 = cy;
                radial.R0 = 0;
                radial.X1 = cx;
                radial.Y1 = cy;
                radial.R1 = Math.Max(gradient.RadiusX, gradient.RadiusY);

                radial.Stops = ConvertStops(gradient.Stops);
                content.ApplyRadialGradient(radial);
            }
            else if (gradient.Type == GradientType.Conic)
            {
                var conic = new PdfConicGradient();

                // Center is stored as fraction (0..1) for conic gradients
                conic.CenterX = x + gradient.Center.X * width;
                conic.CenterY = y + gradient.Center.Y * height;
                conic.StartAngle = gradient.Angle;
                conic.Width = width;
                conic.Height = height;
                conic.Stops = ConvertStops(gradient.Stops);

                content.ApplyConicGradient(conic);
            }
        }

        private static PdfGradientColorStop[] ConvertStops(GradientStop[] stops)
        {
            var result = new PdfGradientColorStop[stops.Length];
            for (int i = 0; i < stops.Length; i++)
            {
                result[i] = new PdfGradientColorStop(stops[i].Position, stops[i].Color);
            }
            return result;
        }
    }
}
