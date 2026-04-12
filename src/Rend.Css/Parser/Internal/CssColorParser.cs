using System;
using System.Collections.Generic;
using Rend.Core.Values;

namespace Rend.Css.Parser.Internal
{
    /// <summary>
    /// Parses CSS color values: hex (#fff, #ffffff, #ffffffff), rgb(), rgba(), hsl(), hsla(), named colors.
    /// </summary>
    internal static class CssColorParser
    {
        /// <summary>
        /// Try to parse a color from a hash token value (without the '#').
        /// </summary>
        public static bool TryParseHex(string hex, out CssColor color)
        {
            color = default;
            if (hex == null) return false;

            switch (hex.Length)
            {
                case 3: // #rgb
                {
                    if (TryHex(hex[0], out int r) && TryHex(hex[1], out int g) && TryHex(hex[2], out int b))
                    {
                        color = new CssColor((byte)(r * 17), (byte)(g * 17), (byte)(b * 17));
                        return true;
                    }
                    return false;
                }
                case 4: // #rgba
                {
                    if (TryHex(hex[0], out int r) && TryHex(hex[1], out int g) &&
                        TryHex(hex[2], out int b) && TryHex(hex[3], out int a))
                    {
                        color = new CssColor((byte)(r * 17), (byte)(g * 17), (byte)(b * 17), (byte)(a * 17));
                        return true;
                    }
                    return false;
                }
                case 6: // #rrggbb
                {
                    if (TryHex2(hex, 0, out int r) && TryHex2(hex, 2, out int g) && TryHex2(hex, 4, out int b))
                    {
                        color = new CssColor((byte)r, (byte)g, (byte)b);
                        return true;
                    }
                    return false;
                }
                case 8: // #rrggbbaa
                {
                    if (TryHex2(hex, 0, out int r) && TryHex2(hex, 2, out int g) &&
                        TryHex2(hex, 4, out int b) && TryHex2(hex, 6, out int a))
                    {
                        color = new CssColor((byte)r, (byte)g, (byte)b, (byte)a);
                        return true;
                    }
                    return false;
                }
                default:
                    return false;
            }
        }

        /// <summary>
        /// Try to parse rgb() or rgba() from function arguments.
        /// </summary>
        public static bool TryParseRgb(List<CssValue> args, out CssColor color)
        {
            color = default;

            // rgb(r, g, b) or rgba(r, g, b, a)
            // Values can be numbers (0-255) or percentages (0%-100%)
            if (args.Count < 3) return false;

            if (!TryGetColorComponent(args[0], 255f, out float r)) return false;
            if (!TryGetColorComponent(args[1], 255f, out float g)) return false;
            if (!TryGetColorComponent(args[2], 255f, out float b)) return false;

            float a = 1f;
            if (args.Count >= 4)
            {
                if (!TryGetAlpha(args[3], out a)) return false;
            }

            color = new CssColor(
                ClampByte(r),
                ClampByte(g),
                ClampByte(b),
                (byte)Math.Round(Math.Max(0f, Math.Min(1f, a)) * 255f));
            return true;
        }

        /// <summary>
        /// Try to parse hsl() or hsla() from function arguments.
        /// </summary>
        public static bool TryParseHsl(List<CssValue> args, out CssColor color)
        {
            color = default;

            if (args.Count < 3) return false;

            // Hue: number (degrees) or dimension with deg/rad/grad/turn
            float h;
            if (args[0] is CssNumberValue hn)
                h = hn.Value;
            else if (args[0] is CssDimensionValue hd)
            {
                h = ConvertAngle(hd.Value, hd.Unit);
            }
            else return false;

            // Saturation and lightness: percentages
            if (!(args[1] is CssPercentageValue sp)) return false;
            if (!(args[2] is CssPercentageValue lp)) return false;

            float s = sp.Value / 100f;
            float l = lp.Value / 100f;

            float a = 1f;
            if (args.Count >= 4)
            {
                if (!TryGetAlpha(args[3], out a)) return false;
            }

            color = CssColor.FromHsl(h, s, l, a);
            return true;
        }

        /// <summary>
        /// Try to parse hwb() from function arguments.
        /// </summary>
        public static bool TryParseHwb(List<CssValue> args, out CssColor color)
        {
            color = default;
            if (args.Count < 3) return false;

            float h;
            if (args[0] is CssNumberValue hn)
                h = hn.Value;
            else if (args[0] is CssDimensionValue hd)
                h = ConvertAngle(hd.Value, hd.Unit);
            else return false;

            if (!(args[1] is CssPercentageValue wp)) return false;
            if (!(args[2] is CssPercentageValue bp)) return false;

            float w = wp.Value / 100f;
            float b = bp.Value / 100f;

            float a = 1f;
            if (args.Count >= 4)
            {
                if (!TryGetAlpha(args[3], out a)) return false;
            }

            color = CssColor.FromHwb(h, w, b, a);
            return true;
        }

        /// <summary>
        /// Try to parse lab() from function arguments.
        /// </summary>
        public static bool TryParseLab(List<CssValue> args, out CssColor color)
        {
            color = default;
            if (args.Count < 3) return false;

            if (!TryGetLabComponent(args[0], 100f, out float l)) return false;
            if (!TryGetLabComponent(args[1], 125f, out float a)) return false;
            if (!TryGetLabComponent(args[2], 125f, out float b)) return false;

            // [CSS-COLOR4 §6.1] L clamped to [0, 100] at parsed-value time
            l = Math.Max(0f, Math.Min(100f, l));

            float alpha = 1f;
            if (args.Count >= 4)
            {
                if (!TryGetAlpha(args[3], out alpha)) return false;
            }

            color = CssColor.FromLab(l, a, b, alpha);
            return true;
        }

        /// <summary>
        /// Try to parse lch() from function arguments.
        /// </summary>
        public static bool TryParseLch(List<CssValue> args, out CssColor color)
        {
            color = default;
            if (args.Count < 3) return false;

            if (!TryGetLabComponent(args[0], 100f, out float l)) return false;
            if (!TryGetLabComponent(args[1], 150f, out float c)) return false;

            // [CSS-COLOR4 §6.1] L clamped to [0, 100] at parsed-value time
            l = Math.Max(0f, Math.Min(100f, l));

            float h;
            if (args[2] is CssNumberValue hn)
            {
                h = hn.Value;
            }
            else if (args[2] is CssDimensionValue hd)
            {
                h = ConvertAngle(hd.Value, hd.Unit);
            }
            else
            {
                return false;
            }

            float alpha = 1f;
            if (args.Count >= 4)
            {
                if (!TryGetAlpha(args[3], out alpha)) return false;
            }

            color = CssColor.FromLch(l, c, h, alpha);
            return true;
        }

        /// <summary>
        /// Try to parse oklab() from function arguments.
        /// </summary>
        public static bool TryParseOklab(List<CssValue> args, out CssColor color)
        {
            color = default;
            if (args.Count < 3) return false;

            if (!TryGetLabComponent(args[0], 1f, out float l)) return false;
            if (!TryGetLabComponent(args[1], 0.4f, out float a)) return false;
            if (!TryGetLabComponent(args[2], 0.4f, out float b)) return false;

            // [CSS-COLOR4 §6.1] L clamped to [0, 1] at parsed-value time
            l = Math.Max(0f, Math.Min(1f, l));

            float alpha = 1f;
            if (args.Count >= 4)
            {
                if (!TryGetAlpha(args[3], out alpha)) return false;
            }

            color = CssColor.FromOklab(l, a, b, alpha);
            return true;
        }

        /// <summary>
        /// Try to parse oklch() from function arguments.
        /// </summary>
        public static bool TryParseOklch(List<CssValue> args, out CssColor color)
        {
            color = default;
            if (args.Count < 3) return false;

            if (!TryGetLabComponent(args[0], 1f, out float l)) return false;
            if (!TryGetLabComponent(args[1], 0.4f, out float c)) return false;

            // [CSS-COLOR4 §6.1] L clamped to [0, 1] at parsed-value time
            l = Math.Max(0f, Math.Min(1f, l));

            float h;
            if (args[2] is CssNumberValue hn)
            {
                h = hn.Value;
            }
            else if (args[2] is CssDimensionValue hd)
            {
                h = ConvertAngle(hd.Value, hd.Unit);
            }
            else
            {
                return false;
            }

            float alpha = 1f;
            if (args.Count >= 4)
            {
                if (!TryGetAlpha(args[3], out alpha)) return false;
            }

            color = CssColor.FromOklch(l, c, h, alpha);
            return true;
        }

        /// <summary>
        /// Try to parse color-mix() from function arguments.
        /// color-mix(in srgb, color1 p1%, color2 p2%)
        /// </summary>
        public static bool TryParseColorMix(List<CssValue> args, out CssColor color)
        {
            color = default;
            // Minimum: "in", "srgb", color1, color2
            if (args.Count < 4) return false;

            // First two args should be "in" and a color space name
            if (!(args[0] is CssKeywordValue inKw) || !string.Equals(inKw.Keyword, "in", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (!(args[1] is CssKeywordValue csKw))
            {
                return false;
            }

            string interpolationSpace = csKw.Keyword.ToLowerInvariant();

            // Parse color1 [percentage1], color2 [percentage2]
            int idx = 2;
            if (!TryExtractMixColor(args, ref idx, out var c1, out float p1))
            {
                return false;
            }
            if (!TryExtractMixColor(args, ref idx, out var c2, out float p2))
            {
                return false;
            }

            // [CSS-COLOR5 §2.1] Normalize percentages
            if (float.IsNaN(p1) && float.IsNaN(p2))
            {
                p1 = 0.5f;
                p2 = 0.5f;
            }
            else if (float.IsNaN(p1))
            {
                p1 = 1f - p2;
            }
            else if (float.IsNaN(p2))
            {
                p2 = 1f - p1;
            }

            // [CSS-COLOR5 §2.1] If sum > 100%, normalize
            float sum = p1 + p2;
            if (sum > 0f && sum != 1f)
            {
                p1 /= sum;
                p2 /= sum;
            }

            color = MixInSpace(interpolationSpace, c1, p1, c2, p2);
            return true;
        }

        /// <summary>
        /// [CSS-COLOR5 §2.1] Mix two colors in the specified interpolation space.
        /// </summary>
        internal static CssColor MixInSpace(string space, CssColor c1, float p1, CssColor c2, float p2,
            string? hueMethod = null)
        {
            float alpha = c1.A / 255f * p1 + c2.A / 255f * p2;

            switch (space)
            {
                case "lab":
                {
                    SrgbToLab(c1, out float l1, out float a1, out float b1);
                    SrgbToLab(c2, out float l2, out float a2, out float b2);
                    return CssColor.FromLab(
                        l1 * p1 + l2 * p2,
                        a1 * p1 + a2 * p2,
                        b1 * p1 + b2 * p2,
                        alpha);
                }

                case "lch":
                {
                    SrgbToLch(c1, out float l1, out float ch1, out float h1);
                    SrgbToLch(c2, out float l2, out float ch2, out float h2);
                    float hue = InterpolateHue(h1, h2, p1, p2, hueMethod);
                    return CssColor.FromLch(
                        l1 * p1 + l2 * p2,
                        ch1 * p1 + ch2 * p2,
                        hue,
                        alpha);
                }

                case "oklab":
                {
                    SrgbToOklab(c1, out float l1, out float a1, out float b1);
                    SrgbToOklab(c2, out float l2, out float a2, out float b2);
                    return CssColor.FromOklab(
                        l1 * p1 + l2 * p2,
                        a1 * p1 + a2 * p2,
                        b1 * p1 + b2 * p2,
                        alpha);
                }

                case "oklch":
                {
                    SrgbToOklch(c1, out float l1, out float ch1, out float h1);
                    SrgbToOklch(c2, out float l2, out float ch2, out float h2);
                    float hue = InterpolateHue(h1, h2, p1, p2, hueMethod);
                    return CssColor.FromOklch(
                        l1 * p1 + l2 * p2,
                        ch1 * p1 + ch2 * p2,
                        hue,
                        alpha);
                }

                case "hsl":
                {
                    SrgbToHsl(c1, out float h1, out float s1, out float l1);
                    SrgbToHsl(c2, out float h2, out float s2, out float l2);
                    float hue = InterpolateHue(h1, h2, p1, p2, hueMethod);
                    return CssColor.FromHsl(
                        hue,
                        s1 * p1 + s2 * p2,
                        l1 * p1 + l2 * p2,
                        alpha);
                }

                case "hwb":
                {
                    SrgbToHwb(c1, out float h1, out float w1, out float bk1);
                    SrgbToHwb(c2, out float h2, out float w2, out float bk2);
                    float hue = InterpolateHue(h1, h2, p1, p2, hueMethod);
                    return CssColor.FromHwb(
                        hue,
                        w1 * p1 + w2 * p2,
                        bk1 * p1 + bk2 * p2,
                        alpha);
                }

                case "srgb":
                default:
                {
                    return CssColor.Mix(c1, p1, c2, p2);
                }
            }
        }

        /// <summary>
        /// [CSS-COLOR4 §12.1] Dispatch hue interpolation by method name.
        /// </summary>
        private static float InterpolateHue(float h1, float h2, float p1, float p2, string? method)
        {
            switch (method)
            {
                case "longer":
                    return InterpolateHueLonger(h1, h2, p1, p2);
                case "increasing":
                    return InterpolateHueIncreasing(h1, h2, p1, p2);
                case "decreasing":
                    return InterpolateHueDecreasing(h1, h2, p1, p2);
                default:
                    return InterpolateHueShorter(h1, h2, p1, p2);
            }
        }

        /// <summary>
        /// [CSS-COLOR4 §12.1] Shorter hue interpolation.
        /// </summary>
        private static float InterpolateHueShorter(float h1, float h2, float p1, float p2)
        {
            float diff = h2 - h1;
            if (diff > 180f)
            {
                h1 += 360f;
            }
            else if (diff < -180f)
            {
                h2 += 360f;
            }
            float result = h1 * p1 + h2 * p2;
            return ((result % 360f) + 360f) % 360f;
        }

        /// <summary>
        /// [CSS-COLOR4 §12.1] Longer hue interpolation — takes the longer arc around the hue circle.
        /// </summary>
        private static float InterpolateHueLonger(float h1, float h2, float p1, float p2)
        {
            float diff = h2 - h1;
            if (diff > 0f && diff < 180f)
            {
                h1 += 360f;
            }
            else if (diff > -180f && diff <= 0f)
            {
                h2 += 360f;
            }
            float result = h1 * p1 + h2 * p2;
            return ((result % 360f) + 360f) % 360f;
        }

        /// <summary>
        /// [CSS-COLOR4 §12.1] Increasing hue interpolation — hue always increases.
        /// </summary>
        private static float InterpolateHueIncreasing(float h1, float h2, float p1, float p2)
        {
            if (h2 < h1)
            {
                h2 += 360f;
            }
            float result = h1 * p1 + h2 * p2;
            return ((result % 360f) + 360f) % 360f;
        }

        /// <summary>
        /// [CSS-COLOR4 §12.1] Decreasing hue interpolation — hue always decreases.
        /// </summary>
        private static float InterpolateHueDecreasing(float h1, float h2, float p1, float p2)
        {
            if (h1 < h2)
            {
                h1 += 360f;
            }
            float result = h1 * p1 + h2 * p2;
            return ((result % 360f) + 360f) % 360f;
        }

        /// <summary>
        /// Try to parse color() function.
        /// [CSS-COLOR4 §10.1] color(colorspace c1 c2 c3 / alpha)
        /// Supports srgb, srgb-linear, display-p3. Other spaces clamp to sRGB gamut.
        /// </summary>
        public static bool TryParseColorFunction(List<CssValue> args, out CssColor color)
        {
            color = default;
            if (args.Count < 4) return false;

            if (!(args[0] is CssKeywordValue csKw)) return false;
            string colorSpace = csKw.Keyword.ToLowerInvariant();

            if (!TryGetFloatValue(args[1], out float channel1)) return false;
            if (!TryGetFloatValue(args[2], out float channel2)) return false;
            if (!TryGetFloatValue(args[3], out float channel3)) return false;

            float alpha = 1f;
            if (args.Count >= 5)
            {
                if (!TryGetAlpha(args[4], out alpha)) return false;
            }

            byte alphaByte = (byte)Math.Round(Math.Max(0f, Math.Min(1f, alpha)) * 255f);

            switch (colorSpace)
            {
                case "srgb-linear":
                {
                    // [CSS-COLOR4 §10.1] Linear-light sRGB to gamma-encoded sRGB
                    color = new CssColor(
                        LinearToSrgbByte(channel1),
                        LinearToSrgbByte(channel2),
                        LinearToSrgbByte(channel3),
                        alphaByte);
                    return true;
                }

                case "display-p3":
                {
                    // [CSS-COLOR4 §10.1] Display P3 to sRGB via linear-light conversion
                    // 1. Undo display-p3 gamma to get linear display-p3
                    float linearR = SrgbToLinear(channel1);
                    float linearG = SrgbToLinear(channel2);
                    float linearB = SrgbToLinear(channel3);

                    // 2. Linear display-p3 → XYZ(D65) → linear sRGB
                    color = LinearDisplayP3ToSrgb(linearR, linearG, linearB, alphaByte);
                    return true;
                }

                case "display-p3-linear":
                {
                    // [CSS-COLOR4 §10.1] Already linear — skip gamma undo
                    color = LinearDisplayP3ToSrgb(channel1, channel2, channel3, alphaByte);
                    return true;
                }

                case "a98-rgb":
                {
                    // [CSS-COLOR4 §10.1] Adobe RGB 1998 to sRGB
                    // 1. Undo a98-rgb gamma (256/563) — sign-preserving
                    float linearR = A98RgbToLinear(channel1);
                    float linearG = A98RgbToLinear(channel2);
                    float linearB = A98RgbToLinear(channel3);

                    // 2. Linear a98-rgb → XYZ(D65)
                    float x = 0.5766690429f * linearR + 0.1855582379f * linearG + 0.1882286462f * linearB;
                    float y = 0.2973449753f * linearR + 0.6273635663f * linearG + 0.0752914585f * linearB;
                    float z = 0.0270313614f * linearR + 0.0706888525f * linearG + 0.9913375368f * linearB;

                    // 3. XYZ(D65) → linear sRGB → gamma sRGB
                    color = XyzD65ToSrgb(x, y, z, alphaByte);
                    return true;
                }

                case "prophoto-rgb":
                {
                    // [CSS-COLOR4 §10.1] ProPhoto RGB to sRGB
                    // 1. Undo ProPhoto gamma (1/1.8, linear below 1/512)
                    float linearR = ProPhotoToLinear(channel1);
                    float linearG = ProPhotoToLinear(channel2);
                    float linearB = ProPhotoToLinear(channel3);

                    // 2. Linear ProPhoto → XYZ(D50)
                    float xD50 = 0.7977604896f * linearR + 0.1351917082f * linearG + 0.0313493495f * linearB;
                    float yD50 = 0.2880711282f * linearR + 0.7118432178f * linearG + 0.0000856540f * linearB;
                    float zD50 = 0.0000000000f * linearR + 0.0000000000f * linearG + 0.8251046026f * linearB;

                    // 3. D50 → D65 Bradford chromatic adaptation
                    float x = xD50 * 0.9555766f + yD50 * -0.0230393f + zD50 * 0.0631636f;
                    float y = xD50 * -0.0282895f + yD50 * 1.0099416f + zD50 * 0.0210077f;
                    float z = xD50 * 0.0122982f + yD50 * -0.0204830f + zD50 * 1.3299098f;

                    // 4. XYZ(D65) → linear sRGB → gamma sRGB
                    color = XyzD65ToSrgb(x, y, z, alphaByte);
                    return true;
                }

                case "rec2020":
                {
                    // [CSS-COLOR4 §10.1] Rec. 2020 to sRGB
                    // 1. Undo Rec. 2020 transfer function
                    float linearR = Rec2020ToLinear(channel1);
                    float linearG = Rec2020ToLinear(channel2);
                    float linearB = Rec2020ToLinear(channel3);

                    // 2. Linear Rec2020 → XYZ(D65)
                    float x = 0.6369580483f * linearR + 0.1446169036f * linearG + 0.1688809752f * linearB;
                    float y = 0.2627002120f * linearR + 0.6779980715f * linearG + 0.0593017165f * linearB;
                    float z = 0.0000000000f * linearR + 0.0280726930f * linearG + 1.0609850577f * linearB;

                    // 3. XYZ(D65) → linear sRGB → gamma sRGB
                    color = XyzD65ToSrgb(x, y, z, alphaByte);
                    return true;
                }

                case "xyz-d50":
                {
                    // [CSS-COLOR4 §10.1] XYZ with D50 white point
                    // 1. D50 → D65 Bradford chromatic adaptation
                    float x = channel1 * 0.9555766f + channel2 * -0.0230393f + channel3 * 0.0631636f;
                    float y = channel1 * -0.0282895f + channel2 * 1.0099416f + channel3 * 0.0210077f;
                    float z = channel1 * 0.0122982f + channel2 * -0.0204830f + channel3 * 1.3299098f;

                    // 2. XYZ(D65) → linear sRGB → gamma sRGB
                    color = XyzD65ToSrgb(x, y, z, alphaByte);
                    return true;
                }

                case "xyz-d65":
                case "xyz":
                {
                    // [CSS-COLOR4 §10.1] XYZ with D65 white point (xyz is alias for xyz-d65)
                    color = XyzD65ToSrgb(channel1, channel2, channel3, alphaByte);
                    return true;
                }

                case "srgb":
                {
                    color = new CssColor(
                        (byte)Math.Round(Math.Max(0f, Math.Min(1f, channel1)) * 255f),
                        (byte)Math.Round(Math.Max(0f, Math.Min(1f, channel2)) * 255f),
                        (byte)Math.Round(Math.Max(0f, Math.Min(1f, channel3)) * 255f),
                        alphaByte);
                    return true;
                }

                default:
                {
                    // Unknown color space — treat as sRGB (gamut clamp)
                    color = new CssColor(
                        (byte)Math.Round(Math.Max(0f, Math.Min(1f, channel1)) * 255f),
                        (byte)Math.Round(Math.Max(0f, Math.Min(1f, channel2)) * 255f),
                        (byte)Math.Round(Math.Max(0f, Math.Min(1f, channel3)) * 255f),
                        alphaByte);
                    return true;
                }
            }
        }

        /// <summary>
        /// [CSS-COLOR4 §13.1] Convert a linear-light sRGB component to gamma-encoded sRGB byte.
        /// </summary>
        private static byte LinearToSrgbByte(float linear)
        {
            float srgb;
            if (linear <= 0.0031308f)
            {
                srgb = 12.92f * linear;
            }
            else
            {
                srgb = 1.055f * (float)Math.Pow(linear, 1.0 / 2.4) - 0.055f;
            }
            return (byte)Math.Round(Math.Max(0f, Math.Min(1f, srgb)) * 255f);
        }

        /// <summary>
        /// [CSS-COLOR4 §13.1] Convert a gamma-encoded sRGB component to linear-light.
        /// </summary>
        private static float SrgbToLinear(float srgb)
        {
            srgb = Math.Max(0f, Math.Min(1f, srgb));
            if (srgb <= 0.04045f)
            {
                return srgb / 12.92f;
            }
            return (float)Math.Pow((srgb + 0.055) / 1.055, 2.4);
        }

        /// <summary>
        /// [CSS-COLOR4 §10.1] Convert linear-light Display P3 to gamma-encoded sRGB color.
        /// </summary>
        private static CssColor LinearDisplayP3ToSrgb(float linearR, float linearG, float linearB, byte alpha)
        {
            // Linear display-p3 → XYZ(D65)
            float x = 0.4865709486f * linearR + 0.2656676932f * linearG + 0.1982172852f * linearB;
            float y = 0.2289745641f * linearR + 0.6917385218f * linearG + 0.0792869141f * linearB;
            float z = 0.0000000000f * linearR + 0.0451133819f * linearG + 1.0439443689f * linearB;

            return XyzD65ToSrgb(x, y, z, alpha);
        }

        /// <summary>
        /// [CSS-COLOR4 §10.1] Convert XYZ(D65) to gamma-encoded sRGB color.
        /// </summary>
        private static CssColor XyzD65ToSrgb(float x, float y, float z, byte alpha)
        {
            float rl = x * 3.2404542f + y * -1.5371385f + z * -0.4985314f;
            float gl = x * -0.9692660f + y * 1.8760108f + z * 0.0415560f;
            float bl = x * 0.0556434f + y * -0.2040259f + z * 1.0572252f;

            return new CssColor(
                LinearToSrgbByte(rl),
                LinearToSrgbByte(gl),
                LinearToSrgbByte(bl),
                alpha);
        }

        /// <summary>
        /// [CSS-COLOR4 §10.1] Adobe RGB 1998 gamma decode (sign-preserving, exponent 563/256).
        /// </summary>
        private static float A98RgbToLinear(float value)
        {
            float sign = value < 0f ? -1f : 1f;
            return sign * (float)Math.Pow(Math.Abs(value), 563.0 / 256.0);
        }

        /// <summary>
        /// [CSS-COLOR4 §10.1] ProPhoto RGB gamma decode (linear below 1/512, else exponent 1.8).
        /// </summary>
        private static float ProPhotoToLinear(float value)
        {
            const float threshold = 16f / 512f;
            float sign = value < 0f ? -1f : 1f;
            float abs = Math.Abs(value);
            if (abs <= threshold)
            {
                return value / 16f;
            }
            return sign * (float)Math.Pow(abs, 1.8);
        }

        /// <summary>
        /// [CSS-COLOR4 §10.1] Rec. 2020 transfer function decode (BT.2020).
        /// </summary>
        private static float Rec2020ToLinear(float value)
        {
            const float alpha = 1.09929682680944f;
            const float beta = 0.018053968510807f;
            float sign = value < 0f ? -1f : 1f;
            float abs = Math.Abs(value);
            if (abs < beta * 4.5f)
            {
                return value / 4.5f;
            }
            return sign * (float)Math.Pow((abs + alpha - 1f) / alpha, 1.0 / 0.45);
        }

        /// <summary>
        /// Convert gamma-encoded sRGB byte to linear-light float.
        /// </summary>
        private static float SrgbByteToLinear(byte value)
        {
            float s = value / 255f;
            if (s <= 0.04045f)
            {
                return s / 12.92f;
            }
            return (float)Math.Pow((s + 0.055) / 1.055, 2.4);
        }

        /// <summary>
        /// Convert sRGB color to CIE Lab (D50) components.
        /// </summary>
        private static void SrgbToLab(CssColor color, out float labL, out float labA, out float labB)
        {
            // sRGB → linear sRGB
            float rl = SrgbByteToLinear(color.R);
            float gl = SrgbByteToLinear(color.G);
            float bl = SrgbByteToLinear(color.B);

            // linear sRGB → XYZ(D65)
            float x = 0.4124564f * rl + 0.3575761f * gl + 0.1804375f * bl;
            float y = 0.2126729f * rl + 0.7151522f * gl + 0.0721750f * bl;
            float z = 0.0193339f * rl + 0.1191920f * gl + 0.9503041f * bl;

            // XYZ(D65) → XYZ(D50) via Bradford
            float xD50 = 1.0479298208f * x + 0.0229456979f * y + -0.0501922219f * z;
            float yD50 = 0.0296278156f * x + 0.9904344267f * y + -0.0170737830f * z;
            float zD50 = -0.0092430581f * x + 0.0150551440f * y + 0.7521225698f * z;

            // XYZ(D50) → Lab
            float xn = xD50 / 0.96422f;
            float yn = yD50 / 1.0f;
            float zn = zD50 / 0.82521f;

            float fx = LabF(xn);
            float fy = LabF(yn);
            float fz = LabF(zn);

            labL = 116f * fy - 16f;
            labA = 500f * (fx - fy);
            labB = 200f * (fy - fz);
        }

        /// <summary>
        /// Lab forward transfer function f(t).
        /// </summary>
        private static float LabF(float t)
        {
            const float delta = 6f / 29f;
            if (t > delta * delta * delta)
            {
                return (float)Math.Pow(t, 1.0 / 3.0);
            }
            return t / (3f * delta * delta) + 4f / 29f;
        }

        /// <summary>
        /// Convert sRGB color to CIE LCH components.
        /// </summary>
        private static void SrgbToLch(CssColor color, out float lchL, out float lchC, out float lchH)
        {
            SrgbToLab(color, out lchL, out float a, out float b);
            lchC = (float)Math.Sqrt(a * a + b * b);
            lchH = (float)(Math.Atan2(b, a) * 180.0 / Math.PI);
            if (lchH < 0f)
            {
                lchH += 360f;
            }
        }

        /// <summary>
        /// Convert sRGB color to OKLab components.
        /// </summary>
        private static void SrgbToOklab(CssColor color, out float okL, out float okA, out float okB)
        {
            float rl = SrgbByteToLinear(color.R);
            float gl = SrgbByteToLinear(color.G);
            float bl = SrgbByteToLinear(color.B);

            // linear sRGB → LMS
            float l = 0.4122214708f * rl + 0.5363325363f * gl + 0.0514459929f * bl;
            float m = 0.2119034982f * rl + 0.6806995451f * gl + 0.1073969566f * bl;
            float s = 0.0883024619f * rl + 0.2817188376f * gl + 0.6299787005f * bl;

            // Cube root
            float lCbrt = (float)Math.Pow(l, 1.0 / 3.0);
            float mCbrt = (float)Math.Pow(m, 1.0 / 3.0);
            float sCbrt = (float)Math.Pow(s, 1.0 / 3.0);

            // LMS cube roots → OKLab
            okL = 0.2104542553f * lCbrt + 0.7936177850f * mCbrt - 0.0040720468f * sCbrt;
            okA = 1.9779984951f * lCbrt - 2.4285922050f * mCbrt + 0.4505937099f * sCbrt;
            okB = 0.0259040371f * lCbrt + 0.7827717662f * mCbrt - 0.8086757660f * sCbrt;
        }

        /// <summary>
        /// Convert sRGB color to OKLCH components.
        /// </summary>
        private static void SrgbToOklch(CssColor color, out float okL, out float okC, out float okH)
        {
            SrgbToOklab(color, out okL, out float a, out float b);
            okC = (float)Math.Sqrt(a * a + b * b);
            okH = (float)(Math.Atan2(b, a) * 180.0 / Math.PI);
            if (okH < 0f)
            {
                okH += 360f;
            }
        }

        /// <summary>
        /// Convert sRGB color to HSL components.
        /// </summary>
        private static void SrgbToHsl(CssColor color, out float h, out float s, out float l)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            l = (max + min) / 2f;

            if (delta < 0.0001f)
            {
                h = 0f;
                s = 0f;
                return;
            }

            s = l > 0.5f ? delta / (2f - max - min) : delta / (max + min);

            if (max == r)
            {
                h = ((g - b) / delta + (g < b ? 6f : 0f)) * 60f;
            }
            else if (max == g)
            {
                h = ((b - r) / delta + 2f) * 60f;
            }
            else
            {
                h = ((r - g) / delta + 4f) * 60f;
            }
        }

        /// <summary>
        /// Convert sRGB color to HWB components.
        /// </summary>
        private static void SrgbToHwb(CssColor color, out float h, out float w, out float bk)
        {
            SrgbToHsl(color, out h, out _, out _);
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            w = Math.Min(r, Math.Min(g, b));
            bk = 1f - Math.Max(r, Math.Max(g, b));
        }

        /// <summary>
        /// Try to parse a named color (e.g. "red", "transparent").
        /// </summary>
        public static bool TryParseNamed(string name, out CssColor color)
        {
            // Handle "transparent" specially
            if (string.Equals(name, "transparent", StringComparison.OrdinalIgnoreCase))
            {
                color = CssColor.Transparent;
                return true;
            }

            if (string.Equals(name, "currentcolor", StringComparison.OrdinalIgnoreCase))
            {
                // Special value — we return a sentinel. The cascade engine handles it.
                // Use a magic value: RGBA(0,0,0,0) won't collide because transparent is separate.
                // Actually, we'll handle currentColor at the cascade level, not here.
                // For now, return false and let the caller handle the keyword.
                color = default;
                return false;
            }

            return NamedColors.TryLookup(name, out color);
        }

        private static bool TryGetColorComponent(CssValue val, float maxVal, out float result)
        {
            result = 0;
            if (val is CssNumberValue n)
            {
                result = n.Value;
                return true;
            }
            if (val is CssPercentageValue p)
            {
                result = p.Value / 100f * maxVal;
                return true;
            }
            return false;
        }

        private static bool TryGetAlpha(CssValue val, out float alpha)
        {
            alpha = 1f;
            if (val is CssNumberValue n)
            {
                alpha = n.Value;
                return true;
            }
            if (val is CssPercentageValue p)
            {
                alpha = p.Value / 100f;
                return true;
            }
            return false;
        }

        private static float ConvertAngle(float value, string unit)
        {
            switch (unit.ToLowerInvariant())
            {
                case "deg": return value;
                case "rad": return value * (180f / (float)Math.PI);
                case "grad": return value * 0.9f;
                case "turn": return value * 360f;
                default: return value;
            }
        }

        private static byte ClampByte(float value)
        {
            return (byte)Math.Round(Math.Max(0f, Math.Min(255f, value)));
        }

        private static bool TryHex(char c, out int value)
        {
            if (c >= '0' && c <= '9') { value = c - '0'; return true; }
            if (c >= 'a' && c <= 'f') { value = c - 'a' + 10; return true; }
            if (c >= 'A' && c <= 'F') { value = c - 'A' + 10; return true; }
            value = 0;
            return false;
        }

        private static bool TryHex2(string s, int offset, out int value)
        {
            if (TryHex(s[offset], out int hi) && TryHex(s[offset + 1], out int lo))
            {
                value = hi * 16 + lo;
                return true;
            }
            value = 0;
            return false;
        }

        private static bool TryGetLabComponent(CssValue val, float maxVal, out float result)
        {
            result = 0;
            if (val is CssNumberValue n)
            {
                result = n.Value;
                return true;
            }
            if (val is CssPercentageValue p)
            {
                result = p.Value / 100f * maxVal;
                return true;
            }
            // [CSS-COLOR4 §4.1] 'none' keyword treated as 0
            if (val is CssKeywordValue kw && string.Equals(kw.Keyword, "none", StringComparison.OrdinalIgnoreCase))
            {
                result = 0f;
                return true;
            }
            return false;
        }

        private static bool TryGetFloatValue(CssValue val, out float result)
        {
            result = 0;
            if (val is CssNumberValue n) { result = n.Value; return true; }
            if (val is CssPercentageValue p) { result = p.Value / 100f; return true; }
            // [CSS-COLOR4 §4.1] 'none' keyword treated as 0
            if (val is CssKeywordValue kw && string.Equals(kw.Keyword, "none", StringComparison.OrdinalIgnoreCase))
            {
                result = 0f;
                return true;
            }
            return false;
        }

        private static bool TryExtractMixColor(List<CssValue> args, ref int idx, out CssColor color, out float pct)
        {
            color = default;
            pct = float.NaN;
            if (idx >= args.Count) return false;

            if (args[idx] is CssColorValue cv)
            {
                color = cv.Color;
                idx++;
            }
            else if (args[idx] is CssKeywordValue kw && TryParseNamed(kw.Keyword, out color))
            {
                idx++;
            }
            else return false;

            // Optional percentage after color
            if (idx < args.Count && args[idx] is CssPercentageValue pp)
            {
                pct = pp.Value / 100f;
                idx++;
            }

            return true;
        }
    }
}
