using System.IO;
using BenchmarkDotNet.Attributes;
using iText.Html2pdf;
using QuestPDF.Infrastructure;

namespace Rend.Benchmarks;

/// <summary>
/// HTML→PDF comparison: Rend vs iText7 pdfHTML.
/// Both libraries parse HTML+CSS and produce PDF output.
/// All tests use HTML→PDF only.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class HtmlToPdfComparisonBenchmarks
{
    private string _simple = null!;
    private string _simple50 = null!;
    private string _table = null!;
    private string _styled = null!;
    private string _images = null!;

    [GlobalSetup]
    public void Setup()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        _simple = @"<!DOCTYPE html>
<html><body>
<h1>Hello World</h1>
<p>This is a simple paragraph with <strong>bold</strong> and <em>italic</em> text.</p>
<p>Second paragraph with more content to render.</p>
</body></html>";

        // Same 50-line content as low-level PDF test for direct comparison
        var sb50 = new System.Text.StringBuilder();
        sb50.Append("<!DOCTYPE html><html><body>");
        for (int i = 0; i < 50; i++)
            sb50.Append($"<p>Line {i + 1}: The quick brown fox jumps over the lazy dog.</p>");
        sb50.Append("</body></html>");
        _simple50 = sb50.ToString();

        var sb = new System.Text.StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><style>");
        sb.Append("table { border-collapse: collapse; width: 100%; } ");
        sb.Append("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; } ");
        sb.Append("th { background-color: #4CAF50; color: white; } ");
        sb.Append("tr:nth-child(even) { background-color: #f2f2f2; }");
        sb.Append("</style></head><body><h1>Report</h1><table>");
        sb.Append("<tr><th>ID</th><th>Product</th><th>Qty</th><th>Price</th><th>Total</th></tr>");
        for (int i = 0; i < 50; i++)
            sb.Append($"<tr><td>{i + 1}</td><td>Product {i}</td><td>{(i * 7) % 100}</td><td>${(i * 13) % 500}.99</td><td>${((i * 7) % 100) * ((i * 13) % 500)}</td></tr>");
        sb.Append("</table></body></html>");
        _table = sb.ToString();

        _styled = @"<!DOCTYPE html>
<html><head><style>
body { font-family: sans-serif; margin: 40px; color: #333; }
h1 { color: #1a73e8; border-bottom: 2px solid #1a73e8; padding-bottom: 8px; }
.box { background: #f0f4ff; padding: 20px; border-radius: 8px; margin: 20px 0; border: 1px solid #c0d0ff; }
.cols { display: flex; gap: 20px; }
.cols > div { flex: 1; padding: 16px; border: 1px solid #ddd; }
</style></head><body>
<h1>Styled Report</h1>
<div class=""box""><h2>Summary</h2><p>This is a styled summary block.</p></div>
<div class=""cols"">
<div><h3>Section A</h3><p>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p></div>
<div><h3>Section B</h3><p>Ut enim ad minim veniam, quis nostrud exercitation ullamco.</p></div>
</div>
<ul><li>Item 1</li><li>Item 2</li><li>Item 3</li><li>Item 4</li></ul>
</body></html>";

        // Inline images (1x1 red pixel PNG base64)
        var pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";
        var isb = new System.Text.StringBuilder();
        isb.Append("<!DOCTYPE html><html><head><style>");
        isb.Append(".gallery { display: flex; flex-wrap: wrap; gap: 10px; } ");
        isb.Append(".gallery img { width: 80px; height: 80px; border: 2px solid #ccc; }");
        isb.Append("</style></head><body><h1>Gallery</h1><div class=\"gallery\">");
        for (int i = 0; i < 20; i++)
            isb.Append($"<div><img src=\"data:image/png;base64,{pngBase64}\"/><p>Photo {i + 1}</p></div>");
        isb.Append("</div></body></html>");
        _images = isb.ToString();
    }

    // ──────────────────────────────────────────────
    // Simple HTML
    // ──────────────────────────────────────────────

    [Benchmark(Description = "Rend")]
    [BenchmarkCategory("Simple HTML")]
    public byte[] Simple_Rend() => Rend.Render.ToPdf(_simple);

    [Benchmark(Description = "iText pdfHTML")]
    [BenchmarkCategory("Simple HTML")]
    public byte[] Simple_iTextPdfHtml()
    {
        using var ms = new MemoryStream();
        HtmlConverter.ConvertToPdf(_simple, ms);
        return ms.ToArray();
    }

    // ──────────────────────────────────────────────
    // 50 Lines (same content as low-level PDF test)
    // ──────────────────────────────────────────────

    [Benchmark(Description = "Rend")]
    [BenchmarkCategory("50 Lines")]
    public byte[] Lines50_Rend() => Rend.Render.ToPdf(_simple50);

    [Benchmark(Description = "iText pdfHTML")]
    [BenchmarkCategory("50 Lines")]
    public byte[] Lines50_iTextPdfHtml()
    {
        using var ms = new MemoryStream();
        HtmlConverter.ConvertToPdf(_simple50, ms);
        return ms.ToArray();
    }

    // ──────────────────────────────────────────────
    // Table HTML
    // ──────────────────────────────────────────────

    [Benchmark(Description = "Rend")]
    [BenchmarkCategory("Table HTML")]
    public byte[] Table_Rend() => Rend.Render.ToPdf(_table);

    [Benchmark(Description = "iText pdfHTML")]
    [BenchmarkCategory("Table HTML")]
    public byte[] Table_iTextPdfHtml()
    {
        using var ms = new MemoryStream();
        HtmlConverter.ConvertToPdf(_table, ms);
        return ms.ToArray();
    }

    // ──────────────────────────────────────────────
    // Styled HTML (CSS)
    // ──────────────────────────────────────────────

    [Benchmark(Description = "Rend")]
    [BenchmarkCategory("Styled HTML")]
    public byte[] Styled_Rend() => Rend.Render.ToPdf(_styled);

    [Benchmark(Description = "iText pdfHTML")]
    [BenchmarkCategory("Styled HTML")]
    public byte[] Styled_iTextPdfHtml()
    {
        using var ms = new MemoryStream();
        HtmlConverter.ConvertToPdf(_styled, ms);
        return ms.ToArray();
    }

    // ──────────────────────────────────────────────
    // Images HTML
    // ──────────────────────────────────────────────

    [Benchmark(Description = "Rend")]
    [BenchmarkCategory("Images HTML")]
    public byte[] Images_Rend() => Rend.Render.ToPdf(_images);

    [Benchmark(Description = "iText pdfHTML")]
    [BenchmarkCategory("Images HTML")]
    public byte[] Images_iTextPdfHtml()
    {
        using var ms = new MemoryStream();
        HtmlConverter.ConvertToPdf(_images, ms);
        return ms.ToArray();
    }
}
