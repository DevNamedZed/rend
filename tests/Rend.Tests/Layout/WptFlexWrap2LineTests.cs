using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexWrap2LineTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexWrap2LineTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.3] 2 items each on its own line: Y positions
        [Fact]
        public void TwoItemsOnePerLine_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:100px'>
                    <div id='a' style='width:60px;height:30px;background:red'></div>
                    <div id='b' style='width:60px;height:40px;background:blue'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X) < 2, $"b X=0 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2, $"b Y=30 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] 4 items, two per line: X/Y positions
        [Fact]
        public void FourItemsTwoPerLine_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                    <div id='d' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 90) < 2, $"b X=90 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a and b on line 1");
            Assert.True(System.Math.Abs(itemC.ContentRect.X) < 2, $"c X=0 (got {itemC.ContentRect.X})");
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 90) < 2, $"d X=90 (got {itemD.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 30) < 2, $"c Y=30 (got {itemC.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - itemD.ContentRect.Y) < 2, "c and d on line 2");
        }

        // [CSS-FLEXBOX §9.3] 6 items, three per line: X/Y positions
        [Fact]
        public void SixItemsThreePerLine_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:300px'>
                    <div id='a' style='width:90px;height:25px'></div>
                    <div id='b' style='width:90px;height:25px'></div>
                    <div id='c' style='width:90px;height:25px'></div>
                    <div id='d' style='width:90px;height:25px'></div>
                    <div id='e' style='width:90px;height:25px'></div>
                    <div id='f' style='width:90px;height:25px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            var itemE = LayoutTestHelper.FindById(root, "e")!;
            var itemF = LayoutTestHelper.FindById(root, "f")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a,b on line 1");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - itemC.ContentRect.Y) < 2, "b,c on line 1");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - itemE.ContentRect.Y) < 2, "d,e on line 2");
            Assert.True(System.Math.Abs(itemE.ContentRect.Y - itemF.ContentRect.Y) < 2, "e,f on line 2");
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 90) < 2, $"b X=90 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 180) < 2, $"c X=180 (got {itemC.ContentRect.X})");
            Assert.True(System.Math.Abs(itemD.ContentRect.X) < 2, $"d X=0 (got {itemD.ContentRect.X})");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 25) < 2, $"d Y=25 (got {itemD.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9] 2 lines with row-gap
        [Fact]
        public void TwoLines_RowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:20px;width:100px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float gap = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(gap - 20) < 2, $"row-gap expected 20, got {gap}");
        }

        // [CSS-FLEXBOX §9] 2 lines with column-gap within each line
        [Fact]
        public void TwoLines_ColumnGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:10px;width:200px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                    <div id='c' style='width:80px;height:30px'></div>
                    <div id='d' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            float gapLine1 = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            float gapLine2 = itemD.ContentRect.X - (itemC.ContentRect.X + itemC.ContentRect.Width);
            Assert.True(System.Math.Abs(gapLine1 - 10) < 2, $"line1 column-gap expected 10, got {gapLine1}");
            Assert.True(System.Math.Abs(gapLine2 - 10) < 2, $"line2 column-gap expected 10, got {gapLine2}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a,b on line 1");
            Assert.True(itemC.ContentRect.Y > itemA.ContentRect.Y + 28, "c,d on line 2");
        }

        // [CSS-FLEXBOX §9] 2 lines with both row-gap and column-gap
        [Fact]
        public void TwoLines_BothGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:15px;column-gap:10px;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                    <div id='d' style='width:90px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 10) < 2, $"column-gap expected 10, got {columnGap}");
            float rowGap = itemC.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 15) < 2, $"row-gap expected 15, got {rowGap}");
        }

        // [CSS-FLEXBOX §8.4] 2 lines align-content:flex-start packs at top
        [Fact]
        public void TwoLines_AlignContent_FlexStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2, $"b Y=30 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] 2 lines align-content:flex-end packs at bottom
        [Fact]
        public void TwoLines_AlignContent_FlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Lines: 30+40=70. Free=130. flex-end offset=130.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 130) < 2, $"a Y=130 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 160) < 2, $"b Y=160 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] 2 lines align-content:center
        [Fact]
        public void TwoLines_AlignContent_Center()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:center;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Lines: 30+40=70. Free=130. Center offset=65.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 65) < 2, $"a Y=65 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 95) < 2, $"b Y=95 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.4] 2 lines align-content:space-between
        [Fact]
        public void TwoLines_AlignContent_SpaceBetween()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y + itemB.ContentRect.Height - 200) < 2,
                $"b bottom=200 (got {itemB.ContentRect.Y + itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §8.4] 2 lines align-content:stretch distributes cross space
        [Fact]
        public void TwoLines_AlignContent_Stretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:200px'>
                    <div id='a' style='width:60px'></div>
                    <div id='b' style='width:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // 2 lines stretch: each gets 100px
            Assert.True(itemA.ContentRect.Height >= 98,
                $"a stretched to ~100 (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 2,
                $"b Y=100 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] 2 lines with auto height: container height = sum of line cross sizes
        [Fact]
        public void TwoLines_AutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;width:100px'>
                    <div style='width:60px;height:30px'></div>
                    <div style='width:60px;height:50px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            _output.WriteLine($"flex.h={flexContainer.ContentRect.Height}");
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - 80) < 2,
                $"auto height expected 80 (30+50), got {flexContainer.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9.3] 2 lines wrap-reverse: first line at cross-end
        [Fact]
        public void TwoLines_WrapReverse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;width:100px;height:120px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"wrap-reverse: a.Y={itemA.ContentRect.Y} should be > b.Y={itemB.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.2] 2 lines justify-content:center per line
        [Fact]
        public void TwoLines_JustifyContent_Center()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:center;width:200px'>
                    <div id='a' style='width:110px;height:30px'></div>
                    <div id='b' style='width:110px;height:30px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Line 1: a(110), free=90, center offset=45
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 45) < 2,
                $"a X=45 (got {itemA.ContentRect.X})");
            // Line 2: b(110)+c(60)=170, free=30, center offset=15
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 15) < 2,
                $"b X=15 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 125) < 2,
                $"c X=125 (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.2] 2 lines justify-content:flex-end per line
        [Fact]
        public void TwoLines_JustifyContent_FlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:flex-end;width:200px'>
                    <div id='a' style='width:110px;height:30px'></div>
                    <div id='b' style='width:110px;height:30px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Line 1: a(110), flex-end offset=90
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 90) < 2,
                $"a X=90 (got {itemA.ContentRect.X})");
            // Line 2: b(110)+c(60)=170, flex-end offset=30
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 30) < 2,
                $"b X=30 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 140) < 2,
                $"c X=140 (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.2] 2 lines justify-content:space-between per line
        [Fact]
        public void TwoLines_JustifyContent_SpaceBetween()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:space-between;width:200px'>
                    <div id='a' style='width:110px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:110px;height:30px'></div>
                    <div id='d' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            // Line 1: a(110)+b(50)=160<200. a at 0, b at right edge (200-50=150)
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X + itemB.ContentRect.Width - 200) < 2,
                $"b right=200 (got {itemB.ContentRect.X + itemB.ContentRect.Width})");
            // Line 2: c(110)+d(50)=160<200. c at 0, d at right edge
            Assert.True(System.Math.Abs(itemC.ContentRect.X) < 2, $"c X=0 (got {itemC.ContentRect.X})");
            Assert.True(System.Math.Abs(itemD.ContentRect.X + itemD.ContentRect.Width - 200) < 2,
                $"d right=200 (got {itemD.ContentRect.X + itemD.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 2 lines flex-grow distributes per-line independently
        [Fact]
        public void TwoLines_FlexGrow_Independent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='flex:1 0 150px;height:30px'></div>
                    <div id='b' style='flex:1 0 80px;height:30px'></div>
                    <div id='c' style='flex:2 0 80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Line 1: a alone (150+80=230>200), a grows to 200
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"a grows to 200 (got {itemA.ContentRect.Width})");
            // Line 2: b(80)+c(80)=160<200. Free=40. b gets 40/3≈13.3, c gets 80/3≈26.7
            float expectedBWidth = 80 + 40f / 3f;
            float expectedCWidth = 80 + 80f / 3f;
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - expectedBWidth) < 2,
                $"b width ~{expectedBWidth:F1} (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - expectedCWidth) < 2,
                $"c width ~{expectedCWidth:F1} (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.3] 2 lines with different heights: line height = tallest item
        [Fact]
        public void TwoLines_DifferentHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:90px;height:20px'></div>
                    <div id='b' style='width:90px;height:50px'></div>
                    <div id='c' style='width:90px;height:35px'></div>
                    <div id='d' style='width:90px;height:25px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            // Line 1: a(20),b(50) → line height 50
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a,b on same line");
            // Line 2 starts at Y=50
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 50) < 2,
                $"c Y=50 (got {itemC.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - itemD.ContentRect.Y) < 2, "c,d on same line");
        }

        // [CSS-FLEXBOX §9.3] 2 lines with padding on container
        [Fact]
        public void TwoLines_ContainerPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;width:200px;padding:20px'>
                    <div id='a' style='width:110px;height:30px'></div>
                    <div id='b' style='width:110px;height:30px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Content area starts at (20,20), width=200px
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 20) < 2,
                $"a X=20 (padding, got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 20) < 2,
                $"a Y=20 (padding, got {itemA.ContentRect.Y})");
            Assert.True(itemB.ContentRect.Y > itemA.ContentRect.Y + 28,
                "b wraps to line 2 (110+110=220>200)");
        }

        // [CSS-FLEXBOX §9.3] 2 lines with border on container
        [Fact]
        public void TwoLines_ContainerBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;width:200px;border:5px solid black'>
                    <div id='a' style='width:110px;height:30px'></div>
                    <div id='b' style='width:110px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Content area offset by border (5px each side)
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 5) < 2,
                $"a X=5 (border, got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 5) < 2,
                $"a Y=5 (border, got {itemA.ContentRect.Y})");
            Assert.True(itemB.ContentRect.Y > itemA.ContentRect.Y + 28,
                "b wraps to line 2 (110+110=220>200)");
        }

        // [CSS-FLEXBOX §9.3] column wrap producing 2 columns
        [Fact]
        public void ColumnWrap_TwoColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;width:300px;height:60px'>
                    <div id='a' style='width:80px;height:50px'></div>
                    <div id='b' style='width:80px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // a fills first column (50 height fits in 60), b wraps to second column
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 80) < 2, $"b X=80 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y) < 2, $"b Y=0 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] column wrap 2 columns with column-gap
        [Fact]
        public void ColumnWrap_TwoColumns_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;column-gap:20px;width:300px;height:60px'>
                    <div id='a' style='width:80px;height:50px'></div>
                    <div id='b' style='width:80px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2,
                $"b X=100 (80+20 gap, got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.4] 2 lines align-content:space-around
        [Fact]
        public void TwoLines_AlignContent_SpaceAround()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-around;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Lines: 30+40=70. Free=130. 2 lines → margin=130/4=32.5 per half-gap
            float expectedAY = 32.5f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedAY) < 3,
                $"a Y~{expectedAY} (got {itemA.ContentRect.Y})");
            Assert.True(itemA.ContentRect.Y > 2, "space-around pushes first line from top");
        }

        // [CSS-FLEXBOX §9.3] 2 lines wrap-reverse with auto height
        [Fact]
        public void TwoLines_WrapReverse_AutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap-reverse;width:100px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"flex.h={flexContainer.ContentRect.Height} a.y={itemA.ContentRect.Y} b.y={itemB.ContentRect.Y}");
            // Container auto height = 30+40=70
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - 70) < 2,
                $"auto height expected 70, got {flexContainer.ContentRect.Height}");
            // wrap-reverse: line 1 (a) below line 2 (b)
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"wrap-reverse: a.Y={itemA.ContentRect.Y} > b.Y={itemB.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §9.3] 2 lines wrap-reverse with row-gap
        [Fact]
        public void TwoLines_WrapReverse_RowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap-reverse;row-gap:10px;width:100px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"wrap-reverse: a.Y={itemA.ContentRect.Y} > b.Y={itemB.ContentRect.Y}");
            float gap = itemA.ContentRect.Y - (itemB.ContentRect.Y + itemB.ContentRect.Height);
            Assert.True(System.Math.Abs(gap - 10) < 2, $"row-gap expected 10, got {gap}");
        }

        // [CSS-FLEXBOX §9.3] 2 lines auto height includes row-gap
        [Fact]
        public void TwoLines_AutoHeight_IncludesRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;row-gap:20px;width:100px'>
                    <div style='width:60px;height:30px'></div>
                    <div style='width:60px;height:40px'></div>
                </div></body>");
            var flexContainer = LayoutTestHelper.FindById(root, "flex")!;
            float expectedHeight = 30 + 20 + 40;
            Assert.True(System.Math.Abs(flexContainer.ContentRect.Height - expectedHeight) < 2,
                $"auto height expected {expectedHeight}, got {flexContainer.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9.3] 2 lines with container padding+border combined
        [Fact]
        public void TwoLines_ContainerPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;width:180px;padding:10px;border:5px solid black'>
                    <div id='a' style='width:100px;height:30px'></div>
                    <div id='b' style='width:100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Content starts at X=15 (border 5 + padding 10), Y=15
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 15) < 2,
                $"a X=15 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 15) < 2,
                $"a Y=15 (got {itemA.ContentRect.Y})");
            // 100+100=200>180 → b wraps to line 2
            Assert.True(itemB.ContentRect.Y > itemA.ContentRect.Y + 28,
                "b wraps to line 2 (100+100>180)");
        }

        // [CSS-FLEXBOX §9.3] column wrap 2 columns with items of different widths
        [Fact]
        public void ColumnWrap_TwoColumns_DifferentWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;width:300px;height:60px'>
                    <div id='a' style='width:100px;height:50px'></div>
                    <div id='b' style='width:70px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // a is 100px wide, b wraps to column 2 at X=100
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2,
                $"b X=100 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] column wrap 2 columns with row-gap between items in same column
        [Fact]
        public void ColumnWrap_TwoColumns_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;align-content:flex-start;row-gap:10px;width:300px;height:100px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                    <div id='c' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Column 1: a(40)+gap(10)+b(40)=90 < 100, both fit
            Assert.True(System.Math.Abs(itemA.ContentRect.X) < 2, $"a X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X) < 2, $"b X=0 (got {itemB.ContentRect.X})");
            float rowGap = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 10) < 2, $"row-gap expected 10, got {rowGap}");
            // c wraps to column 2: 90+10+40=140>100
            Assert.True(itemC.ContentRect.X > itemA.ContentRect.X + 58,
                $"c wraps to column 2 (got X={itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.2] 2 lines justify-content applied independently to each line
        [Fact]
        public void TwoLines_JustifyContent_IndependentPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;justify-content:center;width:200px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Line 1: 80+80=160, free=40, offset=20
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 20) < 2,
                $"a X=20 (got {itemA.ContentRect.X})");
            // Line 2: 50, free=150, offset=75
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 75) < 2,
                $"c X=75 (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] 2 lines with different item counts per line
        [Fact]
        public void TwoLines_AsymmetricItemCount()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:250px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                    <div id='c' style='width:80px;height:30px'></div>
                    <div id='d' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            // 80+80+80=240<250, so a,b,c on line 1. d on line 2.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2, "a,b on line 1");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - itemC.ContentRect.Y) < 2, "b,c on line 1");
            Assert.True(itemD.ContentRect.Y > itemA.ContentRect.Y + 28, "d on line 2");
            Assert.True(System.Math.Abs(itemD.ContentRect.X) < 2,
                $"d X=0 (got {itemD.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.3] 2 lines flex-grow only on some items
        [Fact]
        public void TwoLines_FlexGrow_PartialItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:120px;height:30px'></div>
                    <div id='b' style='flex:1 0 50px;height:30px'></div>
                    <div id='c' style='flex:1 0 50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // a(120)+b(50)=170<200. Both on line 1. b grows by 30→80
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 120) < 2,
                $"a stays 120 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 80) < 2,
                $"b grows to 80 (got {itemB.ContentRect.Width})");
            // c alone on line 2, grows to 200
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 200) < 2,
                $"c grows to 200 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §8.4] 2 lines align-content:stretch with items that have explicit height
        [Fact]
        public void TwoLines_AlignContent_Stretch_ExplicitItemHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Items have explicit height so they keep 30px, but line cross size stretches to 100px
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 30) < 2,
                $"a keeps explicit height 30 (got {itemA.ContentRect.Height})");
            // b starts at Y=100 (stretched line 1 cross size)
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 2,
                $"b Y=100 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.3] 2 lines with wrap-reverse and align-content:center
        [Fact]
        public void TwoLines_WrapReverse_AlignContent_Center()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;align-content:center;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Total cross = 60. Free = 140. Center offset = 70.
            // wrap-reverse: line 1 (a) at bottom of block, line 2 (b) above
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"wrap-reverse: a.Y={itemA.ContentRect.Y} > b.Y={itemB.ContentRect.Y}");
            float midpoint = (itemA.ContentRect.Y + itemA.ContentRect.Height + itemB.ContentRect.Y) / 2f;
            Assert.True(System.Math.Abs(midpoint - 100) < 3,
                $"centered around 100 (midpoint={midpoint})");
        }

        // [CSS-FLEXBOX §9.3] 2 lines: line cross size determined by tallest item in that line
        [Fact]
        public void TwoLines_CrossSize_TallestItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:90px;height:20px'></div>
                    <div id='b' style='width:90px;height:60px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                </div></body>");
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Line 1: max(20,60)=60. Line 2 starts at Y=60.
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 60) < 2,
                $"c Y=60 (line 1 cross=60, got {itemC.ContentRect.Y})");
        }
    }
}
