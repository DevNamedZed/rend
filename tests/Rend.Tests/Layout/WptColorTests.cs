using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptColorTests
    {
        private readonly ITestOutputHelper _output;
        public WptColorTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Color_NamedColors()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='red' style='color: red; width:10px; height:10px;'></div>
                <div id='green' style='color: green; width:10px; height:10px;'></div>
                <div id='blue' style='color: blue; width:10px; height:10px;'></div></body>");
            var red = (LayoutTestHelper.FindById(root, "red")!.StyledNode as StyledElement)!;
            var green = (LayoutTestHelper.FindById(root, "green")!.StyledNode as StyledElement)!;
            var blue = (LayoutTestHelper.FindById(root, "blue")!.StyledNode as StyledElement)!;
            Assert.Equal(255, red.Style.Color.R);
            Assert.Equal(0, green.Style.Color.R);
            Assert.True(green.Style.Color.G == 128, $"green={green.Style.Color.G}");
            Assert.Equal(255, blue.Style.Color.B);
        }

        [Fact]
        public void Color_Hex()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='color: #ff8800; width:10px; height:10px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(255, styled.Style.Color.R);
            Assert.Equal(136, styled.Style.Color.G);
            Assert.Equal(0, styled.Style.Color.B);
        }

        [Fact]
        public void Color_Rgb()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='color: rgb(100, 150, 200); width:10px; height:10px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(100, styled.Style.Color.R);
            Assert.Equal(150, styled.Style.Color.G);
            Assert.Equal(200, styled.Style.Color.B);
        }

        [Fact]
        public void Color_Rgba()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='color: rgba(100, 150, 200, 0.5); width:10px; height:10px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(100, styled.Style.Color.R);
            Assert.Equal(150, styled.Style.Color.G);
            Assert.Equal(200, styled.Style.Color.B);
            Assert.True(styled.Style.Color.A >= 126 && styled.Style.Color.A <= 128,
                $"alpha 0.5 = ~128 (got {styled.Style.Color.A})");
        }

        [Fact]
        public void Color_Hsl()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='color: hsl(0, 100%, 50%); width:10px; height:10px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            _output.WriteLine($"color=({styled.Style.Color.R},{styled.Style.Color.G},{styled.Style.Color.B})");
            // hsl(0, 100%, 50%) = red
            Assert.Equal(255, styled.Style.Color.R);
            Assert.Equal(0, styled.Style.Color.G);
            Assert.Equal(0, styled.Style.Color.B);
        }

        [Fact]
        public void CurrentColor_InheritsFromColor()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='color: red;'>
                    <div id='test' style='border: 2px solid currentColor; width: 50px; height: 50px;'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            _output.WriteLine($"border-color=({styled.Style.BorderTopColor.R},{styled.Style.BorderTopColor.G},{styled.Style.BorderTopColor.B})");
            // currentColor → inherited color (red)
            Assert.Equal(255, styled.Style.BorderTopColor.R);
            Assert.Equal(0, styled.Style.BorderTopColor.G);
        }

        [Fact]
        public void Transparent_Keyword()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='background-color: transparent; width: 50px; height: 50px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(0, styled.Style.BackgroundColor.A);
        }
    }
}
