using System;

namespace Rend.Html.Selectors.Internal
{
    /// <summary>
    /// Base class for all selector types.
    /// </summary>
    internal abstract class Selector
    {
        public abstract bool Matches(Element element);
        public abstract Specificity GetSpecificity();
    }
}
