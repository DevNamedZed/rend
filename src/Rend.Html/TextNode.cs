using System.Text;

namespace Rend.Html
{
    /// <summary>
    /// Represents a text node in the DOM tree.
    /// </summary>
    public sealed class TextNode : Node
    {
        private string _data;
        private StringBuilder? _builder;

        public override NodeType NodeType => NodeType.Text;

        /// <summary>The text data of this node.</summary>
        public string Data
        {
            get
            {
                if (_builder != null)
                {
                    _data = _builder.ToString();
                    _builder = null;
                }
                return _data;
            }
            set
            {
                _data = value ?? string.Empty;
                _builder = null;
            }
        }

        public override string TextContent
        {
            get => Data;
            set => Data = value ?? string.Empty;
        }

        internal TextNode(string data, Document? ownerDocument)
        {
            _data = data ?? string.Empty;
            OwnerDocument = ownerDocument;
        }

        internal void AppendData(char character)
        {
            if (_builder == null)
            {
                _builder = new StringBuilder(_data.Length + 16);
                if (_data.Length > 0)
                {
                    _builder.Append(_data);
                }
                _data = string.Empty;
            }
            _builder.Append(character);
        }

        public override Node CloneNode(bool deep = false)
        {
            return new TextNode(Data, OwnerDocument);
        }

        public override string ToString()
        {
            string data = Data;
            return $"#text \"{(data.Length > 40 ? data.Substring(0, 40) + "..." : data)}\"";
        }
    }
}
