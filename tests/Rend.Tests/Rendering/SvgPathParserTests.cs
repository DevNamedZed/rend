using System;
using Rend.Rendering;
using Rend.Rendering.Internal;
using Xunit;

namespace Rend.Tests.Rendering
{
    public class SvgPathParserTests
    {
        [Fact]
        public void Parse_MoveTo_Absolute()
        {
            var path = SvgPathParser.Parse("M 10 20");
            var segs = path.GetSegments();
            Assert.Single(segs);
            Assert.Equal(PathSegmentType.MoveTo, segs[0].Type);
            Assert.Equal(10f, segs[0].X);
            Assert.Equal(20f, segs[0].Y);
        }

        [Fact]
        public void Parse_MoveTo_Relative()
        {
            var path = SvgPathParser.Parse("M 10 20 m 5 5");
            var segs = path.GetSegments();
            Assert.Equal(2, segs.Count);
            Assert.Equal(15f, segs[1].X);
            Assert.Equal(25f, segs[1].Y);
        }

        [Fact]
        public void Parse_LineTo_Absolute()
        {
            var path = SvgPathParser.Parse("M 0 0 L 100 200");
            var segs = path.GetSegments();
            Assert.Equal(2, segs.Count);
            Assert.Equal(PathSegmentType.LineTo, segs[1].Type);
            Assert.Equal(100f, segs[1].X);
            Assert.Equal(200f, segs[1].Y);
        }

        [Fact]
        public void Parse_LineTo_Relative()
        {
            var path = SvgPathParser.Parse("M 10 10 l 50 50");
            var segs = path.GetSegments();
            Assert.Equal(2, segs.Count);
            Assert.Equal(60f, segs[1].X);
            Assert.Equal(60f, segs[1].Y);
        }

        [Fact]
        public void Parse_HorizontalLine_Absolute()
        {
            var path = SvgPathParser.Parse("M 0 10 H 50");
            var segs = path.GetSegments();
            Assert.Equal(2, segs.Count);
            Assert.Equal(50f, segs[1].X);
            Assert.Equal(10f, segs[1].Y);
        }

        [Fact]
        public void Parse_VerticalLine_Absolute()
        {
            var path = SvgPathParser.Parse("M 10 0 V 50");
            var segs = path.GetSegments();
            Assert.Equal(2, segs.Count);
            Assert.Equal(10f, segs[1].X);
            Assert.Equal(50f, segs[1].Y);
        }

        [Fact]
        public void Parse_CubicBezier_Absolute()
        {
            var path = SvgPathParser.Parse("M 0 0 C 10 20 30 40 50 60");
            var segs = path.GetSegments();
            Assert.Equal(2, segs.Count);
            Assert.Equal(PathSegmentType.CubicBezierTo, segs[1].Type);
            Assert.Equal(10f, segs[1].X1);
            Assert.Equal(20f, segs[1].Y1);
            Assert.Equal(30f, segs[1].X2);
            Assert.Equal(40f, segs[1].Y2);
            Assert.Equal(50f, segs[1].X);
            Assert.Equal(60f, segs[1].Y);
        }

        [Fact]
        public void Parse_SmoothCubic_ReflectsControlPoint()
        {
            var path = SvgPathParser.Parse("M 0 0 C 10 20 30 40 50 60 S 70 80 90 100");
            var segs = path.GetSegments();
            Assert.Equal(3, segs.Count);
            Assert.Equal(PathSegmentType.CubicBezierTo, segs[2].Type);
            // Reflected control point: 2*50 - 30 = 70, 2*60 - 40 = 80
            Assert.Equal(70f, segs[2].X1);
            Assert.Equal(80f, segs[2].Y1);
            Assert.Equal(90f, segs[2].X);
            Assert.Equal(100f, segs[2].Y);
        }

        [Fact]
        public void Parse_QuadraticBezier_Absolute()
        {
            var path = SvgPathParser.Parse("M 0 0 Q 50 50 100 0");
            var segs = path.GetSegments();
            Assert.Equal(2, segs.Count);
            Assert.Equal(PathSegmentType.QuadraticBezierTo, segs[1].Type);
            Assert.Equal(50f, segs[1].X1);
            Assert.Equal(50f, segs[1].Y1);
            Assert.Equal(100f, segs[1].X);
            Assert.Equal(0f, segs[1].Y);
        }

        [Fact]
        public void Parse_SmoothQuadratic_ReflectsControlPoint()
        {
            var path = SvgPathParser.Parse("M 0 0 Q 50 50 100 0 T 200 0");
            var segs = path.GetSegments();
            Assert.Equal(3, segs.Count);
            Assert.Equal(PathSegmentType.QuadraticBezierTo, segs[2].Type);
            // Reflected: 2*100 - 50 = 150, 2*0 - 50 = -50
            Assert.Equal(150f, segs[2].X1);
            Assert.Equal(-50f, segs[2].Y1);
            Assert.Equal(200f, segs[2].X);
        }

        [Fact]
        public void Parse_Close()
        {
            var path = SvgPathParser.Parse("M 0 0 L 10 0 L 10 10 Z");
            var segs = path.GetSegments();
            Assert.Equal(4, segs.Count);
            Assert.Equal(PathSegmentType.Close, segs[3].Type);
        }

        [Fact]
        public void Parse_Arc_Absolute()
        {
            var path = SvgPathParser.Parse("M 10 80 A 45 45 0 0 0 125 125");
            var segs = path.GetSegments();
            Assert.True(segs.Count >= 2); // MoveTo + at least one Bezier
            Assert.Equal(PathSegmentType.MoveTo, segs[0].Type);
            // Arc should be converted to cubic bezier(s)
            Assert.Equal(PathSegmentType.CubicBezierTo, segs[1].Type);
            // Last point should be approximately (125, 125)
            var last = segs[segs.Count - 1];
            Assert.InRange(last.X, 124f, 126f);
            Assert.InRange(last.Y, 124f, 126f);
        }

        [Fact]
        public void Parse_ImplicitLineTo_AfterMoveTo()
        {
            // After M, subsequent coordinate pairs are treated as L
            var path = SvgPathParser.Parse("M 0 0 10 10 20 20");
            var segs = path.GetSegments();
            Assert.Equal(3, segs.Count);
            Assert.Equal(PathSegmentType.MoveTo, segs[0].Type);
            Assert.Equal(PathSegmentType.LineTo, segs[1].Type);
            Assert.Equal(PathSegmentType.LineTo, segs[2].Type);
        }

        [Fact]
        public void Parse_CompactNotation_NoSpaces()
        {
            // SVG allows coordinates without spaces when sign changes
            var path = SvgPathParser.Parse("M0,0L10,20L30,40Z");
            var segs = path.GetSegments();
            Assert.Equal(4, segs.Count);
            Assert.Equal(10f, segs[1].X);
            Assert.Equal(20f, segs[1].Y);
        }

        [Fact]
        public void Parse_EmptyString_ReturnsEmptyPath()
        {
            var path = SvgPathParser.Parse("");
            Assert.Empty(path.GetSegments());
        }

        [Fact]
        public void Parse_Triangle_Path()
        {
            var path = SvgPathParser.Parse("M 150 0 L 75 200 L 225 200 Z");
            var segs = path.GetSegments();
            Assert.Equal(4, segs.Count);
            Assert.Equal(150f, segs[0].X);
            Assert.Equal(0f, segs[0].Y);
            Assert.Equal(75f, segs[1].X);
            Assert.Equal(200f, segs[1].Y);
            Assert.Equal(225f, segs[2].X);
            Assert.Equal(200f, segs[2].Y);
            Assert.Equal(PathSegmentType.Close, segs[3].Type);
        }

        [Fact]
        public void Parse_RelativeCubic()
        {
            var path = SvgPathParser.Parse("M 10 10 c 10 20 30 40 50 60");
            var segs = path.GetSegments();
            Assert.Equal(2, segs.Count);
            Assert.Equal(PathSegmentType.CubicBezierTo, segs[1].Type);
            // Relative: control1 = (10+10, 10+20) = (20,30)
            Assert.Equal(20f, segs[1].X1);
            Assert.Equal(30f, segs[1].Y1);
            // End = (10+50, 10+60) = (60, 70)
            Assert.Equal(60f, segs[1].X);
            Assert.Equal(70f, segs[1].Y);
        }
    }
}
