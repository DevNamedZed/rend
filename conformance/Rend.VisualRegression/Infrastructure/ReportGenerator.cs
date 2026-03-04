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
            int nearPassCount = results.Count(r => r.Outcome == ComparisonOutcome.NearPass);
            int failCount = results.Count(r => r.Outcome == ComparisonOutcome.Fail);
            int errorCount = results.Count(r => r.Outcome == ComparisonOutcome.Error);
            int totalCount = results.Count;
            double avgDiff = results.Where(r => r.Outcome != ComparisonOutcome.Error)
                .Select(r => r.DiffPercentage).DefaultIfEmpty(0).Average();
            double maxDiff = results.Where(r => r.Outcome != ComparisonOutcome.Error)
                .Select(r => r.DiffPercentage).DefaultIfEmpty(0).Max();
            double medianDiff = 0;
            var nonErrorDiffs = results.Where(r => r.Outcome != ComparisonOutcome.Error)
                .Select(r => r.DiffPercentage).OrderBy(d => d).ToList();
            if (nonErrorDiffs.Count > 0)
                medianDiff = nonErrorDiffs[nonErrorDiffs.Count / 2];

            // Category breakdown
            var categories = results.GroupBy(r => r.Category)
                .Select(g => new
                {
                    Name = g.Key,
                    Total = g.Count(),
                    Passed = g.Count(r => r.Outcome == ComparisonOutcome.Pass || r.Outcome == ComparisonOutcome.NearPass),
                    AvgDiff = g.Where(r => r.Outcome != ComparisonOutcome.Error)
                        .Select(r => r.DiffPercentage).DefaultIfEmpty(0).Average()
                })
                .OrderByDescending(c => c.AvgDiff)
                .ToList();

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
            sb.AppendLine("<h1>Visual Regression: Chrome vs Rend</h1>");
            sb.AppendLine($"<p class=\"timestamp\">Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

            // Filter tags (clickable) + search + stats
            sb.AppendLine("<div class=\"toolbar\">");
            sb.AppendLine("<div class=\"filters\">");
            sb.AppendLine($"  <button class=\"filter-btn filter-all active\" data-filter=\"all\">All <span class=\"filter-count\">{totalCount}</span></button>");
            sb.AppendLine($"  <button class=\"filter-btn filter-pass\" data-filter=\"pass\">Passed <span class=\"filter-count\">{passCount}</span></button>");
            if (nearPassCount > 0)
                sb.AppendLine($"  <button class=\"filter-btn filter-nearpass\" data-filter=\"nearpass\">Near-Pass <span class=\"filter-count\">{nearPassCount}</span></button>");
            sb.AppendLine($"  <button class=\"filter-btn filter-fail\" data-filter=\"fail\">Failed <span class=\"filter-count\">{failCount}</span></button>");
            if (errorCount > 0)
                sb.AppendLine($"  <button class=\"filter-btn filter-error\" data-filter=\"error\">Errors <span class=\"filter-count\">{errorCount}</span></button>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"search-box\">");
            sb.AppendLine("  <input type=\"text\" id=\"search-input\" placeholder=\"Search tests...\" autocomplete=\"off\">");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"category-picker\" id=\"category-picker\">");
            sb.AppendLine("  <div class=\"category-tags\" id=\"category-tags\"></div>");
            sb.AppendLine("  <input type=\"text\" id=\"category-input\" placeholder=\"Filter by category...\" autocomplete=\"off\">");
            sb.AppendLine("  <div class=\"category-dropdown\" id=\"category-dropdown\">");
            foreach (var cat in categories.OrderBy(c => c.Name))
            {
                sb.AppendLine($"    <div class=\"category-option\" data-category=\"{Escape(cat.Name.ToLower())}\">{Escape(cat.Name)} <span class=\"cat-count\">{cat.Passed}/{cat.Total}</span></div>");
            }
            sb.AppendLine("  </div>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<div class=\"stats\">avg {avgDiff:F2}% &middot; median {medianDiff:F2}% &middot; max {maxDiff:F2}%</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</header>");

            // Results table
            sb.AppendLine("<main>");
            sb.AppendLine("<table id=\"results-table\">");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("  <th class=\"sortable\" data-sort=\"index\" data-type=\"number\"># <span class=\"sort-arrow\"></span></th>");
            sb.AppendLine("  <th class=\"sortable\" data-sort=\"status\" data-type=\"number\">Status <span class=\"sort-arrow\"></span></th>");
            sb.AppendLine("  <th class=\"sortable\" data-sort=\"name\" data-type=\"string\">Test Name <span class=\"sort-arrow\"></span></th>");
            sb.AppendLine("  <th class=\"sortable\" data-sort=\"category\" data-type=\"string\">Category <span class=\"sort-arrow\"></span></th>");
            sb.AppendLine("  <th>Chrome</th>");
            sb.AppendLine("  <th>Rend</th>");
            sb.AppendLine("  <th>Diff</th>");
            sb.AppendLine("  <th class=\"sortable sorted-desc\" data-sort=\"diff\" data-type=\"number\">Diff % <span class=\"sort-arrow\">&#9660;</span></th>");
            sb.AppendLine("  <th class=\"sortable\" data-sort=\"duration\" data-type=\"number\">Duration <span class=\"sort-arrow\"></span></th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            // Default sort: by diff percentage descending
            var sorted = results
                .OrderByDescending(r => r.DiffPercentage)
                .ToList();

            int rowIndex = 0;
            foreach (var result in sorted)
            {
                string statusClass = result.Outcome.ToString().ToLower();
                string statusLabel = result.Outcome switch
                {
                    ComparisonOutcome.Pass => "PASS",
                    ComparisonOutcome.NearPass => "~PASS",
                    ComparisonOutcome.Fail => "FAIL",
                    ComparisonOutcome.Error => "ERROR",
                    _ => "UNKNOWN"
                };

                int statusOrder = result.Outcome switch
                {
                    ComparisonOutcome.Fail => 0,
                    ComparisonOutcome.Error => 1,
                    ComparisonOutcome.NearPass => 2,
                    ComparisonOutcome.Pass => 3,
                    _ => 4
                };

                string diffColor = DiffColor(result.DiffPercentage);

                rowIndex++;
                sb.AppendLine($"<tr class=\"result-row {statusClass}\" data-status=\"{statusClass}\" data-sort-index=\"{rowIndex}\" data-sort-status=\"{statusOrder}\" data-sort-name=\"{Escape(result.TestName.ToLower())}\" data-sort-category=\"{Escape(result.Category.ToLower())}\" data-sort-diff=\"{result.DiffPercentage:F4}\" data-sort-duration=\"{result.Duration.TotalMilliseconds:F0}\">");
                sb.AppendLine($"  <td class=\"index-cell\">{rowIndex}</td>");
                sb.AppendLine($"  <td><span class=\"status-badge status-{statusClass}\">{statusLabel}</span></td>");
                sb.AppendLine($"  <td class=\"test-name\">{Escape(result.TestName)}</td>");
                sb.AppendLine($"  <td class=\"category-name\">{Escape(result.Category)}</td>");

                // Chrome image
                sb.AppendLine("  <td class=\"image-cell\">");
                if (result.ChromeImagePath != null && File.Exists(result.ChromeImagePath))
                {
                    string dataUri = ToDataUri(result.ChromeImagePath);
                    sb.AppendLine($"    <img src=\"{dataUri}\" alt=\"Chrome\" class=\"thumb\" data-label=\"Chrome\" onclick=\"openLightbox(this, event)\">");
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
                    sb.AppendLine($"    <img src=\"{dataUri}\" alt=\"Rend\" class=\"thumb\" data-label=\"Rend\" onclick=\"openLightbox(this, event)\">");
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
                    sb.AppendLine($"    <img src=\"{dataUri}\" alt=\"Diff\" class=\"thumb\" data-label=\"Diff\" onclick=\"openLightbox(this, event)\">");
                }
                else
                {
                    sb.AppendLine("    <span class=\"no-image\">N/A</span>");
                }
                sb.AppendLine("  </td>");

                // Diff percentage with color coding
                if (result.Outcome == ComparisonOutcome.Error)
                {
                    sb.AppendLine($"  <td class=\"diff-cell\"><span class=\"diff-error\">{Escape(result.ErrorMessage ?? "Error")}</span></td>");
                }
                else if (result.Outcome == ComparisonOutcome.NearPass)
                {
                    sb.AppendLine($"  <td class=\"diff-cell\"><span class=\"diff-value\" style=\"color:{diffColor}\">{result.DiffPercentage:F2}%</span> <span class=\"diff-shift\">→ {result.ShiftTolerantDiffPercentage:F2}%</span></td>");
                }
                else
                {
                    sb.AppendLine($"  <td class=\"diff-cell\"><span class=\"diff-value\" style=\"color:{diffColor}\">{result.DiffPercentage:F2}%</span></td>");
                }

                // Duration
                sb.AppendLine($"  <td class=\"duration-cell\">{result.Duration.TotalMilliseconds:F0}ms</td>");

                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Row count indicator
            sb.AppendLine("<div class=\"row-count\" id=\"row-count\"></div>");
            sb.AppendLine("</main>");

            // Lightbox overlay — side-by-side view
            sb.AppendLine("<div id=\"lightbox\" class=\"lightbox\" onclick=\"closeLightboxBg(event)\">");
            sb.AppendLine("  <div class=\"lightbox-content\">");
            sb.AppendLine("    <div class=\"lightbox-header\">");
            sb.AppendLine("      <span id=\"lightbox-title\"></span>");
            sb.AppendLine("      <button class=\"lightbox-close\" onclick=\"closeLightbox()\">&times;</button>");
            sb.AppendLine("    </div>");
            sb.AppendLine("    <div class=\"lightbox-body\">");
            sb.AppendLine("      <div class=\"lightbox-panel\"><div class=\"lightbox-label\">Chrome</div><img id=\"lb-chrome\" src=\"\"></div>");
            sb.AppendLine("      <div class=\"lightbox-panel\"><div class=\"lightbox-label\">Rend</div><img id=\"lb-rend\" src=\"\"></div>");
            sb.AppendLine("      <div class=\"lightbox-panel\"><div class=\"lightbox-label\">Diff</div><img id=\"lb-diff\" src=\"\"></div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");
            sb.AppendLine("</div>");

            // JavaScript
            sb.AppendLine("<script>");
            sb.AppendLine(GetJs());
            sb.AppendLine("</script>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private static string DiffColor(double diff)
        {
            if (diff <= 0.0) return "#27ae60";       // perfect green
            if (diff <= 0.5) return "#2ecc71";        // light green
            if (diff <= 1.0) return "#f39c12";        // orange
            if (diff <= 5.0) return "#e67e22";        // dark orange
            if (diff <= 10.0) return "#e74c3c";       // red
            return "#c0392b";                          // dark red
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
    padding: 24px 32px 16px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.15);
    position: sticky;
    top: 0;
    z-index: 100;
}

header h1 {
    font-size: 22px;
    font-weight: 600;
    margin-bottom: 4px;
}

.timestamp {
    font-size: 12px;
    color: #888;
    margin-bottom: 12px;
}

.toolbar {
    display: flex;
    gap: 12px;
    align-items: center;
    flex-wrap: wrap;
}

.filters {
    display: flex;
    gap: 6px;
    flex-wrap: wrap;
}

.filter-btn {
    padding: 5px 12px;
    border: 1px solid rgba(255,255,255,0.2);
    border-radius: 4px;
    background: transparent;
    color: #999;
    cursor: pointer;
    font-size: 13px;
    transition: all 0.15s;
}

.filter-count {
    display: inline-block;
    margin-left: 4px;
    padding: 0 5px;
    border-radius: 8px;
    font-size: 11px;
    font-weight: 600;
    background: rgba(255,255,255,0.1);
    min-width: 18px;
    text-align: center;
}

.filter-btn:hover { background: rgba(255,255,255,0.08); color: #ccc; }

.filter-btn.active { color: #fff; border-color: rgba(255,255,255,0.4); }
.filter-btn.filter-all.active { background: rgba(255,255,255,0.15); }
.filter-btn.filter-pass.active { background: rgba(39,174,96,0.3); border-color: #27ae60; }
.filter-btn.filter-nearpass.active { background: rgba(241,196,15,0.25); border-color: #f1c40f; color: #f1c40f; }
.filter-btn.filter-fail.active { background: rgba(231,76,60,0.3); border-color: #e74c3c; }
.filter-btn.filter-error.active { background: rgba(230,126,34,0.3); border-color: #e67e22; }

.stats {
    margin-left: auto;
    font-size: 12px;
    color: #777;
    white-space: nowrap;
}

.search-box {
    flex: 1;
    min-width: 200px;
    max-width: 350px;
}

.search-box input {
    width: 100%;
    padding: 6px 12px;
    border: 1px solid rgba(255,255,255,0.25);
    border-radius: 4px;
    background: rgba(255,255,255,0.08);
    color: #fff;
    font-size: 13px;
    outline: none;
    transition: border-color 0.15s;
}

.search-box input::placeholder { color: #777; }
.search-box input:focus { border-color: rgba(255,255,255,0.5); background: rgba(255,255,255,0.12); }

/* Category tag-list picker */
.category-picker {
    position: relative;
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 4px;
    min-width: 180px;
    max-width: 400px;
    padding: 3px 6px;
    border: 1px solid rgba(255,255,255,0.2);
    border-radius: 4px;
    background: rgba(255,255,255,0.06);
    cursor: text;
}

.category-picker:focus-within {
    border-color: rgba(255,255,255,0.5);
    background: rgba(255,255,255,0.1);
}

.category-tags {
    display: contents;
}

.category-tag {
    display: inline-flex;
    align-items: center;
    gap: 3px;
    padding: 2px 8px;
    border-radius: 3px;
    font-size: 11px;
    font-weight: 500;
    background: rgba(52,152,219,0.35);
    color: #fff;
    white-space: nowrap;
    animation: tagIn 0.12s ease-out;
}

@keyframes tagIn { from { opacity: 0; transform: scale(0.9); } to { opacity: 1; transform: scale(1); } }

.category-tag .tag-x {
    cursor: pointer;
    font-size: 13px;
    line-height: 1;
    opacity: 0.6;
    margin-left: 2px;
}

.category-tag .tag-x:hover { opacity: 1; }

#category-input {
    flex: 1;
    min-width: 80px;
    border: none;
    outline: none;
    background: transparent;
    color: #fff;
    font-size: 12px;
    padding: 3px 2px;
}

#category-input::placeholder { color: #777; }

.category-dropdown {
    display: none;
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    margin-top: 2px;
    background: #222;
    border: 1px solid #444;
    border-radius: 4px;
    max-height: 240px;
    overflow-y: auto;
    z-index: 200;
    box-shadow: 0 4px 16px rgba(0,0,0,0.4);
}

.category-dropdown.open { display: block; }

.category-option {
    display: flex;
    justify-content: space-between;
    padding: 6px 10px;
    font-size: 12px;
    color: #ccc;
    cursor: pointer;
    transition: background 0.1s;
}

.category-option:hover, .category-option.highlighted {
    background: rgba(52,152,219,0.3);
    color: #fff;
}

.category-option.selected {
    background: rgba(52,152,219,0.15);
    color: #3498db;
}

.category-option .cat-count {
    font-size: 11px;
    opacity: 0.6;
}

main {
    padding: 16px 32px 32px;
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
    position: sticky;
    top: 0;
    z-index: 10;
}

th {
    padding: 10px 14px;
    text-align: left;
    font-size: 12px;
    font-weight: 600;
    color: #555;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    border-bottom: 2px solid #e0e0e0;
    white-space: nowrap;
    user-select: none;
}

th.sortable {
    cursor: pointer;
    transition: background 0.15s;
}

th.sortable:hover {
    background: #e4e7eb;
}

.sort-arrow {
    font-size: 10px;
    margin-left: 4px;
    opacity: 0.4;
}

th.sorted-asc .sort-arrow,
th.sorted-desc .sort-arrow {
    opacity: 1;
}

td {
    padding: 10px 14px;
    border-bottom: 1px solid #eee;
    font-size: 13px;
    vertical-align: middle;
}

tbody tr:hover {
    background: #f8f9fb;
}

.status-badge {
    display: inline-block;
    padding: 2px 8px;
    border-radius: 10px;
    font-size: 11px;
    font-weight: 700;
    letter-spacing: 0.5px;
}

.status-pass { background: #d4edda; color: #155724; }
.status-nearpass { background: #fff3cd; color: #856404; }
.status-fail { background: #f8d7da; color: #721c24; }
.status-error { background: #fff3cd; color: #856404; }

.image-cell {
    text-align: center;
    padding: 6px 8px;
}

.thumb {
    max-width: 140px;
    max-height: 100px;
    border: 1px solid #ddd;
    border-radius: 3px;
    cursor: pointer;
    transition: transform 0.15s, box-shadow 0.15s;
}

.thumb:hover {
    transform: scale(1.08);
    box-shadow: 0 2px 8px rgba(0,0,0,0.15);
}

.no-image {
    color: #bbb;
    font-style: italic;
    font-size: 12px;
}

.diff-cell { text-align: right; font-variant-numeric: tabular-nums; white-space: nowrap; }
.diff-shift { color: #27ae60; font-size: 11px; font-weight: 500; }
.diff-value { font-weight: 600; font-size: 13px; }
.diff-error { color: #e67e22; font-size: 12px; }
.duration-cell { text-align: right; color: #888; font-variant-numeric: tabular-nums; }
.index-cell { text-align: center; color: #999; font-variant-numeric: tabular-nums; font-size: 12px; width: 40px; }

.test-name { font-weight: 500; }
.category-name { color: #666; font-size: 12px; }

.result-row.hidden {
    display: none;
}

.row-count {
    text-align: center;
    padding: 12px;
    color: #888;
    font-size: 13px;
}

/* Lightbox — side-by-side */
.lightbox {
    display: none;
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0,0,0,0.88);
    z-index: 1000;
    justify-content: center;
    align-items: center;
}

.lightbox.visible {
    display: flex;
}

.lightbox-content {
    background: #1a1a2e;
    border-radius: 8px;
    max-width: 95vw;
    max-height: 92vh;
    overflow: auto;
    box-shadow: 0 8px 32px rgba(0,0,0,0.5);
}

.lightbox-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px 20px;
    border-bottom: 1px solid rgba(255,255,255,0.1);
    color: #fff;
    font-size: 15px;
    font-weight: 600;
}

.lightbox-close {
    background: none;
    border: none;
    color: #aaa;
    font-size: 24px;
    cursor: pointer;
    padding: 0 4px;
    line-height: 1;
}

.lightbox-close:hover { color: #fff; }

.lightbox-body {
    display: flex;
    gap: 2px;
    padding: 12px;
}

.lightbox-panel {
    flex: 1;
    text-align: center;
    min-width: 0;
}

.lightbox-label {
    color: #aaa;
    font-size: 12px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 1px;
    margin-bottom: 8px;
}

.lightbox-panel img {
    max-width: 100%;
    max-height: 75vh;
    border-radius: 4px;
    border: 1px solid rgba(255,255,255,0.1);
}
";
        }

        private static string GetJs()
        {
            return @"
// ---- Column sorting ----
var currentSort = { col: 'diff', dir: 'desc' };

function sortTable(col, type) {
    var tbody = document.querySelector('#results-table tbody');
    var rows = Array.from(tbody.querySelectorAll('tr.result-row'));
    var dir = 'asc';
    if (currentSort.col === col) {
        dir = currentSort.dir === 'asc' ? 'desc' : 'asc';
    } else {
        // Default direction: desc for numbers, asc for strings
        dir = (type === 'number') ? 'desc' : 'asc';
    }
    currentSort = { col: col, dir: dir };

    rows.sort(function(a, b) {
        var aVal = a.getAttribute('data-sort-' + col);
        var bVal = b.getAttribute('data-sort-' + col);
        var cmp;
        if (type === 'number') {
            cmp = parseFloat(aVal) - parseFloat(bVal);
        } else {
            cmp = aVal.localeCompare(bVal);
        }
        return dir === 'asc' ? cmp : -cmp;
    });

    rows.forEach(function(row) { tbody.appendChild(row); });
    renumberRows();

    // Update header arrows
    document.querySelectorAll('th.sortable').forEach(function(th) {
        th.classList.remove('sorted-asc', 'sorted-desc');
        var arrow = th.querySelector('.sort-arrow');
        if (th.getAttribute('data-sort') === col) {
            th.classList.add(dir === 'asc' ? 'sorted-asc' : 'sorted-desc');
            arrow.innerHTML = dir === 'asc' ? '&#9650;' : '&#9660;';
        } else {
            arrow.innerHTML = '';
        }
    });
}

document.querySelectorAll('th.sortable').forEach(function(th) {
    th.addEventListener('click', function() {
        sortTable(th.getAttribute('data-sort'), th.getAttribute('data-type'));
    });
});

// ---- Filter functionality ----
var activeFilter = 'all';
var selectedCategories = [];

function applyFilters() {
    var search = document.getElementById('search-input').value.toLowerCase();
    var visible = 0;
    document.querySelectorAll('.result-row').forEach(function(row) {
        var status = row.getAttribute('data-status');
        var name = row.getAttribute('data-sort-name');
        var category = row.getAttribute('data-sort-category');
        var matchFilter = (activeFilter === 'all') || (status === activeFilter);
        var matchSearch = !search || name.indexOf(search) !== -1 || category.indexOf(search) !== -1;
        var matchCategory = selectedCategories.length === 0 || selectedCategories.indexOf(category) !== -1;
        if (matchFilter && matchSearch && matchCategory) {
            row.classList.remove('hidden');
            visible++;
        } else {
            row.classList.add('hidden');
        }
    });
    updateRowCount(visible);
    renumberRows();
}

// ---- Renumber visible rows ----
function renumberRows() {
    var idx = 0;
    document.querySelectorAll('.result-row').forEach(function(row) {
        var cell = row.querySelector('.index-cell');
        if (!cell) return;
        if (row.classList.contains('hidden')) {
            cell.textContent = '';
        } else {
            idx++;
            cell.textContent = idx;
        }
    });
}

function updateRowCount(visible) {
    var total = document.querySelectorAll('.result-row').length;
    var el = document.getElementById('row-count');
    if (visible < total) {
        el.textContent = 'Showing ' + visible + ' of ' + total + ' tests';
    } else {
        el.textContent = '';
    }
}

document.querySelectorAll('.filter-btn').forEach(function(btn) {
    btn.addEventListener('click', function() {
        document.querySelectorAll('.filter-btn').forEach(function(b) { b.classList.remove('active'); });
        btn.classList.add('active');
        activeFilter = btn.getAttribute('data-filter');
        applyFilters();
    });
});

document.getElementById('search-input').addEventListener('input', function() {
    applyFilters();
});

// ---- Category tag-list picker ----
(function() {
    var input = document.getElementById('category-input');
    var dropdown = document.getElementById('category-dropdown');
    var tagsEl = document.getElementById('category-tags');
    var picker = document.getElementById('category-picker');
    var options = Array.from(dropdown.querySelectorAll('.category-option'));
    var highlighted = -1;

    function renderTags() {
        tagsEl.innerHTML = '';
        selectedCategories.forEach(function(cat) {
            var tag = document.createElement('span');
            tag.className = 'category-tag';
            tag.textContent = options.find(function(o) { return o.getAttribute('data-category') === cat; })?.textContent.split(' ')[0] || cat;
            var x = document.createElement('span');
            x.className = 'tag-x';
            x.textContent = '\u00d7';
            x.onclick = function(e) { e.stopPropagation(); removeCategory(cat); };
            tag.appendChild(x);
            tagsEl.appendChild(tag);
        });
        input.placeholder = selectedCategories.length ? '' : 'Filter by category...';
    }

    function removeCategory(cat) {
        selectedCategories = selectedCategories.filter(function(c) { return c !== cat; });
        syncOptions();
        renderTags();
        applyFilters();
    }

    function syncOptions() {
        options.forEach(function(opt) {
            var cat = opt.getAttribute('data-category');
            opt.classList.toggle('selected', selectedCategories.indexOf(cat) !== -1);
        });
    }

    function filterDropdown() {
        var q = input.value.toLowerCase();
        var anyVisible = false;
        options.forEach(function(opt) {
            var show = opt.getAttribute('data-category').indexOf(q) !== -1;
            opt.style.display = show ? '' : 'none';
            if (show) anyVisible = true;
        });
        highlighted = -1;
    }

    function openDropdown() { dropdown.classList.add('open'); filterDropdown(); }
    function closeDropdown() { dropdown.classList.remove('open'); highlighted = -1; }

    picker.addEventListener('click', function() { input.focus(); });
    input.addEventListener('focus', openDropdown);
    input.addEventListener('input', function() { openDropdown(); filterDropdown(); });

    input.addEventListener('keydown', function(e) {
        var visible = options.filter(function(o) { return o.style.display !== 'none'; });
        if (e.key === 'ArrowDown') { e.preventDefault(); highlighted = Math.min(highlighted + 1, visible.length - 1); updateHighlight(visible); }
        else if (e.key === 'ArrowUp') { e.preventDefault(); highlighted = Math.max(highlighted - 1, 0); updateHighlight(visible); }
        else if (e.key === 'Enter') { e.preventDefault(); if (highlighted >= 0 && highlighted < visible.length) toggleOption(visible[highlighted]); }
        else if (e.key === 'Escape') { closeDropdown(); input.blur(); }
        else if (e.key === 'Backspace' && !input.value && selectedCategories.length) { removeCategory(selectedCategories[selectedCategories.length - 1]); }
    });

    function updateHighlight(visible) {
        options.forEach(function(o) { o.classList.remove('highlighted'); });
        if (highlighted >= 0 && highlighted < visible.length) visible[highlighted].classList.add('highlighted');
    }

    function toggleOption(opt) {
        var cat = opt.getAttribute('data-category');
        var idx = selectedCategories.indexOf(cat);
        if (idx === -1) selectedCategories.push(cat);
        else selectedCategories.splice(idx, 1);
        syncOptions();
        renderTags();
        input.value = '';
        filterDropdown();
        applyFilters();
    }

    options.forEach(function(opt) {
        opt.addEventListener('mousedown', function(e) { e.preventDefault(); toggleOption(opt); });
    });

    document.addEventListener('mousedown', function(e) {
        if (!picker.contains(e.target)) closeDropdown();
    });
})();

// ---- Lightbox (side-by-side) ----
function openLightbox(img, event) {
    event.stopPropagation();
    var row = img.closest('tr');
    var images = row.querySelectorAll('img.thumb');
    var testName = row.querySelector('.test-name');
    var title = testName ? testName.textContent : '';

    var lb = document.getElementById('lightbox');
    document.getElementById('lightbox-title').textContent = title;

    // Find chrome/rend/diff images from the row (order: chrome, rend, diff)
    var srcs = [];
    images.forEach(function(i) { srcs.push(i.src); });

    document.getElementById('lb-chrome').src = srcs[0] || '';
    document.getElementById('lb-rend').src = srcs[1] || '';
    document.getElementById('lb-diff').src = srcs[2] || '';

    lb.classList.add('visible');
}

function closeLightbox() {
    document.getElementById('lightbox').classList.remove('visible');
}

function closeLightboxBg(event) {
    if (event.target === document.getElementById('lightbox')) {
        closeLightbox();
    }
}

document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') closeLightbox();
});

// Keyboard shortcut: Ctrl/Cmd+F focuses search
document.addEventListener('keydown', function(e) {
    if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        // Only intercept if not already in input
        if (document.activeElement !== document.getElementById('search-input')) {
            e.preventDefault();
            document.getElementById('search-input').focus();
        }
    }
});
";
        }
    }
}
