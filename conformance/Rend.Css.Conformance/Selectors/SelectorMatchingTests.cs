using System.Linq;
using Rend.Html;
using Rend.Html.Parser;
using Xunit;

namespace Rend.Css.Conformance.Selectors
{
    public class SelectorMatchingTests
    {
        private static Document ParseDoc(string html)
        {
            return HtmlParser.Parse(html);
        }

        #region Type Selectors

        [Fact]
        public void TypeSelector_Div_MatchesDivElements()
        {
            var doc = ParseDoc("<div>hello</div><span>world</span>");
            var results = doc.QuerySelectorAll("div");
            Assert.Single(results);
            Assert.Equal("div", results[0].TagName);
        }

        [Fact]
        public void TypeSelector_Span_MatchesSpanElements()
        {
            var doc = ParseDoc("<div><span>a</span><span>b</span></div>");
            var results = doc.QuerySelectorAll("span");
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void TypeSelector_P_MatchesParagraphs()
        {
            var doc = ParseDoc("<p>one</p><p>two</p><div>three</div>");
            var results = doc.QuerySelectorAll("p");
            Assert.Equal(2, results.Count);
        }

        #endregion

        #region Class Selectors

        [Fact]
        public void ClassSelector_MatchesElementWithClass()
        {
            var doc = ParseDoc("<div class='foo'>a</div><div class='bar'>b</div>");
            var results = doc.QuerySelectorAll(".foo");
            Assert.Single(results);
        }

        [Fact]
        public void ClassSelector_MultipleClasses_MatchesIfContains()
        {
            var doc = ParseDoc("<div class='foo bar baz'>a</div>");
            var results = doc.QuerySelectorAll(".bar");
            Assert.Single(results);
        }

        [Fact]
        public void ClassSelector_NoMatch_ReturnsEmpty()
        {
            var doc = ParseDoc("<div class='foo'>a</div>");
            var results = doc.QuerySelectorAll(".nonexistent");
            Assert.Empty(results);
        }

        #endregion

        #region ID Selectors

        [Fact]
        public void IdSelector_MatchesElementWithId()
        {
            var doc = ParseDoc("<div id='main'>content</div><div id='sidebar'>nav</div>");
            var results = doc.QuerySelectorAll("#main");
            Assert.Single(results);
            Assert.Equal("main", results[0].Id);
        }

        [Fact]
        public void IdSelector_NoMatch_ReturnsEmpty()
        {
            var doc = ParseDoc("<div id='main'>content</div>");
            var results = doc.QuerySelectorAll("#footer");
            Assert.Empty(results);
        }

        #endregion

        #region Attribute Selectors

        [Fact]
        public void AttributeSelector_Exists_MatchesIfPresent()
        {
            var doc = ParseDoc("<input type='text'><input><div>no</div>");
            var results = doc.QuerySelectorAll("[type]");
            Assert.Single(results);
        }

        [Fact]
        public void AttributeSelector_Equals_MatchesExactValue()
        {
            var doc = ParseDoc("<input type='text'><input type='password'>");
            var results = doc.QuerySelectorAll("[type='text']");
            Assert.Single(results);
        }

        [Fact]
        public void AttributeSelector_Includes_MatchesWordInList()
        {
            var doc = ParseDoc("<div class='foo bar baz'>a</div><div class='qux'>b</div>");
            var results = doc.QuerySelectorAll("[class~='bar']");
            Assert.Single(results);
        }

        [Fact]
        public void AttributeSelector_DashMatch_MatchesPrefixWithDash()
        {
            var doc = ParseDoc("<div lang='en'>a</div><div lang='en-US'>b</div><div lang='fr'>c</div>");
            var results = doc.QuerySelectorAll("[lang|='en']");
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void AttributeSelector_Prefix_MatchesStartsWith()
        {
            var doc = ParseDoc("<a href='https://example.com'>a</a><a href='http://test.com'>b</a>");
            var results = doc.QuerySelectorAll("[href^='https']");
            Assert.Single(results);
        }

        [Fact]
        public void AttributeSelector_Suffix_MatchesEndsWith()
        {
            var doc = ParseDoc("<a href='doc.pdf'>a</a><a href='page.html'>b</a>");
            var results = doc.QuerySelectorAll("[href$='.pdf']");
            Assert.Single(results);
        }

        [Fact]
        public void AttributeSelector_Substring_MatchesContains()
        {
            var doc = ParseDoc("<a href='https://example.com/page'>a</a><a href='http://test.com'>b</a>");
            var results = doc.QuerySelectorAll("[href*='example']");
            Assert.Single(results);
        }

        #endregion

        #region Descendant Combinator

        [Fact]
        public void DescendantCombinator_MatchesNestedElements()
        {
            var doc = ParseDoc("<div><p><span>text</span></p></div>");
            var results = doc.QuerySelectorAll("div span");
            Assert.Single(results);
            Assert.Equal("span", results[0].TagName);
        }

        [Fact]
        public void DescendantCombinator_DeepNesting_Matches()
        {
            var doc = ParseDoc("<div><section><article><p>deep</p></article></section></div>");
            var results = doc.QuerySelectorAll("div p");
            Assert.Single(results);
        }

        #endregion

        #region Child Combinator

        [Fact]
        public void ChildCombinator_MatchesDirectChildren()
        {
            var doc = ParseDoc("<div><p>direct</p><section><p>nested</p></section></div>");
            var results = doc.QuerySelectorAll("div > p");
            Assert.Single(results);
        }

        [Fact]
        public void ChildCombinator_NoMatch_ForNonDirectChild()
        {
            var doc = ParseDoc("<div><section><p>nested</p></section></div>");
            var results = doc.QuerySelectorAll("div > p");
            Assert.Empty(results);
        }

        #endregion

        #region Adjacent Sibling Combinator

        [Fact]
        public void AdjacentSibling_MatchesImmediateNextSibling()
        {
            var doc = ParseDoc("<div><h1>title</h1><p>first</p><p>second</p></div>");
            var results = doc.QuerySelectorAll("h1 + p");
            Assert.Single(results);
        }

        [Fact]
        public void AdjacentSibling_NoMatch_ForNonAdjacent()
        {
            var doc = ParseDoc("<div><h1>title</h1><div>gap</div><p>para</p></div>");
            var results = doc.QuerySelectorAll("h1 + p");
            Assert.Empty(results);
        }

        #endregion

        #region General Sibling Combinator

        [Fact]
        public void GeneralSibling_MatchesAllFollowingSiblings()
        {
            var doc = ParseDoc("<div><h1>title</h1><p>a</p><div>b</div><p>c</p></div>");
            var results = doc.QuerySelectorAll("h1 ~ p");
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void GeneralSibling_NoMatch_ForPrecedingSiblings()
        {
            var doc = ParseDoc("<div><p>before</p><h1>title</h1></div>");
            var results = doc.QuerySelectorAll("h1 ~ p");
            Assert.Empty(results);
        }

        #endregion

        #region Pseudo-Classes

        [Fact]
        public void FirstChild_MatchesFirstElementChild()
        {
            var doc = ParseDoc("<ul><li>first</li><li>second</li><li>third</li></ul>");
            var results = doc.QuerySelectorAll("li:first-child");
            Assert.Single(results);
        }

        [Fact]
        public void LastChild_MatchesLastElementChild()
        {
            var doc = ParseDoc("<ul><li>first</li><li>second</li><li>third</li></ul>");
            var results = doc.QuerySelectorAll("li:last-child");
            Assert.Single(results);
        }

        [Fact]
        public void NthChild_Even_MatchesEvenChildren()
        {
            var doc = ParseDoc("<ul><li>1</li><li>2</li><li>3</li><li>4</li></ul>");
            var results = doc.QuerySelectorAll("li:nth-child(2n)");
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void NthChild_Odd_MatchesOddChildren()
        {
            var doc = ParseDoc("<ul><li>1</li><li>2</li><li>3</li><li>4</li></ul>");
            var results = doc.QuerySelectorAll("li:nth-child(2n+1)");
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void NthChild_Specific_MatchesNthChild()
        {
            var doc = ParseDoc("<ul><li>1</li><li>2</li><li>3</li></ul>");
            var results = doc.QuerySelectorAll("li:nth-child(2)");
            Assert.Single(results);
        }

        [Fact]
        public void OnlyChild_MatchesSoleChild()
        {
            var doc = ParseDoc("<div><p>only</p></div><div><p>a</p><p>b</p></div>");
            var results = doc.QuerySelectorAll("p:only-child");
            Assert.Single(results);
        }

        [Fact]
        public void Not_ExcludesMatchingElements()
        {
            var doc = ParseDoc("<div><p class='skip'>a</p><p>b</p><p>c</p></div>");
            var results = doc.QuerySelectorAll("p:not(.skip)");
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void Is_MatchesAnyInList()
        {
            var doc = ParseDoc("<div><h1>a</h1><h2>b</h2><p>c</p></div>");
            var results = doc.QuerySelectorAll(":is(h1, h2)");
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void Empty_MatchesElementsWithNoChildren()
        {
            var doc = ParseDoc("<div><span></span><span>text</span></div>");
            var results = doc.QuerySelectorAll("span:empty");
            Assert.Single(results);
        }

        #endregion

        #region Universal Selector

        [Fact]
        public void UniversalSelector_MatchesAllElements()
        {
            var doc = ParseDoc("<div><span>a</span></div>");
            var results = doc.QuerySelectorAll("div > *");
            Assert.Single(results);
            Assert.Equal("span", results[0].TagName);
        }

        #endregion

        #region Compound Selectors

        [Fact]
        public void TypeAndClass_MatchesCombination()
        {
            var doc = ParseDoc("<div class='active'>a</div><span class='active'>b</span>");
            var results = doc.QuerySelectorAll("div.active");
            Assert.Single(results);
        }

        [Fact]
        public void TypeClassAndId_MatchesCombination()
        {
            var doc = ParseDoc("<div id='main' class='active'>a</div><div class='active'>b</div>");
            var results = doc.QuerySelectorAll("div.active#main");
            Assert.Single(results);
        }

        #endregion

        #region Complex Selectors

        [Fact]
        public void ComplexSelector_MultipleCombinatorsWork()
        {
            var doc = ParseDoc("<div id='root'><ul><li class='item'><a href='#'>link</a></li></ul></div>");
            var results = doc.QuerySelectorAll("#root ul > li.item a");
            Assert.Single(results);
        }

        [Fact]
        public void SelectorList_CommaSeparated_MatchesUnion()
        {
            var doc = ParseDoc("<div>a</div><span>b</span><p>c</p>");
            var results = doc.QuerySelectorAll("div, span");
            Assert.Equal(2, results.Count);
        }

        #endregion
    }
}
