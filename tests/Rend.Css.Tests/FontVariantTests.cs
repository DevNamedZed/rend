using Xunit;

namespace Rend.Css.Tests
{
    public class FontVariantTests
    {
        // ═══════════════════════════════════════════
        // font-variant-ligatures
        // ═══════════════════════════════════════════

        [Fact]
        public void FontVariantLigatures_Normal_Parsed()
        {
            var css = "p { font-variant-ligatures: normal; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-variant-ligatures");
        }

        [Fact]
        public void FontVariantLigatures_None_Parsed()
        {
            var css = "p { font-variant-ligatures: none; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-variant-ligatures");
        }

        [Fact]
        public void FontVariantLigatures_CommonLigatures_Parsed()
        {
            var css = "p { font-variant-ligatures: common-ligatures; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-variant-ligatures");
        }

        // ═══════════════════════════════════════════
        // font-variant-caps
        // ═══════════════════════════════════════════

        [Fact]
        public void FontVariantCaps_SmallCaps_Parsed()
        {
            var css = "p { font-variant-caps: small-caps; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-variant-caps");
        }

        [Fact]
        public void FontVariantCaps_AllSmallCaps_Parsed()
        {
            var css = "p { font-variant-caps: all-small-caps; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-variant-caps");
        }

        [Fact]
        public void FontVariantCaps_TitlingCaps_Parsed()
        {
            var css = "p { font-variant-caps: titling-caps; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-variant-caps");
        }

        // ═══════════════════════════════════════════
        // font-variant-numeric
        // ═══════════════════════════════════════════

        [Fact]
        public void FontVariantNumeric_Normal_Parsed()
        {
            var css = "p { font-variant-numeric: normal; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-variant-numeric");
        }

        [Fact]
        public void FontVariantNumeric_TabularNums_Parsed()
        {
            var css = "p { font-variant-numeric: tabular-nums; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-variant-numeric");
        }

        [Fact]
        public void FontVariantNumeric_OldstyleNums_Parsed()
        {
            var css = "p { font-variant-numeric: oldstyle-nums; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-variant-numeric");
        }

        [Fact]
        public void FontVariantNumeric_SlashedZero_Parsed()
        {
            var css = "p { font-variant-numeric: slashed-zero; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-variant-numeric");
        }

        // ═══════════════════════════════════════════
        // font-variant-east-asian
        // ═══════════════════════════════════════════

        [Fact]
        public void FontVariantEastAsian_Normal_Parsed()
        {
            var css = "p { font-variant-east-asian: normal; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-variant-east-asian");
        }

        [Fact]
        public void FontVariantEastAsian_Jis78_Parsed()
        {
            var css = "p { font-variant-east-asian: jis78; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-variant-east-asian");
        }

        [Fact]
        public void FontVariantEastAsian_FullWidth_Parsed()
        {
            var css = "p { font-variant-east-asian: full-width; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-variant-east-asian");
        }

        // ═══════════════════════════════════════════
        // font-feature-settings
        // ═══════════════════════════════════════════

        [Fact]
        public void FontFeatureSettings_Normal_Parsed()
        {
            var css = "p { font-feature-settings: normal; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-feature-settings");
        }

        [Fact]
        public void FontFeatureSettings_SingleTag_Parsed()
        {
            var css = "p { font-feature-settings: \"smcp\"; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-feature-settings");
        }

        [Fact]
        public void FontFeatureSettings_MultipleTags_Parsed()
        {
            var css = "p { font-feature-settings: \"liga\" 1, \"calt\" 1; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-feature-settings");
        }

        [Fact]
        public void FontFeatureSettings_DisabledTag_Parsed()
        {
            var css = "p { font-feature-settings: \"liga\" 0; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "font-feature-settings");
        }
    }
}
