using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints CSS border-image onto a layout box.
    /// [CSS-BACKGROUNDS-3 §5] Implements the 9-slice model with stretch, repeat,
    /// round, and space tiling modes for edge regions.
    /// </summary>
    internal static class BorderImagePainter
    {
        private enum RepeatMode { Stretch, Repeat, Round, Space }

        /// <summary>
        /// Returns true if this box has a border-image set (and border painting should be skipped).
        /// </summary>
        public static bool HasBorderImage(LayoutBox box)
        {
            if (box.StyledNode?.Style == null) return false;
            var source = box.StyledNode.Style.GetRefValue(PropertyId.BorderImageSource);
            if (source == null) return false;
            if (source is CssKeywordValue kw && kw.Keyword == "none") return false;
            return true;
        }

        public static void Paint(LayoutBox box, IRenderTarget target,
            ImageResolverDelegate? imageResolver = null)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null) return;

            var source = style.GetRefValue(PropertyId.BorderImageSource);
            if (source == null) return;
            if (source is CssKeywordValue kw && kw.Keyword == "none") return;

            RectF borderRect = box.BorderRect;

            // [CSS-BACKGROUNDS-3 §5.4] Outset expands the border image area beyond
            // the border box. Must be computed before border-image-width since
            // percentage widths are relative to the expanded area.
            ResolveOutsetValues(style.GetRefValue(PropertyId.BorderImageOutset),
                out float outsetTop, out float outsetRight, out float outsetBottom, out float outsetLeft);
            RectF imageArea = new RectF(
                borderRect.X - outsetLeft,
                borderRect.Y - outsetTop,
                borderRect.Width + outsetLeft + outsetRight,
                borderRect.Height + outsetTop + outsetBottom);

            // [CSS-BACKGROUNDS-3 §5.2] border-image-width overrides border-width
            // for the image drawing area. Percentages are relative to the border
            // image area (after outset). Numbers multiply the border-width.
            float borderTop = style.BorderTopWidth;
            float borderRight = style.BorderRightWidth;
            float borderBottom = style.BorderBottomWidth;
            float borderLeft = style.BorderLeftWidth;

            ResolveImageWidth(style.GetRefValue(PropertyId.BorderImageWidth),
                borderTop, borderRight, borderBottom, borderLeft,
                imageArea.Width, imageArea.Height,
                out float topW, out float rightW, out float bottomW, out float leftW);

            if (topW <= 0 && rightW <= 0 && bottomW <= 0 && leftW <= 0)
            {
                return;
            }

            // [CSS-BACKGROUNDS-3 §5.1] Check fill keyword from border-image-slice
            // for gradient borders (image borders parse this in PaintImageBorder).
            bool hasFill = HasFillKeyword(style.GetRefValue(PropertyId.BorderImageSlice));

            // [CSS-BACKGROUNDS-3 §5.2] Determine source type: gradient or URL image.
            if (source is CssFunctionValue fn)
            {
                if (fn.Name == "url")
                {
                    string? fnUrl = fn.Arguments.Count > 0 ? ExtractUrlFromArgument(fn.Arguments[0]) : null;
                    if (!string.IsNullOrEmpty(fnUrl) && imageResolver != null)
                    {
                        ImageData? fnImage = imageResolver(fnUrl!);
                        if (fnImage != null && fnImage.Width > 0 && fnImage.Height > 0)
                        {
                            PaintImageBorder(target, style, fnImage, imageArea, topW, rightW, bottomW, leftW);
                        }
                    }
                    return;
                }

                PaintGradientBorder(target, fn, imageArea, topW, rightW, bottomW, leftW, hasFill);
                return;
            }

            // URL source (CssUrlValue or string)
            string? url = source as string;
            if (url == null && source is CssUrlValue urlVal) url = urlVal.Url;
            if (url == null && source is CssStringValue strVal) url = strVal.Value;
            if (string.IsNullOrEmpty(url) || imageResolver == null) return;

            ImageData? imageData = imageResolver(url!);
            if (imageData == null || imageData.Width <= 0 || imageData.Height <= 0) return;

            PaintImageBorder(target, style, imageData, imageArea, topW, rightW, bottomW, leftW);
        }

        // ────────────────────────────────────────────────────────────────
        //  9-Slice image border
        // ────────────────────────────────────────────────────────────────

        private static void PaintImageBorder(IRenderTarget target, ComputedStyle style,
            ImageData image, RectF area, float top, float right, float bottom, float left)
        {
            float imgW = image.Width;
            float imgH = image.Height;

            // [CSS-BACKGROUNDS-3 §5.1] Parse border-image-slice
            var sliceRaw = style.GetRefValue(PropertyId.BorderImageSlice);
            ParseSlice(sliceRaw, imgW, imgH,
                out float sliceTop, out float sliceRight, out float sliceBottom, out float sliceLeft,
                out bool fill);

            // [CSS-BACKGROUNDS-3 §5.4] Parse border-image-repeat
            ParseRepeat(style.GetRefValue(PropertyId.BorderImageRepeat),
                out RepeatMode horizontalRepeat, out RepeatMode verticalRepeat);

            // Source rects (in image pixel coordinates)
            float srcMidX = sliceLeft;
            float srcMidW = imgW - sliceLeft - sliceRight;
            float srcMidY = sliceTop;
            float srcMidH = imgH - sliceTop - sliceBottom;

            // Dest rects (in layout coordinates)
            float destMidX = area.X + left;
            float destMidW = area.Width - left - right;
            float destMidY = area.Y + top;
            float destMidH = area.Height - top - bottom;

            // --- Corners (always stretched) ---
            if (sliceTop > 0 && sliceLeft > 0 && top > 0 && left > 0)
            {
                target.DrawImageRegion(image,
                    new RectF(0, 0, sliceLeft, sliceTop),
                    new RectF(area.X, area.Y, left, top));
            }
            if (sliceTop > 0 && sliceRight > 0 && top > 0 && right > 0)
            {
                target.DrawImageRegion(image,
                    new RectF(imgW - sliceRight, 0, sliceRight, sliceTop),
                    new RectF(area.X + area.Width - right, area.Y, right, top));
            }
            if (sliceBottom > 0 && sliceLeft > 0 && bottom > 0 && left > 0)
            {
                target.DrawImageRegion(image,
                    new RectF(0, imgH - sliceBottom, sliceLeft, sliceBottom),
                    new RectF(area.X, area.Y + area.Height - bottom, left, bottom));
            }
            if (sliceBottom > 0 && sliceRight > 0 && bottom > 0 && right > 0)
            {
                target.DrawImageRegion(image,
                    new RectF(imgW - sliceRight, imgH - sliceBottom, sliceRight, sliceBottom),
                    new RectF(area.X + area.Width - right, area.Y + area.Height - bottom, right, bottom));
            }

            // --- Edges ---
            if (srcMidW > 0 && top > 0 && destMidW > 0)
            {
                var srcRect = new RectF(srcMidX, 0, srcMidW, sliceTop);
                var destRect = new RectF(destMidX, area.Y, destMidW, top);
                DrawEdge(target, image, srcRect, destRect, horizontalRepeat, true);
            }
            if (srcMidW > 0 && bottom > 0 && destMidW > 0)
            {
                var srcRect = new RectF(srcMidX, imgH - sliceBottom, srcMidW, sliceBottom);
                var destRect = new RectF(destMidX, area.Y + area.Height - bottom, destMidW, bottom);
                DrawEdge(target, image, srcRect, destRect, horizontalRepeat, true);
            }
            if (srcMidH > 0 && left > 0 && destMidH > 0)
            {
                var srcRect = new RectF(0, srcMidY, sliceLeft, srcMidH);
                var destRect = new RectF(area.X, destMidY, left, destMidH);
                DrawEdge(target, image, srcRect, destRect, verticalRepeat, false);
            }
            if (srcMidH > 0 && right > 0 && destMidH > 0)
            {
                var srcRect = new RectF(imgW - sliceRight, srcMidY, sliceRight, srcMidH);
                var destRect = new RectF(area.X + area.Width - right, destMidY, right, destMidH);
                DrawEdge(target, image, srcRect, destRect, verticalRepeat, false);
            }

            // --- Center (only if fill keyword present) ---
            if (fill && srcMidW > 0 && srcMidH > 0 && destMidW > 0 && destMidH > 0)
            {
                target.DrawImageRegion(image,
                    new RectF(srcMidX, srcMidY, srcMidW, srcMidH),
                    new RectF(destMidX, destMidY, destMidW, destMidH));
            }
        }

        /// <summary>
        /// [CSS-BACKGROUNDS-3 §5.4] Draws an edge slice with the specified repeat mode.
        /// </summary>
        private static void DrawEdge(IRenderTarget target, ImageData image,
            RectF srcRect, RectF destRect, RepeatMode mode, bool horizontal)
        {
            if (mode == RepeatMode.Stretch)
            {
                target.DrawImageRegion(image, srcRect, destRect);
                return;
            }

            float naturalTileSize = horizontal ? srcRect.Width : srcRect.Height;
            float destLength = horizontal ? destRect.Width : destRect.Height;

            // Scale factor to fit tile into the dest cross-axis dimension
            float crossScale = horizontal
                ? destRect.Height / srcRect.Height
                : destRect.Width / srcRect.Width;
            float scaledTileSize = naturalTileSize * crossScale;

            if (scaledTileSize <= 0)
            {
                return;
            }

            float tileSize;
            int tileCount;
            float startOffset;

            switch (mode)
            {
                case RepeatMode.Round:
                    // [CSS-BACKGROUNDS-3 §5.4] Round: rescale tile so it fits a whole number of times
                    tileCount = Math.Max(1, (int)Math.Round(destLength / scaledTileSize, MidpointRounding.AwayFromZero));
                    tileSize = destLength / tileCount;
                    startOffset = 0;
                    break;

                case RepeatMode.Space:
                    // [CSS-BACKGROUNDS-3 §5.4] Space: whole tiles, distribute extra space as gaps
                    tileCount = Math.Max(1, (int)Math.Floor(destLength / scaledTileSize));
                    tileSize = scaledTileSize;
                    if (tileCount == 1)
                    {
                        startOffset = (destLength - tileSize) / 2;
                    }
                    else
                    {
                        startOffset = 0;
                    }
                    break;

                default: // Repeat
                    // [CSS-BACKGROUNDS-3 §5.4] Repeat: center the tiled pattern, clip excess
                    tileSize = scaledTileSize;
                    tileCount = (int)Math.Ceiling(destLength / tileSize);
                    startOffset = (destLength - tileCount * tileSize) / 2;
                    break;
            }

            target.Save();
            target.PushClipRect(destRect);

            if (mode == RepeatMode.Space && tileCount > 1)
            {
                // [CSS-BACKGROUNDS-3 §5.4] Space distributes extra space "around"
                // tiles: before the first, between each pair, and after the last.
                float gap = (destLength - tileCount * tileSize) / (tileCount + 1);
                for (int i = 0; i < tileCount; i++)
                {
                    float offset = gap + i * (tileSize + gap);
                    RectF tileDest;
                    if (horizontal)
                    {
                        tileDest = new RectF(destRect.X + offset, destRect.Y, tileSize, destRect.Height);
                    }
                    else
                    {
                        tileDest = new RectF(destRect.X, destRect.Y + offset, destRect.Width, tileSize);
                    }
                    target.DrawImageRegion(image, srcRect, tileDest);
                }
            }
            else
            {
                for (int i = 0; i < tileCount; i++)
                {
                    float offset = startOffset + i * tileSize;
                    RectF tileDest;
                    if (horizontal)
                    {
                        tileDest = new RectF(destRect.X + offset, destRect.Y, tileSize, destRect.Height);
                    }
                    else
                    {
                        tileDest = new RectF(destRect.X, destRect.Y + offset, destRect.Width, tileSize);
                    }
                    target.DrawImageRegion(image, srcRect, tileDest);
                }
            }

            target.PopClip();
            target.Restore();
        }

        // ────────────────────────────────────────────────────────────────
        //  Gradient border (unchanged)
        // ────────────────────────────────────────────────────────────────

        private static void PaintGradientBorder(IRenderTarget target, CssFunctionValue fn,
            RectF area, float top, float right, float bottom, float left, bool fill)
        {
            var gradient = BackgroundPainter.ParseCssGradient(fn, area);
            BrushInfo brush;
            if (gradient != null)
            {
                brush = BrushInfo.FromGradient(gradient);
            }
            else
            {
                CssColor? color = ExtractFirstGradientColor(fn);
                if (!color.HasValue)
                {
                    return;
                }
                brush = BrushInfo.Solid(color.Value);
            }

            if (fill)
            {
                // [CSS-BACKGROUNDS-3 §5.4] fill keyword: paint entire image area
                target.FillRect(area, brush);
            }
            else
            {
                // Paint only the border ring (outer - inner) via EvenOdd path
                var innerRect = new RectF(
                    area.X + left,
                    area.Y + top,
                    area.Width - left - right,
                    area.Height - top - bottom);

                var borderPath = new PathData();
                borderPath.MoveTo(area.X, area.Y);
                borderPath.LineTo(area.X + area.Width, area.Y);
                borderPath.LineTo(area.X + area.Width, area.Y + area.Height);
                borderPath.LineTo(area.X, area.Y + area.Height);
                borderPath.Close();
                borderPath.MoveTo(innerRect.X, innerRect.Y);
                borderPath.LineTo(innerRect.X, innerRect.Y + innerRect.Height);
                borderPath.LineTo(innerRect.X + innerRect.Width, innerRect.Y + innerRect.Height);
                borderPath.LineTo(innerRect.X + innerRect.Width, innerRect.Y);
                borderPath.Close();

                target.FillPath(borderPath, brush);
            }
        }

        // ────────────────────────────────────────────────────────────────
        //  Parsing helpers
        // ────────────────────────────────────────────────────────────────

        private static bool HasFillKeyword(object? sliceRaw)
        {
            if (sliceRaw is CssKeywordValue kw && kw.Keyword == "fill")
            {
                return true;
            }
            if (sliceRaw is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (HasFillKeyword(list.Values[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// [CSS-BACKGROUNDS-3 §5.1] Parses border-image-slice values.
        /// Syntax: [&lt;number&gt; | &lt;percentage&gt;]{1,4} &amp;&amp; fill?
        /// Numbers are pixel offsets, percentages relative to image dimension.
        /// </summary>
        private static void ParseSlice(object? raw, float imgW, float imgH,
            out float sliceTop, out float sliceRight, out float sliceBottom, out float sliceLeft,
            out bool fill)
        {
            fill = false;
            // Default: 100% (entire image used as border per spec initial value)
            sliceTop = imgH;
            sliceRight = imgW;
            sliceBottom = imgH;
            sliceLeft = imgW;

            if (raw == null)
            {
                return;
            }

            var values = new float[4];
            bool[] isPercent = new bool[4];
            int count = 0;

            if (raw is CssValue cssValue)
            {
                ExtractSliceValues(cssValue, ref values, ref isPercent, ref count, ref fill);
            }

            if (count == 0)
            {
                return;
            }

            // CSS shorthand expansion: 1→all, 2→TB/LR, 3→T/LR/B, 4→T/R/B/L
            float[] resolved = new float[4];
            bool[] resolvedPct = new bool[4];
            resolved[0] = values[0]; resolvedPct[0] = isPercent[0];
            resolved[1] = count >= 2 ? values[1] : values[0];
            resolvedPct[1] = count >= 2 ? isPercent[1] : isPercent[0];
            resolved[2] = count >= 3 ? values[2] : values[0];
            resolvedPct[2] = count >= 3 ? isPercent[2] : isPercent[0];
            resolved[3] = count >= 4 ? values[3] : resolved[1];
            resolvedPct[3] = count >= 4 ? isPercent[3] : resolvedPct[1];

            sliceTop = resolvedPct[0] ? resolved[0] / 100f * imgH : resolved[0];
            sliceRight = resolvedPct[1] ? resolved[1] / 100f * imgW : resolved[1];
            sliceBottom = resolvedPct[2] ? resolved[2] / 100f * imgH : resolved[2];
            sliceLeft = resolvedPct[3] ? resolved[3] / 100f * imgW : resolved[3];

            // Clamp so slices don't overlap
            sliceTop = Math.Max(0, Math.Min(sliceTop, imgH));
            sliceRight = Math.Max(0, Math.Min(sliceRight, imgW));
            sliceBottom = Math.Max(0, Math.Min(sliceBottom, imgH));
            sliceLeft = Math.Max(0, Math.Min(sliceLeft, imgW));

            if (sliceTop + sliceBottom > imgH)
            {
                float factor = imgH / (sliceTop + sliceBottom);
                sliceTop *= factor;
                sliceBottom *= factor;
            }
            if (sliceLeft + sliceRight > imgW)
            {
                float factor = imgW / (sliceLeft + sliceRight);
                sliceLeft *= factor;
                sliceRight *= factor;
            }
        }

        private static void ExtractSliceValues(CssValue value,
            ref float[] values, ref bool[] isPercent, ref int count, ref bool fill)
        {
            if (value is CssNumberValue num && count < 4)
            {
                values[count] = num.Value;
                isPercent[count] = false;
                count++;
            }
            else if (value is CssPercentageValue pct && count < 4)
            {
                values[count] = pct.Value;
                isPercent[count] = true;
                count++;
            }
            else if (value is CssKeywordValue kw && kw.Keyword == "fill")
            {
                fill = true;
            }
            else if (value is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    ExtractSliceValues(list.Values[i], ref values, ref isPercent, ref count, ref fill);
                }
            }
        }

        /// <summary>
        /// [CSS-BACKGROUNDS-3 §5.4] Parses border-image-repeat.
        /// Syntax: [stretch | repeat | round | space]{1,2}
        /// First = horizontal (top/bottom), second = vertical (left/right).
        /// </summary>
        private static void ParseRepeat(object? raw,
            out RepeatMode horizontal, out RepeatMode vertical)
        {
            horizontal = RepeatMode.Stretch;
            vertical = RepeatMode.Stretch;

            if (raw is CssKeywordValue kw)
            {
                horizontal = ParseRepeatKeyword(kw.Keyword);
                vertical = horizontal;
            }
            else if (raw is CssListValue list && list.Values.Count >= 1)
            {
                if (list.Values[0] is CssKeywordValue first)
                {
                    horizontal = ParseRepeatKeyword(first.Keyword);
                }
                vertical = horizontal;
                if (list.Values.Count >= 2 && list.Values[1] is CssKeywordValue second)
                {
                    vertical = ParseRepeatKeyword(second.Keyword);
                }
            }
        }

        private static RepeatMode ParseRepeatKeyword(string keyword)
        {
            switch (keyword)
            {
                case "repeat": return RepeatMode.Repeat;
                case "round": return RepeatMode.Round;
                case "space": return RepeatMode.Space;
                default: return RepeatMode.Stretch;
            }
        }

        private static CssColor? ExtractFirstGradientColor(CssFunctionValue fn)
        {
            for (int i = 0; i < fn.Arguments.Count; i++)
            {
                if (fn.Arguments[i] is CssColorValue cv)
                {
                    return cv.Color;
                }
                if (fn.Arguments[i] is CssKeywordValue kw)
                {
                    if (Rend.Css.Parser.Internal.CssColorParser.TryParseNamed(kw.Keyword, out var named))
                    {
                        return named;
                    }
                }
            }
            return null;
        }

        private static string? ExtractUrlFromArgument(CssValue argument)
        {
            if (argument is CssStringValue str)
            {
                return str.Value;
            }
            if (argument is CssUrlValue urlVal)
            {
                return urlVal.Url;
            }
            return argument.ToString();
        }

        /// <summary>
        /// [CSS-BACKGROUNDS-3 §5.4] Parses border-image-outset (1-4 values).
        /// Values are lengths or bare numbers (multiples of border-width).
        /// </summary>
        private static void ResolveOutsetValues(object? raw,
            out float top, out float right, out float bottom, out float left)
        {
            top = right = bottom = left = 0;
            if (raw == null) return;

            var values = new float[4];
            int count = 0;
            CollectLengthOrNumber(raw, ref values, ref count);

            if (count == 0) return;

            top = values[0];
            right = count >= 2 ? values[1] : values[0];
            bottom = count >= 3 ? values[2] : values[0];
            left = count >= 4 ? values[3] : (count >= 2 ? values[1] : values[0]);
        }

        /// <summary>
        /// [CSS-BACKGROUNDS-3 §5.2] Parses border-image-width (1-4 values).
        /// Values: length, number (multiplier of border-width), percentage, or auto.
        /// Initial value is 1 (= 1× corresponding border-width).
        /// </summary>
        private static void ResolveImageWidth(object? raw,
            float borderTop, float borderRight, float borderBottom, float borderLeft,
            float areaWidth, float areaHeight,
            out float top, out float right, out float bottom, out float left)
        {
            // Initial value: 1 (= 1× border-width)
            if (raw == null || (raw is CssKeywordValue kw && kw.Keyword == "auto"))
            {
                top = borderTop;
                right = borderRight;
                bottom = borderBottom;
                left = borderLeft;
                return;
            }

            var values = new CssValue[4];
            int count = 0;
            CollectImageWidthValues(raw, ref values, ref count);

            if (count == 0)
            {
                top = borderTop;
                right = borderRight;
                bottom = borderBottom;
                left = borderLeft;
                return;
            }

            // CSS shorthand expansion: 1→all, 2→TB/LR, 3→T/LR/B, 4→T/R/B/L
            CssValue topVal = values[0];
            CssValue rightVal = count >= 2 ? values[1] : values[0];
            CssValue bottomVal = count >= 3 ? values[2] : values[0];
            CssValue leftVal = count >= 4 ? values[3] : (count >= 2 ? values[1] : values[0]);

            top = ResolveSingleImageWidth(topVal, borderTop, areaHeight);
            right = ResolveSingleImageWidth(rightVal, borderRight, areaWidth);
            bottom = ResolveSingleImageWidth(bottomVal, borderBottom, areaHeight);
            left = ResolveSingleImageWidth(leftVal, borderLeft, areaWidth);
        }

        private static float ResolveSingleImageWidth(CssValue value,
            float correspondingBorder, float referenceSize)
        {
            if (value is CssNumberValue num)
            {
                return num.Value * correspondingBorder;
            }
            if (value is CssDimensionValue dim)
            {
                return dim.Value;
            }
            if (value is CssPercentageValue pct)
            {
                return pct.Value / 100f * referenceSize;
            }
            // auto → use border-width
            return correspondingBorder;
        }

        private static void CollectLengthOrNumber(object? raw, ref float[] values, ref int count)
        {
            if (raw is CssNumberValue num && count < 4)
            {
                values[count++] = num.Value;
            }
            else if (raw is CssDimensionValue dim && count < 4)
            {
                values[count++] = dim.Value;
            }
            else if (raw is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count && count < 4; i++)
                {
                    CollectLengthOrNumber(list.Values[i], ref values, ref count);
                }
            }
        }

        private static void CollectImageWidthValues(object? raw, ref CssValue[] values, ref int count)
        {
            if (raw is CssNumberValue && count < 4)
            {
                values[count++] = (CssValue)raw;
            }
            else if (raw is CssDimensionValue && count < 4)
            {
                values[count++] = (CssValue)raw;
            }
            else if (raw is CssPercentageValue && count < 4)
            {
                values[count++] = (CssValue)raw;
            }
            else if (raw is CssKeywordValue && count < 4)
            {
                values[count++] = (CssValue)raw;
            }
            else if (raw is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count && count < 4; i++)
                {
                    CollectImageWidthValues(list.Values[i], ref values, ref count);
                }
            }
        }
    }
}
