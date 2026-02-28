namespace Rend.Output.Pdf
{
    /// <summary>
    /// Configuration options for PDF rendering output.
    /// </summary>
    public sealed class PdfRenderOptions
    {
        /// <summary>Gets or sets whether to generate bookmarks from heading elements.</summary>
        public bool GenerateBookmarks { get; set; } = true;

        /// <summary>Gets or sets whether to generate clickable link annotations.</summary>
        public bool GenerateLinks { get; set; } = true;

        /// <summary>Gets or sets the document title metadata.</summary>
        public string? Title { get; set; }

        /// <summary>Gets or sets the document author metadata.</summary>
        public string? Author { get; set; }

        /// <summary>Gets or sets the underlying PDF document options.</summary>
        public Rend.Pdf.PdfDocumentOptions? DocumentOptions { get; set; }
    }
}
