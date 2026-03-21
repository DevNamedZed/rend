using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// <spec>CSS-FLEXBOX §8 https://drafts.csswg.org/css-flexbox-1/#alignment</spec>
    /// Covers all align-items, align-self, justify-content values in row and column directions.
    /// </summary>
    public class WptFlexAlignAllValuesTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexAlignAllValuesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // ────────────────────────────────────────────────────────────────
        // align-items (row direction, cross axis = Y)
        // ────────────────────────────────────────────────────────────────

        // [CSS-FLEXBOX §8.3] align-items: stretch — items fill cross axis
        [Fact]
        public void AlignItems_Stretch_ItemFillsCrossAxis()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:stretch;height:200px;width:300px'>" +
                "<div id='item' style='width:50px'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.Y={item.ContentRect.Y} item.H={item.ContentRect.Height}");
            Assert.True(item.ContentRect.Y < 2, $"Stretch Y should be 0 (got {item.ContentRect.Y})");
            Assert.True(item.ContentRect.Height >= 198,
                $"Stretch should fill 200px container (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §8.3] align-items: flex-start — items at cross-start
        [Fact]
        public void AlignItems_FlexStart_ItemAtTop()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:flex-start;height:200px;width:300px'>" +
                "<div id='item' style='width:50px;height:40px'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.Y={item.ContentRect.Y}");
            Assert.True(item.ContentRect.Y < 2, $"flex-start Y should be 0 (got {item.ContentRect.Y})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 40) < 2,
                $"Height should be 40 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §8.3] align-items: flex-end — items at cross-end
        [Fact]
        public void AlignItems_FlexEnd_ItemAtBottom()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:flex-end;height:200px;width:300px'>" +
                "<div id='item' style='width:50px;height:40px'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedY = 200 - 40;
            _output.WriteLine($"item.Y={item.ContentRect.Y} expected={expectedY}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"flex-end Y should be {expectedY} (got {item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.3] align-items: center — items centered on cross axis
        [Fact]
        public void AlignItems_Center_ItemCentered()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:center;height:200px;width:300px'>" +
                "<div id='item' style='width:50px;height:40px'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedY = (200 - 40) / 2f;
            _output.WriteLine($"item.Y={item.ContentRect.Y} expected={expectedY}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"center Y should be {expectedY} (got {item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.3] align-items: baseline — items aligned to first baseline
        [Fact]
        public void AlignItems_Baseline_ItemsShareBaseline()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:baseline;width:300px'>" +
                "<div id='tall' style='width:50px;font-size:32px'>A</div>" +
                "<div id='short' style='width:50px;font-size:16px'>B</div>" +
                "</div></body>");

            var tall = LayoutTestHelper.FindById(root, "tall")!;
            var shortItem = LayoutTestHelper.FindById(root, "short")!;
            _output.WriteLine($"tall.Y={tall.ContentRect.Y} tall.H={tall.ContentRect.Height} " +
                $"short.Y={shortItem.ContentRect.Y} short.H={shortItem.ContentRect.Height}");
            // The tall item (32px font) defines the container top. The short item (16px font)
            // is pushed down so its baseline aligns with the tall item's baseline.
            Assert.True(tall.ContentRect.Y < 2,
                $"Tall item should be near top (got {tall.ContentRect.Y})");
            Assert.True(shortItem.ContentRect.Y > 2,
                $"Short item should be pushed down for baseline alignment (got {shortItem.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // align-self (row direction, overrides align-items per item)
        // ────────────────────────────────────────────────────────────────

        // [CSS-FLEXBOX §8.3] align-self: stretch on a single item
        [Fact]
        public void AlignSelf_Stretch_ItemFillsCrossAxis()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:flex-start;height:200px;width:300px'>" +
                "<div id='normal' style='width:50px;height:40px'></div>" +
                "<div id='stretched' style='width:50px;align-self:stretch'></div>" +
                "</div></body>");

            var stretched = LayoutTestHelper.FindById(root, "stretched")!;
            _output.WriteLine($"stretched.H={stretched.ContentRect.Height}");
            Assert.True(stretched.ContentRect.Height >= 198,
                $"align-self:stretch should fill container (got {stretched.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §8.3] align-self: flex-start on a single item
        [Fact]
        public void AlignSelf_FlexStart_ItemAtTop()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:flex-end;height:200px;width:300px'>" +
                "<div id='item' style='width:50px;height:40px;align-self:flex-start'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.Y={item.ContentRect.Y}");
            Assert.True(item.ContentRect.Y < 2,
                $"align-self:flex-start should be at top (got {item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.3] align-self: flex-end on a single item
        [Fact]
        public void AlignSelf_FlexEnd_ItemAtBottom()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:flex-start;height:200px;width:300px'>" +
                "<div id='item' style='width:50px;height:40px;align-self:flex-end'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedY = 200 - 40;
            _output.WriteLine($"item.Y={item.ContentRect.Y} expected={expectedY}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"align-self:flex-end Y should be {expectedY} (got {item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.3] align-self: center on a single item
        [Fact]
        public void AlignSelf_Center_ItemCentered()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:flex-start;height:200px;width:300px'>" +
                "<div id='item' style='width:50px;height:40px;align-self:center'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedY = (200 - 40) / 2f;
            _output.WriteLine($"item.Y={item.ContentRect.Y} expected={expectedY}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"align-self:center Y should be {expectedY} (got {item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.3] align-self: auto inherits align-items value
        [Fact]
        public void AlignSelf_Auto_InheritsAlignItems()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:center;height:200px;width:300px'>" +
                "<div id='item' style='width:50px;height:40px;align-self:auto'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedY = (200 - 40) / 2f;
            _output.WriteLine($"item.Y={item.ContentRect.Y} expected={expectedY}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"align-self:auto should behave as center (got {item.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // align-self overrides align-items (6 combinations)
        // ────────────────────────────────────────────────────────────────

        // [CSS-FLEXBOX §8.3] align-items:stretch, align-self:flex-start
        [Fact]
        public void AlignSelf_FlexStart_Overrides_AlignItems_Stretch()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:stretch;height:200px;width:300px'>" +
                "<div id='item' style='width:50px;height:40px;align-self:flex-start'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.Y={item.ContentRect.Y} item.H={item.ContentRect.Height}");
            Assert.True(item.ContentRect.Y < 2, $"Should be at top (got {item.ContentRect.Y})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 40) < 2,
                $"Should keep height 40 not stretch (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §8.3] align-items:stretch, align-self:flex-end
        [Fact]
        public void AlignSelf_FlexEnd_Overrides_AlignItems_Stretch()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:stretch;height:200px;width:300px'>" +
                "<div id='item' style='width:50px;height:40px;align-self:flex-end'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedY = 200 - 40;
            _output.WriteLine($"item.Y={item.ContentRect.Y} expected={expectedY}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"Should be at bottom (got {item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.3] align-items:stretch, align-self:center
        [Fact]
        public void AlignSelf_Center_Overrides_AlignItems_Stretch()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:stretch;height:200px;width:300px'>" +
                "<div id='item' style='width:50px;height:40px;align-self:center'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedY = (200 - 40) / 2f;
            _output.WriteLine($"item.Y={item.ContentRect.Y} expected={expectedY}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"Should be centered (got {item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.3] align-items:center, align-self:flex-start
        [Fact]
        public void AlignSelf_FlexStart_Overrides_AlignItems_Center()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:center;height:200px;width:300px'>" +
                "<div id='item' style='width:50px;height:40px;align-self:flex-start'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.Y={item.ContentRect.Y}");
            Assert.True(item.ContentRect.Y < 2,
                $"Should be at top overriding center (got {item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.3] align-items:center, align-self:flex-end
        [Fact]
        public void AlignSelf_FlexEnd_Overrides_AlignItems_Center()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:center;height:200px;width:300px'>" +
                "<div id='item' style='width:50px;height:40px;align-self:flex-end'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedY = 200 - 40;
            _output.WriteLine($"item.Y={item.ContentRect.Y} expected={expectedY}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"Should be at bottom overriding center (got {item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.3] align-items:flex-end, align-self:stretch
        [Fact]
        public void AlignSelf_Stretch_Overrides_AlignItems_FlexEnd()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:flex-end;height:200px;width:300px'>" +
                "<div id='item' style='width:50px;align-self:stretch'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.Y={item.ContentRect.Y} item.H={item.ContentRect.Height}");
            Assert.True(item.ContentRect.Y < 2, $"Stretch starts at top (got {item.ContentRect.Y})");
            Assert.True(item.ContentRect.Height >= 198,
                $"Stretch should fill 200px (got {item.ContentRect.Height})");
        }

        // ────────────────────────────────────────────────────────────────
        // justify-content (row direction, main axis = X)
        // ────────────────────────────────────────────────────────────────

        // [CSS-FLEXBOX §8.2] justify-content: flex-start (default)
        [Fact]
        public void JustifyContent_FlexStart_ItemsAtLeft()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;justify-content:flex-start;width:300px'>" +
                "<div id='a' style='width:50px;height:30px'></div>" +
                "<div id='b' style='width:50px;height:30px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.X={itemA.ContentRect.X} b.X={itemB.ContentRect.X}");
            Assert.True(itemA.ContentRect.X < 2, $"First item at X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 50) < 2,
                $"Second item at X=50 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.2] justify-content: flex-end
        [Fact]
        public void JustifyContent_FlexEnd_ItemsAtRight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;justify-content:flex-end;width:300px'>" +
                "<div id='a' style='width:50px;height:30px'></div>" +
                "<div id='b' style='width:50px;height:30px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float expectedAx = 300 - 100; // 300 - (50+50)
            _output.WriteLine($"a.X={itemA.ContentRect.X} expected={expectedAx} b.X={itemB.ContentRect.X}");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedAx) < 2,
                $"First item at X={expectedAx} (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - (expectedAx + 50)) < 2,
                $"Second item at X={expectedAx + 50} (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.2] justify-content: center
        [Fact]
        public void JustifyContent_Center_ItemsCentered()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;justify-content:center;width:300px'>" +
                "<div id='a' style='width:50px;height:30px'></div>" +
                "<div id='b' style='width:50px;height:30px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            float expectedAx = (300 - 100) / 2f; // 100
            _output.WriteLine($"a.X={itemA.ContentRect.X} expected={expectedAx}");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedAx) < 2,
                $"Center offset should be {expectedAx} (got {itemA.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.2] justify-content: space-between
        [Fact]
        public void JustifyContent_SpaceBetween_ItemsSpreadAcrossMainAxis()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;justify-content:space-between;width:300px'>" +
                "<div id='a' style='width:50px;height:30px'></div>" +
                "<div id='b' style='width:50px;height:30px'></div>" +
                "<div id='c' style='width:50px;height:30px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.X={itemA.ContentRect.X} c.X={itemC.ContentRect.X}");
            Assert.True(itemA.ContentRect.X < 2, $"First item at X=0 (got {itemA.ContentRect.X})");
            float expectedCx = 300 - 50; // last item flush right
            Assert.True(System.Math.Abs(itemC.ContentRect.X - expectedCx) < 2,
                $"Last item at X={expectedCx} (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.2] justify-content: space-around
        [Fact]
        public void JustifyContent_SpaceAround_EqualHalfGapsAtEdges()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;justify-content:space-around;width:300px'>" +
                "<div id='a' style='width:50px;height:30px'></div>" +
                "<div id='b' style='width:50px;height:30px'></div>" +
                "<div id='c' style='width:50px;height:30px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Free space = 300 - 150 = 150. 3 items, 6 half-gaps of 25 each.
            // a.X = 25, b.X = 25+50+50 = 125, c.X = 125+50+50 = 225
            float expectedAx = 25;
            _output.WriteLine($"a.X={itemA.ContentRect.X} expected={expectedAx} b.X={itemB.ContentRect.X}");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedAx) < 2,
                $"space-around first item at X={expectedAx} (got {itemA.ContentRect.X})");
            float gapAB = itemB.ContentRect.X - (itemA.ContentRect.X + 50);
            Assert.True(System.Math.Abs(gapAB - 50) < 2,
                $"Gap between items should be 50 (got {gapAB})");
        }

        // [CSS-FLEXBOX §8.2] justify-content: space-evenly
        [Fact]
        public void JustifyContent_SpaceEvenly_EqualGapsEverywhere()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;justify-content:space-evenly;width:300px'>" +
                "<div id='a' style='width:50px;height:30px'></div>" +
                "<div id='b' style='width:50px;height:30px'></div>" +
                "<div id='c' style='width:50px;height:30px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Free space = 300 - 150 = 150. 4 gaps of 37.5 each.
            float expectedGap = 37.5f;
            _output.WriteLine($"a.X={itemA.ContentRect.X} b.X={itemB.ContentRect.X} c.X={itemC.ContentRect.X}");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedGap) < 2,
                $"space-evenly first item at X={expectedGap} (got {itemA.ContentRect.X})");
            float gapAB = itemB.ContentRect.X - (itemA.ContentRect.X + 50);
            Assert.True(System.Math.Abs(gapAB - expectedGap) < 2,
                $"Gap A-B should be {expectedGap} (got {gapAB})");
            float gapBC = itemC.ContentRect.X - (itemB.ContentRect.X + 50);
            Assert.True(System.Math.Abs(gapBC - expectedGap) < 2,
                $"Gap B-C should be {expectedGap} (got {gapBC})");
        }

        // ────────────────────────────────────────────────────────────────
        // Column direction: align-items affects X (cross axis)
        // ────────────────────────────────────────────────────────────────

        // [CSS-FLEXBOX §8.3] column + align-items:center — items centered on X
        [Fact]
        public void Column_AlignItems_Center_ItemCenteredOnX()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column;align-items:center;width:300px;height:200px'>" +
                "<div id='item' style='width:100px;height:40px'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedX = (300 - 100) / 2f;
            _output.WriteLine($"item.X={item.ContentRect.X} expected={expectedX}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"Column center X should be {expectedX} (got {item.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.3] column + align-items:flex-end — items at right edge (X)
        [Fact]
        public void Column_AlignItems_FlexEnd_ItemAtRight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column;align-items:flex-end;width:300px;height:200px'>" +
                "<div id='item' style='width:100px;height:40px'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedX = 300 - 100;
            _output.WriteLine($"item.X={item.ContentRect.X} expected={expectedX}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"Column flex-end X should be {expectedX} (got {item.ContentRect.X})");
        }

        // ────────────────────────────────────────────────────────────────
        // Column direction: justify-content affects Y (main axis)
        // ────────────────────────────────────────────────────────────────

        // [CSS-FLEXBOX §8.2] column + justify-content:center — items centered on Y
        [Fact]
        public void Column_JustifyContent_Center_ItemsCenteredOnY()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column;justify-content:center;width:300px;height:200px'>" +
                "<div id='a' style='width:100px;height:40px'></div>" +
                "<div id='b' style='width:100px;height:40px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            // Total content height = 80. Free = 120. Center offset = 60.
            float expectedY = (200 - 80) / 2f;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} expected={expectedY}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedY) < 2,
                $"Column center Y should be {expectedY} (got {itemA.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.2] column + justify-content:flex-end — items at bottom (Y)
        [Fact]
        public void Column_JustifyContent_FlexEnd_ItemsAtBottom()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column;justify-content:flex-end;width:300px;height:200px'>" +
                "<div id='a' style='width:100px;height:40px'></div>" +
                "<div id='b' style='width:100px;height:40px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float expectedAy = 200 - 80; // 120
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} expected={expectedAy} b.Y={itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedAy) < 2,
                $"Column flex-end first item Y should be {expectedAy} (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (expectedAy + 40)) < 2,
                $"Column flex-end second item Y should be {expectedAy + 40} (got {itemB.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // Additional coverage: multiple items with different align-self values
        // ────────────────────────────────────────────────────────────────

        // [CSS-FLEXBOX §8.3] Three items with different align-self values in one container
        [Fact]
        public void AlignSelf_MixedValues_ThreeItems()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:flex-start;height:200px;width:300px'>" +
                "<div id='start' style='width:50px;height:40px;align-self:flex-start'></div>" +
                "<div id='center' style='width:50px;height:40px;align-self:center'></div>" +
                "<div id='end' style='width:50px;height:40px;align-self:flex-end'></div>" +
                "</div></body>");

            var startItem = LayoutTestHelper.FindById(root, "start")!;
            var centerItem = LayoutTestHelper.FindById(root, "center")!;
            var endItem = LayoutTestHelper.FindById(root, "end")!;

            _output.WriteLine($"start.Y={startItem.ContentRect.Y} center.Y={centerItem.ContentRect.Y} end.Y={endItem.ContentRect.Y}");

            Assert.True(startItem.ContentRect.Y < 2,
                $"flex-start item at top (got {startItem.ContentRect.Y})");
            Assert.True(System.Math.Abs(centerItem.ContentRect.Y - 80) < 2,
                $"center item at Y=80 (got {centerItem.ContentRect.Y})");
            Assert.True(System.Math.Abs(endItem.ContentRect.Y - 160) < 2,
                $"flex-end item at Y=160 (got {endItem.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.2] justify-content: space-between with two items
        [Fact]
        public void JustifyContent_SpaceBetween_TwoItems()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;justify-content:space-between;width:300px'>" +
                "<div id='a' style='width:50px;height:30px'></div>" +
                "<div id='b' style='width:50px;height:30px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.X={itemA.ContentRect.X} b.X={itemB.ContentRect.X}");
            Assert.True(itemA.ContentRect.X < 2, $"First at X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 250) < 2,
                $"Second at X=250 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.3] column + align-items:flex-start — items at left (X=0)
        [Fact]
        public void Column_AlignItems_FlexStart_ItemAtLeft()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column;align-items:flex-start;width:300px;height:200px'>" +
                "<div id='item' style='width:100px;height:40px'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.X={item.ContentRect.X}");
            Assert.True(item.ContentRect.X < 2,
                $"Column flex-start X should be 0 (got {item.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.3] column + align-items:stretch — item fills width
        [Fact]
        public void Column_AlignItems_Stretch_ItemFillsWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column;align-items:stretch;width:300px;height:200px'>" +
                "<div id='item' style='height:40px'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.W={item.ContentRect.Width}");
            Assert.True(item.ContentRect.Width >= 298,
                $"Column stretch should fill 300px width (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §8.2] column + justify-content:space-between
        [Fact]
        public void Column_JustifyContent_SpaceBetween_ItemsSpreadOnY()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column;justify-content:space-between;width:300px;height:200px'>" +
                "<div id='a' style='width:100px;height:40px'></div>" +
                "<div id='b' style='width:100px;height:40px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
            Assert.True(itemA.ContentRect.Y < 2, $"First at top (got {itemA.ContentRect.Y})");
            float expectedBy = 200 - 40;
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - expectedBy) < 2,
                $"Last at bottom Y={expectedBy} (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.2] column + justify-content:flex-start — items at top (Y=0)
        [Fact]
        public void Column_JustifyContent_FlexStart_ItemsAtTop()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column;justify-content:flex-start;width:300px;height:200px'>" +
                "<div id='a' style='width:100px;height:40px'></div>" +
                "<div id='b' style='width:100px;height:40px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
            Assert.True(itemA.ContentRect.Y < 2, $"First at Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 40) < 2,
                $"Second at Y=40 (got {itemB.ContentRect.Y})");
        }

        // ────────────────────────────────────────────────────────────────
        // Edge cases and combined scenarios
        // ────────────────────────────────────────────────────────────────

        // [CSS-FLEXBOX §8.2] justify-content: center with single item
        [Fact]
        public void JustifyContent_Center_SingleItem()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;justify-content:center;width:300px'>" +
                "<div id='item' style='width:100px;height:30px'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedX = (300 - 100) / 2f;
            _output.WriteLine($"item.X={item.ContentRect.X} expected={expectedX}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"Single item centered at X={expectedX} (got {item.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.2] justify-content: space-around with single item centers it
        [Fact]
        public void JustifyContent_SpaceAround_SingleItem_Centered()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;justify-content:space-around;width:300px'>" +
                "<div id='item' style='width:100px;height:30px'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedX = (300 - 100) / 2f;
            _output.WriteLine($"item.X={item.ContentRect.X} expected={expectedX}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"space-around with 1 item should center at X={expectedX} (got {item.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.2+§8.3] Combined: justify-content:center + align-items:center
        [Fact]
        public void JustifyCenter_AlignCenter_ItemCenteredBothAxes()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;justify-content:center;align-items:center;width:300px;height:200px'>" +
                "<div id='item' style='width:80px;height:40px'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedX = (300 - 80) / 2f;
            float expectedY = (200 - 40) / 2f;
            _output.WriteLine($"item.X={item.ContentRect.X} item.Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"Center X should be {expectedX} (got {item.ContentRect.X})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"Center Y should be {expectedY} (got {item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.2+§8.3] Combined: justify-content:flex-end + align-items:flex-end
        [Fact]
        public void JustifyFlexEnd_AlignFlexEnd_ItemAtBottomRight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;justify-content:flex-end;align-items:flex-end;width:300px;height:200px'>" +
                "<div id='item' style='width:80px;height:40px'></div>" +
                "</div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedX = 300 - 80;
            float expectedY = 200 - 40;
            _output.WriteLine($"item.X={item.ContentRect.X} item.Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"flex-end X should be {expectedX} (got {item.ContentRect.X})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"flex-end Y should be {expectedY} (got {item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.3] column direction + align-self:center overrides align-items:flex-start
        [Fact]
        public void Column_AlignSelf_Center_Overrides_FlexStart()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column;align-items:flex-start;width:300px;height:200px'>" +
                "<div id='normal' style='width:100px;height:40px'></div>" +
                "<div id='centered' style='width:100px;height:40px;align-self:center'></div>" +
                "</div></body>");

            var normal = LayoutTestHelper.FindById(root, "normal")!;
            var centered = LayoutTestHelper.FindById(root, "centered")!;
            float expectedCenterX = (300 - 100) / 2f;
            _output.WriteLine($"normal.X={normal.ContentRect.X} centered.X={centered.ContentRect.X}");
            Assert.True(normal.ContentRect.X < 2,
                $"Normal item at X=0 (got {normal.ContentRect.X})");
            Assert.True(System.Math.Abs(centered.ContentRect.X - expectedCenterX) < 2,
                $"Centered item at X={expectedCenterX} (got {centered.ContentRect.X})");
        }

        // [CSS-FLEXBOX §8.2] column + justify-content:space-evenly
        [Fact]
        public void Column_JustifyContent_SpaceEvenly_EqualGapsOnY()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column;justify-content:space-evenly;width:300px;height:200px'>" +
                "<div id='a' style='width:100px;height:30px'></div>" +
                "<div id='b' style='width:100px;height:30px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Free = 200 - 60 = 140. 3 equal gaps of ~46.67.
            float expectedGap = 140f / 3f;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y} gap={expectedGap}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedGap) < 2,
                $"First item at Y={expectedGap} (got {itemA.ContentRect.Y})");
            float gapAB = itemB.ContentRect.Y - (itemA.ContentRect.Y + 30);
            Assert.True(System.Math.Abs(gapAB - expectedGap) < 2,
                $"Gap between items should be {expectedGap} (got {gapAB})");
        }

        // [CSS-FLEXBOX §8.2] column + justify-content:space-around
        [Fact]
        public void Column_JustifyContent_SpaceAround_HalfGapsAtEdgesOnY()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;flex-direction:column;justify-content:space-around;width:300px;height:200px'>" +
                "<div id='a' style='width:100px;height:30px'></div>" +
                "<div id='b' style='width:100px;height:30px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Free = 200 - 60 = 140. 4 half-gaps of 35 each. a.Y = 35.
            float expectedAy = 35;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} expected={expectedAy} b.Y={itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedAy) < 2,
                $"space-around first item at Y={expectedAy} (got {itemA.ContentRect.Y})");
            float gapAB = itemB.ContentRect.Y - (itemA.ContentRect.Y + 30);
            Assert.True(System.Math.Abs(gapAB - 70) < 2,
                $"Gap between items should be 70 (got {gapAB})");
        }

        // [CSS-FLEXBOX §8.3] align-items:flex-start with varying item heights
        [Fact]
        public void AlignItems_FlexStart_VaryingHeights_AllAtTop()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:flex-start;height:200px;width:300px'>" +
                "<div id='a' style='width:50px;height:30px'></div>" +
                "<div id='b' style='width:50px;height:60px'></div>" +
                "<div id='c' style='width:50px;height:90px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y} c.Y={itemC.ContentRect.Y}");
            Assert.True(itemA.ContentRect.Y < 2, $"Item a at top (got {itemA.ContentRect.Y})");
            Assert.True(itemB.ContentRect.Y < 2, $"Item b at top (got {itemB.ContentRect.Y})");
            Assert.True(itemC.ContentRect.Y < 2, $"Item c at top (got {itemC.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §8.3] align-items:flex-end with varying item heights
        [Fact]
        public void AlignItems_FlexEnd_VaryingHeights_AllAtBottom()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:flex;align-items:flex-end;height:200px;width:300px'>" +
                "<div id='a' style='width:50px;height:30px'></div>" +
                "<div id='b' style='width:50px;height:60px'></div>" +
                "<div id='c' style='width:50px;height:90px'></div>" +
                "</div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y} c.Y={itemC.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 170) < 2,
                $"Item a at Y=170 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 140) < 2,
                $"Item b at Y=140 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 110) < 2,
                $"Item c at Y=110 (got {itemC.ContentRect.Y})");
        }
    }
}
