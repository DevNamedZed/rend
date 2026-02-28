using System;
using System.Collections;
using System.Collections.Generic;

namespace Rend.Html
{
    /// <summary>
    /// A space-separated token list backed by an element's attribute (e.g., classList from "class").
    /// </summary>
    public readonly struct TokenList : IEnumerable<string>
    {
        private readonly Element _element;
        private readonly string _attributeName;

        internal TokenList(Element element, string attributeName)
        {
            _element = element;
            _attributeName = attributeName;
        }

        private string RawValue => _element.GetAttribute(_attributeName) ?? string.Empty;

        /// <summary>Number of tokens.</summary>
        public int Count
        {
            get
            {
                var raw = RawValue;
                if (string.IsNullOrWhiteSpace(raw)) return 0;
                int count = 0;
                bool inToken = false;
                for (int i = 0; i < raw.Length; i++)
                {
                    if (char.IsWhiteSpace(raw[i]))
                    {
                        inToken = false;
                    }
                    else if (!inToken)
                    {
                        inToken = true;
                        count++;
                    }
                }
                return count;
            }
        }

        /// <summary>Returns true if the token list contains the given token.</summary>
        public bool Contains(string token)
        {
            var raw = RawValue;
            if (string.IsNullOrEmpty(raw) || string.IsNullOrEmpty(token)) return false;

            int start = 0;
            while (start < raw.Length)
            {
                // Skip whitespace
                while (start < raw.Length && char.IsWhiteSpace(raw[start])) start++;
                if (start >= raw.Length) break;

                // Find end of token
                int end = start;
                while (end < raw.Length && !char.IsWhiteSpace(raw[end])) end++;

                if (end - start == token.Length &&
                    string.Compare(raw, start, token, 0, token.Length, StringComparison.Ordinal) == 0)
                    return true;

                start = end;
            }
            return false;
        }

        /// <summary>Adds a token if not already present.</summary>
        public void Add(string token)
        {
            if (string.IsNullOrEmpty(token)) return;
            if (Contains(token)) return;

            var raw = RawValue;
            _element.SetAttribute(_attributeName,
                string.IsNullOrEmpty(raw) ? token : raw + " " + token);
        }

        /// <summary>Removes a token if present.</summary>
        public void Remove(string token)
        {
            if (string.IsNullOrEmpty(token)) return;
            if (!Contains(token)) return;

            var tokens = GetTokens();
            tokens.Remove(token);
            _element.SetAttribute(_attributeName, string.Join(" ", tokens));
        }

        /// <summary>Toggles a token: adds if absent, removes if present. Returns true if now present.</summary>
        public bool Toggle(string token)
        {
            if (Contains(token))
            {
                Remove(token);
                return false;
            }
            else
            {
                Add(token);
                return true;
            }
        }

        /// <summary>Gets the token at the given index.</summary>
        public string? this[int index]
        {
            get
            {
                var raw = RawValue;
                if (string.IsNullOrEmpty(raw)) return null;

                int current = 0;
                int start = 0;
                while (start < raw.Length)
                {
                    while (start < raw.Length && char.IsWhiteSpace(raw[start])) start++;
                    if (start >= raw.Length) break;

                    int end = start;
                    while (end < raw.Length && !char.IsWhiteSpace(raw[end])) end++;

                    if (current == index)
                        return raw.Substring(start, end - start);

                    current++;
                    start = end;
                }
                return null;
            }
        }

        private List<string> GetTokens()
        {
            var result = new List<string>();
            var raw = RawValue;
            if (string.IsNullOrEmpty(raw)) return result;

            int start = 0;
            while (start < raw.Length)
            {
                while (start < raw.Length && char.IsWhiteSpace(raw[start])) start++;
                if (start >= raw.Length) break;

                int end = start;
                while (end < raw.Length && !char.IsWhiteSpace(raw[end])) end++;

                result.Add(raw.Substring(start, end - start));
                start = end;
            }
            return result;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return GetTokens().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
