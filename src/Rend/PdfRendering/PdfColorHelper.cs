#nullable enable
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Rend.PdfRendering
{
    internal static class PdfColorHelper
    {
        public static SKColor GrayToColor(float gray)
        {
            byte value = ClampByte(gray * 255f);
            return new SKColor(value, value, value);
        }

        public static SKColor RgbToColor(float red, float green, float blue)
        {
            return new SKColor(ClampByte(red * 255f), ClampByte(green * 255f), ClampByte(blue * 255f));
        }

        public static SKColor CmykToColor(float cyan, float magenta, float yellow, float key)
        {
            float red = (1 - cyan) * (1 - key);
            float green = (1 - magenta) * (1 - key);
            float blue = (1 - yellow) * (1 - key);
            return new SKColor(ClampByte(red * 255f), ClampByte(green * 255f), ClampByte(blue * 255f));
        }

        public static SKColor WithAlpha(SKColor color, float alpha)
        {
            if (alpha >= 1f)
            {
                return color;
            }
            return new SKColor(color.Red, color.Green, color.Blue, ClampByte(alpha * 255f));
        }

        public static byte ClampByte(float value)
        {
            int integer = (int)(value + 0.5f);
            if (integer < 0)
            {
                return 0;
            }
            if (integer > 255)
            {
                return 255;
            }
            return (byte)integer;
        }

        public static void OpSetColor(GraphicsState state, List<object> args, bool isFill)
        {
            string colorSpace = isFill ? state.FillColorSpace : state.StrokeColorSpace;
            string separationName = isFill ? state.FillSeparationName : state.StrokeSeparationName;

            var numArgs = args.Where(a => a is double).Select(a => (float)(double)a).ToList();

            SKColor color;
            if (colorSpace.StartsWith("Separation:"))
            {
                // Separation color space: tint value 0=none, 1=full ink
                float tint = numArgs.Count > 0 ? numArgs[0] : 0;
                color = SeparationToColor(separationName, tint);
            }
            else if (colorSpace == "DeviceCMYK" || colorSpace == "CMYK" || numArgs.Count == 4)
            {
                color = CmykToColor(
                    numArgs.Count > 0 ? numArgs[0] : 0,
                    numArgs.Count > 1 ? numArgs[1] : 0,
                    numArgs.Count > 2 ? numArgs[2] : 0,
                    numArgs.Count > 3 ? numArgs[3] : 0);
            }
            else if (colorSpace == "DeviceGray" || colorSpace == "Gray" || colorSpace == "CalGray" || numArgs.Count == 1)
            {
                color = GrayToColor(numArgs.Count > 0 ? numArgs[0] : 0);
            }
            else
            {
                color = RgbToColor(
                    numArgs.Count > 0 ? numArgs[0] : 0,
                    numArgs.Count > 1 ? numArgs[1] : 0,
                    numArgs.Count > 2 ? numArgs[2] : 0);
            }

            if (isFill)
            {
                state.FillColor = color;
            }
            else
            {
                state.StrokeColor = color;
            }
        }

        public static SKColor SeparationToColor(string colorantName, float tint)
        {
            // [SPEC §8.6.6.4] Separation color space: tint 0=no ink, 1=full ink
            // Approximate common colorant names to RGB
            switch (colorantName.ToLowerInvariant())
            {
                case "black":
                case "all":
                    byte gray = ClampByte((1f - tint) * 255f);
                    return new SKColor(gray, gray, gray);
                case "cyan":
                    return CmykToColor(tint, 0, 0, 0);
                case "magenta":
                    return CmykToColor(0, tint, 0, 0);
                case "yellow":
                    return CmykToColor(0, 0, tint, 0);
                case "red":
                    return new SKColor(ClampByte(tint * 255f), 0, 0);
                case "green":
                    return new SKColor(0, ClampByte(tint * 255f), 0);
                case "blue":
                    return new SKColor(0, 0, ClampByte(tint * 255f));
                case "none":
                    return SKColors.Transparent;
                default:
                    // Unknown spot color — approximate as gray
                    byte defaultGray = ClampByte((1f - tint) * 255f);
                    return new SKColor(defaultGray, defaultGray, defaultGray);
            }
        }

        public static SKBlendMode GetSkBlendMode(string mode)
        {
            switch (mode)
            {
                case "Normal":
                case "Compatible": return SKBlendMode.SrcOver;
                case "Multiply": return SKBlendMode.Multiply;
                case "Screen": return SKBlendMode.Screen;
                case "Overlay": return SKBlendMode.Overlay;
                case "Darken": return SKBlendMode.Darken;
                case "Lighten": return SKBlendMode.Lighten;
                case "ColorDodge": return SKBlendMode.ColorDodge;
                case "ColorBurn": return SKBlendMode.ColorBurn;
                case "HardLight": return SKBlendMode.HardLight;
                case "SoftLight": return SKBlendMode.SoftLight;
                case "Difference": return SKBlendMode.Difference;
                case "Exclusion": return SKBlendMode.Exclusion;
                case "Hue": return SKBlendMode.Hue;
                case "Saturation": return SKBlendMode.Saturation;
                case "Color": return SKBlendMode.Color;
                case "Luminosity": return SKBlendMode.Luminosity;
                default: return SKBlendMode.SrcOver;
            }
        }
    }
}
