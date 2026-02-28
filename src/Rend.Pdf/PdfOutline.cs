using System.Collections.Generic;

namespace Rend.Pdf
{
    /// <summary>
    /// A node in the PDF document outline (bookmarks) tree.
    /// </summary>
    public sealed class PdfOutlineNode
    {
        /// <summary>The display title for this bookmark.</summary>
        public string Title { get; set; }

        /// <summary>The target page.</summary>
        public PdfPage Page { get; }

        /// <summary>The Y position on the target page (top of the view).</summary>
        public float YPosition { get; }

        /// <summary>Child outline nodes.</summary>
        public IReadOnlyList<PdfOutlineNode> Children => _children;

        private readonly List<PdfOutlineNode> _children = new List<PdfOutlineNode>();

        internal PdfOutlineNode(string title, PdfPage page, float yPosition)
        {
            Title = title;
            Page = page;
            YPosition = yPosition;
        }

        /// <summary>
        /// Add a child outline node.
        /// </summary>
        public PdfOutlineNode AddChild(string title, PdfPage page, float yPosition = 0)
        {
            var child = new PdfOutlineNode(title, page, yPosition);
            _children.Add(child);
            return child;
        }

        /// <summary>
        /// Count all descendants (including self) for the PDF /Count field.
        /// </summary>
        internal int TotalCount()
        {
            int count = 0;
            foreach (var child in _children)
            {
                count += 1 + child.TotalCount();
            }
            return count;
        }
    }
}
