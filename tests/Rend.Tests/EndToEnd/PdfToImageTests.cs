using System;
using System.IO;
using Rend.Core.Values;
using Rend.Pdf.Reading;
using SkiaSharp;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.EndToEnd
{
    public class PdfToImageTests
    {
        private readonly ITestOutputHelper _output;

        public PdfToImageTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private byte[] RenderPdf(string html, float w = 400, float h = 300)
        {
            return Render.ToPdf(html, new RenderOptions
            {
                PageSize = new SizeF(w, h),
                MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0
            });
        }

        #region PdfToImage API Tests

        [Fact]
        public void RenderPage_ReturnsValidPng()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var png = PdfToImage.RenderPage(pdf, 0, 72f);

            Assert.NotNull(png);
            Assert.True(png.Length > 100, "PNG should be more than 100 bytes");

            // Verify PNG magic bytes
            Assert.Equal(0x89, png[0]);
            Assert.Equal((byte)'P', png[1]);
            Assert.Equal((byte)'N', png[2]);
            Assert.Equal((byte)'G', png[3]);
        }

        [Fact]
        public void RenderPage_DefaultDpi_Returns150dpi()
        {
            var pdf = RenderPdf("<html><body><p>Test</p></body></html>", 200, 100);
            var png = PdfToImage.RenderPage(pdf);

            // At 150 DPI, 200pt page = 200 * 150/72 = ~417px
            using var bitmap = SKBitmap.Decode(png);
            Assert.NotNull(bitmap);
            Assert.InRange(bitmap.Width, 410, 420);
            Assert.InRange(bitmap.Height, 200, 215);
        }

        [Fact]
        public void RenderPage_72dpi_MatchesPagePoints()
        {
            var pdf = RenderPdf("<html><body><p>Test</p></body></html>", 300, 200);
            var png = PdfToImage.RenderPage(pdf, 0, 72f);

            using var bitmap = SKBitmap.Decode(png);
            Assert.NotNull(bitmap);
            Assert.Equal(300, bitmap.Width);
            Assert.Equal(200, bitmap.Height);
        }

        [Fact]
        public void RenderPage_300dpi_HighResolution()
        {
            var pdf = RenderPdf("<html><body><p>Test</p></body></html>", 200, 100);
            var png = PdfToImage.RenderPage(pdf, 0, 300f);

            // At 300 DPI, 200pt = 200 * 300/72 = ~833px
            using var bitmap = SKBitmap.Decode(png);
            Assert.NotNull(bitmap);
            Assert.InRange(bitmap.Width, 830, 840);
            Assert.InRange(bitmap.Height, 410, 420);
        }

        [Fact]
        public void GetPageCount_SinglePage()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            Assert.Equal(1, PdfToImage.GetPageCount(pdf));
        }

        [Fact]
        public void GetPageSize_MatchesRenderOptions()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>", 500, 700);
            var (width, height) = PdfToImage.GetPageSize(pdf);
            Assert.InRange(width, 499f, 501f);
            Assert.InRange(height, 699f, 701f);
        }

        [Fact]
        public void RenderAllPages_ReturnsSinglePageArray()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var pages = PdfToImage.RenderAllPages(pdf, 72f);

            Assert.Single(pages);
            Assert.True(pages[0].Length > 100);
        }

        [Fact]
        public void RenderPage_FromStream()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            using var stream = new MemoryStream(pdf);
            var png = PdfToImage.RenderPage(stream, 0, 72f);

            Assert.NotNull(png);
            Assert.True(png.Length > 100);
        }

        [Fact]
        public void RenderPage_NullData_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => PdfToImage.RenderPage((byte[])null!));
        }

        [Fact]
        public void RenderPage_InvalidPageIndex_Throws()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            Assert.Throws<ArgumentOutOfRangeException>(() => PdfToImage.RenderPage(pdf, 5));
        }

        [Fact]
        public void RenderPage_NegativePageIndex_Throws()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            Assert.Throws<ArgumentOutOfRangeException>(() => PdfToImage.RenderPage(pdf, -1));
        }

        [Fact]
        public void RenderPage_ZeroDpi_Throws()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            Assert.Throws<ArgumentOutOfRangeException>(() => PdfToImage.RenderPage(pdf, 0, 0f));
        }

        #endregion

        #region Rendered Content Tests

        [Fact]
        public void RenderPage_WhiteBackground()
        {
            var pdf = RenderPdf("<html><body></body></html>", 100, 100);
            var png = PdfToImage.RenderPage(pdf, 0, 72f);

            using var bitmap = SKBitmap.Decode(png);
            // Center pixel should be white
            var pixel = bitmap.GetPixel(50, 50);
            Assert.Equal(255, pixel.Red);
            Assert.Equal(255, pixel.Green);
            Assert.Equal(255, pixel.Blue);
        }

        [Fact]
        public void RenderPage_ColoredBackground_Renders()
        {
            var pdf = RenderPdf(@"<html><body style='margin:0;padding:0'>
                <div style='background:#ff0000;width:100px;height:100px'></div>
            </body></html>", 200, 200);
            var png = PdfToImage.RenderPage(pdf, 0, 72f);

            using var bitmap = SKBitmap.Decode(png);
            // Inside the red div
            var pixel = bitmap.GetPixel(50, 50);
            Assert.InRange(pixel.Red, 240, 255);
            Assert.InRange(pixel.Green, 0, 15);
            Assert.InRange(pixel.Blue, 0, 15);
        }

        [Fact]
        public void RenderPage_TextIsNotBlank()
        {
            var pdf = RenderPdf(@"<html><body style='margin:0;padding:0'>
                <p style='font-size:24px;color:#000'>Hello World</p>
            </body></html>", 300, 100);
            var png = PdfToImage.RenderPage(pdf, 0, 72f);

            using var bitmap = SKBitmap.Decode(png);

            // Scan for non-white pixels in the text area (top portion)
            int nonWhite = 0;
            for (int y = 0; y < 50; y++)
            {
                for (int x = 0; x < 200; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    if (pixel.Red < 200 || pixel.Green < 200 || pixel.Blue < 200)
                        nonWhite++;
                }
            }

            _output.WriteLine($"Non-white pixels in text area: {nonWhite}");
            Assert.True(nonWhite > 50, "Text area should contain visible text pixels");
        }

        [Fact]
        public void RenderPage_Border_Renders()
        {
            var pdf = RenderPdf(@"<html><body style='margin:0;padding:20px'>
                <div style='border:3px solid #000;width:100px;height:100px'></div>
            </body></html>", 200, 200);
            var png = PdfToImage.RenderPage(pdf, 0, 72f);

            using var bitmap = SKBitmap.Decode(png);

            // Top border at y≈20 should have dark pixels
            var borderPixel = bitmap.GetPixel(70, 21);
            Assert.True(borderPixel.Red < 50 && borderPixel.Green < 50 && borderPixel.Blue < 50,
                "Border pixel should be dark");
        }

        [Fact]
        public void RenderPage_MultipleColors()
        {
            var pdf = RenderPdf(@"<html><body style='margin:0;padding:0'>
                <div style='background:#ff0000;width:200px;height:50px'></div>
                <div style='background:#00ff00;width:200px;height:50px'></div>
                <div style='background:#0000ff;width:200px;height:50px'></div>
            </body></html>", 200, 200);
            var png = PdfToImage.RenderPage(pdf, 0, 72f);

            using var bitmap = SKBitmap.Decode(png);

            // Red band
            var red = bitmap.GetPixel(100, 25);
            Assert.InRange(red.Red, 240, 255);
            Assert.InRange(red.Green, 0, 15);

            // Green band
            var green = bitmap.GetPixel(100, 75);
            Assert.InRange(green.Green, 240, 255);
            Assert.InRange(green.Red, 0, 15);

            // Blue band
            var blue = bitmap.GetPixel(100, 125);
            Assert.InRange(blue.Blue, 240, 255);
            Assert.InRange(blue.Red, 0, 15);
        }

        #endregion

        #region PdfDocumentReader Tests

        [Fact]
        public void PdfDocumentReader_Open_Parses()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            using var reader = PdfDocumentReader.Open(pdf);

            Assert.Equal(1, reader.PageCount);
            Assert.False(reader.Catalog.IsNull);
            Assert.False(reader.Trailer.IsNull);
        }

        [Fact]
        public void PdfDocumentReader_Open_FromStream()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            using var stream = new MemoryStream(pdf);
            using var reader = PdfDocumentReader.Open(stream);

            Assert.Equal(1, reader.PageCount);
        }

        [Fact]
        public void PdfDocumentReader_GetPage_ReturnsPage()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            using var reader = PdfDocumentReader.Open(pdf);

            var page = reader.GetPage(0);
            Assert.False(page.IsNull);

            var resolved = reader.Resolve(page);
            Assert.True(resolved.IsDict);
        }

        [Fact]
        public void PdfDocumentReader_GetPage_InvalidIndex_ReturnsNull()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            using var reader = PdfDocumentReader.Open(pdf);

            var page = reader.GetPage(99);
            Assert.True(page.IsNull);
        }

        [Fact]
        public void PdfDocumentReader_Page_HasMediaBox()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>", 400, 300);
            using var reader = PdfDocumentReader.Open(pdf);

            var page = reader.Resolve(reader.GetPage(0));
            var mediaBox = reader.Resolve(page["MediaBox"]);
            Assert.False(mediaBox.IsNull);
            Assert.Equal(4, mediaBox.Count);

            float width = mediaBox[2].AsFloat() - mediaBox[0].AsFloat();
            float height = mediaBox[3].AsFloat() - mediaBox[1].AsFloat();
            Assert.InRange(width, 399f, 401f);
            Assert.InRange(height, 299f, 301f);
        }

        [Fact]
        public void PdfDocumentReader_Page_HasResources()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            using var reader = PdfDocumentReader.Open(pdf);

            var page = reader.Resolve(reader.GetPage(0));
            var resources = reader.Resolve(page["Resources"]);
            Assert.True(resources.IsDict, "Page should have Resources dictionary");
        }

        [Fact]
        public void PdfDocumentReader_Page_HasContentStream()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            using var reader = PdfDocumentReader.Open(pdf);

            var page = reader.Resolve(reader.GetPage(0));
            var contents = page["Contents"];
            Assert.False(contents.IsNull, "Page should have Contents");
        }

        [Fact]
        public void PdfDocumentReader_Catalog_HasPages()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            using var reader = PdfDocumentReader.Open(pdf);

            var pagesRef = reader.Catalog["Pages"];
            Assert.False(pagesRef.IsNull);

            var pages = reader.Resolve(pagesRef);
            Assert.True(pages.IsDict);
        }

        #endregion

        #region Text Extraction Tests

        [Fact]
        public void ExtractText_SimpleText()
        {
            var pdf = RenderPdf("<html><body><p>Hello World</p></body></html>");
            var text = PdfToImage.ExtractText(pdf);

            _output.WriteLine($"Extracted: [{text}]");
            Assert.Contains("Hello", text);
            Assert.Contains("World", text);
        }

        [Fact]
        public void ExtractText_MultipleElements()
        {
            var pdf = RenderPdf(@"<html><body>
                <h1>Title</h1>
                <p>Paragraph one</p>
                <p>Paragraph two</p>
            </body></html>");
            var text = PdfToImage.ExtractText(pdf);

            _output.WriteLine($"Extracted: [{text}]");
            Assert.Contains("Title", text);
            Assert.Contains("Paragraph", text);
        }

        [Fact]
        public void ExtractAllText_ReturnsArrayPerPage()
        {
            var pdf = RenderPdf("<html><body><p>Page content</p></body></html>");
            var texts = PdfToImage.ExtractAllText(pdf);

            Assert.Single(texts);
            Assert.Contains("Page", texts[0]);
        }

        [Fact]
        public void ExtractText_NullData_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => PdfToImage.ExtractText(null!));
        }

        [Fact]
        public void ExtractText_EmptyBody_ReturnsEmpty()
        {
            var pdf = RenderPdf("<html><body></body></html>");
            var text = PdfToImage.ExtractText(pdf);
            // Empty body may have no text at all
            Assert.NotNull(text);
        }

        #endregion

        #region RenderPageToBitmap Tests

        [Fact]
        public void RenderPageToBitmap_ReturnsBitmap()
        {
            var pdf = RenderPdf("<html><body><p>Test</p></body></html>", 200, 100);
            using var bitmap = PdfToImage.RenderPageToBitmap(pdf, 0, 72f);

            Assert.NotNull(bitmap);
            Assert.Equal(200, bitmap.Width);
            Assert.Equal(100, bitmap.Height);
        }

        #endregion

        #region PdfObj Tests

        [Fact]
        public void PdfObj_Null_Defaults()
        {
            var obj = PdfObj.Null;
            Assert.True(obj.IsNull);
            Assert.Equal(0, obj.AsInt());
            Assert.Equal(0.0, obj.AsReal());
            Assert.Equal("", obj.AsName());
            Assert.Equal(0, obj.Count);
            Assert.True(obj["anything"].IsNull);
            Assert.True(obj[0].IsNull);
        }

        [Fact]
        public void PdfObj_SafeChaining()
        {
            // Null-safe chaining: accessing nested keys on null returns null, never throws
            var obj = PdfObj.Null;
            var result = obj["Level1"]["Level2"]["Level3"];
            Assert.True(result.IsNull);
        }

        #endregion

        #region Round-trip Tests

        [Fact]
        public void RoundTrip_SimpleHtml_ProducesVisibleOutput()
        {
            var html = @"<!DOCTYPE html>
<html><head><style>
  body { margin: 0; }
  .box { background: #3498db; width: 150px; height: 80px; margin: 20px; }
</style></head>
<body><div class='box'></div></body></html>";

            var pdf = RenderPdf(html, 200, 150);
            _output.WriteLine($"PDF size: {pdf.Length} bytes");

            var png = PdfToImage.RenderPage(pdf, 0, 72f);
            _output.WriteLine($"PNG size: {png.Length} bytes");

            using var bitmap = SKBitmap.Decode(png);
            Assert.Equal(200, bitmap.Width);
            Assert.Equal(150, bitmap.Height);

            // Blue box should be at (20, 20)
            var bluePixel = bitmap.GetPixel(90, 60);
            Assert.InRange(bluePixel.Red, 40, 70);  // #3498db R=52
            Assert.InRange(bluePixel.Green, 130, 170); // G=152
            Assert.InRange(bluePixel.Blue, 200, 230); // B=219
        }

        [Fact]
        public void RoundTrip_Table_Renders()
        {
            var html = @"<!DOCTYPE html>
<html><head><style>
  body { margin: 0; padding: 10px; }
  table { border-collapse: collapse; width: 180px; }
  th { background: #2c3e50; color: white; padding: 4px; }
  td { padding: 4px; border: 1px solid #ddd; }
</style></head>
<body>
<table>
  <tr><th>Name</th><th>Value</th></tr>
  <tr><td>A</td><td>1</td></tr>
  <tr><td>B</td><td>2</td></tr>
</table>
</body></html>";

            var pdf = RenderPdf(html, 200, 200);
            var png = PdfToImage.RenderPage(pdf, 0, 72f);

            using var bitmap = SKBitmap.Decode(png);

            // Table header should be dark (#2c3e50)
            var headerPixel = bitmap.GetPixel(100, 18);
            _output.WriteLine($"Header pixel: R={headerPixel.Red} G={headerPixel.Green} B={headerPixel.Blue}");
            Assert.True(headerPixel.Red < 80, "Header should be dark");
        }

        [Fact]
        public void RoundTrip_ComplexDocument_DoesNotCrash()
        {
            var html = @"<!DOCTYPE html>
<html><head><style>
  body { font-family: sans-serif; margin: 10px; }
  h1 { color: #2c3e50; font-size: 18px; border-bottom: 2px solid #3498db; }
  .card { background: #ecf0f1; border-radius: 4px; padding: 8px; margin: 6px 0; border-left: 3px solid #e74c3c; }
  .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 6px; }
  .stat { background: white; padding: 6px; text-align: center; }
  table { width: 100%; border-collapse: collapse; }
  th { background: #3498db; color: white; padding: 4px; }
  td { padding: 4px; border-bottom: 1px solid #ddd; }
</style></head>
<body>
  <h1>Report</h1>
  <div class='grid'>
    <div class='stat'>$12K</div>
    <div class='stat'>847</div>
  </div>
  <table>
    <tr><th>Metric</th><th>Value</th></tr>
    <tr><td>Speed</td><td>120ms</td></tr>
    <tr><td>Errors</td><td>0.3%</td></tr>
  </table>
  <div class='card'>Note: round-trip test</div>
</body></html>";

            var pdf = RenderPdf(html, 400, 400);
            Assert.True(pdf.Length > 0);

            var png = PdfToImage.RenderPage(pdf, 0, 150f);
            Assert.True(png.Length > 0);

            var (w, h) = PdfToImage.GetPageSize(pdf);
            Assert.InRange(w, 399f, 401f);
            Assert.InRange(h, 399f, 401f);

            _output.WriteLine($"PDF: {pdf.Length} bytes, PNG: {png.Length} bytes, Page: {w}x{h}pt");
        }

        #endregion
    }
}
