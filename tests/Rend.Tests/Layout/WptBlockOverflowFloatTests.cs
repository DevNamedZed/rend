using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests covering the interaction between block formatting contexts (overflow, flow-root),
    /// float containment, float positioning, and float clearing per CSS2 and CSS Display L3.
    /// </summary>
    public class WptBlockOverflowFloatTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockOverflowFloatTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §9.4.1] overflow:hidden establishes BFC that contains floats in parent height
        [Fact]
        public void OverflowHidden_ContainsFloats_ParentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='overflow:hidden;width:200px'>
                    <div style='float:left;width:80px;height:100px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(container.ContentRect.Height >= 99,
                $"overflow:hidden should contain float height (got {container.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] overflow:auto establishes BFC that contains floats
        [Fact]
        public void OverflowAuto_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='overflow:auto;width:200px'>
                    <div style='float:left;width:60px;height:90px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(container.ContentRect.Height >= 89,
                $"overflow:auto should contain float (got {container.ContentRect.Height})");
        }

        // [CSS-DISPLAY §3] display:flow-root establishes BFC that contains floats
        [Fact]
        public void FlowRoot_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='display:flow-root;width:200px'>
                    <div style='float:left;width:70px;height:110px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(container.ContentRect.Height >= 109,
                $"flow-root should contain float (got {container.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] overflow:visible does NOT establish BFC; does not contain floats
        [Fact]
        public void OverflowVisible_DoesNotContainFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='overflow:visible;width:200px'>
                    <div style='float:left;width:80px;height:100px'></div>
                    <div style='height:10px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            // Normal block with overflow:visible should have auto height based on in-flow content only (10px)
            Assert.True(container.ContentRect.Height <= 15,
                $"overflow:visible should not contain float (got {container.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] overflow:scroll establishes BFC that contains floats
        [Fact]
        public void OverflowScroll_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='overflow:scroll;width:200px'>
                    <div style='float:left;width:60px;height:85px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(container.ContentRect.Height >= 84,
                $"overflow:scroll should contain float (got {container.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] BFC avoids adjacent left float
        [Fact]
        public void BfcAvoidsAdjacentFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:100px;height:60px'></div>
                    <div id='bfc' style='overflow:hidden;height:40px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc X={bfc.ContentRect.X}, width={bfc.ContentRect.Width}");
            Assert.True(bfc.ContentRect.X >= 99,
                $"BFC should avoid float (X={bfc.ContentRect.X})");
        }

        // [CSS2 §9.4.1] BFC with padding still contains floats
        [Fact]
        public void BfcWithPadding_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='overflow:hidden;width:200px;padding:10px'>
                    <div style='float:left;width:60px;height:80px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            float totalHeight = container.ContentRect.Height;
            _output.WriteLine($"container height={totalHeight}");
            Assert.True(totalHeight >= 79,
                $"BFC with padding should contain float in content area (got {totalHeight})");
        }

        // [CSS2 §9.4.1] BFC with border still contains floats
        [Fact]
        public void BfcWithBorder_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='overflow:hidden;width:200px;border:5px solid black'>
                    <div style='float:left;width:60px;height:75px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            float contentHeight = container.ContentRect.Height;
            _output.WriteLine($"container content height={contentHeight}");
            Assert.True(contentHeight >= 74,
                $"BFC with border should contain float (got {contentHeight})");
        }

        // [CSS2 §9.5.1] float:left positioned at left content edge
        [Fact]
        public void FloatLeft_AtLeftEdge()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='floated' style='float:left;width:80px;height:40px'></div>
                </div></body>");
            var floated = LayoutTestHelper.FindById(root, "floated")!;
            _output.WriteLine($"float X={floated.ContentRect.X}");
            Assert.True(floated.ContentRect.X < 2,
                $"float:left should be at left edge (X={floated.ContentRect.X})");
        }

        // [CSS2 §9.5.1] float:right positioned at right content edge
        [Fact]
        public void FloatRight_AtRightEdge()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='floated' style='float:right;width:80px;height:40px'></div>
                </div></body>");
            var floated = LayoutTestHelper.FindById(root, "floated")!;
            float expectedX = 120;
            _output.WriteLine($"float X={floated.ContentRect.X}, expected={expectedX}");
            Assert.True(System.Math.Abs(floated.ContentRect.X - expectedX) < 2,
                $"float:right should be at right edge (X={floated.ContentRect.X}, expected {expectedX})");
        }

        // [CSS2 §9.5.1] two left floats side by side
        [Fact]
        public void TwoFloatsLeftSideBySide()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='first' style='float:left;width:80px;height:40px'></div>
                    <div id='second' style='float:left;width:80px;height:40px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"first X={first.ContentRect.X}, second X={second.ContentRect.X}");
            Assert.True(first.ContentRect.X < 2, $"First float at left edge (X={first.ContentRect.X})");
            Assert.True(System.Math.Abs(second.ContentRect.X - 80) < 2,
                $"Second float right of first (X={second.ContentRect.X})");
            Assert.True(System.Math.Abs(second.ContentRect.Y - first.ContentRect.Y) < 2,
                $"Both floats on same line (Y diff={second.ContentRect.Y - first.ContentRect.Y})");
        }

        // [CSS2 §9.5.2] clear:left moves below left float
        [Fact]
        public void ClearLeft_MovesBelowLeftFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='float:left;width:80px;height:50px'></div>
                    <div id='cleared' style='clear:left;height:20px'></div>
                </div></body>");
            var cleared = LayoutTestHelper.FindById(root, "cleared")!;
            _output.WriteLine($"cleared Y={cleared.ContentRect.Y}");
            Assert.True(cleared.ContentRect.Y >= 49,
                $"clear:left should move below float (Y={cleared.ContentRect.Y})");
        }

        // [CSS2 §9.5.2] clear:right moves below right float
        [Fact]
        public void ClearRight_MovesBelowRightFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='float:right;width:80px;height:60px'></div>
                    <div id='cleared' style='clear:right;height:20px'></div>
                </div></body>");
            var cleared = LayoutTestHelper.FindById(root, "cleared")!;
            _output.WriteLine($"cleared Y={cleared.ContentRect.Y}");
            Assert.True(cleared.ContentRect.Y >= 59,
                $"clear:right should move below right float (Y={cleared.ContentRect.Y})");
        }

        // [CSS2 §9.5.2] clear:both moves below all floats
        [Fact]
        public void ClearBoth_MovesBelowAllFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:80px;height:40px'></div>
                    <div style='float:right;width:80px;height:70px'></div>
                    <div id='cleared' style='clear:both;height:20px'></div>
                </div></body>");
            var cleared = LayoutTestHelper.FindById(root, "cleared")!;
            _output.WriteLine($"cleared Y={cleared.ContentRect.Y}");
            Assert.True(cleared.ContentRect.Y >= 69,
                $"clear:both should move below tallest float (Y={cleared.ContentRect.Y})");
        }

        // [CSS2 §10.3.5] float with percentage width resolves against containing block
        [Fact]
        public void FloatPercentageWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='floated' style='float:left;width:50%;height:30px'></div>
                </div></body>");
            var floated = LayoutTestHelper.FindById(root, "floated")!;
            _output.WriteLine($"float width={floated.ContentRect.Width}");
            Assert.True(System.Math.Abs(floated.ContentRect.Width - 100) < 2,
                $"50% of 200 should be 100 (got {floated.ContentRect.Width})");
        }

        // [CSS2 §10.3.5] float shrink-to-fit: auto width sizes to content
        [Fact]
        public void FloatShrinkToFit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='floated' style='float:left'>
                        <div style='width:120px;height:30px'></div>
                    </div>
                </div></body>");
            var floated = LayoutTestHelper.FindById(root, "floated")!;
            _output.WriteLine($"float width={floated.ContentRect.Width}");
            Assert.True(System.Math.Abs(floated.ContentRect.Width - 120) < 2,
                $"Shrink-to-fit float should wrap content (got {floated.ContentRect.Width})");
        }

        // [CSS2 §9.5] float with margin offsets from edge
        [Fact]
        public void FloatWithMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='floated' style='float:left;width:60px;height:40px;margin:15px'></div>
                </div></body>");
            var floated = LayoutTestHelper.FindById(root, "floated")!;
            _output.WriteLine($"float X={floated.ContentRect.X}, Y={floated.ContentRect.Y}");
            Assert.True(System.Math.Abs(floated.ContentRect.X - 15) < 2,
                $"Margin-left offsets float (X={floated.ContentRect.X})");
            Assert.True(System.Math.Abs(floated.ContentRect.Y - 15) < 2,
                $"Margin-top offsets float (Y={floated.ContentRect.Y})");
        }

        // [CSS2 §9.5.1] float with negative margin extends beyond container edge
        [Fact]
        public void FloatNegativeMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='floated' style='float:left;width:60px;height:40px;margin-left:-10px'></div>
                </div></body>");
            var floated = LayoutTestHelper.FindById(root, "floated")!;
            _output.WriteLine($"float X={floated.ContentRect.X}");
            Assert.True(floated.ContentRect.X < 0,
                $"Negative margin-left should pull float left of container (X={floated.ContentRect.X})");
        }

        // [CSS2 §9.4.1] float inside overflow:hidden is clipped by BFC
        [Fact]
        public void FloatInsideOverflowHidden()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='overflow:hidden;width:200px;height:50px'>
                    <div id='floated' style='float:left;width:80px;height:100px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            var floated = LayoutTestHelper.FindById(root, "floated")!;
            _output.WriteLine($"container height={container.ContentRect.Height}, float height={floated.ContentRect.Height}");
            // Container has explicit height, float is taller but container stays at 50
            Assert.True(System.Math.Abs(container.ContentRect.Height - 50) < 2,
                $"Explicit height should not grow (got {container.ContentRect.Height})");
            Assert.True(System.Math.Abs(floated.ContentRect.Height - 100) < 2,
                $"Float keeps its own height (got {floated.ContentRect.Height})");
        }

        // [CSS2 §10.4] float with min-width
        [Fact]
        public void FloatMinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='floated' style='float:left;min-width:100px;height:30px'>
                        <div style='width:50px;height:10px'></div>
                    </div>
                </div></body>");
            var floated = LayoutTestHelper.FindById(root, "floated")!;
            _output.WriteLine($"float width={floated.ContentRect.Width}");
            Assert.True(floated.ContentRect.Width >= 99,
                $"min-width should enforce minimum (got {floated.ContentRect.Width})");
        }

        // [CSS2 §10.4] float with max-width
        [Fact]
        public void FloatMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='floated' style='float:left;max-width:50px;height:30px'>
                        <div style='width:100px;height:10px'></div>
                    </div>
                </div></body>");
            var floated = LayoutTestHelper.FindById(root, "floated")!;
            _output.WriteLine($"float width={floated.ContentRect.Width}");
            Assert.True(floated.ContentRect.Width <= 51,
                $"max-width should cap width (got {floated.ContentRect.Width})");
        }

        // [CSS2 §9.4.1] inline-block BFC contains floats
        [Fact]
        public void InlineBlockBfc_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <span id='inlineBlock' style='display:inline-block;width:150px'>
                        <div style='float:left;width:60px;height:70px'></div>
                    </span>
                </div></body>");
            var inlineBlock = LayoutTestHelper.FindById(root, "inlineBlock")!;
            _output.WriteLine($"inline-block height={inlineBlock.ContentRect.Height}");
            Assert.True(inlineBlock.ContentRect.Height >= 69,
                $"inline-block BFC should contain float (got {inlineBlock.ContentRect.Height})");
        }

        // [CSS2 §17.5.4] table-cell BFC contains floats
        [Fact]
        public void TableCellBfc_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='border-collapse:collapse'>
                    <tr>
                        <td id='cell' style='width:150px'>
                            <div style='float:left;width:60px;height:65px'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            _output.WriteLine($"table-cell height={cell.ContentRect.Height}");
            Assert.True(cell.ContentRect.Height >= 64,
                $"table-cell BFC should contain float (got {cell.ContentRect.Height})");
        }

        // [CSS2 §9.4.1] overflow:hidden with float + in-flow content uses max height
        [Fact]
        public void OverflowHidden_FloatAndContent_UsesMaxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='overflow:hidden;width:200px'>
                    <div style='float:left;width:80px;height:120px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(container.ContentRect.Height >= 119,
                $"BFC height should be max(float, content) = 120 (got {container.ContentRect.Height})");
        }

        // [CSS2 §9.5.2] clear:left does not affect right float
        [Fact]
        public void ClearLeft_DoesNotAffectRightFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:80px;height:40px'></div>
                    <div style='float:right;width:80px;height:80px'></div>
                    <div id='cleared' style='clear:left;height:20px'></div>
                </div></body>");
            var cleared = LayoutTestHelper.FindById(root, "cleared")!;
            _output.WriteLine($"cleared Y={cleared.ContentRect.Y}");
            Assert.True(cleared.ContentRect.Y >= 39 && cleared.ContentRect.Y < 79,
                $"clear:left below left(40) not right(80) (Y={cleared.ContentRect.Y})");
        }

        // [CSS2 §9.5.1] float with margin-bottom affects clearance
        [Fact]
        public void FloatMarginBottom_AffectsClearance()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='float:left;width:80px;height:50px;margin-bottom:20px'></div>
                    <div id='cleared' style='clear:left;height:20px'></div>
                </div></body>");
            var cleared = LayoutTestHelper.FindById(root, "cleared")!;
            _output.WriteLine($"cleared Y={cleared.ContentRect.Y}");
            Assert.True(cleared.ContentRect.Y >= 69,
                $"Clearance includes float margin-bottom (Y={cleared.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] BFC avoids right float
        [Fact(Skip = "Known bug: BFC shrink to avoid right float")]
        public void BfcAvoidsRightFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:right;width:100px;height:60px'></div>
                    <div id='bfc' style='overflow:hidden;height:40px'></div>
                </div></body>");
            var bfc = LayoutTestHelper.FindById(root, "bfc")!;
            _output.WriteLine($"bfc width={bfc.ContentRect.Width}");
            Assert.True(bfc.ContentRect.Width <= 201,
                $"BFC should shrink to avoid right float (width={bfc.ContentRect.Width})");
        }

        // [CSS-DISPLAY §3] flow-root contains multiple floats
        [Fact]
        public void FlowRoot_ContainsMultipleFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='display:flow-root;width:300px'>
                    <div style='float:left;width:80px;height:50px'></div>
                    <div style='float:right;width:80px;height:90px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(container.ContentRect.Height >= 89,
                $"flow-root should contain tallest float (got {container.ContentRect.Height})");
        }

        // [CSS2 §9.5.1] float right with margin-right offsets from right edge
        [Fact]
        public void FloatRight_WithMarginRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='floated' style='float:right;width:60px;height:40px;margin-right:20px'></div>
                </div></body>");
            var floated = LayoutTestHelper.FindById(root, "floated")!;
            float rightEdge = floated.ContentRect.X + floated.ContentRect.Width;
            _output.WriteLine($"float right edge={rightEdge}, container width=200");
            Assert.True(System.Math.Abs(rightEdge - 180) < 2,
                $"float:right with margin-right 20 should end at 180 (got {rightEdge})");
        }

        // [CSS2 §9.5.1] left and right floats side by side in wide container
        [Fact]
        public void LeftAndRightFloats_SideBySide()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='left' style='float:left;width:80px;height:40px'></div>
                    <div id='right' style='float:right;width:80px;height:40px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            _output.WriteLine($"left X={left.ContentRect.X}, right X={right.ContentRect.X}");
            Assert.True(left.ContentRect.X < 2,
                $"Left float at left edge (X={left.ContentRect.X})");
            Assert.True(System.Math.Abs(right.ContentRect.X - 220) < 2,
                $"Right float at right edge (X={right.ContentRect.X})");
            Assert.True(System.Math.Abs(left.ContentRect.Y - right.ContentRect.Y) < 2,
                $"Both floats on same Y (diff={left.ContentRect.Y - right.ContentRect.Y})");
        }

        // [CSS2 §9.4.1] BFC with padding and border contains float, height includes float
        [Fact]
        public void BfcWithPaddingAndBorder_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='overflow:hidden;width:200px;padding:10px;border:5px solid black'>
                    <div style='float:left;width:50px;height:60px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            _output.WriteLine($"container content height={container.ContentRect.Height}");
            Assert.True(container.ContentRect.Height >= 59,
                $"BFC with padding+border should contain float (got {container.ContentRect.Height})");
        }

        // [CSS2 §9.5.1] float with explicit width + padding is content-box by default
        [Fact]
        public void FloatWidth_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='floated' style='float:left;width:100px;padding:10px;height:30px'></div>
                </div></body>");
            var floated = LayoutTestHelper.FindById(root, "floated")!;
            _output.WriteLine($"float content width={floated.ContentRect.Width}");
            Assert.True(System.Math.Abs(floated.ContentRect.Width - 100) < 2,
                $"Content-box: content width should be 100 (got {floated.ContentRect.Width})");
        }

        // [CSS2 §9.5.2] clear:both with no floats has no effect
        [Fact]
        public void ClearBoth_NoFloats_NoEffect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px'></div>
                    <div id='cleared' style='clear:both;height:20px'></div>
                </div></body>");
            var cleared = LayoutTestHelper.FindById(root, "cleared")!;
            _output.WriteLine($"cleared Y={cleared.ContentRect.Y}");
            Assert.True(System.Math.Abs(cleared.ContentRect.Y - 30) < 2,
                $"clear:both with no floats should be at normal position (Y={cleared.ContentRect.Y})");
        }
    }
}
