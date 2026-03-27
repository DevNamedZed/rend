using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class GridBaselineAlignmentTests
    {
        private readonly ITestOutputHelper _output;

        public GridBaselineAlignmentTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AlignItems_Baseline_ShiftsShortItemDown()
        {
            // Two items with different heights, no text/padding.
            // Baseline fallback = bottom edge → taller item's baseline wins.
            // Short item should be shifted down so bottoms align.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px;align-items:baseline'>
                    <div id='a' style='height:20px;background:red'></div>
                    <div id='b' style='height:40px;background:blue'></div>
                </div></body>");

            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;

            // Item A (20px) should be shifted down by 20px so bottom edges align
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - 20) < 2,
                $"Expected item A at Y≈20, got {boxA.ContentRect.Y}");
            Assert.True(System.Math.Abs(boxB.ContentRect.Y - 0) < 2,
                $"Expected item B at Y≈0, got {boxB.ContentRect.Y}");
        }

        [Fact]
        public void AlignSelf_Baseline_OnOneItem_AlignsWithDefault()
        {
            // One item with align-self:baseline, others stretch by default.
            // The baseline item should be positioned at top (no other baseline items
            // to align with → maxBaseline = own baseline → offset = 0).
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:80px;width:200px'>
                    <div id='a' style='align-self:baseline;height:30px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");

            var boxA = LayoutTestHelper.FindById(root, "a")!;

            // Only one baseline item → no shift needed
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - 0) < 2,
                $"Expected item A at Y≈0, got {boxA.ContentRect.Y}");
        }

        [Fact]
        public void AlignItems_Baseline_WithPadding_AlignsOnBaseline()
        {
            // Items with different padding-top shift based on baseline position.
            // Item A: padding-top:30px → baseline at 30+height
            // Item B: padding-top:10px → baseline at 10+height
            // Item B should be shifted down by 20px so baselines align.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px;align-items:baseline'>
                    <div id='a' style='padding-top:30px;height:20px'></div>
                    <div id='b' style='padding-top:10px;height:20px'></div>
                </div></body>");

            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;

            // Item A has higher baseline (30+20=50) vs B (10+20=30)
            // B should be shifted down by 20px
            // Content rect Y: A at padding-top=30, B at 20 + padding-top=10 → 30
            // Both content areas should start at the same Y
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - boxB.ContentRect.Y) < 2,
                $"Expected content rects at same Y, A={boxA.ContentRect.Y}, B={boxB.ContentRect.Y}");
        }

        [Fact]
        public void AlignItems_Baseline_RowGrowsWhenNeeded()
        {
            // Item A: padding-top:20px, height:40px → border-box 60px, baseline at 60
            // Item B: padding-bottom:20px, height:40px → border-box 60px, baseline at 40
            // Max baseline = 60, max descent = 60-40=20 → needed = 80
            // Row should grow from 60 to 80.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='grid' style='display:grid;grid-template-columns:100px 100px;width:200px;align-items:baseline'>
                    <div id='a' style='padding-top:20px;height:40px'></div>
                    <div id='b' style='padding-bottom:20px;height:40px'></div>
                </div></body>");

            var grid = LayoutTestHelper.FindById(root, "grid")!;

            // Auto height should be 80 (baseline group needs 60 above + 20 below)
            Assert.True(grid.ContentRect.Height >= 78,
                $"Expected grid height >= 78 for baseline row growth, got {grid.ContentRect.Height}");
        }

        [Fact]
        public void AlignItems_Baseline_SpanningItemExcluded()
        {
            // Spanning items don't participate in baseline groups.
            // Non-spanning item should not shift due to spanning item.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:60px 60px;width:200px;align-items:baseline'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                    <div id='span' style='grid-column:1/3;grid-row:1/3;height:100px'></div>
                </div></body>");

            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;

            // Both non-spanning items in row 0 should align on baseline
            // Since both are same height (30px), neither should be shifted
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - boxB.ContentRect.Y) < 2,
                $"Expected A and B at same Y, A={boxA.ContentRect.Y}, B={boxB.ContentRect.Y}");
        }
    }
}
