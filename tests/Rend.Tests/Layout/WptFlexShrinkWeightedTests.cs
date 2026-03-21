using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Flexbox weighted flex-shrink conformance per CSS-FLEXBOX §9.7.
    /// Shrink amount = overflow * (flex-shrink * flex-basis) / sum(flex-shrink * flex-basis).
    /// </summary>
    public class WptFlexShrinkWeightedTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexShrinkWeightedTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.7] Two items shrink:1, equal basis:80 in 100px container
        [Fact]
        public void TwoEqualBasis80_InContainer100_ShrinkEqually()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex:0 1 80px;height:30px'></div>
                    <div id='b' style='flex:0 1 80px;height:30px'></div>
                </div></body>");
            // Overflow=60. Scaled: a=1*80=80, b=1*80=80. Total=160.
            // Each shrinks 60*80/160=30 => 50px each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 50) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Shrink 1:2 with equal basis:80 in 100px container
        [Fact]
        public void Shrink1To2_EqualBasis80_InContainer100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex:0 1 80px;height:30px'></div>
                    <div id='b' style='flex:0 2 80px;height:30px'></div>
                </div></body>");
            // Overflow=60. Scaled: a=1*80=80, b=2*80=160. Total=240.
            // a shrinks 60*80/240=20 => 60. b shrinks 60*160/240=40 => 40.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 60) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 40) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Shrink 1:3 with equal basis:80 in 100px container
        [Fact]
        public void Shrink1To3_EqualBasis80_InContainer100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex:0 1 80px;height:30px'></div>
                    <div id='b' style='flex:0 3 80px;height:30px'></div>
                </div></body>");
            // Overflow=60. Scaled: a=1*80=80, b=3*80=240. Total=320.
            // a shrinks 60*80/320=15 => 65. b shrinks 60*240/320=45 => 35.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 65) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 35) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Equal shrink:1 but different basis sizes — larger basis shrinks more
        [Fact]
        public void WeightedByBasis_LargerBasisShrinksMore()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='small' style='flex:0 1 100px;height:30px'></div>
                    <div id='large' style='flex:0 1 200px;height:30px'></div>
                </div></body>");
            // Overflow=100. Scaled: small=1*100=100, large=1*200=200. Total=300.
            // small shrinks 100*100/300=33.33 => 66.67. large shrinks 100*200/300=66.67 => 133.33.
            var itemSmall = LayoutTestHelper.FindById(root, "small")!;
            var itemLarge = LayoutTestHelper.FindById(root, "large")!;
            _output.WriteLine($"small={itemSmall.ContentRect.Width} large={itemLarge.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemSmall.ContentRect.Width - 66.67f) < 1.5f);
            Assert.True(System.Math.Abs(itemLarge.ContentRect.Width - 133.33f) < 1.5f);
            Assert.True(itemLarge.ContentRect.Width > itemSmall.ContentRect.Width,
                "Larger basis item should remain larger after proportional shrink");
        }

        // [CSS-FLEXBOX §9.7] Shrink:0 on one item prevents it from shrinking
        [Fact]
        public void ShrinkZeroOnOneItem_ItemDoesNotShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:120px'>
                    <div id='fixed' style='flex:0 0 80px;height:30px'></div>
                    <div id='shrinkable' style='flex:0 1 80px;height:30px'></div>
                </div></body>");
            // Overflow=40. Only shrinkable shrinks. fixed stays 80. shrinkable => 80-40=40.
            var itemFixed = LayoutTestHelper.FindById(root, "fixed")!;
            var itemShrinkable = LayoutTestHelper.FindById(root, "shrinkable")!;
            _output.WriteLine($"fixed={itemFixed.ContentRect.Width} shrinkable={itemShrinkable.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemFixed.ContentRect.Width - 80) < 1.5f);
            Assert.True(System.Math.Abs(itemShrinkable.ContentRect.Width - 40) < 1.5f);
        }

        // [CSS-FLEXBOX §4.5] min-width:0 allows shrinking below content minimum
        [Fact]
        public void ShrinkWithMinWidthZero_AllowsFullShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:60px'>
                    <div id='a' style='flex:0 1 100px;min-width:0;height:30px'></div>
                    <div id='b' style='flex:0 1 100px;min-width:0;height:30px'></div>
                </div></body>");
            // Overflow=140. Equal scaled factors => each shrinks 70 => 30px each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 30) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 30) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] min-width clamps shrink — item cannot go below min-width
        [Fact]
        public void ShrinkWithMinWidthClamp_ClampsAtMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='clamped' style='flex:0 1 100px;min-width:70px;height:30px'></div>
                    <div id='free' style='flex:0 1 100px;min-width:0;height:30px'></div>
                </div></body>");
            // Overflow=100. Clamped can shrink at most 30 (100-70). Free absorbs rest.
            // clamped => 70, free => 100-70=30.
            var itemClamped = LayoutTestHelper.FindById(root, "clamped")!;
            var itemFree = LayoutTestHelper.FindById(root, "free")!;
            _output.WriteLine($"clamped={itemClamped.ContentRect.Width} free={itemFree.ContentRect.Width}");
            Assert.True(itemClamped.ContentRect.Width >= 69,
                $"Item with min-width:70 should not shrink below 70 (got {itemClamped.ContentRect.Width})");
            Assert.True(itemFree.ContentRect.Width < itemClamped.ContentRect.Width,
                "Free item should be smaller than clamped item");
        }

        // [CSS-FLEXBOX §9.3] Padding is part of outer size; shrink applies to content-box basis
        [Fact]
        public void ShrinkWithPadding_PaddingPreserved()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='padded' style='flex:0 1 120px;padding:10px;height:30px'></div>
                    <div id='plain' style='flex:0 1 120px;height:30px'></div>
                </div></body>");
            // padded outer = 120+20=140, plain outer = 120. Total=260. Overflow=60.
            // Scaled: padded=1*120=120, plain=1*120=120. Total=240.
            // Each shrinks 60*120/240=30. padded content=90, plain content=90.
            var itemPadded = LayoutTestHelper.FindById(root, "padded")!;
            var itemPlain = LayoutTestHelper.FindById(root, "plain")!;
            _output.WriteLine($"padded={itemPadded.ContentRect.Width} plain={itemPlain.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemPadded.ContentRect.Width - 90) < 1.5f);
            Assert.True(System.Math.Abs(itemPlain.ContentRect.Width - 90) < 1.5f);
        }

        // [CSS-FLEXBOX §9.3] Border is part of outer size; shrink applies to content-box basis
        [Fact]
        public void ShrinkWithBorder_BorderPreserved()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='bordered' style='flex:0 1 120px;border:5px solid red;height:30px'></div>
                    <div id='plain' style='flex:0 1 120px;height:30px'></div>
                </div></body>");
            // bordered outer = 120+10=130, plain outer = 120. Total=250. Overflow=50.
            // Scaled: each=1*120=120. Total=240.
            // Each shrinks 50*120/240=25. bordered content=95, plain content=95.
            var itemBordered = LayoutTestHelper.FindById(root, "bordered")!;
            var itemPlain = LayoutTestHelper.FindById(root, "plain")!;
            _output.WriteLine($"bordered={itemBordered.ContentRect.Width} plain={itemPlain.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemBordered.ContentRect.Width - 95) < 1.5f);
            Assert.True(System.Math.Abs(itemPlain.ContentRect.Width - 95) < 1.5f);
        }

        // [CSS-FLEXBOX §9.3] box-sizing:border-box — basis includes padding+border
        [Fact]
        public void ShrinkWithBorderBox_BasisIncludesPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex:0 1 80px;box-sizing:border-box;padding:10px;height:30px'></div>
                    <div id='b' style='flex:0 1 80px;box-sizing:border-box;padding:10px;height:30px'></div>
                </div></body>");
            // border-box: basis=80 includes padding. Outer=80 each. Total=160. Overflow=60.
            // Each shrinks 30 => outer=50, content=50-20=30.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 30) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 30) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Column direction: equal shrink on heights
        [Fact]
        public void ColumnShrinkEqual_HeightShrinks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:120px;width:200px'>
                    <div id='a' style='flex:0 1 100px;min-height:0'></div>
                    <div id='b' style='flex:0 1 100px;min-height:0'></div>
                </div></body>");
            // Overflow=80. Equal scaled => each shrinks 40 => 60px each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.h={itemA.ContentRect.Height} b.h={itemB.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 60) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 60) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Column direction: weighted shrink 1:3
        [Fact]
        public void ColumnShrinkWeighted_1To3()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:120px;width:200px'>
                    <div id='a' style='flex:0 1 100px;min-height:0'></div>
                    <div id='b' style='flex:0 3 100px;min-height:0'></div>
                </div></body>");
            // Overflow=80. Scaled: a=1*100=100, b=3*100=300. Total=400.
            // a shrinks 80*100/400=20 => 80. b shrinks 80*300/400=60 => 40.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.h={itemA.ContentRect.Height} b.h={itemB.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 80) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 40) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Three items shrink 1:1:1 with equal basis
        [Fact]
        public void ThreeItems_Shrink1_1_1_EqualBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:150px'>
                    <div id='a' style='flex:0 1 100px;height:30px'></div>
                    <div id='b' style='flex:0 1 100px;height:30px'></div>
                    <div id='c' style='flex:0 1 100px;height:30px'></div>
                </div></body>");
            // Overflow=150. Equal scaled => each shrinks 50 => 50px each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width} c={itemC.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 50) < 1.5f);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 50) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Three items shrink 1:2:3 with equal basis
        [Fact]
        public void ThreeItems_Shrink1_2_3_EqualBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex:0 1 100px;height:30px'></div>
                    <div id='b' style='flex:0 2 100px;height:30px'></div>
                    <div id='c' style='flex:0 3 100px;height:30px'></div>
                </div></body>");
            // Overflow=200. Scaled: a=1*100=100, b=2*100=200, c=3*100=300. Total=600.
            // a shrinks 200*100/600=33.33 => 66.67.
            // b shrinks 200*200/600=66.67 => 33.33.
            // c shrinks 200*300/600=100 => 0.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width} c={itemC.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 66.67f) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 33.33f) < 1.5f);
            Assert.True(itemC.ContentRect.Width < 1.5f, $"c should shrink to ~0 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Shrink with gap reduces available space
        [Fact]
        public void ShrinkWithGap_GapSubtractedFromAvailable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;gap:20px'>
                    <div id='a' style='flex:0 1 150px;height:30px'></div>
                    <div id='b' style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            // Available=200, gap=20. Total outer=150+150+20=320. Overflow=120.
            // Scaled: each=1*150=150. Total=300. Each shrinks 120*150/300=60 => 90 each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 90) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 90) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] No overflow means no shrink is applied
        [Fact]
        public void NoOverflow_NoShrinkApplied()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0 1 80px;height:30px'></div>
                    <div id='b' style='flex:0 1 80px;height:30px'></div>
                </div></body>");
            // Total=160 < 300. No overflow, items keep their basis.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 80) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Verify item positions are correct after shrink
        [Fact]
        public void PositionsAfterShrink_ItemsArrangedSequentially()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex:0 1 80px;height:30px'></div>
                    <div id='b' style='flex:0 2 80px;height:30px'></div>
                </div></body>");
            // Overflow=60. Scaled: a=1*80=80, b=2*80=160. Total=240.
            // a shrinks 60*80/240=20 => 60. b shrinks 60*160/240=40 => 40.
            // a starts at x=0, b starts at x=60.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.x={itemA.ContentRect.X} a.w={itemA.ContentRect.Width} b.x={itemB.ContentRect.X} b.w={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 60) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 40) < 1.5f);
            float expectedBX = itemA.ContentRect.X + itemA.ContentRect.Width;
            Assert.True(System.Math.Abs(itemB.ContentRect.X - expectedBX) < 1.5f,
                $"b should start right after a (expected {expectedBX}, got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Mix of shrink:0 and shrink:1 — only shrinkable items shrink
        [Fact]
        public void MixedShrink0And1_OnlyShrinkableItemsShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='fixed1' style='flex:0 0 80px;height:30px'></div>
                    <div id='shrink' style='flex:0 1 80px;height:30px'></div>
                    <div id='fixed2' style='flex:0 0 80px;height:30px'></div>
                </div></body>");
            // Total=240. Overflow=40. Only shrink absorbs.
            // fixed1=80, fixed2=80, shrink=80-40=40.
            var itemFixed1 = LayoutTestHelper.FindById(root, "fixed1")!;
            var itemShrink = LayoutTestHelper.FindById(root, "shrink")!;
            var itemFixed2 = LayoutTestHelper.FindById(root, "fixed2")!;
            _output.WriteLine($"fixed1={itemFixed1.ContentRect.Width} shrink={itemShrink.ContentRect.Width} fixed2={itemFixed2.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemFixed1.ContentRect.Width - 80) < 1.5f);
            Assert.True(System.Math.Abs(itemFixed2.ContentRect.Width - 80) < 1.5f);
            Assert.True(System.Math.Abs(itemShrink.ContentRect.Width - 40) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Four items with equal shrink:1 and equal basis
        [Fact]
        public void FourEqualShrink_EqualBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 100px;height:30px'></div>
                    <div id='b' style='flex:0 1 100px;height:30px'></div>
                    <div id='c' style='flex:0 1 100px;height:30px'></div>
                    <div id='d' style='flex:0 1 100px;height:30px'></div>
                </div></body>");
            // Total=400. Overflow=200. Each shrinks 50 => 50px each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width} c={itemC.ContentRect.Width} d={itemD.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 50) < 1.5f);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 50) < 1.5f);
            Assert.True(System.Math.Abs(itemD.ContentRect.Width - 50) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Five items with equal shrink:1 and equal basis
        [Fact]
        public void FiveEqualShrink_EqualBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 100px;height:30px'></div>
                    <div id='b' style='flex:0 1 100px;height:30px'></div>
                    <div id='c' style='flex:0 1 100px;height:30px'></div>
                    <div id='d' style='flex:0 1 100px;height:30px'></div>
                    <div id='e' style='flex:0 1 100px;height:30px'></div>
                </div></body>");
            // Total=500. Overflow=300. Each shrinks 60 => 40px each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            var itemE = LayoutTestHelper.FindById(root, "e")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width} c={itemC.ContentRect.Width} d={itemD.ContentRect.Width} e={itemE.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 40) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 40) < 1.5f);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 40) < 1.5f);
            Assert.True(System.Math.Abs(itemD.ContentRect.Width - 40) < 1.5f);
            Assert.True(System.Math.Abs(itemE.ContentRect.Width - 40) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Shrink with padding and border combined on one item
        [Fact]
        public void ShrinkWithPaddingAndBorder_OuterSizeAffectsOverflow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='decorated' style='flex:0 1 120px;padding:5px;border:5px solid black;height:30px'></div>
                    <div id='plain' style='flex:0 1 120px;height:30px'></div>
                </div></body>");
            // decorated outer = 120+10+10=140. plain outer = 120. Total=260. Overflow=60.
            // Scaled: each=1*120=120. Total=240. Each shrinks 60*120/240=30.
            // decorated content=90, plain content=90.
            var itemDecorated = LayoutTestHelper.FindById(root, "decorated")!;
            var itemPlain = LayoutTestHelper.FindById(root, "plain")!;
            _output.WriteLine($"decorated={itemDecorated.ContentRect.Width} plain={itemPlain.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemDecorated.ContentRect.Width - 90) < 1.5f);
            Assert.True(System.Math.Abs(itemPlain.ContentRect.Width - 90) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Shrink 2:1 with different basis — combined weighting
        [Fact]
        public void Shrink2To1_DifferentBasis_CombinedWeighting()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 2 80px;height:30px'></div>
                    <div id='b' style='flex:0 1 200px;height:30px'></div>
                </div></body>");
            // Overflow=80. Scaled: a=2*80=160, b=1*200=200. Total=360.
            // a shrinks 80*160/360=35.56 => 44.44. b shrinks 80*200/360=44.44 => 155.56.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 44.44f) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 155.56f) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Column shrink with gap
        [Fact]
        public void ColumnShrinkWithGap_GapPreserved()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:100px;width:200px;gap:10px'>
                    <div id='a' style='flex:0 1 80px;min-height:0'></div>
                    <div id='b' style='flex:0 1 80px;min-height:0'></div>
                </div></body>");
            // Available=100, gap=10. Total outer=80+80+10=170. Overflow=70.
            // Each shrinks 35 => 45px each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.h={itemA.ContentRect.Height} b.h={itemB.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 45) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 45) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Three items shrink 1:2:3 verify ordering preserved
        [Fact]
        public void ThreeItems_Shrink1_2_3_OrderingPreserved()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:240px'>
                    <div id='a' style='flex:0 1 120px;height:30px'></div>
                    <div id='b' style='flex:0 2 120px;height:30px'></div>
                    <div id='c' style='flex:0 3 120px;height:30px'></div>
                </div></body>");
            // Overflow=120. Scaled: a=1*120=120, b=2*120=240, c=3*120=360. Total=720.
            // a shrinks 120*120/720=20 => 100. b shrinks 120*240/720=40 => 80. c shrinks 120*360/720=60 => 60.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width} c={itemC.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 80) < 1.5f);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 60) < 1.5f);
            Assert.True(itemA.ContentRect.Width > itemB.ContentRect.Width, "a > b in size");
            Assert.True(itemB.ContentRect.Width > itemC.ContentRect.Width, "b > c in size");
        }

        // [CSS-FLEXBOX §9.7] Shrink with border-box and border combined
        [Fact]
        public void ShrinkBorderBoxWithBorder_BasisIncludesBorderAndPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:120px'>
                    <div id='a' style='flex:0 1 100px;box-sizing:border-box;border:5px solid red;padding:5px;height:30px'></div>
                    <div id='b' style='flex:0 1 100px;box-sizing:border-box;border:5px solid blue;padding:5px;height:30px'></div>
                </div></body>");
            // border-box: basis=100 includes padding+border. Outer=100 each. Total=200. Overflow=80.
            // Each shrinks 40 => outer=60, content=60-20=40.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 40) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 40) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Large shrink factor ratio 1:10
        [Fact]
        public void ShrinkRatio1To10_SkewedDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex:0 1 100px;height:30px'></div>
                    <div id='b' style='flex:0 10 100px;height:30px'></div>
                </div></body>");
            // Overflow=100. Scaled: a=1*100=100, b=10*100=1000. Total=1100.
            // a shrinks 100*100/1100=9.09 => 90.91. b shrinks 100*1000/1100=90.91 => 9.09.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 90.91f) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 9.09f) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Fractional shrink factors 0.5 and 1.5
        [Fact]
        public void FractionalShrinkFactors_HalfAndOneAndHalf()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex:0 0.5 100px;height:30px'></div>
                    <div id='b' style='flex:0 1.5 100px;height:30px'></div>
                </div></body>");
            // Overflow=100. Scaled: a=0.5*100=50, b=1.5*100=150. Total=200.
            // a shrinks 100*50/200=25 => 75. b shrinks 100*150/200=75 => 25.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 75) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 25) < 1.5f);
        }

        // [CSS-FLEXBOX §9.7] Verify total width after shrink equals container width
        [Fact]
        public void TotalWidthAfterShrink_EqualsContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 120px;height:30px'></div>
                    <div id='b' style='flex:0 2 120px;height:30px'></div>
                    <div id='c' style='flex:0 3 120px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float totalWidth = itemA.ContentRect.Width + itemB.ContentRect.Width + itemC.ContentRect.Width;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width} c={itemC.ContentRect.Width} total={totalWidth}");
            Assert.True(System.Math.Abs(totalWidth - 200) < 2,
                $"Total width should equal container width 200 (got {totalWidth})");
        }

        // [CSS-FLEXBOX §9.7] All shrink:0 — items overflow container, no shrinking
        [Fact]
        public void AllShrinkZero_ItemsOverflowNoShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex:0 0 80px;height:30px'></div>
                    <div id='b' style='flex:0 0 80px;height:30px'></div>
                </div></body>");
            // Both shrink:0. Items keep full basis, overflow container.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 1.5f);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 80) < 1.5f);
        }
    }
}
