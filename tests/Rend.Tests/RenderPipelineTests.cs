using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rend.Tests
{
    public class RenderPipelineTests
    {
        [Fact]
        public void ToPdf_SimpleHtml_ReturnsNonEmptyPdfBytes()
        {
            var result = Render.ToPdf("<h1>Hello</h1>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0, "PDF output should not be empty");

            // PDF files start with %PDF
            Assert.True(result.Length >= 4);
            Assert.Equal((byte)'%', result[0]);
            Assert.Equal((byte)'P', result[1]);
            Assert.Equal((byte)'D', result[2]);
            Assert.Equal((byte)'F', result[3]);
        }

        [Fact]
        public void ToPdf_WithOptions_ReturnsValidPdf()
        {
            var options = new RenderOptions
            {
                Title = "Test Document",
                Author = "Test Author",
                MarginTop = 36f,
                MarginBottom = 36f,
            };

            var result = Render.ToPdf("<p>Test content</p>", options);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public void ToPdf_NullHtml_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Render.ToPdf((string)null!));
        }

        [Fact]
        public void ToPdf_EmptyHtml_ReturnsValidPdf()
        {
            var result = Render.ToPdf("");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToPdf_WritesToStream()
        {
            using var stream = new MemoryStream();
            Render.ToPdf("<p>Hello</p>", stream);

            Assert.True(stream.Length > 0);
            stream.Position = 0;
            Assert.Equal('%', (char)stream.ReadByte());
        }

        [Fact]
        public async Task ToPdfAsync_ReturnsValidPdf()
        {
            var result = await Render.ToPdfAsync("<h1>Async Hello</h1>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public async Task ToPdfAsync_WritesToStream()
        {
            using var stream = new MemoryStream();
            await Render.ToPdfAsync("<p>Async stream</p>", stream);

            Assert.True(stream.Length > 0);
        }

        [Fact]
        public void ToImage_SimpleHtml_ReturnsNonEmptyBytes()
        {
            var result = Render.ToImage("<p>Test</p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0, "Image output should not be empty");
        }

        [Fact]
        public void ToImage_NullHtml_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Render.ToImage((string)null!));
        }

        [Fact]
        public void ToImage_WritesToStream()
        {
            using var stream = new MemoryStream();
            Render.ToImage("<p>Stream image</p>", stream);

            Assert.True(stream.Length > 0);
        }

        [Fact]
        public async Task ToImageAsync_ReturnsNonEmptyBytes()
        {
            var result = await Render.ToImageAsync("<p>Async image</p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task ToImageAsync_WritesToStream()
        {
            using var stream = new MemoryStream();
            await Render.ToImageAsync("<p>Async stream image</p>", stream);

            Assert.True(stream.Length > 0);
        }

        [Fact]
        public void ToPdf_ComplexHtml_ProducesPdf()
        {
            string html = @"
                <html>
                <head>
                    <style>
                        body { font-family: sans-serif; color: #333; }
                        h1 { color: navy; border-bottom: 2px solid navy; }
                        .highlight { background-color: yellow; }
                    </style>
                </head>
                <body>
                    <h1>Document Title</h1>
                    <p>This is a <span class=""highlight"">highlighted</span> paragraph.</p>
                    <ul>
                        <li>Item 1</li>
                        <li>Item 2</li>
                        <li>Item 3</li>
                    </ul>
                </body>
                </html>";

            var result = Render.ToPdf(html);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public async Task ToPdfAsync_CancellationToken_Respected()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => Render.ToPdfAsync("<p>Cancelled</p>", cancellationToken: cts.Token));
        }

        [Fact]
        public void ToPdf_DefaultOptions_UsedWhenNull()
        {
            var result = Render.ToPdf("<p>Default options</p>", null);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

    }
}
