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
            // LineHeight = fontSize * (Ascent - Descent + LineGap) / UnitsPerEm
            // = 16 * (800 - (-200) + 90) / 1000
            // = 16 * 1090 / 1000
            // = 17.44
            var metrics = new FontMetricsInfo(800, -200, 90, 1000, 700, 500);
            float lineHeight = metrics.GetLineHeight(16f);

            Assert.Equal(17.44f, lineHeight, 2);
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
            // Common for TrueType: unitsPerEm = 2048
            // LineHeight = 12 * (1854 - (-434) + 0) / 2048
            // = 12 * 2288 / 2048
            // = 13.40625
            var metrics = new FontMetricsInfo(1854, -434, 0, 2048, 1490, 1062);
            float lineHeight = metrics.GetLineHeight(12f);

            Assert.Equal(13.40625f, lineHeight, 4);
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
            // Ascent = fontSize * Ascent / UnitsPerEm = 16 * 800 / 1000 = 12.8
            var metrics = new FontMetricsInfo(800, -200, 0, 1000, 700, 500);
            float ascent = metrics.GetAscent(16f);

            Assert.Equal(12.8f, ascent, 2);
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
            // Descent = fontSize * -Descent / UnitsPerEm = 16 * 200 / 1000 = 3.2
            // Note: Descent is typically negative, so the method negates it
            var metrics = new FontMetricsInfo(800, -200, 0, 1000, 700, 500);
            float descent = metrics.GetDescent(16f);

            Assert.Equal(3.2f, descent, 2);
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

            // 72 * 800 / 1000 = 57.6
            Assert.Equal(57.6f, ascent, 2);
        }

        [Fact]
        public void GetDescent_LargeFontSize()
        {
            var metrics = new FontMetricsInfo(800, -200, 0, 1000, 700, 500);
            float descent = metrics.GetDescent(72f);

            // 72 * 200 / 1000 = 14.4
            Assert.Equal(14.4f, descent, 2);
        }

        [Fact]
        public void GetLineHeight_EqualsAscentPlusDescentPlusLineGapScaled()
        {
            var metrics = new FontMetricsInfo(800, -200, 90, 1000, 700, 500);
            float fontSize = 24f;

            float lineHeight = metrics.GetLineHeight(fontSize);
            float ascent = metrics.GetAscent(fontSize);
            float descent = metrics.GetDescent(fontSize);
            float lineGapScaled = fontSize * metrics.LineGap / metrics.UnitsPerEm;

            Assert.Equal(lineHeight, ascent + descent + lineGapScaled, 4);
        }
    }
}
