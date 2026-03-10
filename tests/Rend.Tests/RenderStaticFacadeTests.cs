using System;
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
        private static bool IsNativeLibraryFailure(Exception ex)
        {
            if (ex is DllNotFoundException || ex is TypeInitializationException || ex is BadImageFormatException)
                return true;
            var inner = ex.InnerException;
            while (inner != null)
            {
                if (inner is DllNotFoundException || inner is TypeInitializationException)
                    return true;
                inner = inner.InnerException;
            }
            return false;
        }

        // ── ToPdf TextReader overloads ──

        [Fact]
        public void ToPdf_TextReader_ReturnsValidPdf()
        {
            byte[] result;
            try
            {
                using var reader = new StringReader("<h1>Static TextReader</h1>");
                result = Render.ToPdf(reader);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex)) { return; }

            Assert.NotNull(result);
            Assert.True(result.Length >= 4);
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public void ToPdf_TextReaderToStream_WritesData()
        {
            using var stream = new MemoryStream();
            try
            {
                using var reader = new StringReader("<p>Static reader stream</p>");
                Render.ToPdf(reader, stream);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex)) { return; }

            Assert.True(stream.Length > 0);
        }

        // ── ToImage TextReader overloads ──

        [Fact]
        public void ToImage_TextReader_ReturnsNonEmpty()
        {
            byte[] result;
            try
            {
                using var reader = new StringReader("<p>Static reader image</p>");
                result = Render.ToImage(reader);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex)) { return; }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToImage_TextReaderToStream_WritesData()
        {
            using var stream = new MemoryStream();
            try
            {
                using var reader = new StringReader("<p>Static reader stream image</p>");
                Render.ToImage(reader, stream);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex)) { return; }

            Assert.True(stream.Length > 0);
        }

        // ── Async TextReader overloads ──

        [Fact]
        public async Task ToPdfAsync_TextReader_ReturnsValidPdf()
        {
            byte[] result;
            try
            {
                using var reader = new StringReader("<p>Async static reader</p>");
                result = await Render.ToPdfAsync(reader);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex)) { return; }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task ToPdfAsync_TextReaderToStream_WritesData()
        {
            using var stream = new MemoryStream();
            try
            {
                using var reader = new StringReader("<p>Async static reader stream</p>");
                await Render.ToPdfAsync(reader, stream);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex)) { return; }

            Assert.True(stream.Length > 0);
        }

        [Fact]
        public async Task ToImageAsync_TextReader_ReturnsNonEmpty()
        {
            byte[] result;
            try
            {
                using var reader = new StringReader("<p>Async static reader image</p>");
                result = await Render.ToImageAsync(reader);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex)) { return; }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task ToImageAsync_TextReaderToStream_WritesData()
        {
            using var stream = new MemoryStream();
            try
            {
                using var reader = new StringReader("<p>Async static reader stream image</p>");
                await Render.ToImageAsync(reader, stream);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex)) { return; }

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

            byte[] result;
            try { result = Render.ToPdf("<p>Options test</p>", options); }
            catch (Exception ex) when (IsNativeLibraryFailure(ex)) { return; }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToImage_WithImageOptions_Works()
        {
            var options = new RenderOptions
            {
                Dpi = 150f,
                ImageFormat = "png",
                ImageQuality = 85
            };

            byte[] result;
            try { result = Render.ToImage("<p>DPI test</p>", options); }
            catch (Exception ex) when (IsNativeLibraryFailure(ex)) { return; }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }
    }
}
