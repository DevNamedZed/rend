using BenchmarkDotNet.Attributes;
using Rend.Css;

namespace Rend.Benchmarks;

[MemoryDiagnoser]
public class CssParserBenchmarks
{
    private string _minimal = null!;
    private string _medium = null!;
    private string _complex = null!;

    [GlobalSetup]
    public void Setup()
    {
        _minimal = "p { color: red; font-size: 14px; }";

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            sb.AppendLine($".class-{i} {{");
            sb.AppendLine($"  color: #{i % 16:x}{(i * 3) % 16:x}{(i * 7) % 16:x};");
            sb.AppendLine($"  font-size: {12 + i % 8}px;");
            sb.AppendLine($"  margin: {i % 20}px {i % 15}px;");
            sb.AppendLine($"  padding: {i % 10}px;");
            sb.AppendLine("  border: 1px solid #ccc;");
            sb.AppendLine(i % 3 == 0 ? "  display: flex;" : i % 3 == 1 ? "  display: grid;" : "  display: block;");
            sb.AppendLine("  background: linear-gradient(to bottom, #fff, #eee);");
            sb.AppendLine("}");
        }
        _medium = sb.ToString();

        sb.Clear();
        sb.AppendLine(":root { --primary: #1a73e8; --secondary: #5f6368; --spacing: 16px; }");
        for (int i = 0; i < 200; i++)
        {
            sb.AppendLine($".component-{i} {{");
            sb.AppendLine("  color: var(--primary);");
            sb.AppendLine($"  font-size: calc(14px + {i % 4}px);");
            sb.AppendLine("  margin: var(--spacing);");
            sb.AppendLine($"  padding: {i % 20}px;");
            sb.AppendLine("  border: 1px solid var(--secondary);");
            sb.AppendLine($"  border-radius: {i % 8}px;");
            sb.AppendLine("  box-shadow: 0 2px 4px rgba(0,0,0,0.1);");
            sb.AppendLine("}");
            sb.AppendLine($".component-{i}:hover {{");
            sb.AppendLine("  transform: translateY(-2px);");
            sb.AppendLine("  box-shadow: 0 4px 8px rgba(0,0,0,0.2);");
            sb.AppendLine("}");
            sb.AppendLine($".component-{i} > .child {{");
            sb.AppendLine("  display: flex; align-items: center; gap: 8px;");
            sb.AppendLine("}");
        }
        sb.AppendLine("@media (max-width: 768px) {");
        for (int i = 0; i < 50; i++)
            sb.AppendLine($"  .component-{i} {{ font-size: 12px; padding: 8px; }}");
        sb.AppendLine("}");
        _complex = sb.ToString();
    }

    [Benchmark]
    public object Minimal() => CssParser.Parse(_minimal);

    [Benchmark]
    public object Medium() => CssParser.Parse(_medium);

    [Benchmark]
    public object Complex() => CssParser.Parse(_complex);
}
