#nullable enable
using System.Collections.Generic;

namespace Rend.Pdf.Parsing
{
    /// <summary>
    /// A single closed contour of a glyph: a start point followed by line/curve
    /// segments, all in absolute font-unit coordinates.
    /// </summary>
    internal sealed class GlyphContour
    {
        public float StartX { get; }
        public float StartY { get; }
        public List<GlyphPathSegment> Segments { get; } = new List<GlyphPathSegment>();

        public GlyphContour(float startX, float startY)
        {
            StartX = startX;
            StartY = startY;
        }
    }
}
