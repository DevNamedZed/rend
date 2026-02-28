namespace Rend.Rendering
{
    /// <summary>
    /// Identifies the type of a path segment.
    /// </summary>
    public enum PathSegmentType : byte
    {
        MoveTo,
        LineTo,
        CubicBezierTo,
        QuadraticBezierTo,
        Close
    }

    /// <summary>
    /// Represents a single segment within a drawing path.
    /// </summary>
    public readonly struct PathSegment
    {
        /// <summary>Gets the segment type.</summary>
        public PathSegmentType Type { get; }

        /// <summary>Gets the target X coordinate.</summary>
        public float X { get; }

        /// <summary>Gets the target Y coordinate.</summary>
        public float Y { get; }

        /// <summary>Gets the first control point X coordinate.</summary>
        public float X1 { get; }

        /// <summary>Gets the first control point Y coordinate.</summary>
        public float Y1 { get; }

        /// <summary>Gets the second control point X coordinate (cubic bezier only).</summary>
        public float X2 { get; }

        /// <summary>Gets the second control point Y coordinate (cubic bezier only).</summary>
        public float Y2 { get; }

        /// <summary>
        /// Creates a new <see cref="PathSegment"/>.
        /// </summary>
        /// <param name="type">The segment type.</param>
        /// <param name="x">The target X coordinate.</param>
        /// <param name="y">The target Y coordinate.</param>
        /// <param name="x1">The first control point X.</param>
        /// <param name="y1">The first control point Y.</param>
        /// <param name="x2">The second control point X.</param>
        /// <param name="y2">The second control point Y.</param>
        public PathSegment(PathSegmentType type, float x = 0, float y = 0, float x1 = 0, float y1 = 0, float x2 = 0, float y2 = 0)
        {
            Type = type;
            X = x;
            Y = y;
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }
    }
}
