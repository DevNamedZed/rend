using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS float containment and BFC interaction.
    /// Covers overflow:hidden/auto/scroll containing floats, display:flow-root,
    /// float positioning, clearing, shrink-to-fit, and float suppression in
    /// flex/grid contexts.
    /// </summary>
    public class WptFloatContainmentTests
    {
        private readonly ITestOutputHelper _output;

        public WptFloatContainmentTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §9.4.1] overflow:hidden establishes BFC and contains floats in parent height
        [Fact]
        public void OverflowHidden_ContainsFloatInHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='overflow:hidden;width:200px'>
                        <div style='float:left;width:80px;height:60px'></div>
                    </div>
                </body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(container.ContentRect.Height >= 59,
                $"overflow:hidden should contain float; height={container.ContentRect.Height}");
        }

        // [CSS2 §9.4.1] overflow:auto establishes BFC and contains floats in parent height
        [Fact]
        public void OverflowAuto_ContainsFloatInHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='overflow:auto;width:200px'>
                        <div style='float:left;width:80px;height:60px'></div>
                    </div>
                </body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(container.ContentRect.Height >= 59,
                $"overflow:auto should contain float; height={container.ContentRect.Height}");
        }

        // [CSS-DISPLAY §3] display:flow-root establishes BFC and contains floats
        [Fact]
        public void FlowRoot_ContainsFloatInHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='display:flow-root;width:200px'>
                        <div style='float:left;width:80px;height:60px'></div>
                    </div>
                </body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(container.ContentRect.Height >= 59,
                $"flow-root should contain float; height={container.ContentRect.Height}");
        }

        // [CSS2 §9.5.1] float:left positions at left content edge
        [Fact]
        public void FloatLeft_BasicPosition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div id='t' style='float:left;width:80px;height:40px'></div>
                    </div>
                </body>");
            var floated = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(floated.ContentRect.X < 2,
                $"float:left should be at left edge; X={floated.ContentRect.X}");
        }

        // [CSS2 §9.5.1] float:right positions at right content edge
        [Fact]
        public void FloatRight_BasicPosition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div id='t' style='float:right;width:80px;height:40px'></div>
                    </div>
                </body>");
            var floated = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(floated.ContentRect.X - 120) < 2,
                $"float:right should be at right edge; X={floated.ContentRect.X}");
        }

        // [CSS2 §9.5.1] two float:left elements stack horizontally
        [Fact]
        public void TwoLeftFloats_SideBySide()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div id='a' style='float:left;width:60px;height:40px'></div>
                        <div id='b' style='float:left;width:60px;height:40px'></div>
                    </div>
                </body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(second.ContentRect.X - 60) < 2,
                $"Second left float should start at 60px; X={second.ContentRect.X}");
            Assert.True(System.Math.Abs(first.ContentRect.Y - second.ContentRect.Y) < 2,
                "Both floats should be on the same line");
        }

        // [CSS2 §9.5.2] clear:left moves element below left float
        [Fact]
        public void ClearLeft_MovesBelowLeftFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div style='float:left;width:80px;height:50px'></div>
                        <div id='t' style='clear:left;height:20px'></div>
                    </div>
                </body>");
            var cleared = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(cleared.ContentRect.Y >= 49,
                $"clear:left should be below float; Y={cleared.ContentRect.Y}");
        }

        // [CSS2 §9.5.2] clear:right moves element below right float
        [Fact]
        public void ClearRight_MovesBelowRightFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div style='float:right;width:80px;height:50px'></div>
                        <div id='t' style='clear:right;height:20px'></div>
                    </div>
                </body>");
            var cleared = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(cleared.ContentRect.Y >= 49,
                $"clear:right should be below float; Y={cleared.ContentRect.Y}");
        }

        // [CSS2 §9.5.2] clear:both moves below tallest float on either side
        [Fact]
        public void ClearBoth_MovesBelowTallestFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div style='float:left;width:80px;height:50px'></div>
                        <div style='float:right;width:80px;height:70px'></div>
                        <div id='t' style='clear:both;height:20px'></div>
                    </div>
                </body>");
            var cleared = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(cleared.ContentRect.Y >= 69,
                $"clear:both should be below tallest float (70px); Y={cleared.ContentRect.Y}");
        }

        // [CSS2 §10.3.5] float with auto width uses shrink-to-fit
        [Fact]
        public void Float_ShrinkToFitWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:left'>
                            <div style='width:80px;height:20px'></div>
                        </div>
                    </div>
                </body>");
            var floated = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(floated.ContentRect.Width - 80) < 2,
                $"Float should shrink-to-fit child width; width={floated.ContentRect.Width}");
        }

        // [CSS2 §10.6.7] float inside BFC root contributes to parent height
        [Fact]
        public void Float_ContributesToHeight_WhenParentIsBfcRoot()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='overflow:hidden;width:200px'>
                        <div style='float:left;width:80px;height:100px'></div>
                    </div>
                </body>");
            var parent = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(parent.ContentRect.Height >= 99,
                $"BFC parent should include float in height; height={parent.ContentRect.Height}");
        }

        // [CSS2 §10.3.5] float with percentage width resolves against containing block
        [Fact]
        public void Float_PercentageWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:left;width:50%;height:40px'></div>
                    </div>
                </body>");
            var floated = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(floated.ContentRect.Width - 150) < 2,
                $"50% float width should be 150px; width={floated.ContentRect.Width}");
        }

        // [CSS2 §9.5] float with margin offsets from edge
        [Fact]
        public void Float_WithMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div id='t' style='float:left;width:60px;height:40px;margin:10px'></div>
                    </div>
                </body>");
            var floated = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(floated.ContentRect.X - 10) < 2,
                $"Float with margin-left:10px should have X=10; X={floated.ContentRect.X}");
            Assert.True(System.Math.Abs(floated.ContentRect.Y - 10) < 2,
                $"Float with margin-top:10px should have Y=10; Y={floated.ContentRect.Y}");
        }

        // [CSS2 §9.5] float with padding increases border box
        [Fact]
        public void Float_WithPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div id='t' style='float:left;width:60px;height:40px;padding:10px'></div>
                    </div>
                </body>");
            var floated = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(floated.ContentRect.Width - 60) < 2,
                $"Content width should be 60px; width={floated.ContentRect.Width}");
            Assert.True(System.Math.Abs(floated.PaddingLeft - 10) < 2,
                $"PaddingLeft should be 10px; padding={floated.PaddingLeft}");
            Assert.True(System.Math.Abs(floated.PaddingTop - 10) < 2,
                $"PaddingTop should be 10px; padding={floated.PaddingTop}");
        }

        // [CSS2 §9.5] float with negative margin can extend outside container
        [Fact]
        public void Float_NegativeMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div id='t' style='float:left;width:60px;height:40px;margin-left:-10px'></div>
                    </div>
                </body>");
            var floated = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(floated.ContentRect.X < 0,
                $"Negative margin-left float should have X < 0; X={floated.ContentRect.X}");
        }

        // [CSS2 §9.5.1] wide float that exceeds container stays at content edge
        [Fact]
        public void Float_WiderThanContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:100px'>
                        <div id='t' style='float:left;width:200px;height:40px'></div>
                    </div>
                </body>");
            var floated = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(floated.ContentRect.X < 2,
                $"Wide float should still start at left edge; X={floated.ContentRect.X}");
            Assert.True(System.Math.Abs(floated.ContentRect.Width - 200) < 2,
                $"Wide float should keep its declared width; width={floated.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §3] float is ignored on flex items
        [Fact]
        public void Float_InsideFlex_Ignored()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:flex;width:200px'>
                        <div id='a' style='float:left;width:60px;height:40px'></div>
                        <div id='b' style='width:60px;height:40px'></div>
                    </div>
                </body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            // In flex context, float is ignored; items should lay out as flex items side by side
            Assert.True(System.Math.Abs(second.ContentRect.X - 60) < 2,
                $"Flex items should be side by side (float ignored); B.X={second.ContentRect.X}");
        }

        // [CSS-GRID §6] float is ignored on grid items
        [Fact]
        public void Float_InsideGrid_Ignored()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:grid;grid-template-columns:60px 60px;width:200px'>
                        <div id='a' style='float:left;height:40px'></div>
                        <div id='b' style='height:40px'></div>
                    </div>
                </body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            // In grid context, float is ignored; items should be placed in grid cells
            Assert.True(System.Math.Abs(second.ContentRect.X - 60) < 2,
                $"Grid items should follow grid placement (float ignored); B.X={second.ContentRect.X}");
        }

        // [CSS2 §10.4] float with min-width respects minimum
        [Fact]
        public void Float_MinWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:left;width:40px;min-width:100px;height:30px'></div>
                    </div>
                </body>");
            var floated = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(floated.ContentRect.Width >= 99,
                $"Float with min-width:100px should be at least 100px; width={floated.ContentRect.Width}");
        }

        // [CSS2 §10.4] float with max-width clamps width
        [Fact]
        public void Float_MaxWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:left;width:200px;max-width:100px;height:30px'></div>
                    </div>
                </body>");
            var floated = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(floated.ContentRect.Width <= 101,
                $"Float with max-width:100px should be at most 100px; width={floated.ContentRect.Width}");
        }

        // [CSS2 §9.5.2] clear after multiple left floats of different heights
        [Fact]
        public void ClearLeft_AfterMultipleFloats()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div style='float:left;width:60px;height:30px'></div>
                        <div style='float:left;width:60px;height:80px'></div>
                        <div style='float:left;width:60px;height:50px'></div>
                        <div id='t' style='clear:left;height:20px'></div>
                    </div>
                </body>");
            var cleared = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(cleared.ContentRect.Y >= 79,
                $"clear:left should be below tallest left float (80px); Y={cleared.ContentRect.Y}");
        }

        // [CSS2 §9.4.1] overflow:hidden BFC contains multiple floats in height
        [Fact]
        public void OverflowHidden_ContainsMultipleFloats()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='overflow:hidden;width:200px'>
                        <div style='float:left;width:60px;height:40px'></div>
                        <div style='float:right;width:60px;height:70px'></div>
                    </div>
                </body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(container.ContentRect.Height >= 69,
                $"BFC should contain tallest float (70px); height={container.ContentRect.Height}");
        }

        // [CSS2 §9.4.1] overflow:scroll establishes BFC and contains floats
        [Fact]
        public void OverflowScroll_ContainsFloatInHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='overflow:scroll;width:200px'>
                        <div style='float:left;width:80px;height:60px'></div>
                    </div>
                </body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(container.ContentRect.Height >= 59,
                $"overflow:scroll should contain float; height={container.ContentRect.Height}");
        }

        // [CSS2 §9.5.1] float:left and float:right in same container
        [Fact]
        public void FloatLeft_And_FloatRight_SameContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div id='left' style='float:left;width:60px;height:40px'></div>
                        <div id='right' style='float:right;width:60px;height:40px'></div>
                    </div>
                </body>");
            var leftFloat = LayoutTestHelper.FindById(root, "left")!;
            var rightFloat = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(leftFloat.ContentRect.X < 2,
                $"Left float should be at left edge; X={leftFloat.ContentRect.X}");
            Assert.True(System.Math.Abs(rightFloat.ContentRect.X - 140) < 2,
                $"Right float should be at right edge; X={rightFloat.ContentRect.X}");
        }

        // [CSS2 §9.5.2] clear:left ignores right floats
        [Fact]
        public void ClearLeft_IgnoresRightFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div style='float:right;width:60px;height:80px'></div>
                        <div id='t' style='clear:left;height:20px'></div>
                    </div>
                </body>");
            var cleared = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(cleared.ContentRect.Y < 5,
                $"clear:left should not be affected by right float; Y={cleared.ContentRect.Y}");
        }

        // [CSS2 §9.5.2] clear:right ignores left floats
        [Fact]
        public void ClearRight_IgnoresLeftFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div style='float:left;width:60px;height:80px'></div>
                        <div id='t' style='clear:right;height:20px'></div>
                    </div>
                </body>");
            var cleared = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(cleared.ContentRect.Y < 5,
                $"clear:right should not be affected by left float; Y={cleared.ContentRect.Y}");
        }

        // [CSS2 §9.4.1] BFC element adjacent to float avoids overlap
        [Fact]
        public void OverflowHidden_AvoidsSiblingFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div style='float:left;width:80px;height:50px'></div>
                        <div id='t' style='overflow:hidden;height:30px'>content</div>
                    </div>
                </body>");
            var bfcBlock = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(bfcBlock.ContentRect.X >= 79,
                $"BFC block should avoid sibling float; X={bfcBlock.ContentRect.X}");
        }

        // [CSS-DISPLAY §3] flow-root BFC avoids sibling left float
        [Fact]
        public void FlowRoot_AvoidsSiblingLeftFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div style='float:left;width:80px;height:50px'></div>
                        <div id='t' style='display:flow-root;height:30px'>content</div>
                    </div>
                </body>");
            var bfcBlock = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(bfcBlock.ContentRect.X >= 79,
                $"Flow-root should avoid left float; X={bfcBlock.ContentRect.X}");
        }

        // [CSS2 §9.4.1] inline-block establishes BFC and contains float
        [Fact]
        public void InlineBlock_ContainsFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <span id='t' style='display:inline-block;width:120px'>
                            <div style='float:left;width:50px;height:40px'></div>
                        </span>
                    </div>
                </body>");
            var inlineBlock = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(inlineBlock.ContentRect.Height >= 39,
                $"Inline-block BFC should contain float; height={inlineBlock.ContentRect.Height}");
        }

        // [CSS2 §9.5] float with both margin and padding
        [Fact]
        public void Float_MarginAndPadding_Combined()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:left;width:60px;height:40px;margin:10px;padding:5px'></div>
                    </div>
                </body>");
            var floated = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(floated.ContentRect.X - 15) < 2,
                $"Content X should be margin(10)+padding(5)=15; X={floated.ContentRect.X}");
            Assert.True(System.Math.Abs(floated.ContentRect.Width - 60) < 2,
                $"Content width should be 60px; width={floated.ContentRect.Width}");
        }

        // [CSS2 §10.6.7] BFC auto height includes float bottom margin edge
        [Fact]
        public void BfcAutoHeight_IncludesFloatWithMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='overflow:hidden;width:200px'>
                        <div style='float:left;width:80px;height:60px;margin-bottom:20px'></div>
                    </div>
                </body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            // Float total height = 60 + 20(margin-bottom) = 80
            Assert.True(container.ContentRect.Height >= 79,
                $"BFC auto height should include float margin-bottom; height={container.ContentRect.Height}");
        }

        // [CSS2 §9.5.1] second left float placed adjacent to first
        [Fact]
        public void TwoLeftFloats_SecondAdjacentToFirst()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='a' style='float:left;width:100px;height:40px'></div>
                        <div id='b' style='float:left;width:100px;height:40px'></div>
                    </div>
                </body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            float expectedSecondX = first.ContentRect.X + first.ContentRect.Width
                                   + first.PaddingRight + first.BorderRightWidth
                                   + first.MarginRight + second.MarginLeft
                                   + second.BorderLeftWidth + second.PaddingLeft;
            Assert.True(System.Math.Abs(second.ContentRect.X - expectedSecondX) < 2,
                $"Second float should be adjacent; expected X~{expectedSecondX}, actual={second.ContentRect.X}");
        }

        // [CSS-FLEXBOX §4] flex item establishes BFC and contains its child floats
        [Fact]
        public void FlexItem_ContainsChildFloats()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:flex;width:200px'>
                        <div id='t' style='width:100px'>
                            <div style='float:left;width:50px;height:40px'></div>
                        </div>
                    </div>
                </body>");
            var flexItem = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(flexItem.ContentRect.Height >= 39,
                $"Flex item BFC should contain float; height={flexItem.ContentRect.Height}");
        }

        // [CSS-GRID §6] grid item establishes BFC and contains its child floats
        [Fact]
        public void GridItem_ContainsChildFloats()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:grid;grid-template-columns:200px;width:200px'>
                        <div id='t'>
                            <div style='float:left;width:50px;height:40px'></div>
                        </div>
                    </div>
                </body>");
            var gridItem = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(gridItem.ContentRect.Height >= 39,
                $"Grid item BFC should contain float; height={gridItem.ContentRect.Height}");
        }
    }
}
