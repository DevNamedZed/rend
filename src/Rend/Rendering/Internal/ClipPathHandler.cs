using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Handles CSS clip-path property by pushing clip paths onto the render target.
    /// Supports inset(), circle(), ellipse(), and polygon() shapes.
    /// </summary>
    internal static class ClipPathHandler
    {
        public static bool Apply(LayoutBox box, IRenderTarget target)
        {
            if (box.StyledNode?.Style == null)
                return false;

            var raw = box.StyledNode.Style.GetRefValue(PropertyId.ClipPath);
            if (raw == null)
                return false;

            if (raw is CssKeywordValue kw && kw.Keyword == "none")
                return false;

            RectF refBox = box.BorderRect;
            var path = BuildClipPath(raw, refBox);
            if (path == null)
                return false;

            target.PushClipPath(path);
            return true;
        }

        public static void Restore(IRenderTarget target)
        {
            target.PopClip();
        }

        private static PathData? BuildClipPath(object raw, RectF refBox)
        {
            if (raw is CssFunctionValue fn)
                return BuildShapePath(fn, refBox);

            if (raw is CssListValue list)
            {
                // clip-path may be a list if combined with a geometry-box keyword
                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (list.Values[i] is CssFunctionValue shapeFn)
                        return BuildShapePath(shapeFn, refBox);
                }
            }

            return null;
        }

        private static PathData? BuildShapePath(CssFunctionValue fn, RectF refBox)
        {
            switch (fn.Name)
            {
                case "inset": return BuildInset(fn, refBox);
                case "circle": return BuildCircle(fn, refBox);
                case "ellipse": return BuildEllipse(fn, refBox);
                case "polygon": return BuildPolygon(fn, refBox);
                default: return null;
            }
        }

        private static PathData BuildInset(CssFunctionValue fn, RectF refBox)
        {
            // inset(top right bottom left [round radius])
            float top = 0, right = 0, bottom = 0, left = 0;
            float radius = 0;

            int count = 0;
            for (int i = 0; i < fn.Arguments.Count; i++)
            {
                if (fn.Arguments[i] is CssKeywordValue kw && kw.Keyword == "round")
                {
                    // Next arg is radius
                    if (i + 1 < fn.Arguments.Count)
                        radius = ResolveLengthOrPercent(fn.Arguments[i + 1], refBox.Width);
                    break;
                }

                float val = ResolveLengthOrPercent(fn.Arguments[i], count < 2 ? refBox.Height : refBox.Width);
                switch (count)
                {
                    case 0: top = val; right = val; bottom = val; left = val; break;
                    case 1: right = val; left = val; break;
                    case 2: bottom = val; break;
                    case 3: left = val; break;
                }
                count++;
            }

            var path = new PathData();
            var clipRect = new RectF(
                refBox.X + left,
                refBox.Y + top,
                refBox.Width - left - right,
                refBox.Height - top - bottom);

            if (radius > 0)
                path.AddRoundedRectangle(clipRect, radius, radius, radius, radius);
            else
                path.AddRectangle(clipRect);

            return path;
        }

        private static PathData BuildCircle(CssFunctionValue fn, RectF refBox)
        {
            // circle(radius at cx cy)
            float cx = refBox.X + refBox.Width / 2;
            float cy = refBox.Y + refBox.Height / 2;
            float r = Math.Min(refBox.Width, refBox.Height) / 2;

            bool foundAt = false;
            int posIdx = 0;
            for (int i = 0; i < fn.Arguments.Count; i++)
            {
                if (fn.Arguments[i] is CssKeywordValue kw)
                {
                    if (kw.Keyword == "at") { foundAt = true; continue; }
                    if (kw.Keyword == "closest-side")
                    {
                        r = Math.Min(refBox.Width, refBox.Height) / 2;
                        continue;
                    }
                    if (kw.Keyword == "farthest-side")
                    {
                        r = Math.Max(refBox.Width, refBox.Height) / 2;
                        continue;
                    }
                }

                if (foundAt)
                {
                    if (posIdx == 0) { cx = refBox.X + ResolveLengthOrPercent(fn.Arguments[i], refBox.Width); posIdx++; }
                    else if (posIdx == 1) { cy = refBox.Y + ResolveLengthOrPercent(fn.Arguments[i], refBox.Height); posIdx++; }
                }
                else if (i == 0)
                {
                    float diagRef = (float)Math.Sqrt(refBox.Width * refBox.Width + refBox.Height * refBox.Height) / 1.4142f;
                    r = ResolveLengthOrPercent(fn.Arguments[i], diagRef);
                }
            }

            return BuildCirclePath(cx, cy, r);
        }

        private static PathData BuildEllipse(CssFunctionValue fn, RectF refBox)
        {
            // ellipse(rx ry at cx cy)
            float cx = refBox.X + refBox.Width / 2;
            float cy = refBox.Y + refBox.Height / 2;
            float rx = refBox.Width / 2;
            float ry = refBox.Height / 2;

            bool foundAt = false;
            int radiusIdx = 0;
            int posIdx = 0;
            for (int i = 0; i < fn.Arguments.Count; i++)
            {
                if (fn.Arguments[i] is CssKeywordValue kw && kw.Keyword == "at")
                {
                    foundAt = true;
                    continue;
                }

                if (foundAt)
                {
                    if (posIdx == 0) { cx = refBox.X + ResolveLengthOrPercent(fn.Arguments[i], refBox.Width); posIdx++; }
                    else if (posIdx == 1) { cy = refBox.Y + ResolveLengthOrPercent(fn.Arguments[i], refBox.Height); posIdx++; }
                }
                else
                {
                    if (radiusIdx == 0) { rx = ResolveLengthOrPercent(fn.Arguments[i], refBox.Width); radiusIdx++; }
                    else if (radiusIdx == 1) { ry = ResolveLengthOrPercent(fn.Arguments[i], refBox.Height); radiusIdx++; }
                }
            }

            return BuildEllipsePath(cx, cy, rx, ry);
        }

        private static PathData BuildPolygon(CssFunctionValue fn, RectF refBox)
        {
            // polygon(x1 y1, x2 y2, ...)
            var path = new PathData();
            bool first = true;

            for (int i = 0; i + 1 < fn.Arguments.Count; i += 2)
            {
                float x = refBox.X + ResolveLengthOrPercent(fn.Arguments[i], refBox.Width);
                float y = refBox.Y + ResolveLengthOrPercent(fn.Arguments[i + 1], refBox.Height);

                if (first) { path.MoveTo(x, y); first = false; }
                else path.LineTo(x, y);
            }

            if (!first) path.Close();
            return path;
        }

        private static PathData BuildCirclePath(float cx, float cy, float r)
        {
            return BuildEllipsePath(cx, cy, r, r);
        }

        private static PathData BuildEllipsePath(float cx, float cy, float rx, float ry)
        {
            const float kappa = 0.5522847498f;
            float kx = rx * kappa;
            float ky = ry * kappa;

            var path = new PathData();
            // Start at top
            path.MoveTo(cx, cy - ry);
            // Top-right quadrant
            path.CubicBezierTo(cx + kx, cy - ry, cx + rx, cy - ky, cx + rx, cy);
            // Bottom-right quadrant
            path.CubicBezierTo(cx + rx, cy + ky, cx + kx, cy + ry, cx, cy + ry);
            // Bottom-left quadrant
            path.CubicBezierTo(cx - kx, cy + ry, cx - rx, cy + ky, cx - rx, cy);
            // Top-left quadrant
            path.CubicBezierTo(cx - rx, cy - ky, cx - kx, cy - ry, cx, cy - ry);
            path.Close();

            return path;
        }

        private static float ResolveLengthOrPercent(CssValue val, float reference)
        {
            if (val is CssDimensionValue dim) return dim.Value;
            if (val is CssPercentageValue pct) return pct.Value / 100f * reference;
            if (val is CssNumberValue num) return num.Value;
            return 0;
        }
    }
}
