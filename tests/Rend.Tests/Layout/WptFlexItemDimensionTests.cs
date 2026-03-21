using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for flex item dimension resolution: explicit width/height, percentage,
    /// flex-basis interactions, flex shorthand, box-sizing, padding/border,
    /// min/max constraints, calc/em/vw units, and auto sizing.
    /// </summary>
    public class WptFlexItemDimensionTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexItemDimensionTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Width100px_IsRespected()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"width:100px should be respected, got {item.ContentRect.Width}");
        }

        [Fact]
        public void Width200px_IsRespected()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"width:200px should be respected, got {item.ContentRect.Width}");
        }

        [Fact]
        public void Width50Percent_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:50%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"50% of 400px should be 200px, got {item.ContentRect.Width}");
        }

        [Fact]
        public void Height50px_IsRespected()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:200px;align-items:flex-start'>
                    <div id='item' style='width:100px;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.h={item.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2,
                $"height:50px should be respected, got {item.ContentRect.Height}");
        }

        [Fact]
        public void Height100Percent_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:200px;align-items:flex-start'>
                    <div id='item' style='width:100px;height:100%'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.h={item.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 200) < 2,
                $"height:100% should fill container (200px), got {item.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis overrides width when both are set
        [Fact]
        public void FlexBasis100px_OverridesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex-basis:100px;width:200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"flex-basis:100px should override width:200px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:auto falls back to width
        [Fact]
        public void FlexBasisAuto_UsesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex-basis:auto;width:150px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"flex-basis:auto should use width:150px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] flex:1 means grow:1 shrink:1 basis:0 — fills container
        [Fact]
        public void Flex1_BasisZero_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:1;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(item.ContentRect.Width >= 299,
                $"flex:1 should fill container (300px), got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §7.2] flex:none = 0 0 auto — preserves width
        [Fact]
        public void FlexNone_PreservesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:none;width:120px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"flex:none should preserve width:120px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §7.2] flex:auto = 1 1 auto — grows from width
        [Fact]
        public void FlexAuto_GrowsFromWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:auto;width:100px;height:30px'></div>
                    <div id='b' style='flex:auto;width:100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.w={itemA.ContentRect.Width}, b.w={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"flex:auto should grow equally to 200px, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"flex:auto should grow equally to 200px, got {itemB.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §7.2] flex:0 0 auto — same as flex:none
        [Fact]
        public void Flex_0_0_Auto_PreservesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 auto;width:140px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 140) < 2,
                $"flex:0 0 auto should preserve width:140px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §7.2] flex:0 0 0px — zero basis, no grow, no shrink
        [Fact]
        public void Flex_0_0_0px_ZeroWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 0px;width:200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(item.ContentRect.Width < 2,
                $"flex:0 0 0px should produce zero width, got {item.ContentRect.Width}");
        }

        // [CSS-BOX §5] width with padding in content-box model
        [Fact]
        public void WidthWithPadding_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:160px;padding:15px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item content.w={item.ContentRect.Width}, pad.l={item.PaddingLeft}, pad.r={item.PaddingRight}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 160) < 2,
                $"Content width should be 160px, got {item.ContentRect.Width}");
            float totalWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight;
            Assert.True(System.Math.Abs(totalWidth - 190) < 2,
                $"Total width with padding should be 190px (160+15+15), got {totalWidth}");
        }

        // [CSS-BOX §5] width with border in content-box model
        [Fact]
        public void WidthWithBorder_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:160px;border:5px solid black;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item content.w={item.ContentRect.Width}, border.l={item.BorderLeftWidth}, border.r={item.BorderRightWidth}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 160) < 2,
                $"Content width should be 160px, got {item.ContentRect.Width}");
            float borderBoxWidth = item.ContentRect.Width + item.BorderLeftWidth + item.BorderRightWidth;
            Assert.True(System.Math.Abs(borderBoxWidth - 170) < 2,
                $"Border-box width should be 170px (160+5+5), got {borderBoxWidth}");
        }

        // [CSS-SIZING §4.1] box-sizing:border-box — width includes padding and border
        [Fact]
        public void WidthBorderBox_IncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:200px;padding:20px;border:5px solid black;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float totalWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight
                + item.BorderLeftWidth + item.BorderRightWidth;
            _output.WriteLine($"item content.w={item.ContentRect.Width}, total={totalWidth}");
            Assert.True(System.Math.Abs(totalWidth - 200) < 2,
                $"Border-box total width should be 200px, got {totalWidth}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"Content width should be 150px (200 - 2*20 - 2*5), got {item.ContentRect.Width}");
        }

        // [CSS-BOX §5] height with padding in content-box model
        [Fact]
        public void HeightWithPadding_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:300px;align-items:flex-start'>
                    <div id='item' style='width:100px;height:80px;padding:10px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item content.h={item.ContentRect.Height}, border.h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"Content height should be 80px, got {item.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 100) < 2,
                $"Border rect height should be 100px (80+10+10), got {item.BorderRect.Height}");
        }

        // [CSS-SIZING §4.1] box-sizing:border-box for height
        [Fact]
        public void HeightBorderBox_IncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:300px;align-items:flex-start'>
                    <div id='item' style='box-sizing:border-box;width:100px;height:100px;padding:15px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float totalHeight = item.ContentRect.Height + item.PaddingTop + item.PaddingBottom
                + item.BorderTopWidth + item.BorderBottomWidth;
            _output.WriteLine($"item content.h={item.ContentRect.Height}, total.h={totalHeight}");
            Assert.True(System.Math.Abs(totalHeight - 100) < 2,
                $"Border-box total height should be 100px, got {totalHeight}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"Content height should be 60px (100 - 2*15 - 2*5), got {item.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9.4] width:auto in column flex stretches to container cross axis
        [Fact]
        public void WidthAuto_ColumnFlex_StretchesToCrossAxis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:250px;height:200px'>
                    <div id='item' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 250) < 2,
                $"Auto width in column flex should stretch to 250px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.4] height:auto in row flex stretches to container cross axis
        [Fact]
        public void HeightAuto_RowFlex_StretchesToCrossAxis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:180px'>
                    <div id='item' style='width:100px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.h={item.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 180) < 2,
                $"Auto height in row flex should stretch to 180px, got {item.ContentRect.Height}");
        }

        // [CSS-SIZING §4.2] min-width clamps flex item
        [Fact]
        public void MinWidth_ClampsFlexShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='item' style='width:180px;min-width:120px;height:30px'></div>
                    <div style='width:180px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(item.ContentRect.Width >= 119,
                $"min-width:120px should prevent shrinking below 120px, got {item.ContentRect.Width}");
        }

        // [CSS-SIZING §4.2] max-width clamps flex grow
        [Fact]
        public void MaxWidth_ClampsFlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:1;max-width:100px;height:30px'></div>
                    <div style='flex:1;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(item.ContentRect.Width <= 101,
                $"max-width:100px should clamp growth, got {item.ContentRect.Width}");
        }

        // [CSS-SIZING §4.2] min-height enforces minimum in column flex
        [Fact]
        public void MinHeight_EnforcesMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:100px'>
                    <div id='item' style='flex:0 1 80px;min-height:70px'></div>
                    <div style='flex:0 1 80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.h={item.ContentRect.Height}");
            Assert.True(item.ContentRect.Height >= 69,
                $"min-height:70px should enforce minimum, got {item.ContentRect.Height}");
        }

        // [CSS-SIZING §4.2] max-height clamps in column flex
        [Fact]
        public void MaxHeight_ClampsGrowInColumn()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:300px'>
                    <div id='item' style='flex:1;max-height:80px'></div>
                    <div style='flex:1'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.h={item.ContentRect.Height}");
            Assert.True(item.ContentRect.Height <= 81,
                $"max-height:80px should clamp growth, got {item.ContentRect.Height}");
        }

        // [CSS-VALUES §8.1] calc() width resolves expression
        [Fact]
        public void CalcWidth_ResolvesExpression()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:calc(100px + 50px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"calc(100px + 50px) should be 150px, got {item.ContentRect.Width}");
        }

        // [CSS-VALUES §5.1.1] em width resolves against font-size
        [Fact]
        public void EmWidth_ResolvesAgainstFontSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;font-size:20px'>
                    <div id='item' style='width:5em;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"5em at font-size:20px should be 100px, got {item.ContentRect.Width}");
        }

        // [CSS-VALUES §5.1.2] vw width resolves against viewport
        [Fact]
        public void VwWidth_ResolvesAgainstViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:25vw;height:30px'></div>
                </div></body>", viewportWidth: 400, viewportHeight: 300);
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"25vw of 400px viewport should be 100px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] percentage width in nested flex
        [Fact]
        public void PercentageWidth_NestedFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div style='display:flex;width:200px'>
                        <div id='item' style='width:50%;height:30px'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"50% of nested 200px container should be 100px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.3] child content determines auto width
        [Fact]
        public void AutoWidth_DeterminedByChildContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item'><div style='width:75px;height:20px'></div></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 75) < 2,
                $"Auto width should match child content (75px), got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] width:0 creates zero-width item
        [Fact]
        public void WidthZero_CreatesZeroWidthItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 auto;width:0;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(item.ContentRect.Width < 1,
                $"width:0 should produce zero-width item, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] height:0 creates zero-height item
        [Fact]
        public void HeightZero_CreatesZeroHeightItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:200px;align-items:flex-start'>
                    <div id='item' style='width:100px;height:0'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.h={item.ContentRect.Height}");
            Assert.True(item.ContentRect.Height < 1,
                $"height:0 should produce zero-height item, got {item.ContentRect.Height}");
        }

        // [CSS-FLEXBOX §9.7] flex:1 with two items splits equally
        [Fact]
        public void Flex1_TwoItems_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.w={itemA.ContentRect.Width}, b.w={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"flex:1 items should split equally to 150px, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"flex:1 items should split equally to 150px, got {itemB.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] flex:1 with three items splits equally
        [Fact]
        public void Flex1_ThreeItems_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.w={itemA.ContentRect.Width}, b.w={itemB.ContentRect.Width}, c.w={itemC.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"Each flex:1 item should be 100px, got a={itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"Each flex:1 item should be 100px, got b={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2,
                $"Each flex:1 item should be 100px, got c={itemC.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §7.2] flex:none does not grow or shrink
        [Fact]
        public void FlexNone_DoesNotShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='item' style='flex:none;width:200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"flex:none should not shrink from 200px, got {item.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc width with percentage and pixel
        [Fact]
        public void CalcWidth_PercentAndPixel()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:calc(50% - 20px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 180) < 2,
                $"calc(50% - 20px) of 400px should be 180px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] border-box with flex-basis includes padding+border
        [Fact]
        public void FlexBasis_BorderBox_IncludesPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;flex:0 0 200px;padding:20px;border:5px solid black;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float totalWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight
                + item.BorderLeftWidth + item.BorderRightWidth;
            _output.WriteLine($"item content.w={item.ContentRect.Width}, total.w={totalWidth}");
            Assert.True(System.Math.Abs(totalWidth - 200) < 2,
                $"Border-box flex-basis total should be 200px, got {totalWidth}");
        }

        // [CSS-FLEXBOX §9.2] width with both padding and border in content-box
        [Fact]
        public void WidthWithPaddingAndBorder_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:120px;padding:10px;border:3px solid black;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item content.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"Content width should be 120px in content-box, got {item.ContentRect.Width}");
            float borderBoxWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight
                + item.BorderLeftWidth + item.BorderRightWidth;
            Assert.True(System.Math.Abs(borderBoxWidth - 146) < 2,
                $"Border-box width should be 146px (120+2*10+2*3), got {borderBoxWidth}");
        }

        // [CSS-FLEXBOX §9.7] flex-basis overrides width, flex:1 overrides both
        [Fact]
        public void Flex1_OverridesWidthAndBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:1;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(item.ContentRect.Width >= 299,
                $"flex:1 (basis:0) should override width:50px and fill container, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] flex:initial = 0 1 auto — uses width as basis, can shrink
        [Fact]
        public void FlexInitial_UsesWidthAsBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:initial;width:80px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2,
                $"flex:initial should use width:80px as basis, got {item.ContentRect.Width}");
        }
    }
}
