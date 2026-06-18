using Rend.Core.Values;
using SkiaSharp;
using Xunit;

namespace Rend.Tests.EndToEnd
{
    /// <summary>
    /// Verifies that a CSS gradient mask (mask-image: linear-gradient) takes effect in PDF output.
    /// PDF axial/radial shadings cannot carry per-stop alpha, so the bridge rasterizes the masked
    /// element through Skia (PDF-B2) and embeds it; the PDF must match the Skia PNG reference.
    /// </summary>
    public class MaskGradientPdfTests
    {
        private const string MaskedBoxHtml = @"<html><body style='margin:0'>
            <div style='width:100px;height:100px;margin:20px;background:#ff0000;
                -webkit-mask-image:linear-gradient(to right, black, transparent);
                mask-image:linear-gradient(to right, black, transparent)'></div>
            </body></html>";

        [Fact]
        public void GradientMask_FadesElement_InPdfMatchingPngReference()
        {
            var options = new RenderOptions
            {
                PageSize = new SizeF(200, 200),
                MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0,
            };

            using SKBitmap reference = SKBitmap.Decode(Render.ToImage(MaskedBoxHtml, options));
            byte[] pdf = Render.ToPdf(MaskedBoxHtml, options);
            using SKBitmap pdfRender = PdfToImage.RenderPageToBitmap(pdf, 0, 72f);

            // The mask alpha runs 1→0 left→right across the box at user (20,20)-(120,120):
            // the left stays the box colour, the right is masked away to the page background.
            AssertReddish(reference, 26, 70, "PNG reference, masked-in (left)");
            AssertWhite(reference, 114, 70, "PNG reference, masked-out (right)");

            AssertReddish(pdfRender, 26, 70, "PDF, masked-in (left)");
            AssertWhite(pdfRender, 114, 70, "PDF, masked-out (right)");
        }

        private static void AssertReddish(SKBitmap bitmap, int x, int y, string where)
        {
            SKColor pixel = bitmap.GetPixel(x, y);
            Assert.True(pixel.Red > 150 && pixel.Red > pixel.Green + 60 && pixel.Red > pixel.Blue + 60,
                $"expected red-dominant at {where} ({x},{y}) but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
        }

        private static void AssertWhite(SKBitmap bitmap, int x, int y, string where)
        {
            SKColor pixel = bitmap.GetPixel(x, y);
            Assert.True(pixel.Red > 230 && pixel.Green > 230 && pixel.Blue > 230,
                $"expected white (masked out) at {where} ({x},{y}) but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
        }
    }
}
