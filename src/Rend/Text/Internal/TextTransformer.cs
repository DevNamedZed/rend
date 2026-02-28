using System;
using System.Text;
using Rend.Css;

namespace Rend.Text.Internal
{
    /// <summary>
    /// Transforms text according to CSS text-transform property values.
    /// </summary>
    public static class TextTransformer
    {
        /// <summary>
        /// Transforms the given text according to the specified CSS text-transform mode.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <param name="transform">The CSS text-transform mode.</param>
        /// <returns>The transformed text.</returns>
        public static string Transform(string text, CssTextTransform transform)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            if (text.Length == 0)
            {
                return text;
            }

            switch (transform)
            {
                case CssTextTransform.None:
                    return text;

                case CssTextTransform.Uppercase:
                    return text.ToUpperInvariant();

                case CssTextTransform.Lowercase:
                    return text.ToLowerInvariant();

                case CssTextTransform.Capitalize:
                    return Capitalize(text);

                default:
                    return text;
            }
        }

        /// <summary>
        /// Capitalizes the first letter of each word in the text.
        /// A word boundary is detected when a whitespace character precedes a non-whitespace character.
        /// </summary>
        private static string Capitalize(string text)
        {
            var sb = new StringBuilder(text.Length);
            bool atWordStart = true;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];

                if (char.IsWhiteSpace(ch))
                {
                    sb.Append(ch);
                    atWordStart = true;
                }
                else if (atWordStart)
                {
                    // Check for surrogate pairs.
                    if (char.IsHighSurrogate(ch) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                    {
                        string pair = text.Substring(i, 2);
                        string upper = pair.ToUpperInvariant();
                        sb.Append(upper);
                        i++; // Skip the low surrogate.
                    }
                    else
                    {
                        sb.Append(char.ToUpperInvariant(ch));
                    }
                    atWordStart = false;
                }
                else
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }
    }
}
