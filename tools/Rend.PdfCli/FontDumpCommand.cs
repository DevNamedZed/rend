using System;
using System.IO;
using Rend.Pdf.Parsing;

namespace Rend.PdfCli
{
    internal static class FontDumpCommand
    {
        public static int Run(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: fonts <file.pdf> [--page <0>]");
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
                var resources = reader.Resolve(pageDict["Resources"]);
                var fonts = reader.Resolve(resources["Font"]);

                if (fonts.IsNull)
                {
                    Console.WriteLine("No fonts found in page resources");
                    return 0;
                }

                foreach (var fontName in fonts.Keys)
                {
                    var fontDict = reader.Resolve(fonts[fontName]);
                    var subtype = reader.Resolve(fontDict["Subtype"]).AsName();
                    var baseFont = reader.Resolve(fontDict["BaseFont"]).AsName();
                    var encoding = reader.Resolve(fontDict["Encoding"]).AsName();

                    bool isCid = subtype == "Type0" || fontDict.ContainsKey("DescendantFonts");
                    PdfObj cidDict = fontDict;
                    if (isCid)
                    {
                        var descendants = reader.Resolve(fontDict["DescendantFonts"]);
                        if (descendants.IsArray && descendants.Count > 0)
                        {
                            cidDict = reader.Resolve(descendants[0]);
                        }
                    }

                    var descriptor = reader.Resolve(cidDict["FontDescriptor"]);
                    bool hasEmbedded = false;
                    string embeddedType = "";
                    if (!descriptor.IsNull)
                    {
                        var ff2 = reader.Resolve(descriptor["FontFile2"]);
                        var ff1 = reader.Resolve(descriptor["FontFile"]);
                        var ff3 = reader.Resolve(descriptor["FontFile3"]);
                        if (!ff2.IsNull) { hasEmbedded = true; embeddedType = "TrueType"; }
                        else if (!ff1.IsNull) { hasEmbedded = true; embeddedType = "Type1"; }
                        else if (!ff3.IsNull) { hasEmbedded = true; embeddedType = "CFF/Type1C"; }
                    }

                    var widths = reader.Resolve(fontDict["Widths"]);
                    var firstChar = reader.Resolve(fontDict["FirstChar"]);
                    var cidWidths = reader.Resolve(cidDict["W"]);

                    Console.WriteLine($"  {fontName,-6} {subtype,-10} {baseFont,-40} enc={encoding}");
                    Console.WriteLine($"         CID={isCid} embedded={hasEmbedded}({embeddedType}) " +
                        $"widths={(!widths.IsNull ? widths.Count + " entries" : cidWidths.IsNull ? "NONE" : "CID /W")} " +
                        $"firstChar={(!firstChar.IsNull ? firstChar.AsInt().ToString() : "n/a")}");
                }

                // Color spaces
                var colorSpaces = reader.Resolve(resources["ColorSpace"]);
                if (!colorSpaces.IsNull)
                {
                    Console.WriteLine("\nColor Spaces:");
                    foreach (var csName in colorSpaces.Keys)
                    {
                        var csObj = reader.Resolve(colorSpaces[csName]);
                        Console.WriteLine($"  {csName}: {csObj}");
                        if (csObj.IsArray)
                        {
                            for (int i = 0; i < Math.Min(csObj.Count, 5); i++)
                            {
                                var item = reader.Resolve(csObj[i]);
                                string extra = "";
                                if (item.IsDict || item.IsStream)
                                {
                                    extra = " keys=[" + string.Join(",", item.Keys) + "]";
                                }
                                Console.WriteLine($"    [{i}]: {item}{extra}");
                            }
                        }
                    }
                }

                // XObjects
                var xobjects = reader.Resolve(resources["XObject"]);
                if (!xobjects.IsNull)
                {
                    Console.WriteLine("\nXObjects:");
                    foreach (var xoName in xobjects.Keys)
                    {
                        var xobj = reader.Resolve(xobjects[xoName]);
                        var subtype = reader.Resolve(xobj["Subtype"]).AsName();
                        if (subtype.Contains("Image"))
                        {
                            var w = reader.Resolve(xobj["Width"]).AsInt();
                            var h = reader.Resolve(xobj["Height"]).AsInt();
                            var bpc = reader.Resolve(xobj["BitsPerComponent"]).AsInt();
                            var filter = reader.Resolve(xobj["Filter"]);
                            var cs = reader.Resolve(xobj["ColorSpace"]);
                            var mask = reader.Resolve(xobj["SMask"]);
                            var imgMask = reader.Resolve(xobj["ImageMask"]);
                            var decode = reader.Resolve(xobj["Decode"]);
                            string csDesc = cs.IsName ? cs.AsName() : (cs.IsArray ? $"[{reader.Resolve(cs[0]).AsName()} count={cs.Count}]" : "none");
                            Console.WriteLine($"  {xoName,-6} Image {w}x{h} bpc={bpc} filter={filter} cs={csDesc} smask={!mask.IsNull} imgMask={imgMask.AsBool()} decode={decode}");
                            if (cs.IsArray)
                            {
                                for (int ci = 0; ci < cs.Count; ci++)
                                {
                                    var item = reader.Resolve(cs[ci]);
                                    Console.WriteLine($"         cs[{ci}]: {item}");
                                }
                            }
                        }
                        else if (subtype.Contains("Form"))
                        {
                            var formRes = reader.Resolve(xobj["Resources"]);
                            Console.WriteLine($"  {xoName,-6} Form hasResources={!formRes.IsNull}");
                            if (!formRes.IsNull)
                            {
                                var formXO = reader.Resolve(formRes["XObject"]);
                                if (!formXO.IsNull)
                                {
                                    foreach (var formXOName in formXO.Keys)
                                    {
                                        var nested = reader.Resolve(formXO[formXOName]);
                                        var nSub = reader.Resolve(nested["Subtype"]).AsName();
                                        if (nSub.Contains("Image"))
                                        {
                                            var nw = reader.Resolve(nested["Width"]).AsInt();
                                            var nh = reader.Resolve(nested["Height"]).AsInt();
                                            var nbpc = reader.Resolve(nested["BitsPerComponent"]).AsInt();
                                            var ncs = reader.Resolve(nested["ColorSpace"]);
                                            var nim = reader.Resolve(nested["ImageMask"]);
                                            var ndec = reader.Resolve(nested["Decode"]);
                                            var nsm = reader.Resolve(nested["SMask"]);
                                            string ncsDesc = ncs.IsName ? ncs.AsName() : (ncs.IsArray ? $"[array:{ncs.Count}]" : ncs.IsNull ? "null" : ncs.ToString());
                                            Console.WriteLine($"    -> {formXOName,-4} Image {nw}x{nh} bpc={nbpc} cs={ncsDesc} imgMask={nim} decode={ndec} smask={!nsm.IsNull}");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"  {xoName,-6} {subtype}");
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
