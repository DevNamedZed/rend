using System;
using System.Diagnostics;
using System.IO;
using BenchmarkDotNet.Attributes;
using Rend;
using Rend.Adapters;
using Rend.Css;
using Rend.Fonts;
using Rend.Html.Parser;
using Rend.Internal;
using Rend.Layout;
using Rend.Output.Pdf;
using Rend.Rendering;
using Rend.Style;
using Rend.Text;

namespace Rend.Benchmarks;

/// <summary>
/// Profiles individual pipeline stages to identify bottlenecks.
/// Not a comparison benchmark — Rend-only with per-stage timing.
/// </summary>
[MemoryDiagnoser]
public class PipelineProfileBenchmarks
{
    private string _simple = null!;
    private string _table = null!;
    private IFontProvider _fontProvider = null!;
    private HarfBuzzTextShaper _textShaper = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simple = @"<!DOCTYPE html>
<html><body>
<h1>Hello World</h1>
<p>This is a simple paragraph with <strong>bold</strong> and <em>italic</em> text.</p>
<p>Second paragraph with more content to render.</p>
</body></html>";

        var sb = new System.Text.StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><style>");
        sb.Append("table { border-collapse: collapse; width: 100%; } ");
        sb.Append("th, td { border: 1px solid #ddd; padding: 8px; } ");
        sb.Append("</style></head><body><table>");
        sb.Append("<tr><th>ID</th><th>Product</th><th>Qty</th><th>Price</th><th>Total</th></tr>");
        for (int i = 0; i < 50; i++)
        {
            sb.Append($"<tr><td>{i + 1}</td><td>Product {i}</td><td>{(i * 7) % 100}</td><td>${(i * 13) % 500}.99</td><td>${((i * 7) % 100) * ((i * 13) % 500)}</td></tr>");
        }
        sb.Append("</table></body></html>");
        _table = sb.ToString();

        // Pre-warm font provider and text shaper (these are Lazy singletons in real pipeline)
        var collection = new FontCollection();
        try { collection.RegisterFromResolver(new SystemFontResolver()); } catch { }
        _fontProvider = collection;
        _textShaper = new HarfBuzzTextShaper();
        _textShaper.FallbackFontProvider = _fontProvider;
    }

    [Benchmark(Description = "Simple_FullPipeline")]
    public byte[] SimpleFullPipeline() => Rend.Render.ToPdf(_simple);

    [Benchmark(Description = "Simple_ParseOnly")]
    public object SimpleParseOnly() => HtmlParser.Parse(_simple);

    [Benchmark(Description = "Simple_ParseAndStyle")]
    public object SimpleParseAndStyle()
    {
        var document = HtmlParser.Parse(_simple);
        var stylesheets = HtmlStyleExtractor.Extract(document);
        var selectorMatcher = new SelectorMatcherAdapter();
        var resolverOptions = new StyleResolverOptions
        {
            MediaType = "print",
            ViewportWidth = 523.28f,
            ViewportHeight = 769.89f,
            DefaultFontSize = 16f,
            ApplyUserAgentStyles = true,
        };
        var styleResolver = new StyleResolver(selectorMatcher, resolverOptions);
        var treeBuilder = new StyleTreeBuilder(styleResolver, _fontProvider);
        return treeBuilder.Build(document, stylesheets);
    }

    [Benchmark(Description = "Simple_ParseStyleLayout")]
    public object SimpleParseStyleLayout()
    {
        var document = HtmlParser.Parse(_simple);
        var stylesheets = HtmlStyleExtractor.Extract(document);
        var selectorMatcher = new SelectorMatcherAdapter();
        var resolverOptions = new StyleResolverOptions
        {
            MediaType = "print",
            ViewportWidth = 523.28f,
            ViewportHeight = 769.89f,
            DefaultFontSize = 16f,
            ApplyUserAgentStyles = true,
        };
        var styleResolver = new StyleResolver(selectorMatcher, resolverOptions);
        var treeBuilder = new StyleTreeBuilder(styleResolver, _fontProvider);
        var styledTree = treeBuilder.Build(document, stylesheets);
        styledTree.PageStyle.PageSize = new Rend.Core.Values.SizeF(595.28f, 841.89f);
        styledTree.PageStyle.MarginTop = 36f;
        styledTree.PageStyle.MarginRight = 36f;
        styledTree.PageStyle.MarginBottom = 36f;
        styledTree.PageStyle.MarginLeft = 36f;

        var layoutEngine = new LayoutEngine(_fontProvider, _textShaper);
        var layoutOptions = new LayoutOptions
        {
            PageSize = new Rend.Core.Values.SizeF(595.28f, 841.89f),
            MarginTop = 36f, MarginRight = 36f, MarginBottom = 36f, MarginLeft = 36f,
            DefaultFontSize = 16f,
            Paginate = true,
        };
        return layoutEngine.Layout(styledTree, layoutOptions);
    }

    [Benchmark(Description = "Simple_NoEmbed")]
    public byte[] SimpleNoEmbed()
    {
        var options = new RenderOptions
        {
            PdfOptions = new Rend.Pdf.PdfDocumentOptions
            {
                Compression = Rend.Pdf.PdfCompression.FlateFast,
                FontEmbedMode = Rend.Pdf.FontEmbedMode.None
            }
        };
        return new HtmlRenderer().ToPdf(_simple, options);
    }

    [Benchmark(Description = "Table_FullPipeline")]
    public byte[] TableFullPipeline() => Rend.Render.ToPdf(_table);
}
