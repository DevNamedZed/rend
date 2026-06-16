using System;
using System.Collections.Generic;
using System.Text;
using Rend.Pdf.Parsing;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class OpenTypeFontBuilderTests
    {
        private static IReadOnlyList<int> Widths(int count)
        {
            var widths = new int[count];
            for (int i = 0; i < count; i++)
            {
                widths[i] = 500;
            }
            return widths;
        }

        private static readonly IReadOnlyDictionary<int, int> NoCmap = new Dictionary<int, int>();

        [Fact]
        public void Build_EmptyCff_ReturnsEmpty()
        {
            byte[] result = OpenTypeFontBuilder.Build(
                Array.Empty<byte>(), "Test", Widths(1), NoCmap, new float[] { 0, -200, 1000, 800 });
            Assert.Empty(result);
        }

        [Fact]
        public void Build_ProducesOttoHeader()
        {
            byte[] minimalCff = { 1, 0, 4, 1 }; // CFF header only
            byte[] result = OpenTypeFontBuilder.Build(
                minimalCff, "TestFont", Widths(1), NoCmap, new float[] { 0, -200, 1000, 800 });

            Assert.True(result.Length > 12);
            Assert.Equal((byte)'O', result[0]);
            Assert.Equal((byte)'T', result[1]);
            Assert.Equal((byte)'T', result[2]);
            Assert.Equal((byte)'O', result[3]);
        }

        [Fact]
        public void Build_ContainsNineRequiredTables()
        {
            byte[] minimalCff = { 1, 0, 4, 1 };
            byte[] result = OpenTypeFontBuilder.Build(
                minimalCff, "TestFont", Widths(1), NoCmap, new float[] { 0, -200, 1000, 800 });

            int numTables = (result[4] << 8) | result[5];
            Assert.Equal(9, numTables);
        }

        [Fact]
        public void Build_TablesAreSortedAlphabetically()
        {
            byte[] minimalCff = { 1, 0, 4, 1 };
            byte[] result = OpenTypeFontBuilder.Build(
                minimalCff, "TestFont", Widths(1), NoCmap, new float[] { 0, -200, 1000, 800 });

            int numTables = (result[4] << 8) | result[5];
            string previousTag = "";
            for (int i = 0; i < numTables; i++)
            {
                int offset = 12 + i * 16;
                string tag = Encoding.ASCII.GetString(result, offset, 4);
                Assert.True(string.Compare(tag, previousTag, StringComparison.Ordinal) >= 0,
                    $"Table '{tag}' should come after '{previousTag}'");
                previousTag = tag;
            }
        }

        [Fact]
        public void Build_HeadTableHasMagicNumber()
        {
            byte[] minimalCff = { 1, 0, 4, 1 };
            byte[] result = OpenTypeFontBuilder.Build(
                minimalCff, "TestFont", Widths(1), NoCmap, new float[] { 0, -200, 1000, 800 });

            // Find head table offset
            int numTables = (result[4] << 8) | result[5];
            for (int i = 0; i < numTables; i++)
            {
                int dirOffset = 12 + i * 16;
                string tag = Encoding.ASCII.GetString(result, dirOffset, 4);
                if (tag == "head")
                {
                    int tableOffset = (result[dirOffset + 8] << 24) | (result[dirOffset + 9] << 16) |
                                      (result[dirOffset + 10] << 8) | result[dirOffset + 11];
                    // Magic number at offset 12 in head table
                    uint magic = (uint)(result[tableOffset + 12] << 24) | (uint)(result[tableOffset + 13] << 16) |
                                 (uint)(result[tableOffset + 14] << 8) | result[tableOffset + 15];
                    Assert.Equal(0x5F0F3CF5u, magic);
                    return;
                }
            }
            Assert.Fail("head table not found");
        }

        [Fact]
        public void Build_MaxpTableHasCorrectGlyphCount()
        {
            byte[] minimalCff = { 1, 0, 4, 1 };
            int expectedGlyphs = 42;
            byte[] result = OpenTypeFontBuilder.Build(
                minimalCff, "TestFont", Widths(expectedGlyphs), NoCmap, new float[] { 0, -200, 1000, 800 });

            int numTables = (result[4] << 8) | result[5];
            for (int i = 0; i < numTables; i++)
            {
                int dirOffset = 12 + i * 16;
                string tag = Encoding.ASCII.GetString(result, dirOffset, 4);
                if (tag == "maxp")
                {
                    int tableOffset = (result[dirOffset + 8] << 24) | (result[dirOffset + 9] << 16) |
                                      (result[dirOffset + 10] << 8) | result[dirOffset + 11];
                    int glyphCount = (result[tableOffset + 4] << 8) | result[tableOffset + 5];
                    Assert.Equal(expectedGlyphs, glyphCount);
                    return;
                }
            }
            Assert.Fail("maxp table not found");
        }

        [Fact]
        public void Build_TableDataIs4ByteAligned()
        {
            byte[] cff = new byte[13]; // Odd size to test padding
            cff[0] = 1; cff[1] = 0; cff[2] = 4; cff[3] = 1;
            byte[] result = OpenTypeFontBuilder.Build(
                cff, "TestFont", Widths(1), NoCmap, new float[] { 0, -200, 1000, 800 });

            int numTables = (result[4] << 8) | result[5];
            for (int i = 0; i < numTables; i++)
            {
                int dirOffset = 12 + i * 16;
                int tableOffset = (result[dirOffset + 8] << 24) | (result[dirOffset + 9] << 16) |
                                  (result[dirOffset + 10] << 8) | result[dirOffset + 11];
                Assert.True(tableOffset % 4 == 0, $"Table at offset {tableOffset} is not 4-byte aligned");
            }
        }
    }
}
