using Xunit;

namespace Rend.Tests.Layout
{
    public class IntrinsicSizingTests
    {
        [Fact]
        public void MinContent_Block_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: min-content; background: red;'>
                    <div>Hello World</div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void MaxContent_Block_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: max-content; background: blue;'>
                    <div>Hello World</div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void FitContent_Block_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: fit-content; background: green;'>
                    <div>Short</div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void MinContent_Float_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='float: left; width: min-content; background: red;'>
                    <div>Hello World This is text</div>
                </div>
                <div>After float</div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void MaxContent_InlineBlock_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <span style='display: inline-block; width: max-content; background: blue;'>
                    Hello World
                </span>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void MinContent_GridItem_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='display: grid; grid-template-columns: min-content 1fr;'>
                    <div style='background: red;'>Narrow</div>
                    <div style='background: blue;'>Fills remaining</div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

    }
}
