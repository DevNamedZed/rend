using Rend.Text.Internal;
using Xunit;

namespace Rend.Tests.Text
{
    public class LineBreakerTests
    {
        private readonly LineBreaker _breaker = new LineBreaker();

        [Fact]
        public void FindBreaks_EmptyString_ReturnsEmpty()
        {
            var result = _breaker.FindBreaks("");
            Assert.Empty(result);
        }

        [Fact]
        public void FindBreaks_SingleChar_ReturnsEmpty()
        {
            var result = _breaker.FindBreaks("a");
            Assert.Empty(result);
        }

        [Fact]
        public void FindBreaks_NullText_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => _breaker.FindBreaks(null!));
        }

        [Fact]
        public void FindBreaks_SpaceBetweenWords_AllowedBreak()
        {
            // "ab cd"
            // Positions: a|b = 0, b|' ' = 1, ' '|c = 2, c|d = 3
            var result = _breaker.FindBreaks("ab cd");
            Assert.Equal(4, result.Length);

            // Break is allowed after the space (position 2: between ' ' and 'c')
            Assert.Equal(LineBreakOpportunity.Allowed, result[2]);
            // No break inside words
            Assert.Equal(LineBreakOpportunity.Forbidden, result[0]);
            Assert.Equal(LineBreakOpportunity.Forbidden, result[3]);
        }

        [Fact]
        public void FindBreaks_MultipleSpaces_AllowedAfterLastSpaceBeforeWord()
        {
            // "a  b"
            var result = _breaker.FindBreaks("a  b");
            // Position 0: a|' ' -> allowed (after letter before space? No, space after a)
            // Position 1: ' '|' ' -> second space
            // Position 2: ' '|b -> allowed
            Assert.Equal(LineBreakOpportunity.Allowed, result[2]);
        }

        [Fact]
        public void FindBreaks_HyphenInWord_AllowedBreakAfterHyphen()
        {
            // "well-known"
            var result = _breaker.FindBreaks("well-known");
            // Position 4 is between '-' and 'k'
            Assert.Equal(LineBreakOpportunity.Allowed, result[4]);
        }

        [Fact]
        public void FindBreaks_MandatoryBreakAfterNewline()
        {
            // "ab\ncd"
            var result = _breaker.FindBreaks("ab\ncd");
            // Position 2 is between '\n' and 'c' - mandatory break after LF
            Assert.Equal(LineBreakOpportunity.Mandatory, result[2]);
        }

        [Fact]
        public void FindBreaks_CRLFPair_NoBreakBetweenCRLF()
        {
            // "a\r\nb"
            var result = _breaker.FindBreaks("a\r\nb");
            // Position 1: between '\r' and '\n' -> Forbidden (CR-LF pair)
            Assert.Equal(LineBreakOpportunity.Forbidden, result[1]);
            // Position 2: between '\n' and 'b' -> Mandatory
            Assert.Equal(LineBreakOpportunity.Mandatory, result[2]);
        }

        [Fact]
        public void FindBreaks_CRAlone_MandatoryBreak()
        {
            // "a\rb"
            var result = _breaker.FindBreaks("a\rb");
            // Position 1: between '\r' and 'b' -> Mandatory
            Assert.Equal(LineBreakOpportunity.Mandatory, result[1]);
        }

        [Fact]
        public void FindBreaks_NonBreakingSpace_Forbidden()
        {
            // "hello\u00A0world"
            var result = _breaker.FindBreaks("hello\u00A0world");
            // Position 4: 'o' and NBSP -> Forbidden
            Assert.Equal(LineBreakOpportunity.Forbidden, result[4]);
            // Position 5: NBSP and 'w' -> Forbidden
            Assert.Equal(LineBreakOpportunity.Forbidden, result[5]);
        }

        [Fact]
        public void FindBreaks_WordJoiner_Forbidden()
        {
            // "hello\u2060world"
            var result = _breaker.FindBreaks("hello\u2060world");
            // Position 4: 'o' and word joiner -> Forbidden
            Assert.Equal(LineBreakOpportunity.Forbidden, result[4]);
            // Position 5: word joiner and 'w' -> Forbidden
            Assert.Equal(LineBreakOpportunity.Forbidden, result[5]);
        }

        [Fact]
        public void FindBreaks_ZeroWidthSpace_AllowedBreak()
        {
            // "hello\u200Bworld"
            var result = _breaker.FindBreaks("hello\u200Bworld");
            // Position 5: between ZWSP and 'w' -> Allowed
            Assert.Equal(LineBreakOpportunity.Allowed, result[5]);
        }

        [Fact]
        public void FindBreaks_NoBreakInsideWord()
        {
            // "hello"
            var result = _breaker.FindBreaks("hello");
            Assert.All(result, b => Assert.Equal(LineBreakOpportunity.Forbidden, b));
        }

        [Fact]
        public void FindBreaks_TabBetweenWords_AllowedBreak()
        {
            // "ab\tcd"
            var result = _breaker.FindBreaks("ab\tcd");
            // Position 2: between '\t' and 'c' -> Allowed
            Assert.Equal(LineBreakOpportunity.Allowed, result[2]);
        }

        [Fact]
        public void FindBreaks_EmDash_AllowedBreakBefore()
        {
            // "hello\u2014world" (em dash)
            var result = _breaker.FindBreaks("hello\u2014world");
            // Position 4: 'o' before em dash -> Allowed
            Assert.Equal(LineBreakOpportunity.Allowed, result[4]);
        }

        [Fact]
        public void FindBreaks_LineSeparator_MandatoryBreak()
        {
            // "ab\u2028cd"
            var result = _breaker.FindBreaks("ab\u2028cd");
            // Position 2: line separator -> mandatory
            Assert.Equal(LineBreakOpportunity.Mandatory, result[2]);
        }

        [Fact]
        public void FindBreaks_ParagraphSeparator_MandatoryBreak()
        {
            // "ab\u2029cd"
            var result = _breaker.FindBreaks("ab\u2029cd");
            Assert.Equal(LineBreakOpportunity.Mandatory, result[2]);
        }
    }
}
