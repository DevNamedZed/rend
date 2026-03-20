using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class CssInheritanceTests
    {
        private readonly ITestOutputHelper _output;
        public CssInheritanceTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Unset_InheritedProperty_InheritsFromParent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='color: red;'>
                    <div id='test' style='color: unset; width: 50px; height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            var styled = box!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            _output.WriteLine($"color=({styled!.Style.Color.R},{styled.Style.Color.G},{styled.Style.Color.B})");
            // unset on inherited property = inherit → red from parent
            Assert.Equal(255, styled.Style.Color.R);
        }

        [Fact]
        public void Unset_NonInheritedProperty_ResetsToInitial()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='border: 5px solid red;'>
                    <div id='test' style='border: unset; width: 50px; height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"border-top={box!.BorderTopWidth}");
            // unset on non-inherited property = initial → 0
            Assert.Equal(0, box.BorderTopWidth);
        }

        [Fact]
        public void LineHeight_Inherits()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='line-height: 2;'>
                    <div id='test' style='width: 50px;'>text</div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            var styled = box!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            _output.WriteLine($"line-height={styled!.Style.LineHeight}");
            // line-height: 2 = unitless multiplier, should inherit
            // Negative encoding = unitless: -2.0
            Assert.True(styled.Style.LineHeight < 0,
                $"Unitless line-height should inherit as negative (got {styled.Style.LineHeight})");
        }

        [Fact]
        public void WhiteSpace_Inherits()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='white-space: nowrap;'>
                    <div id='test' style='width: 50px;'>text</div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            var styled = box!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            _output.WriteLine($"white-space={styled!.Style.WhiteSpace}");
            Assert.Equal(CssWhiteSpace.Nowrap, styled.Style.WhiteSpace);
        }

        [Fact]
        public void TextAlign_Inherits()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='text-align: center;'>
                    <div id='test' style='width: 200px;'>text</div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            var styled = box!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            _output.WriteLine($"text-align={styled!.Style.TextAlign}");
            Assert.Equal(CssTextAlign.Center, styled.Style.TextAlign);
        }

        [Fact]
        public void Direction_Inherits()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='direction: rtl;'>
                    <div id='test' style='width: 200px;'>text</div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            var styled = box!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            Assert.Equal(CssDirection.Rtl, styled!.Style.Direction);
        }

        [Fact]
        public void FontWeight_Inherits()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-weight: bold;'>
                    <div id='test' style='width: 50px;'>text</div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            var styled = box!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            _output.WriteLine($"font-weight={styled!.Style.FontWeight}");
            Assert.Equal(700, styled.Style.FontWeight);
        }

        [Fact]
        public void LetterSpacing_Inherits()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='letter-spacing: 2px;'>
                    <div id='test' style='width: 200px;'>text</div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            var styled = box!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            _output.WriteLine($"letter-spacing={styled!.Style.LetterSpacing}");
            Assert.True(System.Math.Abs(styled.Style.LetterSpacing - 2) < 0.1f,
                $"letter-spacing should inherit (got {styled.Style.LetterSpacing})");
        }
    }
}
