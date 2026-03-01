using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class KeyframesTests
    {
        [Fact]
        public void Keyframes_BasicParsing()
        {
            var sheet = CssParser.Parse(@"
                @keyframes fadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
            ");

            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<KeyframesRule>(sheet.Rules[0]);
            Assert.Equal("fadeIn", rule.Name);
            Assert.Equal(2, rule.Keyframes.Count);
        }

        [Fact]
        public void Keyframes_FromTo_Selectors()
        {
            var sheet = CssParser.Parse(@"
                @keyframes slide {
                    from { left: 0; }
                    to { left: 100px; }
                }
            ");

            var rule = Assert.IsType<KeyframesRule>(sheet.Rules[0]);
            Assert.Equal("from", rule.Keyframes[0].Selector);
            Assert.Equal("to", rule.Keyframes[1].Selector);
        }

        [Fact]
        public void Keyframes_PercentageSelectors()
        {
            var sheet = CssParser.Parse(@"
                @keyframes bounce {
                    0% { top: 0; }
                    50% { top: 100px; }
                    100% { top: 0; }
                }
            ");

            var rule = Assert.IsType<KeyframesRule>(sheet.Rules[0]);
            Assert.Equal(3, rule.Keyframes.Count);
            Assert.Equal("0%", rule.Keyframes[0].Selector);
            Assert.Equal("50%", rule.Keyframes[1].Selector);
            Assert.Equal("100%", rule.Keyframes[2].Selector);
        }

        [Fact]
        public void Keyframes_Declarations_Parsed()
        {
            var sheet = CssParser.Parse(@"
                @keyframes anim {
                    from { opacity: 0; color: red; }
                    to { opacity: 1; color: blue; }
                }
            ");

            var rule = Assert.IsType<KeyframesRule>(sheet.Rules[0]);
            Assert.Equal(2, rule.Keyframes[0].Declarations.Count);
            Assert.Equal("opacity", rule.Keyframes[0].Declarations[0].Property);
            Assert.Equal("color", rule.Keyframes[0].Declarations[1].Property);
        }

        [Fact]
        public void Keyframes_StringName()
        {
            var sheet = CssParser.Parse(@"
                @keyframes ""my animation"" {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
            ");

            var rule = Assert.IsType<KeyframesRule>(sheet.Rules[0]);
            Assert.Equal("my animation", rule.Name);
        }

        [Fact]
        public void Keyframes_WebkitPrefix()
        {
            var sheet = CssParser.Parse(@"
                @-webkit-keyframes fadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
            ");

            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<KeyframesRule>(sheet.Rules[0]);
            Assert.Equal("fadeIn", rule.Name);
        }

        [Fact]
        public void Keyframes_MozPrefix()
        {
            var sheet = CssParser.Parse(@"
                @-moz-keyframes fadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
            ");

            Assert.Single(sheet.Rules);
            var rule = Assert.IsType<KeyframesRule>(sheet.Rules[0]);
            Assert.Equal("fadeIn", rule.Name);
        }

        [Fact]
        public void Keyframes_FollowedByStyleRules()
        {
            var sheet = CssParser.Parse(@"
                @keyframes fadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
                div { color: red; }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            Assert.IsType<KeyframesRule>(sheet.Rules[0]);
            Assert.IsType<StyleRule>(sheet.Rules[1]);
        }

        [Fact]
        public void Keyframes_DoNotAffectStyleResolution()
        {
            var matcher = new MockSelectorMatcher();
            var resolver = new StyleResolver(matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 800,
                ViewportHeight = 600
            });

            resolver.AddStylesheet(CssParser.Parse(@"
                @keyframes fadeIn {
                    from { width: 0; }
                    to { width: 500px; }
                }
                div { width: 200px; }
            "));

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            // Keyframes should not affect computed style
            Assert.Equal(200f, style.Width, 0.01);
        }

        [Fact]
        public void Keyframes_EmptyBody()
        {
            var sheet = CssParser.Parse("@keyframes empty { }");

            var rule = Assert.IsType<KeyframesRule>(sheet.Rules[0]);
            Assert.Equal("empty", rule.Name);
            Assert.Empty(rule.Keyframes);
        }

        [Fact]
        public void Keyframes_MultipleInStylesheet()
        {
            var sheet = CssParser.Parse(@"
                @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
                @keyframes slideIn { from { left: -100px; } to { left: 0; } }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var r1 = Assert.IsType<KeyframesRule>(sheet.Rules[0]);
            var r2 = Assert.IsType<KeyframesRule>(sheet.Rules[1]);
            Assert.Equal("fadeIn", r1.Name);
            Assert.Equal("slideIn", r2.Name);
        }
    }
}
