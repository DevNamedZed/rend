namespace Rend.Html.Parser.Internal
{
    /// <summary>
    /// A token emitted by the HTML tokenizer.
    /// Reused by the tokenizer — the tree builder must consume/copy data before the next token.
    /// </summary>
    internal struct HtmlToken
    {
        public HtmlTokenType Type;

        // For StartTag / EndTag
        public string? TagName;
        public bool SelfClosing;

        // For Character
        public char Character;

        // For Comment
        public string? Data;

        // For Doctype
        public string? DoctypeName;
        public string? PublicIdentifier;
        public string? SystemIdentifier;
        public bool ForceQuirks;

        // Attributes (for StartTag only, managed by HtmlAttributeBuffer)
        public int AttributeCount;

        public void Reset()
        {
            Type = default;
            TagName = null;
            SelfClosing = false;
            Character = '\0';
            Data = null;
            DoctypeName = null;
            PublicIdentifier = null;
            SystemIdentifier = null;
            ForceQuirks = false;
            AttributeCount = 0;
        }
    }
}
