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
            float rx = gradient.RadiusX * bounds.Width;
            float ry = gradient.RadiusY * bounds.Height;

            if (rx <= 0f && ry <= 0f)
            {
                rx = bounds.Width / 2f;
                ry = bounds.Height / 2f;
            }

            // For elliptical gradients, create a circular gradient using the larger radius
            // and scale the other axis via a local matrix transform.
            float radius = Math.Max(rx, ry);
            if (radius <= 0f) radius = 1f;

            if (Math.Abs(rx - ry) < 0.5f)
            {
                // Nearly circular — no scaling needed
                return SKShader.CreateRadialGradient(
                    new SKPoint(cx, cy), radius, colors, positions, SKShaderTileMode.Clamp);
            }

            // Scale the shorter axis to create an ellipse
            var matrix = SKMatrix.Identity;
            if (rx < ry)
            {
                // Scale X axis: map circle of radius ry to ellipse with rx horizontal
                float scaleX = rx / ry;
                matrix = SKMatrix.CreateScale(scaleX, 1f, cx, cy);
            }
            else
            {
                // Scale Y axis: map circle of radius rx to ellipse with ry vertical
                float scaleY = ry / rx;
                matrix = SKMatrix.CreateScale(1f, scaleY, cx, cy);
            }

            return SKShader.CreateRadialGradient(
                new SKPoint(cx, cy), radius, colors, positions, SKShaderTileMode.Clamp, matrix);
        }

        private static SKShader CreateSweepShader(GradientInfo gradient, RectF bounds,
            SKColor[] colors, float[] positions)
        {
            float cx = bounds.X + gradient.Center.X * bounds.Width;
            float cy = bounds.Y + gradient.Center.Y * bounds.Height;

            // CSS conic: 0deg = top (12 o'clock), clockwise
            // Skia sweep: 0deg = right (3 o'clock), clockwise
            // Skia's CreateSweepGradient with startAngle/endAngle has a bug where
            // negative startAngle causes the sub-zero range to clamp instead of interpolate.
            // Workaround: remap CSS stop positions into Skia's default [0°,360°] sweep
            // with proper wrapping at the boundary.
            float offset = (gradient.Angle - 90f) / 360f;
            // Normalize offset to [0, 1)
            offset = offset - (float)Math.Floor(offset);

            if (Math.Abs(offset) < 0.001f || Math.Abs(offset - 1f) < 0.001f)
            {
                // No rotation needed — use default sweep
                return SKShader.CreateSweepGradient(
                    new SKPoint(cx, cy), colors, positions);
            }

            // Remap positions: newPos = (cssPos + offset) mod 1.0
            // This wraps around, so we need to split at the boundary and add interpolated stops.
            var remapped = new System.Collections.Generic.List<(SKColor color, float pos)>();

            for (int i = 0; i < colors.Length; i++)
            {
                float newPos = positions[i] + offset;
                if (newPos >= 1f) newPos -= 1f;
                remapped.Add((colors[i], newPos));
            }

            // Find the wrap point: where a stop crosses the 1.0 boundary.
            // For each pair of adjacent CSS stops where one remaps to > 1 and next to < 1,
            // we need to add interpolated boundary stops at 0.0 and 1.0.
            var finalStops = new System.Collections.Generic.List<(SKColor color, float pos)>();

            for (int i = 0; i < colors.Length - 1; i++)
            {
                float p0 = positions[i] + offset;
                float p1 = positions[i + 1] + offset;

                if (p0 < 1f && p1 >= 1f)
                {
                    // This segment crosses the wrap boundary
                    float t = (1f - p0) / (p1 - p0);
                    SKColor boundaryColor = LerpColor(colors[i], colors[i + 1], t);
                    // Add stop just before the wrap
                    finalStops.Add((boundaryColor, 1f));
                    // Add stop just after the wrap (position 0)
                    finalStops.Add((boundaryColor, 0f));
                }
            }

            // Add all remapped stops
            for (int i = 0; i < remapped.Count; i++)
                finalStops.Add(remapped[i]);

            // Sort by position
            finalStops.Sort((a, b) => a.pos.CompareTo(b.pos));

            var finalColors = new SKColor[finalStops.Count];
            var finalPositions = new float[finalStops.Count];
            for (int i = 0; i < finalStops.Count; i++)
            {
                finalColors[i] = finalStops[i].color;
                finalPositions[i] = finalStops[i].pos;
            }

            return SKShader.CreateSweepGradient(
                new SKPoint(cx, cy), finalColors, finalPositions);
        }

        private static SKColor LerpColor(SKColor a, SKColor b, float t)
        {
            return new SKColor(
                (byte)(a.Red + (b.Red - a.Red) * t),
                (byte)(a.Green + (b.Green - a.Green) * t),
                (byte)(a.Blue + (b.Blue - a.Blue) * t),
                (byte)(a.Alpha + (b.Alpha - a.Alpha) * t));
        }
    }
}
