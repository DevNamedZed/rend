using System;

namespace Rend.Pdf
{
    /// <summary>
    /// Document-level metadata written to the PDF /Info dictionary.
    /// </summary>
    public sealed class PdfDocumentInfo
    {
        /// <summary>Document title.</summary>
        public string? Title { get; set; }

        /// <summary>Author name.</summary>
        public string? Author { get; set; }

        /// <summary>Subject description.</summary>
        public string? Subject { get; set; }

        /// <summary>Keywords (comma-separated).</summary>
        public string? Keywords { get; set; }

        /// <summary>Application that created the original content.</summary>
        public string? Creator { get; set; }

        /// <summary>Application that produced the PDF. Defaults to "Rend.Pdf".</summary>
        public string Producer { get; set; } = "Rend.Pdf";

        /// <summary>Document creation date.</summary>
        public DateTime? CreationDate { get; set; }

        /// <summary>Last modification date.</summary>
        public DateTime? ModDate { get; set; }

        /// <summary>
        /// Format a DateTime as a PDF date string: D:YYYYMMDDHHmmSSOHH'mm
        /// </summary>
        internal static string FormatPdfDate(DateTime dt)
        {
            var utc = dt.ToUniversalTime();
            return $"D:{utc:yyyyMMddHHmmss}Z";
        }
    }
}
