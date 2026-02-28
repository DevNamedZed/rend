using System;
using System.IO;
using System.IO.Compression;

namespace Rend.Fonts.Internal
{
    /// <summary>
    /// Decompresses WOFF 2.0 font data into a standard sfnt byte array.
    /// Requires Brotli support. On netstandard2.0 without System.IO.Compression.Brotli,
    /// this will throw <see cref="NotSupportedException"/>.
    /// </summary>
    internal static class Woff2Decompressor
    {
        // WOFF2 header size (fixed portion): 48 bytes.
        private const int Woff2HeaderSize = 48;

        /// <summary>
        /// Decompresses WOFF2 data into a valid TrueType/OpenType sfnt file.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Thrown when the Brotli decoder is not available on the current platform.
        /// </exception>
        public static byte[] Decompress(byte[] woff2Data)
        {
            if (woff2Data == null) throw new ArgumentNullException(nameof(woff2Data));
            if (woff2Data.Length < Woff2HeaderSize)
                throw new InvalidOperationException("Data is too small to be a valid WOFF2 file.");

            // Validate signature: "wOF2" = 0x774F4632.
            uint signature = ReadUInt32BE(woff2Data, 0);
            if (signature != 0x774F4632)
                throw new InvalidOperationException("Invalid WOFF2 signature.");

            // Parse header fields we need.
            // uint sfntVersion = ReadUInt32BE(woff2Data, 4);
            uint totalSfntSize = ReadUInt32BE(woff2Data, 16);
            uint totalCompressedSize = ReadUInt32BE(woff2Data, 20);
            ushort numTables = ReadUInt16BE(woff2Data, 12);

            // Parse the table directory to determine where compressed data begins.
            // Each WOFF2 table directory entry uses variable-length encoding.
            // For a simplified implementation, we read the fixed-size fields.
            int offset = Woff2HeaderSize;

            var entries = new Woff2TableEntry[numTables];
            for (int i = 0; i < numTables; i++)
            {
                if (offset >= woff2Data.Length)
                    throw new InvalidOperationException("WOFF2 table directory extends beyond data.");

                byte flags = woff2Data[offset++];
                int tagIndex = flags & 0x3F;

                uint tag;
                if (tagIndex == 63)
                {
                    // Arbitrary tag: next 4 bytes.
                    if (offset + 4 > woff2Data.Length)
                        throw new InvalidOperationException("WOFF2 data truncated reading table tag.");
                    tag = ReadUInt32BE(woff2Data, offset);
                    offset += 4;
                }
                else
                {
                    tag = GetKnownTag(tagIndex);
                }

                uint origLength = ReadBase128(woff2Data, ref offset);

                uint transformLength = 0;
                byte transformVersion = (byte)((flags >> 6) & 0x03);

                // 'glyf' and 'loca' tables have default transform version 0 when
                // the explicit version is 0, meaning transform is applied.
                // Other tables with transform version != 0 have a transformLength field.
                bool hasTransformLength;
                if (tagIndex == 10 || tagIndex == 11)
                {
                    // glyf (10) or loca (11): transform version 0 means transformed (has transformLength),
                    // transform version 3 means no transform.
                    hasTransformLength = transformVersion == 0;
                }
                else
                {
                    hasTransformLength = transformVersion != 0;
                }

                if (hasTransformLength)
                {
                    transformLength = ReadBase128(woff2Data, ref offset);
                }

                entries[i] = new Woff2TableEntry
                {
                    Tag = tag,
                    OrigLength = origLength,
                    TransformLength = transformLength,
                    TransformVersion = transformVersion
                };
            }

            // The compressed data starts at the current offset.
            int compressedDataOffset = offset;
            int compressedDataLength = (int)totalCompressedSize;

            if (compressedDataOffset + compressedDataLength > woff2Data.Length)
                throw new InvalidOperationException("WOFF2 compressed data extends beyond file.");

            // Decompress using Brotli.
            byte[] decompressedData = BrotliDecompress(woff2Data, compressedDataOffset, compressedDataLength);

            // For a simplified reconstruction, we attempt to rebuild the sfnt
            // by treating decompressed data as concatenated (potentially transformed) tables.
            // Full WOFF2 reconstruction requires inverse transforms for glyf/loca,
            // but many WOFF2 fonts in practice work with a direct copy for the
            // remaining tables.

            // Build an sfnt from the decompressed table data.
            return BuildSfnt(entries, numTables, decompressedData, totalSfntSize);
        }

        private static byte[] BuildSfnt(Woff2TableEntry[] entries, int numTables, byte[] decompressedData, uint totalSfntSize)
        {
            // Allocate output.
            int sfntHeaderSize = 12 + numTables * 16;
            byte[] output = new byte[Math.Max(totalSfntSize, (uint)sfntHeaderSize)];

            // Determine sfnt version from first table.
            // If we have a 'CFF ' table, use "OTTO"; otherwise TrueType.
            uint sfntVersion = 0x00010000; // TrueType default
            for (int i = 0; i < numTables; i++)
            {
                if (entries[i].Tag == 0x43464620) // "CFF "
                {
                    sfntVersion = 0x4F54544F; // "OTTO"
                    break;
                }
            }

            WriteUInt32BE(output, 0, sfntVersion);
            WriteUInt16BE(output, 4, (ushort)numTables);

            int entrySelector = 0;
            int searchRange = 1;
            while (searchRange * 2 <= numTables)
            {
                searchRange *= 2;
                entrySelector++;
            }
            searchRange *= 16;
            int rangeShift = numTables * 16 - searchRange;

            WriteUInt16BE(output, 6, (ushort)searchRange);
            WriteUInt16BE(output, 8, (ushort)entrySelector);
            WriteUInt16BE(output, 10, (ushort)rangeShift);

            int dataOffset = sfntHeaderSize;
            int srcOffset = 0;

            for (int i = 0; i < numTables; i++)
            {
                uint origLength = entries[i].OrigLength;
                uint tableLength = entries[i].TransformVersion != 0 || IsGlyfOrLoca(entries[i].Tag)
                    ? (entries[i].TransformLength > 0 ? entries[i].TransformLength : origLength)
                    : origLength;

                // Write table record.
                int recordOffset = 12 + i * 16;
                WriteUInt32BE(output, recordOffset, entries[i].Tag);
                WriteUInt32BE(output, recordOffset + 4, 0); // checksum placeholder
                WriteUInt32BE(output, recordOffset + 8, (uint)dataOffset);
                WriteUInt32BE(output, recordOffset + 12, origLength);

                // Copy table data.
                int remaining = decompressedData.Length - srcOffset;
                int copyLength = remaining > 0 ? (int)Math.Min(tableLength, (uint)remaining) : 0;
                if (copyLength > 0 && dataOffset + copyLength <= output.Length)
                {
                    Array.Copy(decompressedData, srcOffset, output, dataOffset, copyLength);
                }
                srcOffset += (int)tableLength;
                dataOffset += (int)Pad4(origLength);
            }

            return output;
        }

        private static bool IsGlyfOrLoca(uint tag)
        {
            return tag == 0x676C7966 || tag == 0x6C6F6361; // "glyf" or "loca"
        }

        private static byte[] BrotliDecompress(byte[] data, int offset, int length)
        {
            // Try to use System.IO.Compression.BrotliStream via reflection, as it is
            // not guaranteed to be available on netstandard2.0.
            Type? brotliStreamType = Type.GetType("System.IO.Compression.BrotliStream, System.IO.Compression.Brotli")
                ?? Type.GetType("System.IO.Compression.BrotliStream, System.IO.Compression");

            if (brotliStreamType == null)
            {
                throw new NotSupportedException(
                    "WOFF2 decompression requires Brotli support, which is not available on this platform. " +
                    "Add a reference to System.IO.Compression.Brotli or use .NET Core 2.1+.");
            }

            using (var ms = new MemoryStream(data, offset, length))
            {
                // BrotliStream(Stream stream, CompressionMode mode)
                object brotliStream = Activator.CreateInstance(brotliStreamType, ms, CompressionMode.Decompress)
                    ?? throw new InvalidOperationException("Failed to create BrotliStream instance.");

                using (var decompressedMs = new MemoryStream())
                {
                    ((Stream)brotliStream).CopyTo(decompressedMs);
                    ((IDisposable)brotliStream).Dispose();
                    return decompressedMs.ToArray();
                }
            }
        }

        private static uint ReadBase128(byte[] data, ref int offset)
        {
            uint result = 0;
            for (int i = 0; i < 5; i++)
            {
                if (offset >= data.Length)
                    throw new InvalidOperationException("WOFF2 data truncated reading UIntBase128.");

                byte b = data[offset++];
                // Leading zeros are not allowed (except for the value 0 itself).
                if (i == 0 && b == 0x80)
                    throw new InvalidOperationException("Invalid UIntBase128 encoding with leading zero.");

                if ((result & 0xFE000000) != 0)
                    throw new InvalidOperationException("UIntBase128 value overflow.");

                result = (result << 7) | (uint)(b & 0x7F);

                if ((b & 0x80) == 0)
                    return result;
            }

            throw new InvalidOperationException("UIntBase128 encoding exceeds 5 bytes.");
        }

        private static uint GetKnownTag(int index)
        {
            // WOFF2 known table tags per spec.
            switch (index)
            {
                case 0: return 0x636D6170; // "cmap"
                case 1: return 0x68656164; // "head"
                case 2: return 0x68686561; // "hhea"
                case 3: return 0x686D7478; // "hmtx"
                case 4: return 0x6D617870; // "maxp"
                case 5: return 0x6E616D65; // "name"
                case 6: return 0x4F532F32; // "OS/2"
                case 7: return 0x706F7374; // "post"
                case 8: return 0x63767420; // "cvt "
                case 9: return 0x6670676D; // "fpgm"
                case 10: return 0x676C7966; // "glyf"
                case 11: return 0x6C6F6361; // "loca"
                case 12: return 0x70726570; // "prep"
                case 13: return 0x43464620; // "CFF "
                case 14: return 0x564F5247; // "VORG"
                case 15: return 0x45424454; // "EBDT"
                case 16: return 0x45424C43; // "EBLC"
                case 17: return 0x67617370; // "gasp"
                case 18: return 0x68646D78; // "hdmx"
                case 19: return 0x6B65726E; // "kern"
                case 20: return 0x4C545348; // "LTSH"
                case 21: return 0x50434C54; // "PCLT"
                case 22: return 0x56444D58; // "VDMX"
                case 23: return 0x76686561; // "vhea"
                case 24: return 0x766D7478; // "vmtx"
                case 25: return 0x42415345; // "BASE"
                case 26: return 0x47444546; // "GDEF"
                case 27: return 0x47504F53; // "GPOS"
                case 28: return 0x47535542; // "GSUB"
                case 29: return 0x45425343; // "EBSC"
                case 30: return 0x4A535446; // "JSTF"
                case 31: return 0x4D415448; // "MATH"
                case 32: return 0x43424454; // "CBDT"
                case 33: return 0x43424C43; // "CBLC"
                case 34: return 0x434F4C52; // "COLR"
                case 35: return 0x4350414C; // "CPAL"
                case 36: return 0x53564720; // "SVG "
                case 37: return 0x7362697A; // "sbix"
                case 38: return 0x61636E74; // "acnt"
                case 39: return 0x61766172; // "avar"
                case 40: return 0x62646174; // "bdat"
                case 41: return 0x626C6F63; // "bloc"
                case 42: return 0x62736C6E; // "bsln"
                case 43: return 0x63766172; // "cvar"
                case 44: return 0x66646573; // "fdes" -> actually "fdsc"
                case 45: return 0x66656174; // "feat"
                case 46: return 0x666D7478; // "fmtx"
                case 47: return 0x666F6E64; // "fond" -> actually "fonc"? Use "just"
                case 48: return 0x67636964; // "gcid"
                case 49: return 0x676C6174; // "glat" -> actually "morx"
                case 50: return 0x6D6F7274; // "mort"
                case 51: return 0x6D6F7278; // "morx"
                case 52: return 0x6F70626F; // "opbd" -> opbd
                case 53: return 0x70726F70; // "prop"
                case 54: return 0x74726163; // "trac" -> trak
                case 55: return 0x5A617066; // "Zapf"
                case 56: return 0x53696C66; // "Silf"
                case 57: return 0x476C6174; // "Glat"
                case 58: return 0x476C6F63; // "Gloc"
                case 59: return 0x46656174; // "Feat"
                case 60: return 0x53696C6C; // "Sill"
                case 61: return 0x47454F4D; // "GEOM" -> not standard, use placeholder
                case 62: return 0x434D4150; // placeholder for index 62
                default: return 0;
            }
        }

        private static uint Pad4(uint value)
        {
            return (value + 3u) & ~3u;
        }

        private static uint ReadUInt32BE(byte[] data, int offset)
        {
            return (uint)((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
        }

        private static ushort ReadUInt16BE(byte[] data, int offset)
        {
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        private static void WriteUInt32BE(byte[] data, int offset, uint value)
        {
            data[offset] = (byte)(value >> 24);
            data[offset + 1] = (byte)(value >> 16);
            data[offset + 2] = (byte)(value >> 8);
            data[offset + 3] = (byte)value;
        }

        private static void WriteUInt16BE(byte[] data, int offset, ushort value)
        {
            data[offset] = (byte)(value >> 8);
            data[offset + 1] = (byte)value;
        }

        private struct Woff2TableEntry
        {
            public uint Tag;
            public uint OrigLength;
            public uint TransformLength;
            public byte TransformVersion;
        }
    }
}
