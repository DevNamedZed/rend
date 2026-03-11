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

                linear.Stops = gradient.Repeating
                    ? ExpandRepeatingStops(gradient.Stops)
                    : ConvertStops(gradient.Stops);
                content.ApplyLinearGradient(linear);
            }
            else if (gradient.Type == GradientType.Radial)
            {
                var radial = new PdfRadialGradient();

                // Center and radii are stored as fractions (0-1); convert to absolute page coordinates
                float cx = x + gradient.Center.X * width;
                float cy = y + gradient.Center.Y * height;
                float rx = gradient.RadiusX * width;
                float ry = gradient.RadiusY * height;

                radial.X0 = cx;
                radial.Y0 = cy;
                radial.R0 = 0;
                radial.X1 = cx;
                radial.Y1 = cy;
                radial.R1 = Math.Max(rx, ry);

                radial.Stops = gradient.Repeating
                    ? ExpandRepeatingStops(gradient.Stops)
                    : ConvertStops(gradient.Stops);
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
                conic.Stops = gradient.Repeating
                    ? ExpandRepeatingStops(gradient.Stops)
                    : ConvertStops(gradient.Stops);

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

        /// <summary>
        /// Expands repeating gradient stops to fill the full 0-1 range.
        /// </summary>
        private static PdfGradientColorStop[] ExpandRepeatingStops(GradientStop[] stops)
        {
            if (stops.Length < 2) return ConvertStops(stops);

            float firstPos = stops[0].Position;
            float lastPos = stops[stops.Length - 1].Position;
            float range = lastPos - firstPos;

            if (range < 0.0001f) return ConvertStops(stops);

            var expanded = new System.Collections.Generic.List<PdfGradientColorStop>();

            // Calculate how many repetitions before and after to cover [0, 1]
            int repsBefore = (int)Math.Ceiling(firstPos / range);
            int repsAfter = (int)Math.Ceiling((1f - lastPos) / range);

            for (int rep = -repsBefore; rep <= repsAfter; rep++)
            {
                float offset = rep * range;
                for (int i = 0; i < stops.Length; i++)
                {
                    float pos = stops[i].Position + offset;
                    if (pos < -0.001f || pos > 1.001f) continue;
                    pos = Math.Max(0f, Math.Min(1f, pos));
                    expanded.Add(new PdfGradientColorStop(pos, stops[i].Color));
                }
            }

            // Sort and deduplicate very close positions
            expanded.Sort((a, b) => a.Position.CompareTo(b.Position));

            // Ensure we have stops at exactly 0 and 1
            if (expanded.Count > 0 && expanded[0].Position > 0.001f)
            {
                var first = expanded[0];
                expanded.Insert(0, new PdfGradientColorStop(0f, first.R, first.G, first.B));
            }
            if (expanded.Count > 0 && expanded[expanded.Count - 1].Position < 0.999f)
            {
                var last = expanded[expanded.Count - 1];
                expanded.Add(new PdfGradientColorStop(1f, last.R, last.G, last.B));
            }

            return expanded.ToArray();
        }
    }
}
