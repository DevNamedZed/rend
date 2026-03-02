using System;
using System.Globalization;

namespace Rend.Css.Media.Internal
{
    /// <summary>
    /// Evaluates @container query conditions against a MediaContext.
    /// Container queries support size conditions (min-width, max-width, min-height, max-height, width, height).
    /// In a static renderer, the viewport dimensions are used as the initial containing block.
    /// </summary>
    internal static class ContainerQueryEvaluator
    {
        /// <summary>
        /// Evaluate a container query condition text against the context.
        /// Format: "[container-name] (condition)" or "(condition)"
        /// Returns true if the query matches.
        /// </summary>
        public static bool Evaluate(string conditionText, MediaContext context)
        {
            if (string.IsNullOrWhiteSpace(conditionText))
                return true;

            var text = conditionText.Trim();

            // Extract the parenthesized condition part
            // Format: "name (condition)" or "(condition)" or "name (cond1) and (cond2)"
            int firstParen = text.IndexOf('(');
            if (firstParen < 0)
                return true; // No condition specified, always matches

            // Everything before the first '(' is the optional container name (ignored in evaluation,
            // since we evaluate against viewport in the static renderer)

            // Evaluate the conditions (AND semantics for multiple conditions)
            return EvaluateConditions(text.Substring(firstParen), context);
        }

        private static bool EvaluateConditions(string condPart, MediaContext context)
        {
            // Split by " and " for AND semantics
            // Handle: "(min-width: 400px) and (max-width: 800px)"
            var lower = condPart.ToLowerInvariant();

            // Split on " and " at the top level (respecting parentheses)
            int depth = 0;
            int segStart = 0;

            for (int i = 0; i < lower.Length; i++)
            {
                if (lower[i] == '(') depth++;
                else if (lower[i] == ')') depth--;
                else if (depth == 0 && i + 5 <= lower.Length && lower.Substring(i, 5) == " and ")
                {
                    string seg = lower.Substring(segStart, i - segStart).Trim();
                    if (!EvaluateSingleCondition(seg, context))
                        return false;
                    segStart = i + 5;
                }
                else if (depth == 0 && i + 4 <= lower.Length && lower.Substring(i, 4) == " or ")
                {
                    // OR semantics
                    string seg = lower.Substring(segStart, i - segStart).Trim();
                    if (EvaluateSingleCondition(seg, context))
                        return true;
                    segStart = i + 4;
                }
            }

            // Evaluate the last (or only) segment
            string last = lower.Substring(segStart).Trim();
            return EvaluateSingleCondition(last, context);
        }

        private static bool EvaluateSingleCondition(string cond, MediaContext context)
        {
            // Strip outer parentheses
            cond = cond.Trim();
            if (cond.StartsWith("(") && cond.EndsWith(")"))
                cond = cond.Substring(1, cond.Length - 2).Trim();

            if (string.IsNullOrWhiteSpace(cond))
                return true;

            // Handle "not (...)"
            if (cond.StartsWith("not "))
            {
                return !EvaluateSingleCondition(cond.Substring(4).Trim(), context);
            }

            // Parse "feature: value" or "feature" (boolean)
            int colonIdx = cond.IndexOf(':');
            if (colonIdx < 0)
            {
                // Boolean: just check if the feature exists (e.g., "width" = has width)
                return true;
            }

            string feature = cond.Substring(0, colonIdx).Trim();
            string valueStr = cond.Substring(colonIdx + 1).Trim();

            return EvaluateFeature(feature, valueStr, context);
        }

        private static bool EvaluateFeature(string feature, string valueStr, MediaContext context)
        {
            float parsedValue = ParseLength(valueStr);
            float containerWidth = context.Width;
            float containerHeight = context.Height;

            switch (feature)
            {
                case "min-width":
                    return containerWidth >= parsedValue;
                case "max-width":
                    return containerWidth <= parsedValue;
                case "width":
                    return Math.Abs(containerWidth - parsedValue) < 0.5f;

                case "min-height":
                    return containerHeight >= parsedValue;
                case "max-height":
                    return containerHeight <= parsedValue;
                case "height":
                    return Math.Abs(containerHeight - parsedValue) < 0.5f;

                case "min-inline-size":
                    return containerWidth >= parsedValue;
                case "max-inline-size":
                    return containerWidth <= parsedValue;
                case "inline-size":
                    return Math.Abs(containerWidth - parsedValue) < 0.5f;

                case "min-block-size":
                    return containerHeight >= parsedValue;
                case "max-block-size":
                    return containerHeight <= parsedValue;
                case "block-size":
                    return Math.Abs(containerHeight - parsedValue) < 0.5f;

                case "orientation":
                    if (valueStr == "portrait") return containerHeight >= containerWidth;
                    if (valueStr == "landscape") return containerWidth > containerHeight;
                    return false;

                case "aspect-ratio":
                case "min-aspect-ratio":
                case "max-aspect-ratio":
                    return EvaluateAspectRatio(feature, valueStr, containerWidth, containerHeight);

                default:
                    return true; // Unknown features match by default for forward compatibility
            }
        }

        private static bool EvaluateAspectRatio(string feature, string valueStr, float width, float height)
        {
            // Parse "16/9" or "1.5"
            float targetRatio;
            int slashIdx = valueStr.IndexOf('/');
            if (slashIdx > 0)
            {
                if (float.TryParse(valueStr.Substring(0, slashIdx).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float num) &&
                    float.TryParse(valueStr.Substring(slashIdx + 1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float den) &&
                    den > 0)
                {
                    targetRatio = num / den;
                }
                else return true;
            }
            else if (float.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float r))
            {
                targetRatio = r;
            }
            else return true;

            float actualRatio = height > 0 ? width / height : 0;

            switch (feature)
            {
                case "min-aspect-ratio": return actualRatio >= targetRatio;
                case "max-aspect-ratio": return actualRatio <= targetRatio;
                case "aspect-ratio": return Math.Abs(actualRatio - targetRatio) < 0.01f;
                default: return true;
            }
        }

        private static float ParseLength(string value)
        {
            value = value.Trim().ToLowerInvariant();

            if (value.EndsWith("px"))
            {
                if (float.TryParse(value.Substring(0, value.Length - 2).Trim(),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out float px))
                    return px;
            }
            else if (value.EndsWith("em") || value.EndsWith("rem"))
            {
                // Approximate: 1em = 16px
                string numPart = value.EndsWith("rem") ? value.Substring(0, value.Length - 3) : value.Substring(0, value.Length - 2);
                if (float.TryParse(numPart.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float em))
                    return em * 16f;
            }
            else if (value.EndsWith("vw"))
            {
                if (float.TryParse(value.Substring(0, value.Length - 2).Trim(),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out float vw))
                    return vw; // Approximate
            }
            else
            {
                // Bare number = px
                if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float num))
                    return num;
            }

            return 0;
        }
    }
}
