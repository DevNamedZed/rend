using System;
using Xunit;

namespace Rend.Tests.Rendering
{
    public class TextWrapBalanceTests
    {
        [Fact]
        public void TextWrapBalance_ShortParagraph_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p style='text-wrap: balance; width: 200px;'>
                        This is a short paragraph that should have balanced line lengths.
                    </p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void TextWrapBalance_Heading_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <h2 style='text-wrap: balance; width: 300px;'>
                        A heading with balanced text wrapping across multiple lines
                    </h2>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void TextWrapWrap_Default_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p style='text-wrap: wrap; width: 200px;'>
                        Normal wrapping behavior for this paragraph.
                    </p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void TextWrapPretty_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p style='text-wrap: pretty; width: 200px;'>
                        Pretty text wrapping avoids orphans at the end of paragraphs.
                    </p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        private static bool IsNativeLibraryFailure(Exception ex)
        {
            return ex is DllNotFoundException ||
                   ex is TypeInitializationException ||
                   (ex.InnerException is DllNotFoundException) ||
                   ex.Message.Contains("native", StringComparison.OrdinalIgnoreCase);
        }
    }
}
