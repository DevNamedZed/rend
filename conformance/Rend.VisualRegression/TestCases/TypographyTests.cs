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
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif;"">
                    <p style=""font-size:10px; margin-bottom:4px;"">Font size 10px</p>
                    <p style=""font-size:14px; margin-bottom:4px;"">Font size 14px</p>
                    <p style=""font-size:18px; margin-bottom:4px;"">Font size 18px</p>
                    <p style=""font-size:24px; margin-bottom:4px;"">Font size 24px</p>
                    <p style=""font-size:32px;"">Font size 32px</p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "typo-font-weight",
                Name = "Font Weight",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:16px;"">
                    <p style=""font-weight:100; margin-bottom:4px;"">Weight 100 (Thin)</p>
                    <p style=""font-weight:300; margin-bottom:4px;"">Weight 300 (Light)</p>
                    <p style=""font-weight:400; margin-bottom:4px;"">Weight 400 (Normal)</p>
                    <p style=""font-weight:600; margin-bottom:4px;"">Weight 600 (Semi-Bold)</p>
                    <p style=""font-weight:700; margin-bottom:4px;"">Weight 700 (Bold)</p>
                    <p style=""font-weight:900;"">Weight 900 (Black)</p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "typo-text-alignment",
                Name = "Text Alignment",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px;"">
                    <p style=""text-align:left; background:#f0f0f0; padding:6px; margin-bottom:6px;"">Left aligned</p>
                    <p style=""text-align:center; background:#f0f0f0; padding:6px; margin-bottom:6px;"">Center aligned</p>
                    <p style=""text-align:right; background:#f0f0f0; padding:6px; margin-bottom:6px;"">Right aligned</p>
                    <p style=""text-align:justify; background:#f0f0f0; padding:6px;"">Justified text that should stretch to fill the entire width of its container element evenly.</p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "typo-text-decoration",
                Name = "Text Decoration",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:16px;"">
                    <p style=""text-decoration:underline; margin-bottom:6px;"">Underlined text</p>
                    <p style=""text-decoration:line-through; margin-bottom:6px;"">Strikethrough text</p>
                    <p style=""text-decoration:overline; margin-bottom:6px;"">Overlined text</p>
                    <p style=""text-decoration:none;"">No decoration</p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "typo-line-height",
                Name = "Line Height",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px;"">
                    <p style=""line-height:1.0; background:#e8f4fd; padding:4px; margin-bottom:8px;"">Line height 1.0. This is a longer text that should show tight spacing between lines when it wraps.</p>
                    <p style=""line-height:1.5; background:#fde8e8; padding:4px; margin-bottom:8px;"">Line height 1.5. This is a longer text that should show normal spacing between lines when it wraps.</p>
                    <p style=""line-height:2.0; background:#e8fde8; padding:4px;"">Line height 2.0. This is a longer text that should show generous spacing between lines when it wraps.</p>
                </body></html>",
            });
        }
    }
}
