using System;
using Rend.Pdf.Internal;

namespace Rend.Pdf.Images
{
    /// <summary>
    /// JPEG image handler. Embeds JPEG data as-is (passthrough, no re-encoding)
    /// with DCTDecode filter. Parses only the JPEG header for dimensions and color space.
    /// </summary>
    internal static class JpegHandler
    {
        public static PdfImage CreateImage(byte[] jpegData, string resourceName, PdfObjectTable objectTable)
        {
            // Parse JPEG header for dimensions and color space
            ParseJpegHeader(jpegData, out int width, out int height, out int components);

            PdfName colorSpace;
            switch (components)
            {
                case 1: colorSpace = PdfName.DeviceGray; break;
                case 4: colorSpace = PdfName.DeviceCMYK; break;
                default: colorSpace = PdfName.DeviceRGB; break;
            }

            // Create image XObject stream — JPEG data is passthrough (no re-encoding!)
            var streamDict = new PdfDictionary(8);
            streamDict[PdfName.Type] = PdfName.XObject;
            streamDict[PdfName.Subtype] = PdfName.Image;
            streamDict[PdfName.Width] = new PdfInteger(width);
            streamDict[PdfName.Height] = new PdfInteger(height);
            streamDict[PdfName.BitsPerComponent] = new PdfInteger(8);
            streamDict[PdfName.ColorSpace] = colorSpace;
            streamDict[PdfName.Filter] = PdfName.DCTDecode;
            streamDict[PdfName.Length] = new PdfInteger(jpegData.Length);

            // Create a non-compressed stream (JPEG data IS the stream data, already compressed)
            var stream = new PdfStream(jpegData, compress: false);
            // Copy dict entries to the stream's dict
            stream.Dict[PdfName.Type] = PdfName.XObject;
            stream.Dict[PdfName.Subtype] = PdfName.Image;
            stream.Dict[PdfName.Width] = new PdfInteger(width);
            stream.Dict[PdfName.Height] = new PdfInteger(height);
            stream.Dict[PdfName.BitsPerComponent] = new PdfInteger(8);
            stream.Dict[PdfName.ColorSpace] = colorSpace;
            stream.Dict[PdfName.Filter] = PdfName.DCTDecode;

            var imageRef = objectTable.Allocate(stream);

            return new PdfImage(width, height, 8, false, ImageFormat.Jpeg,
                                resourceName, imageRef, null);
        }

        private static void ParseJpegHeader(byte[] data, out int width, out int height, out int components)
        {
            width = 0;
            height = 0;
            components = 3; // default RGB

            // Scan for SOF0 (0xFFC0) or SOF2 (0xFFC2) marker
            int pos = 0;
            if (data.Length < 2 || data[0] != 0xFF || data[1] != 0xD8)
                throw new InvalidOperationException("Not a valid JPEG file.");

            pos = 2;
            while (pos < data.Length - 1)
            {
                if (data[pos] != 0xFF)
                {
                    pos++;
                    continue;
                }

                byte marker = data[pos + 1];
                pos += 2;

                // SOF markers: SOF0 through SOF3, SOF5 through SOF7, SOF9 through SOF11, SOF13 through SOF15
                if ((marker >= 0xC0 && marker <= 0xC3) || (marker >= 0xC5 && marker <= 0xC7) ||
                    (marker >= 0xC9 && marker <= 0xCB) || (marker >= 0xCD && marker <= 0xCF))
                {
                    if (pos + 7 < data.Length)
                    {
                        // Skip length (2 bytes) and precision (1 byte)
                        height = (data[pos + 3] << 8) | data[pos + 4];
                        width = (data[pos + 5] << 8) | data[pos + 6];
                        components = data[pos + 7];
                    }
                    return;
                }

                // Skip this marker's data
                if (pos + 1 < data.Length)
                {
                    int segmentLength = (data[pos] << 8) | data[pos + 1];
                    pos += segmentLength;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
