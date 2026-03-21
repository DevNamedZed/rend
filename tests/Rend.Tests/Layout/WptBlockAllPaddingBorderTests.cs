using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for padding and border effects on child positioning, auto width reduction,
    /// box-sizing interactions, outline non-effect, and margin collapse prevention.
    /// </summary>
    public class WptBlockAllPaddingBorderTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockAllPaddingBorderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §8.4] Uniform padding offsets child content area
        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        [InlineData(20)]
        [InlineData(25)]
        [InlineData(30)]
        [InlineData(40)]
        [InlineData(50)]
        public void Padding_Uniform_ChildXOffset(int padding)
        {
            var root = LayoutTestHelper.Layout(
                $"<body style='margin:0'><div style='padding:{padding}px;width:200px'>" +
                $"<div id='child' style='height:20px'></div></div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            Assert.True(System.Math.Abs(child.ContentRect.X - padding) < 1,
                $"Expected child X={padding}, got {child.ContentRect.X}");
        }

        // [CSS2 §8.4] padding-left offsets child X
        [Theory]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(30)]
        public void PaddingLeft_ChildXOffset(int paddingLeft)
        {
            var root = LayoutTestHelper.Layout(
                $"<body style='margin:0'><div style='padding-left:{paddingLeft}px;width:200px'>" +
                $"<div id='child' style='height:20px'></div></div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            Assert.True(System.Math.Abs(child.ContentRect.X - paddingLeft) < 1,
                $"Expected child X={paddingLeft}, got {child.ContentRect.X}");
        }

        // [CSS2 §8.4] padding-top offsets child Y
        [Theory]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(30)]
        public void PaddingTop_ChildYOffset(int paddingTop)
        {
            var root = LayoutTestHelper.Layout(
                $"<body style='margin:0'><div style='padding-top:{paddingTop}px;width:200px'>" +
                $"<div id='child' style='height:20px'></div></div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            Assert.True(System.Math.Abs(child.ContentRect.Y - paddingTop) < 1,
                $"Expected child Y={paddingTop}, got {child.ContentRect.Y}");
        }

        // [CSS2 §8.4] padding shorthand with 1 value sets all four sides
        [Fact]
        public void PaddingShorthand_OneValue_AllSidesEqual()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='padding:12px;width:100px;height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(12, target.PaddingTop);
            Assert.Equal(12, target.PaddingRight);
            Assert.Equal(12, target.PaddingBottom);
            Assert.Equal(12, target.PaddingLeft);
        }

        // [CSS2 §8.4] padding shorthand with 2 values: vertical | horizontal
        [Fact]
        public void PaddingShorthand_TwoValues_VerticalHorizontal()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='padding:8px 16px;width:100px;height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(8, target.PaddingTop);
            Assert.Equal(16, target.PaddingRight);
            Assert.Equal(8, target.PaddingBottom);
            Assert.Equal(16, target.PaddingLeft);
        }

        // [CSS2 §8.4] padding shorthand with 3 values: top | horizontal | bottom
        [Fact]
        public void PaddingShorthand_ThreeValues_TopHorizontalBottom()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='padding:5px 15px 25px;width:100px;height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(5, target.PaddingTop);
            Assert.Equal(15, target.PaddingRight);
            Assert.Equal(25, target.PaddingBottom);
            Assert.Equal(15, target.PaddingLeft);
        }

        // [CSS2 §8.4] padding shorthand with 4 values: top | right | bottom | left
        [Fact]
        public void PaddingShorthand_FourValues_AllDifferent()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='padding:4px 8px 12px 16px;width:100px;height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(4, target.PaddingTop);
            Assert.Equal(8, target.PaddingRight);
            Assert.Equal(12, target.PaddingBottom);
            Assert.Equal(16, target.PaddingLeft);
        }

        // [CSS2 §8.4] padding percentage resolves against containing block width
        [Fact]
        public void PaddingPercent_ResolvesAgainstContainingBlockWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div id='t' style='padding:10%;width:50px;height:50px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 10% of 200px = 20px on all sides
            Assert.True(System.Math.Abs(target.PaddingTop - 20) < 1,
                $"Expected padding-top=20, got {target.PaddingTop}");
            Assert.True(System.Math.Abs(target.PaddingLeft - 20) < 1,
                $"Expected padding-left=20, got {target.PaddingLeft}");
        }

        // [CSS2 §8.5] border widths with solid style
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        public void Border_SolidWidth_AllSidesEqual(int borderWidth)
        {
            var root = LayoutTestHelper.Layout(
                $"<body style='margin:0'><div id='t' style='border:{borderWidth}px solid black;width:100px;height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(borderWidth, target.BorderTopWidth);
            Assert.Equal(borderWidth, target.BorderRightWidth);
            Assert.Equal(borderWidth, target.BorderBottomWidth);
            Assert.Equal(borderWidth, target.BorderLeftWidth);
        }

        // [CSS2 §8.5] border-left offsets child X
        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        public void BorderLeft_ChildXOffset(int borderLeft)
        {
            var root = LayoutTestHelper.Layout(
                $"<body style='margin:0'><div style='border-left:{borderLeft}px solid black;width:200px'>" +
                $"<div id='child' style='height:20px'></div></div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            Assert.True(System.Math.Abs(child.ContentRect.X - borderLeft) < 1,
                $"Expected child X={borderLeft}, got {child.ContentRect.X}");
        }

        // [CSS2 §8.5] border-top offsets child Y
        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        public void BorderTop_ChildYOffset(int borderTop)
        {
            var root = LayoutTestHelper.Layout(
                $"<body style='margin:0'><div style='border-top:{borderTop}px solid black;width:200px'>" +
                $"<div id='child' style='height:20px'></div></div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            Assert.True(System.Math.Abs(child.ContentRect.Y - borderTop) < 1,
                $"Expected child Y={borderTop}, got {child.ContentRect.Y}");
        }

        // [CSS2 §8.5.1] border-style:none zeroes all border widths
        [Fact]
        public void BorderStyleNone_ZerosAllBorderWidths()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='border-width:10px;border-style:none;width:100px;height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(0, target.BorderTopWidth);
            Assert.Equal(0, target.BorderRightWidth);
            Assert.Equal(0, target.BorderBottomWidth);
            Assert.Equal(0, target.BorderLeftWidth);
        }

        // [CSS2 §8] padding + border combined offset child X and Y
        [Fact]
        public void PaddingAndBorder_CombinedChildXOffset()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='padding-left:15px;border-left:5px solid black;width:200px'>" +
                "<div id='child' style='height:20px'></div></div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            float expectedX = 15 + 5;
            Assert.True(System.Math.Abs(child.ContentRect.X - expectedX) < 1,
                $"Expected child X={expectedX}, got {child.ContentRect.X}");
        }

        [Fact]
        public void PaddingAndBorder_CombinedChildYOffset()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='padding-top:10px;border-top:3px solid black;width:200px'>" +
                "<div id='child' style='height:20px'></div></div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            float expectedY = 10 + 3;
            Assert.True(System.Math.Abs(child.ContentRect.Y - expectedY) < 1,
                $"Expected child Y={expectedY}, got {child.ContentRect.Y}");
        }

        // [CSS2 §10.3.3] auto width reduced by padding
        [Fact]
        public void AutoWidth_ReducedByPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='t' style='padding:0 20px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // auto width = 300 - 20*2 = 260
            Assert.True(System.Math.Abs(target.ContentRect.Width - 260) < 1,
                $"Expected width=260, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width reduced by border
        [Fact]
        public void AutoWidth_ReducedByBorder()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='t' style='border:5px solid;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // auto width = 300 - 5*2 = 290
            Assert.True(System.Math.Abs(target.ContentRect.Width - 290) < 1,
                $"Expected width=290, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width reduced by both padding and border
        [Fact]
        public void AutoWidth_ReducedByPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='t' style='padding:0 10px;border:3px solid;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // auto width = 300 - 10*2(padding) - 3*2(border) = 274
            Assert.True(System.Math.Abs(target.ContentRect.Width - 274) < 1,
                $"Expected width=274, got {target.ContentRect.Width}");
        }

        // [CSS-UI §3.2] box-sizing: border-box with padding
        [Fact]
        public void BorderBox_WithPadding_ContentWidthReduced()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;padding:20px;height:100px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // content width = 200 - 20*2 = 160
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 1,
                $"Expected content width=160, got {target.ContentRect.Width}");
            // content height = 100 - 20*2 = 60
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 1,
                $"Expected content height=60, got {target.ContentRect.Height}");
        }

        // [CSS-UI §3.2] box-sizing: border-box with border
        [Fact]
        public void BorderBox_WithBorder_ContentWidthReduced()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;border:10px solid;height:100px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // content width = 200 - 10*2 = 180
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 1,
                $"Expected content width=180, got {target.ContentRect.Width}");
            // content height = 100 - 10*2 = 80
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 1,
                $"Expected content height=80, got {target.ContentRect.Height}");
        }

        // [CSS-UI §3.2] box-sizing: border-box with both padding and border
        [Fact]
        public void BorderBox_WithPaddingAndBorder_ContentWidthReduced()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;padding:15px;border:5px solid;height:100px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // content width = 200 - 15*2 - 5*2 = 160
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 1,
                $"Expected content width=160, got {target.ContentRect.Width}");
            // content height = 100 - 15*2 - 5*2 = 60
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 1,
                $"Expected content height=60, got {target.ContentRect.Height}");
        }

        // [CSS-UI §4] outline does not affect layout or child positioning
        [Fact]
        public void Outline_DoesNotAffectChildPosition()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div style='outline:20px solid red;height:30px'></div>" +
                "<div id='sibling' style='height:30px'></div></div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 30) < 1,
                $"Outline should not push sibling down, got Y={sibling.ContentRect.Y}");
        }

        // [CSS-UI §4] outline does not affect element's own content width
        [Fact]
        public void Outline_DoesNotAffectOwnWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='t' style='outline:15px solid red;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1,
                $"Outline should not reduce width, got {target.ContentRect.Width}");
        }

        // [CSS2 §8.3.1] border on parent prevents margin collapse with first child
        [Fact]
        public void Border_PreventsMarginCollapse_TopChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='parent' style='border-top:1px solid;width:200px'>" +
                "<div id='child' style='margin-top:30px;height:20px'></div></div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            float gap = child.ContentRect.Y - parent.ContentRect.Y;
            // border-top prevents margin collapse: child is 1px(border) + 30px(margin) below parent content edge
            Assert.True(gap >= 30,
                $"Border should prevent collapse, gap={gap}");
        }

        // [CSS2 §8.3.1] border on parent prevents bottom margin collapse with last child
        [Fact]
        public void Border_PreventsMarginCollapse_BottomChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div id='parent' style='border-bottom:1px solid;width:200px'>" +
                "<div style='margin-bottom:30px;height:20px'></div></div>" +
                "<div id='sibling' style='height:20px'></div></div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            // parent auto height includes child margin because border prevents collapse
            float parentBottom = parent.ContentRect.Y + parent.ContentRect.Height +
                parent.PaddingTop + parent.PaddingBottom +
                parent.BorderTopWidth + parent.BorderBottomWidth;
            Assert.True(sibling.ContentRect.Y >= parentBottom - 1,
                $"Border prevents bottom collapse, sibling Y={sibling.ContentRect.Y}, parent bottom={parentBottom}");
        }

        // [CSS2 §8.3.1] padding on parent prevents margin collapse with first child
        [Fact]
        public void Padding_PreventsMarginCollapse_TopChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='parent' style='padding-top:1px;width:200px'>" +
                "<div id='child' style='margin-top:30px;height:20px'></div></div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            // padding-top prevents collapse: child at 1px(padding) + 30px(margin) = 31px
            Assert.True(child.ContentRect.Y >= 31,
                $"Padding should prevent collapse, child Y={child.ContentRect.Y}");
        }

        // [CSS2 §8.3.1] padding on parent prevents bottom margin collapse
        [Fact]
        public void Padding_PreventsMarginCollapse_BottomChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:200px'>" +
                "<div id='parent' style='padding-bottom:1px;width:200px'>" +
                "<div style='margin-bottom:30px;height:20px'></div></div>" +
                "<div id='sibling' style='height:20px'></div></div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            // parent auto height includes child margin because padding prevents collapse
            float parentBottom = parent.ContentRect.Y + parent.ContentRect.Height +
                parent.PaddingTop + parent.PaddingBottom +
                parent.BorderTopWidth + parent.BorderBottomWidth;
            Assert.True(sibling.ContentRect.Y >= parentBottom - 1,
                $"Padding prevents bottom collapse, sibling Y={sibling.ContentRect.Y}, parent bottom={parentBottom}");
        }

        // [CSS2 §8] padding adds to border rect but not content rect
        [Fact]
        public void Padding_BorderRectIncludesPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='padding:20px;width:100px;height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"Content width should be 100, got {target.ContentRect.Width}");
            Assert.Equal(20, target.PaddingLeft);
            Assert.Equal(20, target.PaddingRight);
        }

        // [CSS2 §8] border adds to border rect
        [Fact]
        public void Border_BorderRectIncludesBorder()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='border:8px solid;width:100px;height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"Content width should be 100, got {target.ContentRect.Width}");
            Assert.Equal(8, target.BorderLeftWidth);
            Assert.Equal(8, target.BorderRightWidth);
        }

        // [CSS2 §8.4] padding percentage on vertical axis also resolves against width
        [Fact]
        public void PaddingTopPercent_ResolvesAgainstWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<div id='t' style='padding-top:25%;height:0'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 25% of 400px = 100px
            Assert.True(System.Math.Abs(target.PaddingTop - 100) < 2,
                $"Expected padding-top=100, got {target.PaddingTop}");
        }

        // [CSS2 §10.3.3] auto width with asymmetric padding
        [Fact]
        public void AutoWidth_AsymmetricPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='t' style='padding-left:30px;padding-right:10px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // auto width = 300 - 30 - 10 = 260
            Assert.True(System.Math.Abs(target.ContentRect.Width - 260) < 1,
                $"Expected width=260, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.3.3] auto width with asymmetric border
        [Fact]
        public void AutoWidth_AsymmetricBorder()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'>" +
                "<div id='t' style='border-left:8px solid;border-right:2px solid;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // auto width = 300 - 8 - 2 = 290
            Assert.True(System.Math.Abs(target.ContentRect.Width - 290) < 1,
                $"Expected width=290, got {target.ContentRect.Width}");
        }

        // [CSS-UI §3.2] border-box child position includes padding subtraction from stated width
        [Fact]
        public void BorderBox_ChildPositionedInsidePadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='box-sizing:border-box;width:200px;padding:25px'>" +
                "<div id='child' style='height:20px'></div></div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            Assert.True(System.Math.Abs(child.ContentRect.X - 25) < 1,
                $"Expected child X=25, got {child.ContentRect.X}");
            Assert.True(System.Math.Abs(child.ContentRect.Y - 25) < 1,
                $"Expected child Y=25, got {child.ContentRect.Y}");
        }

        // [CSS-UI §3.2] border-box child auto width fills remaining content area
        [Fact]
        public void BorderBox_ChildAutoWidthFillsContentArea()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='box-sizing:border-box;width:200px;padding:15px;border:5px solid'>" +
                "<div id='child' style='height:20px'></div></div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            // content area = 200 - 15*2 - 5*2 = 160
            Assert.True(System.Math.Abs(child.ContentRect.Width - 160) < 1,
                $"Expected child width=160, got {child.ContentRect.Width}");
        }

        // [CSS2 §8.5] large border with padding combined child offset
        [Fact]
        public void LargeBorderAndPadding_CombinedChildOffset()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='padding:20px;border:10px solid;width:200px'>" +
                "<div id='child' style='height:20px'></div></div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            float expectedX = 20 + 10;
            float expectedY = 20 + 10;
            Assert.True(System.Math.Abs(child.ContentRect.X - expectedX) < 1,
                $"Expected child X={expectedX}, got {child.ContentRect.X}");
            Assert.True(System.Math.Abs(child.ContentRect.Y - expectedY) < 1,
                $"Expected child Y={expectedY}, got {child.ContentRect.Y}");
        }
    }
}
