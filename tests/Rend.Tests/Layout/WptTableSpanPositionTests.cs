using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTableSpanPositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptTableSpanPositionTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Colspan2_CellXPosition_StartsAtFirstColumn()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px'>Span</td>
                        <td style='height:30px'>C</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>A</td>
                        <td style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"colspan2 X={span.ContentRect.X}");
            Assert.True(span.ContentRect.X < 5, $"colspan=2 cell should start near left edge (got {span.ContentRect.X})");
        }

        [Fact]
        public void Colspan2_CellWidth_SpansTwoColumns()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px'>Span</td>
                        <td style='height:30px'>C</td>
                    </tr>
                    <tr>
                        <td id='colA' style='height:30px'>A</td>
                        <td id='colB' style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            float expectedWidth = colA.ContentRect.Width + colB.ContentRect.Width;
            _output.WriteLine($"colspan2 w={span.ContentRect.Width}, colA={colA.ContentRect.Width}, colB={colB.ContentRect.Width}");
            Assert.True(System.Math.Abs(span.ContentRect.Width - expectedWidth) < 3,
                $"colspan=2 width should equal sum of two columns (got {span.ContentRect.Width}, expected ~{expectedWidth})");
        }

        [Fact]
        public void Colspan3_FullWidth_SpansEntireTable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='full' colspan='3' style='height:30px'>Full</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>A</td>
                        <td style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var full = LayoutTestHelper.FindById(root, "full")!;
            var tbl = LayoutTestHelper.FindById(root, "tbl")!;
            _output.WriteLine($"colspan3 w={full.ContentRect.Width}, table w={tbl.ContentRect.Width}");
            Assert.True(full.ContentRect.Width >= tbl.ContentRect.Width - 5,
                $"colspan=3 should span full table width (got {full.ContentRect.Width}, table={tbl.ContentRect.Width})");
        }

        [Fact]
        public void Rowspan2_CellYPosition_StartsAtFirstRow()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2'>Span</td>
                        <td id='firstRowCell' style='height:30px'>B1</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>B2</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var firstRowCell = LayoutTestHelper.FindById(root, "firstRowCell")!;
            _output.WriteLine($"rowspan2 Y={span.ContentRect.Y}, firstRowCell Y={firstRowCell.ContentRect.Y}");
            Assert.True(System.Math.Abs(span.ContentRect.Y - firstRowCell.ContentRect.Y) < 2,
                $"rowspan=2 cell Y should match first row (got {span.ContentRect.Y}, expected ~{firstRowCell.ContentRect.Y})");
        }

        [Fact]
        public void Rowspan2_CellHeight_SpansTwoRows()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2'>Span</td>
                        <td style='height:40px'>B1</td>
                    </tr>
                    <tr>
                        <td style='height:40px'>B2</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"rowspan2 h={span.ContentRect.Height}");
            Assert.True(span.ContentRect.Height >= 78,
                $"rowspan=2 height should span both rows (~80px, got {span.ContentRect.Height})");
        }

        [Fact]
        public void Rowspan3_CellHeight_SpansThreeRows()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='3'>Span</td>
                        <td style='height:25px'>B1</td>
                    </tr>
                    <tr><td style='height:25px'>B2</td></tr>
                    <tr><td style='height:25px'>B3</td></tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"rowspan3 h={span.ContentRect.Height}");
            Assert.True(span.ContentRect.Height >= 73,
                $"rowspan=3 height should span three rows (~75px, got {span.ContentRect.Height})");
        }

        [Fact]
        public void Colspan_WithBorderSpacing_WidthIncludesSpacing()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:separate;border-spacing:10px'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px'>Span</td>
                        <td style='height:30px'>C</td>
                    </tr>
                    <tr>
                        <td id='colA' style='height:30px'>A</td>
                        <td id='colB' style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            float twoColsWithSpacing = colA.ContentRect.Width + colB.ContentRect.Width + 10;
            _output.WriteLine($"colspan2+spacing w={span.ContentRect.Width}, expected ~{twoColsWithSpacing}");
            Assert.True(System.Math.Abs(span.ContentRect.Width - twoColsWithSpacing) < 5,
                $"colspan=2 with border-spacing should include inter-column spacing (got {span.ContentRect.Width}, expected ~{twoColsWithSpacing})");
        }

        [Fact]
        public void Rowspan_WithBorderSpacing_HeightIncludesSpacing()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:separate;border-spacing:10px'>
                    <tr>
                        <td id='span' rowspan='2'>Span</td>
                        <td style='height:30px'>B1</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>B2</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"rowspan2+spacing h={span.ContentRect.Height}");
            Assert.True(span.ContentRect.Height >= 68,
                $"rowspan=2 with spacing should include inter-row spacing (~70px, got {span.ContentRect.Height})");
        }

        [Fact]
        public void Colspan_CellContentWidth_MatchesLayoutWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:240px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px;padding:0'>Span</td>
                        <td style='height:30px;padding:0'>C</td>
                    </tr>
                    <tr>
                        <td style='height:30px;padding:0;width:80px'>A</td>
                        <td style='height:30px;padding:0;width:80px'>B</td>
                        <td style='height:30px;padding:0;width:80px'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"colspan2 content w={span.ContentRect.Width}");
            Assert.True(span.ContentRect.Width >= 155,
                $"colspan=2 content width should span two 80px columns (got {span.ContentRect.Width})");
        }

        [Fact]
        public void Rowspan_WithDifferentRowHeights_SpansCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2'>Span</td>
                        <td style='height:20px'>B1</td>
                    </tr>
                    <tr>
                        <td style='height:50px'>B2</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"rowspan2 diff heights h={span.ContentRect.Height}");
            Assert.True(span.ContentRect.Height >= 68,
                $"rowspan=2 should cover both row heights (20+50=70, got {span.ContentRect.Height})");
        }

        [Fact]
        public void AdjacentCells_AfterColspan_PositionedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td colspan='2' style='height:30px'>Span</td>
                        <td id='after' style='height:30px'>After</td>
                    </tr>
                    <tr>
                        <td id='colA' style='height:30px;width:100px'>A</td>
                        <td id='colB' style='height:30px;width:100px'>B</td>
                        <td style='height:30px;width:100px'>C</td>
                    </tr>
                </table></body>");
            var after = LayoutTestHelper.FindById(root, "after")!;
            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            float expectedX = colA.ContentRect.Width + colB.ContentRect.Width;
            _output.WriteLine($"after colspan X={after.ContentRect.X}, expectedX ~{expectedX}");
            Assert.True(after.ContentRect.X >= expectedX - 5,
                $"Cell after colspan=2 should be positioned in third column (got X={after.ContentRect.X})");
        }

        [Fact]
        public void CellsBelow_Rowspan_PositionedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td rowspan='2' style='width:100px'>Span</td>
                        <td style='height:30px'>B1</td>
                    </tr>
                    <tr>
                        <td id='below' style='height:30px'>B2</td>
                    </tr>
                </table></body>");
            var below = LayoutTestHelper.FindById(root, "below")!;
            _output.WriteLine($"below rowspan X={below.ContentRect.X}, Y={below.ContentRect.Y}");
            Assert.True(below.ContentRect.X >= 95,
                $"Cell next to rowspan should be in second column (got X={below.ContentRect.X})");
            Assert.True(below.ContentRect.Y >= 28,
                $"Cell in second row should be below first row (got Y={below.ContentRect.Y})");
        }

        [Fact]
        public void Colspan_InFirstRow_SetsColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px'>First row span</td>
                    </tr>
                    <tr>
                        <td id='col1' style='height:30px'>A</td>
                        <td id='col2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            _output.WriteLine($"firstRow span w={span.ContentRect.Width}, col1 w={col1.ContentRect.Width}, col2 w={col2.ContentRect.Width}");
            Assert.True(span.ContentRect.Width >= col1.ContentRect.Width + col2.ContentRect.Width - 3,
                $"First row colspan should span full width (got {span.ContentRect.Width})");
        }

        [Fact]
        public void Colspan_InLastRow_SpansCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:30px;width:100px'>A</td>
                        <td style='height:30px;width:100px'>B</td>
                    </tr>
                    <tr>
                        <td id='span' colspan='2' style='height:30px'>Last row span</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"lastRow span w={span.ContentRect.Width}");
            Assert.True(span.ContentRect.Width >= 195,
                $"Last row colspan=2 should span full table width (got {span.ContentRect.Width})");
        }

        [Fact]
        public void Rowspan_InFirstColumn_HeightSpansRows()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2'>First col span</td>
                        <td style='height:35px'>B1</td>
                    </tr>
                    <tr>
                        <td style='height:35px'>B2</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"firstCol rowspan h={span.ContentRect.Height}, X={span.ContentRect.X}");
            Assert.True(span.ContentRect.Height >= 68,
                $"First column rowspan=2 should span both rows (got {span.ContentRect.Height})");
            Assert.True(span.ContentRect.X < 5,
                $"First column rowspan should start at left edge (got X={span.ContentRect.X})");
        }

        [Fact]
        public void Rowspan_InLastColumn_HeightSpansRows()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='firstCol' style='height:35px;width:100px'>A1</td>
                        <td id='span' rowspan='2'>Last col span</td>
                    </tr>
                    <tr>
                        <td style='height:35px'>A2</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var firstCol = LayoutTestHelper.FindById(root, "firstCol")!;
            _output.WriteLine($"lastCol rowspan h={span.ContentRect.Height}, X={span.ContentRect.X}");
            Assert.True(span.ContentRect.Height >= 68,
                $"Last column rowspan=2 should span both rows (got {span.ContentRect.Height})");
            Assert.True(span.ContentRect.X >= firstCol.ContentRect.Width - 5,
                $"Last column rowspan should be in second column (got X={span.ContentRect.X})");
        }

        [Fact]
        public void MixedColspanAndRowspan_PositionsCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='cspan' colspan='2' style='height:30px'>Col span</td>
                        <td id='rspan' rowspan='2'>Row span</td>
                    </tr>
                    <tr>
                        <td id='below1' style='height:30px;width:100px'>A2</td>
                        <td id='below2' style='height:30px;width:100px'>B2</td>
                    </tr>
                </table></body>");
            var cspan = LayoutTestHelper.FindById(root, "cspan")!;
            var rspan = LayoutTestHelper.FindById(root, "rspan")!;
            var below1 = LayoutTestHelper.FindById(root, "below1")!;
            var below2 = LayoutTestHelper.FindById(root, "below2")!;
            _output.WriteLine($"cspan w={cspan.ContentRect.Width}, rspan h={rspan.ContentRect.Height}");
            _output.WriteLine($"below1 X={below1.ContentRect.X}, below2 X={below2.ContentRect.X}");
            float expectedColspanWidth = below1.ContentRect.Width + below2.ContentRect.Width;
            Assert.True(System.Math.Abs(cspan.ContentRect.Width - expectedColspanWidth) < 5,
                $"colspan=2 should span two columns (got {cspan.ContentRect.Width}, expected ~{expectedColspanWidth})");
            Assert.True(rspan.ContentRect.Height >= 58,
                $"rowspan=2 should span both rows (got {rspan.ContentRect.Height})");
            Assert.True(below1.ContentRect.X < 5,
                $"First cell in second row should be at left (got X={below1.ContentRect.X})");
        }

        [Fact]
        public void TwoColspans_InSameRow_PositionedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='span1' colspan='2' style='height:30px'>Span1</td>
                        <td id='span2' colspan='2' style='height:30px'>Span2</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>A</td>
                        <td style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                        <td style='height:30px'>D</td>
                    </tr>
                </table></body>");
            var span1 = LayoutTestHelper.FindById(root, "span1")!;
            var span2 = LayoutTestHelper.FindById(root, "span2")!;
            _output.WriteLine($"span1 X={span1.ContentRect.X} w={span1.ContentRect.Width}, span2 X={span2.ContentRect.X} w={span2.ContentRect.Width}");
            Assert.True(span1.ContentRect.X < span2.ContentRect.X,
                $"First colspan should be left of second (span1 X={span1.ContentRect.X}, span2 X={span2.ContentRect.X})");
            Assert.True(System.Math.Abs(span1.ContentRect.Width - span2.ContentRect.Width) < 5,
                $"Both colspan=2 should have similar width (span1={span1.ContentRect.Width}, span2={span2.ContentRect.Width})");
        }

        [Fact]
        public void TwoRowspans_InSameColumn_StackVertically()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='rspan1' rowspan='2'>R1</td>
                        <td style='height:30px'>B1</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>B2</td>
                    </tr>
                    <tr>
                        <td id='rspan2' rowspan='2'>R2</td>
                        <td style='height:30px'>B3</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>B4</td>
                    </tr>
                </table></body>");
            var rspan1 = LayoutTestHelper.FindById(root, "rspan1")!;
            var rspan2 = LayoutTestHelper.FindById(root, "rspan2")!;
            _output.WriteLine($"rspan1 Y={rspan1.ContentRect.Y} h={rspan1.ContentRect.Height}");
            _output.WriteLine($"rspan2 Y={rspan2.ContentRect.Y} h={rspan2.ContentRect.Height}");
            Assert.True(rspan2.ContentRect.Y >= rspan1.ContentRect.Y + rspan1.ContentRect.Height - 2,
                $"Second rowspan should start after first (rspan1 bottom={rspan1.ContentRect.Y + rspan1.ContentRect.Height}, rspan2 Y={rspan2.ContentRect.Y})");
            Assert.True(rspan1.ContentRect.Height >= 58,
                $"First rowspan=2 height (got {rspan1.ContentRect.Height})");
            Assert.True(rspan2.ContentRect.Height >= 58,
                $"Second rowspan=2 height (got {rspan2.ContentRect.Height})");
        }

        [Fact]
        public void EmptyCells_AdjacentToColspan_PositionedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='empty1' style='height:30px'></td>
                        <td colspan='2' style='height:30px'>Span</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>A</td>
                        <td style='height:30px'>B</td>
                        <td id='empty2' style='height:30px'></td>
                    </tr>
                </table></body>");
            var empty1 = LayoutTestHelper.FindById(root, "empty1")!;
            var empty2 = LayoutTestHelper.FindById(root, "empty2")!;
            _output.WriteLine($"empty1 X={empty1.ContentRect.X} w={empty1.ContentRect.Width}");
            _output.WriteLine($"empty2 X={empty2.ContentRect.X} w={empty2.ContentRect.Width}");
            Assert.True(empty1.ContentRect.X < 5,
                $"Empty cell before colspan should be at left (got X={empty1.ContentRect.X})");
            Assert.True(empty1.ContentRect.Width > 0,
                $"Empty cell should have width (got {empty1.ContentRect.Width})");
            Assert.True(empty2.ContentRect.X >= 195,
                $"Empty cell in third column should be positioned right (got X={empty2.ContentRect.X})");
        }

        [Fact]
        public void EmptyCells_AdjacentToRowspan_PositionedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='empty1' style='height:30px'></td>
                        <td rowspan='2'>Span</td>
                    </tr>
                    <tr>
                        <td id='empty2' style='height:30px'></td>
                    </tr>
                </table></body>");
            var empty1 = LayoutTestHelper.FindById(root, "empty1")!;
            var empty2 = LayoutTestHelper.FindById(root, "empty2")!;
            _output.WriteLine($"empty1 Y={empty1.ContentRect.Y}, empty2 Y={empty2.ContentRect.Y}");
            Assert.True(empty2.ContentRect.Y > empty1.ContentRect.Y,
                $"Second empty cell should be below first (empty1 Y={empty1.ContentRect.Y}, empty2 Y={empty2.ContentRect.Y})");
            Assert.True(empty1.ContentRect.X < 5,
                $"Empty cells should be in first column (got X={empty1.ContentRect.X})");
            Assert.True(empty2.ContentRect.X < 5,
                $"Second empty cell should also be in first column (got X={empty2.ContentRect.X})");
        }

        [Fact]
        public void ThreeByThree_WithColspan_CellPositions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='r1c1' style='height:30px'>R1C1</td>
                        <td id='r1c2' style='height:30px'>R1C2</td>
                        <td id='r1c3' style='height:30px'>R1C3</td>
                    </tr>
                    <tr>
                        <td id='r2span' colspan='2' style='height:30px'>R2 Span</td>
                        <td id='r2c3' style='height:30px'>R2C3</td>
                    </tr>
                    <tr>
                        <td id='r3c1' style='height:30px'>R3C1</td>
                        <td id='r3c2' style='height:30px'>R3C2</td>
                        <td id='r3c3' style='height:30px'>R3C3</td>
                    </tr>
                </table></body>");
            var r1c1 = LayoutTestHelper.FindById(root, "r1c1")!;
            var r1c3 = LayoutTestHelper.FindById(root, "r1c3")!;
            var r2span = LayoutTestHelper.FindById(root, "r2span")!;
            var r2c3 = LayoutTestHelper.FindById(root, "r2c3")!;
            var r3c1 = LayoutTestHelper.FindById(root, "r3c1")!;
            _output.WriteLine($"r1c1 X={r1c1.ContentRect.X}, r1c3 X={r1c3.ContentRect.X}");
            _output.WriteLine($"r2span w={r2span.ContentRect.Width}, r2c3 X={r2c3.ContentRect.X}");
            _output.WriteLine($"r3c1 Y={r3c1.ContentRect.Y}");

            Assert.True(r2span.ContentRect.X < 5,
                $"Colspan in row 2 starts at left (got X={r2span.ContentRect.X})");
            Assert.True(System.Math.Abs(r2c3.ContentRect.X - r1c3.ContentRect.X) < 3,
                $"Cell after colspan aligns with column above (r2c3 X={r2c3.ContentRect.X}, r1c3 X={r1c3.ContentRect.X})");
            Assert.True(r3c1.ContentRect.Y > r2span.ContentRect.Y,
                $"Row 3 is below row 2 (r3c1 Y={r3c1.ContentRect.Y}, r2span Y={r2span.ContentRect.Y})");
        }

        [Fact]
        public void ThreeByThree_WithRowspan_CellPositions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='r1c1' style='height:30px'>R1C1</td>
                        <td id='rspan' rowspan='2'>RSpan</td>
                        <td id='r1c3' style='height:30px'>R1C3</td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='height:30px'>R2C1</td>
                        <td id='r2c3' style='height:30px'>R2C3</td>
                    </tr>
                    <tr>
                        <td id='r3c1' style='height:30px'>R3C1</td>
                        <td id='r3c2' style='height:30px'>R3C2</td>
                        <td id='r3c3' style='height:30px'>R3C3</td>
                    </tr>
                </table></body>");
            var rspan = LayoutTestHelper.FindById(root, "rspan")!;
            var r2c1 = LayoutTestHelper.FindById(root, "r2c1")!;
            var r2c3 = LayoutTestHelper.FindById(root, "r2c3")!;
            var r3c2 = LayoutTestHelper.FindById(root, "r3c2")!;
            _output.WriteLine($"rspan Y={rspan.ContentRect.Y} h={rspan.ContentRect.Height} X={rspan.ContentRect.X}");
            _output.WriteLine($"r2c1 X={r2c1.ContentRect.X}, r2c3 X={r2c3.ContentRect.X}");
            _output.WriteLine($"r3c2 X={r3c2.ContentRect.X} Y={r3c2.ContentRect.Y}");

            Assert.True(rspan.ContentRect.Height >= 58,
                $"Rowspan=2 spans two rows (got h={rspan.ContentRect.Height})");
            Assert.True(r2c1.ContentRect.X < 5,
                $"Cell in row 2 col 1 is at left (got X={r2c1.ContentRect.X})");
            Assert.True(System.Math.Abs(r3c2.ContentRect.X - rspan.ContentRect.X) < 3,
                $"Cell below rowspan aligns with same column (r3c2 X={r3c2.ContentRect.X}, rspan X={rspan.ContentRect.X})");
        }

        [Fact]
        public void Colspan2_MiddleColumns_XPositionMatchesColumn()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='col1' style='height:30px;width:100px'>A</td>
                        <td id='span' colspan='2' style='height:30px'>Span</td>
                        <td id='col4' style='height:30px;width:100px'>D</td>
                    </tr>
                    <tr>
                        <td style='height:30px;width:100px'>A</td>
                        <td style='height:30px;width:100px'>B</td>
                        <td style='height:30px;width:100px'>C</td>
                        <td style='height:30px;width:100px'>D</td>
                    </tr>
                </table></body>");
            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var span = LayoutTestHelper.FindById(root, "span")!;
            var col4 = LayoutTestHelper.FindById(root, "col4")!;
            _output.WriteLine($"col1 X={col1.ContentRect.X} w={col1.ContentRect.Width}");
            _output.WriteLine($"span X={span.ContentRect.X} w={span.ContentRect.Width}");
            _output.WriteLine($"col4 X={col4.ContentRect.X} w={col4.ContentRect.Width}");

            Assert.True(span.ContentRect.X >= col1.ContentRect.X + col1.ContentRect.Width - 3,
                $"Middle colspan starts after first column (got X={span.ContentRect.X})");
            Assert.True(col4.ContentRect.X >= span.ContentRect.X + span.ContentRect.Width - 3,
                $"Fourth column starts after middle colspan (got X={col4.ContentRect.X})");
        }

        [Fact]
        public void Colspan_WithPadding_ContentWidthExcludesPadding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px;padding:10px'>Span</td>
                        <td style='height:30px;padding:10px'>C</td>
                    </tr>
                    <tr>
                        <td style='height:30px;padding:10px'>A</td>
                        <td style='height:30px;padding:10px'>B</td>
                        <td style='height:30px;padding:10px'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"span content w={span.ContentRect.Width}, border w={span.BorderRect.Width}");
            Assert.True(span.BorderRect.Width > span.ContentRect.Width,
                $"Border rect should be wider than content rect due to padding (border={span.BorderRect.Width}, content={span.ContentRect.Width})");
        }

        [Fact]
        public void Rowspan_SameXAsNonSpanning_InSameColumn()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='normal' style='height:30px'>Normal</td>
                        <td style='height:30px'>B1</td>
                    </tr>
                    <tr>
                        <td id='rspan' rowspan='2'>Rowspan</td>
                        <td style='height:30px'>B2</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>B3</td>
                    </tr>
                </table></body>");
            var normal = LayoutTestHelper.FindById(root, "normal")!;
            var rspan = LayoutTestHelper.FindById(root, "rspan")!;
            _output.WriteLine($"normal X={normal.ContentRect.X}, rspan X={rspan.ContentRect.X}");
            Assert.True(System.Math.Abs(normal.ContentRect.X - rspan.ContentRect.X) < 2,
                $"Rowspan cell should have same X as normal cell in same column (normal X={normal.ContentRect.X}, rspan X={rspan.ContentRect.X})");
        }

        [Fact]
        public void Colspan_SameYAsNonSpanning_InSameRow()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px'>Span</td>
                        <td id='normal' style='height:30px'>Normal</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>A</td>
                        <td style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var normal = LayoutTestHelper.FindById(root, "normal")!;
            _output.WriteLine($"span Y={span.ContentRect.Y}, normal Y={normal.ContentRect.Y}");
            Assert.True(System.Math.Abs(span.ContentRect.Y - normal.ContentRect.Y) < 2,
                $"Colspan cell should have same Y as adjacent cell in same row (span Y={span.ContentRect.Y}, normal Y={normal.ContentRect.Y})");
        }

        [Fact]
        public void BorderSpacing_Colspan2_XOffset()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:separate;border-spacing:8px'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px'>Span</td>
                        <td style='height:30px'>C</td>
                    </tr>
                    <tr>
                        <td id='colA' style='height:30px'>A</td>
                        <td style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var colA = LayoutTestHelper.FindById(root, "colA")!;
            _output.WriteLine($"span X={span.ContentRect.X}, colA X={colA.ContentRect.X}");
            Assert.True(System.Math.Abs(span.ContentRect.X - colA.ContentRect.X) < 2,
                $"Colspan and first cell should start at same X with border-spacing (span X={span.ContentRect.X}, colA X={colA.ContentRect.X})");
        }

        [Fact]
        public void BorderSpacing_Rowspan2_YOffset()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:separate;border-spacing:8px'>
                    <tr>
                        <td id='rspan' rowspan='2'>Span</td>
                        <td id='firstCell' style='height:30px'>B1</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>B2</td>
                    </tr>
                </table></body>");
            var rspan = LayoutTestHelper.FindById(root, "rspan")!;
            var firstCell = LayoutTestHelper.FindById(root, "firstCell")!;
            _output.WriteLine($"rspan Y={rspan.ContentRect.Y}, firstCell Y={firstCell.ContentRect.Y}");
            Assert.True(System.Math.Abs(rspan.ContentRect.Y - firstCell.ContentRect.Y) < 2,
                $"Rowspan and first row cell should start at same Y with border-spacing (rspan Y={rspan.ContentRect.Y}, firstCell Y={firstCell.ContentRect.Y})");
        }
    }
}
