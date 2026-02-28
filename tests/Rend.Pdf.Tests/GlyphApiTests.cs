using System;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class GlyphApiTests
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
        // GetGlyphIds
        // ═══════════════════════════════════════════

        [Fact]
        public void GetGlyphIds_AsciiText_ReturnsCorrectCount()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            Span<ushort> glyphIds = stackalloc ushort[10];
            int count = font.GetGlyphIds("Hello".AsSpan(), glyphIds);
            Assert.Equal(5, count);
        }

        [Fact]
        public void GetGlyphIds_EmptyText_ReturnsZero()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            Span<ushort> glyphIds = stackalloc ushort[10];
            int count = font.GetGlyphIds(ReadOnlySpan<char>.Empty, glyphIds);
            Assert.Equal(0, count);
        }

        [Fact]
        public void GetGlyphIds_ReturnsExpectedGlyphValues()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            Span<ushort> glyphIds = stackalloc ushort[3];
            font.GetGlyphIds("ABC".AsSpan(), glyphIds);

            // Standard14 uses code point as glyph ID
            Assert.Equal(65, glyphIds[0]); // A
            Assert.Equal(66, glyphIds[1]); // B
            Assert.Equal(67, glyphIds[2]); // C
        }

        // ═══════════════════════════════════════════
        // GetAdvanceWidths
        // ═══════════════════════════════════════════

        [Fact]
        public void GetAdvanceWidths_ReturnsPositiveWidths()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            ReadOnlySpan<ushort> glyphIds = stackalloc ushort[] { 65, 66, 67 }; // A, B, C
            Span<float> advances = stackalloc float[3];
            font.GetAdvanceWidths(glyphIds, advances);

            Assert.True(advances[0] > 0);
            Assert.True(advances[1] > 0);
            Assert.True(advances[2] > 0);
        }

        [Fact]
        public void GetAdvanceWidths_Courier_AllSame()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Courier);

            ReadOnlySpan<ushort> glyphIds = stackalloc ushort[] { 65, 105, 77 }; // A, i, M
            Span<float> advances = stackalloc float[3];
            font.GetAdvanceWidths(glyphIds, advances);

            Assert.Equal(advances[0], advances[1]);
            Assert.Equal(advances[1], advances[2]);
        }

        [Fact]
        public void GetAdvanceWidths_OutOfRange_ReturnsZero()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            ReadOnlySpan<ushort> glyphIds = stackalloc ushort[] { 60000 };
            Span<float> advances = stackalloc float[1];
            font.GetAdvanceWidths(glyphIds, advances);

            Assert.Equal(0, advances[0]);
        }

        // ═══════════════════════════════════════════
        // ShowGlyphs
        // ═══════════════════════════════════════════

        [Fact]
        public void ShowGlyphs_ProducesTjOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 700);

                ReadOnlySpan<ushort> glyphIds = stackalloc ushort[] { 65, 66, 67 }; // A, B, C
                page.Content.ShowGlyphs(font, glyphIds);
                page.Content.EndText();
            });

            Assert.Contains("Tj", text);
        }

        [Fact]
        public void ShowGlyphs_ProducesHexEncoding()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 700);

                // Glyph ID 65 = 0x0041
                ReadOnlySpan<ushort> glyphIds = stackalloc ushort[] { 65 };
                page.Content.ShowGlyphs(font, glyphIds);
                page.Content.EndText();
            });

            // Should contain hex-encoded glyph: <0041>
            Assert.Contains("0041", text);
        }

        [Fact]
        public void ShowGlyphs_EmptySpan_DoesNotThrow()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.ShowGlyphs(font, ReadOnlySpan<ushort>.Empty);
                page.Content.EndText();
            });

            // Should not contain Tj for empty glyphs
            Assert.DoesNotContain("Tj", text);
        }

        [Fact]
        public void ShowGlyphs_OutsideTextObject_Throws()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            var glyphArray = new ushort[] { 65 };
            Assert.Throws<InvalidOperationException>(() =>
                page.Content.ShowGlyphs(font, glyphArray.AsSpan()));
        }

        // ═══════════════════════════════════════════
        // ShowGlyphsWithPositioning
        // ═══════════════════════════════════════════

        [Fact]
        public void ShowGlyphsWithPositioning_ProducesTJOperator()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.MoveTextPosition(50, 700);

                ReadOnlySpan<GlyphPosition> glyphs = stackalloc GlyphPosition[]
                {
                    new GlyphPosition(65, 0),     // A
                    new GlyphPosition(86, -50),    // V with -50 adjustment
                };
                page.Content.ShowGlyphsWithPositioning(font, glyphs);
                page.Content.EndText();
            });

            Assert.Contains("TJ", text);
        }

        [Fact]
        public void ShowGlyphsWithPositioning_EmptySpan_DoesNotThrow()
        {
            var text = BuildPdfText((doc, page) =>
            {
                var font = doc.GetStandardFont(StandardFont.Helvetica);
                page.Content.BeginText();
                page.Content.SetFont(font, 12);
                page.Content.ShowGlyphsWithPositioning(font, ReadOnlySpan<GlyphPosition>.Empty);
                page.Content.EndText();
            });

            Assert.DoesNotContain("TJ", text);
        }

        [Fact]
        public void ShowGlyphsWithPositioning_OutsideTextObject_Throws()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            var glyphArray = new GlyphPosition[] { new GlyphPosition(65, 0) };
            Assert.Throws<InvalidOperationException>(() =>
                page.Content.ShowGlyphsWithPositioning(font, glyphArray.AsSpan()));
        }

        // ═══════════════════════════════════════════
        // GlyphPosition Struct
        // ═══════════════════════════════════════════

        [Fact]
        public void GlyphPosition_DefaultAdjustment_IsZero()
        {
            var gp = new GlyphPosition(65);
            Assert.Equal(65, gp.GlyphId);
            Assert.Equal(0, gp.XAdvanceAdjustment);
        }

        [Fact]
        public void GlyphPosition_WithAdjustment_Stores()
        {
            var gp = new GlyphPosition(100, -75.5f);
            Assert.Equal(100, gp.GlyphId);
            Assert.Equal(-75.5f, gp.XAdvanceAdjustment);
        }
    }
}
