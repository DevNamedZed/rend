using System;
using System.Collections.Generic;

namespace Rend.Css
{
    /// <summary>
    /// Represents a single Unicode range (e.g. U+0-7F, U+0400-04FF, U+4??)
    /// as used in the @font-face unicode-range descriptor.
    /// </summary>
    public readonly struct UnicodeRange : IEquatable<UnicodeRange>
    {
        /// <summary>Start code point (inclusive).</summary>
        public int Start { get; }

        /// <summary>End code point (inclusive).</summary>
        public int End { get; }

        public UnicodeRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        /// <summary>Check if a code point falls within this range.</summary>
        public bool Contains(int codePoint) => codePoint >= Start && codePoint <= End;

        /// <summary>
        /// Parse a unicode-range CSS value string into a list of ranges.
        /// Supports: U+XX, U+XX-YY, U+XX?? (wildcard), comma-separated lists.
        /// </summary>
        public static List<UnicodeRange> Parse(string value)
        {
            var ranges = new List<UnicodeRange>();
            if (string.IsNullOrWhiteSpace(value))
                return ranges;

            var parts = value.Split(',');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.Length == 0) continue;

                // Must start with U+ or u+
                if (trimmed.Length < 3 ||
                    (trimmed[0] != 'U' && trimmed[0] != 'u') ||
                    trimmed[1] != '+')
                    continue;

                var rangeStr = trimmed.Substring(2);

                // Check for range format: XXXX-YYYY
                int dashIdx = rangeStr.IndexOf('-');
                if (dashIdx > 0)
                {
                    var startHex = rangeStr.Substring(0, dashIdx);
                    var endHex = rangeStr.Substring(dashIdx + 1);
                    if (TryParseHex(startHex, out int start) && TryParseHex(endHex, out int end))
                    {
                        ranges.Add(new UnicodeRange(start, end));
                    }
                }
                // Check for wildcard format: XX?? (? means any hex digit)
                else if (rangeStr.IndexOf('?') >= 0)
                {
                    var startHex = rangeStr.Replace('?', '0');
                    var endHex = rangeStr.Replace('?', 'F');
                    if (TryParseHex(startHex, out int start) && TryParseHex(endHex, out int end))
                    {
                        ranges.Add(new UnicodeRange(start, end));
                    }
                }
                // Single code point: XXXX
                else
                {
                    if (TryParseHex(rangeStr, out int codePoint))
                    {
                        ranges.Add(new UnicodeRange(codePoint, codePoint));
                    }
                }
            }

            return ranges;
        }

        private static bool TryParseHex(string hex, out int value)
        {
            value = 0;
            if (string.IsNullOrEmpty(hex) || hex.Length > 6)
                return false;

            for (int i = 0; i < hex.Length; i++)
            {
                char c = hex[i];
                int digit;
                if (c >= '0' && c <= '9')
                    digit = c - '0';
                else if (c >= 'a' && c <= 'f')
                    digit = c - 'a' + 10;
                else if (c >= 'A' && c <= 'F')
                    digit = c - 'A' + 10;
                else
                    return false;

                value = (value << 4) | digit;
            }
            return true;
        }

        public bool Equals(UnicodeRange other) => Start == other.Start && End == other.End;
        public override bool Equals(object? obj) => obj is UnicodeRange other && Equals(other);
        public override int GetHashCode() => Start * 397 ^ End;
        public override string ToString() =>
            Start == End ? $"U+{Start:X}" : $"U+{Start:X}-{End:X}";
    }
}
