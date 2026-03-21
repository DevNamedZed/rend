using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Flexbox sizing edge cases: percentage basis with grow, calc basis,
    /// border-box interactions, min/max clamping, weighted shrink, zero-size items,
    /// negative free space, and content-based sizing.
    /// </summary>
    public class WptFlexSizingEdgeCaseTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexSizingEdgeCaseTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.2] percentage width + flex-grow: basis resolves from percentage, grow fills remainder
        [Fact]
        public void PercentageWidth_WithFlexGrow_GrowsFromPercentageBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='width:25%;flex-grow:1;height:30px'></div>
                    <div style='width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // basis=25% of 400=100. Free=400-100-100=200. Grow=1 only item growing → gets 200. Total=300.
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2,
                $"25% width + flex-grow:1 should fill to 300 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis: calc() resolves the expression
        [Fact]
        public void FlexBasis_Calc_ResolvesExpression()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex:0 0 calc(50% - 20px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // calc(50% - 20px) of 400 = 200 - 20 = 180
            Assert.True(System.Math.Abs(item.ContentRect.Width - 180) < 2,
                $"calc(50% - 20px) basis should be 180 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] border-box + padding + flex-grow: padding included in flex base size
        [Fact]
        public void BorderBox_Padding_FlexGrow_PaddingIncludedInSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='box-sizing:border-box;padding:20px;flex:1 0 100px;height:60px'></div>
                    <div style='flex:1 0 100px;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // basis=100px each. Free=200. Each gets 100 grow. Item total=200px border-box → content=200-40=160.
            Assert.True(System.Math.Abs(item.ContentRect.Width - 160) < 2,
                $"border-box item content width should be 160 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4.5] min-width:0 allows shrink below content size
        [Fact]
        public void FlexShrink_MinWidth0_ShrinksBelowContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='t' style='flex:0 1 200px;min-width:0;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(item.ContentRect.Width <= 101,
                $"min-width:0 should allow shrink to container (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex item auto sizing uses child content width
        [Fact]
        public void FlexItem_AutoSizing_UsesChildContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex:0 0 auto;height:30px'>
                        <div style='width:150px;height:20px'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"auto basis should size to child content 150 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] max-width clamps growth
        [Fact]
        public void FlexGrow_MaxWidth_ClampsGrowthAndRedistributes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;max-width:100px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                    <div id='c' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.width={itemA!.ContentRect.Width} b.width={itemB!.ContentRect.Width}");
            // a clamped to 100. Remaining 300 split between b and c → 150 each.
            Assert.True(itemA.ContentRect.Width <= 101,
                $"max-width:100 should clamp a (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"remaining space redistributed to b=150 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] min-width prevents shrink
        [Fact]
        public void FlexShrink_MinWidth_PreventsFullShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 180px;min-width:150px;height:30px'></div>
                    <div id='b' style='flex:0 1 180px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            Assert.NotNull(itemA);
            _output.WriteLine($"a.width={itemA!.ContentRect.Width}");
            Assert.True(itemA.ContentRect.Width >= 149,
                $"min-width:150 should prevent shrink below 150 (got {itemA.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §7.1] flex:0 0 auto with no width → sizes to zero (no content)
        [Fact]
        public void Flex_0_0_Auto_NoWidth_SizesToZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:0 0 auto;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // auto basis with no width and no content → 0
            Assert.True(item.ContentRect.Width < 2,
                $"flex:0 0 auto with no content should be ~0 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §7.1] flex:0 0 0px collapses item to zero width
        [Fact]
        public void Flex_0_0_0px_CollapsesToZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:0 0 0px;width:200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // flex-basis: 0px overrides width. No grow → stays at 0.
            Assert.True(item.ContentRect.Width < 2,
                $"flex:0 0 0px should collapse to 0 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] grow distributes proportionally to multiple items
        [Fact]
        public void FlexGrow_DistributesToMultipleItems_1_2_3()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:2 0 0px;height:30px'></div>
                    <div id='c' style='flex:3 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width} c={itemC!.ContentRect.Width}");
            // Total grow=6. a=100, b=200, c=300.
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"grow:1 of 600/6=100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"grow:2 of 600/6=200 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 300) < 2,
                $"grow:3 of 600/6=300 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] shrink weighted by flex-shrink * flex-base-size
        [Fact]
        public void FlexShrink_WeightedByBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0 1 200px;height:30px'></div>
                    <div id='b' style='flex:0 1 400px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            // Overflow=300. Weighted: a=1*200=200, b=1*400=400. Total=600.
            // a shrinks 300*200/600=100→100. b shrinks 300*400/600=200→200.
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 3,
                $"shrink weighted: a should be ~100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 3,
                $"shrink weighted: b should be ~200 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] flex item with margin + grow: margin reduces available space
        [Fact]
        public void FlexGrow_WithMargin_MarginReducesAvailable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex:1 0 0px;margin:0 50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // margin-left:50 + margin-right:50 = 100. Free=400-0-100=300. grow:1 → 300.
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2,
                $"margin should reduce available space: 400-100=300 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9] column flex with percentage height basis
        [Fact]
        public void ColumnFlex_PercentageHeightBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:100px'>
                    <div id='t' style='flex:0 0 50%;'></div>
                    <div style='flex:1 0 0px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}");
            // 50% of 200 = 100
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"column 50% basis of 200 = 100 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9] zero-size flex items don't break layout
        [Fact]
        public void ZeroSizeFlexItems_DoNotBreakLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0 0 0px;height:30px'></div>
                    <div id='b' style='flex:0 0 0px;height:30px'></div>
                    <div id='c' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemC);
            _output.WriteLine($"a={itemA!.ContentRect.Width} c={itemC!.ContentRect.Width}");
            Assert.True(itemA.ContentRect.Width < 2, $"zero-basis item should be ~0 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 300) < 2,
                $"grow item should fill remaining 300 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] negative free space distributes via shrink factors
        [Fact]
        public void NegativeFreeSpace_DistributedByShrinkFactors()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 2 200px;height:30px'></div>
                    <div id='b' style='flex:0 1 200px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            // Overflow=200. Weighted: a=2*200=400, b=1*200=200. Total=600.
            // a shrinks 200*400/600≈133.33→66.67. b shrinks 200*200/600≈66.67→133.33.
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 66.67f) < 3,
                $"shrink:2 item should be ~67 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 133.33f) < 3,
                $"shrink:1 item should be ~133 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:content sizes from item content
        [Fact]
        public void FlexBasis_Content_SizesFromChildContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex:0 0 content;height:40px'>
                        <div style='width:120px;height:20px'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // flex-basis:content → sizes to child content = 120px
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"flex-basis:content should size to child 120 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] grow with non-zero basis: free space = container - sum(bases)
        [Fact]
        public void FlexGrow_NonZeroBasis_DistributesOnlyFreeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='a' style='flex:1 0 100px;height:30px'></div>
                    <div id='b' style='flex:1 0 200px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            // Free=500-100-200=200. Each gets 100. a=200, b=300.
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"a: 100+100=200 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 300) < 2,
                $"b: 200+100=300 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] border-box basis with grow: padding/border part of basis
        [Fact]
        public void FlexGrow_BorderBoxBasis_PaddingInBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='box-sizing:border-box;flex:1 0 200px;padding:30px;border:5px solid black;height:80px'></div>
                    <div style='flex:1 0 0px;height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // basis=200px border-box. Free=400-200-0=200. Each gets 100. Item=300 border-box.
            // Content = 300 - 30*2 - 5*2 = 230.
            Assert.True(System.Math.Abs(item.ContentRect.Width - 230) < 2,
                $"border-box grow: content should be 230 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9] flex:1 with equal items divides space equally
        [Fact]
        public void FlexGrow_EqualItems_DividesEqually()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width} c={itemC!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"equal flex:1 → 300/3=100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"equal flex:1 → 300/3=100 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2,
                $"equal flex:1 → 300/3=100 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] flex-grow with gap: gap reduces free space
        [Fact]
        public void FlexGrow_WithGap_GapReducesFreeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;gap:20px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                    <div id='c' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            Assert.NotNull(itemA);
            _output.WriteLine($"a={itemA!.ContentRect.Width}");
            // Free=400-2*20=360. Each gets 120.
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 120) < 2,
                $"gap:20 * 2 gaps → free=360/3=120 (got {itemA.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9] shrink does not go below zero
        [Fact]
        public void FlexShrink_DoesNotGoBelowZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:50px'>
                    <div id='a' style='flex:0 1 100px;height:30px'></div>
                    <div id='b' style='flex:0 1 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(itemA.ContentRect.Width >= 0, $"width should not be negative (got {itemA.ContentRect.Width})");
            Assert.True(itemB.ContentRect.Width >= 0, $"width should not be negative (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] grow:0 items do not grow even with free space
        [Fact]
        public void FlexGrow0_DoesNotGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='fixed' style='flex:0 0 100px;height:30px'></div>
                    <div id='grow' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var fixedItem = LayoutTestHelper.FindById(root, "fixed");
            var growItem = LayoutTestHelper.FindById(root, "grow");
            Assert.NotNull(fixedItem);
            Assert.NotNull(growItem);
            _output.WriteLine($"fixed={fixedItem!.ContentRect.Width} grow={growItem!.ContentRect.Width}");
            Assert.True(System.Math.Abs(fixedItem.ContentRect.Width - 100) < 2,
                $"grow:0 item should stay at 100 (got {fixedItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(growItem.ContentRect.Width - 300) < 2,
                $"grow:1 item should take remainder 300 (got {growItem.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] unequal grow factors with non-zero basis
        [Fact]
        public void FlexGrow_UnequalFactors_NonZeroBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='a' style='flex:1 0 50px;height:30px'></div>
                    <div id='b' style='flex:3 0 50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            // Free=500-50-50=400. a gets 400*1/4=100→150. b gets 400*3/4=300→350.
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"grow:1 → 50+100=150 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 350) < 2,
                $"grow:3 → 50+300=350 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9] column flex auto height with grow items
        [Fact]
        public void ColumnFlex_GrowItems_FillExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex:1 0 0px'></div>
                    <div id='b' style='flex:2 0 0px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.height={itemA!.ContentRect.Height} b.height={itemB!.ContentRect.Height}");
            // Total grow=3. a=100, b=200.
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2,
                $"column grow:1 → 300/3=100 (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 200) < 2,
                $"column grow:2 → 300*2/3=200 (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9] flex item with both min-width and max-width constraints
        [Fact]
        public void FlexItem_MinAndMaxWidth_BothApply()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='t' style='flex:1 0 0px;min-width:100px;max-width:200px;height:30px'></div>
                    <div style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // Without constraints: 300 each. max-width:200 clamps → 200. Remainder goes to other item.
            Assert.True(item.ContentRect.Width <= 201,
                $"max-width:200 should clamp (got {item.ContentRect.Width})");
            Assert.True(item.ContentRect.Width >= 99,
                $"min-width:100 floor (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] shrink:0 items do not shrink
        [Fact]
        public void FlexShrink0_DoesNotShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='rigid' style='flex:0 0 150px;height:30px'></div>
                    <div id='shrinkable' style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            var rigid = LayoutTestHelper.FindById(root, "rigid");
            var shrinkable = LayoutTestHelper.FindById(root, "shrinkable");
            Assert.NotNull(rigid);
            Assert.NotNull(shrinkable);
            _output.WriteLine($"rigid={rigid!.ContentRect.Width} shrinkable={shrinkable!.ContentRect.Width}");
            Assert.True(System.Math.Abs(rigid.ContentRect.Width - 150) < 2,
                $"shrink:0 should not shrink from 150 (got {rigid.ContentRect.Width})");
            Assert.True(System.Math.Abs(shrinkable.ContentRect.Width - 50) < 2,
                $"shrinkable absorbs all overflow → 50 (got {shrinkable.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] calc basis with addition
        [Fact]
        public void FlexBasis_CalcAddition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex:0 0 calc(100px + 50px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"calc(100px + 50px) = 150 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9] multiple items with mixed grow and fixed
        [Fact]
        public void MixedGrowAndFixed_CorrectDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='fixed1' style='flex:0 0 50px;height:30px'></div>
                    <div id='grow1' style='flex:1 0 0px;height:30px'></div>
                    <div id='fixed2' style='flex:0 0 100px;height:30px'></div>
                    <div id='grow2' style='flex:2 0 0px;height:30px'></div>
                </div></body>");
            var grow1 = LayoutTestHelper.FindById(root, "grow1");
            var grow2 = LayoutTestHelper.FindById(root, "grow2");
            Assert.NotNull(grow1);
            Assert.NotNull(grow2);
            _output.WriteLine($"grow1={grow1!.ContentRect.Width} grow2={grow2!.ContentRect.Width}");
            // Free=500-50-100=350. grow1=350*1/3≈116.67. grow2=350*2/3≈233.33.
            Assert.True(System.Math.Abs(grow1.ContentRect.Width - 116.67f) < 3,
                $"grow:1 share of 350 ≈ 116.67 (got {grow1.ContentRect.Width})");
            Assert.True(System.Math.Abs(grow2.ContentRect.Width - 233.33f) < 3,
                $"grow:2 share of 350 ≈ 233.33 (got {grow2.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9] percentage basis + padding in content-box mode
        [Fact]
        public void PercentageBasis_ContentBox_PaddingAdded()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex:0 0 50%;padding:10px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"content width={item!.ContentRect.Width}");
            // flex-basis:50% of 400=200 content. Padding:10px each side doesn't reduce content in content-box.
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"50% content-box basis: content=200 (got {item.ContentRect.Width})");
        }
    }
}
