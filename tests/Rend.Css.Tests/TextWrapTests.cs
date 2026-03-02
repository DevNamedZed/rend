using Xunit;

namespace Rend.Css.Tests
{
    public class TextWrapTests
    {
        [Fact]
        public void TextWrap_Balance_ParsedCorrectly()
        {
            var sheet = CssParser.Parse("p { text-wrap: balance; }");
            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = Assert.Single(rule.Declarations, d => d.Property == "text-wrap");
            Assert.NotNull(decl);
        }

        [Fact]
        public void TextWrap_Wrap_ParsedCorrectly()
        {
            var sheet = CssParser.Parse("p { text-wrap: wrap; }");
            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = Assert.Single(rule.Declarations, d => d.Property == "text-wrap");
            Assert.NotNull(decl);
        }

        [Fact]
        public void TextWrap_Nowrap_ParsedCorrectly()
        {
            var sheet = CssParser.Parse("p { text-wrap: nowrap; }");
            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = Assert.Single(rule.Declarations, d => d.Property == "text-wrap");
            Assert.NotNull(decl);
        }

        [Fact]
        public void TextWrap_Pretty_ParsedCorrectly()
        {
            var sheet = CssParser.Parse("p { text-wrap: pretty; }");
            Assert.Single(sheet.Rules);
        }

        [Fact]
        public void TextWrap_IsInherited()
        {
            var desc = Rend.Css.Properties.Internal.PropertyRegistry.GetByName("text-wrap");
            Assert.NotNull(desc);
            Assert.True(desc!.Inherited);
        }

        [Fact]
        public void TextWrap_IsKeywordType()
        {
            var desc = Rend.Css.Properties.Internal.PropertyRegistry.GetByName("text-wrap");
            Assert.NotNull(desc);
            Assert.Equal(Properties.Internal.PropertyValueType.Keyword, desc!.ValueType);
        }
    }
}
