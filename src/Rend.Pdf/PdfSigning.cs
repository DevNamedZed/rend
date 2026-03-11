using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Rend.Pdf
{
    /// <summary>
    /// Static convenience API for signing PDF documents.
    /// For dependency injection or testing, use <see cref="IPdfSigningService"/> and <see cref="PdfSigningService"/> directly.
    /// </summary>
    public static class PdfSigning
    {
        private static readonly PdfSigningService Instance = new PdfSigningService();

        /// <inheritdoc cref="IPdfSigningService.SignAsync(Stream, Stream, PdfSignatureOptions, CancellationToken)"/>
        public static Task SignAsync(Stream input, Stream output, PdfSignatureOptions options, CancellationToken cancellationToken = default)
            => Instance.SignAsync(input, output, options, cancellationToken);

        /// <inheritdoc cref="IPdfSigningService.SignAsync(byte[], PdfSignatureOptions, CancellationToken)"/>
        public static Task<byte[]> SignAsync(byte[] pdfBytes, PdfSignatureOptions options, CancellationToken cancellationToken = default)
            => Instance.SignAsync(pdfBytes, options, cancellationToken);

        /// <inheritdoc cref="IPdfSigningService.SignAsync(Stream, Stream, X509Certificate2, string?, string?, CancellationToken)"/>
        public static Task SignAsync(Stream input, Stream output, X509Certificate2 certificate,
            string? signerName = null, string? reason = null, CancellationToken cancellationToken = default)
            => Instance.SignAsync(input, output, certificate, signerName, reason, cancellationToken);

        /// <inheritdoc cref="IPdfSigningService.SignAsync(byte[], X509Certificate2, string?, string?, CancellationToken)"/>
        public static Task<byte[]> SignAsync(byte[] pdfBytes, X509Certificate2 certificate,
            string? signerName = null, string? reason = null, CancellationToken cancellationToken = default)
            => Instance.SignAsync(pdfBytes, certificate, signerName, reason, cancellationToken);
    }
}
