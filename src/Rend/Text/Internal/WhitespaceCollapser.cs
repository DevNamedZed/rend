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
        /// space, applying the segment-break transformation rules used by
        /// Chrome/Blink for <c>white-space: normal</c> and <c>nowrap</c>.
        /// </summary>
        /// <spec>CSS-TEXT-3 §4.1.1 https://drafts.csswg.org/css-text-3/#line-break-transform</spec>
        /// <remarks>
        /// The CSS Text 3 spec defines three ordered rules for each collapsible
        /// segment break:
        /// <list type="number">
        /// <item>If the character immediately before or after the segment break
        /// is U+200B ZERO WIDTH SPACE, the break is removed.</item>
        /// <item>Otherwise, if both neighbors are East Asian Wide/Fullwidth/
        /// Halfwidth (not Ambiguous) and neither is Hangul, the break is
        /// removed.</item>
        /// <item>Otherwise, the break is converted to a single space.</item>
        /// </list>
        /// Chrome implements rule 1 but skips rule 2 — so the WPT reftests that
        /// assert the EAW-only case fail in Chrome as well. We match Chrome:
        /// rules 1 and 3 are honored, rule 2 is intentionally not applied.
        /// </remarks>
        private static string CollapseAll(string text)
        {
            var sb = new StringBuilder(text.Length);
            bool lastWasSpace = false;

            int index = 0;
            while (index < text.Length)
            {
                char ch = text[index];

                if (!IsCollapsibleWhitespace(ch))
                {
                    sb.Append(ch);
                    lastWasSpace = false;
                    index++;
                    continue;
                }

                bool runContainsSegmentBreak = false;
                while (index < text.Length && IsCollapsibleWhitespace(text[index]))
                {
                    if (text[index] == '\n' || text[index] == '\r')
                    {
                        runContainsSegmentBreak = true;
                    }
                    index++;
                }

                bool removeForZwspRule = runContainsSegmentBreak
                    && IsZeroWidthSpaceAdjacent(sb, text, index);

                if (removeForZwspRule)
                {
                    continue;
                }

                if (!lastWasSpace)
                {
                    sb.Append(' ');
                    lastWasSpace = true;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns true when a collapsible-whitespace run is immediately
        /// preceded or followed by U+200B ZERO WIDTH SPACE. Spaces and tabs
        /// adjacent to the segment break are treated as part of the run and
        /// never as the "immediate" neighbor.
        /// </summary>
        private static bool IsZeroWidthSpaceAdjacent(StringBuilder before, string text, int afterIndex)
        {
            if (before.Length > 0 && before[before.Length - 1] == ZeroWidthSpace)
            {
                return true;
            }

            if (afterIndex < text.Length && text[afterIndex] == ZeroWidthSpace)
            {
                return true;
            }

            return false;
        }

        private const char ZeroWidthSpace = '\u200B';

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
