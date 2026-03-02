using System;
using Xunit;

namespace Rend.Tests.Rendering
{
    public class NewCssPropertyRenderTests
    {
        [Fact]
        public void InitialLetter_Normal_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p style='initial-letter: normal;'>
                        This paragraph has a normal initial letter, no drop cap.
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
        public void HangingPunctuation_First_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p style='hanging-punctuation: first; width: 200px;'>
                        \u201CThis is a paragraph with hanging opening quotes.\u201D
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
        public void ForcedColorAdjust_None_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='forced-color-adjust: none; background: blue; color: white; padding: 10px;'>
                        This element opts out of forced color adjustment.
                    </div>");
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
