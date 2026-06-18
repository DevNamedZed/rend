#nullable enable
using System;
using Rend.Pdf.Parsing;
using SkiaSharp;

namespace Rend.PdfRendering
{
    /// <summary>
    /// Renders a page's annotation appearance streams (<c>/AP</c>) on top of the page content,
    /// following the appearance algorithm of ISO 32000-1 §12.5.5. The actual form content stream
    /// is executed by the page renderer through <see cref="AppearanceStreamRenderer"/>; this class
    /// owns only the annotation-specific concerns: which annotations to draw, selecting the normal
    /// appearance (including the <c>/AS</c> sub-state), and the BBox→Rect placement matrix.
    /// </summary>
    /// <spec>PDF-32000-1 §12.5.5 https://opensource.adobe.com/dc-acrobat-sdk-docs/pdfstandards/PDF32000_2008.pdf</spec>
    internal sealed class AnnotationRenderer
    {
        // [SPEC §12.5.3, Table 165] Annotation flags.
        private const int HiddenFlag = 1 << 1;
        private const int NoViewFlag = 1 << 5;

        private readonly PdfDocumentReader _reader;
        private readonly AppearanceStreamRenderer _renderAppearance;
        private readonly Action<string> _addWarning;

        public AnnotationRenderer(PdfDocumentReader reader, AppearanceStreamRenderer renderAppearance,
            Action<string> addWarning)
        {
            _reader = reader;
            _renderAppearance = renderAppearance;
            _addWarning = addWarning;
        }

        public void RenderAnnotations(SKCanvas canvas, PdfObj pageDict)
        {
            PdfObj annotations = _reader.Resolve(pageDict["Annots"]);
            if (!annotations.IsArray)
            {
                return;
            }

            for (int index = 0; index < annotations.Count; index++)
            {
                PdfObj annotation = _reader.Resolve(annotations[index]);
                if (!annotation.IsDict || ShouldSkip(annotation))
                {
                    continue;
                }

                PdfObj appearance = ResolveNormalAppearance(annotation);
                if (appearance.IsNull)
                {
                    continue;
                }

                RenderAnnotation(canvas, annotation, appearance, pageDict);
            }
        }

        private bool ShouldSkip(PdfObj annotation)
        {
            // Popups are only painted when their parent markup is open; never draw them inline.
            if (NameOf(annotation["Subtype"]) == "Popup")
            {
                return true;
            }

            int flags = (int)_reader.Resolve(annotation["F"]).AsInt();
            return (flags & HiddenFlag) != 0 || (flags & NoViewFlag) != 0;
        }

        // [SPEC §12.5.5] The normal appearance /N is either an appearance stream or a sub-dictionary
        // of appearance states keyed by the annotation's current /AS state (checkboxes, radios).
        private PdfObj ResolveNormalAppearance(PdfObj annotation)
        {
            PdfObj normal = _reader.Resolve(_reader.Resolve(annotation["AP"])["N"]);
            if (normal.IsStream)
            {
                return normal;
            }

            if (normal.IsDict)
            {
                string state = NameOf(annotation["AS"]);
                if (state.Length == 0)
                {
                    _addWarning("Annotation /N is a state sub-dictionary but /AS is missing");
                    return PdfObj.Null;
                }

                PdfObj selected = _reader.Resolve(normal[state]);
                if (selected.IsStream)
                {
                    return selected;
                }
            }

            return PdfObj.Null;
        }

        private void RenderAnnotation(SKCanvas canvas, PdfObj annotation, PdfObj appearance, PdfObj pageDict)
        {
            PdfObj rectObj = _reader.Resolve(annotation["Rect"]);
            if (!rectObj.IsArray || rectObj.Count < 4)
            {
                return;
            }

            PdfObj bboxObj = _reader.Resolve(appearance["BBox"]);
            if (!bboxObj.IsArray || bboxObj.Count < 4)
            {
                return;
            }

            SKMatrix alignment = ComputeAlignmentMatrix(Normalize(PdfRect.FromArray(rectObj)),
                PdfRect.FromArray(bboxObj), ReadFormMatrix(appearance));
            if (alignment.ScaleX == 0f || alignment.ScaleY == 0f)
            {
                return;
            }

            canvas.Save();
            canvas.Concat(alignment);
            _renderAppearance(canvas, appearance, pageDict);
            canvas.Restore();
        }

        // [SPEC §12.5.5] Map the appearance's transformed bounding box onto the annotation Rect.
        // Transform the BBox corners by the form Matrix, take the upright bounding box, then build A
        // mapping that box's lower-left/upper-right corners to Rect's. The page renderer applies the
        // form Matrix itself, so concatenating A alone yields the required AA = Matrix × A.
        private static SKMatrix ComputeAlignmentMatrix(PdfRect rect, PdfRect bbox, SKMatrix formMatrix)
        {
            SKPoint corner0 = formMatrix.MapPoint(bbox.Left, bbox.Bottom);
            SKPoint corner1 = formMatrix.MapPoint(bbox.Right, bbox.Bottom);
            SKPoint corner2 = formMatrix.MapPoint(bbox.Right, bbox.Top);
            SKPoint corner3 = formMatrix.MapPoint(bbox.Left, bbox.Top);

            float minX = Math.Min(Math.Min(corner0.X, corner1.X), Math.Min(corner2.X, corner3.X));
            float maxX = Math.Max(Math.Max(corner0.X, corner1.X), Math.Max(corner2.X, corner3.X));
            float minY = Math.Min(Math.Min(corner0.Y, corner1.Y), Math.Min(corner2.Y, corner3.Y));
            float maxY = Math.Max(Math.Max(corner0.Y, corner1.Y), Math.Max(corner2.Y, corner3.Y));

            float boxWidth = maxX - minX;
            float boxHeight = maxY - minY;
            float scaleX = boxWidth > 0f ? rect.Width / boxWidth : 1f;
            float scaleY = boxHeight > 0f ? rect.Height / boxHeight : 1f;
            float translateX = rect.Left - scaleX * minX;
            float translateY = rect.Bottom - scaleY * minY;

            return new SKMatrix(scaleX, 0f, translateX, 0f, scaleY, translateY, 0f, 0f, 1f);
        }

        private SKMatrix ReadFormMatrix(PdfObj appearance)
        {
            PdfObj matrixObj = _reader.Resolve(appearance["Matrix"]);
            if (!matrixObj.IsArray || matrixObj.Count < 6)
            {
                return SKMatrix.Identity;
            }

            float a = matrixObj[0].AsFloat();
            float b = matrixObj[1].AsFloat();
            float c = matrixObj[2].AsFloat();
            float d = matrixObj[3].AsFloat();
            float e = matrixObj[4].AsFloat();
            float f = matrixObj[5].AsFloat();
            return new SKMatrix(a, c, e, b, d, f, 0f, 0f, 1f);
        }

        private string NameOf(PdfObj obj)
        {
            string name = _reader.Resolve(obj).AsName();
            return name.StartsWith("/", StringComparison.Ordinal) ? name.Substring(1) : name;
        }

        private static PdfRect Normalize(PdfRect rect)
        {
            float left = Math.Min(rect.Left, rect.Right);
            float right = Math.Max(rect.Left, rect.Right);
            float bottom = Math.Min(rect.Bottom, rect.Top);
            float top = Math.Max(rect.Bottom, rect.Top);
            return new PdfRect(left, bottom, right, top);
        }
    }

    internal delegate void AppearanceStreamRenderer(SKCanvas canvas, PdfObj appearanceStream, PdfObj pageDict);
}
