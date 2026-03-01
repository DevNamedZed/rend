using System.Linq;
using Rend.Css;
using Xunit;

namespace Rend.Css.Conformance.Parsing
{
    public class AtRuleParsingTests
    {
        #region @media

        [Fact]
        public void Media_SimpleScreen_ParsesAsMediaRule()
        {
            var sheet = CssParser.Parse("@media screen { div { color: red; } }");
            Assert.Single(sheet.Rules);
            var media = Assert.IsType<MediaRule>(sheet.Rules[0]);
            Assert.Equal(CssRuleType.Media, media.Type);
            Assert.Contains("screen", media.MediaText);
            Assert.Single(media.Rules);
        }

        [Fact]
        public void Media_MaxWidth_ParsesCondition()
        {
            var sheet = CssParser.Parse("@media (max-width: 768px) { .mobile { display: block; } }");
            var media = Assert.IsType<MediaRule>(sheet.Rules[0]);
            Assert.Contains("max-width", media.MediaText);
            Assert.Single(media.Rules);
        }

        [Fact]
        public void Media_And_Condition_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("@media screen and (min-width: 1024px) { body { font-size: 18px; } }");
            var media = Assert.IsType<MediaRule>(sheet.Rules[0]);
            Assert.Contains("screen", media.MediaText);
            Assert.Contains("min-width", media.MediaText);
        }

        [Fact]
        public void Media_MultipleRules_NestedCorrectly()
        {
            var css = @"@media print {
                body { font-size: 12pt; }
                .no-print { display: none; }
            }";
            var sheet = CssParser.Parse(css);
            var media = Assert.IsType<MediaRule>(sheet.Rules[0]);
            Assert.Equal(2, media.Rules.Count);
        }

        #endregion

        #region @font-face

        [Fact]
        public void FontFace_ParsesAsFontFaceRule()
        {
            var css = @"@font-face {
                font-family: 'CustomFont';
                src: url('font.woff2');
            }";
            var sheet = CssParser.Parse(css);
            Assert.Single(sheet.Rules);
            var fontFace = Assert.IsType<FontFaceRule>(sheet.Rules[0]);
            Assert.Equal(CssRuleType.FontFace, fontFace.Type);
            Assert.True(fontFace.Declarations.Count > 0);
        }

        [Fact]
        public void FontFace_HasFontFamilyDeclaration()
        {
            var css = "@font-face { font-family: 'TestFont'; src: url('test.woff'); }";
            var sheet = CssParser.Parse(css);
            var fontFace = Assert.IsType<FontFaceRule>(sheet.Rules[0]);
            var family = fontFace.Declarations.FirstOrDefault(d => d.Property == "font-family");
            Assert.NotNull(family);
        }

        #endregion

        #region @import

        [Fact]
        public void Import_StringUrl_ParsesAsImportRule()
        {
            var sheet = CssParser.Parse("@import 'styles.css';");
            Assert.Single(sheet.Rules);
            var import = Assert.IsType<ImportRule>(sheet.Rules[0]);
            Assert.Equal(CssRuleType.Import, import.Type);
            Assert.Equal("styles.css", import.Url);
        }

        [Fact]
        public void Import_UrlFunction_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("@import url('reset.css');");
            var import = Assert.IsType<ImportRule>(sheet.Rules[0]);
            Assert.Equal("reset.css", import.Url);
        }

        [Fact]
        public void Import_WithMedia_ParsesMediaText()
        {
            var sheet = CssParser.Parse("@import 'print.css' print;");
            var import = Assert.IsType<ImportRule>(sheet.Rules[0]);
            Assert.Equal("print.css", import.Url);
            Assert.NotNull(import.MediaText);
            Assert.Contains("print", import.MediaText);
        }

        #endregion

        #region @page

        [Fact]
        public void Page_NoSelector_ParsesAsPageRule()
        {
            var sheet = CssParser.Parse("@page { margin: 1cm; }");
            Assert.Single(sheet.Rules);
            var page = Assert.IsType<PageRule>(sheet.Rules[0]);
            Assert.Equal(CssRuleType.Page, page.Type);
            Assert.True(page.Declarations.Count > 0);
        }

        [Fact]
        public void Page_WithSelector_ParsesPageSelector()
        {
            var sheet = CssParser.Parse("@page :first { margin-top: 2cm; }");
            var page = Assert.IsType<PageRule>(sheet.Rules[0]);
            Assert.NotNull(page.PageSelector);
            Assert.Contains("first", page.PageSelector!);
        }

        #endregion

        #region @supports

        [Fact]
        public void Supports_SimpleCondition_ParsesAsSupportsRule()
        {
            var css = "@supports (display: grid) { .grid { display: grid; } }";
            var sheet = CssParser.Parse(css);
            Assert.Single(sheet.Rules);
            var supports = Assert.IsType<SupportsRule>(sheet.Rules[0]);
            Assert.Equal(CssRuleType.Supports, supports.Type);
            Assert.Contains("display", supports.ConditionText);
            Assert.Single(supports.Rules);
        }

        [Fact]
        public void Supports_NotCondition_ParsesCorrectly()
        {
            var css = "@supports not (display: grid) { .fallback { float: left; } }";
            var sheet = CssParser.Parse(css);
            var supports = Assert.IsType<SupportsRule>(sheet.Rules[0]);
            Assert.Contains("not", supports.ConditionText);
        }

        #endregion

        #region @keyframes

        [Fact]
        public void Keyframes_ParsesAsKeyframesRule()
        {
            var css = @"@keyframes fadeIn {
                from { opacity: 0; }
                to { opacity: 1; }
            }";
            var sheet = CssParser.Parse(css);
            Assert.Single(sheet.Rules);
            var kf = Assert.IsType<KeyframesRule>(sheet.Rules[0]);
            Assert.Equal(CssRuleType.Keyframes, kf.Type);
            Assert.Equal("fadeIn", kf.Name);
            Assert.Equal(2, kf.Keyframes.Count);
        }

        [Fact]
        public void Keyframes_WithPercentage_ParsesStops()
        {
            var css = @"@keyframes slide {
                0% { transform: translateX(0); }
                50% { transform: translateX(50px); }
                100% { transform: translateX(100px); }
            }";
            var sheet = CssParser.Parse(css);
            var kf = Assert.IsType<KeyframesRule>(sheet.Rules[0]);
            Assert.Equal(3, kf.Keyframes.Count);
            Assert.Contains("0%", kf.Keyframes[0].Selector);
            Assert.Contains("50%", kf.Keyframes[1].Selector);
            Assert.Contains("100%", kf.Keyframes[2].Selector);
        }

        [Fact]
        public void Keyframes_StopsHaveDeclarations()
        {
            var css = "@keyframes test { from { opacity: 0; color: red; } to { opacity: 1; } }";
            var sheet = CssParser.Parse(css);
            var kf = Assert.IsType<KeyframesRule>(sheet.Rules[0]);
            Assert.True(kf.Keyframes[0].Declarations.Count >= 1);
        }

        #endregion

        #region @layer

        [Fact]
        public void Layer_DeclarationForm_ParsesAsLayerRule()
        {
            var sheet = CssParser.Parse("@layer base, components;");
            Assert.Single(sheet.Rules);
            var layer = Assert.IsType<LayerRule>(sheet.Rules[0]);
            Assert.Equal(CssRuleType.Layer, layer.Type);
            Assert.False(layer.IsBlock);
            Assert.Contains("base", layer.Names);
            Assert.Contains("components", layer.Names);
        }

        [Fact]
        public void Layer_BlockForm_ParsesWithNestedRules()
        {
            var css = "@layer utilities { .text-center { text-align: center; } }";
            var sheet = CssParser.Parse(css);
            var layer = Assert.IsType<LayerRule>(sheet.Rules[0]);
            Assert.True(layer.IsBlock);
            Assert.Single(layer.Rules);
        }

        #endregion

        #region @namespace

        [Fact]
        public void Namespace_WithPrefix_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("@namespace svg \"http://www.w3.org/2000/svg\";");
            Assert.Single(sheet.Rules);
            var ns = Assert.IsType<NamespaceRule>(sheet.Rules[0]);
            Assert.Equal(CssRuleType.Namespace, ns.Type);
            Assert.Equal("svg", ns.Prefix);
            Assert.Equal("http://www.w3.org/2000/svg", ns.Uri);
        }

        [Fact]
        public void Namespace_DefaultNamespace_ParsesCorrectly()
        {
            var sheet = CssParser.Parse("@namespace \"http://www.w3.org/1999/xhtml\";");
            Assert.Single(sheet.Rules);
            var ns = Assert.IsType<NamespaceRule>(sheet.Rules[0]);
            Assert.Null(ns.Prefix);
            Assert.Equal("http://www.w3.org/1999/xhtml", ns.Uri);
        }

        #endregion

        #region Mixed Rules

        [Fact]
        public void MixedRules_ParseInCorrectOrder()
        {
            var css = @"
                @import 'reset.css';
                @media screen { body { font-size: 16px; } }
                div { color: red; }
            ";
            var sheet = CssParser.Parse(css);
            Assert.Equal(3, sheet.Rules.Count);
            Assert.IsType<ImportRule>(sheet.Rules[0]);
            Assert.IsType<MediaRule>(sheet.Rules[1]);
            Assert.IsType<StyleRule>(sheet.Rules[2]);
        }

        #endregion
    }
}
