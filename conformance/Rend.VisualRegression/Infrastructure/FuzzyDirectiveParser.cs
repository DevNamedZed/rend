using System;
using System.Text.RegularExpressions;

namespace Rend.VisualRegression.Infrastructure
{
    /// <summary>
    /// Parses the WPT reftest fuzzy matching directive — <c>&lt;meta name="fuzzy"
    /// content="..."&gt;</c> — into a <see cref="FuzzyTolerance"/>.
    /// </summary>
    /// <remarks>
    /// Grammar (https://web-platform-tests.org/writing-tests/reftests.html#fuzzy-matching):
    /// <code>
    /// content = [ selector ":" ] fuzzy-range
    /// fuzzy-range = "maxDifference=" range ";" "totalPixels=" range
    ///             | range ";" range
    /// range = number | number "-" number
    /// </code>
    /// The optional selector prefix scopes fuzziness to a subset of the page;
    /// we currently ignore it and apply the tolerance to the whole image.
    /// </remarks>
    public static class FuzzyDirectiveParser
    {
        private static readonly Regex MetaTagPattern = new Regex(
            @"<meta\b[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AttributePattern = new Regex(
            @"(\w+)\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s>]+))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Scans HTML for the first <c>&lt;meta name="fuzzy"&gt;</c> element and
        /// returns its parsed tolerance, or null when no directive is present or
        /// the content attribute cannot be parsed.
        /// </summary>
        public static FuzzyTolerance? Parse(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }

            foreach (Match metaMatch in MetaTagPattern.Matches(html))
            {
                string tag = metaMatch.Value;
                string? name = null;
                string? content = null;

                foreach (Match attrMatch in AttributePattern.Matches(tag))
                {
                    string attrName = attrMatch.Groups[1].Value;
                    string attrValue = attrMatch.Groups[2].Success ? attrMatch.Groups[2].Value
                        : attrMatch.Groups[3].Success ? attrMatch.Groups[3].Value
                        : attrMatch.Groups[4].Value;

                    if (attrName.Equals("name", StringComparison.OrdinalIgnoreCase))
                    {
                        name = attrValue;
                    }
                    else if (attrName.Equals("content", StringComparison.OrdinalIgnoreCase))
                    {
                        content = attrValue;
                    }
                }

                if (name != null && name.Equals("fuzzy", StringComparison.OrdinalIgnoreCase) && content != null)
                {
                    return ParseContent(content);
                }
            }

            return null;
        }

        /// <summary>
        /// Parses a raw fuzzy content attribute string.
        /// </summary>
        public static FuzzyTolerance? ParseContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string body = StripSelectorPrefix(content);

            int? maxDifferenceMin = null;
            int? maxDifferenceMax = null;
            int? totalPixelsMin = null;
            int? totalPixelsMax = null;

            int positionalIndex = 0;
            var parts = body.Split(';');

            foreach (var rawPart in parts)
            {
                string part = rawPart.Trim();
                if (part.Length == 0)
                {
                    continue;
                }

                string key;
                string valueText;
                int equalsIndex = part.IndexOf('=');
                if (equalsIndex >= 0)
                {
                    key = part.Substring(0, equalsIndex).Trim();
                    valueText = part.Substring(equalsIndex + 1).Trim();
                }
                else
                {
                    key = positionalIndex == 0 ? "maxDifference" : "totalPixels";
                    valueText = part;
                    positionalIndex++;
                }

                if (!TryParseRange(valueText, out int rangeMin, out int rangeMax))
                {
                    return null;
                }

                if (key.Equals("maxDifference", StringComparison.OrdinalIgnoreCase))
                {
                    maxDifferenceMin = rangeMin;
                    maxDifferenceMax = rangeMax;
                }
                else if (key.Equals("totalPixels", StringComparison.OrdinalIgnoreCase))
                {
                    totalPixelsMin = rangeMin;
                    totalPixelsMax = rangeMax;
                }
                else
                {
                    return null;
                }
            }

            if (maxDifferenceMax == null || totalPixelsMax == null)
            {
                return null;
            }

            return new FuzzyTolerance(
                maxDifferenceMin ?? 0,
                maxDifferenceMax.Value,
                totalPixelsMin ?? 0,
                totalPixelsMax.Value);
        }

        /// <summary>
        /// Removes an optional leading <c>selector:</c> prefix before the fuzzy
        /// ranges. The selector is detected as any non-numeric substring ending
        /// in a colon that appears before the first range delimiter.
        /// </summary>
        private static string StripSelectorPrefix(string content)
        {
            int firstSemi = content.IndexOf(';');
            int firstEquals = content.IndexOf('=');
            int cutoff = content.Length;
            if (firstSemi >= 0 && firstSemi < cutoff)
            {
                cutoff = firstSemi;
            }
            if (firstEquals >= 0 && firstEquals < cutoff)
            {
                cutoff = firstEquals;
            }

            int colonIndex = content.IndexOf(':');
            if (colonIndex < 0 || colonIndex >= cutoff)
            {
                return content;
            }

            return content.Substring(colonIndex + 1).TrimStart();
        }

        private static bool TryParseRange(string valueText, out int rangeMin, out int rangeMax)
        {
            rangeMin = 0;
            rangeMax = 0;

            if (string.IsNullOrEmpty(valueText))
            {
                return false;
            }

            int dashIndex = valueText.IndexOf('-');
            if (dashIndex < 0)
            {
                if (!int.TryParse(valueText, out int singleValue))
                {
                    return false;
                }
                rangeMin = singleValue;
                rangeMax = singleValue;
                return true;
            }

            string lowText = valueText.Substring(0, dashIndex).Trim();
            string highText = valueText.Substring(dashIndex + 1).Trim();
            if (!int.TryParse(lowText, out rangeMin) || !int.TryParse(highText, out rangeMax))
            {
                return false;
            }
            return true;
        }
    }
}
