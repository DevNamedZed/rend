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
        /// Create a color from HWB values (CSS Color Level 4).
        /// </summary>
        /// <param name="h">Hue in degrees (0-360).</param>
        /// <param name="w">Whiteness (0-1).</param>
        /// <param name="b">Blackness (0-1).</param>
        /// <param name="a">Alpha (0-1).</param>
        public static CssColor FromHwb(float h, float w, float b, float a = 1f)
        {
            w = Math.Max(0f, Math.Min(1f, w));
            b = Math.Max(0f, Math.Min(1f, b));

            if (w + b >= 1f)
            {
                float gray = w / (w + b);
                byte g = (byte)Math.Round(gray * 255f);
                return new CssColor(g, g, g, (byte)Math.Round(Math.Max(0f, Math.Min(1f, a)) * 255f));
            }

            var baseColor = FromHsl(h, 1f, 0.5f);
            float rf = baseColor.R / 255f * (1f - w - b) + w;
            float gf = baseColor.G / 255f * (1f - w - b) + w;
            float bf = baseColor.B / 255f * (1f - w - b) + w;

            return new CssColor(
                (byte)Math.Round(rf * 255f),
                (byte)Math.Round(gf * 255f),
                (byte)Math.Round(bf * 255f),
                (byte)Math.Round(Math.Max(0f, Math.Min(1f, a)) * 255f));
        }

        /// <summary>
        /// Create a color from CIE Lab values (CSS Color Level 4).
        /// </summary>
        /// <param name="l">Lightness (0-100).</param>
        /// <param name="a">a axis (-125 to 125).</param>
        /// <param name="b">b axis (-125 to 125).</param>
        /// <param name="alpha">Alpha (0-1).</param>
        public static CssColor FromLab(float l, float a, float b, float alpha = 1f)
        {
            // Lab → XYZ (D50)
            float fy = (l + 16f) / 116f;
            float fx = a / 500f + fy;
            float fz = fy - b / 200f;

            const float delta = 6f / 29f;
            const float factor = 3f * delta * delta;

            float xr = fx > delta ? fx * fx * fx : (fx - 16f / 116f) * factor;
            float yr = fy > delta ? fy * fy * fy : (fy - 16f / 116f) * factor;
            float zr = fz > delta ? fz * fz * fz : (fz - 16f / 116f) * factor;

            // D50 white point
            float x = xr * 0.96422f;
            float y = yr * 1.0f;
            float z = zr * 0.82521f;

            // D50 → D65 Bradford chromatic adaptation
            float xd = x * 0.9555766f + y * -0.0230393f + z * 0.0631636f;
            float yd = x * -0.0282895f + y * 1.0099416f + z * 0.0210077f;
            float zd = x * 0.0122982f + y * -0.0204830f + z * 1.3299098f;

            // XYZ D65 → linear sRGB
            float rl = xd * 3.2404542f + yd * -1.5371385f + zd * -0.4985314f;
            float gl = xd * -0.9692660f + yd * 1.8760108f + zd * 0.0415560f;
            float bl = xd * 0.0556434f + yd * -0.2040259f + zd * 1.0572252f;

            return new CssColor(
                LinearToSrgbByte(rl),
                LinearToSrgbByte(gl),
                LinearToSrgbByte(bl),
                (byte)Math.Round(Math.Max(0f, Math.Min(1f, alpha)) * 255f));
        }

        /// <summary>
        /// Create a color from CIE LCH values (CSS Color Level 4).
        /// </summary>
        /// <param name="l">Lightness (0-100).</param>
        /// <param name="c">Chroma (0-150).</param>
        /// <param name="h">Hue in degrees (0-360).</param>
        /// <param name="alpha">Alpha (0-1).</param>
        public static CssColor FromLch(float l, float c, float h, float alpha = 1f)
        {
            float hRad = h * ((float)Math.PI / 180f);
            float a = c * (float)Math.Cos(hRad);
            float b = c * (float)Math.Sin(hRad);
            return FromLab(l, a, b, alpha);
        }

        /// <summary>
        /// Create a color from OKLab values (CSS Color Level 4).
        /// </summary>
        /// <param name="l">Lightness (0-1).</param>
        /// <param name="a">a axis (-0.4 to 0.4).</param>
        /// <param name="b">b axis (-0.4 to 0.4).</param>
        /// <param name="alpha">Alpha (0-1).</param>
        public static CssColor FromOklab(float l, float a, float b, float alpha = 1f)
        {
            // OKLab → LMS (intermediate)
            float l_ = l + 0.3963377774f * a + 0.2158037573f * b;
            float m_ = l - 0.1055613458f * a - 0.0638541728f * b;
            float s_ = l - 0.0894841775f * a - 1.2914855480f * b;

            // Cube to get LMS
            float lc = l_ * l_ * l_;
            float mc = m_ * m_ * m_;
            float sc = s_ * s_ * s_;

            // LMS → linear sRGB
            float rl = 4.0767416621f * lc - 3.3077115913f * mc + 0.2309699292f * sc;
            float gl = -1.2684380046f * lc + 2.6097574011f * mc - 0.3413193965f * sc;
            float bl = -0.0041960863f * lc - 0.7034186147f * mc + 1.7076147010f * sc;

            return new CssColor(
                LinearToSrgbByte(rl),
                LinearToSrgbByte(gl),
                LinearToSrgbByte(bl),
                (byte)Math.Round(Math.Max(0f, Math.Min(1f, alpha)) * 255f));
        }

        /// <summary>
        /// Create a color from OKLCH values (CSS Color Level 4).
        /// </summary>
        /// <param name="l">Lightness (0-1).</param>
        /// <param name="c">Chroma (0-0.4).</param>
        /// <param name="h">Hue in degrees (0-360).</param>
        /// <param name="alpha">Alpha (0-1).</param>
        public static CssColor FromOklch(float l, float c, float h, float alpha = 1f)
        {
            float hRad = h * ((float)Math.PI / 180f);
            float a = c * (float)Math.Cos(hRad);
            float b = c * (float)Math.Sin(hRad);
            return FromOklab(l, a, b, alpha);
        }

        /// <summary>
        /// Mix two colors in sRGB space (CSS color-mix()).
        /// </summary>
        /// <param name="c1">First color.</param>
        /// <param name="p1">First color weight (0-1).</param>
        /// <param name="c2">Second color.</param>
        /// <param name="p2">Second color weight (0-1).</param>
        public static CssColor Mix(CssColor c1, float p1, CssColor c2, float p2)
        {
            float total = p1 + p2;
            if (total <= 0f) return Transparent;
            p1 /= total;
            p2 /= total;

            return new CssColor(
                (byte)Math.Round(c1.R * p1 + c2.R * p2),
                (byte)Math.Round(c1.G * p1 + c2.G * p2),
                (byte)Math.Round(c1.B * p1 + c2.B * p2),
                (byte)Math.Round(c1.A * p1 + c2.A * p2));
        }

        private static byte LinearToSrgbByte(float c)
        {
            float s;
            if (c <= 0.0031308f)
                s = 12.92f * c;
            else
                s = 1.055f * (float)Math.Pow(c, 1.0 / 2.4) - 0.055f;
            return (byte)Math.Round(Math.Max(0f, Math.Min(1f, s)) * 255f);
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
