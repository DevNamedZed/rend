using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Flexbox multi-line (wrap) height calculations.
    /// Covers auto height sizing, explicit height, line cross sizes,
    /// row-gap, column-gap, align-content distributions, padding,
    /// border, column wrap, and wrap-reverse.
    /// </summary>
    public class WptFlexMultilineHeightTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexMultilineHeightTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.4] Auto height = sum of line cross sizes
        [Fact]
        public void WrapAutoHeight_SumOfLineHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:100px'>
                    <div style='width:60px;height:40px'></div>
                    <div style='width:60px;height:50px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 90) < 2);
        }

        // [CSS-FLEXBOX §9.4] Explicit height constrains container
        [Fact]
        public void WrapExplicitHeight_ContainerUsesExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:100px;height:300px'>
                    <div style='width:60px;height:40px'></div>
                    <div style='width:60px;height:50px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 300) < 2);
        }

        // [CSS-FLEXBOX §9.4] Line height = tallest item in that line
        [Fact]
        public void WrapLineHeight_TallestItemInLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:200px'>
                    <div style='width:90px;height:30px'></div>
                    <div style='width:90px;height:60px'></div>
                    <div id='secondLine' style='width:90px;height:20px'></div>
                </div></body>");
            // Line 1: max(30,60)=60. Line 2: 20. Total=80
            var container = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 80) < 2);
        }

        // [CSS-FLEXBOX §9.4] Two lines with different cross sizes
        [Fact]
        public void WrapTwoLines_DifferentHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:100px'>
                    <div style='width:60px;height:70px'></div>
                    <div style='width:60px;height:30px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 100) < 2);
        }

        // [CSS-FLEXBOX §9.4] Three lines summed
        [Fact]
        public void WrapThreeLines_SumOfHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:80px'>
                    <div style='width:50px;height:25px'></div>
                    <div style='width:50px;height:35px'></div>
                    <div style='width:50px;height:45px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 105) < 2);
        }

        // [CSS-FLEXBOX §9.4] row-gap adds to auto height
        [Fact]
        public void WrapWithRowGap_AddsToAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;row-gap:10px;width:100px'>
                    <div style='width:60px;height:40px'></div>
                    <div style='width:60px;height:40px'></div>
                    <div style='width:60px;height:40px'></div>
                </div></body>");
            // 3 lines of 40px + 2 gaps of 10px = 140
            var container = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 140) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:stretch distributes extra cross space
        [Fact]
        public void WrapAlignContentStretch_DistributesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:200px'>
                    <div id='t' style='width:60px'></div>
                    <div style='width:60px'></div>
                </div></body>");
            // 2 lines, each gets 100px (200/2)
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"item height={item.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:flex-start packs lines at cross start
        [Fact]
        public void WrapAlignContentFlexStart_PacksAtTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
            Assert.True(itemA.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:flex-end packs lines at cross end
        [Fact]
        public void WrapAlignContentFlexEnd_PacksAtBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            // 2 lines of 30px = 60px total. Pack at bottom: a at 140, b at 170
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 140) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 170) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:center centers lines in cross axis
        [Fact]
        public void WrapAlignContentCenter_CentersLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:center;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            // 2 lines of 30px = 60px. Free = 140. Offset = 70.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:space-between distributes lines
        [Fact]
        public void WrapAlignContentSpaceBetween_DistributesLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            // Line 1 at 0, line 2 at 170 (200-30)
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
            Assert.True(itemA.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 170) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:space-around with three lines
        [Fact]
        public void WrapAlignContentSpaceAround_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-around;width:80px;height:210px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            // 3 lines of 30px = 90px. Free = 120. 6 half-shares of 20.
            // a at 20, b at 20+30+40=90, c at 90+30+40=160
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y} c.Y={itemC.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 20) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 90) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 160) < 2);
        }

        // [CSS-FLEXBOX §9.4] Auto height with mixed item heights per line
        [Fact]
        public void WrapAutoHeight_MixedItemHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:200px'>
                    <div style='width:90px;height:20px'></div>
                    <div style='width:90px;height:50px'></div>
                    <div style='width:90px;height:15px'></div>
                    <div style='width:90px;height:35px'></div>
                </div></body>");
            // Line 1: 90+90=180<200 → max(20,50)=50
            // Line 2: 90+90=180<200 → max(15,35)=35
            // Total = 85
            var container = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 85) < 2);
        }

        // [CSS-FLEXBOX §9.4] Single wrap line = tallest item
        [Fact]
        public void WrapSingleLine_HeightIsTallest()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:300px'>
                    <div style='width:80px;height:25px'></div>
                    <div style='width:80px;height:60px'></div>
                    <div style='width:80px;height:40px'></div>
                </div></body>");
            // All fit on one line: 80*3=240<300. Height = max(25,60,40)=60
            var container = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2);
        }

        // [CSS-FLEXBOX §9.4] Container padding does not shrink line cross size
        [Fact]
        public void WrapWithPadding_PaddingAddsToOuterHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:100px;padding:15px'>
                    <div style='width:60px;height:40px'></div>
                    <div style='width:60px;height:40px'></div>
                </div></body>");
            // 2 lines of 40px = 80px content + 15px top + 15px bottom = 110px border box
            var container = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxHeight = container.ContentRect.Height + 30;
            _output.WriteLine($"content height={container.ContentRect.Height} border-box={borderBoxHeight}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 80) < 2);
        }

        // [CSS-FLEXBOX §9.4] Container border does not shrink line cross size
        [Fact]
        public void WrapWithBorder_BorderAddsToOuterHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:100px;border:5px solid black'>
                    <div style='width:60px;height:40px'></div>
                    <div style='width:60px;height:40px'></div>
                </div></body>");
            // 2 lines of 40px = 80px content height
            var container = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"content height={container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 80) < 2);
        }

        // [CSS-FLEXBOX §9.4] Column wrap: items wrap to next column
        [Fact]
        public void ColumnWrap_ItemsWrapToNextColumn()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:100px;width:300px'>
                    <div id='a' style='width:60px;height:60px'></div>
                    <div id='b' style='width:80px;height:60px'></div>
                </div></body>");
            // Item a: 60px high, fits in column 1. Item b: 60+60=120>100, wraps to column 2.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.X={itemA.ContentRect.X} b.X={itemB.ContentRect.X}");
            Assert.True(itemA.ContentRect.X < 2);
            Assert.True(itemB.ContentRect.X >= 58);
        }

        // [CSS-FLEXBOX §9.4] Column wrap with column-gap increases column spacing
        [Fact]
        public void ColumnWrapWithColumnGap_IncreasesColumnSpacing()
        {
            // Without gap
            var rootNoGap = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:100px;width:300px'>
                    <div id='a' style='width:60px;height:60px'></div>
                    <div id='b' style='width:80px;height:60px'></div>
                </div></body>");
            float noGapSpacing = LayoutTestHelper.FindById(rootNoGap, "b")!.ContentRect.X
                - LayoutTestHelper.FindById(rootNoGap, "a")!.ContentRect.X;

            // With gap
            var rootWithGap = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:100px;column-gap:20px;width:300px'>
                    <div id='a' style='width:60px;height:60px'></div>
                    <div id='b' style='width:80px;height:60px'></div>
                </div></body>");
            float gapSpacing = LayoutTestHelper.FindById(rootWithGap, "b")!.ContentRect.X
                - LayoutTestHelper.FindById(rootWithGap, "a")!.ContentRect.X;

            _output.WriteLine($"noGap spacing={noGapSpacing} withGap spacing={gapSpacing}");
            Assert.True(gapSpacing > noGapSpacing);
        }

        // [CSS-FLEXBOX §9.4] wrap-reverse has same auto height as wrap
        [Fact]
        public void WrapReverse_SameAutoHeightAsWrap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap-reverse;width:100px'>
                    <div style='width:60px;height:40px'></div>
                    <div style='width:60px;height:50px'></div>
                </div></body>");
            // Same as wrap: 40+50=90
            var container = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 90) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:space-evenly with two lines
        [Fact]
        public void WrapAlignContentSpaceEvenly_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-evenly;width:100px;height:210px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            // 2 lines of 30px = 60px. Free = 150. 3 gaps of 50.
            // a at 50, b at 130
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 130) < 2);
        }

        // [CSS-FLEXBOX §9.4] Row-gap with two lines of different heights
        [Fact]
        public void WrapRowGap_DifferentLineHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;row-gap:15px;width:100px'>
                    <div style='width:60px;height:50px'></div>
                    <div style='width:60px;height:30px'></div>
                </div></body>");
            // Line 1: 50px. Gap: 15px. Line 2: 30px. Total = 95
            var container = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 95) < 2);
        }

        // [CSS-FLEXBOX §9.4] Four items wrapping into two lines
        [Fact]
        public void WrapFourItems_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:200px'>
                    <div style='width:90px;height:40px'></div>
                    <div style='width:90px;height:40px'></div>
                    <div style='width:90px;height:60px'></div>
                    <div style='width:90px;height:60px'></div>
                </div></body>");
            // Line 1: 90+90=180<200 → 40px. Line 2: 90+90=180<200 → 60px. Total = 100
            var container = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"container height={container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 100) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:stretch with row-gap
        [Fact]
        public void WrapAlignContentStretch_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;row-gap:20px;width:100px;height:220px'>
                    <div id='a' style='width:60px'></div>
                    <div id='b' style='width:60px'></div>
                </div></body>");
            // 2 lines. Container 220px - 1 gap of 20px = 200px distributable. Each line = 100px.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.h={itemA.ContentRect.Height} b.h={itemB.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 100) < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content:space-between with three lines
        [Fact]
        public void WrapAlignContentSpaceBetween_ThreeLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;width:80px;height:200px'>
                    <div id='a' style='width:50px;height:20px'></div>
                    <div id='b' style='width:50px;height:20px'></div>
                    <div id='c' style='width:50px;height:20px'></div>
                </div></body>");
            // 3 lines of 20px = 60px. Free = 140. 2 gaps of 70.
            // a at 0, b at 90, c at 180
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y} c.Y={itemC.ContentRect.Y}");
            Assert.True(itemA.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 90) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 180) < 2);
        }

        // [CSS-FLEXBOX §9.4] Column wrap three columns: items placed in correct columns
        [Fact]
        public void ColumnWrapThreeColumns_ItemPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:60px;width:300px'>
                    <div id='a' style='width:40px;height:40px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:30px;height:40px'></div>
                </div></body>");
            // Column 1: item a (40px high, fits). Column 2: item b wraps (40+40>60).
            // Column 3: item c wraps. Each in separate column.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.X={itemA.ContentRect.X} b.X={itemB.ContentRect.X} c.X={itemC.ContentRect.X}");
            Assert.True(itemA.ContentRect.X < 2);
            Assert.True(itemB.ContentRect.X >= 38);
            Assert.True(itemC.ContentRect.X > itemB.ContentRect.X);
        }

        // [CSS-FLEXBOX §9.4] Wrap with padding and border combined
        [Fact]
        public void WrapWithPaddingAndBorder_ContentHeightUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:100px;padding:10px;border:5px solid black'>
                    <div style='width:60px;height:30px'></div>
                    <div style='width:60px;height:30px'></div>
                </div></body>");
            // 2 lines of 30px = 60px content height
            var container = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"content height={container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2);
        }

        // [CSS-FLEXBOX §9.4] wrap-reverse with explicit height reverses line order
        [Fact]
        public void WrapReverse_ExplicitHeight_LinesReversed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            // wrap-reverse: first line goes to bottom, second line above it
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y);
        }
    }
}
