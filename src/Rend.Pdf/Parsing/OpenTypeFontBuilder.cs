#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rend.Pdf.Parsing
{
    /// <summary>
    /// Builds a minimal OpenType/CFF font file (SFNT container).
    /// Stateless — takes CFF data and font metrics, produces a complete .otf byte array.
    /// </summary>
    public static class OpenTypeFontBuilder
    {
        public static byte[] Build(byte[] cffData, string fontName, IReadOnlyList<int> advanceWidths,
            IReadOnlyDictionary<int, int> unicodeToGlyph, float[] fontBBox)
        {
            if (cffData == null || cffData.Length == 0)
            {
                return Array.Empty<byte>();
            }

            int numGlyphs = advanceWidths.Count;
            var tables = new Dictionary<string, byte[]>
            {
                ["CFF "] = cffData,
                ["head"] = BuildHeadTable(fontBBox),
                ["hhea"] = BuildHheaTable(fontBBox, numGlyphs),
                ["hmtx"] = BuildHmtxTable(advanceWidths),
                ["maxp"] = BuildMaxpTable(numGlyphs),
                ["name"] = BuildNameTable(fontName),
                ["OS/2"] = BuildOs2Table(fontBBox),
                ["post"] = BuildPostTable(),
                ["cmap"] = BuildCmapTable(unicodeToGlyph),
            };

            return BuildSfnt(tables);
        }

        private static byte[] BuildHeadTable(float[] fontBBox)
        {
            var data = new byte[54];
            WriteUInt16(data, 0, 0x0001);
            WriteUInt16(data, 2, 0x0000);
            WriteUInt32(data, 12, 0x5F0F3CF5);
            WriteUInt16(data, 16, 0x000B);
            WriteUInt16(data, 18, 1000);
            WriteInt16(data, 36, (short)fontBBox[0]);
            WriteInt16(data, 38, (short)fontBBox[1]);
            WriteInt16(data, 40, (short)fontBBox[2]);
            WriteInt16(data, 42, (short)fontBBox[3]);
            WriteUInt16(data, 46, 8);
            WriteInt16(data, 48, 2);
            return data;
        }

        private static byte[] BuildHheaTable(float[] fontBBox, int numGlyphs)
        {
            var data = new byte[36];
            WriteUInt16(data, 0, 0x0001);
            WriteInt16(data, 4, (short)fontBBox[3]);
            WriteInt16(data, 6, (short)fontBBox[1]);
            WriteUInt16(data, 10, 1000);
            WriteInt16(data, 22, 1);
            WriteUInt16(data, 34, (ushort)numGlyphs);
            return data;
        }

        private static byte[] BuildHmtxTable(IReadOnlyList<int> advanceWidths)
        {
            var data = new byte[advanceWidths.Count * 4];
            for (int i = 0; i < advanceWidths.Count; i++)
            {
                int advance = advanceWidths[i];
                WriteUInt16(data, i * 4, (ushort)(advance < 0 ? 0 : (advance > 0xFFFF ? 0xFFFF : advance)));
            }
            return data;
        }

        private static byte[] BuildMaxpTable(int numGlyphs)
        {
            var data = new byte[6];
            WriteUInt16(data, 0, 0x0000);
            WriteUInt16(data, 2, 0x5000);
            WriteUInt16(data, 4, (ushort)numGlyphs);
            return data;
        }

        private static byte[] BuildNameTable(string fontName)
        {
            byte[] unicodeNameBytes = Encoding.BigEndianUnicode.GetBytes(fontName);
            byte[] regularBytes = Encoding.BigEndianUnicode.GetBytes("Regular");

            ushort[] nameIds = { 1, 2, 4, 6 };
            int recordCount = nameIds.Length;
            int stringOffset = 6 + recordCount * 12;

            using var stream = new MemoryStream();
            WriteUInt16(stream, 0);
            WriteUInt16(stream, (ushort)recordCount);
            WriteUInt16(stream, (ushort)stringOffset);

            int offset = 0;
            foreach (ushort nameId in nameIds)
            {
                byte[] stringBytes = (nameId == 2) ? regularBytes : unicodeNameBytes;
                WriteUInt16(stream, 3);
                WriteUInt16(stream, 1);
                WriteUInt16(stream, 0x0409);
                WriteUInt16(stream, nameId);
                WriteUInt16(stream, (ushort)stringBytes.Length);
                WriteUInt16(stream, (ushort)offset);
                offset += stringBytes.Length;
            }

            foreach (ushort nameId in nameIds)
            {
                byte[] stringBytes = (nameId == 2) ? regularBytes : unicodeNameBytes;
                stream.Write(stringBytes, 0, stringBytes.Length);
            }

            return stream.ToArray();
        }

        private static byte[] BuildOs2Table(float[] fontBBox)
        {
            var data = new byte[78];
            WriteUInt16(data, 0, 0x0001);
            WriteInt16(data, 2, 500);
            WriteUInt16(data, 4, 400);
            WriteUInt16(data, 6, 5);
            WriteInt16(data, 68, (short)fontBBox[3]);
            WriteInt16(data, 70, (short)fontBBox[1]);
            WriteUInt16(data, 74, (ushort)Math.Max(0, fontBBox[3]));
            WriteUInt16(data, 76, (ushort)Math.Max(0, -fontBBox[1]));
            return data;
        }

        private static byte[] BuildPostTable()
        {
            var data = new byte[32];
            WriteUInt32(data, 0, 0x00030000);
            return data;
        }

        private static byte[] BuildCmapTable(IReadOnlyDictionary<int, int> unicodeToGlyph)
        {
            byte[] format4 = BuildCmapFormat4(unicodeToGlyph);

            using var stream = new MemoryStream();
            WriteUInt16(stream, 0); // version
            WriteUInt16(stream, 1); // numTables
            WriteUInt16(stream, 3); // platformID = Windows
            WriteUInt16(stream, 1); // encodingID = Unicode BMP
            WriteUInt32(stream, 12); // offset to subtable
            stream.Write(format4, 0, format4.Length);
            return stream.ToArray();
        }

        private static byte[] BuildCmapFormat4(IReadOnlyDictionary<int, int> unicodeToGlyph)
        {
            var codePoints = new List<int>();
            foreach (int code in unicodeToGlyph.Keys)
            {
                if (code >= 0 && code <= 0xFFFF)
                {
                    codePoints.Add(code);
                }
            }
            codePoints.Sort();

            var startCodes = new List<int>();
            var endCodes = new List<int>();
            var idDeltas = new List<int>();
            int index = 0;
            while (index < codePoints.Count)
            {
                int segmentStart = codePoints[index];
                int glyphStart = unicodeToGlyph[segmentStart];
                int last = index;
                while (last + 1 < codePoints.Count &&
                       codePoints[last + 1] == codePoints[last] + 1 &&
                       unicodeToGlyph[codePoints[last + 1]] == unicodeToGlyph[codePoints[last]] + 1)
                {
                    last++;
                }
                startCodes.Add(segmentStart);
                endCodes.Add(codePoints[last]);
                idDeltas.Add((glyphStart - segmentStart) & 0xFFFF);
                index = last + 1;
            }

            startCodes.Add(0xFFFF);
            endCodes.Add(0xFFFF);
            idDeltas.Add(1);

            int segCount = startCodes.Count;
            int searchRange = 2;
            int entrySelector = 0;
            while (searchRange * 2 <= segCount * 2)
            {
                searchRange *= 2;
                entrySelector++;
            }
            int rangeShift = segCount * 2 - searchRange;

            using var stream = new MemoryStream();
            WriteUInt16(stream, 4); // format
            int length = 16 + segCount * 8;
            WriteUInt16(stream, (ushort)length);
            WriteUInt16(stream, 0); // language
            WriteUInt16(stream, (ushort)(segCount * 2));
            WriteUInt16(stream, (ushort)searchRange);
            WriteUInt16(stream, (ushort)entrySelector);
            WriteUInt16(stream, (ushort)rangeShift);
            foreach (int endCode in endCodes) { WriteUInt16(stream, (ushort)endCode); }
            WriteUInt16(stream, 0); // reservedPad
            foreach (int startCode in startCodes) { WriteUInt16(stream, (ushort)startCode); }
            foreach (int idDelta in idDeltas) { WriteUInt16(stream, (ushort)idDelta); }
            foreach (int unused in startCodes) { WriteUInt16(stream, 0); } // idRangeOffset (all 0)
            return stream.ToArray();
        }

        private static byte[] BuildSfnt(Dictionary<string, byte[]> tables)
        {
            int numTables = tables.Count;
            int searchRange = 1;
            int entrySelector = 0;
            while (searchRange * 2 <= numTables)
            {
                searchRange *= 2;
                entrySelector++;
            }
            searchRange *= 16;
            int rangeShift = numTables * 16 - searchRange;

            using var stream = new MemoryStream();
            stream.Write(new byte[] { (byte)'O', (byte)'T', (byte)'T', (byte)'O' }, 0, 4);
            WriteUInt16(stream, (ushort)numTables);
            WriteUInt16(stream, (ushort)searchRange);
            WriteUInt16(stream, (ushort)entrySelector);
            WriteUInt16(stream, (ushort)rangeShift);

            int headerSize = 12 + numTables * 16;
            int currentOffset = headerSize;

            var sortedTags = new List<string>(tables.Keys);
            sortedTags.Sort(StringComparer.Ordinal);

            var offsets = new Dictionary<string, int>();
            foreach (string tag in sortedTags)
            {
                offsets[tag] = currentOffset;
                currentOffset += (tables[tag].Length + 3) & ~3;
            }

            foreach (string tag in sortedTags)
            {
                byte[] tableData = tables[tag];
                stream.Write(Encoding.ASCII.GetBytes(tag), 0, 4);
                WriteUInt32(stream, CalculateChecksum(tableData));
                WriteUInt32(stream, (uint)offsets[tag]);
                WriteUInt32(stream, (uint)tableData.Length);
            }

            foreach (string tag in sortedTags)
            {
                byte[] tableData = tables[tag];
                stream.Write(tableData, 0, tableData.Length);
                int padding = ((tableData.Length + 3) & ~3) - tableData.Length;
                for (int i = 0; i < padding; i++)
                {
                    stream.WriteByte(0);
                }
            }

            return stream.ToArray();
        }

        private static uint CalculateChecksum(byte[] data)
        {
            uint sum = 0;
            int length = (data.Length + 3) & ~3;
            for (int i = 0; i < length; i += 4)
            {
                uint val = 0;
                for (int j = 0; j < 4; j++)
                {
                    val <<= 8;
                    if (i + j < data.Length)
                    {
                        val |= data[i + j];
                    }
                }
                sum += val;
            }
            return sum;
        }

        private static void WriteUInt16(byte[] data, int offset, ushort value)
        {
            data[offset] = (byte)(value >> 8);
            data[offset + 1] = (byte)(value & 0xFF);
        }

        private static void WriteInt16(byte[] data, int offset, short value)
        {
            data[offset] = (byte)((ushort)value >> 8);
            data[offset + 1] = (byte)(value & 0xFF);
        }

        private static void WriteUInt32(byte[] data, int offset, uint value)
        {
            data[offset] = (byte)(value >> 24);
            data[offset + 1] = (byte)(value >> 16);
            data[offset + 2] = (byte)(value >> 8);
            data[offset + 3] = (byte)(value & 0xFF);
        }

        private static void WriteUInt16(Stream stream, ushort value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value & 0xFF));
        }

        private static void WriteUInt32(Stream stream, uint value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value & 0xFF));
        }
    }
}
