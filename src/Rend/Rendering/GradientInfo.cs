using System;
using Rend.Core.Values;

namespace Rend.Rendering
{
    /// <summary>
    /// Describes a gradient fill, including its type, color stops, and geometry parameters.
    /// </summary>
    public sealed class GradientInfo
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
