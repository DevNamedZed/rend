using System;
using System.Collections.Generic;
using System.Linq;
using Rend.Core.Values;
using Xunit;

namespace Rend.Css.Tests
{
    /// <summary>
    /// Simple mock implementation of IStylableElement for testing the cascade engine.
    /// </summary>
    internal class MockStylableElement : IStylableElement
    {
        public string TagName { get; set; } = "div";
        public string? Id { get; set; }
        public IReadOnlyList<string> ClassList { get; set; } = Array.Empty<string>();
        public string? InlineStyle { get; set; }
        public IStylableElement? Parent { get; set; }
        public IStylableElement? PreviousSibling { get; set; }
        public IStylableElement? NextSibling { get; set; }
        public IStylableElement? FirstChild { get; set; }
        public IStylableElement? LastChild { get; set; }
        public IEnumerable<IStylableElement> Children { get; set; } = Array.Empty<IStylableElement>();

        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>();

        public string? GetAttribute(string name)
        {
            return _attributes.TryGetValue(name, out var val) ? val : null;
        }

        public void SetAttribute(string name, string value)
        {
            _attributes[name] = value;
        }
    }

    /// <summary>
    /// Simple mock selector matcher that supports basic matching by tag, class, id.
    /// </summary>
    internal class MockSelectorMatcher : ISelectorMatcher
    {
        public bool Matches(IStylableElement element, string selectorText)
        {
            // Handle comma-separated selectors
            var selectors = selectorText.Split(',');
            foreach (var sel in selectors)
            {
                if (MatchesSingle(element, sel.Trim()))
                    return true;
            }
            return false;
        }

        private bool MatchesSingle(IStylableElement element, string selector)
        {
            // Strip pseudo-elements (::before, ::after) for matching
            int pseudoIdx = selector.IndexOf("::", StringComparison.Ordinal);
            if (pseudoIdx >= 0)
                selector = selector.Substring(0, pseudoIdx).Trim();

            if (selector == "*" || selector.Length == 0)
                return true;

            // Simple tag match
            if (!selector.Contains('.') && !selector.Contains('#') && !selector.Contains(':') && !selector.Contains('['))
                return string.Equals(element.TagName, selector, StringComparison.OrdinalIgnoreCase);

            // #id match
            if (selector.StartsWith("#"))
                return element.Id == selector.Substring(1);

            // .class match
            if (selector.StartsWith("."))
                return element.ClassList.Contains(selector.Substring(1));

            // tag.class match
            if (selector.Contains('.'))
            {
                var parts = selector.Split('.');
                if (parts.Length == 2)
                {
                    return string.Equals(element.TagName, parts[0], StringComparison.OrdinalIgnoreCase)
                        && element.ClassList.Contains(parts[1]);
                }
            }

            return false;
        }

        public CssSpecificity GetSpecificity(string selectorText)
        {
            // Handle comma-separated selectors (take the first)
            var selector = selectorText.Split(',')[0].Trim();

            int ids = 0, classes = 0, elements = 0;

            // Count #ids
            ids = selector.Split('#').Length - 1;

            // Count .classes
            classes = selector.Split('.').Length - 1;

            // Count elements (rough)
            var parts = selector.Split(new[] { ' ', '>', '+', '~' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var clean = part.TrimStart('#', '.');
                if (clean.Length > 0 && !part.StartsWith("#") && !part.StartsWith("."))
                {
                    // Check if part starts with a letter (tag name)
                    if (part.Length > 0 && char.IsLetter(part[0]))
                        elements++;
                }
            }

            return new CssSpecificity(ids, classes, elements);
        }
    }

    public class StyleResolverTests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private StyleResolver CreateResolver(StyleResolverOptions? options = null)
        {
            var resolverOptions = options ?? new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 1920,
                ViewportHeight = 1080
            };
            return new StyleResolver(_matcher, resolverOptions);
        }

        #region Basic Resolution

        [Fact]
        public void Resolve_EmptyStylesheet_ReturnsDefaultComputedStyle()
        {
            var resolver = CreateResolver();
            var element = new MockStylableElement { TagName = "div" };

            var style = resolver.Resolve(element);

            Assert.NotNull(style);
            // Default display for CSS initial value is Inline
            Assert.Equal(CssDisplay.Inline, style.Display);
        }

        [Fact]
        public void Resolve_SimpleRule_AppliesProperty()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("div { display: block; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Block, style.Display);
        }

        [Fact]
        public void Resolve_MultipleProperties_AppliesAll()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("div { display: block; position: relative; visibility: hidden; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Block, style.Display);
            Assert.Equal(CssPosition.Relative, style.Position);
            Assert.Equal(CssVisibility.Hidden, style.Visibility);
        }

        [Fact]
        public void Resolve_NonMatchingRule_DoesNotApply()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("span { display: block; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            // Should remain default (Inline)
            Assert.Equal(CssDisplay.Inline, style.Display);
        }

        #endregion

        #region Specificity

        [Fact]
        public void Resolve_HigherSpecificity_Wins()
        {
            var resolver = CreateResolver();
            var css = "div { display: block; } #main { display: flex; }";
            var sheet = CssParser.Parse(css);
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement
            {
                TagName = "div",
                Id = "main"
            };
            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Flex, style.Display);
        }

        [Fact]
        public void Resolve_ClassBeatsElement_InSpecificity()
        {
            var resolver = CreateResolver();
            var css = "div { display: block; } .container { display: flex; }";
            var sheet = CssParser.Parse(css);
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement
            {
                TagName = "div",
                ClassList = new[] { "container" }
            };
            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Flex, style.Display);
        }

        #endregion

        #region Source Order

        [Fact]
        public void Resolve_SameSpecificity_LaterRuleWins()
        {
            var resolver = CreateResolver();
            var css = "div { display: block; } div { display: flex; }";
            var sheet = CssParser.Parse(css);
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Flex, style.Display);
        }

        [Fact]
        public void Resolve_LaterStylesheet_WinsAtSameSpecificity()
        {
            var resolver = CreateResolver();
            var sheet1 = CssParser.Parse("div { display: block; }");
            var sheet2 = CssParser.Parse("div { display: flex; }");
            resolver.AddStylesheet(sheet1);
            resolver.AddStylesheet(sheet2);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Flex, style.Display);
        }

        #endregion

        #region !important

        [Fact]
        public void Resolve_Important_BeatsHigherSpecificity()
        {
            var resolver = CreateResolver();
            var css = "#main { display: flex; } div { display: block !important; }";
            var sheet = CssParser.Parse(css);
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement
            {
                TagName = "div",
                Id = "main"
            };
            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Block, style.Display);
        }

        [Fact]
        public void Resolve_ImportantVsImportant_HigherSpecificityWins()
        {
            var resolver = CreateResolver();
            var css = "div { display: block !important; } #main { display: flex !important; }";
            var sheet = CssParser.Parse(css);
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement
            {
                TagName = "div",
                Id = "main"
            };
            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Flex, style.Display);
        }

        #endregion

        #region Inline Styles

        [Fact]
        public void Resolve_InlineStyle_BeatsAuthorStyles()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("#main { display: flex; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement
            {
                TagName = "div",
                Id = "main",
                InlineStyle = "display: block;"
            };
            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Block, style.Display);
        }

        [Fact]
        public void Resolve_InlineStyle_ParsesMultipleProperties()
        {
            var resolver = CreateResolver();

            var element = new MockStylableElement
            {
                TagName = "div",
                InlineStyle = "display: flex; position: absolute;"
            };
            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Flex, style.Display);
            Assert.Equal(CssPosition.Absolute, style.Position);
        }

        #endregion

        #region Inheritance

        [Fact]
        public void Resolve_InheritedProperty_InheritsFromParent()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("div { color: #ff0000; }");
            resolver.AddStylesheet(sheet);

            var parent = new MockStylableElement { TagName = "div" };
            var parentStyle = resolver.Resolve(parent);

            var child = new MockStylableElement { TagName = "span", Parent = parent };
            var childStyle = resolver.Resolve(child, parentStyle);

            // color is an inherited property
            Assert.Equal(parentStyle.Color, childStyle.Color);
        }

        [Fact]
        public void Resolve_NonInheritedProperty_DoesNotInherit()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("div { display: flex; }");
            resolver.AddStylesheet(sheet);

            var parent = new MockStylableElement { TagName = "div" };
            var parentStyle = resolver.Resolve(parent);

            var child = new MockStylableElement { TagName = "span", Parent = parent };
            var childStyle = resolver.Resolve(child, parentStyle);

            // display is NOT inherited, child should get initial value (Inline)
            Assert.Equal(CssDisplay.Inline, childStyle.Display);
        }

        [Fact]
        public void Resolve_FontSizeInheritance_Works()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("div { font-size: 24px; }");
            resolver.AddStylesheet(sheet);

            var parent = new MockStylableElement { TagName = "div" };
            var parentStyle = resolver.Resolve(parent);

            var child = new MockStylableElement { TagName = "span", Parent = parent };
            var childStyle = resolver.Resolve(child, parentStyle);

            // font-size is inherited
            Assert.Equal(24f, childStyle.FontSize);
        }

        #endregion

        #region Value Resolution

        [Fact]
        public void Resolve_PxValue_ResolvesToPx()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("div { margin-top: 20px; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(20f, style.MarginTop);
        }

        [Fact]
        public void Resolve_ColorProperty_ResolvesToRGBA()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("div { color: #00ff00; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(0, style.Color.R);
            Assert.Equal(255, style.Color.G);
            Assert.Equal(0, style.Color.B);
        }

        [Fact]
        public void Resolve_BackgroundColor_ResolvesToRGBA()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("div { background-color: #0000ff; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(0, style.BackgroundColor.R);
            Assert.Equal(0, style.BackgroundColor.G);
            Assert.Equal(255, style.BackgroundColor.B);
        }

        [Fact]
        public void Resolve_FontWeightBold_ResolvesTo700()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("div { font-weight: bold; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(700f, style.FontWeight);
        }

        [Fact]
        public void Resolve_FontWeightNormal_ResolvesTo400()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("div { font-weight: normal; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(400f, style.FontWeight);
        }

        [Fact]
        public void Resolve_AutoWidth_ResolvesToNaN()
        {
            var resolver = CreateResolver();
            var sheet = CssParser.Parse("div { width: auto; }");
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.True(float.IsNaN(style.Width));
        }

        #endregion

        #region Multiple Stylesheets

        [Fact]
        public void AddStylesheets_AcceptsMultipleAtOnce()
        {
            var resolver = CreateResolver();
            var sheets = new[]
            {
                CssParser.Parse("div { display: block; }"),
                CssParser.Parse("div { position: relative; }")
            };
            resolver.AddStylesheets(sheets);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Block, style.Display);
            Assert.Equal(CssPosition.Relative, style.Position);
        }

        #endregion

        #region Constructor Validation

        [Fact]
        public void Constructor_NullMatcher_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new StyleResolver(null!));
        }

        [Fact]
        public void Constructor_NullOptions_UsesDefaults()
        {
            var resolver = new StyleResolver(_matcher, null);
            // Should not throw, uses default options
            Assert.NotNull(resolver);
        }

        #endregion

        #region Flexbox Properties

        [Fact]
        public void Resolve_FlexboxProperties_ResolveCorrectly()
        {
            var resolver = CreateResolver();
            var css = "div { display: flex; flex-direction: column; flex-wrap: wrap; justify-content: center; align-items: center; }";
            var sheet = CssParser.Parse(css);
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(CssDisplay.Flex, style.Display);
            Assert.Equal(CssFlexDirection.Column, style.FlexDirection);
            Assert.Equal(CssFlexWrap.Wrap, style.FlexWrap);
            Assert.Equal(CssJustifyContent.Center, style.JustifyContent);
            Assert.Equal(CssAlignItems.Center, style.AlignItems);
        }

        #endregion

        #region Border Properties

        [Fact]
        public void Resolve_BorderStyle_ResolvesCorrectly()
        {
            var resolver = CreateResolver();
            var css = "div { border-top-style: solid; border-top-width: 2px; }";
            var sheet = CssParser.Parse(css);
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(CssBorderStyle.Solid, style.BorderTopStyle);
            Assert.Equal(2f, style.BorderTopWidth);
        }

        #endregion

        #region Text Properties

        [Fact]
        public void Resolve_TextProperties_ResolveCorrectly()
        {
            var resolver = CreateResolver();
            var css = "div { text-align: center; text-transform: uppercase; white-space: nowrap; }";
            var sheet = CssParser.Parse(css);
            resolver.AddStylesheet(sheet);

            var element = new MockStylableElement { TagName = "div" };
            var style = resolver.Resolve(element);

            Assert.Equal(CssTextAlign.Center, style.TextAlign);
            Assert.Equal(CssTextTransform.Uppercase, style.TextTransform);
            Assert.Equal(CssWhiteSpace.Nowrap, style.WhiteSpace);
        }

        #endregion
    }
}
