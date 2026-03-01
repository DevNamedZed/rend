using System;
using Rend.Core.Values;
using Rend.Rendering;
using SkiaSharp;

namespace Rend.Output.Image.Internal
{
    /// <summary>
    /// Converts <see cref="GradientInfo"/> definitions into SkiaSharp <see cref="SKShader"/> instances.
    /// </summary>
    internal static class SkiaGradientBuilder
    {
        /// <summary>
        /// Creates an <see cref="SKShader"/> from the given gradient info and bounding rectangle.
        /// </summary>
        /// <param name="gradient">The gradient definition to convert.</param>
        /// <param name="bounds">The bounding rectangle for the gradient.</param>
        /// <returns>An SKShader, or null if the gradient has no stops.</returns>
        internal static SKShader? CreateShader(GradientInfo gradient, RectF bounds)
        {
            if (gradient.Stops.Length == 0)
            {
                return null;
            }

            var colors = new SKColor[gradient.Stops.Length];
            var positions = new float[gradient.Stops.Length];

            for (int i = 0; i < gradient.Stops.Length; i++)
            {
                GradientStop stop = gradient.Stops[i];
                colors[i] = new SKColor(stop.Color.R, stop.Color.G, stop.Color.B, stop.Color.A);
                positions[i] = stop.Position;
            }

            switch (gradient.Type)
            {
                case GradientType.Linear:
                    return CreateLinearShader(gradient, bounds, colors, positions);
                case GradientType.Radial:
                    return CreateRadialShader(gradient, bounds, colors, positions);
                case GradientType.Conic:
                    return CreateSweepShader(gradient, bounds, colors, positions);
                default:
                    return CreateLinearShader(gradient, bounds, colors, positions);
            }
        }

        private static SKShader CreateLinearShader(GradientInfo gradient, RectF bounds,
            SKColor[] colors, float[] positions)
        {
            // CSS gradient angles: 0deg = "to top" (upward), clockwise rotation.
            // In screen coordinates (Y-down): direction = (sin(angle), -cos(angle)).
            float angleRad = gradient.Angle * (float)(Math.PI / 180.0);
            float dx = (float)Math.Sin(angleRad);
            float dy = -(float)Math.Cos(angleRad);

            float cx = bounds.X + bounds.Width / 2f;
            float cy = bounds.Y + bounds.Height / 2f;

            // CSS spec: gradient line extends to perpendicular intersections with closest corners.
            // Half-length = (|W * sin(angle)| + |H * cos(angle)|) / 2
            float halfLen = (Math.Abs(bounds.Width * (float)Math.Sin(angleRad))
                           + Math.Abs(bounds.Height * (float)Math.Cos(angleRad))) / 2f;

            var start = new SKPoint(cx - dx * halfLen, cy - dy * halfLen);
            var end = new SKPoint(cx + dx * halfLen, cy + dy * halfLen);

            return SKShader.CreateLinearGradient(start, end, colors, positions, SKShaderTileMode.Clamp);
        }

        private static SKShader CreateRadialShader(GradientInfo gradient, RectF bounds,
            SKColor[] colors, float[] positions)
        {
            float cx = bounds.X + gradient.Center.X * bounds.Width;
            float cy = bounds.Y + gradient.Center.Y * bounds.Height;
            float radius = Math.Max(gradient.RadiusX * bounds.Width, gradient.RadiusY * bounds.Height);

            if (radius <= 0f)
            {
                radius = Math.Max(bounds.Width, bounds.Height) / 2f;
            }

            return SKShader.CreateRadialGradient(
                new SKPoint(cx, cy), radius, colors, positions, SKShaderTileMode.Clamp);
        }

        private static SKShader CreateSweepShader(GradientInfo gradient, RectF bounds,
            SKColor[] colors, float[] positions)
        {
            float cx = bounds.X + gradient.Center.X * bounds.Width;
            float cy = bounds.Y + gradient.Center.Y * bounds.Height;

            // CSS conic: 0deg = top (12 o'clock), clockwise
            // Skia sweep: 0deg = right (3 o'clock), clockwise
            // Offset: skia = css - 90
            float startAngle = gradient.Angle - 90f;
            float endAngle = startAngle + 360f;

            return SKShader.CreateSweepGradient(
                new SKPoint(cx, cy), colors, positions, SKShaderTileMode.Clamp, startAngle, endAngle);
        }
    }
}
