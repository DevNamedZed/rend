using System;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class KerningTests
    {
        // ═══════════════════════════════════════════
        // Standard14 Kerning
        // ═══════════════════════════════════════════

        [Fact]
        public void Standard14Font_HasNoKerning()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            Assert.False(font.HasKerning);
        }

        [Fact]
        public void Standard14Font_GetKerning_ReturnsZero()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            // Any pair should return 0 for Standard14
            float kerning = font.GetKerning(65, 86); // A, V
            Assert.Equal(0, kerning);
        }

        [Theory]
        [InlineData(StandardFont.Helvetica)]
        [InlineData(StandardFont.HelveticaBold)]
        [InlineData(StandardFont.TimesRoman)]
        [InlineData(StandardFont.Courier)]
        [InlineData(StandardFont.Symbol)]
        [InlineData(StandardFont.ZapfDingbats)]
        public void AllStandard14Fonts_GetKerning_ReturnsZero(StandardFont sf)
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(sf);

            // Standard 14 fonts have no embedded kerning data
            Assert.Equal(0f, font.GetKerning(0, 0));
            Assert.Equal(0f, font.GetKerning(65, 86));
            Assert.Equal(0f, font.GetKerning(1000, 2000));
        }

        [Fact]
        public void GetKerning_ZeroGlyphIds_ReturnsZero()
        {
            using var doc = new PdfDocument();
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            Assert.Equal(0f, font.GetKerning(0, 0));
        }

        [Fact]
        public void HasKerning_Standard14_IsFalse()
        {
            using var doc = new PdfDocument();
            foreach (StandardFont sf in Enum.GetValues(typeof(StandardFont)))
            {
                var font = doc.GetStandardFont(sf);
                Assert.False(font.HasKerning, $"{sf} should not have kerning data");
            }
        }
    }
}
