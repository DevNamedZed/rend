using Xunit;

namespace Rend.Css.Tests
{
    public class BidirectionalTests
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

        #region UA Stylesheet: bdo/bdi

        [Fact]
        public void Bdo_UaStylesheet_SetsBidiOverride()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "bdo" };

            var style = resolver.Resolve(element);

            Assert.Equal(CssUnicodeBidi.BidiOverride, style.UnicodeBidi);
        }

        [Fact]
        public void Bdi_UaStylesheet_SetsIsolate()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "bdi" };

            var style = resolver.Resolve(element);

            Assert.Equal(CssUnicodeBidi.Isolate, style.UnicodeBidi);
        }

        [Fact]
        public void Div_UaStylesheet_DefaultUnicodeBidiNormal()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement { TagName = "div" };

            var style = resolver.Resolve(element);

            Assert.Equal(CssUnicodeBidi.Normal, style.UnicodeBidi);
        }

        #endregion

        #region CSS Parsing: unicode-bidi values

        [Theory]
        [InlineData("normal", CssUnicodeBidi.Normal)]
        [InlineData("embed", CssUnicodeBidi.Embed)]
        [InlineData("isolate", CssUnicodeBidi.Isolate)]
        [InlineData("bidi-override", CssUnicodeBidi.BidiOverride)]
        [InlineData("isolate-override", CssUnicodeBidi.IsolateOverride)]
        [InlineData("plaintext", CssUnicodeBidi.Plaintext)]
        public void UnicodeBidi_ParsesAllValues(string cssValue, CssUnicodeBidi expected)
        {
            var resolver = CreateResolverWithoutUa();
            var sheet = CssParser.Parse($"div {{ unicode-bidi: {cssValue}; }}");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(expected, style.UnicodeBidi);
        }

        #endregion

        #region CSS Parsing: direction values

        [Theory]
        [InlineData("ltr", CssDirection.Ltr)]
        [InlineData("rtl", CssDirection.Rtl)]
        public void Direction_ParsesValues(string cssValue, CssDirection expected)
        {
            var resolver = CreateResolverWithoutUa();
            var sheet = CssParser.Parse($"div {{ direction: {cssValue}; }}");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(expected, style.Direction);
        }

        #endregion

        #region Author stylesheet overrides UA

        [Fact]
        public void AuthorStylesheet_OverridesBdoUnicodeBidi()
        {
            var resolver = CreateResolverWithUa();
            var sheet = CssParser.Parse("bdo { unicode-bidi: isolate; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "bdo" };
            var style = resolver.Resolve(element);

            // Author stylesheet should override UA
            Assert.Equal(CssUnicodeBidi.Isolate, style.UnicodeBidi);
        }

        [Fact]
        public void AuthorStylesheet_OverridesBdiUnicodeBidi()
        {
            var resolver = CreateResolverWithUa();
            var sheet = CssParser.Parse("bdi { unicode-bidi: embed; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "bdi" };
            var style = resolver.Resolve(element);

            Assert.Equal(CssUnicodeBidi.Embed, style.UnicodeBidi);
        }

        #endregion

        #region Inline style overrides

        [Fact]
        public void InlineStyle_OverridesBdoUnicodeBidi()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement
            {
                TagName = "bdo",
                InlineStyle = "unicode-bidi: plaintext;"
            };

            var style = resolver.Resolve(element);

            Assert.Equal(CssUnicodeBidi.Plaintext, style.UnicodeBidi);
        }

        [Fact]
        public void InlineStyle_DirectionRtl_WithBdo()
        {
            var resolver = CreateResolverWithUa();
            var element = new MockStylableElement
            {
                TagName = "bdo",
                InlineStyle = "direction: rtl;"
            };

            var style = resolver.Resolve(element);

            Assert.Equal(CssDirection.Rtl, style.Direction);
            Assert.Equal(CssUnicodeBidi.BidiOverride, style.UnicodeBidi);
        }

        #endregion
    }
}
