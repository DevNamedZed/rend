namespace Rend.Css.Properties.Internal
{
    /// <summary>
    /// Sentinel float values for CSS intrinsic sizing keywords (min-content, max-content, fit-content).
    /// These are stored in properties that normally hold length values (e.g., width, height).
    /// </summary>
    internal static class SizingKeyword
    {
        /// <summary>width: min-content — use the minimum content width.</summary>
        public const float MinContent = -1e+30f;

        /// <summary>width: max-content — use the maximum content width (no line breaking).</summary>
        public const float MaxContent = -2e+30f;

        /// <summary>width: fit-content — clamp(min-content, available width, max-content).</summary>
        public const float FitContent = -3e+30f;

        /// <summary>
        /// Returns true if the value is a sizing keyword sentinel.
        /// </summary>
        public static bool IsSizingKeyword(float value)
        {
            return value == MinContent || value == MaxContent || value == FitContent;
        }
    }
}
