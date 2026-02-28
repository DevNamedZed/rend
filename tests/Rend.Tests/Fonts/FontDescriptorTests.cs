using Rend.Css;
using Rend.Fonts;
using Xunit;

namespace Rend.Tests.Fonts
{
    public class FontDescriptorTests
    {
        [Fact]
        public void Constructor_SetsProperties()
        {
            var descriptor = new FontDescriptor("Arial", 700f, CssFontStyle.Italic, 125f);

            Assert.Equal("Arial", descriptor.Family);
            Assert.Equal(700f, descriptor.Weight);
            Assert.Equal(CssFontStyle.Italic, descriptor.Style);
            Assert.Equal(125f, descriptor.Stretch);
        }

        [Fact]
        public void Constructor_DefaultValues()
        {
            var descriptor = new FontDescriptor("Helvetica");

            Assert.Equal("Helvetica", descriptor.Family);
            Assert.Equal(400f, descriptor.Weight);
            Assert.Equal(CssFontStyle.Normal, descriptor.Style);
            Assert.Equal(100f, descriptor.Stretch);
        }

        [Fact]
        public void Constructor_ThrowsOnNullFamily()
        {
            Assert.Throws<System.ArgumentNullException>(() => new FontDescriptor(null!));
        }

        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new FontDescriptor("Arial", 400f, CssFontStyle.Normal, 100f);
            var b = new FontDescriptor("Arial", 400f, CssFontStyle.Normal, 100f);

            Assert.True(a.Equals(b));
            Assert.True(a == b);
        }

        [Fact]
        public void Equals_CaseInsensitiveFamily()
        {
            var a = new FontDescriptor("Arial");
            var b = new FontDescriptor("arial");
            var c = new FontDescriptor("ARIAL");

            Assert.True(a.Equals(b));
            Assert.True(a.Equals(c));
            Assert.True(b.Equals(c));
        }

        [Fact]
        public void Equals_DifferentWeight_ReturnsFalse()
        {
            var a = new FontDescriptor("Arial", 400f);
            var b = new FontDescriptor("Arial", 700f);

            Assert.False(a.Equals(b));
            Assert.True(a != b);
        }

        [Fact]
        public void Equals_DifferentStyle_ReturnsFalse()
        {
            var a = new FontDescriptor("Arial", 400f, CssFontStyle.Normal);
            var b = new FontDescriptor("Arial", 400f, CssFontStyle.Italic);

            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equals_DifferentStretch_ReturnsFalse()
        {
            var a = new FontDescriptor("Arial", 400f, CssFontStyle.Normal, 100f);
            var b = new FontDescriptor("Arial", 400f, CssFontStyle.Normal, 125f);

            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equals_DifferentFamily_ReturnsFalse()
        {
            var a = new FontDescriptor("Arial");
            var b = new FontDescriptor("Helvetica");

            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equals_BoxedComparison()
        {
            var a = new FontDescriptor("Arial", 400f);
            object b = new FontDescriptor("Arial", 400f);

            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_BoxedDifferentType_ReturnsFalse()
        {
            var a = new FontDescriptor("Arial");
            Assert.False(a.Equals("not a descriptor"));
        }

        [Fact]
        public void GetHashCode_SameValues_SameHash()
        {
            var a = new FontDescriptor("Arial", 400f, CssFontStyle.Normal, 100f);
            var b = new FontDescriptor("Arial", 400f, CssFontStyle.Normal, 100f);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void GetHashCode_CaseInsensitiveFamily_SameHash()
        {
            var a = new FontDescriptor("Arial");
            var b = new FontDescriptor("arial");

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void GetHashCode_DifferentValues_DifferentHash()
        {
            var a = new FontDescriptor("Arial", 400f);
            var b = new FontDescriptor("Arial", 700f);

            // Not strictly guaranteed, but very unlikely to collide
            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            var descriptor = new FontDescriptor("Arial", 700f, CssFontStyle.Italic, 100f);
            var s = descriptor.ToString();

            Assert.Contains("Arial", s);
            Assert.Contains("700", s);
            Assert.Contains("Italic", s);
        }

        [Fact]
        public void OperatorEquality_Works()
        {
            var a = new FontDescriptor("Times", 400f);
            var b = new FontDescriptor("Times", 400f);
            var c = new FontDescriptor("Times", 700f);

            Assert.True(a == b);
            Assert.False(a == c);
            Assert.True(a != c);
            Assert.False(a != b);
        }
    }
}
