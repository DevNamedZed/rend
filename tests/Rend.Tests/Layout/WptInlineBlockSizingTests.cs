using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptInlineBlockSizingTests
    {
        private readonly ITestOutputHelper _output;

        public WptInlineBlockSizingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.3.9] inline-block respects explicit width
        [Fact]
        public void InlineBlock_RespectsExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:120px;height:20px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2,
                $"inline-block should respect width:120px (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.6.6] inline-block respects explicit height
        [Fact]
        public void InlineBlock_RespectsExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:80px;height:50px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2,
                $"inline-block should respect height:50px (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.3.9] inline-block shrink-to-fit when no width set
        [Fact]
        public void InlineBlock_ShrinkToFitWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-block'>
                        <div style='width:75px;height:10px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 75) < 2,
                $"inline-block should shrink-to-fit child (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.3.9] inline-block auto width from wider child content
        [Fact]
        public void InlineBlock_AutoWidthFromContent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-block'>
                        <div style='width:50px;height:10px'></div>
                        <div style='width:110px;height:10px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 110) < 2,
                $"inline-block auto width = widest child (got {box.ContentRect.Width})");
        }

        // [CSS2 §9.4.2] two inline-blocks on same line
        [Fact]
        public void TwoInlineBlocks_SameLine()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <span id='a' style='display:inline-block;width:100px;height:30px'></span>
                    <span id='b' style='display:inline-block;width:100px;height:30px'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a");
            var boxB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(boxA);
            Assert.NotNull(boxB);
            _output.WriteLine($"a.Y={boxA!.ContentRect.Y} b.Y={boxB!.ContentRect.Y}");
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - boxB.ContentRect.Y) < 2,
                "two inline-blocks should be on the same line (same Y)");
        }

        // [CSS2 §9.4.2] inline-block wraps to next line when exceeding container width
        [Fact]
        public void InlineBlock_WrapsToNextLine()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <span id='a' style='display:inline-block;width:150px;height:30px'></span>
                    <span id='b' style='display:inline-block;width:150px;height:30px'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a");
            var boxB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(boxA);
            Assert.NotNull(boxB);
            _output.WriteLine($"a.Y={boxA!.ContentRect.Y} b.Y={boxB!.ContentRect.Y}");
            Assert.True(boxB.ContentRect.Y > boxA.ContentRect.Y + 10,
                $"second inline-block should wrap to next line (a.Y={boxA.ContentRect.Y}, b.Y={boxB.ContentRect.Y})");
        }

        // [CSS2 §8.3] inline-block with horizontal margins
        [Fact]
        public void InlineBlock_WithMargin()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:100px;height:20px;margin:10px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} margin-box-w={box.MarginRect.Width}");
            Assert.True(box.ContentRect.X >= 9,
                $"inline-block should have left margin offset (got x={box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.MarginRect.Width - 120) < 2,
                $"margin-box width = 100 + 10 + 10 (got {box.MarginRect.Width})");
        }

        // [CSS2 §8.4] inline-block with padding
        [Fact]
        public void InlineBlock_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:100px;height:20px;padding:15px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"padding-box-w={box!.PaddingRect.Width} padding-box-h={box.PaddingRect.Height}");
            Assert.True(System.Math.Abs(box.PaddingRect.Width - 130) < 2,
                $"padding-box width = 100 + 15 + 15 (got {box.PaddingRect.Width})");
            Assert.True(System.Math.Abs(box.PaddingRect.Height - 50) < 2,
                $"padding-box height = 20 + 15 + 15 (got {box.PaddingRect.Height})");
        }

        // [CSS2 §8.5] inline-block with border
        [Fact]
        public void InlineBlock_WithBorder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:100px;height:20px;border:5px solid black'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"border-box-w={box!.BorderRect.Width} border-box-h={box.BorderRect.Height}");
            Assert.True(System.Math.Abs(box.BorderRect.Width - 110) < 2,
                $"border-box width = 100 + 5 + 5 (got {box.BorderRect.Width})");
            Assert.True(System.Math.Abs(box.BorderRect.Height - 30) < 2,
                $"border-box height = 20 + 5 + 5 (got {box.BorderRect.Height})");
        }

        // [CSS2 §10.8.1] vertical-align:top aligns top of inline-block to line box top
        [Fact]
        public void InlineBlock_VerticalAlignTop()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px;line-height:80px'>
                    <span id='t' style='display:inline-block;width:30px;height:30px;vertical-align:top'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"Y={box!.ContentRect.Y}");
            Assert.True(box.ContentRect.Y < 5,
                $"vertical-align:top should place box near top (got Y={box.ContentRect.Y})");
        }

        // [CSS2 §10.8.1] vertical-align:middle positions relative to parent baseline + x-height/2
        [Fact]
        public void InlineBlock_VerticalAlignMiddle()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px;line-height:80px'>
                    <span id='top' style='display:inline-block;width:20px;height:20px;vertical-align:top'></span>
                    <span id='mid' style='display:inline-block;width:20px;height:20px;vertical-align:middle'></span>
                </div></body>");
            var topBox = LayoutTestHelper.FindById(root, "top");
            var midBox = LayoutTestHelper.FindById(root, "mid");
            Assert.NotNull(topBox);
            Assert.NotNull(midBox);
            _output.WriteLine($"top.Y={topBox!.ContentRect.Y} mid.Y={midBox!.ContentRect.Y}");
            Assert.True(midBox.ContentRect.Y > topBox.ContentRect.Y,
                $"vertical-align:middle should be lower than top (top.Y={topBox.ContentRect.Y}, mid.Y={midBox.ContentRect.Y})");
        }

        // [CSS2 §10.8.1] vertical-align:bottom aligns bottom of inline-block to line box bottom
        [Fact]
        public void InlineBlock_VerticalAlignBottom()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='container' style='width:300px;line-height:80px'>
                    <span id='t' style='display:inline-block;width:30px;height:30px;vertical-align:bottom'></span>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            Assert.NotNull(box);
            float containerBottom = container!.ContentRect.Y + container.ContentRect.Height;
            float boxBottom = box!.ContentRect.Y + box.ContentRect.Height;
            _output.WriteLine($"container-bottom={containerBottom} box-bottom={boxBottom}");
            Assert.True(System.Math.Abs(containerBottom - boxBottom) < 5,
                $"vertical-align:bottom box should align near container bottom (container={containerBottom}, box={boxBottom})");
        }

        // [CSS2 §11.1] inline-block with overflow:hidden clips content
        [Fact]
        public void InlineBlock_OverflowHidden()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-block;width:80px;height:40px;overflow:hidden'>
                        <div style='width:200px;height:200px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width} h={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 80) < 2,
                $"overflow:hidden should not expand width (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 40) < 2,
                $"overflow:hidden should not expand height (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.2] inline-block percentage width relative to containing block
        [Fact]
        public void InlineBlock_PercentageWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <span id='t' style='display:inline-block;width:50%;height:20px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2,
                $"inline-block 50% of 300px = 150 (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.4] inline-block min-width constraint
        [Fact]
        public void InlineBlock_MinWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-block;min-width:100px;height:20px'>
                        <div style='width:40px;height:10px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 99,
                $"inline-block min-width should enforce minimum (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.4] inline-block max-width constraint
        [Fact]
        public void InlineBlock_MaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-block;max-width:60px;height:20px'>
                        <div style='width:200px;height:10px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 62,
                $"inline-block max-width should clamp (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.7] inline-block min-height constraint
        [Fact]
        public void InlineBlock_MinHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-block;width:80px;min-height:60px'>
                        <div style='height:10px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(box.ContentRect.Height >= 59,
                $"inline-block min-height should enforce minimum (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.7] inline-block max-height constraint
        [Fact]
        public void InlineBlock_MaxHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-block;width:80px;max-height:30px;overflow:hidden'>
                        <div style='height:200px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(box.ContentRect.Height <= 32,
                $"inline-block max-height should clamp (got {box.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4] inline-block inside flex container becomes flex item
        [Fact]
        public void InlineBlock_InsideFlex_BecomesFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px;height:60px'>
                    <div id='t' style='display:inline-block;width:100px;height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width} h={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2,
                $"flex item should keep explicit width (got {box.ContentRect.Width})");
        }

        // [CSS-GRID §6] inline-block inside grid container becomes grid item
        [Fact]
        public void InlineBlock_InsideGrid_BecomesGridItem()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:300px'>
                    <div id='t' style='display:inline-block;height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"grid item should fill column track (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.6.6] inline-block auto height determined by children
        [Fact]
        public void InlineBlock_AutoHeightFromChildren()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-block;width:100px'>
                        <div style='height:25px'></div>
                        <div style='height:35px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2,
                $"inline-block auto height = sum of children (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.3.9] inline-block with multiple block children sizes to widest
        [Fact]
        public void InlineBlock_WithChildren_WidestChild()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-block'>
                        <div style='width:60px;height:10px'></div>
                        <div style='width:130px;height:10px'></div>
                        <div style='width:90px;height:10px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 130) < 2,
                $"inline-block should be as wide as widest child (got {box.ContentRect.Width})");
        }

        // [CSS2 §16.2] text-align:center centers inline-blocks
        [Fact]
        public void InlineBlocks_CenteredViaTextAlign()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px;text-align:center'>
                    <span id='t' style='display:inline-block;width:100px;height:20px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 5,
                $"text-align:center should center inline-block (got x={box.ContentRect.X}, expected ~100)");
        }

        // [CSS2 §16.2] text-align:right right-aligns inline-blocks
        [Fact]
        public void InlineBlocks_RightAlignedViaTextAlign()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px;text-align:right'>
                    <span id='t' style='display:inline-block;width:100px;height:20px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 200) < 5,
                $"text-align:right should right-align (got x={box.ContentRect.X}, expected ~200)");
        }

        // [CSS2 §8.3] inline-block margin does not collapse
        [Fact]
        public void InlineBlock_MarginDoesNotCollapse()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <span id='a' style='display:inline-block;width:80px;height:30px;margin-right:20px'></span>
                    <span id='b' style='display:inline-block;width:80px;height:30px;margin-left:20px'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a");
            var boxB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(boxA);
            Assert.NotNull(boxB);
            float gap = boxB!.ContentRect.X - (boxA!.ContentRect.X + boxA.ContentRect.Width);
            _output.WriteLine($"gap={gap} a.right={boxA.ContentRect.X + boxA.ContentRect.Width} b.left={boxB.ContentRect.X}");
            Assert.True(gap >= 35,
                $"inline-block margins should not collapse (gap={gap}, expected ~40+)");
        }

        // [CSS2 §10.3.9] inline-block with padding and border affects layout size
        [Fact]
        public void InlineBlock_PaddingAndBorder_AffectsLayoutSize()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:100px;height:40px;padding:10px;border:3px solid black'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"content={box!.ContentRect.Width}x{box.ContentRect.Height} border-box={box.BorderRect.Width}x{box.BorderRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2,
                $"content width should be 100 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.BorderRect.Width - 126) < 2,
                $"border-box width = 100 + 10*2 + 3*2 = 126 (got {box.BorderRect.Width})");
            Assert.True(System.Math.Abs(box.BorderRect.Height - 66) < 2,
                $"border-box height = 40 + 10*2 + 3*2 = 66 (got {box.BorderRect.Height})");
        }

        // [CSS2 §10.3.9] inline-block box-sizing:border-box includes padding+border in width
        [Fact]
        public void InlineBlock_BoxSizingBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:100px;height:50px;padding:10px;border:5px solid black;box-sizing:border-box'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"border-box-w={box!.BorderRect.Width} content-w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.BorderRect.Width - 100) < 2,
                $"border-box width should be 100 (got {box.BorderRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 70) < 2,
                $"content width = 100 - 10*2 - 5*2 = 70 (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.6.6] inline-block auto height with single child
        [Fact]
        public void InlineBlock_AutoHeightSingleChild()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-block;width:80px'>
                        <div style='height:45px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 45) < 2,
                $"inline-block auto height = child height (got {box.ContentRect.Height})");
        }

        // [CSS2 §9.4.2] three inline-blocks fitting on one line
        [Fact]
        public void ThreeInlineBlocks_FitOnOneLine()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='a' style='display:inline-block;width:100px;height:20px'></span>
                    <span id='b' style='display:inline-block;width:100px;height:20px'></span>
                    <span id='c' style='display:inline-block;width:100px;height:20px'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a");
            var boxB = LayoutTestHelper.FindById(root, "b");
            var boxC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(boxA);
            Assert.NotNull(boxB);
            Assert.NotNull(boxC);
            _output.WriteLine($"a.Y={boxA!.ContentRect.Y} b.Y={boxB!.ContentRect.Y} c.Y={boxC!.ContentRect.Y}");
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - boxC.ContentRect.Y) < 2,
                "all three inline-blocks should share the same line");
        }

        // [CSS2 §10.3.9] inline-block percentage width resolves against containing block
        [Fact]
        public void InlineBlock_PercentageWidthResolution()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <span id='t' style='display:inline-block;width:25%;height:20px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 50) < 2,
                $"25% of 200px = 50 (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.6.6] inline-block with nested inline-block
        [Fact]
        public void InlineBlock_NestedInlineBlock()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='outer' style='display:inline-block'>
                        <div id='inner' style='display:inline-block;width:90px;height:35px'></div>
                    </div>
                </div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer");
            var inner = LayoutTestHelper.FindById(root, "inner");
            Assert.NotNull(outer);
            Assert.NotNull(inner);
            _output.WriteLine($"outer.w={outer!.ContentRect.Width} inner.w={inner!.ContentRect.Width}");
            Assert.True(System.Math.Abs(inner.ContentRect.Width - 90) < 2,
                $"inner inline-block width should be 90 (got {inner.ContentRect.Width})");
            Assert.True(outer.ContentRect.Width >= 89,
                $"outer should be at least as wide as inner (got {outer.ContentRect.Width})");
        }

        // [CSS2 §10.3.9] inline-block with margin auto resolves to 0
        [Fact]
        public void InlineBlock_MarginAutoResolvesToZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:100px;height:20px;margin:auto'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"x={box!.ContentRect.X} margin-left={box.MarginLeft} margin-right={box.MarginRight}");
            Assert.True(box.ContentRect.X < 5,
                $"inline-block margin:auto should resolve to 0, not center (got x={box.ContentRect.X})");
        }

        // [CSS2 §10.8.1] inline-blocks with different heights on same line
        [Fact]
        public void InlineBlocks_DifferentHeights_SameLine()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='container' style='width:400px;font-size:0'>
                    <span id='tall' style='display:inline-block;width:50px;height:80px;vertical-align:top'></span>
                    <span id='short' style='display:inline-block;width:50px;height:30px;vertical-align:top'></span>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container");
            var tall = LayoutTestHelper.FindById(root, "tall");
            var shortBox = LayoutTestHelper.FindById(root, "short");
            Assert.NotNull(container);
            Assert.NotNull(tall);
            Assert.NotNull(shortBox);
            _output.WriteLine($"container.h={container!.ContentRect.Height} tall.h={tall!.ContentRect.Height} short.h={shortBox!.ContentRect.Height}");
            Assert.True(container.ContentRect.Height >= 79,
                $"line height should accommodate tallest inline-block (got {container.ContentRect.Height})");
        }
    }
}
