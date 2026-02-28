using System;
using Rend.Css;

namespace Rend.Fonts
{
    /// <summary>
    /// Immutable descriptor identifying a font by family, weight, style, and stretch.
    /// </summary>
    public readonly struct FontDescriptor : IEquatable<FontDescriptor>
    {
        /// <summary>
        /// Gets the font family name.
        /// </summary>
        public string Family { get; }

        /// <summary>
        /// Gets the font weight (default 400 = normal, 700 = bold).
        /// </summary>
        public float Weight { get; }

        /// <summary>
        /// Gets the font style.
        /// </summary>
        public CssFontStyle Style { get; }

        /// <summary>
        /// Gets the font stretch percentage (default 100 = normal).
        /// </summary>
        public float Stretch { get; }

        /// <summary>
        /// Creates a new <see cref="FontDescriptor"/>.
        /// </summary>
        public FontDescriptor(string family, float weight = 400f, CssFontStyle style = CssFontStyle.Normal, float stretch = 100f)
        {
            Family = family ?? throw new ArgumentNullException(nameof(family));
            Weight = weight;
            Style = style;
            Stretch = stretch;
        }

        /// <inheritdoc />
        public bool Equals(FontDescriptor other)
        {
            return string.Equals(Family, other.Family, StringComparison.OrdinalIgnoreCase)
                && Weight == other.Weight
                && Style == other.Style
                && Stretch == other.Stretch;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is FontDescriptor other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Family, StringComparer.OrdinalIgnoreCase);
            hashCode.Add(Weight);
            hashCode.Add(Style);
            hashCode.Add(Stretch);
            return hashCode.ToHashCode();
        }

        /// <summary>Equality operator.</summary>
        public static bool operator ==(FontDescriptor left, FontDescriptor right) => left.Equals(right);

        /// <summary>Inequality operator.</summary>
        public static bool operator !=(FontDescriptor left, FontDescriptor right) => !left.Equals(right);

        /// <inheritdoc />
        public override string ToString() => $"{Family} W{Weight} {Style} S{Stretch}";
    }
}
