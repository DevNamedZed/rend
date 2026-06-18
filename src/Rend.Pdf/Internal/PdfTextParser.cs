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

            // Require a token boundary before the object number so e.g. object 5's "5 0 obj" does
            // not match inside "115 0 obj". A real object header is at the start or preceded by a
            // non-digit (whitespace/newline).
            int idx = -1;
            int searchFrom = 0;
            while (true)
            {
                int candidate = pdfText.IndexOf(marker, searchFrom, StringComparison.Ordinal);
                if (candidate < 0) return "";
                if (candidate == 0 || !char.IsDigit(pdfText[candidate - 1]))
                {
                    idx = candidate;
                    break;
                }
                searchFrom = candidate + 1;
            }

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

        /// <summary>
        /// Finds an inline dictionary entry — <c>/Key &lt;&lt; ... &gt;&gt;</c> — in dictionary
        /// content and returns its inner text (between the delimiters) plus the span covering the
        /// whole <c>/Key &lt;&lt; ... &gt;&gt;</c> match. Matches the key only at a token boundary so
        /// <c>/Font</c> does not match <c>/FontMatrix</c>. Returns false if the key is absent or its
        /// value is not an inline dictionary (e.g. an indirect reference).
        /// </summary>
        public static bool TryGetInlineDictEntry(string dictContent, string key,
            out string innerContent, out int matchStart, out int matchEnd)
        {
            innerContent = "";
            matchStart = -1;
            matchEnd = -1;

            int searchFrom = 0;
            while (true)
            {
                int idx = dictContent.IndexOf(key, searchFrom, StringComparison.Ordinal);
                if (idx < 0)
                {
                    return false;
                }

                int after = idx + key.Length;
                searchFrom = after;

                // Reject a partial match like /FontMatrix when looking for /Font.
                if (after < dictContent.Length)
                {
                    char next = dictContent[after];
                    if (char.IsLetterOrDigit(next))
                    {
                        continue;
                    }
                }

                int cursor = after;
                while (cursor < dictContent.Length && char.IsWhiteSpace(dictContent[cursor]))
                {
                    cursor++;
                }

                if (cursor + 1 >= dictContent.Length || dictContent[cursor] != '<' || dictContent[cursor + 1] != '<')
                {
                    continue; // value is not an inline dictionary
                }

                int depth = 0;
                int scan = cursor;
                while (scan + 1 < dictContent.Length)
                {
                    char ch = dictContent[scan];
                    if (ch == '(')
                    {
                        // A PDF string literal may legally contain `>>`; skip it so it can't be
                        // mistaken for the dictionary's closing delimiter. (Hex strings `<...>`
                        // can't contain `>>`, so they need no special handling.)
                        scan = SkipStringLiteral(dictContent, scan);
                    }
                    else if (ch == '<' && dictContent[scan + 1] == '<')
                    {
                        depth++;
                        scan += 2;
                    }
                    else if (ch == '>' && dictContent[scan + 1] == '>')
                    {
                        depth--;
                        scan += 2;
                        if (depth == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        scan++;
                    }
                }

                if (depth != 0)
                {
                    return false; // unbalanced
                }

                innerContent = dictContent.Substring(cursor + 2, (scan - 2) - (cursor + 2));
                matchStart = idx;
                matchEnd = scan;
                return true;
            }
        }

        // Given the index of a '(' that opens a PDF string literal, returns the index just past the
        // matching ')'. Handles backslash escapes (incl. \( \) \\) and balanced nested parentheses.
        private static int SkipStringLiteral(string s, int openIndex)
        {
            int depth = 0;
            int i = openIndex;
            while (i < s.Length)
            {
                char c = s[i];
                if (c == '\\')
                {
                    i += 2; // skip the escaped character
                    continue;
                }
                if (c == '(')
                {
                    depth++;
                }
                else if (c == ')')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return i + 1;
                    }
                }
                i++;
            }
            return i;
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
