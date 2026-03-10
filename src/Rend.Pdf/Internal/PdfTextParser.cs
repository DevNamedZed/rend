using System;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// Lightweight parser for extracting structural information from PDF text.
    /// Operates on the ISO-8859-1 string representation of PDF bytes.
    /// </summary>
    internal static class PdfTextParser
    {
        public static int FindCatalogObjectNumber(string pdfText)
        {
            int idx = pdfText.LastIndexOf("/Root ", StringComparison.Ordinal);
            if (idx < 0) return -1;
            return ParseIntAt(pdfText, idx + 6);
        }

        public static int FindTrailerSize(string pdfText)
        {
            int idx = pdfText.LastIndexOf("/Size ", StringComparison.Ordinal);
            if (idx < 0) return -1;
            return ParseIntAt(pdfText, idx + 6);
        }

        public static bool HasAcroFormInCatalog(string pdfText, int catalogObjNum)
        {
            return ExtractObjectContent(pdfText, catalogObjNum).Contains("/AcroForm");
        }

        public static int FindFirstPageObjectNumber(string pdfText)
        {
            int searchFrom = 0;
            while (true)
            {
                int idx = pdfText.IndexOf("/Type /Page", searchFrom, StringComparison.Ordinal);
                if (idx < 0) return -1;

                int afterPage = idx + 11;
                if (afterPage < pdfText.Length && pdfText[afterPage] == 's')
                {
                    searchFrom = afterPage;
                    continue;
                }

                int objStart = pdfText.LastIndexOf(" obj", idx, StringComparison.Ordinal);
                if (objStart < 0)
                {
                    searchFrom = afterPage;
                    continue;
                }

                int lineStart = pdfText.LastIndexOf('\n', objStart);
                if (lineStart < 0) lineStart = 0;
                else lineStart++;

                return ParseIntAt(pdfText, lineStart);
            }
        }

        public static long FindStartXrefOffset(string pdfText)
        {
            int idx = pdfText.LastIndexOf("startxref", StringComparison.Ordinal);
            if (idx < 0) return 0;
            idx += 9;
            while (idx < pdfText.Length && (pdfText[idx] == '\n' || pdfText[idx] == '\r' || pdfText[idx] == ' '))
                idx++;
            return ParseLongAt(pdfText, idx);
        }

        public static string ExtractObjectContent(string pdfText, int objNum)
        {
            string marker = objNum + " 0 obj";
            int idx = pdfText.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0) return "";

            int contentStart = idx + marker.Length;
            while (contentStart < pdfText.Length &&
                   (pdfText[contentStart] == '\n' || pdfText[contentStart] == '\r' || pdfText[contentStart] == ' '))
                contentStart++;

            int endObj = pdfText.IndexOf("endobj", contentStart, StringComparison.Ordinal);
            if (endObj < 0) return "";

            return pdfText.Substring(contentStart, endObj - contentStart).Trim();
        }

        public static string RemoveDictEntry(string dictContent, string entryName)
        {
            int idx = dictContent.IndexOf(entryName, StringComparison.Ordinal);
            if (idx < 0) return dictContent;

            int entryEnd = idx + entryName.Length;
            int nextNewline = dictContent.IndexOf('\n', entryEnd);
            if (nextNewline < 0) nextNewline = dictContent.Length;

            return dictContent.Substring(0, idx) + dictContent.Substring(nextNewline);
        }

        public static int FindContentsHexStart(string pdfText, string contentsPlaceholder)
        {
            int idx = pdfText.IndexOf("<" + contentsPlaceholder + ">", StringComparison.Ordinal);
            if (idx < 0) return -1;
            return idx + 1;
        }

        public static int ParseIntAt(string text, int startIdx)
        {
            while (startIdx < text.Length && (text[startIdx] == ' ' || text[startIdx] == '\n' || text[startIdx] == '\r'))
                startIdx++;

            bool negative = false;
            if (startIdx < text.Length && text[startIdx] == '-')
            {
                negative = true;
                startIdx++;
            }

            int value = 0;
            while (startIdx < text.Length && text[startIdx] >= '0' && text[startIdx] <= '9')
            {
                value = value * 10 + (text[startIdx] - '0');
                startIdx++;
            }
            return negative ? -value : value;
        }

        public static long ParseLongAt(string text, int startIdx)
        {
            while (startIdx < text.Length && (text[startIdx] == ' ' || text[startIdx] == '\n' || text[startIdx] == '\r'))
                startIdx++;

            long value = 0;
            while (startIdx < text.Length && text[startIdx] >= '0' && text[startIdx] <= '9')
            {
                value = value * 10 + (text[startIdx] - '0');
                startIdx++;
            }
            return value;
        }
    }
}
