using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class ListStylePositionTests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private ComputedStyle ResolveElement(string css, string tagName = "li")
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

            var element = new MockStylableElement { TagName = tagName };
            return resolver.Resolve(element);
        }

        [Fact]
        public void ListStylePosition_Default_IsOutside()
        {
            var style = ResolveElement("");
            Assert.Equal(CssListStylePosition.Outside, style.ListStylePosition);
        }

        [Fact]
        public void ListStylePosition_Inside()
        {
            var style = ResolveElement("li { list-style-position: inside; }");
            Assert.Equal(CssListStylePosition.Inside, style.ListStylePosition);
        }

        [Fact]
        public void ListStylePosition_Outside()
        {
            var style = ResolveElement("li { list-style-position: outside; }");
            Assert.Equal(CssListStylePosition.Outside, style.ListStylePosition);
        }

        [Fact]
        public void ListStyle_Shorthand_IncludesPosition()
        {
            var style = ResolveElement("li { list-style: decimal inside; }");
            Assert.Equal(CssListStyleType.Decimal, style.ListStyleType);
            Assert.Equal(CssListStylePosition.Inside, style.ListStylePosition);
        }
    }
}
