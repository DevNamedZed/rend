using Rend.Css;
using Xunit;

namespace Rend.Css.Tests
{
    public class UnicodeRangeTests
    {
        // ═══════════════════════════════════════════
        // UnicodeRange.Parse
        // ═══════════════════════════════════════════

        [Fact]
        public void Parse_SingleCodePoint()
        {
            var ranges = UnicodeRange.Parse("U+41");
            Assert.Single(ranges);
            Assert.Equal(0x41, ranges[0].Start);
            Assert.Equal(0x41, ranges[0].End);
        }

        [Fact]
        public void Parse_Range()
        {
            var ranges = UnicodeRange.Parse("U+0-7F");
            Assert.Single(ranges);
            Assert.Equal(0, ranges[0].Start);
            Assert.Equal(0x7F, ranges[0].End);
        }

        [Fact]
        public void Parse_Wildcard()
        {
            var ranges = UnicodeRange.Parse("U+4??");
            Assert.Single(ranges);
            Assert.Equal(0x400, ranges[0].Start);
            Assert.Equal(0x4FF, ranges[0].End);
        }

        [Fact]
        public void Parse_CommaSeparated()
        {
            var ranges = UnicodeRange.Parse("U+0-7F, U+0400-04FF");
            Assert.Equal(2, ranges.Count);
            Assert.Equal(0, ranges[0].Start);
            Assert.Equal(0x7F, ranges[0].End);
            Assert.Equal(0x400, ranges[1].Start);
            Assert.Equal(0x4FF, ranges[1].End);
        }

        [Fact]
        public void Parse_FullRange()
        {
            var ranges = UnicodeRange.Parse("U+0-10FFFF");
            Assert.Single(ranges);
            Assert.Equal(0, ranges[0].Start);
            Assert.Equal(0x10FFFF, ranges[0].End);
        }

        [Fact]
        public void Parse_Lowercase()
        {
            var ranges = UnicodeRange.Parse("u+0-ff");
            Assert.Single(ranges);
            Assert.Equal(0, ranges[0].Start);
            Assert.Equal(0xFF, ranges[0].End);
        }

        [Fact]
        public void Parse_Empty_ReturnsEmpty()
        {
            var ranges = UnicodeRange.Parse("");
            Assert.Empty(ranges);
        }

        [Fact]
        public void Parse_MultipleWildcards()
        {
            var ranges = UnicodeRange.Parse("U+00??");
            Assert.Single(ranges);
            Assert.Equal(0x0000, ranges[0].Start);
            Assert.Equal(0x00FF, ranges[0].End);
        }

        [Fact]
        public void Parse_LatinExtended()
        {
            var ranges = UnicodeRange.Parse("U+0100-024F");
            Assert.Single(ranges);
            Assert.Equal(0x100, ranges[0].Start);
            Assert.Equal(0x24F, ranges[0].End);
        }

        [Fact]
        public void Contains_InRange()
        {
            var range = new UnicodeRange(0x41, 0x5A);
            Assert.True(range.Contains(0x41));  // 'A'
            Assert.True(range.Contains(0x4D));  // 'M'
            Assert.True(range.Contains(0x5A));  // 'Z'
        }

        [Fact]
        public void Contains_OutOfRange()
        {
            var range = new UnicodeRange(0x41, 0x5A);
            Assert.False(range.Contains(0x40));
            Assert.False(range.Contains(0x5B));
            Assert.False(range.Contains(0x61));
        }

        [Fact]
        public void ToString_SingleCodePoint()
        {
            var range = new UnicodeRange(0x41, 0x41);
            Assert.Equal("U+41", range.ToString());
        }

        [Fact]
        public void ToString_Range()
        {
            var range = new UnicodeRange(0, 0x7F);
            Assert.Equal("U+0-7F", range.ToString());
        }

        // ═══════════════════════════════════════════
        // @font-face unicode-range parsing
        // ═══════════════════════════════════════════

        [Fact]
        public void FontFace_UnicodeRange_Parsed()
        {
            var css = "@font-face { font-family: 'MyFont'; unicode-range: U+0-7F; }";
            var sheet = CssParser.Parse(css);
            var ff = Assert.IsType<FontFaceRule>(sheet.Rules[0]);
            Assert.Contains(ff.Declarations, d => d.Property == "unicode-range");
        }

        [Fact]
        public void FontFace_UnicodeRanges_Property()
        {
            var css = "@font-face { font-family: 'MyFont'; unicode-range: U+0-7F, U+0400-04FF; }";
            var sheet = CssParser.Parse(css);
            var ff = Assert.IsType<FontFaceRule>(sheet.Rules[0]);
            var ranges = ff.UnicodeRanges;
            Assert.NotNull(ranges);
            Assert.Equal(2, ranges!.Count);
        }

        [Fact]
        public void FontFace_NoUnicodeRange_ReturnsNull()
        {
            var css = "@font-face { font-family: 'MyFont'; src: url('myfont.woff2'); }";
            var sheet = CssParser.Parse(css);
            var ff = Assert.IsType<FontFaceRule>(sheet.Rules[0]);
            Assert.Null(ff.UnicodeRanges);
        }

        [Fact]
        public void FontFace_UnicodeRange_Multiple()
        {
            var css = "@font-face { font-family: 'Icons'; unicode-range: U+E000-E0FF, U+F000-F0FF, U+F400-F4FF; }";
            var sheet = CssParser.Parse(css);
            var ff = Assert.IsType<FontFaceRule>(sheet.Rules[0]);
            var ranges = ff.UnicodeRanges;
            Assert.NotNull(ranges);
            Assert.Equal(3, ranges!.Count);
            Assert.Equal(0xE000, ranges[0].Start);
            Assert.Equal(0xE0FF, ranges[0].End);
        }
    }
}
