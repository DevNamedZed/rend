using System;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class ParallelPageTests
    {
        [Fact]
        public void ParallelGeneration_ProducesValidPdf()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions
            {
                Compression = PdfCompression.None,
                ParallelPageGeneration = true
            });

            var font = doc.GetStandardFont(StandardFont.Helvetica);
            for (int i = 0; i < 5; i++)
            {
                var page = doc.AddPage(PageSize.A4);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.ShowText(font, $"Page {i + 1}");
                page.Content.EndText();
            }

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.StartsWith("%PDF-1.", text);
            Assert.Contains("/Root", text);
            Assert.Contains("/Pages", text);
        }

        [Fact]
        public void ParallelGeneration_SameOutputAsSerial()
        {
            // Generate with serial
            byte[] serialBytes;
            using (var doc = new PdfDocument(new PdfDocumentOptions
            {
                Compression = PdfCompression.None,
                ParallelPageGeneration = false
            }))
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                for (int i = 0; i < 3; i++)
                {
                    var page = doc.AddPage(PageSize.A4);
                    page.Content.BeginText();
                    page.Content.SetFont(font, 14);
                    page.Content.ShowText(font, $"Hello {i}");
                    page.Content.EndText();
                }
                serialBytes = doc.ToArray();
            }

            // Generate with parallel
            byte[] parallelBytes;
            using (var doc = new PdfDocument(new PdfDocumentOptions
            {
                Compression = PdfCompression.None,
                ParallelPageGeneration = true
            }))
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                for (int i = 0; i < 3; i++)
                {
                    var page = doc.AddPage(PageSize.A4);
                    page.Content.BeginText();
                    page.Content.SetFont(font, 14);
                    page.Content.ShowText(font, $"Hello {i}");
                    page.Content.EndText();
                }
                parallelBytes = doc.ToArray();
            }

            // Output should be identical
            Assert.Equal(serialBytes.Length, parallelBytes.Length);
            Assert.Equal(serialBytes, parallelBytes);
        }

        [Fact]
        public void ParallelGeneration_ManyPages()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions
            {
                Compression = PdfCompression.None,
                ParallelPageGeneration = true
            });

            var font = doc.GetStandardFont(StandardFont.Helvetica);

            for (int i = 0; i < 50; i++)
            {
                var page = doc.AddPage(PageSize.A4);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.ShowText(font, $"Page {i + 1}");
                page.Content.EndText();
                page.Content.SetFillColor(0, 0, 0);
                page.Content.Rectangle(50, 600, 200, 50);
                page.Content.Fill();
            }

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.StartsWith("%PDF-1.", text);
            Assert.Contains("/Pages", text);
            Assert.Contains("/Count 50", text);
        }

        [Fact]
        public void ParallelGeneration_WithAnnotations()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions
            {
                Compression = PdfCompression.None,
                ParallelPageGeneration = true
            });

            for (int i = 0; i < 5; i++)
            {
                var page = doc.AddPage(PageSize.A4);
                page.AddLink(new RectF(50, 700, 200, 20), new Uri($"https://example.com/{i}"));
            }

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.Contains("/Annots", text);
            Assert.Contains("/URI", text);
        }

        [Fact]
        public void ParallelGeneration_SinglePage_StillWorks()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions
            {
                Compression = PdfCompression.None,
                ParallelPageGeneration = true
            });

            var page = doc.AddPage(PageSize.A4);
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.ShowText(font, "Single page");
            page.Content.EndText();

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.StartsWith("%PDF-1.", text);
            Assert.Contains("Single page", text);
        }

        [Fact]
        public void ParallelGeneration_Compressed()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions
            {
                Compression = PdfCompression.Flate,
                ParallelPageGeneration = true
            });

            var font = doc.GetStandardFont(StandardFont.Helvetica);
            for (int i = 0; i < 10; i++)
            {
                var page = doc.AddPage(PageSize.A4);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.ShowText(font, $"Compressed page {i + 1}");
                page.Content.EndText();
            }

            var bytes = doc.ToArray();
            Assert.True(bytes.Length > 0);

            var text = Encoding.ASCII.GetString(bytes);
            Assert.StartsWith("%PDF-1.", text);
            Assert.Contains("/FlateDecode", text);
        }

        [Fact]
        public void ParallelGeneration_WithMultipleFonts()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions
            {
                Compression = PdfCompression.None,
                ParallelPageGeneration = true
            });

            var font1 = doc.GetStandardFont(StandardFont.Helvetica);
            var font2 = doc.GetStandardFont(StandardFont.CourierBold);

            for (int i = 0; i < 5; i++)
            {
                var page = doc.AddPage(PageSize.A4);
                page.Content.BeginText();
                page.Content.SetFont(font1, 12);
                page.Content.ShowText(font1, $"Helvetica {i}");
                page.Content.SetFont(font2, 10);
                page.Content.ShowText(font2, $"Courier {i}");
                page.Content.EndText();
            }

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.Contains("/Font", text);
            Assert.Contains("Helvetica", text);
            Assert.Contains("Courier", text);
        }

        [Fact]
        public void ParallelGeneration_WithGraphicsState()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions
            {
                Compression = PdfCompression.None,
                ParallelPageGeneration = true
            });

            for (int i = 0; i < 5; i++)
            {
                var page = doc.AddPage(PageSize.A4);
                page.Content.SaveState();
                page.Content.SetFillColor(1, 0, 0);
                page.Content.Rectangle(50, 700, 100, 50);
                page.Content.Fill();
                page.Content.RestoreState();
            }

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.StartsWith("%PDF-1.", text);
        }

        [Fact]
        public void ParallelGeneration_DefaultDisabled()
        {
            var options = new PdfDocumentOptions();
            Assert.False(options.ParallelPageGeneration);
        }

        [Fact]
        public void ParallelGeneration_WithHighlightAnnotations()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions
            {
                Compression = PdfCompression.None,
                ParallelPageGeneration = true
            });

            for (int i = 0; i < 5; i++)
            {
                var page = doc.AddPage(PageSize.A4);
                page.AddHighlight(new RectF(50, 700, 200, 15));
            }

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.Contains("/Subtype /Highlight", text);
            Assert.Contains("/Annots", text);
        }
    }
}
