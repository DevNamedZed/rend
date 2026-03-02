using System;
using Rend.Pdf.Internal;

namespace Rend.Pdf
{
    /// <summary>
    /// Represents a file attached to the PDF document.
    /// Create using <see cref="PdfDocument.AttachFile"/>.
    /// </summary>
    public sealed class PdfFileAttachment
    {
        /// <summary>The file name as displayed in the PDF viewer.</summary>
        public string FileName { get; }

        /// <summary>The file data.</summary>
        public byte[] Data { get; }

        /// <summary>MIME type of the attached file (e.g. "text/plain", "application/pdf").</summary>
        public string MimeType { get; }

        /// <summary>Optional description of the attachment.</summary>
        public string? Description { get; set; }

        /// <summary>Creation date of the file.</summary>
        public DateTime? CreationDate { get; set; }

        /// <summary>Modification date of the file.</summary>
        public DateTime? ModDate { get; set; }

        internal PdfReference? FilespecRef { get; set; }

        internal PdfFileAttachment(string fileName, byte[] data, string mimeType)
        {
            FileName = fileName;
            Data = data;
            MimeType = mimeType;
        }
    }
}
