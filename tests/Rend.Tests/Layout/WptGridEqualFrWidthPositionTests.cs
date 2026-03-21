using System.Linq;
using Xunit;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// [CSS-GRID §7.2.3] Equal fr track distribution: verifies that N columns of 1fr
    /// each receive exactly 1/N of the container width and that items are positioned
    /// at the correct X offsets across a range of container sizes.
    /// </summary>
    public class WptGridEqualFrWidthPositionTests
    {
        private const float Tolerance = 1f;

        private static string BuildGridHtml(int columnCount, float containerWidth)
        {
            var frColumns = string.Join(" ", System.Linq.Enumerable.Repeat("1fr", columnCount));
            var items = string.Concat(
                System.Linq.Enumerable.Range(0, columnCount)
                    .Select(index => $"<div id='item{index}' style='height:20px'></div>"));

            return $@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:{frColumns};width:{containerWidth}px'>
                    {items}
                </div></body>";
        }

        private static void AssertFirstAndLastItem(int columnCount, float containerWidth)
        {
            var root = LayoutTestHelper.Layout(BuildGridHtml(columnCount, containerWidth));
            float expectedWidth = containerWidth / columnCount;
            float lastItemIndex = columnCount - 1;
            float expectedLastX = expectedWidth * lastItemIndex;

            var firstItem = LayoutTestHelper.FindById(root, "item0");
            var lastItem = LayoutTestHelper.FindById(root, $"item{lastItemIndex}");

            Assert.NotNull(firstItem);
            Assert.NotNull(lastItem);

            Assert.True(
                System.Math.Abs(firstItem!.ContentRect.Width - expectedWidth) < Tolerance,
                $"First item width: expected {expectedWidth}, got {firstItem.ContentRect.Width}");
            Assert.True(
                System.Math.Abs(firstItem.ContentRect.X) < Tolerance,
                $"First item X: expected 0, got {firstItem.ContentRect.X}");

            Assert.True(
                System.Math.Abs(lastItem!.ContentRect.Width - expectedWidth) < Tolerance,
                $"Last item width: expected {expectedWidth}, got {lastItem.ContentRect.Width}");
            Assert.True(
                System.Math.Abs(lastItem.ContentRect.X - expectedLastX) < Tolerance,
                $"Last item X: expected {expectedLastX}, got {lastItem.ContentRect.X}");
        }

        // ===== 1 column =====

        [Fact]
        public void OneColumn_100px_WidthAndPosition()
        {
            AssertFirstAndLastItem(1, 100);
        }

        [Fact]
        public void OneColumn_200px_WidthAndPosition()
        {
            AssertFirstAndLastItem(1, 200);
        }

        [Fact]
        public void OneColumn_300px_WidthAndPosition()
        {
            AssertFirstAndLastItem(1, 300);
        }

        [Fact]
        public void OneColumn_400px_WidthAndPosition()
        {
            AssertFirstAndLastItem(1, 400);
        }

        [Fact]
        public void OneColumn_500px_WidthAndPosition()
        {
            AssertFirstAndLastItem(1, 500);
        }

        [Fact]
        public void OneColumn_600px_WidthAndPosition()
        {
            AssertFirstAndLastItem(1, 600);
        }

        // ===== 2 columns =====

        [Fact]
        public void TwoColumns_100px_WidthAndPosition()
        {
            AssertFirstAndLastItem(2, 100);
        }

        [Fact]
        public void TwoColumns_200px_WidthAndPosition()
        {
            AssertFirstAndLastItem(2, 200);
        }

        [Fact]
        public void TwoColumns_300px_WidthAndPosition()
        {
            AssertFirstAndLastItem(2, 300);
        }

        [Fact]
        public void TwoColumns_400px_WidthAndPosition()
        {
            AssertFirstAndLastItem(2, 400);
        }

        [Fact]
        public void TwoColumns_500px_WidthAndPosition()
        {
            AssertFirstAndLastItem(2, 500);
        }

        [Fact]
        public void TwoColumns_600px_WidthAndPosition()
        {
            AssertFirstAndLastItem(2, 600);
        }

        // ===== 3 columns =====

        [Fact]
        public void ThreeColumns_150px_WidthAndPosition()
        {
            AssertFirstAndLastItem(3, 150);
        }

        [Fact]
        public void ThreeColumns_300px_WidthAndPosition()
        {
            AssertFirstAndLastItem(3, 300);
        }

        [Fact]
        public void ThreeColumns_450px_WidthAndPosition()
        {
            AssertFirstAndLastItem(3, 450);
        }

        [Fact]
        public void ThreeColumns_600px_WidthAndPosition()
        {
            AssertFirstAndLastItem(3, 600);
        }

        // ===== 4 columns =====

        [Fact]
        public void FourColumns_200px_WidthAndPosition()
        {
            AssertFirstAndLastItem(4, 200);
        }

        [Fact]
        public void FourColumns_400px_WidthAndPosition()
        {
            AssertFirstAndLastItem(4, 400);
        }

        [Fact]
        public void FourColumns_600px_WidthAndPosition()
        {
            AssertFirstAndLastItem(4, 600);
        }

        // ===== 5 columns =====

        [Fact]
        public void FiveColumns_250px_WidthAndPosition()
        {
            AssertFirstAndLastItem(5, 250);
        }

        [Fact]
        public void FiveColumns_500px_WidthAndPosition()
        {
            AssertFirstAndLastItem(5, 500);
        }

        // ===== 6 columns =====

        [Fact]
        public void SixColumns_300px_WidthAndPosition()
        {
            AssertFirstAndLastItem(6, 300);
        }

        [Fact]
        public void SixColumns_600px_WidthAndPosition()
        {
            AssertFirstAndLastItem(6, 600);
        }

        // ===== Additional middle-item position checks =====

        [Fact]
        public void ThreeColumns_300px_MiddleItemPosition()
        {
            var root = LayoutTestHelper.Layout(BuildGridHtml(3, 300));
            var middleItem = LayoutTestHelper.FindById(root, "item1");
            Assert.NotNull(middleItem);
            Assert.True(
                System.Math.Abs(middleItem!.ContentRect.X - 100) < Tolerance,
                $"Middle item X: expected 100, got {middleItem.ContentRect.X}");
            Assert.True(
                System.Math.Abs(middleItem.ContentRect.Width - 100) < Tolerance,
                $"Middle item width: expected 100, got {middleItem.ContentRect.Width}");
        }

        [Fact]
        public void FourColumns_400px_SecondItemPosition()
        {
            var root = LayoutTestHelper.Layout(BuildGridHtml(4, 400));
            var secondItem = LayoutTestHelper.FindById(root, "item1");
            Assert.NotNull(secondItem);
            Assert.True(
                System.Math.Abs(secondItem!.ContentRect.X - 100) < Tolerance,
                $"Second item X: expected 100, got {secondItem.ContentRect.X}");
            Assert.True(
                System.Math.Abs(secondItem.ContentRect.Width - 100) < Tolerance,
                $"Second item width: expected 100, got {secondItem.ContentRect.Width}");
        }

        [Fact]
        public void FourColumns_400px_ThirdItemPosition()
        {
            var root = LayoutTestHelper.Layout(BuildGridHtml(4, 400));
            var thirdItem = LayoutTestHelper.FindById(root, "item2");
            Assert.NotNull(thirdItem);
            Assert.True(
                System.Math.Abs(thirdItem!.ContentRect.X - 200) < Tolerance,
                $"Third item X: expected 200, got {thirdItem.ContentRect.X}");
            Assert.True(
                System.Math.Abs(thirdItem.ContentRect.Width - 100) < Tolerance,
                $"Third item width: expected 100, got {thirdItem.ContentRect.Width}");
        }

        [Fact]
        public void SixColumns_600px_ThirdItemPosition()
        {
            var root = LayoutTestHelper.Layout(BuildGridHtml(6, 600));
            var thirdItem = LayoutTestHelper.FindById(root, "item2");
            Assert.NotNull(thirdItem);
            Assert.True(
                System.Math.Abs(thirdItem!.ContentRect.X - 200) < Tolerance,
                $"Third item X: expected 200, got {thirdItem.ContentRect.X}");
            Assert.True(
                System.Math.Abs(thirdItem.ContentRect.Width - 100) < Tolerance,
                $"Third item width: expected 100, got {thirdItem.ContentRect.Width}");
        }

        [Fact]
        public void SixColumns_600px_FifthItemPosition()
        {
            var root = LayoutTestHelper.Layout(BuildGridHtml(6, 600));
            var fifthItem = LayoutTestHelper.FindById(root, "item4");
            Assert.NotNull(fifthItem);
            Assert.True(
                System.Math.Abs(fifthItem!.ContentRect.X - 400) < Tolerance,
                $"Fifth item X: expected 400, got {fifthItem.ContentRect.X}");
            Assert.True(
                System.Math.Abs(fifthItem.ContentRect.Width - 100) < Tolerance,
                $"Fifth item width: expected 100, got {fifthItem.ContentRect.Width}");
        }

        [Fact]
        public void FiveColumns_500px_MiddleItemPosition()
        {
            var root = LayoutTestHelper.Layout(BuildGridHtml(5, 500));
            var middleItem = LayoutTestHelper.FindById(root, "item2");
            Assert.NotNull(middleItem);
            Assert.True(
                System.Math.Abs(middleItem!.ContentRect.X - 200) < Tolerance,
                $"Middle item X: expected 200, got {middleItem.ContentRect.X}");
            Assert.True(
                System.Math.Abs(middleItem.ContentRect.Width - 100) < Tolerance,
                $"Middle item width: expected 100, got {middleItem.ContentRect.Width}");
        }

        [Fact]
        public void FiveColumns_500px_FourthItemPosition()
        {
            var root = LayoutTestHelper.Layout(BuildGridHtml(5, 500));
            var fourthItem = LayoutTestHelper.FindById(root, "item3");
            Assert.NotNull(fourthItem);
            Assert.True(
                System.Math.Abs(fourthItem!.ContentRect.X - 300) < Tolerance,
                $"Fourth item X: expected 300, got {fourthItem.ContentRect.X}");
            Assert.True(
                System.Math.Abs(fourthItem.ContentRect.Width - 100) < Tolerance,
                $"Fourth item width: expected 100, got {fourthItem.ContentRect.Width}");
        }

        [Fact]
        public void TwoColumns_500px_SecondItemWidth250()
        {
            var root = LayoutTestHelper.Layout(BuildGridHtml(2, 500));
            var secondItem = LayoutTestHelper.FindById(root, "item1");
            Assert.NotNull(secondItem);
            Assert.True(
                System.Math.Abs(secondItem!.ContentRect.X - 250) < Tolerance,
                $"Second item X: expected 250, got {secondItem.ContentRect.X}");
            Assert.True(
                System.Math.Abs(secondItem.ContentRect.Width - 250) < Tolerance,
                $"Second item width: expected 250, got {secondItem.ContentRect.Width}");
        }
    }
}
