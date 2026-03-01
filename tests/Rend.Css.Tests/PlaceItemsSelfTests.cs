using Xunit;

namespace Rend.Css.Tests
{
    public class PlaceItemsSelfTests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private StyleResolver CreateResolver()
        {
            return new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 800,
                ViewportHeight = 600
            });
        }

        private ComputedStyle ResolveElement(string css, string tagName = "div",
            ComputedStyle? parentStyle = null)
        {
            var resolver = CreateResolver();
            if (!string.IsNullOrEmpty(css))
            {
                var sheet = CssParser.Parse(css);
                resolver.AddStylesheet(sheet);
            }

            var element = new MockStylableElement { TagName = tagName };
            return resolver.Resolve(element, parentStyle);
        }

        [Fact]
        public void JustifyItems_Default_IsStretch()
        {
            var style = ResolveElement("");
            Assert.Equal(CssAlignItems.Stretch, style.JustifyItems);
        }

        [Fact]
        public void JustifySelf_Default_IsAuto()
        {
            var style = ResolveElement("");
            // 255 sentinel for auto
            Assert.Equal((CssAlignItems)255, style.JustifySelf);
        }

        [Fact]
        public void JustifyItems_Center()
        {
            var style = ResolveElement("div { justify-items: center; }");
            Assert.Equal(CssAlignItems.Center, style.JustifyItems);
        }

        [Fact]
        public void JustifySelf_FlexEnd()
        {
            var style = ResolveElement("div { justify-self: flex-end; }");
            Assert.Equal(CssAlignItems.FlexEnd, style.JustifySelf);
        }

        [Fact]
        public void PlaceItems_SingleValue()
        {
            var style = ResolveElement("div { place-items: center; }");
            Assert.Equal(CssAlignItems.Center, style.AlignItems);
            Assert.Equal(CssAlignItems.Center, style.JustifyItems);
        }

        [Fact]
        public void PlaceItems_TwoValues()
        {
            var style = ResolveElement("div { place-items: center flex-start; }");
            Assert.Equal(CssAlignItems.Center, style.AlignItems);
            Assert.Equal(CssAlignItems.FlexStart, style.JustifyItems);
        }

        [Fact]
        public void PlaceSelf_SingleValue()
        {
            var style = ResolveElement("div { place-self: center; }");
            Assert.Equal(CssAlignItems.Center, style.AlignSelf);
            Assert.Equal(CssAlignItems.Center, style.JustifySelf);
        }

        [Fact]
        public void PlaceSelf_TwoValues()
        {
            var style = ResolveElement("div { place-self: flex-end stretch; }");
            Assert.Equal(CssAlignItems.FlexEnd, style.AlignSelf);
            Assert.Equal(CssAlignItems.Stretch, style.JustifySelf);
        }
    }
}
