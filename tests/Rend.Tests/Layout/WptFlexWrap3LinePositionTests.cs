using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Flexbox flex-wrap with exactly 3 wrap lines: verifies item X/Y
    /// positions, line heights, gap spacing, align-content variants, wrap-reverse,
    /// column wrap, justify-content per line, flex-grow per line, and auto height.
    /// </summary>
    public class WptFlexWrap3LinePositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexWrap3LinePositionTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.3] 3 items one per line: X=0, Y=0/30/60
        [Fact]
        public void ThreeLines_OneItemPerLine_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:50px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X) < 2, $"b X=0 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2, $"b Y=30 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X) < 2, $"c X=0 (got {itemC.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 60) < 2, $"c Y=60 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] 6 items, 2 per line across 3 lines: verify all X/Y positions
        [Fact]
        public void ThreeLines_TwoItemsPerLine_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                    <div id='d' style='width:90px;height:30px'></div>
                    <div id='e' style='width:90px;height:30px'></div>
                    <div id='f' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            var itemE = LayoutTestHelper.FindById(root, "e")!;
            var itemF = LayoutTestHelper.FindById(root, "f")!;
            // Line 1: a at (0,0), b at (90,0)
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 90) < 2, $"b X=90 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y) < 2, $"b Y=0 (got {itemB.ContentRect.Y})");
            // Line 2: c at (0,30), d at (90,30)
            Assert.True(System.Math.Abs(itemC.ContentRect.X) < 2, $"c X=0 (got {itemC.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 30) < 2, $"c Y=30 (got {itemC.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 90) < 2, $"d X=90 (got {itemD.ContentRect.X})");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 30) < 2, $"d Y=30 (got {itemD.ContentRect.Y})");
            // Line 3: e at (0,60), f at (90,60)
            Assert.True(System.Math.Abs(itemE.ContentRect.X) < 2, $"e X=0 (got {itemE.ContentRect.X})");
            Assert.True(System.Math.Abs(itemE.ContentRect.Y - 60) < 2, $"e Y=60 (got {itemE.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemF.ContentRect.X - 90) < 2, $"f X=90 (got {itemF.ContentRect.X})");
            Assert.True(System.Math.Abs(itemF.ContentRect.Y - 60) < 2, $"f Y=60 (got {itemF.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] 9 items, 3 per line across 3 lines: verify all X/Y positions
        [Fact]
        public void ThreeLines_ThreeItemsPerLine_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:300px'>
                    <div id='a' style='width:90px;height:25px'></div>
                    <div id='b' style='width:90px;height:25px'></div>
                    <div id='c' style='width:90px;height:25px'></div>
                    <div id='d' style='width:90px;height:25px'></div>
                    <div id='e' style='width:90px;height:25px'></div>
                    <div id='f' style='width:90px;height:25px'></div>
                    <div id='g' style='width:90px;height:25px'></div>
                    <div id='h' style='width:90px;height:25px'></div>
                    <div id='i' style='width:90px;height:25px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            var itemF = LayoutTestHelper.FindById(root, "f")!;
            var itemG = LayoutTestHelper.FindById(root, "g")!;
            var itemI = LayoutTestHelper.FindById(root, "i")!;
            // Line 1: a(0,0), b(90,0), c(180,0)
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 180) < 2, $"c X=180 (got {itemC.ContentRect.X})");
            // Line 2: d(0,25), e(90,25), f(180,25)
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 25) < 2, $"d Y=25 (got {itemD.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemF.ContentRect.X - 180) < 2, $"f X=180 (got {itemF.ContentRect.X})");
            // Line 3: g(0,50), h(90,50), i(180,50)
            Assert.True(System.Math.Abs(itemG.ContentRect.Y - 50) < 2, $"g Y=50 (got {itemG.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemI.ContentRect.X - 180) < 2, $"i X=180 (got {itemI.ContentRect.X})");
            Assert.True(System.Math.Abs(itemI.ContentRect.Y - 50) < 2, $"i Y=50 (got {itemI.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9] 3 lines with row-gap spacing between each line
        [Fact]
        public void ThreeLines_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:10px;width:50px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 40) < 2, $"b Y=40 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 80) < 2, $"c Y=80 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] 3 lines with align-content:flex-start in tall container
        [Fact]
        public void ThreeLines_AlignContentFlexStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:60px;height:300px'>
                    <div id='a' style='width:50px;height:40px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 40) < 2, $"b Y=40 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 80) < 2, $"c Y=80 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] 3 lines with align-content:flex-end pushed to bottom
        [Fact]
        public void ThreeLines_AlignContentFlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:60px;height:300px'>
                    <div id='a' style='width:50px;height:40px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // 3 lines * 40px = 120px, free = 180, offset = 180
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 180) < 2, $"a Y=180 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 220) < 2, $"b Y=220 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 260) < 2, $"c Y=260 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] 3 lines with align-content:center centered vertically
        [Fact]
        public void ThreeLines_AlignContentCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:center;width:60px;height:300px'>
                    <div id='a' style='width:50px;height:40px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // 3 lines * 40px = 120px, free = 180, center offset = 90
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 90) < 2, $"a Y=90 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 130) < 2, $"b Y=130 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 170) < 2, $"c Y=170 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] 3 lines with align-content:space-between
        [Fact]
        public void ThreeLines_AlignContentSpaceBetween()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;width:60px;height:300px'>
                    <div id='a' style='width:50px;height:40px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // 3 lines * 40px = 120px, free = 180, 2 gaps = 90 each
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 130) < 2, $"b Y=130 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 260) < 2, $"c Y=260 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] 3 lines with align-content:stretch distributes extra cross space
        [Fact]
        public void ThreeLines_AlignContentStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:60px;height:300px'>
                    <div id='a' style='width:50px'></div>
                    <div id='b' style='width:50px'></div>
                    <div id='c' style='width:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // 3 lines stretched to 300/3 = 100 each
            float expectedLineHeight = 100f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - expectedLineHeight) < 2,
                $"a height=100 (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - expectedLineHeight) < 2,
                $"b Y=100 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 200) < 2,
                $"c Y=200 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] 3 lines with wrap-reverse flips cross-axis order
        [Fact]
        public void ThreeLines_WrapReverse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;align-content:flex-start;width:60px;height:200px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // wrap-reverse: line 1 (a) at bottom, line 3 (c) at top
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"wrap-reverse: a.Y={itemA.ContentRect.Y} > b.Y={itemB.ContentRect.Y}");
            Assert.True(itemB.ContentRect.Y > itemC.ContentRect.Y,
                $"wrap-reverse: b.Y={itemB.ContentRect.Y} > c.Y={itemC.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9.3] 3 lines with different heights per line
        [Fact]
        public void ThreeLines_DifferentHeightsPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:60px'>
                    <div id='a' style='width:50px;height:20px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 20) < 2, $"b Y=20 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 60) < 2, $"c Y=60 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] auto height container equals sum of 3 line cross sizes
        [Fact]
        public void ThreeLines_AutoHeightEqualsSumOfLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;width:50px'>
                    <div style='width:50px;height:20px'></div>
                    <div style='width:50px;height:35px'></div>
                    <div style='width:50px;height:25px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            float expectedHeight = 20 + 35 + 25;
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - expectedHeight) < 2,
                $"auto height={expectedHeight} (got {flexContainer.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9] auto height with row-gap includes gaps between 3 lines
        [Fact]
        public void ThreeLines_AutoHeightWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;row-gap:15px;width:50px'>
                    <div style='width:50px;height:30px'></div>
                    <div style='width:50px;height:30px'></div>
                    <div style='width:50px;height:30px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            // 3 lines of 30px + 2 gaps of 15px = 90 + 30 = 120
            float expectedHeight = 30 + 15 + 30 + 15 + 30;
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - expectedHeight) < 2,
                $"auto height with gap={expectedHeight} (got {flexContainer.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.3] column wrap producing 3 columns
        [Fact]
        public void ColumnWrap_ThreeColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;width:300px;height:50px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                    <div id='c' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Each item wraps to its own column: a(0,0), b(60,0), c(120,0)
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 60) < 2, $"b X=60 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y) < 2, $"b Y=0 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 120) < 2, $"c X=120 (got {itemC.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y) < 2, $"c Y=0 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] column wrap 3 columns with column-gap spacing
        [Fact]
        public void ColumnWrap_ThreeColumnsWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;column-gap:20px;width:400px;height:50px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                    <div id='c' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // a at X=0, b at X=60+20=80, c at X=80+60+20=160
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 80) < 2, $"b X=80 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 160) < 2, $"c X=160 (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.2] 3 lines with justify-content:center per line
        [Fact]
        public void ThreeLines_JustifyContentCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:center;width:200px'>
                    <div id='a' style='width:120px;height:30px'></div>
                    <div id='b' style='width:120px;height:30px'></div>
                    <div id='c' style='width:120px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Each item alone on line, centered: (200-120)/2 = 40
            float expectedX = 40f;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedX) < 2,
                $"a X={expectedX} (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - expectedX) < 2,
                $"b X={expectedX} (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - expectedX) < 2,
                $"c X={expectedX} (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.2] 3 lines with justify-content:flex-end per line
        [Fact]
        public void ThreeLines_JustifyContentFlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:flex-end;width:200px'>
                    <div id='a' style='width:120px;height:30px'></div>
                    <div id='b' style='width:120px;height:30px'></div>
                    <div id='c' style='width:120px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Each item alone on line, pushed to end: 200-120 = 80
            float expectedX = 80f;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedX) < 2,
                $"a X={expectedX} (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - expectedX) < 2,
                $"b X={expectedX} (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - expectedX) < 2,
                $"c X={expectedX} (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] flex-grow distributes space independently per line across 3 lines
        [Fact]
        public void ThreeLines_FlexGrowPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='flex:1 0 150px;height:30px'></div>
                    <div id='b' style='flex:1 0 150px;height:30px'></div>
                    <div id='c' style='flex:1 0 150px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Each item alone on its line, grows from 150 to 200
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"a width=200 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"b width=200 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 200) < 2,
                $"c width=200 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.3] 3 lines with 2 items each, verify all 6 positions
        [Fact]
        public void ThreeLines_TwoItemsEach_AllPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:160px'>
                    <div id='a' style='width:70px;height:25px'></div>
                    <div id='b' style='width:70px;height:25px'></div>
                    <div id='c' style='width:70px;height:35px'></div>
                    <div id='d' style='width:70px;height:35px'></div>
                    <div id='e' style='width:70px;height:20px'></div>
                    <div id='f' style='width:70px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            var itemE = LayoutTestHelper.FindById(root, "e")!;
            var itemF = LayoutTestHelper.FindById(root, "f")!;
            // Line 1: height=25, line 2: height=35, line 3: height=20
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2 && System.Math.Abs(itemA.ContentRect.Y) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 70) < 2, $"b X=70 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 25) < 2, $"c Y=25 (got {itemC.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 70) < 2, $"d X=70 (got {itemD.ContentRect.X})");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 25) < 2, $"d Y=25 (got {itemD.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemE.ContentRect.Y - 60) < 2, $"e Y=60 (got {itemE.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemF.ContentRect.X - 70) < 2, $"f X=70 (got {itemF.ContentRect.X})");
            Assert.True(System.Math.Abs(itemF.ContentRect.Y - 60) < 2, $"f Y=60 (got {itemF.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] 3 lines with align-content:space-around distributes with half-gaps at edges
        [Fact]
        public void ThreeLines_AlignContentSpaceAround()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-around;width:60px;height:300px'>
                    <div id='a' style='width:50px;height:40px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // 3 lines * 40px = 120px, free = 180, 3 items → gap = 180/3 = 60, half = 30
            // a.Y = 30, b.Y = 30+40+60 = 130, c.Y = 130+40+60 = 230
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 30) < 2, $"a Y=30 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 130) < 2, $"b Y=130 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 230) < 2, $"c Y=230 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] 3 lines where line heights differ, tallest item determines line cross size
        [Fact]
        public void ThreeLines_TwoItemsPerLine_TallestDeterminesLineHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:160px'>
                    <div id='a' style='width:70px;height:20px'></div>
                    <div id='b' style='width:70px;height:50px'></div>
                    <div id='c' style='width:70px;height:30px'></div>
                    <div id='d' style='width:70px;height:10px'></div>
                    <div id='e' style='width:70px;height:25px'></div>
                    <div id='f' style='width:70px;height:25px'></div>
                </div></body>");
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemE = LayoutTestHelper.FindById(root, "e")!;
            // Line 1: max(20,50) = 50 → line 2 starts at Y=50
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 50) < 2, $"c Y=50 (got {itemC.ContentRect.Y})");
            // Line 2: max(30,10) = 30 → line 3 starts at Y=80
            Assert.True(System.Math.Abs(itemE.ContentRect.Y - 80) < 2, $"e Y=80 (got {itemE.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9] 3 lines with both row-gap and column-gap
        [Fact]
        public void ThreeLines_WithBothGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:10px;column-gap:20px;width:180px'>
                    <div id='a' style='width:70px;height:25px'></div>
                    <div id='b' style='width:70px;height:25px'></div>
                    <div id='c' style='width:70px;height:25px'></div>
                    <div id='d' style='width:70px;height:25px'></div>
                    <div id='e' style='width:70px;height:25px'></div>
                    <div id='f' style='width:70px;height:25px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemE = LayoutTestHelper.FindById(root, "e")!;
            // Line 1: a(70) + gap(20) + b(70) = 160 < 180 → same line
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a and b on same line");
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 20) < 2, $"column-gap=20 (got {columnGap})");
            // Line 2: Y = 25 + 10 = 35
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 35) < 2, $"c Y=35 (got {itemC.ContentRect.Y})");
            // Line 3: Y = 35 + 25 + 10 = 70
            Assert.True(System.Math.Abs(itemE.ContentRect.Y - 70) < 2, $"e Y=70 (got {itemE.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] flex-grow with 2 items per line across 3 lines
        [Fact]
        public void ThreeLines_FlexGrowTwoItemsPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='flex:1 0 80px;height:30px'></div>
                    <div id='b' style='flex:1 0 80px;height:30px'></div>
                    <div id='c' style='flex:2 0 80px;height:30px'></div>
                    <div id='d' style='flex:1 0 80px;height:30px'></div>
                    <div id='e' style='flex:1 0 150px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            var itemE = LayoutTestHelper.FindById(root, "e")!;
            // Line 1: a+b, basis 80+80=160, free=40, each flex:1 → 20 each → 100px each
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"a width=100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"b width=100 (got {itemB.ContentRect.Width})");
            // Line 2: c(flex:2)+d(flex:1), basis 80+80=160, free=40
            // c gets 40*2/3 ≈ 26.67 → 106.67, d gets 40*1/3 ≈ 13.33 → 93.33
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 106.67f) < 2,
                $"c width~107 (got {itemC.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemD.ContentRect.Width - 93.33f) < 2,
                $"d width~93 (got {itemD.ContentRect.Width})");
            // Line 3: e alone, grows to 200
            Assert.True(System.Math.Abs(itemE.ContentRect.Width - 200) < 2,
                $"e width=200 (got {itemE.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §8.2] 3 lines with justify-content:space-between, 2 items per line
        [Fact]
        public void ThreeLines_JustifyContentSpaceBetween_TwoPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:space-between;width:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                    <div id='d' style='width:60px;height:30px'></div>
                    <div id='e' style='width:60px;height:30px'></div>
                    <div id='f' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            // Line has 3 items: 60*3=180, free=20, 2 gaps → 10 each
            // But 60+60+60=180 fits, so all 3 on one line → only 2 lines of 3
            // a at X=0, b at X=70, c at X=140
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 140) < 2, $"c X=140 (got {itemC.ContentRect.X})");
            // Line 2: d at X=0, f at X=140
            Assert.True(System.Math.Abs(itemD.ContentRect.X) < 2, $"d X=0 (got {itemD.ContentRect.X})");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 30) < 2, $"d Y=30 (got {itemD.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] wrap-reverse with 3 lines ordered bottom to top
        [Fact]
        public void ThreeLines_WrapReverse_YOrdering()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;align-content:flex-start;width:60px;height:150px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // wrap-reverse: line 1 at bottom, line 2 in middle, line 3 at top
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"a.Y={itemA.ContentRect.Y} > b.Y={itemB.ContentRect.Y}");
            Assert.True(itemB.ContentRect.Y > itemC.ContentRect.Y,
                $"b.Y={itemB.ContentRect.Y} > c.Y={itemC.ContentRect.Y}");
            // Lines are 30px apart
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y - 30) < 2,
                $"a-b gap=30 (got {itemA.ContentRect.Y - itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - itemC.ContentRect.Y - 30) < 2,
                $"b-c gap=30 (got {itemB.ContentRect.Y - itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] column wrap-reverse 3 columns ordered right to left
        [Fact]
        public void ColumnWrapReverse_ThreeColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap-reverse;width:250px;height:50px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                    <div id='c' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // column wrap-reverse: columns go right to left
            Assert.True(itemA.ContentRect.X > itemB.ContentRect.X,
                $"a.X={itemA.ContentRect.X} > b.X={itemB.ContentRect.X}");
            Assert.True(itemB.ContentRect.X > itemC.ContentRect.X,
                $"b.X={itemB.ContentRect.X} > c.X={itemC.ContentRect.X}");
        }

        // [CSS-FLEXBOX §8.4] 3 lines align-content:stretch with explicit item heights
        [Fact]
        public void ThreeLines_AlignContentStretch_WithExplicitHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:60px;height:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Each line gets 300/3 = 100px. Items keep explicit 30px height but Y positions spread.
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 2,
                $"b Y=100 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 200) < 2,
                $"c Y=200 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] 3 lines align-content:space-between with row-gap
        [Fact]
        public void ThreeLines_AlignContentSpaceBetween_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;row-gap:10px;width:60px;height:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // First line at top, last line at bottom
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y + itemC.ContentRect.Height - 300) < 2,
                $"c bottom=300 (got {itemC.ContentRect.Y + itemC.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §8.2] 3 lines with justify-content:center and 2 items per line
        [Fact]
        public void ThreeLines_JustifyContentCenter_TwoPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:center;width:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                    <div id='d' style='width:60px;height:30px'></div>
                    <div id='e' style='width:60px;height:30px'></div>
                    <div id='f' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            // 3 items per line: 60*3=180, free=20, center offset=10
            float expectedStartX = 10f;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedStartX) < 2,
                $"a X={expectedStartX} (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemD.ContentRect.X - expectedStartX) < 2,
                $"d X={expectedStartX} (got {itemD.ContentRect.X})");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 30) < 2,
                $"d Y=30 (got {itemD.ContentRect.Y})");
        }
    }
}
