using System.Collections.Generic;
using System.Linq;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Css.Resolution.Internal;
using Xunit;

namespace Rend.Css.Tests
{
    public class CalcEvaluationTests
    {
        // ═══════════════════════════════════════════
        // Basic calc() via CSS parsing + style resolution
        // ═══════════════════════════════════════════

        [Fact]
        public void Calc_SimpleAddition()
        {
            // calc(100px + 50px) = 150px
            float result = ResolveCalcValue("calc(100px + 50px)");
            Assert.Equal(150f, result, 0.01);
        }

        [Fact]
        public void Calc_SimpleSubtraction()
        {
            // calc(200px - 50px) = 150px
            float result = ResolveCalcValue("calc(200px - 50px)");
            Assert.Equal(150f, result, 0.01);
        }

        [Fact]
        public void Calc_Multiplication()
        {
            // calc(10px * 5) = 50px
            float result = ResolveCalcValue("calc(10px * 5)");
            Assert.Equal(50f, result, 0.01);
        }

        [Fact]
        public void Calc_Division()
        {
            // calc(100px / 4) = 25px
            float result = ResolveCalcValue("calc(100px / 4)");
            Assert.Equal(25f, result, 0.01);
        }

        [Fact]
        public void Calc_PercentageMixed()
        {
            // calc(50% - 20px) with container 400px → 200 - 20 = 180px
            float result = ResolveCalcValue("calc(50% - 20px)", percentBase: 400f);
            Assert.Equal(180f, result, 0.01);
        }

        [Fact]
        public void Calc_OperatorPrecedence()
        {
            // calc(100px + 10px * 3) → 100 + 30 = 130 (not (100+10)*3 = 330)
            float result = ResolveCalcValue("calc(100px + 10px * 3)");
            Assert.Equal(130f, result, 0.01);
        }

        [Fact]
        public void Calc_MultipleOperations()
        {
            // calc(100px + 50px - 20px) = 130px
            float result = ResolveCalcValue("calc(100px + 50px - 20px)");
            Assert.Equal(130f, result, 0.01);
        }

        [Fact]
        public void Calc_EmUnits()
        {
            // calc(2em + 10px) with fontSize=16 → 32 + 10 = 42px
            float result = ResolveCalcValue("calc(2em + 10px)", fontSize: 16f);
            Assert.Equal(42f, result, 0.01);
        }

        [Fact]
        public void Calc_SingleValue()
        {
            // calc(100px) = 100px
            float result = ResolveCalcValue("calc(100px)");
            Assert.Equal(100f, result, 0.01);
        }

        [Fact]
        public void Calc_PercentageOnly()
        {
            // calc(100%) with container 500px → 500px
            float result = ResolveCalcValue("calc(100%)", percentBase: 500f);
            Assert.Equal(500f, result, 0.01);
        }

        // ═══════════════════════════════════════════
        // Parser preservation of operators
        // ═══════════════════════════════════════════

        [Fact]
        public void Parser_PreservesCalcOperators()
        {
            var parser = new Rend.Css.Parser.Internal.CssValueParser(
                Tokenize("calc(100px + 50px)"), 100);
            var value = parser.Parse();

            Assert.IsType<CssFunctionValue>(value);
            var fn = (CssFunctionValue)value;
            Assert.Equal("calc", fn.Name);
            Assert.True(fn.Arguments.Count >= 3,
                $"Expected at least 3 arguments (value op value), got {fn.Arguments.Count}");

            // Should have: 100px, +, 50px
            Assert.IsType<CssDimensionValue>(fn.Arguments[0]);
            Assert.IsType<CssKeywordValue>(fn.Arguments[1]);
            Assert.Equal("+", ((CssKeywordValue)fn.Arguments[1]).Keyword);
            Assert.IsType<CssDimensionValue>(fn.Arguments[2]);
        }

        [Fact]
        public void Parser_PreservesCalcSubtraction()
        {
            var parser = new Rend.Css.Parser.Internal.CssValueParser(
                Tokenize("calc(100% - 20px)"), 100);
            var value = parser.Parse();

            Assert.IsType<CssFunctionValue>(value);
            var fn = (CssFunctionValue)value;
            Assert.True(fn.Arguments.Count >= 3);
            Assert.IsType<CssKeywordValue>(fn.Arguments[1]);
            Assert.Equal("-", ((CssKeywordValue)fn.Arguments[1]).Keyword);
        }

        // ═══════════════════════════════════════════
        // min() / max() / clamp() evaluation
        // ═══════════════════════════════════════════

        [Fact]
        public void Min_TwoValues_ReturnsSmaller()
        {
            float result = ResolveCalcValue("min(100px, 200px)");
            Assert.Equal(100f, result, 0.01);
        }

        [Fact]
        public void Min_WithPercentage()
        {
            // min(50%, 300px) with container 400px → min(200, 300) = 200
            float result = ResolveCalcValue("min(50%, 300px)", percentBase: 400f);
            Assert.Equal(200f, result, 0.01);
        }

        [Fact]
        public void Max_TwoValues_ReturnsLarger()
        {
            float result = ResolveCalcValue("max(100px, 200px)");
            Assert.Equal(200f, result, 0.01);
        }

        [Fact]
        public void Max_WithPercentage()
        {
            // max(50%, 300px) with container 400px → max(200, 300) = 300
            float result = ResolveCalcValue("max(50%, 300px)", percentBase: 400f);
            Assert.Equal(300f, result, 0.01);
        }

        [Fact]
        public void Clamp_ValueInRange()
        {
            // clamp(100px, 150px, 200px) → 150
            float result = ResolveCalcValue("clamp(100px, 150px, 200px)");
            Assert.Equal(150f, result, 0.01);
        }

        [Fact]
        public void Clamp_ValueBelowMin()
        {
            // clamp(100px, 50px, 200px) → 100 (clamped to min)
            float result = ResolveCalcValue("clamp(100px, 50px, 200px)");
            Assert.Equal(100f, result, 0.01);
        }

        [Fact]
        public void Clamp_ValueAboveMax()
        {
            // clamp(100px, 300px, 200px) → 200 (clamped to max)
            float result = ResolveCalcValue("clamp(100px, 300px, 200px)");
            Assert.Equal(200f, result, 0.01);
        }

        [Fact]
        public void Clamp_WithPercentage()
        {
            // clamp(50px, 50%, 300px) with container 400px → clamp(50, 200, 300) = 200
            float result = ResolveCalcValue("clamp(50px, 50%, 300px)", percentBase: 400f);
            Assert.Equal(200f, result, 0.01);
        }

        // ═══════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════

        private static float ResolveCalcValue(string cssText,
            float fontSize = 16f, float percentBase = 0f)
        {
            var tokens = Tokenize(cssText);
            var parser = new Rend.Css.Parser.Internal.CssValueParser(tokens, tokens.Length);
            var value = parser.Parse();

            var ctx = new CssResolutionContext(fontSize, fontSize, 800, 600, percentBase);

            var prop = PropertyRegistry.GetByName("width")!;
            if (ValueResolver.TryResolve(value, prop, ctx, out var pv, out var refVal))
            {
                return pv.FloatValue;
            }

            Assert.Fail("Failed to resolve calc value");
            return 0;
        }

        [Fact]
        public void BoxShadow_Inset_ParsesCorrectly()
        {
            // Parse "inset 0 2px 8px rgba(0,0,0,0.3)"
            var parser = new Rend.Css.Parser.Internal.CssValueParser(
                Tokenize("inset 0 2px 8px rgba(0,0,0,0.3)"), 100);
            var value = parser.Parse();

            // Should be a space-separated CssListValue
            Assert.IsType<CssListValue>(value);
            var list = (CssListValue)value;
            Assert.Equal(' ', list.Separator);

            // First value should be CssKeywordValue("inset")
            Assert.True(list.Values.Count >= 2,
                $"Expected at least 2 values, got {list.Values.Count}: {string.Join(", ", list.Values.Select(v => $"{v.GetType().Name}({v})"))}");
            Assert.IsType<CssKeywordValue>(list.Values[0]);
            Assert.Equal("inset", ((CssKeywordValue)list.Values[0]).Keyword);
        }

        private static Rend.Css.Parser.Internal.CssToken[] Tokenize(string text)
        {
            var tokenizer = new Rend.Css.Parser.Internal.CssTokenizer(text);
            var tokens = new List<Rend.Css.Parser.Internal.CssToken>();
            var token = new Rend.Css.Parser.Internal.CssToken();
            while (tokenizer.Read(ref token))
            {
                tokens.Add(token);
                if (token.Type == Rend.Css.Parser.Internal.CssTokenType.EOF)
                    break;
            }
            // Ensure we have an EOF token
            if (tokens.Count == 0 || tokens[tokens.Count - 1].Type != Rend.Css.Parser.Internal.CssTokenType.EOF)
            {
                token.Type = Rend.Css.Parser.Internal.CssTokenType.EOF;
                tokens.Add(token);
            }
            return tokens.ToArray();
        }
    }
}
