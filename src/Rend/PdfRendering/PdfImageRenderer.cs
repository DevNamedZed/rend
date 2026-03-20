#nullable enable
using System;
using System.IO;
using System.IO.Compression;
using Rend.Pdf.Parsing;
using SkiaSharp;

namespace Rend.PdfRendering
{
    internal static class PdfImageRenderer
    {
        public static void DrawInlineImage(SKCanvas canvas, GraphicsState state, InlineImageData img)
        {
            if (img.Width <= 0 || img.Height <= 0 || img.Data.Length == 0)
            {
                return;
            }

            byte[] pixelData = img.Data;

            if (img.Filter == "FlateDecode")
            {
                try
                {
                    using var input = new MemoryStream(pixelData);
                    input.ReadByte();
                    input.ReadByte();
                    using var deflate = new DeflateStream(input, CompressionMode.Decompress);
                    using var output = new MemoryStream();
                    deflate.CopyTo(output);
                    pixelData = output.ToArray();
                }
                catch
                {
                    return;
                }
            }
            else if (img.Filter == "DCTDecode")
            {
                using var bitmap = SKBitmap.Decode(pixelData);
                if (bitmap == null)
                {
                    return;
                }
                DrawBitmapInUnitSquare(canvas, state, bitmap);
                return;
            }

            SKBitmap? decodedBitmap = null;
            if (img.ColorSpace == "DeviceGray")
            {
                decodedBitmap = CreateGrayBitmap(pixelData, img.Width, img.Height, img.BitsPerComponent, false);
            }
            else if (img.ColorSpace == "DeviceCMYK")
            {
                decodedBitmap = CreateCmykBitmap(pixelData, img.Width, img.Height);
            }
            else
            {
                decodedBitmap = CreateRgbBitmap(pixelData, img.Width, img.Height, img.BitsPerComponent);
            }

            if (decodedBitmap == null)
            {
                return;
            }
            try
            {
                DrawBitmapInUnitSquare(canvas, state, decodedBitmap);
            }
            finally
            {
                decodedBitmap.Dispose();
            }
        }

        public static void DrawImageXObject(SKCanvas canvas, GraphicsState state, PdfObj imageDict, PdfDocumentReader reader)
        {
            int width = (int)reader.Resolve(imageDict["Width"]).AsInt();
            int height = (int)reader.Resolve(imageDict["Height"]).AsInt();
            if (width <= 0 || height <= 0)
            {
                return;
            }

            byte[] imageData = reader.GetStreamBytes(imageDict);
            if (imageData == null || imageData.Length == 0)
            {
                return;
            }

            // [SPEC §8.9.6.2] ImageMask: 1-bit stencil painted with current fill color
            var imageMaskObj = reader.Resolve(imageDict["ImageMask"]);
            bool isImageMask = imageMaskObj.AsBool() || imageMaskObj.AsName() == "true" || imageMaskObj.AsInt() == 1;

            if (!isImageMask)
            {
                var bpcObj = reader.Resolve(imageDict["BitsPerComponent"]);
                var csObj = reader.Resolve(imageDict["ColorSpace"]);
                if (bpcObj.AsInt() == 1 && csObj.IsNull)
                {
                    isImageMask = true;
                }
            }

            if (isImageMask)
            {
                DrawImageMask(canvas, state, imageData, width, height, imageDict, reader);
                return;
            }

            SKBitmap? bitmap = null;

            string filter = reader.Resolve(imageDict["Filter"]).AsName();

            if (filter.Contains("DCTDecode") || filter.Contains("DCT"))
            {
                bitmap = SKBitmap.Decode(imageData);
            }
            else if (filter.Contains("JPXDecode") || filter.Contains("JPX"))
            {
                bitmap = SKBitmap.Decode(imageData);
            }
            else
            {
                string colorSpace = ResolveColorSpaceName(reader, imageDict);
                int bitsPerComponent = (int)reader.Resolve(imageDict["BitsPerComponent"]).AsInt();
                if (bitsPerComponent <= 0)
                {
                    bitsPerComponent = 8;
                }

                bitmap = CreateBitmapFromRawPixels(reader, imageData, width, height, colorSpace, bitsPerComponent, imageDict);
            }

            if (bitmap == null)
            {
                return;
            }

            // [SPEC §11.6.5.3] Apply SMask (soft mask) as alpha channel
            var smaskObj = reader.Resolve(imageDict["SMask"]);
            if (!smaskObj.IsNull && smaskObj.IsStream)
            {
                ApplySoftMask(bitmap, smaskObj, reader);
            }

            try
            {
                DrawBitmapInUnitSquare(canvas, state, bitmap);
            }
            finally
            {
                bitmap.Dispose();
            }
        }

        private static void DrawBitmapInUnitSquare(SKCanvas canvas, GraphicsState state, SKBitmap bitmap)
        {
            canvas.Save();
            var imageMatrix = new SKMatrix(
                1f / bitmap.Width, 0, 0,
                0, -1f / bitmap.Height, 1,
                0, 0, 1);
            canvas.Concat(imageMatrix);

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(255, 255, 255, PdfColorHelper.ClampByte(state.FillAlpha * 255f)),
            };
            canvas.DrawBitmap(bitmap, 0, 0, paint);
            canvas.Restore();
        }

        // ─── SMask ────────────────────────────────────────────────────────

        private static void ApplySoftMask(SKBitmap bitmap, PdfObj smaskDict, PdfDocumentReader reader)
        {
            int maskWidth = (int)reader.Resolve(smaskDict["Width"]).AsInt();
            int maskHeight = (int)reader.Resolve(smaskDict["Height"]).AsInt();
            byte[] maskData = reader.GetStreamBytes(smaskDict);

            if (maskData == null || maskData.Length == 0 || maskWidth <= 0 || maskHeight <= 0)
            {
                return;
            }

            int maskBpc = (int)reader.Resolve(smaskDict["BitsPerComponent"]).AsInt();
            if (maskBpc <= 0)
            {
                maskBpc = 8;
            }

            int bitmapWidth = bitmap.Width;
            int bitmapHeight = bitmap.Height;

#if UNSAFE_SUPPORTED
            IntPtr pixelsPtr = bitmap.GetPixels();
            unsafe
            {
                byte* pixels = (byte*)pixelsPtr;
                if (maskBpc == 8 && maskWidth == bitmapWidth && maskHeight == bitmapHeight)
                {
                    for (int y = 0; y < bitmapHeight; y++)
                    {
                        byte* row = pixels + y * bitmap.RowBytes;
                        int maskRowOffset = y * maskWidth;
                        PixelConverter.ApplyAlphaRow(row, maskData, maskRowOffset, bitmapWidth);
                    }
                }
                else
                {
                    for (int y = 0; y < bitmapHeight; y++)
                    {
                        int maskY = y * maskHeight / bitmapHeight;
                        byte* row = pixels + y * bitmap.RowBytes;
                        for (int x = 0; x < bitmapWidth; x++)
                        {
                            int maskX = x * maskWidth / bitmapWidth;
                            byte alpha = GetMaskAlpha(maskData, maskWidth, maskX, maskY, maskBpc);
                            row[x * 4 + 3] = alpha;
                        }
                    }
                }
            }
#else
            for (int y = 0; y < bitmapHeight; y++)
            {
                int maskY = y * maskHeight / bitmapHeight;
                for (int x = 0; x < bitmapWidth; x++)
                {
                    int maskX = x * maskWidth / bitmapWidth;
                    byte alpha = GetMaskAlpha(maskData, maskWidth, maskX, maskY, maskBpc);
                    var pixel = bitmap.GetPixel(x, y);
                    bitmap.SetPixel(x, y, new SKColor(pixel.Red, pixel.Green, pixel.Blue, alpha));
                }
            }
#endif
        }

        private static byte GetMaskAlpha(byte[] maskData, int maskWidth, int maskX, int maskY, int maskBpc)
        {
            if (maskBpc == 8)
            {
                int offset = maskY * maskWidth + maskX;
                return offset < maskData.Length ? maskData[offset] : (byte)255;
            }
            if (maskBpc == 1)
            {
                int stride = (maskWidth + 7) / 8;
                int byteOffset = maskY * stride + maskX / 8;
                if (byteOffset < maskData.Length)
                {
                    int bit = (maskData[byteOffset] >> (7 - (maskX % 8))) & 1;
                    return bit == 1 ? (byte)255 : (byte)0;
                }
                return 255;
            }
            return 255;
        }

        // ─── ImageMask ────────────────────────────────────────────────────

        private static void DrawImageMask(SKCanvas canvas, GraphicsState state,
            byte[] data, int width, int height, PdfObj imageDict, PdfDocumentReader reader)
        {
            var decodeObj = reader.Resolve(imageDict["Decode"]);
            bool invertMask = false;
            if (!decodeObj.IsNull && decodeObj.IsArray && decodeObj.Count >= 2)
            {
                if (decodeObj[0].AsFloat() > 0.5f)
                {
                    invertMask = true;
                }
            }

            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            byte fillR = state.FillColor.Red;
            byte fillG = state.FillColor.Green;
            byte fillB = state.FillColor.Blue;
            int stride = (width + 7) / 8;

#if UNSAFE_SUPPORTED
            IntPtr pixelsPtr = bitmap.GetPixels();
            unsafe
            {
                byte* pixels = (byte*)pixelsPtr;
                int rowBytes = bitmap.RowBytes;
                for (int y = 0; y < height; y++)
                {
                    byte* row = pixels + y * rowBytes;
                    for (int x = 0; x < width; x++)
                    {
                        int byteOffset = y * stride + x / 8;
                        if (byteOffset >= data.Length) { break; }
                        int bit = (data[byteOffset] >> (7 - (x % 8))) & 1;
                        bool painted = invertMask ? (bit == 1) : (bit == 0);
                        int pixelOffset = x * 4;
                        if (painted)
                        {
                            row[pixelOffset] = fillR;
                            row[pixelOffset + 1] = fillG;
                            row[pixelOffset + 2] = fillB;
                            row[pixelOffset + 3] = 255;
                        }
                        else
                        {
                            row[pixelOffset] = 0;
                            row[pixelOffset + 1] = 0;
                            row[pixelOffset + 2] = 0;
                            row[pixelOffset + 3] = 0;
                        }
                    }
                }
            }
#else
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int byteOffset = y * stride + x / 8;
                    if (byteOffset >= data.Length) { break; }
                    int bit = (data[byteOffset] >> (7 - (x % 8))) & 1;
                    bool painted = invertMask ? (bit == 1) : (bit == 0);
                    bitmap.SetPixel(x, y, painted
                        ? new SKColor(fillR, fillG, fillB)
                        : SKColors.Transparent);
                }
            }
#endif

            try
            {
                DrawBitmapInUnitSquare(canvas, state, bitmap);
            }
            finally
            {
                bitmap.Dispose();
            }
        }

        // ─── Color Space Resolution ───────────────────────────────────────

        private static string ResolveColorSpaceName(PdfDocumentReader reader, PdfObj imageDict)
        {
            var colorSpace = reader.Resolve(imageDict["ColorSpace"]);
            if (colorSpace.IsNull)
            {
                return "DeviceRGB";
            }

            string name = colorSpace.AsName();
            if (!string.IsNullOrEmpty(name))
            {
                if (name.StartsWith("/"))
                {
                    name = name.Substring(1);
                }
                return name;
            }

            if (colorSpace.IsArray && colorSpace.Count > 0)
            {
                string arrayName = reader.Resolve(colorSpace[0]).AsName();
                if (arrayName.StartsWith("/"))
                {
                    arrayName = arrayName.Substring(1);
                }

                if (arrayName == "ICCBased" && colorSpace.Count > 1)
                {
                    var iccStream = reader.Resolve(colorSpace[1]);
                    int components = (int)reader.Resolve(iccStream["N"]).AsInt();
                    if (components == 1) { return "DeviceGray"; }
                    if (components == 4) { return "DeviceCMYK"; }
                    return "DeviceRGB";
                }

                if (arrayName == "Indexed") { return "Indexed"; }
                if (arrayName == "CalGray") { return "DeviceGray"; }
                if (arrayName == "CalRGB") { return "DeviceRGB"; }

                return arrayName;
            }

            return "DeviceRGB";
        }

        // ─── Bitmap Creation ──────────────────────────────────────────────

        private static SKBitmap? CreateBitmapFromRawPixels(PdfDocumentReader reader, byte[] data, int width, int height,
            string colorSpace, int bitsPerComponent, PdfObj imageDict)
        {
            bool invertGray = false;
            var decodeObj = reader.Resolve(imageDict["Decode"]);
            if (!decodeObj.IsNull && decodeObj.IsArray && decodeObj.Count >= 2)
            {
                if (decodeObj[0].AsFloat() > decodeObj[1].AsFloat())
                {
                    invertGray = true;
                }
            }

            if (colorSpace == "DeviceGray" || colorSpace == "CalGray")
            {
                return CreateGrayBitmap(data, width, height, bitsPerComponent, invertGray);
            }
            if (colorSpace == "DeviceCMYK")
            {
                return CreateCmykBitmap(data, width, height);
            }
            if (colorSpace == "Indexed")
            {
                return CreateIndexedBitmap(reader, data, width, height, bitsPerComponent, imageDict);
            }

            return CreateRgbBitmap(data, width, height, bitsPerComponent);
        }

        internal static SKBitmap CreateRgbBitmap(byte[] data, int width, int height, int bitsPerComponent)
        {
            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

#if UNSAFE_SUPPORTED
            IntPtr pixelsPtr = bitmap.GetPixels();
            if (bitsPerComponent == 16)
            {
                int bytesPerPixel = 6;
                int stride = (width * 3 * bitsPerComponent + 7) / 8;
                unsafe
                {
                    byte* pixels = (byte*)pixelsPtr;
                    int rowBytes = bitmap.RowBytes;
                    for (int y = 0; y < height; y++)
                    {
                        byte* row = pixels + y * rowBytes;
                        for (int x = 0; x < width; x++)
                        {
                            int srcOffset = y * stride + x * bytesPerPixel;
                            if (srcOffset + 5 > data.Length - 1)
                            {
                                break;
                            }
                            int dstOffset = x * 4;
                            row[dstOffset] = data[srcOffset];
                            row[dstOffset + 1] = data[srcOffset + 2];
                            row[dstOffset + 2] = data[srcOffset + 4];
                            row[dstOffset + 3] = 255;
                        }
                    }
                }
            }
            else
            {
                int stride = width * 3;
                unsafe
                {
                    byte* pixels = (byte*)pixelsPtr;
                    int rowBytes = bitmap.RowBytes;
                    for (int y = 0; y < height; y++)
                    {
                        int srcRowOffset = y * stride;
                        int rowPixels = Math.Min(width, (data.Length - srcRowOffset) / 3);
                        if (rowPixels <= 0)
                        {
                            break;
                        }
                        byte* row = pixels + y * rowBytes;
                        PixelConverter.RgbToRgbaRow(row, data, srcRowOffset, rowPixels);
                    }
                }
            }
#else
            int stride = bitsPerComponent == 16 ? (width * 6) : (width * 3);
            int bpp = bitsPerComponent == 16 ? 6 : 3;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + x * bpp;
                    if (offset + bpp - 1 >= data.Length) { break; }
                    byte r = data[offset];
                    byte g = data[offset + (bitsPerComponent == 16 ? 2 : 1)];
                    byte b = data[offset + (bitsPerComponent == 16 ? 4 : 2)];
                    bitmap.SetPixel(x, y, new SKColor(r, g, b));
                }
            }
#endif
            return bitmap;
        }

        internal static SKBitmap CreateGrayBitmap(byte[] data, int width, int height, int bitsPerComponent, bool invert = false)
        {
            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

#if UNSAFE_SUPPORTED
            IntPtr pixelsPtr = bitmap.GetPixels();
            unsafe
            {
                byte* pixels = (byte*)pixelsPtr;
                int rowBytes = bitmap.RowBytes;

                if (bitsPerComponent == 8 && !invert)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int srcRowOffset = y * width;
                        int rowPixels = Math.Min(width, data.Length - srcRowOffset);
                        if (rowPixels <= 0) { break; }
                        byte* row = pixels + y * rowBytes;
                        PixelConverter.GrayToRgbaRow(row, data, srcRowOffset, rowPixels);
                    }
                }
                else
                {
                    for (int y = 0; y < height; y++)
                    {
                        byte* row = pixels + y * rowBytes;
                        for (int x = 0; x < width; x++)
                        {
                            int gray8 = GetGraySample(data, width, x, y, bitsPerComponent);
                            if (gray8 < 0) { break; }
                            byte gray = invert ? (byte)(255 - gray8) : (byte)gray8;
                            int dstOffset = x * 4;
                            row[dstOffset] = gray;
                            row[dstOffset + 1] = gray;
                            row[dstOffset + 2] = gray;
                            row[dstOffset + 3] = 255;
                        }
                    }
                }
            }
#else
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int gray8 = GetGraySample(data, width, x, y, bitsPerComponent);
                    if (gray8 < 0) { break; }
                    byte gray = invert ? (byte)(255 - gray8) : (byte)gray8;
                    bitmap.SetPixel(x, y, new SKColor(gray, gray, gray));
                }
            }
#endif
            return bitmap;
        }

        private static int GetGraySample(byte[] data, int width, int x, int y, int bpc)
        {
            if (bpc == 8)
            {
                int offset = y * width + x;
                return offset < data.Length ? data[offset] : -1;
            }
            if (bpc == 1)
            {
                int stride = (width + 7) / 8;
                int byteOffset = y * stride + x / 8;
                if (byteOffset >= data.Length) { return -1; }
                int bit = (data[byteOffset] >> (7 - (x % 8))) & 1;
                return bit == 1 ? 255 : 0;
            }
            if (bpc == 4)
            {
                int stride = (width + 1) / 2;
                int byteOffset = y * stride + x / 2;
                if (byteOffset >= data.Length) { return -1; }
                int nibble = (x % 2 == 0) ? (data[byteOffset] >> 4) & 0xF : data[byteOffset] & 0xF;
                return nibble * 17;
            }
            return -1;
        }

        internal static SKBitmap CreateCmykBitmap(byte[] data, int width, int height)
        {
            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

#if UNSAFE_SUPPORTED
            IntPtr pixelsPtr = bitmap.GetPixels();
            unsafe
            {
                byte* pixels = (byte*)pixelsPtr;
                int rowBytes = bitmap.RowBytes;
                for (int y = 0; y < height; y++)
                {
                    byte* row = pixels + y * rowBytes;
                    for (int x = 0; x < width; x++)
                    {
                        int srcOffset = (y * width + x) * 4;
                        if (srcOffset + 3 > data.Length - 1) { break; }
                        float cyan = data[srcOffset] / 255f;
                        float magenta = data[srcOffset + 1] / 255f;
                        float yellow = data[srcOffset + 2] / 255f;
                        float key = data[srcOffset + 3] / 255f;
                        int dstOffset = x * 4;
                        row[dstOffset] = PdfColorHelper.ClampByte((1 - cyan) * (1 - key) * 255f);
                        row[dstOffset + 1] = PdfColorHelper.ClampByte((1 - magenta) * (1 - key) * 255f);
                        row[dstOffset + 2] = PdfColorHelper.ClampByte((1 - yellow) * (1 - key) * 255f);
                        row[dstOffset + 3] = 255;
                    }
                }
            }
#else
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcOffset = (y * width + x) * 4;
                    if (srcOffset + 3 > data.Length - 1) { break; }
                    float cyan = data[srcOffset] / 255f;
                    float magenta = data[srcOffset + 1] / 255f;
                    float yellow = data[srcOffset + 2] / 255f;
                    float key = data[srcOffset + 3] / 255f;
                    bitmap.SetPixel(x, y, PdfColorHelper.CmykToColor(cyan, magenta, yellow, key));
                }
            }
#endif
            return bitmap;
        }

        private static SKBitmap? CreateIndexedBitmap(PdfDocumentReader reader, byte[] data, int width, int height,
            int bitsPerComponent, PdfObj imageDict)
        {
            var colorSpace = reader.Resolve(imageDict["ColorSpace"]);
            if (!colorSpace.IsArray || colorSpace.Count < 4)
            {
                return null;
            }

            int maxIndex = (int)reader.Resolve(colorSpace[2]).AsInt();
            var lookupObj = reader.Resolve(colorSpace[3]);

            byte[] palette;
            if (lookupObj.IsStream)
            {
                palette = reader.GetStreamBytes(lookupObj) ?? Array.Empty<byte>();
            }
            else
            {
                palette = lookupObj.AsBytes();
            }

            if (palette.Length == 0)
            {
                return null;
            }

            string baseColorSpace = reader.Resolve(colorSpace[1]).AsName();
            if (baseColorSpace.StartsWith("/"))
            {
                baseColorSpace = baseColorSpace.Substring(1);
            }
            int componentsPerColor = baseColorSpace == "DeviceCMYK" ? 4 : (baseColorSpace == "DeviceGray" || baseColorSpace == "CalGray" ? 1 : 3);

            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

#if UNSAFE_SUPPORTED
            IntPtr pixelsPtr = bitmap.GetPixels();
            unsafe
            {
                byte* pixels = (byte*)pixelsPtr;
                int rowBytes = bitmap.RowBytes;
                for (int y = 0; y < height; y++)
                {
                    byte* row = pixels + y * rowBytes;
                    for (int x = 0; x < width; x++)
                    {
                        int index = GetSampleValue(data, width, x, y, bitsPerComponent);
                        if (index < 0) { break; }
                        GetPaletteColorBytes(palette, index, componentsPerColor,
                            out byte red, out byte green, out byte blue);
                        int dstOffset = x * 4;
                        row[dstOffset] = red;
                        row[dstOffset + 1] = green;
                        row[dstOffset + 2] = blue;
                        row[dstOffset + 3] = 255;
                    }
                }
            }
#else
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = GetSampleValue(data, width, x, y, bitsPerComponent);
                    if (index < 0) { break; }
                    GetPaletteColorBytes(palette, index, componentsPerColor,
                        out byte red, out byte green, out byte blue);
                    bitmap.SetPixel(x, y, new SKColor(red, green, blue));
                }
            }
#endif
            return bitmap;
        }

        private static int GetSampleValue(byte[] data, int width, int x, int y, int bitsPerComponent)
        {
            if (bitsPerComponent == 8)
            {
                int offset = y * width + x;
                return offset < data.Length ? data[offset] : -1;
            }
            if (bitsPerComponent == 4)
            {
                int stride = (width + 1) / 2;
                int byteOffset = y * stride + x / 2;
                if (byteOffset >= data.Length)
                {
                    return -1;
                }
                return (x % 2 == 0)
                    ? (data[byteOffset] >> 4) & 0xF
                    : data[byteOffset] & 0xF;
            }
            if (bitsPerComponent == 1)
            {
                int stride = (width + 7) / 8;
                int byteOffset = y * stride + x / 8;
                if (byteOffset >= data.Length)
                {
                    return -1;
                }
                return (data[byteOffset] >> (7 - (x % 8))) & 1;
            }
            return -1;
        }

        private static void GetPaletteColorBytes(byte[] palette, int index, int components,
            out byte red, out byte green, out byte blue)
        {
            int offset = index * components;
            if (components == 1)
            {
                byte gray = offset < palette.Length ? palette[offset] : (byte)0;
                red = green = blue = gray;
                return;
            }
            if (components == 4 && offset + 3 < palette.Length)
            {
                float cyan = palette[offset] / 255f;
                float magenta = palette[offset + 1] / 255f;
                float yellow = palette[offset + 2] / 255f;
                float key = palette[offset + 3] / 255f;
                red = PdfColorHelper.ClampByte((1 - cyan) * (1 - key) * 255f);
                green = PdfColorHelper.ClampByte((1 - magenta) * (1 - key) * 255f);
                blue = PdfColorHelper.ClampByte((1 - yellow) * (1 - key) * 255f);
                return;
            }
            if (offset + 2 < palette.Length)
            {
                red = palette[offset];
                green = palette[offset + 1];
                blue = palette[offset + 2];
                return;
            }
            red = green = blue = 0;
        }
    }
}
