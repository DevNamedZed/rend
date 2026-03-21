using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive margin tests covering CSS2 §8.3: individual sides, shorthand expansion,
    /// auto centering, negative margins, percentage margins, margin collapsing rules,
    /// and margin behavior in flex/grid contexts.
    /// </summary>
    public class WptBlockAllMarginTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockAllMarginTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §8.3] margin-left pushes content right by specified amount
        [Theory]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(30)]
        [InlineData(40)]
        [InlineData(50)]
        public void MarginLeft_OffsetsXPosition(int marginLeft)
        {
            var root = LayoutTestHelper.Layout(
                $"<body style='margin:0'><div id='t' style='margin-left:{marginLeft}px;width:50px;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X - marginLeft) < 2,
                $"margin-left:{marginLeft}px should place X at {marginLeft} (got {target.ContentRect.X})");
        }

        // [CSS2 §8.3] margin-top pushes content down by specified amount
        [Theory]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(30)]
        [InlineData(40)]
        [InlineData(50)]
        public void MarginTop_OffsetsYPosition(int marginTop)
        {
            var root = LayoutTestHelper.Layout(
                $"<body style='margin:0'><div id='t' style='margin-top:{marginTop}px;width:50px;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Y - marginTop) < 2,
                $"margin-top:{marginTop}px should place Y at {marginTop} (got {target.ContentRect.Y})");
        }

        // [CSS2 §8.3] margin-right does not affect element's own X position
        [Fact]
        public void MarginRight_DoesNotAffectOwnXPosition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='margin-right:50px;width:100px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X) < 2,
                $"margin-right should not affect own X (got {target.ContentRect.X})");
        }

        // [CSS2 §8.3] margin-bottom affects Y position of the next sibling
        [Fact]
        public void MarginBottom_AffectsNextSiblingY()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px;overflow:hidden'>
                    <div style='margin-bottom:25px;height:30px'></div>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Y - 55) < 2,
                $"Next sibling Y should be 30+25=55 (got {target.ContentRect.Y})");
        }

        // [CSS2 §8.3] margin:auto centers block at various container widths
        [Theory]
        [InlineData(200, 100, 50)]
        [InlineData(400, 200, 100)]
        [InlineData(600, 300, 150)]
        public void MarginAuto_CentersAtWidth(int containerWidth, int childWidth, int expectedX)
        {
            var root = LayoutTestHelper.Layout(
                $"<body style='margin:0'><div style='width:{containerWidth}px'>" +
                $"<div id='t' style='width:{childWidth}px;margin:0 auto;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X - expectedX) < 2,
                $"margin:auto in {containerWidth}px container with {childWidth}px child should center at X={expectedX} (got {target.ContentRect.X})");
        }

        // [CSS2 §8.3] margin-left:auto pushes element to the right
        [Fact]
        public void MarginLeftAuto_PushesRight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:100px;margin-left:auto;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X - 300) < 2,
                $"margin-left:auto should push to X=300 (got {target.ContentRect.X})");
        }

        // [CSS2 §8.3] margin shorthand with 2 values: vertical horizontal
        [Theory]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(50)]
        public void MarginShorthand_TwoValues_Horizontal(int horizontalMargin)
        {
            var root = LayoutTestHelper.Layout(
                $"<body style='margin:0;overflow:hidden'>" +
                $"<div id='t' style='margin:0 {horizontalMargin}px;width:50px;height:20px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X - horizontalMargin) < 2,
                $"margin:0 {horizontalMargin}px should set X={horizontalMargin} (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.MarginRight - horizontalMargin) < 2,
                $"margin-right should be {horizontalMargin} (got {target.MarginRight})");
        }

        // [CSS2 §8.3] margin shorthand with 4 values: top right bottom left
        [Fact]
        public void MarginShorthand_FourValues_AllSides()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'>
                    <div id='t' style='margin:10px 20px 30px 40px;width:50px;height:50px'></div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.Equal(10, target!.MarginTop);
            Assert.Equal(20, target.MarginRight);
            Assert.Equal(30, target.MarginBottom);
            Assert.Equal(40, target.MarginLeft);
        }

        // [CSS2 §8.3] margin shorthand with 4 values: X position = marginLeft
        [Fact]
        public void MarginShorthand_FourValues_XPosition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'>
                    <div id='t' style='margin:10px 20px 30px 40px;width:50px;height:50px'></div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X - 40) < 2,
                $"margin-left:40px should place X at 40 (got {target.ContentRect.X})");
        }

        // [CSS2 §8.3] margin shorthand with 4 values: Y position = marginTop
        [Fact]
        public void MarginShorthand_FourValues_YPosition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'>
                    <div id='t' style='margin:10px 20px 30px 40px;width:50px;height:50px'></div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Y - 10) < 2,
                $"margin-top:10px should place Y at 10 (got {target.ContentRect.Y})");
        }

        // [CSS2 §8.3] margin percentage resolves against containing block width
        [Fact]
        public void MarginPercentage_ResolvesAgainstContainingBlockWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px'>
                    <div id='t' style='margin-left:10%;width:50px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X - 20) < 2,
                $"margin-left:10% of 200px = 20px (got {target.ContentRect.X})");
        }

        // [CSS2 §8.3] margin-top percentage also resolves against containing block width
        [Fact]
        public void MarginTopPercentage_ResolvesAgainstWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px'>
                    <div id='t' style='margin-top:10%;width:50px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Y - 20) < 2,
                $"margin-top:10% of 200px width = 20px (got {target.ContentRect.Y})");
        }

        // [CSS2 §8.3] negative margin-left shifts element left
        [Fact]
        public void NegativeMarginLeft_ShiftsLeft()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px'>
                    <div style='margin-left:50px'>
                        <div id='t' style='margin-left:-20px;width:50px;height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X - 30) < 2,
                $"50px parent margin + (-20px) margin-left = X at 30 (got {target.ContentRect.X})");
        }

        // [CSS2 §8.3] negative margin-top pulls element up
        [Fact]
        public void NegativeMarginTop_PullsUp()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px;overflow:hidden'>
                    <div style='height:40px'></div>
                    <div id='t' style='margin-top:-15px;width:50px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Y - 25) < 2,
                $"40px sibling + (-15px) margin-top = Y at 25 (got {target.ContentRect.Y})");
        }

        // [CSS2 §8.3] margin shorthand 1 value applies to all sides
        [Fact]
        public void MarginShorthand_OneValue()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'>
                    <div id='t' style='margin:15px;width:50px;height:50px'></div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.Equal(15, target!.MarginTop);
            Assert.Equal(15, target.MarginRight);
            Assert.Equal(15, target.MarginBottom);
            Assert.Equal(15, target.MarginLeft);
        }

        // [CSS2 §8.3] margin shorthand 2 values: vertical horizontal
        [Fact]
        public void MarginShorthand_TwoValues()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'>
                    <div id='t' style='margin:10px 25px;width:50px;height:50px'></div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.Equal(10, target!.MarginTop);
            Assert.Equal(25, target.MarginRight);
            Assert.Equal(10, target.MarginBottom);
            Assert.Equal(25, target.MarginLeft);
        }

        // [CSS2 §8.3] margin shorthand 3 values: top horizontal bottom
        [Fact]
        public void MarginShorthand_ThreeValues()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'>
                    <div id='t' style='margin:10px 20px 30px;width:50px;height:50px'></div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.Equal(10, target!.MarginTop);
            Assert.Equal(20, target.MarginRight);
            Assert.Equal(30, target.MarginBottom);
            Assert.Equal(20, target.MarginLeft);
        }

        // [CSS2 §8.3.1] adjacent sibling margins collapse: max(30, 20) = 30
        [Fact]
        public void MarginCollapse_AdjacentSiblings_MaxWins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px'>
                    <div style='margin-bottom:30px;height:20px'></div>
                    <div id='t' style='margin-top:20px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Y - 50) < 2,
                $"max(30,20)=30 gap, Y=20+30=50 (got {target.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] border-top on parent prevents margin collapse with first child
        [Fact]
        public void NoCollapse_WithBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='parent' style='border-top:1px solid black;width:200px'>
                        <div id='child' style='margin-top:30px;height:20px'></div>
                    </div>
                </body>");
            var parent = LayoutTestHelper.FindById(root, "parent");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(parent);
            Assert.NotNull(child);
            float childRelativeY = child!.ContentRect.Y - parent!.ContentRect.Y;
            Assert.True(childRelativeY >= 30,
                $"Border prevents collapse, child offset should be >= 30 (got {childRelativeY})");
        }

        // [CSS2 §8.3.1] overflow:hidden establishes BFC, prevents margin collapse
        [Fact]
        public void NoCollapse_OverflowHidden()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='parent' style='overflow:hidden;width:200px'>
                        <div id='child' style='margin-top:30px;height:20px'></div>
                    </div>
                </body>");
            var parent = LayoutTestHelper.FindById(root, "parent");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(parent);
            Assert.NotNull(child);
            float childRelativeY = child!.ContentRect.Y - parent!.ContentRect.Y;
            Assert.True(childRelativeY >= 29,
                $"overflow:hidden prevents collapse, child offset >= 30 (got {childRelativeY})");
        }

        // [CSS-FLEXBOX §4] flex container prevents margin collapse between children
        [Fact]
        public void NoCollapse_FlexContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:flex;flex-direction:column;width:200px'>
                        <div id='a' style='margin-bottom:30px;height:20px'></div>
                        <div id='b' style='margin-top:20px;height:20px'></div>
                    </div>
                </body>");
            var boxA = LayoutTestHelper.FindById(root, "a");
            var boxB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(boxA);
            Assert.NotNull(boxB);
            float gap = boxB!.ContentRect.Y - (boxA!.ContentRect.Y + 20);
            Assert.True(System.Math.Abs(gap - 50) < 2,
                $"Flex items do not collapse: gap should be 30+20=50 (got {gap})");
        }

        // [CSS-FLEXBOX §8.1] margin on flex item affects positioning within flex container
        [Fact]
        public void MarginInFlexItem_AffectsPosition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:flex;width:400px'>
                        <div id='t' style='margin-left:25px;width:100px;height:30px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X - 25) < 2,
                $"Flex item margin-left:25px should set X=25 (got {target.ContentRect.X})");
        }

        // [CSS-GRID §11.1] margin on grid item affects positioning within grid cell
        [Fact]
        public void MarginInGridItem_AffectsPosition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:grid;grid-template-columns:200px;width:200px'>
                        <div id='t' style='margin-left:15px;height:30px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X - 15) < 2,
                $"Grid item margin-left:15px should set X=15 (got {target.ContentRect.X})");
        }

        // [CSS2 §8.3] margin-bottom on last sibling affects parent auto height
        [Fact]
        public void MarginBottom_LastChild_ParentAutoHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='parent' style='width:200px;overflow:hidden'>
                        <div style='height:40px;margin-bottom:20px'></div>
                    </div>
                </body>");
            var parent = LayoutTestHelper.FindById(root, "parent");
            Assert.NotNull(parent);
            Assert.True(System.Math.Abs(parent!.ContentRect.Height - 60) < 2,
                $"Parent auto height should include margin-bottom: 40+20=60 (got {parent.ContentRect.Height})");
        }

        // [CSS2 §8.3] margin-left:auto + margin-right:auto centers element
        [Fact]
        public void MarginLeftAutoRightAuto_Centers()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:300px'>
                    <div id='t' style='width:100px;margin-left:auto;margin-right:auto;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X - 100) < 2,
                $"margin:auto centers, X should be 100 (got {target.ContentRect.X})");
        }

        // [CSS2 §8.3] margin-right:auto absorbs remaining space (element stays at left)
        [Fact]
        public void MarginRightAuto_ElementStaysLeft()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:100px;margin-right:auto;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X) < 2,
                $"margin-right:auto should keep element at X=0 (got {target.ContentRect.X})");
        }

        // [CSS2 §8.3] margin:0 auto with border-box sizing
        [Fact]
        public void MarginAutoCenter_WithBorderBox()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='box-sizing:border-box;width:200px;margin:0 auto;padding:10px;border:5px solid;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.BorderRect.X - 100) < 2,
                $"border-box 200px in 400px container, border rect X=100 (got {target.BorderRect.X})");
        }

        // [CSS2 §8.3] multiple siblings with margin-bottom pushing each down
        [Fact]
        public void MarginBottom_CascadesToMultipleSiblings()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px;overflow:hidden'>
                    <div style='height:20px;margin-bottom:10px'></div>
                    <div style='height:20px;margin-bottom:10px'></div>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Y - 60) < 2,
                $"Third child Y should be 20+10+20+10=60 (got {target.ContentRect.Y})");
        }

        // [CSS2 §8.3] large margin-left does not expand container
        [Fact]
        public void LargeMarginLeft_DoesNotExpandContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='container' style='width:200px;overflow:hidden'>
                    <div style='margin-left:150px;width:100px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container");
            Assert.NotNull(container);
            Assert.True(System.Math.Abs(container!.ContentRect.Width - 200) < 2,
                $"Container width should remain 200 (got {container.ContentRect.Width})");
        }

        // [CSS2 §8.3] margin-left + margin-right reduce auto width
        [Fact]
        public void MarginLeftRight_ReduceAutoWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:300px'>
                    <div id='t' style='margin-left:30px;margin-right:20px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 250) < 2,
                $"Auto width = 300-30-20=250 (got {target.ContentRect.Width})");
        }

        // [CSS2 §8.3.1] parent-child margin collapse when parent has no border/padding
        [Fact]
        public void MarginCollapse_ParentChild_NoBorderPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='parent' style='margin-top:20px;width:200px'>
                        <div id='child' style='margin-top:30px;height:20px'></div>
                    </div>
                </body>");
            var parent = LayoutTestHelper.FindById(root, "parent");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(parent);
            Assert.NotNull(child);
            Assert.True(System.Math.Abs(parent!.ContentRect.Y - 30) < 2,
                $"Parent-child collapse: max(20,30)=30, parent Y=30 (got {parent.ContentRect.Y})");
            Assert.True(System.Math.Abs(child!.ContentRect.Y - parent.ContentRect.Y) < 2,
                $"Child should be at same Y as parent after collapse (parent={parent.ContentRect.Y}, child={child.ContentRect.Y})");
        }

        // [CSS2 §8.3.1] padding-top on parent prevents collapse with first child
        [Fact]
        public void NoCollapse_WithPaddingTop()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='parent' style='padding-top:1px;width:200px'>
                        <div id='child' style='margin-top:30px;height:20px'></div>
                    </div>
                </body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            Assert.True(child!.ContentRect.Y >= 31,
                $"padding-top prevents collapse, child Y >= 31 (got {child.ContentRect.Y})");
        }

        // [CSS2 §8.3] margin:0 produces zero margin on all sides
        [Fact]
        public void MarginZero_NoOffset()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'>
                    <div id='t' style='margin:0;width:50px;height:50px'></div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.Equal(0, target!.MarginTop);
            Assert.Equal(0, target.MarginRight);
            Assert.Equal(0, target.MarginBottom);
            Assert.Equal(0, target.MarginLeft);
        }

        // [CSS2 §8.3] negative margin-left with auto width expands element
        [Fact]
        public void NegativeMarginLeft_AutoWidth_Expands()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px'>
                    <div id='t' style='margin-left:-20px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(target!.ContentRect.Width >= 219,
                $"Negative margin-left expands auto width to 220 (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §11.1] grid item margins do not collapse
        [Fact]
        public void NoCollapse_GridItems()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:grid;grid-template-columns:1fr;width:200px'>
                        <div id='a' style='margin-bottom:30px;height:20px'></div>
                        <div id='b' style='margin-top:20px;height:20px'></div>
                    </div>
                </body>");
            var boxA = LayoutTestHelper.FindById(root, "a");
            var boxB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(boxA);
            Assert.NotNull(boxB);
            float gap = boxB!.ContentRect.Y - (boxA!.ContentRect.Y + 20);
            Assert.True(gap >= 49,
                $"Grid items do not collapse: gap should be >= 50 (got {gap})");
        }

        // [CSS2 §8.3] margin-top on flex item in row direction
        [Fact]
        public void MarginTop_FlexItemRow()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:flex;width:400px;height:100px'>
                        <div id='t' style='margin-top:15px;width:100px;height:30px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Y - 15) < 2,
                $"Flex row item margin-top:15px should set Y=15 (got {target.ContentRect.Y})");
        }

        // [CSS2 §8.3] margin-bottom:0 between siblings produces no gap
        [Fact]
        public void MarginBottomZero_NoGap()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px;overflow:hidden'>
                    <div style='margin-bottom:0;height:30px'></div>
                    <div id='t' style='margin-top:0;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Y - 30) < 2,
                $"Zero margins produce no gap, Y=30 (got {target.ContentRect.Y})");
        }

        // [CSS2 §8.3] margin-right on auto width does not affect content width calculation
        [Fact]
        public void MarginRight_ReducesAutoWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:300px'>
                    <div id='t' style='margin-right:50px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 250) < 2,
                $"Auto width with margin-right:50px = 300-50=250 (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §8.1] margin:auto in flex cross axis absorbs free space
        [Fact]
        public void FlexItem_MarginAutoTop_PushesDown()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:flex;width:300px;height:100px'>
                        <div id='t' style='margin-top:auto;width:50px;height:30px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Y - 70) < 2,
                $"margin-top:auto in flex pushes to bottom, Y=100-30=70 (got {target.ContentRect.Y})");
        }

        // [CSS2 §8.3] margin-bottom on sibling followed by margin-top on next: larger wins
        [Fact]
        public void MarginCollapse_LargerTopWins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px'>
                    <div style='margin-bottom:10px;height:20px'></div>
                    <div id='t' style='margin-top:40px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Y - 60) < 2,
                $"max(10,40)=40, Y=20+40=60 (got {target.ContentRect.Y})");
        }

        // [CSS2 §8.3] equal margins collapse to same value
        [Fact]
        public void MarginCollapse_EqualMargins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px'>
                    <div style='margin-bottom:25px;height:20px'></div>
                    <div id='t' style='margin-top:25px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Y - 45) < 2,
                $"max(25,25)=25, Y=20+25=45 (got {target.ContentRect.Y})");
        }

        // [CSS-GRID §11.1] margin on grid item with margin-top affects Y in grid cell
        [Fact]
        public void GridItem_MarginTop_AffectsY()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:grid;grid-template-columns:200px;grid-template-rows:80px;width:200px'>
                        <div id='t' style='margin-top:10px;height:30px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Y - 10) < 2,
                $"Grid item margin-top:10px, Y=10 (got {target.ContentRect.Y})");
        }

        // [CSS2 §8.3] margin-left applied inside nested containers
        [Fact]
        public void MarginLeft_NestedContainers()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='margin-left:20px;width:300px'>
                        <div id='t' style='margin-left:15px;width:50px;height:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X - 35) < 2,
                $"Nested margin-left: 20+15=35 (got {target.ContentRect.X})");
        }
    }
}
