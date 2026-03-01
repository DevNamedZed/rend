using System;
using Xunit;

namespace Rend.Tests.Rendering
{
    public class BorderImageTests
    {
        [Fact]
        public void BorderImage_GradientSource_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='width: 200px; height: 200px; border: 10px solid transparent;
                                border-image-source: linear-gradient(red, blue);
                                border-image-slice: 10;'>
                        Gradient border
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
        public void BorderImage_Shorthand_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='width: 200px; height: 200px; border: 10px solid transparent;
                                border-image: linear-gradient(45deg, red, blue) 10 stretch;'>
                        Shorthand border-image
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
        public void BorderImage_None_UsesNormalBorder()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='width: 200px; height: 200px; border: 3px solid red;
                                border-image-source: none;'>
                        Normal border
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
        public void BorderImage_NoBorder_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='width: 200px; height: 200px;
                                border-image-source: linear-gradient(red, blue);'>
                        No border width set
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
        public void BorderImage_RadialGradient_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='width: 200px; height: 200px; border: 15px solid transparent;
                                border-image-source: radial-gradient(red, blue);
                                border-image-slice: 15;'>
                        Radial gradient border
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
        public void BorderImage_DifferentBorderWidths_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='width: 200px; height: 200px;
                                border-top: 5px solid transparent;
                                border-right: 10px solid transparent;
                                border-bottom: 15px solid transparent;
                                border-left: 20px solid transparent;
                                border-image-source: linear-gradient(red, green, blue);'>
                        Different widths
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
        public void BorderImage_WithContent_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='width: 300px; border: 8px solid transparent;
                                border-image: linear-gradient(to right, #f06, #4a90d9) 8;
                                padding: 20px;'>
                        <p>This box has a gradient border image applied.</p>
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
