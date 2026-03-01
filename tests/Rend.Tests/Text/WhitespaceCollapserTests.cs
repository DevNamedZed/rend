using Rend.Css;
using Rend.Text.Internal;
using Xunit;

namespace Rend.Tests.Text
{
    public class WhitespaceCollapserTests
    {
        // --- Normal mode ---

        [Fact]
        public void Collapse_Normal_CollapsesConsecutiveSpaces()
        {
            var result = WhitespaceCollapser.Collapse("hello   world", CssWhiteSpace.Normal);
            Assert.Equal("hello world", result);
        }

        [Fact]
        public void Collapse_Normal_CollapsesTabsAndSpaces()
        {
            var result = WhitespaceCollapser.Collapse("hello\t\t  world", CssWhiteSpace.Normal);
            Assert.Equal("hello world", result);
        }

        [Fact]
        public void Collapse_Normal_CollapsesNewlines()
        {
            var result = WhitespaceCollapser.Collapse("hello\n\nworld", CssWhiteSpace.Normal);
            Assert.Equal("hello world", result);
        }

        [Fact]
        public void Collapse_Normal_PreservesLeadingSpace()
        {
            // Edge trimming is now handled by InlineFormattingContext, not the collapser.
            var result = WhitespaceCollapser.Collapse("  hello", CssWhiteSpace.Normal);
            Assert.Equal(" hello", result);
        }

        [Fact]
        public void Collapse_Normal_PreservesTrailingSpace()
        {
            var result = WhitespaceCollapser.Collapse("hello  ", CssWhiteSpace.Normal);
            Assert.Equal("hello ", result);
        }

        [Fact]
        public void Collapse_Normal_EmptyString_ReturnsEmpty()
        {
            var result = WhitespaceCollapser.Collapse("", CssWhiteSpace.Normal);
            Assert.Equal("", result);
        }

        [Fact]
        public void Collapse_Normal_OnlyWhitespace_ReturnsSingleSpace()
        {
            // Whitespace-only text collapses to a single space; edge trimming is contextual.
            var result = WhitespaceCollapser.Collapse("   \t\n  ", CssWhiteSpace.Normal);
            Assert.Equal(" ", result);
        }

        [Fact]
        public void Collapse_Normal_MixedWhitespace()
        {
            var result = WhitespaceCollapser.Collapse("  hello \n\t world  \r\n end  ", CssWhiteSpace.Normal);
            Assert.Equal(" hello world end ", result);
        }

        [Fact]
        public void Collapse_Normal_SingleWord()
        {
            var result = WhitespaceCollapser.Collapse("hello", CssWhiteSpace.Normal);
            Assert.Equal("hello", result);
        }

        // --- Nowrap mode (same behavior as Normal) ---

        [Fact]
        public void Collapse_Nowrap_CollapsesSpaces()
        {
            var result = WhitespaceCollapser.Collapse("hello   world", CssWhiteSpace.Nowrap);
            Assert.Equal("hello world", result);
        }

        [Fact]
        public void Collapse_Nowrap_CollapsesNewlines()
        {
            var result = WhitespaceCollapser.Collapse("hello\nworld", CssWhiteSpace.Nowrap);
            Assert.Equal("hello world", result);
        }

        // --- Pre mode ---

        [Fact]
        public void Collapse_Pre_PreservesAllWhitespace()
        {
            var result = WhitespaceCollapser.Collapse("hello   world", CssWhiteSpace.Pre);
            Assert.Equal("hello   world", result);
        }

        [Fact]
        public void Collapse_Pre_PreservesNewlines()
        {
            var result = WhitespaceCollapser.Collapse("hello\n\nworld", CssWhiteSpace.Pre);
            Assert.Equal("hello\n\nworld", result);
        }

        [Fact]
        public void Collapse_Pre_PreservesTabs()
        {
            var result = WhitespaceCollapser.Collapse("hello\tworld", CssWhiteSpace.Pre);
            Assert.Equal("hello\tworld", result);
        }

        [Fact]
        public void Collapse_Pre_PreservesLeadingTrailing()
        {
            var result = WhitespaceCollapser.Collapse("  hello  ", CssWhiteSpace.Pre);
            Assert.Equal("  hello  ", result);
        }

        // --- PreWrap mode (same as Pre: preserves everything) ---

        [Fact]
        public void Collapse_PreWrap_PreservesAllWhitespace()
        {
            var result = WhitespaceCollapser.Collapse("hello   world\n\ntest", CssWhiteSpace.PreWrap);
            Assert.Equal("hello   world\n\ntest", result);
        }

        // --- PreLine mode ---

        [Fact]
        public void Collapse_PreLine_CollapsesSpacesButPreservesNewlines()
        {
            var result = WhitespaceCollapser.Collapse("hello   world\nnew line", CssWhiteSpace.PreLine);
            Assert.Equal("hello world\nnew line", result);
        }

        [Fact]
        public void Collapse_PreLine_RemovesTrailingSpaceBeforeNewline()
        {
            var result = WhitespaceCollapser.Collapse("hello   \nworld", CssWhiteSpace.PreLine);
            Assert.Equal("hello\nworld", result);
        }

        [Fact]
        public void Collapse_PreLine_CollapsesTabsToSpace()
        {
            var result = WhitespaceCollapser.Collapse("hello\t\tworld", CssWhiteSpace.PreLine);
            Assert.Equal("hello world", result);
        }

        [Fact]
        public void Collapse_PreLine_PreservesMultipleNewlines()
        {
            var result = WhitespaceCollapser.Collapse("hello\n\nworld", CssWhiteSpace.PreLine);
            Assert.Equal("hello\n\nworld", result);
        }

        [Fact]
        public void Collapse_PreLine_CRLFToLF()
        {
            var result = WhitespaceCollapser.Collapse("hello\r\nworld", CssWhiteSpace.PreLine);
            Assert.Equal("hello\nworld", result);
        }

        // --- BreakSpaces mode (preserves all whitespace) ---

        [Fact]
        public void Collapse_BreakSpaces_PreservesAll()
        {
            var result = WhitespaceCollapser.Collapse("hello   world\n\ntest", CssWhiteSpace.BreakSpaces);
            Assert.Equal("hello   world\n\ntest", result);
        }

        // --- Null handling ---

        [Fact]
        public void Collapse_NullText_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => WhitespaceCollapser.Collapse(null!, CssWhiteSpace.Normal));
        }
    }
}
