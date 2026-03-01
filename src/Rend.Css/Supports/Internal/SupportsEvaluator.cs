using System;
using Rend.Css.Properties.Internal;

namespace Rend.Css.Supports.Internal
{
    /// <summary>
    /// Evaluates CSS @supports conditions.
    /// Supports: (property: value), not, and, or operators.
    /// A property is "supported" if it is registered in PropertyRegistry.
    /// </summary>
    internal static class SupportsEvaluator
    {
        /// <summary>
        /// Evaluate a @supports condition string.
        /// Returns true if the condition is met.
        /// </summary>
        public static bool Evaluate(string conditionText)
        {
            if (string.IsNullOrWhiteSpace(conditionText))
                return false;

            int pos = 0;
            return EvaluateExpression(conditionText, ref pos);
        }

        private static bool EvaluateExpression(string text, ref int pos)
        {
            SkipWhitespace(text, ref pos);

            if (pos >= text.Length) return false;

            // Check for 'not'
            if (StartsWithKeyword(text, pos, "not"))
            {
                pos += 3;
                SkipWhitespace(text, ref pos);
                return !EvaluateExpression(text, ref pos);
            }

            // Must start with '(' for a condition group
            bool result;
            if (pos < text.Length && text[pos] == '(')
            {
                result = EvaluateGroup(text, ref pos);
            }
            else
            {
                return false;
            }

            // Check for 'and' / 'or' chains
            while (pos < text.Length)
            {
                SkipWhitespace(text, ref pos);
                if (pos >= text.Length) break;

                if (StartsWithKeyword(text, pos, "and"))
                {
                    pos += 3;
                    SkipWhitespace(text, ref pos);
                    bool right = EvaluateGroup(text, ref pos);
                    result = result && right;
                }
                else if (StartsWithKeyword(text, pos, "or"))
                {
                    pos += 2;
                    SkipWhitespace(text, ref pos);
                    bool right = EvaluateGroup(text, ref pos);
                    result = result || right;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private static bool EvaluateGroup(string text, ref int pos)
        {
            if (pos >= text.Length || text[pos] != '(')
                return false;

            pos++; // skip '('

            // Find the matching ')'
            int start = pos;
            int depth = 1;
            while (pos < text.Length && depth > 0)
            {
                if (text[pos] == '(') depth++;
                else if (text[pos] == ')') depth--;
                if (depth > 0) pos++;
            }

            string inner = text.Substring(start, pos - start).Trim();

            if (pos < text.Length && text[pos] == ')')
                pos++; // skip ')'

            // Check if inner contains a nested expression (not, and, or, parenthesized groups)
            if (inner.StartsWith("not ", StringComparison.OrdinalIgnoreCase) ||
                inner.StartsWith("(", StringComparison.Ordinal))
            {
                int innerPos = 0;
                return EvaluateExpression(inner, ref innerPos);
            }

            // Check if it's a property: value declaration test
            int colonIdx = inner.IndexOf(':');
            if (colonIdx > 0)
            {
                string property = inner.Substring(0, colonIdx).Trim().ToLowerInvariant();
                // A property is "supported" if it's registered in PropertyRegistry
                return PropertyRegistry.GetByName(property) != null;
            }

            return false;
        }

        private static bool StartsWithKeyword(string text, int pos, string keyword)
        {
            if (pos + keyword.Length > text.Length) return false;
            for (int i = 0; i < keyword.Length; i++)
            {
                if (char.ToLowerInvariant(text[pos + i]) != keyword[i])
                    return false;
            }
            // Must be followed by whitespace or '('
            int end = pos + keyword.Length;
            return end >= text.Length || text[end] == ' ' || text[end] == '(' || text[end] == '\t';
        }

        private static void SkipWhitespace(string text, ref int pos)
        {
            while (pos < text.Length && (text[pos] == ' ' || text[pos] == '\t' || text[pos] == '\n' || text[pos] == '\r'))
                pos++;
        }
    }
}
