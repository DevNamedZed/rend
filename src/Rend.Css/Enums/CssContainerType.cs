namespace Rend.Css
{
    /// <summary>
    /// CSS container-type property values.
    /// Defines whether an element is a query container for container queries.
    /// </summary>
    public enum CssContainerType : int
    {
        /// <summary>Not a query container.</summary>
        Normal = 0,
        /// <summary>Query container for inline and block dimensions.</summary>
        Size = 1,
        /// <summary>Query container for inline dimension only.</summary>
        InlineSize = 2
    }
}
