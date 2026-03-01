using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class ObjectFitTests
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

            var element = new MockStylableElement { TagName = "img" };
            return resolver.Resolve(element);
        }

        [Fact]
        public void ObjectFit_Default_IsFill()
        {
            var style = ResolveElement("");
            Assert.Equal(CssObjectFit.Fill, style.ObjectFit);
        }

        [Fact]
        public void ObjectFit_Contain()
        {
            var style = ResolveElement("img { object-fit: contain; }");
            Assert.Equal(CssObjectFit.Contain, style.ObjectFit);
        }

        [Fact]
        public void ObjectFit_Cover()
        {
            var style = ResolveElement("img { object-fit: cover; }");
            Assert.Equal(CssObjectFit.Cover, style.ObjectFit);
        }

        [Fact]
        public void ObjectFit_None()
        {
            var style = ResolveElement("img { object-fit: none; }");
            Assert.Equal(CssObjectFit.None, style.ObjectFit);
        }

        [Fact]
        public void ObjectFit_ScaleDown()
        {
            var style = ResolveElement("img { object-fit: scale-down; }");
            Assert.Equal(CssObjectFit.ScaleDown, style.ObjectFit);
        }

        [Fact]
        public void ObjectFit_Fill_Explicit()
        {
            var style = ResolveElement("img { object-fit: fill; }");
            Assert.Equal(CssObjectFit.Fill, style.ObjectFit);
        }
    }
}
