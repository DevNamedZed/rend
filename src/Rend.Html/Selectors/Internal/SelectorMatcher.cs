using System.Collections.Generic;

namespace Rend.Html.Selectors.Internal
{
    /// <summary>
    /// CSS selector matching engine. Parses the selector string, then walks the DOM
    /// tree testing each element against the parsed selector AST.
    /// </summary>
    /// <remarks>
    /// Stateless: parsed-selector memoization is owned by the caller's
    /// <see cref="SelectorParseCache"/> (the document the query runs against), passed in per
    /// call. A null cache parses without memoizing (e.g. an element detached from any document).
    /// </remarks>
    internal static class SelectorMatcher
    {
        internal static Element? QuerySelector(Node root, string selector, SelectorParseCache? cache)
        {
            var parsed = GetParsed(selector, cache);
            if (parsed.Count == 0) return null;

            return FindFirst(root, parsed);
        }

        internal static List<Element> QuerySelectorAll(Node root, string selector, SelectorParseCache? cache)
        {
            var parsed = GetParsed(selector, cache);
            var results = new List<Element>();
            if (parsed.Count == 0) return results;

            FindAll(root, parsed, results);
            return results;
        }

        /// <summary>
        /// Test if an element matches a selector string.
        /// </summary>
        internal static bool Matches(Element element, string selector, SelectorParseCache? cache)
        {
            var parsed = GetParsed(selector, cache);
            return MatchesAny(element, parsed);
        }

        private static List<ComplexSelector> GetParsed(string selector, SelectorParseCache? cache)
        {
            if (cache != null)
            {
                return cache.GetParsed(selector);
            }
            return SelectorParser.Parse(selector);
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
