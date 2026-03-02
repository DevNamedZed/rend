using System;
using System.Collections.Generic;
using Rend.Css;
using Rend.Fonts;
using Xunit;

namespace Rend.Tests.Fonts
{
    public class VariableFontTests
    {
        // ═══════════════════════════════════════════
        // FontVariationAxis
        // ═══════════════════════════════════════════

        [Fact]
        public void Axis_Constructor()
        {
            var axis = new FontVariationAxis("wght", 100f, 400f, 900f, "Weight");

            Assert.Equal("wght", axis.Tag);
            Assert.Equal(100f, axis.MinValue);
            Assert.Equal(400f, axis.DefaultValue);
            Assert.Equal(900f, axis.MaxValue);
            Assert.Equal("Weight", axis.Name);
        }

        [Fact]
        public void Axis_IsRegistered_LowercaseTag()
        {
            var wght = new FontVariationAxis("wght", 100f, 400f, 900f, "Weight");
            Assert.True(wght.IsRegistered);

            var wdth = new FontVariationAxis("wdth", 75f, 100f, 125f, "Width");
            Assert.True(wdth.IsRegistered);
        }

        [Fact]
        public void Axis_IsRegistered_UppercaseTag_Custom()
        {
            var custom = new FontVariationAxis("GRAD", 0f, 0f, 1f, "Grade");
            Assert.False(custom.IsRegistered);
        }

        [Fact]
        public void Axis_Contains_InRange()
        {
            var axis = new FontVariationAxis("wght", 100f, 400f, 900f, "Weight");

            Assert.True(axis.Contains(100f));
            Assert.True(axis.Contains(400f));
            Assert.True(axis.Contains(700f));
            Assert.True(axis.Contains(900f));
        }

        [Fact]
        public void Axis_Contains_OutOfRange()
        {
            var axis = new FontVariationAxis("wght", 100f, 400f, 900f, "Weight");

            Assert.False(axis.Contains(50f));
            Assert.False(axis.Contains(901f));
        }

        [Fact]
        public void Axis_Clamp()
        {
            var axis = new FontVariationAxis("wght", 100f, 400f, 900f, "Weight");

            Assert.Equal(100f, axis.Clamp(50f));
            Assert.Equal(400f, axis.Clamp(400f));
            Assert.Equal(900f, axis.Clamp(1000f));
            Assert.Equal(500f, axis.Clamp(500f));
        }

        [Fact]
        public void Axis_Equality()
        {
            var a = new FontVariationAxis("wght", 100f, 400f, 900f, "Weight");
            var b = new FontVariationAxis("wght", 100f, 400f, 900f, "Weight");
            var c = new FontVariationAxis("wdth", 75f, 100f, 125f, "Width");

            Assert.Equal(a, b);
            Assert.NotEqual(a, c);
            Assert.True(a == b);
            Assert.True(a != c);
        }

        [Fact]
        public void Axis_ToString()
        {
            var axis = new FontVariationAxis("wght", 100f, 400f, 900f, "Weight");
            string s = axis.ToString();

            Assert.Contains("wght", s);
            Assert.Contains("100", s);
            Assert.Contains("900", s);
        }

        // ═══════════════════════════════════════════
        // FontNamedInstance
        // ═══════════════════════════════════════════

        [Fact]
        public void NamedInstance_Constructor()
        {
            var coords = new Dictionary<string, float> { ["wght"] = 700f, ["wdth"] = 100f };
            var instance = new FontNamedInstance("Bold", coords);

            Assert.Equal("Bold", instance.Name);
            Assert.Equal(700f, instance.Coordinates["wght"]);
            Assert.Equal(100f, instance.Coordinates["wdth"]);
        }

        [Fact]
        public void NamedInstance_ToString()
        {
            var coords = new Dictionary<string, float> { ["wght"] = 700f };
            var instance = new FontNamedInstance("Bold", coords);

            Assert.Equal("Bold", instance.ToString());
        }

        // ═══════════════════════════════════════════
        // FontEntry.IsVariableFont
        // ═══════════════════════════════════════════

        [Fact]
        public void FontEntry_StaticFont_IsNotVariable()
        {
            var descriptor = new FontDescriptor("TestFont", 400f);
            var metrics = new FontMetricsInfo(800, -200, 0, 1000, 700, 500);
            var entry = new FontEntry(descriptor, new byte[0], metrics, "TestFont", null);

            Assert.False(entry.IsVariableFont);
            Assert.Null(entry.VariationAxes);
            Assert.Null(entry.NamedInstances);
        }

        // ═══════════════════════════════════════════
        // Font Matching with Variable Fonts
        // ═══════════════════════════════════════════

        private static FontEntry CreateVariableEntry(string family, float weight, FontVariationAxis[] axes)
        {
            var descriptor = new FontDescriptor(family, weight);
            var metrics = new FontMetricsInfo(800, -200, 0, 1000, 700, 500);
            var entry = new FontEntry(descriptor, new byte[0], metrics, family, null);
            entry.VariationAxes = new List<FontVariationAxis>(axes).AsReadOnly();
            return entry;
        }

        private static FontEntry CreateStaticEntry(string family, float weight)
        {
            var descriptor = new FontDescriptor(family, weight);
            var metrics = new FontMetricsInfo(800, -200, 0, 1000, 700, 500);
            return new FontEntry(descriptor, new byte[0], metrics, family, null);
        }

        [Fact]
        public void Matching_VariableFont_WeightAxis_MatchesAnyWeight()
        {
            var varFont = CreateVariableEntry("TestFont", 400f, new[]
            {
                new FontVariationAxis("wght", 100f, 400f, 900f, "Weight")
            });
            var candidates = new List<FontEntry> { varFont };

            // Request weight 700 — variable font covers 100-900, should match
            var requested = new FontDescriptor("TestFont", 700f);
            var result = FontMatchingAlgorithm.FindBestMatch(requested, candidates);

            Assert.NotNull(result);
            Assert.Same(varFont, result);
        }

        [Fact]
        public void Matching_VariableFont_PreferredOverStaticFont()
        {
            var staticFont = CreateStaticEntry("TestFont", 400f);
            var varFont = CreateVariableEntry("TestFont", 400f, new[]
            {
                new FontVariationAxis("wght", 100f, 400f, 900f, "Weight")
            });
            var candidates = new List<FontEntry> { staticFont, varFont };

            // Request weight 600 — variable font covers it exactly via axis
            var requested = new FontDescriptor("TestFont", 600f);
            var result = FontMatchingAlgorithm.FindBestMatch(requested, candidates);

            Assert.NotNull(result);
            Assert.Same(varFont, result);
        }

        [Fact]
        public void Matching_VariableFont_WeightOutOfRange_FallsBack()
        {
            var varFont = CreateVariableEntry("TestFont", 400f, new[]
            {
                new FontVariationAxis("wght", 300f, 400f, 700f, "Weight")
            });
            var staticBold = CreateStaticEntry("TestFont", 900f);
            var candidates = new List<FontEntry> { varFont, staticBold };

            // Request weight 900 — variable font only covers 300-700
            var requested = new FontDescriptor("TestFont", 900f);
            var result = FontMatchingAlgorithm.FindBestMatch(requested, candidates);

            Assert.NotNull(result);
            Assert.Same(staticBold, result);
        }

        [Fact]
        public void Matching_VariableFont_WidthAxis()
        {
            var varFont = CreateVariableEntry("TestFont", 400f, new[]
            {
                new FontVariationAxis("wght", 100f, 400f, 900f, "Weight"),
                new FontVariationAxis("wdth", 75f, 100f, 125f, "Width")
            });
            var candidates = new List<FontEntry> { varFont };

            // Request condensed stretch (75%)
            var requested = new FontDescriptor("TestFont", 400f, CssFontStyle.Normal, 75f);
            var result = FontMatchingAlgorithm.FindBestMatch(requested, candidates);

            Assert.NotNull(result);
            Assert.Same(varFont, result);
        }

        [Fact]
        public void Matching_StaticFont_StillWorks()
        {
            var regular = CreateStaticEntry("TestFont", 400f);
            var bold = CreateStaticEntry("TestFont", 700f);
            var candidates = new List<FontEntry> { regular, bold };

            var requested = new FontDescriptor("TestFont", 700f);
            var result = FontMatchingAlgorithm.FindBestMatch(requested, candidates);

            Assert.NotNull(result);
            Assert.Equal(700f, result!.Descriptor.Weight);
        }

        // ═══════════════════════════════════════════
        // fvar Table Parsing (synthetic font data)
        // ═══════════════════════════════════════════

        [Fact]
        public void ParseFvar_SyntheticFont_ExtractsAxes()
        {
            // Build a minimal TrueType font with an fvar table
            var fontData = BuildMinimalVariableFont(new[]
            {
                ("wght", 100f, 400f, 900f, (ushort)256),
                ("wdth", 75f, 100f, 125f, (ushort)257)
            });

            var parsed = new Rend.Fonts.Internal.OpenTypeFontData(fontData);

            Assert.True(parsed.IsVariableFont);
            Assert.NotNull(parsed.VariationAxes);
            Assert.Equal(2, parsed.VariationAxes!.Count);

            Assert.Equal("wght", parsed.VariationAxes[0].Tag);
            Assert.Equal(100f, parsed.VariationAxes[0].MinValue);
            Assert.Equal(400f, parsed.VariationAxes[0].DefaultValue);
            Assert.Equal(900f, parsed.VariationAxes[0].MaxValue);

            Assert.Equal("wdth", parsed.VariationAxes[1].Tag);
            Assert.Equal(75f, parsed.VariationAxes[1].MinValue);
            Assert.Equal(100f, parsed.VariationAxes[1].DefaultValue);
            Assert.Equal(125f, parsed.VariationAxes[1].MaxValue);
        }

        [Fact]
        public void ParseFvar_StaticFont_NoAxes()
        {
            // Build a minimal static font (no fvar table)
            var fontData = BuildMinimalStaticFont();

            var parsed = new Rend.Fonts.Internal.OpenTypeFontData(fontData);

            Assert.False(parsed.IsVariableFont);
            Assert.Null(parsed.VariationAxes);
        }

        // ═══════════════════════════════════════════
        // Synthetic font builder helpers
        // ═══════════════════════════════════════════

        /// <summary>
        /// Builds a minimal valid TrueType font with head, hhea, maxp, hmtx, OS/2, name, cmap, and optionally fvar tables.
        /// This is the minimum set of tables needed for OpenTypeFontData to parse without errors.
        /// </summary>
        private static byte[] BuildMinimalVariableFont(
            (string tag, float min, float def, float max, ushort nameId)[] axes)
        {
            // Start with the minimal static tables
            var tables = BuildMinimalTables();

            // Add fvar table
            int axisCount = axes.Length;
            int axisSize = 20; // standard axis record size
            int instanceCount = 0;
            int instanceSize = 4 + axisCount * 4;
            int fvarSize = 16 + axisCount * axisSize + instanceCount * instanceSize;

            var fvar = new byte[fvarSize];
            // Header
            WriteUInt16(fvar, 0, 1); // majorVersion
            WriteUInt16(fvar, 2, 0); // minorVersion
            WriteUInt16(fvar, 4, 16); // axesArrayOffset
            WriteUInt16(fvar, 6, 2); // reserved
            WriteUInt16(fvar, 8, (ushort)axisCount);
            WriteUInt16(fvar, 10, (ushort)axisSize);
            WriteUInt16(fvar, 12, (ushort)instanceCount);
            WriteUInt16(fvar, 14, (ushort)instanceSize);

            // Axis records
            for (int i = 0; i < axisCount; i++)
            {
                int offset = 16 + i * axisSize;
                // tag (4 bytes)
                for (int j = 0; j < 4; j++)
                    fvar[offset + j] = (byte)axes[i].tag[j];
                // minValue (Fixed 16.16)
                WriteFixed(fvar, offset + 4, axes[i].min);
                // defaultValue (Fixed 16.16)
                WriteFixed(fvar, offset + 8, axes[i].def);
                // maxValue (Fixed 16.16)
                WriteFixed(fvar, offset + 12, axes[i].max);
                // flags
                WriteUInt16(fvar, offset + 16, 0);
                // axisNameID
                WriteUInt16(fvar, offset + 18, axes[i].nameId);
            }

            tables["fvar"] = fvar;

            return AssembleFont(tables);
        }

        private static byte[] BuildMinimalStaticFont()
        {
            return AssembleFont(BuildMinimalTables());
        }

        private static Dictionary<string, byte[]> BuildMinimalTables()
        {
            var tables = new Dictionary<string, byte[]>();

            // head table (54 bytes)
            var head = new byte[54];
            WriteUInt32(head, 0, 0x00010000); // version
            WriteUInt16(head, 18, 1000); // unitsPerEm
            tables["head"] = head;

            // hhea table (36 bytes)
            var hhea = new byte[36];
            WriteUInt32(hhea, 0, 0x00010000); // version
            WriteInt16(hhea, 4, 800); // ascent
            WriteInt16(hhea, 6, -200); // descent
            WriteUInt16(hhea, 34, 1); // numberOfHMetrics
            tables["hhea"] = hhea;

            // maxp table (6 bytes for TrueType)
            var maxp = new byte[6];
            WriteUInt32(maxp, 0, 0x00010000); // version
            WriteUInt16(maxp, 4, 1); // numGlyphs
            tables["maxp"] = maxp;

            // hmtx table (4 bytes for 1 glyph)
            var hmtx = new byte[4];
            WriteUInt16(hmtx, 0, 600); // advanceWidth
            WriteInt16(hmtx, 2, 0); // lsb
            tables["hmtx"] = hmtx;

            // OS/2 table (96 bytes, version 4)
            var os2 = new byte[96];
            WriteUInt16(os2, 0, 4); // version
            WriteUInt16(os2, 4, 400); // usWeightClass
            WriteInt16(os2, 68, 800); // sTypoAscender
            WriteInt16(os2, 70, -200); // sTypoDescender
            WriteInt16(os2, 72, 0); // sTypoLineGap
            WriteInt16(os2, 86, 500); // sxHeight
            WriteInt16(os2, 88, 700); // sCapHeight
            tables["OS/2"] = os2;

            // name table (minimal, with family name "TestVarFont")
            tables["name"] = BuildMinimalNameTable("TestVarFont");

            // cmap table (minimal format 4 with one segment)
            tables["cmap"] = BuildMinimalCmapTable();

            return tables;
        }

        private static byte[] BuildMinimalNameTable(string familyName)
        {
            // Encode family name as UTF-16BE
            var nameBytes = new byte[familyName.Length * 2];
            for (int i = 0; i < familyName.Length; i++)
            {
                nameBytes[i * 2] = (byte)(familyName[i] >> 8);
                nameBytes[i * 2 + 1] = (byte)(familyName[i] & 0xFF);
            }

            int headerSize = 6;
            int recordSize = 12;
            int numRecords = 1; // just family name (nameID=1)
            int storageOffset = headerSize + numRecords * recordSize;
            int totalSize = storageOffset + nameBytes.Length;

            var name = new byte[totalSize];
            WriteUInt16(name, 0, 0); // format
            WriteUInt16(name, 2, (ushort)numRecords);
            WriteUInt16(name, 4, (ushort)storageOffset);

            // Record: platform=3 (Windows), encoding=1 (Unicode BMP), language=0x0409, nameID=1
            int recOff = headerSize;
            WriteUInt16(name, recOff, 3); // platformID
            WriteUInt16(name, recOff + 2, 1); // encodingID
            WriteUInt16(name, recOff + 4, 0x0409); // languageID
            WriteUInt16(name, recOff + 6, 1); // nameID (Family)
            WriteUInt16(name, recOff + 8, (ushort)nameBytes.Length);
            WriteUInt16(name, recOff + 10, 0); // offset into storage

            Buffer.BlockCopy(nameBytes, 0, name, storageOffset, nameBytes.Length);

            return name;
        }

        private static byte[] BuildMinimalCmapTable()
        {
            // Minimal cmap with format 4, one segment (0xFFFF sentinel only)
            // Format 4 needs: header (14 bytes) + endCode[1] (2) + reservedPad (2) +
            //   startCode[1] (2) + idDelta[1] (2) + idRangeOffset[1] (2) = 24 bytes
            int format4Size = 24;
            int f4Offset = 12; // cmap header (4) + 1 subtable record (8)
            int totalSize = f4Offset + format4Size;

            var cmap = new byte[totalSize];
            WriteUInt16(cmap, 0, 0); // version
            WriteUInt16(cmap, 2, 1); // numSubtables

            // Subtable record: platform=3, encoding=1, offset relative to cmap start
            WriteUInt16(cmap, 4, 3); // platformID
            WriteUInt16(cmap, 6, 1); // encodingID
            WriteUInt32(cmap, 8, (uint)f4Offset);

            // Format 4 header
            WriteUInt16(cmap, f4Offset + 0, 4); // format
            WriteUInt16(cmap, f4Offset + 2, (ushort)format4Size); // length
            WriteUInt16(cmap, f4Offset + 4, 0); // language
            WriteUInt16(cmap, f4Offset + 6, 2); // segCountX2 = 2 (1 segment)
            WriteUInt16(cmap, f4Offset + 8, 2); // searchRange
            WriteUInt16(cmap, f4Offset + 10, 0); // entrySelector
            WriteUInt16(cmap, f4Offset + 12, 0); // rangeShift

            // endCode[0] = 0xFFFF
            WriteUInt16(cmap, f4Offset + 14, 0xFFFF);
            // reservedPad
            WriteUInt16(cmap, f4Offset + 16, 0);
            // startCode[0] = 0xFFFF
            WriteUInt16(cmap, f4Offset + 18, 0xFFFF);
            // idDelta[0] = 1
            WriteUInt16(cmap, f4Offset + 20, 1);
            // idRangeOffset[0] = 0
            WriteUInt16(cmap, f4Offset + 22, 0);

            return cmap;
        }

        private static byte[] AssembleFont(Dictionary<string, byte[]> tables)
        {
            int numTables = tables.Count;
            int headerSize = 12 + numTables * 16; // offset table + table directory

            // Calculate total size
            int totalSize = headerSize;
            foreach (var table in tables)
            {
                totalSize += (table.Value.Length + 3) & ~3; // 4-byte align
            }

            var data = new byte[totalSize];

            // Offset table
            WriteUInt32(data, 0, 0x00010000); // sfVersion (TrueType)
            WriteUInt16(data, 4, (ushort)numTables);
            // searchRange, entrySelector, rangeShift — not critical for parsing
            WriteUInt16(data, 6, 0);
            WriteUInt16(data, 8, 0);
            WriteUInt16(data, 10, 0);

            // Write table directory and data
            int dirOffset = 12;
            int dataOffset = headerSize;

            foreach (var kvp in tables)
            {
                string tag = kvp.Key;
                byte[] tableData = kvp.Value;

                // Tag (4 bytes)
                for (int i = 0; i < 4; i++)
                    data[dirOffset + i] = i < tag.Length ? (byte)tag[i] : (byte)' ';

                // Checksum (not validated by our parser)
                WriteUInt32(data, dirOffset + 4, 0);
                // Offset
                WriteUInt32(data, dirOffset + 8, (uint)dataOffset);
                // Length
                WriteUInt32(data, dirOffset + 12, (uint)tableData.Length);

                Buffer.BlockCopy(tableData, 0, data, dataOffset, tableData.Length);

                dirOffset += 16;
                dataOffset += (tableData.Length + 3) & ~3; // 4-byte align
            }

            return data;
        }

        private static void WriteUInt32(byte[] data, int offset, uint value)
        {
            data[offset] = (byte)(value >> 24);
            data[offset + 1] = (byte)(value >> 16);
            data[offset + 2] = (byte)(value >> 8);
            data[offset + 3] = (byte)value;
        }

        private static void WriteUInt16(byte[] data, int offset, ushort value)
        {
            data[offset] = (byte)(value >> 8);
            data[offset + 1] = (byte)value;
        }

        private static void WriteInt16(byte[] data, int offset, short value)
        {
            data[offset] = (byte)(value >> 8);
            data[offset + 1] = (byte)value;
        }

        private static void WriteFixed(byte[] data, int offset, float value)
        {
            // Fixed 16.16
            int intPart = (int)value;
            int fracPart = (int)((value - intPart) * 65536f);
            if (value < 0 && fracPart > 0)
            {
                intPart--;
                fracPart = (int)((value - intPart) * 65536f);
            }
            WriteInt16(data, offset, (short)intPart);
            WriteUInt16(data, offset + 2, (ushort)fracPart);
        }
    }
}
