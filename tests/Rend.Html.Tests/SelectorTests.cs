using Xunit;
using Rend.Html;
using Rend.Html.Parser;
using Rend.Html.Selectors;

namespace Rend.Html.Tests
{
    public class SelectorTests
    {
        private static Document ParseDoc(string bodyContent)
        {
            return HtmlParser.Parse($"<html><head></head><body>{bodyContent}</body></html>");
        }

        // --- Type selector ---

        [Fact]
        public void TypeSelector_MatchesByTagName()
        {
            var doc = ParseDoc("<div></div><span></span>");
            var results = doc.QuerySelectorAll("div");
            Assert.Single(results);
            Assert.Equal("div", results[0].TagName);
        }

        [Fact]
        public void TypeSelector_CaseInsensitive()
        {
            var doc = ParseDoc("<DIV></DIV>");
            var results = doc.QuerySelectorAll("div");
            Assert.Single(results);
        }

        // --- Universal selector ---

        [Fact]
        public void UniversalSelector_MatchesAllElements()
        {
            var doc = ParseDoc("<div><p></p><span></span></div>");
            var results = doc.QuerySelectorAll("*");
            // Should match html, head, body, div, p, span at minimum
            Assert.True(results.Count >= 6);
        }

        // --- ID selector ---

        [Fact]
        public void IdSelector_MatchesById()
        {
            var doc = ParseDoc("<div id=\"target\">Found</div><div id=\"other\">Other</div>");
            var results = doc.QuerySelectorAll("#target");
            Assert.Single(results);
            Assert.Equal("Found", results[0].TextContent);
        }

        [Fact]
        public void IdSelector_NoMatch_ReturnsEmpty()
        {
            var doc = ParseDoc("<div id=\"nope\"></div>");
            var results = doc.QuerySelectorAll("#missing");
            Assert.Empty(results);
        }

        // --- Class selector ---

        [Fact]
        public void ClassSelector_MatchesByClass()
        {
            var doc = ParseDoc("<div class=\"foo\">1</div><div class=\"bar\">2</div><div class=\"foo bar\">3</div>");
            var results = doc.QuerySelectorAll(".foo");
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void ClassSelector_DoesNotMatchPartialClassName()
        {
            var doc = ParseDoc("<div class=\"foobar\">1</div>");
            var results = doc.QuerySelectorAll(".foo");
            Assert.Empty(results);
        }

        // --- Attribute selectors ---

        [Fact]
        public void AttributeSelector_Exists()
        {
            var doc = ParseDoc("<input disabled><input>");
            var results = doc.QuerySelectorAll("[disabled]");
            Assert.Single(results);
        }

        [Fact]
        public void AttributeSelector_Equals()
        {
            var doc = ParseDoc("<input type=\"text\"><input type=\"password\">");
            var results = doc.QuerySelectorAll("[type=\"text\"]");
            Assert.Single(results);
        }

        [Fact]
        public void AttributeSelector_Prefix()
        {
            var doc = ParseDoc("<div data-x=\"hello-world\"></div><div data-x=\"goodbye\"></div>");
            var results = doc.QuerySelectorAll("[data-x^=\"hello\"]");
            Assert.Single(results);
        }

        [Fact]
        public void AttributeSelector_Suffix()
        {
            var doc = ParseDoc("<div data-x=\"hello-world\"></div><div data-x=\"goodbye\"></div>");
            var results = doc.QuerySelectorAll("[data-x$=\"world\"]");
            Assert.Single(results);
        }

        [Fact]
        public void AttributeSelector_Substring()
        {
            var doc = ParseDoc("<div data-x=\"hello-world\"></div><div data-x=\"goodbye\"></div>");
            var results = doc.QuerySelectorAll("[data-x*=\"lo-wo\"]");
            Assert.Single(results);
        }

        [Fact]
        public void AttributeSelector_Includes()
        {
            var doc = ParseDoc("<div data-x=\"foo bar baz\"></div><div data-x=\"foobar\"></div>");
            var results = doc.QuerySelectorAll("[data-x~=\"bar\"]");
            Assert.Single(results);
        }

        [Fact]
        public void AttributeSelector_DashMatch()
        {
            var doc = ParseDoc("<div lang=\"en\"></div><div lang=\"en-US\"></div><div lang=\"fr\"></div>");
            var results = doc.QuerySelectorAll("[lang|=\"en\"]");
            Assert.Equal(2, results.Count);
        }

        // --- Compound selectors ---

        [Fact]
        public void CompoundSelector_TypeAndClass()
        {
            var doc = ParseDoc("<div class=\"x\">1</div><span class=\"x\">2</span>");
            var results = doc.QuerySelectorAll("div.x");
            Assert.Single(results);
            Assert.Equal("div", results[0].TagName);
        }

        [Fact]
        public void CompoundSelector_TypeClassAndId()
        {
            var doc = ParseDoc("<div class=\"x\" id=\"y\">1</div><div class=\"x\">2</div>");
            var results = doc.QuerySelectorAll("div.x#y");
            Assert.Single(results);
            Assert.Equal("1", results[0].TextContent);
        }

        [Fact]
        public void CompoundSelector_TypeAndAttribute()
        {
            var doc = ParseDoc("<input type=\"text\"><input type=\"password\"><div type=\"text\"></div>");
            var results = doc.QuerySelectorAll("input[type=\"text\"]");
            Assert.Single(results);
        }

        // --- Descendant combinator ---

        [Fact]
        public void DescendantCombinator_MatchesDescendants()
        {
            var doc = ParseDoc("<div><p><span>deep</span></p></div>");
            var results = doc.QuerySelectorAll("div span");
            Assert.Single(results);
            Assert.Equal("deep", results[0].TextContent);
        }

        [Fact]
        public void DescendantCombinator_DoesNotMatchSiblings()
        {
            var doc = ParseDoc("<div></div><span>sibling</span>");
            var results = doc.QuerySelectorAll("div span");
            Assert.Empty(results);
        }

        // --- Child combinator ---

        [Fact]
        public void ChildCombinator_MatchesDirectChild()
        {
            var doc = ParseDoc("<div><span>child</span></div>");
            var results = doc.QuerySelectorAll("div > span");
            Assert.Single(results);
        }

        [Fact]
        public void ChildCombinator_DoesNotMatchGrandchild()
        {
            var doc = ParseDoc("<div><p><span>grandchild</span></p></div>");
            var results = doc.QuerySelectorAll("div > span");
            Assert.Empty(results);
        }

        // --- Next sibling combinator ---

        [Fact]
        public void NextSiblingCombinator_MatchesImmediateNextSibling()
        {
            var doc = ParseDoc("<p>1</p><span>2</span><span>3</span>");
            var results = doc.QuerySelectorAll("p + span");
            Assert.Single(results);
            Assert.Equal("2", results[0].TextContent);
        }

        [Fact]
        public void NextSiblingCombinator_DoesNotMatchNonAdjacentSibling()
        {
            var doc = ParseDoc("<p>1</p><div>2</div><span>3</span>");
            var results = doc.QuerySelectorAll("p + span");
            Assert.Empty(results);
        }

        // --- Subsequent sibling combinator ---

        [Fact]
        public void SubsequentSiblingCombinator_MatchesAnySiblingAfter()
        {
            var doc = ParseDoc("<p>1</p><div>2</div><span>3</span><span>4</span>");
            var results = doc.QuerySelectorAll("p ~ span");
            Assert.Equal(2, results.Count);
        }

        // --- Selector list (comma) ---

        [Fact]
        public void SelectorList_MatchesAny()
        {
            var doc = ParseDoc("<div>1</div><span>2</span><p>3</p>");
            var results = doc.QuerySelectorAll("div, span");
            Assert.Equal(2, results.Count);
        }

        // --- SelectorList.Parse and Matches ---

        [Fact]
        public void SelectorList_Parse_MatchesElement()
        {
            var doc = ParseDoc("<div class=\"active\"></div>");
            var div = doc.QuerySelector("div")!;

            var selector = SelectorList.Parse("div.active");
            Assert.True(selector.Matches(div));
        }

        [Fact]
        public void SelectorList_Parse_DoesNotMatchNonMatching()
        {
            var doc = ParseDoc("<div class=\"active\"></div>");
            var div = doc.QuerySelector("div")!;

            var selector = SelectorList.Parse("span.active");
            Assert.False(selector.Matches(div));
        }

        [Fact]
        public void SelectorList_Parse_CommaSeparated()
        {
            var doc = ParseDoc("<div></div><span></span>");
            var div = doc.QuerySelector("div")!;
            var span = doc.QuerySelector("span")!;

            var selector = SelectorList.Parse("div, span");
            Assert.True(selector.Matches(div));
            Assert.True(selector.Matches(span));
        }

        [Fact]
        public void SelectorList_QuerySelector_FindsFirst()
        {
            var doc = ParseDoc("<p>1</p><p>2</p>");
            var selector = SelectorList.Parse("p");

            var result = selector.QuerySelector(doc);
            Assert.NotNull(result);
            Assert.Equal("1", result!.TextContent);
        }

        [Fact]
        public void SelectorList_QuerySelectorAll_FindsAll()
        {
            var doc = ParseDoc("<p>1</p><div><p>2</p></div>");
            var selector = SelectorList.Parse("p");

            var results = selector.QuerySelectorAll(doc);
            Assert.Equal(2, results.Count);
        }

        // --- Pseudo-classes ---

        [Fact]
        public void PseudoClass_FirstChild()
        {
            var doc = ParseDoc("<div><p>1</p><p>2</p><p>3</p></div>");
            var results = doc.QuerySelectorAll("p:first-child");
            Assert.Single(results);
            Assert.Equal("1", results[0].TextContent);
        }

        [Fact]
        public void PseudoClass_LastChild()
        {
            var doc = ParseDoc("<div><p>1</p><p>2</p><p>3</p></div>");
            var results = doc.QuerySelectorAll("p:last-child");
            Assert.Single(results);
            Assert.Equal("3", results[0].TextContent);
        }

        [Fact]
        public void PseudoClass_NthChild()
        {
            var doc = ParseDoc("<div><p>1</p><p>2</p><p>3</p></div>");
            var results = doc.QuerySelectorAll("p:nth-child(2)");
            Assert.Single(results);
            Assert.Equal("2", results[0].TextContent);
        }

        [Fact]
        public void PseudoClass_OnlyChild()
        {
            var doc = ParseDoc("<div><p>only</p></div><div><p>1</p><p>2</p></div>");
            var results = doc.QuerySelectorAll("p:only-child");
            Assert.Single(results);
            Assert.Equal("only", results[0].TextContent);
        }

        [Fact]
        public void PseudoClass_Empty()
        {
            var doc = ParseDoc("<div></div><div>text</div><div><p></p></div>");
            var results = doc.QuerySelectorAll("div:empty");
            Assert.Single(results);
        }

        [Fact]
        public void PseudoClass_Not()
        {
            var doc = ParseDoc("<p class=\"a\">1</p><p class=\"b\">2</p><p class=\"a\">3</p>");
            var results = doc.QuerySelectorAll("p:not(.a)");
            Assert.Single(results);
            Assert.Equal("2", results[0].TextContent);
        }

        [Fact]
        public void PseudoClass_Root()
        {
            var doc = ParseDoc("<div></div>");
            var results = doc.QuerySelectorAll(":root");
            Assert.Single(results);
            Assert.Equal("html", results[0].TagName);
        }

        [Fact]
        public void PseudoClass_FirstOfType()
        {
            var doc = ParseDoc("<div><span>1</span><p>2</p><span>3</span><p>4</p></div>");
            var results = doc.QuerySelectorAll("span:first-of-type");
            // The one inside the div plus possibly others - just check it found the right one
            Assert.True(results.Count >= 1);
            Assert.Equal("1", results[0].TextContent);
        }

        [Fact]
        public void PseudoClass_LastOfType()
        {
            var doc = ParseDoc("<div><span>1</span><p>2</p><span>3</span><p>4</p></div>");
            var results = doc.QuerySelectorAll("div > span:last-of-type");
            Assert.Single(results);
            Assert.Equal("3", results[0].TextContent);
        }

        [Fact]
        public void PseudoClass_OnlyOfType()
        {
            var doc = ParseDoc("<div><span>only-span</span><p>1</p><p>2</p></div>");
            var results = doc.QuerySelectorAll("div > span:only-of-type");
            Assert.Single(results);
            Assert.Equal("only-span", results[0].TextContent);
        }

        [Fact]
        public void PseudoClass_NthChild_Even()
        {
            var doc = ParseDoc("<ul><li>1</li><li>2</li><li>3</li><li>4</li></ul>");
            var results = doc.QuerySelectorAll("li:nth-child(even)");
            Assert.Equal(2, results.Count);
            Assert.Equal("2", results[0].TextContent);
            Assert.Equal("4", results[1].TextContent);
        }

        [Fact]
        public void PseudoClass_NthChild_Odd()
        {
            var doc = ParseDoc("<ul><li>1</li><li>2</li><li>3</li><li>4</li></ul>");
            var results = doc.QuerySelectorAll("li:nth-child(odd)");
            Assert.Equal(2, results.Count);
            Assert.Equal("1", results[0].TextContent);
            Assert.Equal("3", results[1].TextContent);
        }

        // --- Complex selectors ---

        [Fact]
        public void ComplexSelector_MultiLevel()
        {
            var doc = ParseDoc("<div class=\"a\"><ul><li class=\"b\">target</li></ul></div>");
            var results = doc.QuerySelectorAll("div.a li.b");
            Assert.Single(results);
            Assert.Equal("target", results[0].TextContent);
        }

        [Fact]
        public void ComplexSelector_ChildThenDescendant()
        {
            var doc = ParseDoc("<div><ul><li><span>deep</span></li></ul></div>");
            var results = doc.QuerySelectorAll("div > ul span");
            Assert.Single(results);
            Assert.Equal("deep", results[0].TextContent);
        }
    }
}
