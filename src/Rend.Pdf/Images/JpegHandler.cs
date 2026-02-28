using System;
using System.Collections.Generic;
using System.IO.Compression;
using Rend.Pdf.Internal;

namespace Rend.Pdf.Images
{
    /// <summary>
    /// JPEG image handler. Embeds JPEG data as-is (passthrough, no re-encoding)
    /// with DCTDecode filter. Parses only the JPEG header for dimensions and color space.
    /// </summary>
    internal static class JpegHandler
    {
        public static PdfImage CreateImage(byte[] jpegData, string resourceName, PdfObjectTable objectTable,
                                            CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            // Parse JPEG header for dimensions, color space, and ICC profile
            ParseJpegHeader(jpegData, out int width, out int height, out int components, out byte[]? iccProfile);

            PdfObject colorSpaceObj;
            if (iccProfile != null)
            {
                // Build ICCBased color space
                int iccComponents = components;
                var iccStream = new PdfStream(iccProfile, compress: true) { CompressionLevel = compressionLevel };
                iccStream.Dict[PdfName.N] = new PdfInteger(iccComponents);
                PdfName alternate;
                switch (iccComponents)
                {
                    case 1: alternate = PdfName.DeviceGray; break;
                    case 4: alternate = PdfName.DeviceCMYK; break;
                    default: alternate = PdfName.DeviceRGB; break;
                }
                iccStream.Dict[PdfName.Alternate] = alternate;
                var iccRef = objectTable.Allocate(iccStream);
                var csArray = new PdfArray(2);
                csArray.Add(PdfName.ICCBased);
                csArray.Add(iccRef);
                colorSpaceObj = csArray;
            }
            else
            {
                switch (components)
                {
                    case 1: colorSpaceObj = PdfName.DeviceGray; break;
                    case 4: colorSpaceObj = PdfName.DeviceCMYK; break;
                    default: colorSpaceObj = PdfName.DeviceRGB; break;
                }
            }

            // Create a non-compressed stream (JPEG data IS the stream data, already compressed)
            var stream = new PdfStream(jpegData, compress: false) { CompressionLevel = compressionLevel };
            stream.Dict[PdfName.Type] = PdfName.XObject;
            stream.Dict[PdfName.Subtype] = PdfName.Image;
            stream.Dict[PdfName.Width] = new PdfInteger(width);
            stream.Dict[PdfName.Height] = new PdfInteger(height);
            stream.Dict[PdfName.BitsPerComponent] = new PdfInteger(8);
            stream.Dict[PdfName.ColorSpace] = colorSpaceObj;
            stream.Dict[PdfName.Filter] = PdfName.DCTDecode;

            var imageRef = objectTable.Allocate(stream);

            return new PdfImage(width, height, 8, false, ImageFormat.Jpeg,
                                resourceName, imageRef, null);
        }

        private static void ParseJpegHeader(byte[] data, out int width, out int height,
                                             out int components, out byte[]? iccProfile)
        {
            width = 0;
            height = 0;
            components = 3; // default RGB
            iccProfile = null;

            if (data.Length < 2 || data[0] != 0xFF || data[1] != 0xD8)
                throw new InvalidOperationException("Not a valid JPEG file.");

            // ICC profile chunks (APP2 markers may be split across multiple segments)
            List<(int seqNo, byte[] data)>? iccChunks = null;
            int iccTotalChunks = 0;

            int pos = 2;
            while (pos < data.Length - 1)
            {
                if (data[pos] != 0xFF)
                {
                    pos++;
                    continue;
                }

                byte marker = data[pos + 1];
                pos += 2;

                // SOF markers
                if ((marker >= 0xC0 && marker <= 0xC3) || (marker >= 0xC5 && marker <= 0xC7) ||
                    (marker >= 0xC9 && marker <= 0xCB) || (marker >= 0xCD && marker <= 0xCF))
                {
                    if (pos + 7 < data.Length)
                    {
                        height = (data[pos + 3] << 8) | data[pos + 4];
                        width = (data[pos + 5] << 8) | data[pos + 6];
                        components = data[pos + 7];
                    }
                    // Don't return yet — continue scanning for ICC chunks
                    if (pos + 1 < data.Length)
                    {
                        int segLen = (data[pos] << 8) | data[pos + 1];
                        pos += segLen;
                    }
                    else break;
                    continue;
                }

                // APP2 marker (0xE2) — may contain ICC_PROFILE
                if (marker == 0xE2 && pos + 1 < data.Length)
                {
                    int segmentLength = (data[pos] << 8) | data[pos + 1];
                    // Check for "ICC_PROFILE\0" identifier (12 bytes) after segment length
                    if (segmentLength >= 16 && pos + 15 < data.Length &&
                        data[pos + 2] == 'I' && data[pos + 3] == 'C' && data[pos + 4] == 'C' &&
                        data[pos + 5] == '_' && data[pos + 6] == 'P' && data[pos + 7] == 'R' &&
                        data[pos + 8] == 'O' && data[pos + 9] == 'F' && data[pos + 10] == 'I' &&
                        data[pos + 11] == 'L' && data[pos + 12] == 'E' && data[pos + 13] == 0)
                    {
                        int seqNo = data[pos + 14]; // 1-based
                        int totalChunks = data[pos + 15];
                        iccTotalChunks = totalChunks;
                        int iccDataStart = pos + 16;
                        int iccDataLen = segmentLength - 16;
                        if (iccDataLen > 0 && iccDataStart + iccDataLen <= data.Length)
                        {
                            if (iccChunks == null) iccChunks = new List<(int, byte[])>();
                            var chunk = new byte[iccDataLen];
                            Buffer.BlockCopy(data, iccDataStart, chunk, 0, iccDataLen);
                            iccChunks.Add((seqNo, chunk));
                        }
                    }
                    pos += segmentLength;
                    continue;
                }

                // SOS marker — start of scan, stop parsing
                if (marker == 0xDA)
                    break;

                // Skip other markers
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

            // Assemble ICC profile from collected chunks
            if (iccChunks != null && iccChunks.Count == iccTotalChunks)
            {
                iccChunks.Sort((a, b) => a.seqNo.CompareTo(b.seqNo));
                int totalLen = 0;
                foreach (var c in iccChunks) totalLen += c.data.Length;
                iccProfile = new byte[totalLen];
                int offset = 0;
                foreach (var c in iccChunks)
                {
                    Buffer.BlockCopy(c.data, 0, iccProfile, offset, c.data.Length);
                    offset += c.data.Length;
                }
            }
        }
    }
}
