using System;
using System.Globalization;
using Rend.Core.Values;
using Rend.Css.Parser.Internal;
using Rend.Html;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Renders inline SVG elements by traversing the SVG DOM subtree
    /// and converting shapes, paths, text, and groups into IRenderTarget drawing calls.
    /// </summary>
    internal static class SvgRenderer
    {
        /// <summary>
        /// Render an SVG element into the given target at the specified content rect.
        /// </summary>
        public static void Render(Element svgElement, IRenderTarget target, RectF contentRect)
        {
            // Parse viewBox for coordinate mapping
            float vbX = 0, vbY = 0;
            float vbW = contentRect.Width, vbH = contentRect.Height;
            string? viewBox = svgElement.GetAttribute("viewbox") ?? svgElement.GetAttribute("viewBox");
            if (viewBox != null)
                ParseViewBox(viewBox, out vbX, out vbY, out vbW, out vbH);

            // Compute scale from viewBox to content rect
            float scaleX = vbW > 0 ? contentRect.Width / vbW : 1f;
            float scaleY = vbH > 0 ? contentRect.Height / vbH : 1f;

            target.Save();

            // Clip to content rect
            target.PushClipRect(contentRect);

            // Translate to content rect origin and scale from viewBox to content rect
            var transform = Matrix3x2.CreateScale(scaleX, scaleY) *
                            Matrix3x2.CreateTranslation(contentRect.X - vbX * scaleX, contentRect.Y - vbY * scaleY);
            target.SetTransform(transform);

            // Traverse children
            RenderChildren(svgElement, target);

            target.PopClip();
            target.Restore();
        }

        /// <summary>
        /// Parse the SVG viewBox attribute "minX minY width height".
        /// </summary>
        public static bool ParseViewBox(string viewBox, out float x, out float y, out float w, out float h)
        {
            x = y = w = h = 0;
            if (string.IsNullOrWhiteSpace(viewBox)) return false;

            // Split on whitespace and/or commas
            var parts = viewBox.Split(new[] { ' ', ',', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) return false;

            return TryParseFloat(parts[0], out x) &&
                   TryParseFloat(parts[1], out y) &&
                   TryParseFloat(parts[2], out w) &&
                   TryParseFloat(parts[3], out h);
        }

        private static void RenderChildren(Element parent, IRenderTarget target)
        {
            var child = parent.FirstChild;
            while (child != null)
            {
                if (child is Element elem)
                    RenderElement(elem, target);
                child = child.NextSibling;
            }
        }

        private static void RenderElement(Element elem, IRenderTarget target)
        {
            string tag = elem.TagName;

            // Skip <defs> — definitions are referenced by <use>, not rendered directly
            if (tag == "defs") return;

            // Parse common presentation attributes
            var fill = ParseColor(elem.GetAttribute("fill"), CssColor.Black);
            var stroke = ParseColor(elem.GetAttribute("stroke"), CssColor.Transparent);
            float strokeWidth = ParseAttrFloat(elem, "stroke-width", 1f);
            float opacity = ParseAttrFloat(elem, "opacity", 1f);
            bool hasFill = !IsNone(elem.GetAttribute("fill")) && fill.A > 0;
            bool hasStroke = !IsNone(elem.GetAttribute("stroke")) && stroke.A > 0 && strokeWidth > 0;
            float fillOpacity = ParseAttrFloat(elem, "fill-opacity", 1f);
            float strokeOpacity = ParseAttrFloat(elem, "stroke-opacity", 1f);

            // Handle transform
            string? transformAttr = elem.GetAttribute("transform");
            bool hasTransform = transformAttr != null;
            if (hasTransform || opacity < 1f)
                target.Save();

            if (opacity < 1f)
                target.SetOpacity(opacity);

            if (hasTransform)
            {
                var matrix = ParseTransform(transformAttr!);
                target.SetTransform(matrix);
            }

            switch (tag)
            {
                case "g":
                    RenderChildren(elem, target);
                    break;

                case "rect":
                    RenderRect(elem, target, fill, stroke, strokeWidth, hasFill, hasStroke, fillOpacity, strokeOpacity);
                    break;

                case "circle":
                    RenderCircle(elem, target, fill, stroke, strokeWidth, hasFill, hasStroke, fillOpacity, strokeOpacity);
                    break;

                case "ellipse":
                    RenderEllipse(elem, target, fill, stroke, strokeWidth, hasFill, hasStroke, fillOpacity, strokeOpacity);
                    break;

                case "line":
                    RenderLine(elem, target, stroke, strokeWidth, hasStroke, strokeOpacity);
                    break;

                case "polyline":
                    RenderPolyline(elem, target, fill, stroke, strokeWidth, hasFill, hasStroke, fillOpacity, strokeOpacity, false);
                    break;

                case "polygon":
                    RenderPolyline(elem, target, fill, stroke, strokeWidth, hasFill, hasStroke, fillOpacity, strokeOpacity, true);
                    break;

                case "path":
                    RenderPath(elem, target, fill, stroke, strokeWidth, hasFill, hasStroke, fillOpacity, strokeOpacity);
                    break;

                case "text":
                    RenderText(elem, target, fill, fillOpacity);
                    break;

                case "svg":
                    // Nested SVG — render children with potential viewport clipping
                    RenderChildren(elem, target);
                    break;

                case "use":
                    RenderUse(elem, target);
                    break;
            }

            if (hasTransform || opacity < 1f)
                target.Restore();
        }

        private static void RenderRect(Element elem, IRenderTarget target,
            CssColor fill, CssColor stroke, float strokeWidth,
            bool hasFill, bool hasStroke, float fillOpacity, float strokeOpacity)
        {
            float x = ParseAttrFloat(elem, "x", 0);
            float y = ParseAttrFloat(elem, "y", 0);
            float w = ParseAttrFloat(elem, "width", 0);
            float h = ParseAttrFloat(elem, "height", 0);
            float rx = ParseAttrFloat(elem, "rx", 0);
            float ry = ParseAttrFloat(elem, "ry", 0);
            if (w <= 0 || h <= 0) return;

            // If only one radius is specified, use it for both
            if (rx > 0 && ry == 0) ry = rx;
            if (ry > 0 && rx == 0) rx = ry;

            if (rx > 0 || ry > 0)
            {
                var path = new PathData();
                path.AddRoundedRectangle(new RectF(x, y, w, h), rx, rx, rx, rx);
                if (hasFill)
                    target.FillPath(path, BrushInfo.Solid(WithAlpha(fill, fillOpacity)));
                if (hasStroke)
                    target.StrokePath(path, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth));
            }
            else
            {
                var rect = new RectF(x, y, w, h);
                if (hasFill)
                    target.FillRect(rect, BrushInfo.Solid(WithAlpha(fill, fillOpacity)));
                if (hasStroke)
                    target.StrokeRect(rect, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth));
            }
        }

        private static void RenderCircle(Element elem, IRenderTarget target,
            CssColor fill, CssColor stroke, float strokeWidth,
            bool hasFill, bool hasStroke, float fillOpacity, float strokeOpacity)
        {
            float cx = ParseAttrFloat(elem, "cx", 0);
            float cy = ParseAttrFloat(elem, "cy", 0);
            float r = ParseAttrFloat(elem, "r", 0);
            if (r <= 0) return;

            var path = BuildEllipsePath(cx, cy, r, r);
            if (hasFill)
                target.FillPath(path, BrushInfo.Solid(WithAlpha(fill, fillOpacity)));
            if (hasStroke)
                target.StrokePath(path, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth));
        }

        private static void RenderEllipse(Element elem, IRenderTarget target,
            CssColor fill, CssColor stroke, float strokeWidth,
            bool hasFill, bool hasStroke, float fillOpacity, float strokeOpacity)
        {
            float cx = ParseAttrFloat(elem, "cx", 0);
            float cy = ParseAttrFloat(elem, "cy", 0);
            float rx = ParseAttrFloat(elem, "rx", 0);
            float ry = ParseAttrFloat(elem, "ry", 0);
            if (rx <= 0 || ry <= 0) return;

            var path = BuildEllipsePath(cx, cy, rx, ry);
            if (hasFill)
                target.FillPath(path, BrushInfo.Solid(WithAlpha(fill, fillOpacity)));
            if (hasStroke)
                target.StrokePath(path, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth));
        }

        private static void RenderLine(Element elem, IRenderTarget target,
            CssColor stroke, float strokeWidth, bool hasStroke, float strokeOpacity)
        {
            if (!hasStroke) return;
            float x1 = ParseAttrFloat(elem, "x1", 0);
            float y1 = ParseAttrFloat(elem, "y1", 0);
            float x2 = ParseAttrFloat(elem, "x2", 0);
            float y2 = ParseAttrFloat(elem, "y2", 0);

            var path = new PathData();
            path.MoveTo(x1, y1);
            path.LineTo(x2, y2);
            target.StrokePath(path, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth));
        }

        private static void RenderPolyline(Element elem, IRenderTarget target,
            CssColor fill, CssColor stroke, float strokeWidth,
            bool hasFill, bool hasStroke, float fillOpacity, float strokeOpacity, bool close)
        {
            string? points = elem.GetAttribute("points");
            if (string.IsNullOrWhiteSpace(points)) return;

            var path = ParsePoints(points!, close);
            if (hasFill)
                target.FillPath(path, BrushInfo.Solid(WithAlpha(fill, fillOpacity)));
            if (hasStroke)
                target.StrokePath(path, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth));
        }

        private static void RenderPath(Element elem, IRenderTarget target,
            CssColor fill, CssColor stroke, float strokeWidth,
            bool hasFill, bool hasStroke, float fillOpacity, float strokeOpacity)
        {
            string? d = elem.GetAttribute("d");
            if (string.IsNullOrWhiteSpace(d)) return;

            var path = SvgPathParser.Parse(d!);
            if (hasFill)
                target.FillPath(path, BrushInfo.Solid(WithAlpha(fill, fillOpacity)));
            if (hasStroke)
                target.StrokePath(path, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth));
        }

        private static void RenderText(Element elem, IRenderTarget target,
            CssColor fill, float fillOpacity)
        {
            float x = ParseAttrFloat(elem, "x", 0);
            float y = ParseAttrFloat(elem, "y", 0);
            float fontSize = ParseAttrFloat(elem, "font-size", 16f);
            string? fontFamily = elem.GetAttribute("font-family");
            string? fontWeightAttr = elem.GetAttribute("font-weight");
            string? fontStyleAttr = elem.GetAttribute("font-style");

            string text = elem.TextContent ?? "";
            if (string.IsNullOrEmpty(text)) return;

            // Resolve font weight (default 400 = normal)
            float fontWeight = 400f;
            bool bold = false;
            if (fontWeightAttr != null)
            {
                if (fontWeightAttr == "bold") { fontWeight = 700f; bold = true; }
                else if (fontWeightAttr == "normal") { fontWeight = 400f; }
                else if (TryParseFloat(fontWeightAttr, out float w)) { fontWeight = w; bold = w >= 600; }
            }

            // Resolve font style
            var cssFontStyle = Css.CssFontStyle.Normal;
            bool italic = false;
            if (fontStyleAttr == "italic") { cssFontStyle = Css.CssFontStyle.Italic; italic = true; }
            else if (fontStyleAttr == "oblique") { cssFontStyle = Css.CssFontStyle.Oblique; italic = true; }

            // Build font descriptor (default to sans-serif if no family specified)
            string family = fontFamily ?? "sans-serif";
            var fontDesc = new Fonts.FontDescriptor(family, fontWeight, cssFontStyle);

            var style = new TextStyle
            {
                FontSize = fontSize,
                Color = WithAlpha(fill, fillOpacity),
                Font = fontDesc,
                Bold = bold,
                Italic = italic,
            };

            target.DrawText(text, x, y, style);
        }

        private static void RenderUse(Element elem, IRenderTarget target)
        {
            // <use href="#id" x="..." y="..." />
            string? href = elem.GetAttribute("href") ?? elem.GetAttribute("xlink:href");
            if (href == null || !href.StartsWith("#")) return;

            string id = href.Substring(1);
            float x = ParseAttrFloat(elem, "x", 0);
            float y = ParseAttrFloat(elem, "y", 0);

            // Walk up to find the root SVG, then search for the referenced element
            var root = FindRoot(elem);
            if (root == null) return;
            var referenced = FindById(root, id);
            if (referenced == null) return;

            if (x != 0 || y != 0)
            {
                target.Save();
                target.SetTransform(Matrix3x2.CreateTranslation(x, y));
            }

            RenderElement(referenced, target);

            if (x != 0 || y != 0)
                target.Restore();
        }

        // ═══════════════════════════════════════════
        // Helper methods
        // ═══════════════════════════════════════════

        private static PathData BuildEllipsePath(float cx, float cy, float rx, float ry)
        {
            const float kappa = 0.5522847498f;
            float kx = rx * kappa;
            float ky = ry * kappa;

            var path = new PathData();
            path.MoveTo(cx + rx, cy);
            path.CubicBezierTo(cx + rx, cy + ky, cx + kx, cy + ry, cx, cy + ry);
            path.CubicBezierTo(cx - kx, cy + ry, cx - rx, cy + ky, cx - rx, cy);
            path.CubicBezierTo(cx - rx, cy - ky, cx - kx, cy - ry, cx, cy - ry);
            path.CubicBezierTo(cx + kx, cy - ry, cx + rx, cy - ky, cx + rx, cy);
            path.Close();
            return path;
        }

        private static PathData ParsePoints(string points, bool close)
        {
            var path = new PathData();
            var parts = points.Split(new[] { ' ', ',', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            bool first = true;

            for (int i = 0; i + 1 < parts.Length; i += 2)
            {
                if (!TryParseFloat(parts[i], out float x) || !TryParseFloat(parts[i + 1], out float y))
                    continue;

                if (first)
                {
                    path.MoveTo(x, y);
                    first = false;
                }
                else
                {
                    path.LineTo(x, y);
                }
            }

            if (close && !first)
                path.Close();

            return path;
        }

        private static CssColor ParseColor(string? value, CssColor defaultColor)
        {
            if (value == null || value == "inherit" || value == "currentColor")
                return defaultColor;
            if (value == "none" || value == "transparent")
                return CssColor.Transparent;

            // Hex color
            if (value.Length > 0 && value[0] == '#')
            {
                if (CssColorParser.TryParseHex(value.Substring(1), out var hexColor))
                    return hexColor;
            }

            // rgb() function
            if (value.StartsWith("rgb(") || value.StartsWith("rgba("))
            {
                // Simple inline parse: extract numbers
                return ParseRgbFunction(value);
            }

            // Named color
            if (CssColorParser.TryParseNamed(value, out var namedColor))
                return namedColor;

            return defaultColor;
        }

        private static CssColor ParseRgbFunction(string value)
        {
            // Extract content between ( and )
            int start = value.IndexOf('(');
            int end = value.LastIndexOf(')');
            if (start < 0 || end < 0) return CssColor.Black;

            string content = value.Substring(start + 1, end - start - 1);
            var parts = content.Split(new[] { ',', ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 3)
            {
                TryParseFloat(parts[0].Trim(), out float r);
                TryParseFloat(parts[1].Trim(), out float g);
                TryParseFloat(parts[2].Trim(), out float b);
                float a = 1f;
                if (parts.Length >= 4)
                    TryParseFloat(parts[3].Trim(), out a);

                return new CssColor(
                    (byte)Math.Max(0, Math.Min(255, (int)r)),
                    (byte)Math.Max(0, Math.Min(255, (int)g)),
                    (byte)Math.Max(0, Math.Min(255, (int)b)),
                    (byte)Math.Max(0, Math.Min(255, (int)(a <= 1f ? a * 255f : a))));
            }

            return CssColor.Black;
        }

        private static CssColor WithAlpha(CssColor color, float opacity)
        {
            if (opacity >= 1f) return color;
            byte a = (byte)(color.A * opacity);
            return new CssColor(color.R, color.G, color.B, a);
        }

        private static bool IsNone(string? value) => value == "none";

        private static float ParseAttrFloat(Element elem, string name, float defaultValue)
        {
            string? val = elem.GetAttribute(name);
            if (val == null) return defaultValue;

            // Strip "px" suffix if present
            if (val.EndsWith("px"))
                val = val.Substring(0, val.Length - 2);

            return TryParseFloat(val, out float result) ? result : defaultValue;
        }

        private static bool TryParseFloat(string s, out float result)
        {
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        /// <summary>
        /// Parse SVG transform attribute into a Matrix3x2.
        /// Supports: translate, rotate, scale, matrix, skewX, skewY.
        /// </summary>
        private static Matrix3x2 ParseTransform(string transform)
        {
            var result = Matrix3x2.Identity;
            int i = 0;
            int len = transform.Length;

            while (i < len)
            {
                SkipWhitespace(transform, ref i, len);
                if (i >= len) break;

                // Read function name
                int nameStart = i;
                while (i < len && transform[i] != '(')
                    i++;
                if (i >= len) break;

                string name = transform.Substring(nameStart, i - nameStart).Trim();
                i++; // skip '('

                // Read arguments until ')'
                int argsStart = i;
                while (i < len && transform[i] != ')')
                    i++;
                if (i >= len) break;

                string argsStr = transform.Substring(argsStart, i - argsStart);
                i++; // skip ')'

                var args = argsStr.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                float[] vals = new float[args.Length];
                for (int j = 0; j < args.Length; j++)
                    TryParseFloat(args[j].Trim(), out vals[j]);

                switch (name)
                {
                    case "translate":
                        if (vals.Length >= 2)
                            result = Matrix3x2.CreateTranslation(vals[0], vals[1]) * result;
                        else if (vals.Length >= 1)
                            result = Matrix3x2.CreateTranslation(vals[0], 0) * result;
                        break;

                    case "scale":
                        if (vals.Length >= 2)
                            result = Matrix3x2.CreateScale(vals[0], vals[1]) * result;
                        else if (vals.Length >= 1)
                            result = Matrix3x2.CreateScale(vals[0], vals[0]) * result;
                        break;

                    case "rotate":
                        if (vals.Length >= 3)
                        {
                            // rotate(angle, cx, cy) — rotate around a point
                            result = Matrix3x2.CreateTranslation(-vals[1], -vals[2]) * result;
                            result = Matrix3x2.CreateRotation(vals[0] * (float)(Math.PI / 180.0)) * result;
                            result = Matrix3x2.CreateTranslation(vals[1], vals[2]) * result;
                        }
                        else if (vals.Length >= 1)
                        {
                            result = Matrix3x2.CreateRotation(vals[0] * (float)(Math.PI / 180.0)) * result;
                        }
                        break;

                    case "matrix":
                        if (vals.Length >= 6)
                        {
                            var m = new Matrix3x2(vals[0], vals[1], vals[2], vals[3], vals[4], vals[5]);
                            result = m * result;
                        }
                        break;

                    case "skewX":
                        if (vals.Length >= 1)
                        {
                            float tan = (float)Math.Tan(vals[0] * Math.PI / 180.0);
                            result = new Matrix3x2(1, 0, tan, 1, 0, 0) * result;
                        }
                        break;

                    case "skewY":
                        if (vals.Length >= 1)
                        {
                            float tan = (float)Math.Tan(vals[0] * Math.PI / 180.0);
                            result = new Matrix3x2(1, tan, 0, 1, 0, 0) * result;
                        }
                        break;
                }
            }

            return result;
        }

        private static void SkipWhitespace(string s, ref int i, int len)
        {
            while (i < len && (s[i] == ' ' || s[i] == '\t' || s[i] == '\n' || s[i] == '\r' || s[i] == ','))
                i++;
        }

        private static Element? FindRoot(Element elem)
        {
            Node? node = elem;
            while (node.Parent != null)
                node = node.Parent;
            return node as Element ?? elem;
        }

        private static Element? FindById(Node root, string id)
        {
            if (root is Element el && el.GetAttribute("id") == id)
                return el;

            var child = root.FirstChild;
            while (child != null)
            {
                var found = FindById(child, id);
                if (found != null) return found;
                child = child.NextSibling;
            }
            return null;
        }
    }
}
