using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class BackgroundClipOriginTests
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

        // ═══════════════════════════════════════════
        // background-clip
        // ═══════════════════════════════════════════

        [Fact]
        public void BackgroundClip_Default_IsBorderBox()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBackgroundClip.BorderBox, style.BackgroundClip);
        }

        [Fact]
        public void BackgroundClip_PaddingBox()
        {
            var style = ResolveElement("div { background-clip: padding-box; }");
            Assert.Equal(CssBackgroundClip.PaddingBox, style.BackgroundClip);
        }

        [Fact]
        public void BackgroundClip_ContentBox()
        {
            var style = ResolveElement("div { background-clip: content-box; }");
            Assert.Equal(CssBackgroundClip.ContentBox, style.BackgroundClip);
        }

        [Fact]
        public void BackgroundClip_BorderBox_Explicit()
        {
            var style = ResolveElement("div { background-clip: border-box; }");
            Assert.Equal(CssBackgroundClip.BorderBox, style.BackgroundClip);
        }

        // ═══════════════════════════════════════════
        // background-origin
        // ═══════════════════════════════════════════

        [Fact]
        public void BackgroundOrigin_Default_IsPaddingBox()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBackgroundOrigin.PaddingBox, style.BackgroundOrigin);
        }

        [Fact]
        public void BackgroundOrigin_BorderBox()
        {
            var style = ResolveElement("div { background-origin: border-box; }");
            Assert.Equal(CssBackgroundOrigin.BorderBox, style.BackgroundOrigin);
        }

        [Fact]
        public void BackgroundOrigin_ContentBox()
        {
            var style = ResolveElement("div { background-origin: content-box; }");
            Assert.Equal(CssBackgroundOrigin.ContentBox, style.BackgroundOrigin);
        }

        [Fact]
        public void BackgroundOrigin_PaddingBox_Explicit()
        {
            var style = ResolveElement("div { background-origin: padding-box; }");
            Assert.Equal(CssBackgroundOrigin.PaddingBox, style.BackgroundOrigin);
        }
    }
}
