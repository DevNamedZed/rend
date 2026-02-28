namespace Rend.Html
{
    /// <summary>
    /// Represents a text node in the DOM tree.
    /// </summary>
    public sealed class TextNode : Node
    {
        public override NodeType NodeType => NodeType.Text;

        /// <summary>The text data of this node.</summary>
        public string Data { get; set; }

        public override string TextContent
        {
            get => Data;
            set => Data = value ?? string.Empty;
        }

        internal TextNode(string data, Document? ownerDocument)
        {
            Data = data;
            OwnerDocument = ownerDocument;
        }

        public override Node CloneNode(bool deep = false)
        {
            return new TextNode(Data, OwnerDocument);
        }

        public override string ToString() => $"#text \"{(Data.Length > 40 ? Data.Substring(0, 40) + "..." : Data)}\"";
    }
}
