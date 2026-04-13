using System;
using System.Collections.Generic;
using System.IO;

namespace Rend.VisualRegression.Infrastructure
{
    public sealed class VisualTestCase
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();
        public int ViewportWidth { get; set; } = 400;
        public int ViewportHeight { get; set; } = 300;
        /// <summary>
        /// Diff percentage threshold. Tests with diff strictly below this pass.
        /// </summary>
        public double Tolerance { get; set; } = 0.01;

        /// <summary>
        /// Optional per-test fuzzy tolerance parsed from a WPT
        /// <c>&lt;meta name="fuzzy"&gt;</c> directive. When present, tests that
        /// exceed <see cref="Tolerance"/> can still pass if the measured
        /// difference fits within this tolerance's upper bounds.
        /// </summary>
        public FuzzyTolerance? Fuzzy { get; set; }

        /// <summary>
        /// Inline HTML for YAML-defined tests. Null for file-backed tests.
        /// </summary>
        public string? InlineHtml { get; set; }

        /// <summary>
        /// File path for file-backed tests (WPT). HTML is loaded on demand
        /// by calling LoadHtml() — never cached on the test case.
        /// </summary>
        public string? HtmlFilePath { get; set; }

        /// <summary>
        /// Loader function for file-backed tests. Reads file, inlines
        /// stylesheets/fonts, and returns processed HTML. Called each
        /// time LoadHtml() is invoked — result is NOT cached.
        /// </summary>
        public Func<string, string>? HtmlLoader { get; set; }

        /// <summary>
        /// Gets the HTML for this test. For inline tests, returns InlineHtml.
        /// For file-backed tests, loads from disk on each call (no caching).
        /// </summary>
        public string LoadHtml()
        {
            if (InlineHtml != null)
            {
                return InlineHtml;
            }
            if (HtmlFilePath != null && HtmlLoader != null)
            {
                return HtmlLoader(HtmlFilePath);
            }
            return "";
        }

        // Backward compat: Html property delegates to LoadHtml for inline tests
        // or loads once for the old API. New code should use LoadHtml().
        public string Html
        {
            get => LoadHtml();
            set => InlineHtml = value;
        }

        /// <summary>
        /// Sets a deferred file path for file-backed HTML loading.
        /// </summary>
        public void SetDeferredHtml(string filePath, Func<string, string> loader)
        {
            HtmlFilePath = filePath;
            HtmlLoader = loader;
            InlineHtml = null;
        }

        public override string ToString() => $"{Category}/{Name}";

        /// <summary>
        /// Release the cached HTML string to free memory after the test has run.
        /// </summary>
        public void ClearHtml()
        {
            InlineHtml = null;
        }
    }
}
