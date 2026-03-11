using System.IO;
using PdfSharp.Fonts;

namespace Rend.Benchmarks;

/// <summary>
/// Simple font resolver for PDFsharp 6.x that loads Arial from the filesystem.
/// </summary>
internal sealed class PdfSharpFontResolver : IFontResolver
{
    private readonly byte[] _arialData;
    private readonly byte[] _arialBoldData;

    public PdfSharpFontResolver()
    {
        var basePath = File.Exists("/mnt/c/Windows/Fonts/arial.ttf")
            ? "/mnt/c/Windows/Fonts"
            : @"C:\Windows\Fonts";

        var arialPath = Path.Combine(basePath, "arial.ttf");
        var arialBoldPath = Path.Combine(basePath, "arialbd.ttf");

        _arialData = File.Exists(arialPath) ? File.ReadAllBytes(arialPath) : System.Array.Empty<byte>();
        _arialBoldData = File.Exists(arialBoldPath) ? File.ReadAllBytes(arialBoldPath) : _arialData;
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        return new FontResolverInfo(isBold ? "ArialBold" : "Arial");
    }

    public byte[]? GetFont(string faceName)
    {
        return faceName == "ArialBold" ? _arialBoldData : _arialData;
    }
}
