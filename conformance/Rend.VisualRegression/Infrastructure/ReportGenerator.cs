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
                    Passed = g.Count(r => r.Outcome == ComparisonOutcome.Pass),
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

            // Summary badges
            sb.AppendLine("<div class=\"summary\">");
            sb.AppendLine($"  <span class=\"badge badge-total\">{totalCount} Total</span>");
            sb.AppendLine($"  <span class=\"badge badge-pass\">{passCount} Passed ({(totalCount > 0 ? 100.0 * passCount / totalCount : 0):F0}%)</span>");
            sb.AppendLine($"  <span class=\"badge badge-fail\">{failCount} Failed</span>");
            if (errorCount > 0)
                sb.AppendLine($"  <span class=\"badge badge-error\">{errorCount} Errors</span>");
            sb.AppendLine($"  <span class=\"badge badge-avg\">Avg: {avgDiff:F2}%</span>");
            sb.AppendLine($"  <span class=\"badge badge-median\">Median: {medianDiff:F2}%</span>");
            sb.AppendLine($"  <span class=\"badge badge-max\">Max: {maxDiff:F2}%</span>");
            sb.AppendLine("</div>");

            // Filter buttons + search
            sb.AppendLine("<div class=\"toolbar\">");
            sb.AppendLine("<div class=\"filters\">");
            sb.AppendLine("  <button class=\"filter-btn active\" data-filter=\"all\">All</button>");
            sb.AppendLine("  <button class=\"filter-btn\" data-filter=\"fail\">Failures</button>");
            sb.AppendLine("  <button class=\"filter-btn\" data-filter=\"pass\">Passed</button>");
            if (errorCount > 0)
                sb.AppendLine("  <button class=\"filter-btn\" data-filter=\"error\">Errors</button>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"search-box\">");
            sb.AppendLine("  <input type=\"text\" id=\"search-input\" placeholder=\"Search tests...\" autocomplete=\"off\">");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</header>");

            // Category breakdown bar
            sb.AppendLine("<div class=\"category-bar\">");
            sb.AppendLine("<div class=\"category-bar-inner\">");
            foreach (var cat in categories)
            {
                string catClass = cat.AvgDiff < 0.01 ? "cat-perfect" : cat.AvgDiff < 1.0 ? "cat-good" : cat.AvgDiff < 5.0 ? "cat-warn" : "cat-bad";
                sb.AppendLine($"  <span class=\"cat-chip {catClass}\" title=\"{Escape(cat.Name)}: {cat.Passed}/{cat.Total} passed, avg {cat.AvgDiff:F2}%\">");
                sb.AppendLine($"    {Escape(cat.Name)} <small>{cat.Passed}/{cat.Total}</small>");
                sb.AppendLine("  </span>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

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
                    ComparisonOutcome.Fail => "FAIL",
                    ComparisonOutcome.Error => "ERROR",
                    _ => "UNKNOWN"
                };

                int statusOrder = result.Outcome switch
                {
                    ComparisonOutcome.Fail => 0,
                    ComparisonOutcome.Error => 1,
                    ComparisonOutcome.Pass => 2,
                    _ => 3
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

.summary {
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
    margin-bottom: 12px;
}

.badge {
    display: inline-block;
    padding: 4px 12px;
    border-radius: 16px;
    font-size: 13px;
    font-weight: 600;
}

.badge-total { background: #444; color: #fff; }
.badge-pass { background: #27ae60; color: #fff; }
.badge-fail { background: #e74c3c; color: #fff; }
.badge-error { background: #e67e22; color: #fff; }
.badge-avg { background: #2c3e50; color: #f39c12; border: 1px solid #f39c12; }
.badge-median { background: #2c3e50; color: #3498db; border: 1px solid #3498db; }
.badge-max { background: #2c3e50; color: #e74c3c; border: 1px solid #e74c3c; }

.toolbar {
    display: flex;
    gap: 16px;
    align-items: center;
    flex-wrap: wrap;
}

.filters {
    display: flex;
    gap: 6px;
    flex-wrap: wrap;
}

.filter-btn {
    padding: 5px 14px;
    border: 1px solid rgba(255,255,255,0.25);
    border-radius: 4px;
    background: transparent;
    color: #aaa;
    cursor: pointer;
    font-size: 13px;
    transition: all 0.15s;
}

.filter-btn:hover {
    background: rgba(255,255,255,0.1);
    color: #fff;
}

.filter-btn.active {
    background: rgba(255,255,255,0.2);
    color: #fff;
    border-color: rgba(255,255,255,0.5);
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

/* Category breakdown bar */
.category-bar {
    background: #f0f2f5;
    padding: 8px 32px;
    border-bottom: 1px solid #ddd;
    overflow-x: auto;
}

.category-bar-inner {
    display: flex;
    gap: 6px;
    flex-wrap: wrap;
}

.cat-chip {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    padding: 3px 10px;
    border-radius: 12px;
    font-size: 12px;
    font-weight: 500;
    white-space: nowrap;
    cursor: default;
}

.cat-chip small { opacity: 0.7; font-weight: 400; }
.cat-perfect { background: #d4edda; color: #155724; }
.cat-good { background: #d1ecf1; color: #0c5460; }
.cat-warn { background: #fff3cd; color: #856404; }
.cat-bad { background: #f8d7da; color: #721c24; }

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

.diff-cell { text-align: right; font-variant-numeric: tabular-nums; }
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

function applyFilters() {
    var search = document.getElementById('search-input').value.toLowerCase();
    var visible = 0;
    document.querySelectorAll('.result-row').forEach(function(row) {
        var status = row.getAttribute('data-status');
        var name = row.getAttribute('data-sort-name');
        var category = row.getAttribute('data-sort-category');
        var matchFilter = (activeFilter === 'all') || (status === activeFilter);
        var matchSearch = !search || name.indexOf(search) !== -1 || category.indexOf(search) !== -1;
        if (matchFilter && matchSearch) {
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
