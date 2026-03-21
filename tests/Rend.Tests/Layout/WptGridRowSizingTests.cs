using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Grid row track sizing tests covering explicit rows, auto rows, fr units,
    /// row gap, percentage rows, minmax rows, spanning items, implicit rows,
    /// min/max-height constraints, and align-content stretch.
    /// </summary>
    public class WptGridRowSizingTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridRowSizingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §7.2] Single explicit row height
        [Fact]
        public void ExplicitRowHeight_80px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:80px;width:100px'>
                    <div id='t' style='background:green'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 80) < 2);
        }

        // [CSS-GRID §7.2] Auto row sizes from child content height
        [Fact]
        public void AutoRowFromContent_45px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='t' style='height:45px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 45) < 2);
        }

        // [CSS-GRID §7.2] Two explicit fixed rows
        [Fact]
        public void TwoRowsFixed_30px_70px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 70px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.Height - 70) < 2);
        }

        // [CSS-GRID §7.2] Three explicit fixed rows
        [Fact]
        public void ThreeRowsFixed_20px_40px_60px()
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

        // [CSS-GRID §7.2] Single 1fr row with explicit container height fills container
        [Fact]
        public void FrRowWithContainerHeight_1fr_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr;height:200px;width:100px'>
                    <div id='t'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 200) < 2);
        }

        // [CSS-GRID §7.2] 1fr and 2fr rows split container height in 1:2 ratio
        [Fact]
        public void FrRowRatio_1fr2fr_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 2fr;height:300px;width:100px'>
                    <div id='small'></div>
                    <div id='large'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "small")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "large")!.ContentRect.Height - 200) < 2);
        }

        // [CSS-GRID §7.2] Fixed row + fr row: fr takes remaining space
        [Fact]
        public void FixedPlusFrMix_50px_1fr_250px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px 1fr;height:250px;width:100px'>
                    <div id='fixed'></div>
                    <div id='flexible'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "fixed")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "flexible")!.ContentRect.Height - 200) < 2);
        }

        // [CSS-GRID §10.1] Row gap creates spacing between rows
        [Fact]
        public void RowGap_20px_BetweenRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px;row-gap:20px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            var firstBox = LayoutTestHelper.FindById(root, "first")!;
            var secondBox = LayoutTestHelper.FindById(root, "second")!;
            float gap = secondBox.ContentRect.Y - (firstBox.ContentRect.Y + firstBox.ContentRect.Height);
            Assert.True(System.Math.Abs(gap - 20) < 2, $"Expected 20px row-gap, got {gap}");
        }

        // [CSS-GRID §7.5] grid-auto-rows sizes all implicit rows
        [Fact]
        public void AutoRows_55px_SizesImplicitRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-rows:55px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Height - 55) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Height - 55) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Height - 55) < 2);
        }

        // [CSS-GRID §7.2] Percentage rows resolve against definite container height
        [Fact]
        public void PercentageRow_40Percent_Of200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40%;height:200px;width:100px'>
                    <div id='t'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 80) < 2);
        }

        // [CSS-GRID §7.2.1] minmax(100px,200px) row track uses 200px when space allows
        [Fact]
        public void MinmaxRow_FixedRange_UsesMaxInContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:minmax(100px,200px);height:300px;width:100px'>
                    <div id='t' style='height:150px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height >= 148,
                $"Expected at least 150px from minmax content, got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height}");
        }

        // [CSS-GRID §7.2.1] minmax(min,auto) row grows when content exceeds min
        [Fact]
        public void MinmaxRow_ContentExceedsMin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:minmax(30px,auto);width:100px'>
                    <div id='t' style='height:90px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 90) < 2);
        }

        // [CSS-GRID §11.5] Row-spanning item covers both row tracks
        [Fact]
        public void RowSpanningItem_CoversTracksHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 60px;width:200px'>
                    <div id='span' style='grid-row:1/3'></div>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "span")!.ContentRect.Height - 100) < 2,
                $"Spanning item should be 100px, got {LayoutTestHelper.FindById(root, "span")!.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Auto row where tallest content wins the track height
        [Fact]
        public void AutoRowTallestContentWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='short' style='height:25px'></div>
                    <div id='tall' style='height:75px'></div>
                </div></body>");
            var tallBox = LayoutTestHelper.FindById(root, "tall")!;
            Assert.True(System.Math.Abs(tallBox.ContentRect.Height - 75) < 2);
        }

        // [CSS-GRID §7.2] Multiple explicit rows sum to container height
        [Fact]
        public void MultipleRowsSumToContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;grid-template-rows:30px 50px 20px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §15.3] align-content:stretch distributes extra space to rows
        [Fact]
        public void AlignContentStretch_DistributesExtraSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:auto auto;align-content:stretch;height:200px;width:100px'>
                    <div id='r1' style='height:20px'></div>
                    <div id='r2' style='height:20px'></div>
                </div></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1")!;
            var row2 = LayoutTestHelper.FindById(root, "r2")!;
            float totalHeight = row1.ContentRect.Height + row2.ContentRect.Height;
            Assert.True(totalHeight > 38, $"align-content:stretch should grow rows, total={totalHeight}");
        }

        // [CSS-GRID §7.5] Implicit rows created when items overflow explicit grid
        [Fact]
        public void ImplicitRows_BeyondExplicitGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px;grid-auto-rows:50px;width:100px'>
                    <div id='explicit'></div>
                    <div id='implicit1'></div>
                    <div id='implicit2'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "explicit")!.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "implicit1")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "implicit2")!.ContentRect.Height - 50) < 2);
        }

        // [CSS-GRID §7.2] Auto row with empty cell still creates a row track
        [Fact]
        public void AutoRowWithEmptyCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px 100px;grid-template-rows:auto;width:200px'>
                    <div id='filled' style='height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "filled")!.ContentRect.Height - 50) < 2);
        }

        // [CSS-GRID §6.6] min-height on grid item enforces minimum row contribution
        [Fact]
        public void RowWithMinHeightItem_EnforcesMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='t' style='min-height:100px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height >= 98,
                $"Expected at least 100px from min-height, got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height}");
        }

        // [CSS-GRID §6.6] max-height on grid item caps height with align-items:start
        [Fact]
        public void RowWithMaxHeightItem_CapsHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:200px;align-items:start;width:100px'>
                    <div id='t' style='max-height:60px;height:200px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height <= 62,
                $"Expected max 60px from max-height, got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height}");
        }

        // [CSS-GRID §10.1] Row gap with fr rows reduces available space for fr distribution
        [Fact]
        public void RowGapWithFrRows_ReducesAvailable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;row-gap:40px;height:240px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §7.2] Y positions accumulate correctly across rows
        [Fact]
        public void RowYPositions_AccumulateCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:25px 35px 40px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Y - 60) < 2);
        }

        // [CSS-GRID §7.2] Fixed + fr + fixed row layout (header/content/footer pattern)
        [Fact]
        public void FixedFrFixed_HeaderContentFooter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:60px 1fr 40px;height:300px;width:100px'>
                    <div id='header'></div>
                    <div id='content'></div>
                    <div id='footer'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Height - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "footer")!.ContentRect.Height - 40) < 2);
        }

        // [CSS-GRID §7.2] Three equal fr rows split container height evenly
        [Fact]
        public void ThreeEqualFrRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr 1fr;height:270px;width:100px'>
                    <div id='r1'></div>
                    <div id='r2'></div>
                    <div id='r3'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Height - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Height - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Height - 90) < 2);
        }

        // [CSS-GRID §7.2] Percentage rows 25% and 75% of container height
        [Fact]
        public void PercentageRows_25_75()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:25% 75%;height:200px;width:100px'>
                    <div id='quarter'></div>
                    <div id='threequarter'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "quarter")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "threequarter")!.ContentRect.Height - 150) < 2);
        }

        // [CSS-GRID §7.2.1] minmax(50px,100px) with container forcing track to max
        [Fact]
        public void MinmaxRow_FixedMinMax_UsesMax()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:minmax(50px,100px);height:200px;width:100px'>
                    <div id='t'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height >= 98,
                $"Expected at least 100px from minmax max, got {LayoutTestHelper.FindById(root, "t")!.ContentRect.Height}");
        }

        // [CSS-GRID §11.5] Row span with gap includes gap in spanning distance
        [Fact]
        public void RowSpanWithGap_IncludesGapInSpan()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:30px 30px;row-gap:10px;width:200px'>
                    <div id='span' style='grid-row:1/3'></div>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "span")!.ContentRect.Height - 70) < 2,
                $"Spanning item should be 70px (30+10+30), got {LayoutTestHelper.FindById(root, "span")!.ContentRect.Height}");
        }

        // [CSS-GRID §7.2] Grid auto height sums explicit rows plus gaps
        [Fact]
        public void GridAutoHeight_SumsRowsAndGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px 30px;row-gap:15px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 120) < 2);
        }

        // [CSS-GRID §10.4] Default stretch fills item to row track height
        [Fact]
        public void DefaultStretch_FillsRowTrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:120px;width:100px'>
                    <div id='t'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 120) < 2);
        }

        // [CSS-GRID §10.4] align-items:center positions item vertically centered in row
        [Fact]
        public void AlignItemsCenter_CentersInRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;align-items:center;width:100px'>
                    <div id='t' style='height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 30) < 2);
        }

        // [CSS-GRID §10.5] align-items:end positions item at bottom of row
        [Fact]
        public void AlignItemsEnd_PositionsAtBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;align-items:end;width:100px'>
                    <div id='t' style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 70) < 2);
        }
    }
}
