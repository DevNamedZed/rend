using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for calc(), min(), max(), clamp() with various unit combinations
    /// and property contexts.
    /// </summary>
    public class WptCalcSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptCalcSizingTests(ITestOutputHelper output) { _output = output; }

        // calc on width with percentage
        [Fact]
        public void CalcWidth_PercentMinusPx()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:500px'>
                    <div id='t' style='width:calc(80% - 50px);height:20px'></div>
                </div></body>");
            // 80% of 500 = 400. 400-50 = 350.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 350) < 2);
        }

        // calc on height with percentage
        [Fact]
        public void CalcHeight_PercentPlusPx()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px;height:400px'>
                    <div id='t' style='height:calc(50% + 30px)'></div>
                </div></body>");
            // 50% of 400 = 200. 200+30 = 230.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 230) < 2);
        }

        // calc on margin
        [Fact]
        public void CalcMargin()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0;overflow:hidden'>
                <div style='width:400px'>
                    <div id='t' style='margin-left:calc(25% + 10px);width:50px;height:20px'></div>
                </div></body>");
            // 25% of 400 = 100. 100+10 = 110.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 110) < 2);
        }

        // calc on padding
        [Fact]
        public void CalcPadding()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='padding-left:calc(10% + 5px);height:20px'></div>
                </div></body>");
            // 10% of 200 = 20. 20+5 = 25.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.PaddingLeft - 25) < 2);
        }

        // min() picks smaller of two
        [Fact]
        public void Min_PicksSmaller()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:min(200px, 30%);height:20px'></div>
                </div></body>");
            // min(200, 120) = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        // min() with larger percentage
        [Fact]
        public void Min_PercentIsLarger()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:min(100px, 80%);height:20px'></div>
                </div></body>");
            // min(100, 320) = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // max() picks larger
        [Fact]
        public void Max_PicksLarger()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:max(100px, 30%);height:20px'></div>
                </div></body>");
            // max(100, 120) = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        // max() with smaller percentage
        [Fact]
        public void Max_PxIsLarger()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:max(200px, 10%);height:20px'></div>
                </div></body>");
            // max(200, 40) = 200
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // clamp() middle value
        [Fact]
        public void Clamp_MiddleWins()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:clamp(50px, 40%, 300px);height:20px'></div>
                </div></body>");
            // clamp(50, 160, 300) = 160
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 160) < 2);
        }

        // clamp() min clamps up
        [Fact]
        public void Clamp_MinClampsUp()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px'>
                    <div id='t' style='width:clamp(80px, 10%, 200px);height:20px'></div>
                </div></body>");
            // clamp(80, 10, 200) = max(80, min(10, 200)) = 80
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 80) < 2);
        }

        // clamp() max clamps down
        [Fact]
        public void Clamp_MaxClampsDown()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:1000px'>
                    <div id='t' style='width:clamp(50px, 80%, 200px);height:20px'></div>
                </div></body>");
            // clamp(50, 800, 200) = max(50, min(800, 200)) = 200
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // calc with em unit
        [Fact]
        public void Calc_WithEm()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='font-size:16px'>
                    <div id='t' style='width:calc(10em + 20px);height:20px'></div>
                </div></body>");
            // 10em = 160. 160+20 = 180.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 180) < 2);
        }

        // calc with vw
        [Fact]
        public void Calc_WithVw()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:calc(50vw + 20px);height:20px'></div></body>", 400, 300);
            // 50vw = 200. 200+20 = 220.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 220) < 2);
        }

        // calc on flex-basis
        [Fact]
        public void CalcFlexBasis()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex:0 0 calc(50% - 20px);height:30px'></div>
                </div></body>");
            // 50% of 400 = 200. 200-20 = 180.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 180) < 2);
        }

        // calc nested: calc(calc(x) + y)
        [Fact]
        public void CalcNested()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:calc(calc(50px + 50px) * 2);height:20px'></div></body>");
            // (50+50)*2 = 200
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // calc on grid track
        [Fact]
        public void CalcGridTrack()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:calc(50% - 10px) calc(50% - 10px);gap:20px;width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // Each col: 50% of 200 - 10 = 90. Gap = 20. Total = 90+20+90 = 200.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 90) < 2,
                $"calc grid track (got {LayoutTestHelper.FindById(r, "a")!.ContentRect.Width})");
        }
    }
}
