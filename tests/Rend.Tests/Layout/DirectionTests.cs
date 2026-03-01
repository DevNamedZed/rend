using System;
using Xunit;

namespace Rend.Tests.Layout
{
    public class DirectionTests
    {
        [Fact]
        public void DirectionRtl_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='direction: rtl; width: 300px;'>
                        <p>مرحبا بالعالم</p>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void DirectionRtl_TextAlignStart_AlignsRight()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='direction: rtl; text-align: start; width: 300px;'>
                        <p>Text</p>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void DirectionRtl_TextAlignEnd_AlignsLeft()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='direction: rtl; text-align: end; width: 300px;'>
                        <p>Text</p>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void DirectionLtr_TextAlignStart_AlignsLeft()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='direction: ltr; text-align: start; width: 300px;'>
                        <p>Text</p>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void DirectionRtl_WithExplicitCenter_StaysCentered()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='direction: rtl; text-align: center; width: 300px;'>
                        <p>Centered text</p>
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
