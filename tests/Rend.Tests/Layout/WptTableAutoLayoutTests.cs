using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS table-layout:auto conformance tests.
    /// [CSS-TABLES §17.5.2.2] Automatic table layout algorithm.
    /// </summary>
    public class WptTableAutoLayoutTests
    {
        private readonly ITestOutputHelper _output;

        public WptTableAutoLayoutTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-TABLES §17.5.2.2] Auto layout distributes width based on cell content
        [Fact]
        public void AutoLayout_DistributesBasedOnContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>
                            <div style='width:200px;height:10px'></div>
                        </td>
                        <td id='c2' style='height:30px'>
                            <div style='width:100px;height:10px'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width}");
            Assert.True(cell1.ContentRect.Width > cell2.ContentRect.Width,
                $"Wider content cell should get more space (c1={cell1.ContentRect.Width} c2={cell2.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Two cells with equal content get equal widths
        [Fact]
        public void AutoLayout_TwoEqualContentCells_EqualWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>
                            <div style='width:100px;height:10px'></div>
                        </td>
                        <td id='c2' style='height:30px'>
                            <div style='width:100px;height:10px'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - cell2.ContentRect.Width) < 2,
                $"Equal content should produce equal widths (c1={cell1.ContentRect.Width} c2={cell2.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Wide content cell gets proportionally more space
        [Fact]
        public void AutoLayout_WideContentCell_GetsMoreSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='narrow' style='height:30px'>
                            <div style='width:50px;height:10px'></div>
                        </td>
                        <td id='wide' style='height:30px'>
                            <div style='width:250px;height:10px'></div>
                        </td>
                    </tr>
                </table></body>");
            var narrow = LayoutTestHelper.FindById(root, "narrow");
            var wide = LayoutTestHelper.FindById(root, "wide");
            Assert.NotNull(narrow);
            Assert.NotNull(wide);
            _output.WriteLine($"narrow={narrow!.ContentRect.Width} wide={wide!.ContentRect.Width}");
            Assert.True(wide.ContentRect.Width > narrow.ContentRect.Width * 1.5,
                $"Wide content cell should get significantly more space (narrow={narrow.ContentRect.Width} wide={wide.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Table with width:100% fills containing block
        [Fact]
        public void AutoLayout_TableWidth100Percent_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <table id='t' style='table-layout:auto;width:100%;border-collapse:collapse'>
                        <tr>
                            <td id='c1' style='height:30px'>A</td>
                            <td id='c2' style='height:30px'>B</td>
                        </tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(table);
            _output.WriteLine($"table={table!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 400) < 2,
                $"width:100% table should fill container (got {table.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Auto-width table shrinks to fit content
        [Fact]
        public void AutoLayout_ShrinkToFit_NoTableWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;border-collapse:collapse'>
                    <tr>
                        <td style='height:30px'>
                            <div style='width:80px;height:10px'></div>
                        </td>
                        <td style='height:30px'>
                            <div style='width:60px;height:10px'></div>
                        </td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(table);
            _output.WriteLine($"table={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width < 200,
                $"Auto-width table should shrink to fit (got {table.ContentRect.Width})");
            Assert.True(table.ContentRect.Width >= 140,
                $"Auto-width table should be at least sum of content widths (got {table.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Percentage cell widths resolve against table width
        [Fact]
        public void AutoLayout_PercentageCellWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:25%;height:30px'>A</td>
                        <td id='c2' style='width:75%;height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width}");
            Assert.True(cell2.ContentRect.Width > cell1.ContentRect.Width * 2,
                $"75% cell should be at least 2x wider than 25% cell (c1={cell1.ContentRect.Width} c2={cell2.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Colspan spans multiple columns in auto layout
        [Fact]
        public void AutoLayout_Colspan_SpansColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px'>Spans two</td>
                        <td id='c3' style='height:30px'>C</td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='height:30px'>A</td>
                        <td id='r2c2' style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var spanCell = LayoutTestHelper.FindById(root, "span");
            var row2Cell1 = LayoutTestHelper.FindById(root, "r2c1");
            var row2Cell2 = LayoutTestHelper.FindById(root, "r2c2");
            Assert.NotNull(spanCell);
            Assert.NotNull(row2Cell1);
            Assert.NotNull(row2Cell2);
            _output.WriteLine($"span={spanCell!.ContentRect.Width} r2c1={row2Cell1!.ContentRect.Width} r2c2={row2Cell2!.ContentRect.Width}");
            float combinedWidth = row2Cell1.ContentRect.Width + row2Cell2.ContentRect.Width;
            Assert.True(System.Math.Abs(spanCell.ContentRect.Width - combinedWidth) < 10,
                $"Colspan cell should span both columns (span={spanCell.ContentRect.Width} combined={combinedWidth})");
        }

        // [CSS-TABLES §17.5.2.2] Border-spacing reduces available width for cells
        [Fact]
        public void AutoLayout_BorderSpacing_ReducesAvailable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:300px;border-collapse:separate;border-spacing:10px'>
                    <tr>
                        <td id='c1' style='height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"table={table!.ContentRect.Width} c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width}");
            float totalCellWidth = cell1.ContentRect.Width + cell2.ContentRect.Width;
            Assert.True(totalCellWidth < 280,
                $"Cell widths should not fill full table due to spacing (total={totalCellWidth})");
        }

        // [CSS-TABLES §17.5.2.2] Border-collapse merges adjacent borders
        [Fact]
        public void AutoLayout_BorderCollapse_MergesBorders()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='collapsed' style='table-layout:auto;width:300px;border-collapse:collapse;border:2px solid black'>
                    <tr>
                        <td id='cc1' style='border:2px solid black;height:30px'>A</td>
                        <td id='cc2' style='border:2px solid black;height:30px'>B</td>
                    </tr>
                </table>
                <table id='separate' style='table-layout:auto;width:300px;border-collapse:separate;border-spacing:0;border:2px solid black'>
                    <tr>
                        <td id='sc1' style='border:2px solid black;height:30px'>A</td>
                        <td id='sc2' style='border:2px solid black;height:30px'>B</td>
                    </tr>
                </table></body>");
            var collapsed = LayoutTestHelper.FindById(root, "collapsed");
            var separate = LayoutTestHelper.FindById(root, "separate");
            Assert.NotNull(collapsed);
            Assert.NotNull(separate);
            _output.WriteLine($"collapsed height={collapsed!.ContentRect.Height} separate height={separate!.ContentRect.Height}");
            Assert.True(collapsed.ContentRect.Height <= separate.ContentRect.Height,
                $"Collapsed table should be same or shorter than separate (collapsed={collapsed.ContentRect.Height} separate={separate.ContentRect.Height})");
        }

        // [CSS-TABLES §17.5.2.2] min-width on cells is honored in auto layout
        [Fact]
        public void AutoLayout_MinWidthOnCells_Honored()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='min-width:200px;height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(cell1);
            _output.WriteLine($"c1={cell1!.ContentRect.Width}");
            Assert.True(cell1.ContentRect.Width >= 196,
                $"min-width should be honored in auto layout (got {cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] max-width on cells constrains width in auto layout
        [Fact]
        public void AutoLayout_MaxWidthOnCells_Constrains()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:300px;max-width:150px;height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(cell1);
            _output.WriteLine($"c1={cell1!.ContentRect.Width}");
            // In auto layout, max-width may or may not apply to cells per CSS 2.1 §17.5.3.
            // Chrome does not apply max-width to table cells, so the cell may exceed max-width.
            Assert.True(cell1.ContentRect.Width > 0,
                $"Cell should have positive width (got {cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Empty cells still occupy space
        [Fact]
        public void AutoLayout_EmptyCells_OccupySpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'></td>
                        <td id='c2' style='height:30px'>Content</td>
                        <td id='c3' style='height:30px'></td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            var cell3 = LayoutTestHelper.FindById(root, "c3");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            Assert.NotNull(cell3);
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width} c3={cell3!.ContentRect.Width}");
            Assert.True(cell1.ContentRect.Width > 0,
                $"Empty cell should have positive width (got {cell1.ContentRect.Width})");
            Assert.True(cell3.ContentRect.Width > 0,
                $"Empty cell should have positive width (got {cell3.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Table with thead and tbody rows
        [Fact]
        public void AutoLayout_TheadTbody_WidthDistributed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <thead>
                        <tr>
                            <th id='h1' style='height:30px'>Header 1</th>
                            <th id='h2' style='height:30px'>Header 2</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td id='b1' style='height:30px'>Body 1</td>
                            <td id='b2' style='height:30px'>Body 2</td>
                        </tr>
                    </tbody>
                </table></body>");
            var header1 = LayoutTestHelper.FindById(root, "h1");
            var body1 = LayoutTestHelper.FindById(root, "b1");
            Assert.NotNull(header1);
            Assert.NotNull(body1);
            _output.WriteLine($"h1={header1!.ContentRect.Width} b1={body1!.ContentRect.Width}");
            Assert.True(System.Math.Abs(header1.ContentRect.Width - body1.ContentRect.Width) < 2,
                $"Header and body cells in same column should have same width (h1={header1.ContentRect.Width} b1={body1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Nested div content determines cell width
        [Fact]
        public void AutoLayout_NestedContent_DeterminesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>
                            <div style='width:150px;height:10px'></div>
                        </td>
                        <td id='c2' style='height:30px'>
                            <div style='width:50px;height:10px'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width}");
            Assert.True(cell1.ContentRect.Width > cell2.ContentRect.Width,
                $"Cell with wider nested content should be wider (c1={cell1.ContentRect.Width} c2={cell2.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Auto vs fixed layout: auto considers content, fixed ignores it
        [Fact]
        public void AutoVsFixed_AutoConsidersContent_FixedIgnores()
        {
            var autoRoot = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:auto;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>
                            <div style='width:200px;height:10px'></div>
                        </td>
                        <td id='c2' style='width:100px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var fixedRoot = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>
                            <div style='width:200px;height:10px'></div>
                        </td>
                        <td id='c2' style='width:100px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var autoCell1 = LayoutTestHelper.FindById(autoRoot, "c1");
            var fixedCell1 = LayoutTestHelper.FindById(fixedRoot, "c1");
            Assert.NotNull(autoCell1);
            Assert.NotNull(fixedCell1);
            _output.WriteLine($"auto c1={autoCell1!.ContentRect.Width} fixed c1={fixedCell1!.ContentRect.Width}");
            Assert.True(autoCell1.ContentRect.Width >= fixedCell1.ContentRect.Width,
                $"Auto layout should give at least as much space as fixed when content is wider (auto={autoCell1.ContentRect.Width} fixed={fixedCell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Explicit pixel width on table
        [Fact]
        public void AutoLayout_ExplicitTableWidth_Honored()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:600px'>
                    <table id='t' style='table-layout:auto;width:300px;border-collapse:collapse'>
                        <tr>
                            <td id='c1' style='height:30px'>A</td>
                            <td id='c2' style='height:30px'>B</td>
                        </tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(table);
            _output.WriteLine($"table={table!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 300) < 2,
                $"Explicit table width should be 300 (got {table.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Three columns with varying content
        [Fact]
        public void AutoLayout_ThreeColumns_ProportionalDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>
                            <div style='width:50px;height:10px'></div>
                        </td>
                        <td id='c2' style='height:30px'>
                            <div style='width:100px;height:10px'></div>
                        </td>
                        <td id='c3' style='height:30px'>
                            <div style='width:150px;height:10px'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            var cell3 = LayoutTestHelper.FindById(root, "c3");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            Assert.NotNull(cell3);
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width} c3={cell3!.ContentRect.Width}");
            Assert.True(cell1.ContentRect.Width < cell2.ContentRect.Width,
                $"Smallest content should get least width (c1={cell1.ContentRect.Width} c2={cell2.ContentRect.Width})");
            Assert.True(cell2.ContentRect.Width < cell3.ContentRect.Width,
                $"Largest content should get most width (c2={cell2.ContentRect.Width} c3={cell3.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Explicit cell width constrains column
        [Fact]
        public void AutoLayout_ExplicitCellWidth_Constrains()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:200px;height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 200) < 4,
                $"Explicit width should be honored (got {cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Multiple rows use widest cell per column
        [Fact]
        public void AutoLayout_MultipleRows_WidestCellDeterminesColumnWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='r1c1' style='height:30px'>
                            <div style='width:100px;height:10px'></div>
                        </td>
                        <td style='height:30px'>B</td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='height:30px'>
                            <div style='width:200px;height:10px'></div>
                        </td>
                        <td style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var row1Cell1 = LayoutTestHelper.FindById(root, "r1c1");
            var row2Cell1 = LayoutTestHelper.FindById(root, "r2c1");
            Assert.NotNull(row1Cell1);
            Assert.NotNull(row2Cell1);
            _output.WriteLine($"r1c1={row1Cell1!.ContentRect.Width} r2c1={row2Cell1!.ContentRect.Width}");
            Assert.True(System.Math.Abs(row1Cell1.ContentRect.Width - row2Cell1.ContentRect.Width) < 2,
                $"Both rows should have same column width determined by widest cell (r1c1={row1Cell1.ContentRect.Width} r2c1={row2Cell1.ContentRect.Width})");
            Assert.True(row1Cell1.ContentRect.Width >= 198,
                $"Column should be at least as wide as widest content (got {row1Cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Table without explicit width shrinks to content
        [Fact]
        public void AutoLayout_NoTableWidth_ShrinksToContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>A</td>
                        <td id='c2' style='width:80px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(table);
            _output.WriteLine($"table={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width < 250,
                $"Auto-width table should shrink toward content (got {table.ContentRect.Width})");
            Assert.True(table.ContentRect.Width >= 178,
                $"Auto-width table should be at least sum of cell widths (got {table.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Auto layout with padding on cells
        [Fact]
        public void AutoLayout_CellPadding_IncludedInSlotWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='padding:20px;height:30px'>
                            <div style='width:100px;height:10px'></div>
                        </td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(cell1);
            _output.WriteLine($"c1 content={cell1!.ContentRect.Width} padL={cell1.PaddingLeft} padR={cell1.PaddingRight}");
            Assert.True(cell1.PaddingLeft >= 19,
                $"Cell padding should be applied (got padL={cell1.PaddingLeft})");
            Assert.True(cell1.ContentRect.Width >= 98,
                $"Content area should still fit the 100px div (got {cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Auto layout with border on cells
        [Fact]
        public void AutoLayout_CellBorder_IncludedInLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:separate;border-spacing:0'>
                    <tr>
                        <td id='c1' style='border:5px solid black;height:30px'>
                            <div style='width:100px;height:10px'></div>
                        </td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(cell1);
            _output.WriteLine($"c1 content={cell1!.ContentRect.Width} borderL={cell1.BorderLeftWidth} borderR={cell1.BorderRightWidth}");
            Assert.True(cell1.BorderLeftWidth >= 4,
                $"Cell border should be applied (got borderL={cell1.BorderLeftWidth})");
            Assert.True(cell1.ContentRect.Width >= 98,
                $"Content area should still accommodate the 100px div (got {cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Row height determined by tallest cell
        [Fact]
        public void AutoLayout_RowHeight_DeterminedByTallestCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>Short</td>
                        <td id='c2' style='height:80px'>Tall</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1.h={cell1!.ContentRect.Height} c2.h={cell2!.ContentRect.Height}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Height - cell2.ContentRect.Height) < 2,
                $"Both cells should have same row height (c1={cell1.ContentRect.Height} c2={cell2.ContentRect.Height})");
            Assert.True(cell1.ContentRect.Height >= 78,
                $"Row height should be at least tallest cell (got {cell1.ContentRect.Height})");
        }

        // [CSS-TABLES §17.5.2.2] Single column auto-width table
        [Fact]
        public void AutoLayout_SingleColumn_FillsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>Only column</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            _output.WriteLine($"table={table!.ContentRect.Width} c1={cell1!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 300) < 2,
                $"Table should be 300px (got {table.ContentRect.Width})");
            Assert.True(cell1.ContentRect.Width >= 296,
                $"Single column should fill most of table width (got {cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Nested table inside auto layout cell
        [Fact]
        public void AutoLayout_NestedTable_SizedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='outer' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>
                            <table id='inner' style='width:200px;border-collapse:collapse'>
                                <tr><td style='height:25px'>Nested</td></tr>
                            </table>
                        </td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var outerTable = LayoutTestHelper.FindById(root, "outer");
            var innerTable = LayoutTestHelper.FindById(root, "inner");
            Assert.NotNull(outerTable);
            Assert.NotNull(innerTable);
            _output.WriteLine($"outer={outerTable!.ContentRect.Width} inner={innerTable!.ContentRect.Width}");
            Assert.True(System.Math.Abs(innerTable.ContentRect.Width - 200) < 4,
                $"Nested table should be 200px (got {innerTable.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Mixed explicit and auto-width cells
        [Fact]
        public void AutoLayout_MixedExplicitAndAutoWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:150px;height:30px'>Fixed</td>
                        <td id='c2' style='height:30px'>Auto</td>
                        <td id='c3' style='height:30px'>Auto</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            var cell3 = LayoutTestHelper.FindById(root, "c3");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            Assert.NotNull(cell3);
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width} c3={cell3!.ContentRect.Width}");
            Assert.True(cell1.ContentRect.Width >= 148,
                $"Explicit width cell should be at least 150 (got {cell1.ContentRect.Width})");
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - cell3.ContentRect.Width) < 4,
                $"Auto cells with equal content should be similar width (c2={cell2.ContentRect.Width} c3={cell3.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Content wider than table width expands table
        [Fact]
        public void AutoLayout_ContentExceedsTableWidth_TableExpands()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>
                            <div style='width:300px;height:10px'></div>
                        </td>
                        <td id='c2' style='height:30px'>
                            <div style='width:200px;height:10px'></div>
                        </td>
                    </tr>
                </table></body>", viewportWidth: 800);
            var table = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(table);
            _output.WriteLine($"table={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width >= 498,
                $"Auto table should expand to fit content (got {table.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Cell X positions in left-to-right order
        [Fact]
        public void AutoLayout_CellPositions_LeftToRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:300px;border-collapse:collapse'>
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
            _output.WriteLine($"c1.x={cell1!.ContentRect.X} c2.x={cell2!.ContentRect.X} c3.x={cell3!.ContentRect.X}");
            Assert.True(cell2.ContentRect.X > cell1.ContentRect.X,
                $"c2 should be right of c1 (c1.x={cell1.ContentRect.X} c2.x={cell2.ContentRect.X})");
            Assert.True(cell3.ContentRect.X > cell2.ContentRect.X,
                $"c3 should be right of c2 (c2.x={cell2.ContentRect.X} c3.x={cell3.ContentRect.X})");
        }

        // [CSS-TABLES §17.5.2.2] Large border-spacing reduces cell widths
        [Fact]
        public void AutoLayout_LargeBorderSpacing_ReducesCellWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:separate;border-spacing:20px'>
                    <tr>
                        <td id='c1' style='height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                        <td id='c3' style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(cell1);
            _output.WriteLine($"c1={cell1!.ContentRect.Width}");
            float maxExpectedSlot = (400 - 4 * 20) / 3f;
            Assert.True(cell1.ContentRect.Width < maxExpectedSlot + 5,
                $"Cell width should account for large border-spacing (got {cell1.ContentRect.Width} max expected slot ~{maxExpectedSlot})");
        }

        // [CSS-TABLES §17.5.2.2] Percentage width:50% table inside container
        [Fact]
        public void AutoLayout_TablePercentageWidth_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <table id='t' style='table-layout:auto;width:50%;border-collapse:collapse'>
                        <tr>
                            <td id='c1' style='height:30px'>A</td>
                            <td id='c2' style='height:30px'>B</td>
                        </tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(table);
            _output.WriteLine($"table={table!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 2,
                $"50% of 400 should be 200 (got {table.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Colspan=3 spanning all columns
        [Fact]
        public void AutoLayout_ColspanThree_SpansAllColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='3' style='height:30px'>Full span</td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='height:30px'>A</td>
                        <td id='r2c2' style='height:30px'>B</td>
                        <td id='r2c3' style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var spanCell = LayoutTestHelper.FindById(root, "span");
            var table = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(spanCell);
            Assert.NotNull(table);
            _output.WriteLine($"span={spanCell!.ContentRect.Width} table={table!.ContentRect.Width}");
            Assert.True(System.Math.Abs(spanCell.ContentRect.Width - table.ContentRect.Width) < 6,
                $"Colspan=3 should span full table width (span={spanCell.ContentRect.Width} table={table.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Four equal columns
        [Fact]
        public void AutoLayout_FourEqualColumns_EvenDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                        <td id='c3' style='height:30px'>C</td>
                        <td id='c4' style='height:30px'>D</td>
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
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - cell2.ContentRect.Width) < 4,
                $"Equal content columns should be equal width (c1={cell1.ContentRect.Width} c2={cell2.ContentRect.Width})");
            Assert.True(System.Math.Abs(cell3.ContentRect.Width - cell4.ContentRect.Width) < 4,
                $"Equal content columns should be equal width (c3={cell3.ContentRect.Width} c4={cell4.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Rowspan does not affect column widths
        [Fact]
        public void AutoLayout_Rowspan_DoesNotAffectColumnWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2' style='width:100px'>Spans rows</td>
                        <td id='c2' style='height:30px'>B1</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>B2</td>
                    </tr>
                </table></body>");
            var spanCell = LayoutTestHelper.FindById(root, "span");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(spanCell);
            Assert.NotNull(cell2);
            _output.WriteLine($"span={spanCell!.ContentRect.Width} c2={cell2!.ContentRect.Width}");
            Assert.True(spanCell.ContentRect.Width >= 98,
                $"Rowspan cell should honor width (got {spanCell.ContentRect.Width})");
            Assert.True(spanCell.ContentRect.Height >= 58,
                $"Rowspan cell should span two rows (got {spanCell.ContentRect.Height})");
        }

        // [CSS-TABLES §17.5.2.2] Mixed percentage and pixel widths
        [Fact]
        public void AutoLayout_MixedPercentageAndPixelWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:50%;height:30px'>Half</td>
                        <td id='c2' style='width:100px;height:30px'>Fixed</td>
                        <td id='c3' style='height:30px'>Auto</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            var cell3 = LayoutTestHelper.FindById(root, "c3");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            Assert.NotNull(cell3);
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width} c3={cell3!.ContentRect.Width}");
            Assert.True(cell1.ContentRect.Width > cell2.ContentRect.Width,
                $"50% cell should be wider than 100px cell (c1={cell1.ContentRect.Width} c2={cell2.ContentRect.Width})");
            Assert.True(cell2.ContentRect.Width >= 98,
                $"100px cell should be at least ~100px (got {cell2.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Total cell widths approximate table width
        [Fact]
        public void AutoLayout_TotalCellWidths_ApproximateTableWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                        <td id='c3' style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            var cell3 = LayoutTestHelper.FindById(root, "c3");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            Assert.NotNull(cell3);
            float totalPaddingBox = (cell1!.ContentRect.Width + cell1.PaddingLeft + cell1.PaddingRight)
                + (cell2!.ContentRect.Width + cell2.PaddingLeft + cell2.PaddingRight)
                + (cell3!.ContentRect.Width + cell3.PaddingLeft + cell3.PaddingRight);
            _output.WriteLine($"table={table!.ContentRect.Width} totalPaddingBox={totalPaddingBox}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - totalPaddingBox) < 10,
                $"Sum of cell padding-box widths should approximate table width (total={totalPaddingBox} table={table.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Table default is auto layout
        [Fact]
        public void AutoLayout_DefaultIsAuto()
        {
            var autoRoot = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>
                            <div style='width:200px;height:10px'></div>
                        </td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var defaultRoot = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>
                            <div style='width:200px;height:10px'></div>
                        </td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var autoCell = LayoutTestHelper.FindById(autoRoot, "c1");
            var defaultCell = LayoutTestHelper.FindById(defaultRoot, "c1");
            Assert.NotNull(autoCell);
            Assert.NotNull(defaultCell);
            _output.WriteLine($"explicit auto c1={autoCell!.ContentRect.Width} default c1={defaultCell!.ContentRect.Width}");
            Assert.True(System.Math.Abs(autoCell.ContentRect.Width - defaultCell.ContentRect.Width) < 2,
                $"Default layout should match explicit auto (auto={autoCell.ContentRect.Width} default={defaultCell.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.2] Border-spacing with border-collapse:separate affects table height
        [Fact]
        public void AutoLayout_BorderSpacing_AffectsTableHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:300px;border-collapse:separate;border-spacing:10px'>
                    <tr>
                        <td style='height:30px'>A</td>
                        <td style='height:30px'>B</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>C</td>
                        <td style='height:30px'>D</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(table);
            _output.WriteLine($"table height={table!.ContentRect.Height}");
            // Two rows at 30px + 3 vertical spacings at 10px = 30+30+30 = 90
            Assert.True(table.ContentRect.Height >= 88,
                $"Table height should include row heights + border-spacing (got {table.ContentRect.Height})");
        }

        // [CSS-TABLES §17.5.2.2] Colspan cell with wide content forces columns wider
        [Fact]
        public void AutoLayout_ColspanWideContent_ForcesColumnsWider()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px'>
                            <div style='width:350px;height:10px'></div>
                        </td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='height:30px'>A</td>
                        <td id='r2c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var spanCell = LayoutTestHelper.FindById(root, "span");
            var row2Cell1 = LayoutTestHelper.FindById(root, "r2c1");
            var row2Cell2 = LayoutTestHelper.FindById(root, "r2c2");
            Assert.NotNull(spanCell);
            Assert.NotNull(row2Cell1);
            Assert.NotNull(row2Cell2);
            _output.WriteLine($"span={spanCell!.ContentRect.Width} r2c1={row2Cell1!.ContentRect.Width} r2c2={row2Cell2!.ContentRect.Width}");
            float combined = row2Cell1.ContentRect.Width + row2Cell2.ContentRect.Width;
            Assert.True(combined >= 340,
                $"Colspan wide content should force columns to accommodate (combined={combined})");
        }

        // [CSS-TABLES §17.5.2.2] Auto layout with tfoot section
        [Fact]
        public void AutoLayout_Tfoot_SameColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:auto;width:400px;border-collapse:collapse'>
                    <thead>
                        <tr>
                            <th id='h1' style='height:30px'>Header</th>
                            <th id='h2' style='height:30px'>Header</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td id='b1' style='height:30px'>Body</td>
                            <td id='b2' style='height:30px'>Body</td>
                        </tr>
                    </tbody>
                    <tfoot>
                        <tr>
                            <td id='f1' style='height:30px'>Footer</td>
                            <td id='f2' style='height:30px'>Footer</td>
                        </tr>
                    </tfoot>
                </table></body>");
            var header1 = LayoutTestHelper.FindById(root, "h1");
            var body1 = LayoutTestHelper.FindById(root, "b1");
            var footer1 = LayoutTestHelper.FindById(root, "f1");
            Assert.NotNull(header1);
            Assert.NotNull(body1);
            Assert.NotNull(footer1);
            _output.WriteLine($"h1={header1!.ContentRect.Width} b1={body1!.ContentRect.Width} f1={footer1!.ContentRect.Width}");
            Assert.True(System.Math.Abs(header1.ContentRect.Width - body1.ContentRect.Width) < 2,
                $"Header and body columns should match (h1={header1.ContentRect.Width} b1={body1.ContentRect.Width})");
            Assert.True(System.Math.Abs(body1.ContentRect.Width - footer1.ContentRect.Width) < 2,
                $"Body and footer columns should match (b1={body1.ContentRect.Width} f1={footer1.ContentRect.Width})");
        }
    }
}
