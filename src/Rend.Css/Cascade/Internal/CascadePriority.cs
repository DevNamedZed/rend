using System;

namespace Rend.Css.Cascade.Internal
{
    /// <summary>
    /// The full priority of a CSS declaration in the cascade.
    /// Sorted by: origin/importance → specificity → source order.
    /// </summary>
    internal readonly struct CascadePriority : IComparable<CascadePriority>
    {
        /// <summary>
        /// Cascade level derived from origin + importance.
        /// Higher = wins. Per CSS spec:
        /// 0: user-agent normal
        /// 1: user normal
        /// 2: author normal
        /// 3: author !important
        /// 4: user !important
        /// 5: user-agent !important
        /// </summary>
        public int Level { get; }

        /// <summary>The specificity of the selector.</summary>
        public CssSpecificity Specificity { get; }

        /// <summary>Source order (later = higher).</summary>
        public int SourceOrder { get; }

        public CascadePriority(CascadeOrigin origin, bool important, CssSpecificity specificity, int sourceOrder)
        {
            Level = GetLevel(origin, important);
            Specificity = specificity;
            SourceOrder = sourceOrder;
        }

        private static int GetLevel(CascadeOrigin origin, bool important)
        {
            if (!important)
            {
                return (int)origin; // 0=UA, 1=User, 2=Author
            }
            // Important rules reverse the origin order
            switch (origin)
            {
                case CascadeOrigin.Author: return 3;
                case CascadeOrigin.User: return 4;
                case CascadeOrigin.UserAgent: return 5;
                default: return 3;
            }
        }

        public int CompareTo(CascadePriority other)
        {
            if (Level != other.Level) return Level.CompareTo(other.Level);
            int specCmp = Specificity.CompareTo(other.Specificity);
            if (specCmp != 0) return specCmp;
            return SourceOrder.CompareTo(other.SourceOrder);
        }
    }
}
