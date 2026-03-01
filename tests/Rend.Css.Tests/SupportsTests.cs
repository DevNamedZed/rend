using Rend.Css;
using Rend.Css.Supports.Internal;
using Xunit;

namespace Rend.Css.Tests
{
    public class SupportsTests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private StyleResolver CreateResolver()
        {
            return new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 1920,
                ViewportHeight = 1080
            });
        }

        // ═══════════════════════════════════════════
        // SupportsEvaluator unit tests
        // ═══════════════════════════════════════════

        [Theory]
        [InlineData("(display: block)", true)]      // display is registered
        [InlineData("(width: 100px)", true)]         // width is registered
        [InlineData("(color: red)", true)]           // color is registered
        [InlineData("(font-size: 12px)", true)]      // font-size is registered
        [InlineData("(fake-property: value)", false)] // not registered
        [InlineData("(banana: yellow)", false)]      // not registered
        public void Evaluator_SimpleCondition(string condition, bool expected)
        {
            Assert.Equal(expected, SupportsEvaluator.Evaluate(condition));
        }

        [Fact]
        public void Evaluator_NotCondition()
        {
            // not (fake-property: value) → true (we don't support it, so not = true)
            Assert.True(SupportsEvaluator.Evaluate("not (fake-property: value)"));
            // not (display: block) → false (we support display)
            Assert.False(SupportsEvaluator.Evaluate("not (display: block)"));
        }

        [Fact]
        public void Evaluator_AndCondition()
        {
            Assert.True(SupportsEvaluator.Evaluate("(display: block) and (width: 100px)"));
            Assert.False(SupportsEvaluator.Evaluate("(display: block) and (fake: value)"));
        }

        [Fact]
        public void Evaluator_OrCondition()
        {
            Assert.True(SupportsEvaluator.Evaluate("(display: block) or (fake: value)"));
            Assert.True(SupportsEvaluator.Evaluate("(fake: value) or (display: block)"));
            Assert.False(SupportsEvaluator.Evaluate("(fake1: x) or (fake2: y)"));
        }

        [Fact]
        public void Evaluator_EmptyCondition()
        {
            Assert.False(SupportsEvaluator.Evaluate(""));
            Assert.False(SupportsEvaluator.Evaluate("  "));
        }

        // ═══════════════════════════════════════════
        // Parsing @supports rules
        // ═══════════════════════════════════════════

        [Fact]
        public void Parser_SupportsRule_Parsed()
        {
            var sheet = CssParser.Parse("@supports (display: flex) { p { color: red; } }");

            Assert.True(sheet.Rules.Count > 0);
            var rule = sheet.Rules[0] as SupportsRule;
            Assert.NotNull(rule);
            Assert.Equal("(display: flex)", rule!.ConditionText);
            Assert.Equal(1, rule.Rules.Count);
        }

        [Fact]
        public void Parser_SupportsRule_NestedStyleRule()
        {
            var sheet = CssParser.Parse("@supports (display: flex) { .flex { display: flex; } }");

            var supportsRule = sheet.Rules[0] as SupportsRule;
            Assert.NotNull(supportsRule);

            var styleRule = supportsRule!.Rules[0] as StyleRule;
            Assert.NotNull(styleRule);
            Assert.Equal(".flex", styleRule!.SelectorText);
            Assert.True(styleRule.Declarations.Count > 0);
        }

        // ═══════════════════════════════════════════
        // @supports in style resolution
        // ═══════════════════════════════════════════

        [Fact]
        public void Resolve_SupportsTrue_RulesApplied()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("@supports (display: block) { p { width: 200px; } }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var style = resolver.Resolve(element);

            Assert.Equal(200f, style.Width, 0.01);
        }

        [Fact]
        public void Resolve_SupportsFalse_RulesSkipped()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("@supports (fake-property: value) { p { width: 200px; } }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var style = resolver.Resolve(element);

            // Should not apply — width remains auto (NaN)
            Assert.True(float.IsNaN(style.Width));
        }

        [Fact]
        public void Resolve_SupportsNotFalse_RulesApplied()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("@supports not (fake-property: x) { p { width: 300px; } }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var style = resolver.Resolve(element);

            Assert.Equal(300f, style.Width, 0.01);
        }

        [Fact]
        public void Resolve_SupportsAndBothTrue_RulesApplied()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("@supports (display: flex) and (width: 0) { p { width: 400px; } }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var style = resolver.Resolve(element);

            Assert.Equal(400f, style.Width, 0.01);
        }
    }
}
