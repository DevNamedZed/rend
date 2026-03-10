using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Rend.Pdf
{
    /// <summary>
    /// Default implementation of <see cref="IPdfSigningService"/>.
    /// Uses PKCS#7/CMS detached signatures for PAdES-compatible PDF signing.
    /// </summary>
    public sealed class PdfSigningService : IPdfSigningService
    {
        /// <inheritdoc />
        public void Sign(Stream input, Stream output, PdfSignatureOptions options)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.Signer == null) throw new ArgumentException("Signer must be set.", nameof(options));

            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                pdfBytes = ms.ToArray();
            }

            var signed = Internal.PdfSigner.Sign(pdfBytes, options);
            output.Write(signed, 0, signed.Length);
        }

        /// <inheritdoc />
        public byte[] Sign(byte[] pdfBytes, PdfSignatureOptions options)
        {
            if (pdfBytes == null || pdfBytes.Length == 0)
                throw new ArgumentException("PDF data must not be null or empty.", nameof(pdfBytes));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.Signer == null) throw new ArgumentException("Signer must be set.", nameof(options));

            return Internal.PdfSigner.Sign(pdfBytes, options);
        }

        /// <inheritdoc />
        public void Sign(Stream input, Stream output, X509Certificate2 certificate,
            string? signerName = null, string? reason = null)
        {
            var options = new PdfSignatureOptions
            {
                Signer = new Pkcs12Signer(certificate),
                SignerName = signerName,
                Reason = reason
            };
            Sign(input, output, options);
        }

        /// <inheritdoc />
        public byte[] Sign(byte[] pdfBytes, X509Certificate2 certificate,
            string? signerName = null, string? reason = null)
        {
            var options = new PdfSignatureOptions
            {
                Signer = new Pkcs12Signer(certificate),
                SignerName = signerName,
                Reason = reason
            };
            return Sign(pdfBytes, options);
        }
    }
}
