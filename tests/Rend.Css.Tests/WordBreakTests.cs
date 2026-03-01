using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class WordBreakTests
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
        public void WordBreak_Default_IsNormal()
        {
            var style = ResolveElement("");
            Assert.Equal(CssWordBreak.Normal, style.WordBreak);
        }

        [Fact]
        public void WordBreak_BreakAll()
        {
            var style = ResolveElement("div { word-break: break-all; }");
            Assert.Equal(CssWordBreak.BreakAll, style.WordBreak);
        }

        [Fact]
        public void WordBreak_KeepAll()
        {
            var style = ResolveElement("div { word-break: keep-all; }");
            Assert.Equal(CssWordBreak.KeepAll, style.WordBreak);
        }

        [Fact]
        public void WordBreak_BreakWord()
        {
            var style = ResolveElement("div { word-break: break-word; }");
            Assert.Equal(CssWordBreak.BreakWord, style.WordBreak);
        }

        [Fact]
        public void WordBreak_Inherited()
        {
            var resolver = new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 800,
                ViewportHeight = 600
            });

            resolver.AddStylesheet(CssParser.Parse("div { word-break: break-all; }"));

            var parent = new MockStylableElement { TagName = "div" };
            var parentStyle = resolver.Resolve(parent);

            var child = new MockStylableElement { TagName = "span" };
            var childStyle = resolver.Resolve(child, parentStyle);

            Assert.Equal(CssWordBreak.BreakAll, childStyle.WordBreak);
        }
    }
}
