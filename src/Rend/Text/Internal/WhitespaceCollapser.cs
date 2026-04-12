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
        /// Collapses any run of collapsible whitespace characters to a single
        /// space. Matches Chrome/Blink behavior for <c>white-space: normal</c>
        /// and <c>nowrap</c>: segment breaks (newlines) are always converted to
        /// a space, regardless of adjacent character properties.
        /// </summary>
        /// <spec>CSS-TEXT-3 §4.1.1 https://drafts.csswg.org/css-text-3/#line-break-transform</spec>
        /// <remarks>
        /// The CSS Text 3 spec prescribes removing segment breaks when both
        /// adjacent non-whitespace characters are East Asian Fullwidth, Wide, or
        /// Halfwidth (and neither is Hangul). Chrome does not implement that
        /// rule — it always converts segment breaks to spaces — and the WPT
        /// reftests therefore fail in Chrome as well. Our goal is Chrome parity,
        /// so we match the implemented behavior rather than the spec text.
        /// </remarks>
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
