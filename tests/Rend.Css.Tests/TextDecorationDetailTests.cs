using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class TextDecorationDetailTests
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
        public void TextDecorationThickness_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.TextDecorationThickness, 0.01);
        }

        [Fact]
        public void TextDecorationThickness_Set_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-decoration-thickness: 3px; }");
            Assert.Equal(3f, style.TextDecorationThickness, 0.01);
        }

        [Fact]
        public void TextUnderlineOffset_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.TextUnderlineOffset, 0.01);
        }

        [Fact]
        public void TextUnderlineOffset_Set_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-underline-offset: 5px; }");
            Assert.Equal(5f, style.TextUnderlineOffset, 0.01);
        }

        [Fact]
        public void TextDecorationThickness_Em_ResolvesToPx()
        {
            // em resolves against inherited font-size (16px default) during resolution
            var style = ResolveElement("div { text-decoration-thickness: 0.5em; }");
            Assert.Equal(8f, style.TextDecorationThickness, 0.01);
        }

        [Fact]
        public void TextUnderlineOffset_Em_ResolvesToPx()
        {
            var style = ResolveElement("div { text-underline-offset: 0.25em; }");
            Assert.Equal(4f, style.TextUnderlineOffset, 0.01);
        }
    }
}
