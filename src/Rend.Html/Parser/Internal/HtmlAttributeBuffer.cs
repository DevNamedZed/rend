using System;
using System.Text;
using Rend.Core;

namespace Rend.Html.Parser.Internal
{
    /// <summary>
    /// Pooled scratch buffer for accumulating attributes during tokenization.
    /// Reused per token — the tree builder copies attributes into permanent Element storage.
    /// </summary>
    internal sealed class HtmlAttributeBuffer
    {
        private HtmlAttribute[] _attrs;
        private int _count;

        // Scratch buffers for the current attribute being built
        private readonly StringBuilder _nameBuffer = new StringBuilder(32);
        private readonly StringBuilder _valueBuffer = new StringBuilder(128);

        public HtmlAttributeBuffer(int initialCapacity = 8)
        {
            _attrs = new HtmlAttribute[initialCapacity];
            _count = 0;
        }

        public int Count => _count;

        public HtmlAttribute[] Items => _attrs;

        /// <summary>Start building a new attribute. Resets name/value buffers.</summary>
        public void StartAttribute()
        {
            _nameBuffer.Length = 0;
            _valueBuffer.Length = 0;
        }

        /// <summary>Append a character to the current attribute name.</summary>
        public void AppendName(char c) => _nameBuffer.Append(c);

        /// <summary>Append a character to the current attribute value.</summary>
        public void AppendValue(char c) => _valueBuffer.Append(c);

        /// <summary>Append a string to the current attribute value.</summary>
        public void AppendValue(string s) => _valueBuffer.Append(s);

        /// <summary>
        /// Finish the current attribute. Interns the name and adds to the buffer.
        /// Skips duplicate attribute names (per WHATWG spec).
        /// </summary>
        public void FinishAttribute()
        {
            if (_nameBuffer.Length == 0) return;

            var name = StringPool.HtmlNames.Intern(_nameBuffer.ToString());

            // Check for duplicate attribute names — first wins per spec
            for (int i = 0; i < _count; i++)
            {
                if (ReferenceEquals(_attrs[i].Name, name))
                    return; // Duplicate, skip
            }

            if (_count == _attrs.Length)
            {
                var newArr = new HtmlAttribute[_attrs.Length * 2];
                Array.Copy(_attrs, newArr, _count);
                _attrs = newArr;
            }

            _attrs[_count++] = new HtmlAttribute(name, _valueBuffer.ToString());
        }

        /// <summary>Reset for the next token.</summary>
        public void Clear()
        {
            _count = 0;
            _nameBuffer.Length = 0;
            _valueBuffer.Length = 0;
        }

        /// <summary>Copy attributes into an Element.</summary>
        public void CopyTo(Element element)
        {
            if (_count > 0)
                element.SetAttributes(_attrs, _count);
        }
    }
}
