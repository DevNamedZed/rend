using Xunit;

namespace Rend.Tests.Rendering
{
    public class GradientRenderingTests
    {
        [Fact]
        public void LinearGradient_TwoStops_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: 200px; height: 100px; background: linear-gradient(red, blue);'></div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void LinearGradient_ThreeStops_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: 200px; height: 100px; background: linear-gradient(red, green, blue);'></div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void LinearGradient_WithAngle_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: 200px; height: 100px; background: linear-gradient(45deg, red, blue);'></div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void LinearGradient_ToRight_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: 200px; height: 100px; background: linear-gradient(to right, red, blue);'></div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void RadialGradient_TwoStops_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: 200px; height: 200px; background: radial-gradient(red, blue);'></div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void RadialGradient_Circle_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: 200px; height: 200px; background: radial-gradient(circle, red, blue);'></div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ConicGradient_FallsBackGracefully_ProducesValidPdf()
        {
            // Conic gradients are not supported in PDF; should fall back to first stop color
            var result = Render.ToPdf(@"
                <div style='width: 200px; height: 200px; background: conic-gradient(red, blue);'></div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void MultipleGradients_SamePage_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: 200px; height: 100px; background: linear-gradient(red, blue);'></div>
                <div style='width: 200px; height: 100px; background: radial-gradient(green, yellow);'></div>
                <div style='width: 200px; height: 100px; background: linear-gradient(to right, purple, orange);'></div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

    }
}
