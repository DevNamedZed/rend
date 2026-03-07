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

            var shadows = ParseBoxShadow(rawValue as CssValue);
            if (shadows == null || shadows.Count == 0)
            {
                return;
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

                if (shadow.Blur > 0)
                {
                    // Use the render target's Gaussian blur mask for proper shadow rendering.
                    // CSS blur-radius maps to approximately sigma = blur / 2.
                    float sigma = shadow.Blur / 2f;
                    target.SetMaskBlur(sigma);

                    var brush = BrushInfo.Solid(shadow.Color);
                    if (hasRadius)
                    {
                        var path = new PathData();
                        radii.AddToPath(path, new RectF(x, y, w, h));
                        target.FillPath(path, brush);
                    }
                    else
                    {
                        target.FillRect(new RectF(x, y, w, h), brush);
                    }

                    target.SetMaskBlur(0);
                }
                else
                {
                    // Sharp shadow (no blur)
                    var shadowRect = new RectF(x, y, w, h);
                    var brush = BrushInfo.Solid(shadow.Color);

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
                }
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
        private static List<BoxShadowLayer>? ParseBoxShadow(CssValue? value)
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
                    var layer = ParseSingleShadow(list.Values[i]);
                    if (layer.HasValue)
                    {
                        result.Add(layer.Value);
                    }
                }
            }
            else
            {
                // Single shadow (space-separated or a single list)
                var layer = ParseSingleShadow(value);
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
        private static BoxShadowLayer? ParseSingleShadow(CssValue value)
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
                    // Try to parse as color function (rgb, rgba, hsl, hsla)
                    string fname = fn.Name.ToLowerInvariant();
                    if (fname == "rgb" || fname == "rgba")
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
                Color = color ?? new CssColor(0, 0, 0, 255), // default: black
                Inset = inset
            };
        }

        private static float ResolveLength(CssDimensionValue dim)
        {
            // For box-shadow, only px is common. Convert basic units.
            switch (dim.Unit)
            {
                case "px": return dim.Value;
                case "pt": return dim.Value * 96f / 72f;
                case "in": return dim.Value * 96f;
                case "cm": return dim.Value * 96f / 2.54f;
                case "mm": return dim.Value * 96f / 25.4f;
                default: return dim.Value; // assume px
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
        }
    }
}
