using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// <spec>CSS-FLEXBOX §9.7 https://drafts.csswg.org/css-flexbox-1/#resolve-flexible-lengths</spec>
    /// Tests for flex-grow distribution when items have non-zero flex-basis values.
    /// Free space = container - sum(basis), distributed proportionally by grow factor.
    /// </summary>
    public class WptFlexGrowWithBasisTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexGrowWithBasisTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.7] basis:50+grow:1 and basis:100+grow:1 in 300px container
        // Free space = 300 - 150 = 150, split 1:1 => 50+75=125, 100+75=175
        [Fact]
        public void GrowWithBasis_UnequalBasis_EqualGrow_300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 50px;height:30px'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 125) < 2,
                $"basis:50 grow:1 expected ~125 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 175) < 2,
                $"basis:100 grow:1 expected ~175 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] basis:100+grow:1 and basis:100+grow:2 in 400px container
        // Free space = 400 - 200 = 200, split 1:2 => 100+66.67=166.67, 100+133.33=233.33
        [Fact]
        public void GrowWithBasis_EqualBasis_UnequalGrow_400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 100px;height:30px'></div>
                    <div id='b' style='flex:2 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 166.67f) < 2,
                $"grow:1 expected ~166.67 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 233.33f) < 2,
                $"grow:2 expected ~233.33 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] single item basis:0+grow:1 fills entire container
        [Fact]
        public void GrowWithBasis_ZeroBasis_SingleItem_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2,
                $"basis:0 grow:1 should fill 300px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] basis:auto with explicit width:80 + grow:1 grows from 80
        [Fact]
        public void GrowWithBasis_AutoBasis_ExplicitWidth_GrowsFromWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex-grow:1;flex-basis:auto;width:80px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2,
                $"auto basis + width:80 + grow:1, single item fills 300 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] basis:50% + grow:1 in 300px => basis=150, grows to 300
        [Fact]
        public void GrowWithBasis_PercentBasis_SingleItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:1 0 50%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2,
                $"50% basis + grow:1 single item fills container (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] three items basis:50+grow:1 in 300px
        // Free space = 300 - 150 = 150, split 1:1:1 => each gets 50+50=100
        [Fact]
        public void GrowWithBasis_ThreeItems_EqualBasisAndGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 50px;height:30px'></div>
                    <div id='b' style='flex:1 0 50px;height:30px'></div>
                    <div id='c' style='flex:1 0 50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width} c={itemC!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"each item expected ~100 (got a={itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"each item expected ~100 (got b={itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2,
                $"each item expected ~100 (got c={itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] basis:100 + grow:0 stays at 100
        [Fact]
        public void GrowWithBasis_GrowZero_StaysAtBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:0 0 100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"grow:0 keeps basis width (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] two items basis:100+grow:1 in 300px => 150+150
        [Fact]
        public void GrowWithBasis_TwoItems_EqualBasisAndGrow_300()
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
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"expected ~150 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"expected ~150 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] basis:80+grow:1 and basis:120+grow:1 in 300px
        // Free space = 300 - 200 = 100, split 1:1 => 80+50=130, 120+50=170
        [Fact]
        public void GrowWithBasis_80And120_EqualGrow_300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 80px;height:30px'></div>
                    <div id='b' style='flex:1 0 120px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 130) < 2,
                $"basis:80 grow:1 expected ~130 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 170) < 2,
                $"basis:120 grow:1 expected ~170 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] basis:50+grow:2 and basis:100+grow:1 in 300px
        // Free space = 300 - 150 = 150, split 2:1 => 50+100=150, 100+50=150
        [Fact]
        public void GrowWithBasis_UnequalBasis_UnequalGrow_300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:2 0 50px;height:30px'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"basis:50 grow:2 expected ~150 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"basis:100 grow:1 expected ~150 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] grow with column-gap:20px
        // Free space = 300 - 100 - 100 - 20(gap) = 80, split 1:1 => 100+40=140, 100+40=140
        [Fact]
        public void GrowWithBasis_WithColumnGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;column-gap:20px'>
                    <div id='a' style='flex:1 0 100px;height:30px'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 140) < 2,
                $"with gap: expected ~140 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 140) < 2,
                $"with gap: expected ~140 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] grow with container padding:20px
        // width:300px is content-box, so content area = 300. Free = 300 - 100 - 100 = 100, split 1:1 => 150+150
        [Fact]
        public void GrowWithBasis_WithContainerPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;padding:20px'>
                    <div id='a' style='flex:1 0 100px;height:30px'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"with padding: expected ~150 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"with padding: expected ~150 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] items with border-box sizing
        // border-box: basis includes padding+border. Item a: basis=100 includes 10px padding each side.
        // Content area for a = 100 - 20 = 80. But flex resolves in border-box.
        // Free space = 300 - 100 - 100 = 100, split 1:1 => border-box 150 each
        [Fact]
        public void GrowWithBasis_BorderBoxItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 100px;height:30px;box-sizing:border-box;padding:10px'></div>
                    <div id='b' style='flex:1 0 100px;height:30px;box-sizing:border-box;padding:10px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float borderBoxA = itemA!.ContentRect.Width + 20;
            float borderBoxB = itemB!.ContentRect.Width + 20;
            _output.WriteLine($"a content={itemA.ContentRect.Width} border-box={borderBoxA} b content={itemB.ContentRect.Width} border-box={borderBoxB}");
            Assert.True(System.Math.Abs(borderBoxA - 150) < 2,
                $"border-box a expected ~150 (got {borderBoxA})");
            Assert.True(System.Math.Abs(borderBoxB - 150) < 2,
                $"border-box b expected ~150 (got {borderBoxB})");
        }

        // [CSS-FLEXBOX §9.7] column direction: basis + grow distributes height
        // Container height=300, basis:50+grow:1, basis:100+grow:1
        // Free = 300 - 150 = 150, split 1:1 => 125, 175
        [Fact]
        public void GrowWithBasis_ColumnDirection()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex:1 0 50px'></div>
                    <div id='b' style='flex:1 0 100px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.h={itemA!.ContentRect.Height} b.h={itemB!.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 125) < 2,
                $"column grow: a expected height ~125 (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 175) < 2,
                $"column grow: b expected height ~175 (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] five items basis:0+grow:1 in 400px => 80 each
        [Fact]
        public void GrowWithBasis_FiveItems_ZeroBasis_EqualGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                    <div id='c' style='flex:1 0 0px;height:30px'></div>
                    <div id='d' style='flex:1 0 0px;height:30px'></div>
                    <div id='e' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            var itemD = LayoutTestHelper.FindById(root, "d");
            var itemE = LayoutTestHelper.FindById(root, "e");
            Assert.NotNull(itemA);
            Assert.NotNull(itemE);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width} c={itemC!.ContentRect.Width} d={itemD!.ContentRect.Width} e={itemE!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2,
                $"5 items in 400: each expected ~80 (got a={itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC!.ContentRect.Width - 80) < 2,
                $"5 items in 400: each expected ~80 (got c={itemC.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemE.ContentRect.Width - 80) < 2,
                $"5 items in 400: each expected ~80 (got e={itemE.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] basis with max-width clamp: grow distributes but capped
        // Items a(basis:50,grow:2,max:120) and b(basis:50,grow:1) in 300px
        // Without clamp: free=200, a gets 133.33→clamped to 120, remaining 80 goes to b
        // After freeze: a=120(frozen), b gets remaining 300-120=180
        [Fact]
        public void GrowWithBasis_MaxWidthClamps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:2 0 50px;max-width:120px;height:30px'></div>
                    <div id='b' style='flex:1 0 50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 120) < 2,
                $"max-width clamps a to 120 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 180) < 2,
                $"remaining space goes to b: expected ~180 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] basis with min-width enforce: grow cannot shrink below min
        // Items a(basis:50,grow:1,min:200) and b(basis:50,grow:1) in 300px
        // Without clamp: free=200, each gets 100 => a=150, b=150
        // But a has min:200 => a=200(frozen), b gets 300-200=100
        [Fact]
        public void GrowWithBasis_MinWidthEnforced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 50px;min-width:200px;height:30px'></div>
                    <div id='b' style='flex:1 0 50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"min-width enforced at 200 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"remaining space for b: expected ~100 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] three items with varying grow factors: 1:2:3
        // basis:30 each in 300px. Free=210, split 1:2:3 => 30+35=65, 30+70=100, 30+105=135
        [Fact]
        public void GrowWithBasis_ThreeItems_VaryingGrow_123()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 30px;height:30px'></div>
                    <div id='b' style='flex:2 0 30px;height:30px'></div>
                    <div id='c' style='flex:3 0 30px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width} c={itemC!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 65) < 2,
                $"grow:1 expected ~65 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"grow:2 expected ~100 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 135) < 2,
                $"grow:3 expected ~135 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] grow with percentage basis: two items 25%+grow:1 and 25%+grow:1 in 400px
        // basis=100 each, free=200, split 1:1 => 200 each
        [Fact]
        public void GrowWithBasis_PercentBasis_TwoItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 25%;height:30px'></div>
                    <div id='b' style='flex:1 0 25%;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"25% basis + grow:1 expected ~200 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"25% basis + grow:1 expected ~200 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] mixed grow:0 and grow:1 - only growing item absorbs free space
        // a(basis:100,grow:0) and b(basis:100,grow:1) in 300px => a=100, b=200
        [Fact]
        public void GrowWithBasis_MixedGrowZeroAndOne()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0 0 100px;height:30px'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"grow:0 stays at basis (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"grow:1 absorbs all free space (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] basis fills container exactly - no free space to distribute
        [Fact]
        public void GrowWithBasis_NoFreeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 150px;height:30px'></div>
                    <div id='b' style='flex:1 0 150px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"no free space: a stays at basis (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"no free space: b stays at basis (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] large grow factors: 10:1 ratio
        // basis:0 each in 330px. Free=330, split 10:1 => 300, 30
        [Fact]
        public void GrowWithBasis_LargeGrowRatio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:330px'>
                    <div id='a' style='flex:10 0 0px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 300) < 2,
                $"grow:10 expected ~300 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 30) < 2,
                $"grow:1 expected ~30 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] grow with item border: basis is content-box by default
        // a(basis:100,grow:1,border:5px) and b(basis:100,grow:1) in 300px
        // Free = 300 - (100+10) - 100 = 90, split 1:1 => a content=145, b=145
        [Fact]
        public void GrowWithBasis_ItemWithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 100px;height:30px;border:5px solid black'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a content={itemA!.ContentRect.Width} b content={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 145) < 2,
                $"item with border: a content expected ~145 (got {itemA.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] grow with item margin: margin reduces available space
        // a(basis:100,grow:1,margin:10px) and b(basis:100,grow:1) in 300px
        // Available = 300 - 20(margins) = 280. Free = 280 - 200 = 80, split 1:1 => a=140, b=140
        [Fact]
        public void GrowWithBasis_ItemWithMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 100px;height:30px;margin:0 10px'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 140) < 2,
                $"item with margin: a expected ~140 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 140) < 2,
                $"item with margin: b expected ~140 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] column direction with gap
        // Container height=300, gap=20, basis:50+grow:1, basis:50+grow:1
        // Free = 300 - 100 - 20 = 180, split 1:1 => 50+90=140, 50+90=140
        [Fact]
        public void GrowWithBasis_ColumnWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px;row-gap:20px'>
                    <div id='a' style='flex:1 0 50px'></div>
                    <div id='b' style='flex:1 0 50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.h={itemA!.ContentRect.Height} b.h={itemB!.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 140) < 2,
                $"column with gap: a expected height ~140 (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 140) < 2,
                $"column with gap: b expected height ~140 (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] four items with fractional grow: 0.5:0.5:1:1
        // basis:0 each in 300px. Total grow=3. a,b=50 each, c,d=100 each
        [Fact]
        public void GrowWithBasis_FractionalGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0.5 0 0px;height:30px'></div>
                    <div id='b' style='flex:0.5 0 0px;height:30px'></div>
                    <div id='c' style='flex:1 0 0px;height:30px'></div>
                    <div id='d' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemC);
            _output.WriteLine($"a={itemA!.ContentRect.Width} c={itemC!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 2,
                $"grow:0.5 expected ~50 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2,
                $"grow:1 expected ~100 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] basis larger than container with grow: items don't grow (overflow)
        // Two items basis:200 in 300px. Total basis=400 > 300. grow doesn't matter.
        // Without shrink (flex-shrink:0), items overflow at 200 each.
        [Fact]
        public void GrowWithBasis_BasisExceedsContainer_NoShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 200px;height:30px'></div>
                    <div id='b' style='flex:1 0 200px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"basis exceeds container, no shrink: a stays 200 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"basis exceeds container, no shrink: b stays 200 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] grow distributes from non-zero basis with gap and padding combined
        // width:400px is content-box, so content area = 400. gap:10px between 2 items.
        // Free = 400 - 100 - 100 - 10(gap) = 190, split 1:1 => 100+95=195, 100+95=195
        [Fact]
        public void GrowWithBasis_GapAndPaddingCombined()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;padding:0 20px;column-gap:10px'>
                    <div id='a' style='flex:1 0 100px;height:30px'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 195) < 2,
                $"gap+padding: a expected ~195 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 195) < 2,
                $"gap+padding: b expected ~195 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] max-width clamp redistributes to remaining items
        // Three items: a(basis:0,grow:1,max:50), b(basis:0,grow:1), c(basis:0,grow:1) in 300px
        // First pass: each gets 100. a clamped to 50 (frozen). Remaining 250 for b,c => 125 each.
        [Fact]
        public void GrowWithBasis_MaxWidthRedistributes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 0px;max-width:50px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                    <div id='c' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width} c={itemC!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 2,
                $"max-width clamps a to 50 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 125) < 2,
                $"redistributed: b expected ~125 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 125) < 2,
                $"redistributed: c expected ~125 (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] min-width forces growth beyond proportional share
        // Two items: a(basis:50,grow:1,min:200), b(basis:50,grow:3) in 300px
        // Without clamp: free=200, a gets 50 => 100, b gets 150 => 200.
        // But a has min:200, so a=200(frozen), b gets 300-200=100.
        [Fact]
        public void GrowWithBasis_MinWidthForcesLarger()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 50px;min-width:200px;height:30px'></div>
                    <div id='b' style='flex:3 0 50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"min-width forces a to 200 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"remaining for b: expected ~100 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] column with max-height clamp
        // Two items: a(basis:50,grow:1,max-height:100), b(basis:50,grow:1) in height:300
        // Without clamp: free=200, each gets 100 => a=150, b=150.
        // a clamped to max:100 (frozen). b gets 300-100=200.
        [Fact]
        public void GrowWithBasis_ColumnMaxHeightClamp()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex:1 0 50px;max-height:100px'></div>
                    <div id='b' style='flex:1 0 50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.h={itemA!.ContentRect.Height} b.h={itemB!.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2,
                $"max-height clamps a (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 200) < 2,
                $"remaining for b (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] auto basis resolves to content size, then grows from there
        // Two items: a(basis:auto, content ~0, grow:1), b(basis:100, grow:1) in 300px
        // a basis resolves to 0 (no content). Free = 300 - 0 - 100 = 200, split 1:1 => a=100, b=200
        [Fact]
        public void GrowWithBasis_AutoBasis_EmptyContent_GrowsFromZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex-grow:1;flex-basis:auto;height:30px'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"auto basis empty: a expected ~100 (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"basis:100 grow:1: b expected ~200 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] border-box items with border and padding
        // Two items in 300px: a(basis:100, grow:1, border-box, padding:10, border:5)
        //   and b(basis:100, grow:1)
        // border-box: a's basis=100 includes 30px (10+5 per side) of padding+border
        // Free = 300 - 100 - 100 = 100, split 1:1 => a border-box=150, b=150
        [Fact]
        public void GrowWithBasis_BorderBoxWithBorderAndPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 100px;height:30px;box-sizing:border-box;padding:10px;border:5px solid black'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            float borderBoxA = itemA!.ContentRect.Width + 30;
            _output.WriteLine($"a content={itemA.ContentRect.Width} border-box={borderBoxA} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(borderBoxA - 150) < 2,
                $"border-box a expected ~150 (got {borderBoxA})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"b expected ~150 (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] item positions: items placed sequentially from basis+grow widths
        // a(basis:50,grow:1) and b(basis:100,grow:1) in 300px => a=125 at x=0, b=175 at x=125
        [Fact]
        public void GrowWithBasis_ItemPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 50px;height:30px'></div>
                    <div id='b' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.X={itemA!.ContentRect.X} b.X={itemB!.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB!.ContentRect.X - 125) < 2,
                $"b starts after a: expected x~125 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] column positions: items stacked vertically with basis+grow heights
        // a(basis:50,grow:1) and b(basis:100,grow:1) in height:300 => a=125 at y=0, b=175 at y=125
        [Fact]
        public void GrowWithBasis_ColumnPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex:1 0 50px'></div>
                    <div id='b' style='flex:1 0 100px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.Y={itemA!.ContentRect.Y} b.Y={itemB!.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB!.ContentRect.Y - itemA.ContentRect.Y - 125) < 2,
                $"b starts after a: expected y offset ~125 (got {itemB.ContentRect.Y - itemA.ContentRect.Y})");
        }
    }
}
