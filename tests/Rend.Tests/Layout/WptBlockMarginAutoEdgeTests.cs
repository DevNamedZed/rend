using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS2 §10.3.3 block-level margin:auto resolution.
    /// Covers horizontal centering, single-side auto, various widths/containers,
    /// interactions with padding/border/box-sizing/min-max, and auto margin behavior
    /// in different formatting contexts (block, flex, grid).
    /// </summary>
    public class WptBlockMarginAutoEdgeTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockMarginAutoEdgeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.3.3] margin-left:auto + margin-right:auto centers block
        [Fact]
        public void MarginAuto_BothSides_CentersBlock()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:200px;margin-left:auto;margin-right:auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 1,
                $"margin:auto should center block at X=100 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] margin-left:auto pushes block to the right
        [Fact]
        public void MarginLeftAuto_RightAligns()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:100px;margin-left:auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 300) < 1,
                $"margin-left:auto should right-align at X=300 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] margin-right:auto keeps block at the left
        [Fact]
        public void MarginRightAuto_LeftAligns()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:100px;margin-right:auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X) < 1,
                $"margin-right:auto should left-align at X=0 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] auto with 50% width
        [Fact]
        public void MarginAuto_Width50Percent_Centers()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:50%;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1,
                $"width:50% of 400 should be 200 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 1,
                $"margin:auto with 50% width should center at X=100 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] auto with 25% width
        [Fact]
        public void MarginAuto_Width25Percent_Centers()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:25%;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 1,
                $"width:25% of 400 should be 100 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.X - 150) < 1,
                $"margin:auto with 25% width should center at X=150 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] auto with 75% width
        [Fact]
        public void MarginAuto_Width75Percent_Centers()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:75%;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 1,
                $"width:75% of 400 should be 300 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.X - 50) < 1,
                $"margin:auto with 75% width should center at X=50 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] auto in 200px container
        [Fact]
        public void MarginAuto_Container200px_Centers()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='width:100px;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 50) < 1,
                $"margin:auto in 200px container should center at X=50 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] auto in 300px container
        [Fact]
        public void MarginAuto_Container300px_Centers()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='width:100px;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 1,
                $"margin:auto in 300px container should center at X=100 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] auto in 500px container
        [Fact]
        public void MarginAuto_Container500px_Centers()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:500px'>
                    <div id='t' style='width:100px;margin:0 auto;height:20px'></div>
                </div></body>", 600);
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 200) < 1,
                $"margin:auto in 500px container should center at X=200 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] auto with padding on centered element
        [Fact]
        public void MarginAuto_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:200px;padding:0 20px;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            float totalBoxWidth = box!.ContentRect.Width + box.PaddingLeft + box.PaddingRight;
            _output.WriteLine($"x={box.ContentRect.X} contentW={box.ContentRect.Width} paddingL={box.PaddingLeft} paddingR={box.PaddingRight} totalBox={totalBoxWidth}");
            float expectedMargin = (400 - totalBoxWidth) / 2;
            Assert.True(System.Math.Abs(box.ContentRect.X - (expectedMargin + box.PaddingLeft)) < 1,
                $"margin:auto with padding should account for padding in centering (got X={box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] auto with border on centered element
        [Fact]
        public void MarginAuto_WithBorder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:200px;border:10px solid black;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            float totalBoxWidth = box!.ContentRect.Width + box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth;
            _output.WriteLine($"x={box.ContentRect.X} contentW={box.ContentRect.Width} borderL={box.BorderLeftWidth} totalBox={totalBoxWidth}");
            float expectedMargin = (400 - totalBoxWidth) / 2;
            float expectedContentX = expectedMargin + box.BorderLeftWidth;
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedContentX) < 1,
                $"margin:auto with border should center border-box (expected X={expectedContentX}, got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] auto with box-sizing:border-box
        [Fact]
        public void MarginAuto_WithBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:200px;box-sizing:border-box;padding:0 10px;border:5px solid black;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            float borderBoxWidth = box!.ContentRect.Width + box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth;
            _output.WriteLine($"x={box.ContentRect.X} contentW={box.ContentRect.Width} borderBox={borderBoxWidth}");
            Assert.True(System.Math.Abs(borderBoxWidth - 200) < 1,
                $"border-box width should be 200 (got {borderBoxWidth})");
            float expectedMargin = (400 - 200) / 2;
            float expectedContentX = expectedMargin + box.BorderLeftWidth + box.PaddingLeft;
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedContentX) < 1,
                $"margin:auto with border-box should center at X={expectedContentX} (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] auto with min-width preventing shrink
        [Fact]
        public void MarginAuto_WithMinWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:100px;min-width:200px;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 199,
                $"min-width:200px should override width:100px (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 1,
                $"margin:auto with min-width should center at X=100 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] auto with max-width clamping
        [Fact]
        public void MarginAuto_WithMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:300px;max-width:200px;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 201,
                $"max-width:200px should clamp width:300px (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 1,
                $"margin:auto with max-width should center at X=100 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] auto with no explicit width has no centering effect
        [Fact]
        public void MarginAuto_NoWidth_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 1,
                $"auto width should fill container at 400px (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.X) < 1,
                $"auto width block should start at X=0 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] when element is wider than container, auto margins become 0
        [Fact]
        public void MarginAuto_WiderThanContainer_MarginsZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='width:300px;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 1,
                $"width should remain 300px (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.X) < 1,
                $"over-constrained auto margins should resolve to 0 on left (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] nested centering: outer centered, inner centered within outer
        [Fact]
        public void MarginAuto_Nested_BothCentered()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='outer' style='width:300px;margin:0 auto;height:100px'>
                        <div id='inner' style='width:100px;margin:0 auto;height:20px'></div>
                    </div>
                </div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer");
            var inner = LayoutTestHelper.FindById(root, "inner");
            Assert.NotNull(outer);
            Assert.NotNull(inner);
            _output.WriteLine($"outer.x={outer!.ContentRect.X} inner.x={inner!.ContentRect.X}");
            Assert.True(System.Math.Abs(outer.ContentRect.X - 50) < 1,
                $"outer should center at X=50 (got {outer.ContentRect.X})");
            float innerExpectedX = 50 + (300 - 100) / 2;
            Assert.True(System.Math.Abs(inner.ContentRect.X - innerExpectedX) < 1,
                $"inner should center within outer at X={innerExpectedX} (got {inner.ContentRect.X})");
        }

        // [CSS2 §10.3.3] margin:auto in overflow:hidden container (establishes BFC)
        [Fact]
        public void MarginAuto_InOverflowHidden()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px;overflow:hidden'>
                    <div id='t' style='width:200px;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 1,
                $"margin:auto in overflow:hidden should center at X=100 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] margin:0 auto shorthand
        [Fact]
        public void Margin0Auto_CentersBlock()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:200px;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} marginTop={box.MarginTop}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 1,
                $"margin:0 auto should center at X=100 (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.MarginTop) < 1,
                $"margin:0 auto should set top margin to 0 (got {box.MarginTop})");
        }

        // [CSS2 §10.3.3] margin:10px auto with vertical margin and horizontal auto
        [Fact]
        public void Margin10pxAuto_CentersWithVerticalMargin()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:200px;margin:10px auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} y={box.ContentRect.Y} marginTop={box.MarginTop}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 1,
                $"margin:10px auto should center horizontally at X=100 (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 10) < 1,
                $"margin:10px auto should offset vertically by 10px (got {box.ContentRect.Y})");
        }

        // [CSS2 §10.6.3] margin-top:auto in block flow resolves to 0
        [Fact]
        public void MarginTopAuto_InBlockFlow_ResolvesToZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px;height:200px'>
                    <div style='height:50px'></div>
                    <div id='t' style='width:100px;margin-top:auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"y={box!.ContentRect.Y} marginTop={box.MarginTop}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 50) < 1,
                $"margin-top:auto in block should resolve to 0, placing element at Y=50 (got {box.ContentRect.Y})");
        }

        // [CSS2 §10.6.3] margin-bottom:auto in block flow resolves to 0
        [Fact]
        public void MarginBottomAuto_InBlockFlow_ResolvesToZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px;height:200px'>
                    <div id='t' style='width:100px;margin-bottom:auto;height:20px'></div>
                    <div id='sibling' style='width:100px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(box);
            Assert.NotNull(sibling);
            _output.WriteLine($"t.y={box!.ContentRect.Y} sibling.y={sibling!.ContentRect.Y}");
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 20) < 1,
                $"margin-bottom:auto in block resolves to 0, sibling at Y=20 (got {sibling.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.1] auto margins in flex cross axis absorb free space
        [Fact]
        public void MarginAuto_FlexCrossAxis_CentersVertically()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:400px;height:200px'>
                    <div id='t' style='width:100px;height:50px;margin-top:auto;margin-bottom:auto'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"y={box!.ContentRect.Y} height={box.ContentRect.Height}");
            float expectedY = (200 - 50) / 2;
            Assert.True(System.Math.Abs(box.ContentRect.Y - expectedY) < 2,
                $"auto margins on flex cross axis should center at Y={expectedY} (got {box.ContentRect.Y})");
        }

        // [CSS-GRID §11.1] auto margins in grid cell absorb free space
        [Fact]
        public void MarginAuto_GridCell_CentersHorizontally()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid;grid-template-columns:400px;width:400px'>
                    <div id='t' style='width:200px;margin-left:auto;margin-right:auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 2,
                $"auto margins in grid cell should center at X=100 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] margin-left:auto with margin-right:0 right-aligns
        [Fact]
        public void MarginLeftAuto_MarginRight0_RightAligns()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:150px;margin-left:auto;margin-right:0;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 250) < 1,
                $"margin-left:auto;margin-right:0 should right-align at X=250 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] margin-right:auto with margin-left:0 left-aligns
        [Fact]
        public void MarginRightAuto_MarginLeft0_LeftAligns()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:150px;margin-left:0;margin-right:auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X) < 1,
                $"margin-left:0;margin-right:auto should left-align at X=0 (got {box.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.1] margin:auto on flex main axis absorbs space
        [Fact]
        public void MarginAuto_FlexMainAxis_PushesRight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:400px;height:50px'>
                    <div style='width:50px;height:50px'></div>
                    <div id='t' style='width:50px;height:50px;margin-left:auto'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 350) < 2,
                $"margin-left:auto in flex should push to right edge at X=350 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] auto margins with both padding and border
        [Fact]
        public void MarginAuto_WithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:100px;padding:0 20px;border:10px solid black;margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            float totalBoxWidth = box!.ContentRect.Width + box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth;
            _output.WriteLine($"x={box.ContentRect.X} totalBox={totalBoxWidth}");
            float expectedMargin = (400 - totalBoxWidth) / 2;
            float expectedContentX = expectedMargin + box.BorderLeftWidth + box.PaddingLeft;
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedContentX) < 1,
                $"margin:auto with padding+border should center correctly (expected X={expectedContentX}, got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] margin-left:auto with fixed margin-right
        [Fact]
        public void MarginLeftAuto_FixedMarginRight_AbsorbsRemainder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:100px;margin-left:auto;margin-right:50px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 250) < 1,
                $"margin-left:auto with margin-right:50px should place at X=250 (got {box.ContentRect.X})");
        }
    }
}
