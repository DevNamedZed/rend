using System;
using System.Diagnostics;
using System.IO;
using Rend;
using SkiaSharp;

namespace Rend.PdfCli
{
    internal static class CompareCommand
    {
        public static int Run(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: compare <dir-or-file> [--dpi <150>] [--threshold <5.0>]");
                Console.Error.WriteLine("  Renders PDFs with Rend and MuPDF, computes pixel diff percentage.");
                Console.Error.WriteLine("  Requires 'mutool' in PATH.");
                return 1;
            }

            string inputPath = args[1];
            int dpi = 150;
            float threshold = 5.0f;

            for (int i = 2; i < args.Length - 1; i++)
            {
                switch (args[i])
                {
                    case "--dpi": dpi = int.Parse(args[++i]); break;
                    case "--threshold": threshold = float.Parse(args[++i]); break;
                }
            }

            string[] files;
            if (Directory.Exists(inputPath))
            {
                files = Directory.GetFiles(inputPath, "*.pdf");
                Array.Sort(files);
            }
            else if (File.Exists(inputPath))
            {
                files = new[] { inputPath };
            }
            else
            {
                Console.Error.WriteLine($"Path not found: {inputPath}");
                return 1;
            }

            string outputDir = Path.Combine(
                Directory.Exists(inputPath) ? inputPath : Path.GetDirectoryName(inputPath) ?? ".",
                "compare_output");
            Directory.CreateDirectory(outputDir);

            int totalFiles = 0;
            int passedFiles = 0;
            int failedFiles = 0;
            int errorFiles = 0;

            Console.WriteLine($"Comparing {files.Length} PDF(s) at {dpi} DPI (threshold {threshold}%)");
            Console.WriteLine($"Output: {outputDir}");
            Console.WriteLine(new string('-', 90));
            Console.WriteLine($"{"File",-40} {"Pages",5} {"Avg Diff",9} {"Max Diff",9} {"Status",8}");
            Console.WriteLine(new string('-', 90));

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string baseName = Path.GetFileNameWithoutExtension(file);

                try
                {
                    using var reader = new PdfReader(file);
                    if (reader.PageCount == 0)
                    {
                        Console.WriteLine($"{fileName,-40} {"0",5} {"",9} {"",9} {"SKIP",8}");
                        continue;
                    }

                    totalFiles++;
                    float maxDiff = 0;
                    float totalDiff = 0;
                    int pageCount = Math.Min(reader.PageCount, 5); // Limit to first 5 pages

                    for (int page = 0; page < pageCount; page++)
                    {
                        string rendPath = Path.Combine(outputDir, $"{baseName}_p{page + 1}_rend.png");
                        string refPath = Path.Combine(outputDir, $"{baseName}_p{page + 1}_ref.png");
                        string diffPath = Path.Combine(outputDir, $"{baseName}_p{page + 1}_diff.png");

                        // Render with Rend
                        byte[] rendPng = reader.RenderPage(page, dpi);
                        File.WriteAllBytes(rendPath, rendPng);

                        // Render with MuPDF
                        var mutoolResult = RunMutool(file, page + 1, dpi, refPath);
                        if (!mutoolResult)
                        {
                            continue;
                        }

                        // Compare
                        float diff = ComputeDiff(rendPath, refPath, diffPath);
                        totalDiff += diff;
                        if (diff > maxDiff)
                        {
                            maxDiff = diff;
                        }
                    }

                    float avgDiff = totalDiff / pageCount;
                    bool passed = maxDiff <= threshold;

                    if (passed)
                    {
                        passedFiles++;
                    }
                    else
                    {
                        failedFiles++;
                    }

                    string status = passed ? "PASS" : "FAIL";
                    Console.WriteLine($"{fileName,-40} {pageCount,5} {avgDiff,8:F2}% {maxDiff,8:F2}% {status,8}");
                }
                catch (Exception ex)
                {
                    errorFiles++;
                    Console.WriteLine($"{fileName,-40} {"",5} {"",9} {"",9} {"ERROR",8}  {ex.Message}");
                }
            }

            Console.WriteLine(new string('-', 90));
            Console.WriteLine($"Total: {totalFiles} tested, {passedFiles} passed, {failedFiles} failed, {errorFiles} errors");

            return failedFiles > 0 || errorFiles > 0 ? 1 : 0;
        }

        private static bool RunMutool(string pdfPath, int pageNumber, int dpi, string outputPath)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "mutool",
                        Arguments = $"draw -o \"{outputPath}\" -r {dpi} \"{pdfPath}\" {pageNumber}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                process.WaitForExit(30000);
                return process.ExitCode == 0 && File.Exists(outputPath);
            }
            catch
            {
                return false;
            }
        }

        private static float ComputeDiff(string rendPath, string refPath, string diffPath)
        {
            using var rendBitmap = SKBitmap.Decode(rendPath);
            using var refBitmap = SKBitmap.Decode(refPath);

            if (rendBitmap == null || refBitmap == null)
            {
                return 100f;
            }

            int width = Math.Min(rendBitmap.Width, refBitmap.Width);
            int height = Math.Min(rendBitmap.Height, refBitmap.Height);

            using var diffBitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

            int diffPixels = 0;
            int totalPixels = width * height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var rendPixel = rendBitmap.GetPixel(x, y);
                    var refPixel = refBitmap.GetPixel(x, y);

                    int deltaR = Math.Abs(rendPixel.Red - refPixel.Red);
                    int deltaG = Math.Abs(rendPixel.Green - refPixel.Green);
                    int deltaB = Math.Abs(rendPixel.Blue - refPixel.Blue);
                    int totalDelta = deltaR + deltaG + deltaB;

                    if (totalDelta > 30)
                    {
                        diffPixels++;
                        byte intensity = (byte)Math.Min(255, totalDelta);
                        diffBitmap.SetPixel(x, y, new SKColor(255, 0, 0, intensity));
                    }
                    else
                    {
                        byte gray = (byte)((refPixel.Red + refPixel.Green + refPixel.Blue) / 9);
                        diffBitmap.SetPixel(x, y, new SKColor(gray, gray, gray));
                    }
                }
            }

            using var image = SKImage.FromBitmap(diffBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 90);
            File.WriteAllBytes(diffPath, data.ToArray());

            return (float)diffPixels / totalPixels * 100f;
        }
    }
}
