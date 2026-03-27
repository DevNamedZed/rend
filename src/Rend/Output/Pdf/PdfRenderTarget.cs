using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
    internal sealed class PdfRenderTarget : IRenderTarget
    {
        private readonly PdfRenderOptions _options;
        private readonly PdfDocument _doc;
        private readonly PdfFontCache _fontCache = new PdfFontCache();
        private readonly PdfImageCache _imageCache = new PdfImageCache();
        private IFontProvider? _fontProvider;

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
        public void SetMaskBlur(float sigma, bool inner = false)
        {
            _maskBlurSigma = sigma;
        }
        private float _maskBlurSigma;

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

            // Approximate Gaussian blur for outer box shadows
            if (_maskBlurSigma > 0)
            {
                PaintBlurredOuterShadow(rect, brush, content);
                return;
            }

            if (brush.Gradient != null && brush.Gradient.Stops.Length > 0
                && PdfGradientBuilder.IsSupported(brush.Gradient))
            {
                // PDF shading (sh) fills the entire current clip region.
                // Clip to the rect first, then paint the gradient.
                content.SaveState();
                content.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                content.Clip();
                content.EndPath();
                PdfGradientBuilder.Apply(brush.Gradient, content, rect.X, rect.Y, rect.Width, rect.Height);
                content.RestoreState();
            }
            else
            {
                bool hasAlpha = BrushHasAlpha(brush);
                if (hasAlpha) content.SaveState();
                SetFillFromBrush(brush, content, rect.X, rect.Y, rect.Width, rect.Height);
                content.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                content.Fill();
                if (hasAlpha) content.RestoreState();
            }
        }

        /// <inheritdoc />
        public void StrokeRect(RectF rect, PenInfo pen)
        {
            EnsurePage();
            var content = _currentPage!.Content;

            bool hasAlpha = pen.Color.A < 255;
            if (hasAlpha) content.SaveState();
            SetStrokeFromPen(pen, content);
            content.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
            content.Stroke();
            if (hasAlpha) content.RestoreState();
        }

        /// <inheritdoc />
        public void FillRoundRectDifference(RoundedRectInfo outer, RoundedRectInfo inner, BrushInfo brush)
        {
            // PDF fallback: build EvenOdd path from both rounded rects.
            var path = new PathData();
            path.FillType = PathFillType.EvenOdd;
            path.AddRoundedRectangleElliptical(outer.Rect, outer.TlRx, outer.TlRy, outer.TrRx, outer.TrRy,
                                outer.BrRx, outer.BrRy, outer.BlRx, outer.BlRy);
            path.AddRoundedRectangleElliptical(inner.Rect, inner.TlRx, inner.TlRy, inner.TrRx, inner.TrRy,
                                inner.BrRx, inner.BrRy, inner.BlRx, inner.BlRy);
            FillPath(path, brush);
        }

        /// <inheritdoc />
        public void FillPath(PathData path, BrushInfo brush)
        {
            EnsurePage();
            var content = _currentPage!.Content;
            var bounds = path.GetBounds();

            // Approximate Gaussian blur for shadow paths
            if (_maskBlurSigma > 0)
            {
                if (path.FillType == Rendering.PathFillType.EvenOdd)
                    PaintBlurredInsetShadow(path, brush, content);
                else
                    PaintBlurredOuterShadow(bounds, brush, content);
                return;
            }

            if (brush.Gradient != null && brush.Gradient.Stops.Length > 0
                && PdfGradientBuilder.IsSupported(brush.Gradient))
            {
                // PDF shading (sh) fills the entire current clip region.
                // Clip to the path first, then paint the gradient.
                content.SaveState();
                WritePath(path, content);
                if (path.FillType == Rendering.PathFillType.EvenOdd)
                    content.ClipEvenOdd();
                else
                    content.Clip();
                content.EndPath();
                PdfGradientBuilder.Apply(brush.Gradient, content, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                content.RestoreState();
            }
            else
            {
                bool hasAlpha = BrushHasAlpha(brush);
                if (hasAlpha) content.SaveState();
                SetFillFromBrush(brush, content, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                WritePath(path, content);
                if (path.FillType == Rendering.PathFillType.EvenOdd)
                    content.FillEvenOdd();
                else
                    content.Fill();
                if (hasAlpha) content.RestoreState();
            }
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

            // Counter-flip Y for images. The page CTM flips Y, so we negate height
            // and shift Y by height to draw the image right-side-up.
            content.DrawImage(pdfImage,
                destRect.Width, 0f, 0f, -destRect.Height,
                destRect.X, destRect.Y + destRect.Height);
        }

        /// <inheritdoc />
        public void DrawTiledImage(ImageData image, RectF fillArea,
            float tileWidth, float tileHeight, float originX, float originY)
        {
            float startX = originX;
            while (startX > fillArea.X)
            {
                startX -= tileWidth;
            }
            float startY = originY;
            while (startY > fillArea.Y)
            {
                startY -= tileHeight;
            }
            float endX = fillArea.X + fillArea.Width;
            float endY = fillArea.Y + fillArea.Height;

            for (float ty = startY; ty < endY; ty += tileHeight)
            {
                for (float tx = startX; tx < endX; tx += tileWidth)
                {
                    DrawImage(image, new RectF(tx, ty, tileWidth, tileHeight));
                }
            }
        }

        /// <inheritdoc />
        public float MeasureText(string text, TextStyle style) => -1f;

        /// <inheritdoc />
        public void FillRectWithTiledGradient(GradientInfo gradient, RectF fillArea, RectF tileRect)
        {
            // PDF: fall back to filling the entire area with the gradient (no tiling).
            FillRect(fillArea, BrushInfo.FromGradient(gradient));
        }

        /// <inheritdoc />
        public void DrawText(string text, float x, float y, TextStyle style)
        {
            EnsurePage();
            var content = _currentPage!.Content;

            PdfFont pdfFont = ResolvePdfFont(style.Font);

            bool hasAlpha = style.Color.A < 255;
            if (hasAlpha) content.SaveState();
            style.Color.ToFloatRgb(out float r, out float g, out float b);
            content.SetFillColor(r, g, b);
            if (hasAlpha) content.SetFillOpacity(style.Color.A / 255f);

            content.BeginText();
            content.SetFont(pdfFont, style.FontSize);

            if (style.LetterSpacing != 0)
                content.SetCharacterSpacing(style.LetterSpacing);
            if (style.WordSpacing != 0)
                content.SetWordSpacing(style.WordSpacing);

            // Counter-flip Y in text matrix to cancel the page-level Y-flip CTM.
            // CTM is [1 0 0 -1 0 h], so text matrix needs [1 0 0 -1 x y] → combined Y = 1 (upright).
            content.SetTextMatrix(1f, 0f, 0f, -1f, x, y);
            content.ShowText(pdfFont, text);

            if (style.LetterSpacing != 0)
                content.SetCharacterSpacing(0);
            if (style.WordSpacing != 0)
                content.SetWordSpacing(0);

            content.EndText();
            if (hasAlpha) content.RestoreState();
        }

        /// <inheritdoc />
        public void DrawGlyphs(ShapedTextRun run, float x, float y, CssColor color, FontDescriptor font)
        {
            EnsurePage();
            var content = _currentPage!.Content;

            // Use font data from the shaped run (carried from layout) for PDF embedding.
            PdfFont pdfFont = run.FontData != null
                ? ResolvePdfFont(font, run.FontData)
                : ResolvePdfFont(font);

            bool hasAlpha = color.A < 255;
            if (hasAlpha) content.SaveState();
            color.ToFloatRgb(out float r, out float g, out float b);
            content.SetFillColor(r, g, b);
            if (hasAlpha) content.SetFillOpacity(color.A / 255f);

            content.BeginText();
            content.SetFont(pdfFont, run.FontSize);

            // Counter-flip Y in text matrix to cancel the page-level Y-flip CTM.
            content.SetTextMatrix(1f, 0f, 0f, -1f, x, y);

            // Use the HarfBuzz-shaped glyph IDs directly instead of re-encoding
            // from text through cmap. This ensures the PDF output matches the
            // shaped glyph sequence exactly.
            var shapedGlyphs = run.Glyphs;
            int glyphCount = shapedGlyphs.Length;

            // Check whether any glyph's shaped advance differs from the font's default.
            // This happens when letter-spacing, word-spacing, or justify spacing is applied
            // via ApplySpacingToRun in TextPainter.
            float unitsPerEm = pdfFont.Metrics.UnitsPerEm;
            bool needsPositioning = false;
            for (int i = 0; i < glyphCount; i++)
            {
                ushort gid = (ushort)shapedGlyphs[i].GlyphId;
                float defaultAdvance1000 = unitsPerEm > 0
                    ? pdfFont.GetAdvanceWidth(gid) * 1000f / unitsPerEm
                    : 0f;
                float shapedAdvance1000 = run.FontSize > 0
                    ? shapedGlyphs[i].XAdvance * 1000f / run.FontSize
                    : 0f;
                float delta = defaultAdvance1000 - shapedAdvance1000;
                if (delta > 0.1f || delta < -0.1f)
                {
                    needsPositioning = true;
                    break;
                }
            }

            var glyphPositions = needsPositioning ? new GlyphPosition[glyphCount] : null;
            var glyphIds = needsPositioning ? null : new ushort[glyphCount];

            for (int i = 0; i < glyphCount; i++)
            {
                ushort gid = (ushort)shapedGlyphs[i].GlyphId;

                if (needsPositioning)
                {
                    // Compute TJ adjustment: positive = move left, negative = move right.
                    // After showing a glyph, PDF advances by defaultWidth. We want shapedAdvance,
                    // so adjustment = defaultAdvance - shapedAdvance (in 1/1000 text space units).
                    float defaultAdvance1000 = unitsPerEm > 0
                        ? pdfFont.GetAdvanceWidth(gid) * 1000f / unitsPerEm
                        : 0f;
                    float shapedAdvance1000 = run.FontSize > 0
                        ? shapedGlyphs[i].XAdvance * 1000f / run.FontSize
                        : 0f;
                    glyphPositions![i] = new GlyphPosition(gid, defaultAdvance1000 - shapedAdvance1000);
                }
                else
                {
                    glyphIds![i] = gid;
                }

                // Record glyph-to-unicode mapping for ToUnicode CMap (text extraction).
                // Use the cluster index to find the corresponding code point in the original text.
                uint cluster = shapedGlyphs[i].Cluster;
                if (cluster < (uint)run.OriginalText.Length)
                {
                    int codePoint;
                    if (char.IsHighSurrogate(run.OriginalText[(int)cluster])
                        && (int)cluster + 1 < run.OriginalText.Length
                        && char.IsLowSurrogate(run.OriginalText[(int)cluster + 1]))
                    {
                        codePoint = char.ConvertToUtf32(
                            run.OriginalText[(int)cluster],
                            run.OriginalText[(int)cluster + 1]);
                    }
                    else
                    {
                        codePoint = run.OriginalText[(int)cluster];
                    }
                    pdfFont.RecordGlyphWithUnicode(gid, codePoint);
                }
            }

            if (needsPositioning)
            {
                content.ShowGlyphsWithPositioning(pdfFont, (ReadOnlySpan<GlyphPosition>)glyphPositions);
            }
            else
            {
                content.ShowGlyphs(pdfFont, glyphIds);
            }
            content.EndText();
            if (hasAlpha) content.RestoreState();
        }

        public (float UnderlinePosition, float UnderlineThickness,
                float StrikeoutPosition, float StrikeoutThickness) GetDecorationMetrics(
            FontDescriptor font, float fontSize)
        {
            PdfFont pdfFont = ResolvePdfFont(font);
            var metrics = pdfFont.Metrics;
            float scale = metrics.UnitsPerEm > 0 ? fontSize / metrics.UnitsPerEm : 0;

            // post table: underlinePosition is negative (below baseline).
            // Skia convention: positive = below baseline. Negate to match.
            float ulPos = metrics.UnderlineThickness != 0
                ? -metrics.UnderlinePosition * scale
                : fontSize * 0.15f;
            float ulThick = metrics.UnderlineThickness != 0
                ? metrics.UnderlineThickness * scale
                : 1f;

            // OS/2 table: yStrikeoutPosition is positive (above baseline).
            // Skia convention: negative = above baseline. Negate to match.
            float stPos = metrics.StrikeoutSize != 0
                ? -metrics.StrikeoutPosition * scale
                : -fontSize * 0.3f;
            float stThick = metrics.StrikeoutSize != 0
                ? metrics.StrikeoutSize * scale
                : 1f;

            return (ulPos, ulThick, stPos, stThick);
        }

        /// <inheritdoc />
        public (float Ascent, float Descent) GetFontMetrics(FontDescriptor font, float fontSize)
        {
            PdfFont pdfFont = ResolvePdfFont(font);
            var metrics = pdfFont.Metrics;
            float scale = metrics.UnitsPerEm > 0 ? fontSize / metrics.UnitsPerEm : 0;
            float ascent = (float)Math.Round(metrics.Ascent * scale, MidpointRounding.AwayFromZero);
            float descent = (float)Math.Round(-metrics.Descent * scale, MidpointRounding.AwayFromZero);
            return (ascent, descent);
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

        /// <summary>
        /// Sets the font provider for resolving font data when embedding fonts in PDF.
        /// Called by the render pipeline after construction.
        /// </summary>
        internal void SetFontProvider(IFontProvider fontProvider)
        {
            _fontProvider = fontProvider;
        }

        private PdfFont ResolvePdfFont(FontDescriptor descriptor)
        {
            byte[]? fontData = null;
            if (_fontProvider != null)
            {
                var entry = _fontProvider.ResolveFont(descriptor);
                if (entry != null)
                    fontData = entry.FontData;
            }
            var embedMode = _options.DocumentOptions?.FontEmbedMode ?? Rend.Pdf.FontEmbedMode.Subset;
            return _fontCache.GetOrAdd(descriptor, fontData, _doc, embedMode);
        }

        private PdfFont ResolvePdfFont(FontDescriptor descriptor, byte[]? fontData)
        {
            var embedMode = _options.DocumentOptions?.FontEmbedMode ?? Rend.Pdf.FontEmbedMode.Subset;
            return _fontCache.GetOrAdd(descriptor, fontData, _doc, embedMode);
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
                    if (brush.Gradient.Stops[0].Color.A < 255)
                        content.SetFillOpacity(brush.Gradient.Stops[0].Color.A / 255f);
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
                if (brush.Color.A < 255)
                    content.SetFillOpacity(brush.Color.A / 255f);
            }
        }

        private static void SetStrokeFromPen(PenInfo pen, PdfContentStream content)
        {
            pen.Color.ToFloatRgb(out float r, out float g, out float b);
            content.SetStrokeColor(r, g, b);
            if (pen.Color.A < 255)
                content.SetStrokeOpacity(pen.Color.A / 255f);
            content.SetLineWidth(pen.Width);

            if (pen.DashPattern != null && pen.DashPattern.Length > 0)
            {
                content.SetDashPattern(pen.DashPattern, pen.DashOffset);
            }
        }

        private static bool BrushHasAlpha(BrushInfo brush)
        {
            if (brush.Gradient != null && brush.Gradient.Stops.Length > 0)
            {
                for (int i = 0; i < brush.Gradient.Stops.Length; i++)
                    if (brush.Gradient.Stops[i].Color.A < 255) return true;
                return false;
            }
            return brush.Color.A < 255;
        }

        /// <summary>
        /// Renders a Gaussian-blurred outer box shadow by rasterizing it as a PNG image
        /// and embedding it in the PDF. This produces smooth, artifact-free shadows.
        /// </summary>
        private void PaintBlurredOuterShadow(RectF rect, BrushInfo brush, PdfContentStream content)
        {
            float sigma = _maskBlurSigma;
            float pad = (float)System.Math.Ceiling(sigma * 3f);

            CssColor baseColor = brush.Color;
            byte r = baseColor.R, g = baseColor.G, b = baseColor.B, a = baseColor.A;

            // Image covers rect + padding on all sides
            // Use 1 CSS px = 1 image pixel (sufficient for blurred content)
            int imgW = (int)System.Math.Ceiling(rect.Width + pad * 2);
            int imgH = (int)System.Math.Ceiling(rect.Height + pad * 2);
            if (imgW < 1 || imgH < 1) return;

            // Clamp image size to prevent excessive memory usage
            if (imgW > 2000) imgW = 2000;
            if (imgH > 2000) imgH = 2000;

            float scaleX = (rect.Width + pad * 2) / imgW;
            float scaleY = (rect.Height + pad * 2) / imgH;

            // Create RGBA pixel buffer: fill rect area with shadow color, then blur alpha
            byte[] pixels = new byte[imgW * imgH * 4];

            // Rect bounds in image coordinates
            int rx0 = (int)(pad / scaleX);
            int ry0 = (int)(pad / scaleY);
            int rx1 = (int)((pad + rect.Width) / scaleX);
            int ry1 = (int)((pad + rect.Height) / scaleY);
            if (rx1 > imgW) rx1 = imgW;
            if (ry1 > imgH) ry1 = imgH;

            // Fill the rectangle area with full alpha
            for (int y = ry0; y < ry1; y++)
            {
                for (int x = rx0; x < rx1; x++)
                {
                    int idx = (y * imgW + x) * 4;
                    pixels[idx] = r;
                    pixels[idx + 1] = g;
                    pixels[idx + 2] = b;
                    pixels[idx + 3] = a;
                }
            }

            // Apply separable Gaussian blur on alpha channel
            GaussianBlurAlpha(pixels, imgW, imgH, sigma / scaleX);

            // Set RGB on all pixels (blur only affected alpha)
            for (int i = 0; i < pixels.Length; i += 4)
            {
                if (pixels[i + 3] > 0)
                {
                    pixels[i] = r;
                    pixels[i + 1] = g;
                    pixels[i + 2] = b;
                }
            }

            // Encode as PNG and embed
            byte[] pngBytes = EncodePngRgba(pixels, imgW, imgH);
            var image = _doc.AddImage(pngBytes, ImageFormat.Png);

            // Draw at the shadow position (rect - padding)
            var destRect = new RectF(
                rect.X - pad,
                rect.Y - pad,
                rect.Width + pad * 2,
                rect.Height + pad * 2);
            content.DrawImage(image, destRect);
        }

        /// <summary>
        /// Renders a Gaussian-blurred inset box shadow by rasterizing it as a PNG image.
        /// Inset shadow: shadow color fills outside the inner rect, blurs inward.
        /// The result is drawn over the entire outer rect (element area), clipped to it.
        /// </summary>
        private void PaintBlurredInsetShadow(PathData path, BrushInfo brush, PdfContentStream content)
        {
            float sigma = _maskBlurSigma;

            var segs = path.GetSegments();
            if (segs.Count < 10)
            {
                WritePath(path, content);
                content.FillEvenOdd();
                return;
            }

            // Extract outer rect (element boundary) from first sub-path
            float ox = segs[0].X, oy = segs[0].Y;
            float ox2 = segs[1].X, oy2 = segs[2].Y;
            var outerRect = new RectF(ox, oy, ox2 - ox, oy2 - oy);

            // Extract inner rect (the hole / non-shadow area) from second sub-path
            float ix = segs[5].X, iy = segs[5].Y;
            float ix2 = segs[6].X, iy2 = segs[7].Y;
            var innerRect = new RectF(ix, iy, ix2 - ix, iy2 - iy);

            CssColor baseColor = brush.Color;
            byte r = baseColor.R, g = baseColor.G, b = baseColor.B, a = baseColor.A;

            // Image covers the outer rect (element) plus padding so blur doesn't clip at edges
            int padPx = (int)System.Math.Ceiling(sigma * 3f);
            int imgW = (int)System.Math.Ceiling(outerRect.Width) + padPx * 2;
            int imgH = (int)System.Math.Ceiling(outerRect.Height) + padPx * 2;
            if (imgW < 1 || imgH < 1) return;
            if (imgW > 2000) imgW = 2000;
            if (imgH > 2000) imgH = 2000;

            float scaleX = (outerRect.Width + padPx * 2) / imgW;
            float scaleY = (outerRect.Height + padPx * 2) / imgH;

            byte[] pixels = new byte[imgW * imgH * 4];

            // Fill everything with shadow color (the "wall" surrounding the element)
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = r;
                pixels[i + 1] = g;
                pixels[i + 2] = b;
                pixels[i + 3] = a;
            }

            // Clear the inner rect (the hole) — this is the non-shadow center area
            // Convert innerRect from page coords to image coords
            int hx0 = (int)((innerRect.X - outerRect.X + padPx) / scaleX);
            int hy0 = (int)((innerRect.Y - outerRect.Y + padPx) / scaleY);
            int hx1 = (int)((innerRect.X + innerRect.Width - outerRect.X + padPx) / scaleX);
            int hy1 = (int)((innerRect.Y + innerRect.Height - outerRect.Y + padPx) / scaleY);
            if (hx0 < 0) hx0 = 0;
            if (hy0 < 0) hy0 = 0;
            if (hx1 > imgW) hx1 = imgW;
            if (hy1 > imgH) hy1 = imgH;

            for (int y = hy0; y < hy1; y++)
            {
                for (int x = hx0; x < hx1; x++)
                {
                    int idx = (y * imgW + x) * 4;
                    pixels[idx + 3] = 0;
                }
            }

            // Blur alpha channel
            GaussianBlurAlpha(pixels, imgW, imgH, sigma / scaleX);

            // Set RGB on all blurred pixels
            for (int i = 0; i < pixels.Length; i += 4)
            {
                if (pixels[i + 3] > 0)
                {
                    pixels[i] = r;
                    pixels[i + 1] = g;
                    pixels[i + 2] = b;
                }
            }

            // Crop to the outer rect area (strip the padding)
            int cropW = (int)System.Math.Ceiling(outerRect.Width);
            int cropH = (int)System.Math.Ceiling(outerRect.Height);
            if (cropW > imgW) cropW = imgW;
            if (cropH > imgH) cropH = imgH;
            int cropX0 = padPx;
            int cropY0 = padPx;

            byte[] cropped = new byte[cropW * cropH * 4];
            for (int y = 0; y < cropH; y++)
            {
                int srcY = cropY0 + y;
                if (srcY >= imgH) break;
                int copyLen = cropW * 4;
                int srcOff = (srcY * imgW + cropX0) * 4;
                int dstOff = y * cropW * 4;
                if (srcOff + copyLen <= pixels.Length && dstOff + copyLen <= cropped.Length)
                    Buffer.BlockCopy(pixels, srcOff, cropped, dstOff, copyLen);
            }

            byte[] pngBytes = EncodePngRgba(cropped, cropW, cropH);
            var image = _doc.AddImage(pngBytes, ImageFormat.Png);

            // Draw clipped to the outer rect (the element boundary)
            content.SaveState();
            content.Rectangle(outerRect.X, outerRect.Y, outerRect.Width, outerRect.Height);
            content.Clip();
            content.EndPath();
            content.DrawImage(image, outerRect);
            content.RestoreState();
        }

        /// <summary>
        /// Separable Gaussian blur on the alpha channel of an RGBA pixel buffer.
        /// </summary>
        private static void GaussianBlurAlpha(byte[] pixels, int w, int h, float sigma)
        {
            if (sigma < 0.5f) return;

            // Build 1D Gaussian kernel
            int radius = (int)System.Math.Ceiling(sigma * 3f);
            if (radius < 1) radius = 1;
            float[] kernel = new float[radius * 2 + 1];
            float sum = 0;
            for (int i = -radius; i <= radius; i++)
            {
                float v = (float)System.Math.Exp(-(i * i) / (2f * sigma * sigma));
                kernel[i + radius] = v;
                sum += v;
            }
            for (int i = 0; i < kernel.Length; i++)
                kernel[i] /= sum;

            // Horizontal pass: alpha channel only
            byte[] temp = new byte[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float acc = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        int sx = x + k;
                        if (sx < 0) sx = 0;
                        else if (sx >= w) sx = w - 1;
                        acc += pixels[(y * w + sx) * 4 + 3] * kernel[k + radius];
                    }
                    temp[y * w + x] = (byte)(acc + 0.5f);
                }
            }

            // Vertical pass
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float acc = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        int sy = y + k;
                        if (sy < 0) sy = 0;
                        else if (sy >= h) sy = h - 1;
                        acc += temp[sy * w + x] * kernel[k + radius];
                    }
                    int val = (int)(acc + 0.5f);
                    if (val > 255) val = 255;
                    pixels[(y * w + x) * 4 + 3] = (byte)val;
                }
            }
        }

        /// <summary>
        /// Minimal PNG encoder for RGBA pixel data.
        /// </summary>
        private static byte[] EncodePngRgba(byte[] rgba, int width, int height)
        {
            using (var ms = new MemoryStream())
            {
                // PNG signature
                ms.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, 8);

                // IHDR
                byte[] ihdr = new byte[13];
                WriteBigEndian32(ihdr, 0, width);
                WriteBigEndian32(ihdr, 4, height);
                ihdr[8] = 8;  // bit depth
                ihdr[9] = 6;  // RGBA
                ihdr[10] = 0; // compression
                ihdr[11] = 0; // filter
                ihdr[12] = 0; // interlace
                WritePngChunk(ms, "IHDR", ihdr);

                // IDAT — deflate-compressed scanlines with filter byte 0 (None) per row
                byte[] rawData;
                using (var dataMs = new MemoryStream())
                {
                    using (var deflate = new DeflateStream(dataMs, CompressionLevel.Fastest, leaveOpen: true))
                    {
                        byte[] filterByte = new byte[] { 0 };
                        int stride = width * 4;
                        for (int y = 0; y < height; y++)
                        {
                            deflate.Write(filterByte, 0, 1);
                            deflate.Write(rgba, y * stride, stride);
                        }
                    }
                    rawData = dataMs.ToArray();
                }

                // Wrap in zlib: header (0x78 0x01) + deflate data + Adler32
                byte[] zlibData;
                using (var zlibMs = new MemoryStream())
                {
                    zlibMs.WriteByte(0x78);
                    zlibMs.WriteByte(0x01);
                    zlibMs.Write(rawData, 0, rawData.Length);

                    // Compute Adler-32 of uncompressed data
                    uint adler = Adler32(rgba, width, height);
                    byte[] adlerBytes = new byte[4];
                    WriteBigEndian32(adlerBytes, 0, (int)adler);
                    zlibMs.Write(adlerBytes, 0, 4);
                    zlibData = zlibMs.ToArray();
                }

                WritePngChunk(ms, "IDAT", zlibData);

                // IEND
                WritePngChunk(ms, "IEND", Array.Empty<byte>());

                return ms.ToArray();
            }
        }

        private static uint Adler32(byte[] rgba, int width, int height)
        {
            uint a = 1, b = 0;
            int stride = width * 4;
            for (int y = 0; y < height; y++)
            {
                // filter byte = 0
                a = (a + 0) % 65521;
                b = (b + a) % 65521;
                // row data
                for (int x = 0; x < stride; x++)
                {
                    a = (a + rgba[y * stride + x]) % 65521;
                    b = (b + a) % 65521;
                }
            }
            return (b << 16) | a;
        }

        private static void WritePngChunk(Stream s, string type, byte[] data)
        {
            byte[] lenBytes = new byte[4];
            WriteBigEndian32(lenBytes, 0, data.Length);
            s.Write(lenBytes, 0, 4);

            byte[] typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
            s.Write(typeBytes, 0, 4);
            s.Write(data, 0, data.Length);

            // CRC32 over type + data
            uint crc = Crc32(typeBytes, data);
            byte[] crcBytes = new byte[4];
            WriteBigEndian32(crcBytes, 0, (int)crc);
            s.Write(crcBytes, 0, 4);
        }

        private static void WriteBigEndian32(byte[] buf, int offset, int value)
        {
            buf[offset] = (byte)((value >> 24) & 0xFF);
            buf[offset + 1] = (byte)((value >> 16) & 0xFF);
            buf[offset + 2] = (byte)((value >> 8) & 0xFF);
            buf[offset + 3] = (byte)(value & 0xFF);
        }

        private static uint Crc32(byte[] type, byte[] data)
        {
            // CRC-32 per PNG spec (ISO 3309)
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < type.Length; i++)
                crc = Crc32Update(crc, type[i]);
            for (int i = 0; i < data.Length; i++)
                crc = Crc32Update(crc, data[i]);
            return crc ^ 0xFFFFFFFF;
        }

        private static uint Crc32Update(uint crc, byte b)
        {
            crc ^= b;
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 1) != 0)
                    crc = (crc >> 1) ^ 0xEDB88320;
                else
                    crc >>= 1;
            }
            return crc;
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
