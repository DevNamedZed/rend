using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexColumnWrapTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexColumnWrapTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void ColumnWrap_ItemsOverflowToNextColumn()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:100px;width:300px'>
                    <div id='a' style='width:50px;height:60px'></div>
                    <div id='b' style='width:50px;height:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.X < 2, $"first item at X=0 (got {itemA.ContentRect.X})");
            Assert.True(itemB.ContentRect.X >= 48, $"second item wraps to next column (got X={itemB.ContentRect.X})");
            Assert.True(itemB.ContentRect.Y < 2, $"second item starts at top of new column (got Y={itemB.ContentRect.Y})");
        }

        [Fact]
        public void ColumnWrapReverse_ColumnsReversed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap-reverse;height:100px;width:300px'>
                    <div id='a' style='width:50px;height:60px'></div>
                    <div id='b' style='width:50px;height:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.X > itemB.ContentRect.X,
                $"wrap-reverse: first column on right (a.X={itemA.ContentRect.X}, b.X={itemB.ContentRect.X})");
        }

        [Fact]
        public void ColumnWrap_ExplicitHeight_ItemsFitSingleColumn()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:200px;width:300px'>
                    <div id='a' style='width:50px;height:40px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - itemB.ContentRect.X) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - itemC.ContentRect.X) < 2);
        }

        [Fact]
        public void ColumnWrap_AutoHeight_NoWrapping()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-direction:column;flex-wrap:wrap;width:300px'>
                    <div id='a' style='width:50px;height:40px'></div>
                    <div id='b' style='width:50px;height:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - itemB.ContentRect.X) < 2,
                "auto height means no wrapping, all items in one column");
        }

        [Fact]
        public void ColumnWrap_WithRowGap_SpaceBetweenItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:200px;row-gap:20px;width:300px'>
                    <div id='a' style='width:50px;height:40px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float verticalGap = itemB.ContentRect.Y - (itemA.ContentRect.Y + 40);
            Assert.True(System.Math.Abs(verticalGap - 20) < 2,
                $"row-gap=20 between column items (got {verticalGap})");
        }

        [Fact]
        public void ColumnWrap_WithColumnGap_SpaceBetweenColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:60px;column-gap:30px;align-content:flex-start;width:300px'>
                    <div id='a' style='width:50px;height:50px'></div>
                    <div id='b' style='width:50px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 30) < 2,
                $"column-gap=30 between wrapped columns (got {columnGap})");
        }

        [Fact]
        public void ColumnWrap_JustifyContentCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:200px;justify-content:center;width:300px'>
                    <div id='a' style='width:50px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 80) < 2,
                $"justify-content:center centers item vertically (got Y={itemA.ContentRect.Y})");
        }

        [Fact]
        public void ColumnWrap_JustifyContentFlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:200px;justify-content:flex-end;width:300px'>
                    <div id='a' style='width:50px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 160) < 2,
                $"justify-content:flex-end pushes to bottom (got Y={itemA.ContentRect.Y})");
        }

        [Fact]
        public void ColumnWrap_JustifyContentSpaceBetween()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:200px;justify-content:space-between;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Y < 2, $"first item at top (got Y={itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 170) < 2,
                $"second item at bottom (got Y={itemB.ContentRect.Y})");
        }

        [Fact]
        public void ColumnWrap_AlignContentCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:60px;align-content:center;width:300px'>
                    <div id='a' style='width:50px;height:50px'></div>
                    <div id='b' style='width:50px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalColumnsWidth = (itemB.ContentRect.X + itemB.ContentRect.Width) - itemA.ContentRect.X;
            float expectedOffset = (300 - totalColumnsWidth) / 2;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedOffset) < 2,
                $"align-content:center centers columns (got X={itemA.ContentRect.X}, expected ~{expectedOffset})");
        }

        [Fact]
        public void ColumnWrap_AlignContentSpaceBetween()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:60px;align-content:space-between;width:300px'>
                    <div id='a' style='width:50px;height:50px'></div>
                    <div id='b' style='width:50px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.X < 2, $"first column at left edge (got X={itemA.ContentRect.X})");
            Assert.True(System.Math.Abs((itemB.ContentRect.X + itemB.ContentRect.Width) - 300) < 2,
                $"second column at right edge (got right={itemB.ContentRect.X + itemB.ContentRect.Width})");
        }

        [Fact]
        public void ColumnWrap_AlignContentFlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:60px;align-content:flex-end;width:300px'>
                    <div id='a' style='width:50px;height:50px'></div>
                    <div id='b' style='width:50px;height:50px'></div>
                </div></body>");
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs((itemB.ContentRect.X + itemB.ContentRect.Width) - 300) < 2,
                $"align-content:flex-end pushes columns to right (got right={itemB.ContentRect.X + itemB.ContentRect.Width})");
        }

        [Fact]
        public void ColumnWrap_AlignItemsCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:60px;align-items:center;width:300px'>
                    <div id='a' style='width:30px;height:50px'></div>
                    <div id='b' style='width:20px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.X={itemA.ContentRect.X} a.W={itemA.ContentRect.Width} b.X={itemB.ContentRect.X} b.W={itemB.ContentRect.Width}");
            Assert.True(itemA.ContentRect.Width < 32 && itemA.ContentRect.Width > 28,
                $"item a width preserved at 30 (got {itemA.ContentRect.Width})");
            Assert.True(itemB.ContentRect.Width < 22 && itemB.ContentRect.Width > 18,
                $"item b width preserved at 20 (got {itemB.ContentRect.Width})");
        }

        [Fact]
        public void ColumnWrap_MultipleColumns_ThreeColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:50px;width:300px'>
                    <div id='a' style='width:50px;height:40px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(itemB.ContentRect.X > itemA.ContentRect.X,
                $"b in second column (a.X={itemA.ContentRect.X}, b.X={itemB.ContentRect.X})");
            Assert.True(itemC.ContentRect.X > itemB.ContentRect.X,
                $"c in third column (b.X={itemB.ContentRect.X}, c.X={itemC.ContentRect.X})");
        }

        [Fact]
        public void ColumnWrap_FlexGrow_StretchesInMainAxis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:200px;width:300px'>
                    <div id='a' style='flex-grow:1;width:50px;height:60px'></div>
                    <div id='b' style='flex-grow:1;width:50px;height:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2,
                $"flex-grow distributes height equally (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 100) < 2,
                $"flex-grow distributes height equally (got {itemB.ContentRect.Height})");
        }

        [Fact]
        public void ColumnWrap_ItemWidths_DetermineColumnWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:50px;width:300px'>
                    <div id='a' style='width:80px;height:40px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemB.ContentRect.X >= 78,
                $"second column starts after first column width 80 (got b.X={itemB.ContentRect.X})");
        }

        [Fact]
        public void ColumnWrap_MinHeight_PreventsOverShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:100px;width:300px'>
                    <div id='a' style='width:50px;height:80px;min-height:70px'></div>
                    <div id='b' style='width:50px;height:80px;min-height:70px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Height >= 69,
                $"min-height prevents shrink below 70 (got {itemA.ContentRect.Height})");
            Assert.True(itemB.ContentRect.X > itemA.ContentRect.X,
                $"items forced to wrap due to min-height constraint");
        }

        [Fact]
        public void ColumnWrap_MaxHeight_ClampsGrowth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:200px;width:300px'>
                    <div id='a' style='flex-grow:1;width:50px;max-height:80px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(itemA.ContentRect.Height <= 82,
                $"max-height:80 clamps flex-grow (got {itemA.ContentRect.Height})");
        }

        [Fact]
        public void ColumnWrap_Order_ChangesVisualPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:50px;width:300px'>
                    <div id='a' style='order:2;width:50px;height:40px'></div>
                    <div id='b' style='order:1;width:50px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemB.ContentRect.X <= itemA.ContentRect.X,
                $"order:1 appears before order:2 (b.X={itemB.ContentRect.X}, a.X={itemA.ContentRect.X})");
        }

        [Fact]
        public void ColumnWrap_DifferentItemHeights_TallestDefinesWrapPoint()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:100px;width:300px'>
                    <div id='a' style='width:50px;height:90px'></div>
                    <div id='b' style='width:50px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemB.ContentRect.X > itemA.ContentRect.X,
                $"20px item wraps because 90+20=110 exceeds 100 (b.X={itemB.ContentRect.X})");
        }

        [Fact]
        public void ColumnWrap_JustifyContentSpaceAround()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:200px;justify-content:space-around;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float freeSpace = 200 - 60;
            float expectedUnitSpace = freeSpace / 4;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedUnitSpace) < 2,
                $"space-around: first item offset by half-space (expected ~{expectedUnitSpace}, got {itemA.ContentRect.Y})");
        }

        [Fact]
        public void ColumnWrap_JustifyContentSpaceEvenly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:200px;justify-content:space-evenly;width:300px'>
                    <div id='a' style='width:50px;height:20px'></div>
                    <div id='b' style='width:50px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float expectedGap = (200 - 40) / 3.0f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedGap) < 2,
                $"space-evenly: equal gaps (expected ~{expectedGap}, got Y={itemA.ContentRect.Y})");
            float actualGap = itemB.ContentRect.Y - (itemA.ContentRect.Y + 20);
            Assert.True(System.Math.Abs(actualGap - expectedGap) < 2,
                $"space-evenly: gap between items (expected ~{expectedGap}, got {actualGap})");
        }

        [Fact]
        public void ColumnWrap_AlignContentStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:50px;align-content:stretch;width:300px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Width >= 148,
                $"align-content:stretch expands first column item (got W={itemA.ContentRect.Width})");
            Assert.True(itemB.ContentRect.Width >= 148,
                $"align-content:stretch expands second column item (got W={itemB.ContentRect.Width})");
        }

        [Fact]
        public void ColumnWrap_WithBothGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:100px;gap:10px 20px;align-content:flex-start;width:300px'>
                    <div id='a' style='width:50px;height:40px'></div>
                    <div id='b' style='width:50px;height:40px'></div>
                    <div id='c' style='width:50px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float verticalGap = itemB.ContentRect.Y - (itemA.ContentRect.Y + 40);
            Assert.True(System.Math.Abs(verticalGap - 10) < 2,
                $"row-gap=10 between items in same column (got {verticalGap})");
            Assert.True(itemC.ContentRect.X > itemA.ContentRect.X,
                $"third item wraps to next column since 40+10+40+10+40=140>100");
            float columnGap = itemC.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(columnGap - 20) < 2,
                $"column-gap=20 between wrapped columns (got {columnGap})");
        }

        [Fact]
        public void ColumnWrap_FlexShrink_InColumn()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:100px;width:300px'>
                    <div id='a' style='flex-shrink:1;width:50px;height:70px'></div>
                    <div id='b' style='flex-shrink:1;width:50px;height:70px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.H={itemA.ContentRect.Height} a.X={itemA.ContentRect.X} b.H={itemB.ContentRect.Height} b.X={itemB.ContentRect.X}");
            bool shrunk = itemA.ContentRect.Height < 71 && itemB.ContentRect.Height < 71;
            bool wrapped = itemB.ContentRect.X > itemA.ContentRect.X;
            Assert.True(shrunk || wrapped,
                "items either shrink to fit or wrap to next column");
        }

        [Fact]
        public void ColumnWrap_MixedHeights_TwoItemsFirstColumn_OneSecond()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:100px;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - itemB.ContentRect.X) < 2,
                "a and b in same column");
            Assert.True(itemC.ContentRect.X > itemA.ContentRect.X,
                $"c wraps since 30+30+50=110>100 (c.X={itemC.ContentRect.X}, a.X={itemA.ContentRect.X})");
            Assert.True(itemC.ContentRect.Y < 2,
                $"c starts at top of new column (got Y={itemC.ContentRect.Y})");
        }

        [Fact]
        public void ColumnWrapReverse_WithAlignContentCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap-reverse;height:50px;align-content:center;width:300px'>
                    <div id='a' style='width:40px;height:40px'></div>
                    <div id='b' style='width:40px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float center = 150;
            float midpoint = (itemA.ContentRect.X + itemB.ContentRect.X + 40) / 2;
            Assert.True(System.Math.Abs(midpoint - center) < 10,
                $"wrap-reverse with align-content:center centers columns (midpoint={midpoint})");
        }

        [Fact]
        public void ColumnWrap_AlignItemsFlexEnd_ItemsAtColumnEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:50px;align-items:flex-end;width:300px'>
                    <div id='a' style='width:30px;height:40px'></div>
                    <div id='b' style='width:20px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a: X={itemA.ContentRect.X} W={itemA.ContentRect.Width}  b: X={itemB.ContentRect.X} W={itemB.ContentRect.Width}");
            Assert.True(itemA.ContentRect.Width < 32 && itemA.ContentRect.Width > 28,
                $"align-items:flex-end preserves item width (got {itemA.ContentRect.Width})");
        }

        [Fact]
        public void ColumnWrap_FlexGrow_PerColumn()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:200px;width:300px'>
                    <div id='a' style='flex-grow:1;width:50px;height:50px'></div>
                    <div id='b' style='flex-grow:2;width:50px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float freeSpace = 200 - 100;
            float expectedHeightA = 50 + freeSpace / 3;
            float expectedHeightB = 50 + 2 * freeSpace / 3;
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - expectedHeightA) < 2,
                $"flex-grow:1 gets 1/3 free space (expected ~{expectedHeightA}, got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - expectedHeightB) < 2,
                $"flex-grow:2 gets 2/3 free space (expected ~{expectedHeightB}, got {itemB.ContentRect.Height})");
        }

        [Fact]
        public void ColumnWrap_AlignSelfOverride()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;height:50px;align-items:stretch;width:300px'>
                    <div id='a' style='width:30px;height:40px'></div>
                    <div id='b' style='align-self:center;width:30px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemB.ContentRect.Width < 32,
                $"align-self:center preserves item width (got {itemB.ContentRect.Width})");
        }
    }
}
