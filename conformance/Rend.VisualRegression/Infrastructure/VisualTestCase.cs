namespace Rend.VisualRegression.Infrastructure
{
    public sealed class VisualTestCase
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Html { get; set; } = "";
        public int ViewportWidth { get; set; } = 400;
        public int ViewportHeight { get; set; } = 300;
        /// <summary>
        /// Maximum allowed diff percentage for this test to pass (0.0 = exact match).
        /// </summary>
        public double Tolerance { get; set; } = 0.0;
        public override string ToString() => $"{Category}/{Name}";
    }
}
