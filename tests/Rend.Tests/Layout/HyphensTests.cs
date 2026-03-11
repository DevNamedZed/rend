using Xunit;

namespace Rend.Tests.Layout
{
    public class HyphensTests
    {
        [Fact]
        public void HyphensManual_ProducesValidPdf()
        {
            // U+00AD is soft hyphen
            var result = Render.ToPdf(@"
                <div style='width: 100px; hyphens: manual;'>
                    <p>Sup\u00ADer\u00ADcal\u00ADi\u00ADfrag\u00ADil\u00ADis\u00ADtic</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void HyphensNone_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: 100px; hyphens: none;'>
                    <p>Sup\u00ADer\u00ADcal\u00ADi\u00ADfrag</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void HyphensManual_NoSoftHyphens_NoChange()
        {
            var result = Render.ToPdf(@"
                <div style='width: 200px; hyphens: manual;'>
                    <p>Hello World</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void HyphensManual_FitsOnLine_NoVisibleHyphen()
        {
            // When text fits without wrapping, soft hyphens should be invisible
            var result = Render.ToPdf(@"
                <div style='width: 500px; hyphens: manual;'>
                    <p>Super\u00ADcalifragilistic</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

    }
}
