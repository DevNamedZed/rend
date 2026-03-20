#nullable enable
using System;
using SkiaSharp;
#if NET8_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;
#endif

namespace Rend.PdfRendering
{
    internal static class PixelConverter
    {
#if UNSAFE_SUPPORTED
        public static unsafe void GrayToRgbaRow(byte* dst, byte[] src, int srcOffset, int pixelCount)
        {
#if NET8_0_OR_GREATER
            if (Sse2.IsSupported && pixelCount >= 16)
            {
                GrayToRgbaRowSse2(dst, src, srcOffset, pixelCount);
                return;
            }
#endif
            for (int i = 0; i < pixelCount; i++)
            {
                int si = srcOffset + i;
                if (si >= src.Length) { break; }
                byte gray = src[si];
                int di = i * 4;
                dst[di] = gray;
                dst[di + 1] = gray;
                dst[di + 2] = gray;
                dst[di + 3] = 255;
            }
        }

        public static unsafe void RgbToRgbaRow(byte* dst, byte[] src, int srcOffset, int pixelCount)
        {
#if NET8_0_OR_GREATER
            if (Ssse3.IsSupported && pixelCount >= 4)
            {
                RgbToRgbaRowSsse3(dst, src, srcOffset, pixelCount);
                return;
            }
#endif
            for (int i = 0; i < pixelCount; i++)
            {
                int si = srcOffset + i * 3;
                if (si + 2 >= src.Length) { break; }
                int di = i * 4;
                dst[di] = src[si];
                dst[di + 1] = src[si + 1];
                dst[di + 2] = src[si + 2];
                dst[di + 3] = 255;
            }
        }

        public static unsafe void ApplyAlphaRow(byte* dst, byte[] mask, int maskOffset, int pixelCount)
        {
            for (int i = 0; i < pixelCount; i++)
            {
                int mi = maskOffset + i;
                dst[i * 4 + 3] = mi < mask.Length ? mask[mi] : (byte)255;
            }
        }
#endif

        // Safe fallbacks using SKBitmap.SetPixel — used when AllowUnsafeBlocks=false
        public static void GrayToRgbaRowSafe(SKBitmap bitmap, int y, byte[] src, int srcOffset, int pixelCount)
        {
            for (int i = 0; i < pixelCount; i++)
            {
                int si = srcOffset + i;
                if (si >= src.Length) { break; }
                byte gray = src[si];
                bitmap.SetPixel(i, y, new SKColor(gray, gray, gray));
            }
        }

        public static void RgbToRgbaRowSafe(SKBitmap bitmap, int y, byte[] src, int srcOffset, int pixelCount)
        {
            for (int i = 0; i < pixelCount; i++)
            {
                int si = srcOffset + i * 3;
                if (si + 2 >= src.Length) { break; }
                bitmap.SetPixel(i, y, new SKColor(src[si], src[si + 1], src[si + 2]));
            }
        }

        public static void ApplyAlphaRowSafe(SKBitmap bitmap, int y, byte[] mask, int maskOffset, int pixelCount)
        {
            for (int i = 0; i < pixelCount; i++)
            {
                int mi = maskOffset + i;
                byte alpha = mi < mask.Length ? mask[mi] : (byte)255;
                var pixel = bitmap.GetPixel(i, y);
                bitmap.SetPixel(i, y, new SKColor(pixel.Red, pixel.Green, pixel.Blue, alpha));
            }
        }

#if NET8_0_OR_GREATER && UNSAFE_SUPPORTED
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static unsafe void GrayToRgbaRowSse2(byte* dst, byte[] src, int srcOffset, int pixelCount)
        {
            fixed (byte* srcPtr = src)
            {
                byte* srcRow = srcPtr + srcOffset;
                int available = src.Length - srcOffset;
                int vectorCount = Math.Min(pixelCount, available) / 16 * 16;
                var alphaMask = Vector128.Create((byte)255);

                int i = 0;
                for (; i < vectorCount; i += 16)
                {
                    var gray16 = Sse2.LoadVector128(srcRow + i);
                    var grayPairLow = Sse2.UnpackLow(gray16, gray16);
                    var grayPairHigh = Sse2.UnpackHigh(gray16, gray16);
                    var rgba0 = Sse2.UnpackLow(grayPairLow, alphaMask);
                    var rgba1 = Sse2.UnpackHigh(grayPairLow, alphaMask);
                    var rgba2 = Sse2.UnpackLow(grayPairHigh, alphaMask);
                    var rgba3 = Sse2.UnpackHigh(grayPairHigh, alphaMask);
                    Sse2.Store(dst + i * 4, rgba0);
                    Sse2.Store(dst + i * 4 + 16, rgba1);
                    Sse2.Store(dst + i * 4 + 32, rgba2);
                    Sse2.Store(dst + i * 4 + 48, rgba3);
                }

                for (; i < pixelCount && (srcOffset + i) < src.Length; i++)
                {
                    byte gray = srcRow[i];
                    int di = i * 4;
                    dst[di] = gray;
                    dst[di + 1] = gray;
                    dst[di + 2] = gray;
                    dst[di + 3] = 255;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static unsafe void RgbToRgbaRowSsse3(byte* dst, byte[] src, int srcOffset, int pixelCount)
        {
            fixed (byte* srcPtr = src)
            {
                byte* srcRow = srcPtr + srcOffset;
                var shuffleMask = Vector128.Create(
                    (byte)0, 1, 2, 0x80,
                    3, 4, 5, 0x80,
                    6, 7, 8, 0x80,
                    9, 10, 11, 0x80);
                var alphaMask = Vector128.Create(
                    (byte)0, 0, 0, 255,
                    0, 0, 0, 255,
                    0, 0, 0, 255,
                    0, 0, 0, 255);

                int i = 0;
                int maxVec = pixelCount - 3;
                for (; i < maxVec; i += 4)
                {
                    if (srcOffset + i * 3 + 15 >= src.Length) { break; }
                    var rgb = Sse2.LoadVector128(srcRow + i * 3);
                    var rgba = Ssse3.Shuffle(rgb, shuffleMask);
                    rgba = Sse2.Or(rgba, alphaMask);
                    Sse2.Store(dst + i * 4, rgba);
                }

                for (; i < pixelCount; i++)
                {
                    int si = srcOffset + i * 3;
                    if (si + 2 >= src.Length) { break; }
                    int di = i * 4;
                    dst[di] = src[si];
                    dst[di + 1] = src[si + 1];
                    dst[di + 2] = src[si + 2];
                    dst[di + 3] = 255;
                }
            }
        }
#endif
    }
}
