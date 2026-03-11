using Xunit;

namespace Rend.Tests.Rendering
{
    public class NewCssPropertyRenderTests
    {
        [Fact]
        public void InitialLetter_Normal_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p style='initial-letter: normal;'>
                    This paragraph has a normal initial letter, no drop cap.
                </p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void HangingPunctuation_First_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p style='hanging-punctuation: first; width: 200px;'>
                    \u201CThis is a paragraph with hanging opening quotes.\u201D
                </p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ForcedColorAdjust_None_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='forced-color-adjust: none; background: blue; color: white; padding: 10px;'>
                    This element opts out of forced color adjustment.
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

    }
}
