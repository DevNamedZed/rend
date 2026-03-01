using System.Collections.Generic;
using Rend.Adapters;
using Rend.Css;
using Rend.Html;
using Rend.Html.Parser;
using Rend.Style;
using Xunit;

namespace Rend.Tests.Style
{
    public class QuotesTests
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

        private List<StyledPseudoElement> FindAllPseudoElements(StyledNode node, string pseudoType)
        {
            var result = new List<StyledPseudoElement>();
            CollectPseudoElements(node, pseudoType, result);
            return result;
        }

        private void CollectPseudoElements(StyledNode node, string pseudoType, List<StyledPseudoElement> result)
        {
            if (node is StyledPseudoElement pseudo && pseudo.PseudoType == pseudoType)
                result.Add(pseudo);
            if (node is StyledElement element)
            {
                foreach (var child in element.Children)
                    CollectPseudoElements(child, pseudoType, result);
            }
        }

        [Fact]
        public void OpenQuote_DefaultQuotes()
        {
            var tree = BuildStyledTree(
                "<q>Hello</q>",
                "q::before { content: open-quote; }");

            var pseudo = FindPseudoElement(tree.Root, "before");
            Assert.NotNull(pseudo);
            // Default open quote is left double quotation mark
            Assert.Equal("\u201c", pseudo!.Content);
        }

        [Fact]
        public void CloseQuote_DefaultQuotes()
        {
            var tree = BuildStyledTree(
                "<q>Hello</q>",
                "q::after { content: close-quote; }");

            var pseudo = FindPseudoElement(tree.Root, "after");
            Assert.NotNull(pseudo);
            // Default close quote is right double quotation mark
            Assert.Equal("\u201d", pseudo!.Content);
        }

        [Fact]
        public void OpenAndCloseQuote_Pair()
        {
            var tree = BuildStyledTree(
                "<q>Hello</q>",
                "q::before { content: open-quote; } q::after { content: close-quote; }");

            var before = FindPseudoElement(tree.Root, "before");
            var after = FindPseudoElement(tree.Root, "after");
            Assert.NotNull(before);
            Assert.NotNull(after);
            Assert.Equal("\u201c", before!.Content);
            Assert.Equal("\u201d", after!.Content);
        }

        [Fact]
        public void CustomQuotes()
        {
            var tree = BuildStyledTree(
                "<q>Hello</q>",
                "q { quotes: \"<<\" \">>\"; } q::before { content: open-quote; } q::after { content: close-quote; }");

            var before = FindPseudoElement(tree.Root, "before");
            var after = FindPseudoElement(tree.Root, "after");
            Assert.NotNull(before);
            Assert.NotNull(after);
            Assert.Equal("<<", before!.Content);
            Assert.Equal(">>", after!.Content);
        }
    }
}
