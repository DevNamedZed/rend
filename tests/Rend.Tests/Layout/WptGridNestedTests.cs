using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for nested CSS Grid layouts: grid-in-grid, grid-in-flex,
    /// flex-in-grid, multi-level nesting, and interactions with block/table.
    /// </summary>
    public class WptGridNestedTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridNestedTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §12] Inner grid fills outer grid item width
        [Fact]
        public void GridInsideGrid_InnerFillsOuterItemWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr'>
                        <div id='t' style='height:20px'></div>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"Inner 1fr should be 150px (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §12] Inner grid with different column count
        [Fact]
        public void GridInsideGrid_DifferentColumnCounts()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr 1fr'>
                        <div id='t' style='height:20px'></div>
                    </div>
                    <div style='height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedInnerColWidth = 200f / 3f;
            Assert.True(System.Math.Abs(target.ContentRect.Width - expectedInnerColWidth) < 2,
                $"Inner 3-col grid item should be ~{expectedInnerColWidth:F1}px (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §12] Grid inside flex container
        [Fact]
        public void GridInsideFlex_InheritsFlexItemWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div style='flex:1;display:grid;grid-template-columns:1fr 1fr'>
                        <div id='t' style='height:20px'></div>
                        <div style='height:20px'></div>
                    </div>
                    <div style='flex:1;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"Grid inside flex:1 (200px), each 1fr = 100 (got {target.ContentRect.Width})");
        }

        // [CSS-FLEX §4] Flex container inside grid item
        [Fact]
        public void FlexInsideGrid_FlexFillsGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div style='display:flex'>
                        <div id='a' style='flex:1;height:30px'></div>
                        <div id='b' style='flex:2;height:30px'></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"flex:1 should be 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"flex:2 should be 200px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §12] Three-level nesting: grid > grid > grid
        [Fact]
        public void ThreeLevelNesting_GridGridGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:400px;width:400px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr'>
                        <div style='display:grid;grid-template-columns:1fr 1fr'>
                            <div id='t' style='height:20px'></div>
                            <div style='height:20px'></div>
                        </div>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"3-level: 400 / 2 / 2 = 100 (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §12] Three-level nesting: grid > flex > grid
        [Fact]
        public void ThreeLevelNesting_GridFlexGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div style='display:flex'>
                        <div style='flex:1;display:grid;grid-template-columns:1fr 1fr'>
                            <div id='t' style='height:20px'></div>
                            <div style='height:20px'></div>
                        </div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"grid>flex>grid: 300/2 = 150 (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §12] Grid inside a normal block
        [Fact]
        public void GridInsideBlock_InheritsBlockWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:240px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr 1fr'>
                        <div id='t' style='height:20px'></div>
                        <div style='height:20px'></div>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 80) < 2,
                $"Grid in 240px block: 240/3 = 80 (got {target.ContentRect.Width})");
        }

        // [CSS 2.1 §9.2] Block inside grid item flows normally
        [Fact]
        public void BlockInsideGridItem_FlowsNormally()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div>
                        <div id='t' style='width:100px;height:40px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"Block child in grid item: width=100 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 0) < 2,
                $"Block child starts at X=0 (got {target.ContentRect.X})");
        }

        // [CSS-GRID §12] Inner grid inherits available width from auto-sized outer item
        [Fact]
        public void InnerGrid_InheritsAvailableWidth_FromAutoOuterItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;width:360px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr 1fr'>
                        <div id='a' style='height:20px'></div>
                        <div id='b' style='height:20px'></div>
                        <div id='c' style='height:20px'></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 120) < 2,
                $"1fr of 360 inner = 120 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 120) < 2,
                $"b at X=120 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 240) < 2,
                $"c at X=240 (got {itemC.ContentRect.X})");
        }

        // [CSS-GRID §10.1] Inner grid with gap
        [Fact]
        public void InnerGrid_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:220px;width:220px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr;gap:20px'>
                        <div id='a' style='height:20px'></div>
                        <div id='b' style='height:20px'></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"(220-20)/2 = 100 (got {itemA.ContentRect.Width})");
            float gapBetween = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(gapBetween - 20) < 2,
                $"Gap between items = 20 (got {gapBetween})");
        }

        // [CSS-GRID §10.4] Nested grids with alignment: outer align-items center
        [Fact]
        public void NestedGrid_OuterAlignItems_Center()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:center;width:200px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr'>
                        <div id='t' style='height:20px'></div>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 38,
                $"align-items:center in 100px row, inner grid centered (Y={target.ContentRect.Y})");
        }

        // [CSS-FLEX §4] Grid item acting as flex container
        [Fact]
        public void GridItem_AsFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px 200px;width:400px'>
                    <div style='display:flex;justify-content:space-between'>
                        <div id='a' style='width:40px;height:20px'></div>
                        <div id='b' style='width:40px;height:20px'></div>
                    </div>
                    <div style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2,
                $"First flex item at X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 160) < 2,
                $"Last flex item at X=160 (200-40=160) (got {itemB.ContentRect.X})");
        }

        // [CSS-GRID §12] Flex item acting as grid container
        [Fact]
        public void FlexItem_AsGridContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div style='flex:1;display:grid;grid-template-columns:1fr 1fr'>
                        <div id='a' style='height:20px'></div>
                        <div id='b' style='height:20px'></div>
                    </div>
                    <div style='flex:1;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"Grid in flex:1(200px), 1fr = 100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2,
                $"Second grid item at X=100 (got {itemB.ContentRect.X})");
        }

        // [CSS-GRID §8.3] Nested grid with spanning item
        [Fact]
        public void NestedGrid_InnerItemSpansColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr 1fr'>
                        <div id='t' style='grid-column:span 2;height:20px'></div>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"span 2 in 3-col inner grid (300px): 200 (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §7.5] Grid in grid with auto rows
        [Fact]
        public void GridInGrid_AutoRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div style='display:grid;grid-template-columns:1fr;grid-auto-rows:50px'>
                        <div id='a' style=''></div>
                        <div id='b' style=''></div>
                        <div id='c' style=''></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 50) < 2,
                $"auto-rows: 50px (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 50) < 2,
                $"second row at Y=50 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 100) < 2,
                $"third row at Y=100 (got {itemC.ContentRect.Y})");
        }

        // [CSS-GRID §12] Nested grid positioning: inner grid offset by outer padding
        [Fact]
        public void NestedGrid_OuterPadding_OffsetsInner()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;width:300px;padding:20px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr'>
                        <div id='t' style='height:20px'></div>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2,
                $"Outer padding 20px offsets inner grid (X={target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"Inner item: 300/2 = 150 (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §12] Grid with nested table
        [Fact]
        public void GridWithNestedTable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div>
                        <table id='t' style='width:100%;border-collapse:collapse'>
                            <tr><td style='height:30px'>Cell</td></tr>
                        </table>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width >= 298,
                $"Table 100% in grid item = 300px (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §12] Two grids side by side in outer grid columns
        [Fact]
        public void TwoGridsSideBySide_InOuterColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr'>
                        <div id='leftA' style='height:20px'></div>
                        <div id='leftB' style='height:20px'></div>
                    </div>
                    <div style='display:grid;grid-template-columns:1fr 1fr'>
                        <div id='rightA' style='height:20px'></div>
                        <div id='rightB' style='height:20px'></div>
                    </div>
                </div></body>");
            var leftA = LayoutTestHelper.FindById(root, "leftA")!;
            var leftB = LayoutTestHelper.FindById(root, "leftB")!;
            var rightA = LayoutTestHelper.FindById(root, "rightA")!;
            var rightB = LayoutTestHelper.FindById(root, "rightB")!;
            Assert.True(System.Math.Abs(leftA.ContentRect.Width - 100) < 2,
                $"Left grid 1fr = 100 (got {leftA.ContentRect.Width})");
            Assert.True(System.Math.Abs(leftB.ContentRect.X - 100) < 2,
                $"Left grid item B at X=100 (got {leftB.ContentRect.X})");
            Assert.True(System.Math.Abs(rightA.ContentRect.X - 200) < 2,
                $"Right grid starts at X=200 (got {rightA.ContentRect.X})");
            Assert.True(System.Math.Abs(rightB.ContentRect.X - 300) < 2,
                $"Right grid item B at X=300 (got {rightB.ContentRect.X})");
        }

        // [CSS-GRID §10.4] Inner grid justify-items: end inside outer grid
        [Fact]
        public void InnerGrid_JustifyItemsEnd_InOuterGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div style='display:grid;grid-template-columns:200px;justify-items:end'>
                        <div id='t' style='width:60px;height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 140) < 2,
                $"justify-items:end in inner grid: X=140 (got {target.ContentRect.X})");
        }

        // [CSS-GRID §12] Grid in grid, outer with gap, inner without
        [Fact]
        public void OuterGridWithGap_InnerGridWithout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;gap:20px;width:220px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr'>
                        <div id='a' style='height:20px'></div>
                        <div id='b' style='height:20px'></div>
                    </div>
                    <div style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 2,
                $"Outer: (220-20)/2=100, inner: 100/2=50 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 50) < 2,
                $"Inner second item at X=50 (got {itemB.ContentRect.X})");
        }

        // [CSS-GRID §12] Grid with both gap on outer and inner
        [Fact]
        public void NestedGrids_BothWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;gap:10px;width:200px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr;gap:20px'>
                        <div id='a' style='height:20px'></div>
                        <div id='b' style='height:20px'></div>
                    </div>
                    <div id='second' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var secondRow = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 90) < 2,
                $"Inner: (200-20)/2=90 (got {itemA.ContentRect.Width})");
            float rowGap = secondRow.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 10) < 2,
                $"Outer row gap = 10 (got {rowGap})");
        }

        // [CSS-GRID §12] Grid in grid, inner with explicit pixel columns
        [Fact]
        public void InnerGrid_ExplicitPixelColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div style='display:grid;grid-template-columns:80px 120px 100px'>
                        <div id='a' style='height:20px'></div>
                        <div id='b' style='height:20px'></div>
                        <div id='c' style='height:20px'></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2,
                $"Inner col 1: 80 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 120) < 2,
                $"Inner col 2: 120 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2,
                $"Inner col 3: 100 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEX §9.7] Flex inside grid with flex-grow distributing grid item width
        [Fact]
        public void FlexInsideGrid_FlexGrowDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div style='display:flex'>
                        <div id='a' style='flex:1;height:30px'></div>
                        <div id='b' style='flex:3;height:30px'></div>
                    </div>
                    <div style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 2,
                $"flex:1 of 200px = 50 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"flex:3 of 200px = 150 (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §12] Grid item with border-box containing inner grid
        [Fact]
        public void GridItem_BorderBox_ContainingInnerGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div style='box-sizing:border-box;padding:20px;display:grid;grid-template-columns:1fr 1fr'>
                        <div id='t' style='height:20px'></div>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 130) < 2,
                $"border-box 300px - 40px padding = 260 content, 260/2 = 130 (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §12] Three-level: flex > grid > flex
        [Fact]
        public void ThreeLevelNesting_FlexGridFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div style='flex:1;display:grid;grid-template-columns:1fr'>
                        <div style='display:flex'>
                            <div id='t' style='flex:1;height:20px'></div>
                        </div>
                    </div>
                    <div style='flex:1;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"flex>grid>flex: 400/2=200, 1fr=200, flex:1=200 (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §10.5] Grid item align-items stretch with nested grid
        [Fact]
        public void NestedGrid_OuterStretchAffectsInner()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px 200px;width:400px'>
                    <div style='display:grid;grid-template-columns:1fr'>
                        <div id='short' style='height:30px'></div>
                    </div>
                    <div id='tall' style='height:80px'></div>
                </div></body>");
            var shortItem = LayoutTestHelper.FindById(root, "short")!;
            var tallItem = LayoutTestHelper.FindById(root, "tall")!;
            _output.WriteLine($"short parent height: {shortItem.Parent?.ContentRect.Height}, tall: {tallItem.ContentRect.Height}");
            Assert.True(shortItem.Parent != null && shortItem.Parent.ContentRect.Height >= 78,
                $"Inner grid stretches to match tall sibling row (got {shortItem.Parent?.ContentRect.Height})");
        }

        // [CSS-GRID §12] Nested grid with fixed and fr mix
        [Fact]
        public void InnerGrid_MixedFixedAndFr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div style='display:grid;grid-template-columns:100px 1fr'>
                        <div id='fixed' style='height:20px'></div>
                        <div id='flexible' style='height:20px'></div>
                    </div>
                </div></body>");
            var fixedItem = LayoutTestHelper.FindById(root, "fixed")!;
            var flexibleItem = LayoutTestHelper.FindById(root, "flexible")!;
            Assert.True(System.Math.Abs(fixedItem.ContentRect.Width - 100) < 2,
                $"Fixed col: 100 (got {fixedItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(flexibleItem.ContentRect.Width - 200) < 2,
                $"Fr col: 200 (got {flexibleItem.ContentRect.Width})");
        }

        // [CSS-GRID §12] Grid inside grid, inner with row gap
        [Fact]
        public void InnerGrid_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div style='display:grid;grid-template-columns:1fr;row-gap:10px'>
                        <div id='a' style='height:30px'></div>
                        <div id='b' style='height:30px'></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float gap = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(gap - 10) < 2,
                $"Inner row gap: 10 (got {gap})");
        }

        // [CSS-FLEX §4] Flex column inside grid item
        [Fact]
        public void FlexColumn_InsideGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>
                    <div style='display:flex;flex-direction:column'>
                        <div id='a' style='flex:1'></div>
                        <div id='b' style='flex:1'></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 60) < 2,
                $"Flex column in 120px grid row: each 60 (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 60) < 2,
                $"Second flex item at Y=60 (got {itemB.ContentRect.Y})");
        }

        // [CSS-GRID §12] Grid with percentage columns inside grid
        [Fact]
        public void InnerGrid_PercentageColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:400px;width:400px'>
                    <div style='display:grid;grid-template-columns:25% 75%'>
                        <div id='a' style='height:20px'></div>
                        <div id='b' style='height:20px'></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"25% of 400 = 100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 300) < 2,
                $"75% of 400 = 300 (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §12] Grid nested in grid with minmax tracks
        [Fact]
        public void InnerGrid_MinmaxTracks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div style='display:grid;grid-template-columns:minmax(50px,1fr) minmax(50px,2fr)'>
                        <div id='a' style='height:20px'></div>
                        <div id='b' style='height:20px'></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"1fr of 3fr total: 300/3 = 100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"2fr of 3fr total: 600/3 = 200 (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §12] Four-level deep nesting: grid > grid > grid > grid
        [Fact]
        public void FourLevelNesting_AllGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:320px;width:320px'>
                    <div style='display:grid;grid-template-columns:1fr 1fr'>
                        <div style='display:grid;grid-template-columns:1fr 1fr'>
                            <div style='display:grid;grid-template-columns:1fr 1fr'>
                                <div id='t' style='height:10px'></div>
                                <div style='height:10px'></div>
                            </div>
                            <div style='height:10px'></div>
                        </div>
                        <div style='height:10px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 40) < 2,
                $"4-level: 320/2/2/2 = 40 (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §12] Grid auto-placement inside nested grid
        [Fact]
        public void NestedGrid_AutoPlacement_FillsCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div style='display:grid;grid-template-columns:repeat(4,1fr)'>
                        <div id='a' style='height:20px'></div>
                        <div id='b' style='height:20px'></div>
                        <div id='c' style='height:20px'></div>
                        <div id='d' style='height:20px'></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 2,
                $"repeat(4,1fr) in 200px: each=50 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 150) < 2,
                $"4th item at X=150 (got {itemD.ContentRect.X})");
        }

        // [CSS-GRID §10.5] Inner grid align-self end inside outer row
        [Fact]
        public void InnerGrid_AlignSelfEnd_InOuterRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px 200px;grid-template-rows:100px;width:400px'>
                    <div style='align-self:end;display:grid;grid-template-columns:1fr'>
                        <div id='t' style='height:30px'></div>
                    </div>
                    <div style='height:100px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 68,
                $"align-self:end on inner grid: Y near 70 (got {target.ContentRect.Y})");
        }
    }
}
