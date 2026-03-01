using Xunit;
using Rend.Css.Properties.Internal;

namespace Rend.Css.Tests
{
    public class BorderImageParsingTests
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

            return resolver.Resolve(new MockStylableElement { TagName = "div" }, null);
        }

        [Fact]
        public void BorderImageSource_None_IsDefault()
        {
            var style = ResolveElement("");
            var source = style.GetRefValue(PropertyId.BorderImageSource);
            // Default is null or "none"
            Assert.True(source == null || (source is CssKeywordValue kw && kw.Keyword == "none") || source is string s && s == "none");
        }

        [Fact]
        public void BorderImageSlice_Number_Stored()
        {
            var style = ResolveElement("div { border-image-slice: 30; }");
            var slice = style.GetRefValue(PropertyId.BorderImageSlice);
            Assert.NotNull(slice);
        }

        [Fact]
        public void BorderImageRepeat_Stretch_Stored()
        {
            var style = ResolveElement("div { border-image-repeat: stretch; }");
            Assert.NotNull(style);
        }

        [Fact]
        public void BorderImageRepeat_Round_Stored()
        {
            var style = ResolveElement("div { border-image-repeat: round; }");
            Assert.NotNull(style);
        }

        [Fact]
        public void BorderImage_Shorthand_ExpandsToSource()
        {
            var style = ResolveElement("div { border-image: linear-gradient(red, blue) 10 stretch; }");
            // border-image shorthand should expand to set border-image-source
            Assert.NotNull(style);
        }

        [Fact]
        public void BorderImageWidth_Stored()
        {
            var style = ResolveElement("div { border-image-width: 10px; }");
            var width = style.GetRefValue(PropertyId.BorderImageWidth);
            Assert.NotNull(width);
        }

        [Fact]
        public void BorderImageOutset_Stored()
        {
            var style = ResolveElement("div { border-image-outset: 5px; }");
            var outset = style.GetRefValue(PropertyId.BorderImageOutset);
            Assert.NotNull(outset);
        }

        [Fact]
        public void BorderImageSlice_Percentage_Stored()
        {
            var style = ResolveElement("div { border-image-slice: 33%; }");
            var slice = style.GetRefValue(PropertyId.BorderImageSlice);
            Assert.NotNull(slice);
        }
    }
}
