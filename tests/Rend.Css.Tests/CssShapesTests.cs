using Xunit;

namespace Rend.Css.Tests
{
    public class CssShapesTests
    {
        // ═══════════════════════════════════════════
        // shape-outside
        // ═══════════════════════════════════════════

        [Fact]
        public void ShapeOutside_None_Parsed()
        {
            var css = ".box { shape-outside: none; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "shape-outside");
        }

        [Fact]
        public void ShapeOutside_Circle_Parsed()
        {
            var css = ".box { shape-outside: circle(50%); }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "shape-outside");
        }

        [Fact]
        public void ShapeOutside_Ellipse_Parsed()
        {
            var css = ".box { shape-outside: ellipse(25% 50%); }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "shape-outside");
        }

        [Fact]
        public void ShapeOutside_Inset_Parsed()
        {
            var css = ".box { shape-outside: inset(10px 20px 30px 40px); }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "shape-outside");
        }

        [Fact]
        public void ShapeOutside_Polygon_Parsed()
        {
            var css = ".box { shape-outside: polygon(50% 0%, 100% 100%, 0% 100%); }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "shape-outside");
        }

        [Fact]
        public void ShapeOutside_MarginBox_Parsed()
        {
            var css = ".box { shape-outside: margin-box; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "shape-outside");
        }

        // ═══════════════════════════════════════════
        // shape-margin
        // ═══════════════════════════════════════════

        [Fact]
        public void ShapeMargin_Pixels_Parsed()
        {
            var css = ".box { shape-margin: 10px; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "shape-margin");
        }

        [Fact]
        public void ShapeMargin_Em_Parsed()
        {
            var css = ".box { shape-margin: 1.5em; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "shape-margin");
        }

        // ═══════════════════════════════════════════
        // shape-image-threshold
        // ═══════════════════════════════════════════

        [Fact]
        public void ShapeImageThreshold_Zero_Parsed()
        {
            var css = ".box { shape-image-threshold: 0; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "shape-image-threshold");
        }

        [Fact]
        public void ShapeImageThreshold_Decimal_Parsed()
        {
            var css = ".box { shape-image-threshold: 0.5; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "shape-image-threshold");
        }

        // ═══════════════════════════════════════════
        // Combined usage
        // ═══════════════════════════════════════════

        [Fact]
        public void ShapeOutside_WithShapeMargin_Combined()
        {
            var css = ".box { float: left; shape-outside: circle(50%); shape-margin: 10px; }";
            var sheet = CssParser.Parse(css);
            var sr = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Contains(sr.Declarations, d => d.Property == "shape-outside");
            Assert.Contains(sr.Declarations, d => d.Property == "shape-margin");
            Assert.Contains(sr.Declarations, d => d.Property == "float");
        }
    }
}
