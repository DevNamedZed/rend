using System;
using System.Collections.Generic;
using Rend.Core.Values;

namespace Rend.Rendering
{
    /// <summary>
    /// Builds and stores a sequence of path segments for drawing operations.
    /// </summary>
    public sealed class PathData
    {
        private readonly List<PathSegment> _segments = new List<PathSegment>();

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
        /// Returns the segments as a read-only list.
        /// </summary>
        /// <returns>A read-only view of the path segments.</returns>
        public IReadOnlyList<PathSegment> GetSegments()
        {
            return _segments;
        }
    }
}
