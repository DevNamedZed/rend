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
                                            PdfObjectTable objectTable, bool compress,
                                            CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            ParsePng(pngData, out var info, out byte[] rawPixels);

            // Indexed color (colorType 3) — use /Indexed color space directly
            if (info.ColorType == 3)
                return CreateIndexedImage(info, rawPixels, resourceName, objectTable, compress, compressionLevel);

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

            PdfObject colorSpaceObj;
            if (info.IccProfile != null)
            {
                // Build ICCBased color space
                var iccStream = new PdfStream(info.IccProfile, compress) { CompressionLevel = compressionLevel };
                iccStream.Dict[PdfName.N] = new PdfInteger(info.IccComponents);
                iccStream.Dict[PdfName.Alternate] = info.IccComponents == 1 ? PdfName.DeviceGray : PdfName.DeviceRGB;
                var iccRef = objectTable.Allocate(iccStream);
                var csArray = new PdfArray(2);
                csArray.Add(PdfName.ICCBased);
                csArray.Add(iccRef);
                colorSpaceObj = csArray;
            }
            else
            {
                colorSpaceObj = components == 1 ? (PdfObject)PdfName.DeviceGray : PdfName.DeviceRGB;
            }

            var imageStream = new PdfStream(colorData, compress) { CompressionLevel = compressionLevel };
            imageStream.Dict[PdfName.Type] = PdfName.XObject;
            imageStream.Dict[PdfName.Subtype] = PdfName.Image;
            imageStream.Dict[PdfName.Width] = new PdfInteger(info.Width);
            imageStream.Dict[PdfName.Height] = new PdfInteger(info.Height);
            imageStream.Dict[PdfName.BitsPerComponent] = new PdfInteger(outputBitDepth);
            imageStream.Dict[PdfName.ColorSpace] = colorSpaceObj;

            PdfReference? smaskRef = null;
            if (alphaData != null)
            {
                var smaskStream = new PdfStream(alphaData, compress) { CompressionLevel = compressionLevel };
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
            string resourceName, PdfObjectTable objectTable, bool compress,
            CompressionLevel compressionLevel = CompressionLevel.Optimal)
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

            var imageStream = new PdfStream(rawPixels, compress) { CompressionLevel = compressionLevel };
            imageStream.Dict[PdfName.Type] = PdfName.XObject;
            imageStream.Dict[PdfName.Subtype] = PdfName.Image;
            imageStream.Dict[PdfName.Width] = new PdfInteger(info.Width);
            imageStream.Dict[PdfName.Height] = new PdfInteger(info.Height);
            imageStream.Dict[PdfName.BitsPerComponent] = new PdfInteger(outputBitDepth);
            imageStream.Dict[PdfName.ColorSpace] = indexedCs;

            PdfReference? smaskRef = null;
            if (alphaData != null)
            {
                var smaskStream = new PdfStream(alphaData, compress) { CompressionLevel = compressionLevel };
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
            public byte[]? IccProfile; // iCCP chunk: raw ICC profile data
            public int IccComponents; // Number of ICC color components
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

            info.Palette = null;
            info.TrnsData = null;
            info.IccProfile = null;
            info.IccComponents = 0;

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
                else if (chunkTag == "iCCP")
                {
                    // Read null-terminated profile name
                    int nameEnd = pos;
                    while (nameEnd < pos + (int)chunkLength && data[nameEnd] != 0) nameEnd++;
                    // Skip name + null byte + compression method byte (always 0)
                    int profileDataStart = nameEnd + 2;
                    int profileDataLen = (int)chunkLength - (profileDataStart - pos);
                    if (profileDataLen > 2)
                    {
                        // Decompress zlib data (2-byte header + deflate)
                        using (var ms = new MemoryStream(data, profileDataStart + 2, profileDataLen - 2))
                        using (var deflate = new DeflateStream(ms, CompressionMode.Decompress))
                        using (var output = new MemoryStream())
                        {
                            var buf = ArrayPool<byte>.Shared.Rent(4096);
                            try
                            {
                                int bytesRead;
                                while ((bytesRead = deflate.Read(buf, 0, buf.Length)) > 0)
                                    output.Write(buf, 0, bytesRead);
                            }
                            finally
                            {
                                ArrayPool<byte>.Shared.Return(buf);
                            }
                            info.IccProfile = output.ToArray();
                        }
                        // Set component count based on color type
                        switch (info.ColorType)
                        {
                            case 0: case 4: info.IccComponents = 1; break; // Grayscale
                            default: info.IccComponents = 3; break; // RGB, Indexed, RGBA
                        }
                    }
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

            int bitsPerPixel = GetBitsPerPixel(info.ColorType, info.BitDepth);

            if (interlaceMethod == 1)
            {
                // Adam7 interlaced
                rawPixels = DeinterlaceAdam7(decompressed, info.Width, info.Height, bitsPerPixel);
            }
            else
            {
                // Non-interlaced: undo PNG row filters inline
                int filterByteUnit = Math.Max(1, bitsPerPixel / 8);
                int stride = (info.Width * bitsPerPixel + 7) / 8;
                rawPixels = new byte[info.Height * stride];

                int srcPos = 0;
                UnfilterRows(decompressed, ref srcPos, rawPixels, stride, info.Height, filterByteUnit);
            }
        }

        /// <summary>
        /// Undo PNG row filters for a sequence of rows.
        /// Reads (1 + stride) bytes per row from <paramref name="decompressed"/> starting at <paramref name="srcPos"/>.
        /// Writes unfiltered bytes into <paramref name="output"/> (row 0 at offset 0, row 1 at offset <paramref name="outputStride"/>, etc.).
        /// </summary>
        private static void UnfilterRows(byte[] decompressed, ref int srcPos, byte[] output,
                                          int outputStride, int rowCount, int filterByteUnit)
        {
            byte[]? prevRow = null;

            for (int row = 0; row < rowCount; row++)
            {
                byte filterType = decompressed[srcPos++];
                int rowStart = row * outputStride;

                Buffer.BlockCopy(decompressed, srcPos, output, rowStart, outputStride);
                srcPos += outputStride;

                switch (filterType)
                {
                    case 0: break;
                    case 1: // Sub
                        for (int i = filterByteUnit; i < outputStride; i++)
                            output[rowStart + i] = (byte)(output[rowStart + i] + output[rowStart + i - filterByteUnit]);
                        break;
                    case 2: // Up
                        if (prevRow != null)
                            for (int i = 0; i < outputStride; i++)
                                output[rowStart + i] = (byte)(output[rowStart + i] + prevRow[i]);
                        break;
                    case 3: // Average
                        for (int i = 0; i < outputStride; i++)
                        {
                            int left = i >= filterByteUnit ? output[rowStart + i - filterByteUnit] : 0;
                            int up = prevRow != null ? prevRow[i] : 0;
                            output[rowStart + i] = (byte)(output[rowStart + i] + (left + up) / 2);
                        }
                        break;
                    case 4: // Paeth
                        for (int i = 0; i < outputStride; i++)
                        {
                            int left = i >= filterByteUnit ? output[rowStart + i - filterByteUnit] : 0;
                            int up = prevRow != null ? prevRow[i] : 0;
                            int upLeft = (i >= filterByteUnit && prevRow != null) ? prevRow[i - filterByteUnit] : 0;
                            output[rowStart + i] = (byte)(output[rowStart + i] + PaethPredictor(left, up, upLeft));
                        }
                        break;
                }

                if (prevRow == null) prevRow = new byte[outputStride];
                Buffer.BlockCopy(output, rowStart, prevRow, 0, outputStride);
            }
        }

        // Adam7 interlace pass definitions: { startCol, colStep, startRow, rowStep }
        private static readonly int[][] Adam7Passes = new[]
        {
            new[] { 0, 8, 0, 8 }, // pass 1
            new[] { 4, 8, 0, 8 }, // pass 2
            new[] { 0, 4, 4, 8 }, // pass 3
            new[] { 2, 4, 0, 4 }, // pass 4
            new[] { 0, 2, 2, 4 }, // pass 5
            new[] { 1, 2, 0, 2 }, // pass 6
            new[] { 0, 1, 1, 2 }, // pass 7
        };

        /// <summary>
        /// Decode Adam7 interlaced PNG data into a full image buffer.
        /// </summary>
        private static byte[] DeinterlaceAdam7(byte[] decompressed, int width, int height, int bitsPerPixel)
        {
            int fullStride = (width * bitsPerPixel + 7) / 8;
            var result = new byte[height * fullStride];
            int filterByteUnit = Math.Max(1, bitsPerPixel / 8);
            int srcPos = 0;

            for (int pass = 0; pass < 7; pass++)
            {
                int startCol = Adam7Passes[pass][0];
                int colStep = Adam7Passes[pass][1];
                int startRow = Adam7Passes[pass][2];
                int rowStep = Adam7Passes[pass][3];

                int passWidth = (width - startCol + colStep - 1) / colStep;
                int passHeight = (height - startRow + rowStep - 1) / rowStep;

                if (passWidth <= 0 || passHeight <= 0)
                    continue;

                int passStride = (passWidth * bitsPerPixel + 7) / 8;
                var passBuf = new byte[passHeight * passStride];

                UnfilterRows(decompressed, ref srcPos, passBuf, passStride, passHeight, filterByteUnit);

                // Scatter pass pixels into the full image
                if (bitsPerPixel >= 8)
                {
                    int bytesPerPixel = bitsPerPixel / 8;
                    for (int passRow = 0; passRow < passHeight; passRow++)
                    {
                        int srcRowStart = passRow * passStride;
                        int dstRow = startRow + passRow * rowStep;
                        int dstRowStart = dstRow * fullStride;

                        for (int passCol = 0; passCol < passWidth; passCol++)
                        {
                            int dstCol = startCol + passCol * colStep;
                            int srcOffset = srcRowStart + passCol * bytesPerPixel;
                            int dstOffset = dstRowStart + dstCol * bytesPerPixel;
                            Buffer.BlockCopy(passBuf, srcOffset, result, dstOffset, bytesPerPixel);
                        }
                    }
                }
                else
                {
                    // Sub-byte bit depths (1, 2, 4): scatter individual pixel bits
                    int bitDepth = bitsPerPixel; // for sub-byte, bitsPerPixel == bitDepth
                    int ppb = 8 / bitDepth; // pixels per byte
                    int mask = (1 << bitDepth) - 1;

                    for (int passRow = 0; passRow < passHeight; passRow++)
                    {
                        int srcRowStart = passRow * passStride;
                        int dstRow = startRow + passRow * rowStep;
                        int dstRowStart = dstRow * fullStride;

                        for (int passCol = 0; passCol < passWidth; passCol++)
                        {
                            int dstCol = startCol + passCol * colStep;

                            // Extract source pixel value
                            int srcByteIdx = srcRowStart + passCol / ppb;
                            int srcBitShift = 8 - bitDepth - (passCol % ppb) * bitDepth;
                            int pixelValue = (passBuf[srcByteIdx] >> srcBitShift) & mask;

                            // Place into destination
                            int dstByteIdx = dstRowStart + dstCol / ppb;
                            int dstBitShift = 8 - bitDepth - (dstCol % ppb) * bitDepth;
                            result[dstByteIdx] = (byte)((result[dstByteIdx] & ~(mask << dstBitShift)) | (pixelValue << dstBitShift));
                        }
                    }
                }
            }

            return result;
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
