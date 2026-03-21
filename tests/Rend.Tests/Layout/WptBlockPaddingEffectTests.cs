using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests covering the effect of padding on block layout: child offset,
    /// available width reduction, shorthand expansion, percentage resolution,
    /// box-sizing interaction, and padding with flex/grid containers.
    /// </summary>
    public class WptBlockPaddingEffectTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockPaddingEffectTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §8.4] padding-left offsets child X position
        [Fact]
        public void PaddingLeft_OffsetsChildX()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px;padding-left:25px'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 25) < 1,
                $"Expected child X=25, got {target.ContentRect.X}");
        }

        // [CSS2 §8.4] padding-top offsets child Y position
        [Fact]
        public void PaddingTop_OffsetsChildY()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px;padding-top:30px'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 1,
                $"Expected child Y=30, got {target.ContentRect.Y}");
        }

        // [CSS2 §8.4] padding-right does not affect child X position
        [Fact]
        public void PaddingRight_DoesNotAffectChildX()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px;padding-right:40px'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 0) < 1,
                $"Expected child X=0, got {target.ContentRect.X}");
        }

        // [CSS2 §8.4] padding-bottom does not affect child Y position
        [Fact]
        public void PaddingBottom_DoesNotAffectChildY()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px;padding-bottom:40px'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 0) < 1,
                $"Expected child Y=0, got {target.ContentRect.Y}");
        }

        // [CSS2 §10.3.3] padding reduces available width for auto-width children
        // In content-box (default), parent width:300px means content area is 300px, so child fills 300px.
        // To test padding reducing child width, parent needs border-box or no explicit width.
        [Fact]
        public void Padding_ReducesChildAutoWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='box-sizing:border-box;width:300px;padding-left:20px;padding-right:30px'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 250) < 1,
                $"Expected child width=250 (300-20-30), got {target.ContentRect.Width}");
        }

        // [CSS2 §8.4] padding shorthand with 1 value applies to all sides
        [Fact]
        public void PaddingShorthand_OneValue()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0;overflow:hidden'><div id='t' style='padding:15px;width:50px;height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(15, target.PaddingTop);
            Assert.Equal(15, target.PaddingRight);
            Assert.Equal(15, target.PaddingBottom);
            Assert.Equal(15, target.PaddingLeft);
        }

        // [CSS2 §8.4] padding shorthand with 2 values: top/bottom, left/right
        [Fact]
        public void PaddingShorthand_TwoValues()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0;overflow:hidden'><div id='t' style='padding:12px 24px;width:50px;height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(12, target.PaddingTop);
            Assert.Equal(24, target.PaddingRight);
            Assert.Equal(12, target.PaddingBottom);
            Assert.Equal(24, target.PaddingLeft);
        }

        // [CSS2 §8.4] padding shorthand with 3 values: top, left/right, bottom
        [Fact]
        public void PaddingShorthand_ThreeValues()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0;overflow:hidden'><div id='t' style='padding:8px 16px 24px;width:50px;height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(8, target.PaddingTop);
            Assert.Equal(16, target.PaddingRight);
            Assert.Equal(24, target.PaddingBottom);
            Assert.Equal(16, target.PaddingLeft);
        }

        // [CSS2 §8.4] padding shorthand with 4 values: top, right, bottom, left
        [Fact]
        public void PaddingShorthand_FourValues()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0;overflow:hidden'><div id='t' style='padding:5px 10px 15px 20px;width:50px;height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(5, target.PaddingTop);
            Assert.Equal(10, target.PaddingRight);
            Assert.Equal(15, target.PaddingBottom);
            Assert.Equal(20, target.PaddingLeft);
        }

        // [CSS2 §8.4] padding percentage resolves against containing block width
        [Fact]
        public void PaddingPercent_ResolvesAgainstContainingBlockWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div id='t' style='padding-left:10%;padding-right:10%;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.PaddingLeft - 20) < 1,
                $"Expected padding-left=20 (10% of 200), got {target.PaddingLeft}");
            Assert.True(System.Math.Abs(target.PaddingRight - 20) < 1,
                $"Expected padding-right=20 (10% of 200), got {target.PaddingRight}");
        }

        // [CSS2 §8.4] padding-top percentage also resolves against containing block WIDTH (not height)
        [Fact]
        public void PaddingTopPercent_ResolvesAgainstWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px;height:500px'>" +
                "<div id='t' style='padding-top:20%;padding-bottom:10%;height:0'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.PaddingTop - 60) < 1,
                $"Expected padding-top=60 (20% of 300), got {target.PaddingTop}");
            Assert.True(System.Math.Abs(target.PaddingBottom - 30) < 1,
                $"Expected padding-bottom=30 (10% of 300), got {target.PaddingBottom}");
        }

        // [CSS-UI §3.2] padding with box-sizing:border-box does not change content-box dimensions
        [Fact]
        public void Padding_WithBorderBox_ReducesContentWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;height:100px;padding:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 1,
                $"Expected content width=160 (200-20-20), got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 1,
                $"Expected content height=60 (100-20-20), got {target.ContentRect.Height}");
        }

        // [CSS2 §10.3.3] padding with explicit width in content-box: padding is outside width
        [Fact]
        public void Padding_WithExplicitWidth_ContentBox()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:150px;height:80px;padding:10px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"Expected content width=150, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.PaddingRect.Width - 170) < 1,
                $"Expected padding-box width=170 (150+10+10), got {target.PaddingRect.Width}");
        }

        // [CSS-UI §3.2] padding with explicit width in border-box: padding is inside width
        [Fact]
        public void Padding_WithExplicitWidth_BorderBox()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='box-sizing:border-box;width:150px;height:80px;padding:10px;border:2px solid'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // content = 150 - 2*10(pad) - 2*2(border) = 126
            Assert.True(System.Math.Abs(target.ContentRect.Width - 126) < 1,
                $"Expected content width=126, got {target.ContentRect.Width}");
        }

        // [CSS2 §8.4] padding on nested blocks: child X/Y offset is cumulative
        [Fact]
        public void Padding_OnNestedBlocks_CumulativeOffset()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='padding:20px;width:300px'>" +
                "<div style='padding:15px'><div id='t' style='height:10px'></div></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 35) < 1,
                $"Expected child X=35 (20+15), got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 35) < 1,
                $"Expected child Y=35 (20+15), got {target.ContentRect.Y}");
        }

        // [CSS2 §8.4] padding combined with margin on child
        [Fact]
        public void Padding_WithChildMargin()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px;padding:20px'>" +
                "<div id='t' style='margin:10px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 1,
                $"Expected child X=30 (20pad+10margin), got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 1,
                $"Expected child Y=30 (20pad+10margin), got {target.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9] padding on flex container offsets flex items
        [Fact]
        public void Padding_OnFlexContainer_OffsetsItems()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:flex;width:300px;padding:25px'>" +
                "<div id='t' style='width:50px;height:30px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 25) < 1,
                $"Expected flex item X=25, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 25) < 1,
                $"Expected flex item Y=25, got {target.ContentRect.Y}");
        }

        // [CSS-GRID §11] padding on grid container offsets grid items
        [Fact]
        public void Padding_OnGridContainer_OffsetsItems()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;width:300px;padding:30px'>" +
                "<div id='t' style='height:40px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 1,
                $"Expected grid item X=30, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 1,
                $"Expected grid item Y=30, got {target.ContentRect.Y}");
        }

        // [CSS2 §8.4] padding:0 has no effect on child position
        [Fact]
        public void PaddingZero_NoEffect()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px;padding:0'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 0) < 1,
                $"Expected child X=0, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 0) < 1,
                $"Expected child Y=0, got {target.ContentRect.Y}");
        }

        // [CSS2 §8.4] large padding with border-box significantly reduces content area for children
        [Fact]
        public void LargePadding_ReducesAvailableWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='box-sizing:border-box;width:400px;padding:100px'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected child width=200 (400-100-100), got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 1,
                $"Expected child X=100, got {target.ContentRect.X}");
        }

        // [CSS2 §8.4] asymmetric padding: different values on each side with border-box
        [Fact]
        public void AsymmetricPadding_DifferentSides()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='box-sizing:border-box;width:300px;padding-left:10px;padding-right:40px;padding-top:5px;padding-bottom:50px'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 1,
                $"Expected child X=10, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 5) < 1,
                $"Expected child Y=5, got {target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 250) < 1,
                $"Expected child width=250 (300-10-40), got {target.ContentRect.Width}");
        }

        // [CSS2 §10.4] padding with min-width: child respects min-width constraint
        [Fact]
        public void Padding_WithMinWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='box-sizing:border-box;width:200px;padding-left:20px;padding-right:20px'>" +
                "<div id='t' style='min-width:180px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Parent content area = 200-20-20 = 160px, but child min-width:180px overrides
            Assert.True(target.ContentRect.Width >= 179,
                $"Expected child width >= 180 (min-width), got {target.ContentRect.Width}");
        }

        // [CSS2 §10.4] padding with max-width: child width constrained by max-width
        [Fact]
        public void Padding_WithMaxWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px;padding:10px'>" +
                "<div id='t' style='max-width:200px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"Expected child width=200 (max-width clamp), got {target.ContentRect.Width}");
        }

        // [CSS2 §8.4] padding contributes to parent auto height
        [Fact]
        public void Padding_ContributesToParentAutoHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='parent' style='width:200px;padding-top:15px;padding-bottom:25px'>" +
                "<div style='height:50px'></div></div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.PaddingRect.Height - 90) < 1,
                $"Expected parent padding-box height=90 (15+50+25), got {parent.PaddingRect.Height}");
        }

        // [CSS2 §8.3.1] padding prevents margin collapse between parent and child
        [Fact]
        public void Padding_PreventsMarginCollapse()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='parent' style='padding-top:1px;width:200px'>" +
                "<div id='child' style='margin-top:30px;height:20px'></div></div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            // Parent border rect at Y=0, content edge at Y=1 (padding-top:1px).
            // Child margin-top:30px is inside parent, so child Y=1+30=31.
            // Without padding, margin would collapse through parent, pushing parent down.
            Assert.True(System.Math.Abs(parent.BorderRect.Y - 0) < 1,
                $"Parent border edge should stay at Y=0, got {parent.BorderRect.Y}");
            Assert.True(child.ContentRect.Y >= 31,
                $"Child should be at Y>=31 (1px padding + 30px margin), got {child.ContentRect.Y}");
        }

        // [CSS2 §8.4] left+right padding both reduce available width for child (border-box parent)
        [Fact]
        public void LeftRightPadding_BothReduceChildWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='box-sizing:border-box;width:400px;padding-left:50px;padding-right:100px'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 250) < 1,
                $"Expected child width=250 (400-50-100), got {target.ContentRect.Width}");
        }

        // [CSS2 §8.4] padding-left on child adds to parent padding offset
        [Fact]
        public void ChildPadding_AddsToParentPaddingOffset()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px;padding-left:20px'>" +
                "<div style='padding-left:15px'><div id='t' style='height:10px'></div></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 35) < 1,
                $"Expected grandchild X=35 (20+15), got {target.ContentRect.X}");
        }

        // [CSS2 §8.4] padding on a block with border: child offset includes both
        [Fact]
        public void Padding_WithBorder_CombinedOffset()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px;padding:15px;border:5px solid'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 1,
                $"Expected child X=20 (5border+15pad), got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 1,
                $"Expected child Y=20 (5border+15pad), got {target.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9] padding on flex container reduces available main size (border-box)
        [Fact]
        public void Padding_OnFlexContainer_ReducesMainSize()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:flex;box-sizing:border-box;width:300px;padding-left:30px;padding-right:30px'>" +
                "<div id='t' style='flex:1;height:30px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 240) < 1,
                $"Expected flex item width=240 (300-30-30), got {target.ContentRect.Width}");
        }

        // [CSS-GRID §11] padding on grid container reduces available width for tracks (border-box)
        [Fact]
        public void Padding_OnGridContainer_ReducesTrackSpace()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;box-sizing:border-box;width:300px;padding-left:25px;padding-right:25px;grid-template-columns:1fr'>" +
                "<div id='t' style='height:30px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 250) < 1,
                $"Expected grid item width=250 (300-25-25), got {target.ContentRect.Width}");
        }
    }
}
