using System;

namespace Rend.Html.Selectors.Internal
{
    /// <summary>
    /// CSS selector specificity (a, b, c) where:
    /// a = number of ID selectors
    /// b = number of class selectors, attribute selectors, and pseudo-classes
    /// c = number of type selectors and pseudo-elements
    /// </summary>
    internal readonly struct Specificity : IComparable<Specificity>
    {
        public readonly int A;
        public readonly int B;
        public readonly int C;

        public Specificity(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }

        public static Specificity operator +(Specificity left, Specificity right)
        {
            return new Specificity(left.A + right.A, left.B + right.B, left.C + right.C);
        }

        public int CompareTo(Specificity other)
        {
            int cmp = A.CompareTo(other.A);
            if (cmp != 0) return cmp;
            cmp = B.CompareTo(other.B);
            if (cmp != 0) return cmp;
            return C.CompareTo(other.C);
        }

        public override string ToString() => $"({A},{B},{C})";
    }
}
