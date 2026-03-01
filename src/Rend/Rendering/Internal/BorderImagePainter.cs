using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints CSS border-image onto a layout box.
    /// Supports gradient sources and image sources with stretch rendering.
    /// When a border-image is active, it replaces the standard border painting.
    /// </summary>
    internal static class BorderImagePainter
    {
        /// <summary>
        /// Returns true if this box has a border-image set (and border painting should be skipped).
        /// </summary>
        public static bool HasBorderImage(LayoutBox box)
        {
            if (box.StyledNode?.Style == null) return false;
            var source = box.StyledNode.Style.GetRefValue(PropertyId.BorderImageSource);
            if (source == null) return false;
            if (source is CssKeywordValue kw && kw.Keyword == "none") return false;
            return true;
        }

        public static void Paint(LayoutBox box, IRenderTarget target,
            ImageResolverDelegate? imageResolver = null)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null) return;

            var source = style.GetRefValue(PropertyId.BorderImageSource);
            if (source == null) return;
            if (source is CssKeywordValue kw && kw.Keyword == "none") return;

            RectF borderRect = box.BorderRect;
            RectF paddingRect = box.PaddingRect;

            // Determine the border widths
            float topW = style.BorderTopWidth;
            float rightW = style.BorderRightWidth;
            float bottomW = style.BorderBottomWidth;
            float leftW = style.BorderLeftWidth;

            if (topW <= 0 && rightW <= 0 && bottomW <= 0 && leftW <= 0)
                return;

            // Parse border-image-outset
            float outset = ResolveOutset(style.GetRefValue(PropertyId.BorderImageOutset));
            RectF imageArea = new RectF(
                borderRect.X - outset,
                borderRect.Y - outset,
                borderRect.Width + outset * 2,
                borderRect.Height + outset * 2);

            // Gradient source
            if (source is CssFunctionValue gradientFn)
            {
                PaintGradientBorder(target, gradientFn, imageArea, topW, rightW, bottomW, leftW);
                return;
            }

            // URL source
            string? url = source as string;
            if (url == null && source is CssUrlValue urlVal) url = urlVal.Url;
            if (url == null && source is CssStringValue strVal) url = strVal.Value;
            if (string.IsNullOrEmpty(url) || imageResolver == null) return;

            ImageData? imageData = imageResolver(url!);
            if (imageData == null || imageData.Width <= 0 || imageData.Height <= 0) return;

            PaintImageBorder(target, imageData, imageArea, topW, rightW, bottomW, leftW);
        }

        private static void PaintGradientBorder(IRenderTarget target, CssFunctionValue fn,
            RectF area, float top, float right, float bottom, float left)
        {
            // Paint the border area as a filled gradient clipped to just the border regions
            // (border rect minus padding rect)
            var outerPath = new PathData();
            outerPath.AddRectangle(area);

            var innerRect = new RectF(
                area.X + left,
                area.Y + top,
                area.Width - left - right,
                area.Height - top - bottom);

            // Create an exclusion path: outer rect followed by reversed inner rect
            // to fill only the border area
            var borderPath = new PathData();
            // Outer clockwise
            borderPath.MoveTo(area.X, area.Y);
            borderPath.LineTo(area.X + area.Width, area.Y);
            borderPath.LineTo(area.X + area.Width, area.Y + area.Height);
            borderPath.LineTo(area.X, area.Y + area.Height);
            borderPath.Close();
            // Inner counter-clockwise (to create hollow)
            borderPath.MoveTo(innerRect.X, innerRect.Y);
            borderPath.LineTo(innerRect.X, innerRect.Y + innerRect.Height);
            borderPath.LineTo(innerRect.X + innerRect.Width, innerRect.Y + innerRect.Height);
            borderPath.LineTo(innerRect.X + innerRect.Width, innerRect.Y);
            borderPath.Close();

            // For now, use the first and last colors of the gradient as a solid fill
            CssColor? color = ExtractFirstGradientColor(fn);
            if (color.HasValue)
            {
                target.FillPath(borderPath, BrushInfo.Solid(color.Value));
            }
        }

        private static void PaintImageBorder(IRenderTarget target, ImageData image,
            RectF area, float top, float right, float bottom, float left)
        {
            // 9-slice: divide source image and destination area into 9 regions
            float imgW = image.Width;
            float imgH = image.Height;

            // Default slice: use border widths as percentage of image dimensions
            float sliceTop = Math.Min(top / area.Height * imgH, imgH / 2);
            float sliceRight = Math.Min(right / area.Width * imgW, imgW / 2);
            float sliceBottom = Math.Min(bottom / area.Height * imgH, imgH / 2);
            float sliceLeft = Math.Min(left / area.Width * imgW, imgW / 2);

            // Draw the full image stretched to the border area
            // (simplified rendering - full 9-slice would require image slicing support)
            target.Save();

            // Clip to just the border area
            var borderPath = new PathData();
            borderPath.MoveTo(area.X, area.Y);
            borderPath.LineTo(area.X + area.Width, area.Y);
            borderPath.LineTo(area.X + area.Width, area.Y + area.Height);
            borderPath.LineTo(area.X, area.Y + area.Height);
            borderPath.Close();
            var innerRect = new RectF(
                area.X + left, area.Y + top,
                area.Width - left - right, area.Height - top - bottom);
            borderPath.MoveTo(innerRect.X, innerRect.Y);
            borderPath.LineTo(innerRect.X, innerRect.Y + innerRect.Height);
            borderPath.LineTo(innerRect.X + innerRect.Width, innerRect.Y + innerRect.Height);
            borderPath.LineTo(innerRect.X + innerRect.Width, innerRect.Y);
            borderPath.Close();

            target.PushClipPath(borderPath);
            target.DrawImage(image, area);
            target.PopClip();

            target.Restore();
        }

        private static CssColor? ExtractFirstGradientColor(CssFunctionValue fn)
        {
            for (int i = 0; i < fn.Arguments.Count; i++)
            {
                if (fn.Arguments[i] is CssColorValue cv)
                    return cv.Color;
                if (fn.Arguments[i] is CssKeywordValue kw)
                {
                    if (Rend.Css.Parser.Internal.CssColorParser.TryParseNamed(kw.Keyword, out var named))
                        return named;
                }
            }
            return null;
        }

        private static float ResolveOutset(object? raw)
        {
            if (raw is CssNumberValue num) return num.Value;
            if (raw is CssDimensionValue dim) return dim.Value;
            return 0;
        }
    }
}
