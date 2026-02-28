using System.Collections.Generic;

namespace Rend.Html.Selectors.Internal
{
    /// <summary>
    /// CSS selector matching engine. Parses the selector string, then walks the DOM
    /// tree testing each element against the parsed selector AST.
    /// </summary>
    internal static class SelectorMatcher
    {
        // Simple LRU cache for parsed selectors
        private static readonly Dictionary<string, List<ComplexSelector>> _cache =
            new Dictionary<string, List<ComplexSelector>>();
        private static readonly object _cacheLock = new object();

        internal static Element? QuerySelector(Node root, string selector)
        {
            var parsed = GetParsed(selector);
            if (parsed.Count == 0) return null;

            return FindFirst(root, parsed);
        }

        internal static List<Element> QuerySelectorAll(Node root, string selector)
        {
            var parsed = GetParsed(selector);
            var results = new List<Element>();
            if (parsed.Count == 0) return results;

            FindAll(root, parsed, results);
            return results;
        }

        /// <summary>
        /// Test if an element matches a selector string.
        /// </summary>
        internal static bool Matches(Element element, string selector)
        {
            var parsed = GetParsed(selector);
            return MatchesAny(element, parsed);
        }

        private static List<ComplexSelector> GetParsed(string selector)
        {
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(selector, out var cached))
                    return cached;

                var parsed = SelectorParser.Parse(selector);
                if (_cache.Count < 1024) // Limit cache size
                    _cache[selector] = parsed;
                return parsed;
            }
        }

        private static bool MatchesAny(Element element, List<ComplexSelector> selectors)
        {
            for (int i = 0; i < selectors.Count; i++)
            {
                if (selectors[i].Matches(element))
                    return true;
            }
            return false;
        }

        private static Element? FindFirst(Node root, List<ComplexSelector> selectors)
        {
            var child = root.FirstChild;
            while (child != null)
            {
                if (child is Element el)
                {
                    if (MatchesAny(el, selectors))
                        return el;

                    var found = FindFirst(el, selectors);
                    if (found != null) return found;
                }
                child = child.NextSibling;
            }
            return null;
        }

        private static void FindAll(Node root, List<ComplexSelector> selectors, List<Element> results)
        {
            var child = root.FirstChild;
            while (child != null)
            {
                if (child is Element el)
                {
                    if (MatchesAny(el, selectors))
                        results.Add(el);

                    FindAll(el, selectors, results);
                }
                child = child.NextSibling;
            }
        }
    }
}
