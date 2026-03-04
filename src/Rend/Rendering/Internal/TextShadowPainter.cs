using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Parser.Internal;
using Rend.Css.Properties.Internal;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints CSS text-shadow effects by drawing offset copies of text before the main text.
    /// Format: offset-x offset-y [blur-radius] [color]
    /// </summary>
    internal static class TextShadowPainter
    {
        /// <summary>
        /// Paints text shadows for a line fragment. Should be called before the main text is drawn.
        /// </summary>
        public static void Paint(LineFragment fragment, float drawX, float drawY,
            IRenderTarget target, ComputedStyle style)
        {
            object? rawValue = style.GetRefValue(PropertyId.TextShadow);
            if (rawValue == null)
            {
                return;
            }

            var shadows = ParseTextShadow(rawValue as CssValue);
            if (shadows == null || shadows.Count == 0)
            {
                return;
            }

            float fontSize = style.FontSize;
            string fontFamily = style.FontFamily;
            CssFontStyle fontStyle = style.FontStyle;
            float fontWeight = style.FontWeight;

            // Draw shadows in reverse order (first shadow is topmost per CSS spec).
            for (int i = shadows.Count - 1; i >= 0; i--)
            {
                var shadow = shadows[i];
                float shadowX = drawX + shadow.OffsetX;
                float shadowY = drawY + shadow.OffsetY;

                // For blur, we approximate by drawing with reduced opacity.
                CssColor shadowColor = shadow.Color;
                if (shadow.Blur > 0)
                {
                    float alphaFactor = 1f / (1f + shadow.Blur * 0.25f);
                    shadowColor = new CssColor(shadowColor.R, shadowColor.G, shadowColor.B,
                        (byte)(shadowColor.A * alphaFactor));
                }

                string? text = fragment.ShapedRun?.OriginalText ?? fragment.Text;
                if (text == null) continue;

                var textStyle = new TextStyle
                {
                    Font = new Fonts.FontDescriptor(fontFamily, fontWeight, fontStyle, Fonts.FontDescriptor.StretchToPercentage(style.FontStretch)),
                    FontSize = fontSize,
                    Color = shadowColor,
                    Bold = fontWeight >= 700f,
                    Italic = fontStyle == CssFontStyle.Italic || fontStyle == CssFontStyle.Oblique,
                    FontData = fragment.ShapedRun?.FontData
                };
                target.DrawText(text, shadowX, shadowY, textStyle);
            }
        }

        private static List<TextShadowLayer>? ParseTextShadow(CssValue? value)
        {
            if (value == null) return null;
            if (value is CssKeywordValue kw && kw.Keyword == "none") return null;

            var result = new List<TextShadowLayer>();

            if (value is CssListValue list && list.Separator == ',')
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    var layer = ParseSingleShadow(list.Values[i]);
                    if (layer.HasValue) result.Add(layer.Value);
                }
            }
            else
            {
                var layer = ParseSingleShadow(value);
                if (layer.HasValue) result.Add(layer.Value);
            }

            return result;
        }

        private static TextShadowLayer? ParseSingleShadow(CssValue value)
        {
            IReadOnlyList<CssValue> parts;
            if (value is CssListValue spaceList && spaceList.Separator == ' ')
            {
                parts = spaceList.Values;
            }
            else
            {
                parts = new[] { value };
            }

            CssColor? color = null;
            var lengths = new List<float>(3);

            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if (part is CssKeywordValue kwp)
                {
                    if (NamedColors.TryLookup(kwp.Keyword, out var namedColor))
                        color = namedColor;
                }
                else if (part is CssDimensionValue dim)
                {
                    lengths.Add(ResolveLength(dim));
                }
                else if (part is CssNumberValue num && num.Value == 0)
                {
                    lengths.Add(0);
                }
                else if (part is CssColorValue cv)
                {
                    color = cv.Color;
                }
                else if (part is CssFunctionValue fn)
                {
                    string fname = fn.Name.ToLowerInvariant();
                    if (fname == "rgb" || fname == "rgba")
                    {
                        var args = new List<CssValue>(fn.Arguments);
                        if (CssColorParser.TryParseRgb(args, out var rgbColor))
                            color = rgbColor;
                    }
                    else if (fname == "hsl" || fname == "hsla")
                    {
                        var args = new List<CssValue>(fn.Arguments);
                        if (CssColorParser.TryParseHsl(args, out var hslColor))
                            color = hslColor;
                    }
                }
            }

            if (lengths.Count < 2) return null;

            return new TextShadowLayer
            {
                OffsetX = lengths[0],
                OffsetY = lengths[1],
                Blur = lengths.Count > 2 ? lengths[2] : 0,
                Color = color ?? new CssColor(0, 0, 0, 255)
            };
        }

        private static float ResolveLength(CssDimensionValue dim)
        {
            switch (dim.Unit)
            {
                case "px": return dim.Value;
                case "pt": return dim.Value * 96f / 72f;
                case "in": return dim.Value * 96f;
                case "cm": return dim.Value * 96f / 2.54f;
                case "mm": return dim.Value * 96f / 25.4f;
                default: return dim.Value;
            }
        }

        private struct TextShadowLayer
        {
            public float OffsetX;
            public float OffsetY;
            public float Blur;
            public CssColor Color;
        }
    }
}
