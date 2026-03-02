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
        public static PdfFont Parse(byte[] fontData, int fontIndex,
                                     FontEmbedMode embedMode = FontEmbedMode.Subset)
        {
            var reader = new FontReader(fontData);

            // Read offset table
            uint sfVersion = reader.ReadUInt32();
            // 0x00010000 = TrueType, 'OTTO' (0x4F54544F) = OpenType CFF
            bool isCff = sfVersion == 0x4F54544F;
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
            var post = tables.ContainsKey("post") ? ParsePost(fontData, tables["post"]) : default;

            // Extract CFF table data if this is a CFF-based font
            byte[]? cffTableData = null;
            if (isCff && tables.ContainsKey("CFF "))
            {
                var cffEntry = tables["CFF "];
                cffTableData = new byte[cffEntry.Length];
                Buffer.BlockCopy(fontData, (int)cffEntry.Offset, cffTableData, 0, (int)cffEntry.Length);
            }

            // Parse fvar table if present (variable font detection)
            bool isVariableFont = tables.ContainsKey("fvar");

            // Parse kerning tables (GPOS takes priority over kern per OpenType spec)
            Dictionary<uint, short>? kerningPairs = null;
            if (tables.ContainsKey("GPOS"))
                kerningPairs = ParseGposKerning(fontData, tables["GPOS"]);
            if (kerningPairs == null && tables.ContainsKey("kern"))
                kerningPairs = ParseKern(fontData, tables["kern"]);

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

            // Use post table for accurate italic angle (fallback to macStyle heuristic from head)
            float italicAngle = post.HasData ? post.ItalicAngle : head.ItalicAngle;
            // Use post table isFixedPitch if available, otherwise fall back to OS/2 heuristic
            bool isFixedPitch = post.HasData ? post.IsFixedPitch : os2.IsFixedPitch;

            // Build metrics
            float unitsPerEm = head.UnitsPerEm;
            var metrics = new FontMetrics(
                ascent: os2.Ascent != 0 ? os2.Ascent : hhea.Ascent,
                descent: os2.Descent != 0 ? os2.Descent : hhea.Descent,
                capHeight: os2.CapHeight != 0 ? os2.CapHeight : hhea.Ascent * 0.7f,
                xHeight: os2.XHeight != 0 ? os2.XHeight : hhea.Ascent * 0.5f,
                stemV: 80, // Approximate; could be computed from glyph data
                italicAngle: italicAngle,
                bBox: new RectF(head.XMin, head.YMin, head.XMax - head.XMin, head.YMax - head.YMin),
                unitsPerEm: unitsPerEm,
                flags: ComputeFontFlags(isFixedPitch, italicAngle)
            );

            return new PdfFont(baseFontName, metrics, charToGlyph, hmtx, supplementaryMap,
                               isStandard14: false, kerningPairs: kerningPairs,
                               embedMode: isCff ? FontEmbedMode.Full : embedMode,
                               isCff: isCff, cffTableData: cffTableData);
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

        private static int ComputeFontFlags(bool isFixedPitch, float italicAngle)
        {
            int flags = 0;
            // Bit 0 (1): FixedPitch
            if (isFixedPitch) flags |= 1;
            // Bit 5 (32): Nonsymbolic — set for Latin fonts
            flags |= 32;
            // Bit 6 (64): Italic
            if (italicAngle != 0) flags |= 64;
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

        // ═══════════════════════════════════════════
        // Kerning table parsers
        // ═══════════════════════════════════════════

        private static Dictionary<uint, short>? ParseKern(byte[] data, TableEntry entry)
        {
            if (entry.Length < 4) return null;

            var r = new FontReader(data, (int)entry.Offset);
            ushort version = r.ReadUInt16();

            // Only support version 0 (Microsoft/Apple classic format)
            if (version != 0) return null;

            ushort nTables = r.ReadUInt16();
            var pairs = new Dictionary<uint, short>();

            for (int t = 0; t < nTables; t++)
            {
                int subtableStart = r.Position;
                ushort subVersion = r.ReadUInt16();
                ushort subLength = r.ReadUInt16();
                ushort coverage = r.ReadUInt16();

                // Format 0 = horizontal kerning pairs
                int format = coverage >> 8;
                bool horizontal = (coverage & 0x01) != 0;
                bool crossStream = (coverage & 0x04) != 0;

                if (format == 0 && horizontal && !crossStream)
                {
                    ushort nPairs = r.ReadUInt16();
                    r.Skip(6); // searchRange, entrySelector, rangeShift

                    for (int i = 0; i < nPairs; i++)
                    {
                        ushort left = r.ReadUInt16();
                        ushort right = r.ReadUInt16();
                        short value = r.ReadInt16();

                        if (value != 0)
                        {
                            uint key = ((uint)left << 16) | right;
                            pairs[key] = value;
                        }
                    }
                }
                else
                {
                    // Skip unsupported subtable
                    r.Position = subtableStart + subLength;
                }
            }

            return pairs.Count > 0 ? pairs : null;
        }

        private static Dictionary<uint, short>? ParseGposKerning(byte[] data, TableEntry entry)
        {
            if (entry.Length < 10) return null;

            int baseOffset = (int)entry.Offset;
            var r = new FontReader(data, baseOffset);

            // GPOS header
            ushort majorVersion = r.ReadUInt16();
            ushort minorVersion = r.ReadUInt16();
            ushort scriptListOffset = r.ReadUInt16();
            ushort featureListOffset = r.ReadUInt16();
            ushort lookupListOffset = r.ReadUInt16();

            // Find "kern" feature in feature list
            int featureListBase = baseOffset + featureListOffset;
            var fr = new FontReader(data, featureListBase);
            ushort featureCount = fr.ReadUInt16();

            var kernLookupIndices = new List<ushort>();
            for (int i = 0; i < featureCount; i++)
            {
                string tag = fr.ReadTag();
                ushort offset = fr.ReadUInt16();

                if (tag == "kern")
                {
                    // Read feature table to get lookup indices
                    var ft = new FontReader(data, featureListBase + offset);
                    ushort featureParams = ft.ReadUInt16();
                    ushort lookupCount = ft.ReadUInt16();
                    for (int j = 0; j < lookupCount; j++)
                        kernLookupIndices.Add(ft.ReadUInt16());
                }
            }

            if (kernLookupIndices.Count == 0) return null;

            // Read lookup list
            int lookupListBase = baseOffset + lookupListOffset;
            var llr = new FontReader(data, lookupListBase);
            ushort lookupCount2 = llr.ReadUInt16();

            var pairs = new Dictionary<uint, short>();

            foreach (ushort lookupIdx in kernLookupIndices)
            {
                if (lookupIdx >= lookupCount2) continue;

                // Read lookup offset
                var idxReader = new FontReader(data, lookupListBase + 2 + lookupIdx * 2);
                ushort lookupOffset = idxReader.ReadUInt16();
                int lookupBase = lookupListBase + lookupOffset;

                var lr = new FontReader(data, lookupBase);
                ushort lookupType = lr.ReadUInt16();
                ushort lookupFlag = lr.ReadUInt16();
                ushort subtableCount = lr.ReadUInt16();

                // Only process PairPos (type 2) lookups
                if (lookupType != 2) continue;

                for (int s = 0; s < subtableCount; s++)
                {
                    ushort subtableOffset = lr.ReadUInt16();
                    int subtableBase = lookupBase + subtableOffset;

                    ParseGposPairPos(data, subtableBase, pairs);
                }
            }

            return pairs.Count > 0 ? pairs : null;
        }

        private static void ParseGposPairPos(byte[] data, int offset, Dictionary<uint, short> pairs)
        {
            if (offset + 10 > data.Length) return;

            var r = new FontReader(data, offset);
            ushort posFormat = r.ReadUInt16();
            ushort coverageOffset = r.ReadUInt16();
            ushort valueFormat1 = r.ReadUInt16();
            ushort valueFormat2 = r.ReadUInt16();

            // We only extract XAdvance from value record 1 (bit 0x0004)
            int valueRecord1Size = CountValueRecordBytes(valueFormat1);
            int valueRecord2Size = CountValueRecordBytes(valueFormat2);
            bool hasXAdvance1 = (valueFormat1 & 0x0004) != 0;
            int xAdvanceOffset1 = GetXAdvanceOffset(valueFormat1);

            if (!hasXAdvance1) return;

            if (posFormat == 1)
            {
                // Format 1: Individual glyph pairs
                ushort pairSetCount = r.ReadUInt16();

                // Read coverage table to get first glyph IDs
                var coverageGlyphs = ParseCoverageTable(data, offset + coverageOffset);
                if (coverageGlyphs == null) return;

                for (int i = 0; i < pairSetCount && i < coverageGlyphs.Length; i++)
                {
                    ushort pairSetOffset = r.ReadUInt16();
                    int pairSetBase = offset + pairSetOffset;

                    var pr = new FontReader(data, pairSetBase);
                    ushort pairValueCount = pr.ReadUInt16();

                    ushort firstGlyph = coverageGlyphs[i];
                    for (int j = 0; j < pairValueCount; j++)
                    {
                        ushort secondGlyph = pr.ReadUInt16();

                        // Read XAdvance from value record 1
                        int valueStart = pr.Position;
                        pr.Position = valueStart + xAdvanceOffset1;
                        short xAdvance = pr.ReadInt16();
                        // Skip rest of both value records
                        pr.Position = valueStart + valueRecord1Size + valueRecord2Size;

                        if (xAdvance != 0)
                        {
                            uint key = ((uint)firstGlyph << 16) | secondGlyph;
                            pairs[key] = xAdvance;
                        }
                    }
                }
            }
            else if (posFormat == 2)
            {
                // Format 2: Class-based pair adjustment
                ushort classDef1Offset = r.ReadUInt16();
                ushort classDef2Offset = r.ReadUInt16();
                ushort class1Count = r.ReadUInt16();
                ushort class2Count = r.ReadUInt16();

                // Parse class definitions
                var classDef1 = ParseClassDef(data, offset + classDef1Offset);
                var classDef2 = ParseClassDef(data, offset + classDef2Offset);
                var coverageGlyphs = ParseCoverageTable(data, offset + coverageOffset);

                if (classDef1 == null || classDef2 == null || coverageGlyphs == null) return;

                // Build reverse mapping: class -> glyph list
                var class1Glyphs = new Dictionary<ushort, List<ushort>>();
                var class2Glyphs = new Dictionary<ushort, List<ushort>>();

                // Coverage glyphs belong to their class (or class 0)
                foreach (ushort glyph in coverageGlyphs)
                {
                    ushort cls = classDef1.TryGetValue(glyph, out var c) ? c : (ushort)0;
                    if (!class1Glyphs.TryGetValue(cls, out var list))
                    {
                        list = new List<ushort>();
                        class1Glyphs[cls] = list;
                    }
                    list.Add(glyph);
                }

                foreach (var kvp in classDef2)
                {
                    if (!class2Glyphs.TryGetValue(kvp.Value, out var list))
                    {
                        list = new List<ushort>();
                        class2Glyphs[kvp.Value] = list;
                    }
                    list.Add(kvp.Key);
                }

                // Read class pair values
                int recordSize = valueRecord1Size + valueRecord2Size;
                for (ushort c1 = 0; c1 < class1Count; c1++)
                {
                    for (ushort c2 = 0; c2 < class2Count; c2++)
                    {
                        int recordStart = r.Position;
                        r.Position = recordStart + xAdvanceOffset1;
                        short xAdvance = r.ReadInt16();
                        r.Position = recordStart + recordSize;

                        if (xAdvance != 0 &&
                            class1Glyphs.TryGetValue(c1, out var glyphs1) &&
                            class2Glyphs.TryGetValue(c2, out var glyphs2))
                        {
                            foreach (ushort g1 in glyphs1)
                            {
                                foreach (ushort g2 in glyphs2)
                                {
                                    uint key = ((uint)g1 << 16) | g2;
                                    pairs[key] = xAdvance;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static ushort[]? ParseCoverageTable(byte[] data, int offset)
        {
            if (offset + 4 > data.Length) return null;

            var r = new FontReader(data, offset);
            ushort format = r.ReadUInt16();

            if (format == 1)
            {
                ushort glyphCount = r.ReadUInt16();
                var glyphs = new ushort[glyphCount];
                for (int i = 0; i < glyphCount; i++)
                    glyphs[i] = r.ReadUInt16();
                return glyphs;
            }
            else if (format == 2)
            {
                ushort rangeCount = r.ReadUInt16();
                var glyphs = new List<ushort>();
                for (int i = 0; i < rangeCount; i++)
                {
                    ushort startGlyph = r.ReadUInt16();
                    ushort endGlyph = r.ReadUInt16();
                    ushort startCoverageIndex = r.ReadUInt16();
                    for (ushort g = startGlyph; g <= endGlyph; g++)
                        glyphs.Add(g);
                }
                return glyphs.ToArray();
            }

            return null;
        }

        private static Dictionary<ushort, ushort>? ParseClassDef(byte[] data, int offset)
        {
            if (offset + 4 > data.Length) return null;

            var r = new FontReader(data, offset);
            ushort format = r.ReadUInt16();
            var classes = new Dictionary<ushort, ushort>();

            if (format == 1)
            {
                ushort startGlyph = r.ReadUInt16();
                ushort glyphCount = r.ReadUInt16();
                for (int i = 0; i < glyphCount; i++)
                {
                    ushort cls = r.ReadUInt16();
                    if (cls != 0)
                        classes[(ushort)(startGlyph + i)] = cls;
                }
            }
            else if (format == 2)
            {
                ushort rangeCount = r.ReadUInt16();
                for (int i = 0; i < rangeCount; i++)
                {
                    ushort startGlyph = r.ReadUInt16();
                    ushort endGlyph = r.ReadUInt16();
                    ushort cls = r.ReadUInt16();
                    if (cls != 0)
                    {
                        for (ushort g = startGlyph; g <= endGlyph; g++)
                            classes[g] = cls;
                    }
                }
            }

            return classes;
        }

        private static int CountValueRecordBytes(ushort valueFormat)
        {
            int count = 0;
            for (int bit = 0; bit < 8; bit++)
            {
                if ((valueFormat & (1 << bit)) != 0)
                    count += 2;
            }
            return count;
        }

        private static int GetXAdvanceOffset(ushort valueFormat)
        {
            // XAdvance is bit 2 (0x0004). Count bytes for bits before it.
            int offset = 0;
            if ((valueFormat & 0x0001) != 0) offset += 2; // XPlacement
            if ((valueFormat & 0x0002) != 0) offset += 2; // YPlacement
            return offset;
        }

        // ═══════════════════════════════════════════
        // Post table parser
        // ═══════════════════════════════════════════

        private static PostData ParsePost(byte[] data, TableEntry entry)
        {
            var r = new FontReader(data, (int)entry.Offset);
            // version (Fixed 32-bit, skip)
            r.Skip(4);
            // italicAngle: Fixed 16.16 (16-bit integer part + 16-bit fraction)
            short intPart = r.ReadInt16();
            ushort fracPart = r.ReadUInt16();
            float italicAngle = intPart + fracPart / 65536f;
            // underlinePosition, underlineThickness (skip)
            r.Skip(4);
            // isFixedPitch: uint32, nonzero means fixed pitch
            uint isFixedPitch = r.ReadUInt32();

            return new PostData
            {
                ItalicAngle = italicAngle,
                IsFixedPitch = isFixedPitch != 0,
                HasData = true
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

        private struct PostData
        {
            public float ItalicAngle;
            public bool IsFixedPitch;
            public bool HasData;
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
