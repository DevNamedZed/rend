#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Rend.Pdf.Parsing
{
    /// <summary>
    /// Decodes PDF stream filters: FlateDecode, LZWDecode, ASCIIHexDecode, ASCII85Decode.
    /// Each method is stateless and independently testable.
    /// </summary>
    public static class StreamDecoder
    {
        public static byte[] ApplyFilter(string filterName, byte[] data, PdfObj resolvedParams)
        {
            switch (filterName)
            {
                case "FlateDecode":
                    return FlateDecode(data, resolvedParams);
                case "LZWDecode":
                    return LzwDecode(data, resolvedParams);
                case "ASCIIHexDecode":
                    return AsciiHexDecode(data);
                case "ASCII85Decode":
                    return Ascii85Decode(data);
                default:
                    return data;
            }
        }

        public static byte[] FlateDecode(byte[] data, PdfObj resolvedParams)
        {
            byte[] decompressed = DeflateData(data);
            return ApplyPredictor(decompressed, resolvedParams);
        }

        private static byte[] DeflateData(byte[] data)
        {
            int offset = 0;
            if (data.Length >= 2 && (data[0] & 0x0F) == 8)
            {
                offset = 2;
            }

            try
            {
                return DeflateFromOffset(data, offset);
            }
            catch when (offset > 0)
            {
                try
                {
                    return DeflateFromOffset(data, 0);
                }
                catch
                {
                    return data;
                }
            }
            catch
            {
                return data;
            }
        }

        private static byte[] DeflateFromOffset(byte[] data, int offset)
        {
            using var input = new MemoryStream(data, offset, data.Length - offset);
            using var deflate = new DeflateStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            deflate.CopyTo(output);
            return output.ToArray();
        }

        public static byte[] LzwDecode(byte[] data, PdfObj resolvedParams)
        {
            var output = new List<byte>();
            var table = new List<byte[]>();

            for (int i = 0; i < 256; i++)
            {
                table.Add(new byte[] { (byte)i });
            }
            table.Add(Array.Empty<byte>());
            table.Add(Array.Empty<byte>());

            int earlyChange = 1;
            if (!resolvedParams.IsNull)
            {
                var earlyChangeObj = resolvedParams["EarlyChange"];
                if (!earlyChangeObj.IsNull)
                {
                    earlyChange = (int)earlyChangeObj.AsInt();
                }
            }

            int bitPos = 0;
            int codeSize = 9;
            int nextCode = 258;
            int prevCode = -1;

            while (true)
            {
                int code = ReadBits(data, ref bitPos, codeSize);
                if (code < 0)
                {
                    break;
                }

                if (code == 256)
                {
                    table.RemoveRange(258, table.Count - 258);
                    codeSize = 9;
                    nextCode = 258;
                    prevCode = -1;
                    continue;
                }

                if (code == 257)
                {
                    break;
                }

                byte[] entry;
                if (code < table.Count)
                {
                    entry = table[code];
                }
                else if (code == nextCode && prevCode >= 0)
                {
                    var prev = table[prevCode];
                    entry = new byte[prev.Length + 1];
                    Array.Copy(prev, entry, prev.Length);
                    entry[prev.Length] = prev[0];
                }
                else
                {
                    break;
                }

                for (int i = 0; i < entry.Length; i++)
                {
                    output.Add(entry[i]);
                }

                if (prevCode >= 0 && nextCode < 4096)
                {
                    var prev = table[prevCode];
                    var newEntry = new byte[prev.Length + 1];
                    Array.Copy(prev, newEntry, prev.Length);
                    newEntry[prev.Length] = entry[0];
                    table.Add(newEntry);
                    nextCode++;
                }

                prevCode = code;

                int threshold = (1 << codeSize) - earlyChange;
                if (nextCode > threshold && codeSize < 12)
                {
                    codeSize++;
                }
            }

            byte[] decompressed = output.ToArray();
            return ApplyPredictor(decompressed, resolvedParams);
        }

        public static byte[] AsciiHexDecode(byte[] data)
        {
            var result = new List<byte>();
            int nibble = -1;
            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                if (b == '>') { break; }
                if (IsWhitespace(b)) { continue; }

                int val = HexVal(b);
                if (val < 0) { continue; }

                if (nibble < 0)
                {
                    nibble = val;
                }
                else
                {
                    result.Add((byte)((nibble << 4) | val));
                    nibble = -1;
                }
            }
            if (nibble >= 0)
            {
                result.Add((byte)(nibble << 4));
            }

            return result.ToArray();
        }

        public static byte[] Ascii85Decode(byte[] data)
        {
            var result = new List<byte>();
            int count = 0;
            long tuple = 0;

            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                if (b == '~') { break; }
                if (IsWhitespace(b)) { continue; }

                if (b == 'z')
                {
                    result.Add(0);
                    result.Add(0);
                    result.Add(0);
                    result.Add(0);
                    continue;
                }

                if (b < '!' || b > 'u') { continue; }

                tuple = tuple * 85 + (b - '!');
                count++;

                if (count == 5)
                {
                    result.Add((byte)(tuple >> 24));
                    result.Add((byte)(tuple >> 16));
                    result.Add((byte)(tuple >> 8));
                    result.Add((byte)tuple);
                    tuple = 0;
                    count = 0;
                }
            }

            if (count > 0)
            {
                for (int i = count; i < 5; i++)
                {
                    tuple = tuple * 85 + 84;
                }
                for (int i = 0; i < count - 1; i++)
                {
                    result.Add((byte)(tuple >> (24 - i * 8)));
                }
            }

            return result.ToArray();
        }

        public static byte[] ApplyPngPredictor(byte[] data, int columns, int colors, int bitsPerComponent)
        {
            int bytesPerPixel = (colors * bitsPerComponent + 7) / 8;
            int rowBytes = (columns * colors * bitsPerComponent + 7) / 8;
            int srcRowSize = 1 + rowBytes;

            if (data.Length < srcRowSize)
            {
                return data;
            }

            int numRows = data.Length / srcRowSize;
            byte[] output = new byte[numRows * rowBytes];
            byte[] prevRow = new byte[rowBytes];

            for (int row = 0; row < numRows; row++)
            {
                int srcOffset = row * srcRowSize;
                if (srcOffset >= data.Length)
                {
                    break;
                }

                byte filterType = data[srcOffset];
                int dstOffset = row * rowBytes;

                for (int i = 0; i < rowBytes; i++)
                {
                    int srcIdx = srcOffset + 1 + i;
                    if (srcIdx >= data.Length)
                    {
                        break;
                    }

                    byte raw = data[srcIdx];
                    byte a = i >= bytesPerPixel ? output[dstOffset + i - bytesPerPixel] : (byte)0;
                    byte b = prevRow[i];
                    byte c = i >= bytesPerPixel ? prevRow[i - bytesPerPixel] : (byte)0;

                    byte val;
                    switch (filterType)
                    {
                        case 0: val = raw; break;
                        case 1: val = (byte)(raw + a); break;
                        case 2: val = (byte)(raw + b); break;
                        case 3: val = (byte)(raw + ((a + b) >> 1)); break;
                        case 4: val = (byte)(raw + PaethPredictor(a, b, c)); break;
                        default: val = raw; break;
                    }

                    output[dstOffset + i] = val;
                }

                Array.Copy(output, dstOffset, prevRow, 0, rowBytes);
            }

            return output;
        }

        private static byte[] ApplyPredictor(byte[] decompressed, PdfObj resolvedParams)
        {
            if (!resolvedParams.IsNull)
            {
                int predictor = (int)resolvedParams["Predictor"].AsInt();
                if (predictor >= 10 && predictor <= 15)
                {
                    int columns = (int)resolvedParams["Columns"].AsInt();
                    if (columns <= 0) { columns = 1; }
                    int colors = (int)resolvedParams["Colors"].AsInt();
                    if (colors <= 0) { colors = 1; }
                    int bitsPerComponent = (int)resolvedParams["BitsPerComponent"].AsInt();
                    if (bitsPerComponent <= 0) { bitsPerComponent = 8; }

                    return ApplyPngPredictor(decompressed, columns, colors, bitsPerComponent);
                }
            }
            return decompressed;
        }

        private static byte PaethPredictor(byte a, byte b, byte c)
        {
            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);
            if (pa <= pb && pa <= pc) { return a; }
            if (pb <= pc) { return b; }
            return c;
        }

        private static int ReadBits(byte[] data, ref int bitPos, int count)
        {
            int result = 0;
            for (int i = 0; i < count; i++)
            {
                int byteIndex = bitPos / 8;
                int bitIndex = 7 - (bitPos % 8);
                if (byteIndex >= data.Length)
                {
                    return -1;
                }
                int bit = (data[byteIndex] >> bitIndex) & 1;
                result = (result << 1) | bit;
                bitPos++;
            }
            return result;
        }

        private static bool IsWhitespace(byte b)
        {
            return b == 0 || b == 9 || b == 10 || b == 12 || b == 13 || b == 32;
        }

        private static int HexVal(byte b)
        {
            if (b >= '0' && b <= '9') { return b - '0'; }
            if (b >= 'a' && b <= 'f') { return b - 'a' + 10; }
            if (b >= 'A' && b <= 'F') { return b - 'A' + 10; }
            return -1;
        }
    }
}
