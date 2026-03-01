using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rend.VisualRegression.Infrastructure
{
    /// <summary>
    /// Generates a standalone HTML report for visual regression test results.
    /// The report is completely self-contained with inline CSS, JS, and base64-encoded images.
    /// </summary>
    public static class ReportGenerator
    {
        /// <summary>
        /// Generate the HTML report and write it to the specified path.
        /// </summary>
        public static void Generate(IReadOnlyList<ComparisonResult> results, string outputPath)
        {
            var dir = Path.GetDirectoryName(outputPath);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var html = BuildReport(results);
            File.WriteAllText(outputPath, html, Encoding.UTF8);
        }

        private static string BuildReport(IReadOnlyList<ComparisonResult> results)
        {
            int passCount = results.Count(r => r.Outcome == ComparisonOutcome.Pass);
            int failCount = results.Count(r => r.Outcome == ComparisonOutcome.Fail);
            int errorCount = results.Count(r => r.Outcome == ComparisonOutcome.Error);
            int totalCount = results.Count;

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"UTF-8\">");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine("<title>Visual Regression Test Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(GetCss());
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header
            sb.AppendLine("<header>");
            sb.AppendLine("<h1>Visual Regression Test Report</h1>");
            sb.AppendLine($"<p class=\"timestamp\">Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine("<div class=\"summary\">");
            sb.AppendLine($"  <span class=\"badge badge-total\">{totalCount} Total</span>");
            sb.AppendLine($"  <span class=\"badge badge-pass\">{passCount} Passed</span>");
            sb.AppendLine($"  <span class=\"badge badge-fail\">{failCount} Failed</span>");
            sb.AppendLine($"  <span class=\"badge badge-error\">{errorCount} Errors</span>");
            sb.AppendLine("</div>");

            // Filter buttons
            sb.AppendLine("<div class=\"filters\">");
            sb.AppendLine("  <button class=\"filter-btn active\" data-filter=\"all\">All</button>");
            sb.AppendLine("  <button class=\"filter-btn\" data-filter=\"fail\">Failures Only</button>");
            sb.AppendLine("  <button class=\"filter-btn\" data-filter=\"pass\">Passed Only</button>");
            sb.AppendLine("  <button class=\"filter-btn\" data-filter=\"error\">Errors Only</button>");
            sb.AppendLine("</div>");
            sb.AppendLine("</header>");

            // Results table
            sb.AppendLine("<main>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("  <th>Status</th>");
            sb.AppendLine("  <th>Test Name</th>");
            sb.AppendLine("  <th>Category</th>");
            sb.AppendLine("  <th>Chrome</th>");
            sb.AppendLine("  <th>Rend</th>");
            sb.AppendLine("  <th>Diff</th>");
            sb.AppendLine("  <th>Diff %</th>");
            sb.AppendLine("  <th>Duration</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            // Sort: failures and new first, then errors, then pass
            var sorted = results
                .OrderBy(r => r.Outcome switch
                {
                    ComparisonOutcome.Fail => 0,
                    ComparisonOutcome.Error => 1,
                    ComparisonOutcome.Pass => 2,
                    _ => 3
                })
                .ThenBy(r => r.Category)
                .ThenBy(r => r.TestName);

            foreach (var result in sorted)
            {
                string statusClass = result.Outcome.ToString().ToLower();
                string statusLabel = result.Outcome switch
                {
                    ComparisonOutcome.Pass => "PASS",
                    ComparisonOutcome.Fail => "FAIL",
                    ComparisonOutcome.Error => "ERROR",
                    _ => "UNKNOWN"
                };

                sb.AppendLine($"<tr class=\"result-row {statusClass}\" data-status=\"{statusClass}\">");
                sb.AppendLine($"  <td><span class=\"status-badge status-{statusClass}\">{statusLabel}</span></td>");
                sb.AppendLine($"  <td>{Escape(result.TestName)}</td>");
                sb.AppendLine($"  <td>{Escape(result.Category)}</td>");

                // Chrome image
                sb.AppendLine("  <td class=\"image-cell\">");
                if (result.ChromeImagePath != null && File.Exists(result.ChromeImagePath))
                {
                    string dataUri = ToDataUri(result.ChromeImagePath);
                    sb.AppendLine($"    <img src=\"{dataUri}\" alt=\"Chrome\" class=\"thumb\" onclick=\"openLightbox(this.src)\">");
                }
                else
                {
                    sb.AppendLine("    <span class=\"no-image\">N/A</span>");
                }
                sb.AppendLine("  </td>");

                // Rend image
                sb.AppendLine("  <td class=\"image-cell\">");
                if (result.RendImagePath != null && File.Exists(result.RendImagePath))
                {
                    string dataUri = ToDataUri(result.RendImagePath);
                    sb.AppendLine($"    <img src=\"{dataUri}\" alt=\"Rend\" class=\"thumb\" onclick=\"openLightbox(this.src)\">");
                }
                else
                {
                    sb.AppendLine("    <span class=\"no-image\">N/A</span>");
                }
                sb.AppendLine("  </td>");

                // Diff image
                sb.AppendLine("  <td class=\"image-cell\">");
                if (result.DiffImagePath != null && File.Exists(result.DiffImagePath))
                {
                    string dataUri = ToDataUri(result.DiffImagePath);
                    sb.AppendLine($"    <img src=\"{dataUri}\" alt=\"Diff\" class=\"thumb\" onclick=\"openLightbox(this.src)\">");
                }
                else
                {
                    sb.AppendLine("    <span class=\"no-image\">N/A</span>");
                }
                sb.AppendLine("  </td>");

                // Diff percentage
                string diffText = result.Outcome == ComparisonOutcome.Error
                    ? Escape(result.ErrorMessage ?? "Error")
                    : $"{result.DiffPercentage:F2}%";
                sb.AppendLine($"  <td>{diffText}</td>");

                // Duration
                sb.AppendLine($"  <td>{result.Duration.TotalMilliseconds:F0}ms</td>");

                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</main>");

            // Lightbox overlay
            sb.AppendLine("<div id=\"lightbox\" class=\"lightbox\" onclick=\"closeLightbox()\">");
            sb.AppendLine("  <img id=\"lightbox-img\" src=\"\" alt=\"Full size\">");
            sb.AppendLine("</div>");

            // JavaScript
            sb.AppendLine("<script>");
            sb.AppendLine(GetJs());
            sb.AppendLine("</script>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private static string ToDataUri(string filePath)
        {
            try
            {
                var bytes = File.ReadAllBytes(filePath);
                var base64 = Convert.ToBase64String(bytes);
                return $"data:image/png;base64,{base64}";
            }
            catch
            {
                return "";
            }
        }

        private static string Escape(string text)
        {
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

        private static string GetCss()
        {
            return @"
* { margin: 0; padding: 0; box-sizing: border-box; }

body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
    background: #f5f7fa;
    color: #333;
    line-height: 1.6;
}

header {
    background: #1a1a2e;
    color: #fff;
    padding: 24px 32px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.15);
}

header h1 {
    font-size: 24px;
    font-weight: 600;
    margin-bottom: 8px;
}

.timestamp {
    font-size: 13px;
    color: #aaa;
    margin-bottom: 16px;
}

.summary {
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
    margin-bottom: 16px;
}

.badge {
    display: inline-block;
    padding: 6px 16px;
    border-radius: 20px;
    font-size: 14px;
    font-weight: 600;
}

.badge-total { background: #444; color: #fff; }
.badge-pass { background: #27ae60; color: #fff; }
.badge-fail { background: #e74c3c; color: #fff; }
.badge-new { background: #3498db; color: #fff; }
.badge-error { background: #e67e22; color: #fff; }

.filters {
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
}

.filter-btn {
    padding: 6px 16px;
    border: 1px solid rgba(255,255,255,0.3);
    border-radius: 4px;
    background: transparent;
    color: #ccc;
    cursor: pointer;
    font-size: 13px;
    transition: all 0.2s;
}

.filter-btn:hover {
    background: rgba(255,255,255,0.1);
    color: #fff;
}

.filter-btn.active {
    background: rgba(255,255,255,0.2);
    color: #fff;
    border-color: rgba(255,255,255,0.6);
}

main {
    padding: 24px 32px;
}

table {
    width: 100%;
    border-collapse: collapse;
    background: #fff;
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 1px 4px rgba(0,0,0,0.08);
}

thead {
    background: #f0f2f5;
}

th {
    padding: 12px 16px;
    text-align: left;
    font-size: 13px;
    font-weight: 600;
    color: #555;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    border-bottom: 2px solid #e0e0e0;
}

td {
    padding: 12px 16px;
    border-bottom: 1px solid #eee;
    font-size: 14px;
    vertical-align: middle;
}

tr:hover {
    background: #fafbfc;
}

.status-badge {
    display: inline-block;
    padding: 3px 10px;
    border-radius: 12px;
    font-size: 12px;
    font-weight: 700;
    letter-spacing: 0.5px;
}

.status-pass { background: #d4edda; color: #155724; }
.status-fail { background: #f8d7da; color: #721c24; }
.status-new { background: #cce5ff; color: #004085; }
.status-error { background: #fff3cd; color: #856404; }

.image-cell {
    text-align: center;
}

.thumb {
    max-width: 150px;
    max-height: 120px;
    border: 1px solid #ddd;
    border-radius: 4px;
    cursor: pointer;
    transition: transform 0.2s, box-shadow 0.2s;
}

.thumb:hover {
    transform: scale(1.05);
    box-shadow: 0 2px 8px rgba(0,0,0,0.15);
}

.no-image {
    color: #999;
    font-style: italic;
    font-size: 13px;
}

.result-row.hidden {
    display: none;
}

/* Lightbox */
.lightbox {
    display: none;
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0,0,0,0.85);
    z-index: 1000;
    justify-content: center;
    align-items: center;
    cursor: pointer;
}

.lightbox.visible {
    display: flex;
}

.lightbox img {
    max-width: 90%;
    max-height: 90%;
    border-radius: 4px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.5);
}
";
        }

        private static string GetJs()
        {
            return @"
// Filter functionality
document.querySelectorAll('.filter-btn').forEach(function(btn) {
    btn.addEventListener('click', function() {
        document.querySelectorAll('.filter-btn').forEach(function(b) { b.classList.remove('active'); });
        btn.classList.add('active');

        var filter = btn.getAttribute('data-filter');
        document.querySelectorAll('.result-row').forEach(function(row) {
            if (filter === 'all') {
                row.classList.remove('hidden');
            } else {
                var status = row.getAttribute('data-status');
                if (status === filter) {
                    row.classList.remove('hidden');
                } else {
                    row.classList.add('hidden');
                }
            }
        });
    });
});

// Lightbox functionality
function openLightbox(src) {
    var lb = document.getElementById('lightbox');
    document.getElementById('lightbox-img').src = src;
    lb.classList.add('visible');
}

function closeLightbox() {
    document.getElementById('lightbox').classList.remove('visible');
}

document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
        closeLightbox();
    }
});
";
        }
    }
}
