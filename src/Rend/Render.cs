using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rend
{
    /// <summary>
    /// Static convenience API for rendering HTML/CSS to PDF or images.
    /// For dependency injection or testing, use <see cref="IRenderer"/> and <see cref="HtmlRenderer"/> directly.
    /// </summary>
    public static class Render
    {
        private static readonly HtmlRenderer Instance = new HtmlRenderer();

        /// <inheritdoc cref="IRenderer.ToPdf(string, RenderOptions?)"/>
        public static byte[] ToPdf(string html, RenderOptions? options = null)
            => Instance.ToPdf(html, options);

        /// <inheritdoc cref="IRenderer.ToPdf(string, Stream, RenderOptions?)"/>
        public static void ToPdf(string html, Stream output, RenderOptions? options = null)
            => Instance.ToPdf(html, output, options);

        /// <inheritdoc cref="IRenderer.ToPdf(TextReader, RenderOptions?)"/>
        public static byte[] ToPdf(TextReader html, RenderOptions? options = null)
            => Instance.ToPdf(html, options);

        /// <inheritdoc cref="IRenderer.ToPdf(TextReader, Stream, RenderOptions?)"/>
        public static void ToPdf(TextReader html, Stream output, RenderOptions? options = null)
            => Instance.ToPdf(html, output, options);

        /// <inheritdoc cref="IRenderer.ToPdfAsync(string, RenderOptions?, CancellationToken)"/>
        public static Task<byte[]> ToPdfAsync(string html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToPdfAsync(html, options, cancellationToken);

        /// <inheritdoc cref="IRenderer.ToPdfAsync(string, Stream, RenderOptions?, CancellationToken)"/>
        public static Task ToPdfAsync(string html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToPdfAsync(html, output, options, cancellationToken);

        /// <inheritdoc cref="IRenderer.ToPdfAsync(TextReader, RenderOptions?, CancellationToken)"/>
        public static Task<byte[]> ToPdfAsync(TextReader html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToPdfAsync(html, options, cancellationToken);

        /// <inheritdoc cref="IRenderer.ToPdfAsync(TextReader, Stream, RenderOptions?, CancellationToken)"/>
        public static Task ToPdfAsync(TextReader html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToPdfAsync(html, output, options, cancellationToken);

        /// <inheritdoc cref="IRenderer.ToImage(string, RenderOptions?)"/>
        public static byte[] ToImage(string html, RenderOptions? options = null)
            => Instance.ToImage(html, options);

        /// <summary>Renders HTML to an image with full result (including layout tree snapshot).</summary>
        public static RenderResult ToImageResult(string html, RenderOptions? options = null)
            => Instance.ToImageResult(html, options);

        /// <inheritdoc cref="IRenderer.ToImage(string, Stream, RenderOptions?)"/>
        public static void ToImage(string html, Stream output, RenderOptions? options = null)
            => Instance.ToImage(html, output, options);

        /// <inheritdoc cref="IRenderer.ToImage(TextReader, RenderOptions?)"/>
        public static byte[] ToImage(TextReader html, RenderOptions? options = null)
            => Instance.ToImage(html, options);

        /// <inheritdoc cref="IRenderer.ToImage(TextReader, Stream, RenderOptions?)"/>
        public static void ToImage(TextReader html, Stream output, RenderOptions? options = null)
            => Instance.ToImage(html, output, options);

        /// <inheritdoc cref="IRenderer.ToImageAsync(string, RenderOptions?, CancellationToken)"/>
        public static Task<byte[]> ToImageAsync(string html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToImageAsync(html, options, cancellationToken);

        /// <inheritdoc cref="IRenderer.ToImageAsync(string, Stream, RenderOptions?, CancellationToken)"/>
        public static Task ToImageAsync(string html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToImageAsync(html, output, options, cancellationToken);

        /// <inheritdoc cref="IRenderer.ToImageAsync(TextReader, RenderOptions?, CancellationToken)"/>
        public static Task<byte[]> ToImageAsync(TextReader html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToImageAsync(html, options, cancellationToken);

        /// <inheritdoc cref="IRenderer.ToImageAsync(TextReader, Stream, RenderOptions?, CancellationToken)"/>
        public static Task ToImageAsync(TextReader html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToImageAsync(html, output, options, cancellationToken);
    }
}
