using Xunit;

namespace Rend.Tests.Layout
{
    public class MeterProgressTests
    {
        #region Meter

        [Fact]
        public void Meter_BasicValue_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>Fuel level: <meter value='0.7'>70%</meter></p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Meter_WithMinMax_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>Score: <meter min='0' max='100' value='75'>75 out of 100</meter></p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Meter_LowValue_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>Battery: <meter min='0' max='100' low='25' high='75' optimum='80' value='10'>10%</meter></p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Meter_HighValue_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>CPU: <meter min='0' max='100' low='25' high='75' optimum='20' value='90'>90%</meter></p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Meter_OptimalRange_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>Health: <meter min='0' max='100' low='25' high='75' optimum='50' value='50'>50%</meter></p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Meter_MultipleSideBySide_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div>
                    <p>Low: <meter value='0.2'>20%</meter></p>
                    <p>Mid: <meter value='0.5'>50%</meter></p>
                    <p>High: <meter value='0.9'>90%</meter></p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        #endregion

        #region Progress

        [Fact]
        public void Progress_Determinate_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>Download: <progress value='50' max='100'>50%</progress></p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Progress_Indeterminate_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>Loading: <progress>Loading...</progress></p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Progress_FullyComplete_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>Complete: <progress value='100' max='100'>100%</progress></p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Progress_Zero_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>Starting: <progress value='0' max='100'>0%</progress></p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Progress_FractionValue_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>Upload: <progress value='0.33' max='1'>33%</progress></p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Progress_WithCustomWidth_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>Wide: <progress value='70' max='100' style='width: 300px;'>70%</progress></p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        #endregion

    }
}
