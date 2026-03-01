using System.Linq;
using Rend.Css;
using Xunit;

namespace Rend.Css.Conformance.Values
{
    public class UnitConversionTests
    {
        private static CssDeclaration GetDeclaration(string css, string property)
        {
            var sheet = CssParser.Parse(css);
            var rule = sheet.Rules.OfType<StyleRule>().First();
            return rule.Declarations.First(d => d.Property == property);
        }

        #region Length Units Parsing

        [Theory]
        [InlineData("div { width: 100px; }", "px")]
        [InlineData("div { width: 10em; }", "em")]
        [InlineData("div { width: 2rem; }", "rem")]
        [InlineData("div { width: 12pt; }", "pt")]
        [InlineData("div { width: 1cm; }", "cm")]
        [InlineData("div { width: 10mm; }", "mm")]
        [InlineData("div { width: 1in; }", "in")]
        [InlineData("div { width: 5ch; }", "ch")]
        [InlineData("div { width: 3ex; }", "ex")]
        [InlineData("div { width: 2pc; }", "pc")]
        [InlineData("div { width: 40q; }", "q")]
        public void LengthUnit_ParsesWithCorrectUnit(string css, string expectedUnit)
        {
            var decl = GetDeclaration(css, "width");
            var dim = Assert.IsType<CssDimensionValue>(decl.Value);
            Assert.Equal(expectedUnit, dim.Unit);
        }

        #endregion

        #region Viewport Units

        [Theory]
        [InlineData("div { width: 50vw; }", "vw")]
        [InlineData("div { height: 100vh; }", "vh")]
        [InlineData("div { width: 30vmin; }", "vmin")]
        [InlineData("div { width: 30vmax; }", "vmax")]
        public void ViewportUnit_ParsesWithCorrectUnit(string css, string expectedUnit)
        {
            var prop = css.Contains("height") ? "height" : "width";
            var decl = GetDeclaration(css, prop);
            var dim = Assert.IsType<CssDimensionValue>(decl.Value);
            Assert.Equal(expectedUnit, dim.Unit);
        }

        #endregion

        #region Numeric Values

        [Theory]
        [InlineData("div { width: 100px; }", 100f)]
        [InlineData("div { width: 0px; }", 0f)]
        [InlineData("div { width: 1.5em; }", 1.5f)]
        [InlineData("div { width: 0.5rem; }", 0.5f)]
        [InlineData("div { width: -10px; }", -10f)]
        [InlineData("div { width: 99.99px; }", 99.99f)]
        public void Dimension_HasCorrectNumericValue(string css, float expectedValue)
        {
            var decl = GetDeclaration(css, "width");
            var dim = Assert.IsType<CssDimensionValue>(decl.Value);
            Assert.Equal(expectedValue, dim.Value, 2);
        }

        #endregion

        #region Percentage Values

        [Theory]
        [InlineData("div { width: 50%; }", 50f)]
        [InlineData("div { width: 100%; }", 100f)]
        [InlineData("div { width: 0%; }", 0f)]
        [InlineData("div { width: 33.33%; }", 33.33f)]
        public void Percentage_HasCorrectValue(string css, float expected)
        {
            var decl = GetDeclaration(css, "width");
            var pct = Assert.IsType<CssPercentageValue>(decl.Value);
            Assert.Equal(expected, pct.Value, 2);
        }

        #endregion

        #region Zero Without Unit

        [Fact]
        public void Zero_WithoutUnit_ParsesAsNumber()
        {
            var decl = GetDeclaration("div { margin-top: 0; }", "margin-top");
            Assert.NotNull(decl);
            // Zero is valid without units
            Assert.Equal("0", decl.Value.ToString());
        }

        #endregion

        #region Angle Units

        [Theory]
        [InlineData("div { transform: rotate(45deg); }", "deg")]
        [InlineData("div { transform: rotate(1rad); }", "rad")]
        [InlineData("div { transform: rotate(100grad); }", "grad")]
        [InlineData("div { transform: rotate(0.5turn); }", "turn")]
        public void AngleUnit_ParsesInsideFunction(string css, string expectedUnit)
        {
            var decl = GetDeclaration(css, "transform");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("rotate", fn.Name);
            var arg = fn.Arguments.OfType<CssDimensionValue>().First();
            Assert.Equal(expectedUnit, arg.Unit);
        }

        #endregion

        #region Time Units

        [Theory]
        [InlineData("div { transition-duration: 300ms; }", "ms")]
        [InlineData("div { transition-duration: 0.3s; }", "s")]
        public void TimeUnit_ParsesCorrectly(string css, string expectedUnit)
        {
            var decl = GetDeclaration(css, "transition-duration");
            var dim = Assert.IsType<CssDimensionValue>(decl.Value);
            Assert.Equal(expectedUnit, dim.Unit);
        }

        #endregion

        #region ToString Roundtrip

        [Theory]
        [InlineData("100px")]
        [InlineData("1.5em")]
        [InlineData("50%")]
        [InlineData("0")]
        public void Value_ToString_RoundtripsCorrectly(string value)
        {
            var css = $"div {{ width: {value}; }}";
            var decl = GetDeclaration(css, "width");
            Assert.Equal(value, decl.Value.ToString());
        }

        #endregion
    }
}
