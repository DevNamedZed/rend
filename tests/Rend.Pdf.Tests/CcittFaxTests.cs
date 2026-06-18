using System.IO;
using Rend.Pdf.Images;
using Rend.Pdf.Parsing;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class CcittFaxTests
    {
        [Theory]
        [InlineData(240, 240, "diamond")]
        [InlineData(200, 100, "leftHalf")]
        [InlineData(128, 64, "diagonal")]
        public void AddCcittImage_RoundTripsThroughDocumentReader(int columns, int rows, string pattern)
        {
            bool[,] image = BuildPattern(columns, rows, pattern);
            byte[] packed = Pack(image, columns, rows);

            byte[] pdfBytes;
            using (var document = new PdfDocument())
            {
                PdfImage ccittImage = document.AddCcittImage(packed, columns, rows, blackIs1: false);
                PdfPage page = document.AddPage(columns, rows);
                page.Content.DrawImage(ccittImage, columns, 0f, 0f, rows, 0f, 0f);
                using var buffer = new MemoryStream();
                document.Save(buffer);
                pdfBytes = buffer.ToArray();
            }

            using var reader = PdfDocumentReader.Open(pdfBytes);
            PdfObj imageObject = FindFirstImageXObject(reader);
            Assert.False(imageObject.IsNull, "no image XObject found in written PDF");

            byte[] decoded = reader.GetStreamBytes(imageObject);

            Assert.Equal(packed.Length, decoded.Length);
            for (int i = 0; i < packed.Length; i++)
            {
                Assert.True(packed[i] == decoded[i],
                    $"byte {i} differs for {columns}x{rows} '{pattern}': expected {packed[i]:X2}, got {decoded[i]:X2}");
            }
        }

        private static PdfObj FindFirstImageXObject(PdfDocumentReader reader)
        {
            PdfObj page = reader.GetPage(0);
            PdfObj resources = reader.Resolve(page["Resources"]);
            PdfObj xObjects = reader.Resolve(resources["XObject"]);
            foreach (string name in xObjects.Keys)
            {
                PdfObj candidate = reader.Resolve(xObjects[name]);
                if (candidate.IsStream && reader.Resolve(candidate["Subtype"]).AsName() == "Image")
                {
                    return candidate;
                }
            }
            return PdfObj.Null;
        }

        [Theory]
        [InlineData(16, 4, "white")]
        [InlineData(16, 4, "black")]
        [InlineData(16, 4, "leftHalf")]
        [InlineData(8, 8, "diagonal")]
        [InlineData(17, 5, "vstripes")]
        [InlineData(100, 20, "diagonal")]
        [InlineData(1, 1, "black")]
        [InlineData(240, 4, "black")]      // first row: black run 240 → make-up codes
        [InlineData(200, 6, "leftHalf")]   // black run 100 → make-up 64 + terminating 36
        [InlineData(320, 4, "leftHalf")]   // black run 160 → make-up 128 + terminating 32
        [InlineData(240, 240, "diamond")]  // the BasicPdf sample pattern (border + diamond)
        public void EncodeG4_RoundTrips(int columns, int rows, string pattern)
        {
            bool[,] image = BuildPattern(columns, rows, pattern);
            byte[] packed = Pack(image, columns, rows);

            byte[] encoded = CcittFaxCodec.EncodeG4(packed, columns, rows, blackIs1: false);
            byte[] decoded = CcittFaxCodec.DecodeG4(encoded, columns, rows, blackIs1: false);

            Assert.Equal(packed.Length, decoded.Length);
            for (int i = 0; i < packed.Length; i++)
            {
                Assert.True(packed[i] == decoded[i],
                    $"byte {i} differs for {columns}x{rows} '{pattern}': expected {packed[i]:X2}, got {decoded[i]:X2}");
            }
        }

        [Theory]
        [InlineData(2_000_000, 10)]            // columns beyond the cap
        [InlineData(1000, 2_000_000)]          // rows beyond the cap
        [InlineData(100_000, 100_000)]         // product exceeds the output-byte cap
        [InlineData(int.MaxValue - 3, 1)]      // (columns+7)/8 would overflow
        [InlineData(0, 10)]                    // non-positive columns
        public void DecodeG4_RejectsOutOfRangeDimensions(int columns, int rows)
        {
            // Untrusted /Columns and /Rows must not be able to overflow the buffer math or exhaust
            // memory — the codec rejects them so the reader can degrade instead of crashing.
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                CcittFaxCodec.DecodeG4(System.Array.Empty<byte>(), columns, rows, blackIs1: false));
        }

        private static bool[,] BuildPattern(int columns, int rows, string pattern)
        {
            var image = new bool[rows, columns]; // true = black
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    image[y, x] = pattern switch
                    {
                        "white" => false,
                        "black" => true,
                        "leftHalf" => x < columns / 2,
                        "diagonal" => ((x + y) & 1) == 0,
                        "vstripes" => (x % 3) == 0,
                        "diamond" => x < 6 || x >= columns - 6 || y < 6 || y >= rows - 6
                                     || System.Math.Abs(x - columns / 2) + System.Math.Abs(y - rows / 2) < 90,
                        _ => false,
                    };
                }
            }
            return image;
        }

        private static byte[] Pack(bool[,] image, int columns, int rows)
        {
            int rowBytes = (columns + 7) / 8;
            var packed = new byte[rowBytes * rows];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    // blackIs1 = false: black pixel → sample bit 0, white → 1.
                    if (!image[y, x])
                    {
                        packed[y * rowBytes + (x >> 3)] |= (byte)(1 << (7 - (x & 7)));
                    }
                }
            }
            return packed;
        }
    }
}
