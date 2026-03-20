using System;
using System.IO;
using System.IO.Compression;
using Rend.Pdf.Parsing;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class StreamDecoderTests
    {
        [Fact]
        public void AsciiHexDecode_DecodesHexPairs()
        {
            byte[] input = System.Text.Encoding.ASCII.GetBytes("48656C6C6F>");
            byte[] result = StreamDecoder.AsciiHexDecode(input);
            Assert.Equal("Hello", System.Text.Encoding.ASCII.GetString(result));
        }

        [Fact]
        public void AsciiHexDecode_HandlesWhitespace()
        {
            byte[] input = System.Text.Encoding.ASCII.GetBytes("48 65 6C\n6C 6F>");
            byte[] result = StreamDecoder.AsciiHexDecode(input);
            Assert.Equal("Hello", System.Text.Encoding.ASCII.GetString(result));
        }

        [Fact]
        public void AsciiHexDecode_OddNibblePadsWithZero()
        {
            byte[] input = System.Text.Encoding.ASCII.GetBytes("4>");
            byte[] result = StreamDecoder.AsciiHexDecode(input);
            Assert.Single(result);
            Assert.Equal(0x40, result[0]);
        }

        [Fact]
        public void Ascii85Decode_DecodesKnownValue()
        {
            // "Man " = 0x4D616E20 encodes to "9jqo^" in ASCII85
            byte[] input = System.Text.Encoding.ASCII.GetBytes("9jqo^~>");
            byte[] result = StreamDecoder.Ascii85Decode(input);
            Assert.Equal("Man ", System.Text.Encoding.ASCII.GetString(result));
        }

        [Fact]
        public void Ascii85Decode_ZShortcutProducesFourZeroBytes()
        {
            byte[] input = System.Text.Encoding.ASCII.GetBytes("z~>");
            byte[] result = StreamDecoder.Ascii85Decode(input);
            Assert.Equal(4, result.Length);
            Assert.All(result, b => Assert.Equal(0, b));
        }

        [Fact]
        public void FlateDecode_DecompressesZlibData()
        {
            // Compress "Hello, World!" with zlib
            byte[] original = System.Text.Encoding.ASCII.GetBytes("Hello, World!");
            byte[] compressed;
            using (var output = new MemoryStream())
            {
                // Write zlib header (CMF=0x78, FLG=0x01)
                output.WriteByte(0x78);
                output.WriteByte(0x01);
                using (var deflate = new DeflateStream(output, CompressionLevel.Fastest, leaveOpen: true))
                {
                    deflate.Write(original, 0, original.Length);
                }
                compressed = output.ToArray();
            }

            byte[] result = StreamDecoder.FlateDecode(compressed, PdfObj.Null);
            Assert.Equal("Hello, World!", System.Text.Encoding.ASCII.GetString(result));
        }

        [Fact]
        public void FlateDecode_HandlesRawDeflateWithoutZlibHeader()
        {
            byte[] original = System.Text.Encoding.ASCII.GetBytes("Test data");
            byte[] compressed;
            using (var output = new MemoryStream())
            {
                using (var deflate = new DeflateStream(output, CompressionLevel.Fastest, leaveOpen: true))
                {
                    deflate.Write(original, 0, original.Length);
                }
                compressed = output.ToArray();
            }

            byte[] result = StreamDecoder.FlateDecode(compressed, PdfObj.Null);
            Assert.Equal("Test data", System.Text.Encoding.ASCII.GetString(result));
        }

        [Fact]
        public void PngPredictor_SubFilter_AddsLeftNeighbor()
        {
            // PNG Sub filter (type 1): each byte = raw + left neighbor
            // Row: [filter=1, 10, 20, 30]
            // Result: [10, 30, 60] (10, 10+20, 30+30)
            byte[] data = { 1, 10, 20, 30 };
            byte[] result = StreamDecoder.ApplyPngPredictor(data, columns: 3, colors: 1, bitsPerComponent: 8);
            Assert.Equal(3, result.Length);
            Assert.Equal(10, result[0]);
            Assert.Equal(30, result[1]);
            Assert.Equal(60, result[2]);
        }

        [Fact]
        public void PngPredictor_NoneFilter_PassesThrough()
        {
            byte[] data = { 0, 10, 20, 30 };
            byte[] result = StreamDecoder.ApplyPngPredictor(data, columns: 3, colors: 1, bitsPerComponent: 8);
            Assert.Equal(new byte[] { 10, 20, 30 }, result);
        }

        [Fact]
        public void PngPredictor_UpFilter_AddsPreviousRow()
        {
            // Two rows: [filter=0, 10, 20] then [filter=2, 5, 10]
            // Row 1: [10, 20], Row 2: [10+5, 20+10] = [15, 30]
            byte[] data = { 0, 10, 20, 2, 5, 10 };
            byte[] result = StreamDecoder.ApplyPngPredictor(data, columns: 2, colors: 1, bitsPerComponent: 8);
            Assert.Equal(4, result.Length);
            Assert.Equal(10, result[0]);
            Assert.Equal(20, result[1]);
            Assert.Equal(15, result[2]);
            Assert.Equal(30, result[3]);
        }

        [Fact]
        public void LzwDecode_DecodesBasicSequence()
        {
            // LZW-encode a simple repeating pattern and verify round-trip
            // This is a known LZW-compressed stream: clearcode(256), 'A'(65), 'B'(66), EOD(257)
            // Manual LZW encoding of "AB": 256(clear) 65(A) 66(B) 257(EOD)
            // At 9 bits: 256=100000000, 65=001000001, 66=001000010, 257=100000001
            var bits = new System.Collections.Generic.List<byte>();
            // Write bits MSB first, 9 bits per code
            WriteLzwBits(bits, 256, 9); // clear
            WriteLzwBits(bits, 65, 9);  // A
            WriteLzwBits(bits, 66, 9);  // B
            WriteLzwBits(bits, 257, 9); // EOD
            byte[] compressed = PackBits(bits);

            byte[] result = StreamDecoder.LzwDecode(compressed, PdfObj.Null);
            Assert.Equal("AB", System.Text.Encoding.ASCII.GetString(result));
        }

        private static void WriteLzwBits(System.Collections.Generic.List<byte> bits, int value, int count)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                bits.Add((byte)((value >> i) & 1));
            }
        }

        private static byte[] PackBits(System.Collections.Generic.List<byte> bits)
        {
            int byteCount = (bits.Count + 7) / 8;
            byte[] result = new byte[byteCount];
            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i] == 1)
                {
                    result[i / 8] |= (byte)(1 << (7 - (i % 8)));
                }
            }
            return result;
        }

        [Fact]
        public void ApplyFilter_UnknownFilter_ReturnsDataUnchanged()
        {
            byte[] data = { 1, 2, 3 };
            byte[] result = StreamDecoder.ApplyFilter("UnknownFilter", data, PdfObj.Null);
            Assert.Equal(data, result);
        }
    }
}
