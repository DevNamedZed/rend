using Rend.Css;
using Rend.Layout.Internal;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class BoxModelLayoutTests
    {
        private readonly ITestOutputHelper _output;
        public BoxModelLayoutTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void MinHeight_OnBlock_Respected()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width: 100px; min-height: 80px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(box.ContentRect.Height >= 79, $"min-height should be respected (got {box.ContentRect.Height})");
        }

        [Fact]
        public void MaxHeight_OnBlock_Respected()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width: 100px; max-height: 50px;'>
                    <div style='height: 200px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(box.ContentRect.Height <= 51, $"max-height should be respected (got {box.ContentRect.Height})");
        }

        [Fact]
        public void MinWidth_OnBlock_Respected()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 50px;'>
                    <div id='test' style='min-width: 100px; height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            // Block fills container by default, but min-width can exceed container
            Assert.True(box.ContentRect.Width >= 99, $"min-width should be respected (got {box.ContentRect.Width})");
        }

        [Fact]
        public void MaxWidth_OnBlock_Respected()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='max-width: 100px; height: 20px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 101, $"max-width should be respected (got {box.ContentRect.Width})");
        }

        [Fact]
        public void BoxSizing_BorderBox_WidthIncludesPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width: 100px; height: 100px; box-sizing: border-box; padding: 10px; border: 5px solid black;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            float totalWidth = box!.ContentRect.Width + box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth;
            _output.WriteLine($"content={box.ContentRect.Width}x{box.ContentRect.Height} total={totalWidth}");
            Assert.True(System.Math.Abs(totalWidth - 100) < 1, $"border-box total width should be 100 (got {totalWidth})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 70) < 1, $"content width should be 70 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void OverflowHidden_EstablishesBfc()
        {
            // overflow: hidden should contain floats (establishes BFC)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='overflow: hidden; width: 200px;'>
                    <div style='float: left; width: 50px; height: 100px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(box.ContentRect.Height >= 99, $"overflow:hidden should contain float (got {box.ContentRect.Height})");
        }

        [Fact]
        public void DisplayInlineBlock_ShrinkToFit()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <span id='test' style='display: inline-block;'>
                        <div style='width: 80px; height: 30px;'></div>
                    </span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            // inline-block shrinks to fit content
            Assert.True(box.ContentRect.Width <= 81, $"inline-block should shrink-to-fit (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Position_Relative_OffsetsVisually()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='position: relative; top: 20px; left: 30px; width: 50px; height: 50px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} y={box.ContentRect.Y}");
            // Relative positioning offsets visually
            Assert.True(box.ContentRect.X >= 29, $"left:30px should offset X (got {box.ContentRect.X})");
            Assert.True(box.ContentRect.Y >= 19, $"top:20px should offset Y (got {box.ContentRect.Y})");
        }

        [Fact]
        public void Calc_Width_Resolves()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div id='test' style='width: calc(100% - 40px); height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2, $"calc(100% - 40px) of 200px should be 160 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Negative_Margin_Pulls()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div style='height: 50px;'></div>
                    <div id='test' style='margin-top: -20px; height: 30px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"y={box!.ContentRect.Y}");
            // Negative margin should pull the element up
            Assert.True(box.ContentRect.Y < 50, $"negative margin should pull up (Y={box.ContentRect.Y})");
        }

        [Fact]
        public void Float_Left_Positioned()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div id='float' style='float: left; width: 60px; height: 40px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "float");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} y={box.ContentRect.Y} w={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 59, $"Float width should be 60 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Float_Clear_MovesBelow()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div style='float: left; width: 60px; height: 40px;'></div>
                    <div id='cleared' style='clear: left; height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "cleared");
            Assert.NotNull(box);
            _output.WriteLine($"y={box!.ContentRect.Y}");
            Assert.True(box.ContentRect.Y >= 39, $"clear:left should move below float (Y={box.ContentRect.Y})");
        }

        [Fact]
        public void Table_BasicLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table id='tbl' style='width: 200px; border-collapse: collapse;'>
                    <tr>
                        <td style='width: 100px; height: 30px;'>A</td>
                        <td style='width: 100px; height: 30px;'>B</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"w={table!.ContentRect.Width} h={table.ContentRect.Height}");
            Assert.True(table.ContentRect.Width >= 199, $"Table width should be 200 (got {table.ContentRect.Width})");
            Assert.True(table.ContentRect.Height >= 29, $"Table should have height (got {table.ContentRect.Height})");
        }

        [Fact]
        public void ZIndex_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='position: relative; z-index: 5; width: 50px; height: 50px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            var styled = box!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            _output.WriteLine($"z-index={styled!.Style.ZIndex}");
            Assert.Equal(5, styled.Style.ZIndex);
        }

        [Fact]
        public void EmUnit_RelativeToFontSize()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size: 20px;'>
                    <div id='test' style='width: 10em; height: 2em;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width} h={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2, $"10em at 20px should be 200 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 40) < 2, $"2em at 20px should be 40 (got {box.ContentRect.Height})");
        }

        [Fact]
        public void RemUnit_RelativeToRoot()
        {
            var root = LayoutTestHelper.Layout(@"
                <html style='font-size: 20px;'>
                <body style='margin:0'>
                <div style='font-size: 10px;'>
                    <div id='test' style='width: 5rem; height: 2rem;'></div>
                </div></body></html>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width} h={box.ContentRect.Height}");
            // rem is relative to root (html), not parent
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2, $"5rem at root 20px should be 100 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void VwUnit_RelativeToViewport()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width: 50vw; height: 25vh;'></div></body>", 400, 300);
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width} h={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2, $"50vw of 400 should be 200 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 75) < 2, $"25vh of 300 should be 75 (got {box.ContentRect.Height})");
        }

        [Fact]
        public void PercentageWidth_ResolvesAgainstParent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div id='test' style='width: 50%; height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2, $"50% of 200 should be 100 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void MarginAuto_CentersBlock()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <div id='test' style='width: 200px; height: 20px; margin: 0 auto;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            // margin:auto should center: (400-200)/2 = 100px from left
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 2, $"margin:auto should center (X={box.ContentRect.X})");
        }
    }
}
