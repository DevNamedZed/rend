using Rend.Layout;
using Rend.Style;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints replaced element content (such as images) by drawing
    /// the associated <see cref="ImageData"/> into the content rect.
    /// </summary>
    internal static class ReplacedContentPainter
    {
        /// <summary>
        /// If the layout box represents a replaced element (e.g. &lt;img&gt;),
        /// draws the image content into the box's content rectangle.
        /// </summary>
        /// <param name="box">The layout box to paint.</param>
        /// <param name="target">The render target to draw on.</param>
        /// <param name="imageResolver">
        /// An optional delegate that resolves a source URL to image data.
        /// When null, no image is drawn.
        /// </param>
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

            target.DrawImage(imageData, box.ContentRect);
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
