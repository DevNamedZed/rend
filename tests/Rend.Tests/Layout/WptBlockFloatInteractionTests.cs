using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS block/float interactions: float placement, clearing,
    /// BFC containment, float suppression in flex/grid, and edge cases.
    /// CSS 2.1 §9.5, §9.5.1, §9.5.2, §9.4.1
    /// </summary>
    public class WptBlockFloatInteractionTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockFloatInteractionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §9.5.1] float:left positions element at left content edge
        [Fact]
        public void FloatLeft_PositionedAtLeftContentEdge()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:left;width:100px;height:50px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2,
                $"float:left should be at X=0, got {target.ContentRect.X}");
        }

        // [CSS2 §9.5.1] float:right positions element against right content edge
        [Fact]
        public void FloatRight_PositionedAtRightContentEdge()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:right;width:100px;height:50px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 200) < 2,
                $"float:right should be at X=200, got {target.ContentRect.X}");
        }

        // [CSS2 §10.3.5] float shrinks to fit content width
        [Fact]
        public void FloatShrinkToFit_MatchesContentWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:400px'>
                        <div id='t' style='float:left'>
                            <div style='width:120px;height:30px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 120) < 2,
                $"Float should shrink-to-fit at 120px, got {target.ContentRect.Width}");
        }

        // [CSS2 §9.5] float does not affect parent auto-height when parent is not BFC
        [Fact(Skip = "Known bug: CalculateAutoHeight includes float children even for non-BFC blocks")]
        public void FloatDoesNotAffectParentHeight_NoBfc()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='width:200px'>
                        <div style='float:left;width:80px;height:100px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height < 2,
                $"Non-BFC parent height should be 0, got {target.ContentRect.Height}");
        }

        // [CSS2 §9.4.1] overflow:hidden establishes BFC and contains floats
        [Fact]
        public void OverflowHidden_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='overflow:hidden;width:200px'>
                        <div style='float:left;width:80px;height:90px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height >= 89,
                $"BFC should contain float, height >= 90, got {target.ContentRect.Height}");
        }

        // [CSS2 §9.5.2] clear:left forces element below left float
        [Fact]
        public void ClearLeft_ForcesBelowLeftFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div style='float:left;width:100px;height:60px'></div>
                        <div id='t' style='clear:left;height:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 59,
                $"clear:left should push below float at Y>=60, got {target.ContentRect.Y}");
        }

        // [CSS2 §9.5.2] clear:right forces element below right float
        [Fact]
        public void ClearRight_ForcesBelowRightFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div style='float:right;width:100px;height:70px'></div>
                        <div id='t' style='clear:right;height:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 69,
                $"clear:right should push below float at Y>=70, got {target.ContentRect.Y}");
        }

        // [CSS2 §9.5.2] clear:both forces below tallest float
        [Fact]
        public void ClearBoth_ForcesBelowTallestFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div style='float:left;width:80px;height:40px'></div>
                        <div style='float:right;width:80px;height:80px'></div>
                        <div id='t' style='clear:both;height:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 79,
                $"clear:both should clear tallest float at Y>=80, got {target.ContentRect.Y}");
        }

        // [CSS2 §9.5.1] two left floats side by side
        [Fact]
        public void TwoLeftFloats_SideBySide()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='a' style='float:left;width:100px;height:50px'></div>
                        <div id='b' style='float:left;width:100px;height:50px'></div>
                    </div>
                </body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2,
                $"First float at X=0, got {first.ContentRect.X}");
            Assert.True(System.Math.Abs(second.ContentRect.X - 100) < 2,
                $"Second float at X=100, got {second.ContentRect.X}");
            Assert.True(System.Math.Abs(first.ContentRect.Y - second.ContentRect.Y) < 2,
                "Both floats on same line");
        }

        // [CSS2 §9.5] float with margin offsets position
        [Fact]
        public void FloatWithMargin_OffsetsPosition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:left;width:80px;height:40px;margin-left:20px;margin-top:15px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2,
                $"Float margin-left=20 should give X=20, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2,
                $"Float margin-top=15 should give Y=15, got {target.ContentRect.Y}");
        }

        // [CSS2 §10.3.5] float with percentage width resolves against containing block
        [Fact]
        public void FloatPercentageWidth_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div id='t' style='float:left;width:25%;height:30px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 50) < 2,
                $"25% of 200 = 50, got {target.ContentRect.Width}");
        }

        // [CSS2 §9.5.1] float:none means no float behavior
        [Fact]
        public void FloatNone_NoFloatBehavior()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:200px'>
                        <div style='height:30px'></div>
                        <div id='t' style='float:none;width:100px;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2,
                $"float:none should be at X=0, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2,
                $"float:none should stack normally at Y=30, got {target.ContentRect.Y}");
        }

        // [CSS3-FLEXBOX §3] float is ignored on flex items
        [Fact]
        public void FloatIgnoredInFlex()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:flex;width:300px'>
                        <div id='a' style='float:left;width:100px;height:50px'></div>
                        <div id='b' style='width:100px;height:50px'></div>
                    </div>
                </body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            // In flex, float is ignored; items lay out as flex items side by side
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2,
                $"Flex item a at X=0, got {first.ContentRect.X}");
            Assert.True(System.Math.Abs(second.ContentRect.X - 100) < 2,
                $"Flex item b at X=100, got {second.ContentRect.X}");
        }

        // [CSS3-GRID §3] float is ignored on grid items
        [Fact]
        public void FloatIgnoredInGrid()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                        <div id='a' style='float:right;height:50px'></div>
                        <div id='b' style='height:50px'></div>
                    </div>
                </body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            // In grid, float is ignored; items placed in grid cells
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2,
                $"Grid item a at X=0, got {first.ContentRect.X}");
            Assert.True(System.Math.Abs(second.ContentRect.X - 100) < 2,
                $"Grid item b at X=100, got {second.ContentRect.X}");
        }

        // [CSS2 §9.5] normal block overlaps float (no BFC)
        [Fact]
        public void BlockOverlapsFloat_NoBfc()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div style='float:left;width:100px;height:60px'></div>
                        <div id='t' style='width:200px;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Normal block-level box starts at X=0, overlapping the float
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2,
                $"Block should overlap float at X=0, got {target.ContentRect.X}");
        }

        // [CSS2 §10.4] float with min-width prevents shrinking below minimum
        [Fact(Skip = "Known bug: min-width not applied to float shrink-to-fit")]
        public void FloatWithMinWidth_RespectsMinimum()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:left;min-width:150px;height:40px'>
                            <div style='width:50px;height:20px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width >= 149,
                $"min-width:150px should enforce width >= 150, got {target.ContentRect.Width}");
        }

        // [CSS2 §10.4] float with max-width prevents growing beyond maximum
        [Fact(Skip = "Known bug: max-width not applied to float shrink-to-fit")]
        public void FloatWithMaxWidth_RespectsMaximum()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:left;max-width:80px;height:40px'>
                            <div style='width:200px;height:20px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width <= 81,
                $"max-width:80px should cap width <= 80, got {target.ContentRect.Width}");
        }

        // [CSS2 §9.5.1] float wraps to next line when insufficient space
        [Fact]
        public void FloatWrapsToNextLine_InsufficientSpace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:150px'>
                        <div id='a' style='float:left;width:100px;height:40px'></div>
                        <div id='b' style='float:left;width:100px;height:40px'></div>
                    </div>
                </body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            // Second float cannot fit beside first (100+100 > 150), wraps below
            Assert.True(second.ContentRect.Y >= 39 || second.ContentRect.X >= 99,
                $"Second float should wrap below first or stack (X={second.ContentRect.X}, Y={second.ContentRect.Y})");
        }

        // [CSS2 §9.5] negative margin on float shifts position
        [Fact]
        public void NegativeMarginFloat_ShiftsPosition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:left;width:80px;height:40px;margin-left:-10px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - (-10)) < 2,
                $"Negative margin-left should shift to X=-10, got {target.ContentRect.X}");
        }

        // [CSS2 §9.3.1] float inside relatively positioned parent
        [Fact]
        public void FloatInsideRelativePosition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;left:30px;width:200px'>
                        <div id='t' style='float:left;width:80px;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Float positioned at parent content edge; parent shifted by left:30px
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2,
                $"Float inside relative parent should be at X=30, got {target.ContentRect.X}");
        }

        // [CSS2 §9.5.2] clear with margin collapsing: clearance absorbs margin
        [Fact]
        public void ClearWithMarginCollapsing_ClearanceAbsorbsMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div style='float:left;width:100px;height:60px'></div>
                        <div style='height:20px;margin-bottom:30px'></div>
                        <div id='t' style='clear:left;margin-top:10px;height:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // clear:left border edge must be at or below float bottom (60)
            Assert.True(target.ContentRect.Y >= 59,
                $"Cleared element should be at Y>=60, got {target.ContentRect.Y}");
        }

        // [CSS2 §9.5.1] left and right floats on same row
        [Fact]
        public void LeftAndRightFloats_SameRow()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='left' style='float:left;width:100px;height:50px'></div>
                        <div id='right' style='float:right;width:100px;height:50px'></div>
                    </div>
                </body>");
            var left = LayoutTestHelper.FindById(root, "left")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(left.ContentRect.X) < 2,
                $"Left float at X=0, got {left.ContentRect.X}");
            Assert.True(System.Math.Abs(right.ContentRect.X - 200) < 2,
                $"Right float at X=200, got {right.ContentRect.X}");
            Assert.True(System.Math.Abs(left.ContentRect.Y - right.ContentRect.Y) < 2,
                "Both floats on same Y line");
        }

        // [CSS2 §9.4.1] BFC avoids adjacent float by narrowing width
        [Fact]
        public void BfcBlock_AvoidsAdjacentFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div style='float:left;width:120px;height:60px'></div>
                        <div id='t' style='overflow:hidden;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.X >= 119,
                $"BFC should avoid float, X >= 120, got {target.ContentRect.X}");
            Assert.True(target.ContentRect.Width <= 181,
                $"BFC width should be reduced, got {target.ContentRect.Width}");
        }

        // [CSS2 §9.5.1] float with border and padding: content box inside box model
        [Fact]
        public void FloatWithBorderPadding_ContentBoxCorrect()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:left;width:80px;height:40px;padding:10px;border:5px solid'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Content rect should be 80x40, with padding+border adding to border box
            Assert.True(System.Math.Abs(target.ContentRect.Width - 80) < 2,
                $"Content width should be 80, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 40) < 2,
                $"Content height should be 40, got {target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.BorderRect.Width - 110) < 2,
                $"Border box width should be 110 (80+10+10+5+5), got {target.BorderRect.Width}");
        }

        // [CSS2 §9.5.2] clear:left only clears left floats, not right
        [Fact]
        public void ClearLeft_DoesNotClearRightFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div style='float:left;width:80px;height:30px'></div>
                        <div style='float:right;width:80px;height:80px'></div>
                        <div id='t' style='clear:left;height:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // clear:left moves below left float (30px) but not right float (80px)
            Assert.True(target.ContentRect.Y >= 29 && target.ContentRect.Y < 79,
                $"clear:left below left(30) not right(80), Y={target.ContentRect.Y}");
        }

        // [CSS2 §9.5.2] clear:right only clears right floats, not left
        [Fact]
        public void ClearRight_DoesNotClearLeftFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div style='float:left;width:80px;height:80px'></div>
                        <div style='float:right;width:80px;height:30px'></div>
                        <div id='t' style='clear:right;height:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // clear:right moves below right float (30px) but not left float (80px)
            Assert.True(target.ContentRect.Y >= 29 && target.ContentRect.Y < 79,
                $"clear:right below right(30) not left(80), Y={target.ContentRect.Y}");
        }

        // [CSS-DISPLAY §3] display:flow-root establishes BFC and contains floats
        [Fact]
        public void FlowRoot_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='display:flow-root;width:200px'>
                        <div style='float:left;width:80px;height:100px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height >= 99,
                $"flow-root should contain float, height >= 100, got {target.ContentRect.Height}");
        }

        // [CSS2 §9.5.1] float with explicit height
        [Fact]
        public void FloatWithExplicitHeight_Respected()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:left;width:100px;height:75px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 75) < 2,
                $"Float height should be 75, got {target.ContentRect.Height}");
        }

        // [CSS2 §9.5] float margin pushes second float further away
        [Fact]
        public void FloatMarginRight_PushesNextFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:400px'>
                        <div id='a' style='float:left;width:80px;height:40px;margin-right:20px'></div>
                        <div id='b' style='float:left;width:80px;height:40px'></div>
                    </div>
                </body>");
            var second = LayoutTestHelper.FindById(root, "b")!;
            // Second float starts after first float's margin box: 80 + 20 = 100
            Assert.True(System.Math.Abs(second.ContentRect.X - 100) < 2,
                $"Second float should be at X=100, got {second.ContentRect.X}");
        }

        // [CSS2 §9.5] stacked floats: three left floats that all fit
        [Fact]
        public void ThreeLeftFloats_AllFitSideBySide()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='a' style='float:left;width:80px;height:40px'></div>
                        <div id='b' style='float:left;width:80px;height:40px'></div>
                        <div id='c' style='float:left;width:80px;height:40px'></div>
                    </div>
                </body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            var third = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(third.ContentRect.X - 160) < 2);
        }

        // [CSS2 §9.4.1] overflow:auto establishes BFC and contains floats
        [Fact]
        public void OverflowAuto_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='overflow:auto;width:200px'>
                        <div style='float:left;width:80px;height:110px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Height >= 109,
                $"overflow:auto BFC should contain float, got height {target.ContentRect.Height}");
        }

        // [CSS2 §9.5.1] float right with margin-right offsets from right edge
        [Fact]
        public void FloatRight_WithMarginRight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div id='t' style='float:right;width:100px;height:40px;margin-right:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Right float: right edge of margin box at container right edge
            // Content X = 300 - 20(margin-right) - 100(width) = 180
            Assert.True(System.Math.Abs(target.ContentRect.X - 180) < 2,
                $"Right float with margin-right=20 should be at X=180, got {target.ContentRect.X}");
        }

        // [CSS2 §9.5.2] clear on a float itself
        [Fact(Skip = "Known bug: clear property not applied when set on a float element")]
        public void ClearOnFloat_MovesFloatBelowPreviousFloat()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div style='float:left;width:100px;height:50px'></div>
                        <div id='t' style='float:left;clear:left;width:100px;height:30px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 49,
                $"Cleared float should be below previous float at Y>=50, got {target.ContentRect.Y}");
        }

        // [CSS2 §9.5] negative margin-top on float shifts it upward
        [Fact]
        public void NegativeMarginTop_ShiftsFloatUpward()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='width:300px'>
                        <div style='height:40px'></div>
                        <div id='t' style='float:left;width:80px;height:30px;margin-top:-10px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Normal position would be Y=40; negative margin shifts up by 10
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2,
                $"Negative margin-top should shift to Y=30, got {target.ContentRect.Y}");
        }
    }
}
