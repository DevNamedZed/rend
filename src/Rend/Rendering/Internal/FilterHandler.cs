using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Handles CSS filter property by extracting filter functions and applying them
    /// via the render target's compositing layer.
    /// </summary>
    internal static class FilterHandler
    {
        public static bool Apply(LayoutBox box, IRenderTarget target)
        {
            if (box.StyledNode?.Style == null)
                return false;

            var raw = box.StyledNode.Style.GetRefValue(PropertyId.Filter);
            if (raw == null)
                return false;

            if (raw is CssKeywordValue kw && kw.Keyword == "none")
                return false;

            var effects = ExtractEffects(raw);
            if (effects == null || effects.Length == 0)
                return false;

            target.Save();
            target.ApplyFilter(effects);
            return true;
        }

        public static void Restore(IRenderTarget target)
        {
            target.Restore();
        }

        internal static CssFilterEffect[]? ExtractEffects(object raw)
        {
            var list = new List<CssFilterEffect>();

            if (raw is CssFunctionValue fn)
            {
                var effect = ParseFunction(fn);
                if (effect.HasValue)
                    list.Add(effect.Value);
            }
            else if (raw is CssListValue csslist)
            {
                for (int i = 0; i < csslist.Values.Count; i++)
                {
                    if (csslist.Values[i] is CssFunctionValue f)
                    {
                        var effect = ParseFunction(f);
                        if (effect.HasValue)
                            list.Add(effect.Value);
                    }
                }
            }

            return list.Count > 0 ? list.ToArray() : null;
        }

        private static CssFilterEffect? ParseFunction(CssFunctionValue fn)
        {
            switch (fn.Name)
            {
                case "blur":
                    return new CssFilterEffect
                    {
                        Type = CssFilterType.Blur,
                        Amount = GetLengthAmount(fn)
                    };
                case "brightness":
                    return new CssFilterEffect
                    {
                        Type = CssFilterType.Brightness,
                        Amount = GetAmount(fn, 1f)
                    };
                case "contrast":
                    return new CssFilterEffect
                    {
                        Type = CssFilterType.Contrast,
                        Amount = GetAmount(fn, 1f)
                    };
                case "grayscale":
                    return new CssFilterEffect
                    {
                        Type = CssFilterType.Grayscale,
                        Amount = GetAmount(fn, 1f)
                    };
                case "sepia":
                    return new CssFilterEffect
                    {
                        Type = CssFilterType.Sepia,
                        Amount = GetAmount(fn, 1f)
                    };
                case "saturate":
                    return new CssFilterEffect
                    {
                        Type = CssFilterType.Saturate,
                        Amount = GetAmount(fn, 1f)
                    };
                case "hue-rotate":
                    return new CssFilterEffect
                    {
                        Type = CssFilterType.HueRotate,
                        Amount = GetAngleAmount(fn)
                    };
                case "invert":
                    return new CssFilterEffect
                    {
                        Type = CssFilterType.Invert,
                        Amount = GetAmount(fn, 1f)
                    };
                case "opacity":
                    return new CssFilterEffect
                    {
                        Type = CssFilterType.Opacity,
                        Amount = GetAmount(fn, 1f)
                    };
                case "drop-shadow":
                    return ParseDropShadow(fn);
                default:
                    return null;
            }
        }

        private static CssFilterEffect? ParseDropShadow(CssFunctionValue fn)
        {
            // drop-shadow(offsetX offsetY [blur] [color])
            float offsetX = 0, offsetY = 0, blur = 0;
            CssColor color = CssColor.Black;
            int lengthIdx = 0;

            for (int i = 0; i < fn.Arguments.Count; i++)
            {
                var arg = fn.Arguments[i];
                if (arg is CssDimensionValue dim)
                {
                    float px = dim.Value;
                    if (dim.Unit == "em") px *= 16;
                    if (lengthIdx == 0) offsetX = px;
                    else if (lengthIdx == 1) offsetY = px;
                    else if (lengthIdx == 2) blur = px;
                    lengthIdx++;
                }
                else if (arg is CssNumberValue num && num.Value == 0)
                {
                    if (lengthIdx == 0) offsetX = 0;
                    else if (lengthIdx == 1) offsetY = 0;
                    else if (lengthIdx == 2) blur = 0;
                    lengthIdx++;
                }
                else if (arg is CssColorValue cv)
                {
                    color = cv.Color;
                }
            }

            return new CssFilterEffect
            {
                Type = CssFilterType.DropShadow,
                Amount = blur,
                OffsetX = offsetX,
                OffsetY = offsetY,
                Color = color
            };
        }

        /// <summary>Gets a unitless amount or percentage, defaulting if no args.</summary>
        private static float GetAmount(CssFunctionValue fn, float defaultValue)
        {
            if (fn.Arguments.Count == 0) return defaultValue;
            var arg = fn.Arguments[0];
            if (arg is CssPercentageValue pct) return pct.Value / 100f;
            if (arg is CssNumberValue num) return num.Value;
            if (arg is CssDimensionValue dim) return dim.Value;
            return defaultValue;
        }

        /// <summary>Gets a length in px.</summary>
        private static float GetLengthAmount(CssFunctionValue fn)
        {
            if (fn.Arguments.Count == 0) return 0;
            var arg = fn.Arguments[0];
            if (arg is CssDimensionValue dim)
            {
                if (dim.Unit == "px") return dim.Value;
                if (dim.Unit == "em") return dim.Value * 16f;
                if (dim.Unit == "rem") return dim.Value * 16f;
                return dim.Value; // assume px
            }
            if (arg is CssNumberValue num) return num.Value;
            return 0;
        }

        /// <summary>Gets an angle in degrees.</summary>
        private static float GetAngleAmount(CssFunctionValue fn)
        {
            if (fn.Arguments.Count == 0) return 0;
            var arg = fn.Arguments[0];
            if (arg is CssDimensionValue dim)
            {
                if (dim.Unit == "deg") return dim.Value;
                if (dim.Unit == "rad") return dim.Value * (180f / (float)Math.PI);
                if (dim.Unit == "turn") return dim.Value * 360f;
                if (dim.Unit == "grad") return dim.Value * 0.9f;
                return dim.Value;
            }
            if (arg is CssNumberValue num) return num.Value;
            return 0;
        }
    }
}
