using System;
using System.Collections.Generic;
using System.IO;

namespace Rend.Html.Conformance
{
    public static class Html5libDatParser
    {
        public static IEnumerable<Html5libTestCase> Parse(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var fileName = Path.GetFileName(filePath);
            int index = 0;

            int i = 0;
            while (i < lines.Length)
            {
                // Skip blank lines between tests
                if (string.IsNullOrEmpty(lines[i]))
                {
                    i++;
                    continue;
                }

                if (lines[i] != "#data")
                {
                    i++;
                    continue;
                }

                var testCase = new Html5libTestCase
                {
                    SourceFile = fileName,
                    Index = index++
                };

                // Parse #data section
                i++;
                var dataLines = new List<string>();
                while (i < lines.Length && !lines[i].StartsWith("#"))
                {
                    dataLines.Add(lines[i]);
                    i++;
                }
                // The data section: join lines, but the last line's trailing newline
                // depends on the format. html5lib .dat files include a trailing newline
                // for multi-line data but the convention is the data does NOT include
                // the final newline before the next section marker.
                testCase.Data = string.Join("\n", dataLines);

                // Parse remaining sections
                while (i < lines.Length)
                {
                    if (lines[i] == "#errors" || lines[i] == "#new-errors")
                    {
                        i++;
                        while (i < lines.Length && !lines[i].StartsWith("#"))
                        {
                            if (!string.IsNullOrEmpty(lines[i]))
                                testCase.Errors.Add(lines[i]);
                            i++;
                        }
                    }
                    else if (lines[i] == "#document-fragment")
                    {
                        i++;
                        if (i < lines.Length)
                        {
                            var ctx = lines[i].Trim();
                            // Fragment context can have a namespace prefix: "svg path"
                            var spaceIdx = ctx.IndexOf(' ');
                            if (spaceIdx > 0)
                            {
                                testCase.FragmentNamespace = ctx.Substring(0, spaceIdx);
                                testCase.FragmentContext = ctx.Substring(spaceIdx + 1);
                            }
                            else
                            {
                                testCase.FragmentContext = ctx;
                            }
                            i++;
                        }
                    }
                    else if (lines[i] == "#script-on")
                    {
                        testCase.ScriptingEnabled = true;
                        i++;
                    }
                    else if (lines[i] == "#script-off")
                    {
                        testCase.ScriptingEnabled = false;
                        i++;
                    }
                    else if (lines[i] == "#document")
                    {
                        i++;
                        var treeLines = new List<string>();
                        while (i < lines.Length && !string.IsNullOrEmpty(lines[i]))
                        {
                            treeLines.Add(lines[i]);
                            i++;
                        }
                        testCase.ExpectedTree = string.Join("\n", treeLines);
                        break; // End of this test case
                    }
                    else
                    {
                        // Unknown section, skip
                        i++;
                    }
                }

                // Skip the blank line separator
                if (i < lines.Length && string.IsNullOrEmpty(lines[i]))
                    i++;

                yield return testCase;
            }
        }
    }
}
