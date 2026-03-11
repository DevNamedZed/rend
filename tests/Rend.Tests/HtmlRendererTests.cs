using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rend.Tests
{
    /// <summary>
    /// Tests for <see cref="HtmlRenderer"/> (instance-based IRenderer implementation).
    /// Covers all overloads: string, TextReader, Stream, and async variants.
    /// </summary>
    public class HtmlRendererTests
    {
        private readonly HtmlRenderer _renderer = new HtmlRenderer();

        // ── IRenderer interface contract ──

        [Fact]
        public void ImplementsIRenderer()
        {
            Assert.IsAssignableFrom<IRenderer>(_renderer);
        }

        // ── ToPdf string overloads ──

        [Fact]
        public void ToPdf_String_ReturnsValidPdf()
        {
            var result = _renderer.ToPdf("<h1>Hello</h1>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            AssertPdfHeader(result);
        }

        [Fact]
        public void ToPdf_String_NullThrows()
        {
            Assert.Throws<ArgumentNullException>(() => _renderer.ToPdf((string)null!));
        }

        [Fact]
        public void ToPdf_String_EmptyHtml_ReturnsValidPdf()
        {
            var result = _renderer.ToPdf("");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToPdf_String_WithOptions()
        {
            var options = new RenderOptions { Title = "Test", Author = "Author" };
            var result = _renderer.ToPdf("<p>content</p>", options);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        // ── ToPdf stream overloads ──

        [Fact]
        public void ToPdf_StringToStream_WritesData()
        {
            using var stream = new MemoryStream();
            _renderer.ToPdf("<p>Hello</p>", stream);

            Assert.True(stream.Length > 0);
            stream.Position = 0;
            AssertPdfHeader(stream);
        }

        [Fact]
        public void ToPdf_StringToStream_WithOptions()
        {
            var options = new RenderOptions { MarginTop = 36f };
            using var stream = new MemoryStream();
            _renderer.ToPdf("<p>Hello</p>", stream, options);

            Assert.True(stream.Length > 0);
        }

        // ── ToPdf TextReader overloads ──

        [Fact]
        public void ToPdf_TextReader_ReturnsValidPdf()
        {
            using var reader = new StringReader("<h1>TextReader test</h1>");
            var result = _renderer.ToPdf(reader);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            AssertPdfHeader(result);
        }

        [Fact]
        public void ToPdf_TextReaderToStream_WritesData()
        {
            using var stream = new MemoryStream();
            using var reader = new StringReader("<p>TextReader stream</p>");
            _renderer.ToPdf(reader, stream);

            Assert.True(stream.Length > 0);
        }

        // ── ToImage string overloads ──

        [Fact]
        public void ToImage_String_ReturnsNonEmpty()
        {
            var result = _renderer.ToImage("<p>Image test</p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToImage_String_NullThrows()
        {
            Assert.Throws<ArgumentNullException>(() => _renderer.ToImage((string)null!));
        }

        [Fact]
        public void ToImage_StringToStream_WritesData()
        {
            using var stream = new MemoryStream();
            _renderer.ToImage("<p>Stream image</p>", stream);

            Assert.True(stream.Length > 0);
        }

        // ── ToImage TextReader overloads ──

        [Fact]
        public void ToImage_TextReader_ReturnsNonEmpty()
        {
            using var reader = new StringReader("<p>TextReader image</p>");
            var result = _renderer.ToImage(reader);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToImage_TextReaderToStream_WritesData()
        {
            using var stream = new MemoryStream();
            using var reader = new StringReader("<p>TextReader stream image</p>");
            _renderer.ToImage(reader, stream);

            Assert.True(stream.Length > 0);
        }

        // ── Async ToPdf ──

        [Fact]
        public async Task ToPdfAsync_String_ReturnsValidPdf()
        {
            var result = await _renderer.ToPdfAsync("<h1>Async</h1>");

            Assert.NotNull(result);
            AssertPdfHeader(result);
        }

        [Fact]
        public async Task ToPdfAsync_StringToStream_WritesData()
        {
            using var stream = new MemoryStream();
            await _renderer.ToPdfAsync("<p>Async stream</p>", stream);

            Assert.True(stream.Length > 0);
        }

        [Fact]
        public async Task ToPdfAsync_TextReader_ReturnsValidPdf()
        {
            using var reader = new StringReader("<p>Async reader</p>");
            var result = await _renderer.ToPdfAsync(reader);

            Assert.NotNull(result);
            AssertPdfHeader(result);
        }

        [Fact]
        public async Task ToPdfAsync_TextReaderToStream_WritesData()
        {
            using var stream = new MemoryStream();
            using var reader = new StringReader("<p>Async reader stream</p>");
            await _renderer.ToPdfAsync(reader, stream);

            Assert.True(stream.Length > 0);
        }

        [Fact]
        public async Task ToPdfAsync_CancellationToken_Respected()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => _renderer.ToPdfAsync("<p>Cancelled</p>", cancellationToken: cts.Token));
        }

        // ── Async ToImage ──

        [Fact]
        public async Task ToImageAsync_String_ReturnsNonEmpty()
        {
            var result = await _renderer.ToImageAsync("<p>Async image</p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task ToImageAsync_StringToStream_WritesData()
        {
            using var stream = new MemoryStream();
            await _renderer.ToImageAsync("<p>Async stream image</p>", stream);

            Assert.True(stream.Length > 0);
        }

        [Fact]
        public async Task ToImageAsync_TextReader_ReturnsNonEmpty()
        {
            using var reader = new StringReader("<p>Async reader image</p>");
            var result = await _renderer.ToImageAsync(reader);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task ToImageAsync_TextReaderToStream_WritesData()
        {
            using var stream = new MemoryStream();
            using var reader = new StringReader("<p>Async reader stream image</p>");
            await _renderer.ToImageAsync(reader, stream);

            Assert.True(stream.Length > 0);
        }

        [Fact]
        public async Task ToImageAsync_CancellationToken_Respected()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => _renderer.ToImageAsync("<p>Cancelled</p>", cancellationToken: cts.Token));
        }

        // ── Consistency: string vs TextReader produce same output ──

        [Fact]
        public void ToPdf_StringAndTextReader_ProduceSameOutput()
        {
            const string html = "<p>Consistency test</p>";
            var fromString = _renderer.ToPdf(html);
            using var reader = new StringReader(html);
            var fromReader = _renderer.ToPdf(reader);

            Assert.Equal(fromString.Length, fromReader.Length);
        }

        [Fact]
        public void ToImage_StringAndTextReader_ProduceSameOutput()
        {
            const string html = "<p>Consistency test</p>";
            var fromString = _renderer.ToImage(html);
            using var reader = new StringReader(html);
            var fromReader = _renderer.ToImage(reader);

            Assert.Equal(fromString, fromReader);
        }

        // ── Helpers ──

        private static void AssertPdfHeader(byte[] data)
        {
            Assert.True(data.Length >= 4);
            Assert.Equal((byte)'%', data[0]);
            Assert.Equal((byte)'P', data[1]);
            Assert.Equal((byte)'D', data[2]);
            Assert.Equal((byte)'F', data[3]);
        }

        private static void AssertPdfHeader(Stream stream)
        {
            var buf = new byte[4];
            int read = stream.Read(buf, 0, 4);
            Assert.Equal(4, read);
            Assert.Equal((byte)'%', buf[0]);
            Assert.Equal((byte)'P', buf[1]);
            Assert.Equal((byte)'D', buf[2]);
            Assert.Equal((byte)'F', buf[3]);
        }
    }
}
