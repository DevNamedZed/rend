using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS table-layout:fixed conformance tests.
    /// [CSS-TABLES §17.5.2.1] Fixed table layout algorithm.
    /// </summary>
    public class WptTableFixedLayoutTests
    {
        private readonly ITestOutputHelper _output;

        public WptTableFixedLayoutTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-TABLES §17.5.2.1] Fixed layout with explicit pixel widths on first-row cells
        [Fact]
        public void FixedLayout_ExplicitPixelWidths_RespectedFromFirstRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>A</td>
                        <td id='c2' style='width:200px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"table={table!.ContentRect.Width} c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 200) < 2);
        }

        // [CSS-TABLES §17.5.2.1] Content wider than column width is ignored in fixed layout
        [Fact]
        public void FixedLayout_IgnoresContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>
                            <div style='width:500px;height:10px;background:red'></div>
                        </td>
                        <td id='c2' style='width:100px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 100) < 2,
                $"Fixed layout should ignore content: c1 width={cell1.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 100) < 2,
                $"Fixed layout should ignore content: c2 width={cell2.ContentRect.Width}");
        }

        // [CSS-TABLES §17.5.2.1] No explicit widths: equal distribution of table width
        [Fact]
        public void FixedLayout_EqualDistribution_NoExplicitWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:300px;border-collapse:collapse'>
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
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width} c3={cell3!.ContentRect.Width}");
            // Slot=100, UA padding 1px each side, content = 98
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 98) < 2);
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 98) < 2);
            Assert.True(System.Math.Abs(cell3.ContentRect.Width - 98) < 2);
        }

        // [CSS-TABLES §17.5.2.1] Percentage widths resolve against table width
        [Fact]
        public void FixedLayout_PercentageWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:400px;border-collapse:collapse'>
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
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 300) < 2);
        }

        // [CSS-TABLES §17.5.2.1] border-spacing reduces available width for columns
        [Fact]
        public void FixedLayout_WithBorderSpacing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:400px;border-collapse:separate;border-spacing:10px'>
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
            // Available = 400 - 3*10 = 370, each slot = 185, content = 185 - 2 (UA padding) = 183
            float expectedContent = ((400 - 30) / 2f) - 2;
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - expectedContent) < 2);
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - expectedContent) < 2);
        }

        // [CSS-TABLES §17.5.2.1] border-collapse merges borders; column widths include halved borders
        [Fact]
        public void FixedLayout_WithBorderCollapse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:200px;border-collapse:collapse;border:2px solid black'>
                    <tr>
                        <td id='c1' style='border:2px solid black;height:30px'>A</td>
                        <td id='c2' style='border:2px solid black;height:30px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(table);
            _output.WriteLine($"table={table!.ContentRect.Width}x{table.ContentRect.Height}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 4);
        }

        // [CSS-TABLES §17.5.2.1] Explicit table width controls total column layout
        [Fact]
        public void FixedLayout_ExplicitTableWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:600px'>
                    <table id='t' style='table-layout:fixed;width:400px;border-collapse:collapse'>
                        <tr>
                            <td id='c1' style='height:30px'>A</td>
                            <td id='c2' style='height:30px'>B</td>
                        </tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            _output.WriteLine($"table={table!.ContentRect.Width} c1={cell1!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 400) < 2);
            // Slot=200, UA padding 1px each side, content = 198
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 198) < 2);
        }

        // [CSS-TABLES §17.5.2.1] Auto table width with fixed layout: table shrinks to content
        [Fact]
        public void FixedLayout_AutoTableWidth_UsesContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <table id='t' style='table-layout:fixed;border-collapse:collapse'>
                        <tr>
                            <td id='c1' style='width:100px;height:30px'>A</td>
                            <td id='c2' style='width:100px;height:30px'>B</td>
                        </tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(table);
            _output.WriteLine($"table={table!.ContentRect.Width}");
            // With auto width on fixed-layout table, table takes containing block width
            Assert.True(table.ContentRect.Width >= 200,
                $"Fixed layout auto-width table should be at least sum of column widths (got {table.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.1] First row determines widths; subsequent rows are ignored
        [Fact]
        public void FixedLayout_FirstRowDeterminesWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='r1c1' style='width:100px;height:30px'>A</td>
                        <td id='r1c2' style='width:200px;height:30px'>B</td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='width:250px;height:30px'>C</td>
                        <td id='r2c2' style='width:50px;height:30px'>D</td>
                    </tr>
                </table></body>");
            var row1Cell1 = LayoutTestHelper.FindById(root, "r1c1");
            var row2Cell1 = LayoutTestHelper.FindById(root, "r2c1");
            Assert.NotNull(row1Cell1);
            Assert.NotNull(row2Cell1);
            _output.WriteLine($"r1c1={row1Cell1!.ContentRect.Width} r2c1={row2Cell1!.ContentRect.Width}");
            // Both rows should use first-row widths: 100 and 200
            Assert.True(System.Math.Abs(row1Cell1.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(row2Cell1.ContentRect.Width - row1Cell1.ContentRect.Width) < 2,
                $"Second row should match first-row width (r2c1={row2Cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.1] Colspan in fixed layout distributes evenly across spanned columns
        [Fact]
        public void FixedLayout_Colspan_DistributesEvenly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='width:200px;height:30px'>Span</td>
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
            // Colspan=2 with width=200: each spanned column gets 100
            Assert.True(System.Math.Abs(row2Cell1.ContentRect.Width - row2Cell2.ContentRect.Width) < 2,
                $"Spanned columns should be equal (r2c1={row2Cell1.ContentRect.Width}, r2c2={row2Cell2.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.1] Padding on cells is part of the slot width
        [Fact]
        public void FixedLayout_PaddingOnCells()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;padding:10px;height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(cell1);
            _output.WriteLine($"c1 content={cell1!.ContentRect.Width} padL={cell1.PaddingLeft} padR={cell1.PaddingRight}");
            // width:100px is content width; slot = 100 + 10 + 10 = 120
            // Content area should be 100px
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 100) < 2,
                $"Content width should be 100px with padding separate (got {cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.1] Border on cells in separate mode
        [Fact]
        public void FixedLayout_BorderOnCells_Separate()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:300px;border-collapse:separate;border-spacing:0'>
                    <tr>
                        <td id='c1' style='width:100px;border:5px solid black;height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(cell1);
            _output.WriteLine($"c1 content={cell1!.ContentRect.Width} borderL={cell1.BorderLeftWidth} borderR={cell1.BorderRightWidth}");
            // width:100px is content; slot = 100 + 5 + 5 = 110
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 100) < 2,
                $"Content width should be 100px (got {cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.1] Auto vs fixed layout comparison: same widths give same result
        [Fact]
        public void AutoVsFixed_WithExplicitWidths_SameResult()
        {
            var fixedRoot = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>A</td>
                        <td id='c2' style='width:200px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var autoRoot = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:auto;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>A</td>
                        <td id='c2' style='width:200px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var fixedC1 = LayoutTestHelper.FindById(fixedRoot, "c1");
            var autoC1 = LayoutTestHelper.FindById(autoRoot, "c1");
            Assert.NotNull(fixedC1);
            Assert.NotNull(autoC1);
            _output.WriteLine($"fixed c1={fixedC1!.ContentRect.Width} auto c1={autoC1!.ContentRect.Width}");
            // With explicit widths matching table width, both should produce same layout
            Assert.True(System.Math.Abs(fixedC1.ContentRect.Width - autoC1.ContentRect.Width) < 4,
                $"Fixed and auto should match with explicit widths (fixed={fixedC1.ContentRect.Width}, auto={autoC1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.1] Fixed layout ignores min-width on cells
        [Fact]
        public void FixedLayout_IgnoresMinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:50px;min-width:150px;height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(cell1);
            _output.WriteLine($"c1={cell1!.ContentRect.Width}");
            // In fixed layout, min-width on cells is ignored; width:50px should be used
            Assert.True(cell1.ContentRect.Width < 100,
                $"Fixed layout should ignore min-width (got {cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.1] Fixed layout ignores max-width on cells
        [Fact]
        public void FixedLayout_IgnoresMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:200px;max-width:80px;height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(cell1);
            _output.WriteLine($"c1={cell1!.ContentRect.Width}");
            // In fixed layout, max-width on cells is ignored; width:200px should be used
            Assert.True(cell1.ContentRect.Width > 150,
                $"Fixed layout should ignore max-width (got {cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.1] Mixed explicit and auto widths: auto gets remaining space
        [Fact]
        public void FixedLayout_MixedExplicitAndAutoWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>A</td>
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
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width} c3={cell3!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 100) < 2);
            // c2 and c3 share remaining space equally
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - cell3.ContentRect.Width) < 2,
                $"Auto columns should be equal (c2={cell2.ContentRect.Width}, c3={cell3.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.1] Three equal columns, no widths specified, 300px table
        [Fact]
        public void FixedLayout_ThreeEqualColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:300px;border-collapse:collapse'>
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
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width} c3={cell3!.ContentRect.Width}");
            // Slot=100, UA padding 1px each side, content = 98
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 98) < 2);
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 98) < 2);
            Assert.True(System.Math.Abs(cell3.ContentRect.Width - 98) < 2);
        }

        // [CSS-TABLES §17.5.2.1] Table width is honored even when column widths sum exceeds it
        [Fact]
        public void FixedLayout_ColumnWidthsExceedTableWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:150px;height:30px'>A</td>
                        <td id='c2' style='width:150px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"table={table!.ContentRect.Width} c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width}");
            // Cells keep their specified content widths even when sum exceeds table width
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 150) < 2,
                $"Cell1 should keep explicit width (got {cell1.ContentRect.Width})");
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 150) < 2,
                $"Cell2 should keep explicit width (got {cell2.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.1] Multiple rows: all rows have same column widths
        [Fact]
        public void FixedLayout_MultipleRows_UniformColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='r1c1' style='width:150px;height:30px'>Row1A</td>
                        <td id='r1c2' style='width:150px;height:30px'>Row1B</td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='height:30px'>Row2A</td>
                        <td id='r2c2' style='height:30px'>Row2B</td>
                    </tr>
                    <tr>
                        <td id='r3c1' style='height:30px'>Row3A</td>
                        <td id='r3c2' style='height:30px'>Row3B</td>
                    </tr>
                </table></body>");
            var row1Cell1 = LayoutTestHelper.FindById(root, "r1c1");
            var row2Cell1 = LayoutTestHelper.FindById(root, "r2c1");
            var row3Cell1 = LayoutTestHelper.FindById(root, "r3c1");
            Assert.NotNull(row1Cell1);
            Assert.NotNull(row2Cell1);
            Assert.NotNull(row3Cell1);
            _output.WriteLine($"r1c1={row1Cell1!.ContentRect.Width} r2c1={row2Cell1!.ContentRect.Width} r3c1={row3Cell1!.ContentRect.Width}");
            Assert.True(System.Math.Abs(row1Cell1.ContentRect.Width - row2Cell1!.ContentRect.Width) < 2);
            Assert.True(System.Math.Abs(row1Cell1.ContentRect.Width - row3Cell1!.ContentRect.Width) < 2);
        }

        // [CSS-TABLES §17.5.2.1] Fixed layout with percentage table width
        [Fact]
        public void FixedLayout_PercentageTableWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <table id='t' style='table-layout:fixed;width:50%;border-collapse:collapse'>
                        <tr>
                            <td id='c1' style='height:30px'>A</td>
                            <td id='c2' style='height:30px'>B</td>
                        </tr>
                    </table>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            _output.WriteLine($"table={table!.ContentRect.Width} c1={cell1!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 2);
            // Slot=100, UA padding 1px each side, content = 98
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 98) < 2);
        }

        // [CSS-TABLES §17.5.2.1] Fixed layout table height from explicit row heights
        [Fact]
        public void FixedLayout_RowHeightFromExplicitCellHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:50px'>A</td>
                        <td id='c2' style='height:80px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"table.h={table!.ContentRect.Height} c1.h={cell1!.ContentRect.Height} c2.h={cell2!.ContentRect.Height}");
            // Row height should be max of cell heights (80)
            Assert.True(cell1.ContentRect.Height >= 78,
                $"Cell1 should stretch to row height (got {cell1.ContentRect.Height})");
            Assert.True(cell2.ContentRect.Height >= 78,
                $"Cell2 height should be 80 (got {cell2.ContentRect.Height})");
        }

        // [CSS-TABLES §17.5.2.1] Colspan distributes width per-column evenly
        [Fact]
        public void FixedLayout_ColspanThree_DistributesEvenly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='3' style='width:300px;height:30px'>FullSpan</td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='height:30px'>A</td>
                        <td id='r2c2' style='height:30px'>B</td>
                        <td id='r2c3' style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var row2Cell1 = LayoutTestHelper.FindById(root, "r2c1");
            var row2Cell2 = LayoutTestHelper.FindById(root, "r2c2");
            var row2Cell3 = LayoutTestHelper.FindById(root, "r2c3");
            Assert.NotNull(row2Cell1);
            Assert.NotNull(row2Cell2);
            Assert.NotNull(row2Cell3);
            _output.WriteLine($"r2c1={row2Cell1!.ContentRect.Width} r2c2={row2Cell2!.ContentRect.Width} r2c3={row2Cell3!.ContentRect.Width}");
            Assert.True(System.Math.Abs(row2Cell1.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(row2Cell2.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(row2Cell3.ContentRect.Width - 100) < 2);
        }

        // [CSS-TABLES §17.5.2.1] Fixed layout with large border-spacing
        [Fact]
        public void FixedLayout_LargeBorderSpacing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:400px;border-collapse:separate;border-spacing:20px'>
                    <tr>
                        <td id='c1' style='height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                        <td id='c3' style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            // spacing: 4 * 20 = 80, available = 320, each slot = ~106.67, content = 104.67 (minus UA padding 1px each side)
            float expectedSpacing = 4 * 20;
            float expectedContent = ((400 - expectedSpacing) / 3f) - 2;
            _output.WriteLine($"table={table!.ContentRect.Width} c1={cell1!.ContentRect.Width} expectedContent={expectedContent}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - expectedContent) < 2,
                $"Column width should account for spacing (got {cell1.ContentRect.Width}, expected ~{expectedContent})");
        }

        // [CSS-TABLES §17.5.2.1] Fixed layout with one wide and one narrow explicit width
        [Fact]
        public void FixedLayout_AsymmetricExplicitWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:50px;height:30px'>Narrow</td>
                        <td id='c2' style='width:350px;height:30px'>Wide</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 350) < 2);
        }

        // [CSS-TABLES §17.5.2.1] Cell positions: cells are laid out left-to-right with correct X offsets
        [Fact]
        public void FixedLayout_CellPositions_CorrectXOffsets()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>A</td>
                        <td id='c2' style='width:100px;height:30px'>B</td>
                        <td id='c3' style='width:100px;height:30px'>C</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            var cell3 = LayoutTestHelper.FindById(root, "c3");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            Assert.NotNull(cell3);
            _output.WriteLine($"c1.x={cell1!.ContentRect.X} c2.x={cell2!.ContentRect.X} c3.x={cell3!.ContentRect.X}");
            Assert.True(cell2!.ContentRect.X > cell1.ContentRect.X,
                $"c2 should be right of c1 (c1.x={cell1.ContentRect.X}, c2.x={cell2.ContentRect.X})");
            Assert.True(cell3!.ContentRect.X > cell2.ContentRect.X,
                $"c3 should be right of c2 (c2.x={cell2.ContentRect.X}, c3.x={cell3.ContentRect.X})");
        }

        // [CSS-TABLES §17.5.2.1] Fixed layout with border-spacing on cell positions
        [Fact]
        public void FixedLayout_CellPositions_WithBorderSpacing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:400px;border-collapse:separate;border-spacing:10px'>
                    <tr>
                        <td id='c1' style='height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1.x={cell1!.ContentRect.X} c2.x={cell2!.ContentRect.X}");
            // First cell starts after left border-spacing (10px)
            Assert.True(cell1.ContentRect.X >= 9,
                $"First cell should start after border-spacing (c1.x={cell1.ContentRect.X})");
            // Second cell should be offset by first cell width + spacing
            Assert.True(cell2!.ContentRect.X > cell1.ContentRect.X + cell1.ContentRect.Width,
                $"c2 should be after c1 + spacing gap");
        }

        // [CSS-TABLES §17.5.2.1] Fixed layout with mixed percentage and pixel widths
        [Fact]
        public void FixedLayout_MixedPercentageAndPixelWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:400px;border-collapse:collapse'>
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
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 200) < 2,
                $"50% of 400 = 200 (got {cell1.ContentRect.Width})");
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 100) < 2,
                $"Fixed 100px (got {cell2.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.1] Four columns with equal distribution, single row
        [Fact]
        public void FixedLayout_FourEqualColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:400px;border-collapse:collapse'>
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
            // Slot=100, UA padding 1px each side, content = 98
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 98) < 2);
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 98) < 2);
            Assert.True(System.Math.Abs(cell3.ContentRect.Width - 98) < 2);
            Assert.True(System.Math.Abs(cell4.ContentRect.Width - 98) < 2);
        }

        // [CSS-TABLES §17.5.2.1] Colspan=2 in first row with no individual width info
        [Fact]
        public void FixedLayout_ColspanInFirstRow_NoWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px'>Span2</td>
                        <td id='c3' style='height:30px'>C</td>
                        <td id='c4' style='height:30px'>D</td>
                    </tr>
                    <tr>
                        <td id='r2c1' style='height:30px'>A</td>
                        <td id='r2c2' style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                        <td style='height:30px'>D</td>
                    </tr>
                </table></body>");
            var row2Cell1 = LayoutTestHelper.FindById(root, "r2c1");
            var row2Cell2 = LayoutTestHelper.FindById(root, "r2c2");
            Assert.NotNull(row2Cell1);
            Assert.NotNull(row2Cell2);
            _output.WriteLine($"r2c1={row2Cell1!.ContentRect.Width} r2c2={row2Cell2!.ContentRect.Width}");
            // Fixed layout distributes width per COLUMN: 400/4 = 100px each. The row-1 colspan=2
            // cell covers two 100px columns; the row-2 single cells are 100px each, minus UA
            // padding (1px per side) → content = 98. (The earlier "slot/colSpan=50" was wrong.)
            Assert.True(System.Math.Abs(row2Cell1.ContentRect.Width - 98) < 2);
            Assert.True(System.Math.Abs(row2Cell2.ContentRect.Width - 98) < 2);
        }

        // [CSS-TABLES §17.5.2.1] Fixed layout with padding + border-collapse
        [Fact]
        public void FixedLayout_PaddingWithBorderCollapse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;padding:5px;border:1px solid;height:30px'>A</td>
                        <td id='c2' style='padding:5px;border:1px solid;height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(cell1);
            _output.WriteLine($"c1 content={cell1!.ContentRect.Width} pad={cell1.PaddingLeft},{cell1.PaddingRight}");
            // In collapsed mode, borders are halved. Slot = width + padding + border/2
            Assert.True(cell1.ContentRect.Width > 80,
                $"Cell content width should account for padding and halved border (got {cell1.ContentRect.Width})");
        }

        // [CSS-TABLES §17.5.2.1] Single column fixed layout table
        [Fact]
        public void FixedLayout_SingleColumn()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px'>Only</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            _output.WriteLine($"table={table!.ContentRect.Width} c1={cell1!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 2);
            // Slot=200, UA padding 1px each side, content = 198
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 198) < 2);
        }

        // [CSS-TABLES §17.5.2.1] Content overflow is clipped, not expanding cell
        [Fact]
        public void FixedLayout_LongTextDoesNotExpandCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='table-layout:fixed;width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px;overflow:hidden'>
                            This is a very long text that should not expand the cell width in fixed layout mode
                        </td>
                        <td id='c2' style='width:100px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1={cell1!.ContentRect.Width} c2={cell2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 100) < 2,
                $"Long text should not expand fixed cell (got {cell1.ContentRect.Width})");
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 100) < 2,
                $"Adjacent cell should not be affected (got {cell2.ContentRect.Width})");
        }
    }
}
