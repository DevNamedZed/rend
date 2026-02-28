using System;
using Rend.Core.Values;
using Xunit;

namespace Rend.Core.Tests
{
    public class PointFTests
    {
        [Fact]
        public void Constructor_SetsProperties()
        {
            var p = new PointF(3.5f, 7.2f);
            Assert.Equal(3.5f, p.X);
            Assert.Equal(7.2f, p.Y);
        }

        [Fact]
        public void Zero_IsOrigin()
        {
            Assert.Equal(0f, PointF.Zero.X);
            Assert.Equal(0f, PointF.Zero.Y);
        }

        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new PointF(1f, 2f);
            var b = new PointF(1f, 2f);
            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var a = new PointF(1f, 2f);
            var b = new PointF(1f, 3f);
            Assert.False(a.Equals(b));
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equals_DifferentX_ReturnsFalse()
        {
            var a = new PointF(1f, 2f);
            var b = new PointF(99f, 2f);
            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equals_BoxedObject_Works()
        {
            var a = new PointF(1f, 2f);
            object b = new PointF(1f, 2f);
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_NonPointFObject_ReturnsFalse()
        {
            var a = new PointF(1f, 2f);
            Assert.False(a.Equals("not a point"));
            Assert.False(a.Equals(null));
        }

        [Fact]
        public void GetHashCode_EqualPoints_SameHash()
        {
            var a = new PointF(5f, 10f);
            var b = new PointF(5f, 10f);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ToString_FormatsCorrectly()
        {
            var p = new PointF(1.5f, 2.5f);
            Assert.Equal("(1.5, 2.5)", p.ToString());
        }

        [Fact]
        public void NegativeCoordinates_Work()
        {
            var p = new PointF(-10f, -20f);
            Assert.Equal(-10f, p.X);
            Assert.Equal(-20f, p.Y);
        }
    }

    public class SizeFTests
    {
        [Fact]
        public void Constructor_SetsProperties()
        {
            var s = new SizeF(100f, 200f);
            Assert.Equal(100f, s.Width);
            Assert.Equal(200f, s.Height);
        }

        [Fact]
        public void Zero_HasZeroDimensions()
        {
            Assert.Equal(0f, SizeF.Zero.Width);
            Assert.Equal(0f, SizeF.Zero.Height);
        }

        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new SizeF(10f, 20f);
            var b = new SizeF(10f, 20f);
            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var a = new SizeF(10f, 20f);
            var b = new SizeF(10f, 30f);
            Assert.False(a.Equals(b));
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equals_DifferentWidth_ReturnsFalse()
        {
            var a = new SizeF(10f, 20f);
            var b = new SizeF(99f, 20f);
            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equals_BoxedObject_Works()
        {
            var a = new SizeF(10f, 20f);
            object b = new SizeF(10f, 20f);
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_NonSizeFObject_ReturnsFalse()
        {
            var a = new SizeF(10f, 20f);
            Assert.False(a.Equals("not a size"));
            Assert.False(a.Equals(null));
        }

        [Fact]
        public void GetHashCode_EqualSizes_SameHash()
        {
            var a = new SizeF(10f, 20f);
            var b = new SizeF(10f, 20f);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ToString_FormatsCorrectly()
        {
            var s = new SizeF(800f, 600f);
            Assert.Equal("800x600", s.ToString());
        }
    }

    public class RectFTests
    {
        [Fact]
        public void Constructor_SetsProperties()
        {
            var r = new RectF(10f, 20f, 300f, 400f);
            Assert.Equal(10f, r.X);
            Assert.Equal(20f, r.Y);
            Assert.Equal(300f, r.Width);
            Assert.Equal(400f, r.Height);
        }

        [Fact]
        public void EdgeProperties_ComputeCorrectly()
        {
            var r = new RectF(10f, 20f, 300f, 400f);
            Assert.Equal(10f, r.Left);
            Assert.Equal(20f, r.Top);
            Assert.Equal(310f, r.Right);
            Assert.Equal(420f, r.Bottom);
        }

        [Fact]
        public void Empty_HasZeroDimensions()
        {
            var e = RectF.Empty;
            Assert.Equal(0f, e.X);
            Assert.Equal(0f, e.Y);
            Assert.Equal(0f, e.Width);
            Assert.Equal(0f, e.Height);
        }

        [Theory]
        [InlineData(15f, 25f, true)]   // Inside
        [InlineData(10f, 20f, true)]   // Top-left corner
        [InlineData(310f, 420f, true)]  // Bottom-right corner
        [InlineData(9f, 25f, false)]    // Left of rect
        [InlineData(311f, 25f, false)]  // Right of rect
        [InlineData(15f, 19f, false)]   // Above rect
        [InlineData(15f, 421f, false)]  // Below rect
        public void Contains_Float_ReturnsExpected(float x, float y, bool expected)
        {
            var r = new RectF(10f, 20f, 300f, 400f);
            Assert.Equal(expected, r.Contains(x, y));
        }

        [Fact]
        public void Contains_Point_ReturnsExpected()
        {
            var r = new RectF(0f, 0f, 100f, 100f);
            Assert.True(r.Contains(new PointF(50f, 50f)));
            Assert.False(r.Contains(new PointF(101f, 50f)));
        }

        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new RectF(1f, 2f, 3f, 4f);
            var b = new RectF(1f, 2f, 3f, 4f);
            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var a = new RectF(1f, 2f, 3f, 4f);
            var b = new RectF(1f, 2f, 3f, 5f);
            Assert.False(a.Equals(b));
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equals_AllFieldsDiffer_ReturnsFalse()
        {
            var a = new RectF(1f, 2f, 3f, 4f);
            Assert.False(a.Equals(new RectF(99f, 2f, 3f, 4f)));
            Assert.False(a.Equals(new RectF(1f, 99f, 3f, 4f)));
            Assert.False(a.Equals(new RectF(1f, 2f, 99f, 4f)));
        }

        [Fact]
        public void Equals_BoxedObject_Works()
        {
            var a = new RectF(1f, 2f, 3f, 4f);
            object b = new RectF(1f, 2f, 3f, 4f);
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_NonRectFObject_ReturnsFalse()
        {
            var a = new RectF(1f, 2f, 3f, 4f);
            Assert.False(a.Equals("not a rect"));
            Assert.False(a.Equals(null));
        }

        [Fact]
        public void GetHashCode_EqualRects_SameHash()
        {
            var a = new RectF(1f, 2f, 3f, 4f);
            var b = new RectF(1f, 2f, 3f, 4f);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ToString_FormatsCorrectly()
        {
            var r = new RectF(1f, 2f, 3f, 4f);
            Assert.Equal("[1, 2, 3, 4]", r.ToString());
        }
    }

    public class Matrix3x2Tests
    {
        private const float Epsilon = 1e-5f;

        private static void AssertMatrixEqual(Matrix3x2 expected, Matrix3x2 actual)
        {
            Assert.Equal(expected.M11, actual.M11, Epsilon);
            Assert.Equal(expected.M12, actual.M12, Epsilon);
            Assert.Equal(expected.M21, actual.M21, Epsilon);
            Assert.Equal(expected.M22, actual.M22, Epsilon);
            Assert.Equal(expected.M31, actual.M31, Epsilon);
            Assert.Equal(expected.M32, actual.M32, Epsilon);
        }

        [Fact]
        public void Constructor_SetsAllFields()
        {
            var m = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
            Assert.Equal(1f, m.M11);
            Assert.Equal(2f, m.M12);
            Assert.Equal(3f, m.M21);
            Assert.Equal(4f, m.M22);
            Assert.Equal(5f, m.M31);
            Assert.Equal(6f, m.M32);
        }

        [Fact]
        public void Identity_IsCorrect()
        {
            var id = Matrix3x2.Identity;
            Assert.Equal(1f, id.M11);
            Assert.Equal(0f, id.M12);
            Assert.Equal(0f, id.M21);
            Assert.Equal(1f, id.M22);
            Assert.Equal(0f, id.M31);
            Assert.Equal(0f, id.M32);
        }

        [Fact]
        public void Identity_MultipliedByIdentity_IsIdentity()
        {
            var result = Matrix3x2.Identity * Matrix3x2.Identity;
            Assert.Equal(Matrix3x2.Identity, result);
        }

        [Fact]
        public void CreateTranslation_SetsTranslationComponents()
        {
            var t = Matrix3x2.CreateTranslation(10f, 20f);
            Assert.Equal(1f, t.M11);
            Assert.Equal(0f, t.M12);
            Assert.Equal(0f, t.M21);
            Assert.Equal(1f, t.M22);
            Assert.Equal(10f, t.M31);
            Assert.Equal(20f, t.M32);
        }

        [Fact]
        public void CreateScale_SetsScaleComponents()
        {
            var s = Matrix3x2.CreateScale(2f, 3f);
            Assert.Equal(2f, s.M11);
            Assert.Equal(0f, s.M12);
            Assert.Equal(0f, s.M21);
            Assert.Equal(3f, s.M22);
            Assert.Equal(0f, s.M31);
            Assert.Equal(0f, s.M32);
        }

        [Fact]
        public void CreateRotation_ZeroAngle_IsIdentity()
        {
            var r = Matrix3x2.CreateRotation(0f);
            AssertMatrixEqual(Matrix3x2.Identity, r);
        }

        [Fact]
        public void CreateRotation_90Degrees_IsCorrect()
        {
            float angle = (float)(Math.PI / 2.0);
            var r = Matrix3x2.CreateRotation(angle);
            // cos(90) = 0, sin(90) = 1
            // Expected: [0, 1, -1, 0, 0, 0]
            Assert.Equal(0f, r.M11, Epsilon);
            Assert.Equal(1f, r.M12, Epsilon);
            Assert.Equal(-1f, r.M21, Epsilon);
            Assert.Equal(0f, r.M22, Epsilon);
            Assert.Equal(0f, r.M31, Epsilon);
            Assert.Equal(0f, r.M32, Epsilon);
        }

        [Fact]
        public void CreateRotation_180Degrees_IsCorrect()
        {
            float angle = (float)Math.PI;
            var r = Matrix3x2.CreateRotation(angle);
            Assert.Equal(-1f, r.M11, Epsilon);
            Assert.Equal(0f, r.M12, Epsilon);
            Assert.Equal(0f, r.M21, Epsilon);
            Assert.Equal(-1f, r.M22, Epsilon);
        }

        [Fact]
        public void Multiply_TranslationThenScale_IsCorrect()
        {
            var t = Matrix3x2.CreateTranslation(10f, 20f);
            var s = Matrix3x2.CreateScale(2f, 3f);
            var result = t * s;
            // Translation * Scale:
            // M11 = 1*2 + 0*0 = 2, M12 = 1*0 + 0*3 = 0
            // M21 = 0*2 + 1*0 = 0, M22 = 0*0 + 1*3 = 3
            // M31 = 10*2 + 20*0 + 0 = 20, M32 = 10*0 + 20*3 + 0 = 60
            Assert.Equal(2f, result.M11, Epsilon);
            Assert.Equal(0f, result.M12, Epsilon);
            Assert.Equal(0f, result.M21, Epsilon);
            Assert.Equal(3f, result.M22, Epsilon);
            Assert.Equal(20f, result.M31, Epsilon);
            Assert.Equal(60f, result.M32, Epsilon);
        }

        [Fact]
        public void Multiply_ScaleThenTranslation_IsCorrect()
        {
            var s = Matrix3x2.CreateScale(2f, 3f);
            var t = Matrix3x2.CreateTranslation(10f, 20f);
            var result = s * t;
            // Scale * Translation:
            // M11 = 2*1 + 0*0 = 2, M12 = 2*0 + 0*1 = 0
            // M21 = 0*1 + 3*0 = 0, M22 = 0*0 + 3*1 = 3
            // M31 = 0*1 + 0*0 + 10 = 10, M32 = 0*0 + 0*1 + 20 = 20
            Assert.Equal(2f, result.M11, Epsilon);
            Assert.Equal(0f, result.M12, Epsilon);
            Assert.Equal(0f, result.M21, Epsilon);
            Assert.Equal(3f, result.M22, Epsilon);
            Assert.Equal(10f, result.M31, Epsilon);
            Assert.Equal(20f, result.M32, Epsilon);
        }

        [Fact]
        public void TransformPoint_WithIdentity_ReturnsSamePoint()
        {
            var p = new PointF(5f, 10f);
            var result = Matrix3x2.Identity.TransformPoint(p);
            Assert.Equal(5f, result.X, Epsilon);
            Assert.Equal(10f, result.Y, Epsilon);
        }

        [Fact]
        public void TransformPoint_WithTranslation_TranslatesPoint()
        {
            var t = Matrix3x2.CreateTranslation(100f, 200f);
            var result = t.TransformPoint(new PointF(5f, 10f));
            Assert.Equal(105f, result.X, Epsilon);
            Assert.Equal(210f, result.Y, Epsilon);
        }

        [Fact]
        public void TransformPoint_WithScale_ScalesPoint()
        {
            var s = Matrix3x2.CreateScale(2f, 3f);
            var result = s.TransformPoint(new PointF(5f, 10f));
            Assert.Equal(10f, result.X, Epsilon);
            Assert.Equal(30f, result.Y, Epsilon);
        }

        [Fact]
        public void TransformPoint_WithRotation90_RotatesPoint()
        {
            float angle = (float)(Math.PI / 2.0);
            var r = Matrix3x2.CreateRotation(angle);
            var result = r.TransformPoint(new PointF(1f, 0f));
            // (1,0) rotated 90 degrees -> (0,1)
            Assert.Equal(0f, result.X, Epsilon);
            Assert.Equal(1f, result.Y, Epsilon);
        }

        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
            var b = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var a = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
            var b = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 99f);
            Assert.False(a.Equals(b));
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equals_EachFieldDiffers_ReturnsFalse()
        {
            var baseline = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
            Assert.False(baseline.Equals(new Matrix3x2(99f, 2f, 3f, 4f, 5f, 6f)));
            Assert.False(baseline.Equals(new Matrix3x2(1f, 99f, 3f, 4f, 5f, 6f)));
            Assert.False(baseline.Equals(new Matrix3x2(1f, 2f, 99f, 4f, 5f, 6f)));
            Assert.False(baseline.Equals(new Matrix3x2(1f, 2f, 3f, 99f, 5f, 6f)));
            Assert.False(baseline.Equals(new Matrix3x2(1f, 2f, 3f, 4f, 99f, 6f)));
        }

        [Fact]
        public void Equals_BoxedObject_Works()
        {
            var a = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
            object b = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_NonMatrixObject_ReturnsFalse()
        {
            var a = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
            Assert.False(a.Equals("not a matrix"));
            Assert.False(a.Equals(null));
        }

        [Fact]
        public void GetHashCode_EqualMatrices_SameHash()
        {
            var a = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
            var b = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Multiply_AnyMatrixByIdentity_ReturnsSameMatrix()
        {
            var m = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
            var result = m * Matrix3x2.Identity;
            Assert.Equal(m, result);
        }

        [Fact]
        public void Multiply_IdentityByAnyMatrix_ReturnsSameMatrix()
        {
            var m = new Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f);
            var result = Matrix3x2.Identity * m;
            Assert.Equal(m, result);
        }
    }
}
