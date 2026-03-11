using Rend.Fonts;
using Xunit;

namespace Rend.Tests.Fonts
{
    public class FontMetricsInfoTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            var metrics = new FontMetricsInfo(
                ascent: 800,
                descent: -200,
                lineGap: 90,
                unitsPerEm: 1000,
                capHeight: 700,
                xHeight: 500);

            Assert.Equal(800, metrics.Ascent);
            Assert.Equal(-200, metrics.Descent);
            Assert.Equal(90, metrics.LineGap);
            Assert.Equal(1000, metrics.UnitsPerEm);
            Assert.Equal(700, metrics.CapHeight);
            Assert.Equal(500, metrics.XHeight);
        }

        [Fact]
        public void GetLineHeight_ComputesCorrectly()
        {
            // Chrome rounds each component individually (lroundf):
            // ascent = round(16 * 800 / 1000) = round(12.8) = 13
            // descent = round(16 * 200 / 1000) = round(3.2) = 3
            // lineGap = round(16 * 90 / 1000) = round(1.44) = 1
            // total = 13 + 3 + 1 = 17
            var metrics = new FontMetricsInfo(800, -200, 90, 1000, 700, 500);
            float lineHeight = metrics.GetLineHeight(16f);

            Assert.Equal(17f, lineHeight, 2);
        }

        [Fact]
        public void GetLineHeight_WithZeroLineGap()
        {
            // LineHeight = 16 * (800 - (-200) + 0) / 1000 = 16 * 1000/1000 = 16
            var metrics = new FontMetricsInfo(800, -200, 0, 1000, 700, 500);
            float lineHeight = metrics.GetLineHeight(16f);

            Assert.Equal(16f, lineHeight, 2);
        }

        [Fact]
        public void GetLineHeight_WithUnitsPerEm2048()
        {
            // Chrome rounds each component individually (lroundf):
            // ascent = round(12 * 1854 / 2048) = round(10.863) = 11
            // descent = round(12 * 434 / 2048) = round(2.543) = 3
            // lineGap = 0
            // total = 11 + 3 = 14
            var metrics = new FontMetricsInfo(1854, -434, 0, 2048, 1490, 1062);
            float lineHeight = metrics.GetLineHeight(12f);

            Assert.Equal(14f, lineHeight, 4);
        }

        [Fact]
        public void GetLineHeight_ZeroUnitsPerEm_ReturnsFontSize()
        {
            var metrics = new FontMetricsInfo(800, -200, 0, 0, 700, 500);
            float lineHeight = metrics.GetLineHeight(16f);

            Assert.Equal(16f, lineHeight);
        }

        [Fact]
        public void GetAscent_ComputesCorrectly()
        {
            // round(16 * 800 / 1000) = round(12.8) = 13
            var metrics = new FontMetricsInfo(800, -200, 0, 1000, 700, 500);
            float ascent = metrics.GetAscent(16f);

            Assert.Equal(13f, ascent, 2);
        }

        [Fact]
        public void GetAscent_ZeroUnitsPerEm_ReturnsFontSize()
        {
            var metrics = new FontMetricsInfo(800, -200, 0, 0, 700, 500);
            float ascent = metrics.GetAscent(16f);

            Assert.Equal(16f, ascent);
        }

        [Fact]
        public void GetDescent_ComputesCorrectly_ReturnsPositive()
        {
            // round(16 * 200 / 1000) = round(3.2) = 3
            var metrics = new FontMetricsInfo(800, -200, 0, 1000, 700, 500);
            float descent = metrics.GetDescent(16f);

            Assert.Equal(3f, descent, 2);
        }

        [Fact]
        public void GetDescent_ZeroUnitsPerEm_ReturnsZero()
        {
            var metrics = new FontMetricsInfo(800, -200, 0, 0, 700, 500);
            float descent = metrics.GetDescent(16f);

            Assert.Equal(0f, descent);
        }

        [Fact]
        public void GetAscent_LargeFontSize()
        {
            var metrics = new FontMetricsInfo(800, -200, 0, 1000, 700, 500);
            float ascent = metrics.GetAscent(72f);

            // round(72 * 800 / 1000) = round(57.6) = 58
            Assert.Equal(58f, ascent, 2);
        }

        [Fact]
        public void GetDescent_LargeFontSize()
        {
            var metrics = new FontMetricsInfo(800, -200, 0, 1000, 700, 500);
            float descent = metrics.GetDescent(72f);

            // round(72 * 200 / 1000) = round(14.4) = 14
            Assert.Equal(14f, descent, 2);
        }

        [Fact]
        public void GetLineHeight_EqualsRoundedComponentsSum()
        {
            // Chrome rounds each component individually, so lineHeight = round(a) + round(d) + round(lg)
            var metrics = new FontMetricsInfo(800, -200, 90, 1000, 700, 500);
            float fontSize = 24f;

            float lineHeight = metrics.GetLineHeight(fontSize);
            // round(24*800/1000) = round(19.2) = 19
            // round(24*200/1000) = round(4.8) = 5
            // round(24*90/1000) = round(2.16) = 2
            // total = 19 + 5 + 2 = 26
            Assert.Equal(26f, lineHeight, 4);

            // GetAscent uses WinAscent (0 here, so falls back to Ascent=800)
            float ascent = metrics.GetAscent(fontSize);
            Assert.Equal(19f, ascent, 4);
        }
    }
}
