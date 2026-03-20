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
    /// Owns font/shaper caches that live for the renderer's lifetime.
    /// Dispose the renderer to free all cached native font memory.
    /// </summary>
    public sealed class HtmlRenderer : IRenderer, IDisposable
    {
        private readonly object _resourceLock = new object();
        private SkiaFontMapper? _fontMapper;
        private SkiaTextShaper? _textShaper;
        private bool _disposed;

        private SkiaFontMapper EnsureFontMapper()
        {
            if (_fontMapper == null)
            {
                lock (_resourceLock)
                {
                    if (_fontMapper == null)
                    {
                        _fontMapper = new SkiaFontMapper();
                    }
                }
            }
            return _fontMapper;
        }

        private SkiaTextShaper EnsureTextShaper()
        {
            if (_textShaper == null)
            {
                lock (_resourceLock)
                {
                    if (_textShaper == null)
                    {
                        _textShaper = new SkiaTextShaper(EnsureFontMapper());
                    }
                }
            }
            return _textShaper;
        }

        /// <inheritdoc />
        public byte[] ToPdf(string html, RenderOptions? options = null)
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }
            options = options ?? RenderOptions.Default;

            var pdfOptions = new PdfRenderOptions
            {
                GenerateBookmarks = options.GenerateBookmarks,
                GenerateLinks = options.GenerateLinks,
                Title = options.Title,
                Author = options.Author,
                DocumentOptions = options.PdfOptions ?? DefaultPdfDocumentOptions()
            };

            var target = new PdfRenderTarget(pdfOptions);
            var pipeline = new RenderPipeline(options);
            var result = pipeline.Execute(html, target);
            return result.Data;
        }

        /// <inheritdoc />
        public void ToPdf(string html, Stream output, RenderOptions? options = null)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            var data = ToPdf(html, options);
            output.Write(data, 0, data.Length);
        }

        /// <inheritdoc />
        public byte[] ToPdf(TextReader html, RenderOptions? options = null)
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }
            return ToPdf(html.ReadToEnd(), options);
        }

        /// <inheritdoc />
        public void ToPdf(TextReader html, Stream output, RenderOptions? options = null)
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            ToPdf(html.ReadToEnd(), output, options);
        }

        /// <inheritdoc />
        public async Task<byte[]> ToPdfAsync(string html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }
            cancellationToken.ThrowIfCancellationRequested();
            options = options ?? RenderOptions.Default;

            var pdfOptions = new PdfRenderOptions
            {
                GenerateBookmarks = options.GenerateBookmarks,
                GenerateLinks = options.GenerateLinks,
                Title = options.Title,
                Author = options.Author,
                DocumentOptions = options.PdfOptions ?? DefaultPdfDocumentOptions()
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
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            var data = await ToPdfAsync(html, options, cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<byte[]> ToPdfAsync(TextReader html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }
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
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
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
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }
            options = options ?? RenderOptions.Default;

            var effectiveMapper = options.FontMapper ?? EnsureFontMapper();
            var effectiveShaper = options.TextShaper ?? EnsureTextShaper();

            var skiaOptions = new SkiaRenderOptions
            {
                Dpi = options.Dpi,
                Format = options.ImageFormat,
                Quality = options.ImageQuality
            };

            // Pass the effective mapper to SkiaRenderTarget (it does NOT own it).
            using var target = new SkiaRenderTarget(skiaOptions, effectiveMapper);

            // Ensure pipeline uses the renderer's cached shaper and mapper.
            var pipelineOptions = WithImageResources(options, effectiveMapper, effectiveShaper);
            var pipeline = new RenderPipeline(pipelineOptions);
            return pipeline.Execute(html, target);
        }

        /// <inheritdoc />
        public void ToImage(string html, Stream output, RenderOptions? options = null)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            var data = ToImage(html, options);
            output.Write(data, 0, data.Length);
        }

        /// <inheritdoc />
        public byte[] ToImage(TextReader html, RenderOptions? options = null)
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }
            return ToImage(html.ReadToEnd(), options);
        }

        /// <inheritdoc />
        public void ToImage(TextReader html, Stream output, RenderOptions? options = null)
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            ToImage(html.ReadToEnd(), output, options);
        }

        /// <inheritdoc />
        public async Task<byte[]> ToImageAsync(string html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }
            cancellationToken.ThrowIfCancellationRequested();
            options = options ?? RenderOptions.Default;

            var effectiveMapper = options.FontMapper ?? EnsureFontMapper();
            var effectiveShaper = options.TextShaper ?? EnsureTextShaper();

            var skiaOptions = new SkiaRenderOptions
            {
                Dpi = options.Dpi,
                Format = options.ImageFormat,
                Quality = options.ImageQuality
            };

            using var target = new SkiaRenderTarget(skiaOptions, effectiveMapper);

            var pipelineOptions = WithImageResources(options, effectiveMapper, effectiveShaper);
            var pipeline = new RenderPipeline(pipelineOptions);
            var result = await pipeline.ExecuteAsync(html, target, cancellationToken).ConfigureAwait(false);
            return result.Data;
        }

        /// <inheritdoc />
        public async Task ToImageAsync(string html, Stream output, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            var data = await ToImageAsync(html, options, cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<byte[]> ToImageAsync(TextReader html, RenderOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }
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
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
#if NET8_0_OR_GREATER
            var text = await html.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
#else
            cancellationToken.ThrowIfCancellationRequested();
            var text = await html.ReadToEndAsync().ConfigureAwait(false);
#endif
            await ToImageAsync(text, output, options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a copy of the given render options with the font mapper and text shaper
        /// set to the renderer's cached instances (if the caller didn't provide their own).
        /// </summary>
        private static RenderOptions WithImageResources(RenderOptions source, SkiaFontMapper mapper, Text.ITextShaper shaper)
        {
            return new RenderOptions
            {
                PageSize = source.PageSize,
                MarginTop = source.MarginTop,
                MarginRight = source.MarginRight,
                MarginBottom = source.MarginBottom,
                MarginLeft = source.MarginLeft,
                Dpi = source.Dpi,
                ImageFormat = source.ImageFormat,
                ImageQuality = source.ImageQuality,
                DefaultFontSize = source.DefaultFontSize,
                MediaType = source.MediaType,
                FontProvider = source.FontProvider,
                TextShaper = shaper,
                FontMapper = mapper,
                ResourceLoader = source.ResourceLoader,
                BaseUrl = source.BaseUrl,
                ImageResolver = source.ImageResolver,
                GenerateLinks = source.GenerateLinks,
                GenerateBookmarks = source.GenerateBookmarks,
                CaptureLayoutTree = source.CaptureLayoutTree,
                Progress = source.Progress,
                PrefersColorSchemeDark = source.PrefersColorSchemeDark,
                Title = source.Title,
                Author = source.Author,
                PdfOptions = source.PdfOptions,
                HeaderHtml = source.HeaderHtml,
                FooterHtml = source.FooterHtml,
            };
        }

        /// <summary>
        /// Disposes the renderer, freeing all cached native font memory (typefaces, HarfBuzz objects).
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            _textShaper?.Dispose();
            _fontMapper?.Dispose();
        }

        private static Pdf.PdfDocumentOptions DefaultPdfDocumentOptions()
        {
            return new Pdf.PdfDocumentOptions
            {
                Compression = Pdf.PdfCompression.FlateFast
            };
        }
    }
}
