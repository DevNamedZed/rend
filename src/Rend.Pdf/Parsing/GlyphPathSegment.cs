#nullable enable

namespace Rend.Pdf.Parsing
{
    /// <summary>The kind of a <see cref="GlyphPathSegment"/>.</summary>
    internal enum GlyphPathSegmentType
    {
        Line,
        Cubic,
    }

    /// <summary>
    /// One segment of a glyph contour in absolute font-unit coordinates: either a
    /// straight line to <see cref="EndX"/>,<see cref="EndY"/> or a cubic Bézier with
    /// two control points. Produced by <see cref="Type1CharStringInterpreter"/> and
    /// consumed by <see cref="Type2CharStringWriter"/>.
    /// </summary>
    internal readonly struct GlyphPathSegment
    {
        public GlyphPathSegmentType Type { get; }
        public float Control1X { get; }
        public float Control1Y { get; }
        public float Control2X { get; }
        public float Control2Y { get; }
        public float EndX { get; }
        public float EndY { get; }

        private GlyphPathSegment(GlyphPathSegmentType type, float control1X, float control1Y,
            float control2X, float control2Y, float endX, float endY)
        {
            Type = type;
            Control1X = control1X;
            Control1Y = control1Y;
            Control2X = control2X;
            Control2Y = control2Y;
            EndX = endX;
            EndY = endY;
        }

        public static GlyphPathSegment Line(float endX, float endY)
        {
            return new GlyphPathSegment(GlyphPathSegmentType.Line, 0f, 0f, 0f, 0f, endX, endY);
        }

        public static GlyphPathSegment Cubic(float control1X, float control1Y,
            float control2X, float control2Y, float endX, float endY)
        {
            return new GlyphPathSegment(GlyphPathSegmentType.Cubic, control1X, control1Y,
                control2X, control2Y, endX, endY);
        }
    }
}
