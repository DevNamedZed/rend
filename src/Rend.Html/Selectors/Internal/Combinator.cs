namespace Rend.Html.Selectors.Internal
{
    internal enum Combinator : byte
    {
        /// <summary>Descendant combinator (whitespace): A B</summary>
        Descendant,
        /// <summary>Child combinator: A > B</summary>
        Child,
        /// <summary>Next sibling combinator: A + B</summary>
        NextSibling,
        /// <summary>Subsequent sibling combinator: A ~ B</summary>
        SubsequentSibling
    }
}
