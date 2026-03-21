using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive tests for CSS Flexbox flex-wrap item positioning: covers nowrap,
    /// wrap, wrap-reverse, column wrap, gaps, align-content variants, justify-content,
    /// flex-grow per line, auto height, and exact-fit scenarios.
    /// </summary>
    public class WptFlexAllWrapPositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexAllWrapPositionTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.3] nowrap: two items stay on the same Y
        [Fact]
        public void Nowrap_TwoItems_SameY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:nowrap;width:200px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2,
                $"nowrap: a.Y={itemA.ContentRect.Y} should equal b.Y={itemB.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9.3] nowrap: second item X follows first item width
        [Fact]
        public void Nowrap_TwoItems_XPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:nowrap;width:200px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2,
                $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 80) < 2,
                $"b X=80 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] wrap two lines: Y positions stacked by line height
        [Fact]
        public void Wrap_TwoLines_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:100px'>
                    <div id='a' style='width:60px;height:25px'></div>
                    <div id='b' style='width:60px;height:35px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2,
                $"Line 1 Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 25) < 2,
                $"Line 2 Y=25 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] wrap three lines: cumulative Y positions
        [Fact]
        public void Wrap_ThreeLines_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:50px'>
                    <div id='a' style='width:40px;height:15px'></div>
                    <div id='b' style='width:40px;height:25px'></div>
                    <div id='c' style='width:40px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2,
                $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 15) < 2,
                $"b Y=15 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 40) < 2,
                $"c Y=40 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] wrap: different item counts per line (3 + 1)
        [Fact]
        public void Wrap_DifferentItemCountsPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:250px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                    <div id='d' style='width:120px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a and b on line 1");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemC.ContentRect.Y) < 2,
                "a, b, c on line 1 (70+80+90=240<250)");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 30) < 2,
                $"d wraps to line 2 Y=30 (got {itemD.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] wrap: auto height with two lines
        [Fact]
        public void Wrap_AutoHeight_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;width:100px'>
                    <div style='width:60px;height:30px'></div>
                    <div style='width:60px;height:40px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            float expectedHeight = 30 + 40;
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - expectedHeight) < 2,
                $"auto height = 70 (got {flexContainer.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.3] wrap: auto height with three lines
        [Fact]
        public void Wrap_AutoHeight_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;width:50px'>
                    <div style='width:40px;height:20px'></div>
                    <div style='width:40px;height:30px'></div>
                    <div style='width:40px;height:25px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            float expectedHeight = 20 + 30 + 25;
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - expectedHeight) < 2,
                $"auto height = 75 (got {flexContainer.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9] wrap with row-gap two lines
        [Fact]
        public void Wrap_RowGap_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:12px;width:80px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:25px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float actualGap = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(actualGap - 12) < 2,
                $"row-gap=12 (got {actualGap})");
        }

        // [CSS-FLEXBOX §9] wrap with row-gap three lines
        [Fact]
        public void Wrap_RowGap_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:8px;width:50px'>
                    <div id='a' style='width:40px;height:20px'></div>
                    <div id='b' style='width:40px;height:15px'></div>
                    <div id='c' style='width:40px;height:25px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 28) < 2,
                $"b Y=20+8=28 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 51) < 2,
                $"c Y=28+15+8=51 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9] wrap with column-gap between items on same line
        [Fact]
        public void Wrap_ColumnGap_SameLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:15px;width:250px'>
                    <div id='a' style='width:100px;height:30px'></div>
                    <div id='b' style='width:100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float gap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(gap - 15) < 2,
                $"column-gap=15 (got {gap})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2,
                "items should be on same line");
        }

        // [CSS-FLEXBOX §9] wrap with both row-gap and column-gap
        [Fact]
        public void Wrap_BothGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:10px;column-gap:20px;width:220px'>
                    <div id='a' style='width:90px;height:35px'></div>
                    <div id='b' style='width:90px;height:35px'></div>
                    <div id='c' style='width:90px;height:35px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 20) < 2,
                $"column-gap=20 (got {columnGap})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2,
                "a and b on same line (90+20+90=200<220)");
            float rowGap = itemC.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 10) < 2,
                $"row-gap=10 (got {rowGap})");
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:flex-start packs lines at top
        [Fact]
        public void Wrap_AlignContent_Start_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:80px;height:200px'>
                    <div id='a' style='width:70px;height:25px'></div>
                    <div id='b' style='width:70px;height:35px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2,
                $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 25) < 2,
                $"b Y=25 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:flex-end pushes lines to bottom
        [Fact]
        public void Wrap_AlignContent_End_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:80px;height:200px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 120) < 2,
                $"a Y=200-80=120 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 150) < 2,
                $"b Y=120+30=150 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:center with two lines
        [Fact]
        public void Wrap_AlignContent_Center_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:center;width:80px;height:200px'>
                    <div id='a' style='width:70px;height:40px'></div>
                    <div id='b' style='width:70px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalCross = 40 + 40;
            float offset = (200 - totalCross) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - offset) < 2,
                $"a Y={offset} (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (offset + 40)) < 2,
                $"b Y={offset + 40} (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:space-between with two lines
        [Fact]
        public void Wrap_AlignContent_SpaceBetween_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;width:80px;height:200px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2,
                $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 170) < 2,
                $"b Y=170 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:stretch with two lines
        [Fact]
        public void Wrap_AlignContent_Stretch_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:80px;height:180px'>
                    <div id='a' style='width:70px'></div>
                    <div id='b' style='width:70px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Height >= 88,
                $"a stretched to ~90 (got {itemA.ContentRect.Height})");
            Assert.True(itemB.ContentRect.Y >= 88,
                $"b Y ~90 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:flex-start with three lines
        [Fact]
        public void Wrap_AlignContent_Start_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:60px;height:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2,
                $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2,
                $"b Y=30 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 70) < 2,
                $"c Y=70 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:flex-end with three lines
        [Fact]
        public void Wrap_AlignContent_End_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:60px;height:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float totalCross = 30 + 40 + 20;
            float offset = 300 - totalCross;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - offset) < 2,
                $"a Y={offset} (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (offset + 30)) < 2,
                $"b Y={offset + 30} (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - (offset + 70)) < 2,
                $"c Y={offset + 70} (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:center with three lines
        [Fact]
        public void Wrap_AlignContent_Center_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:center;width:60px;height:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float totalCross = 30 + 40 + 20;
            float offset = (300 - totalCross) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - offset) < 2,
                $"a Y={offset} (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (offset + 30)) < 2,
                $"b Y={offset + 30} (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - (offset + 70)) < 2,
                $"c Y={offset + 70} (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:space-between with three lines
        [Fact]
        public void Wrap_AlignContent_SpaceBetween_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;width:60px;height:200px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float totalCross = 30 * 3;
            float freeSpace = 200 - totalCross;
            float gapBetween = freeSpace / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2,
                $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (30 + gapBetween)) < 2,
                $"b Y={30 + gapBetween} (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 170) < 2,
                $"c Y=170 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:stretch with three lines
        [Fact]
        public void Wrap_AlignContent_Stretch_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:60px;height:210px'>
                    <div id='a' style='width:50px'></div>
                    <div id='b' style='width:50px'></div>
                    <div id='c' style='width:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float expectedLineHeight = 210f / 3f;
            Assert.True(itemA.ContentRect.Height >= expectedLineHeight - 2,
                $"a stretched to ~{expectedLineHeight} (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - expectedLineHeight) < 2,
                $"b Y~{expectedLineHeight} (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - expectedLineHeight * 2) < 2,
                $"c Y~{expectedLineHeight * 2} (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] wrap-reverse: two lines, first line below second
        [Fact]
        public void WrapReverse_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;align-content:flex-start;width:80px;height:120px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"wrap-reverse: a.Y={itemA.ContentRect.Y} should be > b.Y={itemB.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9.3] wrap-reverse: three lines, reversed stacking
        [Fact]
        public void WrapReverse_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;align-content:flex-start;width:60px;height:200px'>
                    <div id='a' style='width:50px;height:25px'></div>
                    <div id='b' style='width:50px;height:25px'></div>
                    <div id='c' style='width:50px;height:25px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"a.Y={itemA.ContentRect.Y} > b.Y={itemB.ContentRect.Y}");
            Assert.True(itemB.ContentRect.Y > itemC.ContentRect.Y,
                $"b.Y={itemB.ContentRect.Y} > c.Y={itemC.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9.3] column wrap: two columns, X positions
        [Fact]
        public void ColumnWrap_TwoColumns_XPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;width:300px;height:60px'>
                    <div id='a' style='width:70px;height:50px'></div>
                    <div id='b' style='width:80px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2,
                $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 70) < 2,
                $"b X=70 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y) < 2,
                $"b Y=0 in new column (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] column wrap: three columns, X positions
        [Fact]
        public void ColumnWrap_ThreeColumns_XPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;width:400px;height:50px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:70px;height:40px'></div>
                    <div id='c' style='width:80px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2,
                $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 60) < 2,
                $"b X=60 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 130) < 2,
                $"c X=130 (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] column wrap with column-gap between columns
        [Fact]
        public void ColumnWrap_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;column-gap:15px;width:300px;height:50px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2,
                $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 75) < 2,
                $"b X=60+15=75 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] wrap exact fit: items that sum exactly to container do not wrap
        [Fact]
        public void Wrap_ExactFit_NoWrapping()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:150px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2,
                "a and b on same line");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemC.ContentRect.Y) < 2,
                "all three items on single line (50+50+50=150)");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 100) < 2,
                $"c X=100 (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] wrap with flex-grow distributes space per line
        [Fact]
        public void Wrap_FlexGrow_PerLine()
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
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"a alone on line 1, grows to 200 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"b shares line 2 equally, 100 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2,
                $"c shares line 2 equally, 100 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §8.2] wrap + justify-content:center per line
        [Fact]
        public void Wrap_JustifyContent_Center_PerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:center;width:200px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:150px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float line1Free = 200 - 100;
            float line1Offset = line1Free / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - line1Offset) < 2,
                $"a X={line1Offset} (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - (line1Offset + 50)) < 2,
                $"b X={line1Offset + 50} (got {itemB.ContentRect.X})");
            float line2Free = 200 - 150;
            float line2Offset = line2Free / 2f;
            Assert.True(System.Math.Abs(itemC.ContentRect.X - line2Offset) < 2,
                $"c X={line2Offset} (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] wrap: line height determined by tallest item
        [Fact]
        public void Wrap_LineHeight_TallestItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:90px;height:20px'></div>
                    <div id='b' style='width:90px;height:60px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                </div></body>");
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 60) < 2,
                $"Line 2 starts at tallest item height=60 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] column wrap-reverse: columns go right to left
        [Fact]
        public void ColumnWrapReverse_ColumnOrder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap-reverse;width:200px;height:50px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.X > itemB.ContentRect.X,
                $"column wrap-reverse: a.X={itemA.ContentRect.X} > b.X={itemB.ContentRect.X}");
        }

        // [CSS-FLEXBOX §9.3] wrap with 5 items: 2+2+1 distribution
        [Fact]
        public void Wrap_FiveItems_TwoPlusTwoPlusOneDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:150px'>
                    <div id='a' style='width:70px;height:20px'></div>
                    <div id='b' style='width:70px;height:20px'></div>
                    <div id='c' style='width:70px;height:25px'></div>
                    <div id='d' style='width:70px;height:25px'></div>
                    <div id='e' style='width:70px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            var itemE = LayoutTestHelper.FindById(root, "e")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2,
                "a and b on line 1");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - itemD.ContentRect.Y) < 2,
                "c and d on line 2");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 20) < 2,
                $"line 2 Y=20 (got {itemC.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemE.ContentRect.Y - 45) < 2,
                $"line 3 Y=20+25=45 (got {itemE.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:space-around with two lines
        [Fact]
        public void Wrap_AlignContent_SpaceAround_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-around;width:80px;height:200px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalCross = 30 + 30;
            float freeSpace = 200 - totalCross;
            float halfGap = freeSpace / 4f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - halfGap) < 2,
                $"a Y={halfGap} (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (halfGap + 30 + halfGap * 2)) < 2,
                $"b Y={halfGap + 30 + halfGap * 2} (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:space-evenly with two lines
        [Fact]
        public void Wrap_AlignContent_SpaceEvenly_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-evenly;width:80px;height:210px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalCross = 30 + 30;
            float freeSpace = 210 - totalCross;
            float evenGap = freeSpace / 3f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - evenGap) < 2,
                $"a Y={evenGap} (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (evenGap + 30 + evenGap)) < 2,
                $"b Y={evenGap + 30 + evenGap} (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] nowrap: items overflow rather than wrapping
        [Fact]
        public void Nowrap_ItemsOverflow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:nowrap;width:100px'>
                    <div id='a' style='width:60px;height:30px;flex-shrink:0'></div>
                    <div id='b' style='width:60px;height:30px;flex-shrink:0'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2,
                "nowrap: both items remain on same line even when overflowing");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 60) < 2,
                $"b X=60 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] wrap auto height includes row-gap between lines
        [Fact]
        public void Wrap_AutoHeight_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;row-gap:15px;width:80px'>
                    <div style='width:70px;height:30px'></div>
                    <div style='width:70px;height:40px'></div>
                    <div style='width:70px;height:20px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            float expectedHeight = 30 + 15 + 40 + 15 + 20;
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - expectedHeight) < 2,
                $"auto height=120 (got {flexContainer.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.3] wrap-reverse with auto height
        [Fact]
        public void WrapReverse_AutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap-reverse;width:80px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:40px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            float expectedHeight = 30 + 40;
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - expectedHeight) < 2,
                $"auto height=70 (got {flexContainer.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §8.2] wrap + justify-content:flex-end per line
        [Fact]
        public void Wrap_JustifyContent_FlexEnd_PerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:flex-end;width:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                    <div id='c' style='width:150px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemB.ContentRect.X + itemB.ContentRect.Width - 200) < 2,
                $"line 1 right-aligned: b right edge at 200 (got {itemB.ContentRect.X + itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X + itemC.ContentRect.Width - 200) < 2,
                $"line 2 right-aligned: c right edge at 200 (got {itemC.ContentRect.X + itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §8.2] wrap + justify-content:space-between distributes per line
        [Fact]
        public void Wrap_JustifyContent_SpaceBetween_TwoPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:space-between;width:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                    <div id='d' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2,
                $"a at left (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 70) < 2,
                $"b at middle (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X + itemC.ContentRect.Width - 200) < 2,
                $"c at right edge line 1 (got {itemC.ContentRect.X + itemC.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemD.ContentRect.X) < 2,
                $"d at left of line 2 (got {itemD.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] column wrap: two items in first column, one wraps
        [Fact]
        public void ColumnWrap_TwoInFirstColumn_OneWraps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;width:300px;height:80px'>
                    <div id='a' style='width:60px;height:35px'></div>
                    <div id='b' style='width:60px;height:35px'></div>
                    <div id='c' style='width:60px;height:35px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2,
                $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2,
                $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X) < 2,
                $"b X=0, same column (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 35) < 2,
                $"b Y=35 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 60) < 2,
                $"c wraps to X=60 (got {itemC.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y) < 2,
                $"c Y=0 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] wrap: single item does not wrap
        [Fact]
        public void Wrap_SingleItem_NoWrap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:100px'>
                    <div id='a' style='width:80px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2,
                $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2,
                $"a Y=0 (got {itemA.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] wrap with flex-grow: unequal grow factors on wrapped line
        [Fact]
        public void Wrap_FlexGrow_UnequalFactors()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:300px'>
                    <div id='a' style='flex:1 0 100px;height:30px'></div>
                    <div id='b' style='flex:3 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 125) < 2,
                $"a gets 100+25=125 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 175) < 2,
                $"b gets 100+75=175 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §8.2] wrap + justify-content:space-evenly per line
        [Fact]
        public void Wrap_JustifyContent_SpaceEvenly_PerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:space-evenly;width:200px'>
                    <div id='a' style='width:40px;height:30px'></div>
                    <div id='b' style='width:40px;height:30px'></div>
                    <div id='c' style='width:40px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float totalItems = 40 * 3;
            float freeSpace = 200 - totalItems;
            float evenGap = freeSpace / 4f;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - evenGap) < 2,
                $"a X={evenGap} (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2,
                "all on same line");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemC.ContentRect.Y) < 2,
                "all on same line");
        }

        // [CSS-FLEXBOX §9.3] column wrap with row-gap between items in same column
        [Fact]
        public void ColumnWrap_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;row-gap:10px;width:300px;height:100px'>
                    <div id='a' style='width:50px;height:40px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float rowGap = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 10) < 2,
                $"row-gap=10 (got {rowGap})");
            Assert.True(itemC.ContentRect.X > itemA.ContentRect.X + 48,
                $"c wraps to next column (got X={itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] column wrap-reverse: three columns reversed
        [Fact]
        public void ColumnWrapReverse_ThreeColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap-reverse;width:300px;height:50px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                    <div id='c' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(itemA.ContentRect.X > itemB.ContentRect.X,
                $"a.X={itemA.ContentRect.X} > b.X={itemB.ContentRect.X}");
            Assert.True(itemB.ContentRect.X > itemC.ContentRect.X,
                $"b.X={itemB.ContentRect.X} > c.X={itemC.ContentRect.X}");
        }

        // [CSS-FLEXBOX §9.3] wrap-reverse + align-content:center
        [Fact]
        public void WrapReverse_AlignContent_Center()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;align-content:center;width:80px;height:200px'>
                    <div id='a' style='width:70px;height:30px'></div>
                    <div id='b' style='width:70px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"wrap-reverse: a.Y={itemA.ContentRect.Y} > b.Y={itemB.ContentRect.Y}");
            float midpoint = (itemB.ContentRect.Y + itemA.ContentRect.Y + itemA.ContentRect.Height) / 2f;
            Assert.True(System.Math.Abs(midpoint - 100) < 2,
                $"lines centered at 100 (got midpoint={midpoint})");
        }

        // [CSS-FLEXBOX §9.3] wrap: mixed widths cause uneven line fill
        [Fact]
        public void Wrap_MixedWidths_UnevenLineFill()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:120px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:80px;height:30px'></div>
                    <div id='d' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2,
                "a and b on line 1 (120+50=170<200)");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - itemD.ContentRect.Y) < 2,
                "c and d on line 2 (80+80=160<200)");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 30) < 2,
                $"line 2 Y=30 (got {itemC.ContentRect.Y})");
        }
    }
}
