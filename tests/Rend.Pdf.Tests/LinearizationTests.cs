using System;
using System.IO;
using System.Text;
using Rend.Core.Values;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class LinearizationTests
    {
        // ═══════════════════════════════════════════
        // Non-linearized baseline
        // ═══════════════════════════════════════════

        [Fact]
        public void NonLinearized_DoesNotContainLinearizedKey()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.DoesNotContain("/Linearized", pdfText);
        }

        // ═══════════════════════════════════════════
        // Linearized document structure
        // ═══════════════════════════════════════════

        [Fact]
        public void Linearized_HasLinearizedDictAsFirstObject()
        {
            var options = new PdfDocumentOptions
            {
                Linearize = true,
                Compression = PdfCompression.None
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            byte[] pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // The /Linearized key must appear in the output
            Assert.Contains("/Linearized 1", pdfText);

            // The linearization dictionary should appear early in the file,
            // before the first regular object
            int linIdx = pdfText.IndexOf("/Linearized 1", StringComparison.Ordinal);
            Assert.True(linIdx > 0 && linIdx < 500,
                $"/Linearized 1 should appear near the start of the file (found at {linIdx})");
        }

        [Fact]
        public void Linearized_HasValidXref()
        {
            var options = new PdfDocumentOptions
            {
                Linearize = true,
                Compression = PdfCompression.None
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            byte[] pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // Must have xref table
            Assert.Contains("xref", pdfText);

            // Must have startxref
            Assert.Contains("startxref", pdfText);

            // Must have trailer
            Assert.Contains("trailer", pdfText);

            // xref entries should be well-formed (10-digit offset, space, 5-digit gen)
            int xrefIdx = pdfText.IndexOf("xref\n", StringComparison.Ordinal);
            Assert.True(xrefIdx >= 0, "xref table not found");

            // Verify the free list head entry exists
            int freeIdx = pdfText.IndexOf("0000000000 65535 f ", xrefIdx, StringComparison.Ordinal);
            Assert.True(freeIdx >= 0, "Free list head entry not found in xref");
        }

        [Fact]
        public void Linearized_HasCorrectFileLength()
        {
            var options = new PdfDocumentOptions
            {
                Linearize = true,
                Compression = PdfCompression.None
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            byte[] pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // Extract /L value from linearization dictionary
            int lIdx = pdfText.IndexOf("/L ", StringComparison.Ordinal);
            Assert.True(lIdx >= 0, "/L key not found");

            int valueStart = lIdx + 3;
            int valueEnd = valueStart;
            while (valueEnd < pdfText.Length && (pdfText[valueEnd] >= '0' && pdfText[valueEnd] <= '9'))
                valueEnd++;

            long reportedLength = long.Parse(pdfText.Substring(valueStart, valueEnd - valueStart));

            // /L must equal the actual file length
            Assert.Equal(pdfBytes.Length, reportedLength);
        }

        [Fact]
        public void Linearized_MultiPage_HasCorrectPageCount()
        {
            var options = new PdfDocumentOptions
            {
                Linearize = true,
                Compression = PdfCompression.None
            };
            using var doc = new PdfDocument(options);

            // Add 5 pages
            for (int i = 0; i < 5; i++)
                doc.AddPage(595, 842);

            byte[] pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // Find /N value in linearization dictionary (page count)
            // It should appear early in the file within the linearization dict
            int linIdx = pdfText.IndexOf("/Linearized 1", StringComparison.Ordinal);
            Assert.True(linIdx >= 0);

            // Find /N after /Linearized (within the same dictionary)
            int nIdx = pdfText.IndexOf("/N ", linIdx, StringComparison.Ordinal);
            Assert.True(nIdx >= 0, "/N key not found in linearization dictionary");

            int valueStart = nIdx + 3;
            int valueEnd = valueStart;
            while (valueEnd < pdfText.Length && (pdfText[valueEnd] >= '0' && pdfText[valueEnd] <= '9'))
                valueEnd++;

            int reportedPageCount = int.Parse(pdfText.Substring(valueStart, valueEnd - valueStart));
            Assert.Equal(5, reportedPageCount);
        }

        [Fact]
        public void Linearized_IsStillValidPdf()
        {
            var options = new PdfDocumentOptions
            {
                Linearize = true,
                Compression = PdfCompression.None
            };
            using var doc = new PdfDocument(options);
            doc.Info.Title = "Linearization Test";
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginText();
            page.Content.SetFont(font, 24);
            page.Content.MoveTextPosition(50, 750);
            page.Content.ShowText(font, "Hello, Linearized World!");
            page.Content.EndText();

            byte[] pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // Basic PDF structure checks
            Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(pdfBytes, 0, 5));
            Assert.Contains("%%EOF", pdfText);
            Assert.Contains("/Catalog", pdfText);
            Assert.Contains("/Page", pdfText);
            Assert.Contains("/Linearized 1", pdfText);

            // The text content should still be present
            Assert.Contains("Linearization Test", pdfText);

            // File should be non-trivial
            Assert.True(pdfBytes.Length > 200);
        }

        [Fact]
        public void Linearized_HasHintTableReference()
        {
            var options = new PdfDocumentOptions
            {
                Linearize = true,
                Compression = PdfCompression.None
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            byte[] pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // /H array must be present with two values [offset length]
            int hIdx = pdfText.IndexOf("/H [", StringComparison.Ordinal);
            Assert.True(hIdx >= 0, "/H hint table reference not found");

            // Extract the two values
            int bracketStart = hIdx + 4;
            int bracketEnd = pdfText.IndexOf(']', bracketStart);
            Assert.True(bracketEnd > bracketStart, "Malformed /H array");

            string hValues = pdfText.Substring(bracketStart, bracketEnd - bracketStart).Trim();
            string[] parts = hValues.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, parts.Length);

            // Both values should be positive integers
            long hintOffset = long.Parse(parts[0]);
            long hintLength = long.Parse(parts[1]);
            Assert.True(hintOffset > 0, "Hint table offset should be positive");
            Assert.True(hintLength > 0, "Hint table length should be positive");

            // Hint offset + length should not exceed file size
            Assert.True(hintOffset + hintLength <= pdfBytes.Length,
                "Hint table extends beyond file boundary");
        }

        [Fact]
        public void Linearized_FirstPageObjectReference()
        {
            var options = new PdfDocumentOptions
            {
                Linearize = true,
                Compression = PdfCompression.None
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);
            doc.AddPage(612, 792); // second page

            byte[] pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // /O references the first page object number
            int linIdx = pdfText.IndexOf("/Linearized 1", StringComparison.Ordinal);
            int oIdx = pdfText.IndexOf("/O ", linIdx, StringComparison.Ordinal);
            Assert.True(oIdx >= 0, "/O key not found");

            int valueStart = oIdx + 3;
            int valueEnd = valueStart;
            while (valueEnd < pdfText.Length && (pdfText[valueEnd] >= '0' && pdfText[valueEnd] <= '9'))
                valueEnd++;

            int firstPageObjNum = int.Parse(pdfText.Substring(valueStart, valueEnd - valueStart));
            Assert.True(firstPageObjNum > 0, "First page object number should be positive");

            // Verify that object actually exists in the output
            // The library writes "N obj" (without generation number) for gen 0 objects
            string objMarker = $"{firstPageObjNum} obj";
            Assert.Contains(objMarker, pdfText);
        }

        [Fact]
        public void Linearized_EndOfFirstPageOffset()
        {
            var options = new PdfDocumentOptions
            {
                Linearize = true,
                Compression = PdfCompression.None
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            byte[] pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // /E must be present
            int linIdx = pdfText.IndexOf("/Linearized 1", StringComparison.Ordinal);
            int eIdx = pdfText.IndexOf("/E ", linIdx, StringComparison.Ordinal);
            Assert.True(eIdx >= 0, "/E key not found");

            int valueStart = eIdx + 3;
            int valueEnd = valueStart;
            while (valueEnd < pdfText.Length && (pdfText[valueEnd] >= '0' && pdfText[valueEnd] <= '9'))
                valueEnd++;

            long endOfFirstPage = long.Parse(pdfText.Substring(valueStart, valueEnd - valueStart));

            // E should be within the file
            Assert.True(endOfFirstPage > 0, "End of first page offset should be positive");
            Assert.True(endOfFirstPage <= pdfBytes.Length,
                "End of first page offset should not exceed file size");
        }

        [Fact]
        public void Linearized_WithCompression_StillValid()
        {
            var options = new PdfDocumentOptions
            {
                Linearize = true,
                Compression = PdfCompression.Flate
            };
            using var doc = new PdfDocument(options);
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 750);
            page.Content.ShowText(font, "Compressed + Linearized");
            page.Content.EndText();

            byte[] pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(pdfBytes, 0, 5));
            Assert.Contains("/Linearized 1", pdfText);
            Assert.Contains("%%EOF", pdfText);
        }

        [Fact]
        public void Linearized_MultiPage_WithContent_StillValid()
        {
            var options = new PdfDocumentOptions
            {
                Linearize = true,
                Compression = PdfCompression.None
            };
            using var doc = new PdfDocument(options);
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            for (int i = 0; i < 3; i++)
            {
                var page = doc.AddPage(PageSize.A4);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 750);
                page.Content.ShowText(font, $"Page {i + 1} content");
                page.Content.EndText();
            }

            byte[] pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(pdfBytes, 0, 5));
            Assert.Contains("/Linearized 1", pdfText);
            Assert.Contains("/N 3", pdfText); // 3 pages
            Assert.Contains("%%EOF", pdfText);
            Assert.Contains("/Catalog", pdfText);
        }

        [Fact]
        public void Linearized_XrefOffsetMatchesActualObjects()
        {
            var options = new PdfDocumentOptions
            {
                Linearize = true,
                Compression = PdfCompression.None
            };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            byte[] pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // Find the xref table
            int xrefIdx = pdfText.IndexOf("xref\n", StringComparison.Ordinal);
            Assert.True(xrefIdx >= 0);

            // Find the startxref value
            int startXrefIdx = pdfText.LastIndexOf("startxref\n", StringComparison.Ordinal);
            Assert.True(startXrefIdx >= 0);

            int offsetStart = startXrefIdx + "startxref\n".Length;
            int offsetEnd = offsetStart;
            while (offsetEnd < pdfText.Length && pdfText[offsetEnd] >= '0' && pdfText[offsetEnd] <= '9')
                offsetEnd++;

            long reportedXrefOffset = long.Parse(pdfText.Substring(offsetStart, offsetEnd - offsetStart));

            // The startxref value should point to the actual xref table position
            Assert.Equal(xrefIdx, reportedXrefOffset);
        }

        // ═══════════════════════════════════════════
        // Helper
        // ═══════════════════════════════════════════

        private static string SaveToString(PdfDocument doc)
        {
            using var ms = new MemoryStream();
            doc.Save(ms);
            return Encoding.Latin1.GetString(ms.ToArray());
        }
    }
}
