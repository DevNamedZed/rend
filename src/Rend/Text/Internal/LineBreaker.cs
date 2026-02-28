using System;

namespace Rend.Text.Internal
{
    /// <summary>
    /// Represents the type of line break opportunity at a given position.
    /// </summary>
    public enum LineBreakOpportunity : byte
    {
        /// <summary>No break is allowed at this position.</summary>
        Forbidden,
        /// <summary>A break is allowed at this position.</summary>
        Allowed,
        /// <summary>A break is mandatory at this position.</summary>
        Mandatory
    }

    /// <summary>
    /// Finds line break opportunities in text per a simplified UAX #14 algorithm.
    /// </summary>
    public sealed class LineBreaker
    {
        /// <summary>
        /// Finds line break opportunities between each pair of adjacent characters.
        /// Returns an array of length <c>text.Length - 1</c> (or empty for text of length 0 or 1),
        /// where each element indicates the break opportunity between character at index <c>i</c>
        /// and character at index <c>i + 1</c>.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>Array of break opportunities between adjacent characters.</returns>
        public LineBreakOpportunity[] FindBreaks(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            if (text.Length <= 1)
            {
                return Array.Empty<LineBreakOpportunity>();
            }

            var result = new LineBreakOpportunity[text.Length - 1];

            // Initialize all positions as Forbidden.
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = LineBreakOpportunity.Forbidden;
            }

            for (int i = 0; i < result.Length; i++)
            {
                char current = text[i];
                char next = text[i + 1];

                // Rule: Mandatory break after LF.
                if (current == '\n')
                {
                    result[i] = LineBreakOpportunity.Mandatory;
                    continue;
                }

                // Rule: CR LF pair - no break between CR and LF; mandatory break after LF is handled above.
                if (current == '\r')
                {
                    if (next == '\n')
                    {
                        // No break between CR and LF.
                        result[i] = LineBreakOpportunity.Forbidden;
                    }
                    else
                    {
                        // CR alone is a mandatory break.
                        result[i] = LineBreakOpportunity.Mandatory;
                    }
                    continue;
                }

                // Rule: No break at start of text (i == 0 is already Forbidden by default).

                // Rule: Always break after line/paragraph separators.
                if (current == '\u000B' || current == '\u000C' || current == '\u0085' ||
                    current == '\u2028' || current == '\u2029')
                {
                    result[i] = LineBreakOpportunity.Mandatory;
                    continue;
                }

                // Rule: Break after zero-width space.
                if (current == '\u200B')
                {
                    result[i] = LineBreakOpportunity.Allowed;
                    continue;
                }

                // Rule: Do not break before or after non-breaking space.
                if (current == '\u00A0' || next == '\u00A0')
                {
                    result[i] = LineBreakOpportunity.Forbidden;
                    continue;
                }

                // Rule: Do not break before or after word joiner.
                if (current == '\u2060' || current == '\uFEFF' ||
                    next == '\u2060' || next == '\uFEFF')
                {
                    result[i] = LineBreakOpportunity.Forbidden;
                    continue;
                }

                // Rule: Break after spaces (unless next is start of line or special).
                if (current == ' ' || current == '\t' ||
                    IsUnicodeSpace(current))
                {
                    // Allow break after space before non-space.
                    if (!IsUnicodeSpace(next) && next != ' ')
                    {
                        result[i] = LineBreakOpportunity.Allowed;
                    }
                    continue;
                }

                // Rule: Break after hyphens.
                if (current == '-' || current == '\u2010' || current == '\u2013' || current == '\u00AD')
                {
                    result[i] = LineBreakOpportunity.Allowed;
                    continue;
                }

                // Rule: Break before hyphens (for em dash).
                if (next == '\u2014') // em dash
                {
                    result[i] = LineBreakOpportunity.Allowed;
                    continue;
                }

                // Rule: CJK ideographs can break before and after.
                if (IsCjkIdeograph(current) || IsCjkIdeograph(next))
                {
                    // Do not break before certain CJK closing punctuation.
                    var nextClass = LineBreakClassifier.GetClass(next);
                    if (nextClass == LineBreakClass.CL || nextClass == LineBreakClass.EX ||
                        nextClass == LineBreakClass.NS)
                    {
                        result[i] = LineBreakOpportunity.Forbidden;
                        continue;
                    }

                    // Do not break after CJK opening punctuation.
                    var currentClass = LineBreakClassifier.GetClass(current);
                    if (currentClass == LineBreakClass.OP)
                    {
                        result[i] = LineBreakOpportunity.Forbidden;
                        continue;
                    }

                    result[i] = LineBreakOpportunity.Allowed;
                    continue;
                }

                // Default: no break inside words (alphabetic/numeric sequences).
                // result[i] remains Forbidden.
            }

            return result;
        }

        private static bool IsUnicodeSpace(char ch)
        {
            return ch == '\u1680' ||
                   (ch >= '\u2000' && ch <= '\u200A') ||
                   ch == '\u205F' ||
                   ch == '\u3000';
        }

        private static bool IsCjkIdeograph(char ch)
        {
            return (ch >= 0x3400 && ch <= 0x4DBF) ||
                   (ch >= 0x4E00 && ch <= 0x9FFF) ||
                   (ch >= 0xF900 && ch <= 0xFAFF);
        }
    }
}
