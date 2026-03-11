using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Rend.Pdf
{
    /// <summary>
    /// Default implementation of <see cref="IPdfSigningService"/>.
    /// Uses PKCS#7/CMS detached signatures for PAdES-compatible PDF signing.
    /// </summary>
    public sealed class PdfSigningService : IPdfSigningService
    {
        /// <inheritdoc />
        public async Task SignAsync(Stream input, Stream output, PdfSignatureOptions options, CancellationToken cancellationToken = default)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.Signer == null) throw new ArgumentException("Signer must be set.", nameof(options));

            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
                await input.CopyToAsync(ms, 81920, cancellationToken).ConfigureAwait(false);
                pdfBytes = ms.ToArray();
            }

            var signed = await Internal.PdfSigner.SignAsync(pdfBytes, options, cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(signed, 0, signed.Length, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<byte[]> SignAsync(byte[] pdfBytes, PdfSignatureOptions options, CancellationToken cancellationToken = default)
        {
            if (pdfBytes == null) throw new ArgumentNullException(nameof(pdfBytes));
            if (pdfBytes.Length == 0) throw new ArgumentException("PDF data must not be empty.", nameof(pdfBytes));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.Signer == null) throw new ArgumentException("Signer must be set.", nameof(options));

            return await Internal.PdfSigner.SignAsync(pdfBytes, options, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task SignAsync(Stream input, Stream output, X509Certificate2 certificate,
            string? signerName = null, string? reason = null, CancellationToken cancellationToken = default)
        {
            var options = new PdfSignatureOptions
            {
                Signer = new Pkcs12Signer(certificate),
                SignerName = signerName,
                Reason = reason
            };
            return SignAsync(input, output, options, cancellationToken);
        }

        /// <inheritdoc />
        public Task<byte[]> SignAsync(byte[] pdfBytes, X509Certificate2 certificate,
            string? signerName = null, string? reason = null, CancellationToken cancellationToken = default)
        {
            var options = new PdfSignatureOptions
            {
                Signer = new Pkcs12Signer(certificate),
                SignerName = signerName,
                Reason = reason
            };
            return SignAsync(pdfBytes, options, cancellationToken);
        }
    }
}
