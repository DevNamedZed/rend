using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptPercentageResolutionTests
    {
        private readonly ITestOutputHelper _output;
        public WptPercentageResolutionTests(ITestOutputHelper output) { _output = output; }

        // width: 50% in 400px container = 200
        [Fact]
        public void Width_50Percent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='width:50%;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // width: 100% fills parent
        [Fact]
        public void Width_100Percent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='width:100%;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 300) < 2);
        }

        // height: 50% in 400px parent = 200
        [Fact]
        public void Height_50Percent_Definite()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:100px;height:400px'><div id='t' style='height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 200) < 2);
        }

        // height: 50% in auto parent = 0 (auto)
        [Fact]
        public void Height_50Percent_Auto()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:100px'><div id='t' style='height:50%'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height < 1);
        }

        // padding-top: 10% resolves against parent WIDTH
        [Fact]
        public void PaddingTop_Percent_AgainstWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px;height:400px'><div id='t' style='padding-top:10%;height:0'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.PaddingTop - 20) < 2);
        }

        // margin-left: 25% resolves against parent WIDTH
        [Fact]
        public void MarginLeft_Percent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0;overflow:hidden'><div style='width:400px'><div id='t' style='margin-left:25%;width:50px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 100) < 2);
        }

        // margin-top: 10% resolves against parent WIDTH (not height!)
        [Fact]
        public void MarginTop_Percent_AgainstWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0;overflow:hidden'><div style='width:300px;height:500px'><div id='t' style='margin-top:10%;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.MarginTop - 30) < 2);
        }

        // nested percentage width chains
        [Fact]
        public void Nested_Percent_Width()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:800px'><div style='width:50%'><div style='width:50%'><div id='t' style='width:50%;height:10px'></div></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // percentage width inside flex item resolves against flex item width
        [Fact]
        public void Percent_InFlexItem()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='width:200px;height:30px'><div id='t' style='width:50%;height:10px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // percentage width inside grid item resolves against track width
        [Fact]
        public void Percent_InGridItem()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;width:300px'><div><div id='t' style='width:50%;height:10px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 150) < 2);
        }

        // percentage on abspos resolves against containing block
        [Fact]
        public void Percent_AbsPos_Width()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:200px'><div id='t' style='position:absolute;width:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // percentage on abspos height resolves against CB
        [Fact]
        public void Percent_AbsPos_Height()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:400px'><div id='t' style='position:absolute;width:50px;height:25%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        // percentage top/left on abspos
        [Fact]
        public void Percent_AbsPos_TopLeft()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:200px'><div id='t' style='position:absolute;top:10%;left:20%;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 20) < 2);
        }

        // percentage on fixed position resolves against viewport
        [Fact]
        public void Percent_Fixed()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='position:fixed;top:0;left:0;width:50%;height:25%'></div></body>", 400, 200);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 50) < 2);
        }

        // percentage width with box-sizing: border-box
        [Fact]
        public void Percent_BorderBox()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='box-sizing:border-box;width:50%;padding:20px;border:5px solid;height:30px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            float totalW = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(System.Math.Abs(totalW - 200) < 2);
        }

        // min-width percentage
        [Fact]
        public void MinWidth_Percent_Resolves()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='width:30px;min-width:50%;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // max-width percentage
        [Fact]
        public void MaxWidth_Percent_Resolves()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='max-width:25%;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // flex-basis percentage resolves against flex container
        [Fact]
        public void FlexBasis_Percent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='flex:0 0 25%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // calc with percentage: calc(50% + 20px)
        [Fact]
        public void Calc_PercentPlusPx()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='width:calc(50% + 20px);height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 220) < 2);
        }

        // min() with percentage
        [Fact]
        public void Min_Percent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='width:min(300px,40%);height:20px'></div></div></body>");
            // min(300, 160) = 160
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 160) < 2);
        }

        // clamp with percentage
        [Fact]
        public void Clamp_Percent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='width:clamp(100px,30%,200px);height:20px'></div></div></body>");
            // clamp(100, 120, 200) = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }
    }
}
