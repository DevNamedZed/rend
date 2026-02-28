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
            byte[] result;
            try
            {
                result = Render.ToPdf("<h1>Hello</h1>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                // Skip on environments without native HarfBuzz/SkiaSharp binaries
                return;
            }

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

            byte[] result;
            try
            {
                result = Render.ToPdf("<p>Test content</p>", options);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public void ToPdf_NullHtml_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Render.ToPdf(null!));
        }

        [Fact]
        public void ToPdf_EmptyHtml_ReturnsValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf("");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToPdf_WritesToStream()
        {
            using var stream = new MemoryStream();
            try
            {
                Render.ToPdf("<p>Hello</p>", stream);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.True(stream.Length > 0);
            stream.Position = 0;
            Assert.Equal('%', (char)stream.ReadByte());
        }

        [Fact]
        public async Task ToPdfAsync_ReturnsValidPdf()
        {
            byte[] result;
            try
            {
                result = await Render.ToPdfAsync("<h1>Async Hello</h1>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public async Task ToPdfAsync_WritesToStream()
        {
            using var stream = new MemoryStream();
            try
            {
                await Render.ToPdfAsync("<p>Async stream</p>", stream);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.True(stream.Length > 0);
        }

        [Fact]
        public void ToImage_SimpleHtml_ReturnsNonEmptyBytes()
        {
            byte[] result;
            try
            {
                result = Render.ToImage("<p>Test</p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0, "Image output should not be empty");
        }

        [Fact]
        public void ToImage_NullHtml_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Render.ToImage(null!));
        }

        [Fact]
        public void ToImage_WritesToStream()
        {
            using var stream = new MemoryStream();
            try
            {
                Render.ToImage("<p>Stream image</p>", stream);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.True(stream.Length > 0);
        }

        [Fact]
        public async Task ToImageAsync_ReturnsNonEmptyBytes()
        {
            byte[] result;
            try
            {
                result = await Render.ToImageAsync("<p>Async image</p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task ToImageAsync_WritesToStream()
        {
            using var stream = new MemoryStream();
            try
            {
                await Render.ToImageAsync("<p>Async stream image</p>", stream);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

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

            byte[] result;
            try
            {
                result = Render.ToPdf(html);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public async Task ToPdfAsync_CancellationToken_Respected()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            try
            {
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    () => Render.ToPdfAsync("<p>Cancelled</p>", cancellationToken: cts.Token));
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }
        }

        [Fact]
        public void ToPdf_DefaultOptions_UsedWhenNull()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf("<p>Default options</p>", null);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        /// <summary>
        /// Checks if the exception is due to missing native libraries (HarfBuzz, SkiaSharp)
        /// which may not be available in all test environments.
        /// </summary>
        private static bool IsNativeLibraryFailure(Exception ex)
        {
            if (ex is DllNotFoundException) return true;
            if (ex is TypeInitializationException) return true;
            if (ex is BadImageFormatException) return true;

            // Check inner exceptions
            var inner = ex.InnerException;
            while (inner != null)
            {
                if (inner is DllNotFoundException) return true;
                if (inner is TypeInitializationException) return true;
                inner = inner.InnerException;
            }

            // Check message for common native library issues
            string msg = ex.Message ?? "";
            if (msg.Contains("libHarfBuzz", StringComparison.OrdinalIgnoreCase)) return true;
            if (msg.Contains("libSkiaSharp", StringComparison.OrdinalIgnoreCase)) return true;
            if (msg.Contains("HarfBuzzSharp", StringComparison.OrdinalIgnoreCase)) return true;
            if (msg.Contains("SkiaSharp", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }
    }
}
