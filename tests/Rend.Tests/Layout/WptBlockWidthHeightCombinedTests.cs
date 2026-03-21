using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests covering combined width and height property interactions on block elements:
    /// fixed pixel dimensions, percentage resolution, calc(), min/max constraints,
    /// box-sizing, viewport units, em units, fit-content, auto margins, sibling
    /// positioning, and nested percentage resolution on both axes.
    /// </summary>
    public class WptBlockWidthHeightCombinedTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockWidthHeightCombinedTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.2/10.5] fixed pixel width and height
        [Fact]
        public void FixedWidth100_FixedHeight50()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"Expected width=100, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 1,
                $"Expected height=50, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FixedWidth200_FixedHeight100()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:200px;height:100px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected width=200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 1,
                $"Expected height=100, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FixedWidth300_FixedHeight150()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:300px;height:150px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1,
                $"Expected width=300, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 150) < 1,
                $"Expected height=150, got {target.ContentRect.Height}");
        }

        // [CSS2 §10.2] percentage width resolves against containing block width
        [Fact]
        public void PercentWidth50_AutoHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:50%;height:auto'>" +
                "<div style='height:30px'></div></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected width=200 (50% of 400), got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 30) < 2,
                $"Expected height=30 (auto from child), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.5] percentage height requires containing block with definite height
        [Fact]
        public void AutoWidth_PercentHeight50()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px;height:200px'>" +
                "<div id='t' style='width:auto;height:50%'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 1,
                $"Expected width=400 (auto fills parent), got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 1,
                $"Expected height=100 (50% of 200), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.2/10.5] both axes percentage
        [Fact]
        public void PercentWidth50_PercentHeight50()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px;height:200px'>" +
                "<div id='t' style='width:50%;height:50%'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected width=200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 1,
                $"Expected height=100, got {target.ContentRect.Height}");
        }

        // [CSS Values §8] calc() on both axes
        [Fact]
        public void CalcWidth_CalcHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px;height:200px'>" +
                "<div id='t' style='width:calc(50% - 20px);height:calc(50% + 20px)'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2,
                $"Expected width=180 (50%*400-20), got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 120) < 2,
                $"Expected height=120 (50%*200+20), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.4] min-width and min-height
        [Fact]
        public void MinWidth100_MinHeight50_EmptyBlock()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='min-width:100px;min-height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width >= 100,
                $"Expected width>=100 (min-width), got {target.ContentRect.Width}");
            Assert.True(target.ContentRect.Height >= 49,
                $"Expected height>=50 (min-height), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.4] max-width and max-height clamp content
        [Fact]
        public void MaxWidth200_MaxHeight100_Clamps()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='max-width:200px;max-height:100px'>" +
                "<div style='width:500px;height:500px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width <= 201,
                $"Expected width<=200 (max-width), got {target.ContentRect.Width}");
            Assert.True(target.ContentRect.Height <= 101,
                $"Expected height<=100 (max-height), got {target.ContentRect.Height}");
        }

        // [CSS-UI §3.2] border-box: width/height include padding+border
        [Fact]
        public void BorderBox_Width200_Height100_Padding20()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='box-sizing:border-box;width:200px;height:100px;padding:20px'>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentWidth = 200 - 20 - 20;
            float expectedContentHeight = 100 - 20 - 20;
            Assert.True(System.Math.Abs(target.ContentRect.Width - expectedContentWidth) < 1,
                $"Expected content width=160, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - expectedContentHeight) < 1,
                $"Expected content height=60, got {target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.BorderRect.Width - 200) < 1,
                $"Expected border-box width=200, got {target.BorderRect.Width}");
            Assert.True(System.Math.Abs(target.BorderRect.Height - 100) < 1,
                $"Expected border-box height=100, got {target.BorderRect.Height}");
        }

        // [CSS2 §10.2] content-box: width/height are content area only
        [Fact]
        public void ContentBox_Width200_Height100_Padding20()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='box-sizing:content-box;width:200px;height:100px;padding:20px'>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected content width=200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 1,
                $"Expected content height=100, got {target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.BorderRect.Width - 240) < 1,
                $"Expected border-box width=240, got {target.BorderRect.Width}");
            Assert.True(System.Math.Abs(target.BorderRect.Height - 140) < 1,
                $"Expected border-box height=140, got {target.BorderRect.Height}");
        }

        // [CSS2 §10.2/10.5] auto width and auto height with no content
        [Fact]
        public void AutoWidth_AutoHeight_Empty()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:auto;height:auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 1,
                $"Expected width=400 (auto fills parent), got {target.ContentRect.Width}");
            Assert.True(target.ContentRect.Height < 1,
                $"Expected height=0 (no content), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.6.3] auto height wraps children
        [Fact]
        public void AutoWidth_AutoHeight_WithChildren()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:auto;height:auto'>" +
                "<div style='height:40px'></div>" +
                "<div style='height:60px'></div></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 1,
                $"Expected width=400, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height=100 (40+60), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.2/10.5] fixed width with zero height
        [Fact]
        public void FixedWidth100_Height0()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:0'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"Expected width=100, got {target.ContentRect.Width}");
            Assert.True(target.ContentRect.Height < 1,
                $"Expected height=0, got {target.ContentRect.Height}");
        }

        // [CSS2 §10.2/10.5] zero width with fixed height
        [Fact]
        public void Width0_FixedHeight100()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:0;height:100px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width < 1,
                $"Expected width=0, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 1,
                $"Expected height=100, got {target.ContentRect.Height}");
        }

        // [CSS Values §5.1.2] viewport width unit
        [Fact]
        public void VwWidth_VhHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:50vw;height:25vh'></div></body>",
                viewportWidth: 400, viewportHeight: 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Expected width=200 (50vw of 400), got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 75) < 2,
                $"Expected height=75 (25vh of 300), got {target.ContentRect.Height}");
        }

        // [CSS Values §5.1.1] em-based width and height
        [Fact]
        public void EmWidth_EmHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0;font-size:16px'>" +
                "<div id='t' style='width:10em;height:5em'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 2,
                $"Expected width=160 (10em * 16px), got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2,
                $"Expected height=80 (5em * 16px), got {target.ContentRect.Height}");
        }

        // [CSS Sizing §5.1] fit-content width with auto height
        [Fact]
        public void FitContentWidth_AutoHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:fit-content;height:auto'>" +
                "<div style='width:120px;height:30px'></div></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width <= 121,
                $"Expected fit-content width<=120 (child width), got {target.ContentRect.Width}");
            Assert.True(target.ContentRect.Width >= 119,
                $"Expected fit-content width>=120, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 30) < 2,
                $"Expected height=30 (from child), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.3.3] margin:auto centers block with fixed width and height
        [Fact]
        public void MarginAuto_WithFixedWidthHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px;height:300px'>" +
                "<div id='t' style='width:200px;height:100px;margin:0 auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2,
                $"Expected X=100 (centered in 400px), got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected width=200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 1,
                $"Expected height=100, got {target.ContentRect.Height}");
        }

        // [CSS2 §10.6.3] sibling Y position after a block with fixed width and height
        [Fact]
        public void SiblingY_AfterFixedWidthHeightBlock()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div style='width:200px;height:80px'></div>" +
                "<div id='sibling' style='width:300px;height:40px'></div></div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 80) < 2,
                $"Expected sibling Y=80 (after 80px block), got {sibling.ContentRect.Y}");
        }

        // [CSS2 §10.2/10.5] nested percentage resolution on both axes
        [Fact]
        public void NestedPercentage_BothAxes()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px;height:300px'>" +
                "<div style='width:50%;height:50%'>" +
                "<div id='inner' style='width:50%;height:50%'></div></div></div></body>");
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            Assert.True(System.Math.Abs(inner.ContentRect.Width - 100) < 2,
                $"Expected width=100 (50% of 50% of 400), got {inner.ContentRect.Width}");
            Assert.True(System.Math.Abs(inner.ContentRect.Height - 75) < 2,
                $"Expected height=75 (50% of 50% of 300), got {inner.ContentRect.Height}");
        }

        // [CSS2 §10.4] min-width overrides smaller width
        [Fact]
        public void MinWidth_OverridesSmallWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:50px;min-width:100px;height:40px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width >= 99,
                $"Expected width>=100 (min-width overrides), got {target.ContentRect.Width}");
        }

        // [CSS2 §10.4] max-width overrides larger width
        [Fact]
        public void MaxWidth_OverridesLargeWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:300px;max-width:200px;height:40px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width <= 201,
                $"Expected width<=200 (max-width clamps), got {target.ContentRect.Width}");
        }

        // [CSS2 §10.7] min-height overrides smaller height
        [Fact]
        public void MinHeight_OverridesSmallHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:20px;min-height:80px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height >= 79,
                $"Expected height>=80 (min-height overrides), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.7] max-height overrides larger height
        [Fact]
        public void MaxHeight_OverridesLargeHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:300px;max-height:100px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height <= 101,
                $"Expected height<=100 (max-height clamps), got {target.ContentRect.Height}");
        }

        // [CSS-UI §3.2] border-box with border and padding
        [Fact]
        public void BorderBox_WithBorderAndPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='box-sizing:border-box;width:200px;height:100px;" +
                "padding:10px;border:5px solid'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentWidth = 200 - 10 - 10 - 5 - 5;
            float expectedContentHeight = 100 - 10 - 10 - 5 - 5;
            Assert.True(System.Math.Abs(target.ContentRect.Width - expectedContentWidth) < 1,
                $"Expected content width={expectedContentWidth}, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - expectedContentHeight) < 1,
                $"Expected content height={expectedContentHeight}, got {target.ContentRect.Height}");
        }

        // [CSS2 §10.3.3] auto width with fixed height fills containing block
        [Fact]
        public void AutoWidth_FixedHeight_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:350px'>" +
                "<div id='t' style='height:60px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 350) < 1,
                $"Expected width=350 (fills container), got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 1,
                $"Expected height=60, got {target.ContentRect.Height}");
        }

        // [CSS2 §10.6.3] auto height with multiple children and margins
        [Fact]
        public void AutoHeight_ChildrenWithMargins()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px;overflow:hidden'>" +
                "<div id='t' style='width:200px'>" +
                "<div style='height:30px;margin-bottom:10px'></div>" +
                "<div style='height:50px;margin-top:10px'></div></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 90) < 2,
                $"Expected height=90 (30+collapse(10,10)+50), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.2/10.5] sibling stacking: three blocks with different widths and heights
        [Fact]
        public void ThreeSiblings_DifferentDimensions()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='a' style='width:100px;height:30px'></div>" +
                "<div id='b' style='width:200px;height:40px'></div>" +
                "<div id='c' style='width:300px;height:50px'></div></div></body>");
            var blockA = LayoutTestHelper.FindById(root, "a")!;
            var blockB = LayoutTestHelper.FindById(root, "b")!;
            var blockC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(blockA.ContentRect.Y - 0) < 1);
            Assert.True(System.Math.Abs(blockB.ContentRect.Y - 30) < 2,
                $"Expected B.Y=30, got {blockB.ContentRect.Y}");
            Assert.True(System.Math.Abs(blockC.ContentRect.Y - 70) < 2,
                $"Expected C.Y=70, got {blockC.ContentRect.Y}");
        }

        // [CSS Values §8] calc() with mixed units on width
        [Fact]
        public void CalcWidth_MixedUnits()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:calc(100% - 50px);height:40px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 350) < 2,
                $"Expected width=350 (100%-50px of 400), got {target.ContentRect.Width}");
        }

        // [CSS Values §8] calc() with mixed units on height
        [Fact]
        public void CalcHeight_MixedUnits()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px;height:200px'>" +
                "<div id='t' style='width:100px;height:calc(100% - 40px)'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 160) < 2,
                $"Expected height=160 (100%-40px of 200), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.4] min-width and max-width interact: min wins over max when min > max
        [Fact]
        public void MinWidth_GreaterThan_MaxWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='width:50px;min-width:200px;max-width:150px;height:40px'>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width >= 199,
                $"Expected width>=200 (min-width wins over max-width), got {target.ContentRect.Width}");
        }

        // [CSS2 §10.7] min-height and max-height interact: min wins over max when min > max
        [Fact]
        public void MinHeight_GreaterThan_MaxHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='width:100px;height:20px;min-height:150px;max-height:100px'>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height >= 149,
                $"Expected height>=150 (min-height wins over max-height), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.2/10.5] percentage width with padding in content-box
        [Fact]
        public void PercentWidth_WithPadding_ContentBox()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:50%;padding:10px;height:40px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected content width=200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.PaddingLeft - 10) < 1);
            Assert.True(System.Math.Abs(target.PaddingRight - 10) < 1);
        }

        // [CSS2 §10.2/10.5] percentage width with border-box sizing
        [Fact]
        public void PercentWidth_BorderBox()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='box-sizing:border-box;width:50%;padding:20px;height:60px'>" +
                "</div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.BorderRect.Width - 200) < 1,
                $"Expected border-box width=200 (50% of 400), got {target.BorderRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 1,
                $"Expected content width=160 (200-20-20), got {target.ContentRect.Width}");
        }

        // [CSS Values §5.1.1] em-based dimensions with custom font-size
        [Fact]
        public void EmDimensions_CustomFontSize()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='font-size:20px'>" +
                "<div id='t' style='width:5em;height:3em'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"Expected width=100 (5em * 20px), got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2,
                $"Expected height=60 (3em * 20px), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.3.3] block with margin, padding, border and fixed dimensions
        [Fact]
        public void FixedDimensions_WithMarginPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:200px;height:100px;margin:10px;padding:5px;border:2px solid'>" +
                "</div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected content width=200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 1,
                $"Expected content height=100, got {target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 17) < 1,
                $"Expected X=17 (margin:10 + border:2 + padding:5), got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 17) < 2,
                $"Expected Y=17, got {target.ContentRect.Y}");
        }

        // [CSS2 §10.6.3] sibling after margin-bottom of sized block
        [Fact]
        public void SiblingY_AfterMarginBottom()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div style='width:200px;height:60px;margin-bottom:20px'></div>" +
                "<div id='sibling' style='width:300px;height:30px'></div></div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 80) < 2,
                $"Expected sibling Y=80 (60+20 margin), got {sibling.ContentRect.Y}");
        }

        // [CSS2 §10.5] deeply nested percentage height chain
        [Fact]
        public void DeeplyNested_PercentageHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='width:400px;height:400px'>" +
                "<div style='width:100%;height:50%'>" +
                "<div style='width:100%;height:50%'>" +
                "<div id='deep' style='width:100%;height:50%'></div></div></div></div></body>");
            var deep = LayoutTestHelper.FindById(root, "deep")!;
            Assert.True(System.Math.Abs(deep.ContentRect.Height - 50) < 2,
                $"Expected height=50 (50%^3 of 400), got {deep.ContentRect.Height}");
        }

        // [CSS Values §5.1.2] vmin and vmax units
        [Fact]
        public void VminWidth_VmaxHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:50vmin;height:25vmax'></div></body>",
                viewportWidth: 400, viewportHeight: 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"Expected width=150 (50vmin of min(400,300)=300), got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height=100 (25vmax of max(400,300)=400), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.2/10.5] width:auto fills but height:auto collapses when empty
        [Fact]
        public void AutoWidthFills_AutoHeightCollapses()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:250px;height:200px'>" +
                "<div id='t'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 250) < 1,
                $"Expected width=250, got {target.ContentRect.Width}");
            Assert.True(target.ContentRect.Height < 1,
                $"Expected height=0, got {target.ContentRect.Height}");
        }

        // [CSS2 §10.4/10.7] min and max constraints together on both axes
        [Fact]
        public void MinMax_BothAxes_Constrained()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='width:50px;height:30px;min-width:100px;min-height:60px;" +
                "max-width:300px;max-height:200px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"Expected width=100 (clamped by min-width), got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 1,
                $"Expected height=60 (clamped by min-height), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.4/10.7] max clamps on both axes
        [Fact]
        public void MaxClamps_BothAxes()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='t' style='width:500px;height:400px;max-width:200px;max-height:100px'>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected width=200 (clamped by max-width), got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 1,
                $"Expected height=100 (clamped by max-height), got {target.ContentRect.Height}");
        }

        // [CSS Sizing §5.1] fit-content shrinks to widest child
        [Fact]
        public void FitContent_ShrinkToWidestChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='width:fit-content'>" +
                "<div style='width:80px;height:20px'></div>" +
                "<div style='width:150px;height:20px'></div>" +
                "<div style='width:100px;height:20px'></div></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"Expected width=150 (widest child), got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2,
                $"Expected height=60 (3*20), got {target.ContentRect.Height}");
        }
    }
}
