namespace Rend.VisualRegression.Infrastructure
{
    /// <summary>
    /// A parsed WPT <c>&lt;meta name="fuzzy"&gt;</c> tolerance directive.
    /// Represents the per-channel difference range and differing-pixel-count range
    /// that a reftest is allowed to produce while still being considered a pass.
    /// </summary>
    /// <remarks>
    /// See https://web-platform-tests.org/writing-tests/reftests.html#fuzzy-matching
    /// for the authoritative grammar.
    /// </remarks>
    public sealed class FuzzyTolerance
    {
        /// <summary>
        /// Minimum of the allowed max per-channel difference range (inclusive).
        /// </summary>
        public int MaxDifferenceMin { get; }

        /// <summary>
        /// Maximum of the allowed max per-channel difference range (inclusive).
        /// </summary>
        public int MaxDifferenceMax { get; }

        /// <summary>
        /// Minimum of the allowed differing-pixel count range (inclusive).
        /// </summary>
        public int TotalPixelsMin { get; }

        /// <summary>
        /// Maximum of the allowed differing-pixel count range (inclusive).
        /// </summary>
        public int TotalPixelsMax { get; }

        public FuzzyTolerance(int maxDifferenceMin, int maxDifferenceMax, int totalPixelsMin, int totalPixelsMax)
        {
            MaxDifferenceMin = maxDifferenceMin;
            MaxDifferenceMax = maxDifferenceMax;
            TotalPixelsMin = totalPixelsMin;
            TotalPixelsMax = totalPixelsMax;
        }

        /// <summary>
        /// Returns true when a measured rendering difference lies within the
        /// upper bounds of this tolerance. Lower bounds are intentionally not
        /// enforced — a rendering that is closer to the reference than the
        /// directive expected is never a regression in this project.
        /// </summary>
        /// <param name="diffPixels">Number of pixels that differ between expected and actual.</param>
        /// <param name="maxChannelDiff">Largest absolute per-channel delta observed across all pixels.</param>
        public bool Accepts(int diffPixels, int maxChannelDiff)
        {
            if (diffPixels > TotalPixelsMax)
            {
                return false;
            }
            if (maxChannelDiff > MaxDifferenceMax)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return $"maxDifference={MaxDifferenceMin}-{MaxDifferenceMax};totalPixels={TotalPixelsMin}-{TotalPixelsMax}";
        }
    }
}
