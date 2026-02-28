using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Rend.Pdf.Internal;

namespace Rend.Pdf.Images
{
    /// <summary>
    /// PNG image handler. Decodes PNG, separates alpha channel into an SMask,
    /// handles indexed color, sub-byte depths, 16-bit channels,
    /// and re-compresses pixel data with FlateDecode for PDF embedding.
    /// </summary>
    internal static class PngHandler
    {
        public static PdfImage CreateImage(byte[] pngData, string resourceName,
                                            PdfObjectTable objectTable, bool compress)
        {
            ParsePng(pngData, out var info, out byte[] rawPixels);

            // Indexed color (colorType 3) — use /Indexed color space directly
            if (info.ColorType == 3)
                return CreateIndexedImage(info, rawPixels, resourceName, objectTable, compress);

            // For 16-bit images, downsample to 8-bit for PDF compatibility
            int outputBitDepth = info.BitDepth;
            if (info.BitDepth == 16)
            {
                rawPixels = Downsample16To8(rawPixels, info.Width, info.Height, GetChannelCount(info.ColorType));
                outputBitDepth = 8;
            }

            // For sub-byte depths (1, 2, 4), unpack to 8-bit
            if (info.BitDepth < 8 && info.ColorType == 0)
            {
                rawPixels = UnpackSubByte(rawPixels, info.Width, info.Height, info.BitDepth);
                outputBitDepth = 8;
            }

            int components = GetChannelCount(info.ColorType);
            bool hasAlpha = info.ColorType == 4 || info.ColorType == 6;

            byte[] colorData;
            byte[]? alphaData;

            if (hasAlpha)
            {
                SeparateAlpha(rawPixels, info.Width, info.Height, components, out colorData, out alphaData);
                components = info.ColorType == 6 ? 3 : 1;
            }
            else if (info.TrnsData != null && !hasAlpha)
            {
                // tRNS chunk: create alpha mask from transparency color
                CreateTrnsAlpha(rawPixels, info, out colorData, out alphaData);
            }
            else
            {
                colorData = rawPixels;
                alphaData = null;
            }

            PdfName colorSpace;
            switch (components)
            {
                case 1: colorSpace = PdfName.DeviceGray; break;
                default: colorSpace = PdfName.DeviceRGB; break;
            }

            var imageStream = new PdfStream(colorData, compress);
            imageStream.Dict[PdfName.Type] = PdfName.XObject;
            imageStream.Dict[PdfName.Subtype] = PdfName.Image;
            imageStream.Dict[PdfName.Width] = new PdfInteger(info.Width);
            imageStream.Dict[PdfName.Height] = new PdfInteger(info.Height);
            imageStream.Dict[PdfName.BitsPerComponent] = new PdfInteger(outputBitDepth);
            imageStream.Dict[PdfName.ColorSpace] = colorSpace;

            PdfReference? smaskRef = null;
            if (alphaData != null)
            {
                var smaskStream = new PdfStream(alphaData, compress);
                smaskStream.Dict[PdfName.Type] = PdfName.XObject;
                smaskStream.Dict[PdfName.Subtype] = PdfName.Image;
                smaskStream.Dict[PdfName.Width] = new PdfInteger(info.Width);
                smaskStream.Dict[PdfName.Height] = new PdfInteger(info.Height);
                smaskStream.Dict[PdfName.BitsPerComponent] = new PdfInteger(8);
                smaskStream.Dict[PdfName.ColorSpace] = PdfName.DeviceGray;

                smaskRef = objectTable.Allocate(smaskStream);
                imageStream.Dict[PdfName.SMask] = smaskRef;
            }

            var imageRef = objectTable.Allocate(imageStream);
            return new PdfImage(info.Width, info.Height, outputBitDepth,
                                alphaData != null, ImageFormat.Png, resourceName, imageRef, smaskRef);
        }

        private static PdfImage CreateIndexedImage(PngInfo info, byte[] rawPixels,
            string resourceName, PdfObjectTable objectTable, bool compress)
        {
            if (info.Palette == null)
                throw new InvalidOperationException("Indexed PNG missing PLTE chunk.");

            // For sub-byte indexed images, unpack palette indices to 8-bit
            int outputBitDepth = info.BitDepth;
            if (info.BitDepth < 8)
            {
                rawPixels = UnpackSubByteIndexed(rawPixels, info.Width, info.Height, info.BitDepth);
                outputBitDepth = 8;
            }

            int maxIndex = info.Palette.Length / 3 - 1;

            // Build /Indexed color space: [/Indexed /DeviceRGB maxIndex paletteHexString]
            var indexedCs = new PdfArray(4);
            indexedCs.Add(new PdfName("Indexed"));
            indexedCs.Add(PdfName.DeviceRGB);
            indexedCs.Add(new PdfInteger(maxIndex));
            indexedCs.Add(new PdfHexString(info.Palette));

            // Check for tRNS alpha on indexed images
            byte[]? alphaData = null;
            if (info.TrnsData != null && info.TrnsData.Length > 0)
            {
                // tRNS for indexed: each byte is the alpha for that palette index
                int pixelCount = info.Width * info.Height;
                alphaData = new byte[pixelCount];
                for (int i = 0; i < pixelCount; i++)
                {
                    int idx = rawPixels[i] & 0xFF;
                    alphaData[i] = idx < info.TrnsData.Length ? info.TrnsData[idx] : (byte)255;
                }
            }

            var imageStream = new PdfStream(rawPixels, compress);
            imageStream.Dict[PdfName.Type] = PdfName.XObject;
            imageStream.Dict[PdfName.Subtype] = PdfName.Image;
            imageStream.Dict[PdfName.Width] = new PdfInteger(info.Width);
            imageStream.Dict[PdfName.Height] = new PdfInteger(info.Height);
            imageStream.Dict[PdfName.BitsPerComponent] = new PdfInteger(outputBitDepth);
            imageStream.Dict[PdfName.ColorSpace] = indexedCs;

            PdfReference? smaskRef = null;
            if (alphaData != null)
            {
                var smaskStream = new PdfStream(alphaData, compress);
                smaskStream.Dict[PdfName.Type] = PdfName.XObject;
                smaskStream.Dict[PdfName.Subtype] = PdfName.Image;
                smaskStream.Dict[PdfName.Width] = new PdfInteger(info.Width);
                smaskStream.Dict[PdfName.Height] = new PdfInteger(info.Height);
                smaskStream.Dict[PdfName.BitsPerComponent] = new PdfInteger(8);
                smaskStream.Dict[PdfName.ColorSpace] = PdfName.DeviceGray;

                smaskRef = objectTable.Allocate(smaskStream);
                imageStream.Dict[PdfName.SMask] = smaskRef;
            }

            var imageRef = objectTable.Allocate(imageStream);
            return new PdfImage(info.Width, info.Height, outputBitDepth,
                                alphaData != null, ImageFormat.Png, resourceName, imageRef, smaskRef);
        }

        private struct PngInfo
        {
            public int Width;
            public int Height;
            public int BitDepth;
            public int ColorType;
            public byte[]? Palette;  // PLTE chunk: RGB triplets
            public byte[]? TrnsData; // tRNS chunk: transparency data
        }

        private static void ParsePng(byte[] data, out PngInfo info, out byte[] rawPixels)
        {
            // Validate PNG signature
            if (data.Length < 8 || data[0] != 137 || data[1] != 80 || data[2] != 78 || data[3] != 71)
                throw new InvalidOperationException("Not a valid PNG file.");

            int pos = 8;

            // Read IHDR
            uint ihdrLength = ReadUInt32BE(data, pos); pos += 4;
            string ihdrTag = System.Text.Encoding.ASCII.GetString(data, pos, 4); pos += 4;
            if (ihdrTag != "IHDR")
                throw new InvalidOperationException("Expected IHDR chunk.");

            info.Width = (int)ReadUInt32BE(data, pos); pos += 4;
            info.Height = (int)ReadUInt32BE(data, pos); pos += 4;
            info.BitDepth = data[pos++];
            info.ColorType = data[pos++];
            pos++; // compressionMethod
            pos++; // filterMethod
            int interlaceMethod = data[pos++];
            pos += 4; // CRC

            if (interlaceMethod != 0)
                throw new NotSupportedException("Interlaced PNGs are not yet supported.");

            info.Palette = null;
            info.TrnsData = null;

            var compressedData = new List<byte[]>();
            int totalCompressedLength = 0;

            while (pos + 8 <= data.Length)
            {
                uint chunkLength = ReadUInt32BE(data, pos); pos += 4;
                string chunkTag = System.Text.Encoding.ASCII.GetString(data, pos, 4); pos += 4;

                if (chunkTag == "IDAT")
                {
                    var chunk = new byte[chunkLength];
                    Buffer.BlockCopy(data, pos, chunk, 0, (int)chunkLength);
                    compressedData.Add(chunk);
                    totalCompressedLength += (int)chunkLength;
                }
                else if (chunkTag == "PLTE")
                {
                    info.Palette = new byte[chunkLength];
                    Buffer.BlockCopy(data, pos, info.Palette, 0, (int)chunkLength);
                }
                else if (chunkTag == "tRNS")
                {
                    info.TrnsData = new byte[chunkLength];
                    Buffer.BlockCopy(data, pos, info.TrnsData, 0, (int)chunkLength);
                }
                else if (chunkTag == "IEND")
                {
                    break;
                }

                pos += (int)chunkLength + 4; // data + CRC
            }

            // Decompress IDAT (zlib: 2-byte header + deflate)
            var allCompressed = new byte[totalCompressedLength];
            int offset = 0;
            foreach (var chunk in compressedData)
            {
                Buffer.BlockCopy(chunk, 0, allCompressed, offset, chunk.Length);
                offset += chunk.Length;
            }

            byte[] decompressed;
            using (var ms = new MemoryStream(allCompressed, 2, allCompressed.Length - 2))
            using (var deflate = new DeflateStream(ms, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                var buffer = ArrayPool<byte>.Shared.Rent(8192);
                try
                {
                    int bytesRead;
                    while ((bytesRead = deflate.Read(buffer, 0, buffer.Length)) > 0)
                        output.Write(buffer, 0, bytesRead);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
                decompressed = output.ToArray();
            }

            // Undo PNG row filters
            // For sub-byte depths, stride is computed in bytes (ceiling division)
            int bitsPerPixel = GetBitsPerPixel(info.ColorType, info.BitDepth);
            int filterByteUnit = Math.Max(1, bitsPerPixel / 8); // bytes per complete pixel for filter
            int stride = (info.Width * bitsPerPixel + 7) / 8;   // bytes per row (ceiling)
            rawPixels = new byte[info.Height * stride];

            int srcPos = 0;
            byte[]? prevRow = null;

            for (int row = 0; row < info.Height; row++)
            {
                byte filterType = decompressed[srcPos++];
                int rowStart = row * stride;

                Buffer.BlockCopy(decompressed, srcPos, rawPixels, rowStart, stride);
                srcPos += stride;

                switch (filterType)
                {
                    case 0: break;
                    case 1: // Sub
                        for (int i = filterByteUnit; i < stride; i++)
                            rawPixels[rowStart + i] = (byte)(rawPixels[rowStart + i] + rawPixels[rowStart + i - filterByteUnit]);
                        break;
                    case 2: // Up
                        if (prevRow != null)
                            for (int i = 0; i < stride; i++)
                                rawPixels[rowStart + i] = (byte)(rawPixels[rowStart + i] + prevRow[i]);
                        break;
                    case 3: // Average
                        for (int i = 0; i < stride; i++)
                        {
                            int left = i >= filterByteUnit ? rawPixels[rowStart + i - filterByteUnit] : 0;
                            int up = prevRow != null ? prevRow[i] : 0;
                            rawPixels[rowStart + i] = (byte)(rawPixels[rowStart + i] + (left + up) / 2);
                        }
                        break;
                    case 4: // Paeth
                        for (int i = 0; i < stride; i++)
                        {
                            int left = i >= filterByteUnit ? rawPixels[rowStart + i - filterByteUnit] : 0;
                            int up = prevRow != null ? prevRow[i] : 0;
                            int upLeft = (i >= filterByteUnit && prevRow != null) ? prevRow[i - filterByteUnit] : 0;
                            rawPixels[rowStart + i] = (byte)(rawPixels[rowStart + i] + PaethPredictor(left, up, upLeft));
                        }
                        break;
                }

                if (prevRow == null) prevRow = new byte[stride];
                Buffer.BlockCopy(rawPixels, rowStart, prevRow, 0, stride);
            }
        }

        private static int GetChannelCount(int colorType)
        {
            switch (colorType)
            {
                case 0: return 1;  // Grayscale
                case 2: return 3;  // RGB
                case 3: return 1;  // Indexed (1 index per pixel)
                case 4: return 2;  // Grayscale + Alpha
                case 6: return 4;  // RGBA
                default: return 3;
            }
        }

        private static int GetBitsPerPixel(int colorType, int bitDepth)
        {
            return GetChannelCount(colorType) * bitDepth;
        }

        private static byte[] Downsample16To8(byte[] data, int width, int height, int channels)
        {
            int pixelCount = width * height;
            int bytesPerPixel16 = channels * 2;
            int bytesPerPixel8 = channels;
            var result = new byte[pixelCount * bytesPerPixel8];

            for (int i = 0; i < pixelCount; i++)
            {
                int src = i * bytesPerPixel16;
                int dst = i * bytesPerPixel8;
                for (int c = 0; c < channels; c++)
                {
                    // Take high byte of 16-bit value (big-endian)
                    result[dst + c] = data[src + c * 2];
                }
            }

            return result;
        }

        private static byte[] UnpackSubByte(byte[] packed, int width, int height, int bitDepth)
        {
            // Unpack 1, 2, or 4-bit grayscale pixels to 8-bit
            int stride = (width * bitDepth + 7) / 8;
            var result = new byte[width * height];
            int pixelsPerByte = 8 / bitDepth;
            int mask = (1 << bitDepth) - 1;
            int maxVal = mask;

            for (int row = 0; row < height; row++)
            {
                int srcRowStart = row * stride;
                int dstRowStart = row * width;
                for (int col = 0; col < width; col++)
                {
                    int byteIndex = srcRowStart + col / pixelsPerByte;
                    int bitShift = 8 - bitDepth - (col % pixelsPerByte) * bitDepth;
                    int val = (packed[byteIndex] >> bitShift) & mask;
                    // Scale to 0-255
                    result[dstRowStart + col] = (byte)(val * 255 / maxVal);
                }
            }

            return result;
        }

        private static byte[] UnpackSubByteIndexed(byte[] packed, int width, int height, int bitDepth)
        {
            // Unpack 1, 2, or 4-bit palette indices to 8-bit (no scaling, just extract index)
            int stride = (width * bitDepth + 7) / 8;
            var result = new byte[width * height];
            int pixelsPerByte = 8 / bitDepth;
            int mask = (1 << bitDepth) - 1;

            for (int row = 0; row < height; row++)
            {
                int srcRowStart = row * stride;
                int dstRowStart = row * width;
                for (int col = 0; col < width; col++)
                {
                    int byteIndex = srcRowStart + col / pixelsPerByte;
                    int bitShift = 8 - bitDepth - (col % pixelsPerByte) * bitDepth;
                    result[dstRowStart + col] = (byte)((packed[byteIndex] >> bitShift) & mask);
                }
            }

            return result;
        }

        private static void SeparateAlpha(byte[] rawPixels, int width, int height,
                                            int components, out byte[] colorData, out byte[] alphaData)
        {
            int colorComponents = components - 1;
            int pixelCount = width * height;
            colorData = new byte[pixelCount * colorComponents];
            alphaData = new byte[pixelCount];

            int srcIdx = 0;
            int colorIdx = 0;
            int alphaIdx = 0;

            for (int i = 0; i < pixelCount; i++)
            {
                for (int c = 0; c < colorComponents; c++)
                    colorData[colorIdx++] = rawPixels[srcIdx++];
                alphaData[alphaIdx++] = rawPixels[srcIdx++];
            }
        }

        private static void CreateTrnsAlpha(byte[] rawPixels, PngInfo info,
                                             out byte[] colorData, out byte[] alphaData)
        {
            int pixelCount = info.Width * info.Height;
            colorData = rawPixels;

            if (info.ColorType == 0 && info.TrnsData != null && info.TrnsData.Length >= 2)
            {
                // Grayscale: tRNS is a single 16-bit gray value (use high byte for 8-bit)
                byte transparentGray = info.TrnsData[info.BitDepth <= 8 ? 1 : 0];
                alphaData = new byte[pixelCount];
                for (int i = 0; i < pixelCount; i++)
                    alphaData[i] = rawPixels[i] == transparentGray ? (byte)0 : (byte)255;
            }
            else if (info.ColorType == 2 && info.TrnsData != null && info.TrnsData.Length >= 6)
            {
                // RGB: tRNS is 3x 16-bit values (R, G, B)
                byte tr = info.TrnsData[1];
                byte tg = info.TrnsData[3];
                byte tb = info.TrnsData[5];
                alphaData = new byte[pixelCount];
                for (int i = 0; i < pixelCount; i++)
                {
                    int idx = i * 3;
                    bool isTransparent = rawPixels[idx] == tr &&
                                          rawPixels[idx + 1] == tg &&
                                          rawPixels[idx + 2] == tb;
                    alphaData[i] = isTransparent ? (byte)0 : (byte)255;
                }
            }
            else
            {
                alphaData = null!;
                colorData = rawPixels;
            }
        }

        private static int PaethPredictor(int a, int b, int c)
        {
            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);
            if (pa <= pb && pa <= pc) return a;
            if (pb <= pc) return b;
            return c;
        }

        private static uint ReadUInt32BE(byte[] data, int offset)
        {
            return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) |
                   ((uint)data[offset + 2] << 8) | data[offset + 3];
        }
    }
}
