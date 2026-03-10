using System.IO;

namespace Rend
{
    /// <summary>
    /// Resolves image URLs to their content. Called for every image encountered
    /// in HTML (<c>&lt;img src&gt;</c>) or CSS (<c>background-image</c>,
    /// <c>border-image</c>, <c>list-style-image</c>).
    /// </summary>
    public interface IImageResolver
    {
        /// <summary>
        /// Resolve an image URL to a readable stream of image bytes.
        /// Return null to skip the image or fall back to the default resource loader.
        /// The caller will dispose the returned stream.
        /// </summary>
        /// <param name="url">The image URL (absolute or relative).</param>
        /// <returns>A stream containing the image data, or null.</returns>
        Stream? Resolve(string url);
    }
}
