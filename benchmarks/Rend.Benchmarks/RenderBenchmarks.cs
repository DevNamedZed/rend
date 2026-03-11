using System;
using BenchmarkDotNet.Attributes;
using Rend;

namespace Rend.Benchmarks;

/// <summary>
/// Full HTML→PDF pipeline benchmarks (Rend-only — no competitors do this natively).
/// Measures: HTML parse → CSS resolve → layout → text shaping → PDF output.
/// </summary>
[MemoryDiagnoser]
public class RenderBenchmarks
{
    private string _simpleParagraph = null!;
    private string _tableReport = null!;
    private string _flexLayout = null!;
    private string _gridDashboard = null!;
    private string _styledPage = null!;
    private string _imageHeavy = null!;
    private string _nestedLayout = null!;
    private string _longDocument = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simpleParagraph = @"<!DOCTYPE html>
<html><body>
<h1>Hello World</h1>
<p>This is a simple paragraph with <strong>bold</strong> and <em>italic</em> text.</p>
</body></html>";

        var sb = new System.Text.StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><style>");
        sb.Append("table { border-collapse: collapse; width: 100%; } ");
        sb.Append("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; font-size: 11px; } ");
        sb.Append("th { background-color: #4CAF50; color: white; } ");
        sb.Append("tr:nth-child(even) { background-color: #f2f2f2; }");
        sb.Append("</style></head><body><h1>Sales Report</h1><table>");
        sb.Append("<thead><tr><th>ID</th><th>Product</th><th>Qty</th><th>Price</th><th>Total</th></tr></thead><tbody>");
        for (int i = 0; i < 50; i++)
            sb.Append($"<tr><td>{i + 1}</td><td>Product {i}</td><td>{(i * 7) % 100}</td><td>${(i * 13) % 500}.99</td><td>${((i * 7) % 100) * ((i * 13) % 500)}</td></tr>");
        sb.Append("</tbody></table></body></html>");
        _tableReport = sb.ToString();

        sb.Clear();
        sb.Append("<!DOCTYPE html><html><head><style>");
        sb.Append(".container { display: flex; flex-wrap: wrap; gap: 16px; padding: 16px; } ");
        sb.Append(".card { flex: 1 1 200px; border: 1px solid #ddd; border-radius: 8px; padding: 16px; } ");
        sb.Append(".card h3 { margin: 0 0 8px 0; color: #333; } ");
        sb.Append(".card p { margin: 0; color: #666; font-size: 14px; }");
        sb.Append("</style></head><body><h1>Dashboard</h1><div class=\"container\">");
        for (int i = 0; i < 30; i++)
            sb.Append($"<div class=\"card\"><h3>Card {i}</h3><p>Description for card number {i}.</p></div>");
        sb.Append("</div></body></html>");
        _flexLayout = sb.ToString();

        sb.Clear();
        sb.Append("<!DOCTYPE html><html><head><style>");
        sb.Append(".grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; padding: 16px; } ");
        sb.Append(".item { background: #f5f5f5; border: 1px solid #ddd; border-radius: 4px; padding: 12px; } ");
        sb.Append(".span-2 { grid-column: span 2; } ");
        sb.Append(".span-row { grid-row: span 2; }");
        sb.Append("</style></head><body><h1>Grid Layout</h1><div class=\"grid\">");
        for (int i = 0; i < 24; i++)
        {
            string cls = "item" + (i % 5 == 0 ? " span-2" : "") + (i % 7 == 0 ? " span-row" : "");
            sb.Append($"<div class=\"{cls}\"><h3>Item {i}</h3><p>Grid item content {i}</p></div>");
        }
        sb.Append("</div></body></html>");
        _gridDashboard = sb.ToString();

        _styledPage = @"<!DOCTYPE html>
<html><head><style>
body { font-family: Arial, sans-serif; margin: 40px; color: #333; }
h1 { color: #1a73e8; border-bottom: 2px solid #1a73e8; padding-bottom: 8px; }
.highlight { background: linear-gradient(135deg, #667eea, #764ba2); color: white; padding: 20px; border-radius: 8px; margin: 20px 0; }
.two-col { display: flex; gap: 20px; }
.two-col > div { flex: 1; padding: 16px; border: 1px solid #ddd; border-radius: 4px; }
ul { columns: 2; }
</style></head><body>
<h1>Styled Report</h1>
<div class=""highlight""><h2>Summary</h2><p>This is a styled summary block with gradients and rounded corners.</p></div>
<div class=""two-col"">
<div><h3>Section A</h3><p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore.</p></div>
<div><h3>Section B</h3><p>Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo.</p></div>
</div>
<ul><li>Item 1</li><li>Item 2</li><li>Item 3</li><li>Item 4</li><li>Item 5</li><li>Item 6</li></ul>
</body></html>";

        // Create a 1x1 red pixel PNG as base64 for inline images
        var pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";

        var isb = new System.Text.StringBuilder();
        isb.Append("<!DOCTYPE html><html><head><style>");
        isb.Append("body { font-family: sans-serif; } ");
        isb.Append(".gallery { display: flex; flex-wrap: wrap; gap: 10px; } ");
        isb.Append(".gallery img { width: 80px; height: 80px; border: 2px solid #ccc; border-radius: 4px; } ");
        isb.Append(".caption { text-align: center; font-size: 12px; color: #666; }");
        isb.Append("</style></head><body><h1>Image Gallery</h1><div class=\"gallery\">");
        for (int i = 0; i < 20; i++)
            isb.Append($"<div><img src=\"data:image/png;base64,{pngBase64}\"/><div class=\"caption\">Photo {i + 1}</div></div>");
        isb.Append("</div></body></html>");
        _imageHeavy = isb.ToString();

        // Deeply nested layout: flexbox inside grid inside flexbox
        var nsb = new System.Text.StringBuilder();
        nsb.Append("<!DOCTYPE html><html><head><style>");
        nsb.Append("body { margin: 20px; font-family: sans-serif; } ");
        nsb.Append(".outer { display: flex; gap: 20px; } ");
        nsb.Append(".sidebar { width: 200px; } .sidebar ul { list-style: none; padding: 0; } ");
        nsb.Append(".sidebar li { padding: 8px; border-bottom: 1px solid #eee; } ");
        nsb.Append(".main { flex: 1; display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; } ");
        nsb.Append(".widget { border: 1px solid #ddd; border-radius: 6px; padding: 12px; } ");
        nsb.Append(".widget-inner { display: flex; justify-content: space-between; align-items: center; } ");
        nsb.Append(".stat { font-size: 24px; font-weight: bold; color: #1a73e8; }");
        nsb.Append("</style></head><body>");
        nsb.Append("<div class=\"outer\"><div class=\"sidebar\"><h3>Navigation</h3><ul>");
        for (int i = 0; i < 10; i++)
            nsb.Append($"<li>Menu Item {i + 1}</li>");
        nsb.Append("</ul></div><div class=\"main\">");
        for (int i = 0; i < 12; i++)
            nsb.Append($"<div class=\"widget\"><h4>Widget {i + 1}</h4><div class=\"widget-inner\"><span class=\"stat\">{(i + 1) * 42}</span><span>units</span></div><p>Details for widget {i + 1}.</p></div>");
        nsb.Append("</div></div></body></html>");
        _nestedLayout = nsb.ToString();

        // Long document with multiple sections
        var lsb = new System.Text.StringBuilder();
        lsb.Append("<!DOCTYPE html><html><head><style>");
        lsb.Append("body { font-family: serif; margin: 40px; line-height: 1.6; color: #333; } ");
        lsb.Append("h1 { color: #1a1a1a; border-bottom: 2px solid #333; } ");
        lsb.Append("h2 { color: #444; margin-top: 24px; } ");
        lsb.Append("blockquote { border-left: 4px solid #ccc; margin: 16px 0; padding: 8px 16px; color: #666; } ");
        lsb.Append("code { background: #f4f4f4; padding: 2px 6px; border-radius: 3px; font-family: monospace; }");
        lsb.Append("</style></head><body><h1>Technical Document</h1>");
        for (int s = 0; s < 10; s++)
        {
            lsb.Append($"<h2>Chapter {s + 1}: Topic Area</h2>");
            for (int p = 0; p < 5; p++)
                lsb.Append("<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris.</p>");
            lsb.Append("<blockquote>This is an important note that should be highlighted in the document.</blockquote>");
            lsb.Append("<p>Here is some <code>inline code</code> within a paragraph of text.</p>");
        }
        lsb.Append("</body></html>");
        _longDocument = lsb.ToString();
    }

    [Benchmark(Description = "Simple paragraph")]
    public byte[] Pdf_Simple() => Render.ToPdf(_simpleParagraph);

    [Benchmark(Description = "Table 50x5")]
    public byte[] Pdf_Table() => Render.ToPdf(_tableReport);

    [Benchmark(Description = "Flex 30 cards")]
    public byte[] Pdf_Flex() => Render.ToPdf(_flexLayout);

    [Benchmark(Description = "Grid 24 items")]
    public byte[] Pdf_Grid() => Render.ToPdf(_gridDashboard);

    [Benchmark(Description = "Styled page")]
    public byte[] Pdf_Styled() => Render.ToPdf(_styledPage);

    [Benchmark(Description = "20 inline images")]
    public byte[] Pdf_Images() => Render.ToPdf(_imageHeavy);

    [Benchmark(Description = "Nested flex+grid")]
    public byte[] Pdf_Nested() => Render.ToPdf(_nestedLayout);

    [Benchmark(Description = "Long document (10 chapters)")]
    public byte[] Pdf_Long() => Render.ToPdf(_longDocument);

    [Benchmark(Description = "Simple → Image")]
    public byte[] Image_Simple() => Render.ToImage(_simpleParagraph);

    [Benchmark(Description = "Styled → Image")]
    public byte[] Image_Styled() => Render.ToImage(_styledPage);
}
