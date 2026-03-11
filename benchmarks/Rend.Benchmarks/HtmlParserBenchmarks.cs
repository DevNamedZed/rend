using System.Linq;
using BenchmarkDotNet.Attributes;
using Rend.Html.Parser;

namespace Rend.Benchmarks;

[MemoryDiagnoser]
public class HtmlParserBenchmarks
{
    private string _minimal = null!;
    private string _medium = null!;
    private string _complex = null!;

    [GlobalSetup]
    public void Setup()
    {
        _minimal = "<p>Hello</p>";

        _medium = $"""
            <!DOCTYPE html>
            <html>
            <head><title>Test</title></head>
            <body>
                <h1>Title</h1>
                {string.Concat(Enumerable.Range(0, 50).Select(i => $"<p>Paragraph {i} with <strong>bold</strong> and <a href=\"#\">links</a>.</p>\n"))}
                <table>
                    {string.Concat(Enumerable.Range(0, 10).Select(r => $"<tr>{string.Concat(Enumerable.Range(0, 5).Select(c => $"<td>Cell {r},{c}</td>"))}</tr>\n"))}
                </table>
            </body>
            </html>
            """;

        _complex = $"""
            <!DOCTYPE html>
            <html>
            <head><title>Complex Page</title></head>
            <body>
                <header><nav><ul>{string.Concat(Enumerable.Range(0, 8).Select(i => $"<li><a href=\"/page{i}\">Page {i}</a></li>"))}</ul></nav></header>
                <main>
                    {string.Concat(Enumerable.Range(0, 200).Select(i => $"<div class=\"card\"><h2>Card {i}</h2><p>Description for card {i} with <em>emphasis</em>.</p><img src=\"img{i}.png\" alt=\"Image {i}\"></div>\n"))}
                    <table>
                        {string.Concat(Enumerable.Range(0, 50).Select(r => $"<tr>{string.Concat(Enumerable.Range(0, 8).Select(c => $"<td>Data {r},{c}</td>"))}</tr>\n"))}
                    </table>
                    <form>
                        {string.Concat(Enumerable.Range(0, 20).Select(i => $"<label>Field {i}</label><input type=\"text\" name=\"field{i}\" value=\"value{i}\">\n"))}
                        <select>{string.Concat(Enumerable.Range(0, 50).Select(i => $"<option value=\"{i}\">Option {i}</option>"))}</select>
                        <textarea>Some text content</textarea>
                    </form>
                </main>
                <footer><p>Footer content</p></footer>
            </body>
            </html>
            """;
    }

    [Benchmark]
    public object Minimal() => HtmlParser.Parse(_minimal);

    [Benchmark]
    public object Medium() => HtmlParser.Parse(_medium);

    [Benchmark]
    public object Complex() => HtmlParser.Parse(_complex);
}
