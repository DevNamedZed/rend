using System.Linq;
using Rend.Css;
using Xunit;

namespace Rend.Css.Conformance.Parsing
{
    public class PropertyParsingTests
    {
        private static CssDeclaration? GetDeclaration(string css, string property)
        {
            var sheet = CssParser.Parse(css);
            var rule = sheet.Rules.OfType<StyleRule>().First();
            return rule.Declarations.FirstOrDefault(d => d.Property == property);
        }

        #region Width & Height

        [Theory]
        [InlineData("div { width: 100px; }", "width", "100px")]
        [InlineData("div { width: 50%; }", "width", "50%")]
        [InlineData("div { width: auto; }", "width", "auto")]
        [InlineData("div { height: 200px; }", "height", "200px")]
        [InlineData("div { height: 100%; }", "height", "100%")]
        public void Width_And_Height_ParseCorrectly(string css, string property, string expected)
        {
            var decl = GetDeclaration(css, property);
            Assert.NotNull(decl);
            Assert.Equal(expected, decl!.Value.ToString());
        }

        #endregion

        #region Margin & Padding Longhands

        [Theory]
        [InlineData("div { margin-top: 10px; }", "margin-top", "10px")]
        [InlineData("div { margin-right: 20px; }", "margin-right", "20px")]
        [InlineData("div { margin-bottom: 15px; }", "margin-bottom", "15px")]
        [InlineData("div { margin-left: 5px; }", "margin-left", "5px")]
        [InlineData("div { margin-top: auto; }", "margin-top", "auto")]
        [InlineData("div { padding-top: 10px; }", "padding-top", "10px")]
        [InlineData("div { padding-right: 20px; }", "padding-right", "20px")]
        [InlineData("div { padding-bottom: 15px; }", "padding-bottom", "15px")]
        [InlineData("div { padding-left: 5px; }", "padding-left", "5px")]
        public void Margin_And_Padding_Longhands_ParseCorrectly(string css, string property, string expected)
        {
            var decl = GetDeclaration(css, property);
            Assert.NotNull(decl);
            Assert.Equal(expected, decl!.Value.ToString());
        }

        #endregion

        #region Display

        [Theory]
        [InlineData("div { display: block; }", "block")]
        [InlineData("div { display: inline; }", "inline")]
        [InlineData("div { display: flex; }", "flex")]
        [InlineData("div { display: grid; }", "grid")]
        [InlineData("div { display: none; }", "none")]
        [InlineData("div { display: inline-block; }", "inline-block")]
        [InlineData("div { display: table; }", "table")]
        public void Display_ParsesKeywords(string css, string expected)
        {
            var decl = GetDeclaration(css, "display");
            Assert.NotNull(decl);
            Assert.Equal(expected, decl!.Value.ToString());
        }

        #endregion

        #region Position

        [Theory]
        [InlineData("div { position: static; }", "static")]
        [InlineData("div { position: relative; }", "relative")]
        [InlineData("div { position: absolute; }", "absolute")]
        [InlineData("div { position: fixed; }", "fixed")]
        [InlineData("div { position: sticky; }", "sticky")]
        public void Position_ParsesKeywords(string css, string expected)
        {
            var decl = GetDeclaration(css, "position");
            Assert.NotNull(decl);
            Assert.Equal(expected, decl!.Value.ToString());
        }

        #endregion

        #region Float & Clear

        [Theory]
        [InlineData("div { float: left; }", "float", "left")]
        [InlineData("div { float: right; }", "float", "right")]
        [InlineData("div { float: none; }", "float", "none")]
        [InlineData("div { clear: both; }", "clear", "both")]
        [InlineData("div { clear: left; }", "clear", "left")]
        [InlineData("div { clear: none; }", "clear", "none")]
        public void Float_And_Clear_ParseCorrectly(string css, string property, string expected)
        {
            var decl = GetDeclaration(css, property);
            Assert.NotNull(decl);
            Assert.Equal(expected, decl!.Value.ToString());
        }

        #endregion

        #region Overflow, Z-Index, Opacity, Visibility

        [Theory]
        [InlineData("div { z-index: 10; }", "z-index", "10")]
        [InlineData("div { opacity: 0.5; }", "opacity", "0.5")]
        [InlineData("div { visibility: hidden; }", "visibility", "hidden")]
        [InlineData("div { visibility: visible; }", "visibility", "visible")]
        public void ZIndex_Opacity_Visibility_ParseCorrectly(string css, string property, string expected)
        {
            var decl = GetDeclaration(css, property);
            Assert.NotNull(decl);
            Assert.Equal(expected, decl!.Value.ToString());
        }

        #endregion

        #region Color & Background-Color

        [Fact]
        public void Color_NamedColor_ParsesAsColor()
        {
            var decl = GetDeclaration("div { color: red; }", "color");
            Assert.NotNull(decl);
            Assert.IsType<CssColorValue>(decl!.Value);
        }

        [Fact]
        public void BackgroundColor_NamedColor_ParsesAsColor()
        {
            var decl = GetDeclaration("div { background-color: blue; }", "background-color");
            Assert.NotNull(decl);
            Assert.IsType<CssColorValue>(decl!.Value);
        }

        #endregion

        #region Font Properties

        [Theory]
        [InlineData("div { font-size: 16px; }", "font-size", "16px")]
        [InlineData("div { font-size: 1.5em; }", "font-size", "1.5em")]
        [InlineData("div { font-size: 100%; }", "font-size", "100%")]
        public void FontSize_ParsesCorrectly(string css, string property, string expected)
        {
            var decl = GetDeclaration(css, property);
            Assert.NotNull(decl);
            Assert.Equal(expected, decl!.Value.ToString());
        }

        [Theory]
        [InlineData("div { font-weight: bold; }", "bold")]
        [InlineData("div { font-weight: normal; }", "normal")]
        public void FontWeight_Keywords_ParseCorrectly(string css, string expected)
        {
            var decl = GetDeclaration(css, "font-weight");
            Assert.NotNull(decl);
            Assert.Equal(expected, decl!.Value.ToString());
        }

        [Theory]
        [InlineData("div { font-style: italic; }", "italic")]
        [InlineData("div { font-style: normal; }", "normal")]
        [InlineData("div { font-style: oblique; }", "oblique")]
        public void FontStyle_ParsesCorrectly(string css, string expected)
        {
            var decl = GetDeclaration(css, "font-style");
            Assert.NotNull(decl);
            Assert.Equal(expected, decl!.Value.ToString());
        }

        [Fact]
        public void FontFamily_SingleValue_ParsesCorrectly()
        {
            var decl = GetDeclaration("div { font-family: Arial; }", "font-family");
            Assert.NotNull(decl);
            Assert.Contains("arial", decl!.Value.ToString().ToLowerInvariant());
        }

        #endregion

        #region Text Properties

        [Theory]
        [InlineData("div { text-align: center; }", "text-align", "center")]
        [InlineData("div { text-align: left; }", "text-align", "left")]
        [InlineData("div { text-align: right; }", "text-align", "right")]
        [InlineData("div { text-align: justify; }", "text-align", "justify")]
        public void TextAlign_ParsesCorrectly(string css, string property, string expected)
        {
            var decl = GetDeclaration(css, property);
            Assert.NotNull(decl);
            Assert.Equal(expected, decl!.Value.ToString());
        }

        [Fact]
        public void LineHeight_Dimension_ParsesCorrectly()
        {
            var decl = GetDeclaration("div { line-height: 1.5em; }", "line-height");
            Assert.NotNull(decl);
            Assert.Equal("1.5em", decl!.Value.ToString());
        }

        [Fact]
        public void LineHeight_Unitless_ParsesCorrectly()
        {
            var decl = GetDeclaration("div { line-height: 1.5; }", "line-height");
            Assert.NotNull(decl);
            Assert.Equal("1.5", decl!.Value.ToString());
        }

        #endregion

        #region Border Properties (Longhands)

        [Theory]
        [InlineData("div { border-width: 1px; }", "border-width")]
        [InlineData("div { border-style: solid; }", "border-style")]
        [InlineData("div { border-color: red; }", "border-color")]
        public void Border_Longhands_AreExpandedOrParsed(string css, string property)
        {
            // border-width, border-style, border-color are shorthands that expand to per-side
            var sheet = CssParser.Parse(css);
            var rule = sheet.Rules.OfType<StyleRule>().First();
            Assert.True(rule.Declarations.Count > 0);
        }

        [Fact]
        public void BorderRadius_SingleValue_Parses()
        {
            // border-radius is a shorthand
            var sheet = CssParser.Parse("div { border-radius: 5px; }");
            var rule = sheet.Rules.OfType<StyleRule>().First();
            var tl = rule.Declarations.FirstOrDefault(d => d.Property == "border-top-left-radius");
            Assert.NotNull(tl);
            Assert.Equal("5px", tl!.Value.ToString());
        }

        #endregion

        #region Box Sizing

        [Theory]
        [InlineData("div { box-sizing: border-box; }", "border-box")]
        [InlineData("div { box-sizing: content-box; }", "content-box")]
        public void BoxSizing_ParsesCorrectly(string css, string expected)
        {
            var decl = GetDeclaration(css, "box-sizing");
            Assert.NotNull(decl);
            Assert.Equal(expected, decl!.Value.ToString());
        }

        #endregion

        #region Flexbox

        [Theory]
        [InlineData("div { flex-direction: row; }", "flex-direction", "row")]
        [InlineData("div { flex-direction: column; }", "flex-direction", "column")]
        [InlineData("div { flex-direction: row-reverse; }", "flex-direction", "row-reverse")]
        [InlineData("div { flex-wrap: wrap; }", "flex-wrap", "wrap")]
        [InlineData("div { flex-wrap: nowrap; }", "flex-wrap", "nowrap")]
        [InlineData("div { justify-content: center; }", "justify-content", "center")]
        [InlineData("div { justify-content: space-between; }", "justify-content", "space-between")]
        [InlineData("div { align-items: center; }", "align-items", "center")]
        [InlineData("div { align-items: flex-start; }", "align-items", "flex-start")]
        public void Flexbox_Properties_ParseCorrectly(string css, string property, string expected)
        {
            var decl = GetDeclaration(css, property);
            Assert.NotNull(decl);
            Assert.Equal(expected, decl!.Value.ToString());
        }

        #endregion

        #region Gap

        [Fact]
        public void Gap_SingleValue_ExpandsToRowAndColumnGap()
        {
            // gap is a shorthand that expands to row-gap and column-gap
            var sheet = CssParser.Parse("div { gap: 10px; }");
            var rule = sheet.Rules.OfType<StyleRule>().First();
            var rowGap = rule.Declarations.FirstOrDefault(d => d.Property == "row-gap");
            var colGap = rule.Declarations.FirstOrDefault(d => d.Property == "column-gap");
            Assert.NotNull(rowGap);
            Assert.NotNull(colGap);
            Assert.Equal("10px", rowGap!.Value.ToString());
            Assert.Equal("10px", colGap!.Value.ToString());
        }

        #endregion

        #region Grid

        [Fact]
        public void GridTemplateColumns_ParsesCorrectly()
        {
            var decl = GetDeclaration("div { grid-template-columns: 1fr 2fr; }", "grid-template-columns");
            Assert.NotNull(decl);
            Assert.Contains("1fr", decl!.Value.ToString());
            Assert.Contains("2fr", decl!.Value.ToString());
        }

        [Fact]
        public void GridTemplateRows_ParsesCorrectly()
        {
            var decl = GetDeclaration("div { grid-template-rows: 100px auto; }", "grid-template-rows");
            Assert.NotNull(decl);
            Assert.Contains("100px", decl!.Value.ToString());
        }

        #endregion

        #region Transform & Transition

        [Fact]
        public void Transform_FunctionValue_ParsesCorrectly()
        {
            var decl = GetDeclaration("div { transform: rotate(45deg); }", "transform");
            Assert.NotNull(decl);
            var fn = Assert.IsType<CssFunctionValue>(decl!.Value);
            Assert.Equal("rotate", fn.Name);
        }

        [Fact]
        public void Transition_ParsesCorrectly()
        {
            var decl = GetDeclaration("div { transition: all 0.3s ease; }", "transition");
            Assert.NotNull(decl);
            Assert.Contains("all", decl!.Value.ToString());
        }

        #endregion

        #region Important

        [Fact]
        public void Important_Flag_IsParsed()
        {
            var decl = GetDeclaration("div { color: red !important; }", "color");
            Assert.NotNull(decl);
            Assert.True(decl!.Important);
        }

        [Fact]
        public void Without_Important_Flag_IsFalse()
        {
            var decl = GetDeclaration("div { color: red; }", "color");
            Assert.NotNull(decl);
            Assert.False(decl!.Important);
        }

        #endregion

        #region Value Types

        [Fact]
        public void Dimension_Value_HasCorrectType()
        {
            var decl = GetDeclaration("div { width: 100px; }", "width");
            Assert.NotNull(decl);
            var dim = Assert.IsType<CssDimensionValue>(decl!.Value);
            Assert.Equal(100f, dim.Value);
            Assert.Equal("px", dim.Unit);
        }

        [Fact]
        public void Percentage_Value_HasCorrectType()
        {
            var decl = GetDeclaration("div { width: 50%; }", "width");
            Assert.NotNull(decl);
            var pct = Assert.IsType<CssPercentageValue>(decl!.Value);
            Assert.Equal(50f, pct.Value);
        }

        [Fact]
        public void Keyword_Value_HasCorrectType()
        {
            var decl = GetDeclaration("div { display: block; }", "display");
            Assert.NotNull(decl);
            var kw = Assert.IsType<CssKeywordValue>(decl!.Value);
            Assert.Equal("block", kw.Keyword);
        }

        [Fact]
        public void Number_Value_HasCorrectType()
        {
            var decl = GetDeclaration("div { z-index: 42; }", "z-index");
            Assert.NotNull(decl);
            var num = Assert.IsType<CssNumberValue>(decl!.Value);
            Assert.Equal(42f, num.Value);
        }

        #endregion

        #region Overflow (expanded)

        [Fact]
        public void Overflow_Single_ExpandsToXAndY()
        {
            var sheet = CssParser.Parse("div { overflow: hidden; }");
            var rule = sheet.Rules.OfType<StyleRule>().First();
            var ox = rule.Declarations.FirstOrDefault(d => d.Property == "overflow-x");
            var oy = rule.Declarations.FirstOrDefault(d => d.Property == "overflow-y");
            Assert.NotNull(ox);
            Assert.NotNull(oy);
            Assert.Equal("hidden", ox!.Value.ToString());
            Assert.Equal("hidden", oy!.Value.ToString());
        }

        #endregion
    }
}
