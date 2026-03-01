using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// Low-level PDF byte writer. Buffers output and tracks byte position for xref offsets.
    /// Designed for minimal allocations — all number formatting is done directly into the buffer.
    /// </summary>
    internal sealed class PdfWriter : IDisposable
    {
        // Pre-encoded constant byte sequences
        public static readonly byte[] Bytes_true = { (byte)'t', (byte)'r', (byte)'u', (byte)'e' };
        public static readonly byte[] Bytes_false = { (byte)'f', (byte)'a', (byte)'l', (byte)'s', (byte)'e' };
        public static readonly byte[] Bytes_null = { (byte)'n', (byte)'u', (byte)'l', (byte)'l' };
        public static readonly byte[] Bytes_ObjRef = { (byte)' ', (byte)'R' };
        public static readonly byte[] Bytes_ObjStart = { (byte)' ', (byte)'o', (byte)'b', (byte)'j', (byte)'\n' };
        public static readonly byte[] Bytes_ObjEnd = { (byte)'\n', (byte)'e', (byte)'n', (byte)'d', (byte)'o', (byte)'b', (byte)'j', (byte)'\n' };
        public static readonly byte[] Bytes_DictOpen = { (byte)'<', (byte)'<', (byte)'\n' };
        public static readonly byte[] Bytes_DictClose = { (byte)'>', (byte)'>' };
        public static readonly byte[] Bytes_stream = { (byte)'s', (byte)'t', (byte)'r', (byte)'e', (byte)'a', (byte)'m' };
        public static readonly byte[] Bytes_endstream = { (byte)'e', (byte)'n', (byte)'d', (byte)'s', (byte)'t', (byte)'r', (byte)'e', (byte)'a', (byte)'m' };

        private static readonly byte[] HexChars = {
            (byte)'0', (byte)'1', (byte)'2', (byte)'3',
            (byte)'4', (byte)'5', (byte)'6', (byte)'7',
            (byte)'8', (byte)'9', (byte)'A', (byte)'B',
            (byte)'C', (byte)'D', (byte)'E', (byte)'F'
        };

        private readonly Stream _output;
        private byte[] _buffer;
        private int _bufferPos;
        private long _totalBytesWritten;

        private const int DefaultBufferSize = 65536; // 64KB

        public PdfWriter(Stream output, int bufferSize = DefaultBufferSize)
        {
            _output = output;
            _buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            _bufferPos = 0;
            _totalBytesWritten = 0;
        }

        /// <summary>
        /// Current byte position in the output stream. Used for xref offsets.
        /// </summary>
        public long Position => _totalBytesWritten + _bufferPos;

        /// <summary>Encryption engine, set when the document uses encryption.</summary>
        internal PdfEncryptor? Encryptor { get; set; }

        /// <summary>Object number of the object currently being written. Used for per-object encryption.</summary>
        internal int CurrentObjectNumber { get; set; }

        /// <summary>Generation number of the object currently being written.</summary>
        internal int CurrentGeneration { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte b)
        {
            if (_bufferPos >= _buffer.Length - 1)
                Flush();
            _buffer[_bufferPos++] = b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteHexByte(byte b)
        {
            EnsureCapacity(2);
            _buffer[_bufferPos++] = HexChars[(b >> 4) & 0x0F];
            _buffer[_bufferPos++] = HexChars[b & 0x0F];
        }

        public void WriteRaw(byte[] bytes)
        {
            if (bytes.Length > _buffer.Length / 2)
            {
                Flush();
                _output.Write(bytes, 0, bytes.Length);
                _totalBytesWritten += bytes.Length;
                return;
            }
            EnsureCapacity(bytes.Length);
            Buffer.BlockCopy(bytes, 0, _buffer, _bufferPos, bytes.Length);
            _bufferPos += bytes.Length;
        }

        public void WriteRawBytes(byte[] bytes, int offset, int count)
        {
            if (count > _buffer.Length / 2)
            {
                Flush();
                _output.Write(bytes, offset, count);
                _totalBytesWritten += count;
                return;
            }
            EnsureCapacity(count);
            Buffer.BlockCopy(bytes, offset, _buffer, _bufferPos, count);
            _bufferPos += count;
        }

        /// <summary>
        /// Write an ASCII string directly. Used for comments, headers.
        /// </summary>
        public void WriteAscii(string text)
        {
            EnsureCapacity(text.Length);
            for (int i = 0; i < text.Length; i++)
                _buffer[_bufferPos++] = (byte)text[i];
        }

        /// <summary>
        /// Write a long integer without allocating a string.
        /// </summary>
        public void WriteLong(long value)
        {
            EnsureCapacity(20); // max long digits
            if (value < 0)
            {
                _buffer[_bufferPos++] = (byte)'-';
                value = -value;
            }
            if (value == 0)
            {
                _buffer[_bufferPos++] = (byte)'0';
                return;
            }

            // Write digits in reverse, then swap
            int start = _bufferPos;
            while (value > 0)
            {
                _buffer[_bufferPos++] = (byte)('0' + (value % 10));
                value /= 10;
            }
            // Reverse the digits
            int end = _bufferPos - 1;
            while (start < end)
            {
                byte tmp = _buffer[start];
                _buffer[start] = _buffer[end];
                _buffer[end] = tmp;
                start++;
                end--;
            }
        }

        /// <summary>
        /// Write a float with up to 4 decimal places, no trailing zeros, no string allocation.
        /// </summary>
        public void WriteFloat(float value)
        {
            EnsureCapacity(32);

            if (value < 0)
            {
                _buffer[_bufferPos++] = (byte)'-';
                value = -value;
            }

            // Split into integer and fractional parts
            // Multiply by 10000 for 4 decimal places
            long scaled = (long)Math.Round(value * 10000.0);
            long intPart = scaled / 10000;
            long fracPart = scaled % 10000;

            // Write integer part
            if (intPart == 0)
            {
                _buffer[_bufferPos++] = (byte)'0';
            }
            else
            {
                int start = _bufferPos;
                while (intPart > 0)
                {
                    _buffer[_bufferPos++] = (byte)('0' + (intPart % 10));
                    intPart /= 10;
                }
                // Reverse
                int end = _bufferPos - 1;
                while (start < end)
                {
                    byte tmp = _buffer[start];
                    _buffer[start] = _buffer[end];
                    _buffer[end] = tmp;
                    start++;
                    end--;
                }
            }

            // Write fractional part (strip trailing zeros)
            if (fracPart > 0)
            {
                _buffer[_bufferPos++] = (byte)'.';

                // fracPart is 0-9999, always write as 4 digits then strip trailing zeros
                byte d1 = (byte)('0' + (fracPart / 1000));
                byte d2 = (byte)('0' + ((fracPart / 100) % 10));
                byte d3 = (byte)('0' + ((fracPart / 10) % 10));
                byte d4 = (byte)('0' + (fracPart % 10));

                _buffer[_bufferPos++] = d1;
                int trailingEnd = _bufferPos; // at least 1 digit

                _buffer[_bufferPos++] = d2;
                if (d2 != (byte)'0' || d3 != (byte)'0' || d4 != (byte)'0')
                    trailingEnd = _bufferPos;

                _buffer[_bufferPos++] = d3;
                if (d3 != (byte)'0' || d4 != (byte)'0')
                    trailingEnd = _bufferPos;

                _buffer[_bufferPos++] = d4;
                if (d4 != (byte)'0')
                    trailingEnd = _bufferPos;

                _bufferPos = trailingEnd;
            }
        }

        public void WriteSpace()
        {
            WriteByte((byte)' ');
        }

        public void WriteNewLine()
        {
            WriteByte((byte)'\n');
        }

        public void Flush()
        {
            if (_bufferPos > 0)
            {
                _output.Write(_buffer, 0, _bufferPos);
                _totalBytesWritten += _bufferPos;
                _bufferPos = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int bytes)
        {
            if (_bufferPos + bytes > _buffer.Length)
                Flush();
        }

        public void Dispose()
        {
            Flush();
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null!;
        }
    }
}
