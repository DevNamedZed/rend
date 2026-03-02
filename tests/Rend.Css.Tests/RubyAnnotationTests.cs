using Xunit;

namespace Rend.Css.Tests
{
    public class RubyAnnotationTests
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

        private StyleResolver CreateResolverWithoutUa()
        {
            return new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 1920,
                ViewportHeight = 1080
            });
        }

        #region UA Stylesheet: ruby elements

        [Fact]
        public void Ruby_UaStylesheet_DisplayRuby()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "ruby" };

            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Ruby, style.Display);
        }

        [Fact]
        public void Rt_UaStylesheet_DisplayRubyText()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "rt" };

            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.RubyText, style.Display);
        }

        [Fact]
        public void Rt_UaStylesheet_HalfFontSize()
        {
            var resolver = CreateResolverWithUa();
            // rt with a parent that has font-size 16px
            var parent = new MockStylableElement { TagName = "ruby" };
            var element = new MockStylableElement { TagName = "rt", Parent = parent };

            var style = resolver.Resolve(element);

            // 0.5em = half of parent font size (16 * 0.5 = 8)
            Assert.Equal(8f, style.FontSize, 0.5f);
        }

        [Fact]
        public void Rb_UaStylesheet_DisplayRubyBase()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "rb" };

            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.RubyBase, style.Display);
        }

        [Fact]
        public void Rtc_UaStylesheet_DisplayRubyTextContainer()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "rtc" };

            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.RubyTextContainer, style.Display);
        }

        [Fact]
        public void Rp_UaStylesheet_DisplayNone()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "rp" };

            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.None, style.Display);
        }

        #endregion

        #region CSS Parsing: display ruby values

        [Theory]
        [InlineData("ruby", CssDisplay.Ruby)]
        [InlineData("ruby-text", CssDisplay.RubyText)]
        [InlineData("ruby-base", CssDisplay.RubyBase)]
        [InlineData("ruby-text-container", CssDisplay.RubyTextContainer)]
        public void Display_ParsesRubyValues(string cssValue, CssDisplay expected)
        {
            var resolver = CreateResolverWithoutUa();
            var sheet = CssParser.Parse($"div {{ display: {cssValue}; }}");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(expected, style.Display);
        }

        #endregion

        #region CSS Parsing: ruby-position

        [Theory]
        [InlineData("over", CssRubyPosition.Over)]
        [InlineData("under", CssRubyPosition.Under)]
        public void RubyPosition_ParsesValues(string cssValue, CssRubyPosition expected)
        {
            var resolver = CreateResolverWithoutUa();
            var sheet = CssParser.Parse($"div {{ ruby-position: {cssValue}; }}");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(expected, style.RubyPosition);
        }

        #endregion

        #region CSS Parsing: ruby-align

        [Theory]
        [InlineData("space-around", CssRubyAlign.SpaceAround)]
        [InlineData("center", CssRubyAlign.Center)]
        [InlineData("space-between", CssRubyAlign.SpaceBetween)]
        [InlineData("start", CssRubyAlign.Start)]
        public void RubyAlign_ParsesValues(string cssValue, CssRubyAlign expected)
        {
            var resolver = CreateResolverWithoutUa();
            var sheet = CssParser.Parse($"div {{ ruby-align: {cssValue}; }}");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(expected, style.RubyAlign);
        }

        #endregion

        #region Parsing on specific elements

        [Fact]
        public void RubyPosition_OnRubyElement()
        {
            var resolver = CreateResolverWithoutUa();
            var sheet = CssParser.Parse("ruby { ruby-position: under; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "ruby" };
            var style = resolver.Resolve(element);

            Assert.Equal(CssRubyPosition.Under, style.RubyPosition);
        }

        [Fact]
        public void RubyAlign_OnRubyElement()
        {
            var resolver = CreateResolverWithoutUa();
            var sheet = CssParser.Parse("ruby { ruby-align: center; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "ruby" };
            var style = resolver.Resolve(element);

            Assert.Equal(CssRubyAlign.Center, style.RubyAlign);
        }

        [Fact]
        public void RubyPosition_DefaultIsOver()
        {
            var resolver = CreateResolverWithoutUa();
            var element = new MockStylableElement { TagName = "ruby" };
            var style = resolver.Resolve(element);

            Assert.Equal(CssRubyPosition.Over, style.RubyPosition);
        }

        [Fact]
        public void RubyAlign_DefaultIsSpaceAround()
        {
            var resolver = CreateResolverWithoutUa();
            var element = new MockStylableElement { TagName = "ruby" };
            var style = resolver.Resolve(element);

            Assert.Equal(CssRubyAlign.SpaceAround, style.RubyAlign);
        }

        #endregion
    }
}
