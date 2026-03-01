using Xunit;

namespace Rend.Tests.Rendering
{
    public class BackdropFilterMaskTests
    {
        [Fact]
        public void BackdropFilter_Opacity_ProducesValidPdf()
        {
            string html = @"<html><body>
                <div style='background: blue; width: 200px; height: 200px;'>
                    <div style='backdrop-filter: opacity(0.5); width: 100px; height: 100px; background: red;'>
                        Content
                    </div>
                </div>
            </body></html>";
            var pdf = Render.ToPdf(html, new RenderOptions { PageSize = new Core.Values.SizeF(400, 300) });
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 100);
        }

        [Fact]
        public void BackdropFilter_Blur_GracefulDegradation()
        {
            string html = @"<html><body>
                <div style='background: blue; width: 200px; height: 200px;'>
                    <div style='backdrop-filter: blur(10px); width: 100px; height: 100px; background: rgba(255,255,255,0.5);'>
                        Blurred backdrop
                    </div>
                </div>
            </body></html>";
            var pdf = Render.ToPdf(html, new RenderOptions { PageSize = new Core.Values.SizeF(400, 300) });
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 100);
        }

        [Fact]
        public void BackdropFilter_None_ProducesValidPdf()
        {
            string html = @"<html><body>
                <div style='backdrop-filter: none; width: 100px; height: 100px; background: red;'>
                    No filter
                </div>
            </body></html>";
            var pdf = Render.ToPdf(html, new RenderOptions { PageSize = new Core.Values.SizeF(400, 300) });
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 100);
        }

        [Fact]
        public void BackdropFilter_MultipleFilters_ProducesValidPdf()
        {
            string html = @"<html><body>
                <div style='backdrop-filter: opacity(0.8) blur(5px); width: 100px; height: 100px; background: green;'>
                    Multiple filters
                </div>
            </body></html>";
            var pdf = Render.ToPdf(html, new RenderOptions { PageSize = new Core.Values.SizeF(400, 300) });
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 100);
        }

        [Fact]
        public void MaskImage_None_ProducesValidPdf()
        {
            string html = @"<html><body>
                <div style='mask-image: none; width: 100px; height: 100px; background: red;'>
                    No mask
                </div>
            </body></html>";
            var pdf = Render.ToPdf(html, new RenderOptions { PageSize = new Core.Values.SizeF(400, 300) });
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 100);
        }

        [Fact]
        public void MaskImage_LinearGradient_GracefulDegradation()
        {
            string html = @"<html><body>
                <div style='mask-image: linear-gradient(black, transparent); width: 100px; height: 100px; background: red;'>
                    Masked content
                </div>
            </body></html>";
            var pdf = Render.ToPdf(html, new RenderOptions { PageSize = new Core.Values.SizeF(400, 300) });
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 100);
        }

        [Fact]
        public void MaskRepeat_Parsed_ProducesValidPdf()
        {
            string html = @"<html><body>
                <div style='mask-repeat: no-repeat; width: 100px; height: 100px; background: blue;'>
                    Mask repeat
                </div>
            </body></html>";
            var pdf = Render.ToPdf(html, new RenderOptions { PageSize = new Core.Values.SizeF(400, 300) });
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 100);
        }

        [Fact]
        public void MaskMode_Parsed_ProducesValidPdf()
        {
            string html = @"<html><body>
                <div style='mask-mode: alpha; width: 100px; height: 100px; background: green;'>
                    Mask mode
                </div>
            </body></html>";
            var pdf = Render.ToPdf(html, new RenderOptions { PageSize = new Core.Values.SizeF(400, 300) });
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 100);
        }
    }
}
