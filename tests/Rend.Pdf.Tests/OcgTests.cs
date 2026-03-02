using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class OcgTests
    {
        [Fact]
        public void Layer_ContainsOCGType()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var layer = doc.AddLayer("Watermark");
            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginLayer(layer);
            page.Content.SetFillColor(0.8f, 0.8f, 0.8f);
            page.Content.Rectangle(0, 0, 100, 100);
            page.Content.Fill();
            page.Content.EndLayer();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Type /OCG", text);
        }

        [Fact]
        public void Layer_ContainsOCProperties()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var layer = doc.AddLayer("Layer1");
            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginLayer(layer);
            page.Content.SetFillColor(1, 0, 0);
            page.Content.Rectangle(0, 0, 50, 50);
            page.Content.Fill();
            page.Content.EndLayer();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/OCProperties", text);
            Assert.Contains("/OCGs", text);
        }

        [Fact]
        public void Layer_ContainsBdcEmc()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var layer = doc.AddLayer("Content");
            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginLayer(layer);
            page.Content.SetFillColor(0, 1, 0);
            page.Content.Rectangle(10, 10, 80, 80);
            page.Content.Fill();
            page.Content.EndLayer();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/OC /OC1 BDC", text);
            Assert.Contains("EMC", text);
        }

        [Fact]
        public void Layer_ContainsPropertiesResource()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var layer = doc.AddLayer("Test");
            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginLayer(layer);
            page.Content.SetFillColor(0, 0, 1);
            page.Content.Rectangle(0, 0, 50, 50);
            page.Content.Fill();
            page.Content.EndLayer();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            // Properties dict should map resource name to OCG reference
            Assert.Contains("/Properties", text);
        }

        [Fact]
        public void Layer_NameInPdf()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var layer = doc.AddLayer("My Layer");
            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginLayer(layer);
            page.Content.SetFillColor(1, 0, 0);
            page.Content.Rectangle(0, 0, 50, 50);
            page.Content.Fill();
            page.Content.EndLayer();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Name (My Layer)", text);
        }

        [Fact]
        public void Layer_DefaultVisibleTrue()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var layer = doc.AddLayer("Visible", defaultVisible: true);
            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginLayer(layer);
            page.Content.SetFillColor(1, 0, 0);
            page.Content.Rectangle(0, 0, 50, 50);
            page.Content.Fill();
            page.Content.EndLayer();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/BaseState /ON", text);
            // Should NOT have an OFF array since layer is visible
            Assert.DoesNotContain("/OFF", text);
        }

        [Fact]
        public void Layer_DefaultVisibleFalse()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var layer = doc.AddLayer("Hidden", defaultVisible: false);
            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginLayer(layer);
            page.Content.SetFillColor(1, 0, 0);
            page.Content.Rectangle(0, 0, 50, 50);
            page.Content.Fill();
            page.Content.EndLayer();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/OFF", text);
        }

        [Fact]
        public void MultipleLayers()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var layer1 = doc.AddLayer("Background");
            var layer2 = doc.AddLayer("Text");
            var page = doc.AddPage(PageSize.A4);

            page.Content.BeginLayer(layer1);
            page.Content.SetFillColor(0.9f, 0.9f, 0.9f);
            page.Content.Rectangle(0, 0, 200, 200);
            page.Content.Fill();
            page.Content.EndLayer();

            page.Content.BeginLayer(layer2);
            page.Content.SetFillColor(0, 0, 0);
            page.Content.Rectangle(50, 50, 100, 100);
            page.Content.Fill();
            page.Content.EndLayer();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/OC /OC1 BDC", text);
            Assert.Contains("/OC /OC2 BDC", text);
            Assert.Contains("/Name (Background)", text);
            Assert.Contains("/Name (Text)", text);
        }

        [Fact]
        public void Layer_OrderArray()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var layer = doc.AddLayer("Test");
            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginLayer(layer);
            page.Content.SetFillColor(1, 0, 0);
            page.Content.Rectangle(0, 0, 50, 50);
            page.Content.Fill();
            page.Content.EndLayer();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Order", text);
        }

        [Fact]
        public void Layer_ValidPdf()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var layer = doc.AddLayer("Check");
            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginLayer(layer);
            page.Content.SetFillColor(0.5f, 0.5f, 0.5f);
            page.Content.Rectangle(0, 0, 100, 100);
            page.Content.Fill();
            page.Content.EndLayer();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.StartsWith("%PDF-1.", text);
            Assert.Contains("/Root", text);
        }

        [Fact]
        public void Layer_ReusedAcrossPages()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var layer = doc.AddLayer("Shared");
            var page1 = doc.AddPage(PageSize.A4);
            page1.Content.BeginLayer(layer);
            page1.Content.SetFillColor(1, 0, 0);
            page1.Content.Rectangle(0, 0, 50, 50);
            page1.Content.Fill();
            page1.Content.EndLayer();

            var page2 = doc.AddPage(PageSize.A4);
            page2.Content.BeginLayer(layer);
            page2.Content.SetFillColor(0, 0, 1);
            page2.Content.Rectangle(0, 0, 50, 50);
            page2.Content.Fill();
            page2.Content.EndLayer();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            // Only one OCG object should exist despite being used on both pages
            var ocgTypeCount = CountOccurrences(text, "/Type /OCG");
            Assert.Equal(1, ocgTypeCount);
        }

        [Fact]
        public void Layer_MixedVisibility()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var visibleLayer = doc.AddLayer("Visible", defaultVisible: true);
            var hiddenLayer = doc.AddLayer("Hidden", defaultVisible: false);

            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginLayer(visibleLayer);
            page.Content.SetFillColor(1, 0, 0);
            page.Content.Rectangle(0, 0, 50, 50);
            page.Content.Fill();
            page.Content.EndLayer();

            page.Content.BeginLayer(hiddenLayer);
            page.Content.SetFillColor(0, 0, 1);
            page.Content.Rectangle(50, 50, 50, 50);
            page.Content.Fill();
            page.Content.EndLayer();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/BaseState /ON", text);
            Assert.Contains("/OFF", text);
            Assert.Contains("/Name (Visible)", text);
            Assert.Contains("/Name (Hidden)", text);
        }

        [Fact]
        public void Layer_NoLayersNoOcProperties()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.Content.SetFillColor(0, 0, 0);
            page.Content.Rectangle(0, 0, 50, 50);
            page.Content.Fill();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.DoesNotContain("/OCProperties", text);
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
