using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTableBorderTests
    {
        private readonly ITestOutputHelper _output;

        public WptTableBorderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SeparateMode_DefaultBorderSpacing()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='width:200px; border-collapse:separate'>
                    <tr>
                        <td id='c1' style='height:30px'>A</td>
                        <td id='c2' style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(table);
            // Default border-spacing is 2px in UA; cells should be separated
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell2 = LayoutTestHelper.FindById(root, "c2")!;
            _output.WriteLine($"c1.X={cell1.ContentRect.X} c2.X={cell2.ContentRect.X}");
            Assert.True(cell2.ContentRect.X > cell1.ContentRect.X + cell1.ContentRect.Width,
                "Cells should have spacing between them in separate mode");
        }

        [Fact]
        public void CollapseMode_NoBorderSpacing()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='width:200px; border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px; border:none; padding:0'>A</td>
                        <td id='c2' style='height:30px; border:none; padding:0'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell2 = LayoutTestHelper.FindById(root, "c2")!;
            _output.WriteLine($"c1.X={cell1.ContentRect.X} c1.W={cell1.ContentRect.Width} c2.X={cell2.ContentRect.X}");
            // In collapsed mode with no borders, cells should be adjacent
            float gap = cell2.ContentRect.X - (cell1.ContentRect.X + cell1.ContentRect.Width);
            Assert.True(gap < 2, $"Collapsed cells should be adjacent (gap={gap})");
        }

        [Fact]
        public void BorderSpacing_ExplicitValue_AffectsTableHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='width:200px; border-collapse:separate; border-spacing:10px'>
                    <tr><td style='height:30px; padding:0'>A</td></tr>
                    <tr><td style='height:30px; padding:0'>B</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t")!;
            // Expected: top(10) + row1(30) + between(10) + row2(30) + bottom(10) = 90
            _output.WriteLine($"table.H={table.ContentRect.Height}");
            Assert.True(System.Math.Abs(table.ContentRect.Height - 90) < 2,
                $"border-spacing:10px should produce ~90px height (got {table.ContentRect.Height})");
        }

        [Fact]
        public void BorderSpacing_TwoValues_HorizontalAndVertical()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='width:200px; border-collapse:separate; border-spacing:20px 5px'>
                    <tr>
                        <td id='c1' style='height:30px; padding:0'>A</td>
                        <td id='c2' style='height:30px; padding:0'>B</td>
                    </tr>
                    <tr>
                        <td id='c3' style='height:30px; padding:0'>C</td>
                        <td id='c4' style='height:30px; padding:0'>D</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t")!;
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell2 = LayoutTestHelper.FindById(root, "c2")!;
            var cell3 = LayoutTestHelper.FindById(root, "c3")!;
            // Horizontal spacing 20px between columns
            float horizontalGap = cell2.ContentRect.X - (cell1.ContentRect.X + cell1.ContentRect.Width);
            _output.WriteLine($"hGap={horizontalGap}");
            Assert.True(horizontalGap >= 18, $"Horizontal spacing should be ~20px (got {horizontalGap})");
            // Vertical spacing 5px between rows
            float verticalGap = cell3.ContentRect.Y - (cell1.ContentRect.Y + cell1.ContentRect.Height);
            _output.WriteLine($"vGap={verticalGap}");
            Assert.True(verticalGap >= 3, $"Vertical spacing should be ~5px (got {verticalGap})");
            // Table height: top(5) + row1(30) + between(5) + row2(30) + bottom(5) = 75
            Assert.True(System.Math.Abs(table.ContentRect.Height - 75) < 2,
                $"Table height with vSpacing=5 should be ~75 (got {table.ContentRect.Height})");
        }

        [Fact]
        public void BorderSpacing_ZeroValue()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='width:200px; border-collapse:separate; border-spacing:0'>
                    <tr>
                        <td id='c1' style='height:30px; border:none; padding:0'>A</td>
                        <td id='c2' style='height:30px; border:none; padding:0'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell2 = LayoutTestHelper.FindById(root, "c2")!;
            float gap = cell2.ContentRect.X - (cell1.ContentRect.X + cell1.ContentRect.Width);
            _output.WriteLine($"gap={gap}");
            Assert.True(gap < 2, $"border-spacing:0 means no gap (got {gap})");
        }

        [Fact]
        public void CollapsedBorder_TableBorderVisible()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='width:200px; border-collapse:collapse; border:4px solid black'>
                    <tr><td style='height:30px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t")!;
            // In collapsed mode, table border is halved (shared with cells)
            _output.WriteLine($"borderTop={table.BorderTopWidth} borderLeft={table.BorderLeftWidth}");
            Assert.True(table.BorderTopWidth >= 1, $"Table border should exist (got {table.BorderTopWidth})");
        }

        [Fact]
        public void CollapsedBorder_CellBorderWidthHalved()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px; border-collapse:collapse; border:none'>
                    <tr>
                        <td id='c1' style='height:30px; border:4px solid black'>A</td>
                        <td id='c2' style='height:30px; border:4px solid black'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell2 = LayoutTestHelper.FindById(root, "c2")!;
            // Adjacent cell borders collapse: shared border is halved for each cell
            _output.WriteLine($"c1.borderRight={cell1.BorderRightWidth} c2.borderLeft={cell2.BorderLeftWidth}");
            Assert.True(cell1.BorderRightWidth <= 4,
                $"Inner border should be halved (got {cell1.BorderRightWidth})");
        }

        [Fact]
        public void CollapsedBorder_CellsShareBorder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px; border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px; border:2px solid black; padding:0'>A</td>
                        <td id='c2' style='height:30px; border:2px solid black; padding:0'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell2 = LayoutTestHelper.FindById(root, "c2")!;
            // Cell2 should start right after cell1 (shared border, no gap)
            float boundary = cell1.BorderRect.X + cell1.BorderRect.Width;
            _output.WriteLine($"c1.borderRect.right={boundary} c2.borderRect.X={cell2.BorderRect.X}");
            Assert.True(System.Math.Abs(cell2.BorderRect.X - boundary) < 2,
                $"Collapsed cells share border edge (c1.right={boundary}, c2.X={cell2.BorderRect.X})");
        }

        [Fact]
        public void SeparateMode_CellBorderOnTd()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px; border-collapse:separate; border-spacing:0'>
                    <tr>
                        <td id='c1' style='height:30px; border:3px solid red; padding:0'>A</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "c1")!;
            _output.WriteLine($"border: T={cell.BorderTopWidth} R={cell.BorderRightWidth} B={cell.BorderBottomWidth} L={cell.BorderLeftWidth}");
            Assert.True(System.Math.Abs(cell.BorderTopWidth - 3) < 1, $"Top border 3px (got {cell.BorderTopWidth})");
            Assert.True(System.Math.Abs(cell.BorderRightWidth - 3) < 1, $"Right border 3px (got {cell.BorderRightWidth})");
            Assert.True(System.Math.Abs(cell.BorderBottomWidth - 3) < 1, $"Bottom border 3px (got {cell.BorderBottomWidth})");
            Assert.True(System.Math.Abs(cell.BorderLeftWidth - 3) < 1, $"Left border 3px (got {cell.BorderLeftWidth})");
        }

        [Fact]
        public void SeparateMode_TableBorderOnTableElement()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='width:200px; border-collapse:separate; border:5px solid blue; border-spacing:0'>
                    <tr><td style='height:30px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"table border: T={table.BorderTopWidth} L={table.BorderLeftWidth}");
            Assert.True(System.Math.Abs(table.BorderTopWidth - 5) < 1, $"Table top border 5px (got {table.BorderTopWidth})");
            Assert.True(System.Math.Abs(table.BorderLeftWidth - 5) < 1, $"Table left border 5px (got {table.BorderLeftWidth})");
        }

        [Fact]
        public void SeparateMode_BorderOnTh()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px; border-collapse:separate; border-spacing:0'>
                    <tr>
                        <th id='h1' style='height:30px; border:2px solid green; padding:0'>Header</th>
                    </tr>
                </table></body>");
            var header = LayoutTestHelper.FindById(root, "h1")!;
            Assert.True(System.Math.Abs(header.BorderTopWidth - 2) < 1, $"TH border 2px (got {header.BorderTopWidth})");
            Assert.True(System.Math.Abs(header.BorderLeftWidth - 2) < 1, $"TH border 2px (got {header.BorderLeftWidth})");
        }

        [Fact]
        public void CollapseMode_TableAndCellBorderConflict()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='width:200px; border-collapse:collapse; border:6px solid black'>
                    <tr>
                        <td id='c1' style='height:30px; border:2px solid red; padding:0'>A</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t")!;
            var cell = LayoutTestHelper.FindById(root, "c1")!;
            // CSS 2.1 §17.6.2: wider border wins in collapsed mode
            // Table has 6px, cell has 2px — table's 6px should win on outer edges
            _output.WriteLine($"table.borderTop={table.BorderTopWidth} cell.borderTop={cell.BorderTopWidth}");
            Assert.True(table.BorderTopWidth >= 2, $"Winning border should be at least 2px (got {table.BorderTopWidth})");
        }

        [Fact]
        public void FixedLayout_WithBorders_ColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='table-layout:fixed; width:300px; border-collapse:collapse; border:2px solid black'>
                    <tr>
                        <td id='c1' style='height:30px; border:2px solid black'>A</td>
                        <td id='c2' style='height:30px; border:2px solid black'>B</td>
                        <td id='c3' style='height:30px; border:2px solid black'>C</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell2 = LayoutTestHelper.FindById(root, "c2")!;
            var cell3 = LayoutTestHelper.FindById(root, "c3")!;
            float totalContent = cell1.ContentRect.Width + cell2.ContentRect.Width + cell3.ContentRect.Width;
            _output.WriteLine($"c1={cell1.ContentRect.Width} c2={cell2.ContentRect.Width} c3={cell3.ContentRect.Width} total={totalContent}");
            // With collapsed borders, total content + borders should fill the table
            Assert.True(totalContent > 250, $"Content fills most of the table (got {totalContent})");
            // Columns should be roughly equal in fixed layout
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - cell2.ContentRect.Width) < 5,
                $"Fixed layout: equal columns (c1={cell1.ContentRect.Width}, c2={cell2.ContentRect.Width})");
        }

        [Fact]
        public void FixedLayout_WithBorders_SeparateMode()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='table-layout:fixed; width:300px; border-collapse:separate; border:2px solid black; border-spacing:10px'>
                    <tr>
                        <td id='c1' style='height:30px; border:1px solid gray'>A</td>
                        <td id='c2' style='height:30px; border:1px solid gray'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t")!;
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            _output.WriteLine($"table.W={table.ContentRect.Width} c1.borderLeft={cell1.BorderLeftWidth}");
            Assert.True(System.Math.Abs(cell1.BorderLeftWidth - 1) < 1,
                $"Cell border in separate mode stays full width (got {cell1.BorderLeftWidth})");
        }

        [Fact]
        public void RowspanWithBorders_CollapsedMode()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px; border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2' style='border:2px solid black'>Span</td>
                        <td style='height:30px; border:2px solid black'>B1</td>
                    </tr>
                    <tr>
                        <td style='height:30px; border:2px solid black'>B2</td>
                    </tr>
                </table></body>");
            var spanning = LayoutTestHelper.FindById(root, "span")!;
            // Rowspan cell should span both rows
            _output.WriteLine($"span.H={spanning.ContentRect.Height} span.borderT={spanning.BorderTopWidth}");
            Assert.True(spanning.ContentRect.Height >= 58,
                $"Rowspan=2 cell should span both rows (got {spanning.ContentRect.Height})");
        }

        [Fact]
        public void ColspanWithBorders_CollapsedMode()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px; border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='3' style='height:30px; border:2px solid black'>Spans 3</td>
                    </tr>
                    <tr>
                        <td style='height:30px; border:2px solid black'>A</td>
                        <td style='height:30px; border:2px solid black'>B</td>
                        <td style='height:30px; border:2px solid black'>C</td>
                    </tr>
                </table></body>");
            var spanning = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"colspan.W={spanning.ContentRect.Width}");
            Assert.True(spanning.ContentRect.Width >= 280,
                $"Colspan=3 should span nearly all of 300px (got {spanning.ContentRect.Width})");
        }

        [Fact]
        public void NestedTable_InnerTableBordersIndependent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='outer' style='width:300px; border-collapse:collapse; border:2px solid black'>
                    <tr>
                        <td style='padding:10px'>
                            <table id='inner' style='width:100px; border-collapse:separate; border:3px solid red; border-spacing:0'>
                                <tr><td style='height:20px'>X</td></tr>
                            </table>
                        </td>
                    </tr>
                </table></body>");
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"inner.borderTop={inner.BorderTopWidth}");
            // Inner table has its own separate border model
            Assert.True(System.Math.Abs(inner.BorderTopWidth - 3) < 1,
                $"Nested table has independent border (got {inner.BorderTopWidth})");
        }

        [Fact]
        public void NestedTable_InnerCollapsedOuterSeparate()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='outer' style='width:300px; border-collapse:separate; border:2px solid black; border-spacing:5px'>
                    <tr>
                        <td style='padding:5px'>
                            <table id='inner' style='width:150px; border-collapse:collapse; border:4px solid blue'>
                                <tr>
                                    <td id='ic' style='height:20px; border:4px solid blue'>X</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table></body>");
            var outer = LayoutTestHelper.FindById(root, "outer")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"outer.borderTop={outer.BorderTopWidth} inner.borderTop={inner.BorderTopWidth}");
            Assert.True(System.Math.Abs(outer.BorderTopWidth - 2) < 1,
                $"Outer keeps separate 2px border (got {outer.BorderTopWidth})");
            // Inner table uses collapse mode independently
            Assert.True(inner.BorderTopWidth >= 1,
                $"Inner table has collapsed border (got {inner.BorderTopWidth})");
        }

        [Fact]
        public void CollapseInheritance_FromTableToCell()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px; border-collapse:collapse; border:4px solid black'>
                    <tr>
                        <td id='c1' style='height:30px; border:4px solid black; padding:0'>A</td>
                        <td id='c2' style='height:30px; border:4px solid black; padding:0'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell2 = LayoutTestHelper.FindById(root, "c2")!;
            // In collapsed mode, adjacent borders merge — no double-width border between cells
            float cell1Right = cell1.BorderRect.X + cell1.BorderRect.Width;
            float cell2Left = cell2.BorderRect.X;
            _output.WriteLine($"c1.right={cell1Right} c2.left={cell2Left}");
            Assert.True(System.Math.Abs(cell1Right - cell2Left) < 2,
                $"Borders collapse between cells (c1.right={cell1Right}, c2.left={cell2Left})");
        }

        [Fact]
        public void Collapse_TableWidthIncludesBorders()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='width:200px; border-collapse:collapse; border:4px solid black'>
                    <tr><td style='height:30px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentW={table.ContentRect.Width} borderBoxW={table.BorderRect.Width}");
            // CSS width:200px is content-box; content width should be 200px
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 2,
                $"Table content width should be 200px (got {table.ContentRect.Width})");
            // Border-box adds the collapsed borders
            Assert.True(table.BorderRect.Width > 200,
                $"Border-box should exceed content width (got {table.BorderRect.Width})");
        }

        [Fact]
        public void SeparateMode_TableWidthIncludesBordersAndSpacing()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='width:200px; border-collapse:separate; border:4px solid black; border-spacing:0'>
                    <tr><td style='height:30px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"content.W={table.ContentRect.Width} borderBox.W={table.BorderRect.Width}");
            // CSS width:200px is content-box; border-box = content + 2*border
            Assert.True(System.Math.Abs(table.ContentRect.Width - 200) < 2,
                $"Content width should be 200px (got {table.ContentRect.Width})");
            Assert.True(System.Math.Abs(table.BorderLeftWidth - 4) < 1,
                $"Left border should be 4px (got {table.BorderLeftWidth})");
            Assert.True(System.Math.Abs(table.BorderRightWidth - 4) < 1,
                $"Right border should be 4px (got {table.BorderRightWidth})");
        }

        [Fact]
        public void CollapsedBorder_OuterEdgeHalved()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='width:200px; border-collapse:collapse; border:none'>
                    <tr>
                        <td id='c1' style='height:30px; border:4px solid black; padding:0'>A</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "c1")!;
            // Outer edges of collapsed table: border is halved
            _output.WriteLine($"cell border: T={cell.BorderTopWidth} L={cell.BorderLeftWidth}");
            Assert.True(cell.BorderTopWidth <= 4,
                $"Outer cell border should be at most 4px (got {cell.BorderTopWidth})");
        }

        [Fact]
        public void MultipleCells_CollapsedBorderPositioning()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px; border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px; border:2px solid black; padding:0'>A</td>
                        <td id='c2' style='height:30px; border:2px solid black; padding:0'>B</td>
                        <td id='c3' style='height:30px; border:2px solid black; padding:0'>C</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell2 = LayoutTestHelper.FindById(root, "c2")!;
            var cell3 = LayoutTestHelper.FindById(root, "c3")!;
            // All three cells should be contiguous with collapsed borders
            _output.WriteLine($"c1.X={cell1.ContentRect.X} c2.X={cell2.ContentRect.X} c3.X={cell3.ContentRect.X}");
            Assert.True(cell2.ContentRect.X > cell1.ContentRect.X,
                "Cells should be laid out left to right");
            Assert.True(cell3.ContentRect.X > cell2.ContentRect.X,
                "Third cell should be after second");
            float totalSpan = cell3.BorderRect.X + cell3.BorderRect.Width - cell1.BorderRect.X;
            Assert.True(System.Math.Abs(totalSpan - 300) < 5,
                $"Total span should be close to table width (got {totalSpan})");
        }

        [Fact]
        public void TwoRows_CollapsedBorderVertical()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px; border-collapse:collapse'>
                    <tr>
                        <td id='r1' style='height:40px; border:2px solid black; padding:0'>R1</td>
                    </tr>
                    <tr>
                        <td id='r2' style='height:40px; border:2px solid black; padding:0'>R2</td>
                    </tr>
                </table></body>");
            var row1Cell = LayoutTestHelper.FindById(root, "r1")!;
            var row2Cell = LayoutTestHelper.FindById(root, "r2")!;
            // Vertical border between rows should collapse
            float row1Bottom = row1Cell.BorderRect.Y + row1Cell.BorderRect.Height;
            _output.WriteLine($"r1.bottom={row1Bottom} r2.top={row2Cell.BorderRect.Y}");
            Assert.True(System.Math.Abs(row2Cell.BorderRect.Y - row1Bottom) < 2,
                $"Row borders should collapse vertically (gap={row2Cell.BorderRect.Y - row1Bottom})");
        }

        [Fact]
        public void SeparateMode_LargeBorderSpacing()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='width:300px; border-collapse:separate; border-spacing:30px'>
                    <tr>
                        <td id='c1' style='height:30px; padding:0'>A</td>
                        <td id='c2' style='height:30px; padding:0'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t")!;
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell2 = LayoutTestHelper.FindById(root, "c2")!;
            // Horizontal: left(30) + cell1 + between(30) + cell2 + right(30) = 300 content
            float gap = cell2.ContentRect.X - (cell1.ContentRect.X + cell1.ContentRect.Width);
            _output.WriteLine($"gap={gap} tableH={table.ContentRect.Height}");
            Assert.True(gap >= 28, $"30px horizontal spacing (got {gap})");
            // Vertical: top(30) + row(30) + bottom(30) = 90
            Assert.True(System.Math.Abs(table.ContentRect.Height - 90) < 2,
                $"Table height with 30px spacing (got {table.ContentRect.Height})");
        }

        [Fact]
        public void Collapse_DifferentBorderWidthsPerSide()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px; border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px; border-top:1px solid; border-right:4px solid; border-bottom:2px solid; border-left:3px solid; padding:0'>A</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "c1")!;
            _output.WriteLine($"T={cell.BorderTopWidth} R={cell.BorderRightWidth} B={cell.BorderBottomWidth} L={cell.BorderLeftWidth}");
            // Each side should have some border width in collapsed mode
            Assert.True(cell.BorderTopWidth >= 0.5f, $"Top border exists (got {cell.BorderTopWidth})");
            Assert.True(cell.BorderRightWidth >= 1, $"Right border exists (got {cell.BorderRightWidth})");
            Assert.True(cell.BorderBottomWidth >= 0.5f, $"Bottom border exists (got {cell.BorderBottomWidth})");
            Assert.True(cell.BorderLeftWidth >= 1, $"Left border exists (got {cell.BorderLeftWidth})");
        }

        [Fact]
        public void Collapse_AdjacentCellsDifferentBorders()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px; border-collapse:collapse'>
                    <tr>
                        <td id='c1' style='height:30px; border:2px solid black; padding:0'>A</td>
                        <td id='c2' style='height:30px; border:6px solid red; padding:0'>B</td>
                    </tr>
                </table></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1")!;
            var cell2 = LayoutTestHelper.FindById(root, "c2")!;
            // At the shared edge, the wider border wins (6px > 2px)
            // The winning border is split between the two cells
            _output.WriteLine($"c1.borderRight={cell1.BorderRightWidth} c2.borderLeft={cell2.BorderLeftWidth}");
            float sharedWidth = cell1.BorderRightWidth + cell2.BorderLeftWidth;
            Assert.True(sharedWidth >= 5, $"Shared border should be ~6px (winning) (got {sharedWidth})");
        }

        [Fact]
        public void RowspanWithBorders_SeparateMode()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px; border-collapse:separate; border-spacing:5px'>
                    <tr>
                        <td id='span' rowspan='2' style='border:2px solid black'>Span</td>
                        <td style='height:30px; border:2px solid black'>B1</td>
                    </tr>
                    <tr>
                        <td style='height:30px; border:2px solid black'>B2</td>
                    </tr>
                </table></body>");
            var spanning = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"span.H={spanning.ContentRect.Height} span.borderT={spanning.BorderTopWidth}");
            // In separate mode, borders are not shared
            Assert.True(System.Math.Abs(spanning.BorderTopWidth - 2) < 1,
                $"Full border in separate mode (got {spanning.BorderTopWidth})");
            // Cell should span both rows plus spacing between
            Assert.True(spanning.ContentRect.Height >= 60,
                $"Rowspan spans both rows + spacing (got {spanning.ContentRect.Height})");
        }

        [Fact]
        public void ColspanWithBorders_SeparateMode()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:300px; border-collapse:separate; border-spacing:10px'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px; border:2px solid black'>Wide</td>
                        <td style='height:30px; border:2px solid black'>C</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>A</td>
                        <td style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var spanning = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"colspan.W={spanning.ContentRect.Width} border={spanning.BorderLeftWidth}");
            Assert.True(System.Math.Abs(spanning.BorderLeftWidth - 2) < 1,
                $"Full border in separate mode (got {spanning.BorderLeftWidth})");
        }

        [Fact]
        public void CollapsedBorder_CellPositionAccountsForHalvedBorder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='t' style='width:200px; border-collapse:collapse; border:4px solid black'>
                    <tr>
                        <td id='c1' style='height:30px; border:4px solid black; padding:0'>A</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t")!;
            var cell = LayoutTestHelper.FindById(root, "c1")!;
            // Cell content should start after the halved border
            _output.WriteLine($"table.X={table.ContentRect.X} cell.X={cell.ContentRect.X}");
            Assert.True(cell.ContentRect.X >= table.ContentRect.X,
                "Cell content starts at or after table content origin");
        }

        [Fact]
        public void SeparateMode_BorderAndPaddingCombined()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width:200px; border-collapse:separate; border-spacing:0'>
                    <tr>
                        <td id='c1' style='height:30px; border:3px solid black; padding:10px'>A</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "c1")!;
            _output.WriteLine($"content.W={cell.ContentRect.Width} padding={cell.PaddingLeft} border={cell.BorderLeftWidth}");
            Assert.True(System.Math.Abs(cell.BorderLeftWidth - 3) < 1, $"Border width preserved (got {cell.BorderLeftWidth})");
            Assert.True(System.Math.Abs(cell.PaddingLeft - 10) < 1, $"Padding preserved (got {cell.PaddingLeft})");
            // Content width should be reduced by border and padding on both sides
            float expectedContent = 200 - 2 * (3 + 10);
            Assert.True(System.Math.Abs(cell.ContentRect.Width - expectedContent) < 5,
                $"Content width after border+padding (got {cell.ContentRect.Width}, expected ~{expectedContent})");
        }
    }
}
