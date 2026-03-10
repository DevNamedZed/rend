using System.Collections.Generic;
using System.IO;

namespace Rend.Pdf
{
    /// <summary>
    /// Static convenience facade over <see cref="PdfOverlay"/>.
    /// For DI or testing, use <see cref="IPdfOverlay"/> and <see cref="PdfOverlay"/> directly.
    /// </summary>
    public static class PdfOverlays
    {
        private static readonly PdfOverlay Instance = new PdfOverlay();

        public static void Apply(Stream input, Stream output, IEnumerable<PdfOverlayElement> elements)
            => Instance.Apply(input, output, elements);

        public static byte[] Apply(byte[] pdfBytes, IEnumerable<PdfOverlayElement> elements)
            => Instance.Apply(pdfBytes, elements);
    }
}
