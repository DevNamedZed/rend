using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Rend.Pdf
{
    /// <summary>
    /// Signs PDF documents with PAdES-compatible digital signatures.
    /// Register as a singleton in DI containers. Thread-safe.
    /// </summary>
    public interface IPdfSigningService
    {
        /// <summary>Asynchronously reads a PDF from <paramref name="input"/>, signs it, and writes the signed PDF to <paramref name="output"/>.</summary>
        /// <param name="input">Stream containing the PDF to sign.</param>
        /// <param name="output">Stream to write the signed PDF to.</param>
        /// <param name="options">Signature configuration (signer, name, reason, etc.).</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task SignAsync(Stream input, Stream output, PdfSignatureOptions options, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously signs a PDF byte array and returns the signed PDF bytes.</summary>
        /// <param name="pdfBytes">The PDF document to sign.</param>
        /// <param name="options">Signature configuration (signer, name, reason, etc.).</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The signed PDF as a byte array.</returns>
        Task<byte[]> SignAsync(byte[] pdfBytes, PdfSignatureOptions options, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously reads a PDF from <paramref name="input"/>, signs it with the specified certificate, and writes to <paramref name="output"/>.</summary>
        /// <param name="input">Stream containing the PDF to sign.</param>
        /// <param name="output">Stream to write the signed PDF to.</param>
        /// <param name="certificate">X.509 certificate with private key for signing.</param>
        /// <param name="signerName">Optional signer name displayed in the signature field.</param>
        /// <param name="reason">Optional reason for signing.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task SignAsync(Stream input, Stream output, X509Certificate2 certificate, string? signerName = null, string? reason = null, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously signs a PDF byte array with the specified certificate and returns the signed PDF bytes.</summary>
        /// <param name="pdfBytes">The PDF document to sign.</param>
        /// <param name="certificate">X.509 certificate with private key for signing.</param>
        /// <param name="signerName">Optional signer name displayed in the signature field.</param>
        /// <param name="reason">Optional reason for signing.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The signed PDF as a byte array.</returns>
        Task<byte[]> SignAsync(byte[] pdfBytes, X509Certificate2 certificate, string? signerName = null, string? reason = null, CancellationToken cancellationToken = default);
    }
}
