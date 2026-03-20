using System;
using System.IO;
using Rend.Pdf.Parsing;

namespace Rend.PdfCli
{
    internal static class DumpCommand
    {
        public static int Run(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: dump <file.pdf> [--page <0>]");
                return 1;
            }

            string filePath = args[1];
            int pageIndex = 0;

            for (int i = 2; i < args.Length - 1; i++)
            {
                if (args[i] == "--page")
                {
                    pageIndex = int.Parse(args[++i]);
                }
            }

            try
            {
                using var reader = PdfDocumentReader.Open(File.ReadAllBytes(filePath));
                var pageDict = reader.Resolve(reader.GetPage(pageIndex));
                var contents = reader.Resolve(pageDict["Contents"]);

                byte[] contentBytes;
                if (contents.IsArray)
                {
                    using var memoryStream = new MemoryStream();
                    for (int i = 0; i < contents.Count; i++)
                    {
                        var streamObj = reader.Resolve(contents[i]);
                        if (streamObj.IsStream)
                        {
                            var bytes = reader.GetStreamBytes(streamObj);
                            if (bytes != null && bytes.Length > 0)
                            {
                                memoryStream.Write(bytes, 0, bytes.Length);
                                memoryStream.WriteByte((byte)'\n');
                            }
                        }
                    }
                    contentBytes = memoryStream.ToArray();
                }
                else if (contents.IsStream)
                {
                    contentBytes = reader.GetStreamBytes(contents) ?? Array.Empty<byte>();
                }
                else
                {
                    Console.Error.WriteLine("No content stream found");
                    return 1;
                }

                string text = System.Text.Encoding.ASCII.GetString(contentBytes);
                // Show first 3000 chars focusing on text operators
                int printed = 0;
                foreach (var line in text.Split('\n'))
                {
                    string trimmed = line.Trim();
                    if (trimmed.Contains("BT") || trimmed.Contains("ET") ||
                        trimmed.Contains("Tf") || trimmed.Contains("Td") ||
                        trimmed.Contains("Tm") || trimmed.Contains("Tj") ||
                        trimmed.Contains("TJ") || trimmed.Contains("cm") ||
                        trimmed.Contains("Do") || trimmed.Contains("re") ||
                        printed < 200)
                    {
                        Console.WriteLine(trimmed);
                        printed++;
                        if (printed > 300)
                        {
                            break;
                        }
                    }
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
