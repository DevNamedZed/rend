using System.Collections.Generic;
using Rend.Html.Selectors.Internal;

namespace Rend.Html.Selectors
{
    /// <summary>
    /// A parsed CSS selector list. Parse once, match many times.
    /// </summary>
    public sealed class SelectorList
    {
        private readonly List<ComplexSelector> _selectors;

        private SelectorList(List<ComplexSelector> selectors)
        {
            _selectors = selectors;
        }

        /// <summary>Parse a CSS selector string into a SelectorList.</summary>
        public static SelectorList Parse(string selector)
        {
            var parsed = SelectorParser.Parse(selector);
            return new SelectorList(parsed);
        }

        /// <summary>Returns true if the element matches this selector.</summary>
        public bool Matches(Element element)
        {
            for (int i = 0; i < _selectors.Count; i++)
            {
                if (_selectors[i].Matches(element))
                    return true;
            }
            return false;
        }

        /// <summary>Find the first descendant element matching this selector.</summary>
        public Element? QuerySelector(Node root)
        {
            return FindFirst(root);
        }

        /// <summary>Find all descendant elements matching this selector.</summary>
        public List<Element> QuerySelectorAll(Node root)
        {
            var results = new List<Element>();
            FindAll(root, results);
            return results;
        }

        private Element? FindFirst(Node root)
        {
            var child = root.FirstChild;
            while (child != null)
            {
                if (child is Element el)
                {
                    if (Matches(el)) return el;
                    var found = FindFirst(el);
                    if (found != null) return found;
                }
                child = child.NextSibling;
            }
            return null;
        }

        private void FindAll(Node root, List<Element> results)
        {
            var child = root.FirstChild;
            while (child != null)
            {
                if (child is Element el)
                {
                    if (Matches(el)) results.Add(el);
                    FindAll(el, results);
                }
                child = child.NextSibling;
            }
        }
    }
}
