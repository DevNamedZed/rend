using System.Collections.Generic;
using Rend.Css;
using Rend.Html;
using Rend.Html.Selectors;

namespace Rend.Adapters
{
    /// <summary>
    /// Bridges Rend.Html's selector engine to Rend.Css's <see cref="ISelectorMatcher"/>.
    /// Converts <see cref="IStylableElement"/> back to the underlying <see cref="Element"/>
    /// for matching, and computes specificity via the Html selector parser.
    /// </summary>
    public sealed class SelectorMatcherAdapter : ISelectorMatcher
    {
        private readonly Dictionary<string, SelectorList> _cache = new Dictionary<string, SelectorList>();

        public bool Matches(IStylableElement element, string selectorText)
        {
            if (element is StylableElementAdapter adapter)
            {
                var parsed = GetParsed(selectorText);
                return parsed.Matches(adapter.Element);
            }
            return false;
        }

        public CssSpecificity GetSpecificity(string selectorText)
        {
            // Parse the selector and compute specificity.
            // We use a simple heuristic parser matching the CSS spec:
            // count #id, .class/[attr]/:pseudo-class, type selectors
            int a = 0, b = 0, c = 0;
            int i = 0;
            while (i < selectorText.Length)
            {
                char ch = selectorText[i];

                if (ch == '#')
                {
                    a++;
                    i++;
                    while (i < selectorText.Length && IsNameChar(selectorText[i])) i++;
                }
                else if (ch == '.')
                {
                    b++;
                    i++;
                    while (i < selectorText.Length && IsNameChar(selectorText[i])) i++;
                }
                else if (ch == '[')
                {
                    b++;
                    while (i < selectorText.Length && selectorText[i] != ']') i++;
                    if (i < selectorText.Length) i++;
                }
                else if (ch == ':')
                {
                    i++;
                    if (i < selectorText.Length && selectorText[i] == ':')
                    {
                        // Pseudo-element
                        c++;
                        i++;
                    }
                    else
                    {
                        // Pseudo-class
                        b++;
                    }
                    while (i < selectorText.Length && IsNameChar(selectorText[i])) i++;
                    // Skip function arguments like :nth-child(...)
                    if (i < selectorText.Length && selectorText[i] == '(')
                    {
                        int depth = 1;
                        i++;
                        while (i < selectorText.Length && depth > 0)
                        {
                            if (selectorText[i] == '(') depth++;
                            else if (selectorText[i] == ')') depth--;
                            i++;
                        }
                    }
                }
                else if (ch == '*')
                {
                    // Universal — no specificity
                    i++;
                }
                else if (ch == ' ' || ch == '>' || ch == '+' || ch == '~' || ch == ',')
                {
                    i++;
                }
                else if (IsNameStartChar(ch))
                {
                    c++;
                    while (i < selectorText.Length && IsNameChar(selectorText[i])) i++;
                }
                else
                {
                    i++;
                }
            }

            return new CssSpecificity(a, b, c);
        }

        private SelectorList GetParsed(string selectorText)
        {
            if (!_cache.TryGetValue(selectorText, out var parsed))
            {
                parsed = SelectorList.Parse(selectorText);
                if (_cache.Count < 2048)
                    _cache[selectorText] = parsed;
            }
            return parsed;
        }

        private static bool IsNameStartChar(char c)
            => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || c == '-' || c > 127;

        private static bool IsNameChar(char c)
            => IsNameStartChar(c) || (c >= '0' && c <= '9');
    }
}
