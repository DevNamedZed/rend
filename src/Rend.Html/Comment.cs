namespace Rend.Html
{
    /// <summary>
    /// Represents an HTML comment node.
    /// </summary>
    public sealed class Comment : Node
    {
        public override NodeType NodeType => NodeType.Comment;

        /// <summary>The comment text (without the delimiters).</summary>
        public string Data { get; set; }

        public override string TextContent
        {
            get => Data;
            set => Data = value ?? string.Empty;
        }

        internal Comment(string data, Document? ownerDocument)
        {
            Data = data;
            OwnerDocument = ownerDocument;
        }

        public override Node CloneNode(bool deep = false)
        {
            return new Comment(Data, OwnerDocument);
        }

        public override string ToString() => $"<!--{(Data.Length > 40 ? Data.Substring(0, 40) + "..." : Data)}-->";
    }
}
