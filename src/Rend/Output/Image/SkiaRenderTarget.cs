using System;
using System.Collections.Generic;
using System.IO;
using Rend.Core.Values;
using Rend.Fonts;
using Rend.Output.Image.Internal;
using Rend.Rendering;
using Rend.Text;
using SkiaSharp;

namespace Rend.Output.Image
{
    /// <summary>
    /// An <see cref="IRenderTarget"/> implementation that produces raster image output
    /// using SkiaSharp for drawing operations.
    /// </summary>
    public sealed class SkiaRenderTarget : IRenderTarget, IDisposable
    {
        private readonly SkiaRenderOptions _options;
        private readonly SkiaFontMapper _fontMapper = new SkiaFontMapper();
        private readonly SkiaPaintPool _paintPool = new SkiaPaintPool();
        private readonly List<byte[]> _renderedPages = new List<byte[]>();
        private readonly Stack<float> _opacityStack = new Stack<float>();

        private SKBitmap? _currentBitmap;
        private SKCanvas? _currentCanvas;
        private float _currentOpacity = 1f;
        private SKBlendMode _currentBlendMode = SKBlendMode.SrcOver;
        private SKFilterQuality _currentFilterQuality = SKFilterQuality.Medium;
        private float _dpiScale;
        private bool _disposed;

        /// <summary>
        /// Creates a new <see cref="SkiaRenderTarget"/> with the specified options.
        /// </summary>
        /// <param name="options">Rendering options, or null for defaults.</param>
        public SkiaRenderTarget(SkiaRenderOptions? options = null)
        {
            _options = options ?? new SkiaRenderOptions();
            _dpiScale = _options.Dpi / 96f;
        }

        /// <inheritdoc />
        public void BeginPage(float width, float height)
        {
            // Dispose any existing bitmap from a previous page that was not properly ended.
            FinishCurrentPage();

            int pixelWidth = Math.Max(1, (int)Math.Ceiling(width * _dpiScale));
            int pixelHeight = Math.Max(1, (int)Math.Ceiling(height * _dpiScale));

            _currentBitmap = new SKBitmap(pixelWidth, pixelHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            _currentCanvas = new SKCanvas(_currentBitmap);

            // Fill with white background.
            _currentCanvas.Clear(SKColors.White);

            // Apply DPI scaling so that drawing coordinates remain in CSS pixels.
            if (Math.Abs(_dpiScale - 1f) > 0.001f)
            {
                _currentCanvas.Scale(_dpiScale, _dpiScale);
            }

            _currentOpacity = 1f;
            _currentBlendMode = SKBlendMode.SrcOver;
            _currentFilterQuality = SKFilterQuality.Medium;
        }

        /// <inheritdoc />
        public void EndPage()
        {
            FinishCurrentPage();
        }

        /// <inheritdoc />
        public void Save()
        {
            EnsureCanvas();
            _opacityStack.Push(_currentOpacity);
            _currentCanvas!.Save();
        }

        /// <inheritdoc />
        public void Restore()
        {
            EnsureCanvas();
            _currentCanvas!.Restore();
            if (_opacityStack.Count > 0)
                _currentOpacity = _opacityStack.Pop();
        }

        /// <inheritdoc />
        public void SetTransform(Matrix3x2 transform)
        {
            EnsureCanvas();

            // SkiaSharp uses SKMatrix (3x3). Map our Matrix3x2 to it.
            // Matrix3x2 layout: x' = x*M11 + y*M21 + M31, y' = x*M12 + y*M22 + M32
            // SKMatrix layout: x' = x*ScaleX + y*SkewX + TransX, y' = x*SkewY + y*ScaleY + TransY
            var matrix = new SKMatrix
            {
                ScaleX = transform.M11,
                SkewX = transform.M21,
                TransX = transform.M31,
                SkewY = transform.M12,
                ScaleY = transform.M22,
                TransY = transform.M32,
                Persp0 = 0f,
                Persp1 = 0f,
                Persp2 = 1f
            };

            // Apply DPI scaling before the custom transform so drawing coordinates
            // remain in CSS pixels.
            if (Math.Abs(_dpiScale - 1f) > 0.001f)
            {
                var scaled = SKMatrix.CreateScale(_dpiScale, _dpiScale);
                matrix = SKMatrix.Concat(scaled, matrix);
            }

            _currentCanvas!.SetMatrix(matrix);
        }

        /// <inheritdoc />
        public void SetOpacity(float opacity)
        {
            opacity = Math.Max(0f, Math.Min(1f, opacity));
            _currentOpacity = opacity;
            if (opacity < 1f)
            {
                EnsureCanvas();
                // Use SaveLayer to create a compositing group. All subsequent draw
                // operations render to an offscreen buffer. When Restore() is called
                // the buffer is composited at this alpha.
                //
                // OpacityCompositor called Save() before us. Undo that Save and replace
                // it with a SaveLayer so the save/restore bookkeeping stays balanced.
                _currentCanvas!.Restore();
                if (_opacityStack.Count > 0)
                    _opacityStack.Pop();
                _opacityStack.Push(1f);

                using (var paint = new SKPaint())
                {
                    // Transparent white with the requested alpha — colour channels are
                    // irrelevant because SaveLayer uses only the alpha for compositing.
                    paint.Color = new SKColor(255, 255, 255, (byte)(opacity * 255));
                    _currentCanvas.SaveLayer(paint);
                }
                // Individual draw calls should not double-apply opacity.
                _currentOpacity = 1f;
            }
        }

        /// <inheritdoc />
        public void SetBlendMode(Css.CssMixBlendMode blendMode)
        {
            _currentBlendMode = MapBlendMode(blendMode);
        }

        private static SKBlendMode MapBlendMode(Css.CssMixBlendMode mode)
        {
            switch (mode)
            {
                case Css.CssMixBlendMode.Multiply: return SKBlendMode.Multiply;
                case Css.CssMixBlendMode.Screen: return SKBlendMode.Screen;
                case Css.CssMixBlendMode.Overlay: return SKBlendMode.Overlay;
                case Css.CssMixBlendMode.Darken: return SKBlendMode.Darken;
                case Css.CssMixBlendMode.Lighten: return SKBlendMode.Lighten;
                case Css.CssMixBlendMode.ColorDodge: return SKBlendMode.ColorDodge;
                case Css.CssMixBlendMode.ColorBurn: return SKBlendMode.ColorBurn;
                case Css.CssMixBlendMode.HardLight: return SKBlendMode.HardLight;
                case Css.CssMixBlendMode.SoftLight: return SKBlendMode.SoftLight;
                case Css.CssMixBlendMode.Difference: return SKBlendMode.Difference;
                case Css.CssMixBlendMode.Exclusion: return SKBlendMode.Exclusion;
                case Css.CssMixBlendMode.Hue: return SKBlendMode.Hue;
                case Css.CssMixBlendMode.Saturation: return SKBlendMode.Saturation;
                case Css.CssMixBlendMode.Color: return SKBlendMode.Color;
                case Css.CssMixBlendMode.Luminosity: return SKBlendMode.Luminosity;
                default: return SKBlendMode.SrcOver;
            }
        }

        /// <inheritdoc />
        public void SetImageRendering(Css.CssImageRendering rendering)
        {
            switch (rendering)
            {
                case Css.CssImageRendering.Pixelated:
                case Css.CssImageRendering.CrispEdges:
                    _currentFilterQuality = SKFilterQuality.None;
                    break;
                default:
                    _currentFilterQuality = SKFilterQuality.Medium;
                    break;
            }
        }

        private float _maskBlurSigma;

        /// <inheritdoc />
        public void SetMaskBlur(float sigma)
        {
            _maskBlurSigma = sigma;
        }

        /// <inheritdoc />
        public void PushClipRect(RectF rect)
        {
            EnsureCanvas();
            _currentCanvas!.Save();
            _currentCanvas.ClipRect(ToSKRect(rect));
        }

        /// <inheritdoc />
        public void PushClipPath(PathData path)
        {
            EnsureCanvas();
            _currentCanvas!.Save();
            using (var skPath = SkiaPathConverter.Convert(path))
            {
                _currentCanvas.ClipPath(skPath);
            }
        }

        /// <inheritdoc />
        public void PopClip()
        {
            EnsureCanvas();
            _currentCanvas!.Restore();
        }

        /// <inheritdoc />
        public void FillRect(RectF rect, BrushInfo brush)
        {
            EnsureCanvas();
            var paint = _paintPool.Rent();
            try
            {
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.Fill;
                ApplyBrush(paint, brush, rect);
                if (_maskBlurSigma > 0)
                    paint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, _maskBlurSigma);

                _currentCanvas!.DrawRect(ToSKRect(rect), paint);
            }
            finally
            {
                if (paint.MaskFilter != null) { paint.MaskFilter.Dispose(); paint.MaskFilter = null; }
                ClearShader(paint);
                _paintPool.Return(paint);
            }
        }

        /// <inheritdoc />
        public void StrokeRect(RectF rect, PenInfo pen)
        {
            EnsureCanvas();
            var paint = _paintPool.Rent();
            try
            {
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.Stroke;
                ApplyPen(paint, pen);

                _currentCanvas!.DrawRect(ToSKRect(rect), paint);
            }
            finally
            {
                ClearPathEffect(paint);
                _paintPool.Return(paint);
            }
        }

        /// <inheritdoc />
        public void FillPath(PathData path, BrushInfo brush)
        {
            EnsureCanvas();
            using (var skPath = SkiaPathConverter.Convert(path))
            {
                var paint = _paintPool.Rent();
                try
                {
                    paint.IsAntialias = true;
                    paint.Style = SKPaintStyle.Fill;
                    ApplyBrush(paint, brush, GetPathBounds(skPath));
                    if (_maskBlurSigma > 0)
                        paint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, _maskBlurSigma);

                    _currentCanvas!.DrawPath(skPath, paint);
                }
                finally
                {
                    if (paint.MaskFilter != null) { paint.MaskFilter.Dispose(); paint.MaskFilter = null; }
                    ClearShader(paint);
                    _paintPool.Return(paint);
                }
            }
        }

        /// <inheritdoc />
        public void StrokePath(PathData path, PenInfo pen)
        {
            EnsureCanvas();
            using (var skPath = SkiaPathConverter.Convert(path))
            {
                var paint = _paintPool.Rent();
                try
                {
                    paint.IsAntialias = true;
                    paint.Style = SKPaintStyle.Stroke;
                    ApplyPen(paint, pen);

                    _currentCanvas!.DrawPath(skPath, paint);
                }
                finally
                {
                    ClearPathEffect(paint);
                    _paintPool.Return(paint);
                }
            }
        }

        /// <inheritdoc />
        public void DrawImage(ImageData image, RectF destRect)
        {
            EnsureCanvas();

            using (var skData = SKData.CreateCopy(image.Data))
            using (var skImage = SKImage.FromEncodedData(skData))
            {
                if (skImage != null)
                {
                    var paint = _paintPool.Rent();
                    try
                    {
                        paint.IsAntialias = _currentFilterQuality != SKFilterQuality.None;
                        paint.FilterQuality = _currentFilterQuality;
                        paint.Color = new SKColor(255, 255, 255, (byte)(_currentOpacity * 255));

                        var sourceRect = new SKRect(0, 0, skImage.Width, skImage.Height);
                        _currentCanvas!.DrawImage(skImage, sourceRect, ToSKRect(destRect), paint);
                    }
                    finally
                    {
                        _paintPool.Return(paint);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void DrawText(string text, float x, float y, TextStyle style)
        {
            EnsureCanvas();
            var paint = _paintPool.Rent();
            try
            {
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.Fill;
                var tc = style.Color;
                paint.Color = new SKColor(tc.R, tc.G, tc.B, (byte)(tc.A * _currentOpacity));
                paint.TextSize = style.FontSize;

                SKTypeface typeface = _fontMapper.GetOrCreate(style.Font, null);
                paint.Typeface = typeface;

                if (style.LetterSpacing != 0 || style.WordSpacing != 0)
                {
                    // Draw characters individually with adjusted positions
                    DrawTextWithSpacing(text, x, y, paint, style.LetterSpacing, style.WordSpacing);
                }
                else
                {
                    _currentCanvas!.DrawText(text, x, y, paint);
                }
            }
            finally
            {
                _paintPool.Return(paint);
            }
        }

        /// <inheritdoc />
        public void DrawGlyphs(ShapedTextRun run, float x, float y, CssColor color, FontDescriptor font)
        {
            EnsureCanvas();

            // Fall back to DrawText with the original text, as positioning individual
            // glyphs requires the SKFont API (SkiaSharp 2.88+ canvas.DrawGlyphs).
            // Using DrawText with the original text provides acceptable results.
            var paint = _paintPool.Rent();
            try
            {
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.Fill;
                paint.Color = new SKColor(color.R, color.G, color.B, (byte)(color.A * _currentOpacity));
                paint.TextSize = run.FontSize;

                SKTypeface typeface = _fontMapper.GetOrCreate(font, null);
                paint.Typeface = typeface;

                _currentCanvas!.DrawText(run.OriginalText, x, y, paint);
            }
            finally
            {
                _paintPool.Return(paint);
            }
        }

        /// <inheritdoc />
        public void AddLink(RectF rect, string uri)
        {
            // Links are not applicable to raster image output.
        }

        /// <inheritdoc />
        public void AddBookmark(string title, int level, float yPosition)
        {
            // Bookmarks are not applicable to raster image output.
        }

        /// <inheritdoc />
        public void Finish(Stream output)
        {
            // Ensure any in-progress page is flushed.
            FinishCurrentPage();

            if (_renderedPages.Count == 0)
            {
                return;
            }

            // For single page, write the page directly.
            // For multi-page, write only the first page (image formats are single-page).
            byte[] data = _renderedPages[0];
            output.Write(data, 0, data.Length);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                DisposeCurrentBitmap();
                _fontMapper.Dispose();
                _paintPool.Dispose();
            }
        }

        // -------------------------------------------------------
        // Private helpers
        // -------------------------------------------------------

        private void EnsureCanvas()
        {
            if (_currentCanvas == null)
            {
                throw new InvalidOperationException(
                    "No page is currently active. Call BeginPage before issuing drawing commands.");
            }
        }

        private void FinishCurrentPage()
        {
            if (_currentCanvas != null && _currentBitmap != null)
            {
                _currentCanvas.Flush();
                byte[] encoded = EncodeBitmap(_currentBitmap);
                _renderedPages.Add(encoded);
                DisposeCurrentBitmap();
            }
        }

        private void DisposeCurrentBitmap()
        {
            _currentCanvas?.Dispose();
            _currentCanvas = null;
            _currentBitmap?.Dispose();
            _currentBitmap = null;
        }

        private byte[] EncodeBitmap(SKBitmap bitmap)
        {
            using (var image = SKImage.FromBitmap(bitmap))
            {
                SKEncodedImageFormat format = ParseFormat(_options.Format);
                using (var data = image.Encode(format, _options.Quality))
                {
                    return data.ToArray();
                }
            }
        }

        private static SKEncodedImageFormat ParseFormat(string format)
        {
            if (string.Equals(format, "jpeg", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(format, "jpg", StringComparison.OrdinalIgnoreCase))
            {
                return SKEncodedImageFormat.Jpeg;
            }
            if (string.Equals(format, "webp", StringComparison.OrdinalIgnoreCase))
            {
                return SKEncodedImageFormat.Webp;
            }
            return SKEncodedImageFormat.Png;
        }

        private void ApplyBrush(SKPaint paint, BrushInfo brush, RectF bounds)
        {
            paint.BlendMode = _currentBlendMode;
            byte alpha = (byte)(_currentOpacity * 255);

            if (brush.Gradient != null && brush.Gradient.Stops.Length > 0)
            {
                var shader = SkiaGradientBuilder.CreateShader(brush.Gradient, bounds);
                if (shader != null)
                {
                    paint.Shader = shader;
                    paint.Color = new SKColor(255, 255, 255, alpha);
                    return;
                }

                // Fallback: use first stop color.
                GradientStop first = brush.Gradient.Stops[0];
                paint.Color = new SKColor(first.Color.R, first.Color.G, first.Color.B, (byte)(first.Color.A * _currentOpacity));
            }
            else
            {
                paint.Color = new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, (byte)(brush.Color.A * _currentOpacity));
            }
        }

        private void ApplyPen(SKPaint paint, PenInfo pen)
        {
            paint.BlendMode = _currentBlendMode;
            paint.Color = new SKColor(pen.Color.R, pen.Color.G, pen.Color.B, (byte)(pen.Color.A * _currentOpacity));
            paint.StrokeWidth = pen.Width;

            if (pen.DashPattern != null && pen.DashPattern.Length > 0)
            {
                // SKPathEffect.CreateDash requires an even-length array.
                float[] pattern = pen.DashPattern;
                if (pattern.Length % 2 != 0)
                {
                    // Duplicate the array to make it even-length per Skia requirements.
                    var evenPattern = new float[pattern.Length * 2];
                    Array.Copy(pattern, 0, evenPattern, 0, pattern.Length);
                    Array.Copy(pattern, 0, evenPattern, pattern.Length, pattern.Length);
                    pattern = evenPattern;
                }

                var effect = SKPathEffect.CreateDash(pattern, pen.DashOffset);
                paint.PathEffect = effect;
            }
        }

        private static void ClearShader(SKPaint paint)
        {
            if (paint.Shader != null)
            {
                paint.Shader.Dispose();
                paint.Shader = null;
            }
        }

        private static void ClearPathEffect(SKPaint paint)
        {
            if (paint.PathEffect != null)
            {
                paint.PathEffect.Dispose();
                paint.PathEffect = null;
            }
        }

        private static SKRect ToSKRect(RectF rect)
        {
            return new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom);
        }

        private static SKColor ToSKColor(CssColor color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        private void DrawTextWithSpacing(string text, float x, float y, SKPaint paint,
                                           float letterSpacing, float wordSpacing)
        {
            float cursorX = x;
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                string s = ch.ToString();
                _currentCanvas!.DrawText(s, cursorX, y, paint);
                float advance = paint.MeasureText(s);
                cursorX += advance;

                if (i < text.Length - 1)
                    cursorX += letterSpacing;
                if (ch == ' ')
                    cursorX += wordSpacing;
            }
        }

        private static RectF GetPathBounds(SKPath path)
        {
            SKRect bounds = path.Bounds;
            return new RectF(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
        }
    }
}
