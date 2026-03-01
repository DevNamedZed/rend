using System;
using System.IO;
using System.Text;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class TaggedPdfTests
    {
        [Fact]
        public void TaggedPdf_EnabledOption_ProducesStructTreeRoot()
        {
            var options = new PdfDocumentOptions { EnableTaggedPdf = true };
            using var doc = new PdfDocument(options);
            var page = doc.AddPage(595, 842);
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 750);
            page.Content.ShowText(font, "Tagged PDF test");
            page.Content.EndText();

            string pdfText = SaveToString(doc);
            Assert.Contains("/StructTreeRoot", pdfText);
        }

        [Fact]
        public void TaggedPdf_EnabledOption_ProducesMarkInfo()
        {
            var options = new PdfDocumentOptions { EnableTaggedPdf = true };
            using var doc = new PdfDocument(options);
            var page = doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/MarkInfo", pdfText);
            Assert.Contains("/Marked true", pdfText);
        }

        [Fact]
        public void TaggedPdf_EnabledOption_ProducesStructParents()
        {
            var options = new PdfDocumentOptions { EnableTaggedPdf = true };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/StructParents", pdfText);
        }

        [Fact]
        public void TaggedPdf_EnabledOption_ProducesTabOrder()
        {
            var options = new PdfDocumentOptions { EnableTaggedPdf = true };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/Tabs /S", pdfText);
        }

        [Fact]
        public void TaggedPdf_EnabledOption_ProducesRoleMap()
        {
            var options = new PdfDocumentOptions { EnableTaggedPdf = true };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/RoleMap", pdfText);
        }

        [Fact]
        public void TaggedPdf_EnabledOption_ProducesSectElements()
        {
            var options = new PdfDocumentOptions { EnableTaggedPdf = true };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/S /Sect", pdfText);
        }

        [Fact]
        public void TaggedPdf_EnabledOption_ProducesParentTree()
        {
            var options = new PdfDocumentOptions { EnableTaggedPdf = true };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/ParentTree", pdfText);
            Assert.Contains("/Nums", pdfText);
        }

        [Fact]
        public void TaggedPdf_Disabled_NoStructTreeRoot()
        {
            var options = new PdfDocumentOptions { EnableTaggedPdf = false };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.DoesNotContain("/StructTreeRoot", pdfText);
            Assert.DoesNotContain("/MarkInfo", pdfText);
        }

        [Fact]
        public void TaggedPdf_MarkedContent_BMC_EMC_Operators()
        {
            var options = new PdfDocumentOptions { EnableTaggedPdf = true, Compression = PdfCompression.None };
            using var doc = new PdfDocument(options);
            var page = doc.AddPage(595, 842);
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            page.Content.BeginMarkedContent("Sect");
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 750);
            page.Content.ShowText(font, "Section content");
            page.Content.EndText();
            page.Content.EndMarkedContent();

            string pdfText = SaveToString(doc);
            Assert.Contains("/Sect BMC", pdfText);
            Assert.Contains("EMC", pdfText);
        }

        [Fact]
        public void TaggedPdf_MarkedContent_BDC_WithMCID()
        {
            var options = new PdfDocumentOptions { EnableTaggedPdf = true, Compression = PdfCompression.None };
            using var doc = new PdfDocument(options);
            var page = doc.AddPage(595, 842);

            int mcid = page.Content.BeginMarkedContentDict("P");
            page.Content.EndMarkedContent();

            Assert.Equal(0, mcid);
            string pdfText = SaveToString(doc);
            Assert.Contains("/P <</MCID 0>> BDC", pdfText);
        }

        [Fact]
        public void TaggedPdf_MarkedContent_MultipleMCIDs_Increment()
        {
            var options = new PdfDocumentOptions { EnableTaggedPdf = true };
            using var doc = new PdfDocument(options);
            var page = doc.AddPage(595, 842);

            int mcid0 = page.Content.BeginMarkedContentDict("P");
            page.Content.EndMarkedContent();
            int mcid1 = page.Content.BeginMarkedContentDict("Span");
            page.Content.EndMarkedContent();

            Assert.Equal(0, mcid0);
            Assert.Equal(1, mcid1);
        }

        [Fact]
        public void TaggedPdf_MultiplePages_ProducesMultipleSectElements()
        {
            var options = new PdfDocumentOptions { EnableTaggedPdf = true };
            using var doc = new PdfDocument(options);
            doc.AddPage(595, 842);
            doc.AddPage(595, 842);
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            // Should have structure elements for each page
            Assert.Contains("/StructTreeRoot", pdfText);
            // Count occurrences of /S /Sect
            int sectCount = 0;
            int idx = 0;
            while ((idx = pdfText.IndexOf("/S /Sect", idx, StringComparison.Ordinal)) >= 0)
            {
                sectCount++;
                idx += 8;
            }
            Assert.Equal(3, sectCount);
        }

        [Fact]
        public void TaggedPdf_EndMarkedContentWithoutBegin_Throws()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(595, 842);

            Assert.Throws<InvalidOperationException>(() => page.Content.EndMarkedContent());
        }

        [Fact]
        public void TaggedPdf_WithXmpMetadata_BothPresent()
        {
            var options = new PdfDocumentOptions
            {
                EnableTaggedPdf = true,
                IncludeXmpMetadata = true
            };
            using var doc = new PdfDocument(options);
            doc.Info.Title = "Accessible Document";
            doc.AddPage(595, 842);

            string pdfText = SaveToString(doc);
            Assert.Contains("/StructTreeRoot", pdfText);
            Assert.Contains("/Metadata", pdfText);
        }

        private static string SaveToString(PdfDocument doc)
        {
            using var ms = new MemoryStream();
            doc.Save(ms);
            return Encoding.Latin1.GetString(ms.ToArray());
        }
    }
}
