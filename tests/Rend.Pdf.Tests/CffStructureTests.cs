using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Rend.Pdf.Parsing;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Pdf.Tests
{
    public class CffStructureTests
    {
        private readonly ITestOutputHelper _output;

        public CffStructureTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Convert_RealFont_CffHasValidHeader()
        {
            byte[]? otf = ConvertTestFont();
            if (otf == null) { return; }

            byte[] cff = ExtractCffTable(otf);
            Assert.True(cff.Length > 10, "CFF table too small");

            // CFF Header: major=1, minor=0, hdrSize=4, offSize>=1
            Assert.Equal(1, cff[0]); // major
            Assert.Equal(0, cff[1]); // minor
            Assert.Equal(4, cff[2]); // hdrSize
            Assert.True(cff[3] >= 1 && cff[3] <= 4, $"offSize={cff[3]} should be 1-4");
        }

        [Fact]
        public void Convert_RealFont_CffHasNameIndex()
        {
            byte[]? otf = ConvertTestFont();
            if (otf == null) { return; }

            byte[] cff = ExtractCffTable(otf);

            // After 4-byte header, Name INDEX starts
            int pos = 4;
            int nameCount = ReadIndex(cff, ref pos, out var nameEntries);
            Assert.Equal(1, nameCount);
            Assert.Single(nameEntries);

            string fontName = Encoding.ASCII.GetString(nameEntries[0]);
            _output.WriteLine($"Font name in CFF: {fontName}");
            Assert.Contains("Nimbus", fontName);
        }

        [Fact]
        public void Convert_RealFont_TopDictHasCharStringsAndPrivate()
        {
            byte[]? otf = ConvertTestFont();
            if (otf == null) { return; }

            byte[] cff = ExtractCffTable(otf);
            int pos = 4;

            // Skip Name INDEX
            ReadIndex(cff, ref pos, out _);

            // Read Top DICT INDEX
            int dictCount = ReadIndex(cff, ref pos, out var dictEntries);
            Assert.Equal(1, dictCount);

            byte[] topDict = dictEntries[0];
            var dictValues = ParseCffDict(topDict);

            _output.WriteLine($"Top DICT operators: {string.Join(", ", dictValues.Keys)}");

            // Must have charset (15), CharStrings (17), Private (18)
            Assert.True(dictValues.ContainsKey(15), "Missing charset operator (15)");
            Assert.True(dictValues.ContainsKey(17), "Missing CharStrings operator (17)");
            Assert.True(dictValues.ContainsKey(18), "Missing Private operator (18)");

            // Private has two operands: size and offset
            var privateOps = dictValues[18];
            Assert.True(privateOps.Count >= 2, $"Private needs 2 operands, got {privateOps.Count}");
            int privateSize = privateOps[0];
            int privateOffset = privateOps[1];
            _output.WriteLine($"Private: size={privateSize}, offset={privateOffset}");

            Assert.True(privateOffset > 0, "Private offset must be positive");
            Assert.True(privateOffset + privateSize <= cff.Length,
                $"Private extends beyond CFF: offset={privateOffset} + size={privateSize} > {cff.Length}");
        }

        [Fact]
        public void Convert_RealFont_CharStringsIndexHasGlyphs()
        {
            byte[]? otf = ConvertTestFont();
            if (otf == null) { return; }

            byte[] cff = ExtractCffTable(otf);
            int pos = 4;
            ReadIndex(cff, ref pos, out _); // Name
            ReadIndex(cff, ref pos, out var dictEntries); // Top DICT

            var dictValues = ParseCffDict(dictEntries[0]);
            int charStringsOffset = dictValues[17][0];

            // Read CharStrings INDEX at the specified offset
            int csPos = charStringsOffset;
            int csCount = ReadIndex(cff, ref csPos, out var csEntries);
            _output.WriteLine($"CharStrings: {csCount} glyphs");
            Assert.True(csCount > 5, $"Expected more than 5 glyphs, got {csCount}");

            // Each charstring should end with endchar (14)
            int endcharCount = 0;
            foreach (var cs in csEntries)
            {
                if (cs.Length > 0 && cs[cs.Length - 1] == 14)
                {
                    endcharCount++;
                }
            }
            _output.WriteLine($"Glyphs ending with endchar: {endcharCount}/{csCount}");
            Assert.True(endcharCount > csCount / 2,
                $"Most glyphs should end with endchar(14): only {endcharCount}/{csCount} do");
        }

        [Fact]
        public void Convert_RealFont_PrivateDictIsValid()
        {
            byte[]? otf = ConvertTestFont();
            if (otf == null) { return; }

            byte[] cff = ExtractCffTable(otf);
            int pos = 4;
            ReadIndex(cff, ref pos, out _); // Name
            ReadIndex(cff, ref pos, out var dictEntries); // Top DICT

            var dictValues = ParseCffDict(dictEntries[0]);
            int privateSize = dictValues[18][0];
            int privateOffset = dictValues[18][1];

            // Read Private DICT
            byte[] privateDict = new byte[privateSize];
            Array.Copy(cff, privateOffset, privateDict, 0, Math.Min(privateSize, cff.Length - privateOffset));

            var privateDictValues = ParseCffDict(privateDict);
            _output.WriteLine($"Private DICT operators: {string.Join(", ", privateDictValues.Keys)}");

            // Should have defaultWidthX (20) and nominalWidthX (21)
            Assert.True(privateDictValues.ContainsKey(20), "Missing defaultWidthX (20)");
            Assert.True(privateDictValues.ContainsKey(21), "Missing nominalWidthX (21)");

            // Check for Subrs (19) — should be present if font has Subrs
            if (privateDictValues.ContainsKey(19))
            {
                int subrsOffset = privateDictValues[19][0]; // relative to Private DICT start
                int absoluteSubrsOffset = privateOffset + subrsOffset;
                _output.WriteLine($"Local Subrs INDEX at absolute offset {absoluteSubrsOffset} (relative {subrsOffset})");

                Assert.True(absoluteSubrsOffset < cff.Length,
                    $"Subrs offset {absoluteSubrsOffset} exceeds CFF length {cff.Length}");

                // Try to read the Subrs INDEX
                int subrsPos = absoluteSubrsOffset;
                int subrsCount = ReadIndex(cff, ref subrsPos, out var subrsEntries);
                _output.WriteLine($"Local Subrs: {subrsCount} entries");
                Assert.True(subrsCount > 0, "Expected Subrs to have entries");
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────

        private byte[]? ConvertTestFont()
        {
            string[] paths = { "type1_F42.bin", "../../../type1_F42.bin", "../../../../type1_F42.bin",
                              "../../../../../type1_F42.bin", "../../../../../../type1_F42.bin" };
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    byte[] type1Data = File.ReadAllBytes(path);
                    byte[]? result = Type1ToCffConverter.Convert(type1Data);
                    _output.WriteLine($"Converted {type1Data.Length} -> {result?.Length ?? 0} bytes");
                    return result;
                }
            }
            _output.WriteLine("Test font not found, skipping");
            return null;
        }

        private byte[] ExtractCffTable(byte[] otf)
        {
            int numTables = (otf[4] << 8) | otf[5];
            for (int i = 0; i < numTables; i++)
            {
                int dirOffset = 12 + i * 16;
                string tag = Encoding.ASCII.GetString(otf, dirOffset, 4);
                if (tag == "CFF ")
                {
                    int tableOffset = (otf[dirOffset + 8] << 24) | (otf[dirOffset + 9] << 16) |
                                      (otf[dirOffset + 10] << 8) | otf[dirOffset + 11];
                    int tableLength = (otf[dirOffset + 12] << 24) | (otf[dirOffset + 13] << 16) |
                                      (otf[dirOffset + 14] << 8) | otf[dirOffset + 15];
                    byte[] cff = new byte[tableLength];
                    Array.Copy(otf, tableOffset, cff, 0, tableLength);
                    return cff;
                }
            }
            throw new Exception("CFF table not found");
        }

        private int ReadIndex(byte[] data, ref int pos, out List<byte[]> entries)
        {
            entries = new List<byte[]>();
            if (pos + 2 > data.Length) { return 0; }

            int count = (data[pos] << 8) | data[pos + 1];
            pos += 2;

            if (count == 0) { return 0; }

            int offSize = data[pos];
            pos++;

            int[] offsets = new int[count + 1];
            for (int i = 0; i <= count; i++)
            {
                int offset = 0;
                for (int j = 0; j < offSize; j++)
                {
                    offset = (offset << 8) | data[pos++];
                }
                offsets[i] = offset;
            }

            int dataStart = pos;
            for (int i = 0; i < count; i++)
            {
                int start = dataStart + offsets[i] - 1;
                int end = dataStart + offsets[i + 1] - 1;
                int length = end - start;
                byte[] entry = new byte[length];
                if (start + length <= data.Length)
                {
                    Array.Copy(data, start, entry, 0, length);
                }
                entries.Add(entry);
            }

            pos = dataStart + offsets[count] - 1;
            return count;
        }

        private Dictionary<int, List<int>> ParseCffDict(byte[] data)
        {
            var result = new Dictionary<int, List<int>>();
            var operands = new List<int>();
            int pos = 0;

            while (pos < data.Length)
            {
                byte b = data[pos];

                if (b >= 32 && b <= 246)
                {
                    operands.Add(b - 139);
                    pos++;
                }
                else if (b >= 247 && b <= 250 && pos + 1 < data.Length)
                {
                    operands.Add((b - 247) * 256 + data[pos + 1] + 108);
                    pos += 2;
                }
                else if (b >= 251 && b <= 254 && pos + 1 < data.Length)
                {
                    operands.Add(-(b - 251) * 256 - data[pos + 1] - 108);
                    pos += 2;
                }
                else if (b == 28 && pos + 2 < data.Length)
                {
                    operands.Add((short)((data[pos + 1] << 8) | data[pos + 2]));
                    pos += 3;
                }
                else if (b == 29 && pos + 4 < data.Length)
                {
                    operands.Add((data[pos + 1] << 24) | (data[pos + 2] << 16) |
                                 (data[pos + 3] << 8) | data[pos + 4]);
                    pos += 5;
                }
                else if (b == 12 && pos + 1 < data.Length)
                {
                    int op = 1200 + data[pos + 1];
                    result[op] = new List<int>(operands);
                    operands.Clear();
                    pos += 2;
                }
                else if (b <= 21)
                {
                    result[b] = new List<int>(operands);
                    operands.Clear();
                    pos++;
                }
                else
                {
                    pos++;
                }
            }

            return result;
        }
    }
}
