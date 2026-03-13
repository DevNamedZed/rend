using System;
using Rend.Css;

namespace Rend.Fonts
{
    /// <summary>
    /// Immutable descriptor identifying a font by family list, weight, style, and stretch.
    /// The <see cref="Families"/> array preserves the CSS font-family fallback chain.
    /// <see cref="Family"/> returns the first (primary) family name for convenience.
    /// </summary>
    public readonly struct FontDescriptor : IEquatable<FontDescriptor>
    {
        private static readonly string[] DefaultFamilies = new[] { "serif" };

        /// <summary>
        /// Gets the ordered list of font family names (CSS fallback chain).
        /// </summary>
        public string[] Families { get; }

        /// <summary>
        /// Gets the primary (first) font family name.
        /// </summary>
        public string Family => Families.Length > 0 ? Families[0] : "serif";

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
        /// Creates a new <see cref="FontDescriptor"/> with a single family name.
        /// </summary>
        public FontDescriptor(string family, float weight = 400f, CssFontStyle style = CssFontStyle.Normal, float stretch = 100f)
        {
            if (family == null)
            {
                throw new ArgumentNullException(nameof(family));
            }
            Families = new[] { family };
            Weight = weight;
            Style = style;
            Stretch = stretch;
        }

        /// <summary>
        /// Creates a new <see cref="FontDescriptor"/> with an ordered list of family names
        /// representing the CSS font-family fallback chain.
        /// </summary>
        public FontDescriptor(string[] families, float weight = 400f, CssFontStyle style = CssFontStyle.Normal, float stretch = 100f)
        {
            if (families == null || families.Length == 0)
            {
                throw new ArgumentNullException(nameof(families));
            }
            Families = families;
            Weight = weight;
            Style = style;
            Stretch = stretch;
        }

        /// <inheritdoc />
        public bool Equals(FontDescriptor other)
        {
            if (Weight != other.Weight || Style != other.Style || Stretch != other.Stretch)
            {
                return false;
            }
            if (Families.Length != other.Families.Length)
            {
                return false;
            }
            for (int i = 0; i < Families.Length; i++)
            {
                if (!string.Equals(Families[i], other.Families[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is FontDescriptor other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            for (int i = 0; i < Families.Length; i++)
            {
                hashCode.Add(Families[i], StringComparer.OrdinalIgnoreCase);
            }
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
        public override string ToString() => $"{string.Join(", ", Families)} W{Weight} {Style} S{Stretch}";

        /// <summary>
        /// Converts a <see cref="CssFontStretch"/> enum value to its CSS percentage equivalent.
        /// </summary>
        public static float StretchToPercentage(CssFontStretch stretch)
        {
            switch (stretch)
            {
                case CssFontStretch.UltraCondensed: return 50f;
                case CssFontStretch.ExtraCondensed: return 62.5f;
                case CssFontStretch.Condensed: return 75f;
                case CssFontStretch.SemiCondensed: return 87.5f;
                case CssFontStretch.Normal: return 100f;
                case CssFontStretch.SemiExpanded: return 112.5f;
                case CssFontStretch.Expanded: return 125f;
                case CssFontStretch.ExtraExpanded: return 150f;
                case CssFontStretch.UltraExpanded: return 200f;
                default: return 100f;
            }
        }
    }
}
