using System.Collections;
using System.Collections.Generic;

namespace Rend.Html
{
    /// <summary>
    /// A read-only view over an element's attribute array.
    /// </summary>
    public readonly struct AttributeList : IEnumerable<HtmlAttribute>
    {
        private readonly HtmlAttribute[] _items;
        private readonly int _count;

        internal AttributeList(HtmlAttribute[] items, int count)
        {
            _items = items;
            _count = count;
        }

        /// <summary>Number of attributes.</summary>
        public int Count => _count;

        /// <summary>Gets the attribute at the given index.</summary>
        public HtmlAttribute this[int index] => _items[index];

        public Enumerator GetEnumerator() => new Enumerator(_items, _count);

        IEnumerator<HtmlAttribute> IEnumerable<HtmlAttribute>.GetEnumerator() => new Enumerator(_items, _count);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(_items, _count);

        public struct Enumerator : IEnumerator<HtmlAttribute>
        {
            private readonly HtmlAttribute[] _items;
            private readonly int _count;
            private int _index;

            internal Enumerator(HtmlAttribute[] items, int count)
            {
                _items = items;
                _count = count;
                _index = -1;
            }

            public HtmlAttribute Current => _items[_index];

            object IEnumerator.Current => _items[_index];

            public bool MoveNext() => ++_index < _count;

            public void Reset() => _index = -1;

            public void Dispose() { }
        }
    }
}
