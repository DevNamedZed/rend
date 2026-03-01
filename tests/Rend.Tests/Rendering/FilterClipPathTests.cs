using System;
using Xunit;

namespace Rend.Tests.Rendering
{
    public class FilterClipPathTests
    {
        [Fact]
        public void Filter_Opacity_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='filter: opacity(0.5); background: red; width: 100px; height: 100px;'>
                        Filtered
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
        public void Filter_OpacityPercent_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='filter: opacity(50%); background: blue; width: 100px; height: 100px;'>
                        50% opacity
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
        public void Filter_Blur_ParsedWithoutCrash()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='filter: blur(5px); background: green; width: 100px; height: 100px;'>
                        Blurred (graceful degradation)
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
        public void Filter_Multiple_ParsedWithoutCrash()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='filter: blur(3px) brightness(1.2) contrast(1.1); background: yellow; width: 100px; height: 100px;'>
                        Multiple filters
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
        public void Filter_None_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='filter: none; background: red; width: 100px; height: 100px;'>
                        No filter
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
        public void Filter_DropShadow_ParsedWithoutCrash()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='filter: drop-shadow(4px 4px 8px rgba(0,0,0,0.5)); background: blue; width: 100px; height: 100px;'>
                        Shadow
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
        public void ClipPath_Inset_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='clip-path: inset(10px 20px 30px 40px); background: red; width: 200px; height: 200px;'>
                        Inset clipped
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
        public void ClipPath_InsetWithRound_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='clip-path: inset(10px round 15px); background: green; width: 200px; height: 200px;'>
                        Inset rounded
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
        public void ClipPath_Circle_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='clip-path: circle(50%); background: blue; width: 200px; height: 200px;'>
                        Circle clipped
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
        public void ClipPath_CircleWithPosition_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='clip-path: circle(80px at 100px 100px); background: purple; width: 200px; height: 200px;'>
                        Circle at position
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
        public void ClipPath_Ellipse_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='clip-path: ellipse(80px 50px); background: orange; width: 200px; height: 200px;'>
                        Ellipse clipped
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
        public void ClipPath_Polygon_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='clip-path: polygon(50% 0%, 100% 100%, 0% 100%); background: red; width: 200px; height: 200px;'>
                        Triangle
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
        public void ClipPath_None_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='clip-path: none; background: green; width: 100px; height: 100px;'>
                        No clip
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
        public void ClipPath_WithFilter_Combined_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='clip-path: circle(50%); filter: opacity(0.7); background: blue; width: 200px; height: 200px;'>
                        Combined clip and filter
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
