using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Rend.Output.Pdf;
using Rend.Output.Image;

namespace Rend
{
    /// <summary>
    /// Default implementation of <see cref="IRenderer"/>.
    /// Executes the full rendering pipeline: HTML parsing, CSS cascade, layout, and output.
    /// Thread-safe and suitable for use as a singleton.
    /// </summary>
    public sealed class HtmlRenderer : IRenderer
    {
        /// <inheritdoc />
        public byte[] ToPdf(string html, RenderOptions? options = null)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            options = options ?? RenderOptions.Default;

            var pdfOptions = new PdfRenderOptions
            {
                GenerateBookmarks = options.GenerateBookmarks,
                GenerateLinks = options.GenerateLinks,
                Title = options.Title,
                Author = options.Author
            };

            var target = new PdfRenderTarget(pdfOptions);
            var pipeline = new RenderPipeline(options);
            var result = pipeline.Execute(html, target);
            return result.Data;
        }

        /// <inheritdoc />
        public void ToPdf(string html, Stream output, RenderOptions? options = null)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            var data = ToPdf(html, options);
            output.Write(data, 0, data.Length);
        }

        /// <inheritdoc />
        public byte[] ToPdf(TextReader html, RenderOptions? options = null)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            return ToPdf(html.ReadToEnd(), options);
        }

        /// <inheritdoc />
        public void ToPdf(TextReader html, Stream output, RenderOptions? options = null)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (output == null) throw new ArgumentNullException(nameof(output));
            ToPdf(html.ReadToEnd(), output, options);
        }

        /// <inheritdoc />
        public async Task<byte[]> ToPdfAsync(string html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            cancellationToken.ThrowIfCancellationRequested();
            options = options ?? RenderOptions.Default;

            var pdfOptions = new PdfRenderOptions
            {
                GenerateBookmarks = options.GenerateBookmarks,
                GenerateLinks = options.GenerateLinks,
                Title = options.Title,
                Author = options.Author
            };

            var target = new PdfRenderTarget(pdfOptions);
            var pipeline = new RenderPipeline(options);
            var result = await pipeline.ExecuteAsync(html, target, cancellationToken).ConfigureAwait(false);
            return result.Data;
        }

        /// <inheritdoc />
        public async Task ToPdfAsync(string html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            var data = await ToPdfAsync(html, options, cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<byte[]> ToPdfAsync(TextReader html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
#if NET8_0_OR_GREATER
            var text = await html.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
#else
            cancellationToken.ThrowIfCancellationRequested();
            var text = await html.ReadToEndAsync().ConfigureAwait(false);
#endif
            return await ToPdfAsync(text, options, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task ToPdfAsync(TextReader html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (output == null) throw new ArgumentNullException(nameof(output));
#if NET8_0_OR_GREATER
            var text = await html.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
#else
            cancellationToken.ThrowIfCancellationRequested();
            var text = await html.ReadToEndAsync().ConfigureAwait(false);
#endif
            await ToPdfAsync(text, output, options, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public byte[] ToImage(string html, RenderOptions? options = null)
        {
            return ToImageResult(html, options).Data;
        }

        /// <summary>
        /// Renders HTML to an image and returns the full result including layout tree snapshot.
        /// </summary>
        public RenderResult ToImageResult(string html, RenderOptions? options = null)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            options = options ?? RenderOptions.Default;

            var skiaOptions = new SkiaRenderOptions
            {
                Dpi = options.Dpi,
                Format = options.ImageFormat,
                Quality = options.ImageQuality
            };

            using var target = new SkiaRenderTarget(skiaOptions, options.FontMapper);
            var pipeline = new RenderPipeline(options);
            return pipeline.Execute(html, target);
        }

        /// <inheritdoc />
        public void ToImage(string html, Stream output, RenderOptions? options = null)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            var data = ToImage(html, options);
            output.Write(data, 0, data.Length);
        }

        /// <inheritdoc />
        public byte[] ToImage(TextReader html, RenderOptions? options = null)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            return ToImage(html.ReadToEnd(), options);
        }

        /// <inheritdoc />
        public void ToImage(TextReader html, Stream output, RenderOptions? options = null)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (output == null) throw new ArgumentNullException(nameof(output));
            ToImage(html.ReadToEnd(), output, options);
        }

        /// <inheritdoc />
        public async Task<byte[]> ToImageAsync(string html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            cancellationToken.ThrowIfCancellationRequested();
            options = options ?? RenderOptions.Default;

            var skiaOptions = new SkiaRenderOptions
            {
                Dpi = options.Dpi,
                Format = options.ImageFormat,
                Quality = options.ImageQuality
            };

            using var target = new SkiaRenderTarget(skiaOptions, options.FontMapper);
            var pipeline = new RenderPipeline(options);
            var result = await pipeline.ExecuteAsync(html, target, cancellationToken).ConfigureAwait(false);
            return result.Data;
        }

        /// <inheritdoc />
        public async Task ToImageAsync(string html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            var data = await ToImageAsync(html, options, cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<byte[]> ToImageAsync(TextReader html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
#if NET8_0_OR_GREATER
            var text = await html.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
#else
            cancellationToken.ThrowIfCancellationRequested();
            var text = await html.ReadToEndAsync().ConfigureAwait(false);
#endif
            return await ToImageAsync(text, options, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task ToImageAsync(TextReader html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (output == null) throw new ArgumentNullException(nameof(output));
#if NET8_0_OR_GREATER
            var text = await html.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
#else
            cancellationToken.ThrowIfCancellationRequested();
            var text = await html.ReadToEndAsync().ConfigureAwait(false);
#endif
            await ToImageAsync(text, output, options, cancellationToken).ConfigureAwait(false);
        }
    }
}
