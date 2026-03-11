using System.IO;
using BenchmarkDotNet.Attributes;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using PdfSharpCore = PdfSharp.Pdf;
using PdfSharpDrawing = PdfSharp.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Rend.Benchmarks;

/// <summary>
/// Fair comparison: all libraries use their low-level PDF writer API.
/// No HTML parsing, no CSS, no layout engines — just raw PDF generation.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class PdfComparisonBenchmarks
{
    private Rend.Pdf.PdfFontData _cachedFont = null!;
    private byte[] _fontData = null!;

    [GlobalSetup]
    public void Setup()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        PdfSharp.Fonts.GlobalFontSettings.FontResolver = new PdfSharpFontResolver();

        var fontPath = "/mnt/c/Windows/Fonts/arial.ttf";
        if (!File.Exists(fontPath))
            fontPath = @"C:\Windows\Fonts\arial.ttf";
        if (!File.Exists(fontPath))
            fontPath = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";

        _fontData = File.Exists(fontPath) ? File.ReadAllBytes(fontPath) : System.Array.Empty<byte>();
        if (_fontData.Length > 0)
            _cachedFont = Rend.Pdf.PdfFontData.FromBytes(_fontData);
    }

    // ──────────────────────────────────────────────
    // Simple: heading + 2 paragraphs
    // ──────────────────────────────────────────────

    [Benchmark(Description = "Rend")]
    [BenchmarkCategory("Simple")]
    public byte[] Simple_Rend()
    {
        var doc = new Rend.Pdf.PdfDocument();
        var page = doc.AddPage(595.28f, 841.89f);
        var font = doc.GetStandardFont(Rend.Pdf.StandardFont.Helvetica);

        var cs = page.Content;
        cs.BeginText();
        cs.SetFont(font, 24f);
        cs.MoveTextPosition(72f, 750f);
        cs.ShowText(font, "Hello World");
        cs.SetFont(font, 12f);
        cs.MoveTextPosition(72f, 720f);
        cs.ShowText(font, "This is a simple paragraph with bold and italic text.");
        cs.MoveTextPosition(72f, 700f);
        cs.ShowText(font, "Second paragraph with more content to render.");
        cs.EndText();

        using var ms = new MemoryStream();
        doc.Save(ms);
        return ms.ToArray();
    }

    [Benchmark(Description = "QuestPDF")]
    [BenchmarkCategory("Simple")]
    public byte[] Simple_QuestPDF()
    {
        using var ms = new MemoryStream();
        QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(72);
                page.Content().Column(col =>
                {
                    col.Item().Text("Hello World").FontSize(24);
                    col.Item().Text("This is a simple paragraph with bold and italic text.");
                    col.Item().Text("Second paragraph with more content to render.");
                });
            });
        }).GeneratePdf(ms);
        return ms.ToArray();
    }

    [Benchmark(Description = "PDFsharp")]
    [BenchmarkCategory("Simple")]
    public byte[] Simple_PDFsharp()
    {
        var doc = new PdfSharpCore.PdfDocument();
        var page = doc.AddPage();
        page.Size = PdfSharp.PageSize.A4;
        var gfx = PdfSharpDrawing.XGraphics.FromPdfPage(page);

        gfx.DrawString("Hello World", new PdfSharpDrawing.XFont("Arial", 24),
            PdfSharpDrawing.XBrushes.Black, 72, 72);
        gfx.DrawString("This is a simple paragraph with bold and italic text.",
            new PdfSharpDrawing.XFont("Arial", 12), PdfSharpDrawing.XBrushes.Black, 72, 100);
        gfx.DrawString("Second paragraph with more content to render.",
            new PdfSharpDrawing.XFont("Arial", 12), PdfSharpDrawing.XBrushes.Black, 72, 116);

        using var ms = new MemoryStream();
        doc.Save(ms);
        return ms.ToArray();
    }

    [Benchmark(Description = "iText7")]
    [BenchmarkCategory("Simple")]
    public byte[] Simple_iText7()
    {
        using var ms = new MemoryStream();
        using var writer = new PdfWriter(ms);
        using var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
        using var document = new iText.Layout.Document(pdf);

        document.Add(new Paragraph("Hello World").SetFontSize(24));
        document.Add(new Paragraph("This is a simple paragraph with bold and italic text."));
        document.Add(new Paragraph("Second paragraph with more content to render."));

        document.Close();
        return ms.ToArray();
    }

    // ──────────────────────────────────────────────
    // 50 Lines: Single page, 50 lines of text
    // ──────────────────────────────────────────────

    [Benchmark(Description = "Rend")]
    [BenchmarkCategory("50 Lines")]
    public byte[] Lines50_Rend()
    {
        if (_cachedFont == null) return System.Array.Empty<byte>();

        var opts = new Rend.Pdf.PdfDocumentOptions { Compression = Rend.Pdf.PdfCompression.FlateFast };
        var doc = new Rend.Pdf.PdfDocument(opts);
        var page = doc.AddPage(595.28f, 841.89f);
        var font = doc.AddFont(_cachedFont);

        var cs = page.Content;
        cs.BeginText();
        cs.SetFont(font, 12f);
        for (int i = 0; i < 50; i++)
        {
            cs.MoveTextPosition(72f, 750f - i * 14f);
            cs.ShowText(font, $"Line {i + 1}: The quick brown fox jumps over the lazy dog.");
        }
        cs.EndText();

        using var ms = new MemoryStream();
        doc.Save(ms);
        return ms.ToArray();
    }

    [Benchmark(Description = "QuestPDF")]
    [BenchmarkCategory("50 Lines")]
    public byte[] Lines50_QuestPDF()
    {
        using var ms = new MemoryStream();
        QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(72);
                page.Content().Column(col =>
                {
                    for (int i = 0; i < 50; i++)
                        col.Item().Text($"Line {i + 1}: The quick brown fox jumps over the lazy dog.");
                });
            });
        }).GeneratePdf(ms);
        return ms.ToArray();
    }

    [Benchmark(Description = "PDFsharp")]
    [BenchmarkCategory("50 Lines")]
    public byte[] Lines50_PDFsharp()
    {
        var doc = new PdfSharpCore.PdfDocument();
        var page = doc.AddPage();
        page.Size = PdfSharp.PageSize.A4;
        var gfx = PdfSharpDrawing.XGraphics.FromPdfPage(page);
        var font = new PdfSharpDrawing.XFont("Arial", 12);

        for (int i = 0; i < 50; i++)
            gfx.DrawString($"Line {i + 1}: The quick brown fox jumps over the lazy dog.",
                font, PdfSharpDrawing.XBrushes.Black, 72, 72 + i * 14);

        using var ms = new MemoryStream();
        doc.Save(ms);
        return ms.ToArray();
    }

    [Benchmark(Description = "iText7")]
    [BenchmarkCategory("50 Lines")]
    public byte[] Lines50_iText7()
    {
        using var ms = new MemoryStream();
        using var writer = new PdfWriter(ms);
        using var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
        using var document = new iText.Layout.Document(pdf);

        for (int i = 0; i < 50; i++)
            document.Add(new Paragraph($"Line {i + 1}: The quick brown fox jumps over the lazy dog."));

        document.Close();
        return ms.ToArray();
    }

    // ──────────────────────────────────────────────
    // Table: 50 rows x 5 columns
    // ──────────────────────────────────────────────

    [Benchmark(Description = "Rend")]
    [BenchmarkCategory("Table")]
    public byte[] Table_Rend()
    {
        if (_cachedFont == null) return System.Array.Empty<byte>();

        var opts = new Rend.Pdf.PdfDocumentOptions { Compression = Rend.Pdf.PdfCompression.FlateFast };
        var doc = new Rend.Pdf.PdfDocument(opts);
        var page = doc.AddPage(595.28f, 841.89f);
        var font = doc.AddFont(_cachedFont);

        var table = new Rend.Pdf.PdfTableBuilder(font, 10f);
        table.SetBounds(72f, 72f, 595.28f - 144f);
        table.SetPadding(4f);
        table.SetBorder(0.5f);
        table.AddColumn(40);
        table.AddColumn(150);
        table.AddColumn(40);
        table.AddColumn(60);
        table.AddColumn(70);

        table.AddRow("ID", "Product", "Qty", "Price", "Total");
        for (int i = 0; i < 50; i++)
        {
            table.AddRow(
                $"{i + 1}",
                $"Product {i}",
                $"{(i * 7) % 100}",
                $"${(i * 13) % 500}.99",
                $"${((i * 7) % 100) * ((i * 13) % 500)}");
        }

        table.Draw(page.Content);

        using var ms = new MemoryStream();
        doc.Save(ms);
        return ms.ToArray();
    }

    [Benchmark(Description = "QuestPDF")]
    [BenchmarkCategory("Table")]
    public byte[] Table_QuestPDF()
    {
        using var ms = new MemoryStream();
        QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(72);
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(40);
                        cols.RelativeColumn(3);
                        cols.ConstantColumn(40);
                        cols.ConstantColumn(60);
                        cols.ConstantColumn(70);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#4CAF50").Text("ID").FontColor("#fff");
                        header.Cell().Background("#4CAF50").Text("Product").FontColor("#fff");
                        header.Cell().Background("#4CAF50").Text("Qty").FontColor("#fff");
                        header.Cell().Background("#4CAF50").Text("Price").FontColor("#fff");
                        header.Cell().Background("#4CAF50").Text("Total").FontColor("#fff");
                    });

                    for (int i = 0; i < 50; i++)
                    {
                        table.Cell().Border(1).Padding(4).Text($"{i + 1}");
                        table.Cell().Border(1).Padding(4).Text($"Product {i}");
                        table.Cell().Border(1).Padding(4).Text($"{(i * 7) % 100}");
                        table.Cell().Border(1).Padding(4).Text($"${(i * 13) % 500}.99");
                        table.Cell().Border(1).Padding(4).Text($"${((i * 7) % 100) * ((i * 13) % 500)}");
                    }
                });
            });
        }).GeneratePdf(ms);
        return ms.ToArray();
    }

    [Benchmark(Description = "iText7")]
    [BenchmarkCategory("Table")]
    public byte[] Table_iText7()
    {
        using var ms = new MemoryStream();
        using var writer = new PdfWriter(ms);
        using var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
        using var document = new iText.Layout.Document(pdf);

        var table = new Table(5).UseAllAvailableWidth();
        string[] headers = { "ID", "Product", "Qty", "Price", "Total" };
        foreach (var h in headers)
            table.AddHeaderCell(new Cell().Add(new Paragraph(h)));

        for (int i = 0; i < 50; i++)
        {
            table.AddCell($"{i + 1}");
            table.AddCell($"Product {i}");
            table.AddCell($"{(i * 7) % 100}");
            table.AddCell($"${(i * 13) % 500}.99");
            table.AddCell($"${((i * 7) % 100) * ((i * 13) % 500)}");
        }

        document.Add(table);
        document.Close();
        return ms.ToArray();
    }

    // ──────────────────────────────────────────────
    // MultiPage: 10 pages, 60 lines each
    // ──────────────────────────────────────────────

    [Benchmark(Description = "Rend")]
    [BenchmarkCategory("MultiPage")]
    public byte[] MultiPage_Rend()
    {
        if (_cachedFont == null) return System.Array.Empty<byte>();

        var opts = new Rend.Pdf.PdfDocumentOptions { Compression = Rend.Pdf.PdfCompression.FlateFast };
        var doc = new Rend.Pdf.PdfDocument(opts);
        var font = doc.AddFont(_cachedFont);

        for (int p = 0; p < 10; p++)
        {
            var page = doc.AddPage(595.28f, 841.89f);
            var cs = page.Content;
            cs.BeginText();
            cs.SetFont(font, 11f);
            for (int i = 0; i < 60; i++)
            {
                cs.MoveTextPosition(72f, 770f - i * 12f);
                cs.ShowText(font, $"Page {p + 1}, Line {i + 1}: Sample text for benchmarking.");
            }
            cs.EndText();
        }

        using var ms = new MemoryStream();
        doc.Save(ms);
        return ms.ToArray();
    }

    [Benchmark(Description = "QuestPDF")]
    [BenchmarkCategory("MultiPage")]
    public byte[] MultiPage_QuestPDF()
    {
        using var ms = new MemoryStream();
        QuestPDF.Fluent.Document.Create(container =>
        {
            for (int p = 0; p < 10; p++)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(72);
                    int pageNum = p + 1;
                    page.Content().Column(col =>
                    {
                        for (int i = 0; i < 60; i++)
                            col.Item().Text($"Page {pageNum}, Line {i + 1}: Sample text for benchmarking.").FontSize(11);
                    });
                });
            }
        }).GeneratePdf(ms);
        return ms.ToArray();
    }

    [Benchmark(Description = "PDFsharp")]
    [BenchmarkCategory("MultiPage")]
    public byte[] MultiPage_PDFsharp()
    {
        var doc = new PdfSharpCore.PdfDocument();
        var font = new PdfSharpDrawing.XFont("Arial", 11);

        for (int p = 0; p < 10; p++)
        {
            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            var gfx = PdfSharpDrawing.XGraphics.FromPdfPage(page);

            for (int i = 0; i < 60; i++)
                gfx.DrawString($"Page {p + 1}, Line {i + 1}: Sample text for benchmarking.",
                    font, PdfSharpDrawing.XBrushes.Black, 72, 72 + i * 12);
        }

        using var ms = new MemoryStream();
        doc.Save(ms);
        return ms.ToArray();
    }

    [Benchmark(Description = "iText7")]
    [BenchmarkCategory("MultiPage")]
    public byte[] MultiPage_iText7()
    {
        using var ms = new MemoryStream();
        using var writer = new PdfWriter(ms);
        using var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
        using var document = new iText.Layout.Document(pdf);

        for (int p = 0; p < 10; p++)
        {
            if (p > 0) document.Add(new AreaBreak());
            for (int i = 0; i < 60; i++)
                document.Add(new Paragraph($"Page {p + 1}, Line {i + 1}: Sample text for benchmarking.")
                    .SetFontSize(11));
        }

        document.Close();
        return ms.ToArray();
    }
}
