using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
        /// Asynchronously resolve an image URL to a readable stream of image bytes.
        /// Return null to skip the image or fall back to the default resource loader.
        /// The caller will dispose the returned stream.
        /// </summary>
        /// <param name="url">The image URL (absolute or relative).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A stream containing the image data, or null.</returns>
        Task<Stream?> ResolveAsync(string url, CancellationToken cancellationToken = default);
    }
}
