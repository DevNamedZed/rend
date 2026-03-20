using System;
using System.IO;
using Rend.Pdf.Parsing;

namespace Rend.PdfCli
{
    internal static class Type1DumpCommand
    {
        public static int Run(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: type1dump <file.pdf> [--page <0>] [--font <name>]");
                return 1;
            }

            string filePath = args[1];
            int pageIndex = 0;
            string targetFont = "";

            for (int i = 2; i < args.Length - 1; i++)
            {
                if (args[i] == "--page") { pageIndex = int.Parse(args[++i]); }
                if (args[i] == "--font") { targetFont = args[++i]; }
            }

            try
            {
                using var reader = PdfDocumentReader.Open(File.ReadAllBytes(filePath));
                var pageDict = reader.Resolve(reader.GetPage(pageIndex));
                var resources = reader.Resolve(pageDict["Resources"]);
                var fonts = reader.Resolve(resources["Font"]);

                foreach (var fontName in fonts.Keys)
                {
                    if (!string.IsNullOrEmpty(targetFont) && fontName != targetFont)
                    {
                        continue;
                    }

                    var fontDict = reader.Resolve(fonts[fontName]);
                    var subtype = reader.Resolve(fontDict["Subtype"]).AsName();
                    if (!subtype.Contains("Type1"))
                    {
                        continue;
                    }

                    byte[]? fontData = reader.GetFontProgramData(fontDict);
                    if (fontData == null || fontData.Length == 0)
                    {
                        Console.WriteLine($"{fontName}: no embedded data");
                        continue;
                    }

                    Console.WriteLine($"{fontName}: {fontData.Length} bytes");
                    Console.WriteLine($"  First 16 bytes: {BitConverter.ToString(fontData, 0, Math.Min(16, fontData.Length))}");

                    // Check format
                    if (fontData.Length >= 6 && fontData[0] == 0x80)
                    {
                        // PFB format: 0x80 type length[4]
                        int type = fontData[1];
                        int length = fontData[2] | (fontData[3] << 8) | (fontData[4] << 16) | (fontData[5] << 24);
                        Console.WriteLine($"  Format: PFB (type={type}, segment_length={length})");

                        // Dump header ASCII portion
                        int headerStart = 6;
                        int headerEnd = Math.Min(headerStart + length, fontData.Length);
                        string header = System.Text.Encoding.ASCII.GetString(fontData, headerStart, Math.Min(500, headerEnd - headerStart));
                        Console.WriteLine($"  Header: {header.Substring(0, Math.Min(300, header.Length))}...");
                    }
                    else if (fontData.Length >= 4 && fontData[0] == '%' && fontData[1] == '!')
                    {
                        // PFA format: ASCII text
                        string header = System.Text.Encoding.ASCII.GetString(fontData, 0, Math.Min(500, fontData.Length));
                        Console.WriteLine($"  Format: PFA");
                        Console.WriteLine($"  Header: {header.Substring(0, Math.Min(300, header.Length))}...");
                    }
                    else if (fontData.Length >= 4 &&
                        ((fontData[0] == 0 && fontData[1] == 1 && fontData[2] == 0 && fontData[3] == 0) ||
                         (fontData[0] == 'O' && fontData[1] == 'T' && fontData[2] == 'T' && fontData[3] == 'O')))
                    {
                        Console.WriteLine($"  Format: TrueType/OpenType (Skia can load this!)");
                    }
                    else
                    {
                        Console.WriteLine($"  Format: Unknown");
                    }

                    // Save font data to file for analysis
                    if (!string.IsNullOrEmpty(targetFont))
                    {
                        string outPath = $"type1_{fontName}.bin";
                        File.WriteAllBytes(outPath, fontData);
                        Console.WriteLine($"  Saved to: {outPath}");
                    }

                    Console.WriteLine();
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: {ex.Message}");
                return 1;
            }
        }
    }
}
