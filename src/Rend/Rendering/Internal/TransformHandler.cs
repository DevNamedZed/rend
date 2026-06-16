using System;
using System.Collections.Generic;
using System.Numerics;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Css.Resolution.Internal;
using Rend.Layout;
using Transform2D = Rend.Core.Values.Matrix3x2;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Handles CSS transform application by converting style transforms
    /// to a matrix and applying them to the render target.
    /// [CSS-TRANSFORM1] 2D: translate, scale, rotate, skew, matrix.
    /// [CSS-TRANSFORM2] 3D: perspective, rotateX/Y/Z, rotate3d, translate3d/Z,
    ///   scale3d/Z, matrix3d.
    /// </summary>
    internal static class TransformHandler
    {
        /// <summary>
        /// Checks whether the box has a CSS transform and, if so, saves the
        /// render target state and applies the transform matrix.
        /// </summary>
        /// <returns><c>true</c> if a transform was applied and the state needs to be restored.</returns>
        public static bool Apply(LayoutBox box, IRenderTarget target, Rend.Core.Values.SizeF viewport)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
            {
                return false;
            }

            Matrix4x4 composed;
            bool hasEffectiveTransform;
            if (box.ParticipatesIn3DContext)
            {
                // [CSS-TRANSFORM2 §4] Participant of a preserve-3d 3D rendering context: use the
                // accumulated 4x4 the painter already computed (EnsureAccumulated3D), not the
                // local composed transform, so ancestor preserve-3d transforms are folded in.
                composed = box.Accumulated3DTransform;
                hasEffectiveTransform = composed != Matrix4x4.Identity;
            }
            else
            {
                composed = BuildComposed(box, style, viewport, out hasEffectiveTransform);
            }
            if (!hasEffectiveTransform)
            {
                return false;
            }

            // [CSS-TRANSFORM2 §5] backface-visibility: hidden — if the element's
            // back face is toward the viewer, skip rendering entirely.
            CssValue? backfaceValue = style.GetRefValue(PropertyId.BackfaceVisibility) as CssValue;
            bool backfaceHidden = backfaceValue is CssKeywordValue bfKw && bfKw.Keyword == "hidden";
            if (backfaceHidden)
            {
                // Transform the surface normal (0,0,1) by the composed matrix.
                // If the resulting Z is negative, the back face is showing.
                float normalZ = composed.M13 * 0 + composed.M23 * 0 + composed.M33 * 1 + composed.M43 * 0;
                if (normalZ < 0)
                {
                    box.BackfaceHidden = true;
                    return false;
                }
            }

            // Flatten 4x4 → 3x3 for rendering
            Transform2D finalMatrix = Transform2D.FromMatrix4x4(composed);
            if (finalMatrix == Transform2D.Identity)
            {
                return false;
            }

            // [CSS-TRANSFORM2 §3] A transform that flattens to a non-invertible (zero-area) 2D
            // matrix maps the box to a line or point (e.g. rotateX/Y(90deg)). Chrome paints
            // nothing in that case; reusing the skip-paint flag culls the box AND its subtree
            // (otherwise a near-degenerate squash leaves anti-aliased glyph remnants on the
            // noise floor). Perspective matrices can still cover a non-zero area, so the 2x2
            // determinant test only applies without perspective.
            if (!finalMatrix.HasPerspective)
            {
                float determinant = finalMatrix.M11 * finalMatrix.M22 - finalMatrix.M12 * finalMatrix.M21;
                if (Math.Abs(determinant) < 1e-6f)
                {
                    box.BackfaceHidden = true;
                    return false;
                }
            }

            target.Save();
            target.SetTransform(finalMatrix);
            return true;
        }

        /// <summary>
        /// Builds the box's composed 4x4 transform (transform-origin · transform · perspective,
        /// row-vector) in absolute page coordinates. Sets <paramref name="hasEffectiveTransform"/>
        /// to false — and returns Identity — when the box has no transform inputs, or when its
        /// transform and the parent perspective both reduce to identity (the two cases where
        /// <see cref="Apply"/> must not touch the render target). Decoupled from Apply's
        /// render-side concerns so the preserve-3d 3D-context accumulation can reuse it even for
        /// transform-less boxes.
        /// </summary>
        private static Matrix4x4 BuildComposed(LayoutBox box, ComputedStyle style, Rend.Core.Values.SizeF viewport, out bool hasEffectiveTransform)
        {
            hasEffectiveTransform = false;

            // Check for transform property
            CssValue? transformValue = null;
            object? rawValue = style.GetRefValue(PropertyId.Transform);
            if (rawValue is CssValue tv && !(tv is CssKeywordValue tkw && tkw.Keyword == "none"))
            {
                transformValue = tv;
            }

            // [CSS-TRANSFORM2 §2.3] Check individual transform properties
            CssValue? translateValue = style.GetRefValue(PropertyId.Translate) as CssValue;
            CssValue? rotateValue = style.GetRefValue(PropertyId.Rotate) as CssValue;
            CssValue? scaleValue = style.GetRefValue(PropertyId.Scale) as CssValue;

            // Skip "none" keywords
            if (translateValue is CssKeywordValue trKw && trKw.Keyword == "none") { translateValue = null; }
            if (rotateValue is CssKeywordValue roKw && roKw.Keyword == "none") { rotateValue = null; }
            if (scaleValue is CssKeywordValue scKw && scKw.Keyword == "none") { scaleValue = null; }

            if (transformValue == null && translateValue == null && rotateValue == null && scaleValue == null)
            {
                return Matrix4x4.Identity;
            }

            // Compute transform origin (default: center of border box)
            RectF borderRect = box.BorderRect;
            float originX = borderRect.X + borderRect.Width * 0.5f;
            float originY = borderRect.Y + borderRect.Height * 0.5f;

            object? originValue = style.GetRefValue(PropertyId.TransformOrigin);
            if (originValue is CssValue originCss)
            {
                ResolveTransformOrigin(originCss, borderRect, viewport, out originX, out originY);
            }

            // [CSS-TRANSFORM2 §7] Compose perspective + transform as 4x4, then flatten
            Matrix4x4 parentPerspective = GetParentPerspective4x4(box, viewport);

            // [CSS-TRANSFORM2 §2.3] Individual properties compose as:
            // CSS: translate * rotate * scale * transform (column-vector)
            // Row-vector: transform * scale * rotate * translate
            Matrix4x4 transform4x4 = Matrix4x4.Identity;
            if (transformValue != null)
            {
                transform4x4 = BuildTransformMatrix4x4(transformValue, borderRect, viewport);
            }
            if (scaleValue != null)
            {
                transform4x4 = transform4x4 * ResolveScaleProperty(scaleValue);
            }
            if (rotateValue != null)
            {
                transform4x4 = transform4x4 * ResolveRotateProperty(rotateValue);
            }
            if (translateValue != null)
            {
                transform4x4 = transform4x4 * ResolveTranslateProperty(translateValue, borderRect, viewport);
            }

            if (parentPerspective == Matrix4x4.Identity && transform4x4 == Matrix4x4.Identity)
            {
                return Matrix4x4.Identity;
            }

            // [CSS-TRANSFORM2 §6] Row-vector composition. The child transform is applied around
            // the child's transform-origin; the parent's perspective is applied around the PARENT's
            // perspective-origin (the vanishing point) — not the child's origin.
            var toOrigin4 = Matrix4x4.CreateTranslation(-originX, -originY, 0);
            var fromOrigin4 = Matrix4x4.CreateTranslation(originX, originY, 0);
            Matrix4x4 composed;
            if (parentPerspective == Matrix4x4.Identity)
            {
                composed = toOrigin4 * transform4x4 * fromOrigin4;
            }
            else
            {
                GetParentPerspectiveOrigin(box, viewport, out float perspOriginX, out float perspOriginY);
                var toPersp4 = Matrix4x4.CreateTranslation(-perspOriginX, -perspOriginY, 0);
                var fromPersp4 = Matrix4x4.CreateTranslation(perspOriginX, perspOriginY, 0);
                // [transform around child-origin] then [perspective around perspective-origin].
                // Reduces to the prior formula when the two origins coincide (the default case).
                composed = toOrigin4 * transform4x4 * fromOrigin4
                    * toPersp4 * parentPerspective * fromPersp4;
            }

            hasEffectiveTransform = true;
            return composed;
        }

        /// <summary>
        /// Returns the box's local composed 4x4 transform in absolute page coordinates (Identity
        /// when the box has no transform), independent of <see cref="Apply"/>'s render-side early
        /// returns. Used to seed a preserve-3d context root's accumulated transform so its
        /// participating children fold the root's own transform in.
        /// </summary>
        internal static Matrix4x4 ComputeLocalComposed(LayoutBox box, Rend.Core.Values.SizeF viewport)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
            {
                return Matrix4x4.Identity;
            }
            return BuildComposed(box, style, viewport, out _);
        }

        /// <summary>
        /// [CSS-TRANSFORM2 §4] Populates <see cref="LayoutBox.Accumulated3DTransform"/> and
        /// <see cref="LayoutBox.Depth3D"/> for a participant of a preserve-3d 3D rendering
        /// context. The accumulated transform folds the box's local composed matrix into its
        /// parent's accumulated transform (row-vector); Depth3D is the Z of the box's absolute
        /// border-box center under that matrix — the painter's-algorithm back-to-front sort key.
        /// Must be called before the participant is painted so <see cref="Apply"/> reads it.
        /// </summary>
        internal static void EnsureAccumulated3D(LayoutBox box, Rend.Core.Values.SizeF viewport)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            Matrix4x4 localComposed = style != null
                ? BuildComposed(box, style, viewport, out _)
                : Matrix4x4.Identity;
            Matrix4x4 parentAccumulated = box.Parent != null
                ? box.Parent.Accumulated3DTransform
                : Matrix4x4.Identity;
            Matrix4x4 accumulated = localComposed * parentAccumulated;
            box.Accumulated3DTransform = accumulated;

            RectF borderRect = box.BorderRect;
            float centerX = borderRect.X + borderRect.Width * 0.5f;
            float centerY = borderRect.Y + borderRect.Height * 0.5f;
            // Z of the transformed absolute border-box center (row-vector: [cx cy 0 1] * M).
            box.Depth3D = centerX * accumulated.M13 + centerY * accumulated.M23 + accumulated.M43;
        }

        /// <summary>
        /// Restores the render target state if a transform was previously applied.
        /// </summary>
        public static void Restore(IRenderTarget target)
        {
            target.Restore();
        }

        /// <summary>
        /// [CSS-TRANSFORM2 §7] Get the perspective matrix from the parent's CSS perspective property.
        /// Returns a 4x4 matrix to be composed with the child's transform before flattening.
        /// </summary>
        private static Matrix4x4 GetParentPerspective4x4(LayoutBox box, Rend.Core.Values.SizeF viewport)
        {
            LayoutBox? parent = box.Parent;
            if (parent == null)
            {
                return Matrix4x4.Identity;
            }

            ComputedStyle? parentStyle = parent.StyledNode?.Style;
            if (parentStyle == null)
            {
                return Matrix4x4.Identity;
            }

            object? perspValue = parentStyle.GetRefValue(PropertyId.Perspective);
            if (perspValue == null)
            {
                return Matrix4x4.Identity;
            }

            float distance = 0f;
            if (perspValue is CssValue perspCss)
            {
                if (perspCss is CssKeywordValue kwPersp && kwPersp.Keyword == "none")
                {
                    return Matrix4x4.Identity;
                }
                distance = ResolveLength(perspCss, viewport);
            }

            if (distance <= 0f)
            {
                return Matrix4x4.Identity;
            }

            // [CSS-TRANSFORM2 §7] perspective matrix: M34 = -1/d
            var perspMatrix = Matrix4x4.Identity;
            perspMatrix.M34 = -1f / distance;
            return perspMatrix;
        }

        /// <summary>
        /// [CSS-TRANSFORM2 §6] The vanishing point for the parent's perspective, in absolute
        /// coordinates. perspective-origin is resolved against the parent's border box; the
        /// default is its center (50% 50%).
        /// </summary>
        private static void GetParentPerspectiveOrigin(LayoutBox box, Rend.Core.Values.SizeF viewport, out float originX, out float originY)
        {
            LayoutBox? parent = box.Parent;
            RectF parentRect = parent != null ? parent.BorderRect : box.BorderRect;
            originX = parentRect.X + parentRect.Width * 0.5f;
            originY = parentRect.Y + parentRect.Height * 0.5f;

            object? originValue = parent?.StyledNode?.Style?.GetRefValue(PropertyId.PerspectiveOrigin);
            if (originValue is CssValue originCss
                && !(originCss is CssKeywordValue kw && kw.Keyword == "none"))
            {
                ResolveTransformOrigin(originCss, parentRect, viewport, out originX, out originY);
            }
        }

        /// <summary>
        /// [CSS-TRANSFORM2 §2.3] Resolve the 'translate' individual property.
        /// Syntax: none | x [y [z]]
        /// </summary>
        private static Matrix4x4 ResolveTranslateProperty(CssValue value, RectF borderRect, Rend.Core.Values.SizeF viewport)
        {
            if (value is CssListValue list && list.Separator == ' ')
            {
                float tx = list.Values.Count > 0 ? ResolveLengthOrPercent(list.Values[0], borderRect.Width, viewport) : 0;
                float ty = list.Values.Count > 1 ? ResolveLengthOrPercent(list.Values[1], borderRect.Height, viewport) : 0;
                float tz = list.Values.Count > 2 ? ResolveLength(list.Values[2], viewport) : 0;
                return Matrix4x4.CreateTranslation(tx, ty, tz);
            }
            float singleTx = ResolveLengthOrPercent(value, borderRect.Width, viewport);
            return Matrix4x4.CreateTranslation(singleTx, 0, 0);
        }

        /// <summary>
        /// [CSS-TRANSFORM2 §2.3] Resolve the 'rotate' individual property.
        /// Syntax: none | angle | [x y z] angle
        /// </summary>
        private static Matrix4x4 ResolveRotateProperty(CssValue value)
        {
            if (value is CssListValue list && list.Separator == ' ')
            {
                if (list.Values.Count == 4)
                {
                    float rx = GetNumber(list.Values[0]);
                    float ry = GetNumber(list.Values[1]);
                    float rz = GetNumber(list.Values[2]);
                    float angle = ResolveAngle(list.Values[3]);
                    var axis = new Vector3(rx, ry, rz);
                    float length = axis.Length();
                    if (length < 0.0001f)
                    {
                        return Matrix4x4.Identity;
                    }
                    return Matrix4x4.CreateFromAxisAngle(axis / length, angle);
                }
                if (list.Values.Count == 2)
                {
                    // "x angle" or "y angle" or "z angle" keyword + angle
                    if (list.Values[0] is CssKeywordValue axisKw)
                    {
                        float angle = ResolveAngle(list.Values[1]);
                        switch (axisKw.Keyword)
                        {
                            case "x": return Matrix4x4.CreateRotationX(angle);
                            case "y": return Matrix4x4.CreateRotationY(angle);
                            case "z": return Matrix4x4.CreateRotationZ(angle);
                        }
                    }
                }
            }
            // Single angle → rotateZ
            float singleAngle = ResolveAngle(value);
            return Matrix4x4.CreateRotationZ(singleAngle);
        }

        /// <summary>
        /// [CSS-TRANSFORM2 §2.3] Resolve the 'scale' individual property.
        /// Syntax: none | sx [sy [sz]]
        /// </summary>
        private static Matrix4x4 ResolveScaleProperty(CssValue value)
        {
            if (value is CssListValue list && list.Separator == ' ')
            {
                float sx = list.Values.Count > 0 ? GetNumber(list.Values[0]) : 1;
                float sy = list.Values.Count > 1 ? GetNumber(list.Values[1]) : sx;
                float sz = list.Values.Count > 2 ? GetNumber(list.Values[2]) : 1;
                return Matrix4x4.CreateScale(sx, sy, sz);
            }
            if (value is CssPercentageValue pct)
            {
                float s = pct.Value / 100f;
                return Matrix4x4.CreateScale(s, s, 1);
            }
            float singleScale = GetNumber(value);
            return Matrix4x4.CreateScale(singleScale, singleScale, 1);
        }

        /// <summary>
        /// Build the transform as a 4x4 matrix (always, for perspective composition).
        /// </summary>
        private static Matrix4x4 BuildTransformMatrix4x4(CssValue value, RectF? refBox, Rend.Core.Values.SizeF viewport)
        {
            var functions = CollectFunctions(value);
            if (functions.Count == 0)
            {
                return Matrix4x4.Identity;
            }

            Matrix4x4 result = Matrix4x4.Identity;
            for (int i = 0; i < functions.Count; i++)
            {
                Matrix4x4 m = Parse3DFunction(functions[i], refBox, viewport);
                result = result * m;
            }
            return result;
        }

        /// <summary>
        /// Builds a Matrix3x2 from a CSS transform value (single function or space-separated list).
        /// Uses 4x4 matrix internally for 3D transforms, then flattens to 3x3.
        /// </summary>
        internal static Transform2D BuildTransformMatrix(CssValue value, Rend.Core.Values.SizeF viewport, RectF? refBox = null)
        {
            var functions = CollectFunctions(value);
            if (functions.Count == 0)
            {
                return Transform2D.Identity;
            }

            // Check if any function requires 3D
            bool has3D = false;
            for (int i = 0; i < functions.Count; i++)
            {
                if (Is3DFunction(functions[i].Name))
                {
                    has3D = true;
                    break;
                }
            }

            if (has3D)
            {
                return Build3DTransform(functions, refBox, viewport);
            }

            return Build2DTransform(functions, refBox, viewport);
        }

        private static Transform2D Build2DTransform(List<CssFunctionValue> functions, RectF? refBox, Rend.Core.Values.SizeF viewport)
        {
            Transform2D result = Transform2D.Identity;
            for (int i = 0; i < functions.Count; i++)
            {
                Transform2D m = Parse2DFunction(functions[i], refBox, viewport);
                result = result * m;
            }
            return result;
        }

        private static Transform2D Build3DTransform(List<CssFunctionValue> functions, RectF? refBox, Rend.Core.Values.SizeF viewport)
        {
            // [CSS-TRANSFORM2 §5] Compose transforms as 4x4 matrices, then flatten
            Matrix4x4 result = Matrix4x4.Identity;
            for (int i = 0; i < functions.Count; i++)
            {
                Matrix4x4 m = Parse3DFunction(functions[i], refBox, viewport);
                result = result * m;
            }
            return Transform2D.FromMatrix4x4(result);
        }

        private static List<CssFunctionValue> CollectFunctions(CssValue value)
        {
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
            return functions;
        }

        private static bool Is3DFunction(string name)
        {
            string lower = name.ToLowerInvariant();
            return lower == "rotatex" || lower == "rotatey" || lower == "rotatez"
                || lower == "rotate3d" || lower == "translatez" || lower == "translate3d"
                || lower == "scalez" || lower == "scale3d" || lower == "matrix3d"
                || lower == "perspective";
        }

        private static Matrix4x4 Parse3DFunction(CssFunctionValue fn, RectF? refBox, Rend.Core.Values.SizeF viewport)
        {
            string name = fn.Name.ToLowerInvariant();
            var args = fn.Arguments;

            switch (name)
            {
                // --- 2D functions promoted to 4x4 ---
                case "translate":
                {
                    float tx = args.Count > 0 ? ResolveLengthOrPercent(args[0], refBox?.Width ?? 0, viewport) : 0;
                    float ty = args.Count > 1 ? ResolveLengthOrPercent(args[1], refBox?.Height ?? 0, viewport) : 0;
                    return Matrix4x4.CreateTranslation(tx, ty, 0);
                }

                case "translatex":
                {
                    float tx = args.Count > 0 ? ResolveLengthOrPercent(args[0], refBox?.Width ?? 0, viewport) : 0;
                    return Matrix4x4.CreateTranslation(tx, 0, 0);
                }

                case "translatey":
                {
                    float ty = args.Count > 0 ? ResolveLengthOrPercent(args[0], refBox?.Height ?? 0, viewport) : 0;
                    return Matrix4x4.CreateTranslation(0, ty, 0);
                }

                case "translatez":
                {
                    float tz = args.Count > 0 ? ResolveLength(args[0], viewport) : 0;
                    return Matrix4x4.CreateTranslation(0, 0, tz);
                }

                case "translate3d":
                {
                    float tx = args.Count > 0 ? ResolveLengthOrPercent(args[0], refBox?.Width ?? 0, viewport) : 0;
                    float ty = args.Count > 1 ? ResolveLengthOrPercent(args[1], refBox?.Height ?? 0, viewport) : 0;
                    float tz = args.Count > 2 ? ResolveLength(args[2], viewport) : 0;
                    return Matrix4x4.CreateTranslation(tx, ty, tz);
                }

                case "scale":
                {
                    float sx = args.Count > 0 ? GetNumber(args[0]) : 1;
                    float sy = args.Count > 1 ? GetNumber(args[1]) : sx;
                    return Matrix4x4.CreateScale(sx, sy, 1);
                }

                case "scalex":
                {
                    float sx = args.Count > 0 ? GetNumber(args[0]) : 1;
                    return Matrix4x4.CreateScale(sx, 1, 1);
                }

                case "scaley":
                {
                    float sy = args.Count > 0 ? GetNumber(args[0]) : 1;
                    return Matrix4x4.CreateScale(1, sy, 1);
                }

                case "scalez":
                {
                    float sz = args.Count > 0 ? GetNumber(args[0]) : 1;
                    return Matrix4x4.CreateScale(1, 1, sz);
                }

                case "scale3d":
                {
                    float sx = args.Count > 0 ? GetNumber(args[0]) : 1;
                    float sy = args.Count > 1 ? GetNumber(args[1]) : 1;
                    float sz = args.Count > 2 ? GetNumber(args[2]) : 1;
                    return Matrix4x4.CreateScale(sx, sy, sz);
                }

                case "rotate":
                {
                    float angle = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    return Matrix4x4.CreateRotationZ(angle);
                }

                case "rotatex":
                {
                    float angle = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    return Matrix4x4.CreateRotationX(angle);
                }

                case "rotatey":
                {
                    float angle = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    return Matrix4x4.CreateRotationY(angle);
                }

                case "rotatez":
                {
                    float angle = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    return Matrix4x4.CreateRotationZ(angle);
                }

                case "rotate3d":
                {
                    float rx = args.Count > 0 ? GetNumber(args[0]) : 0;
                    float ry = args.Count > 1 ? GetNumber(args[1]) : 0;
                    float rz = args.Count > 2 ? GetNumber(args[2]) : 0;
                    float angle = args.Count > 3 ? ResolveAngle(args[3]) : 0;
                    var axis = new Vector3(rx, ry, rz);
                    float length = axis.Length();
                    if (length < 0.0001f)
                    {
                        return Matrix4x4.Identity;
                    }
                    return Matrix4x4.CreateFromAxisAngle(axis / length, angle);
                }

                case "skew":
                {
                    float ax = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    float ay = args.Count > 1 ? ResolveAngle(args[1]) : 0;
                    var m = Matrix4x4.Identity;
                    m.M21 = (float)Math.Tan(ax);
                    m.M12 = (float)Math.Tan(ay);
                    return m;
                }

                case "skewx":
                {
                    float ax = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    var m = Matrix4x4.Identity;
                    m.M21 = (float)Math.Tan(ax);
                    return m;
                }

                case "skewy":
                {
                    float ay = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    var m = Matrix4x4.Identity;
                    m.M12 = (float)Math.Tan(ay);
                    return m;
                }

                case "perspective":
                {
                    // [CSS-TRANSFORM2 §14] perspective(d)
                    float distance = args.Count > 0 ? ResolveLength(args[0], viewport) : 0;
                    if (distance <= 0)
                    {
                        return Matrix4x4.Identity;
                    }
                    var m = Matrix4x4.Identity;
                    m.M34 = -1f / distance;
                    return m;
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
                        // CSS matrix(a,b,c,d,e,f) → 4x4
                        return new Matrix4x4(
                            a, b, 0, 0,
                            c, d, 0, 0,
                            0, 0, 1, 0,
                            e, f, 0, 1);
                    }
                    return Matrix4x4.Identity;
                }

                case "matrix3d":
                {
                    // [CSS-TRANSFORM2 §14] matrix3d(16 values in column-major order)
                    if (args.Count >= 16)
                    {
                        return new Matrix4x4(
                            GetNumber(args[0]), GetNumber(args[1]),
                            GetNumber(args[2]), GetNumber(args[3]),
                            GetNumber(args[4]), GetNumber(args[5]),
                            GetNumber(args[6]), GetNumber(args[7]),
                            GetNumber(args[8]), GetNumber(args[9]),
                            GetNumber(args[10]), GetNumber(args[11]),
                            GetNumber(args[12]), GetNumber(args[13]),
                            GetNumber(args[14]), GetNumber(args[15]));
                    }
                    return Matrix4x4.Identity;
                }

                default:
                    return Matrix4x4.Identity;
            }
        }

        private static Transform2D Parse2DFunction(CssFunctionValue fn, RectF? refBox, Rend.Core.Values.SizeF viewport)
        {
            string name = fn.Name.ToLowerInvariant();
            var args = fn.Arguments;

            switch (name)
            {
                case "translate":
                {
                    float tx = args.Count > 0 ? ResolveLengthOrPercent(args[0], refBox?.Width ?? 0, viewport) : 0;
                    float ty = args.Count > 1 ? ResolveLengthOrPercent(args[1], refBox?.Height ?? 0, viewport) : 0;
                    return Transform2D.CreateTranslation(tx, ty);
                }

                case "translatex":
                {
                    float tx = args.Count > 0 ? ResolveLengthOrPercent(args[0], refBox?.Width ?? 0, viewport) : 0;
                    return Transform2D.CreateTranslation(tx, 0);
                }

                case "translatey":
                {
                    float ty = args.Count > 0 ? ResolveLengthOrPercent(args[0], refBox?.Height ?? 0, viewport) : 0;
                    return Transform2D.CreateTranslation(0, ty);
                }

                case "scale":
                {
                    float sx = args.Count > 0 ? GetNumber(args[0]) : 1;
                    float sy = args.Count > 1 ? GetNumber(args[1]) : sx;
                    return Transform2D.CreateScale(sx, sy);
                }

                case "scalex":
                {
                    float sx = args.Count > 0 ? GetNumber(args[0]) : 1;
                    return Transform2D.CreateScale(sx, 1);
                }

                case "scaley":
                {
                    float sy = args.Count > 0 ? GetNumber(args[0]) : 1;
                    return Transform2D.CreateScale(1, sy);
                }

                case "rotate":
                {
                    float angle = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    return Transform2D.CreateRotation(angle);
                }

                case "skew":
                {
                    float ax = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    float ay = args.Count > 1 ? ResolveAngle(args[1]) : 0;
                    return Transform2D.CreateSkew(ax, ay);
                }

                case "skewx":
                {
                    float ax = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    return Transform2D.CreateSkew(ax, 0);
                }

                case "skewy":
                {
                    float ay = args.Count > 0 ? ResolveAngle(args[0]) : 0;
                    return Transform2D.CreateSkew(0, ay);
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
                        return new Transform2D(a, b, c, d, e, f);
                    }
                    return Transform2D.Identity;
                }

                default:
                    return Transform2D.Identity;
            }
        }

        private static void ResolveTransformOrigin(CssValue value, RectF borderRect,
            Rend.Core.Values.SizeF viewport, out float originX, out float originY)
        {
            // Default: center
            originX = borderRect.X + borderRect.Width * 0.5f;
            originY = borderRect.Y + borderRect.Height * 0.5f;

            if (value is CssListValue list && list.Separator == ' ' && list.Values.Count >= 2)
            {
                // [CSS-TRANSFORMS-1 §3] Keyword components may appear in either order
                // (e.g. "top center" == "center top" == "50% 0%"). A vertical keyword
                // (top/bottom) first, or a horizontal keyword (left/right) second, means
                // the X and Y components are swapped. Lengths/percentages stay positional.
                CssValue first = list.Values[0];
                CssValue second = list.Values[1];
                bool swap = IsVerticalOriginKeyword(first) || IsHorizontalOriginKeyword(second);
                CssValue xValue = swap ? second : first;
                CssValue yValue = swap ? first : second;
                originX = borderRect.X + ResolveOriginComponent(xValue, borderRect.Width, viewport);
                originY = borderRect.Y + ResolveOriginComponent(yValue, borderRect.Height, viewport);
            }
            else if (value is CssKeywordValue kwOrigin)
            {
                ResolveOriginKeyword(kwOrigin.Keyword, borderRect, out originX, out originY);
            }
            else if (value is CssDimensionValue dim)
            {
                originX = borderRect.X + ResolveLengthValue(dim);
            }
            else if (value is CssPercentageValue pct)
            {
                originX = borderRect.X + pct.Value / 100f * borderRect.Width;
            }
        }

        private static bool IsVerticalOriginKeyword(CssValue value)
            => value is CssKeywordValue kw && (kw.Keyword == "top" || kw.Keyword == "bottom");

        private static bool IsHorizontalOriginKeyword(CssValue value)
            => value is CssKeywordValue kw && (kw.Keyword == "left" || kw.Keyword == "right");

        private static float ResolveOriginComponent(CssValue value, float size, Rend.Core.Values.SizeF viewport)
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
            if (value is CssKeywordValue kwComp)
            {
                switch (kwComp.Keyword)
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
            if (value is CssFunctionValue fn && fn.Name == "calc")
            {
                return ValueResolver.EvaluateDeferredCalc(fn, size, viewport.Width, viewport.Height);
            }
            return size * 0.5f;
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
                    break;
            }
        }

        private static float ResolveLengthOrPercent(CssValue value, float referenceSize, Rend.Core.Values.SizeF viewport)
        {
            if (value is CssDimensionValue dim)
            {
                return ResolveLengthValue(dim);
            }
            if (value is CssNumberValue num)
            {
                return num.Value;
            }
            if (value is CssPercentageValue pct)
            {
                return pct.Value * referenceSize / 100f;
            }
            if (value is CssFunctionValue fn && fn.Name == "calc")
            {
                return ValueResolver.EvaluateDeferredCalc(fn, referenceSize, viewport.Width, viewport.Height);
            }
            return 0;
        }

        private static float ResolveLength(CssValue value, Rend.Core.Values.SizeF viewport)
        {
            return ResolveLengthOrPercent(value, 0, viewport);
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
                case "em": return dim.Value * 16f; // Approximate em as 16px
                case "rem": return dim.Value * 16f;
                default: return dim.Value;
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
                    default: return dim.Value * ((float)Math.PI / 180f);
                }
            }
            if (value is CssNumberValue num)
            {
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
