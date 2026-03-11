using Xunit;

namespace Rend.Tests.Rendering
{
    public class TextWrapBalanceTests
    {
        [Fact]
        public void TextWrapBalance_ShortParagraph_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p style='text-wrap: balance; width: 200px;'>
                    This is a short paragraph that should have balanced line lengths.
                </p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void TextWrapBalance_Heading_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <h2 style='text-wrap: balance; width: 300px;'>
                    A heading with balanced text wrapping across multiple lines
                </h2>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void TextWrapWrap_Default_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p style='text-wrap: wrap; width: 200px;'>
                    Normal wrapping behavior for this paragraph.
                </p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void TextWrapPretty_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p style='text-wrap: pretty; width: 200px;'>
                    Pretty text wrapping avoids orphans at the end of paragraphs.
                </p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

    }
}
