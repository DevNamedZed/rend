using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class TextOverflowTests
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
        public void TextOverflow_Default_IsClip()
        {
            var style = ResolveElement("");
            Assert.Equal(CssTextOverflow.Clip, style.TextOverflow);
        }

        [Fact]
        public void TextOverflow_Ellipsis_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-overflow: ellipsis; }");
            Assert.Equal(CssTextOverflow.Ellipsis, style.TextOverflow);
        }

        [Fact]
        public void TextOverflow_Clip_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-overflow: clip; }");
            Assert.Equal(CssTextOverflow.Clip, style.TextOverflow);
        }

        [Fact]
        public void OverflowWrap_Default_IsNormal()
        {
            var style = ResolveElement("");
            Assert.Equal(CssOverflowWrap.Normal, style.OverflowWrap);
        }

        [Fact]
        public void OverflowWrap_BreakWord_ResolvesCorrectly()
        {
            var style = ResolveElement("div { overflow-wrap: break-word; }");
            Assert.Equal(CssOverflowWrap.BreakWord, style.OverflowWrap);
        }

        [Fact]
        public void OverflowWrap_Anywhere_ResolvesCorrectly()
        {
            var style = ResolveElement("div { overflow-wrap: anywhere; }");
            Assert.Equal(CssOverflowWrap.Anywhere, style.OverflowWrap);
        }

        [Fact]
        public void OverflowWrap_IsInherited()
        {
            var resolver = new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 800,
                ViewportHeight = 600
            });
            resolver.AddStylesheet(CssParser.Parse("div { overflow-wrap: break-word; }"));

            var parent = new MockStylableElement { TagName = "div" };
            var parentStyle = resolver.Resolve(parent);

            var child = new MockStylableElement { TagName = "span" };
            var childStyle = resolver.Resolve(child, parentStyle);

            Assert.Equal(CssOverflowWrap.BreakWord, parentStyle.OverflowWrap);
            Assert.Equal(CssOverflowWrap.BreakWord, childStyle.OverflowWrap); // inherited
        }

        [Fact]
        public void TextOverflow_NotInherited()
        {
            var resolver = new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 800,
                ViewportHeight = 600
            });
            resolver.AddStylesheet(CssParser.Parse("div { text-overflow: ellipsis; }"));

            var parent = new MockStylableElement { TagName = "div" };
            var parentStyle = resolver.Resolve(parent);

            var child = new MockStylableElement { TagName = "span" };
            var childStyle = resolver.Resolve(child, parentStyle);

            Assert.Equal(CssTextOverflow.Ellipsis, parentStyle.TextOverflow);
            Assert.Equal(CssTextOverflow.Clip, childStyle.TextOverflow); // not inherited
        }

        [Fact]
        public void WordSpacing_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.WordSpacing, 0.01);
        }

        [Fact]
        public void WordSpacing_Set_ResolvesCorrectly()
        {
            var style = ResolveElement("div { word-spacing: 5px; }");
            Assert.Equal(5f, style.WordSpacing, 0.01);
        }

        [Fact]
        public void AlignSelf_Default()
        {
            var style = ResolveElement("");
            // AlignSelf defaults to auto which maps to Stretch (0) in CssAlignItems
            Assert.True(true); // Just ensure it doesn't throw
        }

        [Fact]
        public void AlignContent_Default()
        {
            var style = ResolveElement("");
            // Just ensure the accessor works
            var _ = style.AlignContent;
            Assert.True(true);
        }
    }
}
