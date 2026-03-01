using System;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Handles CSS backdrop-filter property. Supports opacity() via render target opacity.
    /// Other backdrop-filter functions (blur, brightness, etc.) require compositor support
    /// and gracefully degrade in static PDF/image output.
    /// </summary>
    internal static class BackdropFilterHandler
    {
        public static bool Apply(LayoutBox box, IRenderTarget target)
        {
            if (box.StyledNode?.Style == null)
                return false;

            var raw = box.StyledNode.Style.GetRefValue(PropertyId.BackdropFilter);
            if (raw == null)
                return false;

            if (raw is CssKeywordValue kw && kw.Keyword == "none")
                return false;

            // Extract opacity filter value if present (same approach as filter)
            float opacity = ExtractOpacityFilter(raw);
            if (opacity < 1f)
            {
                target.Save();
                target.SetOpacity(Math.Max(0f, opacity));
                return true;
            }

            return false;
        }

        public static void Restore(IRenderTarget target)
        {
            target.Restore();
        }

        private static float ExtractOpacityFilter(object raw)
        {
            if (raw is CssFunctionValue fn && fn.Name == "opacity")
                return GetFilterAmount(fn);

            if (raw is CssListValue list)
            {
                float result = 1f;
                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (list.Values[i] is CssFunctionValue f && f.Name == "opacity")
                        result *= GetFilterAmount(f);
                }
                return result;
            }

            return 1f;
        }

        private static float GetFilterAmount(CssFunctionValue fn)
        {
            if (fn.Arguments.Count == 0) return 1f;
            var arg = fn.Arguments[0];
            if (arg is CssPercentageValue pct) return pct.Value / 100f;
            if (arg is CssNumberValue num) return num.Value;
            if (arg is CssDimensionValue dim) return dim.Value;
            return 1f;
        }
    }
}
