using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Flex shrink distribution, weighted shrink, min-width clamping,
    /// and interactions with padding/border/margin/gap/box-sizing.
    /// </summary>
    public class WptFlexShrinkClampTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexShrinkClampTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX 9.7] Two items with shrink:1 shrink equally
        [Fact]
        public void ShrinkEqual_TwoItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;height:20px'></div>
                    <div id='b' style='flex:0 1 150px;height:20px'></div>
                </div></body>");
            // Overflow=100. Equal basis, equal shrink. Each shrinks 50 -> 100px.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 100) < 2, $"a should be ~100 (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 100) < 2, $"b should be ~100 (got {widthB})");
        }

        // [CSS-FLEXBOX 9.7] Three items with shrink:1 shrink equally
        [Fact]
        public void ShrinkEqual_ThreeItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0 1 200px;height:20px'></div>
                    <div id='b' style='flex:0 1 200px;height:20px'></div>
                    <div id='c' style='flex:0 1 200px;height:20px'></div>
                </div></body>");
            // Overflow=300. Each basis=200, shrink=1. Weighted=200 each, total=600.
            // Each shrinks 300*200/600=100 -> 100px.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            var widthC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}, c={widthC}");
            Assert.True(System.Math.Abs(widthA - 100) < 2, $"a should be ~100 (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 100) < 2, $"b should be ~100 (got {widthB})");
            Assert.True(System.Math.Abs(widthC - 100) < 2, $"c should be ~100 (got {widthC})");
        }

        // [CSS-FLEXBOX 9.7] Weighted shrink 1:2 with equal basis
        [Fact]
        public void ShrinkWeighted_1To2()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 200px;height:20px'></div>
                    <div id='b' style='flex:0 2 200px;height:20px'></div>
                </div></body>");
            // Overflow=200. a: 1*200=200, b: 2*200=400. Total=600.
            // a shrinks 200*200/600=66.67 -> 133.33. b shrinks 200*400/600=133.33 -> 66.67.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 133.33f) < 3, $"a should be ~133 (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 66.67f) < 3, $"b should be ~67 (got {widthB})");
        }

        // [CSS-FLEXBOX 9.7] shrink:0 prevents any shrinking
        [Fact]
        public void ShrinkZero_NoShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 0 150px;height:20px'></div>
                    <div id='b' style='flex:0 1 150px;height:20px'></div>
                </div></body>");
            // a won't shrink. b absorbs all overflow (100) -> 50px.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 150) < 2, $"a should stay at basis 150 (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 50) < 2, $"b should shrink to ~50 (got {widthB})");
        }

        // [CSS-FLEXBOX 9.7] min-width:0 allows full shrink past content size
        [Fact]
        public void ShrinkWithMinWidthZero_AllowsFullShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex:0 1 200px;min-width:0;height:20px'></div>
                </div></body>");
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}");
            Assert.True(System.Math.Abs(widthA - 100) < 2, $"min-width:0 allows shrink to container (got {widthA})");
        }

        // [CSS-FLEXBOX 9.7] min-width clamps shrink
        [Fact]
        public void ShrinkWithMinWidth_Clamps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 180px;min-width:150px;height:20px'></div>
                    <div id='b' style='flex:0 1 120px;height:20px'></div>
                </div></body>");
            // Overflow=100. a clamped at 150 (can only shrink 30). b absorbs the rest -> 50.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(widthA >= 149, $"a should be clamped at min-width 150 (got {widthA})");
        }

        // [CSS-FLEXBOX 9.7] Larger basis shrinks more with equal shrink factors
        [Fact]
        public void ShrinkWeightedByBasis_LargerBasisShrinksMore()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 100px;height:20px'></div>
                    <div id='b' style='flex:0 1 200px;height:20px'></div>
                </div></body>");
            // Overflow=100. a: 1*100=100, b: 1*200=200. Total=300.
            // a shrinks 100*100/300=33.33 -> 66.67. b shrinks 100*200/300=66.67 -> 133.33.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 66.67f) < 3, $"a should be ~67 (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 133.33f) < 3, $"b should be ~133 (got {widthB})");
        }

        // [CSS-FLEXBOX 9.7] Different basis sizes with different shrink factors
        [Fact]
        public void ShrinkDifferentBasisAndFactors()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 100px;height:20px'></div>
                    <div id='b' style='flex:0 2 150px;height:20px'></div>
                    <div id='c' style='flex:0 3 50px;height:20px'></div>
                </div></body>");
            // Overflow=100. Scaled: a=1*100=100, b=2*150=300, c=3*50=150. Total=550.
            // a shrinks 100*100/550=18.18 -> 81.82
            // b shrinks 100*300/550=54.55 -> 95.45
            // c shrinks 100*150/550=27.27 -> 22.73
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            var widthC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}, c={widthC}");
            Assert.True(System.Math.Abs(widthA - 81.82f) < 3, $"a should be ~82 (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 95.45f) < 3, $"b should be ~95 (got {widthB})");
            Assert.True(System.Math.Abs(widthC - 22.73f) < 3, $"c should be ~23 (got {widthC})");
        }

        // [CSS-FLEXBOX 9.7] Shrink never produces negative width
        [Fact]
        public void ShrinkNeverBelowZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:50px'>
                    <div id='a' style='flex:0 1 200px;min-width:0;height:20px'></div>
                    <div id='b' style='flex:0 1 200px;min-width:0;height:20px'></div>
                </div></body>");
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(widthA >= 0, $"a should never be negative (got {widthA})");
            Assert.True(widthB >= 0, $"b should never be negative (got {widthB})");
        }

        // [CSS-FLEXBOX 9.7] Shrink with padding (content-box by default)
        [Fact]
        public void ShrinkWithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;padding:10px;height:20px'></div>
                    <div id='b' style='flex:0 1 150px;height:20px'></div>
                </div></body>");
            // a basis=150 content + 20 padding = 170 outer. b basis=150. Total=320. Overflow=120.
            // Padding is not shrinkable, but basis is content-box.
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            _output.WriteLine($"a content={boxA.ContentRect.Width}, padding L/R={boxA.PaddingRect.Width}");
            Assert.True(boxA.ContentRect.Width > 0, "a content width should be positive");
            Assert.True(boxA.ContentRect.Width < 150, "a should have shrunk from 150 basis");
        }

        // [CSS-FLEXBOX 9.7] Shrink with border (content-box by default)
        [Fact]
        public void ShrinkWithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;border:5px solid black;height:20px'></div>
                    <div id='b' style='flex:0 1 150px;height:20px'></div>
                </div></body>");
            // a outer = 150 + 10 = 160. b = 150. Total=310. Overflow=110.
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            _output.WriteLine($"a content={boxA.ContentRect.Width}, border={boxA.BorderRect.Width}");
            Assert.True(boxA.ContentRect.Width > 0, "a content width should be positive");
            Assert.True(boxA.ContentRect.Width < 150, "a should have shrunk from 150 basis");
        }

        // [CSS-FLEXBOX 9.7] Shrink with border-box: basis includes padding+border
        [Fact]
        public void ShrinkWithBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;box-sizing:border-box;padding:10px;border:5px solid black;height:40px'></div>
                    <div id='b' style='flex:0 1 150px;height:20px'></div>
                </div></body>");
            // With border-box, basis 150 includes padding+border. Total outer=300. Overflow=100.
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var borderWidth = boxA.BorderRect.Width;
            _output.WriteLine($"a border-box width={borderWidth}, content={boxA.ContentRect.Width}");
            Assert.True(borderWidth < 150, "a border-box width should have shrunk from 150");
            Assert.True(borderWidth > 0, "a should still have positive width");
        }

        // [CSS-FLEXBOX 9.7] Shrink with margin
        [Fact]
        public void ShrinkWithMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 120px;margin:0 10px;height:20px'></div>
                    <div id='b' style='flex:0 1 120px;height:20px'></div>
                </div></body>");
            // a outer=120+20=140. b=120. Total=260. Overflow=60.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(widthA < 120, "a should have shrunk from 120 basis");
            Assert.True(widthA + widthB + 20 <= 202, "total outer widths should fit in 200px container");
        }

        // [CSS-FLEXBOX 9.7] Column flex-direction shrink
        [Fact]
        public void ColumnShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:100px'>
                    <div id='a' style='flex:0 1 150px;min-height:0'></div>
                    <div id='b' style='flex:0 1 150px;min-height:0'></div>
                </div></body>");
            // Overflow=100. Equal shrink. Each -> 100px.
            var heightA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Height;
            var heightB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Height;
            _output.WriteLine($"a={heightA}, b={heightB}");
            Assert.True(System.Math.Abs(heightA - 100) < 2, $"a should be ~100 (got {heightA})");
            Assert.True(System.Math.Abs(heightB - 100) < 2, $"b should be ~100 (got {heightB})");
        }

        // [CSS-FLEXBOX 9.7] shrink:0 preserves basis exactly
        [Fact]
        public void ShrinkZero_PreservesBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 0 180px;height:20px'></div>
                    <div id='b' style='flex:0 0 180px;height:20px'></div>
                </div></body>");
            // Both have shrink:0. Neither shrinks. They overflow container.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 180) < 2, $"a should stay at 180 (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 180) < 2, $"b should stay at 180 (got {widthB})");
        }

        // [CSS-FLEXBOX 9.7] Fractional shrink factor 0.5
        [Fact]
        public void ShrinkFractional_Half()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 0.5 200px;height:20px'></div>
                    <div id='b' style='flex:0 1 200px;height:20px'></div>
                </div></body>");
            // Overflow=200. a: 0.5*200=100, b: 1*200=200. Total=300.
            // a shrinks 200*100/300=66.67 -> 133.33. b shrinks 200*200/300=133.33 -> 66.67.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 133.33f) < 3, $"a should be ~133 (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 66.67f) < 3, $"b should be ~67 (got {widthB})");
        }

        // [CSS-FLEXBOX 9.7] Large shrink factor (10x vs 1x)
        [Fact]
        public void ShrinkLargeFactor()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 200px;height:20px'></div>
                    <div id='b' style='flex:0 10 200px;height:20px'></div>
                </div></body>");
            // Overflow=200. a: 1*200=200, b: 10*200=2000. Total=2200.
            // a shrinks 200*200/2200=18.18 -> 181.82. b shrinks 200*2000/2200=181.82 -> 18.18.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 181.82f) < 3, $"a should be ~182 (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 18.18f) < 3, $"b should be ~18 (got {widthB})");
        }

        // [CSS-FLEXBOX 9.7] Shrink with gap consuming space
        [Fact]
        public void ShrinkWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;gap:20px'>
                    <div id='a' style='flex:0 1 150px;height:20px'></div>
                    <div id='b' style='flex:0 1 150px;height:20px'></div>
                </div></body>");
            // Available after gap: 200-20=180. Total basis=300. Overflow=120.
            // Each shrinks equally: 60 -> 90px each.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 90) < 3, $"a should be ~90 (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 90) < 3, $"b should be ~90 (got {widthB})");
        }

        // [CSS-FLEXBOX 9.7] Mixed shrink:0 and shrink:1
        [Fact]
        public void MixedShrinkZeroAndOne()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 0 120px;height:20px'></div>
                    <div id='b' style='flex:0 1 120px;height:20px'></div>
                    <div id='c' style='flex:0 1 120px;height:20px'></div>
                </div></body>");
            // a won't shrink. b and c share overflow. Total basis=360. Available for b+c = 200-120=80.
            // b and c each: equal basis, equal shrink -> 40px each.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            var widthC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}, c={widthC}");
            Assert.True(System.Math.Abs(widthA - 120) < 2, $"a should stay at 120 (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 40) < 3, $"b should be ~40 (got {widthB})");
            Assert.True(System.Math.Abs(widthC - 40) < 3, $"c should be ~40 (got {widthC})");
        }

        // [CSS-FLEXBOX 9.7] Three items with different shrink factors
        [Fact]
        public void ThreeItems_DifferentShrinkFactors()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 200px;height:20px'></div>
                    <div id='b' style='flex:0 2 200px;height:20px'></div>
                    <div id='c' style='flex:0 3 200px;height:20px'></div>
                </div></body>");
            // Overflow=400. a: 1*200=200, b: 2*200=400, c: 3*200=600. Total=1200.
            // a shrinks 400*200/1200=66.67 -> 133.33
            // b shrinks 400*400/1200=133.33 -> 66.67
            // c shrinks 400*600/1200=200 -> 0 (but clamped at min-width auto)
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            var widthC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}, c={widthC}");
            Assert.True(widthA > widthB, $"a should be wider than b (a={widthA}, b={widthB})");
            Assert.True(widthB > widthC, $"b should be wider than c (b={widthB}, c={widthC})");
            Assert.True(widthC >= 0, $"c should not be negative (got {widthC})");
        }

        // [CSS-FLEXBOX 9.7] Shrink below basis but above min-width
        [Fact]
        public void ShrinkBelowBasis_AboveMinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 160px;min-width:80px;height:20px'></div>
                    <div id='b' style='flex:0 1 160px;min-width:40px;height:20px'></div>
                </div></body>");
            // Overflow=120. Without clamping: each shrinks 60 -> 100px. Both above min, so fine.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(widthA >= 80, $"a should be >= min-width 80 (got {widthA})");
            Assert.True(widthB >= 40, $"b should be >= min-width 40 (got {widthB})");
            Assert.True(widthA < 160, $"a should have shrunk below basis 160 (got {widthA})");
            Assert.True(widthB < 160, $"b should have shrunk below basis 160 (got {widthB})");
        }

        // [CSS-FLEXBOX 9.7] Column shrink with gap
        [Fact]
        public void ColumnShrinkWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:100px;gap:20px'>
                    <div id='a' style='flex:0 1 150px;min-height:0'></div>
                    <div id='b' style='flex:0 1 150px;min-height:0'></div>
                </div></body>");
            // Available after gap: 200-20=180. Total basis=300. Overflow=120.
            // Each shrinks equally: 60 -> 90px.
            var heightA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Height;
            var heightB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Height;
            _output.WriteLine($"a={heightA}, b={heightB}");
            Assert.True(System.Math.Abs(heightA - 90) < 3, $"a should be ~90 (got {heightA})");
            Assert.True(System.Math.Abs(heightB - 90) < 3, $"b should be ~90 (got {heightB})");
        }

        // [CSS-FLEXBOX 9.7] Shrink with padding and border-box sizing
        [Fact]
        public void ShrinkWithPaddingBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;box-sizing:border-box;padding:20px;height:60px'></div>
                    <div id='b' style='flex:0 1 150px;height:20px'></div>
                </div></body>");
            // border-box: a outer basis=150 (includes padding). b content basis=150.
            // Weighted shrink uses flex-basis * shrink-factor; border-box basis is content portion.
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            var borderWidth = boxA.BorderRect.Width;
            _output.WriteLine($"a border={borderWidth}, content={boxA.ContentRect.Width}, b={widthB}");
            Assert.True(borderWidth < 150, $"a should have shrunk from 150 (got {borderWidth})");
            Assert.True(boxA.ContentRect.Width > 0, "a content width should be positive after padding subtracted");
            Assert.True(borderWidth + widthB <= 202, $"total should fit in container (got {borderWidth + widthB})");
        }

        // [CSS-FLEXBOX 9.7] Single item shrinks to container
        [Fact]
        public void SingleItemShrinks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex:0 1 300px;min-width:0;height:20px'></div>
                </div></body>");
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}");
            Assert.True(System.Math.Abs(widthA - 100) < 2, $"single item should shrink to container (got {widthA})");
        }

        // [CSS-FLEXBOX 9.7] Shrink sum width equals container
        [Fact]
        public void ShrinkSumMatchesContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0 1 200px;min-width:0;height:20px'></div>
                    <div id='b' style='flex:0 2 200px;min-width:0;height:20px'></div>
                    <div id='c' style='flex:0 1 200px;min-width:0;height:20px'></div>
                </div></body>");
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            var widthC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Width;
            var total = widthA + widthB + widthC;
            _output.WriteLine($"a={widthA}, b={widthB}, c={widthC}, total={total}");
            Assert.True(System.Math.Abs(total - 300) < 3, $"item widths should sum to container (got {total})");
        }

        // [CSS-FLEXBOX 9.7] Column shrink with min-height clamping
        [Fact]
        public void ColumnShrink_MinHeightClamps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:100px;width:100px'>
                    <div id='a' style='flex:0 1 80px;min-height:60px'></div>
                    <div id='b' style='flex:0 1 80px;min-height:0'></div>
                </div></body>");
            // Overflow=60. a clamped at min-height 60 (can shrink 20). b absorbs the rest.
            var heightA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Height;
            var heightB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Height;
            _output.WriteLine($"a={heightA}, b={heightB}");
            Assert.True(heightA >= 59, $"a should be clamped at min-height 60 (got {heightA})");
            Assert.True(heightB < 80, $"b should have shrunk (got {heightB})");
        }

        // [CSS-FLEXBOX 9.7] Shrink with border and padding combined
        [Fact]
        public void ShrinkWithBorderAndPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 120px;padding:5px;border:3px solid black;height:20px'></div>
                    <div id='b' style='flex:0 1 120px;height:20px'></div>
                </div></body>");
            // a outer = 120 + 10 + 6 = 136. b = 120. Total = 256. Overflow = 56.
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            _output.WriteLine($"a content={boxA.ContentRect.Width}, border={boxA.BorderRect.Width}");
            Assert.True(boxA.ContentRect.Width > 0, "content should be positive");
            Assert.True(boxA.ContentRect.Width < 120, "should have shrunk from 120 basis");
        }

        // [CSS-FLEXBOX 9.7] Shrink:0 items overflow container
        [Fact]
        public void ShrinkZero_AllItems_Overflow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;width:100px'>
                    <div id='a' style='flex:0 0 80px;height:20px'></div>
                    <div id='b' style='flex:0 0 80px;height:20px'></div>
                </div></body>");
            // No shrinking at all. Items overflow to 160px total.
            var widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            var widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            _output.WriteLine($"a={widthA}, b={widthB}");
            Assert.True(System.Math.Abs(widthA - 80) < 2, $"a should stay at 80 (got {widthA})");
            Assert.True(System.Math.Abs(widthB - 80) < 2, $"b should stay at 80 (got {widthB})");
        }
    }
}
