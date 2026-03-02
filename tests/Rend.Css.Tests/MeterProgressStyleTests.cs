using Xunit;

namespace Rend.Css.Tests
{
    public class MeterProgressStyleTests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private StyleResolver CreateResolverWithUa()
        {
            return new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = true,
                DefaultFontSize = 16,
                ViewportWidth = 1920,
                ViewportHeight = 1080
            });
        }

        [Fact]
        public void Meter_UaStylesheet_DisplayInlineBlock()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "meter" };

            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.InlineBlock, style.Display);
        }

        [Fact]
        public void Progress_UaStylesheet_DisplayInlineBlock()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "progress" };

            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.InlineBlock, style.Display);
        }
    }
}
