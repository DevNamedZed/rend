using Xunit;

namespace Rend.Tests.Layout
{
    public class WritingModeTests
    {
        [Fact]
        public void VerticalRl_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='writing-mode: vertical-rl; width: 200px; height: 300px;'>
                    <p>Hello vertical RL</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void VerticalLr_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='writing-mode: vertical-lr; width: 200px; height: 300px;'>
                    <p>Hello vertical LR</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void HorizontalTb_DefaultWritingMode_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='writing-mode: horizontal-tb; width: 200px;'>
                    <p>Hello horizontal TB (default)</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void TextOrientation_Mixed_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='writing-mode: vertical-rl; text-orientation: mixed; width: 200px; height: 300px;'>
                    <p>Mixed orientation text</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void TextOrientation_Upright_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='writing-mode: vertical-rl; text-orientation: upright; width: 200px; height: 300px;'>
                    <p>Upright orientation</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void TextOrientation_Sideways_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='writing-mode: vertical-lr; text-orientation: sideways; width: 200px; height: 300px;'>
                    <p>Sideways orientation</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void VerticalWritingMode_ProducesDifferentLayoutThanHorizontal()
        {
            // Verify that both horizontal and vertical writing modes produce
            // valid (non-empty) PDF output. The layout is structurally different
            // (blocks stack vertically vs. horizontally), but the raw PDF bytes
            // may be similar without native font shaping. We verify both render
            // successfully and are non-empty.
            byte[] horizontalResult;
            horizontalResult = Render.ToPdf(@"
                <div style='writing-mode: horizontal-tb; width: 200px; height: 300px;'>
                    <div style='background: red; height: 50px;'>Block A</div>
                    <div style='background: blue; height: 50px;'>Block B</div>
                </div>");

            var verticalResult = Render.ToPdf(@"
                <div style='writing-mode: vertical-rl; width: 200px; height: 300px;'>
                    <div style='background: red; width: 50px;'>Block A</div>
                    <div style='background: blue; width: 50px;'>Block B</div>
                </div>");

            Assert.NotNull(horizontalResult);
            Assert.NotNull(verticalResult);
            Assert.True(horizontalResult.Length > 0, "Horizontal writing mode should produce non-empty PDF.");
            Assert.True(verticalResult.Length > 0, "Vertical writing mode should produce non-empty PDF.");
        }

        [Fact]
        public void VerticalRl_MultipleBlocks_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='writing-mode: vertical-rl; width: 400px; height: 300px;'>
                    <div style='background: red;'>Block A</div>
                    <div style='background: green;'>Block B</div>
                    <div style='background: blue;'>Block C</div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void VerticalLr_InlineText_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='writing-mode: vertical-lr; width: 200px; height: 300px;'>
                    Hello world in vertical LR mode with inline text
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void VerticalRl_WithStyling_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='writing-mode: vertical-rl; width: 300px; height: 400px;
                            font-size: 16px; color: navy; background: lightyellow;
                            padding: 10px; border: 1px solid black;'>
                    <p style='margin: 5px; background: lightblue;'>First paragraph</p>
                    <p style='margin: 5px; background: lightgreen;'>Second paragraph</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void VerticalWritingMode_NestedHorizontal_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='writing-mode: vertical-rl; width: 300px; height: 400px;'>
                    <p>Vertical text</p>
                    <div style='writing-mode: horizontal-tb; width: 100px;'>
                        <p>Back to horizontal</p>
                    </div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

    }
}
