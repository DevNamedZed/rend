using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Flexbox flex:1 equal distribution conformance tests.
    /// Verifies both computed widths and X positions for N items equally
    /// sharing container space across various container sizes.
    /// </summary>
    public class WptFlexGrowEqualWidthPositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexGrowEqualWidthPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private string BuildFlexHtml(int itemCount, int containerWidth)
        {
            var items = new System.Text.StringBuilder();
            for (int index = 0; index < itemCount; index++)
            {
                items.Append($"<div id='item{index}' style='flex:1;height:30px'></div>");
            }
            return $"<body style='margin:0'><div style='display:flex;width:{containerWidth}px'>{items}</div></body>";
        }

        // ── 1 item ──────────────────────────────────────────────────────

        // [CSS-FLEXBOX §9.7] Single flex:1 item fills 100px container
        [Fact]
        public void OneItem_Container100_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(1, 100));
            var item = LayoutTestHelper.FindById(root, "item0")!;
            _output.WriteLine($"item0: X={item.ContentRect.X}, W={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2, $"Width should be 100 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.X) < 2, $"X should be 0 (got {item.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Single flex:1 item fills 200px container
        [Fact]
        public void OneItem_Container200_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(1, 200));
            var item = LayoutTestHelper.FindById(root, "item0")!;
            _output.WriteLine($"item0: X={item.ContentRect.X}, W={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2, $"Width should be 200 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.X) < 2, $"X should be 0 (got {item.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Single flex:1 item fills 300px container
        [Fact]
        public void OneItem_Container300_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(1, 300));
            var item = LayoutTestHelper.FindById(root, "item0")!;
            _output.WriteLine($"item0: X={item.ContentRect.X}, W={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2, $"Width should be 300 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.X) < 2, $"X should be 0 (got {item.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Single flex:1 item fills 400px container
        [Fact]
        public void OneItem_Container400_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(1, 400));
            var item = LayoutTestHelper.FindById(root, "item0")!;
            _output.WriteLine($"item0: X={item.ContentRect.X}, W={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 400) < 2, $"Width should be 400 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.X) < 2, $"X should be 0 (got {item.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Single flex:1 item fills 500px container
        [Fact]
        public void OneItem_Container500_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(1, 500), 500);
            var item = LayoutTestHelper.FindById(root, "item0")!;
            _output.WriteLine($"item0: X={item.ContentRect.X}, W={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 500) < 2, $"Width should be 500 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.X) < 2, $"X should be 0 (got {item.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Single flex:1 item fills 600px container
        [Fact]
        public void OneItem_Container600_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(1, 600), 600);
            var item = LayoutTestHelper.FindById(root, "item0")!;
            _output.WriteLine($"item0: X={item.ContentRect.X}, W={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 600) < 2, $"Width should be 600 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.X) < 2, $"X should be 0 (got {item.ContentRect.X})");
        }

        // ── 2 items ─────────────────────────────────────────────────────

        // [CSS-FLEXBOX §9.7] Two flex:1 items in 100px container: 50px each
        [Fact]
        public void TwoItems_Container100_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(2, 100));
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item1")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item1: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 50) < 2, $"First width should be 50 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 50) < 2, $"Last width should be 50 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 50) < 2, $"Last X should be 50 (got {last.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Two flex:1 items in 200px container: 100px each
        [Fact]
        public void TwoItems_Container200_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(2, 200));
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item1")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item1: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 100) < 2, $"First width should be 100 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 100) < 2, $"Last width should be 100 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 100) < 2, $"Last X should be 100 (got {last.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Two flex:1 items in 300px container: 150px each
        [Fact]
        public void TwoItems_Container300_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(2, 300));
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item1")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item1: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 150) < 2, $"First width should be 150 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 150) < 2, $"Last width should be 150 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 150) < 2, $"Last X should be 150 (got {last.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Two flex:1 items in 400px container: 200px each
        [Fact]
        public void TwoItems_Container400_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(2, 400));
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item1")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item1: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 200) < 2, $"First width should be 200 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 200) < 2, $"Last width should be 200 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 200) < 2, $"Last X should be 200 (got {last.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Two flex:1 items in 500px container: 250px each
        [Fact]
        public void TwoItems_Container500_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(2, 500), 500);
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item1")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item1: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 250) < 2, $"First width should be 250 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 250) < 2, $"Last width should be 250 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 250) < 2, $"Last X should be 250 (got {last.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Two flex:1 items in 600px container: 300px each
        [Fact]
        public void TwoItems_Container600_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(2, 600), 600);
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item1")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item1: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 300) < 2, $"First width should be 300 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 300) < 2, $"Last width should be 300 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 300) < 2, $"Last X should be 300 (got {last.ContentRect.X})");
        }

        // ── 3 items ─────────────────────────────────────────────────────

        // [CSS-FLEXBOX §9.7] Three flex:1 items in 150px container: 50px each
        [Fact]
        public void ThreeItems_Container150_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(3, 150));
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item2")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item2: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 50) < 2, $"First width should be 50 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 50) < 2, $"Last width should be 50 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 100) < 2, $"Last X should be 100 (got {last.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Three flex:1 items in 300px container: 100px each
        [Fact]
        public void ThreeItems_Container300_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(3, 300));
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item2")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item2: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 100) < 2, $"First width should be 100 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 100) < 2, $"Last width should be 100 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 200) < 2, $"Last X should be 200 (got {last.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Three flex:1 items in 450px container: 150px each
        [Fact]
        public void ThreeItems_Container450_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(3, 450), 450);
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item2")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item2: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 150) < 2, $"First width should be 150 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 150) < 2, $"Last width should be 150 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 300) < 2, $"Last X should be 300 (got {last.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Three flex:1 items in 600px container: 200px each
        [Fact]
        public void ThreeItems_Container600_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(3, 600), 600);
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item2")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item2: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 200) < 2, $"First width should be 200 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 200) < 2, $"Last width should be 200 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 400) < 2, $"Last X should be 400 (got {last.ContentRect.X})");
        }

        // ── 4 items ─────────────────────────────────────────────────────

        // [CSS-FLEXBOX §9.7] Four flex:1 items in 200px container: 50px each
        [Fact]
        public void FourItems_Container200_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(4, 200));
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item3")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item3: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 50) < 2, $"First width should be 50 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 50) < 2, $"Last width should be 50 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 150) < 2, $"Last X should be 150 (got {last.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Four flex:1 items in 400px container: 100px each
        [Fact]
        public void FourItems_Container400_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(4, 400));
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item3")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item3: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 100) < 2, $"First width should be 100 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 100) < 2, $"Last width should be 100 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 300) < 2, $"Last X should be 300 (got {last.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Four flex:1 items in 600px container: 150px each
        [Fact]
        public void FourItems_Container600_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(4, 600), 600);
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item3")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item3: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 150) < 2, $"First width should be 150 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 150) < 2, $"Last width should be 150 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 450) < 2, $"Last X should be 450 (got {last.ContentRect.X})");
        }

        // ── 5 items ─────────────────────────────────────────────────────

        // [CSS-FLEXBOX §9.7] Five flex:1 items in 250px container: 50px each
        [Fact]
        public void FiveItems_Container250_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(5, 250));
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item4")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item4: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 50) < 2, $"First width should be 50 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 50) < 2, $"Last width should be 50 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 200) < 2, $"Last X should be 200 (got {last.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Five flex:1 items in 500px container: 100px each
        [Fact]
        public void FiveItems_Container500_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(5, 500), 500);
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item4")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item4: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 100) < 2, $"First width should be 100 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 100) < 2, $"Last width should be 100 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 400) < 2, $"Last X should be 400 (got {last.ContentRect.X})");
        }

        // ── 6 items ─────────────────────────────────────────────────────

        // [CSS-FLEXBOX §9.7] Six flex:1 items in 300px container: 50px each
        [Fact]
        public void SixItems_Container300_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(6, 300));
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item5")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item5: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 50) < 2, $"First width should be 50 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 50) < 2, $"Last width should be 50 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 250) < 2, $"Last X should be 250 (got {last.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Six flex:1 items in 600px container: 100px each
        [Fact]
        public void SixItems_Container600_WidthAndPosition()
        {
            var root = LayoutTestHelper.Layout(BuildFlexHtml(6, 600), 600);
            var first = LayoutTestHelper.FindById(root, "item0")!;
            var last = LayoutTestHelper.FindById(root, "item5")!;
            _output.WriteLine($"item0: X={first.ContentRect.X}, W={first.ContentRect.Width}; item5: X={last.ContentRect.X}, W={last.ContentRect.Width}");
            Assert.True(System.Math.Abs(first.ContentRect.Width - 100) < 2, $"First width should be 100 (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2, $"First X should be 0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.Width - 100) < 2, $"Last width should be 100 (got {last.ContentRect.Width})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 500) < 2, $"Last X should be 500 (got {last.ContentRect.X})");
        }
    }
}
