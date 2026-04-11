using System;

namespace Rend.Text.Internal
{
    /// <summary>
    /// Represents the type of line break opportunity at a given position.
    /// </summary>
    internal enum LineBreakOpportunity : byte
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
    internal sealed class LineBreaker
    {
        /// <summary>
        /// Finds line break opportunities between each pair of adjacent characters.
        /// Returns an array of length <c>text.Length - 1</c> (or empty for text of length 0 or 1),
        /// where each element indicates the break opportunity between character at index <c>i</c>
        /// and character at index <c>i + 1</c>.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <param name="lineBreak">The CSS line-break property value.</param>
        /// <param name="breakSpaces">When true, every space is a wrapping opportunity (CSS break-spaces).</param>
        /// <returns>Array of break opportunities between adjacent characters.</returns>
        public LineBreakOpportunity[] FindBreaks(string text,
            Css.CssLineBreak lineBreak = Css.CssLineBreak.Auto,
            bool breakSpaces = false)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            if (text.Length <= 1)
            {
                return Array.Empty<LineBreakOpportunity>();
            }

            var result = new LineBreakOpportunity[text.Length - 1];

            // [CSS-TEXT-3 §5.3] line-break: anywhere — every position is a break opportunity
            if (lineBreak == Css.CssLineBreak.Anywhere)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    char current = text[i];
                    if (current == '\n')
                    {
                        result[i] = LineBreakOpportunity.Mandatory;
                    }
                    else if (current == '\r')
                    {
                        result[i] = LineBreakOpportunity.Mandatory;
                    }
                    else
                    {
                        result[i] = LineBreakOpportunity.Allowed;
                    }
                }
                return result;
            }

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

                // Rule: Break after spaces.
                if (current == ' ' || current == '\t' ||
                    IsUnicodeSpace(current))
                {
                    // [CSS-TEXT-3 §4.1.3] break-spaces: every preserved space is
                    // a wrapping opportunity — mark Allowed after EVERY space.
                    if (breakSpaces)
                    {
                        result[i] = LineBreakOpportunity.Allowed;
                    }
                    else if (!IsUnicodeSpace(next) && next != ' ')
                    {
                        // Normal: allow break after space before non-space only.
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
                    // [UAX #14 LB13] Do not break before closing punctuation, exclamation,
                    // infix separators, symbols, or non-starters.
                    var nextClass = LineBreakClassifier.GetClass(next);
                    if (nextClass == LineBreakClass.CL || nextClass == LineBreakClass.CP ||
                        nextClass == LineBreakClass.EX || nextClass == LineBreakClass.IS_ ||
                        nextClass == LineBreakClass.SY || nextClass == LineBreakClass.NS)
                    {
                        result[i] = LineBreakOpportunity.Forbidden;
                        continue;
                    }

                    // [UAX #14 LB14] Do not break after opening punctuation.
                    var currentClass = LineBreakClassifier.GetClass(current);
                    if (currentClass == LineBreakClass.OP)
                    {
                        result[i] = LineBreakOpportunity.Forbidden;
                        continue;
                    }

                    // [CSS-TEXT-3 §5.3] line-break: strict — forbid breaks before
                    // small kana, prolonged sound mark, and iteration marks.
                    if (lineBreak == Css.CssLineBreak.Strict && IsSmallKanaOrIterationMark(next))
                    {
                        result[i] = LineBreakOpportunity.Forbidden;
                        continue;
                    }

                    result[i] = LineBreakOpportunity.Allowed;
                    continue;
                }

                // [CSS-TEXT-3 §5.3] line-break: loose — allow breaks before certain
                // CJK mid-sentence punctuation that normal mode forbids.
                if (lineBreak == Css.CssLineBreak.Loose && IsCjkLooseBreakBefore(next))
                {
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

        /// <summary>
        /// [CSS-TEXT-3 §5.3] Small kana, prolonged sound mark, and iteration marks.
        /// In strict mode, line breaks before these characters are forbidden.
        /// </summary>
        private static bool IsSmallKanaOrIterationMark(char ch)
        {
            // Hiragana small letters
            if (ch == '\u3041' || ch == '\u3043' || ch == '\u3045' || ch == '\u3047' || ch == '\u3049' ||
                ch == '\u3063' || ch == '\u3083' || ch == '\u3085' || ch == '\u3087' || ch == '\u308E' ||
                ch == '\u3095' || ch == '\u3096')
            {
                return true;
            }
            // Katakana small letters
            if (ch == '\u30A1' || ch == '\u30A3' || ch == '\u30A5' || ch == '\u30A7' || ch == '\u30A9' ||
                ch == '\u30C3' || ch == '\u30E3' || ch == '\u30E5' || ch == '\u30E7' || ch == '\u30EE' ||
                ch == '\u30F5' || ch == '\u30F6')
            {
                return true;
            }
            // Prolonged sound mark
            if (ch == '\u30FC') { return true; }
            // Iteration marks
            if (ch == '\u3005' || ch == '\u303B' || ch == '\u309D' || ch == '\u309E' ||
                ch == '\u30FD' || ch == '\u30FE')
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// [CSS-TEXT-3 §5.3] In loose mode, breaks before these CJK punctuation are allowed.
        /// </summary>
        private static bool IsCjkLooseBreakBefore(char ch)
        {
            return ch == '\u30FB' || // katakana middle dot
                   ch == '\u3005' || // ideographic iteration mark
                   ch == '\u301C' || // wave dash
                   ch == '\u30A0';   // katakana-hiragana double hyphen
        }
    }
}
