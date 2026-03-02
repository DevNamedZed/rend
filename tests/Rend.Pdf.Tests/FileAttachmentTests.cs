using System;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class FileAttachmentTests
    {
        [Fact]
        public void AttachFile_ContainsEmbeddedFiles()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AttachFile("readme.txt", Encoding.UTF8.GetBytes("Hello, World!"), "text/plain");
            doc.AddPage(PageSize.A4);

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/EmbeddedFiles", text);
        }

        [Fact]
        public void AttachFile_ContainsFilespec()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AttachFile("test.pdf", new byte[] { 1, 2, 3 }, "application/pdf");
            doc.AddPage(PageSize.A4);

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Type /Filespec", text);
        }

        [Fact]
        public void AttachFile_ContainsFileName()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AttachFile("data.csv", new byte[] { 0x41, 0x42 }, "text/csv");
            doc.AddPage(PageSize.A4);

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/F (data.csv)", text);
            Assert.Contains("/UF (data.csv)", text);
        }

        [Fact]
        public void AttachFile_ContainsEmbeddedFileType()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AttachFile("image.png", new byte[] { 0x89, 0x50 }, "image/png");
            doc.AddPage(PageSize.A4);

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Type /EmbeddedFile", text);
        }

        [Fact]
        public void AttachFile_ContainsEFDictionary()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AttachFile("doc.txt", new byte[] { 0x48, 0x65 }, "text/plain");
            doc.AddPage(PageSize.A4);

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/EF", text);
        }

        [Fact]
        public void AttachFile_ContainsNamesInCatalog()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AttachFile("test.txt", new byte[] { 0x41 }, "text/plain");
            doc.AddPage(PageSize.A4);

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Names", text);
        }

        [Fact]
        public void AttachFile_ContainsFileSize()
        {
            var data = new byte[] { 1, 2, 3, 4, 5 };
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AttachFile("test.bin", data, "application/octet-stream");
            doc.AddPage(PageSize.A4);

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Size 5", text);
        }

        [Fact]
        public void AttachFile_WithDescription()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var attachment = doc.AttachFile("report.txt", new byte[] { 0x41 }, "text/plain");
            attachment.Description = "Monthly report";
            doc.AddPage(PageSize.A4);

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Desc (Monthly report)", text);
        }

        [Fact]
        public void AttachFile_MultipleFiles()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AttachFile("file1.txt", new byte[] { 0x41 }, "text/plain");
            doc.AttachFile("file2.txt", new byte[] { 0x42 }, "text/plain");
            doc.AddPage(PageSize.A4);

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("(file1.txt)", text);
            Assert.Contains("(file2.txt)", text);
            // Both should be in the EmbeddedFiles name tree
            var filespecCount = CountOccurrences(text, "/Type /Filespec");
            Assert.Equal(2, filespecCount);
        }

        [Fact]
        public void AttachFile_ValidPdf()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AttachFile("test.txt", new byte[] { 0x41, 0x42, 0x43 }, "text/plain");
            doc.AddPage(PageSize.A4);

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.StartsWith("%PDF-1.", text);
            Assert.Contains("/Root", text);
        }

        [Fact]
        public void AttachFile_NoAttachments_NoNamesDict()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddPage(PageSize.A4);

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.DoesNotContain("/EmbeddedFiles", text);
        }

        [Fact]
        public void AttachFile_WithDates()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var attachment = doc.AttachFile("dated.txt", new byte[] { 0x41 }, "text/plain");
            attachment.CreationDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
            attachment.ModDate = new DateTime(2024, 6, 20, 14, 0, 0, DateTimeKind.Utc);
            doc.AddPage(PageSize.A4);

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Params", text);
            Assert.Contains("/CreationDate", text);
            Assert.Contains("/ModDate", text);
        }

        [Fact]
        public void AttachFile_LargeData()
        {
            var data = new byte[10000];
            new Random(42).NextBytes(data);

            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AttachFile("large.bin", data, "application/octet-stream");
            doc.AddPage(PageSize.A4);

            var pdfBytes = doc.ToArray();
            Assert.True(pdfBytes.Length > data.Length, "PDF should contain at least the attached data");
            var text = Encoding.ASCII.GetString(pdfBytes);
            Assert.Contains("/Size 10000", text);
        }

        private static int CountOccurrences(string text, string pattern)
        {
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(pattern, index)) != -1)
            {
                count++;
                index += pattern.Length;
            }
            return count;
        }
    }
}
