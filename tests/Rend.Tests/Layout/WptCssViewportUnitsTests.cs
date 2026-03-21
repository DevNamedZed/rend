using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS viewport-relative units (vw, vh, vmin, vmax)
    /// in width, height, padding, margin, calc(), flex-basis, and grid contexts.
    /// <spec>CSS-VALUES-4 §7.1 https://drafts.csswg.org/css-values-4/#viewport-relative-lengths</spec>
    /// </summary>
    public class WptCssViewportUnitsTests
    {
        private readonly ITestOutputHelper _output;

        public WptCssViewportUnitsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-VALUES-4 §7.1] vw: 1vw = 1% of viewport width

        [Fact]
        public void Vw_50_In_400px_Viewport_Equals_200()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:50vw;height:10px'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void Vw_100_In_400px_Viewport_Equals_400()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100vw;height:10px'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 1);
        }

        [Fact]
        public void Vw_25_In_400px_Viewport_Equals_100()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:25vw;height:10px'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1);
        }

        // [CSS-VALUES-4 §7.1] vh: 1vh = 1% of viewport height

        [Fact]
        public void Vh_50_In_300px_Viewport_Equals_150()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:10px;height:50vh'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 150) < 1);
        }

        [Fact]
        public void Vh_100_In_300px_Viewport_Equals_300()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:10px;height:100vh'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 300) < 1);
        }

        [Fact]
        public void Vh_25_In_300px_Viewport_Equals_75()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:10px;height:25vh'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 75) < 1);
        }

        // [CSS-VALUES-4 §7.1] vmin: 1vmin = 1% of min(viewport width, viewport height)

        [Fact]
        public void Vmin_100_In_400x300_Equals_300()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100vmin;height:10px'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void Vmin_50_In_400x300_Equals_150()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:50vmin;height:10px'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1);
        }

        // [CSS-VALUES-4 §7.1] vmax: 1vmax = 1% of max(viewport width, viewport height)

        [Fact]
        public void Vmax_100_In_400x300_Equals_400()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100vmax;height:10px'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 1);
        }

        [Fact]
        public void Vmax_50_In_400x300_Equals_200()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:50vmax;height:10px'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1);
        }

        // [CSS-VALUES-4 §7.1] vw/vh in larger viewports

        [Fact]
        public void Vw_50_In_800px_Viewport_Equals_400()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:50vw;height:10px'></div></body>",
                800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 1);
        }

        [Fact]
        public void Vh_50_In_600px_Viewport_Equals_300()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:10px;height:50vh'></div></body>",
                800, 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 300) < 1);
        }

        // [CSS-VALUES-4 §7.1] vw applied to height property, vh applied to width property

        [Fact]
        public void Vw_In_Height_Property()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:10px;height:50vw'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 1);
        }

        [Fact]
        public void Vh_In_Width_Property()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:50vh;height:10px'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1);
        }

        // [CSS-VALUES-4 §7.1] viewport units in padding

        [Fact]
        public void Vw_In_Padding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='padding:10vw;width:0;height:0'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.PaddingLeft - 40) < 1);
            Assert.True(System.Math.Abs(target.PaddingRight - 40) < 1);
            Assert.True(System.Math.Abs(target.PaddingTop - 40) < 1);
            Assert.True(System.Math.Abs(target.PaddingBottom - 40) < 1);
        }

        [Fact]
        public void Vh_In_Margin()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0;overflow:hidden'><div id='t' style='margin:10vh;width:10px;height:10px'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.MarginTop - 30) < 1);
            Assert.True(System.Math.Abs(target.MarginLeft - 30) < 1);
        }

        // [CSS-VALUES-4 §8.1] calc() with viewport units

        [Fact]
        public void Calc_50vw_Plus_20px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:calc(50vw + 20px);height:10px'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 220) < 1);
        }

        [Fact]
        public void Calc_50vh_Minus_10px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:10px;height:calc(50vh - 10px)'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 140) < 1);
        }

        // [CSS-VALUES-4 §7.1] vw in flex-basis

        [Fact]
        public void Vw_In_FlexBasis()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:flex'><div id='t' style='flex:0 0 50vw;height:10px'></div></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1);
        }

        // [CSS-VALUES-4 §7.1] vw in grid item width

        [Fact]
        public void Vw_In_GridItemWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:1fr'>" +
                "<div id='t' style='width:50vw;height:10px'></div>" +
                "</div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1);
        }

        // [CSS-VALUES-4 §7.1] 100vw fills entire viewport width

        [Fact]
        public void Vw_100_Fills_Viewport_Width()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100vw;height:10px'></div></body>",
                600, 400);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 600) < 1);
        }

        // [CSS-VALUES-4 §7.1] 100vh fills entire viewport height

        [Fact]
        public void Vh_100_Fills_Viewport_Height()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:10px;height:100vh'></div></body>",
                600, 400);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 400) < 1);
        }

        // [CSS-VALUES-4 §7.1] different viewport sizes change resolved values

        [Fact]
        public void Vw_50_In_1024px_Viewport_Equals_512()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:50vw;height:10px'></div></body>",
                1024, 768);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 512) < 1);
        }

        [Fact]
        public void Vh_50_In_768px_Viewport_Equals_384()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:10px;height:50vh'></div></body>",
                1024, 768);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 384) < 1);
        }

        // [CSS-VALUES-4 §7.1] vmin/vmax when height > width

        [Fact]
        public void Vmin_When_Height_Greater_Than_Width()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100vmin;height:10px'></div></body>",
                300, 500);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void Vmax_When_Height_Greater_Than_Width()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100vmax;height:10px'></div></body>",
                300, 500);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 500) < 1);
        }

        // [CSS-VALUES-4 §7.1] vmin/vmax in height context

        [Fact]
        public void Vmin_In_Height()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:10px;height:50vmin'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 150) < 1);
        }

        [Fact]
        public void Vmax_In_Height()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:10px;height:50vmax'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 1);
        }
    }
}
