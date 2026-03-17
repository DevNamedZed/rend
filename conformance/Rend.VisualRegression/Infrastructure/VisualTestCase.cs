using System.Collections.Generic;

namespace Rend.VisualRegression.Infrastructure
{
    public sealed class VisualTestCase
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();
        public string Html { get; set; } = "";
        public int ViewportWidth { get; set; } = 400;
        public int ViewportHeight { get; set; } = 300;
        /// <summary>
        /// Diff percentage threshold. Tests with diff strictly below this pass.
        /// </summary>
        public double Tolerance { get; set; } = 0.01;
        public override string ToString() => $"{Category}/{Name}";
    }
}
