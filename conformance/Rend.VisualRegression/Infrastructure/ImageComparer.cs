using System;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace Rend.VisualRegression.Infrastructure
{
    /// <summary>
    /// High-performance pixel-by-pixel image comparison using direct byte access.
    /// Avoids SKBitmap.GetPixel() P/Invoke overhead by working with raw pixel spans.
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
        /// Compare two already-decoded bitmaps using direct pixel spans.
        /// </summary>
        internal static (double DiffFraction, int DiffPixels, int TotalPixels) CompareDecoded(
            SKBitmap expectedBitmap, SKBitmap actualBitmap, int perChannelThreshold = 0)
        {
            int expectedWidth = expectedBitmap.Width;
            int expectedHeight = expectedBitmap.Height;
            int actualWidth = actualBitmap.Width;
            int actualHeight = actualBitmap.Height;
            int width = Math.Max(expectedWidth, actualWidth);
            int height = Math.Max(expectedHeight, actualHeight);
            int total = width * height;
            int diffCount = 0;

            var expectedPixels = expectedBitmap.GetPixelSpan();
            var actualPixels = actualBitmap.GetPixelSpan();
            var expectedRgba = MemoryMarshal.Cast<byte, uint>(expectedPixels);
            var actualRgba = MemoryMarshal.Cast<byte, uint>(actualPixels);

            bool sameDimensions = expectedWidth == actualWidth && expectedHeight == actualHeight;

            if (sameDimensions && perChannelThreshold == 0)
            {
                // Fast path: same dimensions, exact match — just compare uint values
                for (int i = 0; i < total; i++)
                {
                    if (expectedRgba[i] != actualRgba[i])
                    {
                        diffCount++;
                    }
                }
            }
            else if (sameDimensions)
            {
                // Same dimensions with threshold
                for (int i = 0; i < total; i++)
                {
                    if (!PixelsMatchRaw(expectedRgba[i], actualRgba[i], perChannelThreshold))
                    {
                        diffCount++;
                    }
                }
            }
            else
            {
                // Different dimensions — row-by-row with bounds checks
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (x >= expectedWidth || y >= expectedHeight ||
                            x >= actualWidth || y >= actualHeight)
                        {
                            diffCount++;
                            continue;
                        }

                        uint expectedVal = expectedRgba[y * expectedWidth + x];
                        uint actualVal = actualRgba[y * actualWidth + x];

                        if (!PixelsMatchRaw(expectedVal, actualVal, perChannelThreshold))
                        {
                            diffCount++;
                        }
                    }
                }
            }

            double fraction = total > 0 ? (double)diffCount / total : 0.0;
            return (fraction, diffCount, total);
        }

        /// <summary>
        /// Compare two bitmaps with 1-pixel shift tolerance using direct pixel access.
        /// </summary>
        internal static (int StrictDiffPixels, int ShiftTolerantDiffPixels, int TotalPixels) CompareWithShiftTolerance(
            SKBitmap expectedBitmap, SKBitmap actualBitmap, int perChannelThreshold = 0)
        {
            int expectedWidth = expectedBitmap.Width;
            int expectedHeight = expectedBitmap.Height;
            int actualWidth = actualBitmap.Width;
            int actualHeight = actualBitmap.Height;
            int width = Math.Max(expectedWidth, actualWidth);
            int height = Math.Max(expectedHeight, actualHeight);
            int total = width * height;
            int strictDiff = 0;
            int shiftDiff = 0;

            var expectedPixels = MemoryMarshal.Cast<byte, uint>(expectedBitmap.GetPixelSpan());
            var actualPixels = MemoryMarshal.Cast<byte, uint>(actualBitmap.GetPixelSpan());

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x >= expectedWidth || y >= expectedHeight ||
                        x >= actualWidth || y >= actualHeight)
                    {
                        strictDiff++;
                        shiftDiff++;
                        continue;
                    }

                    uint expectedVal = expectedPixels[y * expectedWidth + x];
                    uint actualVal = actualPixels[y * actualWidth + x];

                    if (PixelsMatchRaw(expectedVal, actualVal, perChannelThreshold))
                    {
                        continue;
                    }

                    strictDiff++;

                    if (MatchesNeighborRaw(expectedPixels, expectedWidth, expectedHeight, x, y, actualVal, perChannelThreshold))
                    {
                        continue;
                    }

                    if (MatchesNeighborRaw(actualPixels, actualWidth, actualHeight, x, y, expectedVal, perChannelThreshold))
                    {
                        continue;
                    }

                    shiftDiff++;
                }
            }

            return (strictDiff, shiftDiff, total);
        }

        /// <summary>
        /// Check if pixel matches any pixel in a 3x3 neighborhood using raw pixel span.
        /// </summary>
        private static bool MatchesNeighborRaw(ReadOnlySpan<uint> pixels, int width, int height,
            int centerX, int centerY, uint targetPixel, int threshold)
        {
            int minY = Math.Max(0, centerY - 1);
            int maxY = Math.Min(height - 1, centerY + 1);
            int minX = Math.Max(0, centerX - 1);
            int maxX = Math.Min(width - 1, centerX + 1);

            for (int ny = minY; ny <= maxY; ny++)
            {
                int rowOffset = ny * width;
                for (int nx = minX; nx <= maxX; nx++)
                {
                    if (nx == centerX && ny == centerY)
                    {
                        continue;
                    }

                    if (PixelsMatchRaw(pixels[rowOffset + nx], targetPixel, threshold))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Compare two RGBA pixels packed as uint with per-channel threshold.
        /// Layout: byte 0=R, byte 1=G, byte 2=B, byte 3=A (RGBA8888).
        /// </summary>
        internal static bool PixelsMatchRaw(uint a, uint b, int threshold)
        {
            if (a == b)
            {
                return true;
            }
            if (threshold == 0)
            {
                return false;
            }

            int diffR = Math.Abs((int)(a & 0xFF) - (int)(b & 0xFF));
            int diffG = Math.Abs((int)((a >> 8) & 0xFF) - (int)((b >> 8) & 0xFF));
            int diffB = Math.Abs((int)((a >> 16) & 0xFF) - (int)((b >> 16) & 0xFF));
            int diffA = Math.Abs((int)((a >> 24) & 0xFF) - (int)((b >> 24) & 0xFF));

            return diffR <= threshold && diffG <= threshold &&
                   diffB <= threshold && diffA <= threshold;
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
