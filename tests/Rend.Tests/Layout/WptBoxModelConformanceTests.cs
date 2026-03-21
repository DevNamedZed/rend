using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// WPT-style conformance tests for the CSS box model: margin, padding, border, box-sizing,
    /// width/height auto/percentage, min/max constraints, and margin collapsing.
    /// </summary>
    public class WptBoxModelConformanceTests
    {
        private readonly ITestOutputHelper _output;

        public WptBoxModelConformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 8.4] padding-left pushes content area right
        [Fact]
        public void PaddingLeft_PushesContentRight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='padding-left:25px;width:100px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.X={box.ContentRect.X} PaddingLeft={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingLeft - 25) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.X - 25) < 2);
        }

        // [CSS2 8.4] padding-right reduces available content width for auto-width element
        [Fact]
        public void PaddingRight_ReducesContentWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='padding-right:50px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} PaddingRight={box.PaddingRight}");
            Assert.True(System.Math.Abs(box.PaddingRight - 50) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Width - 350) < 2);
        }

        // [CSS2 8.4] padding-top pushes content area down
        [Fact]
        public void PaddingTop_PushesContentDown()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='padding-top:30px;width:100px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Y={box.ContentRect.Y} PaddingTop={box.PaddingTop}");
            Assert.True(System.Math.Abs(box.PaddingTop - 30) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Y - 30) < 2);
        }

        // [CSS2 8.4] padding-bottom increases element total height
        [Fact]
        public void PaddingBottom_AddsToElementHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='padding-bottom:40px;width:100px;height:20px'></div>
                    <div id='next' style='height:10px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var next = LayoutTestHelper.FindById(root, "next")!;
            _output.WriteLine($"PaddingBottom={box.PaddingBottom} next.Y={next.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.PaddingBottom - 40) < 2);
            // next element Y should be height(20) + padding-bottom(40) = 60
            Assert.True(System.Math.Abs(next.ContentRect.Y - 60) < 2);
        }

        // [CSS2 8.4] padding shorthand with one value applies to all sides
        [Fact]
        public void PaddingShorthand_OneValue_AllSides()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='padding:15px;width:100px;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"top={box.PaddingTop} right={box.PaddingRight} bottom={box.PaddingBottom} left={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingTop - 15) < 2);
            Assert.True(System.Math.Abs(box.PaddingRight - 15) < 2);
            Assert.True(System.Math.Abs(box.PaddingBottom - 15) < 2);
            Assert.True(System.Math.Abs(box.PaddingLeft - 15) < 2);
        }

        // [CSS2 8.4] padding shorthand with two values: vertical horizontal
        [Fact]
        public void PaddingShorthand_TwoValues_VerticalHorizontal()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='padding:10px 25px;width:100px;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"top={box.PaddingTop} right={box.PaddingRight} bottom={box.PaddingBottom} left={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingTop - 10) < 2);
            Assert.True(System.Math.Abs(box.PaddingRight - 25) < 2);
            Assert.True(System.Math.Abs(box.PaddingBottom - 10) < 2);
            Assert.True(System.Math.Abs(box.PaddingLeft - 25) < 2);
        }

        // [CSS2 8.4] padding shorthand with four values: top right bottom left
        [Fact]
        public void PaddingShorthand_FourValues()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='padding:5px 10px 15px 20px;width:100px;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"top={box.PaddingTop} right={box.PaddingRight} bottom={box.PaddingBottom} left={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingTop - 5) < 2);
            Assert.True(System.Math.Abs(box.PaddingRight - 10) < 2);
            Assert.True(System.Math.Abs(box.PaddingBottom - 15) < 2);
            Assert.True(System.Math.Abs(box.PaddingLeft - 20) < 2);
        }

        // [CSS2 8.4] padding percentage resolves against containing block width
        [Fact]
        public void PaddingPercentage_ResolvesAgainstContainingBlockWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:300px'>
                    <div id='t' style='padding:10%;width:100px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"paddingTop={box.PaddingTop} paddingLeft={box.PaddingLeft}");
            // 10% of 300px = 30px on all sides
            Assert.True(System.Math.Abs(box.PaddingTop - 30) < 2);
            Assert.True(System.Math.Abs(box.PaddingRight - 30) < 2);
            Assert.True(System.Math.Abs(box.PaddingBottom - 30) < 2);
            Assert.True(System.Math.Abs(box.PaddingLeft - 30) < 2);
        }

        // [CSS2 8.5] border-width affects layout
        [Fact]
        public void BorderWidth_AffectsLayout()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='border:10px solid black;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"borderTop={box.BorderTopWidth} contentWidth={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.BorderTopWidth - 10) < 2);
            Assert.True(System.Math.Abs(box.BorderRightWidth - 10) < 2);
            Assert.True(System.Math.Abs(box.BorderBottomWidth - 10) < 2);
            Assert.True(System.Math.Abs(box.BorderLeftWidth - 10) < 2);
            // auto width = 400 - 2*10(border) = 380
            Assert.True(System.Math.Abs(box.ContentRect.Width - 380) < 2);
        }

        // [CSS2 8.5] border shorthand sets width, style, and color
        [Fact]
        public void BorderShorthand_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='border:5px solid red;width:100px;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"top={box.BorderTopWidth} right={box.BorderRightWidth} bottom={box.BorderBottomWidth} left={box.BorderLeftWidth}");
            Assert.True(System.Math.Abs(box.BorderTopWidth - 5) < 2);
            Assert.True(System.Math.Abs(box.BorderRightWidth - 5) < 2);
            Assert.True(System.Math.Abs(box.BorderBottomWidth - 5) < 2);
            Assert.True(System.Math.Abs(box.BorderLeftWidth - 5) < 2);
        }

        // [CSS2 8.5] individual border sides set independently
        [Fact]
        public void BorderSides_Independent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='border-top:2px solid black;border-right:4px solid black;border-bottom:6px solid black;border-left:8px solid black;width:100px;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"top={box.BorderTopWidth} right={box.BorderRightWidth} bottom={box.BorderBottomWidth} left={box.BorderLeftWidth}");
            Assert.True(System.Math.Abs(box.BorderTopWidth - 2) < 2);
            Assert.True(System.Math.Abs(box.BorderRightWidth - 4) < 2);
            Assert.True(System.Math.Abs(box.BorderBottomWidth - 6) < 2);
            Assert.True(System.Math.Abs(box.BorderLeftWidth - 8) < 2);
        }

        // [CSS-UI 3.2] box-sizing: content-box (default) - width is content only
        [Fact]
        public void BoxSizing_ContentBox_Default()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:200px;padding:20px;border:5px solid black;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                                   + box.BorderLeftWidth + box.BorderRightWidth;
            _output.WriteLine($"contentWidth={box.ContentRect.Width} borderBoxWidth={borderBoxWidth}");
            // content-box: content is 200px, border box = 200 + 40 + 10 = 250
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(borderBoxWidth - 250) < 2);
        }

        // [CSS-UI 3.2] box-sizing: border-box - width includes padding+border
        [Fact]
        public void BoxSizing_BorderBox_WidthIncludesPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;padding:20px;border:5px solid black;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                                   + box.BorderLeftWidth + box.BorderRightWidth;
            _output.WriteLine($"contentWidth={box.ContentRect.Width} borderBoxWidth={borderBoxWidth}");
            // border-box: border box = 200px, content = 200 - 40(pad) - 10(border) = 150
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(borderBoxWidth - 200) < 2);
        }

        // [CSS-UI 3.2] box-sizing: border-box with padding only (no border)
        [Fact]
        public void BoxSizing_BorderBox_WithPaddingOnly()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;padding:30px;height:80px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentWidth={box.ContentRect.Width} contentHeight={box.ContentRect.Height}");
            // content = 200 - 60 = 140 wide, 80 - 60 = 20 tall
            Assert.True(System.Math.Abs(box.ContentRect.Width - 140) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 20) < 2);
        }

        // [CSS-UI 3.2] box-sizing: border-box with both padding and border
        [Fact]
        public void BoxSizing_BorderBox_WithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:300px;padding:15px;border:10px solid black;height:100px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentWidth={box.ContentRect.Width} contentHeight={box.ContentRect.Height}");
            // content = 300 - 30(pad) - 20(border) = 250 wide
            // content height = 100 - 30(pad) - 20(border) = 50
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2);
        }

        // [CSS2 8.3] margin-left shifts element right
        [Fact]
        public void MarginLeft_ShiftsRight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='margin-left:50px;width:100px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X} MarginLeft={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginLeft - 50) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.X - 50) < 2);
        }

        // [CSS2 8.3] margin-right on block element does not affect its own position
        [Fact]
        public void MarginRight_DoesNotAffectOwnPosition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='margin-right:50px;width:100px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X} MarginRight={box.MarginRight}");
            Assert.True(System.Math.Abs(box.MarginRight - 50) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.X - 0) < 2);
        }

        // [CSS2 8.3] margin-top shifts element down
        [Fact]
        public void MarginTop_ShiftsDown()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'><div style='width:400px;overflow:hidden'>
                    <div id='t' style='margin-top:40px;width:100px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y} MarginTop={box.MarginTop}");
            Assert.True(System.Math.Abs(box.MarginTop - 40) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Y - 40) < 2);
        }

        // [CSS2 8.3] margin-bottom creates space after element
        [Fact]
        public void MarginBottom_CreatesSpaceAfter()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div style='margin-bottom:35px;height:20px'></div>
                    <div id='t' style='height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y}");
            // Y = 20 (height of first div) + 35 (margin-bottom) = 55
            Assert.True(System.Math.Abs(box.ContentRect.Y - 55) < 2);
        }

        // [CSS2 8.3.1] margin:auto centers block horizontally
        [Fact]
        public void MarginAuto_CentersHorizontally()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:200px;margin:0 auto;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X} MarginLeft={box.MarginLeft} MarginRight={box.MarginRight}");
            // (400 - 200) / 2 = 100
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 2);
        }

        // [CSS2 8.3.1] margin-left:auto pushes element to right
        [Fact]
        public void MarginLeftAuto_PushesToRight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:150px;margin-left:auto;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X}");
            // margin-left absorbs: 400 - 150 = 250
            Assert.True(System.Math.Abs(box.ContentRect.X - 250) < 2);
        }

        // [CSS2 8.3.1] margin-right:auto pushes element to left (element stays at X=0)
        [Fact]
        public void MarginRightAuto_PushesToLeft()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:150px;margin-right:auto;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X}");
            // margin-right:auto absorbs remaining space, element stays at left
            Assert.True(System.Math.Abs(box.ContentRect.X - 0) < 2);
        }

        // [CSS2 8.3.1] margin collapsing between adjacent siblings
        [Fact]
        public void MarginCollapsing_AdjacentSiblings()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div style='margin-bottom:30px;height:40px'></div>
                    <div id='t' style='margin-top:20px;height:30px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y}");
            // collapsed margin = max(30, 20) = 30, Y = 40 + 30 = 70
            Assert.True(System.Math.Abs(box.ContentRect.Y - 70) < 2);
        }

        // [CSS2 8.3.1] margin collapsing between parent and first child (no padding/border)
        [Fact]
        public void MarginCollapsing_ParentChild_NoPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='parent' style='margin-top:20px'>
                      <div id='child' style='margin-top:30px;height:40px'></div>
                    </div>
                  </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // parent margin 20 + child margin 30 collapse to max(20,30) = 30
            // parent.Y = 30, child.Y = 30 (collapsed through, same as parent)
            Assert.True(System.Math.Abs(parent.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Y - 30) < 2);
        }

        // [CSS2 8.3.1] no margin collapsing when parent has padding
        [Fact]
        public void NoMarginCollapsing_WithPaddingOnParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'><div style='width:400px;overflow:hidden'>
                    <div id='parent' style='padding-top:1px;margin-top:20px'>
                      <div id='child' style='margin-top:30px;height:40px'></div>
                    </div>
                  </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // padding prevents collapse: parent at Y=20+1(pad)=21 content, child at Y=21+30=51
            Assert.True(child.ContentRect.Y > parent.ContentRect.Y + 25);
        }

        // [CSS2 8.3.1] no margin collapsing when parent has border
        [Fact]
        public void NoMarginCollapsing_WithBorderOnParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'><div style='width:400px;overflow:hidden'>
                    <div id='parent' style='border-top:1px solid black;margin-top:20px'>
                      <div id='child' style='margin-top:30px;height:40px'></div>
                    </div>
                  </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // border prevents collapse: child.Y should be well above parent.Y + child margin
            Assert.True(child.ContentRect.Y > parent.ContentRect.Y + 25);
        }

        // [CSS2 8.3.1] no margin collapsing with overflow:hidden (establishes BFC)
        [Fact]
        public void NoMarginCollapsing_WithOverflowHidden()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='parent' style='overflow:hidden;margin-top:10px'>
                      <div id='child' style='margin-top:30px;height:40px'></div>
                    </div>
                  </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.Y={parent.ContentRect.Y} child.Y={child.ContentRect.Y}");
            // overflow:hidden establishes BFC, prevents margin collapse
            // parent at Y=10, child at Y=10+30=40
            Assert.True(child.ContentRect.Y > parent.ContentRect.Y + 25);
        }

        // [CSS2 8.3] negative margin-top pulls element up
        [Fact]
        public void NegativeMarginTop_PullsUp()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div style='height:60px'></div>
                    <div id='t' style='margin-top:-20px;height:30px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y}");
            // Y = 60 - 20 = 40
            Assert.True(System.Math.Abs(box.ContentRect.Y - 40) < 2);
        }

        // [CSS2 8.3] negative margin-left pulls element left
        [Fact]
        public void NegativeMarginLeft_PullsLeft()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='margin-left:-15px;width:100px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - (-15)) < 2);
        }

        // [CSS2 10.3.3] width:auto fills available space
        [Fact]
        public void WidthAuto_FillsAvailableSpace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:350px'>
                    <div id='t' style='height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 350) < 2);
        }

        // [CSS2 10.3.3] width as percentage of containing block
        [Fact]
        public void Width50Percent_HalfOfContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:50%;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS2 10.6.3] height:auto determined by content
        [Fact]
        public void HeightAuto_FromContent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t'>
                      <div style='height:45px'></div>
                      <div style='height:35px'></div>
                    </div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            // auto height = 45 + 35 = 80
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        // [CSS2 10.4] min-width prevents shrinking below minimum
        [Fact]
        public void MinWidth_PreventsShrinking()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:100px'>
                    <div id='t' style='width:50px;min-width:120px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 119);
        }

        // [CSS2 10.4] max-width prevents growing beyond maximum
        [Fact]
        public void MaxWidth_PreventsGrowing()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='max-width:150px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 151);
        }

        // [CSS2 10.7] min-height ensures minimum height
        [Fact]
        public void MinHeight_EnsuresMinimum()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;min-height:80px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height >= 79);
        }

        // [CSS2 10.7] max-height limits expansion
        [Fact]
        public void MaxHeight_LimitsExpansion()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;max-height:50px'>
                    <div style='height:200px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height <= 51);
        }

        // [CSS2 8] content-box total = width + padding + border (margin outside)
        [Fact]
        public void ContentBox_TotalComposition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:160px;padding:15px;border:5px solid black;height:80px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float totalWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                               + box.BorderLeftWidth + box.BorderRightWidth;
            float totalHeight = box.ContentRect.Height + box.PaddingTop + box.PaddingBottom
                                + box.BorderTopWidth + box.BorderBottomWidth;
            _output.WriteLine($"contentW={box.ContentRect.Width} totalW={totalWidth} contentH={box.ContentRect.Height} totalH={totalHeight}");
            // content=160, padding=30, border=10 => total=200
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2);
            Assert.True(System.Math.Abs(totalWidth - 200) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(totalHeight - 120) < 2);
        }

        // [CSS2 8.3] margin percentage resolves against containing block width (even for vertical)
        [Fact]
        public void MarginPercentage_ResolvesAgainstContainingBlockWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'><div style='width:200px;overflow:hidden'>
                    <div id='t' style='margin:10%;width:100px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"marginTop={box.MarginTop} marginLeft={box.MarginLeft} marginRight={box.MarginRight} marginBottom={box.MarginBottom}");
            // 10% of 200px = 20px on all sides
            Assert.True(System.Math.Abs(box.MarginTop - 20) < 2);
            Assert.True(System.Math.Abs(box.MarginRight - 20) < 2);
            Assert.True(System.Math.Abs(box.MarginBottom - 20) < 2);
            Assert.True(System.Math.Abs(box.MarginLeft - 20) < 2);
        }

        // [CSS2 10.3.3] auto width with padding and border subtracts from available
        [Fact]
        public void AutoWidth_SubtractsPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='padding:20px;border:5px solid black;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Width={box.ContentRect.Width}");
            // auto width = 400 - 40(padding) - 10(border) = 350
            Assert.True(System.Math.Abs(box.ContentRect.Width - 350) < 2);
        }

        // [CSS2 10.3.3] auto width with margin, padding, and border
        [Fact]
        public void AutoWidth_SubtractsMarginPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='margin:0 20px;padding:0 10px;border-left:3px solid;border-right:7px solid;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Width={box.ContentRect.Width}");
            // auto width = 400 - 40(margin) - 20(padding) - 10(border) = 330
            Assert.True(System.Math.Abs(box.ContentRect.Width - 330) < 2);
        }

        // [CSS-UI 3.2] border-box: height includes padding and border
        [Fact]
        public void BoxSizing_BorderBox_HeightIncludesPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;height:100px;padding:10px;border:5px solid black'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxHeight = box.ContentRect.Height + box.PaddingTop + box.PaddingBottom
                                    + box.BorderTopWidth + box.BorderBottomWidth;
            _output.WriteLine($"contentHeight={box.ContentRect.Height} borderBoxHeight={borderBoxHeight}");
            // border-box height = 100, content = 100 - 20(pad) - 10(border) = 70
            Assert.True(System.Math.Abs(box.ContentRect.Height - 70) < 2);
            Assert.True(System.Math.Abs(borderBoxHeight - 100) < 2);
        }

        // [CSS2 8.3] margin collapsing: larger margin wins
        [Fact]
        public void MarginCollapsing_LargerMarginWins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div style='margin-bottom:10px;height:20px'></div>
                    <div id='t' style='margin-top:50px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y}");
            // collapsed margin = max(10, 50) = 50, Y = 20 + 50 = 70
            Assert.True(System.Math.Abs(box.ContentRect.Y - 70) < 2);
        }

        // [CSS2 8.3.1] equal margins collapse to single margin
        [Fact]
        public void MarginCollapsing_EqualMargins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div style='margin-bottom:25px;height:30px'></div>
                    <div id='t' style='margin-top:25px;height:30px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y}");
            // collapsed margin = max(25, 25) = 25, Y = 30 + 25 = 55
            Assert.True(System.Math.Abs(box.ContentRect.Y - 55) < 2);
        }

        // [CSS2 8.5.1] border-width computes to 0 when border-style is none
        [Fact]
        public void BorderWidthZero_WhenStyleNone()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='border:10px none red;width:100px;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"borderTop={box.BorderTopWidth}");
            Assert.True(System.Math.Abs(box.BorderTopWidth - 0) < 1);
            Assert.True(System.Math.Abs(box.BorderRightWidth - 0) < 1);
        }

        // [CSS2 8.3] margin:auto with fixed width in fixed container
        [Fact]
        public void MarginAuto_BothSides_EqualDistribution()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:120px;margin:0 auto;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X} MarginLeft={box.MarginLeft} MarginRight={box.MarginRight}");
            // remaining = 400 - 120 = 280, each side = 140
            Assert.True(System.Math.Abs(box.ContentRect.X - 140) < 2);
            Assert.True(System.Math.Abs(box.MarginLeft - 140) < 2);
            Assert.True(System.Math.Abs(box.MarginRight - 140) < 2);
        }

        // [CSS2 10.3.3] auto width fills container minus margin
        [Fact]
        public void AutoWidth_FillsContainerMinusMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:300px'>
                    <div id='t' style='margin-left:30px;margin-right:20px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Width={box.ContentRect.Width} X={box.ContentRect.X}");
            // auto width = 300 - 30 - 20 = 250
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.X - 30) < 2);
        }

        // [CSS2 8.4] padding on all sides with auto width
        [Fact]
        public void PaddingAllSides_AutoWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='padding:20px;height:30px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Width={box.ContentRect.Width} X={box.ContentRect.X}");
            // auto width = 400 - 40(padding) = 360
            Assert.True(System.Math.Abs(box.ContentRect.Width - 360) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.X - 20) < 2);
        }

        // [CSS2 8.5] border only on one side
        [Fact]
        public void BorderOneSide_AffectsContentPosition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='border-left:8px solid black;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X} Width={box.ContentRect.Width} BorderLeft={box.BorderLeftWidth}");
            Assert.True(System.Math.Abs(box.BorderLeftWidth - 8) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.X - 8) < 2);
            // auto width = 400 - 8 = 392
            Assert.True(System.Math.Abs(box.ContentRect.Width - 392) < 2);
        }

        // [CSS2 10.4] min-width with percentage
        [Fact]
        public void MinWidth_Percentage()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:50px;min-width:25%;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Width={box.ContentRect.Width}");
            // min-width 25% of 400 = 100, width:50px < 100 => use 100
            Assert.True(box.ContentRect.Width >= 99);
        }

        // [CSS2 10.4] max-width with percentage
        [Fact]
        public void MaxWidth_Percentage()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:300px;max-width:50%;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Width={box.ContentRect.Width}");
            // max-width 50% of 400 = 200, width:300px > 200 => use 200
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS-UI 3.2] border-box with min-width: min applies to border box
        [Fact]
        public void BoxSizing_BorderBox_MinWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:50px;min-width:200px;padding:20px;border:5px solid black;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                                   + box.BorderLeftWidth + box.BorderRightWidth;
            _output.WriteLine($"contentWidth={box.ContentRect.Width} borderBoxWidth={borderBoxWidth}");
            // min-width:200px > width:50px, so border-box = 200
            // content = 200 - 40 - 10 = 150
            Assert.True(System.Math.Abs(borderBoxWidth - 200) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2);
        }

        // [CSS-UI 3.2] border-box with max-height: max applies to border box
        [Fact]
        public void BoxSizing_BorderBox_MaxHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;height:300px;max-height:100px;padding:10px;border:5px solid black'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxHeight = box.ContentRect.Height + box.PaddingTop + box.PaddingBottom
                                    + box.BorderTopWidth + box.BorderBottomWidth;
            _output.WriteLine($"contentHeight={box.ContentRect.Height} borderBoxHeight={borderBoxHeight}");
            // max-height:100px < height:300px, so border-box = 100
            // content = 100 - 20 - 10 = 70
            Assert.True(System.Math.Abs(borderBoxHeight - 100) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 70) < 2);
        }

        // [CSS2 8] padding percentage on vertical axis also resolves against width
        [Fact]
        public void PaddingPercentage_VerticalAlsoUsesWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:250px'>
                    <div id='t' style='padding-top:20%;padding-bottom:10%;width:100px;height:0'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"paddingTop={box.PaddingTop} paddingBottom={box.PaddingBottom}");
            // 20% of 250 = 50, 10% of 250 = 25
            Assert.True(System.Math.Abs(box.PaddingTop - 50) < 2);
            Assert.True(System.Math.Abs(box.PaddingBottom - 25) < 2);
        }

        // [CSS2 10.3.3] width percentage with padding and border in content-box
        [Fact]
        public void WidthPercentage_WithPaddingBorder_ContentBox()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:50%;padding:10px;border:5px solid black;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                                   + box.BorderLeftWidth + box.BorderRightWidth;
            _output.WriteLine($"contentWidth={box.ContentRect.Width} borderBoxWidth={borderBoxWidth}");
            // content-box: content width = 50% of 400 = 200
            // border box = 200 + 20 + 10 = 230
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(borderBoxWidth - 230) < 2);
        }

        // [CSS2 10.3.3] width percentage with padding in border-box
        [Fact]
        public void WidthPercentage_WithPadding_BorderBox()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='box-sizing:border-box;width:50%;padding:10px;border:5px solid black;height:40px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                                   + box.BorderLeftWidth + box.BorderRightWidth;
            _output.WriteLine($"contentWidth={box.ContentRect.Width} borderBoxWidth={borderBoxWidth}");
            // border-box: border-box width = 50% of 400 = 200
            // content = 200 - 20(pad) - 10(border) = 170
            Assert.True(System.Math.Abs(borderBoxWidth - 200) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Width - 170) < 2);
        }

        // [CSS2 8.3] negative margin increases auto width
        [Fact]
        public void NegativeMargin_IncreasesAutoWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:300px'>
                    <div id='t' style='margin-left:-20px;margin-right:-30px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Width={box.ContentRect.Width}");
            // auto width = 300 - (-20) - (-30) = 300 + 20 + 30 = 350
            Assert.True(System.Math.Abs(box.ContentRect.Width - 350) < 2);
        }

        // [CSS2 8] sibling positioning with margin, padding, border
        [Fact]
        public void Sibling_PositionedAfterMarginPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div style='height:30px;padding-bottom:10px;border-bottom:5px solid black;margin-bottom:15px'></div>
                    <div id='t' style='height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y}");
            // Y = 30(content) + 10(padding-bottom) + 5(border-bottom) + 15(margin-bottom) = 60
            Assert.True(System.Math.Abs(box.ContentRect.Y - 60) < 2);
        }

        // [CSS2 8.5] border: thin medium thick keyword widths
        [Fact]
        public void BorderKeywordWidths()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='border-top:thin solid;border-right:medium solid;border-bottom:thick solid;border-left:1px solid;width:100px;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"top={box.BorderTopWidth} right={box.BorderRightWidth} bottom={box.BorderBottomWidth} left={box.BorderLeftWidth}");
            Assert.True(System.Math.Abs(box.BorderTopWidth - 1) < 1);
            Assert.True(System.Math.Abs(box.BorderRightWidth - 3) < 1);
            Assert.True(System.Math.Abs(box.BorderBottomWidth - 5) < 1);
            Assert.True(System.Math.Abs(box.BorderLeftWidth - 1) < 1);
        }

        // [CSS2 10.6.3] auto height with nested padding children
        [Fact]
        public void AutoHeight_IncludesChildPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t'>
                      <div style='padding:10px;height:20px'></div>
                    </div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            // auto height = child content(20) + child padding-top(10) + child padding-bottom(10) = 40
            Assert.True(System.Math.Abs(box.ContentRect.Height - 40) < 2);
        }

        // [CSS2 8.4] padding shorthand with three values: top horizontal bottom
        [Fact]
        public void PaddingShorthand_ThreeValues()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='padding:5px 15px 25px;width:100px;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"top={box.PaddingTop} right={box.PaddingRight} bottom={box.PaddingBottom} left={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingTop - 5) < 2);
            Assert.True(System.Math.Abs(box.PaddingRight - 15) < 2);
            Assert.True(System.Math.Abs(box.PaddingBottom - 25) < 2);
            Assert.True(System.Math.Abs(box.PaddingLeft - 15) < 2);
        }

        // [CSS2 8.3] margin shorthand with two values: vertical horizontal
        [Fact]
        public void MarginShorthand_TwoValues()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'><div style='width:400px;overflow:hidden'>
                    <div id='t' style='margin:10px 30px;width:100px;height:20px'></div>
                  </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"top={box.MarginTop} right={box.MarginRight} bottom={box.MarginBottom} left={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginTop - 10) < 2);
            Assert.True(System.Math.Abs(box.MarginRight - 30) < 2);
            Assert.True(System.Math.Abs(box.MarginBottom - 10) < 2);
            Assert.True(System.Math.Abs(box.MarginLeft - 30) < 2);
        }
    }
}
