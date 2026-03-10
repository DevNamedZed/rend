using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Rend.Pdf
{
    /// <summary>
    /// Static convenience facade over <see cref="PdfSigningService"/>.
    /// For DI or testing, use <see cref="IPdfSigningService"/> and <see cref="PdfSigningService"/> directly.
    /// </summary>
    public static class PdfSigning
    {
        private static readonly PdfSigningService Instance = new PdfSigningService();

        public static void Sign(Stream input, Stream output, PdfSignatureOptions options)
            => Instance.Sign(input, output, options);

        public static byte[] Sign(byte[] pdfBytes, PdfSignatureOptions options)
            => Instance.Sign(pdfBytes, options);

        public static void Sign(Stream input, Stream output, X509Certificate2 certificate,
            string? signerName = null, string? reason = null)
            => Instance.Sign(input, output, certificate, signerName, reason);

        public static byte[] Sign(byte[] pdfBytes, X509Certificate2 certificate,
            string? signerName = null, string? reason = null)
            => Instance.Sign(pdfBytes, certificate, signerName, reason);
    }
}
