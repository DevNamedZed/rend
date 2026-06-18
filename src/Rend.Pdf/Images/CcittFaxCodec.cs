#nullable enable
using System;
using System.Collections.Generic;

namespace Rend.Pdf.Images
{
    /// <summary>
    /// CCITT Group 4 (ITU-T T.6, 2D) fax codec for bilevel (1bpp) images, used both to embed
    /// CCITT-compressed images in written PDFs and to decode <c>CCITTFaxDecode</c> streams when
    /// reading. Encode and decode share the T.4 run-length and 2D-mode code tables so the two
    /// stay consistent; the encoder's spec-compliance is cross-checked against MuPDF.
    /// [SPEC] ITU-T T.4 (run-length codes), T.6 (2D coding); ISO 32000-1 §7.4.6.
    /// Pixels are exchanged as packed 1bpp rows, MSB-first. With <c>blackIs1=false</c> (PDF
    /// default) a black pixel is sample bit 0 and white is 1; <c>blackIs1=true</c> reverses it.
    /// </summary>
    internal static class CcittFaxCodec
    {
        // Mode sentinels for ReadMode; deliberately outside the vertical-delta range [-3,3]
        // so they never collide with a VL1/VL2/VL3 (-1/-2/-3) result.
        private const int Pass = 100;
        private const int Horizontal = 101;

        // ── T.4 terminating + make-up run-length codes. Each entry: (bitLength, bits). ──
        private static readonly (int Len, int Bits)[] WhiteTerminating =
        {
            (8,0x35),(6,0x07),(4,0x07),(4,0x08),(4,0x0B),(4,0x0C),(4,0x0E),(4,0x0F),
            (5,0x13),(5,0x14),(5,0x07),(5,0x08),(6,0x08),(6,0x03),(6,0x34),(6,0x35),
            (6,0x2A),(6,0x2B),(7,0x27),(7,0x0C),(7,0x08),(7,0x17),(7,0x03),(7,0x04),
            (7,0x28),(7,0x2B),(7,0x13),(7,0x24),(7,0x18),(8,0x02),(8,0x03),(8,0x1A),
            (8,0x1B),(8,0x12),(8,0x13),(8,0x14),(8,0x15),(8,0x16),(8,0x17),(8,0x28),
            (8,0x29),(8,0x2A),(8,0x2B),(8,0x2C),(8,0x2D),(8,0x04),(8,0x05),(8,0x0A),
            (8,0x0B),(8,0x52),(8,0x53),(8,0x54),(8,0x55),(8,0x24),(8,0x25),(8,0x58),
            (8,0x59),(8,0x5A),(8,0x5B),(8,0x4A),(8,0x4B),(8,0x32),(8,0x33),(8,0x34),
        };

        private static readonly (int Len, int Bits)[] WhiteMakeup =
        {
            (5,0x1B),(5,0x12),(6,0x17),(7,0x37),(8,0x36),(8,0x37),(8,0x64),(8,0x65),
            (8,0x68),(8,0x67),(9,0xCC),(9,0xCD),(9,0xD2),(9,0xD3),(9,0xD4),(9,0xD5),
            (9,0xD6),(9,0xD7),(9,0xD8),(9,0xD9),(9,0xDA),(9,0xDB),(9,0x98),(9,0x99),
            (9,0x9A),(6,0x18),(9,0x9B),
        };

        private static readonly (int Len, int Bits)[] BlackTerminating =
        {
            (10,0x37),(3,0x02),(2,0x03),(2,0x02),(3,0x03),(4,0x03),(4,0x02),(5,0x03),
            (6,0x05),(6,0x04),(7,0x04),(7,0x05),(7,0x07),(8,0x04),(8,0x07),(9,0x18),
            (10,0x17),(10,0x18),(10,0x08),(11,0x67),(11,0x68),(11,0x6C),(11,0x37),(11,0x28),
            (11,0x17),(11,0x18),(12,0xCA),(12,0xCB),(12,0xCC),(12,0xCD),(12,0x68),(12,0x69),
            (12,0x6A),(12,0x6B),(12,0xD2),(12,0xD3),(12,0xD4),(12,0xD5),(12,0xD6),(12,0xD7),
            (12,0x6C),(12,0x6D),(12,0xDA),(12,0xDB),(12,0x54),(12,0x55),(12,0x56),(12,0x57),
            (12,0x64),(12,0x65),(12,0x52),(12,0x53),(12,0x24),(12,0x37),(12,0x38),(12,0x27),
            (12,0x28),(12,0x58),(12,0x59),(12,0x2B),(12,0x2C),(12,0x5A),(12,0x66),(12,0x67),
        };

        private static readonly (int Len, int Bits)[] BlackMakeup =
        {
            (10,0x0F),(12,0xC8),(12,0xC9),(12,0x5B),(12,0x33),(12,0x34),(12,0x35),(13,0x6C),
            (13,0x6D),(13,0x4A),(13,0x4B),(13,0x4C),(13,0x4D),(13,0x72),(13,0x73),(13,0x74),
            (13,0x75),(13,0x76),(13,0x77),(13,0x52),(13,0x53),(13,0x54),(13,0x55),(13,0x5A),
            (13,0x5B),(13,0x64),(13,0x65),
        };

        // Shared make-up codes 1792..2560 (both colours).
        private static readonly (int Len, int Bits)[] SharedMakeup =
        {
            (11,0x08),(11,0x0C),(11,0x0D),(12,0x12),(12,0x13),(12,0x14),(12,0x15),(12,0x16),
            (12,0x17),(12,0x1C),(12,0x1D),(12,0x1E),(12,0x1F),
        };

        private const int EolLen = 12;
        private const int EolBits = 0x001;

        public static byte[] EncodeG4(byte[] packedRows, int columns, int rows, bool blackIs1)
        {
            var writer = new BitWriter();
            int[] reference = { columns, columns };

            for (int row = 0; row < rows; row++)
            {
                bool[] line = UnpackRow(packedRows, row, columns, blackIs1);
                int[] current = ChangingElements(line, columns);
                EncodeRow(writer, current, reference, columns);
                reference = current;
            }

            // EOFB = two EOL codes.
            writer.WriteBits(EolBits, EolLen);
            writer.WriteBits(EolBits, EolLen);
            return writer.ToArray();
        }

        // Guards against attacker-controlled /Columns and /Rows from untrusted PDFs: bounds the
        // per-row work and the output allocation so the int arithmetic below cannot overflow and a
        // hostile image cannot exhaust memory. Far larger than any real fax/scan.
        private const int MaxDimension = 1 << 20;       // 1,048,576 px per side
        private const long MaxOutputBytes = 1L << 29;   // 512 MiB decoded

        public static byte[] DecodeG4(byte[] data, int columns, int rows, bool blackIs1)
        {
            if (columns <= 0 || columns > MaxDimension || rows > MaxDimension)
            {
                throw new ArgumentOutOfRangeException(nameof(columns),
                    $"CCITT dimensions out of range: columns={columns}, rows={rows}");
            }

            var reader = new BitReader(data);
            int rowBytes = (columns + 7) / 8;
            int maxRows = (int)Math.Min(MaxDimension, MaxOutputBytes / Math.Max(rowBytes, 1));
            if (rows > maxRows)
            {
                throw new ArgumentOutOfRangeException(nameof(rows),
                    $"CCITT image too large: {(long)rowBytes * rows} bytes exceeds the {MaxOutputBytes}-byte cap");
            }

            byte[] output = new byte[rowBytes * Math.Max(rows, 0)];
            var decoded = new List<bool[]>();
            int[] reference = { columns, columns };

            int produced = 0;
            while ((rows <= 0 || produced < rows) && produced < maxRows)
            {
                int[]? current = DecodeRow(reader, reference, columns);
                if (current == null)
                {
                    break;
                }
                bool[] line = LineFromChanges(current, columns);
                decoded.Add(line);
                reference = current;
                produced++;
                if (rows <= 0 && reader.AtEnd)
                {
                    break;
                }
            }

            if (rows <= 0)
            {
                rowBytes = (columns + 7) / 8;
                output = new byte[rowBytes * decoded.Count];
            }
            for (int row = 0; row < decoded.Count && row < (rows <= 0 ? decoded.Count : rows); row++)
            {
                PackRow(decoded[row], output, row, columns, blackIs1);
            }
            return output;
        }

        private static void EncodeRow(BitWriter writer, int[] current, int[] reference, int columns)
        {
            int a0 = -1;
            bool color = false; // false = white, true = black
            while (a0 < columns)
            {
                int a1 = NextChange(current, a0);
                int b1 = FindB1(reference, a0, color);
                int b2 = b1 < columns ? NextChange(reference, b1) : columns;

                if (b2 < a1)
                {
                    writer.WriteBits(0x1, 4); // Pass: 0001
                    a0 = b2;
                    continue;
                }

                int delta = a1 - b1;
                if (delta >= -3 && delta <= 3)
                {
                    WriteVerticalCode(writer, delta);
                    a0 = a1;
                    color = !color;
                }
                else
                {
                    writer.WriteBits(0x1, 3); // Horizontal: 001
                    int a2 = NextChange(current, a1);
                    int run1 = a1 - (a0 < 0 ? 0 : a0);
                    int run2 = a2 - a1;
                    WriteRun(writer, run1, color);
                    WriteRun(writer, run2, !color);
                    a0 = a2;
                }
            }
        }

        private static int[]? DecodeRow(BitReader reader, int[] reference, int columns)
        {
            var changes = new List<int>();
            int a0 = -1;
            bool color = false;

            while (a0 < columns)
            {
                int mode = ReadMode(reader);
                if (mode == int.MinValue)
                {
                    return changes.Count == 0 && a0 < 0 ? null : Finish(changes, columns);
                }

                int b1 = FindB1(reference, a0, color);
                int b2 = b1 < columns ? NextChange(reference, b1) : columns;

                if (mode == Pass)
                {
                    a0 = b2;
                }
                else if (mode == Horizontal)
                {
                    int start = a0 < 0 ? 0 : a0;
                    int run1 = ReadRun(reader, color);
                    int run2 = ReadRun(reader, !color);
                    int a1 = start + run1;
                    int a2 = a1 + run2;
                    AddChange(changes, a1, columns);
                    AddChange(changes, a2, columns);
                    a0 = a2;
                }
                else
                {
                    int a1 = b1 + mode; // mode = vertical delta (-3..3)
                    AddChange(changes, a1, columns);
                    a0 = a1;
                    color = !color;
                }
            }
            return Finish(changes, columns);
        }

        private static int[] Finish(List<int> changes, int columns)
        {
            changes.Add(columns);
            changes.Add(columns);
            return changes.ToArray();
        }

        private static void AddChange(List<int> changes, int position, int columns)
        {
            int clamped = position < 0 ? 0 : (position > columns ? columns : position);
            changes.Add(clamped);
        }

        // First changing element on the reference line to the right of a0 whose colour is
        // opposite to the current colour (i.e. the run it begins is opposite). Reference
        // changes alternate colours starting with a white→black transition at index 0.
        private static int FindB1(int[] reference, int a0, bool color)
        {
            int i = 0;
            while (i < reference.Length && reference[i] <= a0)
            {
                i++;
            }
            // reference[i] begins a black run when i is even (0-based). Opposite of `color`
            // means: if a0 is white (color=false) we want a white→black change → even index.
            bool changeStartsBlack = (i & 1) == 0;
            bool wantStartsBlack = !color ? true : false;
            if (changeStartsBlack != wantStartsBlack && i < reference.Length)
            {
                i++;
            }
            return i < reference.Length ? reference[i] : reference[reference.Length - 1];
        }

        private static int NextChange(int[] changes, int position)
        {
            for (int i = 0; i < changes.Length; i++)
            {
                if (changes[i] > position)
                {
                    return changes[i];
                }
            }
            return changes[changes.Length - 1];
        }

        private static void WriteVerticalCode(BitWriter writer, int delta)
        {
            switch (delta)
            {
                case 0: writer.WriteBits(0x1, 1); break;       // V0   = 1
                case 1: writer.WriteBits(0x3, 3); break;       // VR1  = 011
                case 2: writer.WriteBits(0x3, 6); break;       // VR2  = 000011
                case 3: writer.WriteBits(0x3, 7); break;       // VR3  = 0000011
                case -1: writer.WriteBits(0x2, 3); break;      // VL1  = 010
                case -2: writer.WriteBits(0x2, 6); break;      // VL2  = 000010
                case -3: writer.WriteBits(0x2, 7); break;      // VL3  = 0000010
            }
        }

        private static void WriteRun(BitWriter writer, int run, bool black)
        {
            int remaining = run;
            while (remaining > 2560)
            {
                WriteCode(writer, MakeupCode(2560, black));
                remaining -= 2560;
            }
            if (remaining >= 64)
            {
                int makeup = (remaining / 64) * 64;
                WriteCode(writer, MakeupCode(makeup, black));
                remaining -= makeup;
            }
            WriteCode(writer, black ? BlackTerminating[remaining] : WhiteTerminating[remaining]);
        }

        private static (int Len, int Bits) MakeupCode(int run, bool black)
        {
            if (run >= 1792)
            {
                return SharedMakeup[(run - 1792) / 64];
            }
            int index = run / 64 - 1;
            return black ? BlackMakeup[index] : WhiteMakeup[index];
        }

        private static void WriteCode(BitWriter writer, (int Len, int Bits) code)
        {
            writer.WriteBits(code.Bits, code.Len);
        }

        // ── Decode-side prefix tables (built once). ──
        private static readonly Dictionary<int, int> WhiteRunLookup = BuildRunLookup(false);
        private static readonly Dictionary<int, int> BlackRunLookup = BuildRunLookup(true);
        private static readonly int WhiteMaxLen = MaxLen(WhiteRunLookup);
        private static readonly int BlackMaxLen = MaxLen(BlackRunLookup);

        private static Dictionary<int, int> BuildRunLookup(bool black)
        {
            var map = new Dictionary<int, int>();
            var terminating = black ? BlackTerminating : WhiteTerminating;
            var makeup = black ? BlackMakeup : WhiteMakeup;
            for (int run = 0; run < terminating.Length; run++)
            {
                map[Key(terminating[run])] = run;
            }
            for (int i = 0; i < makeup.Length; i++)
            {
                map[Key(makeup[i])] = (i + 1) * 64;
            }
            for (int i = 0; i < SharedMakeup.Length; i++)
            {
                map[Key(SharedMakeup[i])] = 1792 + i * 64;
            }
            return map;
        }

        private static int Key((int Len, int Bits) code) => (code.Len << 16) | code.Bits;
        private static int MaxLen(Dictionary<int, int> map)
        {
            int max = 0;
            foreach (int key in map.Keys)
            {
                max = Math.Max(max, key >> 16);
            }
            return max;
        }

        private static int ReadRun(BitReader reader, bool black)
        {
            var map = black ? BlackRunLookup : WhiteRunLookup;
            int maxLen = black ? BlackMaxLen : WhiteMaxLen;
            int total = 0;
            while (true)
            {
                int run = ReadCode(reader, map, maxLen);
                if (run < 0)
                {
                    return total;
                }
                total += run;
                if (run < 64)
                {
                    return total; // terminating code
                }
            }
        }

        private static int ReadCode(BitReader reader, Dictionary<int, int> map, int maxLen)
        {
            int bits = 0;
            for (int len = 1; len <= maxLen; len++)
            {
                int bit = reader.ReadBit();
                if (bit < 0)
                {
                    return -1;
                }
                bits = (bits << 1) | bit;
                if (map.TryGetValue((len << 16) | bits, out int run))
                {
                    return run;
                }
            }
            return -1;
        }

        // Returns the 2D mode: Pass, Horizontal, a vertical delta (-3..3), or int.MinValue at end.
        private static int ReadMode(BitReader reader)
        {
            int bit = reader.ReadBit();
            if (bit < 0)
            {
                return int.MinValue;
            }
            if (bit == 1)
            {
                return 0; // V0 = 1
            }
            // 0...
            bit = reader.ReadBit();
            if (bit < 0) { return int.MinValue; }
            if (bit == 1)
            {
                // 01x: VR1 (011) or VL1 (010)
                return reader.ReadBit() == 1 ? 1 : -1;
            }
            // 00...
            bit = reader.ReadBit();
            if (bit < 0) { return int.MinValue; }
            if (bit == 1)
            {
                return Horizontal; // 001
            }
            // 000...
            bit = reader.ReadBit();
            if (bit < 0) { return int.MinValue; }
            if (bit == 1)
            {
                return Pass; // 0001
            }
            // 0000...
            bit = reader.ReadBit();
            if (bit < 0) { return int.MinValue; }
            if (bit == 1)
            {
                // 00001x: VR2 (000011) or VL2 (000010)
                return reader.ReadBit() == 1 ? 2 : -2;
            }
            // 00000...
            bit = reader.ReadBit();
            if (bit < 0) { return int.MinValue; }
            if (bit == 1)
            {
                // 000001x: VR3 (0000011) or VL3 (0000010)
                return reader.ReadBit() == 1 ? 3 : -3;
            }
            // A long run of zeros → EOL/EOFB; treat as end of data.
            return int.MinValue;
        }

        private static bool[] UnpackRow(byte[] packed, int row, int columns, bool blackIs1)
        {
            int rowBytes = (columns + 7) / 8;
            int offset = row * rowBytes;
            var line = new bool[columns]; // true = black
            for (int x = 0; x < columns; x++)
            {
                int byteIndex = offset + (x >> 3);
                int bit = byteIndex < packed.Length ? (packed[byteIndex] >> (7 - (x & 7))) & 1 : 0;
                // Default (blackIs1=false): sample 0 = black. blackIs1=true: sample 1 = black.
                line[x] = blackIs1 ? bit == 1 : bit == 0;
            }
            return line;
        }

        private static void PackRow(bool[] line, byte[] output, int row, int columns, bool blackIs1)
        {
            int rowBytes = (columns + 7) / 8;
            int offset = row * rowBytes;
            for (int x = 0; x < columns; x++)
            {
                bool black = line[x];
                int sample = blackIs1 ? (black ? 1 : 0) : (black ? 0 : 1);
                if (sample == 1)
                {
                    output[offset + (x >> 3)] |= (byte)(1 << (7 - (x & 7)));
                }
            }
        }

        private static int[] ChangingElements(bool[] line, int columns)
        {
            var changes = new List<int>();
            bool previous = false; // imaginary white before the line
            for (int x = 0; x < columns; x++)
            {
                if (line[x] != previous)
                {
                    changes.Add(x);
                    previous = line[x];
                }
            }
            changes.Add(columns);
            changes.Add(columns);
            return changes.ToArray();
        }

        private static bool[] LineFromChanges(int[] changes, int columns)
        {
            var line = new bool[columns];
            bool color = false; // white before first change
            int x = 0;
            int i = 0;
            while (x < columns)
            {
                int next = i < changes.Length ? changes[i] : columns;
                if (next > columns) { next = columns; }
                for (; x < next && x < columns; x++)
                {
                    line[x] = color;
                }
                color = !color;
                i++;
                if (i > changes.Length) { break; }
            }
            return line;
        }

        private sealed class BitWriter
        {
            private readonly List<byte> _bytes = new List<byte>();
            private int _current;
            private int _bitCount;

            public void WriteBits(int value, int length)
            {
                for (int i = length - 1; i >= 0; i--)
                {
                    int bit = (value >> i) & 1;
                    _current = (_current << 1) | bit;
                    _bitCount++;
                    if (_bitCount == 8)
                    {
                        _bytes.Add((byte)_current);
                        _current = 0;
                        _bitCount = 0;
                    }
                }
            }

            public byte[] ToArray()
            {
                if (_bitCount > 0)
                {
                    _bytes.Add((byte)(_current << (8 - _bitCount)));
                    _current = 0;
                    _bitCount = 0;
                }
                return _bytes.ToArray();
            }
        }

        private sealed class BitReader
        {
            private readonly byte[] _data;
            private int _bitPos;

            public BitReader(byte[] data) => _data = data;

            public bool AtEnd => (_bitPos >> 3) >= _data.Length;

            public int ReadBit()
            {
                int byteIndex = _bitPos >> 3;
                if (byteIndex >= _data.Length)
                {
                    return -1;
                }
                int bit = (_data[byteIndex] >> (7 - (_bitPos & 7))) & 1;
                _bitPos++;
                return bit;
            }
        }
    }
}
