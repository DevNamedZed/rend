using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Parser.Internal;
using Rend.Css.Properties.Internal;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints CSS box-shadow effects. Supports offset, spread, and color.
    /// Blur is rendered via Gaussian mask blur on the render target.
    /// </summary>
    internal static class BoxShadowPainter
    {
        /// <summary>
        /// Paints outer (non-inset) box-shadows for the given box.
        /// Called BEFORE painting the background per CSS painting order.
        /// </summary>
        public static void PaintOuter(LayoutBox box, IRenderTarget target)
        {
            PaintShadows(box, target, inset: false);
        }

        /// <summary>
        /// Paints inset box-shadows for the given box.
        /// Called AFTER painting the background per CSS painting order.
        /// </summary>
        public static void PaintInset(LayoutBox box, IRenderTarget target)
        {
            PaintShadows(box, target, inset: true);
        }

        /// <summary>
        /// Paints all box-shadows (both outer and inset). Legacy entry point.
        /// </summary>
        public static void Paint(LayoutBox box, IRenderTarget target)
        {
            PaintShadows(box, target, inset: null);
        }

        private static void PaintShadows(LayoutBox box, IRenderTarget target, bool? inset)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
            {
                return;
            }

            object? rawValue = style.GetRefValue(PropertyId.BoxShadow);
            if (rawValue == null)
            {
                return;
            }

            var shadows = ParseBoxShadow(rawValue as CssValue, style.FontSize);
            if (shadows == null || shadows.Count == 0)
            {
                return;
            }

            // [CSS-BACKGROUNDS-3 §7.1] Resolve currentColor for shadows with no explicit color.
            CssColor elementColor = style.Color;
            for (int si = 0; si < shadows.Count; si++)
            {
                if (shadows[si].UseCurrentColor)
                {
                    var shadow = shadows[si];
                    shadow.Color = elementColor;
                    shadow.UseCurrentColor = false;
                    shadows[si] = shadow;
                }
            }

            RectF borderRect = box.BorderRect;

            // Border radius for rounded shadows
            var radii = BorderRadiusResolver.Resolve(style, borderRect);
            bool hasRadius = radii.HasRadius;

            // Draw shadows in reverse order (first shadow is topmost per CSS spec).
            for (int i = shadows.Count - 1; i >= 0; i--)
            {
                var shadow = shadows[i];

                // Filter by inset/outer if specified
                if (inset.HasValue && shadow.Inset != inset.Value)
                {
                    continue;
                }

                if (shadow.Inset)
                {
                    PaintInsetShadow(shadow, borderRect, hasRadius, radii, target);
                    continue;
                }

                // Compute shadow rectangle: border rect + spread, offset.
                float x = borderRect.X + shadow.OffsetX - shadow.Spread;
                float y = borderRect.Y + shadow.OffsetY - shadow.Spread;
                float w = borderRect.Width + shadow.Spread * 2;
                float h = borderRect.Height + shadow.Spread * 2;

                if (w <= 0 || h <= 0)
                {
                    continue;
                }

                // [CSS-BACKGROUNDS-3 §7.3.1] Outer box-shadow is drawn outside
                // the border edge only — clipped inside the border-box.
                // Clip out the border box, then draw the shadow normally.
                var shadowRect = new RectF(x, y, w, h);
                var brush = BrushInfo.Solid(shadow.Color);

                // Create an exclusion clip: draw shadow everywhere EXCEPT inside border box.
                var clipPath = new PathData();
                // Outer: large area encompassing shadow + blur
                float clipExpand = shadow.Blur * 2 + shadow.Spread + 10;
                var clipOuter = new RectF(
                    Math.Min(shadowRect.X, borderRect.X) - clipExpand,
                    Math.Min(shadowRect.Y, borderRect.Y) - clipExpand,
                    Math.Max(shadowRect.Right, borderRect.Right) - Math.Min(shadowRect.X, borderRect.X) + clipExpand * 2,
                    Math.Max(shadowRect.Bottom, borderRect.Bottom) - Math.Min(shadowRect.Y, borderRect.Y) + clipExpand * 2);
                clipPath.MoveTo(clipOuter.X, clipOuter.Y);
                clipPath.LineTo(clipOuter.Right, clipOuter.Y);
                clipPath.LineTo(clipOuter.Right, clipOuter.Bottom);
                clipPath.LineTo(clipOuter.X, clipOuter.Bottom);
                clipPath.Close();
                // Inner cutout: border box
                if (hasRadius)
                {
                    radii.AddToPath(clipPath, borderRect);
                }
                else
                {
                    clipPath.MoveTo(borderRect.X, borderRect.Y);
                    clipPath.LineTo(borderRect.Right, borderRect.Y);
                    clipPath.LineTo(borderRect.Right, borderRect.Bottom);
                    clipPath.LineTo(borderRect.X, borderRect.Bottom);
                    clipPath.Close();
                }
                clipPath.FillType = PathFillType.EvenOdd;
                target.PushClipPath(clipPath);

                if (shadow.Blur > 0)
                {
                    float sigma = shadow.Blur / 2f;
                    target.SetMaskBlur(sigma);
                }

                if (hasRadius)
                {
                    var path = new PathData();
                    radii.AddToPath(path, shadowRect);
                    target.FillPath(path, brush);
                }
                else
                {
                    target.FillRect(shadowRect, brush);
                }

                if (shadow.Blur > 0)
                {
                    target.SetMaskBlur(0);
                }
                target.PopClip();
            }
        }

        private static void PaintInsetShadow(BoxShadowLayer shadow, RectF borderRect,
            bool hasRadius, BorderRadii radii, IRenderTarget target)
        {
            // Inset shadow: render inside the border box, ON TOP of the background.
            // The shadow area is the border rect contracted by spread, then offset.
            float innerX = borderRect.X + shadow.Spread + shadow.OffsetX;
            float innerY = borderRect.Y + shadow.Spread + shadow.OffsetY;
            float innerW = borderRect.Width - shadow.Spread * 2;
            float innerH = borderRect.Height - shadow.Spread * 2;

            if (innerW <= 0 || innerH <= 0)
            {
                // Spread is larger than the box — fill entirely with shadow color.
                target.Save();
                if (hasRadius)
                {
                    var clipPath = new PathData();
                    radii.AddToPath(clipPath, borderRect);
                    target.PushClipPath(clipPath);
                }
                else
                {
                    target.PushClipRect(borderRect);
                }
                target.FillRect(borderRect, BrushInfo.Solid(shadow.Color));
                target.PopClip();
                target.Restore();
                return;
            }

            var innerRect = new RectF(innerX, innerY, innerW, innerH);

            target.Save();
            // Clip to the border box
            if (hasRadius)
            {
                var clipPath = new PathData();
                radii.AddToPath(clipPath, borderRect);
                target.PushClipPath(clipPath);
            }
            else
            {
                target.PushClipRect(borderRect);
            }

            // Draw a frame path (large outer rect with inner rect hole) using EvenOdd fill.
            // The outer boundary is inflated well beyond the clip rect so its blur edges
            // are invisible — only the inner boundary's blur (extending into the box center)
            // is visible. This matches Chrome's inset shadow rendering approach.
            if (shadow.Blur > 0)
            {
                float sigma = shadow.Blur / 2f;
                target.SetMaskBlur(sigma);
            }

            // Inflate outer boundary by 3*blur so the outer edge blur is fully clipped away
            float inflate = shadow.Blur > 0 ? shadow.Blur * 3f : 0f;
            var outerRect = new RectF(
                borderRect.X - inflate, borderRect.Y - inflate,
                borderRect.Width + inflate * 2, borderRect.Height + inflate * 2);

            var framePath = new PathData();
            framePath.AddRectangle(outerRect);
            framePath.AddRectangle(innerRect);
            framePath.FillType = PathFillType.EvenOdd;
            target.FillPath(framePath, BrushInfo.Solid(shadow.Color));

            if (shadow.Blur > 0)
            {
                target.SetMaskBlur(0);
            }

            target.PopClip();
            target.Restore();
        }

        /// <summary>
        /// Parses a raw CssValue into a list of box-shadow layers.
        /// </summary>
        private static List<BoxShadowLayer>? ParseBoxShadow(CssValue? value, float fontSize = 16f)
        {
            if (value == null)
            {
                return null;
            }

            // box-shadow: none
            if (value is CssKeywordValue kw && kw.Keyword == "none")
            {
                return null;
            }

            var result = new List<BoxShadowLayer>();

            // Multiple shadows are comma-separated
            if (value is CssListValue list && list.Separator == ',')
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    var layer = ParseSingleShadow(list.Values[i], fontSize);
                    if (layer.HasValue)
                    {
                        result.Add(layer.Value);
                    }
                }
            }
            else
            {
                // Single shadow (space-separated or a single list)
                var layer = ParseSingleShadow(value, fontSize);
                if (layer.HasValue)
                {
                    result.Add(layer.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Parses a single box-shadow layer from a CssValue.
        /// Format: [inset] offset-x offset-y [blur [spread]] [color]
        /// </summary>
        private static BoxShadowLayer? ParseSingleShadow(CssValue value, float fontSize = 16f)
        {
            // A single shadow is a space-separated list of values
            IReadOnlyList<CssValue> parts;
            if (value is CssListValue spaceList && spaceList.Separator == ' ')
            {
                parts = spaceList.Values;
            }
            else
            {
                // Single value — not enough for a valid shadow
                parts = new[] { value };
            }

            bool inset = false;
            CssColor? color = null;
            var lengths = new List<float>(4);

            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];

                if (part is CssKeywordValue kwp)
                {
                    if (kwp.Keyword == "inset")
                    {
                        inset = true;
                    }
                    else if (NamedColors.TryLookup(kwp.Keyword, out var namedColor))
                    {
                        color = namedColor;
                    }
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
                    // [CSS-VALUES §8] calc/min/max/clamp functions → evaluate as length
                    if (fname == "calc" || fname == "min" || fname == "max" || fname == "clamp")
                    {
                        var ctx = new Core.Values.CssResolutionContext(
                            fontSize, fontSize, 0, 0, 0);
                        float calcResult = Css.Resolution.Internal.ValueResolver.EvaluateCalc(
                            fn.Arguments, ctx);
                        lengths.Add(calcResult);
                    }
                    else if (fname == "rgb" || fname == "rgba")
                    {
                        var args = new List<CssValue>(fn.Arguments);
                        if (Rend.Css.Parser.Internal.CssColorParser.TryParseRgb(args, out var rgbColor))
                        {
                            color = rgbColor;
                        }
                    }
                    else if (fname == "hsl" || fname == "hsla")
                    {
                        var args = new List<CssValue>(fn.Arguments);
                        if (Rend.Css.Parser.Internal.CssColorParser.TryParseHsl(args, out var hslColor))
                        {
                            color = hslColor;
                        }
                    }
                }
            }

            // Need at least offset-x and offset-y
            if (lengths.Count < 2)
            {
                return null;
            }

            return new BoxShadowLayer
            {
                OffsetX = lengths[0],
                OffsetY = lengths[1],
                Blur = lengths.Count > 2 ? lengths[2] : 0,
                Spread = lengths.Count > 3 ? lengths[3] : 0,
                // [CSS-BACKGROUNDS-3 §7.1] When color is omitted, it defaults to currentColor.
                // We use a sentinel (A=0, R=0, G=0, B=1) to indicate "resolve later".
                Color = color ?? CssColor.Transparent,
                UseCurrentColor = color == null,
                Inset = inset
            };
        }

        private static float ResolveLength(CssDimensionValue dim, float fontSize = 16f)
        {
            switch (dim.Unit)
            {
                case "px": return dim.Value;
                case "em": return dim.Value * fontSize;
                case "rem": return dim.Value * 16f;
                case "pt": return dim.Value * 96f / 72f;
                case "in": return dim.Value * 96f;
                case "cm": return dim.Value * 96f / 2.54f;
                case "mm": return dim.Value * 96f / 25.4f;
                default: return dim.Value;
            }
        }

        private struct BoxShadowLayer
        {
            public float OffsetX;
            public float OffsetY;
            public float Blur;
            public float Spread;
            public CssColor Color;
            public bool Inset;
            public bool UseCurrentColor;
        }
    }
}
