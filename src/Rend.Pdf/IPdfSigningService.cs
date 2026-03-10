using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Rend.Pdf
{
    /// <summary>
    /// Signs PDF documents with PAdES-compatible digital signatures.
    /// </summary>
    public interface IPdfSigningService
    {
        void Sign(Stream input, Stream output, PdfSignatureOptions options);
        byte[] Sign(byte[] pdfBytes, PdfSignatureOptions options);
        void Sign(Stream input, Stream output, X509Certificate2 certificate, string? signerName = null, string? reason = null);
        byte[] Sign(byte[] pdfBytes, X509Certificate2 certificate, string? signerName = null, string? reason = null);
    }
}
