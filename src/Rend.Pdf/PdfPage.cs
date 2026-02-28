using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Pdf.Internal;

namespace Rend.Pdf
{
    /// <summary>
    /// Represents a single page in a PDF document.
    /// </summary>
    public sealed class PdfPage
    {
        private readonly PdfObjectTable _objectTable;
        private readonly List<PdfAnnotation> _annotations = new List<PdfAnnotation>();

        /// <summary>Page width in points (1/72 inch).</summary>
        public float Width { get; }

        /// <summary>Page height in points (1/72 inch).</summary>
        public float Height { get; }

        /// <summary>Zero-based page index.</summary>
        public int PageIndex { get; }

        /// <summary>The content stream for drawing on this page.</summary>
        public PdfContentStream Content { get; }

        /// <summary>Annotations on this page.</summary>
        internal IReadOnlyList<PdfAnnotation> Annotations => _annotations;

        internal PdfPage(float width, float height, int pageIndex,
                         PdfObjectTable objectTable, bool compress, int bufferSize)
        {
            Width = width;
            Height = height;
            PageIndex = pageIndex;
            _objectTable = objectTable;
            Content = new PdfContentStream(objectTable, compress, bufferSize);
        }

        /// <summary>
        /// Add a URI link annotation (clickable area that opens a URL).
        /// </summary>
        public PdfLinkAnnotation AddLink(RectF rect, Uri uri)
        {
            var annotation = new PdfLinkAnnotation(rect, uri);
            _annotations.Add(annotation);
            return annotation;
        }

        /// <summary>
        /// Add an internal link annotation (clickable area that jumps to another page).
        /// </summary>
        public PdfLinkAnnotation AddLink(RectF rect, PdfPage targetPage, float yPosition = 0)
        {
            var annotation = new PdfLinkAnnotation(rect, targetPage, yPosition);
            _annotations.Add(annotation);
            return annotation;
        }
    }

    /// <summary>
    /// Base class for PDF annotations.
    /// </summary>
    public abstract class PdfAnnotation
    {
        /// <summary>The rectangle on the page where this annotation appears.</summary>
        public RectF Rect { get; }

        internal PdfAnnotation(RectF rect) => Rect = rect;

        internal abstract PdfDictionary ToPdfDictionary(PdfObjectTable objectTable,
                                                         Dictionary<PdfPage, PdfReference> pageRefs);
    }

    /// <summary>
    /// A link annotation — URI or internal GoTo.
    /// </summary>
    public sealed class PdfLinkAnnotation : PdfAnnotation
    {
        /// <summary>External URI target (null for internal links).</summary>
        public Uri? Uri { get; }

        /// <summary>Internal link target page (null for external links).</summary>
        public PdfPage? TargetPage { get; }

        /// <summary>Y position on target page (for internal links).</summary>
        public float TargetY { get; }

        internal PdfLinkAnnotation(RectF rect, Uri uri) : base(rect)
        {
            Uri = uri;
        }

        internal PdfLinkAnnotation(RectF rect, PdfPage targetPage, float yPosition) : base(rect)
        {
            TargetPage = targetPage;
            TargetY = yPosition;
        }

        internal override PdfDictionary ToPdfDictionary(PdfObjectTable objectTable,
                                                         Dictionary<PdfPage, PdfReference> pageRefs)
        {
            var dict = new PdfDictionary(6);
            dict[PdfName.Type] = PdfName.Annot;
            dict[PdfName.Subtype] = PdfName.Link;

            var rectArray = new PdfArray(4);
            rectArray.Add(new PdfReal(Rect.X));
            rectArray.Add(new PdfReal(Rect.Y));
            rectArray.Add(new PdfReal(Rect.Right));
            rectArray.Add(new PdfReal(Rect.Bottom));
            dict[PdfName.Rect] = rectArray;

            // Invisible border
            var border = new PdfArray(3);
            border.Add(new PdfInteger(0));
            border.Add(new PdfInteger(0));
            border.Add(new PdfInteger(0));
            dict[PdfName.Border] = border;

            if (Uri != null)
            {
                var action = new PdfDictionary(3);
                action[PdfName.Type] = PdfName.Action;
                action[PdfName.S] = PdfName.URI;
                action[PdfName.URI] = new PdfString(Uri.AbsoluteUri);
                dict[PdfName.A] = action;
            }
            else if (TargetPage != null && pageRefs.TryGetValue(TargetPage, out var pageRef))
            {
                var action = new PdfDictionary(3);
                action[PdfName.Type] = PdfName.Action;
                action[PdfName.S] = PdfName.GoTo;

                var dest = new PdfArray(5);
                dest.Add(pageRef);
                dest.Add(PdfName.XYZ);
                dest.Add(new PdfInteger(0));
                dest.Add(new PdfReal(TargetY));
                dest.Add(PdfNull.Instance);
                action[PdfName.D] = dest;

                dict[PdfName.A] = action;
            }

            return dict;
        }
    }
}
