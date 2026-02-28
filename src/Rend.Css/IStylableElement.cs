using System.Collections.Generic;

namespace Rend.Css
{
    /// <summary>
    /// Abstraction for an HTML element used by the cascade engine.
    /// Decouples Rend.Css from Rend.Html — the orchestrator provides an adapter.
    /// </summary>
    public interface IStylableElement
    {
        /// <summary>Tag name (lowercase, interned).</summary>
        string TagName { get; }

        /// <summary>The element's id attribute, or null.</summary>
        string? Id { get; }

        /// <summary>The element's class list.</summary>
        IReadOnlyList<string> ClassList { get; }

        /// <summary>Get attribute value by name, or null.</summary>
        string? GetAttribute(string name);

        /// <summary>The inline style attribute text, or null.</summary>
        string? InlineStyle { get; }

        /// <summary>Parent element, or null for the root.</summary>
        IStylableElement? Parent { get; }

        /// <summary>Previous sibling element, or null.</summary>
        IStylableElement? PreviousSibling { get; }

        /// <summary>Next sibling element, or null.</summary>
        IStylableElement? NextSibling { get; }

        /// <summary>First child element, or null.</summary>
        IStylableElement? FirstChild { get; }

        /// <summary>Last child element, or null.</summary>
        IStylableElement? LastChild { get; }

        /// <summary>The child elements (for iteration).</summary>
        IEnumerable<IStylableElement> Children { get; }
    }
}
