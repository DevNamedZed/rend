using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Rend.Tests
{
    /// <summary>
    /// Tests for the <see cref="Render"/> static facade — specifically TextReader overloads
    /// and edge cases not covered by RenderPipelineTests.
    /// </summary>
    public class RenderStaticFacadeTests
    {
        // ── ToPdf TextReader overloads ──

        [Fact]
        public void ToPdf_TextReader_ReturnsValidPdf()
        {
            using var reader = new StringReader("<h1>Static TextReader</h1>");
            var result = Render.ToPdf(reader);

            Assert.NotNull(result);
            Assert.True(result.Length >= 4);
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public void ToPdf_TextReaderToStream_WritesData()
        {
            using var stream = new MemoryStream();
            using var reader = new StringReader("<p>Static reader stream</p>");
            Render.ToPdf(reader, stream);

            Assert.True(stream.Length > 0);
        }

        // ── ToImage TextReader overloads ──

        [Fact]
        public void ToImage_TextReader_ReturnsNonEmpty()
        {
            using var reader = new StringReader("<p>Static reader image</p>");
            var result = Render.ToImage(reader);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToImage_TextReaderToStream_WritesData()
        {
            using var stream = new MemoryStream();
            using var reader = new StringReader("<p>Static reader stream image</p>");
            Render.ToImage(reader, stream);

            Assert.True(stream.Length > 0);
        }

        // ── Async TextReader overloads ──

        [Fact]
        public async Task ToPdfAsync_TextReader_ReturnsValidPdf()
        {
            using var reader = new StringReader("<p>Async static reader</p>");
            var result = await Render.ToPdfAsync(reader);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task ToPdfAsync_TextReaderToStream_WritesData()
        {
            using var stream = new MemoryStream();
            using var reader = new StringReader("<p>Async static reader stream</p>");
            await Render.ToPdfAsync(reader, stream);

            Assert.True(stream.Length > 0);
        }

        [Fact]
        public async Task ToImageAsync_TextReader_ReturnsNonEmpty()
        {
            using var reader = new StringReader("<p>Async static reader image</p>");
            var result = await Render.ToImageAsync(reader);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task ToImageAsync_TextReaderToStream_WritesData()
        {
            using var stream = new MemoryStream();
            using var reader = new StringReader("<p>Async static reader stream image</p>");
            await Render.ToImageAsync(reader, stream);

            Assert.True(stream.Length > 0);
        }

        // ── RenderOptions configuration ──

        [Fact]
        public void ToPdf_WithAllOptions_Works()
        {
            var options = new RenderOptions
            {
                Title = "Full Options Test",
                Author = "Test Author",
                MarginTop = 36f,
                MarginRight = 36f,
                MarginBottom = 36f,
                MarginLeft = 36f,
                GenerateBookmarks = false,
                GenerateLinks = false,
                DefaultFontSize = 14f,
                MediaType = "print"
            };

            var result = Render.ToPdf("<p>Options test</p>", options);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToImage_WithImageOptions_Works()
        {
            var options = new RenderOptions
            {
                Dpi = 150f,
                ImageFormat = ImageOutputFormat.Png,
                ImageQuality = 85
            };

            var result = Render.ToImage("<p>DPI test</p>", options);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }
    }
}
