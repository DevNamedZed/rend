using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptColorConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptColorConformanceTests(ITestOutputHelper output) { _output = output; }

        // named colors
        [Fact] public void Named_Red() { AssertColor("red", 255, 0, 0); }
        [Fact] public void Named_Green() { AssertColor("green", 0, 128, 0); }
        [Fact] public void Named_Blue() { AssertColor("blue", 0, 0, 255); }
        [Fact] public void Named_White() { AssertColor("white", 255, 255, 255); }
        [Fact] public void Named_Black() { AssertColor("black", 0, 0, 0); }
        [Fact] public void Named_Yellow() { AssertColor("yellow", 255, 255, 0); }
        [Fact] public void Named_Cyan() { AssertColor("cyan", 0, 255, 255); }
        [Fact] public void Named_Magenta() { AssertColor("magenta", 255, 0, 255); }
        [Fact] public void Named_Orange() { AssertColor("orange", 255, 165, 0); }
        [Fact] public void Named_Purple() { AssertColor("purple", 128, 0, 128); }
        [Fact] public void Named_Lime() { AssertColor("lime", 0, 255, 0); }

        // hex colors
        [Fact] public void Hex3_F00() { AssertColor("#f00", 255, 0, 0); }
        [Fact] public void Hex6_FF8800() { AssertColor("#ff8800", 255, 136, 0); }
        [Fact] public void Hex8_FF000080() { AssertColorAlpha("#ff000080", 255, 0, 0, 128); }
        [Fact] public void Hex4_F00F() { AssertColorAlpha("#f00f", 255, 0, 0, 255); }

        // rgb()
        [Fact] public void Rgb_Values() { AssertColor("rgb(100,150,200)", 100, 150, 200); }
        [Fact] public void Rgb_Zero() { AssertColor("rgb(0,0,0)", 0, 0, 0); }
        [Fact] public void Rgb_Max() { AssertColor("rgb(255,255,255)", 255, 255, 255); }

        // rgba()
        [Fact]
        public void Rgba_HalfAlpha()
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='color:rgba(100,150,200,0.5);width:10px;height:10px'></div></body>");
            var s = (LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(100, s.Style.Color.R);
            Assert.Equal(150, s.Style.Color.G);
            Assert.Equal(200, s.Style.Color.B);
            Assert.True(s.Style.Color.A >= 126 && s.Style.Color.A <= 129);
        }

        // hsl()
        [Fact] public void Hsl_Red() { AssertColor("hsl(0,100%,50%)", 255, 0, 0); }
        [Fact] public void Hsl_Green() { AssertColor("hsl(120,100%,50%)", 0, 255, 0); }
        [Fact] public void Hsl_Blue() { AssertColor("hsl(240,100%,50%)", 0, 0, 255); }

        // transparent
        [Fact]
        public void Transparent()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='background-color:transparent;width:10px;height:10px'></div></body>");
            Assert.Equal(0, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.BackgroundColor.A);
        }

        // currentColor on border
        [Fact]
        public void CurrentColor_Border()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='color:red'><div id='t' style='border:2px solid currentColor;width:50px;height:50px'></div></div></body>");
            Assert.Equal(255, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.BorderTopColor.R);
        }

        // opacity values
        [Fact]
        public void Opacity_Zero()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='opacity:0;width:10px;height:10px'></div></body>");
            Assert.True(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.Opacity < 0.01f);
        }

        [Fact]
        public void Opacity_One()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='opacity:1;width:10px;height:10px'></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.Opacity - 1) < 0.01f);
        }

        [Fact]
        public void Opacity_Half()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='opacity:0.5;width:10px;height:10px'></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.Opacity - 0.5f) < 0.01f);
        }

        // opacity doesn't affect layout
        [Fact]
        public void Opacity_NoLayoutEffect()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='opacity:0;height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        // color inherits
        [Fact]
        public void Color_Inherits()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='color:red'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.Equal(255, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.Color.R);
        }

        // background-color doesn't inherit
        [Fact]
        public void BackgroundColor_NoInherit()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='background:red'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.Equal(0, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.BackgroundColor.A);
        }

        private void AssertColor(string css, byte r, byte g, byte b)
        {
            var root = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='color:{css};width:10px;height:10px'></div></body>");
            var s = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(r, s.Style.Color.R);
            Assert.Equal(g, s.Style.Color.G);
            Assert.Equal(b, s.Style.Color.B);
        }

        private void AssertColorAlpha(string css, byte r, byte g, byte b, byte a)
        {
            var root = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='color:{css};width:10px;height:10px'></div></body>");
            var s = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(r, s.Style.Color.R);
            Assert.Equal(g, s.Style.Color.G);
            Assert.Equal(b, s.Style.Color.B);
            Assert.Equal(a, s.Style.Color.A);
        }
    }
}
