using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Rend.Core
{
    /// <summary>
    /// Thread-safe string interning pool. Ensures that identical strings share
    /// the same object reference, enabling O(1) reference-equality comparison
    /// for tag names, attribute names, CSS property names, etc.
    /// </summary>
    public sealed class StringPool
    {
        private readonly Dictionary<string, string> _pool;
        private readonly object _lock = new object();

        public StringPool(int initialCapacity = 256)
        {
            _pool = new Dictionary<string, string>(initialCapacity, StringComparer.Ordinal);
        }

        /// <summary>
        /// Intern a string. Returns the canonical instance.
        /// If this string was seen before, returns the same object reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Intern(string value)
        {
            if (value == null) return null!;

            lock (_lock)
            {
                if (_pool.TryGetValue(value, out var existing))
                    return existing;

                _pool[value] = value;
                return value;
            }
        }

        /// <summary>
        /// Intern a substring without allocating an intermediate string
        /// (when the span matches an existing interned string).
        /// </summary>
        public string Intern(ReadOnlySpan<char> value)
        {
            // For netstandard2.0 we need to create the string first
            // On newer runtimes this could use Dictionary<ReadOnlyMemory<char>>
            string str = value.ToString();
            return Intern(str);
        }

        /// <summary>Number of unique strings in the pool.</summary>
        public int Count
        {
            get { lock (_lock) return _pool.Count; }
        }

        /// <summary>
        /// Shared pool for HTML tag names and attribute names.
        /// Pre-populated with common HTML tags.
        /// </summary>
        public static readonly StringPool HtmlNames = CreateHtmlPool();

        /// <summary>
        /// Shared pool for CSS property names.
        /// Pre-populated with common CSS properties.
        /// </summary>
        public static readonly StringPool CssNames = CreateCssPool();

        private static StringPool CreateHtmlPool()
        {
            var pool = new StringPool(128);
            // Pre-intern common HTML tags
            var tags = new[]
            {
                "html", "head", "body", "div", "span", "p", "a", "img", "br", "hr",
                "h1", "h2", "h3", "h4", "h5", "h6", "ul", "ol", "li", "dl", "dt", "dd",
                "table", "thead", "tbody", "tfoot", "tr", "th", "td", "caption", "colgroup", "col",
                "form", "input", "button", "select", "option", "textarea", "label", "fieldset", "legend",
                "section", "article", "aside", "nav", "header", "footer", "main", "figure", "figcaption",
                "strong", "em", "b", "i", "u", "s", "small", "sub", "sup", "code", "pre", "blockquote",
                "style", "link", "meta", "title", "script", "noscript", "template",
                "video", "audio", "source", "canvas", "svg", "path", "circle", "rect", "line",
                // Common attributes
                "id", "class", "style", "href", "src", "alt", "title", "type", "name", "value",
                "width", "height", "rel", "media", "charset", "content", "lang", "dir",
                "data", "role", "tabindex", "hidden", "disabled", "readonly", "placeholder"
            };
            foreach (var tag in tags) pool.Intern(tag);
            return pool;
        }

        private static StringPool CreateCssPool()
        {
            var pool = new StringPool(256);
            var props = new[]
            {
                "display", "position", "float", "clear", "visibility", "opacity", "overflow",
                "overflow-x", "overflow-y", "z-index", "cursor", "pointer-events",
                "width", "height", "min-width", "max-width", "min-height", "max-height",
                "margin", "margin-top", "margin-right", "margin-bottom", "margin-left",
                "padding", "padding-top", "padding-right", "padding-bottom", "padding-left",
                "border", "border-width", "border-style", "border-color",
                "border-top", "border-right", "border-bottom", "border-left",
                "border-top-width", "border-right-width", "border-bottom-width", "border-left-width",
                "border-top-style", "border-right-style", "border-bottom-style", "border-left-style",
                "border-top-color", "border-right-color", "border-bottom-color", "border-left-color",
                "border-radius", "border-top-left-radius", "border-top-right-radius",
                "border-bottom-left-radius", "border-bottom-right-radius",
                "background", "background-color", "background-image", "background-position",
                "background-size", "background-repeat", "background-attachment", "background-clip",
                "color", "font", "font-family", "font-size", "font-weight", "font-style",
                "font-variant", "font-stretch", "line-height", "letter-spacing", "word-spacing",
                "text-align", "text-decoration", "text-transform", "text-indent", "text-overflow",
                "white-space", "word-break", "word-wrap", "overflow-wrap",
                "vertical-align", "direction", "unicode-bidi",
                "flex", "flex-direction", "flex-wrap", "flex-flow", "flex-grow", "flex-shrink", "flex-basis",
                "justify-content", "align-items", "align-self", "align-content", "order", "gap",
                "grid", "grid-template", "grid-template-columns", "grid-template-rows",
                "grid-template-areas", "grid-column", "grid-row", "grid-area",
                "grid-auto-columns", "grid-auto-rows", "grid-auto-flow", "grid-gap",
                "column-gap", "row-gap",
                "list-style", "list-style-type", "list-style-position", "list-style-image",
                "table-layout", "border-collapse", "border-spacing", "empty-cells", "caption-side",
                "transform", "transform-origin", "transition", "animation",
                "box-sizing", "box-shadow", "text-shadow", "outline",
                "content", "counter-increment", "counter-reset", "quotes",
                "page-break-before", "page-break-after", "page-break-inside",
                "break-before", "break-after", "break-inside", "orphans", "widows",
                "top", "right", "bottom", "left",
                "object-fit", "object-position",
                // Common values
                "none", "auto", "inherit", "initial", "unset",
                "block", "inline", "inline-block", "flex", "grid", "table", "inline-flex", "inline-grid",
                "static", "relative", "absolute", "fixed", "sticky",
                "hidden", "visible", "scroll", "collapse",
                "solid", "dashed", "dotted", "double", "groove", "ridge", "inset", "outset",
                "normal", "bold", "italic", "oblique",
                "left", "right", "center", "justify",
                "nowrap", "pre", "pre-wrap", "pre-line", "break-all", "keep-all",
                "transparent", "currentColor",
                "row", "column", "row-reverse", "column-reverse", "wrap", "nowrap", "wrap-reverse",
                "start", "end", "flex-start", "flex-end", "space-between", "space-around", "space-evenly", "stretch",
                "border-box", "content-box", "padding-box"
            };
            foreach (var prop in props) pool.Intern(prop);
            return pool;
        }
    }
}
