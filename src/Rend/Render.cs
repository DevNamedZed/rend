using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Rend.Output.Pdf;
using Rend.Output.Image;

namespace Rend
{
    /// <summary>
    /// Static entry point for the Rend rendering engine.
    /// Converts HTML to PDF or image output.
    /// </summary>
    public static class Render
    {
        /// <summary>
        /// Render HTML to a PDF byte array.
        /// </summary>
        public static byte[] ToPdf(string html, RenderOptions? options = null)
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

        /// <summary>
        /// Render HTML to a PDF and write to a stream.
        /// </summary>
        public static void ToPdf(string html, Stream output, RenderOptions? options = null)
        {
            var data = ToPdf(html, options);
            output.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Render HTML to a PDF byte array asynchronously.
        /// </summary>
        public static Task<byte[]> ToPdfAsync(string html, RenderOptions? options = null,
                                                CancellationToken cancellationToken = default)
        {
            return Task.Run(() => ToPdf(html, options), cancellationToken);
        }

        /// <summary>
        /// Render HTML to a PDF and write to a stream asynchronously.
        /// </summary>
        public static async Task ToPdfAsync(string html, Stream output, RenderOptions? options = null,
                                              CancellationToken cancellationToken = default)
        {
            var data = await ToPdfAsync(html, options, cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Render HTML to an image byte array.
        /// </summary>
        public static byte[] ToImage(string html, RenderOptions? options = null)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            options = options ?? RenderOptions.Default;

            var skiaOptions = new SkiaRenderOptions
            {
                Dpi = options.Dpi,
                Format = options.ImageFormat,
                Quality = options.ImageQuality
            };

            var target = new SkiaRenderTarget(skiaOptions);
            var pipeline = new RenderPipeline(options);
            var result = pipeline.Execute(html, target);
            return result.Data;
        }

        /// <summary>
        /// Render HTML to an image and write to a stream.
        /// </summary>
        public static void ToImage(string html, Stream output, RenderOptions? options = null)
        {
            var data = ToImage(html, options);
            output.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Render HTML to an image byte array asynchronously.
        /// </summary>
        public static Task<byte[]> ToImageAsync(string html, RenderOptions? options = null,
                                                  CancellationToken cancellationToken = default)
        {
            return Task.Run(() => ToImage(html, options), cancellationToken);
        }

        /// <summary>
        /// Render HTML to an image and write to a stream asynchronously.
        /// </summary>
        public static async Task ToImageAsync(string html, Stream output, RenderOptions? options = null,
                                                CancellationToken cancellationToken = default)
        {
            var data = await ToImageAsync(html, options, cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        }
    }
}
