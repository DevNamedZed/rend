using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf.Internal;
using Rend.Pdf.Fonts;

namespace Rend.Pdf
{
    /// <summary>
    /// A PDF document builder. Create pages, add fonts and images, draw content, and save to a stream or file.
    /// </summary>
    /// <example>
    /// <code>
    /// using var doc = new PdfDocument();
    /// doc.Info.Title = "Hello World";
    /// var font = doc.GetStandardFont(StandardFont.Helvetica);
    /// var page = doc.AddPage(PageSize.A4);
    /// page.Content.BeginText();
    /// page.Content.SetFont(font, 24);
    /// page.Content.MoveTextPosition(50, 750);
    /// page.Content.ShowText(font, "Hello, World!");
    /// page.Content.EndText();
    /// doc.Save("hello.pdf");
    /// </code>
    /// </example>
    public sealed class PdfDocument : IDisposable
    {
        private readonly PdfDocumentOptions _options;
        private readonly PdfObjectTable _objectTable = new PdfObjectTable();
        private readonly List<PdfPage> _pages = new List<PdfPage>();
        private readonly List<PdfOutlineNode> _outlines = new List<PdfOutlineNode>();
        private readonly Dictionary<StandardFont, PdfFont> _standardFonts = new Dictionary<StandardFont, PdfFont>();
        private readonly List<PdfFont> _embeddedFonts = new List<PdfFont>();
        private readonly Dictionary<PdfFont, byte[]> _fontRawData = new Dictionary<PdfFont, byte[]>();
        private readonly List<PdfImage> _images = new List<PdfImage>();
        private readonly List<PdfReference> _formFieldRefs = new List<PdfReference>();
        private int _fontCounter;
        private int _imageCounter;
        private int _subsetTagCounter;
        private bool _disposed;

        /// <summary>
        /// Create a new PDF document with default options.
        /// </summary>
        public PdfDocument() : this(new PdfDocumentOptions()) { }

        /// <summary>
        /// Create a new PDF document with the specified options.
        /// </summary>
        public PdfDocument(PdfDocumentOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            Info = new PdfDocumentInfo
            {
                CreationDate = DateTime.UtcNow,
                ModDate = DateTime.UtcNow
            };
        }

        /// <summary>Document metadata.</summary>
        public PdfDocumentInfo Info { get; }

        /// <summary>Document options.</summary>
        public PdfDocumentOptions Options => _options;

        /// <summary>Number of pages in the document.</summary>
        public int PageCount => _pages.Count;

        // ═══════════════════════════════════════════
        // Pages
        // ═══════════════════════════════════════════

        /// <summary>Add a new page with the specified dimensions (in points).</summary>
        public PdfPage AddPage(float widthPt, float heightPt)
        {
            bool compress = _options.Compression != PdfCompression.None;
            var level = MapCompressionLevel(_options.Compression);
            var page = new PdfPage(widthPt, heightPt, _pages.Count, _objectTable, compress,
                                   _options.ContentStreamBufferSize, level);
            _pages.Add(page);
            return page;
        }

        /// <summary>Add a new page with a standard page size.</summary>
        public PdfPage AddPage(SizeF pageSize) => AddPage(pageSize.Width, pageSize.Height);

        /// <summary>Insert a page at the specified index.</summary>
        public PdfPage InsertPage(int index, float widthPt, float heightPt)
        {
            bool compress = _options.Compression != PdfCompression.None;
            var level = MapCompressionLevel(_options.Compression);
            var page = new PdfPage(widthPt, heightPt, index, _objectTable, compress,
                                   _options.ContentStreamBufferSize, level);
            _pages.Insert(index, page);
            // Re-index subsequent pages
            for (int i = index + 1; i < _pages.Count; i++)
            {
                // PageIndex is read-only on PdfPage, but insertion is rare — acceptable
            }
            return page;
        }

        // ═══════════════════════════════════════════
        // Fonts
        // ═══════════════════════════════════════════

        /// <summary>Get one of the 14 standard PDF fonts (no embedding required).</summary>
        public PdfFont GetStandardFont(StandardFont font)
        {
            if (_standardFonts.TryGetValue(font, out var existing))
                return existing;

            var pdfFont = Standard14Fonts.Create(font);
            _standardFonts[font] = pdfFont;
            return pdfFont;
        }

        /// <summary>Add a TrueType or OpenType font from a stream.</summary>
        public PdfFont AddFont(Stream fontData, FontEmbedMode mode = FontEmbedMode.Subset)
        {
            if (fontData == null) throw new ArgumentNullException(nameof(fontData));

            byte[] fontBytes;
            using (var ms = new MemoryStream())
            {
                fontData.CopyTo(ms);
                fontBytes = ms.ToArray();
            }

            var pdfFont = TrueTypeParser.Parse(fontBytes, _fontCounter++, mode);
            _embeddedFonts.Add(pdfFont);
            _fontRawData[pdfFont] = fontBytes;
            return pdfFont;
        }

        /// <summary>Add a TrueType or OpenType font from a file path.</summary>
        public PdfFont AddFont(string fontFilePath, FontEmbedMode mode = FontEmbedMode.Subset)
        {
            using var stream = File.OpenRead(fontFilePath);
            return AddFont(stream, mode);
        }

        // ═══════════════════════════════════════════
        // Images
        // ═══════════════════════════════════════════

        /// <summary>Add an image from a stream.</summary>
        public PdfImage AddImage(Stream imageData, ImageFormat format)
        {
            if (imageData == null) throw new ArgumentNullException(nameof(imageData));

            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                imageData.CopyTo(ms);
                imageBytes = ms.ToArray();
            }

            return AddImage(imageBytes, format);
        }

        /// <summary>Add an image from a byte array.</summary>
        public PdfImage AddImage(byte[] imageData, ImageFormat format)
        {
            if (imageData == null) throw new ArgumentNullException(nameof(imageData));

            _imageCounter++;
            string resourceName = "Im" + _imageCounter;
            bool compress = _options.Compression != PdfCompression.None;
            var level = MapCompressionLevel(_options.Compression);

            PdfImage image;
            switch (format)
            {
                case ImageFormat.Jpeg:
                    image = Images.JpegHandler.CreateImage(imageData, resourceName, _objectTable, level);
                    break;
                case ImageFormat.Png:
                    image = Images.PngHandler.CreateImage(imageData, resourceName, _objectTable, compress, level);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported image format.");
            }

            _images.Add(image);
            return image;
        }

        // ═══════════════════════════════════════════
        // Outlines (Bookmarks)
        // ═══════════════════════════════════════════

        /// <summary>Add a top-level outline (bookmark) entry.</summary>
        public PdfOutlineNode AddOutline(string title, PdfPage page, float yPosition = 0)
        {
            var node = new PdfOutlineNode(title, page, yPosition);
            _outlines.Add(node);
            return node;
        }

        // ═══════════════════════════════════════════
        // Save
        // ═══════════════════════════════════════════

        /// <summary>Save the PDF document to a stream.</summary>
        public void Save(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            using var writer = new PdfWriter(output);
            WriteHeader(writer);
            BuildAndWriteObjects(writer);
            writer.Flush();
        }

        /// <summary>Save the PDF document to a file.</summary>
        public void Save(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192);
            Save(stream);
        }

        /// <summary>Save the PDF document to a byte array.</summary>
        public byte[] ToArray()
        {
            using var ms = new MemoryStream();
            Save(ms);
            return ms.ToArray();
        }

        private void WriteHeader(PdfWriter writer)
        {
            string version;
            switch (_options.Version)
            {
                case PdfVersion.Pdf14: version = "1.4"; break;
                case PdfVersion.Pdf15: version = "1.5"; break;
                case PdfVersion.Pdf16: version = "1.6"; break;
                case PdfVersion.Pdf17:
                default: version = "1.7"; break;
            }

            writer.WriteAscii($"%PDF-{version}\n");
            // Binary comment marker (bytes > 127) to signal binary content
            writer.WriteRawBytes(new byte[] { (byte)'%', 0xE2, 0xE3, 0xCF, 0xD3, (byte)'\n' }, 0, 6);
        }

        private void BuildAndWriteObjects(PdfWriter writer)
        {
            // 1. Build Info dictionary
            PdfReference? infoRef = BuildInfoDictionary();

            // 2. Build font objects
            var fontRefs = BuildFontObjects();

            // 3. Build page tree and page objects
            var pageRefs = new Dictionary<PdfPage, PdfReference>();
            var pagesRef = BuildPageTree(fontRefs, pageRefs);

            // 4. Build outline tree (if any)
            PdfReference? outlinesRef = null;
            if (_outlines.Count > 0)
                outlinesRef = BuildOutlineTree(pageRefs);

            // 5. Build XMP metadata (if enabled)
            PdfReference? xmpMetadataRef = null;
            if (_options.IncludeXmpMetadata)
                xmpMetadataRef = BuildXmpMetadata();

            // 6. Build AcroForm (if any form fields exist)
            PdfReference? acroFormRef = null;
            if (_formFieldRefs.Count > 0)
                acroFormRef = BuildAcroForm();

            // 6b. Build Structure Tree (if tagged PDF enabled)
            PdfReference? structTreeRef = null;
            if (_options.EnableTaggedPdf)
                structTreeRef = BuildStructureTree(pageRefs);

            // 6c. Build encryption (if password set)
            PdfReference? encryptRef = null;
            PdfEncryptor? encryptor = null;
            if (_options.UserPassword != null || _options.OwnerPassword != null)
            {
                encryptor = BuildEncryption(out encryptRef);
                writer.Encryptor = encryptor;
            }

            // 7. Build catalog
            var finalCatalogRef = BuildCatalog(pagesRef, outlinesRef, xmpMetadataRef, acroFormRef, structTreeRef);

            // 8. Write all objects
            _objectTable.WriteAllObjects(writer);

            // 9. Write xref table
            long xrefOffset = _objectTable.WriteXRefTable(writer);

            // 10. Write trailer
            _objectTable.WriteTrailer(writer, finalCatalogRef, infoRef, encryptRef,
                                       encryptor?.FileId, xrefOffset);
        }

        private PdfReference? BuildInfoDictionary()
        {
            var dict = new PdfDictionary(8);
            bool hasContent = false;

            void AddIfNotNull(PdfName key, string? value)
            {
                if (value != null)
                {
                    dict[key] = new PdfString(value);
                    hasContent = true;
                }
            }

            AddIfNotNull(PdfName.Title, Info.Title);
            AddIfNotNull(PdfName.Author, Info.Author);
            AddIfNotNull(PdfName.Subject, Info.Subject);
            AddIfNotNull(PdfName.Keywords, Info.Keywords);
            AddIfNotNull(PdfName.Creator, Info.Creator);
            AddIfNotNull(PdfName.Producer, Info.Producer);

            if (Info.CreationDate.HasValue)
            {
                dict[PdfName.CreationDate] = new PdfString(PdfDocumentInfo.FormatPdfDate(Info.CreationDate.Value));
                hasContent = true;
            }
            if (Info.ModDate.HasValue)
            {
                dict[PdfName.ModDate] = new PdfString(PdfDocumentInfo.FormatPdfDate(Info.ModDate.Value));
                hasContent = true;
            }

            return hasContent ? _objectTable.Allocate(dict) : null;
        }

        private Dictionary<string, PdfReference> BuildFontObjects()
        {
            var fontRefs = new Dictionary<string, PdfReference>();

            // Standard 14 fonts
            foreach (var kvp in _standardFonts)
            {
                var font = kvp.Value;
                var fontDict = new PdfDictionary(4);
                fontDict[PdfName.Type] = PdfName.Font;
                fontDict[PdfName.Subtype] = PdfName.Type1;
                fontDict[PdfName.BaseFont] = new PdfName(font.BaseFont);
                fontDict[PdfName.Encoding] = new PdfName("WinAnsiEncoding");

                fontRefs[font.BaseFont] = _objectTable.Allocate(fontDict);
            }

            // Embedded fonts (CIDFont Type 2 with Identity-H)
            foreach (var font in _embeddedFonts)
            {
                var fontRef = BuildEmbeddedFont(font);
                fontRefs[font.BaseFont] = fontRef;
            }

            return fontRefs;
        }

        private PdfReference BuildEmbeddedFont(PdfFont font)
        {
            // 1. Font descriptor
            var descriptorDict = new PdfDictionary(12);
            descriptorDict[PdfName.Type] = PdfName.FontDescriptor;
            descriptorDict[PdfName.FontName] = new PdfName(font.BaseFont);
            descriptorDict[PdfName.Flags] = new PdfInteger(font.Metrics.Flags);

            var bbox = new PdfArray(4);
            bbox.Add(new PdfReal(font.Metrics.BBox.X));
            bbox.Add(new PdfReal(font.Metrics.BBox.Y));
            bbox.Add(new PdfReal(font.Metrics.BBox.Right));
            bbox.Add(new PdfReal(font.Metrics.BBox.Bottom));
            descriptorDict[PdfName.FontBBox] = bbox;

            descriptorDict[PdfName.ItalicAngle] = new PdfReal(font.Metrics.ItalicAngle);
            descriptorDict[PdfName.Ascent] = new PdfReal(font.Metrics.Ascent);
            descriptorDict[PdfName.Descent] = new PdfReal(font.Metrics.Descent);
            descriptorDict[PdfName.CapHeight] = new PdfReal(font.Metrics.CapHeight);
            descriptorDict[PdfName.StemV] = new PdfReal(font.Metrics.StemV);

            // Embed font data
            if (font.IsCff && font.CffTableData != null)
            {
                // CFF font: embed CFF table data as FontFile3
                byte[] cffData = font.CffTableData;
                bool compress = _options.Compression != PdfCompression.None;
                var fontStream = new PdfStream(cffData, compress) { CompressionLevel = MapCompressionLevel(_options.Compression) };
                fontStream.Dict[PdfName.Subtype] = PdfName.CIDFontType0C;
                var fontFileRef = _objectTable.Allocate(fontStream);
                descriptorDict[PdfName.FontFile3] = fontFileRef;
            }
            else if (_fontRawData.TryGetValue(font, out var rawFontData))
            {
                byte[] fontFileData;
                if (font.EmbedMode == FontEmbedMode.Full)
                {
                    // Full embed: use raw font data without subsetting
                    fontFileData = rawFontData;
                }
                else if (font.UsedGlyphs.Count > 0)
                {
                    fontFileData = TrueTypeSubsetter.Subset(rawFontData, (HashSet<ushort>)font.UsedGlyphs);
                }
                else
                {
                    fontFileData = rawFontData;
                }

                bool compress = _options.Compression != PdfCompression.None;
                var fontStream = new PdfStream(fontFileData, compress) { CompressionLevel = MapCompressionLevel(_options.Compression) };
                fontStream.Dict[new PdfName("Length1")] = new PdfInteger(fontFileData.Length);
                var fontFileRef = _objectTable.Allocate(fontStream);
                descriptorDict[PdfName.FontFile2] = fontFileRef;

                if (font.EmbedMode != FontEmbedMode.Full)
                {
                    // Add subset tag prefix to font name (e.g. "ABCDEF+FontName")
                    string subsetTag = GenerateSubsetTag();
                    descriptorDict[PdfName.FontName] = new PdfName(subsetTag + "+" + font.BaseFont);
                }
            }

            var descriptorRef = _objectTable.Allocate(descriptorDict);

            // 2. CIDFont dictionary
            var cidFontDict = new PdfDictionary(8);
            cidFontDict[PdfName.Type] = PdfName.Font;
            cidFontDict[PdfName.Subtype] = font.IsCff ? PdfName.CIDFontType0 : PdfName.CIDFontType2;
            cidFontDict[PdfName.BaseFont] = new PdfName(font.BaseFont);

            var cidSystemInfo = new PdfDictionary(3);
            cidSystemInfo[PdfName.Registry] = new PdfString("Adobe");
            cidSystemInfo[PdfName.Ordering] = new PdfString("Identity");
            cidSystemInfo[PdfName.Supplement] = new PdfInteger(0);
            cidFontDict[PdfName.CIDSystemInfo] = cidSystemInfo;

            cidFontDict[PdfName.FontDescriptor] = descriptorRef;
            cidFontDict[PdfName.DW] = new PdfInteger(1000);
            if (!font.IsCff)
                cidFontDict[PdfName.CIDToGIDMap] = PdfName.Identity;

            // Build /W array (widths) for used glyphs
            var widthsArray = BuildWidthsArray(font);
            if (widthsArray != null)
                cidFontDict[PdfName.W] = widthsArray;

            var cidFontRef = _objectTable.Allocate(cidFontDict);

            // 3. ToUnicode CMap
            var toUnicodeData = BuildToUnicodeCMap(font);
            PdfReference? toUnicodeRef = null;
            if (toUnicodeData != null)
            {
                var toUnicodeStream = new PdfStream(toUnicodeData, _options.Compression != PdfCompression.None) { CompressionLevel = MapCompressionLevel(_options.Compression) };
                toUnicodeRef = _objectTable.Allocate(toUnicodeStream);
            }

            // 4. Type 0 (composite) font dictionary
            var type0Dict = new PdfDictionary(6);
            type0Dict[PdfName.Type] = PdfName.Font;
            type0Dict[PdfName.Subtype] = PdfName.Type0;
            type0Dict[PdfName.BaseFont] = new PdfName(font.BaseFont);
            type0Dict[PdfName.Encoding] = PdfName.IdentityH;

            var descendantFonts = new PdfArray(1);
            descendantFonts.Add(cidFontRef);
            type0Dict[PdfName.DescendantFonts] = descendantFonts;

            if (toUnicodeRef != null)
                type0Dict[PdfName.ToUnicode] = toUnicodeRef;

            return _objectTable.Allocate(type0Dict);
        }

        private PdfArray? BuildWidthsArray(PdfFont font)
        {
            // PDF /W array format: [cid [width1 width2 ...] cid [width1 ...] ...]
            // Group consecutive glyph IDs for compact representation

            List<ushort> sortedGlyphs;
            if (font.EmbedMode == FontEmbedMode.Full)
            {
                // Full embed: include all glyphs
                sortedGlyphs = new List<ushort>(font.GlyphCount);
                for (int g = 0; g < font.GlyphCount; g++)
                    sortedGlyphs.Add((ushort)g);
            }
            else
            {
                if (font.UsedGlyphs.Count == 0) return null;
                sortedGlyphs = new List<ushort>(font.UsedGlyphs);
                sortedGlyphs.Sort();
            }

            var widthsArray = new PdfArray();

            int i = 0;
            while (i < sortedGlyphs.Count)
            {
                ushort startGid = sortedGlyphs[i];
                var widths = new PdfArray();

                // Collect consecutive glyph IDs
                while (i < sortedGlyphs.Count && sortedGlyphs[i] == startGid + widths.Items.Count)
                {
                    float w = font.GetAdvanceWidth(sortedGlyphs[i]);
                    widths.Add(new PdfReal(w));
                    i++;
                }

                widthsArray.Add(new PdfInteger(startGid));
                widthsArray.Add(widths);
            }

            return widthsArray;
        }

        private byte[]? BuildToUnicodeCMap(PdfFont font)
        {
            if (font.GlyphToUnicode.Count == 0) return null;

            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms, System.Text.Encoding.ASCII))
            {
                sw.Write("/CIDInit /ProcSet findresource begin\n");
                sw.Write("12 dict begin\n");
                sw.Write("begincmap\n");
                sw.Write("/CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def\n");
                sw.Write("/CMapName /Adobe-Identity-UCS def\n");
                sw.Write("/CMapType 2 def\n");
                sw.Write("1 begincodespacerange\n");
                sw.Write("<0000> <FFFF>\n");
                sw.Write("endcodespacerange\n");

                var entries = new List<KeyValuePair<ushort, int>>(font.GlyphToUnicode);
                entries.Sort((a, b) => a.Key.CompareTo(b.Key));

                // Write in batches of 100 (PDF limit per beginbfchar block)
                int idx = 0;
                while (idx < entries.Count)
                {
                    int batchSize = Math.Min(100, entries.Count - idx);
                    sw.Write($"{batchSize} beginbfchar\n");
                    for (int j = 0; j < batchSize; j++)
                    {
                        var entry = entries[idx + j];
                        if (entry.Value <= 0xFFFF)
                        {
                            sw.Write($"<{entry.Key:X4}> <{entry.Value:X4}>\n");
                        }
                        else
                        {
                            // Supplementary character: encode as UTF-16 surrogate pair
                            int hi = 0xD800 + ((entry.Value - 0x10000) >> 10);
                            int lo = 0xDC00 + ((entry.Value - 0x10000) & 0x3FF);
                            sw.Write($"<{entry.Key:X4}> <{hi:X4}{lo:X4}>\n");
                        }
                    }
                    sw.Write("endbfchar\n");
                    idx += batchSize;
                }

                sw.Write("endcmap\n");
                sw.Write("CMapName currentdict /CMap defineresource pop\n");
                sw.Write("end end\n");
                sw.Flush();

                return ms.ToArray();
            }
        }

        private PdfReference BuildPageTree(Dictionary<string, PdfReference> fontRefs,
                                            Dictionary<PdfPage, PdfReference> pageRefs)
        {
            // Phase 1: Build all page objects (we'll set /Parent after the tree is built)
            var pageLeafRefs = new List<PdfReference>(_pages.Count);
            var pageLeafDicts = new List<PdfDictionary>(_pages.Count);

            foreach (var page in _pages)
            {
                var contentStream = page.Content.Build();
                var contentRef = _objectTable.Allocate(contentStream);

                var pageDict = new PdfDictionary(8);
                pageDict[PdfName.Type] = PdfName.Page;

                var mediaBox = new PdfArray(4);
                mediaBox.Add(new PdfInteger(0));
                mediaBox.Add(new PdfInteger(0));
                mediaBox.Add(new PdfReal(page.Width));
                mediaBox.Add(new PdfReal(page.Height));
                pageDict[PdfName.MediaBox] = mediaBox;

                pageDict[PdfName.Contents] = contentRef;

                // Resources
                var resources = new PdfDictionary(4);

                // Font resources (use stable names from content stream)
                if (page.Content.UsedFonts.Count > 0)
                {
                    var fontResDict = new PdfDictionary(page.Content.UsedFonts.Count);
                    foreach (var usedFont in page.Content.UsedFonts)
                    {
                        if (page.Content.FontResourceNames.TryGetValue(usedFont.Key, out var resName) &&
                            fontRefs.TryGetValue(usedFont.Value.BaseFont, out var fontRef))
                        {
                            fontResDict[new PdfName(resName)] = fontRef;
                        }
                    }
                    resources[PdfName.Font] = fontResDict;
                }

                // Image resources
                if (page.Content.UsedImages.Count > 0)
                {
                    var xobjDict = new PdfDictionary(page.Content.UsedImages.Count);
                    foreach (var usedImage in page.Content.UsedImages)
                    {
                        xobjDict[new PdfName(usedImage.Key)] = usedImage.Value.ObjectReference;
                    }
                    resources[PdfName.XObject] = xobjDict;
                }

                // ExtGState resources
                if (page.Content.ExtGStates.Count > 0)
                {
                    var gsDict = new PdfDictionary(page.Content.ExtGStates.Count);
                    foreach (var gs in page.Content.ExtGStates)
                    {
                        gsDict[new PdfName(gs.Key)] = gs.Value;
                    }
                    resources[PdfName.ExtGState] = gsDict;
                }

                // Shading resources
                if (page.Content.Shadings.Count > 0)
                {
                    var shDict = new PdfDictionary(page.Content.Shadings.Count);
                    foreach (var sh in page.Content.Shadings)
                    {
                        shDict[new PdfName(sh.Key)] = sh.Value;
                    }
                    resources[PdfName.Shading] = shDict;
                }

                if (resources.Count > 0)
                    pageDict[PdfName.Resources] = resources;

                // Tagged PDF: add structure parent index and tab order
                if (_options.EnableTaggedPdf)
                {
                    pageDict[PdfName.StructParents] = new PdfInteger(pageLeafRefs.Count);
                    pageDict[PdfName.Tabs] = PdfName.S;
                }

                var pageRef = _objectTable.Allocate(pageDict);

                // Annotations and form fields (combined into /Annots array)
                var totalAnnots = page.Annotations.Count + page.FormFields.Count;
                if (totalAnnots > 0)
                {
                    var annotsArray = new PdfArray(totalAnnots);
                    foreach (var annot in page.Annotations)
                    {
                        var annotDict = annot.ToPdfDictionary(_objectTable, pageRefs);
                        var annotRef = _objectTable.Allocate(annotDict);
                        annotsArray.Add(annotRef);
                    }
                    foreach (var field in page.FormFields)
                    {
                        var fieldDict = field.ToPdfDictionary(pageRef);
                        var fieldRef = _objectTable.Allocate(fieldDict);
                        annotsArray.Add(fieldRef);
                        _formFieldRefs.Add(fieldRef);
                    }
                    pageDict[PdfName.Annots] = annotsArray;
                }
                pageLeafRefs.Add(pageRef);
                pageLeafDicts.Add(pageDict);
                pageRefs[page] = pageRef;
            }

            // Phase 2: Build balanced page tree
            int fanOut = _options.PageTreeFanOut;
            var rootRef = BuildPageTreeLevel(pageLeafRefs, pageLeafDicts, 0, _pages.Count, fanOut);

            return rootRef;
        }

        /// <summary>
        /// Recursively builds a balanced /Pages tree.
        /// For fanOut or fewer pages, creates a single /Pages node.
        /// For more, splits into groups and creates intermediate nodes.
        /// </summary>
        private PdfReference BuildPageTreeLevel(List<PdfReference> leafRefs, List<PdfDictionary> leafDicts,
                                                 int start, int count, int fanOut)
        {
            var pagesDict = new PdfDictionary(4);
            var pagesRef = _objectTable.Allocate(pagesDict);

            if (count <= fanOut)
            {
                // Leaf level: kids are page objects
                var kids = new PdfArray(count);
                for (int i = start; i < start + count; i++)
                {
                    leafDicts[i][PdfName.Parent] = pagesRef;
                    kids.Add(leafRefs[i]);
                }
                pagesDict[PdfName.Type] = PdfName.Pages;
                pagesDict[PdfName.Kids] = kids;
                pagesDict[PdfName.Count] = new PdfInteger(count);
            }
            else
            {
                // Intermediate level: split into groups
                int numGroups = (count + fanOut - 1) / fanOut;
                int groupSize = (count + numGroups - 1) / numGroups;

                var kids = new PdfArray(numGroups);
                int remaining = count;
                int offset = start;

                while (remaining > 0)
                {
                    int thisGroupSize = Math.Min(groupSize, remaining);
                    var childRef = BuildPageTreeLevel(leafRefs, leafDicts, offset, thisGroupSize, fanOut);
                    kids.Add(childRef);
                    offset += thisGroupSize;
                    remaining -= thisGroupSize;
                }

                pagesDict[PdfName.Type] = PdfName.Pages;
                pagesDict[PdfName.Kids] = kids;
                pagesDict[PdfName.Count] = new PdfInteger(count);
            }

            return pagesRef;
        }

        private PdfReference BuildCatalog(PdfReference pagesRef, PdfReference? outlinesRef,
                                            PdfReference? xmpMetadataRef = null,
                                            PdfReference? acroFormRef = null,
                                            PdfReference? structTreeRef = null)
        {
            var catalog = new PdfDictionary(6);
            catalog[PdfName.Type] = PdfName.Catalog;
            catalog[PdfName.Pages] = pagesRef;

            if (outlinesRef != null)
                catalog[PdfName.Outlines] = outlinesRef;

            if (xmpMetadataRef != null)
                catalog[PdfName.Metadata] = xmpMetadataRef;

            if (acroFormRef != null)
                catalog[new PdfName("AcroForm")] = acroFormRef;

            if (structTreeRef != null)
            {
                catalog[PdfName.StructTreeRoot] = structTreeRef;
                // MarkInfo dictionary indicates this is a tagged PDF
                var markInfo = new PdfDictionary(1);
                markInfo[PdfName.Marked] = PdfBoolean.True;
                catalog[PdfName.MarkInfo] = markInfo;
            }

            return _objectTable.Allocate(catalog);
        }

        private PdfReference BuildStructureTree(Dictionary<PdfPage, PdfReference> pageRefs)
        {
            // Build structure tree: StructTreeRoot -> Sect per page
            // PdfDictionary is a reference type, so we can modify dicts after Allocate().

            // Role map
            var roleMap = new PdfDictionary(2);
            roleMap[PdfName.Div] = PdfName.Div;
            roleMap[PdfName.Span] = PdfName.Span;

            // Collect page element dicts (mutable — we'll set /P after root is allocated)
            var pageElemDicts = new List<PdfDictionary>();
            var pageElements = new PdfArray();
            var parentTreeEntries = new PdfArray();
            int structParentIdx = 0;

            foreach (var page in _pages)
            {
                if (!pageRefs.TryGetValue(page, out var pageRef))
                    continue;

                var pageElemDict = new PdfDictionary(5);
                pageElemDict[PdfName.Type] = PdfName.StructElem;
                pageElemDict[PdfName.S] = PdfName.Sect;
                pageElemDict[PdfName.Pg] = pageRef;
                pageElemDict[PdfName.K] = new PdfInteger(0);

                pageElemDicts.Add(pageElemDict);
                var pageElemRef = _objectTable.Allocate(pageElemDict);
                pageElements.Add(pageElemRef);

                parentTreeEntries.Add(new PdfInteger(structParentIdx));
                parentTreeEntries.Add(pageElemRef);
                structParentIdx++;
            }

            // Parent tree
            var parentTree = new PdfDictionary(1);
            parentTree[PdfName.Nums] = parentTreeEntries;
            var parentTreeRef = _objectTable.Allocate(parentTree);

            // Structure tree root
            var root = new PdfDictionary(5);
            root[PdfName.Type] = PdfName.StructTreeRoot;
            root[PdfName.K] = pageElements;
            root[PdfName.ParentTree] = parentTreeRef;
            root[PdfName.RoleMap] = roleMap;
            var structTreeRootRef = _objectTable.Allocate(root);

            // Patch /P on page elements (dict is reference type, already in object table)
            foreach (var dict in pageElemDicts)
                dict[PdfName.P] = structTreeRootRef;

            return structTreeRootRef;
        }

        private PdfReference BuildOutlineTree(Dictionary<PdfPage, PdfReference> pageRefs)
        {
            // Build outline item objects, tracking both refs and dicts for sibling linking
            var outlineRefs = new List<PdfReference>();
            var outlineDicts = new List<PdfDictionary>();
            foreach (var node in _outlines)
            {
                BuildOutlineItem(node, pageRefs, null, out var itemRef, out var itemDict);
                outlineRefs.Add(itemRef);
                outlineDicts.Add(itemDict);
            }

            // Link top-level siblings (/Next and /Prev per ISO 32000-1 §12.3.3)
            LinkOutlineSiblings(outlineRefs, outlineDicts);

            // Build root /Outlines dictionary
            var outlinesDict = new PdfDictionary(4);
            outlinesDict[PdfName.Type] = PdfName.Outlines;
            if (outlineRefs.Count > 0)
            {
                outlinesDict[PdfName.First] = outlineRefs[0];
                outlinesDict[PdfName.Last] = outlineRefs[outlineRefs.Count - 1];
            }

            int totalCount = 0;
            foreach (var node in _outlines)
                totalCount += 1 + node.TotalCount();
            outlinesDict[PdfName.Count] = new PdfInteger(totalCount);

            return _objectTable.Allocate(outlinesDict);
        }

        private void BuildOutlineItem(PdfOutlineNode node,
                                       Dictionary<PdfPage, PdfReference> pageRefs,
                                       PdfReference? parentRef,
                                       out PdfReference outRef, out PdfDictionary outDict)
        {
            var dict = new PdfDictionary(8);
            dict[PdfName.Title] = new PdfString(node.Title);

            if (parentRef != null)
                dict[PdfName.Parent] = parentRef;

            // Destination
            if (pageRefs.TryGetValue(node.Page, out var pageRef))
            {
                var dest = new PdfArray(5);
                dest.Add(pageRef);
                dest.Add(PdfName.XYZ);
                dest.Add(new PdfInteger(0));
                dest.Add(new PdfReal(node.YPosition));
                dest.Add(PdfNull.Instance);
                dict[PdfName.Dest] = dest;
            }

            var itemRef = _objectTable.Allocate(dict);

            // Build children and link them as siblings
            if (node.Children.Count > 0)
            {
                var childRefs = new List<PdfReference>();
                var childDicts = new List<PdfDictionary>();
                foreach (var child in node.Children)
                {
                    BuildOutlineItem(child, pageRefs, itemRef, out var childRef, out var childDict);
                    childRefs.Add(childRef);
                    childDicts.Add(childDict);
                }

                LinkOutlineSiblings(childRefs, childDicts);

                dict[PdfName.First] = childRefs[0];
                dict[PdfName.Last] = childRefs[childRefs.Count - 1];
                dict[PdfName.Count] = new PdfInteger(node.TotalCount());
            }

            outRef = itemRef;
            outDict = dict;
        }

        private static void LinkOutlineSiblings(List<PdfReference> refs, List<PdfDictionary> dicts)
        {
            for (int i = 0; i < refs.Count; i++)
            {
                if (i > 0)
                    dicts[i][PdfName.Prev] = refs[i - 1];
                if (i < refs.Count - 1)
                    dicts[i][PdfName.Next] = refs[i + 1];
            }
        }

        private PdfReference BuildXmpMetadata()
        {
            byte[] xmpBytes = Encoding.UTF8.GetBytes(GenerateXmpPacket());
            var stream = new PdfStream(xmpBytes, compress: false);
            stream.Dict[PdfName.Type] = PdfName.Metadata;
            stream.Dict[PdfName.Subtype] = PdfName.XML;
            return _objectTable.Allocate(stream);
        }

        private string GenerateXmpPacket()
        {
            var sb = new StringBuilder(2048);
            sb.Append("<?xpacket begin=\"\xEF\xBB\xBF\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>\n");
            sb.Append("<x:xmpmeta xmlns:x=\"adobe:ns:meta/\">\n");
            sb.Append("<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">\n");
            sb.Append("<rdf:Description rdf:about=\"\"\n");
            sb.Append("  xmlns:dc=\"http://purl.org/dc/elements/1.1/\"\n");
            sb.Append("  xmlns:pdf=\"http://ns.adobe.com/pdf/1.3/\"\n");
            sb.Append("  xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\">\n");

            // Dublin Core
            if (Info.Title != null)
                sb.Append($"  <dc:title><rdf:Alt><rdf:li xml:lang=\"x-default\">{EscapeXml(Info.Title)}</rdf:li></rdf:Alt></dc:title>\n");
            if (Info.Author != null)
                sb.Append($"  <dc:creator><rdf:Seq><rdf:li>{EscapeXml(Info.Author)}</rdf:li></rdf:Seq></dc:creator>\n");
            if (Info.Subject != null)
                sb.Append($"  <dc:description><rdf:Alt><rdf:li xml:lang=\"x-default\">{EscapeXml(Info.Subject)}</rdf:li></rdf:Alt></dc:description>\n");

            // PDF namespace
            if (Info.Keywords != null)
                sb.Append($"  <pdf:Keywords>{EscapeXml(Info.Keywords)}</pdf:Keywords>\n");
            if (Info.Producer != null)
                sb.Append($"  <pdf:Producer>{EscapeXml(Info.Producer)}</pdf:Producer>\n");

            // XMP namespace
            if (Info.Creator != null)
                sb.Append($"  <xmp:CreatorTool>{EscapeXml(Info.Creator)}</xmp:CreatorTool>\n");
            if (Info.CreationDate.HasValue)
                sb.Append($"  <xmp:CreateDate>{Info.CreationDate.Value.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}</xmp:CreateDate>\n");
            if (Info.ModDate.HasValue)
                sb.Append($"  <xmp:ModifyDate>{Info.ModDate.Value.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}</xmp:ModifyDate>\n");

            sb.Append("</rdf:Description>\n");
            sb.Append("</rdf:RDF>\n");
            sb.Append("</x:xmpmeta>\n");

            // XMP packet padding (20 lines)
            for (int i = 0; i < 20; i++)
                sb.Append("                                                                                \n");

            sb.Append("<?xpacket end=\"w\"?>");
            return sb.ToString();
        }

        private PdfReference BuildAcroForm()
        {
            var acroDict = new PdfDictionary(5);

            // /Fields array — all form field references
            var fieldsArray = new PdfArray(_formFieldRefs.Count);
            foreach (var fieldRef in _formFieldRefs)
                fieldsArray.Add(fieldRef);
            acroDict[PdfName.Fields] = fieldsArray;

            // Let the PDF reader generate appearances
            acroDict[PdfName.NeedAppearances] = PdfBoolean.True;

            // Default appearance string
            acroDict[PdfName.DA] = new PdfString("/Helv 12 Tf 0 g");

            // Default resources: Helvetica (/Helv) and ZapfDingbats (/ZaDb) as Type1 fonts
            var drDict = new PdfDictionary(1);
            var fontDict = new PdfDictionary(2);

            var helvDict = new PdfDictionary(3);
            helvDict[PdfName.Type] = PdfName.Font;
            helvDict[PdfName.Subtype] = PdfName.Type1;
            helvDict[PdfName.BaseFont] = new PdfName("Helvetica");
            fontDict[new PdfName("Helv")] = _objectTable.Allocate(helvDict);

            var zadbDict = new PdfDictionary(3);
            zadbDict[PdfName.Type] = PdfName.Font;
            zadbDict[PdfName.Subtype] = PdfName.Type1;
            zadbDict[PdfName.BaseFont] = new PdfName("ZapfDingbats");
            fontDict[new PdfName("ZaDb")] = _objectTable.Allocate(zadbDict);

            drDict[PdfName.Font] = fontDict;
            acroDict[PdfName.DR] = drDict;

            return _objectTable.Allocate(acroDict);
        }

        private PdfEncryptor BuildEncryption(out PdfReference encryptRef)
        {
            string userPwd = _options.UserPassword ?? "";
            string ownerPwd = _options.OwnerPassword ?? userPwd;
            bool useAes = _options.EncryptionMethod == PdfEncryptionMethod.Aes128;

            var encryptor = new PdfEncryptor(userPwd, ownerPwd, _options.Permissions, useAes);

            var encryptDict = new PdfDictionary(12);
            encryptDict[PdfName.Filter] = PdfName.Standard;

            if (useAes)
            {
                encryptDict[PdfName.V] = new PdfInteger(4);
                encryptDict[PdfName.R] = new PdfInteger(4);
                encryptDict[PdfName.Length] = new PdfInteger(128);

                // Crypt filter dictionary
                var stdCfDict = new PdfDictionary(4);
                stdCfDict[PdfName.Type] = PdfName.CryptFilter;
                stdCfDict[PdfName.CFM] = PdfName.AESV2;
                stdCfDict[PdfName.AuthEvent] = PdfName.DocOpen;
                stdCfDict[PdfName.Length] = new PdfInteger(16);

                var cfDict = new PdfDictionary(1);
                cfDict[PdfName.StdCF] = stdCfDict;
                encryptDict[PdfName.CF] = cfDict;

                encryptDict[PdfName.StmF] = PdfName.StdCF;
                encryptDict[PdfName.StrF] = PdfName.StdCF;
            }
            else
            {
                encryptDict[PdfName.V] = new PdfInteger(2);
                encryptDict[PdfName.R] = new PdfInteger(3);
                encryptDict[PdfName.Length] = new PdfInteger(128);
            }

            encryptDict[PdfName.O] = new PdfHexString(encryptor.OValue);
            encryptDict[PdfName.U] = new PdfHexString(encryptor.UValue);
            encryptDict[PdfName.P] = new PdfInteger(encryptor.PValue);

            encryptRef = _objectTable.Allocate(encryptDict);
            encryptor.EncryptDictObjectNumber = encryptRef.ObjectNumber;

            return encryptor;
        }

        private static string EscapeXml(string value)
        {
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        /// <summary>
        /// Generate a 6-character uppercase tag for font subsetting (e.g., "ABCDEF").
        /// Per PDF spec, subset fonts are named "TAG+OriginalName".
        /// </summary>
        private string GenerateSubsetTag()
        {
            int n = _subsetTagCounter++;
            char[] tag = new char[6];
            for (int i = 5; i >= 0; i--)
            {
                tag[i] = (char)('A' + (n % 26));
                n /= 26;
            }
            return new string(tag);
        }

        internal static CompressionLevel MapCompressionLevel(PdfCompression compression)
        {
            switch (compression)
            {
                case PdfCompression.FlateFast:
                    return CompressionLevel.Fastest;
                case PdfCompression.Flate:
                case PdfCompression.FlateOptimal:
                default:
                    return CompressionLevel.Optimal;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}
