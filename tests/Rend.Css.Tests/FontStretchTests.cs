using Xunit;

namespace Rend.Css.Tests
{
    public class FontStretchTests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private ComputedStyle ResolveElement(string css, ComputedStyle? parent = null)
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

            return resolver.Resolve(new MockStylableElement { TagName = "div" }, parent);
        }

        [Fact]
        public void Default_IsNormal()
        {
            var style = ResolveElement("");
            Assert.Equal(CssFontStretch.Normal, style.FontStretch);
        }

        [Fact]
        public void Condensed()
        {
            var style = ResolveElement("div { font-stretch: condensed; }");
            Assert.Equal(CssFontStretch.Condensed, style.FontStretch);
        }

        [Fact]
        public void Expanded()
        {
            var style = ResolveElement("div { font-stretch: expanded; }");
            Assert.Equal(CssFontStretch.Expanded, style.FontStretch);
        }

        [Fact]
        public void UltraCondensed()
        {
            var style = ResolveElement("div { font-stretch: ultra-condensed; }");
            Assert.Equal(CssFontStretch.UltraCondensed, style.FontStretch);
        }

        [Fact]
        public void Inherited()
        {
            var parent = ResolveElement("div { font-stretch: semi-expanded; }");
            var child = ResolveElement("", parent);
            Assert.Equal(CssFontStretch.SemiExpanded, child.FontStretch);
        }
    }
}
