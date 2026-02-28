using System;

namespace Rend.Html.Selectors.Internal
{
    /// <summary>
    /// Pseudo-class selectors: :first-child, :last-child, :nth-child(An+B),
    /// :nth-of-type, :not(), :is(), :where(), :has(), :empty, :root, etc.
    /// </summary>
    internal sealed class PseudoClassSelector : Selector
    {
        private readonly PseudoType _type;
        private readonly int _nthA;
        private readonly int _nthB;
        private readonly Selector? _argument; // For :not(), :is(), :where(), :has()

        private PseudoClassSelector(PseudoType type, int nthA = 0, int nthB = 0, Selector? argument = null)
        {
            _type = type;
            _nthA = nthA;
            _nthB = nthB;
            _argument = argument;
        }

        public static PseudoClassSelector FirstChild() => new PseudoClassSelector(PseudoType.FirstChild);
        public static PseudoClassSelector LastChild() => new PseudoClassSelector(PseudoType.LastChild);
        public static PseudoClassSelector FirstOfType() => new PseudoClassSelector(PseudoType.FirstOfType);
        public static PseudoClassSelector LastOfType() => new PseudoClassSelector(PseudoType.LastOfType);
        public static PseudoClassSelector OnlyChild() => new PseudoClassSelector(PseudoType.OnlyChild);
        public static PseudoClassSelector OnlyOfType() => new PseudoClassSelector(PseudoType.OnlyOfType);
        public static PseudoClassSelector Empty() => new PseudoClassSelector(PseudoType.Empty);
        public static PseudoClassSelector Root() => new PseudoClassSelector(PseudoType.Root);

        public static PseudoClassSelector NthChild(int a, int b) =>
            new PseudoClassSelector(PseudoType.NthChild, a, b);

        public static PseudoClassSelector NthLastChild(int a, int b) =>
            new PseudoClassSelector(PseudoType.NthLastChild, a, b);

        public static PseudoClassSelector NthOfType(int a, int b) =>
            new PseudoClassSelector(PseudoType.NthOfType, a, b);

        public static PseudoClassSelector NthLastOfType(int a, int b) =>
            new PseudoClassSelector(PseudoType.NthLastOfType, a, b);

        public static PseudoClassSelector Not(Selector arg) =>
            new PseudoClassSelector(PseudoType.Not, argument: arg);

        public static PseudoClassSelector Is(Selector arg) =>
            new PseudoClassSelector(PseudoType.Is, argument: arg);

        public static PseudoClassSelector Where(Selector arg) =>
            new PseudoClassSelector(PseudoType.Where, argument: arg);

        public static PseudoClassSelector Has(Selector arg) =>
            new PseudoClassSelector(PseudoType.Has, argument: arg);

        public override bool Matches(Element element)
        {
            switch (_type)
            {
                case PseudoType.FirstChild:
                    return IsFirstChild(element);

                case PseudoType.LastChild:
                    return IsLastChild(element);

                case PseudoType.FirstOfType:
                    return IsFirstOfType(element);

                case PseudoType.LastOfType:
                    return IsLastOfType(element);

                case PseudoType.OnlyChild:
                    return IsFirstChild(element) && IsLastChild(element);

                case PseudoType.OnlyOfType:
                    return IsFirstOfType(element) && IsLastOfType(element);

                case PseudoType.Empty:
                    return element.FirstChild == null;

                case PseudoType.Root:
                    return element.Parent is Document;

                case PseudoType.NthChild:
                    return NthParser.Matches(_nthA, _nthB, GetChildIndex(element));

                case PseudoType.NthLastChild:
                    return NthParser.Matches(_nthA, _nthB, GetChildIndexFromEnd(element));

                case PseudoType.NthOfType:
                    return NthParser.Matches(_nthA, _nthB, GetTypeIndex(element));

                case PseudoType.NthLastOfType:
                    return NthParser.Matches(_nthA, _nthB, GetTypeIndexFromEnd(element));

                case PseudoType.Not:
                    return _argument != null && !_argument.Matches(element);

                case PseudoType.Is:
                case PseudoType.Where:
                    return _argument != null && _argument.Matches(element);

                case PseudoType.Has:
                    return _argument != null && HasDescendantMatch(element, _argument);

                default:
                    return false;
            }
        }

        public override Specificity GetSpecificity()
        {
            switch (_type)
            {
                case PseudoType.Where:
                    return new Specificity(0, 0, 0); // :where() has zero specificity

                case PseudoType.Not:
                case PseudoType.Is:
                case PseudoType.Has:
                    return _argument?.GetSpecificity() ?? new Specificity(0, 0, 0);

                default:
                    return new Specificity(0, 1, 0);
            }
        }

        private static bool IsFirstChild(Element el)
        {
            var sibling = el.PreviousSibling;
            while (sibling != null)
            {
                if (sibling is Element) return false;
                sibling = sibling.PreviousSibling;
            }
            return true;
        }

        private static bool IsLastChild(Element el)
        {
            var sibling = el.NextSibling;
            while (sibling != null)
            {
                if (sibling is Element) return false;
                sibling = sibling.NextSibling;
            }
            return true;
        }

        private static bool IsFirstOfType(Element el)
        {
            var sibling = el.PreviousSibling;
            while (sibling != null)
            {
                if (sibling is Element sibEl && ReferenceEquals(sibEl.TagName, el.TagName))
                    return false;
                sibling = sibling.PreviousSibling;
            }
            return true;
        }

        private static bool IsLastOfType(Element el)
        {
            var sibling = el.NextSibling;
            while (sibling != null)
            {
                if (sibling is Element sibEl && ReferenceEquals(sibEl.TagName, el.TagName))
                    return false;
                sibling = sibling.NextSibling;
            }
            return true;
        }

        /// <summary>1-based index among sibling elements.</summary>
        private static int GetChildIndex(Element el)
        {
            int index = 1;
            var sibling = el.PreviousSibling;
            while (sibling != null)
            {
                if (sibling is Element) index++;
                sibling = sibling.PreviousSibling;
            }
            return index;
        }

        private static int GetChildIndexFromEnd(Element el)
        {
            int index = 1;
            var sibling = el.NextSibling;
            while (sibling != null)
            {
                if (sibling is Element) index++;
                sibling = sibling.NextSibling;
            }
            return index;
        }

        /// <summary>1-based index among siblings of the same type.</summary>
        private static int GetTypeIndex(Element el)
        {
            int index = 1;
            var sibling = el.PreviousSibling;
            while (sibling != null)
            {
                if (sibling is Element sibEl && ReferenceEquals(sibEl.TagName, el.TagName))
                    index++;
                sibling = sibling.PreviousSibling;
            }
            return index;
        }

        private static int GetTypeIndexFromEnd(Element el)
        {
            int index = 1;
            var sibling = el.NextSibling;
            while (sibling != null)
            {
                if (sibling is Element sibEl && ReferenceEquals(sibEl.TagName, el.TagName))
                    index++;
                sibling = sibling.NextSibling;
            }
            return index;
        }

        private static bool HasDescendantMatch(Element root, Selector selector)
        {
            var child = root.FirstChild;
            while (child != null)
            {
                if (child is Element el)
                {
                    if (selector.Matches(el)) return true;
                    if (HasDescendantMatch(el, selector)) return true;
                }
                child = child.NextSibling;
            }
            return false;
        }

        private enum PseudoType : byte
        {
            FirstChild,
            LastChild,
            FirstOfType,
            LastOfType,
            OnlyChild,
            OnlyOfType,
            Empty,
            Root,
            NthChild,
            NthLastChild,
            NthOfType,
            NthLastOfType,
            Not,
            Is,
            Where,
            Has
        }
    }
}
