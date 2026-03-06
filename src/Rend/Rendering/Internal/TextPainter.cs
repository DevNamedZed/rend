using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Fonts;
using Rend.Layout;
using Rend.Text;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints text content from line fragments, including text decorations
    /// such as underline, overline, and line-through.
    /// </summary>
    internal static class TextPainter
    {
        internal static bool _debugText = false;
        /// <summary>
        /// Paints a single line fragment onto the render target.
        /// </summary>
        /// <param name="fragment">The line fragment to paint.</param>
        /// <param name="lineX">The X position of the parent line box.</param>
        /// <param name="lineY">The Y position of the parent line box.</param>
        /// <param name="lineBaseline">The baseline offset from the top of the line box.</param>
        /// <param name="target">The render target to draw on.</param>
        /// <param name="style">The computed style for the text.</param>
        /// <param name="isVertical">Whether this fragment belongs to a vertical writing mode line box.</param>
        public static void Paint(LineFragment fragment, float lineX, float lineY,
                                 float lineBaseline, IRenderTarget target, ComputedStyle style,
                                 bool isVertical = false)
        {
            if (isVertical)
            {
                PaintVertical(fragment, lineX, lineY, lineBaseline, target, style);
                return;
            }

            float drawX = lineX + fragment.X;
            float rawY = lineY + fragment.Y + fragment.Baseline;
            float drawY = (float)Math.Floor(rawY);
            if (_debugText && fragment.Text != null && fragment.Text.Length > 0)
                Console.WriteLine($"[TEXT] \"{fragment.Text.Substring(0, Math.Min(20, fragment.Text.Length))}\" drawX={drawX:F2} drawY={drawY} lineX={lineX:F2} lineY={lineY:F4} fragX={fragment.X:F2} fragY={fragment.Y:F4} baseline={fragment.Baseline:F4} rawY={rawY:F4}");

            // Paint text shadows before main text.
            TextShadowPainter.Paint(fragment, drawX, drawY, target, style);

            CssColor color = style.Color;
            float fontSize = style.FontSize;
            string fontFamily = style.FontFamily;
            CssFontStyle fontStyle = style.FontStyle;
            float fontWeight = style.FontWeight;
            float letterSpacing = style.LetterSpacing;
            float wordSpacing = style.WordSpacing;

            if (fragment.ShapedRun != null)
            {
                float stretch = FontDescriptor.StretchToPercentage(style.FontStretch);
                var fontDesc = new FontDescriptor(fontFamily, fontWeight, fontStyle, stretch);
                var run = fragment.ShapedRun;
                if (letterSpacing != 0 || wordSpacing != 0)
                    run = ApplySpacingToRun(run, letterSpacing, wordSpacing);
                target.DrawGlyphs(run, drawX, drawY, color, fontDesc);
            }
            else
            {
                string? text = fragment.Text;
                if (text != null)
                {
                    float stretch = FontDescriptor.StretchToPercentage(style.FontStretch);
                    var textStyle = new TextStyle
                    {
                        Font = new FontDescriptor(fontFamily, fontWeight, fontStyle, stretch),
                        FontSize = fontSize,
                        Color = color,
                        Bold = fontWeight >= 700f,
                        Italic = fontStyle == CssFontStyle.Italic || fontStyle == CssFontStyle.Oblique,
                        LetterSpacing = letterSpacing,
                        WordSpacing = wordSpacing,
                        FontData = null
                    };
                    target.DrawText(text, drawX, drawY, textStyle);
                }
            }

            // Paint text decorations.
            PaintDecorations(fragment, lineX, lineY, lineBaseline, target, style);

            // Paint ruby annotation text if present
            if (fragment.RubyText != null)
            {
                PaintRubyAnnotation(fragment, lineX, lineY, target, style);
            }
        }

        /// <summary>
        /// Paints ruby annotation text above or below a base text fragment.
        /// </summary>
        private static void PaintRubyAnnotation(LineFragment fragment, float lineX, float lineY,
                                                  IRenderTarget target, ComputedStyle baseStyle)
        {
            var rubyStyle = fragment.RubyStyle ?? baseStyle;
            float rubyFontSize = fragment.RubyStyle != null ? rubyStyle.FontSize : baseStyle.FontSize * 0.5f;

            float drawX = lineX + fragment.X;
            float drawY;

            if (fragment.RubyBelow)
            {
                // Position below the base text
                drawY = lineY + fragment.Y + fragment.Height + rubyFontSize * 0.2f;
            }
            else
            {
                // Position above the base text (default: over)
                drawY = lineY + fragment.Y - rubyFontSize * 0.3f;
            }

            // Center the ruby text over the base text
            float baseWidth = fragment.Width;
            float stretch = FontDescriptor.StretchToPercentage(baseStyle.FontStretch);
            var rubyTextStyle = new TextStyle
            {
                Font = new FontDescriptor(
                    rubyStyle.FontFamily,
                    rubyStyle.FontWeight,
                    rubyStyle.FontStyle,
                    stretch),
                FontSize = rubyFontSize,
                Color = rubyStyle.Color,
                Bold = rubyStyle.FontWeight >= 700f,
                Italic = rubyStyle.FontStyle == CssFontStyle.Italic || rubyStyle.FontStyle == CssFontStyle.Oblique
            };

            // Estimate ruby text width for centering
            float charWidth = rubyFontSize * 0.6f; // approximate average char width
            float rubyWidth = fragment.RubyText!.Length * charWidth;
            float xOffset = (baseWidth - rubyWidth) * 0.5f;
            if (xOffset > 0) drawX += xOffset;

            target.DrawText(fragment.RubyText, drawX, drawY, rubyTextStyle);
        }

        /// <summary>
        /// Paints text in vertical writing mode by applying a 90-degree clockwise
        /// rotation transform around the fragment's position.
        /// This handles the "sideways" text orientation case. For text-orientation: mixed,
        /// CJK characters should be drawn upright (TODO: implement per-character classification).
        /// For text-orientation: upright, all characters should be drawn upright (TODO).
        /// </summary>
        private static void PaintVertical(LineFragment fragment, float lineX, float lineY,
                                           float lineBaseline, IRenderTarget target, ComputedStyle style)
        {
            // In the vertical layout, fragments are positioned with:
            //   X = offset within line box (horizontal position of the column)
            //   Y = offset along the inline (vertical) direction
            // We need to draw the text rotated 90 degrees clockwise so that
            // horizontal text appears running top-to-bottom.

            float fragX = lineX + fragment.X;
            float fragY = lineY + fragment.Y;

            // The rotation pivot is at the top-left of the fragment.
            // After rotating 90 degrees CW: (x, y) -> (y, -x)
            // We translate so the text appears in the correct position.
            float pivotX = fragX + fragment.Width * 0.5f;
            float pivotY = fragY + fragment.Width * 0.5f; // use Width (line-height) as the square pivot

            // Save state, apply rotation
            target.Save();

            // Build rotation matrix: rotate 90 degrees clockwise around (pivotX, pivotY)
            float angle = (float)(Math.PI / 2.0);
            var toOrigin = Matrix3x2.CreateTranslation(-pivotX, -pivotY);
            var rotation = Matrix3x2.CreateRotation(angle);
            var fromOrigin = Matrix3x2.CreateTranslation(pivotX, pivotY);
            var transform = toOrigin * rotation * fromOrigin;
            target.SetTransform(transform);

            // Draw text at the pre-rotation position
            float drawX = fragX;
            float drawY = fragY + fragment.Baseline;

            CssColor color = style.Color;
            float fontSize = style.FontSize;
            string fontFamily = style.FontFamily;
            CssFontStyle fontStyle = style.FontStyle;
            float fontWeight = style.FontWeight;
            float letterSpacing = style.LetterSpacing;
            float wordSpacing = style.WordSpacing;

            if (fragment.ShapedRun != null)
            {
                float stretch = FontDescriptor.StretchToPercentage(style.FontStretch);
                var fontDesc = new FontDescriptor(fontFamily, fontWeight, fontStyle, stretch);
                var run = fragment.ShapedRun;
                if (letterSpacing != 0 || wordSpacing != 0)
                    run = ApplySpacingToRun(run, letterSpacing, wordSpacing);
                target.DrawGlyphs(run, drawX, drawY, color, fontDesc);
            }
            else
            {
                string? text = fragment.Text;
                if (text != null)
                {
                    float stretch = FontDescriptor.StretchToPercentage(style.FontStretch);
                    var textStyle = new TextStyle
                    {
                        Font = new FontDescriptor(fontFamily, fontWeight, fontStyle, stretch),
                        FontSize = fontSize,
                        Color = color,
                        Bold = fontWeight >= 700f,
                        Italic = fontStyle == CssFontStyle.Italic || fontStyle == CssFontStyle.Oblique,
                        LetterSpacing = letterSpacing,
                        WordSpacing = wordSpacing,
                        FontData = null
                    };
                    target.DrawText(text, drawX, drawY, textStyle);
                }
            }

            // Restore state (removes rotation)
            target.Restore();

            // TODO: text-orientation: mixed — classify each character as CJK (upright) or
            // Latin/other (sideways 90deg). Currently all text is rotated sideways.
            // TODO: text-orientation: upright — draw each character upright with wider spacing.
        }

        /// <summary>
        /// Creates a new ShapedTextRun with letter-spacing and word-spacing applied to glyph advances.
        /// This preserves HarfBuzz shaping quality instead of falling back to character-by-character rendering.
        /// </summary>
        private static ShapedTextRun ApplySpacingToRun(ShapedTextRun run, float letterSpacing, float wordSpacing)
        {
            var srcGlyphs = run.Glyphs;
            if (srcGlyphs.Length == 0) return run;

            var text = run.OriginalText;
            var newGlyphs = new ShapedGlyph[srcGlyphs.Length];

            for (int i = 0; i < srcGlyphs.Length; i++)
            {
                float extraAdvance = 0;

                // Letter-spacing: add to every glyph's advance (Chrome applies to all, including last)
                extraAdvance += letterSpacing;

                // Word-spacing: add extra advance for space characters
                if (wordSpacing != 0)
                {
                    uint cluster = srcGlyphs[i].Cluster;
                    if (cluster < text.Length && text[(int)cluster] == ' ')
                        extraAdvance += wordSpacing;
                }

                newGlyphs[i] = new ShapedGlyph(
                    srcGlyphs[i].GlyphId,
                    srcGlyphs[i].Cluster,
                    srcGlyphs[i].XAdvance + extraAdvance,
                    srcGlyphs[i].YAdvance,
                    srcGlyphs[i].XOffset,
                    srcGlyphs[i].YOffset);
            }

            return new ShapedTextRun(newGlyphs, text, run.FontSize, run.FontData);
        }

        private static void PaintDecorations(LineFragment fragment, float lineX, float lineY,
                                              float lineBaseline, IRenderTarget target, ComputedStyle style)
        {
            CssTextDecorationLine decoration = style.TextDecorationLine;
            if (decoration == CssTextDecorationLine.None)
            {
                return;
            }

            // Use text-decoration-color if set, otherwise fall back to element's color.
            CssColor decoColor = style.TextDecorationColor;
            if (decoColor.A == 0)
            {
                return;
            }

            float fontSize = style.FontSize;
            // Use text-decoration-thickness if set, otherwise default to 1/16th of font size (min 1px).
            float thickness = style.TextDecorationThickness;
            float strokeWidth = thickness > 0 ? thickness : (fontSize > 16f ? fontSize / 16f : 1f);

            // Build pen based on text-decoration-style.
            CssTextDecorationStyle decoStyle = style.TextDecorationStyle;
            PenInfo pen = BuildDecorationPen(decoColor, strokeWidth, decoStyle);

            float startX = lineX + fragment.X;
            float endX = startX + fragment.Width;

            // text-underline-offset: additional offset for underlines (positive = further from text)
            float underlineOffset = style.TextUnderlineOffset;

            if (decoration == CssTextDecorationLine.Underline)
            {
                // Underline sits slightly below the baseline, plus any custom offset.
                float underlineY = lineY + fragment.Y + fragment.Baseline + fontSize * 0.15f + underlineOffset;
                DrawLine(target, pen, startX, underlineY, endX, underlineY);
            }
            else if (decoration == CssTextDecorationLine.Overline)
            {
                // Overline sits at the top of the text.
                float overlineY = lineY + fragment.Y;
                DrawLine(target, pen, startX, overlineY, endX, overlineY);
            }
            else if (decoration == CssTextDecorationLine.LineThrough)
            {
                // Line-through goes through the middle of the text.
                float strikeY = lineY + fragment.Y + fragment.Height * 0.5f;
                DrawLine(target, pen, startX, strikeY, endX, strikeY);
            }

            // For "wavy" style, draw a second offset line to approximate a wave.
            if (decoStyle == CssTextDecorationStyle.Wavy)
            {
                float wavyOffset = strokeWidth * 2f;
                if (decoration == CssTextDecorationLine.Underline)
                {
                    float underlineY = lineY + fragment.Y + fragment.Baseline + fontSize * 0.15f + underlineOffset + wavyOffset;
                    DrawLine(target, pen, startX, underlineY, endX, underlineY);
                }
                else if (decoration == CssTextDecorationLine.Overline)
                {
                    float overlineY = lineY + fragment.Y + wavyOffset;
                    DrawLine(target, pen, startX, overlineY, endX, overlineY);
                }
                else if (decoration == CssTextDecorationLine.LineThrough)
                {
                    float strikeY = lineY + fragment.Y + fragment.Height * 0.5f + wavyOffset;
                    DrawLine(target, pen, startX, strikeY, endX, strikeY);
                }
            }

            // For "double" style, draw a second line offset below/above.
            if (decoStyle == CssTextDecorationStyle.Double)
            {
                float doubleOffset = strokeWidth * 2f;
                if (decoration == CssTextDecorationLine.Underline)
                {
                    float underlineY = lineY + fragment.Y + fragment.Baseline + fontSize * 0.15f + underlineOffset + doubleOffset;
                    DrawLine(target, pen, startX, underlineY, endX, underlineY);
                }
                else if (decoration == CssTextDecorationLine.Overline)
                {
                    float overlineY = lineY + fragment.Y - doubleOffset;
                    DrawLine(target, pen, startX, overlineY, endX, overlineY);
                }
                else if (decoration == CssTextDecorationLine.LineThrough)
                {
                    float strikeY = lineY + fragment.Y + fragment.Height * 0.5f + doubleOffset;
                    DrawLine(target, pen, startX, strikeY, endX, strikeY);
                }
            }
        }

        private static PenInfo BuildDecorationPen(CssColor color, float strokeWidth, CssTextDecorationStyle decoStyle)
        {
            switch (decoStyle)
            {
                case CssTextDecorationStyle.Dashed:
                {
                    float dashLen = Math.Max(strokeWidth * 3f, 3f);
                    return new PenInfo(color, strokeWidth, new[] { dashLen, dashLen });
                }

                case CssTextDecorationStyle.Dotted:
                {
                    float dotLen = Math.Max(strokeWidth, 1f);
                    return new PenInfo(color, strokeWidth, new[] { dotLen, dotLen });
                }

                default:
                    // Solid, double, wavy all use a solid pen (double/wavy draw extra lines).
                    return new PenInfo(color, strokeWidth);
            }
        }

        private static void DrawLine(IRenderTarget target, PenInfo pen, float x1, float y1, float x2, float y2)
        {
            var path = new PathData();
            path.MoveTo(x1, y1);
            path.LineTo(x2, y2);
            target.StrokePath(path, pen);
        }
    }
}
