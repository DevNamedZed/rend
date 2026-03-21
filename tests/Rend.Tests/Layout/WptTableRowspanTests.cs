using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTableRowspanTests
    {
        private readonly ITestOutputHelper _output;

        public WptTableRowspanTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Rowspan2_HeightSpansTwoRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2' style='padding:0'>S</td>
                        <td style='height:40px;padding:0'>A</td>
                    </tr>
                    <tr>
                        <td style='height:40px;padding:0'>B</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"rowspan2 height={span.ContentRect.Height}");
            Assert.True(System.Math.Abs(span.ContentRect.Height - 80) < 2,
                $"rowspan=2 should span 80px (got {span.ContentRect.Height})");
        }

        [Fact]
        public void Rowspan3_HeightSpansThreeRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='3' style='padding:0'>S</td>
                        <td style='height:25px;padding:0'>A</td>
                    </tr>
                    <tr><td style='height:25px;padding:0'>B</td></tr>
                    <tr><td style='height:25px;padding:0'>C</td></tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"rowspan3 height={span.ContentRect.Height}");
            Assert.True(System.Math.Abs(span.ContentRect.Height - 75) < 2,
                $"rowspan=3 should span 75px (got {span.ContentRect.Height})");
        }

        [Fact]
        public void Rowspan_PushesAdjacentCellsDown()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td rowspan='2' style='height:80px;padding:0'>S</td>
                        <td id='a' style='height:30px;padding:0'>A</td>
                    </tr>
                    <tr>
                        <td id='b' style='height:30px;padding:0'>B</td>
                    </tr>
                </table></body>");
            var cellA = LayoutTestHelper.FindById(root, "a")!;
            var cellB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={cellA.ContentRect.Y} b.Y={cellB.ContentRect.Y}");
            Assert.True(cellB.ContentRect.Y > cellA.ContentRect.Y,
                $"B should be below A (a.Y={cellA.ContentRect.Y}, b.Y={cellB.ContentRect.Y})");
        }

        [Fact]
        public void Colspan2_WidthSpansTwoColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px;padding:0'>S</td>
                        <td style='height:30px;padding:0'>C</td>
                    </tr>
                    <tr>
                        <td style='width:100px;height:30px;padding:0'>A</td>
                        <td style='width:100px;height:30px;padding:0'>B</td>
                        <td style='width:100px;height:30px;padding:0'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"colspan2 width={span.ContentRect.Width}");
            Assert.True(span.ContentRect.Width >= 198,
                $"colspan=2 should be ~200px (got {span.ContentRect.Width})");
        }

        [Fact]
        public void Colspan3_WidthSpansThreeColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='3' style='height:30px;padding:0'>Full</td>
                    </tr>
                    <tr>
                        <td style='width:100px;height:30px;padding:0'>A</td>
                        <td style='width:100px;height:30px;padding:0'>B</td>
                        <td style='width:100px;height:30px;padding:0'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"colspan3 width={span.ContentRect.Width}");
            Assert.True(span.ContentRect.Width >= 298,
                $"colspan=3 should span full width ~300px (got {span.ContentRect.Width})");
        }

        [Fact]
        public void ColspanAndRowspanCombined()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' rowspan='2' style='height:60px;padding:0'>S</td>
                        <td style='height:30px;padding:0'>C1</td>
                    </tr>
                    <tr>
                        <td style='height:30px;padding:0'>C2</td>
                    </tr>
                    <tr>
                        <td style='width:100px;height:30px;padding:0'>A</td>
                        <td style='width:100px;height:30px;padding:0'>B</td>
                        <td style='width:100px;height:30px;padding:0'>C3</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"combined span w={span.ContentRect.Width} h={span.ContentRect.Height}");
            Assert.True(span.ContentRect.Width >= 198,
                $"colspan=2 width should be ~200px (got {span.ContentRect.Width})");
            Assert.True(span.ContentRect.Height >= 58,
                $"rowspan=2 height should be ~60px (got {span.ContentRect.Height})");
        }

        [Fact]
        public void Rowspan_WithBorderSpacing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:separate;border-spacing:10px'>
                    <tr>
                        <td id='span' rowspan='2' style='padding:0'>S</td>
                        <td style='height:30px;padding:0'>A</td>
                    </tr>
                    <tr>
                        <td style='height:30px;padding:0'>B</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"rowspan+spacing height={span.ContentRect.Height}");
            // row1(30) + spacing(10) + row2(30) = 70
            Assert.True(span.ContentRect.Height >= 68,
                $"rowspan=2 with spacing should be >=70px (got {span.ContentRect.Height})");
        }

        [Fact]
        public void Colspan_WithBorderSpacing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:separate;border-spacing:10px'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px;padding:0'>S</td>
                        <td style='height:30px;padding:0'>C</td>
                    </tr>
                    <tr>
                        <td style='height:30px;padding:0'>A</td>
                        <td style='height:30px;padding:0'>B</td>
                        <td style='height:30px;padding:0'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"colspan+spacing width={span.ContentRect.Width}");
            // colspan=2 should include the inter-column spacing
            Assert.True(span.ContentRect.Width > 100,
                $"colspan=2 with spacing should be wider than single col (got {span.ContentRect.Width})");
        }

        [Fact]
        public void Rowspan_WithBorderCollapse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse;border:2px solid black'>
                    <tr>
                        <td id='span' rowspan='2' style='border:2px solid black;padding:0'>S</td>
                        <td style='height:40px;border:2px solid black;padding:0'>A</td>
                    </tr>
                    <tr>
                        <td style='height:40px;border:2px solid black;padding:0'>B</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"rowspan+collapse height={span.ContentRect.Height}");
            // Collapsed borders share widths, rowspan should still span both rows
            Assert.True(span.ContentRect.Height >= 78,
                $"rowspan=2 collapsed should be ~80px (got {span.ContentRect.Height})");
        }

        [Fact]
        public void Colspan_WithBorderCollapse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse;border:2px solid black'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px;border:2px solid black;padding:0'>S</td>
                        <td style='height:30px;border:2px solid black;padding:0'>C</td>
                    </tr>
                    <tr>
                        <td id='a' style='width:100px;height:30px;border:2px solid black;padding:0'>A</td>
                        <td id='b' style='width:100px;height:30px;border:2px solid black;padding:0'>B</td>
                        <td style='width:100px;height:30px;border:2px solid black;padding:0'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var cellA = LayoutTestHelper.FindById(root, "a")!;
            var cellB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"colspan+collapse width={span.ContentRect.Width} a={cellA.ContentRect.Width} b={cellB.ContentRect.Width}");
            Assert.True(span.ContentRect.Width >= cellA.ContentRect.Width + cellB.ContentRect.Width - 4,
                $"colspan=2 collapsed should span A+B (got {span.ContentRect.Width})");
        }

        [Fact]
        public void Rowspan_CellPositionYMatchesFirstRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2' style='padding:0'>S</td>
                        <td id='a' style='height:30px;padding:0'>A</td>
                    </tr>
                    <tr>
                        <td style='height:30px;padding:0'>B</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var cellA = LayoutTestHelper.FindById(root, "a")!;
            _output.WriteLine($"rowspan Y={span.ContentRect.Y} a.Y={cellA.ContentRect.Y}");
            Assert.True(System.Math.Abs(span.ContentRect.Y - cellA.ContentRect.Y) < 2,
                $"rowspan cell should start at same Y as first row (span.Y={span.ContentRect.Y}, a.Y={cellA.ContentRect.Y})");
        }

        [Fact]
        public void Colspan_CellPositionXMatchesFirstColumn()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px;padding:0'>S</td>
                        <td style='height:30px;padding:0'>C</td>
                    </tr>
                    <tr>
                        <td id='a' style='width:100px;height:30px;padding:0'>A</td>
                        <td style='width:100px;height:30px;padding:0'>B</td>
                        <td style='width:100px;height:30px;padding:0'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var cellA = LayoutTestHelper.FindById(root, "a")!;
            _output.WriteLine($"colspan X={span.ContentRect.X} a.X={cellA.ContentRect.X}");
            Assert.True(System.Math.Abs(span.ContentRect.X - cellA.ContentRect.X) < 2,
                $"colspan cell should start at same X as first column (span.X={span.ContentRect.X}, a.X={cellA.ContentRect.X})");
        }

        [Fact]
        public void Rowspan_WithDifferentRowHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2' style='padding:0'>S</td>
                        <td style='height:20px;padding:0'>Short</td>
                    </tr>
                    <tr>
                        <td style='height:60px;padding:0'>Tall</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"rowspan diff heights={span.ContentRect.Height}");
            Assert.True(System.Math.Abs(span.ContentRect.Height - 80) < 2,
                $"rowspan=2 should span 20+60=80px (got {span.ContentRect.Height})");
        }

        [Fact]
        public void Colspan_WithDifferentColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px;padding:0'>S</td>
                        <td style='height:30px;padding:0'>C</td>
                    </tr>
                    <tr>
                        <td id='narrow' style='width:80px;height:30px;padding:0'>Narrow</td>
                        <td id='wide' style='width:150px;height:30px;padding:0'>Wide</td>
                        <td style='height:30px;padding:0'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var narrow = LayoutTestHelper.FindById(root, "narrow")!;
            var wide = LayoutTestHelper.FindById(root, "wide")!;
            float expectedWidth = narrow.ContentRect.Width + wide.ContentRect.Width;
            _output.WriteLine($"colspan diff widths={span.ContentRect.Width} narrow={narrow.ContentRect.Width} wide={wide.ContentRect.Width}");
            Assert.True(System.Math.Abs(span.ContentRect.Width - expectedWidth) < 4,
                $"colspan=2 should span narrow+wide={expectedWidth} (got {span.ContentRect.Width})");
        }

        [Fact]
        public void MultipleRowspans_InSameTable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='s1' rowspan='2' style='padding:0'>S1</td>
                        <td style='height:30px;padding:0'>A</td>
                        <td id='s2' rowspan='3' style='padding:0'>S2</td>
                    </tr>
                    <tr><td style='height:30px;padding:0'>B</td></tr>
                    <tr><td style='height:30px;padding:0'>D</td><td style='height:30px;padding:0'>E</td></tr>
                </table></body>");
            var span1 = LayoutTestHelper.FindById(root, "s1")!;
            var span2 = LayoutTestHelper.FindById(root, "s2")!;
            _output.WriteLine($"s1.h={span1.ContentRect.Height} s2.h={span2.ContentRect.Height}");
            Assert.True(System.Math.Abs(span1.ContentRect.Height - 60) < 2,
                $"rowspan=2 should be ~60px (got {span1.ContentRect.Height})");
            Assert.True(System.Math.Abs(span2.ContentRect.Height - 90) < 2,
                $"rowspan=3 should be ~90px (got {span2.ContentRect.Height})");
        }

        [Fact]
        public void MultipleColspans_InSameTable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td id='s1' colspan='2' style='height:30px;padding:0'>Top-Left</td>
                        <td id='s2' colspan='2' style='height:30px;padding:0'>Top-Right</td>
                    </tr>
                    <tr>
                        <td style='width:100px;height:30px;padding:0'>A</td>
                        <td style='width:100px;height:30px;padding:0'>B</td>
                        <td style='width:100px;height:30px;padding:0'>C</td>
                        <td style='width:100px;height:30px;padding:0'>D</td>
                    </tr>
                </table></body>");
            var span1 = LayoutTestHelper.FindById(root, "s1")!;
            var span2 = LayoutTestHelper.FindById(root, "s2")!;
            _output.WriteLine($"s1.w={span1.ContentRect.Width} s2.w={span2.ContentRect.Width}");
            Assert.True(span1.ContentRect.Width >= 198,
                $"First colspan=2 should be ~200px (got {span1.ContentRect.Width})");
            Assert.True(span2.ContentRect.Width >= 198,
                $"Second colspan=2 should be ~200px (got {span2.ContentRect.Width})");
        }

        [Fact]
        public void Rowspan_InFirstColumn()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2' style='padding:0'>S</td>
                        <td id='a' style='height:35px;padding:0'>A</td>
                    </tr>
                    <tr>
                        <td id='b' style='height:35px;padding:0'>B</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var cellA = LayoutTestHelper.FindById(root, "a")!;
            _output.WriteLine($"first-col rowspan h={span.ContentRect.Height} x={span.ContentRect.X}");
            Assert.True(System.Math.Abs(span.ContentRect.Height - 70) < 2,
                $"First column rowspan=2 height ~70 (got {span.ContentRect.Height})");
            Assert.True(span.ContentRect.X <= cellA.ContentRect.X,
                $"First column rowspan X should be <= A's X (span.X={span.ContentRect.X}, a.X={cellA.ContentRect.X})");
        }

        [Fact]
        public void Rowspan_InLastColumn()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td style='height:35px;padding:0'>A</td>
                        <td id='span' rowspan='2' style='padding:0'>S</td>
                    </tr>
                    <tr>
                        <td style='height:35px;padding:0'>B</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"last-col rowspan h={span.ContentRect.Height} x={span.ContentRect.X}");
            Assert.True(System.Math.Abs(span.ContentRect.Height - 70) < 2,
                $"Last column rowspan=2 height ~70 (got {span.ContentRect.Height})");
            Assert.True(span.ContentRect.X > 50,
                $"Last column rowspan X should be in second column (got {span.ContentRect.X})");
        }

        [Fact]
        public void Colspan_InFirstRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='3' style='height:30px;padding:0'>Header</td>
                    </tr>
                    <tr>
                        <td style='width:100px;height:30px;padding:0'>A</td>
                        <td style='width:100px;height:30px;padding:0'>B</td>
                        <td style='width:100px;height:30px;padding:0'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"first-row colspan w={span.ContentRect.Width}");
            Assert.True(span.ContentRect.Width >= 298,
                $"First row colspan=3 should span full width (got {span.ContentRect.Width})");
            Assert.True(span.ContentRect.Y < 5,
                $"First row colspan should be at top (got Y={span.ContentRect.Y})");
        }

        [Fact]
        public void Colspan_InLastRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td style='width:100px;height:30px;padding:0'>A</td>
                        <td style='width:100px;height:30px;padding:0'>B</td>
                        <td style='width:100px;height:30px;padding:0'>C</td>
                    </tr>
                    <tr>
                        <td id='span' colspan='3' style='height:30px;padding:0'>Footer</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"last-row colspan w={span.ContentRect.Width} y={span.ContentRect.Y}");
            Assert.True(span.ContentRect.Width >= 298,
                $"Last row colspan=3 should span full width (got {span.ContentRect.Width})");
            Assert.True(span.ContentRect.Y >= 28,
                $"Last row colspan should be below first row (got Y={span.ContentRect.Y})");
        }

        [Fact]
        public void EmptyCells_AdjacentToRowspan()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2' style='width:100px;padding:0'>S</td>
                        <td id='a' style='height:30px;padding:0'>A</td>
                        <td style='height:30px;padding:0'></td>
                    </tr>
                    <tr>
                        <td id='b' style='height:30px;padding:0'>B</td>
                        <td style='height:30px;padding:0'></td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var cellA = LayoutTestHelper.FindById(root, "a")!;
            var cellB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"empty adj: span.h={span.ContentRect.Height} a.Y={cellA.ContentRect.Y} b.Y={cellB.ContentRect.Y}");
            Assert.True(System.Math.Abs(span.ContentRect.Height - 60) < 2,
                $"rowspan=2 with empty neighbors should be ~60px (got {span.ContentRect.Height})");
            Assert.True(cellB.ContentRect.Y > cellA.ContentRect.Y,
                $"B should be below A despite empty cells (a.Y={cellA.ContentRect.Y}, b.Y={cellB.ContentRect.Y})");
        }

        [Fact]
        public void EmptyCells_AdjacentToColspan()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px;padding:0'>S</td>
                        <td style='height:30px;padding:0'></td>
                    </tr>
                    <tr>
                        <td style='width:100px;height:30px;padding:0'>A</td>
                        <td style='width:100px;height:30px;padding:0'>B</td>
                        <td style='width:100px;height:30px;padding:0'></td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"empty adj colspan w={span.ContentRect.Width}");
            Assert.True(span.ContentRect.Width >= 198,
                $"colspan=2 with empty neighbor should be ~200px (got {span.ContentRect.Width})");
        }

        [Fact]
        public void Rowspan_TallerThanSummedRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2' style='height:120px;padding:0'>Tall</td>
                        <td id='a' style='height:20px;padding:0'>A</td>
                    </tr>
                    <tr>
                        <td id='b' style='height:20px;padding:0'>B</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var cellA = LayoutTestHelper.FindById(root, "a")!;
            var cellB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"tall rowspan h={span.ContentRect.Height} a.h={cellA.ContentRect.Height} b.h={cellB.ContentRect.Height}");
            // The rowspan cell is 120px tall but rows only sum to 40px -- rowspan cell should keep its height
            Assert.True(span.ContentRect.Height >= 118,
                $"rowspan taller than rows should be >=120px (got {span.ContentRect.Height})");
            // Adjacent rows should expand or the table should grow
            float adjacentTotal = cellA.ContentRect.Height + cellB.ContentRect.Height;
            Assert.True(adjacentTotal >= 38,
                $"Adjacent cells should have at least their min height (got {adjacentTotal})");
        }

        [Fact]
        public void Rowspan2_TableHeightIncludesBothRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td rowspan='2' style='padding:0'>S</td>
                        <td style='height:50px;padding:0'>A</td>
                    </tr>
                    <tr>
                        <td style='height:50px;padding:0'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"table height={table.ContentRect.Height}");
            Assert.True(System.Math.Abs(table.ContentRect.Height - 100) < 2,
                $"table with rowspan=2 should be ~100px (got {table.ContentRect.Height})");
        }

        [Fact]
        public void Rowspan_ThreeColumns_MiddleColumnSpans()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='left' style='height:30px;padding:0'>L1</td>
                        <td id='span' rowspan='2' style='padding:0'>M</td>
                        <td style='height:30px;padding:0'>R1</td>
                    </tr>
                    <tr>
                        <td style='height:30px;padding:0'>L2</td>
                        <td style='height:30px;padding:0'>R2</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var left = LayoutTestHelper.FindById(root, "left")!;
            _output.WriteLine($"middle rowspan h={span.ContentRect.Height} x={span.ContentRect.X}");
            Assert.True(System.Math.Abs(span.ContentRect.Height - 60) < 2,
                $"Middle column rowspan=2 should be ~60px (got {span.ContentRect.Height})");
            Assert.True(span.ContentRect.X > left.ContentRect.X,
                $"Middle column should be right of first column (got span.X={span.ContentRect.X})");
        }

        [Fact]
        public void Colspan_MiddleColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:400px;border-collapse:collapse'>
                    <tr>
                        <td style='width:100px;height:30px;padding:0'>A</td>
                        <td id='span' colspan='2' style='height:30px;padding:0'>Mid</td>
                        <td style='width:100px;height:30px;padding:0'>D</td>
                    </tr>
                    <tr>
                        <td style='width:100px;height:30px;padding:0'>A2</td>
                        <td style='width:100px;height:30px;padding:0'>B2</td>
                        <td style='width:100px;height:30px;padding:0'>C2</td>
                        <td style='width:100px;height:30px;padding:0'>D2</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"middle colspan w={span.ContentRect.Width}");
            Assert.True(span.ContentRect.Width >= 198,
                $"Middle colspan=2 should be ~200px (got {span.ContentRect.Width})");
        }

        [Fact]
        public void Rowspan_WithBorderSpacing_ThreeRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:separate;border-spacing:5px'>
                    <tr>
                        <td id='span' rowspan='3' style='padding:0'>S</td>
                        <td style='height:20px;padding:0'>A</td>
                    </tr>
                    <tr><td style='height:20px;padding:0'>B</td></tr>
                    <tr><td style='height:20px;padding:0'>C</td></tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            // row1(20) + spacing(5) + row2(20) + spacing(5) + row3(20) = 70
            _output.WriteLine($"rowspan3+spacing h={span.ContentRect.Height}");
            Assert.True(span.ContentRect.Height >= 68,
                $"rowspan=3 with spacing should be ~70px (got {span.ContentRect.Height})");
        }

        [Fact]
        public void Colspan_WithBorderSpacing_ThreeColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:separate;border-spacing:5px'>
                    <tr>
                        <td id='span' colspan='3' style='height:30px;padding:0'>S</td>
                    </tr>
                    <tr>
                        <td style='height:30px;padding:0'>A</td>
                        <td style='height:30px;padding:0'>B</td>
                        <td style='height:30px;padding:0'>C</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"colspan3+spacing w={span.ContentRect.Width}");
            // colspan=3 with spacing should include inter-column spacings
            Assert.True(span.ContentRect.Width >= 270,
                $"colspan=3 with spacing should span most of table width (got {span.ContentRect.Width})");
        }

        [Fact]
        public void Rowspan_RowHeightsDistributed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2' style='height:100px;padding:0'>S</td>
                        <td id='a' style='padding:0'>A</td>
                    </tr>
                    <tr>
                        <td id='b' style='padding:0'>B</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var cellA = LayoutTestHelper.FindById(root, "a")!;
            var cellB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"distributed span.h={span.ContentRect.Height} a.h={cellA.ContentRect.Height} b.h={cellB.ContentRect.Height}");
            // The rowspan cell forces 100px total height across both rows
            Assert.True(span.ContentRect.Height >= 98,
                $"rowspan cell should be ~100px (got {span.ContentRect.Height})");
            // Both rows should have some height
            Assert.True(cellA.ContentRect.Height > 0,
                $"Row 1 cell should have positive height (got {cellA.ContentRect.Height})");
            Assert.True(cellB.ContentRect.Height > 0,
                $"Row 2 cell should have positive height (got {cellB.ContentRect.Height})");
        }

        [Fact]
        public void Colspan_WithExplicitWidths_SpansCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td style='width:50px;height:30px;padding:0'>A</td>
                        <td id='span' colspan='2' style='height:30px;padding:0'>S</td>
                    </tr>
                    <tr>
                        <td style='width:50px;height:30px;padding:0'>A2</td>
                        <td id='b' style='width:100px;height:30px;padding:0'>B2</td>
                        <td id='c' style='width:150px;height:30px;padding:0'>C2</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var cellB = LayoutTestHelper.FindById(root, "b")!;
            var cellC = LayoutTestHelper.FindById(root, "c")!;
            float expectedWidth = cellB.ContentRect.Width + cellC.ContentRect.Width;
            _output.WriteLine($"colspan explicit w={span.ContentRect.Width} b={cellB.ContentRect.Width} c={cellC.ContentRect.Width}");
            Assert.True(System.Math.Abs(span.ContentRect.Width - expectedWidth) < 4,
                $"colspan=2 should match B+C width={expectedWidth} (got {span.ContentRect.Width})");
        }

        [Fact]
        public void Rowspan_DoesNotOverlapNonSpanCells()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='span' rowspan='2' style='width:80px;padding:0'>S</td>
                        <td id='a' style='height:30px;padding:0'>A</td>
                    </tr>
                    <tr>
                        <td id='b' style='height:30px;padding:0'>B</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var cellA = LayoutTestHelper.FindById(root, "a")!;
            var cellB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"no overlap: span.X={span.ContentRect.X} span.W={span.ContentRect.Width} a.X={cellA.ContentRect.X} b.X={cellB.ContentRect.X}");
            float spanRight = span.ContentRect.X + span.ContentRect.Width;
            Assert.True(cellA.ContentRect.X >= spanRight - 2,
                $"A should not overlap rowspan (a.X={cellA.ContentRect.X}, spanRight={spanRight})");
            Assert.True(cellB.ContentRect.X >= spanRight - 2,
                $"B should not overlap rowspan (b.X={cellB.ContentRect.X}, spanRight={spanRight})");
        }

        [Fact]
        public void Colspan_DoesNotOverlapNonSpanCells()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='span' colspan='2' style='height:30px;padding:0'>S</td>
                        <td id='c' style='height:30px;padding:0'>C</td>
                    </tr>
                    <tr>
                        <td style='width:100px;height:30px;padding:0'>A</td>
                        <td style='width:100px;height:30px;padding:0'>B</td>
                        <td style='width:100px;height:30px;padding:0'>C2</td>
                    </tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var cellC = LayoutTestHelper.FindById(root, "c")!;
            float spanRight = span.ContentRect.X + span.ContentRect.Width;
            _output.WriteLine($"no overlap: span.Right={spanRight} c.X={cellC.ContentRect.X}");
            Assert.True(cellC.ContentRect.X >= spanRight - 2,
                $"C should not overlap colspan (c.X={cellC.ContentRect.X}, spanRight={spanRight})");
        }
    }
}
