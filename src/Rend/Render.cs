using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rend
{
    /// <summary>
    /// Static convenience facade over <see cref="HtmlRenderer"/>.
    /// For DI or testing, use <see cref="IRenderer"/> and <see cref="HtmlRenderer"/> directly.
    /// </summary>
    public static class Render
    {
        private static readonly HtmlRenderer Instance = new HtmlRenderer();

        public static byte[] ToPdf(string html, RenderOptions? options = null)
            => Instance.ToPdf(html, options);

        public static void ToPdf(string html, Stream output, RenderOptions? options = null)
            => Instance.ToPdf(html, output, options);

        public static byte[] ToPdf(TextReader html, RenderOptions? options = null)
            => Instance.ToPdf(html, options);

        public static void ToPdf(TextReader html, Stream output, RenderOptions? options = null)
            => Instance.ToPdf(html, output, options);

        public static Task<byte[]> ToPdfAsync(string html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToPdfAsync(html, options, cancellationToken);

        public static Task ToPdfAsync(string html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToPdfAsync(html, output, options, cancellationToken);

        public static Task<byte[]> ToPdfAsync(TextReader html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToPdfAsync(html, options, cancellationToken);

        public static Task ToPdfAsync(TextReader html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToPdfAsync(html, output, options, cancellationToken);

        public static byte[] ToImage(string html, RenderOptions? options = null)
            => Instance.ToImage(html, options);

        public static void ToImage(string html, Stream output, RenderOptions? options = null)
            => Instance.ToImage(html, output, options);

        public static byte[] ToImage(TextReader html, RenderOptions? options = null)
            => Instance.ToImage(html, options);

        public static void ToImage(TextReader html, Stream output, RenderOptions? options = null)
            => Instance.ToImage(html, output, options);

        public static Task<byte[]> ToImageAsync(string html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToImageAsync(html, options, cancellationToken);

        public static Task ToImageAsync(string html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToImageAsync(html, output, options, cancellationToken);

        public static Task<byte[]> ToImageAsync(TextReader html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToImageAsync(html, options, cancellationToken);

        public static Task ToImageAsync(TextReader html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
            => Instance.ToImageAsync(html, output, options, cancellationToken);
    }
}
