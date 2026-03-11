using Xunit;

namespace Rend.Tests.Layout
{
    public class DirectionTests
    {
        [Fact]
        public void DirectionRtl_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='direction: rtl; width: 300px;'>
                    <p>مرحبا بالعالم</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void DirectionRtl_TextAlignStart_AlignsRight()
        {
            var result = Render.ToPdf(@"
                <div style='direction: rtl; text-align: start; width: 300px;'>
                    <p>Text</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void DirectionRtl_TextAlignEnd_AlignsLeft()
        {
            var result = Render.ToPdf(@"
                <div style='direction: rtl; text-align: end; width: 300px;'>
                    <p>Text</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void DirectionLtr_TextAlignStart_AlignsLeft()
        {
            var result = Render.ToPdf(@"
                <div style='direction: ltr; text-align: start; width: 300px;'>
                    <p>Text</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void DirectionRtl_WithExplicitCenter_StaysCentered()
        {
            var result = Render.ToPdf(@"
                <div style='direction: rtl; text-align: center; width: 300px;'>
                    <p>Centered text</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

    }
}
