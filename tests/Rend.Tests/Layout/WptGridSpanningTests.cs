using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Grid item spanning behavior tests covering column/row spans,
    /// explicit placement, gap interaction, named areas, auto-flow,
    /// and alignment with spanning items.
    /// </summary>
    public class WptGridSpanningTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridSpanningTests(ITestOutputHelper output) { _output = output; }

        // [CSS-GRID §8.3] grid-column: span 2 in a 3-column fixed grid
        [Fact]
        public void ColumnSpan2_In3ColGrid_Width200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §8.3] grid-column: span 3 in a 3-column grid spans full width
        [Fact]
        public void ColumnSpan3_In3ColGrid_FullWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='span' style='grid-column:span 3;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §8.3] grid-row: span 2 in explicit 2-row grid
        [Fact]
        public void RowSpan2_In2RowGrid_Height100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='span' style='grid-row:span 2'></div>
                    <div style='height:50px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §8.3] grid-column: 1 / 3 explicit start/end
        [Fact]
        public void ExplicitColumn1To3_Width200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='span' style='grid-column:1/3;height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(span.ContentRect.X - 0) < 2);
        }

        // [CSS-GRID §8.3] grid-column: 2 / 4 explicit start/end
        [Fact]
        public void ExplicitColumn2To4_Width200_X100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='span' style='grid-column:2/4;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(span.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §8.3] grid-column: 1 / -1 spans all columns
        [Fact]
        public void Column1ToNeg1_SpansAllColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px 80px;width:240px'>
                    <div id='span' style='grid-column:1/-1;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 240) < 2);
        }

        // [CSS-GRID §8.3] grid-row: 1 / 3 explicit row span
        [Fact]
        public void ExplicitRow1To3_Height100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 60px;width:100px'>
                    <div id='span' style='grid-row:1/3'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §8.3] explicit column and row placement
        [Fact]
        public void ExplicitPlacement_Col2Row2()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px;grid-template-rows:40px 40px;width:160px'>
                    <div id='item' style='grid-column:2;grid-row:2;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 40) < 2);
        }

        // [CSS-GRID §8.3] combined column span + row span via grid-area
        [Fact]
        public void CombinedColspanRowspan_GridArea()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:50px 50px;width:300px'>
                    <div id='span' style='grid-area:1/1/3/3'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(span.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §10.1] span 2 with column-gap includes gap in width
        [Fact]
        public void ColumnSpan2_WithGap_IncludesGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px 80px;gap:20px;width:280px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            // span 2 = 80 + 20 (gap) + 80 = 180
            Assert.True(System.Math.Abs(span.ContentRect.Width - 180) < 2);
        }

        // [CSS-GRID §10.1] span 3 with column-gap includes 2 gaps
        [Fact]
        public void ColumnSpan3_WithGap_Includes2Gaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px 80px;gap:20px;width:280px'>
                    <div id='span' style='grid-column:span 3;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            // span 3 = 80 + 20 + 80 + 20 + 80 = 280
            Assert.True(System.Math.Abs(span.ContentRect.Width - 280) < 2);
        }

        // [CSS-GRID §8.3] spanning item position X starts at correct column
        [Fact]
        public void SpanningItemPositionX_StartsAtCol2()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:60px 60px 60px;width:180px'>
                    <div style='height:20px'></div>
                    <div id='span' style='grid-column:2/4;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(span.ContentRect.Width - 120) < 2);
        }

        // [CSS-GRID §8.3] spanning item position Y with row span
        [Fact]
        public void SpanningItemPositionY_StartsAtRow1()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:30px 30px 30px;width:200px'>
                    <div style='height:30px'></div>
                    <div id='span' style='grid-column:2;grid-row:1/3'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(span.ContentRect.Height - 60) < 2);
        }

        // [CSS-GRID §8.5] non-spanning items placed after spanning item
        [Fact]
        public void NonSpanningItem_AfterSpan_PlacedInNextCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                    <div id='next' style='height:20px'></div>
                </div></body>");
            var next = LayoutTestHelper.FindById(root, "next")!;
            // span occupies cols 1-2, next goes to col 3
            Assert.True(System.Math.Abs(next.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §8.5] two spanning items in same grid
        [Fact]
        public void TwoSpanningItems_SameGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='first' style='grid-column:span 2;height:20px'></div>
                    <div style='height:20px'></div>
                    <div id='second' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.Width - 200) < 2);
            // second starts on row 2
            Assert.True(second.ContentRect.Y > first.ContentRect.Y);
        }

        // [CSS-GRID §8.3] span 2 in 4-column grid
        [Fact]
        public void ColumnSpan2_In4ColGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,80px);width:320px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                    <div style='height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 160) < 2);
        }

        // [CSS-GRID §8.3] full-row span in 4-column grid
        [Fact]
        public void FullRowSpan_In4ColGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,80px);width:320px'>
                    <div id='span' style='grid-column:1/-1;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 320) < 2);
        }

        // [CSS-GRID §8.5] span with named areas
        [Fact]
        public void SpanWithNamedAreas_HeaderSpans2Cols()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-areas:""header header"" ""left right"";grid-template-columns:150px 150px;grid-template-rows:40px 60px;width:300px'>
                    <div id='header' style='grid-area:header'></div>
                    <div id='left' style='grid-area:left'></div>
                    <div id='right' style='grid-area:right'></div>
                </div></body>");
            var header = LayoutTestHelper.FindById(root, "header")!;
            Assert.True(System.Math.Abs(header.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(header.ContentRect.Height - 40) < 2);
        }

        // [CSS-GRID §7.6] span with auto-flow row (default)
        [Fact]
        public void SpanWithAutoFlowRow_WrapsToNextRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                    <div id='span' style='grid-column:span 2;height:30px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            // Third item spans 2, goes to row 2
            Assert.True(System.Math.Abs(span.ContentRect.Width - 200) < 2);
            Assert.True(span.ContentRect.Y >= 29);
        }

        // [CSS-GRID §7.6] span with dense auto-flow fills gap
        [Fact]
        public void SpanWithDenseAutoFlow_FillsGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:60px 60px 60px;grid-auto-flow:dense;width:180px'>
                    <div id='big' style='grid-column:2/4;height:20px'></div>
                    <div id='small' style='height:20px'></div>
                </div></body>");
            var small = LayoutTestHelper.FindById(root, "small")!;
            // dense packs small item into col 1 gap
            Assert.True(small.ContentRect.X < 2);
        }

        // [CSS-GRID §7.5] row span with grid-auto-rows
        [Fact]
        public void RowSpan_WithAutoRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-auto-rows:40px;width:200px'>
                    <div id='span' style='grid-row:span 2'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            // 2 auto rows of 40px each = 80px
            Assert.True(System.Math.Abs(span.ContentRect.Height - 80) < 2);
        }

        // [CSS-GRID §7.2] column span with percentage columns
        [Fact]
        public void ColumnSpan_WithPercentColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:25% 25% 50%;width:400px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            // 25% of 400 = 100, span 2 = 100 + 100 = 200
            Assert.True(System.Math.Abs(span.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2] span with fr columns
        [Fact]
        public void ColumnSpan_WithFrColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 2fr 1fr;width:400px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            // 1fr=100, 2fr=200, span cols 1+2 = 100 + 200 = 300
            Assert.True(System.Math.Abs(span.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §8.3] overlapping spans with explicit placement
        [Fact]
        public void OverlappingSpans_ExplicitPlacement()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:50px;width:300px'>
                    <div id='first' style='grid-column:1/3;grid-row:1;height:50px'></div>
                    <div id='second' style='grid-column:2/4;grid-row:1;height:50px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(first.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §10.4] span + justify-self: center
        [Fact]
        public void Span_WithJustifySelfCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='span' style='grid-column:span 2;justify-self:center;width:100px;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            // span area = 200px, item width = 100px, centered at X = 50
            Assert.True(System.Math.Abs(span.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(span.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §10.4] span + align-self: end
        [Fact]
        public void Span_WithAlignSelfEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px;width:100px'>
                    <div id='span' style='grid-row:span 2;align-self:end;height:30px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            // span area = 80px, item height = 30px, end-aligned at Y = 50
            Assert.True(System.Math.Abs(span.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(span.ContentRect.Height - 30) < 2);
        }

        // [CSS-GRID §10.1] row span with row-gap includes gap
        [Fact]
        public void RowSpan2_WithRowGap_IncludesGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 40px;row-gap:10px;width:200px'>
                    <div id='span' style='grid-row:span 2'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            // 40 + 10 (gap) + 40 = 90
            Assert.True(System.Math.Abs(span.ContentRect.Height - 90) < 2);
        }

        // [CSS-GRID §8.3] span 2 starting at col 3 in 4-col grid
        [Fact]
        public void ColumnSpan2_StartingAtCol3_In4ColGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,80px);width:320px'>
                    <div id='span' style='grid-column:3/5;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.X - 160) < 2);
            Assert.True(System.Math.Abs(span.ContentRect.Width - 160) < 2);
        }

        // [CSS-GRID §8.5] named area spanning 2 rows and 2 columns
        [Fact]
        public void NamedArea_Spanning2x2()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-areas:""a a b"" ""a a c"";grid-template-columns:100px 100px 100px;grid-template-rows:50px 50px;width:300px'>
                    <div id='area' style='grid-area:a'></div>
                    <div id='itemb' style='grid-area:b'></div>
                    <div id='itemc' style='grid-area:c'></div>
                </div></body>");
            var area = LayoutTestHelper.FindById(root, "area")!;
            Assert.True(System.Math.Abs(area.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(area.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §8.3] span with gap and explicit position
        [Fact]
        public void SpanWithGap_ExplicitPosition_CorrectX()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:60px 60px 60px 60px;column-gap:20px;width:300px'>
                    <div id='span' style='grid-column:2/4;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            // X = 60 (col1) + 20 (gap) = 80
            Assert.True(System.Math.Abs(span.ContentRect.X - 80) < 2);
            // width = 60 + 20 + 60 = 140
            Assert.True(System.Math.Abs(span.ContentRect.Width - 140) < 2);
        }

        // [CSS-GRID §8.3] column span with mixed fixed and fr tracks
        [Fact]
        public void ColumnSpan_MixedFixedAndFr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr 100px;width:400px'>
                    <div id='span' style='grid-column:1/3;height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            // col1=100, col2=200 (1fr of remaining 200), span = 100+200 = 300
            Assert.True(System.Math.Abs(span.ContentRect.Width - 300) < 2);
        }
    }
}
