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

            float h;
            if (args[2] is CssNumberValue hn)
                h = hn.Value;
            else if (args[2] is CssDimensionValue hd)
                h = ConvertAngle(hd.Value, hd.Unit);
            else return false;

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

            float h;
            if (args[2] is CssNumberValue hn)
                h = hn.Value;
            else if (args[2] is CssDimensionValue hd)
                h = ConvertAngle(hd.Value, hd.Unit);
            else return false;

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
                return false;
            if (!(args[1] is CssKeywordValue csKw))
                return false;
            // We support srgb mixing; other spaces just fall through to srgb
            // Parse color1 [percentage1], color2 [percentage2]
            int idx = 2;
            if (!TryExtractMixColor(args, ref idx, out var c1, out float p1)) return false;
            if (!TryExtractMixColor(args, ref idx, out var c2, out float p2)) return false;

            // Normalize percentages per spec
            if (float.IsNaN(p1) && float.IsNaN(p2)) { p1 = 0.5f; p2 = 0.5f; }
            else if (float.IsNaN(p1)) { p1 = 1f - p2; }
            else if (float.IsNaN(p2)) { p2 = 1f - p1; }

            color = CssColor.Mix(c1, p1, c2, p2);
            return true;
        }

        /// <summary>
        /// Try to parse color() function.
        /// color(srgb r g b / a) or color(display-p3 r g b / a)
        /// </summary>
        public static bool TryParseColorFunction(List<CssValue> args, out CssColor color)
        {
            color = default;
            if (args.Count < 4) return false;

            // First arg is color space name
            if (!(args[0] is CssKeywordValue csKw)) return false;
            // We treat all spaces as sRGB (clamp to gamut)

            if (!TryGetFloatValue(args[1], out float r)) return false;
            if (!TryGetFloatValue(args[2], out float g)) return false;
            if (!TryGetFloatValue(args[3], out float b)) return false;

            float a = 1f;
            if (args.Count >= 5)
            {
                if (!TryGetAlpha(args[4], out a)) return false;
            }

            color = new CssColor(
                (byte)Math.Round(Math.Max(0f, Math.Min(1f, r)) * 255f),
                (byte)Math.Round(Math.Max(0f, Math.Min(1f, g)) * 255f),
                (byte)Math.Round(Math.Max(0f, Math.Min(1f, b)) * 255f),
                (byte)Math.Round(Math.Max(0f, Math.Min(1f, a)) * 255f));
            return true;
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
            return false;
        }

        private static bool TryGetFloatValue(CssValue val, out float result)
        {
            result = 0;
            if (val is CssNumberValue n) { result = n.Value; return true; }
            if (val is CssPercentageValue p) { result = p.Value / 100f; return true; }
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
