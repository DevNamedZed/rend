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
            // Chrome pixel-snaps block positions (PixelSnappedIntRect) before drawing text.
            // Round the line-box Y to nearest pixel, then floor after adding baseline.
            float snappedLineY = (float)Math.Round(lineY + fragment.Y);
            float drawY = (float)Math.Floor(snappedLineY + fragment.Baseline);

            // Paint text shadows before main text.
            TextShadowPainter.Paint(fragment, drawX, drawY, target, style);

            CssColor color = style.Color;
            float fontSize = style.FontSize;
            string[] fontFamilies = style.FontFamilies;
            CssFontStyle fontStyle = style.FontStyle;
            float fontWeight = style.FontWeight;
            float letterSpacing = style.LetterSpacing;
            float wordSpacing = style.WordSpacing + fragment.JustifyWordSpacing;

            DrawTextFragment(fragment, drawX, drawY, color, fontSize, fontFamilies,
                             fontStyle, fontWeight, letterSpacing, wordSpacing, style, target);

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
                    rubyStyle.FontFamilies,
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
            string[] fontFamilies = style.FontFamilies;
            CssFontStyle fontStyle = style.FontStyle;
            float fontWeight = style.FontWeight;
            float letterSpacing = style.LetterSpacing;
            float wordSpacing = style.WordSpacing + fragment.JustifyWordSpacing;

            DrawTextFragment(fragment, drawX, drawY, color, fontSize, fontFamilies,
                             fontStyle, fontWeight, letterSpacing, wordSpacing, style, target);

            // Restore state (removes rotation)
            target.Restore();

            // TODO: text-orientation: mixed — classify each character as CJK (upright) or
            // Latin/other (sideways 90deg). Currently all text is rotated sideways.
            // TODO: text-orientation: upright — draw each character upright with wider spacing.
        }

        /// <summary>
        /// Draws the text content of a fragment, using shaped glyphs when available
        /// and falling back to DrawText otherwise.
        /// </summary>
        private static void DrawTextFragment(LineFragment fragment, float drawX, float drawY,
                                             CssColor color, float fontSize, string[] fontFamilies,
                                             CssFontStyle fontStyle, float fontWeight,
                                             float letterSpacing, float wordSpacing,
                                             ComputedStyle style, IRenderTarget target)
        {
            if (fragment.ShapedRun != null)
            {
                float stretch = FontDescriptor.StretchToPercentage(style.FontStretch);
                var fontDesc = new FontDescriptor(fontFamilies, fontWeight, fontStyle, stretch);
                var run = fragment.ShapedRun;
                if (letterSpacing != 0 || wordSpacing != 0)
                {
                    run = ApplySpacingToRun(run, letterSpacing, wordSpacing);
                }
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
                        Font = new FontDescriptor(fontFamilies, fontWeight, fontStyle, stretch),
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

            // Get font-specific decoration metrics (underline/strikeout position and thickness).
            float stretch = FontDescriptor.StretchToPercentage(style.FontStretch);
            var fontDesc = new FontDescriptor(style.FontFamilies, style.FontWeight, style.FontStyle, stretch);
            var metrics = target.GetDecorationMetrics(fontDesc, fontSize);

            // Use text-decoration-thickness if set, otherwise use font's underline thickness.
            float thickness = style.TextDecorationThickness;
            float strokeWidth = thickness > 0 ? thickness : metrics.UnderlineThickness;

            // Build pen based on text-decoration-style.
            CssTextDecorationStyle decoStyle = style.TextDecorationStyle;
            PenInfo pen = BuildDecorationPen(decoColor, strokeWidth, decoStyle);

            float startX = lineX + fragment.X;
            float endX = startX + fragment.Width;

            // Snap line-box Y the same way as text rendering (Chrome pixel-snaps block positions)
            float snappedFragY = (float)Math.Round(lineY + fragment.Y);

            // text-underline-offset: additional offset for underlines (positive = further from text)
            float underlineOffset = style.TextUnderlineOffset;

            // Chrome paints solid text decorations as pixel-snapped filled rectangles,
            // not stroked lines. Use FillRect for solid style to match.
            bool useFillRect = decoStyle == CssTextDecorationStyle.Solid;

            // Chrome computes decoration positions relative to the pixel-snapped baseline Y
            // (same Floor as glyph rendering), not the raw fragment top + baseline.
            float baselineY = (float)Math.Floor(snappedFragY + fragment.Baseline);

            if (decoration == CssTextDecorationLine.Underline)
            {
                // [CSS-TEXT-DECOR-4 §3.3] text-underline-position: under places the
                // underline below the descenders rather than at the font's underline position.
                float underlineY;
                if (style.TextUnderlinePosition == CssTextUnderlinePosition.Under)
                {
                    // Compute descent from baseline to bottom of content area.
                    float halfLeading = fragment.ContentHeight > 0
                        ? (fragment.Height - fragment.ContentHeight) / 2f
                        : 0f;
                    float fontDescent = fragment.ContentHeight - (fragment.Baseline - halfLeading);
                    underlineY = baselineY + fontDescent + underlineOffset;
                }
                else
                {
                    underlineY = baselineY + metrics.UnderlinePosition + underlineOffset;
                }

                if (useFillRect)
                {
                    // Chrome floors the underline Y via PixelSnappedIntRect (floor origin, ceil far edge).
                    float snappedY = (float)Math.Floor(underlineY);
                    float snappedH = Math.Max(1f, (float)Math.Round(strokeWidth));
                    target.FillRect(new RectF(startX, snappedY, endX - startX, snappedH),
                                    BrushInfo.Solid(decoColor));
                }
                else
                {
                    DrawLine(target, pen, startX, underlineY, endX, underlineY);
                }
            }
            else if (decoration == CssTextDecorationLine.Overline)
            {
                // Chrome: overline offset from baseline is -ascent.
                // Compute font ascent: distance from baseline to top of content area.
                float halfLeading = fragment.ContentHeight > 0
                    ? (fragment.Height - fragment.ContentHeight) / 2f
                    : 0f;
                float fontAscent = fragment.Baseline - halfLeading;
                float overlineY = baselineY - fontAscent;
                if (useFillRect)
                {
                    float snappedY = (float)Math.Floor(overlineY);
                    float snappedH = Math.Max(1f, (float)Math.Round(strokeWidth));
                    target.FillRect(new RectF(startX, snappedY, endX - startX, snappedH),
                                    BrushInfo.Solid(decoColor));
                }
                else
                    DrawLine(target, pen, startX, overlineY, endX, overlineY);
            }
            else if (decoration == CssTextDecorationLine.LineThrough)
            {
                // Chrome uses the font's OS/2 strikeoutPosition relative to baseline,
                // not the middle of the content area.
                float strikeY = baselineY + metrics.StrikeoutPosition;
                if (useFillRect)
                {
                    float snappedY = (float)Math.Floor(strikeY);
                    float snappedH = Math.Max(1f, (float)Math.Round(strokeWidth));
                    target.FillRect(new RectF(startX, snappedY, endX - startX, snappedH),
                                    BrushInfo.Solid(decoColor));
                }
                else
                    DrawLine(target, pen, startX, strikeY, endX, strikeY);
            }

            // Compute base underline Y (before wavy/double offset) for reuse.
            float baseUnderlineY;
            if (style.TextUnderlinePosition == CssTextUnderlinePosition.Under)
            {
                float hlU = fragment.ContentHeight > 0
                    ? (fragment.Height - fragment.ContentHeight) / 2f
                    : 0f;
                float fontDescentU = fragment.ContentHeight - (fragment.Baseline - hlU);
                baseUnderlineY = baselineY + fontDescentU + underlineOffset;
            }
            else
            {
                baseUnderlineY = baselineY + metrics.UnderlinePosition + underlineOffset;
            }

            // For "wavy" style, draw a second offset line to approximate a wave.
            if (decoStyle == CssTextDecorationStyle.Wavy)
            {
                float wavyOffset = strokeWidth * 2f;
                if (decoration == CssTextDecorationLine.Underline)
                {
                    float underlineY = baseUnderlineY + wavyOffset;
                    DrawLine(target, pen, startX, underlineY, endX, underlineY);
                }
                else if (decoration == CssTextDecorationLine.Overline)
                {
                    float hl = fragment.ContentHeight > 0 ? (fragment.Height - fragment.ContentHeight) / 2f : 0f;
                    float overlineY = snappedFragY + hl + wavyOffset;
                    DrawLine(target, pen, startX, overlineY, endX, overlineY);
                }
                else if (decoration == CssTextDecorationLine.LineThrough)
                {
                    float strikeY = baselineY + metrics.StrikeoutPosition + wavyOffset;
                    DrawLine(target, pen, startX, strikeY, endX, strikeY);
                }
            }

            // For "double" style, draw a second line offset below/above.
            if (decoStyle == CssTextDecorationStyle.Double)
            {
                float doubleOffset = strokeWidth * 2f;
                if (decoration == CssTextDecorationLine.Underline)
                {
                    float underlineY = baseUnderlineY + doubleOffset;
                    DrawLine(target, pen, startX, underlineY, endX, underlineY);
                }
                else if (decoration == CssTextDecorationLine.Overline)
                {
                    float hl = fragment.ContentHeight > 0 ? (fragment.Height - fragment.ContentHeight) / 2f : 0f;
                    float overlineY = snappedFragY + hl - doubleOffset;
                    DrawLine(target, pen, startX, overlineY, endX, overlineY);
                }
                else if (decoration == CssTextDecorationLine.LineThrough)
                {
                    float strikeY = baselineY + metrics.StrikeoutPosition + doubleOffset;
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
                    float dashLen, gapLen;
                    if (strokeWidth >= 3f)
                    {
                        dashLen = strokeWidth * 2f;
                        gapLen = strokeWidth;
                    }
                    else
                    {
                        dashLen = strokeWidth * 3f;
                        gapLen = strokeWidth * 2f;
                    }
                    dashLen = Math.Max(dashLen, 1f);
                    gapLen = Math.Max(gapLen, 1f);
                    return new PenInfo(color, strokeWidth, new[] { dashLen, gapLen });
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
