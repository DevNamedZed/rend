#nullable enable
using Rend.Pdf.Internal;

namespace Rend.Pdf.Images
{
    /// <summary>
    /// Builds a CCITT Group 4 (1bpp bilevel) image XObject from already-encoded fax data.
    /// The data is stored uncompressed with an explicit <c>/Filter /CCITTFaxDecode</c>.
    /// </summary>
    internal static class CcittHandler
    {
        public static PdfImage CreateImage(byte[] g4Data, int width, int height, bool blackIs1,
                                           string resourceName, PdfObjectTable objectTable)
        {
            // CCITT data is already compressed — store it raw and declare the filter explicitly.
            var stream = new PdfStream(g4Data, compress: false);
            stream.Dict[PdfName.Type] = PdfName.XObject;
            stream.Dict[PdfName.Subtype] = PdfName.Image;
            stream.Dict[PdfName.Width] = new PdfInteger(width);
            stream.Dict[PdfName.Height] = new PdfInteger(height);
            stream.Dict[PdfName.BitsPerComponent] = new PdfInteger(1);
            stream.Dict[PdfName.ColorSpace] = PdfName.DeviceGray;
            stream.Dict[PdfName.Filter] = new PdfName("CCITTFaxDecode");

            var decodeParms = new PdfDictionary(4);
            decodeParms[PdfName.K] = new PdfInteger(-1); // pure 2D = Group 4
            decodeParms[PdfName.Columns] = new PdfInteger(width);
            decodeParms[new PdfName("Rows")] = new PdfInteger(height);
            decodeParms[new PdfName("BlackIs1")] = blackIs1 ? PdfBoolean.True : PdfBoolean.False;
            stream.Dict[PdfName.DecodeParms] = decodeParms;

            PdfReference objectReference = objectTable.Allocate(stream);
            return new PdfImage(width, height, 1, false, ImageFormat.Ccitt, resourceName, objectReference, null);
        }
    }
}
