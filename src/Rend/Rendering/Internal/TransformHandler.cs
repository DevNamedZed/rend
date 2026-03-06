using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Handles CSS transform application by converting style transforms
    /// to a <see cref="Matrix3x2"/> and applying them to the render target.
    /// Supports: translate, translateX, translateY, rotate, scale, scaleX, scaleY,
    /// skew, skewX, skewY, matrix.
    /// </summary>
    internal static class TransformHandler
    {
        /// <summary>
        /// Checks whether the box has a CSS transform and, if so, saves the
        /// render target state and applies the transform matrix.
        /// </summary>
        /// <returns><c>true</c> if a transform was applied and the state needs to be restored.</returns>
        public static bool Apply(LayoutBox box, IRenderTarget target)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
            {
                return false;
            }

            object? rawValue = style.GetRefValue(PropertyId.Transform);
            if (rawValue == null)
            {
                return false;
            }

            var transformValue = rawValue as CssValue;
            if (transformValue == null)
            {
                return false;
            }

            // transform: none
            if (transformValue is CssKeywordValue kw && kw.Keyword == "none")
            {
                return false;
            }

            Matrix3x2 matrix = BuildTransformMatrix(transformValue, box.BorderRect);
            if (matrix == Matrix3x2.Identity)
            {
                return false;
            }

            // Compute transform origin (default: center of border box)
            RectF borderRect = box.BorderRect;
            float originX = borderRect.X + borderRect.Width * 0.5f;
            float originY = borderRect.Y + borderRect.Height * 0.5f;

            // Check for custom transform-origin
            object? originValue = style.GetRefValue(PropertyId.TransformOrigin);
            if (originValue is CssValue originCss)
            {
                ResolveTransformOrigin(originCss, borderRect, out originX, out originY);
            }

            // Build final matrix: translate to origin -> apply transform -> translate back
            Matrix3x2 toOrigin = Matrix3x2.CreateTranslation(-originX, -originY);
            Matrix3x2 fromOrigin = Matrix3x2.CreateTranslation(originX, originY);
            Matrix3x2 finalMatrix = toOrigin * matrix * fromOrigin;

            target.Save();
            target.SetTransform(finalMatrix);
            return true;
        }

        /// <summary>
        /// Restores the render target state if a transform was previously applied.
        /// </summary>
        public static void Restore(IRenderTarget target)
        {
            target.Restore();
        }

        /// <summary>
        /// Builds a Matrix3x2 from a CSS transform value (single function or space-separated list).
        /// </summary>
        internal static Matrix3x2 BuildTransformMatrix(CssValue value, RectF? refBox = null)
        {
            // Collect the transform functions
            var functions = new List<CssFunctionValue>();

            if (value is CssFunctionValue fn)
            {
                functions.Add(fn);
            }
            else if (value is CssListValue list && list.Separator == ' ')
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (list.Values[i] is CssFunctionValue listFn)
                    {
                        functions.Add(listFn);
                    }
                }
            }

            if (functions.Count == 0)
            {
                return Matrix3x2.Identity;
            }

            // Per CSS spec, transforms are applied right-to-left (post-multiply).
            // We iterate left-to-right and multiply: result = fn1 * fn2 * fn3 ...
            Matrix3x2 result = Matrix3x2.Identity;
            for (int i = 0; i < functions.Count; i++)
            {
                Matrix3x2 m = ParseTransformFunction(functions[i], refBox);
                result = result * m;
            }

            return result;
        }

        private static Matrix3x2 ParseTransformFunction(CssFunctionValue fn, RectF? refBox)
        {
            string name = fn.Name.ToLowerInvariant();
            var args = fn.Arguments;

            switch (name)
            {
                case "translate":
                {
                    float tx = args.Count > 0 ? ResolveLengthOrPercent(args[0], refBox?.Width ?? 0) : 0;
                    float ty = args.Count > 1 ? ResolveLengthOrPercent(args[1], refBox?.Height ?? 0) : 0;
                    return Matrix3x2.CreateTranslation(tx, ty);
                }

                case "translatex":
                {
                    float tx = args.Count > 0 ? ResolveLengthOrPercent(args[0], refBox?.Width ?? 0) : 0;
                    return Matrix3x2.CreateTranslation(tx, 0);
                }

                case "translatey":
                {
                    float ty = args.Count > 0 ? ResolveLengthOrPercent(args[0], refBox?.Height ?? 0) : 0;
                    return Matrix3x2.CreateTranslation(0, ty);
                }

                case "scale":
                {
                    float sx = args.Count > 0 ? GetNumber(args[0]) : 1;
                    float sy = args.Count > 1 ? GetNumber(args[1]) : sx;
                    return Matrix3x2.CreateScale(sx, sy);
                }

                case "scalex":
                {
                    float sx = args.Count > 0 ? GetNumber(args[0]) : 1;
                    return Matrix3x2.CreateScale(sx, 1);
                }

                case "scaley":
                {
                    float sy = args.Count > 0 ? GetNumber(args[0]) : 1;
                    return Matrix3x2.CreateScale(1, sy);
                }

                case "rotate":
                {
                    float angle = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    return Matrix3x2.CreateRotation(angle);
                }

                case "skew":
                {
                    float ax = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    float ay = args.Count > 1 ? ResolveAngle(args[1]) : 0;
                    return Matrix3x2.CreateSkew(ax, ay);
                }

                case "skewx":
                {
                    float ax = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    return Matrix3x2.CreateSkew(ax, 0);
                }

                case "skewy":
                {
                    float ay = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    return Matrix3x2.CreateSkew(0, ay);
                }

                case "matrix":
                {
                    if (args.Count >= 6)
                    {
                        float a = GetNumber(args[0]);
                        float b = GetNumber(args[1]);
                        float c = GetNumber(args[2]);
                        float d = GetNumber(args[3]);
                        float e = GetNumber(args[4]);
                        float f = GetNumber(args[5]);
                        return new Matrix3x2(a, b, c, d, e, f);
                    }
                    return Matrix3x2.Identity;
                }

                default:
                    return Matrix3x2.Identity;
            }
        }

        private static void ResolveTransformOrigin(CssValue value, RectF borderRect,
            out float originX, out float originY)
        {
            // Default: center
            originX = borderRect.X + borderRect.Width * 0.5f;
            originY = borderRect.Y + borderRect.Height * 0.5f;

            if (value is CssListValue list && list.Separator == ' ' && list.Values.Count >= 2)
            {
                originX = borderRect.X + ResolveOriginComponent(list.Values[0], borderRect.Width);
                originY = borderRect.Y + ResolveOriginComponent(list.Values[1], borderRect.Height);
            }
            else if (value is CssKeywordValue kw)
            {
                ResolveOriginKeyword(kw.Keyword, borderRect, out originX, out originY);
            }
            else if (value is CssDimensionValue dim)
            {
                originX = borderRect.X + ResolveLengthValue(dim);
                // Y stays at center
            }
            else if (value is CssPercentageValue pct)
            {
                originX = borderRect.X + pct.Value / 100f * borderRect.Width;
                // Y stays at center
            }
        }

        private static float ResolveOriginComponent(CssValue value, float size)
        {
            if (value is CssDimensionValue dim)
            {
                return ResolveLengthValue(dim);
            }
            if (value is CssPercentageValue pct)
            {
                return pct.Value / 100f * size;
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
                        return size * 0.5f;
                    case "right":
                    case "bottom":
                        return size;
                }
            }
            return size * 0.5f; // default: center
        }

        private static void ResolveOriginKeyword(string keyword, RectF borderRect,
            out float originX, out float originY)
        {
            originX = borderRect.X + borderRect.Width * 0.5f;
            originY = borderRect.Y + borderRect.Height * 0.5f;

            switch (keyword)
            {
                case "left":
                    originX = borderRect.X;
                    break;
                case "right":
                    originX = borderRect.X + borderRect.Width;
                    break;
                case "top":
                    originY = borderRect.Y;
                    break;
                case "bottom":
                    originY = borderRect.Y + borderRect.Height;
                    break;
                case "center":
                    // Already center
                    break;
            }
        }

        private static float ResolveLengthOrPercent(CssValue value, float referenceSize)
        {
            if (value is CssDimensionValue dim)
            {
                return ResolveLengthValue(dim);
            }
            if (value is CssNumberValue num)
            {
                return num.Value; // unitless: treat as px
            }
            if (value is CssPercentageValue pct)
            {
                return pct.Value * referenceSize / 100f;
            }
            return 0;
        }

        private static float ResolveLength(CssValue value)
        {
            return ResolveLengthOrPercent(value, 0);
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
                default: return dim.Value; // assume px
            }
        }

        private static float ResolveAngle(CssValue value)
        {
            if (value is CssDimensionValue dim)
            {
                switch (dim.Unit)
                {
                    case "deg": return dim.Value * ((float)Math.PI / 180f);
                    case "rad": return dim.Value;
                    case "grad": return dim.Value * ((float)Math.PI / 200f);
                    case "turn": return dim.Value * 2f * (float)Math.PI;
                    default: return dim.Value * ((float)Math.PI / 180f); // assume deg
                }
            }
            if (value is CssNumberValue num)
            {
                // Unitless angle: CSS spec says 0 is valid, others are not,
                // but we'll be lenient and treat as degrees
                return num.Value * ((float)Math.PI / 180f);
            }
            return 0;
        }

        private static float GetNumber(CssValue value)
        {
            if (value is CssNumberValue num)
            {
                return num.Value;
            }
            if (value is CssDimensionValue dim)
            {
                return dim.Value;
            }
            return 0;
        }
    }
}
