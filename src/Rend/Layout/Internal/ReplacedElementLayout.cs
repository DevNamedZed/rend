using System;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Fonts;
using Rend.Style;
using Rend.Text;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Layout for replaced elements (&lt;img&gt;, &lt;svg&gt;, form controls) with intrinsic dimensions.
    /// </summary>
    internal static class ReplacedElementLayout
    {
        /// <summary>
        /// Returns true if this is a replaced element (media, embedded, or form control).
        /// Note: &lt;button&gt; is NOT treated as replaced — it renders children normally as inline-block.
        /// </summary>
        public static bool IsReplaced(StyledElement element)
        {
            string tag = element.TagName;
            if (tag == "img" || tag == "svg" || tag == "video" || tag == "audio" ||
                tag == "canvas" || tag == "iframe" || tag == "object" || tag == "embed" ||
                tag == "math")
            {
                return true;
            }

            // Form controls are replaced (except <button>, which renders children)
            if (tag == "input" || tag == "select" || tag == "textarea")
            {
                return true;
            }

            // Meter and progress are replaced inline-block elements
            if (tag == "meter" || tag == "progress")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the intrinsic aspect ratio (width/height) for a replaced element, or 0 if none.
        /// For SVG with viewBox, the ratio comes from viewBox dimensions.
        /// For img with width/height attributes, the ratio comes from those attributes.
        /// </summary>
        public static float GetIntrinsicRatio(StyledElement element)
        {
            string tag = element.TagName;

            // [CSS-SIZING-4 §4] A bare `aspect-ratio: <ratio>` (without `auto`) overrides the
            // element's natural/intrinsic ratio. `auto` (or `auto <ratio>`) keeps intrinsic preference.
            float overrideRatio = DimensionResolver.GetAspectRatio(element.Style);
            if (overrideRatio > 0 && !DimensionResolver.IsAspectRatioAuto(element.Style))
            {
                return overrideRatio;
            }

            if (tag == "svg")
            {
                // SVG viewBox defines the intrinsic ratio
                string? viewBox = element.GetAttribute("viewbox");
                if (viewBox != null)
                {
                    var parts = viewBox.Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4
                        && float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float vbW)
                        && float.TryParse(parts[3], System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float vbH)
                        && vbH > 0)
                    {
                        return vbW / vbH;
                    }
                }
            }
            else if (tag == "img")
            {
                string? attrW = element.GetAttribute("width");
                string? attrH = element.GetAttribute("height");
                if (attrW != null && attrH != null
                    && float.TryParse(attrW, out float iw)
                    && float.TryParse(attrH, out float ih) && ih > 0)
                {
                    return iw / ih;
                }
                // [CSS-IMAGES-3 §6] SVG inside a data: URI carries its ratio in
                // the root <svg> element's width/height or viewBox. Use that
                // when no HTML width/height attributes override it.
                float svgRatio = GetSvgDataUriRatio(element);
                if (svgRatio > 0)
                {
                    return svgRatio;
                }
            }
            else if (tag == "canvas")
            {
                // [HTML §4.12.5] The canvas element's intrinsic dimensions are its
                // `width`/`height` content attributes (defaulting to 300x150), so its
                // intrinsic ratio is width/height of those attributes.
                float canvasWidth = GetCanvasIntrinsicDimension(element, "width", 300f);
                float canvasHeight = GetCanvasIntrinsicDimension(element, "height", 150f);
                if (canvasHeight > 0)
                {
                    return canvasWidth / canvasHeight;
                }
            }
            else if (tag == "object" || tag == "embed")
            {
                // [CSS-IMAGES-3 §6] An <object>/<embed> referencing an SVG data: URI carries its
                // intrinsic ratio in the embedded root <svg> (width/height lengths or viewBox).
                // This is the natural ratio for `aspect-ratio: auto <ratio>`, which must prefer the
                // intrinsic ratio over the supplied fallback.
                float svgRatio = GetSvgDataUriRatio(element);
                if (svgRatio > 0)
                {
                    return svgRatio;
                }
            }
            // CSS aspect-ratio property
            float cssRatio = DimensionResolver.GetAspectRatio(element.Style);
            if (cssRatio > 0)
            {
                return cssRatio;
            }
            return 0;
        }

        /// <summary>
        /// [HTML §4.12.5] Returns a canvas intrinsic dimension from its <c>width</c>/<c>height</c>
        /// content attribute (a non-negative integer in CSS pixels), falling back to the HTML
        /// default (300x150) when the attribute is absent or invalid.
        /// </summary>
        private static float GetCanvasIntrinsicDimension(StyledElement element, string attributeName, float defaultValue)
        {
            string? attributeValue = element.GetAttribute(attributeName);
            if (attributeValue != null
                && float.TryParse(attributeValue, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float parsed)
                && parsed >= 0)
            {
                return parsed;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns the default intrinsic width for a form control element, or 0 if not a form control.
        /// </summary>
        public static float GetFormControlIntrinsicWidth(StyledElement element, TextMeasurer? measurer = null)
        {
            string tag = element.TagName;

            if (tag == "input")
            {
                string inputType = element.GetAttribute("type")?.ToLowerInvariant() ?? "text";
                switch (inputType)
                {
                    case "checkbox":
                    case "radio":
                        return 13f;
                    case "submit":
                    case "button":
                    case "reset":
                        // Chrome: text_width + padding(6+6) + border(2+2) = text_width + 16
                        string? value = element.GetAttribute("value");
                        if (string.IsNullOrEmpty(value))
                        {
                            value = inputType == "submit" ? "Submit"
                                  : inputType == "reset" ? "Reset"
                                  : "Button";
                        }
                        float textW;
                        if (measurer != null)
                        {
                            var font = new FontDescriptor("sans-serif", 400f);
                            textW = measurer.MeasureWidth(value!, font, 13.333f);
                        }
                        else
                        {
                            textW = value!.Length * 6.1f;
                        }
                        return Math.Max(40f, textW + 16f);
                    default:
                        // text, password, email, url, search, tel, number, etc.
                        // Chrome: size=20 chars × AvgCharWidth() (font '0' glyph advance)
                        // Chrome measured: 177px border-box = 169px content-box
                        // (with UA CSS: border:2px + padding:1px 2px → 8px total horizontal)
                        return 169f;
                }
            }

            if (tag == "select")
            {
                // Chrome 116 select intrinsic sizing (measured via diagnostic):
                // border-box = 75x19 for "Option 1" with 1px border, box-sizing: border-box
                // content-box = 73x17 (border-box minus 2px border)
                // Text width: "Option 1" = 50.39px (Arial 13.333px via DirectWrite)
                // Internal padding: 4px start + 1px + ~15px arrow end ≈ 20px horizontal
                // Our UA has border: 1px, padding: 0 (content-box sizing) → return content-box width
                //
                // Note: Our HarfBuzz measurement gives ~53px for "Option 1" (differs from Chrome's
                // DirectWrite), so we use a per-char estimate of ~6.3px matching Chrome's average.
                float maxTextWidth = 0;
                var child = element.Element.FirstChild;
                while (child != null)
                {
                    if (child is Html.Element optEl && optEl.TagName == "option")
                    {
                        string text = optEl.TextContent?.Trim() ?? "";
                        // ~6.3px per char matches Chrome's Arial 13.333px average char width
                        float w = text.Length * 6.3f;
                        if (w > maxTextWidth) maxTextWidth = w;
                    }
                    child = child.NextSibling;
                }
                if (maxTextWidth == 0) maxTextWidth = 8 * 6.3f;
                // Chrome: content-box = ceil(textWidth) + internal padding (~22px)
                return (float)Math.Ceiling(maxTextWidth) + 22f;
            }

            if (tag == "textarea")
            {
                // Chrome intrinsic textarea sizing (measured from Chrome 116):
                // Content-box width = ceil(avgCharWidth * cols) + scrollbarThickness(17)
                // avgCharWidth for Courier New at 13.333px = 7.329px (from Chrome's DirectWrite)
                // Content-box height = FontMetrics::Height() * rows = 15px * rows
                // Note: Our HarfBuzz shaping gives 8.001px/char (raw font advance) but Chrome's
                // layout uses DirectWrite hinted advances which differ. Use Chrome's measured value.
                string? cols = element.GetAttribute("cols");
                int colCount = 20; // default
                if (cols != null && int.TryParse(cols, out int c) && c > 0)
                    colCount = c;
                // Chrome: ceil(7.329 * cols) + 17 (scrollbar)
                // cols=20 → ceil(146.58)+17 = 147+17 = 164
                // cols=30 → ceil(219.87)+17 = 220+17 = 237
                float charWidth = 7.329f;
                return (float)Math.Ceiling(charWidth * colCount) + 17f;
            }

            if (tag == "meter" || tag == "progress")
                return 80f; // Default width per WHATWG spec

            if (tag == "canvas")
                return GetCanvasIntrinsicDimension(element, "width", 300f);

            if (tag == "video")
                return 300f; // Default 300x150 per HTML spec

            if (tag == "audio")
                return 300f; // Typical audio player width

            if (tag == "iframe")
                return 300f; // Default 300x150 per HTML spec

            if (tag == "math")
                return 0f; // Math elements size to content; will be measured during layout

            return 0f;
        }

        /// <summary>
        /// Returns the default intrinsic height for a form control element, or 0 if not a form control.
        /// </summary>
        public static float GetFormControlIntrinsicHeight(StyledElement element)
        {
            string tag = element.TagName;

            if (tag == "input")
            {
                string inputType = element.GetAttribute("type")?.ToLowerInvariant() ?? "text";
                switch (inputType)
                {
                    case "checkbox":
                    case "radio":
                        return 13f;
                    case "submit":
                    case "button":
                    case "reset":
                        return 21f;
                    default:
                        // Content-box height: 21px total - 4px border - 2px padding = 15px
                        return 15f;
                }
            }

            if (tag == "select")
                return 17f; // Chrome: FontMetrics::Height()+2 = 17px content-box (border-box 19)

            if (tag == "textarea")
            {
                // Chrome: FontMetrics::Height() * rows
                // For Courier New at 13.333px: Height = ascent + descent = 15
                // Chrome diagnostic shows rows=3 → content height = 45
                string? rows = element.GetAttribute("rows");
                int rowCount = 2; // default
                if (rows != null && int.TryParse(rows, out int r) && r > 0)
                    rowCount = r;
                // Line height = font metrics height for line-height:normal
                // For Courier New at 13.333px: 15px per line
                return rowCount * 15f;
            }

            if (tag == "meter" || tag == "progress")
                return 16f; // Default height per WHATWG spec

            if (tag == "canvas")
                return GetCanvasIntrinsicDimension(element, "height", 150f);

            if (tag == "video")
                return 150f; // Default 300x150 per HTML spec

            if (tag == "audio")
                return 32f; // Compact audio player height

            if (tag == "iframe")
                return 150f; // Default 300x150 per HTML spec

            if (tag == "math")
                return 0f; // Math elements size to content

            return 0f;
        }

        /// <summary>
        /// Returns true if the element is a form control that should be treated as replaced.
        /// </summary>

        public static bool IsFormControl(StyledElement element)
        {
            string tag = element.TagName;
            return tag == "input" || tag == "select" || tag == "textarea";
        }

        /// <summary>
        /// Tries to extract intrinsic dimensions from a data: URI image source.
        /// Only decodes enough of the header to read PNG/JPEG/GIF dimensions.
        /// </summary>
        public static bool TryGetDataUriDimensions(StyledElement element, out float width, out float height)
        {
            width = 0;
            height = 0;
            if (element.TagName != "img")
            {
                return false;
            }

            string? src = element.GetAttribute("src");
            if (src == null || !src.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // [CSS-IMAGES-3 §6] SVG in data: URI — read width/height from the root
            // <svg> element. Callers treat the output pair as "per-axis intrinsic
            // size, 0 if none". When the SVG declares only one dimension the other
            // is reported as 0 and callers derive it from the intrinsic ratio
            // exposed by GetIntrinsicRatio. The ratio-only case (neither width nor
            // height declared, viewBox present) is reported via the ratio helper
            // and intentionally returns false here so callers that rely on
            // "partial intrinsic" semantics see the zero-zero case and fall back
            // to the default sizing algorithm on their own.
            if (src.StartsWith("data:image/svg", StringComparison.OrdinalIgnoreCase))
            {
                if (TryExtractSvgRootSize(src, out float svgW, out float svgH, out _, out _))
                {
                    width = svgW > 0 ? svgW : 0;
                    height = svgH > 0 ? svgH : 0;
                    return width > 0 || height > 0;
                }
                return false;
            }

            // Find the base64 data portion
            int commaIdx = src.IndexOf(',');
            if (commaIdx < 0 || commaIdx >= src.Length - 1)
            {
                return false;
            }

            // Only decode enough bytes to read headers (first ~100 bytes of image)
            string base64 = src.Substring(commaIdx + 1);
            // 100 bytes of image data = ceil(100 * 4/3) = 134 base64 chars
            int charsNeeded = Math.Min(base64.Length, 512);
            // Trim to multiple of 4 for valid base64
            charsNeeded = (charsNeeded / 4) * 4;
            if (charsNeeded < 32)
            {
                return false;
            }

            byte[] data;
            try
            {
                data = Convert.FromBase64String(base64.Substring(0, charsNeeded));
            }
            catch
            {
                return false;
            }

            if (data.Length < 8)
            {
                return false;
            }

            // PNG: IHDR at offset 8, width at 16, height at 20 (big-endian)
            if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            {
                if (data.Length >= 24)
                {
                    width = (data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19];
                    height = (data[20] << 24) | (data[21] << 16) | (data[22] << 8) | data[23];
                    return width > 0 && height > 0;
                }
                return false;
            }

            // JPEG: scan for SOF0/SOF2 marker
            if (data[0] == 0xFF && data[1] == 0xD8)
            {
                int offset = 2;
                while (offset + 9 < data.Length)
                {
                    if (data[offset] != 0xFF)
                    {
                        break;
                    }
                    byte marker = data[offset + 1];
                    if (marker == 0xC0 || marker == 0xC2)
                    {
                        height = (data[offset + 5] << 8) | data[offset + 6];
                        width = (data[offset + 7] << 8) | data[offset + 8];
                        return width > 0 && height > 0;
                    }
                    if (offset + 3 < data.Length)
                    {
                        int segLen = (data[offset + 2] << 8) | data[offset + 3];
                        offset += 2 + segLen;
                    }
                    else
                    {
                        break;
                    }
                }
                return false;
            }

            // GIF: width at 6, height at 8 (little-endian)
            if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data.Length >= 10)
            {
                width = data[6] | (data[7] << 8);
                height = data[8] | (data[9] << 8);
                return width > 0 && height > 0;
            }

            return false;
        }

        /// <summary>
        /// Returns the aspect ratio encoded by an SVG <c>data:</c> URI on an img
        /// element, or 0 if the element is not an img, the src is not an SVG
        /// data URI, or the SVG exposes no usable ratio. Prefers explicit
        /// width/height lengths; falls back to the viewBox ratio.
        /// </summary>
        public static float GetSvgDataUriRatio(StyledElement element)
        {
            string tag = element.TagName;
            if (tag != "img" && tag != "object" && tag != "embed")
            {
                return 0f;
            }
            // <object> references its resource via `data`; <img>/<embed> via `src`.
            string? src = tag == "object" ? element.GetAttribute("data") : element.GetAttribute("src");
            if (src == null || !src.StartsWith("data:image/svg", StringComparison.OrdinalIgnoreCase))
            {
                return 0f;
            }
            if (!TryExtractSvgRootSize(src, out float svgW, out float svgH, out float vbW, out float vbH))
            {
                return 0f;
            }
            if (svgW > 0 && svgH > 0)
            {
                return svgW / svgH;
            }
            if (vbW > 0 && vbH > 0)
            {
                return vbW / vbH;
            }
            return 0f;
        }

        /// <summary>
        /// Parses the root <c>&lt;svg&gt;</c> tag of an SVG <c>data:</c> URI and
        /// extracts width, height (from explicit length attributes) and viewBox
        /// dimensions. Percentage and unitless-with-unsupported-unit values for
        /// width/height are reported as 0, because per CSS they do not establish
        /// an intrinsic size.
        /// <spec>CSS-IMAGES-3 §6 https://drafts.csswg.org/css-images-3/#sizing</spec>
        /// </summary>
        private static bool TryExtractSvgRootSize(string src, out float width, out float height,
                                                    out float viewBoxWidth, out float viewBoxHeight)
        {
            width = 0;
            height = 0;
            viewBoxWidth = 0;
            viewBoxHeight = 0;

            int commaIdx = src.IndexOf(',');
            if (commaIdx < 0 || commaIdx >= src.Length - 1)
            {
                return false;
            }

            string mediaPart = src.Substring(5, commaIdx - 5); // after "data:"
            string payload = src.Substring(commaIdx + 1);
            string svgText;
            if (mediaPart.IndexOf(";base64", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                try
                {
                    byte[] bytes = Convert.FromBase64String(payload);
                    svgText = System.Text.Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                svgText = Uri.UnescapeDataString(payload);
            }

            int svgStart = svgText.IndexOf("<svg", StringComparison.OrdinalIgnoreCase);
            if (svgStart < 0)
            {
                return false;
            }
            int svgEnd = svgText.IndexOf('>', svgStart);
            if (svgEnd < 0)
            {
                return false;
            }
            string rootTag = svgText.Substring(svgStart, svgEnd - svgStart);

            width = ParseSvgLengthAttribute(rootTag, "width");
            height = ParseSvgLengthAttribute(rootTag, "height");

            string? viewBoxValue = GetAttributeValue(rootTag, "viewBox");
            if (viewBoxValue != null)
            {
                string[] parts = viewBoxValue.Split(new[] { ' ', ',', '\t', '\n', '\r' },
                                                     StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4
                    && float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                                       System.Globalization.CultureInfo.InvariantCulture, out float vbW)
                    && float.TryParse(parts[3], System.Globalization.NumberStyles.Float,
                                       System.Globalization.CultureInfo.InvariantCulture, out float vbH))
                {
                    viewBoxWidth = vbW;
                    viewBoxHeight = vbH;
                }
            }

            return width > 0 || height > 0 || (viewBoxWidth > 0 && viewBoxHeight > 0);
        }

        private static float ParseSvgLengthAttribute(string rootTag, string attributeName)
        {
            string? rawValue = GetAttributeValue(rootTag, attributeName);
            if (rawValue == null)
            {
                return 0f;
            }
            string trimmed = rawValue.Trim();
            if (trimmed.Length == 0 || trimmed.EndsWith("%", StringComparison.Ordinal))
            {
                return 0f;
            }
            int end = 0;
            while (end < trimmed.Length)
            {
                char c = trimmed[end];
                if (char.IsDigit(c) || c == '.' || c == '-' || c == '+' || c == 'e' || c == 'E')
                {
                    end++;
                    continue;
                }
                break;
            }
            if (end == 0)
            {
                return 0f;
            }
            string numberPart = trimmed.Substring(0, end);
            if (!float.TryParse(numberPart, System.Globalization.NumberStyles.Float,
                                 System.Globalization.CultureInfo.InvariantCulture, out float value))
            {
                return 0f;
            }
            string unit = trimmed.Substring(end).Trim();
            if (unit.Length == 0 || unit.Equals("px", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
            // SVG absolute length units: 1pt = 1.333px, 1pc = 16px, 1mm = 3.7795px,
            // 1cm = 37.795px, 1in = 96px. Percentages and relative units yield no
            // intrinsic size for replaced-element sizing.
            if (unit.Equals("pt", StringComparison.OrdinalIgnoreCase)) { return value * 1.3333333f; }
            if (unit.Equals("pc", StringComparison.OrdinalIgnoreCase)) { return value * 16f; }
            if (unit.Equals("mm", StringComparison.OrdinalIgnoreCase)) { return value * 3.7795276f; }
            if (unit.Equals("cm", StringComparison.OrdinalIgnoreCase)) { return value * 37.795277f; }
            if (unit.Equals("in", StringComparison.OrdinalIgnoreCase)) { return value * 96f; }
            return 0f;
        }

        private static string? GetAttributeValue(string rootTag, string attributeName)
        {
            int searchIdx = 0;
            while (searchIdx < rootTag.Length)
            {
                int attrIdx = rootTag.IndexOf(attributeName, searchIdx, StringComparison.OrdinalIgnoreCase);
                if (attrIdx < 0)
                {
                    return null;
                }
                // Attribute name must be preceded by whitespace or '<svg' prefix end
                // so that substrings inside other attributes are not matched.
                if (attrIdx > 0)
                {
                    char prev = rootTag[attrIdx - 1];
                    if (prev != ' ' && prev != '\t' && prev != '\n' && prev != '\r')
                    {
                        searchIdx = attrIdx + attributeName.Length;
                        continue;
                    }
                }
                int afterName = attrIdx + attributeName.Length;
                // Skip whitespace and '='
                while (afterName < rootTag.Length && (rootTag[afterName] == ' '
                                                      || rootTag[afterName] == '\t'
                                                      || rootTag[afterName] == '\n'
                                                      || rootTag[afterName] == '\r'))
                {
                    afterName++;
                }
                if (afterName >= rootTag.Length || rootTag[afterName] != '=')
                {
                    searchIdx = attrIdx + attributeName.Length;
                    continue;
                }
                afterName++;
                while (afterName < rootTag.Length && (rootTag[afterName] == ' '
                                                      || rootTag[afterName] == '\t'
                                                      || rootTag[afterName] == '\n'
                                                      || rootTag[afterName] == '\r'))
                {
                    afterName++;
                }
                if (afterName >= rootTag.Length)
                {
                    return null;
                }
                char quote = rootTag[afterName];
                if (quote != '"' && quote != '\'')
                {
                    // Unquoted attribute — read until whitespace or end.
                    int unquotedEnd = afterName;
                    while (unquotedEnd < rootTag.Length
                           && rootTag[unquotedEnd] != ' '
                           && rootTag[unquotedEnd] != '\t'
                           && rootTag[unquotedEnd] != '\n'
                           && rootTag[unquotedEnd] != '\r'
                           && rootTag[unquotedEnd] != '>')
                    {
                        unquotedEnd++;
                    }
                    return rootTag.Substring(afterName, unquotedEnd - afterName);
                }
                int valueStart = afterName + 1;
                int valueEnd = rootTag.IndexOf(quote, valueStart);
                if (valueEnd < 0)
                {
                    return null;
                }
                return rootTag.Substring(valueStart, valueEnd - valueStart);
            }
            return null;
        }

        /// <summary>
        /// Resolve the content dimensions for a replaced element.
        /// </summary>
        public static void ResolveDimensions(LayoutBox box, ComputedStyle style,
                                              float containingWidth, float containingHeight,
                                              float intrinsicWidth, float intrinsicHeight)
        {
            float width = style.Width;
            float height = style.Height;
            // [CSS-SIZING-4 §5.1] CSS aspect-ratio overrides intrinsic ratio. When no CSS
            // aspect-ratio is set we fall back to the element's intrinsic ratio (img dims,
            // SVG viewBox). When neither is available — broken images that report 0×0 with
            // no explicit ratio — the element has no aspect ratio and constraints on one
            // axis must NOT transfer to the other axis. Chrome's
            // LayoutReplaced::ComputeReplacedLogicalWidth follows the same rule.
            // [CSS-SIZING-4 §4] A bare `aspect-ratio: <ratio>` overrides the intrinsic ratio, but
            // `aspect-ratio: auto <ratio>` prefers the element's natural ratio and uses the supplied
            // <ratio> only as a fallback when there is no natural one.
            float cssRatio = DimensionResolver.GetAspectRatio(style);
            bool aspectRatioIsAuto = DimensionResolver.IsAspectRatioAuto(style);
            float ratio;
            if (cssRatio > 0 && !aspectRatioIsAuto)
            {
                ratio = cssRatio;
            }
            else if (intrinsicWidth > 0 && intrinsicHeight > 0)
            {
                ratio = intrinsicWidth / intrinsicHeight;
            }
            else if (box.StyledNode is StyledElement ratioElement)
            {
                // GetIntrinsicRatio returns the natural ratio (svg/img/canvas/object), falling back
                // to the `auto <ratio>` value when the element exposes no natural ratio.
                ratio = GetIntrinsicRatio(ratioElement);
            }
            else
            {
                ratio = cssRatio > 0 ? cssRatio : 0;
            }

            // Form controls (input, select, textarea, meter, progress) do NOT have an
            // intrinsic aspect ratio. When one dimension is specified and the other is
            // auto, use the intrinsic value for the auto dimension, not the ratio.
            bool isFormControl = box.StyledNode is StyledElement el &&
                (el.TagName == "input" || el.TagName == "select" || el.TagName == "textarea"
                 || el.TagName == "meter" || el.TagName == "progress");

            // [CSS-SIZING-3 §5.1] For replaced elements, min-content/max-content/fit-content
            // resolve to the intrinsic size. Treat these sizing keywords as auto so that the
            // standard replaced-element sizing algorithm (CSS 2.1 §10.3.2 / §10.6.2) applies.
            if (SizingKeyword.IsSizingKeyword(width))
            {
                width = float.NaN;
            }
            if (SizingKeyword.IsSizingKeyword(height))
            {
                height = float.NaN;
            }

            // Resolve deferred percentage widths (encoded with sentinel offset)
            if (DeferredPercent.IsEncoded(width))
            {
                width = DeferredPercent.Resolve(width, containingWidth);
                if (style.BoxSizing == CssBoxSizing.BorderBox)
                {
                    width -= (box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth);
                }
            }
            else if (!float.IsNaN(width) && style.BoxSizing == CssBoxSizing.BorderBox)
            {
                // Fixed pixel width with border-box: subtract padding+border
                width -= (box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth);
            }
            // [CSS-SIZING-3 §5.4] Resolve percentage heights against the containing block height.
            // When the containing block height is definite (e.g., flex item with explicit height),
            // the percentage resolves to a concrete value; otherwise it behaves as auto.
            if (DeferredPercent.IsEncoded(height))
            {
                if (!float.IsNaN(containingHeight) && containingHeight > 0)
                {
                    height = DeferredPercent.Resolve(height, containingHeight);
                    if (style.BoxSizing == CssBoxSizing.BorderBox)
                    {
                        height -= (box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth);
                    }
                }
                else
                {
                    height = float.NaN;
                }
            }
            else if (!float.IsNaN(height) && style.BoxSizing == CssBoxSizing.BorderBox)
            {
                // Fixed pixel height with border-box: subtract padding+border
                height -= (box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth);
            }

            if (float.IsNaN(width) && float.IsNaN(height))
            {
                // Use intrinsic dimensions. [CSS-IMAGES-3 §6] When only one
                // intrinsic dimension is known and a natural ratio is available
                // (e.g. SVG with width-only + viewBox), derive the missing side
                // from that ratio before the min/max constraints apply.
                width = intrinsicWidth;
                height = intrinsicHeight;
                if (ratio > 0 && !isFormControl)
                {
                    if (width <= 0 && height > 0)
                    {
                        width = height * ratio;
                    }
                    else if (height <= 0 && width > 0)
                    {
                        height = width / ratio;
                    }
                }
            }
            else if (float.IsNaN(width))
            {
                // Height specified, derive width from ratio (images) or use intrinsic
                // (form controls / replaced elements with no ratio).
                if (isFormControl || ratio <= 0)
                {
                    width = intrinsicWidth;
                }
                else
                {
                    width = height * ratio;
                }
            }
            else if (float.IsNaN(height))
            {
                // Width specified, derive height from ratio (images) or use intrinsic
                // (form controls / replaced elements with no ratio).
                if (isFormControl || ratio <= 0)
                {
                    height = intrinsicHeight;
                }
                else
                {
                    height = width / ratio;
                }
            }

            // Apply min/max constraints — resolve percentage values first
            float minW = style.MinWidth;
            float maxW = style.MaxWidth;
            float minH = style.MinHeight;
            float maxH = style.MaxHeight;

            // [CSS-SIZING-3 §5.1] For replaced elements, intrinsic sizing keywords in
            // min/max constraints resolve to the intrinsic size. When the element has
            // an aspect ratio and the other axis is definite, transfer through the
            // ratio (e.g. min-content width = resolvedHeight * ratio when height is
            // specified). This matches Chrome's LayoutReplaced constraint resolution.
            if (SizingKeyword.IsSizingKeyword(minW))
            {
                minW = (ratio > 0 && !float.IsNaN(height) && height > 0)
                    ? height * ratio
                    : intrinsicWidth;
            }
            if (SizingKeyword.IsSizingKeyword(maxW))
            {
                maxW = (ratio > 0 && !float.IsNaN(height) && height > 0)
                    ? height * ratio
                    : intrinsicWidth;
            }
            if (SizingKeyword.IsSizingKeyword(minH))
            {
                minH = (ratio > 0 && !float.IsNaN(width) && width > 0)
                    ? width / ratio
                    : intrinsicHeight;
            }
            if (SizingKeyword.IsSizingKeyword(maxH))
            {
                maxH = (ratio > 0 && !float.IsNaN(width) && width > 0)
                    ? width / ratio
                    : intrinsicHeight;
            }

            // Resolve deferred percentage min/max (encoded with sentinel offset)
            if (DeferredPercent.IsEncoded(maxW))
            {
                maxW = DeferredPercent.Resolve(maxW, containingWidth);
            }
            if (DeferredPercent.IsEncoded(minW))
            {
                minW = DeferredPercent.Resolve(minW, containingWidth);
            }

            // box-sizing: border-box — min/max height/width values include padding+border,
            // but 'height' and 'width' here are content-box. Subtract padding+border so
            // the comparison is in the same coordinate space.
            if (style.BoxSizing == CssBoxSizing.BorderBox)
            {
                float hAdj = box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth;
                float wAdj = box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth;
                if (!float.IsNaN(minH))
                {
                    minH -= hAdj;
                }
                if (!float.IsNaN(maxH) && maxH > 0)
                {
                    maxH -= hAdj;
                }
                if (!float.IsNaN(minW))
                {
                    minW -= wAdj;
                }
                if (!float.IsNaN(maxW) && maxW > 0)
                {
                    maxW -= wAdj;
                }
            }

            // [CSS2 §10.3.2/10.6.2] Apply min/max constraints and re-derive
            // through aspect-ratio when the other axis was auto or sizing-keyword.
            bool heightIsAuto = float.IsNaN(style.Height) || SizingKeyword.IsSizingKeyword(style.Height);
            bool widthIsAuto = float.IsNaN(style.Width) || SizingKeyword.IsSizingKeyword(style.Width);
            // [CSS2 §10.4] When a constraint on one axis causes the other axis to
            // be re-derived through the aspect ratio, the re-derived value must
            // itself be clamped to its own min/max range. Without this final clamp,
            // a min-height transfer can push width past max-width (or vice versa).
            if (!float.IsNaN(maxW) && maxW > 0 && width > maxW)
            {
                width = maxW;
                if (heightIsAuto && !isFormControl && ratio > 0)
                {
                    height = width / ratio;
                    height = ClampToRange(height, minH, maxH);
                }
            }
            if (!float.IsNaN(minW) && width < minW)
            {
                width = minW;
                if (heightIsAuto && !isFormControl && ratio > 0)
                {
                    height = width / ratio;
                    height = ClampToRange(height, minH, maxH);
                }
            }
            if (!float.IsNaN(maxH) && maxH > 0 && height > maxH)
            {
                height = maxH;
                if (widthIsAuto && !isFormControl && ratio > 0)
                {
                    width = height * ratio;
                    width = ClampToRange(width, minW, maxW);
                }
            }
            if (!float.IsNaN(minH) && height < minH)
            {
                height = minH;
                if (widthIsAuto && !isFormControl && ratio > 0)
                {
                    width = height * ratio;
                    width = ClampToRange(width, minW, maxW);
                }
            }

            box.ContentRect = new Core.Values.RectF(box.ContentRect.X, box.ContentRect.Y,
                                                      Math.Max(0, width), Math.Max(0, height));
        }

        private static float ClampToRange(float value, float min, float max)
        {
            if (!float.IsNaN(max) && max > 0 && value > max)
            {
                value = max;
            }
            if (!float.IsNaN(min) && value < min)
            {
                value = min;
            }
            return value;
        }
    }
}
