using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Pdf.Internal;

namespace Rend.Pdf
{
    /// <summary>
    /// PDF content stream — the drawing API for a page.
    /// Wraps an internal <see cref="ContentStreamBuilder"/> and tracks resource usage.
    /// </summary>
    public sealed class PdfContentStream
    {
        private readonly ContentStreamBuilder _builder;
        private readonly Dictionary<string, PdfFont> _usedFonts = new Dictionary<string, PdfFont>();
        private readonly Dictionary<string, string> _fontResourceNames = new Dictionary<string, string>();
        private readonly Dictionary<string, PdfImage> _usedImages = new Dictionary<string, PdfImage>();
        private readonly Dictionary<string, PdfReference> _extGStates = new Dictionary<string, PdfReference>();
        private readonly PdfObjectTable _objectTable;
        private readonly bool _compress;
        private int _graphicsStateDepth;

        internal PdfContentStream(PdfObjectTable objectTable, bool compress, int bufferSize)
        {
            _objectTable = objectTable;
            _compress = compress;
            _builder = new ContentStreamBuilder(bufferSize);
        }

        internal IReadOnlyDictionary<string, PdfFont> UsedFonts => _usedFonts;
        internal IReadOnlyDictionary<string, string> FontResourceNames => _fontResourceNames;
        internal IReadOnlyDictionary<string, PdfImage> UsedImages => _usedImages;
        internal IReadOnlyDictionary<string, PdfReference> ExtGStates => _extGStates;

        /// <summary>
        /// Build the content stream PDF object. Called internally during Save().
        /// </summary>
        internal PdfStream Build()
        {
            if (_graphicsStateDepth != 0)
                throw new InvalidOperationException(
                    $"Unbalanced graphics state: {_graphicsStateDepth} unclosed SaveState() calls.");

            return new PdfStream(_builder.ToArray(), _compress);
        }

        // ═══════════════════════════════════════════
        // Graphics State
        // ═══════════════════════════════════════════

        /// <summary>Push the graphics state stack (PDF operator: q).</summary>
        public void SaveState()
        {
            _graphicsStateDepth++;
            _builder.SaveState();
        }

        /// <summary>Pop the graphics state stack (PDF operator: Q).</summary>
        public void RestoreState()
        {
            if (_graphicsStateDepth <= 0)
                throw new InvalidOperationException("RestoreState() without matching SaveState().");
            _graphicsStateDepth--;
            _builder.RestoreState();
        }

        /// <summary>Set the current transformation matrix (PDF operator: cm).</summary>
        public void SetTransform(float a, float b, float c, float d, float e, float f)
            => _builder.SetTransform(a, b, c, d, e, f);

        /// <summary>Translate the coordinate origin.</summary>
        public void Translate(float tx, float ty)
            => _builder.SetTransform(1, 0, 0, 1, tx, ty);

        /// <summary>Scale the coordinate system.</summary>
        public void Scale(float sx, float sy)
            => _builder.SetTransform(sx, 0, 0, sy, 0, 0);

        /// <summary>Rotate the coordinate system (degrees).</summary>
        public void Rotate(float angleDegrees)
        {
            float rad = angleDegrees * (float)(Math.PI / 180.0);
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);
            _builder.SetTransform(cos, sin, -sin, cos, 0, 0);
        }

        /// <summary>Set line width (PDF operator: w).</summary>
        public void SetLineWidth(float width) => _builder.SetLineWidth(width);

        /// <summary>Set line cap style (PDF operator: J).</summary>
        public void SetLineCap(LineCapStyle cap) => _builder.SetLineCap((int)cap);

        /// <summary>Set line join style (PDF operator: j).</summary>
        public void SetLineJoin(LineJoinStyle join) => _builder.SetLineJoin((int)join);

        /// <summary>Set miter limit (PDF operator: M).</summary>
        public void SetMiterLimit(float limit) => _builder.SetMiterLimit(limit);

        /// <summary>Set dash pattern (PDF operator: d).</summary>
        public void SetDashPattern(float[] pattern, float phase) => _builder.SetDashPattern(pattern, phase);

        // ═══════════════════════════════════════════
        // Color
        // ═══════════════════════════════════════════

        /// <summary>Set fill color (DeviceRGB, 0.0-1.0).</summary>
        public void SetFillColor(float r, float g, float b) => _builder.SetFillColorRgb(r, g, b);

        /// <summary>Set fill color from a CssColor.</summary>
        public void SetFillColor(CssColor color)
        {
            color.ToFloatRgb(out float r, out float g, out float b);
            _builder.SetFillColorRgb(r, g, b);
        }

        /// <summary>Set stroke color (DeviceRGB, 0.0-1.0).</summary>
        public void SetStrokeColor(float r, float g, float b) => _builder.SetStrokeColorRgb(r, g, b);

        /// <summary>Set stroke color from a CssColor.</summary>
        public void SetStrokeColor(CssColor color)
        {
            color.ToFloatRgb(out float r, out float g, out float b);
            _builder.SetStrokeColorRgb(r, g, b);
        }

        /// <summary>Set fill color (DeviceCMYK, 0.0-1.0).</summary>
        public void SetFillColorCmyk(float c, float m, float y, float k) => _builder.SetFillColorCmyk(c, m, y, k);

        /// <summary>Set stroke color (DeviceCMYK, 0.0-1.0).</summary>
        public void SetStrokeColorCmyk(float c, float m, float y, float k) => _builder.SetStrokeColorCmyk(c, m, y, k);

        /// <summary>Set fill color (DeviceGray, 0.0-1.0).</summary>
        public void SetFillColorGray(float gray) => _builder.SetFillColorGray(gray);

        /// <summary>Set stroke color (DeviceGray, 0.0-1.0).</summary>
        public void SetStrokeColorGray(float gray) => _builder.SetStrokeColorGray(gray);

        /// <summary>Set fill opacity (0.0 = transparent, 1.0 = opaque). Uses ExtGState.</summary>
        public void SetFillOpacity(float opacity)
        {
            string key = $"ca{opacity:F2}";
            if (!_extGStates.ContainsKey(key))
            {
                var gsDict = new PdfDictionary(2);
                gsDict[PdfName.Type] = new PdfName("ExtGState");
                gsDict[PdfName.ca] = new PdfReal(opacity);
                _extGStates[key] = _objectTable.Allocate(gsDict);
            }
            _builder.SetExtGState(key);
        }

        /// <summary>Set stroke opacity (0.0 = transparent, 1.0 = opaque). Uses ExtGState.</summary>
        public void SetStrokeOpacity(float opacity)
        {
            string key = $"CA{opacity:F2}";
            if (!_extGStates.ContainsKey(key))
            {
                var gsDict = new PdfDictionary(2);
                gsDict[PdfName.Type] = new PdfName("ExtGState");
                gsDict[PdfName.CA] = new PdfReal(opacity);
                _extGStates[key] = _objectTable.Allocate(gsDict);
            }
            _builder.SetExtGState(key);
        }

        // ═══════════════════════════════════════════
        // Path Construction
        // ═══════════════════════════════════════════

        /// <summary>Begin a new subpath (PDF operator: m).</summary>
        public void MoveTo(float x, float y) => _builder.MoveTo(x, y);

        /// <summary>Straight line to point (PDF operator: l).</summary>
        public void LineTo(float x, float y) => _builder.LineTo(x, y);

        /// <summary>Cubic Bézier curve (PDF operator: c).</summary>
        public void CurveTo(float cx1, float cy1, float cx2, float cy2, float x, float y)
            => _builder.CurveTo(cx1, cy1, cx2, cy2, x, y);

        /// <summary>Cubic Bézier with first control = current point (PDF operator: v).</summary>
        public void CurveToV(float cx2, float cy2, float x, float y)
            => _builder.CurveToV(cx2, cy2, x, y);

        /// <summary>Cubic Bézier with second control = endpoint (PDF operator: y).</summary>
        public void CurveToY(float cx1, float cy1, float x, float y)
            => _builder.CurveToY(cx1, cy1, x, y);

        /// <summary>Close the current subpath (PDF operator: h).</summary>
        public void ClosePath() => _builder.ClosePath();

        /// <summary>Rectangle (PDF operator: re).</summary>
        public void Rectangle(float x, float y, float width, float height)
            => _builder.Rectangle(x, y, width, height);

        /// <summary>Rectangle from RectF.</summary>
        public void Rectangle(RectF rect)
            => _builder.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>Rounded rectangle using Bézier approximation of quarter circles.</summary>
        public void RoundedRectangle(float x, float y, float w, float h, float rx, float ry)
        {
            // Bézier control point factor for circle approximation: 4*(sqrt(2)-1)/3 ≈ 0.5523
            const float k = 0.5523f;
            float kx = rx * k;
            float ky = ry * k;

            _builder.MoveTo(x + rx, y);
            _builder.LineTo(x + w - rx, y);
            _builder.CurveTo(x + w - rx + kx, y, x + w, y + ry - ky, x + w, y + ry);
            _builder.LineTo(x + w, y + h - ry);
            _builder.CurveTo(x + w, y + h - ry + ky, x + w - rx + kx, y + h, x + w - rx, y + h);
            _builder.LineTo(x + rx, y + h);
            _builder.CurveTo(x + rx - kx, y + h, x, y + h - ry + ky, x, y + h - ry);
            _builder.LineTo(x, y + ry);
            _builder.CurveTo(x, y + ry - ky, x + rx - kx, y, x + rx, y);
            _builder.ClosePath();
        }

        // ═══════════════════════════════════════════
        // Path Painting
        // ═══════════════════════════════════════════

        /// <summary>Stroke the current path (PDF operator: S).</summary>
        public void Stroke() => _builder.Stroke();

        /// <summary>Close and stroke (PDF operator: s).</summary>
        public void CloseAndStroke() => _builder.CloseAndStroke();

        /// <summary>Fill using nonzero winding rule (PDF operator: f).</summary>
        public void Fill() => _builder.Fill();

        /// <summary>Fill using even-odd rule (PDF operator: f*).</summary>
        public void FillEvenOdd() => _builder.FillEvenOdd();

        /// <summary>Fill and stroke (PDF operator: B).</summary>
        public void FillAndStroke() => _builder.FillAndStroke();

        /// <summary>Fill (even-odd) and stroke (PDF operator: B*).</summary>
        public void FillEvenOddAndStroke() => _builder.FillEvenOddAndStroke();

        /// <summary>End path without painting (PDF operator: n).</summary>
        public void EndPath() => _builder.EndPath();

        // ═══════════════════════════════════════════
        // Clipping
        // ═══════════════════════════════════════════

        /// <summary>Set clipping path, nonzero winding (PDF operator: W n).</summary>
        public void Clip() => _builder.Clip();

        /// <summary>Set clipping path, even-odd (PDF operator: W* n).</summary>
        public void ClipEvenOdd() => _builder.ClipEvenOdd();

        // ═══════════════════════════════════════════
        // Text
        // ═══════════════════════════════════════════

        /// <summary>Begin a text object (PDF operator: BT).</summary>
        public void BeginText() => _builder.BeginText();

        /// <summary>End a text object (PDF operator: ET).</summary>
        public void EndText() => _builder.EndText();

        /// <summary>Set font and size (PDF operator: Tf).</summary>
        public void SetFont(PdfFont font, float size)
        {
            string resourceName = RegisterFont(font);
            _builder.SetFont(resourceName, size);
        }

        /// <summary>Move text position (PDF operator: Td).</summary>
        public void MoveTextPosition(float tx, float ty) => _builder.MoveTextPosition(tx, ty);

        /// <summary>Set text matrix (PDF operator: Tm).</summary>
        public void SetTextMatrix(float a, float b, float c, float d, float e, float f)
            => _builder.SetTextMatrix(a, b, c, d, e, f);

        /// <summary>Show a Unicode text string. Encodes using the current font.</summary>
        public void ShowText(PdfFont font, string text)
        {
            if (font.IsStandard14)
            {
                _builder.ShowTextLiteral(text);
            }
            else
            {
                byte[] encoded = font.Encode(text);
                _builder.ShowTextHex(encoded);
            }
        }

        /// <summary>Set character spacing (PDF operator: Tc).</summary>
        public void SetCharacterSpacing(float spacing) => _builder.SetCharacterSpacing(spacing);

        /// <summary>Set word spacing (PDF operator: Tw).</summary>
        public void SetWordSpacing(float spacing) => _builder.SetWordSpacing(spacing);

        /// <summary>Set text leading (PDF operator: TL).</summary>
        public void SetTextLeading(float leading) => _builder.SetTextLeading(leading);

        /// <summary>Set text rise for superscript/subscript (PDF operator: Ts).</summary>
        public void SetTextRise(float rise) => _builder.SetTextRise(rise);

        /// <summary>Set text rendering mode (PDF operator: Tr).</summary>
        public void SetTextRenderingMode(TextRenderingMode mode) => _builder.SetTextRenderingMode((int)mode);

        /// <summary>Move to the next line (PDF operator: T*).</summary>
        public void NextLine() => _builder.NextLine();

        // ═══════════════════════════════════════════
        // Images
        // ═══════════════════════════════════════════

        /// <summary>Draw an image at the specified rectangle.</summary>
        public void DrawImage(PdfImage image, RectF destRect)
        {
            RegisterImage(image);
            _builder.SaveState();
            // Transform: translate to position, scale to size
            _builder.SetTransform(destRect.Width, 0, 0, destRect.Height, destRect.X, destRect.Y);
            _builder.PaintXObject(image.ResourceName);
            _builder.RestoreState();
        }

        /// <summary>Draw an image with an explicit transformation matrix.</summary>
        public void DrawImage(PdfImage image, float a, float b, float c, float d, float e, float f)
        {
            RegisterImage(image);
            _builder.SaveState();
            _builder.SetTransform(a, b, c, d, e, f);
            _builder.PaintXObject(image.ResourceName);
            _builder.RestoreState();
        }

        // ═══════════════════════════════════════════
        // Resource tracking
        // ═══════════════════════════════════════════

        private string RegisterFont(PdfFont font)
        {
            if (_fontResourceNames.TryGetValue(font.BaseFont, out var existingName))
                return existingName;

            string resourceName = "F" + (_usedFonts.Count + 1);
            _usedFonts[font.BaseFont] = font;
            _fontResourceNames[font.BaseFont] = resourceName;
            return resourceName;
        }

        private void RegisterImage(PdfImage image)
        {
            if (!_usedImages.ContainsKey(image.ResourceName))
            {
                _usedImages[image.ResourceName] = image;
            }
        }
    }
}
