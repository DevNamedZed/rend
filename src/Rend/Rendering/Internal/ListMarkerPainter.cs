using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Fonts;
using Rend.Layout;
using Rend.Layout.Internal;
using Rend.Style;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints list markers (bullets, counters, and images) for list-item boxes.
    /// </summary>
    internal static class ListMarkerPainter
    {
        /// <summary>
        /// Chrome's default marker offset is based on the UA stylesheet's
        /// padding-inline-start (40px) minus typical text width. We approximate
        /// with a font-size-proportional value that better matches Chrome.
        /// </summary>
        private static float GetMarkerOffset(float fontSize)
        {
            // Chrome positions outside markers roughly 0.5em from the content edge
            return fontSize * 0.5f;
        }

        /// <summary>
        /// Paints the list marker for a list-item box. If list-style-image is set,
        /// draws the image. Otherwise, for disc, circle, and square types, draws a
        /// graphical marker. For numeric and alpha types, draws counter text.
        /// </summary>
        public static void Paint(LayoutBox box, IRenderTarget target, int itemIndex,
                                 ImageResolverDelegate? imageResolver = null)
        {
            if (box.BoxType != BoxType.ListItem)
            {
                return;
            }

            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
            {
                return;
            }

            // Check for <summary> element — render disclosure triangle instead of normal marker
            if (box.StyledNode is StyledElement styledEl && styledEl.TagName == "summary")
            {
                bool isOpen = false;
                // Check if parent <details> has 'open' attribute
                var parentEl = styledEl.Element.Parent as Rend.Html.Element;
                if (parentEl != null && parentEl.TagName == "details")
                {
                    isOpen = parentEl.GetAttribute("open") != null;
                }
                PaintDisclosureTriangle(target, box, style, isOpen);
                return;
            }

            CssListStyleType listType = style.ListStyleType;

            RectF contentRect = box.ContentRect;
            CssColor color = style.Color;
            float fontSize = style.FontSize;
            bool isInside = style.ListStylePosition == CssListStylePosition.Inside;


            // Try list-style-image first
            if (imageResolver != null && TryPaintImageMarker(box, target, style, imageResolver, contentRect, fontSize, isInside))
            {
                return;
            }

            if (listType == CssListStyleType.None)
            {
                return;
            }

            // Chrome sizes bullets at ~0.3em diameter
            float bulletRadius = fontSize * 0.15f;

            // Compute actual pixel line-height for the first line
            float rawLh = style.LineHeight;
            float pixelLineHeight;
            if (float.IsNaN(rawLh) || rawLh == 0) // NaN = normal (font metrics), 0 = legacy normal
                pixelLineHeight = fontSize * 1.2f;
            else if (rawLh < 0) // Negative = unitless multiplier (e.g., -1.4 for line-height: 1.4)
                pixelLineHeight = Math.Abs(rawLh) * fontSize;
            else
                pixelLineHeight = rawLh; // Already in pixels

            // [CSS-LISTS-3 §3] Chrome renders every marker — including the disc,
            // circle, and square bullets — as text via the ::marker pseudo-element.
            // The marker string always ends with a trailing space that forms the
            // visual gap between the marker and the content.
            string? markerText = ListMarkerTextBuilder.BuildMarkerText(listType, itemIndex);

            if (markerText != null)
            {
                PaintCounterText(target, markerText, contentRect, color, fontSize, style, isInside);
            }
        }

        private static bool TryPaintImageMarker(LayoutBox box, IRenderTarget target,
            ComputedStyle style, ImageResolverDelegate imageResolver,
            RectF contentRect, float fontSize, bool isInside)
        {
            object? imageRef = style.GetRefValue(PropertyId.ListStyleImage);
            if (imageRef == null) return false;

            string? imageUrl = null;
            if (imageRef is string s && s != "none")
            {
                imageUrl = s;
            }
            else if (imageRef is CssUrlValue urlVal)
            {
                imageUrl = urlVal.Url;
            }

            if (imageUrl == null) return false;

            ImageData? imageData = imageResolver(imageUrl);
            if (imageData == null) return false;

            // Size marker image to fontSize x fontSize
            float size = fontSize;
            float markerY = contentRect.Y;
            float markerX;
            if (isInside)
            {
                markerX = contentRect.X;
            }
            else
            {
                markerX = contentRect.X - size - 4f;
            }

            target.DrawImage(imageData, new RectF(markerX, markerY, size, size));
            return true;
        }

        private static void PaintDisc(IRenderTarget target, float cx, float cy, float radius, CssColor color)
        {
            // Approximate a filled circle with a small rounded rectangle.
            var rect = new RectF(cx - radius, cy - radius, radius * 2f, radius * 2f);
            var path = new PathData();
            path.AddRoundedRectangle(rect, radius, radius, radius, radius);
            target.FillPath(path, BrushInfo.Solid(color));
        }

        private static void PaintCircle(IRenderTarget target, float cx, float cy, float radius, CssColor color)
        {
            // Approximate a stroked circle with a rounded rectangle outline.
            var rect = new RectF(cx - radius, cy - radius, radius * 2f, radius * 2f);
            var path = new PathData();
            path.AddRoundedRectangle(rect, radius, radius, radius, radius);
            var pen = new PenInfo(color, 0.8f);
            target.StrokePath(path, pen);
        }

        private static void PaintSquare(IRenderTarget target, float cx, float cy, float radius, CssColor color)
        {
            float size = radius * 1.6f;
            var rect = new RectF(cx - size * 0.5f, cy - size * 0.5f, size, size);
            target.FillRect(rect, BrushInfo.Solid(color));
        }

        private static void PaintCounterText(IRenderTarget target, string text, RectF contentRect,
                                               CssColor color, float fontSize, ComputedStyle style,
                                               bool isInside)
        {
            string[] fontFamilies = style.FontFamilies;
            float fontWeight = style.FontWeight;
            CssFontStyle fontStyle = style.FontStyle;

            var textStyle = new TextStyle
            {
                Font = new FontDescriptor(fontFamilies, fontWeight, fontStyle, FontDescriptor.StretchToPercentage(style.FontStretch)),
                FontSize = fontSize,
                Color = color,
                Bold = fontWeight >= 700f,
                Italic = fontStyle == CssFontStyle.Italic || fontStyle == CssFontStyle.Oblique
            };

            // Compute pixel line height for vertical centering. For "normal" line-height,
            // delegate to GetNormalLineHeight so we use the same metric-based value as
            // the inline text layout (font metrics, not 1.2x fontSize).
            var (ascent, descent) = target.GetFontMetrics(textStyle.Font, fontSize);
            float rawLh = style.LineHeight;
            float pixelLineHeight;
            if (float.IsNaN(rawLh) || rawLh == 0)
            {
                float normalLh = target.GetNormalLineHeight(textStyle.Font, fontSize);
                pixelLineHeight = (!float.IsNaN(normalLh) && normalLh > 0) ? normalLh : fontSize * 1.2f;
            }
            else if (rawLh < 0)
            {
                pixelLineHeight = Math.Abs(rawLh) * fontSize;
            }
            else
            {
                pixelLineHeight = rawLh;
            }

            // Position at baseline matching TextPainter's snapping for inline text.
            // CSS half-leading: contentArea = ascent + descent (font metrics).
            // halfLeading = (lineHeight - contentArea) / 2
            // baseline Y from top of line box = halfLeading + ascent
            // TextPainter then snaps: Math.Floor(Math.Round(lineY) + baseline)
            float contentArea = ascent + descent;
            float baselineFromTop = (pixelLineHeight - contentArea) / 2f + ascent;
            float y = (float)Math.Floor(Math.Round(contentRect.Y) + baselineFromTop);

            if (isInside)
            {
                // Inside: draw at the start of the content area.
                float x = contentRect.X;
                target.DrawText(text, x, y, textStyle);
            }
            else
            {
                // Outside: right-align marker text so its trailing space ends at the content edge.
                // Chrome positions outside markers this way — the trailing space in the marker
                // text creates the visual gap between marker and content.
                float textWidth = target.MeasureText(text, textStyle);
                if (textWidth < 0) textWidth = text.Length * fontSize * 0.6f; // fallback
                float x = contentRect.X - textWidth;
                target.DrawText(text, x, y, textStyle);
            }
        }

        /// <summary>
        /// Paint a disclosure triangle for a &lt;summary&gt; element.
        /// ▼ for open, ▶ for closed.
        /// Chrome renders these as Unicode text glyphs (U+25BC/U+25B6) in list_marker.cc,
        /// falling back to Segoe UI Symbol or similar. We draw as paths to avoid font fallback
        /// issues, but match Chrome's sizing and positioning.
        /// </summary>
        private static void PaintDisclosureTriangle(IRenderTarget target, LayoutBox box,
            ComputedStyle style, bool isOpen)
        {
            RectF contentRect = box.ContentRect;
            CssColor color = style.Color;
            float fontSize = style.FontSize;

            // Chrome's disclosure triangle is roughly 0.5em
            float size = fontSize * 0.5f;

            // Compute pixel line height for vertical centering
            float rawLh = style.LineHeight;
            float pixelLineHeight;
            if (float.IsNaN(rawLh) || rawLh == 0) pixelLineHeight = fontSize * 1.2f;
            else if (rawLh < 0) pixelLineHeight = Math.Abs(rawLh) * fontSize;
            else pixelLineHeight = rawLh;

            float centerY = contentRect.Y + pixelLineHeight * 0.5f;
            float centerX = contentRect.X + size * 0.5f + 1f;

            var path = new PathData();
            if (isOpen)
            {
                // Downward pointing triangle ▼
                float halfW = size * 0.5f;
                float halfH = size * 0.4f;
                path.MoveTo(centerX - halfW, centerY - halfH);
                path.LineTo(centerX + halfW, centerY - halfH);
                path.LineTo(centerX, centerY + halfH);
                path.Close();
            }
            else
            {
                // Right pointing triangle ▶
                float halfW = size * 0.4f;
                float halfH = size * 0.5f;
                path.MoveTo(centerX - halfW, centerY - halfH);
                path.LineTo(centerX + halfW, centerY);
                path.LineTo(centerX - halfW, centerY + halfH);
                path.Close();
            }

            target.FillPath(path, BrushInfo.Solid(color));
        }
    }
}
