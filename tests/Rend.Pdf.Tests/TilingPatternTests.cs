using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class TilingPatternTests
    {
        private static string GenerateWithPattern(float width, float height,
            PdfPatternPaintType paintType = PdfPatternPaintType.Colored,
            PdfTilingType tilingType = PdfTilingType.ConstantSpacing)
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var pattern = doc.CreateTilingPattern(width, height, paintType: paintType, tilingType: tilingType);
            // Draw a red rectangle in the pattern cell
            pattern.Content.SetFillColor(1, 0, 0);
            pattern.Content.Rectangle(0, 0, width, height);
            pattern.Content.Fill();

            var page = doc.AddPage(PageSize.A4);
            page.Content.SetFillPattern(pattern);
            page.Content.Rectangle(50, 50, 200, 200);
            page.Content.Fill();

            return Encoding.ASCII.GetString(doc.ToArray());
        }

        [Fact]
        public void Pattern_ContainsPatternType1()
        {
            var text = GenerateWithPattern(20, 20);
            Assert.Contains("/PatternType 1", text);
        }

        [Fact]
        public void Pattern_ContainsPaintType1_Colored()
        {
            var text = GenerateWithPattern(20, 20, PdfPatternPaintType.Colored);
            Assert.Contains("/PaintType 1", text);
        }

        [Fact]
        public void Pattern_ContainsPaintType2_Uncolored()
        {
            var text = GenerateWithPattern(20, 20, PdfPatternPaintType.Uncolored);
            Assert.Contains("/PaintType 2", text);
        }

        [Fact]
        public void Pattern_ContainsTilingType()
        {
            var text = GenerateWithPattern(20, 20, tilingType: PdfTilingType.NoDistortion);
            Assert.Contains("/TilingType 2", text);
        }

        [Fact]
        public void Pattern_ContainsBBox()
        {
            var text = GenerateWithPattern(20, 20);
            Assert.Contains("/BBox", text);
        }

        [Fact]
        public void Pattern_ContainsXStepYStep()
        {
            var text = GenerateWithPattern(20, 20);
            Assert.Contains("/XStep", text);
            Assert.Contains("/YStep", text);
        }

        [Fact]
        public void Pattern_ContentStreamHasFillColor()
        {
            var text = GenerateWithPattern(20, 20);
            // Pattern content contains the red fill and rectangle operator
            Assert.Contains("1 0 0 rg", text);
            Assert.Contains("re", text);
        }

        [Fact]
        public void Pattern_PageUsesPatternColorSpace()
        {
            var text = GenerateWithPattern(20, 20);
            // The page content stream uses /Pattern cs and /P1 scn
            Assert.Contains("/Pattern cs", text);
            Assert.Contains("/P1 scn", text);
        }

        [Fact]
        public void Pattern_ResourceDictContainsPattern()
        {
            var text = GenerateWithPattern(20, 20);
            // Page resources should have /Pattern dictionary with /P1 reference
            Assert.Contains("/Pattern", text);
        }

        [Fact]
        public void Pattern_ValidPdf()
        {
            var text = GenerateWithPattern(20, 20);
            Assert.StartsWith("%PDF-1.", text);
            Assert.Contains("/Root", text);
        }

        [Fact]
        public void Pattern_CustomXStepYStep()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var pattern = doc.CreateTilingPattern(10, 10, xStep: 15, yStep: 15);
            pattern.Content.SetFillColor(0, 0, 1);
            pattern.Content.Rectangle(0, 0, 10, 10);
            pattern.Content.Fill();

            var page = doc.AddPage(PageSize.A4);
            page.Content.SetFillPattern(pattern);
            page.Content.Rectangle(0, 0, 100, 100);
            page.Content.Fill();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/XStep 15", text);
            Assert.Contains("/YStep 15", text);
        }

        [Fact]
        public void Pattern_StrokePattern()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var pattern = doc.CreateTilingPattern(5, 5);
            pattern.Content.SetStrokeColor(0, 1, 0);
            pattern.Content.MoveTo(0, 0);
            pattern.Content.LineTo(5, 5);
            pattern.Content.Stroke();

            var page = doc.AddPage(PageSize.A4);
            page.Content.SetStrokePattern(pattern);
            page.Content.Rectangle(50, 50, 200, 200);
            page.Content.Stroke();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Pattern CS", text);
            Assert.Contains("/P1 SCN", text);
        }

        [Fact]
        public void Pattern_MultiplePatterns()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var pat1 = doc.CreateTilingPattern(10, 10);
            pat1.Content.SetFillColor(1, 0, 0);
            pat1.Content.Rectangle(0, 0, 10, 10);
            pat1.Content.Fill();

            var pat2 = doc.CreateTilingPattern(20, 20);
            pat2.Content.SetFillColor(0, 0, 1);
            pat2.Content.Rectangle(0, 0, 20, 20);
            pat2.Content.Fill();

            var page = doc.AddPage(PageSize.A4);
            page.Content.SetFillPattern(pat1);
            page.Content.Rectangle(0, 0, 200, 200);
            page.Content.Fill();

            page.Content.SetFillPattern(pat2);
            page.Content.Rectangle(200, 0, 200, 200);
            page.Content.Fill();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/P1 scn", text);
            Assert.Contains("/P2 scn", text);
        }

        [Fact]
        public void Pattern_SamePatternReused()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var pattern = doc.CreateTilingPattern(10, 10);
            pattern.Content.SetFillColor(1, 0, 0);
            pattern.Content.Rectangle(0, 0, 10, 10);
            pattern.Content.Fill();

            var page = doc.AddPage(PageSize.A4);
            // Use the same pattern twice — should not duplicate the object
            page.Content.SetFillPattern(pattern);
            page.Content.Rectangle(0, 0, 100, 100);
            page.Content.Fill();

            page.Content.SetFillPattern(pattern);
            page.Content.Rectangle(200, 200, 100, 100);
            page.Content.Fill();

            var text = Encoding.ASCII.GetString(doc.ToArray());
            // Pattern should appear only once in resources
            var patternType1Count = CountOccurrences(text, "/PatternType 1");
            Assert.Equal(1, patternType1Count);
        }

        [Fact]
        public void Pattern_TilingTypeConstantSpacingFaster()
        {
            var text = GenerateWithPattern(20, 20, tilingType: PdfTilingType.ConstantSpacingFaster);
            Assert.Contains("/TilingType 3", text);
        }

        [Fact]
        public void Pattern_DefaultXStepEqualsWidth()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            // xStep/yStep default to 0, which means they should be set to width/height
            var pattern = doc.CreateTilingPattern(25, 30);
            Assert.Equal(25f, pattern.XStep);
            Assert.Equal(30f, pattern.YStep);
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
