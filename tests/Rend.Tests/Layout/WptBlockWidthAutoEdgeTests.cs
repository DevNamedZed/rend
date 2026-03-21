using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS2 §10.3.3 block-level auto width resolution: auto width fills
    /// containing block minus margins/padding/border, percentage width resolution,
    /// calc() expressions, and auto width behavior in flex/grid/float/inline-block contexts.
    /// </summary>
    public class WptBlockWidthAutoEdgeTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockWidthAutoEdgeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.3.3] auto width fills viewport (400px default)
        [Fact]
        public void AutoWidth_FillsViewport_400()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 1,
                $"Expected 400, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width fills parent at 300px
        [Fact]
        public void AutoWidth_FillsParent_300()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'><div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1,
                $"Expected 300, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width fills parent at 200px
        [Fact]
        public void AutoWidth_FillsParent_200()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'><div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected 200, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width minus margin-left: 400 - 50 = 350
        [Fact]
        public void AutoWidth_MinusMarginLeft_50()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='margin-left:50px;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 350) < 1,
                $"Expected 350, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width minus margin-right: 400 - 50 = 350
        [Fact]
        public void AutoWidth_MinusMarginRight_50()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='margin-right:50px;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 350) < 1,
                $"Expected 350, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width minus margin: 0 30px → 400 - 60 = 340
        [Fact]
        public void AutoWidth_MinusMarginHorizontal_30()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='margin:0 30px;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 340) < 1,
                $"Expected 340, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width minus padding: 0 20px → 400 - 40 = 360
        [Fact]
        public void AutoWidth_MinusPaddingHorizontal_20()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='padding:0 20px;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 360) < 1,
                $"Expected 360, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width minus border: 10px solid → 400 - 20 = 380
        [Fact]
        public void AutoWidth_MinusBorder_10()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='border:10px solid black;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 380) < 1,
                $"Expected 380, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width minus all combined:
        // 400 - margin(10+10) - padding(5+5) - border(2+2) = 400 - 34 = 366
        [Fact]
        public void AutoWidth_MinusAllCombined()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='margin:0 10px;padding:0 5px;border:2px solid black;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 366) < 1,
                $"Expected 366, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width in nested (2 levels): outer 300px, inner auto fills 300px
        [Fact]
        public void AutoWidth_Nested_TwoLevels()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='height:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1,
                $"Expected 300, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width in nested (3 levels): 400 → 300 → 200, inner auto fills 200px
        [Fact]
        public void AutoWidth_Nested_ThreeLevels()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                    <div style='width:300px'>
                        <div style='width:200px'>
                            <div id='t' style='height:20px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected 200, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.2] percentage width resolves against parent: 50% of 400 = 200
        [Fact]
        public void PercentageWidth_50_Of_400()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:50%;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected 200, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.2] percentage width: 50% of 200 = 100
        [Fact]
        public void PercentageWidth_50_Of_200()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'><div id='t' style='width:50%;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"Expected 100, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.2] percentage width: 25% of 400 = 100
        [Fact]
        public void PercentageWidth_25_Of_400()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:25%;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"Expected 100, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.2] percentage width: 75% of 400 = 300
        [Fact]
        public void PercentageWidth_75_Of_400()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:75%;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1,
                $"Expected 300, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.2] percentage width: 100% of 300 = 300
        [Fact]
        public void PercentageWidth_100_Of_300()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'><div id='t' style='width:100%;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1,
                $"Expected 300, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc(50% - 20px) of 400 = 200 - 20 = 180
        [Fact]
        public void CalcWidth_50Percent_Minus_20px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:calc(50% - 20px);height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 1,
                $"Expected 180, got {target.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc(100% - 60px) of 400 = 400 - 60 = 340
        [Fact]
        public void CalcWidth_100Percent_Minus_60px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:calc(100% - 60px);height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 340) < 1,
                $"Expected 340, got {target.ContentRect.Width}");
        }

        // [CSS-FLEX §9.2] auto width inside flex item fills flex item width
        [Fact]
        public void AutoWidth_InsideFlexItem_FillsFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                    <div style='display:flex;width:400px'>
                        <div style='flex:1'>
                            <div id='t' style='height:20px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 1,
                $"Expected 400, got {target.ContentRect.Width}");
        }

        // [CSS-GRID §7.1] auto width inside grid cell fills grid cell
        [Fact]
        public void AutoWidth_InsideGridCell_FillsGridCell()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                    <div style='display:grid;grid-template-columns:200px;width:400px'>
                        <div>
                            <div id='t' style='height:20px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected 200, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.5] auto width inside inline-block shrinks to fit content
        [Fact]
        public void AutoWidth_InsideInlineBlock_ShrinksToFit()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                    <div style='display:inline-block'>
                        <div id='t' style='width:80px;height:20px'></div>
                    </div>
                </body>");
            var inlineBlock = LayoutTestHelper.FindByTag(root, "div");
            Assert.NotNull(inlineBlock);
            // The inline-block shrinks to fit its content (80px child)
            Assert.True(inlineBlock!.ContentRect.Width <= 81,
                $"Expected inline-block to shrink to ~80, got {inlineBlock.ContentRect.Width}");
        }

        // [CSS2 §10.3.5] auto width inside float shrinks to fit content
        [Fact]
        public void AutoWidth_InsideFloat_ShrinksToFit()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                    <div style='float:left'>
                        <div id='t' style='width:120px;height:20px'></div>
                    </div>
                </body>");
            var floatBox = LayoutTestHelper.FindByTag(root, "div");
            Assert.NotNull(floatBox);
            // The float shrinks to fit its content (120px child)
            Assert.True(floatBox!.ContentRect.Width <= 121,
                $"Expected float to shrink to ~120, got {floatBox.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width with asymmetric margins: margin-left:30 margin-right:70
        // 400 - 30 - 70 = 300
        [Fact]
        public void AutoWidth_AsymmetricMargins()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='margin-left:30px;margin-right:70px;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1,
                $"Expected 300, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width with padding+border+margin combined in nested container
        // Parent 300px, child: margin 10+10, padding 15+15, border 5+5 → 300 - 60 = 240
        [Fact]
        public void AutoWidth_AllCombined_InNestedContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='margin:0 10px;padding:0 15px;border:5px solid black;height:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 240) < 1,
                $"Expected 240, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width X position accounts for margin-left
        [Fact]
        public void AutoWidth_ContentX_WithMarginLeft()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='margin-left:40px;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 40) < 1,
                $"Expected X=40, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 360) < 1,
                $"Expected width=360, got {target.ContentRect.Width}");
        }
    }
}
