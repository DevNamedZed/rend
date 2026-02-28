using System;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class PdfFontTests
    {
        // ═══════════════════════════════════════════
        // Standard Font Properties
        // ═══════════════════════════════════════════

        [Theory]
        [InlineData(StandardFont.Helvetica, "Helvetica")]
        [InlineData(StandardFont.HelveticaBold, "Helvetica-Bold")]
        [InlineData(StandardFont.HelveticaOblique, "Helvetica-Oblique")]
        [InlineData(StandardFont.HelveticaBoldOblique, "Helvetica-BoldOblique")]
        [InlineData(StandardFont.TimesRoman, "Times-Roman")]
        [InlineData(StandardFont.TimesBold, "Times-Bold")]
        [InlineData(StandardFont.TimesItalic, "Times-Italic")]
        [InlineData(StandardFont.TimesBoldItalic, "Times-BoldItalic")]
        [InlineData(StandardFont.Courier, "Courier")]
        [InlineData(StandardFont.CourierBold, "Courier-Bold")]
        [InlineData(StandardFont.CourierOblique, "Courier-Oblique")]
        [InlineData(StandardFont.CourierBoldOblique, "Courier-BoldOblique")]
        [InlineData(StandardFont.Symbol, "Symbol")]
        [InlineData(StandardFont.ZapfDingbats, "ZapfDingbats")]
        public void StandardFont_HasCorrectBaseFont(StandardFont standardFont, string expectedName)
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(standardFont);
            Assert.Equal(expectedName, font.BaseFont);
        }

        [Fact]
        public void StandardFont_IsStandard14_ReturnsTrue()
        {
            using var doc = new PdfDocument();
            foreach (StandardFont sf in Enum.GetValues(typeof(StandardFont)))
            {
                var font = doc.GetStandardFont(sf);
                Assert.True(font.IsStandard14, $"{sf} should be marked as Standard 14");
            }
        }

        // ═══════════════════════════════════════════
        // Font Metrics
        // ═══════════════════════════════════════════

        [Fact]
        public void Helvetica_HasExpectedMetrics()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            Assert.Equal(718, font.Metrics.Ascent);
            Assert.Equal(-207, font.Metrics.Descent);
            Assert.Equal(718, font.Metrics.CapHeight);
            Assert.Equal(0, font.Metrics.ItalicAngle);
            Assert.Equal(1000, font.Metrics.UnitsPerEm);
        }

        [Fact]
        public void TimesRoman_HasExpectedMetrics()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.TimesRoman);

            Assert.Equal(683, font.Metrics.Ascent);
            Assert.Equal(-217, font.Metrics.Descent);
            Assert.Equal(662, font.Metrics.CapHeight);
            Assert.Equal(0, font.Metrics.ItalicAngle);
            Assert.Equal(1000, font.Metrics.UnitsPerEm);
        }

        [Fact]
        public void Courier_HasExpectedMetrics()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Courier);

            Assert.Equal(629, font.Metrics.Ascent);
            Assert.Equal(-157, font.Metrics.Descent);
            Assert.Equal(562, font.Metrics.CapHeight);
            Assert.Equal(1000, font.Metrics.UnitsPerEm);
        }

        [Fact]
        public void Oblique_HasNonZeroItalicAngle()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.HelveticaOblique);
            Assert.NotEqual(0, font.Metrics.ItalicAngle);
            Assert.Equal(-12, font.Metrics.ItalicAngle);
        }

        [Fact]
        public void TimesItalic_HasNonZeroItalicAngle()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.TimesItalic);
            Assert.Equal(-15.5f, font.Metrics.ItalicAngle);
        }

        [Fact]
        public void BoldFont_HasHigherStemV()
        {
            using var doc = new PdfDocument();
            var regular = doc.GetStandardFont(StandardFont.Helvetica);
            var bold = doc.GetStandardFont(StandardFont.HelveticaBold);

            Assert.True(bold.Metrics.StemV > regular.Metrics.StemV,
                $"Bold StemV ({bold.Metrics.StemV}) should be greater than regular ({regular.Metrics.StemV})");
        }

        // ═══════════════════════════════════════════
        // Glyph ID Lookup
        // ═══════════════════════════════════════════

        [Fact]
        public void GetGlyphId_AsciiCharacter_ReturnsCorrectId()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            // For Standard 14 fonts, code point = glyph index (WinAnsiEncoding)
            ushort glyphA = font.GetGlyphId('A');
            Assert.Equal(65, glyphA);

            ushort glypha = font.GetGlyphId('a');
            Assert.Equal(97, glypha);
        }

        [Fact]
        public void GetGlyphId_Space_Returns32()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            ushort glyphSpace = font.GetGlyphId(' ');
            Assert.Equal(32, glyphSpace);
        }

        [Fact]
        public void GetGlyphId_OutOfRange_ReturnsZero()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            // Standard 14 fonts only have 256 entries
            ushort glyph = font.GetGlyphId(0x4E2D); // Chinese character - not in standard font
            Assert.Equal(0, glyph);
        }

        // ═══════════════════════════════════════════
        // Width Measurement
        // ═══════════════════════════════════════════

        [Fact]
        public void MeasureWidth_EmptyString_ReturnsZero()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            float width = font.MeasureWidth("", 12);
            Assert.Equal(0, width);
        }

        [Fact]
        public void MeasureWidth_ReturnsPositiveValue()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            float width = font.MeasureWidth("Hello", 12);
            Assert.True(width > 0, "Width should be positive for non-empty text");
        }

        [Fact]
        public void MeasureWidth_LargerFontSize_ReturnsLargerWidth()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            float width12 = font.MeasureWidth("Test", 12);
            float width24 = font.MeasureWidth("Test", 24);
            Assert.True(width24 > width12, "Larger font size should produce larger width");
            // Should be exactly double
            Assert.Equal(width12 * 2, width24, 2);
        }

        [Fact]
        public void MeasureWidth_Courier_IsMonospaced()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Courier);

            float widthI = font.MeasureWidth("i", 12);
            float widthM = font.MeasureWidth("m", 12);
            float widthW = font.MeasureWidth("W", 12);

            // Courier is monospaced, so all characters should have the same width
            Assert.Equal(widthI, widthM, 2);
            Assert.Equal(widthI, widthW, 2);
        }

        [Fact]
        public void MeasureWidth_Helvetica_IsProportional()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            float widthI = font.MeasureWidth("i", 12);
            float widthM = font.MeasureWidth("M", 12);

            // In a proportional font, 'i' should be narrower than 'M'
            Assert.True(widthI < widthM, "'i' should be narrower than 'M' in Helvetica");
        }

        [Fact]
        public void MeasureWidth_LongerString_ReturnsLargerWidth()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            float shortWidth = font.MeasureWidth("AB", 12);
            float longWidth = font.MeasureWidth("ABCD", 12);

            Assert.True(longWidth > shortWidth);
        }

        [Fact]
        public void MeasureWidth_Space_HasWidth()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            float width = font.MeasureWidth(" ", 12);
            Assert.True(width > 0, "Space character should have positive width");
        }

        // ═══════════════════════════════════════════
        // GetAdvanceWidth
        // ═══════════════════════════════════════════

        [Fact]
        public void GetAdvanceWidth_ValidGlyph_ReturnsPositiveWidth()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            float width = font.GetAdvanceWidth(65); // 'A'
            Assert.True(width > 0);
        }

        [Fact]
        public void GetAdvanceWidth_Courier_AllSameWidth()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Courier);

            // All Courier glyphs are 600 units wide
            for (ushort i = 32; i < 127; i++)
            {
                float width = font.GetAdvanceWidth(i);
                Assert.Equal(600, width);
            }
        }

        [Fact]
        public void GetAdvanceWidth_OutOfRange_ReturnsZero()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            // Glyph ID well beyond the array
            float width = font.GetAdvanceWidth(60000);
            Assert.Equal(0, width);
        }

        // ═══════════════════════════════════════════
        // Encoding
        // ═══════════════════════════════════════════

        [Fact]
        public void Encode_AsciiText_ProducesTwoBytePerChar()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            // Standard 14 fonts use ShowTextLiteral (not Encode), but Encode still works
            byte[] encoded = font.Encode("AB");
            // 2 characters * 2 bytes = 4 bytes
            Assert.Equal(4, encoded.Length);
        }

        [Fact]
        public void Encode_EmptyString_ProducesEmptyArray()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            byte[] encoded = font.Encode("");
            Assert.Empty(encoded);
        }

        [Fact]
        public void Encode_SingleChar_ProducesTwoBytes()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            byte[] encoded = font.Encode("A");
            Assert.Equal(2, encoded.Length);
            // 'A' = glyph ID 65 = 0x0041 in big-endian
            Assert.Equal(0x00, encoded[0]);
            Assert.Equal(0x41, encoded[1]);
        }
    }
}
