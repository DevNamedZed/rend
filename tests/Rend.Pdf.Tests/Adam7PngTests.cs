using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class Adam7PngTests
    {
        // ═══════════════════════════════════════════
        // Non-interlaced still works (regression)
        // ═══════════════════════════════════════════

        [Fact]
        public void NonInterlaced_1x1_RGBA_StillWorks()
        {
            // 1×1 RGBA non-interlaced PNG
            byte[] png = BuildPng(1, 1, bitDepth: 8, colorType: 6, interlaced: false,
                pixels: new byte[] { 255, 0, 0, 128 }); // red with 50% alpha

            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var image = doc.AddImage(png, ImageFormat.Png);

            Assert.Equal(1, image.Width);
            Assert.Equal(1, image.Height);
            Assert.True(image.HasAlpha);
        }

        [Fact]
        public void NonInterlaced_8x8_Grayscale_StillWorks()
        {
            byte[] pixels = new byte[8 * 8];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = (byte)(i * 4); // gradient

            byte[] png = BuildPng(8, 8, bitDepth: 8, colorType: 0, interlaced: false, pixels: pixels);

            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var image = doc.AddImage(png, ImageFormat.Png);

            Assert.Equal(8, image.Width);
            Assert.Equal(8, image.Height);
            Assert.False(image.HasAlpha);
        }

        // ═══════════════════════════════════════════
        // Adam7 interlaced PNGs
        // ═══════════════════════════════════════════

        [Fact]
        public void Interlaced_1x1_RGBA_DecodesCorrectly()
        {
            byte[] pixels = new byte[] { 255, 0, 0, 255 }; // red, fully opaque

            byte[] png = BuildPng(1, 1, bitDepth: 8, colorType: 6, interlaced: true, pixels: pixels);

            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var image = doc.AddImage(png, ImageFormat.Png);

            Assert.Equal(1, image.Width);
            Assert.Equal(1, image.Height);
            Assert.True(image.HasAlpha);

            // Verify the PDF output is valid and contains image XObject
            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            Assert.Contains("/Image", pdfText);
        }

        [Fact]
        public void Interlaced_8x8_Grayscale_DecodesCorrectly()
        {
            // 8×8 grayscale — exercises all 7 Adam7 passes
            byte[] pixels = new byte[8 * 8];
            for (int row = 0; row < 8; row++)
                for (int col = 0; col < 8; col++)
                    pixels[row * 8 + col] = (byte)(row * 32 + col * 4);

            byte[] png = BuildPng(8, 8, bitDepth: 8, colorType: 0, interlaced: true, pixels: pixels);

            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var image = doc.AddImage(png, ImageFormat.Png);

            Assert.Equal(8, image.Width);
            Assert.Equal(8, image.Height);
            Assert.False(image.HasAlpha);

            // Verify valid PDF
            var page = doc.AddPage(200, 200);
            page.Content.DrawImage(image, new Rend.Core.Values.RectF(0, 0, 200, 200));
            var pdfBytes = doc.ToArray();
            Assert.True(pdfBytes.Length > 100);
        }

        [Fact]
        public void Interlaced_5x3_RGB_NonPowerOfTwo()
        {
            // Non-power-of-2 dimensions — some passes will have 0 pixels
            int w = 5, h = 3;
            byte[] pixels = new byte[w * h * 3]; // RGB
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = (byte)(i % 256);

            byte[] png = BuildPng(w, h, bitDepth: 8, colorType: 2, interlaced: true, pixels: pixels);

            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var image = doc.AddImage(png, ImageFormat.Png);

            Assert.Equal(5, image.Width);
            Assert.Equal(3, image.Height);
            Assert.False(image.HasAlpha);
        }

        [Fact]
        public void Interlaced_1BitIndexed_SubByteScatter()
        {
            // 1-bit indexed (palette) interlaced PNG — exercises sub-byte bit scatter
            int w = 8, h = 8;
            byte[] palette = new byte[] { 0, 0, 0, 255, 255, 255 }; // black and white

            // Pixel indices: checkerboard pattern (0 or 1)
            byte[] pixelIndices = new byte[w * h];
            for (int row = 0; row < h; row++)
                for (int col = 0; col < w; col++)
                    pixelIndices[row * w + col] = (byte)((row + col) % 2);

            byte[] png = BuildIndexedPng(w, h, bitDepth: 1, interlaced: true,
                palette: palette, pixelIndices: pixelIndices);

            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var image = doc.AddImage(png, ImageFormat.Png);

            Assert.Equal(8, image.Width);
            Assert.Equal(8, image.Height);

            // Verify it produces valid PDF output
            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            Assert.Contains("/Indexed", pdfText);
        }

        [Fact]
        public void Interlaced_RGBA_ProducesValidPdfWithSMask()
        {
            // 4×4 RGBA interlaced
            int w = 4, h = 4;
            byte[] pixels = new byte[w * h * 4];
            for (int i = 0; i < w * h; i++)
            {
                pixels[i * 4 + 0] = 128; // R
                pixels[i * 4 + 1] = 64;  // G
                pixels[i * 4 + 2] = 32;  // B
                pixels[i * 4 + 3] = (byte)(i * 16); // A gradient
            }

            byte[] png = BuildPng(w, h, bitDepth: 8, colorType: 6, interlaced: true, pixels: pixels);

            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var image = doc.AddImage(png, ImageFormat.Png);

            Assert.True(image.HasAlpha);

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            Assert.Contains("/SMask", pdfText);
        }

        [Fact]
        public void Interlaced_3x1_SingleRowImage()
        {
            // Edge case: single row
            byte[] pixels = new byte[] { 255, 0, 0, 0, 255, 0, 0, 0, 255 }; // R, G, B pixels

            byte[] png = BuildPng(3, 1, bitDepth: 8, colorType: 2, interlaced: true, pixels: pixels);

            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var image = doc.AddImage(png, ImageFormat.Png);

            Assert.Equal(3, image.Width);
            Assert.Equal(1, image.Height);
        }

        [Fact]
        public void Interlaced_1x3_SingleColumnImage()
        {
            // Edge case: single column
            byte[] pixels = new byte[] { 100, 150, 200 }; // 3 grayscale pixels

            byte[] png = BuildPng(1, 3, bitDepth: 8, colorType: 0, interlaced: true, pixels: pixels);

            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var image = doc.AddImage(png, ImageFormat.Png);

            Assert.Equal(1, image.Width);
            Assert.Equal(3, image.Height);
        }

        // ═══════════════════════════════════════════
        // Synthetic PNG Builder
        // ═══════════════════════════════════════════

        // Adam7 pass definitions: { startCol, colStep, startRow, rowStep }
        private static readonly int[][] Adam7Passes = new[]
        {
            new[] { 0, 8, 0, 8 },
            new[] { 4, 8, 0, 8 },
            new[] { 0, 4, 4, 8 },
            new[] { 2, 4, 0, 4 },
            new[] { 0, 2, 2, 4 },
            new[] { 1, 2, 0, 2 },
            new[] { 0, 1, 1, 2 },
        };

        /// <summary>
        /// Build a valid PNG file from raw pixel data.
        /// Supports grayscale (0), RGB (2), grayscale+alpha (4), RGBA (6).
        /// </summary>
        private static byte[] BuildPng(int width, int height, int bitDepth, int colorType,
                                        bool interlaced, byte[] pixels)
        {
            int channels = GetChannels(colorType);
            int bytesPerPixel = channels * (bitDepth / 8);

            byte[] idatPayload;
            if (interlaced)
            {
                idatPayload = EncodeAdam7(pixels, width, height, bytesPerPixel);
            }
            else
            {
                idatPayload = EncodeNonInterlaced(pixels, width, height, bytesPerPixel);
            }

            byte[] compressedIdat = ZlibCompress(idatPayload);

            return AssemblePng(width, height, bitDepth, colorType, interlaced ? 1 : 0,
                compressedIdat, null);
        }

        /// <summary>
        /// Build a valid indexed (palette) PNG file.
        /// </summary>
        private static byte[] BuildIndexedPng(int width, int height, int bitDepth, bool interlaced,
                                               byte[] palette, byte[] pixelIndices)
        {
            byte[] idatPayload;
            if (interlaced)
            {
                idatPayload = EncodeAdam7Indexed(pixelIndices, width, height, bitDepth);
            }
            else
            {
                idatPayload = EncodeNonInterlacedIndexed(pixelIndices, width, height, bitDepth);
            }

            byte[] compressedIdat = ZlibCompress(idatPayload);

            return AssemblePng(width, height, bitDepth, 3 /* indexed */, interlaced ? 1 : 0,
                compressedIdat, palette);
        }

        private static byte[] EncodeNonInterlaced(byte[] pixels, int width, int height, int bytesPerPixel)
        {
            int stride = width * bytesPerPixel;
            var result = new byte[height * (1 + stride)]; // filter byte + row data
            int pos = 0;
            for (int row = 0; row < height; row++)
            {
                result[pos++] = 0; // filter: None
                Buffer.BlockCopy(pixels, row * stride, result, pos, stride);
                pos += stride;
            }
            return result;
        }

        private static byte[] EncodeNonInterlacedIndexed(byte[] pixelIndices, int width, int height, int bitDepth)
        {
            int ppb = 8 / bitDepth;
            int stride = (width * bitDepth + 7) / 8;
            var result = new byte[height * (1 + stride)];
            int pos = 0;
            for (int row = 0; row < height; row++)
            {
                result[pos++] = 0; // filter: None
                for (int col = 0; col < width; col++)
                {
                    int idx = pixelIndices[row * width + col];
                    int byteOffset = pos + col / ppb;
                    int bitShift = 8 - bitDepth - (col % ppb) * bitDepth;
                    result[byteOffset] |= (byte)(idx << bitShift);
                }
                pos += stride;
            }
            return result;
        }

        private static byte[] EncodeAdam7(byte[] pixels, int width, int height, int bytesPerPixel)
        {
            using var ms = new MemoryStream();

            int stride = width * bytesPerPixel;

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

                int passStride = passWidth * bytesPerPixel;

                for (int passRow = 0; passRow < passHeight; passRow++)
                {
                    ms.WriteByte(0); // filter: None
                    int srcRow = startRow + passRow * rowStep;

                    for (int passCol = 0; passCol < passWidth; passCol++)
                    {
                        int srcCol = startCol + passCol * colStep;
                        int srcOffset = srcRow * stride + srcCol * bytesPerPixel;
                        ms.Write(pixels, srcOffset, bytesPerPixel);
                    }
                }
            }

            return ms.ToArray();
        }

        private static byte[] EncodeAdam7Indexed(byte[] pixelIndices, int width, int height, int bitDepth)
        {
            using var ms = new MemoryStream();
            int ppb = 8 / bitDepth;

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

                int passStride = (passWidth * bitDepth + 7) / 8;

                for (int passRow = 0; passRow < passHeight; passRow++)
                {
                    ms.WriteByte(0); // filter: None
                    int srcRow = startRow + passRow * rowStep;

                    var rowBytes = new byte[passStride];
                    for (int passCol = 0; passCol < passWidth; passCol++)
                    {
                        int srcCol = startCol + passCol * colStep;
                        int idx = pixelIndices[srcRow * width + srcCol];
                        int byteOffset = passCol / ppb;
                        int bitShift = 8 - bitDepth - (passCol % ppb) * bitDepth;
                        rowBytes[byteOffset] |= (byte)(idx << bitShift);
                    }
                    ms.Write(rowBytes, 0, passStride);
                }
            }

            return ms.ToArray();
        }

        private static byte[] ZlibCompress(byte[] data)
        {
            // zlib = 2-byte header + deflate data
            using var ms = new MemoryStream();
            // zlib header: CMF=0x78 (deflate, 32K window), FLG=0x01 (no dict, check bits)
            ms.WriteByte(0x78);
            ms.WriteByte(0x01);
            using (var deflate = new DeflateStream(ms, CompressionLevel.Optimal, leaveOpen: true))
            {
                deflate.Write(data, 0, data.Length);
            }
            return ms.ToArray();
        }

        private static byte[] AssemblePng(int width, int height, int bitDepth, int colorType,
                                           int interlaceMethod, byte[] compressedIdat, byte[]? palette)
        {
            using var ms = new MemoryStream();

            // PNG signature
            ms.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 0, 8);

            // IHDR chunk
            var ihdr = new byte[13];
            WriteUInt32BE(ihdr, 0, (uint)width);
            WriteUInt32BE(ihdr, 4, (uint)height);
            ihdr[8] = (byte)bitDepth;
            ihdr[9] = (byte)colorType;
            ihdr[10] = 0; // compression
            ihdr[11] = 0; // filter
            ihdr[12] = (byte)interlaceMethod;
            WriteChunk(ms, "IHDR", ihdr);

            // PLTE chunk (for indexed)
            if (palette != null)
                WriteChunk(ms, "PLTE", palette);

            // IDAT chunk
            WriteChunk(ms, "IDAT", compressedIdat);

            // IEND chunk
            WriteChunk(ms, "IEND", Array.Empty<byte>());

            return ms.ToArray();
        }

        private static void WriteChunk(Stream stream, string tag, byte[] data)
        {
            // Length (4 bytes BE)
            var lenBytes = new byte[4];
            WriteUInt32BE(lenBytes, 0, (uint)data.Length);
            stream.Write(lenBytes, 0, 4);

            // Tag (4 bytes ASCII)
            var tagBytes = Encoding.ASCII.GetBytes(tag);
            stream.Write(tagBytes, 0, 4);

            // Data
            if (data.Length > 0)
                stream.Write(data, 0, data.Length);

            // CRC (computed over tag + data)
            uint crc = Crc32(tagBytes, data);
            var crcBytes = new byte[4];
            WriteUInt32BE(crcBytes, 0, crc);
            stream.Write(crcBytes, 0, 4);
        }

        private static int GetChannels(int colorType)
        {
            switch (colorType)
            {
                case 0: return 1;
                case 2: return 3;
                case 3: return 1;
                case 4: return 2;
                case 6: return 4;
                default: return 3;
            }
        }

        private static void WriteUInt32BE(byte[] buf, int offset, uint value)
        {
            buf[offset] = (byte)(value >> 24);
            buf[offset + 1] = (byte)((value >> 16) & 0xFF);
            buf[offset + 2] = (byte)((value >> 8) & 0xFF);
            buf[offset + 3] = (byte)(value & 0xFF);
        }

        // CRC-32 for PNG chunks (ISO 3309 / ITU-T V.42)
        private static readonly uint[] Crc32Table = BuildCrc32Table();

        private static uint[] BuildCrc32Table()
        {
            var table = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                uint c = n;
                for (int k = 0; k < 8; k++)
                    c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
                table[n] = c;
            }
            return table;
        }

        private static uint Crc32(byte[] tag, byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < tag.Length; i++)
                crc = Crc32Table[(crc ^ tag[i]) & 0xFF] ^ (crc >> 8);
            for (int i = 0; i < data.Length; i++)
                crc = Crc32Table[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
            return crc ^ 0xFFFFFFFF;
        }
    }
}
