using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptSizingTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void AspectRatio_WidthFromHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='height: 100px; aspect-ratio: 2/1;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width} h={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void AspectRatio_HeightFromWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width: 200px; aspect-ratio: 2/1;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width} h={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void AspectRatio_1to1_Square()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width: 100px; aspect-ratio: 1/1;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void MinContent_Width_Block()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <div id='test' style='width: min-content;'>
                        <div style='width: 80px; height: 10px;'></div>
                        <div style='width: 120px; height: 10px;'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            // min-content = widest child = 120px
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
        }

        [Fact]
        public void FitContent_Constrained()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 60px;'>
                    <div id='test' style='width: fit-content;'>
                        <div style='width: 80px; height: 10px;'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            // fit-content = max(min-content, min(max-content, available))
            // max(80, min(80, 60)) = max(80, 60) = 80
            Assert.True(box.ContentRect.Width >= 59,
                $"fit-content should be at least available (got {box.ContentRect.Width})");
        }

        [Fact]
        public void MaxWidth_ClampsContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <div id='test' style='max-width: 150px; height: 30px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(box!.ContentRect.Width <= 151, $"max-width clamp (got {box.ContentRect.Width})");
        }

        [Fact]
        public void MinWidth_Overrides_MaxWidth()
        {
            // CSS spec: if min > max, min wins
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <div id='test' style='min-width: 200px; max-width: 100px; height: 30px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 199, $"min-width wins over max-width (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Height_Percentage_InDefiniteParent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='height: 200px; width: 100px;'>
                    <div id='test' style='height: 50%;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void Width_Auto_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 300px;'>
                    <div id='test' style='height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 300) < 2,
                $"Auto width fills container (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width_Auto_SubtractsMargins()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 300px;'>
                    <div id='test' style='margin: 0 20px; height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 260) < 2,
                $"Auto width minus margins (got {box.ContentRect.Width})");
        }

        [Fact]
        public void CalcPercent_Width()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <div id='test' style='width: calc(50% - 20px); height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 180) < 2,
                $"calc(50% - 20px) of 400 = 180 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void CalcPercent_Height()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px; height: 400px;'>
                    <div id='test' style='height: calc(25% + 10px);'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 110) < 2,
                $"calc(25% + 10px) of 400 = 110 (got {box.ContentRect.Height})");
        }
    }
}
