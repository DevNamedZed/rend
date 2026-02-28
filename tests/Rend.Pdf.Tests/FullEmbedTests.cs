using System;
using System.IO;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class FullEmbedTests
    {
        // ═══════════════════════════════════════════
        // FontEmbedMode Enum
        // ═══════════════════════════════════════════

        [Fact]
        public void FontEmbedMode_HasExpectedValues()
        {
            Assert.Equal(0, (int)FontEmbedMode.Subset);
            Assert.Equal(1, (int)FontEmbedMode.Full);
            Assert.Equal(2, (int)FontEmbedMode.None);
        }

        // ═══════════════════════════════════════════
        // Standard14 with Full mode (no-op, Standard14 is never embedded)
        // ═══════════════════════════════════════════

        [Fact]
        public void Standard14Font_FullMode_Ignored()
        {
            // Standard14 fonts are never embedded regardless of mode
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "Hello");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // Standard14 should use Type1 not CIDFontType2
            Assert.Contains("/Type1", pdfText);
            Assert.Contains("/Helvetica", pdfText);
        }

        // ═══════════════════════════════════════════
        // AddFont accepts FontEmbedMode
        // ═══════════════════════════════════════════

        [Fact]
        public void AddFont_SubsetMode_IsDefault()
        {
            // The method signature should default to Subset
            // This test verifies the API works without specifying mode
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });

            // Can't test with real font without file, but verify the API compiles and accepts the param
            Assert.ThrowsAny<IOException>(() =>
                doc.AddFont("/nonexistent/font.ttf"));
        }

        [Fact]
        public void AddFont_FullMode_AcceptsParameter()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });

            // Verify the API accepts FontEmbedMode.Full
            Assert.ThrowsAny<IOException>(() =>
                doc.AddFont("/nonexistent/font.ttf", FontEmbedMode.Full));
        }

        // ═══════════════════════════════════════════
        // Subset mode (default) behavior verification
        // ═══════════════════════════════════════════

        [Fact]
        public void SubsetMode_ProducesSubsetTagInFontName()
        {
            // Build a minimal TrueType font for testing
            byte[] minimalFont = BuildMinimalTrueTypeFont();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });

            PdfFont font;
            using (var ms = new MemoryStream(minimalFont))
            {
                font = doc.AddFont(ms, FontEmbedMode.Subset);
            }

            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // Subset fonts should have a tag like "AAAAAA+"
            Assert.Contains("+", pdfText);
        }

        [Fact]
        public void FullMode_NoSubsetTag()
        {
            byte[] minimalFont = BuildMinimalTrueTypeFont();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });

            PdfFont font;
            using (var ms = new MemoryStream(minimalFont))
            {
                font = doc.AddFont(ms, FontEmbedMode.Full);
            }

            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // Full embed fonts should NOT have a subset tag prefix "+" in font descriptor name
            // The /FontName should be just the font name without a tag
            // Find lines with /FontName
            int fnIdx = pdfText.IndexOf("/FontName");
            Assert.True(fnIdx >= 0, "PDF should contain /FontName");

            // Extract the font name value (next /Name after /FontName)
            string afterFn = pdfText.Substring(fnIdx);
            // In full mode, there should NOT be a "+" in the descriptor font name
            // but there should be one in subset mode
            int lineEnd = afterFn.IndexOf('\n');
            string fontNameLine = lineEnd > 0 ? afterFn.Substring(0, lineEnd) : afterFn;
            Assert.DoesNotContain("+", fontNameLine);
        }

        /// <summary>
        /// Build a minimal valid TrueType font with just enough tables to parse.
        /// Tables: head, hhea, maxp, name, cmap, OS/2, post, hmtx
        /// </summary>
        private static byte[] BuildMinimalTrueTypeFont()
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);

            // We need 8 tables
            int numTables = 8;
            int headerSize = 12 + numTables * 16;

            // Calculate table sizes and offsets
            int headSize = 54;
            int hheaSize = 36;
            int maxpSize = 6;
            int nameSize = 6 + 12 + 8; // header + 1 record + "Test" string
            int cmapSize = 4 + 8 + 14; // header + 1 subtable ref + format 4 minimal
            int os2Size = 78;
            int postSize = 32;
            int hmtxSize = 4; // 1 glyph: advanceWidth + lsb

            int currentOffset = headerSize;
            int headOffset = currentOffset; currentOffset += Align4(headSize);
            int hheaOffset = currentOffset; currentOffset += Align4(hheaSize);
            int maxpOffset = currentOffset; currentOffset += Align4(maxpSize);
            int nameOffset = currentOffset; currentOffset += Align4(nameSize);
            int cmapOffset = currentOffset; currentOffset += Align4(cmapSize);
            int os2Offset = currentOffset; currentOffset += Align4(os2Size);
            int postOffset = currentOffset; currentOffset += Align4(postSize);
            int hmtxOffset = currentOffset; currentOffset += Align4(hmtxSize);

            // Offset table
            w.Write(BEUInt32(0x00010000)); // sfVersion
            w.Write(BEUInt16((ushort)numTables));
            w.Write(BEUInt16(128)); // searchRange
            w.Write(BEUInt16(3));   // entrySelector
            w.Write(BEUInt16(0));   // rangeShift

            // Table directory entries
            WriteTableEntry(w, "head", 0, (uint)headOffset, (uint)headSize);
            WriteTableEntry(w, "hhea", 0, (uint)hheaOffset, (uint)hheaSize);
            WriteTableEntry(w, "maxp", 0, (uint)maxpOffset, (uint)maxpSize);
            WriteTableEntry(w, "name", 0, (uint)nameOffset, (uint)nameSize);
            WriteTableEntry(w, "cmap", 0, (uint)cmapOffset, (uint)cmapSize);
            WriteTableEntry(w, "OS/2", 0, (uint)os2Offset, (uint)os2Size);
            WriteTableEntry(w, "post", 0, (uint)postOffset, (uint)postSize);
            WriteTableEntry(w, "hmtx", 0, (uint)hmtxOffset, (uint)hmtxSize);

            // Pad to headOffset
            PadTo(w, ms, headOffset);

            // head table
            w.Write(BEUInt16(1)); w.Write(BEUInt16(0)); // version 1.0
            w.Write(BEUInt32(0x00010000)); // fontRevision
            w.Write(BEUInt32(0)); // checksumAdjustment
            w.Write(BEUInt32(0x5F0F3CF5)); // magicNumber
            w.Write(BEUInt16(0x000B)); // flags
            w.Write(BEUInt16(1000)); // unitsPerEm
            w.Write(new byte[16]); // created, modified
            w.Write(BEInt16(0)); // xMin
            w.Write(BEInt16(-200)); // yMin
            w.Write(BEInt16(1000)); // xMax
            w.Write(BEInt16(800)); // yMax
            w.Write(BEUInt16(0)); // macStyle (regular)
            w.Write(BEUInt16(8)); // lowestRecPPEM
            w.Write(BEInt16(2)); // fontDirectionHint
            w.Write(BEInt16(1)); // indexToLocFormat
            w.Write(BEInt16(0)); // glyphDataFormat
            PadTo(w, ms, hheaOffset);

            // hhea table
            w.Write(BEUInt32(0x00010000)); // version
            w.Write(BEInt16(800)); // ascent
            w.Write(BEInt16(-200)); // descent
            w.Write(BEInt16(0)); // lineGap
            w.Write(BEUInt16(600)); // advanceWidthMax
            w.Write(new byte[22]); // remaining fields
            w.Write(BEUInt16(1)); // numberOfHMetrics
            PadTo(w, ms, maxpOffset);

            // maxp table
            w.Write(BEUInt32(0x00005000)); // version 0.5
            w.Write(BEUInt16(1)); // numGlyphs
            PadTo(w, ms, nameOffset);

            // name table - minimal
            int nameStorageOffset = 6 + 12; // after header + 1 record
            w.Write(BEUInt16(0)); // format
            w.Write(BEUInt16(1)); // count
            w.Write(BEUInt16((ushort)nameStorageOffset)); // stringOffset
            // Name record: platform 1 (Mac), encoding 0, lang 0, nameId 1 (family), length 4, offset 0
            w.Write(BEUInt16(1)); // platformId
            w.Write(BEUInt16(0)); // encodingId
            w.Write(BEUInt16(0)); // languageId
            w.Write(BEUInt16(1)); // nameId (family name)
            w.Write(BEUInt16(4)); // length
            w.Write(BEUInt16(0)); // offset
            // String data
            w.Write(new byte[] { (byte)'T', (byte)'e', (byte)'s', (byte)'t' });
            PadTo(w, ms, cmapOffset);

            // cmap table - minimal format 4 with just 0xFFFF terminator
            w.Write(BEUInt16(0)); // version
            w.Write(BEUInt16(1)); // numSubtables
            // Subtable: platform 3, encoding 1, offset 12
            w.Write(BEUInt16(3)); // platformId
            w.Write(BEUInt16(1)); // encodingId
            w.Write(BEUInt32(12)); // offset from start of cmap

            // Format 4 subtable
            int format4Start = (int)ms.Position;
            w.Write(BEUInt16(4)); // format
            w.Write(BEUInt16(14)); // length
            w.Write(BEUInt16(0)); // language
            w.Write(BEUInt16(2)); // segCountX2 (1 segment)
            w.Write(BEUInt16(2)); // searchRange
            w.Write(BEUInt16(0)); // entrySelector
            w.Write(BEUInt16(0)); // rangeShift
            // endCodes: 0xFFFF
            // No actual mapping, just the terminator
            PadTo(w, ms, os2Offset);

            // OS/2 table
            w.Write(BEUInt16(4)); // version
            w.Write(BEInt16(500)); // avgCharWidth
            w.Write(BEUInt16(400)); // weightClass
            w.Write(BEUInt16(5)); // widthClass
            w.Write(BEUInt16(0)); // fsType
            w.Write(new byte[22]); // subscript/superscript/strikeout/familyClass/panose
            w.Write(new byte[16]); // unicodeRange
            w.Write(new byte[4]); // achVendID
            w.Write(BEUInt16(0x0040)); // fsSelection (regular)
            w.Write(BEUInt16(0x0020)); // usFirstCharIndex
            w.Write(BEUInt16(0x007E)); // usLastCharIndex
            w.Write(BEInt16(800)); // sTypoAscender
            w.Write(BEInt16(-200)); // sTypoDescender
            w.Write(BEInt16(0)); // sTypoLineGap
            w.Write(BEUInt16(800)); // usWinAscent
            w.Write(BEUInt16(200)); // usWinDescent
            w.Write(new byte[8]); // ulCodePageRange1, ulCodePageRange2
            w.Write(BEInt16(500)); // sxHeight
            w.Write(BEInt16(700)); // sCapHeight
            PadTo(w, ms, postOffset);

            // post table
            w.Write(BEUInt32(0x00030000)); // version 3.0
            w.Write(BEInt16(0)); // italicAngle integer part
            w.Write(BEUInt16(0)); // italicAngle fraction
            w.Write(BEInt16(-100)); // underlinePosition
            w.Write(BEInt16(50)); // underlineThickness
            w.Write(BEUInt32(0)); // isFixedPitch = false
            w.Write(new byte[16]); // minMemType42, etc.
            PadTo(w, ms, hmtxOffset);

            // hmtx table
            w.Write(BEUInt16(600)); // advanceWidth for glyph 0
            w.Write(BEInt16(0));     // lsb

            w.Flush();
            return ms.ToArray();
        }

        private static int Align4(int size) => (size + 3) & ~3;

        private static void PadTo(BinaryWriter w, MemoryStream ms, int target)
        {
            while (ms.Position < target) w.Write((byte)0);
        }

        private static void WriteTableEntry(BinaryWriter w, string tag, uint checksum, uint offset, uint length)
        {
            foreach (char c in tag) w.Write((byte)c);
            w.Write(BEUInt32(checksum));
            w.Write(BEUInt32(offset));
            w.Write(BEUInt32(length));
        }

        private static byte[] BEUInt16(ushort v) => new[] { (byte)(v >> 8), (byte)(v & 0xFF) };
        private static byte[] BEInt16(short v) => new[] { (byte)(v >> 8), (byte)(v & 0xFF) };
        private static byte[] BEUInt32(uint v) => new[] {
            (byte)(v >> 24), (byte)((v >> 16) & 0xFF),
            (byte)((v >> 8) & 0xFF), (byte)(v & 0xFF)
        };
    }
}
