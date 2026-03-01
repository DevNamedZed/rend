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
    /// Blur is approximated by rendering multiple translucent layers.
    /// </summary>
    internal static class BoxShadowPainter
    {
        /// <summary>
        /// Paints box-shadow for the given box onto the render target.
        /// </summary>
        public static void Paint(LayoutBox box, IRenderTarget target)
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
            float tlr = style.BorderTopLeftRadius;
            float trr = style.BorderTopRightRadius;
            float brr = style.BorderBottomRightRadius;
            float blr = style.BorderBottomLeftRadius;
            bool hasRadius = tlr > 0f || trr > 0f || brr > 0f || blr > 0f;

            // Draw shadows in reverse order (first shadow is topmost per CSS spec).
            for (int i = shadows.Count - 1; i >= 0; i--)
            {
                var shadow = shadows[i];

                if (shadow.Inset)
                {
                    PaintInsetShadow(shadow, borderRect, hasRadius, tlr, trr, brr, blr, target);
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
                    // Approximate blur by drawing multiple translucent layers
                    // at expanding offsets. Uses 4 layers for a reasonable approximation.
                    int layers = 4;
                    float step = shadow.Blur / layers;
                    float baseAlpha = shadow.Color.A / 255f;

                    for (int j = layers; j >= 1; j--)
                    {
                        float expand = step * j;
                        float layerAlpha = baseAlpha / (j + 1);
                        var layerColor = new CssColor(shadow.Color.R, shadow.Color.G, shadow.Color.B,
                                                       (byte)(layerAlpha * 255));
                        var layerRect = new RectF(x - expand, y - expand, w + expand * 2, h + expand * 2);
                        var brush = BrushInfo.Solid(layerColor);

                        if (hasRadius)
                        {
                            var path = new PathData();
                            path.AddRoundedRectangle(layerRect, tlr + expand, trr + expand,
                                                      brr + expand, blr + expand);
                            target.FillPath(path, brush);
                        }
                        else
                        {
                            target.FillRect(layerRect, brush);
                        }
                    }
                }
                else
                {
                    // Sharp shadow (no blur)
                    var shadowRect = new RectF(x, y, w, h);
                    var brush = BrushInfo.Solid(shadow.Color);

                    if (hasRadius)
                    {
                        var path = new PathData();
                        path.AddRoundedRectangle(shadowRect, tlr, trr, brr, blr);
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
            bool hasRadius, float tlr, float trr, float brr, float blr, IRenderTarget target)
        {
            // Inset shadow: render inside the border box.
            // The shadow is drawn as a filled region clipped to the border rect.
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
                    clipPath.AddRoundedRectangle(borderRect, tlr, trr, brr, blr);
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
                clipPath.AddRoundedRectangle(borderRect, tlr, trr, brr, blr);
                target.PushClipPath(clipPath);
            }
            else
            {
                target.PushClipRect(borderRect);
            }

            if (shadow.Blur > 0)
            {
                // Approximate inset blur with multiple translucent layers
                int layers = 4;
                float step = shadow.Blur / layers;
                float baseAlpha = shadow.Color.A / 255f;

                for (int j = layers; j >= 1; j--)
                {
                    float shrink = step * j;
                    float layerAlpha = baseAlpha / (j + 1);
                    var layerColor = new CssColor(shadow.Color.R, shadow.Color.G, shadow.Color.B,
                                                   (byte)(layerAlpha * 255));

                    // Draw the 4 edge strips of the inset shadow
                    var shrunkInner = new RectF(
                        innerRect.X + shrink,
                        innerRect.Y + shrink,
                        innerRect.Width - shrink * 2,
                        innerRect.Height - shrink * 2);
                    PaintInsetStrips(target, borderRect, shrunkInner, layerColor);
                }
            }
            else
            {
                // Sharp inset shadow — draw the 4 edge strips
                PaintInsetStrips(target, borderRect, innerRect, shadow.Color);
            }

            target.PopClip();
            target.Restore();
        }

        private static void PaintInsetStrips(IRenderTarget target, RectF outer, RectF inner, CssColor color)
        {
            var brush = BrushInfo.Solid(color);

            // Top strip
            if (inner.Y > outer.Y)
            {
                target.FillRect(new RectF(outer.X, outer.Y, outer.Width, inner.Y - outer.Y), brush);
            }

            // Bottom strip
            float innerBottom = inner.Y + inner.Height;
            float outerBottom = outer.Y + outer.Height;
            if (innerBottom < outerBottom)
            {
                target.FillRect(new RectF(outer.X, innerBottom, outer.Width, outerBottom - innerBottom), brush);
            }

            // Left strip (between top and bottom strips)
            if (inner.X > outer.X)
            {
                float stripTop = System.Math.Max(outer.Y, inner.Y);
                float stripBottom = System.Math.Min(outerBottom, innerBottom);
                if (stripBottom > stripTop)
                {
                    target.FillRect(new RectF(outer.X, stripTop, inner.X - outer.X, stripBottom - stripTop), brush);
                }
            }

            // Right strip (between top and bottom strips)
            float innerRight = inner.X + inner.Width;
            float outerRight = outer.X + outer.Width;
            if (innerRight < outerRight)
            {
                float stripTop = System.Math.Max(outer.Y, inner.Y);
                float stripBottom = System.Math.Min(outerBottom, innerBottom);
                if (stripBottom > stripTop)
                {
                    target.FillRect(new RectF(innerRight, stripTop, outerRight - innerRight, stripBottom - stripTop), brush);
                }
            }
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
