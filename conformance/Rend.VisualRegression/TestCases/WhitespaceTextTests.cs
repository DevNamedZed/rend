using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class WhitespaceTextTests
    {
        static WhitespaceTextTests()
        {
            // --- White-space ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ws-pre",
                Name = "White-space: pre",
                Category = "White-space",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:monospace; font-size:13px; background:#fff;"">
                    <pre style=""margin:0; background:#f5f5f5; padding:8px; border:1px solid #ddd;"">Line 1
  indented line
    double indent
Line 4</pre>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ws-nowrap",
                Name = "White-space: nowrap with overflow",
                Category = "White-space",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <div style=""width:200px; border:1px solid #ccc; padding:4px; white-space:nowrap; overflow:hidden;"">
                        This is a long text that should not wrap to the next line because white-space is set to nowrap
                    </div>
                    <div style=""margin-top:8px; width:200px; border:1px solid #ccc; padding:4px; white-space:normal;"">
                        This text should wrap normally within the container width
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ws-pre-wrap",
                Name = "White-space: pre-wrap",
                Category = "White-space",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:monospace; font-size:13px; background:#fff;"">
                    <div style=""width:250px; border:1px solid #ccc; padding:8px; white-space:pre-wrap; background:#f9f9f9;"">This text   has   extra   spaces   that   should   be   preserved   and   it   wraps.</div>
                </body></html>",
            });

            // --- Text-indent ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "text-indent",
                Name = "Text Indent",
                Category = "Text Properties",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <p style=""margin:0 0 8px; text-indent:30px; background:#f0f0f0; padding:4px;"">
                        This paragraph has a 30px text indent on the first line. The second line should not be indented.
                    </p>
                    <p style=""margin:0; text-indent:50px; background:#e0e0e0; padding:4px;"">
                        This has 50px indent applied.
                    </p>
                </body></html>",
            });

            // --- Letter-spacing ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "letter-spacing",
                Name = "Letter Spacing",
                Category = "Text Properties",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <p style=""margin:0 0 6px; letter-spacing:0;"">Normal letter spacing</p>
                    <p style=""margin:0 0 6px; letter-spacing:2px;"">2px letter spacing</p>
                    <p style=""margin:0 0 6px; letter-spacing:5px;"">5px letter spacing</p>
                    <p style=""margin:0; letter-spacing:-1px;"">-1px letter spacing (tighter)</p>
                </body></html>",
            });

            // --- Word-spacing ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "word-spacing",
                Name = "Word Spacing",
                Category = "Text Properties",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <p style=""margin:0 0 6px; word-spacing:0;"">Normal word spacing between words</p>
                    <p style=""margin:0 0 6px; word-spacing:5px;"">5px extra word spacing between words</p>
                    <p style=""margin:0; word-spacing:10px;"">10px extra word spacing between words</p>
                </body></html>",
            });

            // --- Vertical-align ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "vertical-align",
                Name = "Vertical Align in Inline Context",
                Category = "Text Properties",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <p style=""margin:0 0 8px; line-height:2;"">
                        Normal <span style=""vertical-align:super; font-size:10px;"">superscript</span>
                        and <span style=""vertical-align:sub; font-size:10px;"">subscript</span> text
                    </p>
                    <p style=""margin:0; font-size:20px; line-height:1.5;"">
                        Big <span style=""font-size:10px; vertical-align:middle;"">middle</span>
                        and <span style=""font-size:10px; vertical-align:top;"">top</span>
                        and <span style=""font-size:10px; vertical-align:bottom;"">bottom</span>
                    </p>
                </body></html>",
            });

            // --- Text-transform ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "text-transform",
                Name = "Text Transform",
                Category = "Text Properties",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <p style=""margin:0 0 6px; text-transform:uppercase;"">this text should be uppercase</p>
                    <p style=""margin:0 0 6px; text-transform:lowercase;"">THIS TEXT SHOULD BE LOWERCASE</p>
                    <p style=""margin:0; text-transform:capitalize;"">each word should be capitalized</p>
                </body></html>",
            });

            // --- CSS Variables ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "css-variables",
                Name = "CSS Custom Properties",
                Category = "CSS Variables",
                Html = @"<html><head><style>
                    :root { --main-color: #3498db; --spacing: 10px; --radius: 6px; }
                    .var-box { background: var(--main-color); padding: var(--spacing);
                               border-radius: var(--radius); color: white; margin-bottom: 8px; }
                </style></head>
                <body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <div class=""var-box"">Box using CSS variables</div>
                    <div class=""var-box"" style=""--main-color: #e74c3c;"">Override variable to red</div>
                </body></html>",
            });

            // --- Aspect-ratio ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "aspect-ratio",
                Name = "CSS Aspect Ratio",
                Category = "Sizing Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:160px; aspect-ratio:16/9; background:#3498db; margin-bottom:8px;""></div>
                    <div style=""width:100px; aspect-ratio:1/1; background:#e74c3c; margin-bottom:8px;""></div>
                    <div style=""width:200px; aspect-ratio:4/3; background:#27ae60;""></div>
                </body></html>",
            });

            // --- Box-shadow inset ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-shadow-inset",
                Name = "Inset Box Shadow",
                Category = "Box Shadow",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:150px; height:60px; background:#ecf0f1; box-shadow:inset 0 2px 8px rgba(0,0,0,0.3); margin-bottom:10px;""></div>
                    <div style=""width:150px; height:60px; background:#ecf0f1; box-shadow:inset 3px 3px 10px rgba(0,0,0,0.2), inset -3px -3px 10px rgba(255,255,255,0.5);""></div>
                </body></html>",
            });

            // --- Outline ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "outline-styles",
                Name = "CSS Outline Styles",
                Category = "Outline",
                Html = @"<html><body style=""margin:0; padding:15px; background:#fff;"">
                    <div style=""width:120px; height:40px; background:#f0f0f0; outline:2px solid #3498db; margin-bottom:15px;""></div>
                    <div style=""width:120px; height:40px; background:#f0f0f0; outline:3px dashed #e74c3c; margin-bottom:15px;""></div>
                    <div style=""width:120px; height:40px; background:#f0f0f0; outline:2px solid #27ae60; outline-offset:4px;""></div>
                </body></html>",
            });
        }
    }
}
