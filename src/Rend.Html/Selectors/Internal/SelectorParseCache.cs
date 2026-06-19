using System.Collections.Generic;

namespace Rend.Html.Selectors.Internal
{
    /// <summary>
    /// Memoizes parsed selector ASTs keyed by selector string, so repeated
    /// <c>querySelector</c>/<c>matches</c> calls within a document tree do not re-parse
    /// the same selector. Owned by the <see cref="Document"/> that the queries run against,
    /// so the cache lives and dies with its document.
    /// </summary>
    internal sealed class SelectorParseCache
    {
        private const int MaxEntries = 1024;

        private readonly Dictionary<string, List<ComplexSelector>> _entries =
            new Dictionary<string, List<ComplexSelector>>();
        private readonly object _lock = new object();

        /// <summary>
        /// Returns the parsed form of the selector, parsing and caching it on first use.
        /// Once the cache is full, further distinct selectors are parsed but not stored,
        /// so the cache size stays bounded.
        /// </summary>
        public List<ComplexSelector> GetParsed(string selector)
        {
            lock (_lock)
            {
                if (_entries.TryGetValue(selector, out var cached))
                {
                    return cached;
                }

                var parsed = SelectorParser.Parse(selector);
                if (_entries.Count < MaxEntries)
                {
                    _entries[selector] = parsed;
                }
                return parsed;
            }
        }
    }
}
