namespace Rend.Pdf
{
    /// <summary>
    /// Represents an image resource embedded in a PDF document.
    /// </summary>
    public sealed class PdfImage
    {
        /// <summary>Image width in pixels.</summary>
        public int Width { get; }

        /// <summary>Image height in pixels.</summary>
        public int Height { get; }

        /// <summary>Bits per color component (typically 8).</summary>
        public int BitsPerComponent { get; }

        /// <summary>Whether the image has an alpha channel (stored as separate SMask).</summary>
        public bool HasAlpha { get; }

        /// <summary>The format the image was added as.</summary>
        public ImageFormat Format { get; }

        // Internal: the resource name used in content streams (e.g., "Im1")
        internal string ResourceName { get; }

        // Internal: reference to the PDF image XObject
        internal Internal.PdfReference ObjectReference { get; }

        // Internal: reference to the SMask XObject (if alpha)
        internal Internal.PdfReference? SMaskReference { get; }

        internal PdfImage(int width, int height, int bitsPerComponent, bool hasAlpha,
                          ImageFormat format, string resourceName,
                          Internal.PdfReference objectReference,
                          Internal.PdfReference? sMaskReference)
        {
            Width = width;
            Height = height;
            BitsPerComponent = bitsPerComponent;
            HasAlpha = hasAlpha;
            Format = format;
            ResourceName = resourceName;
            ObjectReference = objectReference;
            SMaskReference = sMaskReference;
        }
    }
}
