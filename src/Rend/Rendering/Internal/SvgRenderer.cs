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
        [ThreadStatic] private static float _currentViewportWidth;
        [ThreadStatic] private static float _currentViewportHeight;

        /// <spec>SVG 1.1 §7.10 https://www.w3.org/TR/SVG11/coords.html#Units</spec>
        private enum SvgLengthAxis
        {
            Horizontal,
            Vertical,
            Diagonal
        }
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

            // Save prior thread-static context so nested renders restore cleanly.
            Element? previousSvgRoot = _currentSvgRoot;
            float previousViewportWidth = _currentViewportWidth;
            float previousViewportHeight = _currentViewportHeight;

            _currentSvgRoot = svgElement;
            _currentViewportWidth = vbW;
            _currentViewportHeight = vbH;

            try
            {
                // Traverse children (pass styled tree for CSS property lookup)
                RenderChildren(svgElement, target, styledSvg);
            }
            finally
            {
                _currentSvgRoot = previousSvgRoot;
                _currentViewportWidth = previousViewportWidth;
                _currentViewportHeight = previousViewportHeight;
            }

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
            string? svgTransformAttr = elem.GetAttribute("transform");
            if (styledElem != null)
            {
                object? cssTransformVal = styledElem.Style.GetRefValue(PropertyId.Transform);
                if (cssTransformVal is CssValue csvTransform &&
                    !(csvTransform is CssKeywordValue noneKw && noneKw.Keyword == "none"))
                {
                    transformMatrix = TransformHandler.BuildTransformMatrix(csvTransform);
                    // [CSS-TRANSFORM §1] Invalid CSS transform (zero/degenerate) → fall back to SVG attribute
                    float det = transformMatrix.M11 * transformMatrix.M22 - transformMatrix.M12 * transformMatrix.M21;
                    hasTransform = transformMatrix != Matrix3x2.Identity && Math.Abs(det) > 0.0001f;
                    if (!hasTransform && svgTransformAttr != null)
                    {
                        transformMatrix = ParseTransform(svgTransformAttr);
                        hasTransform = true;
                    }
                }
            }

            // Fall back to inline style, then SVG attribute
            if (!hasTransform)
            {
                string? cssInline = ExtractStyleProperty(elem, "transform");
                if (cssInline != null)
                {
                    transformMatrix = ParseCssTransform(cssInline);
                    float inlineDet = transformMatrix.M11 * transformMatrix.M22 - transformMatrix.M12 * transformMatrix.M21;
                    hasTransform = transformMatrix != Matrix3x2.Identity && Math.Abs(inlineDet) > 0.0001f;
                }
                if (!hasTransform && svgTransformAttr != null)
                {
                    transformMatrix = ParseTransform(svgTransformAttr);
                    hasTransform = transformMatrix != Matrix3x2.Identity;
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

                // Determine transform-box context for resolving percentages/keywords
                bool isFillBox = false;
                if (styledElem != null)
                {
                    object? boxVal = styledElem.Style.GetRefValue(PropertyId.TransformBox);
                    if (boxVal is CssKeywordValue boxKw && boxKw.Keyword == "fill-box")
                    {
                        isFillBox = true;
                    }
                }

                // Get element bounding box for fill-box resolution
                RectF elementBbox = isFillBox ? GetElementBbox(elem) : default;

                // Check CSS computed style for transform-origin
                if (styledElem != null)
                {
                    object? originVal = styledElem.Style.GetRefValue(PropertyId.TransformOrigin);
                    if (originVal is CssValue originCss)
                    {
                        if (isFillBox)
                        {
                            ResolveFillBoxOrigin(originCss, elementBbox, out originX, out originY);
                        }
                        else
                        {
                            ParseSvgTransformOrigin(originCss, out originX, out originY);
                        }
                        hasOrigin = true;
                    }
                }

                // Fall back to SVG attribute
                if (!hasOrigin)
                {
                    string? originAttr = elem.GetAttribute("transform-origin");
                    if (originAttr != null && originAttr.Length > 0)
                    {
                        if (isFillBox)
                        {
                            ParseSvgOriginAttrFillBox(originAttr, elementBbox, out originX, out originY);
                        }
                        else
                        {
                            ParseSvgOriginAttr(originAttr, out originX, out originY);
                        }
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

            // Handle clip-path
            bool hasClipPath = false;
            string? clipPathAttr = elem.GetAttribute("clip-path");
            if (IsUrlRef(clipPathAttr))
            {
                hasClipPath = ApplySvgClipPath(elem, target, clipPathAttr!);
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

            if (hasClipPath)
            {
                target.PopClip();
            }
            if (hasTransform || opacity < 1f)
            {
                target.Restore();
            }
        }

        private static void RenderRect(Element elem, IRenderTarget target,
            BrushInfo fillBrush, CssColor stroke, float strokeWidth,
            bool hasFill, bool hasStroke, float strokeOpacity)
        {
            float x = ParseAttrLength(elem, "x", SvgLengthAxis.Horizontal, 0);
            float y = ParseAttrLength(elem, "y", SvgLengthAxis.Vertical, 0);
            float w = ParseAttrLength(elem, "width", SvgLengthAxis.Horizontal, 0);
            float h = ParseAttrLength(elem, "height", SvgLengthAxis.Vertical, 0);
            float rx = ParseAttrLength(elem, "rx", SvgLengthAxis.Horizontal, 0);
            float ry = ParseAttrLength(elem, "ry", SvgLengthAxis.Vertical, 0);
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
            float cx = ParseAttrLength(elem, "cx", SvgLengthAxis.Horizontal, 0);
            float cy = ParseAttrLength(elem, "cy", SvgLengthAxis.Vertical, 0);
            float r = ParseAttrLength(elem, "r", SvgLengthAxis.Diagonal, 0);
            if (r <= 0) return;

            var path = BuildEllipsePath(cx, cy, r, r);
            if (hasFill) { target.FillPath(path, fillBrush); }
            if (hasStroke) { target.StrokePath(path, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth)); }
        }

        private static void RenderEllipse(Element elem, IRenderTarget target,
            BrushInfo fillBrush, CssColor stroke, float strokeWidth,
            bool hasFill, bool hasStroke, float strokeOpacity)
        {
            float cx = ParseAttrLength(elem, "cx", SvgLengthAxis.Horizontal, 0);
            float cy = ParseAttrLength(elem, "cy", SvgLengthAxis.Vertical, 0);
            float rx = ParseAttrLength(elem, "rx", SvgLengthAxis.Horizontal, 0);
            float ry = ParseAttrLength(elem, "ry", SvgLengthAxis.Vertical, 0);
            if (rx <= 0 || ry <= 0) return;

            var path = BuildEllipsePath(cx, cy, rx, ry);
            if (hasFill) { target.FillPath(path, fillBrush); }
            if (hasStroke) { target.StrokePath(path, new PenInfo(WithAlpha(stroke, strokeOpacity), strokeWidth)); }
        }

        private static void RenderLine(Element elem, IRenderTarget target,
            CssColor stroke, float strokeWidth, bool hasStroke, float strokeOpacity)
        {
            if (!hasStroke) return;
            float x1 = ParseAttrLength(elem, "x1", SvgLengthAxis.Horizontal, 0);
            float y1 = ParseAttrLength(elem, "y1", SvgLengthAxis.Vertical, 0);
            float x2 = ParseAttrLength(elem, "x2", SvgLengthAxis.Horizontal, 0);
            float y2 = ParseAttrLength(elem, "y2", SvgLengthAxis.Vertical, 0);

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
            float x = ParseAttrLength(elem, "x", SvgLengthAxis.Horizontal, 0);
            float y = ParseAttrLength(elem, "y", SvgLengthAxis.Vertical, 0);
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
            float x = ParseAttrLength(elem, "x", SvgLengthAxis.Horizontal, 0);
            float y = ParseAttrLength(elem, "y", SvgLengthAxis.Vertical, 0);

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
            if (val == null)
            {
                return defaultValue;
            }

            // Handle CSS length units (cm, mm, in, pt, px, etc.)
            if (TryParseSvgLength(val.Trim(), out float result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Parse a geometric SVG attribute that accepts length or percentage.
        /// Percentages resolve against the current SVG viewport width, height,
        /// or normalized diagonal according to the axis of the attribute.
        /// </summary>
        /// <spec>SVG 1.1 §7.10 https://www.w3.org/TR/SVG11/coords.html#Units</spec>
        private static float ParseAttrLength(Element elem, string name, SvgLengthAxis axis, float defaultValue)
        {
            string? val = elem.GetAttribute(name);
            if (val == null)
            {
                return defaultValue;
            }

            string trimmed = val.Trim();
            if (trimmed.Length > 1 && trimmed[trimmed.Length - 1] == '%')
            {
                string numberPart = trimmed.Substring(0, trimmed.Length - 1);
                if (!TryParseFloat(numberPart, out float percent))
                {
                    return defaultValue;
                }

                float basis = ResolveViewportBasis(axis);
                return percent / 100f * basis;
            }

            if (TryParseSvgLength(trimmed, out float result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Return the viewport basis used to resolve a percentage length along
        /// the given axis. Diagonal percentages use the normalized viewport
        /// diagonal sqrt(vbW^2 + vbH^2) / sqrt(2).
        /// </summary>
        /// <spec>SVG 1.1 §7.10 https://www.w3.org/TR/SVG11/coords.html#Units</spec>
        private static float ResolveViewportBasis(SvgLengthAxis axis)
        {
            if (axis == SvgLengthAxis.Horizontal)
            {
                return _currentViewportWidth;
            }
            if (axis == SvgLengthAxis.Vertical)
            {
                return _currentViewportHeight;
            }

            float viewportWidth = _currentViewportWidth;
            float viewportHeight = _currentViewportHeight;
            double diagonal = Math.Sqrt(viewportWidth * viewportWidth + viewportHeight * viewportHeight);
            return (float)(diagonal / Math.Sqrt(2d));
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

                // [SVG §7.6] Detect invalid syntax: trailing/leading/double commas
                string trimmedArgs = argsStr.Trim();
                if (trimmedArgs.EndsWith(",") || trimmedArgs.StartsWith(",") || trimmedArgs.Contains(",,"))
                {
                    return Matrix3x2.Identity;
                }

                var args = argsStr.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                float[] vals = new float[args.Length];
                bool invalidArg = false;
                for (int j = 0; j < args.Length; j++)
                {
                    string arg = args[j].Trim();
                    // SVG transform values must be plain numbers — reject % and other units
                    if (arg.EndsWith("%") || !TryParseFloat(arg, out vals[j]))
                    {
                        invalidArg = true;
                        break;
                    }
                }
                if (invalidArg)
                {
                    // [SVG §7.6] Invalid value → entire transform attribute is ignored
                    return Matrix3x2.Identity;
                }

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
                        if (vals.Length == 3)
                        {
                            // rotate(angle, cx, cy) — rotate around a point
                            result = Matrix3x2.CreateTranslation(vals[1], vals[2]) * result;
                            result = Matrix3x2.CreateRotation(vals[0] * (float)(Math.PI / 180.0)) * result;
                            result = Matrix3x2.CreateTranslation(-vals[1], -vals[2]) * result;
                        }
                        else if (vals.Length == 1)
                        {
                            result = Matrix3x2.CreateRotation(vals[0] * (float)(Math.PI / 180.0)) * result;
                        }
                        else
                        {
                            // [SVG §7.6] rotate takes 1 or 3 args — other counts are invalid
                            return Matrix3x2.Identity;
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

        /// <summary>
        /// Apply SVG clipPath referenced by url(#id).
        /// Finds the clipPath element, builds a clip rect from its children.
        /// </summary>
        private static bool ApplySvgClipPath(Element elem, IRenderTarget target, string clipPathValue)
        {
            string trimmed = clipPathValue.Trim();
            int hashIdx = trimmed.IndexOf('#');
            if (hashIdx < 0)
            {
                return false;
            }

            int endIdx = trimmed.IndexOf(')', hashIdx);
            if (endIdx < 0)
            {
                endIdx = trimmed.Length;
            }

            string refId = trimmed.Substring(hashIdx + 1, endIdx - hashIdx - 1).Trim('\'', '"', ' ');
            if (string.IsNullOrEmpty(refId))
            {
                return false;
            }

            var root = _currentSvgRoot ?? FindRoot(elem);
            if (root == null)
            {
                return false;
            }

            var clipElem = FindById(root, refId);
            if (clipElem == null || clipElem.TagName != "clipPath")
            {
                return false;
            }

            // Build clip region from clipPath children (simplified: use first rect)
            var child = clipElem.FirstChild;
            while (child != null)
            {
                if (child is Element clipChild && clipChild.TagName == "rect")
                {
                    float x = ParseAttrFloat(clipChild, "x", 0);
                    float y = ParseAttrFloat(clipChild, "y", 0);
                    float w = ParseAttrFloat(clipChild, "width", 0);
                    float h = ParseAttrFloat(clipChild, "height", 0);
                    if (w > 0 && h > 0)
                    {
                        target.PushClipRect(new RectF(x, y, w, h));
                        return true;
                    }
                }
                child = child.NextSibling;
            }

            return false;
        }

        /// <summary>Get the bounding box of an SVG element from its geometry attributes.</summary>
        private static RectF GetElementBbox(Element elem)
        {
            switch (elem.TagName)
            {
                case "rect":
                {
                    float x = ParseAttrFloat(elem, "x", 0);
                    float y = ParseAttrFloat(elem, "y", 0);
                    float w = ParseAttrFloat(elem, "width", 0);
                    float h = ParseAttrFloat(elem, "height", 0);
                    return new RectF(x, y, w, h);
                }
                case "circle":
                {
                    float cx = ParseAttrFloat(elem, "cx", 0);
                    float cy = ParseAttrFloat(elem, "cy", 0);
                    float r = ParseAttrFloat(elem, "r", 0);
                    return new RectF(cx - r, cy - r, r * 2, r * 2);
                }
                case "ellipse":
                {
                    float cx = ParseAttrFloat(elem, "cx", 0);
                    float cy = ParseAttrFloat(elem, "cy", 0);
                    float rx = ParseAttrFloat(elem, "rx", 0);
                    float ry = ParseAttrFloat(elem, "ry", 0);
                    return new RectF(cx - rx, cy - ry, rx * 2, ry * 2);
                }
                default:
                    return default;
            }
        }

        /// <summary>
        /// Resolve transform-origin relative to the element's fill-box (bounding box).
        /// Percentages and keywords resolve against the bbox dimensions.
        /// </summary>
        private static void ResolveFillBoxOrigin(CssValue value, RectF bbox,
            out float originX, out float originY)
        {
            originX = bbox.X + bbox.Width * 0.5f;
            originY = bbox.Y + bbox.Height * 0.5f;

            if (value is CssListValue list && list.Separator == ' ' && list.Values.Count >= 2)
            {
                originX = bbox.X + ResolveFillBoxComponent(list.Values[0], bbox.Width);
                originY = bbox.Y + ResolveFillBoxComponent(list.Values[1], bbox.Height);
            }
            else
            {
                originX = bbox.X + ResolveFillBoxComponent(value, bbox.Width);
                originY = bbox.Y + bbox.Height * 0.5f;
            }
        }

        private static float ResolveFillBoxComponent(CssValue value, float size)
        {
            if (value is CssDimensionValue dim)
            {
                return ParseCssLength(dim.Value + dim.Unit);
            }
            if (value is CssPercentageValue pct)
            {
                return pct.Value / 100f * size;
            }
            if (value is CssNumberValue num)
            {
                return num.Value;
            }
            if (value is CssKeywordValue kw)
            {
                switch (kw.Keyword)
                {
                    case "left":
                    case "top": return 0f;
                    case "center": return size * 0.5f;
                    case "right":
                    case "bottom": return size;
                }
            }
            return size * 0.5f;
        }

        /// <summary>Parse SVG transform-origin attribute with fill-box context.</summary>
        private static void ParseSvgOriginAttrFillBox(string attr, RectF bbox,
            out float originX, out float originY)
        {
            originX = 0f;
            originY = 0f;

            var parts = attr.Trim().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return;
            }

            if (parts.Length == 1)
            {
                // Single value: try as X, Y defaults to center
                if (IsYKeyword(parts[0]))
                {
                    // Single Y keyword: X=center
                    originX = bbox.X + bbox.Width * 0.5f;
                    TryParseOriginPartFillBox(parts[0], bbox.Height, false, out float oy);
                    originY = bbox.Y + oy;
                }
                else if (TryParseOriginPartFillBox(parts[0], bbox.Width, true, out float ox))
                {
                    originX = bbox.X + ox;
                    originY = bbox.Y + bbox.Height * 0.5f;
                }
                return;
            }

            // [SVG] Y keyword (top/bottom) as first value with non-keyword second → invalid
            if (IsYKeyword(parts[0]) && !IsKeyword(parts[1]))
            {
                return;
            }
            // X keyword (left/right) as second value with non-keyword first → invalid
            if (IsXKeyword(parts[1]) && !IsKeyword(parts[0]))
            {
                return;
            }

            // Determine axis assignment when both are keywords
            string xPart = parts[0];
            string yPart = parts[1];

            // Swap if needed: Y keyword first or X keyword second
            if (IsYKeyword(parts[0]) || IsXKeyword(parts[1]))
            {
                xPart = parts[1];
                yPart = parts[0];
            }

            if (TryParseOriginPartFillBox(xPart, bbox.Width, true, out float ox2) &&
                TryParseOriginPartFillBox(yPart, bbox.Height, false, out float oy2))
            {
                originX = bbox.X + ox2;
                originY = bbox.Y + oy2;
            }
        }

        private static bool IsYKeyword(string part)
        {
            return part == "top" || part == "bottom";
        }

        private static bool IsXKeyword(string part)
        {
            return part == "left" || part == "right";
        }

        private static bool IsKeyword(string part)
        {
            return part == "left" || part == "right" || part == "top" || part == "bottom" || part == "center";
        }

        /// <summary>
        /// Parse one component of SVG transform-origin attribute with fill-box.
        /// With fill-box, keywords resolve against the element bounding box.
        /// </summary>
        private static bool TryParseOriginPartFillBox(string part, float size,
            bool isHorizontal, out float result)
        {
            result = 0f;
            part = part.Trim();

            switch (part)
            {
                case "center":
                    result = size * 0.5f;
                    return true;
                case "left":
                    result = isHorizontal ? 0f : float.NaN;
                    return !float.IsNaN(result);
                case "right":
                    result = isHorizontal ? size : float.NaN;
                    return !float.IsNaN(result);
                case "top":
                    result = !isHorizontal ? 0f : float.NaN;
                    return !float.IsNaN(result);
                case "bottom":
                    result = !isHorizontal ? size : float.NaN;
                    return !float.IsNaN(result);
            }

            if (part.EndsWith("%"))
            {
                if (TryParseFloat(part.TrimEnd('%'), out float pct))
                {
                    result = pct / 100f * size;
                    return true;
                }
                return false;
            }

            if (TryParseSvgLength(part, out result))
            {
                return true;
            }

            return false;
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

        /// <summary>Parse SVG transform-origin attribute value (e.g. "75" or "75 75" or "2cm 2cm").</summary>
        private static void ParseSvgOriginAttr(string attr, out float originX, out float originY)
        {
            originX = 0f;
            originY = 0f;

            var parts = attr.Trim().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return;
            }

            if (!TryParseSvgLength(parts[0].Trim(), out originX))
            {
                originX = 0f;
                return;
            }

            if (parts.Length >= 2)
            {
                if (!TryParseSvgLength(parts[1].Trim(), out originY))
                {
                    originX = 0f;
                    originY = 0f;
                    return;
                }
            }
            else
            {
                originY = originX;
            }
        }

        /// <summary>Parse an SVG length value with optional CSS units (px, cm, mm, in, pt, em).</summary>
        private static bool TryParseSvgLength(string value, out float result)
        {
            result = 0f;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            // Check for known CSS keywords (invalid in numeric context)
            if (char.IsLetter(value[0]) && value != "0")
            {
                return false;
            }

            if (value.EndsWith("cm"))
            {
                return TryParseFloat(value.Substring(0, value.Length - 2), out result) && (result *= 96f / 2.54f) >= 0 || true;
            }
            if (value.EndsWith("mm"))
            {
                return TryParseFloat(value.Substring(0, value.Length - 2), out result) && (result *= 96f / 25.4f) >= 0 || true;
            }
            if (value.EndsWith("in"))
            {
                return TryParseFloat(value.Substring(0, value.Length - 2), out result) && (result *= 96f) >= 0 || true;
            }
            if (value.EndsWith("pt"))
            {
                return TryParseFloat(value.Substring(0, value.Length - 2), out result) && (result *= 96f / 72f) >= 0 || true;
            }
            if (value.EndsWith("pc"))
            {
                return TryParseFloat(value.Substring(0, value.Length - 2), out result) && (result *= 96f / 6f) >= 0 || true;
            }
            if (value.EndsWith("px"))
            {
                return TryParseFloat(value.Substring(0, value.Length - 2), out result);
            }
            if (value.EndsWith("em"))
            {
                return TryParseFloat(value.Substring(0, value.Length - 2), out result) && (result *= 16f) >= 0 || true;
            }

            return TryParseFloat(value, out result);
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
            gradElem = ResolveGradientHref(gradElem, root);

            float x1 = ParseGradientCoord(gradElem, "x1", 0f);
            float y1 = ParseGradientCoord(gradElem, "y1", 0f);
            float x2 = ParseGradientCoord(gradElem, "x2", 1f);
            float y2 = ParseGradientCoord(gradElem, "y2", 0f);

            var stops = ParseGradientStops(gradElem, fillOpacity);
            if (stops == null || stops.Length < 2) { return null; }

            // [SVG §13.2.2] Parse gradientTransform for shader local matrix
            Matrix3x2? shaderTransform = null;
            string? gradTransformAttr = gradElem.GetAttribute("gradientTransform")
                ?? gradElem.GetAttribute("gradienttransform");
            if (gradTransformAttr != null)
            {
                var gradMatrix = ParseTransform(gradTransformAttr);
                if (gradMatrix != Matrix3x2.Identity)
                {
                    shaderTransform = gradMatrix;
                }
            }

            float dx = x2 - x1;
            float dy = y2 - y1;
            float angleDeg = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI) + 90f;

            string spreadMethod = gradElem.GetAttribute("spreadMethod") ?? gradElem.GetAttribute("spreadmethod") ?? "pad";

            var gradient = new GradientInfo
            {
                Type = GradientType.Linear,
                Stops = stops,
                Angle = angleDeg,
                Repeating = spreadMethod == "repeat" || spreadMethod == "reflect"
            };
            var brush = BrushInfo.FromGradient(gradient);
            brush.ShaderTransform = shaderTransform;
            return brush;
        }

        private static BrushInfo? BuildRadialGradientBrush(Element gradElem, Element root, float fillOpacity)
        {
            gradElem = ResolveGradientHref(gradElem, root);

            float cx = ParseGradientCoord(gradElem, "cx", 0.5f);
            float cy = ParseGradientCoord(gradElem, "cy", 0.5f);
            float r = ParseGradientCoord(gradElem, "r", 0.5f);

            var stops = ParseGradientStops(gradElem, fillOpacity);
            if (stops == null || stops.Length < 2) { return null; }

            string spreadMethod = gradElem.GetAttribute("spreadMethod") ?? gradElem.GetAttribute("spreadmethod") ?? "pad";

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
            float patternWidth = ParseAttrFloat(patternElem, "width", 0);
            float patternHeight = ParseAttrFloat(patternElem, "height", 0);
            if (patternWidth <= 0 || patternHeight <= 0)
            {
                return null;
            }

            // [SVG §13.4.5] Apply patternTransform to the pattern coordinate system
            float patternOffsetX = ParseAttrFloat(patternElem, "x", 0);
            float patternOffsetY = ParseAttrFloat(patternElem, "y", 0);
            string? patTransformAttr = patternElem.GetAttribute("patternTransform")
                ?? patternElem.GetAttribute("patterntransform");
            if (patTransformAttr != null)
            {
                var patMatrix = ParseTransform(patTransformAttr);
                // For translate-only transforms, adjust the pattern offset
                patternOffsetX += patMatrix.M31;
                patternOffsetY += patMatrix.M32;
            }

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
                    return new BrushInfo
                    {
                        Image = imageData,
                        ImageOffsetX = patternOffsetX,
                        ImageOffsetY = patternOffsetY
                    };
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
