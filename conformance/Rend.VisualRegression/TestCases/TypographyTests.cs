using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class TypographyTests
    {
        static TypographyTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "typo-font-sizes",
                Name = "Font Sizes",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; line-height:1.2;"">
                    <p style=""font-size:10px; margin:0 0 4px 0;"">Font size 10px</p>
                    <p style=""font-size:14px; margin:0 0 4px 0;"">Font size 14px</p>
                    <p style=""font-size:18px; margin:0 0 4px 0;"">Font size 18px</p>
                    <p style=""font-size:24px; margin:0 0 4px 0;"">Font size 24px</p>
                    <p style=""font-size:32px; margin:0;"">Font size 32px</p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "typo-font-weight",
                Name = "Font Weight",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:16px; line-height:1.2;"">
                    <p style=""font-weight:100; margin:0 0 4px 0;"">Weight 100 (Thin)</p>
                    <p style=""font-weight:300; margin:0 0 4px 0;"">Weight 300 (Light)</p>
                    <p style=""font-weight:400; margin:0 0 4px 0;"">Weight 400 (Normal)</p>
                    <p style=""font-weight:600; margin:0 0 4px 0;"">Weight 600 (Semi-Bold)</p>
                    <p style=""font-weight:700; margin:0 0 4px 0;"">Weight 700 (Bold)</p>
                    <p style=""font-weight:900; margin:0;"">Weight 900 (Black)</p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "typo-text-alignment",
                Name = "Text Alignment",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <p style=""text-align:left; background:#f0f0f0; padding:6px; margin:0 0 6px 0; height:20px;"">Left aligned</p>
                    <p style=""text-align:center; background:#f0f0f0; padding:6px; margin:0 0 6px 0; height:20px;"">Center aligned</p>
                    <p style=""text-align:right; background:#f0f0f0; padding:6px; margin:0 0 6px 0; height:20px;"">Right aligned</p>
                    <p style=""text-align:justify; background:#f0f0f0; padding:6px; margin:0; height:40px;"">Justified text that should stretch to fill the entire width of its container element evenly.</p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "typo-text-decoration",
                Name = "Text Decoration",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:16px; line-height:1.4;"">
                    <p style=""text-decoration:underline; margin:0 0 6px 0;"">Underlined text</p>
                    <p style=""text-decoration:line-through; margin:0 0 6px 0;"">Strikethrough text</p>
                    <p style=""text-decoration:overline; margin:0 0 6px 0;"">Overlined text</p>
                    <p style=""text-decoration:none; margin:0;"">No decoration</p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "typo-line-height",
                Name = "Line Height",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px;"">
                    <p style=""line-height:1.0; background:#e8f4fd; padding:4px; margin:0 0 8px 0;"">Line height 1.0. This is a longer text that should show tight spacing between lines when it wraps.</p>
                    <p style=""line-height:1.5; background:#fde8e8; padding:4px; margin:0 0 8px 0;"">Line height 1.5. This is a longer text that should show normal spacing between lines when it wraps.</p>
                    <p style=""line-height:2.0; background:#e8fde8; padding:4px; margin:0;"">Line height 2.0. This is a longer text that should show generous spacing between lines when it wraps.</p>
                </body></html>",
            });
        }
    }
}
