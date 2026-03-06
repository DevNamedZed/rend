using System;
using Rend.Css;
using Rend.Style;

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
        public static float GetFormControlIntrinsicWidth(StyledElement element)
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
                        // Chrome: 13.333px sans-serif, ~6px avg char width + 12px padding + 4px border
                        string? value = element.GetAttribute("value");
                        if (string.IsNullOrEmpty(value))
                        {
                            value = inputType == "submit" ? "Submit"
                                  : inputType == "reset" ? "Reset"
                                  : "Button";
                        }
                        return Math.Max(40f, value!.Length * 6.1f + 16f);
                    default:
                        // text, password, email, url, search, tel, number, etc.
                        // Chrome: size=20 chars * ~6.7px avg char width + 4px border + 2px padding ≈ 140px
                        return 140f;
                }
            }

            if (tag == "select")
                return 140f;

            if (tag == "textarea")
            {
                string? cols = element.GetAttribute("cols");
                if (cols != null && int.TryParse(cols, out int c) && c > 0)
                    return c * 8f; // ~8px per column character
                return 200f;
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
                        return 21f;
                }
            }

            if (tag == "select")
                return 21f;

            if (tag == "textarea")
            {
                string? rows = element.GetAttribute("rows");
                if (rows != null && int.TryParse(rows, out int r) && r > 0)
                    return r * 16f; // ~16px per row
                return 60f;
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
