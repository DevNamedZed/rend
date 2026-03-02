using Xunit;

namespace Rend.Css.Tests
{
    public class NewCssPropertyTests
    {
        // --- forced-color-adjust ---

        [Fact]
        public void ForcedColorAdjust_Auto_ParsedCorrectly()
        {
            var sheet = CssParser.Parse("div { forced-color-adjust: auto; }");
            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = Assert.Single(rule.Declarations, d => d.Property == "forced-color-adjust");
            Assert.NotNull(decl);
        }

        [Fact]
        public void ForcedColorAdjust_None_ParsedCorrectly()
        {
            var sheet = CssParser.Parse("div { forced-color-adjust: none; }");
            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = Assert.Single(rule.Declarations, d => d.Property == "forced-color-adjust");
            Assert.NotNull(decl);
        }

        [Fact]
        public void ForcedColorAdjust_IsInherited()
        {
            var desc = Rend.Css.Properties.Internal.PropertyRegistry.GetByName("forced-color-adjust");
            Assert.NotNull(desc);
            Assert.True(desc!.Inherited);
        }

        // --- initial-letter ---

        [Fact]
        public void InitialLetter_Normal_ParsedCorrectly()
        {
            var sheet = CssParser.Parse("p { initial-letter: normal; }");
            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = Assert.Single(rule.Declarations, d => d.Property == "initial-letter");
            Assert.NotNull(decl);
        }

        [Fact]
        public void InitialLetter_Number_ParsedCorrectly()
        {
            var sheet = CssParser.Parse("p { initial-letter: 3; }");
            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = Assert.Single(rule.Declarations, d => d.Property == "initial-letter");
            Assert.NotNull(decl);
        }

        [Fact]
        public void InitialLetter_IsNotInherited()
        {
            var desc = Rend.Css.Properties.Internal.PropertyRegistry.GetByName("initial-letter");
            Assert.NotNull(desc);
            Assert.False(desc!.Inherited);
        }

        // --- hanging-punctuation ---

        [Fact]
        public void HangingPunctuation_None_ParsedCorrectly()
        {
            var sheet = CssParser.Parse("p { hanging-punctuation: none; }");
            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = Assert.Single(rule.Declarations, d => d.Property == "hanging-punctuation");
            Assert.NotNull(decl);
        }

        [Fact]
        public void HangingPunctuation_First_ParsedCorrectly()
        {
            var sheet = CssParser.Parse("p { hanging-punctuation: first; }");
            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = Assert.Single(rule.Declarations, d => d.Property == "hanging-punctuation");
            Assert.NotNull(decl);
        }

        [Fact]
        public void HangingPunctuation_Last_ParsedCorrectly()
        {
            var sheet = CssParser.Parse("p { hanging-punctuation: last; }");
            Assert.Single(sheet.Rules);
        }

        [Fact]
        public void HangingPunctuation_ForceEnd_ParsedCorrectly()
        {
            var sheet = CssParser.Parse("p { hanging-punctuation: force-end; }");
            Assert.Single(sheet.Rules);
        }

        [Fact]
        public void HangingPunctuation_IsInherited()
        {
            var desc = Rend.Css.Properties.Internal.PropertyRegistry.GetByName("hanging-punctuation");
            Assert.NotNull(desc);
            Assert.True(desc!.Inherited);
        }
    }
}
