using Xunit;

namespace Rend.Css.Tests
{
    public class AccentColorTests
    {
        [Fact]
        public void AccentColor_ParsedAsColor()
        {
            var sheet = CssParser.Parse("input { accent-color: #ff0000; }");
            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = Assert.Single(rule.Declarations, d => d.Property == "accent-color");
            Assert.NotNull(decl);
        }

        [Fact]
        public void AccentColor_NamedColor()
        {
            var sheet = CssParser.Parse("input { accent-color: blue; }");
            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = Assert.Single(rule.Declarations, d => d.Property == "accent-color");
            Assert.NotNull(decl);
        }

        [Fact]
        public void AccentColor_Auto()
        {
            var sheet = CssParser.Parse("input { accent-color: auto; }");
            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = Assert.Single(rule.Declarations, d => d.Property == "accent-color");
            Assert.NotNull(decl);
        }

        [Fact]
        public void AccentColor_IsInherited()
        {
            // accent-color is registered as inherited in PropertyRegistry
            var desc = Rend.Css.Properties.Internal.PropertyRegistry.GetByName("accent-color");
            Assert.NotNull(desc);
            Assert.True(desc!.Inherited);
        }

        [Fact]
        public void AccentColor_IsColorType()
        {
            var desc = Rend.Css.Properties.Internal.PropertyRegistry.GetByName("accent-color");
            Assert.NotNull(desc);
            Assert.Equal(Properties.Internal.PropertyValueType.Color, desc!.ValueType);
        }
    }
}
