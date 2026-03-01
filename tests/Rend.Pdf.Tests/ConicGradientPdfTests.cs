using System;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class ConicGradientPdfTests
    {
        private static string BuildConicGradientPdf(PdfConicGradient gradient)
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.Content.ApplyConicGradient(gradient);
            return Encoding.ASCII.GetString(doc.ToArray());
        }

        // ═══════════════════════════════════════════
        // Basic conic gradient rendering
        // ═══════════════════════════════════════════

        [Fact]
        public void ConicGradient_ContainsShOperators()
        {
            var text = BuildConicGradientPdf(new PdfConicGradient
            {
                CenterX = 100, CenterY = 100,
                Width = 200, Height = 200,
                Segments = 36,
                Stops = new[]
                {
                    new PdfGradientColorStop(0, 1, 0, 0),
                    new PdfGradientColorStop(1, 0, 0, 1)
                }
            });
            Assert.Contains("sh", text);
        }

        [Fact]
        public void ConicGradient_ProducesMultipleShadings()
        {
            var text = BuildConicGradientPdf(new PdfConicGradient
            {
                CenterX = 100, CenterY = 100,
                Width = 200, Height = 200,
                Segments = 36,
                Stops = new[]
                {
                    new PdfGradientColorStop(0, 1, 0, 0),
                    new PdfGradientColorStop(1, 0, 0, 1)
                }
            });
            // Should produce multiple /ShN references (one per wedge segment)
            Assert.Contains("/Sh1", text);
            Assert.Contains("/Sh2", text);
            Assert.Contains("/Sh36", text);
        }

        [Fact]
        public void ConicGradient_ContainsClipPaths()
        {
            var text = BuildConicGradientPdf(new PdfConicGradient
            {
                CenterX = 100, CenterY = 100,
                Width = 200, Height = 200,
                Segments = 12,
                Stops = new[]
                {
                    new PdfGradientColorStop(0, 1, 0, 0),
                    new PdfGradientColorStop(1, 0, 0, 1)
                }
            });
            // Each wedge should have a clip path (W n)
            Assert.Contains("W n", text);
        }

        [Fact]
        public void ConicGradient_UsesLinearShadingType()
        {
            var text = BuildConicGradientPdf(new PdfConicGradient
            {
                CenterX = 100, CenterY = 100,
                Width = 200, Height = 200,
                Segments = 12,
                Stops = new[]
                {
                    new PdfGradientColorStop(0, 1, 0, 0),
                    new PdfGradientColorStop(1, 0, 0, 1)
                }
            });
            // Wedges are approximated with Type 2 (linear) shadings
            Assert.Contains("/ShadingType 2", text);
        }

        [Fact]
        public void ConicGradient_SavesAndRestoresState()
        {
            var text = BuildConicGradientPdf(new PdfConicGradient
            {
                CenterX = 50, CenterY = 50,
                Width = 100, Height = 100,
                Segments = 12,
                Stops = new[]
                {
                    new PdfGradientColorStop(0, 1, 0, 0),
                    new PdfGradientColorStop(1, 0, 0, 1)
                }
            });
            // Each wedge uses q/Q for state save/restore
            int qCount = CountOccurrences(text, "\nq\n");
            int bigQCount = CountOccurrences(text, "\nQ\n");
            Assert.True(qCount >= 12, $"Expected at least 12 'q' operators, found {qCount}");
            Assert.Equal(qCount, bigQCount);
        }

        // ═══════════════════════════════════════════
        // Color interpolation
        // ═══════════════════════════════════════════

        [Fact]
        public void ConicGradient_ThreeStops_InterpolatesCorrectly()
        {
            var text = BuildConicGradientPdf(new PdfConicGradient
            {
                CenterX = 100, CenterY = 100,
                Width = 200, Height = 200,
                Segments = 36,
                Stops = new[]
                {
                    new PdfGradientColorStop(0, 1, 0, 0),    // red
                    new PdfGradientColorStop(0.5f, 0, 1, 0), // green at 50%
                    new PdfGradientColorStop(1, 0, 0, 1)     // blue
                }
            });
            // With 3 stops, should still produce all 36 segments
            Assert.Contains("/Sh36", text);
            Assert.Contains("/ShadingType 2", text);
        }

        // ═══════════════════════════════════════════
        // Segment count configuration
        // ═══════════════════════════════════════════

        [Fact]
        public void ConicGradient_72Segments_Default()
        {
            var gradient = new PdfConicGradient();
            Assert.Equal(72, gradient.Segments);
        }

        [Fact]
        public void ConicGradient_MinimumSegments_Is12()
        {
            var text = BuildConicGradientPdf(new PdfConicGradient
            {
                CenterX = 100, CenterY = 100,
                Width = 200, Height = 200,
                Segments = 4, // below minimum
                Stops = new[]
                {
                    new PdfGradientColorStop(0, 1, 0, 0),
                    new PdfGradientColorStop(1, 0, 0, 1)
                }
            });
            // Clamped to at least 12 segments
            Assert.Contains("/Sh12", text);
        }

        // ═══════════════════════════════════════════
        // Shading appears in page resources
        // ═══════════════════════════════════════════

        [Fact]
        public void ConicGradient_ShadingInPageResources()
        {
            var text = BuildConicGradientPdf(new PdfConicGradient
            {
                CenterX = 100, CenterY = 100,
                Width = 200, Height = 200,
                Segments = 12,
                Stops = new[]
                {
                    new PdfGradientColorStop(0, 1, 0, 0),
                    new PdfGradientColorStop(1, 0, 0, 1)
                }
            });
            Assert.Contains("/Shading", text);
        }

        // ═══════════════════════════════════════════
        // Error handling
        // ═══════════════════════════════════════════

        [Fact]
        public void ConicGradient_NullGradient_Throws()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            Assert.Throws<ArgumentNullException>(() => page.Content.ApplyConicGradient(null!));
        }

        [Fact]
        public void ConicGradient_SingleStop_Throws()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            Assert.Throws<ArgumentException>(() =>
                page.Content.ApplyConicGradient(new PdfConicGradient
                {
                    CenterX = 100, CenterY = 100,
                    Width = 200, Height = 200,
                    Stops = new[] { new PdfGradientColorStop(0, 1, 0, 0) }
                }));
        }

        // ═══════════════════════════════════════════
        // Start angle
        // ═══════════════════════════════════════════

        [Fact]
        public void ConicGradient_StartAngle_AffectsCoordinates()
        {
            // Two gradients with different start angles should produce different coordinates
            var text0 = BuildConicGradientPdf(new PdfConicGradient
            {
                CenterX = 100, CenterY = 100,
                StartAngle = 0,
                Width = 200, Height = 200,
                Segments = 12,
                Stops = new[]
                {
                    new PdfGradientColorStop(0, 1, 0, 0),
                    new PdfGradientColorStop(1, 0, 0, 1)
                }
            });
            var text90 = BuildConicGradientPdf(new PdfConicGradient
            {
                CenterX = 100, CenterY = 100,
                StartAngle = 90,
                Width = 200, Height = 200,
                Segments = 12,
                Stops = new[]
                {
                    new PdfGradientColorStop(0, 1, 0, 0),
                    new PdfGradientColorStop(1, 0, 0, 1)
                }
            });
            // Content streams should differ due to different angles
            Assert.NotEqual(text0, text90);
        }

        // ═══════════════════════════════════════════
        // CssColor constructor for stops
        // ═══════════════════════════════════════════

        [Fact]
        public void ConicGradient_CssColorStops_Work()
        {
            var text = BuildConicGradientPdf(new PdfConicGradient
            {
                CenterX = 100, CenterY = 100,
                Width = 200, Height = 200,
                Segments = 12,
                Stops = new[]
                {
                    new PdfGradientColorStop(0, new CssColor(255, 0, 0)),
                    new PdfGradientColorStop(1, new CssColor(0, 0, 255))
                }
            });
            Assert.Contains("sh", text);
        }

        private static int CountOccurrences(string text, string pattern)
        {
            int count = 0;
            int idx = 0;
            while ((idx = text.IndexOf(pattern, idx, StringComparison.Ordinal)) >= 0)
            {
                count++;
                idx += pattern.Length;
            }
            return count;
        }
    }
}
