using Rend.Pdf.Internal;

namespace Rend.Pdf
{
    /// <summary>
    /// An Optional Content Group (OCG) — a named layer in the PDF that can be toggled
    /// visible/hidden in a PDF viewer. Also known as a "layer".
    /// Create using <see cref="PdfDocument.AddLayer"/>.
    /// </summary>
    public sealed class PdfOptionalContentGroup
    {
        /// <summary>The display name of this layer.</summary>
        public string Name { get; }

        /// <summary>Whether this layer is visible by default when the PDF is opened.</summary>
        public bool DefaultVisible { get; set; } = true;

        /// <summary>Reference to the OCG dictionary object.</summary>
        internal PdfReference? ObjectReference { get; set; }

        /// <summary>Resource name used in content stream BDC operators (e.g. "OC1").</summary>
        internal string ResourceName { get; set; } = "";

        internal PdfOptionalContentGroup(string name)
        {
            Name = name;
        }
    }
}
