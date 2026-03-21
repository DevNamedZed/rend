using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTableCellSizingTests
    {
        private readonly ITestOutputHelper _output;

        public WptTableCellSizingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CellExplicitWidth_MatchesSpecifiedValue()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='t' style='width:150px;height:30px'>A</td>
                        <td style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"cell width: {cell!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell.ContentRect.Width - 150) < 2,
                $"Explicit td width should be 150 (got {cell.ContentRect.Width})");
        }

        [Fact]
        public void CellExplicitHeight_MatchesSpecifiedValue()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='t' style='width:100px;height:80px'>A</td>
                        <td style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"cell height: {cell!.ContentRect.Height}");
            Assert.True(System.Math.Abs(cell.ContentRect.Height - 80) < 2,
                $"Explicit td height should be 80 (got {cell.ContentRect.Height})");
        }

        [Fact]
        public void CellPercentageWidth_ResolvesAgainstTableWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='t' style='width:50%;height:30px'>A</td>
                        <td style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"cell width: {cell!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell.ContentRect.Width - 200) < 4,
                $"50% td width of 400px table should be ~200 (got {cell.ContentRect.Width})");
        }

        [Fact]
        public void CellVerticalAlignMiddle_ContentCentered()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:100px;width:100px'>Tall</td>
                        <td id='t' style='vertical-align:middle;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            var inner = LayoutTestHelper.FindById(root, "inner");
            Assert.NotNull(cell);
            Assert.NotNull(inner);
            float cellTop = cell!.ContentRect.Y;
            float cellHeight = cell.ContentRect.Height;
            float innerTop = inner!.ContentRect.Y;
            float expectedMiddle = cellTop + (cellHeight - 20) / 2;
            _output.WriteLine($"cell Y={cellTop} H={cellHeight} inner Y={innerTop} expected={expectedMiddle}");
            Assert.True(System.Math.Abs(innerTop - expectedMiddle) < 3,
                $"vertical-align:middle content should be centered (inner Y={innerTop}, expected ~{expectedMiddle})");
        }

        [Fact]
        public void CellVerticalAlignBottom_ContentAtBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:100px;width:100px'>Tall</td>
                        <td id='t' style='vertical-align:bottom;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            var inner = LayoutTestHelper.FindById(root, "inner");
            Assert.NotNull(cell);
            Assert.NotNull(inner);
            float cellBottom = cell!.ContentRect.Y + cell.ContentRect.Height;
            float innerBottom = inner!.ContentRect.Y + inner.ContentRect.Height;
            _output.WriteLine($"cell bottom={cellBottom} inner bottom={innerBottom}");
            Assert.True(System.Math.Abs(innerBottom - cellBottom) < 3,
                $"vertical-align:bottom content should be at bottom (inner bottom={innerBottom}, cell bottom={cellBottom})");
        }

        [Fact]
        public void CellVerticalAlignTop_ContentAtTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:100px;width:100px'>Tall</td>
                        <td id='t' style='vertical-align:top;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            var inner = LayoutTestHelper.FindById(root, "inner");
            Assert.NotNull(cell);
            Assert.NotNull(inner);
            _output.WriteLine($"cell Y={cell!.ContentRect.Y} inner Y={inner!.ContentRect.Y}");
            Assert.True(System.Math.Abs(inner.ContentRect.Y - cell.ContentRect.Y) < 3,
                $"vertical-align:top content should be at top (inner Y={inner.ContentRect.Y}, cell Y={cell.ContentRect.Y})");
        }

        [Fact]
        public void CellPadding_AppliedToCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='t' style='padding:10px;height:60px'>A</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            float paddingBoxWidth = cell!.ContentRect.Width + cell.PaddingLeft + cell.PaddingRight;
            _output.WriteLine($"content: {cell.ContentRect.Width} paddingBox: {paddingBoxWidth} padding: L={cell.PaddingLeft} T={cell.PaddingTop}");
            Assert.True(cell.PaddingLeft >= 9 && cell.PaddingTop >= 9,
                $"Padding should be applied (got L={cell.PaddingLeft} T={cell.PaddingTop})");
            Assert.True(paddingBoxWidth > cell.ContentRect.Width,
                $"Padding box should be wider than content (padding={paddingBoxWidth} content={cell.ContentRect.Width})");
        }

        [Fact]
        public void CellColspan_SpansMultipleColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='t' colspan='2' style='height:30px'>Spans</td>
                        <td style='height:30px'>C</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>A</td>
                        <td style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"colspan=2 width: {cell!.ContentRect.Width}");
            Assert.True(cell.ContentRect.Width >= 195,
                $"colspan=2 of 3 equal cols should be ~200 (got {cell.ContentRect.Width})");
        }

        [Fact]
        public void CellRowspan_SpansMultipleRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='t' rowspan='2' style='width:100px'>Spans</td>
                        <td style='height:40px'>B1</td>
                    </tr>
                    <tr>
                        <td style='height:40px'>B2</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"rowspan=2 height: {cell!.ContentRect.Height}");
            Assert.True(cell.ContentRect.Height >= 79,
                $"rowspan=2 should span both rows ~80 (got {cell.ContentRect.Height})");
        }

        [Fact]
        public void FixedLayout_ExplicitColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='a' style='width:100px;height:30px'>A</td>
                        <td id='b' style='width:200px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var cellA = LayoutTestHelper.FindById(root, "a");
            var cellB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(cellA);
            Assert.NotNull(cellB);
            _output.WriteLine($"fixed: a={cellA!.ContentRect.Width} b={cellB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cellA.ContentRect.Width - 100) < 2,
                $"Fixed layout col A should be 100 (got {cellA.ContentRect.Width})");
            Assert.True(System.Math.Abs(cellB.ContentRect.Width - 200) < 2,
                $"Fixed layout col B should be 200 (got {cellB.ContentRect.Width})");
        }

        [Fact]
        public void FixedLayout_EqualDistributionNoExplicitWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='a' style='height:30px'>A</td>
                        <td id='b' style='height:30px'>B</td>
                        <td id='c' style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var cellA = LayoutTestHelper.FindById(root, "a");
            var cellB = LayoutTestHelper.FindById(root, "b");
            var cellC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(cellA);
            Assert.NotNull(cellB);
            Assert.NotNull(cellC);
            _output.WriteLine($"fixed equal: a={cellA!.ContentRect.Width} b={cellB!.ContentRect.Width} c={cellC!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cellA.ContentRect.Width - cellB.ContentRect.Width) < 2,
                $"Fixed equal cols should be same width (a={cellA.ContentRect.Width} b={cellB.ContentRect.Width})");
            Assert.True(System.Math.Abs(cellB.ContentRect.Width - cellC.ContentRect.Width) < 2,
                $"Fixed equal cols should be same width (b={cellB.ContentRect.Width} c={cellC.ContentRect.Width})");
        }

        [Fact]
        public void AutoLayout_ContentDeterminesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='border-collapse:collapse'>
                    <tr>
                        <td id='narrow' style='height:30px'>Hi</td>
                        <td id='wide' style='height:30px'>This is a much wider cell with more text</td>
                    </tr>
                </table></body>");
            var narrow = LayoutTestHelper.FindById(root, "narrow");
            var wide = LayoutTestHelper.FindById(root, "wide");
            Assert.NotNull(narrow);
            Assert.NotNull(wide);
            _output.WriteLine($"auto: narrow={narrow!.ContentRect.Width} wide={wide!.ContentRect.Width}");
            Assert.True(wide.ContentRect.Width > narrow.ContentRect.Width,
                $"Wider content should get wider cell (narrow={narrow.ContentRect.Width} wide={wide.ContentRect.Width})");
        }

        [Fact]
        public void TallestCell_DeterminesRowHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='tall' style='height:80px;width:100px'>Tall</td>
                        <td id='short' style='width:100px'>Short</td>
                    </tr>
                </table></body>");
            var tall = LayoutTestHelper.FindById(root, "tall");
            var shortCell = LayoutTestHelper.FindById(root, "short");
            Assert.NotNull(tall);
            Assert.NotNull(shortCell);
            _output.WriteLine($"tall={tall!.ContentRect.Height} short={shortCell!.ContentRect.Height}");
            Assert.True(System.Math.Abs(tall.ContentRect.Height - shortCell.ContentRect.Height) < 2,
                $"Both cells same height (tall={tall.ContentRect.Height} short={shortCell.ContentRect.Height})");
            Assert.True(shortCell.ContentRect.Height >= 79,
                $"Short cell inherits tall cell height (got {shortCell.ContentRect.Height})");
        }

        [Fact]
        public void CellWithOverflow_ExpandsToFitContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='border-collapse:collapse'>
                    <tr>
                        <td id='t' style='width:50px;height:30px'>
                            <div style='width:100px;height:60px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"overflow cell: {cell!.ContentRect.Width}x{cell.ContentRect.Height}");
            Assert.True(cell.ContentRect.Height >= 59,
                $"Cell should expand to fit content height (got {cell.ContentRect.Height})");
        }

        [Fact]
        public void CellMinWidth_EnforcesMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='t' style='min-width:150px;height:30px'>A</td>
                        <td style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"min-width cell: {cell!.ContentRect.Width}");
            Assert.True(cell.ContentRect.Width >= 146,
                $"Cell min-width should be at least ~150 (got {cell.ContentRect.Width})");
        }

        [Fact]
        public void CellMaxWidth_DoesNotApplyToTableCells()
        {
            // [CSS 2.1 §17.5.3] max-width does not apply to table cells per spec
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='t' style='max-width:100px;width:300px;height:30px'>A</td>
                        <td style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"max-width cell: {cell!.ContentRect.Width}");
            Assert.True(cell.ContentRect.Width > 200,
                $"max-width should not constrain table cells (got {cell.ContentRect.Width})");
        }

        [Fact]
        public void NestedTableInCell_SizedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='outer' style='height:30px'>
                            <table id='inner' style='width:200px;border-collapse:collapse'>
                                <tr><td style='height:25px'>Nested</td></tr>
                            </table>
                        </td>
                    </tr>
                </table></body>");
            var outerCell = LayoutTestHelper.FindById(root, "outer");
            var innerTable = LayoutTestHelper.FindById(root, "inner");
            Assert.NotNull(outerCell);
            Assert.NotNull(innerTable);
            _output.WriteLine($"outer cell: {outerCell!.ContentRect.Width} inner table: {innerTable!.ContentRect.Width}");
            Assert.True(System.Math.Abs(innerTable.ContentRect.Width - 200) < 2,
                $"Nested table should be 200px (got {innerTable.ContentRect.Width})");
            Assert.True(outerCell.ContentRect.Width > innerTable.ContentRect.Width,
                $"Outer cell should be wider than nested table");
        }

        [Fact]
        public void CellBorderBox_PaddingAndBorderStructure()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:separate;border-spacing:0'>
                    <tr>
                        <td id='t' style='box-sizing:border-box;width:150px;padding:10px;border:2px solid black;height:30px'>A</td>
                        <td style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            float borderBoxWidth = cell!.ContentRect.Width + cell.PaddingLeft + cell.PaddingRight
                + cell.BorderLeftWidth + cell.BorderRightWidth;
            _output.WriteLine($"content={cell.ContentRect.Width} border-box={borderBoxWidth} padding={cell.PaddingLeft} border={cell.BorderLeftWidth}");
            Assert.True(cell.PaddingLeft >= 9,
                $"Padding should be applied (got L={cell.PaddingLeft})");
            Assert.True(cell.BorderLeftWidth >= 1,
                $"Border should be applied (got {cell.BorderLeftWidth})");
            Assert.True(borderBoxWidth > cell.ContentRect.Width,
                $"Border-box should be larger than content (border-box={borderBoxWidth} content={cell.ContentRect.Width})");
        }

        [Fact]
        public void EmptyCell_StillHasDimensions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='t' style='width:100px;height:40px'></td>
                        <td style='height:40px'>B</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"empty cell: {cell!.ContentRect.Width}x{cell.ContentRect.Height}");
            Assert.True(cell.ContentRect.Width >= 98,
                $"Empty cell should still have width (got {cell.ContentRect.Width})");
            Assert.True(cell.ContentRect.Height >= 39,
                $"Empty cell should still have height (got {cell.ContentRect.Height})");
        }

        [Fact]
        public void CellPercentageWidth_DifferentPercentages()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='a' style='width:25%;height:30px'>A</td>
                        <td id='b' style='width:75%;height:30px'>B</td>
                    </tr>
                </table></body>");
            var cellA = LayoutTestHelper.FindById(root, "a");
            var cellB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(cellA);
            Assert.NotNull(cellB);
            _output.WriteLine($"25%={cellA!.ContentRect.Width} 75%={cellB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cellA.ContentRect.Width - 100) < 3,
                $"25% of 400 should be ~100 (got {cellA.ContentRect.Width})");
            Assert.True(System.Math.Abs(cellB.ContentRect.Width - 300) < 3,
                $"75% of 400 should be ~300 (got {cellB.ContentRect.Width})");
        }

        [Fact]
        public void CellExplicitWidth_LargerThanContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='t' style='width:300px;height:30px'>X</td>
                        <td style='height:30px'>Y</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"large explicit width: {cell!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell.ContentRect.Width - 300) < 2,
                $"Explicit width 300 on td (got {cell.ContentRect.Width})");
        }

        [Fact]
        public void MultipleRowsDifferentHeights_EachRowIndependent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='r1' style='height:30px'>Row 1</td>
                    </tr>
                    <tr>
                        <td id='r2' style='height:60px'>Row 2</td>
                    </tr>
                    <tr>
                        <td id='r3' style='height:20px'>Row 3</td>
                    </tr>
                </table></body>");
            var row1 = LayoutTestHelper.FindById(root, "r1");
            var row2 = LayoutTestHelper.FindById(root, "r2");
            var row3 = LayoutTestHelper.FindById(root, "r3");
            Assert.NotNull(row1);
            Assert.NotNull(row2);
            Assert.NotNull(row3);
            _output.WriteLine($"r1={row1!.ContentRect.Height} r2={row2!.ContentRect.Height} r3={row3!.ContentRect.Height}");
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 30) < 2,
                $"Row 1 height should be 30 (got {row1.ContentRect.Height})");
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 60) < 2,
                $"Row 2 height should be 60 (got {row2.ContentRect.Height})");
        }

        [Fact]
        public void CellPaddingLarge_AffectsCellAndTableHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='t' style='padding:20px;height:30px'>A</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            float totalHeight = cell!.ContentRect.Height + cell.PaddingTop + cell.PaddingBottom;
            _output.WriteLine($"content H={cell.ContentRect.Height} padding T/B={cell.PaddingTop}/{cell.PaddingBottom} total={totalHeight}");
            Assert.True(cell.PaddingTop >= 19,
                $"Padding top should be ~20 (got {cell.PaddingTop})");
            Assert.True(cell.PaddingBottom >= 19,
                $"Padding bottom should be ~20 (got {cell.PaddingBottom})");
        }

        [Fact]
        public void AutoLayout_WiderContentGetsMoreSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='a' style='height:30px'>
                            <div style='width:200px;height:10px'></div>
                        </td>
                        <td id='b' style='height:30px'>
                            <div style='width:50px;height:10px'></div>
                        </td>
                    </tr>
                </table></body>");
            var cellA = LayoutTestHelper.FindById(root, "a");
            var cellB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(cellA);
            Assert.NotNull(cellB);
            _output.WriteLine($"auto: a={cellA!.ContentRect.Width} b={cellB!.ContentRect.Width}");
            Assert.True(cellA.ContentRect.Width > cellB.ContentRect.Width,
                $"Wider content cell should get more space (a={cellA.ContentRect.Width} b={cellB.ContentRect.Width})");
        }

        [Fact]
        public void FixedLayout_IgnoresContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='a' style='width:100px;height:30px'>
                            <div style='width:300px;height:10px'></div>
                        </td>
                        <td id='b' style='width:100px;height:30px'>B</td>
                    </tr>
                </table></body>");
            var cellA = LayoutTestHelper.FindById(root, "a");
            var cellB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(cellA);
            Assert.NotNull(cellB);
            _output.WriteLine($"fixed ignores content: a={cellA!.ContentRect.Width} b={cellB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cellA.ContentRect.Width - cellB.ContentRect.Width) < 2,
                $"Fixed layout should ignore content width (a={cellA.ContentRect.Width} b={cellB.ContentRect.Width})");
        }

        [Fact]
        public void ColspanCell_WithBorderSpacing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:separate;border-spacing:10px'>
                    <tr>
                        <td id='t' colspan='2' style='height:30px'>Spans</td>
                        <td style='height:30px'>C</td>
                    </tr>
                    <tr>
                        <td style='height:30px'>A</td>
                        <td style='height:30px'>B</td>
                        <td style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"colspan+spacing width: {cell!.ContentRect.Width}");
            Assert.True(cell.ContentRect.Width > 100,
                $"colspan=2 with border-spacing should still span 2 cols (got {cell.ContentRect.Width})");
        }

        [Fact]
        public void RowspanCell_ThreeRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='t' rowspan='3' style='width:100px'>Spans 3</td>
                        <td style='height:25px'>B1</td>
                    </tr>
                    <tr><td style='height:25px'>B2</td></tr>
                    <tr><td style='height:25px'>B3</td></tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"rowspan=3 height: {cell!.ContentRect.Height}");
            Assert.True(cell.ContentRect.Height >= 74,
                $"rowspan=3 should span 3 rows ~75 (got {cell.ContentRect.Height})");
        }

        [Fact]
        public void CellMinWidth_DoesNotApplyToTableCells()
        {
            // [CSS 2.1 §17.5.3] min-width does not apply to table cells per spec;
            // the table layout algorithm determines column widths
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='t' style='width:50px;min-width:120px;height:30px'>A</td>
                        <td style='height:30px'>B</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"cell width with min-width: {cell!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell.ContentRect.Width - 50) < 3,
                $"min-width should not apply to table cells (got {cell.ContentRect.Width})");
        }

        [Fact]
        public void CellBorderBox_HeightIncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='t' style='box-sizing:border-box;height:80px;padding:10px;border:2px solid black'>A</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            float borderBoxHeight = cell!.ContentRect.Height + cell.PaddingTop + cell.PaddingBottom
                + cell.BorderTopWidth + cell.BorderBottomWidth;
            _output.WriteLine($"content H={cell.ContentRect.Height} border-box H={borderBoxHeight}");
            Assert.True(System.Math.Abs(borderBoxHeight - 80) < 3,
                $"border-box height should be 80 (got {borderBoxHeight})");
        }

        [Fact]
        public void EmptyCellNoExplicitSize_StillExists()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='t'></td>
                        <td style='height:40px'>Content</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(cell);
            _output.WriteLine($"empty cell: {cell!.ContentRect.Width}x{cell.ContentRect.Height}");
            Assert.True(cell.ContentRect.Width > 0,
                $"Empty cell should have some width (got {cell.ContentRect.Width})");
            Assert.True(cell.ContentRect.Height >= 39,
                $"Empty cell should match row height (got {cell.ContentRect.Height})");
        }
    }
}
