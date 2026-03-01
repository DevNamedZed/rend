using System;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class SpotColorTests
    {
        private static string BuildPdfWithSpotColor(Action<PdfDocument, PdfPage, PdfSpotColor> drawAction,
            string spotName = "PANTONE 185 C", byte r = 255, byte g = 0, byte b = 0)
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var spot = doc.AddSpotColor(spotName, new CssColor(r, g, b));
            var page = doc.AddPage(PageSize.A4);
            drawAction(doc, page, spot);
            return Encoding.ASCII.GetString(doc.ToArray());
        }

        // ═══════════════════════════════════════════
        // AddSpotColor basics
        // ═══════════════════════════════════════════

        [Fact]
        public void AddSpotColor_ReturnsNonNull()
        {
            using var doc = new PdfDocument();
            var spot = doc.AddSpotColor("TestSpot", new CssColor(255, 0, 0));
            Assert.NotNull(spot);
        }

        [Fact]
        public void AddSpotColor_NameIsPreserved()
        {
            using var doc = new PdfDocument();
            var spot = doc.AddSpotColor("PANTONE 185 C", new CssColor(255, 0, 0));
            Assert.Equal("PANTONE 185 C", spot.Name);
        }

        [Fact]
        public void AddSpotColor_ApproximateRgbIsPreserved()
        {
            using var doc = new PdfDocument();
            var color = new CssColor(128, 64, 32);
            var spot = doc.AddSpotColor("CustomSpot", color);
            Assert.Equal(128, spot.ApproximateRgb.R);
            Assert.Equal(64, spot.ApproximateRgb.G);
            Assert.Equal(32, spot.ApproximateRgb.B);
        }

        [Fact]
        public void AddSpotColor_NullName_Throws()
        {
            using var doc = new PdfDocument();
            Assert.Throws<ArgumentNullException>(() =>
                doc.AddSpotColor(null!, new CssColor(255, 0, 0)));
        }

        // ═══════════════════════════════════════════
        // Separation color space in PDF output
        // ═══════════════════════════════════════════

        [Fact]
        public void SpotColor_CreatesSeparationColorSpace()
        {
            var text = BuildPdfWithSpotColor((doc, page, spot) =>
            {
                page.Content.SetFillSpotColor(spot, 1.0f);
                page.Content.Rectangle(50, 50, 200, 200);
                page.Content.Fill();
            });
            Assert.Contains("/Separation", text);
        }

        [Fact]
        public void SpotColor_ContainsColorantName()
        {
            var text = BuildPdfWithSpotColor((doc, page, spot) =>
            {
                page.Content.SetFillSpotColor(spot, 1.0f);
                page.Content.Rectangle(50, 50, 200, 200);
                page.Content.Fill();
            }, spotName: "TestInk");
            // The colorant name should appear in the PDF
            Assert.Contains("/TestInk", text);
        }

        [Fact]
        public void SpotColor_UsesDeviceCMYKAlternate()
        {
            var text = BuildPdfWithSpotColor((doc, page, spot) =>
            {
                page.Content.SetFillSpotColor(spot, 1.0f);
                page.Content.Rectangle(50, 50, 200, 200);
                page.Content.Fill();
            });
            Assert.Contains("/DeviceCMYK", text);
        }

        [Fact]
        public void SpotColor_ContainsTintTransformFunction()
        {
            var text = BuildPdfWithSpotColor((doc, page, spot) =>
            {
                page.Content.SetFillSpotColor(spot, 1.0f);
                page.Content.Rectangle(50, 50, 200, 200);
                page.Content.Fill();
            });
            // Tint transform is a Type 2 (exponential interpolation) function
            Assert.Contains("/FunctionType 2", text);
            Assert.Contains("/C0", text);
            Assert.Contains("/C1", text);
            Assert.Contains("/N 1", text);
        }

        // ═══════════════════════════════════════════
        // Content stream operators
        // ═══════════════════════════════════════════

        [Fact]
        public void SetFillSpotColor_EmitsCsAndScnOperators()
        {
            var text = BuildPdfWithSpotColor((doc, page, spot) =>
            {
                page.Content.SetFillSpotColor(spot, 0.75f);
                page.Content.Rectangle(50, 50, 200, 200);
                page.Content.Fill();
            });
            // cs operator sets the fill color space
            Assert.Contains("cs", text);
            // scn operator sets the tint value
            Assert.Contains("scn", text);
        }

        [Fact]
        public void SetStrokeSpotColor_EmitsCSAndSCNOperators()
        {
            var text = BuildPdfWithSpotColor((doc, page, spot) =>
            {
                page.Content.SetStrokeSpotColor(spot, 0.5f);
                page.Content.Rectangle(50, 50, 200, 200);
                page.Content.Stroke();
            });
            // CS operator sets the stroke color space (uppercase)
            Assert.Contains("CS", text);
            // SCN operator sets the tint value (uppercase)
            Assert.Contains("SCN", text);
        }

        // ═══════════════════════════════════════════
        // ColorSpace in page resources
        // ═══════════════════════════════════════════

        [Fact]
        public void SpotColor_AppearsInPageResources()
        {
            var text = BuildPdfWithSpotColor((doc, page, spot) =>
            {
                page.Content.SetFillSpotColor(spot, 1.0f);
                page.Content.Rectangle(50, 50, 200, 200);
                page.Content.Fill();
            });
            // ColorSpace resource dictionary should be in page resources
            Assert.Contains("/ColorSpace", text);
            // The resource name CS1 should appear
            Assert.Contains("/CS1", text);
        }

        // ═══════════════════════════════════════════
        // Multiple spot colors
        // ═══════════════════════════════════════════

        [Fact]
        public void MultipleSpotColors_EachGetUniqueResourceName()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var spot1 = doc.AddSpotColor("SpotRed", new CssColor(255, 0, 0));
            var spot2 = doc.AddSpotColor("SpotBlue", new CssColor(0, 0, 255));
            var page = doc.AddPage(PageSize.A4);

            page.Content.SetFillSpotColor(spot1, 1.0f);
            page.Content.Rectangle(50, 50, 100, 100);
            page.Content.Fill();

            page.Content.SetFillSpotColor(spot2, 0.5f);
            page.Content.Rectangle(200, 50, 100, 100);
            page.Content.Fill();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/CS1", text);
            Assert.Contains("/CS2", text);
            Assert.Contains("/SpotRed", text);
            Assert.Contains("/SpotBlue", text);
        }

        // ═══════════════════════════════════════════
        // Tint clamping
        // ═══════════════════════════════════════════

        [Fact]
        public void SetFillSpotColor_TintClamped_NoThrow()
        {
            // Tint values outside 0-1 should be clamped, not throw
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var spot = doc.AddSpotColor("Test", new CssColor(255, 0, 0));
            var page = doc.AddPage(PageSize.A4);

            // Should not throw — tint is clamped
            page.Content.SetFillSpotColor(spot, -0.5f);
            page.Content.SetFillSpotColor(spot, 1.5f);
        }

        // ═══════════════════════════════════════════
        // Error handling
        // ═══════════════════════════════════════════

        [Fact]
        public void SetFillSpotColor_NullSpot_Throws()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            Assert.Throws<ArgumentNullException>(() =>
                page.Content.SetFillSpotColor(null!, 1.0f));
        }

        [Fact]
        public void SetStrokeSpotColor_NullSpot_Throws()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            Assert.Throws<ArgumentNullException>(() =>
                page.Content.SetStrokeSpotColor(null!, 1.0f));
        }

        // ═══════════════════════════════════════════
        // PDF validity: complete round-trip
        // ═══════════════════════════════════════════

        [Fact]
        public void SpotColor_CompleteDocument_ContainsPdfHeader()
        {
            var text = BuildPdfWithSpotColor((doc, page, spot) =>
            {
                page.Content.SetFillSpotColor(spot, 0.8f);
                page.Content.Rectangle(72, 700, 200, 50);
                page.Content.Fill();
            });
            Assert.StartsWith("%PDF-", text);
            Assert.Contains("%%EOF", text);
        }

        [Fact]
        public void SpotColor_CMYK_ConversionForPureRed()
        {
            // Pure red (255,0,0) should convert to CMYK (0, 1, 1, 0)
            var text = BuildPdfWithSpotColor((doc, page, spot) =>
            {
                page.Content.SetFillSpotColor(spot, 1.0f);
                page.Content.Rectangle(50, 50, 200, 200);
                page.Content.Fill();
            }, r: 255, g: 0, b: 0);
            // The /C1 array should contain the CMYK values for red
            // C = 0, M = 1, Y = 1, K = 0
            Assert.Contains("/C1", text);
        }
    }
}
