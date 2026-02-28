using System.Text;

namespace Rend.Html.Parser.Internal
{
    /// <summary>
    /// Decodes HTML character references: &amp;amp; &amp;#123; &amp;#x1F; and named entities.
    /// </summary>
    internal static class CharRefDecoder
    {
        /// <summary>
        /// Attempts to decode a character reference from the input starting at the given position.
        /// The position should be just after the '&amp;' character.
        /// Returns the decoded string (one or two chars), or null if not a valid reference.
        /// Advances pos past the reference (including trailing ';' if present).
        /// </summary>
        public static string? Decode(string input, ref int pos)
        {
            if (pos >= input.Length) return null;

            char c = input[pos];

            if (c == '#')
            {
                pos++;
                return DecodeNumeric(input, ref pos);
            }

            return DecodeNamed(input, ref pos);
        }

        private static string? DecodeNumeric(string input, ref int pos)
        {
            if (pos >= input.Length) return null;

            bool hex = false;
            if (input[pos] == 'x' || input[pos] == 'X')
            {
                hex = true;
                pos++;
            }

            int start = pos;
            int codePoint = 0;

            if (hex)
            {
                while (pos < input.Length)
                {
                    char c = input[pos];
                    int digit;
                    if (c >= '0' && c <= '9') digit = c - '0';
                    else if (c >= 'a' && c <= 'f') digit = c - 'a' + 10;
                    else if (c >= 'A' && c <= 'F') digit = c - 'A' + 10;
                    else break;

                    codePoint = codePoint * 16 + digit;
                    if (codePoint > 0x10FFFF) codePoint = 0x10FFFF; // Clamp
                    pos++;
                }
            }
            else
            {
                while (pos < input.Length)
                {
                    char c = input[pos];
                    if (c < '0' || c > '9') break;

                    codePoint = codePoint * 10 + (c - '0');
                    if (codePoint > 0x10FFFF) codePoint = 0x10FFFF;
                    pos++;
                }
            }

            // Must have consumed at least one digit
            if (pos == start) return null;

            // Consume optional semicolon
            if (pos < input.Length && input[pos] == ';')
                pos++;

            // Replace null and surrogate code points
            if (codePoint == 0 || (codePoint >= 0xD800 && codePoint <= 0xDFFF))
                codePoint = 0xFFFD;

            // Windows-1252 replacements for 0x80-0x9F range
            if (codePoint >= 0x80 && codePoint <= 0x9F)
                codePoint = ReplaceWindows1252(codePoint);

            return CodePointToString(codePoint);
        }

        private static string? DecodeNamed(string input, ref int pos)
        {
            // Try progressively longer prefixes to find the longest match
            int bestEnd = -1;
            int bestCodePoint = 0;

            int maxLen = input.Length - pos;
            if (maxLen > 32) maxLen = 32; // Named entities are at most ~31 chars

            var sb = new StringBuilder(maxLen);

            for (int i = 0; i < maxLen; i++)
            {
                char c = input[pos + i];
                if (c == ';')
                {
                    sb.Append(c);
                    string name = sb.ToString();
                    // Try with semicolon stripped
                    string nameNoSemi = name.Substring(0, name.Length - 1);
                    if (CharRefTable.TryLookup(nameNoSemi, out int cp))
                    {
                        bestEnd = pos + i + 1;
                        bestCodePoint = cp;
                    }
                    break;
                }

                sb.Append(c);

                // Check for match without semicolon (legacy behavior)
                if (CharRefTable.TryLookup(sb.ToString(), out int cp2))
                {
                    bestEnd = pos + i + 1;
                    bestCodePoint = cp2;
                }

                // Stop if we hit a non-alphanumeric (can't be part of a name)
                if (!char.IsLetterOrDigit(c)) break;
            }

            if (bestEnd < 0) return null;

            pos = bestEnd;
            return CodePointToString(bestCodePoint);
        }

        private static string CodePointToString(int codePoint)
        {
            if (codePoint <= 0xFFFF)
                return new string((char)codePoint, 1);

            // Surrogate pair for supplementary plane
            codePoint -= 0x10000;
            char high = (char)(0xD800 + (codePoint >> 10));
            char low = (char)(0xDC00 + (codePoint & 0x3FF));
            return new string(new[] { high, low });
        }

        /// <summary>
        /// WHATWG spec: replace code points in the 0x80-0x9F range with Windows-1252 mappings.
        /// </summary>
        private static int ReplaceWindows1252(int cp)
        {
            switch (cp)
            {
                case 0x80: return 0x20AC; // Euro sign
                case 0x82: return 0x201A; // Single low-9 quotation mark
                case 0x83: return 0x0192; // Latin small f with hook
                case 0x84: return 0x201E; // Double low-9 quotation mark
                case 0x85: return 0x2026; // Horizontal ellipsis
                case 0x86: return 0x2020; // Dagger
                case 0x87: return 0x2021; // Double dagger
                case 0x88: return 0x02C6; // Modifier letter circumflex accent
                case 0x89: return 0x2030; // Per mille sign
                case 0x8A: return 0x0160; // Latin capital S with caron
                case 0x8B: return 0x2039; // Single left-pointing angle quotation mark
                case 0x8C: return 0x0152; // Latin capital OE
                case 0x8E: return 0x017D; // Latin capital Z with caron
                case 0x91: return 0x2018; // Left single quotation mark
                case 0x92: return 0x2019; // Right single quotation mark
                case 0x93: return 0x201C; // Left double quotation mark
                case 0x94: return 0x201D; // Right double quotation mark
                case 0x95: return 0x2022; // Bullet
                case 0x96: return 0x2013; // En dash
                case 0x97: return 0x2014; // Em dash
                case 0x98: return 0x02DC; // Small tilde
                case 0x99: return 0x2122; // Trade mark sign
                case 0x9A: return 0x0161; // Latin small s with caron
                case 0x9B: return 0x203A; // Single right-pointing angle quotation mark
                case 0x9C: return 0x0153; // Latin small OE
                case 0x9E: return 0x017E; // Latin small z with caron
                case 0x9F: return 0x0178; // Latin capital Y with diaeresis
                default: return cp;
            }
        }
    }
}
