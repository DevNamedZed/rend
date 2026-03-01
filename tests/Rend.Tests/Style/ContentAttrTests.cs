using System.Collections.Generic;
using System.Linq;
using Rend.Adapters;
using Rend.Css;
using Rend.Html;
using Rend.Html.Parser;
using Rend.Style;
using Xunit;

namespace Rend.Tests.Style
{
    public class ContentAttrTests
    {
        private StyledTree BuildStyledTree(string html, string css)
        {
            var document = HtmlParser.Parse(html);
            var stylesheet = CssParser.Parse(css);

            var matcher = new SelectorMatcherAdapter();
            var resolver = new StyleResolver(matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 800,
                ViewportHeight = 600
            });

            var builder = new StyleTreeBuilder(resolver);
            return builder.Build(document, new List<Stylesheet> { stylesheet });
        }

        private StyledPseudoElement? FindPseudoElement(StyledNode node, string pseudoType)
        {
            if (node is StyledElement element)
            {
                foreach (var child in element.Children)
                {
                    if (child is StyledPseudoElement pseudo && pseudo.PseudoType == pseudoType)
                        return pseudo;

                    var found = FindPseudoElement(child, pseudoType);
                    if (found != null) return found;
                }
            }
            return null;
        }

        [Fact]
        public void ContentAttr_Before_ResolvesAttribute()
        {
            var tree = BuildStyledTree(
                "<p data-label=\"Hello\">Text</p>",
                "p::before { content: attr(data-label); }");

            var pseudo = FindPseudoElement(tree.Root, "before");
            Assert.NotNull(pseudo);
            Assert.Equal("Hello", pseudo!.Content);
        }

        [Fact]
        public void ContentAttr_After_ResolvesAttribute()
        {
            var tree = BuildStyledTree(
                "<span title=\"World\">Text</span>",
                "span::after { content: attr(title); }");

            var pseudo = FindPseudoElement(tree.Root, "after");
            Assert.NotNull(pseudo);
            Assert.Equal("World", pseudo!.Content);
        }

        [Fact]
        public void ContentAttr_MissingAttribute_NoPseudoElement()
        {
            var tree = BuildStyledTree(
                "<p>Text</p>",
                "p::before { content: attr(data-label); }");

            var pseudo = FindPseudoElement(tree.Root, "before");
            // attr() returns null when attribute doesn't exist, so no pseudo-element
            Assert.Null(pseudo);
        }

        [Fact]
        public void ContentString_StillWorks()
        {
            var tree = BuildStyledTree(
                "<p>Text</p>",
                "p::before { content: \"Prefix: \"; }");

            var pseudo = FindPseudoElement(tree.Root, "before");
            Assert.NotNull(pseudo);
            Assert.Equal("Prefix: ", pseudo!.Content);
        }

        [Fact]
        public void ContentNone_NoPseudoElement()
        {
            var tree = BuildStyledTree(
                "<p>Text</p>",
                "p::before { content: none; }");

            var pseudo = FindPseudoElement(tree.Root, "before");
            Assert.Null(pseudo);
        }
    }
}
