using System;
using SkiaSharp;

namespace Rend.VisualRegression.Infrastructure
{
    /// <summary>
    /// Generates a visual diff image highlighting pixel differences between two images.
    /// </summary>
    public static class ImageDiffer
    {
        /// <summary>
        /// Generate a diff PNG image.
        /// Matching pixels are rendered as dim grayscale (30% opacity).
        /// Differing pixels are rendered as bright red (#FF0000).
        /// Out-of-bounds pixels (when dimensions differ) are rendered as magenta (#FF00FF).
        /// </summary>
        public static byte[] GenerateDiff(byte[] expectedPng, byte[] actualPng, int perChannelThreshold = 0)
        {
            using var expectedBitmap = SKBitmap.Decode(expectedPng);
            using var actualBitmap = SKBitmap.Decode(actualPng);

            if (expectedBitmap == null || actualBitmap == null)
            {
                return Array.Empty<byte>();
            }

            int width = Math.Max(expectedBitmap.Width, actualBitmap.Width);
            int height = Math.Max(expectedBitmap.Height, actualBitmap.Height);

            using var diffBitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inExpected = x < expectedBitmap.Width && y < expectedBitmap.Height;
                    bool inActual = x < actualBitmap.Width && y < actualBitmap.Height;

                    if (!inExpected || !inActual)
                    {
                        // Out of bounds: magenta
                        diffBitmap.SetPixel(x, y, new SKColor(255, 0, 255, 255));
                    }
                    else
                    {
                        var expectedPixel = expectedBitmap.GetPixel(x, y);
                        var actualPixel = actualBitmap.GetPixel(x, y);

                        if (PixelsMatch(expectedPixel, actualPixel, perChannelThreshold))
                        {
                            // Matching: dim grayscale at 30% opacity
                            byte gray = (byte)((expectedPixel.Red * 0.299 +
                                                 expectedPixel.Green * 0.587 +
                                                 expectedPixel.Blue * 0.114));
                            byte alpha = (byte)(255 * 0.3);
                            diffBitmap.SetPixel(x, y, new SKColor(gray, gray, gray, alpha));
                        }
                        else
                        {
                            // Different: bright red
                            diffBitmap.SetPixel(x, y, new SKColor(255, 0, 0, 255));
                        }
                    }
                }
            }

            using var image = SKImage.FromBitmap(diffBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private static bool PixelsMatch(SKColor a, SKColor b, int threshold)
        {
            return Math.Abs(a.Red - b.Red) <= threshold &&
                   Math.Abs(a.Green - b.Green) <= threshold &&
                   Math.Abs(a.Blue - b.Blue) <= threshold &&
                   Math.Abs(a.Alpha - b.Alpha) <= threshold;
        }
    }
}
