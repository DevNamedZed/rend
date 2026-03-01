using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class ColorAndBackgroundTests
    {
        static ColorAndBackgroundTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-solid",
                Name = "Solid Colors",
                Category = "Color & Background",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px;"">
                    <p style=""color:#e74c3c; margin-bottom:4px;"">Red text (#e74c3c)</p>
                    <p style=""color:#27ae60; margin-bottom:4px;"">Green text (#27ae60)</p>
                    <p style=""color:#3498db; margin-bottom:4px;"">Blue text (#3498db)</p>
                    <p style=""color:#8e44ad; margin-bottom:4px;"">Purple text (#8e44ad)</p>
                    <p style=""color:rgb(230, 126, 34);"">Orange text (rgb)</p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-background",
                Name = "Background Colors",
                Category = "Color & Background",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px;"">
                    <div style=""background:#e74c3c; color:#fff; padding:10px; margin-bottom:6px;"">Red background</div>
                    <div style=""background:#27ae60; color:#fff; padding:10px; margin-bottom:6px;"">Green background</div>
                    <div style=""background:#3498db; color:#fff; padding:10px; margin-bottom:6px;"">Blue background</div>
                    <div style=""background:#2c3e50; color:#ecf0f1; padding:10px;"">Dark background</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-gradient",
                Name = "Linear Gradient",
                Category = "Color & Background",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px;"">
                    <div style=""background:linear-gradient(to right, #3498db, #8e44ad); color:#fff; padding:20px; margin-bottom:10px; border-radius:4px;"">Horizontal gradient</div>
                    <div style=""background:linear-gradient(to bottom, #e74c3c, #f39c12); color:#fff; padding:20px; border-radius:4px;"">Vertical gradient</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-opacity",
                Name = "Opacity",
                Category = "Color & Background",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#ecf0f1;"">
                    <div style=""background:#e74c3c; color:#fff; padding:10px; margin-bottom:6px; opacity:1.0;"">Opacity 1.0</div>
                    <div style=""background:#e74c3c; color:#fff; padding:10px; margin-bottom:6px; opacity:0.75;"">Opacity 0.75</div>
                    <div style=""background:#e74c3c; color:#fff; padding:10px; margin-bottom:6px; opacity:0.5;"">Opacity 0.5</div>
                    <div style=""background:#e74c3c; color:#fff; padding:10px; opacity:0.25;"">Opacity 0.25</div>
                </body></html>",
            });
        }
    }
}
