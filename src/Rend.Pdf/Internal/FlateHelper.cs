using System.Buffers;
using System.IO;
using System.IO.Compression;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// Flate (Deflate) compression utility for PDF streams.
    /// PDF FlateDecode expects raw deflate data (RFC 1951).
    /// </summary>
    internal static class FlateHelper
    {
        /// <summary>
        /// Compress data using Deflate with Optimal level. Returns the compressed bytes.
        /// </summary>
        public static byte[] Compress(byte[] data)
            => Compress(data, CompressionLevel.Optimal);

        /// <summary>
        /// Compress data using Deflate at the specified compression level. Returns the compressed bytes.
        /// </summary>
        public static byte[] Compress(byte[] data, CompressionLevel level)
        {
            if (data.Length == 0) return data;

            using (var output = new MemoryStream(data.Length / 2))
            {
                using (var deflate = new DeflateStream(output, level, leaveOpen: true))
                {
                    deflate.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }

        /// <summary>
        /// Compress data from a stream with Optimal level. Writes compressed bytes to the output stream.
        /// </summary>
        public static void Compress(Stream input, Stream output)
            => Compress(input, output, CompressionLevel.Optimal);

        /// <summary>
        /// Compress data from a stream at the specified compression level. Writes compressed bytes to the output stream.
        /// Uses pooled buffers for the copy.
        /// </summary>
        public static void Compress(Stream input, Stream output, CompressionLevel level)
        {
            using (var deflate = new DeflateStream(output, level, leaveOpen: true))
            {
                var buffer = ArrayPool<byte>.Shared.Rent(8192);
                try
                {
                    int bytesRead;
                    while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        deflate.Write(buffer, 0, bytesRead);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }

        /// <summary>
        /// Decompress Flate-encoded data. Used for PNG image processing.
        /// </summary>
        public static byte[] Decompress(byte[] data)
        {
            using (var input = new MemoryStream(data))
            using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream(data.Length * 2))
            {
                var buffer = ArrayPool<byte>.Shared.Rent(8192);
                try
                {
                    int bytesRead;
                    while ((bytesRead = deflate.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
                return output.ToArray();
            }
        }
    }
}
