namespace Rend.Css.Properties.Internal
{
    /// <summary>
    /// Encodes and decodes deferred CSS percentage values that must be resolved at layout time.
    /// Percentages are stored as sentinel float values using a large negative offset (-10000)
    /// so they don't collide with real negative pixel values (e.g., top: -0.5px).
    ///
    /// Encoding: percentage fraction (0.5 for 50%) → -(10000 + 0.5) = -10000.5
    /// Decoding: -10000.5 → fraction = 0.5, resolved = 0.5 * containingBlockDimension
    /// </summary>
    internal static class DeferredPercent
    {
        /// <summary>
        /// The offset used to distinguish deferred percentages from real negative values.
        /// Any value less than or equal to -Offset is a deferred percentage (unless it's
        /// NegativeInfinity for calc or a SizingKeyword sentinel).
        /// </summary>
        private const float Offset = 10000f;

        /// <summary>
        /// Threshold for detecting deferred percentages. Values at or below this
        /// are deferred percentages (after excluding NaN, NegativeInfinity, and SizingKeywords).
        /// </summary>
        private const float Threshold = -Offset;

        /// <summary>
        /// Encode a percentage fraction as a deferred sentinel value.
        /// E.g., 50% → fraction 0.5 → encoded as -10000.5
        /// </summary>
        public static float Encode(float fraction)
        {
            return -(Offset + fraction);
        }

        /// <summary>
        /// Returns true if the value is a deferred percentage sentinel.
        /// </summary>
        public static bool IsEncoded(float value)
        {
            return value <= Threshold
                && !float.IsNaN(value)
                && !float.IsNegativeInfinity(value)
                && !SizingKeyword.IsSizingKeyword(value);
        }

        /// <summary>
        /// Decode the percentage fraction from a sentinel value.
        /// E.g., -10000.5 → 0.5
        /// </summary>
        public static float DecodeFraction(float value)
        {
            return -(value + Offset);
        }

        /// <summary>
        /// Resolve a deferred percentage against a containing block dimension.
        /// E.g., -10000.5 resolved against 800px → 0.5 * 800 = 400px
        /// Returns the original value unchanged if it's not a deferred percentage.
        /// </summary>
        public static float Resolve(float value, float containingDimension)
        {
            if (IsEncoded(value))
            {
                return DecodeFraction(value) * containingDimension;
            }
            return value;
        }
    }
}
