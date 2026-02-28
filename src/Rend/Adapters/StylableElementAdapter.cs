using System.Collections.Generic;
using System.Linq;
using Rend.Css;
using Rend.Html;

namespace Rend.Adapters
{
    /// <summary>
    /// Wraps an <see cref="Html.Element"/> as <see cref="IStylableElement"/> for the CSS cascade engine.
    /// </summary>
    public sealed class StylableElementAdapter : IStylableElement
    {
        private readonly Element _element;

        public StylableElementAdapter(Element element)
        {
            _element = element;
        }

        public Element Element => _element;

        public string TagName => _element.TagName;

        public string? Id => _element.Id;

        public IReadOnlyList<string> ClassList
        {
            get
            {
                var tokenList = _element.ClassList;
                var list = new List<string>();
                foreach (var cls in tokenList)
                    list.Add(cls);
                return list;
            }
        }

        public string? GetAttribute(string name) => _element.GetAttribute(name);

        public string? InlineStyle => _element.GetAttribute("style");

        public IStylableElement? Parent
        {
            get
            {
                var parent = _element.Parent;
                while (parent != null)
                {
                    if (parent is Element el)
                        return new StylableElementAdapter(el);
                    parent = parent.Parent;
                }
                return null;
            }
        }

        public IStylableElement? PreviousSibling
        {
            get
            {
                var sibling = _element.PreviousSibling;
                while (sibling != null)
                {
                    if (sibling is Element el)
                        return new StylableElementAdapter(el);
                    sibling = sibling.PreviousSibling;
                }
                return null;
            }
        }

        public IStylableElement? NextSibling
        {
            get
            {
                var sibling = _element.NextSibling;
                while (sibling != null)
                {
                    if (sibling is Element el)
                        return new StylableElementAdapter(el);
                    sibling = sibling.NextSibling;
                }
                return null;
            }
        }

        public IStylableElement? FirstChild
        {
            get
            {
                var child = _element.FirstChild;
                while (child != null)
                {
                    if (child is Element el)
                        return new StylableElementAdapter(el);
                    child = child.NextSibling;
                }
                return null;
            }
        }

        public IStylableElement? LastChild
        {
            get
            {
                var child = _element.LastChild;
                while (child != null)
                {
                    if (child is Element el)
                        return new StylableElementAdapter(el);
                    child = child.PreviousSibling;
                }
                return null;
            }
        }

        public IEnumerable<IStylableElement> Children
        {
            get
            {
                var child = _element.FirstChild;
                while (child != null)
                {
                    if (child is Element el)
                        yield return new StylableElementAdapter(el);
                    child = child.NextSibling;
                }
            }
        }
    }
}
