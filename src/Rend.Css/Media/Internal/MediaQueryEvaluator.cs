using System;

namespace Rend.Css.Media.Internal
{
    /// <summary>
    /// Evaluates @media query strings against a MediaContext.
    /// Supports: media types (screen, print, all), width/height features with min/max.
    /// </summary>
    internal static class MediaQueryEvaluator
    {
        /// <summary>
        /// Evaluate a media query text against the context.
        /// Returns true if the media matches (or if the query is empty/invalid).
        /// </summary>
        public static bool Evaluate(string mediaText, MediaContext context)
        {
            if (string.IsNullOrWhiteSpace(mediaText))
                return true;

            // Handle comma-separated media query list (OR semantics)
            var queries = mediaText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < queries.Length; i++)
            {
                if (EvaluateSingle(queries[i].Trim(), context))
                    return true;
            }

            return false;
        }

        private static bool EvaluateSingle(string query, MediaContext context)
        {
            if (string.IsNullOrWhiteSpace(query))
                return true;

            bool negated = false;
            var lower = query.ToLowerInvariant().Trim();

            // Check for "not" prefix
            if (lower.StartsWith("not "))
            {
                negated = true;
                lower = lower.Substring(4).Trim();
            }
            else if (lower.StartsWith("only "))
            {
                lower = lower.Substring(5).Trim();
            }

            bool result = EvaluateBody(lower, context);
            return negated ? !result : result;
        }

        private static bool EvaluateBody(string query, MediaContext context)
        {
            // Split on "and"
            var parts = query.Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i].Trim();

                // Media type
                if (part == "all" || part == "screen" || part == "print")
                {
                    if (part != "all" && !part.Equals(context.MediaType, StringComparison.OrdinalIgnoreCase))
                        return false;
                    continue;
                }

                // Feature in parentheses: (min-width: 768px)
                if (part.StartsWith("(") && part.EndsWith(")"))
                {
                    var feature = part.Substring(1, part.Length - 2).Trim();
                    if (!EvaluateFeature(feature, context))
                        return false;
                    continue;
                }

                // Unknown — ignore (permissive)
            }

            return true;
        }

        private static bool EvaluateFeature(string feature, MediaContext context)
        {
            var colonIdx = feature.IndexOf(':');
            if (colonIdx < 0)
            {
                // Boolean feature (e.g. "color")
                return true; // assume supported
            }

            var name = feature.Substring(0, colonIdx).Trim().ToLowerInvariant();
            var valueStr = feature.Substring(colonIdx + 1).Trim();

            // Parse numeric value with optional unit
            if (!TryParseLength(valueStr, out float value))
                return true; // can't parse — assume match (permissive)

            switch (name)
            {
                case "width": return context.Width == value;
                case "min-width": return context.Width >= value;
                case "max-width": return context.Width <= value;
                case "height": return context.Height == value;
                case "min-height": return context.Height >= value;
                case "max-height": return context.Height <= value;
                case "orientation":
                    return valueStr.Trim().Equals(context.Orientation, StringComparison.OrdinalIgnoreCase);
                default:
                    return true; // unknown feature — assume match
            }
        }

        private static bool TryParseLength(string value, out float px)
        {
            px = 0;
            value = value.Trim().ToLowerInvariant();

            // Try to extract number and unit
            int unitStart = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if ((c >= '0' && c <= '9') || c == '.' || c == '-' || c == '+')
                    unitStart = i + 1;
                else
                    break;
            }

            var numStr = value.Substring(0, unitStart);
            var unit = value.Substring(unitStart).Trim();

            if (!float.TryParse(numStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float num))
                return false;

            switch (unit)
            {
                case "px": case "": px = num; return true;
                case "em": case "rem": px = num * 16; return true; // approximate
                case "pt": px = num * 96f / 72f; return true;
                case "cm": px = num * 96f / 2.54f; return true;
                case "mm": px = num * 96f / 25.4f; return true;
                case "in": px = num * 96f; return true;
                default: px = num; return true;
            }
        }
    }
}
