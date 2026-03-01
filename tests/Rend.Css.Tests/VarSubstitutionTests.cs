using System.Collections.Generic;
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
        // Helpers
        // ═══════════════════════════════════════════

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
            return builder.Build(winners, parentStyle);
        }
    }
}
