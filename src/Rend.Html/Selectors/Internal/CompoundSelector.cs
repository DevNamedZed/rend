using System.Collections.Generic;

namespace Rend.Html.Selectors.Internal
{
    /// <summary>
    /// A compound selector is a sequence of simple selectors that all must match
    /// the same element. E.g., "div.foo#bar" → TypeSelector(div) + ClassSelector(foo) + IdSelector(bar)
    /// </summary>
    internal sealed class CompoundSelector : Selector
    {
        private readonly List<Selector> _selectors;

        public CompoundSelector()
        {
            _selectors = new List<Selector>(4);
        }

        public void Add(Selector selector) => _selectors.Add(selector);

        public int Count => _selectors.Count;

        public override bool Matches(Element element)
        {
            for (int i = 0; i < _selectors.Count; i++)
            {
                if (!_selectors[i].Matches(element))
                    return false;
            }
            return true;
        }

        public override Specificity GetSpecificity()
        {
            var result = new Specificity(0, 0, 0);
            for (int i = 0; i < _selectors.Count; i++)
                result = result + _selectors[i].GetSpecificity();
            return result;
        }
    }
}
