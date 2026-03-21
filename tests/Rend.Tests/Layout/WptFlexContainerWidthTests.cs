using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexContainerWidthTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexContainerWidthTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §3] Block-level flex container fills parent width
        [Fact]
        public void BlockFlex_FillsParentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex'>
                    <div style='width:50px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 400) < 2,
                $"Block flex fills viewport 400 (got {container.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §3] Inline-flex shrinks to fit content
        [Fact]
        public void InlineFlex_ShrinksToFitContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div><span id='t' style='display:inline-flex'>
                    <div style='width:50px;height:20px'></div>
                    <div style='width:70px;height:20px'></div>
                </span></div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 120) < 2,
                $"Inline-flex shrinks to 50+70=120 (got {container.ContentRect.Width})");
        }

        // [CSS2 §10.3.3] Explicit width on flex container
        [Fact]
        public void ExplicitWidth_Honored()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:250px'>
                    <div style='width:50px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 250) < 2,
                $"Explicit width 250 (got {container.ContentRect.Width})");
        }

        // [CSS2 §10.3.3] Percentage width resolves against parent
        [Fact]
        public void PercentageWidth_ResolvesAgainstParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='display:flex;width:50%'>
                        <div style='width:30px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 150) < 2,
                $"50% of 300 = 150 (got {container.ContentRect.Width})");
        }

        // [CSS-VALUES §8.1] calc() width resolves correctly
        [Fact]
        public void CalcWidth_ResolvesCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;width:calc(50% - 40px)'>
                        <div style='width:30px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 160) < 2,
                $"calc(50% - 40px) of 400 = 160 (got {container.ContentRect.Width})");
        }

        // [CSS2 §10.4] min-width enforced on flex container
        [Fact]
        public void MinWidth_Enforced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='display:flex;width:100px;min-width:200px'>
                        <div style='width:30px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(container.ContentRect.Width >= 198,
                $"min-width:200 should override width:100 (got {container.ContentRect.Width})");
        }

        // [CSS2 §10.4] max-width clamps flex container
        [Fact]
        public void MaxWidth_ClampsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='display:flex;width:250px;max-width:150px'>
                        <div style='width:30px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(container.ContentRect.Width <= 152,
                $"max-width:150 should clamp width:250 (got {container.ContentRect.Width})");
        }

        // [CSS2 §10.3.3] Auto width with horizontal margins
        [Fact]
        public void AutoWidth_WithHorizontalMargins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;margin-left:30px;margin-right:50px'>
                        <div style='width:50px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 320) < 2,
                $"400 - 30 - 50 = 320 (got {container.ContentRect.Width})");
        }

        // [CSS2 §10.3.3] Auto width with horizontal padding
        [Fact]
        public void AutoWidth_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;padding-left:20px;padding-right:30px'>
                        <div style='width:50px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentW={container!.ContentRect.Width} borderW={container.BorderRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 350) < 2,
                $"Content width 400 - 20 - 30 = 350 (got {container.ContentRect.Width})");
            Assert.True(System.Math.Abs(container.BorderRect.Width - 400) < 2,
                $"Border width fills parent 400 (got {container.BorderRect.Width})");
        }

        // [CSS2 §10.3.3] Auto width with border
        [Fact]
        public void AutoWidth_WithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;border:5px solid black'>
                        <div style='width:50px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentW={container!.ContentRect.Width} borderW={container.BorderRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 390) < 2,
                $"Content width 400 - 5 - 5 = 390 (got {container.ContentRect.Width})");
            Assert.True(System.Math.Abs(container.BorderRect.Width - 400) < 2,
                $"Border width fills parent 400 (got {container.BorderRect.Width})");
        }

        // [CSS-UI §3.2] border-box sizing includes padding+border in width
        [Fact]
        public void BorderBox_IncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;box-sizing:border-box;padding:15px;border:5px solid black'>
                    <div style='width:30px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentW={container!.ContentRect.Width} borderW={container.BorderRect.Width}");
            Assert.True(System.Math.Abs(container.BorderRect.Width - 200) < 2,
                $"Border-box total 200 (got {container.BorderRect.Width})");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 160) < 2,
                $"Content = 200 - 15*2 - 5*2 = 160 (got {container.ContentRect.Width})");
        }

        // [CSS-VALUES §5.1.2] vw width resolves against viewport
        [Fact]
        public void VwWidth_ResolvesAgainstViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:50vw'>
                    <div style='width:30px;height:20px'></div>
                </div></body>", viewportWidth: 400);
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 200) < 2,
                $"50vw of 400 = 200 (got {container.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §3] Flex container nested inside another flex container
        [Fact]
        public void FlexInFlex_InnerFillsOuterItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='flex:1'>
                        <div id='t' style='display:flex'>
                            <div style='width:30px;height:20px'></div>
                        </div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 300) < 2,
                $"Inner flex fills outer flex item 300 (got {container.ContentRect.Width})");
        }

        // [CSS-GRID + CSS-FLEXBOX] Flex container inside grid cell
        [Fact]
        public void FlexInGrid_FillsGridCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px 1fr;width:400px'>
                    <div id='t' style='display:flex'>
                        <div style='width:30px;height:20px'></div>
                    </div>
                    <div></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 200) < 2,
                $"Flex in 200px grid column (got {container.ContentRect.Width})");
        }

        // [CSS2 §10.3.3] Flex container inside block fills block width
        [Fact]
        public void FlexInBlock_FillsBlockWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:350px'>
                    <div id='t' style='display:flex'>
                        <div style='width:30px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 350) < 2,
                $"Block flex fills 350px parent (got {container.ContentRect.Width})");
        }

        // [CSS-SIZING §4.1] width:min-content on row flex = largest item min-content
        [Fact]
        public void MinContentWidth_LargestItemMinContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:min-content'>
                    <div style='width:80px;height:20px'></div>
                    <div style='width:60px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 80) < 2,
                $"min-content = largest item 80 (got {container.ContentRect.Width})");
        }

        // [CSS-SIZING §4.1] width:max-content expands to widest line
        [Fact]
        public void MaxContentWidth_ExpandsToWidestLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:max-content'>
                    <div style='width:80px;height:20px'></div>
                    <div style='width:60px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 140) < 2,
                $"max-content = sum of items 80+60=140 (got {container.ContentRect.Width})");
        }

        // [CSS-SIZING §4.1] width:fit-content respects available space
        [Fact]
        public void FitContentWidth_RespectsAvailableSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='display:flex;width:fit-content'>
                        <div style='width:80px;height:20px'></div>
                        <div style='width:60px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 140) < 2,
                $"fit-content = content 140 when < available 300 (got {container.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §3] Nested flex width propagation from parent to child
        [Fact]
        public void NestedFlexWidth_PropagatesFromParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='outer' style='display:flex;width:360px'>
                    <div id='inner' style='display:flex;flex:1'>
                        <div style='flex:1;height:20px'></div>
                        <div style='flex:1;height:20px'></div>
                    </div>
                </div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer");
            var inner = LayoutTestHelper.FindById(root, "inner");
            Assert.NotNull(outer);
            Assert.NotNull(inner);
            _output.WriteLine($"outer={outer!.ContentRect.Width} inner={inner!.ContentRect.Width}");
            Assert.True(System.Math.Abs(outer.ContentRect.Width - 360) < 2,
                $"Outer width 360 (got {outer.ContentRect.Width})");
            Assert.True(System.Math.Abs(inner.ContentRect.Width - 360) < 2,
                $"Inner flex:1 fills outer 360 (got {inner.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §3] Empty flex container with explicit width
        [Fact]
        public void EmptyFlexContainer_ExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px'></div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 200) < 2,
                $"Empty flex with width:200 (got {container.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §3] Empty flex container without width fills parent
        [Fact]
        public void EmptyFlexContainer_AutoWidthFillsParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='display:flex'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 300) < 2,
                $"Empty auto-width flex fills parent 300 (got {container.ContentRect.Width})");
        }

        // [CSS2 §10.4] min-width with auto width
        [Fact]
        public void MinWidth_WithAutoWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px'>
                    <div id='t' style='display:flex;min-width:200px'>
                        <div style='width:30px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(container.ContentRect.Width >= 198,
                $"min-width:200 overrides auto from 100px parent (got {container.ContentRect.Width})");
        }

        // [CSS2 §10.4] max-width with auto width
        [Fact]
        public void MaxWidth_WithAutoWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;max-width:200px'>
                        <div style='width:30px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(container.ContentRect.Width <= 202,
                $"max-width:200 clamps auto from 400px parent (got {container.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §3] Inline-flex with explicit width
        [Fact]
        public void InlineFlex_ExplicitWidthHonored()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div><span id='t' style='display:inline-flex;width:180px'>
                    <div style='width:30px;height:20px'></div>
                </span></div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 180) < 2,
                $"Inline-flex explicit width 180 (got {container.ContentRect.Width})");
        }

        // [CSS2 §10.3.3] Margin auto centers flex container
        [Fact]
        public void AutoMargins_CenterFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;width:200px;margin-left:auto;margin-right:auto'>
                        <div style='width:50px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width} x={container.ContentRect.X}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 200) < 2,
                $"Width stays 200 (got {container.ContentRect.Width})");
            Assert.True(System.Math.Abs(container.ContentRect.X - 100) < 2,
                $"Centered at x=100 in 400px parent (got {container.ContentRect.X})");
        }

        // [CSS-FLEXBOX §3] Column flex auto width stretches to widest item
        [Fact]
        public void ColumnFlex_AutoWidthFillsParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='display:flex;flex-direction:column'>
                        <div style='width:80px;height:20px'></div>
                        <div style='width:120px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 300) < 2,
                $"Column flex auto width fills parent 300 (got {container.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §3] Flex container with padding and margin combined
        [Fact]
        public void AutoWidth_WithPaddingAndMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;margin:0 20px;padding:0 15px'>
                        <div style='width:50px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentW={container!.ContentRect.Width} borderW={container.BorderRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 330) < 2,
                $"Content = 400 - 20*2 - 15*2 = 330 (got {container.ContentRect.Width})");
            Assert.True(System.Math.Abs(container.BorderRect.Width - 360) < 2,
                $"Border = 400 - 20*2 = 360 (got {container.BorderRect.Width})");
        }

        // [CSS-FLEXBOX §3] Flex container with border, padding, and border-box
        [Fact]
        public void BorderBox_WithBorderPaddingMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;width:300px;box-sizing:border-box;padding:10px;border:5px solid black;margin:0 20px'>
                        <div style='width:30px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentW={container!.ContentRect.Width} borderW={container.BorderRect.Width}");
            Assert.True(System.Math.Abs(container.BorderRect.Width - 300) < 2,
                $"Border-box total 300 (got {container.BorderRect.Width})");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 270) < 2,
                $"Content = 300 - 10*2 - 5*2 = 270 (got {container.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §3] Flex wrap does not change container width
        [Fact]
        public void FlexWrap_ContainerWidthUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:200px'>
                    <div style='width:120px;height:20px'></div>
                    <div style='width:120px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 200) < 2,
                $"Wrap does not expand container past 200 (got {container.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §3] Multiple nested flex containers propagate width
        [Fact]
        public void DeepNestedFlex_WidthPropagation()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='level1' style='display:flex;width:400px'>
                    <div id='level2' style='display:flex;flex:1'>
                        <div id='level3' style='display:flex;flex:1'>
                            <div style='flex:1;height:20px'></div>
                        </div>
                    </div>
                </div></body>");
            var level1 = LayoutTestHelper.FindById(root, "level1");
            var level2 = LayoutTestHelper.FindById(root, "level2");
            var level3 = LayoutTestHelper.FindById(root, "level3");
            Assert.NotNull(level1);
            Assert.NotNull(level2);
            Assert.NotNull(level3);
            _output.WriteLine($"l1={level1!.ContentRect.Width} l2={level2!.ContentRect.Width} l3={level3!.ContentRect.Width}");
            Assert.True(System.Math.Abs(level1.ContentRect.Width - 400) < 2,
                $"Level1 width 400 (got {level1.ContentRect.Width})");
            Assert.True(System.Math.Abs(level2.ContentRect.Width - 400) < 2,
                $"Level2 flex:1 fills 400 (got {level2.ContentRect.Width})");
            Assert.True(System.Math.Abs(level3.ContentRect.Width - 400) < 2,
                $"Level3 flex:1 fills 400 (got {level3.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §3] Flex container with gap does not change container width
        [Fact]
        public void FlexWithGap_ContainerWidthUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:300px;gap:20px'>
                    <div style='width:50px;height:20px'></div>
                    <div style='width:50px;height:20px'></div>
                    <div style='width:50px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 300) < 2,
                $"Gap does not change container width 300 (got {container.ContentRect.Width})");
        }

        // [CSS2 §10.4] min-width and max-width interact correctly
        [Fact]
        public void MinMaxWidth_Interaction()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;width:100px;min-width:150px;max-width:300px'>
                        <div style='width:30px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 150) < 2,
                $"min-width:150 raises width:100 to 150 (got {container.ContentRect.Width})");
        }

        // [CSS2 §10.3.3] Percentage width with padding and border-box
        [Fact]
        public void PercentageWidth_BorderBoxWithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;width:50%;box-sizing:border-box;padding:10px;border:5px solid black'>
                        <div style='width:30px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentW={container!.ContentRect.Width} borderW={container.BorderRect.Width}");
            Assert.True(System.Math.Abs(container.BorderRect.Width - 200) < 2,
                $"50% of 400 = 200 border-box (got {container.BorderRect.Width})");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 170) < 2,
                $"Content = 200 - 10*2 - 5*2 = 170 (got {container.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §3] Flex column with explicit width narrower than items
        [Fact]
        public void ColumnFlex_ExplicitWidthNarrowerThanItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;width:100px'>
                    <div style='width:200px;height:20px'></div>
                    <div style='width:150px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 100) < 2,
                $"Explicit width:100 constrains container (got {container.ContentRect.Width})");
        }

        // [CSS-VALUES §8.1] calc() with mixed units
        [Fact]
        public void CalcWidth_MixedUnits()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:flex;width:calc(25% + 50px)'>
                        <div style='width:30px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"width={container!.ContentRect.Width}");
            Assert.True(System.Math.Abs(container.ContentRect.Width - 150) < 2,
                $"calc(25% + 50px) = 100 + 50 = 150 (got {container.ContentRect.Width})");
        }
    }
}
