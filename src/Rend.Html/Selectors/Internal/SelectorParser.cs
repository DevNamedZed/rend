using System;
using System.Collections.Generic;

namespace Rend.Html.Selectors.Internal
{
    /// <summary>
    /// Parses a CSS selector string into a selector AST.
    /// Supports: type, universal, id, class, attribute, pseudo-class selectors,
    /// compound selectors, complex selectors with combinators, and selector lists.
    /// </summary>
    internal static class SelectorParser
    {
        /// <summary>
        /// Parse a selector string. Returns a list of complex selectors (selector list).
        /// </summary>
        public static List<ComplexSelector> Parse(string input)
        {
            var result = new List<ComplexSelector>();
            int pos = 0;
            SkipWhitespace(input, ref pos);

            while (pos < input.Length)
            {
                var complex = ParseComplexSelector(input, ref pos);
                if (complex != null)
                    result.Add(complex);

                SkipWhitespace(input, ref pos);
                if (pos < input.Length && input[pos] == ',')
                {
                    pos++;
                    SkipWhitespace(input, ref pos);
                }
            }

            return result;
        }

        private static ComplexSelector? ParseComplexSelector(string input, ref int pos)
        {
            var complex = new ComplexSelector();

            var first = ParseCompoundSelector(input, ref pos);
            if (first == null || first.Count == 0)
                return null;

            complex.AddCompound(first);

            while (pos < input.Length)
            {
                // Check for combinator
                bool hadWhitespace = SkipWhitespace(input, ref pos);
                if (pos >= input.Length || input[pos] == ',')
                    break;

                Combinator combinator;
                char c = input[pos];

                if (c == '>')
                {
                    combinator = Combinator.Child;
                    pos++;
                    SkipWhitespace(input, ref pos);
                }
                else if (c == '+')
                {
                    combinator = Combinator.NextSibling;
                    pos++;
                    SkipWhitespace(input, ref pos);
                }
                else if (c == '~')
                {
                    combinator = Combinator.SubsequentSibling;
                    pos++;
                    SkipWhitespace(input, ref pos);
                }
                else if (hadWhitespace)
                {
                    combinator = Combinator.Descendant;
                }
                else
                {
                    // No combinator and no whitespace — probably still part of compound
                    // (this shouldn't happen if compound parsing is correct)
                    break;
                }

                var next = ParseCompoundSelector(input, ref pos);
                if (next == null || next.Count == 0)
                    break;

                complex.AddCombinator(combinator);
                complex.AddCompound(next);
            }

            return complex;
        }

        private static CompoundSelector? ParseCompoundSelector(string input, ref int pos)
        {
            var compound = new CompoundSelector();

            while (pos < input.Length)
            {
                char c = input[pos];

                if (c == '*')
                {
                    compound.Add(UniversalSelector.Instance);
                    pos++;
                }
                else if (c == '#')
                {
                    pos++;
                    var id = ParseIdentifier(input, ref pos);
                    if (id != null)
                        compound.Add(new IdSelector(id));
                }
                else if (c == '.')
                {
                    pos++;
                    var cls = ParseIdentifier(input, ref pos);
                    if (cls != null)
                        compound.Add(new ClassSelector(cls));
                }
                else if (c == '[')
                {
                    var attr = ParseAttributeSelector(input, ref pos);
                    if (attr != null)
                        compound.Add(attr);
                }
                else if (c == ':')
                {
                    var pseudo = ParsePseudoClass(input, ref pos);
                    if (pseudo != null)
                        compound.Add(pseudo);
                }
                else if (IsIdentStart(c))
                {
                    var tag = ParseIdentifier(input, ref pos);
                    if (tag != null)
                        compound.Add(new TypeSelector(tag));
                }
                else
                {
                    break; // Not part of this compound
                }
            }

            return compound.Count > 0 ? compound : null;
        }

        private static AttributeSelector? ParseAttributeSelector(string input, ref int pos)
        {
            if (pos >= input.Length || input[pos] != '[')
                return null;

            pos++; // Skip [
            SkipWhitespace(input, ref pos);

            var attrName = ParseIdentifier(input, ref pos);
            if (attrName == null) { SkipUntil(input, ref pos, ']'); return null; }

            SkipWhitespace(input, ref pos);

            if (pos >= input.Length || input[pos] == ']')
            {
                if (pos < input.Length) pos++; // Skip ]
                return new AttributeSelector(attrName, AttributeOp.Exists, null, false);
            }

            // Parse operator
            AttributeOp op;
            char c = input[pos];
            if (c == '=')
            {
                op = AttributeOp.Equals;
                pos++;
            }
            else if (pos + 1 < input.Length && input[pos + 1] == '=')
            {
                switch (c)
                {
                    case '~': op = AttributeOp.Includes; break;
                    case '|': op = AttributeOp.DashMatch; break;
                    case '^': op = AttributeOp.Prefix; break;
                    case '$': op = AttributeOp.Suffix; break;
                    case '*': op = AttributeOp.Substring; break;
                    default: SkipUntil(input, ref pos, ']'); return null;
                }
                pos += 2;
            }
            else
            {
                SkipUntil(input, ref pos, ']');
                return null;
            }

            SkipWhitespace(input, ref pos);

            // Parse value (quoted or unquoted)
            var value = ParseAttributeValue(input, ref pos);

            SkipWhitespace(input, ref pos);

            // Check for case flag (i or s)
            bool caseInsensitive = false;
            if (pos < input.Length && (input[pos] == 'i' || input[pos] == 'I'))
            {
                caseInsensitive = true;
                pos++;
                SkipWhitespace(input, ref pos);
            }
            else if (pos < input.Length && (input[pos] == 's' || input[pos] == 'S'))
            {
                pos++;
                SkipWhitespace(input, ref pos);
            }

            if (pos < input.Length && input[pos] == ']')
                pos++;

            return new AttributeSelector(attrName, op, value, caseInsensitive);
        }

        private static string? ParseAttributeValue(string input, ref int pos)
        {
            if (pos >= input.Length) return null;

            char quote = input[pos];
            if (quote == '"' || quote == '\'')
            {
                pos++;
                int start = pos;
                while (pos < input.Length && input[pos] != quote)
                    pos++;
                var value = input.Substring(start, pos - start);
                if (pos < input.Length) pos++; // Skip closing quote
                return value;
            }

            // Unquoted value
            return ParseIdentifier(input, ref pos);
        }

        private static Selector? ParsePseudoClass(string input, ref int pos)
        {
            if (pos >= input.Length || input[pos] != ':')
                return null;

            pos++;

            // Skip :: for pseudo-elements (treat as pseudo-class for matching purposes)
            if (pos < input.Length && input[pos] == ':')
                pos++;

            var name = ParseIdentifier(input, ref pos);
            if (name == null) return null;

            name = name.ToLowerInvariant();

            // Functional pseudo-classes (with parentheses)
            if (pos < input.Length && input[pos] == '(')
            {
                pos++; // Skip (
                SkipWhitespace(input, ref pos);

                Selector? result = null;

                switch (name)
                {
                    case "nth-child":
                    {
                        var expr = ParseUntilCloseParen(input, ref pos);
                        var (a, b) = NthParser.Parse(expr);
                        result = PseudoClassSelector.NthChild(a, b);
                        break;
                    }
                    case "nth-last-child":
                    {
                        var expr = ParseUntilCloseParen(input, ref pos);
                        var (a, b) = NthParser.Parse(expr);
                        result = PseudoClassSelector.NthLastChild(a, b);
                        break;
                    }
                    case "nth-of-type":
                    {
                        var expr = ParseUntilCloseParen(input, ref pos);
                        var (a, b) = NthParser.Parse(expr);
                        result = PseudoClassSelector.NthOfType(a, b);
                        break;
                    }
                    case "nth-last-of-type":
                    {
                        var expr = ParseUntilCloseParen(input, ref pos);
                        var (a, b) = NthParser.Parse(expr);
                        result = PseudoClassSelector.NthLastOfType(a, b);
                        break;
                    }
                    case "not":
                    {
                        var innerSelectors = ParseSelectorListUntilParen(input, ref pos);
                        if (innerSelectors != null)
                            result = PseudoClassSelector.Not(innerSelectors);
                        break;
                    }
                    case "is":
                    {
                        var innerSelectors = ParseSelectorListUntilParen(input, ref pos);
                        if (innerSelectors != null)
                            result = PseudoClassSelector.Is(innerSelectors);
                        break;
                    }
                    case "where":
                    {
                        var innerSelectors = ParseSelectorListUntilParen(input, ref pos);
                        if (innerSelectors != null)
                            result = PseudoClassSelector.Where(innerSelectors);
                        break;
                    }
                    case "has":
                    {
                        var innerSelectors = ParseSelectorListUntilParen(input, ref pos);
                        if (innerSelectors != null)
                            result = PseudoClassSelector.Has(innerSelectors);
                        break;
                    }
                    default:
                        // Unknown functional pseudo-class — skip content
                        SkipUntil(input, ref pos, ')');
                        break;
                }

                if (pos < input.Length && input[pos] == ')')
                    pos++;

                return result;
            }

            // Simple pseudo-classes (no parentheses)
            switch (name)
            {
                case "first-child": return PseudoClassSelector.FirstChild();
                case "last-child": return PseudoClassSelector.LastChild();
                case "first-of-type": return PseudoClassSelector.FirstOfType();
                case "last-of-type": return PseudoClassSelector.LastOfType();
                case "only-child": return PseudoClassSelector.OnlyChild();
                case "only-of-type": return PseudoClassSelector.OnlyOfType();
                case "empty": return PseudoClassSelector.Empty();
                case "root": return PseudoClassSelector.Root();
                default:
                    // Unknown pseudo-class — return a never-matching selector
                    return null;
            }
        }

        private static Selector? ParseSelectorListUntilParen(string input, ref int pos)
        {
            // Find the matching close paren
            int depth = 1;
            int start = pos;
            while (pos < input.Length && depth > 0)
            {
                if (input[pos] == '(') depth++;
                else if (input[pos] == ')') depth--;
                if (depth > 0) pos++;
            }

            var innerStr = input.Substring(start, pos - start);
            var selectors = Parse(innerStr);

            if (selectors.Count == 0) return null;
            if (selectors.Count == 1) return selectors[0];

            // Multiple selectors in the list — wrap in a SelectorList
            return new SelectorListSelector(selectors);
        }

        private static string ParseUntilCloseParen(string input, ref int pos)
        {
            int start = pos;
            int depth = 1;
            while (pos < input.Length && depth > 0)
            {
                if (input[pos] == '(') depth++;
                else if (input[pos] == ')') depth--;
                if (depth > 0) pos++;
            }
            return input.Substring(start, pos - start).Trim();
        }

        private static string? ParseIdentifier(string input, ref int pos)
        {
            if (pos >= input.Length) return null;

            int start = pos;

            // Handle leading hyphen or escape
            if (input[pos] == '-')
                pos++;

            if (pos >= input.Length || (!IsIdentStart(input[pos]) && input[pos] != '\\'))
            {
                pos = start;
                return null;
            }

            while (pos < input.Length && (IsIdentChar(input[pos]) || input[pos] == '\\'))
            {
                if (input[pos] == '\\')
                {
                    pos++; // Skip backslash
                    if (pos < input.Length) pos++; // Skip escaped char
                }
                else
                {
                    pos++;
                }
            }

            if (pos == start) return null;
            return input.Substring(start, pos - start);
        }

        private static bool IsIdentStart(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || c > 127;
        }

        private static bool IsIdentChar(char c)
        {
            return IsIdentStart(c) || (c >= '0' && c <= '9') || c == '-';
        }

        private static bool SkipWhitespace(string input, ref int pos)
        {
            bool skipped = false;
            while (pos < input.Length && (input[pos] == ' ' || input[pos] == '\t' ||
                   input[pos] == '\n' || input[pos] == '\r' || input[pos] == '\f'))
            {
                pos++;
                skipped = true;
            }
            return skipped;
        }

        private static void SkipUntil(string input, ref int pos, char target)
        {
            while (pos < input.Length && input[pos] != target)
                pos++;
        }
    }

    /// <summary>
    /// A selector that matches if any of its child selectors match.
    /// Used for :not(a, b), :is(a, b), etc.
    /// </summary>
    internal sealed class SelectorListSelector : Selector
    {
        private readonly List<ComplexSelector> _selectors;

        public SelectorListSelector(List<ComplexSelector> selectors)
        {
            _selectors = selectors;
        }

        public override bool Matches(Element element)
        {
            for (int i = 0; i < _selectors.Count; i++)
            {
                if (_selectors[i].Matches(element))
                    return true;
            }
            return false;
        }

        public override Specificity GetSpecificity()
        {
            // Take the highest specificity from the list
            var max = new Specificity(0, 0, 0);
            for (int i = 0; i < _selectors.Count; i++)
            {
                var s = _selectors[i].GetSpecificity();
                if (s.CompareTo(max) > 0) max = s;
            }
            return max;
        }
    }
}
