using System.IO;
using BenchmarkDotNet.Attributes;
using Rend.Pdf;

namespace Rend.Benchmarks;

[MemoryDiagnoser]
public class PdfWriterBenchmarks
{
    private byte[] _fontData = null!;

    [GlobalSetup]
    public void Setup()
    {
        var fontPath = "/mnt/c/Windows/Fonts/arial.ttf";
        if (!File.Exists(fontPath))
            fontPath = @"C:\Windows\Fonts\arial.ttf";
        if (!File.Exists(fontPath))
            fontPath = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";

        _fontData = File.Exists(fontPath) ? File.ReadAllBytes(fontPath) : System.Array.Empty<byte>();
    }

    [Benchmark]
    public byte[] EmptyDocument()
    {
        var doc = new PdfDocument();
        doc.AddPage(595.28f, 841.89f);
        using var ms = new MemoryStream();
        doc.Save(ms);
        return ms.ToArray();
    }

    [Benchmark]
    public byte[] SinglePageWithText()
    {
        if (_fontData.Length == 0) return System.Array.Empty<byte>();

        var doc = new PdfDocument();
        var page = doc.AddPage(595.28f, 841.89f);
        using var fontStream = new MemoryStream(_fontData);
        var font = doc.AddFont(fontStream);

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

    [Benchmark]
    public byte[] MultiPage()
    {
        if (_fontData.Length == 0) return System.Array.Empty<byte>();

        var doc = new PdfDocument();
        using var fontStream = new MemoryStream(_fontData);
        var font = doc.AddFont(fontStream);

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
}
