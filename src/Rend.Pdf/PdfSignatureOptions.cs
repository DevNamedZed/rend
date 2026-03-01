namespace Rend.Pdf
{
    /// <summary>
    /// Configuration options for digitally signing a PDF document (PAdES-compatible).
    /// Requires a PKCS#12 (.pfx/.p12) certificate containing a private key.
    /// </summary>
    public sealed class PdfSignatureOptions
    {
        /// <summary>PKCS#12 certificate data (.pfx/.p12) containing private key.</summary>
        public byte[]? CertificateData { get; set; }

        /// <summary>Password for the PKCS#12 certificate.</summary>
        public string? CertificatePassword { get; set; }

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
