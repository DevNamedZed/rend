using Xunit;

namespace Rend.Tests
{
    public class RenderResultTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            var data = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
            var result = new RenderResult(data, 3, "pdf");

            Assert.Same(data, result.Data);
            Assert.Equal(3, result.PageCount);
            Assert.Equal("pdf", result.Format);
        }

        [Fact]
        public void Constructor_SinglePage()
        {
            var data = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
            var result = new RenderResult(data, 1, "image");

            Assert.Equal(1, result.PageCount);
            Assert.Equal("image", result.Format);
        }

        [Fact]
        public void Constructor_EmptyData()
        {
            var result = new RenderResult(System.Array.Empty<byte>(), 0, "pdf");

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PageCount);
        }

        [Fact]
        public void Constructor_LargePageCount()
        {
            var result = new RenderResult(new byte[] { 0x00 }, 100, "pdf");
            Assert.Equal(100, result.PageCount);
        }

        [Fact]
        public void Data_PreservesContent()
        {
            var original = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var result = new RenderResult(original, 1, "pdf");

            Assert.Equal(10, result.Data.Length);
            for (int i = 0; i < original.Length; i++)
            {
                Assert.Equal(original[i], result.Data[i]);
            }
        }

        [Fact]
        public void Format_ImageFormat()
        {
            var result = new RenderResult(new byte[] { 0x89 }, 1, "image");
            Assert.Equal("image", result.Format);
        }

        [Fact]
        public void Format_UnknownFormat()
        {
            var result = new RenderResult(new byte[] { 0x00 }, 1, "unknown");
            Assert.Equal("unknown", result.Format);
        }
    }
}
