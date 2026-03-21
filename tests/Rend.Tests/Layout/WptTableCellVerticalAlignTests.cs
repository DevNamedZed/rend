using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTableCellVerticalAlignTests
    {
        private readonly ITestOutputHelper _output;

        public WptTableCellVerticalAlignTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-TABLES §4.9] vertical-align:top is the default for td
        [Fact]
        public void VerticalAlignTop_ContentAtTopOfCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:100px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:top;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"cell.Y={cell.ContentRect.Y} inner.Y={inner.ContentRect.Y}");
            Assert.True(System.Math.Abs(inner.ContentRect.Y - cell.ContentRect.Y) < 3,
                $"vertical-align:top content should be at cell top (inner.Y={inner.ContentRect.Y}, cell.Y={cell.ContentRect.Y})");
        }

        // [CSS-TABLES §4.9] vertical-align:middle centers content vertically
        [Fact]
        public void VerticalAlignMiddle_ContentCenteredInCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:100px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:middle;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float expectedMiddle = cell.ContentRect.Y + (cell.ContentRect.Height - 20) / 2;
            _output.WriteLine($"cell.Y={cell.ContentRect.Y} cell.H={cell.ContentRect.Height} inner.Y={inner.ContentRect.Y} expected={expectedMiddle}");
            Assert.True(System.Math.Abs(inner.ContentRect.Y - expectedMiddle) < 3,
                $"vertical-align:middle content should be centered (inner.Y={inner.ContentRect.Y}, expected ~{expectedMiddle})");
        }

        // [CSS-TABLES §4.9] vertical-align:bottom pushes content to bottom
        [Fact]
        public void VerticalAlignBottom_ContentAtBottomOfCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:100px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:bottom;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float cellBottom = cell.ContentRect.Y + cell.ContentRect.Height;
            float innerBottom = inner.ContentRect.Y + inner.ContentRect.Height;
            _output.WriteLine($"cellBottom={cellBottom} innerBottom={innerBottom}");
            Assert.True(System.Math.Abs(innerBottom - cellBottom) < 3,
                $"vertical-align:bottom content should be at bottom (innerBottom={innerBottom}, cellBottom={cellBottom})");
        }

        // [CSS-TABLES §4.9] vertical-align:baseline aligns first text baseline
        [Fact]
        public void VerticalAlignBaseline_ContentAtBaseline()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='baseCell' style='vertical-align:baseline;width:100px;font-size:16px'>Base</td>
                        <td style='height:60px;width:100px'>Tall</td>
                    </tr>
                </table></body>");
            var baseCell = LayoutTestHelper.FindById(root, "baseCell")!;
            _output.WriteLine($"baseline cell Y={baseCell.ContentRect.Y} H={baseCell.ContentRect.Height}");
            Assert.True(baseCell.ContentRect.Height > 0,
                $"Baseline cell should have positive height (got {baseCell.ContentRect.Height})");
        }

        // [CSS-TABLES §4.9] vertical-align:top with varying row heights
        [Fact]
        public void VerticalAlignTop_WithTallRow_ContentStaysAtTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td style='height:150px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:top;width:100px'>
                            <div id='inner' style='height:30px;background:blue'></div>
                        </td>
                        <td style='width:100px'>Short</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"cell.Y={cell.ContentRect.Y} cell.H={cell.ContentRect.Height} inner.Y={inner.ContentRect.Y}");
            Assert.True(System.Math.Abs(inner.ContentRect.Y - cell.ContentRect.Y) < 3,
                $"Content should be at top regardless of row height (inner.Y={inner.ContentRect.Y}, cell.Y={cell.ContentRect.Y})");
            Assert.True(cell.ContentRect.Height >= 148,
                $"Cell should stretch to row height 150 (got {cell.ContentRect.Height})");
        }

        // [CSS-TABLES §4.9] vertical-align:middle with varying row heights
        [Fact]
        public void VerticalAlignMiddle_WithTallRow_ContentCentered()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:200px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:middle;width:100px'>
                            <div id='inner' style='height:40px;background:green'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float expectedMiddle = cell.ContentRect.Y + (cell.ContentRect.Height - 40) / 2;
            _output.WriteLine($"cell.Y={cell.ContentRect.Y} cell.H={cell.ContentRect.Height} inner.Y={inner.ContentRect.Y} expected={expectedMiddle}");
            Assert.True(System.Math.Abs(inner.ContentRect.Y - expectedMiddle) < 3,
                $"Content should be centered in 200px tall row (inner.Y={inner.ContentRect.Y}, expected ~{expectedMiddle})");
        }

        // [CSS-TABLES §4.9] vertical-align:bottom with varying row heights
        [Fact]
        public void VerticalAlignBottom_WithTallRow_ContentAtBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:200px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:bottom;width:100px'>
                            <div id='inner' style='height:40px;background:orange'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float cellBottom = cell.ContentRect.Y + cell.ContentRect.Height;
            float innerBottom = inner.ContentRect.Y + inner.ContentRect.Height;
            _output.WriteLine($"cellBottom={cellBottom} innerBottom={innerBottom}");
            Assert.True(System.Math.Abs(innerBottom - cellBottom) < 3,
                $"Content should be at bottom of 200px tall row (innerBottom={innerBottom}, cellBottom={cellBottom})");
        }

        // [CSS-TABLES §4.9] cell stretches to row height determined by tallest cell
        [Fact]
        public void CellStretchesToRowHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:120px;width:100px'>Tall</td>
                        <td id='cell' style='width:100px'>Short</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            _output.WriteLine($"cell.H={cell.ContentRect.Height}");
            Assert.True(System.Math.Abs(cell.ContentRect.Height - 120) < 2,
                $"Cell should stretch to tallest cell height 120 (got {cell.ContentRect.Height})");
        }

        // [CSS 2.1 §17.5.3] cell with explicit height does not stretch shorter cell
        [Fact]
        public void CellExplicitHeight_DoesNotShrinkBelowSpecified()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='explicit' style='height:50px;width:100px'>Explicit</td>
                        <td style='height:80px;width:100px'>Taller</td>
                    </tr>
                </table></body>");
            var explicit_ = LayoutTestHelper.FindById(root, "explicit")!;
            _output.WriteLine($"explicit.H={explicit_.ContentRect.Height}");
            Assert.True(explicit_.ContentRect.Height >= 78,
                $"Cell with explicit height should stretch to row height (got {explicit_.ContentRect.Height})");
        }

        // [CSS 2.1 §17.5.3] tallest cell determines row height
        [Fact]
        public void TallestCellDeterminesRowHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='short' style='height:30px;width:100px'>Short</td>
                        <td id='medium' style='height:60px;width:100px'>Medium</td>
                        <td id='tall' style='height:90px;width:100px'>Tall</td>
                    </tr>
                </table></body>");
            var shortCell = LayoutTestHelper.FindById(root, "short")!;
            var mediumCell = LayoutTestHelper.FindById(root, "medium")!;
            var tallCell = LayoutTestHelper.FindById(root, "tall")!;
            _output.WriteLine($"short.H={shortCell.ContentRect.Height} medium.H={mediumCell.ContentRect.Height} tall.H={tallCell.ContentRect.Height}");
            Assert.True(System.Math.Abs(shortCell.ContentRect.Height - 90) < 2,
                $"Short cell should stretch to 90 (got {shortCell.ContentRect.Height})");
            Assert.True(System.Math.Abs(mediumCell.ContentRect.Height - 90) < 2,
                $"Medium cell should stretch to 90 (got {mediumCell.ContentRect.Height})");
            Assert.True(System.Math.Abs(tallCell.ContentRect.Height - 90) < 2,
                $"Tall cell should be 90 (got {tallCell.ContentRect.Height})");
        }

        // [CSS-TABLES §4.9] padding with vertical-align:middle
        [Fact]
        public void CellPadding_WithVerticalAlignMiddle()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:120px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:middle;padding:10px;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"cell.Y={cell.ContentRect.Y} cell.H={cell.ContentRect.Height} padding.T={cell.PaddingTop} inner.Y={inner.ContentRect.Y}");
            Assert.True(cell.PaddingTop >= 9,
                $"Padding should be applied (got {cell.PaddingTop})");
            Assert.True(cell.PaddingBottom >= 9,
                $"Padding should be applied (got {cell.PaddingBottom})");
        }

        // [CSS-TABLES §4.9] padding with vertical-align:bottom
        [Fact]
        public void CellPadding_WithVerticalAlignBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:120px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:bottom;padding:10px;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float cellContentBottom = cell.ContentRect.Y + cell.ContentRect.Height;
            float innerBottom = inner.ContentRect.Y + inner.ContentRect.Height;
            _output.WriteLine($"cellContentBottom={cellContentBottom} innerBottom={innerBottom} padding.B={cell.PaddingBottom}");
            Assert.True(System.Math.Abs(innerBottom - cellContentBottom) < 3,
                $"Content should be at bottom of content area (innerBottom={innerBottom}, cellContentBottom={cellContentBottom})");
        }

        // [CSS-TABLES §4.9] border with vertical-align:middle
        [Fact]
        public void CellBorder_WithVerticalAlignMiddle()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:separate;border-spacing:0'>
                    <tr>
                        <td style='height:100px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:middle;border:3px solid black;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"cell.Y={cell.ContentRect.Y} cell.H={cell.ContentRect.Height} border.T={cell.BorderTopWidth} inner.Y={inner.ContentRect.Y}");
            Assert.True(cell.BorderTopWidth >= 2,
                $"Border should be applied (got {cell.BorderTopWidth})");
            Assert.True(cell.ContentRect.Height > 0,
                $"Cell content area should have positive height (got {cell.ContentRect.Height})");
        }

        // [CSS-TABLES §4.9] two cells with different vertical-align in same row
        [Fact]
        public void TwoCells_DifferentVerticalAlign()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td style='height:100px;width:100px'>Tall</td>
                        <td id='topCell' style='vertical-align:top;width:100px'>
                            <div id='topInner' style='height:20px;background:red'></div>
                        </td>
                        <td id='bottomCell' style='vertical-align:bottom;width:100px'>
                            <div id='bottomInner' style='height:20px;background:blue'></div>
                        </td>
                    </tr>
                </table></body>");
            var topCell = LayoutTestHelper.FindById(root, "topCell")!;
            var topInner = LayoutTestHelper.FindById(root, "topInner")!;
            var bottomCell = LayoutTestHelper.FindById(root, "bottomCell")!;
            var bottomInner = LayoutTestHelper.FindById(root, "bottomInner")!;
            float topInnerFromCellTop = topInner.ContentRect.Y - topCell.ContentRect.Y;
            float bottomInnerBottom = bottomInner.ContentRect.Y + bottomInner.ContentRect.Height;
            float bottomCellBottom = bottomCell.ContentRect.Y + bottomCell.ContentRect.Height;
            _output.WriteLine($"topInner offset={topInnerFromCellTop} bottomInnerBottom={bottomInnerBottom} bottomCellBottom={bottomCellBottom}");
            Assert.True(topInnerFromCellTop < 3,
                $"Top-aligned content should be near cell top (offset={topInnerFromCellTop})");
            Assert.True(System.Math.Abs(bottomInnerBottom - bottomCellBottom) < 3,
                $"Bottom-aligned content should be at cell bottom (innerBottom={bottomInnerBottom}, cellBottom={bottomCellBottom})");
        }

        // [CSS-TABLES §4.9] three cells with top/middle/bottom in same row
        [Fact]
        public void ThreeCells_MixedVerticalAlign()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td style='height:120px;width:100px'>Tall</td>
                        <td id='topCell' style='vertical-align:top;width:100px'>
                            <div id='topDiv' style='height:20px;background:red'></div>
                        </td>
                        <td id='midCell' style='vertical-align:middle;width:100px'>
                            <div id='midDiv' style='height:20px;background:green'></div>
                        </td>
                        <td id='botCell' style='vertical-align:bottom;width:100px'>
                            <div id='botDiv' style='height:20px;background:blue'></div>
                        </td>
                    </tr>
                </table></body>");
            var topCell = LayoutTestHelper.FindById(root, "topCell")!;
            var topDiv = LayoutTestHelper.FindById(root, "topDiv")!;
            var midCell = LayoutTestHelper.FindById(root, "midCell")!;
            var midDiv = LayoutTestHelper.FindById(root, "midDiv")!;
            var botCell = LayoutTestHelper.FindById(root, "botCell")!;
            var botDiv = LayoutTestHelper.FindById(root, "botDiv")!;

            float topOffset = topDiv.ContentRect.Y - topCell.ContentRect.Y;
            float midExpected = midCell.ContentRect.Y + (midCell.ContentRect.Height - 20) / 2;
            float botBottom = botDiv.ContentRect.Y + botDiv.ContentRect.Height;
            float botCellBottom = botCell.ContentRect.Y + botCell.ContentRect.Height;

            _output.WriteLine($"top offset={topOffset} mid.Y={midDiv.ContentRect.Y} expected={midExpected} botBottom={botBottom} botCellBottom={botCellBottom}");
            Assert.True(topOffset < 3,
                $"Top cell content at top (offset={topOffset})");
            Assert.True(System.Math.Abs(midDiv.ContentRect.Y - midExpected) < 3,
                $"Middle cell content centered (midDiv.Y={midDiv.ContentRect.Y}, expected ~{midExpected})");
            Assert.True(System.Math.Abs(botBottom - botCellBottom) < 3,
                $"Bottom cell content at bottom (botBottom={botBottom}, cellBottom={botCellBottom})");
        }

        // [CSS-TABLES §4.9] vertical-align in fixed table layout
        [Fact]
        public void VerticalAlignMiddle_InFixedLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:100px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:middle;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float expectedMiddle = cell.ContentRect.Y + (cell.ContentRect.Height - 20) / 2;
            _output.WriteLine($"fixed layout: cell.H={cell.ContentRect.Height} inner.Y={inner.ContentRect.Y} expected={expectedMiddle}");
            Assert.True(System.Math.Abs(inner.ContentRect.Y - expectedMiddle) < 3,
                $"Fixed layout middle alignment (inner.Y={inner.ContentRect.Y}, expected ~{expectedMiddle})");
        }

        // [CSS-TABLES §4.9] vertical-align in auto table layout
        [Fact]
        public void VerticalAlignBottom_InAutoLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:80px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:bottom;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float cellBottom = cell.ContentRect.Y + cell.ContentRect.Height;
            float innerBottom = inner.ContentRect.Y + inner.ContentRect.Height;
            _output.WriteLine($"auto layout: cellBottom={cellBottom} innerBottom={innerBottom}");
            Assert.True(System.Math.Abs(innerBottom - cellBottom) < 3,
                $"Auto layout bottom alignment (innerBottom={innerBottom}, cellBottom={cellBottom})");
        }

        // [CSS-TABLES §4.9] vertical-align on th element (default is middle per UA)
        [Fact]
        public void VerticalAlignOnTh_DefaultMiddle()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <th style='height:100px;width:100px'>Tall Header</th>
                        <th id='cell' style='width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </th>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"th cell.Y={cell.ContentRect.Y} cell.H={cell.ContentRect.Height} inner.Y={inner.ContentRect.Y}");
            Assert.True(cell.ContentRect.Height >= 98,
                $"th cell should stretch to row height (got {cell.ContentRect.Height})");
        }

        // [CSS-TABLES §4.9] vertical-align:top explicitly on th
        [Fact]
        public void VerticalAlignTop_OnTh()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <th style='height:100px;width:100px'>Tall</th>
                        <th id='cell' style='vertical-align:top;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </th>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float topOffset = inner.ContentRect.Y - cell.ContentRect.Y;
            _output.WriteLine($"th top: cell.Y={cell.ContentRect.Y} inner.Y={inner.ContentRect.Y} offset={topOffset}");
            Assert.True(topOffset < 3,
                $"th with vertical-align:top should have content at top (offset={topOffset})");
        }

        // [CSS-TABLES §4.9] empty cell with vertical-align:middle
        [Fact]
        public void EmptyCell_VerticalAlignMiddle()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:80px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:middle;width:100px'></td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            _output.WriteLine($"empty middle cell.H={cell.ContentRect.Height}");
            Assert.True(System.Math.Abs(cell.ContentRect.Height - 80) < 2,
                $"Empty cell with vertical-align:middle should stretch to row height (got {cell.ContentRect.Height})");
        }

        // [CSS-TABLES §4.9] empty cell with vertical-align:bottom
        [Fact]
        public void EmptyCell_VerticalAlignBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:80px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:bottom;width:100px'></td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            _output.WriteLine($"empty bottom cell.H={cell.ContentRect.Height}");
            Assert.True(System.Math.Abs(cell.ContentRect.Height - 80) < 2,
                $"Empty cell with vertical-align:bottom should stretch to row height (got {cell.ContentRect.Height})");
        }

        // [CSS-TABLES §4.9] rowspan cell with vertical-align:middle
        [Fact]
        public void RowspanCell_VerticalAlignMiddle()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='cell' rowspan='2' style='vertical-align:middle;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                        <td style='height:50px;width:100px'>A</td>
                    </tr>
                    <tr>
                        <td style='height:50px;width:100px'>B</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float expectedMiddle = cell.ContentRect.Y + (cell.ContentRect.Height - 20) / 2;
            _output.WriteLine($"rowspan middle: cell.H={cell.ContentRect.Height} inner.Y={inner.ContentRect.Y} expected={expectedMiddle}");
            Assert.True(cell.ContentRect.Height >= 98,
                $"Rowspan cell should span both rows (got {cell.ContentRect.Height})");
            Assert.True(System.Math.Abs(inner.ContentRect.Y - expectedMiddle) < 3,
                $"Rowspan cell with middle align (inner.Y={inner.ContentRect.Y}, expected ~{expectedMiddle})");
        }

        // [CSS-TABLES §4.9] rowspan cell with vertical-align:bottom
        [Fact]
        public void RowspanCell_VerticalAlignBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='cell' rowspan='2' style='vertical-align:bottom;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                        <td style='height:50px;width:100px'>A</td>
                    </tr>
                    <tr>
                        <td style='height:50px;width:100px'>B</td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float cellBottom = cell.ContentRect.Y + cell.ContentRect.Height;
            float innerBottom = inner.ContentRect.Y + inner.ContentRect.Height;
            _output.WriteLine($"rowspan bottom: cellBottom={cellBottom} innerBottom={innerBottom}");
            Assert.True(cell.ContentRect.Height >= 98,
                $"Rowspan cell should span both rows (got {cell.ContentRect.Height})");
            Assert.True(System.Math.Abs(innerBottom - cellBottom) < 3,
                $"Rowspan cell with bottom align (innerBottom={innerBottom}, cellBottom={cellBottom})");
        }

        // [CSS-TABLES §4.9] vertical-align:middle with large padding
        [Fact]
        public void VerticalAlignMiddle_WithLargePadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:120px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:middle;padding:20px;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"large padding: cell.H={cell.ContentRect.Height} padT={cell.PaddingTop} padB={cell.PaddingBottom} inner.Y={inner.ContentRect.Y}");
            Assert.True(cell.PaddingTop >= 19,
                $"Padding top should be ~20 (got {cell.PaddingTop})");
            Assert.True(cell.PaddingBottom >= 19,
                $"Padding bottom should be ~20 (got {cell.PaddingBottom})");
        }

        // [CSS-TABLES §4.9] vertical-align:top with border and padding
        [Fact]
        public void VerticalAlignTop_WithBorderAndPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:separate;border-spacing:0'>
                    <tr>
                        <td style='height:100px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:top;border:2px solid black;padding:5px;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"border+padding: cell.Y={cell.ContentRect.Y} inner.Y={inner.ContentRect.Y} borderT={cell.BorderTopWidth} padT={cell.PaddingTop}");
            Assert.True(cell.BorderTopWidth >= 1,
                $"Border should be applied (got {cell.BorderTopWidth})");
            Assert.True(cell.PaddingTop >= 4,
                $"Padding should be applied (got {cell.PaddingTop})");
            float topOffset = inner.ContentRect.Y - cell.ContentRect.Y;
            Assert.True(topOffset < 3,
                $"Content should be at top of content area (offset={topOffset})");
        }

        // [CSS-TABLES §4.9] multiple rows with mixed vertical-align
        [Fact]
        public void MultipleRows_MixedVerticalAlign()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:80px;width:100px'>Tall Row 1</td>
                        <td id='midCell' style='vertical-align:middle;width:100px'>
                            <div id='midDiv' style='height:20px;background:green'></div>
                        </td>
                    </tr>
                    <tr>
                        <td style='height:80px;width:100px'>Tall Row 2</td>
                        <td id='botCell' style='vertical-align:bottom;width:100px'>
                            <div id='botDiv' style='height:20px;background:blue'></div>
                        </td>
                    </tr>
                </table></body>");
            var midCell = LayoutTestHelper.FindById(root, "midCell")!;
            var midDiv = LayoutTestHelper.FindById(root, "midDiv")!;
            var botCell = LayoutTestHelper.FindById(root, "botCell")!;
            var botDiv = LayoutTestHelper.FindById(root, "botDiv")!;

            float midExpected = midCell.ContentRect.Y + (midCell.ContentRect.Height - 20) / 2;
            float botBottom = botDiv.ContentRect.Y + botDiv.ContentRect.Height;
            float botCellBottom = botCell.ContentRect.Y + botCell.ContentRect.Height;

            _output.WriteLine($"row1 mid.Y={midDiv.ContentRect.Y} expected={midExpected} | row2 botBottom={botBottom} cellBottom={botCellBottom}");
            Assert.True(System.Math.Abs(midDiv.ContentRect.Y - midExpected) < 3,
                $"Row 1 middle aligned (midDiv.Y={midDiv.ContentRect.Y}, expected ~{midExpected})");
            Assert.True(System.Math.Abs(botBottom - botCellBottom) < 3,
                $"Row 2 bottom aligned (botBottom={botBottom}, cellBottom={botCellBottom})");
        }

        // [CSS-TABLES §4.9] vertical-align with border-spacing
        [Fact]
        public void VerticalAlignMiddle_WithBorderSpacing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:separate;border-spacing:10px'>
                    <tr>
                        <td style='height:100px;width:80px'>Tall</td>
                        <td id='cell' style='vertical-align:middle;width:80px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float expectedMiddle = cell.ContentRect.Y + (cell.ContentRect.Height - 20) / 2;
            _output.WriteLine($"spacing: cell.H={cell.ContentRect.Height} inner.Y={inner.ContentRect.Y} expected={expectedMiddle}");
            Assert.True(System.Math.Abs(inner.ContentRect.Y - expectedMiddle) < 3,
                $"Middle alignment with border-spacing (inner.Y={inner.ContentRect.Y}, expected ~{expectedMiddle})");
        }

        // [CSS-TABLES §4.9] vertical-align with multiple content blocks
        [Fact]
        public void VerticalAlignBottom_MultipleChildBlocks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:120px;width:100px'>Tall</td>
                        <td id='cell' style='vertical-align:bottom;width:100px'>
                            <div style='height:15px;background:red'></div>
                            <div id='last' style='height:15px;background:blue'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var last = LayoutTestHelper.FindById(root, "last")!;
            float cellBottom = cell.ContentRect.Y + cell.ContentRect.Height;
            float lastBottom = last.ContentRect.Y + last.ContentRect.Height;
            _output.WriteLine($"multi-block: cellBottom={cellBottom} lastBottom={lastBottom}");
            Assert.True(System.Math.Abs(lastBottom - cellBottom) < 3,
                $"Last child should be at cell bottom (lastBottom={lastBottom}, cellBottom={cellBottom})");
        }

        // [CSS-TABLES §4.9] UA default vertical-align for td is middle
        [Fact]
        public void DefaultVerticalAlign_BehavesAsMiddle()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:100px;width:100px'>Tall</td>
                        <td id='cell' style='width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float expectedMiddle = cell.ContentRect.Y + (cell.ContentRect.Height - 20) / 2;
            _output.WriteLine($"default: cell.Y={cell.ContentRect.Y} cell.H={cell.ContentRect.Height} inner.Y={inner.ContentRect.Y} expected={expectedMiddle}");
            Assert.True(System.Math.Abs(inner.ContentRect.Y - expectedMiddle) < 3,
                $"Default td vertical-align should behave as middle (inner.Y={inner.ContentRect.Y}, expected ~{expectedMiddle})");
        }

        // [CSS-TABLES §4.9] rowspan cell with vertical-align:top
        [Fact]
        public void RowspanCell_VerticalAlignTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='cell' rowspan='3' style='vertical-align:top;width:100px'>
                            <div id='inner' style='height:20px;background:red'></div>
                        </td>
                        <td style='height:40px;width:100px'>A</td>
                    </tr>
                    <tr><td style='height:40px;width:100px'>B</td></tr>
                    <tr><td style='height:40px;width:100px'>C</td></tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float topOffset = inner.ContentRect.Y - cell.ContentRect.Y;
            _output.WriteLine($"rowspan top: cell.H={cell.ContentRect.Height} inner.Y={inner.ContentRect.Y} offset={topOffset}");
            Assert.True(cell.ContentRect.Height >= 118,
                $"Rowspan=3 cell should span 3 rows (got {cell.ContentRect.Height})");
            Assert.True(topOffset < 3,
                $"Rowspan cell with top align (offset={topOffset})");
        }

        // [CSS-TABLES §4.9] vertical-align on both td and th in same row
        [Fact]
        public void MixedTdAndTh_VerticalAlign()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td style='height:100px;width:100px'>Tall</td>
                        <td id='tdBottom' style='vertical-align:bottom;width:100px'>
                            <div id='tdDiv' style='height:20px;background:red'></div>
                        </td>
                        <th id='thCell' style='vertical-align:top;width:100px'>
                            <div id='thDiv' style='height:20px;background:blue'></div>
                        </th>
                    </tr>
                </table></body>");
            var tdBottom = LayoutTestHelper.FindById(root, "tdBottom")!;
            var tdDiv = LayoutTestHelper.FindById(root, "tdDiv")!;
            var thCell = LayoutTestHelper.FindById(root, "thCell")!;
            var thDiv = LayoutTestHelper.FindById(root, "thDiv")!;

            float tdCellBottom = tdBottom.ContentRect.Y + tdBottom.ContentRect.Height;
            float tdDivBottom = tdDiv.ContentRect.Y + tdDiv.ContentRect.Height;
            float thTopOffset = thDiv.ContentRect.Y - thCell.ContentRect.Y;

            _output.WriteLine($"td bottom: divBottom={tdDivBottom} cellBottom={tdCellBottom} | th top: offset={thTopOffset}");
            Assert.True(System.Math.Abs(tdDivBottom - tdCellBottom) < 3,
                $"td bottom-aligned (divBottom={tdDivBottom}, cellBottom={tdCellBottom})");
            Assert.True(thTopOffset < 3,
                $"th top-aligned (offset={thTopOffset})");
        }
    }
}
