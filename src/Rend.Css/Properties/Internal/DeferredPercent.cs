namespace Rend.Css.Properties.Internal
{
    /// <summary>
    /// Encodes and decodes deferred CSS percentage values that must be resolved at layout time.
    /// Percentages are stored as sentinel float values using large negative offsets so they
    /// don't collide with real negative pixel values (e.g., top: -0.5px).
    ///
    /// Positive fractions: -(10000 + fraction) e.g., 50% → -10000.5
    /// Negative fractions: -(20000 + |fraction|) e.g., -50% → -20000.5
    /// </summary>
    internal static class DeferredPercent
    {
        private const float Offset = 10000f;
        private const float NegativeOffset = 20000f;
        private const float Threshold = -Offset;
        private const float NegativeThreshold = -NegativeOffset;

        /// <summary>
        /// Encode a percentage fraction as a deferred sentinel value.
        /// Positive fractions: -(10000 + fraction)
        /// Negative fractions: -(20000 + |fraction|)
        /// </summary>
        public static float Encode(float fraction)
        {
            if (fraction < 0)
            {
                return -(NegativeOffset + (-fraction));
            }
            return -(Offset + fraction);
        }

        /// <summary>
        /// Returns true if the value is a deferred percentage sentinel.
        /// </summary>
        public static bool IsEncoded(float value)
        {
            if (float.IsNaN(value) || float.IsNegativeInfinity(value))
            {
                return false;
            }
            if (SizingKeyword.IsSizingKeyword(value))
            {
                return false;
            }
            // Positive fraction range: <= -10000
            if (value <= Threshold && value > NegativeThreshold + 10)
            {
                return true;
            }
            // Negative fraction range: <= -20000
            if (value <= NegativeThreshold)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Decode the percentage fraction from a sentinel value.
        /// </summary>
        public static float DecodeFraction(float value)
        {
            if (value <= NegativeThreshold)
            {
                // Negative fraction: -(20000 + |fraction|) → -|fraction|
                return -(-(value + NegativeOffset));
            }
            // Positive fraction: -(10000 + fraction) → fraction
            return -(value + Offset);
        }

        /// <summary>
        /// Resolve a deferred percentage against a containing block dimension.
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
