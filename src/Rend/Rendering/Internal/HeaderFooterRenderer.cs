using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Fonts;
using Rend.Html.Parser;
using Rend.Layout;
using Rend.Layout.Internal;
using Rend.Style;
using Rend.Text;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Renders HTML headers and footers onto each page.
    /// Supports template variables: {pageNumber}, {totalPages}, {date}.
    /// </summary>
    internal sealed class HeaderFooterRenderer
    {
        private readonly string? _headerHtml;
        private readonly string? _footerHtml;
        private readonly float _marginTop;
        private readonly float _marginBottom;
        private readonly float _marginLeft;
        private readonly float _marginRight;
        private readonly IFontProvider? _fontProvider;
        private readonly ITextShaper? _textShaper;
        private readonly float _defaultFontSize;

        public HeaderFooterRenderer(
            string? headerHtml, string? footerHtml,
            float marginTop, float marginBottom, float marginLeft, float marginRight,
            IFontProvider? fontProvider, ITextShaper? textShaper, float defaultFontSize)
        {
            _headerHtml = headerHtml;
            _footerHtml = footerHtml;
            _marginTop = marginTop;
            _marginBottom = marginBottom;
            _marginLeft = marginLeft;
            _marginRight = marginRight;
            _fontProvider = fontProvider;
            _textShaper = textShaper;
            _defaultFontSize = defaultFontSize;
        }

        public bool HasHeader => !string.IsNullOrEmpty(_headerHtml);
        public bool HasFooter => !string.IsNullOrEmpty(_footerHtml);

        public void RenderHeader(IRenderTarget target, int pageNumber, int totalPages,
            float pageWidth, float pageHeight)
        {
            if (!HasHeader) return;

            var html = SubstituteVariables(_headerHtml!, pageNumber, totalPages);
            float contentWidth = pageWidth - _marginLeft - _marginRight;
            float areaHeight = _marginTop;

            // Render in the top margin area
            RenderContent(target, html, _marginLeft, 0, contentWidth, areaHeight);
        }

        public void RenderFooter(IRenderTarget target, int pageNumber, int totalPages,
            float pageWidth, float pageHeight)
        {
            if (!HasFooter) return;

            var html = SubstituteVariables(_footerHtml!, pageNumber, totalPages);
            float contentWidth = pageWidth - _marginLeft - _marginRight;
            float areaHeight = _marginBottom;
            float areaY = pageHeight - _marginBottom;

            // Render in the bottom margin area
            RenderContent(target, html, _marginLeft, areaY, contentWidth, areaHeight);
        }

        private void RenderContent(IRenderTarget target, string html,
            float x, float y, float width, float height)
        {
            // Parse the HTML
            var document = HtmlParser.Parse(html);

            // Build a minimal styled tree
            var selectorMatcher = new Adapters.SelectorMatcherAdapter();
            var resolverOptions = new StyleResolverOptions
            {
                MediaType = "print",
                ViewportWidth = width,
                ViewportHeight = height,
                DefaultFontSize = _defaultFontSize,
                ApplyUserAgentStyles = true
            };
            var resolver = new StyleResolver(selectorMatcher, resolverOptions);
            var stylesheets = Rend.Internal.HtmlStyleExtractor.Extract(document);
            var treeBuilder = new StyleTreeBuilder(resolver, _fontProvider);
            var styledTree = treeBuilder.Build(document, stylesheets);

            // Layout in the constrained area
            var layoutOptions = new LayoutOptions
            {
                PageSize = new SizeF(width, height),
                MarginTop = 0,
                MarginRight = 0,
                MarginBottom = 0,
                MarginLeft = 0,
                DefaultFontSize = _defaultFontSize,
                Paginate = false
            };

            var layoutEngine = new LayoutEngine(_fontProvider, _textShaper);
            var layoutDoc = layoutEngine.Layout(styledTree, layoutOptions);

            // Translate to the target area and paint
            target.Save();
            target.SetTransform(new Matrix3x2(1, 0, 0, 1, x, y));

            var painter = new Painter();
            // Paint the root box directly (not the pages, since we don't paginate)
            if (layoutDoc.Pages.Count > 0)
                painter.PaintBox(layoutDoc.Pages[0].RootBox, target);

            target.Restore();
        }

        private static string SubstituteVariables(string html, int pageNumber, int totalPages)
        {
            return html
                .Replace("{pageNumber}", pageNumber.ToString())
                .Replace("{totalPages}", totalPages.ToString())
                .Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd"));
        }
    }
}
