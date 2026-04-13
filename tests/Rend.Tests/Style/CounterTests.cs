using System.Collections.Generic;
using Rend.Adapters;
using Rend.Css;
using Rend.Html;
using Rend.Html.Parser;
using Rend.Style;
using Xunit;

namespace Rend.Tests.Style
{
    public class CounterTests
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

        private List<StyledPseudoElement> FindAllPseudoElements(StyledNode node, string pseudoType)
        {
            var result = new List<StyledPseudoElement>();
            CollectPseudoElements(node, pseudoType, result);
            return result;
        }

        private void CollectPseudoElements(StyledNode node, string pseudoType, List<StyledPseudoElement> result)
        {
            if (node is StyledPseudoElement pseudo && pseudo.PseudoType == pseudoType)
            {
                result.Add(pseudo);
            }
            if (node is StyledElement element)
            {
                foreach (var child in element.Children)
                    CollectPseudoElements(child, pseudoType, result);
            }
        }

        [Fact]
        public void Counter_SimpleIncrement()
        {
            var tree = BuildStyledTree(
                "<ol><li>A</li><li>B</li><li>C</li></ol>",
                "ol { counter-reset: item; } li { counter-increment: item; } li::before { content: counter(item); }");

            var pseudos = FindAllPseudoElements(tree.Root, "before");
            Assert.Equal(3, pseudos.Count);
            Assert.Equal("1", pseudos[0].Content);
            Assert.Equal("2", pseudos[1].Content);
            Assert.Equal("3", pseudos[2].Content);
        }

        [Fact]
        public void Counter_ResetWithValue()
        {
            var tree = BuildStyledTree(
                "<ol><li>A</li><li>B</li></ol>",
                "ol { counter-reset: item 5; } li { counter-increment: item; } li::before { content: counter(item); }");

            var pseudos = FindAllPseudoElements(tree.Root, "before");
            Assert.Equal(2, pseudos.Count);
            Assert.Equal("6", pseudos[0].Content);
            Assert.Equal("7", pseudos[1].Content);
        }

        [Fact]
        public void Counter_IncrementByTwo()
        {
            var tree = BuildStyledTree(
                "<ol><li>A</li><li>B</li></ol>",
                "ol { counter-reset: item; } li { counter-increment: item 2; } li::before { content: counter(item); }");

            var pseudos = FindAllPseudoElements(tree.Root, "before");
            Assert.Equal(2, pseudos.Count);
            Assert.Equal("2", pseudos[0].Content);
            Assert.Equal("4", pseudos[1].Content);
        }

        [Fact]
        public void Counter_WithString_Concatenation()
        {
            var tree = BuildStyledTree(
                "<ol><li>A</li></ol>",
                "ol { counter-reset: item; } li { counter-increment: item; } li::before { content: counter(item) \". \"; }");

            var pseudos = FindAllPseudoElements(tree.Root, "before");
            Assert.Single(pseudos);
            Assert.Equal("1. ", pseudos[0].Content);
        }

        [Fact]
        public void Counter_NoCounterReset_ImplicitCreate()
        {
            var tree = BuildStyledTree(
                "<p>A</p><p>B</p>",
                "p { counter-increment: section; } p::before { content: counter(section); }");

            var pseudos = FindAllPseudoElements(tree.Root, "before");
            Assert.Equal(2, pseudos.Count);
            Assert.Equal("1", pseudos[0].Content);
            Assert.Equal("2", pseudos[1].Content);
        }

        [Fact]
        public void CounterReset_None_NoEffect()
        {
            var tree = BuildStyledTree(
                "<p>A</p>",
                "p { counter-reset: none; counter-increment: section; } p::before { content: counter(section); }");

            var pseudos = FindAllPseudoElements(tree.Root, "before");
            Assert.Single(pseudos);
            Assert.Equal("1", pseudos[0].Content);
        }

        // [CSS-LISTS-3 §2] counter-reset on an element establishes a counter whose
        // scope extends to the element, its descendants, AND its following
        // siblings (with their descendants).
        [Fact]
        public void CounterReset_VisibleToFollowingSibling()
        {
            var tree = BuildStyledTree(
                "<div><span class=\"reset\"></span><span class=\"inc\"></span></div>",
                ".reset { counter-reset: x 6 } .inc { counter-increment: x } .inc::before { content: counter(x) }");

            var pseudos = FindAllPseudoElements(tree.Root, "before");
            Assert.Single(pseudos);
            Assert.Equal("7", pseudos[0].Content);
        }

        [Fact]
        public void CounterReset_NestedInDisplayContentsVisibleToFollowingSibling()
        {
            var tree = BuildStyledTree(
                "<div><span class=\"reset-6\"></span><span class=\"contents\"><span class=\"reset-666\"></span></span><span class=\"inc result\"></span></div>",
                ".inc { counter-increment: x }" +
                ".reset-6 { counter-reset: x 6 }" +
                ".reset-666 { counter-reset: x 6 }" +
                ".contents { display: contents }" +
                ".result::before { content: counter(x) }");

            var pseudos = FindAllPseudoElements(tree.Root, "before");
            Assert.Single(pseudos);
            Assert.Equal("7", pseudos[0].Content);
        }

        [Fact]
        public void CounterReset_NestedInstanceDoesNotLeakToOuterSibling()
        {
            var tree = BuildStyledTree(
                "<div id=\"test\"><span></span><span></span><span style=\"counter-reset: c 98\"></span><span></span></div>",
                "#test { counter-reset: c }" +
                "#test span { counter-increment: c }" +
                "#test span::before { content: counter(c) }");

            var pseudos = FindAllPseudoElements(tree.Root, "before");
            Assert.Equal(4, pseudos.Count);
            Assert.Equal("1", pseudos[0].Content);
            Assert.Equal("2", pseudos[1].Content);
            Assert.Equal("99", pseudos[2].Content);
            Assert.Equal("3", pseudos[3].Content);
        }

        [Fact]
        public void CounterReset_NestedAtDescendantShadowsDescendantsOnly()
        {
            var tree = BuildStyledTree(
                "<div><span class=\"outer\"></span><span class=\"inner\"><span class=\"inc-inner\"></span></span><span class=\"inc\"></span></div>",
                ".outer { counter-reset: c 10 }" +
                ".inner { counter-reset: c 100 }" +
                ".inc-inner { counter-increment: c }" +
                ".inc { counter-increment: c }" +
                ".inc-inner::before { content: counter(c) }" +
                ".inc::before { content: counter(c) }");

            var pseudos = FindAllPseudoElements(tree.Root, "before");
            Assert.Equal(2, pseudos.Count);
            Assert.Equal("101", pseudos[0].Content);
            Assert.Equal("11", pseudos[1].Content);
        }
    }
}
