using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rend.Pdf
{
    /// <summary>
    /// Draws text and images onto existing PDF documents without re-rendering.
    /// Uses PDF incremental updates to preserve the original document structure.
    /// Register as a singleton in DI containers. Thread-safe.
    /// </summary>
    public interface IPdfOverlay
    {
        /// <summary>Asynchronously reads a PDF from <paramref name="input"/>, applies the overlay elements, and writes the result to <paramref name="output"/>.</summary>
        /// <param name="input">Stream containing the source PDF.</param>
        /// <param name="output">Stream to write the modified PDF to.</param>
        /// <param name="elements">The text and image elements to draw onto the PDF pages.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task ApplyAsync(Stream input, Stream output, IEnumerable<PdfOverlayElement> elements, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously applies overlay elements to a PDF byte array and returns the modified PDF bytes.</summary>
        /// <param name="pdfBytes">The source PDF document.</param>
        /// <param name="elements">The text and image elements to draw onto the PDF pages.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The modified PDF as a byte array.</returns>
        Task<byte[]> ApplyAsync(byte[] pdfBytes, IEnumerable<PdfOverlayElement> elements, CancellationToken cancellationToken = default);
    }
}
