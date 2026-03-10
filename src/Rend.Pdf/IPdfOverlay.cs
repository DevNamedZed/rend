using System.Collections.Generic;
using System.IO;

namespace Rend.Pdf
{
    /// <summary>
    /// Draws text and images onto existing PDF documents.
    /// </summary>
    public interface IPdfOverlay
    {
        void Apply(Stream input, Stream output, IEnumerable<PdfOverlayElement> elements);
        byte[] Apply(byte[] pdfBytes, IEnumerable<PdfOverlayElement> elements);
    }
}
