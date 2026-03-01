using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class TabSizeTests
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

            var element = new MockStylableElement { TagName = "pre" };
            return resolver.Resolve(element);
        }

        [Fact]
        public void TabSize_Default_Is8()
        {
            var style = ResolveElement("");
            Assert.Equal(8f, style.TabSize);
        }

        [Fact]
        public void TabSize_CustomNumber()
        {
            var style = ResolveElement("pre { tab-size: 4; }");
            Assert.Equal(4f, style.TabSize);
        }

        [Fact]
        public void TabSize_Zero()
        {
            var style = ResolveElement("pre { tab-size: 0; }");
            Assert.Equal(0f, style.TabSize);
        }

        [Fact]
        public void TabSize_Inherited()
        {
            var resolver = new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 800,
                ViewportHeight = 600
            });

            resolver.AddStylesheet(CssParser.Parse("div { tab-size: 2; }"));

            var parent = new MockStylableElement { TagName = "div" };
            var parentStyle = resolver.Resolve(parent);

            var child = new MockStylableElement { TagName = "pre" };
            var childStyle = resolver.Resolve(child, parentStyle);

            Assert.Equal(2f, childStyle.TabSize);
        }
    }
}
