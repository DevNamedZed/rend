using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class LayerTests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private StyleResolver CreateResolver()
        {
            return new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 800,
                ViewportHeight = 600
            });
        }

        // ═══════════════════════════════════════════
        // Parsing @layer rules
        // ═══════════════════════════════════════════

        [Fact]
        public void Layer_DeclarationForm_SingleName()
        {
            var sheet = CssParser.Parse("@layer utilities;");

            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<LayerRule>(sheet.Rules[0]);
            Assert.Single(rule.Names);
            Assert.Equal("utilities", rule.Names[0]);
            Assert.False(rule.IsBlock);
            Assert.Empty(rule.Rules);
        }

        [Fact]
        public void Layer_DeclarationForm_MultipleNames()
        {
            var sheet = CssParser.Parse("@layer base, components, utilities;");

            var rule = Assert.IsType<LayerRule>(sheet.Rules[0]);
            Assert.Equal(3, rule.Names.Count);
            Assert.Equal("base", rule.Names[0]);
            Assert.Equal("components", rule.Names[1]);
            Assert.Equal("utilities", rule.Names[2]);
            Assert.False(rule.IsBlock);
        }

        [Fact]
        public void Layer_BlockForm_WithRules()
        {
            var sheet = CssParser.Parse(@"
                @layer base {
                    p { color: red; }
                    div { margin: 10px; }
                }
            ");

            var rule = Assert.IsType<LayerRule>(sheet.Rules[0]);
            Assert.Single(rule.Names);
            Assert.Equal("base", rule.Names[0]);
            Assert.True(rule.IsBlock);
            Assert.Equal(2, rule.Rules.Count);
        }

        [Fact]
        public void Layer_BlockForm_NestedStyleRules()
        {
            var sheet = CssParser.Parse(@"
                @layer theme {
                    h1 { font-size: 32px; }
                }
            ");

            var rule = Assert.IsType<LayerRule>(sheet.Rules[0]);
            var styleRule = Assert.IsType<StyleRule>(rule.Rules[0]);
            Assert.Equal("h1", styleRule.SelectorText);
        }

        [Fact]
        public void Layer_AnonymousBlock()
        {
            var sheet = CssParser.Parse(@"
                @layer {
                    p { color: blue; }
                }
            ");

            var rule = Assert.IsType<LayerRule>(sheet.Rules[0]);
            Assert.Empty(rule.Names);
            Assert.True(rule.IsBlock);
            Assert.Single(rule.Rules);
        }

        [Fact]
        public void Layer_DottedName()
        {
            var sheet = CssParser.Parse("@layer framework.base;");

            var rule = Assert.IsType<LayerRule>(sheet.Rules[0]);
            Assert.Single(rule.Names);
            Assert.Equal("framework.base", rule.Names[0]);
        }

        [Fact]
        public void Layer_FollowedByStyleRules()
        {
            var sheet = CssParser.Parse(@"
                @layer base;
                div { color: red; }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            Assert.IsType<LayerRule>(sheet.Rules[0]);
            Assert.IsType<StyleRule>(sheet.Rules[1]);
        }

        [Fact]
        public void Layer_MultipleDeclarations()
        {
            var sheet = CssParser.Parse(@"
                @layer base;
                @layer components;
                @layer utilities;
            ");

            Assert.Equal(3, sheet.Rules.Count);
            Assert.All(sheet.Rules, r => Assert.IsType<LayerRule>(r));
        }

        // ═══════════════════════════════════════════
        // @layer in style resolution
        // ═══════════════════════════════════════════

        [Fact]
        public void Layer_BlockRules_AppliedInResolution()
        {
            var resolver = CreateResolver();
            resolver.AddStylesheet(CssParser.Parse(@"
                @layer base {
                    div { width: 200px; }
                }
            "));

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(200f, style.Width, 0.01);
        }

        [Fact]
        public void Layer_UnlayeredWinsOverLayered()
        {
            // Per spec: unlayered styles have higher priority than layered styles
            var resolver = CreateResolver();
            resolver.AddStylesheet(CssParser.Parse(@"
                @layer base {
                    div { width: 100px; }
                }
                div { width: 200px; }
            "));

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            // Unlayered style wins via source order
            Assert.Equal(200f, style.Width, 0.01);
        }

        [Fact]
        public void Layer_DeclarationForm_DoesNotAffectStyles()
        {
            // Declaration-only @layer doesn't contain any style rules
            var resolver = CreateResolver();
            resolver.AddStylesheet(CssParser.Parse(@"
                @layer base, utilities;
                div { width: 300px; }
            "));

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(300f, style.Width, 0.01);
        }
    }
}
