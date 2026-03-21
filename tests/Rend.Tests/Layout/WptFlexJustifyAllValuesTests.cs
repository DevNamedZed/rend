using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Flexbox justify-content property across all values and item counts.
    /// [CSS-FLEXBOX 8.2] Axis alignment: the justify-content property.
    /// Container width=400, each item width=50.
    /// </summary>
    public class WptFlexJustifyAllValuesTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexJustifyAllValuesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private const float ContainerWidth = 400f;
        private const float ItemWidth = 50f;
        private const float ItemHeight = 30f;
        private const float Tolerance = 1.5f;

        private static string BuildRowHtml(string justifyContent, int itemCount)
        {
            var items = "";
            for (int index = 0; index < itemCount; index++)
            {
                items += $"<div id='item{index}' style='width:{ItemWidth}px;height:{ItemHeight}px'></div>";
            }
            return $"<body style='margin:0'><div style='display:flex;justify-content:{justifyContent};width:{ContainerWidth}px'>{items}</div></body>";
        }

        private static string BuildColumnHtml(string justifyContent, int itemCount)
        {
            var items = "";
            for (int index = 0; index < itemCount; index++)
            {
                items += $"<div id='item{index}' style='width:{ItemWidth}px;height:{ItemWidth}px'></div>";
            }
            return $"<body style='margin:0'><div style='display:flex;flex-direction:column;justify-content:{justifyContent};width:{ContainerWidth}px;height:{ContainerWidth}px'>{items}</div></body>";
        }

        private void AssertItemX(Rend.Layout.LayoutBox root, int itemIndex, float expectedX)
        {
            var item = LayoutTestHelper.FindById(root, $"item{itemIndex}");
            Assert.NotNull(item);
            _output.WriteLine($"item{itemIndex}.X = {item!.ContentRect.X} (expected {expectedX})");
            Assert.True(
                System.Math.Abs(item.ContentRect.X - expectedX) < Tolerance,
                $"item{itemIndex}.X expected {expectedX}, got {item.ContentRect.X}");
        }

        private void AssertItemY(Rend.Layout.LayoutBox root, int itemIndex, float expectedY)
        {
            var item = LayoutTestHelper.FindById(root, $"item{itemIndex}");
            Assert.NotNull(item);
            _output.WriteLine($"item{itemIndex}.Y = {item!.ContentRect.Y} (expected {expectedY})");
            Assert.True(
                System.Math.Abs(item.ContentRect.Y - expectedY) < Tolerance,
                $"item{itemIndex}.Y expected {expectedY}, got {item.ContentRect.Y}");
        }

        // ========== flex-start ==========

        // [CSS-FLEXBOX 8.2] justify-content: flex-start - items packed at main-start
        [Fact]
        public void FlexStart_OneItem_StartsAtZero()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-start", 1));
            AssertItemX(root, 0, 0);
        }

        [Fact]
        public void FlexStart_TwoItems_PackedAtStart()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-start", 2));
            AssertItemX(root, 0, 0);
            AssertItemX(root, 1, 50);
        }

        [Fact]
        public void FlexStart_ThreeItems_PackedAtStart()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-start", 3));
            AssertItemX(root, 0, 0);
            AssertItemX(root, 1, 50);
            AssertItemX(root, 2, 100);
        }

        [Fact]
        public void FlexStart_FourItems_PackedAtStart()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-start", 4));
            AssertItemX(root, 0, 0);
            AssertItemX(root, 1, 50);
            AssertItemX(root, 2, 100);
            AssertItemX(root, 3, 150);
        }

        [Fact]
        public void FlexStart_FiveItems_PackedAtStart()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-start", 5));
            AssertItemX(root, 0, 0);
            AssertItemX(root, 1, 50);
            AssertItemX(root, 2, 100);
            AssertItemX(root, 3, 150);
            AssertItemX(root, 4, 200);
        }

        // ========== flex-end ==========

        // [CSS-FLEXBOX 8.2] justify-content: flex-end - items packed at main-end
        [Fact]
        public void FlexEnd_OneItem_AtEnd()
        {
            // free = 400 - 50 = 350
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-end", 1));
            AssertItemX(root, 0, 350);
        }

        [Fact]
        public void FlexEnd_TwoItems_PackedAtEnd()
        {
            // free = 400 - 100 = 300
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-end", 2));
            AssertItemX(root, 0, 300);
            AssertItemX(root, 1, 350);
        }

        [Fact]
        public void FlexEnd_ThreeItems_PackedAtEnd()
        {
            // free = 400 - 150 = 250
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-end", 3));
            AssertItemX(root, 0, 250);
            AssertItemX(root, 1, 300);
            AssertItemX(root, 2, 350);
        }

        [Fact]
        public void FlexEnd_FourItems_PackedAtEnd()
        {
            // free = 400 - 200 = 200
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-end", 4));
            AssertItemX(root, 0, 200);
            AssertItemX(root, 1, 250);
            AssertItemX(root, 2, 300);
            AssertItemX(root, 3, 350);
        }

        [Fact]
        public void FlexEnd_FiveItems_PackedAtEnd()
        {
            // free = 400 - 250 = 150
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-end", 5));
            AssertItemX(root, 0, 150);
            AssertItemX(root, 1, 200);
            AssertItemX(root, 2, 250);
            AssertItemX(root, 3, 300);
            AssertItemX(root, 4, 350);
        }

        // ========== center ==========

        // [CSS-FLEXBOX 8.2] justify-content: center - items centered in container
        [Fact]
        public void Center_OneItem_Centered()
        {
            // free = 400 - 50 = 350, offset = 175
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 1));
            AssertItemX(root, 0, 175);
        }

        [Fact]
        public void Center_TwoItems_Centered()
        {
            // free = 400 - 100 = 300, offset = 150
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 2));
            AssertItemX(root, 0, 150);
            AssertItemX(root, 1, 200);
        }

        [Fact]
        public void Center_ThreeItems_Centered()
        {
            // free = 400 - 150 = 250, offset = 125
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 3));
            AssertItemX(root, 0, 125);
            AssertItemX(root, 1, 175);
            AssertItemX(root, 2, 225);
        }

        [Fact]
        public void Center_FourItems_Centered()
        {
            // free = 400 - 200 = 200, offset = 100
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 4));
            AssertItemX(root, 0, 100);
            AssertItemX(root, 1, 150);
            AssertItemX(root, 2, 200);
            AssertItemX(root, 3, 250);
        }

        [Fact]
        public void Center_FiveItems_Centered()
        {
            // free = 400 - 250 = 150, offset = 75
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 5));
            AssertItemX(root, 0, 75);
            AssertItemX(root, 1, 125);
            AssertItemX(root, 2, 175);
            AssertItemX(root, 3, 225);
            AssertItemX(root, 4, 275);
        }

        // ========== space-between ==========

        // [CSS-FLEXBOX 8.2] justify-content: space-between - first at start, last at end, gaps evenly distributed
        [Fact]
        public void SpaceBetween_TwoItems_StartAndEnd()
        {
            // free = 400 - 100 = 300, gap = 300/1 = 300
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 2));
            AssertItemX(root, 0, 0);
            AssertItemX(root, 1, 350);
        }

        [Fact]
        public void SpaceBetween_ThreeItems_EvenlySpaced()
        {
            // free = 400 - 150 = 250, gap = 250/2 = 125
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 3));
            AssertItemX(root, 0, 0);
            AssertItemX(root, 1, 175);
            AssertItemX(root, 2, 350);
        }

        [Fact]
        public void SpaceBetween_FourItems_EvenlySpaced()
        {
            // free = 400 - 200 = 200, gap = 200/3 = 66.667
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 4));
            AssertItemX(root, 0, 0);
            float gap = 200f / 3f;
            AssertItemX(root, 1, 50 + gap);
            AssertItemX(root, 2, 100 + 2 * gap);
            AssertItemX(root, 3, 150 + 3 * gap);
        }

        [Fact]
        public void SpaceBetween_FiveItems_EvenlySpaced()
        {
            // free = 400 - 250 = 150, gap = 150/4 = 37.5
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 5));
            AssertItemX(root, 0, 0);
            AssertItemX(root, 1, 87.5f);
            AssertItemX(root, 2, 175);
            AssertItemX(root, 3, 262.5f);
            AssertItemX(root, 4, 350);
        }

        // ========== space-around ==========

        // [CSS-FLEXBOX 8.2] justify-content: space-around - equal space around each item (half-gaps at edges)
        [Fact]
        public void SpaceAround_TwoItems_HalfGapsAtEdges()
        {
            // free = 400 - 100 = 300, per-item share = 300/2 = 150, half = 75
            // item0 at 75, item1 at 75 + 50 + 150 = 275
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-around", 2));
            AssertItemX(root, 0, 75);
            AssertItemX(root, 1, 275);
        }

        [Fact]
        public void SpaceAround_ThreeItems_HalfGapsAtEdges()
        {
            // free = 400 - 150 = 250, per-item share = 250/3 = 83.333, half = 41.667
            // item0 at 41.667, item1 at 41.667 + 50 + 83.333 = 175, item2 at 175 + 50 + 83.333 = 308.333
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-around", 3));
            float share = 250f / 3f;
            float halfShare = share / 2f;
            AssertItemX(root, 0, halfShare);
            AssertItemX(root, 1, halfShare + 50 + share);
            AssertItemX(root, 2, halfShare + 100 + 2 * share);
        }

        [Fact]
        public void SpaceAround_FourItems_HalfGapsAtEdges()
        {
            // free = 400 - 200 = 200, per-item share = 200/4 = 50, half = 25
            // item0 at 25, item1 at 125, item2 at 225, item3 at 325
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-around", 4));
            AssertItemX(root, 0, 25);
            AssertItemX(root, 1, 125);
            AssertItemX(root, 2, 225);
            AssertItemX(root, 3, 325);
        }

        // ========== space-evenly ==========

        // [CSS-FLEXBOX 8.2] justify-content: space-evenly - equal gaps between items and at edges
        [Fact]
        public void SpaceEvenly_TwoItems_EqualGaps()
        {
            // free = 400 - 100 = 300, gaps = 3, each gap = 100
            // item0 at 100, item1 at 250
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-evenly", 2));
            AssertItemX(root, 0, 100);
            AssertItemX(root, 1, 250);
        }

        [Fact]
        public void SpaceEvenly_ThreeItems_EqualGaps()
        {
            // free = 400 - 150 = 250, gaps = 4, each gap = 62.5
            // item0 at 62.5, item1 at 175, item2 at 287.5
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-evenly", 3));
            AssertItemX(root, 0, 62.5f);
            AssertItemX(root, 1, 175);
            AssertItemX(root, 2, 287.5f);
        }

        [Fact]
        public void SpaceEvenly_FourItems_EqualGaps()
        {
            // free = 400 - 200 = 200, gaps = 5, each gap = 40
            // item0 at 40, item1 at 130, item2 at 220, item3 at 310
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-evenly", 4));
            AssertItemX(root, 0, 40);
            AssertItemX(root, 1, 130);
            AssertItemX(root, 2, 220);
            AssertItemX(root, 3, 310);
        }

        // ========== column flex-start ==========

        // [CSS-FLEXBOX 8.2] flex-direction:column justify-content: flex-start
        [Fact]
        public void Column_FlexStart_TwoItems_PackedAtTop()
        {
            var root = LayoutTestHelper.Layout(BuildColumnHtml("flex-start", 2));
            AssertItemY(root, 0, 0);
            AssertItemY(root, 1, 50);
        }

        // ========== column flex-end ==========

        // [CSS-FLEXBOX 8.2] flex-direction:column justify-content: flex-end
        [Fact]
        public void Column_FlexEnd_TwoItems_PackedAtBottom()
        {
            // free = 400 - 100 = 300
            var root = LayoutTestHelper.Layout(BuildColumnHtml("flex-end", 2));
            AssertItemY(root, 0, 300);
            AssertItemY(root, 1, 350);
        }

        // ========== column center ==========

        // [CSS-FLEXBOX 8.2] flex-direction:column justify-content: center
        [Fact]
        public void Column_Center_TwoItems_Centered()
        {
            // free = 400 - 100 = 300, offset = 150
            var root = LayoutTestHelper.Layout(BuildColumnHtml("center", 2));
            AssertItemY(root, 0, 150);
            AssertItemY(root, 1, 200);
        }

        // ========== column space-between ==========

        // [CSS-FLEXBOX 8.2] flex-direction:column justify-content: space-between
        [Fact]
        public void Column_SpaceBetween_TwoItems_TopAndBottom()
        {
            // free = 400 - 100 = 300, gap = 300
            var root = LayoutTestHelper.Layout(BuildColumnHtml("space-between", 2));
            AssertItemY(root, 0, 0);
            AssertItemY(root, 1, 350);
        }

        // ========== column space-around ==========

        // [CSS-FLEXBOX 8.2] flex-direction:column justify-content: space-around
        [Fact]
        public void Column_SpaceAround_TwoItems_HalfGapsAtEdges()
        {
            // free = 400 - 100 = 300, per-item share = 150, half = 75
            // item0 at 75, item1 at 75 + 50 + 150 = 275
            var root = LayoutTestHelper.Layout(BuildColumnHtml("space-around", 2));
            AssertItemY(root, 0, 75);
            AssertItemY(root, 1, 275);
        }

        // ========== column space-evenly ==========

        // [CSS-FLEXBOX 8.2] flex-direction:column justify-content: space-evenly
        [Fact]
        public void Column_SpaceEvenly_TwoItems_EqualGaps()
        {
            // free = 400 - 100 = 300, gaps = 3, each gap = 100
            // item0 at 100, item1 at 250
            var root = LayoutTestHelper.Layout(BuildColumnHtml("space-evenly", 2));
            AssertItemY(root, 0, 100);
            AssertItemY(root, 1, 250);
        }

        // ========== space-between single item fallback ==========

        // [CSS-FLEXBOX 8.2] space-between with 1 item falls back to flex-start
        [Fact]
        public void SpaceBetween_OneItem_FallsBackToFlexStart()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 1));
            AssertItemX(root, 0, 0);
        }

        // [CSS-FLEXBOX 8.2] space-around with 1 item centers (half-gap = free/2)
        [Fact]
        public void SpaceAround_OneItem_Centered()
        {
            // free = 400 - 50 = 350, half-gap = 175
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-around", 1));
            AssertItemX(root, 0, 175);
        }

        // [CSS-FLEXBOX 8.2] space-evenly with 1 item centers (gap = free/2)
        [Fact]
        public void SpaceEvenly_OneItem_Centered()
        {
            // free = 400 - 50 = 350, gaps = 2, each gap = 175
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-evenly", 1));
            AssertItemX(root, 0, 175);
        }

        // ========== column flex-start three items ==========

        // [CSS-FLEXBOX 8.2] flex-direction:column flex-start with 3 items
        [Fact]
        public void Column_FlexStart_ThreeItems_PackedAtTop()
        {
            var root = LayoutTestHelper.Layout(BuildColumnHtml("flex-start", 3));
            AssertItemY(root, 0, 0);
            AssertItemY(root, 1, 50);
            AssertItemY(root, 2, 100);
        }

        // ========== column flex-end three items ==========

        // [CSS-FLEXBOX 8.2] flex-direction:column flex-end with 3 items
        [Fact]
        public void Column_FlexEnd_ThreeItems_PackedAtBottom()
        {
            // free = 400 - 150 = 250
            var root = LayoutTestHelper.Layout(BuildColumnHtml("flex-end", 3));
            AssertItemY(root, 0, 250);
            AssertItemY(root, 1, 300);
            AssertItemY(root, 2, 350);
        }

        // ========== column center three items ==========

        // [CSS-FLEXBOX 8.2] flex-direction:column center with 3 items
        [Fact]
        public void Column_Center_ThreeItems_Centered()
        {
            // free = 400 - 150 = 250, offset = 125
            var root = LayoutTestHelper.Layout(BuildColumnHtml("center", 3));
            AssertItemY(root, 0, 125);
            AssertItemY(root, 1, 175);
            AssertItemY(root, 2, 225);
        }

        // ========== column space-between three items ==========

        // [CSS-FLEXBOX 8.2] flex-direction:column space-between with 3 items
        [Fact]
        public void Column_SpaceBetween_ThreeItems_EvenlySpaced()
        {
            // free = 400 - 150 = 250, gap = 250/2 = 125
            var root = LayoutTestHelper.Layout(BuildColumnHtml("space-between", 3));
            AssertItemY(root, 0, 0);
            AssertItemY(root, 1, 175);
            AssertItemY(root, 2, 350);
        }
    }
}
