using System;
using System.IO;
using System.Text;
using Rend.Pdf;
using Rend.Pdf.Fonts;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class Type1FontTests
    {
        // ═══════════════════════════════════════════
        // Format Detection Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void IsPfb_DetectsValidPfbHeader()
        {
            // PFB files start with 0x80 0x01
            byte[] data = new byte[] { 0x80, 0x01, 0x00, 0x00, 0x00, 0x00 };
            Assert.True(Type1FontParser.IsPfb(data));
        }

        [Fact]
        public void IsPfb_RejectsNonPfbData()
        {
            byte[] data = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 };
            Assert.False(Type1FontParser.IsPfb(data));
        }

        [Fact]
        public void IsPfb_RejectsNullData()
        {
            Assert.False(Type1FontParser.IsPfb(null!));
        }

        [Fact]
        public void IsPfb_RejectsShortData()
        {
            Assert.False(Type1FontParser.IsPfb(new byte[] { 0x80 }));
        }

        [Fact]
        public void IsPfa_DetectsAdobeFontHeader()
        {
            byte[] data = Encoding.ASCII.GetBytes("%!PS-AdobeFont-1.0: TestFont 001.000\n");
            Assert.True(Type1FontParser.IsPfa(data));
        }

        [Fact]
        public void IsPfa_DetectsFontType1Header()
        {
            byte[] data = Encoding.ASCII.GetBytes("%!FontType1-1.0: TestFont 001.000\n");
            Assert.True(Type1FontParser.IsPfa(data));
        }

        [Fact]
        public void IsPfa_RejectsNonPfaData()
        {
            byte[] data = Encoding.ASCII.GetBytes("This is not a font file\n");
            Assert.False(Type1FontParser.IsPfa(data));
        }

        [Fact]
        public void IsType1Font_DetectsPfb()
        {
            byte[] data = BuildMinimalPfb();
            Assert.True(Type1FontParser.IsType1Font(data));
        }

        [Fact]
        public void IsType1Font_DetectsPfa()
        {
            byte[] data = BuildMinimalPfa();
            Assert.True(Type1FontParser.IsType1Font(data));
        }

        [Fact]
        public void IsType1Font_RejectsTrueType()
        {
            // TrueType starts with 0x00 0x01 0x00 0x00
            byte[] data = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00 };
            Assert.False(Type1FontParser.IsType1Font(data));
        }

        [Fact]
        public void IsType1Font_RejectsOpenType()
        {
            // OpenType CFF starts with 'OTTO'
            byte[] data = Encoding.ASCII.GetBytes("OTTO\x00\x06\x00\x00");
            Assert.False(Type1FontParser.IsType1Font(data));
        }

        // ═══════════════════════════════════════════
        // PFB Parsing Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void Parse_PfbFormat_ExtractsFontName()
        {
            byte[] pfb = BuildMinimalPfb();
            var parsed = Type1FontParser.Parse(pfb);
            Assert.Equal("TestType1", parsed.FontName);
        }

        [Fact]
        public void Parse_PfbFormat_ExtractsFontBBox()
        {
            byte[] pfb = BuildMinimalPfb();
            var parsed = Type1FontParser.Parse(pfb);
            Assert.Equal(4, parsed.FontBBox.Length);
            Assert.Equal(-100f, parsed.FontBBox[0]);
            Assert.Equal(-200f, parsed.FontBBox[1]);
            Assert.Equal(900f, parsed.FontBBox[2]);
            Assert.Equal(800f, parsed.FontBBox[3]);
        }

        [Fact]
        public void Parse_PfbFormat_ExtractsItalicAngle()
        {
            byte[] pfb = BuildMinimalPfb();
            var parsed = Type1FontParser.Parse(pfb);
            Assert.Equal(-12f, parsed.ItalicAngle);
        }

        [Fact]
        public void Parse_PfbFormat_ExtractsIsFixedPitch()
        {
            byte[] pfb = BuildMinimalPfb();
            var parsed = Type1FontParser.Parse(pfb);
            Assert.False(parsed.IsFixedPitch);
        }

        [Fact]
        public void Parse_PfbFormat_SeparatesSegments()
        {
            byte[] pfb = BuildMinimalPfb();
            var parsed = Type1FontParser.Parse(pfb);

            // Header segment should contain the ASCII header
            Assert.True(parsed.HeaderSegment.Length > 0);
            string header = Encoding.ASCII.GetString(parsed.HeaderSegment);
            Assert.Contains("/FontName", header);

            // Encrypted segment should be non-empty
            Assert.True(parsed.EncryptedSegment.Length > 0);

            // Trailer segment should be present (may be empty for minimal fonts)
            Assert.NotNull(parsed.TrailerSegment);
        }

        [Fact]
        public void Parse_PfbFormat_HeaderContainsFontMetadata()
        {
            byte[] pfb = BuildMinimalPfb();
            var parsed = Type1FontParser.Parse(pfb);
            string header = Encoding.ASCII.GetString(parsed.HeaderSegment);

            Assert.Contains("/FontName /TestType1", header);
            Assert.Contains("/FontBBox", header);
            Assert.Contains("/ItalicAngle", header);
        }

        // ═══════════════════════════════════════════
        // PFA Parsing Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void Parse_PfaFormat_ExtractsFontName()
        {
            byte[] pfa = BuildMinimalPfa();
            var parsed = Type1FontParser.Parse(pfa);
            Assert.Equal("TestPFA", parsed.FontName);
        }

        [Fact]
        public void Parse_PfaFormat_ExtractsFontBBox()
        {
            byte[] pfa = BuildMinimalPfa();
            var parsed = Type1FontParser.Parse(pfa);
            Assert.Equal(4, parsed.FontBBox.Length);
            Assert.Equal(0f, parsed.FontBBox[0]);
            Assert.Equal(-250f, parsed.FontBBox[1]);
            Assert.Equal(1000f, parsed.FontBBox[2]);
            Assert.Equal(750f, parsed.FontBBox[3]);
        }

        // ═══════════════════════════════════════════
        // PdfFont Creation Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void ToPdfFont_CreatesValidPdfFont()
        {
            byte[] pfb = BuildMinimalPfb();
            var parsed = Type1FontParser.Parse(pfb);
            var font = parsed.ToPdfFont(0);

            Assert.NotNull(font);
            Assert.Equal("TestType1", font.BaseFont);
            Assert.False(font.IsStandard14);
        }

        [Fact]
        public void ToPdfFont_HasCorrectMetrics()
        {
            byte[] pfb = BuildMinimalPfb();
            var parsed = Type1FontParser.Parse(pfb);
            var font = parsed.ToPdfFont(0);

            Assert.Equal(1000f, font.Metrics.UnitsPerEm);
            Assert.True(font.Metrics.Ascent > 0);
            Assert.True(font.Metrics.Descent < 0);
            Assert.Equal(-12f, font.Metrics.ItalicAngle);
        }

        [Fact]
        public void ToPdfFont_HasCharWidths()
        {
            byte[] pfb = BuildMinimalPfb();
            var parsed = Type1FontParser.Parse(pfb);
            var font = parsed.ToPdfFont(0);

            // Space character (code 32) should have a non-zero width
            float spaceWidth = font.GetAdvanceWidth(32);
            Assert.True(spaceWidth > 0);
        }

        // ═══════════════════════════════════════════
        // PDF Document Integration Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void AddFont_AcceptsType1Font()
        {
            byte[] pfb = BuildMinimalPfb();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(pfb);
            var font = doc.AddFont(stream);

            Assert.NotNull(font);
            Assert.Equal("TestType1", font.BaseFont);
            Assert.False(font.IsStandard14);
        }

        [Fact]
        public void Type1Font_ProducesValidPdf()
        {
            byte[] pfb = BuildMinimalPfb();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(pfb);
            var font = doc.AddFont(stream);

            var page = doc.AddPage(612, 792);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            Assert.True(pdfBytes.Length > 0);
        }

        [Fact]
        public void Type1Font_HasSubtypeType1()
        {
            byte[] pfb = BuildMinimalPfb();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(pfb);
            var font = doc.AddFont(stream);

            var page = doc.AddPage(612, 792);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // Type 1 embedded font should have /Subtype /Type1 (not /Type0)
            Assert.Contains("/Subtype /Type1", pdfText);
            // Should NOT be wrapped in a Type0 composite font
            Assert.DoesNotContain("/CIDFontType", pdfText);
        }

        [Fact]
        public void Type1Font_HasFontFileNotFontFile2()
        {
            byte[] pfb = BuildMinimalPfb();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(pfb);
            var font = doc.AddFont(stream);

            var page = doc.AddPage(612, 792);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // Type 1 fonts use /FontFile, not /FontFile2 or /FontFile3
            Assert.Contains("/FontFile ", pdfText);
            Assert.DoesNotContain("/FontFile2", pdfText);
            Assert.DoesNotContain("/FontFile3", pdfText);
        }

        [Fact]
        public void Type1Font_HasLength1Length2Length3()
        {
            byte[] pfb = BuildMinimalPfb();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(pfb);
            var font = doc.AddFont(stream);

            var page = doc.AddPage(612, 792);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // Type 1 font stream must have /Length1, /Length2, /Length3
            Assert.Contains("/Length1 ", pdfText);
            Assert.Contains("/Length2 ", pdfText);
            Assert.Contains("/Length3 ", pdfText);
        }

        [Fact]
        public void Type1Font_HasWinAnsiEncoding()
        {
            byte[] pfb = BuildMinimalPfb();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(pfb);
            var font = doc.AddFont(stream);

            var page = doc.AddPage(612, 792);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            Assert.Contains("/Encoding /WinAnsiEncoding", pdfText);
        }

        [Fact]
        public void Type1Font_HasWidthArray()
        {
            byte[] pfb = BuildMinimalPfb();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(pfb);
            var font = doc.AddFont(stream);

            var page = doc.AddPage(612, 792);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // Should have /FirstChar, /LastChar, /Widths
            Assert.Contains("/FirstChar 32", pdfText);
            Assert.Contains("/LastChar 255", pdfText);
            Assert.Contains("/Widths", pdfText);
        }

        [Fact]
        public void Type1Font_HasFontDescriptor()
        {
            byte[] pfb = BuildMinimalPfb();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(pfb);
            var font = doc.AddFont(stream);

            var page = doc.AddPage(612, 792);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            Assert.Contains("/FontDescriptor", pdfText);
            Assert.Contains("/FontName /TestType1", pdfText);
            Assert.Contains("/ItalicAngle", pdfText);
            Assert.Contains("/Ascent", pdfText);
            Assert.Contains("/Descent", pdfText);
            Assert.Contains("/CapHeight", pdfText);
            Assert.Contains("/StemV", pdfText);
        }

        [Fact]
        public void Type1Font_CanMeasureWidth()
        {
            byte[] pfb = BuildMinimalPfb();
            var parsed = Type1FontParser.Parse(pfb);
            var font = parsed.ToPdfFont(0);

            // MeasureWidth should return a positive value for any non-empty text
            float width = font.MeasureWidth("Hello", 12);
            Assert.True(width > 0);
        }

        // ═══════════════════════════════════════════
        // Synthetic Font Builders
        // ═══════════════════════════════════════════

        /// <summary>
        /// Build a minimal synthetic PFB file with correct segment structure.
        /// </summary>
        private static byte[] BuildMinimalPfb()
        {
            // ASCII header segment
            string header =
                "%!PS-AdobeFont-1.0: TestType1 001.000\n" +
                "12 dict begin\n" +
                "/FontInfo 9 dict dup begin\n" +
                " /version (001.000) readonly def\n" +
                " /FullName (Test Type1) readonly def\n" +
                " /FamilyName (TestType1) readonly def\n" +
                " /Weight (Medium) readonly def\n" +
                " /ItalicAngle -12 def\n" +
                " /isFixedPitch false def\n" +
                " /UnderlinePosition -100 def\n" +
                " /UnderlineThickness 50 def\n" +
                "end readonly def\n" +
                "/FontName /TestType1 def\n" +
                "/FontType 1 def\n" +
                "/FontMatrix [0.001 0 0 0.001 0 0] readonly def\n" +
                "/Encoding StandardEncoding def\n" +
                "/FontBBox {-100 -200 900 800} readonly def\n" +
                "currentdict end\n" +
                "currentfile eexec\n";
            byte[] headerBytes = Encoding.ASCII.GetBytes(header);

            // Binary encrypted segment (minimal dummy eexec data)
            // In a real font, this would be the eexec-encrypted Private dict and CharStrings.
            // For testing, we provide a minimal encrypted block.
            byte[] encryptedBytes = BuildMinimalEexecData();

            // ASCII trailer segment
            string trailer = "0000000000000000000000000000000000000000000000000000000000000000\n" +
                             "0000000000000000000000000000000000000000000000000000000000000000\n" +
                             "0000000000000000000000000000000000000000000000000000000000000000\n" +
                             "0000000000000000000000000000000000000000000000000000000000000000\n" +
                             "0000000000000000000000000000000000000000000000000000000000000000\n" +
                             "0000000000000000000000000000000000000000000000000000000000000000\n" +
                             "0000000000000000000000000000000000000000000000000000000000000000\n" +
                             "0000000000000000000000000000000000000000000000000000000000000000\n" +
                             "cleartomark\n";
            byte[] trailerBytes = Encoding.ASCII.GetBytes(trailer);

            // Assemble PFB with segment markers
            using var ms = new MemoryStream();

            // Segment 1: ASCII header
            ms.WriteByte(0x80);
            ms.WriteByte(0x01); // type 1 = ASCII
            WriteLE32(ms, headerBytes.Length);
            ms.Write(headerBytes, 0, headerBytes.Length);

            // Segment 2: Binary encrypted
            ms.WriteByte(0x80);
            ms.WriteByte(0x02); // type 2 = binary
            WriteLE32(ms, encryptedBytes.Length);
            ms.Write(encryptedBytes, 0, encryptedBytes.Length);

            // Segment 3: ASCII trailer
            ms.WriteByte(0x80);
            ms.WriteByte(0x01); // type 1 = ASCII
            WriteLE32(ms, trailerBytes.Length);
            ms.Write(trailerBytes, 0, trailerBytes.Length);

            // EOF marker
            ms.WriteByte(0x80);
            ms.WriteByte(0x03); // type 3 = EOF

            return ms.ToArray();
        }

        /// <summary>
        /// Build a minimal synthetic PFA (ASCII) font file.
        /// </summary>
        private static byte[] BuildMinimalPfa()
        {
            string pfa =
                "%!PS-AdobeFont-1.0: TestPFA 001.000\n" +
                "12 dict begin\n" +
                "/FontInfo 3 dict dup begin\n" +
                " /ItalicAngle 0 def\n" +
                " /isFixedPitch true def\n" +
                "end readonly def\n" +
                "/FontName /TestPFA def\n" +
                "/FontType 1 def\n" +
                "/FontMatrix [0.001 0 0 0.001 0 0] readonly def\n" +
                "/Encoding StandardEncoding def\n" +
                "/FontBBox {0 -250 1000 750} readonly def\n" +
                "currentdict end\n" +
                "currentfile eexec\n" +
                "AABBCCDD00112233445566778899AABBCCDD00112233445566778899AABBCCDD\n" +
                "0000000000000000000000000000000000000000000000000000000000000000\n" +
                "cleartomark\n";
            return Encoding.ASCII.GetBytes(pfa);
        }

        /// <summary>
        /// Build minimal eexec-encrypted data that encodes a tiny Private dict and CharStrings.
        /// </summary>
        private static byte[] BuildMinimalEexecData()
        {
            // The plaintext for eexec encryption:
            // 4 random bytes + Private dict + CharStrings
            string plainText =
                "dup /Private 5 dict dup begin\n" +
                "/RD {string currentfile exch readstring pop} executeonly def\n" +
                "/ND {noaccess def} executeonly def\n" +
                "/NP {noaccess put} executeonly def\n" +
                "/MinFeature {16 16} ND\n" +
                "/lenIV 4 def\n" +
                "/CharStrings 1 dict dup begin\n" +
                "/.notdef 1 RD X ND\n" +
                "end\n" +
                "end\n";

            byte[] plainBytes = new byte[4 + plainText.Length];
            // 4 random prefix bytes
            plainBytes[0] = 0xAA;
            plainBytes[1] = 0xBB;
            plainBytes[2] = 0xCC;
            plainBytes[3] = 0xDD;
            Encoding.ASCII.GetBytes(plainText, 0, plainText.Length, plainBytes, 4);

            // Encrypt with eexec cipher
            ushort key = 55665;
            const ushort c1 = 52845;
            const ushort c2 = 22719;

            byte[] encrypted = new byte[plainBytes.Length];
            for (int i = 0; i < plainBytes.Length; i++)
            {
                byte plain = plainBytes[i];
                byte cipher = (byte)(plain ^ (key >> 8));
                encrypted[i] = cipher;
                key = (ushort)((cipher + key) * c1 + c2);
            }

            return encrypted;
        }

        private static void WriteLE32(MemoryStream ms, int value)
        {
            ms.WriteByte((byte)(value & 0xFF));
            ms.WriteByte((byte)((value >> 8) & 0xFF));
            ms.WriteByte((byte)((value >> 16) & 0xFF));
            ms.WriteByte((byte)((value >> 24) & 0xFF));
        }
    }
}
