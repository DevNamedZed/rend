using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Flexbox flex-grow space distribution conformance tests.
    /// Verifies grow factor ratios, constraint interactions, and edge cases.
    /// </summary>
    public class WptFlexGrowDistributionTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexGrowDistributionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.7] Single item with grow:1 fills container
        [Fact]
        public void SingleItem_Grow1_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-grow:1;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 400) < 2);
        }

        // [CSS-FLEXBOX §9.7] Two items grow:1 split space equally
        [Fact]
        public void TwoItems_Grow1_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 150) < 2);
            Assert.True(System.Math.Abs(widthB - 150) < 2);
        }

        // [CSS-FLEXBOX §9.7] Three items grow:1 split space equally
        [Fact]
        public void ThreeItems_Grow1_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='c' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            var widthC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}, c={widthC}");
            Assert.True(System.Math.Abs(widthA - 100) < 2);
            Assert.True(System.Math.Abs(widthB - 100) < 2);
            Assert.True(System.Math.Abs(widthC - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow 1:2 ratio distributes proportionally
        [Fact]
        public void TwoItems_Grow1And2_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='b' style='flex-grow:2;flex-basis:0px;height:30px'></div>
                </div></body>");
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 100) < 2);
            Assert.True(System.Math.Abs(widthB - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow 1:2:3 ratio distributes proportionally
        [Fact]
        public void ThreeItems_Grow123_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='b' style='flex-grow:2;flex-basis:0px;height:30px'></div>
                    <div id='c' style='flex-grow:3;flex-basis:0px;height:30px'></div>
                </div></body>");
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            var widthC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}, c={widthC}");
            Assert.True(System.Math.Abs(widthA - 100) < 2);
            Assert.True(System.Math.Abs(widthB - 200) < 2);
            Assert.True(System.Math.Abs(widthC - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow 1:1:2 ratio distributes proportionally
        [Fact]
        public void ThreeItems_Grow112_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='c' style='flex-grow:2;flex-basis:0px;height:30px'></div>
                </div></body>");
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            var widthC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}, c={widthC}");
            Assert.True(System.Math.Abs(widthA - 100) < 2);
            Assert.True(System.Math.Abs(widthB - 100) < 2);
            Assert.True(System.Math.Abs(widthC - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow distributes only remaining space after basis
        [Fact]
        public void GrowWithBasis_DistributesRemainingSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex-grow:1;flex-basis:100px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:100px;height:30px'></div>
                </div></body>");
            // Free space = 400 - 200 = 200. Each gets 100 extra → 200 each.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 200) < 2);
            Assert.True(System.Math.Abs(widthB - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow:0 means no growth
        [Fact]
        public void Grow0_NoGrowth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-grow:0;width:80px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-FLEXBOX §9.7] fractional grow factor 0.5 distributes half of free space
        [Fact]
        public void FractionalGrow_HalfFreeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex-grow:0.5;flex-basis:0px;height:30px'></div>
                    <div id='b' style='flex-grow:0.5;flex-basis:0px;height:30px'></div>
                </div></body>");
            // Total grow < 1 means only (total grow) fraction of free space distributed.
            // But grow 0.5+0.5=1.0, so full space is distributed equally.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 150) < 2);
            Assert.True(System.Math.Abs(widthB - 150) < 2);
        }

        // [CSS-FLEXBOX §9.7] gap reduces free space available for growth
        [Fact]
        public void GrowWithGap_ReducesFreeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;gap:20px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='c' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            // Free space = 400 - 0 - 2*20(gaps) = 360. Each gets 120.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            var widthC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}, c={widthC}");
            Assert.True(System.Math.Abs(widthA - 120) < 2);
            Assert.True(System.Math.Abs(widthB - 120) < 2);
            Assert.True(System.Math.Abs(widthC - 120) < 2);
        }

        // [CSS-FLEXBOX §9.7] max-width clamps grown size
        [Fact]
        public void GrowWithMaxWidth_Clamped()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;max-width:100px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            // a wants 200 but clamped to 100. b gets remaining 300.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 100) < 2);
            Assert.True(System.Math.Abs(widthB - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] min-width prevents shrink below minimum
        [Fact]
        public void GrowWithMinWidth_Enforced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;min-width:200px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            // a gets at least 200. b gets 100.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(widthA >= 198, $"a should be at least 200 (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] padding reduces free space for growth
        [Fact]
        public void GrowWithPadding_ReducesFreeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;padding:0 20px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            // a has 40px padding consuming space. Content widths differ.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a content={itemA.ContentRect.Width}, b content={itemB.ContentRect.Width}");
            float totalUsed = itemA.ContentRect.Width + 40 + itemB.ContentRect.Width;
            Assert.True(System.Math.Abs(totalUsed - 400) < 2,
                $"Total should fill container (got {totalUsed})");
        }

        // [CSS-FLEXBOX §9.7] border reduces free space for growth
        [Fact]
        public void GrowWithBorder_ReducesFreeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;border:10px solid black;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            // a has 20px border consuming space.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a content={itemA.ContentRect.Width}, b content={itemB.ContentRect.Width}");
            float totalUsed = itemA.ContentRect.Width + 20 + itemB.ContentRect.Width;
            Assert.True(System.Math.Abs(totalUsed - 400) < 2,
                $"Total should fill container (got {totalUsed})");
        }

        // [CSS-FLEXBOX §9.7] margin reduces free space for growth
        [Fact]
        public void GrowWithMargin_ReducesFreeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;margin:0 30px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            // a has 60px margin consuming free space.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a content={itemA.ContentRect.Width}, b content={itemB.ContentRect.Width}");
            float totalUsed = itemA.ContentRect.Width + 60 + itemB.ContentRect.Width;
            Assert.True(System.Math.Abs(totalUsed - 400) < 2,
                $"Total should fill container (got {totalUsed})");
        }

        // [CSS-FLEXBOX §9.7] column direction grow distributes height
        [Fact]
        public void ColumnDirection_GrowDistributesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px'></div>
                    <div id='c' style='flex-grow:1;flex-basis:0px'></div>
                </div></body>");
            var heightA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Height;
            var heightB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Height;
            var heightC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Height;
            _output.WriteLine($"a={heightA}, b={heightB}, c={heightC}");
            Assert.True(System.Math.Abs(heightA - 100) < 2);
            Assert.True(System.Math.Abs(heightB - 100) < 2);
            Assert.True(System.Math.Abs(heightC - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] equal grow with different basis values
        [Fact]
        public void EqualGrow_DifferentBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex-grow:1;flex-basis:50px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:150px;height:30px'></div>
                </div></body>");
            // Free space = 400 - 200 = 200. Each gets 100 extra.
            // a = 50 + 100 = 150, b = 150 + 100 = 250.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 150) < 2);
            Assert.True(System.Math.Abs(widthB - 250) < 2);
        }

        // [CSS-FLEXBOX §9.7] after max-width clamp, remaining space redistributed
        [Fact]
        public void MaxWidthClamp_RedistributesRemaining()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;max-width:100px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;max-width:100px;height:30px'></div>
                    <div id='c' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            // Initial: each gets 200. a clamped to 100, b clamped to 100. c gets 400.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            var widthC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}, c={widthC}");
            Assert.True(System.Math.Abs(widthA - 100) < 2);
            Assert.True(System.Math.Abs(widthB - 100) < 2);
            Assert.True(System.Math.Abs(widthC - 400) < 2);
        }

        // [CSS-FLEXBOX §9.7] box-sizing:border-box grow accounts for padding+border
        [Fact]
        public void GrowWithBorderBox_AccountsPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;box-sizing:border-box;padding:0 10px;border:5px solid black;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a content={itemA.ContentRect.Width}, b content={itemB.ContentRect.Width}");
            // Both items should fill the container collectively
            float borderBoxA = itemA.ContentRect.Width + 20 + 10; // padding 10*2 + border 5*2
            float totalUsed = borderBoxA + itemB.ContentRect.Width;
            Assert.True(System.Math.Abs(totalUsed - 400) < 2,
                $"Total should fill container (got {totalUsed})");
        }

        // [CSS-FLEXBOX §9.7] five items grow:1 each get 1/5 of container
        [Fact]
        public void FiveItems_Grow1_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='c' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='d' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='e' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            var widthC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Width;
            var widthD = LayoutTestHelper.FindById(root, "d")!.ContentRect.Width;
            var widthE = LayoutTestHelper.FindById(root, "e")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}, c={widthC}, d={widthD}, e={widthE}");
            Assert.True(System.Math.Abs(widthA - 100) < 2);
            Assert.True(System.Math.Abs(widthB - 100) < 2);
            Assert.True(System.Math.Abs(widthC - 100) < 2);
            Assert.True(System.Math.Abs(widthD - 100) < 2);
            Assert.True(System.Math.Abs(widthE - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] extreme ratio grow 1:99
        [Fact]
        public void LargeRatio_Grow1And99()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                    <div id='b' style='flex-grow:99;flex-basis:0px;height:30px'></div>
                </div></body>");
            // Total grow = 100. a = 400/100 = 4. b = 400*99/100 = 396.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 4) < 2);
            Assert.True(System.Math.Abs(widthB - 396) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow:0 items stay at basis while grow>0 items expand
        [Fact]
        public void MixedGrow_ZeroAndNonZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex-grow:0;flex-basis:100px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            // a stays at 100. b gets 300.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 100) < 2);
            Assert.True(System.Math.Abs(widthB - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] column grow with gap reduces free space
        [Fact]
        public void ColumnGrow_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:400px;width:100px;gap:20px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px'></div>
                </div></body>");
            // Free space = 400 - 1*20(gap) = 380. Each gets 190.
            var heightA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Height;
            var heightB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Height;
            _output.WriteLine($"a={heightA}, b={heightB}");
            Assert.True(System.Math.Abs(heightA - 190) < 2);
            Assert.True(System.Math.Abs(heightB - 190) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow with width set — basis:auto resolves to width
        [Fact]
        public void GrowWithExplicitWidth_BasisAuto()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex-grow:1;width:60px;height:30px'></div>
                    <div id='b' style='flex-grow:1;width:40px;height:30px'></div>
                </div></body>");
            // Basis auto → uses width. Free space = 400 - 100 = 300. Each gets 150 extra.
            // a = 60 + 150 = 210. b = 40 + 150 = 190.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 210) < 2);
            Assert.True(System.Math.Abs(widthB - 190) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow with all items having same non-zero basis
        [Fact]
        public void GrowWithSameBasis_EqualGrowth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex-grow:1;flex-basis:80px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:80px;height:30px'></div>
                </div></body>");
            // Free space = 400 - 160 = 240. Each gets 120 extra → 200 each.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 200) < 2);
            Assert.True(System.Math.Abs(widthB - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow with max-width on all items — no growth possible
        [Fact]
        public void GrowWithMaxWidth_AllClamped()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;max-width:50px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;max-width:50px;height:30px'></div>
                </div></body>");
            // Both clamped to 50. Remaining 300 cannot be distributed.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 50) < 2);
            Assert.True(System.Math.Abs(widthB - 50) < 2);
        }

        // [CSS-FLEXBOX §9.7] flex shorthand grow:1 0 0px
        [Fact]
        public void FlexShorthand_Grow1()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            // flex:1 → flex-grow:1 flex-shrink:1 flex-basis:0. Equal split.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            var widthC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}, c={widthC}");
            Assert.True(System.Math.Abs(widthA - 100) < 2);
            Assert.True(System.Math.Abs(widthB - 100) < 2);
            Assert.True(System.Math.Abs(widthC - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] flex shorthand with integer → grow:N
        [Fact]
        public void FlexShorthand_Grow2And3()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='a' style='flex:2;height:30px'></div>
                    <div id='b' style='flex:3;height:30px'></div>
                </div></body>");
            // flex:2 → grow:2 basis:0. flex:3 → grow:3 basis:0.
            // a = 500*2/5 = 200. b = 500*3/5 = 300.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 200) < 2);
            Assert.True(System.Math.Abs(widthB - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow does not expand beyond container when no free space
        [Fact]
        public void Grow_NoFreeSpace_NoExpansion()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex-grow:1;flex-basis:100px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:100px;height:30px'></div>
                </div></body>");
            // Basis totals 200, container is 200. No free space → no growth.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 100) < 2);
            Assert.True(System.Math.Abs(widthB - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow with min-width larger than proportional share
        [Fact]
        public void GrowWithMinWidth_LargerThanShare()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex-grow:1;flex-basis:0px;min-width:250px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            // a proportional = 150 but min-width = 250. b gets 50.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(widthA >= 248, $"a min-width enforced (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 50) < 2);
        }

        // [CSS-FLEXBOX §9.7] column grow distributes height with basis
        [Fact]
        public void ColumnGrow_WithBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:400px;width:100px'>
                    <div id='a' style='flex-grow:1;flex-basis:50px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:150px'></div>
                </div></body>");
            // Free space = 400 - 200 = 200. Each gets 100 extra.
            // a = 50 + 100 = 150. b = 150 + 100 = 250.
            var heightA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Height;
            var heightB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Height;
            _output.WriteLine($"a={heightA}, b={heightB}");
            Assert.True(System.Math.Abs(heightA - 150) < 2);
            Assert.True(System.Math.Abs(heightB - 250) < 2);
        }
    }
}
