#nullable enable
using System;
using System.IO;
using SkiaSharp;

namespace Rend
{
    public static class PdfToImage
    {
        public static byte[] RenderPage(byte[] pdfData, int pageIndex = 0, float dpi = 150f)
        {
            using var reader = new PdfReader(pdfData);
            return reader.RenderPage(pageIndex, dpi);
        }

        public static byte[] RenderPage(string filePath, int pageIndex = 0, float dpi = 150f)
        {
            using var reader = new PdfReader(filePath);
            return reader.RenderPage(pageIndex, dpi);
        }

        public static byte[] RenderPage(Stream pdfStream, int pageIndex = 0, float dpi = 150f)
        {
            using var reader = new PdfReader(pdfStream);
            return reader.RenderPage(pageIndex, dpi);
        }

        public static byte[][] RenderAllPages(byte[] pdfData, float dpi = 150f)
        {
            using var reader = new PdfReader(pdfData);
            return reader.RenderAllPages(dpi);
        }

        public static int GetPageCount(byte[] pdfData)
        {
            if (pdfData == null)
            {
                throw new ArgumentNullException(nameof(pdfData));
            }
            using var reader = new PdfReader(pdfData);
            return reader.PageCount;
        }

        public static SKBitmap RenderPageToBitmap(byte[] pdfData, int pageIndex = 0, float dpi = 150f)
        {
            using var reader = new PdfReader(pdfData);
            return reader.RenderPageToBitmap(pageIndex, dpi);
        }

        public static PdfRendering.PdfPageInfo GetPageInfo(byte[] pdfData, int pageIndex = 0)
        {
            using var reader = new PdfReader(pdfData);
            return reader.GetPageInfo(pageIndex);
        }

        public static string ExtractText(byte[] pdfData, int pageIndex = 0)
        {
            using var reader = new PdfReader(pdfData);
            return reader.ExtractText(pageIndex);
        }

        public static string[] ExtractAllText(byte[] pdfData)
        {
            using var reader = new PdfReader(pdfData);
            return reader.ExtractAllText();
        }
    }
}
