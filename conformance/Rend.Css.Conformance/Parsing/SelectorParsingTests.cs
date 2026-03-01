using System.Linq;
using Rend.Css;
using Xunit;

namespace Rend.Css.Conformance.Parsing
{
    public class SelectorParsingTests
    {
        private static string GetSelectorText(string css)
        {
            var sheet = CssParser.Parse(css);
            var rule = sheet.Rules.OfType<StyleRule>().First();
            return rule.SelectorText;
        }

        #region Simple Selectors

        [Theory]
        [InlineData("div { }", "div")]
        [InlineData("span { }", "span")]
        [InlineData("p { }", "p")]
        [InlineData("body { }", "body")]
        [InlineData("h1 { }", "h1")]
        public void TypeSelector_ParsesCorrectly(string css, string expected)
        {
            Assert.Contains(expected, GetSelectorText(css));
        }

        [Theory]
        [InlineData(".foo { }", ".foo")]
        [InlineData(".bar-baz { }", ".bar-baz")]
        [InlineData(".under_score { }", ".under_score")]
        public void ClassSelector_ParsesCorrectly(string css, string expected)
        {
            Assert.Contains(expected, GetSelectorText(css));
        }

        [Theory]
        [InlineData("#main { }", "#main")]
        [InlineData("#content { }", "#content")]
        public void IdSelector_ParsesCorrectly(string css, string expected)
        {
            Assert.Contains(expected, GetSelectorText(css));
        }

        [Fact]
        public void UniversalSelector_ParsesCorrectly()
        {
            var text = GetSelectorText("* { }");
            Assert.Contains("*", text);
        }

        #endregion

        #region Attribute Selectors

        [Fact]
        public void AttributeExists_ParsesCorrectly()
        {
            var text = GetSelectorText("[disabled] { }");
            Assert.Contains("[disabled]", text);
        }

        [Fact]
        public void AttributeEquals_ParsesCorrectly()
        {
            var text = GetSelectorText("[type='text'] { }");
            Assert.Contains("type", text);
            Assert.Contains("text", text);
        }

        [Fact]
        public void AttributeStartsWith_ParsesCorrectly()
        {
            var text = GetSelectorText("[href^='https'] { }");
            Assert.Contains("href", text);
        }

        [Fact]
        public void AttributeEndsWith_ParsesCorrectly()
        {
            var text = GetSelectorText("[href$='.pdf'] { }");
            Assert.Contains("href", text);
        }

        [Fact]
        public void AttributeContains_ParsesCorrectly()
        {
            var text = GetSelectorText("[class*='btn'] { }");
            Assert.Contains("class", text);
        }

        #endregion

        #region Compound Selectors

        [Fact]
        public void TypeAndClass_ParsesTogether()
        {
            var text = GetSelectorText("div.container { }");
            Assert.Contains("div", text);
            Assert.Contains(".container", text);
        }

        [Fact]
        public void TypeClassAndId_ParsesTogether()
        {
            var text = GetSelectorText("div.active#main { }");
            Assert.Contains("div", text);
            Assert.Contains(".active", text);
            Assert.Contains("#main", text);
        }

        [Fact]
        public void MultipleClasses_ParsesTogether()
        {
            var text = GetSelectorText(".foo.bar.baz { }");
            Assert.Contains(".foo", text);
            Assert.Contains(".bar", text);
            Assert.Contains(".baz", text);
        }

        #endregion

        #region Combinators

        [Fact]
        public void DescendantCombinator_ParsesCorrectly()
        {
            var text = GetSelectorText("div p { }");
            Assert.Contains("div", text);
            Assert.Contains("p", text);
        }

        [Fact]
        public void ChildCombinator_ParsesCorrectly()
        {
            var text = GetSelectorText("ul > li { }");
            Assert.Contains("ul", text);
            Assert.Contains("li", text);
            Assert.Contains(">", text);
        }

        [Fact]
        public void AdjacentSiblingCombinator_ParsesCorrectly()
        {
            var text = GetSelectorText("h1 + p { }");
            Assert.Contains("h1", text);
            Assert.Contains("p", text);
            Assert.Contains("+", text);
        }

        [Fact]
        public void GeneralSiblingCombinator_ParsesCorrectly()
        {
            var text = GetSelectorText("h1 ~ p { }");
            Assert.Contains("h1", text);
            Assert.Contains("p", text);
            Assert.Contains("~", text);
        }

        #endregion

        #region Pseudo-Classes in Selectors

        [Theory]
        [InlineData("a:hover { }")]
        [InlineData("a:focus { }")]
        [InlineData("a:active { }")]
        [InlineData("a:visited { }")]
        [InlineData("a:link { }")]
        public void PseudoClass_ParsesWithoutError(string css)
        {
            var sheet = CssParser.Parse(css);
            Assert.Single(sheet.Rules);
        }

        [Fact]
        public void NthChild_InSelector_Parses()
        {
            var text = GetSelectorText("li:nth-child(2n+1) { }");
            Assert.Contains("nth-child", text);
        }

        [Fact]
        public void Not_InSelector_Parses()
        {
            var text = GetSelectorText("p:not(.hidden) { }");
            Assert.Contains("not", text);
        }

        #endregion

        #region Pseudo-Elements

        [Fact]
        public void Before_PseudoElement_Parses()
        {
            var sheet = CssParser.Parse("div::before { content: ''; }");
            Assert.Single(sheet.Rules);
        }

        [Fact]
        public void After_PseudoElement_Parses()
        {
            var sheet = CssParser.Parse("div::after { content: ''; }");
            Assert.Single(sheet.Rules);
        }

        [Fact]
        public void FirstLine_PseudoElement_Parses()
        {
            var sheet = CssParser.Parse("p::first-line { font-weight: bold; }");
            Assert.Single(sheet.Rules);
        }

        [Fact]
        public void FirstLetter_PseudoElement_Parses()
        {
            var sheet = CssParser.Parse("p::first-letter { font-size: 2em; }");
            Assert.Single(sheet.Rules);
        }

        #endregion

        #region Selector Lists

        [Fact]
        public void CommaSeparatedList_ProducesSingleRule()
        {
            var sheet = CssParser.Parse("h1, h2, h3 { color: red; }");
            Assert.Single(sheet.Rules);
            var text = GetSelectorText("h1, h2, h3 { color: red; }");
            Assert.Contains("h1", text);
            Assert.Contains("h2", text);
            Assert.Contains("h3", text);
        }

        [Fact]
        public void ComplexSelectorList_ProducesSingleRule()
        {
            var sheet = CssParser.Parse("div > p, .container span { color: red; }");
            Assert.Single(sheet.Rules);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void WhitespaceAroundCombinators_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("div   >   p { color: red; }");
            Assert.Single(sheet.Rules);
        }

        [Fact]
        public void DeepSelector_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("html body div.container > main article > p.intro span { color: red; }");
            Assert.Single(sheet.Rules);
        }

        #endregion
    }
}
