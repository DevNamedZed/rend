using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class MultiColumnTests
    {
        static MultiColumnTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "multicol-basic",
                Name = "Basic Multi-Column",
                Category = "Multi-Column Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:12px; background:#fff;"">
                    <div style=""column-count:3; column-gap:16px; line-height:1.5;"">
                        <p style=""margin:0 0 8px;"">Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore.</p>
                        <p style=""margin:0 0 8px;"">Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip.</p>
                        <p style=""margin:0;"">Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat.</p>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "multicol-rule",
                Name = "Multi-Column with Rule",
                Category = "Multi-Column Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:12px; background:#fff;"">
                    <div style=""column-count:2; column-gap:20px; column-rule:1px solid #ccc; line-height:1.5;"">
                        <p style=""margin:0 0 8px;"">First column content with enough text to fill it properly and demonstrate the column rule.</p>
                        <p style=""margin:0;"">Second column content continues here with additional text to demonstrate balanced columns.</p>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "multicol-span",
                Name = "Column Span All",
                Category = "Multi-Column Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:12px; background:#fff;"">
                    <div style=""column-count:2; column-gap:16px; line-height:1.5;"">
                        <p style=""margin:0 0 8px;"">Content in columns above the spanning element.</p>
                        <h3 style=""column-span:all; margin:8px 0; font-size:16px; border-bottom:2px solid #3498db; padding-bottom:4px;"">Full Width Heading</h3>
                        <p style=""margin:0;"">Content after the spanning heading continues in columns.</p>
                    </div>
                </body></html>",
            });
        }
    }
}
