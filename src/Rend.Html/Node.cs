using System;

namespace Rend.Html
{
    /// <summary>
    /// Abstract base class for all DOM nodes. Uses a linked-list structure
    /// for O(1) append and sibling navigation without array reallocation.
    /// </summary>
    public abstract class Node
    {
        /// <summary>The type of this node.</summary>
        public abstract NodeType NodeType { get; }

        /// <summary>The parent node, or null if this is a root/detached node.</summary>
        public Node? Parent { get; internal set; }

        /// <summary>The first child node, or null if this node has no children.</summary>
        public Node? FirstChild { get; internal set; }

        /// <summary>The last child node, or null if this node has no children.</summary>
        public Node? LastChild { get; internal set; }

        /// <summary>The next sibling node, or null if this is the last child.</summary>
        public Node? NextSibling { get; internal set; }

        /// <summary>The previous sibling node, or null if this is the first child.</summary>
        public Node? PreviousSibling { get; internal set; }

        /// <summary>The document that owns this node.</summary>
        public Document? OwnerDocument { get; internal set; }

        /// <summary>Whether this node has any child nodes.</summary>
        public bool HasChildNodes => FirstChild != null;

        /// <summary>Returns a live NodeList of all child nodes.</summary>
        public NodeList ChildNodes => new NodeList(this);

        /// <summary>
        /// The text content of this node and its descendants.
        /// For Element nodes, concatenates all descendant text.
        /// For Text/Comment nodes, returns the node's data.
        /// </summary>
        public virtual string TextContent
        {
            get
            {
                // Default: concatenate text of all descendant text nodes
                var child = FirstChild;
                if (child == null) return string.Empty;

                // Fast path: single text child
                if (child.NextSibling == null && child is TextNode text)
                    return text.Data;

                // General case: walk descendants
                var sb = new System.Text.StringBuilder();
                AppendTextContent(sb);
                return sb.ToString();
            }
            set
            {
                // Remove all children
                RemoveAllChildren();

                // Add a single text node if value is non-empty
                if (!string.IsNullOrEmpty(value))
                {
                    var doc = OwnerDocument ?? (this as Document);
                    if (doc != null)
                        AppendChild(doc.CreateTextNode(value));
                }
            }
        }

        internal void AppendTextContent(System.Text.StringBuilder sb)
        {
            var child = FirstChild;
            while (child != null)
            {
                if (child is TextNode t)
                    sb.Append(t.Data);
                else
                    child.AppendTextContent(sb);
                child = child.NextSibling;
            }
        }

        /// <summary>Appends a child node to the end of this node's children.</summary>
        public Node AppendChild(Node child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (child == this) throw new InvalidOperationException("A node cannot be appended to itself.");

            // If the child is a DocumentFragment, append all its children instead
            if (child is DocumentFragment fragment)
            {
                while (fragment.FirstChild != null)
                    AppendChild(fragment.FirstChild);
                return child;
            }

            // Remove from current parent
            child.Parent?.RemoveChild(child);

            child.Parent = this;
            child.OwnerDocument = OwnerDocument ?? (this as Document);

            if (LastChild == null)
            {
                // No children yet
                FirstChild = child;
                LastChild = child;
                child.PreviousSibling = null;
                child.NextSibling = null;
            }
            else
            {
                // Append after last child
                LastChild.NextSibling = child;
                child.PreviousSibling = LastChild;
                child.NextSibling = null;
                LastChild = child;
            }

            return child;
        }

        /// <summary>Prepends a child node to the beginning of this node's children.</summary>
        public Node PrependChild(Node child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (FirstChild == null)
                return AppendChild(child);

            return InsertBefore(child, FirstChild);
        }

        /// <summary>Inserts a node before the specified reference child.</summary>
        public Node InsertBefore(Node newChild, Node? referenceChild)
        {
            if (newChild == null) throw new ArgumentNullException(nameof(newChild));

            if (referenceChild == null)
                return AppendChild(newChild);

            if (referenceChild.Parent != this)
                throw new InvalidOperationException("Reference node is not a child of this node.");

            // If the child is a DocumentFragment, insert all its children
            if (newChild is DocumentFragment fragment)
            {
                // Collect children first to avoid mutation during iteration
                var children = new System.Collections.Generic.List<Node>();
                var fc = fragment.FirstChild;
                while (fc != null)
                {
                    children.Add(fc);
                    fc = fc.NextSibling;
                }
                foreach (var c in children)
                    InsertBefore(c, referenceChild);
                return newChild;
            }

            // Remove from current parent
            newChild.Parent?.RemoveChild(newChild);

            newChild.Parent = this;
            newChild.OwnerDocument = OwnerDocument ?? (this as Document);

            var prev = referenceChild.PreviousSibling;
            newChild.PreviousSibling = prev;
            newChild.NextSibling = referenceChild;
            referenceChild.PreviousSibling = newChild;

            if (prev != null)
                prev.NextSibling = newChild;
            else
                FirstChild = newChild;

            return newChild;
        }

        /// <summary>Removes a child node from this node.</summary>
        public Node RemoveChild(Node child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (child.Parent != this)
                throw new InvalidOperationException("The node is not a child of this node.");

            var prev = child.PreviousSibling;
            var next = child.NextSibling;

            if (prev != null)
                prev.NextSibling = next;
            else
                FirstChild = next;

            if (next != null)
                next.PreviousSibling = prev;
            else
                LastChild = prev;

            child.Parent = null;
            child.PreviousSibling = null;
            child.NextSibling = null;

            return child;
        }

        /// <summary>Replaces an existing child with a new node.</summary>
        public Node ReplaceChild(Node newChild, Node oldChild)
        {
            InsertBefore(newChild, oldChild);
            RemoveChild(oldChild);
            return oldChild;
        }

        /// <summary>Removes all child nodes.</summary>
        internal void RemoveAllChildren()
        {
            var child = FirstChild;
            while (child != null)
            {
                var next = child.NextSibling;
                child.Parent = null;
                child.PreviousSibling = null;
                child.NextSibling = null;
                child = next;
            }
            FirstChild = null;
            LastChild = null;
        }

        /// <summary>Creates a shallow clone of this node.</summary>
        public abstract Node CloneNode(bool deep = false);

        /// <summary>Copies children from this node into the target (for deep clone).</summary>
        internal void CloneChildrenInto(Node target)
        {
            var child = FirstChild;
            while (child != null)
            {
                target.AppendChild(child.CloneNode(true));
                child = child.NextSibling;
            }
        }
    }
}
