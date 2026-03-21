using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTableStructureTests
    {
        private readonly ITestOutputHelper _output;

        public WptTableStructureTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Basic2x2_CellPositions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='a' style='height:40px'>A</td>
                        <td id='b' style='height:40px'>B</td>
                    </tr>
                    <tr>
                        <td id='c' style='height:40px'>C</td>
                        <td id='d' style='height:40px'>D</td>
                    </tr>
                </table></body>");

            var cellA = LayoutTestHelper.FindById(root, "a")!;
            var cellB = LayoutTestHelper.FindById(root, "b")!;
            var cellC = LayoutTestHelper.FindById(root, "c")!;
            var cellD = LayoutTestHelper.FindById(root, "d")!;

            _output.WriteLine($"A=({cellA.ContentRect.X},{cellA.ContentRect.Y}) B=({cellB.ContentRect.X},{cellB.ContentRect.Y})");
            _output.WriteLine($"C=({cellC.ContentRect.X},{cellC.ContentRect.Y}) D=({cellD.ContentRect.X},{cellD.ContentRect.Y})");

            Assert.True(cellB.ContentRect.X > cellA.ContentRect.X, "B is right of A");
            Assert.True(cellC.ContentRect.Y > cellA.ContentRect.Y, "C is below A");
            Assert.True(cellD.ContentRect.X > cellC.ContentRect.X, "D is right of C");
            Assert.True(System.Math.Abs(cellA.ContentRect.Y - cellB.ContentRect.Y) < 1, "A and B same row Y");
            Assert.True(System.Math.Abs(cellC.ContentRect.Y - cellD.ContentRect.Y) < 1, "C and D same row Y");
        }

        [Fact]
        public void Basic3x3_CellPositions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='r1c1' style='height:30px'>1-1</td>
                        <td id='r1c2' style='height:30px'>1-2</td>
                        <td id='r1c3' style='height:30px'>1-3</td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='height:30px'>2-1</td>
                        <td id='r2c2' style='height:30px'>2-2</td>
                        <td id='r2c3' style='height:30px'>2-3</td>
                    </tr>
                    <tr>
                        <td id='r3c1' style='height:30px'>3-1</td>
                        <td id='r3c2' style='height:30px'>3-2</td>
                        <td id='r3c3' style='height:30px'>3-3</td>
                    </tr>
                </table></body>");

            var r1c1 = LayoutTestHelper.FindById(root, "r1c1")!;
            var r1c3 = LayoutTestHelper.FindById(root, "r1c3")!;
            var r2c2 = LayoutTestHelper.FindById(root, "r2c2")!;
            var r3c1 = LayoutTestHelper.FindById(root, "r3c1")!;
            var r3c3 = LayoutTestHelper.FindById(root, "r3c3")!;

            Assert.True(r1c3.ContentRect.X > r1c1.ContentRect.X, "Column 3 right of column 1");
            Assert.True(r3c1.ContentRect.Y > r1c1.ContentRect.Y, "Row 3 below row 1");
            Assert.True(r2c2.ContentRect.X > r1c1.ContentRect.X, "Center column right of first");
            Assert.True(r2c2.ContentRect.Y > r1c1.ContentRect.Y, "Center row below first");
            Assert.True(r3c3.ContentRect.X > r3c1.ContentRect.X, "Last cell right of first in last row");
            Assert.True(r3c3.ContentRect.Y > r1c3.ContentRect.Y, "Last cell below first in last column");
        }

        [Fact]
        public void Basic4x4_TableDimensions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td style='height:25px'>1</td><td style='height:25px'>2</td>
                        <td style='height:25px'>3</td><td style='height:25px'>4</td>
                    </tr>
                    <tr>
                        <td style='height:25px'>5</td><td style='height:25px'>6</td>
                        <td style='height:25px'>7</td><td style='height:25px'>8</td>
                    </tr>
                    <tr>
                        <td style='height:25px'>9</td><td style='height:25px'>10</td>
                        <td style='height:25px'>11</td><td style='height:25px'>12</td>
                    </tr>
                    <tr>
                        <td style='height:25px'>13</td><td id='last' style='height:25px'>16</td>
                        <td style='height:25px'>15</td><td style='height:25px'>16</td>
                    </tr>
                </table></body>");

            var table = LayoutTestHelper.FindById(root, "tbl")!;
            var allCells = LayoutTestHelper.FindAllByTag(root, "td");

            _output.WriteLine($"4x4 table: {table.ContentRect.Width}x{table.ContentRect.Height}, cells={allCells.Count}");

            Assert.Equal(16, allCells.Count);
            Assert.True(table.ContentRect.Width >= 398, $"Table width ~400 (got {table.ContentRect.Width})");
            Assert.True(table.ContentRect.Height >= 99, $"4 rows x 25px = 100 (got {table.ContentRect.Height})");
        }

        [Fact]
        public void BorderSpacing_CellPositions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:separate;border-spacing:10px'>
                    <tr>
                        <td id='a' style='height:30px'>A</td>
                        <td id='b' style='height:30px'>B</td>
                    </tr>
                    <tr>
                        <td id='c' style='height:30px'>C</td>
                        <td id='d' style='height:30px'>D</td>
                    </tr>
                </table></body>");

            var table = LayoutTestHelper.FindById(root, "tbl")!;
            var cellA = LayoutTestHelper.FindById(root, "a")!;
            var cellB = LayoutTestHelper.FindById(root, "b")!;
            var cellC = LayoutTestHelper.FindById(root, "c")!;

            _output.WriteLine($"table h={table.ContentRect.Height}");
            _output.WriteLine($"A=({cellA.ContentRect.X},{cellA.ContentRect.Y}) B=({cellB.ContentRect.X},{cellB.ContentRect.Y})");

            Assert.True(cellB.ContentRect.X - (cellA.ContentRect.X + cellA.ContentRect.Width) >= 9,
                "Horizontal spacing between cells");
            Assert.True(cellC.ContentRect.Y - (cellA.ContentRect.Y + cellA.ContentRect.Height) >= 9,
                "Vertical spacing between rows");
            // top(10) + row1(30) + between(10) + row2(30) + bottom(10) = 90
            Assert.True(table.ContentRect.Height >= 89,
                $"Table height includes spacing (got {table.ContentRect.Height})");
        }

        [Fact]
        public void BorderCollapse_SharedBorders()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse;border:2px solid black'>
                    <tr>
                        <td id='a' style='border:2px solid black;height:30px'>A</td>
                        <td id='b' style='border:2px solid black;height:30px'>B</td>
                    </tr>
                </table></body>");

            var table = LayoutTestHelper.FindById(root, "tbl")!;
            var cellA = LayoutTestHelper.FindById(root, "a")!;
            var cellB = LayoutTestHelper.FindById(root, "b")!;

            _output.WriteLine($"table border: {table.BorderRect.Width}x{table.BorderRect.Height}");
            _output.WriteLine($"A border: {cellA.BorderRect.Width} B border: {cellB.BorderRect.Width}");

            float totalCellBorderWidth = cellA.BorderRect.Width + cellB.BorderRect.Width;
            Assert.True(totalCellBorderWidth <= table.BorderRect.Width + 2,
                $"Collapsed borders share edges (cells={totalCellBorderWidth} table={table.BorderRect.Width})");
        }

        [Fact]
        public void ExplicitCellWidths()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='narrow' style='width:80px;height:30px'>Narrow</td>
                        <td id='wide' style='width:220px;height:30px'>Wide</td>
                    </tr>
                </table></body>");

            var narrow = LayoutTestHelper.FindById(root, "narrow")!;
            var wide = LayoutTestHelper.FindById(root, "wide")!;

            _output.WriteLine($"narrow={narrow.ContentRect.Width} wide={wide.ContentRect.Width}");

            Assert.True(narrow.ContentRect.Width < wide.ContentRect.Width,
                "Narrow cell should be narrower than wide cell");
            Assert.True(narrow.ContentRect.Width >= 75 && narrow.ContentRect.Width <= 85,
                $"Narrow cell ~80px (got {narrow.ContentRect.Width})");
        }

        [Fact]
        public void PercentageCellWidths()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='quarter' style='width:25%;height:30px'>25%</td>
                        <td id='three_quarter' style='width:75%;height:30px'>75%</td>
                    </tr>
                </table></body>");

            var quarter = LayoutTestHelper.FindById(root, "quarter")!;
            var threeQuarter = LayoutTestHelper.FindById(root, "three_quarter")!;

            _output.WriteLine($"25%={quarter.ContentRect.Width} 75%={threeQuarter.ContentRect.Width}");

            Assert.True(threeQuarter.ContentRect.Width > quarter.ContentRect.Width * 2,
                "75% cell should be more than 2x the 25% cell");
        }

        [Fact]
        public void TableWidthFull()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:500px'>
                    <table id='tbl' style='width:100%;border-collapse:collapse'>
                        <tr><td style='height:30px'>A</td></tr>
                    </table>
                </div></body>");

            var table = LayoutTestHelper.FindById(root, "tbl")!;

            _output.WriteLine($"table width={table.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 500) < 2,
                $"width:100% of 500px container = 500 (got {table.ContentRect.Width})");
        }

        [Fact]
        public void TableAutoWidth_ShrinksToContent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:600px'>
                    <table id='tbl' style='border-collapse:collapse'>
                        <tr>
                            <td style='width:100px;height:30px'>A</td>
                            <td style='width:50px;height:30px'>B</td>
                        </tr>
                    </table>
                </div></body>");

            var table = LayoutTestHelper.FindById(root, "tbl")!;

            _output.WriteLine($"auto table width={table.ContentRect.Width}");
            Assert.True(table.ContentRect.Width < 300,
                $"Auto-width table should not fill container (got {table.ContentRect.Width})");
            Assert.True(table.ContentRect.Width >= 148,
                $"Auto-width table should fit cells (got {table.ContentRect.Width})");
        }

        [Fact]
        public void TheadTbodyTfoot_Ordering()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <thead><tr><td id='head' style='height:20px'>Head</td></tr></thead>
                    <tbody><tr><td id='body' style='height:40px'>Body</td></tr></tbody>
                    <tfoot><tr><td id='foot' style='height:20px'>Foot</td></tr></tfoot>
                </table></body>");

            var headCell = LayoutTestHelper.FindById(root, "head")!;
            var bodyCell = LayoutTestHelper.FindById(root, "body")!;
            var footCell = LayoutTestHelper.FindById(root, "foot")!;

            _output.WriteLine($"head Y={headCell.ContentRect.Y} body Y={bodyCell.ContentRect.Y} foot Y={footCell.ContentRect.Y}");

            Assert.True(headCell.ContentRect.Y < bodyCell.ContentRect.Y,
                "thead should be above tbody");
            Assert.True(bodyCell.ContentRect.Y < footCell.ContentRect.Y,
                "tbody should be above tfoot");
        }

        [Fact]
        public void CaptionPosition_AboveTable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                    <caption id='cap'>Table Caption</caption>
                    <tr><td id='cell' style='height:30px'>A</td></tr>
                </table></body>");

            var caption = LayoutTestHelper.FindById(root, "cap");
            var cell = LayoutTestHelper.FindById(root, "cell")!;

            if (caption != null)
            {
                _output.WriteLine($"caption Y={caption.ContentRect.Y} cell Y={cell.ContentRect.Y}");
                Assert.True(caption.ContentRect.Y < cell.ContentRect.Y,
                    "Caption should be above table cells");
            }
        }

        [Fact]
        public void RowspanHeight_SpansTwoRows()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='spanning' rowspan='2'>Spans</td>
                        <td id='r1' style='height:40px'>R1</td>
                    </tr>
                    <tr>
                        <td id='r2' style='height:40px'>R2</td>
                    </tr>
                </table></body>");

            var spanning = LayoutTestHelper.FindById(root, "spanning")!;
            var row1Cell = LayoutTestHelper.FindById(root, "r1")!;
            var row2Cell = LayoutTestHelper.FindById(root, "r2")!;

            _output.WriteLine($"spanning h={spanning.ContentRect.Height} r1 h={row1Cell.ContentRect.Height} r2 h={row2Cell.ContentRect.Height}");

            Assert.True(spanning.ContentRect.Height >= 79,
                $"Rowspan=2 cell should span both 40px rows (got {spanning.ContentRect.Height})");
            Assert.True(spanning.ContentRect.Y <= row1Cell.ContentRect.Y + 1,
                "Spanning cell starts at first row");
        }

        [Fact]
        public void ColspanWidth_SpansTwoColumns()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='spanning' colspan='2' style='height:30px'>Spans 2</td>
                        <td style='height:30px'>C</td>
                    </tr>
                    <tr>
                        <td id='single' style='height:30px'>A</td>
                        <td style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");

            var spanning = LayoutTestHelper.FindById(root, "spanning")!;
            var single = LayoutTestHelper.FindById(root, "single")!;

            _output.WriteLine($"spanning w={spanning.ContentRect.Width} single w={single.ContentRect.Width}");

            Assert.True(spanning.ContentRect.Width > single.ContentRect.Width * 1.5,
                $"Colspan=2 cell wider than single (span={spanning.ContentRect.Width} single={single.ContentRect.Width})");
        }

        [Fact]
        public void RowHeight_DeterminedByTallestCell()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='tall' style='height:80px'>Tall</td>
                        <td id='short' style='height:30px'>Short</td>
                    </tr>
                </table></body>");

            var tallCell = LayoutTestHelper.FindById(root, "tall")!;
            var shortCell = LayoutTestHelper.FindById(root, "short")!;

            _output.WriteLine($"tall h={tallCell.ContentRect.Height} short h={shortCell.ContentRect.Height}");

            Assert.True(System.Math.Abs(tallCell.ContentRect.Height - shortCell.ContentRect.Height) < 2,
                $"Both cells should match tallest height (tall={tallCell.ContentRect.Height} short={shortCell.ContentRect.Height})");
            Assert.True(shortCell.ContentRect.Height >= 79,
                $"Short cell stretched to match tall cell (got {shortCell.ContentRect.Height})");
        }

        [Fact]
        public void MultipleRows_StackVertically()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr><td id='r1' style='height:30px'>Row 1</td></tr>
                    <tr><td id='r2' style='height:30px'>Row 2</td></tr>
                    <tr><td id='r3' style='height:30px'>Row 3</td></tr>
                </table></body>");

            var row1 = LayoutTestHelper.FindById(root, "r1")!;
            var row2 = LayoutTestHelper.FindById(root, "r2")!;
            var row3 = LayoutTestHelper.FindById(root, "r3")!;

            _output.WriteLine($"r1 Y={row1.ContentRect.Y} r2 Y={row2.ContentRect.Y} r3 Y={row3.ContentRect.Y}");

            Assert.True(row2.ContentRect.Y >= row1.ContentRect.Y + 29, "Row 2 below row 1");
            Assert.True(row3.ContentRect.Y >= row2.ContentRect.Y + 29, "Row 3 below row 2");
            Assert.True(row3.ContentRect.Y >= row1.ContentRect.Y + 58, "Row 3 well below row 1");
        }

        [Fact]
        public void CellPadding_AffectsContentRect()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='padded' style='padding:10px;height:30px'>Padded</td>
                        <td id='unpadded' style='padding:0;height:30px'>No padding</td>
                    </tr>
                </table></body>");

            var padded = LayoutTestHelper.FindById(root, "padded")!;
            var unpadded = LayoutTestHelper.FindById(root, "unpadded")!;

            _output.WriteLine($"padded content={padded.ContentRect.Width} border={padded.BorderRect.Width}");
            _output.WriteLine($"unpadded content={unpadded.ContentRect.Width} border={unpadded.BorderRect.Width}");

            Assert.True(padded.PaddingLeft >= 9, $"Left padding applied (got {padded.PaddingLeft})");
            Assert.True(padded.PaddingTop >= 9, $"Top padding applied (got {padded.PaddingTop})");
            Assert.True(padded.BorderRect.Width > padded.ContentRect.Width,
                "Border rect wider than content rect due to padding");
        }

        [Fact]
        public void NestedTable_RendersInsideCell()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='outer' style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='host'>
                            <table id='inner' style='width:100%;border-collapse:collapse'>
                                <tr><td id='inner_cell' style='height:20px'>Inner</td></tr>
                            </table>
                        </td>
                        <td style='width:100px;height:40px'>Side</td>
                    </tr>
                </table></body>");

            var outer = LayoutTestHelper.FindById(root, "outer")!;
            var hostCell = LayoutTestHelper.FindById(root, "host")!;
            var inner = LayoutTestHelper.FindById(root, "inner");
            var innerCell = LayoutTestHelper.FindById(root, "inner_cell");

            _output.WriteLine($"outer w={outer.ContentRect.Width} host w={hostCell.ContentRect.Width}");

            Assert.NotNull(inner);
            Assert.NotNull(innerCell);
            Assert.True(inner!.ContentRect.X >= hostCell.ContentRect.X,
                "Inner table starts within host cell");
            Assert.True(inner.ContentRect.Width <= hostCell.BorderRect.Width + 1,
                $"Inner table fits in host cell (inner={inner.ContentRect.Width} host border={hostCell.BorderRect.Width})");
        }

        [Fact]
        public void TableLayoutFixed_FirstRowDefinesWidths()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='col1' style='width:100px;height:30px'>Col1</td>
                        <td id='col2' style='width:200px;height:30px'>Col2</td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='height:30px'>R2C1</td>
                        <td id='r2c2' style='height:30px'>R2C2</td>
                    </tr>
                </table></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var row2col1 = LayoutTestHelper.FindById(root, "r2c1")!;
            var row2col2 = LayoutTestHelper.FindById(root, "r2c2")!;

            _output.WriteLine($"col1={col1.ContentRect.Width} col2={col2.ContentRect.Width}");
            _output.WriteLine($"r2c1={row2col1.ContentRect.Width} r2c2={row2col2.ContentRect.Width}");

            Assert.True(col2.ContentRect.Width > col1.ContentRect.Width,
                "200px column wider than 100px column in fixed layout");
            Assert.True(System.Math.Abs(col1.ContentRect.Width - row2col1.ContentRect.Width) < 2,
                "Second row inherits first row column widths in fixed layout");
        }

        [Fact]
        public void TableInFlex_ParticipatesInFlexLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='before' style='width:100px;height:50px;background:red'></div>
                    <table id='tbl' style='border-collapse:collapse'>
                        <tr><td style='width:150px;height:50px'>Cell</td></tr>
                    </table>
                    <div id='after' style='width:100px;height:50px;background:blue'></div>
                </div></body>");

            var before = LayoutTestHelper.FindById(root, "before")!;
            var table = LayoutTestHelper.FindById(root, "tbl")!;
            var after = LayoutTestHelper.FindById(root, "after")!;

            _output.WriteLine($"before X={before.ContentRect.X} table X={table.ContentRect.X} after X={after.ContentRect.X}");

            Assert.True(table.ContentRect.X > before.ContentRect.X,
                "Table placed after first flex item");
            Assert.True(after.ContentRect.X > table.ContentRect.X,
                "Third item placed after table");
        }

        [Fact]
        public void TableInGrid_ParticipatesInGridLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <table id='tbl' style='border-collapse:collapse'>
                        <tr><td style='height:40px'>Cell</td></tr>
                    </table>
                    <div id='sibling' style='height:40px;background:red'></div>
                </div></body>");

            var table = LayoutTestHelper.FindById(root, "tbl")!;
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;

            _output.WriteLine($"table X={table.ContentRect.X} sibling X={sibling.ContentRect.X}");

            Assert.True(sibling.ContentRect.X > table.ContentRect.X,
                "Sibling placed in second grid column, right of table");
            Assert.True(System.Math.Abs(table.ContentRect.Y - sibling.ContentRect.Y) < 2,
                "Table and sibling on same grid row");
        }

        [Fact]
        public void EmptyCells_StillOccupySpace()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='filled' style='height:30px'>Content</td>
                        <td id='empty' style='height:30px'></td>
                    </tr>
                </table></body>");

            var filled = LayoutTestHelper.FindById(root, "filled")!;
            var empty = LayoutTestHelper.FindById(root, "empty")!;

            _output.WriteLine($"filled w={filled.ContentRect.Width} empty w={empty.ContentRect.Width}");

            Assert.True(empty.ContentRect.Width > 0, "Empty cell still has width");
            Assert.True(empty.ContentRect.Height >= 29,
                $"Empty cell matches row height (got {empty.ContentRect.Height})");
        }

        [Fact]
        public void SingleRowTable_Layout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:50px'>One</td>
                        <td id='c2' style='height:50px'>Two</td>
                        <td id='c3' style='height:50px'>Three</td>
                    </tr>
                </table></body>");

            var table = LayoutTestHelper.FindById(root, "tbl")!;
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell3 = LayoutTestHelper.FindById(root, "c3")!;

            _output.WriteLine($"table h={table.ContentRect.Height}");

            Assert.True(table.ContentRect.Height >= 49,
                $"Single row table height matches cell (got {table.ContentRect.Height})");
            Assert.True(System.Math.Abs(cell1.ContentRect.Y - cell3.ContentRect.Y) < 1,
                "All cells on same Y in single row");
        }

        [Fact]
        public void SingleColumnTable_Layout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                    <tr><td id='r1' style='height:30px'>Row 1</td></tr>
                    <tr><td id='r2' style='height:30px'>Row 2</td></tr>
                    <tr><td id='r3' style='height:30px'>Row 3</td></tr>
                </table></body>");

            var table = LayoutTestHelper.FindById(root, "tbl")!;
            var row1 = LayoutTestHelper.FindById(root, "r1")!;
            var row2 = LayoutTestHelper.FindById(root, "r2")!;
            var row3 = LayoutTestHelper.FindById(root, "r3")!;

            _output.WriteLine($"table w={table.ContentRect.Width} h={table.ContentRect.Height}");

            Assert.True(table.ContentRect.Height >= 89,
                $"Three 30px rows = 90px (got {table.ContentRect.Height})");
            Assert.True(System.Math.Abs(row1.ContentRect.X - row2.ContentRect.X) < 1,
                "All cells on same X in single column");
            Assert.True(System.Math.Abs(row2.ContentRect.X - row3.ContentRect.X) < 1,
                "All cells on same X in single column");
            Assert.True(row1.ContentRect.Width >= 195,
                $"Single column cell fills table width (got {row1.ContentRect.Width})");
        }

        [Fact]
        public void BorderSpacing_AsymmetricValues()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:separate;border-spacing:5px 15px'>
                    <tr>
                        <td id='a' style='height:30px'>A</td>
                        <td id='b' style='height:30px'>B</td>
                    </tr>
                    <tr>
                        <td id='c' style='height:30px'>C</td>
                        <td id='d' style='height:30px'>D</td>
                    </tr>
                </table></body>");

            var table = LayoutTestHelper.FindById(root, "tbl")!;
            var cellA = LayoutTestHelper.FindById(root, "a")!;
            var cellC = LayoutTestHelper.FindById(root, "c")!;

            _output.WriteLine($"table h={table.ContentRect.Height}");

            // Vertical spacing: top(15) + row1(30) + between(15) + row2(30) + bottom(15) = 105
            Assert.True(table.ContentRect.Height >= 104,
                $"Asymmetric border-spacing vertical (got {table.ContentRect.Height})");
            Assert.True(cellC.ContentRect.Y - (cellA.ContentRect.Y + cellA.ContentRect.Height) >= 14,
                "Vertical gap between rows is 15px");
        }

        [Fact]
        public void Colspan_WidthMatchesSpannedColumns()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>C1</td>
                        <td id='c2' style='width:100px;height:30px'>C2</td>
                        <td id='c3' style='width:100px;height:30px'>C3</td>
                    </tr>
                    <tr>
                        <td id='span' colspan='2' style='height:30px'>Spans 1+2</td>
                        <td style='height:30px'>C3</td>
                    </tr>
                </table></body>");

            var col1 = LayoutTestHelper.FindById(root, "c1")!;
            var col2 = LayoutTestHelper.FindById(root, "c2")!;
            var spanning = LayoutTestHelper.FindById(root, "span")!;

            float expectedWidth = col1.ContentRect.Width + col2.ContentRect.Width;
            _output.WriteLine($"span w={spanning.ContentRect.Width} expected ~{expectedWidth}");

            Assert.True(System.Math.Abs(spanning.ContentRect.Width - expectedWidth) < 5,
                $"Colspan width matches spanned columns (got {spanning.ContentRect.Width} expected ~{expectedWidth})");
        }

        [Fact]
        public void Rowspan_HeightMatchesSpannedRows()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='3'>Span</td>
                        <td id='r1' style='height:25px'>R1</td>
                    </tr>
                    <tr><td id='r2' style='height:25px'>R2</td></tr>
                    <tr><td id='r3' style='height:25px'>R3</td></tr>
                </table></body>");

            var spanning = LayoutTestHelper.FindById(root, "span")!;
            var row1 = LayoutTestHelper.FindById(root, "r1")!;
            var row3 = LayoutTestHelper.FindById(root, "r3")!;

            float expectedHeight = (row3.ContentRect.Y + row3.ContentRect.Height) - row1.ContentRect.Y;
            _output.WriteLine($"span h={spanning.ContentRect.Height} expected ~{expectedHeight}");

            Assert.True(System.Math.Abs(spanning.ContentRect.Height - expectedHeight) < 3,
                $"Rowspan height matches spanned rows (got {spanning.ContentRect.Height} expected ~{expectedHeight})");
        }

        [Fact]
        public void FixedLayout_IgnoresContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='table-layout:fixed;width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='a' style='width:50px;height:30px'>A</td>
                        <td id='b' style='width:150px;height:30px'>B</td>
                    </tr>
                    <tr>
                        <td id='a2' style='height:30px'>This is very long content that should not affect column width</td>
                        <td id='b2' style='height:30px'>X</td>
                    </tr>
                </table></body>");

            var colA = LayoutTestHelper.FindById(root, "a")!;
            var colA2 = LayoutTestHelper.FindById(root, "a2")!;

            _output.WriteLine($"a w={colA.ContentRect.Width} a2 w={colA2.ContentRect.Width}");

            Assert.True(System.Math.Abs(colA.ContentRect.Width - colA2.ContentRect.Width) < 2,
                $"Fixed layout: second row column width matches first (a={colA.ContentRect.Width} a2={colA2.ContentRect.Width})");
        }

        [Fact]
        public void MultipleRowsWithDifferentHeights_StackCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr><td id='r1' style='height:20px'>R1</td></tr>
                    <tr><td id='r2' style='height:50px'>R2</td></tr>
                    <tr><td id='r3' style='height:30px'>R3</td></tr>
                </table></body>");

            var row1 = LayoutTestHelper.FindById(root, "r1")!;
            var row2 = LayoutTestHelper.FindById(root, "r2")!;
            var row3 = LayoutTestHelper.FindById(root, "r3")!;

            _output.WriteLine($"r1 Y={row1.ContentRect.Y} h={row1.ContentRect.Height}");
            _output.WriteLine($"r2 Y={row2.ContentRect.Y} h={row2.ContentRect.Height}");
            _output.WriteLine($"r3 Y={row3.ContentRect.Y} h={row3.ContentRect.Height}");

            Assert.True(row2.ContentRect.Y >= row1.ContentRect.Y + 19,
                "Row 2 starts after row 1 (20px)");
            Assert.True(row3.ContentRect.Y >= row2.ContentRect.Y + 49,
                "Row 3 starts after row 2 (50px)");
        }

        [Fact]
        public void TableWidth100Percent_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:350px'>
                    <table id='tbl' style='width:100%;border-collapse:collapse'>
                        <tr>
                            <td id='c1' style='height:30px'>A</td>
                            <td id='c2' style='height:30px'>B</td>
                        </tr>
                    </table>
                </div></body>");

            var table = LayoutTestHelper.FindById(root, "tbl")!;
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell2 = LayoutTestHelper.FindById(root, "c2")!;

            _output.WriteLine($"table w={table.ContentRect.Width} c1 w={cell1.ContentRect.Width} c2 w={cell2.ContentRect.Width}");

            Assert.True(System.Math.Abs(table.ContentRect.Width - 350) < 2,
                $"Table fills 350px container (got {table.ContentRect.Width})");
            float totalCellWidth = cell1.ContentRect.Width + cell2.ContentRect.Width;
            Assert.True(totalCellWidth >= 340,
                $"Cells fill most of table width (got {totalCellWidth})");
        }

        [Fact]
        public void CellPadding_IncreasesSlotSize()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='padded' style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='cell' style='padding:20px;height:30px'>Padded</td>
                    </tr>
                </table>
                <table id='unpadded' style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='cell2' style='padding:0;height:30px'>No padding</td>
                    </tr>
                </table></body>");

            var paddedCell = LayoutTestHelper.FindById(root, "cell")!;
            var unpaddedCell = LayoutTestHelper.FindById(root, "cell2")!;

            _output.WriteLine($"padded border h={paddedCell.BorderRect.Height} unpadded border h={unpaddedCell.BorderRect.Height}");

            Assert.True(paddedCell.BorderRect.Height > unpaddedCell.BorderRect.Height,
                "Padded cell border box taller than unpadded");
            Assert.True(paddedCell.PaddingTop >= 19, $"Top padding applied (got {paddedCell.PaddingTop})");
            Assert.True(paddedCell.PaddingBottom >= 19, $"Bottom padding applied (got {paddedCell.PaddingBottom})");
        }

        [Fact]
        public void MultipleTbody_StackVertically()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                    <thead><tr><td id='head' style='height:25px'>Head</td></tr></thead>
                    <tbody><tr><td id='body1' style='height:25px'>Body1</td></tr></tbody>
                    <tbody><tr><td id='body2' style='height:25px'>Body2</td></tr></tbody>
                    <tfoot><tr><td id='foot' style='height:25px'>Foot</td></tr></tfoot>
                </table></body>");

            var headCell = LayoutTestHelper.FindById(root, "head")!;
            var body1Cell = LayoutTestHelper.FindById(root, "body1")!;
            var body2Cell = LayoutTestHelper.FindById(root, "body2")!;
            var footCell = LayoutTestHelper.FindById(root, "foot")!;

            _output.WriteLine($"head Y={headCell.ContentRect.Y} body1 Y={body1Cell.ContentRect.Y} body2 Y={body2Cell.ContentRect.Y} foot Y={footCell.ContentRect.Y}");

            Assert.True(headCell.ContentRect.Y < body1Cell.ContentRect.Y,
                "thead renders before first tbody");
            Assert.True(body1Cell.ContentRect.Y < body2Cell.ContentRect.Y,
                "First tbody renders before second tbody");
            Assert.True(body2Cell.ContentRect.Y < footCell.ContentRect.Y,
                "Second tbody renders before tfoot");
        }
    }
}
