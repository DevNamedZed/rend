using Rend.Pdf;

namespace Rend.Output.Pdf.Internal
{
    /// <summary>
    /// Placeholder for building the PDF document outline (bookmarks) tree
    /// from heading elements (h1-h6) encountered during rendering.
    /// </summary>
    internal static class PdfBookmarkBuilder
    {
        /// <summary>
        /// Adds a bookmark entry to the PDF document outline.
        /// </summary>
        /// <param name="doc">The PDF document to add the outline entry to.</param>
        /// <param name="title">The bookmark title text.</param>
        /// <param name="page">The target page for the bookmark.</param>
        /// <param name="yPosition">The Y position on the page (in PDF coordinates).</param>
        /// <returns>The created outline node, or null if the document is null.</returns>
        internal static PdfOutlineNode? AddBookmark(PdfDocument? doc, string title, PdfPage page, float yPosition)
        {
            if (doc == null)
            {
                return null;
            }

            return doc.AddOutline(title, page, yPosition);
        }
    }
}
