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
        /// Produce a PKCS#7/CMS detached signature for the given data.
        /// </summary>
        /// <param name="data">The PDF byte ranges to be signed (concatenated).</param>
        /// <returns>DER-encoded PKCS#7/CMS detached signature bytes.</returns>
        byte[] Sign(byte[] data);

        /// <summary>
        /// Maximum size in bytes of the signature output.
        /// Used to reserve space in the PDF. Must be large enough to hold
        /// the full CMS signature including certificate chain.
        /// Typical value: 8192.
        /// </summary>
        int EstimatedSignatureSize { get; }
    }
}
