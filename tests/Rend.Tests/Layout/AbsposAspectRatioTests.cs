using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class AbsposAspectRatioTests
    {
        private readonly ITestOutputHelper _output;

        public AbsposAspectRatioTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Abspos_AspectRatio_HeightFromWidth_NotOffsets()
        {
            // CSS Sizing L4 §5.1: abspos with aspect-ratio, width from left/right offsets,
            // and auto height should derive height from ratio, not from top/bottom offsets.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:100px;height:500px;position:relative'>
                    <div id='t' style='aspect-ratio:1/1;position:absolute;left:0;right:0;top:0;bottom:0;background:green'></div>
                </div></body>");

            var box = LayoutTestHelper.FindById(root, "t")!;
            // Width = 100px (from left:0 + right:0 + container width)
            // Height should be 100px (from aspect-ratio 1/1), NOT 500px (from top:0 + bottom:0)
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2,
                $"Expected width≈100, got {box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2,
                $"Expected height≈100 from aspect-ratio, got {box.ContentRect.Height}");
        }

        [Fact]
        public void Abspos_AspectRatio_2to1_HeightHalfWidth()
        {
            // With aspect-ratio: 2/1, height should be half of width
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px;height:400px;position:relative'>
                    <div id='t' style='aspect-ratio:2/1;position:absolute;left:0;right:0;top:0;bottom:0'></div>
                </div></body>");

            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"Expected width≈200, got {box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2,
                $"Expected height≈100 from 2/1 ratio, got {box.ContentRect.Height}");
        }

        [Fact]
        public void Abspos_NoAspectRatio_HeightFromOffsets()
        {
            // Without aspect-ratio, height comes from top+bottom offsets as before
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:100px;height:500px;position:relative'>
                    <div id='t' style='position:absolute;left:0;right:0;top:0;bottom:0;background:green'></div>
                </div></body>");

            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Height - 500) < 2,
                $"Expected height≈500 from offsets, got {box.ContentRect.Height}");
        }
    }
}
