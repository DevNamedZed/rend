using System;
using System.Diagnostics;
using System.IO;
using Rend;

namespace Rend.PdfCli
{
    internal class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 1;
            }

            string command = args[0].ToLowerInvariant();

            switch (command)
            {
                case "render":
                    return RunRender(args);
                case "info":
                    return RunInfo(args);
                case "text":
                    return RunText(args);
                case "test":
                    return RunTest(args);
                case "dump":
                    return DumpCommand.Run(args);
                case "compare":
                    return CompareCommand.Run(args);
                case "type1dump":
                    return Type1DumpCommand.Run(args);
                case "fonts":
                    return FontDumpCommand.Run(args);
                default:
                    Console.Error.WriteLine($"Unknown command: {command}");
                    PrintUsage();
                    return 1;
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  render <file.pdf> [--out <dir>] [--dpi <150>] [--page <0>]");
            Console.WriteLine("  info <file.pdf>");
            Console.WriteLine("  text <file.pdf> [--page <0>]");
            Console.WriteLine("  test <dir-or-file> [--out <dir>] [--dpi <150>]");
        }

        static int RunRender(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: render <file.pdf> [--out <dir>] [--dpi <150>] [--page <0>]");
                return 1;
            }

            string filePath = args[1];
            string outputDir = Path.GetDirectoryName(filePath) ?? ".";
            int dpi = 150;
            int pageIndex = -1;

            for (int i = 2; i < args.Length - 1; i++)
            {
                switch (args[i])
                {
                    case "--out": outputDir = args[++i]; break;
                    case "--dpi": dpi = int.Parse(args[++i]); break;
                    case "--page": pageIndex = int.Parse(args[++i]); break;
                }
            }

            Directory.CreateDirectory(outputDir);

            try
            {
                using var reader = new PdfReader(filePath);
                string baseName = Path.GetFileNameWithoutExtension(filePath);

                int startPage = pageIndex >= 0 ? pageIndex : 0;
                int endPage = pageIndex >= 0 ? pageIndex + 1 : reader.PageCount;

                Console.WriteLine($"[{baseName}] {reader.PageCount} page(s), rendering at {dpi} DPI");

                for (int i = startPage; i < endPage; i++)
                {
                    var stopwatch = Stopwatch.StartNew();
                    byte[] png = reader.RenderPage(i, dpi);
                    stopwatch.Stop();

                    string outPath = Path.Combine(outputDir, $"{baseName}_page{i + 1}.png");
                    File.WriteAllBytes(outPath, png);

                    var pageInfo = reader.GetPageInfo(i);
                    Console.WriteLine($"  Page {i + 1}: {pageInfo.Width:F0}x{pageInfo.Height:F0}pt, " +
                                      $"rot={pageInfo.Rotation}, {png.Length:N0} bytes, {stopwatch.ElapsedMilliseconds}ms -> {outPath}");

                    foreach (var warning in reader.RenderWarnings)
                    {
                        Console.WriteLine($"    WARN: {warning}");
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR [{Path.GetFileName(filePath)}]: {ex.Message}");
                return 1;
            }
        }

        static int RunInfo(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: info <file.pdf>");
                return 1;
            }

            string filePath = args[1];

            try
            {
                using var reader = new PdfReader(filePath);
                var metadata = reader.Metadata;

                Console.WriteLine($"File:         {Path.GetFileName(filePath)}");
                Console.WriteLine($"Pages:        {reader.PageCount}");
                Console.WriteLine($"PDF Version:  {(string.IsNullOrEmpty(metadata.PdfVersion) ? "(unknown)" : metadata.PdfVersion)}");
                Console.WriteLine($"Title:        {(string.IsNullOrEmpty(metadata.Title) ? "(none)" : metadata.Title)}");
                Console.WriteLine($"Author:       {(string.IsNullOrEmpty(metadata.Author) ? "(none)" : metadata.Author)}");
                Console.WriteLine($"Subject:      {(string.IsNullOrEmpty(metadata.Subject) ? "(none)" : metadata.Subject)}");
                Console.WriteLine($"Creator:      {(string.IsNullOrEmpty(metadata.Creator) ? "(none)" : metadata.Creator)}");
                Console.WriteLine($"Producer:     {(string.IsNullOrEmpty(metadata.Producer) ? "(none)" : metadata.Producer)}");
                Console.WriteLine($"Created:      {(string.IsNullOrEmpty(metadata.CreationDate) ? "(none)" : metadata.CreationDate)}");
                Console.WriteLine($"Modified:     {(string.IsNullOrEmpty(metadata.ModificationDate) ? "(none)" : metadata.ModificationDate)}");
                Console.WriteLine($"Encrypted:    {metadata.IsEncrypted}");
                Console.WriteLine($"Signed:       {metadata.IsSigned}");

                if (reader.ParseWarnings.Count > 0)
                {
                    Console.WriteLine($"Parse warnings ({reader.ParseWarnings.Count}):");
                    foreach (var warning in reader.ParseWarnings)
                    {
                        Console.WriteLine($"  {warning}");
                    }
                }

                for (int i = 0; i < reader.PageCount; i++)
                {
                    var pageInfo = reader.GetPageInfo(i);
                    Console.WriteLine($"  Page {i + 1}: {pageInfo.Width:F0}x{pageInfo.Height:F0}pt, rotation={pageInfo.Rotation}");
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: {ex.Message}");
                return 1;
            }
        }

        static int RunText(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: text <file.pdf> [--page <0>]");
                return 1;
            }

            string filePath = args[1];
            int pageIndex = -1;

            for (int i = 2; i < args.Length - 1; i++)
            {
                if (args[i] == "--page")
                {
                    pageIndex = int.Parse(args[++i]);
                }
            }

            try
            {
                using var reader = new PdfReader(filePath);

                int startPage = pageIndex >= 0 ? pageIndex : 0;
                int endPage = pageIndex >= 0 ? pageIndex + 1 : reader.PageCount;

                for (int i = startPage; i < endPage; i++)
                {
                    if (reader.PageCount > 1)
                    {
                        Console.WriteLine($"--- Page {i + 1} ---");
                    }
                    Console.WriteLine(reader.ExtractText(i));
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: {ex.Message}");
                return 1;
            }
        }

        static int RunTest(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: test <dir-or-file> [--out <dir>] [--dpi <150>]");
                return 1;
            }

            string inputPath = args[1];
            string outputDir = "";
            int dpi = 150;

            for (int i = 2; i < args.Length - 1; i++)
            {
                switch (args[i])
                {
                    case "--out": outputDir = args[++i]; break;
                    case "--dpi": dpi = int.Parse(args[++i]); break;
                }
            }

            string[] files;
            if (Directory.Exists(inputPath))
            {
                files = Directory.GetFiles(inputPath, "*.pdf");
                Array.Sort(files);
                if (string.IsNullOrEmpty(outputDir))
                {
                    outputDir = Path.Combine(inputPath, "output");
                }
            }
            else if (File.Exists(inputPath))
            {
                files = new[] { inputPath };
                if (string.IsNullOrEmpty(outputDir))
                {
                    outputDir = Path.Combine(Path.GetDirectoryName(inputPath) ?? ".", "output");
                }
            }
            else
            {
                Console.Error.WriteLine($"Path not found: {inputPath}");
                return 1;
            }

            Directory.CreateDirectory(outputDir);

            int passed = 0;
            int failed = 0;
            int totalPages = 0;
            int totalWarnings = 0;
            var totalStopwatch = Stopwatch.StartNew();

            Console.WriteLine($"Testing {files.Length} PDF(s) at {dpi} DPI, output -> {outputDir}");
            Console.WriteLine(new string('-', 80));

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string baseName = Path.GetFileNameWithoutExtension(file);

                try
                {
                    var fileStopwatch = Stopwatch.StartNew();
                    using var reader = new PdfReader(file);
                    int pageCount = reader.PageCount;

                    var metadata = reader.Metadata;

                    if (reader.ParseWarnings.Count > 0)
                    {
                        foreach (var warning in reader.ParseWarnings)
                        {
                            Console.WriteLine($"  PARSE: {warning}");
                        }
                    }

                    if (pageCount == 0)
                    {
                        string reason = metadata.IsEncrypted ? "(encrypted)" : "(no pages found)";
                        Console.WriteLine($"SKIP  {fileName,-40} 0 pages {reason}");
                        continue;
                    }
                    int fileWarnings = 0;

                    for (int i = 0; i < pageCount; i++)
                    {
                        try
                        {
                            byte[] png = reader.RenderPage(i, dpi);
                            string outPath = Path.Combine(outputDir, $"{baseName}_p{i + 1}.png");
                            File.WriteAllBytes(outPath, png);
                            totalPages++;

                            foreach (var warning in reader.RenderWarnings)
                            {
                                fileWarnings++;
                                totalWarnings++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  ERR page {i + 1}: {ex.Message}");
                        }
                    }

                    fileStopwatch.Stop();

                    string status = fileWarnings > 0 ? "WARN" : "PASS";
                    string encFlag = metadata.IsEncrypted ? " [encrypted]" : "";
                    string verFlag = !string.IsNullOrEmpty(metadata.PdfVersion) ? $" v{metadata.PdfVersion}" : "";
                    Console.WriteLine($"{status,-5} {fileName,-40} {pageCount,3}pg  {fileStopwatch.ElapsedMilliseconds,6}ms  {fileWarnings,3} warn{encFlag}{verFlag}");

                    if (fileWarnings > 0)
                    {
                        passed++;
                    }
                    else
                    {
                        passed++;
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine($"FAIL  {fileName,-40} {ex.Message}");
                }
            }

            totalStopwatch.Stop();
            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"Done: {passed} passed, {failed} failed, {totalPages} pages rendered, " +
                              $"{totalWarnings} warnings, {totalStopwatch.ElapsedMilliseconds}ms total");

            return failed > 0 ? 1 : 0;
        }
    }
}
