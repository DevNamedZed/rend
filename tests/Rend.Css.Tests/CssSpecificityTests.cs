using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rend.Css.Tests
{
    public class CssSpecificityTests
    {
        #region Construction

        [Fact]
        public void Constructor_SetsComponents()
        {
            var spec = new CssSpecificity(1, 2, 3);

            Assert.Equal(1, spec.A);
            Assert.Equal(2, spec.B);
            Assert.Equal(3, spec.C);
            Assert.False(spec.Inline);
        }

        [Fact]
        public void Constructor_InlineFlag_SetsCorrectly()
        {
            var spec = new CssSpecificity(0, 0, 0, inline_: true);
            Assert.True(spec.Inline);
        }

        [Fact]
        public void Zero_IsAllZeros()
        {
            var zero = CssSpecificity.Zero;
            Assert.Equal(0, zero.A);
            Assert.Equal(0, zero.B);
            Assert.Equal(0, zero.C);
            Assert.False(zero.Inline);
        }

        [Fact]
        public void InlineStyle_HasInlineFlag()
        {
            var inline = CssSpecificity.InlineStyle;
            Assert.True(inline.Inline);
        }

        #endregion

        #region Comparison

        [Fact]
        public void CompareTo_HigherA_Wins()
        {
            var low = new CssSpecificity(0, 5, 5);
            var high = new CssSpecificity(1, 0, 0);

            Assert.True(high.CompareTo(low) > 0);
            Assert.True(low.CompareTo(high) < 0);
        }

        [Fact]
        public void CompareTo_SameA_HigherB_Wins()
        {
            var low = new CssSpecificity(0, 0, 10);
            var high = new CssSpecificity(0, 1, 0);

            Assert.True(high.CompareTo(low) > 0);
            Assert.True(low.CompareTo(high) < 0);
        }

        [Fact]
        public void CompareTo_SameAB_HigherC_Wins()
        {
            var low = new CssSpecificity(0, 0, 1);
            var high = new CssSpecificity(0, 0, 2);

            Assert.True(high.CompareTo(low) > 0);
            Assert.True(low.CompareTo(high) < 0);
        }

        [Fact]
        public void CompareTo_Equal_ReturnsZero()
        {
            var a = new CssSpecificity(1, 2, 3);
            var b = new CssSpecificity(1, 2, 3);

            Assert.Equal(0, a.CompareTo(b));
        }

        [Fact]
        public void CompareTo_InlineBeatsNonInline()
        {
            var nonInline = new CssSpecificity(100, 100, 100);
            var inline = new CssSpecificity(0, 0, 0, inline_: true);

            Assert.True(inline.CompareTo(nonInline) > 0);
            Assert.True(nonInline.CompareTo(inline) < 0);
        }

        [Fact]
        public void CompareTo_BothInline_ComparesComponents()
        {
            var a = new CssSpecificity(1, 0, 0, inline_: true);
            var b = new CssSpecificity(0, 0, 0, inline_: true);

            Assert.True(a.CompareTo(b) > 0);
        }

        #endregion

        #region Ordering

        [Fact]
        public void Ordering_SortsBySpecificity()
        {
            var specs = new List<CssSpecificity>
            {
                new CssSpecificity(0, 1, 0),  // .class
                new CssSpecificity(1, 0, 0),  // #id
                new CssSpecificity(0, 0, 1),  // element
                new CssSpecificity(0, 0, 0),  // *
                new CssSpecificity(0, 2, 0),  // .class.class
            };

            var sorted = specs.OrderBy(s => s).ToList();

            // Lowest to highest
            Assert.Equal(new CssSpecificity(0, 0, 0), sorted[0]);
            Assert.Equal(new CssSpecificity(0, 0, 1), sorted[1]);
            Assert.Equal(new CssSpecificity(0, 1, 0), sorted[2]);
            Assert.Equal(new CssSpecificity(0, 2, 0), sorted[3]);
            Assert.Equal(new CssSpecificity(1, 0, 0), sorted[4]);
        }

        #endregion

        #region Equality

        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new CssSpecificity(1, 2, 3);
            var b = new CssSpecificity(1, 2, 3);

            Assert.True(a.Equals(b));
            Assert.True(a == b);
        }

        [Fact]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var a = new CssSpecificity(1, 2, 3);
            var b = new CssSpecificity(1, 2, 4);

            Assert.False(a.Equals(b));
            Assert.True(a != b);
        }

        [Fact]
        public void Equals_DifferentInline_ReturnsFalse()
        {
            var a = new CssSpecificity(0, 0, 0, inline_: true);
            var b = new CssSpecificity(0, 0, 0);

            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equals_BoxedObject_Works()
        {
            var a = new CssSpecificity(1, 2, 3);
            object b = new CssSpecificity(1, 2, 3);

            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_WrongType_ReturnsFalse()
        {
            var a = new CssSpecificity(1, 2, 3);
            Assert.False(a.Equals("not a specificity"));
        }

        [Fact]
        public void GetHashCode_EqualObjects_SameHash()
        {
            var a = new CssSpecificity(1, 2, 3);
            var b = new CssSpecificity(1, 2, 3);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        #endregion

        #region Operators

        [Fact]
        public void LessThan_Works()
        {
            var low = new CssSpecificity(0, 0, 1);
            var high = new CssSpecificity(0, 1, 0);

            Assert.True(low < high);
            Assert.False(high < low);
        }

        [Fact]
        public void GreaterThan_Works()
        {
            var low = new CssSpecificity(0, 0, 1);
            var high = new CssSpecificity(0, 1, 0);

            Assert.True(high > low);
            Assert.False(low > high);
        }

        [Fact]
        public void LessThanOrEqual_Works()
        {
            var a = new CssSpecificity(0, 1, 0);
            var b = new CssSpecificity(0, 1, 0);
            var c = new CssSpecificity(0, 2, 0);

            Assert.True(a <= b);
            Assert.True(a <= c);
            Assert.False(c <= a);
        }

        [Fact]
        public void GreaterThanOrEqual_Works()
        {
            var a = new CssSpecificity(0, 1, 0);
            var b = new CssSpecificity(0, 1, 0);
            var c = new CssSpecificity(0, 2, 0);

            Assert.True(a >= b);
            Assert.True(c >= a);
            Assert.False(a >= c);
        }

        #endregion

        #region ToString

        [Fact]
        public void ToString_Normal_FormatsCorrectly()
        {
            var spec = new CssSpecificity(1, 2, 3);
            Assert.Equal("(1,2,3)", spec.ToString());
        }

        [Fact]
        public void ToString_Inline_IncludesInlinePrefix()
        {
            var spec = new CssSpecificity(0, 0, 0, inline_: true);
            Assert.Contains("inline", spec.ToString());
        }

        [Fact]
        public void ToString_Zero_FormatsAsZeros()
        {
            var spec = CssSpecificity.Zero;
            Assert.Equal("(0,0,0)", spec.ToString());
        }

        #endregion
    }
}
