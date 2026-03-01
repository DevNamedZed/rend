using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Layout;
using Rend.Style;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints replaced element content (such as images) by drawing
    /// the associated <see cref="ImageData"/> into the content rect.
    /// Respects the CSS object-fit property.
    /// </summary>
    internal static class ReplacedContentPainter
    {
        /// <summary>
        /// If the layout box represents a replaced element (e.g. &lt;img&gt;),
        /// draws the image content into the box's content rectangle.
        /// </summary>
        public static void Paint(LayoutBox box, IRenderTarget target, ImageResolverDelegate? imageResolver)
        {
            if (box.StyledNode == null || box.StyledNode.IsText)
            {
                return;
            }

            StyledElement? element = box.StyledNode as StyledElement;
            if (element == null)
            {
                return;
            }

            string tagName = element.TagName;
            if (tagName != "img")
            {
                return;
            }

            string? src = element.GetAttribute("src");
            if (src == null)
            {
                return;
            }

            if (imageResolver == null)
            {
                return;
            }

            ImageData? imageData = imageResolver(src);
            if (imageData == null)
            {
                return;
            }

            RectF contentRect = box.ContentRect;
            CssObjectFit objectFit = element.Style.ObjectFit;
            var (posX, posY) = ParseObjectPosition(element.Style);
            RectF destRect = ComputeObjectFitRect(contentRect, imageData.Width, imageData.Height, objectFit, posX, posY);

            // Clip to content rect when the image may overflow (cover, none, scale-down)
            bool needsClip = objectFit == CssObjectFit.Cover || objectFit == CssObjectFit.None || objectFit == CssObjectFit.ScaleDown;
            if (needsClip)
            {
                target.PushClipRect(contentRect);
            }

            // Apply image-rendering hint
            var imageRendering = element.Style.ImageRendering;
            if (imageRendering != CssImageRendering.Auto)
                target.SetImageRendering(imageRendering);

            target.DrawImage(imageData, destRect);

            // Restore image-rendering
            if (imageRendering != CssImageRendering.Auto)
                target.SetImageRendering(CssImageRendering.Auto);

            if (needsClip)
            {
                target.PopClip();
            }
        }

        /// <summary>
        /// Parse the object-position property from a computed style.
        /// Returns normalized position as fractions (0.0 = left/top, 1.0 = right/bottom).
        /// Default: (0.5, 0.5) = center.
        /// </summary>
        private static (float x, float y) ParseObjectPosition(ComputedStyle style)
        {
            var raw = style.GetRefValue(Css.Properties.Internal.PropertyId.ObjectPosition);
            if (raw == null) return (0.5f, 0.5f);

            if (raw is CssListValue list && list.Values.Count >= 2)
            {
                float x = ResolvePositionComponent(list.Values[0]);
                float y = ResolvePositionComponent(list.Values[1]);
                return (x, y);
            }

            if (raw is CssPercentageValue pct)
                return (pct.Value / 100f, 0.5f);

            if (raw is CssKeywordValue kw)
            {
                float v = KeywordToFraction(kw.Keyword);
                return (v, 0.5f);
            }

            return (0.5f, 0.5f);
        }

        private static float ResolvePositionComponent(CssValue val)
        {
            if (val is CssPercentageValue pct) return pct.Value / 100f;
            if (val is CssKeywordValue kw) return KeywordToFraction(kw.Keyword);
            if (val is CssDimensionValue dim) return 0.5f; // px values need container size
            if (val is CssNumberValue num) return num.Value == 0 ? 0f : 0.5f;
            return 0.5f;
        }

        private static float KeywordToFraction(string keyword)
        {
            switch (keyword)
            {
                case "left":
                case "top": return 0f;
                case "center": return 0.5f;
                case "right":
                case "bottom": return 1f;
                default: return 0.5f;
            }
        }

        private static RectF ComputeObjectFitRect(RectF contentRect, float imgW, float imgH,
            CssObjectFit fit, float posX = 0.5f, float posY = 0.5f)
        {
            if (imgW <= 0 || imgH <= 0)
            {
                return contentRect;
            }

            switch (fit)
            {
                case CssObjectFit.Contain:
                {
                    float ratioW = contentRect.Width / imgW;
                    float ratioH = contentRect.Height / imgH;
                    float ratio = Math.Min(ratioW, ratioH);
                    float scaledW = imgW * ratio;
                    float scaledH = imgH * ratio;
                    float x = contentRect.X + (contentRect.Width - scaledW) * posX;
                    float y = contentRect.Y + (contentRect.Height - scaledH) * posY;
                    return new RectF(x, y, scaledW, scaledH);
                }

                case CssObjectFit.Cover:
                {
                    float ratioW = contentRect.Width / imgW;
                    float ratioH = contentRect.Height / imgH;
                    float ratio = Math.Max(ratioW, ratioH);
                    float scaledW = imgW * ratio;
                    float scaledH = imgH * ratio;
                    float x = contentRect.X + (contentRect.Width - scaledW) * posX;
                    float y = contentRect.Y + (contentRect.Height - scaledH) * posY;
                    return new RectF(x, y, scaledW, scaledH);
                }

                case CssObjectFit.None:
                {
                    float x = contentRect.X + (contentRect.Width - imgW) * posX;
                    float y = contentRect.Y + (contentRect.Height - imgH) * posY;
                    return new RectF(x, y, imgW, imgH);
                }

                case CssObjectFit.ScaleDown:
                {
                    if (imgW <= contentRect.Width && imgH <= contentRect.Height)
                    {
                        float x = contentRect.X + (contentRect.Width - imgW) * posX;
                        float y = contentRect.Y + (contentRect.Height - imgH) * posY;
                        return new RectF(x, y, imgW, imgH);
                    }
                    else
                    {
                        float ratioW = contentRect.Width / imgW;
                        float ratioH = contentRect.Height / imgH;
                        float ratio = Math.Min(ratioW, ratioH);
                        float scaledW = imgW * ratio;
                        float scaledH = imgH * ratio;
                        float x = contentRect.X + (contentRect.Width - scaledW) * posX;
                        float y = contentRect.Y + (contentRect.Height - scaledH) * posY;
                        return new RectF(x, y, scaledW, scaledH);
                    }
                }

                default: // Fill
                    return contentRect;
            }
        }
    }

    /// <summary>
    /// A delegate that resolves a source URL to an <see cref="ImageData"/> instance,
    /// or returns null if the image could not be loaded.
    /// </summary>
    /// <param name="src">The image source URL or data URI.</param>
    /// <returns>The resolved image data, or null.</returns>
    internal delegate ImageData? ImageResolverDelegate(string src);
}
