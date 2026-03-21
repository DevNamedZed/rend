using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class CssSpecificPropertyTests
    {
        private readonly ITestOutputHelper _output;
        public CssSpecificPropertyTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Isolation_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='isolation: isolate; width: 50px; height: 50px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssIsolation.Isolate, styled.Style.Isolation);
        }

        [Fact]
        public void MixBlendMode_Multiply()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='mix-blend-mode: multiply; width: 50px; height: 50px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssMixBlendMode.Multiply, styled.Style.MixBlendMode);
        }

        [Fact]
        public void ImageRendering_Pixelated()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='image-rendering: pixelated; width: 50px; height: 50px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssImageRendering.Pixelated, styled.Style.ImageRendering);
        }

        [Fact]
        public void Contain_Layout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='contain: layout; width: 100px; height: 50px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssContain.Layout, styled.Style.Contain);
        }

        [Fact]
        public void OutlineOffset_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='outline: 2px solid red; outline-offset: 5px; width: 50px; height: 50px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.True(System.Math.Abs(styled.Style.OutlineOffset - 5) < 0.1f,
                $"outline-offset should be 5 (got {styled.Style.OutlineOffset})");
        }

        [Fact]
        public void TextOverflow_Ellipsis_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='text-overflow: ellipsis; overflow: hidden; white-space: nowrap; width: 100px;'>
                    Very long text content that should be truncated
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssTextOverflow.Ellipsis, styled.Style.TextOverflow);
        }

        [Fact]
        public void ColumnSpan_All()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='column-count: 2; width: 200px;'>
                    <div id='span' style='column-span: all; height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "span")!;
            // column-span: all should make the element full container width
            Assert.True(box.ContentRect.Width >= 199,
                $"column-span:all = full width (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Resize_Both()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='resize: both; overflow: auto; width: 100px; height: 100px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssResize.Both, styled.Style.Resize);
        }

        [Fact]
        public void TabSize_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='tab-size: 4; width: 200px;'>text</div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.True(System.Math.Abs(styled.Style.TabSize - 4) < 0.1f,
                $"tab-size should be 4 (got {styled.Style.TabSize})");
        }

        [Fact]
        public void WordBreak_BreakAll_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='word-break: break-all; width: 100px;'>text</div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssWordBreak.BreakAll, styled.Style.WordBreak);
        }

        [Fact]
        public void OverflowWrap_BreakWord_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='overflow-wrap: break-word; width: 100px;'>text</div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflowWrap.BreakWord, styled.Style.OverflowWrap);
        }
    }
}
