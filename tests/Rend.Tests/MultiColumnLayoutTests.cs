using Xunit;

namespace Rend.Tests
{
    public class MultiColumnLayoutTests
    {
        [Fact]
        public void ToPdf_WithColumnCount_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='column-count: 2; column-gap: 20px;'>
                    <p>First paragraph of text that should flow into two columns.</p>
                    <p>Second paragraph of text that continues in the columns.</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public void ToPdf_WithColumnWidth_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='column-width: 200px;'>
                    <p>Content that flows into columns based on width.</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToPdf_WithColumnsShorthand_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='columns: 3; column-gap: 15px;'>
                    <p>Column 1 content</p>
                    <p>Column 2 content</p>
                    <p>Column 3 content</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToPdf_WithColumnRule_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='column-count: 2; column-rule: 1px solid #ccc;'>
                    <p>Left column content.</p>
                    <p>Right column content.</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToPdf_SingleColumn_StillWorks()
        {
            var result = Render.ToPdf(@"
                <div style='column-count: 1;'>
                    <p>Single column content.</p>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ToPdf_NoColumnProperties_NormalLayout()
        {
            // Verify that normal layout still works when no column properties are set
            var result = Render.ToPdf("<div><p>Normal content</p></div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

    }
}
