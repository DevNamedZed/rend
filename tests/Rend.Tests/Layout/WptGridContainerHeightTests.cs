using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Grid container height determination tests. Covers how the grid container
    /// resolves its own height from row tracks, content, gaps, explicit sizing,
    /// min/max constraints, padding, border, and nesting contexts.
    /// </summary>
    public class WptGridContainerHeightTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridContainerHeightTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §12.4] Auto height from single row track
        [Fact]
        public void AutoHeight_SingleRowTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:75px;width:100px'>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 75) < 2);
        }

        // [CSS-GRID §12.4] Auto height from multiple row tracks
        [Fact]
        public void AutoHeight_MultipleRowTracks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:30px 50px 20px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §12.4] Auto height from content when no explicit rows
        [Fact]
        public void AutoHeight_FromContent_NoExplicitRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;width:100px'>
                    <div style='height:60px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 60) < 2);
        }

        // [CSS-GRID §12.4] Auto height from content across multiple auto rows
        [Fact]
        public void AutoHeight_FromContent_MultipleAutoRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;width:100px'>
                    <div style='height:25px'></div>
                    <div style='height:35px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §12.4] Auto height includes row-gap between tracks
        [Fact]
        public void AutoHeight_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px;row-gap:20px;width:100px'>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §12.4] Auto height with row-gap and three rows
        [Fact]
        public void AutoHeight_WithRowGap_ThreeRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px 30px;row-gap:15px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 120) < 2);
        }

        // [CSS-GRID §7.2] Explicit height on grid container
        [Fact]
        public void ExplicitHeight_OverridesAutoSizing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;height:250px;width:100px'>
                    <div style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 250) < 2);
        }

        // [CSS-SIZING §4.1] min-height on grid container
        [Fact]
        public void MinHeight_EnforcesMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;min-height:150px;width:100px'>
                    <div style='height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height >= 149);
        }

        // [CSS-SIZING §4.1] min-height does not shrink container below content
        [Fact]
        public void MinHeight_DoesNotShrinkBelowContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;min-height:50px;width:100px'>
                    <div style='height:120px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 120) < 2);
        }

        // [CSS-SIZING §4.1] max-height caps grid container height
        [Fact]
        public void MaxHeight_CapsContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;height:300px;max-height:150px;width:100px'>
                    <div></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height <= 151);
        }

        // [CSS-SIZING §4.1] max-height does not enlarge container above auto height
        [Fact]
        public void MaxHeight_DoesNotEnlargeAboveAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;max-height:200px;width:100px'>
                    <div style='height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 50) < 2);
        }

        // [CSS-GRID §5.1] Percentage height resolves against containing block
        [Fact]
        public void PercentageHeight_ResolvesAgainstContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:400px'>
                    <div id='t' style='display:grid;grid-template-columns:100px;height:50%;width:100px'>
                        <div></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 200) < 2);
        }

        // [CSS-GRID §7.5] Auto height with grid-auto-rows
        [Fact]
        public void AutoHeight_WithAutoRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-auto-rows:50px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 150) < 2);
        }

        // [CSS-GRID §7.2.3] Auto height with fr rows treats fr as auto (no definite height)
        [Fact]
        public void AutoHeight_FrRowsTreatedAsAuto()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:1fr;width:100px'>
                    <div style='height:45px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 45) < 2);
        }

        // [CSS-GRID §7.2.3] Explicit height with fr rows distributes space
        [Fact]
        public void ExplicitHeight_FrRows_DistributeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;height:200px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §11.5] Auto height with spanning item across explicit rows
        [Fact]
        public void AutoHeight_SpanningItem_AcrossRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:40px 60px;width:100px'>
                    <div style='grid-row:1/3;height:100px'></div>
                </div></body>");
            var containerHeight = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(containerHeight - 100) < 2, $"Expected 100, got {containerHeight}");
        }

        // [CSS-GRID §12.4] Auto height with grid-template-rows specified
        [Fact]
        public void AutoHeight_WithGridTemplateRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:20px 30px 50px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §7.2] Auto height with mixed auto and fixed rows
        [Fact]
        public void AutoHeight_MixedAutoFixedRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:auto 50px auto;width:100px'>
                    <div style='height:30px'></div>
                    <div></div>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §12.4] Height from tallest content per row with two columns
        [Fact]
        public void AutoHeight_TallestContentPerRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div style='height:20px'></div>
                    <div style='height:60px'></div>
                    <div style='height:50px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 110) < 2);
        }

        // [CSS-GRID §12.4] Grid container in flex item inherits flex sizing
        [Fact]
        public void GridInFlexItem_Height()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px'>
                    <div id='t' style='display:grid;grid-template-columns:100px;width:100px'>
                        <div style='height:50px'></div>
                    </div>
                </div></body>");
            var gridContainer = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(gridContainer.ContentRect.Height - 200) < 2);
        }

        // [CSS-GRID §12.4] Grid container in block flow uses auto height
        [Fact]
        public void GridInBlockFlow_AutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='display:grid;grid-template-columns:100px'>
                        <div style='height:80px'></div>
                        <div style='height:40px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 120) < 2);
        }

        // [CSS-GRID §12.4] Empty grid with no explicit height is zero
        [Fact]
        public void EmptyGrid_NoExplicitHeight_IsZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;width:100px'>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height < 2);
        }

        // [CSS-GRID §12.4] Empty grid with explicit height uses that height
        [Fact]
        public void EmptyGrid_ExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;width:100px;height:80px'>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 80) < 2);
        }

        // [CSS-BOX §8.4] Grid height with padding does not inflate content height
        [Fact]
        public void GridHeight_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;padding:20px;width:100px'>
                    <div style='height:60px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.PaddingTop - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.PaddingBottom - 20) < 2);
        }

        // [CSS-BOX §8.1] Grid height with border does not inflate content height
        [Fact]
        public void GridHeight_WithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;border:10px solid black;width:100px'>
                    <div style='height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.BorderTopWidth - 10) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.BorderBottomWidth - 10) < 1);
        }

        // [CSS-BOX §8.4] Grid height with padding and border, border-box sizing
        [Fact]
        public void GridHeight_BorderBox_PaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;box-sizing:border-box;height:200px;padding:20px;border:10px solid black;width:200px'>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 140) < 2);
        }

        // [CSS-GRID §12.4] Auto height with row-gap and auto rows from content
        [Fact]
        public void AutoHeight_RowGap_AutoRowsFromContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;row-gap:10px;width:100px'>
                    <div style='height:30px'></div>
                    <div style='height:40px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 140) < 2);
        }

        // [CSS-GRID §7.2] Auto height with spanning item taller than combined rows
        [Fact]
        public void AutoHeight_SpanningItem_TallerThanCombinedRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div style='grid-row:1/3;height:150px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height >= 149);
        }

        // [CSS-SIZING §4.1] min-height and max-height combined on grid container
        [Fact]
        public void MinMaxHeight_Combined()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;min-height:100px;max-height:200px;width:100px'>
                    <div style='height:50px'></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(height >= 99, $"min-height 100px not enforced, got {height}");
            Assert.True(height <= 201, $"max-height 200px not enforced, got {height}");
        }

        // [CSS-GRID §12.4] Auto height from single auto row with tall content
        [Fact]
        public void AutoHeight_SingleAutoRow_TallContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;width:100px'>
                    <div style='height:200px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 200) < 2);
        }

        // [CSS-GRID §7.2] Auto height with mixed fixed and fr rows (no explicit height, fr treated as auto)
        [Fact]
        public void AutoHeight_MixedFixedFrRows_NoExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:50px 1fr;width:100px'>
                    <div></div>
                    <div style='height:70px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 120) < 2);
        }

        // [CSS-GRID §12.4] Explicit height with mixed fixed and fr rows distributes remainder
        [Fact]
        public void ExplicitHeight_MixedFixedFr_DistributesRemainder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:60px 1fr;height:200px;width:100px'>
                    <div id='fixed'></div>
                    <div id='flexible'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "fixed")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "flexible")!.ContentRect.Height - 140) < 2);
        }

        // [CSS-GRID §12.4] Auto height with row-gap and grid-auto-rows
        [Fact]
        public void AutoHeight_RowGap_GridAutoRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-auto-rows:40px;row-gap:10px;width:100px'>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 90) < 2);
        }

        // [CSS-GRID §12.4] Grid container height includes all implicit rows
        [Fact]
        public void AutoHeight_ImplicitRows_FromOverflow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:30px;grid-auto-rows:25px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 80) < 2);
        }

        // [CSS-GRID §12.4] Grid in flex item with align-items stretch
        [Fact]
        public void GridInFlexItem_AlignStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:stretch;height:300px'>
                    <div id='t' style='display:grid;grid-template-columns:100px;width:100px'>
                        <div style='height:40px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 300) < 2);
        }

        // [CSS-GRID §12.4] Nested grid: outer auto height from inner grid content
        [Fact]
        public void NestedGrid_OuterAutoHeight_FromInnerContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:200px;width:200px'>
                    <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 60px'>
                        <div></div>
                        <div></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §12.4] Grid container with padding-top and padding-bottom
        [Fact]
        public void GridHeight_PaddingDoesNotAffectContentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;padding-top:15px;padding-bottom:25px;width:100px'>
                    <div style='height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 50) < 2);
        }

        // [CSS-GRID §12.4] Explicit height with row-gap: fr rows use remaining space after gap
        [Fact]
        public void ExplicitHeight_FrRows_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;row-gap:20px;height:220px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 220) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.Height - 100) < 2);
        }
    }
}
