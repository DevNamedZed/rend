using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// [CSS-FLEXBOX §9.7] flex-grow with min/max clamping and space redistribution.
    /// When a growing item hits max-width, remaining space redistributes to unclamped items.
    /// When min-width exceeds proportional share, item takes min-width and surplus is deducted.
    /// </summary>
    public class WptFlexGrowClampRedistributeTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexGrowClampRedistributeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.7] Two items grow:1, one clamped by max-width.
        // Container 400px, both basis:0, item A max-width:100. Free=400.
        // Round 1: each gets 200, A clamped to 100. Leftover 100 goes to B.
        // Result: A=100, B=300.
        [Fact]
        public void GrowWithMaxWidthClamp_RedistributesToOtherItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;max-width:100px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"A should be clamped to 100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 300) < 2,
                $"B should get remaining 300 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Three items grow:1, first clamped at max-width:80.
        // Container 300px, basis:0. Round 1: each 100, A clamped to 80. Leftover=20 split B/C.
        // Result: A=80, B=110, C=110.
        [Fact]
        public void ThreeItems_OneClamped_RedistributesEvenly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 0px;max-width:80px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                    <div id='c' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}, C={itemC!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2,
                $"A clamped to 80 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 110) < 2,
                $"B gets 110 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 110) < 2,
                $"C gets 110 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Grow 1:2 ratio, larger item has max-width.
        // Container 300px, basis:0. Proportional: A=100, B=200. B max-width:150.
        // B clamped to 150, leftover=50 goes to A. Result: A=150, B=150.
        [Fact]
        public void GrowRatio1To2_LargerItemMaxWidthClamped()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:2 0 0px;max-width:150px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"B clamped to 150 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"A gets remaining 150 (got {itemA.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] min-width enforced: item gets at least min-width.
        // Container 200px, two items grow:1 basis:0. A min-width:150.
        // Proportional: each 100. A forced to 150. B gets 50.
        [Fact]
        public void GrowWithMinWidthEnforced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:1 0 0px;min-width:150px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"A min-width enforced at 150 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 50) < 2,
                $"B gets remaining 50 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] min-width larger than proportional share.
        // Container 300px, three items grow:1. A min-width:200.
        // Proportional: each 100. A gets 200, remaining 100 split B/C = 50 each.
        [Fact]
        public void MinWidthLargerThanProportionalShare()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 0px;min-width:200px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                    <div id='c' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}, C={itemC!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"A min-width 200 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 50) < 2,
                $"B gets 50 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 50) < 2,
                $"C gets 50 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] max-width smaller than proportional share.
        // Container 400px, two items grow:1 basis:0. A max-width:50.
        // Proportional: each 200. A clamped to 50. B gets 350.
        [Fact]
        public void MaxWidthSmallerThanProportionalShare()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;max-width:50px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 2,
                $"A clamped to 50 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 350) < 2,
                $"B gets 350 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] All items max-width clamped, leftover space remains.
        // Container 400px, two items grow:1 basis:0, both max-width:100.
        // Each clamped to 100. 200px free space unabsorbed.
        [Fact]
        public void AllItemsMaxWidthClamped_FreeSpaceRemains()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;max-width:100px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;max-width:100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"A clamped to 100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"B clamped to 100 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] No free space, grow has no effect.
        // Container 200px, two items basis:100 each, grow:1. No free space.
        [Fact]
        public void NoFreeSpace_GrowHasNoEffect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:1 0 100px;height:30px'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"A stays at 100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"B stays at 100 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Exact free space fills exactly.
        // Container 300px, two items basis:100, grow:1. Free=100, each gets +50.
        [Fact]
        public void ExactFreeSpace_FillsExactly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 100px;height:30px'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"A grows to 150 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"B grows to 150 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Grow with basis + max-width: basis 80, grow:1, max-width:120.
        // Container 400px, two items. Free = 400-160=240, each +120. A clamped at 120.
        // Leftover=80-0=80 goes to B. Result: A=120, B=280.
        [Fact]
        public void GrowWithBasisAndMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 80px;max-width:120px;height:30px'></div>
                    <div id='b' style='flex:1 0 80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 120) < 2,
                $"A clamped to max-width 120 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 280) < 2,
                $"B gets 280 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Grow with basis + min-width: basis 50, grow:1, min-width:180.
        // Container 300px, two items basis:50. Free=200. Each +100. A=150 but min=180.
        // A gets 180, B gets 120.
        [Fact]
        public void GrowWithBasisAndMinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 50px;min-width:180px;height:30px'></div>
                    <div id='b' style='flex:1 0 50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 180) < 2,
                $"A min-width enforced at 180 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 120) < 2,
                $"B gets 120 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Column direction: grow with max-height clamp.
        // Container height:300, two items grow:1 basis:0, A max-height:80.
        // Proportional: each 150. A clamped to 80, B gets 220.
        [Fact]
        public void ColumnGrow_MaxHeightClamp()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex:1 0 0px;max-height:80px'></div>
                    <div id='b' style='flex:1 0 0px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A.H={itemA!.ContentRect.Height}, B.H={itemB!.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 80) < 2,
                $"A clamped to max-height 80 (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 220) < 2,
                $"B gets 220 (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] Column direction: grow with min-height enforced.
        // Container height:200, two items grow:1 basis:0. A min-height:150.
        // Proportional: each 100. A gets 150, B gets 50.
        [Fact]
        public void ColumnGrow_MinHeightEnforced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:100px'>
                    <div id='a' style='flex:1 0 0px;min-height:150px'></div>
                    <div id='b' style='flex:1 0 0px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A.H={itemA!.ContentRect.Height}, B.H={itemB!.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 150) < 2,
                $"A min-height enforced at 150 (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 50) < 2,
                $"B gets 50 (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] Fractional flex-grow: 0.5 each.
        // Container 300px, two items basis:100, grow:0.5. Free=100, total grow=1.0.
        // Each gets 0.5/1.0 * 100 = 50. Result: A=150, B=150.
        [Fact]
        public void GrowFractional_HalfEach()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0.5 0 100px;height:30px'></div>
                    <div id='b' style='flex:0.5 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"A grows to 150 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"B grows to 150 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Grow with gap reduces free space.
        // Container 400px, gap:20px, two items grow:1 basis:0.
        // Free = 400 - 20 = 380. Each gets 190.
        [Fact]
        public void GrowWithGap_ReducesFreeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;gap:20px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 190) < 2,
                $"A gets 190 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 190) < 2,
                $"B gets 190 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Large ratio 1:99. Container 400px, basis:0.
        // A = 400/100 = 4. B = 400*99/100 = 396.
        [Fact]
        public void GrowLargeRatio_1To99()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:99 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 4) < 2,
                $"A gets 4 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 396) < 2,
                $"B gets 396 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Five equal grow items.
        // Container 500px, all grow:1 basis:0. Each gets 100.
        [Fact]
        public void FiveEqualGrowItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                    <div id='c' style='flex:1 0 0px;height:30px'></div>
                    <div id='d' style='flex:1 0 0px;height:30px'></div>
                    <div id='e' style='flex:1 0 0px;height:30px'></div>
                </div></body>", viewportWidth: 600);
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemC = LayoutTestHelper.FindById(root, "c");
            var itemE = LayoutTestHelper.FindById(root, "e");
            Assert.NotNull(itemA);
            Assert.NotNull(itemC);
            Assert.NotNull(itemE);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, C={itemC!.ContentRect.Width}, E={itemE!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"A gets 100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2,
                $"C gets 100 (got {itemC.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemE.ContentRect.Width - 100) < 2,
                $"E gets 100 (got {itemE.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Grow with padding: padding counted in outer hypothetical size.
        // Container 400px, two items grow:1 basis:0. A has 20px padding each side (40px total).
        // A outer basis = 0+40 = 40. B outer basis = 0. Free = 400-40 = 360.
        // Each grows by 360/2 = 180 content. A content=180, B content=180.
        [Fact]
        public void GrowWithPadding_PaddingCountedInOuterSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;padding:0 20px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A.content={itemA!.ContentRect.Width}, B.content={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 180) < 2,
                $"A content 180 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 180) < 2,
                $"B content 180 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Grow with border: border counted in outer hypothetical size.
        // Container 400px, two items grow:1 basis:0. A has 10px border each side (20px total).
        // A outer basis = 0+20 = 20. Free = 400-20 = 380. Each grows by 190 content.
        [Fact]
        public void GrowWithBorder_BorderCountedInOuterSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;border:10px solid black;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A.content={itemA!.ContentRect.Width}, B.content={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 190) < 2,
                $"A content 190 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 190) < 2,
                $"B content 190 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Grow with margin: margin consumed from free space.
        // Container 400px, two items grow:1 basis:0. A margin:0 20px (40px total).
        // Margins reduce free space: 400-40 = 360. Each grows by 180 content.
        [Fact]
        public void GrowWithMargin_MarginReducesFreeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;margin:0 20px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A.content={itemA!.ContentRect.Width}, B.content={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 180) < 2,
                $"A content 180 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 180) < 2,
                $"B content 180 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Grow with box-sizing:border-box and max-width.
        // Container 400px, A grow:1 basis:0, border-box padding:0 20px, max-width:150.
        // A outer basis=40 (padding). Free=360. Proportional each=180. A content clamped to 150
        // (max-width border-box 150 - 40 padding = 110 content max, but flex clamps content to 150).
        // B gets remaining: 400 - (150+40) = 210.
        [Fact]
        public void GrowWithBorderBox_MaxWidthClampedWithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;box-sizing:border-box;padding:0 20px;max-width:150px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A.content={itemA!.ContentRect.Width}, B.content={itemB!.ContentRect.Width}");
            // A content is clamped (max-width applies), B gets the rest
            float totalContentUsed = itemA.ContentRect.Width + itemB.ContentRect.Width;
            Assert.True(System.Math.Abs(totalContentUsed - 360) < 2,
                $"Total content should be 360 (400 minus 40 padding) (got {totalContentUsed})");
            Assert.True(itemA.ContentRect.Width <= 152,
                $"A content should be clamped by max-width (got {itemA.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Grow 1:1 with different basis.
        // Container 400px. A basis:100, B basis:200. Free=100. Each +50.
        // A=150, B=250.
        [Fact]
        public void GrowEqual_DifferentBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 100px;height:30px'></div>
                    <div id='b' style='flex:1 0 200px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"A=150 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 250) < 2,
                $"B=250 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Grow 2:3 ratio from zero basis.
        // Container 500px. A=500*2/5=200. B=500*3/5=300.
        [Fact]
        public void GrowRatio2To3()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='a' style='flex:2 0 0px;height:30px'></div>
                    <div id='b' style='flex:3 0 0px;height:30px'></div>
                </div></body>", viewportWidth: 600);
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"A=200 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 300) < 2,
                $"B=300 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Two items clamped in sequence: multi-pass redistribution.
        // Container 600px, 3 items grow:1 basis:0. Proportional: each 200.
        // A max-width:100, B max-width:150. A clamped first, leftover=100 split B/C.
        // B would get 250, clamped to 150. Leftover=100 to C. Result: A=100, B=150, C=350.
        [Fact]
        public void TwoItemsClamped_MultiPassRedistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1 0 0px;max-width:100px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;max-width:150px;height:30px'></div>
                    <div id='c' style='flex:1 0 0px;height:30px'></div>
                </div></body>", viewportWidth: 700);
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}, C={itemC!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"A clamped to 100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"B clamped to 150 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 350) < 2,
                $"C gets 350 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Grow with gap and max-width clamp.
        // Container 400px, gap:20px, two items grow:1 basis:0. A max-width:100.
        // Free=380. Proportional: each 190. A clamped to 100. B gets 280.
        [Fact]
        public void GrowWithGapAndMaxWidthClamp()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;gap:20px'>
                    <div id='a' style='flex:1 0 0px;max-width:100px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"A={itemA!.ContentRect.Width}, B={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"A clamped to 100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 280) < 2,
                $"B gets 280 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Single item with grow and max-width.
        // Container 400px. Single item grow:1 basis:0 max-width:200.
        // Item clamped to 200, 200px unabsorbed.
        [Fact]
        public void SingleItem_GrowClampedByMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex:1 0 0px;max-width:200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"t={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"Item clamped to 200 (got {item.ContentRect.Width})");
        }
    }
}
