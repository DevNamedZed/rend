using Xunit;

namespace Rend.Css.Tests
{
    public class BreakPropertyTests
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

            return resolver.Resolve(new MockStylableElement { TagName = "div" });
        }

        [Fact]
        public void BreakBefore_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBreakValue.Auto, style.BreakBefore);
        }

        [Fact]
        public void BreakAfter_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBreakValue.Auto, style.BreakAfter);
        }

        [Fact]
        public void BreakInside_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBreakValue.Auto, style.BreakInside);
        }

        [Fact]
        public void BreakBefore_Page()
        {
            var style = ResolveElement("div { break-before: page; }");
            Assert.Equal(CssBreakValue.Page, style.BreakBefore);
        }

        [Fact]
        public void BreakAfter_Column()
        {
            var style = ResolveElement("div { break-after: column; }");
            Assert.Equal(CssBreakValue.Column, style.BreakAfter);
        }

        [Fact]
        public void BreakInside_Avoid()
        {
            var style = ResolveElement("div { break-inside: avoid; }");
            Assert.Equal(CssBreakValue.Avoid, style.BreakInside);
        }

        [Fact]
        public void BreakInside_AvoidColumn()
        {
            var style = ResolveElement("div { break-inside: avoid-column; }");
            Assert.Equal(CssBreakValue.AvoidColumn, style.BreakInside);
        }
    }
}
