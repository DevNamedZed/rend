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
    /// </summary>
    public sealed class HtmlRenderer : IRenderer
    {
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

        public void ToPdf(string html, Stream output, RenderOptions? options = null)
        {
            var data = ToPdf(html, options);
            output.Write(data, 0, data.Length);
        }

        public byte[] ToPdf(TextReader html, RenderOptions? options = null)
            => ToPdf(html.ReadToEnd(), options);

        public void ToPdf(TextReader html, Stream output, RenderOptions? options = null)
            => ToPdf(html.ReadToEnd(), output, options);

        public Task<byte[]> ToPdfAsync(string html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() => ToPdf(html, options), cancellationToken);
        }

        public async Task ToPdfAsync(string html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var data = await ToPdfAsync(html, options, cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        }

        public async Task<byte[]> ToPdfAsync(TextReader html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var text = await html.ReadToEndAsync().ConfigureAwait(false);
            return await ToPdfAsync(text, options, cancellationToken).ConfigureAwait(false);
        }

        public async Task ToPdfAsync(TextReader html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var text = await html.ReadToEndAsync().ConfigureAwait(false);
            await ToPdfAsync(text, output, options, cancellationToken).ConfigureAwait(false);
        }

        public byte[] ToImage(string html, RenderOptions? options = null)
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
            var result = pipeline.Execute(html, target);
            return result.Data;
        }

        public void ToImage(string html, Stream output, RenderOptions? options = null)
        {
            var data = ToImage(html, options);
            output.Write(data, 0, data.Length);
        }

        public byte[] ToImage(TextReader html, RenderOptions? options = null)
            => ToImage(html.ReadToEnd(), options);

        public void ToImage(TextReader html, Stream output, RenderOptions? options = null)
            => ToImage(html.ReadToEnd(), output, options);

        public Task<byte[]> ToImageAsync(string html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() => ToImage(html, options), cancellationToken);
        }

        public async Task ToImageAsync(string html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var data = await ToImageAsync(html, options, cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        }

        public async Task<byte[]> ToImageAsync(TextReader html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var text = await html.ReadToEndAsync().ConfigureAwait(false);
            return await ToImageAsync(text, options, cancellationToken).ConfigureAwait(false);
        }

        public async Task ToImageAsync(TextReader html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var text = await html.ReadToEndAsync().ConfigureAwait(false);
            await ToImageAsync(text, output, options, cancellationToken).ConfigureAwait(false);
        }
    }
}
