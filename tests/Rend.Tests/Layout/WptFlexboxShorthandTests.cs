using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS flex shorthand property behavior per CSS Flexbox specification.
    /// Verifies that flex shorthand values correctly resolve to grow/shrink/basis
    /// and produce expected layout dimensions.
    /// </summary>
    public class WptFlexboxShorthandTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxShorthandTests(ITestOutputHelper output) { _output = output; }

        // flex:1 => grow:1, shrink:1, basis:0 — single item fills container
        [Fact]
        public void FlexOne_SingleItem_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:1;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2,
                $"flex:1 single item should fill 300px container (got {item.ContentRect.Width})");
        }

        // flex:auto => grow:1, shrink:1, basis:auto — fills remaining space
        [Fact]
        public void FlexAuto_FillsRemainingSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='width:80px;height:30px'></div>
                    <div id='t' style='flex:auto;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 220) < 2,
                $"flex:auto should fill remaining 220px (got {item.ContentRect.Width})");
        }

        // flex:none => grow:0, shrink:0, basis:auto — stays at explicit width
        [Fact]
        public void FlexNone_PreservesExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:none;width:120px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"flex:none should preserve width:120px (got {item.ContentRect.Width})");
        }

        // flex:0 => grow:0, shrink:1, basis:0 — collapses to zero
        [Fact]
        public void FlexZero_CollapsesToZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:0;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={item.ContentRect.Width}");
            Assert.True(item.ContentRect.Width < 2,
                $"flex:0 should collapse to ~0px (got {item.ContentRect.Width})");
        }

        // flex:0 0 => grow:0, shrink:0, basis:0 — collapses to zero
        [Fact]
        public void FlexZeroZero_CollapsesToZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:0 0;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={item.ContentRect.Width}");
            Assert.True(item.ContentRect.Width < 2,
                $"flex:0 0 should collapse to ~0px (got {item.ContentRect.Width})");
        }

        // flex:0 0 auto => grow:0, shrink:0, basis:auto — stays at content/width
        [Fact]
        public void FlexZeroZeroAuto_UsesExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:0 0 auto;width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"flex:0 0 auto should use width:100px (got {item.ContentRect.Width})");
        }

        // flex:2 => grow:2, shrink:1, basis:0 — ratio based distribution
        [Fact]
        public void FlexTwo_RatioDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:2;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.width={itemA.ContentRect.Width}, b.width={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"flex:1 item should get 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"flex:2 item should get 200px (got {itemB.ContentRect.Width})");
        }

        // flex:1 0 100px => grow:1, shrink:0, basis:100px
        [Fact]
        public void FlexOneZero100px_GrowsFromBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={item.ContentRect.Width}");
            // basis:100px, grow:1, single item => fills to 300px
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2,
                $"flex:1 0 100px single item should fill 300px (got {item.ContentRect.Width})");
        }

        // flex:0 1 200px => grow:0, shrink:1, basis:200px — stays at basis when room
        [Fact]
        public void FlexZeroOne200px_StaysAtBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:0 1 200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={item.ContentRect.Width}");
            // grow:0, basis:200px, container:300px => stays at 200px
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"flex:0 1 200px should stay at 200px (got {item.ContentRect.Width})");
        }

        // Two items flex:1 split equally in 300px container
        [Fact]
        public void FlexOne_TwoItems_SplitEqually()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.width={itemA.ContentRect.Width}, b.width={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"first flex:1 item should be 150px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"second flex:1 item should be 150px (got {itemB.ContentRect.Width})");
        }

        // Three items flex:1 split equally in 300px container
        [Fact]
        public void FlexOne_ThreeItems_SplitEqually()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width}, c={itemC.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"first flex:1 should be 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"second flex:1 should be 100px (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2,
                $"third flex:1 should be 100px (got {itemC.ContentRect.Width})");
        }

        // Items flex:1 and flex:2 in 1:2 ratio within 300px
        [Fact]
        public void FlexOneAndTwo_OneToTwoRatio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:2;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"flex:1 should get 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"flex:2 should get 200px (got {itemB.ContentRect.Width})");
        }

        // Items flex:1, flex:2, flex:3 in 1:2:3 ratio within 300px
        [Fact]
        public void FlexOneTwoThree_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:2;height:30px'></div>
                    <div id='c' style='flex:3;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width}, c={itemC.ContentRect.Width}");
            // 300px / 6 = 50px per unit. 1:2:3 => 50, 100, 150
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 2,
                $"flex:1 should get 50px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"flex:2 should get 100px (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 150) < 2,
                $"flex:3 should get 150px (got {itemC.ContentRect.Width})");
        }

        // flex:1 with existing content — content doesn't affect width (basis:0)
        [Fact]
        public void FlexOne_WithContent_BasisZeroOverridesContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'>Short</div>
                    <div id='b' style='flex:1;height:30px'>Longer text here</div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width}");
            // Both flex:1 with basis:0 => equal split at 150px each
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"flex:1 with short text should be 150px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"flex:1 with long text should be 150px (got {itemB.ContentRect.Width})");
        }

        // flex:none preserves explicit width even with extra space
        [Fact]
        public void FlexNone_PreservesWidth_NoGrowNoShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:none;width:80px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2,
                $"flex:none should preserve 80px width (got {item.ContentRect.Width})");
        }

        // flex:auto with explicit width uses width as basis, then grows
        [Fact]
        public void FlexAuto_WithExplicitWidth_GrowsFromWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:auto;width:100px;height:30px'></div>
                    <div id='b' style='flex:auto;width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width}");
            // basis:auto => uses width. a=100, b=50, free=150. Both grow:1 => +75 each.
            // a=175, b=125
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 175) < 2,
                $"flex:auto with width:100 should be 175px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 125) < 2,
                $"flex:auto with width:50 should be 125px (got {itemB.ContentRect.Width})");
        }

        // flex:0 0 50% — percentage basis in 300px container
        [Fact]
        public void FlexZeroZero50Percent_PercentageBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:0 0 50%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"flex:0 0 50% in 300px should be 150px (got {item.ContentRect.Width})");
        }

        // flex:1 0 0px behaves same as flex:1
        [Fact]
        public void FlexOneZeroZeroPx_SameAsFlexOne()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width}");
            // Both have effectively flex:1 1 0 => equal split at 150px
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"flex:1 0 0px should be 150px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"flex:1 should be 150px (got {itemB.ContentRect.Width})");
        }

        // flex:initial => grow:0, shrink:1, basis:auto — stays at explicit width
        [Fact]
        public void FlexInitial_NoGrowShrinkable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:initial;width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"flex:initial should preserve 100px width (got {item.ContentRect.Width})");
        }

        // Mixed: flex:0 0 80px (fixed) + flex:1 (fill) in 300px container
        [Fact]
        public void Mixed_FixedPlusFill()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='fixed' style='flex:0 0 80px;height:30px'></div>
                    <div id='fill' style='flex:1;height:30px'></div>
                </div></body>");
            var fixedItem = LayoutTestHelper.FindById(root, "fixed")!;
            var fillItem = LayoutTestHelper.FindById(root, "fill")!;
            _output.WriteLine($"fixed={fixedItem.ContentRect.Width}, fill={fillItem.ContentRect.Width}");
            Assert.True(System.Math.Abs(fixedItem.ContentRect.Width - 80) < 2,
                $"fixed item should be 80px (got {fixedItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(fillItem.ContentRect.Width - 220) < 2,
                $"fill item should be 220px (got {fillItem.ContentRect.Width})");
        }

        // flex:0 0 calc(50% - 10px) — calc in basis
        [Fact]
        public void FlexZeroZeroCalc_CalcInBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:0 0 calc(50% - 10px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={item.ContentRect.Width}");
            // 50% of 300 = 150, minus 10 = 140
            Assert.True(System.Math.Abs(item.ContentRect.Width - 140) < 2,
                $"flex:0 0 calc(50% - 10px) should be 140px (got {item.ContentRect.Width})");
        }

        // Column direction: flex:1 distributes height
        [Fact]
        public void FlexOne_ColumnDirection_DistributesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex:1'></div>
                    <div id='b' style='flex:1'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.h={itemA.ContentRect.Height}, b.h={itemB.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 150) < 2,
                $"column flex:1 first item should be 150px tall (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 150) < 2,
                $"column flex:1 second item should be 150px tall (got {itemB.ContentRect.Height})");
        }

        // Column direction: three items flex:1 distribute height equally
        [Fact]
        public void FlexOne_ColumnDirection_ThreeItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex:1'></div>
                    <div id='b' style='flex:1'></div>
                    <div id='c' style='flex:1'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.h={itemA.ContentRect.Height}, b.h={itemB.ContentRect.Height}, c.h={itemC.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2,
                $"first item should be 100px tall (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 100) < 2,
                $"second item should be 100px tall (got {itemB.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Height - 100) < 2,
                $"third item should be 100px tall (got {itemC.ContentRect.Height})");
        }

        // flex:1 with min-width constraint — item cannot shrink below min-width
        [Fact]
        public void FlexOne_MinWidthConstraint()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;min-width:200px;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width}");
            // Without constraint: each 150px. With min-width:200 on A: A=200, B=100
            Assert.True(itemA.ContentRect.Width >= 198,
                $"flex:1 with min-width:200 should be >= 200px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"remaining item should get 100px (got {itemB.ContentRect.Width})");
        }

        // flex:1 with max-width constraint — item cannot grow beyond max-width
        [Fact]
        public void FlexOne_MaxWidthConstraint()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;max-width:80px;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width}");
            // A clamped to max-width:80, B gets remaining 220
            Assert.True(itemA.ContentRect.Width <= 82,
                $"flex:1 with max-width:80 should be <= 80px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 220) < 2,
                $"remaining item should get 220px (got {itemB.ContentRect.Width})");
        }

        // flex:0 0 auto without explicit width — collapses to content
        [Fact]
        public void FlexZeroZeroAuto_NoWidth_CollapsesToContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:0 0 auto;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={item.ContentRect.Width}");
            // No content, no width => collapses to 0
            Assert.True(item.ContentRect.Width < 2,
                $"flex:0 0 auto without content should collapse (got {item.ContentRect.Width})");
        }

        // Multiple fixed + multiple fill items
        [Fact]
        public void Mixed_TwoFixedTwoFill()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='f1' style='flex:0 0 50px;height:30px'></div>
                    <div id='g1' style='flex:1;height:30px'></div>
                    <div id='f2' style='flex:0 0 50px;height:30px'></div>
                    <div id='g2' style='flex:1;height:30px'></div>
                </div></body>");
            var fixed1 = LayoutTestHelper.FindById(root, "f1")!;
            var grow1 = LayoutTestHelper.FindById(root, "g1")!;
            var fixed2 = LayoutTestHelper.FindById(root, "f2")!;
            var grow2 = LayoutTestHelper.FindById(root, "g2")!;
            _output.WriteLine($"f1={fixed1.ContentRect.Width}, g1={grow1.ContentRect.Width}, f2={fixed2.ContentRect.Width}, g2={grow2.ContentRect.Width}");
            // Fixed: 50+50=100. Remaining: 300. Each grow:1 => 150 each.
            Assert.True(System.Math.Abs(fixed1.ContentRect.Width - 50) < 2,
                $"first fixed should be 50px (got {fixed1.ContentRect.Width})");
            Assert.True(System.Math.Abs(grow1.ContentRect.Width - 150) < 2,
                $"first grow should be 150px (got {grow1.ContentRect.Width})");
            Assert.True(System.Math.Abs(fixed2.ContentRect.Width - 50) < 2,
                $"second fixed should be 50px (got {fixed2.ContentRect.Width})");
            Assert.True(System.Math.Abs(grow2.ContentRect.Width - 150) < 2,
                $"second grow should be 150px (got {grow2.ContentRect.Width})");
        }

        // flex:1 items positioned sequentially (X coordinates)
        [Fact]
        public void FlexOne_ThreeItems_CorrectPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.x={itemA.ContentRect.X}, b.x={itemB.ContentRect.X}, c.x={itemC.ContentRect.X}");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2,
                $"first item X should be 0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2,
                $"second item X should be 100 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 200) < 2,
                $"third item X should be 200 (got {itemC.ContentRect.X})");
        }

        // flex:none items do not shrink even when overflowing
        [Fact]
        public void FlexNone_DoesNotShrink_EvenWhenOverflowing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:none;width:150px;height:30px'></div>
                    <div id='b' style='flex:none;width:150px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width}");
            // flex:none = 0 0 auto, items stay at 150px each even though container is 200px
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"flex:none should keep 150px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"flex:none should keep 150px (got {itemB.ContentRect.Width})");
        }

        // Column flex:auto with height uses height as basis
        [Fact]
        public void FlexAuto_ColumnDirection_UsesHeightAsBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex:auto;height:60px'></div>
                    <div id='b' style='flex:auto;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.h={itemA.ContentRect.Height}, b.h={itemB.ContentRect.Height}");
            // basis:auto => uses height. a=60, b=40, free=200. Both grow:1 => +100 each.
            // a=160, b=140
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 160) < 2,
                $"column flex:auto with height:60 should grow to 160px (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 140) < 2,
                $"column flex:auto with height:40 should grow to 140px (got {itemB.ContentRect.Height})");
        }

        // flex:1 0 100px — two items with same basis split remaining equally
        [Fact]
        public void FlexOneZeroBasis_TwoItems_GrowFromBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 100px;height:30px'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width}");
            // basis:100 each, free=100, each grows by 50 => 150 each
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"first item should be 150px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"second item should be 150px (got {itemB.ContentRect.Width})");
        }

        // flex:0 1 200px shrinks when two items overflow
        [Fact]
        public void FlexZeroOneBasis_ShrinkWhenOverflow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0 1 200px;height:30px'></div>
                    <div id='b' style='flex:0 1 200px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width}");
            // Total basis=400 in 300px. Overflow=100. Both shrink:1, weighted: each loses 50 => 150 each
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"first item should shrink to 150px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"second item should shrink to 150px (got {itemB.ContentRect.Width})");
        }

        // flex:initial does not grow into remaining space
        [Fact]
        public void FlexInitial_DoesNotGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:initial;width:60px;height:30px'></div>
                    <div id='b' style='flex:initial;width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width}");
            // flex:initial => grow:0, items stay at their widths
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 60) < 2,
                $"flex:initial should preserve 60px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 80) < 2,
                $"flex:initial should preserve 80px (got {itemB.ContentRect.Width})");
        }
    }
}
