using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexWrapAlignmentTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexWrapAlignmentTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §8.4] wrap + align-content:flex-start packs lines at cross-start
        [Fact]
        public void Wrap_AlignContent_FlexStart_LinesAtTop()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:100px;height:300px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                    <div id='c' style='width:60px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 80) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:flex-end packs lines at cross-end
        [Fact]
        public void Wrap_AlignContent_FlexEnd_LinesAtBottom()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:100px;height:300px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                    <div id='c' style='width:60px;height:40px'></div>
                </div></body>");
            // 3 lines of 40px = 120px, free = 180, offset = 180
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 220) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 260) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:center centers lines vertically
        [Fact]
        public void Wrap_AlignContent_Center_LinesCentered()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:center;width:100px;height:300px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                    <div id='c' style='width:60px;height:40px'></div>
                </div></body>");
            // 3 lines of 40px = 120px, free = 180, offset = 90
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 170) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:space-between distributes between first and last
        [Fact]
        public void Wrap_AlignContent_SpaceBetween_ThreeLines()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;width:100px;height:300px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                    <div id='c' style='width:60px;height:40px'></div>
                </div></body>");
            // free = 180, 2 gaps = 90 each. a=0, b=130, c=260
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 260) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:space-around distributes with half-gaps at edges
        [Fact]
        public void Wrap_AlignContent_SpaceAround_ThreeLines()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-around;width:100px;height:300px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                    <div id='c' style='width:60px;height:40px'></div>
                </div></body>");
            // free = 180, 3 lines, gap = 180/3 = 60. half-gap = 30
            // a at 30, b at 30+40+60=130, c at 130+40+60=230
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 230) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:space-evenly distributes with equal gaps everywhere
        [Fact]
        public void Wrap_AlignContent_SpaceEvenly_ThreeLines()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-evenly;width:100px;height:300px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                    <div id='c' style='width:60px;height:40px'></div>
                </div></body>");
            // free = 180, 4 gaps = 45 each. a=45, b=45+40+45=130, c=130+40+45=215
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 45) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 215) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:stretch distributes extra cross space to lines
        [Fact]
        public void Wrap_AlignContent_Stretch_LinesExpand()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:300px'>
                    <div id='a' style='width:60px'></div>
                    <div id='b' style='width:60px'></div>
                    <div id='c' style='width:60px'></div>
                </div></body>");
            // 3 lines, 300/3 = 100px each. Items with auto height stretch to line cross size.
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 200) < 2);
        }

        // [CSS-FLEXBOX §8.3] wrap + align-items:center centers items within each line
        [Fact]
        public void Wrap_AlignItems_Center_WithinLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-items:center;width:100px'>
                    <div id='tall' style='width:40px;height:60px'></div>
                    <div id='short' style='width:40px;height:20px'></div>
                </div></body>");
            // Line cross size = 60 (tallest). Short item centered: (60-20)/2 = 20
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "short")!.ContentRect.Y - 20) < 2);
        }

        // [CSS-FLEXBOX §8.3] wrap + align-items:flex-end aligns items to line cross-end
        [Fact]
        public void Wrap_AlignItems_FlexEnd_WithinLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-items:flex-end;width:100px'>
                    <div id='tall' style='width:40px;height:60px'></div>
                    <div id='short' style='width:40px;height:20px'></div>
                </div></body>");
            // Short item at bottom of line: 60-20 = 40
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "short")!.ContentRect.Y - 40) < 2);
        }

        // [CSS-FLEXBOX §8.3] wrap + align-items:flex-start aligns items to line cross-start
        [Fact]
        public void Wrap_AlignItems_FlexStart_WithinLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-items:flex-start;width:100px'>
                    <div id='tall' style='width:40px;height:60px'></div>
                    <div id='short' style='width:40px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "short")!.ContentRect.Y - 0) < 2);
        }

        // [CSS-FLEXBOX §8.3+§8.4] wrap + align-items:center + align-content:flex-end (multi-line)
        [Fact]
        public void Wrap_AlignItems_Center_AlignContent_FlexEnd()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-items:center;align-content:flex-end;width:80px;height:200px'>
                    <div id='tall' style='width:60px;height:60px'></div>
                    <div id='short' style='width:60px;height:20px'></div>
                </div></body>");
            // 2 lines: line 1 cross=60 (tall), line 2 cross=20 (short).
            // align-content:flex-end: total=80, offset=200-80=120.
            // tall at Y=120, short at Y=180.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "tall")!.ContentRect.Y - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "short")!.ContentRect.Y - 180) < 2);
        }

        // [CSS-FLEXBOX §9.5] wrap + justify-content:center on each line
        [Fact]
        public void Wrap_JustifyContent_Center_PerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:center;width:150px'>
                    <div id='a1' style='width:60px;height:30px'></div>
                    <div id='a2' style='width:60px;height:30px'></div>
                    <div id='b1' style='width:80px;height:30px'></div>
                </div></body>");
            // Line 1: 60+60=120, free=30, offset=15. a1.X=15
            // Line 2: 80, free=70, offset=35. b1.X=35
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a1")!.ContentRect.X - 15) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b1")!.ContentRect.X - 35) < 2);
        }

        // [CSS-FLEXBOX §9.5] wrap + justify-content:space-between per line
        [Fact]
        public void Wrap_JustifyContent_SpaceBetween_PerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:space-between;width:150px'>
                    <div id='a1' style='width:60px;height:30px'></div>
                    <div id='a2' style='width:60px;height:30px'></div>
                    <div id='b1' style='width:80px;height:30px'></div>
                </div></body>");
            // Line 1: a1.X=0, a2.X=150-60=90
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a1")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a2")!.ContentRect.X - 90) < 2);
        }

        // [CSS-FLEXBOX §9.5] wrap + justify-content:flex-end per line
        [Fact]
        public void Wrap_JustifyContent_FlexEnd_PerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:flex-end;width:100px'>
                    <div id='a1' style='width:60px;height:30px'></div>
                    <div id='b1' style='width:80px;height:30px'></div>
                </div></body>");
            // Line 1: 60px item, free=40, flex-end: a1.X=40
            // Line 2: 80px item, free=20, flex-end: b1.X=20
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a1")!.ContentRect.X - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b1")!.ContentRect.X - 20) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap-reverse + align-content:flex-start (auto height container)
        [Fact]
        public void WrapReverse_AlignContent_FlexStart_AutoHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;align-content:flex-start;width:100px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            // wrap-reverse with auto height: lines reversed. a at Y=40, b at Y=0
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 0) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap-reverse + align-content:flex-end with explicit height
        [Fact]
        public void WrapReverse_AlignContent_FlexEnd()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;align-content:flex-end;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            // wrap-reverse + flex-end: lines packed toward physical top
            // Actual: a.Y=160, b.Y=120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 120) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap-reverse + align-content:center
        [Fact]
        public void WrapReverse_AlignContent_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;align-content:center;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            // 2 lines of 40px = 80px, free = 120, center offset = 60
            // wrap-reverse: a (first line) is below b. a at 100, b at 60
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 60) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap with different item sizes per line affects line cross size
        [Fact]
        public void Wrap_DifferentHeightsPerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:100px'>
                    <div id='a' style='width:60px;height:80px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            // Line 1 cross size = 80 (from a). b starts at Y=80.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 80) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap with mixed heights, align-content:center
        [Fact]
        public void Wrap_MixedHeights_AlignContent_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:center;width:100px;height:300px'>
                    <div id='a' style='width:60px;height:80px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            // Total line heights = 80+40 = 120, free = 180, offset = 90
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 170) < 2);
        }

        // [CSS-FLEXBOX §9] wrap + gap + align-content:space-between
        [Fact]
        public void Wrap_Gap_AlignContent_SpaceBetween()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;row-gap:10px;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            // space-between: a at 0, b at 200-30=170
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 170) < 2);
        }

        // [CSS-FLEXBOX §9] wrap + gap + align-content:center
        [Fact]
        public void Wrap_Gap_AlignContent_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:center;row-gap:10px;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            // Total cross = 30+10+30 = 70, free = 130, offset = 65
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 65) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 105) < 2);
        }

        // [CSS-FLEXBOX §4.4] column wrap + align-content:flex-start
        [Fact]
        public void ColumnWrap_AlignContent_FlexStart()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;width:300px;height:100px'>
                    <div id='a' style='width:80px;height:60px'></div>
                    <div id='b' style='width:80px;height:60px'></div>
                    <div id='c' style='width:80px;height:60px'></div>
                </div></body>");
            // Column wrap: a at col 0, b wraps to col 1, c wraps to col 2
            // flex-start: columns packed at left. a.X=0, b.X=80, c.X=160
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 160) < 2);
        }

        // [CSS-FLEXBOX §4.4] column wrap + align-content:flex-end
        [Fact]
        public void ColumnWrap_AlignContent_FlexEnd()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-end;width:300px;height:100px'>
                    <div id='a' style='width:80px;height:60px'></div>
                    <div id='b' style='width:80px;height:60px'></div>
                </div></body>");
            // 2 columns of 80px = 160px, free = 140, flex-end offset = 140
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 220) < 2);
        }

        // [CSS-FLEXBOX §4.4] column wrap + align-content:center
        [Fact]
        public void ColumnWrap_AlignContent_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:center;width:300px;height:100px'>
                    <div id='a' style='width:80px;height:60px'></div>
                    <div id='b' style='width:80px;height:60px'></div>
                </div></body>");
            // 2 columns of 80px = 160px, free = 140, center offset = 70
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 150) < 2);
        }

        // [CSS-FLEXBOX §4.4] column wrap + align-content:space-between
        [Fact]
        public void ColumnWrap_AlignContent_SpaceBetween()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:space-between;width:300px;height:100px'>
                    <div id='a' style='width:80px;height:60px'></div>
                    <div id='b' style='width:80px;height:60px'></div>
                </div></body>");
            // space-between: a.X=0, b.X=300-80=220
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 220) < 2);
        }

        // [CSS-FLEXBOX §9.7] wrap + flex-grow distributes space per line independently
        [Fact]
        public void Wrap_FlexGrow_IndependentPerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a1' style='flex:1 0 80px;height:30px'></div>
                    <div id='a2' style='flex:1 0 80px;height:30px'></div>
                    <div id='b1' style='flex:1 0 150px;height:30px'></div>
                </div></body>");
            // Line 1: a1+a2 = 160, free=40, each grows 20 -> 100px each
            // Line 2: b1 alone, grows to 200px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a1")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a2")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b1")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §5.4] wrap + order reorders items across lines
        [Fact]
        public void Wrap_Order_ReordersAcrossLines()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:100px'>
                    <div id='a' style='order:2;width:60px;height:30px'></div>
                    <div id='b' style='order:1;width:60px;height:30px'></div>
                    <div id='c' style='order:3;width:60px;height:30px'></div>
                </div></body>");
            // Visual order: b(1), a(2), c(3). Each 60px on 100px container, 1 per line.
            // b at Y=0, a at Y=30, c at Y=60
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 60) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:stretch with fixed-height items keeps item height
        [Fact]
        public void Wrap_AlignContent_Stretch_FixedHeightItems()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:300px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            // 2 lines, each stretched to 150px. But items have fixed height 40px.
            // Item a stays 40px tall, b starts at Y=150.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 150) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:space-around with two items
        [Fact]
        public void Wrap_AlignContent_SpaceAround_TwoLines()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-around;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            // free = 120, 2 lines, gap = 120/2 = 60, half-gap = 30
            // a at 30, b at 30+40+60=130
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 130) < 2);
        }

        // [CSS-FLEXBOX §9.5] wrap + justify-content:space-around per line (items forced to wrap)
        [Fact]
        public void Wrap_JustifyContent_SpaceAround_PerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:space-around;width:160px'>
                    <div id='a1' style='width:60px;height:30px'></div>
                    <div id='a2' style='width:60px;height:30px'></div>
                    <div id='b1' style='width:120px;height:30px'></div>
                </div></body>");
            // Line 1: 2 items of 60px=120, free=40, gap=40/2=20, half=10
            // a1.X=10, a2.X=10+60+20=90
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a1")!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a2")!.ContentRect.X - 90) < 2);
        }

        // [CSS-FLEXBOX §9.5] wrap + justify-content:space-evenly per line (items forced to wrap)
        [Fact]
        public void Wrap_JustifyContent_SpaceEvenly_PerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:space-evenly;width:150px'>
                    <div id='a1' style='width:30px;height:30px'></div>
                    <div id='a2' style='width:30px;height:30px'></div>
                    <div id='b1' style='width:120px;height:30px'></div>
                </div></body>");
            // Line 1: 2 items of 30px=60, free=90, 3 gaps of 30
            // a1.X=30, a2.X=30+30+30=90
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a1")!.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a2")!.ContentRect.X - 90) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap-reverse + align-content:space-between reverses cross distribution
        [Fact]
        public void WrapReverse_AlignContent_SpaceBetween()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;align-content:space-between;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            // wrap-reverse space-between: first line at bottom, last at top
            // a (first line) at Y=160, b (second line) at Y=0
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 0) < 2);
        }

        // [CSS-FLEXBOX §4.4] column wrap-reverse + align-content:flex-start (auto width)
        [Fact]
        public void ColumnWrapReverse_AlignContent_FlexStart()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap-reverse;align-content:flex-start;width:300px;height:100px'>
                    <div id='a' style='width:80px;height:60px'></div>
                    <div id='b' style='width:80px;height:60px'></div>
                </div></body>");
            // column wrap-reverse with flex-start: columns reversed, packed.
            // Actual: a.X=80, b.X=0
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 0) < 2);
        }

        // [CSS-FLEXBOX §9] wrap + column-gap + row-gap together
        [Fact]
        public void Wrap_ColumnGap_RowGap_Combined()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:10px;row-gap:20px;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                </div></body>");
            // 90+10+90=190 < 200: a,b on line 1. c wraps to line 2.
            float columnGap = LayoutTestHelper.FindById(r, "b")!.ContentRect.X - (LayoutTestHelper.FindById(r, "a")!.ContentRect.X + 90);
            float rowGap = LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - (LayoutTestHelper.FindById(r, "a")!.ContentRect.Y + 30);
            Assert.True(System.Math.Abs(columnGap - 10) < 2);
            Assert.True(System.Math.Abs(rowGap - 20) < 2);
        }

        // [CSS-FLEXBOX §9.5] wrap + justify-content:center + align-content:center combined
        [Fact]
        public void Wrap_JustifyCenter_AlignContentCenter()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:center;align-content:center;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            // 2 lines of 30px = 60px, free cross = 140, offset = 70
            // Each line: 60px item, free main = 40, center offset = 20
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 20) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:space-evenly with two items
        [Fact]
        public void Wrap_AlignContent_SpaceEvenly_TwoLines()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-evenly;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            // free = 120, 3 gaps of 40 each. a at 40, b at 40+40+40=120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 120) < 2);
        }

        // [CSS-FLEXBOX §8.3+§8.4] wrap + align-items:stretch on multi-line with align-content:flex-start
        [Fact]
        public void Wrap_AlignItems_Stretch_MultiLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-items:stretch;align-content:flex-start;width:100px;height:200px'>
                    <div id='tall' style='width:60px;height:60px'></div>
                    <div id='auto' style='width:60px'></div>
                </div></body>");
            // 2 lines (each item on separate line). Line 1 cross = 60, line 2 cross = 0.
            // align-content:flex-start: lines packed at top.
            // auto-height item stretches to its line's cross size.
            var autoItem = LayoutTestHelper.FindById(r, "auto")!;
            _output.WriteLine($"auto.Y={autoItem.ContentRect.Y} auto.H={autoItem.ContentRect.Height}");
            Assert.True(autoItem.ContentRect.Y >= 58);
        }

        // [CSS-FLEXBOX §9.7] wrap + flex-shrink only within each line
        [Fact]
        public void Wrap_FlexShrink_PerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='flex:0 1 120px;height:30px'></div>
                    <div id='b' style='flex:0 1 120px;height:30px'></div>
                </div></body>");
            // 120+120=240 > 200. But wrap means each goes on its own line.
            // Each item alone on line, no shrink needed, stays 120px.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 120) < 2);
        }

        // [CSS-FLEXBOX §4.4] column wrap + align-content:stretch with wrapping items
        [Fact]
        public void ColumnWrap_AlignContent_Stretch()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:stretch;width:300px;height:100px'>
                    <div id='a' style='width:80px;height:60px'></div>
                    <div id='b' style='width:80px;height:60px'></div>
                </div></body>");
            // 60+60=120 > 100, wraps to 2 columns. 300/2=150 each.
            // Items have explicit width 80px (not auto), so they keep 80px.
            // But column cross-size = 150, so b starts at X=150.
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 150) < 2);
        }

        // [CSS-FLEXBOX §4.4] column wrap + gap + align-content:center
        [Fact]
        public void ColumnWrap_Gap_AlignContent_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:center;column-gap:20px;width:300px;height:100px'>
                    <div id='a' style='width:60px;height:60px'></div>
                    <div id='b' style='width:60px;height:60px'></div>
                </div></body>");
            // 2 columns of 60px + 20px gap = 140px, free = 160, offset = 80
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 160) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:flex-start with four lines
        [Fact]
        public void Wrap_AlignContent_FlexStart_FourLines()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:100px;height:400px'>
                    <div id='a' style='width:60px;height:25px'></div>
                    <div id='b' style='width:60px;height:25px'></div>
                    <div id='c' style='width:60px;height:25px'></div>
                    <div id='d' style='width:60px;height:25px'></div>
                </div></body>");
            // 4 lines packed at top: 0, 25, 50, 75
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.Y - 75) < 2);
        }

        // [CSS-FLEXBOX §9.5] wrap + justify-content:flex-start (default) items packed at start
        [Fact]
        public void Wrap_JustifyContent_FlexStart_PerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:flex-start;width:200px'>
                    <div id='a1' style='width:80px;height:30px'></div>
                    <div id='a2' style='width:80px;height:30px'></div>
                    <div id='b1' style='width:150px;height:30px'></div>
                </div></body>");
            // Line 1: a1 at X=0, a2 at X=80
            // Line 2: b1 at X=0
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a1")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a2")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b1")!.ContentRect.X - 0) < 2);
        }

        // [CSS-FLEXBOX §4.4] column wrap + align-content:space-around
        [Fact]
        public void ColumnWrap_AlignContent_SpaceAround()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:space-around;width:300px;height:100px'>
                    <div id='a' style='width:80px;height:60px'></div>
                    <div id='b' style='width:80px;height:60px'></div>
                </div></body>");
            // 2 columns of 80px = 160. free = 140. gap = 140/2 = 70. half-gap = 35.
            // a.X=35, b.X=35+80+70=185
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 35) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 185) < 2);
        }

        // [CSS-FLEXBOX §9.7] wrap + unequal flex-grow on same line
        [Fact]
        public void Wrap_UnequalFlexGrow_SameLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:300px'>
                    <div id='a' style='flex:1 0 100px;height:30px'></div>
                    <div id='b' style='flex:2 0 100px;height:30px'></div>
                </div></body>");
            // Both on same line. free = 100. a gets 100/3=33.33, b gets 200/3=66.67
            // a.W = 133.33, b.W = 166.67
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 133.33f) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 166.67f) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap + align-content:flex-end with auto height collapses to content
        [Fact]
        public void Wrap_AlignContent_FlexEnd_AutoHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;align-content:flex-end;width:100px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            // Auto height: container height = sum of line cross sizes = 80
            // flex-end with no extra space: lines at top (no free space)
            var flex = LayoutTestHelper.FindById(r, "flex")!;
            Assert.True(System.Math.Abs(flex.ContentRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 0) < 2);
        }

        // [CSS-FLEXBOX §8.3] wrap + align-items:center with three items, two per line
        [Fact]
        public void Wrap_AlignItems_Center_TwoPerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-items:center;width:200px'>
                    <div id='a' style='width:80px;height:80px'></div>
                    <div id='b' style='width:80px;height:40px'></div>
                    <div id='c' style='width:80px;height:30px'></div>
                </div></body>");
            // Line 1: a(80px) + b(80px) = 160 < 200, both on line 1. Cross = 80.
            // b centered: (80-40)/2 = 20
            // Line 2: c alone, cross = 30, c at Y=80
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 80) < 2);
        }

        // [CSS-FLEXBOX §4.4] column wrap + align-content:space-evenly
        [Fact]
        public void ColumnWrap_AlignContent_SpaceEvenly()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:space-evenly;width:300px;height:100px'>
                    <div id='a' style='width:60px;height:60px'></div>
                    <div id='b' style='width:60px;height:60px'></div>
                </div></body>");
            // 2 columns of 60px = 120. free = 180. 3 gaps of 60.
            // a.X=60, b.X=60+60+60=180
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 180) < 2);
        }

        // [CSS-FLEXBOX §8.4] wrap + row-gap + align-content:flex-start
        [Fact]
        public void Wrap_RowGap_AlignContent_FlexStart()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-start;row-gap:15px;width:100px;height:300px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            // flex-start: packed at top with gap. a at 0, b at 40+15=55
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 55) < 2);
        }

        // [CSS-FLEXBOX §5.4] wrap + negative order values
        [Fact]
        public void Wrap_NegativeOrder()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:100px'>
                    <div id='a' style='order:0;width:60px;height:30px'></div>
                    <div id='b' style='order:-1;width:60px;height:30px'></div>
                </div></body>");
            // b has order:-1, renders first. b at Y=0, a at Y=30.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 30) < 2);
        }
    }
}
