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

            return GenerateDiffDecoded(expectedBitmap, actualBitmap, perChannelThreshold);
        }

        /// <summary>
        /// Generate a diff PNG from already-decoded bitmaps.
        /// </summary>
        internal static byte[] GenerateDiffDecoded(SKBitmap expectedBitmap, SKBitmap actualBitmap, int perChannelThreshold = 0)
        {
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
                        diffBitmap.SetPixel(x, y, new SKColor(255, 0, 255, 255));
                    }
                    else
                    {
                        var expectedPixel = expectedBitmap.GetPixel(x, y);
                        var actualPixel = actualBitmap.GetPixel(x, y);

                        if (ImageComparer.PixelsMatch(expectedPixel, actualPixel, perChannelThreshold))
                        {
                            byte gray = (byte)((expectedPixel.Red * 0.299 +
                                                 expectedPixel.Green * 0.587 +
                                                 expectedPixel.Blue * 0.114));
                            byte alpha = (byte)(255 * 0.3);
                            diffBitmap.SetPixel(x, y, new SKColor(gray, gray, gray, alpha));
                        }
                        else
                        {
                            diffBitmap.SetPixel(x, y, new SKColor(255, 0, 0, 255));
                        }
                    }
                }
            }

            using var image = SKImage.FromBitmap(diffBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        /// <summary>
        /// Compare and optionally generate diff in a single decode pass.
        /// Returns strict comparison, shift-tolerant comparison, and diff PNG.
        /// </summary>
        public static CompareAndDiffResult CompareAndDiff(
            byte[] expectedPng, byte[] actualPng, int perChannelThreshold = 0)
        {
            using var expectedBitmap = SKBitmap.Decode(expectedPng);
            using var actualBitmap = SKBitmap.Decode(actualPng);

            if (expectedBitmap == null || actualBitmap == null)
            {
                return new CompareAndDiffResult(1.0, 1, 1.0, 1, 1, null);
            }

            // Strict comparison
            var (_, strictDiff, totalPixels) = ImageComparer.CompareDecoded(
                expectedBitmap, actualBitmap, perChannelThreshold);

            // Shift-tolerant comparison (only if there are strict diffs)
            int shiftDiff = 0;
            if (strictDiff > 0)
            {
                var (_, sd, _) = ImageComparer.CompareWithShiftTolerance(
                    expectedBitmap, actualBitmap, perChannelThreshold);
                shiftDiff = sd;
            }

            double strictFraction = totalPixels > 0 ? (double)strictDiff / totalPixels : 0.0;
            double shiftFraction = totalPixels > 0 ? (double)shiftDiff / totalPixels : 0.0;

            byte[]? diffPng = null;
            if (strictDiff > 0)
            {
                diffPng = GenerateDiffDecoded(expectedBitmap, actualBitmap, perChannelThreshold);
            }

            return new CompareAndDiffResult(strictFraction, strictDiff, shiftFraction, shiftDiff, totalPixels, diffPng);
        }
    }

    /// <summary>
    /// Result of a combined compare-and-diff operation.
    /// </summary>
    public readonly struct CompareAndDiffResult
    {
        public readonly double StrictDiffFraction;
        public readonly int StrictDiffPixels;
        public readonly double ShiftTolerantDiffFraction;
        public readonly int ShiftTolerantDiffPixels;
        public readonly int TotalPixels;
        public readonly byte[]? DiffPng;

        public CompareAndDiffResult(double strictFraction, int strictPixels,
            double shiftFraction, int shiftPixels, int totalPixels, byte[]? diffPng)
        {
            StrictDiffFraction = strictFraction;
            StrictDiffPixels = strictPixels;
            ShiftTolerantDiffFraction = shiftFraction;
            ShiftTolerantDiffPixels = shiftPixels;
            TotalPixels = totalPixels;
            DiffPng = diffPng;
        }
    }
}
