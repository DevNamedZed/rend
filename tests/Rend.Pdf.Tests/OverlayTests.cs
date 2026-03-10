using System;
using System.Collections.Generic;
using System.IO;
using Rend.Core.Values;
using Xunit;

namespace Rend.Pdf.Tests
{
    /// <summary>
    /// Tests for <see cref="PdfOverlay"/>, <see cref="PdfOverlays"/> (static),
    /// and <see cref="IPdfOverlay"/> argument validation.
    /// </summary>
    public class OverlayTests
    {
        private readonly IPdfOverlay _overlay = new PdfOverlay();

        /// <summary>Minimal valid PDF for testing.</summary>
        private static byte[] MinimalPdf()
        {
            // A minimal valid PDF that Adobe/any parser accepts
            const string pdf =
                "%PDF-1.0\n" +
                "1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n" +
                "2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n" +
                "3 0 obj<</Type/Page/MediaBox[0 0 612 792]/Parent 2 0 R>>endobj\n" +
                "xref\n0 4\n" +
                "0000000000 65535 f \n" +
                "0000000009 00000 n \n" +
                "0000000058 00000 n \n" +
                "0000000115 00000 n \n" +
                "trailer<</Size 4/Root 1 0 R>>\nstartxref\n191\n%%EOF\n";
            return System.Text.Encoding.ASCII.GetBytes(pdf);
        }

        // ── Argument validation ──

        [Fact]
        public void Apply_ByteArray_NullPdf_Throws()
        {
            Assert.ThrowsAny<ArgumentException>(() =>
                _overlay.Apply(null!, new List<PdfOverlayElement>()));
        }

        [Fact]
        public void Apply_ByteArray_EmptyPdf_Throws()
        {
            Assert.ThrowsAny<ArgumentException>(() =>
                _overlay.Apply(Array.Empty<byte>(), new List<PdfOverlayElement>()));
        }

        [Fact]
        public void Apply_ByteArray_NullElements_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _overlay.Apply(MinimalPdf(), null!));
        }

        [Fact]
        public void Apply_Stream_NullInput_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _overlay.Apply(null!, new MemoryStream(), new List<PdfOverlayElement>()));
        }

        [Fact]
        public void Apply_Stream_NullOutput_Throws()
        {
            using var input = new MemoryStream(MinimalPdf());
            Assert.Throws<ArgumentNullException>(() =>
                _overlay.Apply(input, null!, new List<PdfOverlayElement>()));
        }

        [Fact]
        public void Apply_Stream_NullElements_Throws()
        {
            using var input = new MemoryStream(MinimalPdf());
            using var output = new MemoryStream();
            Assert.Throws<ArgumentNullException>(() =>
                _overlay.Apply(input, output, null!));
        }

        // ── Functional tests ──

        [Fact]
        public void Apply_EmptyElements_ReturnsPdf()
        {
            var pdf = MinimalPdf();
            var result = _overlay.Apply(pdf, new List<PdfOverlayElement>());

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public void Apply_TextOverlay_ReturnsPdf()
        {
            var pdf = MinimalPdf();
            var elements = new List<PdfOverlayElement>
            {
                new TextOverlay
                {
                    Page = 1,
                    X = 100,
                    Y = 100,
                    Text = "Hello World",
                    FontSize = 14,
                    FontFamily = "Helvetica"
                }
            };

            var result = _overlay.Apply(pdf, elements);

            Assert.NotNull(result);
            Assert.True(result.Length > pdf.Length, "Overlaid PDF should be larger");
        }

        [Fact]
        public void Apply_StreamOverload_WritesOutput()
        {
            using var input = new MemoryStream(MinimalPdf());
            using var output = new MemoryStream();
            var elements = new List<PdfOverlayElement>
            {
                new TextOverlay { Page = 1, X = 50, Y = 50, Text = "Stream test" }
            };

            _overlay.Apply(input, output, elements);

            Assert.True(output.Length > 0);
        }

        [Fact]
        public void Apply_TextOverlay_BoldItalic()
        {
            var pdf = MinimalPdf();
            var elements = new List<PdfOverlayElement>
            {
                new TextOverlay
                {
                    Page = 1, X = 10, Y = 10,
                    Text = "Bold Italic",
                    Bold = true, Italic = true,
                    FontFamily = "Helvetica"
                }
            };

            var result = _overlay.Apply(pdf, elements);
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Apply_TextOverlay_CourierFont()
        {
            var pdf = MinimalPdf();
            var elements = new List<PdfOverlayElement>
            {
                new TextOverlay
                {
                    Page = 1, X = 10, Y = 10,
                    Text = "Courier text",
                    FontFamily = "Courier"
                }
            };

            var result = _overlay.Apply(pdf, elements);
            Assert.NotNull(result);
        }

        [Fact]
        public void Apply_TextOverlay_TimesFont()
        {
            var pdf = MinimalPdf();
            var elements = new List<PdfOverlayElement>
            {
                new TextOverlay
                {
                    Page = 1, X = 10, Y = 10,
                    Text = "Times text",
                    FontFamily = "Times"
                }
            };

            var result = _overlay.Apply(pdf, elements);
            Assert.NotNull(result);
        }

        [Fact]
        public void Apply_TextOverlay_WithColor()
        {
            var pdf = MinimalPdf();
            var elements = new List<PdfOverlayElement>
            {
                new TextOverlay
                {
                    Page = 1, X = 10, Y = 10,
                    Text = "Red text",
                    Color = CssColor.FromRgba(255, 0, 0)
                }
            };

            var result = _overlay.Apply(pdf, elements);
            Assert.NotNull(result);
        }

        // ── Static facade (PdfOverlays) ──

        [Fact]
        public void PdfOverlays_Apply_ByteArray_Works()
        {
            var pdf = MinimalPdf();
            var result = PdfOverlays.Apply(pdf, new List<PdfOverlayElement>());

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void PdfOverlays_Apply_Stream_Works()
        {
            using var input = new MemoryStream(MinimalPdf());
            using var output = new MemoryStream();

            PdfOverlays.Apply(input, output, new List<PdfOverlayElement>());

            Assert.True(output.Length > 0);
        }

        // ── Model tests ──

        [Fact]
        public void TextOverlay_DefaultValues()
        {
            var overlay = new TextOverlay();
            Assert.Equal(1, overlay.Page);
            Assert.Equal(0f, overlay.X);
            Assert.Equal(0f, overlay.Y);
            Assert.Equal("", overlay.Text);
            Assert.Equal(12f, overlay.FontSize);
            Assert.Equal("Helvetica", overlay.FontFamily);
            Assert.False(overlay.Bold);
            Assert.False(overlay.Italic);
        }

        [Fact]
        public void ImageOverlay_DefaultValues()
        {
            var overlay = new ImageOverlay();
            Assert.Equal(1, overlay.Page);
            Assert.Equal(0f, overlay.X);
            Assert.Equal(0f, overlay.Y);
            Assert.Empty(overlay.Data);
            Assert.Equal(0f, overlay.Width);
            Assert.Equal(0f, overlay.Height);
        }
    }
}
