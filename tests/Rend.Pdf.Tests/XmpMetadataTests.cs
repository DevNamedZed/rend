using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class XmpMetadataTests
    {
        private static string GeneratePdf(bool includeXmp, PdfCompression compression = PdfCompression.None,
                                           string? title = null, string? author = null,
                                           string? subject = null, string? keywords = null,
                                           string? creator = null)
        {
            var options = new PdfDocumentOptions
            {
                Compression = compression,
                IncludeXmpMetadata = includeXmp
            };
            using var doc = new PdfDocument(options);
            if (title != null) doc.Info.Title = title;
            if (author != null) doc.Info.Author = author;
            if (subject != null) doc.Info.Subject = subject;
            if (keywords != null) doc.Info.Keywords = keywords;
            if (creator != null) doc.Info.Creator = creator;
            doc.AddPage(PageSize.A4);
            return Encoding.UTF8.GetString(doc.ToArray());
        }

        [Fact]
        public void Disabled_NoXpacketInOutput()
        {
            var text = GeneratePdf(includeXmp: false);
            Assert.DoesNotContain("<?xpacket", text);
        }

        [Fact]
        public void Enabled_ContainsXpacket()
        {
            var text = GeneratePdf(includeXmp: true);
            Assert.Contains("<?xpacket", text);
        }

        [Fact]
        public void Enabled_ContainsXmpmeta()
        {
            var text = GeneratePdf(includeXmp: true);
            Assert.Contains("x:xmpmeta", text);
        }

        [Fact]
        public void Enabled_ContainsDcTitle()
        {
            var text = GeneratePdf(includeXmp: true, title: "Test Title");
            Assert.Contains("dc:title", text);
            Assert.Contains("Test Title", text);
        }

        [Fact]
        public void Enabled_ContainsDcCreator()
        {
            var text = GeneratePdf(includeXmp: true, author: "Test Author");
            Assert.Contains("dc:creator", text);
            Assert.Contains("Test Author", text);
        }

        [Fact]
        public void Enabled_ContainsDcDescription()
        {
            var text = GeneratePdf(includeXmp: true, subject: "Test Subject");
            Assert.Contains("dc:description", text);
            Assert.Contains("Test Subject", text);
        }

        [Fact]
        public void Enabled_ContainsPdfKeywords()
        {
            var text = GeneratePdf(includeXmp: true, keywords: "test, pdf, xmp");
            Assert.Contains("pdf:Keywords", text);
            Assert.Contains("test, pdf, xmp", text);
        }

        [Fact]
        public void Enabled_ContainsPdfProducer()
        {
            var text = GeneratePdf(includeXmp: true);
            Assert.Contains("pdf:Producer", text);
            Assert.Contains("Rend.Pdf", text);
        }

        [Fact]
        public void Enabled_ContainsXmpDates()
        {
            var text = GeneratePdf(includeXmp: true);
            Assert.Contains("xmp:CreateDate", text);
            Assert.Contains("xmp:ModifyDate", text);
        }

        [Fact]
        public void StreamNotCompressed_EvenWithFlate()
        {
            var text = GeneratePdf(includeXmp: true, compression: PdfCompression.Flate, title: "Compress Test");
            // The XMP stream should contain readable xpacket even when flate is on
            Assert.Contains("<?xpacket", text);
            Assert.Contains("x:xmpmeta", text);
        }

        [Fact]
        public void MetadataTypeAndSubtypeInOutput()
        {
            var text = GeneratePdf(includeXmp: true);
            Assert.Contains("/Metadata", text);
            Assert.Contains("/XML", text);
        }

        [Fact]
        public void XmlSpecialCharactersEscaped()
        {
            var text = GeneratePdf(includeXmp: true, title: "A & B <C> \"D\" 'E'");
            Assert.Contains("A &amp; B &lt;C&gt; &quot;D&quot; &apos;E&apos;", text);
        }
    }
}
