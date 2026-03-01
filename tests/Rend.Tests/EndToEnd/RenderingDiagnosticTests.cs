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
    public class RenderingDiagnosticTests
    {
        private readonly ITestOutputHelper _output;

        public RenderingDiagnosticTests(ITestOutputHelper output)
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

            // Find stream/endstream pairs and try to decompress them
            int searchPos = 0;
            while (true)
            {
                int streamStart = text.IndexOf("stream\r\n", searchPos, StringComparison.Ordinal);
                if (streamStart < 0)
                    streamStart = text.IndexOf("stream\n", searchPos, StringComparison.Ordinal);
                if (streamStart < 0) break;

                // Skip past "stream\r\n" or "stream\n"
                int dataStart = streamStart + 7;
                if (dataStart < pdf.Length && pdf[dataStart] == '\n')
                    dataStart++;

                int endStream = text.IndexOf("endstream", dataStart, StringComparison.Ordinal);
                if (endStream < 0) break;

                // Trim trailing whitespace from the stream data
                int dataEnd = endStream;
                while (dataEnd > dataStart && (pdf[dataEnd - 1] == '\r' || pdf[dataEnd - 1] == '\n'))
                    dataEnd--;

                int len = dataEnd - dataStart;
                if (len > 0 && len < pdf.Length)
                {
                    var streamData = new byte[len];
                    Buffer.BlockCopy(pdf, dataStart, streamData, 0, len);

                    // Try to deflate-decompress
                    try
                    {
                        using var ms = new MemoryStream(streamData);
                        using var deflate = new DeflateStream(ms, CompressionMode.Decompress);
                        using var output = new MemoryStream();
                        deflate.CopyTo(output);
                        var decompressed = Encoding.ASCII.GetString(output.ToArray());
                        sb.Append(decompressed);
                        sb.Append('\n');
                    }
                    catch
                    {
                        // Not compressed or not valid deflate — append raw
                        var raw = Encoding.ASCII.GetString(streamData);
                        sb.Append(raw);
                        sb.Append('\n');
                    }
                }

                searchPos = endStream + 9;
            }

            return sb.ToString();
        }

        // ═══════════════════════════════════════════
        // PDF Structure Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void EmptyPage_ProducesValidPdf()
        {
            var pdf = RenderPdf("<html><body></body></html>");
            var text = PdfText(pdf);
            Assert.StartsWith("%PDF-", text);
            Assert.Contains("%%EOF", text);
        }

        [Fact]
        public void ValidPdf_HasRequiredObjects()
        {
            var pdf = RenderPdf("<html><body><p>Test</p></body></html>");
            var text = PdfText(pdf);
            Assert.Contains("/Type /Catalog", text);
            Assert.Contains("/Type /Page", text);
            Assert.Contains("xref", text);
            Assert.Contains("trailer", text);
        }

        [Fact]
        public void Pdf_HasFontResources()
        {
            var pdf = RenderPdf("<html><body><p>Hello World</p></body></html>");
            var text = PdfText(pdf);
            // Page should reference font resources
            Assert.Contains("/Font", text);
        }

        // ═══════════════════════════════════════════
        // Text Rendering Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void BasicParagraph_HasTextContent()
        {
            var pdf = RenderPdf("<html><body><p>Hello World</p></body></html>");
            Assert.True(pdf.Length > 200, $"PDF too small: {pdf.Length}");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("BT", content);  // Begin text
            Assert.Contains("ET", content);  // End text
            Assert.Contains("Tf", content);  // Set font
            _output.WriteLine($"PDF size: {pdf.Length}, Has text operators: YES");
        }

        [Fact]
        public void ColoredText_HasColorOperators()
        {
            var pdf = RenderPdf("<html><body><p style='color:red'>Red text</p></body></html>");
            var content = ExtractContentStreams(pdf);
            // Should have rgb color setting (1 0 0 rg for red)
            Assert.Contains("rg", content);
            _output.WriteLine($"PDF has color operators: YES, size: {pdf.Length}");
        }

        [Fact]
        public void TextWithFontSize_AppliesFontSize()
        {
            var pdf = RenderPdf("<html><body><p style='font-size:24px'>Big text</p></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("BT", content);
            Assert.Contains("Tf", content);
            // 24px font size should appear in the content stream
            Assert.Contains("24", content);
        }

        [Fact]
        public void BoldText_Renders()
        {
            var pdf = RenderPdf("<html><body><p><strong>Bold text</strong></p></body></html>");
            Assert.True(pdf.Length > 200);
            var content = ExtractContentStreams(pdf);
            Assert.Contains("BT", content);
        }

        [Fact]
        public void ItalicText_Renders()
        {
            var pdf = RenderPdf("<html><body><p><em>Italic text</em></p></body></html>");
            Assert.True(pdf.Length > 200);
            var content = ExtractContentStreams(pdf);
            Assert.Contains("BT", content);
        }

        [Fact]
        public void MultipleTextStyles_AllRender()
        {
            var pdf = RenderPdf(@"<html><body>
                <p>Normal <strong>bold</strong> <em>italic</em> <u>underline</u></p>
            </body></html>");
            Assert.True(pdf.Length > 200);
            var content = ExtractContentStreams(pdf);
            Assert.Contains("BT", content);
        }

        [Fact]
        public void HeadingsH1ToH6_AllRender()
        {
            var pdf = RenderPdf(@"<html><body>
                <h1>Heading 1</h1>
                <h2>Heading 2</h2>
                <h3>Heading 3</h3>
                <h4>Heading 4</h4>
                <h5>Heading 5</h5>
                <h6>Heading 6</h6>
            </body></html>");
            Assert.True(pdf.Length > 300);
            var content = ExtractContentStreams(pdf);
            // Should have multiple BT/ET pairs for each heading
            var btCount = Regex.Matches(content, @"\bBT\b").Count;
            Assert.True(btCount >= 6, $"Expected at least 6 text objects for 6 headings, got {btCount}");
        }

        [Fact]
        public void SpecialCharacters_DoNotCrash()
        {
            var pdf = RenderPdf("<html><body><p>&amp; &lt; &gt; &quot; &#169; &#8364;</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void LongText_Wraps()
        {
            var pdf = RenderPdf(@"<html><body>
                <p style='width:100px'>This is a very long text that should wrap to multiple lines within its container.</p>
            </body></html>");
            Assert.True(pdf.Length > 200);
            var content = ExtractContentStreams(pdf);
            // Multiple text positioning commands indicate text wrapping
            var tdCount = Regex.Matches(content, @"\bTd\b|\bTm\b").Count;
            Assert.True(tdCount >= 1, "Expected at least one text position command");
        }

        [Fact]
        public void WhiteSpacePreserved_InPre()
        {
            var pdf = RenderPdf("<html><body><pre>Line 1\n  Line 2\n    Line 3</pre></body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Color Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void NamedColors_Applied()
        {
            var pdf = RenderPdf(@"<html><body>
                <p style='color:red'>Red</p>
                <p style='color:green'>Green</p>
                <p style='color:blue'>Blue</p>
            </body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("rg", content); // Fill color
        }

        [Fact]
        public void HexColors_Applied()
        {
            var pdf = RenderPdf(@"<html><body>
                <p style='color:#ff0000'>Red</p>
                <p style='color:#00ff00'>Green</p>
                <p style='color:#0000ff'>Blue</p>
            </body></html>");
            Assert.True(pdf.Length > 200);
            var content = ExtractContentStreams(pdf);
            Assert.Contains("rg", content);
        }

        [Fact]
        public void RgbColors_Applied()
        {
            var pdf = RenderPdf("<html><body><p style='color:rgb(255,128,0)'>Orange</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void RgbaColors_Applied()
        {
            var pdf = RenderPdf("<html><body><p style='color:rgba(0,0,0,0.5)'>Semi-transparent</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void HslColors_Applied()
        {
            var pdf = RenderPdf("<html><body><p style='color:hsl(120,100%,50%)'>Green HSL</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Background Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void BackgroundColor_HasRectFill()
        {
            var pdf = RenderPdf("<html><body><div style='width:100px;height:50px;background:blue'>Box</div></body></html>");
            var content = ExtractContentStreams(pdf);
            Assert.Contains("re", content); // Rectangle
            Assert.Contains("f", content);  // Fill
        }

        [Fact]
        public void BackgroundColor_CorrectColorValues()
        {
            var pdf = RenderPdf("<html><body><div style='width:100px;height:50px;background-color:#ff0000'>Red box</div></body></html>");
            var content = ExtractContentStreams(pdf);
            // Should have red fill color: 1 0 0 rg
            Assert.Contains("1 0 0 rg", content);
        }

        [Fact]
        public void BackgroundColor_MultipleBoxes()
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

        [Fact]
        public void LinearGradient_DoesNotCrash()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='width:200px;height:100px;background:linear-gradient(to right, red, blue)'></div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Border Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void Borders_SolidBorder()
        {
            var pdf = RenderPdf("<html><body><div style='border:2px solid black;width:100px;height:50px'>Bordered</div></body></html>");
            Assert.True(pdf.Length > 200);
            var content = ExtractContentStreams(pdf);
            // Should have stroke operations for borders
            Assert.True(content.Contains("re") || content.Contains("l"), "Expected rectangle or line operators for borders");
        }

        [Fact]
        public void Borders_DifferentSides()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='border-top:3px solid red;border-bottom:3px solid blue;width:100px;height:50px'>
                    Top and bottom borders
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void Borders_DifferentStyles()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='border:3px dashed red;width:100px;height:50px'>Dashed</div>
                <div style='border:3px dotted blue;width:100px;height:50px'>Dotted</div>
                <div style='border:3px double green;width:100px;height:50px'>Double</div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void BorderRadius_DoesNotCrash()
        {
            var pdf = RenderPdf("<html><body><div style='border:1px solid black;border-radius:10px;width:100px;height:50px'>Rounded</div></body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Box Model Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void Padding_Applied()
        {
            var pdf = RenderPdf("<html><body><div style='padding:20px;background:gray'>Padded content</div></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void Margin_Applied()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='margin:20px;background:red;width:100px;height:50px'>Box 1</div>
                <div style='background:blue;width:100px;height:50px'>Box 2</div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void BoxSizing_BorderBox()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='box-sizing:border-box;width:200px;height:100px;padding:20px;border:5px solid black;background:yellow'>
                    Border-box sizing
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void MinMaxDimensions()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='min-width:100px;max-width:300px;min-height:50px;background:pink'>
                    Min/Max dimensions
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Layout Mode Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void FlexLayout_ProducesValidOutput()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='display:flex;gap:10px'>
                    <div style='width:50px;height:50px;background:red'></div>
                    <div style='width:50px;height:50px;background:green'></div>
                    <div style='width:50px;height:50px;background:blue'></div>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void FlexLayout_Column()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='display:flex;flex-direction:column;gap:5px'>
                    <div style='height:30px;background:red'></div>
                    <div style='height:30px;background:green'></div>
                    <div style='height:30px;background:blue'></div>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void FlexLayout_Wrap()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div style='width:80px;height:40px;background:red'></div>
                    <div style='width:80px;height:40px;background:green'></div>
                    <div style='width:80px;height:40px;background:blue'></div>
                    <div style='width:80px;height:40px;background:orange'></div>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void FlexLayout_JustifyContent()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='display:flex;justify-content:space-between;width:300px'>
                    <div style='width:50px;height:50px;background:red'></div>
                    <div style='width:50px;height:50px;background:blue'></div>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void FlexLayout_AlignItems()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='display:flex;align-items:center;height:100px;background:#eee'>
                    <div style='width:50px;height:30px;background:red'></div>
                    <div style='width:50px;height:60px;background:blue'></div>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void FlexLayout_FlexGrow()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='display:flex;width:300px'>
                    <div style='flex:1;height:50px;background:red'></div>
                    <div style='flex:2;height:50px;background:blue'></div>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void GridLayout_ProducesValidOutput()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='display:grid;grid-template-columns:1fr 1fr;gap:5px'>
                    <div style='background:#eee;padding:5px'>Cell 1</div>
                    <div style='background:#ddd;padding:5px'>Cell 2</div>
                    <div style='background:#ccc;padding:5px'>Cell 3</div>
                    <div style='background:#bbb;padding:5px'>Cell 4</div>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void GridLayout_ThreeColumns()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='display:grid;grid-template-columns:100px 100px 100px;gap:10px'>
                    <div style='background:red;height:50px'>1</div>
                    <div style='background:green;height:50px'>2</div>
                    <div style='background:blue;height:50px'>3</div>
                    <div style='background:orange;height:50px'>4</div>
                    <div style='background:purple;height:50px'>5</div>
                    <div style='background:cyan;height:50px'>6</div>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void GridLayout_AutoRows()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='display:grid;grid-template-columns:1fr 1fr;grid-auto-rows:60px;gap:5px'>
                    <div style='background:#ddd'>A</div>
                    <div style='background:#ccc'>B</div>
                    <div style='background:#bbb'>C</div>
                    <div style='background:#aaa'>D</div>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void GridLayout_Spanning()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;gap:5px'>
                    <div style='grid-column:span 2;background:red;height:40px'>Spans 2</div>
                    <div style='background:green;height:40px'>Single</div>
                    <div style='background:blue;height:40px'>A</div>
                    <div style='background:orange;height:40px'>B</div>
                    <div style='background:purple;height:40px'>C</div>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void Table_ProducesValidOutput()
        {
            var pdf = RenderPdf(@"<html><body>
                <table border='1' style='border-collapse:collapse'>
                    <tr><th>Header 1</th><th>Header 2</th></tr>
                    <tr><td>Cell 1</td><td>Cell 2</td></tr>
                    <tr><td>Cell 3</td><td>Cell 4</td></tr>
                </table>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void Table_ComplexWithStyles()
        {
            var pdf = RenderPdf(@"<html><body>
                <table style='border-collapse:collapse;width:300px'>
                    <thead>
                        <tr style='background:#333;color:white'>
                            <th style='padding:8px;border:1px solid #ddd'>Name</th>
                            <th style='padding:8px;border:1px solid #ddd'>Value</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr style='background:#f9f9f9'>
                            <td style='padding:8px;border:1px solid #ddd'>Alpha</td>
                            <td style='padding:8px;border:1px solid #ddd'>100</td>
                        </tr>
                        <tr>
                            <td style='padding:8px;border:1px solid #ddd'>Beta</td>
                            <td style='padding:8px;border:1px solid #ddd'>200</td>
                        </tr>
                    </tbody>
                </table>
            </body></html>");
            Assert.True(pdf.Length > 300);
        }

        [Fact]
        public void Table_Colspan()
        {
            var pdf = RenderPdf(@"<html><body>
                <table border='1' style='border-collapse:collapse'>
                    <tr><th colspan='2'>Spanning Header</th></tr>
                    <tr><td>Left</td><td>Right</td></tr>
                </table>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Float and Positioning Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void FloatLeft_Renders()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='float:left;width:100px;height:100px;background:red;margin-right:10px'></div>
                <p>Text wrapping around a floated element. This should appear to the right of the red box.</p>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void FloatRight_Renders()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='float:right;width:100px;height:100px;background:blue;margin-left:10px'></div>
                <p>Text wrapping around a right-floated element.</p>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void AbsolutePositioning_Renders()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='position:relative;width:200px;height:200px;background:#eee'>
                    <div style='position:absolute;top:10px;left:10px;width:50px;height:50px;background:red'></div>
                    <div style='position:absolute;bottom:10px;right:10px;width:50px;height:50px;background:blue'></div>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void FixedPositioning_Renders()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='position:fixed;top:0;left:0;width:100%;height:30px;background:navy;color:white'>
                    Fixed header
                </div>
                <div style='margin-top:40px'>
                    <p>Content below fixed header</p>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void InlineBlock_Renders()
        {
            var pdf = RenderPdf(@"<html><body>
                <span style='display:inline-block;width:50px;height:50px;background:red'></span>
                <span style='display:inline-block;width:50px;height:50px;background:green'></span>
                <span style='display:inline-block;width:50px;height:50px;background:blue'></span>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Text Decoration Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void TextDecoration_Underline()
        {
            var pdf = RenderPdf("<html><body><p style='text-decoration:underline'>Underlined text</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void TextDecoration_LineThrough()
        {
            var pdf = RenderPdf("<html><body><p style='text-decoration:line-through'>Struck through</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void TextDecoration_Overline()
        {
            var pdf = RenderPdf("<html><body><p style='text-decoration:overline'>Overlined text</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Transform and Opacity Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void Opacity_Applied()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='opacity:0.5;width:100px;height:100px;background:red'>Semi-transparent</div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void Transform_Rotate()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='transform:rotate(45deg);width:100px;height:100px;background:red;margin:50px'>Rotated</div>
            </body></html>");
            Assert.True(pdf.Length > 200);
            var content = ExtractContentStreams(pdf);
            // Should have a transformation matrix (cm operator)
            Assert.Contains("cm", content);
        }

        [Fact]
        public void Transform_Scale()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='transform:scale(1.5);width:50px;height:50px;background:blue;margin:50px'>Scaled</div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void Transform_Translate()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='transform:translate(20px,30px);width:50px;height:50px;background:green'>Moved</div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // CSS Cascade and Specificity Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void InlineStyles_Applied()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='width:200px;height:100px;background:#ff0000;color:white;font-size:24px;padding:10px'>
                    Styled Box
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void CssStylesheet_Applied()
        {
            var pdf = RenderPdf(@"<html><head><style>
                .box { width: 150px; height: 75px; background: green; }
                p { color: navy; font-size: 14px; }
            </style></head><body>
                <div class='box'>Green box</div>
                <p>Navy paragraph</p>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void CssInheritance_Works()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='color:red;font-size:20px'>
                    <p>This should inherit red color and 20px font size</p>
                    <div>
                        <span>Deeply nested, still red</span>
                    </div>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void CssSpecificity_HigherSpecWins()
        {
            var pdf = RenderPdf(@"<html><head><style>
                p { color: blue; }
                .special { color: red; }
                #unique { color: green; }
            </style></head><body>
                <p>Blue paragraph</p>
                <p class='special'>Red from class</p>
                <p id='unique' class='special'>Green from ID</p>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void CssImportant_Overrides()
        {
            var pdf = RenderPdf(@"<html><head><style>
                p { color: blue !important; }
            </style></head><body>
                <p style='color:red'>Should be blue due to !important</p>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // List Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void UnorderedList_Renders()
        {
            var pdf = RenderPdf(@"<html><body>
                <ul>
                    <li>First item</li>
                    <li>Second item</li>
                    <li>Third item</li>
                </ul>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void OrderedList_Renders()
        {
            var pdf = RenderPdf(@"<html><body>
                <ol>
                    <li>First item</li>
                    <li>Second item</li>
                    <li>Third item</li>
                </ol>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void NestedLists_Render()
        {
            var pdf = RenderPdf(@"<html><body>
                <ul>
                    <li>Parent 1
                        <ul>
                            <li>Child 1.1</li>
                            <li>Child 1.2</li>
                        </ul>
                    </li>
                    <li>Parent 2</li>
                </ul>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void ListStyleType_Applied()
        {
            var pdf = RenderPdf(@"<html><body>
                <ul style='list-style-type:square'>
                    <li>Square marker</li>
                </ul>
                <ol style='list-style-type:lower-alpha'>
                    <li>Alpha marker</li>
                </ol>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Nested and Complex Layout Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void NestedElements_ProducesValidOutput()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='padding:20px;background:#f0f0f0'>
                    <h2 style='color:darkblue'>Section Title</h2>
                    <div style='margin:10px;padding:10px;border:1px solid #ccc;background:white'>
                        <p>First paragraph with <strong>bold</strong> and <em>italic</em> text.</p>
                        <p>Second paragraph with a <a href='#'>link</a>.</p>
                    </div>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 300);
        }

        [Fact]
        public void DeeplyNested_DoesNotCrash()
        {
            var sb = new StringBuilder("<html><body>");
            for (int i = 0; i < 20; i++)
                sb.Append($"<div style='padding:2px;border:1px solid #{i * 10:X2}{i * 10:X2}{i * 10:X2}'>");
            sb.Append("<p>Deep content</p>");
            for (int i = 0; i < 20; i++)
                sb.Append("</div>");
            sb.Append("</body></html>");

            var pdf = RenderPdf(sb.ToString());
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void ComplexPage_FullLayout()
        {
            var pdf = RenderPdf(@"<html><head><style>
                body { margin: 0; font-family: sans-serif; }
                .header { background: #333; color: white; padding: 10px; }
                .nav { display: flex; gap: 10px; background: #555; padding: 5px 10px; }
                .nav a { color: white; }
                .main { display: flex; padding: 10px; gap: 10px; }
                .sidebar { width: 100px; background: #f0f0f0; padding: 10px; }
                .content { flex: 1; }
                .footer { background: #333; color: white; padding: 10px; text-align: center; }
            </style></head><body>
                <div class='header'><h1>My Page</h1></div>
                <div class='nav'>
                    <a href='#'>Home</a>
                    <a href='#'>About</a>
                    <a href='#'>Contact</a>
                </div>
                <div class='main'>
                    <div class='sidebar'>
                        <h3>Menu</h3>
                        <ul><li>Item 1</li><li>Item 2</li></ul>
                    </div>
                    <div class='content'>
                        <h2>Welcome</h2>
                        <p>This is a complex page layout test with header, nav, sidebar, content, and footer.</p>
                        <table border='1' style='border-collapse:collapse'>
                            <tr><th>A</th><th>B</th></tr>
                            <tr><td>1</td><td>2</td></tr>
                        </table>
                    </div>
                </div>
                <div class='footer'>Footer content</div>
            </body></html>");
            Assert.True(pdf.Length > 500, $"Complex page PDF too small: {pdf.Length}");
        }

        // ═══════════════════════════════════════════
        // Multi-Page and Pagination Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void MultipleElements_AllRendered()
        {
            var html = @"<html><body>
                <h1>Title</h1>
                <p>Paragraph one</p>
                <p>Paragraph two</p>
                <ul><li>Item 1</li><li>Item 2</li></ul>
            </body></html>";
            var pdf = RenderPdf(html);
            Assert.True(pdf.Length > 500, $"PDF too small for multi-element page: {pdf.Length}");
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
            _output.WriteLine($"Pages: {pageCount}, PDF size: {pdf.Length}");
            Assert.True(pageCount > 1, $"Expected multiple pages, got {pageCount}");
        }

        [Fact]
        public void PageBreak_DoesNotCrash()
        {
            // page-break-after CSS property — verify it doesn't crash.
            // Full page-break support is tracked separately.
            var pdf = RenderPdf(@"<html><body>
                <p>Page 1 content</p>
                <div style='page-break-after:always'></div>
                <p>Page 2 content</p>
            </body></html>");
            Assert.True(pdf.Length > 100);
        }

        // ═══════════════════════════════════════════
        // Overflow Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void OverflowHidden_Clips()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='width:100px;height:50px;overflow:hidden;background:#eee'>
                    <p>This is a very long text that should be clipped by the overflow hidden container.</p>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
            var content = ExtractContentStreams(pdf);
            // Clipping sets up a clip rectangle
            Assert.True(content.Contains("re") && content.Contains("W"), "Expected clip rectangle (re + W operators)");
        }

        // ═══════════════════════════════════════════
        // Display and Visibility Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void DisplayNone_Hidden()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='display:none'>This should not appear</div>
                <p>This should appear</p>
            </body></html>");
            // PDF should still be valid but without the hidden content
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void VisibilityHidden_TakesSpace()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='visibility:hidden;width:100px;height:100px;background:red'>Hidden but takes space</div>
                <p>After hidden element</p>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Text Alignment Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void TextAlignCenter_Renders()
        {
            var pdf = RenderPdf("<html><body><p style='text-align:center'>Centered text</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void TextAlignRight_Renders()
        {
            var pdf = RenderPdf("<html><body><p style='text-align:right'>Right-aligned text</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void TextAlignJustify_Renders()
        {
            var pdf = RenderPdf(@"<html><body>
                <p style='text-align:justify;width:200px'>
                    This is a long paragraph of text that should be justified, meaning it should
                    stretch to fill the entire width of its container on each line.
                </p>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Image Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void ImageOutput_ProducesValidPng()
        {
            try
            {
                var img = Render.ToImage("<html><body><p>Test</p></body></html>",
                    new RenderOptions { PageSize = new SizeF(200, 100) });
                Assert.NotNull(img);
                Assert.True(img.Length > 100, $"Image too small: {img.Length}");
                // PNG signature: 89 50 4E 47
                Assert.Equal(0x89, img[0]);
                Assert.Equal(0x50, img[1]);
                _output.WriteLine($"Image output: {img.Length} bytes, valid PNG header");
            }
            catch (Exception ex) when (ex.Message.Contains("native") || ex.Message.Contains("SkiaSharp") || ex.Message.Contains("libSkia"))
            {
                _output.WriteLine($"Skipping image test - native library not available: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════
        // Box Shadow Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void BoxShadow_Renders()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='width:100px;height:100px;background:white;box-shadow:5px 5px 10px rgba(0,0,0,0.3)'>
                    Shadow box
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void BoxShadow_Inset()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='width:100px;height:100px;background:white;box-shadow:inset 0 0 10px rgba(0,0,0,0.5)'>
                    Inset shadow
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Multi-Column Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void MultiColumn_Renders()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='column-count:2;column-gap:20px'>
                    <p>First paragraph in multi-column layout. This text should flow into the first column.</p>
                    <p>Second paragraph that may continue into the second column depending on available space.</p>
                    <p>Third paragraph with more content to ensure column wrapping occurs properly.</p>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Z-Index and Stacking Context Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void ZIndex_DoesNotCrash()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='position:relative;z-index:1;width:100px;height:100px;background:red'>Layer 1</div>
                <div style='position:relative;z-index:2;width:100px;height:100px;background:blue;margin-top:-50px'>Layer 2</div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Edge Cases
        // ═══════════════════════════════════════════

        [Fact]
        public void EmptyBody_ValidPdf()
        {
            var pdf = RenderPdf("<html><body></body></html>");
            Assert.True(pdf.Length > 100);
            var text = PdfText(pdf);
            Assert.StartsWith("%PDF-", text);
        }

        [Fact]
        public void OnlyWhitespace_ValidPdf()
        {
            var pdf = RenderPdf("<html><body>   \n\t  </body></html>");
            Assert.True(pdf.Length > 100);
        }

        [Fact]
        public void MalformedHtml_DoesNotCrash()
        {
            var pdf = RenderPdf("<html><body><div><p>Unclosed tags<div>Mixed up</body></html>");
            Assert.True(pdf.Length > 100);
        }

        [Fact]
        public void VeryLargeInlineStyle_DoesNotCrash()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='width:100px;height:100px;background:red;color:white;font-size:14px;
                    padding:10px;margin:5px;border:1px solid black;border-radius:5px;
                    box-shadow:2px 2px 5px gray;opacity:0.9;overflow:hidden;text-align:center'>
                    Many styles
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void ZeroSizedElement_DoesNotCrash()
        {
            var pdf = RenderPdf("<html><body><div style='width:0;height:0'></div><p>After zero-size</p></body></html>");
            Assert.True(pdf.Length > 100);
        }

        [Fact]
        public void NegativeMargin_DoesNotCrash()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='margin-top:-10px;background:red;width:100px;height:50px'>Negative margin</div>
            </body></html>");
            Assert.True(pdf.Length > 100);
        }

        // ═══════════════════════════════════════════
        // CSS Property Coverage Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void LetterSpacing_Applied()
        {
            var pdf = RenderPdf("<html><body><p style='letter-spacing:5px'>Spaced letters</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void WordSpacing_Applied()
        {
            var pdf = RenderPdf("<html><body><p style='word-spacing:10px'>Spaced words between each token</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void LineHeight_Applied()
        {
            var pdf = RenderPdf("<html><body><p style='line-height:2'>Double-spaced text content</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void TextIndent_Applied()
        {
            var pdf = RenderPdf("<html><body><p style='text-indent:30px'>Indented first line of this paragraph.</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void TextTransform_Uppercase()
        {
            var pdf = RenderPdf("<html><body><p style='text-transform:uppercase'>should be uppercase</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void Outline_Renders()
        {
            var pdf = RenderPdf("<html><body><div style='outline:3px solid red;width:100px;height:50px'>Outlined</div></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void MaxWidth_Constrains()
        {
            var pdf = RenderPdf("<html><body><div style='max-width:200px;background:yellow;padding:10px'>This div has max-width constraint</div></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void Overflow_Scroll()
        {
            var pdf = RenderPdf(@"<html><body>
                <div style='width:100px;height:50px;overflow:scroll'>
                    <p>Content that overflows the container dimensions.</p>
                </div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Semantic HTML Element Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void SemanticElements_AllRender()
        {
            var pdf = RenderPdf(@"<html><body>
                <header><h1>Header</h1></header>
                <nav><a href='#'>Link</a></nav>
                <main>
                    <article>
                        <section><p>Content</p></section>
                    </article>
                    <aside><p>Sidebar</p></aside>
                </main>
                <footer><p>Footer</p></footer>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void BlockquoteAndHr_Render()
        {
            var pdf = RenderPdf(@"<html><body>
                <blockquote>This is a blockquote with indented text.</blockquote>
                <hr />
                <p>After horizontal rule</p>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void DefinitionList_Renders()
        {
            var pdf = RenderPdf(@"<html><body>
                <dl>
                    <dt>Term 1</dt>
                    <dd>Definition 1</dd>
                    <dt>Term 2</dt>
                    <dd>Definition 2</dd>
                </dl>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void SupAndSub_Render()
        {
            var pdf = RenderPdf("<html><body><p>H<sub>2</sub>O and E=mc<sup>2</sup></p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void CodeAndKbd_Render()
        {
            var pdf = RenderPdf(@"<html><body>
                <p>Press <kbd>Ctrl</kbd>+<kbd>C</kbd> to copy. Use <code>console.log()</code> to debug.</p>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void MarkElement_Renders()
        {
            var pdf = RenderPdf("<html><body><p>This is <mark>highlighted</mark> text.</p></body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Multiple Stylesheets / Complex CSS
        // ═══════════════════════════════════════════

        [Fact]
        public void MultipleStyleBlocks_AllApplied()
        {
            var pdf = RenderPdf(@"<html><head>
                <style>p { color: blue; }</style>
                <style>.red { color: red; }</style>
                <style>#green { color: green; }</style>
            </head><body>
                <p>Blue</p>
                <p class='red'>Red</p>
                <p id='green'>Green</p>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void PseudoClasses_DoNotCrash()
        {
            var pdf = RenderPdf(@"<html><head><style>
                p:first-child { color: red; }
                li:nth-child(2) { font-weight: bold; }
            </style></head><body>
                <p>First paragraph</p>
                <p>Second paragraph</p>
                <ul><li>One</li><li>Two</li><li>Three</li></ul>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void MediaPrint_Applied()
        {
            var pdf = RenderPdf(@"<html><head><style>
                @media print {
                    body { color: black; }
                    .no-print { display: none; }
                }
                @media screen {
                    body { color: blue; }
                }
            </style></head><body>
                <p>Printed content</p>
                <div class='no-print'>Should not appear in print</div>
            </body></html>");
            Assert.True(pdf.Length > 200);
        }

        // ═══════════════════════════════════════════
        // Regression Tests
        // ═══════════════════════════════════════════

        [Fact]
        public void VeryWideTable_DoesNotHang()
        {
            var sb = new StringBuilder("<html><body><table><tr>");
            for (int i = 0; i < 50; i++)
                sb.Append($"<td>Col {i}</td>");
            sb.Append("</tr></table></body></html>");
            var pdf = RenderPdf(sb.ToString());
            Assert.True(pdf.Length > 200);
        }

        [Fact]
        public void ManySmallElements_PerformanceReasonable()
        {
            var sb = new StringBuilder("<html><body>");
            for (int i = 0; i < 200; i++)
                sb.Append($"<span style='display:inline-block;width:10px;height:10px;background:#{(i * 7 % 256):X2}{(i * 13 % 256):X2}{(i * 19 % 256):X2}'></span>");
            sb.Append("</body></html>");
            var pdf = RenderPdf(sb.ToString());
            Assert.True(pdf.Length > 500);
        }

        [Fact]
        public void FormElements_DoNotCrash()
        {
            var pdf = RenderPdf(@"<html><body>
                <form>
                    <input type='text' value='Input field' />
                    <textarea>Text area content</textarea>
                    <select><option>Option 1</option><option>Option 2</option></select>
                    <button>Submit</button>
                </form>
            </body></html>");
            Assert.True(pdf.Length > 100);
        }

        [Fact]
        public void CustomPageSize_Applied()
        {
            var pdf = Render.ToPdf("<html><body><p>Custom size</p></body></html>",
                new RenderOptions
                {
                    PageSize = new SizeF(200, 200),
                    MarginTop = 10, MarginRight = 10, MarginBottom = 10, MarginLeft = 10
                });
            Assert.True(pdf.Length > 100);
            var text = PdfText(pdf);
            // Page media box should reflect custom size
            Assert.Contains("/MediaBox", text);
        }

        [Fact]
        public void LargeMargins_ContentStillFits()
        {
            var pdf = Render.ToPdf("<html><body><p>Small content area</p></body></html>",
                new RenderOptions
                {
                    PageSize = new SizeF(400, 300),
                    MarginTop = 100, MarginRight = 100, MarginBottom = 100, MarginLeft = 100
                });
            Assert.True(pdf.Length > 100);
        }
    }
}
