using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class CssValueResolutionTests
    {
        private readonly ITestOutputHelper _output;
        public CssValueResolutionTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Clamp_Width_Resolves()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <div id='test' style='width: clamp(50px, 50%, 200px); height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            // clamp(50, 200, 200) = max(50, min(200, 200)) = 200
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"clamp(50px, 50%, 200px) of 400 should be 200 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Clamp_Width_LowerBound()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 60px;'>
                    <div id='test' style='width: clamp(50px, 50%, 200px); height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            // clamp(50, 30, 200) = max(50, min(30, 200)) = max(50, 30) = 50
            Assert.True(System.Math.Abs(box.ContentRect.Width - 50) < 2,
                $"clamp should use lower bound (got {box.ContentRect.Width})");
        }

        [Fact]
        public void OverflowClip_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='overflow: clip; width: 100px; height: 50px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            var styled = box!.StyledNode as StyledElement;
            _output.WriteLine($"overflow-x={styled?.Style.OverflowX} overflow-y={styled?.Style.OverflowY}");
        }

        [Fact]
        public void Opacity_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='opacity: 0.5; width: 50px; height: 50px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            var styled = box!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            _output.WriteLine($"opacity={styled!.Style.Opacity}");
            Assert.True(System.Math.Abs(styled.Style.Opacity - 0.5f) < 0.01f,
                $"opacity should be 0.5 (got {styled.Style.Opacity})");
        }

        [Fact]
        public void Visibility_Hidden_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='visibility: hidden; width: 50px; height: 50px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            var styled = box!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            Assert.Equal(CssVisibility.Hidden, styled!.Style.Visibility);
            // visibility:hidden still takes space
            Assert.True(box.ContentRect.Width >= 49, $"visibility:hidden takes space (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Display_InlineBlock_InFlex_NoWrap()
        {
            // inline-block inside flex should become flex item (blockified)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 200px;'>
                    <span id='a' style='display: inline-block; width: 50px; height: 30px;'></span>
                    <span id='b' style='display: inline-block; width: 50px; height: 30px;'></span>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            Assert.True(b!.ContentRect.X > a!.ContentRect.X,
                $"inline-block in flex should lay out horizontally");
        }

        [Fact]
        public void Inherit_FontSize_Cascades()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size: 24px;'>
                    <div id='child' style='font-size: inherit; width: 50px; height: 20px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            var styled = child!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            _output.WriteLine($"font-size={styled!.Style.FontSize}");
            Assert.True(System.Math.Abs(styled.Style.FontSize - 24) < 1,
                $"font-size: inherit should be 24 (got {styled.Style.FontSize})");
        }

        [Fact]
        public void Initial_FontSize_Resets()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size: 24px;'>
                    <div id='child' style='font-size: initial; width: 50px; height: 20px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            var styled = child!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            _output.WriteLine($"font-size={styled!.Style.FontSize}");
            // initial resets to medium = 16px
            Assert.True(System.Math.Abs(styled.Style.FontSize - 16) < 1,
                $"font-size: initial should be 16 (got {styled.Style.FontSize})");
        }

        [Fact]
        public void MultipleBackgrounds_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='background: red, blue; width: 50px; height: 50px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            // Just verify it doesn't crash
            Assert.True(box!.ContentRect.Width >= 49);
        }

        [Fact]
        public void NegativeZIndex_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='position: relative; z-index: -1; width: 50px; height: 50px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            var styled = box!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            _output.WriteLine($"z-index={styled!.Style.ZIndex}");
            Assert.Equal(-1, styled.Style.ZIndex);
        }

        [Fact]
        public void Min_Width_Resolves()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <div id='test' style='width: min(300px, 50%); height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            // min(300, 200) = 200
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"min(300px, 50%) of 400 should be 200 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Max_Width_Resolves()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <div id='test' style='width: max(100px, 25%); height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            // max(100, 100) = 100
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2,
                $"max(100px, 25%) of 400 should be 100 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void PercentagePadding_ResolvesAgainstWidth()
        {
            // CSS spec: percentage padding always resolves against containing block WIDTH
            // (even for padding-top/bottom)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div id='test' style='padding: 10%; height: 0;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"padding: T={box!.PaddingTop} R={box.PaddingRight} B={box.PaddingBottom} L={box.PaddingLeft}");
            // 10% of 200px = 20px for ALL sides
            Assert.True(System.Math.Abs(box.PaddingTop - 20) < 2, $"padding-top 10% of 200 = 20 (got {box.PaddingTop})");
            Assert.True(System.Math.Abs(box.PaddingLeft - 20) < 2, $"padding-left 10% of 200 = 20 (got {box.PaddingLeft})");
        }
    }
}
