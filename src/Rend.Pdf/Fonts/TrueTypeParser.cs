using System;
using System.Collections.Generic;
using System.Text;
using Rend.Core.Values;

namespace Rend.Pdf.Fonts
{
    /// <summary>
    /// Parses TrueType / OpenType font files to extract metrics, glyph mappings,
    /// and advance widths needed for PDF font embedding.
    /// </summary>
    internal static class TrueTypeParser
    {
        /// <summary>
        /// Parse a TrueType/OpenType font file and create a PdfFont.
        /// </summary>
        public static PdfFont Parse(byte[] fontData, int fontIndex)
        {
            var reader = new FontReader(fontData);

            // Read offset table
            uint sfVersion = reader.ReadUInt32();
            // 0x00010000 = TrueType, 'OTTO' (0x4F54544F) = OpenType CFF
            if (sfVersion == 0x4F54544F)
                throw new NotSupportedException(
                    "OpenType CFF fonts (.otf) are not supported. Only TrueType fonts (.ttf) can be embedded.");
            ushort numTables = reader.ReadUInt16();
            reader.Skip(6); // searchRange, entrySelector, rangeShift

            // Read table directory
            var tables = new Dictionary<string, TableEntry>();
            for (int i = 0; i < numTables; i++)
            {
                string tag = reader.ReadTag();
                uint checksum = reader.ReadUInt32();
                uint offset = reader.ReadUInt32();
                uint length = reader.ReadUInt32();
                tables[tag] = new TableEntry(offset, length);
            }

            // Parse required tables
            var head = ParseHead(fontData, tables["head"]);
            var hhea = ParseHhea(fontData, tables["hhea"]);
            var maxp = ParseMaxp(fontData, tables["maxp"]);
            var hmtx = ParseHmtx(fontData, tables["hmtx"], hhea.NumberOfHMetrics, maxp.NumGlyphs);
            var nameStr = ParseName(fontData, tables["name"]);
            var os2 = tables.ContainsKey("OS/2") ? ParseOs2(fontData, tables["OS/2"]) : default;
            var cmap = ParseCmap(fontData, tables["cmap"]);

            // Build font name
            string baseFontName = SanitizeFontName(nameStr.PostScriptName ?? nameStr.FamilyName ?? $"Font{fontIndex}");

            // Build char-to-glyph mapping (BMP)
            var charToGlyph = new ushort[65536];
            Dictionary<int, ushort>? supplementaryMap = null;

            foreach (var kvp in cmap)
            {
                if (kvp.Key < 65536)
                {
                    charToGlyph[kvp.Key] = kvp.Value;
                }
                else
                {
                    if (supplementaryMap == null)
                        supplementaryMap = new Dictionary<int, ushort>();
                    supplementaryMap[kvp.Key] = kvp.Value;
                }
            }

            // Build metrics
            float unitsPerEm = head.UnitsPerEm;
            var metrics = new FontMetrics(
                ascent: os2.Ascent != 0 ? os2.Ascent : hhea.Ascent,
                descent: os2.Descent != 0 ? os2.Descent : hhea.Descent,
                capHeight: os2.CapHeight != 0 ? os2.CapHeight : hhea.Ascent * 0.7f,
                xHeight: os2.XHeight != 0 ? os2.XHeight : hhea.Ascent * 0.5f,
                stemV: 80, // Approximate; could be computed from glyph data
                italicAngle: head.ItalicAngle,
                bBox: new RectF(head.XMin, head.YMin, head.XMax - head.XMin, head.YMax - head.YMin),
                unitsPerEm: unitsPerEm,
                flags: ComputeFontFlags(os2, head)
            );

            return new PdfFont(baseFontName, metrics, charToGlyph, hmtx, supplementaryMap, isStandard14: false);
        }

        private static string SanitizeFontName(string name)
        {
            // PDF font names: remove spaces and special characters
            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (c >= 0x21 && c <= 0x7E && c != '[' && c != ']' && c != '(' && c != ')' &&
                    c != '<' && c != '>' && c != '{' && c != '}' && c != '/' && c != '%')
                    sb.Append(c);
            }
            return sb.Length > 0 ? sb.ToString() : "UnknownFont";
        }

        private static int ComputeFontFlags(Os2Data os2, HeadData head)
        {
            int flags = 0;
            // Bit 1 (2): FixedPitch
            if (os2.IsFixedPitch) flags |= 1;
            // Bit 2 (4): Serif (rough heuristic)
            // Bit 3 (8): Symbolic
            // Bit 4 (16): Script
            // Bit 6 (64): Italic
            if (head.ItalicAngle != 0) flags |= 64;
            // Bit 6 is Nonsymbolic (32) — set for Latin fonts
            flags |= 32;
            return flags;
        }

        // ═══════════════════════════════════════════
        // Table Parsers
        // ═══════════════════════════════════════════

        private static HeadData ParseHead(byte[] data, TableEntry entry)
        {
            var r = new FontReader(data, (int)entry.Offset);
            r.Skip(4); // majorVersion, minorVersion
            r.Skip(4); // fontRevision
            r.Skip(4); // checksumAdjustment
            r.Skip(4); // magicNumber
            ushort flags = r.ReadUInt16();
            ushort unitsPerEm = r.ReadUInt16();
            r.Skip(16); // created, modified
            short xMin = r.ReadInt16();
            short yMin = r.ReadInt16();
            short xMax = r.ReadInt16();
            short yMax = r.ReadInt16();
            ushort macStyle = r.ReadUInt16();
            // italicAngle is in 'post' table, but we approximate from macStyle
            float italicAngle = (macStyle & 0x02) != 0 ? -12f : 0f;

            return new HeadData
            {
                UnitsPerEm = unitsPerEm,
                XMin = xMin,
                YMin = yMin,
                XMax = xMax,
                YMax = yMax,
                ItalicAngle = italicAngle
            };
        }

        private static HheaData ParseHhea(byte[] data, TableEntry entry)
        {
            var r = new FontReader(data, (int)entry.Offset);
            r.Skip(4); // version
            short ascent = r.ReadInt16();
            short descent = r.ReadInt16();
            short lineGap = r.ReadInt16();
            r.Skip(24); // advanceWidthMax ... reserved
            ushort numberOfHMetrics = r.ReadUInt16();

            return new HheaData
            {
                Ascent = ascent,
                Descent = descent,
                LineGap = lineGap,
                NumberOfHMetrics = numberOfHMetrics
            };
        }

        private static MaxpData ParseMaxp(byte[] data, TableEntry entry)
        {
            var r = new FontReader(data, (int)entry.Offset);
            r.Skip(4); // version
            ushort numGlyphs = r.ReadUInt16();
            return new MaxpData { NumGlyphs = numGlyphs };
        }

        private static float[] ParseHmtx(byte[] data, TableEntry entry, int numberOfHMetrics, int numGlyphs)
        {
            var r = new FontReader(data, (int)entry.Offset);
            var widths = new float[numGlyphs];

            ushort lastAdvance = 0;
            for (int i = 0; i < numberOfHMetrics; i++)
            {
                lastAdvance = r.ReadUInt16();
                r.Skip(2); // lsb
                if (i < numGlyphs)
                    widths[i] = lastAdvance;
            }

            // Remaining glyphs share the last advance width
            for (int i = numberOfHMetrics; i < numGlyphs; i++)
            {
                widths[i] = lastAdvance;
            }

            return widths;
        }

        private static NameData ParseName(byte[] data, TableEntry entry)
        {
            var r = new FontReader(data, (int)entry.Offset);
            ushort format = r.ReadUInt16();
            ushort count = r.ReadUInt16();
            ushort stringOffset = r.ReadUInt16();
            int storageOffset = (int)entry.Offset + stringOffset;

            string? familyName = null;
            string? postScriptName = null;

            for (int i = 0; i < count; i++)
            {
                ushort platformId = r.ReadUInt16();
                ushort encodingId = r.ReadUInt16();
                ushort languageId = r.ReadUInt16();
                ushort nameId = r.ReadUInt16();
                ushort length = r.ReadUInt16();
                ushort offset = r.ReadUInt16();

                // Prefer Windows Unicode BMP (platformId=3, encodingId=1)
                if (platformId == 3 && encodingId == 1)
                {
                    int strStart = storageOffset + offset;
                    if (strStart + length <= data.Length)
                    {
                        string value = Encoding.BigEndianUnicode.GetString(data, strStart, length);
                        if (nameId == 1) familyName = value;
                        else if (nameId == 6) postScriptName = value;
                    }
                }
                // Fallback: Mac Roman (platformId=1)
                else if (platformId == 1 && familyName == null)
                {
                    int strStart = storageOffset + offset;
                    if (strStart + length <= data.Length)
                    {
                        string value = Encoding.ASCII.GetString(data, strStart, length);
                        if (nameId == 1) familyName = value;
                        else if (nameId == 6) postScriptName = value;
                    }
                }
            }

            return new NameData { FamilyName = familyName, PostScriptName = postScriptName };
        }

        private static Os2Data ParseOs2(byte[] data, TableEntry entry)
        {
            var r = new FontReader(data, (int)entry.Offset);
            ushort version = r.ReadUInt16();
            short avgCharWidth = r.ReadInt16();
            ushort weightClass = r.ReadUInt16();
            ushort widthClass = r.ReadUInt16();
            r.Skip(2); // fsType
            r.Skip(22); // ySubscript*, ySuperscript*, yStrikeout*, sFamilyClass, panose[10]
            r.Skip(16); // ulUnicodeRange1-4
            r.Skip(4);  // achVendID
            ushort fsSelection = r.ReadUInt16();
            r.Skip(4);  // usFirstCharIndex, usLastCharIndex
            short typoAscender = r.ReadInt16();
            short typoDescender = r.ReadInt16();
            r.Skip(2); // sTypoLineGap
            r.Skip(4); // usWinAscent, usWinDescent

            short xHeight = 0;
            short capHeight = 0;
            if (version >= 2)
            {
                r.Skip(8); // ulCodePageRange1, ulCodePageRange2
                xHeight = r.ReadInt16();
                capHeight = r.ReadInt16();
            }

            return new Os2Data
            {
                WeightClass = weightClass,
                Ascent = typoAscender,
                Descent = typoDescender,
                CapHeight = capHeight,
                XHeight = xHeight,
                IsFixedPitch = avgCharWidth == 500 // crude heuristic
            };
        }

        private static Dictionary<int, ushort> ParseCmap(byte[] data, TableEntry entry)
        {
            var r = new FontReader(data, (int)entry.Offset);
            r.Skip(2); // version
            ushort numSubtables = r.ReadUInt16();

            // Find the best subtable: prefer (3,10) = Windows Unicode full, then (3,1) = Windows BMP
            int bestOffset = -1;
            int bestPriority = -1;

            for (int i = 0; i < numSubtables; i++)
            {
                ushort platformId = r.ReadUInt16();
                ushort encodingId = r.ReadUInt16();
                uint offset = r.ReadUInt32();

                int priority = 0;
                if (platformId == 3 && encodingId == 10) priority = 3; // Windows Unicode full
                else if (platformId == 3 && encodingId == 1) priority = 2; // Windows BMP
                else if (platformId == 0) priority = 1; // Unicode

                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    bestOffset = (int)(entry.Offset + offset);
                }
            }

            if (bestOffset < 0)
                return new Dictionary<int, ushort>();

            return ParseCmapSubtable(data, bestOffset);
        }

        private static Dictionary<int, ushort> ParseCmapSubtable(byte[] data, int offset)
        {
            var map = new Dictionary<int, ushort>();
            var r = new FontReader(data, offset);
            ushort format = r.ReadUInt16();

            switch (format)
            {
                case 4:
                    ParseCmapFormat4(data, offset, map);
                    break;
                case 12:
                    ParseCmapFormat12(data, offset, map);
                    break;
                // Other formats (0, 6, etc.) can be added later
            }

            return map;
        }

        private static void ParseCmapFormat4(byte[] data, int offset, Dictionary<int, ushort> map)
        {
            var r = new FontReader(data, offset);
            r.Skip(2); // format (already read)
            ushort length = r.ReadUInt16();
            r.Skip(2); // language
            ushort segCountX2 = r.ReadUInt16();
            int segCount = segCountX2 / 2;
            r.Skip(6); // searchRange, entrySelector, rangeShift

            var endCodes = new ushort[segCount];
            for (int i = 0; i < segCount; i++) endCodes[i] = r.ReadUInt16();
            r.Skip(2); // reservedPad

            var startCodes = new ushort[segCount];
            for (int i = 0; i < segCount; i++) startCodes[i] = r.ReadUInt16();

            var idDeltas = new short[segCount];
            for (int i = 0; i < segCount; i++) idDeltas[i] = r.ReadInt16();

            int idRangeOffsetsStart = r.Position;
            var idRangeOffsets = new ushort[segCount];
            for (int i = 0; i < segCount; i++) idRangeOffsets[i] = r.ReadUInt16();

            for (int i = 0; i < segCount; i++)
            {
                if (endCodes[i] == 0xFFFF && startCodes[i] == 0xFFFF) break;

                for (int c = startCodes[i]; c <= endCodes[i]; c++)
                {
                    ushort glyphId;
                    if (idRangeOffsets[i] == 0)
                    {
                        glyphId = (ushort)((c + idDeltas[i]) & 0xFFFF);
                    }
                    else
                    {
                        int glyphIndexOffset = idRangeOffsetsStart + i * 2 + idRangeOffsets[i] + (c - startCodes[i]) * 2;
                        if (glyphIndexOffset + 1 < data.Length)
                        {
                            glyphId = (ushort)((data[glyphIndexOffset] << 8) | data[glyphIndexOffset + 1]);
                            if (glyphId != 0)
                                glyphId = (ushort)((glyphId + idDeltas[i]) & 0xFFFF);
                        }
                        else
                        {
                            glyphId = 0;
                        }
                    }

                    if (glyphId != 0)
                        map[c] = glyphId;
                }
            }
        }

        private static void ParseCmapFormat12(byte[] data, int offset, Dictionary<int, ushort> map)
        {
            var r = new FontReader(data, offset);
            r.Skip(2); // format
            r.Skip(2); // reserved
            r.Skip(4); // length
            r.Skip(4); // language
            uint numGroups = r.ReadUInt32();

            for (uint i = 0; i < numGroups; i++)
            {
                uint startCharCode = r.ReadUInt32();
                uint endCharCode = r.ReadUInt32();
                uint startGlyphId = r.ReadUInt32();

                for (uint c = startCharCode; c <= endCharCode; c++)
                {
                    ushort glyphId = (ushort)(startGlyphId + (c - startCharCode));
                    if (glyphId != 0)
                        map[(int)c] = glyphId;
                }
            }
        }

        // ═══════════════════════════════════════════
        // Data structures
        // ═══════════════════════════════════════════

        private struct TableEntry
        {
            public uint Offset;
            public uint Length;
            public TableEntry(uint offset, uint length) { Offset = offset; Length = length; }
        }

        private struct HeadData
        {
            public ushort UnitsPerEm;
            public short XMin, YMin, XMax, YMax;
            public float ItalicAngle;
        }

        private struct HheaData
        {
            public short Ascent, Descent, LineGap;
            public ushort NumberOfHMetrics;
        }

        private struct MaxpData { public ushort NumGlyphs; }

        private struct NameData
        {
            public string? FamilyName;
            public string? PostScriptName;
        }

        private struct Os2Data
        {
            public ushort WeightClass;
            public short Ascent, Descent;
            public short CapHeight, XHeight;
            public bool IsFixedPitch;
        }

        // ═══════════════════════════════════════════
        // Binary reader for big-endian font data
        // ═══════════════════════════════════════════

        private ref struct FontReader
        {
            private readonly byte[] _data;
            public int Position;

            public FontReader(byte[] data, int position = 0)
            {
                _data = data;
                Position = position;
            }

            public ushort ReadUInt16()
            {
                ushort val = (ushort)((_data[Position] << 8) | _data[Position + 1]);
                Position += 2;
                return val;
            }

            public short ReadInt16()
            {
                short val = (short)((_data[Position] << 8) | _data[Position + 1]);
                Position += 2;
                return val;
            }

            public uint ReadUInt32()
            {
                uint val = ((uint)_data[Position] << 24) | ((uint)_data[Position + 1] << 16) |
                           ((uint)_data[Position + 2] << 8) | _data[Position + 3];
                Position += 4;
                return val;
            }

            public string ReadTag()
            {
                var tag = Encoding.ASCII.GetString(_data, Position, 4);
                Position += 4;
                return tag;
            }

            public void Skip(int bytes) => Position += bytes;
        }
    }
}
