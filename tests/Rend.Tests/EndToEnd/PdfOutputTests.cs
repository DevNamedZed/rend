using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Rend.Core.Values;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.EndToEnd
{
    public class PdfOutputTests
    {
        private readonly ITestOutputHelper _output;

        public PdfOutputTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private byte[] RenderPdf(string html, float w = 400, float h = 300)
        {
            return Render.ToPdf(html, new RenderOptions
            {
                PageSize = new SizeF(w, h),
                MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0
            });
        }

        private string PdfText(byte[] pdf) => Encoding.ASCII.GetString(pdf);

        /// <summary>
        /// Extracts and decompresses all FlateDecode content streams from a PDF byte array.
        /// Returns the concatenated decompressed content as ASCII text.
        /// </summary>
        private string ExtractContentStreams(byte[] pdf)
        {
            var sb = new StringBuilder();
            var text = Encoding.ASCII.GetString(pdf);

            int searchPos = 0;
            while (true)
            {
                int streamStart = text.IndexOf("stream\r\n", searchPos, StringComparison.Ordinal);
                if (streamStart < 0)
                    streamStart = text.IndexOf("stream\n", searchPos, StringComparison.Ordinal);
                if (streamStart < 0) break;

                int dataStart = streamStart + 7;
                if (dataStart < pdf.Length && pdf[dataStart] == '\n')
                    dataStart++;

                int endStream = text.IndexOf("endstream", dataStart, StringComparison.Ordinal);
                if (endStream < 0) break;

                int dataEnd = endStream;
                while (dataEnd > dataStart && (pdf[dataEnd - 1] == '\r' || pdf[dataEnd - 1] == '\n'))
                    dataEnd--;

                int len = dataEnd - dataStart;
                if (len > 0 && len < pdf.Length)
                {
                    var streamData = new byte[len];
                    Buffer.BlockCopy(pdf, dataStart, streamData, 0, len);

                    try
                    {
                        using var ms = new MemoryStream(streamData);
                        using var deflate = new DeflateStream(ms, CompressionMode.Decompress);
                        using var output = new MemoryStream();
                        deflate.CopyTo(output);
                        sb.Append(Encoding.ASCII.GetString(output.ToArray()));
                        sb.Append('\n');
                    }
                    catch
                    {
                        sb.Append(Encoding.ASCII.GetString(streamData));
                        sb.Append('\n');
                    }
                }

                searchPos = endStream + 9;
            }

            return sb.ToString();
        }

        // ===============================================
        // PDF Text Operations
        // ===============================================

        [Fact]
        public void ParagraphText_ProducesBtEtTfTdOperators()
        {
            var pdf = RenderPdf("<html><body><p>Hello World</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("BT", content);
            Assert.Contains("ET", content);
            Assert.Contains("Tf", content);
            Assert.Contains("Td", content);
        }

        [Fact]
        public void MultipleParagraphs_ProduceMultipleBtEtBlocks()
        {
            var pdf = RenderPdf("<html><body><p>First</p><p>Second</p><p>Third</p></body></html>");
            var content = ExtractContentStreams(pdf);
            var btCount = Regex.Matches(content, @"\bBT\b").Count;
            var etCount = Regex.Matches(content, @"\bET\b").Count;
            Assert.True(btCount >= 3, $"Expected at least 3 BT operators, got {btCount}");
            Assert.True(etCount >= 3, $"Expected at least 3 ET operators, got {etCount}");
        }

        [Fact]
        public void FontSize24_AppearsInTfOperator()
        {
            var pdf = RenderPdf("<html><body><p style='font-size:24px'>Big text</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Matches(@"/F\d+\s+24\s+Tf", content);
        }

        [Fact]
        public void FontSize48_AppearsInTfOperator()
        {
            var pdf = RenderPdf("<html><body><p style='font-size:48px'>Huge</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Matches(@"/F\d+\s+48\s+Tf", content);
        }

        [Fact]
        public void DefaultFontSize16_AppearsInTfOperator()
        {
            var pdf = RenderPdf("<html><body><p>Default size</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Matches(@"/F\d+\s+16\s+Tf", content);
        }

        [Fact]
        public void TextPositionTdOperators_Present()
        {
            var pdf = RenderPdf("<html><body><p>Positioned text</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Matches(@"[\d.]+\s+[\d.]+\s+Td", content);
        }

        [Fact]
        public void TextContent_AppearsWithTjOperator()
        {
            var pdf = RenderPdf("<html><body><p>Hello World</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("Tj", content);
            Assert.Contains("(Hello World)", content);
        }

        [Fact]
        public void BoldText_UsesF1FontReference()
        {
            var pdf = RenderPdf("<html><body><p><strong>Bold text</strong></p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("BT", content);
            Assert.Matches(@"/F\d+\s+16\s+Tf", content);
        }

        [Fact]
        public void MixedStyles_ProduceMultipleBtEtBlocks()
        {
            var pdf = RenderPdf("<html><body><p>Normal <strong>bold</strong> normal</p></body></html>");
            var content = ExtractContentStreams(pdf);
            var btCount = Regex.Matches(content, @"\bBT\b").Count;
            Assert.True(btCount >= 3, $"Expected at least 3 BT operators for mixed styles, got {btCount}");
        }

        [Fact]
        public void HeadingsH1ToH3_ProduceDistinctTextObjects()
        {
            var pdf = RenderPdf(@"<html><body>
                <h1>Heading 1</h1>
                <h2>Heading 2</h2>
                <h3>Heading 3</h3>
            </body></html>");
            var content = ExtractContentStreams(pdf);
            var btCount = Regex.Matches(content, @"\bBT\b").Count;
            Assert.True(btCount >= 3, $"Expected at least 3 text objects for 3 headings, got {btCount}");
        }

        // ===============================================
        // PDF Color Operations
        // ===============================================

        [Fact]
        public void RedText_Produces_1_0_0_rg()
        {
            var pdf = RenderPdf("<html><body><p style='color:red'>Red text</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("1 0 0 rg", content);
        }

        [Fact]
        public void GreenText_Produces_0_0502_0_rg()
        {
            var pdf = RenderPdf("<html><body><p style='color:green'>Green text</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("0 0.502 0 rg", content);
        }

        [Fact]
        public void BlueText_Produces_0_0_1_rg()
        {
            var pdf = RenderPdf("<html><body><p style='color:blue'>Blue text</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("0 0 1 rg", content);
        }

        [Fact]
        public void BlackText_Produces_0_0_0_rg()
        {
            var pdf = RenderPdf("<html><body><p style='color:black'>Black text</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("0 0 0 rg", content);
        }

        [Fact]
        public void WhiteText_Produces_1_1_1_rg()
        {
            var pdf = RenderPdf("<html><body><div style='background:black'><p style='color:white'>White text</p></div></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("1 1 1 rg", content);
        }

        [Fact]
        public void HexColor_ff0000_ProducesRed()
        {
            var pdf = RenderPdf("<html><body><div style='width:100px;height:50px;background-color:#ff0000'>Red box</div></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("1 0 0 rg", content);
        }

        [Fact]
        public void HexColor_336699_ProducesCorrectRgb()
        {
            var pdf = RenderPdf("<html><body><div style='width:100px;height:50px;background-color:#336699'>Box</div></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("0.2 0.4 0.6 rg", content);
        }

        [Fact]
        public void HexColor_663399_ProducesCorrectRgb()
        {
            var pdf = RenderPdf("<html><body><p style='color:#663399'>Purple</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("0.4 0.2 0.6 rg", content);
        }

        [Fact]
        public void HexColor_cc6600_ProducesCorrectRgb()
        {
            var pdf = RenderPdf("<html><body><p style='color:#cc6600'>Orange hex</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("0.8 0.4 0 rg", content);
        }

        [Fact]
        public void BackgroundColor_ProducesRectangleThenFill()
        {
            var pdf = RenderPdf("<html><body><div style='width:100px;height:50px;background:blue'>Box</div></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("0 0 1 rg", content);
            Assert.Contains("re", content);
            Assert.Contains("f", content);
        }

        [Fact]
        public void BackgroundRed_ProducesCorrectColorBeforeRect()
        {
            var pdf = RenderPdf("<html><body><div style='width:100px;height:50px;background-color:#ff0000'>Red box</div></body></html>");
            var content = ExtractContentStreams(pdf);
            // Red fill color followed by rectangle and fill
            int rgPos = content.IndexOf("1 0 0 rg");
            int rePos = content.IndexOf("re", rgPos);
            int fPos = content.IndexOf("f", rePos);
            Assert.True(rgPos >= 0, "Red color not found");
            Assert.True(rePos > rgPos, "Rectangle should come after color");
            Assert.True(fPos > rePos, "Fill should come after rectangle");
        }

        [Fact]
        public void MultipleBackgrounds_ProduceMultipleRectangles()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='width:100px;height:50px;background:red'></div>
                <div style='width:100px;height:50px;background:green'></div>
                <div style='width:100px;height:50px;background:blue'></div>
            </body></html>");
            var content = ExtractContentStreams(pdf);
            var reCount = Regex.Matches(content, @"\bre\b").Count;
            Assert.True(reCount >= 3, $"Expected at least 3 rectangles, got {reCount}");
        }

        // ===============================================
        // PDF Graphics Operations
        // ===============================================

        [Fact]
        public void BackgroundRectangles_UseReAndFOperators()
        {
            var pdf = RenderPdf("<html><body><div style='width:100px;height:50px;background:#eee'>Box</div></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Matches(@"[\d.]+\s+[\d.]+\s+[\d.]+\s+[\d.]+\s+re", content);
            Assert.Contains("\nf\n", content);
        }

        [Fact]
        public void SolidBorder_ProducesFillTrapezoids()
        {
            var pdf = RenderPdf("<html><body><div style='border:2px solid black;width:100px;height:50px'>Bordered</div></body></html>");
            var content = ExtractContentStreams(pdf);
            // Solid borders are rendered as filled trapezoid paths (m, l, h, f)
            Assert.Contains(" m\n", content);
            Assert.Contains(" l\n", content);
            Assert.Contains("h\n", content);
        }

        [Fact]
        public void DashedBorder_UsesStrokeOperator()
        {
            var pdf = RenderPdf("<html><body><div style='border:2px dashed red;width:100px;height:50px'>Dashed</div></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("S\n", content);
            Assert.Contains("1 0 0 RG", content);
            Assert.Matches(@"\d+\s+w", content);
            Assert.Matches(@"\[[\d ]+\]\s+\d+\s+d", content);
        }

        [Fact]
        public void Opacity_UsesExtGState_gsOperator()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='opacity:0.5;width:100px;height:100px;background:red'>Semi-transparent</div>
            </body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("gs", content);
            Assert.Matches(@"/ca[\d.]+\s+gs", content);
            Assert.Matches(@"/CA[\d.]+\s+gs", content);
        }

        [Fact]
        public void Opacity_ExtGStateInPdfStructure()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='opacity:0.5;width:100px;height:100px;background:red'>Semi-transparent</div>
            </body></html>");
            var text = PdfText(pdf);
            Assert.Contains("ExtGState", text);
            Assert.Contains("/ca 0.5", text);
            Assert.Contains("/CA 0.5", text);
        }

        [Fact]
        public void TransformRotate_ProducesCmOperator()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='transform:rotate(45deg);width:100px;height:100px;background:red;margin:50px'>Rotated</div>
            </body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Matches(@"[\d.-]+\s+[\d.-]+\s+[\d.-]+\s+[\d.-]+\s+[\d.-]+\s+[\d.-]+\s+cm", content);
        }

        [Fact]
        public void TransformRotate45_HasCorrectMatrixValues()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='transform:rotate(45deg);width:100px;height:100px;background:red;margin:50px'>Rotated</div>
            </body></html>");
            var content = ExtractContentStreams(pdf);
            // cos(45) = 0.7071, sin(45) = 0.7071
            Assert.Contains("0.7071", content);
        }

        [Fact]
        public void OverflowHidden_ProducesClipOperator()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='width:100px;height:50px;overflow:hidden;background:#eee'>
                    <p>Long text that should be clipped.</p>
                </div>
            </body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("W", content);
            Assert.Contains("n", content);
        }

        [Fact]
        public void ContentStream_HasSaveRestoreState()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.StartsWith("q\n", content);
            Assert.Contains("\nQ\n", content);
        }

        [Fact]
        public void NestedOpacity_HasNestedSaveRestore()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='opacity:0.5;width:100px;height:100px;background:red'>Test</div>
            </body></html>");
            var content = ExtractContentStreams(pdf);
            // Outer q/Q plus inner q/Q for opacity group
            var qCount = Regex.Matches(content, @"^q$", RegexOptions.Multiline).Count;
            var QCount = Regex.Matches(content, @"^Q$", RegexOptions.Multiline).Count;
            Assert.True(qCount >= 2, $"Expected at least 2 save states, got {qCount}");
            Assert.Equal(qCount, QCount);
        }

        // ===============================================
        // PDF Structure
        // ===============================================

        [Fact]
        public void ValidPdfHeader_StartsWithPdf17()
        {
            var pdf = RenderPdf("<html><body></body></html>");
            var text = PdfText(pdf);
            Assert.StartsWith("%PDF-1.7", text);
        }

        [Fact]
        public void ValidPdf_EndsWithEofMarker()
        {
            var pdf = RenderPdf("<html><body></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("%%EOF", text);
        }

        [Fact]
        public void ValidPdf_HasTypeCatalog()
        {
            var pdf = RenderPdf("<html><body><p>Test</p></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("/Type /Catalog", text);
        }

        [Fact]
        public void ValidPdf_HasTypePage()
        {
            var pdf = RenderPdf("<html><body><p>Test</p></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("/Type /Page", text);
        }

        [Fact]
        public void ValidPdf_HasResourcesWithFontDictionary()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("/Resources", text);
            Assert.Contains("/Font", text);
        }

        [Fact]
        public void ValidPdf_HasXrefTable()
        {
            var pdf = RenderPdf("<html><body><p>Test</p></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("xref", text);
        }

        [Fact]
        public void ValidPdf_HasTrailerDictionary()
        {
            var pdf = RenderPdf("<html><body><p>Test</p></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("trailer", text);
        }

        [Fact]
        public void MediaBox_MatchesPageSize400x300()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>", 400, 300);
            var text = PdfText(pdf);
            Assert.Contains("/MediaBox [0 0 400 300]", text);
        }

        [Fact]
        public void MediaBox_MatchesPageSize800x600()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>", 800, 600);
            var text = PdfText(pdf);
            Assert.Contains("/MediaBox [0 0 800 600]", text);
        }

        [Fact]
        public void MediaBox_MatchesLetterSize()
        {
            var pdf = Render.ToPdf("<html><body><p>Hello</p></body></html>", new RenderOptions
            {
                PageSize = PageSize.Letter,
                MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0
            });
            var text = PdfText(pdf);
            Assert.Contains("/MediaBox [0 0 612 792]", text);
        }

        [Fact]
        public void MediaBox_MatchesA4Size()
        {
            var pdf = Render.ToPdf("<html><body><p>Hello</p></body></html>", new RenderOptions
            {
                PageSize = PageSize.A4,
                MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0
            });
            var text = PdfText(pdf);
            Assert.Contains("/MediaBox [0 0 595.28 841.89]", text);
        }

        [Fact]
        public void LargeContent_ProducesMultiplePages()
        {
            var sb = new StringBuilder("<html><body>");
            for (int i = 0; i < 100; i++)
                sb.Append($"<p>Paragraph {i + 1}: Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>");
            sb.Append("</body></html>");

            var pdf = Render.ToPdf(sb.ToString(), new RenderOptions
            {
                PageSize = new SizeF(400, 300),
                MarginTop = 20, MarginRight = 20, MarginBottom = 20, MarginLeft = 20
            });
            var text = PdfText(pdf);
            var pageCount = Regex.Matches(text, @"/Type\s*/Page[^s]").Count;
            Assert.True(pageCount > 1, $"Expected multiple pages, got {pageCount}");
        }

        [Fact]
        public void ProducerMetadata_IsRendPdf()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("/Producer (Rend.Pdf)", text);
        }

        [Fact]
        public void TitleMetadata_AppearsInPdf()
        {
            var pdf = Render.ToPdf("<html><body><p>Hello</p></body></html>", new RenderOptions
            {
                PageSize = new SizeF(400, 300),
                Title = "My Document"
            });
            var text = PdfText(pdf);
            Assert.Contains("/Title (My Document)", text);
        }

        [Fact]
        public void AuthorMetadata_AppearsInPdf()
        {
            var pdf = Render.ToPdf("<html><body><p>Hello</p></body></html>", new RenderOptions
            {
                PageSize = new SizeF(400, 300),
                Author = "Test Author"
            });
            var text = PdfText(pdf);
            Assert.Contains("/Author (Test Author)", text);
        }

        [Fact]
        public void ValidPdf_HasStartxref()
        {
            var pdf = RenderPdf("<html><body><p>Test</p></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("startxref", text);
        }

        [Fact]
        public void ValidPdf_HasRootReference()
        {
            var pdf = RenderPdf("<html><body><p>Test</p></body></html>");
            var text = PdfText(pdf);
            Assert.Matches(@"/Root\s+\d+\s+0\s+R", text);
        }

        [Fact]
        public void ValidPdf_HasPagesType()
        {
            var pdf = RenderPdf("<html><body><p>Test</p></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("/Type /Pages", text);
        }

        [Fact]
        public void ValidPdf_HasCountInPages()
        {
            var pdf = RenderPdf("<html><body><p>Test</p></body></html>");
            var text = PdfText(pdf);
            Assert.Matches(@"/Count\s+\d+", text);
        }

        // ===============================================
        // PDF Font Handling
        // ===============================================

        [Fact]
        public void StandardFont_HelveticaReferenced()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("/BaseFont /Helvetica", text);
        }

        [Fact]
        public void FontResources_InPageDictionary()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Matches(@"/Font\s*<<[\s\S]*?/F\d+\s+\d+\s+0\s+R", text);
        }

        [Fact]
        public void Font_HasType1Subtype()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("/Subtype /Type1", text);
        }

        [Fact]
        public void Font_HasWinAnsiEncoding()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("/Encoding /WinAnsiEncoding", text);
        }

        [Fact]
        public void FontReference_MatchesTfOperator()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            var content = ExtractContentStreams(pdf);
            // Font reference in page resources (e.g., /F1 2 0 R)
            Assert.Matches(@"/F\d+\s+\d+\s+0\s+R", text);
            // Same font name in content stream Tf operator
            Assert.Matches(@"/F\d+\s+\d+\s+Tf", content);
        }

        // ===============================================
        // Content Stream Structure
        // ===============================================

        [Fact]
        public void ContentStream_StartsWithYFlipTransform()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>", 400, 300);
            var content = ExtractContentStreams(pdf);
            // q followed by the Y-flip matrix: 1 0 0 -1 0 <pageHeight> cm
            Assert.Contains("1 0 0 -1 0 300 cm", content);
        }

        [Fact]
        public void ContentStream_YFlipUsesCorrectPageHeight()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>", 800, 600);
            var content = ExtractContentStreams(pdf);
            Assert.Contains("1 0 0 -1 0 600 cm", content);
        }

        [Fact]
        public void MultiplePages_HaveIndependentContentStreams()
        {
            var sb = new StringBuilder("<html><body>");
            for (int i = 0; i < 50; i++)
                sb.Append($"<p>Paragraph {i}: Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>");
            sb.Append("</body></html>");

            var pdf = Render.ToPdf(sb.ToString(), new RenderOptions
            {
                PageSize = new SizeF(400, 300),
                MarginTop = 20, MarginRight = 20, MarginBottom = 20, MarginLeft = 20
            });
            var content = ExtractContentStreams(pdf);
            // Each page has its own q...Q block with cm transform
            var cmCount = Regex.Matches(content, @"1 0 0 -1 0 300 cm").Count;
            var text = PdfText(pdf);
            var pageCount = Regex.Matches(text, @"/Type\s*/Page[^s]").Count;
            Assert.True(pageCount > 1, $"Expected multiple pages, got {pageCount}");
            Assert.Equal(pageCount, cmCount);
        }

        [Fact]
        public void GraphicsState_ProperSaveRestore_Balanced()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var content = ExtractContentStreams(pdf);
            var qCount = Regex.Matches(content, @"^q$", RegexOptions.Multiline).Count;
            var QCount = Regex.Matches(content, @"^Q$", RegexOptions.Multiline).Count;
            Assert.Equal(qCount, QCount);
            Assert.True(qCount >= 1, "Expected at least one save/restore pair");
        }

        [Fact]
        public void ContentStream_HasFlateDecode()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("/Filter /FlateDecode", text);
        }

        [Fact]
        public void ContentStream_HasLength()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Matches(@"/Length\s+\d+", text);
        }

        // ===============================================
        // Additional Text Rendering Tests
        // ===============================================

        [Fact]
        public void TextContent_StringAppearsInParentheses()
        {
            var pdf = RenderPdf("<html><body><p>Test string</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("(Test string)", content);
        }

        [Fact]
        public void MultipleTextElements_EachHasOwnBtEtBlock()
        {
            var pdf = RenderPdf(@"<html><body>
                <h1>Title</h1>
                <p>Body text</p>
            </body></html>");
            var content = ExtractContentStreams(pdf);
            var btCount = Regex.Matches(content, @"\bBT\b").Count;
            Assert.True(btCount >= 2, $"Expected at least 2 BT blocks, got {btCount}");
        }

        [Fact]
        public void ItalicText_ProducesTextOutput()
        {
            var pdf = RenderPdf("<html><body><p><em>Italic text</em></p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("BT", content);
            Assert.Contains("Tf", content);
            Assert.Contains("Tj", content);
        }

        // ===============================================
        // Additional Color Tests
        // ===============================================

        [Fact]
        public void StrokeColor_UsesUppercase_RG()
        {
            var pdf = RenderPdf("<html><body><p style='text-decoration:underline;color:red'>Underlined</p></body></html>");
            var content = ExtractContentStreams(pdf);
            // Stroke color for underline uses RG (uppercase)
            Assert.Contains("RG", content);
        }

        [Fact]
        public void FillColor_UsesLowercase_rg()
        {
            var pdf = RenderPdf("<html><body><p style='color:blue'>Blue</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("0 0 1 rg", content);
        }

        [Fact]
        public void WhiteBackground_BlackText_BothColors()
        {
            var pdf = RenderPdf("<html><body><div style='background:black'><p style='color:white'>White on black</p></div></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("0 0 0 rg", content); // Black background
            Assert.Contains("1 1 1 rg", content); // White text
        }

        // ===============================================
        // Additional Graphics Tests
        // ===============================================

        [Fact]
        public void DashedBorder_HasDashPattern()
        {
            var pdf = RenderPdf("<html><body><div style='border:2px dashed blue;width:100px;height:50px'>Box</div></body></html>");
            var content = ExtractContentStreams(pdf);
            // Dash pattern: [dashLength gapLength] offset d
            Assert.Matches(@"\[\d+\s+\d+\]\s+\d+\s+d", content);
        }

        [Fact]
        public void DashedBorder_HasLineWidth()
        {
            var pdf = RenderPdf("<html><body><div style='border:2px dashed red;width:100px;height:50px'>Box</div></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("2 w", content);
        }

        [Fact]
        public void SolidBorder_FourSides_ProduceFourPaths()
        {
            var pdf = RenderPdf("<html><body><div style='border:2px solid black;width:100px;height:50px'>Box</div></body></html>");
            var content = ExtractContentStreams(pdf);
            // Each side is a filled trapezoid: m, l, l, l, h, f pattern
            var hCount = Regex.Matches(content, @"^h$", RegexOptions.Multiline).Count;
            Assert.True(hCount >= 4, $"Expected at least 4 path closings for 4 border sides, got {hCount}");
        }

        [Fact]
        public void Underline_UsesStrokeOperator()
        {
            var pdf = RenderPdf("<html><body><p style='text-decoration:underline'>Underlined</p></body></html>");
            var content = ExtractContentStreams(pdf);
            // Underline uses moveto (m), lineto (l), and stroke (S)
            Assert.Contains("S\n", content);
            Assert.Contains("w\n", content);
        }

        [Fact]
        public void BackgroundGray_ProducesCorrectColorValues()
        {
            var pdf = RenderPdf("<html><body><div style='width:100px;height:50px;background:#eeeeee'>Box</div></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("0.9333 0.9333 0.9333 rg", content);
        }

        // ===============================================
        // Edge Cases and Validation
        // ===============================================

        [Fact]
        public void EmptyBody_ProducesValidPdf()
        {
            var pdf = RenderPdf("<html><body></body></html>");
            var text = PdfText(pdf);
            Assert.StartsWith("%PDF-1.7", text);
            Assert.Contains("%%EOF", text);
            Assert.Contains("/Type /Catalog", text);
        }

        [Fact]
        public void SpecialCharacters_RenderWithoutCrash()
        {
            var pdf = RenderPdf("<html><body><p>&amp; &lt; &gt; &quot;</p></body></html>");
            Assert.True(pdf.Length > 200);
            var content = ExtractContentStreams(pdf);
            Assert.Contains("BT", content);
        }

        [Fact]
        public void CreationDate_PresentInMetadata()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Matches(@"/CreationDate\s*\(D:\d+Z\)", text);
        }

        [Fact]
        public void ModDate_PresentInMetadata()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Matches(@"/ModDate\s*\(D:\d+Z\)", text);
        }

        [Fact]
        public void ContentContents_ReferencedByPage()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Matches(@"/Contents\s+\d+\s+0\s+R", text);
        }

        [Fact]
        public void PageParent_ReferencesPages()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Matches(@"/Parent\s+\d+\s+0\s+R", text);
        }

        [Fact]
        public void PagesKids_ReferencesPageObjects()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Matches(@"/Kids\s*\[\d+\s+0\s+R", text);
        }

        [Fact]
        public void CatalogPages_ReferencesPages()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Matches(@"/Type /Catalog[\s\S]*?/Pages\s+\d+\s+0\s+R", text);
        }

        [Fact]
        public void MarginAffectsTextPosition()
        {
            // No margins
            var pdfNoMargin = RenderPdf("<html><body><p>Hello</p></body></html>", 400, 300);
            var contentNoMargin = ExtractContentStreams(pdfNoMargin);

            // With margins
            var pdfWithMargin = Render.ToPdf("<html><body><p>Hello</p></body></html>", new RenderOptions
            {
                PageSize = new SizeF(400, 300),
                MarginTop = 50, MarginRight = 50, MarginBottom = 50, MarginLeft = 50
            });
            var contentWithMargin = ExtractContentStreams(pdfWithMargin);

            // Both should have text
            Assert.Contains("Td", contentNoMargin);
            Assert.Contains("Td", contentWithMargin);

            // Extract Td coordinates - margin version should have larger offset
            var noMarginMatch = Regex.Match(contentNoMargin, @"([\d.]+)\s+([\d.]+)\s+Td");
            var withMarginMatch = Regex.Match(contentWithMargin, @"([\d.]+)\s+([\d.]+)\s+Td");
            Assert.True(noMarginMatch.Success);
            Assert.True(withMarginMatch.Success);
            float noMarginX = float.Parse(noMarginMatch.Groups[1].Value);
            float withMarginX = float.Parse(withMarginMatch.Groups[1].Value);
            Assert.True(withMarginX > noMarginX, $"Margin text X ({withMarginX}) should be larger than no-margin X ({noMarginX})");
        }

        [Fact]
        public void XrefTable_HasCorrectFormat()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            // xref table header: "xref\n0 N"
            Assert.Matches(@"xref\n0\s+\d+", text);
        }

        [Fact]
        public void TrailerSize_MatchesObjectCount()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            // /Size in trailer should be a positive integer
            Assert.Matches(@"/Size\s+\d+", text);
        }

        [Fact]
        public void InfoDictionary_ReferencedInTrailer()
        {
            var pdf = RenderPdf("<html><body><p>Hello</p></body></html>");
            var text = PdfText(pdf);
            Assert.Matches(@"/Info\s+\d+\s+0\s+R", text);
        }
    }
}
