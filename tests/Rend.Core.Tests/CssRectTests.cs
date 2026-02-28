using Rend.Core.Values;
using Xunit;

namespace Rend.Core.Tests
{
    public class CssRectTests
    {
        [Fact]
        public void Constructor_FourValues_SetsAll()
        {
            var r = new CssRect<CssLength>(
                CssLength.Px(1f), CssLength.Px(2f),
                CssLength.Px(3f), CssLength.Px(4f));
            Assert.Equal(CssLength.Px(1f), r.Top);
            Assert.Equal(CssLength.Px(2f), r.Right);
            Assert.Equal(CssLength.Px(3f), r.Bottom);
            Assert.Equal(CssLength.Px(4f), r.Left);
        }

        [Fact]
        public void Constructor_OneValue_SetsAllSame()
        {
            var r = new CssRect<CssLength>(CssLength.Px(10f));
            Assert.Equal(CssLength.Px(10f), r.Top);
            Assert.Equal(CssLength.Px(10f), r.Right);
            Assert.Equal(CssLength.Px(10f), r.Bottom);
            Assert.Equal(CssLength.Px(10f), r.Left);
        }

        [Fact]
        public void Constructor_TwoValues_SetsTopBottomAndLeftRight()
        {
            var r = new CssRect<CssLength>(CssLength.Px(10f), CssLength.Px(20f));
            Assert.Equal(CssLength.Px(10f), r.Top);
            Assert.Equal(CssLength.Px(20f), r.Right);
            Assert.Equal(CssLength.Px(10f), r.Bottom);
            Assert.Equal(CssLength.Px(20f), r.Left);
        }

        [Fact]
        public void Constructor_ThreeValues_SetsTopLeftRightBottom()
        {
            var r = new CssRect<CssLength>(CssLength.Px(10f), CssLength.Px(20f), CssLength.Px(30f));
            Assert.Equal(CssLength.Px(10f), r.Top);
            Assert.Equal(CssLength.Px(20f), r.Right);
            Assert.Equal(CssLength.Px(30f), r.Bottom);
            Assert.Equal(CssLength.Px(20f), r.Left);
        }

        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new CssRect<CssLength>(CssLength.Px(1f), CssLength.Px(2f), CssLength.Px(3f), CssLength.Px(4f));
            var b = new CssRect<CssLength>(CssLength.Px(1f), CssLength.Px(2f), CssLength.Px(3f), CssLength.Px(4f));
            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var a = new CssRect<CssLength>(CssLength.Px(1f), CssLength.Px(2f), CssLength.Px(3f), CssLength.Px(4f));
            var b = new CssRect<CssLength>(CssLength.Px(1f), CssLength.Px(2f), CssLength.Px(3f), CssLength.Px(99f));
            Assert.False(a.Equals(b));
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equals_BoxedObject_Works()
        {
            var a = new CssRect<CssLength>(CssLength.Px(10f));
            object b = new CssRect<CssLength>(CssLength.Px(10f));
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_NonCssRectObject_ReturnsFalse()
        {
            var a = new CssRect<CssLength>(CssLength.Px(10f));
            Assert.False(a.Equals("not a rect"));
            Assert.False(a.Equals(null));
        }

        [Fact]
        public void GetHashCode_EqualValues_SameHash()
        {
            var a = new CssRect<CssLength>(CssLength.Px(1f), CssLength.Px(2f), CssLength.Px(3f), CssLength.Px(4f));
            var b = new CssRect<CssLength>(CssLength.Px(1f), CssLength.Px(2f), CssLength.Px(3f), CssLength.Px(4f));
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ToString_FormatsCorrectly()
        {
            var r = new CssRect<CssLength>(CssLength.Px(1f), CssLength.Px(2f), CssLength.Px(3f), CssLength.Px(4f));
            Assert.Equal("[1px, 2px, 3px, 4px]", r.ToString());
        }
    }

    public class CssEdgesTests
    {
        [Fact]
        public void Constructor_FourValues_SetsAll()
        {
            var e = new CssEdges(10f, 20f, 30f, 40f);
            Assert.Equal(10f, e.Top);
            Assert.Equal(20f, e.Right);
            Assert.Equal(30f, e.Bottom);
            Assert.Equal(40f, e.Left);
        }

        [Fact]
        public void Constructor_OneValue_SetsAllSame()
        {
            var e = new CssEdges(15f);
            Assert.Equal(15f, e.Top);
            Assert.Equal(15f, e.Right);
            Assert.Equal(15f, e.Bottom);
            Assert.Equal(15f, e.Left);
        }

        [Fact]
        public void Constructor_TwoValues_SetsTopBottomAndLeftRight()
        {
            var e = new CssEdges(10f, 20f);
            Assert.Equal(10f, e.Top);
            Assert.Equal(20f, e.Right);
            Assert.Equal(10f, e.Bottom);
            Assert.Equal(20f, e.Left);
        }

        [Fact]
        public void Zero_HasAllZeroes()
        {
            Assert.Equal(0f, CssEdges.Zero.Top);
            Assert.Equal(0f, CssEdges.Zero.Right);
            Assert.Equal(0f, CssEdges.Zero.Bottom);
            Assert.Equal(0f, CssEdges.Zero.Left);
        }

        [Fact]
        public void Horizontal_ReturnsLeftPlusRight()
        {
            var e = new CssEdges(10f, 20f, 30f, 40f);
            Assert.Equal(60f, e.Horizontal); // 40 + 20
        }

        [Fact]
        public void Vertical_ReturnsTopPlusBottom()
        {
            var e = new CssEdges(10f, 20f, 30f, 40f);
            Assert.Equal(40f, e.Vertical); // 10 + 30
        }

        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new CssEdges(1f, 2f, 3f, 4f);
            var b = new CssEdges(1f, 2f, 3f, 4f);
            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var a = new CssEdges(1f, 2f, 3f, 4f);
            Assert.False(a.Equals(new CssEdges(99f, 2f, 3f, 4f)));
            Assert.False(a.Equals(new CssEdges(1f, 99f, 3f, 4f)));
            Assert.False(a.Equals(new CssEdges(1f, 2f, 99f, 4f)));
            Assert.False(a.Equals(new CssEdges(1f, 2f, 3f, 99f)));
        }

        [Fact]
        public void Equals_BoxedObject_Works()
        {
            var a = new CssEdges(1f, 2f, 3f, 4f);
            object b = new CssEdges(1f, 2f, 3f, 4f);
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_NonCssEdgesObject_ReturnsFalse()
        {
            var a = new CssEdges(1f);
            Assert.False(a.Equals("not edges"));
            Assert.False(a.Equals(null));
        }

        [Fact]
        public void GetHashCode_EqualValues_SameHash()
        {
            var a = new CssEdges(1f, 2f, 3f, 4f);
            var b = new CssEdges(1f, 2f, 3f, 4f);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }
    }
}
