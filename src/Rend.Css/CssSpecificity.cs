using System;

namespace Rend.Css
{
    /// <summary>
    /// CSS selector specificity: (inline, A, B, C) per CSS Selectors Level 4.
    /// Inline styles are highest. A = #ids, B = .classes/[attrs]/:pseudo-classes, C = type/::pseudo-elements.
    /// </summary>
    public readonly struct CssSpecificity : IComparable<CssSpecificity>, IEquatable<CssSpecificity>
    {
        public bool Inline { get; }
        public int A { get; }
        public int B { get; }
        public int C { get; }

        public CssSpecificity(int a, int b, int c, bool inline_ = false)
        {
            Inline = inline_;
            A = a;
            B = b;
            C = c;
        }

        public static readonly CssSpecificity Zero = new CssSpecificity(0, 0, 0);
        public static readonly CssSpecificity InlineStyle = new CssSpecificity(0, 0, 0, true);

        public int CompareTo(CssSpecificity other)
        {
            if (Inline != other.Inline) return Inline ? 1 : -1;
            if (A != other.A) return A.CompareTo(other.A);
            if (B != other.B) return B.CompareTo(other.B);
            return C.CompareTo(other.C);
        }

        public bool Equals(CssSpecificity other)
            => Inline == other.Inline && A == other.A && B == other.B && C == other.C;

        public override bool Equals(object? obj)
            => obj is CssSpecificity other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Inline, A, B, C);

        public static bool operator ==(CssSpecificity left, CssSpecificity right) => left.Equals(right);
        public static bool operator !=(CssSpecificity left, CssSpecificity right) => !left.Equals(right);
        public static bool operator <(CssSpecificity left, CssSpecificity right) => left.CompareTo(right) < 0;
        public static bool operator >(CssSpecificity left, CssSpecificity right) => left.CompareTo(right) > 0;
        public static bool operator <=(CssSpecificity left, CssSpecificity right) => left.CompareTo(right) <= 0;
        public static bool operator >=(CssSpecificity left, CssSpecificity right) => left.CompareTo(right) >= 0;

        public override string ToString() => Inline ? $"(inline,{A},{B},{C})" : $"({A},{B},{C})";
    }
}
