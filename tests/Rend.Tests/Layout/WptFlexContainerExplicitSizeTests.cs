using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Flex container explicit sizing: fixed widths/heights, percentages,
    /// calc(), viewport units, min/max constraints, border-box, margins, and inline-flex.
    /// </summary>
    public class WptFlexContainerExplicitSizeTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexContainerExplicitSizeTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.2] flex container with explicit width:100px
        [Fact]
        public void FlexContainer_Width100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:100px;height:30px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 1,
                $"Expected width 100, got {box.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex container with explicit width:200px
        [Fact]
        public void FlexContainer_Width200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;height:30px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 200) < 1,
                $"Expected width 200, got {box.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex container with explicit width:300px
        [Fact]
        public void FlexContainer_Width300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:300px;height:30px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 300) < 1,
                $"Expected width 300, got {box.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex container with explicit width:400px
        [Fact]
        public void FlexContainer_Width400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:400px;height:30px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 400) < 1,
                $"Expected width 400, got {box.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex container with explicit width:500px
        [Fact]
        public void FlexContainer_Width500()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:500px;height:30px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 500) < 1,
                $"Expected width 500, got {box.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex container with explicit height:50px
        [Fact]
        public void FlexContainer_Height50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:100px;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 50) < 1,
                $"Expected height 50, got {box.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9.2] flex container with explicit height:100px
        [Fact]
        public void FlexContainer_Height100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:100px;height:100px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 100) < 1,
                $"Expected height 100, got {box.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9.2] flex container with explicit height:150px
        [Fact]
        public void FlexContainer_Height150()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:100px;height:150px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 150) < 1,
                $"Expected height 150, got {box.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9.2] flex container with explicit height:200px
        [Fact]
        public void FlexContainer_Height200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:100px;height:200px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 200) < 1,
                $"Expected height 200, got {box.ContentRect.Height}");
        }

        // [CSS2 §10.2] percentage width resolves against containing block: 25% of 400 = 100
        [Fact]
        public void FlexContainer_PercentWidth25()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;width:25%;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 1,
                $"Expected width 100 (25% of 400), got {box.ContentRect.Width}");
        }

        // [CSS2 §10.2] percentage width: 50% of 400 = 200
        [Fact]
        public void FlexContainer_PercentWidth50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;width:50%;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 200) < 1,
                $"Expected width 200 (50% of 400), got {box.ContentRect.Width}");
        }

        // [CSS2 §10.2] percentage width: 75% of 400 = 300
        [Fact]
        public void FlexContainer_PercentWidth75()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;width:75%;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 300) < 1,
                $"Expected width 300 (75% of 400), got {box.ContentRect.Width}");
        }

        // [CSS2 §10.2] percentage width: 100% of 400 = 400
        [Fact]
        public void FlexContainer_PercentWidth100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;width:100%;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 400) < 1,
                $"Expected width 400 (100% of 400), got {box.ContentRect.Width}");
        }

        // [CSS2 §10.5] percentage height: 25% of 400 = 100
        [Fact]
        public void FlexContainer_PercentHeight25()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;height:400px'>
                    <div id='t' style='display:flex;width:100px;height:25%'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 100) < 1,
                $"Expected height 100 (25% of 400), got {box.ContentRect.Height}");
        }

        // [CSS2 §10.5] percentage height: 50% of 400 = 200
        [Fact]
        public void FlexContainer_PercentHeight50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;height:400px'>
                    <div id='t' style='display:flex;width:100px;height:50%'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 200) < 1,
                $"Expected height 200 (50% of 400), got {box.ContentRect.Height}");
        }

        // [CSS-VALUES §8.1] calc(200px + 100px) = 300px
        [Fact]
        public void FlexContainer_CalcWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:calc(200px + 100px);height:30px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 300) < 1,
                $"Expected width 300 from calc(200px + 100px), got {box.ContentRect.Width}");
        }

        // [CSS-VALUES §5.1.2] 50vw at viewport 400 = 200
        [Fact]
        public void FlexContainer_ViewportWidth50vw()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:50vw;height:30px'></div></body>",
                viewportWidth: 400, viewportHeight: 300);
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 200) < 1,
                $"Expected width 200 (50vw of 400), got {box.ContentRect.Width}");
        }

        // [CSS-VALUES §5.1.2] 50vh at viewport 300 = 150
        [Fact]
        public void FlexContainer_ViewportHeight50vh()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:100px;height:50vh'></div></body>",
                viewportWidth: 400, viewportHeight: 300);
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 150) < 1,
                $"Expected height 150 (50vh of 300), got {box.ContentRect.Height}");
        }

        // [CSS2 §10.4] min-width prevents shrink below specified value
        [Fact]
        public void FlexContainer_MinWidth200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px'>
                    <div id='t' style='display:flex;min-width:200px;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(box!.ContentRect.Width >= 199,
                $"Expected min-width 200 to hold, got {box.ContentRect.Width}");
        }

        // [CSS2 §10.4] max-width constrains computed width
        [Fact]
        public void FlexContainer_MaxWidth150()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:300px;max-width:150px;height:30px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(box!.ContentRect.Width <= 151,
                $"Expected max-width 150 to clamp, got {box.ContentRect.Width}");
        }

        // [CSS2 §10.7] min-height enforces minimum
        [Fact]
        public void FlexContainer_MinHeight100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:100px;min-height:100px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(box!.ContentRect.Height >= 99,
                $"Expected min-height 100 to hold, got {box.ContentRect.Height}");
        }

        // [CSS2 §10.7] max-height constrains computed height
        [Fact]
        public void FlexContainer_MaxHeight80()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:100px;height:200px;max-height:80px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(box!.ContentRect.Height <= 81,
                $"Expected max-height 80 to clamp, got {box.ContentRect.Height}");
        }

        // [CSS-UI §3.2] border-box: width:300 padding:20 => content width = 300 - 40 = 260
        [Fact]
        public void FlexContainer_BorderBoxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;box-sizing:border-box;width:300px;padding:20px;height:30px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 260) < 1,
                $"Expected content width 260 (300 - 2*20), got {box.ContentRect.Width}");
        }

        // [CSS-UI §3.2] border-box: height:200 padding:20 => content height = 200 - 40 = 160
        [Fact]
        public void FlexContainer_BorderBoxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;box-sizing:border-box;width:100px;height:200px;padding:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 160) < 1,
                $"Expected content height 160 (200 - 2*20), got {box.ContentRect.Height}");
        }

        // [CSS2 §10.3.3] margin:auto on block-level flex container centers horizontally
        [Fact]
        public void FlexContainer_MarginAutoCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;width:200px;height:30px;margin:0 auto'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.X - 100) < 1,
                $"Expected centered at X=100, got {box.ContentRect.X}");
        }

        // [CSS2 §10.3.3] margin-left:auto pushes block to right
        [Fact]
        public void FlexContainer_MarginLeftAutoPushesRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;width:100px;height:30px;margin-left:auto'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.X - 300) < 1,
                $"Expected right-aligned at X=300, got {box.ContentRect.X}");
        }

        // [CSS2 §10.3.3] auto width fills containing block: parent=300 => width=300
        [Fact]
        public void FlexContainer_AutoWidthFillsParent300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='display:flex;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 300) < 1,
                $"Expected auto width to fill 300, got {box.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width fills containing block: parent=400 => width=400
        [Fact]
        public void FlexContainer_AutoWidthFillsParent400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 400) < 1,
                $"Expected auto width to fill 400, got {box.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §3] inline-flex shrinks to fit content
        [Fact]
        public void InlineFlex_ShrinkToFit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-flex'>
                        <div style='width:80px;height:30px'></div>
                        <div style='width:60px;height:30px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 140) < 2,
                $"Expected inline-flex to shrink to 140, got {box.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §3] inline-flex with explicit width overrides shrink-to-fit
        [Fact]
        public void InlineFlex_ExplicitWidthOverridesShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-flex;width:250px'>
                        <div style='width:80px;height:30px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 250) < 1,
                $"Expected explicit width 250 on inline-flex, got {box.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] items inside explicit-width container get correct available space
        [Fact]
        public void FlexContainer_Width200_ItemGrowsFull()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='t' style='flex-grow:1;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 200) < 1,
                $"Expected item to grow to 200, got {box.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] items inside explicit-height container stretch to full cross size
        [Fact]
        public void FlexContainer_Height150_ItemStretchesFull()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;height:150px'>
                    <div id='t' style='width:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 150) < 1,
                $"Expected item to stretch to 150, got {box.ContentRect.Height}");
        }

        // [CSS-UI §3.2] border-box with border: width:300 border:10px padding:20 => content = 300 - 60 = 240
        [Fact]
        public void FlexContainer_BorderBoxWithBorderAndPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;box-sizing:border-box;width:300px;height:30px;border:10px solid black;padding:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 240) < 1,
                $"Expected content width 240 (300 - 2*10 - 2*20), got {box.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] width+height together define both axes
        [Fact]
        public void FlexContainer_Width300_Height100_BothAxes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:300px;height:100px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 300) < 1,
                $"Expected width 300, got {box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 100) < 1,
                $"Expected height 100, got {box.ContentRect.Height}");
        }
    }
}
