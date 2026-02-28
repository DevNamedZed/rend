using System.Collections.Generic;

namespace Rend.Html.Parser.Internal
{
    /// <summary>
    /// The list of active formatting elements, as defined by the WHATWG spec.
    /// Tracks formatting elements (b, i, em, strong, etc.) that need to be
    /// reopened when they're implicitly closed by block-level elements.
    /// Markers are inserted for scope boundaries (applet, marquee, object, etc.).
    /// </summary>
    internal sealed class ActiveFormattingElements
    {
        // Entries: Element or null (null = marker)
        private readonly List<Element?> _entries = new List<Element?>();

        public int Count => _entries.Count;

        public Element? this[int index] => _entries[index];

        /// <summary>Push a formatting element.</summary>
        public void Push(Element element)
        {
            // Noah's Ark clause: if there are already 3 entries with the same
            // tag name and attributes after the last marker, remove the earliest one.
            int matchCount = 0;
            int earliestMatch = -1;

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i] == null) break; // Hit a marker

                if (AreEqual(_entries[i]!, element))
                {
                    matchCount++;
                    earliestMatch = i;
                }
            }

            if (matchCount >= 3 && earliestMatch >= 0)
            {
                _entries.RemoveAt(earliestMatch);
            }

            _entries.Add(element);
        }

        /// <summary>Insert a scope marker (null entry).</summary>
        public void InsertMarker()
        {
            _entries.Add(null);
        }

        /// <summary>Remove an element from the list.</summary>
        public void Remove(Element element)
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_entries[i], element))
                {
                    _entries.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>Replace an element in the list.</summary>
        public void Replace(Element oldElement, Element newElement)
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_entries[i], oldElement))
                {
                    _entries[i] = newElement;
                    return;
                }
            }
        }

        /// <summary>Insert an element at a specific position.</summary>
        public void InsertAt(int index, Element element)
        {
            _entries.Insert(index, element);
        }

        /// <summary>Returns true if the element is in the list (not a marker).</summary>
        public bool Contains(Element element)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (ReferenceEquals(_entries[i], element))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Clear the list back to the last marker (inclusive).
        /// Used when leaving scope boundaries.
        /// </summary>
        public void ClearToLastMarker()
        {
            while (_entries.Count > 0)
            {
                var entry = _entries[_entries.Count - 1];
                _entries.RemoveAt(_entries.Count - 1);
                if (entry == null) break; // Was the marker
            }
        }

        /// <summary>
        /// Reconstruct the active formatting elements.
        /// Reopens formatting elements that were implicitly closed.
        /// Called before inserting character content or certain elements.
        /// </summary>
        public void Reconstruct(HtmlTreeBuilder treeBuilder)
        {
            if (_entries.Count == 0) return;

            var last = _entries[_entries.Count - 1];
            if (last == null) return; // Marker at end

            // Check if the last entry is on the stack — if so, nothing to reconstruct
            if (treeBuilder.IsInOpenElements(last))
                return;

            int i = _entries.Count - 1;

            // Walk backwards to find the first entry that's on the stack or is a marker
            while (i > 0)
            {
                i--;
                var entry = _entries[i];
                if (entry == null || treeBuilder.IsInOpenElements(entry))
                {
                    i++;
                    break;
                }
            }

            // Now walk forward, recreating each element
            for (; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (entry == null) continue;

                var newElement = treeBuilder.CreateAndInsertElement(entry.TagName, entry);
                _entries[i] = newElement;
            }
        }

        /// <summary>
        /// Find the element with the given tag name searching backwards from the end.
        /// Returns the index, or -1 if not found (stops at markers).
        /// </summary>
        public int FindLastWithTag(string tagName)
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i] == null) return -1; // Hit a marker
                if (ReferenceEquals(_entries[i]!.TagName, tagName))
                    return i;
            }
            return -1;
        }

        public void RemoveAt(int index) => _entries.RemoveAt(index);

        private static bool AreEqual(Element a, Element b)
        {
            if (!ReferenceEquals(a.TagName, b.TagName)) return false;

            var attrsA = a.Attributes;
            var attrsB = b.Attributes;
            if (attrsA.Count != attrsB.Count) return false;

            for (int i = 0; i < attrsA.Count; i++)
            {
                var attrA = attrsA[i];
                bool found = false;
                for (int j = 0; j < attrsB.Count; j++)
                {
                    if (ReferenceEquals(attrA.Name, attrsB[j].Name) && attrA.Value == attrsB[j].Value)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) return false;
            }

            return true;
        }
    }
}
