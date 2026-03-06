using System.Collections.Generic;
using Rend.Rendering;
using SkiaSharp;

namespace Rend.Output.Image.Internal
{
    /// <summary>
    /// Converts <see cref="PathData"/> path segments into a SkiaSharp <see cref="SKPath"/>.
    /// </summary>
    internal static class SkiaPathConverter
    {
        /// <summary>
        /// Converts the given path data to an <see cref="SKPath"/>.
        /// </summary>
        /// <param name="path">The path data to convert.</param>
        /// <returns>A new SKPath representing the same geometry.</returns>
        internal static SKPath Convert(PathData path)
        {
            var skPath = new SKPath();
            IReadOnlyList<PathSegment> segments = path.GetSegments();

            for (int i = 0; i < segments.Count; i++)
            {
                PathSegment seg = segments[i];
                switch (seg.Type)
                {
                    case PathSegmentType.MoveTo:
                        skPath.MoveTo(seg.X, seg.Y);
                        break;
                    case PathSegmentType.LineTo:
                        skPath.LineTo(seg.X, seg.Y);
                        break;
                    case PathSegmentType.CubicBezierTo:
                        skPath.CubicTo(seg.X1, seg.Y1, seg.X2, seg.Y2, seg.X, seg.Y);
                        break;
                    case PathSegmentType.QuadraticBezierTo:
                        skPath.QuadTo(seg.X1, seg.Y1, seg.X, seg.Y);
                        break;
                    case PathSegmentType.Close:
                        skPath.Close();
                        break;
                }
            }

            if (path.FillType == PathFillType.EvenOdd)
                skPath.FillType = SKPathFillType.EvenOdd;

            return skPath;
        }
    }
}
