using System.Globalization;
using System.Text;
using Rend.Css;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Builds the textual content of a <c>::marker</c> pseudo-element for a
    /// list item, including the trailing separator space that Chrome emits
    /// between the marker and the content.
    /// </summary>
    /// <spec>CSS-LISTS-3 §3 https://drafts.csswg.org/css-lists/#markers</spec>
    internal static class ListMarkerTextBuilder
    {
        private const string DiscMarker = "\u2022 ";
        private const string CircleMarker = "\u25E6 ";
        private const string SquareMarker = "\u25AA ";
        private const int MaxRomanNumeral = 3999;
        private const int AlphaBufferSize = 8;

        /// <summary>
        /// Returns the rendered marker string for the given list-style-type
        /// and ordinal. The returned string always includes a trailing
        /// space so that layout can measure the full <c>::marker</c> box
        /// width used by Chrome. Returns <c>null</c> for
        /// <see cref="CssListStyleType.None"/>.
        /// </summary>
        public static string? BuildMarkerText(CssListStyleType listType, int ordinal)
        {
            switch (listType)
            {
                case CssListStyleType.Disc:
                {
                    return DiscMarker;
                }
                case CssListStyleType.Circle:
                {
                    return CircleMarker;
                }
                case CssListStyleType.Square:
                {
                    return SquareMarker;
                }
                case CssListStyleType.Decimal:
                {
                    return ordinal.ToString(CultureInfo.InvariantCulture) + ". ";
                }
                case CssListStyleType.DecimalLeadingZero:
                {
                    return ordinal.ToString("D2", CultureInfo.InvariantCulture) + ". ";
                }
                case CssListStyleType.LowerAlpha:
                case CssListStyleType.LowerLatin:
                {
                    return ToAlpha(ordinal, lowercase: true) + ". ";
                }
                case CssListStyleType.UpperAlpha:
                case CssListStyleType.UpperLatin:
                {
                    return ToAlpha(ordinal, lowercase: false) + ". ";
                }
                case CssListStyleType.LowerRoman:
                {
                    return ToRoman(ordinal).ToLowerInvariant() + ". ";
                }
                case CssListStyleType.UpperRoman:
                {
                    return ToRoman(ordinal) + ". ";
                }
                default:
                {
                    return null;
                }
            }
        }

        private static string ToAlpha(int ordinal, bool lowercase)
        {
            if (ordinal <= 0)
            {
                return ordinal.ToString(CultureInfo.InvariantCulture);
            }

            char[] buffer = new char[AlphaBufferSize];
            int position = buffer.Length;
            int value = ordinal - 1;

            do
            {
                position--;
                int remainder = value % 26;
                buffer[position] = (char)((lowercase ? 'a' : 'A') + remainder);
                value = value / 26 - 1;
            }
            while (value >= 0 && position > 0);

            return new string(buffer, position, buffer.Length - position);
        }

        private static string ToRoman(int number)
        {
            if (number <= 0 || number > MaxRomanNumeral)
            {
                return number.ToString(CultureInfo.InvariantCulture);
            }

            int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            string[] numerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

            var result = new StringBuilder(15);
            int remaining = number;

            for (int i = 0; i < values.Length; i++)
            {
                while (remaining >= values[i])
                {
                    result.Append(numerals[i]);
                    remaining -= values[i];
                }
            }

            return result.ToString();
        }
    }
}
