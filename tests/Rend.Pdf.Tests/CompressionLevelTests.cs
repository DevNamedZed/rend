using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class CompressionLevelTests
    {
        // ═══════════════════════════════════════════
        // Enum values
        // ═══════════════════════════════════════════

        [Fact]
        public void PdfCompression_EnumValues_AreCorrect()
        {
            Assert.Equal(0, (int)PdfCompression.None);
            Assert.Equal(1, (int)PdfCompression.Flate);
            Assert.Equal(2, (int)PdfCompression.FlateFast);
            Assert.Equal(3, (int)PdfCompression.FlateOptimal);
        }

        // ═══════════════════════════════════════════
        // FlateFast produces valid PDF
        // ═══════════════════════════════════════════

        [Fact]
        public void FlateFast_ProducesValidPdf()
        {
            var pdfBytes = GenerateTestPdf(PdfCompression.FlateFast);
            AssertValidPdf(pdfBytes);
        }

        [Fact]
        public void FlateFast_ContainsFlateDecode()
        {
            var pdfBytes = GenerateTestPdf(PdfCompression.FlateFast);
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            Assert.Contains("/FlateDecode", pdfText);
        }

        // ═══════════════════════════════════════════
        // FlateOptimal produces valid PDF
        // ═══════════════════════════════════════════

        [Fact]
        public void FlateOptimal_ProducesValidPdf()
        {
            var pdfBytes = GenerateTestPdf(PdfCompression.FlateOptimal);
            AssertValidPdf(pdfBytes);
        }

        [Fact]
        public void FlateOptimal_ContainsFlateDecode()
        {
            var pdfBytes = GenerateTestPdf(PdfCompression.FlateOptimal);
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            Assert.Contains("/FlateDecode", pdfText);
        }

        // ═══════════════════════════════════════════
        // Flate (default) still works
        // ═══════════════════════════════════════════

        [Fact]
        public void Flate_ProducesValidPdf()
        {
            var pdfBytes = GenerateTestPdf(PdfCompression.Flate);
            AssertValidPdf(pdfBytes);
        }

        // ═══════════════════════════════════════════
        // None produces no FlateDecode
        // ═══════════════════════════════════════════

        [Fact]
        public void None_NoFlateDecode()
        {
            var pdfBytes = GenerateTestPdf(PdfCompression.None);
            string pdfText = Encoding.ASCII.GetString(pdfBytes);
            Assert.DoesNotContain("/FlateDecode", pdfText);
        }

        [Fact]
        public void None_ProducesValidPdf()
        {
            var pdfBytes = GenerateTestPdf(PdfCompression.None);
            AssertValidPdf(pdfBytes);
        }

        // ═══════════════════════════════════════════
        // FlateFast output is >= FlateOptimal size
        // ═══════════════════════════════════════════

        [Fact]
        public void FlateFast_OutputSize_GreaterOrEqualTo_FlateOptimal()
        {
            // Generate same document with both levels
            var fastBytes = GenerateLargeTestPdf(PdfCompression.FlateFast);
            var optimalBytes = GenerateLargeTestPdf(PdfCompression.FlateOptimal);

            // Fastest compression produces larger (or equal) output than Optimal
            Assert.True(fastBytes.Length >= optimalBytes.Length,
                $"FlateFast ({fastBytes.Length}) should be >= FlateOptimal ({optimalBytes.Length})");
        }

        [Fact]
        public void Flate_MatchesFlateOptimal_Size()
        {
            // Flate (default) maps to Optimal, so sizes should be identical
            var flateBytes = GenerateLargeTestPdf(PdfCompression.Flate);
            var optimalBytes = GenerateLargeTestPdf(PdfCompression.FlateOptimal);

            Assert.Equal(optimalBytes.Length, flateBytes.Length);
        }

        // ═══════════════════════════════════════════
        // Compression modes work with images
        // ═══════════════════════════════════════════

        [Fact]
        public void FlateFast_WithPngImage_ProducesValidPdf()
        {
            var pdfBytes = GeneratePdfWithImage(PdfCompression.FlateFast);
            AssertValidPdf(pdfBytes);
        }

        [Fact]
        public void FlateOptimal_WithPngImage_ProducesValidPdf()
        {
            var pdfBytes = GeneratePdfWithImage(PdfCompression.FlateOptimal);
            AssertValidPdf(pdfBytes);
        }

        // ═══════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════

        private static byte[] GenerateTestPdf(PdfCompression compression)
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = compression });
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            var page = doc.AddPage(PageSize.A4);

            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 750);
            page.Content.ShowText(font, "Compression level test.");
            page.Content.EndText();

            return doc.ToArray();
        }

        private static byte[] GenerateLargeTestPdf(PdfCompression compression)
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = compression });
            var font = doc.GetStandardFont(StandardFont.Helvetica);
            var page = doc.AddPage(PageSize.A4);

            page.Content.BeginText();
            page.Content.SetFont(font, 10);
            page.Content.MoveTextPosition(50, 750);
            // Write enough repetitive text to make compression differences visible
            for (int i = 0; i < 100; i++)
                page.Content.ShowText(font, "The quick brown fox jumps over the lazy dog. ");
            page.Content.EndText();

            return doc.ToArray();
        }

        private static byte[] GeneratePdfWithImage(PdfCompression compression)
        {
            // Build a small synthetic PNG
            byte[] png = BuildMinimalPng();

            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = compression });
            var image = doc.AddImage(png, ImageFormat.Png);
            var page = doc.AddPage(PageSize.A4);
            page.Content.DrawImage(image, new Rend.Core.Values.RectF(50, 600, 100, 100));

            return doc.ToArray();
        }

        private static byte[] BuildMinimalPng()
        {
            // Build a tiny 2x2 RGB PNG for testing
            int width = 2, height = 2;
            byte[] pixels = new byte[]
            {
                255, 0, 0, 0, 255, 0,   // row 0: red, green
                0, 0, 255, 255, 255, 0,  // row 1: blue, yellow
            };

            using var ms = new MemoryStream();

            // PNG signature
            ms.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 0, 8);

            // IHDR
            var ihdr = new byte[13];
            WriteUInt32BE(ihdr, 0, (uint)width);
            WriteUInt32BE(ihdr, 4, (uint)height);
            ihdr[8] = 8;  // bitDepth
            ihdr[9] = 2;  // colorType: RGB
            ihdr[10] = 0; // compression
            ihdr[11] = 0; // filter
            ihdr[12] = 0; // interlace: none
            WriteChunk(ms, "IHDR", ihdr);

            // IDAT: filter byte (0=None) + row data for each row
            int stride = width * 3;
            var rawIdat = new byte[height * (1 + stride)];
            int pos = 0;
            for (int row = 0; row < height; row++)
            {
                rawIdat[pos++] = 0; // filter: None
                Buffer.BlockCopy(pixels, row * stride, rawIdat, pos, stride);
                pos += stride;
            }

            // zlib compress
            byte[] compressed;
            using (var zlibMs = new MemoryStream())
            {
                zlibMs.WriteByte(0x78);
                zlibMs.WriteByte(0x01);
                using (var deflate = new DeflateStream(zlibMs, CompressionLevel.Optimal, leaveOpen: true))
                    deflate.Write(rawIdat, 0, rawIdat.Length);
                compressed = zlibMs.ToArray();
            }

            WriteChunk(ms, "IDAT", compressed);
            WriteChunk(ms, "IEND", Array.Empty<byte>());

            return ms.ToArray();
        }

        private static void WriteChunk(Stream stream, string tag, byte[] data)
        {
            var lenBytes = new byte[4];
            WriteUInt32BE(lenBytes, 0, (uint)data.Length);
            stream.Write(lenBytes, 0, 4);

            var tagBytes = Encoding.ASCII.GetBytes(tag);
            stream.Write(tagBytes, 0, 4);

            if (data.Length > 0)
                stream.Write(data, 0, data.Length);

            uint crc = Crc32(tagBytes, data);
            var crcBytes = new byte[4];
            WriteUInt32BE(crcBytes, 0, crc);
            stream.Write(crcBytes, 0, 4);
        }

        private static void AssertValidPdf(byte[] pdfBytes)
        {
            Assert.True(pdfBytes.Length > 100, "PDF should be more than 100 bytes");
            string header = Encoding.ASCII.GetString(pdfBytes, 0, 5);
            Assert.Equal("%PDF-", header);
            string fullText = Encoding.ASCII.GetString(pdfBytes);
            Assert.Contains("%%EOF", fullText);
            Assert.Contains("/Catalog", fullText);
        }

        private static void WriteUInt32BE(byte[] buf, int offset, uint value)
        {
            buf[offset] = (byte)(value >> 24);
            buf[offset + 1] = (byte)((value >> 16) & 0xFF);
            buf[offset + 2] = (byte)((value >> 8) & 0xFF);
            buf[offset + 3] = (byte)(value & 0xFF);
        }

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
