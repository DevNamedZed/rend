using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rend.Pdf
{
    /// <summary>
    /// Static convenience API for drawing text and images onto existing PDF documents.
    /// For dependency injection or testing, use <see cref="IPdfOverlay"/> and <see cref="PdfOverlay"/> directly.
    /// </summary>
    public static class PdfOverlays
    {
        private static readonly PdfOverlay Instance = new PdfOverlay();

        /// <inheritdoc cref="IPdfOverlay.ApplyAsync(Stream, Stream, IEnumerable{PdfOverlayElement}, CancellationToken)"/>
        public static Task ApplyAsync(Stream input, Stream output, IEnumerable<PdfOverlayElement> elements, CancellationToken cancellationToken = default)
            => Instance.ApplyAsync(input, output, elements, cancellationToken);

        /// <inheritdoc cref="IPdfOverlay.ApplyAsync(byte[], IEnumerable{PdfOverlayElement}, CancellationToken)"/>
        public static Task<byte[]> ApplyAsync(byte[] pdfBytes, IEnumerable<PdfOverlayElement> elements, CancellationToken cancellationToken = default)
            => Instance.ApplyAsync(pdfBytes, elements, cancellationToken);
    }
}
