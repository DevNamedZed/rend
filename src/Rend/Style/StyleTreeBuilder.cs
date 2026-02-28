using System.Collections.Generic;
using Rend.Adapters;
using Rend.Css;
using Rend.Fonts;
using Rend.Html;

namespace Rend.Style
{
    /// <summary>
    /// Walks the DOM tree, resolves ComputedStyle for each element via the CSS cascade,
    /// and produces a StyledTree for layout.
    /// </summary>
    public sealed class StyleTreeBuilder
    {
        private readonly StyleResolver _resolver;
        private readonly IFontProvider? _fontProvider;

        public StyleTreeBuilder(StyleResolver resolver, IFontProvider? fontProvider = null)
        {
            _resolver = resolver;
            _fontProvider = fontProvider;
        }

        /// <summary>
        /// Build a styled tree from a parsed HTML document and its stylesheets.
        /// </summary>
        public StyledTree Build(Document document, IReadOnlyList<Stylesheet> stylesheets)
        {
            // Add all stylesheets to the resolver
            for (int i = 0; i < stylesheets.Count; i++)
            {
                _resolver.AddStylesheet(stylesheets[i]);

                // Process @font-face rules
                if (_fontProvider != null)
                    FontFaceProcessor.Process(stylesheets[i].Rules, _fontProvider);
            }

            // Collect page style info from all stylesheets
            var pageStyle = new PageStyleInfo();
            for (int i = 0; i < stylesheets.Count; i++)
            {
                var ps = PageStyleProcessor.Process(stylesheets[i].Rules);
                // Last @page rule wins for each property
                pageStyle = ps;
            }

            // Find the root element
            var root = document.DocumentElement;
            if (root == null)
            {
                // Create a minimal styled element if no root
                var emptyStyle = _resolver.Resolve(
                    new StylableElementAdapter(document.Body ?? CreateFallbackElement(document)),
                    null);
                return new StyledTree(
                    new StyledElement(document.Body ?? CreateFallbackElement(document), emptyStyle, new List<StyledNode>()),
                    pageStyle);
            }

            var styledRoot = BuildElement(root, null);
            return new StyledTree(styledRoot, pageStyle);
        }

        private StyledElement BuildElement(Element element, ComputedStyle? parentStyle)
        {
            var adapter = new StylableElementAdapter(element);
            var computedStyle = _resolver.Resolve(adapter, parentStyle);

            var children = new List<StyledNode>();
            var child = element.FirstChild;
            while (child != null)
            {
                if (child is Element childEl)
                {
                    children.Add(BuildElement(childEl, computedStyle));
                }
                else if (child is TextNode textNode)
                {
                    var text = textNode.Data;
                    if (!string.IsNullOrEmpty(text))
                    {
                        children.Add(new StyledText(text, computedStyle));
                    }
                }

                child = child.NextSibling;
            }

            return new StyledElement(element, computedStyle, children);
        }

        private static Element CreateFallbackElement(Document document)
        {
            return document.CreateElement("div");
        }
    }
}
