using System;
using System.IO;
using System.IO.Compression;

namespace Rend.Fonts.Internal
{
    /// <summary>
    /// Decompresses WOFF 1.0 font data into a standard sfnt (TrueType/OpenType) byte array.
    /// </summary>
    internal static class WoffDecompressor
    {
        // WOFF header is 44 bytes.
        private const int WoffHeaderSize = 44;

        // Each WOFF table directory entry is 20 bytes.
        private const int WoffTableEntrySize = 20;

        /// <summary>
        /// Decompresses WOFF 1.0 data into a valid TrueType/OpenType sfnt file.
        /// </summary>
        public static byte[] Decompress(byte[] woffData)
        {
            if (woffData == null) throw new ArgumentNullException(nameof(woffData));
            if (woffData.Length < WoffHeaderSize)
                throw new InvalidOperationException("Data is too small to be a valid WOFF file.");

            // Validate signature: "wOFF" = 0x774F4646.
            uint signature = ReadUInt32BE(woffData, 0);
            if (signature != 0x774F4646)
                throw new InvalidOperationException("Invalid WOFF signature.");

            // Parse WOFF header fields.
            uint sfntVersion = ReadUInt32BE(woffData, 4);
            // uint totalWoffSize = ReadUInt32BE(woffData, 8);  // Not needed for decompression.
            ushort numTables = ReadUInt16BE(woffData, 12);
            // Remaining header fields (reserved, totalSfntSize, etc.) not needed here.

            if (woffData.Length < WoffHeaderSize + numTables * WoffTableEntrySize)
                throw new InvalidOperationException("WOFF data truncated: not enough table directory entries.");

            // Parse table directory entries.
            var entries = new WoffTableEntry[numTables];
            int dirOffset = WoffHeaderSize;
            for (int i = 0; i < numTables; i++)
            {
                entries[i] = new WoffTableEntry
                {
                    Tag = ReadUInt32BE(woffData, dirOffset),
                    Offset = ReadUInt32BE(woffData, dirOffset + 4),
                    CompLength = ReadUInt32BE(woffData, dirOffset + 8),
                    OrigLength = ReadUInt32BE(woffData, dirOffset + 12),
                    OrigChecksum = ReadUInt32BE(woffData, dirOffset + 16)
                };
                dirOffset += WoffTableEntrySize;
            }

            // Compute the sfnt output size.
            // sfnt header: 12 bytes + 16 bytes per table record.
            int sfntHeaderSize = 12 + numTables * 16;
            long totalOrigLength = sfntHeaderSize;
            for (int i = 0; i < numTables; i++)
            {
                totalOrigLength += Pad4(entries[i].OrigLength);
            }

            byte[] output = new byte[totalOrigLength];

            // Write sfnt offset table.
            WriteUInt32BE(output, 0, sfntVersion);
            WriteUInt16BE(output, 4, numTables);

            // Compute searchRange, entrySelector, rangeShift.
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

            // Place table data after the sfnt header.
            int dataOffset = sfntHeaderSize;

            for (int i = 0; i < numTables; i++)
            {
                ref WoffTableEntry entry = ref entries[i];

                // Decompress or copy the table data.
                byte[] tableData;
                if (entry.CompLength < entry.OrigLength)
                {
                    // Compressed with zlib (raw DEFLATE inside WOFF, skip the 2-byte zlib header).
                    tableData = DeflateDecompress(woffData, (int)entry.Offset, (int)entry.CompLength, (int)entry.OrigLength);
                }
                else
                {
                    // Uncompressed: copy directly.
                    tableData = new byte[entry.OrigLength];
                    Array.Copy(woffData, (int)entry.Offset, tableData, 0, (int)entry.OrigLength);
                }

                // Write sfnt table record.
                int recordOffset = 12 + i * 16;
                WriteUInt32BE(output, recordOffset, entry.Tag);
                WriteUInt32BE(output, recordOffset + 4, entry.OrigChecksum);
                WriteUInt32BE(output, recordOffset + 8, (uint)dataOffset);
                WriteUInt32BE(output, recordOffset + 12, entry.OrigLength);

                // Copy table data into the output.
                Array.Copy(tableData, 0, output, dataOffset, tableData.Length);
                dataOffset += (int)Pad4(entry.OrigLength);
            }

            return output;
        }

        private static byte[] DeflateDecompress(byte[] data, int offset, int compressedLength, int originalLength)
        {
            byte[] result = new byte[originalLength];

            // WOFF uses zlib-compressed data. The first 2 bytes are the zlib header;
            // DeflateStream expects raw deflate, so skip them.
            int deflateOffset = offset + 2;
            int deflateLength = compressedLength - 2;

            using (var ms = new MemoryStream(data, deflateOffset, deflateLength))
            using (var deflate = new DeflateStream(ms, CompressionMode.Decompress))
            {
                int totalRead = 0;
                while (totalRead < originalLength)
                {
                    int bytesRead = deflate.Read(result, totalRead, originalLength - totalRead);
                    if (bytesRead == 0)
                        break;
                    totalRead += bytesRead;
                }
            }

            return result;
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

        private struct WoffTableEntry
        {
            public uint Tag;
            public uint Offset;
            public uint CompLength;
            public uint OrigLength;
            public uint OrigChecksum;
        }
    }
}
