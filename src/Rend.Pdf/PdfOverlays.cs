using System.Collections.Generic;
using System.IO;

namespace Rend.Pdf
{
    /// <summary>
    /// Static convenience API for drawing text and images onto existing PDF documents.
    /// For dependency injection or testing, use <see cref="IPdfOverlay"/> and <see cref="PdfOverlay"/> directly.
    /// </summary>
    public static class PdfOverlays
    {
        private static readonly PdfOverlay Instance = new PdfOverlay();

        /// <inheritdoc cref="IPdfOverlay.Apply(Stream, Stream, IEnumerable{PdfOverlayElement})"/>
        public static void Apply(Stream input, Stream output, IEnumerable<PdfOverlayElement> elements)
            => Instance.Apply(input, output, elements);

        /// <inheritdoc cref="IPdfOverlay.Apply(byte[], IEnumerable{PdfOverlayElement})"/>
        public static byte[] Apply(byte[] pdfBytes, IEnumerable<PdfOverlayElement> elements)
            => Instance.Apply(pdfBytes, elements);
    }
}
