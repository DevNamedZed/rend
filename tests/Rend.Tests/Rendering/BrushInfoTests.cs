using Rend.Core.Values;
using Rend.Rendering;
using Xunit;

namespace Rend.Tests.Rendering
{
    public class BrushInfoTests
    {
        [Fact]
        public void Solid_CreatesSolidBrush()
        {
            var color = new CssColor(255, 0, 0, 255);
            var brush = BrushInfo.Solid(color);

            Assert.Equal(255, brush.Color.R);
            Assert.Equal(0, brush.Color.G);
            Assert.Equal(0, brush.Color.B);
            Assert.Equal(255, brush.Color.A);
        }

        [Fact]
        public void Solid_GradientIsNull()
        {
            var brush = BrushInfo.Solid(CssColor.Black);
            Assert.Null(brush.Gradient);
        }

        [Fact]
        public void Solid_ImageIsNull()
        {
            var brush = BrushInfo.Solid(CssColor.Black);
            Assert.Null(brush.Image);
        }

        [Fact]
        public void Solid_TransparentColor()
        {
            var brush = BrushInfo.Solid(CssColor.Transparent);

            Assert.Equal(0, brush.Color.A);
        }

        [Fact]
        public void Solid_WhiteColor()
        {
            var brush = BrushInfo.Solid(CssColor.White);

            Assert.Equal(255, brush.Color.R);
            Assert.Equal(255, brush.Color.G);
            Assert.Equal(255, brush.Color.B);
        }

        [Fact]
        public void FromGradient_SetsGradientProperty()
        {
            var gradient = new GradientInfo(GradientType.Linear, new[]
            {
                new GradientStop(CssColor.Black, 0f),
                new GradientStop(CssColor.White, 1f),
            });

            var brush = BrushInfo.FromGradient(gradient);

            Assert.NotNull(brush.Gradient);
            Assert.Equal(GradientType.Linear, brush.Gradient!.Type);
            Assert.Equal(2, brush.Gradient.Stops.Length);
        }

        [Fact]
        public void Color_CanBeSetDirectly()
        {
            var brush = new BrushInfo();
            brush.Color = new CssColor(128, 64, 32, 200);

            Assert.Equal(128, brush.Color.R);
            Assert.Equal(64, brush.Color.G);
            Assert.Equal(32, brush.Color.B);
            Assert.Equal(200, brush.Color.A);
        }

        [Fact]
        public void Image_CanBeSet()
        {
            var imageData = new ImageData(new byte[] { 0x89, 0x50 }, 10, 10, "png");
            var brush = new BrushInfo();
            brush.Image = imageData;

            Assert.NotNull(brush.Image);
            Assert.Equal("png", brush.Image!.Format);
        }
    }
}
