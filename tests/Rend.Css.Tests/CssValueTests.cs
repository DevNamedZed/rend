using System.Collections.Generic;
using Rend.Core.Values;
using Xunit;

namespace Rend.Css.Tests
{
    public class CssValueTests
    {
        #region CssKeywordValue

        [Fact]
        public void CssKeywordValue_Kind_ReturnsKeyword()
        {
            var kw = new CssKeywordValue("block");
            Assert.Equal(CssValueKind.Keyword, kw.Kind);
        }

        [Fact]
        public void CssKeywordValue_Keyword_ReturnsStoredValue()
        {
            var kw = new CssKeywordValue("auto");
            Assert.Equal("auto", kw.Keyword);
        }

        [Fact]
        public void CssKeywordValue_ToString_ReturnsKeyword()
        {
            var kw = new CssKeywordValue("inherit");
            Assert.Equal("inherit", kw.ToString());
        }

        [Fact]
        public void CssKeywordValue_NullKeyword_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => new CssKeywordValue(null!));
        }

        #endregion

        #region CssNumberValue

        [Fact]
        public void CssNumberValue_Kind_ReturnsNumber()
        {
            var num = new CssNumberValue(42);
            Assert.Equal(CssValueKind.Number, num.Kind);
        }

        [Fact]
        public void CssNumberValue_Value_ReturnsStoredValue()
        {
            var num = new CssNumberValue(3.14f);
            Assert.Equal(3.14f, num.Value, 0.001f);
        }

        [Fact]
        public void CssNumberValue_IsInteger_ReturnsTrueWhenSet()
        {
            var num = new CssNumberValue(5, isInteger: true);
            Assert.True(num.IsInteger);
        }

        [Fact]
        public void CssNumberValue_IsInteger_ReturnsFalseByDefault()
        {
            var num = new CssNumberValue(5.5f);
            Assert.False(num.IsInteger);
        }

        [Fact]
        public void CssNumberValue_ToString_IntegerFormat()
        {
            var num = new CssNumberValue(42, isInteger: true);
            Assert.Equal("42", num.ToString());
        }

        [Fact]
        public void CssNumberValue_ToString_FloatFormat()
        {
            var num = new CssNumberValue(0.5f);
            Assert.Equal("0.5", num.ToString());
        }

        [Fact]
        public void CssNumberValue_Zero_ParsesCorrectly()
        {
            var num = new CssNumberValue(0f, isInteger: true);
            Assert.Equal(0f, num.Value);
            Assert.Equal("0", num.ToString());
        }

        #endregion

        #region CssDimensionValue

        [Fact]
        public void CssDimensionValue_Kind_ReturnsDimension()
        {
            var dim = new CssDimensionValue(10f, "px");
            Assert.Equal(CssValueKind.Dimension, dim.Kind);
        }

        [Fact]
        public void CssDimensionValue_ValueAndUnit_ReturnStoredValues()
        {
            var dim = new CssDimensionValue(16f, "em");
            Assert.Equal(16f, dim.Value);
            Assert.Equal("em", dim.Unit);
        }

        [Fact]
        public void CssDimensionValue_ToString_FormatsCorrectly()
        {
            var dim = new CssDimensionValue(10f, "px");
            Assert.Equal("10px", dim.ToString());
        }

        [Fact]
        public void CssDimensionValue_NegativeValue_FormatsCorrectly()
        {
            var dim = new CssDimensionValue(-5f, "px");
            Assert.Contains("-5", dim.ToString());
            Assert.Contains("px", dim.ToString());
        }

        [Fact]
        public void CssDimensionValue_NullUnit_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => new CssDimensionValue(10f, null!));
        }

        [Theory]
        [InlineData(12f, "pt")]
        [InlineData(2.5f, "em")]
        [InlineData(1f, "rem")]
        [InlineData(100f, "vw")]
        public void CssDimensionValue_VariousUnits_StoreCorrectly(float value, string unit)
        {
            var dim = new CssDimensionValue(value, unit);
            Assert.Equal(value, dim.Value);
            Assert.Equal(unit, dim.Unit);
        }

        #endregion

        #region CssPercentageValue

        [Fact]
        public void CssPercentageValue_Kind_ReturnsPercentage()
        {
            var pct = new CssPercentageValue(50f);
            Assert.Equal(CssValueKind.Percentage, pct.Kind);
        }

        [Fact]
        public void CssPercentageValue_Value_ReturnsStoredValue()
        {
            var pct = new CssPercentageValue(75f);
            Assert.Equal(75f, pct.Value);
        }

        [Fact]
        public void CssPercentageValue_ToString_IncludesPercentSign()
        {
            var pct = new CssPercentageValue(50f);
            Assert.Equal("50%", pct.ToString());
        }

        [Fact]
        public void CssPercentageValue_Zero_FormatsCorrectly()
        {
            var pct = new CssPercentageValue(0f);
            Assert.Equal("0%", pct.ToString());
        }

        [Fact]
        public void CssPercentageValue_Hundred_FormatsCorrectly()
        {
            var pct = new CssPercentageValue(100f);
            Assert.Equal("100%", pct.ToString());
        }

        #endregion

        #region CssColorValue

        [Fact]
        public void CssColorValue_Kind_ReturnsColor()
        {
            var color = new CssColorValue(new CssColor(255, 0, 0));
            Assert.Equal(CssValueKind.Color, color.Kind);
        }

        [Fact]
        public void CssColorValue_Color_ReturnsStoredColor()
        {
            var cssColor = new CssColor(128, 64, 32);
            var val = new CssColorValue(cssColor);
            Assert.Equal(128, val.Color.R);
            Assert.Equal(64, val.Color.G);
            Assert.Equal(32, val.Color.B);
            Assert.Equal(255, val.Color.A);
        }

        [Fact]
        public void CssColorValue_WithAlpha_PreservesAlpha()
        {
            var cssColor = new CssColor(255, 0, 0, 128);
            var val = new CssColorValue(cssColor);
            Assert.Equal(128, val.Color.A);
        }

        [Fact]
        public void CssColorValue_ToString_DelegatestoColor()
        {
            var cssColor = new CssColor(255, 0, 0);
            var val = new CssColorValue(cssColor);
            // CssColor.ToString() returns "rgb(255, 0, 0)" for full alpha
            Assert.Contains("255", val.ToString());
        }

        #endregion

        #region CssStringValue

        [Fact]
        public void CssStringValue_Kind_ReturnsString()
        {
            var str = new CssStringValue("hello");
            Assert.Equal(CssValueKind.String, str.Kind);
        }

        [Fact]
        public void CssStringValue_Value_ReturnsStoredValue()
        {
            var str = new CssStringValue("world");
            Assert.Equal("world", str.Value);
        }

        [Fact]
        public void CssStringValue_ToString_IncludesQuotes()
        {
            var str = new CssStringValue("hello");
            Assert.Equal("\"hello\"", str.ToString());
        }

        [Fact]
        public void CssStringValue_NullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => new CssStringValue(null!));
        }

        [Fact]
        public void CssStringValue_EmptyString_IsValid()
        {
            var str = new CssStringValue("");
            Assert.Equal("", str.Value);
            Assert.Equal("\"\"", str.ToString());
        }

        #endregion

        #region CssUrlValue

        [Fact]
        public void CssUrlValue_Kind_ReturnsUrl()
        {
            var url = new CssUrlValue("image.png");
            Assert.Equal(CssValueKind.Url, url.Kind);
        }

        [Fact]
        public void CssUrlValue_Url_ReturnsStoredValue()
        {
            var url = new CssUrlValue("https://example.com/image.png");
            Assert.Equal("https://example.com/image.png", url.Url);
        }

        [Fact]
        public void CssUrlValue_ToString_WrapsInUrlFunction()
        {
            var url = new CssUrlValue("bg.jpg");
            Assert.Equal("url(bg.jpg)", url.ToString());
        }

        [Fact]
        public void CssUrlValue_NullUrl_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => new CssUrlValue(null!));
        }

        #endregion

        #region CssFunctionValue

        [Fact]
        public void CssFunctionValue_Kind_ReturnsFunction()
        {
            var fn = new CssFunctionValue("rgb", new List<CssValue>());
            Assert.Equal(CssValueKind.Function, fn.Kind);
        }

        [Fact]
        public void CssFunctionValue_Name_ReturnsStoredName()
        {
            var fn = new CssFunctionValue("calc", new List<CssValue>());
            Assert.Equal("calc", fn.Name);
        }

        [Fact]
        public void CssFunctionValue_Arguments_ReturnsStoredArguments()
        {
            var args = new List<CssValue>
            {
                new CssPercentageValue(100),
                new CssDimensionValue(20, "px")
            };
            var fn = new CssFunctionValue("calc", args);
            Assert.Equal(2, fn.Arguments.Count);
        }

        [Fact]
        public void CssFunctionValue_ToString_FormatsWithParens()
        {
            var args = new List<CssValue>
            {
                new CssNumberValue(255, true),
                new CssNumberValue(0, true),
                new CssNumberValue(0, true)
            };
            var fn = new CssFunctionValue("rgb", args);
            var str = fn.ToString();
            Assert.StartsWith("rgb(", str);
            Assert.EndsWith(")", str);
        }

        [Fact]
        public void CssFunctionValue_NullName_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => new CssFunctionValue(null!, new List<CssValue>()));
        }

        [Fact]
        public void CssFunctionValue_NullArguments_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => new CssFunctionValue("fn", null!));
        }

        [Fact]
        public void CssFunctionValue_EmptyArguments_IsValid()
        {
            var fn = new CssFunctionValue("var", new List<CssValue>());
            Assert.Empty(fn.Arguments);
            Assert.Equal("var()", fn.ToString());
        }

        #endregion

        #region CssListValue

        [Fact]
        public void CssListValue_Kind_ReturnsList()
        {
            var list = new CssListValue(new List<CssValue>());
            Assert.Equal(CssValueKind.List, list.Kind);
        }

        [Fact]
        public void CssListValue_Values_ReturnsStoredValues()
        {
            var values = new List<CssValue>
            {
                new CssDimensionValue(10, "px"),
                new CssDimensionValue(20, "px")
            };
            var list = new CssListValue(values);
            Assert.Equal(2, list.Values.Count);
        }

        [Fact]
        public void CssListValue_SpaceSeparator_DefaultBehavior()
        {
            var values = new List<CssValue>
            {
                new CssDimensionValue(10, "px"),
                new CssDimensionValue(20, "px")
            };
            var list = new CssListValue(values);
            Assert.Equal(' ', list.Separator);
            Assert.Contains(" ", list.ToString());
        }

        [Fact]
        public void CssListValue_CommaSeparator_FormatsCorrectly()
        {
            var values = new List<CssValue>
            {
                new CssKeywordValue("Arial"),
                new CssKeywordValue("sans-serif")
            };
            var list = new CssListValue(values, ',');
            Assert.Equal(',', list.Separator);
            Assert.Contains(", ", list.ToString());
        }

        [Fact]
        public void CssListValue_NullValues_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => new CssListValue(null!));
        }

        [Fact]
        public void CssListValue_SingleItem_NoSeparator()
        {
            var values = new List<CssValue> { new CssKeywordValue("bold") };
            var list = new CssListValue(values);
            Assert.Equal("bold", list.ToString());
        }

        #endregion

        #region CssDeclaration

        [Fact]
        public void CssDeclaration_Properties_SetCorrectly()
        {
            var value = new CssKeywordValue("block");
            var decl = new CssDeclaration("display", value, false);

            Assert.Equal("display", decl.Property);
            Assert.Same(value, decl.Value);
            Assert.False(decl.Important);
        }

        [Fact]
        public void CssDeclaration_Important_SetsFlag()
        {
            var value = new CssColorValue(new CssColor(255, 0, 0));
            var decl = new CssDeclaration("color", value, true);

            Assert.True(decl.Important);
        }

        [Fact]
        public void CssDeclaration_ToString_NormalDeclaration()
        {
            var value = new CssKeywordValue("block");
            var decl = new CssDeclaration("display", value);
            Assert.Equal("display: block", decl.ToString());
        }

        [Fact]
        public void CssDeclaration_ToString_ImportantDeclaration()
        {
            var value = new CssKeywordValue("red");
            var decl = new CssDeclaration("color", value, true);
            Assert.Equal("color: red !important", decl.ToString());
        }

        [Fact]
        public void CssDeclaration_DefaultImportant_IsFalse()
        {
            var decl = new CssDeclaration("margin", new CssDimensionValue(0, "px"));
            Assert.False(decl.Important);
        }

        #endregion
    }
}
