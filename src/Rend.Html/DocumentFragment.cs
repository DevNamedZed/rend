namespace Rend.Html
{
    /// <summary>
    /// A lightweight document fragment container.
    /// When appended to a node, its children are transferred instead of the fragment itself.
    /// </summary>
    public sealed class DocumentFragment : Node
    {
        public override NodeType NodeType => NodeType.DocumentFragment;

        internal DocumentFragment(Document? ownerDocument)
        {
            OwnerDocument = ownerDocument;
        }

        public override Node CloneNode(bool deep = false)
        {
            var clone = new DocumentFragment(OwnerDocument);
            if (deep)
                CloneChildrenInto(clone);
            return clone;
        }

        public override string ToString() => "#document-fragment";
    }
}
