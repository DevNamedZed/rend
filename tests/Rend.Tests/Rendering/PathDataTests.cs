using Rend.Core.Values;
using Rend.Rendering;
using Xunit;

namespace Rend.Tests.Rendering
{
    public class PathDataTests
    {
        [Fact]
        public void MoveTo_AddsSegment()
        {
            var path = new PathData();
            path.MoveTo(10f, 20f);

            var segments = path.GetSegments();
            Assert.Single(segments);
            Assert.Equal(PathSegmentType.MoveTo, segments[0].Type);
            Assert.Equal(10f, segments[0].X);
            Assert.Equal(20f, segments[0].Y);
        }

        [Fact]
        public void LineTo_AddsSegment()
        {
            var path = new PathData();
            path.LineTo(30f, 40f);

            var segments = path.GetSegments();
            Assert.Single(segments);
            Assert.Equal(PathSegmentType.LineTo, segments[0].Type);
            Assert.Equal(30f, segments[0].X);
            Assert.Equal(40f, segments[0].Y);
        }

        [Fact]
        public void Close_AddsCloseSegment()
        {
            var path = new PathData();
            path.Close();

            var segments = path.GetSegments();
            Assert.Single(segments);
            Assert.Equal(PathSegmentType.Close, segments[0].Type);
        }

        [Fact]
        public void CubicBezierTo_AddsSegmentWithControlPoints()
        {
            var path = new PathData();
            path.CubicBezierTo(1f, 2f, 3f, 4f, 5f, 6f);

            var segments = path.GetSegments();
            Assert.Single(segments);
            Assert.Equal(PathSegmentType.CubicBezierTo, segments[0].Type);
            Assert.Equal(5f, segments[0].X);
            Assert.Equal(6f, segments[0].Y);
            Assert.Equal(1f, segments[0].X1);
            Assert.Equal(2f, segments[0].Y1);
            Assert.Equal(3f, segments[0].X2);
            Assert.Equal(4f, segments[0].Y2);
        }

        [Fact]
        public void QuadraticBezierTo_AddsSegmentWithControlPoint()
        {
            var path = new PathData();
            path.QuadraticBezierTo(1f, 2f, 3f, 4f);

            var segments = path.GetSegments();
            Assert.Single(segments);
            Assert.Equal(PathSegmentType.QuadraticBezierTo, segments[0].Type);
            Assert.Equal(3f, segments[0].X);
            Assert.Equal(4f, segments[0].Y);
            Assert.Equal(1f, segments[0].X1);
            Assert.Equal(2f, segments[0].Y1);
        }

        [Fact]
        public void AddRectangle_AddsCorrectSegments()
        {
            var path = new PathData();
            path.AddRectangle(new RectF(10f, 20f, 100f, 50f));

            var segments = path.GetSegments();
            // MoveTo + 3 LineTo + Close = 5 segments
            Assert.Equal(5, segments.Count);

            Assert.Equal(PathSegmentType.MoveTo, segments[0].Type);
            Assert.Equal(10f, segments[0].X);
            Assert.Equal(20f, segments[0].Y);

            Assert.Equal(PathSegmentType.LineTo, segments[1].Type);
            Assert.Equal(110f, segments[1].X); // 10 + 100
            Assert.Equal(20f, segments[1].Y);

            Assert.Equal(PathSegmentType.LineTo, segments[2].Type);
            Assert.Equal(110f, segments[2].X);
            Assert.Equal(70f, segments[2].Y); // 20 + 50

            Assert.Equal(PathSegmentType.LineTo, segments[3].Type);
            Assert.Equal(10f, segments[3].X);
            Assert.Equal(70f, segments[3].Y);

            Assert.Equal(PathSegmentType.Close, segments[4].Type);
        }

        [Fact]
        public void AddRoundedRectangle_WithZeroRadii_ProducesRectangle()
        {
            var path = new PathData();
            path.AddRoundedRectangle(new RectF(0f, 0f, 100f, 50f), 0f, 0f, 0f, 0f);

            var segments = path.GetSegments();
            // MoveTo + LineTo(top) + LineTo(right) + LineTo(bottom) + LineTo(left) + Close
            // With 0 radii, no bezier curves
            Assert.Equal(6, segments.Count);
            Assert.Equal(PathSegmentType.MoveTo, segments[0].Type);
            Assert.Equal(PathSegmentType.LineTo, segments[1].Type);
            Assert.Equal(PathSegmentType.LineTo, segments[2].Type);
            Assert.Equal(PathSegmentType.LineTo, segments[3].Type);
            Assert.Equal(PathSegmentType.LineTo, segments[4].Type);
            Assert.Equal(PathSegmentType.Close, segments[5].Type);
        }

        [Fact]
        public void AddRoundedRectangle_WithRadii_IncludesBezierCurves()
        {
            var path = new PathData();
            path.AddRoundedRectangle(new RectF(0f, 0f, 100f, 50f), 5f, 5f, 5f, 5f);

            var segments = path.GetSegments();
            // Should include CubicBezierTo segments for corners
            bool hasBezier = false;
            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i].Type == PathSegmentType.CubicBezierTo)
                {
                    hasBezier = true;
                    break;
                }
            }
            Assert.True(hasBezier, "Expected cubic bezier segments for rounded corners");
        }

        [Fact]
        public void AddRoundedRectangle_ClampsRadii()
        {
            // Radius larger than half the rect should be clamped
            var path = new PathData();
            path.AddRoundedRectangle(new RectF(0f, 0f, 20f, 10f), 100f, 100f, 100f, 100f);

            // Should not throw and should produce valid segments
            var segments = path.GetSegments();
            Assert.True(segments.Count > 0);
        }

        [Fact]
        public void GetSegments_EmptyPath_ReturnsEmptyList()
        {
            var path = new PathData();
            var segments = path.GetSegments();
            Assert.Empty(segments);
        }

        [Fact]
        public void MultipleOperations_AccumulateSegments()
        {
            var path = new PathData();
            path.MoveTo(0f, 0f);
            path.LineTo(10f, 0f);
            path.LineTo(10f, 10f);
            path.LineTo(0f, 10f);
            path.Close();

            Assert.Equal(5, path.GetSegments().Count);
        }

        [Fact]
        public void PathSegment_Type_Close_HasZeroCoordinates()
        {
            var segment = new PathSegment(PathSegmentType.Close);
            Assert.Equal(PathSegmentType.Close, segment.Type);
            Assert.Equal(0f, segment.X);
            Assert.Equal(0f, segment.Y);
        }
    }
}
