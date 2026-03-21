using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Edge-case tests for calc(), nested calc(), min(), max(), clamp()
    /// across different CSS properties and unit combinations.
    /// </summary>
    public class WptCssCalcEdgeCaseTests
    {
        private readonly ITestOutputHelper _output;

        public WptCssCalcEdgeCaseTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-VALUES §8.1] calc() pure addition
        [Fact]
        public void CalcWidth_PxAddition()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:calc(100px + 50px);height:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"Expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() pure subtraction
        [Fact]
        public void CalcWidth_PxSubtraction()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:calc(200px - 50px);height:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"Expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() multiplication
        [Fact]
        public void CalcWidth_PxMultiplication()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:calc(50px * 3);height:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"Expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() division
        [Fact]
        public void CalcWidth_PxDivision()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:calc(300px / 2);height:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"Expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() percentage plus px in width
        [Fact]
        public void CalcWidth_PercentPlusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:calc(50% + 20px);height:10px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50% of 400 = 200, + 20 = 220
            Assert.True(System.Math.Abs(target.ContentRect.Width - 220) < 1,
                $"Expected 220, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() percentage minus px in width
        [Fact]
        public void CalcWidth_PercentMinusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:calc(50% - 20px);height:10px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50% of 400 = 200, - 20 = 180
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 1,
                $"Expected 180, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc(100% - 80px) for near-full width
        [Fact]
        public void CalcWidth_FullPercentMinusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:calc(100% - 80px);height:10px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 100% of 400 = 400, - 80 = 320
            Assert.True(System.Math.Abs(target.ContentRect.Width - 320) < 1,
                $"Expected 320, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc(25% + 50px) quarter plus offset
        [Fact]
        public void CalcWidth_QuarterPercentPlusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:calc(25% + 50px);height:10px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 25% of 400 = 100, + 50 = 150
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"Expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() with em units
        [Fact]
        public void CalcWidth_EmPlusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='font-size:16px'>
                    <div id='t' style='width:calc(10em + 20px);height:10px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 10em at 16px = 160, + 20 = 180
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 1,
                $"Expected 180, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() with vw viewport unit (400px viewport)
        [Fact]
        public void CalcWidth_VwPlusPx()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:calc(50vw + 20px);height:10px'></div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50vw at 400px = 200, + 20 = 220
            Assert.True(System.Math.Abs(target.ContentRect.Width - 220) < 1,
                $"Expected 220, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() with vh viewport unit (300px viewport)
        [Fact]
        public void CalcHeight_VhMinusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='height:300px'>
                    <div id='t' style='height:calc(50vh - 10px)'></div>
                </div></body>",
                400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50vh at 300px = 150, - 10 = 140
            Assert.True(System.Math.Abs(target.ContentRect.Height - 140) < 1,
                $"Expected 140, got {target.ContentRect.Height}");
        }

        // [CSS-VALUES §8.1] nested calc(calc(...) + ...)
        [Fact]
        public void CalcWidth_NestedCalc()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:calc(calc(100px + 50px) + 50px);height:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // (100 + 50) + 50 = 200
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() applied to height property
        [Fact]
        public void CalcHeight_PxAddition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='height:400px'>
                    <div id='t' style='width:50px;height:calc(80px + 40px)'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 120) < 1,
                $"Expected 120, got {target.ContentRect.Height}");
        }

        // [CSS-VALUES §8.1] calc() on padding
        [Fact]
        public void CalcPadding_PxAddition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='padding-left:calc(20px + 10px);height:10px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.PaddingLeft - 30) < 1,
                $"Expected padding-left 30, got {target.PaddingLeft}");
        }

        // [CSS-VALUES §8.1] calc() on margin
        [Fact]
        public void CalcMargin_PercentPlusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'><div style='width:400px'>
                    <div id='t' style='margin-left:calc(10% + 15px);width:50px;height:10px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 10% of 400 = 40, + 15 = 55
            Assert.True(System.Math.Abs(target.MarginLeft - 55) < 1,
                $"Expected margin-left 55, got {target.MarginLeft}");
        }

        // [CSS-VALUES §8.1] calc() on flex-basis
        [Fact]
        public void CalcFlexBasis_PercentMinusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:flex;width:400px'>
                        <div id='t' style='flex:0 0 calc(50% - 20px);height:30px'></div>
                    </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50% of 400 = 200, - 20 = 180
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 1,
                $"Expected 180, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() on grid-template-columns
        [Fact]
        public void CalcGridTemplateColumns()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:grid;grid-template-columns:calc(50% - 10px) calc(50% - 10px);gap:20px;width:200px'>
                        <div id='a' style='height:20px'></div>
                        <div id='b' style='height:20px'></div>
                    </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            // 50% of 200 = 100, - 10 = 90 per column, gap = 20 between
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 90) < 2,
                $"Expected grid column ~90, got {itemA.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() on min-width
        [Fact]
        public void CalcMinWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:50px;min-width:calc(100px + 50px);height:10px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // min-width: 150px overrides width: 50px
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"Expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() on max-width
        [Fact]
        public void CalcMaxWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:300px;max-width:calc(100px + 50px);height:10px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // max-width: 150px clamps width: 300px down
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"Expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() on border-width
        [Fact]
        public void CalcBorderWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='border-left:calc(3px + 2px) solid black;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.BorderLeftWidth - 5) < 1,
                $"Expected border-left-width 5, got {target.BorderLeftWidth}");
        }

        // [CSS-VALUES §8.1] calc() negative result clamped to 0 for width
        [Fact]
        public void CalcWidth_NegativeResultClampedToZero()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:calc(50px - 100px);height:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Negative width clamps to 0
            Assert.True(target.ContentRect.Width >= 0,
                $"Width should not be negative, got {target.ContentRect.Width}");
            Assert.True(target.ContentRect.Width < 1,
                $"Expected 0 (clamped), got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() with rem units
        [Fact]
        public void CalcWidth_RemPlusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<html style='font-size:16px'><body style='margin:0'>
                    <div id='t' style='width:calc(5rem + 20px);height:10px'></div>
                </body></html>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 5rem at 16px root = 80, + 20 = 100
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"Expected 100, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.2] min() picks the smaller of two px values
        [Fact]
        public void Min_TwoPxValues()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:min(200px, 300px);height:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.2] max() picks the larger of two px values
        [Fact]
        public void Max_TwoPxValues()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:max(200px, 300px);height:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1,
                $"Expected 300, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] clamp(min, preferred, max) returns preferred when in range
        [Fact]
        public void Clamp_PreferredInRange()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:clamp(100px, 200px, 300px);height:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] clamp() clamps up to min when preferred is below
        [Fact]
        public void Clamp_PreferredBelowMin()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:clamp(150px, 50px, 300px);height:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // preferred 50 < min 150, result = 150
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"Expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] clamp() clamps down to max when preferred is above
        [Fact]
        public void Clamp_PreferredAboveMax()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:clamp(100px, 500px, 300px);height:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // preferred 500 > max 300, result = 300
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1,
                $"Expected 300, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc() with percentage in height context
        [Fact]
        public void CalcHeight_PercentPlusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:100px;height:200px'>
                    <div id='t' style='height:calc(50% + 30px)'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50% of 200 = 100, + 30 = 130
            Assert.True(System.Math.Abs(target.ContentRect.Height - 130) < 1,
                $"Expected 130, got {target.ContentRect.Height}");
        }

        // [CSS-VALUES §8.1] calc() with mixed multiplication and addition
        [Fact]
        public void CalcWidth_MultiplyThenAdd()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:calc(40px * 2 + 20px);height:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 40*2 + 20 = 100
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"Expected 100, got {target.ContentRect.Width}");
        }
    }
}
