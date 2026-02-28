using System;
using Rend.Css;
using Rend.Style;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Layout for replaced elements (&lt;img&gt;, &lt;svg&gt;) with intrinsic dimensions.
    /// </summary>
    internal static class ReplacedElementLayout
    {
        /// <summary>
        /// Returns true if this is a replaced element.
        /// </summary>
        public static bool IsReplaced(StyledElement element)
        {
            string tag = element.TagName;
            return tag == "img" || tag == "svg" || tag == "video" ||
                   tag == "canvas" || tag == "iframe" || tag == "object" || tag == "embed";
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

            if (float.IsNaN(width) && float.IsNaN(height))
            {
                // Use intrinsic dimensions
                width = intrinsicWidth;
                height = intrinsicHeight;
            }
            else if (float.IsNaN(width))
            {
                // Height specified, derive width
                width = height * ratio;
            }
            else if (float.IsNaN(height))
            {
                // Width specified, derive height
                height = ratio > 0 ? width / ratio : width;
            }

            // Apply min/max constraints
            float minW = style.MinWidth;
            float maxW = style.MaxWidth;
            float minH = style.MinHeight;
            float maxH = style.MaxHeight;

            if (!float.IsNaN(maxW) && maxW > 0 && width > maxW)
            {
                width = maxW;
                if (float.IsNaN(style.Height))
                    height = ratio > 0 ? width / ratio : width;
            }
            if (!float.IsNaN(minW) && width < minW)
            {
                width = minW;
                if (float.IsNaN(style.Height))
                    height = ratio > 0 ? width / ratio : width;
            }
            if (!float.IsNaN(maxH) && maxH > 0 && height > maxH)
            {
                height = maxH;
                if (float.IsNaN(style.Width))
                    width = height * ratio;
            }
            if (!float.IsNaN(minH) && height < minH)
            {
                height = minH;
                if (float.IsNaN(style.Width))
                    width = height * ratio;
            }

            box.ContentRect = new Core.Values.RectF(box.ContentRect.X, box.ContentRect.Y,
                                                      Math.Max(0, width), Math.Max(0, height));
        }
    }
}
