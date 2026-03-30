using System;
using System.Globalization;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Parser.Internal;
using Rend.Css.Properties.Internal;
using Rend.Html;
using Rend.Style;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Renders inline SVG elements by traversing the SVG DOM subtree
    /// and converting shapes, paths, text, and groups into IRenderTarget drawing calls.
    /// </summary>
    internal static class SvgRenderer
    {
        [ThreadStatic] private static Element? _currentSvgRoot;
        /// <summary>
        /// Render an SVG element into the given target at the specified content rect.
        /// </summary>
        public static void Render(Element svgElement, IRenderTarget target, RectF contentRect,
            StyledElement? styledSvg = null)
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

            // Store SVG root for url(#id) lookups
            _currentSvgRoot = svgElement;

            // Traverse children (pass styled tree for CSS property lookup)
            RenderChildren(svgElement, target, styledSvg);

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

        private static void RenderChildren(Element parent, IRenderTarget target,
            StyledElement? styledParent = null)
        {
            var child = parent.FirstChild;
            int childIdx = 0;
            while (child != null)
            {
                if (child is Element elem)
                {
                    // Find corresponding styled element for CSS property access
                    StyledElement? styledChild = FindStyledChild(styledParent, elem, childIdx);
                    RenderElement(elem, target, styledChild);
                    childIdx++;
                }
                child = child.NextSibling;
            }
        }

        /// <summary>
        /// Find the StyledElement child that corresponds to a DOM element.
        /// </summary>
        private static StyledElement? FindStyledChild(StyledElement? parent, Element elem, int hint)
        {
            if (parent == null)
            {
                return null;
            }

            var children = parent.Children;
            // Try hint index first (children usually match DOM order)
            if (hint < children.Count && children[hint] is StyledElement hintEl && hintEl.Element == elem)
            {
                return hintEl;
            }
            // Linear search fallback
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is StyledElement se && se.Element == elem)
                {
                    return se;
                }
            }
            return null;
        }

        private static void RenderElement(Element elem, IRenderTarget target,
            StyledElement? styledElem = null)
        {
            string tag = elem.TagName;

            // Skip <defs> — definitions are referenced by <use>, not rendered directly
            if (tag == "defs") return;

            // Parse common presentation attributes
            string? fillAttr = elem.GetAttribute("fill");
            var fill = ParseColor(fillAttr, CssColor.Black);
            var stroke = ParseColor(elem.GetAttribute("stroke"), CssColor.Transparent);
            float strokeWidth = ParseAttrFloat(elem, "stroke-width", 1f);
            float opacity = ParseAttrFloat(elem, "opacity", 1f);
            bool hasFill = !IsNone(fillAttr) && (fill.A > 0 || IsUrlRef(fillAttr));
            bool hasStroke = !IsNone(elem.GetAttribute("stroke")) && stroke.A > 0 && strokeWidth > 0;
            float fillOpacity = ParseAttrFloat(elem, "fill-opacity", 1f);
            float strokeOpacity = ParseAttrFloat(elem, "stroke-opacity", 1f);

            // Resolve url(#id) gradient fill
            BrushInfo? gradientBrush = null;
            if (IsUrlRef(fillAttr))
            {
                gradientBrush = ResolveUrlFill(elem, fillAttr!, fillOpacity);
                hasFill = gradientBrush != null;
            }

            // Handle transform: CSS computed style overrides SVG attribute
            Matrix3x2 transformMatrix = Matrix3x2.Identity;
            bool hasTransform = false;

            // Check CSS computed style for transform (from stylesheet rules)
            if (styledElem != null)
            {
                object? cssTransformVal = styledElem.Style.GetRefValue(PropertyId.Transform);
                if (cssTransformVal is CssValue csvTransform &&
                    !(csvTransform is CssKeywordValue noneKw && noneKw.Keyword == "none"))
                {
                    transformMatrix = TransformHandler.BuildTransformMatrix(csvTransform);
                    hasTransform = true;
                }
            }

            // Fall back to inline style, then SVG attribute
            if (!hasTransform)
            {
                string? cssInline = ExtractStyleProperty(elem, "transform");
                if (cssInline != null)
                {
                    transformMatrix = ParseCssTransform(cssInline);
                    hasTransform = true;
                }
                else
                {
                    string? svgAttr = elem.GetAttribute("transform");
                    if (svgAttr != null)
                    {
                        transformMatrix = ParseTransform(svgAttr);
                        hasTransform = true;
                    }
                }
            }

            if (hasTransform || opacity < 1f)
            {
                target.Save();
            }

            if (opacity < 1f)
            {
                target.SetOpacity(opacity);
            }

            if (hasTransform)
            {
                // [CSS-TRANSFORM §8] Apply transform-origin
                float originX = 0f;
                float originY = 0f;
                bool hasOrigin = false;

                // Check CSS computed style for transform-origin
                if (styledElem != null)
                {
                    object? originVal = styledElem.Style.GetRefValue(PropertyId.TransformOrigin);
                    if (originVal is CssValue originCss)
                    {
                        ParseSvgTransformOrigin(originCss, out originX, out originY);
                        hasOrigin = true;
                    }
                }

                // Fall back to SVG attribute
                if (!hasOrigin)
                {
                    string? originAttr = elem.GetAttribute("transform-origin");
                    if (originAttr != null && originAttr.Length > 0)
                    {
                        ParseSvgOriginAttr(originAttr, out originX, out originY);
                        hasOrigin = true;
                    }
                }

                if (hasOrigin && (originX != 0f || originY != 0f))
                {
                    var toOrigin = Matrix3x2.CreateTranslation(-originX, -originY);
                    var fromOrigin = Matrix3x2.CreateTranslation(originX, originY);
                    transformMatrix = toOrigin * transformMatrix * fromOrigin;
                }

                target.ConcatTransform(transformMatrix);
            }

            // Build the fill brush (gradient or solid)
            BrushInfo fillBrush = gradientBrush ?? BrushInfo.Solid(WithAlpha(fill, fillOpacity));

            switch (tag)
            {
                case "g":
                    RenderChildren(elem, target, styledElem);
                    break;

                case "rect":
                    RenderRect(elem, target, fillBrush, stroke, strokeWidth, hasFill, hasStroke, strokeOpacity);
                    break;

                case "circle":
                    RenderCircle(elem, target, fillBrush, stroke, strokeWidth, hasFill, hasStroke, strokeOpacity);
                    break;

                case "ellipse":
                    RenderEllipse(elem, target, fillBrush, stroke, strokeWidth, hasFill, hasStroke, strokeOpacity);
                    break;

                case "line":
                    RenderLine(elem, target, stroke, strokeWidth, hasStroke, strokeOpacity);
                    break;

                case "polyline":
                    RenderPolyline(elem, target, fillBrush, stroke, strokeWidth, hasFill, hasStroke, strokeOpacity, false);
                    break;

                case "polygon":
                    RenderPolyline(elem, target, fillBrush, stroke, strokeWidth, hasFill, hasStroke, strokeOpacity, true);
                    break;

                case "path":
                    RenderPath(elem, target, fillBrush, stroke, strokeWidth, hasFill, hasStroke, strokeOpacity);
                    break;

                case "text":
                    RenderText(elem, target, fill, fillOpacity);
                    break;

                case "svg":
                    RenderChildren(elem, target, styledElem);
                    break;

                case "use":
                    RenderUse(elem, target);
                    break;
            }

            if (hasTransform || opacity < 1f)
                target.Restore();
        }

        private static void RenderRect(Element elem, IRenderTarget target,
            BrushInfo fillBrush, CssColor stroke, float strokeWidth,
            bool hasFill, bool hasStroke, float strokeOpacity)
        {
            float x = ParseAttrFloat(elem, "x", 0);
            float y = ParseAttrFloat(elem, "y", 0);
            float w = ParseAttrFloat(elem, "width", 0);
            float h = ParseAttrFloat(elem, "height", 0);
            float rx = ParseAttrFloat(elem, "rx", 0);
            float ry = ParseAttrFloat(elem, "ry", 0);
            if (w <= 0 || h <= 0) return;

            if (rx > 0 && ry == 0) { ry = rx; }
            if (ry > 0 && rx == 0) { rx = ry; }

            if (rx > 0 || ry > 0)
            {
                var path = new PathData();
                path.AddRoundedRectangle(new RectF(x, y, w, h), rx, rx, rx, rx);
                if (hasFill)
                {
                    target.FillPath(path, fillBrush);
                }
                if (hasStroke)
                {
                    target.StrokePath(path, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth));
                }
            }
            else
            {
                var rect = new RectF(x, y, w, h);
                if (hasFill)
                {
                    target.FillRect(rect, fillBrush);
                }
                if (hasStroke)
                {
                    target.StrokeRect(rect, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth));
                }
            }
        }

        private static void RenderCircle(Element elem, IRenderTarget target,
            BrushInfo fillBrush, CssColor stroke, float strokeWidth,
            bool hasFill, bool hasStroke, float strokeOpacity)
        {
            float cx = ParseAttrFloat(elem, "cx", 0);
            float cy = ParseAttrFloat(elem, "cy", 0);
            float r = ParseAttrFloat(elem, "r", 0);
            if (r <= 0) return;

            var path = BuildEllipsePath(cx, cy, r, r);
            if (hasFill) { target.FillPath(path, fillBrush); }
            if (hasStroke) { target.StrokePath(path, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth)); }
        }

        private static void RenderEllipse(Element elem, IRenderTarget target,
            BrushInfo fillBrush, CssColor stroke, float strokeWidth,
            bool hasFill, bool hasStroke, float strokeOpacity)
        {
            float cx = ParseAttrFloat(elem, "cx", 0);
            float cy = ParseAttrFloat(elem, "cy", 0);
            float rx = ParseAttrFloat(elem, "rx", 0);
            float ry = ParseAttrFloat(elem, "ry", 0);
            if (rx <= 0 || ry <= 0) return;

            var path = BuildEllipsePath(cx, cy, rx, ry);
            if (hasFill) { target.FillPath(path, fillBrush); }
            if (hasStroke) { target.StrokePath(path, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth)); }
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
            BrushInfo fillBrush, CssColor stroke, float strokeWidth,
            bool hasFill, bool hasStroke, float strokeOpacity, bool close)
        {
            string? points = elem.GetAttribute("points");
            if (string.IsNullOrWhiteSpace(points)) return;

            var path = ParsePoints(points!, close);
            if (hasFill) { target.FillPath(path, fillBrush); }
            if (hasStroke) { target.StrokePath(path, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth)); }
        }

        private static void RenderPath(Element elem, IRenderTarget target,
            BrushInfo fillBrush, CssColor stroke, float strokeWidth,
            bool hasFill, bool hasStroke, float strokeOpacity)
        {
            string? d = elem.GetAttribute("d");
            if (string.IsNullOrWhiteSpace(d)) return;

            var path = SvgPathParser.Parse(d!);
            if (hasFill) { target.FillPath(path, fillBrush); }
            if (hasStroke) { target.StrokePath(path, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth)); }
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

            // Build font descriptor (default to sans-serif if no family specified).
            // SVG font-family attribute can be comma-separated like CSS.
            string[] families = Fonts.FontMatchingAlgorithm.ParseFontFamilyList(fontFamily ?? "sans-serif");
            var fontDesc = new Fonts.FontDescriptor(families, fontWeight, cssFontStyle);

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
            // Use AddRoundedRectangleElliptical with radii = half dimensions to create an oval.
            // This triggers native SKRoundRect (type=kOval) matching Chrome's Skia drawOval.
            var path = new PathData();
            var rect = new RectF(cx - rx, cy - ry, rx * 2f, ry * 2f);
            path.AddRoundedRectangleElliptical(rect, rx, ry, rx, ry, rx, ry, rx, ry);
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
                            // Prepend order: T(cx,cy) first, then R, then T(-cx,-cy)
                            // so point * result = point * T(-cx,-cy) * R(angle) * T(cx,cy)
                            result = Matrix3x2.CreateTranslation(vals[1], vals[2]) * result;
                            result = Matrix3x2.CreateRotation(vals[0] * (float)(Math.PI / 180.0)) * result;
                            result = Matrix3x2.CreateTranslation(-vals[1], -vals[2]) * result;
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

        /// <summary>
        /// Extract a CSS property value from an element's inline style attribute.
        /// </summary>
        private static string? ExtractStyleProperty(Element elem, string property)
        {
            string? style = elem.GetAttribute("style");
            if (style == null)
            {
                return null;
            }

            int idx = style.IndexOf(property + ":", StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                return null;
            }

            int start = idx + property.Length + 1;
            int end = style.IndexOf(';', start);
            if (end < 0)
            {
                end = style.Length;
            }
            return style.Substring(start, end - start).Trim();
        }

        /// <summary>
        /// Parse a CSS transform value string (e.g. "rotate(90deg) scale(2)") into a Matrix3x2.
        /// Uses CSS angle units (deg, rad, grad, turn) unlike SVG which uses bare degrees.
        /// </summary>
        private static Matrix3x2 ParseCssTransform(string cssTransform)
        {
            var result = Matrix3x2.Identity;
            int i = 0;
            int len = cssTransform.Length;

            while (i < len)
            {
                SkipWhitespace(cssTransform, ref i, len);
                if (i >= len)
                {
                    break;
                }

                int nameStart = i;
                while (i < len && cssTransform[i] != '(')
                {
                    i++;
                }
                if (i >= len)
                {
                    break;
                }

                string name = cssTransform.Substring(nameStart, i - nameStart).Trim().ToLowerInvariant();
                i++; // skip '('

                int argsStart = i;
                while (i < len && cssTransform[i] != ')')
                {
                    i++;
                }
                if (i >= len)
                {
                    break;
                }

                string argsStr = cssTransform.Substring(argsStart, i - argsStart);
                i++; // skip ')'

                var args = argsStr.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                switch (name)
                {
                    case "rotate":
                    {
                        float angle = ParseCssAngle(args.Length > 0 ? args[0] : "0");
                        result = Matrix3x2.CreateRotation(angle) * result;
                        break;
                    }
                    case "scale":
                    {
                        TryParseFloat(args.Length > 0 ? args[0] : "1", out float sx);
                        float sy = sx;
                        if (args.Length > 1)
                        {
                            TryParseFloat(args[1], out sy);
                        }
                        result = Matrix3x2.CreateScale(sx, sy) * result;
                        break;
                    }
                    case "translate":
                    {
                        float tx = ParseCssLength(args.Length > 0 ? args[0] : "0");
                        float ty = args.Length > 1 ? ParseCssLength(args[1]) : 0;
                        result = Matrix3x2.CreateTranslation(tx, ty) * result;
                        break;
                    }
                    case "translatex":
                    {
                        float tx = ParseCssLength(args.Length > 0 ? args[0] : "0");
                        result = Matrix3x2.CreateTranslation(tx, 0) * result;
                        break;
                    }
                    case "translatey":
                    {
                        float ty = ParseCssLength(args.Length > 0 ? args[0] : "0");
                        result = Matrix3x2.CreateTranslation(0, ty) * result;
                        break;
                    }
                    case "skewx":
                    {
                        float angle = ParseCssAngle(args.Length > 0 ? args[0] : "0");
                        result = Matrix3x2.CreateSkew(angle, 0) * result;
                        break;
                    }
                    case "skewy":
                    {
                        float angle = ParseCssAngle(args.Length > 0 ? args[0] : "0");
                        result = Matrix3x2.CreateSkew(0, angle) * result;
                        break;
                    }
                    case "matrix":
                    {
                        if (args.Length >= 6)
                        {
                            float[] vals = new float[6];
                            for (int j = 0; j < 6; j++)
                            {
                                TryParseFloat(args[j].Trim(), out vals[j]);
                            }
                            result = new Matrix3x2(vals[0], vals[1], vals[2], vals[3], vals[4], vals[5]) * result;
                        }
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>Parse a CSS angle value (e.g. "90deg", "1.57rad") to radians.</summary>
        private static float ParseCssAngle(string value)
        {
            value = value.Trim();
            if (value.EndsWith("deg"))
            {
                TryParseFloat(value.Substring(0, value.Length - 3), out float deg);
                return deg * ((float)Math.PI / 180f);
            }
            if (value.EndsWith("rad"))
            {
                TryParseFloat(value.Substring(0, value.Length - 3), out float rad);
                return rad;
            }
            if (value.EndsWith("grad"))
            {
                TryParseFloat(value.Substring(0, value.Length - 4), out float grad);
                return grad * ((float)Math.PI / 200f);
            }
            if (value.EndsWith("turn"))
            {
                TryParseFloat(value.Substring(0, value.Length - 4), out float turn);
                return turn * 2f * (float)Math.PI;
            }
            // Bare number: assume degrees (CSS default)
            TryParseFloat(value, out float bare);
            return bare * ((float)Math.PI / 180f);
        }

        /// <summary>Parse a CSS length value (e.g. "50px", "10%") to pixels.</summary>
        private static float ParseCssLength(string value)
        {
            value = value.Trim();
            if (value.EndsWith("px"))
            {
                TryParseFloat(value.Substring(0, value.Length - 2), out float px);
                return px;
            }
            if (value.EndsWith("em"))
            {
                TryParseFloat(value.Substring(0, value.Length - 2), out float em);
                return em * 16f;
            }
            TryParseFloat(value, out float num);
            return num;
        }

        /// <summary>Parse SVG transform-origin from CSS computed value.</summary>
        private static void ParseSvgTransformOrigin(CssValue value, out float originX, out float originY)
        {
            originX = 0f;
            originY = 0f;

            if (value is CssListValue list && list.Separator == ' ' && list.Values.Count >= 2)
            {
                originX = ResolveSvgOriginValue(list.Values[0]);
                originY = ResolveSvgOriginValue(list.Values[1]);
            }
            else
            {
                originX = ResolveSvgOriginValue(value);
            }
        }

        private static float ResolveSvgOriginValue(CssValue value)
        {
            if (value is CssDimensionValue dim)
            {
                return ParseCssLength(dim.Value + dim.Unit);
            }
            if (value is CssPercentageValue pct)
            {
                // For SVG without transform-box, percentages are ambiguous.
                // Default: treat as viewport fraction (not useful). Return 0.
                return 0f;
            }
            if (value is CssNumberValue num)
            {
                return num.Value;
            }
            if (value is CssKeywordValue kw)
            {
                switch (kw.Keyword)
                {
                    case "center": return 0f; // Would need element size context
                    case "left":
                    case "top": return 0f;
                }
            }
            return 0f;
        }

        /// <summary>Parse SVG transform-origin attribute value (e.g. "75" or "75 75").</summary>
        private static void ParseSvgOriginAttr(string attr, out float originX, out float originY)
        {
            originX = 0f;
            originY = 0f;

            var parts = attr.Trim().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return;
            }

            // SVG presentation attribute: only numeric values (px) are valid
            if (!TryParseFloat(parts[0].Trim().TrimEnd('p', 'x'), out originX))
            {
                originX = 0f;
                return;
            }

            if (parts.Length >= 2)
            {
                if (!TryParseFloat(parts[1].Trim().TrimEnd('p', 'x'), out originY))
                {
                    // Invalid second value → use default
                    originX = 0f;
                    originY = 0f;
                    return;
                }
            }
            else
            {
                // Single value: second defaults to same as first (matching Chrome)
                originY = originX;
            }
        }

        private static bool IsUrlRef(string? value)
        {
            return value != null && value.StartsWith("url(");
        }

        /// <summary>
        /// Resolve a url(#id) fill reference to a BrushInfo (gradient or pattern).
        /// </summary>
        private static BrushInfo? ResolveUrlFill(Element elem, string fillValue, float fillOpacity)
        {
            string trimmed = fillValue.Trim();
            if (!trimmed.StartsWith("url(")) { return null; }

            int hashIdx = trimmed.IndexOf('#');
            if (hashIdx < 0) { Console.WriteLine("[SVG] no # in url"); return null; }

            int endIdx = trimmed.IndexOf(')', hashIdx);
            if (endIdx < 0) { endIdx = trimmed.Length; }

            string refId = trimmed.Substring(hashIdx + 1, endIdx - hashIdx - 1).Trim('\'', '"', ' ');
            if (string.IsNullOrEmpty(refId)) { Console.WriteLine("[SVG] empty refId"); return null; }

            var root = _currentSvgRoot ?? FindRoot(elem);
            if (root == null) { return null; }

            var refElem = FindById(root, refId);
            if (refElem == null) { return null; }

            switch (refElem.TagName)
            {
                case "linearGradient":
                    return BuildLinearGradientBrush(refElem, root, fillOpacity);
                case "radialGradient":
                    return BuildRadialGradientBrush(refElem, root, fillOpacity);
                case "pattern":
                    return BuildPatternBrush(refElem, root, fillOpacity);
                default:
                    return null;
            }
        }

        private static BrushInfo? BuildLinearGradientBrush(Element gradElem, Element root, float fillOpacity)
        {
            // Resolve href/xlink:href inheritance
            gradElem = ResolveGradientHref(gradElem, root);

            float x1 = ParseGradientCoord(gradElem, "x1", 0f);
            float y1 = ParseGradientCoord(gradElem, "y1", 0f);
            float x2 = ParseGradientCoord(gradElem, "x2", 1f);
            float y2 = ParseGradientCoord(gradElem, "y2", 0f);

            var stops = ParseGradientStops(gradElem, fillOpacity);
            if (stops == null || stops.Length < 2) { return null; }

            // Convert SVG coordinates to angle for GradientInfo
            float dx = x2 - x1;
            float dy = y2 - y1;
            float angleDeg = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI) + 90f;

            string spreadMethod = gradElem.GetAttribute("spreadMethod") ?? "pad";

            var gradient = new GradientInfo
            {
                Type = GradientType.Linear,
                Stops = stops,
                Angle = angleDeg,
                Repeating = spreadMethod == "repeat" || spreadMethod == "reflect"
            };
            return BrushInfo.FromGradient(gradient);
        }

        private static BrushInfo? BuildRadialGradientBrush(Element gradElem, Element root, float fillOpacity)
        {
            gradElem = ResolveGradientHref(gradElem, root);

            float cx = ParseGradientCoord(gradElem, "cx", 0.5f);
            float cy = ParseGradientCoord(gradElem, "cy", 0.5f);
            float r = ParseGradientCoord(gradElem, "r", 0.5f);

            var stops = ParseGradientStops(gradElem, fillOpacity);
            if (stops == null || stops.Length < 2) { return null; }

            string spreadMethod = gradElem.GetAttribute("spreadMethod") ?? "pad";

            var gradient = new GradientInfo
            {
                Type = GradientType.Radial,
                Stops = stops,
                Center = new PointF(cx, cy),
                RadiusX = r,
                RadiusY = r,
                Repeating = spreadMethod == "repeat" || spreadMethod == "reflect"
            };
            return BrushInfo.FromGradient(gradient);
        }

        private static BrushInfo? BuildPatternBrush(Element patternElem, Element root, float fillOpacity)
        {
            // Parse pattern dimensions
            float patternWidth = ParseAttrFloat(patternElem, "width", 0);
            float patternHeight = ParseAttrFloat(patternElem, "height", 0);
            if (patternWidth <= 0 || patternHeight <= 0)
            {
                return null;
            }

            // Build a simple solid-color tile by evaluating the pattern's child rects
            // This is a simplified approach: render children as colored rectangles
            int tileW = Math.Max(1, (int)Math.Ceiling(patternWidth));
            int tileH = Math.Max(1, (int)Math.Ceiling(patternHeight));

            using (var tileBitmap = new SkiaSharp.SKBitmap(tileW, tileH, SkiaSharp.SKColorType.Rgba8888, SkiaSharp.SKAlphaType.Premul))
            using (var tileCanvas = new SkiaSharp.SKCanvas(tileBitmap))
            {
                tileCanvas.Clear(SkiaSharp.SKColors.Transparent);

                // Draw each child element directly on the tile canvas
                var paint = new SkiaSharp.SKPaint { IsAntialias = true };
                var child = patternElem.FirstChild;
                while (child != null)
                {
                    if (child is Element childElem)
                    {
                        DrawPatternChild(childElem, tileCanvas, paint);
                    }
                    child = child.NextSibling;
                }
                paint.Dispose();

                // Encode the tile and create an ImageData for tiled fill
                using (var image = SkiaSharp.SKImage.FromBitmap(tileBitmap))
                {
                    if (image == null)
                    {
                        return null;
                    }
                    var encoded = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
                    if (encoded == null)
                    {
                        return null;
                    }
                    var imageData = new ImageData(encoded.ToArray(), tileW, tileH, "png");
                    return new BrushInfo { Image = imageData };
                }
            }
        }

        /// <summary>
        /// Draw a simple SVG element directly onto an SKCanvas (for pattern tiles).
        /// Supports rect, circle, ellipse, and path.
        /// </summary>
        private static void DrawPatternChild(Element elem, SkiaSharp.SKCanvas canvas, SkiaSharp.SKPaint paint)
        {
            string tag = elem.TagName;
            var fillColor = ParseColor(elem.GetAttribute("fill"), CssColor.Black);
            if (IsNone(elem.GetAttribute("fill")))
            {
                return;
            }

            paint.Color = new SkiaSharp.SKColor(fillColor.R, fillColor.G, fillColor.B, fillColor.A);
            paint.Style = SkiaSharp.SKPaintStyle.Fill;

            switch (tag)
            {
                case "rect":
                {
                    float x = ParseAttrFloat(elem, "x", 0);
                    float y = ParseAttrFloat(elem, "y", 0);
                    float w = ParseAttrFloat(elem, "width", 0);
                    float h = ParseAttrFloat(elem, "height", 0);
                    if (w > 0 && h > 0)
                    {
                        canvas.DrawRect(x, y, w, h, paint);
                    }
                    break;
                }
                case "circle":
                {
                    float cx = ParseAttrFloat(elem, "cx", 0);
                    float cy = ParseAttrFloat(elem, "cy", 0);
                    float r = ParseAttrFloat(elem, "r", 0);
                    if (r > 0)
                    {
                        canvas.DrawCircle(cx, cy, r, paint);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Resolve gradient href chain (SVG gradients can inherit stops via xlink:href).
        /// </summary>
        private static Element ResolveGradientHref(Element gradElem, Element root)
        {
            string? href = gradElem.GetAttribute("href") ?? gradElem.GetAttribute("xlink:href");
            if (href != null && href.StartsWith("#"))
            {
                string refId = href.Substring(1);
                var refElem = FindById(root, refId);
                if (refElem != null)
                {
                    // If this gradient has no stops, inherit from referenced gradient
                    bool hasStops = false;
                    var child = gradElem.FirstChild;
                    while (child != null)
                    {
                        if (child is Element childEl && childEl.TagName == "stop")
                        {
                            hasStops = true;
                            break;
                        }
                        child = child.NextSibling;
                    }
                    if (!hasStops)
                    {
                        return refElem;
                    }
                }
            }
            return gradElem;
        }

        private static GradientStop[]? ParseGradientStops(Element gradElem, float fillOpacity)
        {
            var stops = new System.Collections.Generic.List<GradientStop>();

            var child = gradElem.FirstChild;
            while (child != null)
            {
                if (child is Element stopElem && stopElem.TagName == "stop")
                {
                    float offset = 0f;
                    string? offsetAttr = stopElem.GetAttribute("offset");
                    if (offsetAttr != null)
                    {
                        offsetAttr = offsetAttr.Trim();
                        if (offsetAttr.EndsWith("%"))
                        {
                            TryParseFloat(offsetAttr.TrimEnd('%'), out offset);
                            offset /= 100f;
                        }
                        else
                        {
                            TryParseFloat(offsetAttr, out offset);
                        }
                    }

                    // Parse stop-color (attribute or style)
                    string? stopColorStr = stopElem.GetAttribute("stop-color");
                    if (stopColorStr == null)
                    {
                        // Check inline style
                        string? style = stopElem.GetAttribute("style");
                        if (style != null)
                        {
                            int idx = style.IndexOf("stop-color:");
                            if (idx >= 0)
                            {
                                int start = idx + 11;
                                int end = style.IndexOf(';', start);
                                if (end < 0) { end = style.Length; }
                                stopColorStr = style.Substring(start, end - start).Trim();
                            }
                        }
                    }

                    CssColor stopColor = ParseColor(stopColorStr, CssColor.Black);
                    float stopOpacity = ParseAttrFloat(stopElem, "stop-opacity", 1f) * fillOpacity;
                    stopColor = WithAlpha(stopColor, stopOpacity);

                    offset = Math.Max(0f, Math.Min(1f, offset));
                    stops.Add(new GradientStop(stopColor, offset));
                }
                child = child.NextSibling;
            }

            return stops.Count >= 2 ? stops.ToArray() : null;
        }

        private static float ParseGradientCoord(Element elem, string attr, float defaultVal)
        {
            string? val = elem.GetAttribute(attr);
            if (val == null) { return defaultVal; }

            val = val.Trim();
            if (val.EndsWith("%"))
            {
                if (TryParseFloat(val.TrimEnd('%'), out float pct))
                {
                    return pct / 100f;
                }
            }
            else
            {
                if (TryParseFloat(val, out float num))
                {
                    return num;
                }
            }
            return defaultVal;
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
