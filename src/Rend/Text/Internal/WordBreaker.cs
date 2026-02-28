using System;
using System.Collections.Generic;

namespace Rend.Text.Internal
{
    /// <summary>
    /// Finds word boundaries in text per a simplified UAX #29 algorithm.
    /// </summary>
    public static class WordBreaker
    {
        /// <summary>
        /// Returns the indices of word boundaries in the given text.
        /// A word boundary exists at position <c>i</c> if there is a boundary between
        /// characters at <c>i - 1</c> and <c>i</c>. Position 0 and <c>text.Length</c>
        /// are always boundaries.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>Sorted array of word boundary indices.</returns>
        public static int[] FindWordBoundaries(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            if (text.Length == 0)
            {
                return new[] { 0 };
            }

            var boundaries = new List<int> { 0 };

            for (int i = 1; i < text.Length; i++)
            {
                char prev = text[i - 1];
                char curr = text[i];

                // Rule: Do not break within CRLF.
                if (prev == '\r' && curr == '\n')
                {
                    continue;
                }

                // Rule: Break after newlines.
                if (prev == '\n' || prev == '\r' || prev == '\u000B' ||
                    prev == '\u000C' || prev == '\u0085' ||
                    prev == '\u2028' || prev == '\u2029')
                {
                    boundaries.Add(i);
                    continue;
                }

                // Rule: Break before newlines.
                if (curr == '\n' || curr == '\r' || curr == '\u000B' ||
                    curr == '\u000C' || curr == '\u0085' ||
                    curr == '\u2028' || curr == '\u2029')
                {
                    boundaries.Add(i);
                    continue;
                }

                var prevCat = GetWordCategory(prev);
                var currCat = GetWordCategory(curr);

                // Rule: Do not break between same category (letter-letter, digit-digit).
                if (prevCat == currCat && prevCat != WordCategory.Other)
                {
                    continue;
                }

                // Rule: Do not break between letter and digit (e.g., "A1").
                if ((prevCat == WordCategory.Letter && currCat == WordCategory.Digit) ||
                    (prevCat == WordCategory.Digit && currCat == WordCategory.Letter))
                {
                    continue;
                }

                // Rule: Do not break around mid-word punctuation (apostrophe, middle dot)
                // between letters.
                if (prevCat == WordCategory.Letter && IsMidLetter(curr) && i + 1 < text.Length)
                {
                    var nextCat = GetWordCategory(text[i + 1]);
                    if (nextCat == WordCategory.Letter)
                    {
                        continue;
                    }
                }
                if (IsMidLetter(prev) && currCat == WordCategory.Letter && i >= 2)
                {
                    var prevPrevCat = GetWordCategory(text[i - 2]);
                    if (prevPrevCat == WordCategory.Letter)
                    {
                        continue;
                    }
                }

                // Rule: Do not break around mid-number punctuation (comma, period) between digits.
                if (prevCat == WordCategory.Digit && IsMidNum(curr) && i + 1 < text.Length)
                {
                    var nextCat = GetWordCategory(text[i + 1]);
                    if (nextCat == WordCategory.Digit)
                    {
                        continue;
                    }
                }
                if (IsMidNum(prev) && currCat == WordCategory.Digit && i >= 2)
                {
                    var prevPrevCat = GetWordCategory(text[i - 2]);
                    if (prevPrevCat == WordCategory.Digit)
                    {
                        continue;
                    }
                }

                // Rule: Do not break between consecutive spaces.
                if (prevCat == WordCategory.Space && currCat == WordCategory.Space)
                {
                    continue;
                }

                // Otherwise, break.
                boundaries.Add(i);
            }

            // Always add final boundary.
            boundaries.Add(text.Length);

            return boundaries.ToArray();
        }

        private enum WordCategory
        {
            Letter,
            Digit,
            Space,
            Other
        }

        private static WordCategory GetWordCategory(char ch)
        {
            if (char.IsLetter(ch)) return WordCategory.Letter;
            if (char.IsDigit(ch)) return WordCategory.Digit;
            if (char.IsWhiteSpace(ch)) return WordCategory.Space;
            return WordCategory.Other;
        }

        private static bool IsMidLetter(char ch)
        {
            // Apostrophes and middle dot used within words.
            return ch == '\'' || ch == '\u2019' || ch == '\u00B7' || ch == ':';
        }

        private static bool IsMidNum(char ch)
        {
            // Period, comma, and other separators used within numbers.
            return ch == '.' || ch == ',' || ch == '\u066B' || ch == '\u066C';
        }
    }
}
