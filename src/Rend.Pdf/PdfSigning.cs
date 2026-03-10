using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Rend.Pdf
{
    /// <summary>
    /// Static convenience API for signing PDF documents.
    /// For dependency injection or testing, use <see cref="IPdfSigningService"/> and <see cref="PdfSigningService"/> directly.
    /// </summary>
    public static class PdfSigning
    {
        private static readonly PdfSigningService Instance = new PdfSigningService();

        /// <inheritdoc cref="IPdfSigningService.Sign(Stream, Stream, PdfSignatureOptions)"/>
        public static void Sign(Stream input, Stream output, PdfSignatureOptions options)
            => Instance.Sign(input, output, options);

        /// <inheritdoc cref="IPdfSigningService.Sign(byte[], PdfSignatureOptions)"/>
        public static byte[] Sign(byte[] pdfBytes, PdfSignatureOptions options)
            => Instance.Sign(pdfBytes, options);

        /// <inheritdoc cref="IPdfSigningService.Sign(Stream, Stream, X509Certificate2, string?, string?)"/>
        public static void Sign(Stream input, Stream output, X509Certificate2 certificate,
            string? signerName = null, string? reason = null)
            => Instance.Sign(input, output, certificate, signerName, reason);

        /// <inheritdoc cref="IPdfSigningService.Sign(byte[], X509Certificate2, string?, string?)"/>
        public static byte[] Sign(byte[] pdfBytes, X509Certificate2 certificate,
            string? signerName = null, string? reason = null)
            => Instance.Sign(pdfBytes, certificate, signerName, reason);
    }
}
