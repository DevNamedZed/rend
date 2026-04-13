using System.Collections.Generic;
using System.Linq;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Cascade.Internal;
using Rend.Css.Properties.Internal;
using Rend.Css.Resolution.Internal;
using Xunit;

namespace Rend.Css.Tests
{
    public class VarSubstitutionTests
    {
        // ═══════════════════════════════════════════
        // Basic var() substitution
        // ═══════════════════════════════════════════

        [Fact]
        public void Var_SubstitutesCustomProperty()
        {
            // --my-width: 200px; width: var(--my-width);
            var winners = new Dictionary<string, CascadedDeclaration>
            {
                ["--my-width"] = MakeDecl("--my-width", new CssDimensionValue(200, "px")),
                ["width"] = MakeDecl("width", MakeVar("--my-width")),
            };

            var style = BuildStyle(winners);

            Assert.Equal(200f, style.Width, 0.01);
        }

        [Fact]
        public void Var_FallbackValueUsed()
        {
            // width: var(--missing, 100px) → uses fallback 100px
            var winners = new Dictionary<string, CascadedDeclaration>
            {
                ["width"] = MakeDecl("width", MakeVarWithFallback("--missing", new CssDimensionValue(100, "px"))),
            };

            var style = BuildStyle(winners);

            Assert.Equal(100f, style.Width, 0.01);
        }

        [Fact]
        public void Var_InheritedFromParent()
        {
            // Parent: --color: red; Child: color: var(--color);
            var parentWinners = new Dictionary<string, CascadedDeclaration>
            {
                ["--text-size"] = MakeDecl("--text-size", new CssDimensionValue(24, "px")),
            };
            var parentStyle = BuildStyle(parentWinners);

            var childWinners = new Dictionary<string, CascadedDeclaration>
            {
                ["font-size"] = MakeDecl("font-size", MakeVar("--text-size")),
            };
            var childStyle = BuildStyle(childWinners, parentStyle);

            Assert.Equal(24f, childStyle.FontSize, 0.01);
        }

        [Fact]
        public void Var_ChildOverridesParent()
        {
            // Parent: --size: 100px; Child: --size: 200px; width: var(--size);
            var parentWinners = new Dictionary<string, CascadedDeclaration>
            {
                ["--size"] = MakeDecl("--size", new CssDimensionValue(100, "px")),
            };
            var parentStyle = BuildStyle(parentWinners);

            var childWinners = new Dictionary<string, CascadedDeclaration>
            {
                ["--size"] = MakeDecl("--size", new CssDimensionValue(200, "px")),
                ["width"] = MakeDecl("width", MakeVar("--size")),
            };
            var childStyle = BuildStyle(childWinners, parentStyle);

            Assert.Equal(200f, childStyle.Width, 0.01);
        }

        [Fact]
        public void Var_ChainedReferences()
        {
            // --a: 50px; --b: var(--a); width: var(--b);
            var winners = new Dictionary<string, CascadedDeclaration>
            {
                ["--a"] = MakeDecl("--a", new CssDimensionValue(50, "px")),
                ["--b"] = MakeDecl("--b", MakeVar("--a")),
                ["width"] = MakeDecl("width", MakeVar("--b")),
            };

            var style = BuildStyle(winners);

            Assert.Equal(50f, style.Width, 0.01);
        }

        [Fact]
        public void Var_MissingWithNoFallback_DefaultsToZero()
        {
            // width: var(--missing) → no fallback → treated as 0/invalid
            var winners = new Dictionary<string, CascadedDeclaration>
            {
                ["width"] = MakeDecl("width", MakeVar("--missing")),
            };

            var style = BuildStyle(winners);

            // The var resolves to 0 (invalid) which becomes 0px
            Assert.Equal(0f, style.Width, 0.01);
        }

        [Fact]
        public void Var_CustomPropertiesStored()
        {
            // --my-prop: 42px
            var winners = new Dictionary<string, CascadedDeclaration>
            {
                ["--my-prop"] = MakeDecl("--my-prop", new CssDimensionValue(42, "px")),
            };

            var style = BuildStyle(winners);

            Assert.NotNull(style.CustomProperties);
            Assert.True(style.CustomProperties!.ContainsKey("--my-prop"));
        }

        [Fact]
        public void Var_NoCustomProperties_NullDictionary()
        {
            var winners = new Dictionary<string, CascadedDeclaration>
            {
                ["width"] = MakeDecl("width", new CssDimensionValue(100, "px")),
            };

            var style = BuildStyle(winners);

            Assert.Null(style.CustomProperties);
        }

        [Fact]
        public void Var_InListValue_Substituted()
        {
            // margin: var(--space) var(--space) → both substituted
            var varRef = MakeVar("--space");
            var listValue = new CssListValue(new List<CssValue> { varRef, varRef }, ' ');

            var customProps = new Dictionary<string, CssValue>
            {
                ["--space"] = new CssDimensionValue(10, "px"),
            };

            var result = ComputedStyleBuilder.SubstituteVar(listValue, customProps);

            Assert.IsType<CssListValue>(result);
            var list = (CssListValue)result;
            Assert.Equal(2, list.Values.Count);
            Assert.IsType<CssDimensionValue>(list.Values[0]);
            Assert.Equal(10f, ((CssDimensionValue)list.Values[0]).Value, 0.01);
        }

        [Fact]
        public void Var_NonVarValue_Unchanged()
        {
            var value = new CssDimensionValue(100, "px");
            var customProps = new Dictionary<string, CssValue>
            {
                ["--unused"] = new CssDimensionValue(200, "px"),
            };

            var result = ComputedStyleBuilder.SubstituteVar(value, customProps);

            Assert.Same(value, result); // Should return same instance
        }

        // ═══════════════════════════════════════════
        // revert keyword
        // ═══════════════════════════════════════════

        [Fact]
        public void Revert_NonInheritedProperty_UsesInitial()
        {
            // width: 200px on parent; width: revert on child → initial (0/auto)
            var parentWinners = new Dictionary<string, CascadedDeclaration>
            {
                ["width"] = MakeDecl("width", new CssDimensionValue(200, "px")),
            };
            var parentStyle = BuildStyle(parentWinners);

            var childWinners = new Dictionary<string, CascadedDeclaration>
            {
                ["width"] = MakeDecl("width", new CssKeywordValue("revert")),
            };
            var childStyle = BuildStyle(childWinners, parentStyle);

            // width is non-inherited, revert → initial value (NaN = auto)
            Assert.True(float.IsNaN(childStyle.Width));
        }

        [Fact]
        public void Revert_InheritedProperty_InheritsFromParent()
        {
            // font-size: 24px on parent; font-size: revert on child → inherits 24px
            var parentWinners = new Dictionary<string, CascadedDeclaration>
            {
                ["font-size"] = MakeDecl("font-size", new CssDimensionValue(24, "px")),
            };
            var parentStyle = BuildStyle(parentWinners);

            var childWinners = new Dictionary<string, CascadedDeclaration>
            {
                ["font-size"] = MakeDecl("font-size", new CssKeywordValue("revert")),
            };
            var childStyle = BuildStyle(childWinners, parentStyle);

            // font-size is inherited, revert → inherits from parent
            Assert.Equal(24f, childStyle.FontSize, 0.01);
        }

        // ═══════════════════════════════════════════
        // Full parser pipeline tests (CSS text → parsed value)
        // ═══════════════════════════════════════════

        [Fact]
        public void Var_FullParse_SimpleFallback()
        {
            // Parse through the full CSS pipeline
            var sheet = CssParser.Parse("div { width: var(--w, 100px); }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = rule.Declarations.First(d => d.Property == "width");

            // Should be a CssFunctionValue("var") with 2 args
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("var", fn.Name);
            Assert.Equal(2, fn.Arguments.Count);

            // First arg: custom property name
            var propName = Assert.IsType<CssKeywordValue>(fn.Arguments[0]);
            Assert.Equal("--w", propName.Keyword);

            // Second arg: fallback value
            var fallback = Assert.IsType<CssDimensionValue>(fn.Arguments[1]);
            Assert.Equal(100f, fallback.Value, 0.01);
            Assert.Equal("px", fallback.Unit);
        }

        [Fact]
        public void Var_FullParse_SpaceSeparatedFallback()
        {
            // var(--border, 2px solid #333) kept as shorthand (pending-substitution)
            // Not expanded to longhands at parse time — that happens after var() resolution
            var sheet = CssParser.Parse("div { border: var(--border, 2px solid #333); }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = rule.Declarations.First(d => d.Property == "border");

            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("var", fn.Name);
            Assert.Equal(2, fn.Arguments.Count);

            // Fallback should be a space-separated list: [2px, solid, #333333]
            var fallback = Assert.IsType<CssListValue>(fn.Arguments[1]);
            Assert.Equal(3, fallback.Values.Count);
        }

        [Fact]
        public void Var_FullParse_CommaSeparatedFallback()
        {
            // var(--font, Arial, sans-serif) → fallback is comma-separated list
            var sheet = CssParser.Parse("div { font-family: var(--font, Arial, sans-serif); }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var decl = rule.Declarations.First(d => d.Property == "font-family");

            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("var", fn.Name);
            Assert.Equal(2, fn.Arguments.Count);

            // Fallback should be a comma-separated list: [Arial, sans-serif]
            var fallback = Assert.IsType<CssListValue>(fn.Arguments[1]);
            Assert.Equal(',', fallback.Separator);
            Assert.Equal(2, fallback.Values.Count);
        }

        [Fact]
        public void Var_FullParse_FallbackResolvedInStyle()
        {
            // End-to-end: var(--missing, 100px) should resolve to width: 100px
            var style = ResolveElement("div { width: var(--missing, 100px); }");
            Assert.Equal(100f, style.Width, 0.01);
        }

        [Fact]
        public void Var_FullParse_CommaSeparatedFallbackResolvedInStyle()
        {
            // End-to-end: var(--font, Arial, sans-serif) should resolve font-family
            var style = ResolveElement("div { font-family: var(--font, Arial, sans-serif); }");
            // Should resolve to "arial" (first item in comma list)
            Assert.Equal("arial", style.FontFamily);
        }

        // ═══════════════════════════════════════════
        // Background shorthand box keyword
        // ═══════════════════════════════════════════

        [Fact]
        public void Background_SingleBoxKeyword_SetsBothClipAndOrigin()
        {
            var style = ResolveElement("div { background: #e74c3c padding-box; }");
            Assert.Equal(CssBackgroundClip.PaddingBox, style.BackgroundClip);
            Assert.Equal(CssBackgroundOrigin.PaddingBox, style.BackgroundOrigin);
        }

        [Fact]
        public void Background_ShorthandExpands_ClipAndOrigin()
        {
            // Verify the shorthand parser produces the right longhands
            var sheet = CssParser.Parse("div { background: #e74c3c padding-box; }");
            var rule = Assert.IsType<StyleRule>(sheet.Rules[0]);
            var clipDecl = rule.Declarations.FirstOrDefault(d => d.Property == "background-clip");
            var originDecl = rule.Declarations.FirstOrDefault(d => d.Property == "background-origin");
            Assert.NotNull(clipDecl);
            Assert.NotNull(originDecl);
            var clipKw = Assert.IsType<CssKeywordValue>(clipDecl.Value);
            var originKw = Assert.IsType<CssKeywordValue>(originDecl.Value);
            Assert.Equal("padding-box", clipKw.Keyword);
            Assert.Equal("padding-box", originKw.Keyword);
        }

        [Fact]
        public void Background_ContentBox_SetsBothClipAndOrigin()
        {
            var style = ResolveElement("div { background: #3498db content-box; }");
            Assert.Equal(CssBackgroundClip.ContentBox, style.BackgroundClip);
            Assert.Equal(CssBackgroundOrigin.ContentBox, style.BackgroundOrigin);
        }

        // ═══════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════

        private ComputedStyle ResolveElement(string css)
        {
            var matcher = new MockSelectorMatcher();
            var resolver = new StyleResolver(matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 800,
                ViewportHeight = 600
            });

            if (!string.IsNullOrEmpty(css))
            {
                resolver.AddStylesheet(CssParser.Parse(css));
            }

            var element = new MockStylableElement { TagName = "div" };
            return resolver.Resolve(element);
        }

        private static CssValue MakeVar(string name)
        {
            return new CssFunctionValue("var", new List<CssValue>
            {
                new CssKeywordValue(name),
            });
        }

        private static CssValue MakeVarWithFallback(string name, CssValue fallback)
        {
            return new CssFunctionValue("var", new List<CssValue>
            {
                new CssKeywordValue(name),
                fallback,
            });
        }

        private static CascadedDeclaration MakeDecl(string property, CssValue value)
        {
            var decl = new CssDeclaration(property, value, false);
            return new CascadedDeclaration(decl, new CascadePriority(CascadeOrigin.Author, false, CssSpecificity.Zero, 0));
        }

        private static ComputedStyle BuildStyle(
            Dictionary<string, CascadedDeclaration> winners,
            ComputedStyle? parentStyle = null)
        {
            var ctx = new CssResolutionContext(16, 16, 800, 600);
            var builder = new ComputedStyleBuilder(ctx);

            // Adapt the single-winner test fixture to the production
            // cascade shape (one CascadedProperty per property with a
            // single candidate inside).
            var cascaded = new Dictionary<string, CascadedProperty>(winners.Count);
            foreach (var kvp in winners)
            {
                var property = new CascadedProperty();
                property.Add(kvp.Value);
                cascaded[kvp.Key] = property;
            }

            return builder.Build(cascaded, parentStyle);
        }
    }
}
