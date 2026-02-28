using System;
using Rend.Core.Values;

namespace Rend.Rendering
{
    /// <summary>
    /// Represents a color stop within a gradient, defined by a color and a position from 0 to 1.
    /// </summary>
    public readonly struct GradientStop : IEquatable<GradientStop>
    {
        /// <summary>Gets the color at this stop.</summary>
        public CssColor Color { get; }

        /// <summary>Gets the position of this stop along the gradient, in the range 0 to 1.</summary>
        public float Position { get; }

        /// <summary>
        /// Creates a new <see cref="GradientStop"/>.
        /// </summary>
        /// <param name="color">The color at this stop.</param>
        /// <param name="position">The position along the gradient (0 to 1).</param>
        public GradientStop(CssColor color, float position)
        {
            Color = color;
            Position = position;
        }

        /// <inheritdoc />
        public bool Equals(GradientStop other)
            => Color == other.Color && Position == other.Position;

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj is GradientStop other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(Color, Position);

        /// <summary>Equality operator.</summary>
        public static bool operator ==(GradientStop left, GradientStop right) => left.Equals(right);

        /// <summary>Inequality operator.</summary>
        public static bool operator !=(GradientStop left, GradientStop right) => !left.Equals(right);

        /// <inheritdoc />
        public override string ToString() => $"{Color} @ {Position:P0}";
    }
}
