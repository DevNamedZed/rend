using System;
using System.IO;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class PdfDocumentTests
    {
        // ═══════════════════════════════════════════
        // Document Creation
        // ═══════════════════════════════════════════

        [Fact]
        public void Constructor_Default_CreatesEmptyDocument()
        {
            using var doc = new PdfDocument();
            Assert.Equal(0, doc.PageCount);
        }

        [Fact]
        public void Constructor_WithOptions_AppliesOptions()
        {
            var options = new PdfDocumentOptions
            {
                Version = PdfVersion.Pdf14,
                Compression = PdfCompression.None
            };
            using var doc = new PdfDocument(options);
            Assert.Equal(PdfVersion.Pdf14, doc.Options.Version);
            Assert.Equal(PdfCompression.None, doc.Options.Compression);
        }

        [Fact]
        public void Constructor_NullOptions_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PdfDocument(null!));
        }

        // ═══════════════════════════════════════════
        // Document Info
        // ═══════════════════════════════════════════

        [Fact]
        public void Info_DefaultProducer_IsRendPdf()
        {
            using var doc = new PdfDocument();
            Assert.Equal("Rend.Pdf", doc.Info.Producer);
        }

        [Fact]
        public void Info_SetProperties_RetainsValues()
        {
            using var doc = new PdfDocument();
            doc.Info.Title = "Test Title";
            doc.Info.Author = "Test Author";
            doc.Info.Subject = "Test Subject";
            doc.Info.Keywords = "test, pdf";
            doc.Info.Creator = "Test Creator";

            Assert.Equal("Test Title", doc.Info.Title);
            Assert.Equal("Test Author", doc.Info.Author);
            Assert.Equal("Test Subject", doc.Info.Subject);
            Assert.Equal("test, pdf", doc.Info.Keywords);
            Assert.Equal("Test Creator", doc.Info.Creator);
        }

        [Fact]
        public void Info_CreationDate_IsSetAutomatically()
        {
            using var doc = new PdfDocument();
            Assert.NotNull(doc.Info.CreationDate);
            Assert.True((DateTime.UtcNow - doc.Info.CreationDate.Value).TotalSeconds < 5);
        }

        [Fact]
        public void Info_MetadataAppearsInOutput()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.Info.Title = "Metadata Test";
            doc.Info.Author = "Unit Test";
            doc.AddPage(PageSize.A4);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.Contains("Metadata Test", text);
            Assert.Contains("Unit Test", text);
        }

        // ═══════════════════════════════════════════
        // Pages
        // ═══════════════════════════════════════════

        [Fact]
        public void AddPage_WithDimensions_IncrementsPageCount()
        {
            using var doc = new PdfDocument();
            doc.AddPage(612, 792);
            Assert.Equal(1, doc.PageCount);
        }

        [Fact]
        public void AddPage_WithSizeF_IncrementsPageCount()
        {
            using var doc = new PdfDocument();
            doc.AddPage(PageSize.A4);
            Assert.Equal(1, doc.PageCount);
        }

        [Fact]
        public void AddPage_Multiple_TracksAllPages()
        {
            using var doc = new PdfDocument();
            doc.AddPage(PageSize.A4);
            doc.AddPage(PageSize.Letter);
            doc.AddPage(PageSize.Legal);
            Assert.Equal(3, doc.PageCount);
        }

        [Fact]
        public void AddPage_ReturnsPdfPage_WithCorrectDimensions()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(612, 792);
            Assert.Equal(612, page.Width);
            Assert.Equal(792, page.Height);
        }

        [Fact]
        public void AddPage_A4_HasCorrectDimensions()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            Assert.Equal(PageSize.A4.Width, page.Width);
            Assert.Equal(PageSize.A4.Height, page.Height);
        }

        [Fact]
        public void AddPage_PageIndex_IsZeroBased()
        {
            using var doc = new PdfDocument();
            var page0 = doc.AddPage(PageSize.A4);
            var page1 = doc.AddPage(PageSize.A4);
            Assert.Equal(0, page0.PageIndex);
            Assert.Equal(1, page1.PageIndex);
        }

        [Fact]
        public void InsertPage_AtIndex_InsertsCorrectly()
        {
            using var doc = new PdfDocument();
            doc.AddPage(PageSize.A4);
            doc.AddPage(PageSize.A4);
            var inserted = doc.InsertPage(1, 500, 700);
            Assert.Equal(3, doc.PageCount);
            Assert.Equal(500, inserted.Width);
            Assert.Equal(700, inserted.Height);
        }

        // ═══════════════════════════════════════════
        // Save
        // ═══════════════════════════════════════════

        [Fact]
        public void Save_ToStream_ProducesValidPdfHeader()
        {
            using var doc = new PdfDocument();
            doc.AddPage(PageSize.A4);

            using var ms = new MemoryStream();
            doc.Save(ms);

            ms.Position = 0;
            var bytes = ms.ToArray();
            var headerText = Encoding.ASCII.GetString(bytes, 0, Math.Min(20, bytes.Length));
            Assert.StartsWith("%PDF-", headerText);
        }

        [Fact]
        public void Save_ToStream_ContainsEofMarker()
        {
            using var doc = new PdfDocument();
            doc.AddPage(PageSize.A4);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);
            Assert.Contains("%%EOF", text);
        }

        [Fact]
        public void Save_WithPdfVersion14_WritesCorrectVersion()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Version = PdfVersion.Pdf14 });
            doc.AddPage(PageSize.A4);

            var bytes = doc.ToArray();
            var headerText = Encoding.ASCII.GetString(bytes, 0, 10);
            Assert.StartsWith("%PDF-1.4", headerText);
        }

        [Fact]
        public void Save_WithPdfVersion17_WritesCorrectVersion()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Version = PdfVersion.Pdf17 });
            doc.AddPage(PageSize.A4);

            var bytes = doc.ToArray();
            var headerText = Encoding.ASCII.GetString(bytes, 0, 10);
            Assert.StartsWith("%PDF-1.7", headerText);
        }

        [Fact]
        public void Save_ContainsCatalog()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddPage(PageSize.A4);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);
            Assert.Contains("/Catalog", text);
        }

        [Fact]
        public void Save_ContainsPageObject()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddPage(PageSize.A4);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);
            Assert.Contains("/Page", text);
        }

        [Fact]
        public void Save_ContainsMediaBox()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddPage(612, 792);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);
            Assert.Contains("/MediaBox", text);
        }

        [Fact]
        public void Save_ContainsXrefTable()
        {
            using var doc = new PdfDocument();
            doc.AddPage(PageSize.A4);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);
            Assert.Contains("xref", text);
        }

        [Fact]
        public void Save_ContainsTrailer()
        {
            using var doc = new PdfDocument();
            doc.AddPage(PageSize.A4);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);
            Assert.Contains("trailer", text);
        }

        [Fact]
        public void ToArray_ReturnsValidPdf()
        {
            using var doc = new PdfDocument();
            doc.AddPage(PageSize.A4);

            var bytes = doc.ToArray();
            Assert.True(bytes.Length > 0);
            var header = Encoding.ASCII.GetString(bytes, 0, 5);
            Assert.Equal("%PDF-", header);
        }

        [Fact]
        public void Save_NullStream_Throws()
        {
            using var doc = new PdfDocument();
            Assert.Throws<ArgumentNullException>(() => doc.Save((Stream)null!));
        }

        [Fact]
        public void Save_NullFilePath_Throws()
        {
            using var doc = new PdfDocument();
            Assert.Throws<ArgumentNullException>(() => doc.Save((string)null!));
        }

        [Fact]
        public void Save_MultiplePages_AllPagesInOutput()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddPage(PageSize.A4);
            doc.AddPage(PageSize.Letter);
            doc.AddPage(PageSize.Legal);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            // The Pages tree should have a Count of 3
            Assert.Contains("/Count 3", text);
        }

        [Fact]
        public void Save_WithCompression_ProducesSmallerOutput()
        {
            // Generate the same document with and without compression
            byte[] compressed, uncompressed;

            using (var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.Flate }))
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                var page = doc.AddPage(PageSize.A4);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 750);
                // Write enough text to make compression meaningful
                for (int i = 0; i < 50; i++)
                    page.Content.ShowText(font, "This is a test line of text that should benefit from compression. ");
                page.Content.EndText();
                compressed = doc.ToArray();
            }

            using (var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None }))
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                var page = doc.AddPage(PageSize.A4);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 750);
                for (int i = 0; i < 50; i++)
                    page.Content.ShowText(font, "This is a test line of text that should benefit from compression. ");
                page.Content.EndText();
                uncompressed = doc.ToArray();
            }

            Assert.True(compressed.Length < uncompressed.Length,
                $"Compressed ({compressed.Length}) should be smaller than uncompressed ({uncompressed.Length})");
        }

        // ═══════════════════════════════════════════
        // Fonts
        // ═══════════════════════════════════════════

        [Theory]
        [InlineData(StandardFont.Helvetica, "Helvetica")]
        [InlineData(StandardFont.HelveticaBold, "Helvetica-Bold")]
        [InlineData(StandardFont.TimesRoman, "Times-Roman")]
        [InlineData(StandardFont.Courier, "Courier")]
        public void GetStandardFont_ReturnsCorrectBaseFont(StandardFont standardFont, string expectedName)
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(standardFont);
            Assert.Equal(expectedName, font.BaseFont);
            Assert.True(font.IsStandard14);
        }

        [Fact]
        public void GetStandardFont_SameFont_ReturnsSameInstance()
        {
            using var doc = new PdfDocument();
            var font1 = doc.GetStandardFont(StandardFont.Helvetica);
            var font2 = doc.GetStandardFont(StandardFont.Helvetica);
            Assert.Same(font1, font2);
        }

        [Fact]
        public void GetStandardFont_DifferentFonts_ReturnDifferentInstances()
        {
            using var doc = new PdfDocument();
            var helvetica = doc.GetStandardFont(StandardFont.Helvetica);
            var courier = doc.GetStandardFont(StandardFont.Courier);
            Assert.NotSame(helvetica, courier);
        }

        [Fact]
        public void GetStandardFont_AllFontsCanBeObtained()
        {
            using var doc = new PdfDocument();
            foreach (StandardFont sf in Enum.GetValues(typeof(StandardFont)))
            {
                var font = doc.GetStandardFont(sf);
                Assert.NotNull(font);
                Assert.True(font.IsStandard14);
            }
        }

        [Fact]
        public void Save_WithFont_ContainsFontReference()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.ShowText(font, "Hello");
            page.Content.EndText();

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);
            Assert.Contains("/Helvetica", text);
            Assert.Contains("/Type1", text);
        }

        // ═══════════════════════════════════════════
        // Full Document Generation
        // ═══════════════════════════════════════════

        [Fact]
        public void FullDocument_HelloWorld_ProducesValidPdf()
        {
            using var doc = new PdfDocument();
            doc.Info.Title = "Hello World";
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            var page = doc.AddPage(PageSize.A4);

            page.Content.BeginText();
            page.Content.SetFont(font, 24);
            page.Content.MoveTextPosition(50, 750);
            page.Content.ShowText(font, "Hello, World!");
            page.Content.EndText();

            var bytes = doc.ToArray();

            // Verify PDF header
            Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(bytes, 0, 5));

            // Verify EOF
            var fullText = Encoding.ASCII.GetString(bytes);
            Assert.Contains("%%EOF", fullText);

            // Verify non-trivial size
            Assert.True(bytes.Length > 100, "PDF should be more than 100 bytes");
        }

        [Fact]
        public void Dispose_CanCallMultipleTimes()
        {
            var doc = new PdfDocument();
            doc.AddPage(PageSize.A4);
            doc.Dispose();
            doc.Dispose(); // Should not throw
        }

        // ═══════════════════════════════════════════
        // Images
        // ═══════════════════════════════════════════

        [Fact]
        public void AddImage_NullStream_Throws()
        {
            using var doc = new PdfDocument();
            Assert.Throws<ArgumentNullException>(() => doc.AddImage((Stream)null!, ImageFormat.Jpeg));
        }

        [Fact]
        public void AddImage_NullBytes_Throws()
        {
            using var doc = new PdfDocument();
            Assert.Throws<ArgumentNullException>(() => doc.AddImage((byte[])null!, ImageFormat.Jpeg));
        }

        [Fact]
        public void AddFont_NullStream_Throws()
        {
            using var doc = new PdfDocument();
            Assert.Throws<ArgumentNullException>(() => doc.AddFont((Stream)null!));
        }
    }
}
