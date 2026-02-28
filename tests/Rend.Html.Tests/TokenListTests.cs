using System.Collections.Generic;
using System.Linq;
using Xunit;
using Rend.Html;

namespace Rend.Html.Tests
{
    public class TokenListTests
    {
        private static Element CreateElementWithClass(string? classValue)
        {
            var doc = new Document();
            var el = doc.CreateElement("div");
            if (classValue != null)
                el.SetAttribute("class", classValue);
            return el;
        }

        [Fact]
        public void Contains_ReturnsTrueForExistingToken()
        {
            var el = CreateElementWithClass("foo bar baz");
            Assert.True(el.ClassList.Contains("foo"));
            Assert.True(el.ClassList.Contains("bar"));
            Assert.True(el.ClassList.Contains("baz"));
        }

        [Fact]
        public void Contains_ReturnsFalseForMissingToken()
        {
            var el = CreateElementWithClass("foo bar");
            Assert.False(el.ClassList.Contains("baz"));
        }

        [Fact]
        public void Contains_ReturnsFalseForPartialMatch()
        {
            var el = CreateElementWithClass("foobar");
            Assert.False(el.ClassList.Contains("foo"));
        }

        [Fact]
        public void Contains_ReturnsFalseForEmptyClass()
        {
            var el = CreateElementWithClass(null);
            Assert.False(el.ClassList.Contains("foo"));
        }

        [Fact]
        public void Contains_ReturnsFalseForEmptyToken()
        {
            var el = CreateElementWithClass("foo bar");
            Assert.False(el.ClassList.Contains(""));
        }

        [Fact]
        public void Count_ReturnsCorrectCount()
        {
            var el = CreateElementWithClass("foo bar baz");
            Assert.Equal(3, el.ClassList.Count);
        }

        [Fact]
        public void Count_ReturnsZeroWhenEmpty()
        {
            var el = CreateElementWithClass(null);
            Assert.Equal(0, el.ClassList.Count);
        }

        [Fact]
        public void Count_HandlesExtraWhitespace()
        {
            var el = CreateElementWithClass("  foo   bar   ");
            Assert.Equal(2, el.ClassList.Count);
        }

        [Fact]
        public void Add_AddsNewToken()
        {
            var el = CreateElementWithClass("foo");
            el.ClassList.Add("bar");

            Assert.True(el.ClassList.Contains("foo"));
            Assert.True(el.ClassList.Contains("bar"));
            Assert.Equal(2, el.ClassList.Count);
        }

        [Fact]
        public void Add_DoesNotDuplicateExistingToken()
        {
            var el = CreateElementWithClass("foo bar");
            el.ClassList.Add("foo");

            Assert.Equal(2, el.ClassList.Count);
        }

        [Fact]
        public void Add_ToEmptyTokenList()
        {
            var el = CreateElementWithClass(null);
            el.ClassList.Add("new");

            Assert.True(el.ClassList.Contains("new"));
            Assert.Equal(1, el.ClassList.Count);
        }

        [Fact]
        public void Add_EmptyString_NoOp()
        {
            var el = CreateElementWithClass("foo");
            el.ClassList.Add("");

            Assert.Equal(1, el.ClassList.Count);
        }

        [Fact]
        public void Remove_RemovesExistingToken()
        {
            var el = CreateElementWithClass("foo bar baz");
            el.ClassList.Remove("bar");

            Assert.True(el.ClassList.Contains("foo"));
            Assert.False(el.ClassList.Contains("bar"));
            Assert.True(el.ClassList.Contains("baz"));
            Assert.Equal(2, el.ClassList.Count);
        }

        [Fact]
        public void Remove_NoOpWhenNotPresent()
        {
            var el = CreateElementWithClass("foo bar");
            el.ClassList.Remove("baz");

            Assert.Equal(2, el.ClassList.Count);
        }

        [Fact]
        public void Remove_EmptyString_NoOp()
        {
            var el = CreateElementWithClass("foo");
            el.ClassList.Remove("");

            Assert.Equal(1, el.ClassList.Count);
        }

        [Fact]
        public void Toggle_AddsWhenAbsent_ReturnsTrue()
        {
            var el = CreateElementWithClass("foo");
            var result = el.ClassList.Toggle("bar");

            Assert.True(result);
            Assert.True(el.ClassList.Contains("bar"));
        }

        [Fact]
        public void Toggle_RemovesWhenPresent_ReturnsFalse()
        {
            var el = CreateElementWithClass("foo bar");
            var result = el.ClassList.Toggle("bar");

            Assert.False(result);
            Assert.False(el.ClassList.Contains("bar"));
        }

        [Fact]
        public void Toggle_Twice_RestoresOriginalState()
        {
            var el = CreateElementWithClass("foo");
            el.ClassList.Toggle("bar");
            el.ClassList.Toggle("bar");

            Assert.False(el.ClassList.Contains("bar"));
            Assert.Equal(1, el.ClassList.Count);
        }

        [Fact]
        public void Indexer_ReturnsTokenAtPosition()
        {
            var el = CreateElementWithClass("alpha beta gamma");
            Assert.Equal("alpha", el.ClassList[0]);
            Assert.Equal("beta", el.ClassList[1]);
            Assert.Equal("gamma", el.ClassList[2]);
        }

        [Fact]
        public void Indexer_ReturnsNull_OutOfRange()
        {
            var el = CreateElementWithClass("foo");
            Assert.Null(el.ClassList[5]);
        }

        [Fact]
        public void Indexer_ReturnsNull_WhenEmpty()
        {
            var el = CreateElementWithClass(null);
            Assert.Null(el.ClassList[0]);
        }

        [Fact]
        public void Enumeration_YieldsAllTokens()
        {
            var el = CreateElementWithClass("a b c");
            var tokens = el.ClassList.ToList();

            Assert.Equal(3, tokens.Count);
            Assert.Contains("a", tokens);
            Assert.Contains("b", tokens);
            Assert.Contains("c", tokens);
        }

        [Fact]
        public void Enumeration_EmptyList_YieldsNothing()
        {
            var el = CreateElementWithClass(null);
            var tokens = el.ClassList.ToList();

            Assert.Empty(tokens);
        }

        [Fact]
        public void BackedByAttribute_ChangesReflectInAttribute()
        {
            var el = CreateElementWithClass("foo");
            el.ClassList.Add("bar");

            var attrValue = el.GetAttribute("class");
            Assert.NotNull(attrValue);
            Assert.Contains("foo", attrValue!);
            Assert.Contains("bar", attrValue);
        }

        [Fact]
        public void BackedByAttribute_DirectAttributeChange_ReflectsInClassList()
        {
            var el = CreateElementWithClass("foo");
            el.SetAttribute("class", "alpha beta");

            Assert.True(el.ClassList.Contains("alpha"));
            Assert.True(el.ClassList.Contains("beta"));
            Assert.False(el.ClassList.Contains("foo"));
        }
    }
}
