using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css.Parser.Internal;
using Rend.Rendering;
using SkiaSharp;

namespace Rend.Output.Image.Internal
{
    /// <summary>
    /// Converts <see cref="GradientInfo"/> definitions into SkiaSharp <see cref="SKShader"/> instances.
    /// </summary>
    internal static class SkiaGradientBuilder
    {
        private const int InterpolationSteps = 64;

        /// <summary>
        /// Creates an <see cref="SKShader"/> from the given gradient info and bounding rectangle.
        /// </summary>
        internal static SKShader? CreateShader(GradientInfo gradient, RectF bounds, SKMatrix? localMatrix = null)
        {
            if (gradient.Stops.Length == 0)
            {
                return null;
            }

            SKColor[] colors;
            float[] positions;

            if (gradient.ColorInterpolationSpace != null && gradient.ColorInterpolationSpace != "srgb")
            {
                ExpandStopsForColorSpace(gradient, out colors, out positions);
            }
            else
            {
                colors = new SKColor[gradient.Stops.Length];
                positions = new float[gradient.Stops.Length];
                for (int i = 0; i < gradient.Stops.Length; i++)
                {
                    GradientStop stop = gradient.Stops[i];
                    colors[i] = new SKColor(stop.Color.R, stop.Color.G, stop.Color.B, stop.Color.A);
                    positions[i] = stop.Position;
                }
            }

            // [CSS-BACKGROUNDS §2.11] When a positioning area is specified (canvas
            // backgrounds), use it for shader geometry instead of the fill bounds.
            RectF shaderBounds = gradient.PositioningBounds ?? bounds;

            switch (gradient.Type)
            {
                case GradientType.Linear:
                    return CreateLinearShader(gradient, shaderBounds, colors, positions, localMatrix);
                case GradientType.Radial:
                    return CreateRadialShader(gradient, shaderBounds, colors, positions);
                case GradientType.Conic:
                    return CreateSweepShader(gradient, shaderBounds, colors, positions);
                default:
                    return CreateLinearShader(gradient, bounds, colors, positions, localMatrix);
            }
        }

        /// <summary>
        /// [CSS-COLOR4 §12] Pre-interpolate gradient stops in a non-sRGB color space.
        /// Skia only interpolates in sRGB; we generate intermediate stops so the sRGB
        /// segments approximate the target color-space curve.
        /// </summary>
        private static void ExpandStopsForColorSpace(GradientInfo gradient,
            out SKColor[] colors, out float[] positions)
        {
            string space = gradient.ColorInterpolationSpace!;
            string? hueMethod = gradient.HueInterpolationMethod;
            var stopList = gradient.Stops;

            var expandedColors = new List<SKColor>();
            var expandedPositions = new List<float>();

            expandedColors.Add(new SKColor(stopList[0].Color.R, stopList[0].Color.G,
                stopList[0].Color.B, stopList[0].Color.A));
            expandedPositions.Add(stopList[0].Position);

            for (int i = 0; i < stopList.Length - 1; i++)
            {
                CssColor startColor = stopList[i].Color;
                CssColor endColor = stopList[i + 1].Color;
                float startPos = stopList[i].Position;
                float endPos = stopList[i + 1].Position;

                for (int step = 1; step <= InterpolationSteps; step++)
                {
                    float t = step / (float)InterpolationSteps;
                    float position = startPos + (endPos - startPos) * t;

                    CssColor mixed = CssColorParser.MixInSpace(space, startColor, 1f - t,
                        endColor, t, hueMethod);

                    expandedColors.Add(new SKColor(mixed.R, mixed.G, mixed.B, mixed.A));
                    expandedPositions.Add(position);
                }
            }

            colors = expandedColors.ToArray();
            positions = expandedPositions.ToArray();
        }

        private static SKShader CreateLinearShader(GradientInfo gradient, RectF bounds,
            SKColor[] colors, float[] positions, SKMatrix? localMatrix = null)
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

            var tileMode = gradient.Repeating ? SKShaderTileMode.Repeat : SKShaderTileMode.Clamp;

            if (gradient.Repeating && positions.Length >= 2)
            {
                // For repeating gradients, the shader start/end points span only the
                // first-to-last stop distance, not the full gradient line. Skia's Repeat
                // tile mode then tiles this pattern across the entire surface.
                float firstPos = positions[0];
                float lastPos = positions[positions.Length - 1];
                float range = lastPos - firstPos;
                if (range > 0.0001f)
                {
                    // Remap positions to 0-1 within the repeat range
                    var remapped = new float[positions.Length];
                    for (int i = 0; i < positions.Length; i++)
                        remapped[i] = (positions[i] - firstPos) / range;

                    // Compute shader points spanning only the repeat distance
                    float startOffset = -halfLen + firstPos * 2f * halfLen;
                    float endOffset = -halfLen + lastPos * 2f * halfLen;
                    var start = new SKPoint(cx + dx * startOffset, cy + dy * startOffset);
                    var end = new SKPoint(cx + dx * endOffset, cy + dy * endOffset);

                    return SKShader.CreateLinearGradient(start, end, colors, remapped, tileMode);
                }
            }

            var defaultStart = new SKPoint(cx - dx * halfLen, cy - dy * halfLen);
            var defaultEnd = new SKPoint(cx + dx * halfLen, cy + dy * halfLen);

            // [SVG §13.2.2] Apply gradientTransform to shader start/end points
            if (localMatrix.HasValue)
            {
                var m = localMatrix.Value;
                defaultStart = m.MapPoint(defaultStart);
                defaultEnd = m.MapPoint(defaultEnd);
            }

            return SKShader.CreateLinearGradient(defaultStart, defaultEnd, colors, positions, tileMode);
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

            var tileMode = gradient.Repeating ? SKShaderTileMode.Repeat : SKShaderTileMode.Clamp;

            // For repeating radial gradients, remap positions to 0-1 within the repeat
            // range and adjust the Skia radius to match the repeat distance.
            // Otherwise Skia tiles at the full radius, not at the last stop position.
            if (gradient.Repeating && positions.Length >= 2)
            {
                float firstPos = positions[0];
                float lastPos = positions[positions.Length - 1];
                float range = lastPos - firstPos;
                if (range > 0.0001f && range < 0.999f)
                {
                    var remapped = new float[positions.Length];
                    for (int i = 0; i < positions.Length; i++)
                    {
                        remapped[i] = (positions[i] - firstPos) / range;
                    }
                    // Skia radius spans the repeat unit (lastPos * radius in pixels)
                    float repeatRadius = radius * lastPos;
                    if (repeatRadius <= 0f) repeatRadius = 1f;
                    positions = remapped;
                    radius = repeatRadius;
                    // Recalculate rx/ry for ellipse scaling below
                    if (rx >= ry)
                    {
                        float scale = lastPos;
                        rx *= scale;
                        ry *= scale;
                    }
                    else
                    {
                        float scale = lastPos;
                        rx *= scale;
                        ry *= scale;
                    }
                }
            }

            if (Math.Abs(rx - ry) < 0.5f)
            {
                // Nearly circular — no scaling needed
                return SKShader.CreateRadialGradient(
                    new SKPoint(cx, cy), radius, colors, positions, tileMode);
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
                new SKPoint(cx, cy), radius, colors, positions, tileMode, matrix);
        }

        private static SKShader CreateSweepShader(GradientInfo gradient, RectF bounds,
            SKColor[] colors, float[] positions)
        {
            float cx = bounds.X + gradient.Center.X * bounds.Width;
            float cy = bounds.Y + gradient.Center.Y * bounds.Height;

            // CSS conic: 0deg = top (12 o'clock), clockwise
            // Skia sweep: 0deg = right (3 o'clock), clockwise
            float offset = (gradient.Angle - 90f) / 360f;
            offset = offset - (float)Math.Floor(offset);

            if (Math.Abs(offset) < 0.001f || Math.Abs(offset - 1f) < 0.001f)
            {
                return SKShader.CreateSweepGradient(
                    new SKPoint(cx, cy), colors, positions);
            }

            // Find the first CSS stop whose Skia position wraps past 1.0
            int splitIndex = -1;
            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i] + offset >= 1f)
                {
                    splitIndex = i;
                    break;
                }
            }

            if (splitIndex < 0)
            {
                var shifted = new float[positions.Length];
                for (int i = 0; i < positions.Length; i++)
                {
                    shifted[i] = positions[i] + offset;
                }
                return SKShader.CreateSweepGradient(
                    new SKPoint(cx, cy), colors, shifted);
            }

            // Compute the interpolated color at the wrap boundary (Skia 0.0/1.0)
            float wrapCssPos = 1f - offset;
            SKColor boundaryColor;
            if (splitIndex > 0)
            {
                float p0 = positions[splitIndex - 1];
                float p1 = positions[splitIndex];
                if (Math.Abs(p1 - p0) < 0.0001f)
                {
                    boundaryColor = colors[splitIndex - 1];
                }
                else
                {
                    float t = (wrapCssPos - p0) / (p1 - p0);
                    boundaryColor = LerpColor(colors[splitIndex - 1], colors[splitIndex], t);
                }
            }
            else
            {
                float p0 = positions[positions.Length - 1];
                float p1 = positions[0] + 1f;
                if (Math.Abs(p1 - p0) < 0.0001f)
                {
                    boundaryColor = colors[positions.Length - 1];
                }
                else
                {
                    float t = Math.Max(0f, Math.Min(1f, (wrapCssPos - p0) / (p1 - p0)));
                    boundaryColor = LerpColor(colors[positions.Length - 1], colors[0], t);
                }
            }

            // Build stops in explicit order to preserve hard transitions at the wrap point.
            // Wrapped CSS stops (splitIndex..end) come first, then non-wrapped (0..splitIndex-1).
            var finalColors = new System.Collections.Generic.List<SKColor>();
            var finalPositions = new System.Collections.Generic.List<float>();

            finalColors.Add(boundaryColor);
            finalPositions.Add(0f);

            for (int i = splitIndex; i < colors.Length; i++)
            {
                finalColors.Add(colors[i]);
                finalPositions.Add(positions[i] + offset - 1f);
            }

            for (int i = 0; i < splitIndex; i++)
            {
                finalColors.Add(colors[i]);
                finalPositions.Add(positions[i] + offset);
            }

            finalColors.Add(boundaryColor);
            finalPositions.Add(1f);

            return SKShader.CreateSweepGradient(
                new SKPoint(cx, cy), finalColors.ToArray(), finalPositions.ToArray());
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
