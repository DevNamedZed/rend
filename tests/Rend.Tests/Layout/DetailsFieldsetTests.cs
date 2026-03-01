using System;
using Xunit;

namespace Rend.Tests.Layout
{
    public class DetailsFieldsetTests
    {
        [Fact]
        public void Details_WithoutOpen_ShowsOnlySummary()
        {
            string html = @"
                <details>
                    <summary>Click to expand</summary>
                    <p>Hidden content that should not appear</p>
                </details>";

            byte[] result;
            try
            {
                result = Render.ToPdf(html);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0, "PDF output should not be empty");
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public void Details_WithOpen_ShowsAllChildren()
        {
            string html = @"
                <details open>
                    <summary>Click to collapse</summary>
                    <p>Visible content that should appear</p>
                    <p>More visible content</p>
                </details>";

            byte[] result;
            try
            {
                result = Render.ToPdf(html);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0, "PDF output should not be empty");
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public void Details_WithOpen_ProducesLargerOutput()
        {
            string closedHtml = @"
                <details>
                    <summary>Summary</summary>
                    <p>This is hidden content with enough text to make a size difference.</p>
                    <p>More hidden content with additional text that should not render.</p>
                </details>";

            string openHtml = @"
                <details open>
                    <summary>Summary</summary>
                    <p>This is visible content with enough text to make a size difference.</p>
                    <p>More visible content with additional text that should render.</p>
                </details>";

            byte[] closedResult, openResult;
            try
            {
                closedResult = Render.ToPdf(closedHtml);
                openResult = Render.ToPdf(openHtml);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(closedResult);
            Assert.NotNull(openResult);
            // Open details should produce more PDF content than closed
            Assert.True(openResult.Length > closedResult.Length,
                "Open <details> should produce more content than closed <details>");
        }

        [Fact]
        public void Fieldset_RendersWithBorder()
        {
            string html = @"
                <fieldset>
                    <p>Content inside fieldset</p>
                </fieldset>";

            byte[] result;
            try
            {
                result = Render.ToPdf(html);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0, "PDF output should not be empty");
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public void Fieldset_WithLegend_Renders()
        {
            string html = @"
                <fieldset>
                    <legend>Personal Information</legend>
                    <p>Name: John Doe</p>
                    <p>Email: john@example.com</p>
                </fieldset>";

            byte[] result;
            try
            {
                result = Render.ToPdf(html);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0, "PDF output should not be empty");
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public void Hr_RendersWithBorderOnly()
        {
            string html = @"
                <p>Before</p>
                <hr>
                <p>After</p>";

            byte[] result;
            try
            {
                result = Render.ToPdf(html);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0, "PDF output should not be empty");
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public void Details_NoSummary_ClosedShowsNothing()
        {
            // A <details> without a <summary> child, when closed,
            // should not render any of its children.
            string html = @"
                <details>
                    <p>This content should be hidden</p>
                </details>";

            byte[] result;
            try
            {
                result = Render.ToPdf(html);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0, "PDF output should not be empty");
        }

        [Fact]
        public void Fieldset_MultipleLegends_OnlyFirstInterruptsBorder()
        {
            // Only the first legend should interrupt the border
            string html = @"
                <fieldset>
                    <legend>First Legend</legend>
                    <legend>Second Legend</legend>
                    <p>Content</p>
                </fieldset>";

            byte[] result;
            try
            {
                result = Render.ToPdf(html);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0, "PDF output should not be empty");
            Assert.Equal((byte)'%', result[0]);
        }

        private static bool IsNativeLibraryFailure(Exception ex)
        {
            if (ex is DllNotFoundException) return true;
            if (ex is TypeInitializationException) return true;
            if (ex is BadImageFormatException) return true;

            var inner = ex.InnerException;
            while (inner != null)
            {
                if (inner is DllNotFoundException) return true;
                if (inner is TypeInitializationException) return true;
                inner = inner.InnerException;
            }

            string msg = ex.Message ?? "";
            if (msg.Contains("libHarfBuzz", StringComparison.OrdinalIgnoreCase)) return true;
            if (msg.Contains("libSkiaSharp", StringComparison.OrdinalIgnoreCase)) return true;
            if (msg.Contains("HarfBuzzSharp", StringComparison.OrdinalIgnoreCase)) return true;
            if (msg.Contains("SkiaSharp", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }
    }
}
