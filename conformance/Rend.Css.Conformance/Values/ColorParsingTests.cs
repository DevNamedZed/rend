using System.Linq;
using Rend.Css;
using Xunit;

namespace Rend.Css.Conformance.Values
{
    public class ColorParsingTests
    {
        private static CssValue GetColorValue(string css)
        {
            var sheet = CssParser.Parse(css);
            var rule = sheet.Rules.OfType<StyleRule>().First();
            return rule.Declarations.First(d => d.Property == "color").Value;
        }

        #region Hex Colors

        [Fact]
        public void Hex3_ShortFormat_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: #f00; }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(0, color.Color.G);
            Assert.Equal(0, color.Color.B);
        }

        [Fact]
        public void Hex3_White_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: #fff; }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(255, color.Color.G);
            Assert.Equal(255, color.Color.B);
        }

        [Fact]
        public void Hex6_FullFormat_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: #ff6600; }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(102, color.Color.G);
            Assert.Equal(0, color.Color.B);
        }

        [Fact]
        public void Hex8_WithAlpha_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: #ff000080; }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(0, color.Color.G);
            Assert.Equal(0, color.Color.B);
            Assert.Equal(128, color.Color.A);
        }

        [Fact]
        public void Hex4_WithAlpha_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: #f008; }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(0, color.Color.G);
            Assert.Equal(0, color.Color.B);
            Assert.Equal(136, color.Color.A);
        }

        #endregion

        #region Named Colors

        [Fact]
        public void NamedColor_Red_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: red; }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(0, color.Color.G);
            Assert.Equal(0, color.Color.B);
        }

        [Fact]
        public void NamedColor_Blue_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: blue; }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(0, color.Color.R);
            Assert.Equal(0, color.Color.G);
            Assert.Equal(255, color.Color.B);
        }

        [Fact]
        public void NamedColor_Green_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: green; }");
            var color = Assert.IsType<CssColorValue>(val);
            // CSS named "green" is #008000
            Assert.Equal(0, color.Color.R);
            Assert.Equal(128, color.Color.G);
            Assert.Equal(0, color.Color.B);
        }

        [Fact]
        public void NamedColor_Transparent_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: transparent; }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(0, color.Color.A);
        }

        [Fact]
        public void NamedColor_CurrentColor_IsKeyword()
        {
            // currentColor is treated as a keyword, not resolved to a CssColorValue
            var val = GetColorValue("div { color: currentColor; }");
            var kw = Assert.IsType<CssKeywordValue>(val);
            Assert.Equal("currentcolor", kw.Keyword);
        }

        #endregion

        #region rgb() / rgba()

        [Fact]
        public void Rgb_IntegerArgs_ParsesAsColor()
        {
            var val = GetColorValue("div { color: rgb(255, 128, 0); }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(128, color.Color.G);
            Assert.Equal(0, color.Color.B);
            Assert.Equal(255, color.Color.A);
        }

        [Fact]
        public void Rgba_WithAlpha_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: rgba(255, 0, 0, 0.5); }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(0, color.Color.G);
            Assert.Equal(0, color.Color.B);
            Assert.Equal(128, color.Color.A);
        }

        [Fact]
        public void Rgb_PercentageArgs_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: rgb(100%, 50%, 0%); }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(128, color.Color.G);
            Assert.Equal(0, color.Color.B);
        }

        #endregion

        #region hsl() / hsla()

        [Fact]
        public void Hsl_Red_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: hsl(0, 100%, 50%); }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(255, color.Color.R);
            Assert.InRange(color.Color.G, 0, 1);
            Assert.InRange(color.Color.B, 0, 1);
        }

        [Fact]
        public void Hsl_Green_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: hsl(120, 100%, 50%); }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.InRange(color.Color.R, 0, 1);
            Assert.Equal(255, color.Color.G);
            Assert.InRange(color.Color.B, 0, 1);
        }

        [Fact]
        public void Hsl_Blue_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: hsl(240, 100%, 50%); }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.InRange(color.Color.R, 0, 1);
            Assert.InRange(color.Color.G, 0, 1);
            Assert.Equal(255, color.Color.B);
        }

        [Fact]
        public void Hsla_WithAlpha_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: hsla(0, 100%, 50%, 0.5); }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(128, color.Color.A);
        }

        #endregion

        #region hwb()

        [Fact]
        public void Hwb_Red_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: hwb(0 0% 0%); }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(0, color.Color.G);
            Assert.Equal(0, color.Color.B);
        }

        [Fact]
        public void Hwb_White_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: hwb(0 100% 0%); }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(255, color.Color.G);
            Assert.Equal(255, color.Color.B);
        }

        #endregion

        #region lab()

        [Fact]
        public void Lab_White_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: lab(100 0 0); }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.InRange(color.Color.R, 250, 255);
            Assert.InRange(color.Color.G, 250, 255);
            Assert.InRange(color.Color.B, 250, 255);
        }

        [Fact]
        public void Lab_Black_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: lab(0 0 0); }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.InRange(color.Color.R, 0, 5);
            Assert.InRange(color.Color.G, 0, 5);
            Assert.InRange(color.Color.B, 0, 5);
        }

        #endregion

        #region lch()

        [Fact]
        public void Lch_White_ParsesCorrectly()
        {
            var val = GetColorValue("div { color: lch(100 0 0); }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.InRange(color.Color.R, 250, 255);
            Assert.InRange(color.Color.G, 250, 255);
            Assert.InRange(color.Color.B, 250, 255);
        }

        #endregion

        #region oklch()

        [Fact]
        public void Oklch_ParsesAsColor()
        {
            var val = GetColorValue("div { color: oklch(0.7 0.15 180); }");
            var color = Assert.IsType<CssColorValue>(val);
            // Should produce some color, exact values depend on conversion
            Assert.NotEqual(0, color.Color.R + color.Color.G + color.Color.B);
        }

        #endregion

        #region oklab()

        [Fact]
        public void Oklab_ParsesAsColor()
        {
            var val = GetColorValue("div { color: oklab(0.5 0.1 -0.1); }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.NotEqual(0, color.Color.R + color.Color.G + color.Color.B);
        }

        #endregion

        #region color-mix()

        [Fact]
        public void ColorMix_TwoNamedColors_ParsesAsColor()
        {
            var val = GetColorValue("div { color: color-mix(in srgb, red, blue); }");
            var color = Assert.IsType<CssColorValue>(val);
            // Mix of red (255,0,0) and blue (0,0,255) at 50% each
            Assert.InRange(color.Color.R, 120, 135);
            Assert.Equal(0, color.Color.G);
            Assert.InRange(color.Color.B, 120, 135);
        }

        #endregion

        #region CssColorValue Properties

        [Fact]
        public void CssColorValue_Kind_IsColor()
        {
            var val = GetColorValue("div { color: #ff0000; }");
            Assert.Equal(CssValueKind.Color, val.Kind);
        }

        [Fact]
        public void CssColorValue_ToString_FormatsAsRgb()
        {
            var val = GetColorValue("div { color: red; }");
            var color = Assert.IsType<CssColorValue>(val);
            Assert.Contains("255", color.ToString());
        }

        #endregion
    }
}
