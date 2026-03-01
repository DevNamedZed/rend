using Xunit;

namespace Rend.Css.Tests
{
    public class BackgroundAttachmentTests
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

            return resolver.Resolve(new MockStylableElement { TagName = "div" });
        }

        [Fact]
        public void Default_IsScroll()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBackgroundAttachment.Scroll, style.BackgroundAttachment);
        }

        [Fact]
        public void Fixed()
        {
            var style = ResolveElement("div { background-attachment: fixed; }");
            Assert.Equal(CssBackgroundAttachment.Fixed, style.BackgroundAttachment);
        }

        [Fact]
        public void Local()
        {
            var style = ResolveElement("div { background-attachment: local; }");
            Assert.Equal(CssBackgroundAttachment.Local, style.BackgroundAttachment);
        }
    }
}
