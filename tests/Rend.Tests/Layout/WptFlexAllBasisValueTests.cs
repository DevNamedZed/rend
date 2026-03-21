using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive flex-basis value coverage: fixed px, percentages,
    /// auto with width fallback, calc(), shorthand keywords, column direction,
    /// border-box interaction, and min/max clamping.
    /// </summary>
    public class WptFlexAllBasisValueTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexAllBasisValueTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.2] flex-basis:0px yields zero-width item
        [Fact]
        public void Basis0px_YieldsZeroWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 0px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width < 1, $"Expected ~0, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:10px sets item width to 10
        [Fact]
        public void Basis10px_SetsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 10px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 10) < 2, $"Expected 10, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:20px sets item width to 20
        [Fact]
        public void Basis20px_SetsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 20px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 20) < 2, $"Expected 20, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:30px sets item width to 30
        [Fact]
        public void Basis30px_SetsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 30px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 30) < 2, $"Expected 30, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:40px sets item width to 40
        [Fact]
        public void Basis40px_SetsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 40px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 40) < 2, $"Expected 40, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:50px sets item width to 50
        [Fact]
        public void Basis50px_SetsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 50) < 2, $"Expected 50, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:60px sets item width to 60
        [Fact]
        public void Basis60px_SetsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 60px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 60) < 2, $"Expected 60, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:80px sets item width to 80
        [Fact]
        public void Basis80px_SetsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 80px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2, $"Expected 80, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:100px sets item width to 100
        [Fact]
        public void Basis100px_SetsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2, $"Expected 100, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:120px sets item width to 120
        [Fact]
        public void Basis120px_SetsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 120px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2, $"Expected 120, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:150px sets item width to 150
        [Fact]
        public void Basis150px_SetsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 150px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2, $"Expected 150, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:200px sets item width to 200
        [Fact]
        public void Basis200px_SetsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2, $"Expected 200, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:250px sets item width to 250
        [Fact]
        public void Basis250px_SetsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 250px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 250) < 2, $"Expected 250, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:300px sets item width to 300
        [Fact]
        public void Basis300px_SetsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 300px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2, $"Expected 300, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:10% of 400px container = 40px
        [Fact]
        public void BasisPercent10_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 10%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 40) < 2, $"Expected 40, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:20% of 400px container = 80px
        [Fact]
        public void BasisPercent20_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 20%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2, $"Expected 80, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:25% of 400px container = 100px
        [Fact]
        public void BasisPercent25_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 25%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2, $"Expected 100, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:33% of 400px container = 132px
        [Fact]
        public void BasisPercent33_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 33%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 132) < 2, $"Expected 132, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:50% of 400px container = 200px
        [Fact]
        public void BasisPercent50_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 50%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2, $"Expected 200, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:75% of 400px container = 300px
        [Fact]
        public void BasisPercent75_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 75%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2, $"Expected 300, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:100% of 400px container = 400px
        [Fact]
        public void BasisPercent100_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 100%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 400) < 2, $"Expected 400, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:auto with width:80px uses the width
        [Fact]
        public void BasisAuto_FallsBackToWidth80()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 auto;width:80px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2, $"Expected 80, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:auto with width:120px uses the width
        [Fact]
        public void BasisAuto_FallsBackToWidth120()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 auto;width:120px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2, $"Expected 120, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:auto with width:200px uses the width
        [Fact]
        public void BasisAuto_FallsBackToWidth200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 auto;width:200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2, $"Expected 200, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] calc(50% - 20px) of 400px container = 180px
        [Fact]
        public void BasisCalcPercentMinus_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 calc(50% - 20px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 180) < 2, $"Expected 180, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] calc(25% + 50px) of 400px container = 150px
        [Fact]
        public void BasisCalcPercentPlus_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 calc(25% + 50px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2, $"Expected 150, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis overrides width when both are set
        [Fact]
        public void BasisOverridesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 100px;width:200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2, $"Expected 100 (basis wins), got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] flex-basis:0 with flex-grow:1 fills container
        [Fact]
        public void Basis0_WithGrow1_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 400) < 2, $"Expected 400, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §4.5] flex:1 shorthand sets basis=0%, grow=1, shrink=1
        [Fact]
        public void ShorthandFlex1_EqualDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2, $"Expected 200, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2, $"Expected 200, got {itemB.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §4.5] flex:auto shorthand sets basis=auto, grow=1, shrink=1
        [Fact]
        public void ShorthandFlexAuto_GrowsFromWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:auto;width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 399, $"Expected ~400 (grows to fill), got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §4.5] flex:none shorthand sets basis=auto, grow=0, shrink=0
        [Fact]
        public void ShorthandFlexNone_KeepsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:none;width:120px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2, $"Expected 120, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] column direction: flex-basis:80px sets item height
        [Fact]
        public void ColumnBasis80_SetsHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px;height:400px'>
                    <div id='item' style='flex:0 0 80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2, $"Expected height 80, got {item.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9.2] border-box: flex-basis:150px with padding:20px
        [Fact]
        public void BasisBorderBox_IncludesPaddingInBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 150px;box-sizing:border-box;padding:20px;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float totalWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight;
            Assert.True(System.Math.Abs(totalWidth - 150) < 2, $"Expected border-box total 150, got {totalWidth}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 110) < 2, $"Expected content width 110, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis clamped by max-width
        [Fact]
        public void BasisClampedByMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 300px;max-width:150px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2, $"Expected 150 (clamped), got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis raised by min-width
        [Fact]
        public void BasisRaisedByMinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 30px;min-width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2, $"Expected 100 (min-width), got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] three items with flex:1 basis:0 share container equally
        [Fact]
        public void ThreeItemsFlex1_EqualThirds()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                    <div id='c' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2, $"Expected 100, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2, $"Expected 100, got {itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2, $"Expected 100, got {itemC.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:auto with no width uses content size (empty = 0)
        [Fact]
        public void BasisAuto_NoWidth_EmptyContentIsZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 auto;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width < 2, $"Expected ~0 (no content), got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] two items: basis:150px vs basis:250px fills 400px
        [Fact]
        public void TwoItemsBasisSumEqualsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:0 0 150px;height:30px'></div>
                    <div id='b' style='flex:0 0 250px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2, $"Expected 150, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 250) < 2, $"Expected 250, got {itemB.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] basis:200px overrides width:80px (basis always wins)
        [Fact]
        public void BasisOverridesSmallWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 200px;width:80px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2, $"Expected 200 (basis wins over width:80), got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] flex:1 with basis:0 two items grow equally
        [Fact]
        public void Flex1Basis0_TwoItems_GrowEqually()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2, $"Expected 200, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2, $"Expected 200, got {itemB.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] column direction: basis:80px with height on item is ignored
        [Fact]
        public void ColumnBasis_OverridesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px;height:400px'>
                    <div id='item' style='flex:0 0 80px;height:200px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2, $"Expected 80 (basis overrides height), got {item.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9.2] border-box basis with border included
        [Fact]
        public void BasisBorderBox_IncludesBorderInBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 150px;box-sizing:border-box;padding:10px;border:5px solid black;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float totalWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight + item.BorderLeftWidth + item.BorderRightWidth;
            Assert.True(System.Math.Abs(totalWidth - 150) < 2, $"Expected border-box total 150, got {totalWidth}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2, $"Expected content 120 (150-10-10-5-5), got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] max-width clamps basis with grow
        [Fact]
        public void BasisWithGrow_ClampedByMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:1 0 0px;max-width:100px;height:30px'></div>
                    <div id='fill' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            var fill = LayoutTestHelper.FindById(root, "fill")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2, $"Expected 100 (max-width), got {item.ContentRect.Width}");
            Assert.True(System.Math.Abs(fill.ContentRect.Width - 300) < 2, $"Expected 300 (remaining), got {fill.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] min-width with shrink prevents collapse below min
        [Fact]
        public void BasisWithShrink_FlooredByMinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='item' style='flex:0 1 300px;min-width:150px;height:30px'></div>
                    <div id='other' style='flex:0 0 100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 149, $"Expected >= 150 (min-width), got {item.ContentRect.Width}");
        }
    }
}
