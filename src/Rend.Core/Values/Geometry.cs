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
    }

    /// <summary>
    /// A 3x2 affine transformation matrix for 2D transforms.
    /// Layout: | M11 M12 |
    ///         | M21 M22 |
    ///         | M31 M32 | (translation)
    /// </summary>
    public readonly struct Matrix3x2 : IEquatable<Matrix3x2>
    {
        public float M11 { get; }
        public float M12 { get; }
        public float M21 { get; }
        public float M22 { get; }
        public float M31 { get; }
        public float M32 { get; }

        public Matrix3x2(float m11, float m12, float m21, float m22, float m31, float m32)
        {
            M11 = m11; M12 = m12;
            M21 = m21; M22 = m22;
            M31 = m31; M32 = m32;
        }

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

        public static Matrix3x2 operator *(Matrix3x2 a, Matrix3x2 b)
        {
            return new Matrix3x2(
                a.M11 * b.M11 + a.M12 * b.M21,
                a.M11 * b.M12 + a.M12 * b.M22,
                a.M21 * b.M11 + a.M22 * b.M21,
                a.M21 * b.M12 + a.M22 * b.M22,
                a.M31 * b.M11 + a.M32 * b.M21 + b.M31,
                a.M31 * b.M12 + a.M32 * b.M22 + b.M32
            );
        }

        public PointF TransformPoint(PointF point)
        {
            return new PointF(
                point.X * M11 + point.Y * M21 + M31,
                point.X * M12 + point.Y * M22 + M32
            );
        }

        public bool Equals(Matrix3x2 other)
            => M11 == other.M11 && M12 == other.M12
            && M21 == other.M21 && M22 == other.M22
            && M31 == other.M31 && M32 == other.M32;

        public override bool Equals(object? obj) => obj is Matrix3x2 other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(M11, M12, M21, M22, M31, M32);
        public static bool operator ==(Matrix3x2 left, Matrix3x2 right) => left.Equals(right);
        public static bool operator !=(Matrix3x2 left, Matrix3x2 right) => !left.Equals(right);
    }
}
