using Xunit;

namespace Rend.Css.Tests
{
    public class ColorLevel4Tests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private ComputedStyle ResolveElement(string css, string tagName = "div")
        {
            var resolver = new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 1920,
                ViewportHeight = 1080
            });
            if (!string.IsNullOrEmpty(css))
            {
                var sheet = CssParser.Parse(css);
                resolver.AddStylesheet(sheet);
            }
            var element = new MockStylableElement { TagName = tagName };
            return resolver.Resolve(element);
        }

        [Fact]
        public void Hwb_Red_ParsesCorrectly()
        {
            var style = ResolveElement("div { color: hwb(0 0% 0%); }");
            Assert.Equal(255, style.Color.R);
            Assert.Equal(0, style.Color.G);
            Assert.Equal(0, style.Color.B);
        }

        [Fact]
        public void Hwb_White_ParsesCorrectly()
        {
            var style = ResolveElement("div { color: hwb(0 100% 0%); }");
            Assert.Equal(255, style.Color.R);
            Assert.Equal(255, style.Color.G);
            Assert.Equal(255, style.Color.B);
        }

        [Fact]
        public void Hwb_WithAlpha_ParsesCorrectly()
        {
            var style = ResolveElement("div { color: hwb(120 0% 0% / 0.5); }");
            Assert.Equal(0, style.Color.R);
            Assert.Equal(255, style.Color.G);
            Assert.Equal(0, style.Color.B);
            Assert.Equal(128, style.Color.A);
        }

        [Fact]
        public void Lab_White_ParsesCorrectly()
        {
            var style = ResolveElement("div { color: lab(100 0 0); }");
            Assert.InRange(style.Color.R, 250, 255);
            Assert.InRange(style.Color.G, 250, 255);
            Assert.InRange(style.Color.B, 250, 255);
        }

        [Fact]
        public void Lab_Black_ParsesCorrectly()
        {
            var style = ResolveElement("div { color: lab(0 0 0); }");
            Assert.InRange(style.Color.R, 0, 5);
            Assert.InRange(style.Color.G, 0, 5);
            Assert.InRange(style.Color.B, 0, 5);
        }

        [Fact]
        public void Lch_ParsesCorrectly()
        {
            var style = ResolveElement("div { color: lch(50 100 40); }");
            // Should produce a reddish color
            Assert.True(style.Color.R > 100);
        }

        [Fact]
        public void Oklab_White_ParsesCorrectly()
        {
            var style = ResolveElement("div { color: oklab(1 0 0); }");
            Assert.InRange(style.Color.R, 250, 255);
            Assert.InRange(style.Color.G, 250, 255);
            Assert.InRange(style.Color.B, 250, 255);
        }

        [Fact]
        public void Oklab_Black_ParsesCorrectly()
        {
            var style = ResolveElement("div { color: oklab(0 0 0); }");
            Assert.Equal(0, style.Color.R);
            Assert.Equal(0, style.Color.G);
            Assert.Equal(0, style.Color.B);
        }

        [Fact]
        public void Oklch_ParsesCorrectly()
        {
            var style = ResolveElement("div { color: oklch(0.7 0.15 180); }");
            // Should produce a tealish color
            Assert.True(style.Color.G > 100);
        }

        [Fact]
        public void Oklch_WithAlpha_ParsesCorrectly()
        {
            var style = ResolveElement("div { color: oklch(1 0 0 / 0.5); }");
            Assert.InRange(style.Color.R, 250, 255);
            Assert.Equal(128, style.Color.A);
        }

        [Fact]
        public void Color_Srgb_ParsesCorrectly()
        {
            var style = ResolveElement("div { color: color(srgb 1 0 0); }");
            Assert.Equal(255, style.Color.R);
            Assert.Equal(0, style.Color.G);
            Assert.Equal(0, style.Color.B);
        }

        [Fact]
        public void Color_DisplayP3_ParsesCorrectly()
        {
            // display-p3 values get clamped to sRGB gamut
            var style = ResolveElement("div { color: color(display-p3 1 0 0); }");
            Assert.Equal(255, style.Color.R);
            Assert.Equal(0, style.Color.G);
            Assert.Equal(0, style.Color.B);
        }

        [Fact]
        public void Color_Srgb_WithAlpha_ParsesCorrectly()
        {
            var style = ResolveElement("div { color: color(srgb 0 1 0 / 0.5); }");
            Assert.Equal(0, style.Color.R);
            Assert.Equal(255, style.Color.G);
            Assert.Equal(0, style.Color.B);
            Assert.Equal(128, style.Color.A);
        }

        [Fact]
        public void Hwb_InBackgroundColor_ParsesCorrectly()
        {
            var style = ResolveElement("div { background-color: hwb(240 0% 0%); }");
            Assert.Equal(0, style.BackgroundColor.R);
            Assert.Equal(0, style.BackgroundColor.G);
            Assert.Equal(255, style.BackgroundColor.B);
        }

        [Fact]
        public void Lab_InBorderColor_ParsesCorrectly()
        {
            var style = ResolveElement("div { border-top-color: lab(100 0 0); }");
            Assert.InRange(style.BorderTopColor.R, 250, 255);
        }
    }
}
