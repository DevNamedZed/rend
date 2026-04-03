using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Fonts;
using Rend.Layout;
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
            string? markString = ResolveEmphasisMark(style);
            if (markString == null)
            {
                return;
            }

            CssColor color = style.TextEmphasisColor;
            if (color.A == 0)
            {
                return;
            }

            bool positionOver = ResolveEmphasisPositionOver(style);
            float fontSize = style.FontSize;
            float markFontSize = fontSize * 0.5f;

            float stretch = FontDescriptor.StretchToPercentage(style.FontStretch);
            var fontDesc = new FontDescriptor(
                style.FontFamilies, style.FontWeight, style.FontStyle, stretch);

            // [CSS-TEXT-DECOR-3 §3.5.3] Position emphasis marks relative to the text
            // content area (ascent/descent), not the line box. With large line-height,
            // half-leading pushes content down but marks must stay near text.
            var (ascent, descent) = target.GetFontMetrics(fontDesc, fontSize);
            float textBaselineY = lineY + lineBaseline;

            float markY;
            if (positionOver)
            {
                // Mark above text: place mark baseline at content area top
                // (textBaseline - ascent), shifted up by the mark's descent.
                markY = textBaselineY - ascent - markFontSize * 0.5f;
            }
            else
            {
                // Mark below text: place mark baseline below content area bottom.
                markY = textBaselineY + descent + markFontSize * 0.8f;
            }

            var markStyle = new TextStyle
            {
                Font = fontDesc,
                FontSize = markFontSize,
                Color = color,
                Bold = style.FontWeight >= 700f,
                Italic = false
            };

            // Measure the actual mark width for proper centering.
            float markWidth = target.MeasureText(markString, markStyle);
            if (markWidth <= 0)
            {
                markWidth = markFontSize * 0.8f;
            }

            if (fragment.ShapedRun != null)
            {
                PaintMarksForGlyphs(fragment, lineX, markY, markString,
                    markWidth, markStyle, target);
            }
            else if (fragment.Text != null)
            {
                PaintMarksForText(fragment, lineX, markY, markString,
                    markWidth, markStyle, target);
            }
        }

        private static void PaintMarksForGlyphs(LineFragment fragment, float lineX,
            float markY, string markString, float markWidth,
            TextStyle markStyle, IRenderTarget target)
        {
            var glyphs = fragment.ShapedRun!.Glyphs;
            string text = fragment.ShapedRun.OriginalText;
            float penX = lineX + fragment.X;

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

                // Center the mark over the glyph advance width.
                float markX = penX + (glyphAdvance - markWidth) * 0.5f;
                target.DrawText(markString, markX, markY, markStyle);
                penX += glyphAdvance;
            }
        }

        private static void PaintMarksForText(LineFragment fragment, float lineX,
            float markY, string markString, float markWidth,
            TextStyle markStyle, IRenderTarget target)
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

                float markX = penX + (charWidth - markWidth) * 0.5f;
                target.DrawText(markString, markX, markY, markStyle);
                penX += charWidth;
            }
        }

        /// <summary>
        /// Resolves the text-emphasis-style CSS value to the actual mark string.
        /// Returns null if no marks should be drawn (none or not set).
        /// </summary>
        internal static string? ResolveEmphasisMark(ComputedStyle style)
        {
            CssValue? emphasisValue = style.TextEmphasisStyle;
            if (emphasisValue == null)
            {
                return null;
            }

            if (emphasisValue is CssKeywordValue kw)
            {
                if (kw.Keyword == "none")
                {
                    return null;
                }
                return ResolveKeywordMark(kw.Keyword);
            }

            if (emphasisValue is CssStringValue sv)
            {
                return string.IsNullOrEmpty(sv.Value) ? null : sv.Value;
            }

            // Space-separated list: e.g. "filled circle", "open dot"
            if (emphasisValue is CssListValue list && list.Separator == ' ')
            {
                bool filled = true;
                string? shape = null;

                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (list.Values[i] is CssKeywordValue lkw)
                    {
                        switch (lkw.Keyword)
                        {
                            case "none": return null;
                            case "filled": filled = true; break;
                            case "open": filled = false; break;
                            default: shape = lkw.Keyword; break;
                        }
                    }
                    else if (list.Values[i] is CssStringValue lsv)
                    {
                        return string.IsNullOrEmpty(lsv.Value) ? null : lsv.Value;
                    }
                }

                if (shape != null)
                {
                    return GetShapeMark(shape, filled);
                }
                // Just "filled" or "open" without explicit shape → default circle
                return filled ? "\u25CF" : "\u25CB";
            }

            return null;
        }

        private static string? ResolveKeywordMark(string keyword)
        {
            return keyword switch
            {
                "filled" => "\u25CF",        // ● filled circle
                "open" => "\u25CB",          // ○ open circle
                "dot" => "\u2022",           // • filled dot
                "circle" => "\u25CF",        // ● filled circle
                "double-circle" => "\u25C9", // ◉ filled double-circle
                "triangle" => "\u25B2",      // ▲ filled triangle
                "sesame" => "\uFE45",        // ﹅ filled sesame
                _ => null
            };
        }

        private static string GetShapeMark(string shape, bool filled)
        {
            return shape switch
            {
                "dot" => filled ? "\u2022" : "\u25E6",
                "circle" => filled ? "\u25CF" : "\u25CB",
                "double-circle" => filled ? "\u25C9" : "\u25CE",
                "triangle" => filled ? "\u25B2" : "\u25B3",
                "sesame" => filled ? "\uFE45" : "\uFE46",
                _ => filled ? "\u25CF" : "\u25CB"
            };
        }

        /// <summary>
        /// Resolves text-emphasis-position to determine if marks appear above (over) text.
        /// Default is "over" for horizontal writing modes.
        /// </summary>
        private static bool ResolveEmphasisPositionOver(ComputedStyle style)
        {
            CssValue? posValue = style.TextEmphasisPosition;
            if (posValue == null)
            {
                return true;
            }

            if (posValue is CssKeywordValue kw)
            {
                return kw.Keyword != "under";
            }

            if (posValue is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (list.Values[i] is CssKeywordValue lkw && lkw.Keyword == "under")
                    {
                        return false;
                    }
                }
            }

            return true;
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
    }
}
