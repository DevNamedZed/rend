namespace Rend.Html
{
    /// <summary>
    /// Represents a DOCTYPE node.
    /// </summary>
    public sealed class DocumentType : Node
    {
        public override NodeType NodeType => NodeType.DocumentType;

        /// <summary>The DOCTYPE name (e.g., "html").</summary>
        public string Name { get; }

        /// <summary>The public identifier, or empty string.</summary>
        public string PublicId { get; }

        /// <summary>The system identifier, or empty string.</summary>
        public string SystemId { get; }

        public override string TextContent
        {
            get => string.Empty;
            set { } // No-op for DocumentType
        }

        internal DocumentType(string name, string publicId, string systemId, Document? ownerDocument)
        {
            Name = name;
            PublicId = publicId;
            SystemId = systemId;
            OwnerDocument = ownerDocument;
        }

        public override Node CloneNode(bool deep = false)
        {
            return new DocumentType(Name, PublicId, SystemId, OwnerDocument);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(PublicId) && string.IsNullOrEmpty(SystemId))
                return $"<!DOCTYPE {Name}>";
            return $"<!DOCTYPE {Name} \"{PublicId}\" \"{SystemId}\">";
        }
    }
}
