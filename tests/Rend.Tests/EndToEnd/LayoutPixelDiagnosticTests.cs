using System;
using Rend;
using Rend.Core.Values;
using SkiaSharp;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.EndToEnd
{
    /// <summary>
    /// Diagnostic tests that render HTML to images and measure actual pixel dimensions
    /// to detect layout width issues.
    /// </summary>
    public class LayoutPixelDiagnosticTests
    {
        private readonly ITestOutputHelper _output;

        public LayoutPixelDiagnosticTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private RenderOptions MakeOptions(int width = 400, int height = 300)
        {
            return new RenderOptions
            {
                PageSize = new SizeF(width, height),
                MarginTop = 0,
                MarginRight = 0,
                MarginBottom = 0,
                MarginLeft = 0,
                Dpi = 96,
                ImageFormat = "png"
            };
        }

        private (int firstRedX, int lastRedX, int redWidth, int imageWidth) MeasureRedWidthOnRow(
            byte[] pngData, int row = 5)
        {
            using var bitmap = SKBitmap.Decode(pngData);
            int w = bitmap.Width;
            int firstRed = -1;
            int lastRed = -1;

            for (int x = 0; x < w; x++)
            {
                var pixel = bitmap.GetPixel(x, row);
                // Check if pixel is "red-ish" (R > 200, G < 80, B < 80)
                if (pixel.Red > 200 && pixel.Green < 80 && pixel.Blue < 80)
                {
                    if (firstRed < 0) firstRed = x;
                    lastRed = x;
                }
            }

            int redWidth = firstRed >= 0 ? lastRed - firstRed + 1 : 0;
            return (firstRed, lastRed, redWidth, w);
        }

        [Fact]
        public void FullWidthRedDiv_NoMarginNoPadding_Fills400px()
        {
            // Simplest possible case: no margins, no padding, full-width red div
            var html = @"<html><body style='margin:0; padding:0;'>
                <div style='background:red; height:50px;'></div>
            </body></html>";

            var png = Render.ToImage(html, MakeOptions());
            var (firstRed, lastRed, redWidth, imageWidth) = MeasureRedWidthOnRow(png, 10);

            _output.WriteLine($"Image width: {imageWidth}");
            _output.WriteLine($"Red range: {firstRed} to {lastRed}, width={redWidth}");

            Assert.Equal(400, imageWidth);
            Assert.Equal(0, firstRed);
            Assert.Equal(399, lastRed);
            Assert.Equal(400, redWidth);
        }

        [Fact]
        public void FullWidthRedDiv_BodyPadding10_Fills380px()
        {
            // Body with 10px padding: red div should fill 380px (10 to 389)
            var html = @"<html><body style='margin:0; padding:10px;'>
                <div style='background:red; height:50px;'></div>
            </body></html>";

            var png = Render.ToImage(html, MakeOptions());
            var (firstRed, lastRed, redWidth, imageWidth) = MeasureRedWidthOnRow(png, 15);

            _output.WriteLine($"Image width: {imageWidth}");
            _output.WriteLine($"Red range: {firstRed} to {lastRed}, width={redWidth}");

            // Expected: 10px padding on each side, so red from x=10 to x=389 = 380px
            Assert.Equal(400, imageWidth);
            Assert.True(redWidth >= 375 && redWidth <= 385,
                $"Expected ~380px red width, got {redWidth}px (first={firstRed}, last={lastRed})");
        }

        [Fact]
        public void FullWidthRedDiv_BodyMargin8_Fills384px()
        {
            // Default body margin (8px per UA): red div should fill 384px
            var html = @"<html><body>
                <div style='background:red; height:50px;'></div>
            </body></html>";

            var png = Render.ToImage(html, MakeOptions());
            var (firstRed, lastRed, redWidth, imageWidth) = MeasureRedWidthOnRow(png, 15);

            _output.WriteLine($"Image width: {imageWidth}");
            _output.WriteLine($"Red range: {firstRed} to {lastRed}, width={redWidth}");

            // Expected: 8px margin on each side, so red from x=8 to x=391 = 384px
            Assert.Equal(400, imageWidth);
            Assert.True(redWidth >= 379 && redWidth <= 389,
                $"Expected ~384px red width, got {redWidth}px (first={firstRed}, last={lastRed})");
        }

        [Fact]
        public void RedDiv_ExplicitWidth200_Is200px()
        {
            var html = @"<html><body style='margin:0; padding:0;'>
                <div style='background:red; width:200px; height:50px;'></div>
            </body></html>";

            var png = Render.ToImage(html, MakeOptions());
            var (firstRed, lastRed, redWidth, imageWidth) = MeasureRedWidthOnRow(png, 10);

            _output.WriteLine($"Image width: {imageWidth}");
            _output.WriteLine($"Red range: {firstRed} to {lastRed}, width={redWidth}");

            Assert.Equal(400, imageWidth);
            Assert.True(redWidth >= 198 && redWidth <= 202,
                $"Expected ~200px red width, got {redWidth}px (first={firstRed}, last={lastRed})");
        }

        [Fact]
        public void NestedDivs_FullWidth()
        {
            // Outer div with padding, inner div should fill remaining width
            var html = @"<html><body style='margin:0; padding:0;'>
                <div style='padding:20px; background:blue;'>
                    <div style='background:red; height:50px;'></div>
                </div>
            </body></html>";

            var png = Render.ToImage(html, MakeOptions());
            var (firstRed, lastRed, redWidth, imageWidth) = MeasureRedWidthOnRow(png, 25);

            _output.WriteLine($"Image width: {imageWidth}");
            _output.WriteLine($"Red range: {firstRed} to {lastRed}, width={redWidth}");

            // Expected: 20px padding on each side of outer div, so red = 360px (20 to 379)
            Assert.True(redWidth >= 355 && redWidth <= 365,
                $"Expected ~360px red width, got {redWidth}px (first={firstRed}, last={lastRed})");
        }

        [Fact]
        public void DumpLayoutDimensions_BasicParagraph()
        {
            // This is the failing visual regression test case - dump pixel data
            var html = @"<html><body style='margin:0; padding:10px; font-family:sans-serif; font-size:14px;'>
                    <p style='color:#333;'>This is a simple paragraph of text used to verify basic text rendering. It should wrap naturally within the given page width.</p>
                </body></html>";

            var png = Render.ToImage(html, MakeOptions());
            using var bitmap = SKBitmap.Decode(png);

            _output.WriteLine($"Image dimensions: {bitmap.Width}x{bitmap.Height}");

            // Scan a few rows for non-white pixels to understand layout bounds
            for (int row = 0; row < Math.Min(bitmap.Height, 80); row += 5)
            {
                int firstNonWhite = -1;
                int lastNonWhite = -1;
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, row);
                    // Non-white check (text pixels, etc.)
                    if (pixel.Red < 240 || pixel.Green < 240 || pixel.Blue < 240)
                    {
                        if (firstNonWhite < 0) firstNonWhite = x;
                        lastNonWhite = x;
                    }
                }
                if (firstNonWhite >= 0)
                {
                    _output.WriteLine($"  Row {row}: non-white from x={firstNonWhite} to x={lastNonWhite} (span={lastNonWhite - firstNonWhite + 1}px)");
                }
            }
        }

        [Fact]
        public void ColorBackground_FullWidth()
        {
            // Matches the visual regression test "color-background"
            var html = @"<html><body style='margin:0; padding:10px; font-family:sans-serif; font-size:14px;'>
                    <div style='background:#e74c3c; color:#fff; padding:10px; margin-bottom:6px;'>Red background</div>
                    <div style='background:#27ae60; color:#fff; padding:10px; margin-bottom:6px;'>Green background</div>
                    <div style='background:#3498db; color:#fff; padding:10px; margin-bottom:6px;'>Blue background</div>
                    <div style='background:#2c3e50; color:#ecf0f1; padding:10px;'>Dark background</div>
                </body></html>";

            var png = Render.ToImage(html, MakeOptions());
            var (firstRed, lastRed, redWidth, imageWidth) = MeasureRedWidthOnRow(png, 15);

            _output.WriteLine($"Image width: {imageWidth}");
            _output.WriteLine($"Red div: x={firstRed} to x={lastRed}, width={redWidth}px");

            // Body has 10px padding. Div has 10px padding but no explicit width (auto).
            // Div auto width = body content width (380) - div horizontal spacing (0 margin + 0 border + 0 margin = 0)
            // Div padding-box (visible background area) = 380px wide, from x=10 to x=389
            Assert.True(redWidth >= 375 && redWidth <= 385,
                $"Expected ~380px red background, got {redWidth}px (first={firstRed}, last={lastRed})");
        }
    }
}
