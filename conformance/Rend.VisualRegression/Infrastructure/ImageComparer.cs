using SkiaSharp;

namespace Rend.VisualRegression.Infrastructure
{
    /// <summary>
    /// Pixel-by-pixel image comparison using SkiaSharp.
    /// </summary>
    public static class ImageComparer
    {
        /// <summary>
        /// Compare two PNG images pixel by pixel.
        /// Returns (diffFraction, diffPixels, totalPixels).
        /// </summary>
        /// <param name="expectedPng">PNG byte data of the expected (reference) image.</param>
        /// <param name="actualPng">PNG byte data of the actual (rendered) image.</param>
        /// <param name="perChannelThreshold">Per-channel tolerance (0-255). Default 0 means exact match.</param>
        public static (double DiffFraction, int DiffPixels, int TotalPixels) Compare(
            byte[] expectedPng, byte[] actualPng, int perChannelThreshold = 0)
        {
            using var expectedBitmap = SKBitmap.Decode(expectedPng);
            using var actualBitmap = SKBitmap.Decode(actualPng);

            if (expectedBitmap == null || actualBitmap == null)
            {
                return (1.0, 1, 1);
            }

            // If dimensions differ, return 100% diff
            if (expectedBitmap.Width != actualBitmap.Width ||
                expectedBitmap.Height != actualBitmap.Height)
            {
                int totalPixels = System.Math.Max(
                    expectedBitmap.Width * expectedBitmap.Height,
                    actualBitmap.Width * actualBitmap.Height);
                return (1.0, totalPixels, totalPixels);
            }

            int width = expectedBitmap.Width;
            int height = expectedBitmap.Height;
            int total = width * height;
            int diffCount = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var expectedPixel = expectedBitmap.GetPixel(x, y);
                    var actualPixel = actualBitmap.GetPixel(x, y);

                    if (!PixelsMatch(expectedPixel, actualPixel, perChannelThreshold))
                    {
                        diffCount++;
                    }
                }
            }

            double fraction = total > 0 ? (double)diffCount / total : 0.0;
            return (fraction, diffCount, total);
        }

        private static bool PixelsMatch(SKColor a, SKColor b, int threshold)
        {
            return System.Math.Abs(a.Red - b.Red) <= threshold &&
                   System.Math.Abs(a.Green - b.Green) <= threshold &&
                   System.Math.Abs(a.Blue - b.Blue) <= threshold &&
                   System.Math.Abs(a.Alpha - b.Alpha) <= threshold;
        }
    }
}
