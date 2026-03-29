using System;
using System.Text;
using Rend.Css;

namespace Rend.Text.Internal
{
    /// <summary>
    /// Collapses and transforms whitespace in text according to CSS white-space property rules.
    /// </summary>
    internal static class WhitespaceCollapser
    {
        /// <summary>
        /// Collapses whitespace in the given text according to the specified CSS white-space mode.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <param name="whiteSpace">The CSS white-space mode.</param>
        /// <returns>The text with whitespace collapsed as appropriate.</returns>
        public static string Collapse(string text, CssWhiteSpace whiteSpace)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            if (text.Length == 0)
            {
                return text;
            }

            switch (whiteSpace)
            {
                case CssWhiteSpace.Pre:
                case CssWhiteSpace.PreWrap:
                case CssWhiteSpace.BreakSpaces:
                    // Preserve all whitespace as-is.
                    return text;

                case CssWhiteSpace.PreLine:
                    return CollapseSpacesPreserveNewlines(text);

                case CssWhiteSpace.Normal:
                case CssWhiteSpace.Nowrap:
                default:
                    return CollapseAll(text);
            }
        }

        /// <summary>
        /// Collapses consecutive whitespace characters to a single space.
        /// [CSS-TEXT-3 §4.1.1] Segment breaks (newlines) between two East Asian
        /// Fullwidth/Wide/Halfwidth characters are removed entirely instead of
        /// collapsing to a space. All other whitespace collapses normally.
        /// Used for white-space: normal and nowrap.
        /// </summary>
        private static string CollapseAll(string text)
        {
            var sb = new StringBuilder(text.Length);
            bool lastWasSpace = false;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];

                if (IsCollapsibleWhitespace(ch))
                {
                    if (!lastWasSpace)
                    {
                        // [CSS-TEXT-3 §4.1.1] Check if this whitespace run contains
                        // a segment break (newline) between two East Asian FWH characters.
                        if (ContainsSegmentBreak(text, i) && ShouldRemoveSegmentBreak(text, sb, i))
                        {
                            // Skip the entire whitespace run — segment break removed.
                            while (i + 1 < text.Length && IsCollapsibleWhitespace(text[i + 1]))
                            {
                                i++;
                            }
                            lastWasSpace = false;
                        }
                        else
                        {
                            sb.Append(' ');
                            lastWasSpace = true;
                        }
                    }
                }
                else
                {
                    sb.Append(ch);
                    lastWasSpace = false;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns true if the whitespace run starting at position i contains
        /// a newline (segment break).
        /// </summary>
        private static bool ContainsSegmentBreak(string text, int start)
        {
            for (int i = start; i < text.Length && IsCollapsibleWhitespace(text[i]); i++)
            {
                if (text[i] == '\n' || text[i] == '\r')
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// [CSS-TEXT-3 §4.1.1] Returns true if the segment break should be removed
        /// (both adjacent non-whitespace characters are East Asian FWH).
        /// </summary>
        private static bool ShouldRemoveSegmentBreak(string text, StringBuilder sb, int wsStart)
        {
            // Find the last non-whitespace character before this run (from StringBuilder).
            int charBefore = GetLastCodePoint(sb);
            if (charBefore < 0)
            {
                return false;
            }

            // Find the first non-whitespace character after this run.
            int afterIdx = wsStart;
            while (afterIdx < text.Length && IsCollapsibleWhitespace(text[afterIdx]))
            {
                afterIdx++;
            }
            if (afterIdx >= text.Length)
            {
                return false;
            }
            int charAfter = GetCodePointAt(text, afterIdx);

            // [CSS-TEXT-3 §4.1.1] If both characters are East Asian Fullwidth (F),
            // Wide (W), or Halfwidth (H), remove the segment break.
            return EastAsianWidth.IsEastAsianFWH(charBefore)
                && EastAsianWidth.IsEastAsianFWH(charAfter);
        }

        /// <summary>
        /// Gets the last Unicode code point from the StringBuilder.
        /// Handles surrogate pairs. Returns -1 if empty.
        /// </summary>
        private static int GetLastCodePoint(StringBuilder sb)
        {
            if (sb.Length == 0)
            {
                return -1;
            }
            char last = sb[sb.Length - 1];
            if (char.IsLowSurrogate(last) && sb.Length >= 2 && char.IsHighSurrogate(sb[sb.Length - 2]))
            {
                return char.ConvertToUtf32(sb[sb.Length - 2], last);
            }
            return last;
        }

        /// <summary>
        /// Gets the Unicode code point at the given position in the string.
        /// Handles surrogate pairs.
        /// </summary>
        private static int GetCodePointAt(string text, int index)
        {
            char ch = text[index];
            if (char.IsHighSurrogate(ch) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
            {
                return char.ConvertToUtf32(ch, text[index + 1]);
            }
            return ch;
        }

        /// <summary>
        /// Collapses consecutive spaces and tabs to a single space, but preserves newlines.
        /// Used for white-space: pre-line.
        /// </summary>
        private static string CollapseSpacesPreserveNewlines(string text)
        {
            var sb = new StringBuilder(text.Length);
            bool lastWasSpace = false;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];

                if (ch == '\n')
                {
                    // Remove any trailing collapsed space before the newline.
                    if (lastWasSpace && sb.Length > 0 && sb[sb.Length - 1] == ' ')
                    {
                        sb.Remove(sb.Length - 1, 1);
                    }
                    sb.Append('\n');
                    lastWasSpace = false;
                }
                else if (ch == '\r')
                {
                    // Handle CR and CRLF: emit as LF.
                    if (lastWasSpace && sb.Length > 0 && sb[sb.Length - 1] == ' ')
                    {
                        sb.Remove(sb.Length - 1, 1);
                    }
                    sb.Append('\n');
                    lastWasSpace = false;

                    // Skip LF in CRLF pair.
                    if (i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        i++;
                    }
                }
                else if (ch == ' ' || ch == '\t' || ch == '\u000C')
                {
                    if (!lastWasSpace)
                    {
                        sb.Append(' ');
                        lastWasSpace = true;
                    }
                }
                else
                {
                    sb.Append(ch);
                    lastWasSpace = false;
                }
            }

            return sb.ToString();
        }

        private static bool IsCollapsibleWhitespace(char ch)
        {
            return ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r' || ch == '\f';
        }
    }
}
