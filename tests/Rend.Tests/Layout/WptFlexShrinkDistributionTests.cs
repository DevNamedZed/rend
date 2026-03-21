using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Flexbox flex-shrink space distribution per CSS-FLEXBOX §9.7.
    /// Shrink is weighted by flex-shrink * flex-basis (scaled flex shrink factor).
    /// </summary>
    public class WptFlexShrinkDistributionTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexShrinkDistributionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.7] Two items with shrink:1 and equal basis shrink equally
        [Fact]
        public void ShrinkEqual_TwoItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;height:30px'></div>
                    <div id='b' style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            // Overflow=100. Scaled: a=1*150=150, b=1*150=150. Total=300.
            // Each shrinks 100*150/300=50 → 100px each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] Three items with shrink:1 and equal basis shrink equally
        [Fact]
        public void ShrinkEqual_ThreeItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0 1 200px;height:30px'></div>
                    <div id='b' style='flex:0 1 200px;height:30px'></div>
                    <div id='c' style='flex:0 1 200px;height:30px'></div>
                </div></body>");
            // Overflow=300. Scaled: each=1*200=200. Total=600.
            // Each shrinks 300*200/600=100 → 100px each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width} c={itemC.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] Weighted shrink 1:2 with equal basis
        [Fact]
        public void ShrinkWeighted_OneToTwo()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;height:30px'></div>
                    <div id='b' style='flex:0 2 150px;height:30px'></div>
                </div></body>");
            // Overflow=100. Scaled: a=1*150=150, b=2*150=300. Total=450.
            // a shrinks 100*150/450≈33.33 → ~116.67. b shrinks 100*300/450≈66.67 → ~83.33.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 116.67f) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 83.33f) < 2);
        }

        // [CSS-FLEXBOX §9.7] flex-shrink:0 prevents shrinking
        [Fact]
        public void ShrinkZero_NoShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 0 150px;height:30px'></div>
                    <div id='b' style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            // Only b shrinks. a stays 150. b shrinks all 100 → 50.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 50) < 2);
        }

        // [CSS-FLEXBOX §4.5] min-width:0 allows items to shrink below content size
        [Fact]
        public void ShrinkWithMinWidthZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex:0 1 200px;min-width:0;height:30px'></div>
                    <div id='b' style='flex:0 1 200px;min-width:0;height:30px'></div>
                </div></body>");
            // Overflow=300. Equal scaled factors → each shrinks 150 → 50px each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 50) < 2);
        }

        // [CSS-FLEXBOX §9.7] min-width clamps shrink — item cannot go below min-width
        [Fact]
        public void ShrinkWithMinWidthClamp()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;min-width:120px;height:30px'></div>
                    <div id='b' style='flex:0 1 150px;min-width:0;height:30px'></div>
                </div></body>");
            // Overflow=100. a clamped at 120 (can only shrink 30). b absorbs remaining.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(itemA.ContentRect.Width >= 119, $"a clamped at min-width (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 80) < 2);
        }

        // [CSS-FLEXBOX §9.7] Shrink weighted by different basis sizes
        [Fact]
        public void ShrinkWeightedByBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 100px;height:30px'></div>
                    <div id='b' style='flex:0 1 200px;height:30px'></div>
                </div></body>");
            // Overflow=100. Scaled: a=1*100=100, b=1*200=200. Total=300.
            // a shrinks 100*100/300≈33.33 → ~66.67. b shrinks 100*200/300≈66.67 → ~133.33.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 66.67f) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 133.33f) < 2);
        }

        // [CSS-FLEXBOX §9.7] Shrink with different basis sizes and different factors
        [Fact]
        public void ShrinkDifferentBasisAndFactors()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 2 100px;height:30px'></div>
                    <div id='b' style='flex:0 1 200px;height:30px'></div>
                </div></body>");
            // Overflow=100. Scaled: a=2*100=200, b=1*200=200. Total=400.
            // a shrinks 100*200/400=50 → 50. b shrinks 100*200/400=50 → 150.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2);
        }

        // [CSS-FLEXBOX §9.3] Padding is part of the flex item outer size
        [Fact]
        public void ShrinkWithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;padding:10px;height:30px'></div>
                    <div id='b' style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            // a's outer hypothetical = 150+20=170, b's outer = 150. Total = 320. Overflow = 120.
            // But flex-basis is the content-box basis. Shrink scaled by basis.
            // Scaled: a=1*150, b=1*150. Total=300. Each shrinks 120*150/300=60.
            // a content=90, b content=90.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 90) < 2);
        }

        // [CSS-FLEXBOX §9.3] Border is part of the flex item outer size
        [Fact]
        public void ShrinkWithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;border:5px solid black;height:30px'></div>
                    <div id='b' style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            // a's outer = 150+10=160, b's outer = 150. Total = 310. Overflow = 110.
            // Scaled: a=1*150, b=1*150. Total=300. Each shrinks 110*150/300=55.
            // a content=95, b content=95.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 95) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 95) < 2);
        }

        // [CSS-FLEXBOX §9.3] Margin is part of the flex item outer size
        [Fact]
        public void ShrinkWithMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;margin:0 10px;height:30px'></div>
                    <div id='b' style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            // a's outer = 150+20=170, b's outer = 150. Total = 320. Overflow = 120.
            // Scaled: a=1*150, b=1*150. Total=300. Each shrinks 120*150/300=60.
            // a content=90, b content=90.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 90) < 2);
        }

        // [CSS-FLEXBOX §9.7] Column direction shrink applies to height
        [Fact]
        public void ColumnShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:100px;width:200px'>
                    <div id='a' style='flex:0 1 80px;min-height:0'></div>
                    <div id='b' style='flex:0 1 80px;min-height:0'></div>
                </div></body>");
            // Overflow=60. Equal scaled → each shrinks 30 → 50px each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.h={itemA.ContentRect.Height} b.h={itemB.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 50) < 2);
        }

        // [CSS-FLEXBOX §9.7] max-width clamps flex item during shrink
        [Fact]
        public void ShrinkWithMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 200px;max-width:80px;height:30px'></div>
                    <div id='b' style='flex:0 1 200px;height:30px'></div>
                </div></body>");
            // a is clamped to max-width:80. b takes remaining = 120.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(itemA.ContentRect.Width <= 81, $"a clamped by max-width (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 120) < 2);
        }

        // [CSS-FLEXBOX §9.7] Negative flex-shrink is invalid per CSS spec, reverts to default (1)
        [Fact]
        public void ShrinkNegative_InvalidUsesDefault()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex-shrink:-1;flex-basis:150px;height:30px'></div>
                    <div id='b' style='flex-shrink:1;flex-basis:150px;height:30px'></div>
                </div></body>");
            // Negative shrink is invalid CSS, parser ignores it → default shrink:1.
            // Both items shrink equally: overflow=100, each shrinks 50 → 100px.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] Fractional flex-shrink values
        [Fact]
        public void ShrinkFractional()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 0.5 150px;height:30px'></div>
                    <div id='b' style='flex:0 1.5 150px;height:30px'></div>
                </div></body>");
            // Overflow=100. Scaled: a=0.5*150=75, b=1.5*150=225. Total=300.
            // a shrinks 100*75/300=25 → 125. b shrinks 100*225/300=75 → 75.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 125) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 75) < 2);
        }

        // [CSS-FLEXBOX §9.7] All items shrink to min-width when overflow exceeds capacity
        [Fact]
        public void ShrinkAllToMinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:60px'>
                    <div id='a' style='flex:0 1 200px;min-width:40px;height:30px'></div>
                    <div id='b' style='flex:0 1 200px;min-width:40px;height:30px'></div>
                </div></body>");
            // Container=60. Total basis=400. Overflow=340. Equal scaled factors.
            // Each would shrink to 30 but min-width:40 clamps. Both stop at 40.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 40) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 40) < 2);
        }

        // [CSS-FLEXBOX §9.7] Shrink with gap reduces available space
        [Fact]
        public void ShrinkWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;gap:20px'>
                    <div id='a' style='flex:0 1 150px;height:30px'></div>
                    <div id='b' style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            // Available=200, gap=20. Usable=180. Total basis=300. Overflow=120.
            // Scaled: a=1*150, b=1*150. Total=300. Each shrinks 120*150/300=60 → 90 each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 90) < 2);
        }

        // [CSS-FLEXBOX §9.7] Only one item has shrink > 0
        [Fact]
        public void ShrinkOneItemOnly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 0 120px;height:30px'></div>
                    <div id='b' style='flex:0 1 120px;height:30px'></div>
                    <div id='c' style='flex:0 0 120px;height:30px'></div>
                </div></body>");
            // Only b shrinks. Total outer = 360. Overflow = 160.
            // a=120, c=120, b=120-160=-40 → clamped at 0 or min-width auto.
            // b absorbs all overflow.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width} c={itemC.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 120) < 2);
            Assert.True(itemB.ContentRect.Width < itemA.ContentRect.Width,
                $"b should shrink (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] box-sizing:border-box — basis includes padding+border
        [Fact]
        public void ShrinkWithBoxSizingBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;box-sizing:border-box;padding:10px;height:30px'></div>
                    <div id='b' style='flex:0 1 150px;box-sizing:border-box;padding:10px;height:30px'></div>
                </div></body>");
            // border-box: basis=150 includes padding. Outer = 150 each. Total=300. Overflow=100.
            // Each shrinks 50 → outer=100, content=100-20=80.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 80) < 2);
        }

        // [CSS-FLEXBOX §4.5] Explicit min-width preserves minimum size during shrink
        [Fact]
        public void ShrinkPreservesExplicitMinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex:0 1 200px;min-width:80px;height:30px'></div>
                    <div id='b' style='flex:0 1 200px;min-width:0;height:30px'></div>
                </div></body>");
            // a has min-width:80. b has min-width:0.
            // Overflow=300. a clamped at 80 (shrinks max 120). b absorbs remaining 180 → 20.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(itemA.ContentRect.Width >= 79,
                $"min-width:80 preserves size (got {itemA.ContentRect.Width})");
            Assert.True(itemB.ContentRect.Width < itemA.ContentRect.Width,
                $"b should be smaller than a (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Three items with shrink 1:2:3 and equal basis
        [Fact]
        public void ShrinkWeighted_OneTwoThree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0 1 200px;height:30px'></div>
                    <div id='b' style='flex:0 2 200px;height:30px'></div>
                    <div id='c' style='flex:0 3 200px;height:30px'></div>
                </div></body>");
            // Overflow=300. Scaled: a=1*200=200, b=2*200=400, c=3*200=600. Total=1200.
            // a shrinks 300*200/1200=50 → 150. b shrinks 300*400/1200=100 → 100. c shrinks 300*600/1200=150 → 50.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width} c={itemC.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 50) < 2);
        }

        // [CSS-FLEXBOX §9.7] Large basis difference — item with larger basis shrinks more
        [Fact]
        public void ShrinkLargeBasisDifference()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 50px;height:30px'></div>
                    <div id='b' style='flex:0 1 250px;height:30px'></div>
                </div></body>");
            // Overflow=100. Scaled: a=1*50=50, b=1*250=250. Total=300.
            // a shrinks 100*50/300≈16.67 → ~33.33. b shrinks 100*250/300≈83.33 → ~166.67.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 33.33f) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 166.67f) < 2);
        }

        // [CSS-FLEXBOX §9.7] Column shrink with gap
        [Fact]
        public void ColumnShrinkWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:100px;width:200px;gap:10px'>
                    <div id='a' style='flex:0 1 80px;min-height:0'></div>
                    <div id='b' style='flex:0 1 80px;min-height:0'></div>
                </div></body>");
            // Available=100, gap=10. Usable=90. Total basis=160. Overflow=70.
            // Each shrinks 35 → 45px each.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.h={itemA.ContentRect.Height} b.h={itemB.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 45) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 45) < 2);
        }

        // [CSS-FLEXBOX §9.7] No overflow means no shrink applied
        [Fact]
        public void NoOverflow_NoShrinkApplied()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:0 1 100px;height:30px'></div>
                    <div id='b' style='flex:0 1 100px;height:30px'></div>
                </div></body>");
            // Total=200 < 400. No overflow, no shrink.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] Shrink with both padding and border combined
        [Fact]
        public void ShrinkWithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;padding:5px;border:5px solid black;height:30px'></div>
                    <div id='b' style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            // a outer = 150+10+10=170, b outer = 150. Total=320. Overflow=120.
            // Scaled: a=1*150, b=1*150. Total=300. Each shrinks 60.
            // a content=90, b content=90.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 90) < 2);
        }

        // [CSS-FLEXBOX §9.7] Shrink factor 0 on all items — no shrinking occurs
        [Fact]
        public void ShrinkZeroOnAll_NoShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 0 150px;height:30px'></div>
                    <div id='b' style='flex:0 0 150px;height:30px'></div>
                </div></body>");
            // Both have shrink:0. Items overflow container.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2);
        }

        // [CSS-FLEXBOX §9.7] Very large shrink factor on one item
        [Fact]
        public void ShrinkVeryLargeFactor()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;height:30px'></div>
                    <div id='b' style='flex:0 100 150px;height:30px'></div>
                </div></body>");
            // Overflow=100. Scaled: a=1*150=150, b=100*150=15000. Total=15150.
            // a shrinks 100*150/15150≈0.99 → ~149. b shrinks 100*15000/15150≈99.01 → ~51.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 149.01f) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 50.99f) < 2);
        }

        // [CSS-FLEXBOX §9.7] Shrink with basis:0 — item has no hypothetical size to shrink from
        [Fact]
        public void ShrinkWithBasisZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 0px;height:30px'></div>
                    <div id='b' style='flex:0 1 300px;height:30px'></div>
                </div></body>");
            // a has basis 0, so scaled shrink factor = 1*0 = 0. a stays near 0.
            // b has all the scaled factor. Overflow=100. b shrinks 100 → 200.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width}");
            Assert.True(itemA.ContentRect.Width <= 0 || float.IsNaN(itemA.ContentRect.Width),
                $"basis:0 item has zero or degenerate width (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] Four items with different shrink factors
        [Fact]
        public void ShrinkFourItems_DifferentFactors()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 100px;height:30px'></div>
                    <div id='b' style='flex:0 2 100px;height:30px'></div>
                    <div id='c' style='flex:0 3 100px;height:30px'></div>
                    <div id='d' style='flex:0 4 100px;height:30px'></div>
                </div></body>");
            // Overflow=200. Scaled: a=100, b=200, c=300, d=400. Total=1000.
            // a shrinks 200*100/1000=20 → 80. b shrinks 40 → 60. c shrinks 60 → 40. d shrinks 80 → 20.
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width} c={itemC.ContentRect.Width} d={itemD.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 40) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Width - 20) < 2);
        }
    }
}
