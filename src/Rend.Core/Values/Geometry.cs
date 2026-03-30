using System;
using System.Runtime.CompilerServices;

namespace Rend.Core.Values
{
    /// <summary>
    /// A 2D point in float coordinates.
    /// </summary>
    public readonly struct PointF : IEquatable<PointF>
    {
        public float X { get; }
        public float Y { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointF(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static readonly PointF Zero = new PointF(0f, 0f);

        public bool Equals(PointF other) => X == other.X && Y == other.Y;
        public override bool Equals(object? obj) => obj is PointF other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public static bool operator ==(PointF left, PointF right) => left.Equals(right);
        public static bool operator !=(PointF left, PointF right) => !left.Equals(right);
        public override string ToString() => $"({X}, {Y})";
    }

    /// <summary>
    /// A 2D size in float coordinates.
    /// </summary>
    public readonly struct SizeF : IEquatable<SizeF>
    {
        public float Width { get; }
        public float Height { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizeF(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public static readonly SizeF Zero = new SizeF(0f, 0f);

        public bool Equals(SizeF other) => Width == other.Width && Height == other.Height;
        public override bool Equals(object? obj) => obj is SizeF other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Width, Height);
        public static bool operator ==(SizeF left, SizeF right) => left.Equals(right);
        public static bool operator !=(SizeF left, SizeF right) => !left.Equals(right);
        public override string ToString() => $"{Width}x{Height}";
    }

    /// <summary>
    /// An axis-aligned rectangle in float coordinates.
    /// </summary>
    public readonly struct RectF : IEquatable<RectF>
    {
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RectF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float Left
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => X;
        }

        public float Top
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Y;
        }

        public float Right
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => X + Width;
        }

        public float Bottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Y + Height;
        }

        public static readonly RectF Empty = new RectF(0f, 0f, 0f, 0f);

        public bool Contains(float x, float y)
            => x >= X && x <= X + Width && y >= Y && y <= Y + Height;

        public bool Contains(PointF point)
            => Contains(point.X, point.Y);

        public bool Equals(RectF other)
            => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

        public override bool Equals(object? obj) => obj is RectF other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
        public static bool operator ==(RectF left, RectF right) => left.Equals(right);
        public static bool operator !=(RectF left, RectF right) => !left.Equals(right);
        public override string ToString() => $"[{X}, {Y}, {Width}, {Height}]";

        /// <summary>
        /// Snap rect edges to integer pixel boundaries, matching Chrome's PixelSnappedIntRect.
        /// Each edge is rounded independently; width/height derived from snapped edges.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RectF PixelSnap()
        {
            // Chrome uses roundf() which rounds half away from zero.
            // C# Math.Round defaults to banker's rounding (round to even), so we
            // must specify AwayFromZero to match Chrome's PixelSnappedIntRect.
            float snappedLeft = (float)Math.Round(X, MidpointRounding.AwayFromZero);
            float snappedTop = (float)Math.Round(Y, MidpointRounding.AwayFromZero);
            float snappedRight = (float)Math.Round(X + Width, MidpointRounding.AwayFromZero);
            float snappedBottom = (float)Math.Round(Y + Height, MidpointRounding.AwayFromZero);
            return new RectF(snappedLeft, snappedTop,
                             snappedRight - snappedLeft, snappedBottom - snappedTop);
        }
    }

    /// <summary>
    /// A 3x2 affine transformation matrix for 2D transforms.
    /// Layout: | M11 M12 |
    ///         | M21 M22 |
    ///         | M31 M32 | (translation)
    /// </summary>
    /// <summary>
    /// 3x3 affine transform matrix with optional perspective components.
    /// Named Matrix3x2 for historical compatibility with the 2D-only API.
    /// Layout: [M11 M12] [M21 M22] [M31 M32] + [Persp0 Persp1 Persp2]
    /// </summary>
    public readonly struct Matrix3x2 : IEquatable<Matrix3x2>
    {
        public float M11 { get; }
        public float M12 { get; }
        public float M21 { get; }
        public float M22 { get; }
        public float M31 { get; }
        public float M32 { get; }

        /// <summary>Perspective row: Persp0, Persp1, Persp2 (default 0, 0, 1).</summary>
        public float Persp0 { get; }
        public float Persp1 { get; }
        public float Persp2 { get; }

        public Matrix3x2(float m11, float m12, float m21, float m22, float m31, float m32)
        {
            M11 = m11; M12 = m12;
            M21 = m21; M22 = m22;
            M31 = m31; M32 = m32;
            Persp0 = 0f; Persp1 = 0f; Persp2 = 1f;
        }

        public Matrix3x2(float m11, float m12, float m21, float m22, float m31, float m32,
            float persp0, float persp1, float persp2)
        {
            M11 = m11; M12 = m12;
            M21 = m21; M22 = m22;
            M31 = m31; M32 = m32;
            Persp0 = persp0; Persp1 = persp1; Persp2 = persp2;
        }

        public bool HasPerspective => Persp0 != 0f || Persp1 != 0f || Persp2 != 1f;

        public static readonly Matrix3x2 Identity = new Matrix3x2(1, 0, 0, 1, 0, 0);

        public static Matrix3x2 CreateTranslation(float x, float y)
            => new Matrix3x2(1, 0, 0, 1, x, y);

        public static Matrix3x2 CreateScale(float sx, float sy)
            => new Matrix3x2(sx, 0, 0, sy, 0, 0);

        public static Matrix3x2 CreateRotation(float angleRadians)
        {
            float cos = (float)Math.Cos(angleRadians);
            float sin = (float)Math.Sin(angleRadians);
            return new Matrix3x2(cos, sin, -sin, cos, 0, 0);
        }

        public static Matrix3x2 CreateSkew(float angleXRadians, float angleYRadians)
        {
            float tanX = (float)Math.Tan(angleXRadians);
            float tanY = (float)Math.Tan(angleYRadians);
            return new Matrix3x2(1, tanY, tanX, 1, 0, 0);
        }

        /// <summary>
        /// [CSS-TRANSFORM2 §7] Flatten a 4x4 matrix by dropping Z row and column.
        /// </summary>
        public static Matrix3x2 FromMatrix4x4(System.Numerics.Matrix4x4 m)
        {
            return new Matrix3x2(
                m.M11, m.M12,
                m.M21, m.M22,
                m.M41, m.M42,
                m.M14, m.M24, m.M44);
        }

        /// <summary>
        /// Full 3x3 matrix multiplication (supports perspective).
        /// </summary>
        public static Matrix3x2 operator *(Matrix3x2 a, Matrix3x2 b)
        {
            if (!a.HasPerspective && !b.HasPerspective)
            {
                // Fast path: pure 2D affine (original logic)
                return new Matrix3x2(
                    a.M11 * b.M11 + a.M12 * b.M21,
                    a.M11 * b.M12 + a.M12 * b.M22,
                    a.M21 * b.M11 + a.M22 * b.M21,
                    a.M21 * b.M12 + a.M22 * b.M22,
                    a.M31 * b.M11 + a.M32 * b.M21 + b.M31,
                    a.M31 * b.M12 + a.M32 * b.M22 + b.M32
                );
            }

            // Full 3x3 multiply for perspective transforms
            // Row 0: [a.M11  a.M21  a.M31] · cols of b (transposed layout)
            // Actually, our layout is:
            // | M11  M21  M31 |    (note: M21 is SkewX, M12 is SkewY)
            // | M12  M22  M32 |
            // | P0   P1   P2  |
            return new Matrix3x2(
                a.M11 * b.M11 + a.M21 * b.M12 + a.M31 * b.Persp0,
                a.M12 * b.M11 + a.M22 * b.M12 + a.M32 * b.Persp0,
                a.M11 * b.M21 + a.M21 * b.M22 + a.M31 * b.Persp1,
                a.M12 * b.M21 + a.M22 * b.M22 + a.M32 * b.Persp1,
                a.M11 * b.M31 + a.M21 * b.M32 + a.M31 * b.Persp2,
                a.M12 * b.M31 + a.M22 * b.M32 + a.M32 * b.Persp2,
                a.Persp0 * b.M11 + a.Persp1 * b.M12 + a.Persp2 * b.Persp0,
                a.Persp0 * b.M21 + a.Persp1 * b.M22 + a.Persp2 * b.Persp1,
                a.Persp0 * b.M31 + a.Persp1 * b.M32 + a.Persp2 * b.Persp2
            );
        }

        public PointF TransformPoint(PointF point)
        {
            float x = point.X * M11 + point.Y * M21 + M31;
            float y = point.X * M12 + point.Y * M22 + M32;
            if (HasPerspective)
            {
                float w = point.X * Persp0 + point.Y * Persp1 + Persp2;
                if (w != 0f && w != 1f)
                {
                    x /= w;
                    y /= w;
                }
            }
            return new PointF(x, y);
        }

        public bool Equals(Matrix3x2 other)
            => M11 == other.M11 && M12 == other.M12
            && M21 == other.M21 && M22 == other.M22
            && M31 == other.M31 && M32 == other.M32
            && Persp0 == other.Persp0 && Persp1 == other.Persp1 && Persp2 == other.Persp2;

        public override bool Equals(object? obj) => obj is Matrix3x2 other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(M11, M12, M21, M22, M31, M32);
        public static bool operator ==(Matrix3x2 left, Matrix3x2 right) => left.Equals(right);
        public static bool operator !=(Matrix3x2 left, Matrix3x2 right) => !left.Equals(right);
    }
}
