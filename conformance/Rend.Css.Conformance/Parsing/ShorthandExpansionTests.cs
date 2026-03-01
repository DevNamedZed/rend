using System.Linq;
using Rend.Css;
using Xunit;

namespace Rend.Css.Conformance.Parsing
{
    public class ShorthandExpansionTests
    {
        private static StyleRule ParseFirstRule(string css)
        {
            var sheet = CssParser.Parse(css);
            return sheet.Rules.OfType<StyleRule>().First();
        }

        private static CssDeclaration? FindDecl(StyleRule rule, string property)
        {
            return rule.Declarations.FirstOrDefault(d => d.Property == property);
        }

        #region Margin Shorthand

        [Fact]
        public void Margin_OneValue_SetsAllFourSides()
        {
            var rule = ParseFirstRule("div { margin: 10px; }");
            Assert.Equal("10px", FindDecl(rule, "margin-top")!.Value.ToString());
            Assert.Equal("10px", FindDecl(rule, "margin-right")!.Value.ToString());
            Assert.Equal("10px", FindDecl(rule, "margin-bottom")!.Value.ToString());
            Assert.Equal("10px", FindDecl(rule, "margin-left")!.Value.ToString());
        }

        [Fact]
        public void Margin_TwoValues_SetsVerticalAndHorizontal()
        {
            var rule = ParseFirstRule("div { margin: 10px 20px; }");
            Assert.Equal("10px", FindDecl(rule, "margin-top")!.Value.ToString());
            Assert.Equal("20px", FindDecl(rule, "margin-right")!.Value.ToString());
            Assert.Equal("10px", FindDecl(rule, "margin-bottom")!.Value.ToString());
            Assert.Equal("20px", FindDecl(rule, "margin-left")!.Value.ToString());
        }

        [Fact]
        public void Margin_ThreeValues_SetsTopHorizontalBottom()
        {
            var rule = ParseFirstRule("div { margin: 10px 20px 30px; }");
            Assert.Equal("10px", FindDecl(rule, "margin-top")!.Value.ToString());
            Assert.Equal("20px", FindDecl(rule, "margin-right")!.Value.ToString());
            Assert.Equal("30px", FindDecl(rule, "margin-bottom")!.Value.ToString());
            Assert.Equal("20px", FindDecl(rule, "margin-left")!.Value.ToString());
        }

        [Fact]
        public void Margin_FourValues_SetsEachSide()
        {
            var rule = ParseFirstRule("div { margin: 10px 20px 30px 40px; }");
            Assert.Equal("10px", FindDecl(rule, "margin-top")!.Value.ToString());
            Assert.Equal("20px", FindDecl(rule, "margin-right")!.Value.ToString());
            Assert.Equal("30px", FindDecl(rule, "margin-bottom")!.Value.ToString());
            Assert.Equal("40px", FindDecl(rule, "margin-left")!.Value.ToString());
        }

        #endregion

        #region Padding Shorthand

        [Fact]
        public void Padding_OneValue_SetsAllFourSides()
        {
            var rule = ParseFirstRule("div { padding: 5px; }");
            Assert.Equal("5px", FindDecl(rule, "padding-top")!.Value.ToString());
            Assert.Equal("5px", FindDecl(rule, "padding-right")!.Value.ToString());
            Assert.Equal("5px", FindDecl(rule, "padding-bottom")!.Value.ToString());
            Assert.Equal("5px", FindDecl(rule, "padding-left")!.Value.ToString());
        }

        [Fact]
        public void Padding_TwoValues_SetsVerticalAndHorizontal()
        {
            var rule = ParseFirstRule("div { padding: 5px 15px; }");
            Assert.Equal("5px", FindDecl(rule, "padding-top")!.Value.ToString());
            Assert.Equal("15px", FindDecl(rule, "padding-right")!.Value.ToString());
            Assert.Equal("5px", FindDecl(rule, "padding-bottom")!.Value.ToString());
            Assert.Equal("15px", FindDecl(rule, "padding-left")!.Value.ToString());
        }

        [Fact]
        public void Padding_ThreeValues_SetsTopHorizontalBottom()
        {
            var rule = ParseFirstRule("div { padding: 5px 15px 25px; }");
            Assert.Equal("5px", FindDecl(rule, "padding-top")!.Value.ToString());
            Assert.Equal("15px", FindDecl(rule, "padding-right")!.Value.ToString());
            Assert.Equal("25px", FindDecl(rule, "padding-bottom")!.Value.ToString());
            Assert.Equal("15px", FindDecl(rule, "padding-left")!.Value.ToString());
        }

        [Fact]
        public void Padding_FourValues_SetsEachSide()
        {
            var rule = ParseFirstRule("div { padding: 5px 15px 25px 35px; }");
            Assert.Equal("5px", FindDecl(rule, "padding-top")!.Value.ToString());
            Assert.Equal("15px", FindDecl(rule, "padding-right")!.Value.ToString());
            Assert.Equal("25px", FindDecl(rule, "padding-bottom")!.Value.ToString());
            Assert.Equal("35px", FindDecl(rule, "padding-left")!.Value.ToString());
        }

        #endregion

        #region Border Shorthand

        [Fact]
        public void Border_WidthStyleColor_ExpandsToAllSides()
        {
            var rule = ParseFirstRule("div { border: 1px solid red; }");

            Assert.Equal("1px", FindDecl(rule, "border-top-width")!.Value.ToString());
            Assert.Equal("solid", FindDecl(rule, "border-top-style")!.Value.ToString());
            Assert.IsType<CssColorValue>(FindDecl(rule, "border-top-color")!.Value);

            Assert.Equal("1px", FindDecl(rule, "border-right-width")!.Value.ToString());
            Assert.Equal("solid", FindDecl(rule, "border-right-style")!.Value.ToString());

            Assert.Equal("1px", FindDecl(rule, "border-bottom-width")!.Value.ToString());
            Assert.Equal("solid", FindDecl(rule, "border-bottom-style")!.Value.ToString());

            Assert.Equal("1px", FindDecl(rule, "border-left-width")!.Value.ToString());
            Assert.Equal("solid", FindDecl(rule, "border-left-style")!.Value.ToString());
        }

        [Fact]
        public void BorderTop_WidthStyleColor_ExpandsCorrectly()
        {
            var rule = ParseFirstRule("div { border-top: 2px dashed blue; }");
            Assert.Equal("2px", FindDecl(rule, "border-top-width")!.Value.ToString());
            Assert.Equal("dashed", FindDecl(rule, "border-top-style")!.Value.ToString());
            Assert.IsType<CssColorValue>(FindDecl(rule, "border-top-color")!.Value);
        }

        [Fact]
        public void BorderRight_WidthStyleColor_ExpandsCorrectly()
        {
            var rule = ParseFirstRule("div { border-right: 3px dotted green; }");
            Assert.Equal("3px", FindDecl(rule, "border-right-width")!.Value.ToString());
            Assert.Equal("dotted", FindDecl(rule, "border-right-style")!.Value.ToString());
        }

        [Fact]
        public void BorderBottom_ExpandsCorrectly()
        {
            var rule = ParseFirstRule("div { border-bottom: 1px solid; }");
            Assert.Equal("1px", FindDecl(rule, "border-bottom-width")!.Value.ToString());
            Assert.Equal("solid", FindDecl(rule, "border-bottom-style")!.Value.ToString());
        }

        [Fact]
        public void BorderLeft_ExpandsCorrectly()
        {
            var rule = ParseFirstRule("div { border-left: 4px double; }");
            Assert.Equal("4px", FindDecl(rule, "border-left-width")!.Value.ToString());
            Assert.Equal("double", FindDecl(rule, "border-left-style")!.Value.ToString());
        }

        #endregion

        #region Background Shorthand

        [Fact]
        public void Background_ColorOnly_ExpandsToBackgroundColor()
        {
            var rule = ParseFirstRule("div { background: red; }");
            var bgColor = FindDecl(rule, "background-color");
            Assert.NotNull(bgColor);
            Assert.IsType<CssColorValue>(bgColor!.Value);
        }

        [Fact]
        public void Background_ColorAndRepeat_ExpandsCorrectly()
        {
            var rule = ParseFirstRule("div { background: blue no-repeat; }");
            var bgColor = FindDecl(rule, "background-color");
            var bgRepeat = FindDecl(rule, "background-repeat");
            Assert.NotNull(bgColor);
            Assert.NotNull(bgRepeat);
            Assert.Equal("no-repeat", bgRepeat!.Value.ToString());
        }

        [Fact]
        public void Background_Url_ExpandsToBackgroundImage()
        {
            var rule = ParseFirstRule("div { background: url(img.png) no-repeat; }");
            var bgImage = FindDecl(rule, "background-image");
            Assert.NotNull(bgImage);
            Assert.Contains("img.png", bgImage!.Value.ToString());
        }

        #endregion

        #region Font Shorthand

        [Fact]
        public void Font_SizeAndFamily_ExpandsCorrectly()
        {
            var rule = ParseFirstRule("div { font: 16px Arial; }");
            var fontSize = FindDecl(rule, "font-size");
            var fontFamily = FindDecl(rule, "font-family");
            Assert.NotNull(fontSize);
            Assert.NotNull(fontFamily);
            Assert.Equal("16px", fontSize!.Value.ToString());
        }

        [Fact]
        public void Font_StyleWeightSizeFamily_ExpandsCorrectly()
        {
            var rule = ParseFirstRule("div { font: italic bold 14px sans-serif; }");
            var fontStyle = FindDecl(rule, "font-style");
            var fontWeight = FindDecl(rule, "font-weight");
            var fontSize = FindDecl(rule, "font-size");
            Assert.NotNull(fontStyle);
            Assert.NotNull(fontWeight);
            Assert.NotNull(fontSize);
            Assert.Equal("italic", fontStyle!.Value.ToString());
            Assert.Equal("bold", fontWeight!.Value.ToString());
            Assert.Equal("14px", fontSize!.Value.ToString());
        }

        [Fact]
        public void Font_SizeLineHeightFamily_ExpandsCorrectly()
        {
            var rule = ParseFirstRule("div { font: 16px/1.5 Arial; }");
            var fontSize = FindDecl(rule, "font-size");
            var lineHeight = FindDecl(rule, "line-height");
            Assert.NotNull(fontSize);
            Assert.NotNull(lineHeight);
            Assert.Equal("16px", fontSize!.Value.ToString());
            Assert.Equal("1.5", lineHeight!.Value.ToString());
        }

        #endregion

        #region Flex Shorthand

        [Fact]
        public void Flex_SingleNumber_SetGrowShrinkBasis()
        {
            var rule = ParseFirstRule("div { flex: 1; }");
            var grow = FindDecl(rule, "flex-grow");
            var shrink = FindDecl(rule, "flex-shrink");
            var basis = FindDecl(rule, "flex-basis");
            Assert.NotNull(grow);
            Assert.NotNull(shrink);
            Assert.NotNull(basis);
            Assert.Equal("1", grow!.Value.ToString());
        }

        [Fact]
        public void Flex_None_SetsZeroGrowZeroShrinkAutoBasis()
        {
            var rule = ParseFirstRule("div { flex: none; }");
            var grow = FindDecl(rule, "flex-grow");
            var shrink = FindDecl(rule, "flex-shrink");
            var basis = FindDecl(rule, "flex-basis");
            Assert.NotNull(grow);
            Assert.NotNull(shrink);
            Assert.NotNull(basis);
            Assert.Equal("0", grow!.Value.ToString());
            Assert.Equal("0", shrink!.Value.ToString());
            Assert.Equal("auto", basis!.Value.ToString());
        }

        [Fact]
        public void Flex_TwoNumbers_SetsGrowAndShrink()
        {
            var rule = ParseFirstRule("div { flex: 2 3; }");
            var grow = FindDecl(rule, "flex-grow");
            var shrink = FindDecl(rule, "flex-shrink");
            Assert.NotNull(grow);
            Assert.NotNull(shrink);
            Assert.Equal("2", grow!.Value.ToString());
            Assert.Equal("3", shrink!.Value.ToString());
        }

        #endregion

        #region Outline Shorthand

        [Fact]
        public void Outline_WidthStyleColor_ExpandsCorrectly()
        {
            var rule = ParseFirstRule("div { outline: 2px solid red; }");
            var outlineWidth = FindDecl(rule, "outline-width");
            var outlineStyle = FindDecl(rule, "outline-style");
            var outlineColor = FindDecl(rule, "outline-color");
            Assert.NotNull(outlineWidth);
            Assert.NotNull(outlineStyle);
            Assert.NotNull(outlineColor);
            Assert.Equal("2px", outlineWidth!.Value.ToString());
            Assert.Equal("solid", outlineStyle!.Value.ToString());
        }

        #endregion

        #region List-Style Shorthand

        [Fact]
        public void ListStyle_TypeAndPosition_ExpandsCorrectly()
        {
            var rule = ParseFirstRule("li { list-style: disc inside; }");
            var type = FindDecl(rule, "list-style-type");
            var position = FindDecl(rule, "list-style-position");
            Assert.NotNull(type);
            Assert.NotNull(position);
            Assert.Equal("disc", type!.Value.ToString());
            Assert.Equal("inside", position!.Value.ToString());
        }

        [Fact]
        public void ListStyle_None_ExpandsCorrectly()
        {
            var rule = ParseFirstRule("li { list-style: none; }");
            var type = FindDecl(rule, "list-style-type");
            Assert.NotNull(type);
            Assert.Equal("none", type!.Value.ToString());
        }

        #endregion

        #region Border-Radius Shorthand

        [Fact]
        public void BorderRadius_OneValue_SetsAllCorners()
        {
            var rule = ParseFirstRule("div { border-radius: 5px; }");
            Assert.Equal("5px", FindDecl(rule, "border-top-left-radius")!.Value.ToString());
            Assert.Equal("5px", FindDecl(rule, "border-top-right-radius")!.Value.ToString());
            Assert.Equal("5px", FindDecl(rule, "border-bottom-right-radius")!.Value.ToString());
            Assert.Equal("5px", FindDecl(rule, "border-bottom-left-radius")!.Value.ToString());
        }

        [Fact]
        public void BorderRadius_TwoValues_SetsDiagonals()
        {
            var rule = ParseFirstRule("div { border-radius: 5px 10px; }");
            Assert.Equal("5px", FindDecl(rule, "border-top-left-radius")!.Value.ToString());
            Assert.Equal("10px", FindDecl(rule, "border-top-right-radius")!.Value.ToString());
            Assert.Equal("5px", FindDecl(rule, "border-bottom-right-radius")!.Value.ToString());
            Assert.Equal("10px", FindDecl(rule, "border-bottom-left-radius")!.Value.ToString());
        }

        [Fact]
        public void BorderRadius_ThreeValues_SetsCorrectly()
        {
            var rule = ParseFirstRule("div { border-radius: 5px 10px 15px; }");
            Assert.Equal("5px", FindDecl(rule, "border-top-left-radius")!.Value.ToString());
            Assert.Equal("10px", FindDecl(rule, "border-top-right-radius")!.Value.ToString());
            Assert.Equal("15px", FindDecl(rule, "border-bottom-right-radius")!.Value.ToString());
            Assert.Equal("10px", FindDecl(rule, "border-bottom-left-radius")!.Value.ToString());
        }

        [Fact]
        public void BorderRadius_FourValues_SetsEachCorner()
        {
            var rule = ParseFirstRule("div { border-radius: 5px 10px 15px 20px; }");
            Assert.Equal("5px", FindDecl(rule, "border-top-left-radius")!.Value.ToString());
            Assert.Equal("10px", FindDecl(rule, "border-top-right-radius")!.Value.ToString());
            Assert.Equal("15px", FindDecl(rule, "border-bottom-right-radius")!.Value.ToString());
            Assert.Equal("20px", FindDecl(rule, "border-bottom-left-radius")!.Value.ToString());
        }

        #endregion
    }
}
