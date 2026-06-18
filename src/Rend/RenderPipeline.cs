using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Rend.Adapters;
using Rend.Css;
using Rend.Css.Media.Internal;
using Rend.Fonts;
using Rend.Html.Parser;
using Rend.Internal;
using Rend.Layout;
using Rend.Rendering;
using Rend.Rendering.Internal;
using Rend.Style;
using Rend.Text;

namespace Rend
{
    /// <summary>
    /// Internal pipeline that orchestrates the full HTML → rendered output flow.
    /// </summary>
    internal sealed class RenderPipeline
    {
        private static readonly System.Lazy<IFontProvider> DefaultFontProvider =
            new System.Lazy<IFontProvider>(CreateDefaultFontProvider);

        private readonly RenderOptions _options;

        public RenderPipeline(RenderOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Execute the full rendering pipeline.
        /// </summary>
        public RenderResult Execute(string html, IRenderTarget target)
        {
            var progress = _options.Progress;

            // 1. Parse HTML
            progress?.Report(new RenderProgress(5, RenderStage.Parsing, "Parsing HTML"));
            var document = HtmlParser.Parse(html);

            // 2. Extract inline stylesheets
            progress?.Report(new RenderProgress(10, RenderStage.Parsing, "Extracting stylesheets"));
            var stylesheets = HtmlStyleExtractor.Extract(document);

            // 3. Load external stylesheets
            var resourceCtx = new ResourceLoadingContext(_options.BaseUrl, _options.ResourceLoader);
            var externalSheets = resourceCtx.LoadExternalStylesheets(document);
            stylesheets.AddRange(externalSheets);

            // 3b. Resolve @import rules in all stylesheets
            for (int i = 0; i < stylesheets.Count; i++)
            {
                stylesheets[i] = resourceCtx.ResolveImports(stylesheets[i]);
            }

            // 4. Set up font provider — wrap the caller's provider in a per-render
            // DocumentFontProvider so @font-face rules are scoped to this document
            // and do not leak into subsequent renders that share the same parent.
            // [CSS-FONTS-4 §5.1.2] document-declared @font-face shadows system fonts.
            progress?.Report(new RenderProgress(20, RenderStage.Styling, "Resolving fonts"));
            var baseFontProvider = _options.FontProvider ?? DefaultFontProvider.Value;
            IFontProvider fontProvider = new Fonts.Internal.DocumentFontProvider(baseFontProvider);

            // Wire font provider into PDF render target for font embedding
            if (target is Output.Pdf.PdfRenderTarget pdfTarget)
            {
                pdfTarget.SetFontProvider(fontProvider);
                pdfTarget.SetDiagnosticSink(_options.OnDiagnostic);
            }

            // 5. Create or reuse text shaper (before style resolver so ch unit can use shaped advances)
            bool ownsTextShaper = false;
            var textShaper = _options.TextShaper;
            if (textShaper == null)
            {
                textShaper = new HarfBuzzTextShaper();
                ownsTextShaper = true;
            }

            // Wire font provider into text shaper for character-level fallback
            // (needed in WASM where SKFontManager has no system fonts)
            if (textShaper is Text.HarfBuzzTextShaper harfBuzzShaper)
            {
                harfBuzzShaper.FallbackFontProvider = fontProvider;
            }

            // 6. Set up style resolver
            var selectorMatcher = new SelectorMatcherAdapter();
            var resolverOptions = new StyleResolverOptions
            {
                MediaType = _options.MediaType ?? (target is Output.Image.SkiaRenderTarget ? "screen" : "print"),
                ViewportWidth = _options.PageSize.Width - _options.MarginLeft - _options.MarginRight,
                ViewportHeight = _options.PageSize.Height - _options.MarginTop - _options.MarginBottom,
                DefaultFontSize = _options.DefaultFontSize,
                ApplyUserAgentStyles = true,
                PrefersColorSchemeDark = _options.PrefersColorSchemeDark,
                PrefersReducedMotion = true,
                MeasureCharWidth = (families, fontSize, codePoint) =>
                    MeasureCharWidthShaped(fontProvider, textShaper, families, fontSize, codePoint)
            };
            var styleResolver = new StyleResolver(selectorMatcher, resolverOptions);

            // 7. Build styled tree
            progress?.Report(new RenderProgress(30, RenderStage.Styling, "Resolving styles"));
            var treeBuilder = new StyleTreeBuilder(styleResolver, fontProvider);
            var styledTree = treeBuilder.Build(document, stylesheets);

            // Override page style with options
            styledTree.PageStyle.PageSize = _options.PageSize;
            styledTree.PageStyle.MarginTop = _options.MarginTop;
            styledTree.PageStyle.MarginRight = _options.MarginRight;
            styledTree.PageStyle.MarginBottom = _options.MarginBottom;
            styledTree.PageStyle.MarginLeft = _options.MarginLeft;

            try
            {

                // 8. Layout
                progress?.Report(new RenderProgress(50, RenderStage.Layout, "Computing layout"));
                var layoutEngine = new LayoutEngine(fontProvider, textShaper);
                var layoutOptions = new LayoutOptions
                {
                    PageSize = _options.PageSize,
                    MarginTop = _options.MarginTop,
                    MarginRight = _options.MarginRight,
                    MarginBottom = _options.MarginBottom,
                    MarginLeft = _options.MarginLeft,
                    DefaultFontSize = _options.DefaultFontSize,
                    Paginate = !(target is Output.Image.SkiaRenderTarget)
                };
                var layoutDoc = layoutEngine.Layout(styledTree, layoutOptions);

                // 9. Resolve images + paint
                progress?.Report(new RenderProgress(70, RenderStage.Rendering, "Resolving images"));

                System.Func<string, byte[]?>? byteLoader = _options.ResourceLoader != null
                    ? url => resourceCtx.LoadResourceBytes(url)
                    : null;

                var mediaContext = new MediaContext(
                    resolverOptions.ViewportWidth, resolverOptions.ViewportHeight, resolverOptions.MediaType);
                var imageResolver = new InlineImageResolver(_options.BaseUrl, _options.ImageResolver, byteLoader, mediaContext);
                var resolvedImages = imageResolver.Resolve(document);

                // 10. Paint
                progress?.Report(new RenderProgress(80, RenderStage.Rendering, "Painting output"));
                System.Func<string, ImageData?> resolveImage = src =>
                {
                    if (resolvedImages.TryGetValue(src, out var img))
                    {
                        return img;
                    }
                    return imageResolver.LoadOnDemand(src);
                };
                var painter = new Painter(resolveImage, _options.GenerateLinks, _options.GenerateBookmarks);

                // Set up header/footer renderer if configured
                HeaderFooterRenderer? hfRenderer = null;
                if (!string.IsNullOrEmpty(_options.HeaderHtml) || !string.IsNullOrEmpty(_options.FooterHtml))
                {
                    hfRenderer = new HeaderFooterRenderer(
                        _options.HeaderHtml, _options.FooterHtml,
                        _options.MarginTop, _options.MarginBottom,
                        _options.MarginLeft, _options.MarginRight,
                        fontProvider, textShaper, _options.DefaultFontSize);
                }

                painter.Paint(layoutDoc, target, hfRenderer);

                // 10b. Capture layout tree snapshot if requested
                LayoutSnapshot? layoutSnapshot = null;
                if (_options.CaptureLayoutTree)
                {
                    layoutSnapshot = LayoutSnapshotBuilder.Build(layoutDoc.RootBox);
                }

                // 11. Finish and collect output
                progress?.Report(new RenderProgress(90, RenderStage.Finishing, "Generating output"));
                using (var ms = new MemoryStream())
                {
                    target.Finish(ms);
                    progress?.Report(new RenderProgress(100, RenderStage.Finishing, "Complete"));
                    return new RenderResult(ms.ToArray(), layoutDoc.Pages.Count, GetFormat(target), layoutSnapshot);
                }
            }
            finally
            {
                if (ownsTextShaper && textShaper is System.IDisposable disposableShaper)
                {
                    disposableShaper.Dispose();
                }
            }
        }

        /// <summary>
        /// Execute the full rendering pipeline asynchronously.
        /// I/O-bound steps (external stylesheets, images) are awaited; CPU-bound steps run synchronously.
        /// </summary>
        public async Task<RenderResult> ExecuteAsync(string html, IRenderTarget target, CancellationToken cancellationToken = default)
        {
            var progress = _options.Progress;

            // 1. Parse HTML
            progress?.Report(new RenderProgress(5, RenderStage.Parsing, "Parsing HTML"));
            cancellationToken.ThrowIfCancellationRequested();
            var document = HtmlParser.Parse(html);

            // 2. Extract inline stylesheets
            progress?.Report(new RenderProgress(10, RenderStage.Parsing, "Extracting stylesheets"));
            var stylesheets = HtmlStyleExtractor.Extract(document);

            // 3. Load external stylesheets (async)
            var resourceCtx = new ResourceLoadingContext(_options.BaseUrl, _options.ResourceLoader);
            var externalSheets = await resourceCtx.LoadExternalStylesheetsAsync(document, cancellationToken).ConfigureAwait(false);
            stylesheets.AddRange(externalSheets);

            // 3b. Resolve @import rules in all stylesheets
            for (int i = 0; i < stylesheets.Count; i++)
            {
                stylesheets[i] = await resourceCtx.ResolveImportsAsync(stylesheets[i], cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // 4. Set up font provider — wrap the caller's provider in a per-render
            // DocumentFontProvider so @font-face rules are scoped to this document
            // and do not leak into subsequent renders that share the same parent.
            // [CSS-FONTS-4 §5.1.2] document-declared @font-face shadows system fonts.
            progress?.Report(new RenderProgress(20, RenderStage.Styling, "Resolving fonts"));
            var baseFontProvider = _options.FontProvider ?? DefaultFontProvider.Value;
            IFontProvider fontProvider = new Fonts.Internal.DocumentFontProvider(baseFontProvider);

            // Wire font provider into PDF render target for font embedding
            if (target is Output.Pdf.PdfRenderTarget pdfTarget)
            {
                pdfTarget.SetFontProvider(fontProvider);
                pdfTarget.SetDiagnosticSink(_options.OnDiagnostic);
            }

            // 5. Create or reuse text shaper (before style resolver so ch unit can use shaped advances)
            bool ownsTextShaperAsync = false;
            var textShaper = _options.TextShaper;
            if (textShaper == null)
            {
                textShaper = new HarfBuzzTextShaper();
                ownsTextShaperAsync = true;
            }

            // Wire font provider into text shaper for character-level fallback
            if (textShaper is Text.HarfBuzzTextShaper harfBuzzShaper2)
            {
                harfBuzzShaper2.FallbackFontProvider = fontProvider;
            }

            // 6. Set up style resolver
            var selectorMatcher = new SelectorMatcherAdapter();
            var resolverOptions = new StyleResolverOptions
            {
                MediaType = _options.MediaType ?? (target is Output.Image.SkiaRenderTarget ? "screen" : "print"),
                ViewportWidth = _options.PageSize.Width - _options.MarginLeft - _options.MarginRight,
                ViewportHeight = _options.PageSize.Height - _options.MarginTop - _options.MarginBottom,
                DefaultFontSize = _options.DefaultFontSize,
                ApplyUserAgentStyles = true,
                PrefersColorSchemeDark = _options.PrefersColorSchemeDark,
                PrefersReducedMotion = true,
                MeasureCharWidth = (families, fontSize, codePoint) =>
                    MeasureCharWidthShaped(fontProvider, textShaper, families, fontSize, codePoint)
            };
            var styleResolver = new StyleResolver(selectorMatcher, resolverOptions);

            // 7. Build styled tree
            progress?.Report(new RenderProgress(30, RenderStage.Styling, "Resolving styles"));
            cancellationToken.ThrowIfCancellationRequested();
            var treeBuilder = new StyleTreeBuilder(styleResolver, fontProvider);
            var styledTree = treeBuilder.Build(document, stylesheets);

            // Override page style with options
            styledTree.PageStyle.PageSize = _options.PageSize;
            styledTree.PageStyle.MarginTop = _options.MarginTop;
            styledTree.PageStyle.MarginRight = _options.MarginRight;
            styledTree.PageStyle.MarginBottom = _options.MarginBottom;
            styledTree.PageStyle.MarginLeft = _options.MarginLeft;

            try
            {

                // 8. Layout
                progress?.Report(new RenderProgress(50, RenderStage.Layout, "Computing layout"));
                cancellationToken.ThrowIfCancellationRequested();
                var layoutEngine = new LayoutEngine(fontProvider, textShaper);
                var layoutOptions = new LayoutOptions
                {
                    PageSize = _options.PageSize,
                    MarginTop = _options.MarginTop,
                    MarginRight = _options.MarginRight,
                    MarginBottom = _options.MarginBottom,
                    MarginLeft = _options.MarginLeft,
                    DefaultFontSize = _options.DefaultFontSize,
                    Paginate = !(target is Output.Image.SkiaRenderTarget)
                };
                var layoutDoc = layoutEngine.Layout(styledTree, layoutOptions);

                // 9. Resolve images + paint (async)
                progress?.Report(new RenderProgress(70, RenderStage.Rendering, "Resolving images"));

                System.Func<string, byte[]?>? byteLoader = _options.ResourceLoader != null
                    ? url => resourceCtx.LoadResourceBytes(url)
                    : null;

                var mediaContextAsync = new MediaContext(
                    resolverOptions.ViewportWidth, resolverOptions.ViewportHeight, resolverOptions.MediaType);
                var imageResolver = new InlineImageResolver(_options.BaseUrl, _options.ImageResolver, byteLoader, mediaContextAsync);
                var resolvedImages = await imageResolver.ResolveAsync(document, cancellationToken).ConfigureAwait(false);

                // 10. Paint
                progress?.Report(new RenderProgress(80, RenderStage.Rendering, "Painting output"));
                cancellationToken.ThrowIfCancellationRequested();
                System.Func<string, ImageData?> resolveImage = src =>
                {
                    if (resolvedImages.TryGetValue(src, out var img))
                    {
                        return img;
                    }
                    return imageResolver.LoadOnDemand(src);
                };
                var painter = new Painter(resolveImage, _options.GenerateLinks, _options.GenerateBookmarks);

                // Set up header/footer renderer if configured
                HeaderFooterRenderer? hfRenderer = null;
                if (!string.IsNullOrEmpty(_options.HeaderHtml) || !string.IsNullOrEmpty(_options.FooterHtml))
                {
                    hfRenderer = new HeaderFooterRenderer(
                        _options.HeaderHtml, _options.FooterHtml,
                        _options.MarginTop, _options.MarginBottom,
                        _options.MarginLeft, _options.MarginRight,
                        fontProvider, textShaper, _options.DefaultFontSize);
                }

                painter.Paint(layoutDoc, target, hfRenderer);

                // 10b. Capture layout tree snapshot if requested
                LayoutSnapshot? layoutSnapshotAsync = null;
                if (_options.CaptureLayoutTree)
                {
                    layoutSnapshotAsync = LayoutSnapshotBuilder.Build(layoutDoc.RootBox);
                }

                // 11. Finish and collect output
                progress?.Report(new RenderProgress(90, RenderStage.Finishing, "Generating output"));
                using (var ms = new MemoryStream())
                {
                    target.Finish(ms);
                    progress?.Report(new RenderProgress(100, RenderStage.Finishing, "Complete"));
                    return new RenderResult(ms.ToArray(), layoutDoc.Pages.Count, GetFormat(target), layoutSnapshotAsync);
                }
            }
            finally
            {
                if (ownsTextShaperAsync && textShaper is System.IDisposable disposableShaperAsync)
                {
                    disposableShaperAsync.Dispose();
                }
            }
        }

        private static IFontProvider CreateDefaultFontProvider()
        {
            var collection = new FontCollection();

            // Try to register system fonts (gracefully handle failures)
            try
            {
                var resolver = new SystemFontResolver();
                collection.RegisterFromResolver(resolver);
            }
            catch
            {
                // System fonts unavailable — fall back to PDF standard fonts
            }

            return collection;
        }

        /// <summary>
        /// [CSS-VALUES-4 §6.1] Measures the advance width of a character using the text shaper.
        /// Chrome uses shaped advances (with hinting) for the ch unit, not raw OpenType metrics.
        /// </summary>
        private static float MeasureCharWidthShaped(
            IFontProvider fontProvider, ITextShaper textShaper,
            string[] families, float fontSize, int codePoint)
        {
            var descriptor = new FontDescriptor(families);
            FontEntry? entry = fontProvider.ResolveFont(descriptor);
            if (entry == null)
            {
                return fontSize * 0.5f;
            }

            string charText = char.ConvertFromUtf32(codePoint);
            var shapedRun = textShaper.Shape(charText, entry.FontData, fontSize);
            if (shapedRun.Glyphs.Length > 0 && shapedRun.TotalWidth > 0)
            {
                return shapedRun.TotalWidth;
            }

            return entry.GetCharWidth(codePoint, fontSize);
        }

        private static string GetFormat(IRenderTarget target)
        {
            if (target is Output.Pdf.PdfRenderTarget) return "pdf";
            if (target is Output.Image.SkiaRenderTarget) return "image";
            return "unknown";
        }
    }
}
