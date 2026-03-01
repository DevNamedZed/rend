using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class CursorPointerTests
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
        public void Cursor_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssCursor.Auto, style.Cursor);
        }

        [Fact]
        public void Cursor_Pointer()
        {
            var style = ResolveElement("div { cursor: pointer; }");
            Assert.Equal(CssCursor.Pointer, style.Cursor);
        }

        [Fact]
        public void Cursor_Text()
        {
            var style = ResolveElement("div { cursor: text; }");
            Assert.Equal(CssCursor.Text, style.Cursor);
        }

        [Fact]
        public void Cursor_NotAllowed()
        {
            var style = ResolveElement("div { cursor: not-allowed; }");
            Assert.Equal(CssCursor.NotAllowed, style.Cursor);
        }

        [Fact]
        public void Cursor_Move()
        {
            var style = ResolveElement("div { cursor: move; }");
            Assert.Equal(CssCursor.Move, style.Cursor);
        }

        [Fact]
        public void Cursor_Wait()
        {
            var style = ResolveElement("div { cursor: wait; }");
            Assert.Equal(CssCursor.Wait, style.Cursor);
        }

        [Fact]
        public void Cursor_Grab()
        {
            var style = ResolveElement("div { cursor: grab; }");
            Assert.Equal(CssCursor.Grab, style.Cursor);
        }

        [Fact]
        public void PointerEvents_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssPointerEvents.Auto, style.PointerEvents);
        }

        [Fact]
        public void PointerEvents_None()
        {
            var style = ResolveElement("div { pointer-events: none; }");
            Assert.Equal(CssPointerEvents.None, style.PointerEvents);
        }

        [Fact]
        public void FontVariant_Default_IsNormal()
        {
            var style = ResolveElement("");
            Assert.Equal(CssFontVariant.Normal, style.FontVariant);
        }

        [Fact]
        public void FontVariant_SmallCaps()
        {
            var style = ResolveElement("div { font-variant: small-caps; }");
            Assert.Equal(CssFontVariant.SmallCaps, style.FontVariant);
        }
    }
}
