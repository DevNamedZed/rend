using Xunit;

namespace Rend.Css.Tests
{
    public class ColumnSpanTests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private ComputedStyle ResolveElement(string css, string tagName = "div",
            ComputedStyle? parentStyle = null)
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
            return resolver.Resolve(element, parentStyle);
        }

        [Fact]
        public void ColumnSpan_Default_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssColumnSpan.None, style.ColumnSpan);
        }

        [Fact]
        public void ColumnSpan_All()
        {
            var style = ResolveElement("div { column-span: all; }");
            Assert.Equal(CssColumnSpan.All, style.ColumnSpan);
        }

        [Fact]
        public void ColumnSpan_None_Explicit()
        {
            var style = ResolveElement("div { column-span: none; }");
            Assert.Equal(CssColumnSpan.None, style.ColumnSpan);
        }
    }
}
