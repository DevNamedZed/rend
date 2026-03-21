using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests verifying that CSS color-related properties interact correctly
    /// with layout: border-color, outline, background-color, opacity,
    /// visibility, currentColor, and border-style effects on computed widths.
    /// </summary>
    public class WptCssColorLayoutTests
    {
        private readonly ITestOutputHelper _output;

        public WptCssColorLayoutTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §8.5.1] border-color alone does not create border space; border-width does
        [Fact]
        public void BorderColor_WithoutWidth_NoLayoutEffect()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='border-color:red;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
            Assert.Equal(0, target.BorderTopWidth);
        }

        // [CSS2 §8.5.1] border-style:none forces border-width to compute to 0
        [Fact]
        public void BorderStyleNone_ForcesBorderWidthToZero()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='border-width:10px;border-style:none;border-color:red;width:100px;height:50px'></div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(0, target.BorderTopWidth);
            Assert.Equal(0, target.BorderRightWidth);
            Assert.Equal(0, target.BorderBottomWidth);
            Assert.Equal(0, target.BorderLeftWidth);
        }

        // [CSS2 §8.5.1] border-style:hidden also forces border-width to compute to 0
        [Fact]
        public void BorderStyleHidden_ForcesBorderWidthToZero()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='border-width:8px;border-style:hidden;border-color:blue;width:100px;height:50px'></div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(0, target.BorderTopWidth);
            Assert.Equal(0, target.BorderBottomWidth);
        }

        // [CSS2 §8] border:2px solid red — border occupies layout space
        [Fact]
        public void BorderSolidColor_TakesLayoutSpace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='border:2px solid red;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(2, target.BorderTopWidth);
            Assert.Equal(2, target.BorderLeftWidth);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 196) < 2);
        }

        // [CSS-UI §4] outline does not affect layout — sibling is not displaced
        [Fact]
        public void Outline_DoesNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='outline:20px solid red;height:40px'></div>
                    <div id='sibling' style='height:30px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 40) < 2);
        }

        // [CSS-UI §4] outline does not reduce content width
        [Fact]
        public void Outline_DoesNotReduceContentWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='outline:10px solid green;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
        }

        // [CSS3-COLOR] background-color does not affect layout dimensions
        [Fact]
        public void BackgroundColor_DoesNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:150px'>
                    <div id='t' style='background-color:yellow;height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2);
        }

        // [CSS3-COLOR] background-color does not displace siblings
        [Fact]
        public void BackgroundColor_DoesNotDisplaceSiblings()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='background-color:lime;height:30px'></div>
                    <div id='sibling' style='height:20px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 30) < 2);
        }

        // [CSS3-COLOR §3.2] opacity does not affect layout
        [Fact]
        public void Opacity_DoesNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='opacity:0.5;height:40px'></div>
                    <div id='sibling' style='height:30px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 40) < 2);
        }

        // [CSS3-COLOR §3.2] opacity:0 element still takes layout space
        [Fact]
        public void OpacityZero_StillTakesLayoutSpace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='opacity:0;height:50px'></div>
                    <div id='sibling' style='height:30px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 50) < 2);
        }

        // [CSS2 §11.2] visibility:hidden — element takes space but is invisible
        [Fact]
        public void VisibilityHidden_TakesLayoutSpace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='visibility:hidden;height:60px'></div>
                    <div id='sibling' style='height:20px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 60) < 2);
        }

        // [CSS2 §11.2] visibility:hidden with border — border space is preserved
        [Fact]
        public void VisibilityHidden_WithBorder_PreservesSpace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='visibility:hidden;border:5px solid red;height:40px'></div>
                    <div id='sibling' style='height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            Assert.Equal(5, target.BorderTopWidth);
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 50) < 2);
        }

        // [CSS2 §8.5.1] border-color:transparent with border-width > 0 still takes space
        [Fact]
        public void TransparentBorderColor_StillTakesLayoutSpace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='border:4px solid transparent;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(4, target.BorderTopWidth);
            Assert.Equal(4, target.BorderLeftWidth);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 192) < 2);
        }

        // [CSS2 §8.5.1] transparent border displaces sibling correctly
        [Fact]
        public void TransparentBorder_DisplacesSibling()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='border:6px solid transparent;height:30px'></div>
                    <div id='sibling' style='height:20px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 42) < 2);
        }

        // [CSS2 §8.5] currentColor in border resolves to inherited color
        [Fact]
        public void CurrentColor_InBorder_ResolvesToInheritedColor()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='color:rgb(0,128,0)'>
                    <div id='t' style='border:3px solid currentColor;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var styled = (target.StyledNode as StyledElement)!;
            Assert.Equal(3, target.BorderTopWidth);
            Assert.Equal(0, styled.Style.BorderTopColor.R);
            Assert.Equal(128, styled.Style.BorderTopColor.G);
        }

        // [CSS2 §8] border shorthand with color sets all sides uniformly
        [Fact]
        public void BorderShorthand_WithColor_SetsAllSides()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:100px'>
                    <div id='t' style='border:3px solid blue;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(3, target.BorderTopWidth);
            Assert.Equal(3, target.BorderRightWidth);
            Assert.Equal(3, target.BorderBottomWidth);
            Assert.Equal(3, target.BorderLeftWidth);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 94) < 2);
        }

        // [CSS2 §8.5] different border colors per side — same widths, no layout difference
        [Fact]
        public void DifferentBorderColors_SameWidths_SameLayout()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='border-width:4px;border-style:solid;border-top-color:red;border-right-color:green;border-bottom-color:blue;border-left-color:orange;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(4, target.BorderTopWidth);
            Assert.Equal(4, target.BorderRightWidth);
            Assert.Equal(4, target.BorderBottomWidth);
            Assert.Equal(4, target.BorderLeftWidth);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 192) < 2);
        }

        // [CSS2 §8.5.1] border-style controls computed border-width: none → 0
        [Fact]
        public void BorderStyle_None_OverridesBorderWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div id='t' style='border-width:5px;border-style:none;border-color:red;width:100px;height:40px'></div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(0, target.BorderTopWidth);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2);
        }

        // [CSS2 §8.5.1] border-style per side: mix of solid and none
        [Fact]
        public void BorderStyle_MixedSolidAndNone_AffectsLayout()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='border-width:5px;border-top-style:solid;border-right-style:none;border-bottom-style:solid;border-left-style:none;border-color:red;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(5, target.BorderTopWidth);
            Assert.Equal(0, target.BorderRightWidth);
            Assert.Equal(5, target.BorderBottomWidth);
            Assert.Equal(0, target.BorderLeftWidth);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
        }

        // [CSS2 §8] border + padding + color: border and padding reduce content, color is visual only
        [Fact]
        public void BorderColorWithPadding_ContentBoxReduction()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='border:3px solid red;padding:7px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2);
        }

        // [CSS-UI §4] outline-color does not affect layout
        [Fact]
        public void OutlineColor_DoesNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='outline:5px solid red;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
            Assert.Equal(0, target.BorderTopWidth);
        }

        // [CSS2 §8.5] initial border-style is none, so border-width defaults to 0 even with color
        [Fact]
        public void DefaultBorderStyle_IsNone_NoBorderSpace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='border-width:5px;border-color:red;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(0, target.BorderTopWidth);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
        }

        // [CSS2 §8] border-color inherits via currentColor when not specified
        [Fact]
        public void BorderColor_DefaultsToCurrentColor()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='color:rgb(255,0,128)'>
                    <div id='t' style='border:2px solid;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var styled = (target.StyledNode as StyledElement)!;
            Assert.Equal(2, target.BorderTopWidth);
            Assert.Equal(255, styled.Style.BorderTopColor.R);
            Assert.Equal(0, styled.Style.BorderTopColor.G);
            Assert.Equal(128, styled.Style.BorderTopColor.B);
        }

        // [CSS2 §11.2] display:none takes no layout space (vs visibility:hidden)
        [Fact]
        public void DisplayNone_VsVisibilityHidden_LayoutDifference()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='display:none;height:50px;background-color:red'></div>
                    <div id='afterNone' style='height:20px'></div>
                </div></body>");
            var afterNone = LayoutTestHelper.FindById(root, "afterNone")!;
            Assert.True(System.Math.Abs(afterNone.ContentRect.Y - 0) < 2);
        }

        // Outline + border together: only border affects layout
        [Fact]
        public void OutlineAndBorder_OnlyBorderAffectsLayout()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='border:3px solid blue;outline:10px solid red;height:40px'></div>
                    <div id='sibling' style='height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            Assert.Equal(3, target.BorderTopWidth);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 194) < 2);
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 46) < 2);
        }
    }
}
