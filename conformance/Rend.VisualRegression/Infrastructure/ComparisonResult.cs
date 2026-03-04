using System;

namespace Rend.VisualRegression.Infrastructure
{
    public enum ComparisonOutcome
    {
        Pass,
        Fail,
        /// <summary>
        /// Passes with 1px shift tolerance but has strict pixel differences.
        /// The rendering is close but has sub-pixel positioning differences.
        /// </summary>
        NearPass,
        Error
    }

    public sealed class ComparisonResult
    {
        public string TestId { get; set; } = "";
        public string TestName { get; set; } = "";
        public string Category { get; set; } = "";
        public ComparisonOutcome Outcome { get; set; }
        /// <summary>Strict diff (no shift tolerance).</summary>
        public double DiffPercentage { get; set; }
        /// <summary>Diff after applying 1px shift tolerance.</summary>
        public double ShiftTolerantDiffPercentage { get; set; }
        public int DiffPixels { get; set; }
        public int ShiftTolerantDiffPixels { get; set; }
        public int TotalPixels { get; set; }
        public string? ChromeImagePath { get; set; }
        public string? RendImagePath { get; set; }
        public string? DiffImagePath { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
