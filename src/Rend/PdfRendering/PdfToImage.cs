#nullable enable
using System;
using System.IO;
using Rend.Pdf.Reading;
using Rend.PdfRendering;
using SkiaSharp;

namespace Rend
{
    /// <summary>
    /// Provides static methods for rendering PDF pages to PNG images.
    /// </summary>
    public static class PdfToImage
    {
        /// <summary>Render a single page of a PDF to a PNG image.</summary>
        /// <param name="pdfData">The raw PDF file bytes.</param>
        /// <param name="pageIndex">Zero-based page index.</param>
        /// <param name="dpi">Output resolution in dots per inch (default 150).</param>
        /// <returns>PNG image bytes.</returns>
        public static byte[] RenderPage(byte[] pdfData, int pageIndex = 0, float dpi = 150f)
        {
            if (pdfData == null) throw new ArgumentNullException(nameof(pdfData));
            if (dpi <= 0) throw new ArgumentOutOfRangeException(nameof(dpi), "DPI must be positive.");

            using var reader = PdfDocumentReader.Open(pdfData);
            ValidatePageIndex(reader, pageIndex);

            float scale = dpi / 72f;
            var renderer = new PdfPageRenderer(reader);
            using var bitmap = renderer.RenderPage(pageIndex, scale);
            return EncodePng(bitmap);
        }

        /// <summary>Render a single page to PNG from a file path.</summary>
        /// <param name="filePath">Path to the PDF file.</param>
        /// <param name="pageIndex">Zero-based page index.</param>
        /// <param name="dpi">Output resolution in dots per inch (default 150).</param>
        /// <returns>PNG image bytes.</returns>
        public static byte[] RenderPage(string filePath, int pageIndex = 0, float dpi = 150f)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            byte[] data = File.ReadAllBytes(filePath);
            return RenderPage(data, pageIndex, dpi);
        }

        /// <summary>Render a single page to PNG from a stream.</summary>
        /// <param name="pdfStream">A readable stream containing PDF data.</param>
        /// <param name="pageIndex">Zero-based page index.</param>
        /// <param name="dpi">Output resolution in dots per inch (default 150).</param>
        /// <returns>PNG image bytes.</returns>
        public static byte[] RenderPage(Stream pdfStream, int pageIndex = 0, float dpi = 150f)
        {
            if (pdfStream == null) throw new ArgumentNullException(nameof(pdfStream));
            byte[] data = ReadStreamFully(pdfStream);
            return RenderPage(data, pageIndex, dpi);
        }

        /// <summary>Render all pages to PNG images.</summary>
        /// <param name="pdfData">The raw PDF file bytes.</param>
        /// <param name="dpi">Output resolution in dots per inch (default 150).</param>
        /// <returns>Array of PNG image byte arrays, one per page.</returns>
        public static byte[][] RenderAllPages(byte[] pdfData, float dpi = 150f)
        {
            if (pdfData == null) throw new ArgumentNullException(nameof(pdfData));
            if (dpi <= 0) throw new ArgumentOutOfRangeException(nameof(dpi), "DPI must be positive.");

            using var reader = PdfDocumentReader.Open(pdfData);
            int count = reader.PageCount;
            float scale = dpi / 72f;
            var renderer = new PdfPageRenderer(reader);
            var results = new byte[count][];

            for (int i = 0; i < count; i++)
            {
                using var bitmap = renderer.RenderPage(i, scale);
                results[i] = EncodePng(bitmap);
            }

            return results;
        }

        /// <summary>Get the number of pages in a PDF.</summary>
        /// <param name="pdfData">The raw PDF file bytes.</param>
        /// <returns>The page count.</returns>
        public static int GetPageCount(byte[] pdfData)
        {
            if (pdfData == null) throw new ArgumentNullException(nameof(pdfData));
            using var reader = PdfDocumentReader.Open(pdfData);
            return reader.PageCount;
        }

        /// <summary>Render a single page to an SKBitmap (caller must dispose).</summary>
        /// <param name="pdfData">The raw PDF file bytes.</param>
        /// <param name="pageIndex">Zero-based page index.</param>
        /// <param name="dpi">Output resolution in dots per inch (default 150).</param>
        /// <returns>An SKBitmap that the caller is responsible for disposing.</returns>
        public static SKBitmap RenderPageToBitmap(byte[] pdfData, int pageIndex = 0, float dpi = 150f)
        {
            if (pdfData == null) throw new ArgumentNullException(nameof(pdfData));
            if (dpi <= 0) throw new ArgumentOutOfRangeException(nameof(dpi), "DPI must be positive.");

            using var reader = PdfDocumentReader.Open(pdfData);
            ValidatePageIndex(reader, pageIndex);

            float scale = dpi / 72f;
            var renderer = new PdfPageRenderer(reader);
            return renderer.RenderPage(pageIndex, scale);
        }

        /// <summary>Get page dimensions in points (1/72 inch).</summary>
        /// <param name="pdfData">The raw PDF file bytes.</param>
        /// <param name="pageIndex">Zero-based page index.</param>
        /// <returns>A tuple of (Width, Height) in PDF points.</returns>
        public static (float Width, float Height) GetPageSize(byte[] pdfData, int pageIndex = 0)
        {
            if (pdfData == null) throw new ArgumentNullException(nameof(pdfData));

            using var reader = PdfDocumentReader.Open(pdfData);
            ValidatePageIndex(reader, pageIndex);

            var pageDict = reader.Resolve(reader.GetPage(pageIndex));

            // Try CropBox first, then MediaBox
            var box = reader.Resolve(pageDict["CropBox"]);
            if (box.IsNull)
                box = reader.Resolve(pageDict["MediaBox"]);
            if (box.IsNull)
            {
                // Walk up the page tree
                var parent = reader.Resolve(pageDict["Parent"]);
                while (!parent.IsNull)
                {
                    box = reader.Resolve(parent["CropBox"]);
                    if (box.IsNull) box = reader.Resolve(parent["MediaBox"]);
                    if (!box.IsNull) break;
                    parent = reader.Resolve(parent["Parent"]);
                }
            }

            if (box.IsNull)
                return (612f, 792f); // Default US Letter

            float left = box[0].AsFloat();
            float bottom = box[1].AsFloat();
            float right = box[2].AsFloat();
            float top = box[3].AsFloat();

            return (right - left, top - bottom);
        }

        /// <summary>Extract text content from a specific PDF page.</summary>
        /// <param name="pdfData">The raw PDF file bytes.</param>
        /// <param name="pageIndex">Zero-based page index.</param>
        /// <returns>Extracted text content.</returns>
        public static string ExtractText(byte[] pdfData, int pageIndex = 0)
        {
            if (pdfData == null) throw new ArgumentNullException(nameof(pdfData));

            using var reader = PdfDocumentReader.Open(pdfData);
            ValidatePageIndex(reader, pageIndex);

            var extractor = new PdfTextExtractor(reader);
            return extractor.ExtractText(pageIndex);
        }

        /// <summary>Extract text content from all pages of a PDF.</summary>
        /// <param name="pdfData">The raw PDF file bytes.</param>
        /// <returns>Array of text strings, one per page.</returns>
        public static string[] ExtractAllText(byte[] pdfData)
        {
            if (pdfData == null) throw new ArgumentNullException(nameof(pdfData));

            using var reader = PdfDocumentReader.Open(pdfData);
            int count = reader.PageCount;
            var extractor = new PdfTextExtractor(reader);
            var results = new string[count];

            for (int i = 0; i < count; i++)
                results[i] = extractor.ExtractText(i);

            return results;
        }

        private static void ValidatePageIndex(PdfDocumentReader reader, int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= reader.PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndex),
                    $"Page index {pageIndex} is out of range. The document has {reader.PageCount} page(s).");
        }

        private static byte[] EncodePng(SKBitmap bitmap)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private static byte[] ReadStreamFully(Stream stream)
        {
            if (stream is MemoryStream ms && ms.TryGetBuffer(out var buffer))
            {
                // Fast path for MemoryStream
                byte[] result = new byte[buffer.Count];
                Array.Copy(buffer.Array!, buffer.Offset, result, 0, buffer.Count);
                return result;
            }

            using var output = new MemoryStream();
            byte[] buf = new byte[8192];
            int read;
            while ((read = stream.Read(buf, 0, buf.Length)) > 0)
                output.Write(buf, 0, read);
            return output.ToArray();
        }
    }
}
