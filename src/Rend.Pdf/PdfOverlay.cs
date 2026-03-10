using System;
using System.Collections.Generic;
using System.IO;

namespace Rend.Pdf
{
    /// <summary>
    /// Default implementation of <see cref="IPdfOverlay"/>.
    /// </summary>
    public sealed class PdfOverlay : IPdfOverlay
    {
        public void Apply(Stream input, Stream output, IEnumerable<PdfOverlayElement> elements)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                pdfBytes = ms.ToArray();
            }

            var result = Internal.PdfOverlayWriter.Apply(pdfBytes, elements);
            output.Write(result, 0, result.Length);
        }

        public byte[] Apply(byte[] pdfBytes, IEnumerable<PdfOverlayElement> elements)
        {
            if (pdfBytes == null || pdfBytes.Length == 0)
                throw new ArgumentException("PDF data must not be null or empty.", nameof(pdfBytes));
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            return Internal.PdfOverlayWriter.Apply(pdfBytes, elements);
        }
    }
}
