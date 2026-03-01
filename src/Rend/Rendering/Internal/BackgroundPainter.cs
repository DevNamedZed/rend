using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Parser.Internal;
using Rend.Css.Properties.Internal;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Paints the background of a layout box, including solid colors, gradients, and images.
    /// Per CSS spec, background-color is painted first, then background-image on top.
    /// </summary>
    internal static class BackgroundPainter
    {
        /// <summary>
        /// Paints the background for the given box onto the render target.
        /// </summary>
        public static void Paint(LayoutBox box, IRenderTarget target,
            ImageResolverDelegate? imageResolver = null)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
            {
                return;
            }

            // Determine clip and origin rects based on background-clip / background-origin
            RectF clipRect = ResolveBoxRect(box, style.BackgroundClip);
            RectF originRect = ResolveBoxRect(box, (CssBackgroundClip)(int)style.BackgroundOrigin);

            // Border-radius for rounded backgrounds.
            float tlr = style.BorderTopLeftRadius;
            float trr = style.BorderTopRightRadius;
            float brr = style.BorderBottomRightRadius;
            float blr = style.BorderBottomLeftRadius;
            bool hasRadius = tlr > 0f || trr > 0f || brr > 0f || blr > 0f;

            // 1. Paint background-color (clipped to background-clip area).
            CssColor bgColor = style.BackgroundColor;
            if (bgColor.A > 0)
            {
                BrushInfo brush = BrushInfo.Solid(bgColor);
                if (hasRadius)
                {
                    var path = new PathData();
                    path.AddRoundedRectangle(clipRect, tlr, trr, brr, blr);
                    target.FillPath(path, brush);
                }
                else
                {
                    target.FillRect(clipRect, brush);
                }
            }

            // 2. Paint background-image (URL images or CSS gradients).
            object? bgImageRef = style.GetRefValue(PropertyId.BackgroundImage);

            // Check for CSS gradient functions
            if (bgImageRef is CssFunctionValue gradientFn)
            {
                var gradient = ParseCssGradient(gradientFn, clipRect);
                if (gradient != null)
                {
                    BrushInfo gradBrush = BrushInfo.FromGradient(gradient);
                    if (hasRadius)
                    {
                        var path = new PathData();
                        path.AddRoundedRectangle(clipRect, tlr, trr, brr, blr);
                        target.FillPath(path, gradBrush);
                    }
                    else
                    {
                        target.FillRect(clipRect, gradBrush);
                    }
                }
                return;
            }

            if (imageResolver == null)
            {
                return;
            }

            // Extract URL from CssUrlValue or string ref
            string? bgImageUrl = null;
            if (bgImageRef is CssUrlValue urlVal)
                bgImageUrl = urlVal.Url;
            else if (bgImageRef is CssKeywordValue kwRef && kwRef.Keyword != "none")
                bgImageUrl = kwRef.Keyword;
            else if (bgImageRef is string strRef)
                bgImageUrl = strRef;

            if (string.IsNullOrEmpty(bgImageUrl) || bgImageUrl == "none")
            {
                return;
            }

            ImageData? imageData = imageResolver(bgImageUrl!);
            if (imageData == null || imageData.Width <= 0 || imageData.Height <= 0)
            {
                return;
            }

            // Calculate image size (relative to background-origin area).
            float imgW = imageData.Width;
            float imgH = imageData.Height;
            ComputeBackgroundSize(style, originRect, imgW, imgH,
                out float scaledW, out float scaledH);

            // Calculate image position (relative to background-origin area).
            ComputeBackgroundPosition(style, originRect, scaledW, scaledH,
                out float posX, out float posY);

            // Get repeat mode.
            int repeatMode = style.GetRawValue(PropertyId.BackgroundRepeat).IntValue;

            // Clip to background-clip rect for tiled backgrounds.
            bool needsClip = repeatMode != (int)CssBackgroundRepeat.NoRepeat;
            if (needsClip)
            {
                if (hasRadius)
                {
                    var clipPath = new PathData();
                    clipPath.AddRoundedRectangle(clipRect, tlr, trr, brr, blr);
                    target.PushClipPath(clipPath);
                }
                else
                {
                    target.PushClipRect(clipRect);
                }
            }

            // Draw image tile(s).
            bool repeatX = repeatMode == (int)CssBackgroundRepeat.Repeat ||
                           repeatMode == (int)CssBackgroundRepeat.RepeatX;
            bool repeatY = repeatMode == (int)CssBackgroundRepeat.Repeat ||
                           repeatMode == (int)CssBackgroundRepeat.RepeatY;

            if (!repeatX && !repeatY)
            {
                // No repeat: single image.
                var destRect = new RectF(posX, posY, scaledW, scaledH);
                target.DrawImage(imageData, destRect);
            }
            else
            {
                // Tile the image.
                float startX = repeatX ? GetTileStart(posX, scaledW, clipRect.X) : posX;
                float startY = repeatY ? GetTileStart(posY, scaledH, clipRect.Y) : posY;
                float endX = repeatX ? clipRect.X + clipRect.Width : posX + scaledW;
                float endY = repeatY ? clipRect.Y + clipRect.Height : posY + scaledH;

                for (float ty = startY; ty < endY; ty += scaledH)
                {
                    for (float tx = startX; tx < endX; tx += scaledW)
                    {
                        target.DrawImage(imageData, new RectF(tx, ty, scaledW, scaledH));
                    }
                }
            }

            if (needsClip)
            {
                target.PopClip();
            }
        }

        private static RectF ResolveBoxRect(LayoutBox box, CssBackgroundClip boxArea)
        {
            switch (boxArea)
            {
                case CssBackgroundClip.BorderBox:
                    return box.BorderRect;
                case CssBackgroundClip.ContentBox:
                    return box.ContentRect;
                default: // PaddingBox
                    return box.PaddingRect;
            }
        }

        /// <summary>
        /// Gets the first tile position that's at or before the container start.
        /// </summary>
        private static float GetTileStart(float pos, float tileSize, float containerStart)
        {
            if (tileSize <= 0) return pos;
            float start = pos;
            while (start > containerStart) start -= tileSize;
            return start;
        }

        /// <summary>
        /// Computes the scaled size of the background image based on background-size.
        /// </summary>
        private static void ComputeBackgroundSize(ComputedStyle style, RectF paddingRect,
            float imgW, float imgH, out float scaledW, out float scaledH)
        {
            // Default: auto (intrinsic size)
            scaledW = imgW;
            scaledH = imgH;

            object? sizeRef = style.GetRefValue(PropertyId.BackgroundSize);
            if (sizeRef == null)
            {
                return;
            }

            if (sizeRef is CssKeywordValue kw)
            {
                switch (kw.Keyword)
                {
                    case "cover":
                    {
                        float ratioW = paddingRect.Width / imgW;
                        float ratioH = paddingRect.Height / imgH;
                        float ratio = Math.Max(ratioW, ratioH);
                        scaledW = imgW * ratio;
                        scaledH = imgH * ratio;
                        return;
                    }
                    case "contain":
                    {
                        float ratioW = paddingRect.Width / imgW;
                        float ratioH = paddingRect.Height / imgH;
                        float ratio = Math.Min(ratioW, ratioH);
                        scaledW = imgW * ratio;
                        scaledH = imgH * ratio;
                        return;
                    }
                    case "auto":
                        // Intrinsic size
                        return;
                }
            }

            if (sizeRef is CssListValue list && list.Separator == ' ' && list.Values.Count >= 2)
            {
                scaledW = ResolveSizeComponent(list.Values[0], paddingRect.Width, imgW);
                scaledH = ResolveSizeComponent(list.Values[1], paddingRect.Height, imgH);
                return;
            }

            if (sizeRef is CssDimensionValue dim)
            {
                scaledW = ResolveLengthValue(dim);
                // auto height: maintain aspect ratio
                scaledH = imgH * (scaledW / imgW);
                return;
            }

            if (sizeRef is CssPercentageValue pct)
            {
                scaledW = pct.Value / 100f * paddingRect.Width;
                scaledH = imgH * (scaledW / imgW);
                return;
            }
        }

        private static float ResolveSizeComponent(CssValue value, float containerSize, float imgSize)
        {
            if (value is CssDimensionValue dim)
            {
                return ResolveLengthValue(dim);
            }
            if (value is CssPercentageValue pct)
            {
                return pct.Value / 100f * containerSize;
            }
            if (value is CssKeywordValue kw && kw.Keyword == "auto")
            {
                return imgSize;
            }
            if (value is CssNumberValue num && num.Value == 0)
            {
                return 0;
            }
            return imgSize; // default: auto
        }

        /// <summary>
        /// Computes the position of the background image.
        /// </summary>
        private static void ComputeBackgroundPosition(ComputedStyle style, RectF paddingRect,
            float scaledW, float scaledH, out float posX, out float posY)
        {
            // Default: 0% 0% (top-left)
            posX = paddingRect.X;
            posY = paddingRect.Y;

            object? posRef = style.GetRefValue(PropertyId.BackgroundPosition);
            if (posRef == null)
            {
                return;
            }

            if (posRef is CssListValue list && list.Separator == ' ' && list.Values.Count >= 2)
            {
                posX = paddingRect.X + ResolvePositionComponent(
                    list.Values[0], paddingRect.Width, scaledW);
                posY = paddingRect.Y + ResolvePositionComponent(
                    list.Values[1], paddingRect.Height, scaledH);
                return;
            }

            // Single value — only set X, Y defaults to 50%
            if (posRef is CssValue singleValue)
            {
                posX = paddingRect.X + ResolvePositionComponent(
                    singleValue, paddingRect.Width, scaledW);
                posY = paddingRect.Y + (paddingRect.Height - scaledH) * 0.5f;
            }
        }

        private static float ResolvePositionComponent(CssValue value,
            float containerSize, float imageSize)
        {
            if (value is CssPercentageValue pct)
            {
                // CSS spec: percentage position = (container - image) * percentage
                return (containerSize - imageSize) * (pct.Value / 100f);
            }
            if (value is CssDimensionValue dim)
            {
                return ResolveLengthValue(dim);
            }
            if (value is CssNumberValue num && num.Value == 0)
            {
                return 0;
            }
            if (value is CssKeywordValue kw)
            {
                switch (kw.Keyword)
                {
                    case "left":
                    case "top":
                        return 0;
                    case "center":
                        return (containerSize - imageSize) * 0.5f;
                    case "right":
                    case "bottom":
                        return containerSize - imageSize;
                }
            }
            return 0; // default
        }

        /// <summary>
        /// Parses a CSS gradient function (linear-gradient, radial-gradient) into a GradientInfo.
        /// </summary>
        private static GradientInfo? ParseCssGradient(CssFunctionValue fn, RectF rect)
        {
            if (fn.Name == "linear-gradient" || fn.Name == "-webkit-linear-gradient")
                return ParseLinearGradient(fn, rect);
            if (fn.Name == "radial-gradient" || fn.Name == "-webkit-radial-gradient")
                return ParseRadialGradient(fn, rect);
            if (fn.Name == "conic-gradient")
                return ParseConicGradient(fn, rect);
            return null;
        }

        private static GradientInfo? ParseLinearGradient(CssFunctionValue fn, RectF rect)
        {
            if (fn.Arguments.Count == 0) return null;

            float angle = 180; // default: to bottom
            int colorStartIdx = 0;

            // Check if first argument is an angle or direction
            var first = fn.Arguments[0];
            if (first is CssDimensionValue dim && dim.Unit == "deg")
            {
                angle = dim.Value;
                colorStartIdx = 1;
            }
            else if (first is CssKeywordValue dirKw)
            {
                string dir = dirKw.Keyword;
                if (dir == "to")
                {
                    // "to" followed by direction keywords
                    string direction = "";
                    for (int i = 1; i < fn.Arguments.Count; i++)
                    {
                        if (fn.Arguments[i] is CssKeywordValue kw2)
                        {
                            direction += kw2.Keyword + " ";
                            colorStartIdx = i + 1;
                        }
                        else break;
                    }
                    angle = DirectionToAngle(direction.Trim());
                }
            }

            var stops = ParseColorStops(fn.Arguments, colorStartIdx);
            if (stops == null || stops.Length < 2) return null;

            return new GradientInfo(GradientType.Linear, stops) { Angle = angle };
        }

        private static GradientInfo? ParseRadialGradient(CssFunctionValue fn, RectF rect)
        {
            if (fn.Arguments.Count == 0) return null;

            // Simplified: parse color stops, use center of rect as center
            var stops = ParseColorStops(fn.Arguments, 0);
            if (stops == null || stops.Length < 2) return null;

            float cx = rect.X + rect.Width * 0.5f;
            float cy = rect.Y + rect.Height * 0.5f;
            float rx = rect.Width * 0.5f;
            float ry = rect.Height * 0.5f;

            return new GradientInfo(GradientType.Radial, stops)
            {
                Center = new Core.Values.PointF(cx, cy),
                RadiusX = rx,
                RadiusY = ry
            };
        }

        private static GradientInfo? ParseConicGradient(CssFunctionValue fn, RectF rect)
        {
            if (fn.Arguments.Count == 0) return null;

            float fromAngle = 0; // default: 0deg (start at top)
            float centerX = 0.5f; // default: center
            float centerY = 0.5f;
            int colorStartIdx = 0;

            // Parse optional "from <angle>" and "at <position>" before color stops
            for (int i = 0; i < fn.Arguments.Count; i++)
            {
                var arg = fn.Arguments[i];
                if (arg is CssKeywordValue kw)
                {
                    if (kw.Keyword == "from")
                    {
                        // Next arg should be the angle
                        if (i + 1 < fn.Arguments.Count && fn.Arguments[i + 1] is CssDimensionValue angleDim && angleDim.Unit == "deg")
                        {
                            fromAngle = angleDim.Value;
                            i++;
                            colorStartIdx = i + 1;
                        }
                    }
                    else if (kw.Keyword == "at")
                    {
                        // Parse center position: "at <x> <y>" or "at center"
                        int posCount = 0;
                        for (int j = i + 1; j < fn.Arguments.Count && posCount < 2; j++)
                        {
                            var posArg = fn.Arguments[j];
                            if (posArg is CssPercentageValue pctPos)
                            {
                                if (posCount == 0) centerX = pctPos.Value / 100f;
                                else centerY = pctPos.Value / 100f;
                                posCount++;
                                colorStartIdx = j + 1;
                            }
                            else if (posArg is CssDimensionValue dimPos)
                            {
                                float px = ResolveLengthValue(dimPos);
                                if (posCount == 0) centerX = rect.Width > 0 ? px / rect.Width : 0.5f;
                                else centerY = rect.Height > 0 ? px / rect.Height : 0.5f;
                                posCount++;
                                colorStartIdx = j + 1;
                            }
                            else if (posArg is CssKeywordValue posKw)
                            {
                                switch (posKw.Keyword)
                                {
                                    case "left": centerX = 0; posCount++; colorStartIdx = j + 1; break;
                                    case "right": centerX = 1; posCount++; colorStartIdx = j + 1; break;
                                    case "top": centerY = 0; posCount++; colorStartIdx = j + 1; break;
                                    case "bottom": centerY = 1; posCount++; colorStartIdx = j + 1; break;
                                    case "center":
                                        if (posCount == 0) centerX = 0.5f;
                                        else centerY = 0.5f;
                                        posCount++;
                                        colorStartIdx = j + 1;
                                        break;
                                    default: goto donePos;
                                }
                            }
                            else break;
                        }
                        donePos:
                        i = colorStartIdx - 1;
                    }
                    else
                    {
                        // Not a gradient keyword — start of color stops
                        break;
                    }
                }
                else if (arg is CssColorValue || arg is CssFunctionValue)
                {
                    // Color value — start of color stops
                    colorStartIdx = i;
                    break;
                }
            }

            var stops = ParseColorStops(fn.Arguments, colorStartIdx);
            if (stops == null || stops.Length < 2) return null;

            return new GradientInfo(GradientType.Conic, stops)
            {
                Angle = fromAngle,
                Center = new Core.Values.PointF(centerX, centerY)
            };
        }

        private static float DirectionToAngle(string direction)
        {
            switch (direction)
            {
                case "top": return 0;
                case "right": return 90;
                case "bottom": return 180;
                case "left": return 270;
                case "top right": return 45;
                case "right top": return 45;
                case "bottom right": return 135;
                case "right bottom": return 135;
                case "bottom left": return 225;
                case "left bottom": return 225;
                case "top left": return 315;
                case "left top": return 315;
                default: return 180;
            }
        }

        private static GradientStop[]? ParseColorStops(System.Collections.Generic.IReadOnlyList<CssValue> args, int startIdx)
        {
            var stops = new System.Collections.Generic.List<GradientStop>();

            for (int i = startIdx; i < args.Count; i++)
            {
                CssColor? color = null;
                float position = -1;

                var val = args[i];
                if (val is CssColorValue cv)
                {
                    color = cv.Color;
                }
                else if (val is CssKeywordValue kw)
                {
                    // Try parsing as a named color
                    if (CssColorParser.TryParseNamed(kw.Keyword, out var parsed))
                        color = parsed;
                    else
                        continue; // skip non-color keywords
                }
                else if (val is CssFunctionValue colorFn)
                {
                    if (TryParseColorFunction(colorFn, out var parsedColor))
                        color = parsedColor;
                    else
                        continue;
                }
                else continue;

                // Check if next argument is a position
                if (i + 1 < args.Count)
                {
                    var next = args[i + 1];
                    if (next is CssPercentageValue pct)
                    {
                        position = pct.Value / 100f;
                        i++;
                    }
                    else if (next is CssDimensionValue posDim)
                    {
                        position = posDim.Value / 100f; // approximate
                        i++;
                    }
                }

                if (color.HasValue)
                    stops.Add(new GradientStop(color.Value, position));
            }

            if (stops.Count < 2) return null;

            // Distribute positions for stops without explicit positions
            DistributeStopPositions(stops);

            return stops.ToArray();
        }

        private static void DistributeStopPositions(System.Collections.Generic.List<GradientStop> stops)
        {
            // First stop defaults to 0, last to 1
            if (stops[0].Position < 0)
                stops[0] = new GradientStop(stops[0].Color, 0f);
            if (stops[stops.Count - 1].Position < 0)
                stops[stops.Count - 1] = new GradientStop(stops[stops.Count - 1].Color, 1f);

            // Distribute remaining stops evenly between known positions
            int i = 0;
            while (i < stops.Count)
            {
                if (stops[i].Position < 0)
                {
                    // Find the next stop with a position
                    int start = i - 1;
                    int end = i;
                    while (end < stops.Count && stops[end].Position < 0) end++;
                    if (end >= stops.Count) end = stops.Count - 1;

                    float startPos = stops[start].Position;
                    float endPos = stops[end].Position;
                    int count = end - start;

                    for (int j = start + 1; j < end; j++)
                    {
                        float t = (float)(j - start) / count;
                        stops[j] = new GradientStop(stops[j].Color, startPos + (endPos - startPos) * t);
                    }
                    i = end;
                }
                else
                {
                    i++;
                }
            }
        }

        private static bool TryParseColorFunction(CssFunctionValue fn, out CssColor color)
        {
            color = default;
            var args = new System.Collections.Generic.List<CssValue>(fn.Arguments.Count);
            for (int i = 0; i < fn.Arguments.Count; i++)
                args.Add(fn.Arguments[i]);

            switch (fn.Name)
            {
                case "rgb":
                case "rgba":
                    return CssColorParser.TryParseRgb(args, out color);
                case "hsl":
                case "hsla":
                    return CssColorParser.TryParseHsl(args, out color);
                default:
                    return false;
            }
        }

        private static float ResolveLengthValue(CssDimensionValue dim)
        {
            switch (dim.Unit)
            {
                case "px": return dim.Value;
                case "pt": return dim.Value * 96f / 72f;
                case "in": return dim.Value * 96f;
                case "cm": return dim.Value * 96f / 2.54f;
                case "mm": return dim.Value * 96f / 25.4f;
                default: return dim.Value;
            }
        }
    }

    /// <summary>
    /// Background-repeat keywords.
    /// </summary>
    internal enum CssBackgroundRepeat
    {
        Repeat = 0,
        NoRepeat = 1,
        RepeatX = 2,
        RepeatY = 3,
    }
}
