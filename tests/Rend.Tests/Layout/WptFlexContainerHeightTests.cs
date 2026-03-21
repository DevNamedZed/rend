using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexContainerHeightTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexContainerHeightTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.4] Row auto height equals tallest item
        [Fact]
        public void RowAutoHeight_EqualsTallestItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:300px'>
                    <div style='width:50px;height:40px'></div>
                    <div style='width:50px;height:80px'></div>
                    <div style='width:50px;height:60px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 80) < 2,
                $"Row auto height should be tallest item 80 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Column auto height equals sum of items
        [Fact]
        public void ColumnAutoHeight_EqualsSumOfItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;width:100px'>
                    <div style='height:30px'></div>
                    <div style='height:50px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 100) < 2,
                $"Column auto height should be sum 100 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Explicit height overrides auto
        [Fact]
        public void ExplicitHeight_OverridesAuto()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;height:200px'>
                    <div style='width:50px;height:40px'></div>
                    <div style='width:50px;height:60px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 200) < 2,
                $"Explicit height 200 should override auto (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.5] min-height enforced on container
        [Fact]
        public void MinHeight_Enforced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;min-height:150px'>
                    <div style='width:50px;height:30px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(container.ContentRect.Height >= 148,
                $"min-height:150 should be enforced (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.5] max-height clamps container
        [Fact]
        public void MaxHeight_ClampsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;width:200px;max-height:80px'>
                    <div style='height:50px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(container.ContentRect.Height <= 82,
                $"max-height:80 should clamp (got {container.ContentRect.Height})");
        }

        // [CSS2 §10.5] Percentage height resolves against parent
        [Fact]
        public void PercentageHeight_ResolvesAgainstParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:400px'>
                    <div id='t' style='display:flex;width:200px;height:50%'>
                        <div style='width:50px;height:30px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 200) < 2,
                $"50% of 400 = 200 (got {container.ContentRect.Height})");
        }

        // [CSS-VALUES §5.1.2] vh height resolves against viewport
        [Fact]
        public void VhHeight_ResolvesAgainstViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;height:50vh'>
                    <div style='width:50px;height:30px'></div>
                </div></body>", viewportHeight: 300);
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 150) < 2,
                $"50vh of 300 = 150 (got {container.ContentRect.Height})");
        }

        // [CSS-VALUES §8.1] calc() height resolves correctly
        [Fact]
        public void CalcHeight_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:400px'>
                    <div id='t' style='display:flex;width:200px;height:calc(50% - 20px)'>
                        <div style='width:50px;height:30px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 180) < 2,
                $"calc(50% - 20px) of 400 = 180 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Column auto height includes gap
        [Fact]
        public void ColumnAutoHeight_IncludesGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;gap:10px;width:100px'>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 90) < 2,
                $"40+10+40 = 90 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Auto height includes padding
        [Fact]
        public void AutoHeight_IncludesPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;padding:15px'>
                    <div style='width:50px;height:60px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentH={container!.ContentRect.Height} borderH={container.BorderRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2,
                $"Content height should be 60 (got {container.ContentRect.Height})");
            Assert.True(System.Math.Abs(container.BorderRect.Height - 90) < 2,
                $"Border height should be 60+15+15=90 (got {container.BorderRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Auto height includes border
        [Fact]
        public void AutoHeight_IncludesBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;border:5px solid black'>
                    <div style='width:50px;height:60px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentH={container!.ContentRect.Height} borderH={container.BorderRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2,
                $"Content height should be 60 (got {container.ContentRect.Height})");
            Assert.True(System.Math.Abs(container.BorderRect.Height - 70) < 2,
                $"Border height should be 60+5+5=70 (got {container.BorderRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Row wrap auto height equals sum of line heights
        [Fact]
        public void RowWrapAutoHeight_EqualsSumOfLineHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:200px'>
                    <div style='width:120px;height:40px'></div>
                    <div style='width:120px;height:60px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 100) < 2,
                $"Two wrap lines: 40+60 = 100 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Row wrap auto height with mixed-height lines
        [Fact]
        public void RowWrapAutoHeight_MixedHeightLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:200px'>
                    <div style='width:90px;height:30px'></div>
                    <div style='width:90px;height:50px'></div>
                    <div style='width:90px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            // Line 1: max(30,50) = 50. Line 2: 20. Total = 70.
            Assert.True(System.Math.Abs(container.ContentRect.Height - 70) < 2,
                $"Line1=50 + Line2=20 = 70 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Column auto height with gap between items
        [Fact]
        public void ColumnAutoHeight_ThreeItemsWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;gap:15px;width:100px'>
                    <div style='height:25px'></div>
                    <div style='height:25px'></div>
                    <div style='height:25px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            // 25*3 + 15*2 = 105
            Assert.True(System.Math.Abs(container.ContentRect.Height - 105) < 2,
                $"25*3 + 15*2 = 105 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Nested flex container inherits auto height correctly
        [Fact]
        public void NestedFlexContainer_AutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='outer' style='display:flex;flex-direction:column;width:200px'>
                    <div id='inner' style='display:flex;width:200px'>
                        <div style='width:50px;height:70px'></div>
                        <div style='width:50px;height:90px'></div>
                    </div>
                </div></body>");
            var inner = LayoutTestHelper.FindById(root, "inner");
            var outer = LayoutTestHelper.FindById(root, "outer");
            Assert.NotNull(inner);
            Assert.NotNull(outer);
            _output.WriteLine($"inner.h={inner!.ContentRect.Height} outer.h={outer!.ContentRect.Height}");
            Assert.True(System.Math.Abs(inner.ContentRect.Height - 90) < 2,
                $"Inner row height = tallest 90 (got {inner.ContentRect.Height})");
            Assert.True(System.Math.Abs(outer.ContentRect.Height - 90) < 2,
                $"Outer column height = inner 90 (got {outer.ContentRect.Height})");
        }

        // [CSS-FLEXBOX + CSS-GRID] Flex container inside grid item
        [Fact]
        public void FlexContainerInGridItem_AutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;width:200px'>
                    <div id='t' style='display:flex'>
                        <div style='width:50px;height:45px'></div>
                        <div style='width:50px;height:75px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 75) < 2,
                $"Flex in grid item height = tallest 75 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Auto height with item margins
        [Fact]
        public void RowAutoHeight_WithItemMargins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px'>
                    <div style='width:50px;height:40px;margin-top:10px;margin-bottom:10px'></div>
                    <div style='width:50px;height:40px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            // Row auto height should accommodate item + its margins: 10+40+10=60
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2,
                $"Item margins contribute to row height: 10+40+10=60 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Empty flex container has zero height
        [Fact]
        public void EmptyFlexContainer_ZeroHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px'></div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(container.ContentRect.Height < 2,
                $"Empty flex container should have ~0 height (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Auto height with single item
        [Fact]
        public void RowAutoHeight_SingleItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px'>
                    <div style='width:100px;height:55px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 55) < 2,
                $"Single item height 55 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.1] Abspos children excluded from auto height
        [Fact]
        public void RowAutoHeight_AbsposExcluded()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;position:relative;width:200px'>
                    <div style='width:50px;height:40px'></div>
                    <div style='position:absolute;width:50px;height:200px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 40) < 2,
                $"Abspos excluded: height=40 not 200 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Row wrap auto height with row-gap
        [Fact]
        public void RowWrapAutoHeight_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;row-gap:10px;width:100px'>
                    <div style='width:60px;height:30px'></div>
                    <div style='width:60px;height:30px'></div>
                    <div style='width:60px;height:30px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            // 3 lines of 30px each + 2 gaps of 10px = 110
            Assert.True(System.Math.Abs(container.ContentRect.Height - 110) < 2,
                $"3*30 + 2*10 = 110 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Column auto height with padding and border
        [Fact]
        public void ColumnAutoHeight_PaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;width:200px;padding:10px;border:5px solid black'>
                    <div style='height:30px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentH={container!.ContentRect.Height} borderH={container.BorderRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 70) < 2,
                $"Content height 30+40=70 (got {container.ContentRect.Height})");
            Assert.True(System.Math.Abs(container.BorderRect.Height - 100) < 2,
                $"Border height 70+10+10+5+5=100 (got {container.BorderRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] min-height larger than content wins
        [Fact]
        public void MinHeight_LargerThanContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;min-height:200px'>
                    <div style='width:50px;height:30px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 200) < 2,
                $"min-height:200 > content 30 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] max-height smaller than explicit height wins
        [Fact]
        public void MaxHeight_SmallerThanExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;height:300px;max-height:100px'>
                    <div style='width:50px;height:30px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(container.ContentRect.Height <= 102,
                $"max-height:100 clamps height:300 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Column flex with gap and padding combined
        [Fact]
        public void ColumnAutoHeight_GapAndPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;gap:10px;padding:20px;width:200px'>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentH={container!.ContentRect.Height} borderH={container.BorderRect.Height}");
            // Content: 30+10+30=70. Border: 70+20+20=110.
            Assert.True(System.Math.Abs(container.ContentRect.Height - 70) < 2,
                $"Content 30+10+30=70 (got {container.ContentRect.Height})");
            Assert.True(System.Math.Abs(container.BorderRect.Height - 110) < 2,
                $"Border 70+20+20=110 (got {container.BorderRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Row wrap with different height items per line
        [Fact]
        public void RowWrapAutoHeight_DifferentHeightsPerLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:200px'>
                    <div style='width:100px;height:25px'></div>
                    <div style='width:100px;height:25px'></div>
                    <div style='width:100px;height:50px'></div>
                    <div style='width:100px;height:35px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            // Line 1: max(25,25)=25. Line 2: max(50,35)=50. Total=75.
            Assert.True(System.Math.Abs(container.ContentRect.Height - 75) < 2,
                $"Line1=25 + Line2=50 = 75 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Column auto height with margin on items
        [Fact]
        public void ColumnAutoHeight_ItemMargins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;width:100px'>
                    <div style='height:30px;margin-bottom:10px'></div>
                    <div style='height:30px;margin-top:10px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            // Flex does not collapse margins: 30+10+10+30=80
            Assert.True(System.Math.Abs(container.ContentRect.Height - 80) < 2,
                $"No margin collapse in flex: 30+10+10+30=80 (got {container.ContentRect.Height})");
        }
    }
}
