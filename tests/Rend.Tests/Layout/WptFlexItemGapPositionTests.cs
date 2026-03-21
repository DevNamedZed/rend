using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexItemGapPositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexItemGapPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §8.2] gap:10px between 2 items — second item X = itemWidth + gap
        [Fact]
        public void Gap10px_TwoItems_XPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:10px;width:300px'>
                    <div id='a' style='width:40px;height:30px'></div>
                    <div id='b' style='width:40px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a.X expected ~0, got {itemA.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 50) < 2, $"b.X expected ~50, got {itemB.ContentRect.X}");
        }

        // [CSS-FLEXBOX §8.2] gap:20px between 3 items — positions at 0, 60+20, 60+20+60+20
        [Fact]
        public void Gap20px_ThreeItems_XPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:20px;width:400px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a.X expected ~0, got {itemA.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 80) < 2, $"b.X expected ~80, got {itemB.ContentRect.X}");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 160) < 2, $"c.X expected ~160, got {itemC.ContentRect.X}");
        }

        // [CSS-FLEXBOX §8.2] gap:10px between 4 items — cumulative positions
        [Fact]
        public void Gap10px_FourItems_XPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:10px;width:400px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                    <div id='d' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a.X expected ~0, got {itemA.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 60) < 2, $"b.X expected ~60, got {itemB.ContentRect.X}");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 120) < 2, $"c.X expected ~120, got {itemC.ContentRect.X}");
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 180) < 2, $"d.X expected ~180, got {itemD.ContentRect.X}");
        }

        // [CSS-FLEXBOX §8.2] gap:10px between 5 items — all positions correct
        [Fact]
        public void Gap10px_FiveItems_XPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:10px;width:400px'>
                    <div id='a' style='width:30px;height:30px'></div>
                    <div id='b' style='width:30px;height:30px'></div>
                    <div id='c' style='width:30px;height:30px'></div>
                    <div id='d' style='width:30px;height:30px'></div>
                    <div id='e' style='width:30px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            var itemE = LayoutTestHelper.FindById(root, "e")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a.X expected ~0, got {itemA.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 40) < 2, $"b.X expected ~40, got {itemB.ContentRect.X}");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 80) < 2, $"c.X expected ~80, got {itemC.ContentRect.X}");
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 120) < 2, $"d.X expected ~120, got {itemD.ContentRect.X}");
            Assert.True(System.Math.Abs(itemE.ContentRect.X - 160) < 2, $"e.X expected ~160, got {itemE.ContentRect.X}");
        }

        // [CSS-FLEXBOX §8.2] gap:0 produces same layout as no gap property
        [Fact]
        public void GapZero_SameAsNoGap()
        {
            var rootWithGap = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:0;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            var rootNoGap = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            var gapB = LayoutTestHelper.FindById(rootWithGap, "b")!;
            var noGapB = LayoutTestHelper.FindById(rootNoGap, "b")!;
            Assert.True(System.Math.Abs(gapB.ContentRect.X - noGapB.ContentRect.X) < 2,
                $"gap:0 b.X={gapB.ContentRect.X} should equal no-gap b.X={noGapB.ContentRect.X}");
        }

        // [CSS-FLEXBOX §8.2] gap with flex-grow — gap subtracted from available space before distribution
        [Fact]
        public void Gap_WithFlexGrow_AllPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:20px;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float expectedWidth = (300f - 2 * 20f) / 3f;
            _output.WriteLine($"expectedWidth={expectedWidth} a.w={itemA.ContentRect.Width} b.x={itemB.ContentRect.X} c.x={itemC.ContentRect.X}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - expectedWidth) < 2,
                $"a.Width expected ~{expectedWidth}, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - (expectedWidth + 20)) < 2,
                $"b.X expected ~{expectedWidth + 20}, got {itemB.ContentRect.X}");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - (2 * expectedWidth + 40)) < 2,
                $"c.X expected ~{2 * expectedWidth + 40}, got {itemC.ContentRect.X}");
        }

        // [CSS-FLEXBOX §8.2] gap with justify-content:center — items centered with gap between
        [Fact]
        public void Gap_JustifyContentCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:10px;justify-content:center;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalUsed = 50 + 10 + 50;
            float expectedOffset = (300 - totalUsed) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedOffset) < 2,
                $"a.X expected ~{expectedOffset}, got {itemA.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - (expectedOffset + 60)) < 2,
                $"b.X expected ~{expectedOffset + 60}, got {itemB.ContentRect.X}");
        }

        // [CSS-FLEXBOX §8.2] gap with justify-content:flex-end — items packed to right with gap
        [Fact]
        public void Gap_JustifyContentFlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:10px;justify-content:flex-end;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalUsed = 50 + 10 + 50;
            float expectedAX = 300 - totalUsed;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedAX) < 2,
                $"a.X expected ~{expectedAX}, got {itemA.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X + itemB.ContentRect.Width - 300) < 2,
                $"b right edge expected ~300, got {itemB.ContentRect.X + itemB.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §8.2] gap with justify-content:space-between — gap adds to distributed space
        [Fact]
        public void Gap_JustifyContentSpaceBetween()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:10px;justify-content:space-between;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2,
                $"a.X expected ~0, got {itemA.ContentRect.X}");
            Assert.True(System.Math.Abs(itemC.ContentRect.X + itemC.ContentRect.Width - 300) < 2,
                $"c right edge expected ~300, got {itemC.ContentRect.X + itemC.ContentRect.Width}");
            float gapAB = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            float gapBC = itemC.ContentRect.X - (itemB.ContentRect.X + itemB.ContentRect.Width);
            Assert.True(System.Math.Abs(gapAB - gapBC) < 2,
                $"space-between gaps should be equal: AB={gapAB}, BC={gapBC}");
            Assert.True(gapAB >= 10, $"space-between gap should be at least column-gap(10), got {gapAB}");
        }

        // [CSS-FLEXBOX §8.2] column gap:10px between 2 items — Y positions
        [Fact]
        public void ColumnGap10px_TwoItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;gap:10px;width:200px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a.Y expected ~0, got {itemA.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 50) < 2, $"b.Y expected ~50, got {itemB.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.2] column gap:20px between 3 items — Y positions
        [Fact]
        public void ColumnGap20px_ThreeItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;gap:20px;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                    <div id='c' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a.Y expected ~0, got {itemA.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 50) < 2, $"b.Y expected ~50, got {itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 100) < 2, $"c.Y expected ~100, got {itemC.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.2] column gap:10px between 4 items — Y positions
        [Fact]
        public void ColumnGap10px_FourItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;gap:10px;width:200px'>
                    <div id='a' style='height:25px'></div>
                    <div id='b' style='height:25px'></div>
                    <div id='c' style='height:25px'></div>
                    <div id='d' style='height:25px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a.Y expected ~0, got {itemA.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 35) < 2, $"b.Y expected ~35, got {itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 70) < 2, $"c.Y expected ~70, got {itemC.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 105) < 2, $"d.Y expected ~105, got {itemD.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.2] percentage gap resolves against containing block width
        [Fact]
        public void GapPercentage_ResolvesAgainstContainingBlockWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:10%;width:400px'>
                    <div id='a' style='width:40px;height:30px'></div>
                    <div id='b' style='width:40px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float actualGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            _output.WriteLine($"percentage gap={actualGap} (expected 40 = 10% of 400)");
            Assert.True(System.Math.Abs(actualGap - 40) < 2, $"10% of 400px gap expected ~40, got {actualGap}");
        }

        // [CSS-FLEXBOX §9] gap with wrap — two lines of items with row-gap between lines
        [Fact]
        public void Gap_WithWrap_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;gap:10px;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 10) < 2, $"column-gap expected ~10, got {columnGap}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a and b should be on same line");
            float rowGap = itemC.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 10) < 2, $"row-gap expected ~10, got {rowGap}");
        }

        // [CSS-FLEXBOX §9.7] gap with flex-shrink — items shrink, gaps stay fixed
        [Fact]
        public void Gap_WithFlexShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:20px;width:200px'>
                    <div id='a' style='width:120px;height:30px'></div>
                    <div id='b' style='width:120px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float actualGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            _output.WriteLine($"a.w={itemA.ContentRect.Width} b.x={itemB.ContentRect.X} gap={actualGap}");
            Assert.True(System.Math.Abs(actualGap - 20) < 2, $"gap should remain 20px even with shrink, got {actualGap}");
            float totalWidth = itemA.ContentRect.Width + 20 + itemB.ContentRect.Width;
            Assert.True(System.Math.Abs(totalWidth - 200) < 2, $"total should fill container: {totalWidth}");
        }

        // [CSS-FLEXBOX §9] row-gap between wrap lines
        [Fact]
        public void RowGap_BetweenWrapLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:25px;width:100px'>
                    <div id='a' style='width:60px;height:35px'></div>
                    <div id='b' style='width:60px;height:35px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float rowGap = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 25) < 2, $"row-gap expected ~25, got {rowGap}");
        }

        // [CSS-FLEXBOX §8.2] column-gap in column direction applies between items in main axis
        [Fact]
        public void ColumnGap_InColumnDirection()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;row-gap:15px;width:200px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float gapAB = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            float gapBC = itemC.ContentRect.Y - (itemB.ContentRect.Y + itemB.ContentRect.Height);
            Assert.True(System.Math.Abs(gapAB - 15) < 2, $"row-gap A-B expected ~15, got {gapAB}");
            Assert.True(System.Math.Abs(gapBC - 15) < 2, $"row-gap B-C expected ~15, got {gapBC}");
        }

        // [CSS-FLEXBOX §8.2] gap with CSS order — gap applies in visual order, not source order
        [Fact]
        public void Gap_WithOrder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:10px;width:300px'>
                    <div id='a' style='width:50px;height:30px;order:2'></div>
                    <div id='b' style='width:50px;height:30px;order:1'></div>
                    <div id='c' style='width:50px;height:30px;order:3'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(itemB.ContentRect.X < itemA.ContentRect.X,
                $"b (order:1) should be before a (order:2): b.X={itemB.ContentRect.X}, a.X={itemA.ContentRect.X}");
            Assert.True(itemA.ContentRect.X < itemC.ContentRect.X,
                $"a (order:2) should be before c (order:3): a.X={itemA.ContentRect.X}, c.X={itemC.ContentRect.X}");
            float gapBA = itemA.ContentRect.X - (itemB.ContentRect.X + itemB.ContentRect.Width);
            float gapAC = itemC.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(gapBA - 10) < 2, $"gap B-A expected ~10, got {gapBA}");
            Assert.True(System.Math.Abs(gapAC - 10) < 2, $"gap A-C expected ~10, got {gapAC}");
        }

        // [CSS-FLEXBOX §8.2] gap with margin on items — margin and gap both contribute to spacing
        [Fact]
        public void Gap_WithMarginOnItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:10px;width:400px'>
                    <div id='a' style='width:50px;height:30px;margin-right:5px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float spacing = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            _output.WriteLine($"a.X={itemA.ContentRect.X} a.W={itemA.ContentRect.Width} b.X={itemB.ContentRect.X} spacing={spacing}");
            Assert.True(spacing >= 14, $"spacing should include margin(5) + gap(10) = 15, got {spacing}");
        }

        // [CSS-FLEXBOX §8.2] gap with padding on items — padding inside item, gap between items
        [Fact]
        public void Gap_WithPaddingOnItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:10px;width:400px'>
                    <div id='a' style='width:50px;height:30px;padding-right:5px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float borderRightA = itemA.ContentRect.X + itemA.ContentRect.Width + 5;
            float gapBetween = itemB.ContentRect.X - borderRightA;
            _output.WriteLine($"a content right={itemA.ContentRect.X + itemA.ContentRect.Width} border right={borderRightA} b.X={itemB.ContentRect.X} gap={gapBetween}");
            Assert.True(System.Math.Abs(gapBetween - 10) < 2, $"gap between border boxes expected ~10, got {gapBetween}");
        }

        // [CSS-FLEXBOX §8.2] gap shorthand with two values — row-gap and column-gap
        [Fact]
        public void GapShorthand_TwoValues()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;gap:20px 10px;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 10) < 2, $"column-gap expected ~10, got {columnGap}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a and b on same line");
            float rowGap = itemC.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 20) < 2, $"row-gap expected ~20, got {rowGap}");
        }

        // [CSS-FLEXBOX §8.2] gap:0px explicit — no spacing between items
        [Fact]
        public void Gap0px_Explicit_NoSpacing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:0px;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 50) < 2, $"b.X expected ~50, got {itemB.ContentRect.X}");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 100) < 2, $"c.X expected ~100, got {itemC.ContentRect.X}");
        }

        // [CSS-FLEXBOX §8.2] large gap that exceeds available space — items overflow
        [Fact]
        public void LargeGap_ExceedsAvailableSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:200px;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float actualGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            _output.WriteLine($"large gap: a.X={itemA.ContentRect.X} b.X={itemB.ContentRect.X} gap={actualGap}");
            Assert.True(System.Math.Abs(actualGap - 200) < 2,
                $"gap should remain 200px even if overflowing, got {actualGap}");
        }

        // [CSS-FLEXBOX §8.2] gap with flex-grow unequal ratios — positions correct
        [Fact]
        public void Gap_WithFlexGrow_UnequalRatios()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:10px;width:300px'>
                    <div id='a' style='flex-grow:1;height:30px'></div>
                    <div id='b' style='flex-grow:2;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float availableForItems = 300 - 10;
            float expectedAWidth = availableForItems / 3f;
            float expectedBWidth = 2 * availableForItems / 3f;
            _output.WriteLine($"a.w={itemA.ContentRect.Width} (expected ~{expectedAWidth}) b.w={itemB.ContentRect.Width} (expected ~{expectedBWidth})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - expectedAWidth) < 2,
                $"a.Width expected ~{expectedAWidth}, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - expectedBWidth) < 2,
                $"b.Width expected ~{expectedBWidth}, got {itemB.ContentRect.Width}");
            float gap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(gap - 10) < 2, $"gap expected ~10, got {gap}");
        }

        // [CSS-FLEXBOX §9] row-gap with three wrapped lines
        [Fact]
        public void RowGap_ThreeWrappedLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:15px;width:100px'>
                    <div id='a' style='width:80px;height:20px'></div>
                    <div id='b' style='width:80px;height:20px'></div>
                    <div id='c' style='width:80px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float gapAB = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            float gapBC = itemC.ContentRect.Y - (itemB.ContentRect.Y + itemB.ContentRect.Height);
            Assert.True(System.Math.Abs(gapAB - 15) < 2, $"row-gap A-B expected ~15, got {gapAB}");
            Assert.True(System.Math.Abs(gapBC - 15) < 2, $"row-gap B-C expected ~15, got {gapBC}");
        }

        // [CSS-FLEXBOX §8.2] column-gap only — row-gap defaults to 0 in wrapped layout
        [Fact]
        public void ColumnGapOnly_RowGapDefaultsToZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:20px;width:150px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 20) < 2, $"column-gap expected ~20, got {columnGap}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a and b on same line");
            float rowGap = itemC.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(rowGap < 2, $"row-gap should default to 0, got {rowGap}");
        }

        // [CSS-FLEXBOX §8.2] gap with single item — no gap effect
        [Fact]
        public void Gap_SingleItem_NoGapEffect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:50px;width:300px'>
                    <div id='a' style='width:100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"single item a.X expected ~0, got {itemA.ContentRect.X}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2, $"single item width expected ~100, got {itemA.ContentRect.Width}");
        }
    }
}
