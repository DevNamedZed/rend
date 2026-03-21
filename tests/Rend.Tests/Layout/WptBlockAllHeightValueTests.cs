using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive tests for every height value type on block-level elements:
    /// auto, fixed px, percentage, viewport units, calc(), min/max constraints,
    /// border-box, and auto height in flex/grid contexts.
    /// </summary>
    public class WptBlockAllHeightValueTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockAllHeightValueTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.6.3] auto height with no children = 0
        [Fact]
        public void HeightAuto_EmptyBlock_IsZero()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:200px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height < 1,
                $"Empty auto-height block should be 0 (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height with 1 child = child height
        [Fact]
        public void HeightAuto_OneChild_EqualsChildHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:200px;overflow:hidden'><div style='height:50px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2,
                $"Auto height with one 50px child should be 50 (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height with 2 children = sum of child heights
        [Fact]
        public void HeightAuto_TwoChildren_SumsHeights()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:200px;overflow:hidden'><div style='height:30px'></div><div style='height:40px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 70) < 2,
                $"Auto height with 30+40 children should be 70 (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] auto height with 3 children = sum of child heights
        [Fact]
        public void HeightAuto_ThreeChildren_SumsHeights()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:200px;overflow:hidden'><div style='height:20px'></div><div style='height:35px'></div><div style='height:15px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 70) < 2,
                $"Auto height with 20+35+15 children should be 70 (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.5] height:10px
        [Fact]
        public void HeightFixed_10px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 10) < 2);
        }

        // [CSS2 §10.5] height:20px
        [Fact]
        public void HeightFixed_20px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:20px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 20) < 2);
        }

        // [CSS2 §10.5] height:30px
        [Fact]
        public void HeightFixed_30px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 30) < 2);
        }

        // [CSS2 §10.5] height:40px
        [Fact]
        public void HeightFixed_40px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:40px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 40) < 2);
        }

        // [CSS2 §10.5] height:50px
        [Fact]
        public void HeightFixed_50px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 50) < 2);
        }

        // [CSS2 §10.5] height:60px
        [Fact]
        public void HeightFixed_60px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:60px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 60) < 2);
        }

        // [CSS2 §10.5] height:80px
        [Fact]
        public void HeightFixed_80px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:80px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 80) < 2);
        }

        // [CSS2 §10.5] height:100px
        [Fact]
        public void HeightFixed_100px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 100) < 2);
        }

        // [CSS2 §10.5] height:150px
        [Fact]
        public void HeightFixed_150px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:150px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 150) < 2);
        }

        // [CSS2 §10.5] height:200px
        [Fact]
        public void HeightFixed_200px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:200px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 200) < 2);
        }

        // [CSS2 §10.5] height:250px
        [Fact]
        public void HeightFixed_250px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:250px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 250) < 2);
        }

        // [CSS2 §10.5] height:300px
        [Fact]
        public void HeightFixed_300px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:300px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 300) < 2);
        }

        // [CSS2 §10.5] height:10% of 200px parent = 20px
        [Fact]
        public void HeightPercent_10_Of200()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='height:200px;width:200px'><div id='t' style='height:10%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 20) < 2,
                $"10% of 200 = 20 (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS2 §10.5] height:25% of 200px parent = 50px
        [Fact]
        public void HeightPercent_25_Of200()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='height:200px;width:200px'><div id='t' style='height:25%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 50) < 2,
                $"25% of 200 = 50 (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS2 §10.5] height:50% of 200px parent = 100px
        [Fact]
        public void HeightPercent_50_Of200()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='height:200px;width:200px'><div id='t' style='height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 100) < 2,
                $"50% of 200 = 100 (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS2 §10.5] height:75% of 200px parent = 150px
        [Fact]
        public void HeightPercent_75_Of200()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='height:200px;width:200px'><div id='t' style='height:75%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 150) < 2,
                $"75% of 200 = 150 (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS2 §10.5] height:100% of 200px parent = 200px
        [Fact]
        public void HeightPercent_100_Of200()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='height:200px;width:200px'><div id='t' style='height:100%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 200) < 2,
                $"100% of 200 = 200 (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS-VALUES §5.1.2] 50vh at viewport 300 = 150
        [Fact]
        public void HeightViewport_50vh_At300()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:50vh'></div></body>",
                viewportWidth: 400, viewportHeight: 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 150) < 2,
                $"50vh at 300 viewport = 150 (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS-VALUES §5.1.2] 100vh at viewport 300 = 300
        [Fact]
        public void HeightViewport_100vh_At300()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:100vh'></div></body>",
                viewportWidth: 400, viewportHeight: 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 300) < 2,
                $"100vh at 300 viewport = 300 (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS-VALUES §8.1] calc(50% + 20px) with 200px parent = 120px
        [Fact]
        public void HeightCalc_PercentPlusPx()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='height:200px;width:200px'><div id='t' style='height:calc(50% + 20px)'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 120) < 2,
                $"calc(50% + 20px) of 200 = 120 (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS-VALUES §8.1] calc(100px + 50px) = 150px
        [Fact]
        public void HeightCalc_PxPlusPx()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:calc(100px + 50px)'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 150) < 2,
                $"calc(100px + 50px) = 150 (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS2 §10.7] min-height:100px expands auto height
        [Fact]
        public void MinHeight_ExpandsAutoHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;min-height:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height >= 99,
                $"min-height:100 should expand empty block (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS2 §10.7] max-height:80px clamps explicit height
        [Fact]
        public void MaxHeight_ClampsExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:200px;max-height:80px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 80) < 2,
                $"max-height:80 should clamp 200 (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS2 §10.7] min-height > max-height: min-height wins
        [Fact]
        public void MinHeight_OverridesMaxHeight_WhenLarger()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:50px;min-height:150px;max-height:80px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height >= 149,
                $"min-height:150 should win over max-height:80 (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS2 §10.5] height:0 is valid
        [Fact]
        public void HeightFixed_Zero()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:0'></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height < 1,
                $"height:0 should be 0 (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS-UI §3.2] border-box: height:100px with padding:10px = content 80px
        [Fact]
        public void HeightBorderBox_SubtractsPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:100px;box-sizing:border-box;padding:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2,
                $"border-box height:100 minus padding:10 top+bottom = 80 content (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.3] auto height flex row = tallest child
        [Fact]
        public void HeightAuto_FlexRow_TallestChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='display:flex;flex-direction:row;width:200px'>" +
                "<div style='width:50px;height:30px'></div>" +
                "<div style='width:50px;height:60px'></div>" +
                "<div style='width:50px;height:45px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2,
                $"Flex row auto height = tallest child 60 (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.3] auto height flex column = sum of children
        [Fact]
        public void HeightAuto_FlexColumn_SumsChildren()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='display:flex;flex-direction:column;width:200px'>" +
                "<div style='height:30px'></div>" +
                "<div style='height:40px'></div>" +
                "<div style='height:20px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 90) < 2,
                $"Flex column auto height = sum 30+40+20 = 90 (got {target.ContentRect.Height})");
        }

        // [CSS-GRID §7.1] auto height grid = sum of row tracks
        [Fact]
        public void HeightAuto_Grid_EqualsRowTracks()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;width:200px'>" +
                "<div style='height:40px'></div>" +
                "<div style='height:50px'></div>" +
                "<div style='height:30px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 120) < 2,
                $"Grid auto height = sum of rows 40+50+30 = 120 (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.7] min-height on auto height with small content
        [Fact]
        public void MinHeight_WithAutoContent_SmallChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;min-height:100px;overflow:hidden'><div style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height >= 99,
                $"min-height:100 overrides 30px content (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS2 §10.7] max-height on auto height with tall content
        [Fact]
        public void MaxHeight_WithAutoContent_TallChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;max-height:80px;overflow:hidden'><div style='height:200px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 80) < 2,
                $"max-height:80 clamps 200px content (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS-UI §3.2] border-box with border and padding
        [Fact]
        public void HeightBorderBox_SubtractsBorderAndPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:100px;box-sizing:border-box;padding:10px;border:5px solid'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // border-box: 100 total = 5+10 + content + 10+5 → content = 70
            Assert.True(System.Math.Abs(target.ContentRect.Height - 70) < 2,
                $"border-box height:100 minus padding:10*2 + border:5*2 = 70 (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.3] flex row auto height with gap
        [Fact]
        public void HeightAuto_FlexRow_WithGap_TallestChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='display:flex;flex-direction:row;gap:10px;width:300px'>" +
                "<div style='width:50px;height:25px'></div>" +
                "<div style='width:50px;height:75px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Row direction: cross-axis height = tallest child; row gap does not affect height
            Assert.True(System.Math.Abs(target.ContentRect.Height - 75) < 2,
                $"Flex row auto height = tallest 75 (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.3] flex column auto height with gap
        [Fact]
        public void HeightAuto_FlexColumn_WithGap_SumsWithGaps()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='display:flex;flex-direction:column;gap:10px;width:200px'>" +
                "<div style='height:30px'></div>" +
                "<div style='height:40px'></div>" +
                "</div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Column direction: height = 30 + 10 + 40 = 80
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2,
                $"Flex column auto height = 30+10+40 = 80 (got {target.ContentRect.Height})");
        }
    }
}
