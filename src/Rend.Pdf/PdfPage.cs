using System;
using System.Collections.Generic;
using System.IO.Compression;
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
        private readonly List<PdfFormField> _formFields = new List<PdfFormField>();

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

        /// <summary>Form fields on this page.</summary>
        internal IReadOnlyList<PdfFormField> FormFields => _formFields;

        internal PdfPage(float width, float height, int pageIndex,
                         PdfObjectTable objectTable, bool compress, int bufferSize,
                         CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            Width = width;
            Height = height;
            PageIndex = pageIndex;
            _objectTable = objectTable;
            Content = new PdfContentStream(objectTable, compress, bufferSize, compressionLevel);
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

        /// <summary>Add a text input field to this page.</summary>
        public PdfTextField AddTextField(string name, RectF rect)
        {
            var field = new PdfTextField(name, rect);
            _formFields.Add(field);
            return field;
        }

        /// <summary>Add a checkbox field to this page.</summary>
        public PdfCheckboxField AddCheckbox(string name, RectF rect)
        {
            var field = new PdfCheckboxField(name, rect);
            _formFields.Add(field);
            return field;
        }

        /// <summary>Add a dropdown (combo box) field to this page.</summary>
        public PdfDropdownField AddDropdown(string name, RectF rect)
        {
            var field = new PdfDropdownField(name, rect);
            _formFields.Add(field);
            return field;
        }

        /// <summary>Add a highlight annotation over the specified rectangle.</summary>
        public PdfHighlightAnnotation AddHighlight(RectF rect, CssColor? color = null)
        {
            var annot = new PdfHighlightAnnotation(rect, color ?? CssColor.FromRgba(255, 255, 0));
            _annotations.Add(annot);
            return annot;
        }

        /// <summary>Add an underline annotation over the specified rectangle.</summary>
        public PdfUnderlineAnnotation AddUnderline(RectF rect, CssColor? color = null)
        {
            var annot = new PdfUnderlineAnnotation(rect, color ?? CssColor.FromRgba(0, 255, 0));
            _annotations.Add(annot);
            return annot;
        }

        /// <summary>Add a strikeout annotation over the specified rectangle.</summary>
        public PdfStrikeOutAnnotation AddStrikeOut(RectF rect, CssColor? color = null)
        {
            var annot = new PdfStrikeOutAnnotation(rect, color ?? CssColor.FromRgba(255, 0, 0));
            _annotations.Add(annot);
            return annot;
        }

        /// <summary>Add a sticky note (text) annotation at the specified position.</summary>
        public PdfStickyNoteAnnotation AddStickyNote(RectF rect, string text, CssColor? color = null)
        {
            var annot = new PdfStickyNoteAnnotation(rect, text, color ?? CssColor.FromRgba(255, 255, 0));
            _annotations.Add(annot);
            return annot;
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

    /// <summary>Base class for text markup annotations (highlight, underline, strikeout).</summary>
    public abstract class PdfTextMarkupAnnotation : PdfAnnotation
    {
        /// <summary>The annotation color.</summary>
        public CssColor Color { get; }

        internal PdfTextMarkupAnnotation(RectF rect, CssColor color) : base(rect)
        {
            Color = color;
        }

        internal abstract PdfName SubtypeName { get; }

        internal override PdfDictionary ToPdfDictionary(PdfObjectTable objectTable,
                                                         Dictionary<PdfPage, PdfReference> pageRefs)
        {
            var dict = new PdfDictionary(6);
            dict[PdfName.Type] = PdfName.Annot;
            dict[PdfName.Subtype] = SubtypeName;

            var rectArray = new PdfArray(4);
            rectArray.Add(new PdfReal(Rect.X));
            rectArray.Add(new PdfReal(Rect.Y));
            rectArray.Add(new PdfReal(Rect.Right));
            rectArray.Add(new PdfReal(Rect.Bottom));
            dict[PdfName.Rect] = rectArray;

            // QuadPoints — 8 values defining the quadrilateral covering the text
            // Order: bottom-left, bottom-right, top-left, top-right (per ISO 32000-1)
            // Actually PDF spec says: x1,y1,x2,y2,x3,y3,x4,y4 where
            // the four vertices in counterclockwise order starting from the lower-left
            var quadPoints = new PdfArray(8);
            quadPoints.Add(new PdfReal(Rect.X));           // lower-left x
            quadPoints.Add(new PdfReal(Rect.Bottom));      // lower-left y
            quadPoints.Add(new PdfReal(Rect.Right));       // lower-right x
            quadPoints.Add(new PdfReal(Rect.Bottom));      // lower-right y
            quadPoints.Add(new PdfReal(Rect.X));           // upper-left x
            quadPoints.Add(new PdfReal(Rect.Y));           // upper-left y
            quadPoints.Add(new PdfReal(Rect.Right));       // upper-right x
            quadPoints.Add(new PdfReal(Rect.Y));           // upper-right y
            dict[PdfName.QuadPoints] = quadPoints;

            // Color
            Color.ToFloatRgb(out float r, out float g, out float b);
            var colorArray = new PdfArray(3);
            colorArray.Add(new PdfReal(r));
            colorArray.Add(new PdfReal(g));
            colorArray.Add(new PdfReal(b));
            dict[PdfName.C] = colorArray;

            return dict;
        }
    }

    /// <summary>Highlight annotation — yellow (or custom color) highlight over text.</summary>
    public sealed class PdfHighlightAnnotation : PdfTextMarkupAnnotation
    {
        internal PdfHighlightAnnotation(RectF rect, CssColor color) : base(rect, color) { }
        internal override PdfName SubtypeName => PdfName.Highlight;
    }

    /// <summary>Underline annotation — marks text with an underline.</summary>
    public sealed class PdfUnderlineAnnotation : PdfTextMarkupAnnotation
    {
        internal PdfUnderlineAnnotation(RectF rect, CssColor color) : base(rect, color) { }
        internal override PdfName SubtypeName => PdfName.Underline;
    }

    /// <summary>StrikeOut annotation — marks text with a strikethrough line.</summary>
    public sealed class PdfStrikeOutAnnotation : PdfTextMarkupAnnotation
    {
        internal PdfStrikeOutAnnotation(RectF rect, CssColor color) : base(rect, color) { }
        internal override PdfName SubtypeName => PdfName.StrikeOut;
    }

    /// <summary>Sticky note (text) annotation — a comment popup at a position.</summary>
    public sealed class PdfStickyNoteAnnotation : PdfAnnotation
    {
        /// <summary>The text content of the note.</summary>
        public string Text { get; }

        /// <summary>The note color.</summary>
        public CssColor Color { get; }

        /// <summary>Whether the note is initially open. Default: false.</summary>
        public bool IsOpen { get; set; }

        internal PdfStickyNoteAnnotation(RectF rect, string text, CssColor color) : base(rect)
        {
            Text = text;
            Color = color;
        }

        internal override PdfDictionary ToPdfDictionary(PdfObjectTable objectTable,
                                                         Dictionary<PdfPage, PdfReference> pageRefs)
        {
            var dict = new PdfDictionary(6);
            dict[PdfName.Type] = PdfName.Annot;
            dict[PdfName.Subtype] = PdfName.Text_Annot;

            var rectArray = new PdfArray(4);
            rectArray.Add(new PdfReal(Rect.X));
            rectArray.Add(new PdfReal(Rect.Y));
            rectArray.Add(new PdfReal(Rect.Right));
            rectArray.Add(new PdfReal(Rect.Bottom));
            dict[PdfName.Rect] = rectArray;

            dict[PdfName.Contents] = new PdfString(Text);
            dict[PdfName.Open] = IsOpen ? PdfBoolean.True : PdfBoolean.False;

            // Color
            Color.ToFloatRgb(out float r, out float g, out float b);
            var colorArray = new PdfArray(3);
            colorArray.Add(new PdfReal(r));
            colorArray.Add(new PdfReal(g));
            colorArray.Add(new PdfReal(b));
            dict[PdfName.C] = colorArray;

            return dict;
        }
    }
}
