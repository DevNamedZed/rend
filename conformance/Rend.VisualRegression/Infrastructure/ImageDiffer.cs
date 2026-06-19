using System;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace Rend.VisualRegression.Infrastructure
{
    /// <summary>
    /// Single-pass image comparison and diff generation using direct pixel access.
    /// Combines strict comparison, shift-tolerant comparison, and diff PNG in one pass.
    /// </summary>
    public static class ImageDiffer
    {
        /// <summary>
        /// Compare and optionally generate diff in a single decode pass.
        /// Uses direct pixel spans instead of GetPixel() for ~100x speedup.
        /// </summary>
        public static CompareAndDiffResult CompareAndDiff(
            byte[] expectedPng, byte[] actualPng, int perChannelThreshold = 0)
        {
            using var expectedBitmap = SKBitmap.Decode(expectedPng);
            using var actualBitmap = SKBitmap.Decode(actualPng);

            if (expectedBitmap == null || actualBitmap == null)
            {
                return new CompareAndDiffResult(1.0, 1, 1.0, 1, 1, 255, null,
                    AntiAliasingClassifier.AaClassification.Real("decode failure"));
            }

            int expectedWidth = expectedBitmap.Width;
            int expectedHeight = expectedBitmap.Height;
            int actualWidth = actualBitmap.Width;
            int actualHeight = actualBitmap.Height;
            int width = Math.Max(expectedWidth, actualWidth);
            int height = Math.Max(expectedHeight, actualHeight);
            int totalPixels = width * height;

            var expectedPixels = MemoryMarshal.Cast<byte, uint>(expectedBitmap.GetPixelSpan());
            var actualPixels = MemoryMarshal.Cast<byte, uint>(actualBitmap.GetPixelSpan());

            bool sameDimensions = expectedWidth == actualWidth && expectedHeight == actualHeight;

            // Fast path: exact match check first (common for passing tests)
            if (sameDimensions)
            {
                bool allMatch = true;
                if (perChannelThreshold == 0)
                {
                    for (int i = 0; i < totalPixels; i++)
                    {
                        if (expectedPixels[i] != actualPixels[i])
                        {
                            allMatch = false;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < totalPixels; i++)
                    {
                        if (!ImageComparer.PixelsMatchRaw(expectedPixels[i], actualPixels[i], perChannelThreshold))
                        {
                            allMatch = false;
                            break;
                        }
                    }
                }

                if (allMatch)
                {
                    return new CompareAndDiffResult(0.0, 0, 0.0, 0, totalPixels, 0, null,
                        AntiAliasingClassifier.AaClassification.Real("no strict diff"));
                }
            }

            // Full comparison pass: strict count + max per-channel delta for fuzzy
            int strictDiff = 0;
            int maxChannelDiff = 0;

            if (sameDimensions)
            {
                for (int i = 0; i < totalPixels; i++)
                {
                    uint expectedVal = expectedPixels[i];
                    uint actualVal = actualPixels[i];
                    if (expectedVal == actualVal)
                    {
                        continue;
                    }

                    int channelDelta = MaxChannelDelta(expectedVal, actualVal);
                    if (channelDelta > maxChannelDiff)
                    {
                        maxChannelDiff = channelDelta;
                    }

                    if (channelDelta > perChannelThreshold)
                    {
                        strictDiff++;
                    }
                }
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (x >= expectedWidth || y >= expectedHeight ||
                            x >= actualWidth || y >= actualHeight)
                        {
                            strictDiff++;
                            maxChannelDiff = 255;
                            continue;
                        }

                        uint expectedVal = expectedPixels[y * expectedWidth + x];
                        uint actualVal = actualPixels[y * actualWidth + x];
                        if (expectedVal == actualVal)
                        {
                            continue;
                        }

                        int channelDelta = MaxChannelDelta(expectedVal, actualVal);
                        if (channelDelta > maxChannelDiff)
                        {
                            maxChannelDiff = channelDelta;
                        }

                        if (channelDelta > perChannelThreshold)
                        {
                            strictDiff++;
                        }
                    }
                }
            }

            double strictFraction = totalPixels > 0 ? (double)strictDiff / totalPixels : 0.0;

            // Shift-tolerant comparison (only if there are strict diffs)
            int shiftDiff = 0;
            if (strictDiff > 0)
            {
                var (_, sd, _) = ImageComparer.CompareWithShiftTolerance(
                    expectedBitmap, actualBitmap, perChannelThreshold);
                shiftDiff = sd;
            }

            double shiftFraction = totalPixels > 0 ? (double)shiftDiff / totalPixels : 0.0;

            // Anti-aliasing classification (post-analysis, while the pixel spans are still alive).
            // Pure label for a parallel report; it never affects pass/fail. Only meaningful when
            // there is a strict diff; same-dimension only (the classifier rejects mismatches).
            AntiAliasingClassifier.AaClassification aa = strictDiff > 0
                ? AntiAliasingClassifier.Classify(expectedPixels, actualPixels, width, height,
                    sameDimensions, strictDiff, maxChannelDiff, shiftFraction, perChannelThreshold)
                : AntiAliasingClassifier.AaClassification.Real("no strict diff");

            // Generate diff PNG only for failing tests
            byte[]? diffPng = null;
            if (strictDiff > 0)
            {
                diffPng = GenerateDiffDirect(expectedPixels, actualPixels,
                    expectedWidth, expectedHeight, actualWidth, actualHeight,
                    width, height, perChannelThreshold);
            }

            return new CompareAndDiffResult(strictFraction, strictDiff, shiftFraction, shiftDiff, totalPixels, maxChannelDiff, diffPng, aa);
        }

        /// <summary>
        /// Returns the largest absolute per-channel delta between two packed
        /// RGBA pixels. Layout: byte 0=R, byte 1=G, byte 2=B, byte 3=A.
        /// </summary>
        private static int MaxChannelDelta(uint a, uint b)
        {
            int deltaR = Math.Abs((int)(a & 0xFF) - (int)(b & 0xFF));
            int deltaG = Math.Abs((int)((a >> 8) & 0xFF) - (int)((b >> 8) & 0xFF));
            int deltaB = Math.Abs((int)((a >> 16) & 0xFF) - (int)((b >> 16) & 0xFF));
            int deltaA = Math.Abs((int)((a >> 24) & 0xFF) - (int)((b >> 24) & 0xFF));
            int maxRG = deltaR > deltaG ? deltaR : deltaG;
            int maxBA = deltaB > deltaA ? deltaB : deltaA;
            return maxRG > maxBA ? maxRG : maxBA;
        }

        /// <summary>
        /// Generate a diff PNG using direct pixel spans — no GetPixel/SetPixel calls.
        /// </summary>
        private static byte[] GenerateDiffDirect(
            ReadOnlySpan<uint> expectedPixels, ReadOnlySpan<uint> actualPixels,
            int expectedWidth, int expectedHeight, int actualWidth, int actualHeight,
            int width, int height, int perChannelThreshold)
        {
            using var diffBitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            var diffSpan = MemoryMarshal.Cast<byte, uint>(diffBitmap.GetPixelSpan());

            uint magenta = 0xFFFF00FF;  // RGBA: R=255, G=0, B=255, A=255
            uint red = 0xFF0000FF;       // RGBA: R=255, G=0, B=0, A=255
            byte dimAlpha = (byte)(255 * 0.3);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int diffIdx = y * width + x;

                    if (x >= expectedWidth || y >= expectedHeight ||
                        x >= actualWidth || y >= actualHeight)
                    {
                        diffSpan[diffIdx] = magenta;
                    }
                    else
                    {
                        uint expectedVal = expectedPixels[y * expectedWidth + x];
                        uint actualVal = actualPixels[y * actualWidth + x];

                        if (ImageComparer.PixelsMatchRaw(expectedVal, actualVal, perChannelThreshold))
                        {
                            // Grayscale at 30% opacity
                            byte r = (byte)(expectedVal & 0xFF);
                            byte g = (byte)((expectedVal >> 8) & 0xFF);
                            byte b = (byte)((expectedVal >> 16) & 0xFF);
                            byte gray = (byte)(r * 0.299 + g * 0.587 + b * 0.114);
                            // Premultiplied alpha
                            byte premGray = (byte)(gray * dimAlpha / 255);
                            diffSpan[diffIdx] = (uint)(premGray | (premGray << 8) | (premGray << 16) | (dimAlpha << 24));
                        }
                        else
                        {
                            diffSpan[diffIdx] = red;
                        }
                    }
                }
            }

            using var image = SKImage.FromBitmap(diffBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 80);
            return data.ToArray();
        }

        /// <summary>
        /// Generate a diff PNG from encoded PNGs.
        /// </summary>
        public static byte[] GenerateDiff(byte[] expectedPng, byte[] actualPng, int perChannelThreshold = 0)
        {
            using var expectedBitmap = SKBitmap.Decode(expectedPng);
            using var actualBitmap = SKBitmap.Decode(actualPng);

            if (expectedBitmap == null || actualBitmap == null)
            {
                return Array.Empty<byte>();
            }

            var expectedPixels = MemoryMarshal.Cast<byte, uint>(expectedBitmap.GetPixelSpan());
            var actualPixels = MemoryMarshal.Cast<byte, uint>(actualBitmap.GetPixelSpan());

            return GenerateDiffDirect(expectedPixels, actualPixels,
                expectedBitmap.Width, expectedBitmap.Height,
                actualBitmap.Width, actualBitmap.Height,
                Math.Max(expectedBitmap.Width, actualBitmap.Width),
                Math.Max(expectedBitmap.Height, actualBitmap.Height),
                perChannelThreshold);
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
        /// <summary>
        /// Largest absolute per-channel delta observed across all pixel
        /// comparisons (including those swallowed by a non-zero threshold).
        /// Used to evaluate WPT fuzzy tolerance directives.
        /// </summary>
        public readonly int MaxChannelDiff;
        public readonly byte[]? DiffPng;
        /// <summary>Anti-aliasing-only classification of this diff (label only; never affects pass/fail).</summary>
        public readonly AntiAliasingClassifier.AaClassification Aa;

        public CompareAndDiffResult(double strictFraction, int strictPixels,
            double shiftFraction, int shiftPixels, int totalPixels,
            int maxChannelDiff, byte[]? diffPng, AntiAliasingClassifier.AaClassification aa)
        {
            StrictDiffFraction = strictFraction;
            StrictDiffPixels = strictPixels;
            ShiftTolerantDiffFraction = shiftFraction;
            ShiftTolerantDiffPixels = shiftPixels;
            TotalPixels = totalPixels;
            MaxChannelDiff = maxChannelDiff;
            DiffPng = diffPng;
            Aa = aa;
        }
    }
}
