using System.Collections.Generic;
using Rend.Css;
using Rend.Css.Properties.Internal;

namespace Rend.Style.Internal
{
    /// <summary>
    /// Tracks CSS counter state during style tree building.
    /// Handles counter-reset, counter-increment, counter-set, and counter() value resolution.
    /// </summary>
    internal sealed class CounterTracker
    {
        // Stack of counter scopes. Each scope is a dictionary of counter name → value.
        // When counter-reset creates a counter, it pushes onto the scope stack.
        private readonly Stack<Dictionary<string, int>> _scopes = new Stack<Dictionary<string, int>>();
        private readonly Dictionary<string, int> _globalCounters = new Dictionary<string, int>();

        // Quote nesting level
        private int _quoteDepth;

        // Default quote pairs: "\u201c" "\u201d" "\u2018" "\u2019"
        private static readonly string[] DefaultQuotes = { "\u201c", "\u201d", "\u2018", "\u2019" };

        public CounterTracker()
        {
            _scopes.Push(_globalCounters);
        }

        /// <summary>
        /// Process counter-reset declarations from the element's computed style.
        /// Parses "name [value] ..." format.
        /// </summary>
        public void ProcessCounterReset(ComputedStyle style)
        {
            var raw = style.GetRefValue(PropertyId.CounterReset);
            if (raw == null) return;

            var entries = ParseCounterEntries(raw);
            if (entries == null) return;

            foreach (var (name, value) in entries)
            {
                // Reset in the innermost scope
                var scope = _scopes.Peek();
                scope[name] = value;
            }
        }

        /// <summary>
        /// Process counter-increment declarations from the element's computed style.
        /// Parses "name [value] ..." format. Default increment is 1.
        /// </summary>
        public void ProcessCounterIncrement(ComputedStyle style)
        {
            var raw = style.GetRefValue(PropertyId.CounterIncrement);
            if (raw == null) return;

            var entries = ParseCounterEntries(raw, defaultValue: 1);
            if (entries == null) return;

            foreach (var (name, value) in entries)
            {
                // Find the counter in any scope (innermost first)
                bool found = false;
                foreach (var scope in _scopes)
                {
                    if (scope.ContainsKey(name))
                    {
                        scope[name] += value;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // If counter doesn't exist, create it implicitly
                    var currentScope = _scopes.Peek();
                    currentScope[name] = value;
                }
            }
        }

        /// <summary>
        /// Process counter-set declarations from the element's computed style.
        /// Sets counter to a specific value without creating a new scope.
        /// Parses "name [value] ..." format. Default value is 0.
        /// </summary>
        public void ProcessCounterSet(ComputedStyle style)
        {
            var raw = style.GetRefValue(PropertyId.CounterSet);
            if (raw == null) return;

            var entries = ParseCounterEntries(raw);
            if (entries == null) return;

            foreach (var (name, value) in entries)
            {
                // Set the counter in the innermost scope that has it, or create one
                bool found = false;
                foreach (var scope in _scopes)
                {
                    if (scope.ContainsKey(name))
                    {
                        scope[name] = value;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    var currentScope = _scopes.Peek();
                    currentScope[name] = value;
                }
            }
        }

        /// <summary>
        /// Get the current value of a counter.
        /// </summary>
        public int GetCounterValue(string name)
        {
            foreach (var scope in _scopes)
            {
                if (scope.TryGetValue(name, out int value))
                    return value;
            }
            return 0; // default when counter doesn't exist
        }

        /// <summary>
        /// Format a counter value using the specified list-style-type.
        /// </summary>
        public string FormatCounter(string name, string? style = null)
        {
            int value = GetCounterValue(name);
            return FormatValue(value, style ?? "decimal");
        }

        /// <summary>
        /// Format all counter values for the given name across all scopes,
        /// concatenated with the specified separator (CSS counters() function).
        /// </summary>
        public string FormatCounters(string name, string separator, string? style = null)
        {
            string listStyle = style ?? "decimal";
            var values = new List<string>();
            // Collect from outermost to innermost scope
            var scopeArray = new Dictionary<string, int>[_scopes.Count];
            _scopes.CopyTo(scopeArray, 0);
            // Stack copies in LIFO order, so reverse to get outermost first
            for (int i = scopeArray.Length - 1; i >= 0; i--)
            {
                if (scopeArray[i].TryGetValue(name, out int val))
                    values.Add(FormatValue(val, listStyle));
            }
            return values.Count > 0 ? string.Join(separator, values) : "0";
        }

        /// <summary>
        /// Push a new counter scope (e.g., when entering an element with counter-reset).
        /// </summary>
        public void PushScope()
        {
            _scopes.Push(new Dictionary<string, int>());
        }

        /// <summary>
        /// Pop the current counter scope.
        /// </summary>
        public void PopScope()
        {
            if (_scopes.Count > 1)
                _scopes.Pop();
        }

        private static string FormatValue(int value, string style)
        {
            switch (style)
            {
                case "decimal":
                    return value.ToString();
                case "decimal-leading-zero":
                    return value < 10 && value >= 0 ? "0" + value : value.ToString();
                case "lower-alpha":
                case "lower-latin":
                    return value >= 1 && value <= 26 ? ((char)('a' + value - 1)).ToString() : value.ToString();
                case "upper-alpha":
                case "upper-latin":
                    return value >= 1 && value <= 26 ? ((char)('A' + value - 1)).ToString() : value.ToString();
                case "lower-roman":
                    return ToRoman(value, false);
                case "upper-roman":
                    return ToRoman(value, true);
                default:
                    return value.ToString();
            }
        }

        private static string ToRoman(int num, bool upper)
        {
            if (num <= 0 || num > 3999) return num.ToString();
            string[] thousands = upper ? new[] { "", "M", "MM", "MMM" } : new[] { "", "m", "mm", "mmm" };
            string[] hundreds = upper
                ? new[] { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" }
                : new[] { "", "c", "cc", "ccc", "cd", "d", "dc", "dcc", "dccc", "cm" };
            string[] tens = upper
                ? new[] { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" }
                : new[] { "", "x", "xx", "xxx", "xl", "l", "lx", "lxx", "lxxx", "xc" };
            string[] ones = upper
                ? new[] { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" }
                : new[] { "", "i", "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix" };
            return thousands[num / 1000] + hundreds[num % 1000 / 100] + tens[num % 100 / 10] + ones[num % 10];
        }

        /// <summary>
        /// Parses counter entries from a CSS value (e.g., "section 0" or "section chapter 3").
        /// Returns null for "none" keyword.
        /// </summary>
        private static List<(string name, int value)>? ParseCounterEntries(object raw, int defaultValue = 0)
        {
            if (raw is CssKeywordValue kw && kw.Keyword == "none")
                return null;

            var entries = new List<(string name, int value)>();

            if (raw is CssKeywordValue nameKw)
            {
                entries.Add((nameKw.Keyword, defaultValue));
                return entries;
            }

            if (raw is CssListValue list)
            {
                int i = 0;
                while (i < list.Values.Count)
                {
                    string? name = null;
                    if (list.Values[i] is CssKeywordValue k)
                        name = k.Keyword;
                    else
                        name = list.Values[i].ToString();

                    if (name == null || name == "none") { i++; continue; }

                    int val = defaultValue;
                    // Check if next token is a number
                    if (i + 1 < list.Values.Count && list.Values[i + 1] is CssNumberValue num)
                    {
                        val = (int)num.Value;
                        i += 2;
                    }
                    else
                    {
                        i++;
                    }

                    entries.Add((name, val));
                }
                return entries;
            }

            // Single ident
            string? singleName = raw.ToString();
            if (singleName != null && singleName != "none")
                entries.Add((singleName, defaultValue));

            return entries.Count > 0 ? entries : null;
        }

        /// <summary>
        /// Get the open-quote string at the current nesting depth, then increment depth.
        /// </summary>
        public string GetOpenQuote(ComputedStyle style)
        {
            var quotes = GetQuotePairs(style);
            int pairIdx = System.Math.Min(_quoteDepth, quotes.Length / 2 - 1);
            if (pairIdx < 0) pairIdx = 0;
            _quoteDepth++;
            return pairIdx * 2 < quotes.Length ? quotes[pairIdx * 2] : "\u201c";
        }

        /// <summary>
        /// Decrement depth, then get the close-quote string at the current nesting depth.
        /// </summary>
        public string GetCloseQuote(ComputedStyle style)
        {
            if (_quoteDepth > 0) _quoteDepth--;
            var quotes = GetQuotePairs(style);
            int pairIdx = System.Math.Min(_quoteDepth, quotes.Length / 2 - 1);
            if (pairIdx < 0) pairIdx = 0;
            return pairIdx * 2 + 1 < quotes.Length ? quotes[pairIdx * 2 + 1] : "\u201d";
        }

        private static string[] GetQuotePairs(ComputedStyle style)
        {
            var raw = style.GetRefValue(PropertyId.Quotes);
            if (raw == null) return DefaultQuotes;
            if (raw is CssKeywordValue kw && (kw.Keyword == "auto" || kw.Keyword == "none"))
                return DefaultQuotes;

            // Parse pairs from CssListValue: "open1" "close1" "open2" "close2" ...
            if (raw is CssListValue list && list.Values.Count >= 2)
            {
                var pairs = new string[list.Values.Count];
                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (list.Values[i] is CssStringValue sv)
                        pairs[i] = sv.Value;
                    else
                        pairs[i] = list.Values[i].ToString() ?? "";
                }
                return pairs;
            }

            return DefaultQuotes;
        }
    }
}
