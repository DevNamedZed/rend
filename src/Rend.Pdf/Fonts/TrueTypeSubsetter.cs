using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace Rend.Pdf.Fonts
{
    /// <summary>
    /// Subsets a TrueType font to include only the glyphs that were actually used.
    /// Produces a valid TrueType font file suitable for embedding as FontFile2.
    /// </summary>
    internal static class TrueTypeSubsetter
    {
        /// <summary>
        /// Subset a TrueType font to include only the specified glyphs.
        /// Always includes glyph 0 (.notdef). Returns the subset font file bytes.
        /// Stateless: each font is subset once per document save. Cross-document
        /// memoization, if ever needed for batch throughput, belongs in a caller-owned
        /// cache — not a process-global static here.
        /// </summary>
        public static byte[] Subset(byte[] originalFont, IReadOnlyCollection<ushort> usedGlyphs)
        {
            if (usedGlyphs.Count == 0)
            {
                return originalFont;
            }
            return SubsetCore(originalFont, usedGlyphs);
        }

        private static byte[] SubsetCore(byte[] originalFont, IReadOnlyCollection<ushort> usedGlyphs)
        {

            // Parse the original font's table directory
            var tables = ParseTableDirectory(originalFont, out ushort numTables, out uint sfVersion);

            // Build the sorted set of glyph IDs to keep (always include .notdef = 0)
            var glyphSet = new SortedSet<ushort> { 0 };
            foreach (var gid in usedGlyphs)
                glyphSet.Add(gid);

            // If the font has composite glyphs, we need to resolve component references
            if (tables.ContainsKey("glyf") && tables.ContainsKey("loca"))
            {
                ResolveCompositeGlyphs(originalFont, tables, glyphSet);
            }

            // Preserve original glyph IDs (no remapping).
            // CIDToGIDMap = Identity means CIDs in the text stream ARE glyph IDs,
            // so the subset must keep glyphs at their original positions.
            ushort maxGid = 0;
            foreach (var gid in glyphSet)
                if (gid > maxGid) maxGid = gid;
            int totalSlots = maxGid + 1;

            // Build subset tables
            var subsetTables = new Dictionary<string, byte[]>();

            // head — copy as-is, we'll fix checksumAdjustment at the end
            if (tables.ContainsKey("head"))
                subsetTables["head"] = CopyTable(originalFont, tables["head"]);

            // hhea — copy, update numberOfHMetrics
            if (tables.ContainsKey("hhea"))
            {
                var hhea = CopyTable(originalFont, tables["hhea"]);
                WriteUInt16BE(hhea, hhea.Length - 2, (ushort)totalSlots);
                subsetTables["hhea"] = hhea;
            }

            // maxp — copy, update numGlyphs
            if (tables.ContainsKey("maxp"))
            {
                var maxp = CopyTable(originalFont, tables["maxp"]);
                WriteUInt16BE(maxp, 4, (ushort)totalSlots);
                subsetTables["maxp"] = maxp;
            }

            // hmtx — preserve glyph IDs, zero-fill unused slots
            if (tables.ContainsKey("hmtx"))
                subsetTables["hmtx"] = SubsetHmtxPreserveIds(originalFont, tables, glyphSet, totalSlots);

            // loca + glyf — preserve glyph IDs, empty entries for unused slots
            if (tables.ContainsKey("glyf") && tables.ContainsKey("loca"))
            {
                SubsetGlyfLocaPreserveIds(originalFont, tables, glyphSet, totalSlots,
                                          out byte[] newGlyf, out byte[] newLoca);
                subsetTables["glyf"] = newGlyf;
                subsetTables["loca"] = newLoca;
            }

            // cmap — pre-built static minimal Format 4 table
            subsetTables["cmap"] = MinimalCmap;

            // name — keep basic name records
            if (tables.ContainsKey("name"))
                subsetTables["name"] = CopyTable(originalFont, tables["name"]);

            // OS/2 — copy as-is
            if (tables.ContainsKey("OS/2"))
                subsetTables["OS/2"] = CopyTable(originalFont, tables["OS/2"]);

            // post — pre-built static minimal version 3.0
            subsetTables["post"] = MinimalPost;

            // cvt, fpgm, prep — copy if present (hinting)
            foreach (var tag in new[] { "cvt ", "fpgm", "prep" })
            {
                if (tables.ContainsKey(tag))
                    subsetTables[tag] = CopyTable(originalFont, tables[tag]);
            }

            // Assemble the new font file
            return AssembleFont(sfVersion, subsetTables);
        }

        private static void ResolveCompositeGlyphs(byte[] font, Dictionary<string, TableRange> tables,
                                                     SortedSet<ushort> glyphSet)
        {
            var locaRange = tables["loca"];
            var glyfRange = tables["glyf"];

            // Determine loca format: check head table indexToLocFormat at offset 50
            var headRange = tables["head"];
            short locaFormat = ReadInt16BE(font, (int)headRange.Offset + 50);
            bool longLoca = locaFormat == 1;

            var maxpRange = tables["maxp"];
            int numGlyphs = ReadUInt16BE(font, (int)maxpRange.Offset + 4);

            // Iteratively resolve composites until no new glyphs added
            var toCheck = new Queue<ushort>(glyphSet);
            while (toCheck.Count > 0)
            {
                ushort gid = toCheck.Dequeue();
                if (gid >= numGlyphs) continue;

                long glyphOffset = GetGlyphOffset(font, locaRange, longLoca, gid);
                long nextOffset = GetGlyphOffset(font, locaRange, longLoca, (ushort)(gid + 1));

                if (nextOffset <= glyphOffset) continue; // empty glyph

                int gStart = (int)(glyfRange.Offset + glyphOffset);
                if (gStart + 10 > font.Length) continue;

                short numberOfContours = ReadInt16BE(font, gStart);
                if (numberOfContours >= 0) continue; // simple glyph, not composite

                // Composite glyph — parse component records
                int pos = gStart + 10; // skip header (numberOfContours + bbox)
                const ushort ARG_1_AND_2_ARE_WORDS = 0x0001;
                const ushort MORE_COMPONENTS = 0x0020;

                while (pos + 4 <= font.Length)
                {
                    ushort flags = ReadUInt16BE(font, pos);
                    ushort componentGid = ReadUInt16BE(font, pos + 2);
                    pos += 4;

                    if (glyphSet.Add(componentGid))
                        toCheck.Enqueue(componentGid);

                    // Skip arguments
                    if ((flags & ARG_1_AND_2_ARE_WORDS) != 0)
                        pos += 4;
                    else
                        pos += 2;

                    // Skip transform
                    if ((flags & 0x0008) != 0) pos += 2;       // WE_HAVE_A_SCALE
                    else if ((flags & 0x0040) != 0) pos += 4;  // WE_HAVE_AN_X_AND_Y_SCALE
                    else if ((flags & 0x0080) != 0) pos += 8;  // WE_HAVE_A_TWO_BY_TWO

                    if ((flags & MORE_COMPONENTS) == 0) break;
                }
            }
        }

        private static long GetGlyphOffset(byte[] font, TableRange locaRange, bool longLoca, ushort glyphIndex)
        {
            int locaStart = (int)locaRange.Offset;
            if (longLoca)
            {
                int idx = locaStart + glyphIndex * 4;
                if (idx + 3 >= font.Length) return 0;
                return ReadUInt32BE(font, idx);
            }
            else
            {
                int idx = locaStart + glyphIndex * 2;
                if (idx + 1 >= font.Length) return 0;
                return (long)ReadUInt16BE(font, idx) * 2;
            }
        }

        private static byte[] SubsetHmtxPreserveIds(byte[] font, Dictionary<string, TableRange> tables,
                                                      SortedSet<ushort> glyphSet, int totalSlots)
        {
            var hmtxRange = tables["hmtx"];
            int hmtxStart = (int)hmtxRange.Offset;

            var hheaRange = tables["hhea"];
            int origHMetrics = ReadUInt16BE(font, (int)hheaRange.Offset + hheaRange.Length - 2);

            // Each slot gets 4 bytes (advanceWidth + lsb). Unused slots are zero-filled.
            var result = new byte[totalSlots * 4];
            for (int gid = 0; gid < totalSlots; gid++)
            {
                if (!glyphSet.Contains((ushort)gid))
                    continue; // leave as zeros

                int srcOffset;
                if (gid < origHMetrics)
                    srcOffset = hmtxStart + gid * 4;
                else
                    srcOffset = hmtxStart + (origHMetrics - 1) * 4;

                if (srcOffset + 3 < font.Length)
                {
                    result[gid * 4] = font[srcOffset];
                    result[gid * 4 + 1] = font[srcOffset + 1];
                    result[gid * 4 + 2] = font[srcOffset + 2];
                    result[gid * 4 + 3] = font[srcOffset + 3];
                }
            }

            return result;
        }

        private static byte[] SubsetHmtx(byte[] font, Dictionary<string, TableRange> tables,
                                           List<ushort> newToOld)
        {
            var hmtxRange = tables["hmtx"];
            int hmtxStart = (int)hmtxRange.Offset;

            // Each hMetric is 4 bytes: advanceWidth (2) + lsb (2)
            // Read original numberOfHMetrics from hhea
            var hheaRange = tables["hhea"];
            int origHMetrics = ReadUInt16BE(font, (int)hheaRange.Offset + hheaRange.Length - 2);

            var result = new byte[newToOld.Count * 4];
            for (int i = 0; i < newToOld.Count; i++)
            {
                ushort oldGid = newToOld[i];
                int srcOffset;
                if (oldGid < origHMetrics)
                {
                    srcOffset = hmtxStart + oldGid * 4;
                }
                else
                {
                    // Use last full metric's advance width + per-glyph lsb
                    srcOffset = hmtxStart + (origHMetrics - 1) * 4;
                }

                if (srcOffset + 3 < font.Length)
                {
                    result[i * 4] = font[srcOffset];
                    result[i * 4 + 1] = font[srcOffset + 1];
                    result[i * 4 + 2] = font[srcOffset + 2];
                    result[i * 4 + 3] = font[srcOffset + 3];
                }
            }

            return result;
        }

        private static void SubsetGlyfLocaPreserveIds(byte[] font, Dictionary<string, TableRange> tables,
                                                       SortedSet<ushort> glyphSet, int totalSlots,
                                                       out byte[] newGlyf, out byte[] newLoca)
        {
            var locaRange = tables["loca"];
            var glyfRange = tables["glyf"];
            var headRange = tables["head"];
            var maxpRange = tables["maxp"];

            short locaFormat = ReadInt16BE(font, (int)headRange.Offset + 50);
            bool longLoca = locaFormat == 1;
            int origNumGlyphs = ReadUInt16BE(font, (int)maxpRange.Offset + 4);

            // Pass 1: Calculate total glyf size
            int totalGlyfSize = 0;
            for (int gid = 0; gid < totalSlots; gid++)
            {
                if (!glyphSet.Contains((ushort)gid) || gid >= origNumGlyphs)
                    continue;

                long offset = GetGlyphOffset(font, locaRange, longLoca, (ushort)gid);
                long nextOffset = (gid + 1 <= origNumGlyphs)
                    ? GetGlyphOffset(font, locaRange, longLoca, (ushort)(gid + 1))
                    : offset;

                int glyphLen = (int)(nextOffset - offset);
                if (glyphLen <= 0) continue;

                int srcStart = (int)(glyfRange.Offset + offset);
                if (srcStart + glyphLen > font.Length) continue;

                totalGlyfSize += glyphLen + ((4 - (glyphLen % 4)) % 4);
            }

            // Pass 2: Write glyph data, preserving original glyph ID positions
            newGlyf = new byte[totalGlyfSize];
            var locaOffsets = new uint[totalSlots + 1];
            int writePos = 0;

            for (int gid = 0; gid < totalSlots; gid++)
            {
                locaOffsets[gid] = (uint)writePos;

                if (!glyphSet.Contains((ushort)gid) || gid >= origNumGlyphs)
                    continue; // empty entry — loca points to same offset as next

                long offset = GetGlyphOffset(font, locaRange, longLoca, (ushort)gid);
                long nextOffset = (gid + 1 <= origNumGlyphs)
                    ? GetGlyphOffset(font, locaRange, longLoca, (ushort)(gid + 1))
                    : offset;

                int glyphLen = (int)(nextOffset - offset);
                if (glyphLen <= 0) continue;

                int srcStart = (int)(glyfRange.Offset + offset);
                if (srcStart + glyphLen > font.Length) continue;

                // Copy glyph data (no remapping needed — glyph IDs are preserved)
                Buffer.BlockCopy(font, srcStart, newGlyf, writePos, glyphLen);
                writePos += glyphLen;

                int padding = (4 - (glyphLen % 4)) % 4;
                writePos += padding;
            }

            locaOffsets[totalSlots] = (uint)writePos;

            // Build long-format loca table
            newLoca = new byte[(totalSlots + 1) * 4];
            for (int i = 0; i <= totalSlots; i++)
            {
                WriteUInt32BE(newLoca, i * 4, locaOffsets[i]);
            }
        }

        private static void SubsetGlyfLoca(byte[] font, Dictionary<string, TableRange> tables,
                                            List<ushort> newToOld, Dictionary<ushort, ushort> oldToNew,
                                            out byte[] newGlyf, out byte[] newLoca)
        {
            var locaRange = tables["loca"];
            var glyfRange = tables["glyf"];
            var headRange = tables["head"];
            var maxpRange = tables["maxp"];

            short locaFormat = ReadInt16BE(font, (int)headRange.Offset + 50);
            bool longLoca = locaFormat == 1;
            int origNumGlyphs = ReadUInt16BE(font, (int)maxpRange.Offset + 4);

            // Pass 1: Calculate total size needed for glyf table
            int totalGlyfSize = 0;
            for (int i = 0; i < newToOld.Count; i++)
            {
                ushort oldGid = newToOld[i];
                if (oldGid >= origNumGlyphs) continue;

                long offset = GetGlyphOffset(font, locaRange, longLoca, oldGid);
                long nextOffset = (oldGid + 1 <= origNumGlyphs)
                    ? GetGlyphOffset(font, locaRange, longLoca, (ushort)(oldGid + 1))
                    : offset;

                int glyphLen = (int)(nextOffset - offset);
                if (glyphLen <= 0) continue;

                int srcStart = (int)(glyfRange.Offset + offset);
                if (srcStart + glyphLen > font.Length) continue;

                totalGlyfSize += glyphLen + ((4 - (glyphLen % 4)) % 4); // padded
            }

            // Pass 2: Write glyph data directly into pre-allocated array
            newGlyf = new byte[totalGlyfSize];
            var locaOffsets = new uint[newToOld.Count + 1];
            int writePos = 0;

            for (int i = 0; i < newToOld.Count; i++)
            {
                locaOffsets[i] = (uint)writePos;
                ushort oldGid = newToOld[i];

                if (oldGid >= origNumGlyphs) continue;

                long offset = GetGlyphOffset(font, locaRange, longLoca, oldGid);
                long nextOffset = (oldGid + 1 <= origNumGlyphs)
                    ? GetGlyphOffset(font, locaRange, longLoca, (ushort)(oldGid + 1))
                    : offset;

                int glyphLen = (int)(nextOffset - offset);
                if (glyphLen <= 0) continue;

                int srcStart = (int)(glyfRange.Offset + offset);
                if (srcStart + glyphLen > font.Length) continue;

                // Copy glyph data directly into output
                Buffer.BlockCopy(font, srcStart, newGlyf, writePos, glyphLen);

                // If composite, remap component glyph IDs in-place
                short numberOfContours = (short)((newGlyf[writePos] << 8) | newGlyf[writePos + 1]);
                if (numberOfContours < 0)
                {
                    RemapCompositeGlyphInPlace(newGlyf, writePos, glyphLen, oldToNew);
                }

                writePos += glyphLen;

                // Pad to 4-byte boundary (array is zero-initialized)
                int padding = (4 - (glyphLen % 4)) % 4;
                writePos += padding;
            }

            locaOffsets[newToOld.Count] = (uint)writePos;

            // Build long-format loca table
            newLoca = new byte[(newToOld.Count + 1) * 4];
            for (int i = 0; i <= newToOld.Count; i++)
            {
                WriteUInt32BE(newLoca, i * 4, locaOffsets[i]);
            }
        }

        private static void RemapCompositeGlyphInPlace(byte[] data, int glyphStart, int glyphLen, Dictionary<ushort, ushort> oldToNew)
        {
            int pos = glyphStart + 10; // skip header
            int end = glyphStart + glyphLen;
            const ushort ARG_1_AND_2_ARE_WORDS = 0x0001;
            const ushort MORE_COMPONENTS = 0x0020;

            while (pos + 4 <= end)
            {
                ushort flags = (ushort)((data[pos] << 8) | data[pos + 1]);
                ushort oldComponentGid = (ushort)((data[pos + 2] << 8) | data[pos + 3]);

                if (oldToNew.TryGetValue(oldComponentGid, out ushort newGid))
                {
                    data[pos + 2] = (byte)(newGid >> 8);
                    data[pos + 3] = (byte)(newGid & 0xFF);
                }

                pos += 4;

                if ((flags & ARG_1_AND_2_ARE_WORDS) != 0) pos += 4;
                else pos += 2;

                if ((flags & 0x0008) != 0) pos += 2;
                else if ((flags & 0x0040) != 0) pos += 4;
                else if ((flags & 0x0080) != 0) pos += 8;

                if ((flags & MORE_COMPONENTS) == 0) break;
            }
        }

        private static void RemapCompositeGlyph(byte[] glyphData, Dictionary<ushort, ushort> oldToNew)
        {
            int pos = 10; // skip header
            const ushort ARG_1_AND_2_ARE_WORDS = 0x0001;
            const ushort MORE_COMPONENTS = 0x0020;

            while (pos + 4 <= glyphData.Length)
            {
                ushort flags = (ushort)((glyphData[pos] << 8) | glyphData[pos + 1]);
                ushort oldComponentGid = (ushort)((glyphData[pos + 2] << 8) | glyphData[pos + 3]);

                // Remap the glyph ID
                if (oldToNew.TryGetValue(oldComponentGid, out ushort newGid))
                {
                    glyphData[pos + 2] = (byte)(newGid >> 8);
                    glyphData[pos + 3] = (byte)(newGid & 0xFF);
                }

                pos += 4;

                if ((flags & ARG_1_AND_2_ARE_WORDS) != 0) pos += 4;
                else pos += 2;

                if ((flags & 0x0008) != 0) pos += 2;
                else if ((flags & 0x0040) != 0) pos += 4;
                else if ((flags & 0x0080) != 0) pos += 8;

                if ((flags & MORE_COMPONENTS) == 0) break;
            }
        }

        // Pre-built minimal cmap: Format 4 with one segment (0xFFFF terminator only).
        // Actual character mapping uses Identity-H CMap + ToUnicode.
        private static readonly byte[] MinimalCmap =
        {
            // cmap header: version=0, numSubtables=1
            0, 0, 0, 1,
            // Subtable entry: platform=3 (Windows), encoding=1 (BMP), offset=12
            0, 3, 0, 1, 0, 0, 0, 12,
            // Format 4: format=4, length=14, language=0, segCountX2=2, searchRange=2, entrySelector=0, rangeShift=0
            0, 4, 0, 14, 0, 0, 0, 2, 0, 2, 0, 0, 0, 0,
            // endCode=0xFFFF, reservedPad=0, startCode=0xFFFF, idDelta=1, idRangeOffset=0
            0xFF, 0xFF, 0, 0, 0xFF, 0xFF, 0, 1, 0, 0
        };

        // Pre-built minimal post table version 3.0 — no glyph names, all zeros except version.
        private static readonly byte[] MinimalPost =
        {
            0, 3, 0, 0, // version 3.0
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        private static byte[] AssembleFont(uint sfVersion, Dictionary<string, byte[]> tables)
        {
            int numTables = tables.Count;

            // Calculate searchRange, entrySelector, rangeShift
            int searchRange = 1;
            int entrySelector = 0;
            while (searchRange * 2 <= numTables)
            {
                searchRange *= 2;
                entrySelector++;
            }
            searchRange *= 16;
            int rangeShift = numTables * 16 - searchRange;

            int headerSize = 12 + numTables * 16;
            int dataOffset = headerSize;

            // Calculate table data offsets (pad each to 4-byte boundary)
            var tableOrder = new List<string>(tables.Keys);
            tableOrder.Sort(StringComparer.Ordinal);

            var tableOffsets = new Dictionary<string, int>();
            int currentOffset = dataOffset;
            foreach (var tag in tableOrder)
            {
                tableOffsets[tag] = currentOffset;
                int len = tables[tag].Length;
                currentOffset += len + ((4 - (len % 4)) % 4); // pad to 4
            }

            int totalSize = currentOffset;
            var result = new byte[totalSize];

            // Write offset table header
            int pos = 0;
            WriteUInt32BE(result, pos, sfVersion); pos += 4;
            WriteUInt16BE(result, pos, (ushort)numTables); pos += 2;
            WriteUInt16BE(result, pos, (ushort)searchRange); pos += 2;
            WriteUInt16BE(result, pos, (ushort)entrySelector); pos += 2;
            WriteUInt16BE(result, pos, (ushort)rangeShift); pos += 2;

            // Write table directory entries
            foreach (var tag in tableOrder)
            {
                var data = tables[tag];
                // Tag (4 bytes ASCII)
                for (int i = 0; i < 4; i++)
                    result[pos + i] = i < tag.Length ? (byte)tag[i] : (byte)' ';
                pos += 4;

                // Checksum
                uint checksum = CalcTableChecksum(data);
                WriteUInt32BE(result, pos, checksum); pos += 4;

                // Offset
                WriteUInt32BE(result, pos, (uint)tableOffsets[tag]); pos += 4;

                // Length
                WriteUInt32BE(result, pos, (uint)data.Length); pos += 4;
            }

            // Write table data
            foreach (var tag in tableOrder)
            {
                var data = tables[tag];
                Buffer.BlockCopy(data, 0, result, tableOffsets[tag], data.Length);
            }

            // Fix head table checksumAdjustment
            if (tableOffsets.ContainsKey("head") && tables["head"].Length >= 12)
            {
                uint fileChecksum = CalcTableChecksum(result);
                uint adjustment = 0xB1B0AFBA - fileChecksum;
                int headOffset = tableOffsets["head"];
                WriteUInt32BE(result, headOffset + 8, adjustment);
            }

            // Fix head indexToLocFormat to 1 (long) since we always write long loca
            if (tableOffsets.ContainsKey("head") && tables["head"].Length >= 52)
            {
                int headOffset = tableOffsets["head"];
                WriteUInt16BE(result, headOffset + 50, 1); // indexToLocFormat = 1 (long)
            }

            return result;
        }

        // ═══════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════

        private struct TableRange
        {
            public uint Offset;
            public int Length;
            public TableRange(uint offset, int length) { Offset = offset; Length = length; }
        }

        private static Dictionary<string, TableRange> ParseTableDirectory(byte[] font,
            out ushort numTables, out uint sfVersion)
        {
            sfVersion = ReadUInt32BE(font, 0);
            numTables = ReadUInt16BE(font, 4);

            var tables = new Dictionary<string, TableRange>();
            int pos = 12;
            for (int i = 0; i < numTables; i++)
            {
                string tag = System.Text.Encoding.ASCII.GetString(font, pos, 4);
                pos += 4;
                pos += 4; // checksum
                uint offset = ReadUInt32BE(font, pos); pos += 4;
                int length = (int)ReadUInt32BE(font, pos); pos += 4;
                tables[tag] = new TableRange(offset, length);
            }

            return tables;
        }

        private static byte[] CopyTable(byte[] font, TableRange range)
        {
            var data = new byte[range.Length];
            Buffer.BlockCopy(font, (int)range.Offset, data, 0, range.Length);
            return data;
        }

        private static uint CalcTableChecksum(byte[] data)
        {
            uint sum = 0;
            int len = data.Length;
            int fullLongs = len / 4;

            // Process 4-byte aligned blocks without per-byte bounds checks
            for (int i = 0; i < fullLongs; i++)
            {
                int off = i * 4;
                sum += ((uint)data[off] << 24) | ((uint)data[off + 1] << 16) |
                       ((uint)data[off + 2] << 8) | data[off + 3];
            }

            // Handle remaining 1-3 bytes
            int tail = fullLongs * 4;
            if (tail < len)
            {
                uint val = 0;
                if (tail < len) val |= (uint)data[tail] << 24;
                if (tail + 1 < len) val |= (uint)data[tail + 1] << 16;
                if (tail + 2 < len) val |= (uint)data[tail + 2] << 8;
                sum += val;
            }

            return sum;
        }

        private static ushort ReadUInt16BE(byte[] data, int offset)
            => BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset));

        private static short ReadInt16BE(byte[] data, int offset)
            => BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(offset));

        private static uint ReadUInt32BE(byte[] data, int offset)
            => BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));

        private static void WriteUInt16BE(byte[] data, int offset, ushort value)
            => BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(offset), value);

        private static void WriteUInt32BE(byte[] data, int offset, uint value)
        {
            BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(offset), value);
        }
    }
}
