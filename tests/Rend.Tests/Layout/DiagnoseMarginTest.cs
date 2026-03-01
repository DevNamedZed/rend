using System;
using Rend;
using Rend.Core.Values;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class DiagnoseMarginTest
    {
        private readonly ITestOutputHelper _output;

        public DiagnoseMarginTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void DiagnoseMarginBottomGap_Simple()
        {
            // Simplest possible test: two divs with a large margin-bottom gap, NO padding
            var html = @"<html><body style=""margin:0; padding:0; background:#ffffff;"">
    <div style=""background:#ff0000; padding:0; margin:0; margin-bottom:20px; height:40px;"">A</div>
    <div style=""background:#00ff00; padding:0; margin:0; height:40px;"">B</div>
</body></html>";

            var options = new RenderOptions
            {
                PageSize = new SizeF(100, 200),
                MarginTop = 0,
                MarginRight = 0,
                MarginBottom = 0,
                MarginLeft = 0,
                Dpi = 96,
                ImageFormat = "png",
            };

            byte[] imageData;
            try
            {
                imageData = Render.ToImage(html, options);
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is TypeInitializationException || ex.InnerException is DllNotFoundException)
            {
                _output.WriteLine($"Skipped: {ex.Message}");
                return;
            }

            _output.WriteLine($"=== SIMPLE TEST (no padding) ===");
            _output.WriteLine($"Image size: {imageData.Length} bytes");

            // Expected layout if margin-bottom:20px works:
            //   Y=0..39: red - div A (height:40)
            //   Y=40..59: white - margin gap (20px)
            //   Y=60..99: green - div B (height:40)
            AnalyzeBitmap(imageData, 50, 120, "SIMPLE");
        }

        [Fact]
        public void DiagnoseMarginBottomGap_ExactOriginal()
        {
            // EXACT same HTML as the color-background visual regression test
            var html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""background:#e74c3c; color:#fff; padding:10px; margin-bottom:6px; height:20px;"">Red background</div>
                    <div style=""background:#27ae60; color:#fff; padding:10px; margin-bottom:6px; height:20px;"">Green background</div>
                    <div style=""background:#3498db; color:#fff; padding:10px; margin-bottom:6px; height:20px;"">Blue background</div>
                    <div style=""background:#2c3e50; color:#ecf0f1; padding:10px; height:20px;"">Dark background</div>
                </body></html>";

            var options = new RenderOptions
            {
                PageSize = new SizeF(400, 300),
                MarginTop = 0,
                MarginRight = 0,
                MarginBottom = 0,
                MarginLeft = 0,
                Dpi = 96,
                ImageFormat = "png",
            };

            byte[] imageData;
            try
            {
                imageData = Render.ToImage(html, options);
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is TypeInitializationException || ex.InnerException is DllNotFoundException)
            {
                _output.WriteLine($"Skipped: {ex.Message}");
                return;
            }

            _output.WriteLine($"=== EXACT ORIGINAL TEST (padding:10px, margin-bottom:6px, height:20px) ===");
            _output.WriteLine($"Image size: {imageData.Length} bytes");

            // Expected layout with body padding:10px:
            //   Y=0..9: white (body padding-top)
            //   Y=10..49: red (div 1: padding-top:10 + height:20 + padding-bottom:10 = 40px)
            //   Y=50..55: white (margin-bottom gap: 6px)
            //   Y=56..95: green (div 2: 40px)
            //   Y=96..101: white (margin gap: 6px)
            //   Y=102..141: blue (div 3: 40px)
            //   Y=142..147: white (margin gap: 6px)
            //   Y=148..187: dark (div 4: 40px)
            _output.WriteLine($"Expected: body-padding(10) + div(40) + gap(6) + div(40) + gap(6) + div(40) + gap(6) + div(40) + body-padding(10)");
            _output.WriteLine($"Expected total: 10 + 40 + 6 + 40 + 6 + 40 + 6 + 40 + 10 = 198px content");

            // Scan every row at x=200 (middle) from y=0 to y=200
            AnalyzeBitmap(imageData, 200, 200, "ORIGINAL");
        }

        [Fact]
        public void DiagnoseMarginBottomGap_WithPaddingNoBoyPadding()
        {
            // Test with div padding:10px but NO body padding - isolate the padding effect
            var html = @"<html><body style=""margin:0; padding:0; background:#ffffff;"">
    <div style=""background:#ff0000; padding:10px; margin-bottom:6px; height:20px;"">A</div>
    <div style=""background:#00ff00; padding:10px; margin:0; height:20px;"">B</div>
</body></html>";

            var options = new RenderOptions
            {
                PageSize = new SizeF(200, 200),
                MarginTop = 0,
                MarginRight = 0,
                MarginBottom = 0,
                MarginLeft = 0,
                Dpi = 96,
                ImageFormat = "png",
            };

            byte[] imageData;
            try
            {
                imageData = Render.ToImage(html, options);
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is TypeInitializationException || ex.InnerException is DllNotFoundException)
            {
                _output.WriteLine($"Skipped: {ex.Message}");
                return;
            }

            _output.WriteLine($"=== WITH PADDING TEST (div padding:10px, no body padding) ===");
            _output.WriteLine($"Image size: {imageData.Length} bytes");

            // Expected:
            //   Y=0..39: red (padding-top:10 + height:20 + padding-bottom:10 = 40px)
            //   Y=40..45: white (margin-bottom gap: 6px)
            //   Y=46..85: green (40px)
            AnalyzeBitmap(imageData, 100, 120, "WITH_PADDING");
        }

        private void AnalyzeBitmap(byte[] imageData, int sampleX, int maxY, string label)
        {
            try
            {
                var skBitmap = SkiaSharp.SKBitmap.Decode(imageData);
                _output.WriteLine($"Bitmap: {skBitmap.Width}x{skBitmap.Height}");

                string prevColor = "";
                int regionStart = 0;
                for (int y = 0; y <= Math.Min(maxY, skBitmap.Height - 1); y++)
                {
                    var pixel = skBitmap.GetPixel(sampleX, y);
                    string color = ClassifyColor(pixel);

                    if (color != prevColor)
                    {
                        if (y > 0)
                        {
                            _output.WriteLine($"  Y={regionStart,3}..{y - 1,3} ({y - regionStart,3}px): {prevColor}");
                        }
                        regionStart = y;
                        prevColor = color;
                    }
                }
                // Print last region
                int lastY = Math.Min(maxY, skBitmap.Height - 1);
                _output.WriteLine($"  Y={regionStart,3}..{lastY,3} ({lastY - regionStart + 1,3}px): {prevColor}");

                // Summary: check specific gap positions
                _output.WriteLine($"");
                _output.WriteLine($"[{label}] Key pixel samples:");
                int[] checkPoints = { 0, 10, 39, 40, 45, 46, 49, 50, 55, 56, 59, 60, 85, 86, 95, 96, 101, 102, 141, 142, 147, 148, 187 };
                foreach (int y in checkPoints)
                {
                    if (y < skBitmap.Height)
                    {
                        var p = skBitmap.GetPixel(sampleX, y);
                        _output.WriteLine($"  Y={y,3}: R={p.Red,3} G={p.Green,3} B={p.Blue,3} => {ClassifyColor(p)}");
                    }
                }

                skBitmap.Dispose();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"SkiaSharp decode failed: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static string ClassifyColor(SkiaSharp.SKColor pixel)
        {
            // Red (#e74c3c or #ff0000)
            if (pixel.Red > 180 && pixel.Green < 100 && pixel.Blue < 100) return "RED";
            // Green (#27ae60 or #00ff00)
            if (pixel.Green > 150 && pixel.Red < 100 && pixel.Blue < 100) return "GREEN";
            // Blue (#3498db)
            if (pixel.Blue > 180 && pixel.Red < 100 && pixel.Green < 180) return "BLUE";
            // Dark (#2c3e50)
            if (pixel.Red < 80 && pixel.Green < 80 && pixel.Blue < 100 && pixel.Red > 20) return "DARK";
            // White / near-white
            if (pixel.Red > 220 && pixel.Green > 220 && pixel.Blue > 220) return "WHITE";
            // Light gray (body background for opacity test)
            if (pixel.Red > 200 && pixel.Green > 200 && pixel.Blue > 200) return "LIGHT_GRAY";
            return $"OTHER(R={pixel.Red},G={pixel.Green},B={pixel.Blue})";
        }
    }
}
