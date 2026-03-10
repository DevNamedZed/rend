namespace Rend.Pdf
{
    /// <summary>
    /// Configuration for digitally signing a PDF document (PAdES-compatible).
    /// </summary>
    public sealed class PdfSignatureOptions
    {
        /// <summary>
        /// The signer that produces the PKCS#7/CMS detached signature.
        /// Use <see cref="Pkcs12Signer"/> for local certificate signing,
        /// or implement <see cref="IPdfSigner"/> for external signing (HSM, cloud KMS).
        /// </summary>
        public IPdfSigner Signer { get; set; } = null!;

        /// <summary>Signer name (displayed in signature field).</summary>
        public string? SignerName { get; set; }

        /// <summary>Reason for signing.</summary>
        public string? Reason { get; set; }

        /// <summary>Location of signing.</summary>
        public string? Location { get; set; }

        /// <summary>Contact information.</summary>
        public string? ContactInfo { get; set; }
    }
}
