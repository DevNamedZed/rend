using System;
using Rend.Core;

namespace Rend.Html
{
    /// <summary>
    /// Represents an HTML element with a tag name, attributes, and children.
    /// Tag names and attribute names are interned for O(1) reference-equality comparison.
    /// </summary>
    public sealed class Element : Node
    {
        private HtmlAttribute[] _attributes;
        private int _attributeCount;

        public override NodeType NodeType => NodeType.Element;

        /// <summary>The tag name (interned, lowercase).</summary>
        public string TagName { get; }

        /// <summary>The namespace URI, if any.</summary>
        public string? NamespaceUri { get; internal set; }

        /// <summary>Read-only view of this element's attributes.</summary>
        public AttributeList Attributes => new AttributeList(_attributes, _attributeCount);

        /// <summary>Space-separated class list backed by the "class" attribute.</summary>
        public TokenList ClassList => new TokenList(this, "class");

        /// <summary>The element's id attribute value, or null.</summary>
        public string? Id
        {
            get => GetAttribute("id");
            set
            {
                if (value == null) RemoveAttribute("id");
                else SetAttribute("id", value);
            }
        }

        internal Element(string tagName, Document? ownerDocument)
        {
            TagName = tagName;
            OwnerDocument = ownerDocument;
            _attributes = Array.Empty<HtmlAttribute>();
            _attributeCount = 0;
        }

        /// <summary>Gets the value of an attribute by name, or null if not present.</summary>
        public string? GetAttribute(string name)
        {
            var internedName = StringPool.HtmlNames.Intern(name);
            for (int i = 0; i < _attributeCount; i++)
            {
                if (ReferenceEquals(_attributes[i].Name, internedName))
                    return _attributes[i].Value;
            }
            return null;
        }

        /// <summary>Sets an attribute value. Creates the attribute if it doesn't exist.</summary>
        public void SetAttribute(string name, string value)
        {
            var internedName = StringPool.HtmlNames.Intern(name);

            // Update existing
            for (int i = 0; i < _attributeCount; i++)
            {
                if (ReferenceEquals(_attributes[i].Name, internedName))
                {
                    _attributes[i] = new HtmlAttribute(internedName, value);
                    return;
                }
            }

            // Add new
            if (_attributeCount == _attributes.Length)
            {
                var newSize = _attributes.Length == 0 ? 4 : _attributes.Length * 2;
                var newArr = new HtmlAttribute[newSize];
                Array.Copy(_attributes, newArr, _attributeCount);
                _attributes = newArr;
            }

            _attributes[_attributeCount++] = new HtmlAttribute(internedName, value);
        }

        /// <summary>Removes an attribute by name. No-op if not present.</summary>
        public void RemoveAttribute(string name)
        {
            var internedName = StringPool.HtmlNames.Intern(name);
            for (int i = 0; i < _attributeCount; i++)
            {
                if (ReferenceEquals(_attributes[i].Name, internedName))
                {
                    // Shift remaining attributes down
                    for (int j = i; j < _attributeCount - 1; j++)
                        _attributes[j] = _attributes[j + 1];
                    _attributeCount--;
                    _attributes[_attributeCount] = default;
                    return;
                }
            }
        }

        /// <summary>Returns true if the element has the specified attribute.</summary>
        public bool HasAttribute(string name)
        {
            return GetAttribute(name) != null;
        }

        /// <summary>
        /// Sets attributes from an array of HtmlAttribute structs (used by the parser).
        /// Replaces any existing attributes.
        /// </summary>
        internal void SetAttributes(HtmlAttribute[] attrs, int count)
        {
            if (count == 0)
            {
                _attributes = Array.Empty<HtmlAttribute>();
                _attributeCount = 0;
                return;
            }

            _attributes = new HtmlAttribute[count];
            Array.Copy(attrs, _attributes, count);
            _attributeCount = count;
        }

        /// <summary>The inner HTML content of this element.</summary>
        public string InnerHtml
        {
            get => Parser.Internal.HtmlSerializer.SerializeChildren(this);
            set => Parser.Internal.HtmlSerializer.ParseAndSetInnerHtml(this, value);
        }

        /// <summary>The outer HTML of this element (including the element itself).</summary>
        public string OuterHtml => Parser.Internal.HtmlSerializer.Serialize(this);

        public override Node CloneNode(bool deep = false)
        {
            var doc = OwnerDocument;
            var clone = new Element(TagName, doc);
            clone.NamespaceUri = NamespaceUri;
            if (_attributeCount > 0)
            {
                clone._attributes = new HtmlAttribute[_attributeCount];
                Array.Copy(_attributes, clone._attributes, _attributeCount);
                clone._attributeCount = _attributeCount;
            }

            if (deep)
                CloneChildrenInto(clone);

            return clone;
        }

        /// <summary>Finds the first descendant element matching the CSS selector.</summary>
        public Element? QuerySelector(string selector)
        {
            return Selectors.Internal.SelectorMatcher.QuerySelector(this, selector);
        }

        /// <summary>Finds all descendant elements matching the CSS selector.</summary>
        public System.Collections.Generic.List<Element> QuerySelectorAll(string selector)
        {
            return Selectors.Internal.SelectorMatcher.QuerySelectorAll(this, selector);
        }

        /// <summary>Returns true if this element matches the given CSS selector.</summary>
        public bool Matches(string selector)
        {
            return Selectors.Internal.SelectorMatcher.Matches(this, selector);
        }

        public override string ToString() => $"<{TagName}>";
    }
}
