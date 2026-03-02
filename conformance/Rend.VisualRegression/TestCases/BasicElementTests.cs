using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class BasicElementTests
    {
        static BasicElementTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "basic-paragraph",
                Name = "Paragraph",
                Category = "Basic Elements",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <p style=""color:#333; margin:0;"">This is a simple paragraph of text used to verify basic text rendering. It should wrap naturally within the given page width.</p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "basic-headings",
                Name = "Headings H1-H6",
                Category = "Basic Elements",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; line-height:1.2;"">
                    <h1 style=""font-size:24px; margin:4px 0;"">Heading 1</h1>
                    <h2 style=""font-size:20px; margin:4px 0;"">Heading 2</h2>
                    <h3 style=""font-size:18px; margin:4px 0;"">Heading 3</h3>
                    <h4 style=""font-size:16px; margin:4px 0;"">Heading 4</h4>
                    <h5 style=""font-size:14px; margin:4px 0;"">Heading 5</h5>
                    <h6 style=""font-size:12px; margin:4px 0;"">Heading 6</h6>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "basic-unordered-list",
                Name = "Unordered List",
                Category = "Basic Elements",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <ul style=""margin:0; padding-left:24px;"">
                        <li>First item</li>
                        <li>Second item</li>
                        <li>Third item</li>
                    </ul>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "basic-ordered-list",
                Name = "Ordered List",
                Category = "Basic Elements",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <ol style=""margin:0; padding-left:24px;"">
                        <li>First item</li>
                        <li>Second item</li>
                        <li>Third item</li>
                    </ol>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "basic-links",
                Name = "Links",
                Category = "Basic Elements",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <p style=""margin:0 0 8px 0;"">Visit <a href=""https://example.com"" style=""color:#3498db; text-decoration:underline;"">Example.com</a> for more info.</p>
                    <p style=""margin:0;""><a href=""#"" style=""color:#e74c3c; text-decoration:none; font-weight:bold;"">Styled link without underline</a></p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "basic-inline-elements",
                Name = "Inline Elements (strong, em, code)",
                Category = "Basic Elements",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <p style=""margin:0;"">This has <strong style=""font-weight:bold;"">bold text</strong> and <em style=""font-style:italic;"">italic text</em> and <code style=""font-family:monospace; background:#f0f0f0; padding:2px 4px;"">inline code</code>.</p>
                </body></html>",
            });
        }
    }
}
