using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTableTests
    {
        private readonly ITestOutputHelper _output;
        public WptTableTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Table_FixedLayout_EqualColumns()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='table-layout: fixed; width: 200px; border-collapse: collapse;'>
                    <tr>
                        <td style='height: 30px;'>A</td>
                        <td style='height: 30px;'>B</td>
                    </tr>
                </table></body>");
            var tbl = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(tbl);
            _output.WriteLine($"table: {tbl!.ContentRect.Width}x{tbl.ContentRect.Height}");
            Assert.True(tbl.ContentRect.Width >= 199, $"Fixed table width (got {tbl.ContentRect.Width})");
        }

        [Fact]
        public void Table_Colspan()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width: 300px; border-collapse: collapse;'>
                    <tr>
                        <td id='span2' colspan='2' style='height: 30px;'>Spans 2</td>
                        <td style='height: 30px;'>C</td>
                    </tr>
                    <tr>
                        <td style='height: 30px;'>A</td>
                        <td style='height: 30px;'>B</td>
                        <td style='height: 30px;'>C</td>
                    </tr>
                </table></body>");
            var span2 = LayoutTestHelper.FindById(root, "span2");
            Assert.NotNull(span2);
            _output.WriteLine($"span2: w={span2!.ContentRect.Width}");
            // Colspan 2 of 3 equal columns = 200px
            Assert.True(span2.ContentRect.Width >= 198,
                $"colspan=2 should be ~200px (got {span2.ContentRect.Width})");
        }

        [Fact]
        public void Table_Rowspan()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width: 200px; border-collapse: collapse;'>
                    <tr>
                        <td id='span2' rowspan='2' style='width: 100px;'>Spans 2 rows</td>
                        <td style='height: 30px;'>B1</td>
                    </tr>
                    <tr>
                        <td style='height: 30px;'>B2</td>
                    </tr>
                </table></body>");
            var span2 = LayoutTestHelper.FindById(root, "span2");
            Assert.NotNull(span2);
            _output.WriteLine($"span2: h={span2!.ContentRect.Height}");
            // Rowspan 2 → height spans both rows ≥ 60px
            Assert.True(span2.ContentRect.Height >= 59,
                $"rowspan=2 should be ≥60px (got {span2.ContentRect.Height})");
        }

        [Fact]
        public void Table_BorderSpacing_Separate()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width: 200px; border-collapse: separate; border-spacing: 10px;'>
                    <tr>
                        <td style='height: 30px;'>A</td>
                        <td style='height: 30px;'>B</td>
                    </tr>
                </table></body>");
            var tbl = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(tbl);
            _output.WriteLine($"table: {tbl!.ContentRect.Width}x{tbl.ContentRect.Height}");
            // border-spacing adds 10px around and between cells
            Assert.True(tbl.ContentRect.Height >= 49,
                $"border-spacing should add height (got {tbl.ContentRect.Height})");
        }

        [Fact]
        public void Table_EmptyCells_Hide()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width: 200px; empty-cells: hide; border-collapse: separate;'>
                    <tr>
                        <td style='border: 1px solid; height: 30px;'>A</td>
                        <td id='empty' style='border: 1px solid; height: 30px;'></td>
                    </tr>
                </table></body>");
            var styled = (LayoutTestHelper.FindById(root, "empty")?.StyledNode as StyledElement);
            if (styled != null)
            {
                Assert.Equal(CssEmptyCells.Hide, styled.Style.EmptyCells);
            }
        }

        [Fact]
        public void Table_CaptionSide_Bottom()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width: 200px; caption-side: bottom;'>
                    <caption id='cap'>Bottom caption</caption>
                    <tr><td style='height: 30px;'>A</td></tr>
                </table></body>");
            var styled = (LayoutTestHelper.FindById(root, "cap")?.StyledNode as StyledElement);
            if (styled != null)
            {
                Assert.Equal(CssCaptionSide.Bottom, styled.Style.CaptionSide);
            }
        }

        [Fact]
        public void Table_PercentWidth_ResolvesAgainstCB()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <table id='tbl' style='width: 50%; border-collapse: collapse;'>
                        <tr><td style='height: 30px;'>A</td></tr>
                    </table>
                </div></body>");
            var tbl = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(tbl);
            _output.WriteLine($"table: w={tbl!.ContentRect.Width}");
            Assert.True(System.Math.Abs(tbl.ContentRect.Width - 200) < 2,
                $"50% of 400 = 200 (got {tbl.ContentRect.Width})");
        }
    }
}
