using Rend.Core.Values;
using SkiaSharp;
using Xunit;

namespace Rend.Tests.EndToEnd
{
    /// <summary>
    /// Verifies a propagated canvas background gradient ([CSS-BACKGROUNDS §2.11]) tiles at the
    /// positioning-area scale in PDF (PDF-B4). The root gradient is sized to the root's (short)
    /// padding box and repeated to fill the taller canvas; previously the PDF path filled the
    /// whole canvas once, stretching a single gradient. Must match the Skia PNG reference.
    /// </summary>
    public class TiledGradientPdfTests
    {
        // Root gradient (red→blue, top→bottom) over a 40px-tall content box on a 300px page → the
        // gradient is sized to ~40px and tiled vertically (~7 repeats).
        private const string CanvasTiledGradientHtml =
            "<html style='background-image:linear-gradient(to bottom,#ff0000,#0000ff)'>" +
            "<body style='margin:0'><div style='height:40px'></div></body></html>";

        [Fact]
        public void CanvasGradient_TilesAtPositioningAreaScale_MatchingPngReference()
        {
            var options = new RenderOptions
            {
                PageSize = new SizeF(100, 300),
                MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0,
            };

            using SKBitmap reference = SKBitmap.Decode(Render.ToImage(CanvasTiledGradientHtml, options));
            byte[] pdf = Render.ToPdf(CanvasTiledGradientHtml, options);
            using SKBitmap pdfRender = PdfToImage.RenderPageToBitmap(pdf, 0, 72f);

            AssertTiledVertically(reference, "PNG reference");
            AssertTiledVertically(pdfRender, "PDF");
        }

        // The tile is ~40px tall, so y=6 and y=46 sit at the same (red) point of consecutive tiles
        // while y=34 is near a tile's blue end. y=240 lands on a later tile's red top — the strong
        // discriminator: a single stretched gradient would be deep blue that far down.
        private static void AssertTiledVertically(SKBitmap bitmap, string label)
        {
            AssertRed(bitmap, 50, 6, $"{label}: tile-0 top");
            AssertBlue(bitmap, 50, 34, $"{label}: tile-0 bottom");
            AssertRed(bitmap, 50, 46, $"{label}: tile-1 top (repeat)");
            AssertRed(bitmap, 50, 240, $"{label}: far tile top (not a single stretch)");
        }

        private static void AssertRed(SKBitmap bitmap, int x, int y, string where)
        {
            SKColor pixel = bitmap.GetPixel(x, y);
            Assert.True(pixel.Red > pixel.Blue + 60,
                $"expected red-dominant at {where} ({x},{y}) but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
        }

        private static void AssertBlue(SKBitmap bitmap, int x, int y, string where)
        {
            SKColor pixel = bitmap.GetPixel(x, y);
            Assert.True(pixel.Blue > pixel.Red + 60,
                $"expected blue-dominant at {where} ({x},{y}) but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
        }
    }
}
