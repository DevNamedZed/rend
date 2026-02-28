namespace Rend.Html
{
    /// <summary>
    /// Represents a single HTML attribute (name-value pair).
    /// Names are interned via StringPool for reference-equality comparison.
    /// </summary>
    public readonly struct HtmlAttribute
    {
        /// <summary>The attribute name (interned).</summary>
        public readonly string Name;

        /// <summary>The attribute value.</summary>
        public readonly string Value;

        public HtmlAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString() => string.IsNullOrEmpty(Value) ? Name : $"{Name}=\"{Value}\"";
    }
}
