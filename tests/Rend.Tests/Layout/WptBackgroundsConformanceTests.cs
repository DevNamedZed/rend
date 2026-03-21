using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBackgroundsConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptBackgroundsConformanceTests(ITestOutputHelper output) { _output = output; }

        // background-color parsed
        [Fact]
        public void BackgroundColor_Green()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='background-color:green;width:50px;height:50px'></div></body>");
            var s = (LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!;
            Assert.True(s.Style.BackgroundColor.G > 100);
            Assert.Equal(255, s.Style.BackgroundColor.A);
        }

        // background-color doesn't inherit
        [Fact]
        public void BackgroundColor_NoInherit()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='background:red'><div id='t' style='width:50px;height:50px'></div></div></body>");
            Assert.Equal(0, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.BackgroundColor.A);
        }

        // background-clip: border-box (default)
        [Fact]
        public void BackgroundClip_BorderBox()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='background:red;width:50px;height:50px'></div></body>");
            Assert.Equal(CssBackgroundClip.BorderBox, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.BackgroundClip);
        }

        // background-clip: padding-box
        [Fact]
        public void BackgroundClip_PaddingBox()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='background:red;background-clip:padding-box;width:50px;height:50px'></div></body>");
            Assert.Equal(CssBackgroundClip.PaddingBox, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.BackgroundClip);
        }

        // background-clip: content-box
        [Fact]
        public void BackgroundClip_ContentBox()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='background:red;background-clip:content-box;width:50px;height:50px'></div></body>");
            Assert.Equal(CssBackgroundClip.ContentBox, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.BackgroundClip);
        }

        // background-origin: padding-box (default)
        [Fact]
        public void BackgroundOrigin_PaddingBox()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='background:red;width:50px;height:50px'></div></body>");
            Assert.Equal(CssBackgroundOrigin.PaddingBox, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.BackgroundOrigin);
        }

        // border-radius parsed
        [Fact]
        public void BorderRadius_10px()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:1px solid;border-radius:10px;width:50px;height:50px'></div></body>");
            Assert.True(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.BorderTopLeftRadius > 0);
        }

        // border styles
        [Fact] public void BorderStyle_Solid() { AssertBorderStyle("solid", CssBorderStyle.Solid); }
        [Fact] public void BorderStyle_Dashed() { AssertBorderStyle("dashed", CssBorderStyle.Dashed); }
        [Fact] public void BorderStyle_Dotted() { AssertBorderStyle("dotted", CssBorderStyle.Dotted); }
        [Fact] public void BorderStyle_Double() { AssertBorderStyle("double", CssBorderStyle.Double); }
        [Fact] public void BorderStyle_Groove() { AssertBorderStyle("groove", CssBorderStyle.Groove); }
        [Fact] public void BorderStyle_Ridge() { AssertBorderStyle("ridge", CssBorderStyle.Ridge); }
        [Fact] public void BorderStyle_Inset() { AssertBorderStyle("inset", CssBorderStyle.Inset); }
        [Fact] public void BorderStyle_Outset() { AssertBorderStyle("outset", CssBorderStyle.Outset); }

        // border-width keywords
        [Fact] public void BorderWidth_Thin() { AssertBorderWidth("thin", 1); }
        [Fact] public void BorderWidth_Medium() { AssertBorderWidth("medium", 3); }
        [Fact] public void BorderWidth_Thick() { AssertBorderWidth("thick", 5); }

        // border-style:none → width=0
        [Fact]
        public void BorderNone_ZeroWidth()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:5px none red;width:100px;height:50px'></div></body>");
            Assert.Equal(0, LayoutTestHelper.FindById(r, "t")!.BorderTopWidth);
        }

        // border-color: currentColor
        [Fact]
        public void BorderColor_CurrentColor()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='color:red'><div id='t' style='border:2px solid;width:50px;height:50px'></div></div></body>");
            Assert.Equal(255, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.BorderTopColor.R);
        }

        // individual border sides
        [Fact]
        public void IndividualBorderSides()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border-top:1px solid;border-right:2px solid;border-bottom:3px solid;border-left:4px solid;width:100px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.Equal(1, t.BorderTopWidth);
            Assert.Equal(2, t.BorderRightWidth);
            Assert.Equal(3, t.BorderBottomWidth);
            Assert.Equal(4, t.BorderLeftWidth);
        }

        // border shorthand overrides individual
        [Fact]
        public void BorderShorthand_Overrides()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border-top:5px solid;border:2px solid;width:100px;height:50px'></div></body>");
            Assert.Equal(2, LayoutTestHelper.FindById(r, "t")!.BorderTopWidth);
        }

        // border reduces content box width
        [Fact]
        public void Border_ReducesContentWidth()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:100px'><div id='t' style='border:5px solid;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 90) < 2);
        }

        // outline doesn't affect layout
        [Fact]
        public void Outline_NoLayoutEffect()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='outline:10px solid red;height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        // box-shadow doesn't affect layout
        [Fact]
        public void BoxShadow_NoLayoutEffect()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='box-shadow:10px 10px 20px black;height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        private void AssertBorderStyle(string value, CssBorderStyle expected)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='border:3px {value} gray;width:50px;height:50px'></div></body>");
            Assert.Equal(expected, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.BorderTopStyle);
        }

        private void AssertBorderWidth(string value, float expected)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='border:{value} solid;width:50px;height:50px'></div></body>");
            Assert.Equal(expected, LayoutTestHelper.FindById(r, "t")!.BorderTopWidth);
        }
    }
}
