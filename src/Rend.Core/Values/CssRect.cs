using System;

namespace Rend.Core.Values
{
    /// <summary>
    /// Represents four-sided CSS values (margin, padding, border-width, etc.).
    /// </summary>
    public readonly struct CssRect<T> : IEquatable<CssRect<T>> where T : IEquatable<T>
    {
        public T Top { get; }
        public T Right { get; }
        public T Bottom { get; }
        public T Left { get; }

        public CssRect(T top, T right, T bottom, T left)
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
        }

        /// <summary>All four sides set to the same value.</summary>
        public CssRect(T all) : this(all, all, all, all) { }

        /// <summary>Top/bottom and left/right pairs.</summary>
        public CssRect(T topBottom, T leftRight) : this(topBottom, leftRight, topBottom, leftRight) { }

        /// <summary>Top, left/right, bottom.</summary>
        public CssRect(T top, T leftRight, T bottom) : this(top, leftRight, bottom, leftRight) { }

        public bool Equals(CssRect<T> other)
            => Top.Equals(other.Top) && Right.Equals(other.Right)
            && Bottom.Equals(other.Bottom) && Left.Equals(other.Left);

        public override bool Equals(object? obj) => obj is CssRect<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Top, Right, Bottom, Left);
        public static bool operator ==(CssRect<T> left, CssRect<T> right) => left.Equals(right);
        public static bool operator !=(CssRect<T> left, CssRect<T> right) => !left.Equals(right);
        public override string ToString() => $"[{Top}, {Right}, {Bottom}, {Left}]";
    }

    /// <summary>
    /// A four-sided float rect (resolved margin/padding/border in px).
    /// </summary>
    public readonly struct CssEdges : IEquatable<CssEdges>
    {
        public float Top { get; }
        public float Right { get; }
        public float Bottom { get; }
        public float Left { get; }

        public CssEdges(float top, float right, float bottom, float left)
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
        }

        public CssEdges(float all) : this(all, all, all, all) { }
        public CssEdges(float topBottom, float leftRight) : this(topBottom, leftRight, topBottom, leftRight) { }

        public static readonly CssEdges Zero = new CssEdges(0);

        /// <summary>Total horizontal extent (left + right).</summary>
        public float Horizontal => Left + Right;

        /// <summary>Total vertical extent (top + bottom).</summary>
        public float Vertical => Top + Bottom;

        public bool Equals(CssEdges other)
            => Top == other.Top && Right == other.Right && Bottom == other.Bottom && Left == other.Left;

        public override bool Equals(object? obj) => obj is CssEdges other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Top, Right, Bottom, Left);
        public static bool operator ==(CssEdges left, CssEdges right) => left.Equals(right);
        public static bool operator !=(CssEdges left, CssEdges right) => !left.Equals(right);
    }
}
