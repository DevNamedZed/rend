using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class SemanticElementTests
    {
        static SemanticElementTests()
        {
            // --- Fieldset / Legend ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "fieldset-legend",
                Name = "Fieldset with Legend",
                Category = "Semantic Elements",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <fieldset style=""border:2px groove #ccc; padding:10px; margin:0;"">
                        <legend style=""padding:0 4px;"">Personal Info</legend>
                        <div style=""margin-bottom:6px;"">Name: John Doe</div>
                        <div>Email: john@example.com</div>
                    </fieldset>
                </body></html>",
            });

            // --- Details / Summary ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "details-open",
                Name = "Details Element (Open)",
                Category = "Semantic Elements",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <details open style=""margin-bottom:10px;"">
                        <summary style=""font-weight:bold; cursor:pointer;"">Click to expand</summary>
                        <p style=""margin:6px 0 0; padding-left:10px; color:#555;"">This content is visible because the details element is open.</p>
                    </details>
                    <details>
                        <summary style=""font-weight:bold;"">Closed section</summary>
                        <p>This should not be visible.</p>
                    </details>
                </body></html>",
            });

            // --- HR ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "hr-styling",
                Name = "Horizontal Rule",
                Category = "Semantic Elements",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <p style=""margin:0 0 4px;"">Content above</p>
                    <hr>
                    <p style=""margin:4px 0;"">Between rules</p>
                    <hr style=""border:none; border-top:2px solid #e74c3c;"">
                    <p style=""margin:4px 0 0;"">Content below</p>
                </body></html>",
            });

            // --- Blockquote ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "blockquote",
                Name = "Blockquote Element",
                Category = "Semantic Elements",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <blockquote style=""border-left:4px solid #3498db; margin:0 0 10px; padding:8px 16px; background:#f8f9fa;"">
                        <p style=""margin:0; color:#555; font-style:italic;"">The only way to do great work is to love what you do.</p>
                    </blockquote>
                    <p style=""margin:0;"">— Steve Jobs</p>
                </body></html>",
            });

            // --- Code / Pre ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "code-pre-block",
                Name = "Code and Pre-formatted Text",
                Category = "Semantic Elements",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <p style=""margin:0 0 8px;"">Use <code style=""background:#f0f0f0; padding:2px 4px; border-radius:3px; font-family:monospace; font-size:13px;"">console.log()</code> to debug.</p>
                    <pre style=""background:#2d2d2d; color:#f8f8f2; padding:12px; border-radius:4px; margin:0; font-family:monospace; font-size:12px; overflow:hidden;"">function hello() {
    return &quot;world&quot;;
}</pre>
                </body></html>",
            });

            // --- Definition List ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "definition-list",
                Name = "Definition List (dl/dt/dd)",
                Category = "Semantic Elements",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <dl style=""margin:0;"">
                        <dt style=""font-weight:bold; margin-bottom:2px;"">HTML</dt>
                        <dd style=""margin:0 0 8px 20px; color:#555;"">HyperText Markup Language</dd>
                        <dt style=""font-weight:bold; margin-bottom:2px;"">CSS</dt>
                        <dd style=""margin:0 0 0 20px; color:#555;"">Cascading Style Sheets</dd>
                    </dl>
                </body></html>",
            });
        }
    }
}
