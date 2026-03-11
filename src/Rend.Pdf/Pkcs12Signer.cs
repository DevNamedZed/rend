using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Rend.Pdf
{
    /// <summary>
    /// Signs PDF documents using a local PKCS#12 (.pfx/.p12) certificate.
    /// </summary>
    public sealed class Pkcs12Signer : IPdfSigner, IDisposable
    {
        private readonly X509Certificate2 _certificate;
        private readonly bool _ownsCertificate;

        /// <summary>
        /// Creates a signer from a <see cref="X509Certificate2"/> instance.
        /// The certificate must contain a private key.
        /// The caller retains ownership of the certificate; it will not be disposed by this signer.
        /// </summary>
        public Pkcs12Signer(X509Certificate2 certificate)
        {
            _certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
            if (!certificate.HasPrivateKey)
                throw new ArgumentException("Certificate must contain a private key.", nameof(certificate));
            _ownsCertificate = false;
        }

        /// <summary>
        /// Creates a signer from PKCS#12 (.pfx/.p12) bytes.
        /// The signer owns the created certificate and will dispose it when <see cref="Dispose"/> is called.
        /// </summary>
        public Pkcs12Signer(byte[] pfxData, string? password = null)
        {
            if (pfxData == null || pfxData.Length == 0)
                throw new ArgumentException("PFX data must not be null or empty.", nameof(pfxData));

            _certificate = new X509Certificate2(pfxData, password, X509KeyStorageFlags.Exportable);
            if (!_certificate.HasPrivateKey)
                throw new ArgumentException("The PFX does not contain a private key.");
            _ownsCertificate = true;
        }

        /// <summary>
        /// Disposes the underlying certificate if this signer owns it (created from byte[]).
        /// </summary>
        public void Dispose()
        {
            if (_ownsCertificate) _certificate.Dispose();
        }

        /// <inheritdoc />
        public int EstimatedSignatureSize => 8192;

        /// <inheritdoc />
        public Task<byte[]> SignAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var contentInfo = new ContentInfo(data);
            var signedCms = new SignedCms(contentInfo, detached: true);
            var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, _certificate)
            {
                DigestAlgorithm = new Oid("2.16.840.1.101.3.4.2.1", "SHA256"),
                IncludeOption = X509IncludeOption.WholeChain
            };
            signedCms.ComputeSignature(signer);
            return Task.FromResult(signedCms.Encode());
        }
    }
}
