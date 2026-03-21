using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests mirroring WPT css-sizing patterns: intrinsic sizing, aspect-ratio,
    /// contain-intrinsic-size, and box-sizing interactions.
    /// </summary>
    public class WptSizingConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptSizingConformanceTests(ITestOutputHelper output) { _output = output; }

        // aspect-ratio 2/1 with width → height = width/2
        [Fact]
        public void AspectRatio_2to1_HeightFromWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;aspect-ratio:2/1'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        // aspect-ratio 1/1 square
        [Fact]
        public void AspectRatio_1to1_Square()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:150px;aspect-ratio:1/1'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 150) < 2);
        }

        // aspect-ratio 16/9
        [Fact]
        public void AspectRatio_16to9()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:320px;aspect-ratio:16/9'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 180) < 2);
        }

        // aspect-ratio with height → width = height * ratio
        [Fact]
        public void AspectRatio_WidthFromHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='height:100px;aspect-ratio:3/1'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 300) < 2);
        }

        // aspect-ratio clamped by max-height
        [Fact]
        public void AspectRatio_MaxHeight_Clamps()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;aspect-ratio:1/1;max-height:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height <= 101);
        }

        // aspect-ratio clamped by max-width
        [Fact]
        public void AspectRatio_MaxWidth_Clamps()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='height:200px;aspect-ratio:2/1;max-width:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width <= 101);
        }

        // aspect-ratio expanded by min-width
        [Fact]
        public void AspectRatio_MinWidth_Expands()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='height:50px;aspect-ratio:1/1;min-width:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 99);
        }

        // width:fit-content shrinks to content
        [Fact]
        public void FitContent_ShrinksToContent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:fit-content'>
                        <div style='width:120px;height:10px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        // width:min-content uses widest child
        [Fact]
        public void MinContent_WidestChild()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:min-content'>
                        <div style='width:80px;height:10px'></div>
                        <div style='width:120px;height:10px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        // width:max-content uses widest possible
        [Fact]
        public void MaxContent_WidestPossible()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:max-content'>
                        <div style='width:150px;height:10px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 150) < 2);
        }

        // box-sizing:border-box with width
        [Fact]
        public void BorderBox_Width()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='box-sizing:border-box;width:200px;padding:20px;border:10px solid;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 140) < 2);
        }

        // box-sizing:border-box with height
        [Fact]
        public void BorderBox_Height()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='box-sizing:border-box;width:100px;height:100px;padding:15px;border:5px solid'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 60) < 2);
        }

        // min-width > max-width: min wins
        [Fact]
        public void MinWidth_Beats_MaxWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='min-width:200px;max-width:100px;height:20px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 199);
        }

        // min-height > max-height: min wins
        [Fact]
        public void MinHeight_Beats_MaxHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;min-height:200px;max-height:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 199);
        }

        // calc(50% - 20px) on width
        [Fact]
        public void Calc_Width()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:calc(50% - 20px);height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 180) < 2);
        }

        // min(300px, 50%) picks smaller
        [Fact]
        public void Min_Width()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:min(300px,50%);height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // max(100px, 25%) picks larger
        [Fact]
        public void Max_Width()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:max(100px,50%);height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // clamp(min, val, max)
        [Fact]
        public void Clamp_Width()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:clamp(80px,30%,200px);height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        // contain:size makes auto height 0
        [Fact]
        public void ContainSize_ZeroAutoHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='contain:size;width:100px'>
                    <div style='height:200px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height < 1);
        }

        // auto width fills container
        [Fact]
        public void AutoWidth_FillsContainer()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:350px'>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 350) < 2);
        }

        // auto width subtracts margins
        [Fact]
        public void AutoWidth_SubtractsMargins()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='margin:0 25px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 250) < 2);
        }

        // margin:auto centers block
        [Fact]
        public void MarginAuto_CentersBlock()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:200px;margin:0 auto;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 100) < 2);
        }

        // percentage height in auto parent = 0
        [Fact]
        public void PercentHeight_AutoParent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='height:50%'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height < 1);
        }

        // percentage height in definite parent
        [Fact]
        public void PercentHeight_DefiniteParent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px;height:300px'>
                    <div id='t' style='height:33.33%'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        // float shrink-to-fit
        [Fact]
        public void Float_ShrinkToFit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='float:left'>
                        <div style='width:90px;height:20px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 90) < 2);
        }

        // abspos shrink-to-fit
        [Fact]
        public void AbsPos_ShrinkToFit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:100px'>
                    <div id='t' style='position:absolute;top:0;left:0'>
                        <div style='width:70px;height:20px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 70) < 2);
        }

        // inline-block shrink-to-fit
        [Fact]
        public void InlineBlock_ShrinkToFit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block'>
                        <div style='width:110px;height:20px'></div>
                    </span>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 110) < 2);
        }
    }
}
