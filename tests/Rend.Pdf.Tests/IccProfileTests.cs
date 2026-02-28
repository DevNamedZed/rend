using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class IccProfileTests
    {
        /// <summary>
        /// Build a minimal 132-byte ICC profile header with the given color space.
        /// </summary>
        private static byte[] BuildSyntheticIccProfile(int components)
        {
            var profile = new byte[132];
            // Profile size (big-endian)
            profile[0] = 0; profile[1] = 0; profile[2] = 0; profile[3] = 132;
            // Preferred CMM type
            // Profile version 2.1.0
            profile[8] = 2; profile[9] = 0x10;
            // Device class: 'mntr' (monitor)
            profile[12] = (byte)'m'; profile[13] = (byte)'n'; profile[14] = (byte)'t'; profile[15] = (byte)'r';
            // Color space signature
            if (components == 1)
            {
                profile[16] = (byte)'G'; profile[17] = (byte)'R'; profile[18] = (byte)'A'; profile[19] = (byte)'Y';
            }
            else
            {
                profile[16] = (byte)'R'; profile[17] = (byte)'G'; profile[18] = (byte)'B'; profile[19] = (byte)' ';
            }
            // PCS: XYZ
            profile[20] = (byte)'X'; profile[21] = (byte)'Y'; profile[22] = (byte)'Z'; profile[23] = (byte)' ';
            // Tag count = 0 (at offset 128)
            profile[128] = 0; profile[129] = 0; profile[130] = 0; profile[131] = 0;
            return profile;
        }

        /// <summary>
        /// Build a minimal PNG with an iCCP chunk containing the given ICC profile.
        /// </summary>
        private static byte[] BuildPngWithIcc(int width, int height, int colorType, byte[] iccProfile)
        {
            using var ms = new MemoryStream();

            // PNG signature
            ms.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 0, 8);

            // IHDR chunk
            var ihdr = new byte[13];
            WriteUInt32BE(ihdr, 0, (uint)width);
            WriteUInt32BE(ihdr, 4, (uint)height);
            ihdr[8] = 8; // bit depth
            ihdr[9] = (byte)colorType;
            ihdr[10] = 0; // compression
            ihdr[11] = 0; // filter
            ihdr[12] = 0; // interlace
            WriteChunk(ms, "IHDR", ihdr);

            // iCCP chunk
            byte[] compressedIcc;
            using (var iccMs = new MemoryStream())
            {
                // zlib header (CMF=0x78, FLG=0x01)
                iccMs.WriteByte(0x78);
                iccMs.WriteByte(0x01);
                using (var deflate = new DeflateStream(iccMs, CompressionLevel.Fastest, leaveOpen: true))
                    deflate.Write(iccProfile, 0, iccProfile.Length);
                compressedIcc = iccMs.ToArray();
            }
            // Profile name "sRGB" + null + compression method (0) + compressed data
            var iccpData = new byte[6 + compressedIcc.Length];
            iccpData[0] = (byte)'s'; iccpData[1] = (byte)'R'; iccpData[2] = (byte)'G'; iccpData[3] = (byte)'B';
            iccpData[4] = 0; // null terminator
            iccpData[5] = 0; // compression method
            Buffer.BlockCopy(compressedIcc, 0, iccpData, 6, compressedIcc.Length);
            WriteChunk(ms, "iCCP", iccpData);

            // PLTE for indexed color
            if (colorType == 3)
            {
                var plte = new byte[] { 255, 0, 0, 0, 255, 0, 0, 0, 255 }; // 3 colors
                WriteChunk(ms, "PLTE", plte);
            }

            // IDAT chunk with minimal image data
            byte[] rawPixels = BuildRawPixelData(width, height, colorType);
            byte[] compressedPixels;
            using (var pixMs = new MemoryStream())
            {
                pixMs.WriteByte(0x78);
                pixMs.WriteByte(0x01);
                using (var deflate = new DeflateStream(pixMs, CompressionLevel.Fastest, leaveOpen: true))
                    deflate.Write(rawPixels, 0, rawPixels.Length);
                compressedPixels = pixMs.ToArray();
            }
            WriteChunk(ms, "IDAT", compressedPixels);

            // IEND
            WriteChunk(ms, "IEND", Array.Empty<byte>());

            return ms.ToArray();
        }

        /// <summary>
        /// Build a minimal PNG without iCCP chunk.
        /// </summary>
        private static byte[] BuildPngWithoutIcc(int width, int height, int colorType)
        {
            using var ms = new MemoryStream();
            ms.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 0, 8);

            var ihdr = new byte[13];
            WriteUInt32BE(ihdr, 0, (uint)width);
            WriteUInt32BE(ihdr, 4, (uint)height);
            ihdr[8] = 8;
            ihdr[9] = (byte)colorType;
            WriteChunk(ms, "IHDR", ihdr);

            byte[] rawPixels = BuildRawPixelData(width, height, colorType);
            byte[] compressedPixels;
            using (var pixMs = new MemoryStream())
            {
                pixMs.WriteByte(0x78);
                pixMs.WriteByte(0x01);
                using (var deflate = new DeflateStream(pixMs, CompressionLevel.Fastest, leaveOpen: true))
                    deflate.Write(rawPixels, 0, rawPixels.Length);
                compressedPixels = pixMs.ToArray();
            }
            WriteChunk(ms, "IDAT", compressedPixels);
            WriteChunk(ms, "IEND", Array.Empty<byte>());

            return ms.ToArray();
        }

        private static byte[] BuildRawPixelData(int width, int height, int colorType)
        {
            int channels;
            switch (colorType)
            {
                case 0: channels = 1; break; // grayscale
                case 2: channels = 3; break; // RGB
                case 3: channels = 1; break; // indexed
                case 4: channels = 2; break; // gray+alpha
                case 6: channels = 4; break; // RGBA
                default: channels = 3; break;
            }
            int stride = width * channels;
            var data = new byte[height * (1 + stride)]; // filter byte + pixel data per row
            // All filter bytes = 0 (None), pixel data = 0
            return data;
        }

        /// <summary>
        /// Build a minimal JPEG with an APP2 ICC_PROFILE marker.
        /// </summary>
        private static byte[] BuildJpegWithIcc(int width, int height, byte[] iccProfile)
        {
            using var ms = new MemoryStream();
            // SOI
            ms.WriteByte(0xFF); ms.WriteByte(0xD8);

            // APP2 ICC_PROFILE marker
            var iccMarkerData = new byte[16 + iccProfile.Length];
            // Segment length (includes 2 bytes for length field)
            int segLen = 2 + 14 + iccProfile.Length;
            iccMarkerData[0] = (byte)(segLen >> 8);
            iccMarkerData[1] = (byte)(segLen & 0xFF);
            // "ICC_PROFILE\0"
            byte[] id = System.Text.Encoding.ASCII.GetBytes("ICC_PROFILE");
            Buffer.BlockCopy(id, 0, iccMarkerData, 2, 11);
            iccMarkerData[13] = 0; // null
            iccMarkerData[14] = 1; // chunk sequence number
            iccMarkerData[15] = 1; // total chunks
            Buffer.BlockCopy(iccProfile, 0, iccMarkerData, 16, iccProfile.Length);
            ms.WriteByte(0xFF); ms.WriteByte(0xE2);
            ms.Write(iccMarkerData, 0, iccMarkerData.Length);

            // SOF0 marker
            ms.WriteByte(0xFF); ms.WriteByte(0xC0);
            var sof = new byte[11];
            sof[0] = 0; sof[1] = 11; // length
            sof[2] = 8; // precision
            sof[3] = (byte)(height >> 8); sof[4] = (byte)(height & 0xFF);
            sof[5] = (byte)(width >> 8); sof[6] = (byte)(width & 0xFF);
            sof[7] = 3; // components
            sof[8] = 1; sof[9] = 0x11; sof[10] = 0; // component 1
            ms.Write(sof, 0, sof.Length);

            // SOS marker (minimal)
            ms.WriteByte(0xFF); ms.WriteByte(0xDA);
            ms.WriteByte(0); ms.WriteByte(3); // length = 3
            ms.WriteByte(0); // data

            // EOI
            ms.WriteByte(0xFF); ms.WriteByte(0xD9);

            return ms.ToArray();
        }

        /// <summary>
        /// Build a minimal JPEG without ICC profile.
        /// </summary>
        private static byte[] BuildJpegWithoutIcc(int width, int height)
        {
            using var ms = new MemoryStream();
            ms.WriteByte(0xFF); ms.WriteByte(0xD8);

            // SOF0
            ms.WriteByte(0xFF); ms.WriteByte(0xC0);
            var sof = new byte[11];
            sof[0] = 0; sof[1] = 11;
            sof[2] = 8;
            sof[3] = (byte)(height >> 8); sof[4] = (byte)(height & 0xFF);
            sof[5] = (byte)(width >> 8); sof[6] = (byte)(width & 0xFF);
            sof[7] = 3;
            sof[8] = 1; sof[9] = 0x11; sof[10] = 0;
            ms.Write(sof, 0, sof.Length);

            // SOS
            ms.WriteByte(0xFF); ms.WriteByte(0xDA);
            ms.WriteByte(0); ms.WriteByte(3);
            ms.WriteByte(0);

            // EOI
            ms.WriteByte(0xFF); ms.WriteByte(0xD9);

            return ms.ToArray();
        }

        private static void WriteChunk(Stream ms, string type, byte[] data)
        {
            var lenBytes = new byte[4];
            WriteUInt32BE(lenBytes, 0, (uint)data.Length);
            ms.Write(lenBytes, 0, 4);
            var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
            ms.Write(typeBytes, 0, 4);
            if (data.Length > 0) ms.Write(data, 0, data.Length);
            // CRC (simplified — just write 4 zero bytes since we don't validate CRC in parsing)
            ms.Write(new byte[4], 0, 4);
        }

        private static void WriteUInt32BE(byte[] buf, int offset, uint value)
        {
            buf[offset] = (byte)(value >> 24);
            buf[offset + 1] = (byte)(value >> 16);
            buf[offset + 2] = (byte)(value >> 8);
            buf[offset + 3] = (byte)value;
        }

        // ═══════════════════════════════════════════
        // Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void PngWithIcc_ContainsICCBased()
        {
            var iccProfile = BuildSyntheticIccProfile(3);
            var png = BuildPngWithIcc(2, 2, 2, iccProfile);
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddImage(png, ImageFormat.Png);
            doc.AddPage(PageSize.A4);
            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/ICCBased", text);
            Assert.Contains("/Alternate", text);
            Assert.Contains("/N", text);
        }

        [Fact]
        public void PngWithoutIcc_UsesDeviceRGB()
        {
            var png = BuildPngWithoutIcc(2, 2, 2);
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddImage(png, ImageFormat.Png);
            doc.AddPage(PageSize.A4);
            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/DeviceRGB", text);
            Assert.DoesNotContain("/ICCBased", text);
        }

        [Fact]
        public void JpegWithIcc_ContainsICCBased()
        {
            var iccProfile = BuildSyntheticIccProfile(3);
            var jpeg = BuildJpegWithIcc(2, 2, iccProfile);
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddImage(jpeg, ImageFormat.Jpeg);
            doc.AddPage(PageSize.A4);
            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/ICCBased", text);
        }

        [Fact]
        public void JpegWithoutIcc_UsesDeviceRGB()
        {
            var jpeg = BuildJpegWithoutIcc(2, 2);
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddImage(jpeg, ImageFormat.Jpeg);
            doc.AddPage(PageSize.A4);
            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/DeviceRGB", text);
            Assert.DoesNotContain("/ICCBased", text);
        }

        [Fact]
        public void GrayscalePngWithIcc_HasN1()
        {
            var iccProfile = BuildSyntheticIccProfile(1);
            var png = BuildPngWithIcc(2, 2, 0, iccProfile);
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddImage(png, ImageFormat.Png);
            doc.AddPage(PageSize.A4);
            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/N 1", text);
        }

        [Fact]
        public void PngWithIcc_CompressionStillValid()
        {
            var iccProfile = BuildSyntheticIccProfile(3);
            var png = BuildPngWithIcc(2, 2, 2, iccProfile);
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.Flate });
            var image = doc.AddImage(png, ImageFormat.Png);
            doc.AddPage(PageSize.A4);
            // Should not throw
            var bytes = doc.ToArray();
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void PngWithIcc_DimensionsCorrect()
        {
            var iccProfile = BuildSyntheticIccProfile(3);
            var png = BuildPngWithIcc(4, 3, 2, iccProfile);
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var image = doc.AddImage(png, ImageFormat.Png);
            Assert.Equal(4, image.Width);
            Assert.Equal(3, image.Height);
        }
    }
}
