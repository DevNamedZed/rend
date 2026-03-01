using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class TextDecorationStyleTests
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
        public void TextDecorationStyle_Default_IsSolid()
        {
            var style = ResolveElement("");
            Assert.Equal(CssTextDecorationStyle.Solid, style.TextDecorationStyle);
        }

        [Fact]
        public void TextDecorationStyle_Dashed()
        {
            var style = ResolveElement("div { text-decoration-style: dashed; }");
            Assert.Equal(CssTextDecorationStyle.Dashed, style.TextDecorationStyle);
        }

        [Fact]
        public void TextDecorationStyle_Dotted()
        {
            var style = ResolveElement("div { text-decoration-style: dotted; }");
            Assert.Equal(CssTextDecorationStyle.Dotted, style.TextDecorationStyle);
        }

        [Fact]
        public void TextDecorationStyle_Double()
        {
            var style = ResolveElement("div { text-decoration-style: double; }");
            Assert.Equal(CssTextDecorationStyle.Double, style.TextDecorationStyle);
        }

        [Fact]
        public void TextDecorationStyle_Wavy()
        {
            var style = ResolveElement("div { text-decoration-style: wavy; }");
            Assert.Equal(CssTextDecorationStyle.Wavy, style.TextDecorationStyle);
        }

        [Fact]
        public void TextDecorationColor_Default_IsCurrentColor()
        {
            var style = ResolveElement("div { color: green; }");
            var c = style.TextDecorationColor;
            // currentColor resolves to element's color
            Assert.Equal(0, c.R);
            Assert.Equal(128, c.G);
            Assert.Equal(0, c.B);
        }

        [Fact]
        public void TextDecorationColor_Explicit()
        {
            var style = ResolveElement("div { text-decoration-color: red; }");
            Assert.Equal(255, style.TextDecorationColor.R);
            Assert.Equal(0, style.TextDecorationColor.G);
            Assert.Equal(0, style.TextDecorationColor.B);
        }

        [Fact]
        public void TextDecoration_Shorthand_IncludesStyle()
        {
            var style = ResolveElement("div { text-decoration: underline dashed red; }");
            Assert.Equal(CssTextDecorationLine.Underline, style.TextDecorationLine);
            Assert.Equal(CssTextDecorationStyle.Dashed, style.TextDecorationStyle);
            Assert.Equal(255, style.TextDecorationColor.R);
        }
    }
}
