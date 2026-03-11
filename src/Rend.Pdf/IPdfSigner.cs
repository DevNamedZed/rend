using System.Threading;
using System.Threading.Tasks;

namespace Rend.Pdf
{
    /// <summary>
    /// Produces a PKCS#7/CMS detached signature for PDF signing.
    /// Implement this for external signing (HSM, cloud KMS, etc.)
    /// or use <see cref="Pkcs12Signer"/> for local certificate-based signing.
    /// </summary>
    public interface IPdfSigner
    {
        /// <summary>
        /// Asynchronously produce a PKCS#7/CMS detached signature for the given data.
        /// For local signing (e.g. PKCS#12), return <see cref="Task.FromResult{TResult}"/>.
        /// For HSM or cloud KMS, perform the remote signing call here.
        /// </summary>
        /// <param name="data">The PDF byte ranges to be signed (concatenated).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>DER-encoded PKCS#7/CMS detached signature bytes.</returns>
        Task<byte[]> SignAsync(byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Maximum size in bytes of the signature output.
        /// Used to reserve space in the PDF. Must be large enough to hold
        /// the full CMS signature including certificate chain.
        /// Typical value: 8192.
        /// </summary>
        int EstimatedSignatureSize { get; }
    }
}
