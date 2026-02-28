using System;
using Rend.Core;
using Xunit;

namespace Rend.Core.Tests
{
    public class StringPoolTests
    {
        [Fact]
        public void Intern_ReturnsSameReference()
        {
            var pool = new StringPool();
            string a = new string(new[] { 'h', 'e', 'l', 'l', 'o' });
            string b = new string(new[] { 'h', 'e', 'l', 'l', 'o' });
            // Ensure they are different references before interning
            Assert.False(ReferenceEquals(a, b));

            string internedA = pool.Intern(a);
            string internedB = pool.Intern(b);
            Assert.True(ReferenceEquals(internedA, internedB));
        }

        [Fact]
        public void Intern_ReturnsOriginalValue()
        {
            var pool = new StringPool();
            string result = pool.Intern("test");
            Assert.Equal("test", result);
        }

        [Fact]
        public void Intern_Null_ReturnsNull()
        {
            var pool = new StringPool();
            string? result = pool.Intern((string)null!);
            Assert.Null(result);
        }

        [Fact]
        public void Intern_Span_ReturnsSameAsStringIntern()
        {
            var pool = new StringPool();
            string str = pool.Intern("hello");
            string fromSpan = pool.Intern("hello".AsSpan());
            Assert.True(ReferenceEquals(str, fromSpan));
        }

        [Fact]
        public void Count_ReflectsUniqueStrings()
        {
            var pool = new StringPool();
            Assert.Equal(0, pool.Count);

            pool.Intern("a");
            Assert.Equal(1, pool.Count);

            pool.Intern("b");
            Assert.Equal(2, pool.Count);

            // Interning same string again should not increase count
            pool.Intern("a");
            Assert.Equal(2, pool.Count);
        }

        [Fact]
        public void Count_DifferentStrings_AllCounted()
        {
            var pool = new StringPool();
            for (int i = 0; i < 100; i++)
            {
                pool.Intern($"str_{i}");
            }
            Assert.Equal(100, pool.Count);
        }

        [Fact]
        public void InitialCapacity_WorksWithSmallValues()
        {
            var pool = new StringPool(4);
            pool.Intern("a");
            pool.Intern("b");
            pool.Intern("c");
            pool.Intern("d");
            pool.Intern("e"); // Exceeds initial capacity
            Assert.Equal(5, pool.Count);
        }

        [Fact]
        public void HtmlNames_IsNotNull()
        {
            Assert.NotNull(StringPool.HtmlNames);
        }

        [Fact]
        public void HtmlNames_ContainsCommonTags()
        {
            var pool = StringPool.HtmlNames;
            // Check that common tags are pre-interned
            string div = pool.Intern("div");
            Assert.Equal("div", div);

            // Interning again should return same reference
            string div2 = pool.Intern("div");
            Assert.True(ReferenceEquals(div, div2));
        }

        [Fact]
        public void HtmlNames_ContainsCommonAttributes()
        {
            var pool = StringPool.HtmlNames;
            string cls = pool.Intern("class");
            Assert.Equal("class", cls);

            string cls2 = pool.Intern("class");
            Assert.True(ReferenceEquals(cls, cls2));
        }

        [Fact]
        public void CssNames_IsNotNull()
        {
            Assert.NotNull(StringPool.CssNames);
        }

        [Fact]
        public void CssNames_ContainsCommonProperties()
        {
            var pool = StringPool.CssNames;
            string display = pool.Intern("display");
            Assert.Equal("display", display);

            string display2 = pool.Intern("display");
            Assert.True(ReferenceEquals(display, display2));
        }

        [Fact]
        public void CssNames_ContainsCommonValues()
        {
            var pool = StringPool.CssNames;
            string none = pool.Intern("none");
            Assert.Equal("none", none);

            string none2 = pool.Intern("none");
            Assert.True(ReferenceEquals(none, none2));
        }

        [Fact]
        public void HtmlNames_HasPreInternedEntries()
        {
            // The pool should have entries from creation
            Assert.True(StringPool.HtmlNames.Count > 0);
        }

        [Fact]
        public void CssNames_HasPreInternedEntries()
        {
            Assert.True(StringPool.CssNames.Count > 0);
        }

        [Fact]
        public void Intern_EmptyString_Works()
        {
            var pool = new StringPool();
            string result = pool.Intern("");
            Assert.Equal("", result);
            Assert.Equal(1, pool.Count);
        }

        [Fact]
        public void Intern_CaseSensitive()
        {
            var pool = new StringPool();
            pool.Intern("Hello");
            pool.Intern("hello");
            Assert.Equal(2, pool.Count);
        }
    }
}
