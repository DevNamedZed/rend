using System;
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
        public static (double DiffFraction, int DiffPixels, int TotalPixels) Compare(
            byte[] expectedPng, byte[] actualPng, int perChannelThreshold = 0)
        {
            using var expectedBitmap = SKBitmap.Decode(expectedPng);
            using var actualBitmap = SKBitmap.Decode(actualPng);

            if (expectedBitmap == null || actualBitmap == null)
            {
                return (1.0, 1, 1);
            }

            return CompareDecoded(expectedBitmap, actualBitmap, perChannelThreshold);
        }

        /// <summary>
        /// Compare two already-decoded bitmaps pixel by pixel.
        /// </summary>
        internal static (double DiffFraction, int DiffPixels, int TotalPixels) CompareDecoded(
            SKBitmap expectedBitmap, SKBitmap actualBitmap, int perChannelThreshold = 0)
        {
            int width = Math.Max(expectedBitmap.Width, actualBitmap.Width);
            int height = Math.Max(expectedBitmap.Height, actualBitmap.Height);
            int total = width * height;
            int diffCount = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inExpected = x < expectedBitmap.Width && y < expectedBitmap.Height;
                    bool inActual = x < actualBitmap.Width && y < actualBitmap.Height;

                    if (!inExpected || !inActual)
                    {
                        diffCount++;
                        continue;
                    }

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

        /// <summary>
        /// Compare two bitmaps with 1-pixel shift tolerance.
        /// A pixel that doesn't match at (x,y) is forgiven if it matches any
        /// neighbor in a 3x3 area of the other image. Returns both strict and
        /// shift-tolerant diff counts.
        /// </summary>
        internal static (int StrictDiffPixels, int ShiftTolerantDiffPixels, int TotalPixels) CompareWithShiftTolerance(
            SKBitmap expectedBitmap, SKBitmap actualBitmap, int perChannelThreshold = 0)
        {
            int width = Math.Max(expectedBitmap.Width, actualBitmap.Width);
            int height = Math.Max(expectedBitmap.Height, actualBitmap.Height);
            int total = width * height;
            int strictDiff = 0;
            int shiftDiff = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inExpected = x < expectedBitmap.Width && y < expectedBitmap.Height;
                    bool inActual = x < actualBitmap.Width && y < actualBitmap.Height;

                    if (!inExpected || !inActual)
                    {
                        strictDiff++;
                        shiftDiff++;
                        continue;
                    }

                    var expectedPixel = expectedBitmap.GetPixel(x, y);
                    var actualPixel = actualBitmap.GetPixel(x, y);

                    if (PixelsMatch(expectedPixel, actualPixel, perChannelThreshold))
                        continue;

                    strictDiff++;

                    // Check if actual pixel matches any expected neighbor (1px shift)
                    if (MatchesNeighbor(expectedBitmap, x, y, actualPixel, perChannelThreshold))
                        continue;

                    // Check if expected pixel matches any actual neighbor (1px shift)
                    if (MatchesNeighbor(actualBitmap, x, y, expectedPixel, perChannelThreshold))
                        continue;

                    // No neighbor match — this is a real diff even with shift tolerance
                    shiftDiff++;
                }
            }

            return (strictDiff, shiftDiff, total);
        }

        /// <summary>
        /// Check if pixel matches any pixel in a 3x3 neighborhood in the bitmap.
        /// </summary>
        private static bool MatchesNeighbor(SKBitmap bitmap, int cx, int cy, SKColor pixel, int threshold)
        {
            int w = bitmap.Width;
            int h = bitmap.Height;

            for (int dy = -1; dy <= 1; dy++)
            {
                int ny = cy + dy;
                if (ny < 0 || ny >= h) continue;

                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue; // skip center, already checked

                    int nx = cx + dx;
                    if (nx < 0 || nx >= w) continue;

                    if (PixelsMatch(bitmap.GetPixel(nx, ny), pixel, threshold))
                        return true;
                }
            }

            return false;
        }

        internal static bool PixelsMatch(SKColor a, SKColor b, int threshold)
        {
            return Math.Abs(a.Red - b.Red) <= threshold &&
                   Math.Abs(a.Green - b.Green) <= threshold &&
                   Math.Abs(a.Blue - b.Blue) <= threshold &&
                   Math.Abs(a.Alpha - b.Alpha) <= threshold;
        }
    }
}
