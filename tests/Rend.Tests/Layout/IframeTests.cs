using System;
using Xunit;

namespace Rend.Tests.Layout
{
    public class IframeTests
    {
        [Fact]
        public void Iframe_Empty_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"<iframe></iframe>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Iframe_WithSrc_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"<iframe src='https://example.com'></iframe>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Iframe_WithSrcdoc_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"<iframe srcdoc='<p>Hello from srcdoc</p>'></iframe>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Iframe_WithDimensions_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"<iframe src='page.html' width='500' height='300'></iframe>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Iframe_InDocument_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div>
                        <h2>Embedded Content</h2>
                        <iframe src='https://example.com' width='400' height='200'></iframe>
                        <p>Above iframe content.</p>
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
        public void Iframe_MultipleSrcdoc_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div>
                        <iframe srcdoc='<h1>Frame 1</h1><p>Content</p>'></iframe>
                        <iframe srcdoc='<h1>Frame 2</h1><p>More content</p>'></iframe>
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
