using Xunit;

namespace Rend.Css.Tests
{
    public class DialogStyleTests
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
        public void Dialog_UaStylesheet_DisplayBlock()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "dialog" };

            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Block, style.Display);
        }

        [Fact]
        public void Dialog_UaStylesheet_PositionAbsolute()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "dialog" };

            var style = resolver.Resolve(element);

            Assert.Equal(CssPosition.Absolute, style.Position);
        }

        [Fact]
        public void Dialog_UaStylesheet_HasPadding()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "dialog" };

            var style = resolver.Resolve(element);

            // 1em = 16px (default font size)
            Assert.Equal(16f, style.PaddingTop, 0.5f);
            Assert.Equal(16f, style.PaddingBottom, 0.5f);
            Assert.Equal(16f, style.PaddingLeft, 0.5f);
            Assert.Equal(16f, style.PaddingRight, 0.5f);
        }

        [Fact]
        public void Dialog_UaStylesheet_HasBorder()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "dialog" };

            var style = resolver.Resolve(element);

            Assert.Equal(CssBorderStyle.Solid, style.BorderTopStyle);
        }

        [Fact]
        public void Dialog_UaStylesheet_BackgroundWhite()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "dialog" };

            var style = resolver.Resolve(element);

            // White background
            Assert.Equal(255, style.BackgroundColor.R);
            Assert.Equal(255, style.BackgroundColor.G);
            Assert.Equal(255, style.BackgroundColor.B);
        }
    }
}
