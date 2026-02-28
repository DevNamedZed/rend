using System;
using System.Collections.Generic;

namespace Rend.Html.Selectors.Internal
{
    /// <summary>
    /// A complex selector is a chain of compound selectors separated by combinators.
    /// E.g., "div > p.foo + span" → ComplexSelector with 3 entries.
    /// Matching is done right-to-left: match the rightmost compound first,
    /// then walk up/sideways based on the combinator.
    /// </summary>
    internal sealed class ComplexSelector : Selector
    {
        // Entries from left to right: [compound0, combinator0, compound1, combinator1, compound2]
        // The rightmost compound is the "subject" — the element being tested.
        private readonly List<CompoundSelector> _compounds;
        private readonly List<Combinator> _combinators;

        public ComplexSelector()
        {
            _compounds = new List<CompoundSelector>(4);
            _combinators = new List<Combinator>(3);
        }

        public void AddCompound(CompoundSelector compound)
        {
            _compounds.Add(compound);
        }

        public void AddCombinator(Combinator combinator)
        {
            _combinators.Add(combinator);
        }

        public override bool Matches(Element element)
        {
            if (_compounds.Count == 0) return false;

            // Match the rightmost (subject) compound
            int idx = _compounds.Count - 1;
            if (!_compounds[idx].Matches(element))
                return false;

            // Walk backwards through the chain
            Node? current = element;
            for (int i = idx - 1; i >= 0; i--)
            {
                var combinator = _combinators[i];
                var compound = _compounds[i];

                switch (combinator)
                {
                    case Combinator.Descendant:
                        current = ((Element)current!).Parent;
                        bool found = false;
                        while (current != null)
                        {
                            if (current is Element ancestor && compound.Matches(ancestor))
                            {
                                current = ancestor;
                                found = true;
                                break;
                            }
                            current = current.Parent;
                        }
                        if (!found) return false;
                        break;

                    case Combinator.Child:
                        current = ((Element)current!).Parent;
                        if (current == null || !(current is Element parent) || !compound.Matches(parent))
                            return false;
                        current = parent;
                        break;

                    case Combinator.NextSibling:
                        var prev = ((Element)current!).PreviousSibling;
                        while (prev != null && !(prev is Element))
                            prev = prev.PreviousSibling;
                        if (prev == null || !(prev is Element prevEl) || !compound.Matches(prevEl))
                            return false;
                        current = prevEl;
                        break;

                    case Combinator.SubsequentSibling:
                        var sib = ((Element)current!).PreviousSibling;
                        bool sibFound = false;
                        while (sib != null)
                        {
                            if (sib is Element sibEl && compound.Matches(sibEl))
                            {
                                current = sibEl;
                                sibFound = true;
                                break;
                            }
                            sib = sib.PreviousSibling;
                        }
                        if (!sibFound) return false;
                        break;
                }
            }

            return true;
        }

        public override Specificity GetSpecificity()
        {
            var result = new Specificity(0, 0, 0);
            for (int i = 0; i < _compounds.Count; i++)
                result = result + _compounds[i].GetSpecificity();
            return result;
        }
    }
}
