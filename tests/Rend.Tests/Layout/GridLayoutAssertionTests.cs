using Rend.Layout.Internal;
using Rend.Style;
using Xunit;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Layout-level tests for CSS Grid.
    /// </summary>
    public class GridLayoutAssertionTests
    {
        [Fact]
        public void Grid_TwoColumns_EqualWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: 1fr 1fr; width: 200px;'>
                    <div id='a' style='height: 30px;'></div>
                    <div id='b' style='height: 30px;'></div>
                </div></body>");

            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            Assert.True(System.Math.Abs(a!.ContentRect.Width - 100) < 2,
                $"A should be ~100px wide (got {a.ContentRect.Width})");
            Assert.True(System.Math.Abs(b!.ContentRect.Width - 100) < 2,
                $"B should be ~100px wide (got {b.ContentRect.Width})");
        }

        [Fact]
        public void Grid_ExplicitPlacement()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: 100px 100px; grid-template-rows: 50px 50px; width: 200px;'>
                    <div id='a' style='grid-column: 2; grid-row: 1;'></div>
                    <div id='b' style='grid-column: 1; grid-row: 2;'></div>
                </div></body>");

            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            // A is in column 2 (X >= 100), row 1 (Y near top)
            Assert.True(a!.ContentRect.X >= 99, $"A should be in column 2 (X={a.ContentRect.X})");
            // B is in column 1 (X near 0), row 2 (Y >= 50)
            Assert.True(b!.ContentRect.X < 10, $"B should be in column 1 (X={b.ContentRect.X})");
            Assert.True(b!.ContentRect.Y >= 49, $"B should be in row 2 (Y={b.ContentRect.Y})");
        }

        [Fact]
        public void Grid_Gap_AddsSpacing()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 20px; width: 220px;'>
                    <div id='a' style='height: 30px;'></div>
                    <div id='b' style='height: 30px;'></div>
                </div></body>");

            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            float gap = b!.ContentRect.X - (a!.ContentRect.X + a.ContentRect.Width);
            Assert.True(gap >= 18 && gap <= 22,
                $"Gap should be ~20px (got {gap})");
        }

        [Fact]
        public void Grid_AutoRows_SizesImplicitTracks()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: 100px; grid-auto-rows: 40px; width: 100px;'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");

            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            var c = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(a);
            Assert.NotNull(b);
            Assert.NotNull(c);
            Assert.True(System.Math.Abs(a!.ContentRect.Height - 40) < 2,
                $"Auto row height should be 40px (got {a.ContentRect.Height})");
            Assert.True(System.Math.Abs(b!.ContentRect.Height - 40) < 2,
                $"Auto row height should be 40px (got {b.ContentRect.Height})");
        }

        [Fact]
        public void Grid_MinMax_ClampsTracks()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: minmax(50px, 1fr) minmax(50px, 1fr); width: 80px;'>
                    <div id='a' style='height: 30px;'></div>
                    <div id='b' style='height: 30px;'></div>
                </div></body>");

            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            // Each column min is 50px but container is only 80px — still should respect min
            Assert.True(a!.ContentRect.Width >= 39,
                $"Column should respect min (got {a.ContentRect.Width})");
        }

        [Fact]
        public void Grid_AlignItems_Center()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: 200px; grid-template-rows: 100px; align-items: center; width: 200px;'>
                    <div id='item' style='height: 30px;'></div>
                </div></body>");

            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // Centered in 100px row: item Y should be offset by (100-30)/2 = 35px
            Assert.True(item!.ContentRect.Height <= 31,
                $"Item height should be 30px (got {item.ContentRect.Height})");
        }
    }
}
