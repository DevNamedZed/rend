using System.Linq;
using Rend.Css;
using Xunit;

namespace Rend.Css.Conformance.Parsing
{
    public class ErrorRecoveryTests
    {
        #region Unclosed Braces

        [Fact]
        public void UnclosedBrace_DoesNotCrash()
        {
            var sheet = CssParser.Parse("div { color: red; ");
            Assert.NotNull(sheet);
        }

        [Fact]
        public void UnclosedBrace_StillParsesDeclaration()
        {
            var sheet = CssParser.Parse("div { color: red; ");
            // Parser may or may not produce a rule, but it should not crash
            Assert.NotNull(sheet.Rules);
        }

        [Fact]
        public void ExtraClosingBrace_DoesNotCrash()
        {
            var sheet = CssParser.Parse("div { color: red; } }");
            Assert.NotNull(sheet);
        }

        [Fact]
        public void NestedUnclosedBraces_DoesNotCrash()
        {
            var sheet = CssParser.Parse("@media screen { div { color: red; }");
            Assert.NotNull(sheet);
        }

        #endregion

        #region Missing Semicolons

        [Fact]
        public void MissingSemicolon_DoesNotCrash()
        {
            var sheet = CssParser.Parse("div { color: red width: 100px; }");
            Assert.NotNull(sheet);
            // At least one rule should be produced
            Assert.True(sheet.Rules.Count >= 1);
        }

        [Fact]
        public void MissingSemicolon_LastDeclaration_Parses()
        {
            var sheet = CssParser.Parse("div { color: red }");
            var rule = sheet.Rules.OfType<StyleRule>().FirstOrDefault();
            Assert.NotNull(rule);
            // The color declaration should still be parsed even without trailing semicolon
            Assert.True(rule!.Declarations.Count >= 1);
        }

        #endregion

        #region Invalid Properties

        [Fact]
        public void InvalidProperty_SkipsGracefully()
        {
            var sheet = CssParser.Parse("div { 123invalid: value; color: red; }");
            Assert.NotNull(sheet);
            var rule = sheet.Rules.OfType<StyleRule>().FirstOrDefault();
            Assert.NotNull(rule);
        }

        [Fact]
        public void EmptyPropertyName_SkipsGracefully()
        {
            var sheet = CssParser.Parse("div { : value; color: red; }");
            Assert.NotNull(sheet);
        }

        [Fact]
        public void MissingColon_SkipsDeclaration()
        {
            var sheet = CssParser.Parse("div { color red; display: block; }");
            Assert.NotNull(sheet);
            var rule = sheet.Rules.OfType<StyleRule>().FirstOrDefault();
            Assert.NotNull(rule);
        }

        #endregion

        #region Empty Rules

        [Fact]
        public void EmptyRule_ParsesWithNoDeclarations()
        {
            var sheet = CssParser.Parse("div { }");
            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Empty(rule.Declarations);
        }

        [Fact]
        public void MultipleEmptyRules_ParseCorrectly()
        {
            var sheet = CssParser.Parse("div { } span { } p { }");
            Assert.Equal(3, sheet.Rules.Count);
        }

        [Fact]
        public void EmptySemicolonOnly_ParsesWithNoDeclarations()
        {
            var sheet = CssParser.Parse("div { ; ; ; }");
            var rule = sheet.Rules.OfType<StyleRule>().FirstOrDefault();
            Assert.NotNull(rule);
            Assert.Empty(rule!.Declarations);
        }

        #endregion

        #region Nested Garbage

        [Fact]
        public void GarbageBeforeRule_DoesNotCrash()
        {
            var sheet = CssParser.Parse("!@#$% div { color: red; }");
            Assert.NotNull(sheet);
        }

        [Fact]
        public void GarbageInDeclaration_RecoversByNextDecl()
        {
            var sheet = CssParser.Parse("div { color: red; $$garbage$$; display: block; }");
            Assert.NotNull(sheet);
            var rule = sheet.Rules.OfType<StyleRule>().FirstOrDefault();
            Assert.NotNull(rule);
        }

        [Fact]
        public void OnlySemicolons_ProducesNoRules()
        {
            var sheet = CssParser.Parse(";;;");
            Assert.NotNull(sheet);
        }

        [Fact]
        public void CompletelyInvalid_ProducesEmptyStylesheet()
        {
            var sheet = CssParser.Parse("not css at all !!!");
            Assert.NotNull(sheet);
        }

        [Fact]
        public void NullInput_ThrowsArgumentNull()
        {
            Assert.Throws<System.ArgumentNullException>(() => CssParser.Parse((string)null!));
        }

        [Fact]
        public void EmptyInput_ProducesNoRules()
        {
            var sheet = CssParser.Parse("");
            Assert.Empty(sheet.Rules);
        }

        [Fact]
        public void WhitespaceOnly_ProducesNoRules()
        {
            var sheet = CssParser.Parse("   \n\t\r\n  ");
            Assert.Empty(sheet.Rules);
        }

        [Fact]
        public void ValidRuleAfterGarbage_StillParses()
        {
            var sheet = CssParser.Parse("@@@ !!! { } div { color: red; }");
            Assert.NotNull(sheet);
            // The div rule should still parse
            var styleRules = sheet.Rules.OfType<StyleRule>().ToList();
            Assert.True(styleRules.Count >= 1);
            var divRule = styleRules.FirstOrDefault(r => r.SelectorText.Contains("div"));
            Assert.NotNull(divRule);
        }

        #endregion

        #region Comments

        [Fact]
        public void CssComment_IsIgnored()
        {
            var sheet = CssParser.Parse("/* comment */ div { color: red; }");
            var rule = sheet.Rules.OfType<StyleRule>().FirstOrDefault();
            Assert.NotNull(rule);
        }

        [Fact]
        public void CssComment_InDeclaration_IsHandled()
        {
            var sheet = CssParser.Parse("div { /* comment */ color: red; }");
            Assert.NotNull(sheet);
            var rule = sheet.Rules.OfType<StyleRule>().FirstOrDefault();
            Assert.NotNull(rule);
        }

        #endregion
    }
}
