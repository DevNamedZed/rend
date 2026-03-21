using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Grid row/height sizing tests covering explicit rows, auto rows,
    /// fr rows, row gap, percentage rows, minmax rows, min/max-height on items,
    /// spanning items, alignment, and nested grids.
    /// </summary>
    public class WptGridHeightSizingTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridHeightSizingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §7.2] Explicit row heights via grid-template-rows
        [Fact]
        public void ExplicitRowHeight_SingleRow_50px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px;width:100px'>
                    <div id='t' style='background:red'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 50) < 2);
        }

        // [CSS-GRID §7.2] Multiple explicit row heights
        [Fact]
        public void ExplicitRowHeight_TwoRows_40px_80px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 80px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.Height - 80) < 2);
        }

        // [CSS-GRID §7.2] Auto row sizes to fit content
        [Fact]
        public void AutoRow_SizesFromContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='t' style='height:35px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 35) < 2);
        }

        // [CSS-GRID §7.2] Auto row with two items picks tallest for row track
        [Fact]
        public void AutoRow_TallestItemDefinesRowTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='short' style='height:20px'></div>
                    <div id='tall' style='height:60px'></div>
                </div></body>");
            var tallItem = LayoutTestHelper.FindById(root, "tall")!;
            Assert.True(System.Math.Abs(tallItem.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "short")!.ContentRect.Height - 20) < 2);
        }

        // [CSS-GRID §7.2] fr rows with explicit container height
        [Fact]
        public void FrRow_WithExplicitContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr;height:200px;width:100px'>
                    <div id='t'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 200) < 2);
        }

        // [CSS-GRID §7.2] 1fr + 2fr row ratio with explicit container height
        [Fact]
        public void FrRows_1fr2fr_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 2fr;height:300px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.Height - 200) < 2);
        }

        // [CSS-GRID §7.2] Fixed + fr row mix
        [Fact]
        public void FixedPlusFrRow_Mix()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:60px 1fr;height:200px;width:100px'>
                    <div id='fixed'></div>
                    <div id='flex'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "fixed")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "flex")!.ContentRect.Height - 140) < 2);
        }

        // [CSS-GRID §10.1] Row gap separates rows
        [Fact]
        public void RowGap_SeparatesRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px;row-gap:20px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            var firstBottom = LayoutTestHelper.FindById(root, "first")!.ContentRect.Y + LayoutTestHelper.FindById(root, "first")!.ContentRect.Height;
            var secondTop = LayoutTestHelper.FindById(root, "second")!.ContentRect.Y;
            float gap = secondTop - firstBottom;
            Assert.True(System.Math.Abs(gap - 20) < 2, $"row-gap should be 20px (got {gap})");
        }

        // [CSS-GRID §7.5] grid-auto-rows sizes implicit rows
        [Fact]
        public void AutoRows_SizesImplicitRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-rows:70px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                    <div id='third'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Height - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.Height - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "third")!.ContentRect.Height - 70) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-rows with explicit first row
        [Fact]
        public void AutoRows_AfterExplicitRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px;grid-auto-rows:60px;width:100px'>
                    <div id='explicit'></div>
                    <div id='implicit'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "explicit")!.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "implicit")!.ContentRect.Height - 60) < 2);
        }

        // [CSS-GRID §7.2] Percentage row heights resolve against container
        [Fact]
        public void PercentageRow_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:25% 75%;height:200px;width:100px'>
                    <div id='quarter'></div>
                    <div id='three_quarter'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "quarter")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "three_quarter")!.ContentRect.Height - 150) < 2);
        }

        // [CSS-GRID §7.2.1] minmax() on row tracks with content larger than min
        [Fact]
        public void MinmaxRow_ContentLargerThanMin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:minmax(50px,auto);width:100px'>
                    <div id='t' style='height:80px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 80) < 2);
        }

        // [CSS-GRID §7.2.1] minmax() auto grows beyond minimum
        [Fact]
        public void MinmaxRow_AutoGrowsBeyondMin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:minmax(30px,auto);width:100px'>
                    <div id='t' style='height:80px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 80) < 2);
        }

        // [CSS-GRID §6.6] min-height on grid item
        [Fact]
        public void MinHeight_OnGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='t' style='min-height:90px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height >= 89, $"min-height should enforce 90px (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS-GRID §6.6] max-height on grid item with align-self start
        [Fact]
        public void MaxHeight_OnGridItem_WithAlignStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:200px;align-items:start;width:100px'>
                    <div id='t' style='max-height:50px;height:200px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height <= 52, $"max-height should cap at 50px (got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height})");
        }

        // [CSS-GRID §7.2] Grid auto height from content (no explicit height)
        [Fact]
        public void GridAutoHeight_FromContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;width:100px'>
                    <div style='height:45px'></div>
                    <div style='height:55px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §7.2] Explicit grid container height
        [Fact]
        public void ExplicitGridHeight_ContainerSized()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;height:300px;width:100px'>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 300) < 2);
        }

        // [CSS-GRID §10.4] Row stretch alignment (default)
        [Fact]
        public void RowStretch_DefaultAlignment()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;width:100px'>
                    <div id='t'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §10.5] Grid item explicit height overrides stretch
        [Fact]
        public void ExplicitHeight_OverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;width:100px'>
                    <div id='t' style='height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 40) < 2);
        }

        // [CSS-GRID §11.5] Row with spanning item distributes height
        [Fact]
        public void RowSpanningItem_DistributesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='span' style='grid-row:1/3'></div>
                    <div style='height:50px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "span")!.ContentRect.Height >= 99, $"Spanning item should cover both rows (got {LayoutTestHelper.FindById(root, "span")!.ContentRect.Height})");
        }

        // [CSS-GRID §7.2] Multiple rows with different sizes
        [Fact]
        public void MultipleRows_DifferentSizes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:20px 40px 60px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Height - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Height - 60) < 2);
        }

        // [CSS-GRID §7.2] Y positions accumulate across rows
        [Fact]
        public void RowYPositions_Accumulate()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 50px 70px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Y - 80) < 2);
        }

        // [CSS-GRID §7.2] Nested grid row heights
        [Fact]
        public void NestedGrid_RowHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:auto;width:200px'>
                    <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 40px'>
                        <div id='inner1'></div>
                        <div id='inner2'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "inner1")!.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "inner2")!.ContentRect.Height - 40) < 2);
        }

        // [CSS-GRID §10.1] Row gap with fr rows reduces available space
        [Fact]
        public void RowGap_WithFrRows_ReducesAvailable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;row-gap:20px;height:220px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §7.2] Three fr rows with different weights
        [Fact]
        public void FrRows_1fr_2fr_3fr_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 2fr 3fr;height:300px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Height - 150) < 2);
        }

        // [CSS-GRID §7.2] Fixed + fr + fixed row mix
        [Fact]
        public void FixedFrFixed_RowMix()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 1fr 40px;height:200px;width:100px'>
                    <div id='top'></div>
                    <div id='middle'></div>
                    <div id='bottom'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "top")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "middle")!.ContentRect.Height - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bottom")!.ContentRect.Height - 40) < 2);
        }

        // [CSS-GRID §10.4] align-items: center positions item in middle of row
        [Fact]
        public void AlignItems_Center_PositionsMiddle()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;align-items:center;width:100px'>
                    <div id='t' style='height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 30) < 2);
        }

        // [CSS-GRID §10.4] align-items: start positions at top of row
        [Fact]
        public void AlignItems_Start_PositionsTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;align-items:start;width:100px'>
                    <div id='t' style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 30) < 2);
        }

        // [CSS-GRID §11.5] Spanning item with row gap includes gap in span
        [Fact]
        public void RowSpan_WithGap_IncludesGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 40px;row-gap:20px;width:200px'>
                    <div id='span' style='grid-row:1/3'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "span")!.ContentRect.Height - 100) < 2, $"Span should include gap (got {LayoutTestHelper.FindById(root, "span")!.ContentRect.Height})");
        }

        // [CSS-GRID §7.2.1] minmax(min, max) row with fr max in explicit height container
        [Fact]
        public void MinmaxRow_FrMax_WithContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:minmax(30px,1fr) minmax(30px,2fr);height:300px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.Height - 200) < 2);
        }

        // [CSS-GRID §7.2] Grid auto height sums all row tracks
        [Fact]
        public void GridAutoHeight_SumsRowTracks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;grid-template-rows:25px 35px 40px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §7.2] Grid auto height with row gap
        [Fact]
        public void GridAutoHeight_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px 30px;row-gap:10px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 110) < 2);
        }

        // [CSS-GRID §6.6] min-height on grid item enforces minimum
        [Fact]
        public void MinHeight_EnforcesMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='tall' style='min-height:80px'></div>
                    <div id='sibling' style='height:20px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "tall")!.ContentRect.Height >= 79);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sibling")!.ContentRect.Height - 20) < 2);
        }

        // [CSS-GRID §7.2] Nested grid with explicit outer row auto-sizes to inner content
        [Fact]
        public void NestedGrid_OuterAutoRow_SizesToInnerContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='outer' style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='inner' style='display:grid;grid-template-columns:100px;grid-template-rows:25px 35px'>
                        <div></div>
                        <div></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "outer")!.ContentRect.Height - 60) < 2);
        }

        // [CSS-GRID §10.5] align-self: end on item within explicit row
        [Fact]
        public void AlignSelf_End_WithinExplicitRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:120px;width:100px'>
                    <div id='t' style='align-self:end;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 80) < 2);
        }

        // [CSS-GRID §7.2] Two fixed + two fr rows
        [Fact]
        public void TwoFixedTwoFr_Rows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px 1fr 2fr 50px;height:400px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                    <div id='r4'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Height - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r4")!.ContentRect.Height - 50) < 2);
        }
    }
}
