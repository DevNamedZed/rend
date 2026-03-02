using Xunit;

namespace Rend.Css.Tests
{
    public class ContainerQueryTests
    {
        // ═══════════════════════════════════════════
        // Parsing
        // ═══════════════════════════════════════════

        [Fact]
        public void ContainerRule_IsParsed()
        {
            var css = "@container (min-width: 400px) { .card { color: red; } }";
            var sheet = CssParser.Parse(css);
            Assert.NotEmpty(sheet.Rules);
            var rule = Assert.IsType<ContainerRule>(sheet.Rules[0]);
            Assert.Equal("(min-width: 400px)", rule.ConditionText);
            Assert.Single(rule.Rules);
        }

        [Fact]
        public void ContainerRule_WithName_IsParsed()
        {
            var css = "@container sidebar (min-width: 300px) { .nav { display: none; } }";
            var sheet = CssParser.Parse(css);
            var rule = Assert.IsType<ContainerRule>(sheet.Rules[0]);
            Assert.Equal("sidebar (min-width: 300px)", rule.ConditionText);
        }

        [Fact]
        public void ContainerRule_MultipleConditions_IsParsed()
        {
            var css = "@container (min-width: 400px) and (max-width: 800px) { .card { font-size: 18px; } }";
            var sheet = CssParser.Parse(css);
            var rule = Assert.IsType<ContainerRule>(sheet.Rules[0]);
            Assert.Contains("min-width", rule.ConditionText);
            Assert.Contains("max-width", rule.ConditionText);
        }

        [Fact]
        public void ContainerRule_NestedRules_AreParsed()
        {
            var css = @"
                @container (min-width: 400px) {
                    .card { color: red; }
                    .title { font-size: 24px; }
                }";
            var sheet = CssParser.Parse(css);
            var rule = Assert.IsType<ContainerRule>(sheet.Rules[0]);
            Assert.Equal(2, rule.Rules.Count);
        }

        [Fact]
        public void ContainerRule_Empty_IsParsed()
        {
            var css = "@container (min-width: 400px) { }";
            var sheet = CssParser.Parse(css);
            var rule = Assert.IsType<ContainerRule>(sheet.Rules[0]);
            Assert.Empty(rule.Rules);
        }

        // ═══════════════════════════════════════════
        // container-type property
        // ═══════════════════════════════════════════

        [Fact]
        public void ContainerType_Normal_Parsed()
        {
            var css = ".box { container-type: normal; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "container-type" && d.Value.ToString().Contains("normal"));
        }

        [Fact]
        public void ContainerType_Size_Parsed()
        {
            var css = ".box { container-type: size; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "container-type" && d.Value.ToString().Contains("size"));
        }

        [Fact]
        public void ContainerType_InlineSize_Parsed()
        {
            var css = ".box { container-type: inline-size; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "container-type" && d.Value.ToString().Contains("inline-size"));
        }

        // ═══════════════════════════════════════════
        // container-name property
        // ═══════════════════════════════════════════

        [Fact]
        public void ContainerName_Parsed()
        {
            var css = ".sidebar { container-name: sidebar; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "container-name");
        }

        // ═══════════════════════════════════════════
        // container shorthand
        // ═══════════════════════════════════════════

        [Fact]
        public void Container_Shorthand_NameAndType_Expanded()
        {
            var css = ".box { container: sidebar / size; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "container-name");
            Assert.Contains(sr.Declarations, d => d.Property == "container-type");
        }

        [Fact]
        public void Container_Shorthand_TypeOnly_Expanded()
        {
            var css = ".box { container: inline-size; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "container-type");
        }

        // ═══════════════════════════════════════════
        // CssRuleType
        // ═══════════════════════════════════════════

        [Fact]
        public void ContainerRule_HasCorrectType()
        {
            var css = "@container (min-width: 400px) { .card { color: red; } }";
            var sheet = CssParser.Parse(css);
            Assert.Equal(CssRuleType.Container, sheet.Rules[0].Type);
        }
    }
}
