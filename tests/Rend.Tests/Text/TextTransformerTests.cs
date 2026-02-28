using Rend.Css;
using Rend.Text.Internal;
using Xunit;

namespace Rend.Tests.Text
{
    public class TextTransformerTests
    {
        [Fact]
        public void Transform_None_ReturnsOriginal()
        {
            var result = TextTransformer.Transform("Hello World", CssTextTransform.None);
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void Transform_Uppercase_ConvertsToUpper()
        {
            var result = TextTransformer.Transform("Hello World", CssTextTransform.Uppercase);
            Assert.Equal("HELLO WORLD", result);
        }

        [Fact]
        public void Transform_Lowercase_ConvertsToLower()
        {
            var result = TextTransformer.Transform("Hello World", CssTextTransform.Lowercase);
            Assert.Equal("hello world", result);
        }

        [Fact]
        public void Transform_Capitalize_CapitalizesFirstLetterOfEachWord()
        {
            var result = TextTransformer.Transform("hello world", CssTextTransform.Capitalize);
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void Transform_Capitalize_PreservesExistingCase()
        {
            var result = TextTransformer.Transform("hELLO wORLD", CssTextTransform.Capitalize);
            Assert.Equal("HELLO WORLD", result);
        }

        [Fact]
        public void Transform_Capitalize_MultipleSpaces()
        {
            var result = TextTransformer.Transform("hello  world", CssTextTransform.Capitalize);
            Assert.Equal("Hello  World", result);
        }

        [Fact]
        public void Transform_Capitalize_SingleWord()
        {
            var result = TextTransformer.Transform("hello", CssTextTransform.Capitalize);
            Assert.Equal("Hello", result);
        }

        [Fact]
        public void Transform_Capitalize_LeadingWhitespace()
        {
            var result = TextTransformer.Transform("  hello", CssTextTransform.Capitalize);
            Assert.Equal("  Hello", result);
        }

        [Fact]
        public void Transform_Capitalize_TabSeparated()
        {
            var result = TextTransformer.Transform("hello\tworld", CssTextTransform.Capitalize);
            Assert.Equal("Hello\tWorld", result);
        }

        [Fact]
        public void Transform_EmptyString_ReturnsEmpty()
        {
            Assert.Equal("", TextTransformer.Transform("", CssTextTransform.Uppercase));
            Assert.Equal("", TextTransformer.Transform("", CssTextTransform.Lowercase));
            Assert.Equal("", TextTransformer.Transform("", CssTextTransform.Capitalize));
            Assert.Equal("", TextTransformer.Transform("", CssTextTransform.None));
        }

        [Fact]
        public void Transform_NullText_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => TextTransformer.Transform(null!, CssTextTransform.None));
        }

        [Fact]
        public void Transform_Uppercase_WithNumbers()
        {
            var result = TextTransformer.Transform("hello123", CssTextTransform.Uppercase);
            Assert.Equal("HELLO123", result);
        }

        [Fact]
        public void Transform_Lowercase_WithNumbers()
        {
            var result = TextTransformer.Transform("HELLO123", CssTextTransform.Lowercase);
            Assert.Equal("hello123", result);
        }

        [Fact]
        public void Transform_Capitalize_AlreadyCapitalized()
        {
            var result = TextTransformer.Transform("Hello World", CssTextTransform.Capitalize);
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void Transform_Capitalize_NewlineSeparated()
        {
            var result = TextTransformer.Transform("hello\nworld", CssTextTransform.Capitalize);
            Assert.Equal("Hello\nWorld", result);
        }
    }
}
