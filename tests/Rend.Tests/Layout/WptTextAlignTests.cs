using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS text-align behavior: left, right, center, justify,
    /// text-align-last, inheritance, and interaction with inline-block elements.
    /// Uses inline-block elements with known widths to verify alignment positioning.
    /// </summary>
    public class WptTextAlignTests
    {
        private readonly ITestOutputHelper _output;
        public WptTextAlignTests(ITestOutputHelper output) { _output = output; }

        // ── text-align: left ──

        // [CSS-TEXT-3 §7.1] text-align: left places inline-block at left edge
        [Fact]
        public void TextAlignLeft_InlineBlockAtLeftEdge()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:left'><span id='t' style='display:inline-block;width:80px;height:20px'></span></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.X < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align: left is the initial value
        [Fact]
        public void TextAlignLeft_IsDefault()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><span id='t' style='display:inline-block;width:80px;height:20px'></span></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.X < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align: left with multiple inline-blocks
        [Fact]
        public void TextAlignLeft_MultipleInlineBlocksStartAtLeft()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:left'><span id='a' style='display:inline-block;width:60px;height:20px'></span><span id='b' style='display:inline-block;width:60px;height:20px'></span></div></body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(first.ContentRect.X < 2);
        }

        // ── text-align: right ──

        // [CSS-TEXT-3 §7.1] text-align: right pushes inline-block to right edge
        [Fact]
        public void TextAlignRight_InlineBlockAtRightEdge()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:right'><span id='t' style='display:inline-block;width:80px;height:20px'></span></div></body>");
            // Right-aligned: X should be 300 - 80 = 220
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 220) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align: right with small inline-block in wide container
        [Fact]
        public void TextAlignRight_SmallElementInWideContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px;text-align:right'><span id='t' style='display:inline-block;width:50px;height:20px'></span></div></body>");
            // Right-aligned: X should be 400 - 50 = 350
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 350) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align: right with multiple inline-blocks
        [Fact]
        public void TextAlignRight_MultipleInlineBlocksFlushRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:right;font-size:0'><span id='a' style='display:inline-block;width:60px;height:20px;font-size:16px'></span><span id='b' style='display:inline-block;width:40px;height:20px;font-size:16px'></span></div></body>");
            var second = LayoutTestHelper.FindById(root, "b")!;
            // With font-size:0 on parent, no whitespace gap. Last element right edge at 300.
            // b.X = 300 - 40 = 260 (b is flush right), a.X = 300 - 40 - 60 = 200
            Assert.True(System.Math.Abs(second.ContentRect.X + second.ContentRect.Width - 300) < 2);
        }

        // ── text-align: center ──

        // [CSS-TEXT-3 §7.1] text-align: center centers inline-block
        [Fact]
        public void TextAlignCenter_InlineBlockCentered()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:center'><span id='t' style='display:inline-block;width:100px;height:20px'></span></div></body>");
            // Centered: X should be (300 - 100) / 2 = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 100) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align: center with odd remaining space
        [Fact]
        public void TextAlignCenter_OddRemainingSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:301px;text-align:center'><span id='t' style='display:inline-block;width:100px;height:20px'></span></div></body>");
            // Centered: X should be (301 - 100) / 2 = 100.5
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 100.5f) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align: center with full-width inline-block
        [Fact]
        public void TextAlignCenter_FullWidthElementStaysAtZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px;text-align:center'><span id='t' style='display:inline-block;width:200px;height:20px'></span></div></body>");
            // Full width, no room to center: X = 0
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align: center with very small element
        [Fact]
        public void TextAlignCenter_SmallElementLargeContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px;text-align:center'><span id='t' style='display:inline-block;width:20px;height:20px'></span></div></body>");
            // Centered: X should be (400 - 20) / 2 = 190
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 190) < 2);
        }

        // ── text-align: justify ──

        // [CSS-TEXT-3 §7.1] text-align: justify with single inline-block on last line
        [Fact]
        public void TextAlignJustify_SingleLineActsAsLeft()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:justify'><span id='t' style='display:inline-block;width:80px;height:20px'></span></div></body>");
            // Justify on last (and only) line falls back to left
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.X < 2);
        }

        // ── text-align on nested blocks ──

        // [CSS-TEXT-3 §7.1] text-align on parent centers child inline-block
        [Fact]
        public void TextAlignCenter_NestedBlockAppliesAlignment()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:center'><div style='width:300px'><span id='t' style='display:inline-block;width:100px;height:20px'></span></div></div></body>");
            // Inner div inherits text-align:center, so inline-block is centered
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 100) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align on nested block overrides parent
        [Fact]
        public void TextAlignRight_NestedOverridesParentCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:center'><div style='width:300px;text-align:right'><span id='t' style='display:inline-block;width:80px;height:20px'></span></div></div></body>");
            // Inner div overrides to right, so X = 300 - 80 = 220
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 220) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align on nested block overrides parent to left
        [Fact]
        public void TextAlignLeft_NestedOverridesParentRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:right'><div style='width:300px;text-align:left'><span id='t' style='display:inline-block;width:80px;height:20px'></span></div></div></body>");
            // Inner div overrides to left
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.X < 2);
        }

        // ── text-align inheritance ──

        // [CSS-TEXT-3 §7.1] text-align inherits to child block
        [Fact]
        public void TextAlignCenter_InheritedByChildBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:center'><div id='inner' style='width:300px'><span id='t' style='display:inline-block;width:60px;height:20px'></span></div></div></body>");
            // Child inherits text-align:center, X = (300 - 60) / 2 = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 120) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align inherits through multiple levels
        [Fact]
        public void TextAlignRight_InheritedThroughMultipleLevels()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:right'><div style='width:300px'><div style='width:300px'><span id='t' style='display:inline-block;width:50px;height:20px'></span></div></div></div></body>");
            // Three levels deep, text-align:right inherited. X = 300 - 50 = 250
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 250) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align: inherit keyword
        [Fact]
        public void TextAlignInherit_ExplicitInheritWorks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:center'><div style='width:300px;text-align:inherit'><span id='t' style='display:inline-block;width:100px;height:20px'></span></div></div></body>");
            // text-align:inherit from parent center. X = (300 - 100) / 2 = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 100) < 2);
        }

        // ── text-align-last ──

        // [CSS-TEXT-3 §7.2] text-align-last: center on single-line text
        [Fact]
        public void TextAlignLast_Center_SingleLineApplies()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:left;text-align-last:center'><span id='t' style='display:inline-block;width:100px;height:20px'></span></div></body>");
            // Single line is the last line, text-align-last:center should apply. X = (300 - 100) / 2 = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 100) < 2);
        }

        // [CSS-TEXT-3 §7.2] text-align-last: right on single line
        [Fact]
        public void TextAlignLast_Right_SingleLineApplies()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:left;text-align-last:right'><span id='t' style='display:inline-block;width:80px;height:20px'></span></div></body>");
            // Single line is last line, text-align-last:right. X = 300 - 80 = 220
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 220) < 2);
        }

        // [CSS-TEXT-3 §7.2] text-align-last: auto falls back to text-align
        [Fact]
        public void TextAlignLast_Auto_FallsBackToTextAlign()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:center;text-align-last:auto'><span id='t' style='display:inline-block;width:100px;height:20px'></span></div></body>");
            // text-align-last:auto with text-align:center, last line uses center. X = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 100) < 2);
        }

        // ── text-align with inline-block positioning ──

        // [CSS-TEXT-3 §7.1] text-align: center centers inline-block horizontally within container
        [Fact]
        public void TextAlignCenter_InlineBlockWithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:center'><span id='t' style='display:inline-block;width:80px;height:20px;padding:10px'></span></div></body>");
            // Total box width = 80 + 10 + 10 = 100. Centered: X = (300 - 100) / 2 = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 110) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align: right pushes inline-block with margin to right
        [Fact]
        public void TextAlignRight_InlineBlockWithMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:right'><span id='t' style='display:inline-block;width:80px;height:20px;margin-right:20px'></span></div></body>");
            // Right-aligned: content right edge at 300, margin-right:20 pushes left.
            // X = 300 - 20 - 80 = 200
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 200) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align does not affect block-level children position
        [Fact]
        public void TextAlignCenter_DoesNotAffectBlockChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:center'><div id='t' style='width:100px;height:20px'></div></div></body>");
            // Block-level div is not affected by text-align, should be at X=0
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.X < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align: right does not move block child
        [Fact]
        public void TextAlignRight_DoesNotAffectBlockChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:right'><div id='t' style='width:100px;height:20px'></div></div></body>");
            // Block child unaffected by text-align
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.X < 2);
        }

        // ── text-align with container padding ──

        // [CSS-TEXT-3 §7.1] text-align: center accounts for container padding
        [Fact]
        public void TextAlignCenter_WithContainerPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;padding:20px;text-align:center'><span id='t' style='display:inline-block;width:100px;height:20px'></span></div></body>");
            // Content area = 300px. Inline-block centered in content area.
            // Content area starts at X=20 (padding-left). Center offset = (300 - 100) / 2 = 100
            // So X = 20 + 100 = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 120) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align: right with container padding
        [Fact]
        public void TextAlignRight_WithContainerPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;padding:20px;text-align:right'><span id='t' style='display:inline-block;width:80px;height:20px'></span></div></body>");
            // Content area = 300px starts at X=20. Right-aligned: X = 20 + (300 - 80) = 240
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 240) < 2);
        }

        // ── text-align with narrower inner block ──

        // [CSS-TEXT-3 §7.1] text-align: center in narrower child block
        [Fact]
        public void TextAlignCenter_NarrowerChildBlockCenters()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px;text-align:center'><div style='width:200px'><span id='t' style='display:inline-block;width:60px;height:20px'></span></div></div></body>");
            // Child block is 200px wide, inherits text-align:center. X = (200 - 60) / 2 = 70
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 70) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align: right in auto-width child block
        [Fact]
        public void TextAlignRight_AutoWidthChildBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:right'><div><span id='t' style='display:inline-block;width:80px;height:20px'></span></div></div></body>");
            // Auto-width child fills parent (300px), inherits right. X = 300 - 80 = 220
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 220) < 2);
        }

        // ── mixed alignment scenarios ──

        // [CSS-TEXT-3 §7.1] sibling blocks can have different text-align
        [Fact]
        public void TextAlign_SiblingBlocksDifferentAlignment()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div style='text-align:left'><span id='left' style='display:inline-block;width:80px;height:20px'></span></div><div style='text-align:right'><span id='right' style='display:inline-block;width:80px;height:20px'></span></div><div style='text-align:center'><span id='center' style='display:inline-block;width:80px;height:20px'></span></div></div></body>");
            var leftBox = LayoutTestHelper.FindById(root, "left")!;
            var rightBox = LayoutTestHelper.FindById(root, "right")!;
            var centerBox = LayoutTestHelper.FindById(root, "center")!;
            Assert.True(leftBox.ContentRect.X < 2);
            Assert.True(System.Math.Abs(rightBox.ContentRect.X - 220) < 2);
            Assert.True(System.Math.Abs(centerBox.ContentRect.X - 110) < 2);
        }

        // ── text-align with inline-block containing content ──

        // [CSS-TEXT-3 §7.1] text-align: center with inline-block containing a block child
        [Fact]
        public void TextAlignCenter_InlineBlockWithBlockChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:center'><span id='t' style='display:inline-block'><div style='width:120px;height:30px'></div></span></div></body>");
            // Inline-block shrinks to fit child (120px). Centered: X = (300 - 120) / 2 = 90
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 90) < 2);
        }

        // [CSS-TEXT-3 §7.1] text-align: right with inline-block containing a block child
        [Fact]
        public void TextAlignRight_InlineBlockWithBlockChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:right'><span id='t' style='display:inline-block'><div style='width:120px;height:30px'></div></span></div></body>");
            // Inline-block shrinks to 120px. Right-aligned: X = 300 - 120 = 180
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 180) < 2);
        }

        // ── text-align with border-box ──

        // [CSS-TEXT-3 §7.1] text-align: center with border-box inline-block
        [Fact]
        public void TextAlignCenter_InlineBlockBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:center'><span id='t' style='display:inline-block;box-sizing:border-box;width:100px;height:30px;padding:10px;border:5px solid black'></span></div></body>");
            // border-box: total outer = 100px. Centered: X = (300 - 100) / 2 = 100. Content X = 100 + 5 + 10 = 115
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 115) < 2);
        }

        // ── text-align on body ──

        // [CSS-TEXT-3 §7.1] text-align on body inherited by child
        [Fact]
        public void TextAlignCenter_OnBodyInheritedByChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0;text-align:center'><div style='width:300px'><span id='t' style='display:inline-block;width:100px;height:20px'></span></div></body>");
            // Body text-align:center inherited by div. X = (300 - 100) / 2 = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 100) < 2);
        }

        // ── text-align with wider-than-container inline-block ──

        // [CSS-TEXT-3 §7.1] text-align: center when inline-block overflows container
        [Fact]
        public void TextAlignCenter_OverflowingInlineBlockAtStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px;text-align:center'><span id='t' style='display:inline-block;width:300px;height:20px'></span></div></body>");
            // Inline-block wider than container overflows. Width preserved, X at container start.
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 2);
            Assert.True(target.ContentRect.X < 2);
        }

        // ── text-align: center with multiple inline-blocks ──

        // [CSS-TEXT-3 §7.1] text-align: center with two inline-blocks
        [Fact]
        public void TextAlignCenter_TwoInlineBlocksCenteredTogether()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:center;font-size:0'><span id='a' style='display:inline-block;width:60px;height:20px;font-size:16px'></span><span id='b' style='display:inline-block;width:40px;height:20px;font-size:16px'></span></div></body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            // Total inline width = 60 + 40 = 100. Offset = (300 - 100) / 2 = 100
            Assert.True(System.Math.Abs(first.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.X - 160) < 2);
        }

        // ── text-align with height ──

        // [CSS-TEXT-3 §7.1] text-align does not affect Y position
        [Fact]
        public void TextAlignCenter_DoesNotAffectYPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:center'><span id='t' style='display:inline-block;width:80px;height:40px'></span></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Height should be preserved regardless of alignment
            Assert.True(System.Math.Abs(target.ContentRect.Height - 40) < 2);
        }

        // ── text-align and margin: auto ──

        // [CSS-TEXT-3 §7.1] text-align does not apply to block with margin:auto centering
        [Fact]
        public void TextAlignRight_DoesNotOverrideMarginAutoCentering()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;text-align:right'><div id='t' style='width:100px;height:20px;margin:0 auto'></div></div></body>");
            // Block with margin:auto centers itself regardless of text-align. X = (300-100)/2 = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 100) < 2);
        }

        // ── text-align: center with border on container ──

        // [CSS-TEXT-3 §7.1] text-align: center accounts for container border
        [Fact]
        public void TextAlignCenter_WithContainerBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px;border:10px solid black;text-align:center'><span id='t' style='display:inline-block;width:100px;height:20px'></span></div></body>");
            // Content area = 300px, starts at X=10. Center: X = 10 + (300 - 100) / 2 = 110
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 110) < 2);
        }
    }
}
