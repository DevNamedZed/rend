using Rend.Core.Values;
using Rend.Pdf;

namespace Rend.Output.Pdf.Internal
{
    /// <summary>
    /// Placeholder for collecting hyperlink annotations from rendered content
    /// and converting them to PDF link annotations.
    /// </summary>
    internal static class PdfLinkCollector
    {
        /// <summary>
        /// Adds a URI link annotation to the specified PDF page.
        /// The rectangle is expected to be in PDF coordinates (bottom-left origin).
        /// </summary>
        /// <param name="page">The PDF page to add the annotation to.</param>
        /// <param name="rect">The clickable rectangle in PDF coordinates.</param>
        /// <param name="uri">The URI to link to.</param>
        internal static void AddLink(PdfPage page, RectF rect, System.Uri uri)
        {
            page.AddLink(rect, uri);
        }
    }
}
