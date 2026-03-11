using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// Flate (zlib) compression utility for PDF streams.
    /// PDF FlateDecode expects zlib-wrapped data (RFC 1950): 2-byte header + deflate + Adler32 checksum.
    /// </summary>
    internal static class FlateHelper
    {
        /// <summary>
        /// Compress data using zlib format with Optimal level. Returns the compressed bytes.
        /// </summary>
        public static byte[] Compress(byte[] data)
            => Compress(data, CompressionLevel.Optimal);

        /// <summary>
        /// Compress data using zlib format at the specified compression level. Returns the compressed bytes.
        /// </summary>
        public static byte[] Compress(byte[] data, CompressionLevel level)
        {
            if (data.Length == 0) return data;

#if NET6_0_OR_GREATER
            // Estimate: compressed data is typically 40-60% of original
            int estimate = Math.Max(data.Length / 2, 64);
            using (var output = new MemoryStream(estimate))
            {
                using (var zlib = new ZLibStream(output, level, leaveOpen: true))
                {
                    zlib.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
#else
            return CompressZlib(data, level);
#endif
        }

        /// <summary>
        /// Compress data from a stream with Optimal level. Writes compressed bytes to the output stream.
        /// </summary>
        public static void Compress(Stream input, Stream output)
            => Compress(input, output, CompressionLevel.Optimal);

        /// <summary>
        /// Compress data from a stream at the specified compression level. Writes compressed bytes to the output stream.
        /// </summary>
        public static void Compress(Stream input, Stream output, CompressionLevel level)
        {
#if NET6_0_OR_GREATER
            using (var zlib = new ZLibStream(output, level, leaveOpen: true))
            {
                var buffer = ArrayPool<byte>.Shared.Rent(8192);
                try
                {
                    int bytesRead;
                    while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        zlib.Write(buffer, 0, bytesRead);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
#else
            // Read all input, compress with zlib wrapper
            using (var ms = new MemoryStream())
            {
                var buffer = ArrayPool<byte>.Shared.Rent(8192);
                try
                {
                    int bytesRead;
                    while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                        ms.Write(buffer, 0, bytesRead);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
                byte[] compressed = CompressZlib(ms.ToArray(), level);
                output.Write(compressed, 0, compressed.Length);
            }
#endif
        }

        /// <summary>
        /// Decompress Flate-encoded data. Used for PNG image processing.
        /// Handles both zlib-wrapped (RFC 1950) and raw deflate (RFC 1951) data.
        /// </summary>
        public static byte[] Decompress(byte[] data)
        {
#if NET6_0_OR_GREATER
            // Try zlib first, fall back to raw deflate for PNG IDAT chunks
            try
            {
                using (var input = new MemoryStream(data))
                using (var zlib = new ZLibStream(input, CompressionMode.Decompress))
                using (var output = new MemoryStream(data.Length * 2))
                {
                    var buffer = ArrayPool<byte>.Shared.Rent(8192);
                    try
                    {
                        int bytesRead;
                        while ((bytesRead = zlib.Read(buffer, 0, buffer.Length)) > 0)
                            output.Write(buffer, 0, bytesRead);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                    return output.ToArray();
                }
            }
            catch
            {
                // Fall back to raw deflate
                return DecompressRaw(data);
            }
#else
            return DecompressRaw(data);
#endif
        }

        private static byte[] DecompressRaw(byte[] data)
        {
            // Skip zlib header if present (2 bytes starting with 0x78)
            int offset = 0;
            if (data.Length >= 2 && data[0] == 0x78)
                offset = 2;

            using (var input = new MemoryStream(data, offset, data.Length - offset))
            using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream(data.Length * 2))
            {
                var buffer = ArrayPool<byte>.Shared.Rent(8192);
                try
                {
                    int bytesRead;
                    while ((bytesRead = deflate.Read(buffer, 0, buffer.Length)) > 0)
                        output.Write(buffer, 0, bytesRead);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
                return output.ToArray();
            }
        }

#if !NET6_0_OR_GREATER
        /// <summary>
        /// Compress with manual zlib wrapper for netstandard2.0 (no ZLibStream).
        /// Adds 2-byte zlib header (0x78 0x9C) and 4-byte Adler32 checksum.
        /// </summary>
        private static byte[] CompressZlib(byte[] data, CompressionLevel level)
        {
            using (var output = new MemoryStream(data.Length / 2 + 6))
            {
                // Zlib header: CMF=0x78 (deflate, 32K window), FLG=0x9C (level=optimal, check bits)
                output.WriteByte(0x78);
                output.WriteByte(0x9C);

                using (var deflate = new DeflateStream(output, level, leaveOpen: true))
                {
                    deflate.Write(data, 0, data.Length);
                }

                // Adler32 checksum (big-endian)
                uint adler = Adler32(data);
                output.WriteByte((byte)(adler >> 24));
                output.WriteByte((byte)(adler >> 16));
                output.WriteByte((byte)(adler >> 8));
                output.WriteByte((byte)adler);

                return output.ToArray();
            }
        }

        private static uint Adler32(byte[] data)
        {
            const uint MOD = 65521;
            // NMAX: max bytes we can sum before uint32 overflow requires modulo
            // sum(255 * NMAX) < 2^31 → NMAX = 5552
            const int NMAX = 5552;
            uint a = 1, b = 0;
            int i = 0;
            int len = data.Length;

            while (i < len)
            {
                int blockLen = Math.Min(NMAX, len - i);
                int end = i + blockLen;
                for (; i < end; i++)
                {
                    a += data[i];
                    b += a;
                }
                a %= MOD;
                b %= MOD;
            }

            return (b << 16) | a;
        }
#endif
    }
}
