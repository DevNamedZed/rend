using System;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class ObjectStreamTests
    {
        // ═══════════════════════════════════════════
        // Cross-reference streams (no object packing)
        // ═══════════════════════════════════════════

        [Fact]
        public void UseObjectStreams_Pdf15_ProducesValidPdf()
        {
            var pdfBytes = GenerateTestPdf(useObjStreams: true, PdfVersion.Pdf15);
            AssertValidPdf(pdfBytes);
        }

        [Fact]
        public void UseObjectStreams_Pdf17_ProducesValidPdf()
        {
            var pdfBytes = GenerateTestPdf(useObjStreams: true, PdfVersion.Pdf17);
            AssertValidPdf(pdfBytes);
        }

        [Fact]
        public void UseObjectStreams_ContainsXRefType()
        {
            var pdfBytes = GenerateTestPdf(useObjStreams: true, PdfVersion.Pdf15);
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            Assert.Contains("/Type /XRef", pdfText);
        }

        [Fact]
        public void UseObjectStreams_ContainsObjStmType()
        {
            var pdfBytes = GenerateTestPdf(useObjStreams: true, PdfVersion.Pdf15);
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            Assert.Contains("/Type /ObjStm", pdfText);
        }

        [Fact]
        public void UseObjectStreams_NoTraditionalXref()
        {
            var pdfBytes = GenerateTestPdf(useObjStreams: true, PdfVersion.Pdf15);
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            // Should not contain traditional "xref\n" table header
            // The /XRef stream replaces the traditional xref table
            Assert.DoesNotContain("\nxref\n", pdfText);
            Assert.DoesNotContain("trailer\n", pdfText);
        }

        [Fact]
        public void UseObjectStreams_ContainsStartxref()
        {
            var pdfBytes = GenerateTestPdf(useObjStreams: true, PdfVersion.Pdf15);
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            Assert.Contains("startxref", pdfText);
        }

        [Fact]
        public void UseObjectStreams_SmallerThanTraditional()
        {
            var traditional = GenerateLargeTestPdf(useObjStreams: false);
            var objStreams = GenerateLargeTestPdf(useObjStreams: true);

            // Object streams should produce smaller output due to compression
            Assert.True(objStreams.Length < traditional.Length,
                $"Object streams ({objStreams.Length}) should be smaller than traditional ({traditional.Length})");
        }

        // ═══════════════════════════════════════════
        // Traditional xref still works
        // ═══════════════════════════════════════════

        [Fact]
        public void TraditionalXref_Pdf15_StillWorks()
        {
            var pdfBytes = GenerateTestPdf(useObjStreams: false, PdfVersion.Pdf15);
            AssertValidPdf(pdfBytes);
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            Assert.Contains("xref\n", pdfText);
            Assert.Contains("trailer", pdfText);
        }

        // ═══════════════════════════════════════════
        // Version gating
        // ═══════════════════════════════════════════

        [Fact]
        public void UseObjectStreams_Pdf14_FallsBackToTraditional()
        {
            // PDF 1.4 doesn't support xref streams, so UseObjectStreams should be ignored
            var pdfBytes = GenerateTestPdf(useObjStreams: true, PdfVersion.Pdf14);
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            // Should use traditional xref when version is too low
            Assert.Contains("xref\n", pdfText);
            Assert.Contains("trailer", pdfText);
        }

        // ═══════════════════════════════════════════
        // XRef stream W array
        // ═══════════════════════════════════════════

        [Fact]
        public void UseObjectStreams_ContainsWArray()
        {
            var pdfBytes = GenerateTestPdf(useObjStreams: true, PdfVersion.Pdf15);
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            // The W array specifies field sizes: [type-size offset-size gen-size]
            Assert.Contains("/W [", pdfText);
        }

        // ═══════════════════════════════════════════
        // Multi-page with object streams
        // ═══════════════════════════════════════════

        [Fact]
        public void UseObjectStreams_MultiPage_ProducesValidPdf()
        {
            var options = new PdfDocumentOptions
            {
                Version = PdfVersion.Pdf17,
                UseObjectStreams = true
            };

            using var doc = new PdfDocument(options);
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            for (int i = 0; i < 5; i++)
            {
                var page = doc.AddPage(PageSize.A4);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 750);
                page.Content.ShowText(font, $"Page {i + 1}");
                page.Content.EndText();
            }

            var pdfBytes = doc.ToArray();
            AssertValidPdf(pdfBytes);

            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            Assert.Contains("/Type /XRef", pdfText);
            Assert.Contains("/Type /ObjStm", pdfText);
        }

        // ═══════════════════════════════════════════
        // Object streams with metadata
        // ═══════════════════════════════════════════

        [Fact]
        public void UseObjectStreams_WithMetadata_ProducesValidPdf()
        {
            var options = new PdfDocumentOptions
            {
                Version = PdfVersion.Pdf15,
                UseObjectStreams = true
            };

            using var doc = new PdfDocument(options);
            doc.Info.Title = "Object Stream Test";
            doc.Info.Author = "Test Author";

            var font = doc.GetStandardFont(StandardFont.Helvetica);
            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 750);
            page.Content.ShowText(font, "Test with metadata.");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            AssertValidPdf(pdfBytes);
        }

        // ═══════════════════════════════════════════
        // Object streams contain /N and /First
        // ═══════════════════════════════════════════

        [Fact]
        public void UseObjectStreams_ObjStmHasNAndFirst()
        {
            var pdfBytes = GenerateTestPdf(useObjStreams: true, PdfVersion.Pdf15);
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            // ObjStm should have /N (count of objects) and /First (byte offset of first object)
            Assert.Contains("/N ", pdfText);
            Assert.Contains("/First ", pdfText);
        }

        // ═══════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════

        private static byte[] GenerateTestPdf(bool useObjStreams, PdfVersion version = PdfVersion.Pdf17)
        {
            var options = new PdfDocumentOptions
            {
                Version = version,
                UseObjectStreams = useObjStreams
            };

            using var doc = new PdfDocument(options);
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            var page = doc.AddPage(PageSize.A4);

            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 750);
            page.Content.ShowText(font, "Cross-reference stream test.");
            page.Content.EndText();

            return doc.ToArray();
        }

        private static byte[] GenerateLargeTestPdf(bool useObjStreams)
        {
            var options = new PdfDocumentOptions
            {
                Version = PdfVersion.Pdf17,
                UseObjectStreams = useObjStreams
            };

            using var doc = new PdfDocument(options);
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            for (int p = 0; p < 10; p++)
            {
                var page = doc.AddPage(PageSize.A4);
                page.Content.BeginText();
                page.Content.SetFont(font, 10);
                page.Content.MoveTextPosition(50, 750);
                for (int i = 0; i < 50; i++)
                    page.Content.ShowText(font, "The quick brown fox jumps over the lazy dog. ");
                page.Content.EndText();
            }

            return doc.ToArray();
        }

        private static void AssertValidPdf(byte[] pdfBytes)
        {
            Assert.True(pdfBytes.Length > 100, "PDF should be more than 100 bytes");
            string header = Encoding.ASCII.GetString(pdfBytes, 0, 5);
            Assert.Equal("%PDF-", header);
            string fullText = Encoding.ASCII.GetString(pdfBytes);
            Assert.Contains("%%EOF", fullText);
            // Note: /Catalog may be inside a compressed ObjStm, so we check for /Root instead
            // which appears in the xref stream trailer dict (always uncompressed)
            Assert.Contains("/Root", fullText);
        }
    }
}
