using Rend.Adapters;
using Rend.Css;
using Rend.Html.Parser;
using Xunit;

namespace Rend.Tests.Adapters
{
    public class SelectorMatcherAdapterTests
    {
        private readonly SelectorMatcherAdapter _matcher = new SelectorMatcherAdapter();

        [Fact]
        public void Matches_TypeSelector_MatchesCorrectElement()
        {
            var doc = HtmlParser.Parse("<div><p>Hello</p></div>");
            var p = FindFirstElement(doc, "p");
            var adapter = new StylableElementAdapter(p!);

            Assert.True(_matcher.Matches(adapter, "p"));
            Assert.False(_matcher.Matches(adapter, "div"));
        }

        [Fact]
        public void Matches_IdSelector_MatchesById()
        {
            var doc = HtmlParser.Parse("<div id=\"main\">Content</div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            Assert.True(_matcher.Matches(adapter, "#main"));
            Assert.False(_matcher.Matches(adapter, "#other"));
        }

        [Fact]
        public void Matches_ClassSelector_MatchesByClass()
        {
            var doc = HtmlParser.Parse("<div class=\"foo bar\">Content</div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            Assert.True(_matcher.Matches(adapter, ".foo"));
            Assert.True(_matcher.Matches(adapter, ".bar"));
            Assert.False(_matcher.Matches(adapter, ".baz"));
        }

        [Fact]
        public void Matches_CompoundSelector_MatchesAll()
        {
            var doc = HtmlParser.Parse("<div id=\"main\" class=\"container\">Content</div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            Assert.True(_matcher.Matches(adapter, "div.container"));
            Assert.True(_matcher.Matches(adapter, "div#main"));
            Assert.True(_matcher.Matches(adapter, "div#main.container"));
        }

        [Fact]
        public void Matches_DescendantCombinator()
        {
            var doc = HtmlParser.Parse("<div><p><span>Text</span></p></div>");
            var span = FindFirstElement(doc, "span");
            var adapter = new StylableElementAdapter(span!);

            Assert.True(_matcher.Matches(adapter, "div span"));
            Assert.True(_matcher.Matches(adapter, "p span"));
        }

        [Fact]
        public void Matches_ChildCombinator()
        {
            var doc = HtmlParser.Parse("<div><span>Direct</span></div>");
            var span = FindFirstElement(doc, "span");
            var adapter = new StylableElementAdapter(span!);

            Assert.True(_matcher.Matches(adapter, "div > span"));
        }

        [Fact]
        public void Matches_AttributeSelector()
        {
            var doc = HtmlParser.Parse("<input type=\"text\" />");
            var input = FindFirstElement(doc, "input");
            var adapter = new StylableElementAdapter(input!);

            Assert.True(_matcher.Matches(adapter, "[type]"));
            Assert.True(_matcher.Matches(adapter, "input[type]"));
        }

        [Fact]
        public void Matches_UniversalSelector()
        {
            var doc = HtmlParser.Parse("<div>Content</div>");
            var div = FindFirstElement(doc, "div");
            var adapter = new StylableElementAdapter(div!);

            Assert.True(_matcher.Matches(adapter, "*"));
        }

        [Fact]
        public void Matches_ReturnsFalse_ForNonAdapterElement()
        {
            // When passed a non-StylableElementAdapter, should return false
            var mockElement = new MockStylableElement();
            Assert.False(_matcher.Matches(mockElement, "div"));
        }

        [Fact]
        public void GetSpecificity_IdSelector_Returns_1_0_0()
        {
            var specificity = _matcher.GetSpecificity("#main");
            Assert.Equal(1, specificity.A);
            Assert.Equal(0, specificity.B);
            Assert.Equal(0, specificity.C);
        }

        [Fact]
        public void GetSpecificity_ClassSelector_Returns_0_1_0()
        {
            var specificity = _matcher.GetSpecificity(".container");
            Assert.Equal(0, specificity.A);
            Assert.Equal(1, specificity.B);
            Assert.Equal(0, specificity.C);
        }

        [Fact]
        public void GetSpecificity_TypeSelector_Returns_0_0_1()
        {
            var specificity = _matcher.GetSpecificity("div");
            Assert.Equal(0, specificity.A);
            Assert.Equal(0, specificity.B);
            Assert.Equal(1, specificity.C);
        }

        [Fact]
        public void GetSpecificity_CompoundSelector()
        {
            // div#main.container = 1 type + 1 id + 1 class = (1,1,1)
            var specificity = _matcher.GetSpecificity("div#main.container");
            Assert.Equal(1, specificity.A);
            Assert.Equal(1, specificity.B);
            Assert.Equal(1, specificity.C);
        }

        [Fact]
        public void GetSpecificity_AttributeSelector_CountsAsB()
        {
            var specificity = _matcher.GetSpecificity("[type=\"text\"]");
            Assert.Equal(0, specificity.A);
            Assert.Equal(1, specificity.B);
            Assert.Equal(0, specificity.C);
        }

        [Fact]
        public void GetSpecificity_PseudoClass_CountsAsB()
        {
            var specificity = _matcher.GetSpecificity(":hover");
            Assert.Equal(0, specificity.A);
            Assert.Equal(1, specificity.B);
            Assert.Equal(0, specificity.C);
        }

        [Fact]
        public void GetSpecificity_PseudoElement_CountsAsC()
        {
            var specificity = _matcher.GetSpecificity("::before");
            Assert.Equal(0, specificity.A);
            Assert.Equal(0, specificity.B);
            Assert.Equal(1, specificity.C);
        }

        [Fact]
        public void GetSpecificity_UniversalSelector_AddsNothing()
        {
            var specificity = _matcher.GetSpecificity("*");
            Assert.Equal(0, specificity.A);
            Assert.Equal(0, specificity.B);
            Assert.Equal(0, specificity.C);
        }

        [Fact]
        public void GetSpecificity_ComplexSelector_WithDescendant()
        {
            // "div .item" = 1 type + 1 class = (0,1,1)
            var specificity = _matcher.GetSpecificity("div .item");
            Assert.Equal(0, specificity.A);
            Assert.Equal(1, specificity.B);
            Assert.Equal(1, specificity.C);
        }

        [Fact]
        public void GetSpecificity_MultipleClasses()
        {
            // ".a.b.c" = 3 classes = (0,3,0)
            var specificity = _matcher.GetSpecificity(".a.b.c");
            Assert.Equal(0, specificity.A);
            Assert.Equal(3, specificity.B);
            Assert.Equal(0, specificity.C);
        }

        [Fact]
        public void GetSpecificity_PseudoClassWithFunction()
        {
            // ":nth-child(2n+1)" = 1 pseudo-class = (0,1,0)
            var specificity = _matcher.GetSpecificity(":nth-child(2n+1)");
            Assert.Equal(0, specificity.A);
            Assert.Equal(1, specificity.B);
            Assert.Equal(0, specificity.C);
        }

        private static Rend.Html.Element? FindFirstElement(Rend.Html.Node node, string tagName)
        {
            if (node is Rend.Html.Element el && el.TagName == tagName)
                return el;

            var child = node.FirstChild;
            while (child != null)
            {
                var result = FindFirstElement(child, tagName);
                if (result != null) return result;
                child = child.NextSibling;
            }
            return null;
        }

        private class MockStylableElement : IStylableElement
        {
            public string TagName => "div";
            public string? Id => null;
            public System.Collections.Generic.IReadOnlyList<string> ClassList => System.Array.Empty<string>();
            public string? GetAttribute(string name) => null;
            public string? InlineStyle => null;
            public IStylableElement? Parent => null;
            public IStylableElement? PreviousSibling => null;
            public IStylableElement? NextSibling => null;
            public IStylableElement? FirstChild => null;
            public IStylableElement? LastChild => null;
            public System.Collections.Generic.IEnumerable<IStylableElement> Children
            {
                get { yield break; }
            }
        }
    }
}
