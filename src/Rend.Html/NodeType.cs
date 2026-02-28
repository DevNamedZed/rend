namespace Rend.Html
{
    /// <summary>
    /// The type of a DOM node, matching the WHATWG DOM specification node type constants.
    /// </summary>
    public enum NodeType
    {
        Element = 1,
        Text = 3,
        Comment = 8,
        Document = 9,
        DocumentType = 10,
        DocumentFragment = 11
    }
}
