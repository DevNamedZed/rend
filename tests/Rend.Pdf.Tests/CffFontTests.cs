using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class CffFontTests
    {
        [Fact]
        public void CffFont_CanBeParsed()
        {
            var fontData = BuildMinimalCffFont();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(fontData);
            var font = doc.AddFont(stream);

            Assert.NotNull(font);
            Assert.False(font.IsStandard14);
        }

        [Fact]
        public void CffFont_HasValidMetrics()
        {
            var fontData = BuildMinimalCffFont();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(fontData);
            var font = doc.AddFont(stream);

            Assert.True(font.Metrics.UnitsPerEm > 0);
            Assert.True(font.Metrics.Ascent > 0);
        }

        [Fact]
        public void CffFont_ProducesValidPdf()
        {
            var fontData = BuildMinimalCffFont();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(fontData);
            var font = doc.AddFont(stream);

            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            Assert.True(pdfBytes.Length > 0);
        }

        [Fact]
        public void CffFont_UsesCIDFontType0()
        {
            var fontData = BuildMinimalCffFont();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(fontData);
            var font = doc.AddFont(stream);

            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            Assert.Contains("/CIDFontType0", pdfText);
            Assert.DoesNotContain("/CIDFontType2", pdfText);
        }

        [Fact]
        public void CffFont_UsesFontFile3()
        {
            var fontData = BuildMinimalCffFont();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(fontData);
            var font = doc.AddFont(stream);

            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            Assert.Contains("/FontFile3", pdfText);
            Assert.Contains("/CIDFontType0C", pdfText);
            Assert.DoesNotContain("/FontFile2", pdfText);
        }

        [Fact]
        public void CffFont_NoCIDToGIDMap()
        {
            var fontData = BuildMinimalCffFont();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(fontData);
            var font = doc.AddFont(stream);

            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            // CFF fonts use CIDFontType0 which doesn't need CIDToGIDMap
            Assert.DoesNotContain("/CIDToGIDMap", pdfText);
        }

        [Fact]
        public void CffFont_HasToUnicode()
        {
            var fontData = BuildMinimalCffFont();
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            using var stream = new MemoryStream(fontData);
            var font = doc.AddFont(stream);

            var page = doc.AddPage(PageSize.A4);
            page.Content.BeginText();
            page.Content.SetFont(font, 12);
            page.Content.MoveTextPosition(50, 700);
            page.Content.ShowText(font, "A");
            page.Content.EndText();

            var pdfBytes = doc.ToArray();
            string pdfText = Encoding.ASCII.GetString(pdfBytes);

            Assert.Contains("/ToUnicode", pdfText);
        }

        /// <summary>
        /// Build a minimal valid OpenType CFF font with 'OTTO' signature.
        /// Contains required tables: head, hhea, maxp, hmtx, cmap, name, OS/2, post, CFF.
        /// </summary>
        private static byte[] BuildMinimalCffFont()
        {
            // Build individual tables
            var tables = new Dictionary<string, byte[]>
            {
                ["CFF "] = BuildMinimalCffTable(),
                ["OS/2"] = BuildMinimalOs2(),
                ["cmap"] = BuildMinimalCmap(),
                ["head"] = BuildMinimalHead(),
                ["hhea"] = BuildMinimalHhea(),
                ["hmtx"] = BuildMinimalHmtx(),
                ["maxp"] = BuildMinimalMaxp(),
                ["name"] = BuildMinimalName(),
                ["post"] = BuildMinimalPost()
            };

            // Assemble font file
            int numTables = tables.Count;
            int searchRange = 1, entrySelector = 0;
            while (searchRange * 2 <= numTables) { searchRange *= 2; entrySelector++; }
            searchRange *= 16;
            int rangeShift = numTables * 16 - searchRange;

            int headerSize = 12 + numTables * 16;
            var tagOrder = new List<string>(tables.Keys);
            tagOrder.Sort(StringComparer.Ordinal);

            var offsets = new Dictionary<string, int>();
            int offset = headerSize;
            foreach (var tag in tagOrder)
            {
                offsets[tag] = offset;
                offset += tables[tag].Length + ((4 - (tables[tag].Length % 4)) % 4);
            }

            var result = new byte[offset];
            int pos = 0;

            // Offset table: 'OTTO' signature
            W32(result, ref pos, 0x4F54544F); // 'OTTO'
            W16(result, ref pos, (ushort)numTables);
            W16(result, ref pos, (ushort)searchRange);
            W16(result, ref pos, (ushort)entrySelector);
            W16(result, ref pos, (ushort)rangeShift);

            // Table directory
            foreach (var tag in tagOrder)
            {
                for (int i = 0; i < 4; i++)
                    result[pos++] = i < tag.Length ? (byte)tag[i] : (byte)' ';
                W32(result, ref pos, 0); // checksum (skip)
                W32(result, ref pos, (uint)offsets[tag]);
                W32(result, ref pos, (uint)tables[tag].Length);
            }

            // Table data
            foreach (var tag in tagOrder)
                Buffer.BlockCopy(tables[tag], 0, result, offsets[tag], tables[tag].Length);

            return result;
        }

        private static byte[] BuildMinimalCffTable()
        {
            // Minimal CFF font with .notdef and one glyph
            // CFF structure: Header, Name INDEX, Top DICT INDEX, String INDEX, Global Subr INDEX, CharStrings INDEX
            using var ms = new MemoryStream();

            // Header (4 bytes)
            ms.WriteByte(1);    // major
            ms.WriteByte(0);    // minor
            ms.WriteByte(4);    // hdrSize
            ms.WriteByte(1);    // offSize (1 byte)

            // Name INDEX: 1 entry "TestCFF"
            var nameBytes = Encoding.ASCII.GetBytes("TestCFF");
            WriteIndex(ms, new[] { nameBytes });

            // Top DICT INDEX: 1 entry with minimal dict
            var topDict = new MemoryStream();
            // charset offset (will be near the end)
            // For minimal CFF, use predefined charset 0 (ISOAdobe)
            // Write CharStrings offset — we'll calculate and write a placeholder
            // Actually for minimal: just encode CharStrings offset
            // We need to calculate where things land...
            // Simplified: encode ROS (for CIDFont), charset, CharStrings offset
            // For a non-CID CFF, just encode charset=0 (predefined) and CharStrings offset

            // CharStrings will start after: header(4) + nameIndex + topDictIndex + stringIndex(3 empty) + globalSubrIndex(3 empty)
            // We'll fix the offset after building everything

            // Top DICT encoding:
            // charset = 0 means ISOAdobe (predefined) — encode as: 0 15 (operand 0, operator 15)
            topDict.WriteByte(0x8B); // encode integer 0
            topDict.WriteByte(15);   // operator: charset

            // We'll encode CharStrings offset below after calculating it
            byte[] topDictPartial = topDict.ToArray();

            // String INDEX: empty
            var emptyIndex = new byte[] { 0, 0, 0 }; // count=0 (2 bytes) + padding? Actually count=0 is just 2 bytes

            // Global Subr INDEX: empty (count=0)

            // Calculate CharStrings offset
            // After header: name index + top dict index + string index + global subr index
            // name index size: 2(count) + 1(offSize) + 2(offsets) + nameBytes.Length
            int nameIndexSize = 2 + 1 + 2 * 1 + nameBytes.Length; // count(2) + offSize(1) + (count+1)*offSize + data
            // Actually INDEX format: count(2) + offSize(1) + (count+1) offsets of offSize bytes + data

            // Let's just build everything and compute offset dynamically
            // Reset and build properly with a two-pass approach

            ms.Position = 0;
            ms.SetLength(0);

            // === Pass 1: build everything except TopDict's CharStrings offset ===

            // Header
            ms.WriteByte(1); ms.WriteByte(0); ms.WriteByte(4); ms.WriteByte(1);
            // afterHeader = 4;

            // Name INDEX
            int nameIndexStart = (int)ms.Position;
            WriteIndex(ms, new[] { nameBytes });
            int nameIndexEnd = (int)ms.Position;

            // Top DICT INDEX placeholder — we'll come back
            int topDictIndexStart = (int)ms.Position;
            // For now write a placeholder TopDict with fixed size
            // Encode: charset=0, CharStrings=<offset>
            // charset=0: 0x8B 0x0F (2 bytes)
            // CharStrings offset: we need 5-byte int encoding to be safe
            // Use 29 <4-byte int> followed by operator 17
            var topDictBytes = new byte[8];
            topDictBytes[0] = 0x8B; // integer 0
            topDictBytes[1] = 15;   // operator: charset
            topDictBytes[2] = 29;   // 5-byte integer follows
            // bytes 3-6: 4-byte big-endian integer (CharStrings offset) — fill later
            topDictBytes[7] = 17;   // operator: CharStrings

            WriteIndex(ms, new[] { topDictBytes });
            int topDictIndexEnd = (int)ms.Position;

            // String INDEX (empty)
            W16BE(ms, 0); // count = 0
            int afterStringIndex = (int)ms.Position;

            // Global Subr INDEX (empty)
            W16BE(ms, 0); // count = 0
            int afterGlobalSubr = (int)ms.Position;

            // CharStrings INDEX
            int charStringsOffset = (int)ms.Position;
            // .notdef charstring: endchar (14)
            var notdefCharString = new byte[] { 14 }; // endchar
            // Glyph 1 charstring: minimal path — moveto, lineto, endchar
            // rmoveto: dx dy rmoveto (21)
            // 100 0 rmoveto = 0xF6 0x8B 21
            var glyph1CharString = new byte[]
            {
                0xF6, 0x8B, 21,        // 100 0 rmoveto
                0x8B, 0xF6, 5,          // 0 100 rlineto
                0x75, 0x8B, 5,          // -100 0 rlineto
                14                       // endchar
            };
            WriteIndex(ms, new[] { notdefCharString, glyph1CharString });

            // === Pass 2: fix CharStrings offset in TopDict ===
            var fontBytes = ms.ToArray();

            // The TopDict data is inside the INDEX at topDictIndexStart
            // INDEX header: count(2) + offSize(1) + offsets((count+1)*offSize) + data
            // count=1, offSize=1, offsets = [1, len+1], data = topDictBytes
            int topDictDataStart = topDictIndexStart + 2 + 1 + 2; // count(2) + offSize(1) + 2 offsets(1 each)

            // CharStrings offset is at topDictDataStart + 3 (skip charset encoding 2 bytes + int tag 1 byte)
            fontBytes[topDictDataStart + 3] = (byte)((charStringsOffset >> 24) & 0xFF);
            fontBytes[topDictDataStart + 4] = (byte)((charStringsOffset >> 16) & 0xFF);
            fontBytes[topDictDataStart + 5] = (byte)((charStringsOffset >> 8) & 0xFF);
            fontBytes[topDictDataStart + 6] = (byte)(charStringsOffset & 0xFF);

            return fontBytes;
        }

        private static byte[] BuildMinimalHead()
        {
            var d = new byte[54];
            // version = 1.0
            d[0] = 0; d[1] = 1; d[2] = 0; d[3] = 0;
            // fontRevision, checksumAdjustment, magicNumber
            d[12] = 0x5F; d[13] = 0x0F; d[14] = 0x3C; d[15] = 0xF5; // magic
            // flags
            d[16] = 0; d[17] = 0x0B;
            // unitsPerEm = 1000
            d[18] = 0x03; d[19] = 0xE8;
            // created, modified (16 bytes skip)
            // xMin=0, yMin=-200, xMax=500, yMax=800
            d[36] = 0; d[37] = 0;      // xMin
            d[38] = 0xFF; d[39] = 0x38; // yMin = -200
            d[40] = 0x01; d[41] = 0xF4; // xMax = 500
            d[42] = 0x03; d[43] = 0x20; // yMax = 800
            // macStyle = 0
            // indexToLocFormat = 0 (not used for CFF)
            return d;
        }

        private static byte[] BuildMinimalHhea()
        {
            var d = new byte[36];
            // version = 1.0
            d[0] = 0; d[1] = 1; d[2] = 0; d[3] = 0;
            // ascent = 800
            d[4] = 0x03; d[5] = 0x20;
            // descent = -200
            d[6] = 0xFF; d[7] = 0x38;
            // lineGap = 0
            // ... skip to numberOfHMetrics at offset 34
            d[34] = 0; d[35] = 2; // 2 glyphs
            return d;
        }

        private static byte[] BuildMinimalMaxp()
        {
            var d = new byte[6];
            // version = 0.5 (CFF maxp is version 0.5, only 6 bytes: version + numGlyphs)
            d[0] = 0; d[1] = 0; d[2] = 0x50; d[3] = 0x00; // 0x00005000 = 0.5
            d[4] = 0; d[5] = 2; // numGlyphs = 2
            return d;
        }

        private static byte[] BuildMinimalHmtx()
        {
            // 2 glyphs, each with advanceWidth(2) + lsb(2)
            var d = new byte[8];
            // glyph 0 (.notdef): width=500
            d[0] = 0x01; d[1] = 0xF4; d[2] = 0; d[3] = 0;
            // glyph 1: width=600
            d[4] = 0x02; d[5] = 0x58; d[6] = 0; d[7] = 0;
            return d;
        }

        private static byte[] BuildMinimalCmap()
        {
            using var ms = new MemoryStream();
            // cmap header
            W16BE(ms, 0);  // version
            W16BE(ms, 1);  // numSubtables

            // Platform 3 (Windows), Encoding 1 (BMP), offset=12
            W16BE(ms, 3); W16BE(ms, 1); W32BE(ms, 12);

            // Format 4 subtable mapping 'A' (0x41) to glyph 1
            int subtableStart = (int)ms.Position;
            W16BE(ms, 4);   // format
            W16BE(ms, 32);  // length
            W16BE(ms, 0);   // language
            W16BE(ms, 4);   // segCountX2 (2 segments)
            W16BE(ms, 4);   // searchRange
            W16BE(ms, 1);   // entrySelector
            W16BE(ms, 0);   // rangeShift

            // endCodes
            W16BE(ms, 0x41);    // segment 1: end=A
            W16BE(ms, 0xFFFF);  // segment 2: end=0xFFFF

            W16BE(ms, 0);       // reservedPad

            // startCodes
            W16BE(ms, 0x41);    // segment 1: start=A
            W16BE(ms, 0xFFFF);  // segment 2: start=0xFFFF

            // idDeltas
            W16BE(ms, unchecked((ushort)(1 - 0x41))); // maps A to glyph 1
            W16BE(ms, 1);       // sentinel

            // idRangeOffsets
            W16BE(ms, 0);
            W16BE(ms, 0);

            return ms.ToArray();
        }

        private static byte[] BuildMinimalOs2()
        {
            var d = new byte[78]; // version 2 OS/2 table
            // version = 2
            d[0] = 0; d[1] = 2;
            // xAvgCharWidth = 500
            d[2] = 0x01; d[3] = 0xF4;
            // usWeightClass = 400
            d[4] = 0x01; d[5] = 0x90;
            // usWidthClass = 5
            d[6] = 0; d[7] = 5;
            // skip to fsSelection (offset 62)
            // skip to sTypoAscender (offset 68)
            d[68] = 0x03; d[69] = 0x20; // typoAscender = 800
            d[70] = 0xFF; d[71] = 0x38; // typoDescender = -200
            // skip sTypoLineGap(2), usWinAscent(2), usWinDescent(2) = offset 72..77
            // version 2 fields start at 78... we need at least 86 bytes for xHeight and capHeight
            // Let's extend
            d = new byte[86];
            d[0] = 0; d[1] = 2;
            d[2] = 0x01; d[3] = 0xF4;
            d[4] = 0x01; d[5] = 0x90;
            d[6] = 0; d[7] = 5;
            d[68] = 0x03; d[69] = 0x20; // typoAscender = 800
            d[70] = 0xFF; d[71] = 0x38; // typoDescender = -200
            // offset 78: ulCodePageRange1(4) + ulCodePageRange2(4) = 8 bytes
            // offset 86: sxHeight(2), sCapHeight(2)
            // Need more space
            d = new byte[90];
            d[0] = 0; d[1] = 2;
            d[2] = 0x01; d[3] = 0xF4;
            d[4] = 0x01; d[5] = 0x90;
            d[6] = 0; d[7] = 5;
            d[68] = 0x03; d[69] = 0x20; // typoAscender = 800
            d[70] = 0xFF; d[71] = 0x38; // typoDescender = -200
            // ulCodePageRange1-2 at 78: 8 bytes of zeros
            // sxHeight at 86
            d[86] = 0x01; d[87] = 0xF4; // xHeight = 500
            // sCapHeight at 88
            d[88] = 0x02; d[89] = 0xBC; // capHeight = 700
            return d;
        }

        private static byte[] BuildMinimalName()
        {
            var familyName = Encoding.BigEndianUnicode.GetBytes("TestCFF");
            var psName = Encoding.BigEndianUnicode.GetBytes("TestCFF");

            using var ms = new MemoryStream();
            int storageStart = 6 + 2 * 12; // header(6) + 2 records * 12 bytes each

            W16BE(ms, 0);  // format
            W16BE(ms, 2);  // count
            W16BE(ms, (ushort)storageStart); // stringOffset (relative to start of table)

            // Record 1: family name (nameID=1)
            W16BE(ms, 3); W16BE(ms, 1); W16BE(ms, 0x0409);
            W16BE(ms, 1); W16BE(ms, (ushort)familyName.Length); W16BE(ms, 0);

            // Record 2: PostScript name (nameID=6)
            W16BE(ms, 3); W16BE(ms, 1); W16BE(ms, 0x0409);
            W16BE(ms, 6); W16BE(ms, (ushort)psName.Length); W16BE(ms, (ushort)familyName.Length);

            ms.Write(familyName, 0, familyName.Length);
            ms.Write(psName, 0, psName.Length);

            return ms.ToArray();
        }

        private static byte[] BuildMinimalPost()
        {
            var d = new byte[32];
            // version 3.0 (no glyph names)
            d[0] = 0; d[1] = 3; d[2] = 0; d[3] = 0;
            return d;
        }

        // Helpers

        private static void WriteIndex(MemoryStream ms, byte[][] items)
        {
            int count = items.Length;
            W16BE(ms, (ushort)count);

            // Calculate total data size
            int totalData = 0;
            foreach (var item in items) totalData += item.Length;

            // Determine offSize
            byte offSize = 1;
            if (totalData + 1 > 255) offSize = 2;
            if (totalData + 1 > 65535) offSize = 3;
            if (totalData + 1 > 16777215) offSize = 4;
            ms.WriteByte(offSize);

            // Write offsets (1-based)
            int off = 1;
            WriteOffset(ms, offSize, off);
            foreach (var item in items)
            {
                off += item.Length;
                WriteOffset(ms, offSize, off);
            }

            // Write data
            foreach (var item in items)
                ms.Write(item, 0, item.Length);
        }

        private static void WriteOffset(MemoryStream ms, byte offSize, int value)
        {
            switch (offSize)
            {
                case 1: ms.WriteByte((byte)value); break;
                case 2: W16BE(ms, (ushort)value); break;
                case 3: ms.WriteByte((byte)((value >> 16) & 0xFF));
                        ms.WriteByte((byte)((value >> 8) & 0xFF));
                        ms.WriteByte((byte)(value & 0xFF)); break;
                case 4: W32BE(ms, (uint)value); break;
            }
        }

        private static void W16(byte[] data, ref int pos, ushort value)
        {
            data[pos++] = (byte)(value >> 8);
            data[pos++] = (byte)(value & 0xFF);
        }

        private static void W32(byte[] data, ref int pos, uint value)
        {
            data[pos++] = (byte)((value >> 24) & 0xFF);
            data[pos++] = (byte)((value >> 16) & 0xFF);
            data[pos++] = (byte)((value >> 8) & 0xFF);
            data[pos++] = (byte)(value & 0xFF);
        }

        private static void W16BE(MemoryStream ms, ushort value)
        {
            ms.WriteByte((byte)(value >> 8));
            ms.WriteByte((byte)(value & 0xFF));
        }

        private static void W32BE(MemoryStream ms, uint value)
        {
            ms.WriteByte((byte)((value >> 24) & 0xFF));
            ms.WriteByte((byte)((value >> 16) & 0xFF));
            ms.WriteByte((byte)((value >> 8) & 0xFF));
            ms.WriteByte((byte)(value & 0xFF));
        }
    }
}
