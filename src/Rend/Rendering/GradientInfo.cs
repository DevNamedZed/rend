using System;
using Rend.Core.Values;

namespace Rend.Rendering
{
    /// <summary>
    /// Describes a gradient fill, including its type, color stops, and geometry parameters.
    /// </summary>
    internal sealed class GradientInfo
    {
        /// <summary>Gets or sets the type of gradient.</summary>
        public GradientType Type { get; set; }

        /// <summary>Gets or sets the color stops that define the gradient.</summary>
        public GradientStop[] Stops { get; set; }

        /// <summary>Gets or sets the angle in degrees for linear gradients.</summary>
        public float Angle { get; set; }

        /// <summary>Gets or sets the center point for radial and conic gradients.</summary>
        public PointF Center { get; set; }

        /// <summary>Gets or sets the horizontal radius for radial gradients.</summary>
        public float RadiusX { get; set; }

        /// <summary>Gets or sets the vertical radius for radial gradients.</summary>
        public float RadiusY { get; set; }

        /// <summary>Whether this is a repeating gradient (repeating-linear-gradient, repeating-radial-gradient, etc.).</summary>
        public bool Repeating { get; set; }

        /// <summary>
        /// [CSS-BACKGROUNDS §2.11] Optional explicit positioning area for the gradient shader.
        /// When set, the shader is sized/positioned to this rect instead of the fill rect.
        /// Used for canvas backgrounds where the painting area (canvas) differs from
        /// the positioning area (root/body padding box).
        /// </summary>
        public RectF? PositioningBounds { get; set; }

        /// <summary>
        /// [CSS-COLOR4 §12] Color interpolation space for gradient stops (e.g., "hsl", "lch", "oklch").
        /// When null, defaults to sRGB (Skia native interpolation).
        /// </summary>
        public string? ColorInterpolationSpace { get; set; }

        /// <summary>
        /// [CSS-COLOR4 §12.1] Hue interpolation method for polar color spaces.
        /// One of "shorter" (default), "longer", "increasing", "decreasing".
        /// </summary>
        public string? HueInterpolationMethod { get; set; }

        /// <summary>
        /// Creates a new <see cref="GradientInfo"/> with default values.
        /// </summary>
        public GradientInfo()
        {
            Stops = Array.Empty<GradientStop>();
        }

        /// <summary>
        /// Creates a new <see cref="GradientInfo"/> with the specified type and stops.
        /// </summary>
        /// <param name="type">The gradient type.</param>
        /// <param name="stops">The color stops.</param>
        public GradientInfo(GradientType type, GradientStop[] stops)
        {
            Type = type;
            Stops = stops ?? throw new ArgumentNullException(nameof(stops));
        }
    }
}
