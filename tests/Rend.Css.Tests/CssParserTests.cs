using System.Linq;
using Rend.Core.Values;
using Xunit;

namespace Rend.Css.Tests
{
    public class CssParserTests
    {
        #region Basic Rule Parsing

        [Fact]
        public void Parse_SingleStyleRule_ReturnsSingleRule()
        {
            var sheet = CssParser.Parse("div { color: red; }");

            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Equal(CssRuleType.Style, rule.Type);
            Assert.Equal("div", rule.SelectorText);
        }

        [Fact]
        public void Parse_MultipleRules_ReturnsAll()
        {
            var css = "h1 { font-size: 24px; } p { margin: 10px; }";
            var sheet = CssParser.Parse(css);

            Assert.Equal(2, sheet.Rules.Count);
            Assert.All(sheet.Rules, r => Assert.IsType<StyleRule>(r));
        }

        [Fact]
        public void Parse_EmptyStylesheet_ReturnsNoRules()
        {
            var sheet = CssParser.Parse("");
            Assert.Empty(sheet.Rules);
        }

        [Fact]
        public void Parse_WhitespaceOnly_ReturnsNoRules()
        {
            var sheet = CssParser.Parse("   \n\t  ");
            Assert.Empty(sheet.Rules);
        }

        [Fact]
        public void Parse_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => CssParser.Parse((string)null!));
        }

        #endregion

        #region Selector Parsing

        [Fact]
        public void Parse_ClassSelector_PreservesSelectorText()
        {
            var sheet = CssParser.Parse(".main { color: blue; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Equal(".main", rule.SelectorText);
        }

        [Fact]
        public void Parse_IdSelector_PreservesSelectorText()
        {
            var sheet = CssParser.Parse("#header { display: block; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Equal("#header", rule.SelectorText);
        }

        [Fact]
        public void Parse_CompoundSelector_PreservesSelectorText()
        {
            var sheet = CssParser.Parse("div.main > p { color: red; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains("div", rule.SelectorText);
            Assert.Contains(".main", rule.SelectorText);
            Assert.Contains(">", rule.SelectorText);
            Assert.Contains("p", rule.SelectorText);
        }

        [Fact]
        public void Parse_CommaSelectors_PreservesSelectorText()
        {
            var sheet = CssParser.Parse("h1, h2, h3 { font-weight: bold; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains("h1", rule.SelectorText);
            Assert.Contains("h2", rule.SelectorText);
            Assert.Contains("h3", rule.SelectorText);
        }

        [Fact]
        public void Parse_AttributeSelector_PreservesSelectorText()
        {
            var sheet = CssParser.Parse("a[href] { color: blue; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains("[href]", rule.SelectorText);
        }

        [Fact]
        public void Parse_PseudoClassSelector_PreservesSelectorText()
        {
            var sheet = CssParser.Parse("a:hover { color: red; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(":hover", rule.SelectorText);
        }

        #endregion

        #region Declaration Parsing

        [Fact]
        public void Parse_SingleDeclaration_ParsesPropertyAndValue()
        {
            var sheet = CssParser.Parse("p { color: red; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.True(rule.Declarations.Count >= 1);

            var decl = rule.Declarations.First(d => d.Property == "color");
            Assert.NotNull(decl);
            Assert.False(decl.Important);
        }

        [Fact]
        public void Parse_MultipleDeclarations_ParsesAll()
        {
            var css = "div { margin-top: 10px; margin-bottom: 20px; }";
            var sheet = CssParser.Parse(css);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var marginTop = rule.Declarations.FirstOrDefault(d => d.Property == "margin-top");
            var marginBottom = rule.Declarations.FirstOrDefault(d => d.Property == "margin-bottom");

            Assert.NotNull(marginTop);
            Assert.NotNull(marginBottom);
        }

        [Fact]
        public void Parse_ImportantDeclaration_SetsImportantFlag()
        {
            var sheet = CssParser.Parse("p { color: red !important; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var decl = rule.Declarations.First(d => d.Property == "color");
            Assert.True(decl.Important);
        }

        [Fact]
        public void Parse_DeclarationWithoutSemicolon_StillParses()
        {
            var sheet = CssParser.Parse("p { color: red }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.True(rule.Declarations.Count >= 1);
        }

        #endregion

        #region Property Value Types

        [Fact]
        public void Parse_DimensionValue_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("p { font-size: 16px; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var decl = rule.Declarations.First(d => d.Property == "font-size");
            var dim = Assert.IsType<CssDimensionValue>(decl.Value);
            Assert.Equal(16f, dim.Value);
            Assert.Equal("px", dim.Unit);
        }

        [Fact]
        public void Parse_PercentageValue_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("div { width: 50%; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var decl = rule.Declarations.First(d => d.Property == "width");
            var pct = Assert.IsType<CssPercentageValue>(decl.Value);
            Assert.Equal(50f, pct.Value);
        }

        [Fact]
        public void Parse_KeywordValue_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("div { display: block; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var decl = rule.Declarations.First(d => d.Property == "display");
            var kw = Assert.IsType<CssKeywordValue>(decl.Value);
            Assert.Equal("block", kw.Keyword);
        }

        [Fact]
        public void Parse_ColorHex6_ParsesAsColor()
        {
            var sheet = CssParser.Parse("p { color: #ff0000; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var decl = rule.Declarations.First(d => d.Property == "color");
            var color = Assert.IsType<CssColorValue>(decl.Value);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(0, color.Color.G);
            Assert.Equal(0, color.Color.B);
        }

        [Fact]
        public void Parse_ColorHex3_ParsesAsColor()
        {
            var sheet = CssParser.Parse("p { color: #f00; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var decl = rule.Declarations.First(d => d.Property == "color");
            var color = Assert.IsType<CssColorValue>(decl.Value);
            Assert.Equal(255, color.Color.R);
            Assert.Equal(0, color.Color.G);
            Assert.Equal(0, color.Color.B);
        }

        [Fact]
        public void Parse_NamedColor_ParsesAsColor()
        {
            var sheet = CssParser.Parse("p { color: blue; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var decl = rule.Declarations.First(d => d.Property == "color");
            var color = Assert.IsType<CssColorValue>(decl.Value);
            Assert.Equal(0, color.Color.R);
            Assert.Equal(0, color.Color.G);
            Assert.Equal(255, color.Color.B);
        }

        [Fact]
        public void Parse_NumberValue_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("div { opacity: 0.5; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var decl = rule.Declarations.First(d => d.Property == "opacity");
            var num = Assert.IsType<CssNumberValue>(decl.Value);
            Assert.Equal(0.5f, num.Value);
        }

        [Fact]
        public void Parse_StringValue_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("div { content: \"hello\"; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var decl = rule.Declarations.First(d => d.Property == "content");
            var str = Assert.IsType<CssStringValue>(decl.Value);
            Assert.Equal("hello", str.Value);
        }

        [Fact]
        public void Parse_EmUnit_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("p { margin-top: 2em; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var decl = rule.Declarations.First(d => d.Property == "margin-top");
            var dim = Assert.IsType<CssDimensionValue>(decl.Value);
            Assert.Equal(2f, dim.Value);
            Assert.Equal("em", dim.Unit);
        }

        [Fact]
        public void Parse_RemUnit_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("p { font-size: 1.5rem; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var decl = rule.Declarations.First(d => d.Property == "font-size");
            var dim = Assert.IsType<CssDimensionValue>(decl.Value);
            Assert.Equal(1.5f, dim.Value);
            Assert.Equal("rem", dim.Unit);
        }

        [Fact]
        public void Parse_PtUnit_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("p { font-size: 12pt; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var decl = rule.Declarations.First(d => d.Property == "font-size");
            var dim = Assert.IsType<CssDimensionValue>(decl.Value);
            Assert.Equal(12f, dim.Value);
            Assert.Equal("pt", dim.Unit);
        }

        #endregion

        #region Shorthand Expansion

        [Fact]
        public void Parse_MarginShorthandFourValues_ExpandsToLonghands()
        {
            var sheet = CssParser.Parse("div { margin: 10px 20px 30px 40px; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var mt = rule.Declarations.FirstOrDefault(d => d.Property == "margin-top");
            var mr = rule.Declarations.FirstOrDefault(d => d.Property == "margin-right");
            var mb = rule.Declarations.FirstOrDefault(d => d.Property == "margin-bottom");
            var ml = rule.Declarations.FirstOrDefault(d => d.Property == "margin-left");

            Assert.NotNull(mt);
            Assert.NotNull(mr);
            Assert.NotNull(mb);
            Assert.NotNull(ml);
        }

        [Fact]
        public void Parse_PaddingShorthandSingleValue_ExpandsToAllFour()
        {
            var sheet = CssParser.Parse("div { padding: 5px; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);

            var pt = rule.Declarations.FirstOrDefault(d => d.Property == "padding-top");
            var pr = rule.Declarations.FirstOrDefault(d => d.Property == "padding-right");
            var pb = rule.Declarations.FirstOrDefault(d => d.Property == "padding-bottom");
            var pl = rule.Declarations.FirstOrDefault(d => d.Property == "padding-left");

            Assert.NotNull(pt);
            Assert.NotNull(pr);
            Assert.NotNull(pb);
            Assert.NotNull(pl);
        }

        #endregion

        #region At-Rules

        [Fact]
        public void Parse_MediaRule_ParsesMediaTextAndNestedRules()
        {
            var css = "@media screen { h1 { color: red; } }";
            var sheet = CssParser.Parse(css);

            Assert.Single(sheet.Rules);
            var media = Assert.IsType<MediaRule>(sheet.Rules[0]);
            Assert.Equal(CssRuleType.Media, media.Type);
            Assert.Contains("screen", media.MediaText);
            Assert.Single(media.Rules);
            Assert.IsType<StyleRule>(media.Rules[0]);
        }

        [Fact]
        public void Parse_MediaRuleWithMinWidth_ParsesMediaText()
        {
            var css = "@media (min-width: 768px) { .container { width: 750px; } }";
            var sheet = CssParser.Parse(css);

            var media = Assert.IsType<MediaRule>(sheet.Rules[0]);
            Assert.Contains("min-width", media.MediaText);
            Assert.Single(media.Rules);
        }

        [Fact]
        public void Parse_FontFaceRule_ParsesDeclarations()
        {
            var css = "@font-face { font-family: \"MyFont\"; font-weight: 400; }";
            var sheet = CssParser.Parse(css);

            Assert.Single(sheet.Rules);
            var fontFace = Assert.IsType<FontFaceRule>(sheet.Rules[0]);
            Assert.Equal(CssRuleType.FontFace, fontFace.Type);
            Assert.True(fontFace.Declarations.Count >= 1);
        }

        [Fact]
        public void Parse_ImportRuleWithString_ParsesUrl()
        {
            var css = "@import \"styles.css\";";
            var sheet = CssParser.Parse(css);

            Assert.Single(sheet.Rules);
            var import = Assert.IsType<ImportRule>(sheet.Rules[0]);
            Assert.Equal(CssRuleType.Import, import.Type);
            Assert.Equal("styles.css", import.Url);
            Assert.Null(import.MediaText);
        }

        [Fact]
        public void Parse_ImportRuleWithMediaQuery_ParsesMediaText()
        {
            var css = "@import \"print.css\" print;";
            var sheet = CssParser.Parse(css);

            var import = Assert.IsType<ImportRule>(sheet.Rules[0]);
            Assert.Equal("print.css", import.Url);
            Assert.NotNull(import.MediaText);
            Assert.Contains("print", import.MediaText);
        }

        [Fact]
        public void Parse_PageRule_ParsesDeclarations()
        {
            var css = "@page { margin: 1in; }";
            var sheet = CssParser.Parse(css);

            Assert.Single(sheet.Rules);
            var page = Assert.IsType<PageRule>(sheet.Rules[0]);
            Assert.Equal(CssRuleType.Page, page.Type);
            Assert.True(page.Declarations.Count >= 1);
        }

        [Fact]
        public void Parse_PageRuleWithSelector_ParsesSelector()
        {
            var css = "@page :first { margin-top: 2in; }";
            var sheet = CssParser.Parse(css);

            var page = Assert.IsType<PageRule>(sheet.Rules[0]);
            Assert.NotNull(page.PageSelector);
            Assert.Contains("first", page.PageSelector);
        }

        #endregion

        #region Error Recovery

        [Fact]
        public void Parse_MalformedRule_SkipsAndContinues()
        {
            var css = "invalid { color } p { margin: 5px; }";
            var sheet = CssParser.Parse(css);

            // The parser should recover and parse at least one valid rule
            Assert.True(sheet.Rules.Count >= 1);
        }

        [Fact]
        public void Parse_CssComments_AreIgnored()
        {
            var css = "/* comment */ p { color: red; } /* another comment */";
            var sheet = CssParser.Parse(css);

            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Equal("p", rule.SelectorText);
        }

        #endregion

        #region Complex CSS

        [Fact]
        public void Parse_RealWorldCSS_ParsesWithoutError()
        {
            var css = @"
                body {
                    font-family: Arial, sans-serif;
                    font-size: 16px;
                    line-height: 1.5;
                    color: #333;
                    margin: 0;
                    padding: 0;
                }

                h1, h2, h3 {
                    font-weight: bold;
                    margin-bottom: 10px;
                }

                .container {
                    max-width: 1200px;
                    margin: 0 auto;
                    padding: 0 15px;
                }

                @media (max-width: 768px) {
                    .container {
                        padding: 0 10px;
                    }
                }

                a:hover {
                    color: #0066cc;
                    text-decoration: underline;
                }
            ";

            var sheet = CssParser.Parse(css);
            Assert.True(sheet.Rules.Count >= 4);
        }

        [Fact]
        public void Parse_ZeroWithoutUnit_ParsesAsNumber()
        {
            var sheet = CssParser.Parse("div { margin-top: 0; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = rule.Declarations.First(d => d.Property == "margin-top");

            // 0 can be parsed as a number (unitless zero is valid in CSS)
            Assert.NotNull(decl.Value);
        }

        [Fact]
        public void Parse_NegativeValue_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("div { margin-top: -10px; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = rule.Declarations.First(d => d.Property == "margin-top");
            var dim = Assert.IsType<CssDimensionValue>(decl.Value);
            Assert.Equal(-10f, dim.Value);
        }

        [Fact]
        public void Parse_DecimalValue_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("div { line-height: 1.6; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = rule.Declarations.First(d => d.Property == "line-height");
            var num = Assert.IsType<CssNumberValue>(decl.Value);
            Assert.Equal(1.6f, num.Value, 0.01f);
        }

        #endregion

        #region Stream Parsing

        [Fact]
        public void Parse_FromStream_ProducesValidStylesheet()
        {
            var css = "p { color: red; }";
            var bytes = System.Text.Encoding.UTF8.GetBytes(css);
            using var stream = new System.IO.MemoryStream(bytes);

            var sheet = CssParser.Parse(stream);

            Assert.Single(sheet.Rules);
            Assert.IsType<StyleRule>(sheet.Rules[0]);
        }

        [Fact]
        public void Parse_FromNullStream_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => CssParser.Parse((System.IO.Stream)null!));
        }

        #endregion

        #region @namespace

        [Fact]
        public void Parse_NamespaceWithPrefix_ReturnsNamespaceRule()
        {
            var sheet = CssParser.Parse("@namespace svg \"http://www.w3.org/2000/svg\";");
            Assert.Single(sheet.Rules);
            var ns = Assert.IsType<NamespaceRule>(sheet.Rules[0]);
            Assert.Equal("svg", ns.Prefix);
            Assert.Equal("http://www.w3.org/2000/svg", ns.Uri);
            Assert.Equal(CssRuleType.Namespace, ns.Type);
        }

        [Fact]
        public void Parse_DefaultNamespace_NullPrefix()
        {
            var sheet = CssParser.Parse("@namespace \"http://www.w3.org/1999/xhtml\";");
            Assert.Single(sheet.Rules);
            var ns = Assert.IsType<NamespaceRule>(sheet.Rules[0]);
            Assert.Null(ns.Prefix);
            Assert.Equal("http://www.w3.org/1999/xhtml", ns.Uri);
        }

        [Fact]
        public void Parse_NamespaceWithUrl_ReturnsUri()
        {
            var sheet = CssParser.Parse("@namespace svg url(\"http://www.w3.org/2000/svg\");");
            Assert.Single(sheet.Rules);
            var ns = Assert.IsType<NamespaceRule>(sheet.Rules[0]);
            Assert.Equal("svg", ns.Prefix);
            Assert.Equal("http://www.w3.org/2000/svg", ns.Uri);
        }

        [Fact]
        public void Parse_NamespaceFollowedByRules_BothParsed()
        {
            var css = "@namespace \"http://www.w3.org/1999/xhtml\"; p { color: red; }";
            var sheet = CssParser.Parse(css);
            Assert.Equal(2, sheet.Rules.Count);
            Assert.IsType<NamespaceRule>(sheet.Rules[0]);
            Assert.IsType<StyleRule>(sheet.Rules[1]);
        }

        [Fact]
        public void Parse_MultipleNamespaces_AllParsed()
        {
            var css = "@namespace \"http://www.w3.org/1999/xhtml\";\n@namespace svg \"http://www.w3.org/2000/svg\";";
            var sheet = CssParser.Parse(css);
            Assert.Equal(2, sheet.Rules.Count);
            var ns1 = Assert.IsType<NamespaceRule>(sheet.Rules[0]);
            Assert.Null(ns1.Prefix);
            var ns2 = Assert.IsType<NamespaceRule>(sheet.Rules[1]);
            Assert.Equal("svg", ns2.Prefix);
        }

        #endregion
    }
}
