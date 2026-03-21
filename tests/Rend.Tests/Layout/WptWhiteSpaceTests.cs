using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS white-space property behavior: normal, nowrap, pre, pre-wrap, pre-line.
    /// Validates wrapping, whitespace collapsing, and newline handling per CSS Text Level 3.
    /// </summary>
    public class WptWhiteSpaceTests
    {
        private readonly ITestOutputHelper _output;
        public WptWhiteSpaceTests(ITestOutputHelper output) { _output = output; }

        // ======= white-space: normal =======

        // [CSS-TEXT-3 §3] normal: inline-blocks wrap to next line when container is narrow
        [Fact]
        public void Normal_InlineBlocksWrapInNarrowContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='c' style='width:100px;white-space:normal'>" +
                "<span id='a' style='display:inline-block;width:60px;height:20px'></span>" +
                "<span id='b' style='display:inline-block;width:60px;height:20px'></span>" +
                "</div></body>");
            var second = LayoutTestHelper.FindById(root, "b")!;
            // 60+60 > 100, so second inline-block wraps to Y >= 20
            Assert.True(second.ContentRect.Y >= 18, $"Expected wrap, Y={second.ContentRect.Y}");
        }

        // [CSS-TEXT-3 §3] normal: inline-blocks fit on one line in wide container
        [Fact]
        public void Normal_InlineBlocksFitOnOneLine()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='c' style='width:300px;white-space:normal'>" +
                "<span id='a' style='display:inline-block;width:60px;height:20px'></span>" +
                "<span id='b' style='display:inline-block;width:60px;height:20px'></span>" +
                "</div></body>");
            var second = LayoutTestHelper.FindById(root, "b")!;
            // 60+60 < 300, both fit on same line
            Assert.True(second.ContentRect.Y < 2, $"Expected same line, Y={second.ContentRect.Y}");
        }

        // [CSS-TEXT-3 §4.1] normal: multiple spaces collapse to single space
        [Fact]
        public void Normal_CollapsesMultipleSpaces()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px;white-space:normal'>" +
                "<span id='single' style='display:inline-block'>A B</span>" +
                "<span id='multi' style='display:inline-block'>A      B</span>" +
                "</div></body>");
            var single = LayoutTestHelper.FindById(root, "single")!;
            var multi = LayoutTestHelper.FindById(root, "multi")!;
            // Both should have same width since multiple spaces collapse
            Assert.True(System.Math.Abs(single.ContentRect.Width - multi.ContentRect.Width) < 2,
                $"Single={single.ContentRect.Width}, Multi={multi.ContentRect.Width}");
        }

        // [CSS-TEXT-3 §4] normal: newlines in source treated as spaces (collapsed)
        [Fact]
        public void Normal_NewlinesTreatedAsSpaces()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:400px;white-space:normal'>word1\nword2</div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            // Text should be on one line since newline is collapsed to space
            Assert.True(container.ContentRect.Height < 40, $"Height={container.ContentRect.Height}");
        }

        // [CSS-TEXT-3 §3] normal: container height reflects wrapped content
        [Fact]
        public void Normal_ContainerGrowsWithWrappedContent()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:80px;white-space:normal'>" +
                "<span style='display:inline-block;width:60px;height:30px'></span>" +
                "<span style='display:inline-block;width:60px;height:30px'></span>" +
                "</div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            // Two inline-blocks that don't fit side by side: container height >= 60
            Assert.True(container.ContentRect.Height >= 58, $"Height={container.ContentRect.Height}");
        }

        // ======= white-space: nowrap =======

        // [CSS-TEXT-3 §3] nowrap: inline-blocks do NOT wrap even when container is narrow
        [Fact]
        public void Nowrap_InlineBlocksDoNotWrap()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='c' style='width:100px;white-space:nowrap'>" +
                "<span id='a' style='display:inline-block;width:60px;height:20px'></span>" +
                "<span id='b' style='display:inline-block;width:60px;height:20px'></span>" +
                "</div></body>");
            var second = LayoutTestHelper.FindById(root, "b")!;
            // nowrap prevents wrapping: both on same line, second at X ~60
            Assert.True(second.ContentRect.Y < 2, $"Expected no wrap, Y={second.ContentRect.Y}");
        }

        // [CSS-TEXT-3 §3] nowrap: content overflows narrow container
        [Fact]
        public void Nowrap_ContentOverflowsContainer()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='c' style='width:100px;white-space:nowrap'>" +
                "<span id='a' style='display:inline-block;width:80px;height:20px'></span>" +
                "<span id='b' style='display:inline-block;width:80px;height:20px'></span>" +
                "</div></body>");
            var second = LayoutTestHelper.FindById(root, "b")!;
            // Second inline-block should be at X >= 80 (overflowing the 100px container)
            Assert.True(second.ContentRect.X >= 78, $"Expected overflow, X={second.ContentRect.X}");
        }

        // [CSS-TEXT-3 §4.1] nowrap: whitespace still collapses
        [Fact]
        public void Nowrap_CollapsesWhitespace()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px;white-space:nowrap'>" +
                "<span id='single' style='display:inline-block'>A B</span>" +
                "<span id='multi' style='display:inline-block'>A      B</span>" +
                "</div></body>");
            var single = LayoutTestHelper.FindById(root, "single")!;
            var multi = LayoutTestHelper.FindById(root, "multi")!;
            Assert.True(System.Math.Abs(single.ContentRect.Width - multi.ContentRect.Width) < 2,
                $"Single={single.ContentRect.Width}, Multi={multi.ContentRect.Width}");
        }

        // [CSS-TEXT-3 §3] nowrap: three inline-blocks all stay on one line
        [Fact]
        public void Nowrap_ThreeInlineBlocksOnOneLine()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='c' style='width:100px;white-space:nowrap'>" +
                "<span id='a' style='display:inline-block;width:50px;height:20px'></span>" +
                "<span id='b' style='display:inline-block;width:50px;height:20px'></span>" +
                "<span id='c3' style='display:inline-block;width:50px;height:20px'></span>" +
                "</div></body>");
            var third = LayoutTestHelper.FindById(root, "c3")!;
            Assert.True(third.ContentRect.Y < 2, $"Expected no wrap, Y={third.ContentRect.Y}");
            Assert.True(third.ContentRect.X >= 98, $"Expected X>=100, X={third.ContentRect.X}");
        }

        // [CSS-TEXT-3 §4] nowrap: newlines collapsed like normal
        [Fact]
        public void Nowrap_NewlinesCollapsed()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:400px;white-space:nowrap'>line1\nline2</div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            // Newlines treated as spaces, single line
            Assert.True(container.ContentRect.Height < 40, $"Height={container.ContentRect.Height}");
        }

        // ======= white-space: pre =======

        // [CSS-TEXT-3 §3] pre: preserves spaces (wider than collapsed version)
        [Fact]
        public void Pre_PreservesMultipleSpaces()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<span id='normal' style='display:inline-block;white-space:normal'>A B</span>" +
                "<span id='pre' style='display:inline-block;white-space:pre'>A      B</span>" +
                "</div></body>");
            var normal = LayoutTestHelper.FindById(root, "normal")!;
            var pre = LayoutTestHelper.FindById(root, "pre")!;
            // pre version with 6 spaces should be significantly wider
            Assert.True(pre.ContentRect.Width > normal.ContentRect.Width + 10,
                $"Pre={pre.ContentRect.Width}, Normal={normal.ContentRect.Width}");
        }

        // [CSS-TEXT-3 §3] pre: newlines cause line breaks
        [Fact]
        public void Pre_NewlinesCauseLineBreaks()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:400px;white-space:pre'>line1\nline2</div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            // Two lines: height should be at least 2 line-heights (~32px for 16px font)
            Assert.True(container.ContentRect.Height >= 30, $"Height={container.ContentRect.Height}");
        }

        // [CSS-TEXT-3 §3] pre: does not wrap at container edge
        [Fact]
        public void Pre_DoesNotWrapAtContainerEdge()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='c' style='width:100px;white-space:pre'>" +
                "<span id='a' style='display:inline-block;width:60px;height:20px'></span>" +
                "<span id='b' style='display:inline-block;width:60px;height:20px'></span>" +
                "</div></body>");
            var second = LayoutTestHelper.FindById(root, "b")!;
            // pre prevents soft wrapping, both on same line
            Assert.True(second.ContentRect.Y < 2, $"Expected no wrap, Y={second.ContentRect.Y}");
        }

        // [CSS-TEXT-3 §3] pre: leading spaces preserved
        [Fact]
        public void Pre_LeadingSpacesPreserved()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<span id='nopad' style='display:inline-block;white-space:pre'>AB</span>" +
                "<span id='padded' style='display:inline-block;white-space:pre'>   AB</span>" +
                "</div></body>");
            var nopad = LayoutTestHelper.FindById(root, "nopad")!;
            var padded = LayoutTestHelper.FindById(root, "padded")!;
            // Padded version should be wider due to preserved leading spaces
            Assert.True(padded.ContentRect.Width > nopad.ContentRect.Width + 5,
                $"Padded={padded.ContentRect.Width}, Nopad={nopad.ContentRect.Width}");
        }

        // [CSS-TEXT-3 §3] pre: three newlines produce three visible lines
        [Fact]
        public void Pre_MultipleNewlinesProduceMultipleLines()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:400px;white-space:pre'>A\nB\nC</div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            // Three lines, height >= 3 line-heights (~48px for 16px font)
            Assert.True(container.ContentRect.Height >= 44, $"Height={container.ContentRect.Height}");
        }

        // ======= white-space: pre-wrap =======

        // [CSS-TEXT-3 §3] pre-wrap: preserves spaces
        [Fact]
        public void PreWrap_PreservesMultipleSpaces()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<span id='normal' style='display:inline-block;white-space:normal'>A B</span>" +
                "<span id='prewrap' style='display:inline-block;white-space:pre-wrap'>A      B</span>" +
                "</div></body>");
            var normal = LayoutTestHelper.FindById(root, "normal")!;
            var prewrap = LayoutTestHelper.FindById(root, "prewrap")!;
            // pre-wrap preserves spaces, should be wider
            Assert.True(prewrap.ContentRect.Width > normal.ContentRect.Width + 10,
                $"PreWrap={prewrap.ContentRect.Width}, Normal={normal.ContentRect.Width}");
        }

        // [CSS-TEXT-3 §3] pre-wrap: wraps at container edge unlike pre
        [Fact]
        public void PreWrap_WrapsAtContainerEdge()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='c' style='width:100px;white-space:pre-wrap'>" +
                "<span id='a' style='display:inline-block;width:60px;height:20px'></span>" +
                "<span id='b' style='display:inline-block;width:60px;height:20px'></span>" +
                "</div></body>");
            var second = LayoutTestHelper.FindById(root, "b")!;
            // pre-wrap allows soft wrapping at container edge
            Assert.True(second.ContentRect.Y >= 18, $"Expected wrap, Y={second.ContentRect.Y}");
        }

        // [CSS-TEXT-3 §3] pre-wrap: newlines cause line breaks
        [Fact]
        public void PreWrap_NewlinesCauseLineBreaks()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:400px;white-space:pre-wrap'>line1\nline2</div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(container.ContentRect.Height >= 30, $"Height={container.ContentRect.Height}");
        }

        // [CSS-TEXT-3 §3] pre-wrap: container grows taller when content wraps
        [Fact]
        public void PreWrap_ContainerGrowsWhenWrapping()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='wide' style='width:400px;white-space:pre-wrap'>A      B</div>" +
                "<div id='narrow' style='width:40px;white-space:pre-wrap'>A      B</div>" +
                "</body>");
            var wide = LayoutTestHelper.FindById(root, "wide")!;
            var narrow = LayoutTestHelper.FindById(root, "narrow")!;
            // Narrow container forces wrap, so it should be taller
            Assert.True(narrow.ContentRect.Height > wide.ContentRect.Height,
                $"Narrow={narrow.ContentRect.Height}, Wide={wide.ContentRect.Height}");
        }

        // [CSS-TEXT-3 §3] pre-wrap: leading spaces preserved
        [Fact]
        public void PreWrap_LeadingSpacesPreserved()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<span id='nopad' style='display:inline-block;white-space:pre-wrap'>AB</span>" +
                "<span id='padded' style='display:inline-block;white-space:pre-wrap'>   AB</span>" +
                "</div></body>");
            var nopad = LayoutTestHelper.FindById(root, "nopad")!;
            var padded = LayoutTestHelper.FindById(root, "padded")!;
            Assert.True(padded.ContentRect.Width > nopad.ContentRect.Width + 5,
                $"Padded={padded.ContentRect.Width}, Nopad={nopad.ContentRect.Width}");
        }

        // ======= white-space: pre-line =======

        // [CSS-TEXT-3 §3] pre-line: collapses multiple spaces like normal
        [Fact]
        public void PreLine_CollapsesMultipleSpaces()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<span id='single' style='display:inline-block;white-space:pre-line'>A B</span>" +
                "<span id='multi' style='display:inline-block;white-space:pre-line'>A      B</span>" +
                "</div></body>");
            var single = LayoutTestHelper.FindById(root, "single")!;
            var multi = LayoutTestHelper.FindById(root, "multi")!;
            // pre-line collapses spaces, widths should be equal
            Assert.True(System.Math.Abs(single.ContentRect.Width - multi.ContentRect.Width) < 2,
                $"Single={single.ContentRect.Width}, Multi={multi.ContentRect.Width}");
        }

        // [CSS-TEXT-3 §3] pre-line: newlines cause line breaks (preserved)
        [Fact]
        public void PreLine_NewlinesCauseLineBreaks()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:400px;white-space:pre-line'>line1\nline2</div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            // Two lines from explicit newline
            Assert.True(container.ContentRect.Height >= 30, $"Height={container.ContentRect.Height}");
        }

        // [CSS-TEXT-3 §3] pre-line: wraps at container edge
        [Fact]
        public void PreLine_WrapsAtContainerEdge()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='c' style='width:100px;white-space:pre-line'>" +
                "<span id='a' style='display:inline-block;width:60px;height:20px'></span>" +
                "<span id='b' style='display:inline-block;width:60px;height:20px'></span>" +
                "</div></body>");
            var second = LayoutTestHelper.FindById(root, "b")!;
            // pre-line allows soft wrapping
            Assert.True(second.ContentRect.Y >= 18, $"Expected wrap, Y={second.ContentRect.Y}");
        }

        // [CSS-TEXT-3 §3] pre-line: multiple newlines produce multiple lines
        [Fact]
        public void PreLine_MultipleNewlinesProduceMultipleLines()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:400px;white-space:pre-line'>A\nB\nC</div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            // Three lines
            Assert.True(container.ContentRect.Height >= 44, $"Height={container.ContentRect.Height}");
        }

        // [CSS-TEXT-3 §4] pre-line: newline with surrounding spaces still breaks
        [Fact]
        public void PreLine_NewlineWithSurroundingSpacesBreaks()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:400px;white-space:pre-line'>word1   \n   word2</div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            // Spaces around newline collapse, but newline itself is preserved
            Assert.True(container.ContentRect.Height >= 30, $"Height={container.ContentRect.Height}");
        }

        // ======= Cross-value comparisons =======

        // [CSS-TEXT-3 §3] normal vs nowrap: same content, different wrap behavior
        [Fact]
        public void Normal_Vs_Nowrap_HeightDifference()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='wrap' style='width:100px;white-space:normal'>" +
                "<span style='display:inline-block;width:60px;height:20px'></span>" +
                "<span style='display:inline-block;width:60px;height:20px'></span>" +
                "</div>" +
                "<div id='nowrap' style='width:100px;white-space:nowrap'>" +
                "<span style='display:inline-block;width:60px;height:20px'></span>" +
                "<span style='display:inline-block;width:60px;height:20px'></span>" +
                "</div>" +
                "</body>");
            var wrap = LayoutTestHelper.FindById(root, "wrap")!;
            var nowrap = LayoutTestHelper.FindById(root, "nowrap")!;
            // normal wraps (height ~40), nowrap stays on one line (height ~20)
            Assert.True(wrap.ContentRect.Height > nowrap.ContentRect.Height,
                $"Wrap={wrap.ContentRect.Height}, Nowrap={nowrap.ContentRect.Height}");
        }

        // [CSS-TEXT-3 §3] pre vs pre-wrap: both preserve spaces, only pre-wrap wraps
        [Fact]
        public void Pre_Vs_PreWrap_WrapBehavior()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div id='pre' style='width:100px;white-space:pre'>" +
                "<span id='preA' style='display:inline-block;width:60px;height:20px'></span>" +
                "<span id='preB' style='display:inline-block;width:60px;height:20px'></span>" +
                "</div>" +
                "<div id='pw' style='width:100px;white-space:pre-wrap'>" +
                "<span id='pwA' style='display:inline-block;width:60px;height:20px'></span>" +
                "<span id='pwB' style='display:inline-block;width:60px;height:20px'></span>" +
                "</div>" +
                "</body>");
            var preB = LayoutTestHelper.FindById(root, "preB")!;
            var pwB = LayoutTestHelper.FindById(root, "pwB")!;
            // pre: no wrap (same line), pre-wrap: wraps
            Assert.True(preB.ContentRect.Y < 2, $"Pre should not wrap, Y={preB.ContentRect.Y}");
            Assert.True(pwB.ContentRect.Y >= 18, $"Pre-wrap should wrap, Y={pwB.ContentRect.Y}");
        }

        // [CSS-TEXT-3 §3] nowrap with explicit br still breaks
        [Fact]
        public void Nowrap_BrStillBreaks()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:400px;white-space:nowrap'>line1<br/>line2</div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            // <br> forces a line break even with nowrap
            Assert.True(container.ContentRect.Height >= 30, $"Height={container.ContentRect.Height}");
        }

        // [CSS-TEXT-3 §3] pre: tabs preserved as whitespace (wider than normal)
        [Fact]
        public void Pre_TabsPreserved()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'>" +
                "<span id='notab' style='display:inline-block;white-space:pre'>AB</span>" +
                "<span id='tab' style='display:inline-block;white-space:pre'>A\tB</span>" +
                "</div></body>");
            var notab = LayoutTestHelper.FindById(root, "notab")!;
            var tab = LayoutTestHelper.FindById(root, "tab")!;
            // Tab in pre mode should make the text wider
            Assert.True(tab.ContentRect.Width > notab.ContentRect.Width + 2,
                $"Tab={tab.ContentRect.Width}, NoTab={notab.ContentRect.Width}");
        }
    }
}
