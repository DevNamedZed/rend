using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class OutlineStyleTests
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

        [Fact]
        public void OutlineStyle_Default_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBorderStyle.None, style.OutlineStyle);
        }

        [Fact]
        public void OutlineStyle_Solid()
        {
            var style = ResolveElement("div { outline-style: solid; }");
            Assert.Equal(CssBorderStyle.Solid, style.OutlineStyle);
        }

        [Fact]
        public void OutlineStyle_Dashed()
        {
            var style = ResolveElement("div { outline-style: dashed; }");
            Assert.Equal(CssBorderStyle.Dashed, style.OutlineStyle);
        }

        [Fact]
        public void OutlineWidth_Default_IsMedium()
        {
            var style = ResolveElement("");
            Assert.Equal(3f, style.OutlineWidth, 0.01);
        }

        [Fact]
        public void OutlineWidth_Set()
        {
            var style = ResolveElement("div { outline-width: 5px; }");
            Assert.Equal(5f, style.OutlineWidth, 0.01);
        }

        [Fact]
        public void OutlineOffset_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.OutlineOffset, 0.01);
        }

        [Fact]
        public void OutlineOffset_Set()
        {
            var style = ResolveElement("div { outline-offset: 10px; }");
            Assert.Equal(10f, style.OutlineOffset, 0.01);
        }

        [Fact]
        public void OutlineColor_Default_IsCurrentColor()
        {
            var style = ResolveElement("div { color: red; }");
            // Default outline-color is currentColor → resolves to element's color
            var c = style.OutlineColor;
            Assert.Equal(255, c.R);
            Assert.Equal(0, c.G);
            Assert.Equal(0, c.B);
        }

        [Fact]
        public void OutlineColor_Explicit()
        {
            var style = ResolveElement("div { outline-color: blue; }");
            var c = style.OutlineColor;
            Assert.Equal(0, c.R);
            Assert.Equal(0, c.G);
            Assert.Equal(255, c.B);
        }

        [Fact]
        public void Outline_Shorthand()
        {
            var style = ResolveElement("div { outline: 2px solid red; }");
            Assert.Equal(2f, style.OutlineWidth, 0.01);
            Assert.Equal(CssBorderStyle.Solid, style.OutlineStyle);
            Assert.Equal(255, style.OutlineColor.R);
        }
    }
}
