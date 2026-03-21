using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for the effect of padding on flex items.
    /// Verifies that padding reduces content area, offsets content position,
    /// and interacts correctly with flex properties per CSS Flexbox and Box Model specs.
    /// </summary>
    public class WptFlexItemPaddingEffectTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexItemPaddingEffectTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-BOX4 §6.1] padding on flex item reduces content width
        [Fact]
        public void PaddingReducesContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:0 0 200px;padding-left:30px;padding-right:20px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content w={item.ContentRect.Width}, border w={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"Content width should be 200px (flex-basis is content-box) (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 250) < 2,
                $"Border box width should be 250px (200 + 30 + 20) (got {item.BorderRect.Width})");
        }

        // [CSS-BOX4 §6.1] padding on flex item reduces content height
        [Fact]
        public void PaddingReducesContentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:0 0 100px;height:80px;padding-top:15px;padding-bottom:25px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content h={item.ContentRect.Height}, border h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"Content height should be 80px (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 120) < 2,
                $"Border box height should be 120px (80 + 15 + 25) (got {item.BorderRect.Height})");
        }

        // [CSS-BOX4 §6.1] padding-left offsets content X position
        [Fact]
        public void PaddingLeftOffsetsContentX()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:0 0 100px;padding-left:25px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content x={item.ContentRect.X}, padding x={item.PaddingRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - 25) < 2,
                $"Content X should be offset by padding-left 25px (got {item.ContentRect.X})");
            Assert.True(System.Math.Abs(item.PaddingRect.X - 0) < 2,
                $"Padding rect X should be at 0 (got {item.PaddingRect.X})");
        }

        // [CSS-BOX4 §6.1] padding-top offsets content Y position
        [Fact]
        public void PaddingTopOffsetsContentY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:0 0 100px;padding-top:20px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content y={item.ContentRect.Y}, padding y={item.PaddingRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 20) < 2,
                $"Content Y should be offset by padding-top 20px (got {item.ContentRect.Y})");
            Assert.True(System.Math.Abs(item.PaddingRect.Y - 0) < 2,
                $"Padding rect Y should be at 0 (got {item.PaddingRect.Y})");
        }

        // [CSS-BOX4 §5] padding shorthand with 1 value applies to all sides
        [Fact]
        public void PaddingShorthandOneValue()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:0 0 100px;padding:15px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"padL={item.PaddingLeft}, padR={item.PaddingRight}, padT={item.PaddingTop}, padB={item.PaddingBottom}");
            Assert.True(System.Math.Abs(item.PaddingLeft - 15) < 1, $"PaddingLeft should be 15 (got {item.PaddingLeft})");
            Assert.True(System.Math.Abs(item.PaddingRight - 15) < 1, $"PaddingRight should be 15 (got {item.PaddingRight})");
            Assert.True(System.Math.Abs(item.PaddingTop - 15) < 1, $"PaddingTop should be 15 (got {item.PaddingTop})");
            Assert.True(System.Math.Abs(item.PaddingBottom - 15) < 1, $"PaddingBottom should be 15 (got {item.PaddingBottom})");
        }

        // [CSS-BOX4 §5] padding shorthand with 2 values: vertical horizontal
        [Fact]
        public void PaddingShorthandTwoValues()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:0 0 100px;padding:10px 20px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"padT={item.PaddingTop}, padR={item.PaddingRight}, padB={item.PaddingBottom}, padL={item.PaddingLeft}");
            Assert.True(System.Math.Abs(item.PaddingTop - 10) < 1, $"PaddingTop should be 10 (got {item.PaddingTop})");
            Assert.True(System.Math.Abs(item.PaddingBottom - 10) < 1, $"PaddingBottom should be 10 (got {item.PaddingBottom})");
            Assert.True(System.Math.Abs(item.PaddingLeft - 20) < 1, $"PaddingLeft should be 20 (got {item.PaddingLeft})");
            Assert.True(System.Math.Abs(item.PaddingRight - 20) < 1, $"PaddingRight should be 20 (got {item.PaddingRight})");
        }

        // [CSS-BOX4 §5] padding shorthand with 4 values: top right bottom left
        [Fact]
        public void PaddingShorthandFourValues()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:0 0 100px;padding:5px 10px 15px 20px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"padT={item.PaddingTop}, padR={item.PaddingRight}, padB={item.PaddingBottom}, padL={item.PaddingLeft}");
            Assert.True(System.Math.Abs(item.PaddingTop - 5) < 1, $"PaddingTop should be 5 (got {item.PaddingTop})");
            Assert.True(System.Math.Abs(item.PaddingRight - 10) < 1, $"PaddingRight should be 10 (got {item.PaddingRight})");
            Assert.True(System.Math.Abs(item.PaddingBottom - 15) < 1, $"PaddingBottom should be 15 (got {item.PaddingBottom})");
            Assert.True(System.Math.Abs(item.PaddingLeft - 20) < 1, $"PaddingLeft should be 20 (got {item.PaddingLeft})");
        }

        // [CSS-BOX4 §5.4] percentage padding resolves against flex container width
        [Fact]
        public void PaddingPercentageResolvesAgainstContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 200px;padding-left:10%;padding-right:5%;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"padL={item.PaddingLeft}, padR={item.PaddingRight}");
            Assert.True(System.Math.Abs(item.PaddingLeft - 40) < 2,
                $"PaddingLeft 10% of 400 should be 40px (got {item.PaddingLeft})");
            Assert.True(System.Math.Abs(item.PaddingRight - 20) < 2,
                $"PaddingRight 5% of 400 should be 20px (got {item.PaddingRight})");
        }

        // [CSS-FLEXBOX §9.7] padding with flex-grow: padded item has larger border box
        [Fact]
        public void PaddingWithFlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='padded' style='flex:1;padding:20px;height:40px'></div>
                    <div id='plain' style='flex:1;height:40px'></div>
                </div></body>");
            var padded = LayoutTestHelper.FindById(root, "padded")!;
            var plain = LayoutTestHelper.FindById(root, "plain")!;
            float paddedBorderWidth = padded.BorderRect.Width;
            float plainBorderWidth = plain.BorderRect.Width;
            _output.WriteLine($"padded border={paddedBorderWidth}, plain border={plainBorderWidth}");
            Assert.True(System.Math.Abs(paddedBorderWidth + plainBorderWidth - 300) < 3,
                $"Total should sum to 300px (got {paddedBorderWidth + plainBorderWidth})");
            Assert.True(paddedBorderWidth > plainBorderWidth,
                $"Padded item border box should be wider than plain (padded={paddedBorderWidth}, plain={plainBorderWidth})");
        }

        // [CSS-FLEXBOX §9.7] padding with flex-shrink: padding is part of the outer size
        [Fact]
        public void PaddingWithFlexShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='item' style='flex:0 1 180px;padding:15px;height:40px'></div>
                    <div style='flex:0 0 100px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content w={item.ContentRect.Width}, border w={item.BorderRect.Width}");
            Assert.True(item.BorderRect.Width < 210,
                $"Padded item should shrink below original 210px (180+30) (got {item.BorderRect.Width})");
            Assert.True(item.BorderRect.Width <= 102,
                $"Padded item border box should be at most 100px (200-100) (got {item.BorderRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] padding with explicit flex-basis
        [Fact]
        public void PaddingWithFlexBasis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 150px;padding:10px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content w={item.ContentRect.Width}, border w={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"Content width should equal flex-basis 150px (content-box) (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 170) < 2,
                $"Border box should be 170px (150 + 10 + 10) (got {item.BorderRect.Width})");
        }

        // [CSS-SIZING3 §3.1] padding with border-box: padding is inside declared width
        [Fact]
        public void PaddingWithBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;flex:0 0 200px;padding:25px;height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content w={item.ContentRect.Width}, border w={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 200) < 2,
                $"Border box width should be 200px (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"Content width should be 150px (200 - 25 - 25) (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 80) < 2,
                $"Border box height should be 80px (got {item.BorderRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 30) < 2,
                $"Content height should be 30px (80 - 25 - 25) (got {item.ContentRect.Height})");
        }

        // [CSS-BOX4 §6] padding combined with border on flex item
        [Fact]
        public void PaddingWithBorderCombined()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 100px;padding:10px;border:5px solid black;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content w={item.ContentRect.Width}, border w={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"Content width should be 100px (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 130) < 2,
                $"Border box should be 130px (100 + 10 + 10 + 5 + 5) (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.PaddingLeft - 10) < 1,
                $"PaddingLeft should be 10 (got {item.PaddingLeft})");
            Assert.True(System.Math.Abs(item.BorderLeftWidth - 5) < 1,
                $"BorderLeftWidth should be 5 (got {item.BorderLeftWidth})");
        }

        // [CSS-ALIGN3 §6.1] padding on a stretch-aligned flex item
        [Fact]
        public void PaddingOnStretchItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:200px;align-items:stretch'>
                    <div id='item' style='flex:0 0 100px;padding:15px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content h={item.ContentRect.Height}, border h={item.BorderRect.Height}, padT={item.PaddingTop}, padB={item.PaddingBottom}");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 200) < 2,
                $"Border box height should stretch to 200px (got {item.BorderRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 170) < 2,
                $"Content height should be 170px (200 - 15 - 15) (got {item.ContentRect.Height})");
        }

        // [CSS-BOX4 §5] asymmetric padding on flex item
        [Fact]
        public void AsymmetricPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 100px;padding:5px 30px 15px 10px;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content w={item.ContentRect.Width}, border w={item.BorderRect.Width}, content h={item.ContentRect.Height}, border h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 140) < 2,
                $"Border box width should be 140px (100 + 10 + 30) (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 80) < 2,
                $"Border box height should be 80px (60 + 5 + 15) (got {item.BorderRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.X - item.PaddingRect.X - 10) < 2,
                $"Content X should be offset by padding-left 10px from padding rect");
        }

        // [CSS-FLEXBOX §9] padding on column-direction flex item
        [Fact]
        public void PaddingOnColumnFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:400px'>
                    <div id='item' style='flex:0 0 100px;padding:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content h={item.ContentRect.Height}, border h={item.BorderRect.Height}, content w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"Content height should be 100px (flex-basis is content-box in column) (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 140) < 2,
                $"Border box height should be 140px (100 + 20 + 20) (got {item.BorderRect.Height})");
            Assert.True(System.Math.Abs(item.PaddingTop - 20) < 1,
                $"PaddingTop should be 20 (got {item.PaddingTop})");
        }

        // [CSS-FLEXBOX §4] padding on inline-flex item
        [Fact]
        public void PaddingOnInlineFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:inline-flex;width:300px'>
                    <div id='item' style='flex:0 0 100px;padding:12px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content w={item.ContentRect.Width}, border w={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"Content width should be 100px in inline-flex (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 124) < 2,
                $"Border box should be 124px (100 + 12 + 12) (got {item.BorderRect.Width})");
        }

        // [CSS-FLEXBOX §8.3] padding on flex item with gap between items
        [Fact]
        public void PaddingWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;gap:20px'>
                    <div id='a' style='flex:0 0 80px;padding:10px;height:40px'></div>
                    <div id='b' style='flex:0 0 80px;padding:10px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float gapBetween = itemB.BorderRect.X - (itemA.BorderRect.X + itemA.BorderRect.Width);
            _output.WriteLine($"a border w={itemA.BorderRect.Width}, b border x={itemB.BorderRect.X}, gap={gapBetween}");
            Assert.True(System.Math.Abs(gapBetween - 20) < 2,
                $"Gap between padded items should be 20px (got {gapBetween})");
            Assert.True(System.Math.Abs(itemA.BorderRect.Width - 100) < 2,
                $"Item A border box should be 100px (80 + 10 + 10) (got {itemA.BorderRect.Width})");
        }

        // [CSS-BOX4 §6] padding combined with margin on flex item
        [Fact]
        public void PaddingWithMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 100px;padding:10px;margin:20px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content w={item.ContentRect.Width}, border w={item.BorderRect.Width}, margin w={item.MarginRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"Content width should be 100px (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 120) < 2,
                $"Border box should be 120px (100 + 10 + 10) (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.MarginRect.Width - 160) < 2,
                $"Margin box should be 160px (120 + 20 + 20) (got {item.MarginRect.Width})");
        }

        // [CSS-BOX4 §5] padding:0 has no effect on content area
        [Fact]
        public void PaddingZeroNoEffect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:0 0 120px;padding:0;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content w={item.ContentRect.Width}, border w={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 1,
                $"Content width should be 120px with padding:0 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 120) < 1,
                $"Border box should equal content box at 120px (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(item.PaddingLeft) < 1,
                $"PaddingLeft should be 0 (got {item.PaddingLeft})");
            Assert.True(System.Math.Abs(item.PaddingTop) < 1,
                $"PaddingTop should be 0 (got {item.PaddingTop})");
        }

        // [CSS-SIZING3 §3.1] large padding exceeds declared item width (content-box)
        [Fact]
        public void LargePaddingExceedsItemWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 50px;padding:40px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content w={item.ContentRect.Width}, border w={item.BorderRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 50) < 2,
                $"Content width should be 50px (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 130) < 2,
                $"Border box should be 130px (50 + 40 + 40) (got {item.BorderRect.Width})");
        }

        // [CSS-SIZING3 §4] padding with min-width constraint
        [Fact]
        public void PaddingWithMinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:0 1 200px;padding:10px;min-width:100px;height:40px'></div>
                    <div style='flex:0 0 250px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content w={item.ContentRect.Width}, border w={item.BorderRect.Width}");
            Assert.True(item.ContentRect.Width >= 98,
                $"Content width should be at least min-width 100px (got {item.ContentRect.Width})");
        }

        // [CSS-SIZING3 §4] padding with max-width constraint (content-box)
        [Fact]
        public void PaddingWithMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:1;padding:10px;max-width:150px;height:40px'></div>
                    <div style='flex:1;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content w={item.ContentRect.Width}, border w={item.BorderRect.Width}");
            Assert.True(item.ContentRect.Width <= 152,
                $"Content width should not exceed max-width 150px (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 170) < 2,
                $"Border box should be 170px (150 content + 10 + 10 padding) (got {item.BorderRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] two items with different padding and equal flex-grow
        [Fact]
        public void TwoItemsDifferentPaddingEqualGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;padding:5px;height:40px'></div>
                    <div id='b' style='flex:1;padding:25px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalBorderWidth = itemA.BorderRect.Width + itemB.BorderRect.Width;
            _output.WriteLine($"a border={itemA.BorderRect.Width} content={itemA.ContentRect.Width}, b border={itemB.BorderRect.Width} content={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(totalBorderWidth - 300) < 3,
                $"Total border box widths should sum to 300px (got {totalBorderWidth})");
            Assert.True(itemA.BorderRect.Width < itemB.BorderRect.Width,
                $"Item A (less padding) should have smaller border box (a={itemA.BorderRect.Width}, b={itemB.BorderRect.Width})");
        }

        // [CSS-BOX4 §6.1] padding-right offsets content area end
        [Fact]
        public void PaddingRightReducesBorderBoxEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 100px;padding-right:30px;height:40px'></div>
                    <div id='next' style='flex:0 0 50px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            var next = LayoutTestHelper.FindById(root, "next")!;
            _output.WriteLine($"item border w={item.BorderRect.Width}, next x={next.ContentRect.X}");
            Assert.True(System.Math.Abs(item.BorderRect.Width - 130) < 2,
                $"Item border box should be 130px (100 + 30) (got {item.BorderRect.Width})");
            Assert.True(System.Math.Abs(next.ContentRect.X - 130) < 2,
                $"Next item should start at 130px (got {next.ContentRect.X})");
        }

        // [CSS-BOX4 §6.1] padding-bottom on flex item affects border box height
        [Fact]
        public void PaddingBottomAffectsBorderBoxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:0 0 100px;padding-bottom:35px;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"content h={item.ContentRect.Height}, border h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2,
                $"Content height should be 50px (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 85) < 2,
                $"Border box height should be 85px (50 + 35) (got {item.BorderRect.Height})");
        }

        // [CSS-FLEXBOX §9] padding on flex item with flex-wrap and overflow
        [Fact]
        public void PaddingWithFlexWrap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='flex:0 0 100px;padding:20px;height:30px'></div>
                    <div id='b' style='flex:0 0 100px;padding:20px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a y={itemA.BorderRect.Y}, b y={itemB.BorderRect.Y}");
            Assert.True(itemB.BorderRect.Y > itemA.BorderRect.Y,
                $"Item B should wrap to next line (a y={itemA.BorderRect.Y}, b y={itemB.BorderRect.Y})");
            Assert.True(System.Math.Abs(itemA.BorderRect.Width - 140) < 2,
                $"Item A border box should be 140px (100 + 20 + 20) (got {itemA.BorderRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] padding on multiple growing items sums correctly
        [Fact]
        public void PaddingOnMultipleGrowingItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1;padding:10px;height:40px'></div>
                    <div id='b' style='flex:1;padding:10px;height:40px'></div>
                    <div id='c' style='flex:1;padding:10px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float totalBorderWidth = itemA.BorderRect.Width + itemB.BorderRect.Width + itemC.BorderRect.Width;
            _output.WriteLine($"a={itemA.BorderRect.Width}, b={itemB.BorderRect.Width}, c={itemC.BorderRect.Width}, total={totalBorderWidth}");
            Assert.True(System.Math.Abs(totalBorderWidth - 400) < 3,
                $"Total border box widths should sum to 400px (got {totalBorderWidth})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - itemB.ContentRect.Width) < 2,
                $"All items should have equal content width");
        }

        // [CSS-FLEXBOX §9] vertical padding on column flex item affects main axis
        [Fact]
        public void VerticalPaddingOnColumnFlexItemAffectsMainAxis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:300px'>
                    <div id='a' style='flex:0 0 80px;padding-top:10px;padding-bottom:20px'></div>
                    <div id='b' style='flex:0 0 80px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a border h={itemA.BorderRect.Height}, b y={itemB.BorderRect.Y}");
            Assert.True(System.Math.Abs(itemA.BorderRect.Height - 110) < 2,
                $"Item A border box height should be 110px (80 + 10 + 20) (got {itemA.BorderRect.Height})");
            Assert.True(System.Math.Abs(itemB.BorderRect.Y - 110) < 2,
                $"Item B should start at 110px (got {itemB.BorderRect.Y})");
        }

        // [CSS-BOX4 §5.4] percentage padding-top resolves against container width (not height)
        [Fact]
        public void PercentagePaddingTopResolvesAgainstContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;height:400px'>
                    <div id='item' style='flex:0 0 80px;padding-top:10%;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            _output.WriteLine($"padT={item.PaddingTop}, container w=200");
            Assert.True(System.Math.Abs(item.PaddingTop - 20) < 2,
                $"Padding-top 10% should resolve to 20px (10% of 200px width) (got {item.PaddingTop})");
        }
    }
}
