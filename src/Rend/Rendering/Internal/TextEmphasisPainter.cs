using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Fonts;
using Rend.Layout;
using Rend.Layout.Internal;
using Rend.Text;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints text emphasis marks above or below text characters.
    /// [CSS-TEXT-DECOR-3 §3.5] Emphasis marks are drawn over each character
    /// of the element, similar to ruby annotations.
    /// </summary>
    internal static class TextEmphasisPainter
    {
        /// <summary>
        /// Paints emphasis marks for a text fragment if text-emphasis-style is set.
        /// </summary>
        public static void Paint(LineFragment fragment, float lineX, float lineY,
            float lineBaseline, IRenderTarget target, ComputedStyle style)
        {
            string? markString = TextEmphasisResolver.ResolveEmphasisMark(style);
            if (markString == null)
            {
                return;
            }

            CssColor color = style.TextEmphasisColor;
            if (color.A == 0)
            {
                return;
            }

            var context = CreateContext(style, lineY, lineBaseline, markString, color);
            var primaryMarkFont = BuildMarkFont(context, fontData: null, target);

            if (fragment.ShapedRun != null)
            {
                PaintMarksForGlyphs(fragment, lineX, context, primaryMarkFont, target);
            }
            else if (fragment.Text != null)
            {
                PaintMarksForText(fragment, lineX, primaryMarkFont, markString, target);
            }
        }

        /// <summary>
        /// Gathers the invariants needed to position every emphasis mark in the fragment.
        /// These do not depend on the per-glyph font, so are computed once per Paint call.
        /// </summary>
        private static EmphasisPaintContext CreateContext(ComputedStyle style, float lineY,
            float lineBaseline, string markString, CssColor color)
        {
            float fontSize = style.FontSize;
            float markFontSize = fontSize * 0.5f;
            bool positionOver = TextEmphasisResolver.ResolveEmphasisPositionOver(style);
            float textBaselineY = lineY + lineBaseline;

            float stretch = FontDescriptor.StretchToPercentage(style.FontStretch);
            var primaryDescriptor = new FontDescriptor(
                style.FontFamilies, style.FontWeight, style.FontStyle, stretch);

            bool bold = style.FontWeight >= 700f;

            return new EmphasisPaintContext(
                markString, color, fontSize, markFontSize,
                positionOver, textBaselineY, primaryDescriptor, bold);
        }

        /// <summary>
        /// [CSS-TEXT-DECOR-3 §3.5.3] Builds the Y position, draw style, and measured width
        /// of the emphasis mark when rendered with a specific font. When <paramref name="fontData"/>
        /// is non-null the metrics come from that per-glyph fallback font, and the mark itself
        /// is drawn with it — this matches Chrome's Blink pipeline where the mark follows the
        /// actual font used to shape the underlying glyph.
        /// </summary>
        private static EmphasisMarkFont BuildMarkFont(in EmphasisPaintContext context,
            byte[]? fontData, IRenderTarget target)
        {
            float ascent;
            float descent;
            if (fontData != null)
            {
                (ascent, descent) = target.GetFontMetrics(fontData, context.FontSize);
            }
            else
            {
                (ascent, descent) = target.GetFontMetrics(context.PrimaryDescriptor, context.FontSize);
            }

            float markY;
            if (context.PositionOver)
            {
                // Mark above text: place the mark baseline at the content-area top
                // (textBaseline - ascent), shifted up by roughly the mark's descent.
                markY = context.TextBaselineY - ascent - context.MarkFontSize * 0.5f;
            }
            else
            {
                // Mark below text: place the mark baseline below the content-area bottom.
                markY = context.TextBaselineY + descent + context.MarkFontSize * 0.8f;
            }

            // [CSS-TEXT-DECOR-3 §3.5] The mark glyph itself is always drawn with the
            // cascade-resolved primary font — Chrome's Blink pipeline does NOT substitute
            // the per-glyph fallback font for the mark character, only uses its metrics
            // for vertical positioning. Verified by inspecting position-property-001 PNGs:
            // Chrome renders ● as a small thin Latin-serif dot even when the underlying
            // CJK text glyphs came from a CJK fallback font.
            var markStyle = new TextStyle
            {
                Font = context.PrimaryDescriptor,
                FontSize = context.MarkFontSize,
                Color = context.Color,
                Bold = context.Bold,
                Italic = false,
                FontData = null
            };

            float markWidth = target.MeasureText(context.MarkString, markStyle);
            if (markWidth <= 0)
            {
                markWidth = context.MarkFontSize * 0.8f;
            }

            return new EmphasisMarkFont(markY, markStyle, markWidth);
        }

        private static void PaintMarksForGlyphs(LineFragment fragment, float lineX,
            in EmphasisPaintContext context, EmphasisMarkFont primaryMarkFont, IRenderTarget target)
        {
            var shaped = fragment.ShapedRun!;
            var glyphs = shaped.Glyphs;
            string text = shaped.OriginalText;
            byte[]?[]? overrides = shaped.GlyphFontOverrides;
            float penX = lineX + fragment.X;

            byte[]? currentFontData = null;
            EmphasisMarkFont currentMarkFont = primaryMarkFont;

            for (int i = 0; i < glyphs.Length; i++)
            {
                float glyphAdvance = glyphs[i].XAdvance;

                // [CSS-TEXT-DECOR-3 §3.5.1] Skip emphasis on space characters.
                uint cluster = glyphs[i].Cluster;
                if (cluster < text.Length && char.IsWhiteSpace(text[(int)cluster]))
                {
                    penX += glyphAdvance;
                    continue;
                }

                // [CSS-TEXT-DECOR-3 §3.5.2] Skip punctuation characters.
                if (cluster < text.Length && IsPunctuationForEmphasis(text[(int)cluster]))
                {
                    penX += glyphAdvance;
                    continue;
                }

                // [CSS-TEXT-DECOR-3 §3.5] When the text glyph came from per-glyph font
                // fallback (e.g. CJK codepoint missing in the cascade-resolved Latin font),
                // Chrome positions and draws the emphasis mark using that same fallback
                // font. Recompute mark font state whenever the underlying glyph's font
                // changes — most runs have 0-1 transitions so this is cheap.
                byte[]? glyphFontData = overrides != null ? overrides[i] : null;
                if (!ReferenceEquals(glyphFontData, currentFontData))
                {
                    currentFontData = glyphFontData;
                    currentMarkFont = glyphFontData == null
                        ? primaryMarkFont
                        : BuildMarkFont(context, glyphFontData, target);
                }

                float markX = penX + (glyphAdvance - currentMarkFont.MarkWidth) * 0.5f;
                target.DrawText(context.MarkString, markX, currentMarkFont.MarkY, currentMarkFont.MarkStyle);
                penX += glyphAdvance;
            }
        }

        private static void PaintMarksForText(LineFragment fragment, float lineX,
            EmphasisMarkFont primaryMarkFont, string markString, IRenderTarget target)
        {
            string text = fragment.Text!;
            float charWidth = fragment.Width / Math.Max(text.Length, 1);
            float penX = lineX + fragment.X;

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsWhiteSpace(text[i]) || IsPunctuationForEmphasis(text[i]))
                {
                    penX += charWidth;
                    continue;
                }

                float markX = penX + (charWidth - primaryMarkFont.MarkWidth) * 0.5f;
                target.DrawText(markString, markX, primaryMarkFont.MarkY, primaryMarkFont.MarkStyle);
                penX += charWidth;
            }
        }

        /// <summary>
        /// [CSS-TEXT-DECOR-3 §3.5.2] Characters that should NOT receive emphasis marks.
        /// </summary>
        private static bool IsPunctuationForEmphasis(char ch)
        {
            if (char.IsPunctuation(ch) || char.IsSeparator(ch) || char.IsControl(ch))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Invariants for painting every emphasis mark in a fragment — everything that does
        /// not depend on the actual per-glyph font choice.
        /// </summary>
        private readonly struct EmphasisPaintContext
        {
            public string MarkString { get; }
            public CssColor Color { get; }
            public float FontSize { get; }
            public float MarkFontSize { get; }
            public bool PositionOver { get; }
            public float TextBaselineY { get; }
            public FontDescriptor PrimaryDescriptor { get; }
            public bool Bold { get; }

            public EmphasisPaintContext(string markString, CssColor color, float fontSize,
                float markFontSize, bool positionOver, float textBaselineY,
                FontDescriptor primaryDescriptor, bool bold)
            {
                MarkString = markString;
                Color = color;
                FontSize = fontSize;
                MarkFontSize = markFontSize;
                PositionOver = positionOver;
                TextBaselineY = textBaselineY;
                PrimaryDescriptor = primaryDescriptor;
                Bold = bold;
            }
        }

        /// <summary>
        /// The resolved mark-drawing state for one specific font — recomputed whenever a
        /// fragment crosses a per-glyph fallback boundary.
        /// </summary>
        private readonly struct EmphasisMarkFont
        {
            public float MarkY { get; }
            public TextStyle MarkStyle { get; }
            public float MarkWidth { get; }

            public EmphasisMarkFont(float markY, TextStyle markStyle, float markWidth)
            {
                MarkY = markY;
                MarkStyle = markStyle;
                MarkWidth = markWidth;
            }
        }
    }
}
