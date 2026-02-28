using System;
using System.IO;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class PdfContentStreamTests
    {
        /// <summary>
        /// Helper: create a document and page with no compression, return the PDF text output.
        /// </summary>
        private static string BuildPdfText(Action<PdfDocument, PdfPage> drawAction)
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            drawAction(doc, page);
            var bytes = doc.ToArray();
            return Encoding.ASCII.GetString(bytes);
        }

        // ═══════════════════════════════════════════
        // Rectangle and Path Operations
        // ═══════════════════════════════════════════

        [Fact]
        public void Rectangle_AppearsInContentStream()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.Rectangle(10, 20, 100, 50);
                page.Content.Stroke();
            });

            // The content stream should contain the "re" operator
            Assert.Contains("re", text);
            Assert.Contains("S", text);
        }

        [Fact]
        public void Rectangle_WithRectF_AppearsInContentStream()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.Rectangle(new RectF(10, 20, 100, 50));
                page.Content.Fill();
            });

            Assert.Contains("re", text);
            Assert.Contains("f", text);
        }

        [Fact]
        public void Fill_AppearsInContentStream()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Fill();
            });

            Assert.Contains("f\n", text);
        }

        [Fact]
        public void Stroke_AppearsInContentStream()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Stroke();
            });

            Assert.Contains("S\n", text);
        }

        [Fact]
        public void FillAndStroke_AppearsInContentStream()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.FillAndStroke();
            });

            Assert.Contains("B\n", text);
        }

        [Fact]
        public void FillEvenOdd_AppearsInContentStream()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.FillEvenOdd();
            });

            Assert.Contains("f*", text);
        }

        [Fact]
        public void CloseAndStroke_AppearsInContentStream()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.MoveTo(0, 0);
                page.Content.LineTo(100, 100);
                page.Content.CloseAndStroke();
            });

            Assert.Contains("s\n", text);
        }

        // ═══════════════════════════════════════════
        // Path Construction
        // ═══════════════════════════════════════════

        [Fact]
        public void MoveTo_LineTo_ProducesPathOperators()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.MoveTo(10, 20);
                page.Content.LineTo(100, 200);
                page.Content.Stroke();
            });

            Assert.Contains(" m\n", text);
            Assert.Contains(" l\n", text);
        }

        [Fact]
        public void CurveTo_ProducesCurveOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.MoveTo(0, 0);
                page.Content.CurveTo(10, 20, 30, 40, 50, 60);
                page.Content.Stroke();
            });

            Assert.Contains(" c\n", text);
        }

        [Fact]
        public void ClosePath_ProducesCloseOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.MoveTo(0, 0);
                page.Content.LineTo(100, 0);
                page.Content.LineTo(100, 100);
                page.Content.ClosePath();
                page.Content.Stroke();
            });

            Assert.Contains("h\n", text);
        }

        [Fact]
        public void RoundedRectangle_ProducesPathOperators()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.RoundedRectangle(10, 20, 200, 100, 10, 10);
                page.Content.Stroke();
            });

            // Should contain curve operators for rounded corners
            Assert.Contains(" c\n", text);
            Assert.Contains(" m\n", text);
            Assert.Contains("h\n", text);
        }

        [Fact]
        public void EndPath_ProducesEndPathOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.EndPath();
            });

            Assert.Contains("n\n", text);
        }

        // ═══════════════════════════════════════════
        // Color Operations
        // ═══════════════════════════════════════════

        [Fact]
        public void SetFillColor_Rgb_ProducesRgOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SetFillColor(1.0f, 0.0f, 0.0f);
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Fill();
            });

            Assert.Contains("rg", text);
        }

        [Fact]
        public void SetStrokeColor_Rgb_ProducesRGOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SetStrokeColor(0.0f, 0.0f, 1.0f);
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Stroke();
            });

            Assert.Contains("RG", text);
        }

        [Fact]
        public void SetFillColorGray_ProducesGrayOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SetFillColorGray(0.5f);
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Fill();
            });

            Assert.Contains(" g\n", text);
        }

        [Fact]
        public void SetStrokeColorGray_ProducesGrayOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SetStrokeColorGray(0.5f);
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Stroke();
            });

            Assert.Contains(" G\n", text);
        }

        [Fact]
        public void SetFillColorCmyk_ProducesCmykOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SetFillColorCmyk(1.0f, 0.0f, 0.0f, 0.0f);
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Fill();
            });

            Assert.Contains(" k\n", text);
        }

        [Fact]
        public void SetStrokeColorCmyk_ProducesCmykOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SetStrokeColorCmyk(0.0f, 1.0f, 0.0f, 0.0f);
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Stroke();
            });

            Assert.Contains(" K\n", text);
        }

        // ═══════════════════════════════════════════
        // Graphics State
        // ═══════════════════════════════════════════

        [Fact]
        public void SaveState_RestoreState_ProducesQOperators()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SaveState();
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Fill();
                page.Content.RestoreState();
            });

            Assert.Contains("q\n", text);
            Assert.Contains("Q\n", text);
        }

        [Fact]
        public void RestoreState_WithoutSaveState_Throws()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);

            Assert.Throws<InvalidOperationException>(() =>
            {
                page.Content.RestoreState();
            });
        }

        [Fact]
        public void UnbalancedSaveState_ThrowsOnSave()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.Content.SaveState();
            // Not calling RestoreState - should throw on Save()

            Assert.Throws<InvalidOperationException>(() =>
            {
                doc.ToArray();
            });
        }

        [Fact]
        public void NestedSaveRestore_Works()
        {
            // Should not throw
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SaveState();
                page.Content.SaveState();
                page.Content.Rectangle(0, 0, 50, 50);
                page.Content.Fill();
                page.Content.RestoreState();
                page.Content.RestoreState();
            });

            Assert.Contains("q\n", text);
            Assert.Contains("Q\n", text);
        }

        [Fact]
        public void SetLineWidth_AppearsInContentStream()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SetLineWidth(2.5f);
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Stroke();
            });

            Assert.Contains(" w\n", text);
        }

        [Fact]
        public void SetLineCap_AppearsInContentStream()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SetLineCap(LineCapStyle.Round);
                page.Content.MoveTo(0, 0);
                page.Content.LineTo(100, 100);
                page.Content.Stroke();
            });

            Assert.Contains(" J\n", text);
        }

        [Fact]
        public void SetLineJoin_AppearsInContentStream()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SetLineJoin(LineJoinStyle.Bevel);
                page.Content.MoveTo(0, 0);
                page.Content.LineTo(100, 0);
                page.Content.LineTo(100, 100);
                page.Content.Stroke();
            });

            Assert.Contains(" j\n", text);
        }

        [Fact]
        public void SetTransform_ProducesCmOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SaveState();
                page.Content.SetTransform(1, 0, 0, 1, 100, 200);
                page.Content.Rectangle(0, 0, 50, 50);
                page.Content.Fill();
                page.Content.RestoreState();
            });

            Assert.Contains(" cm\n", text);
        }

        [Fact]
        public void Translate_ProducesCmOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SaveState();
                page.Content.Translate(100, 200);
                page.Content.Rectangle(0, 0, 50, 50);
                page.Content.Fill();
                page.Content.RestoreState();
            });

            Assert.Contains(" cm\n", text);
        }

        [Fact]
        public void Scale_ProducesCmOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SaveState();
                page.Content.Scale(2, 2);
                page.Content.Rectangle(0, 0, 50, 50);
                page.Content.Fill();
                page.Content.RestoreState();
            });

            Assert.Contains(" cm\n", text);
        }

        [Fact]
        public void Rotate_ProducesCmOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SaveState();
                page.Content.Rotate(45);
                page.Content.Rectangle(0, 0, 50, 50);
                page.Content.Fill();
                page.Content.RestoreState();
            });

            Assert.Contains(" cm\n", text);
        }

        [Fact]
        public void Clip_ProducesClipOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SaveState();
                page.Content.Rectangle(10, 10, 200, 200);
                page.Content.Clip();
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Fill();
                page.Content.RestoreState();
            });

            Assert.Contains("W n", text);
        }

        [Fact]
        public void ClipEvenOdd_ProducesClipOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SaveState();
                page.Content.Rectangle(10, 10, 200, 200);
                page.Content.ClipEvenOdd();
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Fill();
                page.Content.RestoreState();
            });

            Assert.Contains("W* n", text);
        }

        [Fact]
        public void SetDashPattern_ProducesDashOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SetDashPattern(new float[] { 5, 3 }, 0);
                page.Content.MoveTo(0, 0);
                page.Content.LineTo(100, 100);
                page.Content.Stroke();
            });

            Assert.Contains(" d\n", text);
        }

        [Fact]
        public void SetMiterLimit_ProducesMiterOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SetMiterLimit(10);
                page.Content.MoveTo(0, 0);
                page.Content.LineTo(100, 0);
                page.Content.LineTo(100, 100);
                page.Content.Stroke();
            });

            Assert.Contains(" M\n", text);
        }

        // ═══════════════════════════════════════════
        // Text Operations
        // ═══════════════════════════════════════════

        [Fact]
        public void BeginText_EndText_ProducesTextOperators()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.BeginText();
                page.Content.EndText();
            });

            Assert.Contains("BT\n", text);
            Assert.Contains("ET\n", text);
        }

        [Fact]
        public void SetFont_ProducesTfOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.ShowText(font, "Test");
                page.Content.EndText();
            });

            Assert.Contains("Tf", text);
        }

        [Fact]
        public void ShowText_Standard14_ProducesLiteralString()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 750);
                page.Content.ShowText(font, "Hello World");
                page.Content.EndText();
            });

            // Standard 14 uses literal string encoding: (Hello World) Tj
            Assert.Contains("(Hello World) Tj", text);
        }

        [Fact]
        public void MoveTextPosition_ProducesTdOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.BeginText();
                page.Content.MoveTextPosition(100, 200);
                page.Content.EndText();
            });

            Assert.Contains("Td", text);
        }

        [Fact]
        public void SetTextMatrix_ProducesTmOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.BeginText();
                page.Content.SetTextMatrix(1, 0, 0, 1, 100, 200);
                page.Content.EndText();
            });

            Assert.Contains("Tm", text);
        }

        [Fact]
        public void SetCharacterSpacing_ProducesTcOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.BeginText();
                page.Content.SetCharacterSpacing(2.0f);
                page.Content.EndText();
            });

            Assert.Contains("Tc", text);
        }

        [Fact]
        public void SetWordSpacing_ProducesTwOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.BeginText();
                page.Content.SetWordSpacing(5.0f);
                page.Content.EndText();
            });

            Assert.Contains("Tw", text);
        }

        [Fact]
        public void SetTextLeading_ProducesTLOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.BeginText();
                page.Content.SetTextLeading(14.0f);
                page.Content.EndText();
            });

            Assert.Contains("TL", text);
        }

        [Fact]
        public void SetTextRise_ProducesTsOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.BeginText();
                page.Content.SetTextRise(5.0f);
                page.Content.EndText();
            });

            Assert.Contains("Ts", text);
        }

        [Fact]
        public void SetTextRenderingMode_ProducesTrOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.BeginText();
                page.Content.SetTextRenderingMode(TextRenderingMode.FillAndStroke);
                page.Content.EndText();
            });

            Assert.Contains("Tr", text);
        }

        [Fact]
        public void NextLine_ProducesNextLineOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.BeginText();
                page.Content.NextLine();
                page.Content.EndText();
            });

            Assert.Contains("T*", text);
        }

        [Fact]
        public void ShowText_EscapesSpecialCharacters()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 750);
                page.Content.ShowText(font, "Test (with) parens");
                page.Content.EndText();
            });

            // Parentheses should be escaped with backslash
            Assert.Contains("\\(", text);
            Assert.Contains("\\)", text);
        }

        [Fact]
        public void ShowText_BackslashIsEscaped()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 750);
                page.Content.ShowText(font, "path\\to\\file");
                page.Content.EndText();
            });

            Assert.Contains("\\\\", text);
        }

        // ═══════════════════════════════════════════
        // Opacity
        // ═══════════════════════════════════════════

        [Fact]
        public void SetFillOpacity_CreatesExtGState()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SaveState();
                page.Content.SetFillOpacity(0.5f);
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Fill();
                page.Content.RestoreState();
            });

            Assert.Contains("gs", text);
            Assert.Contains("/ExtGState", text);
        }

        [Fact]
        public void SetStrokeOpacity_CreatesExtGState()
        {
            var text = BuildPdfText((doc, page) =>
            {
                page.Content.SaveState();
                page.Content.SetStrokeOpacity(0.75f);
                page.Content.Rectangle(0, 0, 100, 100);
                page.Content.Stroke();
                page.Content.RestoreState();
            });

            Assert.Contains("gs", text);
            Assert.Contains("/ExtGState", text);
        }

        // ═══════════════════════════════════════════
        // Combined Operations
        // ═══════════════════════════════════════════

        [Fact]
        public void ComplexDrawing_ProducesValidPdf()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            var page = doc.AddPage(PageSize.A4);

            // Draw a colored rectangle
            page.Content.SaveState();
            page.Content.SetFillColor(1.0f, 0.0f, 0.0f);
            page.Content.Rectangle(50, 700, 200, 100);
            page.Content.Fill();
            page.Content.RestoreState();

            // Draw stroked rectangle
            page.Content.SaveState();
            page.Content.SetStrokeColor(0.0f, 0.0f, 1.0f);
            page.Content.SetLineWidth(2.0f);
            page.Content.Rectangle(50, 550, 200, 100);
            page.Content.Stroke();
            page.Content.RestoreState();

            // Draw text
            page.Content.BeginText();
            page.Content.SetFont(font, 24);
            page.Content.MoveTextPosition(50, 500);
            page.Content.ShowText(font, "Hello, World!");
            page.Content.EndText();

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            // Verify it's a valid PDF
            Assert.StartsWith("%PDF-", text);
            Assert.Contains("%%EOF", text);

            // Verify operators are present
            Assert.Contains("rg", text);
            Assert.Contains("RG", text);
            Assert.Contains("re", text);
            Assert.Contains("BT", text);
            Assert.Contains("ET", text);
            Assert.Contains("Tj", text);
        }

        [Fact]
        public void MultipleTextBlocks_ProducesValidPdf()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var helvetica = doc.GetStandardFont(StandardFont.Helvetica);
            var courier = doc.GetStandardFont(StandardFont.Courier);
            var page = doc.AddPage(PageSize.A4);

            page.Content.BeginText();
            page.Content.SetFont(helvetica, 12);
            page.Content.MoveTextPosition(50, 750);
            page.Content.ShowText(helvetica, "Helvetica text");
            page.Content.EndText();

            page.Content.BeginText();
            page.Content.SetFont(courier, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(courier, "Courier text");
            page.Content.EndText();

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.Contains("(Helvetica text) Tj", text);
            Assert.Contains("(Courier text) Tj", text);
        }
    }
}
