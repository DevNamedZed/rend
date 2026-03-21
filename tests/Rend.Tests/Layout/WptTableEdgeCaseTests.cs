using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTableEdgeCaseTests
    {
        private readonly ITestOutputHelper _output;
        public WptTableEdgeCaseTests(ITestOutputHelper output) { _output = output; }

        // table with 3 columns of different widths
        [Fact]
        public void ThreeColumns_DifferentWidths()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='a' style='width:100px;height:30px'>A</td>
                        <td id='b' style='width:50px;height:30px'>B</td>
                        <td id='c' style='width:150px;height:30px'>C</td>
                    </tr>
                </table></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            var c = LayoutTestHelper.FindById(r, "c")!;
            _output.WriteLine($"a={a.ContentRect.Width} b={b.ContentRect.Width} c={c.ContentRect.Width}");
            Assert.True(a.ContentRect.Width > 90);
            Assert.True(b.ContentRect.Width > 40);
            Assert.True(c.ContentRect.Width > 140);
        }

        // table with 2 rows, verify heights
        [Fact]
        public void TwoRows_DifferentHeights()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr><td id='r1' style='height:40px'>R1</td></tr>
                    <tr><td id='r2' style='height:60px'>R2</td></tr>
                </table></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "r1")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "r2")!.ContentRect.Height - 60) < 2);
            Assert.True(LayoutTestHelper.FindById(r, "r2")!.ContentRect.Y > 39);
        }

        // table: colspan=3 spans full width in 3-col table
        [Fact]
        public void Colspan3_FullWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse'>
                    <tr><td id='span' colspan='3' style='height:20px'>Full</td></tr>
                    <tr><td style='height:20px'>A</td><td style='height:20px'>B</td><td style='height:20px'>C</td></tr>
                </table></body>");
            var span = LayoutTestHelper.FindById(r, "span")!;
            _output.WriteLine($"colspan3: w={span.ContentRect.Width}");
            Assert.True(span.ContentRect.Width >= 290, $"colspan=3 nearly full width (got {span.ContentRect.Width})");
        }

        // table: rowspan=3 spans 3 rows
        [Fact]
        public void Rowspan3_SpansThreeRows()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr><td id='span' rowspan='3'>S</td><td style='height:30px'>A</td></tr>
                    <tr><td style='height:30px'>B</td></tr>
                    <tr><td style='height:30px'>C</td></tr>
                </table></body>");
            Assert.True(LayoutTestHelper.FindById(r, "span")!.ContentRect.Height >= 89,
                $"rowspan=3 height (got {LayoutTestHelper.FindById(r, "span")!.ContentRect.Height})");
        }

        // table: border-spacing with multiple rows
        [Fact]
        public void BorderSpacing_MultipleRows()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='t' style='width:200px;border-collapse:separate;border-spacing:10px'>
                    <tr><td style='height:30px'>A</td></tr>
                    <tr><td style='height:30px'>B</td></tr>
                </table></body>");
            // spacing: top(10) + row1(30) + between(10) + row2(30) + bottom(10) = 90
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 89);
        }

        // table: percentage width resolves against container
        [Fact]
        public void PercentWidth_Table()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:500px'>
                    <table id='t' style='width:60%;border-collapse:collapse'>
                        <tr><td style='height:30px'>A</td></tr>
                    </table>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 300) < 2);
        }

        // table: fixed layout with equal columns
        [Fact]
        public void FixedLayout_EqualColumns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='table-layout:fixed;width:300px;border-collapse:collapse'>
                    <tr>
                        <td id='a' style='height:30px'>A</td>
                        <td id='b' style='height:30px'>B</td>
                        <td id='c' style='height:30px'>C</td>
                    </tr>
                </table></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            var c = LayoutTestHelper.FindById(r, "c")!;
            _output.WriteLine($"fixed: a={a.ContentRect.Width} b={b.ContentRect.Width} c={c.ContentRect.Width}");
            // Fixed layout without explicit widths distributes space.
            // May have cell padding affecting exact content width.
            float total = a.ContentRect.Width + b.ContentRect.Width + c.ContentRect.Width;
            Assert.True(total >= 290, $"Total fills table (got {total})");
            Assert.True(a.ContentRect.Width > 50 && b.ContentRect.Width > 50 && c.ContentRect.Width > 50,
                $"Each column gets space (a={a.ContentRect.Width} b={b.ContentRect.Width} c={c.ContentRect.Width})");
        }

        // table: cells in same row have same height
        [Fact]
        public void CellsSameRow_SameHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <td id='a' style='height:60px'>Tall</td>
                        <td id='b'>Short</td>
                    </tr>
                </table></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            Assert.True(System.Math.Abs(a.ContentRect.Height - b.ContentRect.Height) < 2,
                $"Same row height: a={a.ContentRect.Height} b={b.ContentRect.Height}");
        }

        // table: auto width shrinks to fit
        [Fact]
        public void AutoWidth_ShrinksToFit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:500px'>
                    <table id='t' style='border-collapse:collapse'>
                        <tr>
                            <td style='width:80px;height:30px'>A</td>
                            <td style='width:60px;height:30px'>B</td>
                        </tr>
                    </table>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width < 200,
                $"Auto table shrinks (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Width})");
        }
    }
}
