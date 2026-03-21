using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexItemMarginCollapseTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexItemMarginCollapseTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void FlexRow_AdjacentItems_MarginsDoNotCollapse()
        {
            // CSS Flexbox spec: margins of adjacent flex items never collapse
            // Two items with margin-right:20 and margin-left:30 should produce 50px gap, not 30px
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:400px;'>
                    <div id='a' style='width:100px; height:50px; margin-right:20px;'></div>
                    <div id='b' style='width:100px; height:50px; margin-left:30px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float gap = itemB!.ContentRect.X - (itemA!.ContentRect.X + itemA.ContentRect.Width);
            _output.WriteLine($"gap between items: {gap}");
            // Margins add: 20 + 30 = 50
            Assert.True(System.Math.Abs(gap - 50) < 2,
                $"Flex item margins should add (20+30=50), not collapse (got {gap})");
        }

        [Fact]
        public void FlexRow_ItemMarginTop_DoesNotCollapseWithContainer()
        {
            // Flex item margin-top should not collapse with flex container margin-top
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='container' style='display:flex; width:400px; margin-top:20px;'>
                    <div id='item' style='width:100px; height:50px; margin-top:30px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(container);
            Assert.NotNull(item);
            _output.WriteLine($"container.Y={container!.ContentRect.Y} item.Y={item!.ContentRect.Y}");
            // Container at Y=20, item margin-top=30 pushes item down inside container
            // If margins collapsed, container would be at Y=30 and item at same position
            Assert.True(container.ContentRect.Y >= 18,
                $"Container should start at ~20px (got {container.ContentRect.Y})");
            Assert.True(item.ContentRect.Y > container.ContentRect.Y,
                $"Item should be below container content start (item.Y={item.ContentRect.Y}, container.Y={container.ContentRect.Y})");
        }

        [Fact]
        public void FlexColumn_AdjacentItems_MarginsAdd()
        {
            // In column flex, adjacent items' vertical margins should add, not collapse
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; width:200px;'>
                    <div id='a' style='height:40px; margin-bottom:20px;'></div>
                    <div id='b' style='height:40px; margin-top:30px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float gap = itemB!.ContentRect.Y - (itemA!.ContentRect.Y + itemA.ContentRect.Height);
            _output.WriteLine($"vertical gap between column flex items: {gap}");
            // Margins add: 20 + 30 = 50
            Assert.True(System.Math.Abs(gap - 50) < 2,
                $"Column flex margins should add (20+30=50), not collapse (got {gap})");
        }

        [Fact]
        public void FlexColumn_ItemMarginTop_DoesNotCollapseWithContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='container' style='display:flex; flex-direction:column; width:200px; margin-top:15px;'>
                    <div id='item' style='height:40px; margin-top:25px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(container);
            Assert.NotNull(item);
            _output.WriteLine($"container.Y={container!.ContentRect.Y} item.Y={item!.ContentRect.Y}");
            // No collapse: container at 15, item 25px below container content start
            Assert.True(item!.ContentRect.Y >= container.ContentRect.Y + 23,
                $"Item margin should not collapse with container (item.Y={item.ContentRect.Y}, container.Y={container.ContentRect.Y})");
        }

        [Fact]
        public void FlexRow_ThreeItems_AllMarginsAdd()
        {
            // Three flex items: all inter-item margins should add
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:400px;'>
                    <div id='a' style='width:50px; height:30px; margin-right:10px;'></div>
                    <div id='b' style='width:50px; height:30px; margin-left:10px; margin-right:15px;'></div>
                    <div id='c' style='width:50px; height:30px; margin-left:5px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            float gapAB = itemB!.ContentRect.X - (itemA!.ContentRect.X + itemA.ContentRect.Width);
            float gapBC = itemC!.ContentRect.X - (itemB.ContentRect.X + itemB.ContentRect.Width);
            _output.WriteLine($"gapAB={gapAB} gapBC={gapBC}");
            Assert.True(System.Math.Abs(gapAB - 20) < 2,
                $"A-B gap should be 10+10=20 (got {gapAB})");
            Assert.True(System.Math.Abs(gapBC - 20) < 2,
                $"B-C gap should be 15+5=20 (got {gapBC})");
        }

        [Fact]
        public void Block_AdjacentSiblings_MarginsCollapse_ForComparison()
        {
            // In normal block flow, adjacent sibling margins collapse (max wins)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px;'>
                    <div id='a' style='height:40px; margin-bottom:20px;'></div>
                    <div id='b' style='height:40px; margin-top:30px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float gap = itemB!.ContentRect.Y - (itemA!.ContentRect.Y + itemA.ContentRect.Height);
            _output.WriteLine($"block gap (collapsed): {gap}");
            // Block margins collapse: max(20, 30) = 30
            Assert.True(System.Math.Abs(gap - 30) < 2,
                $"Block margins should collapse to max(20,30)=30 (got {gap})");
        }

        [Fact]
        public void FlexColumn_SameMargins_AddInsteadOfCollapse()
        {
            // Same margins as block test but in flex column -- should add, not collapse
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; width:200px;'>
                    <div id='a' style='height:40px; margin-bottom:20px;'></div>
                    <div id='b' style='height:40px; margin-top:30px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float gap = itemB!.ContentRect.Y - (itemA!.ContentRect.Y + itemA.ContentRect.Height);
            _output.WriteLine($"flex column gap (no collapse): {gap}");
            // Flex margins add: 20 + 30 = 50
            Assert.True(System.Math.Abs(gap - 50) < 2,
                $"Flex column margins should add (20+30=50), not collapse (got {gap})");
        }

        [Fact]
        public void FlexItem_ChildMargin_DoesNotCollapseThrough()
        {
            // Child's margin-top inside a flex item should not collapse through the flex item boundary
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; width:200px;'>
                    <div id='itemA' style='height:40px; margin-bottom:10px;'></div>
                    <div id='itemB'>
                        <div id='child' style='margin-top:30px; height:20px;'></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "itemA");
            var itemB = LayoutTestHelper.FindById(root, "itemB");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(child);
            _output.WriteLine($"itemA.Y={itemA!.ContentRect.Y} itemB.Y={itemB!.ContentRect.Y} child.Y={child!.ContentRect.Y}");
            // Child's margin-top should be contained within itemB, not leak out
            Assert.True(child.ContentRect.Y >= itemB.ContentRect.Y + 28,
                $"Child margin should stay inside flex item (child.Y={child.ContentRect.Y}, itemB.Y={itemB.ContentRect.Y})");
        }

        [Fact]
        public void FlexItem_ChildMarginBottom_DoesNotCollapseWithNextItem()
        {
            // Last child margin-bottom inside a flex item should not collapse with next flex item
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; width:200px;'>
                    <div id='itemA'>
                        <div style='height:30px; margin-bottom:20px;'></div>
                    </div>
                    <div id='itemB' style='margin-top:15px; height:30px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "itemA");
            var itemB = LayoutTestHelper.FindById(root, "itemB");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float borderBottomA = itemA!.ContentRect.Y + itemA.ContentRect.Height;
            float gap = itemB!.ContentRect.Y - borderBottomA;
            _output.WriteLine($"itemA bottom={borderBottomA} itemB.Y={itemB.ContentRect.Y} gap={gap}");
            // itemA's auto height includes child's margin-bottom (or not, depending on collapse rules within BFC)
            // But the inter-item gap should include itemB's margin-top=15 fully
            Assert.True(itemB.ContentRect.Y > borderBottomA,
                $"ItemB should be below itemA (itemB.Y={itemB.ContentRect.Y}, itemA bottom={borderBottomA})");
        }

        [Fact]
        public void GridItems_MarginsDoNotCollapse()
        {
            // Grid items also don't collapse margins (same spec principle)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid; grid-template-columns:1fr; width:200px;'>
                    <div id='a' style='height:40px; margin-bottom:20px;'></div>
                    <div id='b' style='height:40px; margin-top:30px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float gap = itemB!.ContentRect.Y - (itemA!.ContentRect.Y + itemA.ContentRect.Height);
            _output.WriteLine($"grid item gap: {gap}");
            // Grid margins should add: 20 + 30 = 50
            Assert.True(System.Math.Abs(gap - 50) < 2,
                $"Grid item margins should add (20+30=50), not collapse (got {gap})");
        }

        [Fact]
        public void GridItems_MarginDoesNotCollapseWithContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='container' style='display:grid; grid-template-columns:1fr; width:200px; margin-top:20px;'>
                    <div id='item' style='height:40px; margin-top:30px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(container);
            Assert.NotNull(item);
            _output.WriteLine($"grid container.Y={container!.ContentRect.Y} item.Y={item!.ContentRect.Y}");
            Assert.True(container.ContentRect.Y >= 18,
                $"Grid container at ~20px (got {container.ContentRect.Y})");
            Assert.True(item!.ContentRect.Y > container.ContentRect.Y,
                $"Grid item should be pushed down by its own margin (item.Y={item.ContentRect.Y}, container.Y={container.ContentRect.Y})");
        }

        [Fact]
        public void FlexRow_EqualMargins_Add()
        {
            // Two items with identical margin-right and margin-left
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:400px;'>
                    <div id='a' style='width:80px; height:30px; margin-right:25px;'></div>
                    <div id='b' style='width:80px; height:30px; margin-left:25px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float gap = itemB!.ContentRect.X - (itemA!.ContentRect.X + itemA.ContentRect.Width);
            _output.WriteLine($"equal margins gap: {gap}");
            // 25 + 25 = 50 (in block context with collapse it would be max(25,25) = 25)
            Assert.True(System.Math.Abs(gap - 50) < 2,
                $"Equal margins should add to 50 in flex (got {gap})");
        }

        [Fact]
        public void FlexColumn_MarginTopAndBottom_BothPreserved()
        {
            // Each flex item's margin-top and margin-bottom are fully preserved
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; width:200px;'>
                    <div id='a' style='height:30px; margin-top:10px; margin-bottom:15px;'></div>
                    <div id='b' style='height:30px; margin-top:20px; margin-bottom:10px;'></div>
                    <div id='c' style='height:30px; margin-top:5px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a.Y={itemA!.ContentRect.Y} b.Y={itemB!.ContentRect.Y} c.Y={itemC!.ContentRect.Y}");
            // A starts at margin-top 10: Y=10, bottom at 10+30=40
            // Gap A-B: 15 + 20 = 35, so B.Y = 40 + 35 = 75
            // Gap B-C: 10 + 5 = 15, so C.Y = 75 + 30 + 15 = 120
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 10) < 2,
                $"A.Y should be ~10 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 75) < 2,
                $"B.Y should be ~75 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 120) < 2,
                $"C.Y should be ~120 (got {itemC.ContentRect.Y})");
        }

        [Fact]
        public void FlexItem_NestedBlockChildren_MarginsCollapseInsideItem()
        {
            // Inside a flex item (which is a BFC), child block margins CAN collapse with each other
            // But they cannot escape the flex item boundary
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; width:200px;'>
                    <div id='item'>
                        <div id='childA' style='height:20px; margin-bottom:15px;'></div>
                        <div id='childB' style='height:20px; margin-top:25px;'></div>
                    </div>
                </div></body>");
            var childA = LayoutTestHelper.FindById(root, "childA");
            var childB = LayoutTestHelper.FindById(root, "childB");
            Assert.NotNull(childA);
            Assert.NotNull(childB);
            float gap = childB!.ContentRect.Y - (childA!.ContentRect.Y + childA.ContentRect.Height);
            _output.WriteLine($"block children inside flex item gap: {gap}");
            // Inside BFC, margins collapse: max(15, 25) = 25
            Assert.True(System.Math.Abs(gap - 25) < 2,
                $"Block children inside flex item should collapse margins to 25 (got {gap})");
        }

        [Fact]
        public void FlexRow_MarginLeft_FirstItem_PreservedFromContainer()
        {
            // First item's margin-left should not collapse with container
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='container' style='display:flex; width:400px;'>
                    <div id='item' style='width:100px; height:30px; margin-left:40px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(container);
            Assert.NotNull(item);
            float offset = item!.ContentRect.X - container!.ContentRect.X;
            _output.WriteLine($"first item offset from container: {offset}");
            Assert.True(System.Math.Abs(offset - 40) < 2,
                $"First item margin-left=40 preserved (got offset {offset})");
        }

        [Fact]
        public void FlexColumn_LastItemMarginBottom_DoesNotCollapseWithContainer()
        {
            // Last flex item's margin-bottom should not collapse with container margin-bottom
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='container' style='display:flex; flex-direction:column; width:200px; margin-bottom:20px;'>
                    <div id='item' style='height:50px; margin-bottom:30px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(container);
            Assert.NotNull(item);
            _output.WriteLine($"container height={container!.ContentRect.Height} item height={item!.ContentRect.Height}");
            // Container auto height should include item height + item margin-bottom
            // If margins collapsed, container height would be just 50
            Assert.True(container.ContentRect.Height >= 78,
                $"Container should encompass item + its margin-bottom (height={container.ContentRect.Height})");
        }

        [Fact]
        public void FlexColumn_ZeroMargins_NoGap()
        {
            // With no margins, flex items should be directly adjacent
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; width:200px;'>
                    <div id='a' style='height:40px;'></div>
                    <div id='b' style='height:40px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float gap = itemB!.ContentRect.Y - (itemA!.ContentRect.Y + itemA.ContentRect.Height);
            _output.WriteLine($"zero margin gap: {gap}");
            Assert.True(System.Math.Abs(gap) < 2,
                $"No margins means no gap (got {gap})");
        }

        [Fact]
        public void FlexRow_MarginRightOnLast_PreservedFromContainerEdge()
        {
            // Last item's margin-right should be preserved, not collapsed with container
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='container' style='display:flex; width:400px;'>
                    <div id='item' style='width:100px; height:30px; margin-right:50px;'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(container);
            Assert.NotNull(item);
            _output.WriteLine($"item marginRight={item!.MarginRight}");
            Assert.True(System.Math.Abs(item.MarginRight - 50) < 2,
                $"Last item margin-right should be preserved (got {item.MarginRight})");
        }

        [Fact]
        public void FlexVsBlock_SameMarkup_DifferentMarginBehavior()
        {
            // Compare identical children in flex vs block context
            string children = @"
                <div id='a' style='height:30px; margin-bottom:20px;'></div>
                <div id='b' style='height:30px; margin-top:20px;'></div>";
            var blockRoot = LayoutTestHelper.Layout($@"
                <body style='margin:0'>
                <div style='width:200px;'>{children}</div></body>");
            var flexRoot = LayoutTestHelper.Layout($@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; width:200px;'>{children}</div></body>");
            var blockB = LayoutTestHelper.FindById(blockRoot, "b");
            var flexB = LayoutTestHelper.FindById(flexRoot, "b");
            Assert.NotNull(blockB);
            Assert.NotNull(flexB);
            _output.WriteLine($"block B.Y={blockB!.ContentRect.Y} flex B.Y={flexB!.ContentRect.Y}");
            // Block: margins collapse max(20,20)=20, gap=20, B.Y=30+20=50
            // Flex: margins add 20+20=40, gap=40, B.Y=30+40=70
            Assert.True(flexB.ContentRect.Y > blockB.ContentRect.Y + 10,
                $"Flex B should be lower than block B (flex={flexB.ContentRect.Y}, block={blockB.ContentRect.Y})");
        }

        [Fact]
        public void FlexColumn_LargeMarginBottom_SmallMarginTop_Add()
        {
            // Asymmetric margins that would collapse to max(40,5)=40 in block, but add to 45 in flex
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; width:200px;'>
                    <div id='a' style='height:30px; margin-bottom:40px;'></div>
                    <div id='b' style='height:30px; margin-top:5px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float gap = itemB!.ContentRect.Y - (itemA!.ContentRect.Y + itemA.ContentRect.Height);
            _output.WriteLine($"asymmetric margin gap: {gap}");
            Assert.True(System.Math.Abs(gap - 45) < 2,
                $"Asymmetric margins should add (40+5=45), not collapse to 40 (got {gap})");
        }

        [Fact]
        public void FlexColumn_OnlyMarginBottom_PreservedFully()
        {
            // Only margin-bottom on first item, no margin-top on second
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; width:200px;'>
                    <div id='a' style='height:30px; margin-bottom:35px;'></div>
                    <div id='b' style='height:30px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float gap = itemB!.ContentRect.Y - (itemA!.ContentRect.Y + itemA.ContentRect.Height);
            _output.WriteLine($"margin-bottom only gap: {gap}");
            Assert.True(System.Math.Abs(gap - 35) < 2,
                $"margin-bottom=35 fully preserved (got {gap})");
        }

        [Fact]
        public void FlexColumn_OnlyMarginTop_PreservedFully()
        {
            // Only margin-top on second item, no margin-bottom on first
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; width:200px;'>
                    <div id='a' style='height:30px;'></div>
                    <div id='b' style='height:30px; margin-top:35px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float gap = itemB!.ContentRect.Y - (itemA!.ContentRect.Y + itemA.ContentRect.Height);
            _output.WriteLine($"margin-top only gap: {gap}");
            Assert.True(System.Math.Abs(gap - 35) < 2,
                $"margin-top=35 fully preserved (got {gap})");
        }

        [Fact]
        public void FlexItem_FirstChildMarginTop_DoesNotEscapeItem()
        {
            // First child's margin-top inside a flex item should not escape to affect flex layout
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; width:200px;'>
                    <div id='itemA' style='height:40px;'></div>
                    <div id='itemB'>
                        <div id='child' style='margin-top:50px; height:20px;'></div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "itemA");
            var itemB = LayoutTestHelper.FindById(root, "itemB");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"itemA bottom={itemA!.ContentRect.Y + itemA.ContentRect.Height} itemB.Y={itemB!.ContentRect.Y}");
            // itemB should start right after itemA (no gap since no margins on items themselves)
            float gap = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(gap) < 2,
                $"Child's margin-top should not escape flex item boundary (gap={gap})");
        }

        [Fact]
        public void FlexRow_AllItemsWithMargin_TotalOffset()
        {
            // Verify total X positions account for all non-collapsed margins
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:400px;'>
                    <div id='a' style='width:60px; height:30px; margin-left:10px; margin-right:10px;'></div>
                    <div id='b' style='width:60px; height:30px; margin-left:10px; margin-right:10px;'></div>
                    <div id='c' style='width:60px; height:30px; margin-left:10px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a.X={itemA!.ContentRect.X} b.X={itemB!.ContentRect.X} c.X={itemC!.ContentRect.X}");
            // A: starts at margin-left=10, X=10, ends at 10+60=70, margin-right=10 -> 80
            // B: margin-left=10 -> 90, X=90, ends at 90+60=150, margin-right=10 -> 160
            // C: margin-left=10 -> 170, X=170
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 10) < 2,
                $"A.X should be ~10 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 90) < 2,
                $"B.X should be ~90 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 170) < 2,
                $"C.X should be ~170 (got {itemC.ContentRect.X})");
        }
    }
}
