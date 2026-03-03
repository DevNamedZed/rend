using System;
using System.Collections.Generic;
using System.IO.Compression;
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
        private readonly Dictionary<string, PdfReference> _shadings = new Dictionary<string, PdfReference>();
        private readonly Dictionary<string, PdfReference> _colorSpaces = new Dictionary<string, PdfReference>();
        private readonly Dictionary<string, PdfTilingPattern> _patterns = new Dictionary<string, PdfTilingPattern>();
        private readonly PdfObjectTable _objectTable;
        private readonly bool _compress;
        private readonly CompressionLevel _compressionLevel;
        private int _graphicsStateDepth;
        private bool _inTextObject;
        private int _shadingCounter;
        private int _patternCounter;

        internal PdfContentStream(PdfObjectTable objectTable, bool compress, int bufferSize,
                                   CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            _objectTable = objectTable;
            _compress = compress;
            _compressionLevel = compressionLevel;
            _builder = new ContentStreamBuilder(bufferSize);
        }

        internal IReadOnlyDictionary<string, PdfFont> UsedFonts => _usedFonts;
        internal IReadOnlyDictionary<string, string> FontResourceNames => _fontResourceNames;
        internal IReadOnlyDictionary<string, PdfImage> UsedImages => _usedImages;
        internal IReadOnlyDictionary<string, PdfReference> ExtGStates => _extGStates;
        internal IReadOnlyDictionary<string, PdfReference> Shadings => _shadings;
        internal IReadOnlyDictionary<string, PdfReference> ColorSpaces => _colorSpaces;
        internal IReadOnlyDictionary<string, PdfTilingPattern> Patterns => _patterns;

        /// <summary>
        /// Build the content stream PDF object. Called internally during Save().
        /// </summary>
        internal PdfStream Build()
        {
            if (_graphicsStateDepth != 0)
                throw new InvalidOperationException(
                    $"Unbalanced graphics state: {_graphicsStateDepth} unclosed SaveState() calls.");

            if (_inTextObject)
                throw new InvalidOperationException(
                    "Unclosed text object: BeginText() was called without a matching EndText().");

            return new PdfStream(_builder.ToArray(), _compress) { CompressionLevel = _compressionLevel };
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

        /// <summary>Concatenate a transform with the current CTM (same as cm operator).</summary>
        public void ConcatTransform(float a, float b, float c, float d, float e, float f)
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

        // ═══════════════════════════════════════════
        // Marked Content
        // ═══════════════════════════════════════════

        private int _markedContentDepth;
        private int _nextMcid;

        /// <summary>Begin a marked content sequence (BMC operator).</summary>
        public void BeginMarkedContent(string tag)
        {
            _markedContentDepth++;
            _builder.BeginMarkedContent(tag);
        }

        /// <summary>Begin a marked content sequence with MCID (BDC operator). Returns the MCID assigned.</summary>
        public int BeginMarkedContentDict(string tag)
        {
            int mcid = _nextMcid++;
            _markedContentDepth++;
            _builder.BeginMarkedContentDict(tag, mcid);
            return mcid;
        }

        /// <summary>End a marked content sequence (EMC operator).</summary>
        public void EndMarkedContent()
        {
            if (_markedContentDepth <= 0)
                throw new InvalidOperationException("EndMarkedContent() without matching BeginMarkedContent().");
            _markedContentDepth--;
            _builder.EndMarkedContent();
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

        /// <summary>Set fill color using a spot (separation) color.</summary>
        /// <param name="spotColor">The spot color registered with PdfDocument.AddSpotColor().</param>
        /// <param name="tint">Tint value from 0.0 (no ink / white) to 1.0 (full ink).</param>
        public void SetFillSpotColor(PdfSpotColor spotColor, float tint)
        {
            if (spotColor == null) throw new ArgumentNullException(nameof(spotColor));
            if (spotColor.ColorSpaceRef == null)
                throw new InvalidOperationException("Spot color has not been registered with a PdfDocument.");
            RegisterColorSpace(spotColor);
            _builder.SetFillColorSpace(spotColor.ResourceName);
            _builder.SetFillColorScn(Math.Max(0f, Math.Min(1f, tint)));
        }

        /// <summary>Set stroke color using a spot (separation) color.</summary>
        /// <param name="spotColor">The spot color registered with PdfDocument.AddSpotColor().</param>
        /// <param name="tint">Tint value from 0.0 (no ink / white) to 1.0 (full ink).</param>
        public void SetStrokeSpotColor(PdfSpotColor spotColor, float tint)
        {
            if (spotColor == null) throw new ArgumentNullException(nameof(spotColor));
            if (spotColor.ColorSpaceRef == null)
                throw new InvalidOperationException("Spot color has not been registered with a PdfDocument.");
            RegisterColorSpace(spotColor);
            _builder.SetStrokeColorSpace(spotColor.ResourceName);
            _builder.SetStrokeColorScn(Math.Max(0f, Math.Min(1f, tint)));
        }

        private void RegisterColorSpace(PdfSpotColor spotColor)
        {
            if (!_colorSpaces.ContainsKey(spotColor.ResourceName))
            {
                _colorSpaces[spotColor.ResourceName] = spotColor.ColorSpaceRef!;
            }
        }

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

        /// <summary>Set blend mode via ExtGState. Uses PDF blend mode names (e.g. "Multiply").</summary>
        public void SetBlendMode(string blendModeName)
        {
            string key = $"BM_{blendModeName}";
            if (!_extGStates.ContainsKey(key))
            {
                var gsDict = new PdfDictionary(2);
                gsDict[PdfName.Type] = new PdfName("ExtGState");
                gsDict[new PdfName("BM")] = new PdfName(blendModeName);
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
        public void BeginText()
        {
            if (_inTextObject)
                throw new InvalidOperationException("Nested text objects are not allowed. EndText() must be called before another BeginText().");
            _inTextObject = true;
            _builder.BeginText();
        }

        /// <summary>End a text object (PDF operator: ET).</summary>
        public void EndText()
        {
            if (!_inTextObject)
                throw new InvalidOperationException("EndText() called without a matching BeginText().");
            _inTextObject = false;
            _builder.EndText();
        }

        /// <summary>Set font and size (PDF operator: Tf).</summary>
        public void SetFont(PdfFont font, float size)
        {
            string resourceName = RegisterFont(font);
            _builder.SetFont(resourceName, size);
        }

        /// <summary>Move text position (PDF operator: Td).</summary>
        public void MoveTextPosition(float tx, float ty)
        {
            EnsureInTextObject();
            _builder.MoveTextPosition(tx, ty);
        }

        /// <summary>Set text matrix (PDF operator: Tm).</summary>
        public void SetTextMatrix(float a, float b, float c, float d, float e, float f)
        {
            EnsureInTextObject();
            _builder.SetTextMatrix(a, b, c, d, e, f);
        }

        /// <summary>Show a Unicode text string. Encodes using the current font.</summary>
        public void ShowText(PdfFont font, string text)
        {
            EnsureInTextObject();
            if (font.IsStandard14 || font.IsType1)
            {
                _builder.ShowTextLiteral(text);
            }
            else
            {
                byte[] encoded = font.Encode(text);
                _builder.ShowTextHex(encoded);
            }
        }

        /// <summary>
        /// Show text with individual position adjustments (PDF operator: TJ).
        /// Adjustments are in thousandths of a unit of text space.
        /// </summary>
        public void ShowTextWithPositioning(PdfFont font, params TextPositionEntry[] entries)
        {
            EnsureInTextObject();
            if (entries == null || entries.Length == 0) return;

            // Convert entries to the byte[][] + float[] format expected by ContentStreamBuilder
            var textSegments = new System.Collections.Generic.List<byte[]>();
            var adjustments = new System.Collections.Generic.List<float>();

            foreach (var entry in entries)
            {
                if (entry.HasText)
                {
                    byte[] encoded;
                    if (font.IsStandard14 || font.IsType1)
                    {
                        // Standard14 and Type 1 fonts use literal encoding (1 byte per char)
                        var text = entry.Text!;
                        encoded = new byte[text.Length];
                        for (int i = 0; i < text.Length; i++)
                            encoded[i] = (byte)text[i];
                    }
                    else
                    {
                        encoded = font.Encode(entry.Text!);
                    }
                    textSegments.Add(encoded);
                }
                if (entry.HasAdjustment)
                {
                    adjustments.Add(entry.Adjustment);
                }
            }

            _builder.ShowTextWithPositioning(textSegments.ToArray(), adjustments.ToArray());
        }

        /// <summary>
        /// Show glyphs by glyph IDs (PDF operator: Tj with hex encoding).
        /// Records glyph usage for subsetting.
        /// </summary>
        public void ShowGlyphs(PdfFont font, ReadOnlySpan<ushort> glyphIds)
        {
            EnsureInTextObject();
            if (glyphIds.Length == 0) return;

            var encoded = new byte[glyphIds.Length * 2];
            for (int i = 0; i < glyphIds.Length; i++)
            {
                font.RecordGlyphUsage(glyphIds[i]);
                PdfFont.EncodeGlyphId(glyphIds[i], encoded, i * 2);
            }
            _builder.ShowTextHex(encoded);
        }

        /// <summary>
        /// Show glyphs with individual positioning adjustments (PDF operator: TJ).
        /// Records glyph usage for subsetting.
        /// </summary>
        public void ShowGlyphsWithPositioning(PdfFont font, ReadOnlySpan<GlyphPosition> glyphs)
        {
            EnsureInTextObject();
            if (glyphs.Length == 0) return;

            var textSegments = new System.Collections.Generic.List<byte[]>();
            var adjustments = new System.Collections.Generic.List<float>();

            for (int i = 0; i < glyphs.Length; i++)
            {
                font.RecordGlyphUsage(glyphs[i].GlyphId);
                var encoded = new byte[2];
                PdfFont.EncodeGlyphId(glyphs[i].GlyphId, encoded, 0);
                textSegments.Add(encoded);

                if (glyphs[i].XAdvanceAdjustment != 0)
                {
                    adjustments.Add(glyphs[i].XAdvanceAdjustment);
                }
            }

            _builder.ShowTextWithPositioning(textSegments.ToArray(), adjustments.ToArray());
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

        /// <summary>Set text horizontal scaling as a percentage (PDF operator: Tz). Default is 100.</summary>
        public void SetTextHorizontalScaling(float percent) => _builder.SetTextHorizontalScaling(percent);

        /// <summary>Move to the next line (PDF operator: T*).</summary>
        public void NextLine()
        {
            EnsureInTextObject();
            _builder.NextLine();
        }

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
        // Gradients / Shadings
        // ═══════════════════════════════════════════

        /// <summary>Apply a linear gradient shading to the current graphics state.</summary>
        public void ApplyLinearGradient(PdfLinearGradient gradient)
        {
            if (gradient == null) throw new ArgumentNullException(nameof(gradient));
            if (gradient.Stops == null || gradient.Stops.Length < 2)
                throw new ArgumentException("Linear gradient requires at least 2 color stops.", nameof(gradient));

            var shadingDict = new PdfDictionary(6);
            shadingDict[PdfName.ShadingType] = new PdfInteger((int)PdfShadingType.Linear);
            shadingDict[PdfName.ColorSpace] = PdfName.DeviceRGB;

            var coords = new PdfArray(4);
            coords.Add(new PdfReal(gradient.X0));
            coords.Add(new PdfReal(gradient.Y0));
            coords.Add(new PdfReal(gradient.X1));
            coords.Add(new PdfReal(gradient.Y1));
            shadingDict[PdfName.Coords] = coords;

            var extend = new PdfArray(2);
            extend.Add(gradient.ExtendStart ? PdfBoolean.True : PdfBoolean.False);
            extend.Add(gradient.ExtendEnd ? PdfBoolean.True : PdfBoolean.False);
            shadingDict[PdfName.Extend] = extend;

            var functionRef = BuildGradientFunction(gradient.Stops);
            shadingDict[PdfName.Function] = functionRef;

            RegisterShading(shadingDict);
        }

        /// <summary>Apply a radial gradient shading to the current graphics state.</summary>
        public void ApplyRadialGradient(PdfRadialGradient gradient)
        {
            if (gradient == null) throw new ArgumentNullException(nameof(gradient));
            if (gradient.Stops == null || gradient.Stops.Length < 2)
                throw new ArgumentException("Radial gradient requires at least 2 color stops.", nameof(gradient));

            var shadingDict = new PdfDictionary(6);
            shadingDict[PdfName.ShadingType] = new PdfInteger((int)PdfShadingType.Radial);
            shadingDict[PdfName.ColorSpace] = PdfName.DeviceRGB;

            var coords = new PdfArray(6);
            coords.Add(new PdfReal(gradient.X0));
            coords.Add(new PdfReal(gradient.Y0));
            coords.Add(new PdfReal(gradient.R0));
            coords.Add(new PdfReal(gradient.X1));
            coords.Add(new PdfReal(gradient.Y1));
            coords.Add(new PdfReal(gradient.R1));
            shadingDict[PdfName.Coords] = coords;

            var extend = new PdfArray(2);
            extend.Add(gradient.ExtendStart ? PdfBoolean.True : PdfBoolean.False);
            extend.Add(gradient.ExtendEnd ? PdfBoolean.True : PdfBoolean.False);
            shadingDict[PdfName.Extend] = extend;

            var functionRef = BuildGradientFunction(gradient.Stops);
            shadingDict[PdfName.Function] = functionRef;

            RegisterShading(shadingDict);
        }

        /// <summary>
        /// Apply a conic (sweep) gradient by approximating it with thin wedge segments,
        /// each filled with a linear gradient. PDF has no native conic shading type,
        /// so we tessellate the full sweep into N triangular wedges.
        /// </summary>
        public void ApplyConicGradient(PdfConicGradient gradient)
        {
            if (gradient == null) throw new ArgumentNullException(nameof(gradient));
            if (gradient.Stops == null || gradient.Stops.Length < 2)
                throw new ArgumentException("Conic gradient requires at least 2 color stops.", nameof(gradient));

            int segments = Math.Max(12, gradient.Segments);
            float cx = gradient.CenterX;
            float cy = gradient.CenterY;
            // Radius large enough to cover the bounding box from center
            float radius = (float)Math.Sqrt(gradient.Width * gradient.Width + gradient.Height * gradient.Height);

            // CSS conic: 0deg = top (12 o'clock), clockwise
            // PDF coords: y increases downward in our usage (transformed), but we work in
            // math convention where angle 0 = right (3 o'clock), counterclockwise.
            // Convert: pdf_angle = 90 - css_angle (then negate for clockwise)
            // We iterate clockwise from startAngle.
            float startAngleDeg = gradient.StartAngle;
            float degreesPerSegment = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float segStartDeg = startAngleDeg + i * degreesPerSegment;
                float segEndDeg = segStartDeg + degreesPerSegment;

                // Fractional position along the gradient (0..1)
                float t0 = (float)i / segments;
                float t1 = (float)(i + 1) / segments;

                // Interpolate color at t0 and t1 from the gradient stops
                InterpolateStopColor(gradient.Stops, t0, out float r0, out float g0, out float b0);
                InterpolateStopColor(gradient.Stops, t1, out float r1, out float g1, out float b1);

                // Convert CSS angles to math radians for coordinate calculation
                // CSS: 0deg = top (north), clockwise
                // Math: 0rad = right (east), counterclockwise
                // CSS angle a -> math angle = (90 - a) in degrees -> radians, but since CSS is CW
                // we use: math_rad = (90 - css_deg) * PI/180
                float radStart = (90f - segStartDeg) * (float)(Math.PI / 180.0);
                float radEnd = (90f - segEndDeg) * (float)(Math.PI / 180.0);

                float x0 = cx + radius * (float)Math.Cos(radStart);
                float y0 = cy - radius * (float)Math.Sin(radStart);
                float x1 = cx + radius * (float)Math.Cos(radEnd);
                float y1 = cy - radius * (float)Math.Sin(radEnd);

                // Draw a filled triangle (cx,cy) -> (x0,y0) -> (x1,y1)
                // with a linear gradient from the midpoint of edge0 to midpoint of edge1
                // For simplicity and visual quality, fill the wedge with the average color
                // and apply a linear gradient shading across it.

                // Save state, clip to wedge, apply linear gradient shading
                _builder.SaveState();
                _graphicsStateDepth++;

                // Build wedge clip path: triangle cx,cy -> x0,y0 -> x1,y1
                _builder.MoveTo(cx, cy);
                _builder.LineTo(x0, y0);
                _builder.LineTo(x1, y1);
                _builder.ClosePath();
                _builder.Clip();

                // Create a linear gradient shading for this wedge segment
                // Direction: from center outward at mid-angle, but for color variation
                // we want the gradient along the sweep direction.
                // Use a linear gradient from the start edge to end edge of the wedge.
                float midX0 = (cx + x0) / 2f;
                float midY0 = (cy + y0) / 2f;
                float midX1 = (cx + x1) / 2f;
                float midY1 = (cy + y1) / 2f;

                var shadingDict = new PdfDictionary(6);
                shadingDict[PdfName.ShadingType] = new PdfInteger((int)PdfShadingType.Linear);
                shadingDict[PdfName.ColorSpace] = PdfName.DeviceRGB;

                var coords = new PdfArray(4);
                coords.Add(new PdfReal(midX0));
                coords.Add(new PdfReal(midY0));
                coords.Add(new PdfReal(midX1));
                coords.Add(new PdfReal(midY1));
                shadingDict[PdfName.Coords] = coords;

                var extend = new PdfArray(2);
                extend.Add(PdfBoolean.True);
                extend.Add(PdfBoolean.True);
                shadingDict[PdfName.Extend] = extend;

                var stops = new PdfGradientColorStop[]
                {
                    new PdfGradientColorStop(0, r0, g0, b0),
                    new PdfGradientColorStop(1, r1, g1, b1)
                };
                var functionRef = BuildGradientFunction(stops);
                shadingDict[PdfName.Function] = functionRef;

                RegisterShading(shadingDict);

                _graphicsStateDepth--;
                _builder.RestoreState();
            }
        }

        /// <summary>
        /// Interpolates a color from the gradient stops at the given position t (0..1).
        /// </summary>
        private static void InterpolateStopColor(PdfGradientColorStop[] stops, float t,
            out float r, out float g, out float b)
        {
            if (t <= stops[0].Position)
            {
                r = stops[0].R;
                g = stops[0].G;
                b = stops[0].B;
                return;
            }
            if (t >= stops[stops.Length - 1].Position)
            {
                r = stops[stops.Length - 1].R;
                g = stops[stops.Length - 1].G;
                b = stops[stops.Length - 1].B;
                return;
            }

            for (int i = 0; i < stops.Length - 1; i++)
            {
                if (t >= stops[i].Position && t <= stops[i + 1].Position)
                {
                    float range = stops[i + 1].Position - stops[i].Position;
                    float frac = range > 0 ? (t - stops[i].Position) / range : 0;
                    r = stops[i].R + (stops[i + 1].R - stops[i].R) * frac;
                    g = stops[i].G + (stops[i + 1].G - stops[i].G) * frac;
                    b = stops[i].B + (stops[i + 1].B - stops[i].B) * frac;
                    return;
                }
            }

            // Fallback: last stop
            r = stops[stops.Length - 1].R;
            g = stops[stops.Length - 1].G;
            b = stops[stops.Length - 1].B;
        }

        private void RegisterShading(PdfDictionary shadingDict)
        {
            _shadingCounter++;
            string name = "Sh" + _shadingCounter;
            var shadingRef = _objectTable.Allocate(shadingDict);
            _shadings[name] = shadingRef;
            _builder.ApplyShading(name);
        }

        private PdfReference BuildGradientFunction(PdfGradientColorStop[] stops)
        {
            if (stops.Length == 2)
            {
                return BuildType2Function(stops[0], stops[1]);
            }

            // Type 3 stitching function for 3+ stops
            var stitchDict = new PdfDictionary(6);
            stitchDict[PdfName.FunctionType] = new PdfInteger(3);

            var domain = new PdfArray(2);
            domain.Add(new PdfReal(0));
            domain.Add(new PdfReal(1));
            stitchDict[PdfName.Domain] = domain;

            var range = new PdfArray(6);
            range.Add(new PdfReal(0)); range.Add(new PdfReal(1));
            range.Add(new PdfReal(0)); range.Add(new PdfReal(1));
            range.Add(new PdfReal(0)); range.Add(new PdfReal(1));
            stitchDict[PdfName.Range] = range;

            var functions = new PdfArray(stops.Length - 1);
            var bounds = new PdfArray(stops.Length - 2);
            var encode = new PdfArray((stops.Length - 1) * 2);

            for (int i = 0; i < stops.Length - 1; i++)
            {
                var subRef = BuildType2Function(stops[i], stops[i + 1]);
                functions.Add(subRef);
                encode.Add(new PdfReal(0));
                encode.Add(new PdfReal(1));

                if (i > 0)
                    bounds.Add(new PdfReal(stops[i].Position));
            }

            stitchDict[PdfName.Functions] = functions;
            stitchDict[PdfName.Bounds] = bounds;
            stitchDict[PdfName.Encode] = encode;

            return _objectTable.Allocate(stitchDict);
        }

        private PdfReference BuildType2Function(PdfGradientColorStop from, PdfGradientColorStop to)
        {
            var funcDict = new PdfDictionary(6);
            funcDict[PdfName.FunctionType] = new PdfInteger(2);

            var domain = new PdfArray(2);
            domain.Add(new PdfReal(0));
            domain.Add(new PdfReal(1));
            funcDict[PdfName.Domain] = domain;

            var c0 = new PdfArray(3);
            c0.Add(new PdfReal(from.R));
            c0.Add(new PdfReal(from.G));
            c0.Add(new PdfReal(from.B));
            funcDict[PdfName.C0] = c0;

            var c1 = new PdfArray(3);
            c1.Add(new PdfReal(to.R));
            c1.Add(new PdfReal(to.G));
            c1.Add(new PdfReal(to.B));
            funcDict[PdfName.C1] = c1;

            funcDict[PdfName.N] = new PdfInteger(1);

            return _objectTable.Allocate(funcDict);
        }

        // ═══════════════════════════════════════════
        // Optional Content Groups (Layers)
        // ═══════════════════════════════════════════

        private readonly Dictionary<string, PdfOptionalContentGroup> _ocgs = new Dictionary<string, PdfOptionalContentGroup>();
        private int _ocgCounter;

        internal IReadOnlyDictionary<string, PdfOptionalContentGroup> OCGs => _ocgs;

        /// <summary>
        /// Begin optional content — content drawn between this call and <see cref="EndLayer"/>
        /// is associated with the given layer and can be toggled visible/hidden in PDF viewers.
        /// </summary>
        public void BeginLayer(PdfOptionalContentGroup layer)
        {
            if (layer == null) throw new System.ArgumentNullException(nameof(layer));
            RegisterOCG(layer);
            // BDC /OC /ResourceName — begin optional content with properties reference
            _builder.BeginOptionalContent(layer.ResourceName);
        }

        /// <summary>End optional content sequence started by <see cref="BeginLayer"/>.</summary>
        public void EndLayer()
        {
            _builder.EndMarkedContent();
        }

        private void RegisterOCG(PdfOptionalContentGroup layer)
        {
            if (string.IsNullOrEmpty(layer.ResourceName))
            {
                _ocgCounter++;
                layer.ResourceName = "OC" + _ocgCounter;
            }
            if (!_ocgs.ContainsKey(layer.ResourceName))
            {
                _ocgs[layer.ResourceName] = layer;
            }
        }

        // ═══════════════════════════════════════════
        // Tiling Patterns
        // ═══════════════════════════════════════════

        /// <summary>Set the fill color to a tiling pattern.</summary>
        public void SetFillPattern(PdfTilingPattern pattern)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            RegisterPattern(pattern);
            _builder.SetFillColorSpace("Pattern");
            _builder.SetFillPatternScn(pattern.ResourceName);
        }

        /// <summary>Set the stroke color to a tiling pattern.</summary>
        public void SetStrokePattern(PdfTilingPattern pattern)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            RegisterPattern(pattern);
            _builder.SetStrokeColorSpace("Pattern");
            _builder.SetStrokePatternScn(pattern.ResourceName);
        }

        private void RegisterPattern(PdfTilingPattern pattern)
        {
            if (string.IsNullOrEmpty(pattern.ResourceName))
            {
                _patternCounter++;
                pattern.ResourceName = "P" + _patternCounter;
            }
            if (!_patterns.ContainsKey(pattern.ResourceName))
            {
                _patterns[pattern.ResourceName] = pattern;
            }
        }

        // ═══════════════════════════════════════════
        // Text state helpers
        // ═══════════════════════════════════════════

        private void EnsureInTextObject()
        {
            if (!_inTextObject)
                throw new InvalidOperationException(
                    "This operation requires an active text object. Call BeginText() first.");
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
