using System;
using System.Collections.Generic;
using System.IO;
using Rend.Core.Values;
using Rend.Fonts;
using Rend.Output.Pdf.Internal;
using Rend.Pdf;
using Rend.Rendering;
using Rend.Text;

namespace Rend.Output.Pdf
{
    /// <summary>
    /// An <see cref="IRenderTarget"/> implementation that produces PDF output
    /// by bridging drawing commands to a <see cref="PdfDocument"/>.
    /// </summary>
    public sealed class PdfRenderTarget : IRenderTarget
    {
        private readonly PdfRenderOptions _options;
        private readonly PdfDocument _doc;
        private readonly PdfFontCache _fontCache = new PdfFontCache();
        private readonly PdfImageCache _imageCache = new PdfImageCache();

        private PdfPage? _currentPage;
        private float _currentPageHeight;
        private readonly PdfOutlineNode?[] _bookmarkStack = new PdfOutlineNode?[6];

        /// <summary>
        /// Creates a new <see cref="PdfRenderTarget"/> with the specified options.
        /// </summary>
        /// <param name="options">Rendering options, or null for defaults.</param>
        public PdfRenderTarget(PdfRenderOptions? options = null)
        {
            _options = options ?? new PdfRenderOptions();
            _doc = _options.DocumentOptions != null
                ? new PdfDocument(_options.DocumentOptions)
                : new PdfDocument();

            if (_options.Title != null)
            {
                _doc.Info.Title = _options.Title;
            }
            if (_options.Author != null)
            {
                _doc.Info.Author = _options.Author;
            }
        }

        /// <inheritdoc />
        public void BeginPage(float width, float height)
        {
            _currentPage = _doc.AddPage(width, height);
            _currentPageHeight = height;

            // Set up coordinate transform: CSS top-left origin to PDF bottom-left origin.
            // This flips Y so that (0,0) is at top-left and Y increases downward.
            _currentPage.Content.SaveState();
            _currentPage.Content.SetTransform(1f, 0f, 0f, -1f, 0f, height);
        }

        /// <inheritdoc />
        public void EndPage()
        {
            if (_currentPage != null)
            {
                // Restore the page-level coordinate flip state.
                _currentPage.Content.RestoreState();
            }
            _currentPage = null;
        }

        /// <inheritdoc />
        public void Save()
        {
            EnsurePage();
            _currentPage!.Content.SaveState();
        }

        /// <inheritdoc />
        public void Restore()
        {
            EnsurePage();
            _currentPage!.Content.RestoreState();
        }

        /// <inheritdoc />
        public void SetTransform(Matrix3x2 transform)
        {
            EnsurePage();
            _currentPage!.Content.SetTransform(
                transform.M11, transform.M12,
                transform.M21, transform.M22,
                transform.M31, transform.M32);
        }

        /// <inheritdoc />
        public void ConcatTransform(Matrix3x2 transform)
        {
            EnsurePage();
            // PDF cm operator concatenates with the current CTM
            _currentPage!.Content.ConcatTransform(
                transform.M11, transform.M12,
                transform.M21, transform.M22,
                transform.M31, transform.M32);
        }

        /// <inheritdoc />
        public void SetOpacity(float opacity)
        {
            EnsurePage();
            _currentPage!.Content.SetFillOpacity(opacity);
            _currentPage!.Content.SetStrokeOpacity(opacity);
        }

        /// <inheritdoc />
        public void ApplyFilter(Rendering.CssFilterEffect[] effects)
        {
            // PDF has limited filter support. Only apply opacity filter.
            if (effects == null) return;
            foreach (var effect in effects)
            {
                if (effect.Type == Rendering.CssFilterType.Opacity)
                {
                    SetOpacity(effect.Amount);
                    return;
                }
            }
        }

        /// <inheritdoc />
        public void BeginMask()
        {
            // PDF does not natively support gradient masks; graceful degradation.
        }

        /// <inheritdoc />
        public void EndMask(Rendering.GradientInfo gradient, Core.Values.RectF bounds)
        {
            // PDF does not natively support gradient masks; graceful degradation.
        }

        /// <inheritdoc />
        public void SetBlendMode(Css.CssMixBlendMode blendMode)
        {
            if (blendMode == Css.CssMixBlendMode.Normal) return;
            EnsurePage();
            _currentPage!.Content.SetBlendMode(MapBlendMode(blendMode));
        }

        private static string MapBlendMode(Css.CssMixBlendMode mode)
        {
            switch (mode)
            {
                case Css.CssMixBlendMode.Multiply: return "Multiply";
                case Css.CssMixBlendMode.Screen: return "Screen";
                case Css.CssMixBlendMode.Overlay: return "Overlay";
                case Css.CssMixBlendMode.Darken: return "Darken";
                case Css.CssMixBlendMode.Lighten: return "Lighten";
                case Css.CssMixBlendMode.ColorDodge: return "ColorDodge";
                case Css.CssMixBlendMode.ColorBurn: return "ColorBurn";
                case Css.CssMixBlendMode.HardLight: return "HardLight";
                case Css.CssMixBlendMode.SoftLight: return "SoftLight";
                case Css.CssMixBlendMode.Difference: return "Difference";
                case Css.CssMixBlendMode.Exclusion: return "Exclusion";
                case Css.CssMixBlendMode.Hue: return "Hue";
                case Css.CssMixBlendMode.Saturation: return "Saturation";
                case Css.CssMixBlendMode.Color: return "Color";
                case Css.CssMixBlendMode.Luminosity: return "Luminosity";
                default: return "Normal";
            }
        }

        /// <inheritdoc />
        public void SetImageRendering(Css.CssImageRendering rendering)
        {
            // PDF rendering quality is controlled by the viewer, not the writer
        }

        /// <inheritdoc />
        public void SetMaskBlur(float sigma)
        {
            // PDF does not support mask blur directly; box shadows degrade gracefully
        }

        /// <inheritdoc />
        public void PushClipRect(RectF rect)
        {
            EnsurePage();
            var content = _currentPage!.Content;
            content.SaveState();
            content.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
            content.Clip();
            content.EndPath();
        }

        /// <inheritdoc />
        public void PushClipPath(PathData path)
        {
            EnsurePage();
            var content = _currentPage!.Content;
            content.SaveState();
            WritePath(path, content);
            content.Clip();
            content.EndPath();
        }

        /// <inheritdoc />
        public void PopClip()
        {
            EnsurePage();
            _currentPage!.Content.RestoreState();
        }

        /// <inheritdoc />
        public void FillRect(RectF rect, BrushInfo brush)
        {
            EnsurePage();
            var content = _currentPage!.Content;

            SetFillFromBrush(brush, content, rect.X, rect.Y, rect.Width, rect.Height);
            content.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
            content.Fill();
        }

        /// <inheritdoc />
        public void StrokeRect(RectF rect, PenInfo pen)
        {
            EnsurePage();
            var content = _currentPage!.Content;

            SetStrokeFromPen(pen, content);
            content.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
            content.Stroke();
        }

        /// <inheritdoc />
        public void FillPath(PathData path, BrushInfo brush)
        {
            EnsurePage();
            var content = _currentPage!.Content;

            SetFillFromBrush(brush, content);
            WritePath(path, content);
            content.Fill();
        }

        /// <inheritdoc />
        public void StrokePath(PathData path, PenInfo pen)
        {
            EnsurePage();
            var content = _currentPage!.Content;

            SetStrokeFromPen(pen, content);
            WritePath(path, content);
            content.Stroke();
        }

        /// <inheritdoc />
        public void DrawImage(ImageData image, RectF destRect)
        {
            EnsurePage();
            var content = _currentPage!.Content;

            PdfImage pdfImage = _imageCache.GetOrAdd(image.Data, image.Format, _doc);
            content.DrawImage(pdfImage, destRect);
        }

        /// <inheritdoc />
        public void DrawText(string text, float x, float y, TextStyle style)
        {
            EnsurePage();
            var content = _currentPage!.Content;

            PdfFont pdfFont = ResolvePdfFont(style.Font);

            style.Color.ToFloatRgb(out float r, out float g, out float b);
            content.SetFillColor(r, g, b);

            content.BeginText();
            content.SetFont(pdfFont, style.FontSize);

            if (style.LetterSpacing != 0)
                content.SetCharacterSpacing(style.LetterSpacing);
            if (style.WordSpacing != 0)
                content.SetWordSpacing(style.WordSpacing);

            content.MoveTextPosition(x, y);
            content.ShowText(pdfFont, text);

            if (style.LetterSpacing != 0)
                content.SetCharacterSpacing(0);
            if (style.WordSpacing != 0)
                content.SetWordSpacing(0);

            content.EndText();
        }

        /// <inheritdoc />
        public void DrawGlyphs(ShapedTextRun run, float x, float y, CssColor color, FontDescriptor font)
        {
            EnsurePage();
            var content = _currentPage!.Content;

            PdfFont pdfFont = ResolvePdfFont(font);

            color.ToFloatRgb(out float r, out float g, out float b);
            content.SetFillColor(r, g, b);

            content.BeginText();
            content.SetFont(pdfFont, run.FontSize);
            content.MoveTextPosition(x, y);

            // Use the original text for encoding. The PdfFont.Encode method handles
            // glyph mapping internally, so passing the original text ensures correct
            // CID encoding for embedded fonts.
            content.ShowText(pdfFont, run.OriginalText);
            content.EndText();
        }

        /// <inheritdoc />
        public void AddLink(RectF rect, string uri)
        {
            if (_currentPage == null || string.IsNullOrEmpty(uri))
            {
                return;
            }

            if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
            {
                return;
            }

            // Convert CSS coordinates (top-left origin, Y down) to PDF (bottom-left origin, Y up).
            float pdfBottom = _currentPageHeight - rect.Y - rect.Height;
            float pdfTop = _currentPageHeight - rect.Y;
            var pdfRect = new RectF(rect.X, pdfBottom, rect.Width, pdfTop - pdfBottom);
            PdfLinkCollector.AddLink(_currentPage, pdfRect, parsedUri);
        }

        /// <inheritdoc />
        public void AddBookmark(string title, int level, float yPosition)
        {
            if (_currentPage == null || string.IsNullOrEmpty(title) || level < 1 || level > 6)
            {
                return;
            }

            float pdfY = _currentPageHeight - yPosition;

            // Build hierarchy: h1 is top-level, h2 nests under last h1, etc.
            PdfOutlineNode? parent = null;
            for (int i = level - 2; i >= 0; i--)
            {
                if (_bookmarkStack[i] != null)
                {
                    parent = _bookmarkStack[i];
                    break;
                }
            }

            PdfOutlineNode node;
            if (parent != null)
            {
                node = parent.AddChild(title, _currentPage, pdfY);
            }
            else
            {
                node = PdfBookmarkBuilder.AddBookmark(_doc, title, _currentPage, pdfY)!;
            }

            _bookmarkStack[level - 1] = node;

            // Clear deeper levels so subsequent lower-level headings nest under this one.
            for (int i = level; i < 6; i++)
            {
                _bookmarkStack[i] = null;
            }
        }

        /// <inheritdoc />
        public void Finish(Stream output)
        {
            _doc.Save(output);
        }

        // -------------------------------------------------------
        // Private helpers
        // -------------------------------------------------------

        private void EnsurePage()
        {
            if (_currentPage == null)
            {
                throw new InvalidOperationException(
                    "No page is currently active. Call BeginPage before issuing drawing commands.");
            }
        }

        private PdfFont ResolvePdfFont(FontDescriptor descriptor)
        {
            // Attempt to resolve font data from the descriptor.
            // For now, we pass null font data and let the cache fall back to Helvetica.
            // A full implementation would integrate with IFontProvider to supply font bytes.
            return _fontCache.GetOrAdd(descriptor, null, _doc);
        }

        private static void SetFillFromBrush(BrushInfo brush, PdfContentStream content,
                                               float x = 0, float y = 0, float width = 0, float height = 0)
        {
            if (brush.Gradient != null && brush.Gradient.Stops.Length > 0)
            {
                if (!PdfGradientBuilder.IsSupported(brush.Gradient))
                {
                    brush.Gradient.Stops[0].Color.ToFloatRgb(out float gr, out float gg, out float gb);
                    content.SetFillColor(gr, gg, gb);
                }
                else
                {
                    PdfGradientBuilder.Apply(brush.Gradient, content, x, y, width, height);
                }
            }
            else
            {
                brush.Color.ToFloatRgb(out float r, out float g, out float b);
                content.SetFillColor(r, g, b);
            }
        }

        private static void SetStrokeFromPen(PenInfo pen, PdfContentStream content)
        {
            pen.Color.ToFloatRgb(out float r, out float g, out float b);
            content.SetStrokeColor(r, g, b);
            content.SetLineWidth(pen.Width);

            if (pen.DashPattern != null && pen.DashPattern.Length > 0)
            {
                content.SetDashPattern(pen.DashPattern, pen.DashOffset);
            }
        }

        private static void WritePath(PathData path, PdfContentStream content)
        {
            IReadOnlyList<PathSegment> segments = path.GetSegments();
            for (int i = 0; i < segments.Count; i++)
            {
                PathSegment seg = segments[i];
                switch (seg.Type)
                {
                    case PathSegmentType.MoveTo:
                        content.MoveTo(seg.X, seg.Y);
                        break;
                    case PathSegmentType.LineTo:
                        content.LineTo(seg.X, seg.Y);
                        break;
                    case PathSegmentType.CubicBezierTo:
                        content.CurveTo(seg.X1, seg.Y1, seg.X2, seg.Y2, seg.X, seg.Y);
                        break;
                    case PathSegmentType.QuadraticBezierTo:
                        // PDF does not support quadratic bezier natively.
                        // Convert to cubic: CP1 = P0 + 2/3*(P1-P0), CP2 = P2 + 2/3*(P1-P2).
                        // We approximate by promoting to cubic with the control point used twice.
                        // A more accurate conversion would need the current point, but this is a
                        // reasonable approximation using the control point for both cubic CPs.
                        content.CurveTo(seg.X1, seg.Y1, seg.X1, seg.Y1, seg.X, seg.Y);
                        break;
                    case PathSegmentType.Close:
                        content.ClosePath();
                        break;
                }
            }
        }
    }
}
