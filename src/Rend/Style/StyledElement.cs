using System.Collections.Generic;
using Rend.Css;
using Rend.Html;

namespace Rend.Style
{
    /// <summary>
    /// An element node in the styled tree: wraps an HTML Element with its resolved ComputedStyle
    /// and styled children (elements and text nodes).
    /// </summary>
    public sealed class StyledElement : StyledNode
    {
        private readonly List<StyledNode> _children;

        public StyledElement(Element element, ComputedStyle style, List<StyledNode> children)
            : base(style)
        {
            Element = element;
            _children = children;
        }

        public override bool IsText => false;

        /// <summary>The underlying HTML element.</summary>
        public Element Element { get; }

        /// <summary>The tag name of the element.</summary>
        public string TagName => Element.TagName;

        /// <summary>The styled children (elements and text nodes).</summary>
        public IReadOnlyList<StyledNode> Children => _children;

        /// <summary>Get attribute value from the underlying element.</summary>
        public string? GetAttribute(string name) => Element.GetAttribute(name);
    }
}
