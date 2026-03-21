using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Flexbox flex-wrap item positioning: verifies X/Y coordinates,
    /// line heights, gap spacing, align-content, justify-content, wrap-reverse,
    /// and column wrap across multiple wrap configurations.
    /// </summary>
    public class WptFlexWrapPositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexWrapPositionTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.3] Two items wrap to two lines, verify X positions
        [Fact]
        public void WrapTwoLines_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:100px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"First item X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X) < 2, $"Second item X=0 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] Two items wrap to two lines, verify Y positions
        [Fact]
        public void WrapTwoLines_ItemYPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:100px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"First line Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2, $"Second line Y=first line height 30 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] Three lines, each with one item
        [Fact]
        public void WrapThreeLines_OneItemPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:50px'>
                    <div id='a' style='width:40px;height:20px'></div>
                    <div id='b' style='width:40px;height:30px'></div>
                    <div id='c' style='width:40px;height:25px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"Line 1 Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 20) < 2, $"Line 2 Y=20 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 50) < 2, $"Line 3 Y=50 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] Different item heights per line, line height = tallest item
        [Fact]
        public void WrapDifferentHeightsPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:90px;height:20px'></div>
                    <div id='b' style='width:90px;height:50px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Line 1: a(20) and b(50), same Y=0. Line height = max(20,50) = 50
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2);
            // Line 2: c starts at Y=50 (tallest from line 1)
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 50) < 2, $"Line 2 Y=50 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] First wrap line always starts at Y=0
        [Fact]
        public void WrapFirstLine_YIsZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:80px'>
                    <div id='a' style='width:70px;height:45px'></div>
                    <div id='b' style='width:70px;height:35px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"First line Y=0 (got {itemA.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] Second wrap line Y equals first line cross size
        [Fact]
        public void WrapSecondLine_YEqualsFirstLineHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:80px'>
                    <div id='a' style='width:70px;height:45px'></div>
                    <div id='b' style='width:70px;height:35px'></div>
                </div></body>");
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 45) < 2, $"Second line Y=45 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9] row-gap adds spacing between wrap lines
        [Fact]
        public void WrapWithRowGap_SpacingBetweenLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:15px;width:80px'>
                    <div id='a' style='width:70px;height:40px'></div>
                    <div id='b' style='width:70px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float actualGap = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(actualGap - 15) < 2, $"row-gap=15 (got {actualGap})");
        }

        // [CSS-FLEXBOX §9] column-gap between items on the same wrap line
        [Fact]
        public void WrapWithColumnGap_SpacingWithinLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:10px;width:200px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float gap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(gap - 10) < 2, $"column-gap=10 (got {gap})");
        }

        // [CSS-FLEXBOX §9.3] One item per line: all items have X=0
        [Fact]
        public void WrapOnePerLine_AllXZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:50px'>
                    <div id='a' style='width:50px;height:20px'></div>
                    <div id='b' style='width:50px;height:25px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X) < 2, $"b X=0 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X) < 2, $"c X=0 (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] Two items per line: second item X = first item width
        [Fact]
        public void WrapTwoPerLine_SecondItemXPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                    <div id='c' style='width:80px;height:30px'></div>
                    <div id='d' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 80) < 2, $"b X=80 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X) < 2, $"c X=0 (got {itemC.ContentRect.X})");
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 80) < 2, $"d X=80 (got {itemD.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] Three items per line: X positions are cumulative widths
        [Fact]
        public void WrapThreePerLine_XPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:300px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:100px;height:30px'></div>
                    <div id='c' style='width:80px;height:30px'></div>
                    <div id='d' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 90) < 2, $"b X=90 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 190) < 2, $"c X=190 (got {itemC.ContentRect.X})");
            // d wraps: 90+100+80+90=360 > 300
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 30) < 2, $"d Y=30 (got {itemD.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] flex-grow distributes space independently per wrap line
        [Fact]
        public void WrapFlexGrowPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='flex:1 0 150px;height:30px'></div>
                    <div id='b' style='flex:1 0 60px;height:30px'></div>
                    <div id='c' style='flex:1 0 60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Line 1: a alone (150+60=210>200), grows to 200
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2, $"a grows to 200 (got {itemA.ContentRect.Width})");
            // Line 2: b and c share 200px equally (60+60=120<200, each flex:1)
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2, $"b grows to 100 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2, $"c grows to 100 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §8.2] justify-content:center per wrap line
        [Fact]
        public void WrapJustifyContentCenter_PerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:center;width:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Both on same line: 60+60=120, free=80, center offset=40
            float expectedStartX = 40;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedStartX) < 2,
                $"a X centered at {expectedStartX} (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - (expectedStartX + 60)) < 2,
                $"b X at {expectedStartX + 60} (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.4] align-content:flex-start packs lines at cross-start
        [Fact]
        public void WrapAlignContentFlexStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:80px;height:200px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2, $"b Y=30 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] align-content:center centers all wrap lines
        [Fact]
        public void WrapAlignContentCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:center;width:80px;height:200px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Two lines: 30+40=70. Free=130. Center offset=65.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 65) < 2, $"a Y=65 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 95) < 2, $"b Y=95 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] wrap-reverse flips cross-axis line order
        [Fact]
        public void WrapReverse_LinePositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;width:80px;height:120px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // wrap-reverse: first line at cross-end, second line above
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"wrap-reverse: a.Y={itemA.ContentRect.Y} should be > b.Y={itemB.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9.3] wrap-reverse: first line below second line
        [Fact]
        public void WrapReverse_FirstLineBelowSecondLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;align-content:flex-start;width:80px;height:100px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // wrap-reverse: cross axis inverted, first line (a) below second line (b)
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"wrap-reverse: a.Y={itemA.ContentRect.Y} should be > b.Y={itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y - 30) < 2,
                $"Lines separated by 30px (got {itemA.ContentRect.Y - itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] column wrap: items flow down then to next column
        [Fact]
        public void ColumnWrap_ItemPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;width:200px;height:70px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                    <div id='c' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // a at (0,0), b wraps (40+40=80>70) to next column at (60,0), c wraps to third column
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 60) < 2, $"b X=60 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y) < 2, $"b Y=0 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 120) < 2, $"c X=120 (got {itemC.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y) < 2, $"c Y=0 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] column wrap: second column X = first column width
        [Fact]
        public void ColumnWrap_ColumnXPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;width:300px;height:60px'>
                    <div id='a' style='width:80px;height:50px'></div>
                    <div id='b' style='width:100px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // a fills first column (50 height), b wraps to second column at X=80
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 80) < 2, $"b X=80 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] Items exactly fill the line: no wrapping occurs
        [Fact]
        public void WrapExactFit_NoWrapping()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:100px;height:30px'></div>
                    <div id='b' style='width:100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2,
                $"No wrap: a.Y={itemA.ContentRect.Y} == b.Y={itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2,
                $"b at X=100 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] row-gap with three wrap lines
        [Fact]
        public void WrapRowGap_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:10px;width:50px'>
                    <div id='a' style='width:50px;height:20px'></div>
                    <div id='b' style='width:50px;height:25px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // a at Y=0, b at Y=20+10=30, c at Y=30+25+10=65
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2, $"b Y=30 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 65) < 2, $"c Y=65 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9] both row-gap and column-gap with wrap
        [Fact]
        public void WrapBothGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:10px;column-gap:20px;width:200px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                    <div id='c' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Line 1: a(80) + gap(20) + b(80) = 180 < 200 → same line
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2);
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 20) < 2, $"column-gap=20 (got {columnGap})");
            // c wraps: 80+20+80+20+80=280 > 200
            float rowGap = itemC.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 10) < 2, $"row-gap=10 (got {rowGap})");
        }

        // [CSS-FLEXBOX §8.2] justify-content:space-between per wrap line
        [Fact]
        public void WrapJustifyContentSpaceBetween_PerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:space-between;width:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // All three on line 1: 60*3=180<200, space-between distributes 20px across 2 gaps
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 140) < 2,
                $"c X=140 (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.4] align-content:flex-end pushes all lines to bottom
        [Fact]
        public void WrapAlignContentFlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:80px;height:200px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Two lines: 30+40=70. Free=130. flex-end offset=130.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 130) < 2, $"a Y=130 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 160) < 2, $"b Y=160 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] align-content:space-between distributes lines
        [Fact]
        public void WrapAlignContentSpaceBetween()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;width:80px;height:200px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:30px'></div>
                    <div id='c' style='width:70px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Three lines: 30*3=90. Free=110. 2 gaps of 55.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 85) < 2, $"b Y=85 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 170) < 2, $"c Y=170 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] column-gap with column wrap
        [Fact]
        public void ColumnWrapWithColumnGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;column-gap:20px;width:300px;height:60px'>
                    <div id='a' style='width:60px;height:50px'></div>
                    <div id='b' style='width:60px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // a at X=0, b wraps to next column at X=60+20=80
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 80) < 2, $"b X=80 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] wrap with flex-grow and different basis per line
        [Fact]
        public void WrapFlexGrow_DifferentBasisPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:300px'>
                    <div id='a' style='flex:1 0 100px;height:30px'></div>
                    <div id='b' style='flex:2 0 100px;height:30px'></div>
                    <div id='c' style='flex:1 0 200px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Line 1: a(100) + b(100) = 200 < 300. Free=100. a gets 33.3, b gets 66.7
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 133.33f) < 2,
                $"a width ~133 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 166.67f) < 2,
                $"b width ~167 (got {itemB.ContentRect.Width})");
            // Line 2: c alone, grows to 300
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 300) < 2,
                $"c grows to 300 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §8.4] align-content:stretch distributes extra cross space
        [Fact]
        public void WrapAlignContentStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:80px;height:200px'>
                    <div id='a' style='width:70px'></div>
                    <div id='b' style='width:70px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Two lines, each stretched to 100px (200/2)
            Assert.True(itemA.ContentRect.Height >= 98,
                $"a height stretched to ~100 (got {itemA.ContentRect.Height})");
            Assert.True(itemB.ContentRect.Y >= 98,
                $"b Y at ~100 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] wrap-reverse with align-content:flex-start
        [Fact]
        public void WrapReverse_AlignContentFlexStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;align-content:flex-start;width:80px;height:200px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // wrap-reverse + flex-start: lines pack at cross-end (bottom)
            // Line 1 (a) at bottom, line 2 (b) above
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"wrap-reverse: a.Y={itemA.ContentRect.Y} > b.Y={itemB.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9.3] column wrap-reverse: columns flow right-to-left
        [Fact]
        public void ColumnWrapReverse_ColumnOrder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap-reverse;width:200px;height:60px'>
                    <div id='a' style='width:60px;height:50px'></div>
                    <div id='b' style='width:60px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // column wrap-reverse: first column at cross-end (right), second column to the left
            Assert.True(itemA.ContentRect.X > itemB.ContentRect.X,
                $"column wrap-reverse: a.X={itemA.ContentRect.X} > b.X={itemB.ContentRect.X}");
        }
    }
}
