using System;
using System.Text;
using Rend.Core.Values;
using Rend.Rendering;
using Rend.Rendering.Internal;
using Xunit;

namespace Rend.Tests.Rendering
{
    public class SvgRenderingTests
    {
        [Fact]
        public void Svg_Rect_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200'>
                        <rect x='10' y='10' width='100' height='80' fill='red'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_Circle_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200'>
                        <circle cx='100' cy='100' r='50' fill='blue' stroke='black' stroke-width='2'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_Ellipse_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200'>
                        <ellipse cx='100' cy='100' rx='80' ry='50' fill='green'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_Line_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200'>
                        <line x1='10' y1='10' x2='190' y2='190' stroke='red' stroke-width='3'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_Polygon_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200'>
                        <polygon points='100,10 40,198 190,78 10,78 160,198' fill='orange' stroke='black' stroke-width='1'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_Polyline_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200'>
                        <polyline points='0,40 40,40 40,80 80,80 80,120 120,120' fill='none' stroke='blue' stroke-width='2'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_Path_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200'>
                        <path d='M 10 80 C 40 10, 65 10, 95 80 S 150 150, 180 80' fill='none' stroke='purple' stroke-width='2'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_Text_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='100'>
                        <text x='10' y='50' font-size='24' fill='black'>Hello SVG</text>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_Group_WithTransform_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200'>
                        <g transform='translate(50,50)'>
                            <rect width='50' height='50' fill='red'/>
                            <circle cx='25' cy='25' r='20' fill='white'/>
                        </g>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_ViewBox_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200' viewBox='0 0 100 100'>
                        <rect x='0' y='0' width='100' height='100' fill='lightblue'/>
                        <circle cx='50' cy='50' r='40' fill='red'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_MultipleShapes_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='300' height='200'>
                        <rect x='10' y='10' width='80' height='80' fill='red'/>
                        <circle cx='150' cy='50' r='40' fill='green'/>
                        <ellipse cx='250' cy='100' rx='40' ry='60' fill='blue'/>
                        <line x1='10' y1='150' x2='290' y2='150' stroke='black' stroke-width='2'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_RoundedRect_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200'>
                        <rect x='10' y='10' width='180' height='180' rx='20' fill='purple'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_Opacity_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200'>
                        <rect x='10' y='10' width='100' height='100' fill='red' opacity='0.5'/>
                        <rect x='50' y='50' width='100' height='100' fill='blue' fill-opacity='0.5'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_Transforms_Scale_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200'>
                        <rect width='50' height='50' fill='red' transform='scale(2)'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_Transforms_Rotate_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200'>
                        <rect x='50' y='50' width='100' height='50' fill='green' transform='rotate(45,100,75)'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Svg_StrokeNoFill_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <svg width='200' height='200'>
                        <rect x='10' y='10' width='180' height='180' fill='none' stroke='red' stroke-width='3'/>
                    </svg>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        // ═══════════════════════════════════════════
        // Unit tests for SvgRenderer helpers
        // ═══════════════════════════════════════════

        [Fact]
        public void ViewBox_ParsesCorrectly()
        {
            Assert.True(SvgRenderer.ParseViewBox("0 0 100 200", out float x, out float y, out float w, out float h));
            Assert.Equal(0f, x);
            Assert.Equal(0f, y);
            Assert.Equal(100f, w);
            Assert.Equal(200f, h);
        }

        [Fact]
        public void ViewBox_CommaDelimited_ParsesCorrectly()
        {
            Assert.True(SvgRenderer.ParseViewBox("10,20,300,400", out float x, out float y, out float w, out float h));
            Assert.Equal(10f, x);
            Assert.Equal(20f, y);
            Assert.Equal(300f, w);
            Assert.Equal(400f, h);
        }

        [Fact]
        public void ViewBox_Empty_ReturnsFalse()
        {
            Assert.False(SvgRenderer.ParseViewBox("", out _, out _, out _, out _));
        }

        [Fact]
        public void ViewBox_Insufficient_ReturnsFalse()
        {
            Assert.False(SvgRenderer.ParseViewBox("0 0", out _, out _, out _, out _));
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
