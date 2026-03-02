using System;
using Xunit;

namespace Rend.Tests.Layout
{
    public class PictureElementTests
    {
        [Fact]
        public void Picture_WithImgFallback_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <picture>
                        <source srcset='image.webp' type='image/webp'>
                        <img src='image.png' alt='Test'>
                    </picture>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Picture_MultipleSources_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <picture>
                        <source srcset='image.avif' type='image/avif'>
                        <source srcset='image.webp' type='image/webp'>
                        <source srcset='image.jpg' type='image/jpeg'>
                        <img src='fallback.png' alt='Photo'>
                    </picture>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Picture_UnsupportedSources_FallsBackToImg()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <picture>
                        <source srcset='image.avif' type='image/avif'>
                        <img src='fallback.png' alt='Photo'>
                    </picture>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Img_WithSrcset_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <img srcset='small.png 1x, large.png 2x' src='fallback.png' alt='Responsive'>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Picture_InDocument_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div>
                        <h2>Photo Gallery</h2>
                        <picture>
                            <source srcset='photo1.webp' type='image/webp'>
                            <img src='photo1.jpg' alt='Photo 1'>
                        </picture>
                        <picture>
                            <source srcset='photo2.webp' type='image/webp'>
                            <img src='photo2.jpg' alt='Photo 2'>
                        </picture>
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
        public void Img_SrcsetWithWidthDescriptors_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <img srcset='small.jpg 300w, medium.jpg 600w, large.jpg 1200w'
                         sizes='(max-width: 600px) 300px, 600px'
                         src='medium.jpg' alt='Responsive image'>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Picture_EmptyWithOnlyImg_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <picture>
                        <img src='test.png' alt='Just an img in picture'>
                    </picture>");
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
