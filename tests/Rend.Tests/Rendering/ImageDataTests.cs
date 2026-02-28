using Rend.Rendering;
using Xunit;

namespace Rend.Tests.Rendering
{
    public class ImageDataTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            var data = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
            var image = new ImageData(data, 640, 480, "png");

            Assert.Same(data, image.Data);
            Assert.Equal(640, image.Width);
            Assert.Equal(480, image.Height);
            Assert.Equal("png", image.Format);
        }

        [Fact]
        public void Constructor_NullData_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => new ImageData(null!, 100, 100, "png"));
        }

        [Fact]
        public void Constructor_NullFormat_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => new ImageData(new byte[] { 0x00 }, 100, 100, null!));
        }

        [Fact]
        public void Constructor_JpegFormat()
        {
            var image = new ImageData(new byte[] { 0xFF, 0xD8, 0xFF }, 800, 600, "jpeg");

            Assert.Equal("jpeg", image.Format);
            Assert.Equal(800, image.Width);
            Assert.Equal(600, image.Height);
        }

        [Fact]
        public void Constructor_ZeroDimensions()
        {
            var image = new ImageData(new byte[] { 0x00 }, 0, 0, "png");

            Assert.Equal(0, image.Width);
            Assert.Equal(0, image.Height);
        }

        [Fact]
        public void Constructor_EmptyData()
        {
            var image = new ImageData(System.Array.Empty<byte>(), 0, 0, "png");

            Assert.Empty(image.Data);
        }

        [Fact]
        public void Constructor_WebpFormat()
        {
            var image = new ImageData(new byte[] { 0x52, 0x49, 0x46, 0x46 }, 320, 240, "webp");

            Assert.Equal("webp", image.Format);
        }

        [Fact]
        public void Constructor_LargeData()
        {
            var data = new byte[1024 * 1024]; // 1MB
            var image = new ImageData(data, 1920, 1080, "png");

            Assert.Equal(1024 * 1024, image.Data.Length);
            Assert.Equal(1920, image.Width);
            Assert.Equal(1080, image.Height);
        }
    }
}
