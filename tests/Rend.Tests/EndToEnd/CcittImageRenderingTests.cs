using System;
using System.IO;
using Rend.Pdf;
using SkiaSharp;
using Xunit;

namespace Rend.Tests.EndToEnd
{
    /// <summary>
    /// End-to-end raster verification for CCITT Group 4 image XObjects: write a bilevel image
    /// with the PDF writer, then rasterize it through the PDF reader and assert known pixels.
    /// Exercises the full chain — CcittFaxCodec.EncodeG4 → CCITTFaxDecode filter → DeviceGray
    /// 1bpp bitmap → page raster.
    /// </summary>
    public class CcittImageRenderingTests
    {
        private const int Size = 240;
        private const int BorderThickness = 6;
        private const int DiamondRadius = 90;

        [Fact]
        public void DiamondCcittImage_RastersToExpectedPixels()
        {
            byte[] packed = BuildDiamondPacked();

            byte[] pdfBytes;
            using (var document = new PdfDocument())
            {
                PdfImage image = document.AddCcittImage(packed, Size, Size, blackIs1: false);
                PdfPage page = document.AddPage(Size, Size);
                page.Content.DrawImage(image, Size, 0f, 0f, Size, 0f, 0f);
                using var buffer = new MemoryStream();
                document.Save(buffer);
                pdfBytes = buffer.ToArray();
            }

            using SKBitmap bitmap = PdfToImage.RenderPageToBitmap(pdfBytes, 0, 72f);
            Assert.Equal(Size, bitmap.Width);
            Assert.Equal(Size, bitmap.Height);

            // Diamond is centered and symmetric under both flips, so orientation is irrelevant.
            AssertBlack(bitmap, Size / 2, Size / 2, "diamond center");
            AssertBlack(bitmap, BorderThickness / 2, Size / 2, "left border");
            AssertBlack(bitmap, Size / 2, BorderThickness / 2, "top border");

            // At the vertical midline the diamond spans x in (30, 210); x=15 is past the
            // 6px border yet outside the diamond → background white.
            AssertWhite(bitmap, 15, Size / 2, "gap between border and diamond");
            AssertWhite(bitmap, Size - 16, Size / 2, "gap on the right side");

            int blackPixels = CountBlackPixels(bitmap);
            int expected = CountExpectedBlackPixels();
            Assert.InRange(blackPixels, (int)(expected * 0.9), (int)(expected * 1.1));
        }

        private static byte[] BuildDiamondPacked()
        {
            int rowBytes = (Size + 7) / 8;
            var packed = new byte[rowBytes * Size];
            for (int i = 0; i < packed.Length; i++)
            {
                packed[i] = 0xFF; // blackIs1=false: bit 1 = white.
            }
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    if (IsBlack(x, y))
                    {
                        packed[y * rowBytes + (x >> 3)] &= (byte)~(1 << (7 - (x & 7)));
                    }
                }
            }
            return packed;
        }

        private static bool IsBlack(int x, int y)
        {
            bool border = x < BorderThickness || x >= Size - BorderThickness
                          || y < BorderThickness || y >= Size - BorderThickness;
            bool diamond = Math.Abs(x - Size / 2) + Math.Abs(y - Size / 2) < DiamondRadius;
            return border || diamond;
        }

        private static int CountExpectedBlackPixels()
        {
            int count = 0;
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    if (IsBlack(x, y))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private static int CountBlackPixels(SKBitmap bitmap)
        {
            int count = 0;
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    SKColor pixel = bitmap.GetPixel(x, y);
                    if (pixel.Red < 128 && pixel.Green < 128 && pixel.Blue < 128)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private static void AssertBlack(SKBitmap bitmap, int x, int y, string where)
        {
            SKColor pixel = bitmap.GetPixel(x, y);
            Assert.True(pixel.Red < 80 && pixel.Green < 80 && pixel.Blue < 80,
                $"expected black at {where} ({x},{y}) but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
        }

        private static void AssertWhite(SKBitmap bitmap, int x, int y, string where)
        {
            SKColor pixel = bitmap.GetPixel(x, y);
            Assert.True(pixel.Red > 200 && pixel.Green > 200 && pixel.Blue > 200,
                $"expected white at {where} ({x},{y}) but got R={pixel.Red} G={pixel.Green} B={pixel.Blue}");
        }
    }
}
