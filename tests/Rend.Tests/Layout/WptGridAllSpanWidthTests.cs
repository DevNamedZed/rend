using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive grid spanning width and height tests covering column span in
    /// various grid configurations, row spans, gaps, fr tracks, percentage tracks,
    /// and named area spanning.
    /// </summary>
    public class WptGridAllSpanWidthTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridAllSpanWidthTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // ──────────────────────────────────────────────
        // Column span 2 in grids with 2–6 equal columns
        // ──────────────────────────────────────────────

        // [CSS-GRID §8.3] span 2 in a 2-column grid occupies full width
        [Fact]
        public void ColumnSpan2_In2ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 200) < 2,
                $"span 2 in 2-col grid: expected 200, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] span 2 in a 3-column grid occupies 2/3 of width
        [Fact]
        public void ColumnSpan2_In3ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 200) < 2,
                $"span 2 in 3-col grid: expected 200, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] span 2 in a 4-column grid
        [Fact]
        public void ColumnSpan2_In4ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,80px);width:320px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 160) < 2,
                $"span 2 in 4-col grid: expected 160, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] span 2 in a 5-column grid
        [Fact]
        public void ColumnSpan2_In5ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(5,60px);width:300px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 120) < 2,
                $"span 2 in 5-col grid: expected 120, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] span 2 in a 6-column grid
        [Fact]
        public void ColumnSpan2_In6ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(6,50px);width:300px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 100) < 2,
                $"span 2 in 6-col grid: expected 100, got {span.ContentRect.Width}");
        }

        // ──────────────────────────────────────────────
        // Column span 3 in grids with 3–6 equal columns
        // ──────────────────────────────────────────────

        // [CSS-GRID §8.3] span 3 in a 3-column grid occupies full width
        [Fact]
        public void ColumnSpan3_In3ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,100px);width:300px'>
                    <div id='span' style='grid-column:span 3;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 300) < 2,
                $"span 3 in 3-col grid: expected 300, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] span 3 in a 4-column grid
        [Fact]
        public void ColumnSpan3_In4ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,80px);width:320px'>
                    <div id='span' style='grid-column:span 3;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 240) < 2,
                $"span 3 in 4-col grid: expected 240, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] span 3 in a 5-column grid
        [Fact]
        public void ColumnSpan3_In5ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(5,60px);width:300px'>
                    <div id='span' style='grid-column:span 3;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 180) < 2,
                $"span 3 in 5-col grid: expected 180, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] span 3 in a 6-column grid
        [Fact]
        public void ColumnSpan3_In6ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(6,50px);width:300px'>
                    <div id='span' style='grid-column:span 3;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 150) < 2,
                $"span 3 in 6-col grid: expected 150, got {span.ContentRect.Width}");
        }

        // ──────────────────────────────────────────────
        // Column span 4 in grids with 4–6 equal columns
        // ──────────────────────────────────────────────

        // [CSS-GRID §8.3] span 4 in a 4-column grid occupies full width
        [Fact]
        public void ColumnSpan4_In4ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,80px);width:320px'>
                    <div id='span' style='grid-column:span 4;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 320) < 2,
                $"span 4 in 4-col grid: expected 320, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] span 4 in a 5-column grid
        [Fact]
        public void ColumnSpan4_In5ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(5,60px);width:300px'>
                    <div id='span' style='grid-column:span 4;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 240) < 2,
                $"span 4 in 5-col grid: expected 240, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] span 4 in a 6-column grid
        [Fact]
        public void ColumnSpan4_In6ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(6,50px);width:300px'>
                    <div id='span' style='grid-column:span 4;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 200) < 2,
                $"span 4 in 6-col grid: expected 200, got {span.ContentRect.Width}");
        }

        // ──────────────────────────────────────────────
        // Column span 5 in grids with 5–6 equal columns
        // ──────────────────────────────────────────────

        // [CSS-GRID §8.3] span 5 in a 5-column grid occupies full width
        [Fact]
        public void ColumnSpan5_In5ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(5,60px);width:300px'>
                    <div id='span' style='grid-column:span 5;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 300) < 2,
                $"span 5 in 5-col grid: expected 300, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] span 5 in a 6-column grid
        [Fact]
        public void ColumnSpan5_In6ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(6,50px);width:300px'>
                    <div id='span' style='grid-column:span 5;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 250) < 2,
                $"span 5 in 6-col grid: expected 250, got {span.ContentRect.Width}");
        }

        // ──────────────────────────────────────────────
        // Span all columns (1/-1) in grids with 2–6 columns
        // ──────────────────────────────────────────────

        // [CSS-GRID §8.3] grid-column: 1/-1 in 2-column grid
        [Fact]
        public void SpanAll_In2ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(2,100px);width:200px'>
                    <div id='span' style='grid-column:1/-1;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 200) < 2,
                $"1/-1 in 2-col grid: expected 200, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] grid-column: 1/-1 in 3-column grid
        [Fact]
        public void SpanAll_In3ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,100px);width:300px'>
                    <div id='span' style='grid-column:1/-1;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 300) < 2,
                $"1/-1 in 3-col grid: expected 300, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] grid-column: 1/-1 in 4-column grid
        [Fact]
        public void SpanAll_In4ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,80px);width:320px'>
                    <div id='span' style='grid-column:1/-1;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 320) < 2,
                $"1/-1 in 4-col grid: expected 320, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] grid-column: 1/-1 in 5-column grid
        [Fact]
        public void SpanAll_In5ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(5,60px);width:300px'>
                    <div id='span' style='grid-column:1/-1;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 300) < 2,
                $"1/-1 in 5-col grid: expected 300, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] grid-column: 1/-1 in 6-column grid
        [Fact]
        public void SpanAll_In6ColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(6,50px);width:300px'>
                    <div id='span' style='grid-column:1/-1;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 300) < 2,
                $"1/-1 in 6-col grid: expected 300, got {span.ContentRect.Width}");
        }

        // ──────────────────────────────────────────────
        // Column span 2 with various gap sizes
        // ──────────────────────────────────────────────

        // [CSS-GRID §10.1] span 2 with 10px gap: width = 2*col + 1*gap
        [Fact]
        public void ColumnSpan2_WithGap10()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,80px);column-gap:10px;width:260px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 170) < 2,
                $"span 2 with gap 10: expected 170, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §10.1] span 2 with 20px gap
        [Fact]
        public void ColumnSpan2_WithGap20()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,80px);column-gap:20px;width:280px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 180) < 2,
                $"span 2 with gap 20: expected 180, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §10.1] span 2 with 30px gap
        [Fact]
        public void ColumnSpan2_WithGap30()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,80px);column-gap:30px;width:300px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 190) < 2,
                $"span 2 with gap 30: expected 190, got {span.ContentRect.Width}");
        }

        // ──────────────────────────────────────────────
        // Column span 3 with gap
        // ──────────────────────────────────────────────

        // [CSS-GRID §10.1] span 3 with 15px gap: width = 3*col + 2*gap
        [Fact]
        public void ColumnSpan3_WithGap15()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,60px);column-gap:15px;width:285px'>
                    <div id='span' style='grid-column:span 3;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 210) < 2,
                $"span 3 with gap 15: expected 210, got {span.ContentRect.Width}");
        }

        // ──────────────────────────────────────────────
        // Column span 2 X positions (starting at different columns)
        // ──────────────────────────────────────────────

        // [CSS-GRID §8.3] span 2 starting at column 1 (X = 0)
        [Fact]
        public void ColumnSpan2_StartColumn1_XPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,80px);width:320px'>
                    <div id='span' style='grid-column:1/span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.X - 0) < 2,
                $"span 2 at col 1: expected X=0, got {span.ContentRect.X}");
            Assert.True(System.Math.Abs(span.ContentRect.Width - 160) < 2,
                $"span 2 at col 1: expected W=160, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] span 2 starting at column 2
        [Fact]
        public void ColumnSpan2_StartColumn2_XPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,80px);width:320px'>
                    <div id='span' style='grid-column:2/span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.X - 80) < 2,
                $"span 2 at col 2: expected X=80, got {span.ContentRect.X}");
            Assert.True(System.Math.Abs(span.ContentRect.Width - 160) < 2,
                $"span 2 at col 2: expected W=160, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §8.3] span 2 starting at column 3 in a 4-column grid
        [Fact]
        public void ColumnSpan2_StartColumn3_XPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,80px);width:320px'>
                    <div id='span' style='grid-column:3/span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.X - 160) < 2,
                $"span 2 at col 3: expected X=160, got {span.ContentRect.X}");
            Assert.True(System.Math.Abs(span.ContentRect.Width - 160) < 2,
                $"span 2 at col 3: expected W=160, got {span.ContentRect.Width}");
        }

        // ──────────────────────────────────────────────
        // Row span 2 heights in 2-row and 3-row grids
        // ──────────────────────────────────────────────

        // [CSS-GRID §8.3] row span 2 in a 2-row grid occupies full height
        [Fact]
        public void RowSpan2_In2RowGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='span' style='grid-row:span 2'></div>
                    <div style='height:50px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Height - 100) < 2,
                $"row span 2 in 2-row grid: expected 100, got {span.ContentRect.Height}");
        }

        // [CSS-GRID §8.3] row span 2 in a 3-row grid (first two rows)
        [Fact]
        public void RowSpan2_In3RowGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 60px 30px;width:200px'>
                    <div id='span' style='grid-row:span 2'></div>
                    <div style='height:40px'></div>
                    <div style='height:60px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Height - 100) < 2,
                $"row span 2 in 3-row grid: expected 100, got {span.ContentRect.Height}");
        }

        // [CSS-GRID §8.3] row span 2 starting at row 2 in a 3-row grid
        [Fact]
        public void RowSpan2_StartRow2_In3RowGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 50px 60px;width:200px'>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                    <div id='span' style='grid-row:2/span 2'></div>
                    <div style='height:50px'></div>
                    <div style='height:60px'></div>
                    <div style='height:60px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Height - 110) < 2,
                $"row span 2 at row 2: expected 110, got {span.ContentRect.Height}");
            Assert.True(System.Math.Abs(span.ContentRect.Y - 40) < 2,
                $"row span 2 at row 2: expected Y=40, got {span.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // Row span with gap
        // ──────────────────────────────────────────────

        // [CSS-GRID §10.1] row span 2 with 10px row-gap: height = 2*row + 1*gap
        [Fact]
        public void RowSpan2_WithRowGap10()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 40px;row-gap:10px;width:200px'>
                    <div id='span' style='grid-row:span 2'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Height - 90) < 2,
                $"row span 2 with gap 10: expected 90, got {span.ContentRect.Height}");
        }

        // [CSS-GRID §10.1] row span 2 with 20px row-gap
        [Fact]
        public void RowSpan2_WithRowGap20()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;row-gap:20px;width:200px'>
                    <div id='span' style='grid-row:span 2'></div>
                    <div style='height:50px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Height - 120) < 2,
                $"row span 2 with gap 20: expected 120, got {span.ContentRect.Height}");
        }

        // ──────────────────────────────────────────────
        // Column span 2 with fr columns
        // ──────────────────────────────────────────────

        // [CSS-GRID §7.2] span 2 in a 3-column fr grid
        [Fact]
        public void ColumnSpan2_WithFrColumns_EqualFr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 200) < 2,
                $"span 2 in 3x1fr: expected 200, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §7.2] span 2 in mixed fr grid (1fr 2fr 1fr) starting at col 1
        [Fact]
        public void ColumnSpan2_WithMixedFrColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 2fr 1fr;width:400px'>
                    <div id='span' style='grid-column:1/span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 300) < 2,
                $"span 2 in 1fr+2fr: expected 300, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §7.2] span 2 in a 4-column fr grid with gap
        [Fact]
        public void ColumnSpan2_WithFrColumns_AndGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr 1fr;column-gap:20px;width:340px'>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            float expectedColumnWidth = (340 - 3 * 20) / 4.0f;
            float expectedSpanWidth = 2 * expectedColumnWidth + 20;
            Assert.True(System.Math.Abs(span.ContentRect.Width - expectedSpanWidth) < 2,
                $"span 2 in 4x1fr with gap 20: expected {expectedSpanWidth}, got {span.ContentRect.Width}");
        }

        // ──────────────────────────────────────────────
        // Column span 2 with percentage columns
        // ──────────────────────────────────────────────

        // [CSS-GRID §7.2] span 2 in a 3-column percentage grid
        [Fact]
        public void ColumnSpan2_WithPercentageColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:25% 25% 50%;width:400px'>
                    <div id='span' style='grid-column:1/span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 200) < 2,
                $"span 2 in 25%+25%: expected 200, got {span.ContentRect.Width}");
        }

        // [CSS-GRID §7.2] span 2 starting at column 2 in percentage grid
        [Fact]
        public void ColumnSpan2_PercentageColumns_StartColumn2()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:20% 30% 50%;width:400px'>
                    <div id='span' style='grid-column:2/span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 320) < 2,
                $"span 2 in 30%+50%: expected 320, got {span.ContentRect.Width}");
            Assert.True(System.Math.Abs(span.ContentRect.X - 80) < 2,
                $"span 2 start col 2: expected X=80, got {span.ContentRect.X}");
        }

        // ──────────────────────────────────────────────
        // Named area spanning
        // ──────────────────────────────────────────────

        // [CSS-GRID §8.5] named area spanning 2 columns in a 3-column layout
        [Fact]
        public void NamedArea_Span2Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-areas:""header header sidebar"" ""main main sidebar"";grid-template-columns:100px 100px 100px;grid-template-rows:40px 60px;width:300px'>
                    <div id='header' style='grid-area:header'></div>
                    <div id='main' style='grid-area:main'></div>
                    <div id='sidebar' style='grid-area:sidebar'></div>
                </div></body>");
            var header = LayoutTestHelper.FindById(root, "header")!;
            var main = LayoutTestHelper.FindById(root, "main")!;
            var sidebar = LayoutTestHelper.FindById(root, "sidebar")!;
            Assert.True(System.Math.Abs(header.ContentRect.Width - 200) < 2,
                $"header spans 2 cols: expected 200, got {header.ContentRect.Width}");
            Assert.True(System.Math.Abs(main.ContentRect.Width - 200) < 2,
                $"main spans 2 cols: expected 200, got {main.ContentRect.Width}");
            Assert.True(System.Math.Abs(sidebar.ContentRect.Height - 100) < 2,
                $"sidebar spans 2 rows: expected 100, got {sidebar.ContentRect.Height}");
        }

        // [CSS-GRID §8.5] named area spanning 3 columns
        [Fact]
        public void NamedArea_Span3Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-areas:""banner banner banner"" ""left center right"";grid-template-columns:80px 140px 80px;grid-template-rows:30px 50px;width:300px'>
                    <div id='banner' style='grid-area:banner'></div>
                    <div id='left' style='grid-area:left'></div>
                    <div id='center' style='grid-area:center'></div>
                    <div id='right' style='grid-area:right'></div>
                </div></body>");
            var banner = LayoutTestHelper.FindById(root, "banner")!;
            Assert.True(System.Math.Abs(banner.ContentRect.Width - 300) < 2,
                $"banner spans 3 cols: expected 300, got {banner.ContentRect.Width}");
            Assert.True(System.Math.Abs(banner.ContentRect.X - 0) < 2,
                $"banner starts at X=0, got {banner.ContentRect.X}");
        }

        // [CSS-GRID §8.5] named area spanning rows and columns (L-shaped layout)
        [Fact]
        public void NamedArea_SpanRowsAndColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-areas:""nav nav"" ""nav nav"" ""footer footer"";grid-template-columns:150px 150px;grid-template-rows:40px 40px 30px;width:300px'>
                    <div id='nav' style='grid-area:nav'></div>
                    <div id='footer' style='grid-area:footer'></div>
                </div></body>");
            var nav = LayoutTestHelper.FindById(root, "nav")!;
            var footer = LayoutTestHelper.FindById(root, "footer")!;
            Assert.True(System.Math.Abs(nav.ContentRect.Width - 300) < 2,
                $"nav spans 2 cols: expected 300, got {nav.ContentRect.Width}");
            Assert.True(System.Math.Abs(nav.ContentRect.Height - 80) < 2,
                $"nav spans 2 rows: expected 80, got {nav.ContentRect.Height}");
            Assert.True(System.Math.Abs(footer.ContentRect.Width - 300) < 2,
                $"footer spans 2 cols: expected 300, got {footer.ContentRect.Width}");
            Assert.True(System.Math.Abs(footer.ContentRect.Y - 80) < 2,
                $"footer Y after nav: expected 80, got {footer.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // Column span 2 X position with gap
        // ──────────────────────────────────────────────

        // [CSS-GRID §8.3] span 2 starting at column 2 with column-gap
        [Fact]
        public void ColumnSpan2_StartColumn2_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,60px);column-gap:20px;width:300px'>
                    <div id='span' style='grid-column:2/span 2;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.X - 80) < 2,
                $"span 2 at col 2 with gap: expected X=80, got {span.ContentRect.X}");
            Assert.True(System.Math.Abs(span.ContentRect.Width - 140) < 2,
                $"span 2 at col 2 with gap: expected W=140, got {span.ContentRect.Width}");
        }

        // ──────────────────────────────────────────────
        // Row span 3 height
        // ──────────────────────────────────────────────

        // [CSS-GRID §8.3] row span 3 in a 3-row grid occupies full height
        [Fact]
        public void RowSpan3_In3RowGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:30px 40px 50px;width:200px'>
                    <div id='span' style='grid-row:span 3'></div>
                    <div style='height:30px'></div>
                    <div style='height:40px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Height - 120) < 2,
                $"row span 3 in 3-row grid: expected 120, got {span.ContentRect.Height}");
        }

        // ──────────────────────────────────────────────
        // Combined column and row gap with spanning
        // ──────────────────────────────────────────────

        // [CSS-GRID §10.1] span 2 cols and span 2 rows with both gaps
        [Fact]
        public void ColumnAndRowSpan2_WithBothGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,60px);grid-template-rows:40px 40px 40px;column-gap:10px;row-gap:10px;width:200px'>
                    <div id='span' style='grid-column:span 2;grid-row:span 2;'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 130) < 2,
                $"col span 2 with gap 10: expected 130, got {span.ContentRect.Width}");
            Assert.True(System.Math.Abs(span.ContentRect.Height - 90) < 2,
                $"row span 2 with gap 10: expected 90, got {span.ContentRect.Height}");
        }

        // ──────────────────────────────────────────────
        // Span all with gap
        // ──────────────────────────────────────────────

        // [CSS-GRID §8.3] 1/-1 with column-gap still spans full grid width
        [Fact]
        public void SpanAll_WithColumnGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,80px);column-gap:10px;width:260px'>
                    <div id='span' style='grid-column:1/-1;height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(span.ContentRect.Width - 260) < 2,
                $"1/-1 with gap: expected 260, got {span.ContentRect.Width}");
        }
    }
}
