using Rend.Text;
using Xunit;

namespace Rend.Tests.Text
{
    public class ShapedGlyphTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            var glyph = new ShapedGlyph(
                glyphId: 42,
                cluster: 3,
                xAdvance: 8.5f,
                yAdvance: 0f,
                xOffset: 1.0f,
                yOffset: -0.5f);

            Assert.Equal(42u, glyph.GlyphId);
            Assert.Equal(3u, glyph.Cluster);
            Assert.Equal(8.5f, glyph.XAdvance);
            Assert.Equal(0f, glyph.YAdvance);
            Assert.Equal(1.0f, glyph.XOffset);
            Assert.Equal(-0.5f, glyph.YOffset);
        }

        [Fact]
        public void Constructor_ZeroValues()
        {
            var glyph = new ShapedGlyph(0, 0, 0f, 0f, 0f, 0f);

            Assert.Equal(0u, glyph.GlyphId);
            Assert.Equal(0u, glyph.Cluster);
            Assert.Equal(0f, glyph.XAdvance);
            Assert.Equal(0f, glyph.YAdvance);
            Assert.Equal(0f, glyph.XOffset);
            Assert.Equal(0f, glyph.YOffset);
        }

        [Fact]
        public void Constructor_LargeGlyphId()
        {
            var glyph = new ShapedGlyph(65535, 100, 12.5f, 0f, 0f, 0f);
            Assert.Equal(65535u, glyph.GlyphId);
        }

        [Fact]
        public void ShapedTextRun_ComputesTotalWidth()
        {
            var glyphs = new[]
            {
                new ShapedGlyph(1, 0, 10f, 0f, 0f, 0f),
                new ShapedGlyph(2, 1, 8f, 0f, 0f, 0f),
                new ShapedGlyph(3, 2, 12f, 0f, 0f, 0f),
            };

            var run = new ShapedTextRun(glyphs, "abc", 16f);

            Assert.Equal(30f, run.TotalWidth);
            Assert.Equal("abc", run.OriginalText);
            Assert.Equal(16f, run.FontSize);
            Assert.Equal(3, run.Glyphs.Length);
        }

        [Fact]
        public void ShapedTextRun_EmptyGlyphs_ZeroWidth()
        {
            var glyphs = System.Array.Empty<ShapedGlyph>();
            var run = new ShapedTextRun(glyphs, "", 16f);

            Assert.Equal(0f, run.TotalWidth);
            Assert.Empty(run.Glyphs);
        }

        [Fact]
        public void ShapedTextRun_NullGlyphs_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => new ShapedTextRun(null!, "test", 16f));
        }

        [Fact]
        public void ShapedTextRun_NullText_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => new ShapedTextRun(System.Array.Empty<ShapedGlyph>(), null!, 16f));
        }
    }
}
