using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive flex item sizing tests covering width, percentage, flex shorthand,
    /// grow/shrink ratios, min/max constraints, border-box, cross-axis sizing,
    /// alignment, column direction, gap, margin, and padding scenarios.
    /// </summary>
    public class WptFlexItemAllSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexItemAllSizingTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.2] explicit width:100 on flex item
        [Fact]
        public void ExplicitWidth100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:100px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.2] percentage width resolves against flex container
        [Fact]
        public void PercentageWidth50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:50%;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §7.2] flex:1 single item fills container
        [Fact]
        public void FlexOneFillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:1;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-FLEXBOX §7.2] flex:0 0 100px fixed basis
        [Fact]
        public void FlexFixedBasis100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 100px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.2] flex:0 0 auto with width:80 uses width as basis
        [Fact]
        public void FlexAutoBasusWithWidth80()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 auto;width:80px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-FLEXBOX §9.2] flex:0 0 50% percentage basis
        [Fact]
        public void FlexPercentageBasis50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 50%;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §7.1] flex:none width:100 no grow no shrink
        [Fact]
        public void FlexNoneWithWidth100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:none;width:100px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §7.1] flex:auto width:80 grows to fill
        [Fact]
        public void FlexAutoWithWidth80()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:auto;width:80px;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width >= 299);
        }

        // [CSS-FLEXBOX §9.7] flex 1:2:3 ratio distributes widths proportionally
        [Fact]
        public void FlexGrowRatio123_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:2 0 0px;height:30px'></div>
                    <div id='c' style='flex:3 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] flex 1:2:3 ratio X positions
        [Fact]
        public void FlexGrowRatio123_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:2 0 0px;height:30px'></div>
                    <div id='c' style='flex:3 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] fixed + grow + fixed layout
        [Fact]
        public void FixedGrowFixed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='left' style='flex:0 0 80px;height:30px'></div>
                    <div id='middle' style='flex:1;height:30px'></div>
                    <div id='right' style='flex:0 0 120px;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left")!;
            var middle = LayoutTestHelper.FindById(root, "middle")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(left.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(middle.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(right.ContentRect.Width - 120) < 2);
        }

        // [CSS-FLEXBOX §9.7] two equal shrink:1 items
        [Fact]
        public void ShrinkOneEqualItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;height:30px'></div>
                    <div id='b' style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] shrink:0 prevents item from shrinking
        [Fact]
        public void ShrinkZeroNoShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='item' style='flex-shrink:0;width:200px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §9] min-width:100 clamps shrink result
        [Fact]
        public void MinWidthClampsShrinkedItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:150px'>
                    <div id='item' style='flex:0 1 200px;min-width:100px;height:30px'></div>
                    <div style='flex:0 1 200px;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width >= 99);
        }

        // [CSS-FLEXBOX §9] max-width:80 clamps grown item
        [Fact]
        public void MaxWidthClampsGrownItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:1;max-width:80px;height:30px'></div>
                    <div style='flex:1;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width <= 81);
        }

        // [CSS-FLEXBOX §9.2] border-box width:200 padding:20 content width is 160
        [Fact]
        public void BorderBoxWidthWithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:200px;padding:20px;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 160) < 2);
            float borderBoxWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight;
            Assert.True(System.Math.Abs(borderBoxWidth - 200) < 2);
        }

        // [CSS-FLEXBOX §9.4] explicit height:50 on flex item in row direction
        [Fact]
        public void ExplicitHeight50InRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:100px'>
                    <div id='item' style='width:50px;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Height - 50) < 2);
        }

        // [CSS-FLEXBOX §9.4] align-items:stretch fills cross-axis to container height
        [Fact]
        public void StretchFillsCrossAxis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:100px;align-items:stretch'>
                    <div id='item' style='width:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-FLEXBOX §9.4] align-items:center centers item vertically
        [Fact]
        public void AlignItemsCenterVerticalPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:100px;align-items:center'>
                    <div id='item' style='width:50px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Y - 30) < 2);
        }

        // [CSS-FLEXBOX §9.4] align-items:flex-end positions item at bottom
        [Fact]
        public void AlignItemsFlexEndVerticalPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:100px;align-items:flex-end'>
                    <div id='item' style='width:50px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Y - 60) < 2);
        }

        // [CSS-FLEXBOX §9.4] column direction stretch fills cross-axis width
        [Fact]
        public void ColumnStretchFillsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:300px;align-items:stretch'>
                    <div id='item' style='height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] column flex:1 fills available height
        [Fact]
        public void ColumnFlexOneFillsHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:300px'>
                    <div id='item' style='flex:1'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Height - 300) < 2);
        }

        // [CSS-FLEXBOX §8.2] gap between items in row direction
        [Fact]
        public void GapBetweenRowItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;gap:20px'>
                    <div id='a' style='width:100px;height:30px'></div>
                    <div id='b' style='width:100px;height:30px'></div>
                    <div id='c' style='width:100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 240) < 2);
        }

        // [CSS-FLEXBOX §9] margin on flex items affects position
        [Fact]
        public void MarginOnItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='width:80px;height:30px;margin-right:20px'></div>
                    <div id='b' style='width:80px;height:30px;margin-left:10px'></div>
                </div></body>");
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 110) < 2);
        }

        // [CSS-FLEXBOX §9] padding on flex items reduces content area
        [Fact]
        public void PaddingOnItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;padding:15px;height:30px'></div>
                    <div id='b' style='flex:1;padding:15px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.PaddingLeft - 15) < 1);
            Assert.True(System.Math.Abs(itemA.PaddingRight - 15) < 1);
            float contentWidthA = itemA.ContentRect.Width;
            float contentWidthB = itemB.ContentRect.Width;
            Assert.True(System.Math.Abs(contentWidthA - contentWidthB) < 2);
            float totalA = contentWidthA + itemA.PaddingLeft + itemA.PaddingRight;
            float totalB = contentWidthB + itemB.PaddingLeft + itemB.PaddingRight;
            Assert.True(System.Math.Abs(totalA + totalB - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] flex:1 with two items splits evenly
        [Fact]
        public void FlexOneTwoItemsSplitEvenly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §7.1] flex:initial (0 1 auto) does not grow
        [Fact]
        public void FlexInitialDoesNotGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:initial;width:120px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width - 120) < 2);
        }

        // [CSS-FLEXBOX §9.4] align-items:flex-start keeps item at top
        [Fact]
        public void AlignItemsFlexStartVerticalPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:100px;align-items:flex-start'>
                    <div id='item' style='width:50px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Y - 0) < 2);
        }

        // [CSS-FLEXBOX §9.7] column direction two flex:1 items split height
        [Fact]
        public void ColumnTwoFlexOneSplitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:300px'>
                    <div id='a' style='flex:1'></div>
                    <div id='b' style='flex:1'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 150) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 150) < 2);
        }

        // [CSS-FLEXBOX §8.2] column gap between items
        [Fact]
        public void ColumnGapBetweenItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:300px;gap:10px'>
                    <div id='a' style='height:50px'></div>
                    <div id='b' style='height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 60) < 2);
        }

        // [CSS-FLEXBOX §9.7] flex grow with non-zero basis distributes remaining space
        [Fact]
        public void FlexGrowWithBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='a' style='flex:1 0 100px;height:30px'></div>
                    <div id='b' style='flex:1 0 200px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 300) < 2);
        }

        // [CSS-FLEXBOX §9.2] border-box with border and padding
        [Fact]
        public void BorderBoxWithBorderAndPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:200px;padding:20px;border:5px solid black;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float borderBoxWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight
                + item.BorderLeftWidth + item.BorderRightWidth;
            Assert.True(System.Math.Abs(borderBoxWidth - 200) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2);
        }

        // [CSS-FLEXBOX §9.7] three items with gap and flex:1
        [Fact]
        public void ThreeItemsWithGapAndFlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:340px;gap:20px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.4] align-self overrides align-items
        [Fact]
        public void AlignSelfOverridesAlignItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:100px;align-items:flex-start'>
                    <div id='item' style='align-self:flex-end;width:50px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Y - 60) < 2);
        }

        // [CSS-FLEXBOX §9.7] weighted shrink: shrink factors 1 and 3
        [Fact]
        public void WeightedShrinkFactors()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;height:30px'></div>
                    <div id='b' style='flex:0 3 150px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 125) < 3);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 75) < 3);
        }

        // [CSS-FLEXBOX §9] min-width on grow item clamps minimum
        [Fact]
        public void MinWidthOnGrowItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:1;min-width:100px;height:30px'></div>
                    <div id='b' style='flex:3;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width >= 99);
        }

        // [CSS-FLEXBOX §9] max-width on item clamps maximum, redistributes remainder
        [Fact]
        public void MaxWidthRedistributes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;max-width:80px;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(itemA.ContentRect.Width <= 81);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 110) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 110) < 2);
        }

        // [CSS-FLEXBOX §9.4] column align-items:center centers horizontally
        [Fact]
        public void ColumnAlignItemsCenterHorizontal()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px;align-items:center'>
                    <div id='item' style='width:80px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.X - 60) < 2);
        }

        // [CSS-FLEXBOX §9.7] column flex grow with gap
        [Fact]
        public void ColumnFlexGrowWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:230px;gap:10px'>
                    <div id='a' style='flex:1'></div>
                    <div id='b' style='flex:1'></div>
                    <div id='c' style='flex:1'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 70) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 70) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Height - 70) < 2);
        }

        // [CSS-FLEXBOX §9] margin-top on item in row direction shifts item vertically
        [Fact]
        public void MarginTopShiftsItemVertically()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:100px;align-items:flex-start'>
                    <div id='item' style='width:50px;height:40px;margin-top:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Y - 20) < 2);
        }

        // [CSS-FLEXBOX §9.7] four equal flex items split evenly
        [Fact]
        public void FourEqualFlexItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                    <div id='d' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.2] content-box width:200 padding:20 total outer width is 240
        [Fact]
        public void ContentBoxWidthWithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:200px;padding:20px;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2);
            float outerWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight;
            Assert.True(System.Math.Abs(outerWidth - 240) < 2);
        }
    }
}
