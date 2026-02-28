using Rend.Core;

namespace Rend.Html
{
    /// <summary>
    /// Represents an HTML document. The root of the DOM tree.
    /// </summary>
    public sealed class Document : Node
    {
        public override NodeType NodeType => NodeType.Document;

        /// <summary>The document's DOCTYPE node, if present.</summary>
        public DocumentType? Doctype
        {
            get
            {
                var child = FirstChild;
                while (child != null)
                {
                    if (child is DocumentType dt) return dt;
                    child = child.NextSibling;
                }
                return null;
            }
        }

        /// <summary>The root html element.</summary>
        public Element? DocumentElement
        {
            get
            {
                var child = FirstChild;
                while (child != null)
                {
                    if (child is Element el && ReferenceEquals(el.TagName, _htmlTag))
                        return el;
                    child = child.NextSibling;
                }
                return null;
            }
        }

        /// <summary>The head element.</summary>
        public Element? Head
        {
            get
            {
                var html = DocumentElement;
                if (html == null) return null;
                var child = html.FirstChild;
                while (child != null)
                {
                    if (child is Element el && ReferenceEquals(el.TagName, _headTag))
                        return el;
                    child = child.NextSibling;
                }
                return null;
            }
        }

        /// <summary>The body element.</summary>
        public Element? Body
        {
            get
            {
                var html = DocumentElement;
                if (html == null) return null;
                var child = html.FirstChild;
                while (child != null)
                {
                    if (child is Element el && ReferenceEquals(el.TagName, _bodyTag))
                        return el;
                    child = child.NextSibling;
                }
                return null;
            }
        }

        /// <summary>The document title (from the title element in head).</summary>
        public string Title
        {
            get
            {
                var head = Head;
                if (head == null) return string.Empty;
                var child = head.FirstChild;
                while (child != null)
                {
                    if (child is Element el && ReferenceEquals(el.TagName, _titleTag))
                        return el.TextContent;
                    child = child.NextSibling;
                }
                return string.Empty;
            }
        }

        public override string TextContent
        {
            get => string.Empty; // Document nodes return empty per spec
            set { } // No-op
        }

        // Pre-interned tag names for fast lookup
        private static readonly string _htmlTag = StringPool.HtmlNames.Intern("html");
        private static readonly string _headTag = StringPool.HtmlNames.Intern("head");
        private static readonly string _bodyTag = StringPool.HtmlNames.Intern("body");
        private static readonly string _titleTag = StringPool.HtmlNames.Intern("title");

        public Document()
        {
            OwnerDocument = null; // Document is its own owner
        }

        /// <summary>Creates a new Element with the given tag name (interned).</summary>
        public Element CreateElement(string tagName)
        {
            var interned = StringPool.HtmlNames.Intern(tagName.ToLowerInvariant());
            return new Element(interned, this);
        }

        /// <summary>Creates a new text node.</summary>
        public TextNode CreateTextNode(string data)
        {
            return new TextNode(data, this);
        }

        /// <summary>Creates a new comment node.</summary>
        public Comment CreateComment(string data)
        {
            return new Comment(data, this);
        }

        /// <summary>Creates a new document type node.</summary>
        public DocumentType CreateDocumentType(string name, string publicId, string systemId)
        {
            return new DocumentType(name, publicId, systemId, this);
        }

        /// <summary>Creates a new document fragment.</summary>
        public DocumentFragment CreateDocumentFragment()
        {
            return new DocumentFragment(this);
        }

        /// <summary>
        /// Finds the first element matching the CSS selector.
        /// Returns null if no match.
        /// </summary>
        public Element? QuerySelector(string selector)
        {
            return Selectors.Internal.SelectorMatcher.QuerySelector(this, selector);
        }

        /// <summary>
        /// Finds all elements matching the CSS selector.
        /// </summary>
        public System.Collections.Generic.List<Element> QuerySelectorAll(string selector)
        {
            return Selectors.Internal.SelectorMatcher.QuerySelectorAll(this, selector);
        }

        /// <summary>Finds an element by its id attribute.</summary>
        public Element? GetElementById(string id)
        {
            return FindElementById(this, id);
        }

        private static Element? FindElementById(Node root, string id)
        {
            var child = root.FirstChild;
            while (child != null)
            {
                if (child is Element el)
                {
                    if (el.Id == id) return el;
                    var found = FindElementById(el, id);
                    if (found != null) return found;
                }
                child = child.NextSibling;
            }
            return null;
        }

        /// <summary>Finds all elements with the given tag name.</summary>
        public System.Collections.Generic.List<Element> GetElementsByTagName(string tagName)
        {
            var interned = StringPool.HtmlNames.Intern(tagName.ToLowerInvariant());
            var result = new System.Collections.Generic.List<Element>();
            CollectByTagName(this, interned, result);
            return result;
        }

        private static void CollectByTagName(Node root, string tagName, System.Collections.Generic.List<Element> result)
        {
            var child = root.FirstChild;
            while (child != null)
            {
                if (child is Element el)
                {
                    if (ReferenceEquals(el.TagName, tagName))
                        result.Add(el);
                    CollectByTagName(el, tagName, result);
                }
                child = child.NextSibling;
            }
        }

        /// <summary>Finds all elements with the given class name.</summary>
        public System.Collections.Generic.List<Element> GetElementsByClassName(string className)
        {
            var result = new System.Collections.Generic.List<Element>();
            CollectByClassName(this, className, result);
            return result;
        }

        private static void CollectByClassName(Node root, string className, System.Collections.Generic.List<Element> result)
        {
            var child = root.FirstChild;
            while (child != null)
            {
                if (child is Element el)
                {
                    if (el.ClassList.Contains(className))
                        result.Add(el);
                    CollectByClassName(el, className, result);
                }
                child = child.NextSibling;
            }
        }

        public override Node CloneNode(bool deep = false)
        {
            var clone = new Document();
            if (deep)
                CloneChildrenInto(clone);
            return clone;
        }

        public override string ToString() => "#document";
    }
}
