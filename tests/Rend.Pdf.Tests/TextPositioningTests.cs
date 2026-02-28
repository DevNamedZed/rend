using System;
using System.IO;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class TextPositioningTests
    {
        private static string BuildPdfText(Action<PdfDocument, PdfPage> drawAction)
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            drawAction(doc, page);
            var bytes = doc.ToArray();
            return Encoding.ASCII.GetString(bytes);
        }

        // ═══════════════════════════════════════════
        // TJ Operator (ShowTextWithPositioning)
        // ═══════════════════════════════════════════

        [Fact]
        public void ShowTextWithPositioning_ProducesTJOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 700);
                page.Content.ShowTextWithPositioning(font,
                    TextPositionEntry.FromText("A"),
                    TextPositionEntry.FromAdjustment(-50),
                    TextPositionEntry.FromText("V"));
                page.Content.EndText();
            });

            Assert.Contains("TJ", text);
        }

        [Fact]
        public void ShowTextWithPositioning_EmptyEntries_DoesNotThrow()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.ShowTextWithPositioning(font);
                page.Content.EndText();
            });

            // Should produce valid PDF without TJ
            Assert.Contains("BT", text);
            Assert.Contains("ET", text);
        }

        [Fact]
        public void ShowTextWithPositioning_TextOnlyEntries_Works()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.ShowTextWithPositioning(font,
                    TextPositionEntry.FromText("Hello"));
                page.Content.EndText();
            });

            Assert.Contains("TJ", text);
        }

        [Fact]
        public void ShowTextWithPositioning_AdjustmentOnlyEntries_Works()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.ShowTextWithPositioning(font,
                    TextPositionEntry.FromText("A"),
                    TextPositionEntry.FromAdjustment(-100));
                page.Content.EndText();
            });

            Assert.Contains("TJ", text);
        }

        [Fact]
        public void ShowTextWithPositioning_TextAndAdjustment_Works()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.ShowTextWithPositioning(font,
                    TextPositionEntry.FromTextAndAdjustment("AB", -50));
                page.Content.EndText();
            });

            Assert.Contains("TJ", text);
        }

        // ═══════════════════════════════════════════
        // TextPositionEntry Struct
        // ═══════════════════════════════════════════

        [Fact]
        public void TextPositionEntry_FromText_HasTextNoAdjustment()
        {
            var entry = TextPositionEntry.FromText("Hello");
            Assert.True(entry.HasText);
            Assert.False(entry.HasAdjustment);
            Assert.Equal("Hello", entry.Text);
            Assert.Equal(0, entry.Adjustment);
        }

        [Fact]
        public void TextPositionEntry_FromAdjustment_HasAdjustmentNoText()
        {
            var entry = TextPositionEntry.FromAdjustment(-100);
            Assert.False(entry.HasText);
            Assert.True(entry.HasAdjustment);
            Assert.Null(entry.Text);
            Assert.Equal(-100, entry.Adjustment);
        }

        [Fact]
        public void TextPositionEntry_FromTextAndAdjustment_HasBoth()
        {
            var entry = TextPositionEntry.FromTextAndAdjustment("AV", -50);
            Assert.True(entry.HasText);
            Assert.True(entry.HasAdjustment);
            Assert.Equal("AV", entry.Text);
            Assert.Equal(-50, entry.Adjustment);
        }

        // ═══════════════════════════════════════════
        // Tz Operator (Text Horizontal Scaling)
        // ═══════════════════════════════════════════

        [Fact]
        public void SetTextHorizontalScaling_ProducesTzOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.SetTextHorizontalScaling(150);
                page.Content.MoveTextPosition(50, 700);
                page.Content.ShowText(font, "Stretched");
                page.Content.EndText();
            });

            Assert.Contains("150 Tz", text);
        }

        [Fact]
        public void SetTextHorizontalScaling_DefaultIs100()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.SetTextHorizontalScaling(100);
                page.Content.MoveTextPosition(50, 700);
                page.Content.ShowText(font, "Normal");
                page.Content.EndText();
            });

            Assert.Contains("100 Tz", text);
        }

        [Fact]
        public void SetTextHorizontalScaling_Condensed()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.SetTextHorizontalScaling(75);
                page.Content.MoveTextPosition(50, 700);
                page.Content.ShowText(font, "Condensed");
                page.Content.EndText();
            });

            Assert.Contains("75 Tz", text);
        }

        // ═══════════════════════════════════════════
        // BT/ET State Validation
        // ═══════════════════════════════════════════

        [Fact]
        public void BeginText_Nested_ThrowsInvalidOperation()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);

            page.Content.BeginText();
            Assert.Throws<InvalidOperationException>(() => page.Content.BeginText());
        }

        [Fact]
        public void EndText_WithoutBeginText_ThrowsInvalidOperation()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);

            Assert.Throws<InvalidOperationException>(() => page.Content.EndText());
        }

        [Fact]
        public void ShowText_OutsideTextObject_ThrowsInvalidOperation()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            Assert.Throws<InvalidOperationException>(() => page.Content.ShowText(font, "Hello"));
        }

        [Fact]
        public void MoveTextPosition_OutsideTextObject_ThrowsInvalidOperation()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);

            Assert.Throws<InvalidOperationException>(() => page.Content.MoveTextPosition(50, 700));
        }

        [Fact]
        public void SetTextMatrix_OutsideTextObject_ThrowsInvalidOperation()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);

            Assert.Throws<InvalidOperationException>(() =>
                page.Content.SetTextMatrix(1, 0, 0, 1, 50, 700));
        }

        [Fact]
        public void NextLine_OutsideTextObject_ThrowsInvalidOperation()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);

            Assert.Throws<InvalidOperationException>(() => page.Content.NextLine());
        }

        [Fact]
        public void ShowTextWithPositioning_OutsideTextObject_ThrowsInvalidOperation()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            Assert.Throws<InvalidOperationException>(() =>
                page.Content.ShowTextWithPositioning(font, TextPositionEntry.FromText("Hello")));
        }

        [Fact]
        public void Build_UnclosedTextObject_ThrowsInvalidOperation()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "Unclosed");

            // Save triggers Build() which should detect the unclosed text object
            Assert.Throws<InvalidOperationException>(() => doc.ToArray());
        }

        [Fact]
        public void BeginEnd_MultipleSequences_Works()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);

                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 700);
                page.Content.ShowText(font, "First");
                page.Content.EndText();

                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 680);
                page.Content.ShowText(font, "Second");
                page.Content.EndText();
            });

            // Should contain two BT/ET pairs
            int btCount = 0;
            int etCount = 0;
            int idx = 0;
            while ((idx = text.IndexOf("BT\n", idx)) >= 0) { btCount++; idx += 3; }
            idx = 0;
            while ((idx = text.IndexOf("ET\n", idx)) >= 0) { etCount++; idx += 3; }
            Assert.Equal(2, btCount);
            Assert.Equal(2, etCount);
        }

        [Fact]
        public void SetFont_OutsideTextObject_DoesNotThrow()
        {
            // SetFont is valid outside BT/ET (text state operators are graphics state)
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.SetFont(font, 12);
                page.Content.BeginText();
                page.Content.MoveTextPosition(50, 700);
                page.Content.ShowText(font, "Hello");
                page.Content.EndText();
            });

            Assert.Contains("Tf", text);
        }

        [Fact]
        public void SetCharacterSpacing_OutsideTextObject_DoesNotThrow()
        {
            // Tc is valid outside BT/ET
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.SetCharacterSpacing(2);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 700);
                page.Content.ShowText(font, "Hello");
                page.Content.EndText();
            });

            Assert.Contains("Tc", text);
        }
    }
}
