using Xunit;

namespace Rend.Css.Tests
{
    public class NewPropertyKeywordTests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private ComputedStyle ResolveElement(string css, string tagName = "div",
            ComputedStyle? parentStyle = null)
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
            return resolver.Resolve(element, parentStyle);
        }

        // Column Fill
        [Fact]
        public void ColumnFill_Default_IsBalance()
        {
            var style = ResolveElement("");
            Assert.Equal(CssColumnFill.Balance, style.ColumnFill);
        }

        [Fact]
        public void ColumnFill_Auto()
        {
            var style = ResolveElement("div { column-fill: auto; }");
            Assert.Equal(CssColumnFill.Auto, style.ColumnFill);
        }

        [Fact]
        public void ColumnFill_Balance_Explicit()
        {
            var style = ResolveElement("div { column-fill: balance; }");
            Assert.Equal(CssColumnFill.Balance, style.ColumnFill);
        }

        // Writing Mode
        [Fact]
        public void WritingMode_Default_IsHorizontalTb()
        {
            var style = ResolveElement("");
            Assert.Equal(CssWritingMode.HorizontalTb, style.WritingMode);
        }

        [Fact]
        public void WritingMode_VerticalRl()
        {
            var style = ResolveElement("div { writing-mode: vertical-rl; }");
            Assert.Equal(CssWritingMode.VerticalRl, style.WritingMode);
        }

        [Fact]
        public void WritingMode_VerticalLr()
        {
            var style = ResolveElement("div { writing-mode: vertical-lr; }");
            Assert.Equal(CssWritingMode.VerticalLr, style.WritingMode);
        }

        [Fact]
        public void WritingMode_Inherited()
        {
            var parentStyle = ResolveElement("div { writing-mode: vertical-rl; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(CssWritingMode.VerticalRl, childStyle.WritingMode);
        }

        // Text Orientation
        [Fact]
        public void TextOrientation_Default_IsMixed()
        {
            var style = ResolveElement("");
            Assert.Equal(CssTextOrientation.Mixed, style.TextOrientation);
        }

        [Fact]
        public void TextOrientation_Upright()
        {
            var style = ResolveElement("div { text-orientation: upright; }");
            Assert.Equal(CssTextOrientation.Upright, style.TextOrientation);
        }

        [Fact]
        public void TextOrientation_Sideways()
        {
            var style = ResolveElement("div { text-orientation: sideways; }");
            Assert.Equal(CssTextOrientation.Sideways, style.TextOrientation);
        }

        [Fact]
        public void TextOrientation_Inherited()
        {
            var parentStyle = ResolveElement("div { text-orientation: upright; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(CssTextOrientation.Upright, childStyle.TextOrientation);
        }

        // Mask Mode
        [Fact]
        public void MaskMode_Default_IsMatchSource()
        {
            var style = ResolveElement("");
            Assert.Equal(CssMaskMode.MatchSource, style.MaskMode);
        }

        [Fact]
        public void MaskMode_Luminance()
        {
            var style = ResolveElement("div { mask-mode: luminance; }");
            Assert.Equal(CssMaskMode.Luminance, style.MaskMode);
        }

        [Fact]
        public void MaskMode_Alpha()
        {
            var style = ResolveElement("div { mask-mode: alpha; }");
            Assert.Equal(CssMaskMode.Alpha, style.MaskMode);
        }

        // Word-wrap alias
        [Fact]
        public void WordWrap_Alias_MapsToOverflowWrap()
        {
            var style = ResolveElement("div { word-wrap: break-word; }");
            Assert.Equal(CssOverflowWrap.BreakWord, style.OverflowWrap);
        }

        // Mask shorthand
        [Fact]
        public void MaskShorthand_None()
        {
            var style = ResolveElement("div { mask: none; }");
            // mask: none should set mask-image to none
            var raw = style.GetRefValue(Properties.Internal.PropertyId.MaskImage);
            Assert.Equal("none", raw);
        }
    }
}
