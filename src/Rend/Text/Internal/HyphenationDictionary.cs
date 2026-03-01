using System;
using System.Collections.Generic;

namespace Rend.Text.Internal
{
    /// <summary>
    /// Implements the Knuth-Liang hyphenation algorithm.
    /// Patterns are loaded in TeX hyphenation pattern format. Each pattern contains
    /// interleaved digits and letters, where digits indicate hyphenation weights at
    /// positions between characters. Odd weights allow hyphenation; even weights forbid it.
    /// The maximum weight at each position across all matching patterns determines the result.
    /// </summary>
    internal sealed class HyphenationDictionary
    {
        private readonly Dictionary<string, byte[]> _patterns = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        private int _leftMin = 2;
        private int _rightMin = 3;

        /// <summary>
        /// Gets or sets the minimum number of characters before the first hyphen point.
        /// Default is 2.
        /// </summary>
        public int LeftMin
        {
            get => _leftMin;
            set => _leftMin = Math.Max(1, value);
        }

        /// <summary>
        /// Gets or sets the minimum number of characters after the last hyphen point.
        /// Default is 3.
        /// </summary>
        public int RightMin
        {
            get => _rightMin;
            set => _rightMin = Math.Max(1, value);
        }

        /// <summary>
        /// Loads hyphenation patterns from a string in TeX hyphenation pattern format.
        /// Patterns are separated by whitespace (spaces, tabs, newlines).
        /// Each pattern consists of interleaved letters and digits, e.g. ".hy1p", "he2n", "hen3a4t".
        /// A leading or trailing dot indicates a word boundary.
        /// </summary>
        /// <param name="patterns">The pattern string to load.</param>
        public void LoadPatterns(string patterns)
        {
            if (patterns == null) throw new ArgumentNullException(nameof(patterns));

            // Split on whitespace
            int i = 0;
            int len = patterns.Length;
            while (i < len)
            {
                // Skip whitespace
                while (i < len && char.IsWhiteSpace(patterns[i]))
                    i++;

                if (i >= len) break;

                // Collect token
                int start = i;
                while (i < len && !char.IsWhiteSpace(patterns[i]))
                    i++;

                string token = patterns.Substring(start, i - start);
                if (token.Length > 0)
                {
                    ParseAndAddPattern(token);
                }
            }
        }

        private void ParseAndAddPattern(string pattern)
        {
            // A pattern like ".hy1p" has letters (plus dots for word boundary) and digits.
            // We separate them: the letter sequence forms the key, and the digit array
            // records the weight at each inter-character position.
            //
            // For pattern "he2n": letters = "hen", weights[0]=0, weights[1]=0, weights[2]=2, weights[3]=0
            // weights array has length = letters.Length + 1

            // Extract letters and digit positions
            var letters = new List<char>();
            var digits = new List<byte>();
            bool lastWasDigit = false;

            for (int i = 0; i < pattern.Length; i++)
            {
                char c = pattern[i];
                if (c >= '0' && c <= '9')
                {
                    byte val = (byte)(c - '0');
                    if (lastWasDigit)
                    {
                        // Shouldn't happen in valid patterns, but handle gracefully
                        digits[digits.Count - 1] = Math.Max(digits[digits.Count - 1], val);
                    }
                    else
                    {
                        digits.Add(val);
                        lastWasDigit = true;
                    }
                }
                else
                {
                    if (!lastWasDigit)
                    {
                        // No digit before this letter position, insert 0
                        digits.Add(0);
                    }
                    letters.Add(c);
                    lastWasDigit = false;
                }
            }
            // Ensure trailing weight
            if (!lastWasDigit)
            {
                digits.Add(0);
            }

            string key = new string(letters.ToArray());
            byte[] weights = digits.ToArray();

            _patterns[key] = weights;
        }

        /// <summary>
        /// Returns an array of booleans indicating valid hyphen positions for the given word.
        /// <c>result[i]</c> is <c>true</c> if a hyphen can be inserted after character at index <c>i</c>.
        /// The array length equals <c>word.Length - 1</c> (positions between characters),
        /// or is empty for words with fewer than 2 characters.
        /// </summary>
        /// <param name="word">The word to hyphenate (letters only, no spaces or punctuation).</param>
        /// <returns>Array of booleans for each inter-character position.</returns>
        public bool[] FindHyphenPoints(string word)
        {
            if (string.IsNullOrEmpty(word) || word.Length < 2)
            {
                return Array.Empty<bool>();
            }

            // Prepend/append dots for word boundary matching
            string work = "." + word.ToLowerInvariant() + ".";
            int workLen = work.Length;

            // Weights array: one per inter-character position in the work string
            // Position i means between work[i-1] and work[i], or equivalently "before work[i]"
            // We track weights at positions 0..workLen (inclusive)
            var levels = new byte[workLen + 1];

            // Slide each possible substring over the work string and look up patterns
            for (int i = 0; i < workLen; i++)
            {
                for (int j = i + 1; j <= workLen; j++)
                {
                    string fragment = work.Substring(i, j - i);
                    if (_patterns.TryGetValue(fragment, out byte[] weights))
                    {
                        // Apply weights: weights[k] applies to position (i + k)
                        for (int k = 0; k < weights.Length && (i + k) <= workLen; k++)
                        {
                            if (weights[k] > levels[i + k])
                            {
                                levels[i + k] = weights[k];
                            }
                        }
                    }
                }
            }

            // Convert to boolean array for the original word positions.
            // levels[0] corresponds to before the dot, levels[1] to before word[0], etc.
            // Position between word[i] and word[i+1] corresponds to levels[i + 2]
            // (because work = "." + word + ".", so word[i] is work[i+1],
            //  and the position between word[i] and word[i+1] is work position i+2)
            int resultLen = word.Length - 1;
            var result = new bool[resultLen];

            for (int i = 0; i < resultLen; i++)
            {
                int levelIndex = i + 2; // offset by 2: 1 for dot prefix, 1 for 0-based to position
                bool isOdd = (levels[levelIndex] & 1) != 0;

                // Apply min prefix/suffix constraints
                bool inPrefix = (i + 1) < _leftMin;           // i+1 chars before this point
                bool inSuffix = (word.Length - i - 1) < _rightMin; // remaining chars after this point

                result[i] = isOdd && !inPrefix && !inSuffix;
            }

            return result;
        }
    }
}
