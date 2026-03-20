using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class OverflowLayoutTests
    {
        private readonly ITestOutputHelper _output;
        public OverflowLayoutTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void OverflowHidden_ClipsContent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='clip' style='overflow: hidden; width: 100px; height: 50px;'>
                    <div style='width: 200px; height: 200px;'></div>
                </div></body>");
            var clip = LayoutTestHelper.FindById(root, "clip");
            Assert.NotNull(clip);
            _output.WriteLine($"clip: {clip!.ContentRect.Width}x{clip.ContentRect.Height}");
            // overflow:hidden should not expand to fit content
            Assert.True(clip.ContentRect.Height <= 51, $"overflow:hidden should clip (got {clip.ContentRect.Height})");
        }

        [Fact]
        public void OverflowAuto_EstablishesBfc()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='auto' style='overflow: auto; width: 200px;'>
                    <div style='float: left; width: 50px; height: 80px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "auto");
            Assert.NotNull(box);
            _output.WriteLine($"auto: h={box!.ContentRect.Height}");
            Assert.True(box.ContentRect.Height >= 79, $"overflow:auto should contain float (got {box.ContentRect.Height})");
        }

        [Fact]
        public void OverflowVisible_DoesNotClip()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='vis' style='overflow: visible; width: 100px; height: 50px;'>
                    <div style='width: 200px; height: 200px;'></div>
                </div></body>");
            var vis = LayoutTestHelper.FindById(root, "vis");
            Assert.NotNull(vis);
            _output.WriteLine($"visible: {vis!.ContentRect.Width}x{vis.ContentRect.Height}");
            // overflow:visible with explicit height should keep that height
            Assert.True(vis.ContentRect.Height <= 51, $"Explicit height respected (got {vis.ContentRect.Height})");
        }

        [Fact]
        public void AbsolutePosition_PercentWidth_ResolvesAfterLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position: relative; width: 300px;'>
                    <div id='abs' style='position: absolute; width: 50%; height: 50px;'></div>
                    <div style='height: 100px;'></div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(abs);
            _output.WriteLine($"abs: {abs!.ContentRect.Width}x{abs.ContentRect.Height}");
            Assert.True(System.Math.Abs(abs.ContentRect.Width - 150) < 2,
                $"50% of 300 should be 150 (got {abs.ContentRect.Width})");
        }
    }
}
