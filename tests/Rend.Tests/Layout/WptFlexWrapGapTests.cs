using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexWrapGapTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexWrapGapTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9] column-gap between items on same wrap line
        [Fact]
        public void Wrap_ColumnGap_SameLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:10px;width:200px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 10) < 2, $"column-gap expected 10, got {columnGap}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "items should be on same line");
        }

        // [CSS-FLEXBOX §9] row-gap between wrapped lines
        [Fact]
        public void Wrap_RowGap_BetweenLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:15px;width:100px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float rowGap = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 15) < 2, $"row-gap expected 15, got {rowGap}");
        }

        // [CSS-FLEXBOX §9] both column-gap and row-gap with wrapping
        [Fact]
        public void Wrap_BothGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:10px;row-gap:20px;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 10) < 2, $"column-gap expected 10, got {columnGap}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a and b should be on same line");
            float rowGap = itemC.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 20) < 2, $"row-gap expected 20, got {rowGap}");
        }

        // [CSS-FLEXBOX §9] gap triggers earlier wrapping when items+gap exceed container
        [Fact]
        public void Wrap_GapTriggersEarlierWrap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:30px;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemB.ContentRect.Y > itemA.ContentRect.Y + 28, "gap should force b to next line (90+30+90=210 > 200)");
        }

        // [CSS-FLEXBOX §8.4] gap with align-content:stretch distributes extra cross space
        [Fact]
        public void Wrap_Gap_AlignContent_Stretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:10px;align-content:stretch;width:100px;height:200px'>
                    <div id='a' style='width:60px'></div>
                    <div id='b' style='width:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.h={itemA.ContentRect.Height} b.h={itemB.ContentRect.Height} b.y={itemB.ContentRect.Y}");
            float expectedLineHeight = (200f - 10f) / 2f;
            Assert.True(itemA.ContentRect.Height >= expectedLineHeight - 2, $"a height expected ~{expectedLineHeight}, got {itemA.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §8.4] gap with align-content:center
        [Fact]
        public void Wrap_Gap_AlignContent_Center()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:10px;align-content:center;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalCrossSize = 30 + 10 + 30;
            float expectedOffset = (200 - totalCrossSize) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedOffset) < 2, $"a.Y expected ~{expectedOffset}, got {itemA.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (expectedOffset + 40)) < 2, $"b.Y expected ~{expectedOffset + 40}, got {itemB.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.4] gap with align-content:space-between
        [Fact]
        public void Wrap_Gap_AlignContent_SpaceBetween()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:10px;align-content:space-between;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Y < 2, $"a.Y expected ~0, got {itemA.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y + itemB.ContentRect.Height - 200) < 2, $"b bottom expected ~200, got {itemB.ContentRect.Y + itemB.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9] column wrap (flex-direction:column) with column-gap between columns
        [Fact]
        public void ColumnWrap_Gap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;column-gap:20px;width:300px;height:50px'>
                    <div id='a' style='width:50px;height:50px'></div>
                    <div id='b' style='width:50px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.x={itemA.ContentRect.X} a.w={itemA.ContentRect.Width} b.x={itemB.ContentRect.X}");
            Assert.True(itemB.ContentRect.X > itemA.ContentRect.X + itemA.ContentRect.Width, "b should be in a separate column from a");
            Assert.True(itemA.ContentRect.X < 2, "a should start at left edge");
            Assert.True(itemB.ContentRect.X > 52, "b column should be offset by at least item width + gap");
        }

        // [CSS-FLEXBOX §9] wrap-reverse with row-gap (auto height avoids stretch distribution)
        [Fact]
        public void WrapReverse_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap-reverse;row-gap:10px;width:100px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            _output.WriteLine($"a.y={itemA.ContentRect.Y} b.y={itemB.ContentRect.Y} flex.h={flexContainer.ContentRect.Height}");
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y, "wrap-reverse: first line should be below second line");
            float reverseRowGap = itemA.ContentRect.Y - (itemB.ContentRect.Y + itemB.ContentRect.Height);
            Assert.True(System.Math.Abs(reverseRowGap - 10) < 2, $"row-gap expected 10, got {reverseRowGap}");
        }

        // [CSS-FLEXBOX §9] gap with flex-grow on wrapped line
        [Fact]
        public void Wrap_Gap_FlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:20px;width:200px'>
                    <div id='a' style='flex:1 0 60px;height:30px'></div>
                    <div id='b' style='flex:1 0 60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float expectedItemWidth = (200f - 20f) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - expectedItemWidth) < 2, $"a width expected ~{expectedItemWidth}, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - expectedItemWidth) < 2, $"b width expected ~{expectedItemWidth}, got {itemB.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9] gap with justify-content:space-between
        [Fact]
        public void Wrap_Gap_JustifyContent_SpaceBetween()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:10px;justify-content:space-between;width:200px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.X < 2, $"a.X expected ~0, got {itemA.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X + itemB.ContentRect.Width - 200) < 2, $"b right edge expected ~200, got {itemB.ContentRect.X + itemB.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9] gap with justify-content:center
        [Fact]
        public void Wrap_Gap_JustifyContent_Center()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:10px;justify-content:center;width:200px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalMainSize = 50 + 10 + 50;
            float expectedOffset = (200 - totalMainSize) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedOffset) < 2, $"a.X expected ~{expectedOffset}, got {itemA.ContentRect.X}");
        }

        // [CSS-FLEXBOX §9] gap with different item sizes on wrapped lines
        [Fact]
        public void Wrap_Gap_DifferentItemSizes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:10px;row-gap:10px;width:200px'>
                    <div id='a' style='width:100px;height:40px'></div>
                    <div id='b' style='width:80px;height:50px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 10) < 2, $"column-gap expected 10, got {columnGap}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a and b should be on same line");
            float lineOneCrossSize = System.Math.Max(itemA.ContentRect.Height, itemB.ContentRect.Height);
            float expectedCY = itemA.ContentRect.Y + lineOneCrossSize + 10;
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - expectedCY) < 2, $"c.Y expected ~{expectedCY}, got {itemC.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9] gap with percentage value resolves against flex container width
        [Fact]
        public void Wrap_Gap_Percentage()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:10%;width:400px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            _output.WriteLine($"percentage column-gap={columnGap}");
            Assert.True(System.Math.Abs(columnGap - 40) < 3, $"10% of 400px expected ~40, got {columnGap}");
        }

        // [CSS-FLEXBOX §9] large gap pushes every item to its own line
        [Fact]
        public void Wrap_LargeGap_ForcesOnePerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:150px;width:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(itemB.ContentRect.Y > itemA.ContentRect.Y + 28, "b should be on next line");
            Assert.True(itemC.ContentRect.Y > itemB.ContentRect.Y + 28, "c should be on next line");
        }

        // [CSS-FLEXBOX §9] gap:0 should have no effect on layout
        [Fact]
        public void Wrap_GapZero_NoEffect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;gap:0;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap) < 2, $"gap:0 expected 0 gap, got {columnGap}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "items should be on same line");
        }

        // [CSS-FLEXBOX §9] three items wrap with gap producing two lines
        [Fact]
        public void Wrap_ThreeItems_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:10px;row-gap:10px;width:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a and b on same line");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemC.ContentRect.Y) < 2, "a and c on same line (60+10+60+10+60=200)");
        }

        // [CSS-FLEXBOX §9] four items wrap two per line with gap
        [Fact]
        public void Wrap_FourItems_TwoPerLine_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:10px;row-gap:10px;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                    <div id='d' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a and b on same line");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - itemD.ContentRect.Y) < 2, "c and d on same line");
            float rowGap = itemC.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 10) < 2, $"row-gap expected 10, got {rowGap}");
            float columnGapLine1 = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGapLine1 - 10) < 2, $"column-gap line1 expected 10, got {columnGapLine1}");
            float columnGapLine2 = itemD.ContentRect.X - (itemC.ContentRect.X + itemC.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGapLine2 - 10) < 2, $"column-gap line2 expected 10, got {columnGapLine2}");
        }

        // [CSS-FLEXBOX §9] gap shorthand sets both row-gap and column-gap
        [Fact]
        public void Wrap_GapShorthand()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;gap:15px;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 15) < 2, $"column-gap from shorthand expected 15, got {columnGap}");
            float rowGap = itemC.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 15) < 2, $"row-gap from shorthand expected 15, got {rowGap}");
        }

        // [CSS-FLEXBOX §9] gap with align-content:space-around
        [Fact]
        public void Wrap_Gap_AlignContent_SpaceAround()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:10px;align-content:space-around;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.y={itemA.ContentRect.Y} b.y={itemB.ContentRect.Y}");
            Assert.True(itemA.ContentRect.Y > 2, "space-around should push first line away from top");
            float spaceBetweenLines = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(spaceBetweenLines >= 10, $"space between lines should be at least the row-gap of 10, got {spaceBetweenLines}");
        }

        // [CSS-FLEXBOX §9] gap with align-content:flex-end
        [Fact]
        public void Wrap_Gap_AlignContent_FlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:10px;align-content:flex-end;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalCrossSize = 30 + 10 + 30;
            float expectedAY = 200 - totalCrossSize;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedAY) < 2, $"a.Y expected ~{expectedAY}, got {itemA.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y + itemB.ContentRect.Height - 200) < 2, $"b bottom expected ~200, got {itemB.ContentRect.Y + itemB.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9] column wrap with row-gap between items in same column
        [Fact]
        public void ColumnWrap_RowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;row-gap:10px;width:300px;height:120px'>
                    <div id='a' style='width:50px;height:50px'></div>
                    <div id='b' style='width:50px;height:50px'></div>
                    <div id='c' style='width:50px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float rowGap = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 10) < 2, $"row-gap expected 10, got {rowGap}");
            Assert.True(itemC.ContentRect.X > itemA.ContentRect.X + 48, "c should wrap to next column (50+10+50=110 fits, but 50+10+50+10+50=170>120)");
        }

        // [CSS-FLEXBOX §9] wrap-reverse with column-gap
        [Fact]
        public void WrapReverse_WithColumnGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;column-gap:10px;width:200px;height:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.x={itemA.ContentRect.X} a.y={itemA.ContentRect.Y} b.x={itemB.ContentRect.X} b.y={itemB.ContentRect.Y}");
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 10) < 2, $"column-gap expected 10, got {columnGap}");
        }

        // [CSS-FLEXBOX §9] container auto height includes row-gap
        [Fact]
        public void Wrap_AutoHeight_IncludesRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;row-gap:20px;width:100px'>
                    <div style='width:60px;height:40px'></div>
                    <div style='width:60px;height:50px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            float expectedHeight = 40 + 20 + 50;
            _output.WriteLine($"flex container height={flexContainer.ContentRect.Height}");
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - expectedHeight) < 2, $"auto height expected ~{expectedHeight}, got {flexContainer.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9] gap with justify-content:space-evenly
        [Fact]
        public void Wrap_Gap_JustifyContent_SpaceEvenly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:10px;justify-content:space-evenly;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(itemA.ContentRect.X > 2, "space-evenly should push first item from left edge");
            Assert.True(itemC.ContentRect.X + itemC.ContentRect.Width < 298, "space-evenly should not push last item to right edge");
        }

        // [CSS-FLEXBOX §9] five items, gap causes 3+2 wrap split
        [Fact]
        public void Wrap_FiveItems_ThreePlusTwoSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:5px;row-gap:5px;width:200px'>
                    <div id='a' style='width:60px;height:25px'></div>
                    <div id='b' style='width:60px;height:25px'></div>
                    <div id='c' style='width:60px;height:25px'></div>
                    <div id='d' style='width:60px;height:25px'></div>
                    <div id='e' style='width:60px;height:25px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            var itemE = LayoutTestHelper.FindById(root, "e")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a and b on line 1");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemC.ContentRect.Y) < 2, "a and c on line 1 (60+5+60+5+60=190<200)");
            Assert.True(itemD.ContentRect.Y > itemA.ContentRect.Y + 23, "d should wrap to line 2");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - itemE.ContentRect.Y) < 2, "d and e on line 2");
        }
    }
}
