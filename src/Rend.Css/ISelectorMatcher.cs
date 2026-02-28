namespace Rend.Css
{
    /// <summary>
    /// Abstraction for matching CSS selectors against elements.
    /// Decouples Rend.Css from Rend.Html's selector engine.
    /// The orchestrator provides an adapter that bridges Rend.Html.Selectors.
    /// </summary>
    public interface ISelectorMatcher
    {
        /// <summary>
        /// Returns true if the element matches the given selector string.
        /// </summary>
        bool Matches(IStylableElement element, string selectorText);

        /// <summary>
        /// Compute the specificity of a selector string.
        /// </summary>
        CssSpecificity GetSpecificity(string selectorText);
    }
}
