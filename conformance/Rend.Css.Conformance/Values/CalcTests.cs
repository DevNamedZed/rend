using System.Linq;
using Rend.Css;
using Xunit;

namespace Rend.Css.Conformance.Values
{
    public class CalcTests
    {
        private static CssDeclaration GetDeclaration(string css, string property)
        {
            var sheet = CssParser.Parse(css);
            var rule = sheet.Rules.OfType<StyleRule>().First();
            return rule.Declarations.First(d => d.Property == property);
        }

        #region Basic Calc Expressions

        [Fact]
        public void Calc_SimpleSubtraction_ParsesAsFunction()
        {
            var decl = GetDeclaration("div { width: calc(100% - 20px); }", "width");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("calc", fn.Name);
        }

        [Fact]
        public void Calc_SimpleAddition_ParsesAsFunction()
        {
            var decl = GetDeclaration("div { width: calc(50% + 10px); }", "width");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("calc", fn.Name);
        }

        [Fact]
        public void Calc_Multiplication_ParsesAsFunction()
        {
            var decl = GetDeclaration("div { width: calc(100px * 2); }", "width");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("calc", fn.Name);
        }

        [Fact]
        public void Calc_Division_ParsesAsFunction()
        {
            var decl = GetDeclaration("div { width: calc(100% / 3); }", "width");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("calc", fn.Name);
        }

        #endregion

        #region Calc Arguments Contain Operands

        [Fact]
        public void Calc_ContainsPercentageOperand()
        {
            var decl = GetDeclaration("div { width: calc(100% - 20px); }", "width");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Contains(fn.Arguments, a => a is CssPercentageValue);
        }

        [Fact]
        public void Calc_ContainsDimensionOperand()
        {
            var decl = GetDeclaration("div { width: calc(100% - 20px); }", "width");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Contains(fn.Arguments, a => a is CssDimensionValue);
        }

        [Fact]
        public void Calc_ContainsSubtractionOperator()
        {
            var decl = GetDeclaration("div { width: calc(100% - 20px); }", "width");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Contains(fn.Arguments, a => a is CssKeywordValue kw && kw.Keyword == "-");
        }

        [Fact]
        public void Calc_ContainsAdditionOperator()
        {
            var decl = GetDeclaration("div { width: calc(50% + 10px); }", "width");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Contains(fn.Arguments, a => a is CssKeywordValue kw && kw.Keyword == "+");
        }

        [Fact]
        public void Calc_ContainsMultiplicationOperator()
        {
            var decl = GetDeclaration("div { width: calc(50% * 2); }", "width");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Contains(fn.Arguments, a => a is CssKeywordValue kw && kw.Keyword == "*");
        }

        #endregion

        #region Nested Calc

        [Fact]
        public void Calc_NestedCalc_ParsesCorrectly()
        {
            var decl = GetDeclaration("div { width: calc(calc(100% - 20px) / 2); }", "width");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("calc", fn.Name);
            Assert.Contains(fn.Arguments, a => a is CssFunctionValue nested && nested.Name == "calc");
        }

        #endregion

        #region Calc in Different Properties

        [Fact]
        public void Calc_InHeight_Parses()
        {
            var decl = GetDeclaration("div { height: calc(100vh - 60px); }", "height");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("calc", fn.Name);
        }

        [Fact]
        public void Calc_InMargin_Parses()
        {
            var decl = GetDeclaration("div { margin-top: calc(2em + 5px); }", "margin-top");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("calc", fn.Name);
        }

        [Fact]
        public void Calc_InPadding_Parses()
        {
            var decl = GetDeclaration("div { padding-left: calc(1rem + 10px); }", "padding-left");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("calc", fn.Name);
        }

        [Fact]
        public void Calc_InFontSize_Parses()
        {
            var decl = GetDeclaration("div { font-size: calc(1rem + 2px); }", "font-size");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("calc", fn.Name);
        }

        #endregion

        #region Min/Max/Clamp Functions

        [Fact]
        public void Min_ParsesAsFunction()
        {
            var decl = GetDeclaration("div { width: min(50%, 300px); }", "width");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("min", fn.Name);
        }

        [Fact]
        public void Max_ParsesAsFunction()
        {
            var decl = GetDeclaration("div { width: max(50%, 300px); }", "width");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("max", fn.Name);
        }

        [Fact]
        public void Clamp_ParsesAsFunction()
        {
            var decl = GetDeclaration("div { width: clamp(200px, 50%, 800px); }", "width");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("clamp", fn.Name);
        }

        #endregion

        #region Var Function

        [Fact]
        public void Var_ParsesAsFunction()
        {
            var decl = GetDeclaration("div { color: var(--main-color); }", "color");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("var", fn.Name);
        }

        [Fact]
        public void Var_WithFallback_ParsesAsFunction()
        {
            var decl = GetDeclaration("div { color: var(--main-color, red); }", "color");
            var fn = Assert.IsType<CssFunctionValue>(decl.Value);
            Assert.Equal("var", fn.Name);
            Assert.True(fn.Arguments.Count >= 1);
        }

        #endregion

        #region ToString Preservation

        [Fact]
        public void Calc_ToString_ContainsCalc()
        {
            var decl = GetDeclaration("div { width: calc(100% - 20px); }", "width");
            Assert.Contains("calc", decl.Value.ToString());
        }

        [Fact]
        public void Calc_ToString_ContainsPercentage()
        {
            var decl = GetDeclaration("div { width: calc(100% - 20px); }", "width");
            Assert.Contains("100%", decl.Value.ToString());
        }

        #endregion
    }
}
