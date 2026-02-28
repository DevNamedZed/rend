using System;
using System.Runtime.CompilerServices;

namespace Rend.Core.Values
{
    /// <summary>
    /// Represents an RGBA color value. Immutable value type.
    /// </summary>
    public readonly struct CssColor : IEquatable<CssColor>
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CssColor(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static readonly CssColor Transparent = new CssColor(0, 0, 0, 0);
        public static readonly CssColor Black = new CssColor(0, 0, 0);
        public static readonly CssColor White = new CssColor(255, 255, 255);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CssColor FromRgba(byte r, byte g, byte b, byte a = 255)
            => new CssColor(r, g, b, a);

        /// <summary>
        /// Create a color from HSL values.
        /// </summary>
        /// <param name="h">Hue in degrees (0-360).</param>
        /// <param name="s">Saturation (0-1).</param>
        /// <param name="l">Lightness (0-1).</param>
        /// <param name="a">Alpha (0-1).</param>
        public static CssColor FromHsl(float h, float s, float l, float a = 1f)
        {
            h = ((h % 360f) + 360f) % 360f;
            s = Math.Max(0f, Math.Min(1f, s));
            l = Math.Max(0f, Math.Min(1f, l));

            float c = (1f - Math.Abs(2f * l - 1f)) * s;
            float x = c * (1f - Math.Abs((h / 60f) % 2f - 1f));
            float m = l - c / 2f;

            float r1, g1, b1;
            if (h < 60f) { r1 = c; g1 = x; b1 = 0f; }
            else if (h < 120f) { r1 = x; g1 = c; b1 = 0f; }
            else if (h < 180f) { r1 = 0f; g1 = c; b1 = x; }
            else if (h < 240f) { r1 = 0f; g1 = x; b1 = c; }
            else if (h < 300f) { r1 = x; g1 = 0f; b1 = c; }
            else { r1 = c; g1 = 0f; b1 = x; }

            return new CssColor(
                (byte)Math.Round((r1 + m) * 255f),
                (byte)Math.Round((g1 + m) * 255f),
                (byte)Math.Round((b1 + m) * 255f),
                (byte)Math.Round(a * 255f)
            );
        }

        /// <summary>
        /// Returns normalized float components for PDF (0.0 - 1.0 range).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToFloatRgb(out float r, out float g, out float b)
        {
            r = R / 255f;
            g = G / 255f;
            b = B / 255f;
        }

        /// <summary>
        /// Returns the alpha as a float (0.0 - 1.0).
        /// </summary>
        public float AlphaF
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => A / 255f;
        }

        public bool Equals(CssColor other)
            => R == other.R && G == other.G && B == other.B && A == other.A;

        public override bool Equals(object? obj)
            => obj is CssColor other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(R, G, B, A);

        public static bool operator ==(CssColor left, CssColor right) => left.Equals(right);
        public static bool operator !=(CssColor left, CssColor right) => !left.Equals(right);

        public override string ToString()
            => A == 255
                ? $"rgb({R}, {G}, {B})"
                : $"rgba({R}, {G}, {B}, {AlphaF:F2})";
    }
}
