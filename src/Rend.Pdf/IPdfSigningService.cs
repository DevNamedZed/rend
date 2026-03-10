using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Rend.Pdf
{
    /// <summary>
    /// Signs PDF documents with PAdES-compatible digital signatures.
    /// Register as a singleton in DI containers. Thread-safe.
    /// </summary>
    public interface IPdfSigningService
    {
        /// <summary>Reads a PDF from <paramref name="input"/>, signs it, and writes the signed PDF to <paramref name="output"/>.</summary>
        /// <param name="input">Stream containing the PDF to sign.</param>
        /// <param name="output">Stream to write the signed PDF to.</param>
        /// <param name="options">Signature configuration (signer, name, reason, etc.).</param>
        void Sign(Stream input, Stream output, PdfSignatureOptions options);

        /// <summary>Signs a PDF byte array and returns the signed PDF bytes.</summary>
        /// <param name="pdfBytes">The PDF document to sign.</param>
        /// <param name="options">Signature configuration (signer, name, reason, etc.).</param>
        /// <returns>The signed PDF as a byte array.</returns>
        byte[] Sign(byte[] pdfBytes, PdfSignatureOptions options);

        /// <summary>Reads a PDF from <paramref name="input"/>, signs it with the specified certificate, and writes to <paramref name="output"/>.</summary>
        /// <param name="input">Stream containing the PDF to sign.</param>
        /// <param name="output">Stream to write the signed PDF to.</param>
        /// <param name="certificate">X.509 certificate with private key for signing.</param>
        /// <param name="signerName">Optional signer name displayed in the signature field.</param>
        /// <param name="reason">Optional reason for signing.</param>
        void Sign(Stream input, Stream output, X509Certificate2 certificate, string? signerName = null, string? reason = null);

        /// <summary>Signs a PDF byte array with the specified certificate and returns the signed PDF bytes.</summary>
        /// <param name="pdfBytes">The PDF document to sign.</param>
        /// <param name="certificate">X.509 certificate with private key for signing.</param>
        /// <param name="signerName">Optional signer name displayed in the signature field.</param>
        /// <param name="reason">Optional reason for signing.</param>
        /// <returns>The signed PDF as a byte array.</returns>
        byte[] Sign(byte[] pdfBytes, X509Certificate2 certificate, string? signerName = null, string? reason = null);
    }
}
