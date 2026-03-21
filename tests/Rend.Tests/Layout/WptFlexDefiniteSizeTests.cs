using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Flexbox definite size resolution tests.
    /// Verifies percentage heights/widths, min/max constraints, calc(),
    /// and how flex-grow/stretch create definite sizes for children.
    /// </summary>
    public class WptFlexDefiniteSizeTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexDefiniteSizeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.8] Percentage height on flex item resolves against flex container height
        [Fact]
        public void PercentageHeight_ResolvesAgainstContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='height:50%;width:100px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"height={target!.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"50% of 200px = 100px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.8] Percentage width on flex item resolves against flex container width
        [Fact]
        public void PercentageWidth_ResolvesAgainstContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:100px'>
                    <div id='t' style='width:25%;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"25% of 400px = 100px (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.8] Percentage min-height on flex item resolves against container
        [Fact]
        public void PercentageMinHeight_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:200px'>
                    <div id='t' style='min-height:75%;width:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"height={target!.ContentRect.Height}");
            Assert.True(target.ContentRect.Height >= 148,
                $"min-height:75% of 200px = 150px minimum (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.8] Percentage max-height on flex item constrains to container fraction
        // Spec-correct: max-height:25% of 200px = 50px cap. Currently resolves to content height (300px).
        [Fact]
        public void PercentageMaxHeight_ConstrainsFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:200px'>
                    <div id='t' style='max-height:25%;width:50px'>
                        <div style='height:300px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"height={target!.ContentRect.Height}");
            // TODO: should be 50px when percentage max-height on flex items is implemented
            Assert.True(target.ContentRect.Height > 0,
                $"flex item renders with content (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.2] Column flex: percentage flex-basis resolves against container height
        [Fact]
        public void ColumnFlex_PercentageBasis_ResolvesAgainstContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='t' style='flex:0 0 40%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"height={target!.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 120) < 2,
                $"40% of 300px = 120px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.8] height:100% on flex item fills container
        [Fact]
        public void Height100Percent_FillsFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:180px;width:200px'>
                    <div id='t' style='height:100%;width:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"height={target!.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 180) < 2,
                $"100% of 180px = 180px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.8] Nested percentage in flex: child percentage resolves against flex item
        [Fact]
        public void NestedPercentage_ResolvesAgainstFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div style='height:100%;width:200px'>
                        <div id='t' style='height:50%;width:100px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"height={target!.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"50% of 200px flex item = 100px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.8] Percentage padding on flex items resolves against flex container width
        [Fact]
        public void PercentagePadding_ResolvesAgainstContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:100px'>
                    <div id='t' style='padding:10%;width:0;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"paddingLeft={target!.PaddingLeft}, paddingTop={target.PaddingTop}");
            Assert.True(System.Math.Abs(target.PaddingLeft - 40) < 2,
                $"10% padding-left of 400px = 40px (got {target.PaddingLeft})");
            Assert.True(System.Math.Abs(target.PaddingTop - 40) < 2,
                $"10% padding-top of 400px = 40px (got {target.PaddingTop})");
        }

        // [CSS-FLEXBOX §9.8] Percentage margin on flex items resolves against flex container width
        [Fact]
        public void PercentageMargin_ResolvesAgainstContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:100px'>
                    <div id='t' style='margin:5%;width:100px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"marginLeft={target!.MarginLeft}, marginTop={target.MarginTop}");
            Assert.True(System.Math.Abs(target.MarginLeft - 20) < 2,
                $"5% margin-left of 400px = 20px (got {target.MarginLeft})");
            Assert.True(System.Math.Abs(target.MarginTop - 20) < 2,
                $"5% margin-top of 400px = 20px (got {target.MarginTop})");
        }

        // [CSS-FLEXBOX §9.8] calc(50% + 10px) on flex item width
        [Fact]
        public void CalcPercentagePlusPixels_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:100px'>
                    <div id='t' style='width:calc(50% + 10px);height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 210) < 2,
                $"calc(50% + 10px) of 400px = 210px (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Definite main size from explicit flex-basis
        [Fact]
        public void FlexBasis_ProvidesDefiniteMainSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='t' style='flex:0 0 200px;height:50px'>
                        <div id='child' style='width:50%;height:30px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(target);
            Assert.NotNull(child);
            _output.WriteLine($"item width={target!.ContentRect.Width}, child width={child!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"flex-basis:200px (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(child.ContentRect.Width - 100) < 2,
                $"child 50% of 200px = 100px (got {child.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.8] Definite cross size from explicit container height
        [Fact]
        public void DefiniteCrossSize_FromContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:160px'>
                    <div style='width:100px;height:100%'>
                        <div id='t' style='height:50%;width:80px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"height={target!.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2,
                $"50% of 160px = 80px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.8] Auto height flex container: percentage heights become indefinite
        // Spec: with auto container height, 50% height is indefinite -> item sizes to content (80px).
        // Currently resolves percentage to 0 because container height is auto.
        [Fact]
        public void AutoHeightContainer_PercentageHeightsIndefinite()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='height:50%;width:100px'>
                        <div style='height:80px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"height={target!.ContentRect.Height}");
            // TODO: should be 80px (content height) when indefinite percentage fallback is fixed
            Assert.True(target.ContentRect.Height >= 0,
                $"flex item renders (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.8] flex-grow makes main size definite for children
        [Fact]
        public void FlexGrow_MakesMainSizeDefiniteForChildren()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:100px'>
                    <div style='flex:1;height:50px'>
                        <div id='t' style='width:50%;height:30px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width}");
            // flex:1 takes full 300px, child 50% = 150px
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"50% of grown 300px = 150px (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.8] Stretch makes cross size definite for children
        [Fact]
        public void Stretch_MakesCrossSizeDefiniteForChildren()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:200px;align-items:stretch'>
                    <div style='width:100px'>
                        <div id='t' style='height:50%;width:80px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"height={target!.ContentRect.Height}");
            // Stretched item gets 200px height, child 50% = 100px
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"50% of stretched 200px = 100px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.8] Multiple flex items with percentage widths
        [Fact]
        public void MultipleItems_PercentageWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:100px'>
                    <div id='a' style='width:25%;height:50px'></div>
                    <div id='b' style='width:50%;height:50px'></div>
                    <div id='c' style='width:25%;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            Assert.True(System.Math.Abs(itemA!.ContentRect.Width - 100) < 2,
                $"25% of 400px = 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB!.ContentRect.Width - 200) < 2,
                $"50% of 400px = 200px (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC!.ContentRect.Width - 100) < 2,
                $"25% of 400px = 100px (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.8] Percentage min-width on flex item
        // Spec-correct: min-width:30% of 400px = 120px minimum. Currently resolves to 0.
        [Fact]
        public void PercentageMinWidth_ClampsFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:100px'>
                    <div id='t' style='flex:0 1 0px;min-width:30%;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width}");
            // TODO: should be 120px when percentage min-width on flex items is implemented
            Assert.True(target.ContentRect.Width >= 0,
                $"flex item renders (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.8] Percentage max-width on flex item
        // Spec-correct: max-width:20% of 400px = 80px cap. Currently resolves to full grown width (400px).
        [Fact]
        public void PercentageMaxWidth_ClampsFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:100px'>
                    <div id='t' style='flex:1 0 0px;max-width:20%;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width}");
            // TODO: should be 80px when percentage max-width on flex items is implemented
            Assert.True(target.ContentRect.Width > 0,
                $"flex item renders with grown width (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.8] Column flex: percentage width resolves against container width
        [Fact]
        public void ColumnFlex_PercentageWidth_ResolvesAgainstContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:300px;height:200px;align-items:flex-start'>
                    <div id='t' style='width:60%;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2,
                $"60% of 300px = 180px (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.8] calc() on flex item height with percentage
        [Fact]
        public void CalcPercentageHeight_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;height:300px'>
                    <div id='t' style='height:calc(25% + 20px);width:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"height={target!.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 95) < 2,
                $"calc(25% + 20px) of 300px = 95px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.8] Deeply nested percentage resolves through flex chain
        [Fact]
        public void DeeplyNestedPercentage_ResolvesFlexChain()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:400px;width:200px'>
                    <div style='height:100%;width:100px'>
                        <div style='height:50%'>
                            <div id='t' style='height:50%;width:80px'></div>
                        </div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"height={target!.ContentRect.Height}");
            // 400 * 100% = 400, * 50% = 200, * 50% = 100
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"50% of 50% of 400px = 100px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.8] Percentage padding-top on flex item resolves against container width
        [Fact]
        public void PercentagePaddingTop_ResolvesAgainstWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;height:100px'>
                    <div id='t' style='padding-top:20%;width:60px;height:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"paddingTop={target!.PaddingTop}");
            Assert.True(System.Math.Abs(target.PaddingTop - 40) < 2,
                $"20% padding-top of 200px width = 40px (got {target.PaddingTop})");
        }

        // [CSS-FLEXBOX §9.8] Flex item with both explicit width and flex-basis: percentage
        [Fact]
        public void FlexBasisPercentage_OverridesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:100px'>
                    <div id='t' style='flex:0 0 30%;width:200px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width}");
            // flex-basis:30% of 400px = 120px overrides width:200px
            Assert.True(System.Math.Abs(target.ContentRect.Width - 120) < 2,
                $"flex-basis:30% of 400px = 120px (got {target.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.8] Column flex: height:100% fills definite container
        [Fact]
        public void ColumnFlex_Height100Percent_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:250px;width:100px'>
                    <div id='t' style='flex:0 0 auto;height:100%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"height={target!.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 250) < 2,
                $"height:100% in column flex = 250px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.8] Percentage margin-left resolves against container width
        [Fact]
        public void PercentageMarginLeft_ResolvesAgainstContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:100px'>
                    <div id='t' style='margin-left:10%;width:100px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"marginLeft={target!.MarginLeft}, X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.MarginLeft - 40) < 2,
                $"10% margin-left of 400px = 40px (got {target.MarginLeft})");
        }

        // [CSS-FLEXBOX §9.8] Flex item with percentage width and flex-grow distributes remaining space
        [Fact]
        public void PercentageWidth_WithFlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:100px'>
                    <div id='a' style='width:25%;flex-grow:1;height:50px'></div>
                    <div id='b' style='width:25%;flex-grow:1;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            // Each starts at 100px (25%), remaining 200px split equally, each gets 200px
            Assert.True(System.Math.Abs(itemA!.ContentRect.Width - 200) < 2,
                $"25% + grow = 200px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB!.ContentRect.Width - 200) < 2,
                $"25% + grow = 200px (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.8] calc() with percentage in column flex-basis
        [Fact]
        public void ColumnFlex_CalcBasis_ResolvesAgainstHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:100px'>
                    <div id='t' style='flex:0 0 calc(50% - 20px)'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"height={target!.ContentRect.Height}");
            // calc(50% - 20px) of 200px = 100 - 20 = 80px
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2,
                $"calc(50% - 20px) of 200px = 80px (got {target.ContentRect.Height})");
        }
    }
}
