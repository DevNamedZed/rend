using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class CssParsingEdgeCases
    {
        private readonly ITestOutputHelper _output;
        public CssParsingEdgeCases(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void NegativeFlexGrow_Rejected()
        {
            // negative flex-grow is invalid → defaults to 0
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 200px;'>
                    <div id='item' style='flex-grow: -1; width: 50px; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            var styled = item!.StyledNode as StyledElement;
            _output.WriteLine($"flex-grow={styled?.Style.FlexGrow} width={item.ContentRect.Width}");
            // Invalid value → initial (0)
            Assert.True(styled!.Style.FlexGrow == 0, $"Negative flex-grow should be rejected (got {styled.Style.FlexGrow})");
            Assert.True(item.ContentRect.Width <= 51, $"No grow (got {item.ContentRect.Width})");
        }

        [Fact]
        public void NegativeFlexShrink_Rejected()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 100px;'>
                    <div id='a' style='flex-shrink: -2; width: 100px; height: 30px;'></div>
                    <div id='b' style='flex-shrink: -3; width: 100px; height: 30px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            _output.WriteLine($"a.w={a!.ContentRect.Width} b.w={b!.ContentRect.Width}");
            // Invalid shrink → default (1). Both items shrink equally: 50px each
            Assert.True(System.Math.Abs(a.ContentRect.Width - 50) < 2,
                $"Invalid shrink → default 1, equal shrink (got a={a.ContentRect.Width})");
        }

        [Fact]
        public void Flex_Shorthand_SingleNumber_BasisIsZero()
        {
            // flex: 2 → flex-grow:2 flex-shrink:1 flex-basis:0
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 300px;'>
                    <div id='a' style='flex: 1; height: 30px;'></div>
                    <div id='b' style='flex: 2; height: 30px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            // flex:1 → basis:0, grow:1. flex:2 → basis:0, grow:2.
            // Distributes 300px: a=100, b=200
            Assert.True(System.Math.Abs(a!.ContentRect.Width - 100) < 2, $"flex:1 (got {a.ContentRect.Width})");
            Assert.True(System.Math.Abs(b!.ContentRect.Width - 200) < 2, $"flex:2 (got {b.ContentRect.Width})");
        }

        [Fact]
        public void Gap_SingleValue_AppliesBoth()
        {
            // gap: 10px → row-gap:10px column-gap:10px
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 10px; width: 210px;'>
                    <div id='a' style='height: 30px;'></div>
                    <div id='b' style='height: 30px;'></div>
                    <div id='c' style='height: 30px;'></div>
                    <div id='d' style='height: 30px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            var c = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(a);
            Assert.NotNull(b);
            Assert.NotNull(c);
            float colGap = b!.ContentRect.X - (a!.ContentRect.X + a.ContentRect.Width);
            float rowGap = c!.ContentRect.Y - (a.ContentRect.Y + a.ContentRect.Height);
            _output.WriteLine($"col-gap={colGap} row-gap={rowGap}");
            Assert.True(System.Math.Abs(colGap - 10) < 2, $"column-gap 10 (got {colGap})");
            Assert.True(System.Math.Abs(rowGap - 10) < 2, $"row-gap 10 (got {rowGap})");
        }

        [Fact]
        public void Overflow_Clip_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='overflow: clip; width: 100px; height: 50px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Clip, styled.Style.OverflowX);
            Assert.Equal(CssOverflow.Clip, styled.Style.OverflowY);
        }

        [Fact]
        public void Overflow_XY_Separate()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='overflow-x: hidden; overflow-y: scroll; width: 100px; height: 50px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Hidden, styled.Style.OverflowX);
            Assert.Equal(CssOverflow.Scroll, styled.Style.OverflowY);
        }

        [Fact]
        public void Display_FlowRoot_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='display: flow-root; width: 100px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssDisplay.FlowRoot, styled.Style.Display);
        }

        [Fact]
        public void Position_Sticky_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='position: sticky; top: 10px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssPosition.Sticky, styled.Style.Position);
        }

        [Fact]
        public void BoxSizing_ContentBox_Default()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width: 100px; padding: 10px; border: 5px solid;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            // Default box-sizing is content-box: width=100 is content only
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 1,
                $"content-box: content width should be 100 (got {box.ContentRect.Width})");
            // Total = 100 + 20(padding) + 10(border) = 130
            float total = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight
                        + box.BorderLeftWidth + box.BorderRightWidth;
            Assert.True(System.Math.Abs(total - 130) < 1, $"Total should be 130 (got {total})");
        }
    }
}
