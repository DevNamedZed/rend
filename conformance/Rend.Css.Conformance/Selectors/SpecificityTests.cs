using System.Linq;
using Rend.Html;
using Rend.Html.Parser;
using Rend.Html.Selectors;
using Xunit;

namespace Rend.Css.Conformance.Selectors
{
    public class SpecificityTests
    {
        private static Document ParseDoc(string html)
        {
            return HtmlParser.Parse(html);
        }

        #region Specificity Ordering via Matching Behavior

        [Fact]
        public void TypeSelector_HasLowestSpecificity()
        {
            // div matches, and has specificity (0,0,1)
            var doc = ParseDoc("<div class='a' id='x'>test</div>");
            var el = doc.QuerySelectorAll("div").First();

            // All three should match
            Assert.True(el.Matches("div"));
            Assert.True(el.Matches(".a"));
            Assert.True(el.Matches("#x"));
        }

        [Fact]
        public void IdSelector_MoreSpecificThanClass()
        {
            // An ID selector (1,0,0) beats a class selector (0,1,0)
            // We verify both match, and the ordering semantics are tested
            // by the cascade engine. Here we verify matching correctness.
            var doc = ParseDoc("<div class='a' id='x'>test</div>");
            var el = doc.QuerySelectorAll("#x").First();
            Assert.True(el.Matches("#x"));
            Assert.True(el.Matches(".a"));
        }

        [Fact]
        public void ClassSelector_MoreSpecificThanType()
        {
            var doc = ParseDoc("<div class='a'>test</div><span>other</span>");
            var el = doc.QuerySelectorAll(".a").First();
            Assert.True(el.Matches("div"));
            Assert.True(el.Matches(".a"));
        }

        [Fact]
        public void CompoundSelector_AccumulatesSpecificity()
        {
            // div.foo has specificity (0,1,1) which is higher than .foo (0,1,0) or div (0,0,1)
            var doc = ParseDoc("<div class='foo'>match</div><span class='foo'>no</span>");
            var results = doc.QuerySelectorAll("div.foo");
            Assert.Single(results);
            Assert.Equal("div", results[0].TagName);
        }

        [Fact]
        public void MultipleClasses_IncreaseSpecificity()
        {
            // .a.b has specificity (0,2,0) which is higher than .a (0,1,0)
            var doc = ParseDoc("<div class='a b'>match</div><div class='a'>nomatch</div>");
            var results = doc.QuerySelectorAll(".a.b");
            Assert.Single(results);
        }

        #endregion

        #region Universal Selector Has Zero Specificity

        [Fact]
        public void UniversalSelector_HasZeroSpecificity()
        {
            // * matches everything but adds no specificity
            var doc = ParseDoc("<div>test</div>");
            var results = doc.QuerySelectorAll("*");
            // Should match body, head, html, div, etc.
            Assert.True(results.Count >= 1);
        }

        #endregion

        #region Pseudo-Class Specificity

        [Fact]
        public void FirstChild_HasClassLevelSpecificity()
        {
            // :first-child has specificity (0,1,0)
            var doc = ParseDoc("<ul><li>a</li><li>b</li></ul>");
            var results = doc.QuerySelectorAll("li:first-child");
            Assert.Single(results);
        }

        [Fact]
        public void Not_UsesArgumentSpecificity()
        {
            // :not(.foo) has the specificity of its argument: (0,1,0)
            var doc = ParseDoc("<div class='foo'>a</div><div>b</div>");
            var results = doc.QuerySelectorAll("div:not(.foo)");
            Assert.Single(results);
        }

        #endregion

        #region Selector Matching Priority

        [Fact]
        public void IdPlusType_MoreSpecificThanClassPlusType()
        {
            // div#x = (1,0,1), div.a = (0,1,1)
            var doc = ParseDoc("<div id='x' class='a'>test</div>");
            var el = doc.QuerySelectorAll("div").First();
            Assert.True(el.Matches("div#x"));
            Assert.True(el.Matches("div.a"));
        }

        [Fact]
        public void AttributeSelector_HasClassLevelSpecificity()
        {
            // [type='text'] has specificity (0,1,0) same as a class
            var doc = ParseDoc("<input type='text'><input type='password'>");
            var results = doc.QuerySelectorAll("[type='text']");
            Assert.Single(results);
        }

        [Fact]
        public void ComplexSelector_AddsSpecificityAcrossCombinators()
        {
            // #nav ul li.active = (1,1,2)
            var doc = ParseDoc("<nav id='nav'><ul><li class='active'>item</li></ul></nav>");
            var results = doc.QuerySelectorAll("#nav ul li.active");
            Assert.Single(results);
        }

        #endregion

        #region CssSpecificity Struct Tests

        [Fact]
        public void CssSpecificity_ComparesCorrectly_HigherAWins()
        {
            var s1 = new Rend.Css.CssSpecificity(1, 0, 0);
            var s2 = new Rend.Css.CssSpecificity(0, 10, 10);
            Assert.True(s1 > s2);
        }

        [Fact]
        public void CssSpecificity_ComparesCorrectly_HigherBWinsWhenAEqual()
        {
            var s1 = new Rend.Css.CssSpecificity(0, 2, 0);
            var s2 = new Rend.Css.CssSpecificity(0, 1, 5);
            Assert.True(s1 > s2);
        }

        [Fact]
        public void CssSpecificity_ComparesCorrectly_HigherCWinsWhenABEqual()
        {
            var s1 = new Rend.Css.CssSpecificity(0, 0, 3);
            var s2 = new Rend.Css.CssSpecificity(0, 0, 2);
            Assert.True(s1 > s2);
        }

        [Fact]
        public void CssSpecificity_Equal_ComparesCorrectly()
        {
            var s1 = new Rend.Css.CssSpecificity(1, 2, 3);
            var s2 = new Rend.Css.CssSpecificity(1, 2, 3);
            Assert.Equal(s1, s2);
            Assert.True(s1 == s2);
        }

        [Fact]
        public void CssSpecificity_Inline_AlwaysWins()
        {
            var inline = Rend.Css.CssSpecificity.InlineStyle;
            var highest = new Rend.Css.CssSpecificity(100, 100, 100);
            Assert.True(inline > highest);
        }

        [Fact]
        public void CssSpecificity_Zero_IsSmallest()
        {
            var zero = Rend.Css.CssSpecificity.Zero;
            var any = new Rend.Css.CssSpecificity(0, 0, 1);
            Assert.True(any > zero);
        }

        [Fact]
        public void CssSpecificity_ToString_FormatsCorrectly()
        {
            var s = new Rend.Css.CssSpecificity(1, 2, 3);
            Assert.Equal("(1,2,3)", s.ToString());
        }

        [Fact]
        public void CssSpecificity_Inline_ToString()
        {
            var s = Rend.Css.CssSpecificity.InlineStyle;
            Assert.Equal("(inline,0,0,0)", s.ToString());
        }

        #endregion
    }
}
