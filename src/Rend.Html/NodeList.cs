using System.Collections;
using System.Collections.Generic;

namespace Rend.Html
{
    /// <summary>
    /// A live list of child nodes backed by the linked-list structure.
    /// Enumerating walks FirstChild → NextSibling.
    /// </summary>
    public readonly struct NodeList : IEnumerable<Node>
    {
        private readonly Node _parent;

        internal NodeList(Node parent)
        {
            _parent = parent;
        }

        /// <summary>Number of child nodes. O(n) — walks the linked list.</summary>
        public int Count
        {
            get
            {
                int count = 0;
                var child = _parent.FirstChild;
                while (child != null)
                {
                    count++;
                    child = child.NextSibling;
                }
                return count;
            }
        }

        /// <summary>Gets the child at the given index. O(n).</summary>
        public Node? this[int index]
        {
            get
            {
                int i = 0;
                var child = _parent.FirstChild;
                while (child != null)
                {
                    if (i == index) return child;
                    i++;
                    child = child.NextSibling;
                }
                return null;
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(_parent.FirstChild);

        IEnumerator<Node> IEnumerable<Node>.GetEnumerator() => new Enumerator(_parent.FirstChild);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(_parent.FirstChild);

        public struct Enumerator : IEnumerator<Node>
        {
            private Node? _current;
            private Node? _next;

            internal Enumerator(Node? first)
            {
                _current = null;
                _next = first;
            }

            public Node Current => _current!;

            object IEnumerator.Current => _current!;

            public bool MoveNext()
            {
                if (_next == null) return false;
                _current = _next;
                _next = _current.NextSibling;
                return true;
            }

            public void Reset() { }

            public void Dispose() { }
        }
    }
}
