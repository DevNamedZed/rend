using Xunit;

namespace Rend.Css.Tests
{
    public class MiscPropertyTests
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

        // Hyphens
        [Fact]
        public void Hyphens_Default_IsManual()
        {
            var style = ResolveElement("");
            Assert.Equal(CssHyphens.Manual, style.Hyphens);
        }

        [Fact]
        public void Hyphens_Auto()
        {
            var style = ResolveElement("div { hyphens: auto; }");
            Assert.Equal(CssHyphens.Auto, style.Hyphens);
        }

        [Fact]
        public void Hyphens_None()
        {
            var style = ResolveElement("div { hyphens: none; }");
            Assert.Equal(CssHyphens.None, style.Hyphens);
        }

        [Fact]
        public void Hyphens_Inherited()
        {
            var parent = ResolveElement("div { hyphens: auto; }");
            var child = ResolveElement("", parent);
            Assert.Equal(CssHyphens.Auto, child.Hyphens);
        }

        // Text Rendering
        [Fact]
        public void TextRendering_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssTextRendering.Auto, style.TextRendering);
        }

        [Fact]
        public void TextRendering_OptimizeLegibility()
        {
            var style = ResolveElement("div { text-rendering: optimizelegibility; }");
            Assert.Equal(CssTextRendering.OptimizeLegibility, style.TextRendering);
        }

        [Fact]
        public void TextRendering_GeometricPrecision()
        {
            var style = ResolveElement("div { text-rendering: geometricprecision; }");
            Assert.Equal(CssTextRendering.GeometricPrecision, style.TextRendering);
        }

        // Image Rendering
        [Fact]
        public void ImageRendering_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssImageRendering.Auto, style.ImageRendering);
        }

        [Fact]
        public void ImageRendering_CrispEdges()
        {
            var style = ResolveElement("div { image-rendering: crisp-edges; }");
            Assert.Equal(CssImageRendering.CrispEdges, style.ImageRendering);
        }

        [Fact]
        public void ImageRendering_Pixelated()
        {
            var style = ResolveElement("div { image-rendering: pixelated; }");
            Assert.Equal(CssImageRendering.Pixelated, style.ImageRendering);
        }

        // Contain
        [Fact]
        public void Contain_Default_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssContain.None, style.Contain);
        }

        [Fact]
        public void Contain_Strict()
        {
            var style = ResolveElement("div { contain: strict; }");
            Assert.Equal(CssContain.Strict, style.Contain);
        }

        [Fact]
        public void Contain_Content()
        {
            var style = ResolveElement("div { contain: content; }");
            Assert.Equal(CssContain.Content, style.Contain);
        }

        [Fact]
        public void Contain_Paint()
        {
            var style = ResolveElement("div { contain: paint; }");
            Assert.Equal(CssContain.Paint, style.Contain);
        }
        // Resize
        [Fact]
        public void Resize_Default_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssResize.None, style.Resize);
        }

        [Fact]
        public void Resize_Both()
        {
            var style = ResolveElement("div { resize: both; }");
            Assert.Equal(CssResize.Both, style.Resize);
        }

        // Appearance
        [Fact]
        public void Appearance_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssAppearance.Auto, style.Appearance);
        }

        [Fact]
        public void Appearance_None()
        {
            var style = ResolveElement("div { appearance: none; }");
            Assert.Equal(CssAppearance.None, style.Appearance);
        }

        // User-Select
        [Fact]
        public void UserSelect_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssUserSelect.Auto, style.UserSelect);
        }

        [Fact]
        public void UserSelect_None()
        {
            var style = ResolveElement("div { user-select: none; }");
            Assert.Equal(CssUserSelect.None, style.UserSelect);
        }

        // Isolation
        [Fact]
        public void Isolation_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssIsolation.Auto, style.Isolation);
        }

        [Fact]
        public void Isolation_Isolate()
        {
            var style = ResolveElement("div { isolation: isolate; }");
            Assert.Equal(CssIsolation.Isolate, style.Isolation);
        }

        // Mix-Blend-Mode
        [Fact]
        public void MixBlendMode_Default_IsNormal()
        {
            var style = ResolveElement("");
            Assert.Equal(CssMixBlendMode.Normal, style.MixBlendMode);
        }

        [Fact]
        public void MixBlendMode_Multiply()
        {
            var style = ResolveElement("div { mix-blend-mode: multiply; }");
            Assert.Equal(CssMixBlendMode.Multiply, style.MixBlendMode);
        }

        [Fact]
        public void MixBlendMode_Overlay()
        {
            var style = ResolveElement("div { mix-blend-mode: overlay; }");
            Assert.Equal(CssMixBlendMode.Overlay, style.MixBlendMode);
        }

        [Fact]
        public void MixBlendMode_ColorDodge()
        {
            var style = ResolveElement("div { mix-blend-mode: color-dodge; }");
            Assert.Equal(CssMixBlendMode.ColorDodge, style.MixBlendMode);
        }
    }
}
