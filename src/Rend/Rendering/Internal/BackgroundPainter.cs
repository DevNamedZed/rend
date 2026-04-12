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
        [ThreadStatic] private static CssColor? _currentColor;

        /// <summary>
        /// Paints the background for the given box onto the render target.
        /// Per CSS Backgrounds L3 §3, layers are painted back-to-front: last layer first, first layer on top.
        /// </summary>
        public static void Paint(LayoutBox box, IRenderTarget target,
            ImageResolverDelegate? imageResolver = null)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
            {
                return;
            }

            // Border-radius for rounded backgrounds.
            var radii = BorderRadiusResolver.Resolve(style, box.BorderRect);
            bool hasRadius = radii.HasRadius;

            // Determine how many background layers we have.
            object? bgImageRef = style.GetRefValue(PropertyId.BackgroundImage);
            int layerCount = 1;
            CssListValue? imageList = null;
            if (bgImageRef is CssListValue imgList && imgList.Separator == ',')
            {
                layerCount = imgList.Values.Count;
                imageList = imgList;
            }

            // Get multi-layer longhand refs for repeat/position/size/clip/origin
            object? repeatRef = style.GetRefValue(PropertyId.BackgroundRepeat);
            object? positionRef = style.GetRefValue(PropertyId.BackgroundPosition);
            object? sizeRef = style.GetRefValue(PropertyId.BackgroundSize);
            object? clipRef = style.GetRefValue(PropertyId.BackgroundClip);
            object? originRef = style.GetRefValue(PropertyId.BackgroundOrigin);

            // 1. Paint background-color first (below all layers), using final layer's clip area.
            CssBackgroundClip finalClip = GetLayerClipEnum(clipRef, layerCount - 1, style.BackgroundClip);
            RectF colorClipRect = ResolveBoxRect(box, finalClip);
            CssColor bgColor = style.BackgroundColor;
            if (bgColor.A > 0)
            {
                BrushInfo brush = BrushInfo.Solid(bgColor);
                if (hasRadius)
                {
                    var path = new PathData();
                    radii.AddToPath(path, colorClipRect);
                    target.FillPath(path, brush);
                }
                else
                {
                    target.FillRect(colorClipRect.PixelSnap(), brush);
                }
            }

            // 2. Paint background-image layers back-to-front (last layer painted first).
            for (int layerIdx = layerCount - 1; layerIdx >= 0; layerIdx--)
            {
                object? layerImage = imageList != null ? imageList.Values[layerIdx] : bgImageRef;
                CssBackgroundClip layerClipEnum = GetLayerClipEnum(clipRef, layerIdx, style.BackgroundClip);
                CssBackgroundOrigin layerOriginEnum = GetLayerOriginEnum(originRef, layerIdx, style.BackgroundOrigin);
                RectF clipRect = ResolveBoxRect(box, layerClipEnum);
                RectF originRect = ResolveBoxRect(box, (CssBackgroundClip)(int)layerOriginEnum);

                PaintBackgroundLayer(layerImage, layerIdx, repeatRef, positionRef, sizeRef,
                    style, clipRect, originRect, radii, hasRadius, target, imageResolver);
            }
        }

        /// <summary>
        /// [CSS2 §14.2] Paints background-image layers from a style onto the full canvas rect.
        /// Used when propagating the body element's background to the canvas.
        /// </summary>
        internal static void PaintCanvasBackgroundImage(ComputedStyle style, RectF canvasRect,
            IRenderTarget target, ImageResolverDelegate? imageResolver, RectF? originRect = null)
        {
            object? bgImageRef = style.GetRefValue(PropertyId.BackgroundImage);
            if (bgImageRef == null)
            {
                return;
            }

            int layerCount = 1;
            CssListValue? imageList = null;
            if (bgImageRef is CssListValue imgList && imgList.Separator == ',')
            {
                layerCount = imgList.Values.Count;
                imageList = imgList;
            }

            object? repeatRef = style.GetRefValue(PropertyId.BackgroundRepeat);
            object? positionRef = style.GetRefValue(PropertyId.BackgroundPosition);
            object? sizeRef = style.GetRefValue(PropertyId.BackgroundSize);
            var emptyRadii = new BorderRadii();
            RectF positioningArea = originRect ?? canvasRect;

            for (int layerIdx = layerCount - 1; layerIdx >= 0; layerIdx--)
            {
                object? layerImage = imageList != null ? imageList.Values[layerIdx] : bgImageRef;

                // [CSS-BACKGROUNDS §2.11] For canvas background gradients, the gradient
                // must be sized to the positioning area and tiled to fill the canvas.
                bool isCanvasGradient = layerImage is CssFunctionValue
                    && positioningArea.Width > 0 && positioningArea.Height > 0
                    && (Math.Abs(canvasRect.X - positioningArea.X) > 1f
                        || Math.Abs(canvasRect.Y - positioningArea.Y) > 1f
                        || Math.Abs(canvasRect.Width - positioningArea.Width) > 1f
                        || Math.Abs(canvasRect.Height - positioningArea.Height) > 1f);
                if (isCanvasGradient && layerImage is CssFunctionValue canvasGradientFn)
                {
                    var gradient = ParseCssGradient(canvasGradientFn, positioningArea, style.Color);
                    if (gradient != null)
                    {
                        target.FillRectWithTiledGradient(gradient, canvasRect, positioningArea);
                        continue;
                    }
                }

                PaintBackgroundLayer(layerImage, layerIdx, repeatRef, positionRef, sizeRef,
                    style, canvasRect, positioningArea, emptyRadii, false, target, imageResolver);
            }
        }

        /// <summary>
        /// Gets the background-clip enum for a specific layer index.
        /// </summary>
        private static CssBackgroundClip GetLayerClipEnum(object? clipRef, int layerIdx, CssBackgroundClip fallback)
        {
            if (clipRef is CssListValue clipList && clipList.Separator == ',')
            {
                if (layerIdx < clipList.Values.Count)
                {
                    return ParseClipKeyword(clipList.Values[layerIdx], fallback);
                }
                // CSS spec: cycle through available values
                if (clipList.Values.Count > 0)
                {
                    return ParseClipKeyword(clipList.Values[layerIdx % clipList.Values.Count], fallback);
                }
            }
            return fallback;
        }

        /// <summary>
        /// Gets the background-origin enum for a specific layer index.
        /// </summary>
        private static CssBackgroundOrigin GetLayerOriginEnum(object? originRef, int layerIdx, CssBackgroundOrigin fallback)
        {
            if (originRef is CssListValue originList && originList.Separator == ',')
            {
                if (layerIdx < originList.Values.Count)
                {
                    return ParseOriginKeyword(originList.Values[layerIdx], fallback);
                }
                if (originList.Values.Count > 0)
                {
                    return ParseOriginKeyword(originList.Values[layerIdx % originList.Values.Count], fallback);
                }
            }
            return fallback;
        }

        private static CssBackgroundClip ParseClipKeyword(CssValue val, CssBackgroundClip fallback)
        {
            if (val is CssKeywordValue kw)
            {
                switch (kw.Keyword)
                {
                    case "border-box": return CssBackgroundClip.BorderBox;
                    case "padding-box": return CssBackgroundClip.PaddingBox;
                    case "content-box": return CssBackgroundClip.ContentBox;
                }
            }
            return fallback;
        }

        private static CssBackgroundOrigin ParseOriginKeyword(CssValue val, CssBackgroundOrigin fallback)
        {
            if (val is CssKeywordValue kw)
            {
                switch (kw.Keyword)
                {
                    case "border-box": return CssBackgroundOrigin.BorderBox;
                    case "padding-box": return CssBackgroundOrigin.PaddingBox;
                    case "content-box": return CssBackgroundOrigin.ContentBox;
                }
            }
            return fallback;
        }

        /// <summary>
        /// Paints a single background-image layer.
        /// </summary>
        private static void PaintBackgroundLayer(object? layerImage, int layerIdx,
            object? repeatRef, object? positionRef, object? sizeRef,
            ComputedStyle style, RectF clipRect, RectF originRect,
            BorderRadii radii, bool hasRadius, IRenderTarget target,
            ImageResolverDelegate? imageResolver)
        {
            // Check for CSS gradient functions
            if (layerImage is CssFunctionValue gradientFn)
            {
                var gradient = ParseCssGradient(gradientFn, clipRect, style.Color);
                if (gradient != null)
                {

                    BrushInfo gradBrush = BrushInfo.FromGradient(gradient);
                    if (hasRadius)
                    {
                        var path = new PathData();
                        radii.AddToPath(path, clipRect);
                        target.FillPath(path, gradBrush);
                    }
                    else
                    {
                        target.FillRect(clipRect.PixelSnap(), gradBrush);
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
            if (layerImage is CssUrlValue urlVal)
            {
                bgImageUrl = urlVal.Url;
            }
            else if (layerImage is CssKeywordValue kwRef && kwRef.Keyword != "none")
            {
                bgImageUrl = kwRef.Keyword;
            }
            else if (layerImage is string strRef)
            {
                bgImageUrl = strRef;
            }

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
            object? layerSize = GetLayerRef(sizeRef, layerIdx);
            ComputeBackgroundSizeFromRef(layerSize, originRect, imgW, imgH,
                out float scaledW, out float scaledH);

            // Calculate image position (relative to background-origin area).
            object? layerPosition = GetLayerRef(positionRef, layerIdx);
            ComputeBackgroundPositionFromRef(layerPosition, originRect, scaledW, scaledH,
                out float posX, out float posY);

            // Get repeat mode for this layer.
            int repeatMode = GetLayerRepeatMode(repeatRef, layerIdx, style);

            // Clip to background-clip rect for tiled backgrounds.
            bool needsClip = repeatMode != (int)CssBackgroundRepeat.NoRepeat;
            if (needsClip)
            {
                if (hasRadius)
                {
                    var clipPath = new PathData();
                    radii.AddToPath(clipPath, clipRect);
                    target.PushClipPath(clipPath);
                }
                else
                {
                    target.PushClipRect(clipRect);
                }
            }

            // [CSS-BACKGROUNDS-3 §3.4] round: scale image so integer tiles fill the area.
            // space: distribute extra space evenly between unscaled tiles.
            bool isRound = repeatMode == (int)CssBackgroundRepeat.Round;
            bool isSpace = repeatMode == (int)CssBackgroundRepeat.Space;
            float spaceGapX = 0, spaceGapY = 0;
            if (isRound && scaledW > 0 && scaledH > 0)
            {
                float areaW = clipRect.Width;
                float areaH = clipRect.Height;
                int tilesX = Math.Max(1, (int)Math.Round(areaW / scaledW));
                int tilesY = Math.Max(1, (int)Math.Round(areaH / scaledH));
                scaledW = areaW / tilesX;
                scaledH = areaH / tilesY;
            }
            else if (isSpace && scaledW > 0 && scaledH > 0)
            {
                float areaW = clipRect.Width;
                float areaH = clipRect.Height;
                int tilesX = Math.Max(1, (int)(areaW / scaledW));
                int tilesY = Math.Max(1, (int)(areaH / scaledH));
                if (tilesX > 1)
                {
                    spaceGapX = (areaW - tilesX * scaledW) / (tilesX - 1);
                }
                if (tilesY > 1)
                {
                    spaceGapY = (areaH - tilesY * scaledH) / (tilesY - 1);
                }
            }

            // Draw image tile(s).
            bool repeatX = repeatMode == (int)CssBackgroundRepeat.Repeat ||
                           repeatMode == (int)CssBackgroundRepeat.RepeatX ||
                           isRound || isSpace;
            bool repeatY = repeatMode == (int)CssBackgroundRepeat.Repeat ||
                           repeatMode == (int)CssBackgroundRepeat.RepeatY ||
                           isRound || isSpace;

            if (!repeatX && !repeatY)
            {
                // No repeat: single image.
                var destRect = new RectF(posX, posY, scaledW, scaledH);
                target.DrawImage(imageData, destRect);
            }
            else
            {
                // Tile the image using shader-based tiling for seamless boundaries.
                RectF fillArea;
                if (repeatX && repeatY)
                {
                    fillArea = clipRect;
                }
                else if (repeatX)
                {
                    fillArea = new RectF(clipRect.X, posY, clipRect.Width, scaledH);
                }
                else
                {
                    fillArea = new RectF(posX, clipRect.Y, scaledW, clipRect.Height);
                }

                target.DrawTiledImage(imageData, fillArea, scaledW, scaledH, posX, posY);
            }

            if (needsClip)
            {
                target.PopClip();
            }
        }

        /// <summary>
        /// Gets the ref value for a specific layer from a potentially comma-separated list.
        /// </summary>
        private static object? GetLayerRef(object? refValue, int layerIdx)
        {
            if (refValue is CssListValue list && list.Separator == ',')
            {
                if (layerIdx < list.Values.Count)
                {
                    return list.Values[layerIdx];
                }
                // CSS spec: cycle through available values
                if (list.Values.Count > 0)
                {
                    return list.Values[layerIdx % list.Values.Count];
                }
            }
            return refValue;
        }

        /// <summary>
        /// Gets the repeat mode for a specific layer.
        /// </summary>
        private static int GetLayerRepeatMode(object? repeatRef, int layerIdx, ComputedStyle style)
        {
            if (repeatRef is CssListValue repeatList && repeatList.Separator == ',')
            {
                int idx = layerIdx < repeatList.Values.Count ? layerIdx : layerIdx % repeatList.Values.Count;
                if (idx < repeatList.Values.Count)
                {
                    var val = repeatList.Values[idx];
                    if (val is CssKeywordValue kw)
                    {
                        switch (kw.Keyword)
                        {
                            case "repeat": return (int)CssBackgroundRepeat.Repeat;
                            case "no-repeat": return (int)CssBackgroundRepeat.NoRepeat;
                            case "repeat-x": return (int)CssBackgroundRepeat.RepeatX;
                            case "repeat-y": return (int)CssBackgroundRepeat.RepeatY;
                            case "round": return (int)CssBackgroundRepeat.Round;
                            case "space": return (int)CssBackgroundRepeat.Space;
                        }
                    }
                }
            }
            return style.GetRawValue(PropertyId.BackgroundRepeat).IntValue;
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
        private static void ComputeBackgroundSizeFromRef(object? sizeRef, RectF paddingRect,
            float imgW, float imgH, out float scaledW, out float scaledH)
        {
            // Default: auto (intrinsic size)
            scaledW = imgW;
            scaledH = imgH;

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
        private static void ComputeBackgroundPositionFromRef(object? posRef, RectF paddingRect,
            float scaledW, float scaledH, out float posX, out float posY)
        {
            // Default: 0% 0% (top-left)
            posX = paddingRect.X;
            posY = paddingRect.Y;

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
        internal static GradientInfo? ParseCssGradient(CssFunctionValue fn, RectF rect, CssColor? currentColor = null)
        {
            // Store currentColor for ParseColorStops to use
            _currentColor = currentColor;

            if (fn.Name == "linear-gradient" || fn.Name == "-webkit-linear-gradient")
                return ParseLinearGradient(fn, rect);
            if (fn.Name == "repeating-linear-gradient")
            {
                var g = ParseLinearGradient(fn, rect);
                if (g != null) g.Repeating = true;
                return g;
            }
            if (fn.Name == "radial-gradient" || fn.Name == "-webkit-radial-gradient")
                return ParseRadialGradient(fn, rect);
            if (fn.Name == "repeating-radial-gradient")
            {
                var g = ParseRadialGradient(fn, rect);
                if (g != null) g.Repeating = true;
                return g;
            }
            if (fn.Name == "conic-gradient")
                return ParseConicGradient(fn, rect);
            if (fn.Name == "repeating-conic-gradient")
            {
                var g = ParseConicGradient(fn, rect);
                if (g != null) g.Repeating = true;
                return g;
            }
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
                    // [CSS-IMAGES4 §3.1] "to" followed by direction keywords; stop at "in" (color interpolation)
                    string direction = "";
                    for (int i = 1; i < fn.Arguments.Count; i++)
                    {
                        if (fn.Arguments[i] is CssKeywordValue kw2 && kw2.Keyword != "in")
                        {
                            direction += kw2.Keyword + " ";
                            colorStartIdx = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    angle = DirectionToAngle(direction.Trim(), rect.Width, rect.Height);
                }
            }

            // [CSS-IMAGES4 §3] Parse optional color interpolation method
            TryParseColorInterpolationMethod(fn.Arguments, ref colorStartIdx,
                out string? colorSpace, out string? hueMethod);

            // Compute gradient line length for resolving px stop positions
            float angleRad = angle * (float)(Math.PI / 180.0);
            float gradientLineLength = Math.Abs(rect.Width * (float)Math.Sin(angleRad))
                                     + Math.Abs(rect.Height * (float)Math.Cos(angleRad));
            if (gradientLineLength <= 0) gradientLineLength = 1;

            var stops = ParseColorStops(fn.Arguments, colorStartIdx, gradientLineLength, _currentColor);
            if (stops == null || stops.Length < 2) return null;

            return new GradientInfo(GradientType.Linear, stops)
            {
                Angle = angle,
                ColorInterpolationSpace = colorSpace,
                HueInterpolationMethod = hueMethod
            };
        }

        private static GradientInfo? ParseRadialGradient(CssFunctionValue fn, RectF rect)
        {
            if (fn.Arguments.Count == 0) return null;

            // Parse shape/size/position descriptor before color stops
            bool isCircle = false;
            float centerX = 0.5f; // fractional center (0-1)
            float centerY = 0.5f;
            int colorStartIdx = 0;
            // 0 = farthest-corner (default), 1 = closest-side, 2 = farthest-side, 3 = closest-corner
            int sizeKeyword = 0;

            for (int i = 0; i < fn.Arguments.Count; i++)
            {
                var arg = fn.Arguments[i];
                if (arg is CssKeywordValue kw)
                {
                    string k = kw.Keyword;
                    if (k == "circle") { isCircle = true; colorStartIdx = i + 1; }
                    else if (k == "ellipse") { colorStartIdx = i + 1; }
                    else if (k == "closest-side") { sizeKeyword = 1; colorStartIdx = i + 1; }
                    else if (k == "farthest-side") { sizeKeyword = 2; colorStartIdx = i + 1; }
                    else if (k == "closest-corner") { sizeKeyword = 3; colorStartIdx = i + 1; }
                    else if (k == "farthest-corner") { sizeKeyword = 0; colorStartIdx = i + 1; }
                    else if (k == "at")
                    {
                        // Parse position
                        int posCount = 0;
                        for (int j = i + 1; j < fn.Arguments.Count && posCount < 2; j++)
                        {
                            var posArg = fn.Arguments[j];
                            if (posArg is CssPercentageValue pctPos)
                            {
                                if (posCount == 0) centerX = pctPos.Value / 100f;
                                else centerY = pctPos.Value / 100f;
                                posCount++; colorStartIdx = j + 1;
                            }
                            else if (posArg is CssDimensionValue dimPos)
                            {
                                float px = ResolveLengthValue(dimPos);
                                if (posCount == 0) centerX = rect.Width > 0 ? px / rect.Width : 0.5f;
                                else centerY = rect.Height > 0 ? px / rect.Height : 0.5f;
                                posCount++; colorStartIdx = j + 1;
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
                                        posCount++; colorStartIdx = j + 1; break;
                                    default: goto donePos;
                                }
                            }
                            else break;
                        }
                        donePos:
                        // Ensure i advances past "at" + any parsed positions.
                        // If no positions were found, skip "at" itself to avoid infinite loop.
                        if (colorStartIdx <= i)
                        {
                            colorStartIdx = i + 1;
                        }
                        i = colorStartIdx - 1;
                    }
                    else
                    {
                        // Not a gradient descriptor keyword — start of color stops
                        break;
                    }
                }
                else
                {
                    // Not a keyword — start of color stops
                    colorStartIdx = i;
                    break;
                }
            }

            // Compute radii based on shape and size keyword BEFORE parsing stops,
            // since px stop positions need to be normalized against the gradient radius.
            float absCx = centerX * rect.Width;
            float absCy = centerY * rect.Height;

            // Distances from center to each side.
            float dLeft = absCx;
            float dRight = rect.Width - absCx;
            float dTop = absCy;
            float dBottom = rect.Height - absCy;

            float rx, ry;
            float gradientRadius; // absolute pixel radius for stop normalization
            if (isCircle)
            {
                float r;
                switch (sizeKeyword)
                {
                    case 1: // closest-side
                        r = Math.Min(Math.Min(dLeft, dRight), Math.Min(dTop, dBottom));
                        break;
                    case 2: // farthest-side
                        r = Math.Max(Math.Max(dLeft, dRight), Math.Max(dTop, dBottom));
                        break;
                    case 3: // closest-corner
                    {
                        float cLeft = Math.Min(dLeft, dRight);
                        float cTop = Math.Min(dTop, dBottom);
                        r = (float)Math.Sqrt(cLeft * cLeft + cTop * cTop);
                        break;
                    }
                    default: // 0: farthest-corner (CSS default)
                    {
                        float fLeft = Math.Max(dLeft, dRight);
                        float fTop = Math.Max(dTop, dBottom);
                        r = (float)Math.Sqrt(fLeft * fLeft + fTop * fTop);
                        break;
                    }
                }
                rx = rect.Width > 0 ? r / rect.Width : 0.5f;
                ry = rect.Height > 0 ? r / rect.Height : 0.5f;
                gradientRadius = r;
            }
            else
            {
                // Ellipse: radii are proportional to box dimensions.
                float erxAbs, eryAbs;
                switch (sizeKeyword)
                {
                    case 1: // closest-side
                        erxAbs = Math.Min(dLeft, dRight);
                        eryAbs = Math.Min(dTop, dBottom);
                        break;
                    case 2: // farthest-side
                        erxAbs = Math.Max(dLeft, dRight);
                        eryAbs = Math.Max(dTop, dBottom);
                        break;
                    case 3: // closest-corner
                    {
                        float cDx = Math.Min(dLeft, dRight);
                        float cDy = Math.Min(dTop, dBottom);
                        float ratio = rect.Width > 0 && rect.Height > 0 ? rect.Width / rect.Height : 1f;
                        eryAbs = (float)Math.Sqrt(cDx * cDx / (ratio * ratio) + cDy * cDy);
                        erxAbs = eryAbs * ratio;
                        break;
                    }
                    default: // 0: farthest-corner (CSS default)
                    {
                        float fDx = Math.Max(dLeft, dRight);
                        float fDy = Math.Max(dTop, dBottom);
                        float ratio2 = rect.Width > 0 && rect.Height > 0 ? rect.Width / rect.Height : 1f;
                        float ery2 = (float)Math.Sqrt(fDx * fDx / (ratio2 * ratio2) + fDy * fDy);
                        erxAbs = ery2 * ratio2;
                        eryAbs = ery2;
                        break;
                    }
                }
                rx = rect.Width > 0 ? erxAbs / rect.Width : 0.5f;
                ry = rect.Height > 0 ? eryAbs / rect.Height : 0.5f;
                // For elliptical gradients, Skia uses the larger radius and scales.
                // Stop positions normalize against the larger radius.
                gradientRadius = Math.Max(erxAbs, eryAbs);
            }

            // [CSS-IMAGES4 §3] Parse optional color interpolation method
            TryParseColorInterpolationMethod(fn.Arguments, ref colorStartIdx,
                out string? colorSpace, out string? hueMethod);

            var stops = ParseColorStops(fn.Arguments, colorStartIdx, gradientRadius, _currentColor);
            if (stops == null || stops.Length < 2) return null;

            return new GradientInfo(GradientType.Radial, stops)
            {
                Center = new Core.Values.PointF(centerX, centerY),
                RadiusX = rx,
                RadiusY = ry,
                ColorInterpolationSpace = colorSpace,
                HueInterpolationMethod = hueMethod
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
                        if (colorStartIdx <= i)
                        {
                            colorStartIdx = i + 1;
                        }
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

            // [CSS-IMAGES4 §3] Parse optional color interpolation method
            TryParseColorInterpolationMethod(fn.Arguments, ref colorStartIdx,
                out string? colorSpace, out string? hueMethod);

            var stops = ParseColorStops(fn.Arguments, colorStartIdx, 0, _currentColor, isConic: true);
            if (stops == null || stops.Length < 2) return null;

            return new GradientInfo(GradientType.Conic, stops)
            {
                Angle = fromAngle,
                Center = new Core.Values.PointF(centerX, centerY),
                ColorInterpolationSpace = colorSpace,
                HueInterpolationMethod = hueMethod
            };
        }

        private static float DirectionToAngle(string direction, float boxWidth, float boxHeight)
        {
            switch (direction)
            {
                case "top": return 0;
                case "right": return 90;
                case "bottom": return 180;
                case "left": return 270;
                // Corner directions: CSS spec requires the gradient line angle to depend
                // on the box dimensions so that the gradient line endpoints touch the corners.
                // angle = atan2(dx, -dy) where (dx, dy) is the direction toward the target corner.
                case "top right":
                case "right top":
                    return (float)(Math.Atan2(boxWidth, boxHeight) * (180.0 / Math.PI));
                case "bottom right":
                case "right bottom":
                    return (float)(Math.Atan2(boxWidth, -boxHeight) * (180.0 / Math.PI));
                case "bottom left":
                case "left bottom":
                {
                    float a = (float)(Math.Atan2(-boxWidth, -boxHeight) * (180.0 / Math.PI));
                    return a < 0 ? a + 360f : a;
                }
                case "top left":
                case "left top":
                {
                    float a = (float)(Math.Atan2(-boxWidth, boxHeight) * (180.0 / Math.PI));
                    return a < 0 ? a + 360f : a;
                }
                default: return 180;
            }
        }

        /// <summary>
        /// [CSS-IMAGES4 §3] Parses "in &lt;colorspace&gt; [longer|shorter|increasing|decreasing] hue".
        /// </summary>
        private static void TryParseColorInterpolationMethod(
            System.Collections.Generic.IReadOnlyList<CssValue> args, ref int index,
            out string? colorSpace, out string? hueMethod)
        {
            colorSpace = null;
            hueMethod = null;

            if (index >= args.Count)
            {
                return;
            }

            if (!(args[index] is CssKeywordValue inKw && inKw.Keyword == "in"))
            {
                return;
            }

            index++;
            if (index >= args.Count || !(args[index] is CssKeywordValue spaceKw))
            {
                return;
            }

            colorSpace = spaceKw.Keyword;
            index++;

            if (index >= args.Count || !(args[index] is CssKeywordValue methodKw))
            {
                return;
            }

            string method = methodKw.Keyword;
            if (method == "shorter" || method == "longer"
                || method == "increasing" || method == "decreasing")
            {
                hueMethod = method;
                index++;

                if (index < args.Count && args[index] is CssKeywordValue hueKw
                    && hueKw.Keyword == "hue")
                {
                    index++;
                }
            }
        }

        private static GradientStop[]? ParseColorStops(System.Collections.Generic.IReadOnlyList<CssValue> args, int startIdx, float gradientLineLength = 0, CssColor? currentColor = null, bool isConic = false)
        {
            var stops = new System.Collections.Generic.List<GradientStop>();

            for (int i = startIdx; i < args.Count; i++)
            {
                CssColor? color = null;
                float position = float.NaN;

                var val = args[i];
                if (val is CssColorValue cv)
                {
                    color = cv.Color;
                }
                else if (val is CssKeywordValue kw)
                {
                    if (kw.Keyword == "currentcolor" || kw.Keyword == "currentColor")
                    {
                        color = currentColor ?? new CssColor(0, 0, 0, 255);
                    }
                    else if (CssColorParser.TryParseNamed(kw.Keyword, out var parsed))
                    {
                        color = parsed;
                    }
                    else
                    {
                        continue;
                    }
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
                float position2 = float.NaN;
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
                        if (isConic && IsAngleUnit(posDim.Unit))
                        {
                            // [CSS-IMAGES4 §3.4] Conic stops accept angle units
                            position = ResolveAngleFraction(posDim);
                        }
                        else
                        {
                            // Convert px position to fraction of gradient line length
                            float px = ResolveLengthValue(posDim);
                            position = gradientLineLength > 0 ? px / gradientLineLength : px / 100f;
                        }
                        i++;
                    }
                    else if (next is CssNumberValue numPos && numPos.Value == 0)
                    {
                        // [CSS-IMAGES4 §3] Bare 0 is valid as 0% in gradient stops
                        position = 0f;
                        i++;
                    }

                    // CSS double-position syntax: "color pos1 pos2" → two stops
                    if (!float.IsNaN(position) && i + 1 < args.Count)
                    {
                        var next2 = args[i + 1];
                        if (next2 is CssPercentageValue pct2)
                        {
                            position2 = pct2.Value / 100f;
                            i++;
                        }
                        else if (next2 is CssDimensionValue posDim2)
                        {
                            if (isConic && IsAngleUnit(posDim2.Unit))
                            {
                                position2 = ResolveAngleFraction(posDim2);
                            }
                            else
                            {
                                float px2 = ResolveLengthValue(posDim2);
                                position2 = gradientLineLength > 0 ? px2 / gradientLineLength : px2 / 100f;
                            }
                            i++;
                        }
                        else if (next2 is CssNumberValue numPos2 && numPos2.Value == 0)
                        {
                            position2 = 0f;
                            i++;
                        }
                    }
                }

                if (color.HasValue)
                {
                    stops.Add(new GradientStop(color.Value, position));
                    // Double-position: add a second stop at the end position
                    if (!float.IsNaN(position2))
                    {
                        stops.Add(new GradientStop(color.Value, position2));
                    }
                }
            }

            if (stops.Count < 2) return null;

            // Distribute positions for stops without explicit positions
            DistributeStopPositions(stops);

            return stops.ToArray();
        }

        private static void DistributeStopPositions(System.Collections.Generic.List<GradientStop> stops)
        {
            // First stop defaults to 0, last to 1
            if (float.IsNaN(stops[0].Position))
            {
                stops[0] = new GradientStop(stops[0].Color, 0f);
            }
            if (float.IsNaN(stops[stops.Count - 1].Position))
            {
                stops[stops.Count - 1] = new GradientStop(stops[stops.Count - 1].Color, 1f);
            }

            // Distribute remaining stops evenly between known positions
            int i = 0;
            while (i < stops.Count)
            {
                if (float.IsNaN(stops[i].Position))
                {
                    // Find the next stop with a position
                    int start = i - 1;
                    int end = i;
                    while (end < stops.Count && float.IsNaN(stops[end].Position)) end++;
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

            // [CSS-IMAGES3 §3.4.3] Fixup: each position must be >= previous position
            for (int j = 1; j < stops.Count; j++)
            {
                if (stops[j].Position < stops[j - 1].Position)
                {
                    stops[j] = new GradientStop(stops[j].Color, stops[j - 1].Position);
                }
            }
        }

        private static bool IsAngleUnit(string unit)
        {
            return unit == "deg" || unit == "grad" || unit == "rad" || unit == "turn";
        }

        /// <summary>
        /// Convert an angle dimension to a 0-1 fraction of a full circle.
        /// </summary>
        private static float ResolveAngleFraction(CssDimensionValue dim)
        {
            switch (dim.Unit)
            {
                case "deg": return dim.Value / 360f;
                case "grad": return dim.Value / 400f;
                case "rad": return dim.Value / (2f * (float)Math.PI);
                case "turn": return dim.Value;
                default: return dim.Value / 360f;
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
        Round = 4,
        Space = 5,
    }
}
