using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTableWidthResolutionTests
    {
        private readonly ITestOutputHelper _output;

        public WptTableWidthResolutionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AutoWidth_ShrinksToFitCellContent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:500px'>
                    <table id='tbl' style='border-collapse:collapse'>
                        <tr>
                            <td style='width:80px;height:20px'>A</td>
                            <td style='width:60px;height:20px'>B</td>
                        </tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width < 500,
                $"Auto-width table should shrink below container (got {table.ContentRect.Width})");
            Assert.True(table.ContentRect.Width >= 140,
                $"Auto-width table should be at least sum of cell widths (got {table.ContentRect.Width})");
        }

        [Fact]
        public void Width100Percent_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <table id='tbl' style='width:100%;border-collapse:collapse'>
                        <tr><td style='height:20px'>A</td></tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 400) < 2,
                $"width:100% should fill container (got {table.ContentRect.Width})");
        }

        [Fact]
        public void ExplicitPixelWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:250px;border-collapse:collapse'>
                    <tr><td style='height:20px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 250) < 2,
                $"Explicit 250px width (got {table.ContentRect.Width})");
        }

        [Fact]
        public void PercentageWidth_ResolvesAgainstContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <table id='tbl' style='width:50%;border-collapse:collapse'>
                        <tr><td style='height:20px'>A</td></tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 150) < 2,
                $"50% of 300px = 150px (got {table.ContentRect.Width})");
        }

        [Fact]
        public void TableInContainer_InheritsContainerConstraint()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <table id='tbl' style='width:100%;border-collapse:collapse'>
                        <tr>
                            <td style='height:20px'>A</td>
                            <td style='height:20px'>B</td>
                        </tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 2,
                $"Table should fill 200px container (got {table.ContentRect.Width})");
        }

        [Fact]
        public void MinWidth_OnExplicitWidthTable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <table id='tbl' style='width:100px;min-width:300px;border-collapse:collapse'>
                        <tr><td style='height:20px'>A</td></tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width >= 298,
                $"min-width:300px should override width:100px (got {table.ContentRect.Width})");
        }

        [Fact]
        public void MaxWidth_ClampsTableWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <table id='tbl' style='width:100%;max-width:200px;border-collapse:collapse'>
                        <tr><td style='height:20px'>A</td></tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width <= 202,
                $"max-width:200px should clamp (got {table.ContentRect.Width})");
        }

        [Fact]
        public void BorderSpacing_IncludedInTableWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='border-collapse:separate;border-spacing:10px'>
                    <tr>
                        <td style='width:80px;height:20px'>A</td>
                        <td style='width:80px;height:20px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            // border-spacing: left(10) + cell(80) + between(10) + cell(80) + right(10) = 190
            Assert.True(table.ContentRect.Width >= 188,
                $"border-spacing should add to table width (got {table.ContentRect.Width})");
        }

        [Fact]
        public void BorderCollapse_NoExtraSpacing()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='border-collapse:collapse;width:200px'>
                    <tr>
                        <td style='border:1px solid black;height:20px'>A</td>
                        <td style='border:1px solid black;height:20px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 2,
                $"Collapsed borders, table width = 200px (got {table.ContentRect.Width})");
        }

        [Fact]
        public void CellPadding_AffectsContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='cell' style='padding:10px;height:20px'>A</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var cell = LayoutTestHelper.FindById(root, "cell");
            Assert.NotNull(table);
            Assert.NotNull(cell);
            _output.WriteLine($"table width={table!.ContentRect.Width}, cell content={cell!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 2,
                $"Table width stays at 200px (got {table.ContentRect.Width})");
            Assert.True(cell.ContentRect.Width < 200,
                $"Cell content width should be less than table due to padding (got {cell.ContentRect.Width})");
        }

        [Fact]
        public void BorderBox_IncludesBorderAndPaddingInWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:200px;box-sizing:border-box;border:5px solid black;padding:10px;border-collapse:separate'>
                    <tr><td style='height:20px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table border-rect width={table!.BorderRect.Width}, content={table.ContentRect.Width}");
            Assert.True(table.BorderRect.Width <= 202,
                $"border-box: border rect should be ~200px (got {table.BorderRect.Width})");
            Assert.True(table.ContentRect.Width < 200,
                $"border-box: content width should be less than 200px (got {table.ContentRect.Width})");
        }

        [Fact]
        public void MarginAuto_CentersTable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <table id='tbl' style='width:200px;margin:0 auto;border-collapse:collapse'>
                        <tr><td style='height:20px'>A</td></tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table x={table!.ContentRect.X}, width={table.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 2,
                $"Table width should be 200px (got {table.ContentRect.Width})");
            float expectedX = 100; // (400 - 200) / 2
            Assert.True(System.Math.Abs(table.ContentRect.X - expectedX) < 2,
                $"margin:auto should center table at x~100 (got {table.ContentRect.X})");
        }

        [Fact]
        public void TableInFlex_BecomesFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <table id='tbl' style='border-collapse:collapse'>
                        <tr><td style='width:100px;height:20px'>A</td></tr>
                    </table>
                    <div style='flex:1;height:20px'></div>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width < 400,
                $"Table as flex item should not fill container (got {table.ContentRect.Width})");
        }

        [Fact]
        public void TableInGrid_FillsGridCell()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px 200px;width:400px'>
                    <table id='tbl' style='border-collapse:collapse'>
                        <tr><td style='height:20px'>A</td></tr>
                    </table>
                    <div style='height:20px'></div>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width >= 198,
                $"Table in grid cell should fill 200px track (got {table.ContentRect.Width})");
        }

        [Fact]
        public void CaptionWithTable_TableHasPositiveWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:300px;border-collapse:collapse'>
                    <caption id='cap'>Table caption</caption>
                    <tr><td style='height:20px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var caption = LayoutTestHelper.FindById(root, "cap");
            Assert.NotNull(table);
            Assert.NotNull(caption);
            _output.WriteLine($"table width={table!.ContentRect.Width}, caption width={caption!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 300) < 2,
                $"Table with caption should be 300px (got {table.ContentRect.Width})");
            Assert.True(caption.ContentRect.Width > 0,
                $"Caption should have positive width (got {caption.ContentRect.Width})");
        }

        [Fact]
        public void FixedLayout_UsesFirstRowColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='a' style='width:100px;height:20px'>A</td>
                        <td id='b' style='width:200px;height:20px'>B</td>
                    </tr>
                    <tr>
                        <td style='width:250px;height:20px'>Ignored</td>
                        <td style='height:20px'>Ignored</td>
                    </tr>
                </table></body>");
            var cellA = LayoutTestHelper.FindById(root, "a");
            var cellB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(cellA);
            Assert.NotNull(cellB);
            _output.WriteLine($"a={cellA!.ContentRect.Width}, b={cellB!.ContentRect.Width}");
            Assert.True(cellA.ContentRect.Width < cellB.ContentRect.Width,
                $"Fixed layout: first row widths apply, a < b (a={cellA.ContentRect.Width}, b={cellB.ContentRect.Width})");
        }

        [Fact]
        public void AutoLayout_ConsidersAllRowsContent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='border-collapse:collapse'>
                    <tr>
                        <td id='r1c1' style='width:60px;height:20px'>A</td>
                        <td style='height:20px'>B</td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='width:120px;height:20px'>Wide</td>
                        <td style='height:20px'>B</td>
                    </tr>
                </table></body>");
            var cellR1 = LayoutTestHelper.FindById(root, "r1c1");
            var cellR2 = LayoutTestHelper.FindById(root, "r2c1");
            Assert.NotNull(cellR1);
            Assert.NotNull(cellR2);
            _output.WriteLine($"r1c1={cellR1!.ContentRect.Width}, r2c1={cellR2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cellR1.ContentRect.Width - cellR2.ContentRect.Width) < 2,
                $"Auto layout: both rows same column width (r1={cellR1.ContentRect.Width}, r2={cellR2.ContentRect.Width})");
            Assert.True(cellR1.ContentRect.Width >= 118,
                $"Column uses widest cell (120px) (got {cellR1.ContentRect.Width})");
        }

        [Fact]
        public void ColspanCell_SpansMultipleColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:20px'>Spanning</td>
                        <td style='height:20px'>C</td>
                    </tr>
                    <tr>
                        <td style='height:20px'>A</td>
                        <td style='height:20px'>B</td>
                        <td style='height:20px'>C</td>
                    </tr>
                </table></body>");
            var spanning = LayoutTestHelper.FindById(root, "span");
            Assert.NotNull(spanning);
            _output.WriteLine($"colspan=2 width={spanning!.ContentRect.Width}");
            Assert.True(spanning.ContentRect.Width >= 198,
                $"colspan=2 should span ~200px of 300px table (got {spanning.ContentRect.Width})");
        }

        [Fact]
        public void WideContent_ExpandsAutoWidthTable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <table id='narrow' style='border-collapse:collapse'>
                        <tr><td style='width:50px;height:20px'>S</td></tr>
                    </table>
                    <table id='wide' style='border-collapse:collapse'>
                        <tr><td style='width:300px;height:20px'>Wide content here</td></tr>
                    </table>
                </div></body>");
            var narrow = LayoutTestHelper.FindById(root, "narrow");
            var wide = LayoutTestHelper.FindById(root, "wide");
            Assert.NotNull(narrow);
            Assert.NotNull(wide);
            _output.WriteLine($"narrow={narrow!.ContentRect.Width}, wide={wide!.ContentRect.Width}");
            Assert.True(wide.ContentRect.Width > narrow.ContentRect.Width,
                $"Wide content expands table (narrow={narrow.ContentRect.Width}, wide={wide.ContentRect.Width})");
            Assert.True(wide.ContentRect.Width >= 298,
                $"Wide table should be at least 300px (got {wide.ContentRect.Width})");
        }

        [Fact]
        public void NarrowContent_ShrinksAutoWidthTable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <table id='tbl' style='border-collapse:collapse'>
                        <tr><td style='width:30px;height:20px'>X</td></tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width < 100,
                $"Narrow content should shrink table (got {table.ContentRect.Width})");
        }

        [Fact]
        public void EmptyTable_HasMinimalWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='border-collapse:collapse'>
                    <tr><td></td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"empty table width={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width < 50,
                $"Empty table should have minimal width (got {table.ContentRect.Width})");
        }

        [Fact]
        public void ExplicitWidth_SmallerThanContent_HonorsExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:50px;border-collapse:collapse'>
                    <tr>
                        <td style='width:100px;height:20px'>A</td>
                        <td style='width:100px;height:20px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width > 0,
                $"Table should have positive width (got {table.ContentRect.Width})");
            Assert.True(table.ContentRect.Width <= 202,
                $"Table should honor explicit small width (got {table.ContentRect.Width})");
        }

        [Fact]
        public void MinWidth_LargerThanExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:100px;min-width:250px;border-collapse:collapse'>
                    <tr><td style='height:20px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width >= 248,
                $"min-width should override smaller width (got {table.ContentRect.Width})");
        }

        [Fact]
        public void MaxWidth_SmallerThanExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:400px;max-width:150px;border-collapse:collapse'>
                    <tr><td style='height:20px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width <= 152,
                $"max-width should override larger width (got {table.ContentRect.Width})");
        }

        [Fact]
        public void FixedLayout_DistributesEvenlyWithNoColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='a' style='height:20px'>A</td>
                        <td id='b' style='height:20px'>B</td>
                        <td id='c' style='height:20px'>C</td>
                    </tr>
                </table></body>");
            var cellA = LayoutTestHelper.FindById(root, "a");
            var cellB = LayoutTestHelper.FindById(root, "b");
            var cellC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(cellA);
            Assert.NotNull(cellB);
            Assert.NotNull(cellC);
            _output.WriteLine($"a={cellA!.ContentRect.Width}, b={cellB!.ContentRect.Width}, c={cellC!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cellA.ContentRect.Width - cellB.ContentRect.Width) < 5,
                $"Fixed layout equal distribution (a={cellA.ContentRect.Width}, b={cellB.ContentRect.Width})");
            Assert.True(System.Math.Abs(cellB.ContentRect.Width - cellC.ContentRect.Width) < 5,
                $"Fixed layout equal distribution (b={cellB.ContentRect.Width}, c={cellC.ContentRect.Width})");
        }

        [Fact]
        public void BorderSpacing_LargeSpacing_AffectsAutoWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='border-collapse:separate;border-spacing:20px'>
                    <tr>
                        <td style='width:60px;height:20px'>A</td>
                        <td style='width:60px;height:20px'>B</td>
                        <td style='width:60px;height:20px'>C</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            // 4 spacing gaps(20*4=80) + 3 cells(60*3=180) = 260
            Assert.True(table.ContentRect.Width >= 258,
                $"Large border-spacing adds to width (got {table.ContentRect.Width})");
        }

        [Fact]
        public void PercentageWidth_75Percent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <table id='tbl' style='width:75%;border-collapse:collapse'>
                        <tr><td style='height:20px'>A</td></tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 300) < 2,
                $"75% of 400px = 300px (got {table.ContentRect.Width})");
        }

        [Fact]
        public void TableInFlex_WithExplicitWidth_RespectsWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <table id='tbl' style='width:200px;border-collapse:collapse'>
                        <tr><td style='height:20px'>A</td></tr>
                    </table>
                    <div style='flex:1;height:20px'></div>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 4,
                $"Table with explicit width in flex should be ~200px (got {table.ContentRect.Width})");
        }

        [Fact]
        public void BorderCollapse_WithBorders_SharedBorderWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse;border:2px solid black'>
                    <tr>
                        <td id='cell' style='border:2px solid black;height:20px'>A</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table border-rect={table!.BorderRect.Width}, content={table.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 4,
                $"Collapsed border table width ~200px (got {table.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_MultipleColumns_SumsMinWidths()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='border-collapse:collapse'>
                    <tr>
                        <td style='width:50px;height:20px'>A</td>
                        <td style='width:75px;height:20px'>B</td>
                        <td style='width:100px;height:20px'>C</td>
                        <td style='width:25px;height:20px'>D</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width >= 248,
                $"Auto width sums columns: 50+75+100+25=250 (got {table.ContentRect.Width})");
        }

        [Fact]
        public void TableInGrid_StretchesWithAlignDefault()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <table id='tbl' style='border-collapse:collapse'>
                        <tr><td style='height:20px'>A</td></tr>
                    </table>
                    <div style='height:20px'></div>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width >= 198,
                $"Table in 1fr grid column should stretch to ~200px (got {table.ContentRect.Width})");
        }

        [Fact]
        public void ExplicitWidth_LargerThanContainer_Overflows()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <table id='tbl' style='width:400px;border-collapse:collapse'>
                        <tr><td style='height:20px'>A</td></tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table width={table!.ContentRect.Width}");
            Assert.True(table.ContentRect.Width >= 398,
                $"Explicit width overflows container (got {table.ContentRect.Width})");
        }
    }
}
