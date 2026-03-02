using System;
using System.Collections.Generic;

namespace Rend.Fonts
{
    /// <summary>
    /// Represents a variation axis from the OpenType fvar table.
    /// Variable fonts define continuous axes (e.g., weight 100–900, width 75–125).
    /// </summary>
    public readonly struct FontVariationAxis : IEquatable<FontVariationAxis>
    {
        /// <summary>The 4-character axis tag (e.g., "wght", "wdth", "ital", "slnt", "opsz").</summary>
        public string Tag { get; }

        /// <summary>The minimum value for this axis.</summary>
        public float MinValue { get; }

        /// <summary>The default value for this axis.</summary>
        public float DefaultValue { get; }

        /// <summary>The maximum value for this axis.</summary>
        public float MaxValue { get; }

        /// <summary>The human-readable name of this axis (from the name table).</summary>
        public string Name { get; }

        /// <summary>Whether this is a registered (standard) axis. Registered axes have lowercase tags.</summary>
        public bool IsRegistered => Tag.Length == 4 && Tag[0] >= 'a' && Tag[0] <= 'z';

        public FontVariationAxis(string tag, float minValue, float defaultValue, float maxValue, string name)
        {
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));
            MinValue = minValue;
            DefaultValue = defaultValue;
            MaxValue = maxValue;
            Name = name ?? string.Empty;
        }

        /// <summary>Returns true if the given value is within this axis's range.</summary>
        public bool Contains(float value) => value >= MinValue && value <= MaxValue;

        /// <summary>Clamps a value to this axis's range.</summary>
        public float Clamp(float value) => Math.Max(MinValue, Math.Min(MaxValue, value));

        public bool Equals(FontVariationAxis other) =>
            Tag == other.Tag && MinValue == other.MinValue &&
            DefaultValue == other.DefaultValue && MaxValue == other.MaxValue;

        public override bool Equals(object? obj) => obj is FontVariationAxis other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Tag, MinValue, DefaultValue, MaxValue);

        public static bool operator ==(FontVariationAxis left, FontVariationAxis right) => left.Equals(right);
        public static bool operator !=(FontVariationAxis left, FontVariationAxis right) => !left.Equals(right);

        public override string ToString() => $"{Tag} [{MinValue}..{DefaultValue}..{MaxValue}] \"{Name}\"";
    }

    /// <summary>
    /// A named instance from the fvar table (e.g., "Bold", "Light Condensed").
    /// </summary>
    public readonly struct FontNamedInstance
    {
        /// <summary>The instance name (from the name table).</summary>
        public string Name { get; }

        /// <summary>The axis values for this instance, keyed by axis tag.</summary>
        public IReadOnlyDictionary<string, float> Coordinates { get; }

        public FontNamedInstance(string name, Dictionary<string, float> coordinates)
        {
            Name = name ?? string.Empty;
            Coordinates = coordinates ?? throw new ArgumentNullException(nameof(coordinates));
        }

        public override string ToString() => Name;
    }
}
