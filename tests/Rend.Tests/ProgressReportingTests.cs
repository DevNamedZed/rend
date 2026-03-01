using System;
using System.Collections.Generic;
using Xunit;

namespace Rend.Tests
{
    /// <summary>
    /// Synchronous IProgress implementation for testing (avoids SynchronizationContext issues).
    /// </summary>
    internal sealed class SyncProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;
        public SyncProgress(Action<T> handler) => _handler = handler;
        public void Report(T value) => _handler(value);
    }

    public class ProgressReportingTests
    {
        [Fact]
        public void ToPdf_WithProgress_ReportsStages()
        {
            var reports = new List<RenderProgress>();

            var options = new RenderOptions
            {
                Progress = new SyncProgress<RenderProgress>(p => reports.Add(p))
            };

            byte[] result;
            try
            {
                result = Render.ToPdf("<p>Hello</p>", options);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return; // Skip on environments without native binaries
            }

            // Should have received progress reports
            Assert.True(reports.Count >= 5, $"Expected at least 5 progress reports, got {reports.Count}");

            // First report should be at Parsing stage
            Assert.Equal(RenderStage.Parsing, reports[0].Stage);

            // Last report should be at 100% Finishing
            var last = reports[reports.Count - 1];
            Assert.Equal(RenderStage.Finishing, last.Stage);
            Assert.Equal(100, last.Percentage);

            // Percentages should be non-decreasing
            for (int i = 1; i < reports.Count; i++)
            {
                Assert.True(reports[i].Percentage >= reports[i - 1].Percentage,
                    $"Progress went backwards at step {i}: {reports[i - 1].Percentage} → {reports[i].Percentage}");
            }
        }

        [Fact]
        public void ToPdf_WithoutProgress_NoError()
        {
            try
            {
                var result = Render.ToPdf("<p>Test</p>");
                Assert.NotNull(result);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }
        }

        [Fact]
        public void RenderProgress_Properties()
        {
            var progress = new RenderProgress(50, RenderStage.Layout, "Computing layout");
            Assert.Equal(50, progress.Percentage);
            Assert.Equal(RenderStage.Layout, progress.Stage);
            Assert.Equal("Computing layout", progress.Description);
        }

        [Fact]
        public void RenderStage_AllValues()
        {
            // Verify all stages are distinct
            var stages = new[] { RenderStage.Parsing, RenderStage.Styling, RenderStage.Layout,
                                 RenderStage.Rendering, RenderStage.Finishing };
            var set = new HashSet<RenderStage>(stages);
            Assert.Equal(stages.Length, set.Count);
        }

        private static bool IsNativeLibraryFailure(Exception ex)
        {
            return ex is DllNotFoundException ||
                   ex is TypeInitializationException ||
                   (ex.InnerException != null && IsNativeLibraryFailure(ex.InnerException));
        }
    }
}
