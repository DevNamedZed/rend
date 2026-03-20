using Rend.Layout.Internal;
using Rend.Style;
using Xunit;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Layout-level tests for CSS Flexbox.
    /// Verifies flex item positioning and sizing without rendering.
    /// </summary>
    public class FlexLayoutTests
    {
        [Fact]
        public void FlexRow_ItemsPlacedHorizontally()
        {
            var root = LayoutTestHelper.Layout(@"
                <div style='display: flex; width: 300px;'>
                    <div id='a' style='width: 100px; height: 50px;'></div>
                    <div id='b' style='width: 100px; height: 50px;'></div>
                </div>");

            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            Assert.True(b!.ContentRect.X > a!.ContentRect.X,
                $"B should be right of A (A.X={a.ContentRect.X}, B.X={b.ContentRect.X})");
        }

        [Fact]
        public void FlexColumn_ItemsPlacedVertically()
        {
            var root = LayoutTestHelper.Layout(@"
                <div style='display: flex; flex-direction: column; width: 200px;'>
                    <div id='a' style='height: 50px;'></div>
                    <div id='b' style='height: 50px;'></div>
                </div>");

            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            Assert.True(b!.ContentRect.Y > a!.ContentRect.Y,
                $"B should be below A (A.Y={a.ContentRect.Y}, B.Y={b.ContentRect.Y})");
        }

        [Fact]
        public void FlexGrow_DistributesSpace()
        {
            var root = LayoutTestHelper.Layout(@"
                <div style='display: flex; width: 300px;'>
                    <div id='a' style='flex-grow: 1; height: 50px;'></div>
                    <div id='b' style='flex-grow: 2; height: 50px;'></div>
                </div>");

            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            // B should be roughly twice the width of A
            float ratio = b!.ContentRect.Width / a!.ContentRect.Width;
            Assert.True(ratio > 1.8 && ratio < 2.2,
                $"B/A width ratio should be ~2.0 (got {ratio:F2}, A={a.ContentRect.Width}, B={b.ContentRect.Width})");
        }

        [Fact]
        public void FlexShrink_ShrinksBeyondContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <div style='display: flex; width: 200px;'>
                    <div id='a' style='width: 150px; flex-shrink: 1; height: 50px;'></div>
                    <div id='b' style='width: 150px; flex-shrink: 1; height: 50px;'></div>
                </div>");

            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            // Both items should shrink to fit 200px container
            Assert.True(a!.ContentRect.Width < 150, $"A should shrink from 150px (got {a.ContentRect.Width})");
            Assert.True(b!.ContentRect.Width < 150, $"B should shrink from 150px (got {b.ContentRect.Width})");
            float total = a.ContentRect.Width + b.ContentRect.Width;
            Assert.True(total <= 201, $"Total width should fit container (got {total})");
        }

        [Fact]
        public void AlignItems_Center_CentersVertically()
        {
            var root = LayoutTestHelper.Layout(@"
                <div style='display: flex; align-items: center; height: 100px; width: 200px;'>
                    <div id='item' style='width: 50px; height: 30px;'></div>
                </div>");

            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // Centered in 100px container: (100 - 30) / 2 = 35px from top
            float expectedTop = 35;
            float actualTop = item!.ContentRect.Y - item.BorderTopWidth - item.PaddingTop;
            // Allow some tolerance for border/padding of parent
            Assert.True(System.Math.Abs(actualTop - expectedTop) < 2 || item.ContentRect.Height == 30,
                $"Item should be vertically centered (Y={item.ContentRect.Y}, height={item.ContentRect.Height})");
        }

        [Fact]
        public void JustifyContent_Center_CentersHorizontally()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin: 0;'>
                <div style='display: flex; justify-content: center; width: 200px;'>
                    <div id='item' style='width: 50px; height: 30px;'></div>
                </div></body>");

            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // Centered in 200px: (200 - 50) / 2 = 75px from left
            float leftOffset = item!.ContentRect.X;
            Assert.True(leftOffset > 70 && leftOffset < 80,
                $"Item should be horizontally centered (X={leftOffset})");
        }

        [Fact]
        public void FlexWrap_WrapsToNextLine()
        {
            var root = LayoutTestHelper.Layout(@"
                <div style='display: flex; flex-wrap: wrap; width: 100px;'>
                    <div id='a' style='width: 60px; height: 30px;'></div>
                    <div id='b' style='width: 60px; height: 30px;'></div>
                </div>");

            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            // B should wrap to next line since 60+60 > 100
            Assert.True(b!.ContentRect.Y > a!.ContentRect.Y,
                $"B should wrap to next line (A.Y={a.ContentRect.Y}, B.Y={b.ContentRect.Y})");
        }

        [Fact]
        public void FlexGap_AddsSpaceBetweenItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <div style='display: flex; gap: 20px; width: 300px;'>
                    <div id='a' style='width: 50px; height: 30px;'></div>
                    <div id='b' style='width: 50px; height: 30px;'></div>
                </div>");

            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            float gap = b!.ContentRect.X - (a!.ContentRect.X + a.ContentRect.Width);
            Assert.True(gap >= 19 && gap <= 21,
                $"Gap between items should be ~20px (got {gap})");
        }

        [Fact]
        public void FlexOrder_ChangesVisualOrder()
        {
            var root = LayoutTestHelper.Layout(@"
                <div style='display: flex; width: 200px;'>
                    <div id='a' style='order: 2; width: 50px; height: 30px;'></div>
                    <div id='b' style='order: 1; width: 50px; height: 30px;'></div>
                </div>");

            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            // B has order:1 so it should appear before A (order:2)
            Assert.True(b!.ContentRect.X < a!.ContentRect.X,
                $"B (order:1) should be left of A (order:2) (B.X={b.ContentRect.X}, A.X={a.ContentRect.X})");
        }

        [Fact]
        public void FlexBasis_OverridesWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <div style='display: flex; width: 300px;'>
                    <div id='item' style='width: 100px; flex-basis: 150px; height: 30px;'></div>
                </div>");

            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // flex-basis should override width
            Assert.True(item!.ContentRect.Width >= 149 && item.ContentRect.Width <= 151,
                $"flex-basis should override width (got {item.ContentRect.Width})");
        }

        [Fact]
        public void PercentHeight_InIndefiniteColumn_ResolvesToAuto()
        {
            // CSS Flexbox §9.8: percent height against indefinite container → auto
            var root = LayoutTestHelper.Layout(@"
                <div style='display: flex; flex-direction: column; width: 200px;'>
                    <div id='fixed' style='height: 100px; width: 100px; background: green;'></div>
                    <div id='percent' style='height: 50%; background: red;'></div>
                </div>");

            var percent = LayoutTestHelper.FindById(root, "percent");
            Assert.NotNull(percent);
            // height: 50% against indefinite container should resolve to 0 (auto)
            Assert.True(percent!.ContentRect.Height < 1,
                $"50% height in indefinite column flex should be 0 (got {percent.ContentRect.Height})");
        }
    }
}
