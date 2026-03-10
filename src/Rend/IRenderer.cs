using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rend
{
    /// <summary>
    /// Renders HTML/CSS content to PDF documents or raster images.
    /// Register as a singleton in DI containers. Thread-safe.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>Renders HTML to a PDF document and returns the PDF bytes.</summary>
        /// <param name="html">The HTML content to render.</param>
        /// <param name="options">Optional rendering configuration (page size, margins, fonts, etc.).</param>
        /// <returns>The rendered PDF as a byte array.</returns>
        byte[] ToPdf(string html, RenderOptions? options = null);

        /// <summary>Renders HTML to a PDF document and writes it to the specified stream.</summary>
        /// <param name="html">The HTML content to render.</param>
        /// <param name="output">The stream to write the PDF to.</param>
        /// <param name="options">Optional rendering configuration.</param>
        void ToPdf(string html, Stream output, RenderOptions? options = null);

        /// <summary>Renders HTML from a <see cref="TextReader"/> to a PDF document.</summary>
        /// <param name="html">A reader providing the HTML content.</param>
        /// <param name="options">Optional rendering configuration.</param>
        /// <returns>The rendered PDF as a byte array.</returns>
        byte[] ToPdf(TextReader html, RenderOptions? options = null);

        /// <summary>Renders HTML from a <see cref="TextReader"/> to a PDF document and writes it to the specified stream.</summary>
        /// <param name="html">A reader providing the HTML content.</param>
        /// <param name="output">The stream to write the PDF to.</param>
        /// <param name="options">Optional rendering configuration.</param>
        void ToPdf(TextReader html, Stream output, RenderOptions? options = null);

        /// <summary>Asynchronously renders HTML to a PDF document.</summary>
        /// <param name="html">The HTML content to render.</param>
        /// <param name="options">Optional rendering configuration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The rendered PDF as a byte array.</returns>
        Task<byte[]> ToPdfAsync(string html, RenderOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously renders HTML to a PDF document and writes it to the specified stream.</summary>
        /// <param name="html">The HTML content to render.</param>
        /// <param name="output">The stream to write the PDF to.</param>
        /// <param name="options">Optional rendering configuration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task ToPdfAsync(string html, Stream output, RenderOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously renders HTML from a <see cref="TextReader"/> to a PDF document.</summary>
        /// <param name="html">A reader providing the HTML content.</param>
        /// <param name="options">Optional rendering configuration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The rendered PDF as a byte array.</returns>
        Task<byte[]> ToPdfAsync(TextReader html, RenderOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously renders HTML from a <see cref="TextReader"/> to a PDF document and writes it to the specified stream.</summary>
        /// <param name="html">A reader providing the HTML content.</param>
        /// <param name="output">The stream to write the PDF to.</param>
        /// <param name="options">Optional rendering configuration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task ToPdfAsync(TextReader html, Stream output, RenderOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>Renders HTML to a raster image (PNG, JPEG, or WebP) and returns the image bytes.</summary>
        /// <param name="html">The HTML content to render.</param>
        /// <param name="options">Optional rendering configuration (DPI, format, quality, etc.).</param>
        /// <returns>The rendered image as a byte array.</returns>
        byte[] ToImage(string html, RenderOptions? options = null);

        /// <summary>Renders HTML to a raster image and writes it to the specified stream.</summary>
        /// <param name="html">The HTML content to render.</param>
        /// <param name="output">The stream to write the image to.</param>
        /// <param name="options">Optional rendering configuration.</param>
        void ToImage(string html, Stream output, RenderOptions? options = null);

        /// <summary>Renders HTML from a <see cref="TextReader"/> to a raster image.</summary>
        /// <param name="html">A reader providing the HTML content.</param>
        /// <param name="options">Optional rendering configuration.</param>
        /// <returns>The rendered image as a byte array.</returns>
        byte[] ToImage(TextReader html, RenderOptions? options = null);

        /// <summary>Renders HTML from a <see cref="TextReader"/> to a raster image and writes it to the specified stream.</summary>
        /// <param name="html">A reader providing the HTML content.</param>
        /// <param name="output">The stream to write the image to.</param>
        /// <param name="options">Optional rendering configuration.</param>
        void ToImage(TextReader html, Stream output, RenderOptions? options = null);

        /// <summary>Asynchronously renders HTML to a raster image.</summary>
        /// <param name="html">The HTML content to render.</param>
        /// <param name="options">Optional rendering configuration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The rendered image as a byte array.</returns>
        Task<byte[]> ToImageAsync(string html, RenderOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously renders HTML to a raster image and writes it to the specified stream.</summary>
        /// <param name="html">The HTML content to render.</param>
        /// <param name="output">The stream to write the image to.</param>
        /// <param name="options">Optional rendering configuration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task ToImageAsync(string html, Stream output, RenderOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously renders HTML from a <see cref="TextReader"/> to a raster image.</summary>
        /// <param name="html">A reader providing the HTML content.</param>
        /// <param name="options">Optional rendering configuration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The rendered image as a byte array.</returns>
        Task<byte[]> ToImageAsync(TextReader html, RenderOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously renders HTML from a <see cref="TextReader"/> to a raster image and writes it to the specified stream.</summary>
        /// <param name="html">A reader providing the HTML content.</param>
        /// <param name="output">The stream to write the image to.</param>
        /// <param name="options">Optional rendering configuration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task ToImageAsync(TextReader html, Stream output, RenderOptions? options = null, CancellationToken cancellationToken = default);
    }
}
