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
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <p style=""color:#e74c3c; margin:0 0 4px 0;"">Red text (#e74c3c)</p>
                    <p style=""color:#27ae60; margin:0 0 4px 0;"">Green text (#27ae60)</p>
                    <p style=""color:#3498db; margin:0 0 4px 0;"">Blue text (#3498db)</p>
                    <p style=""color:#8e44ad; margin:0 0 4px 0;"">Purple text (#8e44ad)</p>
                    <p style=""color:rgb(230, 126, 34); margin:0;"">Orange text (rgb)</p>
                </body></html>",
            });

            // Use explicit heights for background color divs to avoid vertical shift from font metrics
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-background",
                Name = "Background Colors",
                Category = "Color & Background",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""background:#e74c3c; color:#fff; padding:10px; margin-bottom:6px; height:20px;"">Red background</div>
                    <div style=""background:#27ae60; color:#fff; padding:10px; margin-bottom:6px; height:20px;"">Green background</div>
                    <div style=""background:#3498db; color:#fff; padding:10px; margin-bottom:6px; height:20px;"">Blue background</div>
                    <div style=""background:#2c3e50; color:#ecf0f1; padding:10px; height:20px;"">Dark background</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-gradient",
                Name = "Linear Gradient",
                Category = "Color & Background",
                Tolerance = 0.0, // Gradient rendering may differ between engines
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""background:linear-gradient(to right, #3498db, #8e44ad); color:#fff; padding:20px; margin-bottom:10px; border-radius:4px; height:20px;"">Horizontal gradient</div>
                    <div style=""background:linear-gradient(to bottom, #e74c3c, #f39c12); color:#fff; padding:20px; border-radius:4px; height:20px;"">Vertical gradient</div>
                </body></html>",
            });

            // Opacity test with explicit heights to avoid vertical drift
            // High tolerance because opacity blending with body background differs due to font metrics
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-opacity",
                Name = "Opacity",
                Category = "Color & Background",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4; background:#ecf0f1;"">
                    <div style=""background:#e74c3c; color:#fff; padding:10px; margin-bottom:6px; opacity:1.0; height:20px;"">Opacity 1.0</div>
                    <div style=""background:#e74c3c; color:#fff; padding:10px; margin-bottom:6px; opacity:0.75; height:20px;"">Opacity 0.75</div>
                    <div style=""background:#e74c3c; color:#fff; padding:10px; margin-bottom:6px; opacity:0.5; height:20px;"">Opacity 0.5</div>
                    <div style=""background:#e74c3c; color:#fff; padding:10px; opacity:0.25; height:20px;"">Opacity 0.25</div>
                </body></html>",
            });
        }
    }
}
