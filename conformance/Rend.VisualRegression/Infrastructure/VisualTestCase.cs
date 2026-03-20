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
        /// HTML content. For inline tests (YAML), set directly.
        /// For file-backed tests (WPT), set HtmlFilePath instead — HTML is loaded lazily.
        /// </summary>
        public string Html
        {
            get
            {
                if (_html != null)
                {
                    return _html;
                }
                if (_htmlFilePath != null)
                {
                    _html = _htmlLoader!(_htmlFilePath);
                    _htmlFilePath = null;
                    _htmlLoader = null;
                }
                return _html ?? "";
            }
            set { _html = value; }
        }

        private string? _html;
        private string? _htmlFilePath;
        private System.Func<string, string>? _htmlLoader;

        /// <summary>
        /// Sets a deferred file path for lazy HTML loading.
        /// The loader function is called once when Html is first accessed.
        /// </summary>
        public void SetDeferredHtml(string filePath, System.Func<string, string> loader)
        {
            _htmlFilePath = filePath;
            _htmlLoader = loader;
            _html = null;
        }

        public override string ToString() => $"{Category}/{Name}";
    }
}
