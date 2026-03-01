using Rend.Core.Values;
using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class MultiColumnStyleTests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private ComputedStyle ResolveElement(string css)
        {
            var resolver = new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 800,
                ViewportHeight = 600
            });

            if (!string.IsNullOrEmpty(css))
                resolver.AddStylesheet(CssParser.Parse(css));

            var element = new MockStylableElement { TagName = "div" };
            return resolver.Resolve(element);
        }

        [Fact]
        public void ColumnCount_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.ColumnCount));
        }

        [Fact]
        public void ColumnCount_Set_ResolvesCorrectly()
        {
            var style = ResolveElement("div { column-count: 3; }");
            Assert.Equal(3f, style.ColumnCount, 0.01);
        }

        [Fact]
        public void ColumnWidth_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.ColumnWidth));
        }

        [Fact]
        public void ColumnWidth_Set_ResolvesCorrectly()
        {
            var style = ResolveElement("div { column-width: 200px; }");
            Assert.Equal(200f, style.ColumnWidth, 0.01);
        }

        [Fact]
        public void ColumnGap_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.ColumnGap, 0.01);
        }

        [Fact]
        public void ColumnGap_Set_ResolvesCorrectly()
        {
            var style = ResolveElement("div { column-gap: 20px; }");
            Assert.Equal(20f, style.ColumnGap, 0.01);
        }

        [Fact]
        public void ColumnRuleStyle_Default_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBorderStyle.None, style.ColumnRuleStyle);
        }

        [Fact]
        public void ColumnRuleStyle_Solid_ResolvesCorrectly()
        {
            var style = ResolveElement("div { column-rule-style: solid; }");
            Assert.Equal(CssBorderStyle.Solid, style.ColumnRuleStyle);
        }

        [Fact]
        public void ColumnRuleWidth_Default_IsMedium()
        {
            var style = ResolveElement("");
            Assert.Equal(3f, style.ColumnRuleWidth, 0.01);
        }

        [Fact]
        public void ColumnRuleWidth_Set_ResolvesCorrectly()
        {
            var style = ResolveElement("div { column-rule-width: 2px; }");
            Assert.Equal(2f, style.ColumnRuleWidth, 0.01);
        }

        // Shorthand tests

        [Fact]
        public void Columns_Shorthand_CountOnly()
        {
            var sheet = CssParser.Parse("div { columns: 3; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(rule.Declarations, d => d.Property == "column-count");
        }

        [Fact]
        public void Columns_Shorthand_WidthOnly()
        {
            var sheet = CssParser.Parse("div { columns: 200px; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(rule.Declarations, d => d.Property == "column-width");
        }

        [Fact]
        public void Columns_Shorthand_WidthAndCount()
        {
            var sheet = CssParser.Parse("div { columns: 200px 3; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(rule.Declarations, d => d.Property == "column-width");
            Assert.Contains(rule.Declarations, d => d.Property == "column-count");
        }

        [Fact]
        public void ColumnRule_Shorthand_Expands()
        {
            var sheet = CssParser.Parse("div { column-rule: 1px solid red; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(rule.Declarations, d => d.Property == "column-rule-width");
            Assert.Contains(rule.Declarations, d => d.Property == "column-rule-style");
            Assert.Contains(rule.Declarations, d => d.Property == "column-rule-color");
        }

        [Fact]
        public void ColumnCount_NotInherited()
        {
            // column-count is non-inherited; child should get auto
            var resolver = new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 800,
                ViewportHeight = 600
            });
            resolver.AddStylesheet(CssParser.Parse("div { column-count: 3; }"));

            var parent = new MockStylableElement { TagName = "div" };
            var parentStyle = resolver.Resolve(parent);

            var child = new MockStylableElement { TagName = "p" };
            var childStyle = resolver.Resolve(child, parentStyle);

            Assert.Equal(3f, parentStyle.ColumnCount, 0.01);
            Assert.True(float.IsNaN(childStyle.ColumnCount)); // not inherited
        }
    }
}
