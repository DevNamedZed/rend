namespace Rend.Css
{
    /// <summary>
    /// A single CSS declaration: property name, parsed value, and importance flag.
    /// </summary>
    public sealed class CssDeclaration
    {
        /// <summary>The property name (lowercase, interned).</summary>
        public string Property { get; }

        /// <summary>The parsed value.</summary>
        public CssValue Value { get; }

        /// <summary>Whether this declaration has !important.</summary>
        public bool Important { get; }

        public CssDeclaration(string property, CssValue value, bool important = false)
        {
            Property = property;
            Value = value;
            Important = important;
        }

        public override string ToString()
            => Important ? $"{Property}: {Value} !important" : $"{Property}: {Value}";
    }
}
