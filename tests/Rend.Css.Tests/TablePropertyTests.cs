using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class TablePropertyTests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private ComputedStyle ResolveElement(string css, string tagName = "table")
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

            var element = new MockStylableElement { TagName = tagName };
            return resolver.Resolve(element);
        }

        [Fact]
        public void BorderSpacing_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.BorderSpacing, 0.01);
        }

        [Fact]
        public void BorderSpacing_Set()
        {
            var style = ResolveElement("table { border-spacing: 10px; }");
            Assert.Equal(10f, style.BorderSpacing, 0.01);
        }

        [Fact]
        public void CaptionSide_Default_IsTop()
        {
            var style = ResolveElement("");
            Assert.Equal(CssCaptionSide.Top, style.CaptionSide);
        }

        [Fact]
        public void CaptionSide_Bottom()
        {
            var style = ResolveElement("table { caption-side: bottom; }");
            Assert.Equal(CssCaptionSide.Bottom, style.CaptionSide);
        }

        [Fact]
        public void EmptyCells_Default_IsShow()
        {
            var style = ResolveElement("");
            Assert.Equal(CssEmptyCells.Show, style.EmptyCells);
        }

        [Fact]
        public void EmptyCells_Hide()
        {
            var style = ResolveElement("table { empty-cells: hide; }");
            Assert.Equal(CssEmptyCells.Hide, style.EmptyCells);
        }

        [Fact]
        public void BorderCollapse_Separate()
        {
            var style = ResolveElement("table { border-collapse: separate; }");
            Assert.Equal(CssBorderCollapse.Separate, style.BorderCollapse);
        }

        [Fact]
        public void BorderCollapse_Collapse()
        {
            var style = ResolveElement("table { border-collapse: collapse; }");
            Assert.Equal(CssBorderCollapse.Collapse, style.BorderCollapse);
        }
    }
}
