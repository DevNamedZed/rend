using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS min(), max(), clamp() math functions per CSS Values and Units Level 4 sections 8.2-8.4.
    /// Covers pure px, percentage, calc, viewport units, em, nested functions, and various property contexts.
    /// </summary>
    public class WptCssMinMaxClampTests
    {
        private readonly ITestOutputHelper _output;

        public WptCssMinMaxClampTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-VALUES §8.2] min() with two px values picks the smaller
        [Fact]
        public void MinTwoPxValuesPicksSmaller()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:min(200px, 300px);height:10px'></div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"min(200px,300px) expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.2] min() with percent and px where percent is smaller
        [Fact]
        public void MinPercentSmallerThanPx()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='width:min(50%, 200px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50% of 300 = 150, min(150, 200) = 150
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"min(50%,200px) in 300px container expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.2] min() with percent and px where px is smaller
        [Fact]
        public void MinPxSmallerThanPercent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:500px'>
                    <div id='t' style='width:min(50%, 200px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50% of 500 = 250, min(250, 200) = 200
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"min(50%,200px) in 500px container expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] max() with two px values picks the larger
        [Fact]
        public void MaxTwoPxValuesPicksLarger()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:max(100px, 200px);height:10px'></div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"max(100px,200px) expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] max() with percent and px where percent is larger
        [Fact]
        public void MaxPercentLargerThanPx()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:max(50%, 100px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50% of 400 = 200, max(200, 100) = 200
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"max(50%,100px) in 400px container expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] max() with percent and px where px is larger
        [Fact]
        public void MaxPxLargerThanPercent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px'>
                    <div id='t' style='width:max(50%, 100px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50% of 100 = 50, max(50, 100) = 100
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"max(50%,100px) in 100px container expected 100, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.4] clamp() preferred value within range passes through
        [Fact]
        public void ClampPreferredWithinRange()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:clamp(100px, 200px, 300px);height:10px'></div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"clamp(100px,200px,300px) expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.4] clamp() preferred below min clamps to min
        [Fact]
        public void ClampPreferredBelowMinClampsToMin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:clamp(100px, 50px, 300px);height:10px'></div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // clamp(100, 50, 300) = max(100, min(50, 300)) = max(100, 50) = 100
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"clamp(100px,50px,300px) expected 100, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.4] clamp() preferred above max clamps to max
        [Fact]
        public void ClampPreferredAboveMaxClampsToMax()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:clamp(100px, 400px, 300px);height:10px'></div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // clamp(100, 400, 300) = max(100, min(400, 300)) = max(100, 300) = 300
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 2,
                $"clamp(100px,400px,300px) expected 300, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.2] min() with calc() sub-expressions
        [Fact]
        public void MinWithCalcSubExpressions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:min(calc(100px + 50px), 200px);height:10px'></div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // min(150, 200) = 150
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"min(calc(100px+50px),200px) expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] max() with calc() sub-expressions
        [Fact]
        public void MaxWithCalcSubExpressions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:max(calc(200px - 50px), 100px);height:10px'></div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // max(150, 100) = 150
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"max(calc(200px-50px),100px) expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.4] clamp() with percentage preferred value
        [Fact]
        public void ClampWithPercentPreferred()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:clamp(100px, 50%, 300px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50% of 400 = 200, clamp(100, 200, 300) = 200
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"clamp(100px,50%,300px) expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.2] min() applied to width property
        [Fact]
        public void MinInWidthProperty()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:min(150px, 250px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"min(150px,250px) in width expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] max() applied to height property
        [Fact]
        public void MaxInHeightProperty()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px;height:400px'>
                    <div id='t' style='height:max(100px, 200px);width:50px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2,
                $"max(100px,200px) in height expected 200, got {target.ContentRect.Height}");
        }

        // [CSS-VALUES §8.4] clamp() applied to padding property
        [Fact]
        public void ClampInPaddingProperty()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='padding-left:clamp(10px, 50px, 80px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // clamp(10, 50, 80) = 50
            Assert.True(System.Math.Abs(target.PaddingLeft - 50) < 2,
                $"clamp(10px,50px,80px) in padding expected 50, got {target.PaddingLeft}");
        }

        // [CSS-VALUES §8.2] min() applied to margin property
        [Fact]
        public void MinInMarginProperty()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0;overflow:hidden'>
                <div style='width:400px'>
                    <div id='t' style='margin-left:min(30px, 60px);width:50px;height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // min(30, 60) = 30, content starts at x=30
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2,
                $"min(30px,60px) in margin-left expected x=30, got {target.ContentRect.X}");
        }

        // [CSS-VALUES §8.2] nested min(min()) picks innermost smaller value
        [Fact]
        public void NestedMinInsideMin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:min(min(300px, 150px), 200px);height:10px'></div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // inner min(300, 150) = 150, outer min(150, 200) = 150
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"min(min(300px,150px),200px) expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.2] min() with vw viewport unit
        [Fact]
        public void MinWithVwUnit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:min(50vw, 300px);height:10px'></div>
            </body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50vw = 200, min(200, 300) = 200
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"min(50vw,300px) expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] max() with em unit
        [Fact]
        public void MaxWithEmUnit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='font-size:20px'>
                    <div id='t' style='width:max(5em, 80px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 5em = 100, max(100, 80) = 100
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"max(5em,80px) expected 100, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.4] clamp() with mixed units: em min, percent preferred, px max
        [Fact]
        public void ClampWithMixedUnits()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:16px'>
                    <div id='t' style='width:clamp(3em, 50%, 250px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 3em = 48, 50% of 400 = 200, clamp(48, 200, 250) = 200
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"clamp(3em,50%,250px) expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.2] min(100%, 300px) responsive — container narrower than 300px
        [Fact]
        public void MinFullPercentAndPxNarrowContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='width:min(100%, 300px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 100% of 200 = 200, min(200, 300) = 200
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"min(100%,300px) in 200px container expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.2] min(100%, 300px) responsive — container wider than 300px
        [Fact]
        public void MinFullPercentAndPxWideContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:500px'>
                    <div id='t' style='width:min(100%, 300px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 100% of 500 = 500, min(500, 300) = 300
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 2,
                $"min(100%,300px) in 500px container expected 300, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] max(50px, 10%) responsive — container where 10% > 50px
        [Fact]
        public void MaxPxAndPercentWideContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:600px'>
                    <div id='t' style='width:max(50px, 10%);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 10% of 600 = 60, max(50, 60) = 60
            Assert.True(System.Math.Abs(target.ContentRect.Width - 60) < 2,
                $"max(50px,10%) in 600px container expected 60, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] max(50px, 10%) responsive — container where 10% < 50px
        [Fact]
        public void MaxPxAndPercentNarrowContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='width:max(50px, 10%);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 10% of 200 = 20, max(50, 20) = 50
            Assert.True(System.Math.Abs(target.ContentRect.Width - 50) < 2,
                $"max(50px,10%) in 200px container expected 50, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.4] clamp() in flex-basis property
        [Fact]
        public void ClampInFlexBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex:0 0 clamp(80px, 50%, 250px);height:20px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50% of 400 = 200, clamp(80, 200, 250) = 200
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"clamp in flex-basis expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.4] clamp() in grid-template-columns property
        [Fact]
        public void ClampInGridTemplateColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:clamp(50px, 40%, 200px) 1fr;width:400px'>
                    <div id='t' style='height:20px'></div>
                    <div style='height:20px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 40% of 400 = 160, clamp(50, 160, 200) = 160
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 2,
                $"clamp in grid-template-columns expected 160, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.2] min() applied to min-width property
        [Fact]
        public void MinInMinWidthProperty()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:50px;min-width:min(100px, 200px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // min(100, 200) = 100, width:50px < min-width:100, so result = 100
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"min(100px,200px) in min-width expected 100, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] max() applied to max-width property
        [Fact]
        public void MaxInMaxWidthProperty()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:300px;max-width:max(100px, 200px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // max(100, 200) = 200, width:300px > max-width:200, so result = 200
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"max(100px,200px) in max-width expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.2] min() with three arguments picks smallest
        [Fact]
        public void MinThreeArguments()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:min(300px, 150px, 250px);height:10px'></div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"min(300px,150px,250px) expected 150, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] max() with three arguments picks largest
        [Fact]
        public void MaxThreeArguments()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:max(100px, 250px, 180px);height:10px'></div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 250) < 2,
                $"max(100px,250px,180px) expected 250, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.4] clamp() where min > max, min wins per spec
        [Fact]
        public void ClampMinGreaterThanMaxMinWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:clamp(200px, 150px, 100px);height:10px'></div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // clamp(200, 150, 100) = max(200, min(150, 100)) = max(200, 100) = 200
            // Per spec: if MIN > MAX, result is MIN
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"clamp(200px,150px,100px) min>max expected 200, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.4] clamp() with percentage min clamping up
        [Fact]
        public void ClampPercentMinClampsUp()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='width:clamp(40%, 30px, 300px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 40% of 200 = 80, clamp(80, 30, 300) = max(80, min(30, 300)) = max(80, 30) = 80
            Assert.True(System.Math.Abs(target.ContentRect.Width - 80) < 2,
                $"clamp(40%,30px,300px) expected 80, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.2] min() with vw where vw is smaller
        [Fact]
        public void MinVwSmallerThanPx()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:min(25vw, 200px);height:10px'></div>
            </body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 25vw = 100, min(100, 200) = 100
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"min(25vw,200px) expected 100, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] max() with em where em is smaller, px wins
        [Fact]
        public void MaxEmSmallerPxWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='font-size:16px'>
                    <div id='t' style='width:max(3em, 100px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 3em = 48, max(48, 100) = 100
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"max(3em,100px) expected 100, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.4] clamp() in height with all px values
        [Fact]
        public void ClampInHeightAllPx()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px;height:400px'>
                    <div id='t' style='height:clamp(50px, 120px, 200px);width:50px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // clamp(50, 120, 200) = 120
            Assert.True(System.Math.Abs(target.ContentRect.Height - 120) < 2,
                $"clamp(50px,120px,200px) in height expected 120, got {target.ContentRect.Height}");
        }

        // [CSS-VALUES §8.2] min() with calc() containing percentage
        [Fact]
        public void MinWithCalcPercentage()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:min(calc(50% - 20px), 250px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // calc(50% - 20px) = 200 - 20 = 180, min(180, 250) = 180
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2,
                $"min(calc(50%-20px),250px) expected 180, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.3] max() with calc() where calc wins
        [Fact]
        public void MaxWithCalcWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:max(calc(25% + 50px), 100px);height:10px'></div>
                </div>
            </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // calc(25% + 50px) = 100 + 50 = 150, max(150, 100) = 150
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"max(calc(25%+50px),100px) expected 150, got {target.ContentRect.Width}");
        }
    }
}
