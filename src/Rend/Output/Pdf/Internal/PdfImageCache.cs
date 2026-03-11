using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Rend.Pdf;

namespace Rend.Output.Pdf.Internal
{
    /// <summary>
    /// Deduplicates image embeddings in a PDF document by content hash.
    /// If the same image data is drawn multiple times, only one copy is embedded.
    /// </summary>
    internal sealed class PdfImageCache
    {
        private readonly Dictionary<string, PdfImage> _cache = new Dictionary<string, PdfImage>(StringComparer.Ordinal);

        /// <summary>
        /// Gets an existing cached image or adds a new one to the PDF document.
        /// </summary>
        /// <param name="imageData">The raw image bytes.</param>
        /// <param name="format">The image format string (e.g. "png", "jpeg").</param>
        /// <param name="doc">The PDF document to add the image to.</param>
        /// <returns>A PDF image resource for use in content streams.</returns>
        internal PdfImage GetOrAdd(byte[] imageData, string format, PdfDocument doc)
        {
            string hash = ComputeHash(imageData);
            if (_cache.TryGetValue(hash, out var existing))
            {
                return existing;
            }

            ImageFormat pdfFormat = ParseImageFormat(format);
            PdfImage image = doc.AddImage(imageData, pdfFormat);
            _cache[hash] = image;
            return image;
        }

        private static ImageFormat ParseImageFormat(string format)
        {
            if (string.Equals(format, "jpeg", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(format, "jpg", StringComparison.OrdinalIgnoreCase))
            {
                return ImageFormat.Jpeg;
            }

            // Default to PNG for all other formats.
            return ImageFormat.Png;
        }

        [ThreadStatic] private static SHA256? t_sha256;

        private static string ComputeHash(byte[] data)
        {
            var sha = t_sha256 ??= SHA256.Create();
            byte[] hash = sha.ComputeHash(data);
#if NET5_0_OR_GREATER
            return Convert.ToHexString(hash);
#else
            return BitConverter.ToString(hash);
#endif
        }
    }
}
