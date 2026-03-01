using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Cascade.Internal;
using Xunit;

namespace Rend.Css.Tests
{
    public class PseudoElementTests
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
        // ExtractPseudoElement
        // ═══════════════════════════════════════════

        [Theory]
        [InlineData("p::before", "before")]
        [InlineData("p::after", "after")]
        [InlineData("div.cls::before", "before")]
        [InlineData("#id::after", "after")]
        [InlineData("p::first-letter", "first-letter")]
        [InlineData("p::first-line", "first-line")]
        public void ExtractPseudoElement_Recognized(string selector, string expected)
        {
            var result = CascadeCollector.ExtractPseudoElement(selector);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("p")]
        [InlineData("div.cls")]
        [InlineData("p:hover")]
        [InlineData("p::placeholder")]
        public void ExtractPseudoElement_ReturnsNull(string selector)
        {
            var result = CascadeCollector.ExtractPseudoElement(selector);
            Assert.Null(result);
        }

        // ═══════════════════════════════════════════
        // Pseudo-element style resolution
        // ═══════════════════════════════════════════

        [Fact]
        public void ResolvePseudoElement_Before_ReturnsStyle()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("p::before { content: \"Hello\"; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var beforeStyle = resolver.ResolvePseudoElement(element, "before", elementStyle);

            Assert.NotNull(beforeStyle);
            Assert.Equal("Hello", beforeStyle!.Content);
        }

        [Fact]
        public void ResolvePseudoElement_After_ReturnsStyle()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("p::after { content: \"World\"; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var afterStyle = resolver.ResolvePseudoElement(element, "after", elementStyle);

            Assert.NotNull(afterStyle);
            Assert.Equal("World", afterStyle!.Content);
        }

        [Fact]
        public void ResolvePseudoElement_NoRules_ReturnsNull()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("p { color: red; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var beforeStyle = resolver.ResolvePseudoElement(element, "before", elementStyle);

            Assert.Null(beforeStyle);
        }

        [Fact]
        public void ResolvePseudoElement_NoContent_ReturnsNull()
        {
            var resolver = CreateResolver();
            // Has ::before rule but no content property
            var sheet = CssParser.Parse("p::before { color: red; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var beforeStyle = resolver.ResolvePseudoElement(element, "before", elementStyle);

            Assert.Null(beforeStyle);
        }

        [Fact]
        public void ResolvePseudoElement_ContentNone_ReturnsStyleWithNone()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("p::before { content: none; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var beforeStyle = resolver.ResolvePseudoElement(element, "before", elementStyle);

            // content: none is technically resolved, but content value should be "none"
            Assert.NotNull(beforeStyle);
        }

        [Fact]
        public void ResolvePseudoElement_InheritsFromElement()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("p { font-size: 24px; } p::before { content: \"X\"; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var beforeStyle = resolver.ResolvePseudoElement(element, "before", elementStyle);

            Assert.NotNull(beforeStyle);
            // font-size is inherited, so ::before should inherit from the element
            Assert.Equal(24f, beforeStyle!.FontSize, 0.01);
        }

        [Fact]
        public void ResolvePseudoElement_OwnStyleOverridesInherited()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("p { font-size: 24px; } p::before { content: \"X\"; font-size: 12px; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var beforeStyle = resolver.ResolvePseudoElement(element, "before", elementStyle);

            Assert.NotNull(beforeStyle);
            // ::before has its own font-size which overrides inheritance
            Assert.Equal(12f, beforeStyle!.FontSize, 0.01);
        }

        [Fact]
        public void ResolvePseudoElement_DoesNotAffectElementStyle()
        {
            var resolver = CreateResolver();
            // The ::before rule should not leak into the element's style
            var sheet = CssParser.Parse("p::before { content: \"X\"; width: 100px; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);

            // Element's width should not be affected
            Assert.True(float.IsNaN(elementStyle.Width)); // auto (NaN)
        }

        [Fact]
        public void ResolvePseudoElement_DifferentTagNoMatch()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("p::before { content: \"X\"; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var elementStyle = resolver.Resolve(element);
            var beforeStyle = resolver.ResolvePseudoElement(element, "before", elementStyle);

            Assert.Null(beforeStyle);
        }

        [Fact]
        public void ResolvePseudoElement_BeforeAndAfterBothResolved()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse(
                "p::before { content: \"Start\"; } p::after { content: \"End\"; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var beforeStyle = resolver.ResolvePseudoElement(element, "before", elementStyle);
            var afterStyle = resolver.ResolvePseudoElement(element, "after", elementStyle);

            Assert.NotNull(beforeStyle);
            Assert.Equal("Start", beforeStyle!.Content);
            Assert.NotNull(afterStyle);
            Assert.Equal("End", afterStyle!.Content);
        }

        // ═══════════════════════════════════════════
        // ::first-letter
        // ═══════════════════════════════════════════

        [Fact]
        public void ResolvePseudoElement_FirstLetter_ReturnsStyle()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("p::first-letter { font-size: 48px; color: red; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var firstLetterStyle = resolver.ResolvePseudoElement(element, "first-letter", elementStyle);

            Assert.NotNull(firstLetterStyle);
            Assert.Equal(48f, firstLetterStyle!.FontSize, 0.01);
        }

        [Fact]
        public void ResolvePseudoElement_FirstLetter_NoContentRequired()
        {
            var resolver = CreateResolver();
            // ::first-letter doesn't need content property
            var sheet = CssParser.Parse("p::first-letter { font-size: 32px; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var firstLetterStyle = resolver.ResolvePseudoElement(element, "first-letter", elementStyle);

            Assert.NotNull(firstLetterStyle);
        }

        [Fact]
        public void ResolvePseudoElement_FirstLetter_NoRules_ReturnsNull()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("p { color: red; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var firstLetterStyle = resolver.ResolvePseudoElement(element, "first-letter", elementStyle);

            Assert.Null(firstLetterStyle);
        }

        [Fact]
        public void ResolvePseudoElement_FirstLetter_InheritsFromElement()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("p { font-size: 20px; } p::first-letter { color: blue; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var firstLetterStyle = resolver.ResolvePseudoElement(element, "first-letter", elementStyle);

            Assert.NotNull(firstLetterStyle);
            // font-size inherited from element
            Assert.Equal(20f, firstLetterStyle!.FontSize, 0.01);
        }

        // ═══════════════════════════════════════════
        // ::first-line
        // ═══════════════════════════════════════════

        [Fact]
        public void ResolvePseudoElement_FirstLine_ReturnsStyle()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("p::first-line { font-weight: bold; color: green; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var firstLineStyle = resolver.ResolvePseudoElement(element, "first-line", elementStyle);

            Assert.NotNull(firstLineStyle);
            Assert.Equal(700f, firstLineStyle!.FontWeight, 0.01);
        }

        [Fact]
        public void ResolvePseudoElement_FirstLine_NoContentRequired()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("p::first-line { color: red; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var firstLineStyle = resolver.ResolvePseudoElement(element, "first-line", elementStyle);

            Assert.NotNull(firstLineStyle);
        }

        [Fact]
        public void ResolvePseudoElement_FirstLine_DifferentTag_ReturnsNull()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("p::first-line { color: red; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var elementStyle = resolver.Resolve(element);
            var firstLineStyle = resolver.ResolvePseudoElement(element, "first-line", elementStyle);

            Assert.Null(firstLineStyle);
        }

        [Fact]
        public void ResolvePseudoElement_FirstLetter_BeforeStillRequiresContent()
        {
            var resolver = CreateResolver();
            // ::before still requires content, while ::first-letter doesn't
            var sheet = CssParser.Parse("p::before { color: red; } p::first-letter { font-size: 40px; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "p" };
            var elementStyle = resolver.Resolve(element);
            var beforeStyle = resolver.ResolvePseudoElement(element, "before", elementStyle);
            var firstLetterStyle = resolver.ResolvePseudoElement(element, "first-letter", elementStyle);

            Assert.Null(beforeStyle); // No content property
            Assert.NotNull(firstLetterStyle); // Doesn't need content
        }
    }
}
