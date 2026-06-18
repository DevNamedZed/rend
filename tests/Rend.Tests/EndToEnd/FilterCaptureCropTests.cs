using Rend.Core.Values;
using Rend.Pdf.Parsing;
using SkiaSharp;
using Xunit;

namespace Rend.Tests.EndToEnd
{
    /// <summary>
    /// Verifies that a filtered element's offscreen capture is embedded cropped to its content
    /// bounds, not as a page-sized raster (PDF-B7), while still rendering at the correct position.
    /// </summary>
    public class FilterCaptureCropTests
    {
        // A small blurred box near the top-left of a large page. The capture must crop to roughly
        // the box + blur spread, far smaller than the full page raster.
        private const string SmallBlurredBoxHtml = @"<html><body style='margin:0'>
            <div style='width:40px;height:40px;margin:50px;background:#ff0000;filter:blur(2px)'></div>
            </body></html>";

        [Fact]
        public void FilteredElement_EmbedsCroppedImage_AtCorrectPosition()
        {
            var options = new RenderOptions
            {
                PageSize = new SizeF(300, 300),
                MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0,
            };

            using SKBitmap reference = SKBitmap.Decode(Render.ToImage(SmallBlurredBoxHtml, options));
            byte[] pdf = Render.ToPdf(SmallBlurredBoxHtml, options);
            using SKBitmap pdfRender = PdfToImage.RenderPageToBitmap(pdf, 0, 72f);

            // Box at layout (50,50)-(90,90); its centre is red in both reference and PDF, and the
            // far corner stays white — the filtered element is placed correctly after cropping.
            AssertRed(reference, 70, 70, "PNG box centre");
            AssertWhite(reference, 250, 250, "PNG far corner");
            AssertRed(pdfRender, 70, 70, "PDF box centre");
            AssertWhite(pdfRender, 250, 250, "PDF far corner");

            // The embedded filter image must be cropped to ~box+blur, well below the full-page
            // raster (300pt × 2× supersample = 600px). A page-sized raster would be ~600px wide.
            ImageDimensions embedded = ReadFirstImageDimensions(pdf);
            Assert.True(embedded.Width > 0 && embedded.Height > 0, "expected an embedded filter image");
            Assert.True(embedded.Width < 250 && embedded.Height < 250,
                $"expected a cropped image (<250px), got {embedded.Width}x{embedded.Height}");
        }

        private sealed class ImageDimensions
        {
            public int Width { get; }
            public int Height { get; }
            public ImageDimensions(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }

        private static ImageDimensions ReadFirstImageDimensions(byte[] pdf)
        {
            using var reader = PdfDocumentReader.Open(pdf);
            PdfObj page = reader.GetPage(0);
            PdfObj resources = reader.Resolve(page["Resources"]);
            PdfObj xObjects = reader.Resolve(resources["XObject"]);
            foreach (string name in xObjects.Keys)
            {
                PdfObj candidate = reader.Resolve(xObjects[name]);
                if (candidate.IsStream && reader.Resolve(candidate["Subtype"]).AsName() == "Image")
                {
                    int width = (int)reader.Resolve(candidate["Width"]).AsInt();
                    int height = (int)reader.Resolve(candidate["Height"]).AsInt();
                    return new ImageDimensions(width, height);
                }
            }
            return new ImageDimensions(0, 0);
        }

        private static void AssertRed(SKBitmap bitmap, int x, int y, string where)
        {
            SKColor pixel = bitmap.GetPixel(x, y);
            Assert.True(pixel.Red > 150 && pixel.Red > pixel.Blue + 60,
                $"expected red-dominant at {where} ({x},{y}) but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
        }

        private static void AssertWhite(SKBitmap bitmap, int x, int y, string where)
        {
            SKColor pixel = bitmap.GetPixel(x, y);
            Assert.True(pixel.Red > 230 && pixel.Green > 230 && pixel.Blue > 230,
                $"expected white at {where} ({x},{y}) but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
        }
    }
}
