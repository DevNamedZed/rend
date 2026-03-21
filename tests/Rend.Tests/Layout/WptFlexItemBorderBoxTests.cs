using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for flex items with box-sizing: border-box.
    /// Verifies that width/height declarations include padding and border
    /// per CSS Box Sizing Level 3 within flexbox layout contexts.
    /// </summary>
    public class WptFlexItemBorderBoxTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexItemBorderBoxTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-SIZING3 §3.1] border-box width includes padding
        [Fact]
        public void BorderBox_Width_IncludesPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:200px;padding:20px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content={item.ContentRect.Width}, border={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 200) < 2,
                $"Border box width should be 200px (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 160) < 2,
                $"Content width should be 160px (200 - 20 - 20) (got {item.ContentRect.Width})");
        }

        // [CSS-SIZING3 §3.1] border-box width includes border
        [Fact]
        public void BorderBox_Width_IncludesBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:200px;border:10px solid black;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content={item.ContentRect.Width}, border={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 200) < 2,
                $"Border box width should be 200px (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 180) < 2,
                $"Content width should be 180px (200 - 10 - 10) (got {item.ContentRect.Width})");
        }

        // [CSS-SIZING3 §3.1] border-box width includes both padding and border
        [Fact]
        public void BorderBox_Width_IncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:200px;padding:15px;border:5px solid black;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContent = 200 - 15 - 15 - 5 - 5;
            _output.WriteLine($"content={item.ContentRect.Width}, border={item.BorderRect.Width}, expected content={expectedContent}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 200) < 2,
                $"Border box width should be 200px (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedContent) < 2,
                $"Content width should be {expectedContent}px (got {item.ContentRect.Width})");
        }

        // [CSS-SIZING3 §3.1] border-box height includes padding and border
        [Fact]
        public void BorderBox_Height_IncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:100px;height:100px;padding:10px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentHeight = 100 - 10 - 10 - 5 - 5;
            _output.WriteLine($"content h={item.ContentRect.Height}, border h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 100) < 2,
                $"Border box height should be 100px (got {item.BorderRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Content height should be {expectedContentHeight}px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis with border-box is the border box size
        [Fact]
        public void BorderBox_FlexBasis_IsBorderBoxSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;flex:0 0 150px;padding:20px;border:5px solid black;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContent = 150 - 20 - 20 - 5 - 5;
            _output.WriteLine($"content={item.ContentRect.Width}, border={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 150) < 2,
                $"Border box width should be 150px from flex-basis (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedContent) < 2,
                $"Content width should be {expectedContent}px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] flex-grow distributes space after accounting for border-box sizes
        [Fact]
        public void BorderBox_FlexGrow_DistributesRemainingSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='fixed' style='box-sizing:border-box;width:100px;padding:10px;border:5px solid black;height:40px'></div>
                    <div id='grow' style='box-sizing:border-box;flex-grow:1;padding:10px;border:5px solid black;height:40px'></div>
                </div></body>");
            var fixedItem = LayoutTestHelper.FindById(root, "fixed")!;
            var growItem = LayoutTestHelper.FindById(root, "grow")!;
            _output.WriteLine($"fixed border={fixedItem.BorderRect.Width}, grow border={growItem.BorderRect.Width}");
            Assert.True(System.Math.Abs(fixedItem.BorderRect.Width - 100) < 2,
                $"Fixed item border box should be 100px (got {fixedItem.BorderRect.Width})");
            Assert.True(System.Math.Abs(growItem.BorderRect.Width - 300) < 2,
                $"Grow item border box should be 300px (400 - 100) (got {growItem.BorderRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] flex-shrink with border-box items
        [Fact]
        public void BorderBox_FlexShrink_ShrinksBorderBoxSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='box-sizing:border-box;flex:0 1 150px;padding:10px;height:40px'></div>
                    <div id='b' style='box-sizing:border-box;flex:0 1 150px;padding:10px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalBorderWidth = itemA.BorderRect.Width + itemB.BorderRect.Width;
            _output.WriteLine($"a border={itemA.BorderRect.Width}, b border={itemB.BorderRect.Width}, total={totalBorderWidth}");
            Assert.True(System.Math.Abs(totalBorderWidth - 200) < 3,
                $"Total border box widths should sum to 200px (got {totalBorderWidth})");
            Assert.True(System.Math.Abs(itemA.BorderRect.Width - itemB.BorderRect.Width) < 2,
                $"Both items should shrink equally");
        }

        // [CSS-SIZING3 §3.1] percentage width resolves to border box
        [Fact]
        public void BorderBox_PercentageWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:50%;padding:20px;border:5px solid black;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContent = 200 - 20 - 20 - 5 - 5;
            _output.WriteLine($"content={item.ContentRect.Width}, border={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 200) < 2,
                $"Border box width should be 200px (50% of 400) (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedContent) < 2,
                $"Content width should be {expectedContent}px (got {item.ContentRect.Width})");
        }

        // [CSS-VALUES4 §8.1] calc() width with border-box
        [Fact]
        public void BorderBox_CalcWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:calc(100px + 50px);padding:10px;border:5px solid black;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContent = 150 - 10 - 10 - 5 - 5;
            _output.WriteLine($"content={item.ContentRect.Width}, border={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 150) < 2,
                $"Border box width should be 150px from calc (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedContent) < 2,
                $"Content width should be {expectedContent}px (got {item.ContentRect.Width})");
        }

        // [CSS-SIZING3 §4] min-width with border-box prevents shrinking below border box minimum
        [Fact]
        public void BorderBox_MinWidth_PreventsUndersize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;flex:0 1 200px;min-width:120px;padding:10px;border:5px solid black;height:40px'></div>
                    <div style='flex:0 0 350px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"border width={item.BorderRect.Width}");
            Assert.True(item.BorderRect.Width >= 118,
                $"Border box width should be at least 120px due to min-width (got {item.BorderRect.Width})");
        }

        // [CSS-SIZING3 §4] max-width with border-box constrains the item
        [Fact]
        public void BorderBox_MaxWidth_CapsSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:300px;max-width:200px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"border width={item.BorderRect.Width}");
            Assert.True(item.BorderRect.Width <= 202,
                $"Border box width should not exceed 200px due to max-width (got {item.BorderRect.Width})");
        }

        // [CSS-SIZING3 §3.1] content-box vs border-box comparison: same declared width, different content widths
        [Fact]
        public void ContentBox_Vs_BorderBox_ContentWidthDiffers()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='content' style='box-sizing:content-box;width:150px;padding:10px;border:5px solid black;height:40px'></div>
                    <div id='border' style='box-sizing:border-box;width:150px;padding:10px;border:5px solid black;height:40px'></div>
                </div></body>");
            var contentBoxItem = LayoutTestHelper.FindById(root, "content")!;
            var borderBoxItem = LayoutTestHelper.FindById(root, "border")!;
            _output.WriteLine($"content-box: content={contentBoxItem.ContentRect.Width}, border={contentBoxItem.BorderRect.Width}");
            _output.WriteLine($"border-box: content={borderBoxItem.ContentRect.Width}, border={borderBoxItem.BorderRect.Width}");
            Assert.True(System.Math.Abs(contentBoxItem.ContentRect.Width - 150) < 2,
                $"content-box item content width should be 150px (got {contentBoxItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(contentBoxItem.BorderRect.Width - 180) < 2,
                $"content-box item border width should be 180px (got {contentBoxItem.BorderRect.Width})");
            Assert.True(System.Math.Abs(borderBoxItem.ContentRect.Width - 120) < 2,
                $"border-box item content width should be 120px (got {borderBoxItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(borderBoxItem.BorderRect.Width - 150) < 2,
                $"border-box item border width should be 150px (got {borderBoxItem.BorderRect.Width})");
        }

        // [CSS-FLEXBOX §9] border-box in column flex direction
        [Fact]
        public void BorderBox_ColumnFlex_HeightIncludesPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:400px'>
                    <div id='item' style='box-sizing:border-box;height:100px;padding:15px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentHeight = 100 - 15 - 15 - 5 - 5;
            _output.WriteLine($"content h={item.ContentRect.Height}, border h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 100) < 2,
                $"Border box height should be 100px in column flex (got {item.BorderRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Content height should be {expectedContentHeight}px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9] border-box in column flex with flex-basis
        [Fact]
        public void BorderBox_ColumnFlex_FlexBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:400px'>
                    <div id='item' style='box-sizing:border-box;flex:0 0 120px;padding:10px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentHeight = 120 - 10 - 10 - 5 - 5;
            _output.WriteLine($"content h={item.ContentRect.Height}, border h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 120) < 2,
                $"Border box height should be 120px from flex-basis (got {item.BorderRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Content height should be {expectedContentHeight}px (got {item.ContentRect.Height})");
        }

        // [CSS-BOX4 §6] margin is always outside border-box
        [Fact]
        public void BorderBox_MarginIsOutsideBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:200px;padding:10px;border:5px solid black;margin:20px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"border={item.BorderRect.Width}, margin={item.MarginRect.Width}, content x={item.ContentRect.X}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 200) < 2,
                $"Border box width should still be 200px with margin (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.MarginRect.Width - 240) < 2,
                $"Margin box width should be 240px (200 + 20 + 20) (got {item.MarginRect.Width})");
        }

        // [CSS-SIZING3 §3.1] asymmetric padding with border-box
        [Fact]
        public void BorderBox_AsymmetricPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:200px;padding:5px 30px 15px 10px;border:3px solid black;height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentWidth = 200 - 10 - 30 - 3 - 3;
            float expectedContentHeight = 80 - 5 - 15 - 3 - 3;
            _output.WriteLine($"content w={item.ContentRect.Width} h={item.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 200) < 2,
                $"Border box width should be 200px (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedContentWidth) < 2,
                $"Content width should be {expectedContentWidth}px (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 80) < 2,
                $"Border box height should be 80px (got {item.BorderRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Content height should be {expectedContentHeight}px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §8.3] gap with border-box items: gap is between border boxes
        [Fact]
        public void BorderBox_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;gap:20px'>
                    <div id='a' style='box-sizing:border-box;flex:1;padding:10px;border:5px solid black;height:40px'></div>
                    <div id='b' style='box-sizing:border-box;flex:1;padding:10px;border:5px solid black;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float gapBetween = itemB.BorderRect.X - (itemA.BorderRect.X + itemA.BorderRect.Width);
            _output.WriteLine($"a border={itemA.BorderRect.Width}, b border={itemB.BorderRect.Width}, gap={gapBetween}");
            Assert.True(System.Math.Abs(gapBetween - 20) < 2,
                $"Gap between border boxes should be 20px (got {gapBetween})");
            Assert.True(System.Math.Abs(itemA.BorderRect.Width - itemB.BorderRect.Width) < 2,
                $"Both items should have equal border box width");
        }

        // [CSS-FLEXBOX §9] two items splitting container evenly with border-box
        [Fact]
        public void BorderBox_TwoItems_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='box-sizing:border-box;flex:1;padding:10px;border:5px solid black;height:40px'></div>
                    <div id='b' style='box-sizing:border-box;flex:1;padding:10px;border:5px solid black;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalBorderWidth = itemA.BorderRect.Width + itemB.BorderRect.Width;
            _output.WriteLine($"a border={itemA.BorderRect.Width}, b border={itemB.BorderRect.Width}, total={totalBorderWidth}");
            Assert.True(System.Math.Abs(totalBorderWidth - 300) < 3,
                $"Total border box widths should sum to 300px (got {totalBorderWidth})");
            Assert.True(System.Math.Abs(itemA.BorderRect.Width - 150) < 2,
                $"Each item border box should be 150px (got a={itemA.BorderRect.Width})");
        }

        // [CSS-ALIGN3 §6.1] align-items:stretch with border-box height
        [Fact]
        public void BorderBox_AlignItemsStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:200px;align-items:stretch'>
                    <div id='item' style='box-sizing:border-box;width:100px;padding:10px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"border h={item.BorderRect.Height}, content h={item.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 200) < 2,
                $"Stretched border box height should be 200px (got {item.BorderRect.Height})");
            float expectedContentHeight = 200 - 10 - 10 - 5 - 5;
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Content height should be {expectedContentHeight}px after stretch (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] flex-grow with unequal border-box padding distributes free space equally by content
        [Fact]
        public void BorderBox_FlexGrow_UnequalPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='box-sizing:border-box;flex:1;padding:5px;height:40px'></div>
                    <div id='b' style='box-sizing:border-box;flex:1;padding:30px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalBorderWidth = itemA.BorderRect.Width + itemB.BorderRect.Width;
            _output.WriteLine($"a border={itemA.BorderRect.Width} content={itemA.ContentRect.Width}, b border={itemB.BorderRect.Width} content={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(totalBorderWidth - 300) < 3,
                $"Total border box widths should sum to 300px (got {totalBorderWidth})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - itemB.ContentRect.Width) < 2,
                $"Both items should have equal content width (a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width})");
            Assert.True(itemA.BorderRect.Width < itemB.BorderRect.Width,
                $"Item A (less padding) should have smaller border box than B (a={itemA.BorderRect.Width}, b={itemB.BorderRect.Width})");
        }

        // [CSS-SIZING3 §4] min-height with border-box in column flex
        [Fact]
        public void BorderBox_ColumnFlex_MinHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px'>
                    <div id='item' style='box-sizing:border-box;min-height:80px;padding:10px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"border h={item.BorderRect.Height}");
            Assert.True(item.BorderRect.Height >= 78,
                $"Border box height should be at least 80px due to min-height (got {item.BorderRect.Height})");
        }

        // [CSS-SIZING3 §4] max-height with border-box in column flex constrains the item
        [Fact]
        public void BorderBox_ColumnFlex_MaxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:400px'>
                    <div id='item' style='box-sizing:border-box;height:200px;max-height:100px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"border h={item.BorderRect.Height}");
            Assert.True(item.BorderRect.Height <= 102,
                $"Border box height should not exceed 100px due to max-height (got {item.BorderRect.Height})");
        }

        // [CSS-FLEXBOX §9.2] border-box flex-basis:0 means zero border box, content may be negative → clamped
        [Fact]
        public void BorderBox_FlexBasisZero_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='box-sizing:border-box;flex:1 0 0px;padding:10px;height:40px'></div>
                    <div id='b' style='box-sizing:border-box;flex:1 0 0px;padding:10px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalBorderWidth = itemA.BorderRect.Width + itemB.BorderRect.Width;
            _output.WriteLine($"a border={itemA.BorderRect.Width}, b border={itemB.BorderRect.Width}, total={totalBorderWidth}");
            Assert.True(System.Math.Abs(totalBorderWidth - 300) < 3,
                $"Total border box widths should sum to 300px (got {totalBorderWidth})");
        }

        // [CSS-SIZING3 §3.1] border-box with only border (no padding)
        [Fact]
        public void BorderBox_OnlyBorder_NoPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:200px;border:15px solid black;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content={item.ContentRect.Width}, border={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 200) < 2,
                $"Border box width should be 200px (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 170) < 2,
                $"Content width should be 170px (200 - 15 - 15) (got {item.ContentRect.Width})");
        }

        // [CSS-SIZING3 §3.1] border-box with only padding (no border)
        [Fact]
        public void BorderBox_OnlyPadding_NoBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:200px;padding:25px;height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content={item.ContentRect.Width}, border={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 200) < 2,
                $"Border box width should be 200px (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"Content width should be 150px (200 - 25 - 25) (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9] three items with border-box, one fixed, two growing
        [Fact]
        public void BorderBox_ThreeItems_MixedFlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='fixed' style='box-sizing:border-box;width:100px;padding:10px;height:40px'></div>
                    <div id='grow1' style='box-sizing:border-box;flex:1;padding:10px;height:40px'></div>
                    <div id='grow2' style='box-sizing:border-box;flex:2;padding:10px;height:40px'></div>
                </div></body>");
            var fixedItem = LayoutTestHelper.FindById(root, "fixed")!;
            var grow1 = LayoutTestHelper.FindById(root, "grow1")!;
            var grow2 = LayoutTestHelper.FindById(root, "grow2")!;
            float totalBorderWidth = fixedItem.BorderRect.Width + grow1.BorderRect.Width + grow2.BorderRect.Width;
            _output.WriteLine($"fixed={fixedItem.BorderRect.Width}, grow1={grow1.BorderRect.Width}, grow2={grow2.BorderRect.Width}");
            Assert.True(System.Math.Abs(fixedItem.BorderRect.Width - 100) < 2,
                $"Fixed item should be 100px (got {fixedItem.BorderRect.Width})");
            Assert.True(System.Math.Abs(totalBorderWidth - 400) < 3,
                $"Total should sum to 400px (got {totalBorderWidth})");
            Assert.True(grow2.ContentRect.Width > grow1.ContentRect.Width,
                $"grow2 content should be wider than grow1 (grow1={grow1.ContentRect.Width}, grow2={grow2.ContentRect.Width})");
            Assert.True(System.Math.Abs(grow2.ContentRect.Width - grow1.ContentRect.Width * 2) < 3,
                $"grow2 content should be 2x grow1 content (grow1={grow1.ContentRect.Width}, grow2={grow2.ContentRect.Width})");
        }

        // [CSS-BOX4 §6] margin between border-box items affects positions
        [Fact]
        public void BorderBox_ItemSpacing_WithMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='box-sizing:border-box;width:100px;padding:10px;margin-right:30px;height:40px'></div>
                    <div id='b' style='box-sizing:border-box;width:100px;padding:10px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a border x={itemA.BorderRect.X} w={itemA.BorderRect.Width}, b border x={itemB.BorderRect.X}");
            Assert.True(System.Math.Abs(itemA.BorderRect.Width - 100) < 2,
                $"Item A border box should be 100px (got {itemA.BorderRect.Width})");
            Assert.True(System.Math.Abs(itemB.BorderRect.X - 130) < 2,
                $"Item B should start at 130px (100 + 30 margin) (got {itemB.BorderRect.X})");
        }

        // [CSS-SIZING3 §3.1] large padding that exceeds declared width: content clamped to 0
        [Fact]
        public void BorderBox_PaddingExceedsWidth_ContentClamped()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:100px;padding:60px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content={item.ContentRect.Width}, border={item.BorderRect.Width}");
            Assert.True(item.ContentRect.Width >= -1,
                $"Content width should be 0 or non-negative when padding exceeds width (got {item.ContentRect.Width})");
            Assert.True(item.BorderRect.Width >= 118,
                $"Border box should be at least 120px (padding 60+60) (got {item.BorderRect.Width})");
        }

        // [CSS-FLEXBOX §9] column flex-grow with border-box distributes height
        [Fact]
        public void BorderBox_ColumnFlex_FlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:300px'>
                    <div id='a' style='box-sizing:border-box;flex:1;padding:10px;border:5px solid black'></div>
                    <div id='b' style='box-sizing:border-box;flex:1;padding:10px;border:5px solid black'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a border h={itemA.BorderRect.Height}, b border h={itemB.BorderRect.Height}");
            Assert.True(System.Math.Abs(itemA.BorderRect.Height - 150) < 2,
                $"Each item should get 150px border height (got a={itemA.BorderRect.Height})");
            Assert.True(System.Math.Abs(itemB.BorderRect.Height - 150) < 2,
                $"Each item should get 150px border height (got b={itemB.BorderRect.Height})");
        }

        // [CSS-FLEXBOX §9] border-box width with asymmetric borders
        [Fact]
        public void BorderBox_AsymmetricBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:200px;border-left:10px solid black;border-right:20px solid black;border-top:3px solid black;border-bottom:7px solid black;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedContentWidth = 200 - 10 - 20;
            float expectedContentHeight = 60 - 3 - 7;
            _output.WriteLine($"content w={item.ContentRect.Width} h={item.ContentRect.Height}, border w={item.BorderRect.Width} h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 200) < 2,
                $"Border box width should be 200px (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedContentWidth) < 2,
                $"Content width should be {expectedContentWidth}px (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 60) < 2,
                $"Border box height should be 60px (got {item.BorderRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - expectedContentHeight) < 2,
                $"Content height should be {expectedContentHeight}px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] weighted flex-shrink with border-box: shrink ratio uses border box basis
        [Fact]
        public void BorderBox_FlexShrink_WeightedDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='box-sizing:border-box;flex:0 1 180px;padding:10px;height:40px'></div>
                    <div id='b' style='box-sizing:border-box;flex:0 3 180px;padding:10px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a border={itemA.BorderRect.Width}, b border={itemB.BorderRect.Width}");
            Assert.True(itemA.BorderRect.Width > itemB.BorderRect.Width,
                $"Item A (shrink:1) should be wider than B (shrink:3) (a={itemA.BorderRect.Width}, b={itemB.BorderRect.Width})");
            float totalBorderWidth = itemA.BorderRect.Width + itemB.BorderRect.Width;
            Assert.True(System.Math.Abs(totalBorderWidth - 200) < 3,
                $"Total border box widths should sum to container (got {totalBorderWidth})");
        }

        // [CSS-ALIGN3 §6.1] align-items:stretch with explicit border-box height should not override
        [Fact]
        public void BorderBox_AlignItemsStretch_ExplicitHeightNotOverridden()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:200px;align-items:stretch'>
                    <div id='item' style='box-sizing:border-box;width:100px;height:80px;padding:10px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"border h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 80) < 2,
                $"Explicit border-box height should not be overridden by stretch (got {item.BorderRect.Height})");
        }

        // [CSS-VALUES4 §8.1] calc with percentage width in border-box
        [Fact]
        public void BorderBox_CalcPercentagePlusPixels()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:calc(50% - 20px);padding:10px;border:5px solid black;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float expectedBorderWidth = 180;
            float expectedContentWidth = 180 - 10 - 10 - 5 - 5;
            _output.WriteLine($"content={item.ContentRect.Width}, border={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - expectedBorderWidth) < 2,
                $"Border box width should be {expectedBorderWidth}px (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - expectedContentWidth) < 2,
                $"Content width should be {expectedContentWidth}px (got {item.ContentRect.Width})");
        }
    }
}
