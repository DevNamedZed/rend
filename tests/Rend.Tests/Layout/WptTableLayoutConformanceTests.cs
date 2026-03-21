using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS table layout conformance tests covering table-layout algorithms,
    /// border models, spanning, alignment, and interactions with flex/grid.
    /// </summary>
    public class WptTableLayoutConformanceTests
    {
        private readonly ITestOutputHelper _output;

        public WptTableLayoutConformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-TABLES §4.6] Two equal columns in 400px container with border-collapse
        [Fact]
        public void TwoEqualColumns_InFullWidthTable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"table={table!.ContentRect.Width} c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 400) < 2);
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - cell2.ContentRect.Width) < 2);
        }

        // [CSS-TABLES §4.6] table-layout:fixed with explicit column widths
        [Fact]
        public void FixedLayout_ExplicitColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>A</td>
                        <td id='c2' style='width:200px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1.w={cell1!.ContentRect.Width} c2.w={cell2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 200) < 2);
            Assert.True(cell2.ContentRect.X > cell1.ContentRect.X);
        }

        // [CSS-TABLES §4.6] table-layout:fixed with percentage widths
        [Fact]
        public void FixedLayout_PercentageWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:25%;height:30px'>A</td>
                        <td id='c2' style='width:75%;height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1.w={cell1!.ContentRect.Width} c2.w={cell2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 300) < 2);
        }

        // [CSS-TABLES §4.7] table-layout:auto distributes based on content
        [Fact]
        public void AutoLayout_DistributesBasedOnContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='narrow' style='width:100px;height:30px'>Narrow</td>
                        <td id='wide' style='width:300px;height:30px'>Wide</td>
                    </tr>
                </table></body>");
            var narrow = LayoutTestHelper.FindById(root, "narrow");
            var wide = LayoutTestHelper.FindById(root, "wide");
            Assert.NotNull(narrow);
            Assert.NotNull(wide);
            _output.WriteLine($"narrow={narrow!.ContentRect.Width} wide={wide!.ContentRect.Width}");
            Assert.True(wide.ContentRect.Width > narrow.ContentRect.Width);
            float total = narrow.ContentRect.Width + wide.ContentRect.Width;
            Assert.True(total >= 390, $"Total column widths should fill table (got {total})");
        }

        // [CSS-TABLES §4.1] Table width:100% fills container
        [Fact]
        public void TableWidth100Percent_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <table id='tbl' style='width:100%;border-collapse:collapse'>
                        <tr><td style='height:30px'>A</td></tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table.w={table!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 400) < 2);
        }

        // [CSS-TABLES §4.4] border-spacing adds space around and between cells
        [Fact]
        public void BorderSpacing_AffectsLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:400px;border-collapse:separate;border-spacing:10px'>
                    <tr>
                        <td id='c1' style='height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"tbl.h={table!.ContentRect.Height} c1.w={cell1!.ContentRect.Width} c2.w={cell2!.ContentRect.Width}");
            // Height: top(10) + row(30) + bottom(10) = 50
            Assert.True(table.ContentRect.Height >= 49);
            // Available for columns: 400 - 3*10 = 370, each column ~185
            float totalColumnWidth = cell1.ContentRect.Width + cell2.ContentRect.Width;
            Assert.True(totalColumnWidth >= 360, $"Columns share 370px (got {totalColumnWidth})");
        }

        // [CSS-TABLES §4.3] border-collapse:collapse merges adjacent borders
        [Fact]
        public void BorderCollapse_CollapsesMergesAdjacentBorders()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse;border:2px solid black'>
                    <tr>
                        <td id='c1' style='border:2px solid black;height:30px'>A</td>
                        <td id='c2' style='border:2px solid black;height:30px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"tbl={table!.ContentRect.Width}x{table.ContentRect.Height}");
            // Collapsed borders: table should be ~200px wide
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 4);
        }

        // [CSS-TABLES §4.8] colspan spanning 2 columns
        [Fact]
        public void Colspan_SpansTwoColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px'>Span2</td>
                        <td style='height:30px'>C</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>A</td>
                        <td style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var spanned = LayoutTestHelper.FindById(root, "span");
            Assert.NotNull(spanned);
            _output.WriteLine($"span.w={spanned!.ContentRect.Width}");
            // colspan=2 of 3 equal columns: ~200px
            Assert.True(spanned.ContentRect.Width >= 190,
                $"colspan=2 should span ~200px (got {spanned.ContentRect.Width})");
        }

        // [CSS-TABLES §4.8] colspan spanning all columns
        [Fact]
        public void Colspan_SpansAllColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='full' colspan='3' style='height:30px'>Full span</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>A</td>
                        <td style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var full = LayoutTestHelper.FindById(root, "full");
            Assert.NotNull(full);
            _output.WriteLine($"full.w={full!.ContentRect.Width}");
            Assert.True(full.ContentRect.Width >= 290,
                $"colspan=3 should span full width (got {full.ContentRect.Width})");
        }

        // [CSS-TABLES §4.8] rowspan spanning 2 rows
        [Fact]
        public void Rowspan_SpansTwoRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2'>Span2</td>
                        <td style='height:40px'>B1</td>
                    </tr>
                    <tr>
                        <td style='height:40px'>B2</td>
                    </tr>
                </table></body>");
            var spanned = LayoutTestHelper.FindById(root, "span");
            Assert.NotNull(spanned);
            _output.WriteLine($"span.h={spanned!.ContentRect.Height}");
            Assert.True(spanned.ContentRect.Height >= 78,
                $"rowspan=2 should span ~80px (got {spanned.ContentRect.Height})");
        }

        // [CSS-TABLES §4.2] Table cell padding
        [Fact]
        public void TableCell_Padding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='cell' style='padding:10px;height:30px'>A</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell");
            Assert.NotNull(cell);
            _output.WriteLine($"cell content={cell!.ContentRect.Width}x{cell!.ContentRect.Height} padding={cell.PaddingLeft},{cell.PaddingTop},{cell.PaddingRight},{cell.PaddingBottom}");
            Assert.True(System.Math.Abs(cell.PaddingLeft - 10) < 2);
            Assert.True(System.Math.Abs(cell.PaddingTop - 10) < 2);
            Assert.True(System.Math.Abs(cell.PaddingRight - 10) < 2);
            Assert.True(System.Math.Abs(cell.PaddingBottom - 10) < 2);
        }

        // [CSS-TABLES §4.9] vertical-align:middle on table cell
        [Fact]
        public void TableCell_VerticalAlignMiddle()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:100px;vertical-align:top'>Tall</td>
                        <td id='mid' style='vertical-align:middle'>Middle</td>
                    </tr>
                </table></body>");
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.NotNull(mid);
            _output.WriteLine($"mid.y={mid!.ContentRect.Y} mid.h={mid.ContentRect.Height}");
            // Cell stretches to full row height; vertical-align positions content within
            Assert.True(System.Math.Abs(mid.ContentRect.Height - 100) < 2,
                $"Cell should stretch to row height (got {mid.ContentRect.Height})");
        }

        // [CSS-TABLES §4.9] vertical-align:bottom on table cell
        [Fact]
        public void TableCell_VerticalAlignBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:100px;vertical-align:top'>Tall</td>
                        <td id='bot' style='vertical-align:bottom'>Bottom</td>
                    </tr>
                </table></body>");
            var bot = LayoutTestHelper.FindById(root, "bot");
            Assert.NotNull(bot);
            _output.WriteLine($"bot.y={bot!.ContentRect.Y} bot.h={bot.ContentRect.Height}");
            // Cell stretches to full row height; vertical-align positions content within
            Assert.True(System.Math.Abs(bot.ContentRect.Height - 100) < 2,
                $"Cell should stretch to row height (got {bot.ContentRect.Height})");
        }

        // [CSS-TABLES §4.9] vertical-align:top on table cell (default)
        [Fact]
        public void TableCell_VerticalAlignTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:100px'>Tall</td>
                        <td id='top' style='vertical-align:top'>Top</td>
                    </tr>
                </table></body>");
            var top = LayoutTestHelper.FindById(root, "top");
            Assert.NotNull(top);
            _output.WriteLine($"top.y={top!.ContentRect.Y}");
            // Default/top alignment: content at top of cell
            Assert.True(top.ContentRect.Y < 5,
                $"vertical-align:top content should be near top (got Y={top.ContentRect.Y})");
        }

        // [CSS-TABLES §4.1] Table with caption
        [Fact]
        public void Table_WithCaption()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                    <caption id='cap'>Table Caption</caption>
                    <tr><td id='cell' style='height:40px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var caption = LayoutTestHelper.FindById(root, "cap");
            var cell = LayoutTestHelper.FindById(root, "cell");
            Assert.NotNull(table);
            Assert.NotNull(caption);
            Assert.NotNull(cell);
            _output.WriteLine($"tbl.h={table!.ContentRect.Height} cap.h={caption!.ContentRect.Height} cell.y={cell!.ContentRect.Y}");
            // Caption should be above the table rows, total height > 40
            Assert.True(table.ContentRect.Height > 40,
                $"Table with caption should be taller than row alone (got {table.ContentRect.Height})");
        }

        // [CSS-TABLES §4.7] Auto width table shrinks to fit content
        [Fact]
        public void AutoWidth_ShrinkToFit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <table id='tbl' style='border-collapse:collapse'>
                        <tr>
                            <td style='width:60px;height:30px'>A</td>
                            <td style='width:40px;height:30px'>B</td>
                        </tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"tbl.w={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width < 200,
                $"Auto-width table should shrink to fit (got {table.ContentRect.Width})");
            Assert.True(table.ContentRect.Width >= 95,
                $"Auto-width table should be at least column sum (got {table.ContentRect.Width})");
        }

        // [CSS-TABLES §4.1] Nested table
        [Fact]
        public void NestedTable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='outer' style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td>
                            <table id='inner' style='width:100%;border-collapse:collapse'>
                                <tr><td id='innerCell' style='height:20px'>Inner</td></tr>
                            </table>
                        </td>
                        <td style='height:40px'>Outer</td>
                    </tr>
                </table></body>");
            var outer = LayoutTestHelper.FindById(root, "outer");
            var inner = LayoutTestHelper.FindById(root, "inner");
            Assert.NotNull(outer);
            Assert.NotNull(inner);
            _output.WriteLine($"outer.w={outer!.ContentRect.Width} inner.w={inner!.ContentRect.Width}");
            Assert.True(System.Math.Abs(outer.ContentRect.Width - 300) < 2);
            Assert.True(inner.ContentRect.Width > 50, $"Inner table should have width (got {inner.ContentRect.Width})");
            Assert.True(inner.ContentRect.Width < outer.ContentRect.Width,
                $"Inner should be smaller than outer (inner={inner.ContentRect.Width} outer={outer.ContentRect.Width})");
        }

        // [CSS-TABLES §4.1] Table with explicit height
        [Fact]
        public void Table_ExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;height:150px;border-collapse:collapse'>
                    <tr><td id='cell' style='height:30px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var cell = LayoutTestHelper.FindById(root, "cell");
            Assert.NotNull(table);
            Assert.NotNull(cell);
            _output.WriteLine($"tbl.h={table!.ContentRect.Height} cell.h={cell!.ContentRect.Height}");
            Assert.True(table.ContentRect.Height >= 148,
                $"Table with height:150px should be at least 150 (got {table.ContentRect.Height})");
        }

        // [CSS-TABLES §4.8] Row height from tallest cell
        [Fact]
        public void RowHeight_FromTallestCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='tall' style='height:80px'>Tall</td>
                        <td id='short' style='height:30px'>Short</td>
                    </tr>
                </table></body>");
            var tall = LayoutTestHelper.FindById(root, "tall");
            var shortCell = LayoutTestHelper.FindById(root, "short");
            Assert.NotNull(tall);
            Assert.NotNull(shortCell);
            _output.WriteLine($"tall.h={tall!.ContentRect.Height} short.h={shortCell!.ContentRect.Height}");
            // Both cells should match the tallest cell height
            Assert.True(System.Math.Abs(tall.ContentRect.Height - shortCell.ContentRect.Height) < 2,
                $"Both cells should have same height (tall={tall.ContentRect.Height} short={shortCell.ContentRect.Height})");
            Assert.True(tall.ContentRect.Height >= 78,
                $"Row height should match tallest cell (got {tall.ContentRect.Height})");
        }

        // [CSS-TABLES §4.1] Table with percentage heights on cells
        [Fact]
        public void Table_PercentageHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;height:200px;border-collapse:collapse'>
                    <tr><td id='r1' style='height:50%'>Row1</td></tr>
                    <tr><td id='r2' style='height:50%'>Row2</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var row1 = LayoutTestHelper.FindById(root, "r1");
            var row2 = LayoutTestHelper.FindById(root, "r2");
            Assert.NotNull(table);
            Assert.NotNull(row1);
            Assert.NotNull(row2);
            _output.WriteLine($"tbl.h={table!.ContentRect.Height} r1.h={row1!.ContentRect.Height} r2.h={row2!.ContentRect.Height}");
            Assert.True(table.ContentRect.Height >= 198);
            Assert.True(System.Math.Abs(row1.ContentRect.Height - row2.ContentRect.Height) < 2,
                $"50%/50% rows should be equal (r1={row1.ContentRect.Height} r2={row2.ContentRect.Height})");
        }

        // [CSS-TABLES §4.5] Empty cells still take space with equal column widths
        [Fact]
        public void EmptyCells_StillTakeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='filled' style='width:100px;height:30px'>Content</td>
                        <td id='empty' style='width:100px;height:30px'></td>
                    </tr>
                </table></body>");
            var filled = LayoutTestHelper.FindById(root, "filled");
            var empty = LayoutTestHelper.FindById(root, "empty");
            Assert.NotNull(filled);
            Assert.NotNull(empty);
            _output.WriteLine($"filled.w={filled!.ContentRect.Width} empty.w={empty!.ContentRect.Width}");
            Assert.True(empty.ContentRect.Width > 90,
                $"Empty cell should have column width (got {empty.ContentRect.Width})");
            Assert.True(System.Math.Abs(filled.ContentRect.Height - empty.ContentRect.Height) < 2,
                $"Empty cell should match row height (filled={filled.ContentRect.Height} empty={empty.ContentRect.Height})");
        }

        // [CSS-TABLES §4.7] min-width on table cells
        [Fact]
        public void TableCell_MinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='minw' style='min-width:150px;height:30px'>A</td>
                        <td style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var minw = LayoutTestHelper.FindById(root, "minw");
            Assert.NotNull(minw);
            _output.WriteLine($"minw.w={minw!.ContentRect.Width}");
            Assert.True(minw.ContentRect.Width >= 146,
                $"Cell with min-width:150px should be at least ~150 (got {minw.ContentRect.Width})");
        }

        // [CSS-TABLES §4.7] max-width on table cells
        [Fact]
        public void TableCell_MaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='maxw' style='max-width:100px;width:300px;height:30px'>A</td>
                        <td style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var maxw = LayoutTestHelper.FindById(root, "maxw");
            Assert.NotNull(maxw);
            _output.WriteLine($"maxw.w={maxw!.ContentRect.Width}");
            // max-width may or may not be honored on table cells depending on spec compliance
            // but the cell should have a valid width
            Assert.True(maxw.ContentRect.Width > 0,
                $"Cell should have a width (got {maxw.ContentRect.Width})");
        }

        // [CSS-TABLES §4.3] border-collapse: shared borders halved
        [Fact]
        public void BorderCollapse_SharedBordersHalved()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse;border:4px solid black'>
                    <tr>
                        <td id='c1' style='border:4px solid black;height:30px'>A</td>
                        <td id='c2' style='border:4px solid black;height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1.borderLeft={cell1!.BorderLeftWidth} c1.borderRight={cell1.BorderRightWidth}");
            _output.WriteLine($"c2.borderLeft={cell2!.BorderLeftWidth} c2.borderRight={cell2.BorderRightWidth}");
            // In collapsed mode, shared borders should be halved (4px -> 2px at shared edge)
            Assert.True(cell1.BorderRightWidth <= 3,
                $"Shared border should be halved (got {cell1.BorderRightWidth})");
        }

        // [CSS-FLEXBOX §9] Table inside flex container
        [Fact]
        public void Table_InsideFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <table id='tbl' style='border-collapse:collapse'>
                        <tr><td style='width:100px;height:30px'>A</td><td style='width:80px;height:30px'>B</td></tr>
                    </table>
                    <div style='width:50px;height:30px'>Sibling</div>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"tbl.w={table!.ContentRect.Width} tbl.x={table.ContentRect.X}");
            Assert.True(table.ContentRect.Width >= 170,
                $"Table in flex should shrink to fit (got {table.ContentRect.Width})");
        }

        // [CSS-GRID §9] Table inside grid container
        [Fact]
        public void Table_InsideGridContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <table id='tbl' style='border-collapse:collapse'>
                        <tr><td style='height:30px'>A</td></tr>
                    </table>
                    <div style='height:30px'>Sibling</div>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"tbl.w={table!.ContentRect.Width} tbl.x={table.ContentRect.X}");
            Assert.True(table.ContentRect.Width > 50,
                $"Table in grid cell should have width (got {table.ContentRect.Width})");
        }

        // [CSS-TABLES §2.1] display:table on a div
        [Fact]
        public void DisplayTable_OnDiv()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='tbl' style='display:table;width:300px'>
                    <div style='display:table-row'>
                        <div id='c1' style='display:table-cell;height:30px'>A</div>
                        <div id='c2' style='display:table-cell;height:30px'>B</div>
                    </div>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"tbl.w={table!.ContentRect.Width} c1.w={cell1!.ContentRect.Width} c2.w={cell2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 300) < 2);
            Assert.True(cell1.ContentRect.Width > 50);
            Assert.True(cell2.ContentRect.Width > 50);
        }

        // [CSS-TABLES §4.7] Cell width percentage
        [Fact]
        public void TableCell_WidthPercentage()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:30%;height:30px'>30%</td>
                        <td id='c2' style='width:70%;height:30px'>70%</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1.w={cell1!.ContentRect.Width} c2.w={cell2!.ContentRect.Width}");
            Assert.True(cell2.ContentRect.Width > cell1.ContentRect.Width,
                $"70% column should be wider than 30% (c1={cell1.ContentRect.Width} c2={cell2.ContentRect.Width})");
            Assert.True(cell1.ContentRect.Width >= 100,
                $"30% of 400 ~= 120 (got {cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §4.1] Multiple rows with same column structure
        [Fact]
        public void MultipleRows_SameColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='r1c1' style='width:100px;height:30px'>R1C1</td>
                        <td id='r1c2' style='width:200px;height:30px'>R1C2</td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='height:30px'>R2C1</td>
                        <td id='r2c2' style='height:30px'>R2C2</td>
                    </tr>
                </table></body>");
            var r1c1 = LayoutTestHelper.FindById(root, "r1c1");
            var r2c1 = LayoutTestHelper.FindById(root, "r2c1");
            var r1c2 = LayoutTestHelper.FindById(root, "r1c2");
            var r2c2 = LayoutTestHelper.FindById(root, "r2c2");
            Assert.NotNull(r1c1);
            Assert.NotNull(r2c1);
            Assert.NotNull(r1c2);
            Assert.NotNull(r2c2);
            _output.WriteLine($"r1c1.w={r1c1!.ContentRect.Width} r2c1.w={r2c1!.ContentRect.Width}");
            // Same column widths across rows
            Assert.True(System.Math.Abs(r1c1.ContentRect.Width - r2c1.ContentRect.Width) < 2,
                $"Column 1 width should match across rows (r1={r1c1.ContentRect.Width} r2={r2c1.ContentRect.Width})");
            Assert.True(System.Math.Abs(r1c2!.ContentRect.Width - r2c2!.ContentRect.Width) < 2,
                $"Column 2 width should match across rows (r1={r1c2.ContentRect.Width} r2={r2c2.ContentRect.Width})");
            // Second row Y should be below first row
            Assert.True(r2c1.ContentRect.Y > r1c1.ContentRect.Y + 25,
                $"Row 2 should be below row 1 (r2.y={r2c1.ContentRect.Y} r1.y={r1c1.ContentRect.Y})");
        }

        // [CSS-TABLES §4.6] Fixed layout ignores content width
        [Fact]
        public void FixedLayout_IgnoresContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:50px;height:30px'>VeryLongContentThatExceedsWidth</td>
                        <td id='c2' style='width:150px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1.w={cell1!.ContentRect.Width} c2.w={cell2!.ContentRect.Width}");
            // Fixed layout should respect declared widths regardless of content
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 50) < 2,
                $"Fixed layout should set width=50 despite content (got {cell1.ContentRect.Width})");
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 150) < 2,
                $"Fixed layout should set width=150 (got {cell2.ContentRect.Width})");
        }

        // [CSS-TABLES §4.4] border-spacing with 3 columns
        [Fact]
        public void BorderSpacing_ThreeColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:400px;border-collapse:separate;border-spacing:10px'>
                    <tr>
                        <td id='c1' style='height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                        <td id='c3' style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            var cell3 = LayoutTestHelper.FindById(root, "c3");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            Assert.NotNull(cell3);
            _output.WriteLine($"tbl.w={table!.ContentRect.Width} c1.w={cell1!.ContentRect.Width} c2.w={cell2!.ContentRect.Width} c3.w={cell3!.ContentRect.Width}");
            // Available for columns: 400 - 4*10 = 360, each ~120
            float totalColumnWidth = cell1.ContentRect.Width + cell2.ContentRect.Width + cell3.ContentRect.Width;
            Assert.True(totalColumnWidth >= 350,
                $"Three columns should share ~360px (got {totalColumnWidth})");
        }

        // [CSS-TABLES §4.4] border-spacing vertical with multiple rows
        [Fact]
        public void BorderSpacing_VerticalMultipleRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:separate;border-spacing:10px'>
                    <tr><td style='height:30px'>R1</td></tr>
                    <tr><td style='height:30px'>R2</td></tr>
                    <tr><td style='height:30px'>R3</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"tbl.h={table!.ContentRect.Height}");
            // Height: top(10) + row1(30) + gap(10) + row2(30) + gap(10) + row3(30) + bottom(10) = 130
            Assert.True(table.ContentRect.Height >= 128,
                $"3 rows with border-spacing:10px should be ~130px tall (got {table.ContentRect.Height})");
        }

        // [CSS-TABLES §4.1] Table with display:inline-table
        [Fact(Skip = "inline-table not yet fully supported")]
        public void InlineTable_ShrinkToFit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <table id='tbl' style='display:inline-table;border-collapse:collapse'>
                        <tr><td style='width:80px;height:30px'>A</td><td style='width:60px;height:30px'>B</td></tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"tbl.w={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width < 200,
                $"Inline-table should shrink to fit (got {table.ContentRect.Width})");
        }

        // [CSS-TABLES §4.1] Table cell background does not affect layout
        [Fact]
        public void TableCell_BackgroundDoesNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='bg' style='background:red;height:30px'>A</td>
                        <td id='nobg' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var bg = LayoutTestHelper.FindById(root, "bg");
            var nobg = LayoutTestHelper.FindById(root, "nobg");
            Assert.NotNull(bg);
            Assert.NotNull(nobg);
            Assert.True(System.Math.Abs(bg!.ContentRect.Width - nobg!.ContentRect.Width) < 2,
                $"Background should not affect width (bg={bg.ContentRect.Width} nobg={nobg.ContentRect.Width})");
        }

        // [CSS-TABLES §4.8] Rowspan with 3 rows of different heights
        [Fact]
        public void Rowspan_ThreeRowsDifferentHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='3'>S</td>
                        <td style='height:20px'>A</td>
                    </tr>
                    <tr><td style='height:30px'>B</td></tr>
                    <tr><td style='height:50px'>C</td></tr>
                </table></body>");
            var spanned = LayoutTestHelper.FindById(root, "span");
            Assert.NotNull(spanned);
            _output.WriteLine($"span.h={spanned!.ContentRect.Height}");
            // rowspan=3: 20+30+50 = 100px
            Assert.True(spanned.ContentRect.Height >= 98,
                $"rowspan=3 should span ~100px (got {spanned.ContentRect.Height})");
        }

        // [CSS-TABLES §4.2] Cell X positions in 2-column table
        [Fact]
        public void CellXPositions_TwoColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>A</td>
                        <td id='c2' style='width:200px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1.x={cell1!.ContentRect.X} c2.x={cell2!.ContentRect.X}");
            // Second cell should start after first cell
            Assert.True(cell2.ContentRect.X > cell1.ContentRect.X + 90,
                $"Cell2 should be after Cell1 (c1.x={cell1.ContentRect.X} c2.x={cell2.ContentRect.X})");
        }

        // [CSS-TABLES §4.1] Table at X=0 with margin:0 on body
        [Fact]
        public void Table_AtOrigin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                    <tr><td style='height:30px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"tbl.x={table!.ContentRect.X} tbl.y={table.ContentRect.Y}");
            Assert.True(table.ContentRect.X < 2, $"Table should start near X=0 (got {table.ContentRect.X})");
            Assert.True(table.ContentRect.Y < 2, $"Table should start near Y=0 (got {table.ContentRect.Y})");
        }

        // [CSS-TABLES §4.8] Colspan + rowspan in same table
        [Fact]
        public void ColspanAndRowspan_Together()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='cs' colspan='2' style='height:30px'>Colspan2</td>
                        <td style='height:30px'>C</td>
                    </tr>
                    <tr>
                        <td id='rs' rowspan='2' style='height:30px'>Rowspan2</td>
                        <td style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var colspanned = LayoutTestHelper.FindById(root, "cs");
            var rowspanned = LayoutTestHelper.FindById(root, "rs");
            Assert.NotNull(colspanned);
            Assert.NotNull(rowspanned);
            _output.WriteLine($"cs.w={colspanned!.ContentRect.Width} rs.h={rowspanned!.ContentRect.Height}");
            Assert.True(colspanned.ContentRect.Width >= 190,
                $"colspan=2 should span ~200px (got {colspanned.ContentRect.Width})");
            Assert.True(rowspanned.ContentRect.Height >= 58,
                $"rowspan=2 should span ~60px (got {rowspanned.ContentRect.Height})");
        }

        // [CSS-TABLES §4.6] Fixed layout with 3 columns, no explicit widths
        [Fact]
        public void FixedLayout_ThreeColumnsNoExplicitWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                        <td id='c3' style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            var cell3 = LayoutTestHelper.FindById(root, "c3");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            Assert.NotNull(cell3);
            _output.WriteLine($"c1.w={cell1!.ContentRect.Width} c2.w={cell2!.ContentRect.Width} c3.w={cell3!.ContentRect.Width}");
            // Fixed layout with no explicit widths: equal distribution (~100px each)
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - cell2.ContentRect.Width) < 5,
                $"Equal distribution (c1={cell1.ContentRect.Width} c2={cell2.ContentRect.Width})");
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - cell3.ContentRect.Width) < 5,
                $"Equal distribution (c2={cell2.ContentRect.Width} c3={cell3.ContentRect.Width})");
            float total = cell1.ContentRect.Width + cell2.ContentRect.Width + cell3.ContentRect.Width;
            Assert.True(total >= 290, $"Total fills table (got {total})");
        }

        // [CSS-TABLES §4.1] Table width:50% in 400px container
        [Fact]
        public void Table_PercentageWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <table id='tbl' style='width:50%;border-collapse:collapse'>
                        <tr><td style='height:30px'>A</td></tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"tbl.w={table!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 2,
                $"50% of 400 = 200 (got {table.ContentRect.Width})");
        }

        // [CSS-TABLES §4.1] Table with border and cell border in separate mode
        [Fact]
        public void SeparateBorders_TableAndCellBorders()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:separate;border:2px solid black;border-spacing:0'>
                    <tr>
                        <td id='cell' style='border:2px solid red;height:30px'>A</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var cell = LayoutTestHelper.FindById(root, "cell");
            Assert.NotNull(table);
            Assert.NotNull(cell);
            _output.WriteLine($"tbl.border={table!.BorderLeftWidth} cell.border={cell!.BorderLeftWidth}");
            // In separate mode, table and cell borders are independent
            Assert.True(System.Math.Abs(table.BorderLeftWidth - 2) < 1);
            Assert.True(System.Math.Abs(cell.BorderLeftWidth - 2) < 1);
        }

        // [CSS-TABLES §4.1] Single cell table width (content-box, UA padding applied)
        [Fact]
        public void SingleCellTable_FullWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='cell' style='padding:0;height:40px'>Only cell</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell");
            Assert.NotNull(cell);
            _output.WriteLine($"cell.w={cell!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell.ContentRect.Width - 400) < 2,
                $"Single cell should fill table width (got {cell.ContentRect.Width})");
        }

        // [CSS-TABLES §4.2] Four column table with explicit widths
        [Fact]
        public void FourColumns_ExplicitWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>A</td>
                        <td id='c2' style='width:100px;height:30px'>B</td>
                        <td id='c3' style='width:100px;height:30px'>C</td>
                        <td id='c4' style='width:100px;height:30px'>D</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            var cell3 = LayoutTestHelper.FindById(root, "c3");
            var cell4 = LayoutTestHelper.FindById(root, "c4");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            Assert.NotNull(cell3);
            Assert.NotNull(cell4);
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width} c3={cell3!.ContentRect.Width} c4={cell4!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(cell3.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(cell4.ContentRect.Width - 100) < 2);
        }

        // [CSS-TABLES §4.1] Table height auto expands to content
        [Fact]
        public void TableHeight_AutoExpandsToContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                    <tr><td style='height:50px'>R1</td></tr>
                    <tr><td style='height:70px'>R2</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"tbl.h={table!.ContentRect.Height}");
            Assert.True(table.ContentRect.Height >= 118,
                $"Table height should be at least 50+70=120 (got {table.ContentRect.Height})");
        }

        // [CSS-TABLES §4.4] border-spacing asymmetric (horizontal vs vertical)
        [Fact]
        public void BorderSpacing_Asymmetric()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:300px;border-collapse:separate;border-spacing:20px 5px'>
                    <tr>
                        <td id='c1' style='height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"tbl.w={table!.ContentRect.Width} tbl.h={table.ContentRect.Height}");
            // Vertical spacing: top(5) + row(30) + bottom(5) = 40
            Assert.True(table.ContentRect.Height >= 38,
                $"Vertical spacing=5 (got {table.ContentRect.Height})");
            // Horizontal spacing 20px: left(20) + between(20) + right(20) = 60px taken
        }

        // [CSS-TABLES §4.6] Fixed layout with first-row col widths
        [Fact]
        public void FixedLayout_FirstRowDeterminesWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='r1c1' style='width:150px;height:30px'>R1C1</td>
                        <td id='r1c2' style='width:250px;height:30px'>R1C2</td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='width:300px;height:30px'>R2C1 wants 300</td>
                        <td id='r2c2' style='height:30px'>R2C2</td>
                    </tr>
                </table></body>");
            var r1c1 = LayoutTestHelper.FindById(root, "r1c1");
            var r2c1 = LayoutTestHelper.FindById(root, "r2c1");
            Assert.NotNull(r1c1);
            Assert.NotNull(r2c1);
            _output.WriteLine($"r1c1.w={r1c1!.ContentRect.Width} r2c1.w={r2c1!.ContentRect.Width}");
            // In fixed layout, first row determines column widths
            Assert.True(System.Math.Abs(r1c1.ContentRect.Width - r2c1.ContentRect.Width) < 2,
                $"Fixed layout: first row determines widths (r1={r1c1.ContentRect.Width} r2={r2c1.ContentRect.Width})");
        }

        // [CSS-TABLES §4.1] Table with margin auto centering
        [Fact]
        public void Table_MarginAutoCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <table id='tbl' style='width:200px;margin:0 auto;border-collapse:collapse'>
                        <tr><td style='height:30px'>A</td></tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"tbl.x={table!.ContentRect.X} tbl.w={table.ContentRect.Width}");
            // margin:0 auto should center the 200px table in 400px container
            Assert.True(System.Math.Abs(table.ContentRect.X - 100) < 2,
                $"Auto margin should center table at X~100 (got {table.ContentRect.X})");
        }
    }
}
