using System.Collections.Generic;
using System.IO;
using Rend.Adapters;
using Rend.Css;
using Rend.Fonts;
using Rend.Html.Parser;
using Rend.Internal;
using Rend.Layout;
using Rend.Rendering;
using Rend.Style;
using Rend.Text;

namespace Rend
{
    /// <summary>
    /// Internal pipeline that orchestrates the full HTML → rendered output flow.
    /// </summary>
    internal sealed class RenderPipeline
    {
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
            // 1. Parse HTML
            var document = HtmlParser.Parse(html);

            // 2. Extract inline stylesheets
            var stylesheets = HtmlStyleExtractor.Extract(document);

            // 3. Load external stylesheets
            var resourceCtx = new ResourceLoadingContext(_options.BaseUrl, _options.ResourceLoader);
            var externalSheets = resourceCtx.LoadExternalStylesheets(document);
            stylesheets.AddRange(externalSheets);

            // 4. Set up font provider
            var fontProvider = _options.FontProvider ?? CreateDefaultFontProvider();

            // 5. Set up style resolver
            var selectorMatcher = new SelectorMatcherAdapter();
            var resolverOptions = new StyleResolverOptions
            {
                MediaType = "print",
                ViewportWidth = _options.PageSize.Width - _options.MarginLeft - _options.MarginRight,
                ViewportHeight = _options.PageSize.Height - _options.MarginTop - _options.MarginBottom,
                DefaultFontSize = _options.DefaultFontSize,
                ApplyUserAgentStyles = true
            };
            var styleResolver = new StyleResolver(selectorMatcher, resolverOptions);

            // 6. Build styled tree
            var treeBuilder = new StyleTreeBuilder(styleResolver, fontProvider);
            var styledTree = treeBuilder.Build(document, stylesheets);

            // Override page style with options
            styledTree.PageStyle.PageSize = _options.PageSize;
            styledTree.PageStyle.MarginTop = _options.MarginTop;
            styledTree.PageStyle.MarginRight = _options.MarginRight;
            styledTree.PageStyle.MarginBottom = _options.MarginBottom;
            styledTree.PageStyle.MarginLeft = _options.MarginLeft;

            // 7. Create text shaper
            ITextShaper textShaper = new HarfBuzzTextShaper();

            // 8. Layout
            var layoutEngine = new LayoutEngine(fontProvider, textShaper);
            var layoutOptions = new LayoutOptions
            {
                PageSize = _options.PageSize,
                MarginTop = _options.MarginTop,
                MarginRight = _options.MarginRight,
                MarginBottom = _options.MarginBottom,
                MarginLeft = _options.MarginLeft,
                DefaultFontSize = _options.DefaultFontSize,
                Paginate = true
            };
            var layoutDoc = layoutEngine.Layout(styledTree, layoutOptions);

            // 9. Paint
            var painter = new Painter();
            painter.Paint(layoutDoc, target);

            // 10. Finish and collect output
            using (var ms = new MemoryStream())
            {
                target.Finish(ms);
                return new RenderResult(ms.ToArray(), layoutDoc.Pages.Count, GetFormat(target));
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

        private static string GetFormat(IRenderTarget target)
        {
            if (target is Output.Pdf.PdfRenderTarget) return "pdf";
            if (target is Output.Image.SkiaRenderTarget) return "image";
            return "unknown";
        }
    }
}
