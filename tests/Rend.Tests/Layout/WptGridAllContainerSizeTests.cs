using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Verifies that equal-width fr columns divide container width evenly
    /// across a matrix of container widths and column counts.
    /// [CSS-GRID §7.2.3] equal fr tracks share available space equally.
    /// </summary>
    public class WptGridAllContainerSizeTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridAllContainerSizeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private void AssertFirstItemWidth(int containerWidth, int columnCount)
        {
            float expectedWidth = (float)containerWidth / columnCount;

            string columnTemplate = string.Join(" ", System.Linq.Enumerable.Repeat("1fr", columnCount));
            string items = string.Join("",
                System.Linq.Enumerable.Range(0, columnCount)
                    .Select(index => $"<div id='item{index}' style='height:20px'></div>"));

            string html = $@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:{columnTemplate};width:{containerWidth}px'>
                    {items}
                </div></body>";

            var root = LayoutTestHelper.Layout(html);
            var firstItem = LayoutTestHelper.FindById(root, "item0");
            Assert.NotNull(firstItem);

            float actualWidth = firstItem!.ContentRect.Width;
            _output.WriteLine($"Container={containerWidth}px, Columns={columnCount}, Expected={expectedWidth}px, Actual={actualWidth}px");
            Assert.True(
                System.Math.Abs(actualWidth - expectedWidth) < 2,
                $"First item width {actualWidth}px should be ~{expectedWidth}px " +
                $"(container={containerWidth}px / {columnCount} cols)");
        }

        // --- 1 column ---

        [Fact]
        public void Width100_1Column_ItemIs100()
        {
            AssertFirstItemWidth(100, 1);
        }

        [Fact]
        public void Width150_1Column_ItemIs150()
        {
            AssertFirstItemWidth(150, 1);
        }

        [Fact]
        public void Width200_1Column_ItemIs200()
        {
            AssertFirstItemWidth(200, 1);
        }

        [Fact]
        public void Width250_1Column_ItemIs250()
        {
            AssertFirstItemWidth(250, 1);
        }

        [Fact]
        public void Width300_1Column_ItemIs300()
        {
            AssertFirstItemWidth(300, 1);
        }

        [Fact]
        public void Width350_1Column_ItemIs350()
        {
            AssertFirstItemWidth(350, 1);
        }

        [Fact]
        public void Width400_1Column_ItemIs400()
        {
            AssertFirstItemWidth(400, 1);
        }

        [Fact]
        public void Width450_1Column_ItemIs450()
        {
            AssertFirstItemWidth(450, 1);
        }

        [Fact]
        public void Width500_1Column_ItemIs500()
        {
            AssertFirstItemWidth(500, 1);
        }

        [Fact]
        public void Width600_1Column_ItemIs600()
        {
            AssertFirstItemWidth(600, 1);
        }

        // --- 2 columns ---

        [Fact]
        public void Width100_2Columns_ItemIs50()
        {
            AssertFirstItemWidth(100, 2);
        }

        [Fact]
        public void Width150_2Columns_ItemIs75()
        {
            AssertFirstItemWidth(150, 2);
        }

        [Fact]
        public void Width200_2Columns_ItemIs100()
        {
            AssertFirstItemWidth(200, 2);
        }

        [Fact]
        public void Width250_2Columns_ItemIs125()
        {
            AssertFirstItemWidth(250, 2);
        }

        [Fact]
        public void Width300_2Columns_ItemIs150()
        {
            AssertFirstItemWidth(300, 2);
        }

        [Fact]
        public void Width350_2Columns_ItemIs175()
        {
            AssertFirstItemWidth(350, 2);
        }

        [Fact]
        public void Width400_2Columns_ItemIs200()
        {
            AssertFirstItemWidth(400, 2);
        }

        [Fact]
        public void Width450_2Columns_ItemIs225()
        {
            AssertFirstItemWidth(450, 2);
        }

        [Fact]
        public void Width500_2Columns_ItemIs250()
        {
            AssertFirstItemWidth(500, 2);
        }

        [Fact]
        public void Width600_2Columns_ItemIs300()
        {
            AssertFirstItemWidth(600, 2);
        }

        // --- 3 columns ---

        [Fact]
        public void Width100_3Columns_ItemIs33()
        {
            AssertFirstItemWidth(100, 3);
        }

        [Fact]
        public void Width150_3Columns_ItemIs50()
        {
            AssertFirstItemWidth(150, 3);
        }

        [Fact]
        public void Width200_3Columns_ItemIs66()
        {
            AssertFirstItemWidth(200, 3);
        }

        [Fact]
        public void Width250_3Columns_ItemIs83()
        {
            AssertFirstItemWidth(250, 3);
        }

        [Fact]
        public void Width300_3Columns_ItemIs100()
        {
            AssertFirstItemWidth(300, 3);
        }

        [Fact]
        public void Width350_3Columns_ItemIs116()
        {
            AssertFirstItemWidth(350, 3);
        }

        [Fact]
        public void Width400_3Columns_ItemIs133()
        {
            AssertFirstItemWidth(400, 3);
        }

        [Fact]
        public void Width450_3Columns_ItemIs150()
        {
            AssertFirstItemWidth(450, 3);
        }

        [Fact]
        public void Width500_3Columns_ItemIs166()
        {
            AssertFirstItemWidth(500, 3);
        }

        [Fact]
        public void Width600_3Columns_ItemIs200()
        {
            AssertFirstItemWidth(600, 3);
        }

        // --- 4 columns ---

        [Fact]
        public void Width100_4Columns_ItemIs25()
        {
            AssertFirstItemWidth(100, 4);
        }

        [Fact]
        public void Width150_4Columns_ItemIs37()
        {
            AssertFirstItemWidth(150, 4);
        }

        [Fact]
        public void Width200_4Columns_ItemIs50()
        {
            AssertFirstItemWidth(200, 4);
        }

        [Fact]
        public void Width250_4Columns_ItemIs62()
        {
            AssertFirstItemWidth(250, 4);
        }

        [Fact]
        public void Width300_4Columns_ItemIs75()
        {
            AssertFirstItemWidth(300, 4);
        }

        [Fact]
        public void Width350_4Columns_ItemIs87()
        {
            AssertFirstItemWidth(350, 4);
        }

        [Fact]
        public void Width400_4Columns_ItemIs100()
        {
            AssertFirstItemWidth(400, 4);
        }

        [Fact]
        public void Width450_4Columns_ItemIs112()
        {
            AssertFirstItemWidth(450, 4);
        }

        [Fact]
        public void Width500_4Columns_ItemIs125()
        {
            AssertFirstItemWidth(500, 4);
        }

        [Fact]
        public void Width600_4Columns_ItemIs150()
        {
            AssertFirstItemWidth(600, 4);
        }
    }
}
