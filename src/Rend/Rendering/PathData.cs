using System;
using System.Collections.Generic;
using Rend.Core.Values;

namespace Rend.Rendering
{
    /// <summary>
    /// Specifies the fill rule for a path.
    /// </summary>
    public enum PathFillType
    {
        /// <summary>Non-zero winding fill rule (default).</summary>
        Winding,
        /// <summary>Even-odd fill rule (used for paths with holes).</summary>
        EvenOdd
    }

    /// <summary>
    /// Stores rounded rectangle parameters when a path is a pure rounded rect.
    /// This enables backends to use native rounded rect operations (e.g. Skia's SkRRect)
    /// instead of bezier approximations, matching Chrome's rendering exactly.
    /// </summary>
    public sealed class RoundedRectInfo
    {
        public RectF Rect { get; }
        public float TlRx { get; }
        public float TlRy { get; }
        public float TrRx { get; }
        public float TrRy { get; }
        public float BrRx { get; }
        public float BrRy { get; }
        public float BlRx { get; }
        public float BlRy { get; }

        public RoundedRectInfo(RectF rect, float tlRx, float tlRy, float trRx, float trRy,
                               float brRx, float brRy, float blRx, float blRy)
        {
            Rect = rect;
            TlRx = tlRx; TlRy = tlRy;
            TrRx = trRx; TrRy = trRy;
            BrRx = brRx; BrRy = brRy;
            BlRx = blRx; BlRy = blRy;
        }
    }

    /// <summary>
    /// Builds and stores a sequence of path segments for drawing operations.
    /// </summary>
    public sealed class PathData
    {
        private readonly List<PathSegment> _segments = new List<PathSegment>();

        /// <summary>
        /// Gets or sets the fill type for this path.
        /// </summary>
        public PathFillType FillType { get; set; } = PathFillType.Winding;

        /// <summary>
        /// If this path is a pure rounded rectangle, stores the parameters for native backend optimization.
        /// </summary>
        public RoundedRectInfo? RoundedRect { get; private set; }

        /// <summary>
        /// Moves the current point to the specified location without drawing.
        /// </summary>
        public void MoveTo(float x, float y)
        {
            _segments.Add(new PathSegment(PathSegmentType.MoveTo, x, y));
        }

        /// <summary>
        /// Draws a straight line from the current point to the specified location.
        /// </summary>
        public void LineTo(float x, float y)
        {
            _segments.Add(new PathSegment(PathSegmentType.LineTo, x, y));
        }

        /// <summary>
        /// Draws a cubic bezier curve from the current point to (x, y)
        /// using (x1, y1) and (x2, y2) as control points.
        /// </summary>
        public void CubicBezierTo(float x1, float y1, float x2, float y2, float x, float y)
        {
            _segments.Add(new PathSegment(PathSegmentType.CubicBezierTo, x, y, x1, y1, x2, y2));
        }

        /// <summary>
        /// Draws a quadratic bezier curve from the current point to (x, y)
        /// using (x1, y1) as the control point.
        /// </summary>
        public void QuadraticBezierTo(float x1, float y1, float x, float y)
        {
            _segments.Add(new PathSegment(PathSegmentType.QuadraticBezierTo, x, y, x1, y1));
        }

        /// <summary>
        /// Closes the current sub-path by drawing a line back to the starting point.
        /// </summary>
        public void Close()
        {
            _segments.Add(new PathSegment(PathSegmentType.Close));
        }

        /// <summary>
        /// Adds a rectangle to the path as a closed sub-path.
        /// </summary>
        /// <param name="rect">The rectangle to add.</param>
        public void AddRectangle(RectF rect)
        {
            MoveTo(rect.X, rect.Y);
            LineTo(rect.X + rect.Width, rect.Y);
            LineTo(rect.X + rect.Width, rect.Y + rect.Height);
            LineTo(rect.X, rect.Y + rect.Height);
            Close();
        }

        /// <summary>
        /// Adds a rounded rectangle to the path as a closed sub-path,
        /// using cubic bezier curves for each corner.
        /// </summary>
        /// <param name="rect">The bounding rectangle.</param>
        /// <param name="topLeft">The top-left corner radius.</param>
        /// <param name="topRight">The top-right corner radius.</param>
        /// <param name="bottomRight">The bottom-right corner radius.</param>
        /// <param name="bottomLeft">The bottom-left corner radius.</param>
        public void AddRoundedRectangle(RectF rect, float topLeft, float topRight, float bottomRight, float bottomLeft)
        {
            // Store rounded rect info for native backend optimization (Skia SkRRect).
            if (_segments.Count == 0)
            {
                RoundedRect = new RoundedRectInfo(rect, topLeft, topLeft, topRight, topRight,
                                                   bottomRight, bottomRight, bottomLeft, bottomLeft);
            }

            // Kappa: control point distance for approximating a quarter circle with a cubic bezier.
            const float kappa = 0.5522847498f;

            float x = rect.X;
            float y = rect.Y;
            float w = rect.Width;
            float h = rect.Height;

            // Clamp radii so they don't exceed half the rect dimensions.
            float maxRadiusX = w / 2f;
            float maxRadiusY = h / 2f;
            topLeft = Math.Min(topLeft, Math.Min(maxRadiusX, maxRadiusY));
            topRight = Math.Min(topRight, Math.Min(maxRadiusX, maxRadiusY));
            bottomRight = Math.Min(bottomRight, Math.Min(maxRadiusX, maxRadiusY));
            bottomLeft = Math.Min(bottomLeft, Math.Min(maxRadiusX, maxRadiusY));

            // Start at the top edge, after the top-left corner.
            MoveTo(x + topLeft, y);

            // Top edge -> top-right corner.
            LineTo(x + w - topRight, y);
            if (topRight > 0f)
            {
                float cp = topRight * kappa;
                CubicBezierTo(
                    x + w - topRight + cp, y,
                    x + w, y + topRight - cp,
                    x + w, y + topRight);
            }

            // Right edge -> bottom-right corner.
            LineTo(x + w, y + h - bottomRight);
            if (bottomRight > 0f)
            {
                float cp = bottomRight * kappa;
                CubicBezierTo(
                    x + w, y + h - bottomRight + cp,
                    x + w - bottomRight + cp, y + h,
                    x + w - bottomRight, y + h);
            }

            // Bottom edge -> bottom-left corner.
            LineTo(x + bottomLeft, y + h);
            if (bottomLeft > 0f)
            {
                float cp = bottomLeft * kappa;
                CubicBezierTo(
                    x + bottomLeft - cp, y + h,
                    x, y + h - bottomLeft + cp,
                    x, y + h - bottomLeft);
            }

            // Left edge -> top-left corner.
            LineTo(x, y + topLeft);
            if (topLeft > 0f)
            {
                float cp = topLeft * kappa;
                CubicBezierTo(
                    x, y + topLeft - cp,
                    x + topLeft - cp, y,
                    x + topLeft, y);
            }

            Close();
        }

        /// <summary>
        /// Add a rounded rectangle with elliptical corners (separate rx/ry per corner).
        /// </summary>
        public void AddRoundedRectangleElliptical(RectF rect,
            float tlRx, float tlRy, float trRx, float trRy,
            float brRx, float brRy, float blRx, float blRy)
        {
            // Store rounded rect info for native backend optimization (Skia SkRRect).
            if (_segments.Count == 0)
            {
                RoundedRect = new RoundedRectInfo(rect, tlRx, tlRy, trRx, trRy, brRx, brRy, blRx, blRy);
            }

            const float kappa = 0.5522847498f;

            float x = rect.X;
            float y = rect.Y;
            float w = rect.Width;
            float h = rect.Height;

            // CSS spec: if sum of adjacent radii exceeds dimension, scale them down proportionally
            float scaleX = 1f, scaleY = 1f;
            float topSum = tlRx + trRx;
            float bottomSum = blRx + brRx;
            float leftSum = tlRy + blRy;
            float rightSum = trRy + brRy;
            if (topSum > w) scaleX = Math.Min(scaleX, w / topSum);
            if (bottomSum > w) scaleX = Math.Min(scaleX, w / bottomSum);
            if (leftSum > h) scaleY = Math.Min(scaleY, h / leftSum);
            if (rightSum > h) scaleY = Math.Min(scaleY, h / rightSum);
            // Apply uniform scaling per axis
            if (scaleX < 1f) { tlRx *= scaleX; trRx *= scaleX; blRx *= scaleX; brRx *= scaleX; }
            if (scaleY < 1f) { tlRy *= scaleY; trRy *= scaleY; blRy *= scaleY; brRy *= scaleY; }

            // Start at the top edge, after the top-left corner
            MoveTo(x + tlRx, y);

            // Top edge -> top-right corner
            LineTo(x + w - trRx, y);
            if (trRx > 0f || trRy > 0f)
            {
                CubicBezierTo(
                    x + w - trRx + trRx * kappa, y,
                    x + w, y + trRy - trRy * kappa,
                    x + w, y + trRy);
            }

            // Right edge -> bottom-right corner
            LineTo(x + w, y + h - brRy);
            if (brRx > 0f || brRy > 0f)
            {
                CubicBezierTo(
                    x + w, y + h - brRy + brRy * kappa,
                    x + w - brRx + brRx * kappa, y + h,
                    x + w - brRx, y + h);
            }

            // Bottom edge -> bottom-left corner
            LineTo(x + blRx, y + h);
            if (blRx > 0f || blRy > 0f)
            {
                CubicBezierTo(
                    x + blRx - blRx * kappa, y + h,
                    x, y + h - blRy + blRy * kappa,
                    x, y + h - blRy);
            }

            // Left edge -> top-left corner
            LineTo(x, y + tlRy);
            if (tlRx > 0f || tlRy > 0f)
            {
                CubicBezierTo(
                    x, y + tlRy - tlRy * kappa,
                    x + tlRx - tlRx * kappa, y,
                    x + tlRx, y);
            }

            Close();
        }

        /// <summary>
        /// Returns the segments as a read-only list.
        /// </summary>
        /// <returns>A read-only view of the path segments.</returns>
        public IReadOnlyList<PathSegment> GetSegments()
        {
            return _segments;
        }
    }
}
