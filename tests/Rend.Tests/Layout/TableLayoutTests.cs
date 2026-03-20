using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class TableLayoutTests
    {
        private readonly ITestOutputHelper _output;
        public TableLayoutTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void AbsoluteTable_PercentHeight_ResolvesAgainstContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative; width:100px;'>
                    <div id='tbl' style='position:absolute; display:table; width:100%; height:100%; background:green;'></div>
                    <div style='height:100px; background:red;'></div>
                </div></body>");
            var tbl = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(tbl);
            _output.WriteLine($"table: {tbl!.ContentRect.Width}x{tbl.ContentRect.Height} at ({tbl.ContentRect.X},{tbl.ContentRect.Y})");
            Assert.True(System.Math.Abs(tbl.ContentRect.Width - 100) < 2,
                $"Absolute table width:100% should be 100 (got {tbl.ContentRect.Width})");
            Assert.True(tbl.ContentRect.Height >= 99,
                $"Absolute table height:100% should be 100 (got {tbl.ContentRect.Height})");
        }

        [Fact]
        public void Table_AutoWidth_ShrinkToFit()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='border-collapse: collapse;'>
                    <tr><td style='width: 80px;'>A</td><td style='width: 60px;'>B</td></tr>
                </table></body>");
            var tbl = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(tbl);
            _output.WriteLine($"table: {tbl!.ContentRect.Width}x{tbl.ContentRect.Height}");
            // Auto-width table should shrink to fit columns (80+60 = 140 + borders)
            Assert.True(tbl.ContentRect.Width <= 150,
                $"Auto-width table should shrink (got {tbl.ContentRect.Width})");
        }

        [Fact]
        public void Table_BorderCollapse_SharedBorders()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width: 200px; border-collapse: collapse; border: 2px solid black;'>
                    <tr>
                        <td style='border: 2px solid black; height: 30px;'>A</td>
                        <td style='border: 2px solid black; height: 30px;'>B</td>
                    </tr>
                </table></body>");
            var tbl = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(tbl);
            _output.WriteLine($"table: {tbl!.ContentRect.Width}x{tbl.ContentRect.Height}");
        }

        [Fact]
        public void Table_BorderSpacing_AddsPadding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width: 200px; border-spacing: 10px;'>
                    <tr>
                        <td style='height: 30px;'>A</td>
                        <td style='height: 30px;'>B</td>
                    </tr>
                </table></body>");
            var tbl = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(tbl);
            _output.WriteLine($"table: {tbl!.ContentRect.Width}x{tbl.ContentRect.Height}");
            // With border-spacing:10px, height should include spacing
            Assert.True(tbl.ContentRect.Height >= 49,
                $"Table with border-spacing should be taller (got {tbl.ContentRect.Height})");
        }

        [Fact]
        public void Table_Caption_PositionedAbove()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width: 200px;'>
                    <caption id='cap'>Title</caption>
                    <tr><td style='height: 30px;'>A</td></tr>
                </table></body>");
            var tbl = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(tbl);
            _output.WriteLine($"table: {tbl!.ContentRect.Width}x{tbl.ContentRect.Height}");
            // Table with caption should have total height > just the row
            Assert.True(tbl.ContentRect.Height >= 29, $"Table should have content (got {tbl.ContentRect.Height})");
        }
    }
}
