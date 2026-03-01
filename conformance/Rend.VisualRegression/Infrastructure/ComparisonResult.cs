using System;

namespace Rend.VisualRegression.Infrastructure
{
    public enum ComparisonOutcome
    {
        Pass,
        Fail,
        Error
    }

    public sealed class ComparisonResult
    {
        public string TestId { get; set; } = "";
        public string TestName { get; set; } = "";
        public string Category { get; set; } = "";
        public ComparisonOutcome Outcome { get; set; }
        public double DiffPercentage { get; set; }
        public int DiffPixels { get; set; }
        public int TotalPixels { get; set; }
        public string? ChromeImagePath { get; set; }
        public string? RendImagePath { get; set; }
        public string? DiffImagePath { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
