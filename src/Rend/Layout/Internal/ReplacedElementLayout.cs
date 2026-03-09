using System;
using Rend.Css;
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
                        // Chrome: size=20 chars * ~6.7px avg char width ≈ 134px content-box
                        // Total 140px with 2px border + 1px padding on each side (set in UA CSS)
                        return 134f;
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

            if (tag == "video" || tag == "canvas")
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

            if (tag == "video" || tag == "canvas")
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
        /// Resolve the content dimensions for a replaced element.
        /// </summary>
        public static void ResolveDimensions(LayoutBox box, ComputedStyle style,
                                              float containingWidth, float intrinsicWidth, float intrinsicHeight)
        {
            float width = style.Width;
            float height = style.Height;
            float ratio = intrinsicHeight > 0 ? intrinsicWidth / intrinsicHeight : 1f;

            // Form controls (input, select, textarea, meter, progress) do NOT have an
            // intrinsic aspect ratio. When one dimension is specified and the other is
            // auto, use the intrinsic value for the auto dimension, not the ratio.
            bool isFormControl = box.StyledNode is StyledElement el &&
                (el.TagName == "input" || el.TagName == "select" || el.TagName == "textarea"
                 || el.TagName == "meter" || el.TagName == "progress");

            // Resolve deferred percentage widths (encoded as negative fractions)
            if (width < 0 && width > -1.01f)
            {
                width = -width * containingWidth;
                if (style.BoxSizing == CssBoxSizing.BorderBox)
                    width -= (box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth);
            }
            if (height < 0 && height > -1.01f)
                height = float.NaN; // percentage heights without containing block → auto

            if (float.IsNaN(width) && float.IsNaN(height))
            {
                // Use intrinsic dimensions
                width = intrinsicWidth;
                height = intrinsicHeight;
            }
            else if (float.IsNaN(width))
            {
                // Height specified, derive width from ratio (images) or use intrinsic (form controls)
                width = isFormControl ? intrinsicWidth : height * ratio;
            }
            else if (float.IsNaN(height))
            {
                // Width specified, derive height from ratio (images) or use intrinsic (form controls)
                height = isFormControl ? intrinsicHeight : (ratio > 0 ? width / ratio : width);
            }

            // Apply min/max constraints
            float minW = style.MinWidth;
            float maxW = style.MaxWidth;
            float minH = style.MinHeight;
            float maxH = style.MaxHeight;

            if (!float.IsNaN(maxW) && maxW > 0 && width > maxW)
            {
                width = maxW;
                if (float.IsNaN(style.Height) && !isFormControl)
                    height = ratio > 0 ? width / ratio : width;
            }
            if (!float.IsNaN(minW) && width < minW)
            {
                width = minW;
                if (float.IsNaN(style.Height) && !isFormControl)
                    height = ratio > 0 ? width / ratio : width;
            }
            if (!float.IsNaN(maxH) && maxH > 0 && height > maxH)
            {
                height = maxH;
                if (float.IsNaN(style.Width) && !isFormControl)
                    width = height * ratio;
            }
            if (!float.IsNaN(minH) && height < minH)
            {
                height = minH;
                if (float.IsNaN(style.Width) && !isFormControl)
                    width = height * ratio;
            }

            box.ContentRect = new Core.Values.RectF(box.ContentRect.X, box.ContentRect.Y,
                                                      Math.Max(0, width), Math.Max(0, height));
        }
    }
}
