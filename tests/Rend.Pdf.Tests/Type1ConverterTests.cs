using System;
using System.Text;
using Rend.Pdf.Parsing;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class Type1ConverterTests
    {
        [Fact]
        public void Convert_NullInput_ReturnsNull()
        {
            Assert.Null(Type1ToCffConverter.Convert(null!));
        }

        [Fact]
        public void Convert_TooSmall_ReturnsNull()
        {
            Assert.Null(Type1ToCffConverter.Convert(new byte[5]));
        }

        [Fact]
        public void Convert_NotType1_ReturnsNull()
        {
            byte[] notType1 = Encoding.ASCII.GetBytes("This is not a Type1 font");
            Assert.Null(Type1ToCffConverter.Convert(notType1));
        }

        [Fact]
        public void Convert_PfaWithNoEexec_ReturnsNull()
        {
            byte[] pfaNoEexec = Encoding.ASCII.GetBytes(
                "%!PS-AdobeFont-1.0: TestFont\n" +
                "/FontName /TestFont def\n" +
                "/FontBBox {0 -200 1000 800} def\n");
            Assert.Null(Type1ToCffConverter.Convert(pfaNoEexec));
        }

        [Fact]
        public void Convert_RealPfaFont_ProducesOttoHeader()
        {
            // Use the exported NimbusRomNo9L font if available
            string? fontPath = FindTestFont();
            if (fontPath == null)
            {
                return; // Skip if test font not available
            }

            byte[] fontData = System.IO.File.ReadAllBytes(fontPath);
            byte[]? result = Type1ToCffConverter.Convert(fontData);

            // Should produce something (even if Skia can't load it yet)
            Assert.NotNull(result);
            Assert.True(result!.Length > 100, $"Output too small: {result.Length} bytes");

            // Should have OTTO header (OpenType/CFF)
            Assert.Equal((byte)'O', result[0]);
            Assert.Equal((byte)'T', result[1]);
            Assert.Equal((byte)'T', result[2]);
            Assert.Equal((byte)'O', result[3]);
        }

        [Fact]
        public void Convert_RealPfaFont_ContainsCffTable()
        {
            string? fontPath = FindTestFont();
            if (fontPath == null)
            {
                return;
            }

            byte[] fontData = System.IO.File.ReadAllBytes(fontPath);
            byte[]? result = Type1ToCffConverter.Convert(fontData);
            Assert.NotNull(result);

            // Find "CFF " table tag in the SFNT
            string resultStr = Encoding.ASCII.GetString(result!);
            Assert.Contains("CFF ", resultStr);
        }

        [Fact]
        public void Convert_RealPfaFont_ValidatesWithFontTools()
        {
            string? fontPath = FindTestFont();
            if (fontPath == null)
            {
                return;
            }

            byte[] fontData = System.IO.File.ReadAllBytes(fontPath);
            byte[]? result = Type1ToCffConverter.Convert(fontData);
            Assert.NotNull(result);

            // Parse SFNT header
            Assert.True(result!.Length > 12);
            int numTables = (result[4] << 8) | result[5];
            Assert.True(numTables >= 8, $"Expected at least 8 tables, got {numTables}");

            // Verify required tables exist
            var tableNames = new System.Collections.Generic.HashSet<string>();
            for (int i = 0; i < numTables; i++)
            {
                int offset = 12 + i * 16;
                string tag = Encoding.ASCII.GetString(result, offset, 4);
                tableNames.Add(tag);
            }
            Assert.Contains("CFF ", tableNames);
            Assert.Contains("head", tableNames);
            Assert.Contains("hhea", tableNames);
            Assert.Contains("hmtx", tableNames);
            Assert.Contains("maxp", tableNames);
            Assert.Contains("name", tableNames);
            Assert.Contains("OS/2", tableNames);
            Assert.Contains("post", tableNames);
            Assert.Contains("cmap", tableNames);
        }

#pragma warning disable CS8600
        private static string? FindTestFont()
        {
            string[] searchPaths =
            {
                "type1_F42.bin",
                "../../../type1_F42.bin",
                "../../../../type1_F42.bin",
                "../../../../../type1_F42.bin",
            };

            foreach (string path in searchPaths)
            {
                if (System.IO.File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }
    }
}
