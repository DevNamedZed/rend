using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive tests for CSS Flexbox wrapping behavior per CSS Flexbox Level 1.
    /// Covers flex-wrap, wrap-reverse, nowrap, column wrap, align-content with wrap,
    /// gap interactions, flex-grow per line, margins, padding, and edge cases.
    /// </summary>
    public class WptFlexboxWrapConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxWrapConformanceTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.4] flex-wrap:wrap — items exceeding container wrap to next line
        [Fact]
        public void FlexWrapWrap_ItemsExceedContainer_WrapToNextLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:80px;height:40px'></div>
                    <div id='b' style='width:80px;height:40px'></div>
                    <div id='c' style='width:80px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // 80+80=160 fits in 200, so a and b on line 1
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2);
            // 80+80+80=240 > 200, so c wraps to line 2 at Y=40
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 0) < 2);
        }

        // [CSS-FLEXBOX §9.4] flex-wrap:wrap-reverse — wrapped lines stack in reverse cross direction
        [Fact]
        public void FlexWrapWrapReverse_LinesStackInReverseCrossDirection()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;width:100px;height:100px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // wrap-reverse: first line at bottom, second line above
            // a is on first line (bottom), b is on second line (above a)
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"First line should be below second: a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9.4] flex-wrap:nowrap (default) — items do not wrap, may overflow
        [Fact]
        public void FlexWrapNowrap_ItemsDoNotWrap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:nowrap;width:100px'>
                    <div id='a' style='flex-shrink:0;width:60px;height:30px'></div>
                    <div id='b' style='flex-shrink:0;width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // Both on same line — same Y
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2);
            // b starts at X=60 (overflows container)
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 60) < 2);
        }

        // [CSS-FLEXBOX §9.4] flex-direction:column + flex-wrap:wrap — wraps to next column
        [Fact]
        public void FlexDirectionColumn_WrapToNextColumn()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;width:200px;height:100px'>
                    <div id='a' style='width:50px;height:60px'></div>
                    <div id='b' style='width:50px;height:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // 60+60=120 > 100, so b wraps to next column
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(itemB.ContentRect.X >= 48,
                $"b should wrap to next column: b.X={itemB.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 0) < 2);
        }

        // [CSS-FLEXBOX §9.4] flex-direction:column + wrap with 3 items across 2 columns
        [Fact]
        public void FlexDirectionColumnWrap_ThreeItemsTwoColumns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;width:300px;height:100px'>
                    <div id='a' style='width:50px;height:60px'></div>
                    <div id='b' style='width:50px;height:60px'></div>
                    <div id='c' style='width:50px;height:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // a fits in column 1. b wraps to column 2 (60+60=120>100).
            // c wraps to column 3 (60+60=120>100 in column 2).
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(itemB.ContentRect.X > itemA.ContentRect.X);
            Assert.True(itemC.ContentRect.X > itemB.ContentRect.X);
        }

        // [CSS-FLEXBOX §9.4] Multiple wrap lines with different cross sizes
        [Fact]
        public void WrapLinesWithDifferentCrossSizes()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;width:100px'>
                    <div id='a' style='width:60px;height:50px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                    <div id='c' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // Each item on its own line since 60+60>100
            // Line 1 height = 50 (from a), line 2 height = 30 (from b)
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 80) < 2);
            // Container auto-height = 50+30+40 = 120
            var container = LayoutTestHelper.FindById(r, "flex")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 120) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:flex-start with wrap — lines packed at start
        [Fact]
        public void WrapAlignContentFlexStart_LinesAtStart()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // Lines at top with no extra spacing
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:flex-end with wrap — lines packed at end
        [Fact]
        public void WrapAlignContentFlexEnd_LinesAtEnd()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // Two lines of 30px = 60px. Free = 140. Lines pushed to bottom.
            // a at Y=140, b at Y=170
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 140) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 170) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:center with wrap — lines centered
        [Fact]
        public void WrapAlignContentCenter_LinesCentered()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:center;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // Two lines of 30px = 60px. Free = 140. Center offset = 70.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:space-between with wrap — lines spaced evenly
        [Fact]
        public void WrapAlignContentSpaceBetween_LinesSpacedEvenly()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // 3 lines of 30px = 90px. Free = 110. 2 gaps of 55.
            // a at 0, b at 30+55=85, c at 85+30+55=170
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 85) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 170) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:space-around with wrap — equal space around each line
        [Fact]
        public void WrapAlignContentSpaceAround_EqualSpaceAroundLines()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-around;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // 2 lines of 40px = 80px. Free = 120. 4 half-gaps of 30.
            // a at Y=30, b at Y=30+40+60=130
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 130) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:stretch (default) — lines stretched to fill container
        [Fact]
        public void WrapAlignContentStretch_LinesStretchToFillContainer()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:200px'>
                    <div id='a' style='width:60px'></div>
                    <div id='b' style='width:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // 2 lines, each stretched to 100px (200/2)
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2,
                $"a should stretch to 100px: got {itemA.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 2);
        }

        // [CSS-FLEXBOX §9] flex-wrap with row-gap and column-gap
        [Fact]
        public void WrapWithRowGapAndColumnGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:10px;column-gap:20px;width:200px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                    <div id='c' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // 80+20+80=180 <= 200, so a and b fit on line 1
            // c wraps to line 2 with row-gap=10 between lines
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2);
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + 80);
            Assert.True(System.Math.Abs(columnGap - 20) < 2,
                $"column-gap should be 20: got {columnGap}");
            float rowGap = itemC.ContentRect.Y - (itemA.ContentRect.Y + 30);
            Assert.True(System.Math.Abs(rowGap - 10) < 2,
                $"row-gap should be 10: got {rowGap}");
        }

        // [CSS-FLEXBOX §9.7] flex-grow on items within wrapped lines — each line grows independently
        [Fact]
        public void WrapWithFlexGrow_EachLineGrowsIndependently()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='flex-grow:1;flex-shrink:0;flex-basis:120px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-shrink:0;flex-basis:120px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            _output.WriteLine($"a.W={itemA.ContentRect.Width} a.Y={itemA.ContentRect.Y}");
            _output.WriteLine($"b.W={itemB.ContentRect.Width} b.Y={itemB.ContentRect.Y}");
            // Each alone on its line (120+120>200). Each grows to 200.
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"a should grow to 200: got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"b should grow to 200: got {itemB.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.4] Wrap with different item heights — line cross size = tallest item
        [Fact]
        public void WrapWithDifferentItemHeights_LineCrossSizeIsTallest()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:90px;height:50px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                    <div id='c' style='width:90px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // a(90) + b(90) = 180 <= 200: same line. Line height = max(50,30) = 50.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2);
            // c wraps to line 2 at Y=50 (tallest of line 1)
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 50) < 2);
        }

        // [CSS-FLEXBOX §9.4] Wrap with min-width preventing items from shrinking
        [Fact]
        public void WrapWithMinWidth_ItemsCannotShrinkBelowMin()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:150px'>
                    <div id='a' style='min-width:80px;height:30px;flex:1 1 80px'></div>
                    <div id='b' style='min-width:80px;height:30px;flex:1 1 80px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // 80+80=160 > 150: items wrap (each on own line with flex-wrap)
            // Each grows to 150 on its own line
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2);
            Assert.True(itemB.ContentRect.Y >= 28);
        }

        // [CSS-FLEXBOX §9.4] Wrap with percentage widths
        [Fact]
        public void WrapWithPercentageWidths()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:60%;height:30px;flex-shrink:0'></div>
                    <div id='b' style='width:60%;height:30px;flex-shrink:0'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // 60% of 200 = 120. 120+120=240 > 200: b wraps.
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 120) < 2);
            Assert.True(itemB.ContentRect.Y >= 28);
        }

        // [CSS-FLEXBOX §5.4] Wrap with order property reordering items
        [Fact]
        public void WrapWithOrder_ItemsReorderedBeforeWrap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:150px'>
                    <div id='a' style='order:2;width:80px;height:30px'></div>
                    <div id='b' style='order:1;width:80px;height:30px'></div>
                    <div id='c' style='order:3;width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // Visual order: b(1), a(2), c(3). b and a on line 1 (80+80=160>150 — wraps!)
            // Actually b alone on line 1, a alone on line 2, c alone on line 3
            // because 80+80=160 > 150
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 60) < 2);
            // b should appear first (leftmost, topmost)
            Assert.True(itemB.ContentRect.X < itemA.ContentRect.X + 2);
        }

        // [CSS-FLEXBOX §9.4] Multiple rows with 3+ items each
        [Fact]
        public void WrapMultipleRowsThreeItemsEach()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:300px'>
                    <div id='a' style='width:90px;height:25px'></div>
                    <div id='b' style='width:90px;height:25px'></div>
                    <div id='c' style='width:90px;height:25px'></div>
                    <div id='d' style='width:90px;height:25px'></div>
                    <div id='e' style='width:90px;height:25px'></div>
                    <div id='f' style='width:90px;height:25px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            var itemD = LayoutTestHelper.FindById(r, "d")!;
            var itemE = LayoutTestHelper.FindById(r, "e")!;
            var itemF = LayoutTestHelper.FindById(r, "f")!;
            // 90*3=270 <= 300: 3 items per row
            // Row 1: a,b,c at Y=0. Row 2: d,e,f at Y=25.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(itemE.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(itemF.ContentRect.Y - 25) < 2);
            // X positions: 0, 90, 180 on each row
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemE.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(itemF.ContentRect.X - 180) < 2);
        }

        // [CSS-FLEXBOX §9.4] Single item with wrap — stays on first line
        [Fact]
        public void WrapSingleItem_StaysOnFirstLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px;height:100px'>
                    <div id='a' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2);
        }

        // [CSS-FLEXBOX §9.4] Items that exactly fill the line — no wrap
        [Fact]
        public void WrapExactFit_NoWrapNeeded()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:100px;height:30px'></div>
                    <div id='b' style='width:100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // 100+100=200 exactly fits
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2);
        }

        // [CSS-FLEXBOX §9.4] One pixel overflow triggers wrap
        [Fact]
        public void WrapOnePixelOverflow_TriggersWrap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:101px;height:30px;flex-shrink:0'></div>
                    <div id='b' style='width:100px;height:30px;flex-shrink:0'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // 101+100=201 > 200: b wraps to next line
            Assert.True(itemB.ContentRect.Y >= 28,
                $"b should wrap to next line: b.Y={itemB.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9.4] Wrap with padding on container
        [Fact]
        public void WrapWithPaddingOnContainer()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px;padding:10px'>
                    <div id='a' style='width:100px;height:30px'></div>
                    <div id='b' style='width:100px;height:30px'></div>
                    <div id='c' style='width:100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // Content area = 200px. 100+100=200 fits on one line.
            // Items offset by padding: X starts at 10, Y starts at 10.
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 110) < 2);
            // c wraps to line 2 at Y=10+30=40
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 10) < 2);
        }

        // [CSS-FLEXBOX §9.4] Wrap with margin on items
        [Fact]
        public void WrapWithMarginOnItems()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:80px;height:30px;margin:5px'></div>
                    <div id='b' style='width:80px;height:30px;margin:5px'></div>
                    <div id='c' style='width:80px;height:30px;margin:5px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // a occupies 80+5+5=90, b occupies 90. Total=180 <= 200: same line.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2);
            // a content at X=5 (margin-left), b content at X=90+5=95
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 5) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 95) < 2);
            // c occupies 90. 180+90=270 > 200: c wraps.
            // c at Y = 5 (margin-top of a) + 30 + 5 (margin-bottom of a) + 5 (margin-top of c)
            Assert.True(itemC.ContentRect.Y >= 38);
        }

        // [CSS-FLEXBOX §9.4] Column wrap with explicit container height
        [Fact]
        public void ColumnWrapWithExplicitHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;width:200px;height:80px'>
                    <div id='a' style='width:40px;height:30px'></div>
                    <div id='b' style='width:40px;height:30px'></div>
                    <div id='c' style='width:40px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // Column 1: a(30)+b(30)=60 <= 80. Column 2: c wraps (60+30=90>80).
            Assert.True(System.Math.Abs(itemA.ContentRect.X - itemB.ContentRect.X) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2);
            Assert.True(itemC.ContentRect.X > itemA.ContentRect.X);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 0) < 2);
        }

        // [CSS-FLEXBOX §9.4] wrap-reverse with align-content:flex-start
        [Fact]
        public void WrapReverseAlignContentFlexStart()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;align-content:flex-start;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // wrap-reverse: cross axis starts from bottom.
            // flex-start in wrap-reverse means from bottom.
            // First line (a) at bottom, second line (b) above.
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"a should be below b: a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9.4] wrap-reverse with two items per line
        [Fact]
        public void WrapReverseTwoItemsPerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;width:200px;height:100px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // a,b on line 1 (first line, placed at bottom in wrap-reverse)
            // c on line 2 (above line 1)
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2,
                "a and b should be on same line");
            Assert.True(itemC.ContentRect.Y < itemA.ContentRect.Y,
                $"c should be above a: c.Y={itemC.ContentRect.Y} a.Y={itemA.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9.4] Wrap with gap triggers earlier wrapping
        [Fact]
        public void WrapWithGap_TriggersEarlierWrapping()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:30px;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // 90+30+90=210 > 200: gap causes b to wrap
            Assert.True(itemB.ContentRect.Y >= 28,
                $"b should wrap due to gap: b.Y={itemB.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9.4] Wrap with flex-grow: single item on its line grows to fill
        [Fact]
        public void WrapFlexGrowLastLine_GrowsToFill()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='last' style='flex-grow:1;flex-basis:50px;height:30px'></div>
                </div></body>");
            var lastItem = LayoutTestHelper.FindById(r, "last")!;
            _output.WriteLine($"last.W={lastItem.ContentRect.Width}");
            // Single item with flex-grow:1 grows from basis 50 to fill line (200).
            Assert.True(System.Math.Abs(lastItem.ContentRect.Width - 200) < 2,
                $"last item should grow to 200: got {lastItem.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.4] Column wrap-reverse
        [Fact]
        public void ColumnWrapReverse()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap-reverse;width:200px;height:100px'>
                    <div id='a' style='width:50px;height:60px'></div>
                    <div id='b' style='width:50px;height:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // Column wrap-reverse: first column at right side, wraps leftward
            Assert.True(itemA.ContentRect.X > itemB.ContentRect.X,
                $"a should be to the right of b: a.X={itemA.ContentRect.X} b.X={itemB.ContentRect.X}");
        }

        // [CSS-FLEXBOX §9.4] Wrap with flex-basis percentage
        [Fact]
        public void WrapWithFlexBasisPercentage()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:300px'>
                    <div id='a' style='flex:0 0 40%;height:30px'></div>
                    <div id='b' style='flex:0 0 40%;height:30px'></div>
                    <div id='c' style='flex:0 0 40%;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // 40% of 300 = 120. 120+120=240 <= 300: a,b on line 1. c wraps.
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2);
            Assert.True(itemC.ContentRect.Y >= 28);
        }

        // [CSS-FLEXBOX §8.4] align-content:space-between with only one line — collapses to flex-start
        [Fact]
        public void WrapAlignContentSpaceBetween_SingleLine_CollapsesToFlexStart()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;width:200px;height:200px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // 80+80=160 <= 200: single line. space-between with 1 line → flex-start.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 0) < 2);
        }

        // [CSS-FLEXBOX §9.4] Wrap container auto-height includes all lines
        [Fact]
        public void WrapContainerAutoHeight_IncludesAllLines()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;width:100px'>
                    <div style='width:60px;height:25px'></div>
                    <div style='width:60px;height:35px'></div>
                    <div style='width:60px;height:45px'></div>
                    <div style='width:60px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(r, "flex")!;
            // Each on own line (60+60>100): total = 25+35+45+20 = 125
            Assert.True(System.Math.Abs(container.ContentRect.Height - 125) < 2,
                $"Container height should be 125: got {container.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9.4] Wrap with mixed flex-shrink:0 items — some wrap, some don't
        [Fact]
        public void WrapMixedItemSizes_WrapCorrectly()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:150px;height:30px'></div>
                    <div id='b' style='width:30px;height:30px'></div>
                    <div id='c' style='width:30px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // 150+30=180 <= 200: a,b on line 1. 180+30=210 > 200: c wraps.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 150) < 2);
            Assert.True(itemC.ContentRect.Y >= 28);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 0) < 2);
        }

        // [CSS-FLEXBOX §9.4] Wrap with row-gap affecting container auto-height
        [Fact]
        public void WrapRowGap_AffectsContainerAutoHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;row-gap:15px;width:100px'>
                    <div style='width:60px;height:30px'></div>
                    <div style='width:60px;height:30px'></div>
                    <div style='width:60px;height:30px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(r, "flex")!;
            // 3 lines of 30px + 2 gaps of 15px = 90 + 30 = 120
            Assert.True(System.Math.Abs(container.ContentRect.Height - 120) < 2,
                $"Container height should be 120: got {container.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9.4] Wrap with align-items:center — items centered within their line
        [Fact]
        public void WrapWithAlignItemsCenter_ItemsCenteredInLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-items:center;width:200px'>
                    <div id='tall' style='width:90px;height:60px'></div>
                    <div id='short' style='width:90px;height:20px'></div>
                </div></body>");
            var tallItem = LayoutTestHelper.FindById(r, "tall")!;
            var shortItem = LayoutTestHelper.FindById(r, "short")!;
            // Both on line 1 (90+90=180<=200). Line height = 60.
            // short centered: Y = (60-20)/2 = 20
            Assert.True(System.Math.Abs(tallItem.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(shortItem.ContentRect.Y - 20) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:stretch with three wrapped lines
        [Fact]
        public void WrapAlignContentStretch_ThreeLines()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:300px'>
                    <div id='a' style='width:60px'></div>
                    <div id='b' style='width:60px'></div>
                    <div id='c' style='width:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // 3 lines, each stretched to 100px (300/3)
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2,
                $"a should stretch to 100: got {itemA.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 200) < 2);
        }
    }
}
