using System;
using Rend.Core;

namespace Rend.Html.Selectors.Internal
{
    /// <summary>Type selector: matches elements by tag name (e.g., "div").</summary>
    internal sealed class TypeSelector : Selector
    {
        private readonly string _tagName;

        public TypeSelector(string tagName)
        {
            _tagName = StringPool.HtmlNames.Intern(tagName.ToLowerInvariant());
        }

        public override bool Matches(Element element)
        {
            return ReferenceEquals(element.TagName, _tagName);
        }

        public override Specificity GetSpecificity() => new Specificity(0, 0, 1);
    }

    /// <summary>Universal selector: matches all elements (*).</summary>
    internal sealed class UniversalSelector : Selector
    {
        public static readonly UniversalSelector Instance = new UniversalSelector();

        public override bool Matches(Element element) => true;
        public override Specificity GetSpecificity() => new Specificity(0, 0, 0);
    }

    /// <summary>ID selector: matches elements by id (#foo).</summary>
    internal sealed class IdSelector : Selector
    {
        private readonly string _id;

        public IdSelector(string id)
        {
            _id = id;
        }

        public override bool Matches(Element element)
        {
            return string.Equals(element.Id, _id, StringComparison.Ordinal);
        }

        public override Specificity GetSpecificity() => new Specificity(1, 0, 0);
    }

    /// <summary>Class selector: matches elements by class name (.foo).</summary>
    internal sealed class ClassSelector : Selector
    {
        private readonly string _className;

        public ClassSelector(string className)
        {
            _className = className;
        }

        public override bool Matches(Element element)
        {
            return element.ClassList.Contains(_className);
        }

        public override Specificity GetSpecificity() => new Specificity(0, 1, 0);
    }

    /// <summary>
    /// Attribute selector: [attr], [attr=val], [attr~=val], [attr|=val],
    /// [attr^=val], [attr$=val], [attr*=val]
    /// </summary>
    internal sealed class AttributeSelector : Selector
    {
        private readonly string _attribute;
        private readonly string? _value;
        private readonly AttributeOp _op;
        private readonly bool _caseInsensitive;

        public AttributeSelector(string attribute, AttributeOp op, string? value, bool caseInsensitive)
        {
            _attribute = attribute;
            _op = op;
            _value = value;
            _caseInsensitive = caseInsensitive;
        }

        public override bool Matches(Element element)
        {
            var attrVal = element.GetAttribute(_attribute);

            if (_op == AttributeOp.Exists)
                return attrVal != null;

            if (attrVal == null || _value == null)
                return false;

            var comparison = _caseInsensitive
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            switch (_op)
            {
                case AttributeOp.Equals:
                    return string.Equals(attrVal, _value, comparison);

                case AttributeOp.Includes:
                    // Space-separated list contains value
                    return ContainsWord(attrVal, _value, comparison);

                case AttributeOp.DashMatch:
                    // Equal to value or starts with value followed by -
                    return string.Equals(attrVal, _value, comparison) ||
                           (attrVal.StartsWith(_value, comparison) &&
                            attrVal.Length > _value.Length &&
                            attrVal[_value.Length] == '-');

                case AttributeOp.Prefix:
                    return attrVal.StartsWith(_value, comparison);

                case AttributeOp.Suffix:
                    return attrVal.EndsWith(_value, comparison);

                case AttributeOp.Substring:
                    return attrVal.IndexOf(_value, comparison) >= 0;

                default:
                    return false;
            }
        }

        private static bool ContainsWord(string haystack, string needle, StringComparison comparison)
        {
            int start = 0;
            while (start < haystack.Length)
            {
                while (start < haystack.Length && char.IsWhiteSpace(haystack[start])) start++;
                if (start >= haystack.Length) break;

                int end = start;
                while (end < haystack.Length && !char.IsWhiteSpace(haystack[end])) end++;

                if (end - start == needle.Length &&
                    string.Compare(haystack, start, needle, 0, needle.Length, comparison) == 0)
                    return true;

                start = end;
            }
            return false;
        }

        public override Specificity GetSpecificity() => new Specificity(0, 1, 0);
    }

    internal enum AttributeOp : byte
    {
        Exists,     // [attr]
        Equals,     // [attr=val]
        Includes,   // [attr~=val]
        DashMatch,  // [attr|=val]
        Prefix,     // [attr^=val]
        Suffix,     // [attr$=val]
        Substring   // [attr*=val]
    }
}
