using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexWrapLineHeightTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexWrapLineHeightTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.4] wrap 2 lines: second line Y = first line height
        [Fact]
        public void Wrap_TwoLines_SecondLineYEqualsFirstLineHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:100px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Y < 2, $"first item Y should be 0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 40) < 2, $"second item Y should be 40 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] wrap 3 lines: each line Y = cumulative height of previous lines
        [Fact]
        public void Wrap_ThreeLines_CumulativeYPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:80px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(itemA.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2, $"second line Y=30 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 70) < 2, $"third line Y=70 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] wrap line height = tallest item in that line
        [Fact]
        public void Wrap_LineHeight_EqualsHeightOfTallestItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:80px;height:20px'></div>
                    <div id='b' style='width:80px;height:60px'></div>
                    <div id='c' style='width:80px;height:30px'></div>
                </div></body>");
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Line 1: a(20)+b(60) fit in 200px, tallest=60. Line 2 starts at Y=60.
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 60) < 2, $"second line Y=60 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] wrap different heights per line: each line tracks its own tallest
        [Fact]
        public void Wrap_DifferentHeightsPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:100px'>
                    <div id='a' style='width:50px;height:25px'></div>
                    <div id='b' style='width:50px;height:45px'></div>
                    <div id='c' style='width:50px;height:15px'></div>
                    <div id='d' style='width:50px;height:35px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            // Line 1: a(25)+b(45) → tallest=45. Line 2: c(15)+d(35) → starts at Y=45.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 45) < 2, $"line 2 Y=45 (got {itemC.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 45) < 2, $"line 2 Y=45 (got {itemD.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] wrap auto container height = sum of all line cross sizes
        [Fact]
        public void Wrap_AutoContainerHeight_SumOfLineHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;width:100px'>
                    <div style='width:60px;height:30px'></div>
                    <div style='width:60px;height:50px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            // Each item on own line: 30+50=80
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - 80) < 2,
                $"auto height=80 (got {flexContainer.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] wrap auto height with row-gap adds gaps between lines
        [Fact]
        public void Wrap_AutoHeight_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;row-gap:10px;width:80px'>
                    <div style='width:50px;height:30px'></div>
                    <div style='width:50px;height:40px'></div>
                    <div style='width:50px;height:20px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            // 3 lines: 30+10+40+10+20 = 110
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - 110) < 2,
                $"auto height with gaps=110 (got {flexContainer.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §8.4] align-content:stretch expands line heights to fill container
        [Fact]
        public void Wrap_AlignContentStretch_LineHeightsExpanded()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:200px'>
                    <div id='a' style='width:60px'></div>
                    <div id='b' style='width:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // 2 lines stretched to 200/2 = 100px each
            _output.WriteLine($"a.h={itemA.ContentRect.Height} b.h={itemB.ContentRect.Height}");
            Assert.True(itemA.ContentRect.Height >= 99, $"stretched line height=100 (got {itemA.ContentRect.Height})");
            Assert.True(itemB.ContentRect.Height >= 99, $"stretched line height=100 (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §8.4] align-content:stretch with 3 lines distributes evenly
        [Fact]
        public void Wrap_AlignContentStretch_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:80px;height:300px'>
                    <div id='a' style='width:50px'></div>
                    <div id='b' style='width:50px'></div>
                    <div id='c' style='width:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // 3 lines stretched to 300/3 = 100px each
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 2, $"b.Y=100 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 200) < 2, $"c.Y=200 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] align-content:flex-start packs lines at start
        [Fact]
        public void Wrap_AlignContentFlexStart_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:100px;height:300px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 40) < 2, $"flex-start b.Y=40 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] align-content:flex-end packs lines at end
        [Fact]
        public void Wrap_AlignContentFlexEnd_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:100px;height:300px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Free = 300 - 90 = 210. Lines start at 210.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 210) < 2, $"flex-end a.Y=210 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 250) < 2, $"flex-end b.Y=250 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] align-content:center centers all lines
        [Fact]
        public void Wrap_AlignContentCenter_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:center;width:100px;height:300px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Free = 300 - 90 = 210. Center offset = 105.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 105) < 2, $"center a.Y=105 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 145) < 2, $"center b.Y=145 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] align-content:space-between distributes between first and last
        [Fact]
        public void Wrap_AlignContentSpaceBetween_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;width:80px;height:200px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Free = 200 - 90 = 110. 2 gaps of 55. a=0, b=85, c=170
            Assert.True(itemA.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 85) < 2, $"space-between b.Y=85 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 170) < 2, $"space-between c.Y=170 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] wrap-reverse: lines stack in reverse cross direction
        [Fact]
        public void WrapReverse_YPositionsReversed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // wrap-reverse: first line at bottom, second line above
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"wrap-reverse: a.Y ({itemA.ContentRect.Y}) should be below b.Y ({itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] wrap-reverse with 3 lines: bottom-to-top stacking
        [Fact]
        public void WrapReverse_ThreeLines_BottomToTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;width:80px;height:200px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // First line (a) at bottom, third line (c) at top
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"a.Y ({itemA.ContentRect.Y}) > b.Y ({itemB.ContentRect.Y})");
            Assert.True(itemB.ContentRect.Y > itemC.ContentRect.Y,
                $"b.Y ({itemB.ContentRect.Y}) > c.Y ({itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] column wrap: items wrap to next column
        [Fact]
        public void ColumnWrap_XPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;height:100px;width:300px'>
                    <div id='a' style='width:60px;height:60px'></div>
                    <div id='b' style='width:60px;height:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // a fits in column 1, b overflows (60+60=120>100) → column 2
            Assert.True(itemA.ContentRect.X < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 60) < 2,
                $"column wrap b.X=60 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.4] column wrap: 3 columns of items
        [Fact]
        public void ColumnWrap_ThreeColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;height:80px;width:400px'>
                    <div id='a' style='width:50px;height:50px'></div>
                    <div id='b' style='width:50px;height:50px'></div>
                    <div id='c' style='width:50px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(itemA.ContentRect.X < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 50) < 2, $"b.X=50 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 100) < 2, $"c.X=100 (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.4] column wrap: auto width = sum of column widths
        [Fact]
        public void ColumnWrap_AutoWidth_SumOfColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:inline-flex;flex-direction:column;flex-wrap:wrap;height:80px'>
                    <div style='width:40px;height:50px'></div>
                    <div style='width:60px;height:50px'></div>
                    <div style='width:30px;height:50px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            // Column 1: first(40px wide), Column 2: second(60px wide), Column 3: third(30px wide)
            // Column widths = max item width per column: 40+60+30=130
            _output.WriteLine($"flex.w={flexContainer.ContentRect.Width}");
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Width - 130) < 2,
                $"auto width=130 (got {flexContainer.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.4] column wrap with column-gap adds gaps between columns
        [Fact]
        public void ColumnWrap_WithColumnGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;height:80px;width:300px;column-gap:20px'>
                    <div id='a' style='width:50px;height:50px'></div>
                    <div id='b' style='width:50px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float gap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(gap - 20) < 2, $"column-gap=20 (got {gap})");
        }

        // [CSS-FLEXBOX §9.4] wrap single line: all items on one line at Y=0
        [Fact]
        public void Wrap_SingleLine_AllAtYZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:300px'>
                    <div id='a' style='width:80px;height:40px'></div>
                    <div id='b' style='width:80px;height:50px'></div>
                    <div id='c' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // 80+80+80=240 < 300 → all on one line
            Assert.True(itemA.ContentRect.Y < 2);
            Assert.True(itemB.ContentRect.Y < 2);
            Assert.True(itemC.ContentRect.Y < 2);
        }

        // [CSS-FLEXBOX §9.4] wrap exact fit: items totaling container width stay on one line
        [Fact]
        public void Wrap_ExactFit_NoWrapping()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:100px;height:30px'></div>
                    <div id='b' style='width:100px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2,
                "exact fit items should be on same line");
        }

        // [CSS-FLEXBOX §8.4] align-content:space-around with 2 wrapped lines
        [Fact]
        public void Wrap_AlignContentSpaceAround_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-around;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Free=140, 2 lines. space-around: gap=140/2=70, half-gap=35. a at 35, b at 35+30+70=135
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 35) < 2,
                $"space-around a.Y=35 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 135) < 2,
                $"space-around b.Y=135 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] align-content:space-evenly with wrapped lines
        [Fact]
        public void Wrap_AlignContentSpaceEvenly_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-evenly;width:100px;height:210px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Free=150, 3 gaps of 50: a at 50, b at 130
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 50) < 2,
                $"space-evenly a.Y=50 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 130) < 2,
                $"space-evenly b.Y=130 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] wrap with mixed line heights: auto height = sum of max-heights
        [Fact]
        public void Wrap_AutoHeight_MixedLineHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;width:150px'>
                    <div style='width:70px;height:25px'></div>
                    <div style='width:70px;height:55px'></div>
                    <div style='width:70px;height:35px'></div>
                    <div style='width:70px;height:15px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            // Line 1: 70+70=140<150, max(25,55)=55. Line 2: 70+70=140<150, max(35,15)=35. Total=90.
            _output.WriteLine($"flex.h={flexContainer.ContentRect.Height}");
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - 90) < 2,
                $"auto height=90 (got {flexContainer.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] row-gap affects Y positions of subsequent lines
        [Fact]
        public void Wrap_RowGap_AffectsSubsequentLinePositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:15px;width:80px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Line 1 height=30, gap=15, line 2 at 45. Line 2 height=40, gap=15, line 3 at 100.
            Assert.True(itemA.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 45) < 2, $"b.Y=45 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 100) < 2, $"c.Y=100 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] column wrap with column-gap: second column offset includes gap
        [Fact]
        public void ColumnWrap_ColumnGap_AffectsColumnPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;height:80px;width:300px;column-gap:15px'>
                    <div id='a' style='width:40px;height:50px'></div>
                    <div id='b' style='width:60px;height:50px'></div>
                    <div id='c' style='width:30px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Column 1: a(40px). Column 2 at 40+15=55: b(60px). Column 3 at 55+60+15=130: c(30px).
            Assert.True(itemA.ContentRect.X < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 55) < 2,
                $"b.X=55 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 130) < 2,
                $"c.X=130 (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.4] wrap two items per line with varying heights
        [Fact]
        public void Wrap_TwoItemsPerLine_VaryingLineHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:90px;height:20px'></div>
                    <div id='b' style='width:90px;height:40px'></div>
                    <div id='c' style='width:90px;height:60px'></div>
                    <div id='d' style='width:90px;height:10px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            // Line 1: a+b, 90+90=180<200, tallest=40. Line 2: c+d, tallest=60.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 40) < 2, $"line 2 Y=40 (got {itemC.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 40) < 2, $"line 2 Y=40 (got {itemD.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] align-content:flex-end with 3 wrapped lines
        [Fact]
        public void Wrap_AlignContentFlexEnd_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:80px;height:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Total line heights=90. Free=210. Lines pushed to end.
            // a at 210, b at 240, c at 280
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 210) < 2, $"a.Y=210 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 240) < 2, $"b.Y=240 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 280) < 2, $"c.Y=280 (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] align-content:center with 3 wrapped lines
        [Fact]
        public void Wrap_AlignContentCenter_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:center;width:80px;height:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Total line heights=90. Free=210. Offset=105.
            // a at 105, b at 135, c at 175
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 105) < 2, $"a.Y=105 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 135) < 2, $"b.Y=135 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 175) < 2, $"c.Y=175 (got {itemC.ContentRect.Y})");
        }
    }
}
