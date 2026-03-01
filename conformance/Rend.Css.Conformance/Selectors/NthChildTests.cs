using System.Linq;
using Rend.Html;
using Rend.Html.Parser;
using Xunit;

namespace Rend.Css.Conformance.Selectors
{
    public class NthChildTests
    {
        private static Document ParseDoc(string html)
        {
            return HtmlParser.Parse(html);
        }

        private const string FiveItemList =
            "<ul><li>1</li><li>2</li><li>3</li><li>4</li><li>5</li></ul>";

        private const string TenItemList =
            "<ul><li>1</li><li>2</li><li>3</li><li>4</li><li>5</li>" +
            "<li>6</li><li>7</li><li>8</li><li>9</li><li>10</li></ul>";

        #region Keywords

        [Fact]
        public void NthChild_Odd_MatchesOddPositions()
        {
            var doc = ParseDoc(FiveItemList);
            var results = doc.QuerySelectorAll("li:nth-child(odd)");
            Assert.Equal(3, results.Count); // 1st, 3rd, 5th
        }

        [Fact]
        public void NthChild_Even_MatchesEvenPositions()
        {
            var doc = ParseDoc(FiveItemList);
            var results = doc.QuerySelectorAll("li:nth-child(even)");
            Assert.Equal(2, results.Count); // 2nd, 4th
        }

        #endregion

        #region Specific Index

        [Theory]
        [InlineData("1", 1)]
        [InlineData("2", 1)]
        [InlineData("3", 1)]
        [InlineData("5", 1)]
        public void NthChild_SpecificNumber_MatchesSingleElement(string n, int expectedCount)
        {
            var doc = ParseDoc(FiveItemList);
            var results = doc.QuerySelectorAll($"li:nth-child({n})");
            Assert.Equal(expectedCount, results.Count);
        }

        [Fact]
        public void NthChild_OutOfRange_MatchesNone()
        {
            var doc = ParseDoc(FiveItemList);
            var results = doc.QuerySelectorAll("li:nth-child(10)");
            Assert.Empty(results);
        }

        #endregion

        #region An+B Formulas

        [Fact]
        public void NthChild_2n_MatchesEverySecond()
        {
            var doc = ParseDoc(TenItemList);
            var results = doc.QuerySelectorAll("li:nth-child(2n)");
            Assert.Equal(5, results.Count); // 2,4,6,8,10
        }

        [Fact]
        public void NthChild_2nPlus1_MatchesOdd()
        {
            var doc = ParseDoc(TenItemList);
            var results = doc.QuerySelectorAll("li:nth-child(2n+1)");
            Assert.Equal(5, results.Count); // 1,3,5,7,9
        }

        [Fact]
        public void NthChild_3n_MatchesEveryThird()
        {
            var doc = ParseDoc(TenItemList);
            var results = doc.QuerySelectorAll("li:nth-child(3n)");
            // 3, 6, 9
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public void NthChild_3nPlus1_MatchesCorrectly()
        {
            var doc = ParseDoc(TenItemList);
            var results = doc.QuerySelectorAll("li:nth-child(3n+1)");
            // 1, 4, 7, 10
            Assert.Equal(4, results.Count);
        }

        [Fact]
        public void NthChild_3nPlus2_MatchesCorrectly()
        {
            var doc = ParseDoc(TenItemList);
            var results = doc.QuerySelectorAll("li:nth-child(3n+2)");
            // 2, 5, 8
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public void NthChild_n_MatchesAll()
        {
            var doc = ParseDoc(FiveItemList);
            var results = doc.QuerySelectorAll("li:nth-child(n)");
            Assert.Equal(5, results.Count);
        }

        [Fact]
        public void NthChild_nPlus3_MatchesFromThirdOnward()
        {
            var doc = ParseDoc(FiveItemList);
            var results = doc.QuerySelectorAll("li:nth-child(n+3)");
            // 3, 4, 5
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public void NthChild_NegN_Plus5_MatchesFirstFive()
        {
            var doc = ParseDoc(TenItemList);
            var results = doc.QuerySelectorAll("li:nth-child(-n+5)");
            // 1, 2, 3, 4, 5
            Assert.Equal(5, results.Count);
        }

        [Fact]
        public void NthChild_NegN_Plus3_MatchesFirstThree()
        {
            var doc = ParseDoc(FiveItemList);
            var results = doc.QuerySelectorAll("li:nth-child(-n+3)");
            // 1, 2, 3
            Assert.Equal(3, results.Count);
        }

        #endregion

        #region nth-last-child

        [Fact]
        public void NthLastChild_1_MatchesLastElement()
        {
            var doc = ParseDoc(FiveItemList);
            var results = doc.QuerySelectorAll("li:nth-last-child(1)");
            Assert.Single(results);
        }

        [Fact]
        public void NthLastChild_2_MatchesSecondFromLast()
        {
            var doc = ParseDoc(FiveItemList);
            var results = doc.QuerySelectorAll("li:nth-last-child(2)");
            Assert.Single(results);
        }

        [Fact]
        public void NthLastChild_Even_MatchesCorrectly()
        {
            var doc = ParseDoc(FiveItemList);
            var results = doc.QuerySelectorAll("li:nth-last-child(even)");
            // Counting from end: 5th→1, 4th→2(even), 3rd→3, 2nd→4(even), 1st→5
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void NthLastChild_Odd_MatchesCorrectly()
        {
            var doc = ParseDoc(FiveItemList);
            var results = doc.QuerySelectorAll("li:nth-last-child(odd)");
            Assert.Equal(3, results.Count);
        }

        #endregion

        #region nth-of-type

        [Fact]
        public void NthOfType_MatchesByType()
        {
            var doc = ParseDoc("<div><span>a</span><p>1</p><span>b</span><p>2</p><span>c</span></div>");
            var results = doc.QuerySelectorAll("p:nth-of-type(2)");
            Assert.Single(results);
        }

        [Fact]
        public void NthOfType_Even_MatchesEvenOfType()
        {
            var doc = ParseDoc("<div><p>1</p><p>2</p><p>3</p><p>4</p></div>");
            var results = doc.QuerySelectorAll("p:nth-of-type(even)");
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void FirstOfType_MatchesFirstOfEachType()
        {
            var doc = ParseDoc("<div><span>a</span><p>b</p><span>c</span><p>d</p></div>");
            var results = doc.QuerySelectorAll("p:first-of-type");
            Assert.Single(results);
        }

        [Fact]
        public void LastOfType_MatchesLastOfEachType()
        {
            var doc = ParseDoc("<div><span>a</span><p>b</p><span>c</span><p>d</p></div>");
            var results = doc.QuerySelectorAll("p:last-of-type");
            Assert.Single(results);
        }

        [Fact]
        public void OnlyOfType_MatchesSoleTypeChild()
        {
            var doc = ParseDoc("<div><p>para</p><span>a</span><span>b</span></div>");
            var results = doc.QuerySelectorAll("p:only-of-type");
            Assert.Single(results);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void NthChild_SingleChild_MatchesFirst()
        {
            var doc = ParseDoc("<ul><li>only</li></ul>");
            var results = doc.QuerySelectorAll("li:nth-child(1)");
            Assert.Single(results);
        }

        [Fact]
        public void NthChild_EmptyList_MatchesNone()
        {
            var doc = ParseDoc("<ul></ul>");
            var results = doc.QuerySelectorAll("li:nth-child(1)");
            Assert.Empty(results);
        }

        #endregion
    }
}
