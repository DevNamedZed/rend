using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexContainerPaddingTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexContainerPaddingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ContainerPadding_BorderBox_ReducesAvailableSpaceForItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 300px; box-sizing: border-box; padding: 20px;'>
                    <div id='item' style='flex-grow: 1; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item width={item!.ContentRect.Width}");
            float expectedWidth = 300 - 20 - 20;
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedWidth) < 2,
                $"border-box padding should reduce available space: expected ~{expectedWidth}, got {item.ContentRect.Width}");
        }

        [Fact]
        public void ContainerBorder_BorderBox_ReducesAvailableSpaceForItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 300px; box-sizing: border-box; border: 10px solid black;'>
                    <div id='item' style='flex-grow: 1; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item width={item!.ContentRect.Width}");
            float expectedWidth = 300 - 10 - 10;
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedWidth) < 2,
                $"border-box border should reduce available space: expected ~{expectedWidth}, got {item.ContentRect.Width}");
        }

        [Fact]
        public void ContainerPaddingAndBorder_BorderBox_ReducesAvailableSpaceForItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 400px; box-sizing: border-box; padding: 15px; border: 5px solid black;'>
                    <div id='item' style='flex-grow: 1; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item width={item!.ContentRect.Width}");
            float expectedWidth = 400 - 15 - 15 - 5 - 5;
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedWidth) < 2,
                $"border-box padding+border should reduce available space: expected ~{expectedWidth}, got {item.ContentRect.Width}");
        }

        [Fact]
        public void ContainerBorderBox_WithPadding_ItemWidthMatchesContentBox()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 300px; box-sizing: border-box; padding: 20px;'>
                    <div id='item' style='flex-grow: 1; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item width={item!.ContentRect.Width}");
            float expectedWidth = 300 - 20 - 20;
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedWidth) < 2,
                $"border-box with padding: item should fill content area ~{expectedWidth}, got {item.ContentRect.Width}");
        }

        [Fact]
        public void ContainerBorderBox_WithBorder_ItemWidthMatchesContentBox()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 300px; box-sizing: border-box; border: 10px solid black;'>
                    <div id='item' style='flex-grow: 1; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item width={item!.ContentRect.Width}");
            float expectedWidth = 300 - 10 - 10;
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedWidth) < 2,
                $"border-box with border: item should fill content area ~{expectedWidth}, got {item.ContentRect.Width}");
        }

        [Fact]
        public void ContainerBorderBox_WithPaddingAndBorder_ItemWidthMatchesContentBox()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 300px; box-sizing: border-box; padding: 15px; border: 5px solid black;'>
                    <div id='item' style='flex-grow: 1; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item width={item!.ContentRect.Width}");
            float expectedWidth = 300 - 15 - 15 - 5 - 5;
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedWidth) < 2,
                $"border-box with padding+border: item should fill content area ~{expectedWidth}, got {item.ContentRect.Width}");
        }

        [Fact]
        public void ItemsOffsetByContainerPadding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; width: 300px; padding: 25px;'>
                    <div id='item' style='width: 50px; height: 30px;'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(flex);
            Assert.NotNull(item);
            _output.WriteLine($"flex content X={flex!.ContentRect.X}, item X={item!.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - flex.ContentRect.X) < 2,
                $"Item X should match flex content X (item={item.ContentRect.X}, flex content={flex.ContentRect.X})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - flex.ContentRect.Y) < 2,
                $"Item Y should match flex content Y (item={item.ContentRect.Y}, flex content={flex.ContentRect.Y})");
        }

        [Fact]
        public void ItemX_IncludesPaddingOffset()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; width: 300px; padding-left: 30px;'>
                    <div id='first' style='width: 50px; height: 30px;'></div>
                    <div id='second' style='width: 50px; height: 30px;'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(flex);
            Assert.NotNull(first);
            Assert.NotNull(second);
            _output.WriteLine($"flex content X={flex!.ContentRect.X}, first X={first!.ContentRect.X}, second X={second!.ContentRect.X}");
            Assert.True(first.ContentRect.X >= 30 - 2,
                $"First item should start at padding-left offset ~30 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(second.ContentRect.X - (first.ContentRect.X + 50)) < 2,
                $"Second item X should be first X + first width (got {second.ContentRect.X})");
        }

        [Fact]
        public void ColumnFlex_ContainerPadding_BorderBox_ReducesCrossAxisWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; flex-direction: column; width: 200px; box-sizing: border-box; padding: 20px;'>
                    <div id='item' style='height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item width={item!.ContentRect.Width}");
            float expectedWidth = 200 - 20 - 20;
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedWidth) < 2,
                $"Column flex border-box padding reduces cross-axis width: expected ~{expectedWidth}, got {item.ContentRect.Width}");
        }

        [Fact]
        public void ColumnFlex_ContainerBorder_BorderBox_ReducesCrossAxisWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; flex-direction: column; width: 200px; box-sizing: border-box; border: 10px solid black;'>
                    <div id='item' style='height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item width={item!.ContentRect.Width}");
            float expectedWidth = 200 - 10 - 10;
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedWidth) < 2,
                $"Column flex border-box border reduces cross-axis width: expected ~{expectedWidth}, got {item.ContentRect.Width}");
        }

        [Fact]
        public void ContainerPadding_BorderBox_WithFlexGrow_ItemsFillReducedSpace()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 300px; box-sizing: border-box; padding: 20px;'>
                    <div id='a' style='flex-grow: 1; height: 30px;'></div>
                    <div id='b' style='flex-grow: 1; height: 30px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float availableWidth = 300 - 20 - 20;
            float expectedPerItem = availableWidth / 2f;
            _output.WriteLine($"a width={itemA!.ContentRect.Width}, b width={itemB!.ContentRect.Width}, expected ~{expectedPerItem}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - expectedPerItem) < 2,
                $"Item A should get half of reduced space: expected ~{expectedPerItem}, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - expectedPerItem) < 2,
                $"Item B should get half of reduced space: expected ~{expectedPerItem}, got {itemB.ContentRect.Width}");
        }

        [Fact]
        public void ContainerPadding_BorderBox_WithFlexShrink_ItemsShrinkWithinReducedSpace()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 200px; box-sizing: border-box; padding: 20px;'>
                    <div id='a' style='width: 120px; flex-shrink: 1; height: 30px;'></div>
                    <div id='b' style='width: 120px; flex-shrink: 1; height: 30px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float availableWidth = 200 - 20 - 20;
            _output.WriteLine($"a width={itemA!.ContentRect.Width}, b width={itemB!.ContentRect.Width}, available={availableWidth}");
            Assert.True(itemA.ContentRect.Width < 120,
                $"Item A should shrink from 120px (got {itemA.ContentRect.Width})");
            Assert.True(itemB.ContentRect.Width < 120,
                $"Item B should shrink from 120px (got {itemB.ContentRect.Width})");
            float totalItemWidth = itemA.ContentRect.Width + itemB.ContentRect.Width;
            Assert.True(System.Math.Abs(totalItemWidth - availableWidth) < 2,
                $"Items total width should equal available space ~{availableWidth} (got {totalItemWidth})");
        }

        [Fact]
        public void ContainerPadding_ContentBox_WithGap_GapAppliesWithinContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 300px; padding: 20px; gap: 10px;'>
                    <div id='a' style='flex-grow: 1; height: 30px;'></div>
                    <div id='b' style='flex-grow: 1; height: 30px;'></div>
                    <div id='c' style='flex-grow: 1; height: 30px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            float totalGap = 10 * 2;
            float expectedPerItem = (300 - totalGap) / 3f;
            _output.WriteLine($"a={itemA!.ContentRect.Width}, b={itemB!.ContentRect.Width}, c={itemC!.ContentRect.Width}, expected ~{expectedPerItem}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - expectedPerItem) < 2,
                $"Item A with gap: expected ~{expectedPerItem}, got {itemA.ContentRect.Width}");
            float gapBetweenAB = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(gapBetweenAB - 10) < 2,
                $"Gap between items should be 10px (got {gapBetweenAB})");
        }

        [Fact]
        public void ContainerPadding_ContentBox_WithJustifyContentCenter_CenteredInContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; width: 300px; padding: 20px; justify-content: center;'>
                    <div id='item' style='width: 100px; height: 30px;'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(flex);
            Assert.NotNull(item);
            float expectedItemX = flex!.ContentRect.X + (300 - 100) / 2f;
            _output.WriteLine($"item X={item!.ContentRect.X}, expected ~{expectedItemX}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedItemX) < 2,
                $"justify-content:center: item X expected ~{expectedItemX}, got {item.ContentRect.X}");
        }

        [Fact]
        public void ContainerPadding_ContentBox_WithAlignItemsCenter_CenteredInContentHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; width: 300px; height: 100px; padding: 20px; align-items: center;'>
                    <div id='item' style='width: 50px; height: 30px;'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(flex);
            Assert.NotNull(item);
            float expectedItemY = flex!.ContentRect.Y + (100 - 30) / 2f;
            _output.WriteLine($"item Y={item!.ContentRect.Y}, expected ~{expectedItemY}, flex content Y={flex.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedItemY) < 2,
                $"align-items:center: item Y expected ~{expectedItemY}, got {item.ContentRect.Y}");
        }

        [Fact]
        public void ContainerPadding_Percentage_BorderBox_ResolvesAgainstContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <div id='flex' style='display: flex; width: 400px; box-sizing: border-box; padding: 10%;'>
                        <div id='item' style='flex-grow: 1; height: 30px;'></div>
                    </div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(flex);
            Assert.NotNull(item);
            float percentagePadding = 400 * 0.10f;
            float expectedContentWidth = 400 - percentagePadding - percentagePadding;
            _output.WriteLine($"item width={item!.ContentRect.Width}, flex content width={flex!.ContentRect.Width}, expected ~{expectedContentWidth}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedContentWidth) < 2,
                $"border-box percentage padding (10% of 400): item width expected ~{expectedContentWidth}, got {item.ContentRect.Width}");
        }

        [Fact]
        public void ContainerPadding_ContentBox_ItemsGetFullContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 400px; padding: 30px;'>
                    <div id='a' style='flex-grow: 1; height: 30px;'></div>
                    <div id='b' style='flex-grow: 1; height: 30px;'></div>
                    <div id='c' style='flex-grow: 1; height: 30px;'></div>
                    <div id='d' style='flex-grow: 1; height: 30px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemD = LayoutTestHelper.FindById(root, "d");
            Assert.NotNull(itemA);
            Assert.NotNull(itemD);
            float expectedPerItem = 400 / 4f;
            _output.WriteLine($"a width={itemA!.ContentRect.Width}, d width={itemD!.ContentRect.Width}, expected ~{expectedPerItem}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - expectedPerItem) < 2,
                $"content-box: 4 equal flex-grow items share full width: expected ~{expectedPerItem}, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemD.ContentRect.Width - expectedPerItem) < 2,
                $"Last item should also be ~{expectedPerItem} (got {itemD.ContentRect.Width})");
        }

        [Fact]
        public void ContainerPadding_BorderBox_AsymmetricLeftRight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; width: 300px; box-sizing: border-box; padding-left: 40px; padding-right: 10px;'>
                    <div id='item' style='flex-grow: 1; height: 30px;'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(flex);
            Assert.NotNull(item);
            float expectedWidth = 300 - 40 - 10;
            _output.WriteLine($"item width={item!.ContentRect.Width}, expected ~{expectedWidth}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedWidth) < 2,
                $"border-box asymmetric padding: item width expected ~{expectedWidth}, got {item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.X - flex!.ContentRect.X) < 2,
                $"Item X should start at content X (item={item.ContentRect.X}, content={flex.ContentRect.X})");
        }

        [Fact]
        public void ContainerPadding_ContentBox_ContainerHeightReflectsPadding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; width: 300px; padding: 25px;'>
                    <div style='width: 50px; height: 40px;'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(flex);
            _output.WriteLine($"flex border height={flex!.BorderRect.Height}, content height={flex.ContentRect.Height}");
            float expectedBorderHeight = 40 + 25 + 25;
            Assert.True(System.Math.Abs(flex.BorderRect.Height - expectedBorderHeight) < 2,
                $"Border-box height should include padding: expected ~{expectedBorderHeight}, got {flex.BorderRect.Height}");
        }

        [Fact]
        public void ContainerPadding_ContentBox_SpaceBetween_DistributesInContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; width: 300px; padding: 20px; justify-content: space-between;'>
                    <div id='first' style='width: 40px; height: 30px;'></div>
                    <div id='last' style='width: 40px; height: 30px;'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            var first = LayoutTestHelper.FindById(root, "first");
            var last = LayoutTestHelper.FindById(root, "last");
            Assert.NotNull(flex);
            Assert.NotNull(first);
            Assert.NotNull(last);
            float expectedLastX = flex!.ContentRect.X + 300 - 40;
            _output.WriteLine($"first X={first!.ContentRect.X}, last X={last!.ContentRect.X}, expected last X ~{expectedLastX}");
            Assert.True(System.Math.Abs(first.ContentRect.X - flex.ContentRect.X) < 2,
                $"space-between first item at content start (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.X - expectedLastX) < 2,
                $"space-between last item at content end: expected ~{expectedLastX}, got {last.ContentRect.X}");
        }

        [Fact]
        public void ColumnFlex_ContainerPaddingTop_ItemsOffsetFromTop()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; flex-direction: column; width: 200px; padding-top: 30px;'>
                    <div id='item' style='height: 40px;'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(flex);
            Assert.NotNull(item);
            _output.WriteLine($"flex content Y={flex!.ContentRect.Y}, item Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - flex.ContentRect.Y) < 2,
                $"Column item Y should match flex content Y (item={item.ContentRect.Y}, flex content={flex.ContentRect.Y})");
            Assert.True(flex.ContentRect.Y >= 30 - 2,
                $"Flex content Y should account for padding-top ~30 (got {flex.ContentRect.Y})");
        }

        [Fact]
        public void ContainerBorder_WithPadding_CombinedInset()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; width: 300px; padding: 10px; border: 5px solid red;'>
                    <div id='item' style='width: 50px; height: 30px;'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(flex);
            Assert.NotNull(item);
            float expectedItemX = flex!.BorderRect.X + 5 + 10;
            _output.WriteLine($"flex border X={flex.BorderRect.X}, item X={item!.ContentRect.X}, expected ~{expectedItemX}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedItemX) < 2,
                $"Item X should be border + padding inset from border edge: expected ~{expectedItemX}, got {item.ContentRect.X}");
        }

        [Fact]
        public void ContainerPadding_BorderBox_WithFlexWrap_ItemsWrapWithinReducedWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; flex-wrap: wrap; width: 200px; box-sizing: border-box; padding: 20px;'>
                    <div id='a' style='width: 100px; height: 30px;'></div>
                    <div id='b' style='width: 100px; height: 30px;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float contentWidth = 200 - 20 - 20;
            _output.WriteLine($"content width={contentWidth}, a X={itemA!.ContentRect.X}, b X={itemB!.ContentRect.X}, b Y={itemB.ContentRect.Y}");
            Assert.True(itemB.ContentRect.Y > itemA.ContentRect.Y,
                $"Items should wrap in border-box (contentWidth={contentWidth}): b.Y={itemB.ContentRect.Y} should be > a.Y={itemA.ContentRect.Y}");
        }

        [Fact]
        public void ContainerPadding_FixedSizeItems_PositionedAfterPadding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; width: 300px; padding: 15px;'>
                    <div id='a' style='width: 60px; height: 30px;'></div>
                    <div id='b' style='width: 60px; height: 30px;'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(flex);
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float expectedFirstX = flex!.ContentRect.X;
            float expectedSecondX = expectedFirstX + 60;
            _output.WriteLine($"a X={itemA!.ContentRect.X} expected ~{expectedFirstX}, b X={itemB!.ContentRect.X} expected ~{expectedSecondX}");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedFirstX) < 2,
                $"First item X at content start: expected ~{expectedFirstX}, got {itemA.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - expectedSecondX) < 2,
                $"Second item X after first: expected ~{expectedSecondX}, got {itemB.ContentRect.X}");
        }

        [Fact]
        public void ColumnFlex_ContainerPadding_ItemHeightsUnaffectedByPadding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; flex-direction: column; width: 200px; height: 300px; padding: 20px;'>
                    <div id='item' style='height: 50px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item height={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2,
                $"Fixed-height item in column flex with padding should keep height 50 (got {item.ContentRect.Height})");
        }

        [Fact]
        public void ContainerPadding_ContentBox_WithAlignItemsFlexEnd_OffsetFromContentBottom()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; width: 300px; height: 120px; padding: 20px; align-items: flex-end;'>
                    <div id='item' style='width: 50px; height: 30px;'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(flex);
            Assert.NotNull(item);
            float expectedItemY = flex!.ContentRect.Y + 120 - 30;
            _output.WriteLine($"item Y={item!.ContentRect.Y}, expected ~{expectedItemY}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedItemY) < 2,
                $"align-items:flex-end: item Y expected ~{expectedItemY}, got {item.ContentRect.Y}");
        }
    }
}
