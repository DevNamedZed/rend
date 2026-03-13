using System.Linq;
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

        // ═══════════════════════════════════════════
        // background shorthand with <box> value
        // ═══════════════════════════════════════════

        [Fact]
        public void Background_Shorthand_ContentBox_SetsClipAndOrigin()
        {
            var style = ResolveElement("div { background: #3498db content-box; }");
            Assert.Equal(CssBackgroundClip.ContentBox, style.BackgroundClip);
            Assert.Equal(CssBackgroundOrigin.ContentBox, style.BackgroundOrigin);
        }

        [Fact]
        public void Background_Shorthand_PaddingBox_SetsClipAndOrigin()
        {
            var style = ResolveElement("div { background: #e74c3c padding-box; }");
            Assert.Equal(CssBackgroundClip.PaddingBox, style.BackgroundClip);
            Assert.Equal(CssBackgroundOrigin.PaddingBox, style.BackgroundOrigin);
        }

        [Fact]
        public void Background_Shorthand_PaddingBox_WithColor()
        {
            // Matches the visual regression test: background:#e74c3c padding-box
            var style = ResolveElement("div { background: #e74c3c padding-box; }");
            Assert.Equal(CssBackgroundClip.PaddingBox, style.BackgroundClip);
            // Background color should be set
            Assert.Equal(231, style.BackgroundColor.R);
            Assert.Equal(76, style.BackgroundColor.G);
        }
    }
}
